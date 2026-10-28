// NIGHT19 U3-33.7 (builder E): port of skygrind/src/enemies/drone.js (createDrones, createOrbs), behind
// design.enemies.n19 (the director spawns it only with the block on: Director.Spawnable). drone.js's header:
// Drone + orbs (SPEC 2.0 §5): hovers 4-8 m over the ground below it, keeps 12-18 m from the player,
// climbs over pillars it would hit (sphere probe, no pathfinding), and with line of sight to the
// player's eye charges 0.4 s and fires an orb every 2 s. Orbs are pooled: geometry and shots destroy
// them, and a punch reflects them (a parry) at the nearest enemy.
// Fixed step for the simulation; instanced meshes interpolated on frame time.
// The port: the drone is a body in the Husks' pools (its own, drone.maxCount; stepped after the husks: main.js's
// createEnemies order) so the registry, the brain (ai.kinds.drone: the ranged token, the far ring; `flies`: no ground
// navigation or watchdog), the director and every weapon see it; its `position` is its centre as on the web (AimHeight 0,
// a sphere of drone.radius to rays). The orbs are DroneOrbs (config.orb, pooled: orb.poolSize objects made once), stepped
// before the enemies (main.js simulate: orbs.step, then enemies.step); a punch reflects one through SpitterAcid.TryReflect
// (the loadout's one reflect call; punch.js tryReflect is the orbs' own on the web). The web's Math.random() (the hover,
// the orbit side, the first shot's stagger, the deliberate miss's side) is a Mulberry32 from the run seed ^
// design.enemies.n19.droneSalt (Husks.NewRun). The brain's aim (brain.js rec.out.aim: lead + a deliberate miss) is Aim
// here, with the target's velocity (Husks sets targetMover). solidTop (main.js: a drone inside a map box rises out of it)
// is not ported: the port's spawn nodes are on open floor (logged). Events keep the web's names: droneCharge {id},
// orbFire, orbDestroyed {reason, position}, enemyShot, enemyCommit / enemyAttackDone (the id, as the port's husks).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>config.drone (and the body / orb numbers a drone reads), read once.</summary>
    public sealed class DroneParams
    {
        public double hp, radius, speed, accel, hoverMin, hoverMax, rangeMin, rangeMax, orbitSpeed, probeDistance, climbAbove;
        public double climbProbeHeight, fireInterval, chargeTime, losSlack, separation, fireStagger, sideAngle, sidePush, sideDecay;
        public double rayLength, spawnRise, hitFlashTime, eyeHeight, orbSpeed, orbRadius, killPlaneY, Dt;
        public int rayEvery;
        public AimMath.A aim;
        /// <summary>NIGHT40 E3 (design.n40.foes40.drone.approachSpeed / ai.kinds.drone.speed; the sim sets it at a run's start, 1 with foes40 off):
        /// a drone spawned with it != 1 takes it as its paceMult (the brain's speed cap x it, as SWIFT's). 1: the spawn leaves paceMult alone, as before.
        /// Its climb and hover keep drone.speed.</summary>
        public double moveMult = 1;

        public static DroneParams From(GameConfig c, double killPlaneY) => new DroneParams
        {
            hp = c.Num("drone.hp"), radius = c.Num("drone.radius"), speed = c.Num("drone.speed"), accel = c.Num("drone.accel"),
            hoverMin = c.Num("drone.hoverMin"), hoverMax = c.Num("drone.hoverMax"), rangeMin = c.Num("drone.rangeMin"), rangeMax = c.Num("drone.rangeMax"),
            orbitSpeed = c.Num("drone.orbitSpeed"), probeDistance = c.Num("drone.probeDistance"), climbAbove = c.Num("drone.climbAbove"),
            climbProbeHeight = c.Num("drone.climbProbeHeight"), fireInterval = c.Num("drone.fireInterval"), chargeTime = c.Num("drone.chargeTime"),
            losSlack = c.Num("drone.losSlack"), separation = c.Num("drone.separation"), fireStagger = c.Num("drone.fireStagger"),
            rayEvery = c.Int("drone.rayEvery"), rayLength = c.Num("drone.rayLength"), sideAngle = c.Num("drone.sideAngle"),
            sidePush = c.Num("drone.sidePush"), sideDecay = c.Num("drone.sideDecay"), spawnRise = c.Num("drone.spawnRise"),
            hitFlashTime = c.Num("drone.hitFlashTime"), eyeHeight = c.movement.body.eyeHeight, orbSpeed = c.Num("orb.speed"),
            orbRadius = c.Num("orb.radius"), killPlaneY = killPlaneY, aim = AimMath.A.From(c), Dt = c.Dt,
        };

        /// <summary>The body the Husks' pool and the weapons read: a sphere of radius (height 2r for the punch's capsule), aimed at its centre.</summary>
        public HuskParams Body() => new HuskParams
        {
            kind = "drone", hp = hp, speed = speed, radius = radius, height = 2 * radius, aimHeight = 0, spawnLift = 0, hitFlashTime = hitFlashTime,
            killPlaneY = killPlaneY,
        };
    }

    /// <summary>droneCharge {id} (one reused payload per drone).</summary>
    public sealed class DroneEvent
    {
        public int id;
    }

    public sealed class Drone : Husk
    {
        public double hover; // m above the ground, rolled per drone
        public int orbit = 1; // +-1 drift direction
        public double climbTo = double.NegativeInfinity; // m, while climbing over a blocking pillar
        public double sideX, sideZ; // POWER P1: the obstacle rays' sideways steer (m/s, eases out)
        public int rayTick;
        public int fireSteps; // until the next charge may start
        public int chargeSteps; // > 0 while charging
        public double spin, pulse; // rad (visual)
        /// <summary>The target's mover this step (the aim's lead); null: a still target.</summary>
        public Mover targetMover;
        /// <summary>NIGHT20 (design.director.n20, set by the director at the spawn; false / 0: drone.js's climb exactly): the
        /// climb's look-down starts under the first roof above the probe; a climb that gains no height for climbStall s gives
        /// up and doesn't re-latch for climbHold s.</summary>
        public bool roofCheck;
        public double climbStall, climbHold;
        int climbStallSteps, climbHoldSteps;
        static readonly Vec3 Up = new Vec3(0, 1, 0);
        const double RoofGap = 0.01; // m under the roof the look-down starts (inside nothing)
        const double RiseEpsilon = 0.1; // m/s: slower is no progress (a climb pinned under a roof has its rise pushed out)
        readonly DroneParams d;
        readonly DroneOrbs orbs;
        readonly EventBus events;
        readonly Func<double> random; // the web's Math.random: the pool's seeded stream
        readonly DroneEvent said = new DroneEvent();

        public Drone(DroneParams d, DroneOrbs orbs, EventBus events, Func<double> random) : base(d.Body())
        {
            this.d = d;
            this.orbs = orbs;
            this.events = events;
            this.random = random;
        }

        public override bool HasHead => false;

        /// <summary>drone.js raycast: a sphere of drone.radius at its centre, no headshots.</summary>
        public override double RayHit(Vec3 o, Vec3 dir, out bool headshot)
        {
            headshot = false;
            return CombatMath.RaySphere(o, dir, position, d.radius);
        }

        /// <summary>drones.spawn: above the point (spawnRise + radius), the hover, the orbit side and the first shot's stagger rolled.</summary>
        public override bool Spawn(int newId, Vec3 pos, Vec3 target, ICollisionWorld world)
        {
            id = newId;
            alive = true;
            hp = maxHp = d.hp;
            climbTo = double.NegativeInfinity;
            climbStallSteps = climbHoldSteps = 0; // NIGHT20
            chargeSteps = 0;
            flash = 0;
            hover = d.hoverMin + random() * (d.hoverMax - d.hoverMin);
            orbit = random() < 0.5 ? -1 : 1;
            fireSteps = MovementMath.StepsFor(d.fireInterval * (d.fireStagger + random()), d.Dt);
            dealtDamage = false;
            ai = null;
            committed = false;
            position = new Vec3(pos.x, pos.y + d.spawnRise + d.radius, pos.z);
            previous = position;
            velocity = default;
            sideX = sideZ = 0;
            rayTick = 0;
            spin = pulse = 0;
            grounded = leaping = onEdge = hitWall = launched = false;
            windupSteps = staggerSteps = knockSteps = cooldownSteps = 0;
            jump = null;
            yaw = 0; // drone.js has no yaw (affixes.js frontHit reads enemy.yaw ?? 0)
            Shielded = false;
            Airborne = true; // a flying kind (LAUNCH + SNIPE)
            if (d.moveMult != 1) paceMult = d.moveMult; // NIGHT40 E3 foes40: the brain's speed cap x this (HuskBrain: k.speed x e.paceMult); 1: untouched, as before
            return true;
        }

        /// <summary>drones.teleport: above the spot, still, no climb.</summary>
        public override void Teleport(Vec3 spot)
        {
            position = new Vec3(spot.x, spot.y + d.spawnRise + d.radius, spot.z);
            previous = position;
            velocity = default;
            climbTo = double.NegativeInfinity;
            climbStallSteps = climbHoldSteps = 0; // NIGHT20
        }

        /// <summary>drones.knockback: a push added to its velocity (no knockback clock).</summary>
        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt)
        {
            double len = JsMath.Hypot(dirX, dirZ);
            if (len == 0) len = 1;
            velocity.x += dirX / len * speed;
            velocity.z += dirZ / len * speed;
        }

        /// <summary>Drones have no stagger, stun or launch (the web's drone system has none: enemies.js calls them with ?.).</summary>
        public override void Stagger(double time, double dt)
        {
        }

        public override void Stun(double time, double dt)
        {
        }

        public override void Launch(double up)
        {
        }

        /// <summary>One fixed step (drones.step: stepDrone; the kill plane is the pool's).</summary>
        public override void Tick(NavJumper jumper, Vec3 tp, Health target, IReadOnlyList<Enemy> pool, ICollisionWorld world, EventBus bus, double dt, double gravity) =>
            StepDrone(tp, target, pool, world, dt);

        /// <summary>Clear line of sight from the drone to a point (Octree ray).</summary>
        bool HasLos(ICollisionWorld world, Vec3 from, Vec3 to)
        {
            Vec3 dir = to - from;
            double length = dir.Length;
            if (length == 0) return true;
            dir = Vec3.DivideScalar(dir, length);
            return !world.RayIntersect(from, dir, out RayHit hit) || hit.distance >= length - d.losSlack;
        }

        /// <summary>Ground height below a point (the kill plane when there is nothing).</summary>
        double GroundBelow(ICollisionWorld world, Vec3 point) =>
            world.RayIntersect(point, new Vec3(0, -1, 0), out RayHit hit) ? point.y - hit.distance : d.killPlaneY;

        /// <summary>The world as a sphere sees it (Octree.sphereIntersect: a capsule with both ends at the centre).</summary>
        bool SphereIntersect(ICollisionWorld world, Vec3 center, out Vec3 normal, out double depth) =>
            WorldCollider.CapsuleIntersect(world, new Capsule { start = center, end = center, radius = d.radius }, out normal, out depth);

        double Free(ICollisionWorld world, double fx, double fz, double ang)
        {
            double c = JsMath.Cos(ang);
            double s = JsMath.Sin(ang);
            return world.RayIntersect(position, new Vec3(fx * c - fz * s, 0, fx * s + fz * c), out RayHit hit) ? hit.distance : double.PositiveInfinity;
        }

        /// <summary>brain.js rec.out.aim: where this drone's orb should go (a lead on the eye x 0.8, a deliberate miss off the hit roll).</summary>
        Vec3 Aim(Vec3 from, Vec3 tp)
        {
            var eye = new Vec3(tp.x, tp.y + d.eyeHeight * 0.8, tp.z);
            Vec3 vel = targetMover != null ? targetMover.velocity : default;
            AimMath.LeadPoint(from, eye, vel, d.orbSpeed, d.aim.maxLead, out Vec3 point);
            if (ai != null && !ai.hit) point = AimMath.MissPoint(from, point, d.aim.missBy, random());
            return Vec3.Normalize(point - from);
        }

        void StepDrone(Vec3 tp, Health target, IReadOnlyList<Enemy> pool, ICollisionWorld world, double dt)
        {
            var eye = new Vec3(tp.x, tp.y + d.eyeHeight, tp.z);
            double dx = tp.x - position.x;
            double dz = tp.z - position.z;
            double dist = JsMath.Hypot(dx, dz);
            if (dist == 0) dist = 1;

            // Desired horizontal velocity: close in, back off, or drift sideways in range. With a Survival brain
            // (SURVIVAL S4) it flies to its far-ring slot instead and repositions after every shot.
            double vx, vz;
            if (ai != null)
            {
                vx = ai.vx;
                vz = ai.vz;
            }
            else if (dist > d.rangeMax)
            {
                vx = dx / dist * d.speed;
                vz = dz / dist * d.speed;
            }
            else if (dist < d.rangeMin)
            {
                vx = -dx / dist * d.speed;
                vz = -dz / dist * d.speed;
            }
            else
            {
                vx = -dz / dist * d.orbitSpeed * orbit;
                vz = dx / dist * d.orbitSpeed * orbit;
            }

            // POWER P1, 3D steering: a forward ray (a few times a second) at the drone's height; blocked ahead -> steer
            // to the freer side (two rays at +/- sideAngle) while the climb below lifts it over the obstacle.
            double hSpeed = JsMath.Hypot(vx, vz);
            if (hSpeed > 0.1 && ++rayTick % d.rayEvery == 0)
            {
                double fx = vx / hSpeed;
                double fz = vz / hSpeed;
                if (Free(world, fx, fz, 0) < d.rayLength)
                {
                    double left = Free(world, fx, fz, d.sideAngle);
                    double right = Free(world, fx, fz, -d.sideAngle);
                    double turn = left >= right ? 1 : -1;
                    sideX = -fz * turn * d.speed * d.sidePush;
                    sideZ = fx * turn * d.speed * d.sidePush;
                }
            }
            sideX *= 1 - Math.Min(1, d.sideDecay * dt);
            sideZ *= 1 - Math.Min(1, d.sideDecay * dt);
            vx += sideX;
            vz += sideZ;
            hSpeed = JsMath.Hypot(vx, vz);
            // Sphere probe ahead: blocked -> climb to 2 m above the blocking top (no pathfinding).
            if (hSpeed > 0)
            {
                var probe = new Vec3(position.x + vx / hSpeed * d.probeDistance, position.y, position.z + vz / hSpeed * d.probeDistance);
                bool blocked = SphereIntersect(world, probe, out _, out _);
                Vec3 look = probe;
                if (roofCheck)
                {
                    // NIGHT20: the sphere probe misses a wall once its centre is inside it (pressed against the wall), and a
                    // probe touching a face looks down short of it: a ray ahead finds the face, and the look-down is just past it
                    double ux = vx / hSpeed, uz = vz / hSpeed;
                    if (world.RayIntersect(position, new Vec3(ux, 0, uz), out RayHit face) && face.distance <= d.probeDistance + d.radius)
                    {
                        blocked = true;
                        look = new Vec3(position.x + ux * (face.distance + d.radius), position.y, position.z + uz * (face.distance + d.radius));
                    }
                    else if (blocked) look = new Vec3(probe.x + ux * d.radius, probe.y, probe.z + uz * d.radius);
                }
                if (blocked)
                {
                    if (climbHoldSteps == 0) // NIGHT20: a climb that gave up doesn't re-latch for climbHold s (0 with the block off)
                    {
                        var above = new Vec3(look.x, position.y + d.climbProbeHeight, look.z);
                        // NIGHT20 (roofCheck): look down from under the first roof above the drone (the Survival map's 40 m roof
                        // barrier, an overhang), not from on top of it (the web's look-down lands on the roof: a 43.6 m climb).
                        // From the drone, not the probe: the probe's centre is often inside the wall it met.
                        if (roofCheck && world.RayIntersect(position, Up, out RayHit roof) && roof.distance < d.climbProbeHeight)
                            above.y = position.y + roof.distance - RoofGap;
                        climbTo = Math.Max(climbTo, GroundBelow(world, above) + d.climbAbove + d.radius);
                    }
                }
                else if (position.y >= climbTo - d.radius)
                {
                    climbTo = double.NegativeInfinity;
                }
            }
            bool climbing = climbTo > position.y + d.radius;
            if (climbStall > 0)
            {
                // NIGHT20: a climb that gains no height for climbStall s (a roof over it) lets go and holds off climbHold s
                if (climbHoldSteps > 0) climbHoldSteps--;
                if (climbing)
                {
                    if (velocity.y > RiseEpsilon) climbStallSteps = 0; // rising (a roof's push-out leaves it ~0)
                    else if (++climbStallSteps >= MovementMath.StepsFor(climbStall, dt))
                    {
                        climbTo = double.NegativeInfinity;
                        climbing = false;
                        climbStallSteps = 0;
                        climbHoldSteps = MovementMath.StepsFor(climbHold, dt);
                    }
                }
                else climbStallSteps = 0;
            }
            if (climbing)
            {
                vx = 0;
                vz = 0;
            }
            double targetY = Math.Max(GroundBelow(world, position) + hover, climbTo);
            double vy = Math.Max(-d.speed, Math.Min(d.speed, (targetY - position.y) * d.accel));

            double k = Math.Min(1, d.accel * dt);
            velocity.x += (vx - velocity.x) * k;
            velocity.y += (vy - velocity.y) * k;
            velocity.z += (vz - velocity.z) * k;

            // Light separation from other drones.
            for (int i = 0; i < pool.Count; i++) // indexed: a foreach over the interface boxes its enumerator (U2-02)
            {
                if (!(pool[i] is Drone other) || !other.alive || other == this) continue;
                double ox = position.x - other.position.x;
                double oy = position.y - other.position.y;
                double oz = position.z - other.position.z;
                double od = JsMath.Hypot(ox, oy, oz);
                double min = d.separation;
                if (od > 0 && od < min)
                {
                    double push = (min - od) / min * d.speed * dt * d.accel;
                    velocity.x += ox / od * push;
                    velocity.y += oy / od * push;
                    velocity.z += oz / od * push;
                }
            }

            position += velocity * dt;
            if (SphereIntersect(world, position, out Vec3 n, out double depth))
            {
                position += n * depth;
                double into = Vec3.Dot(velocity, n);
                if (into < 0) velocity -= n * into;
            }

            // Fire: line of sight to the eye -> 0.4 s charge -> orb. A brain supplies the (budgeted) line of sight,
            // the ranged token and the aim (a lead + deliberate misses that grow with your speed).
            if (fireSteps > 0) fireSteps--;
            if (chargeSteps > 0)
            {
                if (--chargeSteps == 0)
                {
                    if (!target.Dead && (ai != null ? ai.los : HasLos(world, position, eye)))
                    {
                        Vec3 dir;
                        if (ai != null)
                        {
                            dir = Aim(position, tp);
                            events.Emit("enemyShot", id); // brain.js: {id, kind 'drone', hit, speed} (the port's husks emit ids)
                        }
                        else dir = Vec3.Normalize(eye - position);
                        orbs.Fire(position + dir * (d.radius + d.orbRadius), dir, this, d.orbSpeed); // NIGHT40 E3: d.orbSpeed (orb.speed with foes40 off: the same shot, life x1)
                    }
                    fireSteps = MovementMath.StepsFor(d.fireInterval - d.chargeTime, dt); // one orb every fireInterval
                    if (ai != null) events.Emit("enemyAttackDone", id);
                }
            }
            else if (fireSteps == 0 && !target.Dead && (ai != null ? ai.attack && ai.los : HasLos(world, position, eye)))
            {
                chargeSteps = MovementMath.StepsFor(d.chargeTime, dt);
                said.id = id;
                events.Emit("droneCharge", said);
                if (ai != null) events.Emit("enemyCommit", id);
            }
            committed = chargeSteps > 0;
            Airborne = true;
        }
    }

    /// <summary>
    /// drone.js createOrbs: the orbs, pooled (orb.poolSize, made once). An orb flies straight at orb.speed for orb.lifetime
    /// (x lifeMult for a slower shot); geometry destroys it; it hurts the player it touches for orb.damage (a dashing player's
    /// i-frames let it through: it ignores that player from then on); a reflected orb homes on its target at
    /// reflectSpeedMult x speed for reflectDamageMult x damage and hits the first enemy it overlaps (onReflectHit: the
    /// player's loadout, weapon "orb").
    /// </summary>
    public sealed partial class DroneOrbs
    {
        public sealed class Orb
        {
            public bool alive, reflected, dodged;
            public Vec3 position, previous, velocity;
            public int ageSteps;
            /// <summary>NIGHT42 W1 parry42: steps left of the mercy window after this orb touched the player (0: none). While it runs the orb
            /// waits at the contact point and a punch / swing can still send it back; the damage lands when it runs out.</summary>
            public int mercy42;
            public double damage, lifeMult = 1;
            public Husk shooter; // the Drone that fired it (stuck rule: dealt damage)
            public Enemy target; // the enemy a reflected orb homes on
        }

        /// <summary>orbDestroyed {reason, position} (one reused payload).</summary>
        public sealed class DestroyedEvent
        {
            public string reason;
            public Vec3 position;
        }

        public readonly Orb[] pool;
        public Func<Enemy, double, Vec3, bool> onReflectHit; // how a reflected orb damages an enemy (the sim wires the player's loadout)
        /// <summary>NIGHT36 P parry36 (SurvivalSim.Core36 sets it with the row on; null: off): a reflected orb ended on a body or a wall here.</summary>
        public Action<Vec3> onReflectEnd36;
        /// <summary>NIGHT40 E3 foes40 (design.n40.foes40.drone.reflectKillsShooter; the sim sets it at a run's start, false with the row off): a reflected
        /// orb that reaches the drone that fired it lands at least that drone's hit points (a kill).</summary>
        public bool reflectKillsShooter;
        /// <summary>NIGHT40 E3: reflected orbs that reached their own shooter (the harness reads it).</summary>
        public int shooterReturns;
        readonly EventBus events;
        readonly double speed, damage, radius, lifetime, reflectRange, reflectSpeedMult, reflectDamageMult, capsuleRadius, capsuleHeight, dt;
        readonly DestroyedEvent destroyed = new DestroyedEvent();
        public int fired, reflects; // counters (tests, debug)

        public DroneOrbs(GameConfig cfg, EventBus events)
        {
            this.events = events;
            speed = cfg.Num("orb.speed");
            damage = cfg.Num("orb.damage");
            radius = cfg.Num("orb.radius");
            lifetime = cfg.Num("orb.lifetime");
            reflectRange = cfg.Num("orb.reflectRange");
            reflectSpeedMult = cfg.Num("orb.reflectSpeedMult");
            reflectDamageMult = cfg.Num("orb.reflectDamageMult");
            capsuleRadius = cfg.movement.body.capsuleRadius;
            capsuleHeight = cfg.movement.body.capsuleHeight;
            dt = cfg.Dt;
            pool = new Orb[cfg.Int("orb.poolSize")];
            for (int i = 0; i < pool.Length; i++) pool[i] = new Orb();
        }

        public double Radius => radius;

        void Kill(Orb orb, string reason)
        {
            orb.alive = false;
            destroyed.reason = reason;
            destroyed.position = orb.position;
            events.Emit("orbDestroyed", destroyed);
        }

        /// <summary>Fire an orb from `from` along unit `dir` at orb.speed; null when the pool is full.</summary>
        public Orb Fire(Vec3 from, Vec3 dir, Husk shooter) => Fire(from, dir, shooter, speed);

        public Orb Fire(Vec3 from, Vec3 dir, Husk shooter, double shotSpeed)
        {
            Orb orb = null;
            foreach (Orb o in pool)
            {
                if (o.alive) continue;
                orb = o;
                break;
            }
            if (orb == null) return null;
            orb.alive = true;
            orb.position = from;
            orb.previous = from;
            orb.velocity = dir * shotSpeed;
            orb.ageSteps = 0;
            orb.mercy42 = 0;
            orb.damage = damage;
            orb.reflected = false;
            orb.dodged = false;
            orb.shooter = shooter;
            orb.target = null;
            orb.lifeMult = Math.Max(1, speed / shotSpeed); // the same range at any speed (slow boss orbs live longer)
            fired++;
            events.Emit("orbFire", orb);
            return orb;
        }

        /// <summary>Nearest live hostile orb within `range` of a point (the punch reflect).</summary>
        /// <summary>NIGHT42 W1 parry42 (N42Blocks.parry42, design.n42.parry42; the sim sets these at a run's start, SurvivalSim.N42.Weapons.cs; off:
        /// false / 0, the reach above bit for bit). GG 2026-10-14: "the parry attack timing from the flying enimies is relaly tight and you miss
        /// depsite felling like you hit it". ULTRAKILL 1.03b (hitfeel-web.md section 4): a hitbox test instead of a ray, and 3 mercy frames
        /// between a projectile touching the player and the damage. Here: the reach is a VOLUME (the orb's surface within reach42 of the body's
        /// axis, the eye down to the capsule's lower sphere, not the eye's point), reach42 = reflectRange x reachMult (DECIDE42 D-3: 1.5x), and a
        /// touching orb waits mercySteps42 before its damage lands (a punch in that time still parries).</summary>
        public bool parry42;
        public double reach42;
        public int mercySteps42;
        /// <summary>parry42's counters (the harness reads them): orbs held in a mercy window, orbs parried inside one.</summary>
        public int mercyHeld42, mercyParried42;

        /// <summary>parry42: the nearest live hostile orb whose surface is within reach42 of the body's axis below `eye`.</summary>
        public Orb NearestHostile42(Vec3 eye)
        {
            Orb best = null;
            double bestD = reach42;
            double drop = capsuleHeight - 2 * capsuleRadius; // the eye to the lower sphere's centre (the capsule's axis, about)
            foreach (Orb orb in pool)
            {
                if (!orb.alive || orb.reflected) continue;
                double dd = PointSegmentDistance(orb.position, eye.x, eye.y, eye.z, eye.x, eye.y - drop, eye.z) - radius;
                if (dd <= bestD)
                {
                    best = orb;
                    bestD = dd;
                }
            }
            return best;
        }

        public Orb NearestHostile(Vec3 point, double range)
        {
            Orb best = null;
            double bestD = range;
            foreach (Orb orb in pool)
            {
                if (!orb.alive || orb.reflected) continue;
                double dd = Vec3.DistanceTo(orb.position, point);
                if (dd <= bestD)
                {
                    best = orb;
                    bestD = dd;
                }
            }
            return best;
        }

        /// <summary>Send an orb back at reflectSpeedMult x speed and reflectDamageMult x damage, homing on `enemy` while it lives, else along `fallbackDir`.</summary>
        public void Reflect(Orb orb, Enemy enemy, Vec3 fallbackDir)
        {
            orb.velocity = fallbackDir * (speed * reflectSpeedMult);
            orb.damage = damage * reflectDamageMult;
            orb.reflected = true;
            orb.target = enemy;
            orb.ageSteps = 0;
            if (orb.mercy42 > 0) mercyParried42++; // NIGHT42 W1 parry42: sent back inside its mercy window
            orb.mercy42 = 0;
            reflects++;
        }

        /// <summary>punch.js tryReflect: the nearest hostile orb within orb.reflectRange of the eye goes back at the enemy nearest to it
        /// (NIGHT42 W1 parry42: within reach42 of the body, NearestHostile42).</summary>
        public Orb TryReflect(Vec3 origin, Vec3 dir, EnemyRegistry enemies)
        {
            Orb orb = parry43 ? NearestHostile43(origin, dir) : parry42 ? NearestHostile42(origin) : NearestHostile(origin, reflectRange); // NIGHT43 W1 parry43: the catch volume + the time window (Core/Parry43.cs; off: NIGHT42's)
            if (orb == null) return null;
            double best = double.PositiveInfinity;
            Enemy nearest = null;
            foreach (Enemy e in enemies.all)
            {
                if (!e.alive) continue;
                double dd = Vec3.DistanceTo(e.AimPoint, orb.position);
                if (dd < best)
                {
                    best = dd;
                    nearest = e;
                }
            }
            Reflect(orb, nearest, dir);
            return orb;
        }

        public void Clear()
        {
            foreach (Orb orb in pool) orb.alive = false;
        }

        /// <summary>Distance from point p to the segment a-b (the player's capsule axis).</summary>
        static double PointSegmentDistance(Vec3 p, double ax, double ay, double az, double bx, double by, double bz)
        {
            double abx = bx - ax, aby = by - ay, abz = bz - az;
            double len2 = abx * abx + aby * aby + abz * abz;
            double t = len2 > 0 ? ((p.x - ax) * abx + (p.y - ay) * aby + (p.z - az) * abz) / len2 : 0;
            t = Math.Max(0, Math.Min(1, t));
            return JsMath.Hypot(p.x - (ax + abx * t), p.y - (ay + aby * t), p.z - (az + abz * t));
        }

        /// <summary>The first live enemy a sphere overlaps (enemies.hitEnemy: each kind's hitSphere).</summary>
        static Enemy HitEnemy(EnemyRegistry enemies, Vec3 point, double r)
        {
            foreach (Enemy e in enemies.all)
            {
                if (!e.alive) continue;
                if (e is Drone)
                {
                    if (Vec3.DistanceTo(e.position, point) <= r + e.Radius) return e;
                    continue;
                }
                // enemyKind.js hitSphere: the nearest point on its vertical axis
                double y = Math.Max(e.position.y + e.Radius, Math.Min(e.position.y + e.Height - e.Radius, point.y));
                if (JsMath.Hypot(point.x - e.position.x, point.y - y, point.z - e.position.z) <= r + e.Radius) return e;
            }
            return null;
        }

        /// <summary>One fixed step: move, age out, hit geometry, the players (hostile) or enemies (reflected).</summary>
        public void Step(Players players, Vec3 soloPos, Health soloHealth, ICollisionWorld world, EnemyRegistry enemies, double stepDt)
        {
            int baseLife = MovementMath.StepsFor(lifetime, stepDt);
            foreach (Orb orb in pool)
            {
                if (!orb.alive) continue;
                if (orb.mercy42 > 0)
                {
                    // NIGHT42 W1 parry42: the orb touched the player and waits at the contact point; a parry (Reflect) clears it.
                    if (--orb.mercy42 > 0) continue;
                    MercyLands42(orb, players, soloPos, soloHealth);
                    continue;
                }
                orb.previous = orb.position;
                if (orb.reflected && orb.target != null && orb.target.alive)
                {
                    // Home on the target so a parried orb always returns to sender.
                    orb.velocity = Vec3.Normalize(orb.target.AimPoint - orb.position) * (speed * reflectSpeedMult);
                }
                orb.position += orb.velocity * stepDt;
                if (++orb.ageSteps >= baseLife * orb.lifeMult)
                {
                    Kill(orb, "expired");
                    continue;
                }
                // A reflected orb checks its target before the level.
                if (orb.reflected)
                {
                    Enemy enemy = HitEnemy(enemies, orb.position, radius);
                    if (enemy != null)
                    {
                        double landed = orb.damage;
                        if (enemy == orb.shooter)
                        {
                            shooterReturns++;
                            if (reflectKillsShooter && enemy is Husk sh && sh.hp > landed) landed = sh.hp; // NIGHT40 E3 foes40: its own orb kills it
                        }
                        onReflectHit?.Invoke(enemy, landed, orb.position);
                        Kill(orb, "enemy");
                        onReflectEnd36?.Invoke(orb.position); // NIGHT36 P parry36: the return bursts (null: off)
                        continue;
                    }
                }
                if (WorldCollider.CapsuleIntersect(world, new Capsule { start = orb.position, end = orb.position, radius = radius }, out _, out _))
                {
                    Kill(orb, "geometry");
                    if (orb.reflected) onReflectEnd36?.Invoke(orb.position); // NIGHT36 P parry36: a wall sets a return off too
                    continue;
                }
                if (orb.reflected || orb.dodged) continue;
                Vec3 p;
                Health health;
                if (players != null)
                {
                    Player t = players.Nearest(orb.position);
                    p = t.Feet;
                    health = t.health;
                }
                else
                {
                    p = soloPos;
                    health = soloHealth;
                }
                if (health == null || health.Dead) continue;
                double dd = PointSegmentDistance(orb.position, p.x, p.y + capsuleRadius, p.z, p.x, p.y + capsuleHeight - capsuleRadius, p.z);
                if (dd > radius + capsuleRadius) continue;
                if (parry42 && mercySteps42 > 0)
                {
                    // NIGHT42 W1 parry42: the mercy window opens; the damage lands when it runs out (MercyLands42)
                    orb.mercy42 = mercySteps42;
                    mercyHeld42++;
                    continue;
                }
                // x / z: where it came from (the damage-direction indicator)
                var from = new Vec3(orb.position.x - orb.velocity.x * 0.2, orb.position.y, orb.position.z - orb.velocity.z * 0.2);
                if (health.Damage(orb.damage, "drone", from))
                {
                    if (orb.shooter != null) orb.shooter.dealtDamage = true;
                    Kill(orb, "player");
                }
                else orb.dodged = true; // dash i-frames: it flies on through
            }
        }

        /// <summary>NIGHT42 W1 parry42: a held orb's mercy ran out with no parry: its damage lands now on the player nearest it (as a touch
        /// would have), and it dies; a dash's i-frames at that moment let it fly on (dodged), as before.</summary>
        void MercyLands42(Orb orb, Players players, Vec3 soloPos, Health soloHealth)
        {
            Health health = players != null ? players.Nearest(orb.position).health : soloHealth;
            if (health == null || health.Dead) return;
            var from = new Vec3(orb.position.x - orb.velocity.x * 0.2, orb.position.y, orb.position.z - orb.velocity.z * 0.2);
            if (health.Damage(orb.damage, "drone", from))
            {
                if (orb.shooter != null) orb.shooter.dealtDamage = true;
                Kill(orb, "player");
            }
            else orb.dodged = true;
        }
    }
}
