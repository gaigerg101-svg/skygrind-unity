// NIGHT55 builder C (camera and look): Unity-only numbers (camera, render, the a255 reel's lines and thresholds). Every number here is
// view-only: no sim, no Mover, no collider reads them. Unity-free (the SimHarness compiles this file with -p:Extra for `n55look`).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---------------- step55 (Game/Move55.cs Move55Step): the grounded step's camera height, held in sim time ----------------

        /// <summary>step55: the critically damped spring's rate (rad/s) that returns the held step to the true eye height, stepped in sim time
        /// (1/120 s). A held x0 is (1 + w t) e^-wt of itself at t: with 18, 46 % at 0.1 s, 14 % at 0.2 s, 4 % at 0.3 s. The camera's largest
        /// vertical speed while it returns is w x0 / e (0.35 m: 2.3 m/s, 0.039 m a frame at 60 fps), against move54's omega 40 and its
        /// keepRate pass-through (a first frame of ~0.1 m at 60 fps). Source's stair smoothing slews ~2.9 m/s (web.md item 5).</summary>
        public const double Step55Omega = 18;
        /// <summary>step55: a grounded step (both ends on the ground or sliding) that moved the feet at least this (m) in one 120 Hz step is
        /// held whole (a 45 degree ramp at 10 m/s moves 0.059 m a step: never held; move54's stepMin).</summary>
        public const double Step55StepMin = 0.1;
        /// <summary>step55: the held offset never exceeds this (m): a staircase of snaps accumulates up to one step height, not more
        /// (Source's clamp at stepSize; collision.groundSnap 0.35 is the largest snap down).</summary>
        public const double Step55MaxOffset = 0.4;
        /// <summary>step55: a step moving the feet farther than this (m) vertically is a teleport / respawn / level start: the offset drops to 0.</summary>
        public const double Step55ResetJump = 1;

        // ---------------- ssao55 (Game/Ssao55.cs): the SSAO pass's blue noise held still ----------------

        /// <summary>ssao55: the blue-noise offset (texture UV, 0..1) put into _SSAOBlueNoiseParams.zw every frame instead of the pass's two
        /// Random.value: any fixed value gives a still pattern; 0.5 / 0.5 is the middle of the pass's own range.</summary>
        public const float Ssao55NoiseOffsetX = 0.5f, Ssao55NoiseOffsetY = 0.5f;

        // ---------------- decor55 (Game/Decor55.cs): the level accent's string course ----------------

        /// <summary>decor55: frames after a map is seen before the band is built (the map, its world and its boxes are up; Decor54's settle is 3).</summary>
        public const int Decor55SettleFrames = 5;
        /// <summary>decor55: the band's bottom, m over the floor beside the wall: a lintel-height course (doors are ~3 m), above the eye (1.6-1.7 m)
        /// and well above the waist-high cyan ride strip, so the two lines never read as one; low enough that an entry view at 4-15 m sees it.</summary>
        public const double Decor55BandY = 2.7;
        /// <summary>decor55: the band's height (m): ~25 px tall at 8 m at 1080p (the entry frames' walls are 4-15 m away), so the accent covers about
        /// 1 % of an entry frame with one wall in view (slate P0-decor's absolute floor).</summary>
        public const double Decor55BandH = 0.32;
        /// <summary>decor55: how far the band stands proud of the face (m): a slab, so its top and bottom edges catch the eye at a grazing angle.</summary>
        public const double Decor55BandOut = 0.04;
        /// <summary>decor55: a face is cut into segments of at most this (m), each with its own floor sample (a wall spanning two floor heights).</summary>
        public const double Decor55BandSegment = 4;
        /// <summary>decor55: faces shorter than this (m) get no band (thin posts, the ends of thin walls).</summary>
        public const double Decor55BandMinLen = 1.0;
        /// <summary>decor55: the floor probe stands this far out from the face (m), clear of the face's own collider.</summary>
        public const double Decor55BandProbe = 0.35;
        /// <summary>decor55: a floor up to this far (m) under the box's bottom still counts (a wall box standing on a raised floor's edge).</summary>
        public const double Decor55BandFloorBelow = 3;
        /// <summary>decor55: the band stays at least this far (m) under the box's top (the wall-top rim keeps its own line).</summary>
        public const double Decor55BandTopClear = 0.6;
        /// <summary>decor55: the band's luminance as a share of the usable cyan token's (0x19d3e6): usable cyan stays the brightest class on screen
        /// (n54-freeze a452 entry frames: usable cyan's 95th-percentile luminance 0.76-0.85).</summary>
        public const float Decor55BandLum = 0.62f;

        // ---------------- the a255 reel's SSAO part (a still camera in two rooms) ----------------

        /// <summary>a255 SSAO: frames to settle after a side's switch, then frames read (full resolution, the read region below).</summary>
        public const int Ssao55ReelSettle = 20, Ssao55ReelFrames = 60;
        /// <summary>a255 SSAO: a pixel "changed" when its luminance (0..1) moves more than this between two frames (slate P0-ssao: |dL| > 2 %).</summary>
        public const float Ssao55ReelDelta = 0.02f;
        /// <summary>a255 SSAO: the read region (share of the screen): the left middle (a154's), clear of the viewmodel, the reticle and the HUD.</summary>
        public const float Ssao55ReelU0 = 0.04f, Ssao55ReelU1 = 0.40f, Ssao55ReelV0 = 0.20f, Ssao55ReelV1 = 0.80f;
        /// <summary>a255 SSAO: the spot faces a wall between these distances (m) from a room's spawn (the AO lives in corners and at the wall's foot).</summary>
        public const double Ssao55ReelWallMin = 3, Ssao55ReelWallMax = 14;
        /// <summary>a255 SSAO: the camera looks this far down (rad) so the floor-wall corner (the AO) is in the read region.</summary>
        public const double Ssao55ReelPitch = -0.18;
        /// <summary>a255 SSAO: the slow yaw side's rate (rad per frame: 1 deg a second at 60 fps).</summary>
        public const double Ssao55ReelYawRate = 0.000291;
        /// <summary>a255 SSAO GREEN: ON's changed share at most this of OFF's; ON's mean luminance within this of OFF's; OFF over this x FLOOR's.</summary>
        public const double Ssao55ReelOnOverOff = 0.5, Ssao55ReelLumTol = 0.02, Ssao55ReelDefectOverFloor = 2;

        // ---------------- the a255 reel and `n55look step` (the same line scan in both) ----------------

        /// <summary>a255 / n55look: the scan's grid spacing (m) over a room's rect, inset by StepScanInset (start points of candidate lines).</summary>
        public const double Step55ScanGrid = 2, Step55ScanInset = 1;
        /// <summary>a255 / n55look: headings per start point.</summary>
        public const int Step55ScanHeadings = 16;
        /// <summary>a255 / n55look: a line's length at most (m) and its sample spacing (m).</summary>
        public const double Step55ScanRun = 16, Step55ScanStep = 0.25;
        /// <summary>a255 / n55look: a step is a floor change in [Min, Max] m between two flat samples (normal y >= Move54ReelFlat's 0.97 value,
        /// written again here so the harness needs no Move54 tuning file).</summary>
        public const double Step55ScanStepMin = 0.1, Step55ScanStepMax = 0.45, Step55ScanFlat = 0.97;
        /// <summary>a255 / n55look: a change over this between two samples is a ledge or a hole (the line ends there).</summary>
        public const double Step55ScanLedge = 0.6;
        /// <summary>a255 / n55look: the step at least From m along the line (up to run speed) and Before m before its end.</summary>
        public const double Step55ScanFrom = 3, Step55ScanBefore = 2.5;
        /// <summary>a255: step lines tried in rank order until the Mover crosses one as a grounded step (n52-kiln kb2c: of its 0.40 m lips only some
        /// are stepped up grounded; the others are blocked or jumped).</summary>
        public const int Step55ReelAttempts = 8;
        /// <summary>a255: steps (120 Hz) a run leg lasts at most.</summary>
        public const int Step55RunSteps = 360;
        /// <summary>a255 / n55look: frames within this many seconds of a held step count as the step's frames; flat-ground frames are at least
        /// FlatAway s from any step (the noise floor is read there).</summary>
        public const double Step55StepWindowS = 0.35, Step55FlatAwayS = 0.6;
        /// <summary>a255 GREEN: the row on's largest one-frame camera drop at the step at most this share of the row off's (slate P0-step).</summary>
        public const double Step55OnOverOff = 0.5;
        /// <summary>a255: the row off's drop must exceed this many times the flat-ground noise for a defect to count (else "no defect measured").</summary>
        public const double Step55DefectOverNoise = 2;
        /// <summary>a255 GREEN (one run, row on): the camera's largest one-frame change at the step at most this share of the feet's own (the drawn
        /// feet, NIGHT53's camera). `n55look step` on n52-kiln kb2c: move54 x0.40 of the feet at 60 fps (0.090 / 0.225 m), step55 x0.16 (0.035);
        /// 0.5 x move54's x0.40 = x0.20. The lead's off run (-skygrindOff55) prints move54's own share beside it.</summary>
        public const double Step55ReelOnFeetShare = 0.2;    }
}
