// NIGHT17 U3-30.3 (the shared commit) / U3-30.2 (the identity): the identity's tokens in ONE place (NIGHT17.md section 1,
// IDENTITY.md). Every screen reads them through UiTheme, so a token change restyles the whole game. Display only: none is a
// gameplay number. The hues stay config.json's colour roles (CLAUDE.md rule 8: colors.usable cyan is the player's,
// colors.enemy red-orange is danger, ui.theme.gold is points, ui.theme.text white, ui.theme.panel the ink); these tokens
// set only value, saturation, alpha, type, shape and motion (NIGHT17.md rule 2: identity from value, type, shape and motion,
// never a new hue). The shared commit holds today's look; U3-30.2 commits the picked identity over it.
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the faces (Resources paths of TMP font assets: Editor/ThemeFonts.cs bakes them) ----------------------------------
        // DISPLAY: heads, hero numbers, the wordmark, buttons' labels at 32 px and up. BODY: everything else. RANK: the style
        // rank's letter and the round's hero number. NIGHT17 U3-30.2 picked direction A from the style tile (IDENTITY.md):
        // Archivo Extra Condensed Black Italic (display: already slanted, so no synthetic italic or bold on it), Archivo
        // SemiBold / ExtraBold (body / bold), Tourney Black Italic (the rank and the round).
        public const string ThemeFontDisplay = FontArchivoDisplay;
        public const string ThemeFontBody = FontArchivoBody;
        public const string ThemeFontBodyBold = FontArchivoBodyBold;
        public const string ThemeFontRank = FontTourneyRank;
        // A text at this size or more takes DISPLAY (px at 1080p; below it BODY).
        public const float ThemeDisplayFrom = 32f;
        // NIGHT17 U3-30.2: the candidate faces' TMP assets (Editor/ThemeFonts.MakeN17 bakes them; OFL, Assets/Fonts/<family>).
        public const string FontArchivoDisplay = "Fonts/Archivo-ExtraCondensedBlackItalic SDF";
        public const string FontArchivoBody = "Fonts/Archivo-SemiBold SDF";
        public const string FontArchivoBodyBold = "Fonts/Archivo-ExtraBold SDF";
        public const string FontTourneyRank = "Fonts/Tourney-BlackItalic SDF";
        public const string FontBigShouldersDisplay = "Fonts/BigShouldersDisplay-Black SDF";
        public const string FontSairaBody = "Fonts/Saira-SemiCondensedSemiBold SDF";

        // --- the colour tokens (mixes of the config's hues; NIGHT17.md section 1) ---------------------------------------------
        // INK: panels are opaque near-black violet (ui.theme.panel) at this alpha or more, or no panel at all ("no translucent
        // grey plates": the research's cheap-UI list).
        public const float ThemeInkAlpha = 0.96f;
        // INK-SOFT: a raised row on INK (the panel colour toward the text colour by this share).
        public const float ThemeInkSoftMix = 0.08f;
        // MUTED: secondary text, the text colour toward INK by this share (a token, never "text at 0.74 alpha").
        public const float ThemeMutedMix = 0.42f;
        // The cyan value ladder (the style rank's ladder and focus states: NIGHT17.md rule 2, no hue ladder):
        // CYAN-DEEP the cyan toward INK (fills behind white text), CYAN-PALE toward white, CYAN-HOT nearly white (+ glow).
        public const float ThemeCyanDeepMix = 0.55f;
        public const float ThemeCyanPaleMix = 0.45f;
        public const float ThemeCyanHotMix = 0.78f;

        // --- the type scale (px at 1080p; the research's 1.333 ratio) and the floor --------------------------------------------
        // At most 4 sizes a screen. NIGHT17.md rule 2 raises the floor from 16 to 18 px (13.3 px on the Steam Deck).
        public static readonly float[] ThemeTypeScale = { 18f, 24f, 32f, 42f, 56f, 75f, 100f, 133f };
        public const float ThemeTextFloor = 18f;
        // Tracking (TMP characterSpacing, hundredths of an em) for labels in caps, and for the display face (tight).
        public const float ThemeLabelTracking = 8f;
        public const float ThemeDisplayTracking = 0f;

        // --- shape -------------------------------------------------------------------------------------------------------------
        // One slant (deg; matches the display italic), the 45 deg corner cut (px at 1080p), the accent bar (px), the hard text
        // shadow (px, in INK), the hero number's offset copy (px, in CYAN-DEEP: the wordmark's idea).
        public const float ThemeSlantDeg = 12f;
        public const float ThemeCut = 10f;
        public const float ThemeAccentBar = 4f;
        public const float ThemeShadowOffset = 2f;
        public const float ThemeHeroOffset = 5f;
        // The safe inset (px at 1080p; NIGHT17.md rule 8: 48-64) and the HUD SCALE row's range.
        public const float ThemeSafeInset = 56f;
        public const float ThemeHudScaleMin = 0.8f;
        public const float ThemeHudScaleMax = 1.25f;

        // --- motion (s; NIGHT17.md section 1: in 120-180 ms ease-out-back ~10 % overshoot, out 80-120 ms ease-in) ---------------
        public const double ThemeInTime = 0.15;
        public const double ThemeOutTime = 0.10;
        // Ease-out-back's s: 1.70158 overshoots by 10 %.
        public const float ThemeOvershoot = 1.70158f;
        // A menu enters along the slant from this far (px) down-left.
        public const float ThemeEnterDistance = 60f;
        // A number's punch (1 -> ThemePunchScale -> 1 over ThemePunchTime, at most once per ThemePunchGap) and its count-up
        // (ThemeCountMin .. ThemeCountMax s by the log of the change).
        public const float ThemePunchScale = 1.15f;
        public const double ThemePunchTime = 0.15;
        public const double ThemePunchGap = 0.10;
        public const double ThemeCountMin = 0.25;
        public const double ThemeCountMax = 0.60;
        // A slam (a rank letter, a round number): from this scale to 1 over this many s.
        public const float ThemeSlamFrom = 2.0f;
        public const double ThemeSlamTime = 0.10;
    }
}
