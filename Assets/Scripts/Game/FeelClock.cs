// NIGHT18 (NIGHT18.md section 1.2, the lead's): the reel's frame clock. While a reel scenario records, every rendered frame
// is exactly 1 / UnityTuning.ReelFps of game time, however long it takes in wall time (the JPG writes), so the 120 Hz sim
// steps 2 per frame and every view that takes frameDt from the loop follows. Unity's docs: "captureDeltaTime does not have
// any affect on Time.unscaledTime", and SurvivalGame's loop reads Time.unscaledDeltaTime, so Time.captureDeltaTime alone
// can't pace it; it's set too, for Unity-driven systems.
// The direct wall-time readers route through here (FrameDelta / Now / RealNow): the loop's frame delta, FlashGate.RealNow,
// UiPress' press age, the quit-confirm window (UiMenus), the wheel's select clock (CombatReader). Outside a reel each is
// the wall clock it was. While Active, AUTO quality holds its preset (Quality.cs) and the attract never starts (Kiosk).
// NIGHT63 builder D (D1, same-run SPOT pairs): THE HOLD. The reel's same-run toggle (FeelReel.N63Same.cs) renders one extra frame per shot with
// a row OFF; BeginHold at the end of the shot's frame, EndHold at the end of the held frame. While holding: FrameDelta is 0 (the loop steps the
// sim 0 times and keeps its interpolation alpha: every view gets frameDt 0), Now / RealNow / Frames read the shot frame's, Time.timeScale is 0
// (Time.deltaTime, Time.time, the shaders' _Time, scaled particles and trails stand still), and the views' own wall-clock readers go through
// UnscaledDelta / UnscaledTime / UnscaledNow / FrameCount below (0 delta, the shot frame's time and frame). After EndHold every clock runs on
// with the held span skipped (SameRun63Math.HoldClock), so nothing that ages on them sees the extra frame. With no hold ever begun each shim
// returns Unity's own value, bit for bit (no -skygrindSameRun63: the game and every reel exactly as before).
using UnityEngine;

namespace Skygrind.Game
{
    public static class FeelClock
    {
        /// <summary>A reel scenario is recording: frames are fixed 1 / ReelFps steps of game time.</summary>
        public static bool Active { get; private set; }

        /// <summary>The fixed frame time while Active (s).</summary>
        public static double Fixed => 1.0 / UnityTuning.ReelFps;

        static int startFrame;
        static double startUnscaled, startReal;

        // NIGHT63 D1: the hold's bookkeeping and what it saved
        static readonly SameRun63Math.HoldClock hold = new SameRun63Math.HoldClock();
        static bool everHeld;
        static float timeScaleSaved = 1f;
        static double holdReal, heldReal;

        /// <summary>Start the fixed clock (FeelStrip, before a scenario's pre-roll).</summary>
        public static void Begin()
        {
            if (Active) return;
            Active = true;
            startFrame = FrameCount;
            startUnscaled = UnscaledNow;
            startReal = Time.realtimeSinceStartupAsDouble - heldReal;
            Time.captureDeltaTime = (float)Fixed;
        }

        /// <summary>Back to the wall clock (after a scenario). A hold still open ends here (a scenario stopped inside one).</summary>
        public static void End()
        {
            if (Holding) EndHold();
            if (!Active) return;
            Active = false;
            Time.captureDeltaTime = 0;
        }

        /// <summary>The loop's frame delta (s): 0 while holding, 1 / ReelFps while Active, else Time.unscaledDeltaTime.</summary>
        public static double FrameDelta => hold.Holding ? 0 : Active ? Fixed : Time.unscaledDeltaTime;

        /// <summary>Frames rendered since Begin (0 outside a reel); held frames are not counted.</summary>
        public static int Frames => Active ? FrameCount - startFrame : 0;

        /// <summary>Time.unscaledTimeAsDouble, or the fixed clock's time while Active (a held frame reads the shot frame's).</summary>
        public static double Now => Active ? startUnscaled + (FrameCount - startFrame) * Fixed : UnscaledNow;

        /// <summary>Time.realtimeSinceStartupAsDouble, or the fixed clock's time while Active (a held frame reads the shot frame's).</summary>
        public static double RealNow => Active ? startReal + (FrameCount - startFrame) * Fixed : !everHeld ? Time.realtimeSinceStartupAsDouble : hold.Holding ? holdReal - heldReal : Time.realtimeSinceStartupAsDouble - heldReal;

        // --- NIGHT63 D1: the hold and the views' wall clocks ---------------------------------------------------------------------

        /// <summary>A same-run hold is on (the frame being drawn is a shot's OFF frame: nothing may move but the row).</summary>
        public static bool Holding => hold.Holding;

        /// <summary>Frames held so far this process (the reel's proof line).</summary>
        public static int HeldFrames => hold.HeldFrames;

        /// <summary>Begin a hold at the END of the frame whose shot was written (FeelReel.SameRun63): the next frame steps nothing and draws the
        /// same instant. Time.timeScale 0 for its length (the game itself never writes timeScale; restored by EndHold).</summary>
        public static void BeginHold()
        {
            if (hold.Holding) return;
            everHeld = true;
            hold.Begin(Time.frameCount, Time.unscaledTimeAsDouble);
            holdReal = Time.realtimeSinceStartupAsDouble;
            timeScaleSaved = Time.timeScale;
            Time.timeScale = 0f;
        }

        /// <summary>End the hold at the END of the held frame (its off capture written): the next frame runs on from the shot frame.</summary>
        public static void EndHold()
        {
            if (!hold.Holding) return;
            hold.End(Time.frameCount, Time.unscaledTimeAsDouble);
            heldReal += Time.realtimeSinceStartupAsDouble - holdReal;
            Time.timeScale = timeScaleSaved;
        }

        /// <summary>Time.unscaledDeltaTime for a view's own wall-clock fade or decay: 0 while holding.</summary>
        public static float UnscaledDelta => hold.Holding ? 0f : Time.unscaledDeltaTime;

        /// <summary>Time.unscaledTimeAsDouble for a view's own wall clock: the shot frame's while holding, the held span skipped after.</summary>
        public static double UnscaledNow => everHeld ? hold.Time(Time.unscaledTimeAsDouble) : Time.unscaledTimeAsDouble;

        /// <summary>Time.unscaledTime (float) for a view's own wall clock: the shot frame's while holding, the held span skipped after.</summary>
        public static float UnscaledTime => everHeld ? (float)hold.Time(Time.unscaledTimeAsDouble) : Time.unscaledTime;

        /// <summary>Time.frameCount for a view's frame-counted age or per-frame noise: the shot frame's while holding, held frames skipped after.
        /// (A "once this frame" guard keeps Time.frameCount: the held frame is a frame of its own.)</summary>
        public static int FrameCount => everHeld ? hold.Frame(Time.frameCount) : Time.frameCount;
    }
}
