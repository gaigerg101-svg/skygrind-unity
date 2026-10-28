// NIGHT36 (the lead's loop line for builder L's arena flow, SurvivalSim.Level36.cs): an arena36 room's last kill slows the world for a
// moment (ULTRAKILL ActivateNextWave: SlowDown 0.15 on a wave's last enemy; rea-ultrakill.md). The sim emits `arena36LastKill`
// (Arena36Event: seconds, scale from design.n36.level36); the loop's ImpactBeginFrame takes the slower of every share, as the debut
// card's (SurvivalGame.Debut20.cs). Real time, off with SLOW-MO off or REDUCED MOTION (slowMoOn). Visual time only: a slower share runs
// fewer sim steps a frame, so a tape and the fingerprint never see it.
using System;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        double arena36SlowLeft = -1, arena36SlowScale = 1;
        bool arena36SlowHooked;

        /// <summary>The loop's line (ImpactBeginFrame): the world's share with an arena's last-kill slow-down; the slower wins.</summary>
        double Arena36SlowFrame(double frameDt, double share)
        {
            if (!arena36SlowHooked)
            {
                arena36SlowHooked = true;
                events.On<Arena36Event>("arena36LastKill", e =>
                {
                    if (e == null || e.seconds <= 0) return;
                    arena36SlowLeft = e.seconds;
                    arena36SlowScale = e.scale > 0 && e.scale < 1 ? e.scale : 1;
                });
            }
            if (arena36SlowLeft <= 0) return share;
            arena36SlowLeft -= frameDt; // real time
            if (flow != Flow.Playing || impactFrames == null || !impactFrames.rules.slowMoOn) return share;
            return share <= 0 ? share : Math.Min(share, arena36SlowScale);
        }
    }
}
