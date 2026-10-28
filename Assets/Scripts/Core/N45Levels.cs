// NIGHT45 (the shared commit; Tools/night/n45-levels.cjs): the level-only rows foundry45, iceline45, kiln45 (design.n45.<row>: enabled,
// levelFrom, levelTo). No sim bit: a tape records the level id it loaded, and an n45 id maps to its base unconditionally (it is only ever
// loaded when its row swapped it in). ForceOff (-skygrindOff45 / --off45 and every older off switch) leaves NIGHT44's order.
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class N45Levels
    {
        /// <summary>The level rows in design.n45.</summary>
        public static readonly string[] Rows = { "foundry45", "iceline45", "kiln45" };

        /// <summary>Every NIGHT45 level row off, whatever the config says (the harnesses' and the Player checks' switch).</summary>
        public static bool ForceOff;

        /// <summary>After Curriculum25.N43Levels: each row on swaps its levelFrom for its levelTo.</summary>
        public static void Swap(GameConfig cfg, List<string> res)
        {
            if (ForceOff || res.Count == 0) return;
            foreach (string row in Rows)
            {
                if (!cfg.Design("n45." + row + ".enabled")) continue;
                string from = cfg.Has("design.n45." + row + ".levelFrom") ? cfg.Get("design.n45." + row + ".levelFrom") as string : null;
                string to = cfg.Has("design.n45." + row + ".levelTo") ? cfg.Get("design.n45." + row + ".levelTo") as string : null;
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) continue;
                int i = res.IndexOf(from);
                if (i >= 0) res[i] = to;
            }
        }

        /// <summary>The id older systems know an n45 level by: its row's levelFrom (unconditional); any other id is itself.</summary>
        public static string BaseId(GameConfig cfg, string levelId)
        {
            levelId = NightRows.BaseId(cfg, levelId); // NIGHT46 rows: a NIGHT46+ level is its row's levelFrom first (n46-foundry -> n45-foundry), allocation-free
            if (levelId == null || cfg == null || !levelId.StartsWith("n45-", System.StringComparison.Ordinal)) return levelId;
            foreach (string row in Rows)
            {
                string to = cfg.Has("design.n45." + row + ".levelTo") ? cfg.Get("design.n45." + row + ".levelTo") as string : null;
                if (to == levelId) return cfg.Has("design.n45." + row + ".levelFrom") ? cfg.Get("design.n45." + row + ".levelFrom") as string : levelId;
            }
            return levelId;
        }
    }
}
