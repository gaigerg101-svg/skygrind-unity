// Scratch (builder L, L6): the fixture61 meter on the campaign level files, offline: Fixture61Math.Mounts / Count over LightPlan33.GreenFor, with the real
// numbers (UnityTuning.N61V4.cs) and the repo's config. drawn = null counts every ceiling light (the Player skips panels over a grapple node: this is the
// superset). usage: fix61 <worktree> [map ...]
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;
using M = Skygrind.Game.Fixture61Math;

static class P
{
    static int Main(string[] a)
    {
        string wt = a[0];
        var maps = new List<string>();
        for (int i = 1; i < a.Length; i++) maps.Add(a[i]);
        if (maps.Count == 0) maps.AddRange(new[] { "n52-foundry", "n52-iceline", "n57-kiln", "n58-span", "n52-spire", "n65-foundry", "n67-iceline", "n67-kiln", "n67-spire" });
        GameConfig cfg = GameConfig.Load(Path.Combine(wt, "PortExport", "config.json"));
        LightPlan33.Lights35On = c => c.Design("n35.lights35.enabled");
        int bad = 0;
        foreach (string map in maps)
        {
            string levelDir = (Environment.GetEnvironmentVariable("FIX61_LEVELDIR") ?? "Assets/StreamingAssets/Design/levels").Replace('/', Path.DirectorySeparatorChar);
            string path = Path.Combine(wt, levelDir, map + ".json");
            var json = (Dictionary<string, object>)JsJson.Parse(File.ReadAllText(path));
            LightPlan33.Plan plan = LightPlan33.GreenFor(json, cfg, true, out LightPlan33.Level lv);
            if (plan == null) { Console.WriteLine(map + ": no plan"); continue; }
            List<M.Mount> mounts = M.Mounts(lv, plan, M.Dims.Tuning, null);
            M.Report r = M.Count(plan, mounts, UnityTuning.Fixture61MinRimM, UnityTuning.Look33FrameWidth, null);
            M.Report red = M.Count(plan, new List<M.Mount>(), UnityTuning.Fixture61MinRimM, UnityTuning.Look33FrameWidth, null);
            Console.WriteLine(M.LogLine(map, r) + " | red (no mounts) unmounted " + red.unmounted + " | mount boxes " + mounts.Count);
            if (Environment.GetEnvironmentVariable("FIX61_LIST") == "1")
                for (int i = 0; i < plan.fixes.Count; i++)
                {
                    LightPlan33.Fix f = plan.fixes[i];
                    if (!M.CeilingLight(f)) continue;
                    bool held = false;
                    foreach (M.Mount mt in mounts) if (mt.light == i && mt.kind != "housing") held = true;
                    Console.WriteLine("  light " + i + " room " + f.room + " kind " + f.kind + " type " + f.type + " y " + f.box.y0.ToString("0.00") + ".." + f.box.y1.ToString("0.00") + " x " + f.box.x0.ToString("0.0") + " z " + f.box.z0.ToString("0.0") + (held ? "" : "  UNHELD"));
                }
            bad += r.unmounted;
        }
        Console.WriteLine("TOTAL unmounted " + bad);
        return 0;
    }
}
