// NIGHT49 builder L (design.n49.lights49, a VIEW row; G2-G4, the light side), headless. Hooked without a Program.cs line: a module initializer runs
// the command and exits when argv[1] is "n49lights" (Harness.N48.Loom.cs's head).
//   n49lights [level ...] [--why]
//     For each campaign level (default: the five in play, n47-foundry / -iceline / -kiln / -span / -spire, and n48-foundry), the GREEN plan as the views
//     build it (look30, lights35 and rods40 as the config has them), then LightPlan33.Roles49 on it, and prints per level: the usable surfaces it read
//     (pads, grapple nodes, ride panels, climb steps, fall edges), the campaign.lights rows by their json colour, their DRAWN colour before (lights49
//     off) and after (on), the roles, the off-role rows before and after (cyan on a non-usable row; white on a non-boundary row), the usable rows per
//     room, the landmark and the blind sheet's room. --why lists every row. Exit 1 when any level keeps an off-role row after.
//   It also checks that the COPY below of UnityTuning.L49.cs still matches that file (the harness compiles no render file).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Skygrind.Core;
using Skygrind.Game;

namespace Skygrind.Game
{
    // COPIED from Assets/Scripts/Game/UnityTuning.L49.cs (LightPlan33's lights49 code reads them); `n49lights` fails when they drift. Change them together.
    public static partial class UnityTuning
    {
        public const double L49UsableReachM = 1.5;
        public const double L49BoundaryReachM = 1.5;
        public const double L49BloomThreshold = Look35BloomThreshold;
        public const double L49TierUsable = 2.0;
        public const double L49TierBoundary = 1.15;
        public const double L49TierDanger = 2.0;
        public const double L49TierDim = 0.9;
        public const double L49PaleSat = 0.15;
        public const double L49LandmarkLiftM = 6;
        public const double L49LandmarkPropDropM = 2;
        public const double L49LandmarkRangeM = 30;
        public const double L49LandmarkOverRoom = 1.5;
        public const double L49LandmarkMinIntensity = 8, L49LandmarkMaxIntensity = 60;
        public const double L49LandmarkMinLiftM = 1.0;
        public const double L49LandmarkPassRatio = 1.2;
        public const double L49FallMinDepthM = 1.0;
    }
}

public static partial class Harness
{
    [ModuleInitializer]
    internal static void L49LightsInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n49lights") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            code = L49Lights(args);
        }
        catch (Exception e) { Console.WriteLine("n49lights: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    /// <summary>The copy above vs the file: every `L49Name = value` of UnityTuning.L49.cs has the same value here.</summary>
    static int L49CopyCheck()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Scripts", "Game", "UnityTuning.L49.cs");
        if (!File.Exists(path)) { Console.WriteLine("copy check: " + path + " MISSING"); return 1; }
        string text = File.ReadAllText(path);
        int seen = 0, bad = 0;
        foreach (Match m in Regex.Matches(text, @"\b(L49\w+)\s*=\s*([0-9.]+|Look35BloomThreshold)\s*[,;]"))
        {
            seen++;
            string name = m.Groups[1].Value, val = m.Groups[2].Value;
            double want = val == "Look35BloomThreshold" ? UnityTuning.Look35BloomThreshold : double.Parse(val, CultureInfo.InvariantCulture);
            var f = typeof(UnityTuning).GetField(name);
            double have = f == null ? double.NaN : Convert.ToDouble(f.GetValue(null), CultureInfo.InvariantCulture);
            if (f == null || Math.Abs(have - want) > 1e-12) { bad++; Console.WriteLine($"copy check: {name} file {want} harness {(f == null ? "MISSING" : have.ToString(CultureInfo.InvariantCulture))}"); }
        }
        Console.WriteLine($"copy check (UnityTuning.L49.cs vs the harness's copy): {seen} numbers, {(bad == 0 && seen > 0 ? "MATCH" : bad + " DRIFT")}");
        return bad == 0 && seen > 0 ? 0 : 1;
    }

    static string L49D(SortedDictionary<string, int> d) => string.Join(", ", d.Select(kv => kv.Key + " " + kv.Value));

    static int L49Lights(string[] args)
    {
        int code = L49CopyCheck();
        GameConfig cfg = Config();
        bool why = args.Contains("--why");
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "Design", "levels");
        var ids = args.Skip(1).Where(a => !a.StartsWith("--")).ToList();
        if (ids.Count == 0) ids = new List<string> { "n47-foundry", "n47-iceline", "n47-kiln", "n47-span", "n47-spire", "n48-foundry" };
        bool look30 = cfg.Design("n30.look30.enabled"), lights35 = cfg.Design("n35.lights35.enabled");
        Console.WriteLine($"n49lights: the lights in their roles (usable reach {UnityTuning.L49UsableReachM} m, boundary reach {UnityTuning.L49BoundaryReachM} m; look30 {look30}, lights35 {lights35}, rods40 {Rods40.On(cfg)}; lights49 in the config {cfg.Design("n49.lights49.enabled")})");
        Console.WriteLine($"  tiers (x the bloom threshold {UnityTuning.L49BloomThreshold}): usable {LightPlan33.Hdr49("cyan"):0.00}, boundary {LightPlan33.Hdr49("white"):0.00}, danger {LightPlan33.Hdr49("orange"):0.00}, accent {LightPlan33.Hdr49("level49"):0.00} (HDR)");
        int offAll = 0;
        var tot = new Dictionary<string, int>();
        foreach (string id in ids)
        {
            string f = id.EndsWith(".json") ? id : Path.Combine(dir, id + ".json");
            if (!File.Exists(f)) { Console.WriteLine(id + ": missing"); code = 1; continue; }
            var json = PortData.Obj(JsJson.Parse(File.ReadAllText(f)));
            LightPlan33.Roles49Report r = LightPlan33.Report49(json, cfg, look30, lights35, Path.GetFileNameWithoutExtension(f));
            if (r == null) { Console.WriteLine(id + ": not a campaign level"); continue; }
            int off = r.offCyan + r.offWhite;
            offAll += off;
            Console.WriteLine($"{r.level}: {r.rows} rows; USABLE {r.rowsUsable}, boundary {r.rowsBoundary}, danger {r.rowsDanger}, accent {r.rowsAccent} (undrawn {r.rowsUndrawn})");
            Console.WriteLine($"   surfaces: pads {r.surf.pads}, grapple nodes {r.surf.nodes}, ride panels {r.surf.ride}, climb steps {r.surf.climb}, fall edges {r.surf.edges.Count}");
            Console.WriteLine($"   json colours   [{L49D(r.json)}]");
            Console.WriteLine($"   drawn before   [{L49D(r.before)}]   off-role: cyan on a non-usable row {r.offCyanBefore}, white on a non-boundary row {r.offWhiteBefore}");
            Console.WriteLine($"   drawn after    [{L49D(r.after)}]   off-role: {r.offCyan} / {r.offWhite}  {(off == 0 ? "PASS" : "RED")}");
            Console.WriteLine($"   plan lenses after: usable {r.fixUsable}, exit lamps {r.exits}, boundary {r.fixBoundary}, danger {r.fixDanger}, accent {r.fixAccent}");
            Console.WriteLine($"   usable rows by room: [{L49D(r.usableByRoom)}]; sheet room {r.sheetRoom}; landmark {r.landmark} in {r.landmarkRoom} (target {r.tx:0.0},{r.ty:0.0},{r.tz:0.0}; light {r.lx:0.0},{r.ly:0.0},{r.lz:0.0})");
            if (why) foreach (string w in r.rowLines) Console.WriteLine("     " + w);
            tot[r.level] = r.rowsUsable;
        }
        Console.WriteLine(offAll == 0 ? "SUMMARY PASS: no off-role row after (cyan only on usable rows, white only on boundary rows)" : $"SUMMARY RED: {offAll} off-role rows after");
        if (offAll > 0) code = 1;
        return code;
    }
}
