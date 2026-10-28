// NIGHT64 builder D (slate C4, THE EXIT THAT READS DANGER; view row design.n64.goal64, OFF; the lead's A1 row). gg-proxy (Logs/n63/gg-proxy.md
// #4): "a red-orange vertical line drops from a ceiling lamp down the cyan door" on 5 of 5 spire frames, named since NIGHT60. It is THE CROWN's
// goal column (Route47View.cs BuildColumn, SKYGRIND/Column47 with its through-wall pass): red-gold #FF8A3A (UnityTuning.CLook47RedGoldHex), in
// the danger band, at x -10, the centre of every spire door. CLAUDE.md rule 8: red-orange is enemies and danger only; the column means "the
// boss is there", so the colour table already routes it (design.n63.colours63.numbers.routes.goal47: the boundary's pale violet-white, the boss
// door's tone) but that table's row colours63 is off, and Colours63.Route returns its input while it is (Colours63.N63D.cs Route).
// With this row on, the column (both its passes: one _BaseColor) draws in the table's goal role, read with Colours63Math.From(cfg) (it reads the
// table whatever colours63's enabled) at the red-gold's own luminance (Colours63Math.ToRole), never typed. THE SPLIT (the red team: Route47View
// gave the column and the window shafts one colour): the window shafts over the doors keep theirs (colours63's route, the red-gold while
// colours63 is off: GG's call, DECIDE64). Read every frame (D1's contract: NightRows.Live, the same-run toggle's held off frame). Off: the row
// off, -skygrindOff64 (or an older night's), -skygrindNoGoal64 (any case). View only: no sim read, no tape bit. EditMode: N64GoalTests.
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Goal64Math
    {
        /// <summary>The view row's name (design.n64.goal64).</summary>
        public const string Row = "goal64";
        /// <summary>The one-row off switch: this row alone off (case ignored).</summary>
        public const string NoSwitch = "-skygrindNoGoal64";
        /// <summary>The row's night (its -skygrindOffNN switch covers it from this night on).</summary>
        public const int Night = 64;
        /// <summary>The colour table's route the column reads (design.n63.colours63.numbers.routes.goal47).</summary>
        public const string Route = "goal47";

        /// <summary>A switch turns the row off: -skygrindOffNN with NN &lt;= 64, or -skygrindNoGoal64 (any case).</summary>
        public static bool Off(string[] args) => NightRows.OffFrom(args, "-skygrindOff") <= Night || NightRows.HasSwitch(args, NoSwitch);

        /// <summary>This frame's value: the row's view value (its design, the process's off switch: ViewOn) and its own switches; D1's reel override
        /// wins when it names the row (NightRows.Live's contract). Allocation-free: call it every frame.</summary>
        public static bool Live(GameConfig cfg, string[] args) => NightRows.Live(NightRows.ViewOn(cfg, Row) && !Off(args), Row);

        /// <summary>The column's colour: (r, g, b) through the table's goal route (ToRole at its own luminance). False, with the input unchanged and
        /// role null, when the table is missing or has no goal route (the view then draws the red-gold, as before).</summary>
        public static bool GoalColour(Colours63Math.Table t, double r, double g, double b, out double R, out double G, out double B, out string role)
        {
            R = r; G = g; B = b;
            role = t != null ? t.Route(Route) : null;
            if (role == null) return false;
            Colours63Math.ToRole(t, role, r, g, b, out R, out G, out B);
            return true;
        }

        /// <summary>THE SPLIT: the column's colour this frame: the goal colour while the row is live and the colour is known, else the shafts' (the
        /// colour Route47View gave both before this row).</summary>
        public static Color Column(bool live, bool known, Color goal, Color shafts) => live && known ? goal : shafts;

        /// <summary>The proof line (L-089: grep "[goal64] column live"): printed when the live value changes (and once at the map's first frame),
        /// naming the role, the column's and the shafts' colours, whether the column is built, and the frame and time on the reel's clock.</summary>
        public static string LiveLine(bool live, string role, Color own, Color column, Color shafts, bool built, int frame, double now)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            return "[goal64] column live " + (live ? "on" : "off") + ": role " + (role ?? "none (no goal route in the colour table)")
                + " | column " + BossColours63Math.HexOf(column.r, column.g, column.b) + " (from " + BossColours63Math.HexOf(own.r, own.g, own.b) + ")"
                + " | shafts " + BossColours63Math.HexOf(shafts.r, shafts.g, shafts.b) + " | column built " + (built ? "yes" : "NO")
                + " | frame " + frame.ToString(ci) + ", FeelClock.Now " + now.ToString("0.000", ci);
        }
    }
}
