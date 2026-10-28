// NIGHT58 builder G (G1 kill55's marks after the flash): the a258 reel's Unity-only numbers (a probe's thresholds; no gameplay number).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // a body pixel (warm in the before frame) counts as the hit flash's white when its S is under this and its V over the next
        // (the flash is white x _Flash51Hdr: S ~0, V ~1; NIGHT57's next frame read H 60 S 0.00 V 0.97)
        public const float N58GFlashWhiteS = 0.2f, N58GFlashWhiteV = 0.85f;
        // the flash is over on the first frame after the hit where the white share of the body falls under this (2 %: specks of bloom)
        public const float N58GFlashEndShare = 0.02f;
        // frames the reel waits for the flash's end before it says "not ended" (the flash is ~0.17 s: N51HitFlashHoldS + CrunchHitFlashFade)
        public const int N58GFlashMaxFrames = 60;
        // the probe's square at the mark's projected centre: (2 x this + 1) px a side
        public const int N58GProbeHalf = 3;
    }
}
