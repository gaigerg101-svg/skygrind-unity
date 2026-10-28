// NIGHT43 builder F1 (design.n43.feel43, a view row: no sim bit; G19 GG: "two forces combining"): the FUSION's picture. W1's sim (fusion43) emits
// N43Events.Fusion ("fusion43", payload Fusion43Event) on the step a gauntlet hit / punch lands at high speed and grants its freeze (0.22 s; a
// boss 0.28 s); this draws it, on frame time so it plays through the freeze (Fusion43Burst's timeline, Feel43Math.cs):
//   t 0      the crush frame (PostFx, 2-3 frames, through the flash gate) and a chroma kick; the camera kicks up and rolls (springing back);
//            the word "FUSION" punched in above and beside the hit (Fusion43Screen); the CRACK rumble (Feel43's haptics);
//   t 0-0.09 cyan sparks from the player's side and red-orange embers from the enemy's side pulled into the hit point (ease-in streaks);
//   t 0.09   a hot-pink / white core flash and a light; two pink / white rings blow out with a burst of pink and white sparks;
//   freeze end: trauma +0.6 into the web shake (combat-web.md B6 / B7: the world holds during the stop, the camera lets go after it).
// DECIDE43 D-3: the hot whitish pink (UnityTuning.Fusion43PinkHex) is drawn only in this burst, never on a body; RIVAL keeps 0xff2bd6.
// REDUCED MOTION drops the kick, the roll and the trauma (the burst and the word stay); EFFECTS INTENSITY scales the kick; FLASH INTENSITY
// and the flash gate own the crush. Pooled: a fixed set of bursts and sprites, nothing allocated per frame once built.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>NIGHT43 F1: what the night's feel drew (the feel43 reel reads it; all 0 with the row off).</summary>
    public static class Feel43Probe
    {
        public static int Fusions, FusionSprites, Words, ScreenBursts, SlugBlasts, Burns, GunStars, GunSparks, Slugs, SlugChunks, TorchTicks, TorchSprites, TorchLights, BossOpenReads;
        public static float WordScale, WordAlpha, KickDeg, CoreK, RingRadius;

        public static void Clear()
        {
            Fusions = FusionSprites = Words = ScreenBursts = SlugBlasts = Burns = GunStars = GunSparks = Slugs = SlugChunks = TorchTicks = TorchSprites = TorchLights = BossOpenReads = 0;
            WordScale = WordAlpha = KickDeg = CoreK = RingRadius = 0;
        }

        public static string Line() =>
            $"fusions {Fusions} (sprites {FusionSprites}, screen bursts {ScreenBursts}, words {Words}), gun stars {GunStars} sparks {GunSparks}, slugs {Slugs} (chunks {SlugChunks}, slug43 blasts {SlugBlasts}), burns {Burns}, torch ticks {TorchTicks} (sprites {TorchSprites}, lights {TorchLights}), boss-open reads {BossOpenReads}";
    }

    public sealed class Fusion43View
    {
        const int Bursts = 4;

        sealed class Burst
        {
            public Fusion43Burst t;
            public Vector3 at, toCam;
            public readonly Vector3[] moteFrom = new Vector3[UnityTuning.Fusion43Motes * 2];
            public readonly Vector3[] sparkVel = new Vector3[UnityTuning.Fusion43Sparks];
            public float roll; // +-1: the roll's side
            public bool lit; // the meeting's light asked
        }

        readonly SurvivalGame game;
        readonly Feel43 feel;
        readonly Burst[] bursts = new Burst[Bursts];
        int nextBurst;
        Mulberry32 rng;
        FxSprites trace, flare, ring;
        bool built;
        Color cyan, ember, pink, core;

        // NIGHT43 round 2: the screen layer (the streams, the pop's shards / ring / core, the edge flash, the word): Fusion43Screen.cs
        readonly Fusion43Screen screen;

        public Fusion43View(SurvivalGame game, Feel43 feel)
        {
            this.game = game;
            this.feel = feel;
            for (int i = 0; i < Bursts; i++) bursts[i] = new Burst();
            rng = new Mulberry32(UnityTuning.Fusion43SeedSalt);
            screen = new Fusion43Screen(game, feel);
        }

        public void NewRun()
        {
            rng = new Mulberry32(game.runSeed ^ UnityTuning.Fusion43SeedSalt);
            for (int i = 0; i < Bursts; i++) bursts[i].t.live = false;
            screen.Clear();
        }

        bool Calm() => game.cfg.movement.settings.reducedMotion;

        /// <summary>The FUSION event (the player's): a burst at the hit point, the word, the crush, the chroma, the crack.</summary>
        public void OnFusion(Fusion43Event e)
        {
            if (!Fusion43Burst.For(e, out Fusion43Burst start)) return;
            Camera cam = game.cam;
            Vector3 at = WebSpace.ToUnity(e.position); // W1: the enemy's AimPoint, INSIDE its body
            Vector3 eye = cam != null ? cam.transform.position : at + Vector3.back;
            Vector3 toCam = eye - at;
            float dist = toCam.magnitude;
            toCam = dist > 1e-3f ? toCam / dist : Vector3.back;
            // n43-r1: the burst drawn at the body's centre was hidden by the body (the sprites depth-test): it stands on the camera's side of the skin
            at += toCam * Mathf.Min(UnityTuning.Fusion43FrontOffset, dist * 0.35f);
            Burst b = bursts[nextBurst];
            nextBurst = (nextBurst + 1) % Bursts;
            b.t = start;
            bool big = start.big;
            b.at = at;
            b.toCam = toCam;
            b.lit = false;
            b.roll = (rng.Next() < 0.5) ? -1 : 1;
            float reach = Mathf.Min(UnityTuning.Fusion43MoteReach * (big ? UnityTuning.Fusion43BossScale : 1), Vector3.Distance(eye, at) * UnityTuning.Fusion43MoteReachEye); // never in the eye
            int n = UnityTuning.Fusion43Motes;
            for (int i = 0; i < n * 2; i++)
            {
                // the player's motes (first half) start on the camera's side of the body, the enemy's (second half) on the far side
                Vector3 side = i < n ? toCam : -toCam;
                Vector3 r = SprayMath.UnitSphere(rng.Next(), rng.Next());
                Vector3 d = (side * 0.9f + r * 0.8f).normalized;
                b.moteFrom[i] = at + d * reach * (0.6f + 0.4f * (float)rng.Next());
            }
            for (int i = 0; i < b.sparkVel.Length; i++)
            {
                Vector3 v = SprayMath.UnitSphere(rng.Next(), rng.Next()) * UnityTuning.Fusion43SparkSpeed * (0.55f + 0.45f * (float)rng.Next());
                float toward = Vector3.Dot(v, toCam);
                if (toward > 0) v -= toCam * (toward * 0.85f); // blown out round the body, not into the lens
                b.sparkVel[i] = v;
            }
            Feel43Probe.Fusions++;
            Feel43Probe.Words++;
            screen.Start(at); // the streams, the pop, the edge flash, the word (the chroma kick is at the pop now)
            feel.Post?.Crush43("fusion43");
            feel.Haptics.Fire(Feel43Haptics.Crack);
        }

        void Build()
        {
            built = true;
            Material neon = game.mats.neon;
            trace = new FxSprites(neon, UnityTuning.KenneyTrace, UnityTuning.Fusion43Pool);
            flare = new FxSprites(neon, UnityTuning.KenneyFlare, UnityTuning.Fusion43Pool / 4);
            ring = new FxSprites(neon, UnityTuning.KenneyRing, UnityTuning.Fusion43Pool / 8);
            cyan = (UiTheme.Ready ? UiTheme.CyanHot : Look.C(game.cfg, "colors.usable")) * UnityTuning.Fusion43Glow;
            ember = Look.C(game.cfg, "colors.enemy") * UnityTuning.Fusion43Glow;
            pink = Look.Hex(UnityTuning.Fusion43PinkHex) * UnityTuning.Fusion43Glow;
            core = Look.Hex(UnityTuning.Fusion43CoreHex) * UnityTuning.Fusion43Glow;
            foreach (FxSprites s in new[] { trace, flare, ring })
            {
                s.Warm(cyan);
                s.Warm(ember);
                s.Warm(pink);
                s.Warm(core);
            }
        }

        /// <summary>Once a frame after the camera (Feel43.Render): the kick, the bursts drawn, the word.</summary>
        public void Render(double frameDt, double freezeLeft)
        {
            Camera cam = game.cam;
            if (cam == null) return;
            if (!built) Build();
            Transform ct = cam.transform;
            Vector3 eye = ct.position;
            float kickPitch = 0, kickRoll = 0;
            bool any = false;
            trace.Begin();
            flare.Begin();
            ring.Begin();
            Feel43Probe.CoreK = 0;
            Feel43Probe.RingRadius = 0;
            for (int bi = 0; bi < Bursts; bi++)
            {
                Burst b = bursts[bi];
                if (!b.t.live) continue;
                if (b.t.Advance(frameDt, freezeLeft) && !Calm()) game.trauma.Add(UnityTuning.Fusion43Trauma);
                double t = b.t.age;
                float scale = b.t.big ? UnityTuning.Fusion43BossScale : 1;
                // the kick: the strongest live burst's
                double kk = Fusion43Burst.Kick(t);
                if (kk * UnityTuning.Fusion43KickDeg > kickPitch)
                {
                    kickPitch = (float)(kk * UnityTuning.Fusion43KickDeg);
                    kickRoll = (float)(kk * UnityTuning.Fusion43RollDeg) * b.roll;
                }
                // the motes pulled in (streaks from where they were a moment ago to where they are)
                double c = Fusion43Burst.Converge(t);
                if (c < 1)
                {
                    double cPrev = Fusion43Burst.Converge(t - UnityTuning.Fusion43ConvergeTime * 0.35);
                    int n = UnityTuning.Fusion43Motes;
                    for (int i = 0; i < n * 2; i++)
                    {
                        Vector3 head = Vector3.LerpUnclamped(b.moteFrom[i], b.at, (float)c);
                        Vector3 tail = Vector3.LerpUnclamped(b.moteFrom[i], b.at, (float)cPrev);
                        trace.Streak(tail, head, UnityTuning.Fusion43MoteW * scale, eye, i < n ? cyan : ember, (float)(0.7 + 0.3 * c));
                        Feel43Probe.FusionSprites++;
                    }
                    any = true;
                }
                // the core flash
                double k = Fusion43Burst.Core(t);
                if (k > 0)
                {
                    float coreMax = 2 * Vector3.Distance(eye, b.at) * Mathf.Tan(UnityTuning.Fusion43CoreMaxDeg * Mathf.Deg2Rad); // capped on screen
                    float size = Mathf.Min((float)(UnityTuning.Fusion43CoreSize * scale), coreMax) * (float)Fusion43Burst.CoreSize(t);
                    flare.Billboard(b.at, size, 0, ct, pink, (float)k);
                    flare.Billboard(b.at, size * 0.45f, 0, ct, core, (float)k);
                    Feel43Probe.CoreK = Math.Max(Feel43Probe.CoreK, (float)k);
                    Feel43Probe.FusionSprites += 2;
                    any = true;
                }
                if (!b.lit && t >= UnityTuning.Fusion43ConvergeTime)
                {
                    b.lit = true;
                    game.lightAndLens?.lights?.Flash(b.at, Look.Hex(UnityTuning.Fusion43PinkHex), UnityTuning.Slug43LightIntensity * 1.3, UnityTuning.Slug43LightRange * 1.2, UnityTuning.Fusion43CoreTime);
                }
                // the rings (a second one a frame behind), capped on screen
                float maxR = Vector3.Distance(eye, b.at) * Mathf.Tan(UnityTuning.Fusion43RingMaxDeg * Mathf.Deg2Rad);
                for (int r = 0; r < 2; r++)
                {
                    var (share, rk) = Fusion43Burst.Ring(t, r * (1.0 / 60));
                    if (rk <= 0) continue;
                    float rad = Mathf.Min(UnityTuning.Fusion43RingRadius[b.t.big ? 1 : 0] * (r == 0 ? 1 : 0.7f), maxR) * (float)share;
                    ring.Billboard(b.at, 2 * rad / UnityTuning.KenneyRingMid, r * 0.6f, ct, r == 0 ? pink : core, (float)rk);
                    Feel43Probe.RingRadius = Math.Max(Feel43Probe.RingRadius, rad);
                    Feel43Probe.FusionSprites++;
                    any = true;
                }
                // the sparks blown out from the meeting
                double st = t - UnityTuning.Fusion43ConvergeTime;
                if (st >= 0 && st < UnityTuning.Fusion43SparkLife)
                {
                    float life = (float)(1 - st / UnityTuning.Fusion43SparkLife);
                    float dist = (float)(st * (1 - 0.5 * st / UnityTuning.Fusion43SparkLife)); // slowing as they fly
                    for (int i = 0; i < b.sparkVel.Length; i++)
                    {
                        Vector3 v = b.sparkVel[i] * scale;
                        Vector3 head = b.at + v * dist;
                        Vector3 tail = head - v * 0.035f;
                        trace.Streak(tail, head, UnityTuning.Fusion43SparkW * scale, eye, (i & 1) == 0 ? pink : core, life);
                        Feel43Probe.FusionSprites++;
                    }
                    any = true;
                }
            }
            if (any)
            {
                trace.Draw();
                flare.Draw();
                ring.Draw();
            }
            // the camera: a pitch-up and a roll, springing back (after CameraFeel, as KillFeel42's kick), x EFFECTS INTENSITY
            Feel43Probe.KickDeg = 0;
            if (kickPitch > 0 && !Calm())
            {
                float ei = (float)Math.Max(0, Math.Min(1, game.cfg.movement.settings.effectsIntensity));
                ct.rotation = ct.rotation * Quaternion.Euler(-kickPitch * ei, 0, kickRoll * ei);
                Feel43Probe.KickDeg = kickPitch * ei;
            }
            screen.Render(frameDt); // the screen layer (after the camera kick: its points are projected with this frame's view)
        }
    }
}
