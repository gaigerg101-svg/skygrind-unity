// NIGHT55 builder R (P2-clear; design.n55.clear55, a view row): SurvivalGame's hook for the room-clear hold (Game/Clear55.cs). The loop's one
// time-share line (SurvivalGame.Impact.cs ImpactBeginFrame) calls Clear55Frame last: 0 while the hold runs (the freeze a hitstop gives: no
// sim step, WorldDt 0, HeldThisFrame), the share unchanged otherwise. The beat is built on the first simulated frame from the game's config,
// and listens to the bus's `roomClear` (Campaign24, inside the step of the room's last kill). Visual time only: a held frame runs no sim step,
// so a tape and the fingerprint never see it (as Arena36's last-kill slow). Row off: nothing is built and the share passes through.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        Clear55 clear55;
        bool clear55Read;

        /// <summary>The room-clear beat (null with the row off), built on first use from the game's config and hooked to roomClear.</summary>
        public Clear55 Clear55Beat
        {
            get
            {
                if (!clear55Read && cfg != null && events != null)
                {
                    clear55Read = true;
                    if (Clear55.On(cfg))
                    {
                        clear55 = new Clear55(cfg);
                        Clear55 c = clear55;
                        events.On<RoomEvent>("roomClear", e => c.OnRoomClear(e));
                        OnNewRun += c.Halt;
                    }
                }
                return clear55;
            }
        }

        /// <summary>The loop's line (ImpactBeginFrame, last): the world's share with the room-clear hold (0 while it runs).</summary>
        double Clear55Frame(double frameDt, double share)
        {
            Clear55 c = Clear55Beat;
            if (c == null) return share;
            bool allowed = flow == Flow.Playing && impactFrames != null && impactFrames.rules.hitFreeze != HitFreezeMode.Off;
            return c.Frame(frameDt, share, allowed);
        }
    }
}
