// NIGHT19 U3-33.7 (builder E): port of skygrind/src/enemies/charger.js on the enemyKind.js base (as Swarmer.cs and
// Spitter.cs are), behind design.enemies.n19 (the director spawns it only with the block on: Director.Spawnable).
// charger.js's header:
// CHARGER (SURVIVAL.md §7, S6): the heavy. 150 HP, heavy token. Telegraph 0.8 s: it stomps, glows and a
// charge line is painted on the ground toward where you stand; then it charges straight at 22 m/s: 25
// damage plus a big knockback if it passes through you (jump over it, dash through it, ride a wall). If it
// hits a wall it is stunned for 1.5 s with its back weak point exposed (x3 damage from behind).
// The port: config.charger's numbers (PortExport/config.json) read once (ChargerParams); the body physics are the Husk's
// (moveHusk = enemyKind.js groundMove, with its hitWall: Husk.Move); the web's damageMult (the back weak point) is
// DamageMult, read by EnemyHits.Filter (Core/Enemies/Affixes.cs: the hook weapons.js damage() runs before the kind's
// damage). The player's mover (pm.impulse for the throw) is the target's (Husks sets targetMover from Players each step;
// null with the one-target Step of the tests and the bench, as the web's `pm &&`). The events keep the web's names:
// chargerTell {id, yaw}, chargerCharge, chargerStunned, chargerBlocked {id} (ChargerEvent), and the brain's enemyCommit /
// enemyAttackDone (the id, as the port's husks emit them). Its brain is the Husks' (HuskBrain: ai.kinds.charger, the
// heavy token; ai.jumps.charger.jumps false: a jump link stops it dazed instead, ai.blocked).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>config.charger's attack numbers (and ai.jumps.charger.blockedStun).</summary>
    public sealed class ChargerParams
    {
        public double minRange, maxRange, tellTime, chargeSpeed, chargeDistance, damage, hitRange, knockback, knockbackUp;
        public double stunTime, weakMult, recoverTime, cooldown, lineWidth, knockbackScale, blockedStun, height;
        /// <summary>NIGHT40 E3 foes40 (design.n40.foes40.charger; the sim sets them at a run's start; these defaults with the row off = the web's rule):
        /// frontMult40 x every hit while it is not stunned and a stunned hit from the front (its armour: only the stunned back is open); skidStun40 s stunned when a charge ends without a wall
        /// (0: recover, as the web); backOff40: closer than minRange and free, it walks back out to charge range (the web's charger stood on the player).</summary>
        public double frontMult40 = 1, skidStun40;
        public bool backOff40;
        /// <summary>NIGHT47 B charger47 (design.n47.charger47.numbers; SurvivalSim.N47.Brawl.cs sets them at a run's start; these defaults with the
        /// row off = NIGHT46's rule): lock47 s: the wind-up's line tracks you until lock47 s are left, then locks (0: locked at the wind-up's start);
        /// turn47 rad/s: the charge turns toward you at most this fast (0: straight); burst47: a charge through a light body marks it for the
        /// sim's burst (burst47Target) and a charge into a heavy one stops it, stunned heavyStun47 s; enrageTell47 s: the one faster wind-up a
        /// charger gets after another charger near it dies (enraged47; 0: none).</summary>
        public double lock47, turn47, heavyStun47, enrageTell47;
        public bool burst47;
        /// <summary>NIGHT47 B charger47 (G27; off: false / 0): the glove on the prow lands nothing; the KESTREL in a free charger's back lands x this.</summary>
        public bool gloveFront0_47;
        public double gunBack47;

        public static ChargerParams From(GameConfig c) => new ChargerParams
        {
            minRange = c.Num("charger.minRange"), maxRange = c.Num("charger.maxRange"), tellTime = c.Num("charger.tellTime"),
            chargeSpeed = c.Num("charger.chargeSpeed"), chargeDistance = c.Num("charger.chargeDistance"), damage = c.Num("charger.damage"),
            hitRange = c.Num("charger.hitRange"), knockback = c.Num("charger.knockback"), knockbackUp = c.Num("charger.knockbackUp"),
            stunTime = c.Num("charger.stunTime"), weakMult = c.Num("charger.weakMult"), recoverTime = c.Num("charger.recoverTime"),
            cooldown = c.Num("charger.cooldown"), lineWidth = c.Num("charger.lineWidth"),
            knockbackScale = c.Has("charger.knockbackScale") ? c.Num("charger.knockbackScale") : 1,
            blockedStun = c.Num("ai.jumps.charger.blockedStun"), height = c.Num("charger.height"),
        };

        /// <summary>config.charger through enemyKind.js (no head; lift 0.05; aim at height x 0.55; flash hitFlashTime ?? 0.08).</summary>
        public static HuskParams Body(GameConfig c, double killPlaneY) => new HuskParams
        {
            kind = "charger", hp = c.Num("charger.hp"), speed = c.Num("charger.speed"), radius = c.Num("charger.radius"), height = c.Num("charger.height"),
            aimHeight = c.Has("charger.aimHeight") ? c.Num("charger.aimHeight") : c.Num("charger.height") * 0.55, spawnLift = 0.05,
            hitFlashTime = c.Has("charger.hitFlashTime") ? c.Num("charger.hitFlashTime") : 0.08,
            edgeNormalY = c.Num("ai.edgeNormalY"), edgeRiseSpeed = c.Num("ai.edgeRiseSpeed"), supportedDrop = c.Num("ai.supportedDrop"),
            floorMinNormalY = c.movement.surface.floorMinNormalY, wallMaxAbsNormalY = c.movement.surface.wallMaxAbsNormalY, killPlaneY = killPlaneY,
        };
    }

    /// <summary>chargerTell {id, yaw} / chargerCharge / chargerStunned / chargerBlocked {id} (one reused payload per charger).</summary>
    public sealed class ChargerEvent
    {
        public int id;
        public double yaw;
    }

    public sealed class Charger : Husk
    {
        public string state = "idle"; // idle | tell | charge | stunned | recover | blocked
        public int stateSteps;
        public double chargeYaw; // e.data.chargeYaw: locked at the tell (the painted line shows it)
        public bool hitPlayer; // e.data.hitPlayer: this charge already hit
        public double drawAge; // s alive (visual: the stomp, the line's pulse, the daze)
        /// <summary>The target's mover this step (pm: the throw's impulse, the jump-over test); null: none (the web's `pm &&`).</summary>
        public Mover targetMover;
        readonly EventBus events;
        readonly ChargerParams c;
        readonly ChargerEvent said = new ChargerEvent();
        /// <summary>NIGHT47 charger47: the next wind-up is the faster one (set by the sim when another charger dies); the line locked this wind-up;
        /// a light body this charge ran through, for the sim to burst (null: none); the sim's alive flag of last step (the death edge).</summary>
        public bool enraged47, locked47 = true, wasAlive47;
        /// <summary>NIGHT47 B: this body's charges so far (+1 at each charge's start), and the charge that last hit THE ICEBREAKER (the sim's).</summary>
        public int chargeNo47, bossHitCharge47 = -1;
        public Enemy burst47Target;

        public Charger(HuskParams h, ChargerParams c, EventBus events) : base(h)
        {
            this.c = c;
            this.events = events;
        }

        public ChargerParams Attack => c;
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
            chargeYaw = 0;
            hitPlayer = false;
            drawAge = 0;
            enraged47 = false; // NIGHT47 charger47's per-body state (unused with the row off)
            locked47 = true;
            burst47Target = null;
            wasAlive47 = false;
            return true;
        }

        /// <summary>enemyKind.js teleport: placed (+0.05), still, idle, its attack dropped.</summary>
        public override void Teleport(Vec3 spot)
        {
            base.Teleport(spot);
            state = "idle";
            stateSteps = 0;
            committed = false;
        }

        /// <summary>NIGHT47 B charger47 (G27, the lead's round-3 line "make the room need a gun"; 1 with the row off): the PULSE glove (the
        /// gauntlet, the punch) on the prow lands nothing (gloveFront0_47: "the armour shrugs"), and the KESTREL into the back of a charger that
        /// is not stunned lands gunBack47 in place of the front's armour (its pierce through two lined-up backs is the trick shot).</summary>
        public double WeaponMult47(string weapon, Vec3 from)
        {
            if (!c.gloveFront0_47 && c.gunBack47 <= 0) return 1;
            double fx = -JsMath.Sin(yaw), fz = -JsMath.Cos(yaw);
            bool behind = (from.x - position.x) * fx + (from.z - position.z) * fz < 0;
            if (c.gloveFront0_47 && !behind && (weapon == "gauntlet" || weapon == "punch")) return 0;
            if (c.gunBack47 > 0 && behind && state != "stunned" && weapon == "revolver") return c.gunBack47 / (c.frontMult40 == 1 ? 1 : c.frontMult40);
            return 1;
        }

        /// <summary>charger.js damageMult: x3 from behind while stunned (the back weak point); normal otherwise.</summary>
        public double DamageMult(Vec3 from)
        {
            if (state != "stunned") return c.frontMult40 == 1 ? 1 : c.frontMult40; // NIGHT40 E3 foes40: armoured until it is stunned (x frontMult40)
            double fx = -JsMath.Sin(yaw);
            double fz = -JsMath.Cos(yaw);
            bool behind = (from.x - position.x) * fx + (from.z - position.z) * fz < 0;
            return behind ? c.weakMult : c.frontMult40 == 1 ? 1 : c.frontMult40; // NIGHT40 E3 foes40: stunned, its face stays armoured
        }

        /// <summary>enemyKind.js knockback: x knockbackScale (the heavy barely moves).</summary>
        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt) => base.Knockback(dirX, dirZ, speed * c.knockbackScale, time, dt);

        /// <summary>enemyKind.js launch: up x knockbackScale.</summary>
        public override void Launch(double up) => base.Launch(up * c.knockbackScale);

        /// <summary>enemyKind.js stagger: only a committed attack is cut short (into recover).</summary>
        public override void Stagger(double time, double dt)
        {
            if (!committed) return;
            state = "recover";
            stateSteps = MovementMath.StepsFor(time, dt);
            committed = false;
            if (ai != null) events.Emit("enemyAttackDone", id);
        }

        /// <summary>enemyKind.js stun (POWER P4): it stands still for `time` s; a committed attack is cut short as a stagger.</summary>
        public override void Stun(double time, double dt)
        {
            staggerSteps = Math.Max(staggerSteps, MovementMath.StepsFor(time, dt));
            if (committed) Stagger(time, dt);
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
            else StepCharger(tp, target, pool, world, dt, gravity);
        }

        void Say(string name)
        {
            said.id = id;
            said.yaw = chargeYaw;
            events.Emit(name, said);
        }

        /// <summary>charger.js step.</summary>
        void StepCharger(Vec3 tp, Health target, IReadOnlyList<Enemy> pool, ICollisionWorld world, double dt, double gravity)
        {
            double dx = tp.x - position.x;
            double dz = tp.z - position.z;
            double d = JsMath.Hypot(dx, dz);
            if (state == "tell")
            {
                velocity.x = 0;
                velocity.z = 0;
                if (!locked47)
                {
                    // NIGHT47 charger47: the line follows you through the wind-up, then locks for its last lock47 s (dodge late, not early)
                    if (stateSteps > MovementMath.StepsFor(c.lock47, dt)) { if (d > 0.01) { chargeYaw = JsMath.Atan2(-dx, -dz); yaw = chargeYaw; } }
                    else { locked47 = true; Say("chargerLock47"); }
                }
                if (--stateSteps <= 0)
                {
                    state = "charge";
                    chargeNo47++; // NIGHT47 B: counts this body's charges (brawl47's one stagger per charge into THE ICEBREAKER)
                    yaw = chargeYaw;
                    stateSteps = MovementMath.StepsFor(c.chargeDistance / c.chargeSpeed, dt);
                    hitPlayer = false;
                    Say("chargerCharge");
                }
            }
            else if (state == "charge")
            {
                if (c.turn47 > 0 && d > 0.01)
                {
                    // NIGHT47 charger47: it turns badly toward you (Pinky / Helldivers' charger): at most turn47 rad/s
                    double diff = JsMath.Atan2(-dx, -dz) - yaw;
                    while (diff > Math.PI) diff -= 2 * Math.PI;
                    while (diff < -Math.PI) diff += 2 * Math.PI;
                    double most = c.turn47 * dt;
                    yaw += diff > most ? most : diff < -most ? -most : diff;
                    chargeYaw = yaw;
                }
                velocity.x = -JsMath.Sin(yaw) * c.chargeSpeed;
                velocity.z = -JsMath.Cos(yaw) * c.chargeSpeed;
                // Passing through you: 25 + a big knockback (a jump over it, a dash or a wall ride avoids it).
                Mover pm = targetMover;
                bool over = tp.y > position.y + c.height * 0.8; // pm.position (the target's feet: tp)
                if (!hitPlayer && d <= c.hitRange && !over && !target.Dead)
                {
                    hitPlayer = true;
                    bool hit = target.Damage(c.damage, "charger", position);
                    if (hit) dealtDamage = true; // enemyKind.js hurtPlayer
                    if (hit && pm != null)
                    {
                        double px = -JsMath.Sin(yaw) * c.knockback, pz = -JsMath.Cos(yaw) * c.knockback;
                        pm.Impulse(px, c.knockbackUp, pz);
                    }
                }
                Move(world, dt, gravity);
                if (c.burst47 && pool != null && Burst47(pool))
                {
                    // NIGHT47 charger47: it ran into a heavy body: it stops, stunned (its back open)
                    state = "stunned";
                    stateSteps = MovementMath.StepsFor(c.heavyStun47, dt);
                    velocity = default;
                    Say("chargerStunned");
                    return;
                }
                if (hitWall)
                {
                    state = "stunned";
                    stateSteps = MovementMath.StepsFor(c.stunTime, dt);
                    velocity = default;
                    Say("chargerStunned"); // the web's particle burst (slamDust) is the view's (EnemyViews.Charger.cs)
                    return;
                }
                if (--stateSteps <= 0)
                {
                    if (c.skidStun40 > 0)
                    {
                        // NIGHT40 E3 foes40: a charge that met no wall ends in a skid, stunned (its back open) for skidStun40 s
                        state = "stunned";
                        stateSteps = MovementMath.StepsFor(c.skidStun40, dt);
                        velocity = default;
                        Say("chargerStunned");
                        return;
                    }
                    state = "recover";
                    stateSteps = MovementMath.StepsFor(c.recoverTime, dt);
                }
                return;
            }
            else if (state == "blocked")
            {
                velocity.x = 0;
                velocity.z = 0;
                if (--stateSteps <= 0) state = "idle";
            }
            else if (state == "stunned" || state == "recover")
            {
                velocity.x = 0;
                velocity.z = 0;
                if (--stateSteps <= 0)
                {
                    state = "idle";
                    committed = false;
                    cooldownSteps = MovementMath.StepsFor(c.cooldown, dt);
                    if (ai != null) events.Emit("enemyAttackDone", id);
                }
            }
            else
            {
                if (d > 0.01) yaw = JsMath.Atan2(-dx, -dz);
                bool ready = cooldownSteps == 0 && grounded && d >= c.minRange && d <= c.maxRange && !target.Dead;
                if (ai != null && ai.blocked)
                {
                    // POWER P1: its way needs a jump it can't make (or the watchdog found it stuck): stop, dazed, 1 s.
                    ai.blocked = false;
                    state = "blocked";
                    stateSteps = MovementMath.StepsFor(c.blockedStun, dt);
                    Say("chargerBlocked");
                }
                else if (ready && (ai == null || (ai.attack && ai.los)))
                {
                    state = "tell";
                    stateSteps = MovementMath.StepsFor(enraged47 && c.enrageTell47 > 0 ? c.enrageTell47 : c.tellTime, dt);
                    if (enraged47) { enraged47 = false; Say("chargerEnrage47"); } // NIGHT47 charger47: the one faster wind-up
                    locked47 = c.lock47 <= 0; // NIGHT47 charger47: off = locked at once (NIGHT46)
                    chargeYaw = JsMath.Atan2(-dx, -dz); // locked at the tell: the painted line shows it
                    committed = true;
                    if (ai != null) events.Emit("enemyCommit", id);
                    Say("chargerTell");
                }
                else if (c.backOff40 && d < c.minRange && knockSteps == 0)
                {
                    // NIGHT40 E3 foes40: too close to charge: it backs out (facing you) to its run-up
                    double len = d == 0 ? 1 : d;
                    velocity.x = -dx / len * h.speed;
                    velocity.z = -dz / len * h.speed;
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

        /// <summary>NIGHT47 charger47: a body in the charge's way (horizontal reach = both radii + 0.2 m, the heights overlapping). A light one (a husk
        /// that is not a tank, a swarmer, a spitter) is marked for the sim's burst (burst47Target; one a step) and the charge goes on; a heavy one
        /// (a tank, a warden, another charger, a burrower) stops it: true. Bodies with no feet on the floor (drones) and bosses are not in its way.</summary>
        bool Burst47(IReadOnlyList<Enemy> pool)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                Enemy e = pool[i];
                if (e == this || !e.alive) continue;
                string k = e.kind;
                bool light = (k == "husk" && !e.tank29) || k == "swarmer" || k == "spitter";
                bool heavy = (k == "husk" && e.tank29) || k == "warden" || k == "charger" || k == "burrower";
                if (!light && !heavy) continue;
                double ex = e.position.x - position.x, ez = e.position.z - position.z, r = h.radius + e.Radius + 0.2;
                if (ex * ex + ez * ez > r * r) continue;
                if (e.position.y > position.y + h.height || position.y > e.position.y + e.Height) continue;
                if (heavy) return true;
                if (burst47Target == null) burst47Target = e;
            }
            return false;
        }
    }
}
