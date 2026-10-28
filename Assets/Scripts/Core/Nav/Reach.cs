// Port of skygrind/src/nav/reach.js (NIGHT14 U3-27.4), line for line with its comments:
// Jump reach (SPEC 2.0 §6 nav): how far each movement kind carries you horizontally when landing
// `dy` higher (or lower), simulated with the Mover's real rules (120 Hz, exact vertical steps, the
// fall multiplier past the apex, the dash-jump wind window) but without apex hang, so the answer
// is conservative. Pure: no three.js, unit tested.
using System;

namespace Skygrind.Core
{
    public static class Reach
    {
        /// <summary>The kinds the nav graph links nodes with (a pad is data, not a jump).</summary>
        public static readonly string[] JUMP_KINDS = { "jump", "double", "dash", "dashDouble" };

        /// <summary>
        /// Horizontal reach (m) of one jump kind to land `dy` m higher (negative = lower), or 0 if it can't
        /// get that high. kind: 'jump' | 'double' | 'dash' | 'dashDouble'; dy in m.
        /// </summary>
        public static double JumpReach(string kind, double dy, MovementConfig c)
        {
            double dt = 1 / c.loop.simHz;
            double g = MovementMath.GravityFor(c.jump.height, c.jump.apexTime);
            double v0 = MovementMath.JumpVelocity(c.jump.height, c.jump.apexTime);
            bool dash = kind == "dash" || kind == "dashDouble";
            double speed = dash ? MovementMath.DashSpeed(c.ground.walkSpeed, c.dash.speed) : c.ground.walkSpeed;
            double vy = dash ? v0 * c.dashJump.v0Mult : v0;
            int wind = dash ? MovementMath.StepsFor(c.dashJump.windowTime, dt) : 0;
            int doubleLeft = kind == "double" || kind == "dashDouble" ? 1 : 0;
            double y = 0;
            double t = 0;
            double peak = 0;
            for (int i = 0; i < 2000; i++)
            {
                if (doubleLeft != 0 && vy <= 0)
                {
                    vy = v0 * c.jump.doubleJumpMult; // double jump at the apex
                    doubleLeft = 0;
                }
                double mult = wind > 0 ? c.dashJump.gravityMult : vy > 0 ? 1 : c.jump.fallGravityMult;
                if (wind > 0) wind--;
                VerticalStepResult step = MovementMath.VerticalStep(vy, g * mult, dt);
                vy = step.vy;
                y += step.dy;
                t += dt;
                peak = JsMath.Max(peak, y);
                if (vy < 0 && doubleLeft == 0 && y <= dy) return peak >= dy ? speed * t : 0;
            }
            return 0;
        }

        /// <summary>Horizontal reach (m) of walking off a ledge `drop` m high (positive number), at walk speed.</summary>
        public static double DropReach(double drop, MovementConfig c)
        {
            double g = MovementMath.GravityFor(c.jump.height, c.jump.apexTime) * c.jump.fallGravityMult;
            return c.ground.walkSpeed * Math.Sqrt((2 * JsMath.Max(0, drop)) / g);
        }
    }
}
