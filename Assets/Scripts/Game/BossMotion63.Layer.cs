// NIGHT63 builder V (slate B1; view row design.n63.bossMotion63, OFF in the repo; its numbers by Tools/night/n63-v-rows.cjs): BOSSES THAT MOVE LIKE
// THEY MEAN IT, the one shared view-only layer over the five campaign bosses' views (GG 2026-10-22: "boss animations are too rigid to start").
// Research: Research/2026-10-22-forge63/research-boss-animation.md. Per boss, every frame of its fight, the view's hook (one line in each view:
// Wick37View.cs, Brawl40View.cs, WyrmView.cs, Loom37View.cs, Crown37View.Crown.cs; their bodies in <View>.N63V.cs) asks its BossMotionRig63 for a
// Motion63 from the boss's RIGID pose (what the view draws today, the sim's copy) and applies it to the drawn body:
//  - FOLLOW: the drawn position follows the rigid one through a second-order spring (t3ssel8r: k1 = z / (pi f), k2 = 1 / (2 pi f)^2,
//    k3 = r z / (2 pi f)): it lags as the boss moves off and overshoots as it stops. A jump past follow.snap x size resets it (a teleport).
//  - WIND-UP: from the sim's own attack state (BossAttackClock63: the wind-up's progress to its hit), the boss leans back away from the player,
//    pulls back and crouches, eased in over windup.riseShare of the wind-up and held to the hit, through the lean spring; the idle breath is muted.
//  - STRIKE: the lean target jumps forward (strike.lunge) for strike.holdS; the lean spring (r > 1) snaps through rest and swings back once:
//    follow-through and overshoot.
//  - FLINCH: each damage event (the boss's health fell) starts Quilez's impulse h exp(1 - h), h = t / peakS: a volume-keeping squash, a recoil
//    away from the player, a tip back; scaled by the hit's share of the pool (minShare .. 1 at fullAt). Its clock is the FRAME clock, so it is drawn
//    on the damage event's own frame even inside the sim's hitstop; it adds no freeze of its own.
//  - SLUMP: while staggered (a CYAN window, THE ICEBREAKER stuck or dazed, THE KILN WYRM staggered) the slump spring sags the boss: it sinks,
//    droops toward the player, squashes and sways; it rises back when the stagger ends.
//  - BREATH: a slow pulse in height and lift at all other times.
// THE GUARD: the composed offset is capped at limitShare x the boss's size (its collider radius, the sim's own number), the tilt at limitDeg, the
// squash at limit.squash from 1, so the drawn body never leaves its collider. Springs step on the world's frame time (game.WorldDt: slow motion
// and the sim's hitstop apply), in sub-steps under the system's safe step. Reduced motion (settings.reducedMotion) draws the rigid pose.
// READ ONLY: the layer reads the clock (BossAttackClock63.Read) and the views' own transforms; nothing here reaches the sim (no tape bit, the
// hitboxes, the damage frames and every fingerprint untouched). Off (the row, -skygrindOff63, -skygrindNoBossMotion63; BossMotion63.On read
// every frame): the rigid pose, exactly as before, and the springs hold their state (D1's same-run toggle flips it for one capture).
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>One frame's motion for a boss's drawn body (see the file's head). Rest: no offset, no tilt, unit scale.</summary>
    public struct Motion63
    {
        public bool on;
        /// <summary>World metres added to the drawn body's position (capped at limitShare x size).</summary>
        public Vector3 offset;
        /// <summary>The scale multipliers in the body's own axes (y up; x / z kept volume-equal to y).</summary>
        public Vector3 squash;
        /// <summary>A world rotation applied about the body's pivot: rot' = tilt * rot (capped at limitDeg).</summary>
        public Quaternion tilt;
        /// <summary>The drives: the lean (-1 the full wind-up back, + forward), the wind-up pose 0..1, the flinch envelope 0..1, the slump 0..1,
        /// the breath -1..1; the tilt's parts (degrees forward toward the player, degrees of side sway), the slump's sink (m); the guard's share used.</summary>
        public float lean, windup, flinch, slump, breath, fwdDeg, sideDeg, sinkM, limitShare;

        public static Motion63 Rest => new Motion63 { tilt = Quaternion.identity, squash = Vector3.one };

        /// <summary>A part's offset from its rigid pose for the probe (m): the pivot's offset plus the tilt and the squash at the part's reach.</summary>
        public float Metric(float reach)
        {
            if (!on) return 0;
            tilt.ToAngleAxis(out float deg, out Vector3 _);
            if (deg > 180) deg = 360 - deg;
            float sq = Mathf.Max(Mathf.Abs(squash.x - 1), Mathf.Max(Mathf.Abs(squash.y - 1), Mathf.Abs(squash.z - 1)));
            return offset.magnitude + reach * (deg * Mathf.Deg2Rad) + reach * sq;
        }
    }

    /// <summary>design.n63.bossMotion63.numbers for one boss (its `bosses` entry over the shared numbers). Missing numbers read 0 (the layer
    /// then draws the rigid pose: Valid false).</summary>
    public sealed class BossMotionParams63
    {
        public float followF, followZ, followR, snap, leanF, leanZ, leanR, riseShare, windupDeg, back, crouch, breathMute, lunge, holdS;
        public float flinchPeakS, flinchLifeS, flinchSquash, recoil, flinchDeg, minShare, fullAt;
        public float slumpF, slumpZ, slumpR, sink, slumpDeg, slumpSquash, swayDeg, swayHz, breathHz, breathScale, breathLift;
        public float limitShare, limitDeg, limitSquash, gain, follow;
        /// <summary>THE KILN WYRM's pieces (bosses.wyrm; 0 for the others): the ripple's delay per piece (s), the tail's share, the breath's phase per piece.</summary>
        public float rippleS, tail, wave;
        /// <summary>The side lean at the full wind-up and the side keel at the full slump (degrees, in the screen's plane; the side alternates).</summary>
        public float windupSide, slumpSide;
        /// <summary>NIGHT63 B1d: the slump's slide in the screen's plane, the way its top keels, a share of the boss's size at the full slump
        /// (slump.slide, or bosses.&lt;boss&gt;.slump.slide; absent: 0, no slide).</summary>
        public float slide;

        /// <summary>Every spring has a frequency and the guard has room (a config without the numbers: false).</summary>
        public bool Valid => followF > 0 && leanF > 0 && slumpF > 0 && limitShare > 0 && limitDeg > 0 && limitSquash > 0 && flinchPeakS > 0;

        public static BossMotionParams63 From(GameConfig cfg, string boss)
        {
            var p = new BossMotionParams63();
            if (cfg == null) return p;
            const string R = BossMotion63.Numbers;
            float N(string k) => cfg.Has(R + k) ? (float)cfg.Num(R + k) : 0f;
            p.followF = N("follow.f"); p.followZ = N("follow.z"); p.followR = N("follow.r"); p.snap = N("follow.snap");
            p.leanF = N("lean.f"); p.leanZ = N("lean.z"); p.leanR = N("lean.r");
            p.riseShare = N("windup.riseShare"); p.windupDeg = N("windup.deg"); p.back = N("windup.back"); p.crouch = N("windup.crouch"); p.breathMute = N("windup.breathMute");
            p.windupSide = N("windup.sideDeg"); p.slumpSide = N("slump.sideDeg");
            p.lunge = N("strike.lunge"); p.holdS = N("strike.holdS");
            p.flinchPeakS = N("flinch.peakS"); p.flinchLifeS = N("flinch.lifeS"); p.flinchSquash = N("flinch.squash"); p.recoil = N("flinch.recoil");
            p.flinchDeg = N("flinch.deg"); p.minShare = N("flinch.minShare"); p.fullAt = N("flinch.fullAt");
            p.slumpF = N("slump.f"); p.slumpZ = N("slump.z"); p.slumpR = N("slump.r"); p.sink = N("slump.sink"); p.slumpDeg = N("slump.deg");
            p.slumpSquash = N("slump.squash"); p.swayDeg = N("slump.swayDeg"); p.swayHz = N("slump.swayHz");
            p.breathHz = N("breath.hz"); p.breathScale = N("breath.scale"); p.breathLift = N("breath.lift");
            p.limitShare = N("limit.share"); p.limitDeg = N("limit.deg"); p.limitSquash = N("limit.squash");
            p.slide = N("slump.slide");
            p.gain = 1; p.follow = 1;
            if (boss != null && cfg.Has(R + "bosses." + boss))
            {
                string b = "bosses." + boss + ".";
                if (cfg.Has(R + b + "gain")) p.gain = N(b + "gain");
                if (cfg.Has(R + b + "follow")) p.follow = N(b + "follow");
                if (cfg.Has(R + b + "limitShare")) p.limitShare = N(b + "limitShare");
                if (cfg.Has(R + b + "limitDeg")) p.limitDeg = N(b + "limitDeg");
                // NIGHT63 B1d: a boss's own slump (THE WICK's: Logs/n63/verdict-B1.md, its stagger pair under rule 8 with the shared slump pinned
                // at its guard); a number it does not give keeps the shared one
                if (cfg.Has(R + b + "slump.sink")) p.sink = N(b + "slump.sink");
                if (cfg.Has(R + b + "slump.deg")) p.slumpDeg = N(b + "slump.deg");
                if (cfg.Has(R + b + "slump.sideDeg")) p.slumpSide = N(b + "slump.sideDeg");
                if (cfg.Has(R + b + "slump.squash")) p.slumpSquash = N(b + "slump.squash");
                if (cfg.Has(R + b + "slump.slide")) p.slide = N(b + "slump.slide");
                p.rippleS = N(b + "rippleS");
                p.tail = N(b + "tail");
                p.wave = N(b + "wave");
            }
            return p;
        }
    }

    /// <summary>The layer's pure math (the research's formulas).</summary>
    public static class BossMotion63Math
    {
        /// <summary>t3ssel8r's second-order constants: k1 = z / (pi f), k2 = 1 / (2 pi f)^2, k3 = r z / (2 pi f).</summary>
        public static void K(float f, float z, float r, out float k1, out float k2, out float k3)
        {
            f = Mathf.Max(1e-3f, f);
            k1 = z / (Mathf.PI * f);
            float w = 2f * Mathf.PI * f;
            k2 = 1f / (w * w);
            k3 = r * z / w;
        }

        /// <summary>The system's largest stable semi-implicit Euler step (the spring_motion crate's clamp): sqrt(4 k2 + k1^2) - k1.</summary>
        public static float SafeDt(float k1, float k2) => Mathf.Sqrt(4f * k2 + k1 * k1) - k1;

        /// <summary>Quilez's expImpulse: h exp(1 - h), h = k x; 1 at x = 1 / k.</summary>
        public static float ExpImpulse(float x, float k)
        {
            float h = k * Mathf.Max(0f, x);
            return h * Mathf.Exp(1f - h);
        }

        /// <summary>NIGHT63 B1e (a measurement): a sim point as the layer's body transform draws it, the pivot moved by the offset and the point
        /// turned by the tilt and squashed about the pivot (the body's rest axes the world's: THE WICK's cage, whose view sets exactly these).</summary>
        public static Vector3 DrawnPoint(Vector3 pivot, Motion63 m, Vector3 simPoint) =>
            pivot + m.offset + m.tilt * Vector3.Scale(m.squash, simPoint - pivot);

        /// <summary>A volume-keeping squash: y = s, x = z = 1 / sqrt(s).</summary>
        public static Vector3 Volume(float s)
        {
            s = Mathf.Max(0.05f, s);
            float w = 1f / Mathf.Sqrt(s);
            return new Vector3(w, s, w);
        }

        /// <summary>The wind-up pose 0..1: eased in (smoothstep) over the first riseShare of the wind-up, then held to the hit.</summary>
        public static float WindupPose(double windup01, float riseShare)
        {
            float u = riseShare > 1e-4f ? Mathf.Clamp01((float)windup01 / riseShare) : 1f;
            return u * u * (3f - 2f * u);
        }
    }

    /// <summary>A second-order follower of one float (t3ssel8r's system, semi-implicit Euler, sub-stepped under its safe step).</summary>
    public sealed class Spring63
    {
        float k1, k2 = 1, k3, safe = 1;
        float y, yd, xp;

        public float Value => y;

        public void Set(float f, float z, float r)
        {
            BossMotion63Math.K(f, z, r, out k1, out k2, out k3);
            safe = Mathf.Max(1e-4f, 0.9f * BossMotion63Math.SafeDt(k1, k2));
        }

        public void Reset(float x) { y = x; yd = 0; xp = x; }

        /// <summary>Follow x over dt seconds (dt &lt;= 0: hold).</summary>
        public float Step(float x, float dt)
        {
            if (dt <= 0f) return y;
            float xd = (x - xp) / dt;
            xp = x;
            int n = Mathf.Max(1, Mathf.CeilToInt(dt / safe));
            float h = dt / n;
            for (int i = 0; i < n; i++)
            {
                y += h * yd;
                yd += h * (x + k3 * xd - y - k1 * yd) / k2;
            }
            return y;
        }
    }

    /// <summary>A second-order follower of a point (three Spring63 on x, y, z).</summary>
    public sealed class Spring63V
    {
        readonly Spring63 x = new Spring63(), y = new Spring63(), z = new Spring63();

        public void Set(float f, float zeta, float r) { x.Set(f, zeta, r); y.Set(f, zeta, r); z.Set(f, zeta, r); }
        public void Reset(Vector3 v) { x.Reset(v.x); y.Reset(v.y); z.Reset(v.z); }
        public Vector3 Step(Vector3 v, float dt) => new Vector3(x.Step(v.x, dt), y.Step(v.y, dt), z.Step(v.z, dt));
    }

    /// <summary>One boss's motion state (one per view). Step every frame of its fight (see the file's head).</summary>
    public sealed class BossMotionRig63
    {
        public readonly string boss;
        readonly Spring63V followS = new Spring63V();
        readonly Spring63 leanS = new Spring63(), slumpS = new Spring63();
        BossMotionParams63 setFor;
        bool inited;
        Vector3 lastRigid;
        double lastHp = -1;
        float flinchT = -1, flinchAmp, strikeLeft, breathT;
        float windSide = 1f, slumpSideSign = -1f; // the side the next wind-up cocks to / the next slump keels to (alternating; no randomness)
        BossPhase63 lastPhase;
        string lastAttack;

        /// <summary>Flinches started (damage events seen while on).</summary>
        public int Flinches;

        /// <summary>The live flinch's age on the frame clock (s; -1: none) and its amplitude; the breath's clock (s): a view's secondary parts
        /// (THE KILN WYRM's pieces) ripple and breathe from these.</summary>
        public float FlinchAge => flinchT;
        public float FlinchAmp => flinchAmp;
        public float BreathT => breathT;

        public BossMotionRig63(string boss) { this.boss = boss; }

        /// <summary>A frame: the clock's read `a`, the rigid pose's position (world), the flat direction toward the player, the boss's size (its
        /// collider radius, m), the world's frame time (0 in hitstop) and the frame clock's. Not live: Rest, the state cleared for the next fight.
        /// Off (NIGHT63 fix round 2, D1's contract): the state is stepped exactly as on (springs, flinch, breath, sides) and only the drawing is
        /// skipped (Rest), so the frame after an off stretch (a same-run toggle) equals a run without it.</summary>
        public Motion63 Step(BossMotionParams63 p, BossAttack63 a, Vector3 rigid, Vector3 toward, float size, float worldDt, float frameDt, bool on)
        {
            if (!a.Live) { inited = false; lastHp = -1; flinchT = -1; lastPhase = BossPhase63.None; lastAttack = null; return Motion63.Rest; }
            if (p == null || !p.Valid) return Motion63.Rest;
            Motion63 m = Advance(p, a, rigid, toward, size, worldDt, frameDt);
            return on ? m : Motion63.Rest;
        }

        /// <summary>One frame of the rig's state, on or off (Step draws it only when on).</summary>
        Motion63 Advance(BossMotionParams63 p, BossAttack63 a, Vector3 rigid, Vector3 toward, float size, float worldDt, float frameDt)
        {
            if (!ReferenceEquals(p, setFor))
            {
                followS.Set(p.followF, p.followZ, p.followR);
                leanS.Set(p.leanF, p.leanZ, p.leanR);
                slumpS.Set(p.slumpF, p.slumpZ, p.slumpR);
                setFor = p;
            }
            size = Mathf.Max(0.1f, size);
            float g = p.gain;
            worldDt = Mathf.Max(0f, worldDt);
            frameDt = Mathf.Max(0f, frameDt);
            if (!inited || (rigid - lastRigid).magnitude > p.snap * size)
            {
                followS.Reset(rigid);
                leanS.Reset(0f);
                slumpS.Reset(0f);
                inited = true;
            }
            lastRigid = rigid;
            Vector3 f = new Vector3(toward.x, 0f, toward.z);
            f = f.sqrMagnitude > 1e-8f ? f.normalized : Vector3.forward;
            Vector3 axis = Vector3.Cross(Vector3.up, f).normalized;

            // FOLLOW: the drawn body on its spring (held in hitstop: the rigid pose holds too)
            Vector3 followOff = (followS.Step(rigid, worldDt) - rigid) * p.follow;

            // WIND-UP and STRIKE: the lean's target from the clock
            bool strikeNow = a.phase == BossPhase63.Strike && !(lastPhase == BossPhase63.Strike && lastAttack == a.attack);
            if (strikeNow) strikeLeft = p.holdS;
            if (a.phase == BossPhase63.Windup && !(lastPhase == BossPhase63.Windup && lastAttack == a.attack)) windSide = -windSide; // each attack cocks to the other side
            if (a.phase == BossPhase63.Stagger && lastPhase != BossPhase63.Stagger) slumpSideSign = -slumpSideSign; // each stagger keels to the other side
            float target = 0f;
            float pose = a.phase == BossPhase63.Windup ? BossMotion63Math.WindupPose(a.Windup01, p.riseShare) : 0f;
            if (a.phase == BossPhase63.Windup) target = -pose;
            if (strikeLeft > 0f) { target = p.lunge; strikeLeft -= worldDt; }
            float lean = leanS.Step(target, worldDt);
            float windup = Mathf.Clamp01(-lean);

            // SLUMP
            float slump = Mathf.Max(0f, slumpS.Step(a.phase == BossPhase63.Stagger ? 1f : 0f, worldDt));

            // FLINCH: a damage event (the health fell), on the frame clock
            if (lastHp >= 0 && a.hp < lastHp - 1e-6)
            {
                double share = (lastHp - a.hp) / Math.Max(1.0, a.maxHp);
                float amp = p.minShare + (1f - p.minShare) * Mathf.Clamp01(p.fullAt > 0 ? (float)(share / p.fullAt) : 1f);
                if (flinchT < 0f || flinchT > p.flinchPeakS) { flinchT = 0f; flinchAmp = amp; Flinches++; }
                else flinchAmp = Mathf.Max(flinchAmp, amp);
            }
            lastHp = a.hp;
            float e = 0f;
            if (flinchT >= 0f)
            {
                flinchT += frameDt;
                e = BossMotion63Math.ExpImpulse(flinchT, 1f / Mathf.Max(1e-3f, p.flinchPeakS)) * flinchAmp;
                if (flinchT > p.flinchLifeS) { flinchT = -1f; e = 0f; }
            }

            // BREATH (muted through a wind-up)
            breathT += worldDt;
            float br = Mathf.Sin(2f * Mathf.PI * p.breathHz * breathT) * (1f - p.breathMute * windup);

            // COMPOSE (the gain scales every amplitude)
            float fwdDeg = (p.windupDeg * lean + p.slumpDeg * slump - p.flinchDeg * e) * g;
            float sideDeg = (p.swayDeg * slump * Mathf.Sin(2f * Mathf.PI * p.swayHz * breathT) + p.slumpSide * slump * slumpSideSign + p.windupSide * Mathf.Max(0f, -lean) * windSide) * g;
            float sinkM = p.sink * size * slump * g;
            // NIGHT63 B1d: the slide, the way the top keels (AngleAxis(sideDeg, f) turns up toward f x up = -axis for a positive keel)
            Vector3 slideV = -axis * (p.slide * size * slump * slumpSideSign * g);
            Vector3 off = followOff
                + f * (p.back * size * lean * g)
                + slideV
                + Vector3.down * sinkM
                - f * (p.recoil * size * e * g)
                + Vector3.up * (p.breathLift * size * br * g);
            float sy = (1f - p.crouch * windup * g) * (1f - p.slumpSquash * slump * g) * (1f - p.flinchSquash * e * g) * (1f + p.breathScale * br * g);

            // THE GUARD
            float limitM = p.limitShare * size;
            float offShare = off.magnitude / limitM;
            if (offShare > 1f) { off *= 1f / offShare; sinkM *= 1f / offShare; offShare = 1f; } // the sink a view applies on its own (THE ICEBREAKER's knees) is capped with the offset
            float total = Mathf.Sqrt(fwdDeg * fwdDeg + sideDeg * sideDeg);
            if (total > p.limitDeg) { float k = p.limitDeg / total; fwdDeg *= k; sideDeg *= k; total = p.limitDeg; }
            sy = Mathf.Clamp(sy, 1f - p.limitSquash, 1f + p.limitSquash);
            Quaternion tilt = Quaternion.AngleAxis(fwdDeg, axis) * Quaternion.AngleAxis(sideDeg, f);
            // the composed angle of two small turns can pass their quadrature sum by a hair: hold it to the limit
            tilt.ToAngleAxis(out float ang, out Vector3 ax);
            if (ang > 180f) { ang = 360f - ang; ax = -ax; }
            if (ang > p.limitDeg) tilt = Quaternion.AngleAxis(p.limitDeg, ax);
            float share2 = Mathf.Max(offShare, Mathf.Max(Mathf.Min(ang, p.limitDeg) / p.limitDeg, Mathf.Abs(sy - 1f) / p.limitSquash));

            lastPhase = a.phase;
            lastAttack = a.attack;
            return new Motion63
            {
                on = true, offset = off, squash = BossMotion63Math.Volume(sy), tilt = tilt,
                lean = lean, windup = windup, flinch = e, slump = slump, breath = br, fwdDeg = fwdDeg, sideDeg = sideDeg, sinkM = sinkM, limitShare = share2,
            };
        }

        /// <summary>Every view's hook: the clock's read for this boss, the rig stepped on the game's clocks (on or off: D1's contract), the probe's
        /// frame begun and the body part measured (reach: the body's size, m). Off: Rest (the probe untouched: the reel reads no layer frame).</summary>
        public Motion63 Frame(SurvivalGame game, ref BossMotionParams63 cache, ref GameConfig cachedFor, Vector3 rigid, Vector3 player, float size, out BossAttack63 a)
        {
            a = default;
            if (game == null || game.sim == null) return Motion63.Rest;
            a = BossAttackClock63.Read(game.sim, boss);
            if (!ReferenceEquals(cachedFor, game.cfg)) { cache = BossMotionParams63.From(game.cfg, boss); cachedFor = game.cfg; }
            bool on = BossMotion63.On(game.cfg) && !game.cfg.movement.settings.reducedMotion;
            Motion63 m = Step(cache, a, rigid, player - rigid, size, (float)game.WorldDt, (float)game.FrameDt, on);
            if (!m.on) return m;
            BossMotion63Probe.BeginFrame(boss);
            BossMotion63Probe.Windup = m.windup;
            BossMotion63Probe.Flinch = m.flinch;
            BossMotion63Probe.Slump = m.slump;
            BossMotion63Probe.Breath = m.breath;
            if (m.flinch > 0 && Flinches > 0) BossMotion63Probe.Flinches = Flinches;
            BossMotion63Probe.Part("body", m.Metric(size), m.offset.magnitude, m.limitShare);
            // NIGHT63 B1e (a measurement): the drawn weak point against the sim's hit sphere (centre: the clock's weak point; radius: the size)
            Vector3 simWeak = WebSpace.ToUnity(a.weak);
            BossMotion63Probe.Weak((BossMotion63Math.DrawnPoint(rigid, m, simWeak) - simWeak).magnitude, size);
            return m;
        }
    }
}
