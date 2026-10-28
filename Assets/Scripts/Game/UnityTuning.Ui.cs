// NIGHT12 U3-25.6 (ceremonies and type): the Unity-only numbers of the theme fonts, the round ceremony, RESULTS' tally,
// the HUD's motion, the hit marker's shapes, the living title and the Mystery Box's reel. Display only: none is a
// gameplay number (those come from config.json; the design flags are design.ceremony.* in Design/unity-config.json).
// Where a number is the web's CSS (a literal in hud.js / menus.js / theme.js that config.js doesn't hold) or the
// research's (Research/2026-10-03-polish.md section 2 (f) F1-F5, (b) B2 / B6), the comment says so; the rest are picks.
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the theme fonts (theme.js FONT_FACES; D-13) ---------------------------------------------------------------
        // Resources paths of the TMP font assets SKYGRIND/Make Theme Fonts writes to Assets/Resources/Fonts (static
        // atlases). The display face (Chakra Petch 600) maps italic and bold italic to Chakra Petch 700 italic through its
        // weight table; the body face (Barlow Semi Condensed 600) maps bold to Barlow 700. theme.js ships six faces; these
        // four cover them (500 reads as 600; the rest by TMP's synthetic weight or slant).
        public const string FontDisplayAsset = "Fonts/ChakraPetch-SemiBold SDF";
        public const string FontDisplayItalicAsset = "Fonts/ChakraPetch-BoldItalic SDF";
        public const string FontBodyAsset = "Fonts/BarlowSemiCondensed-SemiBold SDF";
        public const string FontBodyBoldAsset = "Fonts/BarlowSemiCondensed-Bold SDF";
        // The atlases: glyphs rasterised at this point size with this SDF padding (TMP's default ratio is 90 / 9); the
        // editor script tries the atlas sizes in order until every glyph the font has fits. NIGHT12 (the lead): 56 / 6 keeps
        // the ~200 glyphs in a 1024 atlas (72 / 8 needed 2048 x 2048: a 14 MB text asset per face); an SDF this size still
        // draws the 128 px wordmark cleanly.
        public const int FontSamplingPointSize = 56;
        public const int FontAtlasPadding = 6;
        public static readonly Vector2Int[] FontAtlasSizes = { new Vector2Int(1024, 1024), new Vector2Int(2048, 1024), new Vector2Int(2048, 2048) };

        // --- the hit marker's shapes (design.ceremony.hudMotion; research B2: never colour alone) ------------------------
        // A headshot doubles each arm: the second tick this far beside it (px at 1080p).
        public const float HeadMarkerTwinGap = 4f;
        // A kill's marker turns into place: from this many degrees to 0 over this many s (research B2: 45 deg over 80 ms).
        public const float KillMarkerTwistDeg = 45f;
        public const double KillMarkerTwistTime = 0.08;
        // An elite's ring burst (aim.lock.elite kinds): radius and width (px at 1080p), its life (x hud.hitMarkerTime) and
        // the scale it grows from to 1.
        public const float EliteRingRadius = 22f;
        public const float EliteRingWidth = 2f;
        public const double EliteRingLife = 2.0;
        public const float EliteRingFrom = 0.6f;

        // --- TITLE (menus.js .logo / .tagline; design.ceremony.livingTitle for the backdrop) -----------------------------
        // The wordmark: menus.js .logo clamp(64px, 8.5vw, 128px) at 1080p's 1920 px is its 128 px cap; its red-orange copy
        // offset 6 px (.logo .back translate(6px, 6px)); its centre's height over the screen's centre (px at 1080p).
        public const float WordmarkSize = 128f;
        public const float WordmarkShadow = 6f;
        // The tagline's height (px over the centre) and tracking (menus.js .tagline letter-spacing 0.42em; TMP counts
        // characterSpacing in hundredths of an em).
        // menus.js @keyframes logo: brightness 1 -> 1.18 -> 1 over 4 s; the colour is rewritten only after this much change.
        public const float WordmarkBreath = 1.18f;
        public const double WordmarkBreathTime = 4.0;
        public const float WordmarkBreathStep = 0.01f;
        // The band behind the centred column over the flight (px wide at 1080p, the panel colour at this alpha, solid over
        // this share of its half width, fading out beyond) and the shade under the controls line (px tall, alpha).
        public const float TitleBandWidth = 1100f;
        public const float TitleBandAlpha = 0.82f;
        public const float TitleBandSolid = 0.5f;
        public const float TitleShadeHeight = 160f;
        public const float TitleShadeAlpha = 0.75f;

        // --- the living title's flight (TitleView) ------------------------------------------------------------------------
        // The loop: this many spline points round the map's centre, starting at this angle (rad); the odd points pulled in
        // to this share of the radius; the radius a share of the map boxes' half extent; the even / odd points this far (m)
        // over the tallest box (the spires top at 29.2 m on the Survival map, so the camera never meets one).
        public const int TitlePathPoints = 6;
        public const double TitlePathPhase = 0.3;
        public const double TitlePathInner = 0.7;
        public const double TitleOrbitShare = 0.62;
        public const double TitleHeightHigh = 6.0;
        public const double TitleHeightLow = 2.5;
        // One loop takes this many s; the look aims past the centre by this share of the camera's distance to it, this many
        // deg below the camera's own level. NIGHT17 round 3 (its reviewer: "a flat close-up of salmon floor"): the look aimed
        // further past the centre and higher (0.5 / 0.35 were about 25 deg down). Round 4 (both reviewers: "the right half a
        // flat pink floor"): a height share of the tallest box (0.85) still pitched the camera about 8 deg down from 2.5-6 m
        // over the spires, so the horizon sat in the frame's top 14 % and the rest was rooftops; a fixed shallow pitch puts the
        // horizon near mid-frame with the dusk sky and the low sun (sky.sunElevationDeg 7) above it.
        public const double TitleLoopTime = 90.0;
        public const float TitleLookBeyond = 1.2f;
        public const float TitleLookPitchDeg = -3f;
        // The half extent used when a map has no boxes (m; the Survival floor is 96 m across).
        public const double TitleFallbackHalfExtent = 48.0;
        // The neon strips' pulse on TITLE: _BaseColor x (1 +- this) over this many s.
        public const float TitleNeonPulse = 0.35f;
        public const double TitleNeonPeriod = 3.2;

        // --- the round ceremony (design.ceremony.round; research F1) --------------------------------------------------------
        // The counter: the numeral's size (hud.js .hud-round is 44 px; bigger in CoD's corner), the mark's height and the
        // numeral's box (px at 1080p), its bottom over the HUD's margin (above the perk row at margin + 100..130), the ROUND
        // label's gap, height and tracking (hud.js .hud-round small: letter-spacing 0.35em).
        public const float RoundLabelSpacing = 35f;
        // The tally strokes (px at 1080p): width, height, spacing, the four's alternate tilt and the fifth's slash (length,
        // its angle from upright: leaning right).
        // The burn-in: a stroke every 80 ms (research F1), the numeral over 400 ms (F1), held, then the flight to the corner
        // over 300 ms (F1). At the centre the mark is this much bigger (64 x 1.75 = 112 px: F1's 96-120 px), popping in from
        // this extra scale, this far over the centre (px) to keep the crosshair clear; white hot cooling to red-orange over
        // this many s.
        // The clear: the counter flashes white 3 times over 1.5 s (research F1).
        public const double RoundClearTime = 1.5;
        public const int RoundClearFlashes = 3;

        // --- the HUD's motion (design.ceremony.hudMotion; research F4, B6) ---------------------------------------------------
        // The HP bar's damage ghost: white at this alpha, held 250 ms after a drop, drained over 300 ms (F4).
        public const float HpGhostAlpha = 0.85f;
        public const double HpGhostHold = 0.25;
        public const double HpGhostDrain = 0.3;
        // The points roll: up over 0.2 s, a 1.12x punch for a gain of 100 or more (B6), the punch's decay (s).
        public const double PointsRollTime = 0.2;
        public const double PointsPunchAt = 100;
        public const float PointsPunchScale = 1.12f;
        // The plates' padding (px at 1080p): hud.js .hud-points padding 3px 12px 4px 14px; .hud-top padding 3px 14px 3px 18px.
        public const float TopPlatePadY = 3f;
        public const float TopPlatePadLeft = 18f;
        public const float TopPlatePadRight = 14f;
        // The rank box behind the rank letter, its name and its bar (px at 1080p, from the right-middle anchor; the port's
        // style cluster sits there, the web's .hud-rankbox is fluid): its right edge past the margin, centre height, size.

        // --- RESULTS' tally (design.ceremony.resultsTally; research F2) -----------------------------------------------------
        // The column (px at 1080p, from the screen's centre): its centre's x (left of the PAYOFF strip at x +500..+900) and
        // width; the big type's size (the round, the grade; mid is the theme's h1, small its body / label).
        // Rows (y of each group's centre, px over the screen's centre) and sizes.
        public const float TallyHeatWidth = 130f;
        public const float TallyHeatHeight = 40f;
        public const float TallyStampTiltDeg = -8f;
        public const float TallyDetailsLineSpacing = 4f;
        // menus.js .grade: the letter in #0b0a14 on its colour, the plate skewed -10 deg.
        // theme.js --danger-dim: colors.enemy at 0.2 (the HEAT badge's plate).
        // The beats (s from RESULTS opening; F2: the outcome at 0, the round at 300 ms, three cards counting 500 ms each
        // staggered 150 ms, the grade's slam 1.6x -> 1x in 120 ms with a shake and a white flash, then NEW BEST as a stamp).
        public const double TallyFlashTime = 0.15;
        public const float TallyFlashAlpha = 0.35f;
        public const float TallyStampFrom = 1.8f;
        public const double TallyStampTime = 0.14;
        // Each group fades in over this many s; a key or click completes the tally only after this many s (a key held into
        // the run's end doesn't skip it).
        public const double TallySkipGuard = 0.25;

        // --- the Mystery Box's reel (research F3) -----------------------------------------------------------------------------
        // The reveal samples the cycle every design.box.cycleTime at first, slowing (eased in) to this many times it by the
        // end; each new gun pops to this scale and back over this many s; the result settles with a bigger pop over this
        // many s and its cyan flashes to (1 + this) x.
        public const double BoxCycleSlowest = 4.0;
        public const float BoxCyclePop = 1.08f;
        public const float BoxCyclePopTime = 0.1f;
        public const float BoxSettlePop = 1.35f;
        public const float BoxSettleTime = 0.3f;
        public const float BoxSettleFlash = 1.5f;

        // NIGHT12 (the lead): damageNumbers.js's literals (the web types them in its DOM styles; config.js has none): a kill's
        // number #ff7a45 at 26 px, a headshot's #ffd24a at 22 px, a body hit's white at 18 px (all over the 16 px floor).
        public const uint DamageNumberKillHex = 0xff7a45;
        public const uint DamageNumberHeadHex = 0xffd24a;
        public const float DamageNumberKillSize = 26f;
        public const float DamageNumberHeadSize = 22f;
        public const float DamageNumberSize = 18f;
    }
}
