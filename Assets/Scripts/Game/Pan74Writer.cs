// NIGHT74 builder L (L1): one pan74 sweep's stack file as the reel writes it (Pan74.cs documents the layout and why the residual). Per rendered frame: the screen's
// luma, the 8 x 8 block sums of it, and the block sums of the residual against the previous frame warped to this camera (Pan74.Warp). The warp's conventions (which
// way a positive yaw step turns the picture, the pitch's sign, a field-of-view guess) are self-calibrated on the first pair by the smallest Energy and printed, so the
// first Player run shows whether the compensation works (calib: |r| against the plain frame difference).
using System;
using System.IO;
using System.IO.Compression;
using System.Globalization;
using UnityEngine;

namespace Skygrind.Game
{
    sealed class Pan74Writer
    {
        public string path, format, error, calib = "no calibration (fewer than 2 frames)";
        public int w, h, bw, bh, frames;
        public long bytes;
        /// <summary>The first pair's mean |frame difference| with no warp, and with the chosen warp (levels): the compensation's quality figure.</summary>
        public double plainEnergy = double.NaN, warpEnergy = double.NaN;
        FileStream fs;
        GZipStream gz;
        byte[] raw, packed, cur, prev;
        ushort[] luma;
        short[] resid;
        int r, g, b, stride;
        Pan74.Warp warp;
        readonly int planned;
        readonly double vfovDeg, pitchRad;

        public Pan74Writer(string path, int planned, double vfovDeg, double pitchRad)
        {
            this.path = path;
            this.planned = planned;
            this.vfovDeg = vfovDeg;
            this.pitchRad = pitchRad;
        }

        /// <summary>Adds the texture as the next frame; false (error set) when it cannot be read.</summary>
        public bool Add(Texture2D t)
        {
            try
            {
                if (t == null) { error = "no capture"; return false; }
                int block = UnityTuning.N74PanBlock;
                if (fs == null)
                {
                    w = t.width;
                    h = t.height;
                    format = t.format.ToString();
                    if (!Still72.Layout(format, out r, out g, out b, out stride)) { error = "texture format " + format + " is not one the meter reads"; return false; }
                    if (!Pan74.Grid(w, h, block, out bw, out bh)) { error = "the screen " + w + "x" + h + " is not whole " + block + " px blocks"; return false; }
                    cur = new byte[w * h];
                    prev = new byte[w * h];
                    luma = new ushort[bw * bh];
                    resid = new short[bw * bh];
                    packed = new byte[bw * bh * 2 * Pan74.Planes];
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                    gz = Pan74.BeginWrite(fs, bw, bh, planned, block);
                }
                else if (t.width != w || t.height != h) { error = "the screen changed size to " + t.width + "x" + t.height; return false; }
                Unity.Collections.NativeArray<byte> nat = t.GetRawTextureData<byte>();
                if (raw == null || raw.Length != nat.Length) raw = new byte[nat.Length];
                nat.CopyTo(raw);
                Still72.ToLuma(raw, w, h, r, g, b, stride, cur);
                Pan74.BlockSumsLuma(cur, w, h, block, luma);
                if (frames == 0) Array.Clear(resid, 0, resid.Length); // no previous frame yet
                else
                {
                    if (warp == null) Calibrate();
                    Pan74.ResidualSums(cur, prev, warp, w, h, block, resid);
                }
                Pan74.ToBytes(luma, resid, bw * bh, packed);
                gz.Write(packed, 0, packed.Length);
                byte[] swap = prev; prev = cur; cur = swap;
                frames++;
                return true;
            }
            catch (Exception e)
            {
                error = e.GetType().Name + ": " + e.Message;
                return false;
            }
        }

        /// <summary>The first pair (frame 0 at the sweep's first yaw, frame 1 one step on): tries the yaw step both ways, the pitch both ways (when the pose looks level
        /// the pitch cannot matter) and three field-of-view guesses, and keeps the warp with the smallest mean |residual|.</summary>
        void Calibrate()
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            double stepRad = UnityTuning.N74PanRateDeg * Math.PI / 180;
            // NIGHT74 (the meter sharpened): chosen on the WORLD core (no HUD, no gun), the fov scanned finely (Pan74.Calibrate); the old whole-picture figures are printed too, for the n74-t1 / t2 logs' comparison
            warp = Pan74.Calibrate(cur, prev, w, h, vfovDeg, pitchRad, stepRad, UnityTuning.N74PanFovTry, UnityTuning.N74PanCoreX0, UnityTuning.N74PanCoreY0, UnityTuning.N74PanCoreX1, UnityTuning.N74PanCoreY1, out string detail, out plainEnergy, out warpEnergy, out _);
            double wholePlain = Pan74.Energy(cur, prev, null, w, h, 2), wholeWarp = Pan74.Energy(cur, prev, warp, w, h, 2);
            calib = detail + ": over the world core mean |residual| " + warpEnergy.ToString("0.000", ci) + " levels against the plain frame difference " + plainEnergy.ToString("0.000", ci)
                + (warpEnergy < UnityTuning.N74PanCalibGood * plainEnergy ? " (compensated)" : " (WEAK: the warp removed under " + ((1 - UnityTuning.N74PanCalibGood) * 100).ToString("0", ci) + " % of the difference)")
                + "; over the whole picture (the gun and HUD are moved by the warp and read as residual) " + wholeWarp.ToString("0.000", ci) + " against " + wholePlain.ToString("0.000", ci);
        }

        public void Close()
        {
            try
            {
                if (gz != null) { gz.Dispose(); gz = null; }
                if (fs != null)
                {
                    Pan74.PatchFrames(fs, frames);
                    bytes = fs.Length;
                    fs.Dispose();
                    fs = null;
                }
            }
            catch (Exception e) { error = error ?? (e.GetType().Name + ": " + e.Message); }
        }
    }
}
