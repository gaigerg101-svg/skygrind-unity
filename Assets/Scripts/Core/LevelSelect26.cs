// NIGHT26 (builder L, contract level-select): the CAMPAIGN's level select, as data, pure (no engine; the screen is Game/UiLevelSelect26.cs). ULTRAKILL's select
// (RESEARCH.md S3): a column of ten slots in play order (the curriculum's order, padded with LOCKED slots up to design.n26.levelSelect.slots), each with the level's
// number and name, its best aggregate rank, its best time, kills %, style letter, and the weapon it unlocks. Only the first design.n26.levelSelect.playable slots
// can be started (levels 4+ show LOCKED however many map files exist); with design.n26.levelSelect.sequential a slot also needs the one before it cleared.
// The cursor starts on the first playable level not yet cleared (else the last playable one): CONTINUE is that level.
// The unlock column: Design/levels/unlocks.json (builder W's table; Unlocks26.Read accepts {"unlocks": {"<level>": "<weapon>"}}, a flat {"<level>": "<weapon>"},
// or {"unlocks": [{"level": "...", "weapon": "..."}]}), else the level's last room `award`. Unity-only: the web has no campaign.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class LevelSlot26
    {
        public int number; // 1-based
        public string id = "", name = "", unlock = "";
        public bool exists, playable, cleared;
        public LevelBest26 best;
    }

    public sealed class LevelSelect26Params
    {
        public int slots = 10, playable = 3;
        public bool sequential;

        public static LevelSelect26Params From(GameConfig cfg)
        {
            var p = new LevelSelect26Params();
            if (cfg.Has("design.n26.levelSelect.slots")) p.slots = (int)cfg.Num("design.n26.levelSelect.slots");
            if (cfg.Has("design.n26.levelSelect.playable")) p.playable = (int)cfg.Num("design.n26.levelSelect.playable");
            p.sequential = cfg.Design("n26.levelSelect.sequential");
            return p;
        }
    }

    public static class LevelSelect26
    {
        /// <summary>The slots, in play order. hasMap: the level's map file exists in design.maps.list; labelOf: its card label ("LEVEL 3 THE KILN"); unlockOf: the weapon id it
        /// unlocks (null: none).</summary>
        public static List<LevelSlot26> Slots(List<string> order, Profile profile, LevelSelect26Params p, Func<string, bool> hasMap, Func<string, string> labelOf, Func<string, string> unlockOf)
        {
            var list = new List<LevelSlot26>();
            for (int i = 0; i < p.slots; i++)
            {
                var s = new LevelSlot26 { number = i + 1 };
                if (i < order.Count && hasMap(order[i]))
                {
                    s.exists = true;
                    s.id = order[i];
                    s.name = NameOf(labelOf(s.id) ?? s.id);
                    s.best = profile?.LevelBest(s.id);
                    s.cleared = s.best != null && s.best.Played;
                    s.unlock = unlockOf(s.id) ?? "";
                }
                bool prev = i == 0 || !p.sequential || list[i - 1].cleared;
                s.playable = s.exists && i < p.playable && prev;
                list.Add(s);
            }
            return list;
        }

        /// <summary>"LEVEL 3 THE KILN" -> "THE KILN" (the slot shows its number on its own).</summary>
        public static string NameOf(string label)
        {
            string up = label.ToUpperInvariant();
            if (!up.StartsWith("LEVEL ", StringComparison.Ordinal)) return up;
            int i = 6;
            while (i < up.Length && char.IsDigit(up[i])) i++;
            return i > 6 && i < up.Length && up[i] == ' ' ? up.Substring(i + 1) : up;
        }

        /// <summary>The slot the cursor starts on: the first playable level not cleared, else the last playable one, else 0.</summary>
        public static int Cursor(List<LevelSlot26> slots)
        {
            int last = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].playable) continue;
                last = i;
                if (!slots[i].cleared) return i;
            }
            return last;
        }

        /// <summary>The next playable slot from `from` in direction dir (+1 / -1), skipping LOCKED slots; the slot itself when none is.</summary>
        public static int Step(List<LevelSlot26> slots, int from, int dir)
        {
            for (int i = from + dir; i >= 0 && i < slots.Count; i += dir) if (slots[i].playable) return i;
            return from;
        }
    }

    /// <summary>Design/levels/unlocks.json: level id -> the weapon id it unlocks.</summary>
    public static class UnlocksFile26
    {
        public static Dictionary<string, string> Read(string json)
        {
            var map = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(json)) return map;
            try
            {
                object root = JsJson.Parse(json);
                if (root is Dictionary<string, object> w0 && w0.TryGetValue("rows", out object wr) && wr is List<object> wl) // builder W's file: {start, rows:[{n, id, earns}]}
                {
                    foreach (object r in wl)
                        if (r is Dictionary<string, object> d0 && d0.TryGetValue("id", out object i0) && d0.TryGetValue("earns", out object e0) && i0 is string si && e0 is string se) map[si] = se;
                    return map;
                }
                if (root is Dictionary<string, object> o)
                {
                    object u = o.TryGetValue("unlocks", out object x) ? x : root;
                    if (u is Dictionary<string, object> d)
                    {
                        foreach (var kv in d) if (kv.Value is string w && !kv.Key.StartsWith("_", StringComparison.Ordinal)) map[kv.Key] = w;
                    }
                    else if (u is List<object> l) AddRows(l, map);
                }
                else if (root is List<object> rl) AddRows(rl, map);
            }
            catch (Exception)
            {
                map.Clear(); // a bad file: the plan's own awards
            }
            return map;
        }

        static void AddRows(List<object> rows, Dictionary<string, string> map)
        {
            foreach (object r in rows)
            {
                if (!(r is Dictionary<string, object> d)) continue;
                string lv = d.TryGetValue("level", out object a) ? a as string : null;
                string w = d.TryGetValue("weapon", out object b) ? b as string : d.TryGetValue("unlock", out object c) ? c as string : null;
                if (!string.IsNullOrEmpty(lv) && !string.IsNullOrEmpty(w)) map[lv] = w;
            }
        }
    }
}
