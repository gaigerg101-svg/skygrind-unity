// NIGHT17 U3-30.10 (beyond the ask; the lead's): the numbers of the lead's beyond-the-ask pictures. Display only.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // RankWorld: the map's neon at the top rank is (1 + RankWorldGain) x as bright, mixed RankWorldHotMix of the way to
        // CYAN-HOT, pulsing at RankWorldPulseHzLow .. High (cycles / s, by the rank's share) by +/- RankWorldPulseAmp x the
        // share; it rises at RankWorldRiseRate / s and drains at RankWorldFallRate / s (a fall reads slower than a climb);
        // the block is rewritten only when the colour moved by RankWorldEpsilon (summed over r, g, b).
        public const double RankWorldGain = 1.6;
        public const double RankWorldHotMix = 0.6;
        public const double RankWorldPulseHzLow = 0.5;
        public const double RankWorldPulseHzHigh = 2.2;
        public const double RankWorldPulseAmp = 0.35;
        public const double RankWorldRiseRate = 8.0;
        public const double RankWorldFallRate = 1.5;
        public const float RankWorldEpsilon = 0.004f;

        // PowerUpView's pull and flash: the crystal leans toward a catch point PickupCatchAhead m in front of the camera and
        // PickupCatchDrop m below its centre (in view; round 3's first form aimed at the chest, under the camera) within
        // PickupPullRange m outside the sim's grabRadius, up to PickupPullShare of the way (ease-in); a take is the drop
        // going inactive within grabRadius + PickupTakeSlack m (else it expired); its flash starts at PickupFlashScale x,
        // PickupFlashGain x as bright, and shrinks into you over PickupFlashTime s.
        public const double PickupPullRange = 1.0;
        public const float PickupPullShare = 0.65f;
        public const float PickupCatchAhead = 0.9f;
        public const float PickupCatchDrop = 0.3f;
        public const double PickupTakeSlack = 0.6;
        public const double PickupFlashTime = 0.16;
        public const float PickupFlashScale = 0.6f;
        public const double PickupFlashGain = 3.0;
    }
}
