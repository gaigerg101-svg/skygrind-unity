// NIGHT65 builder T (T1; redteam.md 5): `n65bot`, the headless twin of Assets/Tests/EditMode/Game/N65BotOptOutTests.cs (the lead runs that test
// in the Editor; this prints the same numbers from SimHarness, no Editor). For each room id given, the walk pilot (Walk38Pilot, n46flow's) is
// put at the room's spawn on the level (DOORFREE, n46flow's placement) and plays `--seconds` s, twice: the row as the file has it, and with the
// row's "bot43": false set in memory. One line per room and run: the steps on which Walk43Foundry's id script drove the move, whether the
// campaign took the room, the BotRows65 set. Hooked without a Program.cs line (a module initializer, n46flow's / hold52's pattern).
//   n65bot --level n52-foundry --rooms a8,b2,a3 [--seconds 12] [--seed 1]
// Probe numbers only: not gameplay.
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
    internal static void T65BotInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n65bot") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try { code = T65Bot(args); }
        catch (Exception e) { Console.WriteLine("n65bot: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static string T65Arg(string[] args, string name, string d)
    {
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : d;
    }

    static int T65Bot(string[] args)
    {
        string id = T65Arg(args, "--level", "n52-foundry");
        string[] rooms = T65Arg(args, "--rooms", "a8").Split(',');
        double seconds = double.Parse(T65Arg(args, "--seconds", "12"), CultureInfo.InvariantCulture);
        uint seed = uint.Parse(T65Arg(args, "--seed", "1"), CultureInfo.InvariantCulture);
        string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
        Console.WriteLine($"n65bot: level {id} ({path}), rooms {string.Join(",", rooms)}, {seconds:0} s each, seed {seed}; pilot Walk38Pilot + the ported autopilot (normal)");
        int code = 0;
        foreach (string room in rooms)
        {
            var a = T65Run(path, room, false, seconds, seed);
            var b = T65Run(path, room, true, seconds, seed);
            Console.WriteLine($"n65bot {id} {room}: row as filed: id-script steps {a.scripted}, entered {a.entered}, BotRows65 [{a.off}]; with bot43 false: id-script steps {b.scripted}, entered {b.entered}, BotRows65 [{b.off}]");
            if (!a.entered || !b.entered || b.scripted != 0) code = 1;
        }
        Console.WriteLine("n65bot: " + (code == 0 ? "every opted-out room ran 0 id-script steps (entered in both runs)" : "RED (a room not entered, or an id script ran with bot43 false)"));
        return code;
    }

    static (int scripted, bool entered, string off) T65Run(string path, string room, bool optOut, double seconds, uint seed)
    {
        var json = PortData.Obj(PortData.LoadJson(path));
        if (optOut)
            foreach (object o in PortData.Arr(PortData.Obj(json["campaign"])["rooms"]))
            {
                var r = PortData.Obj(o);
                if ((string)r["id"] == room) r["bot43"] = false;
            }
        SurvivalMap map = SurvivalMap.FromExport(json);
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        SurvivalSim sim = SurvivalSim.Headless(Config(), map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var ap = new AutopilotInput(sim, "normal", seed);
        int at = -1;
        ap.GoalProvider = () => sim.campaignLevel.rooms[at < 0 ? 0 : at].spawn;
        var pilot = new Walk38Pilot(sim, ap);
        sim.UseInput(pilot);
        sim.StartRun(seed, null, null);
        at = sim.campaignLevel.rooms.FindIndex(r => r.id == room);
        if (at < 0) throw new Exception("no room " + room);
        RoomDef rd = sim.campaignLevel.rooms[at];
        bool placed = false, entered = false;
        int steps = MovementMath.StepsFor(seconds, sim.step);
        for (int s = 0; s < steps; s++)
        {
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            Campaign24 c = sim.campaign;
            if (c == null || c.room < 0) continue;
            if (!placed) { c.SetDoorFree29(true); sim.Respawn(rd.spawn, rd.spawnYawDeg); placed = true; continue; }
            if (c.room == at && !entered) { entered = true; c.SetDoorFree29(false); }
        }
        ap.Detach();
        return (pilot.idScriptSteps65, entered, string.Join(",", BotRows65.OffRooms(json)));
    }
}
