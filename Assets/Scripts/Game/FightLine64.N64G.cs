// NIGHT64 builder G (NIGHT64.md section 3 leftover 2, design.n64.fightLine64, a VIEW row: no tape bit). gg-proxy on NIGHT63's shipped boss
// frames (Logs/n63/gg-proxy.md #3): the room's name stands as hud47's fight line in the middle of the fight ("THE STACK" on bf/wick-windup,
// "THE SPIRE" on bp/crown37-idle), where the boss's order or nothing belongs. How it gets there: at the boss room's entry the zone's banner
// (Ui.cs zoneEnter -> Banner -> the message slot, MessageSlot33.Title) shows in the slot; the boss bar is up from the same moment, so hud47's
// Fight47Late takes the slot's line into the fight line's queue (prio 1, UiHud47.Boss.cs) and hides the slot; it waits behind bossRule61's
// rule line and is drawn after it, mid-fight. The red (n63-b12's boss63 telemetry): 170 frames each on THE WICK, THE CROWN and THE LOOM.
// With the row on, on a frame the boss bar is up and the fight line draws a ROOM TITLE (a zone's or the level's room's name, as posted or
// without its number), the fight line is not drawn (alpha 0): the boss's order shows in the bar (order64) or nothing in the centre. The
// queue itself is untouched: the title keeps its turn and its time, so every later line comes in on the same frame as without the row, and
// D1's same-run held off frame draws exactly what the off game draws. Never lost: a title hidden in a fight is posted once to the message
// slot after the fight (its bar down), the zone banner's own time, and counted when the slot draws it. The HUD's look is untouched (GG
// 2026-10-20): no font, size, colour, place or fade; only on which frames the fight line draws a room title.
// THE SWITCH is read every frame (NightRows.ViewOn; the row is in SameRun63Math.FrameRows, the lead's A1). Off (the row off, -skygrindOff64 or
// older, -skygrindNoFightLine64): nothing is hidden or posted (NIGHT63's HUD).
// THE COUNTER (L-089), per boss (the bar's name), on HUD frames with the bar up, the row on OR off: would draw = frames the queue draws a room
// title (the off game), drawn = frames it is drawn now; held frames apart; each title's run and whether it was hidden. EditMode:
// N64FightLineTests. The HUD's half: UiFightLine64.N64G.cs.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class FightLine64
    {
        public const string Row = "fightLine64";
        public const string NoSwitch = "-skygrindNoFightLine64";
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
            if (NightRows.HasSwitch(argv, NoSwitch)) return NoSwitch;
            return null;
        }

        /// <summary>Is the row on now? Read every frame: never cache it (D1's same-run toggle flips ViewOn).</summary>
        public static bool On(GameConfig cfg) => OffReason(cfg, Args()) == null;

        /// <summary>Is the fight line hidden this frame: the row on, the boss bar up, the line drawn by the queue, and it is a room title.</summary>
        public static bool Hide(bool rowOn, bool barUp, bool lineDrawn, bool isTitle) => rowOn && barUp && lineDrawn && isTitle;

        /// <summary>The room titles: each zone's or room's name as posted ("3.2b THE STACK") and without its number (Copy47.StripNumber: "THE
        /// STACK", what copy47 posts), case and edge spaces ignored.</summary>
        public sealed class Titles
        {
            readonly HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public void Add(string name)
            {
                if (string.IsNullOrEmpty(name)) return;
                string n = name.Trim();
                if (n.Length == 0) return;
                set.Add(n);
                set.Add(Copy47.StripNumber(n).Trim());
            }

            public bool Has(string text) => !string.IsNullOrEmpty(text) && set.Contains(text.Trim());

            public int Count => set.Count;
        }

        /// <summary>The titles hidden in a fight: held, posted once to the message slot after it, counted when the slot draws them.</summary>
        public sealed class Later
        {
            readonly List<string> waiting = new List<string>(4), posted = new List<string>(4);
            public int Posted, ShownAfter;
            public int Waiting => waiting.Count;

            static int Find(List<string> l, string s)
            {
                for (int i = 0; i < l.Count; i++) if (string.Equals(l[i], s, StringComparison.OrdinalIgnoreCase)) return i;
                return -1;
            }

            /// <summary>A title was hidden this frame (held once, whatever the frames).</summary>
            public void Hidden(string title)
            {
                if (string.IsNullOrEmpty(title) || Find(waiting, title) >= 0) return;
                waiting.Add(title);
            }

            /// <summary>The fight is over: the titles to post now (each once), watched until the slot draws them.</summary>
            public List<string> TakeForPost()
            {
                var l = new List<string>(waiting);
                foreach (string s in waiting) if (Find(posted, s) < 0) posted.Add(s);
                Posted += waiting.Count;
                waiting.Clear();
                return l;
            }

            /// <summary>The slot draws `slotText`: true once for a posted title (ShownAfter counts it).</summary>
            public bool Seen(string slotText)
            {
                if (string.IsNullOrEmpty(slotText) || posted.Count == 0) return false;
                int i = Find(posted, slotText.Trim());
                if (i < 0) return false;
                posted.RemoveAt(i);
                ShownAfter++;
                return true;
            }

            public void Clear()
            {
                waiting.Clear();
                posted.Clear();
                Posted = ShownAfter = 0;
            }
        }

        /// <summary>The proof counter for one boss (the HUD's frames with its bar up).</summary>
        public sealed class Count
        {
            public string Boss { get; private set; } = "";
            /// <summary>The run's frames (held frames apart), those with the row on; room-title frames would draw (the queue: the off game) and
            /// drawn (now); the held frames; title runs and the runs hidden.</summary>
            public int Frames, OnFrames, Would, Drawn, Held, HeldWould, HeldDrawn, TitleRuns, HiddenRuns;
            /// <summary>The current (or last) title run: its frames, its text, whether a frame of it was hidden.</summary>
            public int RunFrames, RunWould, RunDrawn;
            public string RunText = "";
            public bool RunHidden, Dirty;
            bool inRun;

            public void Reset(string boss)
            {
                Boss = boss ?? "";
                Frames = OnFrames = Would = Drawn = Held = HeldWould = HeldDrawn = TitleRuns = HiddenRuns = 0;
                RunFrames = RunWould = RunDrawn = 0;
                RunText = "";
                RunHidden = Dirty = inRun = false;
            }

            /// <summary>One HUD frame with the bar up: `would` the queue draws a room title, `drawn` it is drawn now, `text` the line. True on
            /// the frame a title's run ends (the caller prints TitleLine).</summary>
            public bool Frame(bool held, bool rowOn, bool would, bool drawn, string text)
            {
                Dirty = true;
                if (held)
                {
                    Held++;
                    if (would) HeldWould++;
                    if (would && drawn) HeldDrawn++;
                    return false;
                }
                Frames++;
                if (rowOn) OnFrames++;
                if (would)
                {
                    Would++;
                    if (drawn) Drawn++;
                    if (!inRun) { inRun = true; TitleRuns++; RunFrames = RunWould = RunDrawn = 0; RunText = text ?? ""; RunHidden = false; }
                    RunFrames++;
                    RunWould++;
                    if (drawn) RunDrawn++;
                    else if (!RunHidden) { RunHidden = true; HiddenRuns++; }
                    return false;
                }
                return EndRun();
            }

            /// <summary>A title's run ends (the line moved on, or the bar went down inside it): true once.</summary>
            public bool EndRun()
            {
                if (!inRun) return false;
                inRun = false;
                return true;
            }

            /// <summary>The totals line (`why`: a title's end, the bar went down, a new boss, a new run, the quit; `t`: FeelClock.Now).</summary>
            public string Line(string why, bool rowOnNow, Later later, double t)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                return "[fight64] " + Boss + " | row fightLine64 on " + OnFrames.ToString(ci) + " of " + Frames.ToString(ci) + " frames" + (rowOnNow ? "" : " (off now)")
                    + " | room-title frames in the fight line: would draw " + Would.ToString(ci) + ", drawn " + Drawn.ToString(ci)
                    + " | held " + Held.ToString(ci) + " frames: would draw " + HeldWould.ToString(ci) + ", drawn " + HeldDrawn.ToString(ci) + " (the same-run pairs' on and off halves)"
                    + " | titles mid-fight " + TitleRuns.ToString(ci) + ": hidden " + HiddenRuns.ToString(ci)
                    + ", held for after the fight " + (later != null ? later.Waiting : 0).ToString(ci) + ", posted after " + (later != null ? later.Posted : 0).ToString(ci)
                    + ", shown after " + (later != null ? later.ShownAfter : 0).ToString(ci)
                    + " | " + why + ", t " + t.ToString("0.00", ci) + " s (FeelClock)";
            }

            /// <summary>The line for the title run that just ended.</summary>
            public string TitleLine(bool rowOn, double t)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                return "[fight64] " + Boss + " title \"" + RunText + "\" in the fight line mid-fight " + RunFrames.ToString(ci) + " frames: would draw " + RunWould.ToString(ci)
                    + ", drawn " + RunDrawn.ToString(ci) + " | " + (RunHidden ? "held for after the fight" : "shown as before") + " | row fightLine64 " + (rowOn ? "on" : "off")
                    + " | t " + t.ToString("0.00", ci) + " s (FeelClock)";
            }
        }
    }
}
