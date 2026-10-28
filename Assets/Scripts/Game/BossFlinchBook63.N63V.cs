// NIGHT63 builder V (B1d; Logs/n63/verdict-B1.md: THE CROWN's b8 boss63 reel RED on one "drawn off" flinch, 8983 ms): the boss63 reel's flinch
// book, pure (no Unity object; N63B1dTests). Each fight frame the reel hands it the layer's draw it took this frame (`drew`; dated `drewAgo`
// frames back: THE CROWN draws in LateUpdate, after the reel's coroutine, so its draw of a frame is taken on the next one) with that draw's
// flinch level, then whether the boss's health fell this frame. The pending damage event is judged on this frame's take FIRST, then a new one
// replaces it: b8's crown took two damage events on consecutive frames (8983 and 9000 ms), and the old order counted the first "drawn off"
// before its draw (taken a frame late) was looked at. Not a held frame: no shot was near 8983 ms on b8 (D1's held frames are skipped by the reel
// before they reach the book).
// A damage event is DRAWN when a draw dated at or after its frame shows a flinch over 0.1 (late when that is more than 2 frames after it), OFF
// when 6 frames pass without one, when a new event replaces it before any draw of it was taken, or when the fight ends with it pending.
using System;

namespace Skygrind.Game
{
    public sealed class FlinchBook63
    {
        /// <summary>The flinch level a draw must show to count (BossMotion63Probe.Flinch).</summary>
        public const float DrawnLevel = 0.1f;
        /// <summary>Frames a damage event may wait for its drawn flinch before it is counted off.</summary>
        public const int WaitFrames = 6;
        /// <summary>A drawn flinch more than this many frames after its damage event is late (B1's doneWhen: within 2 frames).</summary>
        public const int LateFrames = 2;

        /// <summary>Damage events, drawn, drawn late, never drawn.</summary>
        public int Damage, Drawn, Late, Off;

        int pendingFrame = -1;
        double pendingMs, pendingA, pendingB;

        /// <summary>A damage event waits for its drawn flinch.</summary>
        public bool Pending => pendingFrame >= 0;

        /// <summary>One fight frame (`frame`: the reel's count of non-held frames): the draw taken this frame (`drew`, dated `drewAgo` frames back,
        /// its flinch level `flinch`), then a damage event if the health fell (`damaged`: at `ms`, hp `hpBefore` -> `hpAfter`). `say` prints the
        /// "[boss63] flinch" line (BossAttackClock63.FlinchLine) of each event judged.</summary>
        public void Frame(int frame, bool drew, int drewAgo, double flinch, bool damaged, double ms, double hpBefore, double hpAfter, Action<string> say)
        {
            Judge(frame, drew, drewAgo, flinch, say); // the pending event on this take first (THE CROWN's draw of the last frame)
            if (damaged)
            {
                if (pendingFrame >= 0) Miss(say); // no draw of it was taken before a new one came
                Damage++;
                pendingFrame = frame;
                pendingMs = ms;
                pendingA = hpBefore;
                pendingB = hpAfter;
                Judge(frame, drew, drewAgo, flinch, say); // a view drawn after the sim's step already shows it
            }
            if (pendingFrame >= 0 && frame - pendingFrame > WaitFrames) Miss(say);
        }

        /// <summary>The fight is over: a damage event still pending was never drawn.</summary>
        public void End(Action<string> say)
        {
            if (pendingFrame >= 0) Miss(say);
        }

        void Judge(int frame, bool drew, int drewAgo, double flinch, Action<string> say)
        {
            if (pendingFrame < 0 || !drew || flinch <= DrawnLevel) return;
            int since = frame - drewAgo - pendingFrame;
            if (since < 0) return; // a draw from before the event
            Drawn++;
            if (since > LateFrames) Late++;
            say?.Invoke(BossAttackClock63.FlinchLine(pendingMs, pendingA, pendingB, since));
            pendingFrame = -1;
        }

        void Miss(Action<string> say)
        {
            Off++;
            say?.Invoke(BossAttackClock63.FlinchLine(pendingMs, pendingA, pendingB, -1));
            pendingFrame = -1;
        }
    }
}
