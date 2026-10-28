// NIGHT29 item 2 (builder V): the light sockets of a CAMPAIGN level (DESIGN29.md section A: lights are SOCKETS in the plan, never
// scattered; GG: lights "aren't properly placed and you can run through them"). Built from the map JSON's `campaign.lights` rows:
//   {id, room, kind: wall|ceiling|pool, at:[x,y,z], normal:[nx,ny,nz], size:[w,h,d], color: cyan|green|amber|white|orange, range, intensity}
// (web coordinates; `at` is the SURFACE point the fixture sits flush on; `normal` points from the surface into the room).
//  - wall: a slab standing out of the wall at most design.n29.lights.maxWallDepth (the wall's own collider absorbs the player);
//  - ceiling: a slab hanging at most CeilingFixtureMaxDepth below the ceiling (above any jump);
//  - pool: a dim flat panel on the floor (no height: nothing to run into) and the room's pooled light.
// Every fixture is a mesh with NO collider, so no fixture is a thing to run through: the depth caps keep each one inside the
// wall's or ceiling's own skin. A few real point lights (LightKitRealLights) follow the fixtures nearest the player. Colours: cyan is
// player-usable, green and amber are cool / warm accents, white is the boundary's pale violet-white; `orange` (the danger colour,
// colors.enemy) is for a hazard room's fixture only. Off with -skygrindOff29 or design.n29.lights.enabled false. View only.
// Also builds the door frames (DoorFrames29.cs) when design.n29.doors.enabled. Created by CampaignLighting24.Build.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The fixtures' pure rules (Crt29Tests' neighbour: Lights29Tests).</summary>
    public static class LightKit29Math
    {
        /// <summary>The fixture's depth: the row's (or the default) clamped to what its surface allows (wall: maxWallDepth; ceiling: CeilingFixtureMaxDepth; pool: 0).</summary>
        public static float Depth(string kind, float d, float maxWallDepth)
        {
            if (kind == "pool") return 0;
            float cap = kind == "ceiling" ? UnityTuning.CeilingFixtureMaxDepth : maxWallDepth;
            return Mathf.Clamp(d, 0.005f, cap);
        }

        /// <summary>The fixture's centre in Unity space: the surface point pushed out along the normal by half its depth (its back face stays on the surface).</summary>
        public static Vector3 Centre(Vector3 surfaceUnity, Vector3 normalUnity, float depth) => surfaceUnity + normalUnity.normalized * (depth * 0.5f);

    }

    public sealed partial class LightKit29 : MonoBehaviour // NIGHT47 F: partial (lava47: LightKit29.Lava47.cs)
    {
        sealed class Fixture
        {
            public string id, room, kind;
            public Vector3 at; // Unity space: where its real light would stand
            public Color color;
            public float range, intensity;
            public bool accent; // an amber or orange row: the room's one accent
            public Vector3 surface, normal; // NIGHT30 (look30's beams): the plan's socket point and its normal (Unity space)
            public float width; // NIGHT30: the fixture's width (m)
            public bool danger; // NIGHT33 L round 2: a brazier (Vfx33 flickers its light +-15 %; danger fixtures only)
            public bool down; // NIGHT33 L round 2: a ceiling panel: its light is a spot straight down (L1: spot down, angle 90; a pool on the floor, the ceiling stays dark)
        }

        bool look30; // NIGHT30 item 8 (builder V, design.n30.look30): bolder light: the accent light brighter, view-only beams on the accent and pool sockets
        /// <summary>For the reel lines: the view-only light beams built (look30).</summary>
        public int beams30, beamsSkipped30;

        SurvivalGame game;
        LevelDef level;
        readonly List<Fixture> fixtures = new List<Fixture>();
        Light[] lights;
        float clock;
        float lightScale;
        /// <summary>Counts for the report and the reel line.</summary>
        public int fixtureCount, doorFrames;

        public static LightKit29 Create(SurvivalGame g, Transform parent, SurvivalMaterials mats)
        {
            if (g == null || g.map == null || g.map.json == null || !g.map.json.ContainsKey("campaign")) return null;
            bool lightsOn = N29Fixes.ViewRowOn(g.cfg, "lights"), doorsOn = N29Fixes.ViewRowOn(g.cfg, "doors");
            if (!lightsOn && !doorsOn) return null;
            var go = new GameObject("Light kit 29");
            go.transform.SetParent(parent, false);
            var k = go.AddComponent<LightKit29>();
            k.Build(g, mats, lightsOn, doorsOn);
            return k;
        }

        /// <summary>
        /// The rooms whose plan row says `roof: true` (DESIGN29 section A: a fight room is CLOSED): RoofView27 gives them no skylight.
        /// Empty with -skygrindOff29 or while all three view rows (lights, doors, props) are off, so NIGHT28's roofs stay as they were.
        /// </summary>
        public static HashSet<string> ClosedRooms(SurvivalGame g)
        {
            var set = new HashSet<string>();
            GameConfig cfg = g.cfg;
            if (!(N29Fixes.ViewRowOn(cfg, "lights") || N29Fixes.ViewRowOn(cfg, "doors") || N29Fixes.ViewRowOn(cfg, "props"))) return set;
            if (g.map == null || g.map.json == null || !g.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> c) || !c.TryGetValue("rooms", out object ro) || !(ro is List<object> rooms)) return set;
            foreach (object o in rooms)
                if (o is Dictionary<string, object> r && r.TryGetValue("roof", out object rf) && rf is bool b && b && r.TryGetValue("id", out object id) && id is string s) set.Add(s);
            return set;
        }

        public static Color ColorNamed(GameConfig cfg, string name) => Named(cfg, name, -1);

        /// <summary>NIGHT41 K2 (look41): a plan colour name in a level's theme (Look41.ThemeFor; -1 = NIGHT40's colours).</summary>
        public static Color ColorNamed(GameConfig cfg, string name, int theme41) => Named(cfg, name, theme41);

        int theme41 = -1; // NIGHT41 K2 (design.n41.look41): this level's theme for the room light's colour; -1 with the row off

        static Color Named(GameConfig cfg, string name, int theme41)
        {
            if (LightPlan33.On49(cfg) && Named49(name, theme41, out Color c49)) return c49; // NIGHT49 L (lights49): the role tokens and the level accent (LightKit29.L49.cs)
            switch (name)
            {
                case "green": return Look.Hex(UnityTuning.FixtureGreenHex);
                case "warm": return Look.Hex(Look41.KeyHex(theme41)); // NIGHT33 L: look33's ceiling panels (lighting, not a usable thing); NIGHT41 K2: the theme's key with look41
                case "amber": return Look.Hex(UnityTuning.FixtureAmberHex);
                case "white": return Look.Hex((uint)cfg.Num("design.n27.roof27.boundaryHex"));
                case "orange": return Look.C(cfg, "colors.enemy"); // danger only (a hazard room's fixture)
                default: return Look.C(cfg, "colors.usable");
            }
        }

        static Vector3 U(List<object> a) => new Vector3((float)PortData.Num(a[0]), (float)PortData.Num(a[1]), (float)-PortData.Num(a[2])); // web -> Unity (z flips)

        /// <summary>NIGHT31 G: is this ceiling point (Unity space) over a grapple node, within UnityTuning.Grapple31PlateClear on the floor plan, with the mark's row on?</summary>
        static bool OverNode31(SurvivalGame g, Vector3 at)
        {
            if (!N31Fixes.ViewRowOn(g.cfg, "grappleMark") || g.sim == null || g.sim.map == null) return false;
            foreach (Vec3 node in g.sim.map.grappleNodes)
            {
                Vector3 p = WebSpace.ToUnity(node);
                float dx = p.x - at.x, dz = p.z - at.z;
                if (at.y > p.y && dx * dx + dz * dz < UnityTuning.Grapple31PlateClear * UnityTuning.Grapple31PlateClear) return true;
            }
            return false;
        }

        void Build(SurvivalGame g, SurvivalMaterials mats, bool lightsOn, bool doorsOn)
        {
            game = g;
            GameConfig cfg = g.cfg;
            level = LevelDef.From(g.map.json);
            lightScale = UnityTuning.LightScale;
            theme41 = Look41.ThemeFor(cfg, g.map.json); // NIGHT41 K2: before any Named call below
            Dictionary<string, object> campaign = PortData.Obj(g.map.json["campaign"]);
            if (doorsOn) doorFrames = DoorFrames29.Build(g, transform, mats, campaign);
            bool propsOn = N29Fixes.ViewRowOn(cfg, "props");
            PropsView29.Create(g, transform, mats, propsOn); // NIGHT29 round 2: the authored props (design.n29.props) and the hazard strips
            if (!lightsOn || !campaign.TryGetValue("lights", out object lo) || !(lo is List<object> rows)) return;
            float maxWall = (float)cfg.Num("design.n29.lights.maxWallDepth");
            float bright = (float)cfg.Num("design.n27.roof27.stripBrightness");
            var kits = new Dictionary<string, MeshKit>();
            var colors = new Dictionary<string, Color>();
            // NIGHT33 L (design.n33.look33): the sockets from the shared plan (snapped flush, pools as ceiling downlights or lava lights; LightPlan33.Green)
            LightPlan33.Plan plan33 = N33Fixes.ViewRowOn(cfg, "look33") ? LightPlan33.GreenFor(g.map.json, cfg, N30Fixes.ViewRowOn(cfg, "look30"), out LightPlan33.Level lv33) : null;
            if (plan33 != null) Build33(g, mats, plan33, bright);
            if (plan33 != null) Build49(plan33); // NIGHT49 L (lights49): the landmark's own light (nothing with the row off: no landmark in the plan)
            if (plan33 == null) foreach (object ro in rows)
            {
                if (!(ro is Dictionary<string, object> r) || !r.ContainsKey("at")) continue;
                string kind = r.TryGetValue("kind", out object ko) && ko is string ks ? ks : "wall";
                string cname = r.TryGetValue("color", out object co) && co is string cs ? cs : "cyan";
                Vector3 at = U(PortData.Arr(r["at"]));
                Vector3 n = r.TryGetValue("normal", out object no) && no is List<object> nl && nl.Count >= 3 ? U(nl) : (kind == "ceiling" ? Vector3.down : kind == "pool" ? Vector3.up : Vector3.forward);
                if (n.sqrMagnitude < 1e-6f) n = kind == "ceiling" ? Vector3.down : Vector3.up;
                n.Normalize();
                float w = UnityTuning.FixtureDefaultW, h = UnityTuning.FixtureDefaultH, d = UnityTuning.FixtureDefaultD;
                if (r.TryGetValue("size", out object so) && so is List<object> sl && sl.Count >= 3)
                {
                    w = (float)PortData.Num(sl[0]); h = (float)PortData.Num(sl[1]); d = (float)PortData.Num(sl[2]);
                }
                d = LightKit29Math.Depth(kind, d, maxWall);
                string key = cname;
                if (!kits.TryGetValue(key, out MeshKit kit)) { kits[key] = kit = new MeshKit(); colors[key] = Named(cfg, cname, theme41); }
                Color col = Color.white;
                if (kind == "pool" && N30Fixes.ViewRowOn(cfg, "look30"))
                {
                    // NIGHT30 item 8 (builder V, design.n30.look30): no emissive floor panel for a pool light. The quad drew at the full strip brightness
                    // (the kits are keyed by colour, so NIGHT29's "pool" dimming never matched) and was the brightest thing in GG's frame 170047 while
                    // lighting nothing; the pool keeps its real light (the Fixture below), which lights the floor where the plan put it.
                }
                else if (kind == "pool")
                {
                    float y = at.y + Light54.Lift(UnityTuning.PoolLift); // NIGHT54 light54: zLiftM (off: 20 mm)
                    kit.Quad(new Vector3(at.x - w * 0.5f, y, at.z - h * 0.5f), new Vector3(at.x + w * 0.5f, y, at.z - h * 0.5f), new Vector3(at.x + w * 0.5f, y, at.z + h * 0.5f), new Vector3(at.x - w * 0.5f, y, at.z + h * 0.5f), Vector3.up, col);
                }
                else if (kind == "ceiling" && OverNode31(g, at))
                {
                    // NIGHT31 G (design.n31.grappleMark; the lead's r2 frames ring31 f0028 / grapple31 f0018: the node read against "a big bright WHITE rectangle"):
                    // a ceiling panel straight over a grapple node is not drawn, so the cyan ring and its diamond read against the dark roof. Its real light stays.
                }
                else
                {
                    Vector3 right, up;
                    if (kind == "ceiling" || Mathf.Abs(n.y) > 0.9f) { right = Vector3.right; up = Vector3.forward; }
                    else { right = Vector3.Cross(Vector3.up, n).normalized; up = Vector3.up; }
                    OrientedBox(kit, LightKit29Math.Centre(at, n, d), right, up, n, w * 0.5f, h * 0.5f, d * 0.5f, col);
                }
                var f = new Fixture
                {
                    id = r.TryGetValue("id", out object io) ? io as string : "", room = r.TryGetValue("room", out object rmo) ? rmo as string : "", kind = kind,
                    at = at + n * (kind == "pool" ? 1.5f : 0.6f), color = colors[key], accent = cname == "amber" || cname == "orange",
                    range = r.TryGetValue("range", out object rg) ? (float)PortData.Num(rg) : 8f,
                    intensity = r.TryGetValue("intensity", out object it) ? (float)PortData.Num(it) : 1f,
                    surface = at, normal = n, width = w,
                };
                fixtures.Add(f);
                fixtureCount++;
            }
            foreach (var kv in kits)
            {
                var m = new Material(mats.neon);
                m.SetColor("_BaseColor", colors[kv.Key] * (kv.Key == "pool" ? bright * UnityTuning.PoolBrightness : bright));
                MeshKit.Object("Fixtures " + kv.Key, transform, kv.Value.Build("fixtures29 " + kv.Key), m, shadows: false);
            }
            look30 = N30Fixes.ViewRowOn(cfg, "look30");
            if (look30) { if (plan33 != null) Beams33(mats, plan33); else BuildBeams30(mats); }
            lavaSlot33 = plan33 != null; // NIGHT33 L round 2: look33 keeps one more slot for the room's lava light (L1 lava row: local light; 3-6 per room in all)
            int nLights = Mathf.Min(plan33 != null ? UnityTuning.Look33KitSlots + 1 : UnityTuning.LightKitRealLights, fixtures.Count);
            lights = new Light[nLights];
            for (int i = 0; i < nLights; i++)
            {
                var lg = new GameObject("Fixture light " + i);
                lg.transform.SetParent(transform, false);
                var l = lg.AddComponent<Light>();
                l.type = LightType.Point; l.shadows = LightShadows.None; l.intensity = 0;
                lights[i] = l;
            }
            BuildLava47(g); // NIGHT47 F lava47 (LightKit29.Lava47.cs): one fixed light per lava fixture (nothing with the row off)
        }

        /// <summary>
        /// NIGHT30 item 8 (look30): view-only LIGHT BEAMS (DESIGN30 E: "volumetric-looking beams that are view-only on landmarks"): a faint additive shaft
        /// of two crossed tapered quads in the socket's colour, for each ACCENT socket (amber / orange: the room's one accent, pointed by the plan at its hero
        /// object; along its normal, tipped down) and each POOL socket (from above down onto the pool: the plan's goal marker, now that its floor quad is gone).
        /// One mesh per colour; no light of its own, no collider.
        /// </summary>
        void BuildBeams30(SurvivalMaterials mats)
        {
            var kits = new Dictionary<Color, MeshKit>();
            // round 5 (the lead's r5 frames): a shaft never stands near the player's entry (a room's spawn) nor on a pickup (its own glow: f6's pool shaft
            // and its health pickup made one glowing white figure), and its wide end is capped
            var avoid = new List<Vector3>();
            if (level != null) foreach (RoomDef rd in level.rooms) avoid.Add(WebSpace.ToUnity(rd.spawn));
            if (game != null && game.map != null && game.map.json != null && game.map.json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp
                && camp.TryGetValue("props", out object po) && po is List<object> props)
                foreach (object o in props)
                    if (o is Dictionary<string, object> pr && pr.TryGetValue("pickup", out object pk) && pk is bool isPick && isPick && pr.TryGetValue("at", out object pa) && pa is List<object> pl && pl.Count >= 3)
                        avoid.Add(U(pl));
            foreach (Fixture f in fixtures)
            {
                bool pool = f.kind == "pool";
                if (!pool && !f.accent) continue;
                bool near = false;
                foreach (Vector3 a in avoid)
                {
                    Vector3 d = a - f.surface;
                    d.y = 0;
                    if (d.magnitude < UnityTuning.Look30BeamKeepClear) { near = true; break; }
                }
                if (near) { beamsSkipped30++; continue; }
                Vector3 top, bottom;
                float wTop, wBottom;
                if (pool)
                {
                    bottom = f.surface + Vector3.up * 0.02f;
                    top = f.surface + Vector3.up * UnityTuning.Look30BeamPoolHeight;
                    wTop = UnityTuning.Look30BeamNarrow;
                    wBottom = Mathf.Clamp(f.width, UnityTuning.Look30BeamNarrow, UnityTuning.Look30BeamMaxWidth);
                }
                else
                {
                    Vector3 dir = (f.normal + Vector3.down * UnityTuning.Look30BeamTilt).normalized;
                    top = f.surface + f.normal * 0.05f;
                    bottom = top + dir * UnityTuning.Look30BeamLength;
                    wTop = Mathf.Max(UnityTuning.Look30BeamNarrow, f.width * 0.5f);
                    wBottom = UnityTuning.Look30BeamSpread;
                }
                if (!kits.TryGetValue(f.color, out MeshKit k)) kits[f.color] = k = new MeshKit();
                Vector3 axis = (bottom - top).normalized;
                Vector3 s1 = Vector3.Cross(axis, Mathf.Abs(axis.y) > 0.9f ? Vector3.right : Vector3.up).normalized;
                Vector3 s2 = Vector3.Cross(axis, s1).normalized;
                foreach (Vector3 s in new[] { s1, s2 })
                {
                    Vector3 a = top - s * wTop * 0.5f, b = top + s * wTop * 0.5f, c = bottom + s * wBottom * 0.5f, d = bottom - s * wBottom * 0.5f;
                    k.Quad(a, b, c, d, Vector3.up, Color.white);
                    k.Quad(d, c, b, a, Vector3.up, Color.white);
                }
                beams30++;
            }
            int i = 0;
            foreach (var kv in kits)
            {
                Material m = FxDraw.Additive(mats.neon, "Beams 30 " + i);
                m.SetColor("_BaseColor", kv.Key * UnityTuning.Look30BeamGain);
                MeshKit.Object("Beams 30 " + i, transform, kv.Value.Build("beams30 " + i), m, shadows: false);
                i++;
            }
        }

        // ==================== NIGHT33 L (design.n33.look33): no floating light (DESIGN33 L round 1) ====================
        /// <summary>For the reel line: sockets drawn, lava lights, shafts built by look33.</summary>
        public int sockets33, lava33, beams33;
        bool lavaSlot33;
        /// <summary>For Vfx33: the real lights and the fixture each now follows (a danger fixture flickers).</summary>
        public Light[] Lights => lights;
        public bool LightIsDanger(int i) => lightFix != null && i < lightFix.Length && lightFix[i] >= 0 && lightFix[i] < fixtures.Count && fixtures[lightFix[i]].danger;
        int[] lightFix;
        float[] baseI33;
        /// <summary>The slot's intensity as the last repick set it (Vfx33's flicker base).</summary>
        public float BaseIntensity(int i) => baseI33 != null && i < baseI33.Length ? baseI33[i] : 0f;

        static Vector3 U33(double x, double y, double z) => new Vector3((float)x, (float)y, (float)-z); // web -> Unity (z flips)

        /// <summary>look33: the lights where the plan put them (0.4 m in front of each fixture, under a ceiling panel for a floor pool, 1.2 m over the lava, at a
        /// pole lamp's head), with L1's range and intensity per fixture type. Round 2: the fixture MESHES are all CampaignLighting24's (Fixtures33, one pass).</summary>
        void Build33(SurvivalGame g, SurvivalMaterials mats, LightPlan33.Plan plan, float bright)
        {
            GameConfig cfg = g.cfg;
            foreach (LightPlan33.Lit l in plan.lights)
            {
                if (l.kind != "socket" && l.kind != "pool" && l.kind != "lava") continue;
                if (l.kind == "lava") lava33++;
                else sockets33++;
                Vector3 at = U33(l.x, l.y, l.z);
                fixtures.Add(new Fixture
                {
                    id = l.id ?? "", room = l.room ?? "", kind = l.kind == "socket" ? "wall" : l.kind, // slot 0 prefers a pool (the goal's light); the lava slot a lava light
                    at = at, color = Named(cfg, l.color ?? "cyan", theme41), accent = l.accent, range = (float)l.range, intensity = (float)l.intensity, surface = at, normal = Vector3.down, width = 1f,
                    danger = l.type == "brazier", down = l.type == "panel",
                });
                fixtureCount++;
            }
        }

        /// <summary>look33: is this plan fix (web space) a ceiling panel straight over a grapple node (NIGHT31 G's rule: no bright panel there; its frame and light stay)?</summary>
        public static bool OverNode33(SurvivalGame g, LightPlan33.Fix f) =>
            OverNode31(g, U33((f.box.x0 + f.box.x1) * 0.5, (f.box.y0 + f.box.y1) * 0.5, (f.box.z0 + f.box.z1) * 0.5));

        /// <summary>look33: look30's shafts from the plan: a pool's shaft hangs from its ceiling downlight to the floor; an accent's leaves its wall fixture (tipped down).</summary>
        void Beams33(SurvivalMaterials mats, LightPlan33.Plan plan)
        {
            var kits = new Dictionary<Color, MeshKit>();
            foreach (LightPlan33.Fix f in plan.fixes)
            {
                if (f.kind != "beam") continue;
                Color col = Named(game.cfg, f.color ?? "cyan", theme41);
                if (!kits.TryGetValue(col, out MeshKit k)) kits[col] = k = new MeshKit();
                Vector3 top = U33(f.tx, f.ty, f.tz), bottom = U33(f.bx, f.by, f.bz);
                float wTop = (float)f.topW, wBottom = (float)f.bottomW;
                Vector3 axis = (bottom - top).normalized;
                Vector3 s1 = Vector3.Cross(axis, Mathf.Abs(axis.y) > 0.9f ? Vector3.right : Vector3.up).normalized;
                Vector3 s2 = Vector3.Cross(axis, s1).normalized;
                foreach (Vector3 s in new[] { s1, s2 })
                {
                    Vector3 a = top - s * wTop * 0.5f, b = top + s * wTop * 0.5f, c = bottom + s * wBottom * 0.5f, d = bottom - s * wBottom * 0.5f;
                    k.Quad(a, b, c, d, Vector3.up, Color.white);
                    k.Quad(d, c, b, a, Vector3.up, Color.white);
                }
                beams33++;
                beams30++;
            }
            int i = 0;
            foreach (var kv in kits)
            {
                Material m = FxDraw.Additive(mats.neon, "Beams 33 " + i);
                m.SetColor("_BaseColor", kv.Key * UnityTuning.Look30BeamGain);
                MeshKit.Object("Beams 33 " + i, transform, kv.Value.Build("beams33 " + i), m, shadows: false);
                i++;
            }
        }

        /// <summary>A box by its centre, axes (right, up, forward) and half extents; six outward quads.</summary>
        public static void OrientedBox(MeshKit k, Vector3 c, Vector3 right, Vector3 up, Vector3 fwd, float hx, float hy, float hz, Color col)
        {
            Vector3 P(int x, int y, int z) => c + right * (x == 0 ? -hx : hx) + up * (y == 0 ? -hy : hy) + fwd * (z == 0 ? -hz : hz);
            k.Quad(P(0, 1, 0), P(1, 1, 0), P(1, 1, 1), P(0, 1, 1), up, col);
            k.Quad(P(0, 0, 0), P(1, 0, 0), P(1, 0, 1), P(0, 0, 1), -up, col);
            k.Quad(P(0, 0, 0), P(1, 0, 0), P(1, 1, 0), P(0, 1, 0), -fwd, col);
            k.Quad(P(0, 0, 1), P(1, 0, 1), P(1, 1, 1), P(0, 1, 1), fwd, col);
            k.Quad(P(0, 0, 0), P(0, 0, 1), P(0, 1, 1), P(0, 1, 0), -right, col);
            k.Quad(P(1, 0, 0), P(1, 0, 1), P(1, 1, 1), P(1, 1, 0), right, col);
        }

        void Update()
        {
            if (lights == null || lights.Length == 0 || fixtures.Count == 0 || game == null || game.cam == null) return;
            Step54Kit(); // NIGHT54 L light54 (LightKit29.Light54.cs): the twins' fade-out and the landmark's ease (nothing with the row off)
            StepLava47(); // NIGHT47 F lava47: the fixed lava lights' fade and breath (nothing with the row off)
            clock -= FeelClock.UnscaledDelta; // NIGHT63 D1: 0 in a same-run held frame
            if (clock > 0) return;
            clock = UnityTuning.LightKitRepick;
            Step49(); // NIGHT49 L (lights49): the landmark stays the strongest light on itself (nothing with the row off)
            Vector3 p = game.cam.transform.position;
            Campaign24 c = game.sim != null ? game.sim.campaign : null;
            string room = c != null && c.room >= 0 && c.room < level.rooms.Count ? level.rooms[c.room].id : null;
            // DESIGN29 A: one accent light per room and a pool at the goal. Slot 0 = the room's pool nearest its exit (the goal's pool), slot 1 =
            // the room's accent (amber / orange row), the rest the nearest fixtures; a slot with no such row falls back to the nearest.
            Vector3 exit = c != null && c.room >= 0 && c.room < level.rooms.Count ? WebSpace.ToUnity(level.rooms[c.room].exit) : p;
            for (int li = 0; li < lights.Length; li++)
            {
                int best = -1, bestPref = -1;
                float bestD = float.MaxValue;
                for (int i = 0; i < fixtures.Count; i++)
                {
                    Fixture f = fixtures[i];
                    if (room != null && f.room != room) continue; // the room you are in lends its fixtures' light
                    if (lava47 && f.kind == "lava") continue; // NIGHT47 F lava47: a lava fixture has its own fixed light (never a roaming slot)
                    bool taken = false;
                    for (int q = 0; q < li; q++) if (l54 ? Taken54(q, i) : (lights[q].transform.position - f.at).sqrMagnitude < 0.01f) { taken = true; break; } // NIGHT54 light54: the WANTED fixture is taken (not where a fading slot still stands)
                    if (taken) continue;
                    int pref = lavaSlot33 && li == lights.Length - 1 ? (f.kind == "lava" ? 1 : 0) : li == 0 && f.kind == "pool" ? 1 : li == 1 && f.accent ? 1 : 0; // the slot's own kind outranks distance (look33: the last slot is the lava's)
                    float dd = (f.at - (li == 0 && pref == 1 ? exit : p)).sqrMagnitude;
                    if (pref > bestPref || (pref == bestPref && dd < bestD)) { bestPref = pref; bestD = dd; best = i; }
                }
                if (l54 && best >= 0) best = Keep54(li, best, bestPref, bestD, room, p, exit); // NIGHT54 light54: hysteresis (a slot moves only to a clearly closer fixture)
                if (best < 0) { if (lava47) { Want47(li, -1, 0f); continue; } lights[li].intensity = 0; continue; } // NIGHT47 F lava47: faded out, never cut
                Fixture bf = fixtures[best];
                if (lightFix == null || lightFix.Length != lights.Length) lightFix = new int[lights.Length];
                lightFix[li] = best;
                if (lava47) { Want47(li, best, bf.intensity * lightScale * (look30 && li == 1 && bf.accent ? UnityTuning.Look30AccentBoost : 1f)); continue; } // NIGHT47 F lava47: the slot fades to its new fixture (LightKit29.Lava47.cs)
                lights[li].transform.position = bf.at;
                if (lavaSlot33)
                {
                    // look33: a panel's light is a spot down (a pool of light under it, a dark ceiling round it); the rest stay points
                    LightType want = bf.down ? LightType.Spot : LightType.Point;
                    if (lights[li].type != want) lights[li].type = want;
                    if (bf.down) { lights[li].transform.rotation = Quaternion.Euler(90f, 0f, 0f); lights[li].spotAngle = UnityTuning.Look33PanelSpotAngle; lights[li].innerSpotAngle = UnityTuning.Look33PanelSpotAngle * 0.5f; }
                }
                lights[li].color = bf.color;
                lights[li].range = bf.range;
                lights[li].intensity = bf.intensity * lightScale * (look30 && li == 1 && bf.accent ? UnityTuning.Look30AccentBoost : 1f); // NIGHT30 look30: the room's accent reads bolder
                if (baseI33 == null || baseI33.Length != lights.Length) baseI33 = new float[lights.Length];
                baseI33[li] = lights[li].intensity; // NIGHT33 L: Vfx33's flicker multiplies this (it never compounds between repicks)
            }
        }
    }
}
