// NIGHT61 builder G (C1, design.n61.text61; Text61.cs holds the switch and the pure gate): one line of text at a time and no repeated instruction
// outside level 1, in the existing HUD (no font, colour, size or place changes: only WHEN a line shows). Hooks (one line each):
//   - UiTeach43.cs Teach43Draw, its first line, before brief55's cap: `s = Text61Filter(s, frameDt);`: the line text36's slot hands the centre
//     card goes through Text61Gate (outside level 1 and outside a boss fight: it waits while hud33's message slot shows a room's title, at most
//     UnityTuning.Text61WaitS; a line already shown this run is withheld). Before brief55's cap, so the cap counts only the time a line stands.
//   - UiTeach43.cs BuildTeach43, its last line: Text61Wire(): a new run resets the gate; "roomEnter" ends the previous room's ROOM CLEAR card
//     (clear55's c55Age) at once, so it never shows under the next room's title.
// Never in a boss fight (the boss bar up): hud47's fight line takes the card's lines there, and the boss's rule line (bossRule61) is the one line a
// boss fight allows at its start. Proof (L-089): Player.log "[text61] shown: <line>" once per line per run (LATE when it waited the whole cap),
// "[text61] waits for the title: <line>", "[text61] withheld repeat: <line>", "[text61] room clear dropped: entering <room id>". Off (the row off,
// -skygrindOff61 or older, -skygrindNoText61): Text61Filter returns its input and the roomEnter handler does nothing (NIGHT60's text).
// NIGHT63 G text63 / T63b (design.n63.text63, read every frame): Text63Filter, the queued gate; its extra proof lines: "[text63] withheld: <line>
// (its technique '<k>' already taught this run | waits as \"<first wording>\" ...)", "[text63] still waiting at run end: <line>" (at runEnd, a
// new run or the game's quit). text63 off: text61's path exactly as judged.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT61 G (text61): what the gate did this session (the reels print it).</summary>
    public static class Text61Probe
    {
        public static int Shown, Late, Waited, Withheld, ClearsDropped;
        public static string LastShown = "", LastWithheld = "";
    }

    public sealed partial class Ui
    {
        readonly Text61Gate t61Gate = new Text61Gate();
        bool t61Wired, t61Read, t61On;
        GameConfig t61Cfg;

        /// <summary>The row for this config (read once per config: the command line is fixed at boot).</summary>
        bool Text61On()
        {
            if (!t61Read || !ReferenceEquals(t61Cfg, game.cfg))
            {
                t61Read = true;
                t61Cfg = game.cfg;
                t61On = Text61.On(game.cfg);
            }
            return t61On;
        }

        /// <summary>From BuildTeach43 (once a session; game.events lives as long as the game): a new run resets the gate; a room entered ends the
        /// previous room's clear card.</summary>
        void Text61Wire()
        {
            if (t61Wired || game.events == null) return;
            t61Wired = true;
            game.OnNewRun += () => { Text63Left("a new run began"); t61Gate.Reset(); };
            game.events.On("runEnd", _ => Text63Left("the run ended")); // NIGHT63 G T63b: each line's fate, the ones still waiting included
            Application.quitting += () => Text63Left("the game quit");
            game.events.On<RoomEvent>("roomEnter", e =>
            {
                if (!Text61On() || c55Root == null || c55Age < 0) return;
                c55Age = -1; // RenderClear55's !live branch hides the card on its next frame (the same path its own end takes)
                Text61Probe.ClearsDropped++;
                Debug.Log($"[text61] room clear dropped: entering {(e != null ? e.id : "?")}");
            });
        }

        /// <summary>UiTeach43.Teach43Draw's first line: the line the centre card may draw now (null: none), through the gate (s itself with the row off).</summary>
        string Text61Filter(string s, double frameDt)
        {
            if (!Text61On()) return s;
            bool bossBar = b43Root != null && b43Root.gameObject.activeSelf;
            bool first = Brief55.FirstLevel(cfg, game.MapId);
            bool titleUp = h33SlotPrio == MessageSlot33.Title && h33Slot != null && h33Slot.Alpha > 0f; // the slot's title, fading out included
            // NIGHT63 G text63 (Text63.cs; design.n63.text63, read every frame): the withholding keyed by the technique a line teaches, the title
            // waited for up to Text63WaitS, and (T63b) a queue: a waiting line is never dropped, each line's fate logged; off: text61 exactly (the
            // line as its key, Text61WaitS, Text61Gate.Pass)
            bool t63 = Text63.On(cfg);
            if (t63) return Text63Filter(s, first, bossBar, titleUp, frameDt);
            double waitMax = Text63.WaitS(false);
            string r = t61Gate.Pass(s, s, first, bossBar, titleUp, frameDt, waitMax);
            switch (t61Gate.Act)
            {
                case Text61Act.Shown:
                case Text61Act.Late:
                    Text61Probe.Shown = t61Gate.Shown;
                    Text61Probe.Late = t61Gate.Late;
                    Text61Probe.LastShown = t61Gate.ActLine;
                    Debug.Log($"[text61] shown: {t61Gate.ActLine}{(t61Gate.Act == Text61Act.Late ? " (LATE: the room's title stood " + waitMax + " s)" : "")} | map {game.MapId}");
                    break;
                case Text61Act.Wait:
                    Text61Probe.Waited = t61Gate.Waited;
                    Debug.Log($"[text61] waits for the title: {t61Gate.ActLine} | map {game.MapId}");
                    break;
                case Text61Act.Repeat:
                    Text61Probe.Withheld = t61Gate.Withheld;
                    Text61Probe.LastWithheld = t61Gate.ActLine;
                    Debug.Log($"[text61] withheld repeat: {t61Gate.ActLine} | map {game.MapId}");
                    break;
            }
            return r;
        }

        /// <summary>NIGHT63 G T63b: text63's path (Text61Gate.PassQueued): the line the card may draw now, every fate logged. "[text61] shown:"
        /// keeps its words (the reels' meters read them), with "| text63 key"; a withholding says why; a wait names the queue.</summary>
        string Text63Filter(string s, bool first, bool bossBar, bool titleUp, double frameDt)
        {
            string key = Text63.KeyOf(s, Text63Map());
            double waitMax = Text63.WaitS(true);
            string r = t61Gate.PassQueued(s, key, first, bossBar, titleUp, frameDt, waitMax, UnityTuning.Text63HoldS);
            if (t61Gate.Events.Count > 0)
            {
                Text61Probe.Shown = t61Gate.Shown;
                Text61Probe.Late = t61Gate.Late;
                Text61Probe.Waited = t61Gate.Waited;
                Text61Probe.Withheld = t61Gate.Withheld;
                foreach (Text63Event e in t61Gate.Events) Text63Log(e, waitMax);
            }
            return r;
        }

        void Text63Log(Text63Event e, double waitMax)
        {
            switch (e.fate)
            {
                case Text63Fate.Shown:
                case Text63Fate.Late:
                    Text61Probe.LastShown = e.line;
                    Debug.Log($"[text61] shown: {e.line}{(e.fate == Text63Fate.Late ? " (LATE: the room's title stood " + waitMax + " s)" : "")} | text63 key '{e.key}' | map {game.MapId}");
                    break;
                case Text63Fate.Waits:
                    Debug.Log($"[text61] waits for the title: {e.line} | text63 queue: never dropped | map {game.MapId}");
                    break;
                case Text63Fate.Taught:
                    Text61Probe.LastWithheld = e.line;
                    Debug.Log(e.key != e.line
                        ? $"[text63] withheld: {e.line} (its technique '{e.key}' already taught this run) | map {game.MapId}"
                        : $"[text61] withheld repeat: {e.line} | map {game.MapId}");
                    break;
                case Text63Fate.Queued:
                    Text61Probe.LastWithheld = e.line;
                    Debug.Log($"[text63] withheld: {e.line} (its technique '{e.key}' waits as \"{e.by}\", the first wording the run reached) | map {game.MapId}");
                    break;
                case Text63Fate.Left:
                    Debug.Log($"[text63] still waiting at run end: {e.line} (technique '{e.key}') | map {(game != null ? game.MapId : "?")}");
                    break;
            }
        }

        /// <summary>NIGHT63 G T63b: the lines still waiting when a run ends (or a new one begins, or the game quits) are logged, not lost silently.</summary>
        void Text63Left(string why)
        {
            List<string> left = t61Gate.DrainWaiting();
            if (left.Count == 0) return;
            foreach (Text63Event e in t61Gate.Events) Text63Log(e, Text63.WaitS(true));
            Debug.Log($"[text63] {left.Count} line(s) still waiting when {why}");
        }

        Dictionary<string, string> t63Map;
        GameConfig t63MapCfg;
        InputDevice t63MapDev;

        /// <summary>NIGHT63 G text63: the technique table as the card draws it (each template through the lesson widget's resolver), rebuilt when the
        /// config or the input device changes (a pad's glyphs differ from the keys).</summary>
        Dictionary<string, string> Text63Map()
        {
            InputDevice dev = PlayerDevices.Last;
            if (t63Map == null || !ReferenceEquals(t63MapCfg, game.cfg) || dev != t63MapDev)
            {
                t63MapCfg = game.cfg;
                t63MapDev = dev;
                t63Map = Text63.Resolved(Text63.Verbs(game.cfg), t => ResolveLesson25(t, null));
            }
            return t63Map;
        }
    }
}
