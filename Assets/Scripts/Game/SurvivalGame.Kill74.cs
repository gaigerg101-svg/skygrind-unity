// NIGHT74 builder B (slate B3, design.n74.kill74; Kill74.cs has the rule and the reasons): THE HOLD. For UnityTuning.N74KillHoldS real seconds from a campaign boss's death event (bossDying; all five since leftover 6) the world's
// time share is UnityTuning.N74KillHoldShare (0.1): the loop runs a tenth of the steps a frame, so the dead pose is held about a third of a second, then the world goes on (the finisher's own slow-mo, bossKit
// killSlowMo for killTime, runs on at its 0.3 share). The pattern is SurvivalGame.Debut20.cs's: "visual time only: the sim's fixed steps are the impact layer's (a slower share runs fewer steps a frame), so
// a tape and the fingerprint never see it": no step is skipped, repeated or changed. Off with the row, and with SLOW-MO off / REDUCED MOTION (impactFrames.rules.slowMoOn), as the debut slow-down is.
// THE HOOK (the lead's two lines in SurvivalGame.Impact.cs, a file outside builder B's list, so not edited here; the file's own pattern for every other slow layer):
//   ImpactBeginFrame, after `share = BossSlowMoFrame(frameDt, share);`:   share = Kill74SlowFrame(frameDt, share); // NIGHT74 B kill74
//   ImpactToTitle, after `DebutSlowHalt();`:                              Kill74Halt(); // NIGHT74 B kill74
// Without the two lines this file does nothing (it hooks the bus lazily from its first call): the flash and finish55's delayed punch-in still run, and the world is not held.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        double kill74Left = -1;
        bool kill74Hooked;

        /// <summary>The loop's line (ImpactBeginFrame): the world's share with the kill hold; the slower wins.</summary>
        public double Kill74SlowFrame(double frameDt, double share)
        {
            if (!kill74Hooked)
            {
                kill74Hooked = true;
                events.On<BossEvent>("bossDying", e => { if (e != null && Kill74.Supports(e.boss) && Kill74.On(cfg)) kill74Left = UnityTuning.N74KillHoldS; });
            }
            if (kill74Left <= 0) return share;
            double t = UnityTuning.N74KillHoldS - kill74Left; // seconds into the hold
            kill74Left -= frameDt; // real time: the hold's own clock
            if (flow != Flow.Playing || impactFrames == null || !impactFrames.rules.slowMoOn) return share;
            double s = Kill74Math.HoldShare(t, UnityTuning.N74KillHoldS, UnityTuning.N74KillHoldShare);
            return share <= 0 ? share : Math.Min(share, s);
        }

        /// <summary>The way to TITLE: no hold carried into the next run.</summary>
        void Kill74Halt() => kill74Left = -1;
    }
}
