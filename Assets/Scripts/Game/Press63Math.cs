// NIGHT63 builder G (leftover 6 = NIGHT61's S7: kiln's presses read as machines; design.n63.press63, a VIEW row). gg-proxy at NIGHT63's start,
// item 1: "kiln's presses still plain blocks"; the N57 critic: "the presses read as plain dark pillars ... the shape of GG's random pillars that
// serve no purpose". The pure half (EditMode: N63PressTests); Press63.cs draws it. Each drop press (a campaign box whose job starts
// UnityTuning.Press63Job: n57-kiln's four, THE PRESS HALL's charger stun posts) is dressed as a press round its UNCHANGED box.
// S7b (S7's GATE FAIL, Logs/n63/verdict-S7.md: the blind SPOT saw "tall, multi-part columns ... with hazard stripes"): the silhouette of a
// press at the player's eye instead of a frame round a column:
//   the bed       a heavy base Press63BedH tall, wider than the column, its top a lit line;
//   the throat    near-black panels round the column from the bed to the crown: the gap's dark back (the old pillar's bands hidden);
//   the uprights  four bright guide columns at the box's corners, bed to crown: the press's frame;
//   the head      a heavy ram head (with a bright die at its bottom) riding between the bed and the crown on its own clock (HeadY: a slow
//                 lift, a hold up top, a hard drop, a dwell on the bed; FeelClock time; each press Press63PhaseS out of phase): raised, it
//                 leaves a Press63-high gap over the bed at the eye;
//   the rods      a bright rod down each face from the head's top to the crown (the piston), in front of the throat;
//   the crown     the widest, heaviest part (4.4-5.8 m), with the lamp (a lens lit while the head waits up top) on its face toward the hall;
//   the stack     a dark housing from the crown to the roof: the bright machine ends at the crown.
// No stripes (a hazard sign on a thing that does not hurt the player). Every part stands out of the box less than body.capsuleRadius, so the
// eye never enters a part of the box it collides with. Web space (y up; the Unity half flips z). No collider, no sim box: the press's box,
// its collision and the nav are the level's.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Press63Math
    {
        public const string Row = "press63";
        public const string NoSwitch = "-skygrindNoPress63";
        public const int Night = 63;

        public enum Part : byte { Bed, Throat, Upright, Crown, Stack, Head, Die, Rod, Housing, Lens }

        /// <summary>One axis-aligned machine part (web space).</summary>
        public struct Piece
        {
            public Part part;
            public Vec3 min, max;
        }

        /// <summary>A part's colours (sRGB hex) by its faces' direction: along z (front), along x (side), the top, the bottom.</summary>
        public struct Tone
        {
            public uint front, side, top, bottom;
        }

        /// <summary>Why the row is off for this config and command line (null: on). Read every frame (D1's same-run toggle flips ViewOn).</summary>
        public static string OffReason(GameConfig cfg, string[] argv)
        {
            if (cfg == null) return "no config";
            if (!NightRows.ViewOn(cfg, Row)) return "design.n63." + Row + " off";
            if (NightRows.OffFrom(argv, "-skygrindOff") <= Night) return "-skygrindOff63 (or older)";
            if (NightRows.HasSwitch(argv, NoSwitch)) return NoSwitch; // NIGHT63 R1 N5: the one rule (case ignored, as before)
            return null;
        }

        /// <summary>A campaign box's job names a drop press.</summary>
        public static bool IsPress(string job) => !string.IsNullOrEmpty(job) && job.StartsWith(UnityTuning.Press63Job, StringComparison.Ordinal);

        /// <summary>The bed's top (the stamp's floor).</summary>
        public static double BedTop(Vec3 min) => min.y + UnityTuning.Press63BedH;

        /// <summary>The crown's bottom: Press63CrownAtM up, or lower on a box too short for the crown under its top.</summary>
        public static double CrownBottom(Vec3 min, Vec3 max) => Math.Min(min.y + UnityTuning.Press63CrownAtM, max.y - UnityTuning.Press63CrownH);

        static Piece Grown(Part part, Vec3 min, Vec3 max, double by, double y0, double y1) =>
            new Piece { part = part, min = new Vec3(min.x - by, y0, min.z - by), max = new Vec3(max.x + by, y1, max.z + by) };

        /// <summary>The static parts of one press: the bed, the throat, 4 guide columns, the crown, the stack (when the box rises past the crown),
        /// the lamp's housing and lens (on the crown's -x face when `lampOnMinX`, else its +x face: toward the hall's middle).</summary>
        public static List<Piece> Pieces(Vec3 min, Vec3 max, bool lampOnMinX)
        {
            var ps = new List<Piece>(10);
            double bedTop = BedTop(min), crownBottom = CrownBottom(min, max), crownTop = crownBottom + UnityTuning.Press63CrownH;
            ps.Add(Grown(Part.Bed, min, max, UnityTuning.Press63BedOutM, min.y, bedTop));
            ps.Add(Grown(Part.Throat, min, max, UnityTuning.Press63ThroatOutM, bedTop, crownBottom));
            double o = UnityTuning.Press63UprightOutM, i = UnityTuning.Press63UprightM - UnityTuning.Press63UprightOutM;
            foreach (bool hx in new[] { false, true })
                foreach (bool hz in new[] { false, true })
                {
                    double x0 = hx ? max.x - i : min.x - o, z0 = hz ? max.z - i : min.z - o;
                    ps.Add(new Piece { part = Part.Upright, min = new Vec3(x0, bedTop, z0), max = new Vec3(x0 + UnityTuning.Press63UprightM, crownBottom, z0 + UnityTuning.Press63UprightM) });
                }
            ps.Add(Grown(Part.Crown, min, max, UnityTuning.Press63CrownOutM, crownBottom, crownTop));
            if (max.y > crownTop) ps.Add(Grown(Part.Stack, min, max, UnityTuning.Press63StackOutM, crownTop, max.y));
            // the lamp: a housing on the crown's face toward the hall's middle, centred on it, its lens proud of it
            double cz = (min.z + max.z) / 2, cy = crownBottom + UnityTuning.Press63CrownH / 2, s = lampOnMinX ? -1 : 1;
            double face = lampOnMinX ? min.x - UnityTuning.Press63CrownOutM : max.x + UnityTuning.Press63CrownOutM;
            double hFace = face + s * UnityTuning.Press63LampD, lFace = hFace + s * UnityTuning.Press63LensD;
            ps.Add(new Piece
            {
                part = Part.Housing,
                min = new Vec3(Math.Min(face, hFace), cy - UnityTuning.Press63LampH / 2, cz - UnityTuning.Press63LampW / 2),
                max = new Vec3(Math.Max(face, hFace), cy + UnityTuning.Press63LampH / 2, cz + UnityTuning.Press63LampW / 2),
            });
            ps.Add(new Piece
            {
                part = Part.Lens,
                min = new Vec3(Math.Min(hFace, lFace), cy - UnityTuning.Press63LensH / 2, cz - UnityTuning.Press63LensW / 2),
                max = new Vec3(Math.Max(hFace, lFace), cy + UnityTuning.Press63LensH / 2, cz + UnityTuning.Press63LensW / 2),
            });
            return ps;
        }

        /// <summary>The ram head with its bottom at `bottomY`: the column's footprint grown by Press63HeadOutM, Press63HeadH tall.</summary>
        public static Piece Head(Vec3 min, Vec3 max, double bottomY) => Grown(Part.Head, min, max, UnityTuning.Press63HeadOutM, bottomY, bottomY + UnityTuning.Press63HeadH);

        /// <summary>The die: the head's bright working edge at its bottom, standing a little out of the head so it shows.</summary>
        public static Piece Die(Vec3 min, Vec3 max, double bottomY) => Grown(Part.Die, min, max, UnityTuning.Press63DieOutM, bottomY, bottomY + UnityTuning.Press63DieH);

        /// <summary>The rods from `y0` (the head's top) to `y1` (the crown's bottom): a strip Press63RodShare of each face's width, centred on it,
        /// standing Press63RodOutM out of the box.</summary>
        public static List<Piece> Rods(Vec3 min, Vec3 max, double y0, double y1)
        {
            var ps = new List<Piece>(4);
            if (y1 <= y0) return ps;
            double cx = (min.x + max.x) / 2, cz = (min.z + max.z) / 2, wx = (max.x - min.x) * UnityTuning.Press63RodShare / 2, wz = (max.z - min.z) * UnityTuning.Press63RodShare / 2, d = UnityTuning.Press63RodOutM;
            ps.Add(new Piece { part = Part.Rod, min = new Vec3(max.x, y0, cz - wz), max = new Vec3(max.x + d, y1, cz + wz) });
            ps.Add(new Piece { part = Part.Rod, min = new Vec3(min.x - d, y0, cz - wz), max = new Vec3(min.x, y1, cz + wz) });
            ps.Add(new Piece { part = Part.Rod, min = new Vec3(cx - wx, y0, max.z), max = new Vec3(cx + wx, y1, max.z + d) });
            ps.Add(new Piece { part = Part.Rod, min = new Vec3(cx - wx, y0, min.z - d), max = new Vec3(cx + wx, y1, min.z) });
            return ps;
        }

        /// <summary>A part's colours (UnityTuning.N63GS7.cs). The lens's own two colours (lit, dark) are Press63LensOnHex / Press63LensOffHex.</summary>
        public static Tone ToneOf(Part p)
        {
            switch (p)
            {
                case Part.Bed: return new Tone { front = UnityTuning.Press63BedFrontHex, side = UnityTuning.Press63BedSideHex, top = UnityTuning.Press63BedTopHex, bottom = UnityTuning.Press63BedSideHex };
                case Part.Throat: return Flat(UnityTuning.Press63ThroatHex);
                case Part.Upright: return new Tone { front = UnityTuning.Press63UprightFrontHex, side = UnityTuning.Press63UprightSideHex, top = UnityTuning.Press63UprightFrontHex, bottom = UnityTuning.Press63UprightSideHex };
                case Part.Crown: return new Tone { front = UnityTuning.Press63CrownFrontHex, side = UnityTuning.Press63CrownSideHex, top = UnityTuning.Press63CrownFrontHex, bottom = UnityTuning.Press63CrownBottomHex };
                case Part.Stack: return Flat(UnityTuning.Press63StackHex);
                case Part.Head: return new Tone { front = UnityTuning.Press63HeadFrontHex, side = UnityTuning.Press63HeadSideHex, top = UnityTuning.Press63HeadTopHex, bottom = UnityTuning.Press63HeadSideHex };
                case Part.Die: return new Tone { front = UnityTuning.Press63DieFrontHex, side = UnityTuning.Press63DieSideHex, top = UnityTuning.Press63DieSideHex, bottom = UnityTuning.Press63DieSideHex };
                case Part.Rod: return new Tone { front = UnityTuning.Press63RodFrontHex, side = UnityTuning.Press63RodSideHex, top = UnityTuning.Press63RodFrontHex, bottom = UnityTuning.Press63RodSideHex };
                case Part.Lens: return Flat(UnityTuning.Press63LensOffHex);
                default: return Flat(UnityTuning.Press63HousingHex);
            }
        }

        static Tone Flat(uint hex) => new Tone { front = hex, side = hex, top = hex, bottom = hex };

        /// <summary>The head's lowest bottom: on the bed (the stamp).</summary>
        public static double Low(Vec3 min) => BedTop(min) + UnityTuning.Press63HeadLowGapM;

        /// <summary>The head's highest bottom: its top Press63HeadHighGapM under the crown.</summary>
        public static double High(Vec3 min, Vec3 max) => CrownBottom(min, max) - UnityTuning.Press63HeadHighGapM - UnityTuning.Press63HeadH;

        static double Period => UnityTuning.Press63LiftS + UnityTuning.Press63HoldS + UnityTuning.Press63DropS + UnityTuning.Press63DwellS;

        static double Cycle(double t, double phase)
        {
            double u = (t + phase) % Period;
            return u < 0 ? u + Period : u;
        }

        /// <summary>The head's bottom at time `t` (s, FeelClock) for a press `phase` s along its cycle: the lift eased from Low to High, held,
        /// dropped with gravity's square, dwelling on the bed.</summary>
        public static double HeadY(double t, double phase, double low, double high)
        {
            double u = Cycle(t, phase), lift = UnityTuning.Press63LiftS, hold = UnityTuning.Press63HoldS, drop = UnityTuning.Press63DropS;
            if (u < lift) { double k = u / lift; return low + (high - low) * k * k * (3 - 2 * k); }
            if (u < lift + hold) return high;
            if (u < lift + hold + drop) { double k = (u - lift - hold) / drop; return high - (high - low) * k * k; }
            return low;
        }

        /// <summary>The lamp is lit while the head waits up top (about to drop).</summary>
        public static bool LampLit(double t, double phase)
        {
            double u = Cycle(t, phase);
            return u >= UnityTuning.Press63LiftS && u < UnityTuning.Press63LiftS + UnityTuning.Press63HoldS;
        }

        /// <summary>The `index`-th press's place in the cycle (s): the hall stamps in turn.</summary>
        public static double Phase(int index) => index * UnityTuning.Press63PhaseS;
    }
}
