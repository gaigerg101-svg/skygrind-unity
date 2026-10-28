// EXT53 builder N: a ride wall near a point, for the a253 reel (FeelReel.N53.RideKill.cs) and the harness's `n53ridekill ride`
// (Tools/SimHarness/Harness.N53.RideKill.cs): the same search in both, so the headless ride is the Player's. Not sim code: nothing in a run
// calls it (it only reads the collider). A wall: a vertical face hit by two level rays (1.0 m and 2.6 m over the floor) 1.5-8 m from a
// floor point, that the same rays still hit at the same distance (+-0.3 m) every 2 m along it for `length` m, with floor under the run line
// and the line clear at 1.0 m. Grid points ring out from `near` (2 m steps to `reach`), 16 headings each; the first wall found wins.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class RideKill53Wall
    {
        /// <summary>The run's start (feet on the floor, `standOff` m off the wall), the along-wall direction, the wall's outward normal.</summary>
        public static bool Find(GameWorld world, Vec3 near, double reach, double length, double standOff, out Vec3 start, out Vec3 along, out Vec3 normal) =>
            Find(world, near, reach, length, standOff, null, out start, out along, out normal);

        /// <summary>As Find, the run line (its start and its end) inside `rect` [x0, z0, x1, z1] when given (a campaign room's rect).</summary>
        public static bool Find(GameWorld world, Vec3 near, double reach, double length, double standOff, double[] rect, out Vec3 start, out Vec3 along, out Vec3 normal)
        {
            start = near;
            along = new Vec3(0, 0, -1);
            normal = new Vec3(1, 0, 0);
            if (world == null) return false;
            int rings = (int)Math.Ceiling(reach / 2);
            for (int ring = 0; ring <= rings; ring++)
                for (int gx = -ring; gx <= ring; gx++)
                    for (int gz = -ring; gz <= ring; gz++)
                    {
                        if (Math.Max(Math.Abs(gx), Math.Abs(gz)) != ring) continue;
                        if (!Floor(world, new Vec3(near.x + gx * 2, near.y, near.z + gz * 2), out Vec3 p)) continue;
                        for (int k = 0; k < 16; k++)
                        {
                            double a = k * Math.PI / 8;
                            var d = new Vec3(Math.Sin(a), 0, Math.Cos(a));
                            if (!Face(world, p, d, 1.5, 8, out double dist, out Vec3 n)) continue;
                            // the wall's own normal (horizontal), the run along it both ways
                            var t = new Vec3(-n.z, 0, n.x);
                            foreach (double sgn in new[] { 1.0, -1.0 })
                            {
                                var u = new Vec3(t.x * sgn, 0, t.z * sgn);
                                // the run line, standOff off the face
                                var s0 = new Vec3(p.x + d.x * (dist - standOff), p.y, p.z + d.z * (dist - standOff));
                                if (!Floor(world, s0, out Vec3 f0) || Math.Abs(f0.y - p.y) > 0.5) continue;
                                // the run line clear from the feet to over a rider's head (put 1.2 m up: the capsule's top ~3.0 m, the eye ~2.8 m;
                                // a door's lintel over a 1.0 m clear line broke the headless ride, n53ridekill ride --shoot), and the line
                                // 1.5 m out (where a target stands) clear at the waist
                                // a horizontal ray never meets a ceiling: the headroom over the line (a tunnel's roof, a door's lintel) by rays up
                                bool clear = Headroom(world, f0, 4.0);
                                // over the capsule's width too (a door jamb 0.3 m out from the wall face stopped the headless ride at its first metre)
                                foreach (double ox in Offsets)
                                    foreach (double hy in Heights)
                                        if (clear && world.RayIntersect(new Vec3(f0.x + n.x * ox, f0.y + hy, f0.z + n.z * ox), u, out RayHit block) && block.distance < length + 2) clear = false;
                                if (clear && world.RayIntersect(new Vec3(f0.x + n.x * 1.5, f0.y + 1.0, f0.z + n.z * 1.5), u, out RayHit blockOut) && blockOut.distance < length + 2) clear = false;
                                if (!clear) continue;
                                bool ok = true;
                                for (double s = 2; s <= length && ok; s += 2)
                                {
                                    var q = new Vec3(f0.x + u.x * s, f0.y, f0.z + u.z * s);
                                    ok = Floor(world, q, out Vec3 fq) && Math.Abs(fq.y - f0.y) <= 0.5 && Headroom(world, fq, 4.0)
                                        && Face(world, fq, new Vec3(-n.x, 0, -n.z), 0.05, standOff + 0.6, out double dq, out Vec3 nq)
                                        && Math.Abs(dq - standOff) <= 0.3 && nq.x * n.x + nq.z * n.z > 0.95;
                                }
                                if (!ok) continue;
                                if (rect != null && rect.Length >= 4 && !(In(rect, f0.x, f0.z) && In(rect, f0.x + u.x * length, f0.z + u.z * length))) continue;
                                start = f0;
                                along = u;
                                normal = n;
                                return true;
                            }
                        }
                    }
            return false;
        }

        // m over the floor the run line must be clear at: every 0.25 m to 3.5 m (a beam between two coarser rays was missed: 0.3 / 1.0 / 1.9 / 2.8 /
        // 3.4 passed a line whose eye ray was blocked 1.15 m ahead)
        static readonly double[] Heights = { 0.25, 0.5, 0.75, 1.0, 1.25, 1.5, 1.75, 2.0, 2.25, 2.5, 2.75, 3.0, 3.25, 3.5 };
        static readonly double[] Offsets = { -0.3, 0, 0.4, 0.8 }; // m off the run line, out from the wall (-0.3: 0.15 m from the face, the capsule's wall side)

        /// <summary>Nothing over the feet within `up` m (a ray straight up from 0.2 m over the floor).</summary>
        static bool Headroom(GameWorld world, Vec3 feet, double up) =>
            !(world.RayIntersect(new Vec3(feet.x, feet.y + 0.2, feet.z), new Vec3(0, 1, 0), out RayHit h) && h.distance < up - 0.2);

        /// <summary>The straight line from `from` to `to` is clear of the collider (a shot's line from the eye to an aim point).</summary>
        public static bool LineClear(GameWorld world, Vec3 from, Vec3 to)
        {
            double dx = to.x - from.x, dy = to.y - from.y, dz = to.z - from.z, d = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (d < 1e-6) return true;
            return !(world.RayIntersect(from, new Vec3(dx / d, dy / d, dz / d), out RayHit h) && h.distance < d - 0.3);
        }

        static bool In(double[] r, double x, double z) => x >= Math.Min(r[0], r[2]) && x <= Math.Max(r[0], r[2]) && z >= Math.Min(r[1], r[3]) && z <= Math.Max(r[1], r[3]);

        /// <summary>The stick (moveX, moveZ) that runs along `along` with the look's yaw along it and leans into the wall (normal outward):
        /// forward 1, right toward the wall by `lean` (never away from it: the ride's away rule).</summary>
        public static void Stick(Vec3 along, Vec3 normal, double lean, out double yaw, out double moveX, out double moveZ)
        {
            yaw = Math.Atan2(-along.x, -along.z);
            double rx = Math.Cos(yaw), rz = -Math.Sin(yaw); // the look's right (yaw 0 looks toward -z; right is +x)
            moveX = -(normal.x * rx + normal.z * rz) * lean;
            moveZ = 1;
        }

        /// <summary>The floor under a point (a ray down from 6 m over it, at most 14 m): the feet's spot on it.</summary>
        static bool Floor(GameWorld world, Vec3 at, out Vec3 feet)
        {
            feet = at;
            var top = new Vec3(at.x, at.y + 6, at.z);
            if (!world.RayIntersect(top, new Vec3(0, -1, 0), out RayHit h) || h.distance > 14 || h.normal.y < 0.8) return false;
            feet = new Vec3(at.x, top.y - h.distance, at.z);
            return true;
        }

        /// <summary>A vertical face along `d` from the feet: both level rays (1.0 m, 2.6 m up) hit it between min and max m, at the same
        /// distance (+-0.3 m), with horizontal normals that agree; `n` is the face's outward normal (toward the ray's origin).</summary>
        static bool Face(GameWorld world, Vec3 feet, Vec3 d, double min, double max, out double dist, out Vec3 n)
        {
            dist = 0;
            n = default;
            if (!world.RayIntersect(new Vec3(feet.x, feet.y + 1.0, feet.z), d, out RayHit lo) || lo.distance < min || lo.distance > max) return false;
            if (!world.RayIntersect(new Vec3(feet.x, feet.y + 2.6, feet.z), d, out RayHit hi) || Math.Abs(hi.distance - lo.distance) > 0.3) return false;
            if (Math.Abs(lo.normal.y) > 0.15 || Math.Abs(hi.normal.y) > 0.15) return false;
            double len = Math.Sqrt(lo.normal.x * lo.normal.x + lo.normal.z * lo.normal.z);
            if (len < 1e-6) return false;
            var nl = new Vec3(lo.normal.x / len, 0, lo.normal.z / len);
            if (nl.x * d.x + nl.z * d.z > 0) nl = new Vec3(-nl.x, 0, -nl.z); // toward the ray's origin, whatever the triangle's winding
            if (nl.x * d.x + nl.z * d.z > -0.7) return false; // the face looks back at the ray (not a grazing hit)
            double lh = Math.Sqrt(hi.normal.x * hi.normal.x + hi.normal.z * hi.normal.z);
            if (lh < 1e-6 || Math.Abs(hi.normal.x * nl.x + hi.normal.z * nl.z) / lh < 0.95) return false;
            dist = lo.distance;
            n = nl;
            return true;
        }
    }
}
