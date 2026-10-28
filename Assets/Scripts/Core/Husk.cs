// Port of skygrind/src/enemies/husk.js (SPEC §5 + 1.4): chases on the ground, a telegraphed melee (a wind-up, then the
// hit lands only if the player is still in reach), leaps up to a player above it; a punch knocks it back, a punch in
// its wind-up (a parry) staggers it. A kinematic body with the world's aggregated capsule push-out (three's games_fps
// approach). Hit shapes: the hunched head sphere (headshot) and the body capsule. With a brain (Survival), the steering
// velocity and the attack permission come from it (HuskBrain); without one it chases straight.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class HuskParams
    {
        public double hp, speed, stopDistance, meleeRange, meleeHitRange, windupTime, meleeDamage, meleeCooldown;
        public double leapMinHeight, leapMaxHorizontal, leapApexAbove, leapCooldown, radius, height;
        public double headRadius, headHeight, headForward, aimHeight, spawnLift, separationRadius, separationSpeed, hitFlashTime;
        public double edgeNormalY, edgeRiseSpeed, supportedDrop, floorMinNormalY, killPlaneY;
        /// <summary>NIGHT19 (builder E): surface.wallMaxAbsNormalY, enemyKind.js groundMove's hitWall test (the CHARGER's stun);
        /// 0 for the kinds that never read it (Move's flag is then never set: nothing else changes).</summary>
        public double wallMaxAbsNormalY;
        public string kind = "husk";
        // The Swarmer's pounce (swarmer.js)
        public double pounceRange, tellTime, pounceApex, biteRange, biteDamage, recoverTime, attackCooldown;

        public static HuskParams From(GameConfig c, double killPlaneY) => new HuskParams
        {
            hp = c.Num("husk.hp"), speed = c.Num("husk.speed"), stopDistance = c.Num("husk.stopDistance"),
            meleeRange = c.Num("husk.meleeRange"), meleeHitRange = c.Num("husk.meleeHitRange"), windupTime = c.Num("husk.windupTime"),
            meleeDamage = c.Num("husk.meleeDamage"), meleeCooldown = c.Num("husk.meleeCooldown"), leapMinHeight = c.Num("husk.leapMinHeight"),
            leapMaxHorizontal = c.Num("husk.leapMaxHorizontal"), leapApexAbove = c.Num("husk.leapApexAbove"), leapCooldown = c.Num("husk.leapCooldown"),
            radius = c.Num("husk.radius"), height = c.Num("husk.height"), headRadius = c.Num("husk.headRadius"), headHeight = c.Num("husk.headHeight"),
            headForward = c.Num("husk.headForward"), aimHeight = c.Num("husk.aimHeight"), spawnLift = c.Num("husk.spawnLift"),
            separationRadius = c.Num("husk.separationRadius"), separationSpeed = c.Num("husk.separationSpeed"), hitFlashTime = c.Num("husk.hitFlashTime"),
            edgeNormalY = c.Num("ai.edgeNormalY"), edgeRiseSpeed = c.Num("ai.edgeRiseSpeed"), supportedDrop = c.Num("ai.supportedDrop"),
            floorMinNormalY = c.movement.surface.floorMinNormalY, killPlaneY = killPlaneY,
        };

        /// <summary>config.swarmer through enemyKind.js (no head; lift 0.05; flash hitFlashTime ?? 0.08).</summary>
        public static HuskParams Swarmer(GameConfig c, double killPlaneY) => new HuskParams
        {
            kind = "swarmer", hp = c.Num("swarmer.hp"), speed = c.Num("swarmer.speed"), radius = c.Num("swarmer.radius"), height = c.Num("swarmer.height"),
            aimHeight = c.Num("swarmer.aimHeight"), spawnLift = 0.05, hitFlashTime = 0.08,
            pounceRange = c.Num("swarmer.pounceRange"), tellTime = c.Num("swarmer.tellTime"), pounceApex = c.Num("swarmer.pounceApex"),
            biteRange = c.Num("swarmer.biteRange"), biteDamage = c.Num("swarmer.damage"), recoverTime = c.Num("swarmer.recoverTime"), attackCooldown = c.Num("swarmer.cooldown"),
            edgeNormalY = c.Num("ai.edgeNormalY"), edgeRiseSpeed = c.Num("ai.edgeRiseSpeed"), supportedDrop = c.Num("ai.supportedDrop"),
            floorMinNormalY = c.movement.surface.floorMinNormalY, killPlaneY = killPlaneY,
        };

        /// <summary>config.spitter through enemyKind.js (no head; lift 0.05; aim at height x 0.55; flash hitFlashTime ?? 0.08).</summary>
        public static HuskParams Spitter(GameConfig c, double killPlaneY) => new HuskParams
        {
            kind = "spitter", hp = c.Num("spitter.hp"), speed = c.Num("spitter.speed"), radius = c.Num("spitter.radius"), height = c.Num("spitter.height"),
            aimHeight = c.Has("spitter.aimHeight") ? c.Num("spitter.aimHeight") : c.Num("spitter.height") * 0.55, spawnLift = 0.05,
            hitFlashTime = c.Has("spitter.hitFlashTime") ? c.Num("spitter.hitFlashTime") : 0.08,
            edgeNormalY = c.Num("ai.edgeNormalY"), edgeRiseSpeed = c.Num("ai.edgeRiseSpeed"), supportedDrop = c.Num("ai.supportedDrop"),
            floorMinNormalY = c.movement.surface.floorMinNormalY, killPlaneY = killPlaneY,
        };
    }

    /// <summary>What the brain asks of a husk this step (enemyKind.js / brain.js: the desired velocity, may it attack).</summary>
    public sealed class HuskAi
    {
        public double vx, vz;
        public bool attack = true;
        public bool flank = true; // a Swarmer: outside your front cone at the last think
        public Func<Vec3, bool> flankNow; // a Swarmer: outside your front cone right now (checked as it crouches)
        public bool los = true; // a Spitter: it saw you at its last line-of-sight check (brain.js rec.los)
        public bool hit = true; // a Spitter: this attack's roll (ai/aim.js hitChance); false aims beside you
        public bool blocked; // NIGHT19 (brain.js rec.out.blocked): a kind that can't jump met a jump (the CHARGER stuns itself)
    }

    public class Husk : Enemy
    {
        protected readonly HuskParams h;
        public bool grounded;
        public bool leaping;
        public bool onEdge;
        public bool committed;
        /// <summary>NIGHT19 (builder E): enemyKind.js groundMove's e.hitWall: the last Move pushed it off a wall (|n.y| under
        /// surface.wallMaxAbsNormalY) while not grounded. Read by the CHARGER only.</summary>
        public bool hitWall;
        /// <summary>NIGHT19 (builder E): thrown up by a launch (Launch) and not landed yet: Enemy.Airborne (LAUNCH + SNIPE).</summary>
        public bool launched;
        /// <summary>NIGHT19 (builder E): SHIELDED's plate half-angle (affixes.kinds.shielded.halfDeg) while Enemy.Shielded.</summary>
        public double plateHalfDeg;
        public int cooldownSteps;
        public int leapCooldownSteps;
        public double windupAge; // s into the current wind-up (visual)
        public double lean; // rad, visual stagger lean
        public HuskAi ai; // null: the plain chase
        public NavJumpState jump; // a nav jump in flight (NavJumper)
        // NIGHT20 U3-34.6 (builder N, design.routes; Core/Nav/RouteLinks.cs RouteJumper): set and read only with the layer on
        // (the husk pool's jumper a RouteJumper); with it off nothing reads them and the step is the web's.
        /// <summary>The highest point of the fall it is in (the landing stagger past ai.grid.drop).</summary>
        public double fallTop;
        /// <summary>Knocked off a climb or a leap: it stands for dropStagger where it lands.</summary>
        public bool landStagger;
        /// <summary>The leap's approach point (the brain steers to it while leapFromSteps runs).</summary>
        public Vec3 leapFrom;
        public int leapFromSteps;
        /// <summary>The layer's own jump state (a leap or a climb; made once per body).</summary>
        public NavJumpState leapState;
        /// <summary>The brain's say on the leap: only where its field has no link onward (under a perch) or on the way to
        /// the approach point; a field with a climb onward climbs instead (true without a brain).</summary>
        public bool leapOk = true;
        RouteJumper routeJumper; // this step's jumper when it is the layer's
        /// <summary>NIGHT20 (builder N, for builder E's SWIFT affix): this enemy's own pace (affixes.js e.speedMult), x the
        /// brain's speed cap for this enemy only (brain.js: k.speed * (e.speedMult ?? 1)); 1 at every spawn (x 1 is exact).</summary>
        public double paceMult = 1;

        public Husk(HuskParams h)
        {
            this.h = h;
            kind = h.kind;
        }

        public HuskParams Params => h;
        public virtual bool HasHead => true;

        public override double Height => h.height;
        public override double Radius => h.radius;
        public override double AimHeight => h.aimHeight;

        public Capsule Capsule => new Capsule
        {
            start = new Vec3(position.x, position.y + h.radius, position.z),
            end = new Vec3(position.x, position.y + h.height - h.radius, position.z),
            radius = h.radius,
        };

        /// <summary>The hunched head sphere's centre: above and in front of the body axis.</summary>
        public Vec3 HeadCenter => new Vec3(position.x - JsMath.Sin(yaw) * h.headForward, position.y + h.headHeight, position.z - JsMath.Cos(yaw) * h.headForward);

        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            double head = CombatMath.RaySphere(o, d, HeadCenter, h.headRadius);
            double body = CombatMath.RayVerticalCapsule(o, d, position, h.height, h.radius);
            headshot = head >= 0 && (body < 0 || head <= body);
            return headshot ? head : body;
        }

        public virtual bool Spawn(int newId, Vec3 pos, Vec3 target, ICollisionWorld world)
        {
            position = new Vec3(pos.x, pos.y + h.spawnLift, pos.z);
            if (WorldCollider.CapsuleIntersect(world, Capsule, out _, out _)) return false;
            id = newId;
            alive = true;
            hp = maxHp = h.hp;
            tank29 = standIn29 = false; // NIGHT29: a reused body is a plain husk (Husks.Spawn makes the tank after this)
            grounded = leaping = onEdge = committed = dealtDamage = false;
            hitWall = launched = false; // NIGHT19
            Airborne = Shielded = false; // NIGHT19: a reused body starts plain (the affixes roll after the spawn)
            windupSteps = cooldownSteps = leapCooldownSteps = knockSteps = staggerSteps = 0;
            flash = lean = windupAge = 0;
            ai = null;
            jump = null;
            fallTop = position.y; // NIGHT20 (routes)
            landStagger = false;
            leapFromSteps = 0;
            leapOk = true;
            paceMult = 1; // NIGHT20: a reused body starts at its kind's pace (SWIFT sets it after the spawn)
            previous = position;
            velocity = default;
            yaw = JsMath.Atan2(-(target.x - pos.x), -(target.z - pos.z));
            return true;
        }

        public override void Stagger(double time, double dt)
        {
            base.Stagger(time, dt);
            committed = false;
        }

        /// <summary>husk.js stun (NIGHT9 U3-09): like a parry stagger, whatever it was doing (the longer stagger wins).</summary>
        public override void Stun(double time, double dt)
        {
            windupSteps = 0;
            committed = false;
            staggerSteps = Math.Max(staggerSteps, MovementMath.StepsFor(time, dt));
        }

        /// <summary>The watchdog's relocation (enemies.teleport): placed at the spot, still, its attack dropped.</summary>
        public virtual void Teleport(Vec3 spot)
        {
            position = new Vec3(spot.x, spot.y + h.spawnLift, spot.z);
            previous = position;
            velocity = default;
            windupSteps = 0;
            leaping = false;
            jump = null;
            fallTop = position.y; // NIGHT20 (routes): a relocation is no fall
            leapFromSteps = 0;
        }

        /// <summary>One fixed step: a nav jump owns the body (NavJumper), or the husk's own step runs.</summary>
        public virtual void Tick(NavJumper jumper, Vec3 tp, Health target, IReadOnlyList<Enemy> pool, ICollisionWorld world, EventBus events, double dt, double gravity)
        {
            bool busy = windupSteps > 0 || staggerSteps > 0 || knockSteps > 0 || leaping;
            routeJumper = jumper as RouteJumper; // NIGHT20: null with design.routes off (the web's leap in Step)
            if (jumper != null && jumper.Step(this, dt, busy))
            {
                if (knockSteps > 0) knockSteps--;
                Move(world, dt, gravity);
            }
            else Step(tp, target, pool, world, events, dt, gravity);
        }

        readonly HuskAttackEvent attackEvent = new HuskAttackEvent(); // reused per attack (U2-02; no handler keeps it)

        /// <summary>The melee hit at the end of the wind-up (SPEC 1.4): lands only if the target is still in reach.</summary>
        void ResolveAttack(Vec3 targetPos, Health target, EventBus events)
        {
            double d = Vec3.DistanceTo(targetPos, position);
            bool hit = false;
            if (d <= h.meleeHitRange)
            {
                hit = target.Damage(h.meleeDamage, "husk", position);
                if (hit) dealtDamage = true;
            }
            attackEvent.id = id;
            attackEvent.hit = hit;
            attackEvent.distance = d;
            events.Emit("huskAttack", attackEvent);
        }

        /// <summary>husk.js stepHusk: the tell, the attack, the leap, the chase (or the brain's velocity), then the move.</summary>
        public void Step(Vec3 tp, Health target, IReadOnlyList<Enemy> pool, ICollisionWorld world, EventBus events, double dt, double gravity)
        {
            double dx = tp.x - position.x;
            double dz = tp.z - position.z;
            double horizontal = JsMath.Hypot(dx, dz);
            double above = tp.y - position.y;
            double distance = JsMath.Hypot(dx, above, dz);
            if (horizontal > 0) yaw = JsMath.Atan2(-dx, -dz);

            if (cooldownSteps > 0) cooldownSteps--;
            if (leapCooldownSteps > 0) leapCooldownSteps--;
            bool knocked = knockSteps > 0;
            bool stunned = knocked || staggerSteps > 0;
            if (knockSteps > 0) knockSteps--;
            if (staggerSteps > 0) staggerSteps--;

            if (windupSteps > 0)
            {
                windupSteps--;
                if (windupSteps == 0)
                {
                    ResolveAttack(tp, target, events);
                    cooldownSteps = MovementMath.StepsFor(h.meleeCooldown, dt);
                    if (ai != null) events.Emit("enemyAttackDone", id);
                }
            }
            else if (!stunned && grounded && cooldownSteps == 0 && distance <= h.meleeRange && !target.Dead && (ai == null || ai.attack))
            {
                windupSteps = MovementMath.StepsFor(h.windupTime, dt);
                windupAge = 0;
                events.Emit("huskWindup", id);
                if (ai != null) events.Emit("enemyCommit", id);
            }
            bool windingUp = windupSteps > 0;
            committed = windingUp;

            bool standing = grounded || (ai != null && onEdge);
            if (standing && knocked)
            {
                // Slide with the knockback velocity.
            }
            else if (standing)
            {
                bool leapNow = !windingUp && !stunned && leapCooldownSteps == 0 && above >= h.leapMinHeight && horizontal <= h.leapMaxHorizontal;
                // NIGHT20 U3-34.6 step 2 (builder N, design.routes): the leap planned (a checked arc, a DANGER crouch, an
                // approach point) by the layer's jumper instead of fired here; with the layer off, the web's leap below.
                if (leapNow && routeJumper != null) leapNow = leapOk && routeJumper.PlanLeap(this, tp, world, dt);
                if (leapNow && routeJumper != null)
                {
                    velocity.x = 0; // the crouch starts next step (RouteJumper)
                    velocity.z = 0;
                }
                else if (leapNow)
                {
                    double stop = horizontal > 0 ? Math.Min(h.stopDistance, horizontal) / horizontal : 0;
                    var to = new Vec3(tp.x - dx * stop, tp.y, tp.z - dz * stop);
                    velocity = CombatMath.LeapVelocity(position, to, h.leapApexAbove, gravity, out _);
                    grounded = false;
                    leaping = true;
                    leapCooldownSteps = MovementMath.StepsFor(h.leapCooldown, dt);
                    events.Emit("huskLeap", id);
                }
                else if (ai != null)
                {
                    // NIGHT20 (routes): a husk under you walking to its leap's approach point doesn't stop there.
                    bool stopHere = ai.attack && horizontal <= h.stopDistance && !(routeJumper != null && leapFromSteps > 0);
                    velocity.x = windingUp || stunned || stopHere ? 0 : ai.vx;
                    velocity.z = windingUp || stunned || stopHere ? 0 : ai.vz;
                }
                else
                {
                    double vx = 0, vz = 0;
                    if (!windingUp && !stunned && horizontal > h.stopDistance)
                    {
                        double speed = Math.Min(h.speed, (horizontal - h.stopDistance) / dt);
                        vx = dx / horizontal * speed;
                        vz = dz / horizontal * speed;
                    }
                    for (int i = 0; i < pool.Count; i++) // indexed: a foreach over the interface boxes its enumerator (U2-02)
                    {
                        Enemy other = pool[i];
                        if (!other.alive || other == this) continue;
                        double ox = position.x - other.position.x, oz = position.z - other.position.z;
                        double od = JsMath.Hypot(ox, oz);
                        if (od > 0 && od < h.separationRadius)
                        {
                            double push = (1 - od / h.separationRadius) * h.separationSpeed;
                            vx += ox / od * push;
                            vz += oz / od * push;
                        }
                    }
                    velocity.x = vx;
                    velocity.z = vz;
                }
            }
            Move(world, dt, gravity);
        }

        /// <summary>husk.js moveHusk: gravity + the velocity + the aggregated capsule push-out, grounded / edge rules.</summary>
        public void Move(ICollisionWorld world, double dt, double gravity)
        {
            double startY = position.y;
            VerticalStepResult v = MovementMath.VerticalStep(velocity.y, gravity, dt);
            velocity.y = v.vy;
            position.x += velocity.x * dt;
            position.y += v.dy;
            position.z += velocity.z * dt;
            Capsule cap = Capsule;
            grounded = false;
            onEdge = false;
            hitWall = false;
            if (WorldCollider.CapsuleIntersect(world, cap, out Vec3 n, out double depth))
            {
                onEdge = n.y < h.floorMinNormalY && n.y > h.edgeNormalY;
                grounded = n.y >= h.floorMinNormalY || (onEdge && velocity.y <= h.edgeRiseSpeed);
                if (grounded)
                {
                    if (velocity.y < 0) velocity.y = 0;
                }
                else
                {
                    if (Math.Abs(n.y) < h.wallMaxAbsNormalY) hitWall = true; // NIGHT19: enemyKind.js e.hitWall (0 for the kinds that never read it)
                    double into = Math.Min(0, Vec3.Dot(n, velocity));
                    velocity = velocity - n * into;
                }
                cap.Translate(n * depth);
                position = new Vec3(cap.start.x, cap.start.y - h.radius, cap.start.z);
                if (!grounded && velocity.y <= 0 && Math.Abs(position.y - startY) < h.supportedDrop) onEdge = true;
            }
            if (grounded) leaping = false;
            // NIGHT19 (builder E): Enemy.Airborne for LAUNCH + SNIPE (builder W's combo): a launch until it lands. Not read
            // with the NIGHT19 blocks off (no number in the step changes).
            if (grounded) launched = false;
            Airborne = launched;
        }

        /// <summary>husk.js launch (NIGHT19: virtual, enemyKind.js scales it by knockbackScale: the CHARGER).</summary>
        public virtual void Launch(double up)
        {
            velocity.y = Math.Max(velocity.y, up);
            grounded = false;
            launched = true; // NIGHT19: Enemy.Airborne until it lands
            Airborne = true;
        }
    }

    public sealed class HuskAttackEvent
    {
        public int id;
        public bool hit;
        public double distance;
    }

    /// <summary>A nav jump in flight (enemies/navJump.js): crouch, then a ballistic arc to the link's landing.</summary>
    public sealed class NavJumpState
    {
        /// <summary>NIGHT20 (builder N, design.routes): what owns the body: the web's nav jump (Web), the layer's leap or climb.</summary>
        public const byte Web = 0, Leap = 1, Climb = 2;
        public byte verb;
        /// <summary>NIGHT20: where the verb started (a leap's crouch spot, a climb's foot at the face).</summary>
        public Vec3 foot;
        /// <summary>NIGHT20: a climb's phase (0 the DANGER tell at the foot, 1 up the face, 2 the mantle), its link kind (Link 5-7)
        /// and the face's outward normal (the view poses the body against it).</summary>
        public int phase, phaseSteps;
        public byte linkKind;
        public Vec3 normal;
        public int crouchSteps = -1; // -1: a fresh request, not started
        public Vec3 to;
        public bool hop; // the watchdog's unstick hop (lands on the spot, no landIn)
        public double apex; // 0: the kind's apex
        public bool flying;
        public int flightSteps;
        public double vx, vz;
    }
}
