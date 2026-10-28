// NIGHT20 U3-34.6 (builder N): the climb's and the leap's look (Game/EnemyViews.Climb.cs); display numbers only (the
// verbs' timings are design.routes'). Metres in the world, rad / s for the pulses.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const float ClimbArmThick = 0.14f; // an arm's square section (a husk's head box is 0.62 x 0.45)
        public const float ClimbClawSize = 0.2f; // a hand's cube: the DANGER part
        public const float ClimbShoulderUp = 0.78f; // the shoulders, as a share of the body's height
        public const float ClimbShoulderOut = 0.9f; // the shoulders' half-spread, as a share of the body's radius
        public const float ClimbReach = 0.55f; // m the hands reach over the head on the face
        public const float ClimbStride = 0.35f; // m the hands alternate by while it climbs (hand over hand)
        public const float ClimbStrideRate = 2.6f; // rad per metre climbed (one hand-over-hand every ~1.2 m)
        public const float ClimbWallGap = 0.06f; // m the hands stay off the face (no z-fighting)
        public const float ClimbTellPulse = 26f; // rad / s: the claws' and the lip bar's DANGER pulse (the husk's windup: 24)
        public const float ClimbLipBarWidth = 0.9f; // m along the lip: where it comes over
        public const float ClimbLipBarDepth = 0.12f; // m into the lip from its edge
        public const float ClimbLipBarLift = 0.03f; // m over the lip's floor
        public const float ClimbLipBarGlow = 1.3f; // the bar's brightness at its pulse's middle (the charge line's 1.2)
        public const float LeapCrouchDrop = 0.25f; // m the hands sink below the hips in the leap's crouch
        public const float LeapLandBarWidth = 1.1f; // m: the landing mark's width (the leap's tell, where it lands)
        // Row 5: a swarmer's wall-crawl (its own verb: legs, not arms; the swarmer is 0.75 m tall, 0.3 m round)
        public const float CrawlLegThick = 0.07f; // a leg's square section
        public const float CrawlSpread = 0.42f; // m each side of the body's axis the feet grip the face
        public const float CrawlRowGap = 0.28f; // m between the three rows of feet up the face
        public const float CrawlStride = 0.14f; // m a foot steps by (two tripods alternating)
        public const float CrawlStrideRate = 9f; // rad per metre climbed (a scuttle: a step every ~0.35 m)
        public const float CrawlTellReach = 0.2f; // m the feet reach up the face during the tell
    }
}
