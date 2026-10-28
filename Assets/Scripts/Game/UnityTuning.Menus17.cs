// NIGHT17 U3-30.8 (builder N, menus and words): the menus' Unity-only numbers and words. The web's menus are CSS (no
// numbers to port) and its copy is the web's; GG's NIGHT16 verdict ("way too wordy ... way too cluttered ... way too many
// options") rewrote them here: at most 8 words a line (3 on the HUD), at most 5 items a menu, 8 rows a settings tab before
// ADVANCED. Px at 1080p from the screen's centre unless said; times in s. Colours, faces, the type scale, the slant and the
// motion are UiTheme's tokens (UnityTuning.Theme.cs), never set here.
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the column (every menu's left edge, inside the safe inset at 16:9 AND 16:10) ------------------------------------------
        // The canvas matches the height (1080 units), so its half-width is 960 at 16:9 and 864 at 16:10 (the Deck's 1280 x 800);
        // with ThemeSafeInset (56) nothing may sit left of -808. A column's left edge at height y is MenuColumnLeft +
        // tan(ThemeSlantDeg) x y (lower rows further left, the one diagonal): -548 at the top (+340), -714 at the bottom
        // (-440). Texts and plates are placed by their left edge on it (MenuLeft / MenuAt); slabs may bleed off the edge.
        public const float MenuColumnLeft = -620f;
        // The left-most x any menu content may take at 16:10 (MenusTests checks the columns against it).
        public const float MenuSafeLeft = -808f;
        public const float MenuSafeRight = 808f;

        // --- the menu item (Persona 5: one diagonal, the focused item lit, the rest INK-SOFT) ----------------------------------
        public const float MenuItemWidth = 460f;
        public const float MenuItemHeight = 60f;
        public const float MenuItemPitch = 68f;
        // The focused item slides this far right along its row (visual only).
        public const float MenuFocusNudge = 18f;
        // The focus look's ease (s): the nudge and the plate's blend toward the focused state.
        public const double MenuFocusTime = 0.09;
        // Round 5: a pointer selects by hover only when it moved at least this far (px) in the frame (a resting or jittering
        // mouse never steals the pad's or the screen's focus).
        public const float NavHoverMinMove = 2f;
        // Each item (a row, a tally line) enters this much later than the one above it (s; one diagonal sweep).
        public const double MenuEnterStagger = 0.03;
        // The INK slab behind a column (a slanted UiShape from off the left edge): its right edge's x at y = 0 per screen
        // (wide enough that its contents sit centred on it), and how far it slides in from the left on entry.
        public const float TitleSlabRight = 80f;
        public const float PauseSlabRight = -20f;
        public const float ResultsSlabRight = 250f;
        public const float MenuSlabEnter = 140f;
        // The world behind a menu: an INK sheet at this alpha (no translucent grey plates over the bright dusk); TITLE's
        // living world is dimmed lighter (it is the attraction), so the slab's column reads first.
        public const float MenuSheetAlpha = 0.88f;
        public const float TitleSheetAlpha = 0.32f;

        // --- NIGHT18 U3-31.7 (builder N): menus in motion, behind FeelAB (off: NIGHT17's path; each old value kept as ...N17) --
        // Build 1's reel (M9): SETTINGS' sheet, slab, head, tabs, footer and BACK, PAUSED's sheet, head and run line, TITLE's
        // best grades appeared whole on the input frame; RESULTS' grade settled in 67 ms and its card foot in 83 ms (M9's
        // floor is 90 ms), and its full-screen flash read as a pop-in.
        // A screen's INK sheet fades from the sheet shown before it (0 when none) to its own alpha over this many s,
        // ease-out-cubic (NIGHT17: whole on its first frame).
        public const double MenuSheetFade18 = 0.18;
        // Round 2's blind reviewer ("too much" on SETTINGS: "still building at 250 ms"): a screen is whole by about 200 ms.
        // Items and parts start MenuEnterStagger18 s apart, no later than MenuEnterSpread18 s (+ ThemeInTime = 200 ms); a
        // SETTINGS tab's rows enter as one group (NIGHT17: MenuEnterStagger apart, unbounded: a 9-row tab took 420 ms).
        public const double MenuEnterStagger18 = 0.015;
        public const double MenuEnterSpread18 = 0.05;
        // RESULTS: the card slides in first (MenuSlabEnter px along the slant), its lines start this much later (s); NIGHT17
        // faded the card in with its first line (the reviewer saw "the same stagger" on both sides)
        public const double ResultsCardLead18 = 0.12;
        // Round 3's blind reviewer (SETTINGS to NIGHT17: "[N18] shows the whole list at once, faint"): a tab's rows cascade
        // again, quick: SettingsRowStagger18 s apart, each sliding ThemeEnterDistance px along the slant from alpha 0 over
        // SettingsRowIn18 s (settles past M9's 90 ms); a 9-row tab in by ~250-280 ms (NIGHT17: 30 ms apart over 150 ms: ~420 ms).
        public const double SettingsRowStagger18 = 0.02;
        public const double SettingsRowIn18 = 0.14; // 0.12 settled in 78 ms at the reel's 60 fps (under M9's 90)
        // PAUSED / RESULTS (round 3: "nearly pixel-identical" to NIGHT17): the slab sweeps in first, MenuSlabEnter18 px
        // down-left along the slant over MenuSlabIn18 s (NIGHT17: MenuSlabEnter px straight left over ThemeInTime), then the
        // items and parts cascade MenuCascade18 s apart from MenuSlabLead18 s.
        public const float MenuSlabEnter18 = 360f;
        public const double MenuSlabIn18 = 0.18;
        public const double MenuSlabLead18 = 0.06;
        public const double MenuCascade18 = 0.022;
        // MODS: the head, the three cards and REROLL rise along the slant from DraftCardEnter18 px, DraftCascade18 s apart.
        public const float DraftCardEnter18 = 150f;
        public const double DraftCascade18 = 0.035;
        // The focused item / settings row slides this far (px) along the slant (right and 12 deg up), ease-out-back over this
        // many s (NIGHT17: MenuFocusNudge px straight right over MenuFocusTime, ease-out-cubic; a settings row didn't move).
        public const float MenuFocusNudge18 = 22f;
        public const float SettingsRowNudge18 = 14f;
        public const double MenuFocusTime18 = 0.16;
        // RESULTS: the grade's slam (s; NIGHT17's ThemeSlamTime settles in 67 ms) and the card foot's fade (s; NIGHT17's
        // ResultsLineFade, alpha only, is whole in 83 ms): both now settle past M9's 90 ms floor; the foot also slides in.
        public const double ResultsGradeSlam18 = 0.16;
        public const double ResultsGradeSlamN17 = ThemeSlamTime;
        public const double ResultsFootFade18 = 0.22;
        public const double ResultsFootFadeN17 = ResultsLineFade;
        // The grade's flash on its own plate (NIGHT17: a full-screen white sheet at TallyFlashAlpha over TallyFlashTime): a
        // white-hot copy of the plate behind the letter, from TallyFlashFrom18 x the plate to TallyFlashTo18 x as it fades
        // from TallyFlashAlpha18 over TallyFlashTime18 s (through the flash gate and FLASH INTENSITY, as before).
        public const float TallyFlashFrom18 = 1.6f;
        public const float TallyFlashTo18 = 1.15f;
        public const float TallyFlashAlpha18 = 0.8f;
        public const double TallyFlashTime18 = 0.2;
        // The hint and lesson cards: a leading "[KEY] " becomes a drawn key chip (KeyChip) before the words, this far apart (px).
        public const float HintChipGap18 = 10f;
        // SETTINGS > ACCESSIBILITY (FeelAB on): FOV KICK and CAMERA SHAKE up front (comfort rows, beside REDUCED MOTION); LOW
        // HEALTH EDGE goes behind ADVANCED so the tab keeps MenuRowsBeforeFold rows. Off: the table's flags (both behind it).
        public static readonly string[] SettingsFront18 = { FovKickSetting, CameraShakeSetting };
        public static readonly string[] SettingsAdvanced18 = { "hurtVignette" };

        // --- TITLE -------------------------------------------------------------------------------------------------------------
        // The wordmark (S133, DISPLAY, CYAN over a CYAN-DEEP copy offset ThemeHeroOffset) and the tagline under it (y).
        public const float TitleMarkY = 330f;
        public const float TitleTagY = 222f;
        // Round 3: a mod card's glyph (px square) and the y of its glyph, name, hero number and the rest of its line (from the
        // card's centre; the card is UnityTuning.ModCardWidth x ModCardHeight).
        public const float ModGlyphSize = 96f;
        public const float ModGlyphY = 82f;
        public const float ModNameY = 8f;
        public const float ModHeroY = -46f;
        public const float ModDescY = -100f;
        // Round 3: the wordmark's cut slab: its padding either side of the text, its height, its deep corner cut, the CYAN
        // accent bar along its lower edge (px tall).
        public const float TitleMarkPadX = 40f;
        public const float TitleMarkPlateHeight = 150f;
        public const float TitleMarkCut = 30f;
        public const float TitleMarkBar = 8f;
        // PLAY: the hero item (its label S75) and the column's first row; RESUME (only with a saved run) rides PLAY's row on
        // the right, smaller (TITLE stays at 5 rows), still on the slab.
        public const float TitlePlayRowY = 120f;
        public const float TitlePlayWidth = 420f;
        public const float TitlePlayHeight = 104f;
        public const float TitleResumeGap = 20f;
        public const float TitleResumeWidth = 220f;
        public const float TitleResumeHeight = 64f;
        // The run setup card: one card of three rows (MODE / HEAT / MAP), a press or left / right cycles a row.
        public const float SetupCardTopY = 18f;
        public const float SetupCardWidth = 520f;
        public const float SetupCardRow = 46f;
        public const float SetupCardPad = 10f;
        public const float SetupCardLabelWidth = 120f;
        // The card's one info line under it, and the column's rows after it.
        public const float SetupInfoGap = 26f;
        public const float TitleAfterCardGap = 64f;
        // CONTRACTS / WEAPONS (both under PROGRESS): BACK and the switch to the other screen, this far left / right of centre.
        public const float MetaSwitchX = 230f;

        // --- PAUSED ------------------------------------------------------------------------------------------------------------
        // The head (S100), the one-line run header under it (round · points · time, BODY muted 24 px), the first row (y).
        public const float PauseHead17Y = 300f;
        public const float PauseRunLineY = 222f;
        public const float PauseFirstRowY = 110f;
        // QUIT asks once: a second press within this many s quits (it autosaves at a break where a save is allowed).
        public const double PauseQuitConfirmWindow = 3.0;

        // --- RESULTS -----------------------------------------------------------------------------------------------------------
        // The hero tally's lines arrive this far apart (s; the research's 200 ms), each fading and sliding in along the slant.
        public const double ResultsLineGap = 0.2;
        public const double ResultsLineFade = 0.15;
        // The outcome head (y), the tally row's y and its three columns from the column's left edge (ROUND, POINTS, GRADE:
        // each a muted S24 label over its number), the labels' rise over the numbers' centres, the grade's column width.
        public const float ResultsHeadY = 360f;
        public const float ResultsHeroY = 150f;
        public const float ResultsPointsFromLeft = 230f;
        public const float ResultsGradeFromLeft = 620f;
        public const float ResultsGradeWidth = 170f;
        // Round 3: the grade's own plate (px square, the hero size), the even gap between the tally's three columns and a
        // column's least width (its S24 label); a drawn key chip (RETRY, DOWN, WHAT GOT YOU): its height, padding, gap to its word.
        public const float ResultsGradePlate = 150f;
        // Round 5: NEW BEST as a small gold stamp (px; tilted UnityTuning.TallyStampTiltDeg) laid over the points' top, its
        // centre this far above the points' centre line.
        public const float ResultsStampWidth = 200f;
        public const float ResultsStampHeight = 46f;
        public const float ResultsStampRise = 56f;
        public const float ResultsColumnGap = 96f;
        public const float ResultsLabelWidth = 110f;
        public const float KeyChipSize = 40f;
        public const float KeyChipPad = 10f;
        public const float KeyChipGap = 16f;
        public const float ResultsHeroLabelGap = 92f;
        // NEW BEST's plate under POINTS (y), and the ROUND numeral's slam (the grade slams from ThemeSlamFrom).
        public const float ResultsBestY = 30f;
        public const float ResultsRoundSlamFrom = 1.5f;
        // The buttons' column (RETRY (R), TITLE, DETAILS; RETRY SAME SEED with DETAILS; dev: COPY SCORE LINE): the first y.
        public const float ResultsFirstRowY = -132f;
        // NIGHT17 U3-30.10 (beyond the ask): the RESULTS card, the frame a player would post: one slanted INK card round the
        // tally row (its top, bottom and right edge at its centre line; its left edge ResultsCardPad left of the column),
        // a CYAN accent bar down its left, THE ROUTE's minimap top-right (ResultsMapSize square, centred at
        // ResultsMapX / ResultsMapY), the best moment, the date / seed and the wordmark along its foot (ResultsCardFootY).
        public const float ResultsCardTop = 300f;
        public const float ResultsCardBottom = -80f;
        public const float ResultsCardRight = 600f;
        public const float ResultsCardPad = 64f; // round 2: the accent bar clear of the ROUND label
        public const float ResultsCardAccent = 8f;
        public const float ResultsMapSize = 280f; // round 2: a map that can be read
        public const float ResultsMapX = 430f;
        public const float ResultsMapY = 120f;
        public const float ResultsCardFootY = -46f;
        // DETAILS: the details block's top (y), width and height (from the column's left edge); the PAYOFF strip's and THE
        // ROUTE plate's right edge (inside MenuSafeRight; UnityTuning.Meta / Route place them at 900, past the Deck's edge).
        public const float ResultsDetailsTop = 330f;
        public const float ResultsDetailsWidth = 760f;
        public const float ResultsDetailsHeight = 330f;
        public const float ResultsPayoffX = 380f;
        public const float ResultsRouteRight = 780f;

        // --- DOWN --------------------------------------------------------------------------------------------------------------
        // Round 2: DOWN darkens the world behind it (an INK sheet), and WHAT GOT YOU's replay a little (its heading and the
        // killer's plate on their own INK plate, padded by RecapBlockPad).
        public const float DownSheetAlpha = 0.62f;
        public const float RecapSheetAlpha = 0.3f;
        public const float RecapBlockPad = 18f;
        public const float DownHeadY = 120f;
        public const float DownLineY = 30f;
        public const float DownRecapY = -30f;
        // Round 3: DOWN's retry row (a drawn key chip and RETRY), under the recap's two lines.
        public const float DownRetryY = -150f;
        // WHAT GOT YOU: the heading and the killer's DANGER plate as one block (the plate this far under the heading's box),
        // the plate this much wider and taller than the name.
        public const float RecapTagGap = 10f;
        public const float RecapTagPadX = 18f;
        public const float RecapTagPadY = 6f;

        // --- SETTINGS ----------------------------------------------------------------------------------------------------------
        // The head (S75, y) and the tab row (text tabs from the column's left edge: the current one lit), the rows' first
        // centre and pitch, the footer's line and BACK. A tab shows at most MenuRowsBeforeFold rows, then ADVANCED.
        public const float Settings17HeadY = 420f;
        public const float SettingsTabsY = 340f;
        public const float SettingsTab17Width = 200f;
        public const float SettingsTabGap = 18f;
        // Round 3: a tab is as wide as its word plus this either side (the gaps between the words even).
        public const float SettingsTabPad = 22f;
        public const float SettingsTab17Height = 48f;
        public const float SettingsFirstRowY = 262f;
        public const float SettingsRowPitch = 54f;
        public const float SettingsRow17Width = 1100f;
        public const float SettingsRow17Height = 48f;
        public const float SettingsFooterY = -340f;
        public const float SettingsBackY = -410f;
        // A row's value side: the slider's bar (a slanted UiShape bar), the value text after it, a select's arrows box.
        public const float SettingsValueX = 240f;
        public const float SettingsSliderWidth = 300f;
        public const float SettingsSliderHeight = 14f;
        public const float SettingsValueWidth = 140f;
        public const float SettingsToggleWidth = 120f;
        // Round 2: a select's value box between its two drawn chevrons (UiChevron, px), and the rows' INK slab (a solid slanted
        // UiShape round the tabs, the rows and the footer: its half-width, top and bottom), the world dimmed behind it.
        public const float SettingsSelectWidth = 200f;
        public const float ChevronWidth = 12f;
        public const float ChevronHeight = 18f;
        public const float SettingsSlabHalfWidth = 650f;
        public const float SettingsSlabTop = 380f;
        public const float SettingsSlabBottom = -372f;
        public const int MenuRowsBeforeFold = 8;
        // NIGHT17 round 1 (the reviewer: ".0014 · 80.2°/1K" is jargon): a range shown as one plain number, its value times
        // this (MOUSE SPEED 0.0014 -> 1.40); the web's RangeText (with its degrees per 1000 counts) for the rest.
        public static readonly Dictionary<string, double> SettingsShownScale = new Dictionary<string, double> { ["sensitivity"] = 1000, [HudScaleSetting] = 100, [TextSize47Setting] = 100 }; // NIGHT47 F: TEXT SIZE reads "150 %"
        // Round 5: a range's unit after its number (HUD SCALE reads "100 %", matching its footer's 80-125 %).
        public static readonly Dictionary<string, string> SettingsShownSuffix = new Dictionary<string, string> { [HudScaleSetting] = " %", [TextSize47Setting] = " %" };
        public const string AdvancedLabel = "ADVANCED";
        public const string AdvancedOpenLabel = "FEWER";
        public const string KeysRowKey = "keys";
        public const string KeysRowLabel = "KEYS";
        // HUD SCALE (NIGHT17.md rule 8; builder H applies it): its settings key, default and step (the range is
        // ThemeHudScaleMin..ThemeHudScaleMax). The web has no such row (its HUD scaled with UI SCALE, which this replaces).
        public const string HudScaleSetting = "hudScale";
        public const double HudScaleDefault = 1.0;
        public const double HudScaleStep = 0.05;

        /// <summary>
        /// SETTINGS' rows, in order: (key, tab, behind ADVANCED, label or null for the store's, the footer line). GAME / CONTROLS
        /// / VIDEO / ACCESSIBILITY, GAMEPAD (shown while a pad is connected), LAB (devMode only: its rows are the store's lab
        /// tab, listed by MenuRules, not here). A key not listed is not shown (its value stays saved and applied).
        /// </summary>
        public static readonly (string key, string tab, bool advanced, string label, string foot)[] MenuSettingsRows =
        {
            (HudScaleSetting, GameTabId, false, "HUD SCALE", "SCALES THE HUD 80-125 %"),
            ("crosshairColor", GameTabId, false, "CROSSHAIR", "COLOURS THE AIMING MARK"),
            ("damageNumbers", GameTabId, false, "DAMAGE NUMBERS", "SHOWS THE DAMAGE OF EACH HIT"),
            ("damageDirection", GameTabId, false, "HIT DIRECTION", "ARCS POINT AT WHAT HIT YOU"),
            ("showPrices", GameTabId, false, "BUY PRICES", "SHOWS WHAT WALL GUNS COST"),
            ("xray", GameTabId, false, "ENEMY X-RAY", "OUTLINES ENEMIES BEHIND WALLS"),
            (OnboardingSetting, GameTabId, false, "TIPS", "SHOWS A CONTROL WHEN IT HELPS"),
            (RecapSetting, GameTabId, false, "WHAT GOT YOU", "REPLAYS YOUR DEATH FROM ABOVE"),
            (RunLogSetting, GameTabId, true, "RUN LOG", "KEEPS A LOCAL LOG FOR BUG REPORTS"),

            ("sensitivity", "controls", false, "MOUSE SPEED", "TURNS THE VIEW FASTER OR SLOWER"),
            ("adsSensitivity", "controls", false, "ZOOM SPEED", "TURN SPEED WHILE LOOKING DOWN SIGHTS"),
            ("invertY", "controls", false, "MOUSE INVERT Y", "FLIPS THE MOUSE UP AND DOWN"),
            ("aimAssist", "controls", false, "MOUSE AIM ASSIST", "PULLS A MOUSE'S AIM ONTO TARGETS"),
            ("snapStrength", "controls", false, "ASSIST STRENGTH", "SETS HOW HARD THE AIM IS PULLED"),
            ("slideMode", "controls", false, "SLIDE", "HOLD THE KEY, OR TAP ON AND OFF"),
            (KeysRowKey, "controls", false, KeysRowLabel, "SEE AND CHANGE EVERY KEY"),
            ("arclineMode", "controls", true, "ARCLINE", "HOLD THE KEY, OR TAP ON AND OFF"),

            ("fovDeg", VideoTabId, false, "FIELD OF VIEW", "WIDENS OR NARROWS THE VIEW"),
            (SettingsView.WindowModeKey, VideoTabId, false, "WINDOW", "HOW THE GAME SITS ON YOUR SCREEN"),
            (SettingsView.ResolutionKey, VideoTabId, false, "RESOLUTION", "AUTO MATCHES YOUR DISPLAY"),
            ("quality", VideoTabId, false, "QUALITY", "TRADES DETAIL FOR FRAMES"),
            ("fpsCap", VideoTabId, false, "FPS CAP", "LIMITS FRAMES A SECOND; OFF IS UNCAPPED"),
            (SettingsView.VsyncKey, VideoTabId, false, "VSYNC", "STOPS TEARING, ADDS A LITTLE LAG"),
            ("renderScale", VideoTabId, false, "RENDER SCALE", "DRAWS FEWER PIXELS: FASTER, SOFTER"),
            ("filmGrain", VideoTabId, false, "FILM GRAIN", "ADDS A FINE GRAIN TO THE PICTURE"),
            ("toneMapping", VideoTabId, true, "TONE MAPPING", "HOW BRIGHT COLOURS ROLL OFF"),
            // NIGHT30 item 8 (builder V): the CRT rows NIGHT29 registered but never listed, and PIXEL SIZE; shown only with N30Fixes.On (UiSettings.Shown30)
            (CrtSetting, VideoTabId, true, CrtLabel, "THE OLD-TV LOOK'S STRENGTH"),
            (Pixel30Setting, VideoTabId, true, Pixel30Label, "BIGGER PIXELS: 1 IS OFF"),

            ("effectsIntensity", AccessTabId, false, "EFFECTS", "SCALES SHAKE, FLASHES AND SPARKS"),
            ("flashIntensity", AccessTabId, false, "FLASHES", "DIMS OR BRIGHTENS EVERY FLASH"),
            (TextSize47Setting, AccessTabId, true, TextSize47Label, "MAKES THE GAME'S TEXT BIGGER, UP TO 200 %"), // NIGHT47 F (hud47): shown only when its item exists (the row on); behind ADVANCED (the tab's front holds 8: MenusTests / MenuMotion18Tests)
            ("reducedMotion", AccessTabId, false, "REDUCED MOTION", "STILLS THE SHAKE AND SWAY"),
            // NIGHT18 (builder N): beside REDUCED MOTION; up front with FeelAB on (SettingsFront18), behind ADVANCED off
            (FovKickSetting, AccessTabId, true, FovKickLabel, "HOW FAR THE VIEW WIDENS AT SPEED"), // the store's desc is 9 words
            (CameraShakeSetting, AccessTabId, true, CameraShakeLabel, "HOW HARD THE SCREEN SHAKES"),
            ("cameraRoll", AccessTabId, false, "CAMERA ROLL", "TILTS THE VIEW ON WALL RIDES"),
            ("hurtVignette", AccessTabId, false, "LOW HEALTH EDGE", "REDDENS THE SCREEN'S EDGES WHEN HURT"),
            (CrtOverlaySetting, AccessTabId, true, CrtOverlayLabel, "TURNS THE OLD-TV LOOK OFF"), // NIGHT30 item 8 (builder V)
            (HudFx30Setting, AccessTabId, true, HudFx30Label, "STILLS THE HUD'S FLICKER AND BUZZ"), // NIGHT30 item 8 (builder V)
            (Skygrind.Core.FeelKeys.HitFreezeSetting, AccessTabId, false, "HIT FREEZE", "PAUSES A BEAT ON BIG HITS"),
            (Skygrind.Core.FeelKeys.SlowMoSetting, AccessTabId, false, "SLOW-MO", "SLOWS TIME ON A GREAT KILL"),

            ("padAimAssist", PadTab17Id, false, "PAD AIM ASSIST", "PULLS A PAD'S AIM ONTO TARGETS"),
            ("padSensX", PadTab17Id, false, "PAD LOOK X", "HOW FAST THE STICK TURNS THE VIEW"),
            ("padSensY", PadTab17Id, false, "PAD LOOK Y", "HOW FAST THE STICK TILTS THE VIEW"),
            ("padInvertY", PadTab17Id, false, "PAD INVERT Y", "FLIPS THE STICK UP AND DOWN"),
            (PadRumbleSetting, PadTab17Id, false, "RUMBLE", "THE PAD SHAKES ON HITS: 0 IS OFF"),
            ("padCurve", PadTab17Id, true, "RESPONSE CURVE", "PRECISE: SLOWER NEAR THE CENTRE"),
            ("padDeadzoneInner", PadTab17Id, true, "INNER DEAD ZONE", "IGNORES STICK DRIFT BELOW THIS"),
            ("padDeadzoneOuter", PadTab17Id, true, "OUTER DEAD ZONE", "FULL TURN SPEED FROM HERE OUT"),
        };

        // The tabs' ids (the store's where they exist, so ShotTour's OpenSettingsTab ids still open them) and labels.
        public const string PadTab17Id = "pad";
        public static readonly (string id, string label)[] MenuSettingsTabs =
        {
            (GameTabId, "GAME"), ("controls", "CONTROLS"), (VideoTabId, "VIDEO"), (AccessTabId, "ACCESSIBILITY"), ("pad", "GAMEPAD"), (LabTabId, "LAB"),
        };

        // --- the words (every line at most 8 words; the HUD at most 3) ------------------------------------------------------------
        public const string PauseQuitLabel = "QUIT";
        public const string PauseSaveQuitLabel = "SAVE & QUIT";
        public const string PauseQuitSure = "SURE? PRESS AGAIN";
        public const string TitleProgressLabel = "PROGRESS";
        public const string ResultsDetailsLabel = "DETAILS";
        public const string ResultsSummaryLabel = "SUMMARY";
        public const string SetupModeLabel = "MODE";
        public const string SetupHeatLabel = "HEAT";
        public const string SetupMapLabel = "MAP";
        public const string StaleTip17 = "STALE GUN: SWAP TO FRESHEN IT";
        public const string SlideChargeHint17 = "SLIDE BURST: RECHARGING";
        public const string ContractBadgeReward17 = "REWARD: THE BADGE";
        public const string ContractDormantLine17 = "+{0} MORE IN A LATER BUILD";

        /// <summary>
        /// The onboarding prompts (glyph + verb, NIGHT17.md U3-30.8): the input action whose glyph leads, the verb, in the
        /// order they may show. Each shows when it's useful (MenuRules.PromptUseful's cases), goes on its first use and comes
        /// back after PromptReshowAfter s unused; at most PromptMaxShown at once; never pausing.
        /// </summary>
        public static readonly (string id, string action, string verb)[] Prompts =
        {
            ("boost", "dash", "BOOST"),
            ("slide", "slide", "SLIDE"),
            ("jump", "jump", "JUMP"),
            ("zoom", "altFire", "ZOOM"),
            ("swap", "quickSwap", "LAST GUN"),
            ("wheel", "radial", "WHEEL"),
            ("buy", "interact", "BUY (HOLD)"),
        };
        public const double PromptReshowAfter = 60.0;
        // NIGHT17 round 1 (the lead: "[TAB] WHEEL sitting at the bottom centre through fights"): no prompt while an enemy is
        // this close (m) or on DOWN; these prompts show once only (gone for good after the first use) and only in a break.
        public const double PromptQuietRange = 15.0;
        // Round 2 (the reviewer: key hints mid-fight): no prompt within this many s of a shot, a swap or a hit (taken or
        // given), nor while sprinting or sliding; each prompt shows at most PromptMaxShows times a session.
        public const double PromptActionQuiet = 3.0;
        public const int PromptMaxShows = 3;
        public static readonly string[] PromptsOnce = { "wheel", "swap" };
        public static readonly string[] PromptsBreakOnly = { "wheel", "swap" };
        public const int PromptMaxShown = 3;
        // The prompts' strip: bottom centre above the safe inset (px from the bottom), each chip's height and gap, and the
        // time a prompt must be useful before it shows (s; a passing moment doesn't flash a chip).
        public const float PromptStripY = 150f;
        public const float PromptChipHeight = 44f;
        public const float PromptChipGap = 14f;
        public const double PromptUsefulFor = 0.6;
        // BOOST / SLIDE are useful while moving on the ground faster than this (m/s); PUNCH within this of an enemy (m).
        public const double PromptMoveSpeed = 4.0;

        /// <summary>
        /// NIGHT17 round 1 (the lead: "the 9-word sentence toast survives"): config.tips.list's tips as at most 3 words on the
        /// HUD (by tip id; {action}: the device's glyph). The web's sentences stay in config.json (its data); a tip with no
        /// line here and more than 3 words never shows (MenuRules.HudHint).
        /// </summary>
        public static readonly Dictionary<string, string> TipLines17 = new Dictionary<string, string>
        {
            ["break"] = "[{interact}] OPEN GATES",
            ["wallBuy"] = "[{interact}] BUY GUN",
            ["forge"] = "[{interact}] FORGE GUN",
            ["relay"] = "[{interact}] LIGHT RELAY",
            ["ready"] = "[{interact}] START ROUND",
            ["powerUp"] = "POWER-UP · GRAB IT",
            ["perk"] = "[{interact}] BUY PERK",
            [StaleTipId] = "STALE · SWAP GUNS",
        };

        /// <summary>Known long notices (config.ui.notices, the lead's hints) by their full text, as at most 3 words.</summary>
        public static readonly Dictionary<string, string> HintLines17 = new Dictionary<string, string>
        {
            ["GAMEPAD CONNECTED · SETTINGS > GAMEPAD"] = "GAMEPAD CONNECTED",
            ["THIS GAMEPAD DOES NOT REPORT THE STANDARD LAYOUT: BUTTONS MAY BE SWAPPED · SEE SETTINGS > GAMEPAD"] = "PAD LAYOUT UNKNOWN",
            ["THE SAVED RUN COULD NOT BE LOADED"] = "SAVE NOT LOADED",
            ["CAN NOT SAVE HERE"] = "NO SAVE HERE",
        };

        /// <summary>
        /// Round 4 (the reviewer: the line under a mod's number ended mid-thought): each mod's line as the noun its number
        /// counts (the card reads "+25%" over "DAMAGE BEYOND 20 M"), at most 6 words. The web's sentences stay in config.json
        /// (its data); a mod missing here shows its sentence with the number cut out (MenuRules.ModLine17). Display copy only.
        /// </summary>
        public static readonly Dictionary<string, string> ModLines17 = new Dictionary<string, string>
        {
            ["skydancer"] = "DAMAGE WHILE AIRBORNE",
            ["wallbanger"] = "PER KILL ON A WALL RIDE",
            ["railgrinder"] = "SHOCK RADIUS ON SLIDE KILLS",
            ["conductor"] = "DAMAGE TO SHOCKED ENEMIES",
            ["pyre"] = "FIRE SPREAD FROM BURNING KILLS",
            ["keenEdge"] = "HEADSHOT STAGGER CHANCE",
            ["gravityWell"] = "DAMAGE TO PULLED ENEMIES",
            ["shrapnel"] = "BURST DAMAGE ON BLAST KILLS",
            ["lance"] = "DAMAGE BEYOND 20 M",
            ["styleTax"] = "DAMAGE PER RANK ABOVE BOLD",
            ["variety"] = "PER KILL WITH A NEW FAMILY",
            ["bloodPrice"] = "HEALED PER KILL",
            ["interest"] = "OF YOUR BANK EACH ROUND",
            ["scavenger"] = "MORE POWER-UP DROPS",
            ["adrenaline"] = "DAMAGE UNDER 40% HP",
            ["momentumRounds"] = "MELEE DAMAGE WITH MOMENTUM",
            ["freshStart"] = "FIRST-SHOT DAMAGE AFTER A SWAP",
            ["bountyHunter"] = "PER ELITE OR AFFIXED KILL",
            ["crossfire"] = "DAMAGE FROM A NEW FAMILY",
        };

        /// <summary>Round 4: the mods that pay points (their glyph and hero may be gold: gold is points only).</summary>
        public static readonly string[] ModsPayingPoints = { "wallbanger", "variety", "interest", "bountyHunter" };

        /// <summary>The first-run lessons as glyph + verb (design.onboarding's ids; {action}: the live key or button).</summary>
        public static readonly Dictionary<string, string> LessonLines17 = new Dictionary<string, string>
        {
            ["moveShoot"] = "SHOOT THE HUSKS",
            ["dashSlide"] = "[{dash}] BOOST",
            ["wallBuy"] = "[{interact}] BUY GUN",
            ["perks"] = "[{interact}] BUY PERK",
            ["powerProtocol"] = "[{interact}] POWER ON",
        };
    }
}
