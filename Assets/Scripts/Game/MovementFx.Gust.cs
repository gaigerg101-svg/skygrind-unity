// NIGHT19 U3-33.5 (builder V): the KESTREL's gust, seen: a burst of air that visibly moves the player. Port-only (the web's
// KESTREL zooms on its alt; the gust is NIGHT19's, design.gust, fired by the sim: builder W's N19Events.Gust, a GustEvent).
// - The cone: UnityTuning.GustStreaks streaks (the Kenney trace sprite) leave the muzzle inside the sim's cone
//   (weapons.specials.shotgun.shock.coneDeg, the web's half-angle, x GustConeShare), fast and slowing on the air's drag
//   (x(t) = v0 / k (1 - e^-kt): about 6 m of the 8 m the gust reaches), long while fast and short as they slow, pale cyan
//   fading over their second half.
// - Two shock rings (the Kenney ring sprite) across the aim: a near one bursting out at the muzzle, a far one travelling out
//   along the aim as it grows; the player's cyan, fading as (1 - t)^1.5.
// - A gust aimed down (the push goes up: a gust-jump) leaves a swirl (the Kenney twirl sprite) on the floor it fired at.
// - The wake: while the push carries the body (GustWakeTime s, the body's speed along the push over GustWakeMinSpeed),
//   trace streaks in the view's outer band laid along the body's velocity, still in the world, so the player flies past
//   them (the sprint lines' way). None with REDUCED MOTION or at EFFECTS INTENSITY 0 (it is a motion cue); the burst
//   itself stays with REDUCED MOTION (it is the gust, not camera motion), scaled by EFFECTS INTENSITY.
// Visual only: it reads the bus and the mover and writes nothing; world time (frozen in a hitstop); its dice are its own
// Mulberry32 (a render-side stream, seeded per run); pooled (UnityTuning.GustPool gusts), no allocation per frame. Every
// frame it draws, it reports its pieces to the reel as FeelFx.Gust (the `gust` scenario measures it).
// NIGHT20 U3-34.3 (builder V): the same air re-aimed onto the THUNDERCLAP's shockwave (GG: "that was supposed to do the
// shockwave, not the pistol"; builder G's N20Events.Shockwave, and the web's ConeWave on the same step drawn once): more
// streaks flying further, the rings x ShockRingScale, a bigger swirl under a launch (UnityTuning.Fx20.cs), reported as
// FeelFx.Shockwave (the reel's `shockwave` scenario). No edge-band wake on a shockwave while the AIR streaks are live
// (SpeedLines20 bursts 18 streaks on it and follows the push's true velocity: the band was GG's "weird border").
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The gust's air, pure (GustFxMath: HandsN19Tests).</summary>
    public static class GustFxMath
    {
        /// <summary>m a streak has travelled at `t` s, leaving at `v0` m/s with drag `drag` 1/s: v0 / k (1 - e^-kt).</summary>
        public static double Travel(double v0, double drag, double t) => drag > 0 ? v0 / drag * (1 - Math.Exp(-drag * t)) : v0 * t;

        /// <summary>m/s a streak moves at `t` s: v0 e^-kt.</summary>
        public static double Speed(double v0, double drag, double t) => v0 * Math.Exp(-drag * t);

        /// <summary>A ring's radius at `t` (0..1 of its life): r0 to r1 on an ease-out (1 - (1 - t)^2).</summary>
        public static double RingRadius(double t, double r0, double r1)
        {
            double u = t < 0 ? 0 : t > 1 ? 1 : t;
            return r0 + (r1 - r0) * (1 - (1 - u) * (1 - u));
        }

        /// <summary>A ring's brightness at `t`: GustRingAlpha x (1 - t)^1.5; 0 from its end.</summary>
        public static double RingAlpha(double t)
        {
            if (t >= 1) return 0;
            double u = 1 - (t < 0 ? 0 : t);
            return UnityTuning.GustRingAlpha * u * Math.Sqrt(u);
        }

        /// <summary>
        /// A streak's direction inside the cone around `dir` (unit; `right`, `up` its unit basis across): uniform over the
        /// cone's disc (the angle from the aim x sqrt(u01) of `halfAngleDeg`, around it by v01).
        /// </summary>
        public static Vector3 ConeDir(Vector3 dir, Vector3 right, Vector3 up, double halfAngleDeg, double u01, double v01)
        {
            double a = halfAngleDeg * Math.PI / 180 * Math.Sqrt(Math.Max(0, Math.Min(1, u01)));
            double around = 2 * Math.PI * v01;
            Vector3 across = right * (float)Math.Cos(around) + up * (float)Math.Sin(around);
            return (dir * (float)Math.Cos(a) + across * (float)Math.Sin(a)).normalized;
        }

        /// <summary>Wake streaks a second, `sinceGust` s after it, the body moving `speedAlongPush` m/s along the push: 0 outside
        /// GustWakeTime or under GustWakeMinSpeed, rising to GustWakeRate at twice the minimum.</summary>
        public static double WakeRate(double sinceGust, double speedAlongPush)
        {
            if (!(sinceGust >= 0) || sinceGust > UnityTuning.GustWakeTime || !(speedAlongPush >= UnityTuning.GustWakeMinSpeed)) return 0;
            return UnityTuning.GustWakeRate * Math.Min(1, speedAlongPush / (2 * UnityTuning.GustWakeMinSpeed));
        }

        /// <summary>m a streak shows at `speed` m/s: GustStreakStretch s of travel, at least GustStreakMinLen.</summary>
        public static double StreakLength(double speed) => Math.Max(UnityTuning.GustStreakMinLen, speed * UnityTuning.GustStreakStretch);

        /// <summary>A streak's brightness at `age` of `life`: whole for the first half, then down to 0.</summary>
        public static double StreakFade(double age, double life)
        {
            if (!(life > 0) || age >= life) return 0;
            double u = age / life;
            return u <= 0.5 ? 1 : (1 - u) / 0.5;
        }

        /// <summary>The gust's FOV kick (deg) at `age` s: whole on its first frame, back to 0 over GustFovBackTime (smoothstep),
        /// x GustFovAirMult in the air. CameraFeel scales it by the FOV KICK setting and stills it with REDUCED MOTION.</summary>
        public static double FovKick(double age, bool air)
        {
            if (!(age >= 0) || age >= UnityTuning.GustFovBackTime) return 0;
            double u = age / UnityTuning.GustFovBackTime;
            return UnityTuning.GustFovKickDeg * (air ? UnityTuning.GustFovAirMult : 1) * (1 - u * u * (3 - 2 * u));
        }
    }

    public sealed partial class MovementFx
    {
        struct GustAir
        {
            public bool live, puff;
            public bool shock; // NIGHT20: the THUNDERCLAP's shockwave (FeelFx.Shockwave), not NIGHT19's gust
            public float scale; // the rings' and the swirl's size (1 for a gust)
            public double age;
            public Vector3 muzzle, dir, feet;
        }

        struct GustStreak
        {
            public bool live, shock;
            public Vector3 start, dir;
            public double v0, age, life;
        }

        struct WakeLine
        {
            public Vector3 a, b;
            public float age, life;
        }

        readonly GustAir[] gustAirs = new GustAir[UnityTuning.GustPool];
        readonly GustStreak[] gustStreaks = new GustStreak[UnityTuning.GustPool * UnityTuning.GustStreaks];
        readonly WakeLine[] wakeLines = new WakeLine[64];
        int nextGust, nextStreak, nextWake;
        FxSprites gustTrace, gustRing, gustTwirl;
        Mulberry32 gustRng;
        double gustCone, sinceGust = double.PositiveInfinity, wakeBudget;
        Vector3 gustPush; // Unity space, unit: the push on the body (opposite the aim)
        bool gustWarm;

        void GustBuild()
        {
            GameConfig cfg = game.cfg;
            gustCone = cfg.Has("weapons.specials.shotgun.shock.coneDeg") ? cfg.Num("weapons.specials.shotgun.shock.coneDeg") : 30; // the sim's cone (the web's half-angle)
            gustRng = new Mulberry32(game.runSeed ^ UnityTuning.GustFxSeedSalt);
            Material neon = game.mats.neon;
            gustTrace = new FxSprites(neon, UnityTuning.KenneyTrace, gustStreaks.Length + wakeLines.Length);
            gustRing = new FxSprites(neon, UnityTuning.KenneyRing, UnityTuning.GustPool * 2);
            gustTwirl = new FxSprites(neon, UnityTuning.KenneyTwirl, UnityTuning.GustPool);
            game.events.On<GustEvent>(N19Events.Gust, OnGust);
            game.events.On(N20Events.Shockwave, OnShockwave); // NIGHT20 U3-34.3: the THUNDERCLAP's shockwave
            game.events.On(N20Events.ConeWave, OnShockwave); // the web's cone wave on the same step: one air (deduped by frame)
        }

        int shockFrame = -1;

        /// <summary>NIGHT20: the bus's shockwave (or its cone wave): the gust's air, bigger, once per frame.</summary>
        void OnShockwave(object payload)
        {
            if (!(payload is ShockwaveEvent e) || e.owner != "player") return;
            if (shockFrame == Time.frameCount) return; // the shockwave and its cone wave in one step: one air
            shockFrame = Time.frameCount;
            LayAir(e.origin, e.dir, true);
        }

        void GustClear()
        {
            gustRng = new Mulberry32(game.runSeed ^ UnityTuning.GustFxSeedSalt);
            for (int i = 0; i < gustAirs.Length; i++) gustAirs[i].live = false;
            for (int i = 0; i < gustStreaks.Length; i++) gustStreaks[i].live = false;
            for (int i = 0; i < wakeLines.Length; i++) wakeLines[i].life = 0;
            sinceGust = double.PositiveInfinity;
            wakeBudget = 0;
        }

        /// <summary>The player's cyan ladder (UiTheme once the HUD made it; colors.usable before).</summary>
        Color GustCyan => UiTheme.Ready ? UiTheme.Cyan : usable;

        Color GustPale => UiTheme.Ready ? UiTheme.CyanPale : Color.Lerp(usable, Color.white, 0.5f);

        /// <summary>The bus's gust (the sim's step): the burst laid out now; drawn from the next frame.</summary>
        void OnGust(GustEvent e)
        {
            if (e == null || e.owner != "player") return;
            LayAir(e.origin, e.dir, false);
        }

        /// <summary>
        /// The air of a gust (NIGHT19) or of the THUNDERCLAP's shockwave (NIGHT20, `shock`: bigger, FeelFx.Shockwave): the
        /// cone's streaks from the muzzle, the two rings, the swirl under a launch, the wake's first handful.
        /// </summary>
        void LayAir(Vec3 origin, Vec3 aim, bool shock)
        {
            Vector3 dir = WebSpace.ToUnity(aim);
            if (dir.sqrMagnitude < 1e-8f) return;
            dir.Normalize();
            Vector3 eye = WebSpace.ToUnity(origin);
            Vector3 muzzle = game.viewmodel != null && game.viewmodel.MuzzleWorld(out Vector3 m) ? m : eye + dir * 0.5f;
            ref GustAir g = ref gustAirs[nextGust];
            nextGust = (nextGust + 1) % gustAirs.Length;
            g.live = true;
            g.age = 0;
            g.muzzle = muzzle;
            g.dir = dir;
            g.shock = shock;
            g.scale = (float)(shock ? UnityTuning.ShockRingScale : 1);
            g.puff = -aim.y > UnityTuning.GustPuffMinUp; // aimed down: the push lifts the body (a gust-jump)
            g.feet = g.puff && game.world != null && game.world.RayIntersect(origin, aim, out RayHit hit) && hit.distance < 6
                ? WebSpace.ToUnity(origin + aim * hit.distance) + Vector3.up * 0.05f
                : WebSpace.ToUnity(game.mover.position) + Vector3.up * 0.05f;
            Vector3 right = Vector3.Cross(Vector3.up, dir);
            if (right.sqrMagnitude < 1e-6f) right = Vector3.Cross(Vector3.forward, dir);
            right.Normalize();
            Vector3 up = Vector3.Cross(dir, right);
            double half = gustCone * UnityTuning.GustConeShare;
            int count = shock ? (int)Math.Round(UnityTuning.GustStreaks * UnityTuning.ShockStreakMult) : UnityTuning.GustStreaks;
            double speed = UnityTuning.GustStreakSpeed * (shock ? UnityTuning.ShockStreakSpeedMult : 1);
            for (int k = 0; k < count; k++)
            {
                ref GustStreak s = ref gustStreaks[nextStreak];
                nextStreak = (nextStreak + 1) % gustStreaks.Length;
                Vector3 d = GustFxMath.ConeDir(dir, right, up, half, gustRng.Next(), (k + gustRng.Next()) / count);
                double around = gustRng.Next() * Math.PI * 2;
                Vector3 scatter = (right * (float)Math.Cos(around) + up * (float)Math.Sin(around)) * (float)(UnityTuning.GustStreakRadial * gustRng.Next());
                s.live = true;
                s.shock = shock;
                s.start = muzzle + d * (float)UnityTuning.GustStreakStart + scatter;
                s.dir = d;
                s.v0 = speed * (1 + (gustRng.Next() * 2 - 1) * UnityTuning.GustStreakSpeedJitter);
                s.age = 0;
                s.life = UnityTuning.GustStreakLife * (1 + (gustRng.Next() * 2 - 1) * UnityTuning.GustStreakLifeJitter);
            }
            if (!shock || !SpeedLines20.Live)
            {
                sinceGust = 0;
                gustPush = -dir;
                wakeBudget = Math.Max(wakeBudget, 3); // a first handful at once
            }
            FeelProbe.Event(shock ? UnityTuning.ProbeShockwave : UnityTuning.ProbeGust);
        }

        /// <summary>Frame (world time): the cone, the rings, the swirl, the wake; reported to the reel as FeelFx.Gust.</summary>
        void GustRender(double dt)
        {
            if (gustTrace == null || !gustTrace.Ready) return;
            if (!gustWarm && UiTheme.Ready)
            {
                gustWarm = true; // the cyan's materials made before the first gust (not on it)
                gustTrace.Warm(GustPale * UnityTuning.GustStreakHdr);
                gustTrace.Warm(GustPale * UnityTuning.GustWakeBrightness);
                gustRing.Warm(GustCyan * UnityTuning.GustRingHdr);
                gustTwirl.Warm(GustCyan * UnityTuning.GustPuffHdr);
            }
            Camera view = game.cam;
            if (view == null) return;
            SettingsConfig st = game.cfg.movement.settings;
            float intensity = (float)Math.Max(0, Math.Min(game.cfg.movement.feel.effectsIntensityMax, st.effectsIntensity));
            gustTrace.Begin();
            gustRing.Begin();
            gustTwirl.Begin();
            Vector3 eye = view.transform.position;
            Color pale = GustPale * UnityTuning.GustStreakHdr, cyan = GustCyan * UnityTuning.GustRingHdr, swirl = GustCyan * UnityTuning.GustPuffHdr;
            bool drew = false;
            for (int i = 0; i < gustAirs.Length; i++)
            {
                ref GustAir g = ref gustAirs[i];
                if (!g.live) continue;
                g.age += dt;
                double end = Math.Max(Math.Max(UnityTuning.GustRingLife, UnityTuning.GustRing2Life), g.puff ? UnityTuning.GustPuffLife : 0);
                if (g.age >= end)
                {
                    g.live = false;
                    continue;
                }
                if (intensity <= 0) continue;
                FeelFx probe = g.shock ? FeelFx.Shockwave : FeelFx.Gust; // NIGHT20: the shockwave's own channel
                float sc = g.scale > 0 ? g.scale : 1;
                double t1 = g.age / UnityTuning.GustRingLife;
                if (t1 < 1)
                {
                    Vector3 c = g.muzzle + g.dir * (float)UnityTuning.GustRingAhead;
                    float r = (float)GustFxMath.RingRadius(t1, UnityTuning.GustRingStartR, UnityTuning.GustRingEndR) * sc;
                    float a = (float)GustFxMath.RingAlpha(t1) * intensity;
                    gustRing.Disc(c, g.dir, r / UnityTuning.KenneyRingOuter, (float)(g.age * 2), cyan, a);
                    FeelProbe.EffectWorld(probe, c, r, a);
                    drew = true;
                }
                double t2 = g.age / UnityTuning.GustRing2Life;
                if (t2 < 1)
                {
                    float along = (float)GustFxMath.RingRadius(t2, UnityTuning.GustRing2From, UnityTuning.GustRing2To) * sc;
                    Vector3 c = g.muzzle + g.dir * along;
                    float r = (float)GustFxMath.RingRadius(t2, UnityTuning.GustRing2StartR, UnityTuning.GustRing2EndR) * sc;
                    float a = (float)GustFxMath.RingAlpha(t2) * intensity * 0.8f;
                    gustRing.Disc(c, g.dir, r / UnityTuning.KenneyRingOuter, (float)(-g.age * 2), cyan, a);
                    FeelProbe.EffectWorld(probe, c, r, a);
                    drew = true;
                }
                if (g.puff)
                {
                    double t3 = g.age / UnityTuning.GustPuffLife;
                    if (t3 < 1)
                    {
                        float r = (float)GustFxMath.RingRadius(t3, UnityTuning.GustPuffStartR, UnityTuning.GustPuffEndR) * (g.shock ? (float)UnityTuning.ShockPuffScale : 1);
                        float a = (float)GustFxMath.RingAlpha(t3) * intensity;
                        float spin = (float)(UnityTuning.GustPuffSpinDeg * Mathf.Deg2Rad * t3);
                        gustTwirl.Disc(g.feet, Vector3.up, r, spin, swirl, a);
                        FeelProbe.EffectWorld(probe, g.feet, r, a);
                        drew = true;
                    }
                }
            }
            for (int i = 0; i < gustStreaks.Length; i++)
            {
                ref GustStreak s = ref gustStreaks[i];
                if (!s.live) continue;
                s.age += dt;
                if (s.age >= s.life)
                {
                    s.live = false;
                    continue;
                }
                if (intensity <= 0) continue;
                double dist = GustFxMath.Travel(s.v0, UnityTuning.GustStreakDrag, s.age);
                double speed = GustFxMath.Speed(s.v0, UnityTuning.GustStreakDrag, s.age);
                float len = (float)Math.Min(GustFxMath.StreakLength(speed), dist + UnityTuning.GustStreakStart);
                Vector3 head = s.start + s.dir * (float)dist, tail = head - s.dir * len;
                float k = (float)GustFxMath.StreakFade(s.age, s.life) * intensity;
                gustTrace.Streak(tail, head, (float)UnityTuning.GustStreakWidth, eye, pale, k);
                FeelProbe.EffectWorld(s.shock ? FeelFx.Shockwave : FeelFx.Gust, (head + tail) * 0.5f, len * 0.5f, k);
                drew = true;
            }
            GustWake(dt, view, intensity, st);
            Color wake = GustPale * UnityTuning.GustWakeBrightness;
            for (int i = 0; i < wakeLines.Length; i++)
            {
                ref WakeLine w = ref wakeLines[i];
                if (!(w.life > 0)) continue;
                w.age += (float)dt;
                if (w.age >= w.life)
                {
                    w.life = 0;
                    continue;
                }
                float k = (1 - w.age / w.life) * intensity;
                gustTrace.Streak(w.a, w.b, UnityTuning.GustWakeWidth * 2, eye, wake, k);
                FeelProbe.EffectWorld(FeelFx.Gust, (w.a + w.b) * 0.5f, UnityTuning.GustWakeLength * 0.5f, k);
                drew = true;
            }
            if (!drew) return;
            gustTrace.Draw();
            gustRing.Draw();
            gustTwirl.Draw();
        }

        /// <summary>The wake's new lines: in the view's outer band, laid along the body's velocity, still in the world.</summary>
        void GustWake(double dt, Camera view, float intensity, SettingsConfig st)
        {
            if (sinceGust < double.PositiveInfinity) sinceGust += dt;
            Mover mover = game.mover;
            Vector3 vel = WebSpace.ToUnity(mover.velocity);
            double along = Vector3.Dot(vel, gustPush);
            double rate = FxGates.MotionOn(st.reducedMotion, st.effectsIntensity) && game.Active ? GustFxMath.WakeRate(sinceGust, along) : 0;
            if (!(rate > 0) || vel.sqrMagnitude < 1e-4f)
            {
                wakeBudget = sinceGust < UnityTuning.GustWakeTime ? wakeBudget : 0;
                return;
            }
            Vector3 fwd = vel.normalized;
            wakeBudget += rate * dt;
            float tanV = Mathf.Tan(view.fieldOfView * 0.5f * Mathf.Deg2Rad), tanH = tanV * view.aspect;
            while (wakeBudget >= 1)
            {
                wakeBudget--;
                double angle = gustRng.Next() * Math.PI * 2;
                float ahead = UnityTuning.GustWakeAheadMin + (float)gustRng.Next() * (UnityTuning.GustWakeAheadMax - UnityTuning.GustWakeAheadMin);
                float r = UnityTuning.GustWakeEdgeMin + (float)gustRng.Next() * (UnityTuning.GustWakeEdgeMax - UnityTuning.GustWakeEdgeMin);
                Vector3 a = view.transform.TransformPoint(new Vector3((float)Math.Cos(angle) * tanH * r * ahead, (float)Math.Sin(angle) * tanV * r * ahead, ahead));
                ref WakeLine w = ref wakeLines[nextWake];
                nextWake = (nextWake + 1) % wakeLines.Length;
                w.a = a;
                w.b = a - fwd * UnityTuning.GustWakeLength;
                w.age = 0;
                w.life = UnityTuning.GustWakeLife;
            }
        }
    }
}
