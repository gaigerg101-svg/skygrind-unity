// NIGHT37 builder W (wick37; NIGHT37.md W1, Research/2026-10-12-wave37/BOSSES.md "L1 THE WICK"): THE STACK, level 1's last room, as one triangle
// soup the sim puts in the world (a BoxLayer of triangles, as Lantern36Arena) at the room row's wick37.centre, from level load, and THE SLAG,
// the rising hazard plane inside it.
//   THE SHAFT   a round wall of `facets` flat facets (apothem `radius`), from the floor to capY; the ENTRY GAP (gapFacets facets centred on +z, the
//               side the player walks in from) open to gapHeight, with a short tunnel (two side walls and a roof) out to the room's door.
//               The shaft's wall is HOT iron: the sim throws a rider off it (SurvivalSim.Wick37.cs), so only the panels can be ridden.
//   THE PANELS  a spiral of flat ride panels (cyan), perTurn per turn, each `rise` higher than the one before, their ride face `panelApothem` from
//               the axis. The spiral is split in sections by the LEDGE RINGS (1.2 m wide floors round the whole wall: the leg checkpoints);
//               no panel crosses a ledge. A jump-off from a panel's far end carries along a chord of the shaft into the next panel's face.
//   THE CAP     a ring catwalk from the shaft's rim (radius) out to capRadius at capY, and its outer wall: one 64-facet ring (ride a full lap).
// ARENA-LOCAL coordinates: the centre (0, 0, 0) on the floor on the shaft's axis; angles are atan2(z, x); the spiral climbs counter-clockwise
// in those terms (dir = +1). Pure geometry + the slag's height: no randomness, JsMath for every angle, no UnityEngine.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public enum Wick37Part : byte { Shaft, Panel, Ledge, Cap, CapWall, Tunnel, Floor, Glass /* NIGHT47 wickTop47: the glass over the shaft at the cap */, Step /* NIGHT47 wick47: the bottom section's steps */ }

    public struct Wick37Tri
    {
        public Vec3 a, b, c;
        public Wick37Part part;
        public int index; // the panel's or ledge's index (-1 for the rest)
    }

    /// <summary>One ride panel: its ride face's centre angle, bottom, the face's two ends (arena-local, at the bottom) and the ride direction.</summary>
    public struct Wick37Panel
    {
        public int index, section;
        public double angle, bottom, top;
        public Vec3 start, end; // the ride face's ends at the bottom (start -> end = the spiral's way)
        public Vec3 normal;     // the ride face's normal (toward the axis)
    }

    /// <summary>design.n37.wick37.stack.</summary>
    public struct Wick37StackParams
    {
        public int facets, gapFacets, perTurn, ledgeSegments;
        public double radius, capY, gapHeight, tunnelLength, panelApothem, panelLength, panelHeight, panelThickness, rise, firstBottom, firstAngleDeg;
        public double ledgeWidth, ledgeThickness, ledgeOver, ledgeArcDeg, capRadius, capWallHeight, capThickness, sectionStartAbove;
        public int panelCount;
        /// <summary>The ledges' tops (computed by Wick37Stack from the spiral: not in config).</summary>
        public double[] ledges;
        public double[] ledgeAfter;
        /// <summary>NIGHT47 wick47 (n47-foundry's STACK, from the level row): the rise per section (floor to ledge 1, ledge 1 to ledge 2, ...); null
        /// = `rise` for every panel (NIGHT46's stack bit for bit).</summary>
        public double[] riseBySection;
        /// <summary>NIGHT47 wick47: the panel height and the ride face length per section; null = panelHeight / panelLength (NIGHT46 bit for bit).</summary>
        public double[] heightBySection, lengthBySection;
        /// <summary>NIGHT47 wick47 (round 3, the lead: the bottom's move is a jump, not a ride): stepCount plain STEP arcs before the panels, each
        /// stepRise over the last (the first over the floor), stepDeg round the spiral, stepArcDeg wide, stepDepth deep from the wall; the last
        /// step is ledge 1 (a checkpoint); the panels start in the next slot. 0 = none (NIGHT46 bit for bit).</summary>
        public int stepCount;
        public double stepRise, stepDeg, stepArcDeg, stepDepth, stepToPanelDeg;
        /// <summary>NIGHT47 wick47 (round 4): each step's own arc (deg); given, the steps stand stepGapDeg apart edge to edge (stepDeg unused).
        /// Null / empty: every step stepArcDeg wide, stepDeg apart.</summary>
        public double[] stepArcs;
        /// <summary>NIGHT47 wick47 (round 4): each step's own depth from the wall (m); null / empty: stepDepth.</summary>
        public double[] stepDepths;
        public double stepGapDeg;
        /// <summary>NIGHT48 climbTop48 (n48-foundry's STACK, G1's top third): a LIP (a catch shelf, built as a Step part: folded in P1, out in
        /// P2) at the far end of each panel named here: lipArcDeg wide from lipStartDeg past the panel's end angle, lipDepth deep from the wall,
        /// its top lipOver over that panel's bottom (a late release past the panel's end lands on it; a short hop rides the next panel). Null /
        /// empty: none (NIGHT47 bit for bit).</summary>
        public double[] lipAfter;
        public double lipOver, lipArcDeg, lipStartDeg, lipDepth;

        public static Wick37StackParams From(GameConfig c)
        {
            const string g = "design.n37.wick37.stack.";
            double N(string k) => c.Num(g + k);
            return new Wick37StackParams
            {
                facets = c.Int(g + "facets"), gapFacets = c.Int(g + "gapFacets"), perTurn = c.Int(g + "perTurn"), ledgeSegments = c.Int(g + "ledgeSegments"),
                radius = N("radius"), gapHeight = N("gapHeight"), tunnelLength = N("tunnelLength"),
                panelApothem = N("panelApothem"), panelLength = N("panelLength"), panelHeight = N("panelHeight"), panelThickness = N("panelThickness"),
                rise = N("rise"), firstBottom = N("firstBottom"), firstAngleDeg = N("firstAngleDeg"),
                ledgeWidth = N("ledgeWidth"), ledgeThickness = N("ledgeThickness"), ledgeOver = N("ledgeOver"), ledgeArcDeg = N("ledgeArcDeg"),
                capRadius = N("capRadius"), capWallHeight = N("capWallHeight"), capThickness = N("capThickness"), sectionStartAbove = N("sectionStartAbove"),
                panelCount = c.Int(g + "panelCount"), ledgeAfter = c.Nums(g + "ledgeAfter"),
            };
        }
    }

    public sealed class Wick37Stack
    {
        public readonly Wick37StackParams p;
        public readonly List<Wick37Tri> tris = new List<Wick37Tri>();
        public readonly List<Wick37Panel> panels = new List<Wick37Panel>();

        /// <summary>The ledge arcs' centre angles (radians, atan2(z, x)); their heights are p.ledges.</summary>
        public double[] ledgeAngle = new double[0];

        public Wick37Stack(Wick37StackParams prm)
        {
            // THE SPIRAL (round 2): one slot every 360 / perTurn degrees, a panel in each; after the panels named by ledgeAfter, the NEXT slot is a
            // LEDGE ARC instead (ledgeOver over the panel before it: a leap lands on it), and the next panel stands sectionStartAbove over the ledge.
            // After the last panel the cap's catwalk is ledgeOver over its bottom. The ledge heights and the cap's height are computed here.
            p = prm;
            double step = 2 * Math.PI / Math.Max(1, p.perTurn), ang = p.firstAngleDeg * Math.PI / 180, b = p.firstBottom;
            var lys = new List<double>();
            var las = new List<double>();
            var after = new List<int>();
            foreach (double v in p.ledgeAfter) after.Add((int)v);
            if (p.stepCount > 0) // NIGHT47 wick47: the steps first (the last is ledge 1), then the panels from the next slot
            {
                double sa = ang;
                for (int k = 0; k < p.stepCount; k++)
                {
                    stepTops.Add((k + 1) * p.stepRise);
                    stepAngles.Add(sa);
                    stepArcList.Add(StepArc(k));
                    if (k < p.stepCount - 1) sa += p.stepArcs != null && p.stepArcs.Length > 0 ? (StepArc(k) + StepArc(k + 1)) / 2 + p.stepGapDeg * Math.PI / 180 : p.stepDeg * Math.PI / 180;
                }
                lys.Add(stepTops[stepTops.Count - 1]);
                las.Add(sa);
                ang = sa + (p.stepToPanelDeg > 0 ? p.stepToPanelDeg * Math.PI / 180 : step);
                b = stepTops[stepTops.Count - 1] + p.sectionStartAbove;
            }
            for (int i = 0; i < p.panelCount; i++)
            {
                slots.Add((ang, b, lys.Count));
                ang += step;
                if (after.Contains(i))
                {
                    double ly = b + p.ledgeOver;
                    lys.Add(ly);
                    las.Add(ang);
                    ang += step;
                    b = ly + p.sectionStartAbove;
                }
                else b += p.riseBySection != null && p.riseBySection.Length > 0 ? p.riseBySection[Math.Min(lys.Count, p.riseBySection.Length - 1)] : p.rise; // NIGHT47 wick47: the section's rise (null: NIGHT46)
            }
            double lastBottom = slots.Count > 0 ? slots[slots.Count - 1].bottom : 0;
            p.capY = lastBottom + p.ledgeOver;
            // the cap's LANDING: one more arc, at the catwalk's height in the slot after the last panel, joined to the catwalk (round 2b: the leap
            // off the last panel flies a chord inside the shaft and never reached the rim at 9 m)
            lys.Add(p.capY);
            las.Add(ang);
            p.ledges = lys.ToArray();
            ledgeAngle = las.ToArray();
            Build();
        }

        readonly List<(double ang, double bottom, int section)> slots = new List<(double, double, int)>();
        /// <summary>NIGHT47 wick47: the steps' tops and centre angles (empty: none).</summary>
        public readonly List<double> stepTops = new List<double>(), stepAngles = new List<double>(), stepArcList = new List<double>();
        /// <summary>NIGHT47 wick47: step k's arc (radians).</summary>
        public double StepDepth(int k) => p.stepDepths != null && p.stepDepths.Length > 0 ? p.stepDepths[Math.Min(k, p.stepDepths.Length - 1)] : p.stepDepth;
        public double StepArc(int k) => (p.stepArcs != null && p.stepArcs.Length > 0 ? p.stepArcs[Math.Min(k, p.stepArcs.Length - 1)] : p.stepArcDeg) * Math.PI / 180;
        /// <summary>NIGHT48 climbTop48: the lips (catch shelves at top-third panels' ends): the panel each follows, its top (arena-local),
        /// its first and last angle (radians, atan2(z, x), counter-clockwise) and its inner radius. Empty: none.</summary>
        public readonly List<int> lipPanel = new List<int>();
        public readonly List<double> lipTops = new List<double>(), lipFrom = new List<double>(), lipTo = new List<double>();
        public double LipInner => p.radius - p.lipDepth;

        /// <summary>NIGHT48 climbTop48: the lip whose arc and depth hold arena-local feet l within `over` m over its top (and 0.6 m under it),
        /// or -1.</summary>
        public int LipAt(Vec3 l, double over)
        {
            if (lipTops.Count == 0) return -1;
            double r = JsMath.Hypot(l.x, l.z), a = JsMath.Atan2(l.z, l.x);
            for (int i = 0; i < lipTops.Count; i++)
            {
                double d0 = a - lipFrom[i];
                while (d0 > Math.PI) d0 -= 2 * Math.PI;
                while (d0 < -Math.PI) d0 += 2 * Math.PI;
                if (d0 < -0.05 || d0 > lipTo[i] - lipFrom[i] + 0.05) continue;
                if (r < LipInner - 0.4 || l.y < lipTops[i] - 0.6 || l.y > lipTops[i] + over) continue;
                return i;
            }
            return -1;
        }

        public double FacetAngle => 2 * Math.PI / p.facets;
        public double CornerRadius => p.radius / JsMath.Cos(FacetAngle / 2);
        public double FacetWidth => 2 * p.radius * JsMath.Sin(FacetAngle / 2) / JsMath.Cos(FacetAngle / 2);
        /// <summary>The gap's half width along +z's tangent (the tunnel's side walls stand here).</summary>
        public double GapHalf => FacetWidth * p.gapFacets / 2;
        /// <summary>The ledge heights (tops) and the cap: the leg checkpoints' floors, low to high.</summary>
        public double LedgeTop(int i) => p.ledges[i];

        public bool InGap(int j)
        {
            int mid = p.facets / 4;
            int first = mid - p.gapFacets / 2;
            int k = ((j - first) % p.facets + p.facets) % p.facets;
            return k < p.gapFacets;
        }

        static Vec3 Polar(double a, double r, double y) => new Vec3(JsMath.Cos(a) * r, y, JsMath.Sin(a) * r);

        /// <summary>NIGHT47 wick47: a per-section number (null or empty: the default).</summary>
        static double BySection(double[] v, int section, double d) => v != null && v.Length > 0 ? v[Math.Min(section, v.Length - 1)] : d;

        void Tri(Vec3 a, Vec3 b, Vec3 c, Vec3 outward, Wick37Part part, int index)
        {
            Vec3 u = b - a, v = c - a;
            var n = new Vec3(u.y * v.z - u.z * v.y, u.z * v.x - u.x * v.z, u.x * v.y - u.y * v.x);
            if (Vec3.Dot(n, outward) < 0) tris.Add(new Wick37Tri { a = a, b = c, c = b, part = part, index = index });
            else tris.Add(new Wick37Tri { a = a, b = b, c = c, part = part, index = index });
        }

        /// <summary>A quad whose face normal points along `outward` (out of the solid, into the air).</summary>
        void Quad(Vec3 a, Vec3 b, Vec3 c, Vec3 d, Vec3 outward, Wick37Part part, int index = -1)
        {
            Tri(a, b, c, outward, part, index);
            Tri(a, c, d, outward, part, index);
        }

        void Build()
        {
            double fa = FacetAngle, rw = CornerRadius, top = p.capY;
            // THE SHAFT: inward normals; the gap's facets open to gapHeight
            for (int j = 0; j < p.facets; j++)
            {
                double a0 = j * fa, a1 = (j + 1) * fa, am = (j + 0.5) * fa;
                Vec3 inward = new Vec3(-JsMath.Cos(am), 0, -JsMath.Sin(am));
                double y0 = InGap(j) ? p.gapHeight : 0;
                Quad(Polar(a0, rw, y0), Polar(a1, rw, y0), Polar(a1, rw, top), Polar(a0, rw, top), inward, Wick37Part.Shaft);
            }
            // THE FLOOR: a disc at y 0 reaching under the wall (the level's own floor is coplanar with it; the harness's map has none)
            for (int j = 0; j < p.facets; j++)
                Tri(new Vec3(0, 0, 0), Polar(j * fa, rw + 0.6, 0), Polar((j + 1) * fa, rw + 0.6, 0), new Vec3(0, 1, 0), Wick37Part.Floor, -1);
            // THE TUNNEL: from the room's door to the gap (side walls at +-GapHalf, a roof at gapHeight)
            double gh = GapHalf, zIn = Math.Sqrt(Math.Max(0, p.radius * p.radius - gh * gh)) - 0.4, zOut = p.radius + p.tunnelLength;
            Quad(new Vec3(-gh, 0, zIn), new Vec3(-gh, 0, zOut), new Vec3(-gh, p.gapHeight, zOut), new Vec3(-gh, p.gapHeight, zIn), new Vec3(1, 0, 0), Wick37Part.Tunnel);
            Quad(new Vec3(gh, 0, zIn), new Vec3(gh, 0, zOut), new Vec3(gh, p.gapHeight, zOut), new Vec3(gh, p.gapHeight, zIn), new Vec3(-1, 0, 0), Wick37Part.Tunnel);
            Quad(new Vec3(-gh, p.gapHeight, zIn), new Vec3(gh, p.gapHeight, zIn), new Vec3(gh, p.gapHeight, zOut), new Vec3(-gh, p.gapHeight, zOut), new Vec3(0, -1, 0), Wick37Part.Tunnel);
            Quad(new Vec3(-gh, 0, zIn), new Vec3(gh, 0, zIn), new Vec3(gh, 0, zOut), new Vec3(-gh, 0, zOut), new Vec3(0, 1, 0), Wick37Part.Floor);
            // THE LEDGES: arcs (ledgeArcDeg wide, in a slot of the spiral) from radius - ledgeWidth out behind the wall; top, underside, the inner
            // lip and the two end faces
            int ls = Math.Max(2, p.ledgeSegments);
            double arc = p.ledgeArcDeg * Math.PI / 180, sa = arc / ls, rOut = rw + 0.4;
            for (int i = 0; i < p.ledges.Length; i++)
            {
                if (p.stepCount > 0 && i == 0) continue; // NIGHT47 wick47: ledge 1 is the last step (built below, the step's arc)
                double yt = p.ledges[i], yb = yt - p.ledgeThickness, rIn = (p.radius - p.ledgeWidth) / JsMath.Cos(sa / 2), a00 = ledgeAngle[i] - arc / 2;
                for (int k = 0; k < ls; k++)
                {
                    double a0 = a00 + k * sa, a1 = a00 + (k + 1) * sa, am = a00 + (k + 0.5) * sa;
                    Quad(Polar(a0, rIn, yt), Polar(a1, rIn, yt), Polar(a1, rOut, yt), Polar(a0, rOut, yt), new Vec3(0, 1, 0), Wick37Part.Ledge, i);
                    Quad(Polar(a0, rIn, yb), Polar(a1, rIn, yb), Polar(a1, rOut, yb), Polar(a0, rOut, yb), new Vec3(0, -1, 0), Wick37Part.Ledge, i);
                    Quad(Polar(a0, rIn, yb), Polar(a1, rIn, yb), Polar(a1, rIn, yt), Polar(a0, rIn, yt), new Vec3(-JsMath.Cos(am), 0, -JsMath.Sin(am)), Wick37Part.Ledge, i);
                }
                double e0 = a00, e1 = a00 + arc;
                Quad(Polar(e0, rIn, yb), Polar(e0, rOut, yb), Polar(e0, rOut, yt), Polar(e0, rIn, yt), new Vec3(JsMath.Sin(e0), 0, -JsMath.Cos(e0)), Wick37Part.Ledge, i);
                Quad(Polar(e1, rIn, yb), Polar(e1, rOut, yb), Polar(e1, rOut, yt), Polar(e1, rIn, yt), new Vec3(-JsMath.Sin(e1), 0, JsMath.Cos(e1)), Wick37Part.Ledge, i);
            }
            // NIGHT47 wick47: THE STEPS (the last is ledge 1): arcs stepArcDeg wide, stepDepth deep, ledgeThickness thick
            for (int i = 0; i < stepTops.Count; i++)
            {
                double arcS = StepArc(i), saS = arcS / ls;
                double yt = stepTops[i], yb = yt - p.ledgeThickness, rIn = (p.radius - StepDepth(i)) / JsMath.Cos(saS / 2), a00 = stepAngles[i] - arcS / 2;
                for (int k = 0; k < ls; k++)
                {
                    double a0 = a00 + k * saS, a1 = a00 + (k + 1) * saS, am = a00 + (k + 0.5) * saS;
                    Quad(Polar(a0, rIn, yt), Polar(a1, rIn, yt), Polar(a1, rOut, yt), Polar(a0, rOut, yt), new Vec3(0, 1, 0), Wick37Part.Step, -1);
                    Quad(Polar(a0, rIn, yb), Polar(a1, rIn, yb), Polar(a1, rOut, yb), Polar(a0, rOut, yb), new Vec3(0, -1, 0), Wick37Part.Step, -1);
                    Quad(Polar(a0, rIn, yb), Polar(a1, rIn, yb), Polar(a1, rIn, yt), Polar(a0, rIn, yt), new Vec3(-JsMath.Cos(am), 0, -JsMath.Sin(am)), Wick37Part.Step, -1);
                }
                double e0 = a00, e1 = a00 + arcS;
                Quad(Polar(e0, rIn, yb), Polar(e0, rOut, yb), Polar(e0, rOut, yt), Polar(e0, rIn, yt), new Vec3(JsMath.Sin(e0), 0, -JsMath.Cos(e0)), Wick37Part.Step, -1);
                Quad(Polar(e1, rIn, yb), Polar(e1, rOut, yb), Polar(e1, rOut, yt), Polar(e1, rIn, yt), new Vec3(-JsMath.Sin(e1), 0, JsMath.Cos(e1)), Wick37Part.Step, -1);
            }
            // THE CAP: the catwalk (radius .. capRadius) at capY, its lip down into the shaft, and the outer ring wall (a ride lap)
            double cfa = fa, crw = p.capRadius / JsMath.Cos(cfa / 2), cyb = top - p.capThickness;
            for (int j = 0; j < p.facets; j++)
            {
                double a0 = j * cfa, a1 = (j + 1) * cfa, am = (j + 0.5) * cfa;
                Quad(Polar(a0, rw, top), Polar(a1, rw, top), Polar(a1, crw + 0.4, top), Polar(a0, crw + 0.4, top), new Vec3(0, 1, 0), Wick37Part.Cap);
                Quad(Polar(a0, rw, cyb), Polar(a1, rw, cyb), Polar(a1, crw + 0.4, cyb), Polar(a0, crw + 0.4, cyb), new Vec3(0, -1, 0), Wick37Part.Cap);
                Quad(Polar(a0, crw, top), Polar(a1, crw, top), Polar(a1, crw, top + p.capWallHeight), Polar(a0, crw, top + p.capWallHeight),
                    new Vec3(-JsMath.Cos(am), 0, -JsMath.Sin(am)), Wick37Part.CapWall);
            }
            // THE PANELS: the spiral's slots (the constructor laid them out)
            for (int i = 0; i < slots.Count; i++) AddPanel(i, slots[i].section, slots[i].ang, slots[i].bottom, BySection(p.heightBySection, slots[i].section, p.panelHeight)); // NIGHT47 wick47: the section's height (null: panelHeight)
            // NIGHT48 climbTop48: THE LIPS (after the panels: their ends are known), arcs like the steps, Step parts (folded in P1)
            if (p.lipAfter != null && p.lipAfter.Length > 0) BuildLips(ls, rOut);
        }

        void BuildLips(int ls, double rOut)
        {
            foreach (double v in p.lipAfter)
            {
                int pi = (int)v;
                if (pi < 0 || pi >= panels.Count) continue;
                Wick37Panel pn = panels[pi];
                double half = JsMath.Atan2((pn.end - pn.start).Length * 0.5, p.panelApothem);
                double a0s = pn.angle + half + p.lipStartDeg * Math.PI / 180, arcL = p.lipArcDeg * Math.PI / 180, saL = arcL / ls;
                double yt = pn.bottom + p.lipOver, yb = yt - p.ledgeThickness, rIn = LipInner / JsMath.Cos(saL / 2);
                lipPanel.Add(pi);
                lipTops.Add(yt);
                lipFrom.Add(a0s);
                lipTo.Add(a0s + arcL);
                for (int k = 0; k < ls; k++)
                {
                    double a0 = a0s + k * saL, a1 = a0s + (k + 1) * saL, am = a0s + (k + 0.5) * saL;
                    Quad(Polar(a0, rIn, yt), Polar(a1, rIn, yt), Polar(a1, rOut, yt), Polar(a0, rOut, yt), new Vec3(0, 1, 0), Wick37Part.Step, -1);
                    Quad(Polar(a0, rIn, yb), Polar(a1, rIn, yb), Polar(a1, rOut, yb), Polar(a0, rOut, yb), new Vec3(0, -1, 0), Wick37Part.Step, -1);
                    Quad(Polar(a0, rIn, yb), Polar(a1, rIn, yb), Polar(a1, rIn, yt), Polar(a0, rIn, yt), new Vec3(-JsMath.Cos(am), 0, -JsMath.Sin(am)), Wick37Part.Step, -1);
                }
                double e0 = a0s, e1 = a0s + arcL;
                Quad(Polar(e0, rIn, yb), Polar(e0, rOut, yb), Polar(e0, rOut, yt), Polar(e0, rIn, yt), new Vec3(JsMath.Sin(e0), 0, -JsMath.Cos(e0)), Wick37Part.Step, -1);
                Quad(Polar(e1, rIn, yb), Polar(e1, rOut, yb), Polar(e1, rOut, yt), Polar(e1, rIn, yt), new Vec3(-JsMath.Sin(e1), 0, JsMath.Cos(e1)), Wick37Part.Step, -1);
            }
        }

        void AddPanel(int index, int section, double ang, double bottom, double height)
        {
            double c = JsMath.Cos(ang), s = JsMath.Sin(ang), half = BySection(p.lengthBySection, section, p.panelLength) / 2, ap = p.panelApothem, bp = ap + p.panelThickness, top = bottom + height;
            var tg = new Vec3(-s, 0, c); // counter-clockwise in atan2(z, x)
            var nIn = new Vec3(-c, 0, -s);
            Vec3 F(double along, double r, double y) => new Vec3(c * r + tg.x * along, y, s * r + tg.z * along);
            Vec3 f0 = F(-half, ap, bottom), f1 = F(half, ap, bottom), f2 = F(half, ap, top), f3 = F(-half, ap, top);
            Vec3 k0 = F(-half, bp, bottom), k1 = F(half, bp, bottom), k2 = F(half, bp, top), k3 = F(-half, bp, top);
            Quad(f0, f1, f2, f3, nIn, Wick37Part.Panel, index);          // the ride face (toward the axis)
            Quad(k0, k1, k2, k3, nIn * -1, Wick37Part.Panel, index);     // the back (toward the hot wall)
            Quad(f3, f2, k2, k3, new Vec3(0, 1, 0), Wick37Part.Panel, index);
            Quad(f0, f1, k1, k0, new Vec3(0, -1, 0), Wick37Part.Panel, index);
            Quad(f0, k0, k3, f3, tg * -1, Wick37Part.Panel, index);
            Quad(f1, k1, k2, f2, tg, Wick37Part.Panel, index);
            panels.Add(new Wick37Panel { index = index, section = section, angle = ang, bottom = bottom, top = top, start = f0, end = f1, normal = nIn });
        }

        /// <summary>The panel whose ride face is nearest arena-local feet `l` (its height band overlapping a 1.8 m capsule), and the horizontal
        /// distance to that face's segment; -1 when none overlaps the height.</summary>
        public int NearestPanel(Vec3 l, out double dist)
        {
            int best = -1;
            dist = 1e9;
            foreach (Wick37Panel pn in panels)
            {
                if (l.y + 1.8 < pn.bottom - 0.2 || l.y > pn.top + 0.2) continue;
                Vec3 a = pn.start, ab = pn.end - pn.start;
                double t = Math.Max(0, Math.Min(1, ((l.x - a.x) * ab.x + (l.z - a.z) * ab.z) / (ab.x * ab.x + ab.z * ab.z)));
                double d = JsMath.Hypot(l.x - (a.x + ab.x * t), l.z - (a.z + ab.z * t));
                if (d < dist) { dist = d; best = pn.index; }
            }
            return best;
        }

        /// <summary>Distance from the axis (arena-local x, z).</summary>
        public static double R(double x, double z) => JsMath.Hypot(x, z);

        /// <summary>The highest ledge (or the floor, or the cap) at or under feet height y: the checkpoint floor the player stands over.</summary>
        public double FloorUnder(double y)
        {
            double best = 0;
            foreach (double l in p.ledges) if (l <= y + 0.05) best = Math.Max(best, l);
            if (p.capY <= y + 0.05) best = p.capY;
            return best;
        }

        /// <summary>The stack in a world layer at `origin`.</summary>
        public void FillLayer(BoxLayer layer, Vec3 origin)
        {
            var list = new List<(Vec3, Vec3, Vec3)>(tris.Count);
            foreach (Wick37Tri t in tris) list.Add((t.a + origin, t.b + origin, t.c + origin));
            layer.SetTriangles(list);
        }

        /// <summary>NIGHT47 wick47 / wickTop47: the stack in a world layer with the ride panels out or folded into the wall (P1: none), and the
        /// glass over the shaft at the cap (P3) or not. Called at a phase change only (it allocates the layer's list).</summary>
        public void FillLayer(BoxLayer layer, Vec3 origin, bool panelsOut, bool glass)
        {
            var list = new List<(Vec3, Vec3, Vec3)>(tris.Count + p.facets);
            foreach (Wick37Tri t in tris)
            {
                if (!panelsOut && (t.part == Wick37Part.Panel || t.part == Wick37Part.Step)) continue; // P1: the panels and the steps folded into the wall
                list.Add((t.a + origin, t.b + origin, t.c + origin));
            }
            if (glass)
                foreach (Wick37Tri t in GlassTris()) list.Add((t.a + origin, t.b + origin, t.c + origin));
            layer.SetTriangles(list);
        }

        List<Wick37Tri> glassTris;
        /// <summary>NIGHT47 wickTop47: the glass: a disc at capY over the whole shaft (to the cap's inner edge, under the catwalk's lip), its top
        /// face only (the floor you stand on), `facets` wedges.</summary>
        public List<Wick37Tri> GlassTris()
        {
            if (glassTris != null) return glassTris;
            glassTris = new List<Wick37Tri>(p.facets);
            double fa = FacetAngle, rw = CornerRadius + 0.05, y = p.capY;
            for (int j = 0; j < p.facets; j++)
            {
                Vec3 a = new Vec3(0, y, 0), b = Polar(j * fa, rw, y), c = Polar((j + 1) * fa, rw, y);
                Vec3 u = b - a, v = c - a;
                var n = new Vec3(u.y * v.z - u.z * v.y, u.z * v.x - u.x * v.z, u.x * v.y - u.y * v.x);
                glassTris.Add(n.y < 0 ? new Wick37Tri { a = a, b = c, c = b, part = Wick37Part.Glass, index = -1 } : new Wick37Tri { a = a, b = b, c = c, part = Wick37Part.Glass, index = -1 });
            }
            return glassTris;
        }

        /// <summary>The stack in a TriangleWorld (the harness).</summary>
        public void FillWorld(TriangleWorld w, Vec3 origin)
        {
            foreach (Wick37Tri t in tris) w.AddTriangle(t.a + origin, t.b + origin, t.c + origin);
        }
    }

    /// <summary>design.n37.wick37.slag.</summary>
    public struct Wick37SlagParams
    {
        public double startY, riseRate, belowLedge, damage, bounceOver, bounceOut, cooldown, draftRise, maxBounce;

        public static Wick37SlagParams From(GameConfig c)
        {
            const string g = "design.n37.wick37.slag.";
            double N(string k) => c.Num(g + k);
            return new Wick37SlagParams
            {
                startY = N("startY"), riseRate = N("riseRate"), belowLedge = N("belowLedge"), damage = N("damage"), bounceOver = N("bounceOver"),
                bounceOut = N("bounceOut"), cooldown = N("cooldown"), draftRise = N("draftRise"), maxBounce = N("maxBounce"),
            };
        }
    }

    /// <summary>THE SLAG: a horizontal hazard plane in the stack. It rises riseRate m/s while the boss attacks and holds still while it rests,
    /// never above (the checkpoint ledge - belowLedge); touching it costs `damage` and bounces the player out (up to the checkpoint ledge's height
    /// + bounceOver, toward the wall): never instant death. Pure: the sim applies the bounce (Mover.Impulse) and the damage.</summary>
    public sealed class Wick37Slag
    {
        public readonly Wick37SlagParams p;
        /// <summary>Arena-local height of the surface.</summary>
        public double y;
        public double cap;
        public bool rising;
        int coolLeft;
        public int touches;

        public Wick37Slag(Wick37SlagParams prm)
        {
            p = prm;
            Reset();
        }

        public void Reset()
        {
            y = p.startY;
            cap = p.startY;
            rising = false;
            coolLeft = 0;
            touches = 0;
        }

        /// <summary>The cap for the checkpoint floor at `ledgeY` (the slag never comes nearer than belowLedge under it; never below its start).</summary>
        public void SetCheckpoint(double ledgeY) => cap = Math.Max(p.startY, ledgeY - p.belowLedge);

        /// <summary>One step: rise while `rising` (clamped at the cap).</summary>
        public void Step(double dt)
        {
            if (coolLeft > 0) coolLeft--;
            if (rising && y < cap) y = Math.Min(cap, y + p.riseRate * dt);
        }

        /// <summary>The draft: the slag jumps draftRise (clamped).</summary>
        public void Draft() => y = Math.Min(cap, y + p.draftRise);

        /// <summary>Does feet height `feetY` touch the surface now (and the cooldown is clear)? Starts the cooldown when it does.</summary>
        public bool Touch(double feetY, int cooldownSteps)
        {
            if (coolLeft > 0 || feetY > y) return false;
            coolLeft = cooldownSteps;
            touches++;
            return true;
        }

        /// <summary>The bounce's up speed to rise from the surface to `targetY` (+ bounceOver) under gravity g, capped at maxBounce.</summary>
        public double BounceUp(double targetY, double g) => Math.Min(p.maxBounce, Math.Sqrt(2 * g * Math.Max(0.5, targetY + p.bounceOver - y)));
    }
}
