// NIGHT29 item 3 (builder V): the boss gap bar's and the DOORS OPEN tag's layout (Game/Hud29.cs). CLAUDE.md rule 5: each with a
// comment. Unity-only UI layout; the gap's own numbers (hurt line, fail gap, scale) are written by the lead / builders into
// Hud29's statics from the sim's config, never typed here.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const int Hud29Sorting = 11; // canvas order: over the HUD's own canvas (10), under the impact frame and the flash canvases
        public const float Hud29RefW = 1920f, Hud29RefH = 1080f; // px: the canvas scaler's reference, so the sizes below are px at 1080p (rule 8: text 16 px or more)
        public const float Hud29GapW = 560f, Hud29GapBarH = 14f; // px: the gap bar's track
        public const float Hud29GapTop = 150f; // px from the top: its label stands 26 px above the plate, so 150 clears the ROUND banner (bottom edge about 112 px at 1080p; it was 110 and the label overlapped it)
        public const float Hud29GapTextPx = 20f; // px: its label
        public const double Hud29GapFallbackMax = 50; // m: the bar's full length when Hud29.BossGapMax was never set (a boss that sets none still shows a bar)
        public const float Hud29GapEase = 14f; // 1/s: the fill follows the gap with this rate (frame time), so a jump in the gap does not snap
        public const float Hud29TagTextPx = 20f; // px: the DOORS OPEN tag
        public const float Hud29TagMargin = 24f; // px: its distance from the top-left corner
        public const float Hud29TagPadY = 6f; // px: its plate's padding
        public const string Hud29TagText = "DOORS OPEN"; // the tag's words (design D-6: the run is UNRANKED while it shows)
    }
}
