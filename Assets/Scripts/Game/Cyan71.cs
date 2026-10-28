// NIGHT71 builder B (slate B4 "boss weak points in bone while closed, cyan only while open", design.n71.cyan71, a VIEW row: no tape bit). A
// play-build A / B for GG's open call (DECIDE64.md:19-20: "should a boss never wear the player's cyan outside its weak point?"; T-153; gg-proxy
// rc2, Logs/n64/gg-proxy.md:90-98: bosses in the player's cyan on 5 shots). NIGHT63's bossColours63 (ON) routes every WEAK part (its
// numbers.parts: THE WICK's core, THE CROWN's gems and halos, THE LOOM's knees / seams / nails, THE KILN WYRM's punchable head, open head, core
// and mark, THE ICEBREAKER's weak rims) to the player's cyan at the site's own luminance, open or closed. With this row on, a weak part is
// routed to BONE (numbers.boneHex, crown40's own pale gem bone, at the site's luminance, never past its brightest channel) while its boss is
// CLOSED, and to the cyan as before while it is OPEN: the boss bar's OPEN (SurvivalSim.N43.Bosses.cs: THE LOOM / THE CROWN in their window,
// THE ICEBREAKER in its window or stuck, THE KILN WYRM staggered or its core open) and THE WICK's window (its brain's Open). So the cyan on a
// boss means "now": GG compares the two looks in one build with -skygrindNoCyan71. Danger parts, the parry yellow and every unrouted colour
// (crown40's passable ring, the views' own tints) are untouched. With bossColours63 off nothing is routed, so nothing here changes.
// THE SWITCH is read every frame (once a frame, cached by Time.frameCount; NightRows.ViewOn: D1's same-run toggle flips it; FrameRows). Off:
// the row off, -skygrindOff71 (or older), -skygrindNoCyan71. Hook: BossColours63.N63D.cs (Routes: a weak role becomes "bone" while closed).
// THE COUNTER (L-089: the input fired): per boss, the frames a weak part was drawn bone (closed) and cyan (open), the row on or off (off: the
// frames it would have been bone); printed at a new run and the quit: "[cyan71] ...".
using System;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Cyan71
    {
        public const string Row = "cyan71";
        public const string NoSwitch = "-skygrindNoCyan71";
        public const int Night = 71;
        public const string Root = "design.n71.cyan71.numbers.";
        /// <summary>The pseudo role a closed weak part takes (BossColours63.Routes).</summary>
        public const string Role = "bone";
        const string RowOff = "design.n71." + Row + " off";

        static string[] args;

        static string[] Args()
        {
            if (args == null)
            {
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
            }
            return args;
        }

        /// <summary>Why the row is off (null: on): the row (NightRows.ViewOn, the reel's live override first), then the off switches.</summary>
        public static string OffReason(GameConfig cfg, string[] argv)
        {
            if (cfg == null) return "no config";
            if (!NightRows.ViewOn(cfg, Row)) return RowOff;
            if (NightRows.OffFrom(argv, "-skygrindOff") <= Night) return "-skygrindOff71 (or older)";
            if (NightRows.HasSwitch(argv, NoSwitch)) return NoSwitch;
            return null;
        }

        static int onFrame = -1;
        static bool onNow;
        static GameConfig onCfg;

        /// <summary>Is the row on this frame? (Read once a frame per config: the colour sites call it many times a frame.)</summary>
        public static bool On(GameConfig cfg)
        {
            int f = Time.frameCount;
            if (f != onFrame || !ReferenceEquals(cfg, onCfg)) { onFrame = f; onCfg = cfg; onNow = OffReason(cfg, Args()) == null; }
            return onNow;
        }

        // ---- the bone ----
        static GameConfig boneFor;
        static double[] bone = { 0.949, 0.918, 0.847 };

        static double[] Bone(GameConfig cfg)
        {
            if (ReferenceEquals(cfg, boneFor)) return bone;
            boneFor = cfg;
            bone = new[] { 0.949, 0.918, 0.847 };
            try
            {
                if (cfg != null && cfg.Has(Root + "boneHex") && ColorUtility.TryParseHtmlString(cfg.Str(Root + "boneHex"), out Color c))
                    bone = new double[] { c.r, c.g, c.b };
            }
            catch (Exception) { }
            return bone;
        }

        /// <summary>`c` in bone at its own luminance (never past its brightest channel: Colours63Math.ToRole's rule), alpha kept. Pure maths.</summary>
        public static void ToBone(double[] b, double r, double g, double bl, out double R, out double G, out double B)
        {
            double l0 = 0.2126 * r + 0.7152 * g + 0.0722 * bl, l1 = 0.2126 * b[0] + 0.7152 * b[1] + 0.0722 * b[2];
            double k = l1 > 0 ? l0 / l1 : 0;
            double m0 = Math.Max(r, Math.Max(g, bl)), mc = Math.Max(b[0], Math.Max(b[1], b[2]));
            if (mc * k > m0 && mc > 0) k = m0 / mc;
            R = b[0] * k; G = b[1] * k; B = b[2] * k;
        }

        public static Color ToBone(GameConfig cfg, Color c)
        {
            ToBone(Bone(cfg), c.r, c.g, c.b, out double r, out double g, out double b);
            return new Color((float)r, (float)g, (float)b, c.a);
        }

        // ---- open or closed ----
        /// <summary>The bosses by their parts' prefix (bossColours63.numbers.parts: "wick.core", "crown.gems" ...).</summary>
        public static readonly string[] Bosses = { "wick", "crown", "loom", "wyrm", "brawl" };
        static readonly int[] openFrame = { -1, -1, -1, -1, -1 };
        static readonly bool[] openNow = new bool[5];

        /// <summary>The boss index of a part ("wyrm.open" -> 3); -1 for none.</summary>
        public static int BossOf(string part)
        {
            if (string.IsNullOrEmpty(part)) return -1;
            for (int i = 0; i < Bosses.Length; i++)
                if (part.Length > Bosses[i].Length && part[Bosses[i].Length] == '.' && string.CompareOrdinal(part, 0, Bosses[i], 0, Bosses[i].Length) == 0) return i;
            return -1;
        }

        /// <summary>Is boss i open now (the bar's OPEN; THE WICK's window)? Once a frame per boss.</summary>
        public static bool Open(int i) // NIGHT74 B weak74 (Weak74.Open): public; the body is unchanged
        {
            int f = Time.frameCount;
            if (openFrame[i] == f) return openNow[i];
            openFrame[i] = f;
            SurvivalSim s = SurvivalGame.Instance != null ? SurvivalGame.Instance.sim : null;
            bool o = false;
            if (s != null)
                switch (i)
                {
                    case 0: o = s.wick37 != null && s.wick37.brain != null && s.wick37.brain.Open; break;
                    case 1: o = s.crown37 != null && s.crown37.brain != null && s.crown37.Open; break;
                    case 2: o = s.loom37 != null && s.loom37.brain != null && s.loom37.Open; break;
                    case 3: o = (s.wyrm31 != null && s.wyrm31.Staggered) || (s.wyrm24 != null && s.wyrm24.CoreOpen); break;
                    case 4: o = s.brawl40 != null && (s.brawl40.Open || s.brawl40.StuckNow); break;
                }
            openNow[i] = o;
            return o;
        }

        /// <summary>The weak part's role this frame: "weak" (its boss open, the row off, or a part of no known boss) or "bone" (closed, the row
        /// on). The counter counts the frame either way.</summary>
        public static string WeakRole(GameConfig cfg, string part)
        {
            int i = BossOf(part);
            if (i < 0) return "weak";
            bool open = Open(i), on = On(cfg);
            count.Frame(i, open, on);
            return on && !open ? Role : "weak";
        }

        // ---- the counter ----
        public sealed class Count
        {
            /// <summary>Per boss: frames a weak part was drawn bone (closed, the row on), cyan while open, cyan while closed (the row off: the
            /// frames the row would draw bone); held same-run frames apart.</summary>
            public readonly int[] Bone = new int[5], OpenCyan = new int[5], ClosedCyan = new int[5], Held = new int[5];
            public int Frames, OnFrames;
            readonly int[] seen = { -1, -1, -1, -1, -1 };
            int frameSeen = -1;
            public bool Dirty;

            public void Reset()
            {
                Array.Clear(Bone, 0, 5); Array.Clear(OpenCyan, 0, 5); Array.Clear(ClosedCyan, 0, 5); Array.Clear(Held, 0, 5);
                for (int i = 0; i < 5; i++) seen[i] = -1;
                Frames = OnFrames = 0;
                frameSeen = -1;
                Dirty = false;
            }

            /// <summary>Boss i's weak part routed this frame (counted once a frame per boss).</summary>
            public void Frame(int i, bool open, bool on)
            {
                int f = Time.frameCount;
                if (seen[i] == f) return;
                seen[i] = f;
                Dirty = true;
                if (frameSeen != f) { frameSeen = f; if (!FeelClock.Holding) { Frames++; if (on) OnFrames++; } }
                if (FeelClock.Holding) { Held[i]++; return; }
                if (open) OpenCyan[i]++;
                else if (on) Bone[i]++;
                else ClosedCyan[i]++;
            }

            public string Line(string why, bool onNow, double t)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                int bone = 0, open = 0, closed = 0;
                for (int i = 0; i < 5; i++) { bone += Bone[i]; open += OpenCyan[i]; closed += ClosedCyan[i]; }
                var sb = new System.Text.StringBuilder("[cyan71] weak points drawn bone ");
                sb.Append(bone.ToString(ci)).Append(" boss-frames (closed, the row on) | cyan while closed ").Append(closed.ToString(ci))
                  .Append(" (the row off: the frames it would draw bone) | cyan while open ").Append(open.ToString(ci)).Append(" | by boss:");
                for (int i = 0; i < 5; i++)
                {
                    if (Bone[i] + OpenCyan[i] + ClosedCyan[i] + Held[i] == 0) continue;
                    sb.Append(' ').Append(Bosses[i]).Append(" bone ").Append(Bone[i].ToString(ci)).Append(" / closed cyan ").Append(ClosedCyan[i].ToString(ci))
                      .Append(" / open cyan ").Append(OpenCyan[i].ToString(ci)).Append(" / held ").Append(Held[i].ToString(ci)).Append(';');
                }
                sb.Append(" | row cyan71 on ").Append(OnFrames.ToString(ci)).Append(" of ").Append(Frames.ToString(ci)).Append(" frames").Append(onNow ? "" : " (off now)")
                  .Append(" | ").Append(why).Append(", t ").Append(t.ToString("0.00", ci)).Append(" s (FeelClock)");
                return sb.ToString();
            }
        }

        static readonly Count count = new Count();
        static bool wired;

        /// <summary>Once a session (BossColours63's first routed call): a new run prints the totals and starts clean; the quit prints them.</summary>
        public static void Wire(SurvivalGame g)
        {
            if (wired || g == null) return;
            wired = true;
            g.OnNewRun += () => { if (count.Dirty) Say(g, "a new run"); count.Reset(); };
            Application.quitting += () => { if (count.Dirty) Say(g, "the quit"); };
        }

        static void Say(SurvivalGame g, string why)
        {
            Debug.Log(count.Line(why, g != null && OffReason(g.cfg, Args()) == null, FeelClock.Now));
            count.Dirty = false;
        }
    }
}
