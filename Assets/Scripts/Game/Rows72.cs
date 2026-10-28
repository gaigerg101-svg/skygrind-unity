// NIGHT72 shared commit (FORGE72 D-3; Tools/night/n72-rows.cjs wrote design.n72, OFF, in both config copies). The command line turns a row
// ON or OFF for this run, read once in SurvivalGame.Awake right after GameConfig.Load and before MovementTuning71.Apply and Bright71.Create
// (GameConfig.Get caches by path, so nothing may read the config before it):
//   -skygrindRows72 <row>:<0|1>[+<row>:<0|1>...]     also -skygrindRows72=<...>, or the list as the next argument
//   -skygrindNo<Row>72                               that row of this night OFF (the Player's own switch, read as NightRows.HasSwitch reads them)
// A row is set in its design block (design.nNN.<row>.enabled, nNN from the digits in its name) and in NightRows.ReelOverride, so ViewOn and
// Live read it every frame. A block with "sim": true is refused: a sim row's bits are fixed at StartRun. An unknown row changes nothing and
// says so. Every line starts "[rows72]" and is printed to the Player log (L-089: the input's own line).
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Rows72
    {
        public const string Switch = "-skygrindRows72";

        /// <summary>This night's view rows (design.n72, FORGE72 D-3): the -skygrindNo&lt;Row&gt;72 switches read these names.</summary>
        public static readonly string[] Night72Rows = { "calm72", "lit72", "fix72", "palette72", "decor72", "body72", "spring72", "bossfx72", "btext72", "goal72", "cam72" };

        /// <summary>A switch the Player takes as a player's switch (N29Fixes.IsAutomationArg): -skygrindRows72 in any form, and -skygrindNo&lt;Row&gt;72 for a
        /// row of this night. Not automation: the boot stays the player's game (NIGHT71 B's rule, for the n72 rows).</summary>
        public static bool PlayerSwitch72(string a)
        {
            if (string.IsNullOrEmpty(a)) return false;
            if (string.Equals(a, Switch, StringComparison.OrdinalIgnoreCase) || a.StartsWith(Switch + "=", StringComparison.OrdinalIgnoreCase)) return true;
            foreach (string row in Night72Rows)
                if (string.Equals(a, "-skygrindNo" + row, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>Reads the switches in args into cfg and NightRows.ReelOverride; returns the lines it printed (the Player's log, and the test's oracle).</summary>
        public static List<string> Apply(GameConfig cfg, string[] args)
        {
            var lines = new List<string>();
            if (cfg == null || args == null) return lines;
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                if (a == null) continue;
                string list = null;
                if (string.Equals(a, Switch, StringComparison.OrdinalIgnoreCase)) list = i + 1 < args.Length ? args[++i] : "";
                else if (a.StartsWith(Switch + "=", StringComparison.OrdinalIgnoreCase)) list = a.Substring(Switch.Length + 1);
                if (list != null)
                {
                    foreach (string pair in list.Split('+')) SetPair(cfg, pair, lines);
                    continue;
                }
                foreach (string row in Night72Rows)
                    if (string.Equals(a, "-skygrindNo" + row, StringComparison.OrdinalIgnoreCase)) Set(cfg, row, false, lines);
            }
            return lines;
        }

        /// <summary>The design night key of a row: "n" + the first run of digits in its name (calm72 -> n72, foundry65 -> n65); null when it has none.</summary>
        public static string NightKeyOf(string row)
        {
            if (string.IsNullOrEmpty(row)) return null;
            int i = 0;
            while (i < row.Length && !char.IsDigit(row[i])) i++;
            int j = i;
            while (j < row.Length && char.IsDigit(row[j])) j++;
            return j > i ? "n" + row.Substring(i, j - i) : null;
        }

        static void SetPair(GameConfig cfg, string pair, List<string> lines)
        {
            int colon = pair.LastIndexOf(':');
            string value = colon < 0 ? null : pair.Substring(colon + 1);
            if (colon <= 0 || (value != "0" && value != "1"))
            {
                lines.Add("[rows72] bad pair '" + pair + "': want <row>:0 or <row>:1 (nothing changed)");
                return;
            }
            Set(cfg, pair.Substring(0, colon), value == "1", lines);
        }

        static void Set(GameConfig cfg, string row, bool on, List<string> lines)
        {
            string night = NightKeyOf(row);
            if (night == null || !cfg.Has("design." + night + "." + row))
            {
                lines.Add("[rows72] unknown row " + row + ": no design." + (night ?? "nNN") + "." + row + " block, nothing changed");
                return;
            }
            Dictionary<string, object> block = PortData.Obj(cfg.Get("design." + night + "." + row));
            if (block.TryGetValue("sim", out object sim) && sim is bool isSim && isSim)
            {
                lines.Add("[rows72] refused " + row + ": a sim row");
                return;
            }
            block["enabled"] = on;
            NightRows.ReelOverride[row] = on;
            lines.Add("[rows72] " + row + "=" + (on ? "1" : "0"));
        }
    }
}
