// NIGHT30 builder B (lantern30; DESIGN30 C1 + I): THE LANTERN RING, a closed ring of FLAT wall panels (the Mover's wall ride turns at most cornerAngleDeg per ride, so a
// circle is a polygon: 16 panels of 22.5 degrees by default, the reviewer's fix, measured by the harness `seam30`), a floor that goes live in the cycle (Lantern29's), a closed roof,
// three platforms AGAINST the wall at three panels (a refuge from the live floor; shots from them do no damage: Lantern30), wall-ride lanes on the panels (y laneLow..laneHigh,
// a view strip only: the whole panel is rideable) and three heat vents on the floor (positions the view glows before the floor goes live). One triangle soup, the same kind as
// Rink28Arena: BoxLayer.SetTriangles for the boss room's world layer (the sim) or TriangleWorld.AddTriangle (the harness). The view draws the SAME triangles (Tris with a Part).
// ARENA-LOCAL coordinates: the centre is (0, 0, 0) on the floor; panel i covers the directions [i, i + 1) x 360 / panels degrees of atan2(z, x) (PanelAt).
// NIGHT32 builder A (design.n32.arenas32; GG 2026-10-10: "the first boss fight arena is not completely circular it needs to be a circle"): while the row is on, each of the 16
// panels is drawn AND collided as facetsPerPanel flat facets (64: 5.6 degree seams, harness `ring32` measures the wall-ride lap), the floor and roof are discs, the platforms arcs.
// The 16 panels stay the fight's logical sections (PanelAt, the sweep, the vents, the platforms, the pilot). Row off: NIGHT31's code path, untouched.
// Pure geometry: no state, no randomness, JsMath for every angle, no UnityEngine.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public enum Lantern30Part { Floor, Panel, Roof, PlatformTop, PlatformSide }

    public struct Lantern30Tri
    {
        public Vec3 a, b, c;
        public Lantern30Part part;
        /// <summary>The panel a Panel / Platform triangle belongs to (-1 for the floor and the roof).</summary>
        public int panel;
    }

    /// <summary>design.n30.lantern30.arena (every number Unity-only; each has its _why in the config).</summary>
    public struct Lantern30ArenaParams
    {
        public int panels;
        public double apothem, height, platformTop, platformDepth, platformInset, laneLow, laneHigh, ventRadius;
        public int[] platformPanels, ventPanels;
        /// <summary>NIGHT32 A (design.n32.arenas32.facetsPerPanel, read only while the row is on): each logical panel is drawn AND collided
        /// as this many flat facets, so the ring is a 16 x 4 = 64-gon (GG 2026-10-10: "it needs to be a circle"). 0 or 1: NIGHT31's flat panels.
        /// The panels stay the fight's 16 sections (PanelAt, the sweep, the vents, the platforms, the pilot).</summary>
        public int facetsPerPanel;

        /// <summary>The ring for a run: facets only while design.n32.arenas32 is on for it (the sim passes its run's n32.arenas32).</summary>
        public static Lantern30ArenaParams From(GameConfig c, bool round32)
        {
            const string g = "design.n30.lantern30.arena.";
            var p = new Lantern30ArenaParams
            {
                panels = c.Int(g + "panels"), apothem = c.Num(g + "apothem"), height = c.Num(g + "height"),
                platformTop = c.Num(g + "platformTop"), platformDepth = c.Num(g + "platformDepth"), platformInset = c.Num(g + "platformInset"),
                laneLow = c.Num(g + "laneLow"), laneHigh = c.Num(g + "laneHigh"), ventRadius = c.Num(g + "ventRadius"),
                platformPanels = Ints(c.Nums(g + "platformPanels")), ventPanels = Ints(c.Nums(g + "ventPanels")),
            };
            if (round32 && c.Has("design.n32.arenas32.facetsPerPanel")) p.facetsPerPanel = c.Int("design.n32.arenas32.facetsPerPanel");
            return p;
        }

        /// <summary>The ring with the design's arenas32 switch (off with --off32 / -skygrindOff32 and every older off switch). A caller that
        /// knows its run's blocks passes them to From(c, round32) instead (a tape's replay).</summary>
        public static Lantern30ArenaParams From(GameConfig c) => From(c, Round32(c));

        /// <summary>Is design.n32.arenas32 on by the config (and not forced off)?</summary>
        public static bool Round32(GameConfig c) => !N32Blocks.ForceOff && c.Design("n32.arenas32.enabled");

        static int[] Ints(double[] v)
        {
            var r = new int[v.Length];
            for (int i = 0; i < v.Length; i++) r[i] = (int)v[i];
            return r;
        }
    }

    public sealed class Lantern30Arena
    {
        public readonly Lantern30ArenaParams p;
        public readonly List<Lantern30Tri> tris = new List<Lantern30Tri>();

        public Lantern30Arena(Lantern30ArenaParams prm)
        {
            p = prm;
            Build();
        }

        public static Lantern30Arena Build(GameConfig cfg) => new Lantern30Arena(Lantern30ArenaParams.From(cfg));

        public static Lantern30Arena Build(GameConfig cfg, bool round32) => new Lantern30Arena(Lantern30ArenaParams.From(cfg, round32));

        // --- NIGHT32 A: the round ring (facets) ----------------------------------------------------------------------------------------------

        /// <summary>Is the ring drawn and collided as facets (design.n32.arenas32 on, facetsPerPanel above 1)?</summary>
        public bool Round => p.facetsPerPanel > 1;
        /// <summary>The wall's flat faces: panels x facetsPerPanel when Round, else the panels.</summary>
        public int Facets => Round ? p.panels * p.facetsPerPanel : p.panels;
        /// <summary>The angle one facet spans (radians); the panel angle when not Round.</summary>
        public double FacetAngle => Round ? 2 * Math.PI / Facets : PanelAngle;
        /// <summary>The seam before facet j (radians of atan2(z, x)).</summary>
        public double FacetSeamAngle(int j) => j * FacetAngle;
        /// <summary>The distance from the centre to a facet's corner on the wall (CornerRadius when not Round).</summary>
        public double FacetCornerRadius => Round ? p.apothem / JsMath.Cos(FacetAngle / 2) : CornerRadius;

        /// <summary>The facet that holds the direction (dx, dz) (the panel when not Round).</summary>
        public int FacetAt(double dx, double dz)
        {
            if (!Round) return PanelAt(dx, dz);
            double a = JsMath.Atan2(dz, dx);
            if (a < 0) a += 2 * Math.PI;
            int j = (int)Math.Floor(a / FacetAngle);
            return j < 0 ? 0 : j >= Facets ? Facets - 1 : j;
        }

        /// <summary>Facet j's inward unit normal (InwardNormal(j) when not Round).</summary>
        public Vec3 FacetNormal(int j)
        {
            if (!Round) return InwardNormal(j);
            double a = (j + 0.5) * FacetAngle;
            return new Vec3(-JsMath.Cos(a), 0, -JsMath.Sin(a));
        }

        /// <summary>The wall's inward normal at the direction (dx, dz): the facet's (the panel's when not Round).</summary>
        public Vec3 InwardNormalAt(double dx, double dz) => Round ? FacetNormal(FacetAt(dx, dz)) : InwardNormal(PanelAt(dx, dz));

        /// <summary>The wall's counter-clockwise unit direction at (dx, dz): the facet's (the panel's when not Round).</summary>
        public Vec3 TangentAt(double dx, double dz)
        {
            if (!Round) return Tangent(PanelAt(dx, dz));
            Vec3 n = FacetNormal(FacetAt(dx, dz));
            return new Vec3(n.z, 0, -n.x);
        }

        /// <summary>The point at angle `a` (radians of atan2(z, x)) on the ring's polygon pulled in to apothem `apo` (the wall at p.apothem),
        /// at height y: on the facet polygon when Round, on the panel polygon otherwise. Views and the platforms use it so they follow the wall.</summary>
        public Vec3 OnRing(double a, double apo, double y)
        {
            double fa = FacetAngle;
            int j = (int)Math.Floor(a / fa);
            double off = a - (j + 0.5) * fa;
            double r = apo / JsMath.Cos(off);
            return new Vec3(JsMath.Cos(a) * r, y, JsMath.Sin(a) * r);
        }

        /// <summary>The angles from a0 to a1 (a0 &lt; a1) with every facet seam strictly between them: consecutive pairs are straight pieces
        /// of the facet polygon (a strip built on them follows the wall exactly).</summary>
        public List<double> ArcBreaks(double a0, double a1)
        {
            var r = new List<double> { a0 };
            double fa = FacetAngle;
            for (int j = (int)Math.Floor(a0 / fa) + 1; j * fa < a1 - 1e-9; j++)
                if (j * fa > a0 + 1e-9) r.Add(j * fa);
            r.Add(a1);
            return r;
        }

        /// <summary>The angle one panel spans (radians).</summary>
        public double PanelAngle => 2 * Math.PI / p.panels;
        /// <summary>The distance from the centre to a panel's corner (the seam).</summary>
        public double CornerRadius => p.apothem / JsMath.Cos(PanelAngle / 2);
        /// <summary>A panel's width along the wall.</summary>
        public double PanelWidth => 2 * p.apothem * JsMath.Sin(PanelAngle / 2) / JsMath.Cos(PanelAngle / 2);

        /// <summary>The panel that holds the direction (dx, dz) from the centre.</summary>
        public int PanelAt(double dx, double dz)
        {
            double a = JsMath.Atan2(dz, dx);
            if (a < 0) a += 2 * Math.PI;
            int i = (int)Math.Floor(a / PanelAngle);
            return i < 0 ? 0 : i >= p.panels ? p.panels - 1 : i;
        }

        /// <summary>Panel i's centre direction (radians of atan2(z, x)).</summary>
        public double PanelCentreAngle(int i) => (i + 0.5) * PanelAngle;

        /// <summary>The seam between panel i - 1 and panel i (radians).</summary>
        public double SeamAngle(int i) => i * PanelAngle;

        /// <summary>Panel i's inward unit normal (towards the centre).</summary>
        public Vec3 InwardNormal(int i)
        {
            double a = PanelCentreAngle(i);
            return new Vec3(-JsMath.Cos(a), 0, -JsMath.Sin(a));
        }

        /// <summary>Panel i's unit direction counter-clockwise (increasing angle) along the wall.</summary>
        public Vec3 Tangent(int i)
        {
            double a = PanelCentreAngle(i);
            return new Vec3(-JsMath.Sin(a), 0, JsMath.Cos(a));
        }

        /// <summary>How far (dx, dz) is from the wall of the panel that holds it (positive inside).</summary>
        public double WallDistance(double dx, double dz)
        {
            if (Round)
            {
                Vec3 fn = FacetNormal(FacetAt(dx, dz)); // NIGHT32 A: the facet that holds the point (the wall it would touch)
                return p.apothem + (dx * fn.x + dz * fn.z);
            }
            int i = PanelAt(dx, dz);
            Vec3 n = InwardNormal(i);
            return p.apothem + (dx * n.x + dz * n.z);
        }

        /// <summary>Does a point (arena-local) stand on a platform's top (feet within `up` m above it)?</summary>
        public bool OnPlatform(Vec3 local, double up)
        {
            if (local.y < p.platformTop - 0.05 || local.y > p.platformTop + up) return false;
            int i = PanelAt(local.x, local.z);
            if (Array.IndexOf(p.platformPanels, i) < 0) return false;
            double d = WallDistance(local.x, local.z);
            return d >= -0.05 && d <= p.platformDepth + 0.4;
        }

        /// <summary>The heat vents' floor positions (arena-local): the view glows them in the tell before the floor goes live.</summary>
        public Vec3 Vent(int k)
        {
            double a = PanelCentreAngle(p.ventPanels[k]);
            return new Vec3(JsMath.Cos(a) * p.ventRadius, 0, JsMath.Sin(a) * p.ventRadius);
        }

        Vec3 Corner(int i, double r, double y)
        {
            double a = SeamAngle(i);
            return new Vec3(JsMath.Cos(a) * r, y, JsMath.Sin(a) * r);
        }

        void Quad(Vec3 a, Vec3 b, Vec3 c, Vec3 d, Vec3 inside, Lantern30Part part, int panel)
        {
            // two triangles wound so the normal points AWAY from `inside` (the solid side)
            Tri(a, b, c, inside, part, panel);
            Tri(a, c, d, inside, part, panel);
        }

        void Tri(Vec3 a, Vec3 b, Vec3 c, Vec3 inside, Lantern30Part part, int panel)
        {
            Vec3 u = b - a, v = c - a;
            var n = new Vec3(u.y * v.z - u.z * v.y, u.z * v.x - u.x * v.z, u.x * v.y - u.y * v.x);
            Vec3 mid = (a + b + c) / 3 - inside;
            if (Vec3.Dot(n, mid) < 0) tris.Add(new Lantern30Tri { a = a, b = c, c = b, part = part, panel = panel });
            else tris.Add(new Lantern30Tri { a = a, b = b, c = c, part = part, panel = panel });
        }

        /// <summary>NIGHT32 A: the round ring. The floor and the roof are discs (a fan of Facets triangles reaching 1 m behind the wall), the
        /// wall is Facets flat quads (5.6 degree seams at 64: the ride's corner rule is 30), and each platform is an arc slab that follows the
        /// wall over its panel (inset platformInset m of arc from each seam), built on the facet seams so its back edge lies on the wall.</summary>
        void BuildRound()
        {
            double h = p.height, fa = FacetAngle, rf = (p.apothem + 1) / JsMath.Cos(fa / 2), rw = FacetCornerRadius;
            int nf = Facets;
            Vec3 Ring(int j, double r, double y) { double a = j * fa; return new Vec3(JsMath.Cos(a) * r, y, JsMath.Sin(a) * r); }
            for (int j = 0; j < nf; j++)
            {
                Tri(new Vec3(0, 0, 0), Ring(j, rf, 0), Ring(j + 1, rf, 0), new Vec3(0, -1, 0), Lantern30Part.Floor, -1);
                Tri(new Vec3(0, h, 0), Ring(j, rf, h), Ring(j + 1, rf, h), new Vec3(0, h + 1, 0), Lantern30Part.Roof, -1);
            }
            for (int j = 0; j < nf; j++)
            {
                Vec3 a0 = Ring(j, rw, 0), a1 = Ring(j + 1, rw, 0), b1 = Ring(j + 1, rw, h), b0 = Ring(j, rw, h);
                Vec3 behind = -FacetNormal(j) * (p.apothem + 1);
                Quad(a0, a1, b1, b0, behind, Lantern30Part.Panel, j / p.facetsPerPanel);
            }
            foreach (int k in p.platformPanels)
            {
                double inset = p.platformInset / p.apothem; // the inset as an arc angle at the wall
                List<double> br = ArcBreaks(SeamAngle(k) + inset, SeamAngle(k + 1) - inset);
                Vec3 up = new Vec3(0, p.platformTop, 0);
                double front = p.apothem - p.platformDepth;
                for (int s = 0; s + 1 < br.Count; s++)
                {
                    Vec3 bL = OnRing(br[s], p.apothem, 0), bR = OnRing(br[s + 1], p.apothem, 0);
                    Vec3 fL = OnRing(br[s], front, 0), fR = OnRing(br[s + 1], front, 0);
                    Vec3 inside = (bL + bR + fL + fR) / 4 + up * 0.5;
                    Quad(bL + up, bR + up, fR + up, fL + up, inside, Lantern30Part.PlatformTop, k);
                    Quad(fL, fR, fR + up, fL + up, inside, Lantern30Part.PlatformSide, k);
                    if (s == 0) Quad(bL, fL, fL + up, bL + up, inside, Lantern30Part.PlatformSide, k);
                    if (s + 2 == br.Count) Quad(bR, fR, fR + up, bR + up, inside, Lantern30Part.PlatformSide, k);
                }
            }
        }

        void Build()
        {
            if (Round) { BuildRound(); return; }
            double rc = CornerRadius, h = p.height, big = rc + 4;
            // the floor (a square under the whole ring, normal up) and the closed roof (normal down)
            Quad(new Vec3(-big, 0, -big), new Vec3(big, 0, -big), new Vec3(big, 0, big), new Vec3(-big, 0, big), new Vec3(0, -1, 0), Lantern30Part.Floor, -1);
            Quad(new Vec3(-big, h, -big), new Vec3(big, h, -big), new Vec3(big, h, big), new Vec3(-big, h, big), new Vec3(0, h + 1, 0), Lantern30Part.Roof, -1);
            for (int i = 0; i < p.panels; i++)
            {
                // the panel: a flat quad from seam i to seam i + 1, facing the centre (solid behind it)
                Vec3 a0 = Corner(i, rc, 0), a1 = Corner(i + 1, rc, 0), b1 = Corner(i + 1, rc, h), b0 = Corner(i, rc, h);
                Vec3 behind = -InwardNormal(i) * (p.apothem + 1);
                Quad(a0, a1, b1, b0, behind, Lantern30Part.Panel, i);
            }
            // the platforms: a slab against panel k, platformDepth deep, inset platformInset from each seam, top at platformTop (solid under it)
            foreach (int k in p.platformPanels)
            {
                Vec3 n = InwardNormal(k), t = Tangent(k);
                double half = PanelWidth / 2 - p.platformInset;
                Vec3 wallMid = -n * p.apothem; // the panel's centre on the floor (n points inward, so the wall is at -n * apothem)
                Vec3 backL = wallMid - t * half, backR = wallMid + t * half;
                Vec3 frontL = backL + n * p.platformDepth, frontR = backR + n * p.platformDepth;
                Vec3 up = new Vec3(0, p.platformTop, 0);
                Vec3 inside = wallMid + n * (p.platformDepth / 2) + up * 0.5;
                Quad(backL + up, backR + up, frontR + up, frontL + up, inside, Lantern30Part.PlatformTop, k);
                Quad(frontL, frontR, frontR + up, frontL + up, inside, Lantern30Part.PlatformSide, k);
                Quad(backL, frontL, frontL + up, backL + up, inside, Lantern30Part.PlatformSide, k);
                Quad(backR, frontR, frontR + up, backR + up, inside, Lantern30Part.PlatformSide, k);
            }
        }

        /// <summary>The ring in a boss room's world layer, moved to the room's origin.</summary>
        public void FillLayer(BoxLayer layer, Vec3 origin)
        {
            var list = new List<(Vec3, Vec3, Vec3)>(tris.Count);
            foreach (Lantern30Tri t in tris) list.Add((t.a + origin, t.b + origin, t.c + origin));
            layer.SetTriangles(list);
        }

        /// <summary>The whole ring in a TriangleWorld (the harness), moved to `origin`.</summary>
        public void FillWorld(TriangleWorld w, Vec3 origin)
        {
            foreach (Lantern30Tri t in tris) w.AddTriangle(t.a + origin, t.b + origin, t.c + origin);
        }
    }
}
