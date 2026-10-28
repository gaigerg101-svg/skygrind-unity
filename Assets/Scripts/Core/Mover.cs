// Port of skygrind/src/movement/mover.js: the shared movement state machine (SPEC §3, §6). Driven only by
// InputFrames, collides only through an ICollisionWorld (the three.js Octree calls mover.js makes). No scene access.
// Same step order, whole-step timers and transitions as the JS, verified bit for bit against the web's own runs
// (PortExport trajectories). The perk hooks are ported (set by the owner; neutral by default, and untested against the
// web game: the exported runs use the defaults). Not ported: the LOW GRAVITY mutator factor (1 by default),
// impulse() (rocket blasts), and the ARCLINE enemy pull (NIGHT21: the zip is ported, aimHook / zipAnchor).
using System;

namespace Skygrind.Core
{
    public struct Button
    {
        public bool held;
        public bool pressed; // a latched key-down edge
    }

    /// <summary>The anchor the owner picked for a grapple press this step (node 0 = a surface).</summary>
    public struct GrappleTarget
    {
        public Vec3 point;
        public int node;
        public bool zip; // NIGHT21 (the ARCLINE in Survival; arcline.js): a big enemy hooked, the rope reels you in (set by the aim hook only)
    }

    /// <summary>A neutral InputFrame (SPEC §6).</summary>
    public struct InputFrame
    {
        public double moveX; // −1..1, + is right
        public double moveZ; // −1..1, + is forward
        public double yaw; // rad, 0 looks toward −z
        public double pitch; // rad
        public Button jump;
        public Button dash;
        public Button slide;
        public Button grapple;
        public Button reel;
        public GrappleTarget? grappleTarget;
        public Button sprint; // NIGHT19 (port-only, design.stamina.n19.ownKey): the sprint's own key (L3); read only with the key's rule on
    }

    public enum MoverEventType
    {
        Jump,
        DoubleJump,
        DashJump,
        BoostJump,
        WallJump,
        Dash,
        DashFail,
        Land,
        Slam,
        SlideStart,
        SlideEnd,
        WallRideStart,
        WallRideEnd,
        JumpPad,
        RopeFire,
        RopeAttach,
        RopeRelease,
        SlideCharged, // NIGHT16 U3-29.4 (port-only, design.move.slideCharge): the slide boost's charge came back
        StaminaFull, // NIGHT17 U3-30.4 (port-only, design.stamina.bar): the bar refilled to full
        SprintStart, // NIGHT17 U3-30.4 (port-only, design.stamina.sprint): a sprint began
        SprintEnd, // NIGHT17 U3-30.4 (port-only, design.stamina.sprint): a sprint ended (key up, empty, standing still, a boost)
        BoostPartial, // NIGHT17 U3-30.4 (port-only, design.stamina.boost): a boost under its full cost (fraction, pips = the bar after)
    }

    /// <summary>A mover event with the web's payload fields (mover.js _emit), named as in the JS.</summary>
    public struct MoverEvent
    {
        public MoverEventType type;
        public double speed; // dash, dashJump, slideStart, doubleJump, boostJump, wallRideStart/End, wallJump, ropeRelease, jumpPad
        public double fallSpeed; // land
        public JumpKind kind; // jump: Ground (incl. coyote) or Slide
        public bool coyote; // jump
        public bool buffered; // jump
        public double speedBefore; // jump
        public double speedAfter; // jump
        public double entrySpeed; // dash
        public MoveState from; // dash
        public double pips; // dash, dashJump
        public Vec3 position; // slam
        public double fallDist; // slam
        public string reason; // wallRideEnd, ropeRelease
        public bool attached; // ropeRelease
        public int wallJumpsLeft; // wallJump
        public Vec3 point; // ropeFire, ropeAttach: x, y, z
        public double distance; // ropeFire
        public double length; // ropeAttach
        public bool node; // ropeFire, ropeAttach
        public bool zip; // ropeFire, ropeAttach (always false here: no enemies)
        public double fraction; // NIGHT17 U3-30.4 boostPartial: the boost's share of a full boost (port-only)
    }

    public enum RopePhase
    {
        None,
        Flying,
        Attached,
    }

    public sealed class Rope
    {
        public RopePhase phase = RopePhase.None;
        public Vec3 anchor;
        public Vec3 hook;
        public Vec3 from;
        public double L;
        public int hookSteps;
        public int hookTotal = 1;
        public int pullSteps;
        public int blindSteps;
        public int cooldownSteps;
        public int rehookSteps;
        public int lastNode;
        public int node;
        public bool taut;
        public double stretch;
        public int attachedSteps;
        public double maxSpeed;
        public int groundTouches;
        public bool zip; // NIGHT21 (mover.js r.zip): a big enemy is hooked, reel hard and let go on arrival
    }

    public sealed class Mover
    {
        const int MaxWallHits = 8; // valid wall contacts remembered per step (corner checks)

        readonly MovementConfig config;
        ICollisionWorld world;

        public event Action<MoverEvent> Events;

        // Read-only state for the camera, HUD and debug.
        public Vec3 position; // feet
        public Vec3 velocity;
        public MoveState state = MoveState.Air;
        public bool grounded;
        public bool crouched;
        public double pips;
        public double regenMult = 1; // × stamina regen (the style rank reward); set by the owner
        public double walkMult = 1; // × ground walk speed (the zoom); set by the owner
        // Perks (SURVIVAL S3), set by the owner.
        public int bonusPips; // VIGOR: extra stamina pips
        public double bonusRideTime; // GRIP: s added to the wall-ride time
        public int bonusWallJumps; // GRIP: wall jumps added per airtime
        public double slideSpeedMult = 1, slideFrictionMult = 1, slideJumpCapMult = 1; // SLIPSTREAM: slideMods
        public double groundFrictionMult = 1; // NIGHT26 surfaces: x ground friction (ice 0.25, mud 2.0), set by the sim from the zone under the player; 1 = the web's mover bit for bit
        public double slideCharge = 1; // NIGHT16 U3-29.4 (design.move.slideCharge, builder M): the slide boost's charge, 1 = ready (the HUD reads it)
        public MoveBalance.State moveBalance; // NIGHT16 U3-29.4 (design.move): the run's balance rules and their clocks; default: every rule off (the web's mover)
        public Stamina.State stamina; // NIGHT17 U3-30.4 (design.stamina, builder M): the run's bar, its rules and clocks; default: every rule off (the web's pips)
        public Flow34.State flow34; // NIGHT34 M1 (design.n34.flow34, builder M): the run's flow rules and their clocks (Core/Flow34.cs); default: every rule off (the web's mover)
        public bool ropeEnabled;
        /// <summary>NIGHT21 (wiring, the ARCLINE in Survival; null elsewhere and in every golden): on a grapple press with no target on
        /// the frame, the owner picks what the hook bites (a node, a surface, a zip; a yank is done in the hook and returns null).</summary>
        public Func<InputFrame, GrappleTarget?> aimHook;
        /// <summary>NIGHT21 (wiring; mover.js getPoint): where the zip's enemy is now (a moving anchor).</summary>
        public Func<Vec3?> zipAnchor;
        public readonly Rope rope = new Rope();
        public int doubleJumpsLeft;
        public int wallJumpsLeft;
        public double lastApexHeight;
        public double lastDashDistance;
        public bool invulnerable;
        public bool touchingWall;
        public Vec3 wallNormal;
        public Vec3 groundNormal = new Vec3(0, 1, 0);
        public double gravityMultiplier = 1;

        // Collision scratch
        Capsule capsule;
        Capsule probe;
        Vec3 move;
        double preMoveSpeed;
        double impactSpeed;
        readonly Vec3[] wallHits = new Vec3[MaxWallHits];
        int wallHitCount;
        Vec3 probeNormal;
        Vec3 lastWallNormal;
        bool hasLastWall;
        Vec3 nearWallNormal;

        // Step scratch
        InputFrame input;
        Vec3 wish;
        double wishSpeed;
        Vec3 forward;

        // Timers (whole steps) + per-airtime bookkeeping
        int bufferSteps;
        int coyoteSteps;
        int groundedSteps;
        int windSteps; // dash-jump lighter gravity
        int boostSteps; // boost-jump window after a slam landing
        int slideBufferSteps; // C pressed just before a predicted landing
        bool jumpCutArmed;
        bool jumpAirtime; // this airtime began with a jump (apex hang eligible)
        bool airborne;
        double takeoffY;
        double airPeakY;

        // Dash + slide + slam
        int dashSteps;
        Vec3 dashDir;
        double dashSpeedNow;
        double dashSpeedBefore;
        Vec3 dashStart;
        bool dashFromSlide;
        Vec3 slideDir;
        double slamStartY;

        // Wall ride + wall jump
        int rideSteps;
        Vec3 rideNormal;
        Vec3 rideDir; // unit direction along the wall
        bool rideLockActive;
        Vec3 rideLockNormal;
        int wallJumpLockSteps;
        Vec3 wallJumpLockNormal;

        SwingState swingState;

        public Mover(MovementConfig config, ICollisionWorld world)
        {
            this.config = config;
            this.world = world;
            pips = config.dash.maxPips;
            doubleJumpsLeft = config.jump.doubleJumps;
            wallJumpsLeft = config.wallJump.maxPerAirtime;
            capsule.radius = config.body.capsuleRadius;
            probe.radius = config.body.capsuleRadius;
        }

        public MovementConfig Config => config;

        public ICollisionWorld World
        {
            get => world;
            set => world = value;
        }

        /// <summary>Horizontal speed (m/s).</summary>
        public double Speed => MovementMath.HorizontalSpeed(velocity);

        /// <summary>True during the dash-jump lighter-gravity window.</summary>
        public bool DashJumpActive => windSteps > 0;

        /// <summary>Stamina pips with perks (VIGOR).</summary>
        public double MaxPips => config.dash.maxPips + bonusPips;

        /// <summary>NIGHT17 U3-30.4: the stamina bar's size with perks (design.stamina.bar: 1, 2 with VIGOR; 0 with the rules off).</summary>
        public double StaminaMax => Stamina.Max(stamina.rules, bonusPips);

        /// <summary>NIGHT17 U3-30.4: is there stamina for a full boost (and its dash-jump)? The pips (rules off): pips >= pipsNeed, the
        /// web's test; the bar: the boost's whole cost (+ dashJumpExtra), so a link the bot planned for a full-speed, full-length
        /// boost never fires a partial one.</summary>
        public bool BoostReady(double pipsNeed, bool dashJump)
        {
            if (!stamina.rules.Any) return pips >= pipsNeed;
            if (pipsNeed <= 0) return true;
            return stamina.bar >= stamina.rules.cost + (dashJump ? stamina.rules.dashJumpExtra : 0);
        }

        /// <summary>Wall jumps per airtime with perks (GRIP).</summary>
        public int MaxWallJumps => config.wallJump.maxPerAirtime + bonusWallJumps;

        public Capsule Body => capsule;

        public int DashStepsLeft => dashSteps;

        public int RideStepsLeft => rideSteps;

        void Emit(MoverEventType type) => Events?.Invoke(new MoverEvent { type = type });

        void Emit(MoverEvent e) => Events?.Invoke(e);

        /// <summary>Teleport with full stamina and zero velocity. Lands on the next step.</summary>
        public void Reset(double x, double y, double z)
        {
            MovementConfig c = config;
            if (state == MoveState.Slide) Emit(MoverEventType.SlideEnd);
            if (state == MoveState.WallRide) Emit(new MoverEvent { type = MoverEventType.WallRideEnd, reason = "reset", speed = Speed });
            position = new Vec3(x, y, z);
            velocity = Vec3.Zero;
            state = MoveState.Air;
            grounded = false;
            crouched = false;
            invulnerable = false;
            pips = MaxPips;
            if (stamina.rules.Any) StaminaReset(); // NIGHT17 U3-30.4 (design.stamina): the bar full, the sprint off
            doubleJumpsLeft = c.jump.doubleJumps;
            wallJumpsLeft = MaxWallJumps;
            touchingWall = false;
            hasLastWall = false;
            bufferSteps = 0;
            coyoteSteps = 0;
            groundedSteps = 0;
            windSteps = 0;
            boostSteps = 0;
            slideBufferSteps = 0;
            jumpCutArmed = false;
            dashSteps = 0;
            rideSteps = 0;
            rideLockActive = false;
            wallJumpLockSteps = 0;
            if (flow34.rules.Any) // NIGHT34 flow34: the clocks off (the rules stay the run's)
            {
                flow34.wallCoyoteLeft = 0;
                flow34.carryArmed = false;
                flow34.lurchArmed = false;
                if (flow34.rules.slideBounce) flow34.sinceSlide = int.MaxValue / 2;
            }
            if (R35) // NIGHT35 wallride35: the clocks and the re-ride memory off (the rules stay the run's)
            {
                flow34.ride35.held = false;
                flow34.ride35.rideAge = 0;
                flow34.ride35.lateLeft = 0;
                flow34.ride35.hasLast = false;
            }
            if (m43.p.Any) // NIGHT43 chimney43 / ride43: the clocks off (the rules stay the run's)
            {
                m43.hasWall = false;
                m43.sinceRide = int.MaxValue / 2;
            }
            if (climb47.p.on) // NIGHT47 climb47: the clocks off (the rules stay the run's)
            {
                climb47.catchLeft = 0;
                climb47.pendingJumpOff = 0;
                climb47.sinceRelease = int.MaxValue / 2;
                climb47.wasHeld = false;
            }
            ClearRope();
            BeginAirtime(y, false);
            SyncCapsule();
        }

        /// <summary>NIGHT34 flow34: the run's flow rules (design.n34.flow34), their clocks and counts reset (the owner, at a run's start).
        /// Params.Neutral: every Mover branch that reads them is skipped (the web's mover).</summary>
        public void SetWall42(in Wall42.Params rules)
        {
            wall42 = rules;
            wall42NoIntent = wall42Landing = wall42NoBounce = wall42Capped = 0;
        }

        /// <summary>NIGHT42 wallfling42 (design.n42.wallfling42, builder M1; Core/Wall42.cs): the run's rules (the owner, at a run's start;
        /// Params.Neutral: every branch that reads them is skipped, NIGHT41's Mover) and the run's counts: air presses near a wall turned into the
        /// double jump for want of intent, kept for a predicted landing, slide bounces refused (too shallow), jump-offs whose out speed was capped.</summary>
        public Wall42.Params wall42;
        public int wall42NoIntent, wall42Landing, wall42NoBounce, wall42Capped;

        /// <summary>NIGHT42 wallfling42: does the slide head into the near wall less than bounceDot (a graze)? Counts the refused bounce on a press.</summary>
        bool Wall42ShallowSlide()
        {
            if (-(slideDir.x * nearWallNormal.x + slideDir.z * nearWallNormal.z) >= wall42.bounceDot) return false;
            if (input.jump.pressed) wall42NoBounce++;
            return true;
        }

        /// <summary>NIGHT42 wallfling42: may an air press near the wall with normal n (not riding it) be a wall jump? The stick must point into
        /// the wall, and no floor landing may be predicted within landSeconds (that press waits for the ground jump). Counts each refused press.</summary>
        bool Wall42Allows(Vec3 n, double gravity)
        {
            if (!Wall42.Intent(wish, wishSpeed, n, wall42.intentDot))
            {
                if (input.jump.pressed) wall42NoIntent++;
                return false;
            }
            if (velocity.y <= 0 && LandingWithin(wall42.landSeconds, gravity * PredictScale()))
            {
                if (input.jump.pressed) wall42Landing++;
                return false;
            }
            return true;
        }

        /// <summary>NIGHT43 chimney43 / ride43 (design.n43, builder M1; Core/Move43.cs): the run's rules, the last wall jump's wall, the clock since
        /// the last ride and the run's counts. Params.Neutral: every branch that reads them is skipped (NIGHT42's Mover).</summary>
        public Move43.State m43;

        /// <summary>NIGHT43 chimney43 / ride43: the run's rules, their clocks and counts reset (the owner, at a run's start).</summary>
        public void SetMove43(in Move43.Params rules) => m43 = new Move43.State { p = rules, sinceRide = int.MaxValue / 2 };

        /// <summary>NIGHT47 climb47 (design.n47.climb47, builder W; Core/Climb47.cs): the run's climb rules, their clocks and counts (saved whole by
        /// RunSnapshot.MoverState). Off (Params.Off) or no climbWall: every "NIGHT47 climb47" branch is skipped (NIGHT46's Mover bit for bit).</summary>
        public Climb47.State climb47;
        /// <summary>NIGHT47 climb47: the owner's CLIMB WALL test (wiring, not saved): the ridden panel's bottom (world y) when the wall with normal n at
        /// the feet p is a climb wall, else NaN. Null = no climb walls.</summary>
        public Func<Vec3, Vec3, double> climbWall;
        public void SetClimb47(in Climb47.Params rules) => climb47 = new Climb47.State { p = rules, sinceRelease = int.MaxValue / 2 };
        /// <summary>NIGHT47 climb47: the climb wall's bottom for normal n here (NaN: not one, or the rules off).</summary>
        double Climb47Bottom(Vec3 n) => climb47.p.on && climbWall != null ? climbWall(position, n) : double.NaN;
        /// <summary>NIGHT47 climb47: the owner's NEXT HOLD test (wiring, not saved): the world y the climb's next hold asks of the feet (the next
        /// panel's bottom, a ledge's top, the cap) from the climb wall with normal n at the feet p; NaN: none (setUp alone).</summary>
        public Func<Vec3, Vec3, double> climbNext;

        /// <summary>NIGHT47 climb47 SET UP: the up speed off a climb wall: at least setUp, and with a next hold, the speed whose rise reaches that
        /// hold's height + guideOver over the feet (clamped guideMin..guideMax): every jump of the climb rises to its next hold, never short, never
        /// over it (measured round 1: a fixed up speed flew a casual hand over a 2.2 m step's panel top). Replaces a lower or a higher up speed.</summary>
        double Climb47Up(Vec3 n, double up)
        {
            ref Climb47.State k = ref climb47;
            double want = JsMath.Max(up, k.p.setUp);
            double ny = climbNext != null ? climbNext(position, n) : double.NaN;
            if (!double.IsNaN(ny) && k.p.guideMax > 0)
            {
                double g = MovementMath.GravityFor(config.jump.height, config.jump.apexTime), h = JsMath.Max(0.2, ny + k.p.guideOver - position.y);
                want = JsMath.Min(k.p.guideMax, JsMath.Max(k.p.guideMin, Math.Sqrt(2 * g * h)));
                k.guided++;
            }
            if (want != up) k.setUps++;
            return want;
        }

        /// <summary>NIGHT43 chimney43: a wall jump off the wall with normal n (here) refills the count first when it is a different wall than the
        /// last wall jump's this airtime. Counts a refill that filled a short count, and a same-wall press.</summary>
        void Chimney43Refill(Vec3 n, bool ride)
        {
            if (!Move43.NewWall(m43, n, position))
            {
                if (!ride) m43.sameWall++;
                return;
            }
            if (wallJumpsLeft >= MaxWallJumps) return;
            wallJumpsLeft = MaxWallJumps;
            if (ride) m43.rideRefills++;
            else m43.refills++;
        }

        /// <summary>NIGHT43 chimney43: the wall a wall jump / jump-off / slide bounce just left (normal n) is the last wall.</summary>
        void Chimney43Mark(Vec3 n)
        {
            m43.hasWall = true;
            m43.wallN = n;
            m43.wallAt = position;
        }

        public void SetFlow34(in Flow34.Params rules) => flow34 = new Flow34.State { rules = rules, sinceSlide = rules.slideBounce ? int.MaxValue / 2 : 0, ride35 = flow34.ride35 /* NIGHT35 wallride35: its own setter */ };

        /// <summary>NIGHT35 wallride35: the run's Lucio ride rules (design.n35.wallride35, Core/Ride35.cs), its clocks and counts reset (the owner,
        /// at a run's start). Params.Neutral: every Mover branch that reads them is skipped (the web's ride).</summary>
        public void SetRide35(in Ride35.Params rules) => flow34.ride35 = new Ride35.State { rules = rules, grip = flow34.ride35.grip /* NIGHT36 wallgrip36: its own setter */, r38 = flow34.ride35.r38, noRide = flow34.ride35.noRide /* NIGHT38 ride38 / noride: their own setters */, r40 = flow34.ride35.r40 /* NIGHT40 ride40: its own setter */ };

        /// <summary>NIGHT38 ride38 (inside wallride35: read only on a HOLD ride while the Lucio ride is on): the run's rules (design.n38.ride38,
        /// Core/Ride38.cs), set by the owner at a run's start. Params.Neutral: NIGHT37's ride.</summary>
        public void SetRide38(in Ride38.Params rules)
        {
            flow34.ride35.r38 = rules;
            flow34.ride35.r38Rising = false;
            flow34.ride35.r38TopHolds = 0;
            flow34.ride35.r38NoRideBlocks = 0;
        }

        /// <summary>NIGHT38 noride (inside wallride35): the level's walls no ride starts on or turns onto (Ride38.NoRideFrom), set by the owner
        /// at a run's start; null: none (every check skipped).</summary>
        public void SetNoRide38(Ride38.Box[] boxes) => flow34.ride35.noRide = boxes != null && boxes.Length > 0 ? boxes : null;

        /// <summary>NIGHT40 ride40 (inside wallride35): every ride holds (Core/Ride40.cs); the owner sets the hold rule's numbers with
        /// SetRide38(Ride40.Hold(..)) at the same run start. Params.Neutral: NIGHT39's ride.</summary>
        public void SetRide40(in Ride40.Params rules)
        {
            ref Ride35.State r = ref flow34.ride35;
            r.r40 = rules;
            r.r40LockLeft = 0;
            r.r40Low = r.r40Slow = r.r40NoIntent = r.r40Short = r.r40Locked = 0;
        }

        /// <summary>NIGHT38 ride38: is the current ride a hold ride under ride38 (the height kept, the pitch drifts it)? NIGHT40 ride40: every ride.</summary>
        public bool HoldRide38 => flow34.ride35.rules.on && flow34.ride35.r38.on && state == MoveState.WallRide && (flow34.ride35.held || flow34.ride35.r40.on);

        /// <summary>NIGHT36 wallgrip36 (inside wallride35: read only while the Lucio ride is on): the run's grip rules (design.n36.wallgrip36,
        /// Core/Grip36.cs), set by the owner at a run's start. Params.Neutral: wallride35's ride.</summary>
        public void SetGrip36(in Grip36.Params rules)
        {
            flow34.ride35.grip = rules;
            flow34.ride35.gripAge = 0;
            flow34.ride35.gripClamps = 0;
        }

        /// <summary>NIGHT35 wallride35: is the Lucio ride on for this run?</summary>
        bool R35 => flow34.ride35.rules.on;

        /// <summary>NIGHT35 wallride35: is the current ride a hold ride (entered with jump held)?</summary>
        public bool HoldRide35 => flow34.ride35.rules.on && state == MoveState.WallRide && flow34.ride35.held;

        /// <summary>NIGHT36 wallgrip36: is the grip on for this run (inside wallride35)? The camera's eased ride roll reads it.</summary>
        public bool Grip36On => flow34.ride35.rules.on && flow34.ride35.grip.on;

        /// <summary>NIGHT36 wallgrip36: the run's attaches whose rise was clamped (the reel reads it).</summary>
        public int Grip36Clamps => flow34.ride35.gripClamps;

        /// <summary>
        /// mover.js impulse (NIGHT7: the web API the spike left out; the punch / melee lunge use it): an outside push
        /// adds to the velocity; an upward push lifts you off the ground or out of a slide into the air (no apex hang:
        /// it is not a jump). A dash keeps its own velocity.
        /// </summary>
        public void Impulse(double dx, double dy, double dz)
        {
            if (state == MoveState.Dash) return;
            velocity.x += dx;
            velocity.y += dy;
            velocity.z += dz;
            if (dy > 0 && (state == MoveState.Ground || state == MoveState.Slide))
            {
                if (state == MoveState.Slide) Emit(MoverEventType.SlideEnd);
                grounded = false;
                state = MoveState.Air;
                coyoteSteps = 0;
                jumpCutArmed = false;
                BeginAirtime(position.y, false);
            }
        }

        /// <summary>NIGHT16 U3-29.4: the run's balance rules (design.move), their clocks reset, the charge full (the owner, at a run's start).</summary>
        public void SetMoveBalance(in MoveBalance.Params rules)
        {
            moveBalance = new MoveBalance.State { rules = rules, wasAirborne = airborne, slideBase = config.slide.floorSpeed * slideSpeedMult };
            slideCharge = 1;
        }

        /// <summary>NIGHT17 U3-30.4: the run's stamina rules (design.stamina), the bar full, the sprint off (the owner, at a run's start).</summary>
        public void SetStamina(in Stamina.Params rules)
        {
            stamina = new Stamina.State { rules = rules };
            if (rules.Any) StaminaReset();
        }

        /// <summary>mover.js restoreStamina (NIGHT7, as Impulse): refill every pip (a parry, SPEC 1.5).</summary>
        public void RestoreStamina()
        {
            pips = MaxPips;
            if (stamina.rules.Any) stamina.bar = StaminaMax; // NIGHT17 U3-30.4: a full bar
        }

        /// <summary>NIGHT17 U3-30.4 (design.stamina.rewards): a kill's or a parry's refill (the owner; the slam's is the Mover's own).</summary>
        public void StaminaReward(double amount)
        {
            if (stamina.rules.Any && stamina.rules.rewards) stamina.bar = Stamina.Add(stamina.bar, amount, StaminaMax);
        }

        /// <summary>NIGHT42 boost42 (Core/Boost42.cs; called only with the row on): a THUNDERCLAP boost is a launch: letting go of jump after it
        /// no longer halves the climb (the jump's variable-height cut, jump.releaseMult, disarmed as a wall jump or a pad does).</summary>
        public void Boost42Launch() => jumpCutArmed = false;

        /// <summary>
        /// Launch (jump pads). Ends a dash, slide or ride; no apex hang (not a jump). Aimed pads also set the
        /// horizontal velocity.
        /// </summary>
        public void Launch(double vy, double? vx = null, double? vz = null)
        {
            if (rope.phase != RopePhase.None) ReleaseRope("launch", false);
            if (state == MoveState.Dash) EndDash(input);
            if (state == MoveState.Slide) Emit(MoverEventType.SlideEnd);
            if (state == MoveState.WallRide) EndRide("launch");
            velocity.y = vy;
            if (vx.HasValue)
            {
                velocity.x = vx.Value;
                velocity.z = vz.Value;
            }
            grounded = false;
            state = MoveState.Air;
            coyoteSteps = 0;
            boostSteps = 0;
            windSteps = 0;
            jumpCutArmed = false;
            BeginAirtime(position.y, false);
            Emit(new MoverEvent { type = MoverEventType.JumpPad, speed = vy });
        }

        /// <summary>Advance one fixed step.</summary>
        public void Step(in InputFrame frame, double dt)
        {
            MovementConfig c = config;
            double gravity = MovementMath.GravityFor(c.jump.height, c.jump.apexTime);
            input = frame;
            if (moveBalance.rules.Any) BalanceBeginStep(); // NIGHT16 U3-29.4 (design.move): the charge's clock, the landing

            // 1. Input: wish direction (yaw only, so pitch never tilts movement) and the jump buffer.
            wish = MovementMath.WishDirection(input.moveX, input.moveZ, input.yaw);
            forward = MovementMath.WishDirection(0, 1, input.yaw);
            wishSpeed = c.ground.walkSpeed * JsMath.Min(1, JsMath.Hypot(input.moveX, input.moveZ));
            if (input.jump.pressed) bufferSteps = MovementMath.StepsFor(c.jump.bufferTime, dt);
            if (climb47.p.on) // NIGHT47 climb47: the clock since the jump was let go in the air (the buffer onto a climb wall)
            {
                if (climb47.wasHeld && !input.jump.held && state == MoveState.Air) climb47.sinceRelease = 0;
                else if (climb47.sinceRelease < int.MaxValue / 2) climb47.sinceRelease++;
                climb47.wasHeld = input.jump.held;
            }

            // 2. Stamina
            pips = MovementMath.RegenPips(pips, c.dash.regenPerSec * regenMult, MaxPips, dt, state == MoveState.Dash);
            if (stamina.rules.Any) StaminaStep(dt); // NIGHT17 U3-30.4 (design.stamina): the bar's refill, the sprint's start / end / drain

            // 2b. ARCLINE: fire the hook, fly it, attach; let go (key up, jump, dash, slam, a blocked rope).
            StepRope(dt);
            if (R35) Ride35Actions(); // NIGHT35 wallride35: the release jump-off, the crouch drop, the late jump (before the web's actions)

            // 3. Actions: dash, jump, slam (C in the air), slide (C on the ground). Slam needs to have been airborne
            // when the step began, so C + Space on the same ground step never slams.
            MoveState startState = state;
            bool airborneAtStart = startState == MoveState.Air || startState == MoveState.WallRide || startState == MoveState.Swing;
            if (input.dash.pressed) TryDash(dt);
            if (bufferSteps > 0 && state != MoveState.Swing) TryJump(dt, gravity);
            if (input.slide.pressed && airborneAtStart && (state == MoveState.Air || state == MoveState.WallRide)) TrySlam(dt, gravity);
            if (state == MoveState.Ground && input.slide.held) StartSlide();

            // 4. State logic (the swing moves and collides in its own sub-steps: then only the timers and transitions run)
            if (state == MoveState.Swing)
            {
                StepSwing(dt);
                TickTimers();
                AfterMove(dt);
                return;
            }
            switch (state)
            {
                case MoveState.Ground: StepGround(dt); break;
                case MoveState.Air: StepAir(dt); break;
                case MoveState.Slide: StepSlide(dt); break;
                case MoveState.Dash: StepDash(); break;
                case MoveState.WallRide: StepWallRide(dt); break;
                case MoveState.Slam: StepSlam(); break;
            }

            // 5–6. Gravity (one multiplier, exact for constant acceleration), then the safety cap.
            var flags = new GravityFlags { jumpHeld = input.jump.held, jumpAirtime = jumpAirtime, windActive = windSteps > 0 };
            gravityMultiplier = MovementMath.GravityScale(state, velocity.y, flags, c);
            if (flow34.rules.ridePhase && state == MoveState.WallRide) // NIGHT34 flow34 ridePhase: the grip, then the ramp back to the ride's gravityMult
                gravityMultiplier = Flow34.RideGravity(MovementMath.StepsFor(c.wallRide.maxTime + bonusRideTime, 1 / c.loop.simHz) - rideSteps, flow34.rules, gravityMultiplier, velocity.y);
            if (R35 && state == MoveState.WallRide && (flow34.ride35.held || flow34.ride35.r40.on)) // NIGHT35 wallride35: a hold ride keeps its height (a rising entry brakes, then a slow sink; NIGHT40 ride40: every ride, ride38's hold below replaces the sink)
            {
                if (velocity.y > 0) gravityMultiplier = flow34.ride35.rules.riseGravityMult;
                else
                {
                    gravityMultiplier = 0;
                    velocity.y = -flow34.ride35.rules.sinkSpeed;
                }
            }
            if (R35 && state == MoveState.WallRide && flow34.ride35.grip.on) // NIGHT36 wallgrip36 (inside wallride35): a wall never lifts you: brake, grip, then a slow sink
            {
                ref Ride35.State r = ref flow34.ride35;
                r.gripAge++;
                gravityMultiplier = Grip36.Gravity(r.grip, r.gripAge, r.held, r.rules.sinkSpeed, dt, ref velocity.y);
            }
            if (R35 && state == MoveState.WallRide && (flow34.ride35.held || flow34.ride35.r40.on) && flow34.ride35.r38.on) // NIGHT38 ride38 (inside wallride35): a hold ride keeps its height; the look pitch drifts it (NIGHT40 ride40: every ride)
                Ride38Hold(dt);
            VerticalStepResult vertical = MovementMath.VerticalStep(velocity.y, gravity * gravityMultiplier, dt);
            velocity.y = vertical.vy;
            velocity = MovementMath.ClampHorizontal(velocity, c.speed.maxHorizontal);

            // 7. Move + collide (remember the speed before walls clip it: wall-ride entry keeps it)
            preMoveSpeed = Speed;
            bool wasGrounded = grounded;
            move = new Vec3(velocity.x * dt, vertical.dy, velocity.z * dt);
            MoveAndCollide();
            // Ground snap (SPEC 2.0): running or sliding down a ramp or off a tiny step stays grounded.
            if (!grounded && wasGrounded && (state == MoveState.Ground || state == MoveState.Slide) && velocity.y <= 0) SnapToGround();
            if (state == MoveState.Dash && !grounded) grounded = ProbeFloor(c.collision.groundProbe);

            TickTimers();

            // 8. Transitions
            AfterMove(dt);
        }

        /// <summary>Timers tick before transitions so a freshly set timer lasts its full length.</summary>
        void TickTimers()
        {
            if (bufferSteps > 0) bufferSteps--;
            if (coyoteSteps > 0) coyoteSteps--;
            if (flow34.rules.wallCoyote && flow34.wallCoyoteLeft > 0) flow34.wallCoyoteLeft--; // NIGHT34 flow34 wallCoyote
            if (flow34.rules.lurch && flow34.lurchArmed) flow34.sinceJump++; // NIGHT34 flow34 lurch: the window's clock
            if (flow34.rules.slideBounce && flow34.sinceSlide < int.MaxValue / 2) flow34.sinceSlide++; // NIGHT34 flow34 slideBounce34: since the slide start
            if (R35 && flow34.ride35.lateLeft > 0) flow34.ride35.lateLeft--; // NIGHT35 wallride35: the late jump window
            if (R35 && flow34.ride35.r40LockLeft > 0) flow34.ride35.r40LockLeft--; // NIGHT40 ride40: the same-wall lockout
            if (m43.p.Any && m43.sinceRide < int.MaxValue / 2) m43.sinceRide++; // NIGHT43 ride43 (d) / chimney43's chain: the clock since a wall was left
            if (windSteps > 0) windSteps--;
            if (boostSteps > 0) boostSteps--;
            if (wallJumpLockSteps > 0) wallJumpLockSteps--;
            if (slideBufferSteps > 0)
            {
                slideBufferSteps--;
                // The predicted landing didn't happen and C is still held: slam after all (SPEC 1.3).
                if (slideBufferSteps == 0 && !grounded && state == MoveState.Air && input.slide.held) StartSlam();
            }
        }

        // --- ARCLINE ------------------------------------------------------------------------------------------------

        /// <summary>The rope's body point: the chest (the feet + ropeHeight).</summary>
        public Vec3 RopePoint => new Vec3(position.x, position.y + config.arcline.ropeHeight, position.z);

        void ClearRope()
        {
            rope.phase = RopePhase.None;
            rope.hookSteps = 0;
            rope.pullSteps = 0;
            rope.blindSteps = 0;
            rope.zip = false;
            zipAnchor = null;
            rope.taut = false;
            rope.stretch = 0;
            if (state == MoveState.Swing) state = MoveState.Air;
        }

        /// <summary>
        /// Let go of the rope: all velocity kept (× risingBoost while rising; a jump release adds a small boost too),
        /// then the cooldown; the same node can't be hooked again for a moment.
        /// </summary>
        void ReleaseRope(string reason, bool keepVelocity = true)
        {
            ArclineConfig a = config.arcline;
            if (rope.phase == RopePhase.None) return;
            bool wasAttached = rope.phase == RopePhase.Attached;
            if (wasAttached && keepVelocity) velocity = SwingMath.ReleaseVelocity(velocity, a.risingBoost, reason == "jump" ? a.jumpUp : 0);
            if (flow34.rules.zipCarry && flow34.carryArmed) // NIGHT34 flow34 zipCarry: the pull's release adds back the momentum it erased
            {
                if (wasAttached && keepVelocity)
                {
                    velocity = Flow34.Carry(velocity, flow34.carryVel, flow34.rules.carryMult);
                    flow34.carries++;
                }
                flow34.carryArmed = false;
            }
            rope.cooldownSteps = MovementMath.StepsFor(a.cooldown, 1 / config.loop.simHz);
            rope.lastNode = rope.node;
            rope.rehookSteps = MovementMath.StepsFor(a.rehook, 1 / config.loop.simHz);
            double speed = velocity.Length;
            ClearRope();
            if (wasAttached)
            {
                if (!grounded) BeginAirtime(position.y, reason == "jump");
                jumpAirtime = reason == "jump";
            }
            Emit(new MoverEvent { type = MoverEventType.RopeRelease, reason = reason, speed = speed, attached = wasAttached });
        }

        /// <summary>Hook firing, flight, attaching, the release rules and the line-of-sight break (one fixed step).</summary>
        void StepRope(double dt)
        {
            ArclineConfig a = config.arcline;
            Rope r = rope;
            if (r.cooldownSteps > 0) r.cooldownSteps--;
            if (r.rehookSteps > 0 && --r.rehookSteps == 0) r.lastNode = 0;
            if (!ropeEnabled)
            {
                if (r.phase != RopePhase.None) ReleaseRope("off", false);
                return;
            }
            if (r.phase == RopePhase.None)
            {
                if (!input.grapple.pressed || r.cooldownSteps > 0) return;
                if (!input.grappleTarget.HasValue && aimHook != null) input.grappleTarget = aimHook(input); // NIGHT21: the ARCLINE's pick (arcline.js step)
                if (!input.grappleTarget.HasValue) return;
                GrappleTarget target = input.grappleTarget.Value;
                if (target.node != 0 && target.node == r.lastNode) return; // the same node, too soon
                r.from = RopePoint;
                r.anchor = target.point;
                r.hook = r.from;
                double d = Vec3.DistanceTo(r.from, r.anchor);
                r.hookTotal = Math.Max(1, MovementMath.StepsFor(SwingMath.HookTime(d, a), dt));
                r.hookSteps = r.hookTotal;
                r.node = target.node;
                r.zip = target.zip;
                r.phase = RopePhase.Flying;
                Emit(new MoverEvent { type = MoverEventType.RopeFire, point = target.point, distance = d, node = target.node != 0, zip = r.zip });
                return;
            }
            if (!input.grapple.held)
            {
                ReleaseRope("letGo");
                return;
            }
            if (r.zip && zipAnchor != null) r.anchor = zipAnchor() ?? r.anchor; // a moving anchor (an enemy you zip to)
            if (r.phase == RopePhase.Flying)
            {
                r.hookSteps--;
                double k = 1 - (double)r.hookSteps / r.hookTotal;
                r.from = RopePoint;
                r.hook = new Vec3(r.from.x + (r.anchor.x - r.from.x) * k, r.from.y + (r.anchor.y - r.from.y) * k, r.from.z + (r.anchor.z - r.from.z) * k);
                if (r.hookSteps > 0) return;
                // The hook bites: the rope is as long as the gap (never past maxLen), a short pull-in yanks you toward it.
                double gap = Vec3.DistanceTo(RopePoint, r.anchor);
                r.L = JsMath.Max(a.minLen, JsMath.Min(gap, a.maxLen));
                r.pullSteps = MovementMath.StepsFor(a.pullTime, dt);
                r.blindSteps = 0;
                r.attachedSteps = 0;
                r.maxSpeed = 0;
                r.groundTouches = 0;
                r.phase = RopePhase.Attached;
                r.hook = r.anchor;
                // Hooked from the ground: a small lift so the swing gets under way.
                if (grounded || state == MoveState.Ground || state == MoveState.Slide) velocity.y = JsMath.Max(velocity.y, a.groundLift);
                EnterSwing();
                Emit(new MoverEvent { type = MoverEventType.RopeAttach, length = r.L, node = r.node != 0, point = r.anchor, zip = r.zip });
                return;
            }
            // Attached: jump lets go with a small boost; dash lets go and dashes; C in the air lets go and slams.
            r.attachedSteps++;
            if (input.jump.pressed && !grounded)
            {
                ReleaseRope("jump");
                bufferSteps = 0; // the jump press was the release
                return;
            }
            if (input.dash.pressed)
            {
                ReleaseRope("dash");
                return; // TryDash runs next (AIR now)
            }
            if (input.slide.pressed && !grounded)
            {
                ReleaseRope("slam");
                return;
            }
            // The rope breaks when something blocks it for a moment, or when it would have to stretch past maxLen.
            Vec3 chest = RopePoint;
            double dist = Vec3.DistanceTo(chest, r.anchor);
            if (dist > a.maxLen * (1 + a.stretch))
            {
                ReleaseRope("length");
                return;
            }
            Vec3 dir = Vec3.DivideScalar(r.anchor - chest, dist != 0 ? dist : 1);
            bool blocked = world.RayIntersect(chest, dir, out RayHit hit) && hit.distance < dist - a.losSlack;
            r.blindSteps = blocked ? r.blindSteps + 1 : 0;
            if (r.blindSteps >= MovementMath.StepsFor(a.losBreak, dt))
            {
                ReleaseRope("blocked");
                return;
            }
            // A zip (a big enemy hooked): reel hard and let go when you arrive (then swing your melee).
            if (r.zip && (dist < a.zip.arrive || r.attachedSteps > MovementMath.StepsFor(a.zip.maxTime, dt)))
            {
                ReleaseRope("zip");
                return;
            }
            // On the ground the rope pays out (no leash); the next airtime swings on what is out.
            if (state != MoveState.Swing && !grounded && state != MoveState.Dash && state != MoveState.Slam) EnterSwing();
            if (grounded && state != MoveState.Swing) r.L = JsMath.Max(r.L, JsMath.Min(a.maxLen, dist));
        }

        void EnterSwing()
        {
            if (state == MoveState.Dash)
            {
                RecordDashDistance();
                dashSteps = 0;
                invulnerable = false;
            }
            if (state == MoveState.Slide) Emit(MoverEventType.SlideEnd);
            if (state == MoveState.WallRide) EndRide("rope");
            state = MoveState.Swing;
            grounded = false;
            coyoteSteps = 0;
            jumpCutArmed = false;
            windSteps = 0;
            if (!airborne) BeginAirtime(position.y, false);
        }

        /// <summary>
        /// One fixed step on the rope: substeps of SwingMath.Step, each followed by the capsule collision (so the rope
        /// can never push you into geometry). Landing on something ends the swing (the rope stays attached, pays out).
        /// </summary>
        void StepSwing(double dt)
        {
            MovementConfig c = config;
            ArclineConfig a = c.arcline;
            Rope r = rope;
            SwingParams p = SwingMath.Params(a, MovementMath.GravityFor(c.jump.height, c.jump.apexTime));
            int n = Math.Max(1, a.substeps);
            double h = dt / n;
            var inp = new SwingInput
            {
                reel = input.reel.held || r.zip,
                wish = wishSpeed > 0 ? wish : (Vec3?)null,
            };
            if (r.zip) p.reelSpeed = a.zip.reelSpeed;
            gravityMultiplier = a.gravityMult;
            grounded = false;
            touchingWall = false;
            wallHitCount = 0;
            impactSpeed = 0;
            for (int i = 0; i < n; i++)
            {
                inp.pulling = r.pullSteps > 0;
                swingState.pos = RopePoint;
                Vec3 before = swingState.pos;
                if (r.zip)
                {
                    if (flow34.rules.zipCarry && !flow34.carryArmed) // NIGHT34 flow34 zipCarry: the velocity the pull is about to replace
                    {
                        flow34.carryArmed = true;
                        flow34.carryVel = new Vec3(velocity.x, 0, velocity.z);
                    }
                    // A zip (a big enemy hooked): straight at it at the zip speed, the rope taut the whole way.
                    Vec3 toward = r.anchor - swingState.pos;
                    double zd = toward.Length;
                    if (zd == 0) zd = 1;
                    swingState.vel = toward * (a.zip.reelSpeed / zd);
                    swingState.pos = swingState.pos + swingState.vel * h;
                    r.L = JsMath.Max(a.minLen * 0.5, zd - a.zip.reelSpeed * h);
                    r.taut = true;
                }
                else
                {
                    swingState.vel = velocity;
                    swingState.L = r.L;
                    SwingStepResult o = SwingMath.Step(ref swingState, r.anchor, inp, p, h);
                    r.L = swingState.L;
                    r.taut = o.taut;
                    r.stretch = o.stretch;
                }
                velocity = swingState.vel;
                // Collide along the move the rope asked for (contacts clip the velocity too).
                move = swingState.pos - before;
                bool wasGrounded = grounded;
                SyncCapsule();
                capsule.Translate(move);
                ResolveContacts();
                double rad = capsule.radius;
                position = new Vec3(capsule.start.x, capsule.start.y - rad, capsule.start.z);
                grounded = grounded || wasGrounded;
            }
            if (r.pullSteps > 0) r.pullSteps--;
            double speed = velocity.Length;
            r.maxSpeed = JsMath.Max(r.maxSpeed, speed);
            velocity = MovementMath.ClampHorizontal(velocity, a.softCap * 1.2);
            if (grounded && velocity.y <= 0.5 && r.pullSteps == 0 && !r.zip)
            {
                r.groundTouches++;
                state = MoveState.Air; // AfterMove lands it (GROUND / SLIDE); the rope stays attached and pays out
            }
        }

        // --- Actions ------------------------------------------------------------------------------------------------

        void TryDash(double dt)
        {
            MovementConfig c = config;
            MoveState s = state;
            if (s != MoveState.Ground && s != MoveState.Air && s != MoveState.Slide && s != MoveState.WallRide) return;
            if (stamina.rules.Any && !Stamina.CanBoost(stamina.rules, stamina.bar))
            {
                Emit(MoverEventType.DashFail); // NIGHT17 U3-30.4: the bar is under the boost's floor (the pips are held full with the bar on)
                return;
            }
            if (pips < c.dash.pipCost)
            {
                Emit(MoverEventType.DashFail); // not buffered (SPEC 1.3)
                return;
            }

            double entry = Speed;
            if (s == MoveState.WallRide) dashDir = MovementMath.WallRideDashDir(wish, wishSpeed > 0, forward, rideNormal);
            else if (wishSpeed > 0) dashDir = wish;
            else dashDir = forward; // no input: camera forward, flattened

            if (s == MoveState.Slide) Emit(MoverEventType.SlideEnd);
            if (s == MoveState.WallRide) EndRide("dash");
            pips -= c.dash.pipCost;
            dashSpeedNow = MovementMath.DashSpeed(entry, c.dash.speed);
            dashSpeedBefore = entry;
            dashSteps = MovementMath.StepsFor(c.dash.duration, dt);
            if (stamina.rules.Any) StaminaBoost(dt); // NIGHT17 U3-30.4: the bar pays; a partial boost is shorter (its invulnerable steps too)
            dashStart = position;
            dashFromSlide = s == MoveState.Slide;
            velocity = new Vec3(dashDir.x * dashSpeedNow, 0, dashDir.z * dashSpeedNow);
            jumpCutArmed = false;
            windSteps = 0;
            invulnerable = true;
            state = MoveState.Dash;
            Emit(new MoverEvent { type = MoverEventType.Dash, entrySpeed = entry, speed = dashSpeedNow, pips = pips, from = s });
        }

        void RecordDashDistance()
        {
            Vec3 d = position - dashStart;
            lastDashDistance = JsMath.Hypot(d.x, d.z);
        }

        /// <summary>Dash ends without a jump: exit speed rule, then back to the state underneath.</summary>
        void EndDash(in InputFrame frame)
        {
            double exit = MovementMath.DashExitSpeed(config.ground.walkSpeed, dashSpeedBefore);
            velocity.x = dashDir.x * exit;
            velocity.z = dashDir.z * exit;
            RecordDashDistance();
            invulnerable = false;
            dashSteps = 0;
            if (!grounded) state = MoveState.Air;
            else if (frame.slide.held) StartSlide();
            else state = MoveState.Ground;
        }

        void TryJump(double dt, double gravity)
        {
            MovementConfig c = config;
            MoveState s = state;
            bool wallNear = s == MoveState.Air && FindNearWall();
            bool wall42Refused = false;
            if (wall42.on && wallNear && !Wall42Allows(nearWallNormal, gravity)) // NIGHT42 wallfling42: no intent or a landing ahead: no wall jump
            {
                wallNear = false;
                wall42Refused = true;
            }
            bool flowCoyoteWall = false;
            if (m43.p.chimney && flow34.rules.wallCoyote && s == MoveState.Air && !wallNear && flow34.wallCoyoteLeft > 0) // NIGHT43 chimney43: the coyote wall may be a new wall
                Chimney43Refill(flow34.wallCoyoteNormal, false);
            if (flow34.rules.wallCoyote && s == MoveState.Air && !wallNear && flow34.wallCoyoteLeft > 0 && wallJumpsLeft > 0)
            {
                wallNear = true; // NIGHT34 flow34 wallCoyote: the ended ride's wall still counts (its normal)
                nearWallNormal = flow34.wallCoyoteNormal;
                flowCoyoteWall = true;
            }
            if (m43.p.chimney && !flowCoyoteWall && (s == MoveState.WallRide || wallNear)) // NIGHT43 chimney43: a different wall refills the count first
                Chimney43Refill(s == MoveState.WallRide ? rideNormal : nearWallNormal, false);
            if (m43.p.ride && m43.p.pressPushes && R35 && s == MoveState.WallRide && wallJumpsLeft <= 0) // NIGHT43 ride43 (h): a press on a ride always pushes off (never the double jump straight up)
            {
                m43.pressPushes++;
                Ride35JumpOff(rideNormal, false);
                return;
            }
            var ctx = new JumpContext
            {
                state = s,
                grounded = grounded,
                coyoteSteps = coyoteSteps,
                doubleJumpsLeft = doubleJumpsLeft,
                wallNear = wallNear,
                wallJumpsLeft = wallJumpsLeft,
                boostSteps = boostSteps,
                pips = pips,
                dashJumpCost = c.dashJump.extraPipCost,
            };
            if (stamina.rules.Any)
            {
                ctx.pips = stamina.bar; // NIGHT17 U3-30.4: the dash-jump's extra cost in bar units (design.stamina.boost.dashJumpExtra)
                ctx.dashJumpCost = stamina.rules.dashJumpExtra;
            }
            ctx.landingSoon =
                s == MoveState.Air &&
                coyoteSteps == 0 &&
                doubleJumpsLeft > 0 &&
                !(wallNear && wallJumpsLeft > 0) &&
                velocity.y <= 0 &&
                LandingWithin(bufferSteps * dt, gravity * PredictScale());
            // NIGHT34 flow34 wallBuffer: a press with a double jump left waits for a wall predicted within the buffer (landingSoon's rule, for
            // walls); on the buffer's last step it is the double jump after all, so a miss never eats the press
            if (flow34.rules.wallBuffer && s == MoveState.Air && !wallNear && coyoteSteps == 0 && (wallJumpsLeft > 0 || m43.p.chimney /* NIGHT43 chimney43: the wall ahead is usually a new wall */) && doubleJumpsLeft > 0 &&
                !ctx.landingSoon && bufferSteps > 1 && !(wall42.on && (wall42Refused || wishSpeed <= 0)) && Flow34WallSoon(bufferSteps * dt)) // NIGHT42 wallfling42: no hold for a wall the stick does not mean
            {
                if (input.jump.pressed) flow34.heldForWall++;
                return;
            }
            // NIGHT34 flow34 slideBounce34 (rea-ultrakill.md, Movement): sliding (or within startSteps of a slide start) into a wall, the jump is a wall
            // jump that REFLECTS the slide off the wall at its full speed (plus a push away) and keeps at least the wall jump's up speed
            if (flow34.rules.slideBounce && (s == MoveState.Slide || flow34.sinceSlide <= flow34.rules.slideBounceSteps) && s != MoveState.WallRide &&
                wallJumpsLeft > 0 && FindNearWall() && slideDir.x * nearWallNormal.x + slideDir.z * nearWallNormal.z < -0.05 &&
                !(wall42.on && Wall42ShallowSlide())) // NIGHT42 wallfling42: a slide that only grazes the wall jumps as a slide jump
            {
                Flow34SlideBounce(s);
                return;
            }
            JumpKind kind = MovementMath.ResolveJump(ctx);
            if (kind == JumpKind.None) return;
            if (flowCoyoteWall && kind == JumpKind.Wall) // NIGHT34 flow34 wallCoyote: spent
            {
                flow34.coyoteWallJumps++;
                flow34.wallCoyoteLeft = 0;
            }

            double v0 = MovementMath.JumpVelocity(c.jump.height, c.jump.apexTime);
            double speedBefore = Speed;
            bool fromGround = grounded || kind == JumpKind.Ground;
            if (s == MoveState.Dash && kind != JumpKind.Dash) EndDash(input); // 0 pips: exit rule, then a normal jump
            bool cutArmed = true;

            switch (kind)
            {
                case JumpKind.Dash:
                    // Keep the dash's horizontal velocity; lighter gravity for the wind window.
                    pips -= c.dashJump.extraPipCost;
                    if (stamina.rules.Any) StaminaSpend(stamina.rules.dashJumpExtra); // NIGHT17 U3-30.4: the bar pays the extra
                    RecordDashDistance();
                    dashSteps = 0;
                    invulnerable = false;
                    velocity.y = v0 * c.dashJump.v0Mult;
                    windSteps = MovementMath.StepsFor(c.dashJump.windowTime, dt);
                    cutArmed = false; // a launch, not a short-hop jump
                    Emit(new MoverEvent { type = MoverEventType.DashJump, speed = Speed, pips = pips });
                    break;
                case JumpKind.Boost:
                    if (s == MoveState.Slide) Emit(MoverEventType.SlideEnd);
                    velocity.y = v0 * c.boostJump.v0Mult;
                    boostSteps = 0;
                    cutArmed = false; // a launch, not a short-hop jump
                    Emit(new MoverEvent { type = MoverEventType.BoostJump, speed = velocity.y });
                    break;
                case JumpKind.Wall:
                {
                    Vec3 n = s == MoveState.WallRide ? rideNormal : nearWallNormal;
                    // NIGHT43 chimney43 (round 2, GG's G2: "the rapid bounce from wall to wall naturally carries them upward"): a pressed wall jump off a
                    // NEW wall within chainWindow of leaving the last one is a chain: chainUp up, chainOut out
                    bool chain43 = m43.p.chimney && m43.p.chainUp > 0 && m43.hasWall && m43.sinceRide <= m43.p.chainSteps && Move43.NewWall(m43, n, position);
                    if (chain43)
                    {
                        velocity = MovementMath.WallJumpVelocity(velocity, n, m43.p.chainOut, m43.p.chainUp);
                        m43.chains++;
                    }
                    else
                    velocity = wall42.on // NIGHT42 wallfling42: a smaller back push, more lift
                        ? MovementMath.WallJumpVelocity(velocity, n, wall42.outSpeed, v0 * wall42.v0Mult)
                        : MovementMath.WallJumpVelocity(velocity, n, c.wallJump.outSpeed, v0 * c.wallJump.v0Mult);
                    wallJumpsLeft--;
                    if (m43.p.Any) Chimney43Mark(n); // NIGHT43 chimney43: this wall is the last wall
                    if (m43.p.Any) m43.sinceRide = 0; // NIGHT43 ride43 (d) / chimney43's chain: a wall jump leaves a wall too
                    if (climb47.p.on && !double.IsNaN(Climb47Bottom(n))) velocity.y = Climb47Up(n, velocity.y); // NIGHT47 climb47: SET UP (guided to the next hold) off a climb wall
                    doubleJumpsLeft = c.jump.doubleJumps; // SPEC 1.3: refreshed by a wall jump
                    wallJumpLockSteps = MovementMath.StepsFor(c.wallRide.rejoinLockTime, dt);
                    wallJumpLockNormal = n;
                    rideLockActive = false;
                    if (s == MoveState.WallRide) EndRide("jump");
                    cutArmed = false; // SPEC 1.4: a launch, releasing Space doesn't cut it
                    Emit(new MoverEvent { type = MoverEventType.WallJump, speed = Speed, wallJumpsLeft = wallJumpsLeft });
                    break;
                }
                case JumpKind.Double:
                    if (s == MoveState.WallRide) EndRide("jump");
                    velocity.y = m43.p.boost && velocity.y > v0 * c.jump.doubleJumpMult ? velocity.y : v0 * c.jump.doubleJumpMult; // NIGHT43 boost43 (round 2): a double jump never lowers a rise (a boost)
                    doubleJumpsLeft--;
                    Emit(new MoverEvent { type = MoverEventType.DoubleJump, speed = speedBefore });
                    break;
                default:
                    // Ground (incl. coyote) or slide
                    if (kind == JumpKind.Slide)
                    {
                        double boosted = MovementMath.SlideJumpSpeed(speedBefore, c.slide.jumpHorizontalMult, c.slide.jumpBoostMaxSpeed * slideJumpCapMult);
                        if (moveBalance.rules.hopFalloff) boosted = BalanceHop(speedBefore); // NIGHT16 U3-29.4: the chain's falloff
                        if (speedBefore > 0)
                        {
                            velocity.x *= boosted / speedBefore;
                            velocity.z *= boosted / speedBefore;
                        }
                        Emit(MoverEventType.SlideEnd);
                    }
                    velocity.y = v0;
                    Emit(new MoverEvent
                    {
                        type = MoverEventType.Jump,
                        kind = kind,
                        coyote = s == MoveState.Air,
                        buffered = !input.jump.pressed,
                        speedBefore = speedBefore,
                        speedAfter = Speed,
                    });
                    break;
            }

            if (flow34.rules.lurch && fromGround) // NIGHT34 flow34 lurch: a takeoff arms the one redirect
            {
                flow34.lurchArmed = true;
                flow34.sinceJump = 0;
            }
            if (fromGround || kind == JumpKind.Boost) BeginAirtime(position.y, true);
            jumpAirtime = true;
            jumpCutArmed = cutArmed;
            bufferSteps = 0;
            coyoteSteps = 0;
            grounded = false;
            state = MoveState.Air;
        }

        void TrySlam(double dt, double gravity)
        {
            MovementConfig c = config;
            // A C press just before a predicted landing becomes a slide on landing (SPEC 1.3).
            int window = MovementMath.StepsFor(c.slam.slidePredictTime, dt);
            if (velocity.y <= 0 && LandingWithin(window * dt, gravity * PredictScale()))
            {
                slideBufferSteps = window;
                return;
            }
            // NIGHT43 ride43 (d): just after leaving a wall B is a drop that keeps the horizontal speed (a rise cut to 0), not a slam, unless the
            // look is down past slamLookDeg
            if (m43.p.ride && state == MoveState.Air && m43.sinceRide <= m43.p.slamGraceSteps && JsMath.Sin(input.pitch) >= -m43.p.sinSlamLook)
            {
                if (velocity.y > 0) velocity.y = 0;
                jumpCutArmed = false;
                m43.bDrops++;
                return;
            }
            StartSlam();
        }

        void StartSlam()
        {
            if (state == MoveState.WallRide) EndRide("slam");
            velocity = new Vec3(0, -config.slam.speed, 0);
            slamStartY = position.y;
            slideBufferSteps = 0;
            windSteps = 0;
            jumpCutArmed = false;
            state = MoveState.Slam;
        }

        void StartSlide()
        {
            MovementConfig c = config;
            double speed = Speed;
            if (flow34.rules.slideBounce) flow34.sinceSlide = 0; // NIGHT34 flow34 slideBounce34: a slide starts
            // Direction is locked at slide start (SPEC 1.2): current motion, else input, else facing.
            if (speed > 0) slideDir = new Vec3(velocity.x / speed, 0, velocity.z / speed);
            else if (wishSpeed > 0) slideDir = wish;
            else slideDir = MovementMath.WishDirection(0, 1, input.yaw);
            double slideSpeed = JsMath.Max(c.slide.floorSpeed * slideSpeedMult, speed);
            if (moveBalance.rules.Any) slideSpeed = BalanceSlideStart(speed, slideSpeed); // NIGHT16 U3-29.4: the snap needs the charge
            velocity.x = slideDir.x * slideSpeed;
            velocity.z = slideDir.z * slideSpeed;
            crouched = true;
            SyncCapsule();
            state = MoveState.Slide;
            Emit(new MoverEvent { type = MoverEventType.SlideStart, speed = slideSpeed });
        }

        void StartRide(Vec3 normal)
        {
            MovementConfig c = config;
            rideNormal = normal;
            double outward = velocity.x * normal.x + velocity.z * normal.z;
            if (outward > 0)
            {
                velocity.x -= outward * normal.x;
                velocity.z -= outward * normal.z;
            }
            if (velocity.y < 0) velocity.y = 0; // SPEC 1.3: falling entries are caught, rising entries keep rising
            // Keep the full horizontal speed, redirected along the wall (no loss from the approach angle).
            double clipped = JsMath.Hypot(velocity.x, velocity.z);
            rideDir = new Vec3(velocity.x / clipped, 0, velocity.z / clipped);
            double along = JsMath.Max(clipped, preMoveSpeed);
            velocity.x = rideDir.x * along;
            velocity.z = rideDir.z * along;
            rideSteps = MovementMath.StepsFor(c.wallRide.maxTime + bonusRideTime, 1 / c.loop.simHz);
            doubleJumpsLeft = c.jump.doubleJumps; // SPEC 1.3: refreshed on ride start
            if (m43.p.chimney && m43.p.rideRefills) Chimney43Refill(normal, true); // NIGHT43 chimney43: a ride on a new wall refills the wall jumps
            jumpCutArmed = false;
            windSteps = 0;
            wallNormal = normal;
            if (R35) // NIGHT35 wallride35: a hold ride (jump held at the entry) has only the safety cap; the late window closes
            {
                ref Ride35.State r = ref flow34.ride35;
                r.held = input.jump.held;
                r.rideAge = 0;
                r.lateLeft = 0;
                r.rides++;
                if (r.grip.on) // NIGHT36 wallgrip36: the attach keeps only a small kick of a jump's rise (the along speed is kept above)
                {
                    r.gripAge = 0;
                    if (velocity.y > r.grip.attachUpMax)
                    {
                        if (m43.p.ride) // NIGHT43 ride43 (b): the snap eased: keepRise x the rise kept (never under the clamp), then braked by gravity
                        {
                            double kept = velocity.y * m43.p.keepRise;
                            velocity.y = kept > r.grip.attachUpMax ? kept : r.grip.attachUpMax;
                            m43.eased++;
                        }
                        else velocity.y = r.grip.attachUpMax;
                        r.gripClamps++;
                    }
                }
                if (r.held)
                {
                    r.holdRides++;
                    rideSteps = r.r38.on ? r.r38.guardSteps : r.rules.safetySteps; // NIGHT38 ride38: only the long stuck guard
                    if (r.r38.on) r.r38Rising = velocity.y > 0; // NIGHT38 ride38: a rising entry brakes first (wallride35 / wallgrip36's brake)
                    bufferSteps = 0; // a press held for this wall (flow34 wallBuffer) is this hold ride, not a wall jump off it next step
                }
                else if (r.r40.on && r.r38.on) // NIGHT40 ride40: a tap ride holds too (no 1.2 s timer, only the stuck guard; a rising entry brakes first)
                {
                    rideSteps = r.r38.guardSteps;
                    r.r38Rising = velocity.y > 0;
                }
            }
            state = MoveState.WallRide;
            if (climb47.p.on) Climb47Start(normal); // NIGHT47 climb47: on a climb wall: the speed floor, THE CATCH, the buffered jump-off
            Emit(new MoverEvent { type = MoverEventType.WallRideStart, speed = along });
        }

        /// <summary>NIGHT47 climb47 (a ride just started; rules on): on a climb wall, the along speed floored at retrySpeed, THE CATCH armed when the
        /// feet are under the panel's bottom + catchLine by at most catchBelow, and a jump let go within the buffer queued as this ride's jump-off.</summary>
        void Climb47Start(Vec3 normal)
        {
            ref Climb47.State k = ref climb47;
            k.catchLeft = 0;
            k.pendingJumpOff = 0;
            double b = Climb47Bottom(normal);
            k.onClimb = !double.IsNaN(b);
            if (double.IsNaN(b)) return;
            double along = JsMath.Hypot(velocity.x, velocity.z);
            if (along < k.p.retrySpeed && along > 1e-6)
            {
                velocity.x = velocity.x / along * k.p.retrySpeed;
                velocity.z = velocity.z / along * k.p.retrySpeed;
                k.speedFloors++;
            }
            double line = b + k.p.catchLine;
            if ((position.y < line && position.y >= line - k.p.catchBelow) || (position.y > line && position.y <= line + k.p.catchAbove))
            {
                k.catchLeft = k.p.catchEaseSteps;
                k.catchTarget = line;
                k.catches++;
            }
            if (!input.jump.held && k.sinceRelease <= k.p.bufferSteps) k.pendingJumpOff = 1;
        }

        /// <summary>Leaves the ride. Air endings lock the same wall until you stop touching it (SPEC 1.3).</summary>
        void EndRide(string reason)
        {
            if (reason != "jump" && reason != "land")
            {
                rideLockActive = true;
                rideLockNormal = rideNormal;
            }
            // NIGHT34 flow34 wallCoyote: a ride that ends on its own (away, timeout, corner, lost, blocked) leaves the wall jump a moment longer
            if (flow34.rules.wallCoyote && (reason == "away" || reason == "timeout" || reason == "corner" || reason == "lost" || reason == "blocked"))
            {
                flow34.wallCoyoteLeft = flow34.rules.wallCoyoteSteps;
                flow34.wallCoyoteNormal = rideNormal;
            }
            if (R35) // NIGHT35 wallride35: the re-ride memory; a ride that ends on its own leaves the late jump
            {
                ref Ride35.State r = ref flow34.ride35;
                if (reason != "land" && reason != "reset")
                {
                    r.hasLast = true;
                    r.lastNormal = rideNormal;
                    r.lastEnd = position;
                    if (r.r40.lockoutSteps > 0) // NIGHT40 ride40: the same wall locked out for a moment
                    {
                        r.r40LockLeft = r.r40.lockoutSteps;
                        r.r40LockNormal = rideNormal;
                    }
                }
                if (reason == "safety") r.safetyEnds++;
                if (reason == "away" || reason == "lost" || reason == "corner" || reason == "blocked" || reason == "timeout" || reason == "safety" || reason == "drop")
                {
                    r.lateLeft = r.rules.lateSteps;
                    if (climb47.p.on && climb47.p.coyoteSteps > r.lateLeft && (climb47.onClimb || !double.IsNaN(Climb47Bottom(rideNormal)))) r.lateLeft = climb47.p.coyoteSteps; // NIGHT47 climb47: COYOTE off a climb wall
                    r.lateNormal = rideNormal;
                    r.lateHeld = r.held && input.jump.held;
                }
                r.held = false;
            }
            if (m43.p.Any && reason != "land" && reason != "reset") m43.sinceRide = 0; // NIGHT43 ride43 (d) / chimney43's chain: the wall just left
            rideSteps = 0;
            state = MoveState.Air;
            Emit(new MoverEvent { type = MoverEventType.WallRideEnd, reason = reason, speed = Speed });
        }

        // --- State handlers -----------------------------------------------------------------------------------------

        void StepGround(double dt)
        {
            MovementConfig c = config;
            // Friction is skipped on the first grounded step so bunny hops keep speed (SPEC §3.2).
            if (groundedSteps > 0) velocity = MovementMath.ApplyFriction(velocity, c.ground.friction * groundFrictionMult, c.ground.stopSpeed, dt);
            if (stamina.sprintStep) wishSpeed = Stamina.SprintWish(stamina.rules, input.moveX, input.moveZ); // NIGHT17 U3-30.4: the sprint (only with design.stamina.sprint)
            velocity = MovementMath.Accelerate(velocity, wish, wishSpeed * walkMult, c.ground.accel, dt);
        }

        void StepAir(double dt)
        {
            MovementConfig c = config;
            if (flow34.rules.lurch && flow34.lurchArmed && flow34.sinceJump <= flow34.rules.lurchSteps && wishSpeed > 0) // NIGHT34 flow34 lurch: one redirect
            {
                double wl = JsMath.Hypot(wish.x, wish.z);
                Vec3? turned = wl > 1e-9 ? Flow34.Lurch(velocity, wish.x / wl, wish.z / wl, flow34.rules) : null;
                if (turned.HasValue)
                {
                    velocity = turned.Value;
                    flow34.lurchArmed = false;
                    flow34.lurches++;
                }
            }
            velocity = MovementMath.Accelerate(velocity, wish, wishSpeed, c.air.accel, dt);
            JumpRelease release = MovementMath.ReleaseJump(velocity.y, input.jump.held, c.jump.releaseMult, jumpCutArmed);
            velocity.y = release.vy;
            jumpCutArmed = release.cutArmed;
        }

        void StepSlide(double dt)
        {
            MovementConfig c = config;
            if (!input.slide.held && HasHeadroom())
            {
                state = MoveState.Ground;
                Emit(MoverEventType.SlideEnd);
                return;
            }
            // Locked direction, but walls deflect it: follow the post-collision velocity.
            double speed = Speed;
            if (speed > 0) slideDir = new Vec3(velocity.x / speed, 0, velocity.z / speed);
            // Slope boost (SPEC 2.0): gravity along the ground speeds a downhill slide (no slide friction while going
            // downhill, so ramps pay off) and bleeds an uphill one.
            double slope = grounded && groundNormal.y < 1
                ? MovementMath.SlopeSlideAccel(groundNormal, slideDir, MovementMath.GravityFor(c.jump.height, c.jump.apexTime), c.slide.slopeBoost)
                : 0;
            double floor = c.slide.floorSpeed * slideSpeedMult;
            if (moveBalance.rules.Any) floor = BalanceSlideFloor(floor, slope); // NIGHT16 U3-29.4: the uncharged floor, the flat-ground decay
            if (speed > floor && groundedSteps > 0 && slope <= 0)
            {
                velocity = MovementMath.ApplyFriction(velocity, c.slide.friction * slideFrictionMult, c.ground.stopSpeed, dt);
                speed = Speed;
            }
            speed = JsMath.Max(0, speed + slope * dt);
            speed = JsMath.Max(floor, speed);
            velocity.x = slideDir.x * speed;
            velocity.z = slideDir.z * speed;
        }

        // --- NIGHT16 U3-29.4: design.move (MoveBalance), only when a rule is on ------------------------------------

        /// <summary>At a step's start: the landing (Land() ran in the last step: the hop chain's window) and the charge's clock.</summary>
        void BalanceBeginStep()
        {
            ref MoveBalance.State b = ref moveBalance;
            if (!airborne && b.wasAirborne) b.sinceLand = 0;
            else if (b.sinceLand < 1 << 30) b.sinceLand++;
            b.wasAirborne = airborne;
            if (!b.rules.slideCharge || !b.spent) return;
            bool back = MoveBalance.TickCharge(ref b, state == MoveState.Slide);
            slideCharge = MoveBalance.ChargeProgress(b.spent, b.rechargeLeft, b.rules.rechargeSteps);
            if (back) Emit(MoverEventType.SlideCharged);
        }

        /// <summary>StartSlide: a snap up to the floor uses the charge; without it the slide keeps its entry speed. The slide's base floor.</summary>
        double BalanceSlideStart(double speed, double slideSpeed)
        {
            ref MoveBalance.State b = ref moveBalance;
            double floor = config.slide.floorSpeed * slideSpeedMult;
            bool charged = true;
            if (b.rules.slideCharge)
            {
                if (speed < floor) charged = !b.spent; // the snap would add speed: it needs the charge
                if (speed < floor || b.spent)
                {
                    b.spent = true;
                    b.rechargeLeft = b.rules.rechargeSteps; // used now, or a slide entered while recharging: the clock starts over
                    slideCharge = 0;
                }
            }
            // NIGHT16 review: an uncharged slide is never slower than walking (from a standstill it went 0 m/s and rooted the
            // player: StepSlide has no input acceleration and a locked direction); the floor is the decay's target.
            b.slideBase = MoveBalance.EntryFloor(floor, speed, charged, BalanceWalkFloor());
            b.slideSteps = 0;
            return charged ? slideSpeed : b.slideBase;
        }

        /// <summary>StepSlide's floor: the slide's base, eased toward the walk speed on flat ground after the sustain.</summary>
        double BalanceSlideFloor(double floor, double slope)
        {
            ref MoveBalance.State b = ref moveBalance;
            double f = b.slideBase;
            if (b.rules.slideDecay && slope == 0) f = MoveBalance.DecayedFloor(f, BalanceWalkFloor(), b.slideSteps, b.rules.sustainSteps, b.rules.decaySteps);
            if (b.slideSteps < 1 << 30) b.slideSteps++;
            return f;
        }

        /// <summary>The walk speed a slide never drops below under the rules (the decay's target and an uncharged slide's floor):
        /// ground.walkSpeed x the SLIPSTREAM slideSpeedMult.</summary>
        double BalanceWalkFloor() => config.ground.walkSpeed * slideSpeedMult;

        /// <summary>A slide-jump's horizontal speed: the multiplier falls off along a chain of quick hops; the cap stays.</summary>
        double BalanceHop(double speedBefore)
        {
            ref MoveBalance.State b = ref moveBalance;
            MovementConfig c = config;
            b.hopChain = MoveBalance.NextHop(b.hopLive, b.hopChain, b.sinceLand, b.rules.chainSteps);
            b.hopLive = true;
            double mult = MoveBalance.HopMult(c.slide.jumpHorizontalMult, b.hopChain, b.rules.hopStep, b.rules.minMult);
            return MovementMath.SlideJumpSpeed(speedBefore, mult, c.slide.jumpBoostMaxSpeed * slideJumpCapMult);
        }

        // --- NIGHT17 U3-30.4: design.stamina (Stamina), only when the bar is on ---------------------------------------

        /// <summary>The bar full, the sprint off (a reset, a run's start); the web's pips held full (their checks always pass: the bar decides).</summary>
        void StaminaReset()
        {
            ref Stamina.State s = ref stamina;
            if (s.sprinting) Emit(MoverEventType.SprintEnd);
            s.bar = StaminaMax;
            s.sinceSpend = 1 << 20;
            s.emptied = false;
            s.armed = false;
            s.sprinting = false;
            s.sprintStep = false;
            s.full = true;
            s.lastBoost = 1;
            pips = MaxPips;
        }

        /// <summary>Step 2 with the bar on: the boost key's hold (one key: the press boosts, a hold after it sprints), the
        /// sprint's start / end and its drain, the refill, the full edge. The sprint runs on the ground while the key is held
        /// and the stick moves; a hop or a slide pauses it (no drain, no speed) without ending it.</summary>
        void StaminaStep(double dt)
        {
            ref Stamina.State s = ref stamina;
            pips = MaxPips;
            double max = StaminaMax;
            bool held = input.dash.held, pressed = input.dash.pressed;
            if (pressed) s.armed = true; // the press boosts (TryDash, this step); the hold after it sprints
            if (!held) s.armed = false;
            // NIGHT19 (the shared commit's sprint hook): the sprint's own key (input.sprint: L3, toggled or held by the input
            // layer) also wants the sprint, without a boost first; read only with design.stamina.n19.ownKey (off: NIGHT18's rule)
            bool want = s.rules.sprint && ((s.armed && held && !pressed) || (s.rules.ownKey && input.sprint.held)) && s.bar > 0;
            bool moving = wishSpeed > 0;
            if (s.sprinting && (!want || (state == MoveState.Ground && !moving))) StaminaSprintEnd();
            else if (!s.sprinting && want && state == MoveState.Ground && moving)
            {
                s.sprinting = true;
                Emit(MoverEventType.SprintStart);
            }
            s.sprintStep = s.sprinting && state == MoveState.Ground && moving;
            if (s.sprintStep)
            {
                s.bar = Stamina.Drain(s.rules, s.bar, dt);
                if (s.bar <= 0)
                {
                    s.bar = 0;
                    s.emptied = true;
                    s.armed = false; // it ends by itself; no lockout: the next press boosts (with what came back) and arms it again
                    StaminaSprintEnd(); // this step still runs at the sprint's speed
                }
            }
            bool busy = state == MoveState.Dash || s.sprintStep;
            if (busy) s.sinceSpend = 0;
            else if (s.sinceSpend < 1 << 20) s.sinceSpend++;
            s.bar = Stamina.Refill(s.rules, s.bar, max, regenMult, s.sinceSpend, s.emptied, busy, dt);
            if (s.emptied && s.bar > 0) s.emptied = false;
            bool full = s.bar >= max;
            if (full && !s.full) Emit(MoverEventType.StaminaFull);
            s.full = full;
        }

        void StaminaSprintEnd()
        {
            stamina.sprinting = false;
            Emit(MoverEventType.SprintEnd);
        }

        /// <summary>A spend from the bar (the boost, the dash-jump's extra): the refill's delay starts over.</summary>
        void StaminaSpend(double amount)
        {
            ref Stamina.State s = ref stamina;
            s.bar = JsMath.Max(0, s.bar - amount);
            s.sinceSpend = 0;
            s.emptied = false;
            pips = MaxPips;
        }

        /// <summary>TryDash with the bar on: the boost takes up to its cost; under fullAt x cost it runs for its share of the
        /// full duration (never below the floor), so its distance and its invulnerable steps scale with it (EndDash ends both).</summary>
        void StaminaBoost(double dt)
        {
            double fraction = Stamina.BoostFraction(stamina.rules, stamina.bar);
            StaminaSpend(Stamina.BoostSpend(stamina.rules, stamina.bar));
            stamina.lastBoost = fraction;
            if (fraction >= 1) return;
            dashSteps = Stamina.BoostSteps(config.dash.duration, fraction, dt);
            Emit(new MoverEvent { type = MoverEventType.BoostPartial, fraction = fraction, pips = stamina.bar });
        }

        void StepDash() => velocity = new Vec3(dashDir.x * dashSpeedNow, 0, dashDir.z * dashSpeedNow);

        void StepSlam() => velocity = new Vec3(0, -config.slam.speed, 0);

        void StepWallRide(double dt)
        {
            MovementConfig c = config;
            Vec3 n = rideNormal;
            // Input pointing away from the wall ends the ride (SPEC 1.3).
            if (wishSpeed > 0 && wish.x * n.x + wish.z * n.z > c.wallRide.awayDot)
            {
                EndRide("away");
                StepAir(dt);
                return;
            }
            // Keep only the along-wall part, floored at the ride minimum.
            double into = velocity.x * n.x + velocity.z * n.z;
            velocity.x -= into * n.x;
            velocity.z -= into * n.z;
            double along = JsMath.Hypot(velocity.x, velocity.z);
            if (along > 0) rideDir = new Vec3(velocity.x / along, 0, velocity.z / along);
            along = JsMath.Max(c.wallRide.minSpeed, along);
            if (R35) flow34.ride35.rideSpeed = along = JsMath.Max(c.wallRide.minSpeed * (1 + flow34.ride35.rules.speedBuff), along); // NIGHT35 wallride35: the ride's speed buff
            velocity.x = rideDir.x * along;
            velocity.z = rideDir.z * along;
            if (R35 && (flow34.ride35.held || flow34.ride35.r40.on)) // NIGHT35 wallride35: a small speed into the wall (the contact clips it) keeps a curved wall's contact (NIGHT40 ride40: every ride)
            {
                velocity.x -= n.x * flow34.ride35.rules.stick;
                velocity.z -= n.z * flow34.ride35.rules.stick;
            }
        }

        // --- Transitions --------------------------------------------------------------------------------------------

        void AfterMove(double dt)
        {
            MovementConfig c = config;
            double cosCorner = JsMath.Cos((c.wallRide.cornerAngleDeg * Math.PI) / 180);

            if (state == MoveState.Dash)
            {
                dashSteps--;
                if (dashSteps <= 0) EndDash(input);
            }

            if (grounded)
            {
                // First grounded step after any airtime is a landing, even mid-dash.
                if (airborne) Land();
                else groundedSteps++;
            }
            else
            {
                if (state == MoveState.Ground || state == MoveState.Slide)
                {
                    // Walked or slid off an edge: coyote time starts now.
                    if (state == MoveState.Slide) Emit(MoverEventType.SlideEnd);
                    state = MoveState.Air;
                    coyoteSteps = MovementMath.StepsFor(c.jump.coyoteTime, dt);
                }
                if (!airborne) BeginAirtime(position.y, false);
                airPeakY = JsMath.Max(airPeakY, position.y);
                lastApexHeight = airPeakY - takeoffY;

                if (m43.p.ride && m43.p.dashAttach && R35 && state == MoveState.Dash && touchingWall && rope.phase != RopePhase.Attached) Ride43DashAttach(cosCorner); // NIGHT43 ride43 (g): a ride it starts is checked from the next step
                else if (state == MoveState.WallRide) CheckRide(cosCorner);
                else if (state == MoveState.Air && rope.phase != RopePhase.Attached)
                {
                    CheckRideEntry(cosCorner);
                    if (R35 && state == MoveState.Air) Ride35Attach(cosCorner); // NIGHT35 wallride35: jump held beside a wall: drawn onto it
                }
            }

            // The ride lock clears once the locked wall is out of probe reach (SPEC 1.3).
            if (rideLockActive && (grounded || !ProbeWall(rideLockNormal, cosCorner, out _))) rideLockActive = false;

            // Stand up when the crouch is no longer wanted and there is headroom (SPEC 1.2).
            bool wantCrouch = state == MoveState.Slide || (state == MoveState.Dash && dashFromSlide && input.slide.held);
            if (crouched && !wantCrouch && HasHeadroom())
            {
                crouched = false;
                SyncCapsule();
            }
        }

        void CheckRideEntry(double cosCorner)
        {
            MovementConfig c = config;
            if (!touchingWall) return;
            Vec3 n = wallNormal;
            if (RideLock43(n, cosCorner)) return;
            if (WallJumpLock43(n, cosCorner)) return;
            if (R35 && !Ride35Entry(n)) return; // NIGHT35 wallride35: the re-ride rule, the lowered gate and the head-on entry of a hold ride
            else if (!R35 && MovementMath.AlongWallSpeed(velocity, n) <= c.wallRide.entrySpeed) return;
            StartRide(n);
        }

        /// <summary>NIGHT35 wallride35 (only with the row on): may a ride start on the wall with normal n? The re-ride rule (a wall facing like the
        /// last ridden one, closer than reRideDistance to where that ride ended: no); the gate (a hold ride: rules.entrySpeed along the wall, else the
        /// web's); a hold ride's head-on entry (too little speed along the wall) rides the way the camera faces at the approach speed.</summary>
        bool Ride35Entry(Vec3 n)
        {
            ref Ride35.State r = ref flow34.ride35;
            if (r.noRide != null && NoRide38(n)) return false; // NIGHT38 noride: no ride starts on a noride wall
            if (r.r40.on && !Ride40Intent(n, capsule.radius)) return false; // NIGHT40 ride40: the attach asks for intent
            if (ReRide43(n))
            {
                r.reRideBlocks++;
                return false;
            }
            bool held = input.jump.held;
            double gate = held ? r.rules.entrySpeed : config.wallRide.entrySpeed;
            if (MovementMath.AlongWallSpeed(velocity, n) > gate) return true;
            if (!held) return false;
            double speed = JsMath.Max(preMoveSpeed, Speed);
            if (speed <= gate) return false;
            double fx = forward.x - n.x * (forward.x * n.x + forward.z * n.z), fz = forward.z - n.z * (forward.x * n.x + forward.z * n.z);
            double fl = JsMath.Hypot(fx, fz);
            if (fl < 0.2) return false; // looking straight at the wall: no side to ride to
            velocity.x = fx / fl * speed;
            velocity.z = fz / fl * speed;
            return true;
        }

        void CheckRide(double cosCorner)
        {
            MovementConfig c = config;
            Vec3 n = rideNormal;
            rideSteps--;
            if (R35 && (flow34.ride35.held || flow34.ride35.r40.on)) // NIGHT35 wallride35: the HOLD ride's safety cap, corners and curved walls followed (a ride without jump held: the web's checks; NIGHT40 ride40: every ride)
            {
                Ride35Check(cosCorner);
                return;
            }
            if (rideSteps <= 0)
            {
                EndRide("timeout");
                return;
            }
            // Any wall contact more than the corner angle off the ride wall: a corner (SPEC 1.3).
            for (int i = 0; i < wallHitCount; i++)
            {
                if (Vec3.Dot(wallHits[i], n) < cosCorner)
                {
                    EndRide("corner");
                    return;
                }
            }
            if (!ProbeWall(n, -1, out Vec3 hit))
            {
                EndRide("lost");
                return;
            }
            if (Vec3.Dot(hit, n) < cosCorner)
            {
                EndRide("corner");
                return;
            }
            if (MovementMath.AlongWallSpeed(velocity, n) < c.wallRide.entrySpeed)
            {
                EndRide("blocked");
                return;
            }
            wallNormal = n;
        }

        void Land()
        {
            MovementConfig c = config;
            bool wasSlam = state == MoveState.Slam;
            airborne = false;
            doubleJumpsLeft = c.jump.doubleJumps;
            wallJumpsLeft = MaxWallJumps;
            groundedSteps = 0;
            coyoteSteps = 0;
            windSteps = 0;
            slideBufferSteps = 0;
            jumpCutArmed = false;
            jumpAirtime = false;
            rideLockActive = false;
            if (state == MoveState.WallRide) EndRide("land");
            if (flow34.rules.wallCoyote) flow34.wallCoyoteLeft = 0; // NIGHT34 flow34 wallCoyote: a landing ends it
            if (flow34.rules.lurch) flow34.lurchArmed = false; // NIGHT34 flow34 lurch: a landing ends the window
            if (R35) // NIGHT35 wallride35: a landing clears the re-ride memory and the late window
            {
                flow34.ride35.hasLast = false;
                flow34.ride35.lateLeft = 0;
            }
            if (m43.p.Any) // NIGHT43 chimney43 / ride43: a landing clears the last wall and the B grace
            {
                m43.hasWall = false;
                m43.sinceRide = int.MaxValue / 2;
            }
            if (wasSlam)
            {
                boostSteps = MovementMath.StepsFor(c.boostJump.window, 1 / c.loop.simHz);
                state = MoveState.Air; // falls through to GROUND/SLIDE below
                Emit(new MoverEvent { type = MoverEventType.Slam, position = position, fallDist = slamStartY - position.y });
                if (stamina.rules.Any && stamina.rules.rewards) StaminaReward(stamina.rules.slam); // NIGHT17 U3-30.4: a slam's landing refills
            }
            if (state == MoveState.Air)
            {
                if (input.slide.held) StartSlide();
                else state = MoveState.Ground;
            }
            Emit(new MoverEvent { type = MoverEventType.Land, fallSpeed = impactSpeed });
        }

        void BeginAirtime(double y, bool fromJump)
        {
            airborne = true;
            jumpAirtime = fromJump;
            takeoffY = y;
            airPeakY = y;
            lastApexHeight = 0;
        }

        // --- Collision ----------------------------------------------------------------------------------------------

        /// <summary>Capsule from the feet position and the current (standing or sliding) height.</summary>
        void SyncCapsule()
        {
            MovementConfig c = config;
            double r = c.body.capsuleRadius;
            double height = crouched ? c.slide.colliderHeight : c.body.capsuleHeight;
            capsule.radius = r;
            capsule.start = new Vec3(position.x, position.y + r, position.z);
            capsule.end = new Vec3(position.x, position.y + height - r, position.z);
        }

        /// <summary>Moves by `move` in substeps of at most maxSubstepMove, resolving contacts after each.</summary>
        void MoveAndCollide()
        {
            MovementConfig c = config;
            int substeps = Math.Max(1, (int)Math.Ceiling(move.Length / c.loop.maxSubstepMove));
            grounded = false;
            touchingWall = false;
            wallHitCount = 0;
            impactSpeed = 0;
            SyncCapsule();
            for (int i = 0; i < substeps; i++)
            {
                // Contacts clip `move`, so later substeps use what is left of it.
                capsule.Translate(Vec3.DivideScalar(move, substeps));
                ResolveContacts();
            }
            double r = capsule.radius;
            position = new Vec3(capsule.start.x, capsule.start.y - r, capsule.start.z);
        }

        /// <summary>Per-candidate resolution: each contact keeps its own normal (floor + wall never blend).</summary>
        void ResolveContacts()
        {
            for (int iter = 0; iter < config.collision.maxResolveIterations; iter++)
            {
                int count = world.GatherCandidates(capsule);
                bool hit = false;
                for (int i = 0; i < count; i++)
                {
                    if (!world.CapsuleIntersect(capsule, i, out CapsuleContact contact)) continue;
                    hit = true;
                    ResolveContact(contact);
                }
                if (!hit) break;
            }
        }

        /// <summary>A side contact: near-horizontal normal and a point on the cylinder section (SPEC 1.3).</summary>
        bool IsValidWall(Vec3 normal, Vec3 point, in Capsule cap) =>
            Math.Abs(normal.y) < config.surface.wallMaxAbsNormalY &&
            point.y > cap.start.y &&
            point.y < cap.end.y;

        void ResolveContact(CapsuleContact contact)
        {
            MovementConfig c = config;
            Vec3 n = contact.normal;
            Vec3 point = contact.point;
            double depth = contact.depth;

            // Wall validity uses the ORIGINAL normal and point, before any adjustment below.
            if (IsValidWall(n, point, capsule))
            {
                touchingWall = true;
                wallNormal = NormalizeJs(new Vec3(n.x, 0, n.z));
                lastWallNormal = wallNormal;
                hasLastWall = true;
                if (wallHitCount < MaxWallHits) wallHits[wallHitCount++] = wallNormal;
            }

            bool feet = point.y < capsule.start.y + capsule.radius;
            if (feet && n.y >= c.surface.floorMinNormalY)
            {
                // Floor: push straight up, so standing on an edge never creeps sideways.
                capsule.Translate(new Vec3(0, depth / n.y, 0));
                if (velocity.y < 0)
                {
                    impactSpeed = JsMath.Max(impactSpeed, -velocity.y);
                    velocity.y = 0;
                }
                if (move.y < 0) move.y = 0;
                grounded = true;
                groundNormal = n;
                return;
            }

            // NIGHT5 F6 (Celeste-style corner correction): rising into a ceiling's edge with the head's rim only just under it
            // (the edge within collision.cornerCorrect x the radius of the head's axis), slide past it sideways and keep the
            // jump, instead of losing the vertical speed to the edge. A flat ceiling (the edge near the axis) blocks as before.
            if (!feet && n.y < 0 && velocity.y > 0 && point.y > capsule.end.y)
            {
                double dx = capsule.end.x - point.x;
                double dz = capsule.end.z - point.z;
                double dh = JsMath.Hypot(dx, dz);
                double nudge = capsule.radius - dh;
                if (dh > 1e-6 && nudge <= capsule.radius * c.collision.cornerCorrect)
                {
                    double k = (nudge + c.collision.cornerSkin) / dh;
                    capsule.Translate(new Vec3(dx * k, 0, dz * k));
                    return;
                }
            }

            if (!feet && n.y > 0)
            {
                // An upward normal at head height (an overhang edge): push sideways only, never lift.
                n.y = 0;
                if (n.x * n.x + n.z * n.z == 0) return;
                n = NormalizeJs(n);
            }

            capsule.Translate(n * depth);
            velocity = ClipInto(velocity, n);
            move = ClipInto(move, n);
        }

        /// <summary>Removes the part of `vec` that points into a surface with normal `n`.</summary>
        static Vec3 ClipInto(Vec3 vec, Vec3 n)
        {
            double into = Vec3.Dot(vec, n);
            if (into < 0) vec += n * -into;
            return vec;
        }

        // three.js Vector3.normalize: multiplyScalar(1 / (length || 1)).
        static Vec3 NormalizeJs(Vec3 v) => Vec3.Normalize(v);

        /// <summary>True if any contact exists for a capsule (the stand-up and probe tests).</summary>
        bool AnyContact(in Capsule cap, bool floorOnly)
        {
            int count = world.GatherCandidates(cap);
            for (int i = 0; i < count; i++)
            {
                if (!world.CapsuleIntersect(cap, i, out CapsuleContact contact)) continue;
                if (!floorOnly) return true;
                bool feet = contact.point.y < cap.start.y + cap.radius;
                if (feet && contact.normal.y >= config.surface.floorMinNormalY) return true;
            }
            return false;
        }

        /// <summary>
        /// Wall probe: moves a copy of the capsule probeDistance along −normal and returns the valid wall normal it hits
        /// that best matches `normal`. With minDot > −1, weaker matches count as no hit.
        /// </summary>
        bool ProbeWall(Vec3 normal, double minDot, out Vec3 result)
        {
            MovementConfig c = config;
            probe = capsule;
            probe.Translate(normal * -c.wallRide.probeDistance);
            int count = world.GatherCandidates(probe);
            double best = double.NegativeInfinity;
            for (int i = 0; i < count; i++)
            {
                if (!world.CapsuleIntersect(probe, i, out CapsuleContact contact) || !IsValidWall(contact.normal, contact.point, probe)) continue;
                Vec3 n = NormalizeJs(new Vec3(contact.normal.x, 0, contact.normal.z));
                double d = Vec3.Dot(n, normal);
                if (d > best)
                {
                    best = d;
                    probeNormal = n;
                }
            }
            result = probeNormal;
            return best > minDot;
        }

        /// <summary>Is a valid wall close enough to wall jump off? Sets nearWallNormal (SPEC 1.3).</summary>
        bool FindNearWall()
        {
            if (touchingWall)
            {
                nearWallNormal = wallNormal;
                return true;
            }
            if (!hasLastWall) return false;
            if (!ProbeWall(lastWallNormal, JsMath.Cos((config.wallRide.cornerAngleDeg * Math.PI) / 180), out Vec3 hit)) return false;
            nearWallNormal = hit;
            return true;
        }

        /// <summary>NIGHT34 flow34 slideBounce34 (only called with the rule on, a valid wall near, the slide heading into it): the slide's wall jump.
        /// The horizontal velocity: the slide direction reflected off the wall plus push x the normal, renormalised, at the full speed (the larger of
        /// the speed now and the speed before the wall clipped it); vertical: at least the wall jump's up speed. The rest is the wall jump's bookkeeping.</summary>
        void Flow34SlideBounce(MoveState s)
        {
            MovementConfig c = config;
            Vec3 n = nearWallNormal;
            double speed = JsMath.Max(Speed, preMoveSpeed);
            Vec3 d = Flow34.BounceDir(slideDir, n, flow34.rules.slideBouncePush);
            double up = MovementMath.JumpVelocity(c.jump.height, c.jump.apexTime) * c.wallJump.v0Mult;
            if (s == MoveState.Dash)
            {
                RecordDashDistance();
                dashSteps = 0;
                invulnerable = false;
            }
            if (s == MoveState.Slide) Emit(MoverEventType.SlideEnd);
            bool fromGround = grounded;
            velocity = new Vec3(d.x * speed, JsMath.Max(velocity.y, up), d.z * speed);
            wallJumpsLeft--;
            if (m43.p.Any) Chimney43Mark(n); // NIGHT43 chimney43: this wall is the last wall
            doubleJumpsLeft = c.jump.doubleJumps;
            wallJumpLockSteps = MovementMath.StepsFor(c.wallRide.rejoinLockTime, 1 / c.loop.simHz);
            wallJumpLockNormal = n;
            rideLockActive = false;
            flow34.bounces++;
            flow34.sinceSlide = int.MaxValue / 2;
            Emit(new MoverEvent { type = MoverEventType.WallJump, speed = Speed, wallJumpsLeft = wallJumpsLeft });
            if (fromGround) BeginAirtime(position.y, true);
            jumpAirtime = true;
            jumpCutArmed = false;
            bufferSteps = 0;
            coyoteSteps = 0;
            grounded = false;
            state = MoveState.Air;
        }

        /// <summary>NIGHT35 wallride35 (only with the row on), before the web's actions: on a ride, crouch drops off (no boost, no slam: the press is
        /// spent) and a hold ride's released jump is the jump-off; in the air inside the late window, a jump press (or a hold ride's release) is the
        /// jump-off from the wall that ride ended on.</summary>
        void Ride35Actions()
        {
            ref Ride35.State r = ref flow34.ride35;
            if (state == MoveState.WallRide)
            {
                if (input.slide.pressed)
                {
                    r.drops++;
                    EndRide("drop");
                    input.slide.pressed = false;
                    return;
                }
                if (climb47.pendingJumpOff > 0) // NIGHT47 climb47: BUFFER: the jump let go just before this climb ride began is its jump-off
                {
                    climb47.pendingJumpOff = 0;
                    climb47.buffered++;
                    Ride35JumpOff(rideNormal, false);
                    return;
                }
                if (r.held && !input.jump.held) Ride35JumpOff(rideNormal, false);
                return;
            }
            if (state != MoveState.Air || r.lateLeft <= 0) return;
            if (!input.jump.pressed && !(r.lateHeld && !input.jump.held)) return;
            Ride35JumpOff(r.lateNormal, true);
        }

        /// <summary>NIGHT35 wallride35 (in the air, not touching a wall, jump held): a valid wall beside the motion (a ray left and right of the
        /// horizontal velocity from the capsule's middle) within the capsule radius + attachReach, that a hold ride could start on (the locks, the
        /// re-ride rule, the lowered gate), draws the player onto it: the speed into it is raised to attachPull for this step (the contact then
        /// starts the hold ride). Lucio's "jump at a nearby wall and hold": a near miss rides.</summary>
        void Ride35Attach(double cosCorner)
        {
            ref Ride35.State r = ref flow34.ride35;
            double reach = r.r40.on && r.r40.attachReach >= 0 ? r.r40.attachReach : r.rules.attachReach; // NIGHT40 ride40: a shorter near-miss draw
            bool held = input.jump.held;
            bool tap = !held && m43.p.ride && m43.p.tapDraw; // NIGHT43 ride43 (a): the draw on a tap ride too
            if (held && m43.p.ride && m43.p.holdReach > reach) reach = m43.p.holdReach; // NIGHT43 ride43 (a', round 2): jump held reaches farther (THE FLUE's lane)
            if ((!held && !tap) || touchingWall || reach <= 0) return;
            double hs = JsMath.Hypot(velocity.x, velocity.z);
            if (hs < 1) return;
            double ux = velocity.x / hs, uz = velocity.z / hs;
            Vec3 mid = new Vec3((capsule.start.x + capsule.end.x) * 0.5, (capsule.start.y + capsule.end.y) * 0.5, (capsule.start.z + capsule.end.z) * 0.5);
            double gate = held ? r.rules.entrySpeed : config.wallRide.entrySpeed; // a tap ride's entry gate (Ride35Entry)
            for (int k = -1; k <= 1; k += 2)
            {
                if (!Ride35DrawOnto(mid, new Vec3(-uz * k, 0, ux * k), capsule.radius + reach, cosCorner, gate)) continue;
                if (tap) m43.tapDraws++;
                return;
            }
            if (climb47.p.on && climbWall != null && climb47.p.catchReach > reach) // NIGHT47 climb47: REACH: a farther draw, onto a climb wall only
            {
                for (int k = -1; k <= 1; k += 2)
                {
                    Vec3 d = new Vec3(-uz * k, 0, ux * k);
                    if (!world.RayIntersect(mid, d, out RayHit ch) || ch.distance > capsule.radius + climb47.p.catchReach) continue;
                    if (double.IsNaN(Climb47Bottom(NormalizeJs(new Vec3(ch.normal.x, 0, ch.normal.z))))) continue;
                    if (!Ride35DrawOnto(mid, d, capsule.radius + climb47.p.catchReach, cosCorner, gate)) continue;
                    climb47.reachDraws++;
                    return;
                }
            }
            if (!m43.p.ride || m43.p.attachReach <= 0) return;
            // NIGHT43 ride43 (a): swept: ahead-diagonal of the motion, attachReach past the capsule plus this step's move
            double sweep = capsule.radius + m43.p.attachReach + hs / config.loop.simHz;
            for (int k = -1; k <= 1; k += 2)
            {
                double dx = ux - uz * k, dz = uz + ux * k, dl = JsMath.Hypot(dx, dz);
                if (!Ride35DrawOnto(mid, new Vec3(dx / dl, 0, dz / dl), sweep, cosCorner, gate)) continue;
                m43.sweptDraws++;
                if (tap) m43.tapDraws++;
                return;
            }
        }

        /// <summary>NIGHT35 wallride35's near-miss draw along one ray (d, unit, horizontal) from the capsule's middle: a valid wall within maxDist that
        /// a ride could start on (the locks, the re-ride rule, the gate along the wall, ride40's intent, no noride) draws the player onto it (the speed
        /// into it raised to attachPull this step). True when drawn.</summary>
        bool Ride35DrawOnto(Vec3 mid, Vec3 d, double maxDist, double cosCorner, double gate)
        {
            ref Ride35.State r = ref flow34.ride35;
            if (!world.RayIntersect(mid, d, out RayHit hit) || hit.distance > maxDist) return false;
            if (Math.Abs(hit.normal.y) >= config.surface.wallMaxAbsNormalY || Vec3.Dot(hit.normal, d) >= 0) return false;
            Vec3 n = NormalizeJs(new Vec3(hit.normal.x, 0, hit.normal.z));
            if (RideLock43(n, cosCorner)) return false;
            if (WallJumpLock43(n, cosCorner)) return false;
            if (ReRide43(n)) return false;
            if (MovementMath.AlongWallSpeed(velocity, n) <= gate) return false;
            if (r.r40.on && !Ride40Intent(n, hit.distance, false, true)) return false; // NIGHT40 ride40: the draw asks for intent too (NIGHT43 ride43 round 2: always the stick, never (e)'s waiver)
            if (r.noRide != null && Ride38.NoRideAt(r.noRide, new Vec3(mid.x + d.x * hit.distance, mid.y, mid.z + d.z * hit.distance), config.wallRide.probeDistance)) // NIGHT38 noride: never drawn onto it
            {
                r.r38NoRideBlocks++;
                return false;
            }
            double into = -(velocity.x * n.x + velocity.z * n.z);
            if (into < r.rules.attachPull)
            {
                velocity.x -= n.x * (r.rules.attachPull - into);
                velocity.z -= n.z * (r.rules.attachPull - into);
            }
            r.attaches++;
            return true;
        }

        /// <summary>NIGHT35 wallride35: the jump-off from the wall with normal n: the web's wall jump shape (the along speed kept, rules.outSpeed out)
        /// with the up speed from the look pitch and the exit boost; the double jump refreshed; the airtime's wall jumps not spent.</summary>
        void Ride35JumpOff(Vec3 n, bool late)
        {
            MovementConfig c = config;
            ref Ride35.State r = ref flow34.ride35;
            double up = Ride35.JumpUp(input.pitch, r.rules);
            Vec3 v = r.rules.capJumpOff39 // NIGHT39 ride39 (only with the row on): the jump-off's whole speed held to chainCap (or the ride's own, if faster)
                ? Ride35.Boost39(MovementMath.WallJumpVelocity(velocity, n, r.rules.outSpeed, up), r.rules, r.rideSpeed)
                : Ride35.Boost(MovementMath.WallJumpVelocity(velocity, n, r.rules.outSpeed, up), r.rules);
            if (wall42.on && r.rideAge < wall42.rideShortSteps) // NIGHT42 wallfling42: a short ride's jump-off: pitch rule and along speed kept, its speed out from the wall capped
            {
                Vec3 capped = Wall42.CapOut(v, n, wall42.rideOutMax);
                if (capped.x != v.x || capped.z != v.z) wall42Capped++;
                v = capped;
            }
            if (climb47.p.on && !double.IsNaN(Climb47Bottom(n))) v.y = Climb47Up(n, v.y); // NIGHT47 climb47: SET UP (guided to the next hold) off a climb wall
            if (late && climb47.p.on) climb47.coyoteJumps++; // NIGHT47 climb47: a late jump-off (the coyote, lengthened on a climb wall)
            if (state == MoveState.WallRide) EndRide("jump");
            velocity = v;
            doubleJumpsLeft = c.jump.doubleJumps;
            wallJumpLockSteps = MovementMath.StepsFor(c.wallRide.rejoinLockTime, 1 / c.loop.simHz);
            wallJumpLockNormal = n;
            rideLockActive = false;
            r.lateLeft = 0;
            r.jumpOffs++;
            if (m43.p.Any) Chimney43Mark(n); // NIGHT43 chimney43: the jump-off's wall is the last wall (no spend)
            if (m43.p.Any) m43.sinceRide = 0; // NIGHT43 ride43 (d) / chimney43's chain: the wall just left (a late jump-off too)
            if (late) r.lateJumps++;
            r.lastUp = up;
            r.lastSpeed = JsMath.Hypot(v.x, v.z);
            jumpAirtime = true;
            jumpCutArmed = false;
            bufferSteps = 0;
            coyoteSteps = 0;
            grounded = false;
            state = MoveState.Air;
            Emit(new MoverEvent { type = MoverEventType.WallJump, speed = Speed, wallJumpsLeft = wallJumpsLeft });
        }

        /// <summary>NIGHT35 wallride35: CheckRide with the row on (rideSteps already ticked). The safety cap (a hold ride) or the web's timer; after
        /// cornerMinHold, an inside corner (a contact ahead) or an outside corner (the wall ends: the next face behind the ride's direction) up to
        /// followMaxDeg turns the ride onto it; the normal is re-sampled every step (a curved wall); the gate ends a ride too slow along the wall.</summary>
        void Ride35Check(double cosCorner)
        {
            ref Ride35.State r = ref flow34.ride35;
            Vec3 n = rideNormal;
            if (rideSteps <= 0)
            {
                EndRide(r.held || r.r40.on ? "safety" : "timeout"); // NIGHT40 ride40: a tap ride's end here is the stuck guard too
                return;
            }
            r.rideAge++;
            bool follow = r.rideAge >= r.rules.cornerMinSteps;
            for (int i = 0; i < wallHitCount; i++)
            {
                double d = Vec3.Dot(wallHits[i], n);
                if (d >= cosCorner) continue;
                if (wallHits[i].x * rideDir.x + wallHits[i].z * rideDir.z > -0.1) continue; // not ahead (the wall a turn just left, behind)
                if (follow && d >= r.rules.cosFollowMax) Ride35Turn(wallHits[i], true);
                else EndRide("corner");
                return;
            }
            if (!ProbeWall(n, -1, out Vec3 hit))
            {
                if (follow && Ride35OutsideFace(n, out Vec3 back, out double past))
                {
                    r.lostSteps = 0;
                    if (past >= capsule.radius) Ride35Turn(back, true); // the edge cleared: around the corner
                    return; // still clearing the edge: straight on
                }
                if (r.lostSteps < (m43.p.ride ? m43.p.lostGraceSteps : r.rules.lostGraceSteps)) // NIGHT43 ride43 (c): the longer lost grace
                {
                    r.lostSteps++;
                    return;
                }
                EndRide("lost");
                return;
            }
            if (m43.p.ride && r.lostSteps > r.rules.lostGraceSteps) m43.graceSaves++; // NIGHT43 ride43 (c): a ride wallride35's grace would have ended
            r.lostSteps = 0;
            double dh = Vec3.Dot(hit, n);
            if (dh < cosCorner)
            {
                if (follow && dh >= r.rules.cosFollowMax) Ride35Turn(hit, true);
                else EndRide("corner");
                return;
            }
            if (dh < 1 - 1e-9) Ride35Turn(hit, false); // a curved or faceted wall: follow its normal
            double gate = r.held ? r.rules.entrySpeed : config.wallRide.entrySpeed;
            if (MovementMath.AlongWallSpeed(velocity, rideNormal) < gate)
            {
                EndRide("blocked");
                return;
            }
            wallNormal = rideNormal;
        }

        /// <summary>NIGHT35 wallride35: an outside corner's next face: a ray back along the ride (-rideDir) from the capsule's middle moved just past
        /// the ridden wall's plane (r + 0.1 m along -n) hits a valid wall facing the ray within r + 0.3 m, turned at most followMaxDeg from n.
        /// `past` = how far the capsule's middle is past that face's plane (the edge is cleared at the capsule radius).</summary>
        bool Ride35OutsideFace(Vec3 n, out Vec3 face, out double past)
        {
            face = default;
            past = 0;
            double r = capsule.radius;
            Vec3 mid = new Vec3((capsule.start.x + capsule.end.x) * 0.5, (capsule.start.y + capsule.end.y) * 0.5, (capsule.start.z + capsule.end.z) * 0.5);
            Vec3 o = new Vec3(mid.x - n.x * (r + 0.1), mid.y, mid.z - n.z * (r + 0.1));
            Vec3 dir = new Vec3(-rideDir.x, 0, -rideDir.z);
            if (!world.RayIntersect(o, dir, out RayHit hit) || hit.distance > r + 0.3) return false;
            if (Math.Abs(hit.normal.y) >= config.surface.wallMaxAbsNormalY || Vec3.Dot(hit.normal, dir) >= 0) return false;
            face = NormalizeJs(new Vec3(hit.normal.x, 0, hit.normal.z));
            if (Vec3.Dot(face, n) < flow34.ride35.rules.cosFollowMax) return false;
            past = hit.distance * -(face.x * dir.x + face.z * dir.z);
            return true;
        }

        /// <summary>NIGHT35 wallride35: the ride turns onto the wall with normal nn at its speed (the larger of the speed now and the ride's
        /// own, so an inside corner's clip loses nothing): the new wall's tangent on the side the ride was going (around an outside corner, away
        /// from an inside one).</summary>
        void Ride35Turn(Vec3 nn, bool corner)
        {
            Vec3 old = rideNormal;
            double s = nn.x * rideDir.x + nn.z * rideDir.z;
            double k = s > 0 ? -0.5 : 0.5;
            Vec3 t = Ride35.Tangent(nn, new Vec3(rideDir.x + old.x * k, 0, rideDir.z + old.z * k));
            if ((t.x == 0 && t.z == 0) || (corner && flow34.ride35.noRide != null && NoRide38(nn))) // NIGHT38 noride: a corner onto a noride face ends the ride
            {
                EndRide("corner");
                return;
            }
            double sp = JsMath.Hypot(velocity.x, velocity.z);
            if (corner)
            {
                sp = JsMath.Max(sp, flow34.ride35.rideSpeed);
                flow34.ride35.corners++;
            }
            rideNormal = nn;
            wallNormal = nn;
            rideDir = t;
            velocity.x = t.x * sp;
            velocity.z = t.z * sp;
        }

        /// <summary>NIGHT38 ride38 (a HOLD ride with the row on, after wallride35 / wallgrip36's gravity): once a rising entry has braked, the
        /// vertical speed is the look pitch's drift (Ride38.Drift: 0 inside the dead zone) with no gravity; a rise is cut to 0 when the capsule's
        /// upper sample (the top of the valid contact band) moved up by this step's rise has no wall toward the ridden wall within the radius +
        /// topProbe (the wall's top: the band stays on the wall).</summary>
        void Ride38Hold(double dt)
        {
            ref Ride35.State r = ref flow34.ride35;
            if (r.r38Rising)
            {
                if (velocity.y > 0)
                {
                    // NIGHT43 ride43 (b, round 2): the eased snap keeps a rise, so the wall's top cuts it too (it carried a jump past a short
                    // panel's top: the ride lost its wall and the player fell)
                    if (m43.p.ride)
                    {
                        MovementConfig c0 = config;
                        double rad0 = c0.body.capsuleRadius;
                        Vec3 n0 = rideNormal;
                        Vec3 o0 = new Vec3(position.x, position.y + c0.body.capsuleHeight - rad0 + velocity.y * dt, position.z);
                        if (!world.RayIntersect(o0, new Vec3(-n0.x, 0, -n0.z), out RayHit h0) || h0.distance > rad0 + r.r38.topProbe || Math.Abs(h0.normal.y) >= c0.surface.wallMaxAbsNormalY)
                        {
                            velocity.y = 0;
                            gravityMultiplier = 0;
                            r.r38Rising = false;
                            r.r38TopHolds++;
                            m43.topCuts++;
                        }
                    }
                    return; // wallride35 / wallgrip36's brake on a rising entry
                }
                r.r38Rising = false;
            }
            gravityMultiplier = 0;
            double vy = Ride38.Drift(input.pitch, r.r38);
            if (vy > 0)
            {
                MovementConfig c = config;
                double rad = c.body.capsuleRadius;
                Vec3 n = rideNormal;
                Vec3 o = new Vec3(position.x, position.y + c.body.capsuleHeight - rad + vy * dt, position.z);
                if (!world.RayIntersect(o, new Vec3(-n.x, 0, -n.z), out RayHit hit) || hit.distance > rad + r.r38.topProbe || Math.Abs(hit.normal.y) >= c.surface.wallMaxAbsNormalY)
                {
                    vy = 0;
                    r.r38TopHolds++;
                }
            }
            if (climb47.catchLeft > 0) // NIGHT47 climb47: THE CATCH: eased to the panel's catch line over catchEase, up or down (a speed, so the contact resolves it)
            {
                vy = (climb47.catchTarget - position.y) / (climb47.catchLeft * dt);
                climb47.catchLeft--;
            }
            velocity.y = vy;
        }

        /// <summary>NIGHT43 ride43 (round 2, the lead's card: G13 at the root): the ride lock (an ended ride's wall, until out of probe reach) blocks the
        /// wall with normal n only when it is the SAME wall (Move43.SameWall from where that ride ended); off: NIGHT42's facing test alone.</summary>
        bool RideLock43(Vec3 n, double cosCorner) =>
            rideLockActive && Vec3.Dot(n, rideLockNormal) >= cosCorner && (!m43.p.ride || !R35 || Move43.SameWall(m43.p, rideLockNormal, flow34.ride35.lastEnd, n, position));

        /// <summary>NIGHT43 ride43 (round 2): the wall-jump lock (rejoinLockTime after a wall jump / jump-off) blocks only the SAME wall (from where the
        /// jump left: m43.wallAt); off: NIGHT42's facing test alone.</summary>
        bool WallJumpLock43(Vec3 n, double cosCorner) =>
            wallJumpLockSteps > 0 && Vec3.Dot(n, wallJumpLockNormal) >= cosCorner && (!m43.p.ride || !m43.hasWall || Move43.SameWall(m43.p, wallJumpLockNormal, m43.wallAt, n, position));

        /// <summary>NIGHT35 wallride35's re-ride rule (a wall facing within reRideDeg of the last ridden one, within reRideDistance m of where that ride
        /// ended, until a landing) or, with NIGHT43 ride43 on (round 2), Move43.SameWall: a wall sameWallRise m higher (THE STACK's panel a turn up,
        /// a chimney) or sameWallDrop m lower (falling back down it) is not the same wall.</summary>
        bool ReRide43(Vec3 n)
        {
            ref Ride35.State r = ref flow34.ride35;
            if (!r.hasLast) return false;
            if (m43.p.ride) return Move43.SameWall(m43.p, r.lastNormal, r.lastEnd, n, position);
            return Vec3.Dot(n, r.lastNormal) >= r.rules.cosReRide && JsMath.Hypot(position.x - r.lastEnd.x, position.z - r.lastEnd.z) < r.rules.reRideDistance;
        }

        /// <summary>NIGHT40 ride40 (inside wallride35; only with the row on): may a ride start on the wall with normal n, `dist` m from the capsule's
        /// axis? The same-wall lockout; the horizontal speed before the wall clipped it; the stick toward the wall; the ground below the feet (a ray
        /// down from the feet); the wall's face still there minWallHeight above the feet (a ray toward the wall from the axis at that height, within
        /// dist + the ride probe + 0.1 m). Each refusal is counted by rule.</summary>
        bool Ride40Intent(Vec3 n, double dist, bool dash = false, bool draw = false)
        {
            ref Ride35.State r = ref flow34.ride35;
            Ride40.Params p = r.r40;
            MovementConfig c = config;
            if (r.r40LockLeft > 0 && Vec3.Dot(n, r.r40LockNormal) >= r.rules.cosReRide)
            {
                if (m43.p.ride && !Move43.SameWall(m43.p, r.r40LockNormal, r.lastEnd, n, position)) m43.planeSkips++; // NIGHT43 ride43 (f, round 2): another wall (Move43.SameWall), not locked
                else
                {
                    r.r40Locked++;
                    return false;
                }
            }
            if (!dash && p.minSpeed > 0 && JsMath.Max(preMoveSpeed, Speed) < p.minSpeed) // NIGHT43 ride43 (g): a dash is fast enough
            {
                r.r40Slow++;
                return false;
            }
            if (!dash && p.inputDot > -1 && (wishSpeed <= 0 || -(wish.x * n.x + wish.z * n.z) < p.inputDot) && !(climb47.p.on && climbWall != null && input.jump.held && !double.IsNaN(Climb47Bottom(n)))) // NIGHT43 ride43 (g): the dash into the wall is the intent; NIGHT47 climb47: on a climb wall, jump held is the intent (REACH)
            {
                // NIGHT43 ride43 (e): jump held, moving along the wall (not away from it, the stick not away) is intent enough
                if (m43.p.ride && m43.p.holdNoStick && !draw && input.jump.held && velocity.x * n.x + velocity.z * n.z <= 1e-6 && (wishSpeed <= 0 || wish.x * n.x + wish.z * n.z <= c.wallRide.awayDot))
                    m43.noStickHolds++;
                else
                {
                    r.r40NoIntent++;
                    return false;
                }
            }
            if (p.minClearance > 0 && !(climb47.p.on && climbWall != null && !grounded && input.jump.held && !double.IsNaN(Climb47Bottom(n))) // NIGHT47 climb47: a jump at a climb wall rides it off the floor (REACH)
                && world.RayIntersect(new Vec3(position.x, position.y + 0.01, position.z), new Vec3(0, -1, 0), out RayHit down) && down.distance - 0.01 < p.minClearance)
            {
                r.r40Low++;
                return false;
            }
            if (p.minWallHeight > 0)
            {
                double wallH = climb47.p.on && climb47.p.faceAbove > 0 && climbWall != null && input.jump.held && !double.IsNaN(Climb47Bottom(n)) ? JsMath.Min(p.minWallHeight, climb47.p.faceAbove) : p.minWallHeight; // NIGHT47 climb47: a climb panel asks less face over the feet
                Vec3 o = new Vec3(position.x, position.y + wallH, position.z);
                if (!world.RayIntersect(o, new Vec3(-n.x, 0, -n.z), out RayHit face) || face.distance > dist + c.wallRide.probeDistance + 0.1 || Math.Abs(face.normal.y) >= c.surface.wallMaxAbsNormalY)
                {
                    r.r40Short++;
                    return false;
                }
            }
            return true;
        }

        /// <summary>NIGHT43 ride43 (g) (DOOM Eternal's climb walls; research/movement-web.md B7): a dash in the air touching a valid wall it heads into
        /// (dashDir . -n >= dashIntoDot) ends and attaches: the locks, the re-ride rule, noride and ride40's clearance / wall height as any attach
        /// (no stick, no speed check: the dash is the intent); the ride goes along the wall the way the dash (else the camera) points, at the dash's
        /// speed, held to the ride chain's cap (wallride35 / ride39 chainCap 16; never the 24 m/s dash itself, never under the dash's exit speed).</summary>
        void Ride43DashAttach(double cosCorner)
        {
            ref Ride35.State r = ref flow34.ride35;
            Vec3 n = wallNormal;
            if (-(dashDir.x * n.x + dashDir.z * n.z) < m43.p.dashIntoDot) return;
            if (RideLock43(n, cosCorner)) return;
            if (WallJumpLock43(n, cosCorner)) return;
            if (r.noRide != null && NoRide38(n)) return;
            if (ReRide43(n)) return;
            if (r.r40.on && !Ride40Intent(n, capsule.radius, true)) return;
            double into = dashDir.x * n.x + dashDir.z * n.z;
            double tx = dashDir.x - into * n.x, tz = dashDir.z - into * n.z, tl = JsMath.Hypot(tx, tz);
            if (tl < 0.2)
            {
                double f = forward.x * n.x + forward.z * n.z;
                tx = forward.x - f * n.x;
                tz = forward.z - f * n.z;
                tl = JsMath.Hypot(tx, tz);
                if (tl < 0.2) return; // straight at the wall, looking at it: no side to ride to
            }
            double exit = MovementMath.DashExitSpeed(config.ground.walkSpeed, dashSpeedBefore);
            exit = JsMath.Max(exit, JsMath.Min(dashSpeedNow, r.rules.chainCap)); // the dash's speed carried onto the wall, held to the ride chain's cap (ride39 16)
            EndDash(input);
            if (state != MoveState.Air) return;
            velocity.x = tx / tl * exit;
            velocity.z = tz / tl * exit;
            preMoveSpeed = exit; // StartRide keeps max(speed, preMoveSpeed): the exit speed, not the dash's
            m43.dashAttaches++;
            StartRide(n);
        }

        /// <summary>NIGHT38 noride (inside wallride35; only with the level's noride boxes set): is the wall with normal n beside the capsule a
        /// noride box? The capsule's contact band (its cylinder section) pushed one radius toward the wall, the boxes grown by the ride probe.</summary>
        bool NoRide38(Vec3 n)
        {
            ref Ride35.State r = ref flow34.ride35;
            if (r.noRide == null) return false;
            double rad = capsule.radius;
            if (!Ride38.NoRideBand(r.noRide, capsule.start.x - n.x * rad, capsule.start.z - n.z * rad, capsule.start.y, capsule.end.y, config.wallRide.probeDistance)) return false;
            r.r38NoRideBlocks++;
            return true;
        }

        /// <summary>NIGHT34 flow34 wallBuffer (only called with the rule on): is a valid wall ahead along the horizontal velocity within `seconds`?
        /// A ray from the capsule's middle along the horizontal velocity hits a wall (|normal.y| under surface.wallMaxAbsNormalY, facing the move
        /// more head-on than the ride's away rule) within speed x seconds + the capsule radius + the ride probe.</summary>
        bool Flow34WallSoon(double seconds)
        {
            double hs = JsMath.Hypot(velocity.x, velocity.z);
            if (hs < 1e-6) return false;
            Vec3 dir = new Vec3(velocity.x / hs, 0, velocity.z / hs);
            Vec3 mid = new Vec3((capsule.start.x + capsule.end.x) * 0.5, (capsule.start.y + capsule.end.y) * 0.5, (capsule.start.z + capsule.end.z) * 0.5);
            if (!world.RayIntersect(mid, dir, out RayHit hit)) return false;
            if (hit.distance > hs * seconds + capsule.radius + config.wallRide.probeDistance) return false;
            if (Math.Abs(hit.normal.y) >= config.surface.wallMaxAbsNormalY) return false;
            Vec3 n = NormalizeJs(new Vec3(hit.normal.x, 0, hit.normal.z));
            if (m43.p.ride) // NIGHT43 ride43 (i): a shallow approach (from bufferDeg off the wall line) holds the press for the wall too
            {
                if (Vec3.Dot(n, dir) < -m43.p.sinBuffer)
                {
                    if (Vec3.Dot(n, dir) >= -config.wallRide.awayDot) m43.shallowHolds++;
                    return true;
                }
                return false;
            }
            return Vec3.Dot(n, dir) < -config.wallRide.awayDot;
        }

        /// <summary>Ground snap: if floor is within groundSnap below the feet, step down onto it.</summary>
        void SnapToGround()
        {
            MovementConfig c = config;
            if (!world.RayIntersect(capsule.start, new Vec3(0, -1, 0), out RayHit hit)) return;
            if (hit.normal.y < c.surface.floorMinNormalY) return;
            double gap = hit.distance - capsule.radius;
            if (gap < 0 || gap > c.collision.groundSnap) return;
            capsule.Translate(new Vec3(0, -gap, 0));
            position.y -= gap;
            groundNormal = hit.normal;
            grounded = true;
            if (velocity.y > 0) velocity.y = 0;
        }

        /// <summary>Is there floor within `distance` below the feet? (DASH has no gravity to find it.)</summary>
        bool ProbeFloor(double distance)
        {
            probe = capsule;
            probe.Translate(new Vec3(0, -distance, 0));
            return AnyContact(probe, true);
        }

        /// <summary>Can the full-height capsule fit? Tests only the part above the sliding capsule, slightly thinner.</summary>
        bool HasHeadroom()
        {
            MovementConfig c = config;
            double r = c.body.capsuleRadius;
            probe.radius = r - c.collision.headroomSkin;
            probe.start = new Vec3(position.x, position.y + c.slide.colliderHeight - r, position.z);
            probe.end = new Vec3(position.x, position.y + c.body.capsuleHeight - r, position.z);
            return !AnyContact(probe, false);
        }

        /// <summary>Gravity multiplier to assume for landing predictions: the one in effect right now.</summary>
        double PredictScale()
        {
            var flags = new GravityFlags { jumpHeld = input.jump.held, jumpAirtime = jumpAirtime, windActive = windSteps > 0 };
            return MovementMath.GravityScale(MoveState.Air, velocity.y, flags, config);
        }

        /// <summary>Predicts a floor landing within `seconds` from a ray straight down (SPEC 1.2 buffer rule).</summary>
        bool LandingWithin(double seconds, double gravity)
        {
            if (!world.RayIntersect(capsule.start, new Vec3(0, -1, 0), out RayHit hit)) return false;
            if (hit.normal.y < config.surface.floorMinNormalY) return false;
            double gap = hit.distance - capsule.radius;
            return gap <= MovementMath.FallDistance(velocity.y, gravity, seconds);
        }
    }
}
