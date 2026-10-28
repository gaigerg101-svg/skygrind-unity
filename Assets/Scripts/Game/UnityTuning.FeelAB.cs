// NIGHT18 (the shared commit, the lead's): the A / B switch's numbers (FeelAB), the pick helpers every builder uses for a
// NIGHT18 number beside its NIGHT17 one, the FOV KICK / CAMERA SHAKE settings' keys, and the reel's clock and probe.
// Unity-only: the web has no A / B switch, no reel and no such settings.
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the pick: NIGHT18's value or NIGHT17's, by FeelAB.On (named numbers on both sides, never literals) ---------------

        /// <summary>NIGHT18: `n18` while FeelAB is on, else `n17` (UnityTuning.Pick(FeelAB.On, UnityTuning.X, UnityTuning.XN17)).</summary>
        public static float Pick(bool on, float n18, float n17) => on ? n18 : n17;
        public static double Pick(bool on, double n18, double n17) => on ? n18 : n17;
        public static int Pick(bool on, int n18, int n17) => on ? n18 : n17;
        public static bool Pick(bool on, bool n18, bool n17) => on ? n18 : n17;
        public static Color Pick(bool on, Color n18, Color n17) => on ? n18 : n17;
        public static Vector3 Pick(bool on, Vector3 n18, Vector3 n17) => on ? n18 : n17;

        // --- the switch ----------------------------------------------------------------------------------------------------

        /// <summary>The command-line flag: `-skygrindFeelAB off` renders NIGHT17's look for the whole run (the reel's A side).</summary>
        public const string FeelABFlag = "-skygrindFeelAB";
        /// <summary>The live toggle in a developer build. F1-F3, F8 and F9 are taken (DevKeys, SurvivalGame.Debug.cs).</summary>
        public const Key FeelABKey = Key.F6;
        /// <summary>The LAB row's settings key (never saved across boots: FeelAB.Boot sets it from the switch).</summary>
        public const string FeelABSetting = "feelAB";
        public const string FeelABLabel = "FEEL N18";
        public const string FeelABDesc = "TONIGHT'S FEEL (OFF: NIGHT17'S); F6 FLIPS IT";
        /// <summary>The corner chip's words, size and place (a developer build only; bottom-left, inside the 56 px safe inset).</summary>
        public const string FeelABChipN18 = "FEEL N18", FeelABChipN17 = "FEEL N17";
        public const float FeelABChipInset = 56f, FeelABChipWidth = 220f, FeelABChipHeight = 30f;
        /// <summary>Above every game canvas (the toasts sit at 100-200).</summary>
        public const int FeelABChipSort = 900;

        // --- FOV KICK / CAMERA SHAKE (ACCESS; 0-100 %, stored 0-1; builder M applies them in CameraFeel, builder N shows them) --

        /// <summary>NIGHT18 (NIGHT18.md's CAMERA FX switch): every FOV kick (boost, sprint, slide, the impact layer's) x this.</summary>
        public const string FovKickSetting = "fovKick";
        /// <summary>NIGHT18: every camera shake (trauma, the kill's crunch) x this.</summary>
        public const string CameraShakeSetting = "cameraShake";
        /// <summary>GG's CAMERA FX switch default: 100 %.</summary>
        public const double FovKickDefault = 1.0, CameraShakeDefault = 1.0;
        /// <summary>The rows' step (10 %).</summary>
        public const double CameraFxStep = 0.1;
        public const string FovKickLabel = "FOV KICK", CameraShakeLabel = "CAMERA SHAKE";
        public const string FovKickDesc = "SCALES THE VIEW'S WIDEN ON BOOST, SPRINT AND SLIDE";
        public const string CameraShakeDesc = "SCALES THE SCREEN SHAKE";

        // --- the reel's clock (FeelClock) and probe (FeelProbe) -------------------------------------------------------------

        /// <summary>The reel's fixed capture rate: every recorded frame is exactly 1 / ReelFps of game time.</summary>
        public const int ReelFps = 60;
        /// <summary>The reel's pre-roll before a scenario's event (ms): the frames before it are recorded too.</summary>
        public const int ReelPreRollMs = 300;
        /// <summary>The post-roll after a kill / the flourish (ms).</summary>
        public const int ReelPostRollKillMs = 1200, ReelPostRollFlourishMs = 1500;
        /// <summary>The probe's ring: the most frames one scenario records (30 s at 60 fps), and the most enemies / UI elements / chunk points a frame logs.
        /// NIGHT60 V (B3 a): ProbeMaxEnemies 8 -> 32. FeelProbe.Enemy drops every report past it without a trace; at 8 NIGHT59's a258 / a355 filled it on
        /// 94 frames each (Logs/feel/n59-b2/*/telemetry.jsonl), and the swarmers and the spitters now report before the N19 / N20 / boss kinds (the render
        /// order). 32 holds the director's maxAlive (24; N60ProbeTests pins it) plus 8 for what the director does not count (bosses, a level's own
        /// spawns: how many was not checked).</summary>
        public const int ProbeMaxFrames = 1800, ProbeMaxEnemies = 32, ProbeMaxUi = 160, ProbeMaxChunkPoints = 64;
        /// <summary>The probe re-finds the root canvases every this many frames (a canvas made mid-scenario is seen).</summary>
        public const int ProbeRescanFrames = 15;
        /// <summary>The probe's per-enemy world anchor: its sim position + this many m (about a husk's middle), projected each frame.</summary>
        public const float ProbeAnchorHeight = 0.9f;
        /// <summary>kill20 / kill6: the player's other spots tried (m from the spawn, 8 headings each) when the spawn has no floor at exactly the distance.</summary>
        public static readonly double[] ReelMoveTries = { 4, 8, 12, 16 };
        /// <summary>horde5: one blast that kills five: the COMET (weapons.rocket.splashRadius 4 m) on a pack within 1 m of its middle, 7 m out (outside the splash). NIGHT17's THUNDERCLAP (an 8 deg cone, the front bodies blocking) took 1-2 of 5.</summary>
        public const string ReelHordeBlastGun = "rocket";
        public static readonly double[] ReelHordeDistances = { 7.0, 7.6, 6.4 };
        public static readonly double[] ReelHordeLaterals = { 0.0, 0.6, -0.6 };
        /// <summary>The land scenario's drop (m above open floor; NIGHT18.md: a 3 m drop).</summary>
        public const double ReelLandHeight = 3;
        /// <summary>land: the camera looks this far down while it falls and lands (a level look hides a 0.14 m dip; NIGHT17's strip used -58).</summary>
        public const double ReelLandPitchDeg = -20;
        /// <summary>roundend records from the last enemy of round 1 once at least this many died (the fight before is off the record).</summary>
        public const int ReelRoundEndKillsBeforeRecord = 3;
        /// <summary>ui-results: DOWN plays this many frames off the record before the recording starts (RESULTS follows the down time).</summary>
        public const int ReelResultsRecordAfterFrames = 60;
        /// <summary>The reel's JPG quality (frames at 1920 x 1080).</summary>
        public const int ReelJpgQuality = 90;
    }
}
