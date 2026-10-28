// NIGHT27 roof27 (builder V; design.n27.roof27): the indoor look of a CAMPAIGN level. View only: no sim change, no collider (the player already cannot leave).
//  - a ceiling at (the tallest box of the plan + ceilingAbove m), dusk-dark violet, over every room and every link between two doors; a boss room keeps a
//    skylight hole of skylightShare of its floor (GG: open is also fine; the slot is the compromise);
//  - 6-10 emissive strips under the ceiling per room: cyan (colors.usable) for the two nearest the way on, pale violet-white (boundary) for the rest; never
//    red-orange (danger and enemies only) and never magenta (RIVAL);
//  - baked ambient occlusion: the stone is a few huge quads with no vertices to carry AO, so it is laid as gradient vertex-colour strips (SKYGRIND/Ao27,
//    black, alpha = 1 - factor) on the floor and the wall face beside each box (inside corner, aoCorner) and along each box top's edges (aoEdge), easing to 1
//    over aoFalloff;
//  - at most maxLights real point lights in all (the room kit has two: this adds the rest, on the strips nearest the player);
//  - gradient ambient and exponential fog (RenderSettings) and a tighter stone fog far (the _SkyFogParams global), all restored when the view is destroyed.
// Created by CampaignLighting24.Create. SetVisible(false/true) is the lead's frame-ms A/B (roof ON vs OFF).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class RoofView27 : MonoBehaviour
    {
        SurvivalGame game;
        GameObject root;
        Light[] lights;
        readonly List<Vector3> stripAt = new List<Vector3>();
        readonly List<Color> stripColor = new List<Color>();
        float lightIntensity, stripBrightness;
        float lightClock;
        // saved scene state, restored on destroy
        AmbientMode prevAmbient; Color prevSky, prevEq, prevGround, prevFogColor; bool prevFog; FogMode prevFogMode; float prevDensity;
        Vector4 prevFogParams; bool changedStoneFog;

        /// <summary>Counts for the report and the reel line.</summary>
        public int ceilingQuads, strips, aoQuads, extraLights;
        public float ceilingY;
        public bool visible = true;

        static readonly int SkyFogParams = Shader.PropertyToID("_SkyFogParams");

        static Color Hex(double v)
        {
            int h = (int)v;
            return new Color(((h >> 16) & 255) / 255f, ((h >> 8) & 255) / 255f, (h & 255) / 255f);
        }

        static Vector3 P(double x, double y, double z) => new Vector3((float)x, (float)y, (float)-z); // web -> Unity (z flips)

        public static RoofView27 Create(SurvivalGame g, Transform parent, SurvivalMaterials mats)
        {
            if (N27Blocks.ForceOff || !g.cfg.Design("n27.roof27.enabled") || g.map == null || g.map.json == null || !g.map.json.ContainsKey("campaign")) return null;
            var go = new GameObject("Roof 27");
            go.transform.SetParent(parent, false);
            var v = go.AddComponent<RoofView27>();
            v.Build(g, mats);
            return v;
        }

        /// <summary>Show or hide the roof's meshes and lights (the lead's ms A/B); the ambient and fog stay.</summary>
        public void SetVisible(bool on)
        {
            visible = on;
            if (root != null) root.SetActive(on);
            if (lights != null) foreach (Light l in lights) if (l != null) l.enabled = on;
        }

        void Build(SurvivalGame g, SurvivalMaterials mats)
        {
            game = g;
            GameConfig cfg = g.cfg;
            LevelDef level = LevelDef.From(g.map.json);
            root = new GameObject("Roof meshes");
            root.transform.SetParent(transform, false);

            double maxY = g.map.floor.max.y;
            foreach (MapBox b in g.map.boxes) if (b.max.y > maxY) maxY = b.max.y;
            double ceilY = maxY + cfg.Num("design.n27.roof27.ceilingAbove");
            ceilingY = (float)ceilY;

            // --- the ceiling ---
            var ceil = new MeshKit();
            double share = cfg.Num("design.n27.roof27.skylightShare");
            Color white = Color.white;
            HashSet<string> closed = LightKit29.ClosedRooms(g); // NIGHT29 item 5: a room whose plan says `roof: true` keeps no skylight
            // NIGHT33 L (design.n33.look33): each room's ceiling and strips sit under that room's OWN roof (LightPlan33's roomCeil), not at one level-wide height
            // above every lower room's roof slab (where the strips hung hidden or floating); no link ceilings (each door has its lintel)
            LightPlan33.Plan plan33 = N33Fixes.ViewRowOn(cfg, "look33") ? LightPlan33.GreenFor(g.map.json, cfg, N30Fixes.ViewRowOn(cfg, "look30"), out _) : null;
            double levelCeilY = ceilY;
            foreach (RoomDef r in level.rooms)
            {
                if (r.x1 <= r.x0 || r.z1 <= r.z0) continue;
                ceilY = plan33 != null && plan33.roomCeil.TryGetValue(r.id, out double rcy) ? rcy - Light54.Lift(UnityTuning.Look33CeilLift) : levelCeilY; // NIGHT54 light54: zLiftM under the slab (off: 5 mm)
                if (r.kind == "boss" && share > 0 && !closed.Contains(r.id))
                {
                    double s = System.Math.Sqrt(share), w = r.x1 - r.x0, d = r.z1 - r.z0;
                    double hx0 = (r.x0 + r.x1) * 0.5 - w * s * 0.5, hx1 = hx0 + w * s, hz0 = (r.z0 + r.z1) * 0.5 - d * s * 0.5, hz1 = hz0 + d * s;
                    CeilRect(ceil, r.x0, r.z0, hx0, r.z1, ceilY, white);
                    CeilRect(ceil, hx1, r.z0, r.x1, r.z1, ceilY, white);
                    CeilRect(ceil, hx0, r.z0, hx1, hz0, ceilY, white);
                    CeilRect(ceil, hx0, hz1, hx1, r.z1, ceilY, white);
                    ceilingQuads += 4;
                }
                else { CeilRect(ceil, r.x0, r.z0, r.x1, r.z1, ceilY, white); ceilingQuads++; }
            }
            ceilY = levelCeilY;
            foreach (DoorDef door in level.doors)
            {
                if (plan33 != null) break;
                RoomDef a = level.rooms.Find(q => q.id == door.a), b = level.rooms.Find(q => q.id == door.b);
                if (a == null || b == null) continue;
                if (LinkRect(a, b, out double lx0, out double lz0, out double lx1, out double lz1)) { CeilRect(ceil, lx0, lz0, lx1, lz1, ceilY, white); ceilingQuads++; }
            }
            Shader ceilShader = Shader.Find("SKYGRIND/Ceiling27") ?? Shader.Find("SKYGRIND/Particle");
            var ceilMat = new Material(ceilShader);
            // NIGHT35 V lights35 (the lead's n34-r5 f5 / f5b: the flat violet ceiling read as the dusk sky, so its panels and strips "floated"): a dark warm
            // grey ceiling with the joists visible against it; the fog keeps roof27's violet
            Color ceilCol = N35Fixes.ViewRowOn(cfg, "lights35") ? Look.Hex(UnityTuning.Look35CeilingHex) : Hex(cfg.Num("design.n27.roof27.ceilingHex"));
            // GG 2026-10-26 (the rooms too dark): SetColor already converts a gamma colour to linear in this linear-space project, so `.linear`
            // converted it twice (0x4a403a, ~74/255 meant, drew near black). With Bright71 on the colour goes in once, as meant.
            Color ceilSet = Bright71.On ? ceilCol : ceilCol.linear;
            ceilMat.SetColor("_Color", ceilSet);
            ceilMat.SetColor("_BaseColor", ceilSet); // the fallback shader's name
            ceilMat.SetFloat("_Cull", 0);
            MeshKit.Object("Ceiling", root.transform, ceil.Build("roof27 ceiling"), ceilMat, shadows: false);

            // --- the emissive strips ---
            stripBrightness = (float)cfg.Num("design.n27.roof27.stripBrightness");
            Color cyan = Look.C(cfg, "colors.usable") * stripBrightness, pale = Hex(cfg.Num("design.n27.roof27.boundaryHex")) * stripBrightness;
            var kitCyan = new MeshKit();
            var kitPale = new MeshKit();
            double sw = cfg.Num("design.n27.roof27.stripWidth");
            int smin = (int)cfg.Num("design.n27.roof27.stripMin"), smax = (int)cfg.Num("design.n27.roof27.stripMax");
            if (plan33 != null) Strips33(cfg, plan33, kitCyan, kitPale);
            foreach (RoomDef r in level.rooms)
            {
                if (plan33 != null) break;
                if (r.x1 <= r.x0 || r.z1 <= r.z0) continue;
                double w = r.x1 - r.x0, d = r.z1 - r.z0, per = 2 * (w + d);
                int n = SkateMath27.StripCount(per, 6, smin, smax);
                // n segments around the perimeter inset 0.4 m, each 55 % of its slot; the two whose middle is nearest the exit are cyan
                double inset = 0.4;
                double iw = w - 2 * inset, id = d - 2 * inset, ip = 2 * (iw + id);
                if (iw <= 0 || id <= 0) continue;
                var mids = new List<Vector3>();
                var spans = new List<(double a0, double a1)>();
                for (int i = 0; i < n; i++)
                {
                    double t0 = ip * (i + 0.225) / n, t1 = ip * (i + 0.775) / n;
                    spans.Add((t0, t1));
                    PerimeterPoint(r, inset, iw, id, (t0 + t1) * 0.5, out double mx, out double mz);
                    mids.Add(P(mx, ceilY - 0.03, mz));
                }
                Vector3 exit = P(r.exit.x, ceilY, r.exit.z);
                var order = new List<int>();
                for (int i = 0; i < n; i++) order.Add(i);
                order.Sort((p, q) => (new Vector2(mids[p].x - exit.x, mids[p].z - exit.z)).sqrMagnitude.CompareTo((new Vector2(mids[q].x - exit.x, mids[q].z - exit.z)).sqrMagnitude));
                for (int i = 0; i < n; i++)
                {
                    bool isCyan = order.IndexOf(i) < 2;
                    MeshKit k = isCyan ? kitCyan : kitPale;
                    // the segment as a run of short quads along the perimeter (it turns corners)
                    double a0 = spans[i].a0, a1 = spans[i].a1;
                    int pieces = Mathf.Max(1, Mathf.CeilToInt((float)((a1 - a0) / 2.0)));
                    for (int pc = 0; pc < pieces; pc++)
                    {
                        double u0 = a0 + (a1 - a0) * pc / pieces, u1 = a0 + (a1 - a0) * (pc + 1) / pieces;
                        PerimeterPoint(r, inset, iw, id, u0, out double x0, out double z0);
                        PerimeterPoint(r, inset, iw, id, u1, out double x1, out double z1);
                        double dx = x1 - x0, dz = z1 - z0, len = System.Math.Sqrt(dx * dx + dz * dz);
                        if (len < 1e-4) continue;
                        double nx = -dz / len * sw * 0.5, nz = dx / len * sw * 0.5;
                        k.Quad(P(x0 - nx, ceilY - 0.03, z0 - nz), P(x1 - nx, ceilY - 0.03, z1 - nz), P(x1 + nx, ceilY - 0.03, z1 + nz), P(x0 + nx, ceilY - 0.03, z0 + nz), Vector3.down, Color.white);
                    }
                    stripAt.Add(mids[i]);
                    stripColor.Add(isCyan ? Look.C(cfg, "colors.usable") : Hex(cfg.Num("design.n27.roof27.boundaryHex")));
                    strips++;
                }
            }
            var cyanMat = new Material(mats.neon); cyanMat.SetColor("_BaseColor", cyan);
            var paleMat = new Material(mats.neon); paleMat.SetColor("_BaseColor", pale);
            MeshKit.Object("Strips cyan", root.transform, kitCyan.Build("roof27 strips cyan"), cyanMat, shadows: false);
            MeshKit.Object("Strips boundary", root.transform, kitPale.Build("roof27 strips pale"), paleMat, shadows: false);

            // --- the AO ---
            var ao = new MeshKit();
            BuildAo(cfg, g.map, ao);
            var aoShader = Shader.Find("SKYGRIND/Ao27");
            if (aoShader != null && ao.Vertices > 0)
                MeshKit.Object("AO strips", root.transform, ao.Build("roof27 ao"), new Material(aoShader), shadows: false);

            // --- lights: at most maxLights real point lights in all; the room kit already owns two ---
            int extra = Mathf.Clamp((int)cfg.Num("design.n27.roof27.maxLights") - 2, 0, 4);
            if (plan33 != null) extra = 0; // NIGHT33 L round 2: the strips are emissive only (L1 recessed strip: "none, or a light every 6 m"); the room's 5-6 real lights are the kit's
            lightIntensity = (float)cfg.Num("design.n27.roof27.lightIntensity") * UnityTuning.LightScale;
            lights = new Light[extra];
            for (int i = 0; i < extra; i++)
            {
                var lg = new GameObject("Roof light " + i);
                lg.transform.SetParent(transform, false);
                var l = lg.AddComponent<Light>();
                l.type = LightType.Point; l.shadows = LightShadows.None; l.range = (float)cfg.Num("design.n27.roof27.lightRange"); l.intensity = 0;
                lights[i] = l;
            }
            extraLights = extra;

            // --- ambient and fog ---
            prevAmbient = RenderSettings.ambientMode; prevSky = RenderSettings.ambientSkyColor; prevEq = RenderSettings.ambientEquatorColor; prevGround = RenderSettings.ambientGroundColor;
            prevFog = RenderSettings.fog; prevFogMode = RenderSettings.fogMode; prevDensity = RenderSettings.fogDensity; prevFogColor = RenderSettings.fogColor;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Hex(cfg.Num("design.n27.roof27.ambientSkyHex"));
            RenderSettings.ambientEquatorColor = Hex(cfg.Num("design.n27.roof27.ambientEquatorHex"));
            RenderSettings.ambientGroundColor = Hex(cfg.Num("design.n27.roof27.ambientGroundHex"));
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = (float)cfg.Num("design.n27.roof27.fogDensity");
            RenderSettings.fogColor = Hex(cfg.Num("design.n27.roof27.ceilingHex"));
            int theme33 = plan33 != null ? LightPlan33.Theme(g.map.json) : -1;
            if (theme33 >= 0)
            {
                // NIGHT33 L round 2 (look33): the theme's ambient and fog (L1 post table): dusk violet, never orange (the lava's light stays local)
                RenderSettings.ambientSkyColor = Look.Hex(UnityTuning.Look33ThemeAmbientSky[theme33]);
                RenderSettings.ambientEquatorColor = Look.Hex(UnityTuning.Look33ThemeAmbientEquator[theme33]);
                RenderSettings.ambientGroundColor = Look.Hex(UnityTuning.Look33ThemeAmbientGround[theme33]);
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogDensity = (float)UnityTuning.Look33ThemeFogDensity[theme33];
                RenderSettings.fogColor = Look.Hex(UnityTuning.Look33ThemeFogHex[theme33]);
                // NIGHT49 V (look49): the colour script's row for the level (UnityTuning.V49.cs); PostFx holds the same values every frame after.
                // NIGHT50 V (G3): PostFx's own switch (the reel's force, else the row AND this level in look49.numbers.levels) and its own writer.
                if (PostFx.Look49OnFor(cfg, g.map.json)) PostFx.Ambient49(theme33);
            }
            prevFogParams = Shader.GetGlobalVector(SkyFogParams);
            if (prevFogParams.y > 0)
            {
                var f = prevFogParams;
                f.y *= (float)cfg.Num("design.n27.roof27.fogFarScale");
                Shader.SetGlobalVector(SkyFogParams, f);
                changedStoneFog = true;
            }
        }

        /// <summary>NIGHT33 L (look33): the strips from the shared plan, each a flat bar flush under its own room's roof (the face the player sees, facing down),
        /// and the strip lights where the plan put them (under each strip's middle piece).</summary>
        void Strips33(GameConfig cfg, LightPlan33.Plan plan, MeshKit kitCyan, MeshKit kitPale)
        {
            foreach (LightPlan33.Fix f in plan.fixes)
            {
                if (f.kind != "roofStrip") continue;
                MeshKit k = f.color == "cyan" ? kitCyan : kitPale;
                double y = f.box.y0;
                k.Quad(P(f.box.x0, y, f.box.z0), P(f.box.x1, y, f.box.z0), P(f.box.x1, y, f.box.z1), P(f.box.x0, y, f.box.z1), Vector3.down, Color.white);
                k.Quad(P(f.box.x0, y, f.box.z1), P(f.box.x1, y, f.box.z1), P(f.box.x1, y, f.box.z0), P(f.box.x0, y, f.box.z0), Vector3.down, Color.white); // both windings: the neon material culls back faces
            }
            foreach (LightPlan33.Lit l in plan.lights)
            {
                if (l.kind != "roof") continue;
                stripAt.Add(P(l.x, l.y + 0.4, l.z)); // Update hangs the light 0.4 m under this point (the plan's light position)
                stripColor.Add(l.color == "cyan" ? Look.C(cfg, "colors.usable") : Hex(cfg.Num("design.n27.roof27.boundaryHex")));
                strips++;
            }
        }

        void OnDestroy()
        {
            if (lights == null) return; // never built
            RenderSettings.ambientMode = prevAmbient; RenderSettings.ambientSkyColor = prevSky; RenderSettings.ambientEquatorColor = prevEq; RenderSettings.ambientGroundColor = prevGround;
            RenderSettings.fog = prevFog; RenderSettings.fogMode = prevFogMode; RenderSettings.fogDensity = prevDensity; RenderSettings.fogColor = prevFogColor;
            if (changedStoneFog) Shader.SetGlobalVector(SkyFogParams, prevFogParams);
        }

        // a point `t` metres along the perimeter of the room rect inset by `inset`, walking x0 -> x1 along z0, then z, then back
        static void PerimeterPoint(RoomDef r, double inset, double iw, double id, double t, out double x, out double z)
        {
            double x0 = r.x0 + inset, z0 = r.z0 + inset;
            double ip = 2 * (iw + id);
            t = ((t % ip) + ip) % ip;
            if (t < iw) { x = x0 + t; z = z0; }
            else if (t < iw + id) { x = x0 + iw; z = z0 + (t - iw); }
            else if (t < 2 * iw + id) { x = x0 + iw - (t - iw - id); z = z0 + id; }
            else { x = x0; z = z0 + id - (t - 2 * iw - id); }
        }

        static void CeilRect(MeshKit k, double x0, double z0, double x1, double z1, double y, Color c)
        {
            if (x1 <= x0 || z1 <= z0) return;
            k.Quad(P(x0, y, z0), P(x1, y, z0), P(x1, y, z1), P(x0, y, z1), Vector3.down, c);
        }

        /// <summary>The link between two doored rooms: the gap along the axis that separates them, across the overlap of their other extents (or their union).</summary>
        static bool LinkRect(RoomDef a, RoomDef b, out double x0, out double z0, out double x1, out double z1)
        {
            double zGap = System.Math.Max(a.z0, b.z0) - System.Math.Min(a.z1, b.z1), xGap = System.Math.Max(a.x0, b.x0) - System.Math.Min(a.x1, b.x1);
            x0 = z0 = x1 = z1 = 0;
            if (zGap >= xGap && zGap > 0)
            {
                z0 = System.Math.Min(a.z1, b.z1); z1 = System.Math.Max(a.z0, b.z0);
                double ox0 = System.Math.Max(a.x0, b.x0), ox1 = System.Math.Min(a.x1, b.x1);
                if (ox1 > ox0) { x0 = ox0; x1 = ox1; } else { x0 = System.Math.Min(a.x0, b.x0); x1 = System.Math.Max(a.x1, b.x1); }
                return true;
            }
            if (xGap > 0)
            {
                x0 = System.Math.Min(a.x1, b.x1); x1 = System.Math.Max(a.x0, b.x0);
                double oz0 = System.Math.Max(a.z0, b.z0), oz1 = System.Math.Min(a.z1, b.z1);
                if (oz1 > oz0) { z0 = oz0; z1 = oz1; } else { z0 = System.Math.Min(a.z0, b.z0); z1 = System.Math.Max(a.z1, b.z1); }
                return true;
            }
            return false;
        }

        // --- AO ---
        void BuildAo(GameConfig cfg, SurvivalMap map, MeshKit k)
        {
            float corner = (float)cfg.Num("design.n27.roof27.aoCorner"), edge = (float)cfg.Num("design.n27.roof27.aoEdge"), fall = (float)cfg.Num("design.n27.roof27.aoFalloff");
            float aC = 1f - corner, aE = 1f - edge;
            double floorY = map.floor.max.y;
            foreach (MapBox b in map.boxes)
            {
                double w = b.max.x - b.min.x, d = b.max.z - b.min.z, h = b.max.y - b.min.y;
                if (h < 0.5 || w < 0.05 || d < 0.05) continue;
                bool onFloor = b.min.y <= floorY + 0.1;
                double lift = 0.012;
                if (onFloor)
                {
                    // the floor beside the box: four strips from the wall face outward
                    FloorStrip(k, b.min.x, b.min.z, b.max.x, b.min.z, 0, -1, fall, floorY + lift, aC);
                    FloorStrip(k, b.min.x, b.max.z, b.max.x, b.max.z, 0, 1, fall, floorY + lift, aC);
                    FloorStrip(k, b.min.x, b.min.z, b.min.x, b.max.z, -1, 0, fall, floorY + lift, aC);
                    FloorStrip(k, b.max.x, b.min.z, b.max.x, b.max.z, 1, 0, fall, floorY + lift, aC);
                    // the wall face above the floor: up from its foot
                    double up = System.Math.Min(fall, h);
                    WallStrip(k, b.min.x, b.min.z, b.max.x, b.min.z, 0, -1, floorY, up, aC, lift);
                    WallStrip(k, b.min.x, b.max.z, b.max.x, b.max.z, 0, 1, floorY, up, aC, lift);
                    WallStrip(k, b.min.x, b.min.z, b.min.x, b.max.z, -1, 0, floorY, up, aC, lift);
                    WallStrip(k, b.max.x, b.min.z, b.max.x, b.max.z, 1, 0, floorY, up, aC, lift);
                }
                // the top's convex edges: inward from each rim (only on a top wide enough to hold two falloffs)
                if (w >= 2 * fall && d >= 2 * fall)
                {
                    double ty = b.max.y + lift;
                    FloorStrip(k, b.min.x, b.min.z, b.max.x, b.min.z, 0, 1, fall, ty, aE);
                    FloorStrip(k, b.min.x, b.max.z, b.max.x, b.max.z, 0, -1, fall, ty, aE);
                    FloorStrip(k, b.min.x, b.min.z, b.min.x, b.max.z, 1, 0, fall, ty, aE);
                    FloorStrip(k, b.max.x, b.min.z, b.max.x, b.max.z, -1, 0, fall, ty, aE);
                }
            }
            aoQuads = k.Vertices / 4;
        }

        // a horizontal gradient strip along the segment (x0,z0)-(x1,z1), `fall` m wide toward (dx,dz) (web axes); alpha `a` at the segment, 0 at the far side
        static void FloorStrip(MeshKit k, double x0, double z0, double x1, double z1, double dx, double dz, float fall, double y, float a)
        {
            var near = new Color(0, 0, 0, a);
            var far = new Color(0, 0, 0, 0);
            k.QuadGradient(P(x0, y, z0), P(x1, y, z1), P(x1 + dx * fall, y, z1 + dz * fall), P(x0 + dx * fall, y, z0 + dz * fall), Vector3.up, near, near, far, far);
        }

        // a vertical gradient strip up a wall face from `floorY`, pushed `lift` m out of the face (web axes dx, dz = the face's outward normal)
        static void WallStrip(MeshKit k, double x0, double z0, double x1, double z1, double dx, double dz, double floorY, double up, float a, double lift)
        {
            var near = new Color(0, 0, 0, a);
            var far = new Color(0, 0, 0, 0);
            double ox = dx * lift, oz = dz * lift;
            k.QuadGradient(P(x0 + ox, floorY, z0 + oz), P(x1 + ox, floorY, z1 + oz), P(x1 + ox, floorY + up, z1 + oz), P(x0 + ox, floorY + up, z0 + oz), P(dx, 0, dz), near, near, far, far);
        }

        void Update()
        {
            if (!visible || lights == null || lights.Length == 0 || stripAt.Count == 0 || game == null || game.cam == null) return;
            lightClock -= FeelClock.UnscaledDelta; // NIGHT63 D1: 0 in a same-run held frame
            if (lightClock > 0) return;
            lightClock = 0.25f; // re-pick the nearest strips four times a second; no light is created per frame
            Vector3 p = game.cam.transform.position;
            for (int li = 0; li < lights.Length; li++)
            {
                int best = -1;
                float bestD = float.MaxValue;
                for (int i = 0; i < stripAt.Count; i++)
                {
                    bool taken = false;
                    for (int q = 0; q < li; q++) if ((lights[q].transform.position - stripAt[i]).sqrMagnitude < 0.01f) { taken = true; break; }
                    if (taken) continue;
                    float dd = (stripAt[i] - p).sqrMagnitude;
                    if (dd < bestD) { bestD = dd; best = i; }
                }
                if (best < 0) { lights[li].intensity = 0; continue; }
                lights[li].transform.position = stripAt[best] + Vector3.down * 0.4f;
                lights[li].color = stripColor[best];
                lights[li].intensity = lightIntensity;
            }
        }
    }
}
