// NIGHT57 builder V: Unity-only numbers of tonight's view work (render, capture), each with its reason. The levels' caps and the role arcs are config
// (design.n56.colour56.numbers.cap, written by Tools/night/n57-v-rows.cjs).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---------------- colour56's world saturation cap (Game/Render/Cap57Pass.cs, Assets/Shaders/Resources/Cap57.shader) ----------------

        /// <summary>cap57: 1 = the cap tests the saturation of the sRGB value (the camera's colour is linear; the frame the player sees and the meter
        /// reads, n56-colour.py's HSV on the PNG, is sRGB: a linear S overstates a dark colour's sRGB S), 0 = as stored.</summary>
        public const float Cap57Srgb = 1f;

        /// <summary>colour56 on NIGHT57 (the r1 greyscale guard: v1none 25/35 -> v1on 18/36, the keys lifting the world's luminance p75 over the
        /// enemies and hazards): a moved decor piece's largest channel at most this (Colour56.Clamp; NIGHT56's Colour56AccentMaxChannel 1.0 with
        /// -skygrindOff57): the accent keeps its hue and saturation at ~0.6 of its value, under the world's upper quarter (n57-r1 v1none p75 0.13-0.41).</summary>
        public const float Colour57AccentMaxChannel = 0.6f;

        // ---------------- rim56 (Game/Rim56.cs), NIGHT57's one attempt ----------------

        /// <summary>rim56 on NIGHT57: skin52's lit side and shadow side x this over NIGHT56's (lit 1.5 -> 2.2, shade 0.70 / 0.38 / 0.30 -> 1.03 / 0.56 /
        /// 0.44: the same shadow-to-lit ratio, the body's hue kept). The body is unlit (_BaseColor, then MixFog, then the post's EV / ACES): its value
        /// at 8-20 m is the fog's and the post's, so the HDR gain is what reaches the frame (a rough ACES estimate puts a body at V ~0.60 near ~0.69;
        /// not measured). n56-r5 foundry with colour56's cap simulated: the bodies' S over the environment's p90 (0.70) already; V 0.57-0.62 under
        /// its 0.67-0.69. Risk: up close the lit side over the bloom threshold glows more (skin52's "jelly" cut was the rim, not the body).</summary>
        public const float Rim57Gain = 1.47f;

        // ---------------- the a357 reel (FeelReel.N57.Colour.cs) ----------------

        /// <summary>a357: frames the reel waits after hiding the cap before the -nocap twin is shot (the pass is enqueued per frame: the next frame is
        /// already without it; one more for the capture's end of frame).</summary>
        public const int Cap57TwinFrames = 1;
    }
}
