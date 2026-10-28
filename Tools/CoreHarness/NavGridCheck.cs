// NIGHT15 U3-28.3 (builder N): the nav grid port checked outside Unity. For each exported map (PortExport/maps/*.json)
// it runs NavGridBuilder.ExportFor (the exporter's whole step: the collider, the zone parts, tagAt, the build) and
// compares the result with the file's navGrid and zoneParts bit for bit (GoldenResult.SameJson, keys in order), then the
// duel grid (spire-duel-navgrid.json: the core at config.core.duelPreset). One line per check, EQUAL or the first
// differences. From the repo root:
//   dotnet run --project Tools/CoreHarness -c Release -- navgrid [portExportDir]
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Skygrind.Core;

public static class NavGridCheck
{
    static string Verdict(List<string> diffs) => diffs.Count == 0 ? "EQUAL" : $"DIFFERENT ({diffs.Count} mismatches): {string.Join(" | ", diffs.Take(5))}";

    static List<string> Diff(object want, object got, string what)
    {
        var r = new GoldenResult(what);
        r.SameJson(want, got, what, ordered: true);
        return r.mismatches;
    }

    static string Counts(Dictionary<string, object> g)
    {
        var c = PortData.Obj(g["linkCounts"]);
        int links = PortData.Arr(PortData.Obj(g["links"])["linkTo"]).Count;
        return $"{PortData.Num(g["count"])} nodes, {links} links (walk {PortData.Num(c["walk"])}, jump {PortData.Num(c["jump"])}, drop {PortData.Num(c["drop"])}, gap {PortData.Num(c["gap"])})";
    }

    /// <summary>Returns the number of failing checks.</summary>
    public static int Run(string root, Action<string> log)
    {
        int bad = 0;
        var cfg = new GameConfig(PortData.Obj(PortData.LoadJson(Path.Combine(root, "config.json"))));
        foreach (string name in new[] { "survival", "spire", "training" })
        {
            try
            {
                var json = PortData.Obj(PortData.LoadJson(Path.Combine(root, "maps", name + ".json")));
                var t = Stopwatch.StartNew();
                var (navGrid, zoneParts) = NavGridBuilder.ExportFor(cfg, json);
                double ms = t.Elapsed.TotalMilliseconds;
                List<string> d = Diff(json["navGrid"], navGrid, name + ".navGrid");
                if (d.Count > 0) bad++;
                log($"navgrid {name}: {Counts(navGrid)}: {Verdict(d)} (ExportFor {ms:F0} ms)");
                List<string> z = Diff(json["zoneParts"], zoneParts, name + ".zoneParts");
                if (z.Count > 0) bad++;
                int parts = zoneParts == null ? 0 : PortData.Arr(zoneParts["parts"]).Count;
                log($"zoneParts {name}: {(zoneParts == null ? "none" : parts + " parts")}: {Verdict(z)}");
            }
            catch (Exception e)
            {
                bad++;
                log($"navgrid {name}: FAIL {e.GetType().Name}: {e.Message}");
            }
        }
        try
        {
            var json = PortData.Obj(PortData.LoadJson(Path.Combine(root, "maps", "spire.json")));
            var want = PortData.Obj(PortData.LoadJson(Path.Combine(root, "maps", "spire-duel-navgrid.json")));
            var spec = PortData.Obj(PortData.Obj(json["core"])["spec"]);
            var cs = new CoreSpec { x0 = PortData.Num(spec["x0"]), z0 = PortData.Num(spec["z0"]), cells = (int)PortData.Num(spec["cells"]), cellSize = PortData.Num(spec["cellSize"]) };
            MovementConfig m = cfg.movement;
            var core = new CoreGrid(m, cs);
            string preset = cfg.Str("core.duelPreset");
            core.SetHeights(CoreGrid.PresetHeights(preset, cs.cells, m.core.maxHeight, m.core.heightStep, new Mulberry32(1)));
            var t = Stopwatch.StartNew();
            var got = NavGridBuilder.Build(cfg, new LayeredWorld(PortData.BuildMapOctree(json), core), PortData.Num(json["floorSize"]), null, preset);
            double ms = t.Elapsed.TotalMilliseconds;
            var d = new List<string>();
            foreach (string k in new[] { "cell", "size", "origin", "count", "linkKinds", "linkCounts", "corePreset" }) d.AddRange(Diff(want[k], got[k], "duel." + k));
            foreach (string k in new[] { "y", "tag", "col" }) d.AddRange(Diff(PortData.Obj(want["nodes"])[k], PortData.Obj(got["nodes"])[k], "duel.nodes." + k));
            foreach (string k in new[] { "linkStart", "linkTo", "linkKind", "linkCost" }) d.AddRange(Diff(PortData.Obj(want["links"])[k], PortData.Obj(got["links"])[k], "duel.links." + k));
            if (d.Count > 0) bad++;
            log($"navgrid spire-duel (core {preset}): {Counts(got)}: {Verdict(d)} (Build {ms:F0} ms)");
        }
        catch (Exception e)
        {
            bad++;
            log($"navgrid spire-duel: FAIL {e.GetType().Name}: {e.Message}");
        }
        log($"navgrid total: {bad} failing");
        return bad;
    }
}
