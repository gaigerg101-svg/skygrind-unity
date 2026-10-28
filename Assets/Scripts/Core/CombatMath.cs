// Ports of skygrind/src/combat/combatMath.js (the pure combat math: cones, ray vs sphere / vertical capsule,
// ricochet and punch picks, the husk leap, the swap timeline), gunMath.js sweepTargets (the melee cone), player/ads.js's
// pure zoom helpers and fx/springs.js's closed-form damped spring (NIGHT5 F2: the weapon kick). Vectors are Vec3,
// inputs never mutated. Golden vectors: PortExport/vectors/combatMath.json, gunMath.json.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>A vertical capsule target standing on `base` (husk.js punchTargets): its enemy and wind-up state.</summary>
    public sealed class BodyTarget
    {
        public object enemy;
        public Vec3 basePos;
        public double height;
        public double radius;
        public int windupSteps;
    }

    public struct TargetPick
    {
        public BodyTarget target;
        public double distance; // eye to body surface
    }

    public static class CombatMath
    {
        /// <summary>A direction uniformly distributed over a cone's solid angle (cos theta uniform in [cos half, 1]).</summary>
        public static Vec3 SampleCone(Vec3 dir, double halfAngle, double u1, double u2)
        {
            double cosT = 1 - u1 * (1 - JsMath.Cos(halfAngle));
            double sinT = Math.Sqrt(Math.Max(0, 1 - cosT * cosT));
            double phi = 2 * Math.PI * u2;
            bool useY = Math.Abs(dir.y) < 0.9;
            double hx = useY ? 0 : 1;
            double hy = useY ? 1 : 0;
            double t1x = dir.y * 0 - dir.z * hy;
            double t1y = dir.z * hx - dir.x * 0;
            double t1z = dir.x * hy - dir.y * hx;
            double len = JsMath.Hypot(t1x, t1y, t1z);
            t1x /= len;
            t1y /= len;
            t1z /= len;
            double t2x = dir.y * t1z - dir.z * t1y;
            double t2y = dir.z * t1x - dir.x * t1z;
            double t2z = dir.x * t1y - dir.y * t1x;
            double a = sinT * JsMath.Cos(phi);
            double b = sinT * JsMath.Sin(phi);
            return new Vec3(dir.x * cosT + t1x * a + t2x * b, dir.y * cosT + t1y * a + t2y * b, dir.z * cosT + t1z * a + t2z * b);
        }

        /// <summary>Ray vs sphere: distance to the first hit in front of the origin, 0 inside, or -1.</summary>
        public static double RaySphere(Vec3 o, Vec3 d, Vec3 c, double r)
        {
            double ox = o.x - c.x, oy = o.y - c.y, oz = o.z - c.z;
            double b = ox * d.x + oy * d.y + oz * d.z;
            double cc = ox * ox + oy * oy + oz * oz - r * r;
            double disc = b * b - cc;
            if (disc < 0) return -1;
            double s = Math.Sqrt(disc);
            if (-b - s >= 0) return -b - s;
            if (-b + s >= 0) return 0;
            return -1;
        }

        /// <summary>Ray vs a vertical capsule standing on `base` (its lowest point): distance or -1.</summary>
        public static double RayVerticalCapsule(Vec3 o, Vec3 d, Vec3 basePos, double height, double r)
        {
            double y0 = basePos.y + r;
            double y1 = basePos.y + height - r;
            double best = double.PositiveInfinity;
            double px = o.x - basePos.x;
            double pz = o.z - basePos.z;
            double a = d.x * d.x + d.z * d.z;
            if (a > 0)
            {
                double b = px * d.x + pz * d.z;
                double c = px * px + pz * pz - r * r;
                double disc = b * b - a * c;
                if (disc >= 0)
                {
                    double s = Math.Sqrt(disc);
                    foreach (double t in new[] { (-b - s) / a, (-b + s) / a })
                    {
                        double y = o.y + t * d.y;
                        if (t >= 0 && y >= y0 && y <= y1 && t < best) best = t;
                    }
                }
            }
            foreach (double cy in new[] { y0, y1 })
            {
                double t = RaySphere(o, d, new Vec3(basePos.x, cy, basePos.z), r);
                if (t >= 0 && t < best) best = t;
            }
            return double.IsPositiveInfinity(best) ? -1 : best;
        }

        static double Dist(Vec3 a, Vec3 b) => JsMath.Hypot(a.x - b.x, a.y - b.y, a.z - b.z);

        /// <summary>The nearest target within range with line of sight from a bounce point (the zoomed Kestrel's ricochet).</summary>
        public static int PickRicochetTarget(Vec3 from, IReadOnlyList<Vec3> points, double range, Func<Vec3, Vec3, bool> hasLos)
        {
            int best = -1;
            double bestDist = range;
            for (int i = 0; i < points.Count; i++)
            {
                double dd = Dist(from, points[i]);
                if (dd <= bestDist && hasLos(from, points[i]))
                {
                    best = i;
                    bestDist = dd;
                }
            }
            return best;
        }

        /// <summary>A ballistic leap peaking `apexAbove` over max(from.y, to.y) that lands on `to`: velocity and flight time.</summary>
        public static Vec3 LeapVelocity(Vec3 from, Vec3 to, double apexAbove, double gravity, out double time)
        {
            double peakY = Math.Max(from.y, to.y) + apexAbove;
            double vy = Math.Sqrt(2 * gravity * (peakY - from.y));
            time = vy / gravity + Math.Sqrt(2 * (peakY - to.y) / gravity);
            return new Vec3((to.x - from.x) / time, vy, (to.z - from.z) / time);
        }

        // --- the swap timeline (SPEC 1.5) ------------------------------------------------------------------------

        public static double SwapLowered(double t, double lowerSteps, double totalSteps)
        {
            if (t <= 0 || t >= totalSteps) return 0;
            if (t <= lowerSteps) return t / lowerSteps;
            return (totalSteps - t) / (totalSteps - lowerSteps);
        }

        public static bool SwapCanFire(double t, double lowerSteps, double totalSteps, double fireFraction)
        {
            if (t >= totalSteps) return true;
            if (t < lowerSteps) return false;
            return (t - lowerSteps) / (totalSteps - lowerSteps) >= fireFraction;
        }

        public static double SwapMirror(double t, double lowerSteps, double totalSteps)
        {
            double raise = totalSteps - lowerSteps;
            if (t < lowerSteps) return totalSteps - t / lowerSteps * raise;
            return (totalSteps - t) / raise * lowerSteps;
        }

        // --- punch, parry and the melee cone -----------------------------------------------------------------------

        /// <summary>The punch target: the nearest body in range whose axis is inside the cone (widened by its angular radius).</summary>
        public static TargetPick? PickPunchTarget(Vec3 origin, Vec3 dir, IReadOnlyList<BodyTarget> targets, double range, double halfAngle)
        {
            TargetPick? best = null;
            for (int i = 0; i < targets.Count; i++) // indexed: a foreach over the interface boxes its enumerator (U2-02)
            {
                BodyTarget t = targets[i];
                double y0 = t.basePos.y + t.radius;
                double y1 = t.basePos.y + t.height - t.radius;
                double px = t.basePos.x - origin.x;
                double py = Math.Min(y1, Math.Max(y0, origin.y)) - origin.y;
                double pz = t.basePos.z - origin.z;
                double d = JsMath.Hypot(px, py, pz);
                double surface = Math.Max(0, d - t.radius);
                if (surface > range || (best.HasValue && surface >= best.Value.distance)) continue;
                if (d > t.radius)
                {
                    double cos = (px * dir.x + py * dir.y + pz * dir.z) / d;
                    double slack = JsMath.Asin(t.radius / d);
                    if (JsMath.Acos(Math.Min(1, Math.Max(-1, cos))) > halfAngle + slack) continue;
                }
                best = new TargetPick { target = t, distance = surface };
            }
            return best;
        }

        /// <summary>A punch that lands during the target's wind-up is a parry.</summary>
        public static bool IsParry(int windupSteps) => windupSteps > 0;

        /// <summary>gunMath.js sweepTargets: every body in the cone and in range, nearest first.</summary>
        public static List<TargetPick> SweepTargets(Vec3 origin, Vec3 dir, IReadOnlyList<BodyTarget> targets, double range, double halfAngle, List<TargetPick> output = null)
        {
            output ??= new List<TargetPick>();
            output.Clear();
            for (int i = 0; i < targets.Count; i++) // indexed: a foreach over the interface boxes its enumerator (U2-02)
            {
                BodyTarget t = targets[i];
                double y0 = t.basePos.y + t.radius;
                double y1 = t.basePos.y + t.height - t.radius;
                double px = t.basePos.x - origin.x;
                double py = Math.Min(y1, Math.Max(y0, origin.y)) - origin.y;
                double pz = t.basePos.z - origin.z;
                double d = JsMath.Hypot(px, py, pz);
                double surface = Math.Max(0, d - t.radius);
                if (surface > range) continue;
                if (d > t.radius)
                {
                    double cos = (px * dir.x + py * dir.y + pz * dir.z) / d;
                    double slack = JsMath.Asin(Math.Min(1, t.radius / d));
                    if (JsMath.Acos(Math.Min(1, Math.Max(-1, cos))) > halfAngle + slack) continue;
                }
                output.Add(new TargetPick { target = t, distance = surface });
            }
            // A stable sort, as JS's Array.sort (List.Sort isn't): equal distances keep their input order.
            for (int i = 1; i < output.Count; i++)
            {
                TargetPick x = output[i];
                int j = i - 1;
                while (j >= 0 && output[j].distance > x.distance)
                {
                    output[j + 1] = output[j];
                    j--;
                }
                output[j + 1] = x;
            }
            return output;
        }

        // --- ads.js: the optical zoom ------------------------------------------------------------------------------

        const double Deg = Math.PI / 180;

        public static double ZoomedFov(double baseDeg, double zoom)
        {
            if (!(zoom > 1)) return baseDeg;
            return 2 * JsMath.Atan(JsMath.Tan(baseDeg * Deg / 2) / zoom) / Deg;
        }

        public static double ZoomSensitivity(double baseDeg, double zoomDeg, double mult = 1)
        {
            if (zoomDeg >= baseDeg) return 1;
            return JsMath.Tan(zoomDeg * Deg / 2) / JsMath.Tan(baseDeg * Deg / 2) * mult;
        }
    }

    /// <summary>fx/springs.js: a closed-form underdamped spring (x(t) = A e^(-zeta w t) sin(wd t)).</summary>
    public struct SpringShape
    {
        public double freqHz, zeta, w, a, wd, tPeak, peakPerA, overshoot;

        /// <summary>springShape: rates, the time of the first peak, the peak per unit amplitude and the overshoot ratio.</summary>
        public static SpringShape Of(double freqHz, double zeta)
        {
            double w = 2 * Math.PI * freqHz;
            double a = zeta * w;
            double wd = w * Math.Sqrt(1 - zeta * zeta);
            double tPeak = JsMath.Atan2(wd, a) / wd; // x' = 0: tan(wd t) = wd / a
            return new SpringShape
            {
                freqHz = freqHz, zeta = zeta, w = w, a = a, wd = wd, tPeak = tPeak,
                peakPerA = JsMath.Exp(-a * tPeak) * JsMath.Sin(wd * tPeak),
                overshoot = JsMath.Exp(-Math.PI * zeta / Math.Sqrt(1 - zeta * zeta)),
            };
        }

        /// <summary>springValue: x(t) of a kick with amplitude A (0 before it starts).</summary>
        public double Value(double A, double t) => t <= 0 ? 0 : A * JsMath.Exp(-a * t) * JsMath.Sin(wd * t);

        /// <summary>springVelocity: x'(t) of a kick with amplitude A.</summary>
        public double Velocity(double A, double t) => t < 0 ? 0 : A * JsMath.Exp(-a * t) * (wd * JsMath.Cos(wd * t) - a * JsMath.Sin(wd * t));

        /// <summary>settleTime: when a kick that peaks at `peak` stays inside +-eps for good (its envelope under eps).</summary>
        public static double SettleTime(double peak, SpringShape s, double eps)
        {
            double A = Math.Abs(peak / s.peakPerA);
            return A <= eps ? 0 : JsMath.Log(A / eps) / s.a;
        }

        /// <summary>softCap: x below `knee x cap` passes through, above it bends toward +-cap (tanh), never past it.</summary>
        public static double SoftCap(double x, double cap, double knee = 0.5)
        {
            double k = cap * knee;
            double m = Math.Abs(x);
            if (m <= k) return x;
            return JsSign(x) * (k + (cap - k) * JsMath.Tanh((m - k) / (cap - k)));
        }

        /// <summary>Math.sign: -1, +1, or x itself for +-0 and NaN (C#'s Math.Sign returns an int and throws on NaN).</summary>
        static double JsSign(double x) => x > 0 ? 1 : x < 0 ? -1 : x;
    }

    /// <summary>fx/springs.js createSpringBank: additive kicks over N channels; a kick is dropped once under its snaps.</summary>
    public sealed class SpringBank
    {
        readonly int capacity, n;
        readonly double[] amp, age, rate, wd, snaps;
        readonly bool[] live;

        public SpringBank(int capacity, double[] snaps)
        {
            this.capacity = capacity;
            this.snaps = snaps;
            n = snaps.Length;
            amp = new double[capacity * n];
            age = new double[capacity];
            rate = new double[capacity];
            wd = new double[capacity];
            live = new bool[capacity];
        }

        /// <summary>Largest envelope / snap ratio of slot i (under 1: gone).</summary>
        double Weight(int i)
        {
            double e = JsMath.Exp(-rate[i] * age[i]);
            double w = 0;
            for (int c = 0; c < n; c++) w = JsMath.Max(w, Math.Abs(amp[i * n + c]) * e / snaps[c]);
            return w;
        }

        /// <summary>Kicks still ringing.</summary>
        public int Active
        {
            get
            {
                int k = 0;
                for (int i = 0; i < capacity; i++) k += live[i] ? 1 : 0;
                return k;
            }
        }

        /// <summary>A kick of spring `s` whose first peak on channel c is peaks[c].</summary>
        public void Add(SpringShape s, params double[] peaks)
        {
            int slot = -1;
            for (int i = 0; i < capacity; i++) if (!live[i]) { slot = i; break; }
            if (slot < 0)
            {
                double least = double.PositiveInfinity;
                for (int i = 0; i < capacity; i++)
                {
                    double w = Weight(i);
                    if (w < least) { least = w; slot = i; }
                }
            }
            for (int c = 0; c < n; c++) amp[slot * n + c] = (c < peaks.Length ? peaks[c] : 0) / s.peakPerA;
            age[slot] = 0;
            rate[slot] = s.a;
            wd[slot] = s.wd;
            live[slot] = true;
        }

        /// <summary>Frame time: advance every kick and drop the settled ones.</summary>
        public void Update(double dt)
        {
            for (int i = 0; i < capacity; i++)
            {
                if (!live[i]) continue;
                age[i] += dt;
                if (Weight(i) < 1) live[i] = false;
            }
        }

        /// <summary>value(c): channel c now, the sum of every live kick (exactly 0 with none live).</summary>
        public double Sample(int channel)
        {
            double sum = 0;
            for (int i = 0; i < capacity; i++)
            {
                if (!live[i]) continue;
                sum += new SpringShape { a = rate[i], wd = wd[i] }.Value(amp[i * n + channel], age[i]);
            }
            return sum;
        }

        /// <summary>velocity(c): channel c's rate of change now (units per s).</summary>
        public double Velocity(int channel)
        {
            double sum = 0;
            for (int i = 0; i < capacity; i++)
            {
                if (!live[i]) continue;
                sum += new SpringShape { a = rate[i], wd = wd[i] }.Velocity(amp[i * n + channel], age[i]);
            }
            return sum;
        }

        public void Clear()
        {
            for (int i = 0; i < capacity; i++) live[i] = false;
        }
    }
}
