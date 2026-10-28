// NIGHT43 builder E1: `n43rooms iceline` (G23 / G19, design.n43.iceline43). Level 2 played by `n43rooms clear`'s harness player (kill path
// `kill`, rooms43 on) on n40-iceline (the control) and on n43-iceline (iceline43 on): per room the waves spawned, the CHARGERS that came before
// THE BRAWL (x9), and in g6 when the KESTREL lesson cleared vs when the FUSION husk (wave 1, afterLesson43) spawned. RED when n43-iceline
// has fewer than 2 more chargers than n40-iceline, g6's husk spawned before its lesson, or a fight room's door stayed shut.
//   n43rooms iceline [--seed n]
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N43RoomsMore3(string what, string[] args, ref int code)
    {
        if (what == "iceline") { code = N43RmIceline(args); return; }
        N43RoomsMore4(what, args, ref code);
    }

    static partial void N43RoomsMore4(string what, string[] args, ref int code);

    static int N43RmIceline(string[] args)
    {
        uint seed = uint.Parse(N43Arg(args, "--seed", "1"), CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        int bad = 0;
        var chargers = new int[2];
        for (int k = 0; k < 2; k++)
        {
            bool n43 = k == 1;
            string id = N43RmLevelId(cfg, "iceline", n43);
            string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
            var spawnsByRoom = new SortedDictionary<string, List<string>>();
            double g6Lesson0 = -1, g6Wave1 = -1;
            int g6Spawns = 0;
            SurvivalSim simRef = null;
            var rows = N43RmClearRun(cfg, path, "kill", true, n43, seed, 120, false, sim =>
            {
                simRef = sim;
                int st = 0;
                sim.events.On<RoomEvent>("roomEnter", _ => { });
                sim.events.On<SpawnEvent>("enemySpawn", e =>
                {
                    Campaign24 c = sim.campaign;
                    string room = c != null && c.Room != null ? c.Room.id : "?";
                    if (!spawnsByRoom.TryGetValue(room, out var l)) spawnsByRoom[room] = l = new List<string>();
                    l.Add(e.kind);
                    if (room == "g6" && ++g6Spawns == 3) g6Wave1 = c.roomSteps * sim.step;
                });
                sim.events.On<LessonEvent>("lessonClear", e => { if (e.id == "g6" && e.index <= 1 && g6Lesson0 < 0) g6Lesson0 = e.time; });
                st++;
            });
            int ch = spawnsByRoom.Where(kv => kv.Key != "x9").Sum(kv => kv.Value.Count(x => x == "charger"));
            chargers[k] = ch;
            int held = rows.Count(r => r.fight && !r.opened);
            bad += held;
            Console.WriteLine($"  {id} ({(n43 ? "iceline43 on" : "the control")}): chargers before THE BRAWL {ch}; fight doors opened {rows.Count(r => r.fight && r.opened)}/{rows.Count(r => r.fight)}");
            foreach (var kv in spawnsByRoom)
                Console.WriteLine($"      {kv.Key}: {string.Join(" ", kv.Value.GroupBy(x => x).Select(gp => gp.Key + " x" + gp.Count()))}");
            if (n43)
            {
                bool order = g6Wave1 < 0 || (g6Lesson0 >= 0 && g6Wave1 >= g6Lesson0);
                Console.WriteLine($"      g6: the KESTREL lesson cleared at {g6Lesson0:0.00} s, the FUSION husk spawned at {g6Wave1:0.00} s (room time): {(order && g6Wave1 >= 0 ? "after the lesson" : "RED")}");
                if (!order || g6Wave1 < 0) bad++;
            }
        }
        bool more = chargers[1] >= chargers[0] + 2;
        if (!more) bad++;
        Console.WriteLine($"n43rooms iceline SUMMARY: chargers before THE BRAWL {chargers[0]} -> {chargers[1]} ({(more ? "+" + (chargers[1] - chargers[0]) : "RED: not 2 more")}); {(bad == 0 ? "PASS" : "RED")}");
        return bad == 0 ? 0 : 1;
    }
}
