// NIGHT39 builder V (C1 row 2, the verbs reel on the n38 levels): the wall the verbs reel (FeelReel.N35.Verbs.cs, Wall35) rides for VERBS rows 9-11,
// moved here (UnityEngine-free, so the harness runs the SAME pick: `n39views wall`). R1 measured both n38 REDs as the reel's spot, not the ride:
//   - n38-span: the longest wall was box 5 (zone p2, the east wall's NORIDE liner over the void): noride, so no ride starts;
//   - n38-foundry: b2's east wall over THE GANTRY's pit 1: the player drops into the 4 m pit and meets L1's deck face 1.2 m along: both rides end `corner`.
// The old search checked only the wall and the clearance at y + 1 along the run. For a ride (rows 9-11) the run now also stops where
//   - the wall's face at the capsule's band (feet + 0.1 .. + 1.9 m) is a noride box the Mover refuses (its own boxes: Mover.flow34.ride35.noRide, null
//     when NIGHT38 is forced off, so a NIGHT37 run picks as before), or
//   - the lane 0.7 m off the wall has no floor within 1.6 m under y + 1 (FloorAt: Wall35's Floor35 test), the start included.
// The rows 13 / 18 search (floors: their spots' floors, NIGHT37 K) is unchanged. A reel's pick, not a rule: nothing here changes the sim.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class VerbsWall39
    {
        /// <summary>A floor within 1.6 m under (x, y + 1, z) (FeelReel.N35.Verbs.cs Floor35).</summary>
        public static bool FloorAt(SurvivalSim sim, double x, double y, double z) =>
            sim.world.RayIntersect(new Vec3(x, y + 1.0, z), new Vec3(0, -1, 0), out RayHit dh) && dh.distance <= 1.6 && dh.normal.y > 0.7;

        /// <summary>The spots the wall's rows stand on: its start (rows 9-11), the slide's spot 3.3 m out (13), the grapple's spot 9 m out (18).</summary>
        public static bool WallFloors(SurvivalSim sim, Vec3 p0, Vec3 t, Vec3 n, double y) =>
            FloorAt(sim, p0.x, y, p0.z) && FloorAt(sim, p0.x + t.x * 2 + n.x * 3.3, y, p0.z + t.z * 2 + n.z * 3.3) && FloorAt(sim, p0.x + t.x * 4 + n.x * 9, y, p0.z + t.z * 4 + n.z * 9);

        /// <summary>Is the wall face behind lane point (x, z) (the face `dist` m along -n) a noride wall at the capsule's band from floor `y`?</summary>
        static bool NoRide(Ride38.Box[] boxes, double x, double z, Vec3 n, double dist, double y) =>
            boxes != null && Ride38.NoRideBand(boxes, x - n.x * dist, z - n.z * dist, y + 0.1, y + 1.9, 0.1);

        /// <summary>
        /// The longest straight wall found by rays from each non-boss room's centre: a start 0.7 m off the wall (at the room's floor + 0.2), the direction
        /// along it, its normal, the clear length ahead (whole metres), the room. `floors`: rows 13 / 18 (their spots' floors). `ride`: rows 9-11 (the run
        /// stops at a noride face or a lane with no floor). Neither: NIGHT37's plain search.
        /// </summary>
        public static bool Pick(SurvivalSim sim, bool floors, bool ride, out Vec3 start, out Vec3 along, out Vec3 normal, out double length, out string room)
        {
            start = along = normal = default;
            length = 0;
            room = "";
            Ride38.Box[] noRide = ride && sim.mover != null ? sim.mover.flow34.ride35.noRide : null;
            foreach (RoomDef r in sim.campaignLevel.rooms)
            {
                if (r.kind == "boss") continue;
                double fy = r.spawn.y;
                var c = new Vec3((r.x0 + r.x1) / 2, fy + 1.0, (r.z0 + r.z1) / 2);
                for (int k = 0; k < 16; k++)
                {
                    double a = k * Math.PI / 8;
                    var d = new Vec3(Math.Sin(a), 0, Math.Cos(a));
                    if (!sim.world.RayIntersect(c, d, out RayHit h) || h.distance > 20 || h.distance < 2) continue;
                    Vec3 n = h.normal;
                    if (Math.Abs(n.y) > 0.2) continue;
                    double nl = Math.Sqrt(n.x * n.x + n.z * n.z);
                    n = new Vec3(n.x / nl, 0, n.z / nl);
                    var hit = new Vec3(c.x + d.x * h.distance, c.y, c.z + d.z * h.distance);
                    var p0 = new Vec3(hit.x + n.x * 0.7, hit.y, hit.z + n.z * 0.7);
                    if (ride && (!FloorAt(sim, p0.x, fy, p0.z) || NoRide(noRide, p0.x, p0.z, n, 0.7, fy))) continue;
                    for (int s = 0; s < 2; s++)
                    {
                        int sgn = s == 0 ? 1 : -1;
                        var t = new Vec3(-n.z * sgn, 0, n.x * sgn);
                        double len = 0;
                        for (int m = 1; m <= 30; m++)
                        {
                            var q = new Vec3(p0.x + t.x * m, p0.y, p0.z + t.z * m);
                            bool wall = sim.world.RayIntersect(q, new Vec3(-n.x, 0, -n.z), out RayHit hw) && hw.distance < 1.4;
                            bool clear = !sim.world.RayIntersect(new Vec3(q.x - t.x, q.y, q.z - t.z), t, out RayHit hc) || hc.distance > 1.05;
                            if (!wall || !clear) break;
                            if (ride && (!FloorAt(sim, q.x, fy, q.z) || NoRide(noRide, q.x, q.z, n, hw.distance, fy))) break;
                            // NIGHT40 (ride40's attach asks for a wall >= 2 m above the feet: a 2.5 m wall is a platform face by design, n40-r3 r3kiln2 row 9):
                            // with ride40 live the reel rides only a wall that still stands 4 m above the floor
                            if (ride && sim.n40.ride40 && !(sim.world.RayIntersect(new Vec3(q.x, fy + 4.0, q.z), new Vec3(-n.x, 0, -n.z), out RayHit hTall) && hTall.distance < 1.6)) break;
                            len = m;
                        }
                        if (len > length && (!floors || WallFloors(sim, p0, t, n, fy)))
                        {
                            length = len;
                            start = new Vec3(p0.x, fy + 0.2, p0.z);
                            along = t;
                            normal = n;
                            room = r.id;
                        }
                    }
                }
            }
            return length >= 8;
        }
    }
}
