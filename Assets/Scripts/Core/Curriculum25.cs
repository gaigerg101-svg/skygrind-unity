// NIGHT25 (builder L): the CAMPAIGN's level order, pure (no engine). design.n25.curriculum.order lists the level ids in play order ("tutorial",
// "layer1" (level 2, THE KILN), "level3" ... "level10"); a level is offered only when its block is on: "tutorial" needs design.n25.tutorial,
// "levelN" (N >= 3) needs design.n25.curriculum, any other id (layer1) is always there (NIGHT24's game). With both blocks off the order is
// ["layer1"]: NIGHT24's CAMPAIGN. The MODE row's CAMPAIGN starts at Start(); a cleared level's RESULTS continues with Next().
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class Curriculum25
    {
        /// <summary>The levels offered with these blocks, in play order.</summary>
        public static List<string> Order(GameConfig cfg, N25Blocks n25)
        {
            var res = new List<string>();
            // NIGHT30 (builder L): while design.n30.levels is on, the order is design.n30.levels.order (the rebuilt n30-* levels); off, NIGHT29's order
            string key = N30Levels(cfg) ? "design.n30.levels.order" : "design.n25.curriculum.order";
            if (cfg.Has(key) && cfg.Get(key) is List<object> raw)
                foreach (object o in raw)
                {
                    string id = o as string;
                    if (string.IsNullOrEmpty(id)) continue;
                    if (id == "tutorial" && !n25.tutorial) continue;
                    if (id.StartsWith("level") && !n25.curriculum) continue;
                    if ((id.StartsWith("n29-") || id.StartsWith("n30-")) && !(n25.tutorial || n25.curriculum)) continue; // NIGHT29: the rebuilt levels 1-3 need the curriculum blocks like the levels they replace (NIGHT30: the n30 ids too)
                    res.Add(id);
                }
            N32Levels(cfg, res);
            N33Levels(cfg, res);
            N34Levels(cfg, res);
            N35Levels(cfg, res);
            N36Levels(cfg, res);
            N37Levels(cfg, res);
            N38Levels(cfg, res);
            N40Levels(cfg, res);
            N41Levels(cfg, res);
            N42Levels(cfg, res);
            N43Levels(cfg, res);
            N45Levels.Swap(cfg, res); // NIGHT45: the level-only rows (Core/N45Levels.cs)
            NightRows.SwapLevels(cfg, res); // NIGHT46 rows: the NIGHT46+ level rows (design.nNN.<row>.levelFrom / levelTo), nights in order
            if (res.Count == 0) res.Add("layer1");
            return res;
        }

        /// <summary>NIGHT32 (the lead): the NIGHT32 level rows. design.n32.level5 (on) inserts design.n32.level5.id right after (and only when the order holds)
        /// design.n32.level5.after; design.n32.arenas32 / span32 (on) swap each level id in their `levelSwap` map ({old id: new id}) for its
        /// rebuilt file. A flow choice like design.n30.levels (the level a run loads is already on its tape). Off with -skygrindOff32 / --off32
        /// (N32Blocks.ForceOff) or with each row off: NIGHT31's order.</summary>
        public static void N32Levels(GameConfig cfg, List<string> res)
        {
            if (N32Blocks.ForceOff || res.Count == 0) return;
            if (cfg.Design("n32.level5.enabled") && At(cfg, "design.n32.level5.id") is string id5 && id5.Length > 0 && !res.Contains(id5))
            {
                int at = res.IndexOf(At(cfg, "design.n32.level5.after") as string ?? "");
                if (at >= 0) res.Insert(at + 1, id5); // only right after level 4: an order without it (NIGHT29's, the tutorial alone) gets no level 5 (suite run 1: Lesson25Tests.TheOrder_FollowsTheBlocks)
            }
            foreach (string row in SwapRows)
            {
                if (!cfg.Design("n32." + row + ".enabled") || !(At(cfg, "design.n32." + row + ".levelSwap") is Dictionary<string, object> map)) continue;
                for (int i = 0; i < res.Count; i++)
                    if (map.TryGetValue(res[i], out object to) && to is string s && s.Length > 0) res[i] = s;
            }
        }

        /// <summary>NIGHT32: the rows that swap levels for rebuilt files (A: levels 1-3; S: level 4).</summary>
        static readonly string[] SwapRows = { "arenas32", "span32" };

        /// <summary>NIGHT33 (the lead): design.n33.kit33 (on) swaps each level id in its `levelSwap` map ({old id: new id}) for its dressed and
        /// overhauled n33 file (K dresses, R reshapes rooms in the same files). Runs after N32Levels, so it maps NIGHT32's ids. A flow choice
        /// (the level a run loads is on its tape). Off with -skygrindOff33 / --off33 (N33Blocks.ForceOff) or with the row off: NIGHT32's order.</summary>
        /// <summary>NIGHT34 (the lead): design.n34.lines34 (on) swaps each level id in its `levelSwap` map ({old id: new id}) for its
        /// rebuilt movement-line n34 file (builder A). Runs after N33Levels, so it maps NIGHT33's ids; a level not in the map keeps its n33
        /// file. A flow choice (the level a run loads is on its tape). Off with -skygrindOff34 / --off34 (N34Blocks.ForceOff) or with the row off.</summary>
        public static void N34Levels(GameConfig cfg, List<string> res)
        {
            if (N34Blocks.ForceOff || res.Count == 0) return;
            if (!cfg.Design("n34.lines34.enabled") || !(At(cfg, "design.n34.lines34.levelSwap") is Dictionary<string, object> map)) return;
            for (int i = 0; i < res.Count; i++)
                if (map.TryGetValue(res[i], out object to) && to is string s && s.Length > 0) res[i] = s;
        }

        /// <summary>NIGHT35 (the lead; builder A owns the row): design.n35.campaign6 (on) makes the campaign six levels. It swaps each id in
        /// its `levelSwap` map ({old id: new id}, after N34Levels: A's n35 files), inserts `finaleId` (LEVEL 6) right after `after` (the
        /// swapped order's level 5), then drops every id after the finale: the legacy levels leave the order (their files stay in the tree).
        /// Without `after` in the order nothing is inserted or dropped. A flow choice (the level a run loads is on its tape). Off with
        /// -skygrindOff35 / --off35 (N35Blocks.ForceOff) or with the row off: NIGHT34's order.</summary>
        public static void N35Levels(GameConfig cfg, List<string> res)
        {
            if (N35Blocks.ForceOff || res.Count == 0 || !cfg.Design("n35.campaign6.enabled")) return;
            if (At(cfg, "design.n35.campaign6.levelSwap") is Dictionary<string, object> map)
                for (int i = 0; i < res.Count; i++)
                    if (map.TryGetValue(res[i], out object to) && to is string s && s.Length > 0) res[i] = s;
            int at = res.IndexOf(At(cfg, "design.n35.campaign6.after") as string ?? "");
            if (at < 0) return;
            string fin = At(cfg, "design.n35.campaign6.finaleId") as string;
            int keep = at + 1;
            if (!string.IsNullOrEmpty(fin))
            {
                res.Remove(fin);
                res.Insert(at + 1, fin);
                keep = at + 2;
            }
            if (res.Count > keep) res.RemoveRange(keep, res.Count - keep);
        }

        /// <summary>NIGHT36 (the lead; builder L owns the row): design.n36.level36 (on) swaps each id in its `levelSwap` map ({old id: new
        /// id}, after N35Levels: L's from-scratch n36 files; tonight level 1 only). A flow choice (the level a run loads is on its tape).
        /// Off with -skygrindOff36 / --off36 (N36Blocks.ForceOff) or with the row off: NIGHT35's order.</summary>
        public static void N36Levels(GameConfig cfg, List<string> res)
        {
            if (N36Blocks.ForceOff || res.Count == 0 || !cfg.Design("n36.level36.enabled")) return;
            if (At(cfg, "design.n36.level36.levelSwap") is Dictionary<string, object> map)
                for (int i = 0; i < res.Count; i++)
                    if (map.TryGetValue(res[i], out object to) && to is string s && s.Length > 0) res[i] = s;
        }

        /// <summary>NIGHT37 (the lead; each boss builder owns its row): a boss row on (design.n37.wick37 / stag37 / loom37 / crown37) swaps
        /// its `levelFrom` for its `levelTo` (after N36Levels: the level with the new boss). A flow choice (the level a run loads is on
        /// its tape). Off with -skygrindOff37 / --off37 (N37Blocks.ForceOff) or with the row off: NIGHT36's order.</summary>
        public static void N37Levels(GameConfig cfg, List<string> res)
        {
            if (N37Blocks.ForceOff || res.Count == 0) return;
            foreach (string row in N37LevelRows)
            {
                if (!cfg.Design("n37." + row + ".enabled")) continue;
                string from = At(cfg, "design.n37." + row + ".levelFrom") as string, to = At(cfg, "design.n37." + row + ".levelTo") as string;
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) continue;
                int i = res.IndexOf(from);
                if (i >= 0) res[i] = to;
            }
        }

        static readonly string[] N37LevelRows = { "wick37", "stag37", "loom37", "crown37" };

        /// <summary>NIGHT38 (the lead; each level builder owns its row): a level row on (design.n38.foundry38 / iceline38 / kiln38 / span38 /
        /// spire38 / finale38) swaps its `levelFrom` for its `levelTo` (after N37Levels: the order with NIGHT37's boss rows). A flow choice
        /// (the level a run loads is on its tape). Off with -skygrindOff38 / --off38 (N38Blocks.ForceOff) or with the row off: NIGHT37's order.</summary>
        public static void N38Levels(GameConfig cfg, List<string> res)
        {
            if (N38Blocks.ForceOff || res.Count == 0) return;
            foreach (string row in N38LevelRows)
            {
                if (!cfg.Design("n38." + row + ".enabled")) continue;
                string from = At(cfg, "design.n38." + row + ".levelFrom") as string, to = At(cfg, "design.n38." + row + ".levelTo") as string;
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) continue;
                int i = res.IndexOf(from);
                if (i >= 0) res[i] = to;
            }
        }

        static readonly string[] N38LevelRows = { "foundry38", "iceline38", "kiln38", "span38", "spire38", "finale38" };

        /// <summary>NIGHT40 (the lead's hook; each level builder owns its row): a level row on (design.n40.kiln40 / iceline40 / loom40 /
        /// crown40) swaps its `levelFrom` for its `levelTo` (after N38Levels); cut40 on removes every id in design.n40.cut40.cut (level 6).
        /// A flow choice (the level a run loads is on its tape). Off with -skygrindOff40 / --off40 (N40Blocks.ForceOff) or the row off.</summary>
        public static void N40Levels(GameConfig cfg, List<string> res)
        {
            if (N40Blocks.ForceOff || res.Count == 0) return;
            foreach (string row in N40LevelRows)
            {
                if (!cfg.Design("n40." + row + ".enabled")) continue;
                string from = At(cfg, "design.n40." + row + ".levelFrom") as string, to = At(cfg, "design.n40." + row + ".levelTo") as string;
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) continue;
                int i = res.IndexOf(from);
                if (i >= 0) res[i] = to;
            }
            if (cfg.Design("n40.cut40.enabled") && At(cfg, "design.n40.cut40.cut") is List<object> cut)
                foreach (object o in cut)
                    if (o is string id && res.Count > 1) res.Remove(id);
        }

        static readonly string[] N40LevelRows = { "kiln40", "iceline40", "loom40", "crown40" };

        /// <summary>NIGHT41 (the lead's hook; each level builder owns its row): a level row on (design.n41.foundry41 / kiln41 / spire41)
        /// swaps its `levelFrom` for its `levelTo` (after N40Levels); finale41 on puts its `levelTo` back as the last level (level 6; OFF
        /// tonight, DECIDE41 D-3). A flow choice (the level a run loads is on its tape). Off with -skygrindOff41 / --off41
        /// (N41Blocks.ForceOff) or the row off.</summary>
        public static void N41Levels(GameConfig cfg, List<string> res)
        {
            if (N41Blocks.ForceOff || res.Count == 0) return;
            foreach (string row in N41LevelRows)
            {
                if (!cfg.Design("n41." + row + ".enabled")) continue;
                string from = At(cfg, "design.n41." + row + ".levelFrom") as string, to = At(cfg, "design.n41." + row + ".levelTo") as string;
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) continue;
                int i = res.IndexOf(from);
                if (i >= 0) res[i] = to;
            }
            if (cfg.Design("n41.finale41.enabled") && At(cfg, "design.n41.finale41.levelTo") is string last && last.Length > 0 && !res.Contains(last))
                res.Add(last);
        }

        static readonly string[] N41LevelRows = { "foundry41", "kiln41", "spire41" };

        /// <summary>NIGHT42 (the lead's hook; L1 owns the row): foundry42 on swaps its `levelFrom` (n41-foundry) for its `levelTo`
        /// (n42-foundry), after N41Levels. Off with -skygrindOff42 / --off42 (N42Blocks.ForceOff) or the row off.</summary>
        public static void N42Levels(GameConfig cfg, List<string> res)
        {
            if (N42Blocks.ForceOff || res.Count == 0) return;
            foreach (string row in N42LevelRows)
            {
                if (!cfg.Design("n42." + row + ".enabled")) continue;
                string from = At(cfg, "design.n42." + row + ".levelFrom") as string, to = At(cfg, "design.n42." + row + ".levelTo") as string;
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) continue;
                int i = res.IndexOf(from);
                if (i >= 0) res[i] = to;
            }
        }

        static readonly string[] N42LevelRows = { "foundry42" };

        /// <summary>NIGHT43 (the lead's hook; L1 / E1 / K1 own the rows): foundry43, iceline43 and kiln43 on swap their `levelFrom`
        /// (n42-foundry, n40-iceline, n41-kiln) for their `levelTo` (n43-*), after N42Levels. Off with -skygrindOff43 / --off43
        /// (N43Blocks.ForceOff) or the row off.</summary>
        public static void N43Levels(GameConfig cfg, List<string> res)
        {
            if (N43Blocks.ForceOff || res.Count == 0) return;
            foreach (string row in N43LevelRows)
            {
                if (!cfg.Design("n43." + row + ".enabled")) continue;
                string from = At(cfg, "design.n43." + row + ".levelFrom") as string, to = At(cfg, "design.n43." + row + ".levelTo") as string;
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) continue;
                int i = res.IndexOf(from);
                if (i >= 0) res[i] = to;
            }
        }

        static readonly string[] N43LevelRows = { "foundry43", "iceline43", "kiln43" };

        public static void N33Levels(GameConfig cfg, List<string> res)
        {
            if (N33Blocks.ForceOff || res.Count == 0) return;
            if (!cfg.Design("n33.kit33.enabled") || !(At(cfg, "design.n33.kit33.levelSwap") is Dictionary<string, object> map)) return;
            for (int i = 0; i < res.Count; i++)
                if (map.TryGetValue(res[i], out object to) && to is string s && s.Length > 0) res[i] = s;
        }

        static object At(GameConfig cfg, string path) => cfg.Has(path) ? cfg.Get(path) : null;

        /// <summary>NIGHT30: the rebuilt levels' switch (design.n30.levels.enabled, a flow row with no sim bit). Off with -skygrindOff30 (N30Blocks.ForceOff).</summary>
        public static bool N30Levels(GameConfig cfg) => !N30Blocks.ForceOff && cfg.Design("n30.levels.enabled");

        /// <summary>The level a new CAMPAIGN run starts on.</summary>
        public static string Start(GameConfig cfg, N25Blocks n25) => Order(cfg, n25)[0];

        /// <summary>The level after `current` (null: it was the last, or it is not in the order).</summary>
        public static string Next(GameConfig cfg, N25Blocks n25, string current)
        {
            List<string> order = Order(cfg, n25);
            int at = order.IndexOf(current);
            return at >= 0 && at + 1 < order.Count ? order[at + 1] : null;
        }
    }
}
