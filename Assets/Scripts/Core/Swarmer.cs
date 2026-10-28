// Port of skygrind/src/enemies/swarmer.js on the enemyKind.js base (SURVIVAL.md §7): fodder. A small, fast quadruped
// (swarmer.hp, swarmer.speed) that circles to your sides and back and pounces from there: a swarmer.tellTime crouch, then
// a short leap (swarmer.pounceApex) that lands 0.6 m short of you and bites once for swarmer.damage within biteRange, then
// recoverTime. It pounces only from outside your front cone (swarmer.frontConeDeg) and with a `swarm` token (HuskBrain).
// No head (body capsule only), no parry (it never winds up). The body physics are the Husk's (moveHusk = groundMove).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Swarmer : Husk
    {
        public string state = "idle"; // idle | crouch | pounce | recover
        public int stateSteps;
        public bool bit;
        public double drawAge; // s alive (visual: the crouch tell pulse)
        readonly EventBus events;

        public Swarmer(HuskParams h, EventBus events) : base(h)
        {
            this.events = events;
        }

        public override bool HasHead => false;

        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            headshot = false;
            return CombatMath.RayVerticalCapsule(o, d, position, h.height, h.radius);
        }

        public override bool Spawn(int newId, Vec3 pos, Vec3 target, ICollisionWorld world)
        {
            if (!base.Spawn(newId, pos, target, world)) return false;
            state = "idle";
            stateSteps = 0;
            bit = false;
            drawAge = 0;
            return true;
        }

        public override void Teleport(Vec3 spot)
        {
            base.Teleport(spot);
            state = "idle";
            stateSteps = 0;
            committed = false;
        }

        /// <summary>enemyKind.js stagger: only a committed attack is cut short (into recover).</summary>
        public override void Stagger(double time, double dt)
        {
            if (!committed) return;
            state = "recover";
            stateSteps = MovementMath.StepsFor(time, dt);
            committed = false;
            if (ai != null) events.Emit("enemyAttackDone", id);
        }

        /// <summary>enemyKind.js step: the timers (a stun holds it like a knockback), then a nav jump or its own step.</summary>
        public override void Tick(NavJumper jumper, Vec3 tp, Health target, IReadOnlyList<Enemy> pool, ICollisionWorld world, EventBus bus, double dt, double gravity)
        {
            if (cooldownSteps > 0) cooldownSteps--;
            if (knockSteps > 0) knockSteps--;
            if (staggerSteps > 0)
            {
                staggerSteps--;
                if (knockSteps == 0)
                {
                    knockSteps = 1;
                    velocity.x = 0;
                    velocity.z = 0;
                }
            }
            bool busy = committed || (state != "idle" && state != "recover") || knockSteps > 0;
            if (jumper != null && jumper.Step(this, dt, busy)) Move(world, dt, gravity);
            else StepSwarmer(tp, target, world, bus, dt, gravity);
        }

        void StepSwarmer(Vec3 tp, Health target, ICollisionWorld world, EventBus bus, double dt, double gravity)
        {
            double dx = tp.x - position.x;
            double dz = tp.z - position.z;
            double d = JsMath.Hypot(dx, dz);
            if (d > 0.01) yaw = JsMath.Atan2(-dx, -dz);
            if (state == "crouch")
            {
                velocity.x = 0;
                velocity.z = 0;
                if (--stateSteps <= 0)
                {
                    // Pounce: a short arc that lands just short of where you are now.
                    double k = d > 0 ? Math.Max(0, d - 0.6) / d : 0;
                    var land = new Vec3(position.x + dx * k, tp.y, position.z + dz * k);
                    velocity = CombatMath.LeapVelocity(position, land, h.pounceApex, gravity, out _);
                    state = "pounce";
                    stateSteps = MovementMath.StepsFor(1.2, dt);
                    bit = false;
                    bus.Emit("swarmerPounce", id);
                }
            }
            else if (state == "pounce")
            {
                double bx = tp.x - position.x, by = tp.y + 0.9 - (position.y + 0.4), bz = tp.z - position.z;
                if (!bit && Math.Sqrt(bx * bx + by * by + bz * bz) <= h.biteRange)
                {
                    bit = true;
                    if (target.Damage(h.biteDamage, "swarmer", position)) dealtDamage = true;
                }
                if ((grounded && stateSteps < MovementMath.StepsFor(1.1, dt)) || --stateSteps <= 0)
                {
                    state = "recover";
                    stateSteps = MovementMath.StepsFor(h.recoverTime, dt);
                    committed = false;
                    if (ai != null) bus.Emit("enemyAttackDone", id);
                }
            }
            else
            {
                if (state == "recover" && --stateSteps <= 0) state = "idle";
                bool ready = state == "idle" && cooldownSteps == 0 && grounded && knockSteps == 0;
                // From your side or back only (outside your front cone), within range, with a token.
                bool flank = ai == null || (ai.flankNow != null ? ai.flankNow(position) : ai.flank);
                if (ready && d <= h.pounceRange && flank && (ai == null || ai.attack) && !target.Dead)
                {
                    state = "crouch";
                    stateSteps = MovementMath.StepsFor(h.tellTime, dt);
                    committed = true;
                    cooldownSteps = MovementMath.StepsFor(h.attackCooldown, dt);
                    if (ai != null) bus.Emit("enemyCommit", id);
                    bus.Emit("swarmerCrouch", id);
                }
                else if (knockSteps == 0)
                {
                    double len = d == 0 ? 1 : d;
                    velocity.x = ai != null ? ai.vx : dx / len * h.speed;
                    velocity.z = ai != null ? ai.vz : dz / len * h.speed;
                }
            }
            Move(world, dt, gravity);
        }
    }
}
