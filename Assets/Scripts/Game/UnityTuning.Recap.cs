// NIGHT14 U3-27 (the shared types): agent V's Unity-only numbers, U3-27.9 (WHAT GOT YOU: the orbit camera, the outline,
// the name tag, the skip). Each with a comment saying why the web has no value for it (CLAUDE.md rule 5): the web's DOWN
// shows the recap text only (hud.js setDown), so every number here is the replay's own. The replay's timing (seconds,
// replaySeconds, slowSeconds, slowRate) is design.recap in Design/unity-config.json.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The settings key that turns WHAT GOT YOU off (a Unity-only SETTINGS row, GAME tab: the web's DOWN has no replay)
        // and its default (on: the flag design.recap.enabled decides whether the feature exists at all).
        public const string RecapSetting = "recap";
        public const bool RecapDefault = true;

        // --- the flight recorder (FlightRecorder: the ring's per-frame slots, preallocated at boot) ---------------------
        // Enemy slots a frame: twice director.maxAlive (24). A frame with more alive keeps the first 48 in the registry's order.
        public const int RecapMaxEnemies = 48;
        // Mark slots a frame: this step's hits / kills / hurts first, then the spitters' globs (pool 36) and puddles, the
        // kill drops, the player's rockets and nails; past 64 the rest of that frame's marks are dropped (drawn only).
        public const int RecapMaxMarks = 64;

        // --- the playback (SurvivalGame.Recap.cs) ------------------------------------------------------------------------
        // The last frame holds this long (real s) before DOWN goes on: a beat to read the killing blow.
        public const float RecapHoldSeconds = 0.6f;
        // A press in the first 0.3 s doesn't skip: a shot or a jump already on its way at the death would skip it unseen.
        public const float RecapSkipGrace = 0.3f;

        // --- the orbit camera (RecapStage; main.js's attract flythrough is the model: a slow orbit, a little above) ---------
        // Radius (m) round the point between you and the killer: this much at least, plus PerMeter of your distance to it,
        // at most Max (a swarm's far killer stays in frame without the arena turning into a map).
        public const float RecapOrbitRadiusMin = 6f, RecapOrbitRadiusPerMeter = 0.8f, RecapOrbitRadiusMax = 20f;
        // How far above the ground plane it looks down (deg), and how fast it goes round (deg per real s; ~5 s replay: 70 deg).
        public const float RecapOrbitPitchDeg = 38f;
        public const float RecapOrbitSpeedDeg = 14f;
        // The look-at point and the radius ease toward their targets at this rate (1/s): no jump when the killer moves.
        public const float RecapCameraEase = 5f;
        // A wall between the look-at point and the camera (the C# collider's ray) pulls it in, this far (m) off the wall,
        // never nearer than MinRadius.
        public const float RecapCameraClearance = 0.4f;
        public const float RecapCameraMinRadius = 2.5f;
        // The look-at point's height over the feet (m), and the replay's field of view (deg, the web's default 90 is a
        // first-person view; from outside, 60 keeps the bodies' proportions).
        public const float RecapLookHeight = 1f;
        public const float RecapFovDeg = 60f;

        // --- what it draws (RecapStage) ----------------------------------------------------------------------------------
        // The killer's outline: a red-orange (colors.enemy) hull this far (m) off each of its parts, its colour x Boost so
        // the edge reads brighter than the body against the dusk.
        public const float RecapOutlineWidth = 0.07f;
        public const float RecapOutlineBoost = 1.6f;
        // A hit / kill mark stays drawn this long in replay time (s) as a white cube of this size (m): a step is 8 ms.
        public const float RecapHitShow = 0.15f;
        public const float RecapHitSize = 0.16f, RecapKillSize = 0.32f;
        // The player's projectiles and the kill drops as cyan cubes (m): the web's own models are the live views' (not drawn
        // in the replay).
        public const float RecapNailSize = 0.12f, RecapRocketSize = 0.3f, RecapDropSize = 0.4f;
        // The puddle's disc thickness and lift (m), SpitterView's (0.04 m thick at +0.03).
        public const float RecapPuddleThickness = 0.02f, RecapPuddleLift = 0.03f;
        // Where you were: a cyan capsule (the movement body's size) and a nub this far forward at eye height showing your look.
        public const float RecapNoseForward = 0.45f, RecapNoseSize = 0.2f;

        // --- the UI (UiRecap.cs; px at 1080p) ----------------------------------------------------------------------------
        // WHAT GOT YOU's heading from the top, the skip hint from the bottom.
        public const float RecapHeadTop = 70f, RecapHeadWidth = 1200f, RecapHeadHeight = 100f;
        public const float RecapHintBottom = 64f, RecapHintWidth = 900f, RecapHintHeight = 36f;
        // The killer's name tag: its box, and how far (m) above the killer's height the tag's foot points.
        public const float RecapTagWidth = 640f, RecapTagHeight = 48f, RecapTagAbove = 0.45f;
        // With no killer on screen (THE FALL, or it is off the frame) the name sits this far (px) under the heading.
    }
}
