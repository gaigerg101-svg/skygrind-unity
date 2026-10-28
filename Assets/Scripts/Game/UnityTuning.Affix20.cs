// NIGHT20 U3-34.9 C3 (builder E owns this file): the affixes' tells as affixes.js shows them through the web's telegraph
// (telegraph.circle for VOLATILE's fuse, telegraph.flash for a blink). Visual only: the sim never reads them.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>VOLATILE's fuse disc: colors.enemy (DANGER) x this (the web's circle reads as a warm warning fill).</summary>
        public const float AffixFuseGain = 1.2f;
        /// <summary>A blink's flash: white x this, AffixBlinkSize m wide (telegraph.flash(x, y, z, 0.9)), fading over AffixBlinkFlash s.</summary>
        public const float AffixBlinkGain = 2f, AffixBlinkSize = 0.9f, AffixBlinkFlash = 0.25f;
    }
}
