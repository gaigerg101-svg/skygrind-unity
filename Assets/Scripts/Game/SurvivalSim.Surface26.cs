// NIGHT26 (builder M, design.n26.surfaces): the surface zones of the level in the one sim. A level plan's "surfaces" list (the map JSON's
// "campaign" block; Core/Surface26.cs) is read at the run's start; each fixed step (the lead's N26SurfaceStep call, after the mover's step, so the
// value stands for the NEXT step's ground friction) sets mover.groundFrictionMult from the zone under the player's feet and pushes on a boost strip.
// Off (--off26, a tape without the bit, or no zones): the multiplier stays 1 and nothing runs: NIGHT25's mover bit for bit. No input, no RNG, no tape change:
// the surface state is the level data and the player's position.
namespace Skygrind.Game
{
    using Skygrind.Core;

    public sealed partial class SurvivalSim
    {
        /// <summary>This run's surface zones (null: the block is off or the level has none).</summary>
        public Surfaces26 surfaces26;

        partial void N26SurfaceStartRun()
        {
            surfaces26 = null;
            if (mover != null) mover.groundFrictionMult = 1;
            if (!n26.surfaces || map == null || map.json == null) return;
            Surfaces26 s = Surfaces26.From(map.json, SurfaceRules.From(cfg));
            if (!s.Any) return;
            surfaces26 = s;
            s.Reset(mover);
        }

        partial void N26SurfaceStep(double dt)
        {
            if (surfaces26 != null) surfaces26.Step(mover);
        }
    }
}
