// NIGHT27 (builder A): the level rows' harness lines.
//   hazard27 [--off]   hazardFloor27: layer1's casting floor (room r2) holds a floor strip; a run with the player parked on it (the room's enemies cannot
//                      hurt: hp is refilled every step, the number is the strip's own damage counter) shows: no damage before the telegraph, none during it
//                      (1.5 s), 12 HP/s (ticks) only while live (8 s), none after; a player parked off the strip (the wall lane's side) takes none.
//                      --off: the row off, the strip is plain floor (RED: 0 damage, the state never leaves 0).
// Dispatch (the lead's Program.cs): `N27Level(cmd, args, ref code);`
using System;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void N27Level(string cmd, string[] args, ref int code)
    {
        if (cmd == "hazard27") code = Hazard27(args);
    }

    static int Hazard27(string[] args)
    {
        bool off = Array.IndexOf(args, "--off") >= 0;
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath("Design/levels/layer1.json"))));
        double[] onStrip = HazardProbe(cfg, map, off, 12, 20, out string onLine);
        double[] offStrip = HazardProbe(cfg, map, off, -5, 20, out string offLine);
        Console.WriteLine($"hazard27 [row {(off ? "OFF" : "ON")}]: player ON the strip (x 12, z 20): {onLine}");
        Console.WriteLine($"hazard27 [row {(off ? "OFF" : "ON")}]: player OFF the strip (x -5, z 20): {offLine}");
        double tele = cfg.Num("design.n27.hazardFloor27.telegraphSeconds"), live = cfg.Num("design.n27.hazardFloor27.liveSeconds"), dps = cfg.Num("design.n27.hazardFloor27.damagePerSecond");
        // onStrip: [quiet damage, telegraph damage, live damage, after damage, live seconds seen, telegraph seconds seen]
        bool pass = Math.Abs(onStrip[2] - dps * live) <= dps * cfg.Num("design.n27.hazardFloor27.tickSeconds") + 1e-6 && onStrip[0] == 0 && onStrip[1] == 0 && onStrip[3] == 0
            && Math.Abs(onStrip[4] - live) <= 0.02 && Math.Abs(onStrip[5] - tele) <= 0.02 && offStrip[0] + offStrip[1] + offStrip[2] + offStrip[3] == 0;
        Console.WriteLine($"hazard27 {(pass ? "GREEN" : "RED")}: expected {dps * live:0.#} HP in the live window ({dps} HP/s x {live} s), 0 elsewhere, telegraph {tele} s, off the strip 0");
        return pass ? 0 : 1;
    }

    /// <summary>One run in layer1's room r2 with the player held at (x, z); returns [damage before the telegraph, in the telegraph, live, after, live seconds, telegraph seconds].</summary>
    static double[] HazardProbe(GameConfig cfg, SurvivalMap map, bool off, double x, double z, out string line)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n24Override = new N24Blocks { campaign = true, wyrm = true };
        sim.n27Override = off ? N27Blocks.Off : new N27Blocks { hazardFloor27 = true };
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(1, null, null);
        sim.Step();
        sim.zones.Open("r2", true);
        double[] dmg = new double[4];
        int liveSteps = 0, teleSteps = 0, entered = -1;
        int total = MovementMath.StepsFor(30, sim.step);
        for (int s = 0; s < total; s++)
        {
            if (entered >= 0 && s - entered > MovementMath.StepsFor(20, sim.step)) break; // one cycle: the second telegraph is at 24 s after the entry
            sim.health.hp = sim.health.max;
            Campaign24 c = sim.campaign;
            if (c == null || c.room < 1)
            {
                // walk the level's first room away: stand in r1 and kill its waves until its door opens, then stand in r2
                Vec3 t = c != null && c.room == 0 && c.phase == Campaign24.Phase.Cleared ? sim.campaignLevel.rooms[1].spawn : sim.campaignLevel.rooms[0].spawn;
                sim.mover.Reset(t.x, t.y, t.z);
                sim.husks.KillAll();
                sim.brain.Clear();
            }
            else sim.mover.Reset(x, 0, z);
            double before = sim.hazard27Damage;
            sim.Step();
            if (entered < 0 && sim.campaign != null && sim.campaign.room == 1) { entered = s; teleSteps = liveSteps = 0; }
            int st = sim.hazard27State;
            if (st == 1) teleSteps++;
            if (st == 2) liveSteps++;
            double d = sim.hazard27Damage - before;
            int bucket = st == 1 ? 1 : st == 2 ? 2 : (teleSteps + liveSteps == 0 ? 0 : 3);
            dmg[bucket] += d;
        }
        line = $"entered r2 at step {entered}, telegraph {teleSteps * sim.step:0.00} s (damage {dmg[1]:0.#}), live {liveSteps * sim.step:0.00} s (damage {dmg[2]:0.#}), before {dmg[0]:0.#}, after {dmg[3]:0.#}";
        return new[] { dmg[0], dmg[1], dmg[2], dmg[3], liveSteps * sim.step, teleSteps * sim.step };
    }
}
