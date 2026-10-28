// NIGHT37 builder S (stag37; NIGHT37.md S1, BOSSES.md "L2 THE STAG"): THE CANAL, level 2's boss arena: a frozen canal LOOP the Stag runs and the
// player chases it round. Pure geometry and the loop's path maths (the Stag's path follower, the slipstream's "behind", the lanes), no state but
// the slab flag and the floes, no randomness, JsMath for every angle, no UnityEngine. The sim puts it in the world at the room row's
// stag37.centre (Game/SurvivalSim.Stag37.cs) as three layers: the canal (a triangle soup), the floes (boxes, refilled when one drops) and the
// lane-cut barriers (boxes).
// ARENA-LOCAL coordinates (web axes: north is -z): the centre (0, 0, 0) on the ice. The loop, counter-clockwise seen from above:
//   straight A   the east side, x = +R, from z = +half to z = -half (northward), three skate lanes (open ice);
//   turn N       a half circle round (0, -half) from east to west through the north; its OUTER wall is a ride panel (the bank);
//   straight B   the west side, x = -R, southward: lane walls (ridable, with gaps to change lanes), the TUNNEL, and the FLOES under its lanes;
//   turn S       a half circle round (0, +half) back to the east through the south; the ENTRY GAP is in its outer wall at the south apex.
// The path coordinate s runs 0..Length along the centre line from straight A's south end; lat is metres OUTWARD from the centre line (the
// inner lane is lat = -laneWidth, the outer +laneWidth). Every solid has a job: the outer walls and the lane walls are ride panels (cyan),
// the island is the loop's inside (the line), the tunnel's roof is the leg-3 set piece, the slab is the walk-in's lock.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public enum Stag37Part : byte { Floor, Wall, Island, LaneWall, Tunnel, Lintel, Slab }

    public struct Stag37Tri
    {
        public Vec3 a, b, c;
        public Stag37Part part;
    }

    /// <summary>One box solid of the canal (arena-local), with its part.</summary>
    public struct Stag37Box
    {
        public Vec3 min, max;
        public Stag37Part part;
    }

    /// <summary>One floe: a slab of straight B's ice under one lane (arena-local box), its lane index and its s range.</summary>
    public struct Stag37Floe
    {
        public Vec3 min, max;
        public int lane;
        public double s0, s1;
    }

    /// <summary>design.n37.stag37.arena.</summary>
    public sealed class Stag37ArenaParams
    {
        public double straight, turnR, laneWidth, wallHeight, islandHeight, gapHeight, tunnelRoofY, tunnelFrom, tunnelTo, laneWallHeight, laneWallThick, floeLength, floeThick, floeGap, waterY;
        public int lanes, turnFacets, gapFacets;
        /// <summary>The lane walls on straight B: pairs [from, to] in metres along B from its north end.</summary>
        public double[] laneWallRuns = new double[0];
        /// <summary>NIGHT38 S2 stag38: these numbers are the row's (the lane walls' height and runs from design.n38.stag38) and the island's faces
        /// are listed as ride faces (the view's cyan trims). False: NIGHT37's canal bit for bit.</summary>
        public bool is38, islandRide;

        /// <summary>design.n37.stag37.arena; with `stag38` (the run's NIGHT38 bit) design.n38.stag38's lane walls and island ride over it.</summary>
        public static Stag37ArenaParams From(GameConfig c, bool stag38 = false)
        {
            const string g = "design.n37.stag37.arena.";
            double N(string k) => c.Num(g + k);
            var p = new Stag37ArenaParams
            {
                straight = N("straight"), turnR = N("turnR"), laneWidth = N("laneWidth"), lanes = (int)N("lanes"), wallHeight = N("wallHeight"),
                islandHeight = N("islandHeight"), turnFacets = (int)N("turnFacets"), gapFacets = (int)N("gapFacets"), gapHeight = N("gapHeight"),
                tunnelRoofY = N("tunnelRoofY"), tunnelFrom = N("tunnelFrom"), tunnelTo = N("tunnelTo"), laneWallHeight = N("laneWallHeight"),
                laneWallThick = N("laneWallThick"), laneWallRuns = c.Nums(g + "laneWallRuns"), floeLength = N("floeLength"), floeThick = N("floeThick"),
                floeGap = N("floeGap"), waterY = N("waterY"),
            };
            const string g38 = "design.n38.stag38.";
            if (stag38 && c.Has(g38 + "laneWallHeight"))
            {
                p.is38 = true;
                p.laneWallHeight = c.Num(g38 + "laneWallHeight");
                p.laneWallRuns = c.Nums(g38 + "laneWallRuns");
                p.islandRide = c.Design("n38.stag38.islandRide");
            }
            return p;
        }
    }

    public sealed class Stag37Arena
    {
        public readonly Stag37ArenaParams p;
        public readonly List<Stag37Tri> tris = new List<Stag37Tri>();
        public readonly List<Stag37Floe> floes = new List<Stag37Floe>();
        /// <summary>The ride panels' faces for the view's cyan trim: (a, b) the bottom edge ends, inward normal, height.</summary>
        public readonly List<(Vec3 a, Vec3 b, Vec3 n, double h)> rideFaces = new List<(Vec3, Vec3, Vec3, double)>();

        public Stag37Arena(Stag37ArenaParams prm)
        {
            p = prm;
            Build();
        }

        // ---- the loop's numbers
        public double Half => p.straight / 2;
        public double CanalHalf => p.lanes * p.laneWidth / 2;
        public double OuterR => p.turnR + CanalHalf;
        public double InnerR => p.turnR - CanalHalf;
        public double TurnLength => Math.PI * p.turnR;
        public double Length => 2 * p.straight + 2 * TurnLength;
        /// <summary>The s where each part starts: straight A 0, turn N, straight B, turn S.</summary>
        public double SA => 0;
        public double STurnN => p.straight;
        public double SB => p.straight + TurnLength;
        public double STurnS => 2 * p.straight + TurnLength;
        /// <summary>The lateral offset of lane i (0 = inner).</summary>
        public double LaneLat(int i) => (i - (p.lanes - 1) * 0.5) * p.laneWidth;
        public int LaneOf(double lat)
        {
            int i = (int)Math.Floor(lat / p.laneWidth + p.lanes * 0.5);
            return i < 0 ? 0 : i >= p.lanes ? p.lanes - 1 : i;
        }
        public double Wrap(double s)
        {
            double L = Length;
            s %= L;
            return s < 0 ? s + L : s;
        }
        /// <summary>The signed path distance from s0 forward to s1, in (-Length/2, Length/2].</summary>
        public double Ahead(double s0, double s1)
        {
            double d = Wrap(s1 - s0);
            return d > Length / 2 ? d - Length : d;
        }
        public bool OnStraightB(double s) { s = Wrap(s); return s >= SB && s < STurnS; }
        public bool OnTurn(double s) { s = Wrap(s); return (s >= STurnN && s < SB) || s >= STurnS; }
        /// <summary>Metres along straight B from its north end (meaningful on B).</summary>
        public double AlongB(double s) => Wrap(s) - SB;
        public bool InTunnel(double s) { if (!OnStraightB(s)) return false; double b = AlongB(s); return b >= p.tunnelFrom && b <= p.tunnelTo; }
        /// <summary>Is there a lane wall on straight B at s (no lane change there)?</summary>
        public bool LaneWallAt(double s, double margin = 0)
        {
            if (!OnStraightB(s)) return false;
            double b = AlongB(s);
            for (int i = 0; i + 1 < p.laneWallRuns.Length; i += 2)
                if (b >= p.laneWallRuns[i] - margin && b <= p.laneWallRuns[i + 1] + margin) return true;
            return false;
        }

        /// <summary>The arena-local point at path s, lateral lat (outward), on the ice (y 0), and the path's unit heading there.</summary>
        public Vec3 PointAt(double s, double lat, out Vec3 heading)
        {
            s = Wrap(s);
            double R = p.turnR, h = Half, r = R + lat;
            if (s < STurnN) { heading = new Vec3(0, 0, -1); return new Vec3(r, 0, h - s); }
            if (s < SB)
            {
                double th = (s - STurnN) / R, c = JsMath.Cos(th), sn = JsMath.Sin(th);
                heading = new Vec3(-sn, 0, -c);
                return new Vec3(r * c, 0, -h - r * sn);
            }
            if (s < STurnS) { heading = new Vec3(0, 0, 1); return new Vec3(-r, 0, -h + (s - SB)); }
            {
                double th = (s - STurnS) / R, c = JsMath.Cos(th), sn = JsMath.Sin(th);
                heading = new Vec3(sn, 0, c);
                return new Vec3(-r * c, 0, h + r * sn);
            }
        }

        public Vec3 PointAt(double s, double lat) => PointAt(s, lat, out _);

        /// <summary>The path coordinate of an arena-local point: s on the centre line and lat outward from it.</summary>
        public void Project(double x, double z, out double s, out double lat)
        {
            double R = p.turnR, h = Half;
            if (z >= -h && z <= h)
            {
                if (x >= 0) { s = h - z; lat = x - R; }
                else { s = SB + (z + h); lat = -x - R; }
                return;
            }
            if (z < -h)
            {
                double dz = -(z + h), th = JsMath.Atan2(dz, x);
                if (th < 0) th = x >= 0 ? 0 : Math.PI;
                s = STurnN + th * R;
                lat = JsMath.Hypot(x, dz) - R;
                return;
            }
            {
                double dz = z - h, th = JsMath.Atan2(dz, -x);
                if (th < 0) th = x <= 0 ? 0 : Math.PI;
                s = STurnS + th * R;
                lat = JsMath.Hypot(x, dz) - R;
            }
        }

        /// <summary>The ground speed factor of a lane at s: a turn's lane at lat is (R + lat) / R as long as the centre line (straights: 1).</summary>
        public double ArcFactor(double s, double lat) => OnTurn(s) ? (p.turnR + lat) / p.turnR : 1;

        /// <summary>Is an arena-local point over the canal's ice (inside the outer wall, outside the island)?</summary>
        public bool OnCanal(double x, double z)
        {
            Project(x, z, out _, out double lat);
            return Math.Abs(lat) <= CanalHalf + 0.05;
        }

        /// <summary>Is an arena-local point over straight B's water (the floes' footprint)?</summary>
        public bool OverWater(double x, double z) => x < 0 && z >= -Half && z <= Half && -x >= InnerR && -x <= OuterR;

        // ---- building
        void Tri(Vec3 a, Vec3 b, Vec3 c, Vec3 solid, Stag37Part part)
        {
            Vec3 u = b - a, v = c - a;
            var n = new Vec3(u.y * v.z - u.z * v.y, u.z * v.x - u.x * v.z, u.x * v.y - u.y * v.x);
            Vec3 mid = (a + b + c) / 3 - solid;
            if (Vec3.Dot(n, mid) < 0) tris.Add(new Stag37Tri { a = a, b = c, c = b, part = part });
            else tris.Add(new Stag37Tri { a = a, b = b, c = c, part = part });
        }

        void Quad(Vec3 a, Vec3 b, Vec3 c, Vec3 d, Vec3 solid, Stag37Part part)
        {
            Tri(a, b, c, solid, part);
            Tri(a, c, d, solid, part);
        }

        /// <summary>A box as 12 outward triangles.</summary>
        void Box(Vec3 lo, Vec3 hi, Stag37Part part, bool bottom = true)
        {
            Vec3 m = (lo + hi) * 0.5;
            Vec3 V(double x, double y, double z) => new Vec3(x, y, z);
            double x0 = lo.x, y0 = lo.y, z0 = lo.z, x1 = hi.x, y1 = hi.y, z1 = hi.z;
            Quad(V(x0, y1, z0), V(x1, y1, z0), V(x1, y1, z1), V(x0, y1, z1), m, part); // top
            if (bottom) Quad(V(x0, y0, z0), V(x1, y0, z0), V(x1, y0, z1), V(x0, y0, z1), m, part);
            Quad(V(x0, y0, z0), V(x0, y1, z0), V(x0, y1, z1), V(x0, y0, z1), m, part); // -x
            Quad(V(x1, y0, z0), V(x1, y1, z0), V(x1, y1, z1), V(x1, y0, z1), m, part); // +x
            Quad(V(x0, y0, z0), V(x1, y0, z0), V(x1, y1, z0), V(x0, y1, z0), m, part); // -z
            Quad(V(x0, y0, z1), V(x1, y0, z1), V(x1, y1, z1), V(x0, y1, z1), m, part); // +z
        }

        /// <summary>The south turn's outer facet j (0 = west end ... turnFacets-1 = east end) is in the entry gap (centred on the south apex).</summary>
        public bool InGap(int j)
        {
            int first = p.turnFacets / 2 - p.gapFacets / 2;
            return j >= first && j < first + p.gapFacets;
        }

        /// <summary>The entry gap's middle on the ice (arena-local) and the inward direction (the way the player walks in: north).</summary>
        public Vec3 GapPoint => new Vec3(0, 0, Half + OuterR);

        void Build()
        {
            double h = Half, R = p.turnR, ro = OuterR, ri = InnerR, H = p.wallHeight, fa = Math.PI / p.turnFacets;
            double rf = ro / JsMath.Cos(fa / 2) + 0.05; // the facets' corners (the wall's apothem is OuterR)
            var below = new Vec3(0, -1, 0);
            // the ice: straight A, the two turns' squares (the island and the walls bound them); straight B is the floes (their own layer)
            Quad(new Vec3(ri - 0.2, 0, -h), new Vec3(ro + 0.5, 0, -h), new Vec3(ro + 0.5, 0, h), new Vec3(ri - 0.2, 0, h), below, Stag37Part.Floor);
            Quad(new Vec3(-rf, 0, -h - rf), new Vec3(rf, 0, -h - rf), new Vec3(rf, 0, -h), new Vec3(-rf, 0, -h), below, Stag37Part.Floor);
            Quad(new Vec3(-rf, 0, h), new Vec3(rf, 0, h), new Vec3(rf, 0, h + rf + 1.5), new Vec3(-rf, 0, h + rf + 1.5), below, Stag37Part.Floor);
            // the straights' outer walls (ride panels: the wall's face looks inward)
            Quad(new Vec3(ro, 0, -h), new Vec3(ro, 0, h), new Vec3(ro, H, h), new Vec3(ro, H, -h), new Vec3(ro + 1, 0, 0), Stag37Part.Wall);
            rideFaces.Add((new Vec3(ro, 0, h), new Vec3(ro, 0, -h), new Vec3(-1, 0, 0), H));
            Quad(new Vec3(-ro, 0, -h), new Vec3(-ro, 0, h), new Vec3(-ro, H, h), new Vec3(-ro, H, -h), new Vec3(-ro - 1, 0, 0), Stag37Part.Wall);
            rideFaces.Add((new Vec3(-ro, 0, -h), new Vec3(-ro, 0, h), new Vec3(1, 0, 0), H));
            // the turns: outer wall facets (the bank: ride it) and the island's rounded ends
            for (int turn = 0; turn < 2; turn++)
            {
                double cz = turn == 0 ? -h : h, sz = turn == 0 ? -1 : 1;
                for (int j = 0; j < p.turnFacets; j++)
                {
                    // facet j spans angle [j fa, (j+1) fa] from the west (x < 0) side round the far apex to the east: x = -cos, z = cz + sz sin
                    Vec3 A(double r, int k, double y) => new Vec3(-r * JsMath.Cos(k * fa), y, cz + sz * r * JsMath.Sin(k * fa));
                    Vec3 behind = new Vec3(-(ro + 2) * JsMath.Cos((j + 0.5) * fa), 0, cz + sz * (ro + 2) * JsMath.Sin((j + 0.5) * fa));
                    Vec3 inward = new Vec3(JsMath.Cos((j + 0.5) * fa), 0, -sz * JsMath.Sin((j + 0.5) * fa));
                    if (turn == 1 && InGap(j))
                    {
                        Quad(A(rf, j, p.gapHeight), A(rf, j + 1, p.gapHeight), A(rf, j + 1, H), A(rf, j, H), behind, Stag37Part.Lintel);
                        Quad(A(rf, j, 0), A(rf, j + 1, 0), A(rf, j + 1, p.gapHeight), A(rf, j, p.gapHeight), behind, Stag37Part.Slab);
                    }
                    else
                    {
                        Quad(A(rf, j, 0), A(rf, j + 1, 0), A(rf, j + 1, H), A(rf, j, H), behind, Stag37Part.Wall);
                        rideFaces.Add((A(rf, j, 0), A(rf, j + 1, 0), inward, H));
                    }
                    // the island's end: a facet wall at InnerR and its top
                    double ir = ri / JsMath.Cos(fa / 2);
                    Vec3 centre = new Vec3(0, 0, cz);
                    Quad(A(ir, j, 0), A(ir, j + 1, 0), A(ir, j + 1, p.islandHeight), A(ir, j, p.islandHeight), centre, Stag37Part.Island);
                    Tri(new Vec3(0, p.islandHeight, cz), A(ir, j, p.islandHeight), A(ir, j + 1, p.islandHeight), new Vec3(0, p.islandHeight - 1, cz), Stag37Part.Island);
                }
            }
            // the island between the straights (the loop's inside: no short cut)
            Box(new Vec3(-ri, -0.5, -h), new Vec3(ri, p.islandHeight, h), Stag37Part.Island, false);
            if (p.islandRide) // NIGHT38 S2 stag38: the island's faces are ride walls (they always were solid; now the view trims them cyan)
            {
                rideFaces.Add((new Vec3(ri, 0, h), new Vec3(ri, 0, -h), new Vec3(1, 0, 0), p.islandHeight));
                rideFaces.Add((new Vec3(-ri, 0, -h), new Vec3(-ri, 0, h), new Vec3(-1, 0, 0), p.islandHeight));
                double ir = ri / JsMath.Cos(fa / 2);
                for (int turn = 0; turn < 2; turn++)
                {
                    double cz = turn == 0 ? -h : h, sz = turn == 0 ? -1 : 1;
                    for (int j = 0; j < p.turnFacets; j++)
                    {
                        Vec3 A(int k) => new Vec3(-ir * JsMath.Cos(k * fa), 0, cz + sz * ir * JsMath.Sin(k * fa));
                        var outward = new Vec3(-JsMath.Cos((j + 0.5) * fa), 0, sz * JsMath.Sin((j + 0.5) * fa));
                        rideFaces.Add((A(j), A(j + 1), outward, p.islandHeight));
                    }
                }
            }
            // straight B: lane walls (ride panels, with gaps to change lanes), the tunnel's roof
            double wt = p.laneWallThick;
            for (int i = 0; i + 1 < p.laneWallRuns.Length; i += 2)
            {
                double z0 = -h + p.laneWallRuns[i], z1 = -h + p.laneWallRuns[i + 1];
                for (int k = 1; k < p.lanes; k++)
                {
                    double x = -(R + (k - p.lanes * 0.5) * p.laneWidth); // between lane k-1 and lane k
                    Box(new Vec3(x - wt / 2, p.waterY, z0), new Vec3(x + wt / 2, p.laneWallHeight, z1), Stag37Part.LaneWall, false);
                    rideFaces.Add((new Vec3(x - wt / 2, 0, z0), new Vec3(x - wt / 2, 0, z1), new Vec3(-1, 0, 0), p.laneWallHeight));
                    rideFaces.Add((new Vec3(x + wt / 2, 0, z1), new Vec3(x + wt / 2, 0, z0), new Vec3(1, 0, 0), p.laneWallHeight));
                }
            }
            double tz0 = -h + p.tunnelFrom, tz1 = -h + p.tunnelTo;
            Box(new Vec3(-ro - 0.5, p.tunnelRoofY, tz0), new Vec3(-ri + 0.5, p.tunnelRoofY + 1.5, tz1), Stag37Part.Tunnel);
            // the floes: one slab per lane per floeLength along B
            int n = Math.Max(1, (int)Math.Round(p.straight / p.floeLength));
            double fl = p.straight / n;
            for (int lane = 0; lane < p.lanes; lane++)
            {
                double xa = -(R + LaneLat(lane) + p.laneWidth / 2), xb = -(R + LaneLat(lane) - p.laneWidth / 2);
                for (int k = 0; k < n; k++)
                {
                    double z0 = -h + k * fl + p.floeGap / 2, z1 = -h + (k + 1) * fl - p.floeGap / 2;
                    floes.Add(new Stag37Floe { min = new Vec3(xa, -p.floeThick, z0), max = new Vec3(xb, 0, z1), lane = lane, s0 = SB + k * fl, s1 = SB + (k + 1) * fl });
                }
            }
        }

        /// <summary>The canal in a world layer at `origin` (the slab only when `slab`).</summary>
        public void FillLayer(BoxLayer layer, Vec3 origin, bool slab)
        {
            var list = new List<(Vec3, Vec3, Vec3)>(tris.Count);
            foreach (Stag37Tri t in tris)
                if (slab || t.part != Stag37Part.Slab) list.Add((t.a + origin, t.b + origin, t.c + origin));
            layer.SetTriangles(list);
        }

        /// <summary>The floes that stand (down[i] false) in a world layer at `origin`.</summary>
        public void FillFloes(BoxLayer layer, Vec3 origin, bool[] down)
        {
            var list = new List<(Vec3, Vec3)>(floes.Count);
            for (int i = 0; i < floes.Count; i++)
                if (down == null || i >= down.Length || !down[i]) list.Add((floes[i].min + origin, floes[i].max + origin));
            layer.SetBoxes(list);
        }

        /// <summary>The floe under an arena-local point (-1: none).</summary>
        public int FloeAt(double x, double z)
        {
            for (int i = 0; i < floes.Count; i++)
            {
                Stag37Floe f = floes[i];
                if (x >= f.min.x && x <= f.max.x && z >= f.min.z && z <= f.max.z) return i;
            }
            return -1;
        }
    }
}
