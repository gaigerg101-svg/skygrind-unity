// NIGHT74 builder L (L1, the moving-camera flicker meter): the pure part of the pan74 reel's stack file and its motion compensation (no UnityEngine call, so a
// scratch console and the EditMode test compile it alone). The reel (FeelReel.N74.L.cs) turns the camera through a fixed slow yaw sweep at a room's first card
// pose and writes, for every rendered frame, per 8 x 8 pixel block: (1) the SUM of the luma, and (2) the SUM of the motion-compensated RESIDUAL
// r = luma(this frame) - luma(the previous frame warped to this camera). Why the residual: a thin bright line or a hard edge crossing a block makes its mean
// luma rise and fall (a sawtooth on a drifting background), which a flip-flop test on the plain means reads as flicker (a synthetic calm pan flagged 18 % of its
// block-windows). A camera that only turns about the eye is a pure rotation, so the picture moves by a homography that does NOT depend on depth: the previous frame
// can be warped to the current camera exactly (the field of view, the pose's pitch and the yaw step are known; the sign conventions are self-calibrated by
// Energy on the first pair), and what is left in r is what the camera did not explain: flicker, shimmer, noise, and a little resampling blur at hard edges.
// Tools/night/n74-flick.py reads the file. Layout, little-endian:
//   "P74B" (4 bytes) | version int32 (2) | blocksWide int32 | blocksHigh int32 | frames int32 | block int32 (pixels per block side, 8) | planes int32 (2) |
//   a gzip stream, per frame: blocksWide * blocksHigh uint16 luma sums (each at most 64 * 255 = 16320: a mean is sum / 64), then blocksWide * blocksHigh int16
//   residual sums (a mean residual is sum / 64; frame 0 has no previous frame: zeros).
// Rows run top to bottom. Luma is Still72's integer Rec.709 on the screen's 8-bit values, so a level here is a level in still72's numbers and n73-hue.py's.
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Skygrind.Game
{
    public static class Pan74
    {
        public const int Version = 2;
        public const int HeaderBytes = 28;
        public const int Planes = 2;
        static readonly byte[] Magic = { (byte)'P', (byte)'7', (byte)'4', (byte)'B' };

        /// <summary>The camera's yaw (degrees) at kept frame k of `frames`: a straight sweep through `centreDeg` at UnityTuning.N74PanRateDeg per frame, the
        /// middle frame at the centre. The same for every run (the path is the pose's yaw and the tuning, nothing live).</summary>
        public static double YawDeg(double centreDeg, int k, int frames) => centreDeg + UnityTuning.N74PanRateDeg * (k - (frames - 1) / 2.0);

        /// <summary>Whether a screen of w x h pixels divides into whole blocks (1920 x 1080 does at 8: 240 x 135).</summary>
        public static bool Grid(int w, int h, int block, out int bw, out int bh)
        {
            bw = block > 0 ? w / block : 0;
            bh = block > 0 ? h / block : 0;
            return block > 0 && w % block == 0 && h % block == 0 && bw > 0 && bh > 0;
        }

        /// <summary>A plane of w * h integer luma bytes to the top-down block sums: sums[by * bw + bx] is the sum of the block x block pixels at column bx, row by.</summary>
        public static void BlockSumsLuma(byte[] luma, int w, int h, int block, ushort[] sums)
        {
            if (luma == null || sums == null) throw new ArgumentNullException();
            if (!Grid(w, h, block, out int bw, out int bh)) throw new ArgumentException("the screen " + w + "x" + h + " is not whole blocks of " + block);
            if (luma.Length < w * h || sums.Length < bw * bh) throw new ArgumentException("buffer too small for " + w + "x" + h);
            var acc = new int[bw];
            for (int by = 0; by < bh; by++)
            {
                Array.Clear(acc, 0, bw);
                for (int yy = 0; yy < block; yy++)
                {
                    int src = (by * block + yy) * w;
                    for (int bx = 0; bx < bw; bx++)
                    {
                        int s = 0;
                        for (int xx = 0; xx < block; xx++) s += luma[src++];
                        acc[bx] += s;
                    }
                }
                for (int bx = 0; bx < bw; bx++) sums[by * bw + bx] = (ushort)acc[bx];
            }
        }

        /// <summary>Raw texture rows (Unity stores them bottom-up) to the top-down block sums of their integer luma (Still72's weights; the test's entry).</summary>
        public static void BlockSums(byte[] raw, int w, int h, int r, int g, int b, int stride, int block, ushort[] sums)
        {
            var luma = new byte[w * h];
            Still72.ToLuma(raw, w, h, r, g, b, stride, luma);
            BlockSumsLuma(luma, w, h, block, sums);
        }

        /// <summary>The previous frame's sampling map for a yaw step: for every pixel of the CURRENT frame, where its ray falls in the PREVIOUS frame (bilinear: the
        /// top-left source pixel and the fractions of the right and lower neighbours in 1/256ths), or no source (the ray leaves the picture).</summary>
        public sealed class Warp
        {
            public readonly int w, h;
            public readonly int[] at;      // index of the top-left source pixel in the previous frame, -1 when the source is outside it
            public readonly byte[] fx, fy; // 0-255: the weight (in 256ths) of the right / lower neighbour

            /// <summary>The pinhole camera of vertical field of view `vfovDeg` on a w x h picture (square pixels), pitched `pitchRad` (positive: looking up); the world turns
            /// about its up axis by `stepRad` between the two frames. The sign of `stepRad` (which way a positive yaw turns the picture) and of the pitch are the game's
            /// conventions: the reel tries both and keeps the one with the smaller Energy.</summary>
            public Warp(int w, int h, double vfovDeg, double pitchRad, double stepRad)
            {
                this.w = w;
                this.h = h;
                at = new int[w * h];
                fx = new byte[w * h];
                fy = new byte[w * h];
                double f = (h / 2.0) / Math.Tan(vfovDeg * Math.PI / 360.0);
                // the world's up axis in camera coordinates (x right, y up, z forward) for a camera looking up by the pitch
                // (a camera looking up by p has its forward vector tilted toward the world's up: up . forward = sin p, up . cameraUp = cos p)
                double ax = 0, ay = Math.Cos(pitchRad), az = Math.Sin(pitchRad);
                double c = Math.Cos(stepRad), s = Math.Sin(stepRad), k1 = 1 - c;
                int i = 0;
                for (int y = 0; y < h; y++)
                {
                    double dy = -(y + 0.5 - h / 2.0) / f;
                    for (int x = 0; x < w; x++, i++)
                    {
                        double dx = (x + 0.5 - w / 2.0) / f, dz = 1;
                        double dot = ax * dx + ay * dy + az * dz;
                        // Rodrigues: d' = d cos + (a x d) sin + a (a . d) (1 - cos)
                        double ex = dx * c + (ay * dz - az * dy) * s + ax * dot * k1;
                        double ey = dy * c + (az * dx - ax * dz) * s + ay * dot * k1;
                        double ez = dz * c + (ax * dy - ay * dx) * s + az * dot * k1;
                        if (ez < 0.05) { at[i] = -1; continue; }
                        // (+1e-7: a step of 0 must read every pixel as itself, not as a point a rounding error short of it, which would blend in its neighbour)
                        double xs = w / 2.0 + f * ex / ez - 0.5 + 1e-7, ys = h / 2.0 - f * ey / ez - 0.5 + 1e-7;
                        if (xs < 0 || ys < 0 || xs >= w - 1 || ys >= h - 1) { at[i] = -1; continue; }
                        int x0 = (int)xs, y0 = (int)ys;
                        at[i] = y0 * w + x0;
                        fx[i] = (byte)Math.Min(255, (int)((xs - x0) * 256));
                        fy[i] = (byte)Math.Min(255, (int)((ys - y0) * 256));
                    }
                }
            }
        }

        /// <summary>Mean |r| in levels over every `stride`-th pixel (both ways) that has a source, r = cur - the previous frame warped by `m` (null: no warp, the plain frame difference).
        /// The reel's calibration: the warp (sign of the yaw step, sign of the pitch) with the smallest value is the game's convention.</summary>
        public static double Energy(byte[] cur, byte[] prev, Warp m, int w, int h, int stride) => EnergyRect(cur, prev, m, w, h, stride, 0, 0, w, h);

        /// <summary>Energy over the pixels x0 .. x1 - 1, y0 .. y1 - 1 only. The calibration takes the WORLD (a core rectangle with no HUD and no gun): the screen-fixed gun and HUD are moved by the warp and read as
        /// residual, so over the whole picture the Player's calibration logged "WEAK" (n74-t2: mean |residual| 5.6 against a plain difference of 3.1) and chose a field of view 8 % too small, while on the world core
        /// the same warp left 0.37 - 0.44 of the plain difference (Research/2026-10-27-wave74/L/flick74-short.txt).</summary>
        public static double EnergyRect(byte[] cur, byte[] prev, Warp m, int w, int h, int stride, int x0, int y0, int x1, int y1)
        {
            long sum = 0, n = 0;
            for (int y = Math.Max(0, y0); y < Math.Min(h, y1); y += stride)
                for (int x = Math.Max(0, x0); x < Math.Min(w, x1); x += stride)
                {
                    int i = y * w + x;
                    int mix;
                    if (m == null) mix = prev[i] << 16;
                    else
                    {
                        int a0 = m.at[i];
                        if (a0 < 0) continue;
                        int fxx = m.fx[i], fyy = m.fy[i];
                        mix = ((prev[a0] * (256 - fxx) + prev[a0 + 1] * fxx) * (256 - fyy) + (prev[a0 + w] * (256 - fxx) + prev[a0 + w + 1] * fxx) * fyy);
                    }
                    int r = (cur[i] << 16) - mix;
                    sum += r < 0 ? -r : r;
                    n++;
                }
            return n == 0 ? double.NaN : sum / 65536.0 / n;
        }

        /// <summary>The warp for a sweep's first pair, chosen on the WORLD: the rectangle (rx0, ry0) - (rx1, ry1), as shares of the picture, which holds no HUD and no gun. Stage 1 picks the yaw step's sign and the
        /// pitch's sign (the pitch's only when the pose is not level) at the camera's own field of view; stage 2 scans `fovScales` (multiples of the camera's vertical fov, with the chosen signs) for the smallest mean |residual|:
        /// a step of 0.2 degrees at the wrong fov moves the middle of the picture by the wrong number of pixels, which the scan finds (the real shift of n74-t2's sweeps, 2.58 px a frame, is that of fov 72.8, x0.984).
        /// `detail` says what was chosen; plain / best are the plain frame difference's and the chosen warp's mean |r| over the rectangle (levels).</summary>
        public static Warp Calibrate(byte[] cur, byte[] prev, int w, int h, double vfovDeg, double pitchRad, double stepRad, double[] fovScales,
            double rx0, double ry0, double rx1, double ry1, out string detail, out double plain, out double best, out double chosenFovDeg)
        {
            int x0 = (int)(w * rx0), y0 = (int)(h * ry0), x1 = (int)(w * rx1), y1 = (int)(h * ry1);
            plain = EnergyRect(cur, prev, null, w, h, 2, x0, y0, x1, y1);
            double[] pitchSigns = Math.Abs(pitchRad) < 0.01 ? new[] { 1.0 } : new[] { 1.0, -1.0 };
            int bestRot = 1;
            double bestPs = 1, e1 = double.MaxValue;
            foreach (int rot in new[] { 1, -1 })
                foreach (double ps in pitchSigns)
                {
                    double e = EnergyRect(cur, prev, new Warp(w, h, vfovDeg, pitchRad * ps, rot * stepRad), w, h, 2, x0, y0, x1, y1);
                    if (e < e1) { e1 = e; bestRot = rot; bestPs = ps; }
                }
            Warp warp = null;
            double bestScale = 1;
            best = double.MaxValue;
            foreach (double scale in fovScales)
            {
                var m = new Warp(w, h, vfovDeg * scale, pitchRad * bestPs, bestRot * stepRad);
                double e = EnergyRect(cur, prev, m, w, h, 2, x0, y0, x1, y1);
                if (e < best) { best = e; warp = m; bestScale = scale; }
            }
            if (warp == null) { warp = new Warp(w, h, vfovDeg, pitchRad, stepRad); best = e1; }
            chosenFovDeg = vfovDeg * bestScale;
            detail = "yaw step " + (bestRot > 0 ? "+" : "-") + ", pitch sign " + (bestPs > 0 ? "+" : "-") + ", vertical fov " + (vfovDeg * bestScale).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " deg (x" + bestScale.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ")";
            return warp;
        }

        /// <summary>The residual block sums: sums[by * bw + bx] = the sum over the block of (cur - the previous frame warped by `m`), in levels, rounded (a pixel with no source adds 0).</summary>
        public static void ResidualSums(byte[] cur, byte[] prev, Warp m, int w, int h, int block, short[] sums)
        {
            if (!Grid(w, h, block, out int bw, out int bh)) throw new ArgumentException("the screen " + w + "x" + h + " is not whole blocks of " + block);
            var acc = new long[bw];
            for (int by = 0; by < bh; by++)
            {
                Array.Clear(acc, 0, bw);
                for (int yy = 0; yy < block; yy++)
                {
                    int row = (by * block + yy) * w;
                    for (int bx = 0; bx < bw; bx++)
                    {
                        long s = 0;
                        int i = row + bx * block;
                        for (int xx = 0; xx < block; xx++, i++)
                        {
                            int a0 = m.at[i];
                            if (a0 < 0) continue;
                            int fxx = m.fx[i], fyy = m.fy[i];
                            int mix = ((prev[a0] * (256 - fxx) + prev[a0 + 1] * fxx) * (256 - fyy) + (prev[a0 + w] * (256 - fxx) + prev[a0 + w + 1] * fxx) * fyy);
                            s += (cur[i] << 16) - mix;
                        }
                        acc[bx] += s;
                    }
                }
                for (int bx = 0; bx < bw; bx++)
                {
                    long v = (long)Math.Round(acc[bx] / 65536.0);
                    sums[by * bw + bx] = (short)(v < short.MinValue ? short.MinValue : v > short.MaxValue ? short.MaxValue : v);
                }
            }
        }

        /// <summary>The bytes of a frame's two planes, little-endian (the layout above; Windows and the Editor are little-endian, which this asserts).</summary>
        public static void ToBytes(ushort[] luma, short[] resid, int count, byte[] into)
        {
            if (!BitConverter.IsLittleEndian) throw new InvalidOperationException("the pan74 stack file is little-endian");
            Buffer.BlockCopy(luma, 0, into, 0, count * 2);
            Buffer.BlockCopy(resid, 0, into, count * 2, count * 2);
        }

        public static void WriteHeader(Stream s, int bw, int bh, int frames, int block)
        {
            s.Write(Magic, 0, 4);
            WriteInt(s, Version);
            WriteInt(s, bw);
            WriteInt(s, bh);
            WriteInt(s, frames);
            WriteInt(s, block);
            WriteInt(s, Planes);
        }

        /// <summary>Patches the frame count (bytes 16-19) of a file whose header is already written; the stream must be seekable.</summary>
        public static void PatchFrames(Stream s, int frames)
        {
            long at = s.Position;
            s.Seek(16, SeekOrigin.Begin);
            WriteInt(s, frames);
            s.Seek(at, SeekOrigin.Begin);
        }

        static void WriteInt(Stream s, int v)
        {
            s.WriteByte((byte)v);
            s.WriteByte((byte)(v >> 8));
            s.WriteByte((byte)(v >> 16));
            s.WriteByte((byte)(v >> 24));
        }

        static int ReadInt(Stream s)
        {
            int a = s.ReadByte(), b = s.ReadByte(), c = s.ReadByte(), d = s.ReadByte();
            if ((a | b | c | d) < 0) throw new EndOfStreamException("short header");
            return a | (b << 8) | (c << 16) | (d << 24);
        }

        /// <summary>Opens the payload for writing: the header, then a gzip stream (Fastest) over `file`, which stays open; the caller disposes the gzip stream
        /// before PatchFrames.</summary>
        public static GZipStream BeginWrite(Stream file, int bw, int bh, int frames, int block)
        {
            WriteHeader(file, bw, bh, frames, block);
            return new GZipStream(file, CompressionLevel.Fastest, true);
        }

        /// <summary>Reads a whole stack file back into per-frame luma sums and residual sums (the test's decoder; n74-flick.py does the same).</summary>
        public static void Read(Stream file, out int bw, out int bh, out int block, out List<ushort[]> luma, out List<short[]> resid)
        {
            var m = new byte[4];
            if (file.Read(m, 0, 4) != 4 || m[0] != Magic[0] || m[1] != Magic[1] || m[2] != Magic[2] || m[3] != Magic[3]) throw new InvalidDataException("not a pan74 file");
            int version = ReadInt(file);
            if (version != Version) throw new InvalidDataException("pan74 version " + version);
            bw = ReadInt(file);
            bh = ReadInt(file);
            int frames = ReadInt(file);
            block = ReadInt(file);
            int planes = ReadInt(file);
            if (planes != Planes) throw new InvalidDataException("pan74 planes " + planes);
            luma = new List<ushort[]>();
            resid = new List<short[]>();
            int n = bw * bh;
            var bytes = new byte[n * 2 * Planes];
            using (var gz = new GZipStream(file, CompressionMode.Decompress, true))
            {
                for (int k = 0; k < frames; k++)
                {
                    int got = 0;
                    while (got < bytes.Length)
                    {
                        int r = gz.Read(bytes, got, bytes.Length - got);
                        if (r <= 0) throw new EndOfStreamException("frame " + k + " short");
                        got += r;
                    }
                    var l = new ushort[n];
                    var s = new short[n];
                    for (int i = 0; i < n; i++)
                    {
                        l[i] = (ushort)(bytes[2 * i] | (bytes[2 * i + 1] << 8));
                        s[i] = (short)(bytes[2 * n + 2 * i] | (bytes[2 * n + 2 * i + 1] << 8));
                    }
                    luma.Add(l);
                    resid.Add(s);
                }
            }
        }
    }
}
