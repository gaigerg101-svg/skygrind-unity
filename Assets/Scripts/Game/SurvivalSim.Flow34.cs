// NIGHT34 builder M (design.n34.flow34; NIGHT34.md M1): the run's flow rules handed to the Mover at the run's start (Core/Flow34.cs; the
// Mover's flow34 branches read them). The row off (or --off34 / -skygrindOff34, or a tape of format 16 or older): Flow34.Params.Neutral, so every
// branch is skipped and the Mover is the web's bit for bit. Each technique's own sub-switch is design.n34.flow34.<name>.enabled.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The harness's switch for flow34 (null: the run's n34 blocks decide). Process-wide, read at each run's start.</summary>
        public static bool? Force34Flow;

        /// <summary>The harness's per-technique probe (null: the config's sub-switches): wallCoyote, wallBuffer, zipCarry.</summary>
        public static bool[] Probe34Flow;

        partial void N34FlowStartRun()
        {
            if (Force34Flow.HasValue) n34.flow34 = Force34Flow.Value;
            Flow34.Params p = Flow34.Params.From(cfg, n34.flow34);
            if (Probe34Flow != null && n34.flow34)
            {
                p.wallCoyote = Probe34Flow[0];
                p.wallBuffer = Probe34Flow[1];
                p.zipCarry = Probe34Flow[2];
            }
            mover.SetFlow34(p);
        }
    }
}
