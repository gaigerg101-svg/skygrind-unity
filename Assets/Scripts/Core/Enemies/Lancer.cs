// NIGHT20 U3-34.9 C2 (builder E): port of skygrind/src/enemies/lancer.js (createLancers), behind design.adds.lancer (the
// director deals it only with the row on: Director.Spawnable(kind, n19, lancer)). lancer.js's header:
// LANCER (SURVIVAL.md §7, S6): a sniper (70 HP, ranged token). It perches on high points in open zones
// (ledges, bridges, islands) and never walks: it blinks between perches. With a token and line of sight
// its laser sight tracks you WITH LAG (a first-order follow, so fast movement escapes it) for 1.2 s, then
// it fires a hitscan beam along the laser for 25; cover between you at that moment blocks it. It relocates
// after every shot (a short blink). While you are hunted (camping) its laser follows twice as fast.
// The port: a body in the Husks' pools (its own, lancer.maxCount; stepped after the chargers: main.js's createEnemies
// order), so the registry, the director and every weapon see it (enemyKind.js's facade: a vertical capsule of
// lancer.radius x lancer.height, no head, aimed at lancer.aimHeight). Its perches are main.js's getPerches (the director's
// high spawn nodes: Husks.lancerPerches), isHunted the director's camp score at 100 % (Husks.lancerHunted). The web's
// Math.random (the perch pick, the laser's start off you) is a Mulberry32 from the run seed ^ design.adds.lancer.salt
// (Husks.NewRun). The brain: the web's lancer always has one (brain.js k.stationary: the ranged token, line of sight, no
// steering); the port's HuskBrain has no stationary kind yet, so the director doesn't attach a lancer (Director.SpawnGroup)
// and the lancer runs lancer.js's `!ai` path with its own line of sight in place of the brain's (a ray from its eye to your
// chest every ai.losMaxAge s, the brain's own age): with a brain (ai set) it reads ai.attack and ai.los as the web does.
// enemyKind.js's step wrapper is kept (the cooldown, knock and stagger clocks before the kind's step); its stagger / stun
// (a committed lancer drops its aim) as the generic kind's; it never moves, so a knockback or a launch changes nothing.
// Events keep the web's names: lancerAim {id}, lancerFire {id, hit, offset}, lancerBlink {id, blind}; enemyCommit /
// enemyAttackDone (the id) only with a brain, as the web's. The view reads State, AimTo, AimProgress, Muzzle, BeamTo and
// Shots (Game/EnemyViews.N20.cs: the body and the DANGER laser; V draws the beam's look from the same fields).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>config.lancer (and the numbers a lancer reads), read once.</summary>
    public sealed class LancerParams
    {
        public double hp, radius, height, aimHeight, aimTime, trackLag, damage, hitRadius, range, cooldown, blinkTime, relocateMin, blindBlink;
        public double hitFlashTime, losMaxAge, killPlaneY;
        public int perchTries, maxCount;
        /// <summary>The far perches of a relocation (nextPerch's `far`), reused (single-threaded sim).</summary>
        public readonly List<SpawnNode> scratch = new List<SpawnNode>(64);

        public static LancerParams From(GameConfig c, double killPlaneY) => new LancerParams
        {
            hp = c.Num("lancer.hp"), radius = c.Num("lancer.radius"), height = c.Num("lancer.height"), aimHeight = c.Num("lancer.aimHeight"),
            aimTime = c.Num("lancer.aimTime"), trackLag = c.Num("lancer.trackLag"), damage = c.Num("lancer.damage"), hitRadius = c.Num("lancer.hitRadius"),
            range = c.Num("lancer.range"), cooldown = c.Num("lancer.cooldown"), blinkTime = c.Num("lancer.blinkTime"), relocateMin = c.Num("lancer.relocateMin"),
            blindBlink = c.Num("lancer.blindBlink"), perchTries = c.Int("lancer.perchTries"), maxCount = c.Int("lancer.maxCount"),
            hitFlashTime = c.Has("lancer.hitFlashTime") ? c.Num("lancer.hitFlashTime") : 0.08, // enemyKind.js: c.hitFlashTime ?? 0.08
            losMaxAge = c.Num("ai.losMaxAge"), killPlaneY = killPlaneY,
        };

        /// <summary>The body the pool and the weapons read (enemyKind.js: lift 0.05, the kind's aimHeight).</summary>
        public HuskParams Body() => new HuskParams
        {
            kind = "lancer", hp = hp, radius = radius, height = height, aimHeight = aimHeight, spawnLift = 0.05, hitFlashTime = hitFlashTime,
            killPlaneY = killPlaneY,
        };
    }

    /// <summary>lancerAim {id} / lancerFire {id, hit, offset} / lancerBlink {id, blind} (one reused payload per lancer).</summary>
    public sealed class LancerEvent
    {
        public int id;
        public bool hit, blind;
        public double offset;
    }

    public sealed class Lancer : Husk
    {
        public enum State { Idle, Aim, Blink, Recover }

        public State state;
        public int stateSteps;
        /// <summary>The laser's end while aiming (lancer.js e.data.aim): it follows your chest with trackLag.</summary>
        public Vec3 AimTo;
        /// <summary>The last beam's end (e.data.beamTo) and how many it has fired (the view flashes a beam when it changes).</summary>
        public Vec3 BeamTo;
        public int Shots;
        public bool LastHit;
        /// <summary>The target's mover this step (its crouch lowers the chest); null: standing.</summary>
        public Mover targetMover;
        double blind;
        bool los;
        int losTick, aimSteps;
        bool hasTo;
        SpawnNode to;
        readonly LancerParams c;
        readonly EventBus events;
        readonly Func<double> random;
        readonly Func<List<SpawnNode>> perches;
        readonly Func<bool> hunted;
        readonly LancerEvent said = new LancerEvent();

        public Lancer(LancerParams c, EventBus events, Func<double> random, Func<List<SpawnNode>> perches, Func<bool> hunted) : base(c.Body())
        {
            this.c = c;
            this.events = events;
            this.random = random;
            this.perches = perches;
            this.hunted = hunted;
        }

        public override bool HasHead => false;

        /// <summary>The aim's progress, 0 .. 1 (the laser thickens; lancer.js render's r), 0 when not aiming.</summary>
        public double AimProgress => state == State.Aim && aimSteps > 0 ? 1 - (double)stateSteps / aimSteps : 0;

        /// <summary>lancer.js muzzleOf: 0.7 m ahead of its axis at 0.8 x its height.</summary>
        public Vec3 Muzzle => new Vec3(position.x - JsMath.Sin(yaw) * 0.7, position.y + c.height * 0.8, position.z - JsMath.Cos(yaw) * 0.7);

        /// <summary>enemyKind.js raycast: a vertical capsule, no head.</summary>
        public override double RayHit(Vec3 o, Vec3 dir, out bool headshot)
        {
            headshot = false;
            return CombatMath.RayVerticalCapsule(o, dir, position, c.height, c.radius);
        }

        public override bool Spawn(int newId, Vec3 pos, Vec3 target, ICollisionWorld world)
        {
            if (!base.Spawn(newId, pos, target, world)) return false;
            state = State.Idle;
            stateSteps = aimSteps = 0;
            blind = 0;
            los = false;
            losTick = 0;
            hasTo = false;
            Shots = 0;
            LastHit = false;
            AimTo = BeamTo = position;
            grounded = true; // perched (no gravity: lancer.js zeroes its velocity every step)
            return true;
        }

        /// <summary>enemyKind.js stagger: a committed lancer drops its aim (state recover).</summary>
        public override void Stagger(double time, double dt)
        {
            if (!committed) return;
            state = State.Recover;
            stateSteps = MovementMath.StepsFor(time, dt);
            committed = false;
            if (ai != null) events.Emit("enemyAttackDone", id);
        }

        /// <summary>enemyKind.js stun: held for `time` s; a committed lancer drops its aim.</summary>
        public override void Stun(double time, double dt)
        {
            staggerSteps = Math.Max(staggerSteps, MovementMath.StepsFor(time, dt));
            if (committed) Stagger(time, dt);
        }

        /// <summary>enemyKind.js step (the clocks), then lancer.js step.</summary>
        public override void Tick(NavJumper jumper, Vec3 tp, Health target, IReadOnlyList<Enemy> pool, ICollisionWorld world, EventBus bus, double dt, double gravity)
        {
            if (cooldownSteps > 0) cooldownSteps--;
            if (knockSteps > 0) knockSteps--;
            if (staggerSteps > 0)
            {
                staggerSteps--;
                if (knockSteps == 0) knockSteps = 1;
            }
            StepLancer(tp, target, world, dt);
        }

        bool Sees(ICollisionWorld world, Vec3 from, Vec3 to, double maxLen)
        {
            Vec3 dir = to - from;
            double len = dir.Length;
            if (len > maxLen) return false;
            dir = Vec3.DivideScalar(dir, len > 0 ? len : 1);
            return !world.RayIntersect(from, dir, out RayHit hit) || hit.distance >= len - 0.5;
        }

        /// <summary>lancer.js nextPerch: a perch at least relocateMin m from here that can see you (perchTries rolls); else any
        /// far one; none at all: it stays.</summary>
        bool NextPerch(ICollisionWorld world, Vec3 chest, out SpawnNode perch)
        {
            perch = default;
            List<SpawnNode> all = perches();
            if (all == null || all.Count == 0) return false;
            List<SpawnNode> far = c.scratch;
            far.Clear();
            for (int i = 0; i < all.Count; i++)
                if (JsMath.Hypot(all[i].x - position.x, all[i].z - position.z) >= c.relocateMin) far.Add(all[i]);
            List<SpawnNode> pick = far.Count > 0 ? far : all;
            for (int i = 0; i < c.perchTries; i++)
            {
                SpawnNode p = pick[(int)Math.Floor(random() * pick.Count)];
                if (Sees(world, new Vec3(p.x, p.y + c.height * 0.9, p.z), chest, c.range))
                {
                    perch = p;
                    return true;
                }
            }
            perch = pick[(int)Math.Floor(random() * pick.Count)];
            return true;
        }

        void Blink(ICollisionWorld world, Vec3 chest, bool blindly, double dt)
        {
            state = State.Blink;
            stateSteps = MovementMath.StepsFor(c.blinkTime, dt);
            hasTo = NextPerch(world, chest, out to);
            said.id = id;
            said.blind = blindly;
            said.hit = false;
            said.offset = 0;
            events.Emit("lancerBlink", said);
        }

        void StepLancer(Vec3 tp, Health target, ICollisionWorld world, double dt)
        {
            var chest = new Vec3(tp.x, tp.y + (targetMover != null && targetMover.crouched ? 0.6 : 1.2), tp.z);
            // Perch at once on spawn (it spawns on a high node) and stay put (no walking, no gravity drift).
            velocity = default;
            if (state == State.Blink)
            {
                if (--stateSteps <= 0)
                {
                    if (hasTo)
                    {
                        position = new Vec3(to.x, to.y + 0.05, to.z);
                        previous = position;
                    }
                    state = State.Idle;
                    cooldownSteps = MovementMath.StepsFor(c.cooldown, dt);
                }
                return;
            }
            double dx = chest.x - position.x;
            double dz = chest.z - position.z;
            yaw = JsMath.Atan2(-dx, -dz);
            if (state == State.Aim)
            {
                // The laser follows you with lag (faster while you camp): outrun it and the shot misses.
                double lag = c.trackLag * (hunted() ? 0.5 : 1);
                double k = 1 - JsMath.Exp(-dt / lag); // fdlibm (DeterminismTests: no System.Math transcendental in Core)
                AimTo = new Vec3(AimTo.x + (chest.x - AimTo.x) * k, AimTo.y + (chest.y - AimTo.y) * k, AimTo.z + (chest.z - AimTo.z) * k);
                if (--stateSteps <= 0) Fire(target, world, chest, dt);
                return;
            }
            double d = JsMath.Hypot(dx, dz);
            // A perch that can't see you (or is out of range) for a while is a dead end: blink to one that can (POWER P1).
            if (--losTick <= 0)
            {
                los = Sees(world, new Vec3(position.x, position.y + c.height * 0.9, position.z), chest, double.PositiveInfinity);
                losTick = MovementMath.StepsFor(c.losMaxAge, dt);
            }
            bool seeing = ai != null ? ai.los : los;
            blind = !seeing || d > c.range ? blind + dt : 0;
            if (blind >= c.blindBlink && cooldownSteps == 0)
            {
                blind = 0;
                Blink(world, chest, true, dt);
                return;
            }
            if (cooldownSteps == 0 && d <= c.range && !target.Dead && (ai != null ? ai.attack && ai.los : los))
            {
                state = State.Aim;
                stateSteps = aimSteps = MovementMath.StepsFor(c.aimTime, dt);
                AimTo = new Vec3(chest.x + (random() - 0.5) * 4, chest.y + 1.5, chest.z + (random() - 0.5) * 4); // starts off you
                committed = true;
                if (ai != null) events.Emit("enemyCommit", id);
                said.id = id;
                said.hit = said.blind = false;
                said.offset = 0;
                events.Emit("lancerAim", said);
            }
        }

        /// <summary>The beam: the first wall along the laser, and whether it passes through you before it.</summary>
        void Fire(Health target, ICollisionWorld world, Vec3 chest, double dt)
        {
            Vec3 muzzle = Muzzle;
            Vec3 dir = AimTo - muzzle;
            double len = dir.Length;
            dir = Vec3.DivideScalar(dir, len > 0 ? len : 1);
            double reach = world.RayIntersect(muzzle, dir, out RayHit wall) ? wall.distance : c.range;
            Vec3 rel = chest - muzzle;
            double along = Vec3.Dot(rel, dir);
            double off = (rel - dir * along).Length;
            bool hit = along > 0 && along < reach && off <= c.hitRadius && !target.Dead;
            if (hit && target.Damage(c.damage, "lancer", position)) dealtDamage = true;
            BeamTo = muzzle + dir * Math.Min(reach, c.range);
            Shots++;
            LastHit = hit;
            said.id = id;
            said.hit = hit;
            said.blind = false;
            said.offset = JsMath.Round(off * 100) / 100; // lancer.js: Math.round(off * 100) / 100
            events.Emit("lancerFire", said);
            committed = false;
            if (ai != null) events.Emit("enemyAttackDone", id);
            // Relocate after every shot.
            Blink(world, chest, false, dt);
        }
    }
}
