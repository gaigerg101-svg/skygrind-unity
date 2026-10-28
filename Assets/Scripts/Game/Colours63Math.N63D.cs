// NIGHT63 builder D (slate C1, THE COLOUR TABLE; view row design.n63.colours63): the table's pure parts (EditMode: N63ColoursTests). GG
// 2026-10-19: "The effects are inconsistent it all needs to be unified"; 2026-10-22: "effect colores everything doenst feel alligned". The research
// (Research/2026-10-22-forge63/research-effect-colours.md): one hue one meaning, the world muted under the effects, one table every effect reads.
// THE TABLE is the row's numbers (written by Tools/night/n63-d-rows.cjs): the role colours (player, danger and the boundary READ from config's own
// role colours colors.usable / colors.enemy / barrier.boundaryColor; the parry yellow (DECIDE63 D-1: the parry tell only) and RIVAL's magenta
// typed in the table, config having none), the role bands (hue in degrees on the sRGB colour), the world tier (an off-palette world colour keeps
// its hue at the table's saturation and its own luminance: under the effects, never a new meaning) and the routes (a thing that wears a role it
// does not mean, moved to the role it does: THE CROWN's goal column, red-gold in the danger band down spire's cyan exit, to the boundary's pale
// violet-white like the boss door; the lava's gold, amber off every band, to danger). Colours63.N63D.cs applies it, read every frame.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Colours63Math
    {
        public const string Row = "colours63";
        public const string NoSwitch = "-skygrindNoColours63";
        public const int Night = 63;
        const string P = "design.n63.colours63.numbers.";

        /// <summary>The table (one per config).</summary>
        public sealed class Table
        {
            public double[] player = new double[3], danger = new double[3], boundary = new double[3], parry = new double[3], rival = new double[3];
            public double dangerLo, dangerHi, parryLo, parryHi, playerLo, playerHi, rivalLo, rivalHi;
            public double boundaryHueLo, boundaryHueHi, boundarySatMax, boundaryValMin, neutralSat, neutralVal;
            public double worldSat, routeSat, routeVal;
            internal readonly Dictionary<string, string> routes = new Dictionary<string, string>(StringComparer.Ordinal);

            /// <summary>The role a route goes to (null: no such route).</summary>
            public string Route(string what) => what != null && routes.TryGetValue(what, out string r) ? r : null;

            internal double[] Colour(string role)
            {
                switch (role)
                {
                    case "player": return player;
                    case "danger": return danger;
                    case "boundary": return boundary;
                    case "parry": return parry;
                    case "rival": return rival;
                }
                return null;
            }
        }

        /// <summary>The table from the row's numbers; null when they are missing or malformed (the view then changes nothing).</summary>
        public static Table From(GameConfig cfg)
        {
            if (cfg == null || !cfg.Has(P + "roles.player")) return null;
            try
            {
                var t = new Table();
                Rgb(cfg, cfg.Str(P + "roles.player"), t.player);
                Rgb(cfg, cfg.Str(P + "roles.danger"), t.danger);
                Rgb(cfg, cfg.Str(P + "roles.boundary"), t.boundary);
                if (!Hex(cfg.Str(P + "roles.parryHex"), t.parry) || !Hex(cfg.Str(P + "roles.rivalHex"), t.rival)) return null;
                t.dangerLo = cfg.Num(P + "bands.dangerLo"); t.dangerHi = cfg.Num(P + "bands.dangerHi");
                t.parryLo = cfg.Num(P + "bands.parryLo"); t.parryHi = cfg.Num(P + "bands.parryHi");
                t.playerLo = cfg.Num(P + "bands.playerLo"); t.playerHi = cfg.Num(P + "bands.playerHi");
                t.rivalLo = cfg.Num(P + "bands.rivalLo"); t.rivalHi = cfg.Num(P + "bands.rivalHi");
                t.boundaryHueLo = cfg.Num(P + "bands.boundaryHueLo"); t.boundaryHueHi = cfg.Num(P + "bands.boundaryHueHi");
                t.boundarySatMax = cfg.Num(P + "bands.boundarySatMax"); t.boundaryValMin = cfg.Num(P + "bands.boundaryValMin");
                t.neutralSat = cfg.Num(P + "bands.neutralSat"); t.neutralVal = cfg.Num(P + "bands.neutralVal");
                t.worldSat = cfg.Num(P + "worldSat");
                t.routeSat = cfg.Num(P + "routeSat");
                t.routeVal = cfg.Num(P + "routeVal");
                if (cfg.Get(P + "routes") is Dictionary<string, object> rs)
                    foreach (KeyValuePair<string, object> kv in rs)
                        if (!kv.Key.StartsWith("_", StringComparison.Ordinal) && kv.Value is string role && t.Colour(role) != null) t.routes[kv.Key] = role;
                return t;
            }
            catch (Exception) { return null; }
        }

        static void Rgb(GameConfig cfg, string path, double[] into)
        {
            var (r, g, b) = cfg.Rgb(path);
            into[0] = r;
            into[1] = g;
            into[2] = b;
        }

        /// <summary>"#RRGGBB" into 0-1 channels.</summary>
        public static bool Hex(string s, double[] into)
        {
            if (s == null || s.Length != 7 || s[0] != '#') return false;
            uint x = 0;
            for (int i = 1; i < 7; i++)
            {
                int v = HexDigit(s[i]);
                if (v < 0) return false;
                x = x * 16 + (uint)v;
            }
            into[0] = ((x >> 16) & 255) / 255.0;
            into[1] = ((x >> 8) & 255) / 255.0;
            into[2] = (x & 255) / 255.0;
            return true;
        }

        static int HexDigit(char c) => c >= '0' && c <= '9' ? c - '0' : c >= 'a' && c <= 'f' ? c - 'a' + 10 : c >= 'A' && c <= 'F' ? c - 'A' + 10 : -1;

        /// <summary>Hue (deg), saturation and value of a colour (HDR allowed: hue and saturation are its own at any intensity).</summary>
        public static void Hsv(double r, double g, double b, out double h, out double s, out double v)
        {
            double mx = Math.Max(r, Math.Max(g, b)), mn = Math.Min(r, Math.Min(g, b)), d = mx - mn;
            h = 0;
            if (d > 1e-12)
            {
                if (mx == r) h = (60 * ((g - b) / d) + 360) % 360;
                else if (mx == g) h = 60 * ((b - r) / d) + 120;
                else h = 60 * ((r - g) / d) + 240;
            }
            s = mx > 0 ? d / mx : 0;
            v = mx;
        }

        static bool InArc(double h, double lo, double hi) => lo <= hi ? h >= lo && h <= hi : h >= lo || h <= hi;

        /// <summary>The colour's role: player, danger, parry, rival (its band), boundary (pale violet-white), neutral (no hue), or off.</summary>
        public static string RoleOf(Table t, double r, double g, double b)
        {
            Hsv(r, g, b, out double h, out double s, out double v);
            if (s < t.neutralSat || v < t.neutralVal || (s < t.routeSat && v < t.routeVal)) return "neutral"; // no hue that reads; a dark weak tint is a material's own tone
            if (InArc(h, t.boundaryHueLo, t.boundaryHueHi) && s < t.boundarySatMax && v >= t.boundaryValMin) return "boundary";
            if (InArc(h, t.dangerLo, t.dangerHi)) return "danger";
            if (InArc(h, t.parryLo, t.parryHi)) return "parry";
            if (InArc(h, t.playerLo, t.playerHi)) return "player";
            if (InArc(h, t.rivalLo, t.rivalHi)) return "rival";
            return "off";
        }

        /// <summary>Is a world colour routed to the world tier: off every role band, at or over the route floor (saturation, value)?</summary>
        public static bool Routes(Table t, double r, double g, double b)
        {
            Hsv(r, g, b, out _, out double s, out double v);
            return s >= t.routeSat && v >= t.routeVal && RoleOf(t, r, g, b) == "off";
        }

        /// <summary>NIGHT72 C1: does a hue (deg) lie in the danger band widened by `marginDeg` on both sides (345 - m .. 35 + m)? A world ACCENT there reads as danger
        /// on a dark wall whatever its saturation: the spire's rose (0xe67793, hue 345.0 exactly on the band's edge) was the "dark-red stripes" of gg-proxy #2.</summary>
        public static bool InDangerReach(Table t, double hue, double marginDeg) => InArc(((hue % 360) + 360) % 360, ((t.dangerLo - marginDeg) % 360 + 360) % 360, (t.dangerHi + marginDeg) % 360);

        /// <summary>NIGHT72 C1: the world tier with the hue moved to `hueDeg` (the saturation down to the table's, the luminance kept; a colour already under the
        /// tier's saturation keeps its saturation): a world accent that wears danger goes to a hue that wears no role (UnityTuning.Colours72ExitHueDeg).</summary>
        public static void WorldAt(Table t, double r, double g, double b, double hueDeg, out double R, out double G, out double B)
        {
            Hsv(r, g, b, out _, out double s, out double v);
            R = r; G = g; B = b;
            if (v <= 0) return;
            FromHsv(hueDeg, Math.Min(s, t.worldSat), 1, out double nr, out double ng, out double nb);
            double l0 = Luma(r, g, b), l1 = Luma(nr, ng, nb);
            double k = l1 > 0 ? l0 / l1 : 0;
            R = nr * k; G = ng * k; B = nb * k;
        }

        static double Luma(double r, double g, double b) => 0.2126 * r + 0.7152 * g + 0.0722 * b;

        static void FromHsv(double h, double s, double v, out double r, out double g, out double b)
        {
            double hh = ((h % 360) + 360) % 360 / 60, f = hh - Math.Floor(hh);
            double p = v * (1 - s), q = v * (1 - s * f), u = v * (1 - s * (1 - f));
            switch ((int)Math.Floor(hh) % 6)
            {
                case 0: r = v; g = u; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = u; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = u; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }
        }

        /// <summary>The world tier: the hue kept, the saturation down to the table's, the luminance (Rec. 709) kept; a colour already at or
        /// under the tier's saturation is returned unchanged.</summary>
        public static void World(Table t, double r, double g, double b, out double R, out double G, out double B)
        {
            Hsv(r, g, b, out double h, out double s, out double v);
            R = r; G = g; B = b;
            if (s <= t.worldSat || v <= 0) return;
            FromHsv(h, t.worldSat, 1, out double nr, out double ng, out double nb);
            double l0 = Luma(r, g, b), l1 = Luma(nr, ng, nb);
            double k = l1 > 0 ? l0 / l1 : 0;
            R = nr * k; G = ng * k; B = nb * k;
        }

        /// <summary>A route to a role: the role's own colour at the original's luminance (Rec. 709). An unknown role is refused.</summary>
        public static void ToRole(Table t, string role, double r, double g, double b, out double R, out double G, out double B)
        {
            double[] c = t != null ? t.Colour(role) : null;
            if (c == null) throw new ArgumentException("no role '" + role + "' in the colour table");
            double l0 = Luma(r, g, b), l1 = Luma(c[0], c[1], c[2]);
            double k = l1 > 0 ? l0 / l1 : 0;
            // NIGHT63 C1 fix round: never past the original's brightest channel (the lava's gold in danger's red-orange at the gold's luminance was
            // (1.8, 0.92, 0.34): the screen clipped it back to amber-yellow); then the brightest channel is kept instead of the luminance
            double m0 = Math.Max(r, Math.Max(g, b)), mc = Math.Max(c[0], Math.Max(c[1], c[2]));
            if (mc * k > m0 && mc > 0) k = m0 / mc;
            R = c[0] * k; G = c[1] * k; B = c[2] * k;
        }

        /// <summary>A switch turns the row off: -skygrindOffNN with NN &lt;= 63, or -skygrindNoColours63 (any case).</summary>
        public static bool Off(string[] args)
        {
            return NightRows.OffFrom(args, "-skygrindOff") <= Night || NightRows.HasSwitch(args, NoSwitch); // NIGHT63 R1 N5: the one rule
        }

        /// <summary>The row's own rule (read once a map: its config and the command line); the live value each frame is NightRows.Live(decided, Row).</summary>
        public static bool Decide(bool rowOn, string[] args) => rowOn && !Off(args);

        /// <summary>NIGHT63 review S2: which of a map's two scans is due now (0 none): the first once `frames` (counted after the frame the map was
        /// seen) reach Colours63FirstScanFrames, the second once `since` (decor54's own clock: FeelClock.UnscaledDelta summed the same way, 0 in a
        /// held frame) reaches Colours63SecondScanS, half a second after decor54's second pass (Decor54SecondPassS) on the same clock. Never the
        /// wall clock: on a slow Player it ran ahead of the reel's fixed clock, so the second scan could come before the pass it waits for.</summary>
        public static int ScanDue(int scans, int frames, double since) =>
            scans == 0 && frames >= UnityTuning.Colours63FirstScanFrames ? 1 : scans == 1 && since >= UnityTuning.Colours63SecondScanS ? 2 : 0;
    }
}
