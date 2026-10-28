// The movement sections of skygrind/src/config.js (SPEC.md §3.1–§3.6, §5), values copied verbatim.
// Units: meters, seconds, m/s, degrees. (S) = the web game's own starting guess.
// Field names match the JS keys so a value can be checked against config.js by eye. Mutable on purpose, like the
// web config the lil-gui panel edits; tests create a fresh instance.
using System;

namespace Skygrind.Core
{
    [Serializable]
    public sealed class MovementConfig
    {
        public LoopConfig loop = new LoopConfig();
        public CollisionConfig collision = new CollisionConfig();
        public BodyConfig body = new BodyConfig();
        public SurfaceConfig surface = new SurfaceConfig();
        public SpeedConfig speed = new SpeedConfig();
        public GroundConfig ground = new GroundConfig();
        public AirConfig air = new AirConfig();
        public JumpConfig jump = new JumpConfig();
        public DashConfig dash = new DashConfig();
        public SlideConfig slide = new SlideConfig();
        public DashJumpConfig dashJump = new DashJumpConfig();
        public SlamConfig slam = new SlamConfig();
        public BoostJumpConfig boostJump = new BoostJumpConfig();
        public WallRideConfig wallRide = new WallRideConfig();
        public WallJumpConfig wallJump = new WallJumpConfig();
        public JumpPadConfig jumpPad = new JumpPadConfig();
        public ArclineConfig arcline = new ArclineConfig();
        public CoreConfig core = new CoreConfig();
        public EndlessConfig endless = new EndlessConfig();
        public HuskConfig husk = new HuskConfig();
        public AiConfig ai = new AiConfig();
        // The camera side of the feel (player.js render, keyboardMouse.js look).
        public MouseConfig mouse = new MouseConfig();
        public CameraConfig camera = new CameraConfig();
        public FeelConfig feel = new FeelConfig();
        public SettingsConfig settings = new SettingsConfig();
    }

    // The core "Grinder" (SPEC 2.0 §5.2)
    [Serializable]
    public sealed class CoreConfig
    {
        public double maxHeight = 6; // m, tallest core pillar
        public double heightStep = 1.5; // m, pillar heights are multiples of this (jumpable steps)
        public double minHeight = 0.02; // m, pillars at or below this have no collision (the floor)
        public double carryTolerance = 0.15; // m, feet this close to a moving pillar top ride with it
        public double floorThickness = 2; // m, the platform slab under the map
    }

    // The Husk's chase movement (enemies/husk.js; the enemy scale bench uses it)
    [Serializable]
    public sealed class HuskConfig
    {
        public double speed = 6; // m/s chase on the ground
        public double stopDistance = 1.0; // m, Husks stop this far from the player
        public double radius = 0.4; // m capsule
        public double height = 1.8; // m capsule
        public double spawnLift = 0.05; // m above the requested point, so a floor spawn doesn't start in contact
        public double separationRadius = 1.2; // m, Husks push apart inside this
        public double separationSpeed = 3; // m/s push at full overlap
    }

    [Serializable]
    public sealed class AiConfig
    {
        public double edgeNormalY = 0.25; // a summed contact normal this upright (floor + wall, a ledge corner) ...
        public double edgeRiseSpeed = 0.5; // m/s: ... counts as ground for an enemy not rising faster than this
        public double supportedDrop = 0.002; // m: an enemy "falling" but held this still in a step is supported
    }

    [Serializable]
    public sealed class EndlessConfig
    {
        public double reshapeTime = 1.5; // s, a core reshape
    }

    // §2 Mouse look
    [Serializable]
    public sealed class MouseConfig
    {
        public double pitchLimitDeg = 89; // look up/down clamp
    }

    // §3.5 Camera + FOV
    [Serializable]
    public sealed class CameraConfig
    {
        public double fovBoostDeg = 8; // extra FOV at full boost speed (S)
        public double fovBoostStartSpeed = 8; // m/s where widening starts (walk)
        public double fovBoostFullSpeed = 24; // m/s where widening is full (dash)
        public double fovEaseRate = 8; // 1/s, exponential ease toward the speed FOV target (S)
        public double dashFovKickDeg = 10; // extra FOV during a dash (S)
        public double dashFovInTime = 0.05; // s to ease the kick in (S)
        public double dashFovOutTime = 0.25; // s to ease the kick out (S)
        public double maxFovBoostDeg = 18; // total FOV never exceeds base + this
        public double near = 0.05; // m near clip plane (S)
        public double far = 450; // m far clip plane (S)
        public double eyeEaseTime = 0.1; // s for the eye to move between standing and sliding height (S)
    }

    // §3.5 Feel layer (camera parts; scaled by settings.effectsIntensity). The camera-only subset: speed lines,
    // the dash tint, recoil and hitstop are not ported (see SPIKE-RESULTS.md).
    [Serializable]
    public sealed class FeelConfig
    {
        public double landingDipPerSpeed = 0.01; // m of camera dip per m/s of fall speed (S)
        public double landingDipMax = 0.25; // m (S)
        public double landingDipReturnTime = 0.2; // s spring back (S)
        public double landingDipAttack = 0.06; // s to reach the full dip (S)
        public double strafeRollDeg = 1.5; // camera roll when strafing at walk speed (S)
        public double rollEaseRate = 10; // 1/s, exponential ease toward the target roll (S)
        public double effectsIntensityMax = 2; // settings.effectsIntensity is clamped to 0..this
        public int kickSlots = 8; // simultaneous camera kicks
        public double jumpKickDeg = 1; // camera pitch up on a jump, wall jump or boost jump
        public double jumpKickAttack = 0.03; // s
        public double jumpKickRecover = 0.2; // s
        public double slamPunchDeg = 2; // camera pitch down on a slam landing
        public double slamPunchAttack = 0.02; // s
        public double slamPunchRecover = 0.2; // s
        public double slideTiltDeg = 3; // camera roll while sliding (+ leans left)
        public double slideFovDeg = 3; // extra FOV while sliding (eased with the speed FOV)
    }

    // §8 settings (the defaults)
    [Serializable]
    public sealed class SettingsConfig
    {
        public double fovDeg = 74; // base vertical FOV
        public bool cameraRoll = true; // strafe + wall-ride roll
        public double effectsIntensity = 1; // 0–2 for dip, roll, FOV effects
        public double sensitivity = 0.0014; // rad of look per pixel
        public bool invertY = false;
        public bool reducedMotion = false; // no camera roll
    }

    // §3.1 Physics loop
    [Serializable]
    public sealed class LoopConfig
    {
        public double simHz = 120; // fixed simulation steps per second
        public double maxFrameDelta = 0.05; // s, frame delta clamp
        public double maxSubstepMove = 0.3; // m, longest move in one collision substep (no tunneling)
    }

    // §3.1 Collision resolution
    [Serializable]
    public sealed class CollisionConfig
    {
        public int maxResolveIterations = 4; // contact passes per substep
        public double groundProbe = 0.05; // m below the feet that still counts as floor while dashing (S)
        public double groundSnap = 0.35; // m: walking/sliding off a drop this small keeps you grounded (ramps) (S)
        public double headroomSkin = 0.02; // m shaved off the stand-up test capsule so wall contact is not "blocked" (S)
        // NIGHT5 F6: a rising head that clips a ceiling's edge within this share of the capsule radius slides past it
        // (cornerSkin m clear) instead of losing the jump (Celeste's corner correction; web docs/MOVEMENT-CHANGES.md).
        public double cornerCorrect = 0.25;
        public double cornerSkin = 0.005;
    }

    // §3.1 Collision body
    [Serializable]
    public sealed class BodyConfig
    {
        public double capsuleRadius = 0.35; // m
        public double capsuleHeight = 1.8; // m, total height including both caps
        public double eyeHeight = 1.6; // m above the feet
    }

    // §3.1 Surface type from the collision normal
    [Serializable]
    public sealed class SurfaceConfig
    {
        public double floorMinNormalY = 0.7; // floor when normal.y >= this
        public double wallMaxAbsNormalY = 0.3; // wall when |normal.y| < this; anything between is a slope
    }

    // §3.1 Safety cap, not a feel tool
    [Serializable]
    public sealed class SpeedConfig
    {
        public double maxHorizontal = 40; // m/s
    }

    // §3.2 Base movement, ground
    [Serializable]
    public sealed class GroundConfig
    {
        public double walkSpeed = 8; // m/s, every other speed is a multiple of this
        public double accel = 10; // 1/s, Quake-style (multiplied by wish speed)
        public double friction = 6; // 1/s; skipped on the first grounded step
        public double stopSpeed = 2.5; // m/s, friction control floor (S)
    }

    // §3.2 Base movement, air
    [Serializable]
    public sealed class AirConfig
    {
        public double accel = 1.5; // 1/s, Quake-style, cap applies only along the input direction (S)
    }

    // §3.2 Jumping (Pittman: v0 = 2h/t, g = 2h/t²)
    [Serializable]
    public sealed class JumpConfig
    {
        public double height = 1.5; // m, apex height -> v0 ≈ 8.33 m/s
        public double apexTime = 0.36; // s, time to apex -> gravity ≈ 23.1 m/s²
        public double fallGravityMult = 1.35; // × gravity once past the apex (S)
        public double apexHangMult = 0.5; // × gravity near the apex while Space is held (S)
        public double apexHangSpeed = 2.0; // m/s, apex hang applies while |vertical speed| is below this (S)
        public double coyoteTime = 0.1; // s after leaving a floor that a jump still counts
        public double bufferTime = 0.1; // s before landing that a jump press is remembered (S)
        public double releaseMult = 0.5; // × vertical speed when Space is released while rising (S)
        public int doubleJumps = 1; // per airtime; refreshed on landing, wall-ride start and wall jump
        public double doubleJumpMult = 0.9; // × v0
    }

    // §3.3 Dash + stamina
    [Serializable]
    public sealed class DashConfig
    {
        public double speed = 24; // m/s (3× walk)
        public double duration = 0.25; // s -> covers 6 m (S)
        public int pipCost = 1; // stamina pips per dash
        public int maxPips = 3; // stamina pips
        public double regenPerSec = 0.7; // pips/s, one pip at a time, none while dashing
    }

    // §3.3 Slide + slide jump
    [Serializable]
    public sealed class SlideConfig
    {
        public double cameraDrop = 0.7; // m the camera lowers while sliding (eye 0.9 m)
        public double colliderHeight = 1.0; // m capsule height while sliding
        public double floorSpeed = 11.6; // m/s minimum slide speed (1.45× walk)
        public double friction = 1.5; // 1/s above the floor speed (S)
        public double jumpHorizontalMult = 1.2; // × horizontal speed on a slide jump, only below the boost cap (S)
        public double jumpBoostMaxSpeed = 24; // m/s, slide jump never boosts past this; at or above it speed is kept
        public double slopeBoost = 1.6; // × g·sinθ·cosθ gained sliding downhill (bled uphill) (S)
        public double slopeRampLength = 8; // m, the Spire slide ramps (20° wedges), for the tests
    }

    // §3.3 Dash jump (horizontal boost, "wind" window)
    [Serializable]
    public sealed class DashJumpConfig
    {
        public double v0Mult = 0.8; // × v0
        public double gravityMult = 0.5; // × gravity during the window
        public double windowTime = 0.5; // s of lighter gravity
        public int extraPipCost = 1; // pips on top of the dash (2 total)
    }

    // §3.3 Slam
    [Serializable]
    public sealed class SlamConfig
    {
        public double speed = 48; // m/s straight down (6× walk)
        public double shockwaveRadius = 5; // m
        public double shockwaveDamage = 30; // HP
        public double slidePredictTime = 0.1; // s, a C press this close to a predicted landing slides instead
    }

    // §3.3 Boost jump (vertical, after a slam landing)
    [Serializable]
    public sealed class BoostJumpConfig
    {
        public double window = 0.15; // s after a slam landing
        public double v0Mult = 1.8; // × v0 (S)
    }

    // §3.3 Wall ride
    [Serializable]
    public sealed class WallRideConfig
    {
        public double entrySpeed = 4; // m/s along the wall needed to start (and keep) a ride
        public double minSpeed = 8; // m/s floor on speed along the wall while riding
        public double gravityMult = 0.15; // × gravity while riding (S)
        public double maxTime = 1.2; // s, the ride ends after this (S)
        public double rollDeg = 10; // camera roll away from the wall
        public double probeDistance = 0.12; // m, stick probe along −wallNormal each step (S)
        public double awayDot = 0.5; // (input direction · wall normal) above this ends the ride (S)
        public double cornerAngleDeg = 30; // wall normal change that ends the ride (convex corner)
        public double rejoinLockTime = 0.15; // s after a wall jump before the same wall can be ridden again
    }

    // §3.3 Wall jump
    [Serializable]
    public sealed class WallJumpConfig
    {
        public double outSpeed = 8; // m/s along the wall normal
        public double v0Mult = 0.9; // × v0 upward
        public int maxPerAirtime = 3;
    }

    // §5 Jump pads
    [Serializable]
    public sealed class JumpPadConfig
    {
        public double apexHeight = 8; // m, launch speed = √(2·g·h) (S)
        public double radius = 1.0; // m, trigger + disc radius (S)
        public double triggerHeight = 0.3; // m above the pad surface that still counts as standing on it (S)
        public double ringWidth = 0.12; // m, the thin ring around the disc
        public double discThickness = 0.04; // m, visual only (pads never collide)
        public int segments = 32; // circle segments for the disc and ring
    }

    // OVERNIGHT2 N4 ARCLINE: the swing gadget. Not ported: price (the shop) and the enemy hooks (zip, pullKinds,
    // enemyPull, enemyPullTime), since the spike arena has no shop and no enemies.
    [Serializable]
    public sealed class ArclineConfig
    {
        public double range = 60; // m: anchors in a cone toward the crosshair
        public double coneDeg = 15; // deg
        public double probeRadius = 1.5; // m sphere-cast
        public double maxLen = 55; // m of rope
        public double minLen = 4; // m of rope
        public double hookSpeed = 600; // m/s the hook flies
        public double hookMin = 0.06; // s it takes at least
        public double hookMax = 0.1; // s it takes at most
        public double pullTime = 0.18; // s: the yank toward the anchor when it bites
        public double pullAccel = 22; // m/s^2
        public double stretch = 0.03; // up to 3% past the rope length a critically damped spring pulls back
        public double springOmega = 30; // rad/s
        public double gravityMult = 0.85; // × gravity while attached (longer arcs)
        public double drag = 0.00008; // quadratic drag (1/m), small
        public double pumpAccel = 12; // m/s^2 of pump from the move keys (strongest at the bottom)
        public double softCap = 42; // m/s the pump fades to
        public double reelSpeed = 18; // m/s the reel key shortens the rope (down to minLen)
        public double risingBoost = 1.08; // × speed when you let go rising
        public double jumpUp = 4; // m/s up on a jump release
        public double cooldown = 0.4; // s before the next hook
        public double rehook = 0.25; // s before the same node takes one again
        public double losBreak = 0.25; // s the rope may be blocked before it breaks
        public double losSlack = 0.4; // m short of the anchor that still counts
        public int substeps = 4; // sub-steps a fixed step while attached (no tunnelling)
        public double ropeHeight = 1.2; // m, the rope's point on you
        public double fovLiftDeg = 4; // extra FOV at full swing speed (visual)
        public double groundLift = 6; // m/s up when the hook bites while you stand
        public double nodeGlow = 2.4;
        public double nodeRadius = 1.1;
        public int nodeColor = 0x7ff0ff;
        public int ropeColor = 0x19d3e6;
        public double ropeWidth = 0.035;
        public int ropeSegments = 14;
        public double sag = 0.35;
        // NIGHT21 (the ARCLINE in Survival): the enemy zip (arcline.zip; the typed loader skips nested objects, so SurvivalSim.Arcline21
        // reads the three numbers from PortExport/config.json at the run's start; these defaults are the web's).
        public ArclineZip zip = new ArclineZip();
    }

    /// <summary>arcline.zip: a big enemy hooked, the rope reels you straight in.</summary>
    [Serializable]
    public sealed class ArclineZip
    {
        public double reelSpeed = 34; // m/s straight at it
        public double arrive = 2.6; // m: let go this close
        public double maxTime = 1.2; // s: let go after this long
    }
}
