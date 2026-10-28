// NIGHT35 builder A (NIGHT35.md A1): campaign35 / lines35.
//   campaign35               the campaign order with design.n35.campaign6 forced ON (the rest of the config as it is): six levels, the finale last, and per
//                            level: its file (Design/levels/<id>.json), its map row (design.maps.list), its unlock row (design.n31.unlocks: n = its place,
//                            the weapons it starts with), whether the grapple is owned there (design.n35.grapple35.fromLevel), its teach35 trigger ids.
//                            Exit 1 on any gap (a missing map or unlock row is RED until the lead runs Tools/night/n35a-lead-rows.cjs).
//   lines35 [level ...]      lines34's per-room bar (walls, nodes, floors, lane, climb) on each level (default: the six of campaign35), with nodes NOT
//                            required before the grapple's level (DECIDE35 D-3); the ride rooms' ride35 rows printed.
//   lines35 walk [level ...] the walk bot (lines34 walk: walk29's pilot, devices ON) on each level (default: the six), 10 seeds; --seeds N.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;

public static partial class Harness
{
    static partial void N35Levels(string cmd, string[] args, ref int code)
    {
        if (cmd != "campaign35" && cmd != "lines35") return;
        if (cmd == "campaign35") { code = Campaign35(true); return; }
        bool walk = args.Length > 1 && args[1] == "walk";
        int seeds = 10;
        for (int i = 1; i < args.Length - 1; i++) if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
        var names = args.Skip(walk ? 2 : 1).Where(a => !a.StartsWith("--") && !char.IsDigit(a[0])).ToList();
        List<string> order = Campaign35Order(Config());
        if (names.Count == 0) names = order;
        int from = Grapple35From(Config());
        code = 0;
        foreach (string id in names)
        {
            if (walk) { code |= Lines34Walk(id, seeds); continue; }
            int at = order.IndexOf(id);
            bool before = at >= 0 && at + 1 < from;
            lines34NodesOptional = before;
            Console.WriteLine($"lines35 {id}: campaign level {(at >= 0 ? (at + 1).ToString(CultureInfo.InvariantCulture) : "?")}, nodes {(before ? "optional (before the grapple's level " + from + ", D-3)" : "required")}");
            try { code |= Lines34Rooms(id); }
            finally { lines34NodesOptional = false; }
            Lines35Rides(id);
        }
    }

    static int Grapple35From(GameConfig cfg) => cfg.Has("design.n35.grapple35.fromLevel") ? (int)cfg.Num("design.n35.grapple35.fromLevel") : 4;

    /// <summary>Curriculum25's order with design.n35.campaign6 forced on (every older row as the config has it; no --off switch).</summary>
    static List<string> Campaign35Order(GameConfig cfg)
    {
        if (cfg.root["design"] is Dictionary<string, object> d && d.TryGetValue("n35", out object n) && n is Dictionary<string, object> n35 && n35.TryGetValue("campaign6", out object c) && c is Dictionary<string, object> row)
            row["enabled"] = true;
        return Curriculum25.Order(cfg, new N25Blocks { tutorial = true, curriculum = true });
    }

    static int Campaign35(bool print)
    {
        GameConfig cfg = Config();
        List<string> order = Campaign35Order(cfg);
        int from = Grapple35From(cfg);
        int bad = 0;
        string finale = cfg.Has("design.n35.campaign6.finaleId") ? cfg.Str("design.n35.campaign6.finaleId") : "";
        Console.WriteLine($"campaign35: the order with design.n35.campaign6 on: {string.Join(", ", order)}");
        if (order.Count != 6 || order[order.Count - 1] != finale) { bad++; Console.WriteLine($"campaign35 RED: {order.Count} levels, last {order.LastOrDefault()} (want 6, the finale {finale} last)"); }
        var maps = new HashSet<string>();
        if (cfg.Has("design.maps.list")) foreach (object o in cfg.Arr("design.maps.list")) if (o is Dictionary<string, object> m && m.TryGetValue("id", out object mid)) maps.Add(mid as string);
        Unlocks31 u = Unlocks31.From(cfg);
        for (int i = 0; i < order.Count; i++)
        {
            string id = order[i];
            string file = Path.Combine(RepoRoot, "Design", "levels", id + ".json");
            string sa = Path.Combine(RepoRoot, "Assets", "StreamingAssets", "Design", "levels", id + ".json");
            bool hasFile = File.Exists(file), sameSa = hasFile && File.Exists(sa) && File.ReadAllText(file) == File.ReadAllText(sa);
            bool hasMap = maps.Contains(id);
            int n = u.LevelN(id);
            List<string> start = u.StartFor(id);
            var teach = new List<string>();
            string boss = "";
            if (hasFile)
            {
                var json = PortData.Obj(PortData.LoadJson(file));
                if (json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp)
                {
                    if (camp.TryGetValue("rooms", out object ro) && ro is List<object> rooms)
                        foreach (object r in rooms)
                            if (r is Dictionary<string, object> rd && rd.TryGetValue("boss", out object b) && b is string bs) boss = bs;
                    if (camp.TryGetValue("teach35", out object t) && t is List<object> tl) // builder T's format: campaign.teach35 [{id, room, rect, minY}]
                        foreach (object tt in tl) if (tt is Dictionary<string, object> td && td.TryGetValue("id", out object tid)) teach.Add($"{tid} ({(td.TryGetValue("room", out object rm) ? rm : "?")})");
                }
            }
            bool ok = hasFile && sameSa && hasMap && n == i + 1 && start != null;
            if (!ok) bad++;
            Console.WriteLine($"campaign35 {(ok ? "GREEN" : "RED")} L{i + 1} {id}: file {(hasFile ? "ok" : "MISSING")}, Player copy {(sameSa ? "same" : "MISSING OR DIFFERENT")}, map row {(hasMap ? "ok" : "MISSING")}, unlock row n {n} {(n == i + 1 ? "ok" : "WRONG (want " + (i + 1) + ")")}, starts with [{(start == null ? "NO ROW: the whole loadout" : string.Join(", ", start))}], grapple {(i + 1 >= from ? "owned" : "not owned (rings dim)")}, boss {boss}, teach35 [{string.Join(", ", teach)}]");
        }
        Console.WriteLine($"campaign35: {(bad == 0 ? "ALL GREEN" : bad + " RED")}");
        return bad == 0 ? 0 : 1;
    }

    /// <summary>The ride rooms' ride35 rows and their D-3 notes (data the generators wrote), for the log.</summary>
    static void Lines35Rides(string id)
    {
        string file = Path.Combine(RepoRoot, "Design", "levels", id + ".json");
        if (!File.Exists(file)) return;
        var json = PortData.Obj(PortData.LoadJson(file));
        if (!(json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp && camp.TryGetValue("rooms", out object ro) && ro is List<object> rooms)) return;
        foreach (object r in rooms)
        {
            if (!(r is Dictionary<string, object> rd)) continue;
            if (rd.TryGetValue("ride35", out object rv) && rv is Dictionary<string, object> ride)
                Console.WriteLine($"lines35 {id} {rd["id"]}: ride room: {(ride.TryGetValue("line", out object l) ? l : "")}");
            if (rd.TryGetValue("d3", out object d3)) Console.WriteLine($"lines35 {id} {rd["id"]}: {d3}");
        }
    }
}
