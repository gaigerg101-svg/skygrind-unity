// Port of skygrind/tests/movementSim.test.js: 120 Hz simulations of the pure movement rules the mover uses (no
// scene). Same inputs, same expected values, same tolerances.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using static Skygrind.Core.MovementMath;
using static Skygrind.Core.Tests.Expect;

namespace Skygrind.Core.Tests
{
    public class MovementSimTests
    {
        static readonly MovementConfig config = new MovementConfig();
        static readonly double DT = 1 / config.loop.simHz;
        static readonly JumpConfig jump = config.jump;
        static readonly DashConfig dash = config.dash;
        static readonly DashJumpConfig dashJump = config.dashJump;
        static readonly GroundConfig ground = config.ground;
        static readonly SlideConfig slide = config.slide;
        static readonly WallJumpConfig wallJump = config.wallJump;
        static readonly WallRideConfig wallRide = config.wallRide;
        static readonly JumpPadConfig jumpPad = config.jumpPad;
        static readonly double G = GravityFor(jump.height, jump.apexTime);
        static readonly double V0 = JumpVelocity(jump.height, jump.apexTime);

        const MoveState GROUND = MoveState.Ground;
        const MoveState AIR = MoveState.Air;
        const MoveState SLIDE = MoveState.Slide;
        const MoveState DASH = MoveState.Dash;
        const MoveState WALLRIDE = MoveState.WallRide;
        const MoveState SLAM = MoveState.Slam;

        /// <summary>Flies from y = 0 with the mover's vertical rules (AIR state); returns the peak height.</summary>
        static double SimulateApex(double v0, bool jumpHeld, bool jumpAirtime)
        {
            var f = new GravityFlags { windActive = false, jumpHeld = jumpHeld, jumpAirtime = jumpAirtime };
            double vy = v0;
            double y = 0;
            double peak = 0;
            for (int i = 0; i < 2000 && y >= 0; i++)
            {
                VerticalStepResult step = VerticalStep(vy, G * GravityScale(AIR, vy, f, config), DT);
                vy = step.vy;
                y += step.dy;
                peak = Math.Max(peak, y);
            }
            return peak;
        }

        /// <summary>A resolveJump context with the JS defaults; override per test.</summary>
        static JumpContext Ctx(
            MoveState state = AIR,
            bool grounded = false,
            int coyoteSteps = 0,
            int doubleJumpsLeft = 1,
            bool landingSoon = false,
            bool wallNear = false,
            int? wallJumpsLeft = null,
            int boostSteps = 0,
            double? pips = null) =>
            new JumpContext
            {
                state = state,
                grounded = grounded,
                coyoteSteps = coyoteSteps,
                doubleJumpsLeft = doubleJumpsLeft,
                landingSoon = landingSoon,
                wallNear = wallNear,
                wallJumpsLeft = wallJumpsLeft ?? wallJump.maxPerAirtime,
                boostSteps = boostSteps,
                pips = pips ?? dash.maxPips,
                dashJumpCost = dashJump.extraPipCost,
            };

        // describe('jump (SPEC 1.3 values)')

        // Hand values (≈ 8.333, ≈ 23.148) replaced by the golden vectors for the same calls: exact.
        [Test, Description("v0 ≈ 8.33 m/s and g ≈ 23.1 m/s²")]
        public void Jump_V0AndGravity()
        {
            Exactly(V0, PortData.Num(Port.Result("movementMath", "jumpVelocity", jump.height, jump.apexTime)));
            Exactly(G, PortData.Num(Port.Result("movementMath", "gravityFor", jump.height, jump.apexTime)));
        }

        [Test, Description("apex is 1.5 ± 0.05 m without apex hang")]
        public void Jump_ApexWithoutHang()
        {
            double apex = SimulateApex(V0, jumpHeld: false, jumpAirtime: true);
            Assert.Greater(apex, 1.45);
            Assert.Less(apex, 1.55);
        }

        [Test, Description("apex hang (Space held) lifts the apex above 1.5 m, to about 1.59")]
        public void Jump_ApexHangLiftsApex()
        {
            double apex = SimulateApex(V0, jumpHeld: true, jumpAirtime: true);
            Assert.Greater(apex, 1.55);
            Assert.Less(apex, 1.62);
        }

        [Test, Description("variable jump release applies only once")]
        public void Jump_ReleaseCutAppliesOnce()
        {
            JumpRelease r = ReleaseJump(V0, true, jump.releaseMult, true); // still held: no cut
            Exactly(r.vy, V0);
            Assert.IsTrue(r.cutArmed);
            r = ReleaseJump(r.vy, false, jump.releaseMult, r.cutArmed); // released while rising: cut
            CloseTo(r.vy, V0 * jump.releaseMult, 10);
            Assert.IsFalse(r.cutArmed);
            double afterCut = r.vy;
            r = ReleaseJump(r.vy, true, jump.releaseMult, r.cutArmed);
            r = ReleaseJump(r.vy, false, jump.releaseMult, r.cutArmed); // second release: no second cut
            Exactly(r.vy, afterCut);
        }

        [Test, Description("the cut disarms at the apex, so releasing while falling does nothing")]
        public void Jump_CutDisarmsAtApex()
        {
            JumpRelease r = ReleaseJump(-1, false, jump.releaseMult, true);
            Exactly(r.vy, -1);
            Assert.IsFalse(r.cutArmed);
        }

        // describe('gravity multipliers (§3.6: exactly one applies)')

        static readonly GravityFlags held = new GravityFlags { jumpHeld = true, jumpAirtime = true, windActive = false };

        static GravityFlags With(GravityFlags f, bool? jumpHeld = null, bool? jumpAirtime = null, bool? windActive = null) =>
            new GravityFlags
            {
                jumpHeld = jumpHeld ?? f.jumpHeld,
                jumpAirtime = jumpAirtime ?? f.jumpAirtime,
                windActive = windActive ?? f.windActive,
            };

        [Test, Description("apex hang applies only when held, in a jump airtime, in AIR, with |vy| below the threshold")]
        public void Gravity_ApexHangConditions()
        {
            double slow = jump.apexHangSpeed / 2;
            Exactly(GravityScale(AIR, slow, held, config), jump.apexHangMult);
            Exactly(GravityScale(AIR, -slow, held, config), jump.apexHangMult);
            Exactly(GravityScale(AIR, slow, With(held, jumpHeld: false), config), 1); // released
            Exactly(GravityScale(AIR, -slow, With(held, jumpHeld: false), config), jump.fallGravityMult);
            Exactly(GravityScale(AIR, slow, With(held, jumpAirtime: false), config), 1); // pad / walk-off
            Exactly(GravityScale(AIR, jump.apexHangSpeed + 1, held, config), 1); // still rising fast
            Exactly(GravityScale(AIR, -(jump.apexHangSpeed + 1), held, config), jump.fallGravityMult);
            Exactly(GravityScale(WALLRIDE, slow, held, config), wallRide.gravityMult); // not in a ride
        }

        // Deviates from the JS on purpose: the shipped config has apexHangMult = dashJump.gravityMult = 0.5, so with
        // the real values this test cannot tell wind from hang (a swapped priority still passes). It uses its own
        // config with the two set apart; the inputs and the expected rule are the JS test's.
        [Test, Description("the dash-jump wind beats apex hang and fall")]
        public void Gravity_WindBeatsHangAndFall()
        {
            var distinct = new MovementConfig();
            distinct.dashJump.gravityMult = 0.5;
            distinct.jump.apexHangMult = 0.8;
            GravityFlags wind = With(held, windActive: true);
            Exactly(GravityScale(AIR, 0.5, wind, distinct), distinct.dashJump.gravityMult);
            Exactly(GravityScale(AIR, -10, wind, distinct), distinct.dashJump.gravityMult);
        }

        [Test, Description("never returns a product of two multipliers")]
        public void Gravity_NeverAProductOfMultipliers()
        {
            var allowed = new HashSet<double> { 0, 1, jump.fallGravityMult, jump.apexHangMult, dashJump.gravityMult, wallRide.gravityMult };
            foreach (MoveState state in Enum.GetValues(typeof(MoveState)))
            {
                foreach (double vy in new[] { -30, -1.9, -0.1, 0, 0.1, 1.9, 30 })
                {
                    foreach (bool jumpHeld in new[] { false, true })
                    {
                        foreach (bool jumpAirtime in new[] { false, true })
                        {
                            foreach (bool windActive in new[] { false, true })
                            {
                                var f = new GravityFlags { jumpHeld = jumpHeld, jumpAirtime = jumpAirtime, windActive = windActive };
                                Assert.IsTrue(allowed.Contains(GravityScale(state, vy, f, config)), $"{state} vy={vy}");
                            }
                        }
                    }
                }
            }
            Exactly(GravityScale(DASH, -5, held, config), 0);
            Exactly(GravityScale(SLAM, -5, held, config), 0);
            Exactly(GravityScale(GROUND, -5, held, config), 1);
        }

        // describe('resolveJump')

        [Test, Description("grounded and sliding presses jump from the ground")]
        public void Resolve_GroundAndSlide()
        {
            Assert.AreEqual(JumpKind.Ground, ResolveJump(Ctx(state: GROUND, grounded: true)));
            Assert.AreEqual(JumpKind.Slide, ResolveJump(Ctx(state: SLIDE, grounded: true)));
        }

        [Test, Description("coyote time turns an air press into a ground jump")]
        public void Resolve_CoyoteTime()
        {
            Assert.AreEqual(JumpKind.Ground, ResolveJump(Ctx(coyoteSteps: StepsFor(jump.coyoteTime, DT))));
        }

        [Test, Description("a press just before a predicted landing stays buffered instead of double jumping")]
        public void Resolve_BufferedBeforeLanding()
        {
            Assert.AreEqual(JumpKind.None, ResolveJump(Ctx(landingSoon: true)));
        }

        [Test, Description("otherwise the air press double jumps, once")]
        public void Resolve_DoubleJumpOnce()
        {
            Assert.AreEqual(JumpKind.Double, ResolveJump(Ctx()));
            Assert.AreEqual(JumpKind.None, ResolveJump(Ctx(doubleJumpsLeft: 0)));
        }

        [Test, Description("boost window beats slide and ground; a press during a slam waits for it")]
        public void Resolve_BoostWindow()
        {
            Assert.AreEqual(JumpKind.Boost, ResolveJump(Ctx(state: GROUND, grounded: true, boostSteps: 3)));
            Assert.AreEqual(JumpKind.Boost, ResolveJump(Ctx(state: SLIDE, grounded: true, boostSteps: 3)));
            Assert.AreEqual(JumpKind.None, ResolveJump(Ctx(state: SLAM)));
        }

        [Test, Description("fallDistance matches constant-gravity kinematics")]
        public void FallDistance_Kinematics()
        {
            CloseTo(FallDistance(-10, 20, 0.1), 1.1, 10);
            Exactly(FallDistance(5, 0, 0.1), 0);
        }

        // describe('dash')

        [Test, Description("covers 6.0 ± 0.1 m in whole 120 Hz steps (0.25 s)")]
        public void Dash_Covers6mInWholeSteps()
        {
            int steps = StepsFor(dash.duration, DT);
            Exactly(steps, PortData.Num(Port.Result("movementMath", "stepsFor", dash.duration, DT))); // golden (30)
            double distance = steps * DashSpeed(0, dash.speed) * DT;
            Assert.Greater(distance, 5.9);
            Assert.Less(distance, 6.1);
        }

        [Test, Description("dash speed = max(24, entry speed) and exit = max(walk, speed before)")]
        public void Dash_SpeedAndExit()
        {
            foreach (double entry in new[] { 0, 8, 11.6, 23.9, 24, 30, 40 })
            {
                Exactly(DashSpeed(entry, dash.speed), Math.Max(24, entry));
                Exactly(DashExitSpeed(ground.walkSpeed, entry), Math.Max(8, entry));
            }
        }

        [Test, Description("is always horizontal and full speed, even looking straight up or down")]
        public void Dash_AlwaysHorizontal()
        {
            // wishDirection only takes yaw; pitch never enters the dash direction.
            foreach (double yaw in new[] { 0, 1, -2.5 })
            {
                Vec3 d = WishDirection(0, 1, yaw);
                Exactly(d.y, 0);
                CloseTo(JsMath.Hypot(d.x, d.z), 1, 10);
            }
        }

        [Test, Description("dash jump costs one extra pip (2 total) and needs it")]
        public void Dash_JumpCostsExtraPip()
        {
            double pips = dash.maxPips;
            pips -= dash.pipCost; // the dash
            Assert.AreEqual(JumpKind.Dash, ResolveJump(Ctx(state: DASH, pips: pips)));
            pips -= dashJump.extraPipCost; // the dash jump
            Exactly(dash.maxPips - pips, 2);
            // With 0 pips left during a dash, the jump is a normal one (and doesn't spend a pip).
            Assert.AreEqual(JumpKind.Ground, ResolveJump(Ctx(state: DASH, pips: 0, grounded: true)));
            Assert.AreEqual(JumpKind.Double, ResolveJump(Ctx(state: DASH, pips: 0, grounded: false)));
        }

        // describe('wall jump')

        [Test, Description("allows 3 per airtime; the 4th falls through to the normal air rule")]
        public void WallJump_ThreePerAirtime()
        {
            int left = wallJump.maxPerAirtime;
            var kinds = new List<JumpKind>();
            for (int i = 0; i < 4; i++)
            {
                JumpKind kind = ResolveJump(Ctx(state: WALLRIDE, wallJumpsLeft: left, doubleJumpsLeft: 1));
                kinds.Add(kind);
                if (kind == JumpKind.Wall) left--;
            }
            CollectionAssert.AreEqual(new[] { JumpKind.Wall, JumpKind.Wall, JumpKind.Wall, JumpKind.Double }, kinds);
            Assert.AreEqual(0, left);
            Assert.AreEqual(JumpKind.None, ResolveJump(Ctx(wallNear: true, wallJumpsLeft: 0, doubleJumpsLeft: 0)));
        }

        [Test, Description("keeps the speed along the wall and kicks out along the normal")]
        public void WallJump_KeepsAlongSpeed_KicksOut()
        {
            var n = new Vec3(1, 0, 0);
            var v = new Vec3(-3, -2, -12);
            Vec3 o = WallJumpVelocity(v, n, wallJump.outSpeed, V0 * wallJump.v0Mult);
            Exactly(o.z, -12);
            Exactly(o.x, wallJump.outSpeed);
            CloseTo(o.y, V0 * wallJump.v0Mult, 10);
            CloseTo(AlongWallSpeed(o, n), 12, 10);
        }

        [Test, Description("a dash out of a wall ride never goes into the wall")]
        public void WallRide_DashNeverIntoWall()
        {
            var n = new Vec3(1, 0, 0);
            var forward = new Vec3(0, 0, -1);
            Vec3 noInput = WallRideDashDir(forward, false, forward, n);
            CloseTo(noInput.x * n.x + noInput.z * n.z, Math.Sqrt(0.5), 10); // 45° away (Math.SQRT1_2)
            Vec3 intoWall = WallRideDashDir(new Vec3(-1, 0, 0), true, forward, n);
            CloseTo(intoWall.x, 1, 10); // reflected straight out
            Vec3 along = WallRideDashDir(new Vec3(0, 0, 1), true, forward, n);
            CloseTo(along.z, 1, 10); // along the wall is kept
        }

        // describe('jump pads')

        [Test, Description("launch speed reaches the 8 m apex under the 1.3 gravity")]
        public void JumpPad_ReachesApex()
        {
            double v = PadLaunchSpeed(jumpPad.apexHeight, G);
            Exactly(v, PortData.Num(Port.Result("movementMath", "padLaunchSpeed", jumpPad.apexHeight, G))); // golden (≈ 19.245)
            double apex = SimulateApex(v, jumpHeld: true, jumpAirtime: false); // pads get no apex hang
            CloseTo(apex, jumpPad.apexHeight, 1);
        }

        // describe('stamina')

        [Test, Description("regenerates one pip at a time, from empty to full")]
        public void Stamina_OnePipAtATime()
        {
            double regenPerSec = dash.regenPerSec;
            int maxPips = dash.maxPips;
            double pips = 0;
            int wholePips = 0;
            int lastWholeStep = 0;
            int perPip = StepsFor(1 / regenPerSec, DT);
            for (int i = 1; i <= perPip * maxPips + 10; i++)
            {
                double before = pips;
                pips = RegenPips(pips, regenPerSec, maxPips, DT, false);
                Assert.LessOrEqual(pips - before, regenPerSec * DT + 1e-12);
                if (Math.Floor(pips) > wholePips)
                {
                    Assert.AreEqual(1, (int)Math.Floor(pips) - wholePips);
                    if (wholePips > 0) Assert.GreaterOrEqual(i - lastWholeStep, perPip - 1);
                    wholePips = (int)Math.Floor(pips);
                    lastWholeStep = i;
                }
            }
            Exactly(pips, maxPips);
        }

        [Test, Description("first pip takes 1 / 0.7 s")]
        public void Stamina_FirstPipTime()
        {
            double pips = 0;
            for (int i = 0; i < StepsFor(1 / dash.regenPerSec, DT); i++) pips = RegenPips(pips, dash.regenPerSec, dash.maxPips, DT, false);
            CloseTo(pips, 1, 2);
        }

        [Test, Description("does not regenerate while dashing and caps at max")]
        public void Stamina_NoRegenWhileDashing_Capped()
        {
            Exactly(RegenPips(1.5, dash.regenPerSec, dash.maxPips, DT, true), 1.5);
            Exactly(RegenPips(dash.maxPips, dash.regenPerSec, dash.maxPips, 10, false), dash.maxPips);
        }

        // describe('slide jump boost')

        [Test, Description("never pushes speed past 24 and never reduces speed")]
        public void SlideJump_NeverPastCap_NeverReduces()
        {
            double mult = slide.jumpHorizontalMult;
            double cap = slide.jumpBoostMaxSpeed;
            for (double speed = 0; speed <= 40; speed += 0.25)
            {
                double o = SlideJumpSpeed(speed, mult, cap);
                Assert.GreaterOrEqual(o, speed);
                Assert.LessOrEqual(o, Math.Max(speed, cap));
                if (speed * mult <= cap) CloseTo(o, speed * mult, 10);
            }
        }

        [Test, Description("a chain from the slide floor climbs to 24 and holds")]
        public void SlideJump_ChainClimbsTo24()
        {
            double speed = slide.floorSpeed;
            var chain = new double[6];
            for (int i = 0; i < 6; i++) chain[i] = speed = SlideJumpSpeed(speed, slide.jumpHorizontalMult, slide.jumpBoostMaxSpeed);
            // JS: chain.map((s) => +s.toFixed(2))
            Exactly(chain[0], PortData.Num(Port.Result("movementMath", "slideJumpSpeed", slide.floorSpeed, slide.jumpHorizontalMult, slide.jumpBoostMaxSpeed))); // golden
            var rounded = Array.ConvertAll(chain, s => Math.Round(s, 2, MidpointRounding.AwayFromZero));
            CollectionAssert.AreEqual(new[] { 13.92, 16.7, 20.04, 24, 24, 24 }, rounded);
        }

        // describe('direction + cap helpers')

        [Test, Description("wishDirection: yaw 0 looks toward −z, D is +x")]
        public void Helpers_WishDirection()
        {
            Vec3 f = WishDirection(0, 1, 0);
            CloseTo(f.x, 0, 10);
            CloseTo(f.z, -1, 10);
            Vec3 r = WishDirection(1, 0, 0);
            CloseTo(r.x, 1, 10);
            CloseTo(r.z, 0, 10);
            Vec3 diag = WishDirection(1, 1, 0);
            CloseTo(JsMath.Hypot(diag.x, diag.z), 1, 10);
        }

        [Test, Description("clampHorizontal caps at 40 m/s and keeps vertical speed")]
        public void Helpers_ClampHorizontal()
        {
            Vec3 v = ClampHorizontal(new Vec3(60, -5, 80), config.speed.maxHorizontal);
            CloseTo(JsMath.Hypot(v.x, v.z), 40, 10);
            Exactly(v.y, -5);
        }

        // describe('aimed jump pads (SPEC 2.0)')

        [Test, Description("lands on the target with split gravity and peaks apexAbove over the higher end")]
        public void AimedPad_LandsOnTarget()
        {
            var from = new Vec3(0, 0, 0);
            var to = new Vec3(0, 9, -14);
            double apexAbove = 2;
            Vec3 v = PadLaunchVelocity(from, to, apexAbove, G, jump.fallGravityMult);
            double x = from.x;
            double y = from.y;
            double z = from.z;
            double vy = v.y;
            double peak = y;
            for (int i = 0; i < 2000; i++)
            {
                double g = G * (vy > 0 ? 1 : jump.fallGravityMult);
                VerticalStepResult step = VerticalStep(vy, g, DT);
                vy = step.vy;
                y += step.dy;
                x += v.x * DT;
                z += v.z * DT;
                peak = Math.Max(peak, y);
                if (vy < 0 && y <= to.y) break;
            }
            CloseTo(peak, to.y + apexAbove, 1);
            Assert.Less(JsMath.Hypot(x - to.x, z - to.z), 0.3);
        }

        // describe('slope boost (SPEC 2.0)')

        static double Deg(double d) => (d * Math.PI) / 180;

        static Vec3 Slope(double theta, Vec3 downhill) =>
            new Vec3(Math.Sin(theta) * downhill.x, Math.Cos(theta), Math.Sin(theta) * downhill.z);

        static readonly Vec3 north = new Vec3(0, 0, -1);

        [Test, Description("sliding downhill gains g·sinθ·cosθ, uphill bleeds the same, flat ground gives 0")]
        public void Slope_DownhillGainsUphillBleeds()
        {
            Vec3 n = Slope(Deg(20), north); // downhill is north
            double down = SlopeSlideAccel(n, north, G, 1);
            CloseTo(down, G * Math.Sin(Deg(20)) * Math.Cos(Deg(20)), 10);
            CloseTo(SlopeSlideAccel(n, new Vec3(0, 0, 1), G, 1), -down, 10);
            Exactly(SlopeSlideAccel(new Vec3(0, 1, 0), north, G, 1), 0);
            CloseTo(SlopeSlideAccel(n, new Vec3(1, 0, 0), G, 1), 0, 10); // across the slope
        }

        [Test, Description("an 8 m 20° ramp adds speed, and the 40 m/s cap still holds")]
        public void Slope_RampAddsSpeed_CapHolds()
        {
            Vec3 n = Slope(Deg(20), north);
            double length = config.slide.slopeRampLength;
            double speed = slide.floorSpeed;
            double travelled = 0;
            while (travelled < length)
            {
                speed += SlopeSlideAccel(n, north, G, config.slide.slopeBoost) * DT;
                travelled += speed * DT;
            }
            Assert.Greater(speed, slide.floorSpeed + 3);
            double fast = 39.9;
            fast = Math.Min(config.speed.maxHorizontal, fast + SlopeSlideAccel(n, north, G, 1) * DT);
            Assert.LessOrEqual(fast, 40);
        }
    }
}
