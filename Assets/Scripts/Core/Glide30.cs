// NIGHT30 (builder S; design.n30.glide, item 6a): GLIDE. Ice is a FLOOR TAG, not a mode (R3: ULTRAKILL's gasoline floor; "friction is a surface
// property"): every verb stays live, the brake goes. A rule applied AFTER the Mover's step, outside it (Mover.cs is frozen), as SkateLane29 was:
//   - BRAKING: on ice the Mover's ground friction (already x design.n26.surfaces.iceFrictionMult) is REPLACED by a glide decay of decayPerSecond of
//     the speed per second. The Mover's own step is v1 = friction(v0) + a (a = the input's acceleration and any wall clip); the rule recomputes
//     friction(v0) with the Mover's own ApplyFriction and sets v = v0 * (1 - decay dt) + a: the input still steers and still brakes exactly as the
//     Mover lets it (backward input slows you; no input glides).
//   - DASH ADDS: a dash begun grounded on ice ends at max(the Mover's exit, entry + dashAdd * max(0, 1 - entry / softCap)) along its direction
//     (the Mover's own exit is max(walk, entry): a dash on normal ground adds nothing once you are fast).
//   - DOWNHILL ADDS: on a sloped ice floor the horizontal part of the floor normal (it points downhill) times gravity x slopeGain is added per
//     second (uphill it bleeds speed: the same term).
//   - SOFT CAP: above softCap the speed loses capDrag of the excess per second (a drag, never a wall).
//   - STEERING: the Mover's ground acceleration is kept (a), and the heading turns toward the move input at up to turnRateDeg per second with the
//     speed kept (at speed the Mover's acceleration alone cannot turn: it adds nothing once the speed along the wish passes the walk speed).
// Pure: JsMath / MovementMath only, no randomness, whole steps. The sim (SurvivalSim.Systems30.cs) says whether the feet are on ice.
using System;

namespace Skygrind.Core
{
    public sealed class Glide30Params
    {
        public double decayPerSecond, dashAdd, softCap, capDrag, slopeGain, turnRateDeg;

        public static Glide30Params From(GameConfig cfg) => new Glide30Params
        {
            decayPerSecond = cfg.Num("design.n30.glide.decayPerSecond"),
            dashAdd = cfg.Num("design.n30.glide.dashAdd"),
            softCap = cfg.Num("design.n30.glide.softCap"),
            capDrag = cfg.Num("design.n30.glide.capDrag"),
            slopeGain = cfg.Num("design.n30.glide.slopeGain"),
            turnRateDeg = cfg.Num("design.n30.glide.turnRateDeg"),
        };
    }

    public sealed class Glide30
    {
        public readonly Glide30Params p;
        readonly MovementConfig mc;
        readonly double dt, gravity;

        Vec3 prevVel;
        bool prevGroundedIce;
        MoveState prevState = MoveState.Ground;
        bool dashArmed;
        double dashEntry;

        /// <summary>Is the player gliding now (grounded on ice): the view's skate trail.</summary>
        public bool Gliding { get; private set; }

        /// <summary>Counters (harness, reel): dashes on ice that added speed, steps glided.</summary>
        public int dashAdds, glideSteps;

        public Glide30(Glide30Params p, MovementConfig mc)
        {
            this.p = p;
            this.mc = mc;
            dt = 1 / mc.loop.simHz;
            gravity = MovementMath.GravityFor(mc.jump.height, mc.jump.apexTime);
        }

        public void Reset()
        {
            prevVel = default;
            prevGroundedIce = false;
            prevState = MoveState.Ground;
            dashArmed = false;
            dashEntry = 0;
            Gliding = false;
            dashAdds = glideSteps = 0;
        }

        /// <summary>The Mover's Dash event (subscribed by the sim): a dash begun grounded on ice is armed with its entry speed.</summary>
        public void OnDash(double entrySpeed, bool groundedOnIce)
        {
            dashArmed = groundedOnIce;
            dashEntry = entrySpeed;
        }

        /// <summary>The speed a dash on ice ends at, from its entry speed (the Mover's exit is max(walk, entry)).</summary>
        public double DashTarget(double entry) => JsMath.Max(JsMath.Max(mc.ground.walkSpeed, entry), entry + p.dashAdd * JsMath.Max(0, 1 - entry / p.softCap));

        /// <summary>One step after the Mover. `onIce`: the feet are on an ice surface (the sim's test); (wishX, wishZ): the move input in world x / z
        /// (InputFrame moveX / moveZ turned by the look's yaw; zero: no input).</summary>
        public void Step(Mover m, bool onIce, double wishX = 0, double wishZ = 0)
        {
            bool groundedIce = onIce && m.grounded;
            MoveState s = m.state;
            // a dash on ice that just ended: it adds speed
            if (prevState == MoveState.Dash && s != MoveState.Dash && dashArmed)
            {
                dashArmed = false;
                double sp = JsMath.Hypot(m.velocity.x, m.velocity.z), target = DashTarget(dashEntry);
                if (sp > 1e-6 && target > sp)
                {
                    double k = target / sp;
                    m.velocity = new Vec3(m.velocity.x * k, m.velocity.y, m.velocity.z * k);
                    dashAdds++;
                }
            }
            else if (s != MoveState.Dash && prevState != MoveState.Dash) dashArmed = false;
            Gliding = groundedIce && s == MoveState.Ground;
            if (Gliding && prevGroundedIce && prevState == MoveState.Ground)
            {
                glideSteps++;
                // the Mover's friction this step, recomputed, swapped for the glide decay (the input's acceleration a kept)
                Vec3 fr = MovementMath.ApplyFriction(prevVel, mc.ground.friction * m.groundFrictionMult, mc.ground.stopSpeed, dt);
                double ax = m.velocity.x - fr.x, az = m.velocity.z - fr.z;
                double keep = JsMath.Max(0, 1 - p.decayPerSecond * dt);
                double vx = prevVel.x * keep + ax, vz = prevVel.z * keep + az;
                // downhill adds (uphill bleeds): the floor normal's horizontal part points downhill
                Vec3 n = m.groundNormal;
                if (n.y < 0.999)
                {
                    vx += n.x * gravity * p.slopeGain * dt;
                    vz += n.z * gravity * p.slopeGain * dt;
                }
                // steering: at speed the Mover's ground acceleration cannot turn you (Quake accel adds nothing once the speed along the wish passes the
                // walk speed), so the glide turns the heading toward the input's direction at up to turnRateDeg per second, the speed kept (R3: TF-style
                // slide steer 1.8 rad/s). A wish more than 120 degrees off the heading is a brake (the Mover's own acceleration does it).
                double wx = wishX, wz = wishZ, wl = JsMath.Hypot(wx, wz), sp0 = JsMath.Hypot(vx, vz);
                if (wl > 0.1 && sp0 > mc.ground.walkSpeed * 0.5)
                {
                    double hv = JsMath.Atan2(vx, vz), hw = JsMath.Atan2(wx, wz), d = hw - hv;
                    while (d > Math.PI) d -= 2 * Math.PI;
                    while (d < -Math.PI) d += 2 * Math.PI;
                    if (Math.Abs(d) <= 120 * Math.PI / 180)
                    {
                        double maxTurn = p.turnRateDeg * Math.PI / 180 * dt;
                        double h = hv + (Math.Abs(d) <= maxTurn ? d : Math.Sign(d) * maxTurn);
                        vx = JsMath.Sin(h) * sp0;
                        vz = JsMath.Cos(h) * sp0;
                    }
                }
                // the soft cap: a drag on the excess
                double sp = JsMath.Hypot(vx, vz);
                if (sp > p.softCap)
                {
                    double ns = sp - (sp - p.softCap) * JsMath.Min(1, p.capDrag * dt);
                    vx *= ns / sp;
                    vz *= ns / sp;
                }
                m.velocity = new Vec3(vx, m.velocity.y, vz);
            }
            prevVel = m.velocity;
            prevGroundedIce = groundedIce;
            prevState = s;
        }
    }
}
