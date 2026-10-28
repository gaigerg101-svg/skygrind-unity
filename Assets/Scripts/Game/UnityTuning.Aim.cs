// NIGHT15 (the shared commit, lead): the aim assist rows' player-facing names (U3-28.7) and the pad's RUMBLE row
// (U3-28.8). UI text and a row's range only: the defaults are design.aimAssist / design.pad in Design/unity-config.json.
// The web names its modes off / assist / snap / lock (shown OFF / ASSIST / SNAP / HARD LOCK) and has no rumble strength.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // SNAP MODE's options as a player reads them (GG, 2026-10-05: "auto aim is too strong"; NIGHT15 rule 6). The saved
        // values stay the web's mode names, so a profile reads the same keys.
        public static readonly (string key, string label)[] AimAssistLabels =
        {
            ("off", "OFF"), ("assist", "LIGHT"), ("snap", "STRONG"), ("lock", "LOCK"),
        };

        // The rows' names (the web's are SNAP MODE / SNAP MODE (PAD): NIGHT15's capture 4 read them as jargon beside the
        // plain values).
        public const string AimAssistRowLabel = "AIM ASSIST";
        public const string PadAimAssistRowLabel = "AIM ASSIST (PAD)";

        // The pad's RUMBLE row (GAMEPAD tab): 0..1 times config.gamepad.vibration's motor strengths, in steps of 0.1. The web
        // rumbles at full strength only (keyboardMouse.js rumble). Its default is design.pad.rumbleDefault.
        public const string PadRumbleSetting = "padRumble";
        public const string PadRumbleLabel = "RUMBLE";
        public const string PadRumbleDesc = "How strong the rumble is (VIBRATION turns it on)";
        public const double PadRumbleStep = 0.1;
    }
}
