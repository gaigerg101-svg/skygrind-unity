// NIGHT63 builder D (slate B3, BOSS COLOURS; view row design.n63.bossColours63): the boss parts' roles, pure (EditMode: N63BossColoursTests).
// GG 2026-10-22: "i would lead towards bosses, effect colores everything doenst feel alligned". The research (ULTRAKILL's parry flash is the same
// on every enemy; a boss gets no new colours): each named boss part (the row's numbers.parts: "wick.core" -> "weak", "wick.edges" -> "danger" ...)
// wears a ROLE of C1's colour table (Colours63Math): weak = the player's cyan (the thing the player acts on: CLAUDE.md rule 8, DESIGN-RULES V-2),
// danger = red-orange, at the site's own luminance. Nothing goes to yellow (DECIDE63 D-1: the parry tell only) or magenta (RIVAL only).
// BossColours63.N63D.cs applies it at the boss views' colour sites, read every frame.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class BossColours63Math
    {
        public const string Row = "bossColours63";
        public const string NoSwitch = "-skygrindNoBossColours63";
        public const int Night = 63;
        const string P = "design.n63.bossColours63.numbers.parts";

        /// <summary>The parts table (part -> weak / danger).</summary>
        public sealed class Parts
        {
            internal readonly Dictionary<string, string> roles = new Dictionary<string, string>(StringComparer.Ordinal);

            public int Count => roles.Count;

            public IEnumerable<string> Names => roles.Keys;

            /// <summary>A part's role ("weak" or "danger"); null when the table has no such part. Allocation-free.</summary>
            public string RoleOf(string part) => part != null && roles.TryGetValue(part, out string r) ? r : null;
        }

        /// <summary>The parts from the row's numbers (only "weak" and "danger" are taken; an _why key is skipped); null when missing.</summary>
        public static Parts From(GameConfig cfg)
        {
            if (cfg == null || !cfg.Has(P) || !(cfg.Get(P) is Dictionary<string, object> d)) return null;
            var p = new Parts();
            foreach (KeyValuePair<string, object> kv in d)
                if (!kv.Key.StartsWith("_", StringComparison.Ordinal) && !kv.Key.EndsWith("_why", StringComparison.Ordinal) && kv.Value is string role && (role == "weak" || role == "danger"))
                    p.roles[kv.Key] = role;
            return p;
        }

        /// <summary>A part's colour through its role: weak -> the table's player cyan, danger -> its danger red-orange, at the original's luminance.
        /// NIGHT63 B3c: a danger route is CLIP-SAFE (ClipSafe), so an HDR red-orange stays red-orange once the screen clips it.</summary>
        public static void Route(Colours63Math.Table t, string role, double r, double g, double b, out double R, out double G, out double B)
        {
            if (t == null || (role != "weak" && role != "danger")) { R = r; G = g; B = b; return; }
            Colours63Math.ToRole(t, role == "weak" ? "player" : "danger", r, g, b, out R, out G, out B);
            if (role == "danger") ClipSafe(t.Colour("danger"), ref R, ref G, ref B);
        }

        /// <summary>NIGHT63 B3c (B3's GATE: THE CROWN's red tell read 35-45 deg amber): the screen clips each channel at 1, so an HDR colour whose
        /// second channel passes its share shifts hue as it clips (danger's green 0.29 at x2.5 shows 0.72: amber). Above 1, the brightest channel
        /// keeps its value (the glow, the bloom) and every other channel stops at the role colour's own share of 1, so the clipped colour on screen
        /// is the role's hue. Within the screen's range (brightest channel <= 1) nothing changes.</summary>
        public static void ClipSafe(double[] role, ref double r, ref double g, ref double b)
        {
            double m = Math.Max(r, Math.Max(g, b));
            if (!(m > 1) || role == null) return;
            double rm = Math.Max(role[0], Math.Max(role[1], role[2]));
            if (!(rm > 0)) return;
            if (r < m) r = Math.Min(r, role[0] / rm);
            if (g < m) g = Math.Min(g, role[1] / rm);
            if (b < m) b = Math.Min(b, role[2] / rm);
        }

        /// <summary>NIGHT63 B3c: a part's colour at a site that multiplies it by `gain` afterwards (THE CROWN's tell, edges, clapper): the gain first,
        /// then the route, so a danger route is clip-safe on the colour the screen gets.</summary>
        public static void Hot(Colours63Math.Table t, string role, double r, double g, double b, double gain, out double R, out double G, out double B) =>
            Route(t, role, r * gain, g * gain, b * gain, out R, out G, out B);

        /// <summary>NIGHT63 B3c: one part's colours on one frame, for the proof line: from (the site's own colour) and to (what the frame used),
        /// the last call that frame and how many calls it had.</summary>
        public struct FramePart
        {
            public string part, role;
            public double fr, fg, fb, tr, tg, tb;
            public int calls;
        }

        /// <summary>NIGHT63 B3c (B3's GATE: the proof line printed only a part's first routed call, from another draw path than the one on the
        /// judged frame): one line per shot naming, per part, the colour that frame used.</summary>
        public static string FrameLine(string file, int frame, IList<FramePart> parts)
        {
            var sb = new System.Text.StringBuilder("[bossColours63] ");
            sb.Append(file).Append(" (frame ").Append(frame.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("): ");
            if (parts == null || parts.Count == 0) return sb.Append("no boss part routed on this frame").ToString();
            for (int i = 0; i < parts.Count; i++)
            {
                FramePart p = parts[i];
                if (i > 0) sb.Append(" | ");
                sb.Append(p.part).Append(' ').Append(p.role).Append(' ').Append(HexOf(p.fr, p.fg, p.fb)).Append(" -> ").Append(HexOf(p.tr, p.tg, p.tb))
                  .Append(" (").Append(p.calls.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(p.calls == 1 ? " call)" : " calls; the last drawn)");
            }
            return sb.ToString();
        }

        /// <summary>A colour as #RRGGBB of its hue at full value, with " xK" for an HDR colour (K its brightest channel).</summary>
        public static string HexOf(double r, double g, double b)
        {
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InvariantCulture;
            double m = Math.Max(1, Math.Max(r, Math.Max(g, b)));
            int R = Math.Max(0, Math.Min(255, (int)Math.Round(r / m * 255))), G = Math.Max(0, Math.Min(255, (int)Math.Round(g / m * 255))), B = Math.Max(0, Math.Min(255, (int)Math.Round(b / m * 255)));
            return "#" + R.ToString("X2", ci) + G.ToString("X2", ci) + B.ToString("X2", ci) + (m > 1 ? " x" + m.ToString("0.0", ci) : "");
        }

        /// <summary>A switch turns the row off: -skygrindOffNN with NN &lt;= 63, or -skygrindNoBossColours63 (any case). C1's own switch does not.</summary>
        public static bool Off(string[] args)
        {
            if (NightRows.OffFrom(args, "-skygrindOff") <= Night) return true;
            if (args != null)
                foreach (string a in args)
                    if (string.Equals(a, NoSwitch, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>The row's own rule (its config and the command line, read once a config); the live value each frame is NightRows.Live(decided, Row).</summary>
        public static bool Decide(bool rowOn, string[] args) => rowOn && !Off(args);
    }
}
