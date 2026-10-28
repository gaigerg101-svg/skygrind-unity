// NIGHT56 builder G (P1-wickWindow, wick55's Player half): `n56 wickfight`, a155 FIGHT MODE headless (FeelReel.N55.Boss.cs B55Fight's sim
// side, step for step): the CAMPAIGN on the level file (default n52-foundry: the live game's WICK), DOORFREE, the husks killed and frozen, God,
// the KESTREL added, the player at the stack's mouth, Wick37Reset + Wick37Begin, Wick37Pilot (hand seed 1) through Wick44ProofInput, wick54 +
// wick55 forced on (--off: wick54 only, the RED run), to the kill or 150 s. --trigger: the reel's new input, Wick56FightInput (n47human's trigger
// habit) round the pilot. Prints a155's own line: shots, punches, core punches, inside an OPEN window (N43BossState.open), the damage share
// dealt while the bar read OPEN (n47human's rule: by the bar's state before the damage), stagger windows, kill time.
//   dotnet run --project Tools/SimHarness -c Release -- n56 wickfight [--level n52-foundry] [--trigger] [--off] [--seed 1]
using System;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static int N56WickFight(string[] args)
    {
        string Arg(string k, string d) { int i = Array.IndexOf(args, k); return i >= 0 && i + 1 < args.Length ? args[i + 1] : d; }
        string id = Arg("--level", "n52-foundry");
        bool trigger = Array.IndexOf(args, "--trigger") >= 0, off = Array.IndexOf(args, "--off") >= 0;
        uint seed = uint.Parse(Arg("--seed", "1"));
        GameConfig cfg = Config();
        string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        NightRowState rs = sim.rowsRegistry.Defaults().Set("wick54", true);
        if (!off) rs = rs.Set("wick55", true);
        sim.rowsOverride = rs;
        var si = new E53Script();
        sim.UseInput(si);
        sim.StartRun(160716, null, null);
        void God() { sim.health.max = sim.health.hp = 1e6; }
        for (int i = 0; i < 600 && (sim.campaign == null || sim.campaign.room < 0); i++) { sim.Step(); sim.Step(); }
        sim.campaign?.SetDoorFree29(true);
        sim.husks.KillAll();
        sim.husks.frozen = true;
        God();
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        if (!sim.wick37Centre.HasValue || sim.wick37Stack == null) { Console.WriteLine($"n56 wickfight {id}: no stack on this level"); return 2; }
        Vec3 c = sim.wick37Centre.Value;
        sim.Respawn(c + new Vec3(0, 0.05, sim.wick37Stack.p.radius + 1.0), 0);
        si.SetLook(0, 0);
        for (int i = 0; i < 40; i++) sim.Step();
        sim.Wick37Reset();
        if (!sim.Wick37Begin(out _, out _)) { Console.WriteLine($"n56 wickfight {id}: Wick37Begin false"); return 2; }
        Wick37 w = sim.wick37;
        var pilot = new Wick37Pilot { hand = new Mulberry32(seed * 7919u + 13u) };
        var pilotIn = new Wick44ProofInput(sim, pilot, si);
        Wick56FightInput trig = trigger ? new Wick56FightInput(sim, pilotIn) : null;
        sim.UseInput(trig != null ? (IPlayerInput)trig : pilotIn);
        int fires = 0, punches = 0, corePunches = 0, punchesOpen = 0;
        bool openNow = false;
        Action o1 = sim.events.On("fire", _ => fires++);
        Action o2 = sim.events.On("punch", _ => punches++);
        Action o3 = sim.events.On<Wick47Event>("wick47CorePunch", _ => { corePunches++; if (openNow) punchesOpen++; });
        double t0 = sim.SimTime, prevFrac = w.HpFrac, dealt = 0, dealtOpen = 0, killAt = -1, firstOpenPunch = -1;
        int seenOpen = 0;
        while (sim.SimTime - t0 < 150)
        {
            God();
            Boss43State bs = sim.N43BossState;
            bool open = bs.valid && bs.open;
            double fr = w.HpFrac;
            if (fr < prevFrac) { dealt += prevFrac - fr; if (openNow) dealtOpen += prevFrac - fr; }
            prevFrac = fr;
            openNow = open;
            if (punchesOpen > seenOpen && firstOpenPunch < 0) firstOpenPunch = sim.SimTime - t0;
            seenOpen = punchesOpen;
            if (!w.Alive) { killAt = sim.SimTime - t0; break; }
            if (Array.IndexOf(args, "--trace") >= 0 && sim.simStep % 1200 == 0)
                Console.WriteLine($"  t {sim.SimTime - t0:0.0} s: leg {w.brain.leg} state {w.brain.state}, bar {w.HpFrac * 100:0} %, feet ({sim.mover.position.x - c.x:0.0}, {sim.mover.position.y:0.0}, {sim.mover.position.z - c.z:0.0}) {sim.mover.state}, rides {pilot.rides}, leaps {pilot.leaps}");
            sim.Step();
        }
        o1(); o2(); o3();
        double share = dealt > 0 ? dealtOpen / dealt : 0;
        Console.WriteLine($"n56 wickfight {id} ({(off ? "wick54 only" : "wick54 + wick55")}, {(trigger ? "pilot + the trigger habit" : "the bare pilot")}): pilot steps {pilotIn.piloted}, rides {pilot.rides}, leaps {pilot.leaps}, shots {fires} (clicks {trig?.clicks ?? 0}), punches {punches}; "
            + $"damage dealt while the bar read OPEN {share * 100:0} %; core punches {corePunches}, inside a window {punchesOpen} (the first at {(firstOpenPunch < 0 ? "never" : firstOpenPunch.ToString("0.0") + " s")}); stagger windows {w.brain.windows}, hits inside {w.windowHits55}; "
            + $"{(killAt >= 0 ? "killed at " + killAt.ToString("0.0") + " s" : "alive at 150 s (bar " + (w.HpFrac * 100).ToString("0") + " %)")}");
        return 0;
    }
}
