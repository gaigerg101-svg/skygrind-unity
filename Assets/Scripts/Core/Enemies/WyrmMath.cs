// NIGHT24 (builder B): THE KILN WYRM's pure maths (Core, no engine): the Catmull-Rom path of a pattern resampled to a constant arc
// length with a clamped pitch, the oriented box (OBB) of a body piece, a ray against it, a sphere against it. Every sin / cos / atan
// goes through JsMath so a run is the same on the harness (net10) and in Unity (Mono). A new module: the web has no worm.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>A pattern's path: points at a constant arc length `ds`, the arc length at each waypoint the pattern named.</summary>
    public sealed class WyrmPath
    {
        public Vec3[] pts;
        public double ds, length;
        /// <summary>The arc length (m) at user waypoint i (the head's start point and the spline's phantom ends are not counted).</summary>
        public double[] wpS;

        /// <summary>The point at arc length `s` (clamped to the ends) and the unit direction of travel there.</summary>
        public Vec3 At(double s, out Vec3 dir)
        {
            double f = s / ds;
            int n = pts.Length;
            if (f <= 0)
            {
                dir = DirAt(0);
                return pts[0];
            }
            if (f >= n - 1)
            {
                dir = DirAt(n - 2);
                return pts[n - 1];
            }
            int i = (int)f;
            double t = f - i;
            Vec3 a = pts[i], b = pts[i + 1];
            dir = DirAt(i);
            return new Vec3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        Vec3 DirAt(int i)
        {
            if (i < 0) i = 0;
            if (i > pts.Length - 2) i = pts.Length - 2;
            return Vec3.Normalize(pts[i + 1] - pts[i]);
        }
    }

    /// <summary>A body piece's frame: the oriented box (centre, forward, right, up, half sizes).</summary>
    public struct WyrmBox
    {
        public Vec3 c, f, r, u;
        public double hr, hu, hf;

        /// <summary>Make the frame from a forward direction (world up for the side; the world z when forward is vertical).</summary>
        public void Orient(Vec3 forward)
        {
            f = forward;
            Vec3 up = Math.Abs(f.y) > 0.98 ? new Vec3(0, 0, 1) : new Vec3(0, 1, 0);
            // r = normalize(up x f), u = f x r
            Vec3 rr = new Vec3(up.y * f.z - up.z * f.y, up.z * f.x - up.x * f.z, up.x * f.y - up.y * f.x);
            r = Vec3.Normalize(rr);
            u = new Vec3(f.y * r.z - f.z * r.y, f.z * r.x - f.x * r.z, f.x * r.y - f.y * r.x);
        }

        /// <summary>The distance along a unit ray to the box (0 when the origin is inside); -1: a miss.</summary>
        public double Ray(Vec3 o, Vec3 d)
        {
            Vec3 p = o - c;
            double ox = Vec3.Dot(p, r), oy = Vec3.Dot(p, u), oz = Vec3.Dot(p, f);
            double dx = Vec3.Dot(d, r), dy = Vec3.Dot(d, u), dz = Vec3.Dot(d, f);
            double t0 = 0, t1 = double.PositiveInfinity;
            if (!Slab(ox, dx, hr, ref t0, ref t1)) return -1;
            if (!Slab(oy, dy, hu, ref t0, ref t1)) return -1;
            if (!Slab(oz, dz, hf, ref t0, ref t1)) return -1;
            return t0;
        }

        static bool Slab(double o, double d, double h, ref double t0, ref double t1)
        {
            if (Math.Abs(d) < 1e-12) return o >= -h && o <= h;
            double a = (-h - o) / d, e = (h - o) / d;
            if (a > e)
            {
                double s = a;
                a = e;
                e = s;
            }
            if (a > t0) t0 = a;
            if (e < t1) t1 = e;
            return t0 <= t1;
        }

        /// <summary>Does a sphere (centre p, radius rad) touch the box?</summary>
        public bool Touches(Vec3 p, double rad)
        {
            Vec3 q = p - c;
            double x = Clamp(Vec3.Dot(q, r), hr), y = Clamp(Vec3.Dot(q, u), hu), z = Clamp(Vec3.Dot(q, f), hf);
            double dx = Vec3.Dot(q, r) - x, dy = Vec3.Dot(q, u) - y, dz = Vec3.Dot(q, f) - z;
            return dx * dx + dy * dy + dz * dz <= rad * rad;
        }

        static double Clamp(double v, double h) => v < -h ? -h : v > h ? h : v;
    }

    public static class WyrmMath
    {
        /// <summary>Uniform Catmull-Rom between p1 and p2 (p0 and p3 are the neighbours), t in 0..1.</summary>
        public static Vec3 CatmullRom(Vec3 p0, Vec3 p1, Vec3 p2, Vec3 p3, double t)
        {
            double t2 = t * t, t3 = t2 * t;
            return new Vec3(
                0.5 * (2 * p1.x + (-p0.x + p2.x) * t + (2 * p0.x - 5 * p1.x + 4 * p2.x - p3.x) * t2 + (-p0.x + 3 * p1.x - 3 * p2.x + p3.x) * t3),
                0.5 * (2 * p1.y + (-p0.y + p2.y) * t + (2 * p0.y - 5 * p1.y + 4 * p2.y - p3.y) * t2 + (-p0.y + 3 * p1.y - 3 * p2.y + p3.y) * t3),
                0.5 * (2 * p1.z + (-p0.z + p2.z) * t + (2 * p0.z - 5 * p1.z + 4 * p2.z - p3.z) * t2 + (-p0.z + 3 * p1.z - 3 * p2.z + p3.z) * t3));
        }

        /// <summary>
        /// A pattern's path: the spline from the head (heading `headDir`) through `wps`, resampled every `ds` m, the pitch clamped to
        /// `pitchDeg` (89 or more: free). `wpS[i]` is the arc length at wps[i]. The spline's phantom ends extend the first and last legs.
        /// </summary>
        public static WyrmPath BuildPath(Vec3 head, Vec3 headDir, IList<Vec3> wps, double pitchDeg, double ds)
        {
            var c = new List<Vec3>(wps.Count + 4);
            c.Add(head - headDir * 8);
            c.Add(head);
            foreach (Vec3 w in wps) c.Add(w);
            c.Add(c[c.Count - 1] + (c[c.Count - 1] - c[c.Count - 2]));
            var dense = new List<Vec3>(512);
            var cum = new List<double>(512);
            var wpAt = new double[wps.Count];
            dense.Add(head);
            cum.Add(0);
            double acc = 0;
            for (int i = 1; i + 2 < c.Count; i++)
            {
                double segLen = Vec3.DistanceTo(c[i], c[i + 1]);
                int m = (int)Math.Ceiling(segLen / 0.25) + 1;
                for (int k = 1; k <= m; k++)
                {
                    Vec3 p = CatmullRom(c[i - 1], c[i], c[i + 1], c[i + 2], (double)k / m);
                    acc += Vec3.DistanceTo(dense[dense.Count - 1], p);
                    dense.Add(p);
                    cum.Add(acc);
                }
                wpAt[i - 1] = acc; // this leg ends at control i + 1 = waypoint i - 1 (control 1 is the head, control 2 waypoint 0)
            }
            int n = (int)Math.Floor(acc / ds) + 2;
            var pts = new Vec3[n];
            pts[0] = head;
            int j = 0;
            for (int q = 1; q < n; q++)
            {
                double target = q * ds;
                if (target > acc) target = acc;
                while (j + 1 < cum.Count - 1 && cum[j + 1] < target) j++;
                double span = cum[j + 1] - cum[j];
                double t = span > 1e-12 ? (target - cum[j]) / span : 0;
                Vec3 a = dense[j], b = dense[j + 1];
                pts[q] = new Vec3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
            }
            if (pitchDeg < 89) ClampPitch(pts, pitchDeg, ds);
            return new WyrmPath { pts = pts, ds = ds, length = (n - 1) * ds, wpS = wpAt };
        }

        /// <summary>Limit each leg's slope to `pitchDeg`: the leg keeps its length and its heading, the climb is cut down.</summary>
        static void ClampPitch(Vec3[] pts, double pitchDeg, double ds)
        {
            double rad = pitchDeg * Math.PI / 180;
            double sinM = JsMath.Sin(rad), cosM = JsMath.Cos(rad);
            double hx = 0, hz = 1;
            var orig = (Vec3[])pts.Clone();
            for (int i = 1; i < pts.Length; i++)
            {
                Vec3 d = orig[i] - orig[i - 1];
                double len = d.Length;
                if (len < 1e-9) continue;
                double h = JsMath.Hypot(d.x, d.z);
                if (h > 1e-9)
                {
                    hx = d.x / h;
                    hz = d.z / h;
                }
                double sinP = d.y / len;
                if (Math.Abs(sinP) <= sinM)
                {
                    // still shift the rest of the path by the error so far: pts[i] was set from the unclamped path
                    pts[i] = pts[i - 1] + d;
                    continue;
                }
                double sy = sinP < 0 ? -sinM : sinM;
                pts[i] = new Vec3(pts[i - 1].x + hx * cosM * len, pts[i - 1].y + sy * len, pts[i - 1].z + hz * cosM * len);
            }
        }

        /// <summary>Sphere (centre c, radius rad) against a ray; the entry distance or -1.</summary>
        public static double RaySphere(Vec3 o, Vec3 d, Vec3 c, double rad) => CombatMath.RaySphere(o, d, c, rad);
    }
}
