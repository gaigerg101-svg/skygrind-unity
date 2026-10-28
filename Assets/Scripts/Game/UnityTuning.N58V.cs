// NIGHT58 builder V: Unity-only numbers of tonight's view work (render, capture), each with its reason. No gameplay number here.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---------------- rim56 (Game/Rim58.N58V.cs), NIGHT58 V1 ----------------

        /// <summary>rim56 on NIGHT58: colour56's accent (decor54's rims and trims, Colour56.Clamp) at x this of its largest channel while rim56 is
        /// on (0.6 -> 0.27 linear). The cause (n58-v-rimcause.py, n57-r3 foundry): the accent sets the environment's p90 V the bodies are judged
        /// against. Simulated on the stills (n58-v-rimsim.py): the accent's sRGB value x 0.75 takes the bodies over the env p90 from 75 % to 91 %;
        /// a linear x 0.45 is ~x 0.70 in sRGB before the tonemap (2.2 gamma; the tonemap's own curve not modelled), a margin under 0.75.</summary>
        public const float Rim58AccentScale = 0.45f;

        // ---------------- the exit lamp (Game/Exit58.N58V.cs), NIGHT58 V3: n57-kiln's exits lit on the clear ----------------

        // exit lamp: its switch is the view row design.n60.lamp60 (NIGHT60 V, B8; Exit58.Off): the compile-time Exit58On (false) is gone.

        /// <summary>exit lamp: the frame's posts and lintel thickness (m): ~2.4x Doors38EdgeW (0.09), so the lit frame reads at 20-30 m over the
        /// curtain's own thin edge.</summary>
        public const float Exit58FrameW = 0.22f;
        /// <summary>exit lamp: the frame stands this far (m) outside the opening, clear of the curtain's posts and of the door's collider.</summary>
        public const float Exit58FrameOut = 0.15f;
        /// <summary>exit lamp: HDR multiplier on colors.usable for the additive frame (Doors38Glow is 2.4): over the bloom threshold, so the frame
        /// glows; the MAIN is the exit's luma >= 1.5x the room's median at the clear frame.</summary>
        public const float Exit58Glow = 3.2f;
        /// <summary>exit lamp: the point light's intensity (URP units; Doors38LightIntensity 3) and range (m; Doors38LightRange 7): the floor and
        /// walls round the exit take the cyan, the rest of the room stays as it was (the guard: nothing else brighter).</summary>
        public const float Exit58LightIntensity = 4f, Exit58LightRange = 6f;
        /// <summary>exit lamp: the light stands this far (m) back into the room being left, at 0.6 of the door's height.</summary>
        public const double Exit58LightBack = 1.5;
        /// <summary>exit lamp: seconds of the one rise from dark to lit when the door opens (no flash: CLAUDE.md rule 8, <= 3 flashes a second).</summary>
        public const float Exit58FadeS = 0.35f;
    }
}
