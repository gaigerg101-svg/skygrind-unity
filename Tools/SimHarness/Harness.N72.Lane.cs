// NIGHT72 builder N, slice N1 (verbs37 row 12, the bunny hop, RED "2 hops" on n52-iceline and n57-kiln since ultra71): the clear lane the reel stages its ground verbs on.
// `n72lane [--level id[,id...]] [--maxclear 60]` prints, per level, what FeelReel.N35.Verbs.cs Open35 picks (the first room from the first award room whose longest
// lane is at least 14 m, rays up to 30 m, the floor under the lane every 2 m) and the longest lane any room has when the rays may go farther, so the hop row's need
// (three jumps at the profile's air time) can be set against what each level offers. The scan is Open35's, copied (FeelReel is not in this project).
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
    internal static void N72LaneInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n72lane") return;
        int code;
        try { code = N72Lane(argv.Skip(1).ToArray()); }
        catch (Exception e) { Console.WriteLine("n72lane: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static double N72Fwd(double yaw, out double fx, out double fz) { fx = -Math.Sin(yaw); fz = -Math.Cos(yaw); return yaw; }

    /// <summary>Open35's scan for one room: the longest lane over 16 headings from the room's spawn (rays at 0.5 m and 1.5 m, at most maxClear; the floor under it every 2 m).</summary>
    static (double len, double yaw) N72RoomLane(SurvivalSim sim, RoomDef r, double maxClear)
    {
        Vec3 o = r.spawn;
        double best = 0, bestYaw = 0;
        for (int k = 0; k < 16; k++)
        {
            double yaw = k * Math.PI / 8;
            N72Fwd(yaw, out double fx, out double fz);
            double clear = maxClear;
            foreach (double h in new[] { 0.5, 1.5 })
                if (sim.world.RayIntersect(new Vec3(o.x, o.y + h, o.z), new Vec3(fx, 0, fz), out RayHit hit)) clear = Math.Min(clear, hit.distance);
            double len = 0;
            for (double d = 2; d <= clear - 1.5; d += 2)
            {
                var p = new Vec3(o.x + fx * d, o.y + 1.0, o.z + fz * d);
                if (!sim.world.RayIntersect(p, new Vec3(0, -1, 0), out RayHit dh) || dh.distance > 1.6) break;
                len = d;
            }
            if (len > best) { best = len; bestYaw = yaw; }
        }
        return (best, bestYaw);
    }

    static int N72Lane(string[] args)
    {
        var ci = CultureInfo.InvariantCulture;
        string levels = "n52-foundry,n52-iceline,n57-kiln,n52-span,n52-spire,n65-foundry,n67-iceline,n67-kiln,n67-spire";
        double maxClear = 60;
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--level" && i + 1 < args.Length) levels = args[++i];
            else if (args[i] == "--maxclear" && i + 1 < args.Length) maxClear = double.Parse(args[++i], ci);
        }
        GameConfig cfg = Config();
        foreach (string id in levels.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
            if (!File.Exists(path)) { Console.WriteLine("n72lane " + id + ": no level file"); continue; }
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            LevelDef level = sim.campaignLevel ?? LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
            // Open35's pick: from the first award room on, the first room whose longest lane (rays to 30 m) is >= 14 m, else the longest below it (>= 8 or the roomiest)
            // the Player's FirstAwardRoom57 is filled when the campaign starts (design.n40.<row>.awards of the level's row); a headless sim has not started one, so the
            // rooms any design.n40 row awards in, taken in the level's room order, stand in for it (the level's own row is the one that applies in the Player)
            var awardIds = new HashSet<string>();
            if (cfg.Has("design.n40"))
                foreach (KeyValuePair<string, object> row in cfg.Obj("design.n40"))
                    if (row.Value is Dictionary<string, object> rd && rd.TryGetValue("awards", out object aw) && aw is List<object> al)
                        foreach (object ao in al)
                            if (ao is Dictionary<string, object> ad && ad.TryGetValue("room", out object rr) && rr is string rs) awardIds.Add(rs);
            string firstAward = level.rooms.Select(x => x.id).FirstOrDefault(x => awardIds.Contains(x)) ?? sim.FirstAwardRoom57;
            bool reached = firstAward == null || level.rooms.FindIndex(x => x.id == firstAward) < 0;
            double pickLen = 0, pickYaw = 0;
            string pickRoom = null;
            foreach (RoomDef r in level.rooms)
            {
                if (!reached && r.id == firstAward) reached = true;
                if (!reached || r.kind == "boss") continue;
                var (len, yaw) = N72RoomLane(sim, r, 30);
                if (len > pickLen) { pickLen = len; pickYaw = yaw; pickRoom = r.id; }
                if (pickLen >= 14) break;
            }
            // the longest lane any non-boss room has (rays to maxClear)
            double longest = 0;
            string longRoom = null;
            var per = new List<string>();
            foreach (RoomDef r in level.rooms)
            {
                if (r.kind == "boss") continue;
                var (len, yaw) = N72RoomLane(sim, r, maxClear);
                per.Add(r.id + " " + len.ToString("0", ci));
                if (len > longest) { longest = len; longRoom = r.id; }
            }
            Console.WriteLine($"n72lane {id}: Open35 picks {pickRoom} lane {pickLen.ToString("0", ci)} m (first award room {firstAward ?? "none"}); the longest lane (rays to {maxClear.ToString("0", ci)} m) {longRoom} {longest.ToString("0", ci)} m; per room: {string.Join(", ", per)}");
            // N1: the row's press logic on the bare Mover over this world, at Open35's pick (HopSite72.Dry), under the web's movement and the ULTRAKILL profile
            RoomDef pr = level.rooms.Find(x => x.id == pickRoom);
            if (pr == null) { Console.WriteLine($"  hop dry run: no pick"); continue; }
            var openAt = new Vec3(pr.spawn.x, pr.spawn.y + 0.2, pr.spawn.z);
            var spawns = new List<Vec3>();
            bool seen = firstAward == null || level.rooms.FindIndex(x => x.id == firstAward) < 0;
            foreach (RoomDef r in level.rooms)
            {
                if (!seen && r.id == firstAward) seen = true;
                if (seen && r.kind != "boss") spawns.Add(r.spawn);
            }
            var pnotes = new List<string>();
            MovementConfig webMc = N72Profile(false, null, pnotes), ultraMc = N72Profile(true, null, pnotes);
            string Cell(HopSite72.Run r) => r.hops + " hops, kept " + r.minKept.ToString("0.0", ci) + " %, " + r.seconds.ToString("0.0", ci) + " s" + (r.Pass ? " PASS" : " RED");
            HopSite72.Run rWeb = HopSite72.Dry(webMc, sim.world, openAt, pickYaw, HopSite72.Window(webMc));
            HopSite72.Run rOld = HopSite72.Dry(ultraMc, sim.world, openAt, pickYaw, 4.0);
            HopSite72.Run rWin = HopSite72.Dry(ultraMc, sim.world, openAt, pickYaw, HopSite72.Window(ultraMc));
            Console.WriteLine($"  hop dry run (bare Mover, indicative only) at Open35's pick: web [{Cell(rWeb)}] | ultra, the old 4 s window [{Cell(rOld)}] | ultra, window {HopSite72.Window(ultraMc).ToString("0.0", ci)} s [{Cell(rWin)}]");
            var cands = HopSite72.Candidates(sim.world, spawns, openAt, pickYaw);
            var spawnRooms = level.rooms.Where(x => x.kind != "boss").SkipWhile(x => firstAward != null && x.id != firstAward && level.rooms.FindIndex(q => q.id == firstAward) >= 0).ToList();
            double wUltra = HopSite72.Window(ultraMc);
            Console.WriteLine("  retry places (longest lane first): " + string.Join(" | ", cands.Select(c => (c.room < spawnRooms.Count ? spawnRooms[c.room].id : "?") + " " + (c.yaw * 180 / Math.PI).ToString("0", ci) + " deg lane " + c.lane.ToString("0", ci) + " m dry [" + Cell(HopSite72.Dry(ultraMc, sim.world, c.at, c.yaw, wUltra)) + "]")));
        }
        return 0;
    }
}
