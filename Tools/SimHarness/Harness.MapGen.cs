// NIGHT15 U3-28.6 (builder G): a map built from its plan without the Editor (the Editor stalls while GG uses the machine;
// the map must exist either way). The conversion is Assets/Editor/Maps/Build_drydock.cs's DrydockPlan (pure), compiled here
// with builder P's UnityEngine-free map pipeline (MapModel, MapModelJson, MapJsonWriter, MapRoundTrip, MapValidate,
// UnityTuning.MapAuthoring), so both ways write the same text. From the repo root:
//   mapgen <plan.json> <out.json>        DRYDOCK's plan -> the map JSON (navGrid / zoneParts by NavGridBuilder.ExportFor);
//                                        out = Design/maps/<id>.json writes it and its byte-identical Player copy
//                                        (MapModelJson.WriteFiles), any other path just that file. Also prints the plan
//                                        checks (DrydockPlan.Check) and the model round trip (the Editor's markers' path).
//   mapvalidate <map.json> <report.md>   MapValidate.Run on a map file -> the report (exit 0: no findings).
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Skygrind.Core;
using Skygrind.EditorTools;
using Skygrind.Game;
using Skygrind.Game.MapAuthoring;

public static partial class Harness
{
    static partial void MapGen(string cmd, string[] args, ref int code)
    {
        if (cmd == "mapgen") code = MapGenBuild(args);
        else if (cmd == "mapvalidate") code = MapGenValidate(args);
    }

    /// <summary>The repo root (PortExport's parent: the harness runs from it).</summary>
    static string MapGenRepo => Path.GetDirectoryName(Root);

    static string MapGenRelative(string path)
    {
        string full = Path.GetFullPath(path).Replace('\\', '/'), repo = Path.GetFullPath(MapGenRepo).Replace('\\', '/') + "/";
        return full.StartsWith(repo, StringComparison.OrdinalIgnoreCase) ? full.Substring(repo.Length) : full;
    }

    static int MapGenBuild(string[] args)
    {
        if (args.Length < 3) throw new ArgumentException("usage: mapgen <plan.json> <out.json>");
        var t = System.Diagnostics.Stopwatch.StartNew();
        GameConfig cfg = Config();
        var plan = PortData.Obj(PortData.LoadJson(Path.GetFullPath(args[1])));
        DrydockPlan.DrydockBuild b = DrydockPlan.Build(plan, cfg);
        List<string> problems = DrydockPlan.Check(b, cfg);
        int roundTrip = DrydockPlan.ModelRoundTrip(b, cfg, out List<string> fromJsonWarnings);

        string outRel = MapGenRelative(args[2]);
        string written;
        bool copySame = true;
        if (outRel == $"{UnityTuning.MapDesignDir}/{b.model.id}.json")
        {
            (string design, string player) = MapModelJson.WriteFiles(MapGenRepo, b.model.id, b.text);
            copySame = File.ReadAllBytes(design).AsSpan().SequenceEqual(File.ReadAllBytes(player));
            written = $"{MapGenRelative(design)} and {MapGenRelative(player)} (byte-identical: {(copySame ? "yes" : "NO")})";
        }
        else
        {
            MapModelJson.WriteTo(Path.GetFullPath(args[2]), b.text);
            written = outRel;
        }

        var navGrid = (Dictionary<string, object>)b.json["navGrid"];
        var zoneParts = (Dictionary<string, object>)b.json["zoneParts"];
        MapModel m = b.model;
        Console.WriteLine($"mapgen {m.id}: {b.text.Length} chars -> {written}; {m.boxes.Count} boxes + the floor, {m.barriers.Count} barriers, {m.wedges.Count} wedges, {m.pads.Count} pads, {m.grappleNodes.Count} grapple nodes, "
            + $"{m.zones.Count} zones ({string.Join(", ", m.zones.ConvertAll(z => $"{z.id} {z.cost} with {z.spots.Count} buy spots"))}), {((List<object>)zoneParts["parts"]).Count} zone parts, nav grid {navGrid.Count} keys; "
            + $"{m.perkSpots.Count} perks, {b.wallBuys.Count} wall guns, {m.boxSpots.Count} box spots, {m.holdSpots.Count} hold rings, {m.relaySpots.Count} relays + {(m.hasSecretRelay ? 1 : 0)} secret, "
            + $"{m.groundSpawns.Count} ground + {m.highSpawns.Count} high spawns ({t.Elapsed.TotalSeconds:0.0} s)");
        foreach (string n in b.notes) Console.WriteLine($"  note: {n}");
        Console.WriteLine($"  model round trip (FromJson -> Export, the Editor's markers' path): {(roundTrip < 0 ? "SAME" : $"DIFFERENT at char {roundTrip}")}; FromJson warnings {fromJsonWarnings.Count}");
        foreach (string w in fromJsonWarnings) Console.WriteLine($"    {w}");
        Console.WriteLine($"  plan checks: {problems.Count} problems");
        foreach (string p in problems) Console.WriteLine($"    {p}");
        // What the game makes of the spots (a headless composition on the written text): the wall guns and perk machines
        // that exist in the port (SurvivalSim.AddWallBuys skips a gun Loadout.Fires refuses; AddMachines a perk not in Perks.Ported).
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(JsJson.Parse(b.text))));
        var guns = sim.wallGuns.ConvertAll(g => g.id);
        var perks = sim.machines.ConvertAll(x => x.spot.perk).FindAll(p => p != "forge");
        bool forgeStands = sim.machines.Exists(x => x.spot.perk == "forge");
        Console.WriteLine($"  in a headless composition: {guns.Count} of {b.wallBuys.Count} wall guns hang ({string.Join(", ", guns)}), {perks.Count} of {m.perkSpots.Count} perk machines stand ({string.Join(", ", perks)}), THE FORGE {(forgeStands ? "stands" : "missing")}; the others are not ported yet");
        return roundTrip < 0 && copySame ? 0 : 1;
    }

    static int MapGenValidate(string[] args)
    {
        if (args.Length < 3) throw new ArgumentException("usage: mapvalidate <map.json> <report.md>");
        var t = System.Diagnostics.Stopwatch.StartNew();
        var json = PortData.Obj(PortData.LoadJson(Path.GetFullPath(args[1])));
        (string md, int findings) = MapValidate.Run(Config(), json, MapGenRelative(args[1]));
        string path = Path.GetFullPath(args[2]);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, md, new UTF8Encoding(false));
        string counts = "";
        var verbs = new List<string>();
        var finds = new List<string>();
        foreach (string line in md.Split('\n'))
        {
            if (line.StartsWith("**findings:")) counts = line;
            if (line.Contains("| VERB |")) verbs.Add(line.Split('|')[1].Trim());
            if (line.Contains("| FINDING |")) finds.Add(line.Split('|')[1].Trim());
        }
        Console.WriteLine($"mapvalidate {MapGenRelative(args[1])}: {counts} -> {MapGenRelative(args[2])} ({t.Elapsed.TotalSeconds:0.0} s)");
        if (verbs.Count > 0) Console.WriteLine($"  VERB rows: {string.Join("; ", verbs)}");
        if (finds.Count > 0) Console.WriteLine($"  FINDING rows: {string.Join("; ", finds)}");
        return findings == 0 ? 0 : 1;
    }
}
