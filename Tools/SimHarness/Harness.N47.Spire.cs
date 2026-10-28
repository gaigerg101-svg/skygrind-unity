// NIGHT47 builder L (NIGHT47.md G42 "level 5 makes sense": the goal visible from the start and from each room; L-cards.md section 4): the route meter.
// Hooked without a Program.cs line: a module initializer runs the command and exits when argv[1] is "n47spire" (Harness.N46.Flow.cs's head).
//   n47spire view [--level <id>] [--x -10] [--z -289] [--bottom 10] [--top 300]
//     THE CROWN'S LIGHT (the level's wienie, L-cards.md section 4): a vertical column at (x, z) (default the v11 core's axis, read from
//     Design/levels/n46/spire.plan.json's spire37 core: x -14..-6, z -293..-285) from y --bottom (10) to --top (300). From the eye (1.6 m) at each room's
//     spawn (v1's is the level's start; v2..v10's stand just inside their entry doors, read from the level), looking ahead (-z): the room SEES
//     the light when at least one column point (every 10 m) is inside the view (bearing <= 53 deg, |up| <= 37 deg: FLOW-AUDIT's frame, as
//     n46span view) and the world's collider ray to it is clear. v11 (open sky) is the checker's control: it must see the light, or the
//     checker is wrong (L-034). PASS (G42) when v1..v10 all see it; today's n46-spire is the RED.
//     Round 4 (SKYGRIND/Column47, Route47View): the column is also drawn where a roof or wall is in front of it (ZTest Greater, a ghost at
//     Route47ColumnThrough), so a second line counts the rooms where a column point is inside the view at all ("through the roofs").
//   n47spire route [--level <id>]
//     THE CROWN'S WINDOWS (gen-n47-spire.cjs: campaign.lights rows <room>-crown47*, on each room's exit wall): from the eye (1.6 m) at each room's
//     spawn (its entry), the room's window centre inside the view (bearing <= 53, |up| <= 37 deg) and the collider ray to it clear (the window
//     sits on the wall: the ray stops 0.3 m short). PASS when v1..v10 each see their window (n46-spire has none: RED).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void L47SpireInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n47spire") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "view";
            if (what == "view") code = L47SpireView(args);
            else if (what == "route") code = L47SpireRoute(args);
            else { Console.WriteLine("usage: n47spire view [--level id] [--x -10] [--z -289] [--bottom 10] [--top 300]"); code = 1; }
        }
        catch (Exception e) { Console.WriteLine("n47spire: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static int L47SpireRoute(string[] args)
    {
        string level = L47Arg(args, "--level", "n47-spire");
        var json = PortData.Obj(PortData.LoadJson("Design/levels/" + level + ".json"));
        LevelDef lv = LevelDef.From(json);
        SurvivalSim sim = SurvivalSim.Headless(Config(), SurvivalMap.FromExport(json));
        var wins = new Dictionary<string, List<Vec3>>();
        if (json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp && camp.TryGetValue("lights", out object lo) && lo is List<object> ls)
            foreach (object o in ls)
            {
                if (!(o is Dictionary<string, object> r) || !(r.TryGetValue("id", out object io) && io is string id) || id.IndexOf("-crown47", StringComparison.Ordinal) < 0) continue;
                var at = PortData.Arr(r["at"]);
                string room = r["room"] as string;
                if (!wins.ContainsKey(room)) wins[room] = new List<Vec3>();
                wins[room].Add(new Vec3(PortData.Num(at[0]), PortData.Num(at[1]), PortData.Num(at[2])));
            }
        int rooms = 0, seen = 0;
        foreach (RoomDef rd in lv.rooms)
        {
            if (rd.id == "v11") continue;
            rooms++;
            Vec3 eye = new Vec3(rd.spawn.x, rd.spawn.y + 1.6, rd.spawn.z);
            bool ok = false;
            string detail = "no window";
            if (wins.TryGetValue(rd.id, out List<Vec3> list))
                foreach (Vec3 t in list)
                {
                    Vec3 d = t - eye;
                    double flat = Math.Sqrt(d.x * d.x + d.z * d.z), len = d.Length;
                    double bearing = Math.Abs(Math.Atan2(d.x, -d.z)) * 180 / Math.PI, up = Math.Atan2(d.y, flat) * 180 / Math.PI;
                    bool inView = bearing <= 53 && Math.Abs(up) <= 37;
                    bool clear = !(sim.world.RayIntersect(eye, d / len, out RayHit hit) && hit.distance < len - 0.3);
                    detail = $"window ({t.x:0.0}, {t.y:0.0}, {t.z:0.0}) {len:0} m, bearing {bearing:0} deg, up {up:0} deg: {(inView ? "in view" : "OUT of view")}, {(clear ? "clear" : "BLOCKED")}";
                    if (inView && clear) { ok = true; break; }
                }
            if (ok) seen++;
            Console.WriteLine($"  {rd.id} {rd.name}: {(ok ? "SEES" : "no")}: {detail}");
        }
        bool pass = seen == rooms;
        Console.WriteLine($"n47spire route {level}: the way-on window seen from {seen} / {rooms} rooms' entries: {(pass ? "GREEN" : "RED")}");
        return pass ? 0 : 1;
    }

    static int L47SpireView(string[] args)
    {
        string level = L47Arg(args, "--level", "n46-spire");
        double cx = double.Parse(L47Arg(args, "--x", "-10"), L47Inv), cz = double.Parse(L47Arg(args, "--z", "-289"), L47Inv);
        double top = double.Parse(L47Arg(args, "--top", "300"), L47Inv), bottom = double.Parse(L47Arg(args, "--bottom", "10"), L47Inv);
        var json = PortData.Obj(PortData.LoadJson("Design/levels/" + level + ".json"));
        LevelDef lv = LevelDef.From(json);
        SurvivalSim sim = SurvivalSim.Headless(Config(), SurvivalMap.FromExport(json));
        int seenRooms = 0, rooms = 0, throughRooms = 0;
        bool control = false;
        foreach (RoomDef rd in lv.rooms)
        {
            Vec3 eye = new Vec3(rd.spawn.x, rd.spawn.y + 1.6, rd.spawn.z);
            int pts = 0, inView = 0, clear = 0;
            double lo = double.NaN, hi = double.NaN;
            for (double h = bottom; h <= top + 1e-9; h += 10)
            {
                Vec3 t = new Vec3(cx, h, cz);
                Vec3 d = t - eye;
                double flat = Math.Sqrt(d.x * d.x + d.z * d.z), len = d.Length;
                double bearing = Math.Abs(Math.Atan2(d.x, -d.z)) * 180 / Math.PI, up = Math.Atan2(d.y, flat) * 180 / Math.PI;
                pts++;
                if (bearing > 53 || Math.Abs(up) > 37) continue;
                inView++;
                bool ok = !(sim.world.RayIntersect(eye, d / len, out RayHit hit) && hit.distance < len - 0.6);
                if (!ok) continue;
                clear++;
                if (double.IsNaN(lo)) lo = h;
                hi = h;
            }
            bool sees = clear > 0;
            if (rd.id == "v11") control = sees;
            else { rooms++; if (sees) seenRooms++; if (inView > 0) throughRooms++; }
            Console.WriteLine($"  {rd.id} {rd.name}: eye ({eye.x:0.0}, {eye.y:0.0}, {eye.z:0.0}), {Vec3.DistanceTo(eye, new Vec3(cx, eye.y, cz)):0} m to the light: "
                + $"{(sees ? $"SEES it (y {lo:0}-{hi:0} clear, {clear} of {inView} points in view)" : $"no ({inView} of {pts} points in view, all blocked)")}{(rd.id == "v11" ? "  [the control]" : "")}");
        }
        bool pass = seenRooms == rooms;
        Console.WriteLine($"n47spire view {level}: through the roofs (Column47's ZTest Greater pass): a column point in view from {throughRooms} / {rooms} rooms");
        Console.WriteLine($"n47spire view {level}: the light seen from {seenRooms} / {rooms} rooms (v11 control: {(control ? "sees it" : "BLIND: the checker is wrong")}): {(pass && control ? "GREEN" : "RED")}");
        return pass && control ? 0 : 1;
    }
}
