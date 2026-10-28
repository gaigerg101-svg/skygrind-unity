// NIGHT27 (the lead): `tape27 [--steps n] [--seed s]`: a taped run with the config's NIGHT27 rows ON (pulse27, chargeShots27, the bosses), then
// replayed in the same process from the tape alone (TapeReplay): EQUAL or the first differing checkpoint. The composed fingerprint forces the
// NIGHT27 blocks off until N27OnPath, so this is the one headless check that a rows-ON run replays bit for bit.
using System;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void N27Tape(string cmd, string[] args, ref int code)
    {
        if (cmd != "tape27") return;
        int steps = 3600;
        uint seed = SimFingerprint.Seed;
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--steps" && i + 1 < args.Length) steps = int.Parse(args[++i]);
            else if (args[i] == "--seed" && i + 1 < args.Length) seed = uint.Parse(args[++i]);
        }
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.n27Override = N27Blocks.ForceOff ? N27Blocks.Off : N27Blocks.From(cfg);
        bool hold = Array.IndexOf(args, "--hold") >= 0;
        if (Array.IndexOf(args, "--warm") >= 0 && !hold) TapeScript.Record(sim, seed + 7, steps, "warm", null, TapeScript.God); // a first run on the same sim: state left over from it must not reach the next run
        RunTape tape;
        if (Array.IndexOf(args, "--hold") >= 0) // the smoke's tape step: fire HELD all the time (a press every 20 steps), moving and jumping, no script
        {
            var si = new ScriptedInput();
            sim.UseInput(si);
            if (Array.IndexOf(args, "--warm") >= 0) TapeScript.Record(sim, seed + 7, 700, "warm", (sm, i) => si.Set(new InputFrame { moveZ = 1 }, new CombatInput { fire = new Button { held = true, pressed = i % 40 == 0 }, weapon = -1 }), TapeScript.God, false); // a first run that ends mid charge / cooldown
            tape = TapeScript.Record(sim, seed, steps, "tape27", (sm, i) =>
            {
                si.Set(new InputFrame { moveX = (i / 120) % 2 == 0 ? 1 : -1, moveZ = 1, jump = new Button { pressed = i % 180 == 0, held = i % 180 < 10 } },
                    new CombatInput { fire = new Button { held = true, pressed = i % 40 == 0 }, weapon = -1 });
            }, TapeScript.God, false);
        }
        else if (Array.IndexOf(args, "--charge") >= 0) // NIGHT27 W: ONE press per hold (what a thumb does): holds of 0.4 + 0.6 s and more, released, so the charge builds, fires and cools inside the tape
        {
            var si = new ScriptedInput();
            sim.UseInput(si);
            Action<SurvivalSim, int> script = (sm, i) =>
            {
                int k = i % 1500; // 12.5 s a cycle: held 0.2 .. 1.6 s (a full pistol charge is 1.0 s), let go, a tap, then 9 s of cooldown with taps
                bool down = (k >= 24 && k < 192) || (k == 600) || (k >= 700 && k < 703) || (k >= 800 && k < 1000);
                bool press = k == 24 || k == 600 || k == 700 || k == 800;
                si.Set(new InputFrame { moveX = (i / 120) % 2 == 0 ? 1 : -1, moveZ = 1 }, new CombatInput { fire = new Button { held = down, pressed = press }, weapon = -1 });
            };
            if (Array.IndexOf(args, "--warm") >= 0) TapeScript.Record(sim, seed + 7, 400, "warm", script, TapeScript.God, false); // a first run that ends mid charge
            tape = TapeScript.Record(sim, seed, steps, "tape27", script, TapeScript.God, false);
        }
        else tape = TapeScript.Record(sim, seed, steps, "tape27", null, TapeScript.God);
        Console.WriteLine($"tape27: {tape.steps} steps taped with {sim.n27} (tape bits {tape.n27}); kills {sim.run.stats.kills}, shots {sim.run.stats.shots}");
        TapeResult r = TapeReplay.Replay(Config(), Map(), tape);
        Console.WriteLine("tape27 replay: " + r.Line);
        code = r.Equal ? 0 : 1;
    }
}
