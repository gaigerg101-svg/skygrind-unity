// NIGHT24 (builder L): `campaign --shaft [<plan|map.json>] [--room kb]` - the FLUE's wall-jump line measured with the real Mover on the level's
// collider. The shaft is a corridor between two facing walls run ALONG its length (a wall ride needs speed along the wall: wallRide.minSpeed): a
// scripted climber runs north hugging the west wall, jumps into a ride, wall-jumps across (wallJump.outSpeed) to the east wall, rides that,
// jumps back, and so on, until it stands at the exit deck's height. Prints the time and the top it reached, so the shaft's width, length and
// the deck's height are chosen from a measurement. LevelValidate24's report quotes the same climber as "the wall-jump route".
using System;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;
using Skygrind.Game.MapAuthoring;

public static partial class Harness
{
    static void ShaftCommand(string[] args, ref int code)
    {
        string path = "Design/levels/layer1.plan.json", room = "kb";
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--room" && i + 1 < args.Length) room = args[++i];
            else if (!args[i].StartsWith("--") && args[i - 1] != "--room") path = args[i];
        }
        GameConfig cfg = Config();
        SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
        LevelDef level = LevelDef.From(json);
        RoomDef r = level.rooms.Find(x => x.id == room);
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.zones.OpenAll();
        // the two facing slabs of the room: the thin boxes of kind "slab" inside its rect
        double west = double.NaN, east = double.NaN, z1 = double.NaN;
        foreach (Skygrind.Core.MapBox b in map.boxes)
        {
            if (b.kind != "slab" || b.min.x < r.x0 || b.max.x > r.x1 || b.min.z < r.z0 || b.max.z > r.z1 || b.max.x - b.min.x > 3) continue;
            if (double.IsNaN(west) || b.max.x < west) west = b.max.x;
            if (double.IsNaN(east) || b.min.x > east) east = b.min.x;
            z1 = double.IsNaN(z1) ? b.max.z : Math.Max(z1, b.max.z);
        }
        double goalY = r.exit.y;
        var (best, seconds, jumps) = LevelValidate24.WallClimb(sim.mover, sim.step, west + 0.8, z1 - 0.5, goalY, 20);
        Console.WriteLine($"shaft {room}: walls x {west:0.##} / {east:0.##} (gap {east - west:0.##} m), start ({west + 0.8:0.#}, {z1 - 0.5:0.#}), goal feet y {goalY:0.##}: best height {best:0.00} m, {(seconds >= 0 ? "STOOD AT THE DECK HEIGHT at " + seconds.ToString("0.0", CultureInfo.InvariantCulture) + " s" : "did not reach it in 20 s")}, wall jumps {jumps}");
        code = seconds >= 0 ? 0 : 1;
    }
}
