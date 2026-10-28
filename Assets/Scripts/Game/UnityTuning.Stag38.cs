// NIGHT38 builder S2 (stag38's read pass; PLAN.md section 5; GG 2026-10-13: "it's just hard to really know what's happening"): render / UI numbers
// only (the sim never reads these). Read by Stag37View.Read38.cs and Stag37View's warnings while the run's stag38 bit is on.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The lane warnings with stag38 on: x this on every RED lane warning's gain (a floe's telegraph, the shard kick's lane strip, the lane
        /// cut's stripe and barrier). 1.8: the floe tell's peak goes 1.6 -> 2.9, past the cyan trims' 1.6 (so danger out-shines the ride trims).</summary>
        public const float Stag38WarnGain = 1.8f;
        /// <summary>A told floe's BEACON: a thin RED column standing on it (m tall, m wide, gain at the tell's end): seen over the 5 m lane walls and
        /// the 6 m island from the far side of the loop (the floe itself is hidden behind a lane wall from the next lane).</summary>
        public const float Stag38BeaconHeight = 9f, Stag38BeaconWidth = 0.35f, Stag38BeaconGain = 2.2f;
        /// <summary>THE STAG's off-screen arrow: a red-orange chevron (rule 8: an enemy) at the screen's edge toward it while it is off screen or behind
        /// you: px from the edge, px size (larger than the climb arrow's 30 x 22: the boss), the pulse's low alpha and period (s; steady with
        /// reduced motion), and the alpha while it is in a wind-up (a tell: full, no pulse).</summary>
        public const float Stag38ArrowInset = 84f, Stag38ArrowW = 44f, Stag38ArrowH = 32f, Stag38ArrowPulseLow = 0.55f, Stag38ArrowPulse = 0.9f;
        /// <summary>The point the arrow aims at: m over THE STAG's feet (its torso; design.n37.stag37.body.height is 3.2).</summary>
        public const float Stag38ArrowAimY = 2f;
    }
}
