// Scratch (builder L, L5): GrowPlan72.Classes / Count over the campaign level files, compared with Tools/night/n72-zscan.cjs --json's classes and counts.
// usage: grow72check <worktree> <zscan.json> <map> [<map> ...]
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

static class P
{
    static int Main(string[] a)
    {
        string wt = a[0];
        var zs = (Dictionary<string, object>)JsJson.Parse(File.ReadAllText(a[1]));
        var levels = (Dictionary<string, object>)zs["levels"];
        int bad = 0;
        for (int i = 2; i < a.Length; i++)
        {
            string map = a[i];
            string path = Path.Combine(wt, "Assets", "StreamingAssets", "Design", "levels", map + ".json");
            SurvivalMap m = SurvivalMap.FromExport(PortData.Obj(JsJson.Parse(File.ReadAllText(path))));
            int[] cls = GrowPlan72.Classes(m.boxes);
            GrowPlan72.Report r = GrowPlan72.Count(m.boxes, cls);
            var js = (Dictionary<string, object>)levels[map];
            var jc = (List<object>)js["classes"];
            bool same = jc.Count == cls.Length;
            for (int k = 0; same && k < cls.Length; k++) if ((int)(double)jc[k] != cls[k]) same = false;
            bool counts = (int)(double)js["raw"] == r.raw && (int)(double)js["mod3"] == r.mod3 && (int)(double)js["plan"] == r.plan;
            Console.WriteLine(GrowPlan72.LogLine(map, r) + " | classes equal to the node tool's: " + (same ? "yes" : "NO") + " | counts equal: " + (counts ? "yes" : "NO"));
            if (!same || !counts) bad++;
        }
        Console.WriteLine(bad == 0 ? "CROSS-CHECK PASS" : "CROSS-CHECK FAIL " + bad);
        return bad;
    }
}
