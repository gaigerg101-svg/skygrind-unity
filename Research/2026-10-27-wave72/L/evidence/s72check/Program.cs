// Scratch check (builder L, L1): writes a stack file with Skygrind.Game.Still72 (the reel's own encoder) from a synthetic RGBA texture sequence,
// reads it back with Still72.Read, and leaves the file for n72-flicker.py to read. The pattern: a gradient scene (bottom-up rows, as Unity stores them)
// with a 32 x 32 block at (160, 540) from the top that swaps black / white every frame, 12 frames, 640 x 360.
using System;
using System.IO;
using Skygrind.Game;

static class P
{
    static int Main(string[] a)
    {
        string path = a[0];
        int w = 640, h = 360, frames = 12, bx = 160 / 3, by = 540 / 3, bs = 32 / 2;
        var raw = new byte[w * h * 4];
        var cur = new byte[w * h];
        var prev = new byte[w * h];
        var delta = new byte[w * h];
        var all = new System.Collections.Generic.List<byte[]>();
        using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
        {
            int written = 0;
            using (var gz = Still72.BeginWrite(fs, w, h, frames + 5)) // a planned count that the patch then corrects, as a failed capture does
            {
                for (int k = 0; k < frames; k++)
                {
                    for (int y = 0; y < h; y++)
                        for (int x = 0; x < w; x++)
                        {
                            int row = h - 1 - y; // the texture row of top-down row y
                            int o = (row * w + x) * 4;
                            byte v = (byte)((x * 255 / w + y) & 0xFF);
                            if (x >= bx && x < bx + bs && y >= by && y < by + bs) v = (byte)(k % 2 == 0 ? 255 : 0);
                            raw[o] = v; raw[o + 1] = (byte)(v / 2); raw[o + 2] = (byte)(v / 3); raw[o + 3] = 255;
                        }
                    Still72.Layout("RGBA32", out int r, out int g, out int b, out int stride);
                    Still72.ToLuma(raw, w, h, r, g, b, stride, cur);
                    all.Add((byte[])cur.Clone());
                    if (k == 0) gz.Write(cur, 0, cur.Length);
                    else { Still72.Delta(cur, prev, delta, cur.Length); gz.Write(delta, 0, delta.Length); }
                    var t = prev; prev = cur; cur = t;
                    written++;
                }
            }
            Still72.PatchFrames(fs, written);
        }
        using (var fs = File.OpenRead(path))
        {
            var back = Still72.Read(fs, out int bw, out int bh);
            bool ok = bw == w && bh == h && back.Count == frames;
            for (int k = 0; ok && k < frames; k++)
                for (int i = 0; i < w * h; i++)
                    if (back[k][i] != all[k][i]) { ok = false; break; }
            Console.WriteLine("Still72 round trip: " + (ok ? "ok" : "FAIL") + " | frames " + back.Count + " | " + bw + "x" + bh + " | file " + new FileInfo(path).Length + " bytes");
            return ok ? 0 : 1;
        }
    }
}
