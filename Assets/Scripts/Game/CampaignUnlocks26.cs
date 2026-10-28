// NIGHT26 (builder L): the campaign's weapon unlocks, Game side. Design/levels/unlocks.json (builder W's table, Core/Unlocks26.cs) says which weapon each level earns
// (GG: "you slowly unlock weapons as you go on"). Two jobs, one call each at a campaign run's start:
//   Load   the table from the campaign maps' folder (null: no file or a bad one: the plan's own room awards only)
//   Apply  sets Campaign24.unlocks (the last room then earns the level's weapon: the weaponEarned event) and makes the loadout the campaign's: the start weapons plus the
//          `earns` of every level the profile has cleared (LevelBest26.Played), added in the file's order and everything else removed (Loadout.FilterOwned26).
// Behind design.n26.weaponIdentity (the block W's unlock order belongs to); a survival run never calls it. Not run in the Editor when written.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class CampaignUnlocks26
    {
        public static bool On(GameConfig cfg) => !N26Blocks.ForceOff && N26Blocks.From(cfg).weaponIdentity;

        public static Unlocks26 Load(IReadOnlyList<SurvivalGame.MapEntry> maps)
        {
            try
            {
                foreach (SurvivalGame.MapEntry m in maps)
                {
                    if (!m.campaignOnly) continue;
                    string p = Path.Combine(Path.GetDirectoryName(m.file) ?? ".", "unlocks.json");
                    if (File.Exists(p)) return Unlocks26.Parse(File.ReadAllText(p));
                }
            }
            catch (Exception) { }
            return null;
        }

        /// <summary>The ids of the levels the profile has cleared.</summary>
        public static List<string> Cleared(Profile profile)
        {
            var ids = new List<string>();
            if (profile != null) foreach (var kv in profile.levelBest) if (kv.Value.Played) ids.Add(kv.Key);
            return ids;
        }

        /// <summary>At a campaign run's start: the table on the campaign, the loadout narrowed to what the cleared levels own. Returns the weapons owned (empty: nothing done).</summary>
        public static List<string> Apply(GameConfig cfg, Campaign24 campaign, Loadout loadout, IReadOnlyList<SurvivalGame.MapEntry> maps, Profile profile)
        {
            var none = new List<string>();
            if (!On(cfg) || campaign == null || loadout == null) return none;
            Unlocks26 u = Load(maps);
            if (u == null) return none;
            campaign.unlocks = u;
            if (campaign.unlocks31) return new List<string>(loadout.owned); // NIGHT31 W: design.n31.unlocks owns the loadout (SurvivalSim.Weapons31.cs)
            List<string> owned = u.OwnedAfter(Cleared(profile));
            foreach (string w in owned) if (!loadout.owned.Contains(w)) loadout.AddWeapon(w);
            loadout.FilterOwned26(owned);
            return owned;
        }
    }
}
