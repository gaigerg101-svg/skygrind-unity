// NIGHT57 (the shared commit; Tools/night/n57-shared.cjs): design.n57.kiln57 swaps n52-kiln for n57-kiln (data: NightRows.SwapLevels; older systems
// read it by NightRows.BaseId). A system that keyed on a raw level id (kiln47's beats: SurvivalSim.N47.Kiln.cs) asks N57LevelFrom then N52LevelFrom.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The level an n57 level row swaps out (its levelFrom) for an n57 id; any other id is returned as is (an id that does not start
        /// with "n57-" never reads the config).</summary>
        internal string N57LevelFrom(string levelId)
        {
            if (levelId == null || !levelId.StartsWith("n57-", StringComparison.Ordinal) || cfg == null || !cfg.Has("design.n57")) return levelId;
            foreach (KeyValuePair<string, object> kv in cfg.Obj("design.n57"))
                if (kv.Value is Dictionary<string, object> row && row.TryGetValue("levelTo", out object to) && to as string == levelId && row.TryGetValue("levelFrom", out object from) && from is string f)
                    return f;
            return levelId;
        }

        /// <summary>NIGHT57 (the lead; D-16): the first room of the level that awards a weapon on entry (design.n40 kiln40's awards: kb2b's
        /// KESTREL), or null. verbs37's staging (FeelReel.N35.Verbs.cs Open35) stages its ground verbs at or after it, where a player already
        /// holds the level's gun.</summary>
        internal string FirstAwardRoom57 => n40AwardRooms.Count > 0 ? n40AwardRooms[0] : null;
    }
}
