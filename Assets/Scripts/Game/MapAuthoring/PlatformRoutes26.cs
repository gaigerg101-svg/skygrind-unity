// NIGHT26 (builder M): the platforming kit's scripted routes on the REAL Mover (Design/levels/kits/platforming.kits.json; Tools/night/gen-platforming-kits.py).
//   HallRun   the wall-ride hall: a wall-jump chain north between the two facing walls (the climber's lean, a wall jump after each 0.3 s of ride), a dash
//             at each gap, over a pit; the seconds from the ledge to the far floor. LevelValidate24 prints it against RIVAL's floor route ("hallRoute").
//   DashCross a dash gap: a run to the lip and a dash with NO jump lands on the far deck; a running jump alone does not (the kit teaches the dash).
// Pure functions over a Mover (no sim, no view); the harness `wallride` and the validate report share them.
using System;
using Skygrind.Core;

namespace Skygrind.Game.MapAuthoring
{
    public static class PlatformRoutes26
    {
        /// <summary>A debugging hook: called after each HallRun step (the harness's --trace).</summary>
        public static Action<int, Mover> Trace;

        public struct RunResult
        {
            public bool reached;
            public double seconds;
            public int wallJumps, dashes;
            public double bestZ, wallRideSeconds, jumpSeconds; // jumpSeconds: when the lip's jump was pressed (the run-up before it is not the route)
        }

        /// <summary>The hall: from (x, 0, zFrom) walking north to the far floor at or beyond zGoal; gaps = [[zSouthEdge, zNorthEdge], ...] (z decreasing); zJump = the lip, where the first jump is pressed (a run-up from the spawn: a ride needs wallRide.minSpeed).</summary>
        public static RunResult HallRun(Mover m, double dt, double x, double zFrom, double zGoal, double[][] gaps, double maxSeconds, double zJump = double.PositiveInfinity, double lean = 0.25, bool wallJumpChain = false)
        {
            m.Reset(x, 0, zFrom);
            var r = new RunResult { bestZ = zFrom };
            bool west = true;
            int steps = MovementMath.StepsFor(maxSeconds, dt);
            int rideSteps = 0, dashed = 0;
            for (int i = 0; i < steps; i++)
            {
                var f = new InputFrame { moveZ = 1, yaw = m.grounded && m.position.z > zJump ? 0 : west ? lean : -lean }; // straight on the run-up, the lean once the jump is made
                bool onWall = m.state == MoveState.WallRide;
                rideSteps = onWall ? rideSteps + 1 : 0;
                if (m.grounded) f.jump = new Button { held = m.position.z <= zJump, pressed = m.position.z <= zJump }; // the first jump is the lip's (zJump); later ones are the chain's
                else if (wallJumpChain && onWall && rideSteps * dt >= 0.3) f.jump = new Button { held = true, pressed = true };
                else f.jump = new Button { held = true, pressed = false };
                // a dash just before each gap (once per gap), in the air and off the wall
                for (int g = 0; g < gaps.Length; g++)
                {
                    if (g < dashed || m.grounded || onWall) continue;
                    if (m.position.z < gaps[g][0] + 1.5 && m.position.z > gaps[g][1])
                    {
                        f.dash = new Button { held = true, pressed = true };
                        f.yaw = 0;
                        dashed = g + 1;
                        r.dashes++;
                    }
                }
                if (r.jumpSeconds == 0 && f.jump.pressed) r.jumpSeconds = i * dt;
                int before = m.wallJumpsLeft;
                m.Step(f, dt);
                if (onWall) r.wallRideSeconds += dt;
                if (m.wallJumpsLeft < before)
                {
                    r.wallJumps++;
                    west = !west;
                }
                Trace?.Invoke(i, m);
                r.bestZ = Math.Min(r.bestZ, m.position.z);
                if (m.grounded && m.position.y >= -0.05 && m.position.z <= zGoal) { r.reached = true; r.seconds = (i + 1) * dt; return r; }
                if (m.position.y < -3) break; // fell in the pit: the run failed
            }
            r.seconds = -1;
            return r;
        }

        /// <summary>A dash gap: the real Mover runs north from (x, 0, zStart) toward the lip at zLip (the deck's north edge is zLip - gap). With dash true it presses dash
        /// at the lip (no jump); otherwise it presses jump at the lip (no dash). Returns whether it stands grounded on the far deck within 3 s.</summary>
        public static bool DashCross(Mover m, double dt, double x, double zStart, double zLip, double gap, bool dash)
        {
            m.Reset(x, 0, zStart);
            bool fired = false;
            int steps = MovementMath.StepsFor(3, dt);
            for (int i = 0; i < steps; i++)
            {
                var f = new InputFrame { moveZ = 1 };
                if (!fired && m.position.z <= zLip + 0.3)
                {
                    fired = true;
                    if (dash) f.dash = new Button { held = true, pressed = true };
                    else f.jump = new Button { held = true, pressed = true };
                }
                m.Step(f, dt);
                if (m.grounded && m.position.y >= -0.05 && m.position.z < zLip - gap) return true;
                if (m.position.y < -3) return false;
            }
            return false;
        }
    }
}
