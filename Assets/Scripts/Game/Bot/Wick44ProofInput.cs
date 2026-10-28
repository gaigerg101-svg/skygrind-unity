// NIGHT44 builder R2 (T-59; SPIKE.md item 1): THE WICK's fight pilot as the sim's input, one pilot step per SIM step.
// The wick37 Player reel used to run Wick37Pilot.Step `ran` times after each frame against one post-frame position and OR the presses into
// forcedMove, which ReaderInput applies to every sim step of the next frame (L-053): a held jump became a double jump, the held attach was lost
// ("panels ridden []"). The harness (Harness.N43.Wick.cs Fight43W) runs pilot.Step -> Set -> sim.Step every step; this adapter does the same
// inside the sim's own Step (SampleMove is its first read, SurvivalSim.Run.cs Step), as Walk38Pilot does for foundry43 (FeelReel.N43.Foundry.cs).
// Wick37Pilot itself is unchanged (every harness walk and gate uses it). No game rule; no allocation per step.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class Wick44ProofInput : IPlayerInput
    {
        readonly SurvivalSim sim;
        readonly IPlayerInput inner;
        public readonly Wick37Pilot pilot;
        double yaw, pitch;
        /// <summary>The pilot's tick: steps since the fight began (the harness's i).</summary>
        public int tick;
        /// <summary>Pilot steps run while THE WICK was alive.</summary>
        public int piloted;

        public Wick44ProofInput(SurvivalSim sim, Wick37Pilot pilot, IPlayerInput inner)
        {
            this.sim = sim;
            this.pilot = pilot;
            this.inner = inner;
            yaw = inner.Yaw;
            pitch = inner.Pitch;
        }

        public double Yaw => yaw;
        public double Pitch => pitch;
        public void SetLook(double y, double p) { yaw = y; pitch = p; inner.SetLook(y, p); }
        public bool Captured => inner.Captured;

        bool Live => sim.wick37 != null && sim.wick37.Alive;

        public InputFrame SampleMove(bool active)
        {
            inner.SampleMove(active); // the reader's latched presses consumed (never applied)
            if (!Live) return new InputFrame { yaw = yaw, pitch = -0.6 };
            pilot.Step(sim, yaw, tick++);
            piloted++;
            yaw = pilot.Move.yaw;
            pitch = pilot.Move.pitch;
            inner.SetLook(yaw, pitch);
            return pilot.Move;
        }

        public CombatInput SampleCombat(bool active)
        {
            inner.SampleCombat(active);
            return Live ? pilot.Combat : new CombatInput { weapon = -1 };
        }
    }
}
