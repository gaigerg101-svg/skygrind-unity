// NIGHT44 builder V2 (design.n44.vis44, a view row: no sim bit). Render numbers for the visual finish: the FUSION's streaks, the TORCH's cone.
// Off = NIGHT43's look exactly (every use is behind Vis44.On).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- the FUSION's streaks (Fusion43Screen): tapered, curved, a hot-pink / white core before the pop ----
        public const int Vis44StreakSegments = 5; // pieces per streak along its curve (a bent line drawn as short tapered pieces)
        public const float Vis44StreakHeadPx = 26; // px the streak's head is across (NIGHT43's flat bar: 12)
        public const float Vis44StreakTailPx = 3; // px the streak's tail is across (the taper)
        public const float Vis44StreakCurlDeg = 55; // deg the streak's path turns from its start to the hit (a swirl in, not a straight spoke)
        public const float Vis44StreakLenShare = 0.75f; // share of its start distance the streak is long (NIGHT43: 0.45)
        public const float Vis44PreCorePx = 150; // px the pink glow at the hit before the pop (grows to this at the meet)
        public const float Vis44PreCoreWhite = 0.45f; // the white heart's size as a share of the pink glow
        public const float Vis44PreCoreFrom = 0.45f; // the glow's size share at t 0 (it grows to 1 at the meet)
        // ---- the TORCH (TorchFeel43): a dense cone from the muzzle to the reach ----
        public const int Vis44TorchRing = 4; // extra flame billboards round the axis at each jet segment
        public const float Vis44TorchConeDeg = 11; // deg half-angle of the cone the ring puffs sit on
        public const float Vis44TorchRingSize = 1.25f; // ring puff size vs the jet puff at that distance
    }
}
