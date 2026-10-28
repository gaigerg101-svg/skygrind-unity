// NIGHT22 U3-36.9 A-7 (builder M): `alloc [--steps n] [--warm n] [--seed s] [--tag t]`: names the allocation behind
// AutopilotTests.PerStep_AllocatesNothing. That test measures the autopilot adapter's own per-step path (SampleMove +
// SampleCombat) with Unity's GC.Alloc profiler recorder, on the stand-in brain with no graph, run 6, from step 2400.
// Here the same loop (same brain, graph, seed, warm-up) counts bytes with GC.GetAllocatedBytesForCurrentThread around
//   - SampleMove and SampleCombat (the test's two calls), and inside SampleMove around its parts:
//   - AutopilotTarget.Update, AutopilotShopper.Plan (breaks only) and the brain's Step;
//   - sim.Step() for scale (not part of the test).
// A call that allocates is printed with its bytes and the step it first allocated at; the last line is the contract:
// "alloc: ..." with the total bytes over the window. The counter is exact (bytes, per thread), the profiler's is a sample
// count (any allocation, any size); a zero here is "none on .NET", which can differ from Mono's boxing/closure behaviour: the
// line says so when it finds nothing. Do not weaken the test: this only names a culprit.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class AllocBucket
    {
        public string name;
        public long bytes, calls, hitCalls, firstStep = -1;
        public long max;
    }

    static partial void AllocCommand(string[] args, ref int code)
    {
        int steps = int.Parse(StampArg(args, "--steps", "24000"));
        int warm = int.Parse(StampArg(args, "--warm", "2400"));
        uint seed = uint.Parse(StampArg(args, "--seed", "6"));
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        AutopilotInput ap;
        var factory = AutopilotInput.BrainFactory;
        var graphs = AutopilotInput.GraphSource;
        try
        {
            AutopilotInput.BrainFactory = AutopilotInput.StandInBrain;
            AutopilotInput.GraphSource = AutopilotInput.NoGraph;
            ap = new AutopilotInput(sim, "normal", seed);
        }
        finally
        {
            AutopilotInput.BrainFactory = factory;
            AutopilotInput.GraphSource = graphs;
        }
        sim.UseInput(ap);
        sim.StartRun(seed, null, null);

        var move = new AllocBucket { name = "SampleMove(true)" };
        var combat = new AllocBucket { name = "SampleCombat(true)" };
        var target = new AllocBucket { name = "  Target.Update()" };
        var plan = new AllocBucket { name = "  Shopper.Plan() (break steps)" };
        var brainStep = new AllocBucket { name = "  brain.Step()" };
        var simStep = new AllocBucket { name = "sim.Step() (scale only)" };
        var draft = new AllocBucket { name = "TakeDraft (the test's own, outside its window)" };
        var buckets = new[] { move, combat, target, plan, brainStep, simStep, draft };
        int fight = 0, shop = 0;
        BotFrame scratch = default;
        double dt = sim.step;

        // prime every call once so a first-use JIT/static init is not charged to the window (the test warms for 2400 steps)
        for (int i = 0; i < steps && sim.flow == Skygrind.Game.Flow.Playing; i++)
        {
            long a0 = GC.GetAllocatedBytesForCurrentThread();
            AutopilotInput.TakeDraft(sim);
            long a1 = GC.GetAllocatedBytesForCurrentThread();
            if (i >= warm)
            {
                Count(draft, a1 - a0, i);
                long m0 = GC.GetAllocatedBytesForCurrentThread();
                ap.SampleMove(true);
                long m1 = GC.GetAllocatedBytesForCurrentThread();
                ap.SampleCombat(true);
                long c1 = GC.GetAllocatedBytesForCurrentThread();
                // the first two are the test's; the rest are beside them (they change the run, which does not matter here)
                Count(move, m1 - m0, i);
                Count(combat, c1 - m1, i);
                bool shopping = sim.run.state == SurvivalRun.State.Break;
                if (shopping) shop++;
                else fight++;
                long t0 = GC.GetAllocatedBytesForCurrentThread();
                ap.Target.Update();
                long t1 = GC.GetAllocatedBytesForCurrentThread();
                Count(target, t1 - t0, i);
                if (shopping)
                {
                    long p0 = GC.GetAllocatedBytesForCurrentThread();
                    ap.Shopper.Plan();
                    long p1 = GC.GetAllocatedBytesForCurrentThread();
                    Count(plan, p1 - p0, i);
                }
                scratch = default;
                long b0 = GC.GetAllocatedBytesForCurrentThread();
                ap.Brain.Step(ref scratch, dt);
                long b1 = GC.GetAllocatedBytesForCurrentThread();
                Count(brainStep, b1 - b0, i);
            }
            long s0 = GC.GetAllocatedBytesForCurrentThread();
            sim.Step();
            long s1 = GC.GetAllocatedBytesForCurrentThread();
            if (i >= warm) Count(simStep, s1 - s0, i);
        }

        Console.WriteLine($"alloc A-7: stand-in brain, no graph, seed {seed}, window from step {warm}: {fight} fight steps, {shop} break steps");
        Console.WriteLine("  bucket                                          calls  calls>0   bytes   max/call  first step");
        foreach (AllocBucket b in buckets)
            Console.WriteLine($"  {b.name,-46} {b.calls,6} {b.hitCalls,8} {b.bytes,7} {b.max,9}  {(b.firstStep < 0 ? "-" : b.firstStep.ToString())}");
        long test = move.bytes + combat.bytes;
        AllocBucket culprit = null;
        foreach (AllocBucket b in new[] { target, plan, brainStep })
            if (b.bytes > 0 && (culprit == null || b.bytes > culprit.bytes)) culprit = b;
        string verdict = test == 0
            ? "NO allocation on .NET in SampleMove or SampleCombat over the window (the Unity profiler's GC.Alloc recorder can still see one that .NET's counter does not, e.g. Mono boxing or a closure; reproduces: NO)"
            : culprit != null ? $"the allocation is inside SampleMove: {culprit.name.Trim()} ({culprit.bytes} bytes over {culprit.hitCalls} calls)"
            : $"SampleMove/SampleCombat allocate {test} bytes but not inside Target.Update, Shopper.Plan or brain.Step: the adapter's own code ({(move.bytes > 0 ? "SampleMove" : "SampleCombat")})";
        Console.WriteLine($"alloc: SampleMove {move.bytes} B in {move.hitCalls}/{move.calls} calls, SampleCombat {combat.bytes} B in {combat.hitCalls}/{combat.calls}; {verdict}");
        code = 0;
    }

    static void Count(AllocBucket b, long bytes, int step)
    {
        b.calls++;
        if (bytes <= 0) return;
        b.bytes += bytes;
        b.hitCalls++;
        if (bytes > b.max) b.max = bytes;
        if (b.firstStep < 0) b.firstStep = step;
    }
}
