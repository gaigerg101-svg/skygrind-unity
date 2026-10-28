// NIGHT30 builder V (item 3; DESIGN30 B + E; view only: no collider, no sim read changes). What the player SEES of the lava and of two boss tells:
//  - THE LAVA SURFACES (design.n30.lava on): each entry of the level JSON's top-level `lava` array (builder L, LEVEL-FORMAT.md: `{ "rect": [x0, z0, x1,
//    z1], "y": surfaceY, "room": "f2" }`, web x / z) is drawn as a molten surface (Shaders/Resources/Lava30.shader: scrolling world-space noise, crust red
//    through danger red-orange to gold seams) breathing at 0.8 Hz, with a GOLD RIM strip along its four edges. It lights nothing (no point light): the
//    lava glows, the safe surfaces keep their colour. A map without the key draws nothing (older maps unchanged). Heat shimmer: not built (SHORT).
//  - A BURNING ENEMY (event `lavaEnemyBurn`, builder S): a red-orange to gold flare column rises and fades at the body (pooled; no silent vanish).
//  - THE LANTERN's floor (design.n30.look30 or lantern30 on; Boss29View stands its full-floor additive quad and its 30 m red-orange light down: they
//    washed walls, roof and the HUD's backings salmon, shot 170227 / the RED frame's mid-frame luminance 0.347): the live floor is this same lava surface
//    over the arena floor only (the ring's 16-gon with lantern30, the square arena without); in the tell it heats up (a low share of the gain rising) and
//    B's heat VENTS (Lantern30Arena.Vent) glow gold; the OPEN CORE gets a white-gold beam to the roof while the floor is live; the SWEEP's marked panel
//    swells red-orange in its tell (2.5 Hz, under 3 a second) and burns bright.
//  - THE KILN WYRM's head (design.n30.wyrm30): a cyan shell round the head while it can be punched (B's SurvivalSim.Wyrm30Punchable: the 0.4 s window).
// Installs itself after the first scene load (no edit in the lead's SurvivalGame files) and finds the game each frame it has none (a scene reload makes a
// new one: the surfaces rebuild for the new map, the events re-subscribe on the new bus). Off (-skygrindOff30, or the rows off): nothing drawn, NIGHT29's.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The lava view's pure parts (EditMode tests).</summary>
    public static class LavaView30Math
    {
        /// <summary>The surface's breath at time t: 1 +- Lava30PulseDepth at Lava30PulseHz; 1 with reduced motion.</summary>
        public static float Pulse(double t, bool reducedMotion) =>
            reducedMotion ? 1f : 1f + UnityTuning.Lava30PulseDepth * (float)Math.Sin(2 * Math.PI * UnityTuning.Lava30PulseHz * t);

        /// <summary>A lava entry's rect (web x0, z0, x1, z1, min first whatever the order written) and surface y; false when the row is malformed.</summary>
        public static bool Rect(object row, out double x0, out double z0, out double x1, out double z1, out double y)
        {
            x0 = z0 = x1 = z1 = y = 0;
            if (!(row is Dictionary<string, object> d) || !d.TryGetValue("rect", out object ro) || !(ro is List<object> r) || r.Count < 4) return false;
            if (!d.TryGetValue("y", out object yo)) return false;
            double a = Convert.ToDouble(r[0], System.Globalization.CultureInfo.InvariantCulture), b = Convert.ToDouble(r[1], System.Globalization.CultureInfo.InvariantCulture);
            double c = Convert.ToDouble(r[2], System.Globalization.CultureInfo.InvariantCulture), e = Convert.ToDouble(r[3], System.Globalization.CultureInfo.InvariantCulture);
            x0 = Math.Min(a, c); x1 = Math.Max(a, c); z0 = Math.Min(b, e); z1 = Math.Max(b, e);
            y = Convert.ToDouble(yo, System.Globalization.CultureInfo.InvariantCulture);
            return x1 - x0 > 1e-3 && z1 - z0 > 1e-3;
        }

        /// <summary>The Lantern floor's gain share: the tell heats from Lava30TellFrom to Lava30TellTo with its progress, live is whole, rest none.</summary>
        public static float LanternFloorShare(Lantern29Stage stage, double tellProgress)
        {
            if (stage == Lantern29Stage.Live) return 1f;
            if (stage != Lantern29Stage.Tell) return 0f;
            double u = Math.Max(0, Math.Min(1, tellProgress));
            return UnityTuning.Lava30TellFrom + (UnityTuning.Lava30TellTo - UnityTuning.Lava30TellFrom) * (float)u;
        }
    }

    public sealed class LavaView30 : MonoBehaviour
    {
        /// <summary>The Lantern's new floor look applies (NIGHT29's wash stands down): design.n30.look30, or the run's lantern30 block; never with -skygrindOff30.</summary>
        public static bool LanternLookOn(GameConfig cfg, SurvivalSim sim) => N30Fixes.ViewRowOn(cfg, "look30") || (N30Fixes.On && sim != null && sim.n30.lantern30);

        /// <summary>For the reels: lava surfaces built for the map, burn flares started, the Lantern floor's share now, the vents / beam / sweep / wyrm shell drawn.</summary>
        public static int SurfacesBuilt, BurnsShown;
        public static float LanternShareNow;
        public static bool VentsShown, BeamShown, SweepShown, WyrmShellShown;

        SurvivalGame game;
        object builtFor; // the map json the surfaces were built for
        bool builtLavaOn;
        Transform root;
        Material lavaMat, glowMat;
        // NIGHT30 lead fix (round-1 Player reel: 15186 NREs): Unity API calls (new MaterialPropertyBlock, Shader.PropertyToID) are not allowed in a
        // MonoBehaviour's field initializers / constructor; the throw left every later field null. They are made in Awake now.
        MaterialPropertyBlock block;
        static int BaseColor, PulseId, GainId;
        void Awake()
        {
            block = new MaterialPropertyBlock();
            BaseColor = Shader.PropertyToID("_BaseColor"); PulseId = Shader.PropertyToID("_Pulse"); GainId = Shader.PropertyToID("_Gain");
        }
        readonly List<GameObject> surfaces = new List<GameObject>();
        readonly List<Renderer> rims = new List<Renderer>();
        Mesh cube;
        // burns
        readonly Transform[] burnT = new Transform[UnityTuning.Lava30BurnPool];
        readonly Renderer[] burnR = new Renderer[UnityTuning.Lava30BurnPool];
        readonly float[] burnAge = new float[UnityTuning.Lava30BurnPool];
        readonly Vector3[] burnAt = new Vector3[UnityTuning.Lava30BurnPool];
        readonly Queue<Vector3> burnQueue = new Queue<Vector3>();
        Action unBurn;
        // the Lantern
        GameObject lanternFloor;
        Renderer lanternFloorR;
        object lanternFloorFor;
        Transform[] ventT = new Transform[0];
        Renderer[] ventR = new Renderer[0];
        Transform beamT, sweepT, wyrmT;
        Renderer beamR, sweepR, wyrmR;
        Mesh sweepMesh;
        int sweepPanelBuilt = -1;
        Color hot, gold;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("LavaView30") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<LavaView30>();
        }

        void OnDestroy() => unBurn?.Invoke();

        bool Attach()
        {
            if (game != null) return true;
            game = FindFirstObjectByType<SurvivalGame>();
            if (game == null || game.cfg == null || game.mats == null || game.events == null) { game = null; return false; }
            unBurn?.Invoke(); // the old bus (a reloaded scene's game is a new one)
            Clear();
            if (root != null) Destroy(root.gameObject); // every part of the last game's view goes with it
            if (lavaMat != null) Destroy(lavaMat);
            if (glowMat != null) Destroy(glowMat);
            root = new GameObject("Lava 30").transform;
            DontDestroyOnLoad(root.gameObject);
            sweepMesh = null;
            sweepPanelBuilt = -1;
            burnQueue.Clear();
            hot = Look.Hex(UnityTuning.Lava30HotHex);
            gold = Look.Hex(UnityTuning.Lava30GoldHex);
            Shader sh = Shader.Find(UnityTuning.Lava30Shader);
            if (sh != null)
            {
                lavaMat = new Material(sh) { name = "Lava30" };
                lavaMat.SetColor("_Hot", hot);
                lavaMat.SetColor("_Gold", gold);
                lavaMat.SetColor("_Crust", Look.Hex(UnityTuning.Lava30CrustHex));
                lavaMat.SetFloat(GainId, UnityTuning.Lava30Gain * Look33Gain()); // NIGHT33 L: look33 dims the surface under the bloom threshold (the lead's n33-r5: lava 56-61 % of the saturated pixels)
            }
            else lavaMat = FxDraw.Opaque(game.mats.neon, "Lava30 (no shader)", hot); // the shader missing from the build: a plain danger red-orange still reads as lava
            glowMat = FxDraw.Additive(game.mats.neon, "Lava30 glow");
            if (cube == null)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube = c.GetComponent<MeshFilter>().sharedMesh;
                Destroy(c);
            }
            for (int i = 0; i < burnT.Length; i++) { burnR[i] = Part("burn " + i, out burnT[i]); burnAge[i] = -1; }
            beamR = Part("core beam", out beamT);
            wyrmR = Part("wyrm head shell", out wyrmT);
            var sg = new GameObject("sweep panel");
            sg.transform.SetParent(root, false);
            sg.AddComponent<MeshFilter>();
            sweepR = sg.AddComponent<MeshRenderer>();
            sweepR.sharedMaterial = glowMat;
            sweepR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            sweepT = sg.transform;
            sg.SetActive(false);
            unBurn = game.events.On("lavaEnemyBurn", o => { if (TryPosition(o, out Vec3 p)) burnQueue.Enqueue(WebSpace.ToUnity(p)); });
            return true;
        }

        /// <summary>The event's position: a Vec3, an Enemy, or any payload with a `position` field / property (builder S's shape is read, not assumed).</summary>
        static bool TryPosition(object o, out Vec3 p)
        {
            p = default;
            if (o == null) return false;
            if (o is Vec3 v) { p = v; return true; }
            Type t = o.GetType();
            var f = t.GetField("position");
            if (f != null && f.GetValue(o) is Vec3 fv) { p = fv; return true; }
            var pr = t.GetProperty("position") ?? t.GetProperty("Position");
            if (pr != null && pr.GetValue(o) is Vec3 pv) { p = pv; return true; }
            var ef = t.GetField("enemy");
            object e = ef != null ? ef.GetValue(o) : null;
            if (e != null && e != o) return TryPosition(e, out p);
            return false;
        }

        MeshRenderer Part(string name, out Transform t)
        {
            var g = new GameObject(name);
            g.transform.SetParent(root, false);
            g.AddComponent<MeshFilter>().sharedMesh = cube;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = glowMat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            g.SetActive(false);
            t = g.transform;
            return r;
        }

        void Tint(Renderer r, Color c)
        {
            block.Clear();
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        void Clear()
        {
            foreach (GameObject g in surfaces) if (g != null) Destroy(g);
            surfaces.Clear();
            rims.Clear();
            if (lanternFloor != null) Destroy(lanternFloor);
            lanternFloor = null;
            lanternFloorFor = null;
            foreach (Transform t in ventT) if (t != null) Destroy(t.gameObject);
            ventT = new Transform[0];
            ventR = new Renderer[0];
            builtFor = null;
            SurfacesBuilt = 0;
        }

        /// <summary>NIGHT33 L (design.n33.look33): the lava surface's and rim's gain scale (1 with the row off: NIGHT32's lava). The Lantern floor and burns keep theirs.</summary>
        float Look33Gain() => game != null && N33Fixes.ViewRowOn(game.cfg, "look33") ? UnityTuning.Look33LavaGainScale : 1f;

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (game == null) return; // destroyed with its scene: found again next frame
            GameConfig cfg = game.cfg;
            SurvivalSim sim = game.sim;
            bool reduced = game.settings != null && game.settings.View.reducedMotion;
            float pulse = Lava47.On(cfg) ? Lava47.Breath(Time.time, reduced) : LavaView30Math.Pulse(Time.time, reduced); // NIGHT47 F lava47: the one breath (LightKit29.Lava47.cs)
            bool lavaOn = N30Fixes.On && sim != null && sim.n30.lava; // the run's own lava block (its tape's), so a replay draws what it burns
            object mapJson = game.map != null ? game.map.json : null;
            if (!ReferenceEquals(mapJson, builtFor) || lavaOn != builtLavaOn) BuildSurfaces(mapJson, lavaOn);
            if (lavaMat != null && lavaMat.HasProperty(PulseId)) lavaMat.SetFloat(PulseId, pulse);
            foreach (Renderer r in rims) if (r != null) Tint(r, gold * (UnityTuning.Lava30RimGain * Look33Gain() * pulse));
            Burns();
            RenderLantern(sim, cfg, pulse);
            RenderWyrm(sim);
        }

        // ------------------------------------------------------------- the surfaces -------------------------------------------------------------

        void BuildSurfaces(object mapJson, bool lavaOn)
        {
            foreach (GameObject g in surfaces) if (g != null) Destroy(g);
            surfaces.Clear();
            rims.Clear();
            builtFor = mapJson;
            builtLavaOn = lavaOn;
            SurfacesBuilt = 0;
            if (!lavaOn || !(mapJson is Dictionary<string, object> map) || !map.TryGetValue("lava", out object lo) || !(lo is List<object> rows)) return;
            float rw = UnityTuning.Lava30RimWidth;
            foreach (object row in rows)
            {
                if (!LavaView30Math.Rect(row, out double x0, out double z0, out double x1, out double z1, out double y)) continue;
                float lift54 = Light54.Lift(UnityTuning.Lava30Lift) - UnityTuning.Lava30Lift; // NIGHT54 light54: the surface and its rim raised together to zLiftM (0 off)
                float yy = (float)y + UnityTuning.Lava30Lift + lift54, yr = (float)y + UnityTuning.Lava30RimLift + lift54;
                // web (x, z) -> Unity (x, -z)
                Vector3 a = new Vector3((float)x0, yy, (float)-z0), b = new Vector3((float)x1, yy, (float)-z0), c = new Vector3((float)x1, yy, (float)-z1), d = new Vector3((float)x0, yy, (float)-z1);
                var k = new MeshKit();
                k.Quad(a, b, c, d, Vector3.up, Color.white);
                k.Quad(d, c, b, a, Vector3.up, Color.white); // both windings (Cull Off in the shader; the fallback material culls)
                Mesh m = k.Build("lava " + SurfacesBuilt);
                surfaces.Add(MeshKit.Object("lava " + SurfacesBuilt, root, m, lavaMat, shadows: false));
                // the gold rim: four strips inside the rect's edges, on top of the surface
                var rk = new MeshKit();
                float ax = (float)x0, bx = (float)x1, az = (float)-z0, bz = (float)-z1; // bz < az in Unity z
                RimStrip(rk, new Vector3(ax, yr, az), new Vector3(bx, yr, az), new Vector3(bx, yr, az - rw), new Vector3(ax, yr, az - rw));
                RimStrip(rk, new Vector3(ax, yr, bz + rw), new Vector3(bx, yr, bz + rw), new Vector3(bx, yr, bz), new Vector3(ax, yr, bz));
                RimStrip(rk, new Vector3(ax, yr, az), new Vector3(ax + rw, yr, az), new Vector3(ax + rw, yr, bz), new Vector3(ax, yr, bz));
                RimStrip(rk, new Vector3(bx - rw, yr, az), new Vector3(bx, yr, az), new Vector3(bx, yr, bz), new Vector3(bx - rw, yr, bz));
                GameObject rim = MeshKit.Object("lava rim " + SurfacesBuilt, root, rk.Build("lava rim " + SurfacesBuilt), glowMat, shadows: false);
                surfaces.Add(rim);
                rims.Add(rim.GetComponent<Renderer>());
                SurfacesBuilt++;
            }
        }

        static void RimStrip(MeshKit k, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            k.Quad(a, b, c, d, Vector3.up, Color.white);
            k.Quad(d, c, b, a, Vector3.up, Color.white);
        }

        // ------------------------------------------------------------- burning enemies -------------------------------------------------------------

        void Burns()
        {
            while (burnQueue.Count > 0)
            {
                Vector3 at = burnQueue.Dequeue();
                int slot = 0;
                float oldest = -2;
                for (int i = 0; i < burnAge.Length; i++)
                {
                    if (burnAge[i] < 0) { slot = i; oldest = -1; break; }
                    if (burnAge[i] > oldest) { oldest = burnAge[i]; slot = i; }
                }
                burnAge[slot] = 0;
                burnAt[slot] = at;
                BurnsShown++;
            }
            float dt = Time.deltaTime;
            for (int i = 0; i < burnAge.Length; i++)
            {
                if (burnT[i] == null) continue;
                bool on = burnAge[i] >= 0 && burnAge[i] < UnityTuning.Lava30BurnSeconds;
                if (burnT[i].gameObject.activeSelf != on) burnT[i].gameObject.SetActive(on);
                if (!on) { burnAge[i] = -1; continue; }
                float u = burnAge[i] / UnityTuning.Lava30BurnSeconds;
                float w = UnityTuning.Lava30BurnWidth * (1f - 0.6f * u), h = UnityTuning.Lava30BurnHeight * (1f - 0.3f * u);
                burnT[i].position = burnAt[i] + Vector3.up * (h * 0.5f + UnityTuning.Lava30BurnRise * u);
                burnT[i].localScale = new Vector3(w, h, w);
                Tint(burnR[i], Color.Lerp(hot, gold, u) * (1.6f * (1f - u)));
                burnAge[i] += dt;
            }
        }

        // ------------------------------------------------------------- THE LANTERN -------------------------------------------------------------

        void RenderLantern(SurvivalSim sim, GameConfig cfg, float pulse)
        {
            bool on = sim != null && LanternLookOn(cfg, sim) && sim.n29.boss1 && sim.lantern29 != null && sim.lantern27 != null && sim.lantern27.Alive;
            Lantern29 l29 = on ? sim.lantern29 : null;
            float share = on ? LavaView30Math.LanternFloorShare(l29.Stage, l29.TellProgress) : 0f;
            LanternShareNow = share;
            Lantern30 l30 = on && sim.Lantern30On ? sim.lantern30 : null;
            // the floor
            if (on && share > 0f)
            {
                object key = (object)l30?.arena ?? sim.lantern27;
                if (lanternFloor == null || !ReferenceEquals(key, lanternFloorFor)) BuildLanternFloor(sim, l30);
                if (lanternFloor != null)
                {
                    if (!lanternFloor.activeSelf) lanternFloor.SetActive(true);
                    block.Clear();
                    block.SetFloat(GainId, UnityTuning.Lava30Gain * share);
                    block.SetFloat(PulseId, pulse);
                    block.SetColor(BaseColor, hot * share); // the fallback material reads _BaseColor
                    lanternFloorR.SetPropertyBlock(block);
                }
            }
            else if (lanternFloor != null && lanternFloor.activeSelf) lanternFloor.SetActive(false);

            // B's Boss30View draws the vents, the core's beam and the sweep (and the Wyrm's shell) since round 1: these stand down while it exists (no doubles)
            bool marks = Boss30View.Current == null;
            // the vents glow in the tell (B's Lantern30.VentsGlow), before the floor goes live
            bool vents = marks && l30 != null && l30.VentsGlow;
            VentsShown = vents;
            if (vents && ventT.Length != l30.arena.p.ventPanels.Length) BuildVents(l30);
            for (int k = 0; k < ventT.Length; k++)
            {
                if (ventT[k] == null) continue;
                if (ventT[k].gameObject.activeSelf != vents) ventT[k].gameObject.SetActive(vents);
                if (!vents) continue;
                Vec3 local = l30.arena.Vent(k);
                Vec3 w = l30.inner.OriginWorld + local;
                float colH = UnityTuning.Lava30VentColumn;
                ventT[k].position = WebSpace.ToUnity(new Vec3(w.x, w.y + colH * 0.5, w.z));
                ventT[k].localScale = new Vector3(UnityTuning.Lava30VentSize, colH, UnityTuning.Lava30VentSize);
                Tint(ventR[k], gold * (UnityTuning.Lava30VentGain * (0.35f + 0.65f * (float)l29.TellProgress) * pulse));
            }

            // the open core's beam while the floor is live
            bool beam = marks && on && l29.CoreExposed;
            BeamShown = beam;
            if (beamT.gameObject.activeSelf != beam) beamT.gameObject.SetActive(beam);
            if (beam)
            {
                Vec3 c = sim.lantern27.Centre;
                double roof = l30 != null ? l30.inner.OriginWorld.y + l30.arena.p.height : sim.lantern27.OriginWorld.y + sim.lantern27.p.arenaHeight;
                double h = Math.Max(1, roof - c.y);
                beamT.position = WebSpace.ToUnity(new Vec3(c.x, c.y + h * 0.5, c.z));
                beamT.localScale = new Vector3(UnityTuning.Lava30BeamWidth, (float)h, UnityTuning.Lava30BeamWidth);
                Tint(beamR, Color.Lerp(Color.white, gold, 0.35f) * (UnityTuning.Lava30BeamGain * pulse));
            }

            // the sweep's marked panel
            bool sweep = marks && l30 != null && l30.SweepPanel >= 0 && (l30.SweepTelling || l30.SweepBurning);
            SweepShown = sweep;
            if (sweepT.gameObject.activeSelf != sweep) sweepT.gameObject.SetActive(sweep);
            if (sweep)
            {
                if (l30.SweepPanel != sweepPanelBuilt) BuildSweep(l30);
                float g = l30.SweepBurning ? UnityTuning.Lava30SweepBurnGain
                    : UnityTuning.Lava30SweepTellGain * (0.55f + 0.45f * Mathf.Sin(Time.time * 2f * Mathf.PI * UnityTuning.Lava30SweepTellHz)) * (0.5f + 0.5f * (float)l30.SweepTellProgress);
                Tint(sweepR, hot * g);
            }
        }

        void BuildLanternFloor(SurvivalSim sim, Lantern30 l30)
        {
            if (lanternFloor != null) Destroy(lanternFloor);
            var k = new MeshKit();
            if (l30 != null)
            {
                Lantern30Arena a = l30.arena;
                Vec3 o = l30.inner.OriginWorld;
                double r = a.FacetCornerRadius, y = o.y + UnityTuning.Lava30LanternLift; // NIGHT32 A: the round ring's facets (the panels when not round)
                var centre = WebSpace.ToUnity(new Vec3(o.x, y, o.z));
                for (int i = 0; i < a.Facets; i++)
                {
                    double s0 = a.FacetSeamAngle(i), s1 = a.FacetSeamAngle(i + 1);
                    var p0 = WebSpace.ToUnity(new Vec3(o.x + Math.Cos(s0) * r, y, o.z + Math.Sin(s0) * r));
                    var p1 = WebSpace.ToUnity(new Vec3(o.x + Math.Cos(s1) * r, y, o.z + Math.Sin(s1) * r));
                    k.Triangle(centre, p0, p1, Vector3.up, Color.white);
                    k.Triangle(centre, p1, p0, Vector3.up, Color.white);
                }
                lanternFloorFor = a;
            }
            else
            {
                Vec3 o = sim.lantern27Origin;
                float half = (float)sim.lantern27.p.arenaHalf, y = (float)o.y + UnityTuning.Lava30LanternLift;
                Vector3 c = WebSpace.ToUnity(new Vec3(o.x, y, o.z));
                Vector3 a0 = c + new Vector3(-half, 0, -half), a1 = c + new Vector3(half, 0, -half), a2 = c + new Vector3(half, 0, half), a3 = c + new Vector3(-half, 0, half);
                k.Quad(a0, a1, a2, a3, Vector3.up, Color.white);
                k.Quad(a3, a2, a1, a0, Vector3.up, Color.white);
                lanternFloorFor = sim.lantern27;
            }
            lanternFloor = MeshKit.Object("lantern lava floor", root, k.Build("lantern lava floor"), lavaMat, shadows: false);
            lanternFloorR = lanternFloor.GetComponent<Renderer>();
        }

        void BuildVents(Lantern30 l30)
        {
            foreach (Transform t in ventT) if (t != null) Destroy(t.gameObject);
            int n = l30.arena.p.ventPanels.Length;
            ventT = new Transform[n];
            ventR = new Renderer[n];
            for (int k = 0; k < n; k++) ventR[k] = Part("vent " + k, out ventT[k]);
        }

        void BuildSweep(Lantern30 l30)
        {
            sweepPanelBuilt = l30.SweepPanel;
            Lantern30Arena a = l30.arena;
            Vec3 o = l30.inner.OriginWorld;
            double r = a.CornerRadius - UnityTuning.Lava30SweepInset;
            double s0 = a.SeamAngle(sweepPanelBuilt), s1 = a.SeamAngle(sweepPanelBuilt + 1);
            double y0 = o.y + a.p.laneLow, y1 = o.y + a.p.laneHigh;
            Vector3 P(double s, double y) => WebSpace.ToUnity(new Vec3(o.x + Math.Cos(s) * r, y, o.z + Math.Sin(s) * r));
            var k = new MeshKit();
            if (a.Round)
            {
                // NIGHT32 A (arenas32): the band follows the round wall's facets, UnityTuning.Lava30SweepInset in from it
                List<double> br = a.ArcBreaks(s0, s1);
                double apo = a.p.apothem - UnityTuning.Lava30SweepInset;
                Vector3 Q(double s, double y) { Vec3 v = a.OnRing(s, apo, y); return WebSpace.ToUnity(new Vec3(o.x + v.x, v.y, o.z + v.z)); }
                for (int s = 0; s + 1 < br.Count; s++)
                {
                    k.Quad(Q(br[s], y0), Q(br[s + 1], y0), Q(br[s + 1], y1), Q(br[s], y1), Vector3.up, Color.white);
                    k.Quad(Q(br[s], y1), Q(br[s + 1], y1), Q(br[s + 1], y0), Q(br[s], y0), Vector3.up, Color.white);
                }
            }
            else
            {
            k.Quad(P(s0, y0), P(s1, y0), P(s1, y1), P(s0, y1), Vector3.up, Color.white);
            k.Quad(P(s0, y1), P(s1, y1), P(s1, y0), P(s0, y0), Vector3.up, Color.white);
            }
            sweepMesh = k.Build("sweep panel", sweepMesh);
            sweepT.GetComponent<MeshFilter>().sharedMesh = sweepMesh;
        }

        // ------------------------------------------------------------- THE KILN WYRM -------------------------------------------------------------

        void RenderWyrm(SurvivalSim sim)
        {
            bool on = Boss30View.Current == null && sim != null && N30Fixes.On && sim.n30.wyrm30 && sim.wyrm24 != null && sim.wyrm24.Alive && sim.Wyrm30Punchable; // B's view draws it when it exists
            WyrmShellShown = on;
            if (wyrmT.gameObject.activeSelf != on) wyrmT.gameObject.SetActive(on);
            if (!on) return;
            WyrmParams p = sim.wyrm24.p;
            float s = (float)Math.Max(p.headW, Math.Max(p.headH, p.headLen)) * UnityTuning.Lava30WyrmGlowScale;
            wyrmT.position = WebSpace.ToUnity(sim.wyrm24.HeadPosition);
            wyrmT.localScale = new Vector3(s, s, s);
            Tint(wyrmR, Look.C(game.cfg, "colors.usable") * UnityTuning.Lava30WyrmGlowGain);
        }
    }
}
