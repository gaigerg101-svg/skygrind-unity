// NIGHT16 polish #36 (builder V): the ADS HUD's literals (POLISH V2, a web port: the web's CSS in hud.js :74-82 and setAds
// :752-770 hold them as literals, not config, so they live here; CLAUDE.md rule 5). Every gameplay-facing number (the
// zoom, the reticle kind, the scope's vignette, the rangefinder's reach) is config: ads.weapons.<id>.reticle / .scope,
// ads.scopeVignette, weapons.rail.range. vh: 1 vh = ui.theme.scaleBaseHeight / 100 canvas units (1080 at 1080p).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // hud.js :74-75 .hud-adsring: a 30 px circle, a 1 px border in the crosshair's colour, a 1 px dark outline
        // (box-shadow 0 0 0 1px rgba(0,0,0,0.35))
        public const float AdsRingDiameter = 30; // px (canvas units)
        public const float AdsRingBorder = 1; // px
        public const float AdsRingOutline = 1; // px, outside the border
        public const float AdsRingOutlineAlpha = 0.35f;
        // hud.js :762 setAds: transform scale(1.6 - 0.6 x the ring's opacity)
        public const double AdsRingScaleFrom = 1.6, AdsRingScaleBy = 0.6;

        // hud.js :76-77 .hud-scope: radial-gradient(circle at center, clear to 31vh, rgba(4,4,10,0.55) at 33vh,
        // rgba(4,4,10,ads.scopeVignette) at 45vh and beyond)
        public const double AdsScopeClearVh = 31, AdsScopeEdgeVh = 33, AdsScopeFullVh = 45;
        public const double AdsScopeEdgeAlpha = 0.55;
        public const byte AdsScopeR = 4, AdsScopeG = 4, AdsScopeB = 10;
        public const int AdsScopeTexture = 256; // texels across the gradient's 90 vh square (a render-only resolution)
        // hud.js :78-79 .hud-scope i: two 1 px lines 62 vh long, rgba(255,255,255,0.55), crossing at the centre
        public const double AdsScopeLineVh = 62;
        public const float AdsScopeLineWidth = 1; // px
        public const float AdsScopeLineAlpha = 0.55f;

        // hud.js :80-81 .hud-range: t-body (ui.theme.type.body), letter-spacing 0.14em, #dff9ff, its top edge 24 px under
        // the centre, centred across
        public const float AdsRangeTop = 24; // px under the centre
        public const float AdsRangeSpacingEm = 0.14f;
        public const uint AdsRangeHex = 0xdff9ff;
        public const float AdsRangeBoxWidth = 240; // px, the label's box (wide enough for "250.0 M"; render-only)
        // hud.js :765: the range shows from the zoom's amount above this (and a hit)
        public const double AdsRangeFromAmount = 0.5;

        // hud.js :63 / :764: the crosshair's arms fade to 1 - 0.6 k while zoomed (--tick; the dot stays)
        public const double AdsTickFade = 0.6;
    }
}
