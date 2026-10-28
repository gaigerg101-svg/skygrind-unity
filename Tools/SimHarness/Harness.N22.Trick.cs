// NIGHT22 leftover 4 (builder M): `arcline2 --trick [--selftest]` (the lead routes `trick2` to TrickCommand), TRICK CHAIN on the harness
// (Core/Trick22.cs, Game/SurvivalSim.Trick22.cs). One scripted run of real mover inputs: a slide-jump, a slam in the air, then a dash-jump
// (each a trick the mover emits), three times over the same floor:
//   chain   the three tricks back to back (inside the 3 s window), row ON
//   broken  the SAME inputs with a 4 s pause between the tricks (outside the window), row ON
//   off     the chain script with the row OFF (the run NIGHT21 plays)
// Prints the style total of each, the CHAIN awards, and the contract line "trick2 PASS|FAIL": chain pays more than broken, and off equals
// broken's trick awards (the row adds only CHAIN bonuses). The fingerprint (row off) is the `fingerprint` command's and stays EQUAL.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class TrickRun
    {
        public double total;
        public int chainAwards;
        public double chainPoints;
        public readonly List<string> tricks = new List<string>();
    }

    static void TrickTick(SurvivalSim sim, double yaw, InputFrame f)
    {
        var inp = (ScriptedInput)sim.input;
        inp.SetLook(yaw, 0);
        inp.Set(f, new CombatInput { weapon = -1 });
        sim.hitstopLeft = 0;
        sim.Step();
    }

    static TrickRun TrickPass(GameConfig cfg, SurvivalMap map, bool on, int pauseSteps)
    {
        var r = new TrickRun();
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n22Override = on ? new N22Blocks { trickChain = true } : N22Blocks.Off;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(2701, null, null);
        sim.events.On("trick", o => r.tricks.Add((string)o));
        sim.events.On("slamUse", _ => r.tricks.Add("slam"));
        sim.events.On("styleAward", o => { var e = (StyleAwardEvent)o; if (e.name.StartsWith("CHAIN")) { r.chainAwards++; r.chainPoints += e.points; } });
        // a wide open floor spot, looking down its longest clear line
        Vec3 from = sim.map.playerSpawn;
        Vec3 best = from;
        double bestRun = 0, yaw = 0;
        var rng = new Mulberry32(2710);
        for (int t = 0; t < 300; t++)
        {
            double a = rng.Next() * Math.PI * 2, rad = 6 + rng.Next() * 16;
            Vec3 p = new Vec3(Math.Cos(a) * rad, 0, Math.Sin(a) * rad);
            if (!sim.world.RayIntersect(new Vec3(p.x, 26, p.z), new Vec3(0, -1, 0), out RayHit down) || 26 - down.distance > 0.2) continue;
            for (int k = 0; k < 16; k++)
            {
                double y = k * Math.PI / 8;
                var dir = new Vec3(-Math.Sin(y), 0, -Math.Cos(y));
                double clear = sim.world.RayIntersect(new Vec3(p.x, 1.0, p.z), dir, out RayHit h) ? h.distance : 60;
                if (clear > bestRun) { bestRun = clear; best = p; yaw = y; }
            }
        }
        sim.Respawn(best, yaw * 180 / Math.PI);
        for (int s = 0; s < 40; s++) TrickTick(sim, yaw, default);
        sim.style.Start();
        sim.health.max = sim.health.hp = 1e6;
        var ahead = new Vec3(best.x - Math.Sin(yaw) * 6, 0, best.z - Math.Cos(yaw) * 6);
        sim.husks.Spawn("husk", ahead, best);
        sim.husks.frozen = true; // a husk stands near (the web's tricks pay near an enemy), harmless
        Button down1 = new Button { held = true, pressed = true }, held1 = new Button { held = true };
        // unit 1: sprint up, slide, jump out of the slide (slideJump)
        for (int s = 0; s < 70; s++) TrickTick(sim, yaw, new InputFrame { moveZ = 1 });
        for (int s = 0; s < 40; s++) TrickTick(sim, yaw, new InputFrame { moveZ = 1, slide = s == 0 ? down1 : held1, jump = s == 25 ? down1 : default });
        for (int s = 0; s < 90; s++) TrickTick(sim, yaw, default); // land
        for (int s = 0; s < pauseSteps; s++) TrickTick(sim, yaw, default);
        // unit 2: a plain jump, then the slam: the slide key pressed in the air
        for (int s = 0; s < 8; s++) TrickTick(sim, yaw, new InputFrame { jump = s == 0 ? down1 : default });
        for (int s = 0; s < 90; s++) TrickTick(sim, yaw, new InputFrame { slide = s == 0 ? down1 : held1 });
        for (int s = 0; s < 40; s++) TrickTick(sim, yaw, default);
        for (int s = 0; s < pauseSteps; s++) TrickTick(sim, yaw, default);
        // unit 3: a dash, then a jump (dashJump)
        for (int s = 0; s < 40; s++) TrickTick(sim, yaw, new InputFrame { moveZ = 1, dash = s == 0 ? down1 : default, jump = s == 8 ? down1 : default });
        for (int s = 0; s < 120; s++) TrickTick(sim, yaw, default);
        r.total = sim.style.total;
        return r;
    }

    static void TrickCommand(string[] args, ref int code)
    {
        if (Array.IndexOf(args, "--selftest") >= 0)
        {
            int failed = NUnit.Framework.MiniRunner.Run(typeof(Skygrind.Core.Tests.Trick22Tests), Console.WriteLine);
            Console.WriteLine($"trick2 --selftest {(failed == 0 ? "PASS" : "FAIL")}");
            code = failed == 0 ? 0 : 1;
            return;
        }
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        int window = MovementMath.StepsFor(cfg.Num("design.n22.trickChain.window"), 1.0 / 120);
        TrickRun chain = TrickPass(cfg, map, true, 0);
        TrickRun broken = TrickPass(cfg, map, true, window + 120);
        TrickRun off = TrickPass(cfg, map, false, 0);
        Console.WriteLine($"  chain : tricks [{string.Join(", ", chain.tricks)}], CHAIN awards {chain.chainAwards} ({chain.chainPoints:0.0} pts), style total {chain.total:0.0}");
        Console.WriteLine($"  broken: tricks [{string.Join(", ", broken.tricks)}], CHAIN awards {broken.chainAwards} ({broken.chainPoints:0.0} pts), style total {broken.total:0.0}");
        Console.WriteLine($"  off   : tricks [{string.Join(", ", off.tricks)}], CHAIN awards {off.chainAwards}, style total {off.total:0.0}");
        bool ok = chain.chainAwards >= 1 && broken.chainAwards == 0 && off.chainAwards == 0 && chain.total > broken.total && chain.tricks.Count >= 3 && broken.tricks.Count >= 3;
        Console.WriteLine($"trick2 {(ok ? "PASS" : "FAIL")}: the scripted slide-jump, slam, dash-jump run pays {chain.total:0.0} style with the chain and {broken.total:0.0} with the same inputs and the chain broken ({chain.chainAwards} CHAIN awards, {chain.chainPoints:0.0} pts vs {broken.chainAwards}); row off {off.total:0.0} with {off.chainAwards} CHAIN awards");
        code = ok ? 0 : 1;
    }
}
