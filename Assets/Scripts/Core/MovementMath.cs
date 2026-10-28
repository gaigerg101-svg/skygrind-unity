// Pure movement math, ported from skygrind/src/movement/movementMath.js (SPEC.md §3.2). No UnityEngine, no scene
// access. Doubles throughout, in the same operation order as the JS, so results match the web game's numbers.
// The JS writes into an optional `out` (which may be the input); here vectors are structs returned by value, which
// gives the same guarantees (inputs never mutated, nothing allocated) with `v = Accelerate(v, ...)` for in place.
using System;

namespace Skygrind.Core
{
    /// <summary>Movement states (SPEC §3.4).</summary>
    public enum MoveState
    {
        Ground,
        Air,
        Slide,
        Dash,
        WallRide,
        Slam,
        Swing, // hanging on the ARCLINE rope (its own gravity, SwingMath)
    }

    /// <summary>What a jump press becomes. None keeps the press buffered (the JS null).</summary>
    public enum JumpKind
    {
        None,
        Boost,
        Dash,
        Slide,
        Ground,
        Wall,
        Double,
    }

    public struct GravityFlags
    {
        public bool jumpHeld;
        public bool jumpAirtime;
        public bool windActive;
    }

    public struct VerticalStepResult
    {
        public double vy; // end speed (m/s)
        public double dy; // displacement (m)
    }

    public struct JumpRelease
    {
        public double vy;
        public bool cutArmed;
    }

    public struct JumpContext
    {
        public MoveState state;
        public bool grounded;
        public int coyoteSteps;
        public int doubleJumpsLeft;
        public bool landingSoon;
        public bool wallNear;
        public int wallJumpsLeft;
        public int boostSteps;
        public double pips;
        public double dashJumpCost;
    }

    public static class MovementMath
    {
        /// <summary>
        /// Quake III PM_Accelerate. Adds speed along wishDir, but only until the velocity's component along wishDir
        /// reaches wishSpeed. Speed perpendicular to wishDir is untouched, which is why air strafing can raise speed.
        /// </summary>
        public static Vec3 Accelerate(Vec3 vel, Vec3 wishDir, double wishSpeed, double accel, double dt)
        {
            double currentSpeed = vel.x * wishDir.x + vel.y * wishDir.y + vel.z * wishDir.z;
            double addSpeed = wishSpeed - currentSpeed;
            double accelSpeed = 0;
            if (addSpeed > 0) accelSpeed = JsMath.Min(accel * wishSpeed * dt, addSpeed);
            return new Vec3(vel.x + accelSpeed * wishDir.x, vel.y + accelSpeed * wishDir.y, vel.z + accelSpeed * wishDir.z);
        }

        /// <summary>
        /// Quake III PM_Friction on horizontal speed only. Below stopSpeed the drop is computed as if moving at
        /// stopSpeed, so the player stops in finite time. Clamped at zero, so it never reverses direction.
        /// </summary>
        public static Vec3 ApplyFriction(Vec3 vel, double friction, double stopSpeed, double dt)
        {
            double speed = JsMath.Hypot(vel.x, vel.z);
            double scale = 0;
            if (speed > 0)
            {
                double control = JsMath.Max(speed, stopSpeed);
                double newSpeed = JsMath.Max(speed - control * friction * dt, 0);
                scale = newSpeed / speed;
            }
            return new Vec3(vel.x * scale, vel.y, vel.z * scale);
        }

        /// <summary>Launch speed that reaches height h at time tApex under constant gravity (Pittman: v0 = 2h/t).</summary>
        public static double JumpVelocity(double h, double tApex) => (2 * h) / tApex;

        /// <summary>Gravity that makes a v0 = 2h/t jump peak at height h at time tApex (Pittman: g = 2h/t²).</summary>
        public static double GravityFor(double h, double tApex) => (2 * h) / (tApex * tApex);

        /// <summary>
        /// A duration in whole fixed steps (at least 1). Timers count steps, so float drift can never add a step
        /// (0.2 s at 120 Hz is exactly 24 steps, not 25). Rounds like JS Math.round.
        /// </summary>
        public static int StepsFor(double seconds, double dt) => (int)JsMath.Max(1, JsMath.Round(seconds / dt));

        /// <summary>
        /// One fixed step of vertical motion under constant gravity, using the average of start and end velocity
        /// (exact for constant acceleration). gravity is downward and already scaled.
        /// </summary>
        public static VerticalStepResult VerticalStep(double vy, double gravity, double dt)
        {
            double next = vy - gravity * dt;
            return new VerticalStepResult { dy = ((vy + next) / 2) * dt, vy = next };
        }

        /// <summary>
        /// Gravity multiplier (SPEC §3.6). Exactly one applies. Priority: DASH/SLAM/SWING 0; WALLRIDE; in AIR the
        /// dash-jump wind, else apex hang (Space held, in a jump airtime, |vy| small), else fall past the apex, else 1.
        /// </summary>
        public static double GravityScale(MoveState state, double vy, GravityFlags flags, MovementConfig config)
        {
            switch (state)
            {
                case MoveState.Dash:
                case MoveState.Slam:
                case MoveState.Swing: // the swing integrates its own gravity (×0.85) in its sub-steps
                    return 0;
                case MoveState.WallRide:
                    return config.wallRide.gravityMult;
                case MoveState.Air:
                    if (flags.windActive) return config.dashJump.gravityMult;
                    if (flags.jumpHeld && flags.jumpAirtime && Math.Abs(vy) < config.jump.apexHangSpeed) return config.jump.apexHangMult;
                    if (vy <= 0) return config.jump.fallGravityMult;
                    return 1;
                default:
                    return 1;
            }
        }

        /// <summary>Dash speed: never slower than the dash itself, never slower than you already were (SPEC 1.2).</summary>
        public static double DashSpeed(double entrySpeed, double dashSpeedValue) => JsMath.Max(dashSpeedValue, entrySpeed);

        /// <summary>Horizontal speed when a dash ends without a jump (SPEC 1.2).</summary>
        public static double DashExitSpeed(double walkSpeed, double speedBeforeDash) => JsMath.Max(walkSpeed, speedBeforeDash);

        /// <summary>Slide jump horizontal speed (SPEC 1.2): boosted only below the cap, never past it, never reduced.</summary>
        public static double SlideJumpSpeed(double speed, double mult, double cap)
        {
            if (speed >= cap) return speed;
            return JsMath.Max(speed, JsMath.Min(speed * mult, cap));
        }

        /// <summary>Stamina regen: continuous, so pips fill one at a time; none while dashing; capped.</summary>
        public static double RegenPips(double pips, double ratePerSec, double maxPips, double dt, bool dashing)
        {
            if (dashing) return pips;
            return JsMath.Min(maxPips, pips + ratePerSec * dt);
        }

        /// <summary>
        /// Variable jump. A jump arms one cut; releasing Space while still rising multiplies vertical speed once.
        /// The cut is disarmed after use or once the jump passes its apex.
        /// </summary>
        public static JumpRelease ReleaseJump(double vy, bool held, double releaseMult, bool cutArmed)
        {
            var r = new JumpRelease { vy = vy, cutArmed = cutArmed && vy > 0 };
            if (r.cutArmed && !held)
            {
                r.vy = vy * releaseMult;
                r.cutArmed = false;
            }
            return r;
        }

        /// <summary>
        /// Which jump a (possibly buffered) press becomes this step. Priority: boost window, dash jump, slide,
        /// ground, coyote, wall, (keep buffered if a landing is predicted), double. None keeps the press buffered.
        /// </summary>
        public static JumpKind ResolveJump(in JumpContext ctx)
        {
            MoveState state = ctx.state;
            if (state == MoveState.Slam) return JumpKind.None; // buffered: it becomes the boost jump on landing
            if (ctx.boostSteps > 0 && (state == MoveState.Ground || state == MoveState.Slide)) return JumpKind.Boost;
            if (state == MoveState.Dash)
            {
                if (ctx.pips >= ctx.dashJumpCost) return JumpKind.Dash;
                // 0 pips: a normal jump from what is under the dash (the mover ends the dash first).
                return ctx.grounded ? JumpKind.Ground : AirJump(ctx);
            }
            if (state == MoveState.Slide) return JumpKind.Slide;
            if (state == MoveState.Ground) return JumpKind.Ground;
            if (state == MoveState.Air && ctx.coyoteSteps > 0) return JumpKind.Ground;
            return AirJump(ctx);
        }

        static JumpKind AirJump(in JumpContext ctx)
        {
            if ((ctx.state == MoveState.WallRide || ctx.wallNear) && ctx.wallJumpsLeft > 0) return JumpKind.Wall;
            if (ctx.landingSoon) return JumpKind.None;
            if (ctx.doubleJumpsLeft > 0) return JumpKind.Double;
            return JumpKind.None;
        }

        /// <summary>Horizontal speed along a wall: the velocity minus its part along the (horizontal) wall normal.</summary>
        public static double AlongWallSpeed(Vec3 vel, Vec3 normal)
        {
            double into = vel.x * normal.x + vel.z * normal.z;
            return JsMath.Hypot(vel.x - into * normal.x, vel.z - into * normal.z);
        }

        /// <summary>Wall jump (SPEC §3.3): keep the speed along the wall, kick out along the normal, set vertical.</summary>
        public static Vec3 WallJumpVelocity(Vec3 vel, Vec3 normal, double outSpeed, double upSpeed)
        {
            double into = vel.x * normal.x + vel.z * normal.z;
            double alongX = vel.x - into * normal.x;
            double alongZ = vel.z - into * normal.z;
            return new Vec3(alongX + normal.x * outSpeed, upSpeed, alongZ + normal.z * outSpeed);
        }

        /// <summary>
        /// Dash direction out of a wall ride (SPEC 1.3): input reflected off the wall if it points into it; with no
        /// input, 45° away from the wall in the facing direction. Unit, horizontal, never into the wall.
        /// </summary>
        public static Vec3 WallRideDashDir(Vec3 inputDir, bool hasInput, Vec3 forward, Vec3 normal)
        {
            double x;
            double z;
            if (hasInput)
            {
                double into = JsMath.Min(0, inputDir.x * normal.x + inputDir.z * normal.z);
                x = inputDir.x - 2 * into * normal.x;
                z = inputDir.z - 2 * into * normal.z;
            }
            else
            {
                double f = forward.x * normal.x + forward.z * normal.z;
                double alongX = forward.x - f * normal.x;
                double alongZ = forward.z - f * normal.z;
                double alongLen = JsMath.Hypot(alongX, alongZ);
                x = (alongLen > 0 ? alongX / alongLen : 0) + normal.x;
                z = (alongLen > 0 ? alongZ / alongLen : 0) + normal.z;
            }
            double len = JsMath.Hypot(x, z);
            return new Vec3(x / len, 0, z / len);
        }

        /// <summary>Pad launch speed that peaks at apexHeight under constant gravity: v = √(2gh).</summary>
        public static double PadLaunchSpeed(double apexHeight, double gravity) => Math.Sqrt(2 * gravity * apexHeight);

        /// <summary>Distance fallen over t seconds from vertical speed vy (negative = falling). Never negative.</summary>
        public static double FallDistance(double vy, double gravity, double t) => JsMath.Max(0, -vy * t + 0.5 * gravity * t * t);

        /// <summary>Horizontal unit wish direction from move input and yaw (yaw 0 looks toward −z). Zero with no input.</summary>
        public static Vec3 WishDirection(double moveX, double moveZ, double yaw)
        {
            double sin = JsMath.Sin(yaw);
            double cos = JsMath.Cos(yaw);
            // forward = (−sin, 0, −cos), right = (cos, 0, −sin)
            double x = -sin * moveZ + cos * moveX;
            double z = -cos * moveZ - sin * moveX;
            double len = JsMath.Hypot(x, z);
            return new Vec3(len > 0 ? x / len : 0, 0, len > 0 ? z / len : 0);
        }

        /// <summary>Horizontal speed (m/s).</summary>
        public static double HorizontalSpeed(Vec3 v) => JsMath.Hypot(v.x, v.z);

        /// <summary>Scales horizontal velocity down to max, leaving vertical speed alone.</summary>
        public static Vec3 ClampHorizontal(Vec3 vel, double max)
        {
            double speed = JsMath.Hypot(vel.x, vel.z);
            double scale = speed > max ? max / speed : 1;
            return new Vec3(vel.x * scale, vel.y, vel.z * scale);
        }

        /// <summary>
        /// Aimed jump pad (SPEC 2.0 §5.2): the launch velocity that peaks apexAbove over the higher of start and
        /// target and lands on the target, with split gravity (base on the way up, × fallMult past the apex).
        /// </summary>
        public static Vec3 PadLaunchVelocity(Vec3 from, Vec3 to, double apexAbove, double gravity, double fallMult)
        {
            double peak = JsMath.Max(from.y, to.y) + apexAbove;
            double vy = Math.Sqrt(2 * gravity * (peak - from.y));
            double time = vy / gravity + Math.Sqrt((2 * (peak - to.y)) / (gravity * fallMult));
            return new Vec3((to.x - from.x) / time, vy, (to.z - from.z) / time);
        }

        /// <summary>
        /// Slope boost (SPEC 2.0 §3.3): the horizontal acceleration a slide gets from gravity along the ground,
        /// g · cosθ · (downhill tilt · slide direction). Positive downhill, negative uphill, 0 on flat ground.
        /// </summary>
        public static double SlopeSlideAccel(Vec3 normal, Vec3 dir, double gravity, double mult) =>
            gravity * mult * normal.y * (normal.x * dir.x + normal.z * dir.z);
    }
}
