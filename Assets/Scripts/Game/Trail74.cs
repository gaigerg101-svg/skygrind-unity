// PG3 builder V2 (LIGHT AND MOTION), row design.n74.trail74 (a VIEW row: NightRows.ViewOn every frame; off: -skygrindRows72 trail74:0). Movement that reads at speed.
// GG's "Never again" list applies (docs/memory/CORE.md): NO screen-border speed lines (retired w20); nothing here is drawn at the picture's edge. Cyan is the player's colour (CLAUDE.md rule 8);
// the dust is the web's neutral colors.slamDust; no red-orange, no magenta. It improves MovementFx (the land puffs, the slam dust ring, the wall-ride streaks, the slide's lines, sparks and heel dust, the
// dash edge tint) and does not repeat it: every piece below is an ADDITION drawn only with the row on, and the row off leaves MovementFx exactly as it was.
//   - DASH: the gun's afterimages. While the dash runs (and 0.2 s after), three cyan echoes of the viewmodel's meshes are drawn in the overlay camera (the web's melee afterimages' way, Viewmodel.cs
//     stepGhosts: additive copies of the rig's meshes): each a little larger (a zoom echo, pushed outward from the view's centre) and pulled opposite the motion in the overlay camera's space, dimmer
//     each (0.20 / 0.12 / 0.06 x 1.6). Plus ONE soft ring pulse from the crosshair (the Kenney circle_02 sprite, locked to the camera 2.2 m ahead, 0.45 -> 3.4 m in 0.22 s, peak 0.30): a pulse that leaves
//     through the border, never a line in it.
//   - SLIDE: a spark trail at the feet. Sparks born beside and just ahead of the feet, thrown up and a little back (70 a second at the slide's full speed, gravity, a small bounce off the floor, each drawn as a
//     short pale-cyan streak; they hang where they are born and the slider runs past them), and two glowing rails laid flat on the floor 0.5 m to either side of the path, one segment per 0.45 m, fading over
//     0.75 s. (A first-person eye cannot see its own heels, so the trail sits where the lower view looks: the existing cue's reasoning.) Starts on the slide's first fast frame (SlideDustFullSpeed).
//   - SLAM: the shockwave ring is a Kenney ring sprite whose OUTER edge is exactly the sim's slam.shockwaveRadius (the visual never claims reach the blast lacks); what the fall height sizes is its
//     brightness, a second inner ring (from 40 % of the fall range), the impact flare, the number and speed of the dust puffs rolled out along the floor and of the shards thrown up. The fall's share is
//     (fallDist - 2) / 12 m, clamped (the mover's Slam event carries fallDist).
//   - WALL JUMP: a kick where the feet left the wall: 5 dust puffs thrown out along the wall's normal, 8 pale-cyan sparks flicked off it, one ring lying on the wall's face (the event carries no wall; the
//     mover's wallNormal is read at the event, the velocity's horizontal direction when it is empty).
// View only: it reads the mover's public state and its events and writes nothing to the sim; no audio, no tape bit. Self-hooked (Pg3Audio's way: a hidden object made at scene load hooks the live
// SurvivalGame's OnRender / OnNewRun and its mover's Events). World time (SurvivalGame.WorldDt: frozen in a hitstop and the pause). Its dice are its own Mulberry32, seeded per run (never a sim stream);
// randomness is only at a spawn: every intensity is a smooth curve, so nothing here flickers. Pooled structs in fixed arrays, FxSprites' batched quads (one RenderMesh a bucket in use), no allocation per
// frame. With REDUCED MOTION or EFFECTS INTENSITY 0 nothing new spawns (FxGates.MotionOn); effects intensity scales the brightness.
// Probe: Trail74Probe.Line() is logged at quit and at each new run: what was drawn, and the events seen (counted with the row off too), so the lead's Player run shows the row fired.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>PG3 V2: what trail74 saw and drew (the Player log's `[trail74]` line).</summary>
    public static class Trail74Probe
    {
        public static bool On;
        /// <summary>Events the mover sent (counted with the row off too: the input fired).</summary>
        public static int SeenDash, SeenSlide, SeenSlam, SeenWallJump;
        /// <summary>Exceptions caught in the row's code (each switches the row off for the session; 0 is the answer).</summary>
        public static int Faults;
        /// <summary>A fresh slide rail's and a fresh spark's colour as submitted to the material (V2Draw's own last colour is shared with rim74 and says little).</summary>
        public static Color RailRgb = Color.black, SparkRgb = Color.black;
        /// <summary>Frames the dash echoes were drawn, dash rings, slide ground segments and sparks, slam rings / dust puffs / shards / flares, wall-jump puffs / sparks / rings.</summary>
        public static int EchoFrames, DashRings, SlideSegs, SlideSparks, SlamRings, SlamPuffs, SlamShards, SlamFlashes, WallPuffs, WallSparks, WallRings;
        /// <summary>The largest fall share (0..1) a slam was sized at, and its fall in m.</summary>
        public static double LastFall01, LastFallM;
        /// <summary>The brightest echo drawn (the dash share x the first echo's alpha).</summary>
        public static double PeakEcho;
        public static int PeakLive;

        public static void Reset()
        {
            SeenDash = SeenSlide = SeenSlam = SeenWallJump = Faults = 0;
            EchoFrames = DashRings = SlideSegs = SlideSparks = SlamRings = SlamPuffs = SlamShards = SlamFlashes = WallPuffs = WallSparks = WallRings = 0;
            LastFall01 = LastFallM = PeakEcho = 0;
            PeakLive = 0;
        }

        public static string Line() =>
            $"[trail74] on {On}; seen: dash {SeenDash}, slide {SeenSlide}, slam {SeenSlam}, wallJump {SeenWallJump}; drawn: dash echoes {EchoFrames} frames (peak {PeakEcho:0.00}) + {DashRings} rings; slide {SlideSegs} ground segments + {SlideSparks} sparks; slam {SlamRings} rings + {SlamFlashes} flares + {SlamPuffs} dust puffs + {SlamShards} shards (last fall {LastFallM:0.0} m = {LastFall01:0.00}); wall jump {WallPuffs} puffs + {WallSparks} sparks + {WallRings} rings; most live at once {PeakLive}; faults {Faults}; fresh rail ({RailRgb.r:0.00}, {RailRgb.g:0.00}, {RailRgb.b:0.00}), fresh spark ({SparkRgb.r:0.00}, {SparkRgb.g:0.00}, {SparkRgb.b:0.00}); draw: {V2Draw.Describe()}";
    }

    public sealed class Trail74View : MonoBehaviour
    {
        public const string Row = Trail74Math.Row;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<Trail74View>() != null) return;
            var go = new GameObject("Trail74");
            DontDestroyOnLoad(go);
            go.AddComponent<Trail74View>();
        }

        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        struct Ring
        {
            public Vector3 at, normal; // kind 2 (the dash): both read from the camera each frame
            public float age, life, r0, r1, peak;
            public byte kind; // 0 floor (its outer edge is r), 1 wall (its band's middle is r), 2 the dash (locked to the camera)
            public bool live;
        }

        struct Puff
        {
            public Vector3 origin, vel;
            public float size, age, life, roll, spin;
            public bool live;
        }

        struct Spark
        {
            public Vector3 pos, vel;
            public float age, life, floorY;
            public bool live;
        }

        struct Seg
        {
            public Vector3 a, b;
            public float age, life;
            public bool live;
        }

        struct Flash
        {
            public Vector3 at;
            public float age, life, size, roll;
            public bool live;
        }

        SurvivalGame game;
        Mover hookedMover;
        Mulberry32 rng = new Mulberry32(1);
        Material[] echoMats;
        RenderParams[] echoParams;
        readonly List<MeshFilter> vmFilters = new List<MeshFilter>(64);
        readonly Mesh[] echoMeshes = new Mesh[UnityTuning.N74EchoMeshCap];
        readonly Matrix4x4[] echoPlace = new Matrix4x4[UnityTuning.N74EchoMeshCap];
        bool built;
        Color cyan, pale, dust;
        double walkSpeed, shockRadius;
        readonly Ring[] rings = new Ring[UnityTuning.N74TrailRingPool];
        readonly Puff[] puffs = new Puff[UnityTuning.N74TrailPuffPool];
        readonly Spark[] sparks = new Spark[UnityTuning.N74TrailSparkPool];
        readonly Seg[] segs = new Seg[UnityTuning.N74TrailSegPool];
        readonly Flash[] flashes = new Flash[UnityTuning.N74TrailFlashPool];
        int nextRing, nextPuff, nextSpark, nextSeg, nextFlash;
        double dashK, sparkBudget;
        bool haveRail;
        Vector3 lastRailL, lastRailR;

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g != game && g != null) Hook(g);
            if (game != null && game.sim != null && game.mover != hookedMover) HookMover(game.mover);
        }

        // the Player proof: what was drawn, printed to the Player log at quit
        void OnApplicationQuit() => Debug.Log(Trail74Probe.Line());

        void Hook(SurvivalGame g)
        {
            game = g;
            hookedMover = null;
            built = false;
            ClearAll();
            g.OnRender += Render;
            g.OnNewRun += () =>
            {
                if (Trail74Probe.SeenDash + Trail74Probe.SeenSlide + Trail74Probe.SeenSlam + Trail74Probe.SeenWallJump > 0) Debug.Log(Trail74Probe.Line());
                ClearAll();
                rng = new Mulberry32(g.runSeed ^ UnityTuning.N74TrailSeedSalt);
            };
            rng = new Mulberry32(g.runSeed ^ UnityTuning.N74TrailSeedSalt);
        }

        void HookMover(Mover m)
        {
            if (hookedMover != null) hookedMover.Events -= OnMover;
            hookedMover = m;
            if (m != null) m.Events += OnMover;
        }

        void ClearAll()
        {
            for (int i = 0; i < rings.Length; i++) rings[i].live = false;
            for (int i = 0; i < puffs.Length; i++) puffs[i].live = false;
            for (int i = 0; i < sparks.Length; i++) sparks[i].live = false;
            for (int i = 0; i < segs.Length; i++) segs[i].live = false;
            for (int i = 0; i < flashes.Length; i++) flashes[i].live = false;
            dashK = sparkBudget = 0;
            haveRail = false;
        }

        void Build(SurvivalGame g)
        {
            if (g.mats == null || g.mats.neon == null) return;
            Material neon = g.mats.neon;
            GameConfig cfg = g.cfg;
            V2Draw.Ensure(neon); // V1's Fx74Draw recipe (pg3-c / pg3-d: the FxSprites batches drew nothing visible in the Player pair frames)
            cyan = Look.C(cfg, "colors.usable");
            pale = Color.Lerp(cyan, Color.white, 0.35f); // the player's cyan, paler: the hot core of a spark or a ring
            dust = Look.C(cfg, "colors.slamDust");
            walkSpeed = cfg.Num("ground.walkSpeed");
            shockRadius = cfg.Num("slam.shockwaveRadius");
            // the echoes: one additive material a copy (the web's melee afterimages' way), drawn by the viewmodel's overlay camera
            echoMats = new Material[UnityTuning.N74DashGhostAlpha.Length];
            echoParams = new RenderParams[echoMats.Length];
            for (int i = 0; i < echoMats.Length; i++)
            {
                echoMats[i] = FxDraw.Additive(neon, "Trail74 echo " + i);
                echoParams[i] = FxDraw.Params(echoMats[i], Viewmodel.Layer);
            }
            built = true;
        }

        // ---------------------------------------------------------------- events

        bool Spawning()
        {
            SurvivalGame g = game;
            if (g == null || g.cfg == null || !built) return false;
            SettingsConfig st = g.cfg.movement.settings;
            return NightRows.ViewOn(g.cfg, Row) && FxGates.MotionOn(st.reducedMotion, st.effectsIntensity);
        }

        bool faulted;

        /// <summary>A view row must never take the game down: an exception in this row's code switches the row off for the session and says so once (the mover's events run inside the sim step).</summary>
        void Fault(string where, Exception ex)
        {
            faulted = true;
            Trail74Probe.Faults++;
            Debug.LogError("[trail74] " + where + " threw; trail74 is off for this session: " + ex);
        }

        void OnMover(MoverEvent e)
        {
            if (faulted) return;
            try { OnMoverInner(e); }
            catch (Exception ex) { Fault("OnMover", ex); }
        }

        void OnMoverInner(MoverEvent e)
        {
            switch (e.type)
            {
                case MoverEventType.Dash:
                    Trail74Probe.SeenDash++;
                    if (Spawning()) AddRing(Vector3.zero, Vector3.forward, UnityTuning.N74DashRingR0, UnityTuning.N74DashRingR1, UnityTuning.N74DashRingLife, UnityTuning.N74DashRingPeak, 2);
                    break;
                case MoverEventType.SlideStart:
                    Trail74Probe.SeenSlide++;
                    if (Spawning()) sparkBudget += 6; // a first handful at once: the cue from the slide's first frame (the existing cue's rule)
                    break;
                case MoverEventType.Slam:
                    Trail74Probe.SeenSlam++;
                    if (Spawning()) OnSlam(e);
                    break;
                case MoverEventType.WallJump:
                    Trail74Probe.SeenWallJump++;
                    if (Spawning()) OnWallJump();
                    break;
            }
        }

        double Rand() => rng.Next();

        void OnSlam(MoverEvent e)
        {
            double fall01 = Trail74Math.Fall01(e.fallDist);
            Trail74Probe.LastFall01 = fall01;
            Trail74Probe.LastFallM = e.fallDist;
            Vector3 at = WebSpace.ToUnity(e.position) + Vector3.up * 0.06f;
            // the ring's outer edge is the sim's shockwave radius; the fall sizes the rest
            AddRing(at, Vector3.up, UnityTuning.N74SlamRingR0, (float)shockRadius, UnityTuning.N74SlamRingLife, (float)Trail74Math.SlamRingPeak(fall01), 0);
            Trail74Probe.SlamRings++;
            if (Trail74Math.SlamHasInner(fall01))
            {
                AddRing(at, Vector3.up, UnityTuning.N74SlamRingR0 * 0.6f, (float)(shockRadius * UnityTuning.N74SlamInnerShare), UnityTuning.N74SlamInnerLife, (float)Trail74Math.SlamRingPeak(fall01) * 0.8f, 0);
                Trail74Probe.SlamRings++;
            }
            // PG3-G: a pool of cyan light on the floor under the whole blast (a filled disc to the shockwave's radius, 0.5 s): the thin ring alone did not hold in a still; the pool does, even from a 4 m fall
            AddRing(at, Vector3.up, (float)shockRadius * UnityTuning.N74SlamPoolShare, (float)shockRadius * UnityTuning.N74SlamPoolShare, UnityTuning.N74SlamPoolLife, UnityTuning.N74SlamPoolPeak, 3);
            AddFlash(at + Vector3.up * 0.3f, (float)Trail74Math.SlamFlashSize(fall01), UnityTuning.N74SlamFlashLife);
            Trail74Probe.SlamFlashes++;
            // dust rolled out along the floor in a ring, and shards thrown up
            int puffCount = Trail74Math.SlamDust(fall01);
            double speed = Trail74Math.SlamDustSpeed(fall01), size = Trail74Math.SlamDustSize(fall01), life = Trail74Math.SlamDustLife(fall01);
            for (int i = 0; i < puffCount; i++)
            {
                double a = (i + Rand() * 0.8) / puffCount * Math.PI * 2;
                var dir = new Vector3((float)Math.Cos(a), 0f, (float)Math.Sin(a));
                AddPuff(at + dir * 0.4f + Vector3.up * 0.1f, dir * (float)(speed * (0.75 + 0.5 * Rand())) + Vector3.up * (float)(0.4 + 0.6 * Rand()), size, life);
                Trail74Probe.SlamPuffs++;
            }
            int shards = Trail74Math.SlamShards(fall01);
            double shardSpeed = Trail74Math.SlamShardSpeed(fall01);
            for (int i = 0; i < shards; i++)
            {
                double a = Rand() * Math.PI * 2;
                var dir = new Vector3((float)Math.Cos(a), 0f, (float)Math.Sin(a));
                double sp = shardSpeed * (0.55 + 0.45 * Rand());
                AddSpark(at, dir * (float)(sp * 0.7) + Vector3.up * (float)(sp * (0.45 + 0.55 * Rand())), 0.35f + (float)Rand() * 0.3f, at.y);
                Trail74Probe.SlamShards++;
            }
        }

        void OnWallJump()
        {
            Mover m = game.mover;
            Vec3 n = m.wallNormal;
            double len = Math.Sqrt(n.x * n.x + n.z * n.z);
            if (len < 0.5)
            {
                double vl = Math.Sqrt(m.velocity.x * m.velocity.x + m.velocity.z * m.velocity.z); // the kick pushed it off the wall: the push is the normal
                if (vl < 0.1) return;
                n = new Vec3(m.velocity.x / vl, 0, m.velocity.z / vl);
                len = 1;
            }
            Vector3 nu = WebSpace.ToUnity(new Vec3(n.x / len, 0, n.z / len)); // out of the wall, into the air
            Vector3 side = Vector3.Cross(Vector3.up, nu);
            Vector3 wallPt = WebSpace.ToUnity(m.position) - nu * (float)game.cfg.movement.body.capsuleRadius + Vector3.up * UnityTuning.N74WallHeightM;
            AddRing(wallPt + nu * 0.03f, nu, UnityTuning.N74WallRingR0, UnityTuning.N74WallRingR1, UnityTuning.N74WallRingLife, UnityTuning.N74WallRingPeak, 1);
            Trail74Probe.WallRings++;
            for (int i = 0; i < UnityTuning.N74WallPuffs; i++)
            {
                Vector3 vel = nu * (float)(1.2 + 1.6 * Rand()) + side * (float)((Rand() * 2 - 1) * 1.3) + Vector3.up * (float)(0.2 + 0.5 * Rand());
                double size = UnityTuning.N74WallPuffSizeMin + (UnityTuning.N74WallPuffSizeMax - UnityTuning.N74WallPuffSizeMin) * Rand();
                AddPuff(wallPt + Vector3.up * (float)((Rand() * 2 - 1) * 0.4) + side * (float)((Rand() * 2 - 1) * 0.3), vel, size, UnityTuning.N74WallPuffLife);
                Trail74Probe.WallPuffs++;
            }
            for (int i = 0; i < UnityTuning.N74WallSparks; i++)
            {
                double sp = UnityTuning.N74WallSparkSpeedMin + (UnityTuning.N74WallSparkSpeedMax - UnityTuning.N74WallSparkSpeedMin) * Rand();
                Vector3 vel = nu * (float)sp + side * (float)((Rand() * 2 - 1) * 2.0) + Vector3.up * (float)(0.5 + 2.0 * Rand());
                AddSpark(wallPt + side * (float)((Rand() * 2 - 1) * 0.3), vel, 0.25f + (float)Rand() * 0.15f, wallPt.y - UnityTuning.N74WallHeightM);
                Trail74Probe.WallSparks++;
            }
        }

        // ---------------------------------------------------------------- pools

        void AddRing(Vector3 at, Vector3 normal, float r0, float r1, float life, float peak, byte kind)
        {
            ref Ring r = ref rings[nextRing];
            nextRing = (nextRing + 1) % rings.Length;
            r.at = at;
            r.normal = normal;
            r.r0 = r0;
            r.r1 = r1;
            r.life = life;
            r.peak = peak;
            r.kind = kind;
            r.age = 0;
            r.live = true;
            if (kind == 2) Trail74Probe.DashRings++;
        }

        void AddPuff(Vector3 at, Vector3 vel, double size, double life)
        {
            ref Puff p = ref puffs[nextPuff];
            nextPuff = (nextPuff + 1) % puffs.Length;
            p.origin = at;
            p.vel = vel;
            p.size = (float)(size * (0.85 + 0.3 * Rand()));
            p.life = (float)(life * (0.85 + 0.3 * Rand()));
            p.roll = (float)(Rand() * Math.PI * 2);
            p.spin = (float)((Rand() * 2 - 1) * 1.2);
            p.age = 0;
            p.live = true;
        }

        void AddSpark(Vector3 at, Vector3 vel, float life, float floorY)
        {
            ref Spark s = ref sparks[nextSpark];
            nextSpark = (nextSpark + 1) % sparks.Length;
            s.pos = at;
            s.vel = vel;
            s.life = life;
            s.floorY = floorY;
            s.age = 0;
            s.live = true;
        }

        void AddSeg(Vector3 a, Vector3 b)
        {
            ref Seg s = ref segs[nextSeg];
            nextSeg = (nextSeg + 1) % segs.Length;
            s.a = a;
            s.b = b;
            s.life = UnityTuning.N74SlideSegLifeS;
            s.age = 0;
            s.live = true;
            Trail74Probe.SlideSegs++;
        }

        void AddFlash(Vector3 at, float size, float life)
        {
            ref Flash f = ref flashes[nextFlash];
            nextFlash = (nextFlash + 1) % flashes.Length;
            f.at = at;
            f.size = size;
            f.life = life;
            f.roll = (float)(Rand() * Math.PI * 0.5); // a four-point star repeats every quarter turn
            f.age = 0;
            f.live = true;
        }

        // ---------------------------------------------------------------- the frame

        void Render(double frameDt, double frameAlpha)
        {
            if (faulted) return;
            try { RenderInner(); }
            catch (Exception ex) { Fault("Render", ex); }
        }

        void RenderInner()
        {
            SurvivalGame g = game;
            if (g == null || g.cam == null || g.sim == null || g.cfg == null || g.mats == null) return;
            bool on = NightRows.ViewOn(g.cfg, Row); // read every frame (D1)
            Trail74Probe.On = on;
            double dt = g.WorldDt;
            Mover m = g.mover;
            if (m == null) return;
            SettingsConfig st = g.cfg.movement.settings;
            bool motion = FxGates.MotionOn(st.reducedMotion, st.effectsIntensity);
            float intensity = (float)Math.Max(0, Math.Min(g.cfg.movement.feel.effectsIntensityMax, st.effectsIntensity));

            // the dash's share keeps running whether or not the row draws (so the frame the row turns on is the frame it would have drawn)
            bool dashing = m.state == MoveState.Dash && g.Active;
            dashK = Trail74Math.Step(dashK, dashing ? 1 : 0, dt, UnityTuning.N74DashRiseS, UnityTuning.N74DashFallS);
            if (!on) return; // the row off in play: nothing is built (no sprite, no material), nothing drawn
            if (!built) Build(g); // first frame on: the textures and materials, once
            if (!built) return;

            Transform camT = g.cam.transform;
            Vector3 eye = camT.position;
            float fdt = (float)dt;
            float nearSq = UnityTuning.N74NearEyeM * UnityTuning.N74NearEyeM;
            if (motion) SpawnSlideTrail(m, g.Active, dt);
            else { haveRail = false; sparkBudget = 0; }

            V2Draw.Ensure(g.mats.neon);
            V2Draw.Begin();
            float gain = UnityTuning.N74FxGain;
            int live = 0;

            // rings
            for (int i = 0; i < rings.Length; i++)
            {
                ref Ring r = ref rings[i];
                if (!r.live) continue;
                r.age += fdt;
                float t = r.age / r.life;
                if (t >= 1) { r.live = false; continue; }
                live++;
                float radius = (float)Trail74Math.RingRadius(r.r0, r.r1, t);
                float k = (float)(Trail74Math.RingBrightness(r.peak, t) * intensity);
                if (k <= 0) continue;
                Color rc = pale * (gain * k);
                if (r.kind == 3)
                {
                    // the slam's pool: a filled disc of the player's cyan on the floor (the glow texture), radius = r1
                    V2Draw.Facing(V2Tex.Glow, r.at, r.normal, 2f * radius, 0f, cyan * (gain * k));
                }
                else if (r.kind == 2)
                {
                    // the dash's pulse: locked to the camera, a ring seen face on, 2.2 m ahead (the ring texture's band is at N74RingMidFrac of its half size)
                    V2Draw.Billboard(V2Tex.Ring, camT, eye + camT.forward * UnityTuning.N74DashRingDist, 2f * radius / UnityTuning.N74RingMidFrac, 0f, rc);
                }
                else
                {
                    // a floor ring's OUTER edge is radius (the band ends at N74RingOuterFrac of its half size); a wall ring's band middle is radius
                    float spriteR = r.kind == 0 ? radius / UnityTuning.N74RingOuterFrac : radius / UnityTuning.N74RingMidFrac;
                    V2Draw.Facing(V2Tex.Ring, r.at, r.normal, 2f * spriteR, 0f, rc);
                }
            }

            // flashes
            for (int i = 0; i < flashes.Length; i++)
            {
                ref Flash f = ref flashes[i];
                if (!f.live) continue;
                f.age += fdt;
                float t = f.age / f.life;
                if (t >= 1) { f.live = false; continue; }
                live++;
                float k = (1 - t) * (1 - t) * intensity;
                if ((f.at - eye).sqrMagnitude < nearSq) continue;
                V2Draw.Billboard(V2Tex.Star, camT, f.at, f.size * (0.7f + 0.5f * t), f.roll, pale * (gain * UnityTuning.N74SlamFlashPeak * k)); // each flash turned its own way: the star's points never line up with the screen's
            }

            // dust puffs: a bloom that slows (v e^-kt), thins at the end
            for (int i = 0; i < puffs.Length; i++)
            {
                ref Puff p = ref puffs[i];
                if (!p.live) continue;
                p.age += fdt;
                float t = p.age / p.life;
                if (t >= 1) { p.live = false; continue; }
                live++;
                Vector3 at = p.origin + p.vel * (float)Trail74Math.Drift(1, p.age);
                float k = (float)Trail74Math.PuffBrightness(t) * intensity;
                if (k <= 0) continue;
                if ((at - eye).sqrMagnitude < nearSq) continue; // a puff at the lens would fill the picture
                V2Draw.Billboard(V2Tex.Glow, camT, at, p.size * (float)Trail74Math.PuffGrow(t), p.roll + p.spin * p.age, dust * k);
            }

            // sparks: short pale-cyan streaks along their velocity, falling, bouncing a little off the floor
            for (int i = 0; i < sparks.Length; i++)
            {
                ref Spark s = ref sparks[i];
                if (!s.live) continue;
                s.age += fdt;
                float t = s.age / s.life;
                if (t >= 1) { s.live = false; continue; }
                live++;
                s.vel.y -= UnityTuning.N74SparkGravity * fdt;
                s.pos += s.vel * fdt;
                if (s.pos.y < s.floorY)
                {
                    s.pos.y = s.floorY;
                    s.vel.y = -s.vel.y * 0.25f;
                    s.vel.x *= 0.6f;
                    s.vel.z *= 0.6f;
                }
                float speed = s.vel.magnitude;
                Vector3 tail = speed > 0.01f ? s.vel / speed * Mathf.Max(0.05f, speed * UnityTuning.N74SparkStreakS) : Vector3.up * 0.05f;
                float k = (float)Trail74Math.SparkBrightness(t) * intensity;
                if ((s.pos - eye).sqrMagnitude < nearSq) continue; // a spark at the lens would be a screen-sized card
                V2Draw.Ribbon(eye, s.pos - tail, s.pos, UnityTuning.N74SparkWidthM, pale * (gain * k), V2Tex.Streak); // the streak texture is hottest at its head (the spark's)
                if (t < 0.1f) Trail74Probe.SparkRgb = V2Draw.LastColour; // a fresh spark's colour as submitted
            }

            // the slide's ground strip: flat on the floor, fading
            for (int i = 0; i < segs.Length; i++)
            {
                ref Seg s = ref segs[i];
                if (!s.live) continue;
                s.age += fdt;
                float t = s.age / s.life;
                if (t >= 1) { s.live = false; continue; }
                live++;
                Vector3 d = s.b - s.a;
                if (d.sqrMagnitude < 1e-6f) continue;
                float k = (float)Trail74Math.SegBrightness(t) * intensity;
                Vector3 pad = d * (0.5f * (UnityTuning.N74SlideSegOverlap - 1f)); // each strip runs a little past its segment: neighbours overlap into one rail
                V2Draw.Flat(s.a - pad + Vector3.up * 0.03f, s.b + pad + Vector3.up * 0.03f, UnityTuning.N74SlideSegWidthM, cyan * (gain * k), V2Tex.Beam);
                if (t < 0.05f) Trail74Probe.RailRgb = V2Draw.LastColour; // a fresh rail's colour as submitted (the target: about (0.10, 0.55, 0.65))
            }

            V2Draw.End();
            if (live > Trail74Probe.PeakLive) Trail74Probe.PeakLive = live;

            // the dash's echoes of the gun, in the overlay camera
            if (motion && dashK > 0.01) DrawEchoes(g, m, (float)dashK * intensity);
        }

        /// <summary>The slide: two glowing rails laid on the floor to either side of the path, and sparks born beside and just ahead of the feet, while sliding on the ground faster than a walk. A first-person
        /// eye cannot see its own heels, so both are placed where the lower view looks (the existing cue's reasoning); both stay where they are born and the slider runs past them.</summary>
        void SpawnSlideTrail(Mover m, bool active, double dt)
        {
            double speed = Math.Sqrt(m.velocity.x * m.velocity.x + m.velocity.z * m.velocity.z);
            double k = m.state == MoveState.Slide && m.grounded && active ? Trail74Math.SlideShare(speed, walkSpeed, UnityTuning.SlideDustFullSpeed) : 0;
            if (k <= 0)
            {
                haveRail = false;
                sparkBudget = 0;
                return;
            }
            double dx = m.velocity.x / speed, dz = m.velocity.z / speed;
            Vec3 p = m.position;
            Vector3 fwd = WebSpace.ToUnity(new Vec3(dx, 0, dz));
            Vector3 side = Vector3.Cross(Vector3.up, fwd);
            Vector3 feet = WebSpace.ToUnity(p);
            Vector3 railL = feet + fwd * UnityTuning.N74SlideRailAheadM - side * UnityTuning.N74SlideRailSideM + Vector3.up * 0.02f;
            Vector3 railR = feet + fwd * UnityTuning.N74SlideRailAheadM + side * UnityTuning.N74SlideRailSideM + Vector3.up * 0.02f;
            if (!haveRail)
            {
                lastRailL = railL;
                lastRailR = railR;
                haveRail = true;
            }
            else if ((railL - lastRailL).sqrMagnitude >= UnityTuning.N74SlideSegMinM * UnityTuning.N74SlideSegMinM)
            {
                AddSeg(lastRailL, railL);
                AddSeg(lastRailR, railR);
                lastRailL = railL;
                lastRailR = railR;
            }
            sparkBudget += UnityTuning.N74SlideSparkRate * k * dt;
            while (sparkBudget >= 1)
            {
                sparkBudget--;
                float ahead = UnityTuning.N74SlideSparkAheadMin + (UnityTuning.N74SlideSparkAheadMax - UnityTuning.N74SlideSparkAheadMin) * (float)Rand();
                float off = (UnityTuning.N74SlideSparkSideMin + (UnityTuning.N74SlideSparkSideMax - UnityTuning.N74SlideSparkSideMin) * (float)Rand()) * (Rand() < 0.5 ? -1f : 1f);
                float bk = UnityTuning.N74SlideSparkBackMin + (UnityTuning.N74SlideSparkBackMax - UnityTuning.N74SlideSparkBackMin) * (float)Rand();
                float up = UnityTuning.N74SlideSparkUpMin + (UnityTuning.N74SlideSparkUpMax - UnityTuning.N74SlideSparkUpMin) * (float)Rand();
                float sd = (float)(Rand() * 2 - 1) * UnityTuning.N74SlideSparkSide;
                float life = UnityTuning.N74SlideSparkLifeMin + (UnityTuning.N74SlideSparkLifeMax - UnityTuning.N74SlideSparkLifeMin) * (float)Rand();
                Vector3 at = feet + fwd * ahead + side * off + Vector3.up * 0.05f;
                AddSpark(at, -fwd * bk + side * sd + Vector3.up * up, life, feet.y);
                Trail74Probe.SlideSparks++;
            }
        }

        /// <summary>Three cyan echoes of the viewmodel's meshes in the overlay camera: bigger and pulled opposite the motion, dimmer each (the web's melee afterimages' recipe).</summary>
        void DrawEchoes(SurvivalGame g, Mover m, float k)
        {
            Viewmodel vm = g.viewmodel;
            if (vm == null || vm.Camera == null || echoMats == null) return;
            Transform cam = vm.Camera.transform;
            cam.GetComponentsInChildren(false, vmFilters);
            if (vmFilters.Count == 0) return;
            Vector3 v = WebSpace.ToUnity(new Vec3(m.velocity.x, 0, m.velocity.z));
            if (v.sqrMagnitude < 1e-4f) v = g.cam.transform.forward;
            Vector3 dirCam = cam.InverseTransformDirection(v.normalized);
            // the meshes that are drawn now (an enabled renderer), read once a frame; the echoes copy them
            int n = 0;
            for (int f = 0; f < vmFilters.Count && n < echoMeshes.Length; f++)
            {
                MeshFilter mf = vmFilters[f];
                if (mf == null) continue;
                Mesh mesh = mf.sharedMesh;
                if (mesh == null) continue;
                Renderer r = mf.GetComponent<Renderer>();
                if (r == null || !r.enabled) continue;
                // THE PG3-C SQUARE (verbs37 f1453-1458: a solid cyan square, hard-edged, fading in and rising with the gun's raise, ON only): the viewmodel carries effect quads (a muzzle flash plane, a glint
                // strip, a glow card) that are invisible in play because their OWN material is additive black or alpha 0; redrawn with the echo's uniform cyan material they became solid cyan squares.
                // The web's afterimages copy only the rig's solid meshes (rig.filters), so the echo copies only solid geometry too: an opaque queue (never an additive / transparent effect material) and a part
                // no bigger than a gun's.
                Material pm = r.sharedMaterial;
                if (pm == null || pm.renderQueue >= (int)UnityEngine.Rendering.RenderQueue.Transparent) continue;
                if (r.bounds.size.sqrMagnitude > UnityTuning.N74EchoMaxPartM * UnityTuning.N74EchoMaxPartM) continue;
                echoMeshes[n] = mesh;
                echoPlace[n] = mf.transform.localToWorldMatrix;
                n++;
            }
            if (n == 0) return;
            Matrix4x4 toWorld = cam.localToWorldMatrix, toLocal = cam.worldToLocalMatrix;
            bool drew = false;
            for (int i = 0; i < echoMats.Length; i++)
            {
                float b = (float)Trail74Math.GhostBrightness(i, 1) * k;
                if (b <= 0.002f) continue;
                echoMats[i].SetColor(BaseColor, cyan * b);
                Matrix4x4 echo = toWorld * Matrix4x4.Translate(-dirCam * (float)Trail74Math.GhostBack(i)) * Matrix4x4.Scale(Vector3.one * (float)Trail74Math.GhostScale(i)) * toLocal;
                for (int f = 0; f < n; f++)
                {
                    Matrix4x4 at = echo * echoPlace[f];
                    int subs = Mathf.Max(1, echoMeshes[f].subMeshCount); // a Tripo model's parts may carry several materials: every submesh echoes
                    for (int sm = 0; sm < subs; sm++) Graphics.RenderMesh(echoParams[i], echoMeshes[f], sm, at);
                }
                drew = true;
            }
            if (drew)
            {
                Trail74Probe.EchoFrames++;
                double peak = Trail74Math.GhostBrightness(0, 1) * k;
                if (peak > Trail74Probe.PeakEcho) Trail74Probe.PeakEcho = peak;
            }
        }
    }
}
