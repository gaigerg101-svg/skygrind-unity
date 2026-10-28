// NIGHT28 (builder A2, arena + ice): THE GLACIER's rink as a REAL curved arena of collision triangles (Research/2026-10-09-wave28/GLACIER-REDESIGN.md section 3.1; GG: "you can't even create a proper
// arena ... you can't even curve the surface"). It walks Rink28Math's stadium centre line and, at a list of stations along it (the straights' ends, the two door edges on each long side, and one station
// per `facetDeg` round each end cap), lays a PROFILE across the line at fixed lateral offsets (positive outward):
//     island wall (lateral -(rc - islandRadius), 0 .. islandHeight)  |  flat LANE (laneWidth)  |  BANK (bankDeg, bankRun)  |  flat LIP (lip)  |  vertical RIM wall (rimHeight)
// with the island's top and two DOORS (a 4 m wide cut through the island's long sides at the middles, floor kept), and an optional collision LID at the rim top. Every triangle's normal points out of the
// solid (into the arena). The result is one triangle soup: BoxLayer.SetTriangles (a boss room's world layer) or TriangleWorld.AddTriangle (the harness). The view draws the SAME triangles
// (Game/RinkView28.cs), the boss builder places things with Rink28Math and this class's accessors, and `IceZone` is the ONE surface zone of the whole rink (the ice rule).
// Pure math on JsMath; no engine references; deterministic. Mover.cs is untouched: the bank is floor-grade (normal.y 0.883 >= 0.7) and the rim is a wall (normal.y 0).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public enum Rink28Part { Floor, Bank, Lip, Rim, IslandWall, IslandTop, DoorWall, DoorFloor, Lid }

    public struct Rink28Tri
    {
        public Vec3 a, b, c;
        public Rink28Part part;
    }

    /// <summary>Where a plan point is: the flat lane, the bank, the flat lip, on top of the island, in a door, or outside the playable rink (rim / beyond).</summary>
    public enum Rink28Region { Lane, Bank, Lip, Island, Door, Outside }

    public struct Rink28Params
    {
        public double cx, cy, cz; // the rink's centre in the sim; cy is the floor's y
        public double straight, centreRadius, islandRadius, islandHeight, doorWidth, laneWidth, bankDeg, bankRun, lip, rimHeight, facetDeg;
        public bool lid;
        public double playerStartS, spawnGap;

        public static bool Enabled(GameConfig cfg) => cfg.Has("design.n28.rink.enabled") && cfg.Bool("design.n28.rink.enabled");

        /// <summary>design.n28.rink.* (each number has a _why in Design/unity-config.json).</summary>
        public static Rink28Params From(GameConfig cfg)
        {
            double N(string k) => cfg.Num("design.n28.rink." + k);
            double[] o = cfg.Nums("design.n28.rink.origin");
            return new Rink28Params
            {
                cx = o[0], cy = o[1], cz = o[2],
                straight = N("straight"), centreRadius = N("centreRadius"), islandRadius = N("islandRadius"), islandHeight = N("islandHeight"), doorWidth = N("doorWidth"),
                laneWidth = N("laneWidth"), bankDeg = N("bankDeg"), bankRun = N("bankRun"), lip = N("lip"), rimHeight = N("rimHeight"), facetDeg = N("facetDeg"),
                lid = cfg.Bool("design.n28.rink.lid"), playerStartS = N("playerStartS"), spawnGap = N("spawnGap"),
            };
        }

        /// <summary>NIGHT30 B (design.n30.glacier30): design.n28.rink with every number found under `over` (e.g. "design.n30.glacier30.rink.") replacing its own.</summary>
        public static Rink28Params From(GameConfig cfg, string over)
        {
            Rink28Params r = From(cfg);
            double O(string k, double v) => cfg.Has(over + k) ? cfg.Num(over + k) : v;
            if (cfg.Has(over + "origin")) { double[] o = cfg.Nums(over + "origin"); r.cx = o[0]; r.cy = o[1]; r.cz = o[2]; }
            r.straight = O("straight", r.straight); r.centreRadius = O("centreRadius", r.centreRadius); r.islandRadius = O("islandRadius", r.islandRadius); r.islandHeight = O("islandHeight", r.islandHeight);
            r.doorWidth = O("doorWidth", r.doorWidth); r.laneWidth = O("laneWidth", r.laneWidth); r.bankDeg = O("bankDeg", r.bankDeg); r.bankRun = O("bankRun", r.bankRun); r.lip = O("lip", r.lip);
            r.rimHeight = O("rimHeight", r.rimHeight); r.facetDeg = O("facetDeg", r.facetDeg); r.playerStartS = O("playerStartS", r.playerStartS); r.spawnGap = O("spawnGap", r.spawnGap);
            return r;
        }
    }

    public sealed class Rink28Arena
    {
        public readonly Rink28Params p;
        public readonly Rink28Math math;
        public readonly List<Rink28Tri> tris = new List<Rink28Tri>();
        /// <summary>The station arc coordinates round the loop (0 = the start of the bottom straight), ascending; the loop closes back to station 0 at Lap.</summary>
        public readonly List<double> stations = new List<double>();

        // lateral offsets from the centre line (positive outward) and heights (sim y)
        public readonly double latIsland, latLaneOut, latBankTop, latLip;
        public readonly double floorY, bankRise, bankTopY, rimTopY, islandTopY, tanBank;

        int idxDoorB0, idxDoorB1, idxDoorT0, idxDoorT1, capEast0, capEast1;

        public Rink28Arena(Rink28Params prm)
        {
            p = prm;
            math = new Rink28Math(p.straight, p.centreRadius, p.cx, p.cz);
            double rad = p.bankDeg * Math.PI / 180;
            tanBank = JsMath.Sin(rad) / JsMath.Cos(rad);
            latIsland = p.islandRadius - p.centreRadius;
            latLaneOut = latIsland + p.laneWidth;
            latBankTop = latLaneOut + p.bankRun;
            latLip = latBankTop + p.lip;
            floorY = p.cy;
            bankRise = p.bankRun * tanBank;
            bankTopY = floorY + bankRise;
            rimTopY = bankTopY + p.rimHeight;
            islandTopY = floorY + p.islandHeight;
            BuildStations();
            BuildTriangles();
        }

        public static Rink28Arena Build(GameConfig cfg) => new Rink28Arena(Rink28Params.From(cfg));

        public double Lap => math.Lap;
        public int CapFacets => Math.Max(2, (int)JsMath.Round(180 / p.facetDeg));

        // --- accessors for the boss, the spawn, the view and the tests -------------------------------------------------------------------------------------

        /// <summary>The lateral offset of the outer edge of the flat lane (where the bank starts).</summary>
        public double LaneOutLat => latLaneOut;
        /// <summary>The lateral offset of the rim wall's foot (the lip's outer edge).</summary>
        public double RimLat => latLip;
        /// <summary>Half the flat floor width: the lane runs from -LaneHalf to +LaneHalf round the centre line.</summary>
        public double LaneHalf => p.laneWidth / 2;

        /// <summary>The floor height at a lateral offset (the profile only: no island, no doors).</summary>
        public double HeightAtLateral(double lat)
        {
            if (lat <= latLaneOut) return floorY;
            if (lat <= latBankTop) return floorY + (lat - latLaneOut) * tanBank;
            return bankTopY;
        }

        /// <summary>The point on the floor at arc s and a lateral offset (y by the profile; the island's top is not here: use IslandPoint).</summary>
        public Vec3 FloorPoint(double s, double lateral)
        {
            math.PointAt(s, lateral, out double x, out double z, out _, out _);
            return new Vec3(x, HeightAtLateral(lateral), z);
        }

        /// <summary>Which part of the rink a plan point is over.</summary>
        public Rink28Region RegionAt(double x, double z)
        {
            double dw = p.doorWidth / 2, dz = p.islandRadius;
            if (Math.Abs(x - p.cx) < dw && Math.Abs(z - p.cz) < dz) return Rink28Region.Door;
            math.Project(x, z, out _, out double lat);
            if (lat < latIsland) return Rink28Region.Island;
            if (lat <= latLaneOut) return Rink28Region.Lane;
            if (lat <= latBankTop) return Rink28Region.Bank;
            if (lat <= latLip) return Rink28Region.Lip;
            return Rink28Region.Outside;
        }

        /// <summary>The walkable surface height at a plan point (floor, bank, lip, door floor, island top); NaN outside the rink.</summary>
        public double SurfaceYAt(double x, double z)
        {
            switch (RegionAt(x, z))
            {
                case Rink28Region.Island: return islandTopY;
                case Rink28Region.Door: return floorY;
                case Rink28Region.Outside: return double.NaN;
            }
            math.Project(x, z, out _, out double lat);
            return HeightAtLateral(lat);
        }

        /// <summary>The player's spawn: on the lane's centre line at playerStartS, facing the direction of travel (counter-clockwise), and the same arc coordinate for the boss `spawnGap` m ahead.</summary>
        public void Spawn(out Vec3 position, out double yawDeg)
        {
            math.PointAt(p.playerStartS, 0, out double x, out double z, out double tx, out double tz);
            position = new Vec3(x, floorY, z);
            yawDeg = Rink28Math.YawDegLooking(tx, tz); // yaw 0 looks toward -z: forward = (-sin yaw, -cos yaw)
        }

        /// <summary>The boss's start arc coordinate (the boss builder reads this): spawnGap m ahead of the player along the loop.</summary>
        public double BossStartS => math.Wrap(p.playerStartS + p.spawnGap);

        /// <summary>The ONE ice rule: a single surface zone over the whole rink's bounding box (x / z beyond the rim, y from -1e9 to 1e9). Lane, banks, lip, island top and doors all sit under it, so
        /// Surfaces26.FrictionMult returns the same multiplier everywhere; there is no other zone and no boost strip.</summary>
        public SurfaceZone IceZone()
        {
            double hx = p.straight / 2 + latLip + p.centreRadius + 8, hz = p.centreRadius + latLip + 8;
            return new SurfaceZone { kind = SurfaceKind.Ice, x0 = p.cx - hx, x1 = p.cx + hx, z0 = p.cz - hz, z1 = p.cz + hz, minY = -1e9, maxY = 1e9 };
        }

        /// <summary>The whole rink as a boss room's world layer (rays on: shots hit the rim and the island).</summary>
        public void FillLayer(BoxLayer layer)
        {
            var list = new List<(Vec3, Vec3, Vec3)>(tris.Count);
            foreach (Rink28Tri t in tris) list.Add((t.a, t.b, t.c));
            layer.SetTriangles(list);
        }

        /// <summary>The whole rink in a TriangleWorld (the harness).</summary>
        public void FillWorld(TriangleWorld w)
        {
            foreach (Rink28Tri t in tris) w.AddTriangle(t.a, t.b, t.c);
        }

        // --- the generator ---------------------------------------------------------------------------------------------------------------------------------

        void BuildStations()
        {
            double L = p.straight, cap = math.CapLength, dw = p.doorWidth / 2;
            int n = CapFacets;
            stations.Add(0);
            idxDoorB0 = stations.Count; stations.Add(L / 2 - dw);
            idxDoorB1 = stations.Count; stations.Add(L / 2 + dw);
            capEast0 = stations.Count; stations.Add(L);
            for (int k = 1; k <= n; k++) stations.Add(L + cap * k / n);
            capEast1 = stations.Count - 1; // = L + cap
            idxDoorT0 = stations.Count; stations.Add(L + cap + L / 2 - dw);
            idxDoorT1 = stations.Count; stations.Add(L + cap + L / 2 + dw);
            stations.Add(2 * L + cap);
            for (int k = 1; k < n; k++) stations.Add(2 * L + cap + cap * k / n);
        }

        Vec3 At(int i, double lat, double y)
        {
            math.PointAt(stations[i % stations.Count], lat, out double x, out double z, out _, out _);
            return new Vec3(x, y, z);
        }

        Vec3 Outward(int i)
        {
            double s = stations[i % stations.Count];
            math.PointAt(s, 0, out double x0, out double z0, out _, out _);
            math.PointAt(s, 1, out double x1, out double z1, out _, out _);
            return new Vec3(x1 - x0, 0, z1 - z0);
        }

        static Vec3 Cross(Vec3 a, Vec3 b) => new Vec3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);

        void Tri(Rink28Part part, Vec3 a, Vec3 b, Vec3 c, Vec3 want)
        {
            Vec3 n = Cross(b - a, c - a);
            if (Vec3.Dot(n, want) < 0) { Vec3 t = b; b = c; c = t; }
            tris.Add(new Rink28Tri { a = a, b = b, c = c, part = part });
        }

        /// <summary>A quad given round its perimeter (a, b, c, d), its normal toward `want`.</summary>
        void Quad(Rink28Part part, Vec3 a, Vec3 b, Vec3 c, Vec3 d, Vec3 want)
        {
            Tri(part, a, b, c, want);
            Tri(part, a, c, d, want);
        }

        void BuildTriangles()
        {
            int count = stations.Count;
            Vec3 up = new Vec3(0, 1, 0), down = new Vec3(0, -1, 0);
            double y0 = floorY;
            for (int i = 0; i < count; i++)
            {
                int j = i + 1;
                bool door = (i == idxDoorB0) || (i == idxDoorT0);
                Vec3 outw = Outward(i) + Outward(j), inw = outw * -1;
                // the flat lane
                Quad(Rink28Part.Floor, At(i, latIsland, y0), At(i, latLaneOut, y0), At(j, latLaneOut, y0), At(j, latIsland, y0), up);
                // the bank, the lip, the rim wall
                Quad(Rink28Part.Bank, At(i, latLaneOut, y0), At(i, latBankTop, bankTopY), At(j, latBankTop, bankTopY), At(j, latLaneOut, y0), up);
                Quad(Rink28Part.Lip, At(i, latBankTop, bankTopY), At(i, latLip, bankTopY), At(j, latLip, bankTopY), At(j, latBankTop, bankTopY), up);
                Quad(Rink28Part.Rim, At(i, latLip, bankTopY), At(i, latLip, rimTopY), At(j, latLip, rimTopY), At(j, latLip, bankTopY), inw);
                // the island's wall (not across a door)
                if (!door) Quad(Rink28Part.IslandWall, At(i, latIsland, y0), At(i, latIsland, islandTopY), At(j, latIsland, islandTopY), At(j, latIsland, y0), outw);
            }
            // the island's top, one convex fan per piece (east and west of the doors)
            var east = new List<Vec3>();
            for (int i = idxDoorB1; i <= idxDoorT0; i++) east.Add(At(i, latIsland, islandTopY));
            Fan(Rink28Part.IslandTop, new Vec3(p.cx + p.straight / 2, islandTopY, p.cz), east, up);
            var west = new List<Vec3>();
            for (int i = idxDoorT1; i < count; i++) west.Add(At(i, latIsland, islandTopY));
            west.Add(At(0, latIsland, islandTopY));
            west.Add(At(idxDoorB0, latIsland, islandTopY));
            Fan(Rink28Part.IslandTop, new Vec3(p.cx - p.straight / 2, islandTopY, p.cz), west, up);
            // the doors: the walls that face the opening, and the floor through it
            Vec3 bE = At(idxDoorB1, latIsland, y0), bW = At(idxDoorB0, latIsland, y0), tE = At(idxDoorT0, latIsland, y0), tW = At(idxDoorT1, latIsland, y0);
            Vec3 bEt = At(idxDoorB1, latIsland, islandTopY), bWt = At(idxDoorB0, latIsland, islandTopY), tEt = At(idxDoorT0, latIsland, islandTopY), tWt = At(idxDoorT1, latIsland, islandTopY);
            Quad(Rink28Part.DoorWall, bE, tE, tEt, bEt, new Vec3(-1, 0, 0)); // the east piece's west face
            Quad(Rink28Part.DoorWall, bW, tW, tWt, bWt, new Vec3(1, 0, 0)); // the west piece's east face
            Quad(Rink28Part.DoorFloor, bE, bW, tW, tE, up);
            // the collision lid at the rim top
            if (p.lid)
            {
                var ring = new List<Vec3>();
                for (int i = 0; i < count; i++) ring.Add(At(i, latLip, rimTopY));
                ring.Add(ring[0]);
                Fan(Rink28Part.Lid, new Vec3(p.cx, rimTopY, p.cz), ring, down);
            }
        }

        void Fan(Rink28Part part, Vec3 centre, List<Vec3> ring, Vec3 want)
        {
            for (int k = 0; k + 1 < ring.Count; k++) Tri(part, centre, ring[k], ring[k + 1], want);
            // an island piece's ring is open: the closing edge runs from the last vertex back to the first
            if (part == Rink28Part.IslandTop) Tri(part, centre, ring[ring.Count - 1], ring[0], want);
        }
    }
}
