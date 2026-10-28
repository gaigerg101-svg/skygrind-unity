// The web game's movement regression presets (skygrind/src/debug/scripts.js: the M1 + M2 presets and their CHECKS
// pass rules), run on the ported Mover against any ICollisionWorld. The runner keeps scripts.js semantics: one
// InputFrame per fixed step from the current segment (button `pressed` = a key-down edge), `until` segments checked
// against the state after each step with a 3 s cap (scriptMaxWait), snapshots rounded to mm like getState().
// Spike scenarios (marked) are not in the web CHECKS; they cover the ramps, the non-convex mesh and the swing.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Skygrind.Core
{
    public struct ScriptInput
    {
        public double moveX;
        public double moveZ;
        public double? yaw;
        public double? pitch;
        public bool jump;
        public bool dash;
        public bool slide;
        public bool grapple;
        public bool reel;
    }

    public struct Snapshot
    {
        public Vec3 position;
        public Vec3 velocity;
        public double speed;
        public MoveState state;
        public bool grounded;
        public bool crouched;
        public double pips;
        public int doubleJumpsLeft;
        public int wallJumpsLeft;
        public double lastApexHeight;
        public double lastDashDistance;
        public bool touchingWall;
        public int step;
    }

    public sealed class Segment
    {
        public int frames = -1;
        public string until; // "grounded" | "airborne" | null (use predicate)
        public Func<Snapshot, bool> predicate;
        public int maxFrames = -1;
        public ScriptInput input;
    }

    public struct LoggedEvent
    {
        public MoverEvent e;
        public int step;
    }

    public sealed class RunResult
    {
        public List<int> counts = new List<int>();
        public List<Snapshot> snapshots = new List<Snapshot>();
        public bool timedOut;
        public Snapshot state;
    }

    public struct PresetOutcome
    {
        public bool pass;
        public string summary;
    }

    public sealed class PresetRunner
    {
        public readonly MovementConfig config;
        public readonly Mover mover;
        public readonly ArenaMap map;
        public readonly CoreGrid core; // stepped before the mover (main.js: arena.stepCore, then player.step)
        public Action afterCoreStep; // e.g. sync Unity's pillar colliders to the core's new heights
        readonly GrappleAim aim;
        public readonly List<LoggedEvent> log = new List<LoggedEvent>();
        public readonly List<Vec3> trace = new List<Vec3>(); // feet after every step (parity comparisons)
        public readonly List<MoveState> stateTrace = new List<MoveState>();
        public int step;
        double lookYaw;
        double lookPitch;
        bool prevJump, prevDash, prevSlide, prevGrapple, prevReel;
        readonly double dt;

        public PresetRunner(MovementConfig config, ArenaMap map, ICollisionWorld world, CoreGrid core = null)
        {
            this.config = config;
            this.map = map;
            this.core = core;
            dt = 1 / config.loop.simHz;
            mover = new Mover(config, world) { ropeEnabled = true };
            aim = new GrappleAim(config, map.grappleNodes);
            mover.Events += e => log.Add(new LoggedEvent { e = e, step = step });
        }

        public int S(double seconds) => MovementMath.StepsFor(seconds, dt);

        static double Round(double n) => JsMath.Round(n * 1000) / 1000;

        static Vec3 Round(Vec3 v) => new Vec3(Round(v.x), Round(v.y), Round(v.z));

        public Snapshot State() => new Snapshot
        {
            position = Round(mover.position),
            velocity = Round(mover.velocity),
            speed = Round(mover.Speed),
            state = mover.state,
            grounded = mover.grounded,
            crouched = mover.crouched,
            pips = Round(mover.pips),
            doubleJumpsLeft = mover.doubleJumpsLeft,
            wallJumpsLeft = mover.wallJumpsLeft,
            lastApexHeight = Round(mover.lastApexHeight),
            lastDashDistance = Round(mover.lastDashDistance),
            touchingWall = mover.touchingWall,
            step = step,
        };

        public IEnumerable<LoggedEvent> EventsOf(MoverEventType type) => log.Where(l => l.e.type == type);

        /// <summary>Respawn at a named test spot; `settle` stands still first (ground spots). Clears the event log.</summary>
        public void Place(string spotName, bool settle = true)
        {
            Spot spot = TrainingMap.TestSpots[spotName];
            mover.Reset(spot.x, spot.y, spot.z);
            lookYaw = spot.yawDeg * Math.PI / 180;
            lookPitch = 0;
            prevJump = prevDash = prevSlide = prevGrapple = prevReel = false;
            if (settle) Run(Frames(S(0.25), default)); // config.debug.settleTime
            log.Clear();
        }

        /// <summary>Mid-air against corridor wall A, moving down the corridor at `speed` (deterministic setup).</summary>
        public void PlaceOnWallA(double speed)
        {
            Place("corridorWallA", false);
            mover.velocity = new Vec3(-0.5, 0, -speed); // config.debug.wallRidePush
        }

        public RunResult Run(params Segment[] segments)
        {
            var r = new RunResult();
            int index = 0;
            int frames = 0;
            bool Done(Segment seg)
            {
                if (seg.frames >= 0) return frames >= seg.frames;
                if (frames >= (seg.maxFrames >= 0 ? seg.maxFrames : S(3))) // config.debug.scriptMaxWait
                {
                    r.timedOut = true;
                    return true;
                }
                if (seg.until == "grounded") return mover.grounded;
                if (seg.until == "airborne") return !mover.grounded;
                return seg.predicate(State());
            }
            while (true)
            {
                while (index < segments.Length && Done(segments[index]))
                {
                    r.counts.Add(frames);
                    r.snapshots.Add(State());
                    frames = 0;
                    index++;
                }
                if (index >= segments.Length) break;
                step++;
                InputFrame frame = Sample(segments[index].input);
                frames++;
                Simulate(frame);
            }
            r.state = State();
            return r;
        }

        InputFrame Sample(ScriptInput input)
        {
            if (input.yaw.HasValue) lookYaw = input.yaw.Value;
            if (input.pitch.HasValue) lookPitch = input.pitch.Value;
            var f = new InputFrame { moveX = input.moveX, moveZ = input.moveZ, yaw = lookYaw, pitch = lookPitch };
            f.jump = new Button { held = input.jump, pressed = input.jump && !prevJump };
            f.dash = new Button { held = input.dash, pressed = input.dash && !prevDash };
            f.slide = new Button { held = input.slide, pressed = input.slide && !prevSlide };
            f.grapple = new Button { held = input.grapple, pressed = input.grapple && !prevGrapple };
            f.reel = new Button { held = input.reel, pressed = input.reel && !prevReel };
            prevJump = input.jump;
            prevDash = input.dash;
            prevSlide = input.slide;
            prevGrapple = input.grapple;
            prevReel = input.reel;
            return f;
        }

        /// <summary>main.js simulate(): the hook pick, the core tween (carries the body), the mover step, then the pads.</summary>
        void Simulate(InputFrame frame)
        {
            Vec3 eye = new Vec3(mover.position.x, mover.position.y + config.body.eyeHeight, mover.position.z);
            aim.Step(ref frame, mover, eye, mover.World);
            if (core != null)
            {
                core.Step(mover);
                afterCoreStep?.Invoke();
            }
            mover.Step(frame, dt);
            map.CheckPads(mover, config);
            trace.Add(mover.position);
            stateTrace.Add(mover.state);
        }

        // --- segment helpers (scripts.js hold / frames / until) ---------------------------------------------------

        public Segment Hold(double seconds, ScriptInput input) => new Segment { frames = S(seconds), input = input };

        public static Segment Frames(int n, ScriptInput input) => new Segment { frames = n, input = input };

        public static Segment Until(Func<Snapshot, bool> predicate, ScriptInput input, int maxFrames = -1) =>
            new Segment { predicate = predicate, input = input, maxFrames = maxFrames };

        public static Segment UntilGrounded(ScriptInput input) => new Segment { until = "grounded", input = input };

        public static Segment UntilAirborne(ScriptInput input) => new Segment { until = "airborne", input = input };
    }

    /// <summary>The presets and their pass rules. Names match scripts.js.</summary>
    public static class MovementPresets
    {
        static readonly ScriptInput N = default;
        static readonly ScriptInput J = new ScriptInput { jump = true };

        static bool Near(double a, double b, double tol) => Math.Abs(a - b) <= tol;

        static double Round(double n) => JsMath.Round(n * 1000) / 1000;

        static Func<Snapshot, bool> IsState(MoveState s) => x => x.state == s;

        static Func<Snapshot, bool> NotState(MoveState s) => x => x.state != s;

        public static readonly (string name, bool web, Func<PresetRunner, PresetOutcome> run)[] All =
        {
            ("jumpApex", true, JumpApex),
            ("dash", true, DashPreset),
            ("stamina", true, Stamina),
            ("slideJumpChain", true, SlideJumpChain),
            ("coyote", true, Coyote),
            ("buffer", true, r => BufferTest(r, true)),
            ("bufferPredict", true, r => BufferTest(r, false)),
            ("pillarEdge", true, PillarEdge),
            ("wall", true, Wall),
            ("dashAllStates", true, DashAllStates),
            ("dashJump", true, DashJump),
            ("slamBoost", true, SlamBoost),
            ("wallRide", true, WallRide),
            ("wallJumpChain", true, WallJumpChain),
            ("jumpPad", true, JumpPad),
            ("slamVsSlide", true, SlamVsSlide),
            ("fullChain", true, FullChain),
            ("tunnel (spike check)", false, Tunnel),
            ("rampSlide (spike)", false, RampSlide),
            ("meshPiece (spike)", false, MeshPiece),
            ("swing (spike)", false, Swing),
            ("grinder (spike)", false, Grinder),
        };

        static PresetOutcome Out(bool pass, string summary) => new PresetOutcome { pass = pass, summary = summary };

        // CHECKS.jumpApex: near(apexNoHang, 1.5, 0.02) && near(apexHeld, 1.584, 0.02)
        static PresetOutcome JumpApex(PresetRunner r)
        {
            double Fly()
            {
                r.Place("lane");
                r.Run(r.Hold(0.5, J), PresetRunner.UntilGrounded(N)); // config.debug.jumpHoldTime
                return r.State().lastApexHeight;
            }
            double apexHeld = Fly();
            double hang = r.config.jump.apexHangMult;
            r.config.jump.apexHangMult = 1; // measure the pure 1.5 m jump without apex hang
            double apexNoHang;
            try
            {
                apexNoHang = Fly();
            }
            finally
            {
                r.config.jump.apexHangMult = hang;
            }
            return Out(Near(apexNoHang, 1.5, 0.02) && Near(apexHeld, 1.584, 0.02), $"apex {apexNoHang} / held {apexHeld}");
        }

        // CHECKS.dash: near(dashDistance, 6, 0.05)
        static PresetOutcome DashPreset(PresetRunner r)
        {
            r.Place("lane");
            Snapshot end = r.Run(PresetRunner.Frames(1, new ScriptInput { dash = true }), PresetRunner.Until(NotState(MoveState.Dash), N)).state;
            return Out(Near(end.lastDashDistance, 6, 0.05), $"dash {end.lastDashDistance} m");
        }

        // CHECKS.stamina: dashes === 3 && failedDashes === 1
        static PresetOutcome Stamina(PresetRunner r)
        {
            r.Place("lane");
            var segs = new List<Segment>();
            for (int i = 0; i < r.config.dash.maxPips; i++)
            {
                segs.Add(PresetRunner.Frames(1, new ScriptInput { dash = true }));
                segs.Add(r.Hold(0.25, N)); // staminaDashGap
            }
            segs.Add(PresetRunner.Frames(1, new ScriptInput { dash = true })); // a 4th dash with 0 pips must fail
            for (int i = 0; i < 10; i++) segs.Add(r.Hold(0.5, N)); // staminaSampleTime / staminaSampleInterval
            r.Run(segs.ToArray());
            int dashes = r.EventsOf(MoverEventType.Dash).Count();
            int failed = r.EventsOf(MoverEventType.DashFail).Count();
            return Out(dashes == 3 && failed == 1, $"{dashes} dashes, {failed} failed");
        }

        // CHECKS.slideJumpChain: hops.at(-1).after === 24
        static PresetOutcome SlideJumpChain(PresetRunner r)
        {
            r.Place("lane");
            var segs = new List<Segment> { r.Hold(0.5, new ScriptInput { moveZ = 1 }), PresetRunner.Frames(1, new ScriptInput { moveZ = 1, slide = true }) };
            for (int i = 0; i < 5; i++)
            {
                // Tap Space (short hops) so five hops at up to 24 m/s stay on the floor.
                segs.Add(PresetRunner.Frames(1, new ScriptInput { moveZ = 1, slide = true, jump = true }));
                segs.Add(PresetRunner.UntilGrounded(new ScriptInput { moveZ = 1, slide = true }));
            }
            r.Run(segs.ToArray());
            double[] hops = r.EventsOf(MoverEventType.Jump).Select(l => Round(l.e.speedAfter)).ToArray();
            return Out(hops.Length > 0 && hops[hops.Length - 1] == 24, "hops " + string.Join(" ", hops));
        }

        // CHECKS.coyote: coyoteJumpFired
        static PresetOutcome Coyote(PresetRunner r)
        {
            r.Place("ledgeEdge");
            int delay = r.S(0.08); // coyoteTestDelay
            var fwd = new ScriptInput { moveZ = 1 };
            r.Run(PresetRunner.UntilAirborne(fwd), PresetRunner.Frames(delay - 1, fwd), r.Hold(0.5, new ScriptInput { moveZ = 1, jump = true }), PresetRunner.UntilGrounded(N));
            LoggedEvent[] jumps = r.EventsOf(MoverEventType.Jump).ToArray();
            bool fired = jumps.Length > 0 && jumps[0].e.coyote;
            return Out(fired, $"coyote {fired}");
        }

        // CHECKS.buffer: bufferedJumpFired; CHECKS.bufferPredict: bufferedJumpFired && doubleJumpsUsed === 0
        static PresetOutcome BufferTest(PresetRunner r, bool withDoubleJump)
        {
            var seq = new List<Segment> { r.Hold(0.5, J), PresetRunner.Frames(1, N) };
            if (withDoubleJump)
            {
                seq.Add(r.Hold(0.5, J));
                seq.Add(PresetRunner.Frames(1, N));
            }
            r.Place("lane");
            RunResult pass1 = r.Run(seq.Concat(new[] { PresetRunner.UntilGrounded(N) }).ToArray());
            int fallSteps = pass1.counts[pass1.counts.Count - 1];
            int lead = r.S(0.08); // bufferTestLead
            r.Place("lane");
            r.Run(seq.Concat(new[] { PresetRunner.Frames(fallSteps - lead, N), r.Hold(0.5, J), PresetRunner.UntilGrounded(N) }).ToArray());
            bool buffered = r.EventsOf(MoverEventType.Jump).Any(l => l.e.buffered);
            int doubles = r.EventsOf(MoverEventType.DoubleJump).Count();
            bool pass = withDoubleJump ? buffered : buffered && doubles == 0;
            return Out(pass, $"buffered {buffered}, double jumps {doubles}");
        }

        // CHECKS.pillarEdge: drift === 0
        static PresetOutcome PillarEdge(PresetRunner r)
        {
            r.Place("pillarEdge");
            Snapshot start = r.State();
            r.Run(PresetRunner.Until(_ => false, N, r.S(2))); // edgeTestTime
            Snapshot end = r.State();
            double drift = Round(JsMath.Hypot(end.position.x - start.position.x, end.position.z - start.position.z));
            return Out(drift == 0, $"drift {drift} (dy {Round(end.position.y - start.position.y)})");
        }

        // CHECKS.wall: rides === 0
        static PresetOutcome Wall(PresetRunner r)
        {
            r.Place("wall");
            r.Run(PresetRunner.Until(_ => false, new ScriptInput { moveZ = 1, moveX = 1 }, r.S(1.2))); // wallTestTime
            int rides = r.EventsOf(MoverEventType.WallRideStart).Count();
            return Out(rides == 0, $"rides {rides}");
        }

        static double DashFrom(PresetRunner r, Segment[] setup, ScriptInput extra)
        {
            var press = extra;
            press.dash = true;
            Snapshot state = r.Run(setup.Concat(new[] { PresetRunner.Frames(1, press), PresetRunner.Until(NotState(MoveState.Dash), extra) }).ToArray()).state;
            return state.lastDashDistance;
        }

        // CHECKS.dashAllStates: every distance near 6 (0.05)
        static PresetOutcome DashAllStates(PresetRunner r)
        {
            r.Place("lane");
            double ground = DashFrom(r, new Segment[0], N);
            r.Place("lane");
            double air = DashFrom(r, new[] { r.Hold(0.3, J) }, N); // dashAllStatesAirDelay
            r.Place("lane");
            double slide = DashFrom(r, new[] { r.Hold(0.5, new ScriptInput { moveZ = 1 }), PresetRunner.Frames(1, new ScriptInput { slide = true }) }, new ScriptInput { slide = true });
            r.PlaceOnWallA(12); // wallRideEntrySpeed
            double ride = DashFrom(r, new[] { PresetRunner.Until(IsState(MoveState.WallRide), N) }, N);
            bool pass = Near(ground, 6, 0.05) && Near(air, 6, 0.05) && Near(slide, 6, 0.05) && Near(ride, 6, 0.05);
            return Out(pass, $"ground {ground}, air {air}, slide {slide}, wall ride {ride}");
        }

        // CHECKS.dashJump: fired && speed === 24
        static PresetOutcome DashJump(PresetRunner r)
        {
            r.Place("lane");
            Snapshot state = r.Run(PresetRunner.Frames(1, new ScriptInput { dash = true }), r.Hold(0.05, N), PresetRunner.Frames(1, J), PresetRunner.UntilGrounded(N)).state;
            LoggedEvent[] dj = r.EventsOf(MoverEventType.DashJump).ToArray();
            double speed = dj.Length > 0 ? Round(dj[0].e.speed) : 0;
            return Out(dj.Length > 0 && speed == 24, $"speed {speed}, apex {state.lastApexHeight}");
        }

        // CHECKS.slamBoost: boostFired && near(boostApex, 4.86, 0.05)
        static PresetOutcome SlamBoost(PresetRunner r)
        {
            r.Place("deckEdge");
            Snapshot state = r.Run(
                PresetRunner.UntilAirborne(new ScriptInput { moveZ = 1 }),
                r.Hold(0.1, N), // slamDropDelay
                PresetRunner.Frames(1, new ScriptInput { slide = true }),
                PresetRunner.UntilGrounded(N),
                PresetRunner.Frames(1, N),
                r.Hold(0.5, J),
                PresetRunner.UntilGrounded(N)).state;
            bool boost = r.EventsOf(MoverEventType.BoostJump).Any();
            return Out(boost && Near(state.lastApexHeight, 4.86, 0.05), $"boost apex {state.lastApexHeight}");
        }

        // CHECKS.wallRide: entered && near(rideTime, 1.2, 0.02)
        static PresetOutcome WallRide(PresetRunner r)
        {
            r.PlaceOnWallA(12);
            RunResult res = r.Run(PresetRunner.Until(IsState(MoveState.WallRide), N), PresetRunner.Until(NotState(MoveState.WallRide), N));
            Snapshot start = res.snapshots[0], end = res.snapshots[1];
            double rideTime = Round((end.step - start.step) / r.config.loop.simHz);
            string reason = r.EventsOf(MoverEventType.WallRideEnd).Select(l => l.e.reason).FirstOrDefault();
            bool entered = start.state == MoveState.WallRide;
            return Out(entered && Near(rideTime, 1.2, 0.02), $"ride {rideTime} s, lost {Round(start.position.y - end.position.y)} m, end {reason}");
        }

        // CHECKS.wallJumpChain: wallJumps === 3
        static PresetOutcome WallJumpChain(PresetRunner r)
        {
            r.PlaceOnWallA(r.config.wallRide.minSpeed);
            var segs = new List<Segment> { PresetRunner.Until(IsState(MoveState.WallRide), N) };
            for (int i = 0; i < r.config.wallJump.maxPerAirtime + 1; i++)
            {
                segs.Add(r.Hold(0.01, N)); // wallJumpDelay
                segs.Add(PresetRunner.Frames(1, J));
                if (i < r.config.wallJump.maxPerAirtime) segs.Add(PresetRunner.Until(IsState(MoveState.WallRide), N));
            }
            r.Run(segs.ToArray());
            double[] speeds = r.EventsOf(MoverEventType.WallJump).Select(l => Round(l.e.speed)).ToArray();
            return Out(speeds.Length == 3, "speeds " + string.Join(" ", speeds));
        }

        // CHECKS.jumpPad: launched && near(apex, target, 0.1)
        static PresetOutcome JumpPad(PresetRunner r)
        {
            r.Place("padApproach");
            Snapshot state = r.Run(PresetRunner.Until(s => s.velocity.y > 1, new ScriptInput { moveZ = 1 }), PresetRunner.UntilGrounded(N)).state;
            bool launched = r.EventsOf(MoverEventType.JumpPad).Any();
            return Out(launched && Near(state.lastApexHeight, r.config.jumpPad.apexHeight, 0.1), $"apex {state.lastApexHeight}");
        }

        // CHECKS.slamVsSlide: C 0.05 s before landing slides (no slam); 0.3 s before slams
        static PresetOutcome SlamVsSlide(PresetRunner r)
        {
            r.Place("lane");
            RunResult pass1 = r.Run(r.Hold(0.5, J), PresetRunner.UntilGrounded(N));
            int fallSteps = pass1.counts[pass1.counts.Count - 1];
            var results = new List<(bool slam, bool slide)>();
            foreach (double lead in new[] { 0.05, 0.3 }) // slamVsSlideLeads
            {
                r.Place("lane");
                var c = new ScriptInput { slide = true };
                r.Run(r.Hold(0.5, J), PresetRunner.Frames(fallSteps - r.S(lead), N), PresetRunner.Frames(1, c), PresetRunner.UntilGrounded(c), PresetRunner.Frames(2, c));
                results.Add((r.EventsOf(MoverEventType.Slam).Any(), r.EventsOf(MoverEventType.SlideStart).Any()));
            }
            bool pass = !results[0].slam && results[0].slide && results[1].slam;
            return Out(pass, $"0.05 s: slide {results[0].slide} slam {results[0].slam}; 0.3 s: slam {results[1].slam}");
        }

        // CHECKS.fullChain: completed (every move fired, nothing timed out)
        static PresetOutcome FullChain(PresetRunner r)
        {
            r.Place("corridorApproach");
            double straight = 0; // yaw 0: straight down the corridor (−z), parallel to the walls
            RunResult res = r.Run(
                r.Hold(0.5, new ScriptInput { moveZ = 1 }),
                PresetRunner.Frames(1, new ScriptInput { moveZ = 1, slide = true }),
                PresetRunner.Frames(1, new ScriptInput { moveZ = 1, slide = true, jump = true }), // slide jump
                PresetRunner.Until(IsState(MoveState.WallRide), J), // wall ride
                r.Hold(0.2, N), // fullChainRideTime
                PresetRunner.Frames(1, J), // wall jump
                r.Hold(0.1, J), // fullChainWallJumpFly
                PresetRunner.Frames(1, new ScriptInput { dash = true, moveZ = 1, yaw = straight }), // dash down the corridor
                r.Hold(0.05, new ScriptInput { yaw = straight }),
                PresetRunner.Frames(1, new ScriptInput { jump = true, yaw = straight }), // dash jump
                r.Hold(0.15, new ScriptInput { jump = true, yaw = straight }), // fullChainSlamDelay
                PresetRunner.Frames(1, new ScriptInput { slide = true, yaw = straight }), // slam
                PresetRunner.UntilGrounded(new ScriptInput { yaw = straight }),
                PresetRunner.Frames(1, new ScriptInput { jump = true, yaw = straight }), // boost jump
                r.Hold(0.5, new ScriptInput { jump = true, yaw = straight }),
                PresetRunner.UntilGrounded(new ScriptInput { yaw = straight }));
            double? First(MoverEventType t, Func<MoverEvent, bool> when = null)
            {
                foreach (LoggedEvent l in r.log)
                    if (l.e.type == t && (when == null || when(l.e))) return Round(t == MoverEventType.Jump ? l.e.speedAfter : l.e.speed);
                return null;
            }
            var chain = new (string move, double? speed)[]
            {
                ("slide", First(MoverEventType.SlideStart)),
                ("slide jump", First(MoverEventType.Jump, e => e.kind == JumpKind.Slide)),
                ("wall ride", First(MoverEventType.WallRideStart)),
                ("wall jump", First(MoverEventType.WallJump)),
                ("dash", First(MoverEventType.Dash)),
                ("dash jump", First(MoverEventType.DashJump)),
                ("slam", First(MoverEventType.Slam) != null ? 0 : (double?)null),
                ("boost jump", First(MoverEventType.BoostJump)),
            };
            bool completed = !res.timedOut && chain.All(c => c.speed.HasValue);
            return Out(completed, string.Join(" ", chain.Select(c => c.speed.HasValue ? c.speed.Value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) : c.move + ":MISSING")));
        }

        // --- Spike scenarios (not web CHECKS) ---------------------------------------------------------------------

        /// <summary>scripts.js tunnel (no web check): release C under the 1.2 m roof; the capsule must stay crouched.</summary>
        static PresetOutcome Tunnel(PresetRunner r)
        {
            r.Place("tunnelApproach");
            double insideZ = TrainingMap.TestSpots["tunnelInside"].z;
            RunResult res = r.Run(PresetRunner.Until(s => s.position.z <= insideZ, new ScriptInput { slide = true }), PresetRunner.Frames(1, N));
            Snapshot after = res.snapshots[1];
            return Out(after.crouched && Near(after.position.y, 0, 0.01), $"crouched {after.crouched}, y {after.position.y}, state {after.state}");
        }

        /// <summary>Run up the first 20° ramp, slide down the second: the slope boost must add speed (movementSim: > floor + 3).</summary>
        static PresetOutcome RampSlide(PresetRunner r)
        {
            r.Place("ramps");
            var run = new ScriptInput { moveZ = 1 };
            RunResult res = r.Run(
                PresetRunner.Until(s => s.position.z <= -10, run),
                PresetRunner.Until(s => s.position.z <= -18, new ScriptInput { moveZ = 1, slide = true }));
            Snapshot bottom = res.snapshots[1];
            double floorPlus3 = r.config.slide.floorSpeed + 3;
            return Out(!res.timedOut && bottom.speed > floorPlus3, $"bottom speed {bottom.speed} m/s (> {floorPlus3}), state {bottom.state}, y {bottom.position.y}");
        }

        /// <summary>Jump onto the L piece's slab, run into its wall: stay grounded on the slab, touch the wall, no drift.</summary>
        static PresetOutcome MeshPiece(PresetRunner r)
        {
            r.Place("meshPiece");
            r.Run(r.Hold(0.6, new ScriptInput { moveZ = 1, jump = true }), PresetRunner.Until(_ => false, new ScriptInput { moveZ = 1 }, r.S(1.5)));
            Snapshot end = r.State();
            double expectX = 29.5 - r.config.body.capsuleRadius;
            bool pass = end.grounded && end.touchingWall && Near(end.position.y, 0.5, 0.01) && Near(end.position.x, expectX, 0.01);
            return Out(pass, $"pos ({end.position.x}, {end.position.y}, {end.position.z}) grounded {end.grounded} wall {end.touchingWall}");
        }

        /// <summary>
        /// Stand on a Grinder cell while the core reshapes from flat to 'stairs' (1.5 s tween): the cell carries the body
        /// up to its new top (n = 6: cell (3, 3) quantizes to 3 m) and leaves it grounded there. Untested against the web.
        /// </summary>
        static PresetOutcome Grinder(PresetRunner r)
        {
            if (r.core == null) return Out(false, "no core in this world");
            r.Place("grinderCell");
            CoreSpec spec = r.core.spec;
            r.core.Reshape(CoreGrid.PresetHeights("stairs", spec.cells, r.config.core.maxHeight, r.config.core.heightStep, new Mulberry32(1)), r.config.endless.reshapeTime, 1 / r.config.loop.simHz);
            double minY = double.PositiveInfinity;
            r.Run(PresetRunner.Until(s => { minY = Math.Min(minY, s.position.y); return false; }, N, r.S(r.config.endless.reshapeTime + 0.5)));
            Snapshot end = r.State();
            bool pass = end.grounded && Near(end.position.y, 3, 1e-9) && minY > -0.01;
            return Out(pass, $"end y {end.position.y} grounded {end.grounded}, lowest y {minY}, state {end.state}");
        }

        /// <summary>
        /// From the top of the 6 m tower (x 16..20, z −20..−16), hook grapple node 5 (12, 22, −12) overhead, swing out
        /// over the floor with the pump, let go with a jump on the way up, land. The rope (~17 m) keeps the arc above
        /// the floor, so the swing never scrapes the ground.
        /// </summary>
        static PresetOutcome Swing(PresetRunner r)
        {
            r.Place("towerTop");
            Spot at = TrainingMap.TestSpots["towerTop"];
            Vec3 node = r.map.grappleNodes[4];
            double dy = node.y - (at.y + r.config.body.eyeHeight);
            double dh = JsMath.Hypot(node.x - at.x, node.z - at.z);
            double pitch = JsMath.Atan2(dy, dh);
            RunResult res = r.Run(
                PresetRunner.Frames(1, new ScriptInput { grapple = true, pitch = pitch }),
                PresetRunner.Until(s => r.mover.rope.phase == RopePhase.Attached, new ScriptInput { grapple = true }),
                r.Hold(0.8, new ScriptInput { grapple = true, moveZ = 1 }),
                PresetRunner.Until(s => s.velocity.y > 0 && !s.grounded, new ScriptInput { grapple = true, moveZ = 1 }),
                PresetRunner.Frames(1, new ScriptInput { grapple = true, jump = true }),
                PresetRunner.UntilGrounded(N));
            bool attached = r.EventsOf(MoverEventType.RopeAttach).Any();
            bool node5 = r.EventsOf(MoverEventType.RopeAttach).Any(l => l.e.node);
            string release = r.EventsOf(MoverEventType.RopeRelease).Select(l => l.e.reason).FirstOrDefault();
            Snapshot end = res.state;
            return Out(attached && node5 && release == "jump" && !res.timedOut,
                $"attached {attached} (node {node5}), release {release}, max rope speed {Round(r.mover.rope.maxSpeed)}, landed ({end.position.x}, {end.position.y}, {end.position.z})");
        }
    }
}
