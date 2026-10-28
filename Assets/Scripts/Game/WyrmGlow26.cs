// NIGHT26 builder V: THE KILN WYRM's glow as pure functions (no engine state, no sim state): the view (WyrmVisual26) reads the sim's public
// state, fills a WyrmFxState26 and asks Evaluate for each piece: a glow (0 off .. 1 full emission, over 1 for a flash), a jaw glow (the head's
// jaws, from the baked vertex colour), a hue (0 red-orange .. 1 amber), a white-hot flash, a burn-out and an aura strength. EditMode tests pin every
// curve (Tests/EditMode/Game/WyrmGlow26Tests.cs). Numbers: UnityTuning.Wyrm26Fx.cs. Red-orange only (rule 8); the white is the damage flash.
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The attacks and tells the glow tells apart (index into UnityTuning.WyrmFxTell*): the sim's bossTell names map onto them.</summary>
    public enum WyrmKind26
    {
        None = 0, Dive = 1, Lunge = 2, Spit = 3, Roar = 4, Squeeze = 5, Shed = 6,
    }

    /// <summary>One piece's look this frame.</summary>
    public struct WyrmPieceFx26
    {
        public float glow, jaw, hue, hot, burn, aura;
    }

    /// <summary>What the view read from the sim, in the view's units (seconds, cycles, 0..1).</summary>
    public struct WyrmFxState26
    {
        public int pieces;
        /// <summary>The idle wave's and the coil pulse's phase (cycles, accumulated by the view: the coil's rate changes, so the phase is an integral).</summary>
        public float idlePhase, coilPhase;
        /// <summary>0..1: how much the worm is coiled (blended in and out), and how near its release is (0 far .. 1 at it).</summary>
        public float coilWeight, approach;
        /// <summary>Seconds since the coil released (WyrmGlow26.None: it has not).</summary>
        public float releaseAge;
        public WyrmKind26 tellKind;
        /// <summary>The tell's progress 0 (just started) .. 1 (it resolves now), and its strobe phase (cycles).</summary>
        public float tellRamp, tellPhase;
        public WyrmKind26 impactKind;
        public float impactAge, trailAge;
        public bool lunging;
        public float hpFrac;
        /// <summary>Seconds since the finisher started; negative while the worm lives.</summary>
        public float deathAge;
        /// <summary>The first piece of the tail that the shed cuts loose (the tail's tell lights from here down).</summary>
        public int tailFrom;
    }

    public static class WyrmGlow26
    {
        /// <summary>An age that means "never happened".</summary>
        public const float None = 1e9f;

        static float Wave(float cycles) => 0.5f + 0.5f * Mathf.Sin(cycles * 6.2831853f);

        /// <summary>NIGHT54 light54 (fix 10): the tell strobe's depth (1: NIGHT53's full 0..1 strobe); WyrmVisual26 sets it each frame from
        /// Light54.Calm with the row on (the 14 / 3.5 / 6 / 9 Hz strobes become a slow, shallower swell).</summary>
        public static float StrobeDepth54 = 1f;

        /// <summary>The idle pulse: a slow wave down the chain, one piece after another (the lag per piece makes it run from the head to the tail).</summary>
        public static float Idle(int piece, float phase) =>
            Mathf.Lerp(UnityTuning.WyrmFxIdleMin, UnityTuning.WyrmFxIdleMax, Wave(phase - piece * UnityTuning.WyrmFxIdlePerPiece));

        /// <summary>The coil's pulse rate in Hz: slow far from the release, fast at it (approach 0..1).</summary>
        public static float CoilRate(float approach) =>
            Mathf.Lerp(UnityTuning.WyrmFxCoilRateFar, UnityTuning.WyrmFxCoilRateNear, Mathf.Pow(Mathf.Clamp01(approach), UnityTuning.WyrmFxCoilRateCurve));

        /// <summary>The coiled pulse: brighter than the idle, its ceiling rising to the peak as the release nears, its floor rising with it.</summary>
        public static float Coil(int piece, float phase, float approach)
        {
            float a = Mathf.Clamp01(approach);
            float hi = Mathf.Lerp(UnityTuning.WyrmFxCoilBase, UnityTuning.WyrmFxCoilPeak, a);
            float lo = UnityTuning.WyrmFxCoilBase * Mathf.Lerp(0.5f, 1f, a);
            return Mathf.Lerp(lo, hi, Wave(phase - piece * UnityTuning.WyrmFxCoilPerPiece));
        }

        /// <summary>Seconds before the pattern's end -> approach 0..1 (1 at the release).</summary>
        public static float Approach(float secondsLeft) =>
            Mathf.Clamp01(1f - secondsLeft / Mathf.Max(0.01f, UnityTuning.WyrmFxCoilApproachSec));

        /// <summary>The release flash on every piece: strongest the moment the coil lets go, gone after WyrmFxReleaseSec; the tail end a little weaker.</summary>
        public static float Release(int piece, int pieces, float age)
        {
            if (age >= UnityTuning.WyrmFxReleaseSec || age < 0f) return 0f;
            float f = 1f - age / UnityTuning.WyrmFxReleaseSec;
            float along = pieces > 1 ? piece / (float)(pieces - 1) : 0f;
            return UnityTuning.WyrmFxReleaseGain * f * f * (1f - 0.5f * along) + (piece == 0 ? UnityTuning.WyrmFxReleaseHead * f : 0f);
        }

        /// <summary>The glow's dimming with the HP: 1 at full health, WyrmFxHpFloor at none.</summary>
        public static float HpDim(float hpFrac) => Mathf.Lerp(UnityTuning.WyrmFxHpFloor, 1f, Mathf.Clamp01(hpFrac));

        /// <summary>The white-hot flash of a hit: 1 at the hit, 0 after WyrmFxHotSec.</summary>
        public static float Hot(float age) => age < 0f || age >= UnityTuning.WyrmFxHotSec ? 0f : 1f - age / UnityTuning.WyrmFxHotSec;

        /// <summary>The head-to-tail front of the trail after a dive or a lunge starts, and the lit head end while the head lunges.</summary>
        public static float Trail(int piece, int pieces, float age, bool lunging)
        {
            float g = 0f;
            if (age >= 0f && age < UnityTuning.WyrmFxTrailSec)
            {
                float d = (piece - age * UnityTuning.WyrmFxTrailSpeed) / UnityTuning.WyrmFxTrailWidth;
                g += UnityTuning.WyrmFxTrailGain * Mathf.Exp(-d * d) * (1f - age / UnityTuning.WyrmFxTrailSec);
            }
            if (lunging && pieces > 1) g += UnityTuning.WyrmFxLungeTrail * Mathf.Pow(1f - piece / (float)(pieces - 1), 1.5f);
            return g;
        }

        /// <summary>The impact flash: the head (and its next two pieces, weaker) the moment a dive, lunge, volley, roar or squeeze lands.</summary>
        public static float Impact(int piece, float age)
        {
            if (age < 0f || age >= UnityTuning.WyrmFxImpactSec || piece > 2) return 0f;
            float f = 1f - age / UnityTuning.WyrmFxImpactSec;
            return UnityTuning.WyrmFxImpactGain * f * f * (piece == 0 ? 1f : UnityTuning.WyrmFxImpactNeighbour / piece);
        }

        /// <summary>The seconds from the finisher's start to the moment a piece is fully burnt out (head first: later pieces burn later).</summary>
        public static float BurnOutTime(int piece) => UnityTuning.WyrmFxFlareSec + piece * UnityTuning.WyrmFxBurnStagger + UnityTuning.WyrmFxBurnSec;

        /// <summary>How long the whole worm takes to burn out (the last piece's time).</summary>
        public static float DeathTotal(int pieces) => BurnOutTime(Mathf.Max(0, pieces - 1));

        /// <summary>The telegraph on a piece: the body's share (the head's whole-head gain, the spit's ports, the squeeze's chain, the tail's for the shed)
        /// and, for the head, the jaw's.</summary>
        public static void Telegraph(WyrmKind26 kind, int piece, int pieces, int tailFrom, float ramp, float phase, out float glow, out float jaw)
        {
            glow = jaw = 0f;
            int k = (int)kind;
            if (k <= 0 || ramp <= 0f) return;
            float r = Mathf.Clamp01(ramp);
            float rate = UnityTuning.WyrmFxTellRate[k];
            float strobe = rate > 0f ? (StrobeDepth54 >= 1f ? 0.5f + 0.5f * Mathf.Sin(phase * 6.2831853f) : 1f - StrobeDepth54 * (0.5f - 0.5f * Mathf.Sin(phase * 6.2831853f))) : 1f; // NIGHT54 light54: a shallower swell (1: NIGHT53's)
            float level = r * r * strobe;
            if (piece == 0)
            {
                glow = UnityTuning.WyrmFxTellHead[k] * level;
                jaw = UnityTuning.WyrmFxTellJaw[k] * level;
                return;
            }
            float body = UnityTuning.WyrmFxTellBody[k];
            if (kind == WyrmKind26.Spit)
            {
                bool port = piece >= 3 && (piece - 3) % 2 == 0; // Wyrm.Volley fires from pieces 3, 5, 7 ...
                glow = body * level * (port ? 1f : 0.2f);
            }
            else glow = body * level;
            if (piece >= tailFrom) glow += UnityTuning.WyrmFxTellTail[k] * level;
        }

        /// <summary>The hue a tell tints the glow with (0 red-orange .. 1 amber).</summary>
        public static float Hue(WyrmKind26 kind, float ramp) => kind == WyrmKind26.None ? 0f : UnityTuning.WyrmFxTellHue[(int)kind] * Mathf.Clamp01(ramp * 2f);

        /// <summary>One piece's look. `hot` is the piece's white-hot flash from the hits (WyrmGlow26.Hot of its own age, plus the neighbours' share).</summary>
        public static WyrmPieceFx26 Evaluate(in WyrmFxState26 s, int piece, float hot)
        {
            int n = s.pieces;
            var fx = new WyrmPieceFx26();
            float idle = Idle(piece, s.idlePhase);
            float coil = Coil(piece, s.coilPhase, s.approach);
            float baseGlow = Mathf.Lerp(idle, coil, Mathf.Clamp01(s.coilWeight)) * HpDim(s.hpFrac);
            float glow = baseGlow;
            float aura = s.coilWeight * UnityTuning.WyrmFxAuraCoil * s.approach * s.approach;
            float release = Release(piece, n, s.releaseAge);
            glow += release;
            aura += release * 0.6f;
            Telegraph(s.tellKind, piece, n, s.tailFrom, s.tellRamp, s.tellPhase, out float tg, out float jaw);
            glow += tg;
            fx.jaw = jaw;
            aura += (tg + jaw * 0.5f) * UnityTuning.WyrmFxAuraTell * 0.5f;
            float trail = Trail(piece, n, s.trailAge, s.lunging);
            glow += trail;
            aura += trail * 0.4f;
            float impact = Impact(piece, s.impactAge);
            glow += impact;
            aura += impact * 0.5f;
            fx.hue = Hue(s.tellKind, s.tellRamp);
            if (s.impactAge < UnityTuning.WyrmFxImpactSec) fx.hue = Mathf.Max(fx.hue, Hue(s.impactKind, 1f));
            fx.hot = hot;
            if (s.deathAge >= 0f)
            {
                float t = Mathf.Clamp01((s.deathAge - (UnityTuning.WyrmFxFlareSec + piece * UnityTuning.WyrmFxBurnStagger)) / UnityTuning.WyrmFxBurnSec);
                float flare = Mathf.Clamp01(s.deathAge / UnityTuning.WyrmFxFlareSec);
                float alive = 1f - t;
                glow = Mathf.Lerp(glow, 2.4f, flare) * alive;
                fx.jaw *= alive;
                fx.hot = Mathf.Max(hot, flare * alive * 0.7f);
                fx.burn = t;
                aura = Mathf.Lerp(aura, 1.2f, flare) * alive;
                fx.hue = Mathf.Lerp(fx.hue, 1f, flare);
            }
            fx.glow = Mathf.Clamp(glow, 0f, 4f);
            fx.aura = Mathf.Clamp(aura, 0f, 2f);
            return fx;
        }
    }
}
