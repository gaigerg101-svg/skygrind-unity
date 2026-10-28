// NIGHT30 builder B: the NIGHT30 boss rules as the view draws them. Not compiled in the Editor by its author (dn-check only); the lead adds two lines in SurvivalGame.Boss26.cs:
// Boss26InitViews: `Boss30View.Current = new Boss30View(this);`   Boss26RenderViews: `Boss30View.Current?.Render(alpha);`
//  - THE LANTERN RING (design.n30.lantern30): the SAME triangles the sim collides with (Lantern30Arena.tris) as stone (panels, floor, roof, the lighter platform tops), a cyan lane strip on every
//    panel (y laneLow..laneHigh), the SWEEP (the marked panel pulses red-orange through its tell, then burns bright), the three heat vents (red-orange plates that glow through the floor's tell
//    and stay lit while it is live: they never glow for nothing), a cyan beam from the lantern to the roof while the core is open; LanternView's courtyard and its eight lanes hide (LanternView.ring30).
//  - THE KILN WYRM HEAD-PUNCH (design.n30.wyrm30): a cyan shell around the head's box while SurvivalSim.Wyrm30Punchable (the 0.4 s window), and on "wyrmStagger" a white burst plus a growing ring.
// Rule 8: cyan player-usable, red-orange danger only, the stone dark (danger reads on the dangerous surface, never as a wash). Numbers: UnityTuning.Boss30.cs.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class Boss30View
    {
        readonly SurvivalGame game;
        readonly Transform root, ringRoot, headT, ringFxT, beamT;
        readonly Renderer headR, ringFxR, beamR;
        readonly Transform[] ventT = new Transform[3];
        readonly Renderer[] ventR = new Renderer[3];
        Transform sweepT;
        Renderer sweepR;
        readonly Transform lobT;
        readonly Renderer lobR;
        /// <summary>The lob's landing spot is drawn now (the platform camper's tell).</summary>
        public bool LobShown => lobT != null && lobT.gameObject.activeSelf;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Mesh boxMesh;
        readonly Material add;
        readonly Light light;
        readonly Color laneC, sweepC, punchC, white;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        bool built;
        GameObject platformsGo;
        float ringFxLeft;
        Vector3 ringFxAt;

        /// <summary>The ring is drawn now (the reel reads it).</summary>
        public bool RingShown => ringRoot != null && ringRoot.gameObject.activeSelf;
        /// <summary>The head's cyan shell is drawn now (the reel counts its frames).</summary>
        public bool HeadGlowShown => headT != null && headT.gameObject.activeSelf;
        /// <summary>The swept panel is drawn now (tell or burn).</summary>
        public bool SweepShown => sweepT != null && sweepT.gameObject.activeSelf;
        /// <summary>The vents glow now (the floor's tell or live stage).</summary>
        public bool VentsLit { get; private set; }
        /// <summary>Staggers seen by the view (white burst + ring).</summary>
        public int staggerBursts;

        /// <summary>The live view (the game's Boss26InitViews makes it; the reel reads it): no SurvivalGame field is needed.</summary>
        public static Boss30View Current;

        public Boss30View(SurvivalGame game)
        {
            this.game = game;
            Current = this;
            laneC = Look.Hex(UnityTuning.Ring30LaneHex, UnityTuning.Ring30LaneGain);
            sweepC = Look.Hex(UnityTuning.Ring30SweepHex);
            punchC = Look.Hex(UnityTuning.WyrmPunch30Hex, UnityTuning.WyrmPunch30Gain);
            white = Look.Hex(0xffffff);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxMesh = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            add = FxDraw.Additive(game.mats.neon, "Boss30 glow");
            root = new GameObject("NIGHT30 BOSSES").transform;
            ringRoot = new GameObject("THE LANTERN RING").transform;
            ringRoot.SetParent(root, false);
            ringRoot.gameObject.SetActive(false);
            for (int i = 0; i < 3; i++) ventR[i] = Part(ringRoot, "vent " + i, out ventT[i]);
            beamR = Part(ringRoot, "core beam", out beamT);
            lobR = Part(ringRoot, "lob mark", out lobT);
            headR = Part(root, "wyrm head punchable", out headT);
            headT.gameObject.SetActive(false);
            ringFxR = Part(root, "wyrm stagger ring", out ringFxT);
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ringFxT.GetComponent<MeshFilter>().sharedMesh = ball.GetComponent<MeshFilter>().sharedMesh; // a flattened sphere: a round burst disc
            Object.Destroy(ball);
            ringFxT.gameObject.SetActive(false);
            var lg = new GameObject("Ring light");
            lg.transform.SetParent(ringRoot, false);
            light = lg.AddComponent<Light>();
            light.type = LightType.Point;
            light.shadows = LightShadows.None;
            light.range = UnityTuning.Ring30LightRange;
            light.intensity = UnityTuning.Ring30LightIntensity * UnityTuning.LightScale;
            light.color = Look.Hex(UnityTuning.Ring30LightHex);
            game.events.On<SurvivalSim.Wyrm30Event>("wyrmStagger", e =>
            {
                staggerBursts++;
                game.ParticleBurst(e.position, white, UnityTuning.WyrmStagger30Bits, UnityTuning.WyrmStagger30Speed, UnityTuning.WyrmStagger30Life, UnityTuning.WyrmStagger30Size);
                ringFxAt = U(e.position);
                ringFxLeft = UnityTuning.WyrmStagger30RingLife;
            });
        }

        Renderer Part(Transform parent, string name, out Transform t)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = boxMesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = add;
            r.shadowCastingMode = ShadowCastingMode.Off;
            t = g.transform;
            return r;
        }

        void Tint(Renderer r, Color c)
        {
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // web z is Unity -z

        static Vector3 N(Vec3 n) => new Vector3((float)n.x, (float)n.y, (float)-n.z);

        /// <summary>NIGHT32 A (arenas32): every wall vertex of the round ring gets the radial normal toward the ring's axis (Unity space), so
        /// the flat facets light as a smooth cylinder.</summary>
        static void SmoothRingNormals(Mesh m, Vector3 centre)
        {
            Vector3[] vs = m.vertices, ns = m.normals;
            for (int i = 0; i < vs.Length && i < ns.Length; i++)
            {
                if (Mathf.Abs(ns[i].y) > 0.5f) continue;
                Vector3 d = centre - vs[i];
                d.y = 0;
                if (d.sqrMagnitude > 1e-8f) ns[i] = d.normalized;
            }
            m.normals = ns;
        }

        /// <summary>The swept panel of the round ring: a curved strip over the panel's facets, full height, a little off the wall (world
        /// space under ringRoot, rebuilt when the swept panel changes).</summary>
        Mesh RoundSweepMesh(Lantern30Arena a, Vec3 o, int panel)
        {
            var k = new MeshKit();
            double apo = a.p.apothem - UnityTuning.Ring30SweepStandOff, gap = a.PanelAngle * 0.01;
            var br = a.ArcBreaks(a.SeamAngle(panel) + gap, a.SeamAngle(panel + 1) - gap);
            for (int s = 0; s + 1 < br.Count; s++)
            {
                double am = (br[s] + br[s + 1]) / 2;
                Vec3 rn = new Vec3(-System.Math.Cos(am), 0, -System.Math.Sin(am));
                Vec3 p0 = a.OnRing(br[s], apo, 0) + o, p1 = a.OnRing(br[s + 1], apo, 0) + o, h = new Vec3(0, a.p.height, 0);
                k.Quad(U(p0), U(p1), U(p1 + h), U(p0 + h), N(rn), Color.white);
            }
            return k.Build("lantern swept panel " + panel, roundSweepMesh);
        }

        Mesh roundSweepMesh;
        int roundSweepPanel = -1;

        /// <summary>The ring's meshes, once (the arena does not move): stone, platform tops, the lane strips.</summary>
        void Build(Lantern30Arena a, Vec3 o)
        {
            // one mesh per surface kind, each with its own copy of the stone material tinted through _BaseColor (round 1's Player frame: the stone material ignores vertex colours,
            // so the ring drew terracotta; the panels are dark dusk stone so only the lava and the core read hot)
            var kits = new[] { new MeshKit(), new MeshKit(), new MeshKit(), new MeshKit() }; // panels (+ platform sides), floor, roof, platform tops
            var lanes = new MeshKit();
            foreach (Lantern30Tri t in a.tris)
            {
                Vec3 n = TriangleWorld.TriangleNormal(t.a, t.b, t.c);
                int k = t.part == Lantern30Part.Floor ? 1 : t.part == Lantern30Part.Roof ? 2 : t.part == Lantern30Part.PlatformTop || t.part == Lantern30Part.PlatformSide ? 3 : 0; // NIGHT31 B: the platform sides join the tops (one object Boss31View can hide while the platforms sink)
                kits[k].Triangle(U(t.a + o), U(t.b + o), U(t.c + o), N(n), Color.white);
            }
            for (int i = 0; i < a.p.panels; i++)
            {
                if (a.Round)
                {
                    // NIGHT32 A (arenas32): the round ring's lane follows the facets; a small gap at each logical panel's seams keeps the 16 sections readable
                    double gap = a.PanelAngle * 0.02, apo = a.p.apothem - UnityTuning.Ring30LaneStandOff;
                    var br = a.ArcBreaks(a.SeamAngle(i) + gap, a.SeamAngle(i + 1) - gap);
                    for (int s = 0; s + 1 < br.Count; s++)
                    {
                        double am = (br[s] + br[s + 1]) / 2;
                        Vec3 rn = new Vec3(-System.Math.Cos(am), 0, -System.Math.Sin(am));
                        Vec3 p0 = a.OnRing(br[s], apo, 0) + o, p1 = a.OnRing(br[s + 1], apo, 0) + o;
                        Vec3 lo0 = new Vec3(0, a.p.laneLow, 0), hi0 = new Vec3(0, a.p.laneHigh, 0);
                        lanes.Quad(U(p0 + lo0), U(p1 + lo0), U(p1 + hi0), U(p0 + hi0), N(rn), Color.white);
                    }
                    continue;
                }
                Vec3 n = a.InwardNormal(i), tg = a.Tangent(i);
                Vec3 mid = -n * (a.p.apothem - UnityTuning.Ring30LaneStandOff) + o;
                double half = a.PanelWidth / 2 * 0.96;
                Vec3 l0 = mid - tg * half, l1 = mid + tg * half;
                Vec3 lo = new Vec3(0, a.p.laneLow, 0), hi = new Vec3(0, a.p.laneHigh, 0);
                lanes.Quad(U(l0 + lo), U(l1 + lo), U(l1 + hi), U(l0 + hi), N(n), Color.white);
            }
            uint[] hex = { UnityTuning.Ring30PanelHex, UnityTuning.Ring30FloorHex, UnityTuning.Ring30RoofHex, UnityTuning.Ring30PlatformHex };
            string[] names = { "ring panels", "ring floor", "ring roof", "ring platform tops" };
            for (int k = 0; k < kits.Length; k++)
            {
                var mat = new Material(game.mats.stone) { name = names[k] };
                Color c = Look.Hex(hex[k]);
                if (mat.HasProperty(BaseColor)) mat.SetColor(BaseColor, c);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", null); // a warm stone texture would tint the dusk colour brown: the ring is flat lit stone in its own colour
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", null);
                Mesh km = kits[k].Build("lantern " + names[k]);
                if (k == 0 && a.Round) SmoothRingNormals(km, U(o)); // NIGHT32 A: the 64 facets shade as one curved wall (no visible facets)
                var ringPart = MeshKit.Object(names[k], ringRoot, km, Light54.StoneOrLit(game, mat, "Boss30View", names[k], hex[k]), shadows: false); // NIGHT54 light54: the stone copy drew white (its kits' vertex colour); the hex on URP Lit with the row on
                if (k == 3) platformsGo = ringPart.gameObject;
            }
            Transform lanesT = MeshKit.Object("ring lanes", ringRoot, lanes.Build("lantern ring lanes"), add, shadows: false).transform;
            Tint(lanesT.GetComponent<Renderer>(), laneC);
            var sweepGo = new GameObject("swept panel");
            sweepGo.transform.SetParent(ringRoot, false);
            sweepT = sweepGo.transform;
            sweepGo.AddComponent<MeshFilter>().sharedMesh = boxMesh;
            sweepR = sweepGo.AddComponent<MeshRenderer>();
            sweepR.sharedMaterial = add;
            sweepR.shadowCastingMode = ShadowCastingMode.Off;
            sweepGo.SetActive(false);
            light.transform.position = U(new Vec3(o.x, o.y + a.p.height - UnityTuning.Ring30LightBelowRoof, o.z));
            built = true;
        }

        public void Render(double alpha)
        {
            SurvivalSim sim = game.sim;
            RenderRing(sim);
            RenderWyrm(sim);
            RenderGlacier(sim, alpha);
            RenderLoom(sim);
        }

        // --- THE LOOM: the blocky body under the deck, four legs of three boxes, eight joints (cyan while they take full damage), the CUT's red line -----------------
        Transform loomRoot, loomBodyT, cutT, sweepBarT;
        Renderer loomBodyR, cutR, sweepBarR;
        /// <summary>The Loom's sweep bar is drawn now.</summary>
        public bool LoomSweepShown => sweepBarT != null && sweepBarT.gameObject.activeSelf;
        readonly Transform[] legT = new Transform[12], jointT = new Transform[8];
        readonly Renderer[] legR = new Renderer[12], jointR = new Renderer[8];
        Material loomSolid;
        /// <summary>The Loom is drawn now; joints drawn cyan now; the cut's red line drawn now (the reel counts them).</summary>
        public bool LoomShown => loomRoot != null && loomRoot.gameObject.activeSelf;
        public int LoomCyanJoints { get; private set; }
        public bool CutLineShown => cutT != null && cutT.gameObject.activeSelf;

        Renderer SolidPart(Transform parent, string name, out Transform t)
        {
            Renderer r = Part(parent, name, out t);
            r.sharedMaterial = loomSolid;
            return r;
        }

        void EnsureLoom()
        {
            if (loomRoot != null) return;
            loomSolid = FxDraw.Opaque(game.mats.neon, "Loom body", Color.white);
            loomRoot = new GameObject("THE LOOM").transform;
            loomRoot.SetParent(root, false);
            loomBodyR = SolidPart(loomRoot, "loom body", out loomBodyT);
            for (int i = 0; i < legT.Length; i++) legR[i] = SolidPart(loomRoot, "loom leg " + i, out legT[i]);
            for (int i = 0; i < jointT.Length; i++) jointR[i] = Part(loomRoot, "loom joint " + i, out jointT[i]);
            cutR = Part(loomRoot, "loom cut line", out cutT);
            loomRoot.gameObject.SetActive(false);
        }

        void Beam(Transform t, Vec3 a, Vec3 b, float thick)
        {
            Vector3 ua = U(a), ub = U(b), d = ub - ua;
            float len = d.magnitude;
            t.SetPositionAndRotation((ua + ub) * 0.5f, len > 1e-5f ? Quaternion.LookRotation(d / len, Mathf.Abs(d.y) > 0.99f * len ? Vector3.forward : Vector3.up) : Quaternion.identity);
            t.localScale = new Vector3(thick, thick, len);
        }

        void RenderLoom(SurvivalSim sim)
        {
            Loom30 l = sim?.loom30;
            bool on = l != null && l.Alive;
            if (!on && loomRoot == null) return;
            EnsureLoom();
            if (loomRoot.gameObject.activeSelf != on) loomRoot.gameObject.SetActive(on);
            if (!on) { LoomCyanJoints = 0; return; }
            WyrmBox b = l.box;
            loomBodyT.SetPositionAndRotation(U(b.c), Quaternion.identity);
            loomBodyT.localScale = new Vector3((float)b.hr * 2, (float)b.hu * 2, (float)b.hf * 2);
            Tint(loomBodyR, Look.Hex(UnityTuning.Loom30BodyHex));
            float th = (float)l.p.legThick;
            Color leg = Look.Hex(UnityTuning.Loom30LegHex);
            for (int i = 0; i < 4; i++)
            {
                Beam(legT[3 * i], l.LegRoot(i), l.joints[2 * i + 1], th);
                Beam(legT[3 * i + 1], l.joints[2 * i + 1], l.joints[2 * i], th);
                Beam(legT[3 * i + 2], l.joints[2 * i], l.grips[i], th);
                for (int k = 0; k < 3; k++) Tint(legR[3 * i + k], leg);
            }
            int cyan = 0;
            float pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * UnityTuning.WyrmPunch30PulseRate * 2 * Mathf.PI);
            for (int j = 0; j < 8; j++)
            {
                float s = (float)l.p.jointRadius * 2 * UnityTuning.Loom30JointScale;
                jointT[j].SetPositionAndRotation(U(l.joints[j]), Quaternion.identity);
                jointT[j].localScale = new Vector3(s, s, s);
                bool full = l.jointFull[j];
                if (full) cyan++;
                Tint(jointR[j], full ? punchC * pulse : Look.Hex(UnityTuning.Loom30JointHex, UnityTuning.Loom30JointGain));
            }
            LoomCyanJoints = cyan;
            // the SWEEP: a red-orange bar across the deck at shin height, at the sweeping leg's grip through the tell (pulsing), crossing the deck while live
            bool sw = (l.SweepTelling || l.SweepLive) && l.SweepLeg >= 0;
            if (sweepBarT == null) { sweepBarR = Part(loomRoot, "loom sweep bar", out sweepBarT); }
            if (sweepBarT.gameObject.activeSelf != sw) sweepBarT.gameObject.SetActive(sw);
            if (sw)
            {
                Vec3 ga = l.grips[l.SweepLeg], gb = l.grips[(l.SweepLeg + 2) % 4];
                double prog = l.SweepLive ? l.SweepBar : 0;
                Vec3 at = ga + (gb - ga) * prog;
                Vec3 dir = gb - ga;
                double dl = System.Math.Sqrt(dir.x * dir.x + dir.z * dir.z);
                float span = (float)(dl * 0.75);
                sweepBarT.SetPositionAndRotation(U(new Vec3(at.x, at.y + l.p.sweepHeight * 0.5, at.z)), Quaternion.LookRotation(N(new Vec3(dir.x / dl, 0, dir.z / dl)), Vector3.up));
                sweepBarT.localScale = new Vector3(span, (float)l.p.sweepHeight, 0.4f);
                float swPulse = 0.5f + 0.5f * Mathf.Sin(Time.time * UnityTuning.Ring30SweepTellRate * 2 * Mathf.PI);
                Tint(sweepBarR, sweepC * (l.SweepLive ? UnityTuning.Ring30SweepBurnGain : Mathf.Lerp(UnityTuning.Ring30SweepTellMin, UnityTuning.Ring30SweepTellMax, swPulse)));
            }
            bool cut = l.CutTelling && sim.mover.rope.phase == RopePhase.Attached;
            if (cutT.gameObject.activeSelf != cut) cutT.gameObject.SetActive(cut);
            if (cut)
            {
                Beam(cutT, sim.mover.RopePoint, sim.mover.rope.anchor, UnityTuning.Loom30CutThick);
                float g = Mathf.Lerp(UnityTuning.Ring30SweepTellMin, UnityTuning.Ring30SweepBurnGain, (float)l.CutTellProgress);
                Tint(cutR, sweepC * g);
            }
        }

        // --- THE GLACIER REBUILT: the homing shards, the frost sweep's band, the volley's tell -------------------------------------------
        Transform[] shardT, bandT;
        Renderer[] shardR, bandR;
        Transform volleyT;
        Renderer volleyR;
        /// <summary>The sweep band is drawn now / the volley tell glows now (the reel counts their frames).</summary>
        public bool BandShown => bandT != null && bandT[0].gameObject.activeSelf;
        public bool VolleyGlowShown => volleyT != null && volleyT.gameObject.activeSelf;

        void EnsureGlacier()
        {
            if (shardT != null) return;
            shardT = new Transform[UnityTuning.Glacier30ShardPool];
            shardR = new Renderer[shardT.Length];
            for (int i = 0; i < shardT.Length; i++) { shardR[i] = Part(root, "glacier shard " + i, out shardT[i]); shardT[i].gameObject.SetActive(false); }
            bandT = new Transform[UnityTuning.Glacier30BandSegments];
            bandR = new Renderer[bandT.Length];
            for (int i = 0; i < bandT.Length; i++) { bandR[i] = Part(root, "glacier sweep " + i, out bandT[i]); bandT[i].gameObject.SetActive(false); }
            volleyR = Part(root, "glacier volley tell", out volleyT);
            volleyT.gameObject.SetActive(false);
        }

        void RenderGlacier(SurvivalSim sim, double alpha)
        {
            Glacier30 gl = sim?.glacier30;
            bool on = gl != null && sim.Glacier30On && sim.glacier28 != null && sim.glacier28.Alive;
            if (!on && shardT == null) return;
            EnsureGlacier();
            float a = Mathf.Clamp01((float)alpha);
            int n = on ? Mathf.Min(gl.shards.Count, shardT.Length) : 0;
            for (int i = 0; i < shardT.Length; i++)
            {
                bool live = i < n;
                if (shardT[i].gameObject.activeSelf != live) shardT[i].gameObject.SetActive(live);
                if (!live) continue;
                Glacier30Shard s = gl.shards[i];
                Vector3 v = U(s.vel);
                shardT[i].SetPositionAndRotation(Vector3.Lerp(U(s.prev), U(s.pos), a), v.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(v.normalized) : Quaternion.identity);
                shardT[i].localScale = new Vector3(UnityTuning.Glacier30ShardWidth, UnityTuning.Glacier30ShardWidth, UnityTuning.Glacier30ShardLength);
                Tint(shardR[i], sweepC * UnityTuning.Glacier30ShardGain);
            }
            bool band = on && (gl.SweepTelling || gl.SweepLive);
            for (int i = 0; i < bandT.Length; i++)
            {
                if (bandT[i].gameObject.activeSelf != band) bandT[i].gameObject.SetActive(band);
                if (!band) continue;
                // the band along the lap in segments (it bends round the caps): each a flat plate over the floor at the band's lateral middle
                Glacier28 g = sim.glacier28;
                double hl = g30HalfLength(sim), seg = 2 * hl / bandT.Length, s = gl.SweepS - hl + seg * (i + 0.5), lat = (gl.SweepLat0 + gl.SweepLat1) * 0.5;
                g.rink.PointAt(s, lat, out double x, out double z, out double tx, out double tz);
                double y = g.origin.y + g.FloorAt(lat) + UnityTuning.Glacier30BandLift;
                bandT[i].SetPositionAndRotation(U(new Vec3(x, y, z)), Quaternion.LookRotation(N(new Vec3(tx, 0, tz)), Vector3.up));
                bandT[i].localScale = new Vector3((float)(gl.SweepLat1 - gl.SweepLat0), 0.05f, (float)seg * 1.02f);
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * UnityTuning.Ring30SweepTellRate * 2 * Mathf.PI);
                float gain = gl.SweepLive ? UnityTuning.Ring30SweepBurnGain : Mathf.Lerp(UnityTuning.Ring30SweepTellMin, UnityTuning.Ring30SweepTellMax, pulse) * (float)(0.5 + 0.5 * gl.SweepTellProgress);
                Tint(bandR[i], sweepC * gain);
            }
            bool vt = on && gl.VolleyTelling;
            if (volleyT.gameObject.activeSelf != vt) volleyT.gameObject.SetActive(vt);
            if (vt)
            {
                WyrmBox b = sim.glacier28.box;
                volleyT.SetPositionAndRotation(U(b.c), Quaternion.LookRotation(N(b.f), N(b.u)));
                float k = UnityTuning.Glacier30VolleyScale;
                volleyT.localScale = new Vector3((float)b.hr * 2 * k, (float)b.hu * 2 * k, (float)b.hf * 2 * k);
                Tint(volleyR, sweepC * UnityTuning.Glacier30VolleyGain);
            }
        }

        static double g30HalfLength(SurvivalSim sim) => sim.cfg.Num("design.n30.glacier30.sweepHalfLength");

        void RenderRing(SurvivalSim sim)
        {
            Lantern27 l = sim?.lantern27;
            bool on = l != null && l.Alive && sim.Lantern30On && sim.lantern30 != null && sim.lantern30Arena != null;
            if (game.lanternView != null) game.lanternView.ring30 = on;
            if (ringRoot.gameObject.activeSelf != on) ringRoot.gameObject.SetActive(on);
            if (!on) { VentsLit = false; return; }
            if (platformsGo != null) { bool keep = !sim.Lantern31PlatformsMoved; if (platformsGo.activeSelf != keep) platformsGo.SetActive(keep); } // NIGHT31 B: Boss31View draws the sinking platforms
            Lantern30 r = sim.lantern30;
            Lantern30Arena a = sim.lantern30Arena;
            Vec3 o = l.OriginWorld;
            if (!built) Build(a, o);
            // the vents: dim at rest, bright through the floor's tell, held while live
            Lantern29Stage st = r.rule.Stage;
            VentsLit = st != Lantern29Stage.Rest;
            float vg = st == Lantern29Stage.Tell ? UnityTuning.Ring30VentTellGain * (float)(0.4 + 0.6 * r.rule.TellProgress) : st == Lantern29Stage.Live ? UnityTuning.Ring30VentLiveGain : UnityTuning.Ring30VentRestGain;
            for (int k = 0; k < ventT.Length && k < a.p.ventPanels.Length; k++)
            {
                ventT[k].position = U(a.Vent(k) + o + new Vec3(0, UnityTuning.Ring30VentLift, 0));
                ventT[k].localScale = new Vector3(UnityTuning.Ring30VentSize, 0.05f, UnityTuning.Ring30VentSize);
                Tint(ventR[k], sweepC * vg);
            }
            // the swept panel
            bool sw = r.SweepPanel >= 0 && (r.SweepTelling || r.SweepBurning);
            if (sweepT.gameObject.activeSelf != sw) sweepT.gameObject.SetActive(sw);
            if (sw)
            {
                int i = r.SweepPanel;
                if (a.Round)
                {
                    // NIGHT32 A: a curved strip that follows the round wall (a flat box would cut into the curve at its ends)
                    if (roundSweepPanel != i) { roundSweepMesh = RoundSweepMesh(a, o, i); roundSweepPanel = i; sweepT.GetComponent<MeshFilter>().sharedMesh = roundSweepMesh; }
                    sweepT.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    sweepT.localScale = Vector3.one;
                    float pulseR = 0.5f + 0.5f * Mathf.Sin(Time.time * UnityTuning.Ring30SweepTellRate * 2 * Mathf.PI);
                    float gR = r.SweepBurning ? UnityTuning.Ring30SweepBurnGain : Mathf.Lerp(UnityTuning.Ring30SweepTellMin, UnityTuning.Ring30SweepTellMax, pulseR) * (float)(0.5 + 0.5 * r.SweepTellProgress);
                    Tint(sweepR, sweepC * gR);
                }
                else
                {
                Vec3 n = a.InwardNormal(i);
                Vec3 mid = -n * (a.p.apothem - UnityTuning.Ring30SweepStandOff) + o + new Vec3(0, a.p.height * 0.5, 0);
                sweepT.SetPositionAndRotation(U(mid), Quaternion.LookRotation(N(n), Vector3.up));
                sweepT.localScale = new Vector3((float)(a.PanelWidth * 0.98), (float)a.p.height, 0.04f);
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * UnityTuning.Ring30SweepTellRate * 2 * Mathf.PI);
                float g = r.SweepBurning ? UnityTuning.Ring30SweepBurnGain : Mathf.Lerp(UnityTuning.Ring30SweepTellMin, UnityTuning.Ring30SweepTellMax, pulse) * (float)(0.5 + 0.5 * r.SweepTellProgress);
                Tint(sweepR, sweepC * g);
                }
            }
            // the lob's landing spot on the platform (the camper's tell): a red-orange plate that brightens to the throw
            bool lob = r.LobTelling;
            if (lobT.gameObject.activeSelf != lob) lobT.gameObject.SetActive(lob);
            if (lob)
            {
                lobT.SetPositionAndRotation(U(r.LobTarget + new Vec3(0, UnityTuning.Ring30VentLift, 0)), Quaternion.identity);
                lobT.localScale = new Vector3(UnityTuning.Ring30LobSize, 0.05f, UnityTuning.Ring30LobSize);
                Tint(lobR, sweepC * Mathf.Lerp(UnityTuning.Ring30SweepTellMin, UnityTuning.Ring30SweepTellMax, (float)r.LobTellProgress));
            }
            // the open core's beam, lantern to roof
            bool beam = r.rule.CoreExposed;
            if (beamT.gameObject.activeSelf != beam) beamT.gameObject.SetActive(beam);
            if (beam)
            {
                Vec3 c = l.Centre;
                double top = o.y + a.p.height, h = top - c.y;
                beamT.SetPositionAndRotation(U(new Vec3(c.x, c.y + h * 0.5, c.z)), Quaternion.identity);
                beamT.localScale = new Vector3(UnityTuning.Ring30BeamWidth, (float)h, UnityTuning.Ring30BeamWidth);
                Tint(beamR, laneC * (UnityTuning.Ring30BeamGain / UnityTuning.Ring30LaneGain));
            }
        }

        void RenderWyrm(SurvivalSim sim)
        {
            Wyrm w = sim?.wyrm24;
            bool glow = w != null && w.Alive && sim.Wyrm30Punchable;
            if (headT.gameObject.activeSelf != glow) headT.gameObject.SetActive(glow);
            if (glow)
            {
                WyrmBox b = w.boxes[0];
                headT.SetPositionAndRotation(U(b.c), Quaternion.LookRotation(N(b.f), N(b.u)));
                float k = UnityTuning.WyrmPunch30Scale;
                headT.localScale = new Vector3((float)b.hr * 2 * k, (float)b.hu * 2 * k, (float)b.hf * 2 * k);
                float pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * UnityTuning.WyrmPunch30PulseRate * 2 * Mathf.PI);
                Tint(headR, BossColours63.PartRim(game.cfg, "wyrm.punch", punchC * pulse)); // NIGHT64 B3 (bossColours63, builder D): the punch window's head glow with the head (never routed on NIGHT63; row off: punchC * pulse exactly)
            }
            bool ring = ringFxLeft > 0;
            if (ringFxT.gameObject.activeSelf != ring) ringFxT.gameObject.SetActive(ring);
            if (ring)
            {
                float t = 1 - ringFxLeft / UnityTuning.WyrmStagger30RingLife;
                float d = Mathf.Lerp(UnityTuning.WyrmStagger30RingStart, UnityTuning.WyrmStagger30RingEnd, t);
                ringFxT.SetPositionAndRotation(ringFxAt, Quaternion.identity);
                ringFxT.localScale = new Vector3(d, UnityTuning.WyrmStagger30RingThick, d);
                Tint(ringFxR, Color.Lerp(white, punchC, t) * (UnityTuning.WyrmStagger30RingGain * (1 - t)));
                ringFxLeft -= FeelClock.UnscaledDelta; // NIGHT63 D1: 0 in a same-run held frame
            }
        }
    }
}
