// NIGHT22 U3-36.4 THE STAMP + T-11 (builder S owns this file): `stamp2 [--seed s] [--seconds n] [--tag t]`, the headless half of the
// contract `stamp`:
//   1. the inked stamp's motion (Core/Grades21.cs StampMotion, numbers design.n21.stamp) measured at 60 fps the way feel-metrics M9
//      counts it: the entrance 200-300 ms, the exit 150-200 ms, and the first visible frame is not a pop (scale off rest, alpha < 0.95);
//   2. A-4, the round P: the bot never reaches SS, so a SCRIPTED run feeds the style meter to the top rank every step and holds the
//      player's health (the run is labelled "scripted"): its first round must say roundGrade with p=true exactly once. The same seed
//      unscripted (the bot as it plays) is printed beside it: no P;
//   3. the row off (the same scripted seed) says no roundGrade at all.
// Code 0 when every line PASSes. The grade card vs POINTS (T-11) and the on-screen entrance are the Player reel's (FeelReel.N22.Stamp.cs).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class Stamp2Run
    {
        public List<RoundGradeEvent> said = new List<RoundGradeEvent>();
        public int pEvents, steps;
        public double wall;
    }

    /// <summary>One headless run to the first round clear (and `after` s past it, to see that the event is said once).</summary>
    static Stamp2Run Stamp2One(GameConfig cfg, SurvivalMap map, uint seed, bool scripted, bool row, double maxSeconds, double after)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n21Override = new N21Blocks { stamp = row };
        var ap = new AutopilotInput(sim, "normal", seed);
        ap.TellTheTruth();
        sim.UseInput(ap);
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        var r = new Stamp2Run();
        sim.events.On<RoundGradeEvent>(Grades21.EventName, e =>
        {
            r.said.Add(e);
            if (e.p) r.pEvents++;
        });
        sim.StartRun(seed, null, null);
        int top = cfg.Arr("style.ranks").Count - 1;
        int maxSteps = MovementMath.StepsFor(maxSeconds, sim.step);
        int afterSteps = MovementMath.StepsFor(after, sim.step);
        int clearedAt = -1;
        while (r.steps < maxSteps && sim.flow != Skygrind.Game.Flow.Results)
        {
            if (scripted)
            {
                // the harness's input script: the meter held at the top rank, the player's health held (no damage is counted)
                sim.style.core.DebugSetRank(top);
                sim.health.max = sim.health.hp = 1e6;
            }
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            r.steps++;
            if (clearedAt < 0 && r.said.Count > 0) clearedAt = r.steps;
            if (clearedAt >= 0 && r.steps - clearedAt >= afterSteps) break;
        }
        return r;
    }

    static string Stamp2Describe(Stamp2Run r)
    {
        if (r.said.Count == 0) return "no round cleared";
        RoundGradeEvent e = r.said[0];
        return $"round {e.round} grade {e.grade} (letter {e.letter}, score {e.score:0.000}, damage {e.damage:0.#}, peak rank {e.peakRank}, {e.time:0.0} s, {e.kills} kills) p={(e.p ? "true" : "false")}";
    }

    static partial void Stamp2Command(string[] args, ref int code)
    {
        uint seed = uint.Parse(StampArg(args, "--seed", "1"));
        double maxSeconds = double.Parse(StampArg(args, "--seconds", "420"), System.Globalization.CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        var t = System.Diagnostics.Stopwatch.StartNew();

        // 1. the motion
        bool motionOk = true;
        double inMs = -1, outMs = -1;
        string first = "n/a";
        try
        {
            StampMotion.P p = StampMotion.P.From(cfg);
            inMs = StampMotion.EntranceMs(p, 60);
            outMs = StampMotion.ExitMs(p, 60);
            StampMotion.At(1 / 60.0, p, out double s0, out double a0);
            bool pop = s0 >= 0.95 && s0 <= 1.05 && a0 >= 0.95;
            first = $"first visible frame scale {s0:0.00} alpha {a0:0.00}";
            motionOk = inMs >= 200 && inMs <= 300 && outMs >= 150 && outMs <= 200 && !pop;
            Console.WriteLine($"stamp2 motion (design.n21.stamp, 60 fps, M9's settle rule): entrance {inMs:0} ms (want 200-300), exit {outMs:0} ms (want 150-200), {first}: {(motionOk ? "PASS" : "FAIL")}");
        }
        catch (Exception ex)
        {
            motionOk = false;
            Console.WriteLine($"stamp2 motion: FAIL ({ex.GetType().Name}: {ex.Message})");
        }

        // 2. A-4 (scripted) and the bot as it plays
        Stamp2Run scripted = Stamp2One(cfg, map, seed, true, true, maxSeconds, 4);
        Console.WriteLine($"stamp2 scripted (the harness feeds the style meter to the top rank and holds health), seed {seed}: {Stamp2Describe(scripted)}; roundGrade said {scripted.said.Count}x, p=true {scripted.pEvents}x");
        bool pOnce = scripted.pEvents == 1 && scripted.said.Count == 1 && scripted.said[0].p && scripted.said[0].letter == "S";
        Console.WriteLine($"stamp2 round P (A-4): a scripted run earns grade S, peak SS and no revive and says roundGrade p=true once: {(pOnce ? "PASS" : "FAIL")}");
        Stamp2Run plain = Stamp2One(cfg, map, seed, false, true, maxSeconds, 0);
        bool plainNoP = plain.said.Count > 0 && plain.pEvents == 0;
        Console.WriteLine($"stamp2 unscripted (the bot as it plays), seed {seed}: {Stamp2Describe(plain)}; the bot never reaches a round P {(plainNoP ? "PASS" : "FAIL")}");

        // 3. the row off
        Stamp2Run off = Stamp2One(cfg, map, seed, true, false, 120, 0);
        bool offQuiet = off.said.Count == 0;
        Console.WriteLine($"stamp2 row off (the same scripted seed): roundGrade said {off.said.Count}x {(offQuiet ? "PASS" : "FAIL")}");

        code = motionOk && pOnce && plainNoP && offQuiet ? 0 : 1;
        Console.WriteLine($"stamp2 {(code == 0 ? "PASS" : "FAIL")}; {t.Elapsed.TotalSeconds:0.0} s wall");
    }
}
