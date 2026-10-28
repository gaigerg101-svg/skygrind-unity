// NIGHT74 builder L (L1; Game/Pan74.cs, the moving-camera flicker meter's stack file and motion compensation). Pinned:
//   1. YawDeg: the middle of the sweep is the pose's yaw, the steps are UnityTuning.N74PanRateDeg, the path is the same every call
//   2. Grid: 1920 x 1080 is 240 x 135 blocks of 8; a screen that does not divide is refused; the planted block sits on the grid
//   3. BlockSums turns Unity's bottom-up rows into top-down blocks, sums the integer luma (Still72's weights), reads the byte orders, and never overflows a ushort
//   4. a stack written with BeginWrite / PatchFrames reads back frame for frame, both planes (the file Tools/night/n74-flick.py reads), the patched count wins
//   5. the warp: a synthetic world rendered at two yaws (both conventions of the yaw's sign, a pitched camera): the right warp leaves under a third of the plain frame
//      difference, the calibration's choice (the smallest Energy of the four sign pairs) is the convention the picture was made with, the wrong sign is worse than no warp
//   6. ResidualSums of a frame against itself (no motion) is zero; a flat +10 swap on a block reads +10 x 64; a pixel with no source adds nothing
//   7. the planted control: an alternation of luma 0.5 +- N74PlantAmp is the +-3 % the slate asks for
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Pan74Tests
    {
        [Test]
        public void YawDeg_IsCentredAndSteady()
        {
            int n = UnityTuning.N74PanFrames;
            double mid = (Pan74.YawDeg(30, 0, n) + Pan74.YawDeg(30, n - 1, n)) / 2;
            Assert.AreEqual(30, mid, 1e-9, "the sweep is centred on the pose's yaw");
            Assert.AreEqual(UnityTuning.N74PanRateDeg, Pan74.YawDeg(30, 6, n) - Pan74.YawDeg(30, 5, n), 1e-9);
            Assert.AreEqual(Pan74.YawDeg(-120, 17, n), Pan74.YawDeg(-120, 17, n), "the same path every call");
            Assert.AreEqual(UnityTuning.N74PanRateDeg * (n - 1), Pan74.YawDeg(0, n - 1, n) - Pan74.YawDeg(0, 0, n), 1e-9);
        }

        [Test]
        public void Grid_FullHd_Is240x135_AndOddScreensAreRefused()
        {
            Assert.IsTrue(Pan74.Grid(1920, 1080, 8, out int bw, out int bh));
            Assert.AreEqual(240, bw);
            Assert.AreEqual(135, bh);
            Assert.IsFalse(Pan74.Grid(1920, 1081, 8, out _, out _));
            Assert.IsFalse(Pan74.Grid(1920, 1080, 0, out _, out _));
            Assert.AreEqual(0, UnityTuning.N74PlantX % UnityTuning.N74PanBlock, "the plant sits on the block grid");
            Assert.AreEqual(0, UnityTuning.N74PlantY % UnityTuning.N74PanBlock);
            Assert.AreEqual(0, UnityTuning.N74PlantSize % UnityTuning.N74PanBlock);
        }

        [Test]
        public void BlockSums_FlipRows_SumLuma_AndReadByteOrders()
        {
            // 8 x 16 pixels = two blocks high, one wide; Unity stores the rows bottom-up: stored rows 0-7 are the BOTTOM block (dark, 10), rows 8-15 the TOP block (bright, 200)
            foreach (var f in new[] { "RGBA32", "BGRA32", "ARGB32", "RGB24" })
            {
                Assert.IsTrue(Still72.Layout(f, out int r, out int g, out int b, out int stride), f);
                var raw = new byte[8 * 16 * stride];
                for (int y = 0; y < 16; y++)
                    for (int x = 0; x < 8; x++)
                    {
                        int at = (y * 8 + x) * stride;
                        byte v = (byte)(y < 8 ? 10 : 200);
                        raw[at + r] = v; raw[at + g] = v; raw[at + b] = v;
                    }
                var sums = new ushort[2];
                Pan74.BlockSums(raw, 8, 16, r, g, b, stride, 8, sums);
                Assert.AreEqual(200 * 64, sums[0], f + ": the top block first");
                Assert.AreEqual(10 * 64, sums[1], f);
            }
        }

        [Test]
        public void BlockSums_UsesStill72Luma_AndTheLargestSumFits()
        {
            var raw = new byte[8 * 8 * 4];
            for (int i = 0; i < 64; i++) { raw[i * 4] = 255; raw[i * 4 + 1] = 255; raw[i * 4 + 2] = 255; raw[i * 4 + 3] = 255; }
            var sums = new ushort[1];
            Pan74.BlockSums(raw, 8, 8, 0, 1, 2, 4, 8, sums);
            Assert.AreEqual(255 * 64, sums[0], "white: 64 x 255 = 16320, under 65535");
            for (int i = 0; i < 64; i++) { raw[i * 4] = 255; raw[i * 4 + 1] = 0; raw[i * 4 + 2] = 0; }
            Pan74.BlockSums(raw, 8, 8, 0, 1, 2, 4, 8, sums);
            Assert.AreEqual(64 * Still72.Luma(255, 0, 0), sums[0], "the same weights as still72's luma");
        }

        [Test]
        public void StackFile_RoundTrips_BothPlanes_AndThePatchedCountWins()
        {
            int bw = 5, bh = 3, n = 4;
            var luma = new ushort[n][];
            var resid = new short[n][];
            for (int k = 0; k < n; k++)
            {
                luma[k] = new ushort[bw * bh];
                resid[k] = new short[bw * bh];
                for (int i = 0; i < bw * bh; i++) { luma[k][i] = (ushort)(i * 997 + k * 4001); resid[k][i] = (short)(i * 301 - k * 2000 - 6000); }
            }
            string path = Path.Combine(Path.GetTempPath(), "pan74-test-" + Guid.NewGuid().ToString("N") + ".p74");
            try
            {
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                {
                    using (var gz = Pan74.BeginWrite(fs, bw, bh, n + 5, 8)) // planned 9, five captures failed
                    {
                        var packed = new byte[bw * bh * 2 * Pan74.Planes];
                        for (int k = 0; k < n; k++) { Pan74.ToBytes(luma[k], resid[k], bw * bh, packed); gz.Write(packed, 0, packed.Length); }
                    }
                    Pan74.PatchFrames(fs, n);
                }
                using (var fs = File.OpenRead(path))
                {
                    Pan74.Read(fs, out int rbw, out int rbh, out int block, out List<ushort[]> backL, out List<short[]> backR);
                    Assert.AreEqual(bw, rbw);
                    Assert.AreEqual(bh, rbh);
                    Assert.AreEqual(8, block);
                    Assert.AreEqual(n, backL.Count);
                    Assert.AreEqual(n, backR.Count);
                    for (int k = 0; k < n; k++)
                    {
                        Assert.AreEqual(luma[k], backL[k], "luma frame " + k);
                        Assert.AreEqual(resid[k], backR[k], "residual frame " + k);
                    }
                }
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        // ---- the warp, on a synthetic world seen through the same pinhole camera ----

        const int SW = 320, SH = 176; // a 40 x 22 grid of 8 px blocks
        const double SFov = 60;

        /// <summary>The world texture, a function of the ray's azimuth and elevation (smooth content with a few sharper features).</summary>
        static double Texture(double az, double el) =>
            128 + 50 * Math.Sin(5 * az) * Math.Cos(3 * el) + 30 * Math.Sin(17 * az + 7 * el) + 20 * Math.Sin(41 * az) + (Math.Sin(23 * az) > 0.9 ? 25 : 0);

        /// <summary>The picture a camera at `yawDeg`, pitched `pitchRad` (positive up), sees: world = Ry(yaw) * P * ray, P the pitch; `flip` reverses the yaw's sign (the other convention).</summary>
        static byte[] Render(double yawDeg, double pitchRad, bool flip)
        {
            double f = (SH / 2.0) / Math.Tan(SFov * Math.PI / 360), yaw = (flip ? -1 : 1) * yawDeg * Math.PI / 180, cy = Math.Cos(yaw), sy = Math.Sin(yaw), cp = Math.Cos(pitchRad), sp = Math.Sin(pitchRad);
            var img = new byte[SW * SH];
            for (int y = 0; y < SH; y++)
                for (int x = 0; x < SW; x++)
                {
                    double dx = (x + 0.5 - SW / 2.0) / f, dy = -(y + 0.5 - SH / 2.0) / f;
                    double px = dx, py = dy * cp + sp, pz = -dy * sp + cp; // P * (dx, dy, 1)
                    double wx = px * cy + pz * sy, wy = py, wz = -px * sy + pz * cy;
                    double az = Math.Atan2(wx, wz), el = Math.Atan2(wy, Math.Sqrt(wx * wx + wz * wz));
                    img[y * SW + x] = (byte)Math.Max(0, Math.Min(255, Math.Round(Texture(az, el))));
                }
            return img;
        }

        [TestCase(false, 0.0)]
        [TestCase(false, 0.12)]
        [TestCase(true, 0.0)]
        [TestCase(true, -0.15)]
        public void Warp_RightConventionCompensates_AndCalibrationFindsIt(bool flip, double pitch)
        {
            const double step = 0.6; // degrees a frame: a bigger step than the reel's, so the plain difference is well clear of the rounding
            byte[] prev = Render(40, pitch, flip), cur = Render(40 + step, pitch, flip);
            double plain = Pan74.Energy(cur, prev, null, SW, SH, 1);
            Assert.Greater(plain, 1.0, "the pan moves the picture");
            double best = double.MaxValue, wrong = double.MaxValue;
            int bestRot = 0, bestPs = 0;
            foreach (int rot in new[] { 1, -1 })
                foreach (int ps in new[] { 1, -1 })
                {
                    double e = Pan74.Energy(cur, prev, new Pan74.Warp(SW, SH, SFov, pitch * ps, rot * step * Math.PI / 180), SW, SH, 1);
                    if (e < best) { best = e; bestRot = rot; bestPs = ps; }
                    if (rot == (flip ? 1 : -1)) wrong = Math.Min(wrong, e);
                }
            Assert.AreEqual(flip ? -1 : 1, bestRot, "the yaw step's sign the picture was made with");
            if (Math.Abs(pitch) > 0.01) Assert.AreEqual(1, bestPs, "the pitch's sign");
            Assert.Less(best, plain / 3, "the right warp leaves under a third of the plain difference");
            Assert.Greater(wrong, plain, "the wrong yaw sign is worse than no warp at all");
        }

        static readonly double[] ScaleTry = { 0.90, 0.92, 0.94, 0.96, 0.98, 1.0, 1.02, 1.04, 1.06, 1.08, 1.10 };

        [TestCase(false, 62.0, 0.0)]
        [TestCase(true, 58.0, 0.1)]
        public void Calibrate_OnTheWorldCore_FindsTheRealFov_DespiteAScreenFixedOverlay(bool flip, double toldFov, double pitch)
        {
            const double step = 0.6;
            byte[] prev = Render(40, pitch, flip), cur = Render(40 + step, pitch, flip);
            // a screen-fixed, high-contrast overlay in the lower right (the gun, the HUD), the same in both frames: the warp moves it, so over the whole picture it reads as residual
            for (int y = 120; y < SH; y++)
                for (int x = 200; x < SW; x++)
                {
                    byte v = (byte)(((x / 3 + y / 3) & 1) == 0 ? 230 : 25);
                    prev[y * SW + x] = v;
                    cur[y * SW + x] = v;
                }
            Pan74.Warp m = Pan74.Calibrate(cur, prev, SW, SH, toldFov, pitch, step * Math.PI / 180, ScaleTry, UnityTuning.N74PanCoreX0, UnityTuning.N74PanCoreY0, UnityTuning.N74PanCoreX1, UnityTuning.N74PanCoreY1,
                out string detail, out double plain, out double best, out double chosenFov);
            Assert.IsNotNull(m);
            Assert.AreEqual(SFov, chosenFov, 1.5, "the fov the picture was made at, found from a camera told " + toldFov + ": " + detail);
            StringAssert.Contains(flip ? "yaw step -" : "yaw step +", detail, "the yaw convention");
            Assert.Less(best, plain / 3, "the world core is compensated: " + detail);
        }

        [Test]
        public void EnergyRect_ReadsOnlyItsRectangle()
        {
            byte[] a = Render(40, 0, false), b = Render(40.6, 0, false);
            double whole = Pan74.Energy(a, b, null, SW, SH, 1), same = Pan74.EnergyRect(a, b, null, SW, SH, 1, 0, 0, SW, SH);
            Assert.AreEqual(whole, same, 1e-9, "the whole picture as a rectangle is Energy");
            // a block of the picture made different: the rectangle that excludes it does not see it, the one that holds it does
            for (int y = 100; y < 140; y++) for (int x = 220; x < 300; x++) b[y * SW + x] = (byte)(a[y * SW + x] ^ 0x80);
            double outside = Pan74.EnergyRect(a, b, null, SW, SH, 1, 0, 0, 200, 90), inside = Pan74.EnergyRect(a, b, null, SW, SH, 1, 200, 90, 320, 150);
            Assert.AreEqual(Pan74.EnergyRect(a, Render(40.6, 0, false), null, SW, SH, 1, 0, 0, 200, 90), outside, 1e-9);
            Assert.Greater(inside, Pan74.EnergyRect(a, Render(40.6, 0, false), null, SW, SH, 1, 200, 90, 320, 150) + 5);
        }

        [Test]
        public void ResidualSums_NoMotionIsZero_AFlatSwapReadsItsSize_AndNoSourceAddsNothing()
        {
            var still = Render(10, 0.05, false);
            var m0 = new Pan74.Warp(SW, SH, SFov, 0.05, 0); // no step: every pixel's source is itself
            var sums = new short[(SW / 8) * (SH / 8)];
            Pan74.ResidualSums(still, still, m0, SW, SH, 8, sums);
            foreach (short s in sums) Assert.AreEqual(0, s);
            var up = (byte[])still.Clone();
            for (int y = 8; y < 16; y++) for (int x = 16; x < 24; x++) up[y * SW + x] = (byte)Math.Min(255, up[y * SW + x] + 10);
            // the exact +10 only where the pixel was not clamped: count it from the data instead of assuming
            int added = 0;
            for (int y = 8; y < 16; y++) for (int x = 16; x < 24; x++) added += up[y * SW + x] - still[y * SW + x];
            Pan74.ResidualSums(up, still, m0, SW, SH, 8, sums);
            Assert.AreEqual(added, sums[1 * (SW / 8) + 2], "block (2, 1) holds the swap's sum");
            Assert.AreEqual(0, sums[0]);
            // a step big enough that the left edge has no source: those pixels add nothing, the others still cancel on a flat picture
            var flat = new byte[SW * SH];
            for (int i = 0; i < flat.Length; i++) flat[i] = 90;
            var m = new Pan74.Warp(SW, SH, SFov, 0, 10 * Math.PI / 180);
            Pan74.ResidualSums(flat, flat, m, SW, SH, 8, sums);
            foreach (short s in sums) Assert.AreEqual(0, s, "a flat picture has no residual, with or without a source");
        }

        [Test]
        public void PlantedControl_IsPlusMinusThreePercent()
        {
            // two frames of the swap: luma 0.5 + 0.03 and 0.5 - 0.03, as display bytes; the step between them is 2 x 3 % of 255 = 15.3 levels, the amplitude 7.65
            double hi = (UnityTuning.N74PlantMid + UnityTuning.N74PlantAmp) * 255, lo = (UnityTuning.N74PlantMid - UnityTuning.N74PlantAmp) * 255;
            Assert.AreEqual(0.03 * 255, (hi - lo) / 2, 0.01);
            Assert.AreEqual(0.03f, UnityTuning.N74PlantAmp, 1e-6, "the scout's +-3 % alternating luma");
        }
    }
}
