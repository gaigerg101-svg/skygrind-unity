// NIGHT14 U3-27 (the shared types): agent K's Unity-only numbers, U3-27.7 (the kiosk's hints panel, the attract's layout and camera smoothing). Each with a comment saying why
// the web has no value for it (CLAUDE.md rule 5).
// The hints' sizes are kiosk.js's CSS (.kiosk-hints: a stylesheet string in the module, not config.js), in px at 1080p;
// their texts are kiosk.js drawHints' literals. The attract's numbers are the port's own: the web's attract orbits two
// fighters and never needs them.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the hints panel (kiosk.js CSS) ---------------------------------------------------------------------------
        public const float KioskHintsRight = 76.8f; // .kiosk-hints right: 4vw (of the 1920 px reference width)
        public const float KioskHintsBottom = 64.8f; // .kiosk-hints bottom: 6vh (of 1080)
        public const float KioskHintsWidth = 900f; // the box each line is right-aligned in (text-align: right): wider than the longest line
        public const float KioskHeadSize = 22f; // .kiosk-hints h3 font-size (font-display)
        public const float KioskHeadSpacing = 50f; // h3 letter-spacing 0.5em (TMP characterSpacing is in em / 100)
        public const float KioskHeadGap = 8f; // h3 margin-bottom
        public const float KioskHeadLine = 1.2f; // the h3's line-height: CSS "normal" (about 1.2)
        public const float KioskRowSize = 26f; // .kiosk-hints div font-size (700)
        public const float KioskRowSpacing = 8f; // div letter-spacing 0.08em
        public const float KioskLineHeight = 1.35f; // div line-height (the rows' and the foot's)
        public const float KioskKeyMinWidth = 110f; // .kiosk-hints b min-width
        public const float KioskKeyPad = 8f; // b padding 0 8px (each side)
        public const float KioskKeyGap = 14f; // b margin-right (the key's box to its label)
        public const float KioskKeyBackAlpha = 0.75f; // b background rgba(12, 13, 18, 0.75): the theme's panel at this alpha
        public const float KioskFootSize = 16f; // .foot font-size
        public const float KioskFootSpacing = 30f; // .foot letter-spacing 0.3em
        public const float KioskFootGap = 10f; // .foot margin-top
        public const float KioskFootAlpha = 0.8f; // .foot opacity
        public const string KioskHead = "CONTROLS"; // drawHints' <h3>
        // drawHints' foot said "CLICK SURVIVAL TO PLAY" (the web's mode button); the port's TITLE button is PLAY
        // ("CLICK PLAY TO PLAY" would stutter).
        public const string KioskFoot = "CLICK PLAY TO START";

        /// <summary>
        /// drawHints' rows in its order: the bindings shown (their KeyLabels run together, as `${key('forward')}${key('left')}
        /// ...}`), or a fixed key when `actions` is null, then what they do. Rows whose every action is in PauseKeysSkip (the
        /// ARCLINE's: SURVIVAL's mover has the rope off) are left out, as PAUSED's YOUR KEYS card leaves them out.
        /// </summary>
        public static readonly (string[] actions, string fixedKey, string label)[] KioskRows =
        {
            (new[] { "forward", "left", "back", "right" }, null, "MOVE"),
            (null, "MOUSE", "LOOK · FIRE · ALT"),
            (new[] { "jump" }, null, "JUMP ×2"),
            (new[] { "dash" }, null, "DASH"),
            (new[] { "slide" }, null, "SLIDE / SLAM"),
            (new[] { "punch" }, null, "PUNCH · PARRY CYAN"),
            (new[] { "grapple" }, null, "ARCLINE"),
            (new[] { "interact" }, null, "HOLD TO BUY"),
        };

        // --- the attract (port-only) ---------------------------------------------------------------------------------
        // The flythrough's centre follows the autopilot and its target with this rate (1/s, exponential): the web's attract
        // orbits one duel's two fighters (main.js :2337, unsmoothed); the port's target is the horde, and each new target
        // would jump the orbit's centre.
        public const double AttractCamFollowRate = 1.5;

        // The attract's player HP: the web's attract calls player.setGodMode(true) (main.js :729: health.js's invulnerable
        // test). The port's Health has no god flag in SURVIVAL (its isInvulnerable is the mover's i-frames), so the attract
        // pins HP the way the fingerprint and the smoke do (SurvivalSim.RunFingerprint), again whenever a perk resets it.
        public const double AttractGodHp = 1e6;

        // An attract run ending (a fall's DOWN, an exfil's RESULTS) starts a fresh one ("the show goes on"); one that ends
        // again within this many seconds stops the attract instead (a restart loop would rebuild the world every frame).
        public const double AttractMinRunSeconds = 2.0;
    }
}
