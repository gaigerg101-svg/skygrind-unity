// NIGHT12 U3-25.7 .. U3-25.11 (agent F, the meta's screens): Unity-only layout numbers (px at 1080p, the canvas's
// reference height) and ids for the run setup, CONTRACTS, WEAPONS, the PAYOFF strip, the toasts and the first-run guide.
// The web's menus.css panels are fluid, so none of these has a config.js counterpart; colours come from config.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- shared by the meta's texts ----------------------------------------------------------------------------------
        // Text boxes: an h1 head (as SETTINGS' head), a body line, a small / label line, the extra room an h3 line needs.
        public const float LineBody = 26f;
        public const float LineSmall = 24f;
        public const float LineGapSmall = 4f;
        // Letter spacing (TMP units: 1/100 em): names (menus.css .badge b: 0.12em), labels (theme.js .sk-label: 0.16em),
        // the hint and toast heads (menus.css .hint: 0.08em).
        public const float NameSpacing = 12f;
        public const float LabelSpacing = 16f;
        public const float HintSpacing = 8f;

        // --- the run setup on TITLE (SURVIVAL | DAILY, the HEAT rungs) ------------------------------------------------
        // The row sits this far above the PLAY button's centre (the shared types left y = +80 for it, PLAY at -10).
        public const float SetupAbovePlay = 90f;
        // The info line (today's DAILY, the HEAT rule, the locks) sits this far above the row's centre.
        // The info line's box width (one line; long texts end with an ellipsis).
        // A mode button's width, every row button's height, a HEAT chip's width, the "HEAT" label's width.
        // The gap between buttons of a group, and between the mode group and the HEAT group.
        public const float SetupGroupGap = 30f;
        // The underline of the picked button (the SETTINGS tabs' "On" line is 3 px too).
        // The unlock flag the DAILY waits on (Contracts' design: a contract grants it; the run setup shows the lock).

        // --- the best grade per HEAT rung on TITLE ----------------------------------------------------------------------
        // The row's gap under the title column's last button (QUIT), and its box.
        public const float BestRowGap = 30f;
        public const float BestRowWidth = 1200f;
        public const float BestRowHeight = 30f;

        // --- CONTRACTS and WEAPONS (scrolling lists) --------------------------------------------------------------------
        // The head's centre, the list's top edge and height, the BACK button's centre (y from the screen centre).
        public const float MetaHeadY = 420f;
        public const float MetaListTop = 360f;
        public const float MetaListHeight = 720f;
        public const float MetaBackY = -430f;
        // The list's width, and the horizontal padding inside a row.
        public const float MetaListWidth = 1240f;
        public const float MetaRowPad = 16f;
        // ScrollRect units per wheel unit: InputSystemUIInputModule turns one wheel tick into 6 units (scrollDeltaPerTick),
        // so 14 moves about one contract row per tick (uGUI's default of 1 moves 6 px).
        public const float MetaScrollSensitivity = 14f;
        // The rows' back (menus.css .badge: rgba(255,255,255,0.04)), the bars' back (.badge .pb: 0.12) and the faint edge
        // of an unfinished row (theme.js --faint: the text colour at 0.16).
        // A progress bar's height (menus.css .badge .pb: 5 px; 6 for the 1080p canvas).
        public const float BarHeight = 6f;
        // A contract row: its height, the gap between rows, the name / desc boxes, the bar's width, the status box; the
        // tops (px under the row's top) of the name and desc (left), the value, the bar and the status (right).
        public const float ContractRowHeight = 80f;
        public const float ContractRowGap = 8f;
        public const float ContractNameWidth = 700f;
        public const float ContractBarWidth = 300f;
        public const float ContractStatusWidth = 460f;
        public const float ContractNameY = 10f;
        public const float ContractDescY = 44f;
        public const float ContractValueY = 8f;
        public const float ContractBarY = 36f;
        public const float ContractStatusY = 48f;
        // A gun row: its height and gap, the tier pips (size, gap; 3 tiers tonight: design.mastery), the track bar's width,
        // where the finishes start (x in the row), a swatch (width, height, gap, the frame that turns cyan when it's worn);
        // the tops (px under the row's top) of the name, the pips, the tier text, the track text, the bar, the finish's
        // name and the swatches; the name and tier boxes' widths.
        public const float WeaponRowGap = 8f;
        public const float TierPipSize = 14f;
        public const float TierPipGap = 6f;
        public const int MasteryPips = 3;
        public const float WeaponBarWidth = 420f;
        public const float FinishX = 560f;
        public const float SwatchWidth = 64f;
        public const float SwatchHeight = 36f;
        public const float SwatchGap = 10f;
        public const float SwatchFrame = 3f;
        public const float WeaponNameY = 10f;
        public const float WeaponPipY = 50f;
        public const float WeaponTierY = 45f;
        public const float WeaponTrackY = 74f;
        public const float WeaponBarY = 104f;
        public const float FinishLabelY = 12f;
        public const float SwatchY = 44f;
        public const float WeaponNameWidth = 500f;
        public const float WeaponTierWidth = 280f;

        // --- the PAYOFF strip on DOWN and RESULTS (the right column) ----------------------------------------------------
        // The panel's left edge and top (from the screen centre), its width and bottom: x +500..+900, y +330..-250.
        public const float PayoffTop = 330f;
        public const float PayoffWidth = 400f;
        public const float PayoffBottom = -250f;
        // Inside: the padding, a text line's height (small / body), the room under a bar, the gap before a section.
        public const float PayoffPad = 16f;
        public const float PayoffLine = 24f;
        public const float PayoffBodyLine = 28f;
        public const float PayoffBarRoom = 12f;
        public const float PayoffSectionGap = 12f;
        // At most this many lines per list (contracts done, mastery), the rest summed up as "+N MORE"; the nearest
        // contracts shown (the strip's "3 nearest", Research/retention section 5 #2).
        public const int PayoffListMax = 3;
        public const int PayoffNearest = 3;

        // --- toasts, the hint and the lesson card (top left, under hud.edgeMargin: never over the reticle) ---------------
        // A card's width (menus.css .hint: max-width 520 px), its padding (8 px 16 px), the gap between stacked cards.
        public const float ToastWidth = 520f;
        public const float ToastPadX = 16f;
        public const float ToastPadY = 8f;
        public const float ToastGap = 8f;
        // A toast's life (s) and the fade at each end (menus.css .hint: opacity over 300 ms); at most this many at once,
        // the rest wait their turn.
        public const double ToastLife = 3.2;
        public const double CardFade = 0.3;
        public const int ToastMax = 4;
        // A toast's line heights (the head at the body size, the sub line at the small size).
        public const float ToastHeadLine = 26f;
        public const float ToastSubLine = 22f;
        // The hint's font size (menus.css .hint: 18 px).
        // (NIGHT13 S9: the lesson card's fixed 9 s is gone: it stays until its thing is done; UnityTuning.Impressions.)

        // --- SETTINGS: the Unity-only FIRST-RUN GUIDE toggle (design.onboarding is skippable there) ---------------------
        // Its key, its tab (config.ui.settingsTabs' GAME tab, which had no row Unity applies until now) and its default.
        public const string OnboardingSetting = "onboarding";
        public const string GameTabId = "game";
        public const bool OnboardingDefault = true;
    }
}
