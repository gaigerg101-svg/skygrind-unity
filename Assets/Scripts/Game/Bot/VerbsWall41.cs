// NIGHT41 builder T2 (INVENTORY A11; NIGHT41.md card T2): the verbs reel's row 13 (slide bounce) start, with a floor along the whole slide line.
// Cause (harness `n41proof slidewall --level n40-kiln --doorfree`, measured 12:40): the verbs reel runs with DOORFREE (Campaign35), so VerbsWall39.Pick
// takes kb2's 26 m east wall; the reel's start, 2 m along and 3.3 m out, is (10.00, 0.20, -70.00), on the rim of THE EAST POUR (lava pit [9, -92, 13, -70],
// floor -0.8) that runs under that wall. The 45 deg slide line crosses the pit: the player is in the Air from the first step (slid False), drops to
// y -0.66, the lava throws him up, and he meets the wall in the air at 9.9 / sqrt 2 = 7.00 m/s ("7.07 -> 10.70" in the Player, NIGHT39 to n41-r4).
// Not the game: the slide starts on kb1's 22 m wall (no DOORFREE: `n38kiln slidewall` PASS). Pick's floor test checks the rows' spots only.
// SlideStart walks the wall's length (2 m steps, both directions along it) for a start 3.3 m out whose whole 45 deg line to the wall has a floor within
// 0.3 m of the start's height at every 0.4 m (no pit, no step down); the first one found is used. Pure (the harness runs the same code).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class VerbsWall41
    {
        /// <summary>A floor within `drop` m under (x, y, z) (a ray from 1 m above; a walkable normal).</summary>
        static bool FloorNear(SurvivalSim sim, double x, double y, double z, double drop) =>
            sim.world.RayIntersect(new Vec3(x, y + 1.0, z), new Vec3(0, -1, 0), out RayHit h) && h.distance <= 1.0 + drop && h.normal.y > 0.7;

        /// <summary>Is the 45 deg slide line from `back` along `dir` floored (a floor within 0.3 m of back's height every 0.4 m to the wall)?</summary>
        public static bool LineFloored(SurvivalSim sim, Vec3 back, Vec3 dir)
        {
            for (double d = 0; d <= 3.3 * Math.Sqrt(2) + 0.4; d += 0.4)
                if (!FloorNear(sim, back.x + dir.x * d, back.y, back.z + dir.z * d, 0.3)) return false;
            return true;
        }

        /// <summary>n41-r6: is the wall face behind lane point (x, z) (0.7 m along -n) a noride box at the capsule's band from floor y (VerbsWall39's test)?
        /// R5's foundry41 band on THE GANTRY b2: the slide's jump there was a ground jump (row 13 RED). Such faces are skipped.</summary>
        public static bool NoRideFace(SurvivalSim sim, double x, double z, Vec3 n, double y)
        {
            Ride38.Box[] boxes = sim.mover != null ? sim.mover.flow34.ride35.noRide : null;
            return boxes != null && Ride38.NoRideBand(boxes, x - n.x * 0.7, z - n.z * 0.7, y + 0.1, y + 1.9, 0.1);
        }

        /// <summary>Row 13's start: Pick's wall and the reel's old start (2 m along, +along) whenever its slide line is floored and the wall from 0 to 6 m
        /// along has no noride face (NIGHT40's pick exactly); else PickSlide. `why` says which.</summary>
        public static void Row13Start(SurvivalSim sim, Vec3 st, Vec3 al, Vec3 nm, ref Vec3 back, ref Vec3 inDir, out string why)
        {
            bool noride = false;
            for (int m = 0; m <= 6 && !noride; m++) noride = NoRideFace(sim, st.x + al.x * m, st.z + al.z * m, nm, st.y - 0.2);
            bool floored = LineFloored(sim, back, inDir);
            if (floored && !noride) { why = "Pick's wall, the old start (its slide line floored, no noride face)"; return; }
            if (PickSlide(sim, out Vec3 b, out Vec3 d, out string room, out double len, out string w))
            {
                back = b;
                inDir = d;
                why = $"VerbsWall41 room {room} {len:0} m wall, {w} (Pick's: floored {floored}, noride {noride})";
                return;
            }
            why = $"Pick's wall, the old start (no other: floored {floored}, noride {noride})";
        }

        /// <summary>The row 13 start for the wall (start `st`, `al` along it, `nm` its normal, `len` m clear): `back` 3.3 m out, `inDir` 45 deg along and into
        /// the wall, the line between them floored. False: none on this wall (back / inDir are then the reel's old ones: 2 m along, +al).</summary>
        public static bool SlideStart(SurvivalSim sim, Vec3 st, Vec3 al, Vec3 nm, double len, out Vec3 back, out Vec3 inDir, out string why)
        {
            double r2 = Math.Sqrt(2);
            for (double a = 2; a <= Math.Max(2, len - 2); a += 2)
                for (int side = 1; side >= -1; side -= 2)
                {
                    // going along -al, the start sits 3.3 m further along so the contact stays on the wall
                    double at = side > 0 ? a : a + 3.3;
                    if (at > len || (side > 0 && at + 3.3 > len - 0.5)) continue; // the contact (3.3 m further along) stays on the clear wall
                    var b = new Vec3(st.x + al.x * at + nm.x * 3.3, st.y, st.z + al.z * at + nm.z * 3.3);
                    var dir = new Vec3((al.x * side - nm.x) / r2, 0, (al.z * side - nm.z) / r2);
                    bool ok = true;
                    for (double d = 0; d <= 3.3 * r2 + 0.4 && ok; d += 0.4)
                        ok = FloorNear(sim, b.x + dir.x * d, b.y, b.z + dir.z * d, 0.3);
                    if (!ok) continue;
                    back = b;
                    inDir = dir;
                    why = $"start {at:0} m along the wall, {(side > 0 ? "+" : "-")}along, the slide line floored";
                    return true;
                }
            back = new Vec3(st.x + al.x * 2 + nm.x * 3.3, st.y, st.z + al.z * 2 + nm.z * 3.3);
            inDir = new Vec3((al.x - nm.x) / r2, 0, (al.z - nm.z) / r2);
            why = "NO floored slide line on this wall (the old start)";
            return false;
        }

        /// <summary>Row 13's wall: VerbsWall39.Pick's search (rays from each non-boss room's centre, the clear length along each wall face), keeping the
        /// longest wall (>= 8 m) on which SlideStart finds a floored line. n40-kiln with DOORFREE: kb2's east wall has none (THE EAST POUR runs its whole
        /// floored length but 2 m at each end), so another wall is taken.</summary>
        public static bool PickSlide(SurvivalSim sim, out Vec3 back, out Vec3 inDir, out string room, out double length, out string why)
        {
            back = inDir = default;
            room = why = "";
            length = 0;
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
                    if (NoRideFace(sim, p0.x, p0.z, n, fy)) continue;
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
                            if (NoRideFace(sim, q.x, q.z, n, fy)) break; // n41-r6: never a noride face (foundry41's GANTRY band)
                            len = m;
                        }
                        if (len < 8 || len <= length) continue;
                        var st = new Vec3(p0.x, fy + 0.2, p0.z);
                        if (!SlideStart(sim, st, t, n, len, out Vec3 b, out Vec3 dir, out string w)) continue;
                        length = len;
                        back = b;
                        inDir = dir;
                        room = r.id;
                        why = w;
                    }
                }
            }
            return length >= 8;
        }
    }
}
