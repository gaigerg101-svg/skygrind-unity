// NIGHT36 builder B (boss36; Design/levels/n36/lantern-hall.spec.md): THE LANTERN's HALL, level 1's last room, as one triangle soup the sim
// puts in the world (a BoxLayer of triangles, as Lantern30Arena / Rink28Arena) at the room row's lantern36.centre, from level load:
//   a ring of `facets` flat wall facets (apothem m from the centre, height m tall), the floor and the closed roof as discs, and the ENTRY GAP:
//   the `gapFacets` facets centred on +z (90 deg, the side the player walks in from) are open from the floor to gapHeight; the SLAB
//   (Slab = true) fills them while the fight runs (the doors lock). No platform, no pillar: every solid has a job (PLAN.md 3.3).
// ARENA-LOCAL coordinates: the centre (0, 0, 0) on the floor; facet j covers the directions [j, j + 1) x 360 / facets degrees of atan2(z, x).
// Pure geometry: no state but the slab flag, no randomness, JsMath for every angle, no UnityEngine.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public enum Lantern36Part : byte { Floor, Wall, Roof, Lintel, Slab }

    public struct Lantern36Tri
    {
        public Vec3 a, b, c;
        public Lantern36Part part;
        public int facet;
    }

    /// <summary>design.n36.boss36.lantern.arena.</summary>
    public struct Lantern36ArenaParams
    {
        public int facets, gapFacets;
        public double apothem, height, gapHeight, trimLow, trimHigh;

        public static Lantern36ArenaParams From(GameConfig c)
        {
            const string g = "design.n36.boss36.lantern.arena.";
            return new Lantern36ArenaParams
            {
                facets = c.Int(g + "facets"), gapFacets = c.Int(g + "gapFacets"), apothem = c.Num(g + "apothem"), height = c.Num(g + "height"),
                gapHeight = c.Num(g + "gapHeight"), trimLow = c.Num(g + "trimLow"), trimHigh = c.Num(g + "trimHigh"),
            };
        }
    }

    public sealed class Lantern36Arena
    {
        public readonly Lantern36ArenaParams p;
        /// <summary>Every triangle (the slab's included: FillLayer skips them while the gap is open).</summary>
        public readonly List<Lantern36Tri> tris = new List<Lantern36Tri>();

        public Lantern36Arena(Lantern36ArenaParams prm)
        {
            p = prm;
            Build();
        }

        public double FacetAngle => 2 * Math.PI / p.facets;
        public double CornerRadius => p.apothem / JsMath.Cos(FacetAngle / 2);
        public double FacetWidth => 2 * p.apothem * JsMath.Sin(FacetAngle / 2) / JsMath.Cos(FacetAngle / 2);
        /// <summary>The gap's width along the wall (the door the player walks through).</summary>
        public double GapWidth => FacetWidth * p.gapFacets;

        /// <summary>Is facet j one of the entry gap's (centred on +z, 90 deg)?</summary>
        public bool InGap(int j)
        {
            // +z (90 deg) is a seam when facets is a multiple of 4: the gap's facets sit symmetrically around it
            int mid = p.facets / 4;
            int first = mid - p.gapFacets / 2;
            int k = ((j - first) % p.facets + p.facets) % p.facets;
            return k < p.gapFacets;
        }

        public int FacetAt(double dx, double dz)
        {
            double a = JsMath.Atan2(dz, dx);
            if (a < 0) a += 2 * Math.PI;
            int j = (int)Math.Floor(a / FacetAngle);
            return j < 0 ? 0 : j >= p.facets ? p.facets - 1 : j;
        }

        /// <summary>Facet j's inward unit normal.</summary>
        public Vec3 FacetNormal(int j)
        {
            double a = (j + 0.5) * FacetAngle;
            return new Vec3(-JsMath.Cos(a), 0, -JsMath.Sin(a));
        }

        /// <summary>How far (dx, dz) is inside the wall (positive inside).</summary>
        public double WallDistance(double dx, double dz)
        {
            Vec3 n = FacetNormal(FacetAt(dx, dz));
            return p.apothem + (dx * n.x + dz * n.z);
        }

        /// <summary>The point at angle `a` (radians) on the facet polygon pulled in to apothem `apo`, at height y.</summary>
        public Vec3 OnRing(double a, double apo, double y)
        {
            double fa = FacetAngle;
            int j = (int)Math.Floor(a / fa);
            double off = a - (j + 0.5) * fa;
            double r = apo / JsMath.Cos(off);
            return new Vec3(JsMath.Cos(a) * r, y, JsMath.Sin(a) * r);
        }

        void Tri(Vec3 a, Vec3 b, Vec3 c, Vec3 inside, Lantern36Part part, int facet)
        {
            Vec3 u = b - a, v = c - a;
            var n = new Vec3(u.y * v.z - u.z * v.y, u.z * v.x - u.x * v.z, u.x * v.y - u.y * v.x);
            Vec3 mid = (a + b + c) / 3 - inside;
            if (Vec3.Dot(n, mid) < 0) tris.Add(new Lantern36Tri { a = a, b = c, c = b, part = part, facet = facet });
            else tris.Add(new Lantern36Tri { a = a, b = b, c = c, part = part, facet = facet });
        }

        void Quad(Vec3 a, Vec3 b, Vec3 c, Vec3 d, Vec3 inside, Lantern36Part part, int facet)
        {
            Tri(a, b, c, inside, part, facet);
            Tri(a, c, d, inside, part, facet);
        }

        void Build()
        {
            double h = p.height, fa = FacetAngle, rw = CornerRadius, rf = (p.apothem + 1) / JsMath.Cos(fa / 2);
            Vec3 Ring(int j, double r, double y) { double a = j * fa; return new Vec3(JsMath.Cos(a) * r, y, JsMath.Sin(a) * r); }
            for (int j = 0; j < p.facets; j++)
            {
                // the floor reaches 1 m behind the wall (it meets the hall's floor under the gap), the roof closes the top
                Tri(new Vec3(0, 0, 0), Ring(j, rf, 0), Ring(j + 1, rf, 0), new Vec3(0, -1, 0), Lantern36Part.Floor, -1);
                Tri(new Vec3(0, h, 0), Ring(j, rw, h), Ring(j + 1, rw, h), new Vec3(0, h + 1, 0), Lantern36Part.Roof, -1);
                Vec3 behind = -FacetNormal(j) * (p.apothem + 1);
                if (InGap(j))
                {
                    double gh = p.gapHeight;
                    Quad(Ring(j, rw, gh), Ring(j + 1, rw, gh), Ring(j + 1, rw, h), Ring(j, rw, h), behind, Lantern36Part.Lintel, j);
                    Quad(Ring(j, rw, 0), Ring(j + 1, rw, 0), Ring(j + 1, rw, gh), Ring(j, rw, gh), behind, Lantern36Part.Slab, j);
                }
                else Quad(Ring(j, rw, 0), Ring(j + 1, rw, 0), Ring(j + 1, rw, h), Ring(j, rw, h), behind, Lantern36Part.Wall, j);
            }
        }

        /// <summary>The hall in a world layer at `origin`: the slab only when `slab` (the fight's lock).</summary>
        public void FillLayer(BoxLayer layer, Vec3 origin, bool slab)
        {
            var list = new List<(Vec3, Vec3, Vec3)>(tris.Count);
            foreach (Lantern36Tri t in tris)
                if (slab || t.part != Lantern36Part.Slab) list.Add((t.a + origin, t.b + origin, t.c + origin));
            layer.SetTriangles(list);
        }

        /// <summary>The hall in a TriangleWorld (the harness).</summary>
        public void FillWorld(TriangleWorld w, Vec3 origin, bool slab)
        {
            foreach (Lantern36Tri t in tris)
                if (slab || t.part != Lantern36Part.Slab) w.AddTriangle(t.a + origin, t.b + origin, t.c + origin);
        }
    }
}
