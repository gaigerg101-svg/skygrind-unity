// NIGHT12 U3-25.2 (light and lens: Look.cs, LightAndLens.cs, PostFx.cs, LightPools.cs, Motes.cs, BlobShadows.cs): the
// Unity-only render numbers, each with why the web has no value for it. The web's own numbers come from config.json
// (post, lightPool, motes, blobShadows, fog, fx.flash* / pop*, quality); the web's shader literals (grade.js, motes.js,
// blobShadows.js) that the URP Volume needs on the C# side are named here too, marked as such.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- post.js on the URP Volume ----------------------------------------------------------------------------

        // post.js's chromatic aberration is an offset in uv (pmndrs, radial modulation: at the screen corner red and blue
        // part by about 2.43 x the offset); URP's intensity 1 parts them by 0.0667 of the screen there (UberPost: 0.05 x
        // r^2 x 2/3). 2.43 / 0.0667 = 36: the web's offset x this is URP's intensity.
        public const float ChromaScale = 36f;

        // The web's vignette (pmndrs DEFAULT: smoothstep(0.8, offset x 0.799, d x (darkness + offset))) and URP's
        // ((1 - (3 i d)^2)^(5 s), non-rounded: both in uv) are different curves. Look.FitVignette picks URP's intensity and
        // smoothness by least squares at this many radii from the centre to the screen's corner (uv distance sqrt(0.5)),
        // searching a grid this fine twice (coarse, then around the best cell); once per darkness, at construction.
        public const int VignetteFitSamples = 8;
        // The pmndrs vignette shader's own literals (vignette.frag: smoothstep(0.8, offset * 0.799, ...)), the curve fitted.
        public const double WebVignetteEdge = 0.8, WebVignetteOffsetScale = 0.799;
        public const float VignetteCorner = 0.70710678f;
        public const int VignetteFitGrid = 40;

        // grade.js's shader literals (not in config.post.grade): the shadow band smoothstep(0, 0.45, luma), the highlight
        // band smoothstep(0.35, 1, luma), the highlight tint's share (x 0.25) and how much low HP drains the saturation
        // (x (1 - 0.6 hurt)). URP's ShadowsMidtonesHighlights takes the bands as its limits.
        public const float GradeShadowsEnd = 0.45f;
        public const float GradeHighlightsStart = 0.35f;
        public const float GradeHighlightsEnd = 1f;
        public const float GradeHighlightShare = 0.25f;
        public const float GradeHurtDesaturate = 0.6f;

        // URP's Gaussian bloom adds more of its mip chain than pmndrs' mipmap blur at the same intensity: post.bloomIntensity
        // (1.1) x this is URP's 0.8, the intensity the port shipped with since NIGHT7 (UnityTuning.BloomIntensity, now unused).
        public const float BloomIntensityScale = 0.73f;

        // --- motes.js ----------------------------------------------------------------------------------------------

        // motes.js sizes a mote in pixels (gl_PointSize = size x ... x 30 / d) on the web's canvas, 1080 px tall at a
        // device ratio of 1; Motes.shader scales that by the real render target's height over this.
        public const float MoteRefHeight = 1080f;

        // --- blobShadows.js -----------------------------------------------------------------------------------------

        // The web lays an airborne body's blob at its own feet (faded to blobShadows.airAlpha); the port drops it to the
        // floor under it by one ray down (the C# collider), starting this far above the feet (m), and draws none past
        // BlobMaxDrop (m) of fall (an enemy over the void).
        public const double BlobRayLift = 0.25;
        public const double BlobMaxDrop = 12;

        // --- lightPool.js / effects.js lights ----------------------------------------------------------------------

        // The first-run guide's pool (port-only: NIGHT12 U3-25.11 onboarding, no web counterpart): its pulse (Hz) and how
        // low it dips (x lightPool.machine.intensity at the trough). Cyan (colors.usable: a thing to use).
        public const float GuidePulseRate = 1.2f;
        public const float GuidePulseMin = 0.35f;

        // effects.js pops its light on kills and rocket blasts only; NIGHT12's lead asked for a pop on world impacts too
        // (a port-only look, the family colour of the gun, sharing the flash light as the web's pops do). false: the web's.
        public const bool ImpactPops = true;
    }
}
