// NIGHT15 U3-28.8 (builder K): the pad's Unity-only texts (CLAUDE.md rule 5), each with why the web has none. The
// wheel's numbers (wheelHold, wheelStickDeadzone) and the rumble's defaults are design: Design/unity-config.json
// design.pad, read through GameConfig.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The radial's middle line while the pad's wheel is open and nothing is pointed at: the web's radial is mouse-only
        // (keyboardMouse.js; its line is "POINT · RELEASE " + the radial key), so it has no pad text.
        public const string PadWheelPrompt = "RIGHT STICK · RELEASE";

        // Nintendo glyph words by standard button (PadButtonName order: south, east, west, north, the shoulders, the
        // triggers, Select, Start, the stick clicks, the d-pad). The Input System maps a pad by position (its
        // SwitchProControllerHID's buttonSouth is the button Nintendo prints B on), so A / B and X / Y swap: south reads B,
        // east A, west Y, north X. MINUS / PLUS in words (the TMP atlases' charset, as the PlayStation set), the stick clicks
        // LS / RS as Xbox's. The web shows one neutral set (gamepad.js PAD_GLYPHS: a browser can't tell pads apart).
        public static readonly string[] NintendoGlyphs =
        {
            "B", "A", "Y", "X", "L", "R", "ZL", "ZR", "MINUS", "PLUS", "LS", "RS", "D-PAD UP", "D-PAD DOWN", "D-PAD LEFT", "D-PAD RIGHT",
        };

        // The names that mark a Nintendo pad: a substring (any case) of its layout, display name, product or maker. The
        // Input System's SwitchProControllerHID (layout "SwitchProControllerHID", "Switch Pro Controller", product "Pro
        // Controller", maker "Nintendo Co., Ltd."), Joy-Con, and the Switch console's own NPad layout (UNVERIFIED: that
        // package is under NDA). An XInput or DualShock pad is told by its type first. The web has no device detection.
        public static readonly string[] PadNintendoNames = { "Nintendo", "Switch", "Joy-Con", "Pro Controller", "NPad" };

        // For the lead (not wired: UiHotbar.cs is the lead's). UiHotbar :232 and :335 test PlayStation only, so a Nintendo
        // pad falls to the neutral texts, which name Y and LT where a Nintendo pad prints X and ZL. Same form as
        // UnityTuning.Hotbar.cs' HudPadCycle* / HudPadAlt*.
        public const string HudPadCycleNintendo = "X NEXT · D-PAD LEFT BACK";
        public const string HudPadAltNintendo = "ZL";
    }
}
