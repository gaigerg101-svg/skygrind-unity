// NIGHT35 builder M: `n35move walk <level-id ...> [--seeds n] [--off]` the walk29 pilot (Walk29Pilot + Span30Pilot + the autopilot) on
// Design/levels/<id>.json through the REAL sim, devices on (as lines34 walk), with design.n35.wallride35 AND grapple35 forced ON (--off: both
// forced off: NIGHT34's movement). Prints the level's campaign place and whether the grapple is allowed there (grapple35Here) once.
using System;
using System.Globalization;
using Skygrind.Game;

public static partial class Harness
{
    static readonly System.Collections.Generic.Queue<string> n35Ring = new System.Collections.Generic.Queue<string>();

    static int N35WalkCommand(string[] args)
    {
        int seeds = 10;
        for (int i = 1; i < args.Length - 1; i++) if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
        bool off = Array.IndexOf(args, "--off") >= 0;
        int code = 0;
        int pi = Array.IndexOf(args, "--probe");
        string probe = pi >= 0 && pi + 1 < args.Length ? args[pi + 1] : null;
        if (probe != null)
            SurvivalSim.Probe35Ride = p =>
            {
                if (probe.Contains("corner")) p.cosFollowMax = 1.1; // no corner turns
                if (probe.Contains("reride")) p.reRideDistance = 0;
                if (probe.Contains("buff")) p.speedBuff = 0;
                if (probe.Contains("entry")) p.entrySpeed = 4;
                if (probe.Contains("late")) p.lateSteps = 0;
                if (probe.Contains("hold")) p.safetySteps = 144; // the hold ride's cap = the web's 1.2 s
                if (probe.Contains("stick")) p.stick = 0;
                if (probe.Contains("grace")) p.lostGraceSteps = 0;
                return p;
            };
        string old = walkPrefix;
        Action<SurvivalSim> oldHook = walkAfterStart;
        SurvivalSim.Force35Ride = !off && Array.IndexOf(args, "--grapple-only") < 0;
        SurvivalSim.Force35Grapple = !off && Array.IndexOf(args, "--ride-only") < 0;
        try
        {
            for (int k = 2; k < args.Length; k++)
            {
                string id = args[k];
                if (id.StartsWith("--") || char.IsDigit(id[0]) || id.Length < 5) continue;
                string prefix = id.Substring(0, 3), g = id.Substring(4);
                bool told = false;
                walkPrefix = prefix;
                walkAfterStart = sim =>
                {
                    sim.Devices33Force(true);
                    if (g == "spire" && sim.input is Walk29Pilot w) sim.UseInput(new Spire33Guard(sim, w));
                    if (Array.IndexOf(args, "--debug") >= 0)
                    {
                        var ring = n35Ring; ring.Clear();
                        int n = 0;
                        sim.mover.Events += e =>
                        {
                            n++;
                            ring.Enqueue($"#{n} t{sim.run.steps * sim.step:0.00} {e.type} {e.reason} {e.kind} sp {e.speed:0.0} p ({sim.mover.position.x:0.00},{sim.mover.position.y:0.00},{sim.mover.position.z:0.00}) n ({sim.mover.wallNormal.x:0.00},{sim.mover.wallNormal.z:0.00}) wj {sim.mover.wallJumpsLeft}");
                            if (ring.Count > 30) ring.Dequeue();

                        };
                    }
                    if (!told)
                    {
                        told = true;
                        Console.WriteLine($"n35move walk {id}: wallride35 {sim.n35.wallride35}, grapple35 {sim.n35.grapple35} (pull {sim.Grapple35Pull}), campaign place {sim.grapple35LevelPlace}, grapple allowed here {sim.grapple35Here}");
                    }
                };
                int c = 0;
                N29Walk("walk29", new[] { "walk29", g, "--seeds", seeds.ToString(CultureInfo.InvariantCulture) }, ref c);
                code |= c;
                foreach (string l in n35Ring) Console.WriteLine("    " + l);
            }
        }
        finally
        {
            walkPrefix = old;
            walkAfterStart = oldHook;
            SurvivalSim.Force35Ride = null;
            SurvivalSim.Force35Grapple = null;
            SurvivalSim.Probe35Ride = null;
        }
        return code;
    }
}
