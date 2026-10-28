// NIGHT41 builder B8 (NIGHT41.md B8): the bosses' NIGHT41 sim side. Fills the lead's partials (SurvivalSim.N41.cs):
// - N41LoomStartRun / N41LoomStep: sweep41 (THE LOOM's floor sweep, Core/Enemies/Loom37.N41.cs) goes in or out of THE LOOM's attack list at each
//   fight's begin (Loom41SweepBegin, called from Loom40StayBegin); THE LOOM's weak point (Enemy.weakPoint41, for P3's aim41): the CYAN spinneret
//   while the window is open, else the lit knee nearest you, cleared when it falls.
// - N41BrawlStartRun / N41BrawlStep: THE ICEBREAKER's weak point: its stuck fist (after the haymaker, Brawl40 Stuck), cleared otherwise.
// Off (sweep41 off, --off41): the sweep is not in the list (NIGHT40's picks, bit for bit). The weak points are plain values nothing reads
// unless aim41 is on (no tape, no gameplay).
// No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        Sweep41Params sweep41Params;

        /// <summary>Hooks the sweep's tell gave back this run (the harness).</summary>
        public int loom41SweepRefills;

        partial void N41LoomStartRun()
        {
            if (sweep41Params == null) sweep41Params = Sweep41Params.From(cfg);
            loom41SweepRefills = 0;
            if (loom37 != null) loom37.body.weakPoint41.Clear();
        }

        /// <summary>At THE LOOM's fight begin (Loom40StayBegin, before Place): sweep41 in or out of the brain's list for this fight.</summary>
        void Loom41SweepBegin()
        {
            if (loom37 == null) return;
            if (sweep41Params == null) sweep41Params = Sweep41Params.From(cfg);
            if (loom37.ropeAttached41 == null)
            {
                loom37.ropeAttached41 = () => mover.rope.phase == RopePhase.Attached;
                if (loom37.playerRiding == null) loom37.playerRiding = () => mover.state == MoveState.WallRide;
                // MEASURED round 2 (`n41loom sweep --trace`, seed 4): in leg 2 the pilot met the sweep on a tower top with 0 grapple charges (the
                // across-pulls spent both; 4 s recharge): no answer existed. A sweep that covers the tower tops winds one hook back at its tell
                // (as loom37's tearRefill), so the answer it asks for is always in hand.
                events.On<Loom37Event>("loom41Sweep", e =>
                {
                    if (e.stage != "tell" || loom37 == null || !loom37.sweep41 || !Grapple32On || !sweep41Params.tellRefill) return;
                    if (loom37.sweepTopY > sweep41Params.refillOverY && grappleCharges32 <= 0) G32Give(ref loom41SweepRefills);
                });
            }
            loom37.Sweep41(n41.sweep41, sweep41Params, loom37CanyonX0, loom37CanyonX1);
        }

        partial void N41LoomStep(double dt)
        {
            if (loom37 == null) return;
            Enemy b = loom37.body;
            if (!loom37Owns || !loom37.Alive || loom37.Falling) { b.weakPoint41.Clear(); return; }
            if (loom37.Open) { b.weakPoint41.Set(loom37.Spinneret); return; }
            int best = -1;
            double bd = 0;
            Vec3 me = mover.position;
            for (int i = 0; i < 6; i++)
            {
                if (!loom37.ActiveKnee(i)) continue;
                double d = Vec3.DistanceTo(me, loom37.knees[i].at);
                if (best < 0 || d < bd) { best = i; bd = d; }
            }
            if (best >= 0) b.weakPoint41.Set(loom37.knees[best].at);
            else b.weakPoint41.Clear();
        }

        partial void N41BrawlStartRun()
        {
            if (brawl40 != null) brawl40.body.weakPoint41.Clear();
        }

        partial void N41BrawlStep(double dt)
        {
            if (brawl40 == null) return;
            Enemy b = brawl40.body;
            if (brawl40Owns && brawl40.Alive && brawl40.StuckNow && brawl40.attack == Brawl40.Haymaker) b.weakPoint41.Set(brawl40.Fist);
            else b.weakPoint41.Clear();
        }
    }
}
