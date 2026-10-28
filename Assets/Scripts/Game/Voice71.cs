// NIGHT71 builder B (slate B1 "one order at a time on THE CROWN", design.n71.voice71, a VIEW row: no tape bit). T-155 (gg-proxy rc2: two
// orders at once on THE CROWN's stagger frame bp/crown37-stagger; GG 2026-10-14 "way too much instruction"). While THE CROWN's window is open
// the HUD gives the window order ("STAGGERED: EVERY HIT COUNTS", design.n43.wick43.bosses.window) as hud47's fight line, then (order64) in the
// bar's hint once the line fades, and crown40's own call ("OPEN: SHOOT IT", Crown37View.Crown40.cs, cyan under the crosshair, on its own
// overlay canvas) says the same thing at once: no arbiter sees call40 (Research/2026-10-26-forge71/scout-feel.md T-155).
// With the row on, on every frame the fight line or the bar's hint DRAWS the window order, the crown's window call is not drawn; on every other
// frame it is drawn exactly as without the row (so while bossRule61's line holds the place, or before the fight line takes the order, the call
// stands as before). "BLOCKED" (the call outside the window) is never hidden: it is the answer to a hit, not the window's order. The HUD's look
// is untouched (GG 2026-10-20: "the UI looks good as it is"): no font, size, colour, place or fade changes; only on which frames the call draws.
// THE SWITCH is read every frame (NightRows.ViewOn: D1's same-run toggle flips it; the row is in SameRun63Math.FrameRows, the shared commit).
// Off (the row off, -skygrindOff71 or older, -skygrindNoVoice71): the call is drawn as the game without the row draws it.
// THE COUNTER (the slate's proof; L-089: it prints that its input fired), on THE CROWN's frames with its words up, the row on OR off:
// would both = frames the game without the row draws the window call while the HUD draws the window order (the red, in the same run);
// both drawn = frames both are drawn now (0 with the row on); the same-run toggle's held frames counted apart (each pair is two held frames:
// its on half and its off half). Hook: Crown37View.Voice71.cs (from Crown40Draw, after the call's text: THE CROWN draws in LateUpdate, after
// the HUD's frame in SurvivalGame.Update) and UiVoice71.cs (the HUD's half: what the fight line and the bar's hint draw this frame).
using System;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Voice71
    {
        public const string Row = "voice71";
        public const string NoSwitch = "-skygrindNoVoice71";
        public const int Night = 71;
        /// <summary>The window order's config path (the sim's hint for THE LOOM / THE CROWN while open: SurvivalSim.N43.Bosses.cs).</summary>
        public const string WindowPath = "design.n43.wick43.bosses.window";
        const string RowOff = "design.n71." + Row + " off"; // a constant: OffReason runs every frame, so it never allocates

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

        /// <summary>Why the row is off for this config and command line (null: on): the row (NightRows.ViewOn, the reel's live override first),
        /// then the off switches (Order64.OffReason's order).</summary>
        public static string OffReason(GameConfig cfg, string[] argv)
        {
            if (cfg == null) return "no config";
            if (!NightRows.ViewOn(cfg, Row) && !NightRows.ViewOn(cfg, Btext72.Row)) return RowOff; // NIGHT72 B btext72 builds on voice71: its row on counts as this one on (-skygrindNoVoice71 still turns this one off)
            if (NightRows.OffFrom(argv, "-skygrindOff") <= Night) return "-skygrindOff71 (or older)";
            if (NightRows.HasSwitch(argv, NoSwitch)) return NoSwitch;
            return null;
        }

        /// <summary>Is the row on now? Read every frame: never cache it (D1's same-run toggle flips ViewOn).</summary>
        public static bool On(GameConfig cfg) => OffReason(cfg, Args()) == null;

        /// <summary>Is the crown's call drawn this frame: as without the row, except while the row is on, the call is the window's ("OPEN: SHOOT IT",
        /// `windowCall`) and the HUD draws the window order (`hudOrder`: the fight line or the bar's hint).</summary>
        public static bool CallShown(bool rowOn, bool windowCall, bool hudOrder) => !(rowOn && windowCall && hudOrder);

        /// <summary>The proof counter for THE CROWN (its words' frames).</summary>
        public sealed class Count
        {
            /// <summary>The run's frames (held frames apart), those with the row on; would-both and both-drawn frames; the window call set (the
            /// game without the row draws it) and drawn alone (no HUD order beside it); the HUD's window order (by the fight line, by the bar);
            /// the held frames.</summary>
            public int Frames, OnFrames, Would, Both, Call, CallAlone, Hud, HudLine, HudBar, Held, HeldWould, HeldBoth;
            /// <summary>Windows seen (each run of the window call), and the current (or last) one's frames.</summary>
            public int Windows, RunFrames, RunWould, RunBoth, RunAlone;
            /// <summary>Frames counted since the totals were last printed.</summary>
            public bool Dirty;

            bool inRun;

            public void Reset()
            {
                Frames = OnFrames = Would = Both = Call = CallAlone = Hud = HudLine = HudBar = Held = HeldWould = HeldBoth = 0;
                Windows = RunFrames = RunWould = RunBoth = RunAlone = 0;
                Dirty = false;
                inRun = false;
            }

            /// <summary>One frame of THE CROWN's words: `held` a same-run held frame (counted apart, nothing else moves), `windowCall` the call
            /// set to the window's (the game without the row draws it), `callDrawn` the call drawn now, `line` / `bar` the HUD's window order
            /// drawn by the fight line / the bar's hint. True on the frame the window call ends (the caller prints WindowLine).</summary>
            public bool Frame(bool held, bool rowOn, bool windowCall, bool callDrawn, bool line, bool bar)
            {
                bool hud = line || bar;
                bool would = windowCall && hud, both = windowCall && callDrawn && hud;
                Dirty = true;
                if (held)
                {
                    Held++;
                    if (would) HeldWould++;
                    if (both) HeldBoth++;
                    return false;
                }
                Frames++;
                if (rowOn) OnFrames++;
                if (would) Would++;
                if (both) Both++;
                if (windowCall) Call++;
                if (windowCall && callDrawn && !hud) CallAlone++;
                if (hud) Hud++;
                if (line) HudLine++;
                if (bar) HudBar++;
                if (windowCall)
                {
                    if (!inRun) { inRun = true; Windows++; RunFrames = RunWould = RunBoth = RunAlone = 0; }
                    RunFrames++;
                    if (would) RunWould++;
                    if (both) RunBoth++;
                    if (callDrawn && !hud) RunAlone++;
                    return false;
                }
                if (!inRun) return false;
                inRun = false;
                return true;
            }

            /// <summary>The totals line (`why`: the crown went, a new run, the quit; `t`: FeelClock.Now, the reel's fixed clock in a reel).</summary>
            public string Line(string why, bool rowOnNow, double t)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                return "[voice71] crown both drawn " + Both.ToString(ci) + " frames (the window call and the HUD's window order at once)"
                    + " | would both " + Would.ToString(ci) + " frames (the game without the row draws the window call while the fight line or the bar draws the window order)"
                    + " | the window call set " + Call.ToString(ci) + " frames, drawn alone " + CallAlone.ToString(ci)
                    + " | the HUD's window order " + Hud.ToString(ci) + " frames (the fight line " + HudLine.ToString(ci) + ", the bar " + HudBar.ToString(ci) + ")"
                    + " | held " + Held.ToString(ci) + " frames: would both " + HeldWould.ToString(ci) + ", both drawn " + HeldBoth.ToString(ci) + " (the same-run pairs' on and off halves)"
                    + " | windows " + Windows.ToString(ci) + " | row voice71 on " + OnFrames.ToString(ci) + " of " + Frames.ToString(ci) + " frames" + (rowOnNow ? "" : " (off now)")
                    + " | " + why + ", t " + t.ToString("0.00", ci) + " s (FeelClock)";
            }

            /// <summary>The line for the window that just ended.</summary>
            public string WindowLine(bool rowOn, double t)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                return "[voice71] crown window " + Windows.ToString(ci) + ": both drawn " + RunBoth.ToString(ci) + " of " + RunFrames.ToString(ci) + " frames, would both "
                    + RunWould.ToString(ci) + ", the call drawn alone " + RunAlone.ToString(ci) + " | row voice71 " + (rowOn ? "on" : "off") + " | t " + t.ToString("0.00", ci) + " s (FeelClock)";
            }
        }
    }
}
