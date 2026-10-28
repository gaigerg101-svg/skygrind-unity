// NIGHT14 U3-27 (the shared types): agent V2's Unity-only numbers, U3-27.10 / U3-27.11 (THE ROUTE's map, the card's layout, the ghost's look, the splits line). Each with a comment saying why
// the web has no value for it (CLAUDE.md rule 5).
// The web's RESULTS has no map, it saves no image and has no ghost or splits, so none of these has a web value: they are
// layout, raster and storage numbers. The colours themselves come from config (colors.usable for the path and the ghost,
// colors.enemy for the DOWN, barrier.boundaryColor for the arena's outline, ui.theme.* for plates, text and the kills'
// gold); the speed ramp's ends are the HUD's speed tiers (hud.speedHigh). No UnityEngine here (RouteTrack reads it).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- storage ------------------------------------------------------------------------------------------------------
        // The GHOST's folder under the profile's (Application.persistentDataPath): one <seed code>.bin per seed. The web has
        // no ghosts (and no files: its localStorage holds settings, records and challenges).
        public const string GhostFolderName = "ghosts";
        // The guns a run's TOP GUN table can tell apart (KillEvent.weapon ids: the catalog's guns, the punch, the orb, the
        // drops' shockwave, the traps...; a gun past this many is left uncounted). Preallocated with the track.
        public const int RouteGunSlots = 48;

        // --- THE ROUTE on RESULTS (1080p px from the screen centre) -------------------------------------------------------
        // The plate: from the tally's buttons' right (they end at x +10) to the PAYOFF strip's right edge (x +900), from
        // under the strip (its bottom is y -250) to 10 px over the screen's bottom edge (-540).
        public const float RoutePlateX = 40f;
        public const float RoutePlateTop = -262f;
        public const float RoutePlateBottom = -530f;
        // Without the staged tally (design.ceremony.resultsTally off) RESULTS' buttons sit at x 0 (420 px wide): the plate
        // starts right of them.
        public const float RoutePlateXPlain = 240f;
        // Inside the plate: the padding, the map's square (the plate's height less the padding), the gap to the legend.
        public const float RoutePad = 10f;
        public const float RouteMapPx = 248f;
        public const float RouteLegendGap = 16f;
        // The legend's rows (their pitch, the icon's box, the speed ramp's bar) and the room the legend needs (else hidden).
        public const float RouteLegendLine = 28f;
        public const float RouteLegendIcon = 18f;
        public const float RouteRampWidth = 84f;
        public const float RouteRampHeight = 8f;
        public const float RouteLegendMinWidth = 200f;
        // SAVE CARD and COPY SEED at the plate's right, stacked (the theme's button is 420 x 54: narrower here).
        public const float RouteButtonWidth = 280f;
        public const float RouteButtonHeight = 54f;
        public const float RouteButtonGap = 14f;

        // --- the map's raster (px of a texture RouteMapTexture wide; the card's map scales these by its size / 512) ----------
        // The texture's size: about twice the plate's map square, so a 3 px path reads as a crisp 1.5 px line.
        public const int RouteMapTexture = 512;
        // Metres of margin round the arena's floor (the outline isn't cut by the texture's edge).
        public const double RouteMapMargin = 3;
        // The map's plate (ui.theme.panel) and the floor's tint (barrier.boundaryColor), alphas.
        public const float RoutePlateAlpha = 0.94f;
        public const float RouteFloorAlpha = 0.07f;
        // The map's boxes (walls, bridges, cover, slabs, spires, ledges, islands; ui.theme.text): their footprints filled
        // from the low alpha (the lowest top) to the high one (the highest), their edges a little stronger.
        public const float RouteBoxAlphaLow = 0.08f;
        public const float RouteBoxAlphaHigh = 0.30f;
        public const float RouteBoxEdgeAlpha = 0.34f;
        // The zones' borders (where the ground's zone changes; barrier.boundaryColor) and the arena's outline (its width, px).
        public const float RouteZoneEdgeAlpha = 0.22f;
        public const float RouteBoundaryPx = 3f;
        // The path: its width (px), alpha and the cyan ramp's ends: slow (at rest) = colors.usable x RampDim, colors.usable
        // itself at hud.speedMid, fast (hud.speedHigh and up) = colors.usable toward white by RampWhite.
        public const float RoutePathPx = 3f;
        public const float RoutePathAlpha = 0.95f;
        public const float RouteRampDim = 0.32f;
        public const float RouteRampWhite = 0.45f;
        // The kills (small dots in ui.theme.gold: where your points came from; red-orange stays the danger's), the round
        // ticks (ui.theme.text, across the path), the DOWN (an X in colors.enemy over a dark halo), the start (a ring in
        // ui.theme.text) and the exfil site (a ring in colors.usable, design.exfil.siteRadius wide).
        public const float RouteKillPx = 2.6f;
        public const float RouteKillAlpha = 0.85f;
        public const float RouteTickPx = 11f;
        public const float RouteTickWidthPx = 2.4f;
        public const float RouteDownPx = 10f;
        public const float RouteDownWidthPx = 3.2f;
        public const float RouteHaloPx = 2f;
        public const float RouteStartPx = 7f;
        public const float RouteRingWidthPx = 2.2f;

        // --- the SHARE CARD (a PNG; px of the image) ----------------------------------------------------------------------
        // 1200 x 630: the 1.91:1 size link previews and social posts show without a crop.
        public const int RouteCardWidth = 1200;
        public const int RouteCardHeight = 630;
        // The map's square on the card (and its texture), the padding, the gap between the map and the text column.
        public const int RouteCardMapPx = 570;
        public const float RouteCardPad = 30f;
        public const float RouteCardGap = 40f;
        // The text column's sizes: the wordmark, a big number, a row's label and value, the small lines (seed, stamp).
        public const float RouteCardWordmark = 64f;
        public const float RouteCardBig = 72f;
        public const float RouteCardLabel = 20f;
        public const float RouteCardValue = 34f;
        public const float RouteCardSmall = 20f;
        public const float RouteCardRow = 76f;
        // The card's off-screen rig: a world-space canvas this far below the map (beyond every camera's far plane and the
        // kill plane), drawn by its own camera on Unity's built-in UI layer (5), only while SAVE CARD renders it.
        public const float RouteCardDepth = -20000f;
        public const int RouteCardLayer = 5;
        // The folder under My Pictures and the file's prefix: SKYGRIND-<seed code>-<yyyyMMdd-HHmmss>.png.
        public const string RouteCardFolder = "SKYGRIND";
        public const string RouteCardPrefix = "SKYGRIND-";

        // --- the GHOST (D-53: the player's cyan at design.ghost.alpha, a pale trail) ----------------------------------------
        // The runner's body: a cylinder of the player's capsule (movement.body: radius, height) with this many sides.
        public const int GhostBodySides = 14;
        // The trail: this many segments over design.ghost.trailSeconds, each a thin tube (radius m) at the body's middle
        // height, colors.usable toward white by TrailPale, its alpha falling from design.ghost.alpha to 0 along it.
        public const int GhostTrailSegments = 12;
        public const float GhostTrailRadius = 0.06f;
        public const float GhostTrailPale = 0.5f;

        // --- SPLITS (the HUD line at each round's start) ---------------------------------------------------------------------
        // Under the round line and EXPOSED (top centre: hud.edgeMargin + this), its box, how long it shows and its fade.
        public const float GhostSplitBelow = 72f;
        public const float GhostSplitWidth = 700f;
        public const float GhostSplitHeight = 30f;
        public const double GhostSplitShow = 4.0;
        public const double GhostSplitFade = 0.5;
    }
}
