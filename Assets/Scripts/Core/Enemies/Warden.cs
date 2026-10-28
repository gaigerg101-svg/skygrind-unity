// NIGHT20 U3-34.9 C4 (builder E): port of skygrind/src/enemies/warden.js on the enemyKind.js base (as Charger.cs is),
// behind design.adds.warden (the director deals it only with the row on: Director.Spawnable(kind, n19, lancer, warden)).
// warden.js's header:
// WARDEN (SURVIVAL.md §7, S6): the shield (200 HP, heavy token). A front energy shield absorbs every hit
// from the front 120 deg. Break it by flanking (hits from outside the arc), from above, with rocket splash,
// with ricochet bounces (they hit from any angle) or by punching the shield: that stuns the Warden for 1 s
// with the shield open. It walks steadily at you, shoving other enemies aside, and swings for 20 after a
// heavy 0.8 s tell.
// The port: config.warden's numbers read once (WardenParams); the body physics are the Husk's (enemyKind.js groundMove:
// Husk.Move); the shield is warden.js damageMult (0: absorbed) read by EnemyHits.Filter (Core/Enemies/Affixes.cs: the
// web's enemies.damage, a blocked hit emits `enemyBlocked` and lands nothing: no hit, points, style or camp relief); a
// punch on the shield is warden.js's knockback override. Its brain: HuskBrain drives it as ai.kinds.warden (BrainKinds:
// the heavy token, waitRing 1, 3 m/s); the walk is still clamped to warden.speed (the same 3 m/s: a no-op). Events keep the
// web's names: wardenTell {id}, wardenSwing {id, distance}, wardenShieldBreak {id} (WardenEvent), enemyBlocked {id, kind,
// weapon} (EnemyBlockedEvent), and the brain's enemyCommit / enemyAttackDone (the id, as the port's husks).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>config.warden's numbers.</summary>
    public sealed class WardenParams
    {
        public double speed, knockbackScale, shieldArcDeg, aboveHeight, shieldStun, swingStart, swingRange, damage, tellTime, recoverTime, cooldown;
        public double shoveRadius, shoveSpeed, height;
        /// <summary>NIGHT40 E3 foes40 (the sim sets it at a run's start; false with the row off = the web's rule): only a melee knock (the punch, the
        /// gauntlet's swing: Loadout.MeleeKnock40) opens the shield. The web's override opened it for ANY knock from the front, so the shotgun's
        /// momentum shock (Impact.WaveHit) broke it too (n40foes ttk: the shotgun killed a WARDEN in 3.9 s, faster than the gauntlet's 5.0 s).</summary>
        public bool meleeShield40;
        /// <summary>NIGHT40 E3 foes40: x a front hit the shield softens (EnemyHits.Filter; 1 with the row off).</summary>
        public double frontSoft40 = 1;

        public static WardenParams From(GameConfig c) => new WardenParams
        {
            speed = c.Num("warden.speed"), knockbackScale = c.Num("warden.knockbackScale"), shieldArcDeg = c.Num("warden.shieldArcDeg"),
            aboveHeight = c.Num("warden.aboveHeight"), shieldStun = c.Num("warden.shieldStun"), swingStart = c.Num("warden.swingStart"),
            swingRange = c.Num("warden.swingRange"), damage = c.Num("warden.damage"), tellTime = c.Num("warden.tellTime"),
            recoverTime = c.Num("warden.recoverTime"), cooldown = c.Num("warden.cooldown"), shoveRadius = c.Num("warden.shoveRadius"),
            shoveSpeed = c.Num("warden.shoveSpeed"), height = c.Num("warden.height"),
        };

        /// <summary>config.warden through enemyKind.js (no head; lift 0.05; aim at height x 0.55; flash hitFlashTime ?? 0.08).</summary>
        public static HuskParams Body(GameConfig c, double killPlaneY) => new HuskParams
        {
            kind = "warden", hp = c.Num("warden.hp"), speed = c.Num("warden.speed"), radius = c.Num("warden.radius"), height = c.Num("warden.height"),
            aimHeight = c.Has("warden.aimHeight") ? c.Num("warden.aimHeight") : c.Num("warden.height") * 0.55, spawnLift = 0.05,
            hitFlashTime = c.Has("warden.hitFlashTime") ? c.Num("warden.hitFlashTime") : 0.08,
            edgeNormalY = c.Num("ai.edgeNormalY"), edgeRiseSpeed = c.Num("ai.edgeRiseSpeed"), supportedDrop = c.Num("ai.supportedDrop"),
            floorMinNormalY = c.movement.surface.floorMinNormalY, wallMaxAbsNormalY = c.movement.surface.wallMaxAbsNormalY, killPlaneY = killPlaneY,
        };
    }

    /// <summary>wardenTell / wardenSwing {id, distance} / wardenShieldBreak (one reused payload per warden).</summary>
    public sealed class WardenEvent
    {
        public int id;
        public double distance;
    }

    /// <summary>enemyKind.js's enemyBlocked {id, kind, weapon}: a hit a shield absorbed (one reused payload).</summary>
    public sealed class EnemyBlockedEvent
    {
        public int id;
        public string kind, weapon;
    }

    public sealed class Warden : Husk
    {
        public string state = "idle"; // idle | tell | swing | stunned
        public int stateSteps;
        /// <summary>e.data.shieldDown / blockFlash (s): the shield open after a punch; the block's flash (the view).</summary>
        public double shieldDown, blockFlash;
        /// <summary>NIGHT40 E3: true only inside a melee knock (Loadout.MeleeKnock40 sets it around the call).</summary>
        public bool meleeKnock40;
        public double drawAge; // s alive (visual: the tell's pulse, the shield's shimmer)
        readonly WardenParams c;
        readonly EventBus events;
        readonly WardenEvent said = new WardenEvent();
        static readonly EnemyBlockedEvent blockedSaid = new EnemyBlockedEvent();

        public Warden(HuskParams h, WardenParams c, EventBus events) : base(h)
        {
            this.c = c;
            this.events = events;
        }

        public WardenParams Attack => c;
        public bool ShieldDown => shieldDown > 0;
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
            shieldDown = blockFlash = drawAge = 0;
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

        /// <summary>warden.js shielded: a hit from inside the front arc and not from above, while the shield is up; a rocket's
        /// splash, a ricochet and a slam's shockwave go around it. True: absorbed (`enemyBlocked`).</summary>
        public bool Absorbs(Vec3 from, string weapon)
        {
            if (shieldDown > 0) return false;
            if (weapon == "rocket" || weapon == "ricochet" || weapon == "slam") return false; // splash, ricochet bounces, shockwaves
            double dx = from.x - position.x, dz = from.z - position.z, dy = from.y - (position.y + c.height);
            if (dy > c.aboveHeight) return false; // from above
            double len = JsMath.Hypot(dx, dz);
            if (len == 0) len = 1;
            double facing = (dx * -JsMath.Sin(yaw) + dz * -JsMath.Cos(yaw)) / len;
            if (facing < JsMath.Cos(c.shieldArcDeg / 2 * (Math.PI / 180))) return false;
            blockFlash = 0.12;
            blockedSaid.id = id;
            blockedSaid.kind = kind;
            blockedSaid.weapon = weapon;
            events.Emit("enemyBlocked", blockedSaid);
            return true;
        }

        /// <summary>warden.js knockback: a punch on the shield (from the front) stuns it for shieldStun with the shield open;
        /// anything else x knockbackScale.</summary>
        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt)
        {
            double len = JsMath.Hypot(dirX, dirZ);
            if (len == 0) len = 1;
            double facing = (-dirX * -JsMath.Sin(yaw) + -dirZ * -JsMath.Cos(yaw)) / len;
            if (facing > 0.3 && shieldDown <= 0 && (!c.meleeShield40 || meleeKnock40)) // NIGHT40 E3 foes40: a melee knock only
            {
                shieldDown = c.shieldStun;
                state = "stunned";
                stateSteps = MovementMath.StepsFor(c.shieldStun, dt);
                if (committed)
                {
                    committed = false;
                    if (ai != null) events.Emit("enemyAttackDone", id);
                }
                said.id = id;
                said.distance = 0;
                events.Emit("wardenShieldBreak", said);
                return;
            }
            base.Knockback(dirX, dirZ, speed * c.knockbackScale, time, dt);
        }

        /// <summary>enemyKind.js launch: up x knockbackScale.</summary>
        public override void Launch(double up) => base.Launch(up * c.knockbackScale);

        /// <summary>warden.js onStagger: nothing (a punch's stagger doesn't cut its swing; the shield punch is the knockback).</summary>
        public override void Stagger(double time, double dt)
        {
        }

        /// <summary>warden.js onStun (the Thumper stuns even a Warden): its attack dropped, stunned for `time` s.</summary>
        public override void Stun(double time, double dt)
        {
            if (committed && ai != null) events.Emit("enemyAttackDone", id);
            committed = false;
            int steps = Math.Max(1, (int)JsMath.Round(time / dt));
            stateSteps = state == "stunned" ? Math.Max(stateSteps, steps) : steps;
            state = "stunned";
        }

        /// <summary>enemyKind.js step: the timers, then a nav jump or its own step.</summary>
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
            drawAge += dt;
            bool busy = committed || (state != "idle" && state != "recover") || knockSteps > 0;
            if (jumper != null && jumper.Step(this, dt, busy)) Move(world, dt, gravity);
            else StepWarden(tp, target, pool, world, dt, gravity);
        }

        void StepWarden(Vec3 tp, Health target, IReadOnlyList<Enemy> pool, ICollisionWorld world, double dt, double gravity)
        {
            double dx = tp.x - position.x, dz = tp.z - position.z;
            double d = JsMath.Hypot(dx, dz);
            if (shieldDown > 0) shieldDown -= dt;
            if (blockFlash > 0) blockFlash -= dt;
            knockSteps = 0; // too heavy to knock back
            if (state == "stunned")
            {
                velocity.x = 0;
                velocity.z = 0;
                if (--stateSteps <= 0) state = "idle";
            }
            else if (state == "tell")
            {
                velocity.x = 0;
                velocity.z = 0;
                if (--stateSteps <= 0)
                {
                    if (d <= c.swingRange && !target.Dead && target.Damage(c.damage, "warden", position)) dealtDamage = true;
                    state = "swing";
                    stateSteps = MovementMath.StepsFor(c.recoverTime, dt);
                    said.id = id;
                    said.distance = d;
                    events.Emit("wardenSwing", said);
                }
            }
            else if (state == "swing")
            {
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
                if (cooldownSteps == 0 && d <= c.swingStart && !target.Dead && (ai == null || ai.attack))
                {
                    state = "tell";
                    stateSteps = MovementMath.StepsFor(c.tellTime, dt);
                    committed = true;
                    if (ai != null) events.Emit("enemyCommit", id);
                    said.id = id;
                    said.distance = d;
                    events.Emit("wardenTell", said);
                }
                else
                {
                    // A steady walk at you (with a brain: its route, around obstacles), shoving others aside.
                    double dd = d > 0 ? d : 1;
                    double vx = ai != null ? ai.vx : dx / dd * c.speed;
                    double vz = ai != null ? ai.vz : dz / dd * c.speed;
                    double v = JsMath.Hypot(vx, vz);
                    if (v > c.speed) // the port: the husk's brain steers it at the husk's pace; warden.speed caps it
                    {
                        vx = vx / v * c.speed;
                        vz = vz / v * c.speed;
                    }
                    velocity.x = d > c.swingStart * 0.8 ? vx : 0;
                    velocity.z = d > c.swingStart * 0.8 ? vz : 0;
                    for (int i = 0; i < pool.Count; i++) // indexed: no boxed enumerator (U2-02)
                    {
                        Enemy o = pool[i];
                        if (o == this || !o.alive || o.kind == "warden" || o.kind == "drone" || o.kind == "lancer") continue;
                        double ox = o.position.x - position.x, oz = o.position.z - position.z;
                        double od = JsMath.Hypot(ox, oz);
                        if (od > 0 && od < c.shoveRadius) o.position = new Vec3(o.position.x + ox / od * c.shoveSpeed * dt, o.position.y, o.position.z + oz / od * c.shoveSpeed * dt);
                    }
                }
            }
            Move(world, dt, gravity);
        }
    }
}
