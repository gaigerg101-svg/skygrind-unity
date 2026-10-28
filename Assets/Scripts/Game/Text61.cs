// NIGHT61 builder G (C1 "One line of text at a time, and no instruction outside the first level after its first showing"; design.n61.text61,
// a VIEW row: no tape bit). gg-proxy 2026-10-22 (Logs/n61/gg-proxy-start.md item 3): on n57-kiln's entry frames 4 of 4 showed two or more centre
// text blocks at once; GG 2026-10-14: "way too much instruction at the tsrat of the level" (OWNER.md:110); GG 2026-10-11: "the first level
// will have text overlay explaining mechanics" (OWNER.md:101). With the row on:
//   - outside the campaign's first level and outside a boss fight, the centre card's instruction (UiTeach43's teach card) waits while hud33's
//     message slot shows a room's title (one line at a time; at most UnityTuning.Text61WaitS, then it shows anyway: LATE), and shows at most
//     once per run: a line already shown is withheld when it comes again (keyed by the line as the slot hands it, so a verb's one line);
//   - everywhere, the previous room's ROOM CLEAR card (clear55) goes when the next room is entered (UiText61.cs);
//   - level 1 keeps its overlay (everything passes); a boss fight (the boss bar up) is hud47's fight line: the boss's rule line (bossRule61,
//     UiBossRule61.cs) is the one line a boss fight allows at its start, and nothing here touches it.
// The HUD's look is unchanged (GG 2026-10-20: "the UI looks good as it is, so don't touch that"): only WHEN a line shows. This file holds the
// switch and the pure gate (EditMode: N61TextTests, N61TextRowTests); UiText61.cs wires them. Off (the row off, -skygrindOff61 or older,
// -skygrindNoText61): the card and the clear card behave as NIGHT60's. The sim never reads any of it.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Text61
    {
        public const string Row = "text61";
        public const string NoSwitch = "-skygrindNoText61";

        /// <summary>A switch turns the row off: -skygrindOffNN with NN &lt;= 61 (this night or an older one) or -skygrindNoText61.</summary>
        public static bool Off(string[] args) => NightRows.OffFrom(args, "-skygrindOff") <= 61 || NightRows.HasSwitch(args, NoSwitch); // NIGHT63 R1 N5: case ignored, the one rule

        /// <summary>The row's rule: on with the row on (NightRows.ViewOn) and no switch that turns it off.</summary>
        public static bool Decide(bool rowOn, string[] args) => rowOn && !Off(args);

        static int argsOff = -1; // the command line's verdict, read once (it is fixed at boot): -1 unread, 0 no switch, 1 a switch turns it off

        /// <summary>The row now: the config's design.n61.text61 and this process's switches (read once). The Ui caches it per config.</summary>
        public static bool On(GameConfig cfg)
        {
            if (argsOff < 0)
            {
                string[] args;
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
                argsOff = Off(args) ? 1 : 0;
            }
            return argsOff == 0 && Decide(NightRows.ViewOn(cfg, Row), null);
        }
    }

    /// <summary>What one Text61Gate.Pass did (the Ui prints a proof line for each but None).</summary>
    public enum Text61Act { None, Shown, Late, Wait, Repeat }

    /// <summary>NIGHT63 G T63b: a line's fate under text63's queue. Waits: it entered the queue (not final). Final, one per line a run: Shown,
    /// Late (shown after waiting the whole cap), Taught (withheld: its technique already shown this run), Queued (withheld: its technique waits
    /// in the queue as the first wording the run reached, `by`), Left (still waiting when the run ended).</summary>
    public enum Text63Fate { Waits, Shown, Late, Taught, Queued, Left }

    /// <summary>NIGHT63 G T63b: one fate event (the Ui logs each).</summary>
    public struct Text63Event
    {
        public Text63Fate fate;
        public string line, key, by;
    }

    /// <summary>NIGHT61 G (text61): the centre card's gate. Pure (no Unity types); one per Ui, Reset at each new run.</summary>
    public sealed class Text61Gate
    {
        readonly HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
        string now, pending, lastWithheld;
        double waited;
        bool waiting;

        /// <summary>Counts this session: lines shown (Late among them: shown after the wait cap), waits begun, repeats withheld (one a appearance).</summary>
        public int Shown, Late, Waited, Withheld;
        /// <summary>What the last Pass did, and its line (null with None).</summary>
        public Text61Act Act;
        public string ActLine;

        /// <summary>text61's gate: the withholding keyed by the line itself (once per line a run).</summary>
        public string Pass(string line, bool firstLevel, bool bossBar, bool titleUp, double dt, double waitMax) =>
            Pass(line, line, firstLevel, bossBar, titleUp, dt, waitMax);

        // ---- NIGHT63 G T63b (text63's queue; text61's Pass above is untouched). The GATE's red (Logs/n63/verdict-T63.md): Pass drops a
        // waiting line when the slot hands a newer one or goes empty, so on b6 two lessons never showed and the boost key went to a restatement.
        sealed class Waiting
        {
            public string line, key;
            public double waited;
            public bool said;
        }

        readonly List<Waiting> queue = new List<Waiting>();
        Waiting showing;
        double showAge;
        bool showLate;

        /// <summary>This frame's fate events (PassQueued, DrainWaiting): the Ui logs each.</summary>
        public readonly List<Text63Event> Events = new List<Text63Event>();

        void Fate(Text63Fate f, string line, string key, string by) => Events.Add(new Text63Event { fate = f, line = line, key = key, by = by });

        Waiting Queued(Func<Waiting, bool> match)
        {
            foreach (Waiting w in queue) if (match(w)) return w;
            return null;
        }

        /// <summary>text63's gate: as the keyed Pass (one line at a time, wait for the room's title up to `waitMax`, withhold a technique already
        /// taught) but a line waiting is NEVER dropped: every line the slot hands that is new this run joins a queue in the order the run reaches
        /// it, and the queue shows one line at a time once the title has gone (or its head has waited `waitMax`: LATE). A line on the card stands
        /// while the slot hands it; one released after the slot moved on (from the queue) stands at least `holdS`. A line whose technique is
        /// already taught, or waits in the queue under its first wording, is withheld. The fates are in Events.</summary>
        public string PassQueued(string line, string key, bool firstLevel, bool bossBar, bool titleUp, double dt, double waitMax, double holdS)
        {
            Events.Clear();
            Act = Text61Act.None;
            ActLine = null;
            if (firstLevel || bossBar) return line;
            bool has = !string.IsNullOrEmpty(line);
            if (has && (showing == null || line != showing.line) && Queued(w => w.line == line) == null)
            {
                string k = string.IsNullOrEmpty(key) ? line : key;
                Waiting same = Queued(w => w.key == k);
                if (seen.Contains(k) || same != null)
                {
                    if (line != lastWithheld)
                    {
                        lastWithheld = line;
                        Withheld++;
                        Act = Text61Act.Repeat;
                        ActLine = line;
                        if (same != null) Fate(Text63Fate.Queued, line, k, same.line);
                        else Fate(Text63Fate.Taught, line, k, null);
                    }
                }
                else queue.Add(new Waiting { line = line, key = k });
            }
            // the line on the card: it ends when the slot no longer hands it (a released one after its hold)
            if (showing != null)
            {
                showAge += dt;
                bool held = has && line == showing.line;
                if (!held && (!showLate || showAge >= holdS)) showing = null;
            }
            if (showing == null && queue.Count > 0)
            {
                Waiting head = queue[0];
                if (titleUp && head.waited < waitMax) head.waited += dt;
                else
                {
                    queue.RemoveAt(0);
                    bool late = head.waited >= waitMax;
                    showing = head;
                    showAge = 0;
                    showLate = !(has && line == head.line);
                    seen.Add(head.key);
                    Shown++;
                    if (late) Late++;
                    Act = late ? Text61Act.Late : Text61Act.Shown;
                    ActLine = head.line;
                    Fate(late ? Text63Fate.Late : Text63Fate.Shown, head.line, head.key, null);
                    lastWithheld = null;
                }
            }
            foreach (Waiting w in queue)
                if (!w.said)
                {
                    w.said = true;
                    Waited++;
                    if (Act == Text61Act.None) { Act = Text61Act.Wait; ActLine = w.line; }
                    Fate(Text63Fate.Waits, w.line, w.key, null); // a line shown the frame it came never waits (it left the queue above)
                }
            return showing != null ? showing.line : null;
        }

        /// <summary>The run ended (or a new one begins): the lines still waiting, in order, each with a Left fate in Events; the queue empties.</summary>
        public List<string> DrainWaiting()
        {
            Events.Clear();
            var left = new List<string>(queue.Count);
            foreach (Waiting w in queue)
            {
                left.Add(w.line);
                Fate(Text63Fate.Left, w.line, w.key, null);
            }
            queue.Clear();
            return left;
        }

        /// <summary>A new run: every line may show once again.</summary>
        public void Reset()
        {
            seen.Clear();
            now = pending = lastWithheld = null;
            waited = 0;
            waiting = false;
            queue.Clear(); // NIGHT63 G T63b: the Ui drains (and logs) the lines still waiting first
            showing = null;
            showAge = 0;
            showLate = false;
        }

        /// <summary>The line the card may show this frame: `line` (the slot's line now) or null (withheld or waiting). firstLevel: the campaign's
        /// first level (its overlay stays: everything passes); bossBar: a boss fight's bar is up (hud47's fight line rules: everything passes,
        /// nothing is recorded); titleUp: the message slot shows a title-channel line now; dt: the frame's seconds; waitMax: the longest a new
        /// line waits for the title. NIGHT63 G text63: `key` is what "already shown this run" is kept by: the line itself (text61), or the
        /// technique the line teaches (text63's table: a restatement of a technique already taught is withheld); null or empty: the line.</summary>
        public string Pass(string line, string key, bool firstLevel, bool bossBar, bool titleUp, double dt, double waitMax)
        {
            Act = Text61Act.None;
            ActLine = null;
            if (firstLevel || bossBar) return line;
            if (string.IsNullOrEmpty(line))
            {
                now = pending = lastWithheld = null;
                waited = 0;
                waiting = false;
                return line;
            }
            if (line == now) return line; // the line on the card stays (no flicker), whatever the slot shows later
            string k = string.IsNullOrEmpty(key) ? line : key;
            if (seen.Contains(k))
            {
                if (line != lastWithheld) { lastWithheld = line; Withheld++; Act = Text61Act.Repeat; ActLine = line; }
                return null;
            }
            if (line != pending) { pending = line; waited = 0; waiting = false; }
            if (titleUp && waited < waitMax)
            {
                if (!waiting) { waiting = true; Waited++; Act = Text61Act.Wait; ActLine = line; }
                waited += dt;
                return null;
            }
            bool late = waited >= waitMax;
            now = line;
            seen.Add(k);
            Shown++;
            if (late) Late++;
            Act = late ? Text61Act.Late : Text61Act.Shown;
            ActLine = line;
            pending = lastWithheld = null;
            waited = 0;
            waiting = false;
            return line;
        }
    }
}
