// Port of skygrind/src/combat/momentumMath.js (p17-night6), line for line, the web's comments kept (NIGHT9 U3-07).
// Momentum damage math (OVERNIGHT2 N2). Pure: plain {x, y, z} objects, no three.js, unit tested. `m` is
// config.momentum.
// - Closing speed: the owner's velocity (smoothed over 60 ms) along the direction to the target.
// - M = clamp01((closing - 8) / (32 - 8)); a surge (speed gained in the last 0.35 s above 10 m/s: a dash, a slide start,
//   a boost jump, a slam rebound, a grapple release) adds 0.3, capped at 1.
// - Melee damage x lerp(1, 3, M); guns x lerp(1, 1.35, M); explosions grow their radius instead (+25% at M = 1).
// Port notes: `m` is a MomentumParams and impactWave's `w` (config.momentum.shock) an ImpactWaveParams, read once from
// GameConfig. smoothVelocity mutates `smooth` by ref (Vec3 is a struct) and returns it. surgeOf takes a ring and how
// many of its samples count (momentum.js passes `ring.subarray(0, filled)` until the ring is full: no copy here).
// Golden vectors: PortExport/vectors/momentumMath.json (Golden/GoldenPorted.cs). Web tests: tests/momentum.test.js.
using System;

namespace Skygrind.Core
{
    /// <summary>config.momentum: the numbers momentumMath.js (and momentum.js's ring) read.</summary>
    public sealed class MomentumParams
    {
        public double smoothTime, closingMin, closingMax, surgeWindow, surgeMin, surgeBonus, meleeMax, gunMax, blastRadius;

        public static MomentumParams From(GameConfig cfg, string path = "momentum") => new MomentumParams
        {
            smoothTime = cfg.Num(path + ".smoothTime"),
            closingMin = cfg.Num(path + ".closingMin"),
            closingMax = cfg.Num(path + ".closingMax"),
            surgeWindow = cfg.Num(path + ".surgeWindow"),
            surgeMin = cfg.Num(path + ".surgeMin"),
            surgeBonus = cfg.Num(path + ".surgeBonus"),
            meleeMax = cfg.Num(path + ".meleeMax"),
            gunMax = cfg.Num(path + ".gunMax"),
            blastRadius = cfg.Num(path + ".blastRadius"),
        };
    }

    /// <summary>config.momentum.shock: the numbers impactWave reads.</summary>
    public sealed class ImpactWaveParams
    {
        public double at, shareMin, shareMax, pushMin, pushMax;

        public static ImpactWaveParams From(GameConfig cfg, string path = "momentum.shock") => new ImpactWaveParams
        {
            at = cfg.Num(path + ".at"),
            shareMin = cfg.Num(path + ".shareMin"),
            shareMax = cfg.Num(path + ".shareMax"),
            pushMin = cfg.Num(path + ".pushMin"),
            pushMax = cfg.Num(path + ".pushMax"),
        };
    }

    /// <summary>impactWave's {fires, share, knockback}.</summary>
    public struct ImpactWaveResult
    {
        public bool fires;
        public double share;
        public double knockback;
    }

    public static class MomentumMath
    {
        static double Clamp01(double v) => JsMath.Max(0, JsMath.Min(1, v));

        /// <summary>Exponential smoothing of a velocity toward `v` with time constant `tau` (s) over `dt`. Writes `smooth`.</summary>
        public static Vec3 SmoothVelocity(ref Vec3 smooth, Vec3 v, double dt, double tau)
        {
            double k = tau > 0 ? 1 - JsMath.Exp(-dt / tau) : 1; // JsMath: exp
            smooth.x += (v.x - smooth.x) * k;
            smooth.y += (v.y - smooth.y) * k;
            smooth.z += (v.z - smooth.z) * k;
            return smooth;
        }

        /// <summary>The closing speed: `vel` along the unit direction from `from` to `to` (m/s; negative when moving away).</summary>
        public static double ClosingSpeed(Vec3 vel, Vec3 from, Vec3 to)
        {
            double dx = to.x - from.x;
            double dy = to.y - from.y;
            double dz = to.z - from.z;
            double d = JsMath.Hypot(dx, dy, dz);
            if (d < 1e-6) return JsMath.Hypot(vel.x, vel.y, vel.z);
            return (vel.x * dx + vel.y * dy + vel.z * dz) / d;
        }

        /// <summary>
        /// The surge: speed gained over the recent window, from a ring of speed samples (oldest first is not required: the
        /// minimum of the ring against the newest sample). `ring`: speeds (m/s) of the last `window` s, of which the first
        /// `length` count; `newest`: the latest speed.
        /// </summary>
        public static double SurgeOf(double[] ring, int length, double newest)
        {
            double min = double.PositiveInfinity;
            for (int i = 0; i < length; i++) if (ring[i] < min) min = ring[i];
            return double.IsPositiveInfinity(min) ? 0 : JsMath.Max(0, newest - min);
        }

        /// <summary>surgeOf over the whole ring.</summary>
        public static double SurgeOf(double[] ring, double newest) => SurgeOf(ring, ring.Length, newest);

        /// <summary>Momentum 0..1 from a closing speed and a surge.</summary>
        public static double MomentumOf(double closing, double surge, MomentumParams m)
        {
            double M = Clamp01((closing - m.closingMin) / (m.closingMax - m.closingMin));
            if (surge > m.surgeMin) M = JsMath.Min(1, M + m.surgeBonus);
            return M;
        }

        /// <summary>The damage multiplier of a hit at momentum M: melee lerp(1, meleeMax, M), guns lerp(1, gunMax, M).</summary>
        public static double MomentumDamage(double M, bool melee, MomentumParams m)
        {
            double top = melee ? m.meleeMax : m.gunMax;
            return 1 + (top - 1) * Clamp01(M);
        }

        /// <summary>The blast radius multiplier at momentum M: lerp(1, 1 + blastRadius, M).</summary>
        public static double MomentumRadius(double M, MomentumParams m) => 1 + m.blastRadius * Clamp01(M);

        /// <summary>
        /// A melee impact's shockwave (N2 §3.2) at momentum M: does it fire (M above impactAt and off cooldown), its damage
        /// share of the hit (lerp over M) and its knockback speed (lerp over M).
        /// </summary>
        public static ImpactWaveResult ImpactWave(double M, double cooldownLeft, ImpactWaveParams w)
        {
            bool fires = M > w.at && cooldownLeft <= 0;
            double k = Clamp01((M - w.at) / JsMath.Max(1e-6, 1 - w.at));
            return new ImpactWaveResult { fires = fires, share = w.shareMin + (w.shareMax - w.shareMin) * k, knockback = w.pushMin + (w.pushMax - w.pushMin) * k };
        }
    }
}
