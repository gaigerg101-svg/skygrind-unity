// NIGHT37 builder O (loom37): THE LOOM as Loom37View draws it. Visual only: the sim never reads these (its numbers are design.n37.loom37).
// Dusk palette (rule 8): the body and the legs are dark iron; the knees of the leg's pair glow CYAN (grapple nodes and the weak point: player-usable),
// the other knees a pale violet-white; nails in a knee show as warm points; the spinneret is shut iron until the CYAN window (Boss37Glow); the
// lash's line and the web wall are red-orange (dodge); the spool is YELLOW (parry); a thread about to be cut pulses red-orange; the threads
// from the body to the anchors are pale violet-white.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The body and leg iron (opaque _BaseColor).</summary>
        public const uint Loom37IronHex = 0x2b2633, Loom37IronDarkHex = 0x1a1620;
        /// <summary>The body's size (m: x, y, z) as drawn (the sim's hit sphere is design.n37.loom37.bodyHalfW).</summary>
        public const float Loom37BodyW = 6.4f, Loom37BodyH = 3.0f, Loom37BodyL = 8.0f;
        /// <summary>A leg segment's thickness (m).</summary>
        public const float Loom37LegThick = 0.55f;
        /// <summary>The knee's glow colours and gains: the live pair (cyan), the others (pale), a nail (warm), and the knee sphere's drawn scale x the hit radius.</summary>
        public const uint Loom37KneeLiveHex = 0x36e0ff, Loom37KneeIdleHex = 0xd8d0ff, Loom37NailHex = 0xffb040;
        public const float Loom37KneeLiveGain = 2.2f, Loom37KneeIdleGain = 0.6f, Loom37KneeScale = 0.85f, Loom37NailGainPer = 0.35f;
        /// <summary>The spinneret's drawn radius (m) and its shut colour.</summary>
        public const float Loom37SpinR = 1.2f;
        /// <summary>The red-orange danger colour (the lash, the web, a cut's pulse), its gains (tell / live), the lash's drawn thickness (m).</summary>
        public const uint Loom37DangerHex = 0xff5a1f;
        public const float Loom37TellGain = 1.2f, Loom37LiveGain = 3.0f, Loom37LashThick = 0.35f;
        /// <summary>The spool (YELLOW: parry it), its drawn radius (m), a parried spool's colour (cyan).</summary>
        public const uint Loom37SpoolHex = 0xffe14a;
        public const float Loom37SpoolR = 0.6f, Loom37SpoolGain = 2.4f;
        /// <summary>The threads from the body to the anchors (pale), their thickness (m) and gain.</summary>
        public const uint Loom37ThreadHex = 0xd8d0ff;
        public const float Loom37ThreadThick = 0.06f, Loom37ThreadGain = 0.5f;
        /// <summary>A cut thread anchor's pulse: the sphere radius (m) and the pulse rate (Hz).</summary>
        public const float Loom37CutR = 1.0f, Loom37CutHz = 6f;
        /// <summary>The window glow's radius on the spinneret (m).</summary>
        public const float Loom37GlowR = 1.6f;
        /// <summary>The body's warm under-light (range m, intensity).</summary>
        public const float Loom37LightRange = 24f, Loom37LightIntensity = 2.2f;
    }
}
