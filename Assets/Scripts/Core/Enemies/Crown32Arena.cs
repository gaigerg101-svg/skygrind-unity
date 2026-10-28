// NIGHT32 builder C (design.n32.boss5.arena; DESIGN32 C; GG 2026-10-10: "the first boss fight arena ... needs to be a circle"): THE CROWN's ring, a TRUE circle from the start.
// 64 flat wall facets (5.6 degrees each, under the Mover's cornerAngleDeg, so a wall ride goes all the way round), a closed roof, a floor of a solid INNER DISC (never drops) and an
// OUTER RING of `sectors` sectors that each drop lavaDepth m into lava (beat 2's Drop Ring: the boss names which are down by a bit mask; the pit's walls are built where an up sector
// meets a down one), and three RIM LEDGES (arc slabs against the wall, ledgeTop m high: the "high spot"). One triangle soup, the kind Lantern30Arena / Rink28Arena build:
// BoxLayer.SetTriangles for the boss room's world layer (the sim) or TriangleWorld.AddTriangle (the harness); the view draws the SAME triangles (Tris with a Part and a sector).
// ARENA-LOCAL coordinates: the centre is (0, 0, 0) on the floor; facet i covers the directions [i, i + 1) x 360 / facets degrees of atan2(z, x); sector s covers [s, s + 1) x 360 / sectors.
// The floor is a coarse 2 x sectors-gon (it runs under the wall, hidden), the wall the fine 64-gon. Pure geometry: no state, no randomness, JsMath for every angle, no UnityEngine.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public enum Crown32Part { Inner, Sector, Wall, PitWall, LedgeTop, LedgeSide, Roof }

    public struct Crown32Tri
    {
        public Vec3 a, b, c;
        public Crown32Part part;
        /// <summary>The outer sector a Sector / PitWall triangle belongs to (-1 otherwise); the ledge index for a ledge triangle.</summary>
        public int sector;
    }

    /// <summary>design.n32.boss5.arena (every number Unity-only; each has its _why in the config).</summary>
    public struct Crown32ArenaParams
    {
        public int facets, sectors, ledgeFacets;
        public double apothem, height, innerRadius, lavaDepth, ledgeTop, ledgeDepth, spawnRadius;
        public int[] ledgeSectors;

        public static Crown32ArenaParams From(GameConfig c)
        {
            const string g = "design.n32.boss5.arena.";
            double[] ls = c.Nums(g + "ledgeSectors");
            var li = new int[ls.Length];
            for (int i = 0; i < ls.Length; i++) li[i] = (int)ls[i];
            return new Crown32ArenaParams
            {
                facets = c.Int(g + "facets"), sectors = c.Int(g + "sectors"), ledgeFacets = c.Int(g + "ledgeFacets"),
                apothem = c.Num(g + "apothem"), height = c.Num(g + "height"), innerRadius = c.Num(g + "innerRadius"), lavaDepth = c.Num(g + "lavaDepth"),
                ledgeTop = c.Num(g + "ledgeTop"), ledgeDepth = c.Num(g + "ledgeDepth"), spawnRadius = c.Num(g + "spawnRadius"), ledgeSectors = li,
            };
        }
    }

    public sealed class Crown32Arena
    {
        public readonly Crown32ArenaParams p;
        public readonly List<Crown32Tri> tris = new List<Crown32Tri>();
        /// <summary>The sectors down now (bit s = sector s): what Build last laid.</summary>
        public int DownMask { get; private set; }

        public Crown32Arena(Crown32ArenaParams prm)
        {
            p = prm;
            Build(0);
        }

        public static Crown32Arena Build(GameConfig cfg) => new Crown32Arena(Crown32ArenaParams.From(cfg));

        public double FacetAngle => 2 * Math.PI / p.facets;
        public double SectorAngle => 2 * Math.PI / p.sectors;
        /// <summary>The distance from the centre to a facet's corner (the wall's seams).</summary>
        public double CornerRadius => p.apothem / JsMath.Cos(FacetAngle / 2);
        /// <summary>The floor polygon's segments (two per sector) and its outer radius (past the wall's corners: hidden under it).</summary>
        public int FloorSegments => p.sectors * 2;
        public double FloorRadius => CornerRadius / JsMath.Cos(Math.PI / FloorSegments) + 0.5;

        static double Ang(double dx, double dz)
        {
            double a = JsMath.Atan2(dz, dx);
            return a < 0 ? a + 2 * Math.PI : a;
        }

        /// <summary>The outer sector that holds the direction (dx, dz) from the centre.</summary>
        public int SectorAt(double dx, double dz)
        {
            int i = (int)Math.Floor(Ang(dx, dz) / SectorAngle);
            return i < 0 ? 0 : i >= p.sectors ? p.sectors - 1 : i;
        }

        /// <summary>The facet that holds the direction (dx, dz).</summary>
        public int FacetAt(double dx, double dz)
        {
            int i = (int)Math.Floor(Ang(dx, dz) / FacetAngle);
            return i < 0 ? 0 : i >= p.facets ? p.facets - 1 : i;
        }

        /// <summary>Is (dx, dz) on the inner disc (inside the floor polygon's inner ring, which never drops)? A circle test with the polygon's apothem (conservative).</summary>
        public bool InInner(double dx, double dz) => JsMath.Hypot(dx, dz) < p.innerRadius * JsMath.Cos(Math.PI / FloorSegments);

        /// <summary>How far (dx, dz) is from the wall of the facet that holds it (positive inside).</summary>
        public double WallDistance(double dx, double dz)
        {
            double a = (FacetAt(dx, dz) + 0.5) * FacetAngle;
            return p.apothem - (dx * JsMath.Cos(a) + dz * JsMath.Sin(a));
        }

        /// <summary>The ledge (index into ledgeSectors) whose top holds the arena-local point within `up` m above it, or -1.</summary>
        public int LedgeAt(Vec3 local, double up)
        {
            if (local.y < p.ledgeTop - 0.05 || local.y > p.ledgeTop + up) return -1;
            for (int k = 0; k < p.ledgeSectors.Length; k++)
            {
                LedgeSpan(k, out int f0, out int f1);
                int f = FacetAt(local.x, local.z);
                bool inSpan = f0 <= f1 ? f >= f0 && f < f1 : f >= f0 || f < f1;
                if (!inSpan) continue;
                double d = WallDistance(local.x, local.z);
                if (d >= -0.05 && d <= p.ledgeDepth + 0.4) return k;
            }
            return -1;
        }

        /// <summary>Ledge k's facets [f0, f1) (centred in its sector).</summary>
        public void LedgeSpan(int k, out int f0, out int f1)
        {
            int perSector = p.facets / p.sectors;
            int mid = p.ledgeSectors[k] * perSector + perSector / 2;
            f0 = ((mid - p.ledgeFacets / 2) % p.facets + p.facets) % p.facets;
            f1 = (f0 + p.ledgeFacets) % p.facets;
        }

        /// <summary>Ledge k's top centre (arena-local).</summary>
        public Vec3 LedgeCentre(int k)
        {
            LedgeSpan(k, out int f0, out _);
            double a = (f0 + p.ledgeFacets * 0.5) * FacetAngle, r = p.apothem - p.ledgeDepth / 2;
            return new Vec3(JsMath.Cos(a) * r, p.ledgeTop, JsMath.Sin(a) * r);
        }

        /// <summary>A point at radius r in direction angle a (radians of atan2(z, x)), height y (arena-local).</summary>
        public static Vec3 Polar(double a, double r, double y) => new Vec3(JsMath.Cos(a) * r, y, JsMath.Sin(a) * r);

        void Quad(Vec3 a, Vec3 b, Vec3 c, Vec3 d, Vec3 inside, Crown32Part part, int sector)
        {
            // two triangles wound so the normal points AWAY from `inside` (the solid side), as Lantern30Arena
            Tri(a, b, c, inside, part, sector);
            Tri(a, c, d, inside, part, sector);
        }

        void Tri(Vec3 a, Vec3 b, Vec3 c, Vec3 inside, Crown32Part part, int sector)
        {
            Vec3 u = b - a, v = c - a;
            var n = new Vec3(u.y * v.z - u.z * v.y, u.z * v.x - u.x * v.z, u.x * v.y - u.y * v.x);
            Vec3 mid = (a + b + c) / 3 - inside;
            if (Vec3.Dot(n, mid) < 0) tris.Add(new Crown32Tri { a = a, b = c, c = b, part = part, sector = sector });
            else tris.Add(new Crown32Tri { a = a, b = b, c = c, part = part, sector = sector });
        }

        bool Down(int s) => (DownMask & (1 << (((s % p.sectors) + p.sectors) % p.sectors))) != 0;

        /// <summary>Lay the ring with the sectors in `downMask` dropped (bit s = sector s).</summary>
        public void Build(int downMask)
        {
            DownMask = downMask;
            tris.Clear();
            int segs = FloorSegments;
            double segA = 2 * Math.PI / segs, rIn = p.innerRadius, rOut = FloorRadius, rc = CornerRadius, h = p.height, deep = -p.lavaDepth;
            // the inner disc (a fan, normal up) and the roof (normal down)
            for (int j = 0; j < segs; j++)
            {
                double a0 = j * segA, a1 = (j + 1) * segA;
                Tri(new Vec3(0, 0, 0), Polar(a0, rIn, 0), Polar(a1, rIn, 0), new Vec3(0, -1, 0), Crown32Part.Inner, -1);
                Tri(new Vec3(0, h, 0), Polar(a0, rOut, h), Polar(a1, rOut, h), new Vec3(0, h + 1, 0), Crown32Part.Roof, -1);
            }
            // the outer ring: two segments per sector, up at y 0 or down at -lavaDepth; the pit walls where an up neighbour (or the inner disc) meets a down sector
            for (int s = 0; s < p.sectors; s++)
            {
                bool down = Down(s);
                double y = down ? deep : 0;
                for (int j = 0; j < 2; j++)
                {
                    double a0 = (s * 2 + j) * segA, a1 = (s * 2 + j + 1) * segA;
                    Quad(Polar(a0, rIn, y), Polar(a0, rOut, y), Polar(a1, rOut, y), Polar(a1, rIn, y), new Vec3(0, y - 1, 0), Crown32Part.Sector, s);
                    if (down) // the inner disc's edge (solid on the disc side)
                        Quad(Polar(a0, rIn, deep), Polar(a1, rIn, deep), Polar(a1, rIn, 0), Polar(a0, rIn, 0), new Vec3(0, deep / 2, 0), Crown32Part.PitWall, s);
                }
                if (!down) continue;
                double sa0 = s * SectorAngle, sa1 = (s + 1) * SectorAngle, rm = (rIn + rOut) / 2;
                if (!Down(s - 1)) Quad(Polar(sa0, rIn, deep), Polar(sa0, rOut, deep), Polar(sa0, rOut, 0), Polar(sa0, rIn, 0), Polar(sa0 - SectorAngle / 2, rm, deep / 2), Crown32Part.PitWall, s);
                if (!Down(s + 1)) Quad(Polar(sa1, rIn, deep), Polar(sa1, rOut, deep), Polar(sa1, rOut, 0), Polar(sa1, rIn, 0), Polar(sa1 + SectorAngle / 2, rm, deep / 2), Crown32Part.PitWall, s);
            }
            // the wall: one flat quad per facet from the pit's depth to the roof, facing the centre (solid behind it)
            for (int i = 0; i < p.facets; i++)
            {
                double a0 = i * FacetAngle, a1 = (i + 1) * FacetAngle, am = (i + 0.5) * FacetAngle;
                Quad(Polar(a0, rc, deep), Polar(a1, rc, deep), Polar(a1, rc, h), Polar(a0, rc, h), Polar(am, p.apothem + 1, h / 2), Crown32Part.Wall, -1);
            }
            // the rim ledges: per facet in the span, a top quad from the wall to ledgeDepth in, a front face down to the pit's depth (the sector under it may drop), two ends
            for (int k = 0; k < p.ledgeSectors.Length; k++)
            {
                LedgeSpan(k, out int f0, out _);
                double rFront = p.apothem - p.ledgeDepth, top = p.ledgeTop;
                for (int m = 0; m < p.ledgeFacets; m++)
                {
                    int f = (f0 + m) % p.facets;
                    double a0 = f * FacetAngle, a1 = (f + 1) * FacetAngle, am = (f + 0.5) * FacetAngle;
                    double fr = rFront / JsMath.Cos(FacetAngle / 2); // the front edge parallel to the facet
                    Vec3 inside = Polar(am, p.apothem - p.ledgeDepth / 2, top / 2);
                    Quad(Polar(a0, rc, top), Polar(a1, rc, top), Polar(a1, fr, top), Polar(a0, fr, top), inside, Crown32Part.LedgeTop, k);
                    Quad(Polar(a0, fr, deep), Polar(a1, fr, deep), Polar(a1, fr, top), Polar(a0, fr, top), inside, Crown32Part.LedgeSide, k);
                    if (m == 0) Quad(Polar(a0, fr, deep), Polar(a0, rc, deep), Polar(a0, rc, top), Polar(a0, fr, top), Polar(a0 + FacetAngle / 2, p.apothem - p.ledgeDepth / 2, top / 2), Crown32Part.LedgeSide, k);
                    if (m == p.ledgeFacets - 1) Quad(Polar(a1, fr, deep), Polar(a1, rc, deep), Polar(a1, rc, top), Polar(a1, fr, top), Polar(a1 - FacetAngle / 2, p.apothem - p.ledgeDepth / 2, top / 2), Crown32Part.LedgeSide, k);
                }
            }
        }

        /// <summary>The ring in a boss room's world layer, moved to the room's origin.</summary>
        public void FillLayer(BoxLayer layer, Vec3 origin)
        {
            var list = new List<(Vec3, Vec3, Vec3)>(tris.Count);
            foreach (Crown32Tri t in tris) list.Add((t.a + origin, t.b + origin, t.c + origin));
            layer.SetTriangles(list);
        }

        /// <summary>The whole ring in a TriangleWorld (a harness), moved to `origin`.</summary>
        public void FillWorld(TriangleWorld w, Vec3 origin)
        {
            foreach (Crown32Tri t in tris) w.AddTriangle(t.a + origin, t.b + origin, t.c + origin);
        }
    }
}
