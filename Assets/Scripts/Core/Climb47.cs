// NIGHT47 builder W (climb47, sim row; NIGHT47.md G1 / G2, section 3 "The climb"; Research/2026-10-18-wave47/W.md, W-cards.md section 2;
// DECIDE47 D-2: DECIDE46 N-5 lifted for this row only). THE CLIMB'S FORGIVENESS, on CLIMB WALLS only (a face the owner names through
// Mover.climbWall: THE STACK's ride panels in the WICK's fight). Measured cause (W.md): a jump-off gains 2.16 m with the look up at the release
// (1.15 m level), the panels rise 2.80 m, so each attach lands 0.56 m lower on its panel until the capsule misses the face (GG: "slipping
// further down each time"). The rules (each number in design.n47.climb47.numbers with its why):
//   SET UP      a jump-off or a wall jump off a climb wall SETS the up speed to at least setUp (never adds to a falling one: Celeste's WallJump
//               assigns Speed.Y, W.md Verified); the look pitch still steers above it.
//   THE CATCH   a ride that starts on a climb wall with the feet under the panel's bottom + catchLine (by at most catchBelow) is lifted to that
//               line over catchEase s (eased), and one up to catchAbove over it settles down to it: every attach starts at the same height on
//               its panel, so every jump gains the same.
//   COYOTE      a ride on a climb wall that ends on its own leaves the jump-off open for coyote s (wallride35's late window, lengthened).
//   BUFFER      a jump released up to buffer s before a ride starts on a climb wall is that ride's jump-off (on its first step).
//   REACH       the near-miss draw onto a climb wall reaches catchReach past the capsule, and ride40's stick-intent rule is waived on a climb
//               wall (a look up to ~15 deg off still attaches).
//   SPEED       a ride on a climb wall runs at least retrySpeed along it (a retry from a catch ledge starts at full climb speed).
// Off (Params.Off, the row off, --off47, or no climbWall): every branch in Mover.cs marked "NIGHT47 climb47" is skipped: NIGHT46 bit for bit.
// Whole steps (MovementMath.StepsFor), no allocation, no randomness, no UnityEngine.
using System;

namespace Skygrind.Core
{
    public static class Climb47
    {
        public struct Params
        {
            public bool on;
            public double setUp, catchBelow, catchAbove, catchLine, catchReach, retrySpeed, guideOver, guideMin, guideMax, faceAbove;
            public int catchEaseSteps, coyoteSteps, bufferSteps;

            public static Params Off => default;

            public static Params From(GameConfig c, bool on)
            {
                if (!on) return Off;
                const string g = "design.n47.climb47.numbers.";
                double dt = c.Dt;
                return new Params
                {
                    on = true,
                    setUp = c.Num(g + "setUp"),
                    catchBelow = c.Num(g + "catchBelow"),
                    catchAbove = c.Num(g + "catchAbove"),
                    catchLine = c.Num(g + "catchLine"),
                    catchReach = c.Num(g + "catchReach"),
                    retrySpeed = c.Num(g + "retrySpeed"),
                    guideOver = c.Num(g + "guideOver"), guideMin = c.Num(g + "guideMin"), guideMax = c.Num(g + "guideMax"), faceAbove = c.Num(g + "faceAbove"),
                    catchEaseSteps = Math.Max(1, MovementMath.StepsFor(c.Num(g + "catchEase"), dt)),
                    coyoteSteps = MovementMath.StepsFor(c.Num(g + "coyote"), dt),
                    bufferSteps = MovementMath.StepsFor(c.Num(g + "buffer"), dt),
                };
            }
        }

        /// <summary>The Mover's climb47 state (Mover.climb47, saved whole by RunSnapshot.MoverState): the rules, the catch's ease, the clock since
        /// the jump was let go in the air, the buffered jump-off, the counts the harness reads.</summary>
        public struct State
        {
            public Params p;
            public int catchLeft, sinceRelease, pendingJumpOff;
            public double catchTarget;
            public bool wasHeld, onClimb; // onClimb: the ride now (or the last) is on a climb wall (the coyote reads it: the ride may end past the panel's end)
            public int catches, setUps, buffered, coyoteJumps, reachDraws, speedFloors, guided;
        }
    }
}
