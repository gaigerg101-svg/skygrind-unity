// NIGHT55 builder C (camera and look; view rows step55, ssao55, decor55, fov55): SurvivalGame's hooks. Read only: the sim, the feet, the Mover and
// every ray are untouched. Each hook is one line in an existing file:
//   SurvivalGame.Move54.cs Move54StepEnded:  Step55StepEnded();               after each 120 Hz sim step (the step's feet, before and after)
//   SurvivalGame.Move54.cs Move54Drawn:      if (Step55Drawn(...)) return ...; the camera's height (step55 replaces move54's step offset when on)
// Row off (design.n55.step55 off, or -skygrindOff55): no Move55Step is built and both hooks are no-ops (NIGHT54's camera: move54's Move54Step).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        Move55Step move55Step;
        bool move55Read;

        /// <summary>The step55 smoother (null with the row off), built on first use from the game's config.</summary>
        public Move55Step Step55Steps
        {
            get
            {
                if (!move55Read && cfg != null)
                {
                    move55Read = true;
                    if (Move55.On(cfg)) move55Step = new Move55Step(step);
                }
                return move55Step;
            }
        }

        /// <summary>After each sim step (from Move54StepEnded): the step's end state (on the ground or sliding) and the feet's heights.</summary>
        void Step55StepEnded()
        {
            Move55Step s = Step55Steps;
            if (s == null || mover == null) return;
            s.StepEnded(mover.grounded && (mover.state == MoveState.Ground || mover.state == MoveState.Slide), previous.y, current.y);
        }

        /// <summary>The camera's feet with step55 on (from Move54Drawn): `drawn` plus the held step's offset at the frame's alpha; false with the
        /// row off (move54's own path runs).</summary>
        bool Step55Drawn(ref Vec3 drawn, bool simulating)
        {
            Move55Step s = Step55Steps;
            if (s == null) return false;
            if (!(simulating && flow == Flow.Playing && !attractOn)) { s.Reset(); return true; }
            double o = s.Draw(Alpha);
            if (o != 0) drawn = new Vec3(drawn.x, drawn.y + o, drawn.z);
            return true;
        }
    }
}
