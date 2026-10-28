// NIGHT72 builder L (L1, the flicker meter): the pure part of the still72 reel's stack file (no UnityEngine call, so a scratch console and the EditMode
// test compile it alone). The reel (FeelReel.N72.L.cs) holds the camera at a pose for UnityTuning.L72StillFrames rendered frames and writes their
// luma into ONE file per pose, which Tools/night/n72-flicker.py reads. File layout, little-endian:
//   "S72L" (4 bytes) | version int32 (1) | width int32 | height int32 | frames int32 | a gzip stream of frames * width * height bytes:
//   frame 0 as its luma, then every later frame k as (luma[k] - luma[k-1]) mod 256 (a still frame is a run of zeros, so a calm pose is small).
// Rows run top to bottom. Luma is integer Rec.709 on the screen's 8-bit values: (54 R + 183 G + 19 B + 128) >> 8 (54 + 183 + 19 = 256), the same
// weights as the reels' other meters (0.2126 / 0.7152 / 0.0722), taken on the gamma-encoded bytes (what a viewer sees; "levels" below are those bytes).
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Skygrind.Game
{
    public static class Still72
    {
        public const int Version = 1;
        public const int HeaderBytes = 20;
        static readonly byte[] Magic = { (byte)'S', (byte)'7', (byte)'2', (byte)'L' };

        /// <summary>Integer Rec.709 luma of one pixel (0-255).</summary>
        public static byte Luma(int r, int g, int b) => (byte)((54 * r + 183 * g + 19 * b + 128) >> 8);

        /// <summary>The byte offsets of R, G, B in a pixel and the pixel's size, for the texture formats a screen capture comes back in
        /// ("RGBA32", "BGRA32", "ARGB32", "RGB24"); false for any other format.</summary>
        public static bool Layout(string format, out int r, out int g, out int b, out int stride)
        {
            switch (format)
            {
                case "RGBA32": r = 0; g = 1; b = 2; stride = 4; return true;
                case "BGRA32": r = 2; g = 1; b = 0; stride = 4; return true;
                case "ARGB32": r = 1; g = 2; b = 3; stride = 4; return true;
                case "RGB24": r = 0; g = 1; b = 2; stride = 3; return true;
                default: r = g = b = stride = 0; return false;
            }
        }

        /// <summary>Raw texture rows (Unity stores them bottom-up) to a top-down luma plane of w * h bytes.</summary>
        public static void ToLuma(byte[] raw, int w, int h, int r, int g, int b, int stride, byte[] luma)
        {
            if (raw == null || luma == null) throw new ArgumentNullException();
            if (raw.Length < w * h * stride || luma.Length < w * h) throw new ArgumentException("buffer too small for " + w + "x" + h);
            for (int y = 0; y < h; y++)
            {
                int src = (h - 1 - y) * w * stride, dst = y * w;
                for (int x = 0; x < w; x++)
                {
                    luma[dst + x] = (byte)((54 * raw[src + r] + 183 * raw[src + g] + 19 * raw[src + b] + 128) >> 8);
                    src += stride;
                }
            }
        }

        /// <summary>out[i] = (cur[i] - prev[i]) mod 256.</summary>
        public static void Delta(byte[] cur, byte[] prev, byte[] output, int n)
        {
            for (int i = 0; i < n; i++) output[i] = unchecked((byte)(cur[i] - prev[i]));
        }

        public static void WriteHeader(Stream s, int w, int h, int frames)
        {
            s.Write(Magic, 0, 4);
            WriteInt(s, Version);
            WriteInt(s, w);
            WriteInt(s, h);
            WriteInt(s, frames);
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

        /// <summary>Opens the payload for writing: the header, then a gzip stream (Fastest: a calm pose is mostly zeros) that the caller disposes
        /// before PatchFrames. Returns the gzip stream over `file`, which stays open.</summary>
        public static GZipStream BeginWrite(Stream file, int w, int h, int frames)
        {
            WriteHeader(file, w, h, frames);
            return new GZipStream(file, CompressionLevel.Fastest, true);
        }

        /// <summary>Reads a whole stack file back into per-frame luma planes (the test's and the scratch check's decoder; n72-flicker.py does the same).</summary>
        public static List<byte[]> Read(Stream file, out int w, out int h)
        {
            var m = new byte[4];
            if (file.Read(m, 0, 4) != 4 || m[0] != Magic[0] || m[1] != Magic[1] || m[2] != Magic[2] || m[3] != Magic[3]) throw new InvalidDataException("not a still72 file");
            int version = ReadInt(file);
            if (version != Version) throw new InvalidDataException("still72 version " + version);
            w = ReadInt(file);
            h = ReadInt(file);
            int frames = ReadInt(file);
            var res = new List<byte[]>();
            int n = w * h;
            using (var gz = new GZipStream(file, CompressionMode.Decompress, true))
            {
                byte[] prev = null;
                for (int k = 0; k < frames; k++)
                {
                    var cur = new byte[n];
                    int got = 0;
                    while (got < n)
                    {
                        int r = gz.Read(cur, got, n - got);
                        if (r <= 0) throw new EndOfStreamException("frame " + k + " short");
                        got += r;
                    }
                    if (prev != null) for (int i = 0; i < n; i++) cur[i] = unchecked((byte)(cur[i] + prev[i]));
                    res.Add(cur);
                    prev = cur;
                }
            }
            return res;
        }
    }
}
