// NIGHT28 (builder A2): the checks that make the rink's rules HARD (GLACIER-REDESIGN.md 3.2: "A test makes the rule hard"). Pure (no engine); the EditMode tests and the SimHarness `rink28` command call
// the same code, so the number in the report is the number the test holds.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class Rink28Check
    {
        /// <summary>The ONE ice rule: on a `step` m grid across the rink's bounding box, every FLOOR-GRADE surface point (a downward ray from just under the lid hits it with normal.y >= 0.7: the lane, the
        /// bank, the lip, the island top, a door's floor) reads exactly the ice friction multiplier from `surf`, and `surf` holds exactly one zone. Returns the number of floor-grade points checked; `bad` is
        /// the count that read anything else and `firstBad` describes the first.</summary>
        public static int UniformIce(Rink28Arena arena, TriangleWorld world, Surfaces26 surf, double step, out int bad, out string firstBad)
        {
            bad = 0;
            firstBad = null;
            int checkedPoints = 0;
            double ice = surf.rules.iceMult;
            if (surf.zones.Count != 1 || surf.zones[0].kind != SurfaceKind.Ice)
            {
                bad++;
                firstBad = $"the surfaces hold {surf.zones.Count} zone(s), not exactly one ice zone";
            }
            double hx = arena.p.straight / 2 + arena.RimLat + arena.p.centreRadius, hz = arena.p.centreRadius + arena.RimLat;
            double startY = arena.rimTopY - 0.1;
            for (double x = arena.p.cx - hx; x <= arena.p.cx + hx + 1e-9; x += step)
            {
                for (double z = arena.p.cz - hz; z <= arena.p.cz + hz + 1e-9; z += step)
                {
                    if (!world.RayIntersect(new Vec3(x, startY, z), new Vec3(0, -1, 0), out RayHit hit)) continue;
                    if (hit.normal.y < 0.7) continue; // a wall, or the rim: not floor
                    double y = startY - hit.distance;
                    checkedPoints++;
                    double got = surf.FrictionMult(x, y, z);
                    if (got != ice)
                    {
                        bad++;
                        if (firstBad == null) firstBad = $"({x:0.00}, {y:0.00}, {z:0.00}) reads x{got} not x{ice}";
                    }
                }
            }
            return checkedPoints;
        }

        /// <summary>Winding and closure: every floor-grade triangle (Floor, Bank, Lip, IslandTop, DoorFloor) faces up, every Rim and wall triangle faces into the arena, the lid faces down. Returns the count of
        /// triangles that do not.</summary>
        public static int WindingProblems(Rink28Arena arena, out string first)
        {
            first = null;
            int bad = 0;
            foreach (Rink28Tri t in arena.tris)
            {
                Vec3 n = TriangleWorld.TriangleNormal(t.a, t.b, t.c);
                bool ok;
                switch (t.part)
                {
                    case Rink28Part.Floor: case Rink28Part.Bank: case Rink28Part.Lip: case Rink28Part.IslandTop: case Rink28Part.DoorFloor: ok = n.y >= 0.7; break;
                    case Rink28Part.Lid: ok = n.y <= -0.99; break;
                    case Rink28Part.Rim: ok = Math.Abs(n.y) < 1e-9 && TowardCentre(arena, t, n); break;
                    case Rink28Part.IslandWall: ok = Math.Abs(n.y) < 1e-9 && !TowardCentre(arena, t, n); break;
                    default: ok = Math.Abs(n.y) < 1e-9; break; // DoorWall
                }
                if (!ok)
                {
                    bad++;
                    if (first == null) first = $"{t.part} triangle normal ({n.x:0.00}, {n.y:0.00}, {n.z:0.00}) at ({t.a.x:0.0}, {t.a.y:0.0}, {t.a.z:0.0})";
                }
            }
            return bad;
        }

        // The rim's normal points toward the centre line's inside (into the arena); the island wall's points away from the island (toward the lane).
        static bool TowardCentre(Rink28Arena arena, Rink28Tri t, Vec3 n)
        {
            double mx = (t.a.x + t.b.x + t.c.x) / 3, mz = (t.a.z + t.b.z + t.c.z) / 3;
            arena.math.Project(mx, mz, out _, out double lat);
            // moving a little along the normal must move the lateral offset toward the centre line (|lat| shrinks for the rim: lat > 0, so lat decreases)
            arena.math.Project(mx + n.x * 0.1, mz + n.z * 0.1, out _, out double lat2);
            return lat2 < lat;
        }

        /// <summary>The largest angle (degrees) between the normals of two triangles that share an edge on the round caps' bank (the facet rule: well under the wall ride's 30 degree corner angle).</summary>
        public static double MaxFacetAngleDeg(Rink28Arena arena)
        {
            double best = 0;
            Rink28Tri[] banks = arena.tris.FindAll(t => t.part == Rink28Part.Rim).ToArray();
            for (int i = 0; i < banks.Length; i++)
            {
                Vec3 a = TriangleWorld.TriangleNormal(banks[i].a, banks[i].b, banks[i].c);
                for (int j = i + 1; j < banks.Length; j++)
                {
                    if (!ShareEdge(banks[i], banks[j])) continue;
                    Vec3 b = TriangleWorld.TriangleNormal(banks[j].a, banks[j].b, banks[j].c);
                    double d = Math.Max(-1, Math.Min(1, Vec3.Dot(a, b)));
                    best = Math.Max(best, JsMath.Acos(d) * 180 / Math.PI);
                }
            }
            return best;
        }

        static bool ShareEdge(Rink28Tri s, Rink28Tri t)
        {
            int shared = 0;
            foreach (Vec3 u in new[] { s.a, s.b, s.c })
                foreach (Vec3 v in new[] { t.a, t.b, t.c })
                    if (u == v) shared++;
            return shared >= 2;
        }
    }
}
