// NIGHT25 (builder F): Unity-only layout and timing numbers for the lesson line (UiLesson25.cs). The web has no tutorial line; the words are
// Design/levels/CURRICULUM.md's. Nothing here is a gameplay number.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const float Lesson25Bottom = 230f; // px above the bottom edge (1080p reference): over the weapon / points block, under the crosshair
        public const float Lesson25Height = 56f; // px: the plate (one S24 line, 18 px being the floor)
        public const float Lesson25MinWidth = 260f; // px: the plate's least width
        public const float Lesson25PadX = 28f; // px: text to the plate's end
        public const double Lesson25FadeIn = 0.2; // s: the line's entrance (M9's 90-300 ms; alpha 0 on the first frame, no pop-in)
        public const double WeaponSwitch25Time = 0.15; // s: the weaponSwitch wipe across the weapon plate (carry-over 7)
        public const double WeaponSwitch25RampIn = 0.05; // s: its alpha ramps in (no pop-in)
        public const float WeaponSwitch25Alpha = 0.45f; // its peak alpha: a wipe over the card, never an opaque flash
        public const double Cue25MaxWait = 2.0; // s: the longest the DOOR OPEN cue waits for a banner to clear the top centre (a banner is 3 s; a stuck timer must not hide the cue for the whole clear)
        public const double Fade25In = 0.1; // s: the short alpha fade-in the T-12 M9 fixes share (weapon-name swap, combo plate, hotbar cells, threat chunk): <= 0.12 s as UiDeadline22's
        public const double StyleMeterFadeIn25 = 0.1; // s: the style meter block's alpha fade-in at its first showing (T-12, M9 read its first frame at full alpha as a pop-in; the slam's scale is kept; <= 0.12 s as UiDeadline22's)
        public const double KillMarkerFadeIn25 = 0.04; // s: the kill marker's fade-in (T-12, M9): about two frames, so the kill still reads at once; the web has no such marker
        // the CAMPAIGN RESULTS screen (UiCampaignResults25.cs; 1080p reference, y up from the screen centre; the web has no campaign)
        public const float CampRes25Width = 900f; // px: the head and summary lines
        public const float CampRes25SummaryY = 290f; // px: the summary line (under the head at ResultsHeadY 360)
        public const float CampRes25FirstRowY = 225f; // px: the first room row
        public const float CampRes25RowPitch = 46f; // px between rows (S32 text)
        public const int CampRes25MaxRows = 8; // rows built (the buttons start at ResultsFirstRowY -132: 225 - 7 x 46 = -97 clears them)
        public const float CampRes25NameW = 520f, CampRes25RankW = 90f, CampRes25TimeW = 440f; // px: the three columns
        public const double CampRes25FadeIn = 0.12; // s: every line's fade-in (M9; as UiDeadline22's)
        public const double CampRes25SummaryAt = 0.15, CampRes25RowsAt = 0.3, CampRes25RowStagger = 0.07; // s after the screen opens
        public const double CampRes25MoveGrace = 0.5; // s after the buttons appear in which a menu direction cannot take the focus off CONTINUE
        public const double CampRes25ButtonsAt = 0.9; // s: the buttons appear and CONTINUE takes the focus (a jump's A during the clear never takes it)
        public const double Lesson25FadeOut = 0.2; // s: its exit once the verb is used

        /// <summary>The composite placeholders no single binding names: {move} and {look}, as the keyboard + mouse and as the pad read them.</summary>
        public static readonly System.Collections.Generic.Dictionary<string, (string keys, string pad)> Lesson25Aliases = new System.Collections.Generic.Dictionary<string, (string keys, string pad)>
        {
            ["move"] = ("WASD", "L STICK"),
            ["look"] = ("MOUSE", "R STICK"),
            ["fire"] = ("LEFT CLICK", null), // the mouse buttons are not in input.bindings (a keyboard has no code for fire: it read "FIRE TO FIRE"); null: the pad reads its layout
            ["altFire"] = ("RIGHT CLICK", null),
        };

        /// <summary>The tutorial's one line per lesson step, as FORMATS (Design/levels/CURRICULUM.md, rule 5: at most 6 words). {name} = the live key on a keyboard, the
        /// live pad button on a pad (Lesson25Text.Resolve): {move} {look} {jump} {dash} {slide} {fire} {punch} {special} {grapple} {interact}. Words are text, not gameplay numbers.</summary>
        public static readonly System.Collections.Generic.Dictionary<string, string> Lesson25Lines = new System.Collections.Generic.Dictionary<string, string>
        {
            ["1.1"] = "{move} MOVE  {look} LOOK",
            ["1.2.pit"] = "{jump} JUMP THE PIT",
            ["1.2.tunnel"] = "HOLD {slide} TO SLIDE",
            ["1.3.wall"] = "{jump} OFF THE WALLS",
            ["1.3.pad"] = "STEP ON THE PAD",
            ["1.3.dash"] = "JUMP, THEN {dash} TO DASH",
            ["1.4.fire"] = "{fire} TO FIRE",
            ["1.4.arc"] = "WATCH THE ARC",
            ["1.5.punch"] = "{punch} PUNCH WHILE FIRING",
            ["1.5.charge"] = "HITS BUILD THE PULSE",
        };
    }
}
