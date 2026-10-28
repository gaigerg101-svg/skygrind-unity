// NIGHT37 builder S (stag37; NIGHT37.md S1, BOSSES.md "L2 THE STAG"): what the player SEES of THE CANAL and THE STAG.
// The lead adds two lines in SurvivalGame.Boss26.cs:  Boss26InitViews: `Stag37View.Current = new Stag37View(this);`
//                                                      Boss26RenderViews: `Stag37View.Current?.Render(alpha);`
//  - THE CANAL (whenever the level has the stag37 row): the SAME triangles the sim collides with (Stag37Arena): the ice, the bank walls and the
//    island in dusk stone, straight B's floes one by one (a floe about to drop pulses RED; a dropped one is gone: the water under it), the lane
//    lines on the ice (pale dashes), a cyan trim at the bottom and the top of every ride face (the bank walls and the lane walls: cyan =
//    player-usable), the entry slab red-orange while locked, pale point lights round the loop.
// Rule 8 colours (UnityTuning.Stag37).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class Stag37View
    {
        public static Stag37View Current;

        readonly SurvivalGame game;
        readonly Transform root, canalRoot, fxRoot;
        readonly Mesh boxMesh;
        readonly Material add;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        Vec3 builtAt;
        Stag37Arena builtArena; // NIGHT38 S2: stag38 swaps the arena at a run's start (the canal is rebuilt with it)
        bool built;
        GameObject slabGo;
        Renderer slabR;
        readonly List<GameObject> floeGos = new List<GameObject>();
        readonly List<Renderer> floeTellRs = new List<Renderer>();
        float t;

        /// <summary>Frames the reel reads: the canal drawn, the slab shown, floes down / telegraphed now.</summary>
        public bool CanalShown => canalRoot != null && canalRoot.gameObject.activeSelf;
        public bool SlabShown => slabGo != null && slabGo.activeSelf;
        public int FloesDown, FloesTold;

        public Stag37View(SurvivalGame game)
        {
            this.game = game;
            Current = this;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxMesh = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            add = FxDraw.Additive(game.mats.neon, "Stag37 glow");
            root = new GameObject("NIGHT37 THE STAG").transform;
            canalRoot = new GameObject("canal").transform;
            canalRoot.SetParent(root, false);
            canalRoot.gameObject.SetActive(false);
            fxRoot = new GameObject("fight fx").transform;
            fxRoot.SetParent(root, false);
            fxRoot.gameObject.SetActive(false);
            InitFight();
        }

        partial void InitFight();
        partial void RenderFight(SurvivalSim sim, double alpha, float dt);
        partial void ClearFight();

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // web z is Unity -z
        static Vector3 N(Vec3 n) => new Vector3((float)n.x, (float)n.y, (float)-n.z);

        void Tint(Renderer r, Color c)
        {
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        Material Opaque(string name, uint hex)
        {
            var mat = new Material(game.mats.stone) { name = name };
            Color c = Look.Hex(hex);
            if (mat.HasProperty(BaseColor)) mat.SetColor(BaseColor, c);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", null);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", null);
            return Light54.StoneOrLit(game, mat, "Stag37View", name, hex); // NIGHT54 light54: Stone.shader reads the vertex colour, not _BaseColor (white): the intended hex on URP Lit with the row on
        }

        Renderer Glow(Transform parent, string name, Mesh mesh, out Transform tr)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = add;
            r.shadowCastingMode = ShadowCastingMode.Off;
            tr = g.transform;
            return r;
        }

        // --- the canal --------------------------------------------------------------------------------------------------------------------------

        void BuildCanal(Stag37Arena a, Vec3 o)
        {
            foreach (Transform ch in canalRoot) Object.Destroy(ch.gameObject);
            floeGos.Clear();
            floeTellRs.Clear();
            beaconRs38.Clear(); // NIGHT38 S2: the floes' beacons (Stag37View.Read38.cs)
            Stag37Part[] parts = { Stag37Part.Floor, Stag37Part.Wall, Stag37Part.Island, Stag37Part.LaneWall, Stag37Part.Tunnel, Stag37Part.Lintel };
            uint[] hex = { UnityTuning.Canal37IceHex, UnityTuning.Canal37WallHex, UnityTuning.Canal37IslandHex, UnityTuning.Canal37WallHex, UnityTuning.Canal37TunnelHex, UnityTuning.Canal37WallHex };
            var kits = new MeshKit[parts.Length];
            for (int k = 0; k < kits.Length; k++) kits[k] = new MeshKit();
            var slab = new MeshKit();
            foreach (Stag37Tri tr in a.tris)
            {
                Vec3 n = TriangleWorld.TriangleNormal(tr.a, tr.b, tr.c);
                if (tr.part == Stag37Part.Slab) { slab.Triangle(U(tr.a + o), U(tr.b + o), U(tr.c + o), N(n), Color.white); continue; }
                int k = System.Array.IndexOf(parts, tr.part);
                kits[k < 0 ? 1 : k].Triangle(U(tr.a + o), U(tr.b + o), U(tr.c + o), N(n), Color.white);
            }
            for (int k = 0; k < kits.Length; k++)
                if (kits[k].Vertices > 0) MeshKit.Object("canal " + parts[k], canalRoot, kits[k].Build("stag37 " + parts[k]), Opaque("canal " + parts[k], hex[k]), shadows: parts[k] != Stag37Part.Floor);
            // the floes: one object each (a dropped one is hidden), with a RED telegraph plate on top
            Material floeMat = Opaque("canal floe", UnityTuning.Canal37FloeHex);
            for (int i = 0; i < a.floes.Count; i++)
            {
                Stag37Floe f = a.floes[i];
                var fk = new MeshKit();
                Color white = Color.white;
                fk.Box(f.min + o, f.max + o, white, white);
                GameObject go = MeshKit.Object("floe " + i, canalRoot, fk.Build("stag37 floe"), floeMat, shadows: false);
                floeGos.Add(go);
                Renderer tell = Glow(go.transform, "tell", boxMesh, out Transform tt);
                Vec3 c = (f.min + f.max) * 0.5 + o;
                tt.position = U(new Vec3(c.x, f.max.y + o.y + 0.02, c.z));
                tt.localScale = new Vector3((float)(f.max.x - f.min.x) * 0.92f, 0.02f, (float)(f.max.z - f.min.z) * 0.96f);
                tt.gameObject.SetActive(false);
                floeTellRs.Add(tell);
                Beacon38(go, new Vec3(c.x, f.max.y + o.y, c.z)); // NIGHT38 S2 stag38: a red column over a told floe (hidden until then)
            }
            // the lane lines: pale dashes on the lane boundaries round the whole loop
            var lanes = new MeshKit();
            double len = a.Length, dash = UnityTuning.Canal37LaneDash, gap = UnityTuning.Canal37LaneGap, w = UnityTuning.Canal37LaneWidth * 0.5;
            Color lc = Color.white;
            for (int k = 1; k < a.p.lanes; k++)
            {
                double lat = (k - a.p.lanes * 0.5) * a.p.laneWidth;
                for (double s = 0; s < len; s += dash + gap)
                {
                    double s1 = System.Math.Min(len, s + dash);
                    if (a.LaneWallAt(s, 0.5) || a.LaneWallAt(s1, 0.5)) continue; // a lane wall stands there
                    Vec3 p0 = a.PointAt(s, lat - w) + o, p1 = a.PointAt(s1, lat - w) + o, p2 = a.PointAt(s1, lat + w) + o, p3 = a.PointAt(s, lat + w) + o;
                    Vector3 lift = new Vector3(0, UnityTuning.Canal37LaneLift, 0);
                    lanes.Quad(U(p0) + lift, U(p1) + lift, U(p2) + lift, U(p3) + lift, Vector3.up, lc);
                }
            }
            Renderer laneR = MeshKit.Object("canal lane lines", canalRoot, lanes.Build("stag37 lanes"), add, shadows: false).GetComponent<Renderer>();
            Tint(laneR, Look.Hex(UnityTuning.Canal37LaneHex, UnityTuning.Canal37LaneGain));
            // the ride trims: a cyan strip at the bottom and the top of every ride face
            var trim = new MeshKit();
            foreach (var (pa, pb, n, hgt) in a.rideFaces)
            {
                Vec3 off = n * UnityTuning.Canal37TrimStandOff;
                Vec3 a0 = pa + o + off, b0 = pb + o + off;
                foreach (double y in new[] { UnityTuning.Canal37TrimTop, hgt - UnityTuning.Canal37TrimTop - UnityTuning.Canal37TrimHeight })
                {
                    Vec3 lo = new Vec3(0, y, 0), hi = new Vec3(0, y + UnityTuning.Canal37TrimHeight, 0);
                    trim.Quad(U(a0 + lo), U(b0 + lo), U(b0 + hi), U(a0 + hi), N(n), Color.white);
                }
            }
            Renderer trimR = MeshKit.Object("canal ride trim", canalRoot, trim.Build("stag37 trim"), add, shadows: false).GetComponent<Renderer>();
            Tint(trimR, Look.Hex(UnityTuning.Canal37TrimHex, UnityTuning.Canal37TrimGain));
            slabGo = MeshKit.Object("canal slab (locked)", canalRoot, slab.Build("stag37 slab"), add, shadows: false);
            slabR = slabGo.GetComponent<Renderer>();
            Tint(slabR, Look.Hex(UnityTuning.Canal37DangerHex, UnityTuning.Canal37SlabGain));
            slabGo.SetActive(false);
            // pale lights over both straights
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < UnityTuning.Canal37LightsPerStraight; i++)
                {
                    double z = -a.Half + a.p.straight * (i + 0.5) / UnityTuning.Canal37LightsPerStraight;
                    var lg = new GameObject("canal light");
                    lg.transform.SetParent(canalRoot, false);
                    lg.transform.position = U(new Vec3(o.x + side * a.p.turnR, o.y + UnityTuning.Canal37LightY, o.z + z));
                    Light light = lg.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.shadows = LightShadows.None;
                    light.range = UnityTuning.Canal37LightRange;
                    light.intensity = UnityTuning.Canal37LightIntensity * UnityTuning.LightScale;
                    light.color = Look.Hex(UnityTuning.Canal37LightHex);
                }
            builtAt = o;
            builtArena = a;
            built = true;
        }

        // --- the frame --------------------------------------------------------------------------------------------------------------------------

        public void Render(double alpha)
        {
            SurvivalSim sim = game.sim;
            float dt = Time.deltaTime;
            t += dt;
            if (sim == null || sim.stag37Centre == null || sim.stag37Arena == null)
            {
                canalRoot.gameObject.SetActive(false);
                fxRoot.gameObject.SetActive(false);
                ClearFight();
                HideArrow38();
                return;
            }
            Vec3 o = sim.stag37Centre.Value;
            if (!built || builtAt != o || !ReferenceEquals(builtArena, sim.stag37Arena)) BuildCanal(sim.stag37Arena, o);
            canalRoot.gameObject.SetActive(true);
            slabGo.SetActive(sim.Stag37SlabClosed);
            FloesDown = FloesTold = Beacons38 = 0;
            int tellSteps = sim.Stag37FloeTellSteps;
            float warn = Warn38(sim); // NIGHT38 S2 stag38: the RED warnings brighter (1 with the row off)
            for (int i = 0; i < floeGos.Count; i++)
            {
                bool down = sim.stag37FloeDown != null && i < sim.stag37FloeDown.Length && sim.stag37FloeDown[i];
                floeGos[i].SetActive(!down);
                if (down) { FloesDown++; continue; }
                int left = sim.stag37FloeTell != null && i < sim.stag37FloeTell.Length ? sim.stag37FloeTell[i] : 0;
                Renderer r = floeTellRs[i];
                r.gameObject.SetActive(left > 0);
                float prog = tellSteps > 0 ? 1f - left / (float)tellSteps : 1f;
                if (left > 0)
                {
                    FloesTold++;
                    float g = Mathf.Lerp(UnityTuning.Canal37FloeTellMin, UnityTuning.Canal37FloeTellMax, prog) * (0.8f + 0.2f * Mathf.Sin(t * 18f)) * warn;
                    Tint(r, Look.Hex(UnityTuning.Canal37DangerHex, g));
                }
                RenderBeacons38(sim, i, left > 0, prog);
            }
            fxRoot.gameObject.SetActive(true);
            RenderFight(sim, alpha, dt);
            RenderArrow38(sim); // NIGHT38 S2 stag38: THE STAG's off-screen arrow (Stag37View.Read38.cs)
        }
    }
}
