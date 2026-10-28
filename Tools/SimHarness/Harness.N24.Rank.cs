// NIGHT24 leftovers 1 and 2 (builder R): `ranks [--selftest]`. Runs Campaign24LimitRankTests (the LIMIT flags NO SLIDE / NO JUMP / MELEE ONLY and the
// room ranks) and prints, from layer 1's real plan (Design/levels/layer1.json), each room's par and the rank a clear earns at 0.8 / 1.2 / 2 / 3 x
// par with 0, 1 and 2 deaths, the overall rank and the closing stamp's text for a sample layer. Exit 0 iff the tests pass.
// Dispatch (Harness.N24.cs): else if (cmd == "ranks") RanksCommand(args, ref code);
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;

public static partial class Harness
{
    static void RanksCommand(string[] args, ref int code)
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        int failed = NUnit.Framework.MiniRunner.Run(typeof(Skygrind.Core.Tests.Campaign24LimitRankTests), Console.WriteLine);
        GameConfig cfg = Config();
        string path = Path.Combine("Design", "levels", "layer1.json");
        if (File.Exists(path))
        {
            LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
            var c = new Campaign24(cfg, new EventBus(), level, null, 1.0 / 120);
            c.Start(1);
            Console.WriteLine($"ranks: {level.name}, {level.rooms.Count} rooms; rank by time over par (rows) and deaths (columns 0 / 1 / 2)");
            for (int i = 0; i < level.rooms.Count; i++)
            {
                RoomDef r = level.rooms[i];
                var sb = new System.Text.StringBuilder($"  room {i + 1} {r.id} ({r.kind}, par {r.par.ToString("0", ci)} s, limits '{Limit24.Banner(r.limits)}'):");
                foreach (double share in new[] { 0.8, 1.2, 2.0, 3.0 })
                    sb.Append($"  x{share.ToString("0.0", ci)} ").Append(c.RankOf(r.par * share, r.par, 0)).Append(c.RankOf(r.par * share, r.par, 1)).Append(c.RankOf(r.par * share, r.par, 2));
                Console.WriteLine(sb.ToString());
            }
            foreach (char ch in new[] { 'S', 'A', 'A', 'B' }) c.roomRanks.Add(ch);
            Console.WriteLine($"ranks: a sample layer S A A B -> overall {c.OverallRank()}, stamp \"{c.StampText()}\"");
        }
        else Console.WriteLine("ranks: Design/levels/layer1.json not found (run from the repo root)");
        code = failed == 0 ? 0 : 1;
        Console.WriteLine(failed == 0 ? "ranks PASS" : "ranks FAIL");
    }
}
