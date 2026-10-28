// NIGHT65 builder T (T2): `n65tour`, the headless twin of Assets/Tests/EditMode/Game/N65TourTests.cs and the dry run of the reel tour65
// (FeelReel.N65.Tour.cs) on a level file, no Player: for each room the reel would tour (the fight rooms, and any room whose row has frames65),
// the poses it would frame (frames65, or the fallback frames36.mid), their WARN verdicts (a pose inside a collision box) and the line65 frames;
// then the planted pose and the parsing cases. One line per pose, as the reel's proof line without the frame and the live wave.
//   n65tour --level n52-foundry [--plant a2:-4,0,-52.5,0,0]
// Exit 1 when a fight room has no pose. Probe numbers only: not gameplay.
// NIGHT66 T (T1a, S3): the verdict is the reel's own rule (Tour65.Green on Tour65.Dry: unframed 0, WARN 0, wedges in the solids), and the
// planted cases (T66TourCases) print RED once and GREEN once fixed for an unframed room and for a pose inside a slide wedge.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void T65TourInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n65tour") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try { code = T65Tour(args); }
        catch (Exception e) { Console.WriteLine("n65tour: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static int T65Tour(string[] args)
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        string id = T65Arg(args, "--level", "n52-foundry");
        string plantArg = T65Arg(args, "--plant", null);
        string path = Path.GetFullPath(id.EndsWith(".json", StringComparison.Ordinal) ? id : Path.Combine("Design", "levels", id + ".json")); // NIGHT66 T1a: a path too (a round's Logs/n66/<tag>/n65-foundry.json)
        var json = PortData.Obj(PortData.LoadJson(path));
        var plant = plantArg != null ? Tour65.Plant(new[] { "-skygrindTourPlant65=" + plantArg }) : null; // the reel's one-token form
        List<double[]> solids = Tour65.Solids(json);
        Console.WriteLine($"n65tour: level {id} ({path}), {solids.Count} solids ({solids.Count(s => s.Length == 6)} collision boxes, {solids.Count(s => s.Length == 7)} wedges); planted {(plantArg ?? "none")}{(plantArg != null && plant == null ? " (UNPARSED)" : "")}");
        int code = 0, rooms = 0, poses = 0, warns = 0, lineRooms = 0;
        var fallback = new List<string>();
        Func<double, string> f1 = v => v.ToString("0.0", ci);
        foreach (object o in PortData.Arr(PortData.Obj(json["campaign"])["rooms"]))
        {
            var row = PortData.Obj(o);
            string rid = (string)row["id"], kind = row.TryGetValue("kind", out object k) ? (string)k : "room";
            List<Tour65.Pose> list = Tour65.Poses(row, out string source);
            bool planted = plant.HasValue && plant.Value.room == rid;
            if (planted) list.Add(plant.Value.pose);
            if (kind != "room" && source != "frames65" && !planted) continue;
            rooms++;
            if (list.Count == 0) { Console.WriteLine($"n65tour {rid}: NO POSE (no frames65, no frames36.mid): RED"); code = 1; continue; }
            if (source == "frames36.mid") fallback.Add(rid);
            List<Tour65.Pose> lf = Tour65.LineFrames(Tour65.Line(row), UnityTuning.T65LineFrames);
            foreach (Tour65.Pose p in list.Concat(lf))
            {
                string inside = Tour65.InsideSolid(solids, p);
                if (inside != null) warns++;
                if (p.source != "line65") poses++;
                string what = p.source == "line65" ? $"line65 {p.name.Substring(4)}/{lf.Count}" : $"pose {p.name} ({p.source})";
                Console.WriteLine($"n65tour {rid} {what} | feet ({f1(p.x)}, {f1(p.y)}, {f1(p.z)}) yaw {p.yawDeg.ToString("0", ci)} pitch {p.pitch.ToString("0.00", ci)}"
                    + (inside != null ? $" | WARN inside a solid ({inside}): the camera in a wall" : ""));
            }
            if (lf.Count > 0) lineRooms++;
            else Console.WriteLine($"n65tour {rid} line65: none in the row (no sequence)");
        }
        // the parsing cases the EditMode test checks (the same calls)
        var f = Tour65.LineFrames(new List<double[]> { new double[] { 0, 0, 0 }, new double[] { 0, 0, -10 }, new double[] { 10, 0, -10 } }, UnityTuning.T65LineFrames);
        bool lineOk = f.Count == 8 && Math.Abs(f[7].x - 10) < 1e-9 && Math.Abs(f[7].z + 10) < 1e-9 && Math.Abs(f[0].yawDeg) < 1e-9 && Math.Abs(f[7].yawDeg + 90) < 1e-9;
        var badPlant = Tour65.Plant(new[] { "-skygrindTourPlant65", "a2:1,x,3" });
        var token = Tour65.Plant(new[] { "-skygrindReelOnly", "tour65", "-skygrindTourPlant65=a8:-4/0/-352.5/180/-0.1" });
        bool tokenOk = token.HasValue && token.Value.room == "a8" && token.Value.pose.z == -352.5 && token.Value.pose.yawDeg == 180;
        Console.WriteLine($"n65tour cases: an L-shaped line65 -> {f.Count} frames, first yaw {f[0].yawDeg.ToString("0", ci)}, last ({f1(f[7].x)}, {f1(f[7].z)}) yaw {f[7].yawDeg.ToString("0", ci)}: {(lineOk ? "PASS" : "FAIL")}; a bad plant 'a2:1,x,3' parses to {(badPlant.HasValue ? "a pose (FAIL)" : "nothing (PASS)")}; the one-token '-skygrindTourPlant65=a8:-4/0/-352.5/180/-0.1': {(tokenOk ? "PASS" : "FAIL")}");
        if (!lineOk || badPlant.HasValue || !tokenOk) code = 1;
        // NIGHT66 T1a (S3): the reel's GREEN rule (Tour65.Green) on the level file's half, then its planted cases, each RED once and GREEN once fixed
        Tour65.DryTour dry = Tour65.Dry(json, plant);
        bool casesOk = T66TourCases();
        if (!casesOk) code = 1;
        if (!dry.Green) code = 1;
        Console.WriteLine($"n65tour {id}: rooms toured {dry.rooms} (rows {rooms}), poses {dry.poses} (on the fallback frames36.mid: {(dry.fallback.Count > 0 ? string.Join(", ", dry.fallback) : "none")}), line65 in {lineRooms} room(s), WARN {dry.warns}{(dry.warned.Count > 0 ? " (" + string.Join(", ", dry.warned) + ")" : "")}, unframed {(dry.unframed.Count > 0 ? string.Join(", ", dry.unframed) : "none")}; tour65 rule (Tour65.Green, S3) {(dry.Green ? "GREEN" : "RED")}; cases {(casesOk ? "PASS" : "FAIL")}; {(code == 0 ? "GREEN" : "RED")}");
        return code;
    }

    /// <summary>NIGHT66 T1a: tour65's GREEN rule on planted level files: a fight room with no pose (unframed) reads RED, then GREEN once it has a
    /// pose; a pose inside a slide wedge reads RED (Solids reads the wedges), then GREEN once it stands on the ramp; the old rule (`framed == poses`,
    /// no unframed / WARN term) read GREEN on both REDs, printed beside them.</summary>
    static bool T66TourCases()
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        Func<string, Dictionary<string, object>> J = t => PortData.Obj(JsJson.Parse(t));
        const string floor = "\"boxes\":[{\"min\":[-50,-1,-50],\"max\":[50,0,50],\"kind\":\"floor\"}]";
        const string wedge = "\"wedges\":[{\"x\":0,\"z\":0,\"yaw\":0,\"length\":8,\"width\":4,\"rise\":2.9}]";
        // case 1: room p2 (a fight room) has no pose at all
        var unframed = J("{\"collision\":{" + floor + "},\"campaign\":{\"rooms\":[{\"id\":\"p1\",\"kind\":\"room\",\"frames65\":{\"entry\":[10,0,10,0,0]}},{\"id\":\"p2\",\"kind\":\"room\"}]}}");
        var framed = J("{\"collision\":{" + floor + "},\"campaign\":{\"rooms\":[{\"id\":\"p1\",\"kind\":\"room\",\"frames65\":{\"entry\":[10,0,10,0,0]}},{\"id\":\"p2\",\"kind\":\"room\",\"frames65\":{\"entry\":[20,0,20,0,0]}}]}}");
        // case 2: the pose's feet 6 m up a 2.9 m / 8 m ramp rising toward -z (its top there 2.175 m): inside at y 0, on it at y 2.2
        var inWedge = J("{\"collision\":{" + floor + "," + wedge + "},\"campaign\":{\"rooms\":[{\"id\":\"p1\",\"kind\":\"room\",\"frames65\":{\"ramp\":[0,0,-6,0,0]}}]}}");
        var onWedge = J("{\"collision\":{" + floor + "," + wedge + "},\"campaign\":{\"rooms\":[{\"id\":\"p1\",\"kind\":\"room\",\"frames65\":{\"ramp\":[0,2.2,-6,0,0]}}]}}");
        Tour65.DryTour a = Tour65.Dry(unframed, null), b = Tour65.Dry(framed, null), c = Tour65.Dry(inWedge, null), d = Tour65.Dry(onWedge, null);
        // the old rule (FeelReel.N65.Tour.cs:124 at the t0 sha): rooms > 0 && framed == poses (always true) && the live terms; the boxes-only Solids
        Func<Tour65.DryTour, bool> old = t => t.rooms > 0;
        int boxesOnly = 0;
        foreach (double[] s in Tour65.Solids(inWedge)) if (s.Length == 6) boxesOnly++;
        bool ok = !a.Green && b.Green && !c.Green && d.Green && a.unframed.Count == 1 && a.unframed[0] == "p2" && c.warns == 1 && d.warns == 0;
        Console.WriteLine($"n65tour case unframed (p2 has no pose): unframed {string.Join(", ", a.unframed)}, WARN {a.warns} -> {(a.Green ? "GREEN" : "RED")} (the old rule: {(old(a) ? "GREEN" : "RED")}); fixed (p2 given a pose): unframed {b.unframed.Count}, WARN {b.warns} -> {(b.Green ? "GREEN" : "RED")}");
        Console.WriteLine($"n65tour case wedge (a pose 6 m up a 2.9 m / 8 m ramp, feet at y 0): solids {Tour65.Solids(inWedge).Count} ({boxesOnly} box, {Tour65.Solids(inWedge).Count - boxesOnly} wedge), WARN {c.warns} ({string.Join(", ", c.warned)}) -> {(c.Green ? "GREEN" : "RED")} (the old rule, boxes only: {(old(c) ? "GREEN" : "RED")}); fixed (feet on the ramp at y 2.2, its top {(2.9 * 6 / 8).ToString("0.000", ci)}): WARN {d.warns} -> {(d.Green ? "GREEN" : "RED")}");
        Console.WriteLine($"n65tour cases (T1a): {(ok ? "PASS" : "FAIL")}");
        return ok;
    }
}
