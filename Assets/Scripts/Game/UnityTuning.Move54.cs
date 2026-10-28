// NIGHT54 M move54: Unity-only numbers (the a354 reel's line and thresholds; the frame pacing default). The row's own numbers are in
// design.n54.move54.numbers (Game/Move54.cs).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>a354: the run line's length (m) from a room's spawn (about 1.8 s at a run).</summary>
        public const double Move54ReelRun = 18;
        /// <summary>a354: headings tried per room (evenly spaced).</summary>
        public const int Move54ReelHeadings = 16;
        /// <summary>a354: the floor scan's spacing along a heading (m).</summary>
        public const double Move54ReelScanStep = 0.25;
        /// <summary>a354: a step on the line is a floor change of at least this between two flat samples (m; under it a pop is too small to read).</summary>
        public const double Move54ReelStepMin = 0.15;
        /// <summary>a354: and at most this (m): collision.groundSnap 0.35 down, collision.stepHeight 0.4 up.</summary>
        public const double Move54ReelStepMax = 0.4;
        /// <summary>a354: a floor change over this between two samples is a ledge or a hole: the heading is not a run line.</summary>
        public const double Move54ReelLedge = 0.45;
        /// <summary>a354: a sample is flat when its floor normal's y is at least this (a ramp's samples are not a step).</summary>
        public const double Move54ReelFlat = 0.97;
        /// <summary>a354: the step sits at least this far along the line (m: the run is up to speed) and this far before its end.</summary>
        public const double Move54ReelStepFrom = 4, Move54ReelStepBefore = 3;
        /// <summary>a354: husks put beside the line (the kills that freeze the loop), the first at From m, every Every m, Side m off it.</summary>
        public const int Move54ReelHusks = 4;
        public const double Move54ReelHuskFrom = 4, Move54ReelHuskEvery = 3, Move54ReelHuskSide = 2.5;
        /// <summary>a354: a shot at most every this many frames (a press when the line to the husk is clear).</summary>
        public const int Move54ReelFireEvery = 12;
        /// <summary>a354: frames per leg at most (out with the kills, back without).</summary>
        public const int Move54ReelLegFrames = 240;
        /// <summary>a354: the feet move at least this fast (m/s, horizontal) for a frame to count as moving.</summary>
        public const double Move54ReelMoving = 3;
        /// <summary>a354: a HOLD frame: the camera moved under this share of the feet's expected distance that frame (x the world's scale outside a freeze).</summary>
        public const double Move54ReelHoldShare = 0.1;
        /// <summary>a354 GREEN: the camera's held time over NIGHT53's freeze time for the same freezes at most this (NIGHT53 holds about x1 of
        /// its freezes, a frame either way; the row's gun kill 0.05 / 0.085 = x0.59, headshot kill 0.08 / 0.12 = x0.67).</summary>
        public const double Move54ReelHoldRatio = 0.8;
        /// <summary>a354 GREEN: the largest one-frame camera height change on the step at most this share of the feet's step (NIGHT53: the whole step in one frame).</summary>
        public const double Move54ReelVyRatio = 0.5;

        /// <summary>move54 on: VSYNC's default when the player never saved one (Unity-only: the browser always synced; tearing on fast turns
        /// with it off). A saved choice always wins; the row off keeps VsyncDefault.</summary>
        public const bool Move54VsyncDefault = true;
    }
}
