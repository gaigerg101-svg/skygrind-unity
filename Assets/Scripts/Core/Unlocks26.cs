// NIGHT26 (builder W; design.n26.weaponIdentity, Design/levels/unlocks.json): which weapon each campaign level earns, and the weapons the campaign owns
// after a set of cleared levels. Pure (no engine). GG: "you slowly unlock weapons as you go on". The file is data: start weapons, then one row per level
// (id, earns). The CAMPAIGN's loadout is `OwnedAfter(clearedLevelIds)` (Loadout.FilterOwned26 narrows a loadout to it; the survival modes never call it).
// The hook for the end of a level is Campaign24's weaponEarned event (a room's `award`): the level-end line is in the lead's report.
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Unlocks26
    {
        public readonly List<string> start = new List<string>();
        public readonly List<(int n, string id, string earns)> rows = new List<(int, string, string)>();

        public static Unlocks26 Parse(string json)
        {
            var root = PortData.Obj(JsJson.Parse(json));
            var u = new Unlocks26();
            foreach (object o in PortData.Arr(root["start"])) u.start.Add((string)o);
            foreach (object o in PortData.Arr(root["rows"]))
            {
                var d = PortData.Obj(o);
                u.rows.Add(((int)PortData.Num(d["n"]), (string)d["id"], (string)d["earns"]));
            }
            return u;
        }

        /// <summary>The weapon a level earns when it is cleared (null: none or an unknown id).</summary>
        public string Earned(string levelId)
        {
            foreach (var r in rows) if (r.id == levelId) return r.earns;
            return null;
        }

        /// <summary>The weapons owned with these levels cleared: the start weapons, then each cleared level's weapon in the file's order, once each.</summary>
        public List<string> OwnedAfter(IEnumerable<string> cleared)
        {
            var done = new HashSet<string>(cleared ?? new string[0]);
            var owned = new List<string>(start);
            foreach (var r in rows)
                if (done.Contains(r.id) && !owned.Contains(r.earns)) owned.Add(r.earns);
            return owned;
        }
    }

    /// <summary>NIGHT31 W (design.n31.unlocks; DESIGN31 A, D-1 as the lead changed it; GG: "start out the first level with only the pistol you
    /// unlock the weapon new weapon at each level"): the campaign's loadout by LEVEL, not by the profile's cleared levels. A level n starts with
    /// `start.weapons` plus the `earns` of every row with a smaller n (level 1 the KESTREL; 2 + nothing yet; 3 + the gauntlet; 4 + the
    /// THUNDERCLAP); a row with a `room` awards its weapon when that room is entered (level 2's gauntlet in i1a, Campaign24.Award31); a row
    /// without one earns as NIGHT30 did (the plan's room award / pickup, else the level's last room). Pure; the table is config, not code.</summary>
    public sealed class Unlocks31
    {
        public readonly List<string> start = new List<string>();
        public readonly List<(int n, string id, string earns, string room)> rows = new List<(int, string, string, string)>();

        public static Unlocks31 From(GameConfig cfg)
        {
            var u = new Unlocks31();
            const string P = "design.n31.unlocks";
            if (cfg.Has(P + ".start.weapons")) u.start.AddRange(cfg.Strs(P + ".start.weapons"));
            if (cfg.Has(P + ".levels.rows"))
                foreach (object o in cfg.Arr(P + ".levels.rows"))
                {
                    var d = PortData.Obj(o);
                    u.rows.Add(((int)PortData.Num(d["n"]), (string)d["id"],
                        d.TryGetValue("earns", out object e) ? e as string : null,
                        d.TryGetValue("room", out object r) ? r as string : null));
                }
            return u;
        }

        /// <summary>The level's number in the table (0: not in it).</summary>
        public int LevelN(string levelId)
        {
            foreach (var r in rows) if (r.id == levelId) return r.n;
            return 0;
        }

        /// <summary>The weapons a run of this level starts with (null: the level is not in the table, the loadout is left alone).</summary>
        public List<string> StartFor(string levelId)
        {
            int n = LevelN(levelId);
            if (n <= 0) return null;
            var owned = new List<string>(start);
            for (int k = 1; k < n; k++)
                foreach (var r in rows)
                    if (r.n == k && r.earns != null && !owned.Contains(r.earns)) { owned.Add(r.earns); break; }
            return owned;
        }

        /// <summary>The room award of this level (weapon and room id; nulls: none, the level earns as NIGHT30 did).</summary>
        public (string weapon, string room) RoomAward(string levelId)
        {
            foreach (var r in rows) if (r.id == levelId && r.earns != null && r.room != null) return (r.earns, r.room);
            return (null, null);
        }
    }

    public sealed partial class Loadout
    {
        /// <summary>The campaign's weapon filter: keep only the weapons in `allowed` (order kept; the wheel and the swap keys read `owned`), the slot clamped.
        /// Never called by a survival mode.</summary>
        public void FilterOwned26(ICollection<string> allowed)
        {
            for (int i = owned.Count - 1; i >= 0; i--)
                if (!allowed.Contains(owned[i])) owned.RemoveAt(i);
            if (owned.Count == 0) return;
            if (slot >= owned.Count) slot = owned.Count - 1;
        }
    }
}
