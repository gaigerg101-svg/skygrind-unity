// NIGHT74 builder L (pads74): the pure parts of the jump pad's steadied particle stream (Pad74.cs has the view, the header there says what and why). No UnityEngine and no
// UnityTuning in here: every number comes in as an argument, so a console can run it (Pad74Tests).
using System;

namespace Skygrind.Game
{
    public static class Pad74Math
    {
        /// <summary>The row's name (read every frame: NightRows.ViewOn).</summary>
        public const string Row = "pads74";

        static float Clamp01(float x) => x < 0f ? 0f : x > 1f ? 1f : x;

        static float Smooth(float a, float b, float x)
        {
            if (b <= a) return x >= b ? 1f : 0f;
            float t = Clamp01((x - a) / (b - a));
            return t * t * (3f - 2f * t);
        }

        /// <summary>The sprite's value at normalised radius r (0 the centre, 1 the quad's edge): (1 - r^2)^2, 1 at the centre and exactly 0 at and past the edge (no step at the quad's border).</summary>
        public static float Falloff(float r)
        {
            if (r >= 1f) return 0f;
            float q = 1f - r * r;
            return q * q;
        }

        /// <summary>The brightness (0-1) of sheet frame i of n: the particle's life at the frame's middle comes up over `fadeIn` of it and goes over the last `fadeOut`, smoothstep both, whole between.</summary>
        public static float FrameLevel(int i, int n, float fadeIn, float fadeOut)
        {
            if (n <= 0) return 0f;
            int k = i < 0 ? 0 : i > n - 1 ? n - 1 : i;
            float t = (k + 0.5f) / n;
            return Smooth(0f, fadeIn, t) * (1f - Smooth(1f - fadeOut, 1f, t));
        }

        /// <summary>The sheet as RGBA32 bytes (one row of `frames` blobs of `texels` px, `texels` rows, row 0 first): rgb is level x falloff x 255 (alpha the same; the additive blend ignores it).
        /// The blob's radius is the frame's half width, so the frame's border texels are (nearly) 0 and a mip level never carries one frame's light into the next.</summary>
        public static byte[] Sheet(int texels, int frames, float fadeIn, float fadeOut)
        {
            int w = texels * frames;
            var px = new byte[w * texels * 4];
            float half = texels * 0.5f;
            for (int f = 0; f < frames; f++)
            {
                float level = FrameLevel(f, frames, fadeIn, fadeOut);
                for (int y = 0; y < texels; y++)
                    for (int x = 0; x < texels; x++)
                    {
                        float dx = (x + 0.5f - half) / half, dy = (y + 0.5f - half) / half;
                        float v = level * Falloff((float)Math.Sqrt(dx * dx + dy * dy));
                        byte b = (byte)(int)Math.Round(Clamp01(v) * 255f);
                        int o = (y * w + f * texels + x) * 4;
                        px[o] = px[o + 1] = px[o + 2] = px[o + 3] = b;
                    }
            }
            return px;
        }

        /// <summary>The pad beam's brightness factor (GG, gg-proxy-start #5: the cyan light columns are "blinding"): `scale` x a fade for a camera close to the column. camDistM is the camera's horizontal distance to the
        /// pad's axis; at or under nearM the beam is at `nearFloor` of its softened light, from farM it is whole, smoothstep between. Colour and presence stay (the factor multiplies the HDR the shipped beam is
        /// painted with, hue untouched; it is never 0). With the row off the view does not call this and paints the shipped number.</summary>
        public static float BeamFactor(float scale, double camDistM, float nearM, float farM, float nearFloor)
        {
            float t = Smooth(nearM, farM, (float)camDistM);
            return scale * (nearFloor + (1f - nearFloor) * t);
        }

        /// <summary>The steady stream's random seed for pad `index`: the fixed base plus the index x 7919 (a prime, so two pads never share a seed).</summary>
        public static uint SeedFor(uint baseSeed, int index) => unchecked(baseSeed + (uint)index * 7919u);
    }
}
