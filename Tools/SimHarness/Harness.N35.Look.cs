// NIGHT35 V (design.n35.lights35): `light35 [level ...] [--why]` (dispatched by the lead's Program.cs: N35Look(cmd, args, ref code)).
//   Scans the campaign levels (Design/levels/n34-*.json and n35-*.json; or the ids given) with LightPlan33.Green (the placement the views run), lights35
//   off (RED: NIGHT34's) vs on (GREEN), and prints per level: fixtures hanging in the sky (LightPlan33.SkyFixtures: no solid box beside or over them
//   reaching their bottom) and the rooms they are in, the light33 numbers (lights, lights with no fixture within 0.6 m, bars unmounted, lights over a
//   pit), and the GREEN lights by type. Exit 1 when any GREEN level keeps a sky fixture, a light with no fixture, or an unmounted part.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N35Look(string cmd, string[] args, ref int code)
    {
        if (cmd != "light35") return;
        code = 0;
        GameConfig cfg = Config();
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "Design", "levels");
        var files = new List<string>();
        bool why = false;
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--why") { why = true; continue; }
            files.Add(args[i].EndsWith(".json") ? args[i] : Path.Combine(dir, args[i] + ".json"));
        }
        if (files.Count == 0)
        {
            foreach (string f in Directory.GetFiles(dir, "n34-*.json")) files.Add(f);
            foreach (string f in Directory.GetFiles(dir, "n35-*.json")) files.Add(f);
            files.Sort(StringComparer.Ordinal);
        }
        Console.WriteLine("light35: a fixture only where a surface holds it (the lead's n34-r5 f5 / f5b). Columns: sky fixtures (rooms) / lights / no fixture <= 0.6 m / parts unmounted (of parts) / over pit");
        int bad = 0;
        foreach (string f in files)
        {
            if (!File.Exists(f)) { Console.WriteLine($"{Path.GetFileNameWithoutExtension(f)}: missing"); code = 1; continue; }
            var json = PortData.Obj(JsJson.Parse(File.ReadAllText(f)));
            LightPlan33.Level lv = LightPlan33.Read(json, Path.GetFileNameWithoutExtension(f));
            if (lv == null) { Console.WriteLine($"{Path.GetFileNameWithoutExtension(f)}: not a campaign level"); continue; }
            int open = 0;
            foreach (LightPlan33.Room r in lv.rooms) if (!r.roof) open++;
            LightPlan33.Nums nr = LightPlan33.Nums.From(cfg), ng = LightPlan33.Nums.From(cfg);
            nr.lights35 = false;
            ng.lights35 = true;
            LightPlan33.Plan red = LightPlan33.Green(lv, nr), green = LightPlan33.Green(lv, ng);
            var whyList = why ? new List<string>() : null;
            LightPlan33.Report rr = LightPlan33.Check(lv, red, null, UnityTuning.Look33KitSlots, true, 0);
            LightPlan33.Report gr = LightPlan33.Check(lv, green, whyList, UnityTuning.Look33KitSlots, true, 0);
            int skyR = LightPlan33.SkyFixtures(lv, red, out int skyRoomsR), skyG = LightPlan33.SkyFixtures(lv, green, out int skyRoomsG);
            int b = skyG + gr.noFixture + gr.barsUnmounted + gr.overPitFloating;
            bad += b;
            Console.WriteLine($"{lv.name,-14} rooms {lv.rooms.Count} ({open} open to the sky)");
            Console.WriteLine($"{"",-14} RED   sky {skyR,3} ({skyRoomsR} rooms) / lights {rr.lights,3} / no fixture {rr.noFixture,2} / unmounted {rr.barsUnmounted,2} of {rr.bars,3} / over pit {rr.overPit,2}");
            Console.WriteLine($"{"",-14} GREEN sky {skyG,3} ({skyRoomsG} rooms) / lights {gr.lights,3} / no fixture {gr.noFixture,2} / unmounted {gr.barsUnmounted,2} of {gr.bars,3} / over pit {gr.overPit,2}  {(b == 0 ? "PASS" : "RED")}");
            Console.WriteLine($"{"",-14} GREEN lights by type: {gr.types}");
            if (whyList != null) foreach (string w in whyList) Console.WriteLine("    " + w);
        }
        Console.WriteLine(bad == 0 ? "SUMMARY PASS: no GREEN fixture in the sky, no light without a fixture, no unmounted part" : $"SUMMARY RED: {bad} GREEN failures (run with --why)");
        if (bad > 0) code = 1;
    }
}
