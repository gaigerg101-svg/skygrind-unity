// NIGHT26 builder V: the KILN WYRM's glow, aura and animation (Game/WyrmVisual26.cs, Game/WyrmGlow26.cs, Shaders/Wyrm26*.shader).
// Render numbers only: the web has no worm, the sim never reads these (its numbers are design.n24.wyrm in PortExport / Design). Every glow is
// red-orange, the danger colour (CLAUDE.md rule 8: no cyan, no magenta); the one white is the damage flash ("white-hot", GG's word).
// A "glow" is 0 (off) .. 1 (the full emission) and may go over 1 for a flash; the shader multiplies it by WyrmFxEmission (HDR) so URP bloom
// (UnityTuning.BloomThreshold, 1) picks up everything above a glow of about 0.3.
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the material (Shaders/Wyrm26.shader) ---------------------------------------------------------------------------------
        /// <summary>The emission's HDR gain at glow 1 (the bloom threshold is 1, so 3.2 blooms hard); the hue of the glow (red-orange, the
        /// hot end amber for hue 1: still the danger family); the white-hot flash colour; the ash the burnt-out pieces fade to.</summary>
        public const float WyrmFxEmission = 4.5f;
        public const uint WyrmFxGlowHex = 0xff4a10, WyrmFxAmberHex = 0xff9a28, WyrmFxHotHex = 0xfff2d6, WyrmFxAshHex = 0x1a1210;
        /// <summary>The emission mask is read from the Tripo albedo: how red a texel is (r minus 1.6 g) from MaskLo (none) to MaskHi (full). The
        /// textures' seams, eyes and orbs are saturated red; the yellow patches and the grey armour read under MaskLo.</summary>
        public const float WyrmFxMaskLo = 0.08f, WyrmFxMaskHi = 0.32f;
        /// <summary>How much of the glow also lifts the armour's own colour (a faint warm wash) so a pulse reads on the whole piece, not only the seams.</summary>
        public const float WyrmFxWash = 0.12f;
        /// <summary>The procedural emission (Wyrm26.shader: seams, eyes, orbs that do not depend on the texture's colours): the head's eye in object space (abs x, y, z, radius;
        /// a guess from the head's silhouette: mirrored in x), and the height above which the body's top orbs glow. Retune from the close-up frames.</summary>
        public static readonly Vector4 WyrmFxEyeA = new Vector4(0.14f, 0.10f, 0.30f, 0.09f);
        public const float WyrmFxOrbY = 0.30f;
        /// <summary>The armour's lit floor (the dusk scene has almost no ambient or sun light: the first Player frames showed the lit armour as a black smudge; every
        /// other enemy is unlit), the red-orange rim on its silhouette (gain, power): the dark pieces read against the dusk even at glow 0.</summary>
        public const float WyrmFxAmbient = 0.55f, WyrmFxRim = 0.8f, WyrmFxRimPower = 2.5f;

        // --- the idle pulse (a slow wave down the chain, one piece after another) ------------------------------------------------------
        /// <summary>Cycles per second, the lag per piece (cycles: the wave runs head to tail), the glow's floor and ceiling.</summary>
        public const float WyrmFxIdleRate = 0.32f, WyrmFxIdlePerPiece = 0.045f, WyrmFxIdleMin = 0.20f, WyrmFxIdleMax = 0.55f;

        // --- the coil (coil and loop patterns): a brighter pulse that speeds up as the release nears ------------------------------------
        /// <summary>The coil's glow floor, and its peak at the release; the pulse's rate (Hz) far from the release and at it; the lag per piece (cycles);
        /// seconds before the pattern's end over which the approach rises 0 -> 1; the rate's curve (approach ^ this).</summary>
        public const float WyrmFxCoilBase = 0.45f, WyrmFxCoilPeak = 1.5f, WyrmFxCoilRateFar = 0.9f, WyrmFxCoilRateNear = 7f, WyrmFxCoilPerPiece = 0.03f;
        public const float WyrmFxCoilApproachSec = 3f, WyrmFxCoilRateCurve = 1.6f;
        /// <summary>Seconds the coil weight takes to blend in and out when the pattern changes (so the pulse never snaps).</summary>
        public const float WyrmFxCoilBlendSec = 0.35f;
        /// <summary>The release: the glow flash over every piece (gain, seconds of its decay; the head's extra).</summary>
        public const float WyrmFxReleaseGain = 2.0f, WyrmFxReleaseSec = 0.5f, WyrmFxReleaseHead = 0.6f;

        // --- the aura: an additive shell round every piece, a halo round the coil, an ember ring ------------------------------------------
        /// <summary>The shell's offset along the normal (m) and its fresnel power; the shell's glow at the coil's peak (x the approach);
        /// the shell's glow gain for the attacks' telegraphs.</summary>
        public const float WyrmFxShell = 0.28f, WyrmFxShellPower = 2.2f, WyrmFxAuraCoil = 0.85f, WyrmFxAuraTell = 0.9f;
        /// <summary>The coil's halo: a soft additive sphere round its centroid: its radius over the coil's own radius, its intensity at the peak.</summary>
        public const float WyrmFxHaloScale = 1.25f, WyrmFxHaloIntensity = 0.30f;
        /// <summary>The ember ring: how many (drawn as one instanced call), the ember's size (m), its radius over the coil's, how far above
        /// and below the coil's middle it drifts (m), its turn rate far from the release and at it (rad/s), its rise (m/s) and, at the release,
        /// the speed it flies outward (m/s) and for how long (s).</summary>
        public const int WyrmFxEmbers = 24;
        public const float WyrmFxEmberSize = 0.2f, WyrmFxEmberRadius = 1.05f, WyrmFxEmberSpread = 3.2f, WyrmFxEmberTurnFar = 0.5f, WyrmFxEmberTurnNear = 3.4f;
        public const float WyrmFxEmberRise = 0.55f, WyrmFxEmberBurst = 11f, WyrmFxEmberBurstSec = 0.7f, WyrmFxEmberGain = 2.4f;

        /// <summary>The halo and embers are skipped when the coil's centre is more than this (m) behind the camera (cost: no off-screen aura).</summary>
        public const float WyrmFxCullBehind = 6f;

        // --- the attacks: a telegraph glow on the head and jaws, a trail down the body, a flash on the impact -------------------------------
        // One row per kind (WyrmGlow26.Kind order: none, dive, lunge, spit, roar, squeeze, shed): the glow's hue (0 red-orange .. 1 amber), the
        // strobe rate (Hz; 0 steady: the dive rises steadily, the lunge strobes fast, the spit beats slowly), and the gains on the jaws, the whole head,
        // the body's ports (spit: the pieces the volley comes from) and the tail (shed).
        public static readonly float[] WyrmFxTellHue = { 0f, 0.15f, 0.85f, 0.45f, 0.05f, 0.30f, 0.60f };
        public static readonly float[] WyrmFxTellRate = { 0f, 0f, 14f, 3.5f, 6f, 0f, 9f };
        public static readonly float[] WyrmFxTellJaw = { 0f, 1.4f, 2.4f, 0.9f, 0.6f, 1.0f, 0.5f };
        public static readonly float[] WyrmFxTellHead = { 0f, 0.7f, 1.2f, 0.5f, 1.4f, 0.8f, 0.3f };
        public static readonly float[] WyrmFxTellBody = { 0f, 0.15f, 0.2f, 1.1f, 0.2f, 0.9f, 0.15f };
        public static readonly float[] WyrmFxTellTail = { 0f, 0f, 0f, 0f, 0f, 0.3f, 1.3f };
        /// <summary>The trail after a dive's or a lunge's start: a bright front runs head to tail (pieces per second, width in pieces, gain),
        /// for this many seconds; while the head lunges the head end stays lit (gain, falling off down the body).</summary>
        public const float WyrmFxTrailSpeed = 34f, WyrmFxTrailWidth = 3.2f, WyrmFxTrailGain = 1.6f, WyrmFxTrailSec = 1.4f, WyrmFxLungeTrail = 0.9f;
        /// <summary>The impact flash on the head and its two neighbours: gain, seconds, and the share the neighbours get.</summary>
        public const float WyrmFxImpactGain = 2.6f, WyrmFxImpactSec = 0.45f, WyrmFxImpactNeighbour = 0.5f;

        // --- damage -----------------------------------------------------------------------------------------------------------------
        /// <summary>The white-hot flash on a hit piece (seconds; its neighbours' share) and the glow's dimming as the HP falls (the floor at 0 HP).</summary>
        public const float WyrmFxHotSec = 0.16f, WyrmFxHotNeighbour = 0.35f, WyrmFxHpFloor = 0.30f;

        // --- death: every piece flares (white-hot), then burns out one by one, head to tail -------------------------------------------------
        /// <summary>Seconds of the flare, the head-to-tail delay between two pieces' burn-outs, how long one piece takes to burn out, and how
        /// long after the sim's death the pieces are still drawn (so the last ones can burn out).</summary>
        public const float WyrmFxFlareSec = 0.35f, WyrmFxBurnStagger = 0.07f, WyrmFxBurnSec = 0.55f, WyrmFxLingerSec = 2.6f;

        // --- the fit: the Tripo pieces are scaled onto the sim's boxes (a shot finds what you see) -------------------------------------------
        /// <summary>A piece's mesh is as long as its box (so a lunge, which stretches the boxes, never leaves a gap) times WyrmFxLengthFit; its
        /// width and height scale between the length's scale (0: keeps the model's proportions) and the box's width (1: fills the hit box).</summary>
        public const float WyrmFxLengthFit = 1.0f, WyrmFxWidthBlend = 0.5f;
        /// <summary>The sim's fixed rate (steps per second): the sim clock the tells and impacts are timed on (WyrmView.cs divides stepNo by the same 120).</summary>
        public const float WyrmFxStepHz = 120f;
    }
}
