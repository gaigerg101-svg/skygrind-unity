// PG3 builder V2 (view row design.n74.rim74; the pure half of Rim74.cs, pinned by Rim74Tests): which bodies wear the glow, how bright it is at a distance, and the boss key's envelope.
// No UnityEngine call here (the numbers are UnityTuning.PG3V2.cs's), so an EditMode test runs it without a scene.
using System;

namespace Skygrind.Game
{
    public static class Rim74Math
    {
        public const string Row = "rim74";

        static double Clamp01(double x) => x < 0 ? 0 : x > 1 ? 1 : x;

        static double Smooth(double e0, double e1, double x)
        {
            double t = Clamp01((x - e0) / (e1 - e0));
            return t * t * (3 - 2 * t);
        }

        /// <summary>The boss bodies' kinds (Core/Enemies/*: Bulwark, Brawl40, Crown32 / 37, Glacier26-28, Hulk26, Lantern27 / 36, Loom30 / 37, Monolith, Stag31 / 37, Wick37, Wyrm). A boss has the entrance key, not the per-enemy glow.</summary>
        public static bool IsBossKind(string kind)
        {
            switch (kind)
            {
                case "bulwark":
                case "brawler":
                case "crown":
                case "crown37":
                case "glacier":
                case "hulk":
                case "lantern":
                case "loom":
                case "boss":
                case "stag":
                case "wick":
                case "wyrm":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Does this kind wear the red-orange glow? Not THE RIVAL (magenta is the RIVAL's colour, CLAUDE.md rule 8), not the BURROWER (buried, it has its own trail view), not a boss (the key).</summary>
        public static bool WearsHalo(string kind) => !string.IsNullOrEmpty(kind) && kind != "rival" && kind != "burrower" && !IsBossKind(kind);

        /// <summary>The radial glow texture's value at `r` (0 at the sprite's centre, 1 at the half-size edge): (1 - r^2)^2, whole at the centre, nothing and no slope at the edge (no visible ring where the quad ends), and the same in every direction (the Kenney flare's lens streak is exactly what this is not).</summary>
        public static double GlowProfile(double r)
        {
            if (!(r < 1)) return 0;
            double q = 1 - Math.Max(0, r) * Math.Max(0, r);
            return q * q;
        }

        /// <summary>The glow's share of its peak at `dist` m: N74HaloNearShare at N74HaloNearM or nearer; 1 from N74HaloFarM; smooth between; gone at N74HaloCloseM0 and whole again from N74HaloCloseM1 (no halo at arm's length); fading to 0 over the last N74HaloMaxFadeM m before N74HaloMaxM, and 0 from there.</summary>
        public static double HaloShare(double dist)
        {
            if (!(dist < UnityTuning.N74HaloMaxM)) return 0;
            double near = UnityTuning.N74HaloNearShare;
            double share = near + (1 - near) * Smooth(UnityTuning.N74HaloNearM, UnityTuning.N74HaloFarM, dist);
            share *= Smooth(UnityTuning.N74HaloCloseM0, UnityTuning.N74HaloCloseM1, dist);
            return share * (1 - Smooth(UnityTuning.N74HaloMaxM - UnityTuning.N74HaloMaxFadeM, UnityTuning.N74HaloMaxM, dist));
        }

        /// <summary>The glow's size share by distance: N74HaloNearScale at 0 m, whole from N74HaloFullSizeM, smooth between.</summary>
        public static double HaloScale(double dist) => UnityTuning.N74HaloNearScale + (1 - UnityTuning.N74HaloNearScale) * Smooth(0, UnityTuning.N74HaloFullSizeM, dist);

        /// <summary>The crowd's thinning for `n` glows in view: whole up to N74HaloCrowdFree, then sqrt(free / n).</summary>
        public static double CrowdShare(int n) => n <= UnityTuning.N74HaloCrowdFree ? 1 : Math.Sqrt((double)UnityTuning.N74HaloCrowdFree / n);

        /// <summary>A glow's half height (m): N74HaloHalfHeightRatio x half the body's height, at most half of N74HaloMaxSizeM.</summary>
        public static double HaloHalfHeight(double height) => Math.Min(UnityTuning.N74HaloMaxSizeM * 0.5, Math.Max(0, height) * 0.5 * UnityTuning.N74HaloHalfHeightRatio);

        /// <summary>A glow's half width (m): N74HaloHalfWidthRadii x the body's radius, never wider than its half height (a tall pill's glow is an upright ellipse).</summary>
        public static double HaloHalfWidth(double radius, double height) => Math.Min(HaloHalfHeight(height), Math.Max(0, radius) * UnityTuning.N74HaloHalfWidthRadii);

        /// <summary>The boss swell's share (0..1): 0 at the settled level and under, 1 at the peak: what the wash in front of the boss follows (it is the swell only, not the settled aura).</summary>
        public static double KeySwell(double level) => Clamp01((level - UnityTuning.N74BossKeySettleLevel) / (1 - UnityTuning.N74BossKeySettleLevel));

        /// <summary>The wash's size (m) for a body `height` m tall seen from `dist` m: N74BossKeyWashSizePerHeight x the height, never over N74BossKeyWashMaxAngleDeg of view.</summary>
        public static double KeyWashSize(double height, double dist)
        {
            double size = Math.Max(0, height) * UnityTuning.N74BossKeyWashSizePerHeight;
            double cap = 2 * Math.Max(0, dist) * Math.Tan(UnityTuning.N74BossKeyWashMaxAngleDeg * 0.5 * Math.PI / 180);
            return Math.Min(size, cap);
        }

        /// <summary>A share moved toward its target at 1 / inS (up) or 1 / outS (down) a second; held at dt 0 (a hitstop).</summary>
        public static double Step(double cur, double target, double dt, double inS, double outS)
        {
            if (!(dt > 0)) return cur;
            double rate = target >= cur ? 1.0 / Math.Max(1e-4, inS) : 1.0 / Math.Max(1e-4, outS);
            double d = target - cur, m = rate * dt;
            return Math.Abs(d) <= m ? target : cur + Math.Sign(d) * m;
        }

        /// <summary>The key's level (0..1 of its peak) `t` s after the entrance: 0 before it; a smooth swell to 1 over N74BossKeyRiseS; held for N74BossKeyHoldS; then down to N74BossKeySettleLevel over N74BossKeySettleS, where it stays.</summary>
        public static double KeyLevel(double t)
        {
            if (t < 0) return 0;
            double rise = UnityTuning.N74BossKeyRiseS, hold = UnityTuning.N74BossKeyHoldS, settle = UnityTuning.N74BossKeySettleS;
            if (t < rise) return Smooth(0, rise, t);
            if (t < rise + hold) return 1;
            double s = Smooth(0, settle, t - rise - hold);
            return 1 + (UnityTuning.N74BossKeySettleLevel - 1) * s;
        }

        /// <summary>The seconds from the trigger to the settled level (the swell's length).</summary>
        public static double KeySwellS => UnityTuning.N74BossKeyRiseS + UnityTuning.N74BossKeyHoldS + UnityTuning.N74BossKeySettleS;

        /// <summary>The key light's range for a boss `height` m tall.</summary>
        public static double KeyRange(double height) => Math.Max(UnityTuning.N74BossKeyRangeMin, Math.Min(UnityTuning.N74BossKeyRangeMax, height * UnityTuning.N74BossKeyRangePerHeight));

        /// <summary>The glow behind the boss: its size for a body `height` m tall.</summary>
        public static double KeyGlowSize(double height) => Math.Min(UnityTuning.N74BossKeyGlowMaxSizeM, Math.Max(0, height) * UnityTuning.N74BossKeyGlowSizePerHeight);
    }
}
