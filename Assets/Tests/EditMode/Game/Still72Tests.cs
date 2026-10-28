// NIGHT72 builder L (L1; Game/Still72.cs, the flicker meter's stack file). Pinned:
//   1. the integer luma: white 255, black 0, and the Rec.709 weights (green outweighs red outweighs blue)
//   2. ToLuma turns Unity's bottom-up rows into a top-down plane and reads RGBA32 / BGRA32 / ARGB32 / RGB24 byte orders
//   3. Delta is the byte difference mod 256 (wraps)
//   4. a stack written with BeginWrite / PatchFrames reads back frame for frame (the file Tools/night/n72-flicker.py reads), the patched count wins
//   5. an unknown texture format is refused
using System.IO;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Still72Tests
    {
        [Test]
        public void LumaEnds_AndWeights()
        {
            Assert.AreEqual(255, Still72.Luma(255, 255, 255));
            Assert.AreEqual(0, Still72.Luma(0, 0, 0));
            Assert.Greater(Still72.Luma(0, 255, 0), Still72.Luma(255, 0, 0));
            Assert.Greater(Still72.Luma(255, 0, 0), Still72.Luma(0, 0, 255));
        }

        [Test]
        public void ToLuma_FlipsRows_AndReadsByteOrders()
        {
            // 2 x 2, rows stored bottom-up: the bottom row (stored first) is dark, the top row (stored second) is bright
            foreach (var f in new[] { "RGBA32", "BGRA32", "ARGB32", "RGB24" })
            {
                Assert.IsTrue(Still72.Layout(f, out int r, out int g, out int b, out int stride), f);
                var raw = new byte[2 * 2 * stride];
                for (int x = 0; x < 2; x++)
                {
                    int bottom = x * stride, top = (2 + x) * stride;
                    raw[bottom + r] = 10; raw[bottom + g] = 10; raw[bottom + b] = 10;
                    raw[top + r] = 200; raw[top + g] = 200; raw[top + b] = 200;
                }
                var luma = new byte[4];
                Still72.ToLuma(raw, 2, 2, r, g, b, stride, luma);
                Assert.AreEqual(new byte[] { 200, 200, 10, 10 }, luma, f);
            }
            // a pure red pixel in RGBA32 is not a pure blue one in BGRA32's order: the offsets differ
            Still72.Layout("RGBA32", out int rr, out _, out _, out _);
            Still72.Layout("BGRA32", out int br, out _, out _, out _);
            Assert.AreNotEqual(rr, br);
        }

        [Test]
        public void Delta_WrapsMod256()
        {
            var cur = new byte[] { 5, 250, 0 };
            var prev = new byte[] { 10, 5, 0 };
            var d = new byte[3];
            Still72.Delta(cur, prev, d, 3);
            Assert.AreEqual(new byte[] { 251, 245, 0 }, d);
        }

        [Test]
        public void StackFile_RoundTrips_AndThePatchedCountWins()
        {
            int w = 8, h = 4, n = 5;
            var frames = new byte[n][];
            for (int k = 0; k < n; k++)
            {
                frames[k] = new byte[w * h];
                for (int i = 0; i < w * h; i++) frames[k][i] = (byte)(i * 7 + k * 41);
            }
            string path = Path.Combine(Path.GetTempPath(), "still72-test-" + System.Guid.NewGuid().ToString("N") + ".s72");
            try
            {
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                {
                    using (var gz = Still72.BeginWrite(fs, w, h, n + 3)) // planned 8, three captures failed
                    {
                        gz.Write(frames[0], 0, w * h);
                        var d = new byte[w * h];
                        for (int k = 1; k < n; k++) { Still72.Delta(frames[k], frames[k - 1], d, w * h); gz.Write(d, 0, w * h); }
                    }
                    Still72.PatchFrames(fs, n);
                }
                using (var fs = File.OpenRead(path))
                {
                    var back = Still72.Read(fs, out int bw, out int bh);
                    Assert.AreEqual(w, bw);
                    Assert.AreEqual(h, bh);
                    Assert.AreEqual(n, back.Count);
                    for (int k = 0; k < n; k++) Assert.AreEqual(frames[k], back[k], "frame " + k);
                }
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [Test]
        public void UnknownFormat_IsRefused()
        {
            Assert.IsFalse(Still72.Layout("RGBAHalf", out _, out _, out _, out _));
        }
    }
}
