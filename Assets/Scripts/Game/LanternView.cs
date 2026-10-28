// NIGHT27 builder B (boss1): THE LANTERN as the view draws it (Core/Enemies/Lantern27.cs; design.n27.boss1). Not compiled in the Editor by its author (dn-check only).
//  - the courtyard: the arena's fixed boxes (floor, shell, corner pillars, roof with its skylight slot) as one stone mesh + a point light; the eight wall segments as separate stone slabs that
//    drop out of the draw when they retract (a segment about to retract flashes red-orange on its lane strip: danger), each with a cyan WALL-RIDE LANE strip on its inner face (player-usable);
//  - the lantern: a dark blocky cube with a thin plate on each of its four side faces: a SHIELD plate in the pale violet-white boundary tone, or, on the face whose shield is down (the face that looks
//    at the wall you ride), a bright cyan plate: that is the lit weak face; in the last stretch of its HP a cyan CORE flame cube burns on top;
//  - the orbs: red-orange spheres (pooled); the attacks' floor decals are the sim's kit.telegraph calls (Telegraph20), wired here as HulkView does.
// Rule 8: cyan player-usable, red-orange danger only, pale violet-white boundary. Numbers: UnityTuning.Boss27.cs. Hooks: SurvivalGame.Boss26.cs (Boss26InitViews / Boss26RenderViews).
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class LanternView
    {
        readonly SurvivalGame game;
        readonly Transform root;
        Transform frameRoot;
        readonly Transform bodyT, coreT;
        readonly Renderer bodyR, coreR;
        readonly Transform[] plateT = new Transform[4], laneT = new Transform[UnityTuning.LanternSegments], segT = new Transform[UnityTuning.LanternSegments], orbT = new Transform[UnityTuning.LanternOrbMax];
        readonly Transform[] sentryT = new Transform[8], sentryGlowT = new Transform[8];
        readonly Renderer[] sentryR = new Renderer[8], sentryGlowR = new Renderer[8];
        readonly Renderer[] plateR = new Renderer[4], laneR = new Renderer[UnityTuning.LanternSegments], orbR = new Renderer[UnityTuning.LanternOrbMax];
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Color body, shield, lit, orb, lane, warn, white;
        readonly Mesh boxMesh;
        readonly Light light;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        bool telegraphWired, framed;

        /// <summary>The lantern is drawn now.</summary>
        public bool Shown => root != null && root.gameObject.activeSelf;

        /// <summary>NIGHT30 B (design.n30.lantern30): Boss30View draws the ring; the courtyard and its eight lanes hide (false: NIGHT29's view).</summary>
        public bool ring30;

        public LanternView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            body = Look.Hex(UnityTuning.LanternBodyHex);
            shield = Look.C(cfg, "barrier.boundaryColor", UnityTuning.LanternShieldGain);
            lit = Look.Hex(UnityTuning.LanternLitHex, UnityTuning.LanternLitGain);
            orb = Look.Hex(UnityTuning.LanternOrbHex, UnityTuning.LanternOrbGain);
            lane = Look.Hex(UnityTuning.LanternLaneHex, UnityTuning.LanternLaneGain);
            warn = Look.Hex(UnityTuning.LanternOrbHex, UnityTuning.LanternWarnGain);
            white = Look.Hex(0xffffff);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxMesh = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh sphere = ball.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(ball);
            Material solid = FxDraw.Opaque(game.mats.neon, "Lantern body", Color.white);
            Material add = FxDraw.Additive(game.mats.neon, "Lantern glow");
            root = new GameObject("THE LANTERN").transform;
            bodyR = Part(root, "body", boxMesh, solid, out bodyT);
            coreR = Part(root, "core flame", boxMesh, add, out coreT);
            for (int i = 0; i < 4; i++) plateR[i] = Part(root, "plate " + i, boxMesh, add, out plateT[i]);
            for (int i = 0; i < UnityTuning.LanternSegments; i++) laneR[i] = Part(root, "lane " + i, boxMesh, add, out laneT[i]);
            for (int i = 0; i < UnityTuning.LanternOrbMax; i++) orbR[i] = Part(root, "orb " + i, sphere, add, out orbT[i]);
            for (int i = 0; i < sentryT.Length; i++)
            {
                sentryR[i] = Part(root, "sentry " + i, boxMesh, solid, out sentryT[i]);
                sentryGlowR[i] = Part(root, "sentry eye " + i, boxMesh, add, out sentryGlowT[i]);
            }
            var lg = new GameObject("Courtyard light");
            lg.transform.SetParent(root, false);
            light = lg.AddComponent<Light>();
            light.type = LightType.Point;
            light.shadows = LightShadows.None;
            light.range = (float)cfg.Num("design.n27.boss1.room.lightRange");
            light.intensity = (float)cfg.Num("design.n27.boss1.room.lightIntensity") * UnityTuning.LightScale;
            light.color = Look.Hex((uint)cfg.Num("design.n27.boss1.room.lightColor"));
            root.gameObject.SetActive(false);
            game.events.On<BossEvent>("bossDying", e => { if (e.boss == "lantern") game.BossDyingSeen(); });
            game.events.On<BossEvent>("bossDown", e =>
            {
                if (e.boss == "lantern") game.ParticleBurst(e.position, white, UnityTuning.LanternDeathBits, UnityTuning.LanternDeathSpeed, UnityTuning.LanternDeathLife, UnityTuning.LanternDeathSize);
            });
        }

        static Renderer Part(Transform parent, string name, Mesh mesh, Material mat, out Transform t)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
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

        /// <summary>The courtyard's fixed boxes (once, on the first fight) and the eight segments as stone slabs.</summary>
        void BuildFrame(WyrmArena arena, Vec3 o)
        {
            GameConfig cfg = game.cfg;
            Color top = Look.C(cfg, "colors.pillarTop"), side = Look.C(cfg, "colors.pillarSide"), frame = Look.Hex(UnityTuning.LanternFrameHex);
            var kit = new MeshKit();
            var segKit = new MeshKit[UnityTuning.LanternSegments];
            for (int i = 0; i < segKit.Length; i++) segKit[i] = new MeshKit();
            foreach (var (min, max, kind) in arena.boxes)
            {
                if (kind.Length == 4 && kind.StartsWith("seg")) segKit[kind[3] - '0'].Box(min + o, max + o, frame, side);
                else kit.Box(min + o, max + o, top, side);
            }
            frameRoot = new GameObject("Courtyard").transform;
            frameRoot.SetParent(root, false);
            MeshKit.Object("Courtyard stone", frameRoot, kit.Build("lantern courtyard"), game.mats.stone, shadows: false);
            for (int i = 0; i < segKit.Length; i++) segT[i] = MeshKit.Object("Segment " + i, frameRoot, segKit[i].Build("lantern segment " + i), game.mats.stone, shadows: false).transform;
            light.transform.position = new Vector3((float)o.x, (float)(o.y + game.cfg.Num("design.n27.boss1.room.lightHeight")), -(float)o.z);
            framed = true;
        }

        /// <summary>The lane strip on segment i's inner face: where it is and how it is oriented (the inner face is the slab's face toward the courtyard's middle).</summary>
        static void LaneBox(int i, double half, double h, Vec3 o, out Vector3 c, out Vector3 scale)
        {
            // the segments: 0 +x (z 0..R), 1 +z (x 0..R), 2 +z (x -R..0), 3 -x (z 0..R), 4 -x (z -R..0), 5 -z (x -R..0), 6 -z (x 0..R), 7 +x (z -R..0)
            double R = half, mid = R * 0.5, t = UnityTuning.LanternLaneThick;
            double x = 0, z = 0, sx = t, sz = R * 0.96;
            switch (i)
            {
                case 0: x = R - t * 0.5; z = mid; sx = t; sz = R * 0.96; break;
                case 7: x = R - t * 0.5; z = -mid; sx = t; sz = R * 0.96; break;
                case 3: x = -R + t * 0.5; z = mid; sx = t; sz = R * 0.96; break;
                case 4: x = -R + t * 0.5; z = -mid; sx = t; sz = R * 0.96; break;
                case 1: z = R - t * 0.5; x = mid; sx = R * 0.96; sz = t; break;
                case 2: z = R - t * 0.5; x = -mid; sx = R * 0.96; sz = t; break;
                case 5: z = -R + t * 0.5; x = -mid; sx = R * 0.96; sz = t; break;
                default: z = -R + t * 0.5; x = mid; sx = R * 0.96; sz = t; break;
            }
            c = U(new Vec3(o.x + x, o.y + UnityTuning.LanternLaneY, o.z + z));
            scale = new Vector3((float)sx, UnityTuning.LanternLaneH, (float)sz);
        }

        public void Render(double alpha)
        {
            SurvivalSim sim = game.sim;
            Lantern27 l = sim?.lantern27;
            if (!telegraphWired && sim?.bossKit != null && (object)game.Telegraphs is ITelegraph tg)
            {
                sim.bossKit.telegraph = tg;
                telegraphWired = true;
            }
            bool on = l != null && l.Alive && sim.n27.boss1;
            if (root.gameObject.activeSelf != on) root.gameObject.SetActive(on);
            if (!on) return;
            if (!framed && sim.lantern27Arena != null) BuildFrame(sim.lantern27Arena, sim.lantern27Origin);
            if (frameRoot != null && frameRoot.gameObject.activeSelf == ring30) frameRoot.gameObject.SetActive(!ring30); // NIGHT30 B: the ring replaces the courtyard
            float a = Mathf.Clamp01((float)alpha);
            WyrmBox b = l.boxes[0];
            Vector3 c = Vector3.Lerp(U(l.prevCenters[0]), U(b.c), a);
            Quaternion q = Quaternion.LookRotation(U(b.f), U(b.u));
            bodyT.SetPositionAndRotation(c, q);
            float s = (float)b.hr * 2;
            bodyT.localScale = new Vector3(s, s, s);
            Tint(bodyR, body);
            // the four plates: a shield (pale violet-white) or the lit face (cyan)
            float th = UnityTuning.LanternPlateThick, sh = UnityTuning.LanternPlateShare * s;
            for (int i = 0; i < 4; i++)
            {
                bool down = l.ShieldDownOn(i);
                Vector3 dir = i == 0 ? U(b.f) : i == 1 ? U(b.r) : i == 2 ? -U(b.f) : -U(b.r);
                plateT[i].SetPositionAndRotation(c + dir * (s * 0.5f + th * 0.5f), q);
                plateT[i].localScale = (i & 1) == 0 ? new Vector3(sh, sh, th) : new Vector3(th, sh, sh);
                Tint(plateR[i], down ? lit : shield * (l.Hurtable ? 1f : 0.6f));
            }
            // the core flame on top, lit in the finisher
            bool flame = l.Finisher;
            if (coreT.gameObject.activeSelf != flame) coreT.gameObject.SetActive(flame);
            if (flame)
            {
                float cs = s * UnityTuning.LanternCoreShare;
                coreT.SetPositionAndRotation(c + Vector3.up * (s * 0.5f + cs * 0.5f), q);
                coreT.localScale = new Vector3(cs, cs, cs);
                Tint(coreR, lit * (UnityTuning.LanternCoreGain / UnityTuning.LanternLitGain));
            }
            // the wall segments and their lanes
            Vec3 o = sim.lantern27Origin;
            int mask = l.retractedMask, warnMask = l.retractWarnMask;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 14f);
            for (int i = 0; i < UnityTuning.LanternSegments; i++)
            {
                bool gone = (mask & (1 << i)) != 0 || ring30, warning = (warnMask & (1 << i)) != 0; // NIGHT30 B: no courtyard lanes on the ring
                if (segT[i] != null && segT[i].gameObject.activeSelf == gone) segT[i].gameObject.SetActive(!gone);
                if (laneT[i].gameObject.activeSelf == gone) laneT[i].gameObject.SetActive(!gone);
                if (gone) continue;
                LaneBox(i, l.p.arenaHalf, l.p.arenaHeight, o, out Vector3 lc, out Vector3 ls);
                laneT[i].SetPositionAndRotation(lc, Quaternion.identity);
                laneT[i].localScale = ls;
                Tint(laneR[i], warning ? warn * (0.5f + pulse) : lane * (l.LitSegment == i ? 1.6f : 1f));
            }
            // the sentries (phase 3): small steel cubes with a red-orange eye (an enemy's); their kills fill the PULSE's pips
            int sn = 0;
            for (int i = 0; i < l.sentries.Count && sn < sentryT.Length; i++, sn++)
            {
                Sentry27 st = l.sentries[i];
                Vector3 sp = Vector3.Lerp(U(st.previous), U(st.position), a);
                float sd = (float)st.radius * 1.6f;
                if (!sentryT[sn].gameObject.activeSelf) { sentryT[sn].gameObject.SetActive(true); sentryGlowT[sn].gameObject.SetActive(true); }
                sentryT[sn].SetPositionAndRotation(sp, Quaternion.identity);
                sentryT[sn].localScale = new Vector3(sd, sd, sd);
                Tint(sentryR[sn], body);
                sentryGlowT[sn].SetPositionAndRotation(sp + Vector3.up * sd * 0.15f, Quaternion.identity);
                sentryGlowT[sn].localScale = new Vector3(sd * 1.04f, sd * 0.22f, sd * 1.04f);
                Tint(sentryGlowR[sn], orb);
            }
            for (int i = sn; i < sentryT.Length; i++) if (sentryT[i].gameObject.activeSelf) { sentryT[i].gameObject.SetActive(false); sentryGlowT[i].gameObject.SetActive(false); }
            // the orbs
            int n = 0;
            for (int i = 0; i < l.orbs.Count && n < orbT.Length; i++, n++)
            {
                Lantern27Orb ob = l.orbs[i];
                if (!orbT[n].gameObject.activeSelf) orbT[n].gameObject.SetActive(true);
                orbT[n].position = Vector3.Lerp(U(ob.prev), U(ob.pos), a);
                float d = (float)ob.radius * 2;
                orbT[n].localScale = new Vector3(d, d, d);
                Tint(orbR[n], orb);
            }
            for (int i = n; i < orbT.Length; i++) if (orbT[i].gameObject.activeSelf) orbT[i].gameObject.SetActive(false);
        }
    }
}
