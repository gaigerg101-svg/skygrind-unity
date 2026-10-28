// NIGHT34 builder M (design.n34.flow34; NIGHT34.md M1): movement patterns from Research/2026-10-11-wave34 RESEARCH.md table M that SKYGRIND
// lacked, each behind its own sub-switch. The Mover reads them through one field (Mover.flow34, a State: the run's rules and their clocks),
// neutral by default: with Params.Neutral (every flag off: the golden vectors', the replays' and every row-off run's config) every branch in
// Mover.cs that reads it is skipped, so the web's mover bit for bit (CLAUDE.md rule 3). Whole steps (MovementMath.StepsFor at the config's
// simHz); no allocation; no randomness.
// What was grepped first (the mechanism, not the name), and found already in the web's mover, so NOT re-built tonight:
// - table M 2, no ground friction on a buffered landing jump: the buffered jump fires in TryJump before StepGround on the first grounded step
//   (Mover.cs `if (bufferSteps > 0 ...) TryJump` ahead of the state switch) and StepGround skips friction while groundedSteps == 0
//   (SPEC 3.2 "so bunny hops keep speed"); the harness `flow34 bhop` measures it (speed kept 100 %).
// - coyote and buffer on SLIDES: walking or sliding off an edge starts coyote (AfterMove, state Ground or Slide); a buffered press that lands
//   in a slide jumps as the slide jump (JumpKind.Slide, the carry). Buffer onto a wall ride: TryJump in WallRide is the wall jump.
// - apex hang (jump.apexHangMult 0.5 below apexHangSpeed 2) and corner correction (collision.cornerCorrect 0.25 m) exist too (table M 8).
// Built tonight (sub-switches under design.n34.flow34):
// - wallCoyote (table M 1, "coyote after leaving a wall too"): a wall ride that ends for any reason but a jump, a landing or the rope
//   leaves wallCoyoteTime s in which a jump is the WALL JUMP off that ride's wall (its normal), even once the wall is out of the 0.12 m probe.
// - wallBuffer (table M 1, "let a buffered jump fire on wall contact as a wall jump"): in the air with a double jump left, a press while a
//   valid wall is predicted within the buffer along the horizontal velocity is held for the wall (the web's landingSoon rule for floors,
//   mirrored for walls), so the double jump is not spent a few steps before the wall.
// - zipCarry (table M 7, "add, never replace, velocity on release"): the ZIP pull (the tap grapple, NIGHT31) replaces the velocity with the
//   pull each substep; with zipCarry its release adds back the horizontal momentum the pull erased (the pre-pull horizontal velocity's part
//   across the release direction, x carryMult), so a sideways run carries through a pull into a slide or a hop.
using System;

namespace Skygrind.Core
{
    public static class Flow34
    {
        /// <summary>design.n34.flow34, read once (every flag off when the block is absent or off).</summary>
        public struct Params
        {
            public bool wallCoyote, wallBuffer, zipCarry, ridePhase, lurch, slideBounce;
            public double wallCoyoteTime, carryMult, rideGripTime, rideRampTime, lurchWindow, lurchMinAngleDeg, lurchMaxTurnDeg, lurchSpeedKeep, slideBouncePush;
            /// <summary>slideBounce: the steps after a slide start that still count as sliding (ULTRAKILL's 4 frames, read as 4 sim steps).</summary>
            public int slideBounceSteps;
            /// <summary>The wall coyote in whole steps at the config's simHz.</summary>
            public int wallCoyoteSteps, rideGripSteps, rideRampSteps, lurchSteps;

            /// <summary>Every rule off: the web's mover.</summary>
            public static Params Neutral => default;

            /// <summary>Any rule on (the Mover's branches run only then).</summary>
            public bool Any => wallCoyote || wallBuffer || zipCarry || ridePhase || lurch || slideBounce;

            /// <summary>design.n34.flow34's sub-switches and numbers, ON only when `rowOn` (the run's n34.flow34 bit) and the sub-switch say so.</summary>
            public static Params From(GameConfig cfg, bool rowOn)
            {
                if (!rowOn || !cfg.Has("design.n34.flow34.wallCoyote")) return Neutral;
                double dt = 1 / cfg.movement.loop.simHz;
                var p = new Params
                {
                    wallCoyote = cfg.Design("n34.flow34.wallCoyote.enabled"),
                    wallBuffer = cfg.Design("n34.flow34.wallBuffer.enabled"),
                    zipCarry = cfg.Design("n34.flow34.zipCarry.enabled"),
                    wallCoyoteTime = cfg.Num("design.n34.flow34.wallCoyote.time"),
                    carryMult = cfg.Num("design.n34.flow34.zipCarry.carryMult"),
                };
                p.wallCoyoteSteps = MovementMath.StepsFor(p.wallCoyoteTime, dt);
                if (cfg.Has("design.n34.flow34.ridePhase")) // leftover 5 (techniques 4 and 5): absent in a config written before them
                {
                    p.ridePhase = cfg.Design("n34.flow34.ridePhase.enabled");
                    p.rideGripTime = cfg.Num("design.n34.flow34.ridePhase.gripTime");
                    p.rideRampTime = cfg.Num("design.n34.flow34.ridePhase.rampTime");
                    p.rideGripSteps = MovementMath.StepsFor(p.rideGripTime, dt);
                    p.rideRampSteps = MovementMath.StepsFor(p.rideRampTime, dt);
                }
                if (cfg.Has("design.n34.flow34.slideBounce34")) // the REA addendum (rea-ultrakill.md, Movement): absent in a config written before it
                {
                    p.slideBounce = cfg.Design("n34.flow34.slideBounce34.enabled");
                    p.slideBouncePush = cfg.Num("design.n34.flow34.slideBounce34.push");
                    p.slideBounceSteps = cfg.Int("design.n34.flow34.slideBounce34.startSteps");
                }
                if (cfg.Has("design.n34.flow34.lurch"))
                {
                    p.lurch = cfg.Design("n34.flow34.lurch.enabled");
                    p.lurchWindow = cfg.Num("design.n34.flow34.lurch.window");
                    p.lurchMinAngleDeg = cfg.Num("design.n34.flow34.lurch.minAngleDeg");
                    p.lurchMaxTurnDeg = cfg.Num("design.n34.flow34.lurch.maxTurnDeg");
                    p.lurchSpeedKeep = cfg.Num("design.n34.flow34.lurch.speedKeep");
                    p.lurchSteps = MovementMath.StepsFor(p.lurchWindow, dt);
                }
                return p;
            }
        }

        /// <summary>The Mover's flow state (Mover.flow34): the run's rules and their clocks, one struct so the run snapshot (MoverState) saves it whole.</summary>
        public struct State
        {
            public Params rules;
            /// <summary>Steps of wall coyote left, and the ended ride's wall normal (horizontal, unit).</summary>
            public int wallCoyoteLeft;
            public Vec3 wallCoyoteNormal;
            /// <summary>The ZIP's pre-pull horizontal velocity, captured at its first pull substep (armed until the rope is let go).</summary>
            public bool carryArmed;
            public Vec3 carryVel;
            /// <summary>The run's counts (the harness reads them): wall jumps through the coyote, presses held for a wall, carries added.</summary>
            public int coyoteWallJumps, heldForWall, carries;
            /// <summary>lurch: armed by a takeoff jump, spent by the one redirect; the steps since that jump; the run's lurches.</summary>
            public bool lurchArmed;
            public int sinceJump, lurches;
            /// <summary>slideBounce: steps since the last slide start (large: none yet), and the run's bounces.</summary>
            public int sinceSlide, bounces;
            /// <summary>NIGHT35 wallride35 (design.n35.wallride35, Core/Ride35.cs): the Lucio ride's rules and clocks, inside this state so the run
            /// snapshot saves them with it (Mover.SetRide35 sets them; default: off, the web's ride).</summary>
            public Ride35.State ride35;
        }

        /// <summary>ridePhase (table M 4): the wall ride's gravity multiplier `elapsed` steps into the ride: 0 for the grip steps while not rising
        /// (a rising entry keeps the web's gravityMult, so the grip never climbs a wall), then a linear ramp to `baseMult` over the ramp steps.</summary>
        public static double RideGravity(int elapsed, in Params p, double baseMult, double vy)
        {
            if (elapsed < p.rideGripSteps) return vy > 0 ? baseMult : 0;
            if (p.rideRampSteps <= 0) return baseMult;
            int k = elapsed - p.rideGripSteps;
            if (k >= p.rideRampSteps) return baseMult;
            return baseMult * k / p.rideRampSteps;
        }

        /// <summary>lurch (table M 6): the horizontal velocity turned toward the unit wish (wx, wz) by at most maxTurnDeg, its speed x speedKeep;
        /// null when the wish is within minAngleDeg of the motion (no lurch: the press was not a change of mind).</summary>
        /// <summary>slideBounce (rea-ultrakill.md, Movement: NewMovement.cs WallJump :1754-1763, the rule's shape only): the horizontal direction off a
        /// wall for a slide's wall jump: the slide direction reflected off the (horizontal, unit) wall normal, plus push x the normal, renormalised.</summary>
        public static Vec3 BounceDir(Vec3 slideDir, Vec3 n, double push)
        {
            double into = slideDir.x * n.x + slideDir.z * n.z;
            double rx = slideDir.x - 2 * into * n.x + n.x * push, rz = slideDir.z - 2 * into * n.z + n.z * push;
            double l = JsMath.Hypot(rx, rz);
            return l > 1e-9 ? new Vec3(rx / l, 0, rz / l) : new Vec3(n.x, 0, n.z);
        }

        public static Vec3? Lurch(Vec3 v, double wx, double wz, in Params p)
        {
            double hs = JsMath.Hypot(v.x, v.z);
            if (hs < 1e-6) return null;
            double ux = v.x / hs, uz = v.z / hs;
            double cos = Math.Max(-1, Math.Min(1, ux * wx + uz * wz));
            double angle = JsMath.Acos(cos);
            if (angle < p.lurchMinAngleDeg * Math.PI / 180) return null;
            double turn = Math.Min(angle, p.lurchMaxTurnDeg * Math.PI / 180);
            double cross = ux * wz - uz * wx; // the turn's sign: toward the wish
            double a = cross >= 0 ? turn : -turn;
            double c = JsMath.Cos(a), s = JsMath.Sin(a);
            double nx = ux * c - uz * s, nz = ux * s + uz * c;
            double speed = hs * p.lurchSpeedKeep;
            return new Vec3(nx * speed, v.y, nz * speed);
        }

        /// <summary>The release velocity with the carry: `release` plus the part of the pre-pull horizontal velocity `pre` across the
        /// release's horizontal direction, x mult (pre's part along it is the pull's own speed already). No release direction: pre's horizontal x mult.</summary>
        public static Vec3 Carry(Vec3 release, Vec3 pre, double mult)
        {
            double hx = release.x, hz = release.z;
            double hl = JsMath.Hypot(hx, hz);
            double px = pre.x, pz = pre.z;
            if (hl > 1e-9)
            {
                double ux = hx / hl, uz = hz / hl;
                double along = px * ux + pz * uz;
                px -= ux * along;
                pz -= uz * along;
            }
            return new Vec3(release.x + px * mult, release.y, release.z + pz * mult);
        }
    }
}
