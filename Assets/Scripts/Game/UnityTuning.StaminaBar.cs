// NIGHT17 U3-30.4 round 5 (builder M): the stamina bar's look at a glance (UiStaminaBar.cs, StaminaBarLook). Its size and
// place stay Hud17's (160 x 9 px, 46 px under the crosshair).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const double StaminaSpendChunk = 0.05; // bar: the least drop in one frame that counts as spent (the sprint drains under 0.02 a frame at 20 fps)
        public const int StaminaPulseLevels = 12; // the pulse's colour levels (set only when the level changes)
        public const float StaminaPulseAlpha = 0.95f; // the pulse's peak: the track at CYAN-DEEP at this alpha (from the hollow INK-SOFT)
    }
}
