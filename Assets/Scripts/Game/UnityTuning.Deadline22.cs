// NIGHT22 U3-36.5 DEADLINE bar (builder S): Unity-only layout and timing numbers for the fuse bar (UiDeadline22.cs). UNCOMPILED when
// written (the builder has no Editor). The fuse's own numbers (length, refills, the burn's hit) are design.n21.deadline.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const float Deadline22Top = 100f; // px below the top edge (1080p reference): under the round / enemy row, over the banner (about 170)
        public const float Deadline22Width = 560f; // px: the bar
        public const float Deadline22Height = 64f; // px: the bar (the number is the bar: 42 px text centred on it)
        public const float Deadline22Inset = 6f; // px: the INK rim round the fill
        public const double Deadline22InTime = 0.2; // s: the bar's entrance (alpha and a drop from above; M9's 90-300 ms)
        public const float Deadline22InDrop = 30f; // px: the entrance's drop
        public const double Deadline22PulseSeconds = 5; // s of fuse left under which the bar pulses (RESEARCH R5: red under 10 s and a pulse on a clock that is mostly long; ours is 18 s, so 5 s is the warning: our tuning)
        public const double Deadline22PulseHz = 3; // pulses a second in the last seconds
        public const float Deadline22PulseMix = 0.35f; // how far the fill goes toward white at a pulse's peak (never off the DANGER hue family)
        public const float Deadline22PulseScale = 0.1f; // the number's scale at a pulse's peak, as an extra on 1
        public const int Deadline22Pops = 3; // "+N" pops alive at once (a kill chain pays several in a second)
        public const double Deadline22PopLife = 0.8; // s: a pop's rise and fade
        public const double Deadline22PopIn = 0.12; // s of fade-in for a "+N" pop (NIGHT23: M9 read its first frame at full alpha as a pop-in; the web has no such pop)
        public const float Deadline22PopRise = 46f; // px a pop rises
        public const double Deadline22FlashTime = 0.35; // s: the burn flash
        public const float Deadline22FlashAlpha = 0.8f; // the burn flash's peak alpha (times FLASH INTENSITY, through the flash gate)
    }
}
