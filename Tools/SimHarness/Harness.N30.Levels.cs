// NIGHT30 (builder L): the rebuilt levels' harness commands (dispatched by the lead's Program.cs: N30Levels(cmd, args, ref code)).
//   walk30 <foundry|iceline|kiln|span> [--seeds n] [--first n] [--cap seconds] [--trace]
//     the walk29 pilot (Walk29Pilot: scripted movement rooms, the autopilot's fights) on Design/levels/n30-NAME.json through the REAL sim with the
//     design config as it is (every block ON stays on); prints per room cleared k of n, the median seconds, deaths, falls, lessons; one SUMMARY line.
//     Same code as walk29 (Harness.N29.Walk.cs) with the level-id prefix n30.
using System;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N30Levels(string cmd, string[] args, ref int code)
    {
        if (cmd == "ridetrace30") { RideTrace30(args); return; }
        if (cmd != "walk30") return;
        string old = walkPrefix;
        walkPrefix = "n30";
        try { N29Walk("walk29", args, ref code); }
        finally { walkPrefix = old; }
    }

    // ridetrace30 <map.json> <x> <z> [runUp] [lean]: the `ride` proof's input script on the real Mover and the map's collider, printing every 12th step
    // (a debugging aid for the lava rooms' wall lanes)
    static void RideTrace30(string[] args)
    {
        GameConfig cfg = Config();
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        var map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(System.IO.Path.GetFullPath(args[1]))));
        var world = new GameWorld(map.BuildOctree(), map.CreateCore(cfg.movement));
        double x = double.Parse(args[2], ci), z = double.Parse(args[3], ci);
        double runUp = args.Length > 4 ? double.Parse(args[4], ci) : 0.5, lean = args.Length > 5 ? double.Parse(args[5], ci) : 0.25;
        var m = new Mover(cfg.movement, world);
        m.Reset(x, 0, z);
        double dt = cfg.Dt;
        int jumpAt = MovementMath.StepsFor(runUp, dt);
        bool dashed = false, wj = false;
        double jumpZ = args.Length > 6 ? double.Parse(args[6], ci) : double.NegativeInfinity;
        for (int i = 0; i < MovementMath.StepsFor(4, dt); i++)
        {
            var f = new InputFrame { moveZ = 1, yaw = i >= jumpAt ? lean : 0 };
            if (i == jumpAt) f.jump = new Button { held = true, pressed = true };
            else if (!wj && m.state == MoveState.WallRide && m.position.z <= jumpZ) { wj = true; f.jump = new Button { held = true, pressed = true }; f.yaw = 0; }
            else if (i > jumpAt) f.jump = new Button { held = true, pressed = m.grounded };
            if (!dashed && i > jumpAt + 6 && !m.grounded) { dashed = true; f.dash = new Button { held = true, pressed = true }; }
            m.Step(f, dt);
            if (i % 12 == 0) Console.WriteLine($"t {i * dt:0.00} pos ({m.position.x:0.00}, {m.position.y:0.00}, {m.position.z:0.00}) state {m.state} grounded {m.grounded}");
        }
    }
}
