// NIGHT29 item 4 (builder V): the CRT overlay's pure rules (Crt29Tests; Shaders/Resources/Crt29.shader mirrors each one) and
// the frame's values the pass reads (Crt29.Set, called by Crt29Driver). View only: no sim path, nothing from the sim's
// random stream. The grain is a fixed integer hash of the pixel and the flicker step (never UnityEngine.Random), so a
// capture at the same time shows the same grain. Look numbers: design.n29.crt (Design/unity-config.json); plumbing:
// UnityTuning.Crt29.cs. Order in the shader (R4b): pixel step -> warp -> sample with split -> scanlines -> grain -> vignette.
using UnityEngine;

namespace Skygrind.Game
{
    public static class Crt29Math
    {
        static float Clamp01(float x) => x < 0 ? 0 : x > 1 ? 1 : x;

        /// <summary>
        /// The warp: a screen position c (-1..1 on both axes) to where the image is sampled. A barrel pull of `k` at the
        /// corners (0.02 = 2 %), the image zoomed by 1 / (1 + k) so the corners land exactly on the edge (no black border):
        /// c x (1 + k r^2 / 2) / (1 + k). k 0 is the identity.
        /// </summary>
        public static Vector2 Warp(Vector2 c, float k)
        {
            float r2 = c.x * c.x + c.y * c.y;
            return c * ((1 + k * r2 * 0.5f) / (1 + k));
        }

        /// <summary>The chromatic split's uv offset at a warped position: 0 in the middle, `chromaUv` at the corner (grows with r^2).</summary>
        public static Vector2 ChromaOffset(Vector2 cw, float chromaUv)
        {
            float r2 = cw.x * cw.x + cw.y * cw.y;
            return cw * (chromaUv * r2 * 0.5f);
        }

        /// <summary>The scanline multiplier at a (warped) uv.y: 1 on a line's bright half, 1 - intensity on its dark half, brightness-compensated.</summary>
        public static float Scan(float v, float count, float intensity)
        {
            float s = 0.5f + 0.5f * Mathf.Sin(v * count * 6.2831853f);
            return (1 - intensity * (1 - s)) / (1 - intensity * UnityTuning.CrtScanMean);
        }

        /// <summary>PCG-style integer hash (the shader's `Hash`, bit for bit).</summary>
        public static uint Hash(uint x)
        {
            unchecked
            {
                x ^= x >> 16;
                x *= 0x7feb352du;
                x ^= x >> 15;
                x *= 0x846ca68bu;
                x ^= x >> 16;
                return x;
            }
        }

        /// <summary>The grain at a pixel on a flicker step: 0 to 1, a fixed function of (x, y, step).</summary>
        public static float Noise(uint x, uint y, uint step)
        {
            uint h = Hash(x ^ Hash(y ^ Hash(step)));
            return (h & 0xFFFFFFu) / 16777216f;
        }

        /// <summary>The flicker step at a real time: floor(t x hz), wrapped; 0 (steady grain) when `frozen` (REDUCED MOTION) or hz is 0.</summary>
        public static uint GrainStep(double t, double hz, bool frozen)
        {
            if (frozen || !(hz > 0) || !(t > 0)) return 0;
            return (uint)(System.Math.Floor(t * hz) % UnityTuning.CrtGrainWrap);
        }

        /// <summary>The vignette multiplier at distance `r` from the centre (centre 0, corner 1.414): 1 inside, 1 - amount at the corner.</summary>
        public static float Vignette(float r, float amount)
        {
            float t = Clamp01((r - UnityTuning.CrtVignetteFrom) / (UnityTuning.CrtVignetteTo - UnityTuning.CrtVignetteFrom));
            return 1 - amount * (t * t * (3 - 2 * t));
        }

        /// <summary>The setting's strength (0-1) as the pass uses it: 0 with the overlay switched off, else clamped.</summary>
        public static float Strength(double setting, bool overlayOn) => !overlayOn ? 0 : Clamp01((float)setting);

        /// <summary>The pixel step's block size in screen pixels at this height (0 = off): design.n29.crt.pixelStep px at 1440p, scaled.</summary>
        public static float PixelBlock(double pixelStep, double screenHeight) =>
            pixelStep > 0 ? (float)System.Math.Max(1, System.Math.Round(pixelStep * screenHeight / UnityTuning.CrtPixelRefHeight)) : 0;
    }

    /// <summary>This frame's CRT values, set by Crt29Driver and read by the pass (the pass is skipped when the strength is 0).</summary>
    public static class Crt29
    {
        public static Camera Target;
        public static float Strength; // 0-1: scales every effect; 0 = no pass
        public static float Warp, ScanIntensity, ScanCount, ChromaUv, Grain, Vignette, PixelStep;
        public static uint GrainStep;
        public static int Frame = -1;

        public static void Set(Camera cam, float strength, float warp, float scanIntensity, float scanCount, float chromaUv, float grain, float vignette, float pixelStep, uint grainStep)
        {
            Target = cam;
            Strength = strength;
            Warp = warp;
            ScanIntensity = scanIntensity;
            ScanCount = scanCount;
            ChromaUv = chromaUv;
            Grain = grain;
            Vignette = vignette;
            PixelStep = pixelStep;
            GrainStep = grainStep;
            Frame = Time.frameCount;
        }

        /// <summary>Should the pass run for `cam` this frame? (NIGHT30: also for THE LOOK's quantise with the CRT at 0.)</summary>
        public static bool On(Camera cam) => cam != null && cam == Target && (Strength > 0 || QuantStrength > 0) && Frame == Time.frameCount;

        // NIGHT30 item 8 (builder V, design.n30.look30): THE LOOK's world pass rides the CRT pass (after post-processing, before the HUD): the colour
        // quantise with an ordered 4x4 Bayer dither (Look30Math.Quantise, mirrored in Crt29.shader) and the grain halved in the dark. 0 = off.
        public static float QuantLevels, QuantDitherScale, QuantStrength, GrainDark;

        /// <summary>The look's values for this frame (Crt29Driver, after Set; QuantStrength 0 = no quantise, GrainDark 0 = the grain as NIGHT29's).</summary>
        public static void SetLook30(float levels, float ditherScale, float strength, float grainDark)
        {
            QuantLevels = levels;
            QuantDitherScale = ditherScale;
            QuantStrength = strength;
            GrainDark = grainDark;
        }

        // NIGHT35 V (design.n35.look35): the PS2 pass on top of THE LOOK: Lines35 virtual lines drawn by point upscale (0 = off), a flat red / blue split
        // across the whole screen (uv x), the quantise in sRGB on the virtual-pixel grid (Srgb35 1). Set by Crt29Driver every frame (0s with the row off).
        public static float Lines35, FlatChroma35, Srgb35;

        // NIGHT35 V (design.n35.ridelook35): the wall-ride focus look's TV tear (0-1, Tear35; TearStep35 the fixed hash seed) and a sweeping bright
        // scanline band (Sweep35 strength, 0 off; SweepY35 its centre, uv y; SweepHalf35 its half height). Set by SlowAim27Driver (0s when off).
        public static float Tear35, TearStep35, Sweep35, SweepY35, SweepHalf35;

        /// <summary>look35's values for this frame (all 0 = the pass as NIGHT30's).</summary>
        public static void SetLook35(float lines, float flatChroma, float srgb)
        {
            Lines35 = lines;
            FlatChroma35 = flatChroma;
            Srgb35 = srgb;
        }

        /// <summary>ridelook35's values for this frame (all 0 = no tear, no sweep).</summary>
        public static void SetRide35(float tear, float tearStep, float sweep, float sweepY, float sweepHalf)
        {
            Tear35 = tear;
            TearStep35 = tearStep;
            Sweep35 = sweep;
            SweepY35 = sweepY;
            SweepHalf35 = sweepHalf;
        }
    }
}
