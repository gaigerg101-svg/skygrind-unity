// NIGHT27 builder P: Unity-only view numbers of the jump pad's look (Game/JumpPadView27.cs). View only: nothing here touches the sim; the pad's radius, target and
// launch are the web's (config jumpPad.*, map.pads). The web had one flat ring, so none of these has a web number.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>Height of the dark body disc's top above the pad's floor point (m): a raised plate you can read from the side.</summary>
        public const float Pad27BodyTop = 0.14f;
        /// <summary>The rim's top (m) and its width outside the pad radius (m): the dark ring that frames the glow.</summary>
        public const float Pad27RimTop = 0.17f, Pad27RimWidth = 0.1f;
        /// <summary>The glow fill sits this high (m); the glow ring and the chevrons a little higher (each layer above the last so the additive layers never z-fight).</summary>
        public const float Pad27FillY = 0.16f, Pad27RingY = 0.17f, Pad27ChevronY = 0.19f;
        /// <summary>The glow ring's band, as fractions of the pad radius (inner, outer); the fill reaches the inner fraction.</summary>
        public const float Pad27RingInner = 0.86f, Pad27RingOuter = 0.97f;
        /// <summary>Dark body and rim colours (dusk slate, near black): the dark that makes the cyan top read.</summary>
        public static readonly UnityEngine.Color Pad27BodyColor = new UnityEngine.Color(0.14f, 0.24f, 0.32f, 1f), Pad27RimColor = new UnityEngine.Color(0.5f, 0.68f, 0.78f, 1f);
        /// <summary>The Player reel read the first version at pad 0.164 vs floor 0.478 (ratio 0.34): the body and rim are lighter (the rim a pale steel, not black) and every glow is far above 1 in HDR.</summary>
        /// <summary>The chevrons and ring mix this much white into the cyan (cyan-white reads brighter and is still cyan-led).</summary>
        public const float Pad27WhiteMix = 0.45f;
        /// <summary>The floor halo: from the rim out to this multiple of the pad radius, at this HDR and height (m); the expanding ring pulse: its HDR, width (x radius), end scale (x radius), height.</summary>
        public const float Pad27HaloOuter = 1.7f, Pad27HaloHdr = 1.1f, Pad27HaloY = 0.05f;
        public const float Pad27WaveHdr = 5f, Pad27WaveWidth = 0.1f, Pad27WaveEnd = 1.9f;
        /// <summary>The beacon: a tall faint beam of this height (m) in segments, each segment's HDR (fading with height), its half width as a fraction of the pad radius.</summary>
        public const float Pad27BeamHeight = 7f, Pad27BeamHalf = 0.35f;
        public static readonly float[] Pad27BeamHdr = { 2.2f, 1.4f, 0.8f, 0.4f };
        /// <summary>HDR multipliers on colors.usable: the fill (low, additive over the dark body), the ring, the chevrons (brightest: the arrows are the read).</summary>
        public const float Pad27FillHdr = 4.5f, Pad27RingHdr = 9f, Pad27ChevronHdr = 12f;
        /// <summary>The slow glow pulse: its period in seconds and the multiplier's low and high ends (the idle brightness swings between them).</summary>
        public const float Pad27PulsePeriod = 1.7f, Pad27PulseLow = 0.7f, Pad27PulseHigh = 1.0f;
        /// <summary>The launch burst: the added glow multiplier at its peak, its decay in seconds, and the particles it throws.</summary>
        public const float Pad27BurstGlow = 1.2f, Pad27BurstSeconds = 0.55f;
        public const int Pad27BurstParticles = 24;
        /// <summary>Chevrons per pad, the sweep rate (cycles per second), and the rising chevrons' travel height (m) for an unaimed pad.</summary>
        public const int Pad27Chevrons = 3;
        public const float Pad27SweepRate = 0.9f, Pad27RiseHeight = 1.9f;
        /// <summary>The chevron size as a fraction of the pad radius (half-width of the V), its arm thickness as a fraction of that, and the stride between chevrons.</summary>
        public const float Pad27ChevronSize = 0.34f, Pad27ChevronThick = 0.28f, Pad27ChevronStride = 0.42f;
        /// <summary>The upward stream: particles a second, life (s), speed (m/s), size (m) and the cap.</summary>
        public const float Pad27StreamRate = 12f, Pad27StreamLife = 1.5f, Pad27StreamSpeed = 2.0f, Pad27StreamSize = 0.13f;
        public const int Pad27StreamMax = 48;

        // The reel scenario `jumppad` (Game/FeelReel.N27.Pad.cs).
        /// <summary>Camera distance from the pad (m), the pixel contrast the pad must reach over the floor, the pulse swing it must show, and the frames it samples.</summary>
        public const double Pad27ReelDistance = 8.0, Pad27ReelMinContrast = 2.0, Pad27ReelMinPulseRatio = 1.2;
        public const int Pad27ReelFrames = 4;
    }
}
