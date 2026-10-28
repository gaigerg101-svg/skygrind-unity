// NIGHT21 U3-35.6 DEADLINE (builder S owns this file): the fuse in the sim, through the shared commit's hooks
// (SurvivalSim.N21.cs: N21DeadlineStartRun at the run's start, N21DeadlineStep once per fixed step after the enemies). With
// design.n21.deadline off (or --off21) the twist is not among the draws and the fuse never runs: NIGHT20's game bit for bit.
// The twist's draw and its points double are Core/Enemies/SpecialRounds.cs's (the sim's `specials`, made at N20's run start,
// which comes before this hook); the fuse is Core/Deadline21.cs's.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The DEADLINE fuse (Core/Deadline21.cs). Null before the first run.</summary>
        public Deadline21 deadline21;

        partial void N21DeadlineStartRun()
        {
            deadline21 ??= new Deadline21(cfg, events, health, () => mover.state == MoveState.Slide);
            deadline21.NewRun(n21.deadline);
            if (specials != null)
            {
                specials.deadline = n21.deadline;
                specials.deadlineWeight = deadline21.Numbers.weight;
                specials.deadlinePoints = deadline21.Numbers.pointsMult;
            }
        }

        partial void N21DeadlineStep(double dt)
        {
            if (deadline21 != null && deadline21.on) deadline21.Step();
        }
    }
}
