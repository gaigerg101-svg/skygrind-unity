// NIGHT30 builder V (item 8, design.n30.look30): THE LOOK's pure rules (Look30Tests). Shaders/Resources/Crt29.shader mirrors Quantise and GrainScale
// line for line (the world pass); HudFx30.cs uses FlickerAlpha, BuzzOffset and the FlickerGate. No sim path, no random stream: the buzz is a fixed
// function of time.
using UnityEngine;

namespace Skygrind.Game
{
    public static class Look30Math
    {
        /// <summary>The 4x4 Bayer matrix (0..15), row-major.</summary>
        public static readonly int[] Bayer4 = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };

        /// <summary>The ordered-dither offset at a pixel: (Bayer / 16 + 1/32) - 0.5, so it spans -0.47..+0.47 of one level; the cell is `scale` px.</summary>
        public static float Dither(int px, int py, float scale)
        {
            int s = Mathf.Max(1, Mathf.RoundToInt(scale));
            int x = (px / s) & 3, y = (py / s) & 3;
            return (Bayer4[y * 4 + x] + 0.5f) / 16f - 0.5f;
        }

        /// <summary>One channel quantised to `levels` steps with a dither offset (in levels), blended by `strength` from the source.</summary>
        public static float Quantise(float c, float levels, float dither, float strength)
        {
            float n = Mathf.Max(2f, levels) - 1f;
            float q = Mathf.Floor(Mathf.Clamp01(c) * n + 0.5f + dither) / n;
            q = Mathf.Clamp01(q);
            return c + (q - c) * strength;
        }

        public static Color Quantise(Color c, float levels, float dither, float strength) =>
            new Color(Quantise(c.r, levels, dither, strength), Quantise(c.g, levels, dither, strength), Quantise(c.b, levels, dither, strength), c.a);

        /// <summary>The CRT grain's share at a luminance: Look30GrainDarkScale under Look30GrainDarkLuma, whole above it (a short smooth ramp between).</summary>
        public static float GrainScale(float luma)
        {
            float t = Mathf.Clamp01((luma - UnityTuning.Look30GrainDarkLuma * 0.5f) / (UnityTuning.Look30GrainDarkLuma * 0.5f));
            t = t * t * (3f - 2f * t);
            return Mathf.Lerp(UnityTuning.Look30GrainDarkScale, 1f, t);
        }

        /// <summary>PIXEL SIZE (1-4, 1 = off) as the CRT pass's pixel block in screen px: 0 when off, else the size at 1080p scaled to the screen (at least 2).</summary>
        public static float PixelBlock(double setting, double screenHeight)
        {
            if (!(setting > 1.0001)) return 0f;
            return (float)System.Math.Max(2, System.Math.Round(setting * screenHeight / 1080.0));
        }

        /// <summary>The palette (DESIGN30 E): cyan, red-orange, magenta, pale violet-white.</summary>
        public static readonly Color[] Palette = { new Color(0f, 0.9f, 1f), new Color(1f, 0.35f, 0.12f), new Color(1f, 0.2f, 0.85f), new Color(0.85f, 0.8f, 1f) };

        /// <summary>The smallest RGB distance between any two palette colours after the quantise, over every dither offset of the Bayer matrix.</summary>
        public static float PaletteMinGap(float levels, float strength)
        {
            float min = float.MaxValue;
            for (int i = 0; i < Palette.Length; i++)
                for (int j = i + 1; j < Palette.Length; j++)
                    for (int a = 0; a < 16; a++)
                        for (int b = 0; b < 16; b++)
                        {
                            Color p = Quantise(Palette[i], levels, (Bayer4[a] + 0.5f) / 16f - 0.5f, strength);
                            Color q = Quantise(Palette[j], levels, (Bayer4[b] + 0.5f) / 16f - 0.5f, strength);
                            float d = Mathf.Sqrt((p.r - q.r) * (p.r - q.r) + (p.g - q.g) * (p.g - q.g) + (p.b - q.b) * (p.b - q.b));
                            if (d < min) min = d;
                        }
            return min;
        }

        /// <summary>A flicker's overlay alpha on its frame `f` (0-based) of `frames`: bright, dark, bright ... then the rest alpha.</summary>
        public static float FlickerAlpha(int f, int frames, float rest)
        {
            if (f < 0 || f >= frames) return rest;
            return f % 2 == 0 ? UnityTuning.Look30FlickerHigh : UnityTuning.Look30FlickerLow;
        }

        /// <summary>The buzz's horizontal offset (whole px, -Look30BuzzPx..+Look30BuzzPx) at time t: a 12 Hz step through a fixed pattern.</summary>
        public static float BuzzOffset(double t)
        {
            int step = (int)System.Math.Floor(t * UnityTuning.Look30BuzzHz);
            int k = ((step * 7) % 5 + 5) % 5; // 0..4, a fixed walk (no Random)
            return (k - 2) * (UnityTuning.Look30BuzzPx / 2f);
        }

        /// <summary>The glow's alpha `age` s after a state change (rest alpha after Look30GlowSeconds).</summary>
        public static float GlowAlpha(double age, float rest)
        {
            if (age < 0 || age >= UnityTuning.Look30GlowSeconds) return rest;
            float u = (float)(age / UnityTuning.Look30GlowSeconds);
            return Mathf.Lerp(UnityTuning.Look30GlowAlpha, rest, u * u);
        }
    }

    /// <summary>One flicker on the HUD at a time, and at most one start per Look30FlickerGap s (3 a second): R2's safety rules.</summary>
    public sealed class FlickerGate30
    {
        double lastStart = double.NegativeInfinity;
        int owner = -1, framesLeft;

        /// <summary>May panel `id` start a flicker at `now`? True starts it (Look30FlickerFrames frames, counted by Frame).</summary>
        public bool TryStart(int id, double now)
        {
            if (framesLeft > 0 || now - lastStart < UnityTuning.Look30FlickerGap) return false;
            lastStart = now;
            owner = id;
            framesLeft = UnityTuning.Look30FlickerFrames;
            return true;
        }

        /// <summary>The flickering panel's frame index this frame (0-based) and advances; -1 when `id` is not flickering.</summary>
        public int Frame(int id)
        {
            if (id != owner || framesLeft <= 0) return -1;
            int f = UnityTuning.Look30FlickerFrames - framesLeft;
            framesLeft--;
            if (framesLeft == 0) owner = -1;
            return f;
        }

        public bool Busy => framesLeft > 0;
    }

    /// <summary>NIGHT35 V (design.n35.look35 / ridelook35): the PS2 pass's and the ride look's pure rules (Crt29.shader mirrors Lines and the flat split).</summary>
    public static class Look35Math
    {
        /// <summary>The virtual lines at this screen height: Look35Lines, fewer on a short screen (each block at least Look35MinBlockPx px), and fewer
        /// again when PIXEL SIZE asks for bigger blocks (`pixelBlock` screen px, 0 = off). 0 when the height is unknown.</summary>
        public static float Lines(float want, double screenHeight, float pixelBlock)
        {
            if (!(screenHeight > 0) || !(want > 0)) return 0f;
            double lines = System.Math.Min(want, screenHeight / UnityTuning.Look35MinBlockPx);
            if (pixelBlock > 1) lines = System.Math.Min(lines, screenHeight / pixelBlock);
            return (float)System.Math.Max(1, System.Math.Round(lines));
        }

        /// <summary>The flat split in uv x: `px` virtual pixels at `lines` lines on a screen of this aspect (w / h).</summary>
        public static float FlatChromaUv(float px, float lines, double aspect) => lines > 0 && aspect > 0 ? (float)(px / (lines * aspect)) : 0f;

        /// <summary>The CRT setting's gain on the look35 numbers: setting / its default, 0..Look35GainMax (0 with the overlay switched off).</summary>
        public static float Gain(float strength, double defaultStrength)
        {
            if (!(strength > 0)) return 0f;
            float g = defaultStrength > 1e-6 ? (float)(strength / defaultStrength) : 1f;
            return g > UnityTuning.Look35GainMax ? UnityTuning.Look35GainMax : g;
        }
    }

    /// <summary>NIGHT35 V (design.n35.ridelook35): the wall-ride focus look's pure rules (SlowAim27.Focus drives the Volume and the CRT pass with them).</summary>
    public static class Ride35Math
    {
        /// <summary>One eased step of the focus depth toward `target`: in over Ride35InS, out over Ride35OutS (linear, real seconds).</summary>
        public static float Ease(float current, bool target, float dt)
        {
            if (!(dt > 0)) return current;
            return target ? Mathf.Min(1f, current + dt / (float)UnityTuning.Ride35InS) : Mathf.Max(0f, current - dt / (float)UnityTuning.Ride35OutS);
        }

        /// <summary>The CRT pass's glitch at focus depth k and real time `now`: the tear (amount, its hash step at Ride35TearHz) and the sweep (strength, its
        /// centre falling top to bottom once per Ride35SweepPeriod s). REDUCED MOTION: no tear, the sweep held still at the screen's top third.</summary>
        public static void Glitch(float k, double now, bool reducedMotion, out float tear, out float step, out float sweep, out float sweepY)
        {
            k = Mathf.Clamp01(k);
            step = (float)(System.Math.Floor(now * UnityTuning.Ride35TearHz) % UnityTuning.CrtGrainWrap);
            tear = reducedMotion ? 0f : UnityTuning.Ride35Tear * k;
            sweep = UnityTuning.Ride35Sweep * k;
            double ph = now / UnityTuning.Ride35SweepPeriod;
            sweepY = reducedMotion ? 0.66f : (float)(1.0 - (ph - System.Math.Floor(ph))); // uv y is 1 at the top: the band falls down the screen
        }
    }
}
