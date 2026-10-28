// NIGHT42 builder M1 (design.n42.wallfling42, N42Blocks bit 0; GG 2026-10-14 ~18:35, G2: "pressing a near a wall you fly bcak so fast and it
// create very awkward movement"; DECIDE42 D-4; CODE-FACTS G2): a jump near a wall no longer flings the player back.
// Today (CODE-FACTS G2): MovementMath.AirJump makes ANY air press a wall jump whenever a wall is touched or probed near (Mover.FindNearWall),
// before the double jump and before a predicted landing; the wall jump is +8 m/s out with 0.9 x the jump's up speed; the ride's jump-off is
// wallride35's 8 m/s out, scaled up by its exit boost (the whole horizontal speed to chainCap 16).
// With the row on (Mover.wall42, set by the sim at a run's start: SurvivalSim.N42.Move.cs), only inside the Mover's wall-jump / jump-off
// branches (Mover.cs is named for this tonight, NIGHT42.md method 5):
// - intent: an air press near a wall (not riding it) is a wall jump only when the stick points into the wall (-wish . n >= intentDot);
//   otherwise it is the double jump (flow34's wallBuffer does not hold it for a wall either). Riding, or the wall coyote just after a ride
//   (flow34 wallCoyote), is intent already.
// - landing: no wall jump when a floor landing is predicted within landSeconds (falling): the press stays buffered and becomes the ground
//   jump on the landing (the web's landingSoon rule, now ahead of the wall).
// - the push: outSpeed out (was wallJump.outSpeed 8) and v0Mult x the jump's up speed (was wallJump.v0Mult 0.9): a smaller back push, more lift.
// - the slide bounce (flow34 slideBounce34): only when the slide heads at least bounceDot into the wall (was any slide 3 deg into it).
// - the ride's jump-off: its pitch rule (Ride35.JumpUp) and along speed kept; from a ride younger than rideShortSeconds (a jump that caught a
//   wall for a moment: the fling) its speed out from the wall capped at rideOutMax. A longer ride's jump-off is the chain's and keeps its push
//   (a cap on every jump-off took THE GANTRY's chain margins from +32 / +26 % to +10 / +10 % at 7 m/s and to RED at 5: n42move gates).
// The row off (or --off42 / -skygrindOff42, or a tape of format 24 or older): Params.Neutral, every branch skipped: NIGHT41's Mover bit for bit.
namespace Skygrind.Core
{
    public static class Wall42
    {
        public struct Params
        {
            public bool on;
            public double intentDot, outSpeed, v0Mult, landSeconds, rideOutMax, bounceDot;
            /// <summary>rideShortSeconds in sim steps: a jump-off from a ride younger than this is capped (rideOutMax).</summary>
            public int rideShortSteps;

            /// <summary>Every rule off: the Mover is NIGHT41's.</summary>
            public static Params Neutral => default;

            /// <summary>The row's numbers (design.n42.wallfling42) when `on`; Neutral when off.</summary>
            public static Params From(GameConfig cfg, bool on)
            {
                if (!on) return Neutral;
                const string r = "design.n42.wallfling42.";
                return new Params
                {
                    on = true,
                    intentDot = cfg.Num(r + "intentDot"),
                    outSpeed = cfg.Num(r + "outSpeed"),
                    v0Mult = cfg.Num(r + "v0Mult"),
                    landSeconds = cfg.Num(r + "landSeconds"),
                    rideOutMax = cfg.Num(r + "rideOutMax"),
                    bounceDot = cfg.Num(r + "bounceDot"),
                    rideShortSteps = MovementMath.StepsFor(cfg.Num(r + "rideShortSeconds"), 1 / cfg.movement.loop.simHz),
                };
            }
        }

        /// <summary>Does the stick mean the wall with normal n? (wish: the unit wish direction, wishSpeed > 0 when the stick is off centre.)</summary>
        public static bool Intent(Vec3 wish, double wishSpeed, Vec3 n, double intentDot) =>
            wishSpeed > 0 && -(wish.x * n.x + wish.z * n.z) >= intentDot;

        /// <summary>The horizontal velocity's part out from the wall (along n) held to `max`; the along-wall part and the vertical kept.</summary>
        public static Vec3 CapOut(Vec3 v, Vec3 n, double max)
        {
            double outv = v.x * n.x + v.z * n.z;
            if (outv <= max) return v;
            double cut = outv - max;
            return new Vec3(v.x - n.x * cut, v.y, v.z - n.z * cut);
        }
    }
}
