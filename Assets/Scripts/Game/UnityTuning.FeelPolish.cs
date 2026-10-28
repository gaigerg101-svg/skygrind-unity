// NIGHT16 U3-29.8 (the lead's polish queue): render-only numbers of the feel polish (FEEL-DESIGN.md section 5). The web
// has none of these: its land puff is one fixed burst above fx.landPuffSpeed and a slide leaves nothing behind
// (main.js :1522-1529), so each number here is the port's own, with why.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the landing (MovementFx; polish #4) ------------------------------------------------------------------------
        // The fall speed (m/s) at which a landing reads at its fullest: a 6 m drop lands at about 13 m/s, a dash-jump off a
        // ledge 15-20, a fall from THE CRADLE's top 25+; the web's puff starts at fx.landPuffSpeed (12).
        public const double LandFullFallSpeed = 26;
        // At the fullest landing the puff carries this many extra x fx.landPuffCount cubes and spreads this much faster
        // (x fx.landPuffSpread), so a big drop reads bigger than a hop off a step.
        public const double LandPuffExtra = 1.0, LandPuffSpreadExtra = 0.8;
        // A flat dust ring at the feet (the slam ring's mesh and material, not on a slam's own landing): its radius (m) from
        // a 12 m/s landing to the fullest, and its alpha x fx.slamRingOpacity (fainter than a slam's: a landing isn't an attack).
        public const double LandRingRadiusMin = 1.0, LandRingRadiusMax = 2.6;
        // Build 2's land strip (a 22.4 m/s landing) didn't show the ring at 0.5 (0.3 effective on the pink floor): 0.8.
        public const double LandRingAlpha = 0.8;

        // --- the slide's heel dust (MovementFx; polish #5) -------------------------------------------------------------
        // Dust cubes a second from the heels while sliding on the ground faster than a walk, scaling from 0 at
        // ground.walkSpeed to SlideDustRate at SlideDustFullSpeed (m/s: the slide's floor 11.6 sits at about a third; a
        // slope-boosted slide fills it). Each is one colors.slamDust cube, life x fx.landPuffLife, size SlideDustSize, tossed
        // at SlideDustSpeed (m/s), dropped SlideDustBehind m behind the feet along the slide.
        public const double SlideDustRate = 36, SlideDustFullSpeed = 20;
        public const float SlideDustSize = 0.8f, SlideDustSpeed = 1.6f, SlideDustBehind = 0.35f;

        // --- the menus' focus frame (UiNav; polish #8) ------------------------------------------------------------------
        // The cyan frame slides from the last control to the new one in this long (s, ease-out cubic): the eye follows the
        // move between rows, and the selection (so the next press) has already moved; the web's menus.js just repaints.
        public const float NavFocusSlideTime = 0.08f;
        // A pressed menu button dips to this scale at once and eases back to full over PressTime (s): a 4 % dip reads as a
        // press without moving the label off its grid; the web's buttons only recolour on :active.
        public const float PressScale = 0.96f, PressTime = 0.12f;

        // --- the round-clear beat (PostFx; polish #9) --------------------------------------------------------------------
        // The vignette's intensity added (URP's 0..1 scale) while the round's last kill slows time: the edges dim a little so
        // the moment reads as a beat; up in BeatVignetteIn s, held through the slow-mo's hold, out over BeatVignetteOut s
        // (ease-out, about the slow-mo's ease back). The web has no round-clear slow-mo, so no vignette for it either.
        // Build 1's slowmo strip (Logs/shots/n16-b1-strips/n16-strip-slowmo.png): at 0.12 the vignette's edge read as a hard
        // dark arc in the corners (a tunnel, not a beat): 0.07.
        public const double BeatVignetteBoost = 0.07, BeatVignetteIn = 0.06, BeatVignetteOut = 0.45;

        // --- the slide charge's first-time hint (SurvivalGame.FeelHints.cs; polish #17) ---------------------------------
        // Shown once a session, the first time a slide starts with its burst spent (design.move.slideCharge: the web's slide
        // always bursts, so the web has no such line).
        // {0}: design.move.slideCharge.rechargeTime (s), formatted once at the game's start.

        // --- the rank-up pop (Ui.cs RankPop; polish #26) -----------------------------------------------------------------
        // hud.js rankUp's keyframe literals (:467-473; the pop's scale and time are config's style.popScale / popTime): the
        // letter undershoots to 0.92 at 65 % of the pop, the name slides in from 30 px right, overshoots to 4 px left at 70 %.
        // CSS px are canvas px at 1080p. CSS ease-out is cubic-bezier(0, 0, 0.58, 1) (its x2 is the 0.58).
        public const double RankPopUnder = 0.92, RankPopUnderAt = 0.65, RankNameFrom = 30, RankNameOver = -4, RankNameOverAt = 0.7;
        public const double CssEaseOutX2 = 0.58;

        // --- the points pops (UiPointsPops.cs; polish #30) ---------------------------------------------------------------
        // Unity-only: the label's box (CSS sizes it to the text, white-space nowrap; "+1000 ROUND" at 30 px fits 360 px),
        // and the cosmetic x jitter's stream salt (the web's Math.random; "pops").
        public const float PointsPopBoxWidth = 360;
        public const uint PointsPopSeedSalt = 0x706f7073;

        // --- the damage vignette (Ui.cs; polish #31) ---------------------------------------------------------------------
        // hud.js .hud-damage's CSS literals (:246-247; its strength and ease are config's regen.vignetteMax / vignetteEase):
        // clear inside 38 % of the way to the corners, rgba(150, 18, 8, 0.95) at them.
        public const uint DamageVignetteHex = 0x961208;
        public const float DamageVignetteClear = 0.38f, DamageVignetteEdgeAlpha = 0.95f;

        // --- EXPOSED's edge, the buy prompt's poor / denied, regen (UiFeelEdges.cs; polish #32-#34) ----------------------
        // hud.js's CSS literals. .hud-exposed-edge (:219): box-shadow inset 0 0 90px 30px rgba(255, 40, 10, 0.85); its
        // opacity 0.35 + 0.35 sin(7 t) (:695).
        public const uint ExposedEdgeHex = 0xff280a;
        public const float ExposedEdgeBlur = 90, ExposedEdgeSpread = 30, ExposedEdgeAlpha = 0.85f;
        public const double ExposedEdgeBase = 0.35, ExposedEdgeSwing = 0.35, ExposedEdgeRate = 7;
        // .hud-buy.poor (:237): the text #ff9a82, the border-bottom solid danger (the port's hold bar back takes it, at the
        // web's full alpha). .hud-buy.denied (:239-240): hud-deny 0.1 s steps(2) x 4, 4 px at its 50 % keyframe.
        public const uint BuyPoorTextHex = 0xff9a82;
        public const float BuyPoorBarAlpha = 1;
        public const double BuyDenyPeriod = 0.1, BuyDenyRepeats = 4;
        public const float BuyDenyShift = 4;
        // .hud-hp-fill.regen (:121-124): the gradient's stops (#bff8ff at 42 %, back at 72 %), 220 % wide, its position from
        // 100 % to -120 % (a travel of 2.2) every 0.8 s. .hud-regen-tag (:134): #8ff3ff, letter-spacing 0.22em (TMP 22).
        public const uint RegenSheenHex = 0xbff8ff, RegenTagHex = 0x8ff3ff;
        public const float RegenSheenPeak = 0.42f, RegenSheenEnd = 0.72f, RegenSheenSize = 2.2f, RegenTagSpacing = 22;
        public const double RegenSheenTravel = 2.2, RegenSheenPeriod = 0.8;

        // --- the crosshair tag, the charge edge (UiFeelTags.cs; polish #37 / #38) ----------------------------------------
        // hud.js's literals. .hud-tag (:83-84): its top edge 38 px under the centre, letter-spacing 0.3em (TMP 30); tag()'s
        // colours (main.js :2239-2241): PRIMED #bff8ff, SWAP #ffe6a8; its life 0.7 s at opacity min(1, 3 (0.7 - age))
        // (:581 / :670). The box is Unity-only (CSS sizes it to the text). .hud-charge-edge (:170): inset 0 0 140px 20px.
        public const float TagTop = 38, TagSpacing = 30, TagBoxWidth = 600;
        public const uint TagPrimedHex = 0xbff8ff, TagSwapHex = 0xffe6a8;
        public const double TagTime = 0.7, TagFadeRate = 3;
        public const float ChargeEdgeBlur = 140, ChargeEdgeSpread = 20;
    }
}
