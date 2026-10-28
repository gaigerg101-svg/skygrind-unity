// NIGHT37 builder K (boss37's shared view helpers, Game/Boss37Kit.View.cs): render and UI numbers only (CLAUDE.md rule 5: Unity-only, each with
// a comment). Every px is at 1080p (the HUD canvas scales with the height).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the CYAN window glow (a shell + a light on the boss's weak point while Boss37Brain.Open) ---------------------------------------
        public const uint Kit37CyanHex = 0x36E0FF; // the open core's cyan (= Body36OpenHex: rule 8, cyan = player-usable: "your window")
        public const float Kit37GlowGain = 2.2f; // the shell's colour gain at the pulse's peak (additive: over 1 so it blooms past the boss's own rim)
        public const float Kit37GlowPulseHz = 3f; // pulses per second while open (fast enough to read as "now", slower than a tell's flicker)
        public const float Kit37GlowPulseLow = 0.55f; // the pulse's trough as a share of the peak (never dark while open)
        public const float Kit37GlowEndShare = 0.35f; // the glow shrinks to this share of its brightness by the window's end (the window running out reads)
        public const float Kit37GlowScale = 1.35f; // the shell's diameter as a multiple of the radius the boss gives (a halo just outside the weak point)
        public const float Kit37GlowFadeS = 0.25f; // seconds to fade out after the window closes (no pop)
        public const float Kit37GlowOpenKick = 1.6f; // the shell's scale kick at the window's opening (a ring that snaps out, then settles)
        public const float Kit37GlowKickS = 0.2f; // seconds the opening kick takes to settle
        public const float Kit37LightIntensity = 6f; // the cyan point light's intensity at the peak (lights the arena around the open core)
        public const float Kit37LightRange = 14f; // metres: the light's range (the core's neighbourhood: walls within a ride)
        public const int Kit37OpenSparks = 40; // sparks at the window's opening
        public const float Kit37OpenSparkSpeed = 9f; // m/s
        public const float Kit37OpenSparkLife = 0.5f; // s
        public const float Kit37OpenSparkSize = 0.3f; // m

        // --- the stagger bar (under the HUD33 boss bar) ------------------------------------------------------------------------------------
        public const float Kit37BarGap = 4f; // px under the 6 px boss bar
        public const float Kit37BarH = 4f; // px: thinner than the boss bar (secondary: the HP bar stays the main read)
        public const float Kit37BarTrackAlpha = 0.22f; // the track's bone alpha (the empty part)
        public const float Kit37BarFillAlpha = 0.85f; // the meter's bone alpha
        public const float Kit37BarDrainFlashS = 0.18f; // seconds a drain flashes the meter white-hot (each right move reads on the bar)
        public const float Kit37BarFindEveryS = 1f; // seconds between searches for the HUD's boss bar while it is not found (it is built once per HUD)
    }
}
