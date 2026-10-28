// NIGHT60 builder V (B2 kill55's marks near-black): the marks' draw order, a Unity-only render number (no gameplay number: the marks are
// pictures, no Core state; the web has no hit marks, so no config number exists for them).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // render queue steps after the enemy x-ray's (Transparent + EnemyXrayQueueOffset) for kill55's SKYGRIND/Kill59Mark marks
        // (Kill55Damage.Params59): the x-ray's ZTest Greater pass lays aim.xray.color at alpha 0.35 over whatever stands in front of the body,
        // the marks included (Tools/night/n60-v-mark.py: the hole #070303 read median #860802, V 0.53, on NIGHT59's a258 frames); drawn after
        // it, a mark keeps its own near-black (foes48's shield draws after the x-ray the same way: Shield48QueueAfterXray)
        public const int Kill60MarkQueueAfterXray = 1;

        // NIGHT60 V (B7 rim56's trims): decor55's string course's largest channel x this while rim56 is on (Rim60.Band; hue and saturation kept).
        // NIGHT58's accent dim (Rim58AccentScale 0.45) reached decor54's pieces (their V on / off 0.44 on the frame) but never the band (its own
        // Recolour skips Colour56.Clamp); the same 0.45 for the band. Planted on NIGHT58's r1 ON frames (Tools/night/n60-v-rimpath.py): bodies over
        // the env p90 75 / 98 -> 87 / 98 at x0.45 and at x0.60, 86 / 98 at x0.75; V1 cover 38 / 40 at each
        public const float Rim60BandScale = 0.45f;

        // NIGHT60 V (B7 attempt 2): the lit side's gain rim56 writes while on (Rim60.LitGain), in place of NIGHT57's Rim56LitGain x Rim57Gain (2.205),
        // which washed the brightest bodies under the environment's saturation (iceline g2: S 0.72-0.86 off -> 0.53-0.63 on, env S p90 0.65).
        // Planted on NIGHT58's r1 pairs with the band fix (Tools/night/n60-v-rimgain.py, uncalibrated): bodies over the env p90 87 / 98 at 2.205,
        // 89 / 99 at 1.4-1.7, 91 / 99 at 1.3 (91.9 %), 91 / 98 at 1.0 (the fallback: skin52's own lit side, Skin52LitGain).
        // Attempt 3 (NIGHT60 B7): 1.3 -> 1.1. In the Player at 1.3 (n60-b5) the MAIN read 128 / 141 but colour56's guard "world over the tells" went
        // 3 -> 4 / 40: iceline g2-entry's tells (median S 0.646) under the world's capped 0.654, the gain whitening their bright parts. Estimated on
        // n60-b4 (2.205) / n60-b5 (1.3) (Tools/night/n60-v-rimgain2.py, uncalibrated below 1.3): 1.1 -> 130 / 141, 3 / 40, g2-entry 0.694 vs 0.654
        public const float Rim60LitGain = 1.1f;
    }
}
