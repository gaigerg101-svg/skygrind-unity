// NIGHT31 builder B: the NIGHT31 boss rules as the view draws them. Not compiled in the Editor by its author (dn-check only); the lead adds two lines in SurvivalGame.Boss26.cs:
// Boss26InitViews: `Boss31View.Current = new Boss31View(this);`   Boss26RenderViews: `Boss31View.Current?.Render(alpha);`
//  - THE STAG (design.n31.stag31): blocky boxes in the web's style (the body box the sim hits, a head box, four legs), two cyan ANTLER CRYSTALS while intact, the cyan OPEN BACK
//    (a plate on the rear while a punch lands in full), the red-orange CHARGE LINE on the ice through the tell (brightening), a red-orange stomp ring, the red-orange shards, and after the
//    melt a red-orange film over every bank. The rink itself is RinkView28's (the lead points it at sim.stag31Rink: see the report).
// Rule 8: cyan player-usable, red-orange danger only, the body pale ice-grey. Numbers: UnityTuning.Boss31.cs.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class Boss31View
    {
        readonly SurvivalGame game;
        readonly Transform root;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Mesh boxMesh;
        readonly Material add, solid;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        /// <summary>The live view (the game's Boss26InitViews makes it; the reel reads it).</summary>
        public static Boss31View Current;

        // the Stag
        Transform stagRoot, bodyT, headT, neckT, backT, lineT, stompT, waterT;
        readonly Transform[] legT = new Transform[4], crystalT = new Transform[2];
        Renderer bodyR, headR, neckR, backR, lineR, stompR, waterR;
        readonly Renderer[] legR = new Renderer[4], crystalR = new Renderer[2];
        readonly List<Transform> shardT = new List<Transform>();
        readonly List<Renderer> shardR = new List<Renderer>();
        float stompLeft;
        Vec3 stompAt;
        bool waterBuilt;

        /// <summary>What is drawn now (the reel counts frames of each).</summary>
        public bool StagShown => stagRoot != null && stagRoot.gameObject.activeSelf;
        public bool StagLineShown => lineT != null && lineT.gameObject.activeSelf;
        public bool StagBackShown => backT != null && backT.gameObject.activeSelf;
        public int StagCrystalsShown { get; private set; }
        public bool StagWaterShown => waterT != null && waterT.gameObject.activeSelf;
        public int stomps;

        public Boss31View(SurvivalGame game)
        {
            this.game = game;
            Current = this;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxMesh = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            add = FxDraw.Additive(game.mats.neon, "Boss31 glow");
            solid = FxDraw.Opaque(game.mats.neon, "Boss31 body", Color.white);
            root = new GameObject("NIGHT31 BOSSES").transform;
            game.events.On<Stag31Event>("stag31", e =>
            {
                if (e.stage != "stomp") return;
                stomps++;
                stompAt = e.at;
                stompLeft = UnityTuning.Stag31StompLife;
            });
        }

        Renderer Part(Transform parent, string name, out Transform t, bool glow)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = boxMesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = glow ? add : solid;
            r.shadowCastingMode = glow ? ShadowCastingMode.Off : ShadowCastingMode.On;
            t = g.transform;
            return r;
        }

        void Tint(Renderer r, Color c)
        {
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // web z is Unity -z

        static Quaternion Face(Vec3 f) => Quaternion.LookRotation(new Vector3((float)f.x, 0, (float)-f.z).normalized, Vector3.up);

        static void Beam(Transform t, Vector3 a, Vector3 b, float thick)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            t.SetPositionAndRotation((a + b) * 0.5f, len > 1e-5f ? Quaternion.LookRotation(d / len, Mathf.Abs(d.y) > 0.99f * len ? Vector3.forward : Vector3.up) : Quaternion.identity);
            t.localScale = new Vector3(thick, thick, len);
        }

        public void Render(double alpha)
        {
            SurvivalSim sim = game.sim;
            RenderStag(sim, (float)alpha);
            RenderCrumble(sim);
            RenderLanternPlatforms(sim);
            RenderLoom31(sim);
            BossMarks32.Draw(game, boxMesh, add); // NIGHT32 B (view row design.n32.bossMarks): every boss's damage spot and open window
        }

        // --- THE LOOM rebuilt (design.n31.loom31): the deck's middle cracks red-orange through the break tell and then reads as lava; the flyer drop's red-orange mark on the
        // deck; a red-orange shell on every joint through the shake tell (G's / L's r2 notes: the breakable middle had no tell) --------------------------------------------
        Transform loomFxRoot, breakT, dropT;
        Renderer breakR, dropR;
        readonly Transform[] shakeT = new Transform[8];
        readonly Renderer[] shakeR = new Renderer[8];
        /// <summary>What is drawn now (the reel counts frames).</summary>
        public bool LoomBreakShown => breakT != null && breakT.gameObject.activeSelf;
        public bool LoomDropShown => dropT != null && dropT.gameObject.activeSelf;
        public bool LoomShakeShown { get; private set; }

        void RenderLoom31(SurvivalSim sim)
        {
            Loom30 l = sim?.loom30;
            Loom31 r = sim?.loom31;
            bool on = l != null && l.Alive && r != null && sim.n31.loom31;
            if (!on && loomFxRoot == null) return;
            if (loomFxRoot == null)
            {
                loomFxRoot = new GameObject("LOOM 31").transform;
                loomFxRoot.SetParent(root, false);
                breakR = Part(loomFxRoot, "loom deck break", out breakT, true);
                dropR = Part(loomFxRoot, "loom flyer drop mark", out dropT, true);
                var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                dropT.GetComponent<MeshFilter>().sharedMesh = cyl.GetComponent<MeshFilter>().sharedMesh;
                Object.Destroy(cyl);
                for (int i = 0; i < 8; i++) shakeR[i] = Part(loomFxRoot, "loom shake " + i, out shakeT[i], true);
            }
            if (loomFxRoot.gameObject.activeSelf != on) loomFxRoot.gameObject.SetActive(on);
            LoomShakeShown = false;
            if (!on) return;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * UnityTuning.Wyrm31CrackRate * 2 * Mathf.PI);
            bool brk = r.breakState > 0 && r.rect != null && r.rect.Length == 4;
            if (breakT.gameObject.activeSelf != brk) breakT.gameObject.SetActive(brk);
            if (brk)
            {
                double y = l.centre.y + UnityTuning.Loom31BreakLift;
                Vector3 a = U(new Vec3(r.rect[0], y, r.rect[1])), b = U(new Vec3(r.rect[2], y, r.rect[3]));
                breakT.SetPositionAndRotation((a + b) * 0.5f, Quaternion.identity);
                breakT.localScale = new Vector3(Mathf.Abs(b.x - a.x), 0.04f, Mathf.Abs(b.z - a.z));
                float g = r.breakState == 1 ? Mathf.Lerp(UnityTuning.Wyrm31CrackMin, UnityTuning.Wyrm31CrackMax, (float)r.BreakTellProgress) * (0.6f + 0.4f * pulse) : UnityTuning.Loom31LavaGain;
                Tint(breakR, Look.Hex(UnityTuning.Stag31DangerHex, g));
            }
            bool drop = r.DropTelling;
            if (dropT.gameObject.activeSelf != drop) dropT.gameObject.SetActive(drop);
            if (drop)
            {
                float s = (float)r.p.dropRadius * 2;
                dropT.SetPositionAndRotation(U(new Vec3(r.dropAt.x, r.dropAt.y + UnityTuning.Loom31BreakLift, r.dropAt.z)), Quaternion.identity);
                dropT.localScale = new Vector3(s, 0.03f, s);
                Tint(dropR, Look.Hex(UnityTuning.Stag31DangerHex, Mathf.Lerp(UnityTuning.Wyrm31CrackMin, UnityTuning.Wyrm31CrackMax, (float)r.DropTellProgress)));
            }
            bool shake = r.ShakeTelling;
            for (int i = 0; i < 8; i++)
            {
                if (shakeT[i].gameObject.activeSelf != shake) shakeT[i].gameObject.SetActive(shake);
                if (!shake) continue;
                float s = (float)l.p.jointRadius * 2 * UnityTuning.Loom31ShakeScale;
                shakeT[i].SetPositionAndRotation(U(l.joints[i]) + Random.insideUnitSphere * UnityTuning.Loom31ShakeJitter, Quaternion.identity);
                shakeT[i].localScale = new Vector3(s, s, s);
                Tint(shakeR[i], Look.Hex(UnityTuning.Stag31DangerHex, 1.4f + pulse));
            }
            LoomShakeShown = shake;
        }

        // --- THE LANTERN's platforms with the row on (design.n31.lantern31; Boss30View hides its static ones): pale tops with a cyan edge (a refuge, player-usable), a red-orange
        // edge pulsing through the sink tell, the platforms moving down as the sim's SinkTop does; the cyan edge brightens while the core is open ---------------------------
        Transform platRoot, platTopsT, platSidesT, platEdgeT;
        MeshFilter platTopsF, platSidesF, platEdgeF;
        Renderer platTopsR, platSidesR, platEdgeR;
        double platBuiltTop = double.NaN;
        /// <summary>The platforms are drawn now / their edge is red-orange now (the reel counts frames).</summary>
        public bool PlatformsShown => platRoot != null && platRoot.gameObject.activeSelf;
        public bool PlatformSinkTellShown { get; private set; }

        MeshFilter PlatPart(string name, out Transform t, out Renderer r, bool glow)
        {
            var g = new GameObject(name);
            g.transform.SetParent(platRoot, false);
            var f = g.AddComponent<MeshFilter>();
            r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = glow ? add : solid;
            r.shadowCastingMode = glow ? ShadowCastingMode.Off : ShadowCastingMode.On;
            t = g.transform;
            return f;
        }

        void RenderLanternPlatforms(SurvivalSim sim)
        {
            bool on = sim != null && sim.Lantern31PlatformsMoved && sim.lantern27 != null && sim.lantern27.Alive && sim.lantern31 != null && sim.lantern31.SinkTop > 0.01;
            if (!on && platRoot == null) return;
            if (platRoot == null)
            {
                platRoot = new GameObject("LANTERN PLATFORMS 31").transform;
                platRoot.SetParent(root, false);
                platTopsF = PlatPart("platform tops", out platTopsT, out platTopsR, false);
                platSidesF = PlatPart("platform sides", out platSidesT, out platSidesR, false);
                platEdgeF = PlatPart("platform edges", out platEdgeT, out platEdgeR, true);
            }
            if (platRoot.gameObject.activeSelf != on) platRoot.gameObject.SetActive(on);
            PlatformSinkTellShown = false;
            if (!on) return;
            double top = sim.lantern31.SinkTop;
            if (top != platBuiltTop)
            {
                // the same triangles the sim collides with: a Lantern30Arena at this top, its platform parts only
                Lantern30ArenaParams prm = Lantern30ArenaParams.From(sim.cfg, sim.n32.arenas32); // NIGHT32 A: the run's ring (round arc platforms while arenas32 is on)
                prm.platformTop = top;
                var a = new Lantern30Arena(prm);
                Vec3 o = sim.lantern27Origin;
                var tops = new MeshKit();
                var sides = new MeshKit();
                var edges = new MeshKit();
                foreach (Lantern30Tri t in a.tris)
                {
                    if (t.part != Lantern30Part.PlatformTop && t.part != Lantern30Part.PlatformSide) continue;
                    Vec3 n = TriangleWorld.TriangleNormal(t.a, t.b, t.c);
                    Vector3 un = new Vector3((float)n.x, (float)n.y, (float)-n.z);
                    (t.part == Lantern30Part.PlatformTop ? tops : sides).Triangle(U(t.a + o), U(t.b + o), U(t.c + o), un, Color.white);
                    if (t.part == Lantern30Part.PlatformSide)
                    {
                        // a thin glowing band along the top of each side face: the platform's edge
                        Vec3 lift = new Vec3(0, Light54.Lift(UnityTuning.Lantern31EdgeLift), 0); // NIGHT54 light54: zLiftM (off: 10 mm)
                        double yTop = top;
                        Vec3 pa = t.a, pb = t.b, pc = t.c;
                        Vec3[] up = System.Array.FindAll(new[] { pa, pb, pc }, v => System.Math.Abs(v.y - yTop) < 1e-6);
                        if (up.Length == 2)
                        {
                            Vec3 d = new Vec3(0, UnityTuning.Lantern31EdgeHeight, 0), out1 = n * UnityTuning.Lantern31EdgeStandOff;
                            edges.Quad(U(up[0] + o + out1 - d + lift), U(up[1] + o + out1 - d + lift), U(up[1] + o + out1 + lift), U(up[0] + o + out1 + lift), un, Color.white);
                        }
                    }
                }
                platTopsF.sharedMesh = tops.Build("lantern31 platform tops", platTopsF.sharedMesh);
                platSidesF.sharedMesh = sides.Build("lantern31 platform sides", platSidesF.sharedMesh);
                platEdgeF.sharedMesh = edges.Build("lantern31 platform edges", platEdgeF.sharedMesh);
                platBuiltTop = top;
            }
            Tint(platTopsR, Look.Hex(UnityTuning.Lantern31TopHex));
            Tint(platSidesR, Look.Hex(UnityTuning.Lantern31SideHex));
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * UnityTuning.Wyrm31CrackRate * 2 * Mathf.PI);
            Lantern31 l = sim.lantern31;
            if (l.sinkState == 1 || l.sinkState == 2)
            {
                PlatformSinkTellShown = true;
                Tint(platEdgeR, Look.Hex(UnityTuning.Stag31DangerHex, Mathf.Lerp(UnityTuning.Wyrm31CrackMin, UnityTuning.Wyrm31CrackMax, l.sinkState == 1 ? (float)l.SinkTellProgress : 1) * (0.6f + 0.4f * pulse)));
            }
            else Tint(platEdgeR, Look.Hex(UnityTuning.Stag31CyanHex, l.CoreOpen ? UnityTuning.Lantern31EdgeOpenGain : UnityTuning.Lantern31EdgeGain));
        }

        // --- THE KILN WYRM's crumbling platforms (design.n31.wyrm31, builder L's k5.crumble31): the boxes WyrmRoomView leaves out with the row on are drawn here as stone,
        // red-orange cracks pulse over them through the 1 s tell, and they vanish when they fall -------------------------------------------------------------------------
        Transform crumbleRoot;
        readonly List<Transform> crumbleT = new List<Transform>(), crackT = new List<Transform>();
        readonly List<Renderer> crumbleR = new List<Renderer>(), crackR = new List<Renderer>();
        /// <summary>Crumble boxes drawn now / cracks drawn now (the reel counts them).</summary>
        public int CrumbleBoxesShown { get; private set; }
        public bool CracksShown { get; private set; }

        void RenderCrumble(SurvivalSim sim)
        {
            // NIGHT46 V1 (B1's walkin46 hand-off): the crumble draws from the door too, while the worm's arena is the level's own k5 (sim.Wyrm46ArenaHere)
            bool on = sim != null && sim.n31.wyrm31 && sim.wyrm24 != null && (sim.wyrm24.Alive || sim.Wyrm46ArenaHere) && sim.wyrm24Arena != null && sim.wyrm31CrumbleBoxes.Count > 0;
            if (!on && crumbleRoot == null) return;
            if (crumbleRoot == null) { crumbleRoot = new GameObject("WYRM CRUMBLE").transform; crumbleRoot.SetParent(root, false); }
            if (crumbleRoot.gameObject.activeSelf != on) crumbleRoot.gameObject.SetActive(on);
            CrumbleBoxesShown = 0;
            CracksShown = false;
            if (!on) return;
            while (crumbleT.Count < sim.wyrm31CrumbleBoxes.Count)
            {
                crumbleR.Add(Part(crumbleRoot, "crumble box " + crumbleT.Count, out Transform t, false));
                crumbleT.Add(t);
                crackR.Add(Part(crumbleRoot, "crumble cracks " + crackT.Count, out Transform k, true));
                crackT.Add(k);
            }
            Vec3 o = sim.Wyrm24Spawn - sim.wyrm24Arena.spawn;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * UnityTuning.Wyrm31CrackRate * 2 * Mathf.PI);
            for (int i = 0; i < crumbleT.Count; i++)
            {
                int idx = i < sim.wyrm31CrumbleBoxes.Count ? sim.wyrm31CrumbleBoxes[i] : -1;
                bool show = idx >= 0 && idx < sim.wyrm24Arena.boxes.Count && !sim.Wyrm31Fallen(idx);
                if (crumbleT[i].gameObject.activeSelf != show) crumbleT[i].gameObject.SetActive(show);
                bool crack = show && sim.wyrm31CrumbleState == 1;
                if (crackT[i].gameObject.activeSelf != crack) crackT[i].gameObject.SetActive(crack);
                if (!show) continue;
                CrumbleBoxesShown++;
                var (mn, mx, _) = sim.wyrm24Arena.boxes[idx];
                Vector3 a = U(mn + o), b = U(mx + o);
                Vector3 centre = (a + b) * 0.5f, size = new Vector3(Mathf.Abs(b.x - a.x), Mathf.Abs(b.y - a.y), Mathf.Abs(b.z - a.z));
                crumbleT[i].SetPositionAndRotation(centre, Quaternion.identity);
                crumbleT[i].localScale = size;
                Tint(crumbleR[i], Look.Hex(UnityTuning.Wyrm31StoneHex));
                if (!crack) continue;
                CracksShown = true;
                crackT[i].SetPositionAndRotation(centre, Quaternion.identity);
                crackT[i].localScale = size + Vector3.one * UnityTuning.Wyrm31CrackGrow;
                Tint(crackR[i], Look.Hex(UnityTuning.Stag31DangerHex, Mathf.Lerp(UnityTuning.Wyrm31CrackMin, UnityTuning.Wyrm31CrackMax, (float)sim.Wyrm31CrumbleProgress) * (0.6f + 0.4f * pulse)));
            }
        }

        void EnsureStag()
        {
            if (stagRoot != null) return;
            stagRoot = new GameObject("THE STAG").transform;
            stagRoot.SetParent(root, false);
            bodyR = Part(stagRoot, "stag body", out bodyT, false);
            headR = Part(stagRoot, "stag head", out headT, false);
            neckR = Part(stagRoot, "stag neck", out neckT, false);
            for (int i = 0; i < 4; i++) legR[i] = Part(stagRoot, "stag leg " + i, out legT[i], false);
            for (int i = 0; i < 2; i++) crystalR[i] = Part(stagRoot, "stag crystal " + i, out crystalT[i], true);
            backR = Part(stagRoot, "stag open back", out backT, true);
            lineR = Part(stagRoot, "stag charge line", out lineT, true);
            stompR = Part(stagRoot, "stag stomp ring", out stompT, true);
            var ball = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stompT.GetComponent<MeshFilter>().sharedMesh = ball.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(ball);
            for (int i = 0; i < UnityTuning.Stag31ShardPool; i++)
            {
                shardR.Add(Part(stagRoot, "stag shard " + i, out Transform t, true));
                shardT.Add(t);
                t.gameObject.SetActive(false);
            }
            var wg = new GameObject("stag melted banks");
            wg.transform.SetParent(stagRoot, false);
            wg.AddComponent<MeshFilter>();
            waterR = wg.AddComponent<MeshRenderer>();
            waterR.sharedMaterial = add;
            waterR.shadowCastingMode = ShadowCastingMode.Off;
            waterT = wg.transform;
            waterT.gameObject.SetActive(false);
            stagRoot.gameObject.SetActive(false);
        }

        /// <summary>The melted banks' film, once: a strip per bank band (latLaneOut..latBankTop) following the ramp, the lap cut into Stag31WaterStrips pieces.</summary>
        void BuildWater(Rink28Arena a)
        {
            var kit = new MeshKit();
            int n = UnityTuning.Stag31WaterStrips;
            double lift = UnityTuning.Stag31WaterLift;
            for (int i = 0; i < n; i++)
            {
                double s0 = a.Lap * i / n, s1 = a.Lap * (i + 1) / n;
                a.math.PointAt(s0, a.latLaneOut, out double ax, out double az, out _, out _);
                a.math.PointAt(s1, a.latLaneOut, out double bx, out double bz, out _, out _);
                a.math.PointAt(s1, a.latBankTop, out double cx, out double cz, out _, out _);
                a.math.PointAt(s0, a.latBankTop, out double dx, out double dz, out _, out _);
                double y0 = a.HeightAtLateral(a.latLaneOut) + lift, y1 = a.HeightAtLateral(a.latBankTop) + lift;
                kit.Quad(U(new Vec3(ax, y0, az)), U(new Vec3(bx, y0, bz)), U(new Vec3(cx, y1, cz)), U(new Vec3(dx, y1, dz)), Vector3.up, Color.white);
                kit.Quad(U(new Vec3(dx, y1, dz)), U(new Vec3(cx, y1, cz)), U(new Vec3(bx, y0, bz)), U(new Vec3(ax, y0, az)), Vector3.up, Color.white); // both faces (the winding of PointAt's lateral is not checked)
            }
            waterT.GetComponent<MeshFilter>().sharedMesh = kit.Build("stag melted banks");
            waterBuilt = true;
        }

        void RenderStag(SurvivalSim sim, float alpha)
        {
            Stag31 st = sim?.stag31;
            bool on = st != null && st.Alive;
            if (!on && stagRoot == null) return;
            EnsureStag();
            if (stagRoot.gameObject.activeSelf != on) stagRoot.gameObject.SetActive(on);
            if (!on) { StagCrystalsShown = 0; return; }
            WyrmBox b = st.box;
            Quaternion q = Face(st.facing);
            Vector3 fwd = q * Vector3.forward;
            float hr = (float)b.hr, hu = (float)b.hu, hf = (float)b.hf;
            Vector3 c = U(b.c);
            // the ram's silhouette over the sim's box: four tall legs, the torso on top (shoulders at the box's top), a thick neck dropping forward to a head LOWER than the
            // shoulders (the sim's HeadAt, which also carries the crystals and fires the shards); dark so it reads against the pale ice
            float floorY = c.y - hu, H = hu * 2, legH = H * UnityTuning.Stag31LegFrac;
            Color dark = Look.Hex(UnityTuning.Stag31BodyHex), legC = Look.Hex(UnityTuning.Stag31LegHex);
            bodyT.SetPositionAndRotation(new Vector3(c.x, floorY + legH + (H - legH) * 0.5f, c.z), q);
            bodyT.localScale = new Vector3(hr * 2, H - legH, hf * 2);
            Tint(bodyR, dark);
            Vector3 head = U(st.HeadAt);
            float hh = (float)st.p.headHalf;
            headT.SetPositionAndRotation(head, q);
            headT.localScale = new Vector3(hh * 2, hh * 2, hh * 2 * UnityTuning.Stag31HeadLength);
            Tint(headR, dark);
            Beam(neckT, new Vector3(c.x, floorY + H * 0.85f, c.z) + fwd * hf * 0.85f, head, hr * UnityTuning.Stag31NeckThick);
            Tint(neckR, dark);
            Vector3 right = q * Vector3.right;
            float lt = UnityTuning.Stag31LegThick;
            for (int i = 0; i < 4; i++)
            {
                Vector3 at = new Vector3(c.x, floorY + legH * 0.5f, c.z) + fwd * (i < 2 ? hf * 0.72f : -hf * 0.72f) + right * (i % 2 == 0 ? -hr * 0.72f : hr * 0.72f);
                legT[i].SetPositionAndRotation(at, q);
                legT[i].localScale = new Vector3(lt, legH, lt);
                Tint(legR[i], legC);
            }
            float pulse = 0.6f + 0.4f * (0.5f + 0.5f * Mathf.Sin(Time.time * UnityTuning.Stag31PulseRate * 2 * Mathf.PI));
            int shown = 0;
            for (int k = 0; k < 2; k++)
            {
                bool up = st.crystalUp[k];
                if (crystalT[k].gameObject.activeSelf != up) crystalT[k].gameObject.SetActive(up);
                if (!up) continue;
                shown++;
                float s = (float)st.p.crystalRadius * 2;
                crystalT[k].SetPositionAndRotation(U(st.CrystalAt(k)), q * Quaternion.Euler(45, 0, 45));
                crystalT[k].localScale = new Vector3(s, s, s);
                Tint(crystalR[k], Look.Hex(UnityTuning.Stag31CyanHex, UnityTuning.Stag31CrystalGain));
            }
            StagCrystalsShown = shown;
            bool back = st.BackOpen;
            if (backT.gameObject.activeSelf != back) backT.gameObject.SetActive(back);
            if (back)
            {
                backT.SetPositionAndRotation(new Vector3(c.x, floorY + legH + (H - legH) * 0.5f, c.z) - fwd * (hf + 0.05f), q);
                backT.localScale = new Vector3(hr * 2 * 0.9f, (H - legH) * 0.9f, 0.1f);
                Tint(backR, Look.Hex(UnityTuning.Stag31CyanHex, UnityTuning.Stag31BackGain * pulse));
            }
            bool line = st.Telling;
            if (lineT.gameObject.activeSelf != line) lineT.gameObject.SetActive(line);
            if (line)
            {
                Vec3 a0 = new Vec3(st.pos.x, b.c.y - b.hu + UnityTuning.Stag31LineLift, st.pos.z), a1 = new Vec3(st.LineEnd.x, a0.y, st.LineEnd.z);
                Vector3 ua = U(a0), ub = U(a1), d = ub - ua;
                float len = Mathf.Max(0.1f, d.magnitude);
                lineT.SetPositionAndRotation((ua + ub) * 0.5f, Quaternion.LookRotation(d.sqrMagnitude > 1e-6f ? d.normalized : fwd, Vector3.up));
                lineT.localScale = new Vector3(UnityTuning.Stag31LineWidth, 0.02f, len);
                float g = Mathf.Lerp(UnityTuning.Stag31LineTellMin, UnityTuning.Stag31LineTellMax, (float)st.TellProgress);
                Tint(lineR, Look.Hex(UnityTuning.Stag31DangerHex, g));
            }
            if (stompLeft > 0) stompLeft -= Time.deltaTime;
            bool ring = stompLeft > 0;
            if (stompT.gameObject.activeSelf != ring) stompT.gameObject.SetActive(ring);
            if (ring)
            {
                float k = 1 - stompLeft / UnityTuning.Stag31StompLife, r = (float)st.p.stompRadius * 2 * k;
                stompT.SetPositionAndRotation(U(new Vec3(stompAt.x, b.c.y - b.hu + 0.05, stompAt.z)), Quaternion.identity);
                stompT.localScale = new Vector3(r, UnityTuning.Stag31StompRingHeight, r);
                Tint(stompR, Look.Hex(UnityTuning.Stag31DangerHex, 1.5f * (1 - k)));
            }
            for (int i = 0; i < shardT.Count; i++)
            {
                bool vis = i < st.shards.Count;
                if (shardT[i].gameObject.activeSelf != vis) shardT[i].gameObject.SetActive(vis);
                if (!vis) continue;
                Stag31Shard s = st.shards[i];
                float z = UnityTuning.Stag31ShardSize;
                Vector3 v = U(s.vel);
                shardT[i].SetPositionAndRotation(U(s.pos), v.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(v.normalized, Vector3.up) : Quaternion.identity);
                shardT[i].localScale = new Vector3(z * 0.5f, z * 0.5f, z * 2);
                Tint(shardR[i], Look.Hex(UnityTuning.Stag31DangerHex, UnityTuning.Stag31ShardGain));
            }
            if (st.melted && !waterBuilt) BuildWater(st.rink);
            if (waterT.gameObject.activeSelf != st.melted) waterT.gameObject.SetActive(st.melted);
            if (st.melted) Tint(waterR, Look.Hex(UnityTuning.Stag31DangerHex, UnityTuning.Stag31WaterGain));
        }
    }
}
