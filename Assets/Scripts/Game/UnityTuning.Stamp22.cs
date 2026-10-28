// NIGHT22 U3-36.4 THE STAMP (builder S): Unity-only layout numbers for the inked round stamp (UiStamp22.cs). UNCOMPILED when written
// (the builder has no Editor). The stamp's motion numbers (entrance, hold, exit, scales) are design.n21.stamp (Design/unity-config.json).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const float Stamp22Y = 200f; // px above the screen's middle (1080p reference): under the ROUND banner (about +300 and up), over the crosshair's rows
        public const float Stamp22Plate = 200f; // px: the INK plate under the letter (a square, slanted and cut like the theme's plates)
        public const float Stamp22TiltDeg = -7f; // deg: a rubber stamp is never quite level
        public const float Stamp22InkOffset = 8f; // px: the hard INK shadow of the letter, down-right (the theme's shadow, bigger: the letter is 133 px)
        public const float Stamp22EdgeOffset = 3f; // px: the pale edge light up-left of the letter (the Player-safe replacement for the UIEffect shine, DECIDE22 D-7)
        public const float Stamp22EdgeAlpha = 0.7f; // the edge light's alpha
        public const float Stamp22SheenAlpha = 0.45f; // the rank letter's sheen band (D-7's Player-safe shine): CYAN-HOT at this alpha, once per rank-up
        public const float Stamp22LabelGap = 14f; // px: the "ROUND n" label under the plate
    }
}
