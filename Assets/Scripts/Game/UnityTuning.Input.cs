// NIGHT13 U3-26.7 (agent I: the input layer and the gamepad): the Unity-only input numbers and texts (glyph sets, the
// pad-layout storage key, rumble), each with a comment saying why the web has no value for it. The web's own module
// constants (gamepad.js, bindings.js, keyboardMouse.js) are cited C# constants in Core, not here.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The player's pad remap in PlayerPrefs: the web's localStorage key (main.js :188 PAD_LAYOUT_KEY), kept so the
        // stored layout has the same name and JSON (GamepadMath.ReadPadLayout checks it on read). The web keeps it in
        // main.js, not in a pure module, so Core has no copy.
        public const string PadLayoutKey = "skygrind.padLayout.v1";

        // NIGHT19 U3-33.3: the v2 layout's store (design.pad.n19: {"version":2,"layout":{..}}). The web never had a second
        // layout; the v1 key is left as it was (a rollback), InputPrefs migrates it once.
        public const string PadLayoutKeyV2 = "skygrind.padLayout.v2";

        // NIGHT20 U3-34.4: the v3 layout's store (design.pad.n20: RB NEXT WEAPON, Y SPECIAL + ARCLINE; {"version":3,..}). The
        // web never had a third layout; the v2 and v1 keys are left as they were (a rollback), InputPrefs migrates v2 once.
        public const string PadLayoutKeyV3 = "skygrind.padLayout.v3";

        // NIGHT24: the v4 layout's store (design.n24.padY: Y NEXT WEAPON, RB SPECIAL + ARCLINE; {"version":4,..}). Same reason as v2 / v3:
        // the web never had a fourth layout; the older keys stay as they were (a rollback), InputPrefs migrates v3 once.
        public const string PadLayoutKeyV4 = "skygrind.padLayout.v4";

        // NIGHT25: the v5 layout's store (design.n25.meleeFire: PUNCH on RB, SPECIAL + ARCLINE on R3; {"version":5,..}). Same reason: the older keys stay (a rollback).
        public const string PadLayoutKeyV5 = "skygrind.padLayout.v5";

        // NIGHT20: the first-pad hint card (PadHintCard): its rows (an input action whose live glyph leads, then a word; the
        // web has no pad card) and how long it stays (s) before it fades.
        public static readonly (string action, string word)[] PadHintRows =
        {
            ("sprint", "SPRINT"), ("punch", "MELEE"), ("nextWeapon", "NEXT GUN"), ("altFire", "ZOOM"),
        };
        public const double PadHintSeconds = 8.0;

        // NIGHT20 row 6: CONTROLS' GAMEPAD page. Its hint line before NIGHT20 (words), and with the drawn glyphs: "<A> SELECT,
        // THEN PRESS A PAD BUTTON · <B> BACK" (the glyphs placed by ControlsPad20.HintLine).
        public const string ControlsPadHint18 = "GAMEPAD · SELECT, THEN PRESS A PAD BUTTON";
        public const string ControlsPadHintSelect = " SELECT, THEN PRESS A PAD BUTTON · ", ControlsPadHintBack = " BACK";
        // The sticks' rows on the GAMEPAD page (the web's padRows: LEFT STICK MOVE, RIGHT STICK LOOK).
        public const string ControlsPadMove = "MOVE", ControlsPadLook = "LOOK";
        // L3 SPRINT: TOGGLE or HOLD (GG's L3 SPRINT switch, design.pad.n19.l3Sprint, as a setting the player can change: the
        // controller research's checklist row 12, hold / toggle for every held action). The settings key, its row's label and
        // values, and the CONTROLS row's label.
        public const string PadSprintModeSetting = "padSprintMode";
        public const string PadSprintModeLabel = "L3 SPRINT", PadSprintModeDesc = "TOGGLE: A CLICK SPRINTS UNTIL YOU STOP · HOLD: SPRINT WHILE HELD";
        public const string PadSprintToggleLabel = "TOGGLE", PadSprintHoldLabel = "HOLD";

        // --- NIGHT19 U3-33.4: the pad's drawn glyphs (PromptGlyphs, PromptText; the web draws none) ---------------------
        // The atlas the vendored Kenney Input Prompts are packed into (Assets/ThirdParty/KenneyInputPrompts/Resources; its
        // Source~/pack_atlas.py writes it on this grid: change both together): a 1024 px square, 80 px cells, 12 a row, the
        // 64 px glyph centred in each (8 px of clear edge: the mip levels never bleed a neighbour in at prompt sizes).
        public const string PromptAtlasResource = "KenneyInputPromptsAtlas";
        public const int PromptAtlasSize = 1024, PromptAtlasCell = 80, PromptAtlasGlyph = 64, PromptAtlasColumns = 12;

        // The sprite asset's name in the prompts' tags (<sprite="padglyphs" name="xboxa">): one lower-case word, so no
        // token of it reads as a key (the reel's pad scan looks for E, F, Q, R, TAB ...).
        public const string PromptSpriteAsset = "padglyphs";

        // TMP_SpriteAsset's serialized version (TMP 1.1.0+ tables), written so TMP never runs its legacy upgrade on it.
        public const string PromptSpriteAssetVersionJson = "{\"m_Version\":\"1.1.0\"}";

        // A glyph is PromptGlyphEm of the text's size tall (a button reads a little bigger than the capitals beside it), with
        // PromptGlyphRise of it above the baseline (its middle sits on the capitals' middle).
        public const float PromptGlyphEm = 1.25f, PromptGlyphRise = 0.8f;

        // The sprite names' parts: the set (Xbox, PlayStation, Switch) and the glyph (PadButtonName order by position, then
        // the left and right sticks); "hold" ends a HOLD glyph's name. Lower case, no separators (see PromptSpriteAsset).
        public static readonly string[] PromptSetNames = { "xbox", "ps", "switch" };
        public static readonly string[] PromptGlyphNames =
        {
            "a", "b", "x", "y", "lb", "rb", "lt", "rt", "view", "menu", "ls", "rs", "up", "down", "left", "right", "lstick", "rstick",
        };

        // What an action no pad button carries reads as on CONTROLS (a migrated v1 layout may leave SPRINT so).
        public const string PadUnbound = "UNBOUND";

        // The words a HOLD prompt reads as when the glyphs aren't drawn (the fallback: "HOLD X").
        public const string PromptHoldWord = "HOLD ";

        // The hotbar's pad cycle line (UnityTuning.Hotbar's HudPadCycle* in glyphs): NEXT WEAPON's button, then the previous one's.
        public const string PromptCycleNext = " NEXT", PromptCycleSep = " · ", PromptCycleBack = " BACK";

        // The pad wheel's middle between the right stick's glyph and the wheel button's (UnityTuning.PadWheelPrompt's words
        // in glyphs: "<right stick> POINT · RELEASE <Y>").
        public const string PadWheelPoint19 = " POINT · RELEASE ";

        // NIGHT19 U3-33.3: LB / RB's tab switch: the new tab's content slides in this far (px at 1080p) from the side the
        // bumper points to, over the theme's ThemeInTime (a horizontal step reads as "next tab"; the screens' own entrances
        // run down-left along the slant).
        public const float TabSlideDistance19 = 48f;

        // Xbox glyph words by standard button (PadButtonName order: A B X Y LB RB LT RT, View, Menu, the stick clicks, the
        // d-pad). The web shows one neutral set (gamepad.js PAD_GLYPHS: a browser can't tell pads apart reliably); the Input
        // System knows an XInputController. Xbox names the stick clicks LS / RS.
        public static readonly string[] XboxGlyphs =
        {
            "A", "B", "X", "Y", "LB", "RB", "LT", "RT", "VIEW", "MENU", "LS", "RS", "D-PAD UP", "D-PAD DOWN", "D-PAD LEFT", "D-PAD RIGHT",
        };

        // PlayStation glyph words by standard button (the same order), for a DualShockGamepad (DualShock 4 and DualSense):
        // words, not the shapes, because the static TMP atlases' charset lacks them (Ui.cs, HANDOFF). Select reads CREATE
        // (the DualSense's name; the DualShock 4 calls it SHARE), Start OPTIONS.
        public static readonly string[] PlayStationGlyphs =
        {
            "CROSS", "CIRCLE", "SQUARE", "TRIANGLE", "L1", "R1", "L2", "R2", "CREATE", "OPTIONS", "L3", "R3", "D-PAD UP", "D-PAD DOWN", "D-PAD LEFT", "D-PAD RIGHT",
        };
    }
}
