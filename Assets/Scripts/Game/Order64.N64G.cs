// NIGHT64 builder G (slate U1 "One order at a time", design.n64.order64, a VIEW row: no tape bit). gg-proxy (Logs/n63/gg-proxy.md #3): 4 of 18
// boss shots print the same order twice at once ("STAGGERED: HOLD LEFT CLICK, SLUG IT" small under the boss bar and large in the centre); GG
// 2026-10-14: "way too much instruction". One string feeds both copies (UiBossBar43.cs:209-215): the bar's hint b43Hint (18 px, under the bar's
// right end) and hud47's fight line fight47 (40 px, centred; UiHud47.Boss.cs). hud47 hides the hint once, when the bar is built, but Text33.Set
// shows its holder again on every new text (UiStyle33.cs:77-78), so both stand through the fight line's 3.2-3.8 s (code-scout.md section 2).
// With the row on, on every frame the fight line DRAWS the bar's own order (its alpha above 0, the same text) the bar's hint is not drawn; on
// every other frame it is drawn exactly as the game without the row draws it (its text; bossRule61's hold: BossRule61.HintShown). So the order
// shows in the bar again once the fight line has faded, and an order the fight line never draws (it waited behind the entrance card, or a
// newer line replaced it) stands in the bar as before: every order shown, one copy at a time. The write is the frame's last on the hint
// (UiOrder64.N64G.cs, from Fight47Late after Fight47Tick: after Bar43Draw's Set and UiBossRule61's every-frame hold and release, the red
// team's finding, so no one-frame duplicate at the rule line's release). The HUD's look is untouched (GG 2026-10-20: "the UI looks good as it
// is"): no font, size, colour, place or fade changes; only on which frames the small copy is drawn.
// THE SWITCH is read every frame (NightRows.ViewOn: D1's same-run toggle flips it; the row is in SameRun63Math.FrameRows, the lead's A1). Off
// (the row off, -skygrindOff64 or older, -skygrindNoOrder64): nothing is written (NIGHT63's HUD).
// THE COUNTER (the slate's proof; L-089: it prints that its input fired), per boss (the bar's name), on the HUD's frames with the bar up, the
// row on OR off: would-duplicate = frames the fight line draws the bar's order while the game without the row draws the bar's hint (the row-off
// duplicate: the red, in the same run), duplicate shown = frames both are drawn now; the same-run toggle's held frames counted apart (each pair
// is two held frames: its on half and its off half); every order (each new hint text) and where it was shown. EditMode: N64OrderTests.
using System;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Order64
    {
        public const string Row = "order64";
        public const string NoSwitch = "-skygrindNoOrder64";
        public const int Night = 64;
        const string RowOff = "design.n64." + Row + " off"; // a constant: OffReason runs every HUD frame, so it never allocates

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

        /// <summary>Why the row is off for this config and command line (null: on): the row (NightRows.ViewOn, the reel's live override
        /// first), then the off switches (BossTell63.OffReason's order).</summary>
        public static string OffReason(GameConfig cfg, string[] argv)
        {
            if (cfg == null) return "no config";
            if (!NightRows.ViewOn(cfg, Row)) return RowOff;
            if (NightRows.OffFrom(argv, "-skygrindOff") <= Night) return "-skygrindOff64 (or older)";
            if (NightRows.HasSwitch(argv, NoSwitch)) return NoSwitch; // NIGHT63 R1 N5: the one rule (case ignored)
            return null;
        }

        /// <summary>Is the row on now? Read every frame: never cache it (D1's same-run toggle flips ViewOn).</summary>
        public static bool On(GameConfig cfg) => OffReason(cfg, Args()) == null;

        /// <summary>The same order: the fight line's text and the bar's hint, both non-empty, equal with case and edge spaces ignored (Case47 draws
        /// the line in sentence case; its source is the bar's own string). Allocation-free for texts without edge spaces.</summary>
        public static bool Same(string line, string hint)
        {
            if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(hint)) return false;
            return string.Equals(line.Trim(), hint.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>The bar's hint as the game without the row draws it: its text, unless bossRule61's line holds the place (NIGHT63's rule).</summary>
        public static bool OffShown(bool ruleHolds, string hint) => BossRule61.HintShown(ruleHolds, hint);

        /// <summary>Is the bar's hint drawn this frame: as without the row, except while the row is on and the fight line draws the same order.</summary>
        public static bool HintShown(bool rowOn, bool ruleHolds, bool lineDrawn, string line, string hint) =>
            OffShown(ruleHolds, hint) && !(rowOn && lineDrawn && Same(line, hint));

        /// <summary>The proof counter for one boss (the HUD's frames with its bar up).</summary>
        public sealed class Count
        {
            public string Boss { get; private set; } = "";
            /// <summary>The run's frames (held frames apart), those with the row on; would-duplicate and duplicate-shown frames; the held frames.</summary>
            public int Frames, OnFrames, WouldDup, DupShown, Held, HeldWould, HeldShown;
            /// <summary>The fight line's current (or last) run of drawing the bar's order: its frames and its text.</summary>
            public int RunFrames, RunWould, RunShown;
            public string RunText = "";
            /// <summary>Frames counted since the totals were last printed (the HUD prints them at an order's end, the bar's going down, a new
            /// boss, a new run and the quit).</summary>
            public bool Dirty;

            string order;
            bool oLine, oBar, oAfter, inRun;
            int closed, closedShown, closedLine, closedBar, closedAfter;

            public void Reset(string boss)
            {
                Boss = boss ?? "";
                Frames = OnFrames = WouldDup = DupShown = Held = HeldWould = HeldShown = 0;
                RunFrames = RunWould = RunShown = 0;
                RunText = "";
                Dirty = false;
                order = null;
                oLine = oBar = oAfter = inRun = false;
                closed = closedShown = closedLine = closedBar = closedAfter = 0;
            }

            /// <summary>One HUD frame with the bar up: `held` a same-run held frame (counted apart, nothing else moves), `lineDrawn` the fight line
            /// drawn with `line`, `hint` the bar's text, `offShown` the hint as the game without the row draws it, `hintDrawn` as drawn now.
            /// True on the frame the fight line stops drawing the bar's order (the caller prints OrderLine).</summary>
            public bool Frame(bool held, bool rowOn, bool lineDrawn, string line, string hint, bool offShown, bool hintDrawn)
            {
                bool same = lineDrawn && Same(line, hint);
                bool would = same && offShown, dup = same && hintDrawn;
                Dirty = true;
                if (held)
                {
                    Held++;
                    if (would) HeldWould++;
                    if (dup) HeldShown++;
                    return false;
                }
                if (string.IsNullOrEmpty(hint)) { if (order != null) Close(); }
                else if (order == null || !string.Equals(order, hint, StringComparison.Ordinal)) { if (order != null) Close(); order = hint; oLine = oBar = oAfter = false; }
                Frames++;
                if (rowOn) OnFrames++;
                if (would) WouldDup++;
                if (dup) DupShown++;
                if (order != null)
                {
                    if (same) oLine = true;
                    if (hintDrawn) { oBar = true; if (oLine && !same) oAfter = true; }
                }
                if (same)
                {
                    if (!inRun) { inRun = true; RunFrames = RunWould = RunShown = 0; RunText = hint; }
                    RunFrames++;
                    if (would) RunWould++;
                    if (dup) RunShown++;
                    return false;
                }
                if (!inRun) return false;
                inRun = false;
                return true;
            }

            void Close()
            {
                closed++;
                if (oLine || oBar) closedShown++;
                if (oLine) closedLine++;
                if (oBar) closedBar++;
                if (oAfter) closedAfter++;
                order = null;
                oLine = oBar = oAfter = false;
            }

            /// <summary>Every order (each new hint text) so far, the open one included; those shown in either place; by the fight line; in the
            /// bar; in the bar after the fight line drew it.</summary>
            public int Orders => closed + (order != null ? 1 : 0);
            public int OrdersShown => closedShown + (order != null && (oLine || oBar) ? 1 : 0);
            public int InLine => closedLine + (order != null && oLine ? 1 : 0);
            public int InBar => closedBar + (order != null && oBar ? 1 : 0);
            public int BarAfter => closedAfter + (order != null && oAfter ? 1 : 0);

            /// <summary>The totals line (`why`: an order's end, the bar went down, a new boss, a new run, the quit; `t`: FeelClock.Now, the reel's
            /// fixed clock in a reel).</summary>
            public string Line(string why, bool rowOnNow, double t)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                return "[order64] " + Boss + " | row order64 on " + OnFrames.ToString(ci) + " of " + Frames.ToString(ci) + " frames" + (rowOnNow ? "" : " (off now)")
                    + " | would-duplicate " + WouldDup.ToString(ci) + " frames (the fight line draws the bar's order while the game without the row draws the bar's hint)"
                    + " | duplicate shown " + DupShown.ToString(ci) + " frames (both drawn)"
                    + " | held " + Held.ToString(ci) + " frames: would-duplicate " + HeldWould.ToString(ci) + ", duplicate shown " + HeldShown.ToString(ci) + " (the same-run pairs' on and off halves)"
                    + " | orders " + Orders.ToString(ci) + ": shown " + OrdersShown.ToString(ci) + " (by the fight line " + InLine.ToString(ci) + ", in the bar " + InBar.ToString(ci)
                    + ", in the bar after the fight line " + BarAfter.ToString(ci) + ")"
                    + " | " + why + ", t " + t.ToString("0.00", ci) + " s (FeelClock)";
            }

            /// <summary>The line for the fight line's run that just ended (`hintBack`: the bar's hint drawn on that frame).</summary>
            public string OrderLine(bool hintBack, bool rowOn, double t)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                return "[order64] " + Boss + " order \"" + RunText + "\" drawn by the fight line " + RunFrames.ToString(ci) + " frames: would-duplicate " + RunWould.ToString(ci)
                    + ", duplicate shown " + RunShown.ToString(ci) + " | the bar's hint after it: " + (hintBack ? "drawn" : "not drawn") + " | row order64 " + (rowOn ? "on" : "off")
                    + " | t " + t.ToString("0.00", ci) + " s (FeelClock)";
            }
        }
    }
}
