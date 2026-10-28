// NIGHT20 C1 (the lead's loop line for builder H's debut cards, UiDebut20.cs): while a kind's debut card shows, the world
// runs slow (the backlog's C1: "the world slowed on the Game layer", at most 1.5 s per card; DOOM Eternal's stop on a new
// demon). The card emits `debutCard` (DebutCardEvent: the kind, the card's seconds); the loop's ImpactBeginFrame takes the
// slower of the impact layer's share, SKYGRIND TIME's and this one (a freeze stays 0). The slow-down eases in and out
// over UnityTuning.DebutSlowEase s, never lasts more than UnityTuning.DebutSlowMax s, and is off with SLOW-MO off or
// REDUCED MOTION (the impact layer's slowMoOn), as SKYGRIND TIME is. Visual time only: the sim's fixed steps are the
// impact layer's (a slower share runs fewer steps a frame), so a tape and the fingerprint never see it.
using System;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        double debutSlowLeft = -1, debutSlowTotal;
        bool debutSlowHooked;

        /// <summary>The loop's line (ImpactBeginFrame): the world's share with a debut card's slow-down; the slower wins.</summary>
        double DebutSlowFrame(double frameDt, double share)
        {
            if (!debutSlowHooked)
            {
                debutSlowHooked = true;
                events.On<DebutCardEvent>(UnityTuning.Hud20DebutEvent, e =>
                {
                    debutSlowTotal = Math.Min(UnityTuning.DebutSlowMax, e != null && e.seconds > 0 ? e.seconds : UnityTuning.DebutSlowMax);
                    debutSlowLeft = debutSlowTotal;
                });
            }
            if (debutSlowLeft <= 0) return share;
            debutSlowLeft -= frameDt; // real time: the card's own clock
            if (flow != Flow.Playing || impactFrames == null || !impactFrames.rules.slowMoOn) return share;
            double t = debutSlowTotal - Math.Max(0, debutSlowLeft); // seconds into the card
            double ease = Math.Min(1, Math.Min(t, Math.Max(0, debutSlowLeft)) / UnityTuning.DebutSlowEase);
            double s = 1 - (1 - UnityTuning.DebutSlowShare) * ease;
            return share <= 0 ? share : Math.Min(share, s);
        }

        /// <summary>The way to TITLE: no slow-down carried into the next run.</summary>
        void DebutSlowHalt() => debutSlowLeft = -1;
    }
}
