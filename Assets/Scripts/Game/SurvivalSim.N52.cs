// NIGHT52 (the shared commit; Tools/night/n52-shared.cjs): tonight has no sim row. The five level rows (design.n52.kiln52, foundry52, iceline52,
// spire52, span52) swap a level for its n52 copy (data: Curriculum25 / NightRows.SwapLevels); older systems read an n52 id by NightRows.BaseId.
// A system that keyed on a raw level id (kiln47's beats: SurvivalSim.N47.Kiln.cs) asks N52LevelFrom, so the n52 copy keeps it.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The level an n52 level row swaps out (its levelFrom) for an n52 id; any other id is returned as is (no lookup: an id that
        /// does not start with "n52-" never reads the config, so every older level costs one string compare).</summary>
        internal string N52LevelFrom(string levelId)
        {
            if (levelId == null || !levelId.StartsWith("n52-", StringComparison.Ordinal) || cfg == null || !cfg.Has("design.n52")) return levelId;
            foreach (KeyValuePair<string, object> kv in cfg.Obj("design.n52"))
                if (kv.Value is Dictionary<string, object> row && row.TryGetValue("levelTo", out object to) && to as string == levelId && row.TryGetValue("levelFrom", out object from) && from is string f)
                    return f;
            return levelId;
        }
    }
}
