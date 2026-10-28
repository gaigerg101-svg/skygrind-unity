// NIGHT24 (builder F): the view numbers of the reticle arc (design.n24.reticleArc) and the BIG PICK screen (design.n24.bigPick). Unity-only
// (render and UI layout: CLAUDE.md rule 5); every gameplay number of both rows is in Design/unity-config.json. The reticle numbers are R3 section B2's
// inference ("26-48 px under the crosshair, about 140 x 5 px at 1080p, idle alpha 0.15-0.25, ready flash <= 250 ms"), kept as printed.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the reticle arc (UiReticleArc24.cs) ------------------------------------------------------------------------------------------
        // The bar's overall width and thickness in px at 1080p (the contract: about 140 x 5), the radius of the curve (a smile under the crosshair:
        // the ends sit higher than the middle by R x (1 - cos(asin(70 / R))) = about 14 px at 180).
        public const float Arc24Width = 140f, Arc24Thick = 5f, Arc24Radius = 180f;
        // Px under the crosshair's centre to the arc's ENDS and to its lowest point (both inside the contract's 26-48 px).
        public const float Arc24EndsBelow = 28f, Arc24MidBelow = 42f;
        // The three groups left to right: ARCLINE recharge, DASH pips, FIST charge; their widths and the gap between groups / cells (px).
        public const float Arc24WidthArcline = 52f, Arc24WidthDash = 36f, Arc24WidthFist = 44f, Arc24GroupGap = 4f, Arc24CellGap = 2f;
        // Alphas: at rest (nothing recharging: 0.15-0.25), while a group recharges or the fist holds a charge (R3 B2: dim 35-45 % while recharging),
        // and the ready flash's peak.
        public const float Arc24IdleAlpha = 0.2f, Arc24ChargeAlpha = 0.4f, Arc24FlashAlpha = 1f;
        // The ready flash: in / out seconds (R3 B2: 120 + 130 ms = 250 ms, the contract's "250 ms or less").
        public const float Arc24FlashIn = 0.12f, Arc24FlashOut = 0.13f;
        // The first show fades in over this many seconds (M9: no pop-in).
        public const float Arc24FadeIn = 0.25f;
        // The probe names the reel reads.
        public const string Arc24ProbeReady = "reticleArcReady";

        // --- the BIG PICK screen (UiBigPick24.cs) ------------------------------------------------------------------------------------------
        // The screen is the MODS draft's own (cards, sizes and entrance: UnityTuning.ModCard*, the draft's theme text sizes S24 / S32 / S56; the
        // contract's floor is 18 px). This is the countdown line under the cards; the default pick's seconds are config (design.n24.bigPick).
        public const string BigPick24Timer = "PICKS CARD 1 IN {0:0} s";

        // --- the campaign's door cue and room banner (UiCampaignCue24.cs) ---------------------------------------------------------------------
        // Px at 1080p from the top centre: the door cue under the round banner (two lines of Cue24LineH), the room banner above it.
        public const float Cue24Y = -150f, Cue24W = 560f, Cue24LineH = 32f, RoomBanner24Y = -90f, RoomBanner24W = 720f, RoomBanner24H = 44f;
    }
}
