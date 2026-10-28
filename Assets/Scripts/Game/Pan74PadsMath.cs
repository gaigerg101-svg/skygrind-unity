// NIGHT74 builder L (pads74): the pure parts of the pad regions the pan74 reel writes beside each sweep (Pan74Pads.cs writes them; Tools/night/n74-flick.py `pads` reads them).
// A pad region is the screen rectangle of a box round the pad (the pad volume: halo, stream and chevrons; the stream column), per rendered frame, so the meter can say how many of the
// flagged blocks lie on the pads and how many of those a change took away. No UnityEngine in here: a console can run it (Pad74Tests).
//   file `pan74-<level>-<room>.pads`, text: first line `P74PADS 1 <width> <height>`; then one line per frame and pad:
//     `f <frame> <pad index> <seen 0|1> <vol x0> <vol y0> <vol x1> <vol y1> <stream x0> <stream y0> <stream x1> <stream y1>`   (px of the capture, row 0 at the top, the frame numbered as the stack's)
//   seen is 0 when the pad is not in the picture, a corner of its box is behind the camera, or the level's boxes hide it (Hidden); rectangles are then all 0.
using System;
using System.Globalization;
using System.Text;

namespace Skygrind.Game
{
    public static class Pan74PadsMath
    {
        public const string Magic = "P74PADS";

        public static string Header(int w, int h) => Magic + " 1 " + w.ToString(CultureInfo.InvariantCulture) + " " + h.ToString(CultureInfo.InvariantCulture);

        /// <summary>One frame's line for one pad. `seen` false writes zeros.</summary>
        public static string Line(int frame, int pad, bool seen, int[] vol, int[] stream)
        {
            var sb = new StringBuilder("f ");
            sb.Append(frame.ToString(CultureInfo.InvariantCulture)).Append(' ').Append(pad.ToString(CultureInfo.InvariantCulture)).Append(' ').Append(seen ? '1' : '0');
            for (int k = 0; k < 4; k++) sb.Append(' ').Append((seen ? vol[k] : 0).ToString(CultureInfo.InvariantCulture));
            for (int k = 0; k < 4; k++) sb.Append(' ').Append((seen ? stream[k] : 0).ToString(CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        /// <summary>The bounding rectangle (x0, y0, x1, y1 px, top-down: y = height - screen y) of projected points. `sx`, `sy` are Unity screen coordinates (origin bottom-left), `behind[i]` marks a point
        /// behind the camera. False when any point is behind (the rectangle would be wrong) or the rectangle misses the picture altogether.</summary>
        public static bool Rect(double[] sx, double[] sy, bool[] behind, int width, int height, int[] outRect)
        {
            double x0 = double.MaxValue, y0 = double.MaxValue, x1 = double.MinValue, y1 = double.MinValue;
            for (int i = 0; i < sx.Length; i++)
            {
                if (behind[i]) return false;
                double top = height - sy[i];
                if (sx[i] < x0) x0 = sx[i];
                if (sx[i] > x1) x1 = sx[i];
                if (top < y0) y0 = top;
                if (top > y1) y1 = top;
            }
            if (x1 < 0 || y1 < 0 || x0 > width || y0 > height) return false;
            outRect[0] = (int)Math.Floor(x0); outRect[1] = (int)Math.Floor(y0); outRect[2] = (int)Math.Ceiling(x1); outRect[3] = (int)Math.Ceiling(y1);
            return true;
        }

        /// <summary>The eight corners of the box centre (cx, cy, cz) +- half width hw on x and z, cy + y0 .. cy + y1 on y: x, y, z triples in `o` (24 numbers).</summary>
        public static void Corners(double cx, double cy, double cz, double hw, double y0, double y1, double[] o)
        {
            int n = 0;
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 2; k++)
                    {
                        o[n++] = cx + (i == 0 ? -hw : hw);
                        o[n++] = cy + (j == 0 ? y0 : y1);
                        o[n++] = cz + (k == 0 ? -hw : hw);
                    }
        }

        /// <summary>Whether the segment a -> b passes through the box (min xyz, max xyz in b6[0..5]) shrunk by eps on every side: the slab test.</summary>
        public static bool SegmentHitsBox(double ax, double ay, double az, double bx, double by, double bz, double[] b6, double eps)
        {
            double t0 = 0, t1 = 1;
            double[] a = { ax, ay, az }, d = { bx - ax, by - ay, bz - az };
            for (int i = 0; i < 3; i++)
            {
                double lo = b6[i] + eps, hi = b6[i + 3] - eps;
                if (hi <= lo) return false;
                if (Math.Abs(d[i]) < 1e-12)
                {
                    if (a[i] < lo || a[i] > hi) return false;
                    continue;
                }
                double ta = (lo - a[i]) / d[i], tb = (hi - a[i]) / d[i];
                if (ta > tb) { double s = ta; ta = tb; tb = s; }
                if (ta > t0) t0 = ta;
                if (tb < t1) t1 = tb;
                if (t0 > t1) return false;
            }
            return true;
        }

        /// <summary>The pad is hidden when BOTH sight lines (from the camera's eye to the pad's floor point lifted `liftLow` and to it lifted `liftHigh`) pass through a box of the level (b6 arrays of 6; a wedge, 7 numbers, is not tested).</summary>
        public static bool Hidden(double ex, double ey, double ez, double px, double py, double pz, double liftLow, double liftHigh, System.Collections.Generic.List<double[]> boxes, double eps)
        {
            bool low = false, high = false;
            foreach (double[] b in boxes)
            {
                if (b == null || b.Length != 6) continue;
                if (!low && SegmentHitsBox(ex, ey, ez, px, py + liftLow, pz, b, eps)) low = true;
                if (!high && SegmentHitsBox(ex, ey, ez, px, py + liftHigh, pz, b, eps)) high = true;
                if (low && high) return true;
            }
            return false;
        }
    }
}
