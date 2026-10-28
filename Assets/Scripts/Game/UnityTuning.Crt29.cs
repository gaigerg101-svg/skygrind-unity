// NIGHT29 item 4 (builder V): the CRT overlay's Unity-only numbers (Game/Crt29*.cs, Game/Render/Crt29Pass.cs). CLAUDE.md
// rule 5: each with a comment. The look's own numbers (warp, scanlines, chroma, grain, vignette, pixel step, the default
// strength) are the design.n29.crt row of Design/unity-config.json (the web has no CRT); only plumbing lives here.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const string CrtSetting = "crtStrength"; // the VIDEO row's key: the overlay's strength, 0-1 (shown 0-100 %)
        public const string CrtLabel = "CRT"; // the row's label
        public const string CrtDesc = "A retro-TV look over the 3D image: scanlines, a light warp and grain (0 = OFF; the HUD stays sharp)"; // its line
        public const double CrtStep = 0.05; // the slider's step: 5 % (20 stops)
        public const string CrtOverlaySetting = "crtOverlay"; // the ACCESS toggle's key: the whole overlay on or off, whatever the strength
        public const string CrtOverlayLabel = "CRT OVERLAY"; // its label
        public const string CrtOverlayDesc = "Turn the retro-TV overlay off completely (grain flicker, warp and scanlines)"; // its line
        public const bool CrtOverlayDefault = true; // on: the strength row decides (default 35 %); a player who needs it off flips this
        public const double CrtPixelRefHeight = 1440; // px: design.n29.crt.pixelStep counts screen pixels at this height (R4b: 2 px at 1440p is the 720p look)
        public const double CrtGrainWrap = 4096; // grain step indices wrap here so the uint hash input stays small (no visible repeat at 24 Hz: 170 s)
        public const float CrtVignetteFrom = 0.4f; // the vignette starts at this distance from the centre (centre = 0, corner = 1.414) ...
        public const float CrtVignetteTo = 1.41f; // ... and is whole at the corner
        public const float CrtScanMean = 0.5f; // the scanline wave's mean darkening share (1 - s averages 0.5): the brightness compensation divides by 1 - intensity x this
    }
}
