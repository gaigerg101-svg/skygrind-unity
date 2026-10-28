// NIGHT8 U2-10: the SPITTER (skygrind/src/enemies/spitter.js on enemyKind.js, SURVIVAL.md §7, S6) and ai/aim.js.
// SPITTER: a ranged lobber (spitter.hp, the brain's `ranged` token). It holds the mid ring and backs off if you come
// within retreatRange. The tell: its back sac swells and glows for tellTime; then it lobs 3 arcing acid globs in a spread
// (spread m apart), ballistic and leading your velocity (globDamage on a hit). Every glob leaves a puddleRadius acid
// puddle for puddleTime (puddleDps, ticking every puddleTick; overlapping puddles don't stack). While you are hunted
// (camping) it aims at the spot you stand on. A deliberate miss (the brain's hit roll, ai/aim.js) lands beside you.
// Globs and puddles are pooled (spitter.globPool / puddlePool), stepped on the fixed step after the spitters.
// Unity-only (NIGHT8 D-B, the Demo Day goal "parryable globs"): a punch reflects a glob in reach, as the web's punch
// reflects a drone orb (punch.js tryReflect + drone.js reflect): it homes on the nearest enemy at the orb's
// reflectSpeedMult x globSpeed for reflectDamageMult x globDamage, and its hit is weapon "orb" (style RETURN TO SENDER).
// The numbers are the web's config.orb reflect numbers; the web's globs can't be reflected (logged in PORT-STATUS).
// Randomness: the miss side is Math.random() on the web; here the run's seeded stream (Husks.NewRun).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>ai/aim.js (pure): accuracy vs speed, the projectile lead, the deliberate miss.</summary>
    public static class AimMath
    {
        public struct A
        {
            public double baseHit, minHit, fullSpeed, maxLead, missBy;

            public static A From(GameConfig c) => new A
            {
                baseHit = c.Num("ai.aim.baseHit"), minHit = c.Num("ai.aim.minHit"), fullSpeed = c.Num("ai.aim.fullSpeed"),
                maxLead = c.Num("ai.aim.maxLead"), missBy = c.Num("ai.aim.missBy"),
            };
        }

        /// <summary>Hit chance for a player moving at `speed` m/s: baseHit standing, falling linearly to minHit at fullSpeed.</summary>
        public static double HitChance(double speed, A a)
        {
            double t = Math.Max(0, Math.Min(1, speed / a.fullSpeed));
            return a.baseHit + (a.minHit - a.baseHit) * t;
        }

        /// <summary>leadPoint: a first-order intercept capped at maxLead s (vertical lead halved: jumps come back down); returns the lead time.</summary>
        public static double LeadPoint(Vec3 from, Vec3 to, Vec3 vel, double projectileSpeed, double maxLead, out Vec3 output)
        {
            double d = JsMath.Hypot(to.x - from.x, to.y - from.y, to.z - from.z);
            double t = Math.Min(maxLead, d / Math.Max(1e-3, projectileSpeed));
            output = new Vec3(to.x + vel.x * t, to.y + vel.y * t * 0.5, to.z + vel.z * t);
            return t;
        }

        /// <summary>missPoint: `point` pushed sideways (horizontal, perpendicular to the shot) by missBy m, on a side picked by r (0..1).</summary>
        public static Vec3 MissPoint(Vec3 from, Vec3 point, double missBy, double r)
        {
            double dx = point.x - from.x;
            double dz = point.z - from.z;
            double len = JsMath.Hypot(dx, dz);
            if (len == 0 || double.IsNaN(len)) len = 1; // Math.hypot(dx, dz) || 1
            double side = r < 0.5 ? -1 : 1;
            point.x += (-dz / len) * missBy * side;
            point.z += (dx / len) * missBy * side;
            point.y += (r - 0.5) * missBy * 0.6;
            return point;
        }
    }

    /// <summary>config.spitter's attack numbers.</summary>
    public sealed class SpitterParams
    {
        public double minRange, maxRange, retreatRange, globTimeMin, globTimeMax, tellTime, cooldown, globSpeed, spread, globRadius, globDamage;
        public double puddleRadius, puddleTime, puddleDps, puddleTick, height, speed;
        public int globPool, puddlePool;

        public static SpitterParams From(GameConfig c) => new SpitterParams
        {
            minRange = c.Num("spitter.minRange"), maxRange = c.Num("spitter.maxRange"), retreatRange = c.Num("spitter.retreatRange"),
            globTimeMin = c.Num("spitter.globTimeMin"), globTimeMax = c.Num("spitter.globTimeMax"), tellTime = c.Num("spitter.tellTime"),
            cooldown = c.Num("spitter.cooldown"), globSpeed = c.Num("spitter.globSpeed"), spread = c.Num("spitter.spread"),
            globRadius = c.Num("spitter.globRadius"), globDamage = c.Num("spitter.globDamage"), puddleRadius = c.Num("spitter.puddleRadius"),
            puddleTime = c.Num("spitter.puddleTime"), puddleDps = c.Num("spitter.puddleDps"), puddleTick = c.Num("spitter.puddleTick"),
            height = c.Num("spitter.height"), speed = c.Num("spitter.speed"), globPool = c.Int("spitter.globPool"), puddlePool = c.Int("spitter.puddlePool"),
        };
    }

    public sealed class Spitter : Husk
    {
        public string state = "idle"; // idle | tell | recover
        public int stateSteps, tellSteps;
        public double drawAge; // s alive (visual: the tell's pulse)
        readonly EventBus events;
        readonly SpitterAcid acid;
        readonly SpitterParams c;

        public Spitter(HuskParams h, EventBus events, SpitterAcid acid) : base(h)
        {
            this.events = events;
            this.acid = acid;
            c = acid.p;
        }

        public override bool HasHead => false; // spitter.js has no headshot part: a body capsule only

        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            headshot = false;
            return CombatMath.RayVerticalCapsule(o, d, position, h.height, h.radius);
        }

        public override bool Spawn(int newId, Vec3 pos, Vec3 target, ICollisionWorld world)
        {
            if (!base.Spawn(newId, pos, target, world)) return false;
            state = "idle";
            stateSteps = tellSteps = 0;
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
            else StepSpitter(tp, target, world, bus, dt, gravity);
        }

        /// <summary>spitter.js step: the tell, then the volley; else back off inside retreatRange or take the brain's slot.</summary>
        void StepSpitter(Vec3 tp, Health target, ICollisionWorld world, EventBus bus, double dt, double gravity)
        {
            double dx = tp.x - position.x;
            double dz = tp.z - position.z;
            double d = JsMath.Hypot(dx, dz);
            if (d > 0.01) yaw = JsMath.Atan2(-dx, -dz);
            if (state == "tell")
            {
                velocity.x = 0;
                velocity.z = 0;
                if (--stateSteps <= 0)
                {
                    acid.ThrowVolley(this, tp);
                    state = "idle";
                    committed = false;
                    cooldownSteps = MovementMath.StepsFor(c.cooldown, dt);
                    if (ai != null) bus.Emit("enemyAttackDone", id);
                }
            }
            else
            {
                // As the web: a stagger's 'recover' only ends the tell; it doesn't hold the next one (spitter.js never reads it).
                bool ready = cooldownSteps == 0 && d >= c.minRange && d <= c.maxRange && !target.Dead && knockSteps == 0;
                if (ready && (ai == null || (ai.attack && ai.los)))
                {
                    state = "tell";
                    stateSteps = tellSteps = MovementMath.StepsFor(c.tellTime, dt);
                    committed = true;
                    if (ai != null) bus.Emit("enemyCommit", id);
                    bus.Emit("spitterTell", id);
                }
                else if (knockSteps == 0)
                {
                    // Back off when you get close; otherwise the brain's ring slot.
                    if (d < c.retreatRange)
                    {
                        double len = d == 0 ? 1 : d;
                        velocity.x = (-dx / len) * c.speed;
                        velocity.z = (-dz / len) * c.speed;
                    }
                    else
                    {
                        velocity.x = ai != null ? ai.vx : 0;
                        velocity.z = ai != null ? ai.vz : 0;
                    }
                }
            }
            Move(world, dt, gravity);
        }
    }

    /// <summary>The spitters' globs and acid puddles (pooled), and the punch's reflect.</summary>
    public sealed partial class SpitterAcid
    {
        public sealed class Glob
        {
            public bool alive, reflected;
            public Vec3 position, previous, velocity;
            public Husk shooter;
            public Enemy target; // a reflected glob homes on it
            public double age, damage;
        }

        public sealed class Puddle
        {
            public bool alive;
            public Vec3 position;
            public double age;
            public Husk shooter;
        }

        public readonly SpitterParams p;
        public readonly Glob[] globs;
        public readonly Puddle[] puddles;
        readonly EventBus events;
        readonly AimMath.A aim;
        readonly double gravity, capsuleRadius, killPlaneY, reflectRange, reflectSpeedMult, reflectDamageMult;
        double[] acidTicks = new double[4]; // per target (players in join order; one target: [0])
        Players stepPlayers; // this step's targets (null: the one target below)
        Vec3 soloPos;
        Health soloHealth;
        public Mulberry32 rng = new Mulberry32(0x5917);
        public Func<Vec3> targetVelocity = () => default; // the player's velocity (the lead) with one target
        public Player leadTarget; // with Players: the target of the spitter being stepped (Husks.Step sets it), whose velocity it leads
        public Func<bool> hunted = () => false; // camping: aim at the spot you stand on
        public Func<Enemy, double, Vec3, bool> onReflectHit; // a reflected glob's damage (the player's loadout: weapon "orb")
        /// <summary>NIGHT36 P parry36 (SurvivalSim.Core36 sets it with the row on; null: off): a reflected glob ended on a body or a wall here.</summary>
        public Action<Vec3> onReflectEnd36;
        public int volleys, reflects; // counters (tests, debug)

        public SpitterAcid(GameConfig cfg, EventBus events, double killPlaneY)
        {
            p = SpitterParams.From(cfg);
            this.events = events;
            aim = AimMath.A.From(cfg);
            gravity = MovementMath.GravityFor(cfg.movement.jump.height, cfg.movement.jump.apexTime);
            capsuleRadius = cfg.movement.body.capsuleRadius;
            this.killPlaneY = killPlaneY;
            reflectRange = cfg.Num("orb.reflectRange");
            reflectSpeedMult = cfg.Num("orb.reflectSpeedMult");
            reflectDamageMult = cfg.Num("orb.reflectDamageMult");
            globs = new Glob[p.globPool];
            for (int i = 0; i < globs.Length; i++) globs[i] = new Glob();
            puddles = new Puddle[p.puddlePool];
            for (int i = 0; i < puddles.Length; i++) puddles[i] = new Puddle();
        }

        /// <summary>
        /// NIGHT13 U3-26.10 (the run snapshot, inside Husks' section): the stream, every glob and puddle slot (live ones
        /// can outlast a round's last kill: they are saved, their shooters and targets as registry indices), the acid
        /// tick clocks and the counters. Scratch (this step's targets) and hooks are not saved.
        /// </summary>
        public void SaveState(StateWriter w, EnemyRegistry registry)
        {
            w.Rng("rng", rng);
            w.I("volleys", volleys);
            w.I("reflects", reflects);
            w.Doubles("acidTicks", acidTicks);
            var flags = new List<bool>();
            var nums = new List<double>();
            var refs = new List<int>();
            foreach (Glob g in globs)
            {
                flags.Add(g.alive);
                flags.Add(g.reflected);
                nums.AddRange(new[] { g.position.x, g.position.y, g.position.z, g.previous.x, g.previous.y, g.previous.z, g.velocity.x, g.velocity.y, g.velocity.z, g.age, g.damage });
                refs.Add(StateIO.EnemyIndex(registry, g.shooter));
                refs.Add(StateIO.EnemyIndex(registry, g.target));
            }
            w.Bools("globFlags", flags);
            w.Doubles("globNums", nums);
            w.Ints("globRefs", refs);
            flags.Clear();
            nums.Clear();
            refs.Clear();
            foreach (Puddle pd in puddles)
            {
                flags.Add(pd.alive);
                nums.AddRange(new[] { pd.position.x, pd.position.y, pd.position.z, pd.age });
                refs.Add(StateIO.EnemyIndex(registry, pd.shooter));
            }
            w.Bools("puddleFlags", flags);
            w.Doubles("puddleNums", nums);
            w.Ints("puddleRefs", refs);
        }

        public void LoadState(StateReader r, EnemyRegistry registry)
        {
            List<bool> gf = r.Bools("globFlags"), pf = r.Bools("puddleFlags");
            List<double> gn = r.Doubles("globNums"), pn = r.Doubles("puddleNums"), ticks = r.Doubles("acidTicks");
            List<int> gr = r.Ints("globRefs"), pr = r.Ints("puddleRefs");
            if (gf.Count != globs.Length * 2 || gn.Count != globs.Length * 11 || gr.Count != globs.Length * 2) throw new StateException("snapshot: acid: the glob pool's size differs");
            if (pf.Count != puddles.Length || pn.Count != puddles.Length * 4 || pr.Count != puddles.Length) throw new StateException("snapshot: acid: the puddle pool's size differs");
            var shooters = new Husk[globs.Length + puddles.Length];
            var targets = new Enemy[globs.Length];
            for (int i = 0; i < globs.Length; i++)
            {
                shooters[i] = StateIO.EnemyAt(registry, gr[i * 2]) as Husk;
                targets[i] = StateIO.EnemyAt(registry, gr[i * 2 + 1]);
            }
            for (int i = 0; i < puddles.Length; i++) shooters[globs.Length + i] = StateIO.EnemyAt(registry, pr[i]) as Husk;
            rng = r.Rng("rng");
            volleys = r.Int("volleys");
            reflects = r.Int("reflects");
            acidTicks = ticks.ToArray();
            for (int i = 0; i < globs.Length; i++)
            {
                Glob g = globs[i];
                int o = i * 11;
                g.alive = gf[i * 2];
                g.reflected = gf[i * 2 + 1];
                g.position = new Vec3(gn[o], gn[o + 1], gn[o + 2]);
                g.previous = new Vec3(gn[o + 3], gn[o + 4], gn[o + 5]);
                g.velocity = new Vec3(gn[o + 6], gn[o + 7], gn[o + 8]);
                g.age = gn[o + 9];
                g.damage = gn[o + 10];
                g.shooter = shooters[i];
                g.target = targets[i];
            }
            for (int i = 0; i < puddles.Length; i++)
            {
                Puddle pd = puddles[i];
                int o = i * 4;
                pd.alive = pf[i];
                pd.position = new Vec3(pn[o], pn[o + 1], pn[o + 2]);
                pd.age = pn[o + 3];
                pd.shooter = shooters[globs.Length + i];
            }
        }

        /// <summary>A ballistic launch from `from` that lands on `to` after `time` s.</summary>
        Vec3 Lob(Vec3 from, Vec3 to, double time) => new Vec3((to.x - from.x) / time, (to.y - from.y) / time + 0.5 * gravity * time, (to.z - from.z) / time);

        /// <summary>spitter.js throwVolley: three globs in a spread at your lead point (or beside you on a miss).</summary>
        public void ThrowVolley(Spitter e, Vec3 tp)
        {
            var from = new Vec3(e.position.x, e.position.y + p.height * 0.85, e.position.z);
            double d = JsMath.Hypot(tp.x - from.x, tp.z - from.z);
            double time = Math.Max(p.globTimeMin, Math.Min(p.globTimeMax, d / p.globSpeed));
            bool isHunted = hunted();
            Vec3 vel = leadTarget != null ? leadTarget.Velocity : targetVelocity();
            // Lead your velocity (unless you camp: then the spot you stand on); a miss lands beside you on purpose.
            double lead = isHunted ? 0 : time;
            var at = new Vec3(tp.x + vel.x * lead, tp.y + 0.4, tp.z + vel.z * lead);
            if (!Press57Aim(e, ref at, ref time) && e.ai != null && !e.ai.hit) // NIGHT57 G press57: a marked volley to its spot, no miss (Press57.cs; off: false)
            {
                double len = JsMath.Hypot(at.x - from.x, at.z - from.z);
                if (len == 0) len = 1;
                double s = rng.Next() < 0.5 ? -1 : 1;
                at.x += (-(at.z - from.z) / len) * aim.missBy * s;
                at.z += ((at.x - from.x) / len) * aim.missBy * s; // as the web: the new x
            }
            var side = new Vec3(-(at.z - from.z), 0, at.x - from.x);
            double sl = side.Length;
            if (sl > 0) side = Vec3.DivideScalar(side, sl);
            for (int i = -1; i <= 1; i++)
            {
                Glob glob = null;
                foreach (Glob g in globs) if (!g.alive) { glob = g; break; }
                if (glob == null) break;
                glob.alive = true;
                glob.reflected = false;
                glob.target = null;
                glob.age = 0;
                glob.damage = p.globDamage;
                glob.shooter = e;
                glob.position = glob.previous = from;
                glob.velocity = Lob(from, at + side * (i * p.spread), time);
            }
            volleys++;
            events.Emit("spitterVolley", e.id);
        }

        void Splash(Glob glob, Vec3 point)
        {
            glob.alive = false;
            Puddle pd = null;
            foreach (Puddle q in puddles) if (!q.alive) { pd = q; break; }
            if (pd == null)
            {
                pd = puddles[0];
                foreach (Puddle q in puddles) if (q.age > pd.age) pd = q;
            }
            pd.alive = true;
            pd.age = 0;
            pd.shooter = glob.shooter;
            pd.position = point;
            events.Emit("acidPuddle", null);
        }

        static bool SphereHitsWorld(ICollisionWorld world, Vec3 c, double r) =>
            WorldCollider.CapsuleIntersect(world, new Capsule { start = c, end = c, radius = r }, out _, out _);

        /// <summary>The live enemy whose body capsule a sphere touches (a reflected glob's hit), or null.</summary>
        static Enemy HitEnemy(EnemyRegistry enemies, Vec3 c, double r)
        {
            for (int i = 0; i < enemies.all.Count; i++)
            {
                Enemy e = enemies.all[i];
                if (!e.alive) continue;
                double y = Math.Max(e.position.y + e.Radius, Math.Min(e.position.y + e.Height - e.Radius, c.y));
                if (JsMath.Hypot(c.x - e.position.x, c.y - y, c.z - e.position.z) <= r + e.Radius) return e;
            }
            return null;
        }

        /// <summary>One fixed step against one target: the globs (you, the level, a reflected one's target), then the acid's damage.</summary>
        public void Step(Vec3 tp, Health target, ICollisionWorld world, EnemyRegistry enemies, double dt)
        {
            stepPlayers = null;
            soloPos = tp;
            soloHealth = target;
            Run(world, enemies, dt);
            soloHealth = null;
        }

        /// <summary>
        /// One fixed step against every player (NIGHT9): a glob hits the first one it touches (join order); each player
        /// stands in the acid, and ticks its damage, on their own.
        /// </summary>
        public void Step(Players players, ICollisionWorld world, EnemyRegistry enemies, double dt)
        {
            stepPlayers = players;
            Run(world, enemies, dt);
            stepPlayers = null;
        }

        int TargetCount => stepPlayers != null ? stepPlayers.Count : 1;
        Vec3 TargetPos(int i) => stepPlayers != null ? stepPlayers[i].Feet : soloPos;
        Health TargetHealth(int i) => stepPlayers != null ? stepPlayers[i].health : soloHealth;

        void Run(ICollisionWorld world, EnemyRegistry enemies, double dt)
        {
            int n = TargetCount;
            if (acidTicks.Length < n) Array.Resize(ref acidTicks, n);
            foreach (Glob glob in globs)
            {
                if (!glob.alive) continue;
                glob.previous = glob.position;
                if (glob.reflected)
                {
                    // drone.js: a parried orb homes on its target so it always returns to sender (no gravity).
                    if (glob.target != null && glob.target.alive)
                    {
                        Vec3 to = glob.target.AimPoint - glob.position;
                        double tl = to.Length;
                        if (tl > 1e-6) glob.velocity = to * (p.globSpeed * reflectSpeedMult / tl);
                    }
                }
                else glob.velocity.y -= gravity * dt;
                glob.position = glob.position + glob.velocity * dt;
                glob.age += dt;
                if (glob.reflected)
                {
                    Enemy hit = HitEnemy(enemies, glob.position, p.globRadius);
                    if (hit != null)
                    {
                        onReflectHit?.Invoke(hit, glob.damage, glob.position);
                        glob.alive = false;
                        onReflectEnd36?.Invoke(glob.position); // NIGHT36 P parry36: the return bursts (null: off)
                        continue;
                    }
                    if (SphereHitsWorld(world, glob.position, p.globRadius))
                    {
                        glob.alive = false;
                        onReflectEnd36?.Invoke(glob.position); // NIGHT36 P parry36: a wall sets the return off too
                    }
                    else if (glob.age > 4 || glob.position.y < killPlaneY) glob.alive = false;
                    continue;
                }
                // You (a capsule) or the ground / a wall.
                bool hitYou = false;
                for (int i = 0; i < n && !hitYou; i++)
                {
                    Vec3 tp = TargetPos(i);
                    Health target = TargetHealth(i);
                    double dy = glob.position.y - Math.Max(tp.y + 0.3, Math.Min(tp.y + 1.5, glob.position.y));
                    if (JsMath.Hypot(glob.position.x - tp.x, dy, glob.position.z - tp.z) <= p.globRadius + capsuleRadius && !target.Dead)
                    {
                        // where it came from (the damage-direction arc)
                        var from = new Vec3(glob.position.x - glob.velocity.x * 0.2, glob.position.y, glob.position.z - glob.velocity.z * 0.2);
                        if (target.Damage(p.globDamage, "spitter", from) && glob.shooter != null) glob.shooter.dealtDamage = true;
                        Splash(glob, tp);
                        hitYou = true;
                    }
                }
                if (hitYou) continue;
                if (SphereHitsWorld(world, glob.position, p.globRadius) || glob.age > 4 || glob.position.y < killPlaneY) Splash(glob, glob.position);
            }
            // Acid: puddleDps while you stand in any puddle (overlapping puddles don't stack). The puddles age once; each
            // target finds the last live puddle it stands in (pool order, as the web's one pass).
            foreach (Puddle pd in puddles)
            {
                if (!pd.alive) continue;
                pd.age += dt;
                if (pd.age >= p.puddleTime) pd.alive = false;
            }
            for (int i = 0; i < n; i++)
            {
                Vec3 tp = TargetPos(i);
                Puddle inAcid = null;
                foreach (Puddle pd in puddles)
                {
                    if (pd.alive && JsMath.Hypot(tp.x - pd.position.x, tp.z - pd.position.z) <= p.puddleRadius && Math.Abs(tp.y - pd.position.y) < 1.2) inAcid = pd;
                }
                acidTicks[i] = inAcid != null ? acidTicks[i] + dt : 0;
                if (inAcid != null && acidTicks[i] >= p.puddleTick)
                {
                    acidTicks[i] -= p.puddleTick;
                    if (TargetHealth(i).Damage(p.puddleDps * p.puddleTick, "acid") && inAcid.shooter != null) inAcid.shooter.dealtDamage = true;
                }
            }
        }

        /// <summary>
        /// The punch's reflect (punch.js tryReflect): the nearest live glob within orb.reflectRange of `origin` flies back at
        /// the enemy nearest to it (homing), faster and harder. Returns the glob, or null when none is in reach.
        /// </summary>
        public Glob TryReflect(Vec3 origin, Vec3 dir, EnemyRegistry enemies)
        {
            Glob best = Parry43Glob(origin, dir); // NIGHT43 W1 parry43 round 2: the orb's catch rules for globs (Core/Parry43.cs; off: null)
            double bestD = best != null ? -1 : reflectRange; // (a parry43 catch: the 2 m point test below picks nothing over it)
            foreach (Glob g in globs)
            {
                if (!g.alive || g.reflected) continue;
                double d = Vec3.DistanceTo(g.position, origin);
                if (d <= bestD)
                {
                    best = g;
                    bestD = d;
                }
            }
            if (best == null) return TryReflectOrb(origin, dir, enemies);
            Enemy nearest = null;
            double nd = double.PositiveInfinity;
            foreach (Enemy e in enemies.all)
            {
                if (!e.alive) continue;
                double d = Vec3.DistanceTo(e.AimPoint, best.position);
                if (d < nd)
                {
                    nd = d;
                    nearest = e;
                }
            }
            best.velocity = dir * (p.globSpeed * reflectSpeedMult);
            best.damage = p.globDamage * reflectDamageMult;
            best.reflected = true;
            best.target = nearest;
            best.age = 0;
            reflects++;
            events.Emit("globReflect", best.shooter != null ? best.shooter.id : -1);
            return best;
        }

        /// <summary>
        /// NIGHT19 U3-33.7 (builder E): the drones' orbs (Husks sets it; null: none). punch.js tryReflect is the orbs' on the
        /// web; the port's punch calls this one method (Loadout's StepPunch), so with no glob in reach it reflects the
        /// nearest hostile orb (DroneOrbs.TryReflect) and hands back a stand-in glob at the orb (its position: the parry
        /// event's). No drone, no orb: null, as before.
        /// </summary>
        public DroneOrbs orbs;
        readonly Glob orbStandIn = new Glob();
        /// <summary>NIGHT36 P parry36: the drone orb the last stand-in stood for (null: none yet); nothing else reads it.</summary>
        public DroneOrbs.Orb lastOrb36;
        /// <summary>NIGHT36 P parry36: is this glob the drone orb's stand-in?</summary>
        public bool IsOrbStandIn(Glob g) => g == orbStandIn;

        /// <summary>NIGHT42 W1 parry42: the orb half of TryReflect alone (the orbs' own early buffer, Core/Parry42.cs; globs keep parry36's).</summary>
        public Glob TryReflectOrb42(Vec3 origin, Vec3 dir, EnemyRegistry enemies) => orbs != null && orbs.parry43 ? TryReflect(origin, dir, enemies) : TryReflectOrb(origin, dir, enemies); // NIGHT43 W1 parry43 round 2: the buffer catches globs too

        Glob TryReflectOrb(Vec3 origin, Vec3 dir, EnemyRegistry enemies)
        {
            DroneOrbs.Orb orb = orbs?.TryReflect(origin, dir, enemies);
            if (orb == null) return null;
            orbStandIn.position = orb.position;
            orbStandIn.reflected = true;
            orbStandIn.target = orb.target;
            lastOrb36 = orb; // NIGHT36 P parry36: the real orb behind the stand-in (read only by the parry36 path)
            events.Emit("orbReflect", orb.shooter != null ? orb.shooter.id : -1);
            return orbStandIn;
        }

        public void Clear()
        {
            foreach (Glob g in globs) g.alive = false;
            foreach (Puddle pd in puddles) pd.alive = false;
            Array.Clear(acidTicks, 0, acidTicks.Length);
        }

        /// <summary>NIGHT14 (port-only, the clean slate: Husks.ClearPools empties the registry): the slots drop their shooters
        /// and targets, as a fresh composition's slots hold none. Before, a dead glob still named the last run's spitter and
        /// the next run's snapshot refused it ("a reference to an enemy outside the registry").</summary>
        public void DropEnemies()
        {
            foreach (Glob g in globs)
            {
                g.shooter = null;
                g.target = null;
            }
            foreach (Puddle pd in puddles) pd.shooter = null;
        }
    }
}
