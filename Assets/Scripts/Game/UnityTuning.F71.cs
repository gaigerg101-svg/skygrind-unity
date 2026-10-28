// NIGHT71 builder F (F1, T-152a "the camera's chop measured honestly"): the bench's Unity-only numbers (CLAUDE.md rule 5). Measurement
// only: none of these changes what the game does or draws; they are read only while `-skygrindBench` runs (PlayerChecks, BenchLoop71).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>F1: a frame longer than this many display refresh periods missed a refresh (with VSYNC on it is shown twice: the chop a
        /// player sees). The bench counts these on both arms, since the fixed 6.94 ms count reads the panel's own period under VSYNC.</summary>
        public const double BenchMissedRefresh71 = 1.5;

        /// <summary>F1: the player-loop slots the bench prints, ranked by how much more time they take in the worst 1 % of frames than in the
        /// median band (BenchLoop71.Report).</summary>
        public const int BenchLoopTop71 = 8;

        /// <summary>F1: player-loop rows reserved per bench second (the uncapped bench runs about 260-370 fps; frames past the reserve are
        /// counted, not recorded).</summary>
        public const int BenchLoopRowsPerSecond71 = 600;

        /// <summary>F1: a slot's time is kept per frame as a float in ms; more slots than this are timed together in the last one
        /// (Unity 6000.3's player loop has about 110 phase children).</summary>
        public const int BenchLoopMaxSlots71 = 192;

        // --- F4: the reel taps71 (FeelReel.N71.Taps.cs): a KESTREL tap pressed and released inside a kill's freeze; reel-only numbers ---------
        /// <summary>F4: taps tried per kind (a freeze started by a release tap's kill; one started by a press-step shot's kill).</summary>
        public const int Taps71Reps = 4;
        /// <summary>F4: m ahead a 1-hp husk is placed, the first clear one of these (a546's ladder: 8 m found no clear floor on some maps).</summary>
        public static readonly double[] Taps71DistancesM = { 8, 6, 5, 4, 3 };
        /// <summary>F4: game s after placing (the KESTREL's 0.483 s tap interval and the swap's raise have passed).</summary>
        public const double Taps71SettleS = 0.9;
        /// <summary>F4: game s the trigger is held for the press-step kind's charged shot (past charge40's 0.9 s FULL).</summary>
        public const double Taps71ChargeHoldS = 1.2;
        /// <summary>F4: game s between the charged shot and the press-step kill (past the 0.483 s tap interval, inside charge35's 2.5 s
        /// cooldown, so the press fires on its press step).</summary>
        public const double Taps71AfterChargeS = 1.0;
        /// <summary>F4: rad of look pitch for the charged shot, turned away from the husk (it must not be the kill).</summary>
        public const double Taps71AwayPitchRad = 1.0;
        /// <summary>F4: real s to wait for the freeze-starting kill.</summary>
        public const double Taps71KillWaitS = 1.5;
        /// <summary>F4: game s after the freeze for the tap's shot (the tap waits for the 0.483 s interval: never lost, never doubled).</summary>
        public const double Taps71FireWaitS = 1.0;
        /// <summary>F4: real s cap of the whole scenario.</summary>
        public const double Taps71WallS = 240;
    }
}
