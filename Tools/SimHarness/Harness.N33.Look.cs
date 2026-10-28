// NIGHT33 L (design.n33.look33): `light33 [level ...]` (dispatched by the lead's Program.cs: N33Look(cmd, args, ref code)).
//   Scans every campaign level (Design/levels/n32-*.json and any n33-*.json; or the ids given) with LightPlan33 (the placement code the views run) and
//   prints, per level, RED (NIGHT32's placement, look33 off) and GREEN (look33): lights total / lights with no fixture within 0.6 m / emissive bars not
//   mounted (a 0.5 m slice more than 0.02 m from level geometry) / lights over a pit or lava; plus the light shafts with no fixture at their top and the
//   lava lights (by design over lava, at most 1.25 m up). `--why` lists every GREEN failure. Exit 1 when any GREEN count is above 0.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N33Look(string cmd, string[] args, ref int code)
    {
        if (cmd != "light33") return;
        code = 0;
        GameConfig cfg = Config();
        LightPlan33.Nums nums = LightPlan33.Nums.From(cfg);
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "Design", "levels");
        var files = new List<string>();
        bool why = false;
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--why") { why = true; continue; }
            string a = args[i].EndsWith(".json") ? args[i] : Path.Combine(dir, args[i] + ".json");
            if (!a.Contains(Path.DirectorySeparatorChar.ToString()) && !a.Contains("/")) a = Path.Combine(dir, a);
            files.Add(a);
        }
        if (files.Count == 0)
        {
            foreach (string f in Directory.GetFiles(dir, "n32-*.json")) files.Add(f);
            foreach (string f in Directory.GetFiles(dir, "n33-*.json")) files.Add(f);
            files.Sort(StringComparer.Ordinal);
        }
        Console.WriteLine("light33: every light has a fixture (L1 rule 1). Columns: lights / no fixture <= 0.6 m / bars unmounted (of bars) / over pit-lava / shafts unmounted (of shafts) / lava lights (floating)");
        int greenBad = 0;
        foreach (string f in files)
        {
            if (!File.Exists(f)) { Console.WriteLine($"{Path.GetFileNameWithoutExtension(f)}: missing"); code = 1; continue; }
            var json = PortData.Obj(JsJson.Parse(File.ReadAllText(f)));
            LightPlan33.Level lv = LightPlan33.Read(json, Path.GetFileNameWithoutExtension(f));
            if (lv == null) { Console.WriteLine($"{Path.GetFileNameWithoutExtension(f)}: not a campaign level"); continue; }
            var whyList = why ? new List<string>() : null;
            LightPlan33.Report red = LightPlan33.Check(lv, LightPlan33.Red(lv, nums));
            LightPlan33.Report green = LightPlan33.Check(lv, LightPlan33.Green(lv, nums), whyList, UnityTuning.Look33KitSlots, true, 0);
            int bad = green.noFixture + green.barsUnmounted + green.overPitFloating + green.beamsUnmounted + green.lavaFloating + green.hdrOver + (green.realMax > 6 || green.realMin < 3 ? 1 : 0);
            greenBad += bad;
            Console.WriteLine($"{lv.name,-12} RED   {Row(red)}");
            Console.WriteLine($"{"",-12} GREEN {Row(green)}  {(bad == 0 ? "PASS" : "RED")}");
            Console.WriteLine($"{"",-12} GREEN lights by type: {green.types}");
            if (whyList != null) foreach (string w in whyList) Console.WriteLine("    " + w);
        }
        Console.WriteLine(greenBad == 0 ? "SUMMARY PASS: no GREEN light without a fixture, no unmounted bar, no light over a pit" : $"SUMMARY RED: {greenBad} GREEN failures (run with --why)");
        if (greenBad > 0) code = 1;
    }

    static string Row(LightPlan33.Report r) => $"lights {r.lights,3} / no fixture {r.noFixture,3} / bars unmounted {r.barsUnmounted,3} of {r.bars,3} / over pit {r.overPit,2} ({r.overPitFloating,2} floating) / shafts unmounted {r.beamsUnmounted,2} of {r.beams,2} / lava {r.lava,2} ({r.lavaFloating} floating) / real lights per room {r.realMin}-{r.realMax} / HDR max {r.hdrMax:0.0} ({r.hdrOver} over 4)";
}
