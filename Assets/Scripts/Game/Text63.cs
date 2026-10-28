// NIGHT63 builder G (text61's critic points, DECIDE63 D-7, Logs/n63/verdict-P3.md problems 3-5; design.n63.text63, a VIEW row, OFF). text61
// (ON since NIGHT63's P3) keeps one line of text at a time and no instruction twice a run outside level 1; b3's verbs37 runs showed two gaps:
//   (a) its 3 s cap: 4 lines shown "LATE" with the room's title still up (iceline, kiln, span, spire): two blocks at once. With text63 a new
//       line waits up to UnityTuning.Text63WaitS (10 s) for the title to clear;
//   (b) "once a run" counted lines, not what they teach: kiln showed 10 lines, 7 of them the boost, several the same aim-down air shot in other
//       words. With text63 a line's withholding key is the TECHNIQUE it teaches (design.n63.text63.numbers.verbs: the template, as brief55 words
//       it, -> its technique): boost (the aim-down air shot), boostHigher (jump and fire together), boostAgain (fire again at the top, and the
//       summary that says it), flue (fire, jump at the top, fire again). The step tutorial keeps one line per step (PRINCIPLES 1; GG's G26:
//       "smaller, easier jumps first"); a restatement of a technique already taught this run is withheld. A line the table does not name keeps
//       text61's rule (its own key).
//   (c) T63b (text63's GATE FAIL, Logs/n63/verdict-T63.md): text61's gate dropped a line waiting for the title when the slot handed a newer one
//       or went empty (b6: kiln's jump step and spire's TORCH lesson never showed). With text63 the gate queues (Text61Gate.PassQueued): no
//       waiting line is dropped, the lines show one at a time in the order the run reaches them (a line released after the slot moved on stands
//       UnityTuning.Text63HoldS), the first wording of a technique the run reaches teaches it, and each line's fate is logged (shown, withheld,
//       or still waiting at run end).
// The pure half (EditMode: N63TextTests): the switch, the table, its resolution to the drawn text (the lesson widget's resolver, passed in),
// the key and the cap. UiText61.cs's Text61Filter passes them to Text61Gate's keyed Pass when the row is on; off (the row off, -skygrindOff63 or
// older, -skygrindNoText63): text61 exactly as judged (key = the line, 3 s). The sim never reads any of it.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Text63
    {
        public const string Row = "text63";
        public const string NoSwitch = "-skygrindNoText63";
        public const int Night = 63;
        const string VerbsKey = "design.n63.text63.numbers.verbs";

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

        /// <summary>Why the row is off for this config and command line (null: on). Read every frame (D1's same-run toggle flips ViewOn).</summary>
        public static string OffReason(GameConfig cfg, string[] argv)
        {
            if (cfg == null) return "no config";
            if (!NightRows.ViewOn(cfg, Row)) return "design.n63." + Row + " off";
            if (NightRows.OffFrom(argv, "-skygrindOff") <= Night) return "-skygrindOff63 (or older)";
            if (NightRows.HasSwitch(argv, NoSwitch)) return NoSwitch;
            return null;
        }

        /// <summary>Is the row on now (this process's command line)?</summary>
        public static bool On(GameConfig cfg) => OffReason(cfg, Args()) == null;

        static GameConfig verbsFor;
        static Dictionary<string, string> verbs = new Dictionary<string, string>();

        /// <summary>The technique table: template (as brief55 words it, {action} tokens) -> technique. Read once per config; keys ending _why skipped.</summary>
        public static Dictionary<string, string> Verbs(GameConfig cfg)
        {
            if (cfg == null) return new Dictionary<string, string>();
            if (ReferenceEquals(cfg, verbsFor)) return verbs;
            verbsFor = cfg;
            var d = new Dictionary<string, string>(StringComparer.Ordinal);
            if (cfg.Has(VerbsKey) && cfg.Get(VerbsKey) is Dictionary<string, object> raw)
                foreach (KeyValuePair<string, object> kv in raw)
                    if (!kv.Key.EndsWith("_why", StringComparison.Ordinal) && kv.Value is string v && v.Length > 0) d[kv.Key] = v;
            verbs = d;
            return verbs;
        }

        /// <summary>The table as the card sees it: each template resolved (the lesson widget's resolver: the bound key or pad glyph) -> technique.</summary>
        public static Dictionary<string, string> Resolved(IDictionary<string, string> table, Func<string, string> resolve)
        {
            var d = new Dictionary<string, string>(StringComparer.Ordinal);
            if (table == null || resolve == null) return d;
            foreach (KeyValuePair<string, string> kv in table)
            {
                string shown = resolve(kv.Key);
                if (!string.IsNullOrEmpty(shown)) d[shown] = kv.Value;
            }
            return d;
        }

        /// <summary>The withholding key of a drawn line: its technique when the table names it, else the line itself (text61's rule).</summary>
        public static string KeyOf(string line, IDictionary<string, string> resolvedToVerb) =>
            line != null && resolvedToVerb != null && resolvedToVerb.TryGetValue(line, out string v) ? v : line;

        /// <summary>The longest a new line waits for the room's title: Text63WaitS with the row on, text61's Text61WaitS off.</summary>
        public static double WaitS(bool on) => on ? UnityTuning.Text63WaitS : UnityTuning.Text61WaitS;
    }
}
