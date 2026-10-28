// Projectiles, ported line for line from skygrind/src/combat/projectiles.js (SURVIVAL.md §4, S3; POWER P4): nails,
// rockets, Bouncer grenades, Discus saws and Vortex orbs, pooled and owner-agnostic. Fixed step: each projectile sweeps
// its segment against the world collider (gates included), Drone orbs and the enemies facade; the first thing hit wins.
// - Nails stick in the enemy they hit (visual, until it dies or after stickTime).
// - Rockets explode on contact (or at the end of their life): splash damage with falloff and a knockback on their
//   owner (the rocket jump, 0 self damage).
// OVERNIGHT2 N2 specials ported here: stuck nails detonated at once, piercing nails, steered rockets and their
// fragments, and every blast's radius grown by the owner's momentum when it was fired (radiusMult).
//
// NIGHT10 U3-13.3. What this port covers, and what it stubs:
// - Ported: the shared core (take, stick, sweep, steer, blast, explode, arm), fire('nail' | 'rocket'), detonate,
//   steerRockets, clear, count, stuckCount, step's nail / rocket / stuck / fragment loops.
// - Stubbed for N11 (the POWER guns): launch('grenade' | 'disc' | 'vortex'), volley (Rattler darts), airburst, recall,
//   vortexAlt, openVortexes, and step's stepGrenades / stepDiscs / stepVortexes / stepDarts (their pools are empty).
// - No Drone orbs yet (N13): sweep's orb branch and orbs.destroy are left out; nothing can hit an orb.
// - render() lives in the Game (ProjectileViews reads the pools); particles.burst calls go to the `burst` hook.
// - Math.random (arm's spin, blast's fragment angle and spread) is this module's own Mulberry32 stream, seeded per run
//   (CLAUDE.md rule 6); the p19 vectors seed V8's Math.random with the same mulberry32, so the draws line up.
// - A web bug ported as is: stick() is passed the step's scratch `tmp` as the hit point and then overwrites `tmp` with
//   the enemy's aim point, so every stuck nail's offset is (0, 0, 0): it sits at the aim point (projectiles.js:96-106,
//   called from :396). The vectors record the web's behaviour.
// Golden: vectors/projectiles.json (Core/Golden/GoldenProjectiles.cs).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>What projectiles.js needs from a loadout: its damage (hit / kill events), the trigger pull id, the arsenal's statuses.</summary>
    public interface IProjectileLoadout
    {
        /// <summary>loadout.shot: the trigger pull the next hits belong to.</summary>
        int Shot { get; }

        /// <summary>loadout.newShot(): a new trigger pull; returns its id.</summary>
        int NewShot();

        /// <summary>loadout.arsenal?.enabled.</summary>
        bool ArsenalEnabled { get; }

        /// <summary>loadout.arsenal.status(enemy, name).</summary>
        void Status(Enemy enemy, string name);

        /// <summary>loadout.damage(enemy, amount, headshot, weapon, from, assisted, pointScale, opts): true if it killed. `blast`: opts.blast.</summary>
        bool Damage(Enemy enemy, double amount, bool headshot, string weapon, Vec3 from, bool wasAssisted, double pointScale, int shot, bool blast);
    }

    /// <summary>projectiles.js's owner: {id, mover} (the rocket jump pushes its mover).</summary>
    public interface IProjectileOwner
    {
        string Id { get; }
        Vec3 Position { get; }
        void Impulse(double x, double y, double z);
    }

    /// <summary>Aim assist homing (POWER P2): {enemy, id, turn} (turn in rad/s).</summary>
    public struct HomingTarget
    {
        public Enemy enemy;
        public int id;
        public double turn;
    }

    /// <summary>rocketBlast / grenadeBlast / vortexBurst {owner, position, radius, hits} (reused: read it inside the handler).</summary>
    public sealed class BlastEvent
    {
        public string owner;
        public Vec3 position;
        public double radius;
        public int hits;
    }

    /// <summary>rocketJump / grenadeJump {owner, speed, weapon} (reused).</summary>
    public sealed class BlastJumpEvent
    {
        public string owner;
        public double speed;
        public string weapon;
    }

    /// <summary>nailDetonate {owner, nails} (reused).</summary>
    public sealed class DetonateEvent
    {
        public string owner;
        public int nails;
    }

    /// <summary>rocketFragment {owner, position, hits} (reused).</summary>
    public sealed class FragmentEvent
    {
        public string owner;
        public Vec3 position;
        public int hits;
    }

    public sealed class Projectiles : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the stream, the stuck ring's cursor, the scratch vectors that carry between
        // calls, and every live nail, rocket, stuck nail and pending fragment, in their pool slots (a break can have
        // shots in flight: they are saved, not dropped). Enemies (homing, pierced, stuck) are registry indices. The
        // owner (one loadout: solo) isn't a value: Load leaves it null and the loadout's Load hands itself over
        // (Adopt), in either order.
        IProjectileLoadout adoptLoadout;
        IProjectileOwner adoptOwner;

        void CheckOwner(IProjectileLoadout l, IProjectileOwner o, ref IProjectileLoadout seen)
        {
            if (l == null) return;
            if (seen == null) seen = l;
            else if (!ReferenceEquals(seen, l)) throw new StateException("snapshot: projectiles of two owners (a snapshot is solo)");
            if (!ReferenceEquals(l, o)) throw new StateException("snapshot: a projectile whose loadout is not its owner");
        }

        void SaveShots(StateWriter w, string key, Projectile[] list, ref IProjectileLoadout seen)
        {
            w.Begin(key);
            var slots = new List<int>();
            for (int i = 0; i < list.Length; i++) if (list[i].alive) slots.Add(i);
            w.Ints("slots", slots);
            foreach (int i in slots)
            {
                Projectile p = list[i];
                CheckOwner(p.loadout, p.owner, ref seen);
                w.Begin(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                w.V("position", p.position);
                w.V("previous", p.previous);
                w.V("velocity", p.velocity);
                w.V("dir", p.dir);
                w.I("ageSteps", p.ageSteps);
                w.B("owned", p.loadout != null);
                w.D("trail", p.trail);
                w.B("hasHoming", p.hasHoming);
                w.I("homingEnemy", StateIO.EnemyIndex(enemies, p.homing.enemy));
                w.I("homingId", p.homing.id);
                w.D("homingTurn", p.homing.turn);
                w.B("assisted", p.assisted);
                w.D("turned", p.turned);
                w.I("shot", p.shot);
                w.D("spin", p.spin);
                w.D("radiusMult", p.radiusMult);
                w.I("pierce", p.pierce);
                w.Ints("pierced", StateIO.EnemyIndices(enemies, p.pierced));
                w.I("fragments", p.fragments);
                w.B("hasSteer", p.hasSteer);
                w.V("steer", p.steer);
                w.End();
            }
            w.End();
        }

        void LoadShots(StateReader r, string key, Projectile[] list)
        {
            StateReader s = r.Section(key);
            List<int> slots = s.Ints("slots");
            var live = new bool[list.Length];
            foreach (int i in slots)
            {
                if (i < 0 || i >= list.Length || live[i]) throw new StateException($"snapshot: projectiles.{key} slot {i}");
                live[i] = true;
            }
            foreach (Projectile p in list) p.alive = false;
            foreach (int i in slots)
            {
                StateReader e = s.Section(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                Projectile p = list[i];
                p.alive = true;
                p.position = e.V("position");
                p.previous = e.V("previous");
                p.velocity = e.V("velocity");
                p.dir = e.V("dir");
                p.ageSteps = e.Int("ageSteps");
                p.ownedOnLoad = e.B("owned");
                p.loadout = p.ownedOnLoad ? adoptLoadout : null;
                p.owner = p.ownedOnLoad ? adoptOwner : null;
                p.trail = e.D("trail");
                p.hasHoming = e.B("hasHoming");
                p.homing = new HomingTarget { enemy = StateIO.EnemyAt(enemies, e.Int("homingEnemy")), id = e.Int("homingId"), turn = e.D("homingTurn") };
                p.assisted = e.B("assisted");
                p.turned = e.D("turned");
                p.shot = e.Int("shot");
                p.spin = e.D("spin");
                p.radiusMult = e.D("radiusMult");
                p.pierce = e.Int("pierce");
                p.pierced.Clear();
                foreach (int k in e.Ints("pierced")) p.pierced.Add(StateIO.EnemyAt(enemies, k));
                p.fragments = e.Int("fragments");
                p.hasSteer = e.B("hasSteer");
                p.steer = e.V("steer");
            }
        }

        /// <summary>NIGHT20 fix list G2: a grenade's (bounces, rest, spike) and an orb's (open, openSteps, repel, pullMult, pulled) fields, per live slot.</summary>
        void SaveExtras20(StateWriter w, string key, Projectile[] list)
        {
            w.Begin(key);
            for (int i = 0; i < list.Length; i++)
            {
                Projectile p = list[i];
                if (!p.alive) continue;
                w.Begin(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                w.Ints("steps", new[] { p.bounces, p.openSteps });
                w.Bools("flags", new[] { p.rest, p.spike, p.open, p.repel });
                w.D("pullMult", p.pullMult);
                var pulled = new List<int>(p.pulled.Count);
                foreach (Enemy e in p.pulled) pulled.Add(StateIO.EnemyIndex(enemies, e));
                w.Ints("pulled", pulled);
                w.End();
            }
            w.End();
        }

        void LoadExtras20(StateReader r, string key, Projectile[] list)
        {
            StateReader s = r.Section(key);
            for (int i = 0; i < list.Length; i++)
            {
                Projectile p = list[i];
                if (!p.alive) continue;
                StateReader e = s.Section(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                List<int> st = e.Ints("steps");
                List<bool> fl = e.Bools("flags");
                if (st.Count != 2 || fl.Count != 4) throw new StateException($"snapshot: projectiles.{key} slot {i}");
                p.bounces = st[0];
                p.openSteps = st[1];
                p.rest = fl[0];
                p.spike = fl[1];
                p.open = fl[2];
                p.repel = fl[3];
                p.pullMult = e.D("pullMult");
                p.pulled.Clear();
                foreach (int k in e.Ints("pulled")) p.pulled.Add(StateIO.EnemyAt(enemies, k));
            }
        }

        public void Save(StateWriter w)
        {
            IProjectileLoadout seen = null;
            w.Rng("rng", rng);
            w.I("stuckNext", stuckNext);
            w.V("tmp", tmp);
            w.V("seg", seg);
            w.V("blastPoint", blastPoint);
            w.V("p2", p2);
            SaveShots(w, "nails", nails, ref seen);
            SaveShots(w, "rockets", rockets, ref seen);
            // NIGHT20 fix list G2: the BOUNCER's grenades and the VORTEX's orbs, with their own fields
            SaveShots(w, "grenades", grenades, ref seen);
            SaveExtras20(w, "grenadesX", grenades);
            SaveShots(w, "vortexes", vortexes, ref seen);
            SaveExtras20(w, "vortexesX", vortexes);
            w.Ints("stats20", new[] { grenadeBounces, grenadeBlasts, vortexOpens, vortexBursts, vortexPulled });
            w.Begin("stuck");
            var slots = new List<int>();
            for (int i = 0; i < stuck.Length; i++) if (stuck[i].alive) slots.Add(i);
            w.Ints("slots", slots);
            foreach (int i in slots)
            {
                Stuck s = stuck[i];
                CheckOwner(s.loadout, s.loadout as IProjectileOwner, ref seen);
                w.Begin(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                w.I("enemy", StateIO.EnemyIndex(enemies, s.enemy));
                w.I("id", s.id);
                w.V("offset", s.offset);
                w.V("dir", s.dir);
                w.I("ageSteps", s.ageSteps);
                w.B("owned", s.loadout != null);
                w.End();
            }
            w.End();
            w.Begin("fragments");
            w.I("n", fragments.Count);
            for (int i = 0; i < fragments.Count; i++)
            {
                Fragment f = fragments[i];
                CheckOwner(f.loadout, f.owner, ref seen);
                w.Begin(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                w.B("owned", f.loadout != null);
                w.I("shot", f.shot);
                w.B("assisted", f.assisted);
                w.V("at", f.at);
                w.I("steps", f.steps);
                w.End();
            }
            w.End();
        }

        public void Load(StateReader r)
        {
            StateReader st = r.Section("stuck"), fr = r.Section("fragments");
            List<int> stuckSlots = st.Ints("slots");
            foreach (int i in stuckSlots) if (i < 0 || i >= stuck.Length) throw new StateException($"snapshot: projectiles.stuck slot {i}");
            LoadShots(r, "nails", nails);
            LoadShots(r, "rockets", rockets);
            if (r.Has("grenades")) // NIGHT20 fix list G2 (absent in an older save: none in flight)
            {
                List<int> s20 = r.Ints("stats20");
                if (s20.Count != 5) throw new StateException("snapshot: projectiles.stats20");
                LoadShots(r, "grenades", grenades);
                LoadExtras20(r, "grenadesX", grenades);
                LoadShots(r, "vortexes", vortexes);
                LoadExtras20(r, "vortexesX", vortexes);
                grenadeBounces = s20[0];
                grenadeBlasts = s20[1];
                vortexOpens = s20[2];
                vortexBursts = s20[3];
                vortexPulled = s20[4];
            }
            rng = r.Rng("rng");
            stuckNext = r.Int("stuckNext");
            if (stuckNext < 0 || stuckNext >= stuck.Length) throw new StateException($"snapshot: projectiles.stuckNext {stuckNext}");
            tmp = r.V("tmp");
            seg = r.V("seg");
            blastPoint = r.V("blastPoint");
            p2 = r.V("p2");
            foreach (Stuck s in stuck) s.alive = false;
            foreach (int i in stuckSlots)
            {
                StateReader e = st.Section(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                Stuck s = stuck[i];
                s.alive = true;
                s.enemy = StateIO.EnemyAt(enemies, e.Int("enemy"));
                s.id = e.Int("id");
                s.offset = e.V("offset");
                s.dir = e.V("dir");
                s.ageSteps = e.Int("ageSteps");
                s.ownedOnLoad = e.B("owned");
                s.loadout = s.ownedOnLoad ? adoptLoadout : null;
            }
            foreach (Fragment f in fragments) spare.Push(f);
            fragments.Clear();
            int n = fr.Int("n");
            for (int i = 0; i < n; i++)
            {
                StateReader e = fr.Section(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                Fragment f = spare.Count > 0 ? spare.Pop() : new Fragment();
                f.ownedOnLoad = e.B("owned");
                f.loadout = f.ownedOnLoad ? adoptLoadout : null;
                f.owner = f.ownedOnLoad ? adoptOwner : null;
                f.shot = e.Int("shot");
                f.assisted = e.B("assisted");
                f.at = e.V("at");
                f.steps = e.Int("steps");
                fragments.Add(f);
            }
        }

        /// <summary>
        /// NIGHT13 U3-26.10: the loadout's Load hands itself over: the owner of every restored live shot, stuck nail and
        /// fragment that had one (a snapshot is solo: one owner). Called in either order with Load.
        /// </summary>
        public void Adopt(IProjectileLoadout loadout, IProjectileOwner owner)
        {
            adoptLoadout = loadout;
            adoptOwner = owner;
            foreach (Projectile p in nails)
                if (p.alive && p.ownedOnLoad && p.loadout == null)
                {
                    p.loadout = loadout;
                    p.owner = owner;
                }
            foreach (Projectile p in rockets)
                if (p.alive && p.ownedOnLoad && p.loadout == null)
                {
                    p.loadout = loadout;
                    p.owner = owner;
                }
            foreach (Projectile[] list in new[] { grenades, vortexes }) // NIGHT20 fix list G2
                foreach (Projectile p in list)
                    if (p.alive && p.ownedOnLoad && p.loadout == null)
                    {
                        p.loadout = loadout;
                        p.owner = owner;
                    }
            foreach (Stuck s in stuck)
                if (s.alive && s.ownedOnLoad && s.loadout == null) s.loadout = loadout;
            foreach (Fragment f in fragments)
                if (f.ownedOnLoad && f.loadout == null)
                {
                    f.loadout = loadout;
                    f.owner = owner;
                }
        }

        public sealed class Projectile
        {
            public bool alive;
            public Vec3 position, previous, velocity, dir;
            public int ageSteps;
            public IProjectileLoadout loadout;
            public IProjectileOwner owner;
            public double trail;
            public bool hasHoming;
            public HomingTarget homing;
            public bool assisted;
            public double turned;
            public int shot;
            public double spin;
            public double radiusMult = 1;
            public int pierce;
            public readonly HashSet<Enemy> pierced = new HashSet<Enemy>();
            public int fragments;
            public bool hasSteer;
            public Vec3 steer;
            internal bool ownedOnLoad; // NIGHT13 U3-26.10: restored with an owner still to be handed over (Adopt)
            // NIGHT19 U3-33.6: the BOUNCER's grenades (projectiles.js make(): bounces, rest, spike)
            public int bounces;
            public bool rest, spike;
            // NIGHT20 B5: the VORTEX's orbs (projectiles.js make(): open, openSteps, pulled, repel, pullMult)
            public bool open, repel;
            public int openSteps;
            public double pullMult = 1;
            public readonly HashSet<Enemy> pulled = new HashSet<Enemy>();
        }

        public sealed class Stuck
        {
            public bool alive;
            public Enemy enemy;
            public int id;
            public Vec3 offset;
            public Vec3 dir;
            public int ageSteps;
            public IProjectileLoadout loadout;
            internal bool ownedOnLoad; // NIGHT13 U3-26.10 (Adopt)
        }

        /// <summary>A pending Comet fragment blast {loadout, owner, at, steps, shot, assisted}.</summary>
        sealed class Fragment
        {
            public IProjectileLoadout loadout;
            public IProjectileOwner owner;
            public int shot;
            public bool assisted;
            public Vec3 at;
            public int steps;
            internal bool ownedOnLoad; // NIGHT13 U3-26.10 (Adopt)
        }

        readonly EventBus events;
        readonly ICollisionWorld world;
        readonly EnemyRegistry enemies;
        Mulberry32 rng;
        public readonly Projectile[] nails;
        public readonly Projectile[] rockets;
        /// <summary>NIGHT19 U3-33.6: the BOUNCER's grenades (projectiles.js grenades = pool(w.bouncer.pool)); empty with design.arsenal off.</summary>
        public readonly Projectile[] grenades;
        /// <summary>NIGHT20 B5: the VORTEX's orbs (projectiles.js vortexes = pool(w.vortex.pool)); nothing flies with design.adds.b5 off.</summary>
        public readonly Projectile[] vortexes;
        // NIGHT20 B5: the orbs' numbers (weapons.vortex, weapons.specials.vortex.repelSpeed), read once
        readonly double vRadius, vLifetime, vPullRadius, vPullTime, vPullSpeed, vPullCore, vPullHold, vPullGrace, vBurstDamage, vBurstRadius, vEdge, vSpawnForward, vRepelSpeed;
        readonly uint gravityColor;
        public int vortexOpens, vortexBursts, vortexPulled; // projectiles.js stats (vortexOpens, vortexBursts, pulled)
        readonly VortexEvent vortexEvent = new VortexEvent();
        public readonly Stuck[] stuck;
        // NIGHT19: the grenades' numbers (weapons.bouncer), read once
        double gFuse, gSplash, gDamage; // NIGHT26 weaponIdentity rewrites these three (Projectiles.ApplyIdentity26)
        readonly double gGravity, gBounce, gFriction, gRadius, gEdge, gKnockback, gMinUp, gSpawnForward, gRestSpeed, gSpikeFuse;
        readonly uint blastColor;
        readonly List<Enemy> blastHits = new List<Enemy>(); // a blast's bodies, for the loadout's NIGHT19 hook (after the blast)
        public int grenadeBounces, grenadeBlasts; // projectiles.js stats
        readonly List<Fragment> fragments = new List<Fragment>(); // pending, in the web's push order
        readonly Stack<Fragment> spare = new Stack<Fragment>();
        int stuckNext;
        readonly List<Enemy> near = new List<Enemy>(); // enemies.within scratch
        /// <summary>particles.burst(point, colour, count, speed, life, size): the Game draws it (null: none).</summary>
        public Action<Vec3, uint, int, double, double, double> burst;

        // config, read once
        readonly double nailSpeed, nailRange, nailDamage, nailStickTime;
        readonly double rocketSpeed, rocketSpawnForward, rocketLifetime, rocketRadius;
        double rocketSplash, rocketDamage; // NIGHT26 weaponIdentity rewrites these two (Projectiles.ApplyIdentity26)
        readonly double rocketEdge, rocketKnockback, rocketMinUp;
        readonly double detonateDamage, detonateRadius;
        /// <summary>NIGHT33 P (design.n33.shop33 HOT NAILS; the sim sets it at a run's start): x the detonate damage. 1 = the web's (x 1.0 is exact, so the
        /// fingerprint is unchanged).</summary>
        public double detonateMult33 = 1;
        readonly double steerTurn, fragmentSpread, fragmentDelay, fragmentRadius, fragmentDamage;
        readonly double homingAssistAngle, capsuleHeight, simHz;
        readonly uint sparkColor, voltColor;

        // three's scratch vectors (projectiles.js:78-89); kept as locals where the web only uses them within one call.
        Vec3 tmp, seg, blastPoint, p2;

        public Projectiles(GameConfig cfg, EventBus events, ICollisionWorld world, EnemyRegistry enemies, uint seed = 1)
        {
            this.events = events;
            this.world = world;
            this.enemies = enemies;
            nails = Pool(cfg.Int("weapons.nailgun.pool"));
            rockets = Pool(cfg.Int("weapons.rocket.pool"));
            int stuckPool = cfg.Int("weapons.nailgun.stuckPool");
            stuck = new Stuck[stuckPool];
            for (int i = 0; i < stuckPool; i++) stuck[i] = new Stuck();
            nailSpeed = cfg.Num("weapons.nailgun.speed");
            nailRange = cfg.Num("weapons.nailgun.range");
            nailDamage = cfg.Num("weapons.nailgun.damage");
            nailStickTime = cfg.Num("weapons.nailgun.stickTime");
            rocketSpeed = cfg.Num("weapons.rocket.speed");
            rocketSpawnForward = cfg.Num("weapons.rocket.spawnForward");
            rocketLifetime = cfg.Num("weapons.rocket.lifetime");
            rocketRadius = cfg.Num("weapons.rocket.radius");
            rocketSplash = cfg.Num("weapons.rocket.splashRadius");
            rocketDamage = cfg.Num("weapons.rocket.damage");
            rocketEdge = cfg.Num("weapons.rocket.edgeDamage");
            rocketKnockback = cfg.Has("weapons.rocket.knockback") ? cfg.Num("weapons.rocket.knockback") : 0;
            rocketMinUp = cfg.Num("weapons.rocket.knockbackMinUp");
            detonateDamage = cfg.Num("weapons.specials.nailgun.detonateDamage");
            detonateRadius = cfg.Num("weapons.specials.nailgun.radius");
            steerTurn = cfg.Num("weapons.specials.rocket.steerTurn");
            fragmentSpread = cfg.Num("weapons.specials.rocket.fragmentSpread");
            fragmentDelay = cfg.Num("weapons.specials.rocket.fragmentDelay");
            fragmentRadius = cfg.Num("weapons.specials.rocket.fragmentRadius");
            fragmentDamage = cfg.Num("weapons.specials.rocket.fragmentDamage");
            homingAssistAngle = cfg.Num("aim.homingAssistAngle");
            capsuleHeight = cfg.Num("body.capsuleHeight");
            simHz = cfg.Num("loop.simHz");
            sparkColor = (uint)cfg.Num("colors.spark");
            voltColor = (uint)cfg.Num("weapons.families.volt");
            grenades = Pool(cfg.Int("weapons.bouncer.pool"));
            gFuse = cfg.Num("weapons.bouncer.fuse");
            gGravity = MovementMath.GravityFor(cfg.Num("jump.height"), cfg.Num("jump.apexTime")) * cfg.Num("weapons.bouncer.gravityMult");
            gBounce = cfg.Num("weapons.bouncer.bounce");
            gFriction = cfg.Num("weapons.bouncer.friction");
            gRadius = cfg.Num("weapons.bouncer.radius");
            gSplash = cfg.Num("weapons.bouncer.splashRadius");
            gDamage = cfg.Num("weapons.bouncer.damage");
            gEdge = cfg.Num("weapons.bouncer.edgeDamage");
            gKnockback = cfg.Num("weapons.bouncer.knockback");
            gMinUp = cfg.Num("weapons.bouncer.knockbackMinUp");
            gSpawnForward = cfg.Num("weapons.bouncer.spawnForward");
            gRestSpeed = cfg.Num("weapons.bouncer.restSpeed");
            gSpikeFuse = cfg.Num("weapons.bouncer.spikeFuse");
            blastColor = (uint)cfg.Num("weapons.families.blast");
            vortexes = Pool(cfg.Has("weapons.vortex.pool") ? cfg.Int("weapons.vortex.pool") : 0);
            if (vortexes.Length > 0)
            {
                string v = "weapons.vortex.";
                vRadius = cfg.Num(v + "radius");
                vLifetime = cfg.Num(v + "lifetime");
                vPullRadius = cfg.Num(v + "pullRadius");
                vPullTime = cfg.Num(v + "pullTime");
                vPullSpeed = cfg.Num(v + "pullSpeed");
                vPullCore = cfg.Num(v + "pullCore");
                vPullHold = cfg.Num(v + "pullHold");
                vPullGrace = cfg.Num(v + "pullGrace");
                vBurstDamage = cfg.Num(v + "burstDamage");
                vBurstRadius = cfg.Num(v + "burstRadius");
                vEdge = cfg.Num(v + "edgeDamage");
                vSpawnForward = cfg.Num(v + "spawnForward");
                vRepelSpeed = cfg.Num("weapons.specials.vortex.repelSpeed");
                gravityColor = (uint)cfg.Num("weapons.families.gravity");
            }
            Reset(seed);
        }

        static Projectile[] Pool(int n)
        {
            var list = new Projectile[n];
            for (int i = 0; i < n; i++) list[i] = new Projectile();
            return list;
        }

        /// <summary>A new run: nothing in flight, the stream reseeded (the web's Math.random has no seed; the port's has one per run).</summary>
        public void Reset(uint seed)
        {
            Clear();
            rng = new Mulberry32(seed);
            stuckNext = 0;
        }

        static Projectile Take(Projectile[] list)
        {
            foreach (Projectile p in list)
                if (!p.alive) return p;
            return null;
        }

        void Stick(Enemy enemy, Vec3 point, Vec3 dir, IProjectileLoadout loadout)
        {
            Stuck s = stuck[stuckNext];
            stuckNext = (stuckNext + 1) % stuck.Length;
            s.alive = true;
            s.loadout = loadout;
            s.enemy = enemy;
            s.id = enemy.id;
            tmp = enemy.AimPoint; // enemies.aimPoint(enemy, tmp): the web's `point` IS tmp here (see the header), so
            point = tmp; //          offset = aim - aim = (0, 0, 0)
            s.offset = point - tmp;
            s.dir = dir;
            s.ageSteps = 0;
        }

        enum HitType { None, World, Enemy }

        struct Sweep
        {
            public HitType type;
            public double distance;
            public Enemy enemy;
        }

        /// <summary>Sweep one projectile's step: the nearest hit among the world and enemies (no Drone orbs yet).</summary>
        Sweep SweepStep(Projectile p, double length, bool withEnemies = true)
        {
            var best = new Sweep { type = HitType.None };
            if (world.RayIntersect(p.previous, p.dir, out RayHit wall) && wall.distance <= length) best = new Sweep { type = HitType.World, distance = wall.distance };
            if (withEnemies)
            {
                EnemyRayHit? enemy = enemies.Raycast(p.previous, p.dir, length);
                if (enemy.HasValue && (best.type == HitType.None || enemy.Value.distance < best.distance))
                    best = new Sweep { type = HitType.Enemy, distance = enemy.Value.distance, enemy = enemy.Value.enemy };
            }
            return best;
        }

        /// <summary>Aim assist homing (POWER P2): turn toward the target's live aim point at a capped rate, speed kept.</summary>
        void Steer(Projectile p, double dt)
        {
            if (!p.hasHoming) return;
            HomingTarget h = p.homing;
            if (!h.enemy.alive || h.enemy.id != h.id)
            {
                p.hasHoming = false;
                p.homing = default;
                return;
            }
            tmp = h.enemy.AimPoint;
            p.turned += AssistMath.HomeVelocity(ref p.velocity, p.position, tmp, h.turn * dt);
            p.dir = Vec3.Normalize(p.velocity);
            if (p.turned > homingAssistAngle) p.assisted = true; // it would have missed: 0.8x on its hit
        }

        /// <summary>
        /// A splash: damage with falloff to every enemy in the radius, a push on the owner when `spec.knockback` (0 self
        /// damage), particles, the blast event and a shotResult.
        /// </summary>
        readonly BlastEvent blastEvent = new BlastEvent();
        readonly BlastJumpEvent jumpEvent = new BlastJumpEvent();
        readonly ShotEvent shotEvent = new ShotEvent { rayCount = 0, surface = "none" };

        void Blast(Projectile p, Vec3 point, double splashRadius, double damage, double edgeDamage, double knockback, double minUp, string weapon, string eventName, string jumpName, uint color0, uint color1)
        {
            int hits = 0;
            // OVERNIGHT2 N2: momentum at the trigger pull grew the radius (x the Bouncer's momentum variant); damage stays.
            double radius = splashRadius * p.radiusMult;
            enemies.Within(point, radius, near);
            blastHits.Clear();
            for (int i = 0; i < near.Count; i++)
            {
                Enemy enemy = near[i];
                tmp = enemy.AimPoint;
                double d = Vec3.DistanceTo(tmp, point);
                double dealt = JsMath.Round(WeaponMath.SplashDamage(JsMath.Min(d, radius), radius, damage, edgeDamage));
                if (dealt > 0)
                {
                    p.loadout.Damage(enemy, dealt, false, weapon, point, p.assisted, 1, p.shot, true);
                    hits++;
                    blastHits.Add(enemy);
                }
            }
            if (knockback != 0)
            {
                Vec3 m = p.owner.Position;
                tmp = new Vec3(m.x, m.y + capsuleHeight * 0.5, m.z);
                if (WeaponMath.BlastKnockback(point, tmp, radius, knockback, minUp, out Vec3 push) > 0)
                {
                    p.owner.Impulse(push.x, push.y, push.z);
                    if (push.y > 0)
                    {
                        jumpEvent.owner = p.owner.Id;
                        jumpEvent.speed = JsMath.Hypot(push.x, push.y, push.z);
                        jumpEvent.weapon = weapon;
                        events.Emit(jumpName, jumpEvent);
                    }
                }
            }
            if (selfBlast26 && knockback != 0 && p.loadout is Loadout selfLo) selfLo.SelfBlast26(weapon, point, radius); // NIGHT26 weaponIdentity: the owner's own blast hurts
            burst?.Invoke(point, color0, 30, 9, 0.5, 1.6);
            burst?.Invoke(point, color1, 12, 5, 0.25, 1.2);
            blastEvent.owner = p.owner.Id;
            blastEvent.position = point;
            blastEvent.radius = radius;
            blastEvent.hits = hits;
            events.Emit(eventName, blastEvent);
            // The Comet's momentum variant: fragments burst around the blast a moment later.
            for (int i = 0; i < p.fragments; i++)
            {
                double a = rng.Next() * Math.PI * 2;
                double r = fragmentSpread * (0.5 + 0.5 * rng.Next());
                Fragment f = spare.Count > 0 ? spare.Pop() : new Fragment();
                f.loadout = p.loadout;
                f.owner = p.owner;
                f.shot = p.shot;
                f.assisted = p.assisted;
                f.at = new Vec3(point.x + JsMath.Cos(a) * r, point.y + 0.3, point.z + JsMath.Sin(a) * r);
                f.steps = MovementMath.StepsFor(fragmentDelay * (1 + i * 0.35), 1 / simHz);
                fragments.Add(f);
            }
            // NIGHT19 U3-33.6: the loadout's blast combos (LAUNCH, NAILBOMB; nothing with design.arsenal off), after the blast.
            if (p.loadout is Loadout lo)
                for (int i = 0; i < blastHits.Count; i++) lo.OnBlastHit19(blastHits[i], weapon, point);
            EmitShotResult(p.owner, weapon, hits); // no tracer
            p.alive = false;
        }

        // NIGHT26 weaponIdentity (builder W): the rocket's and the BOUNCER's nerfs (damage, splash, fuse, pool) over the web's numbers, and the owner's own
        // blast hurting. The web's numbers are kept in `base*` so a run with the row off (Apply null) gets them back; one Projectiles lives across runs.
        double baseRocketSplash = -1, baseRocketDamage, baseGSplash, baseGDamage, baseGFuse;
        bool selfBlast26;
        int grenadeCap26; // 0: the pool's size

        /// <summary>NIGHT26: apply (or, with null, undo) the identity multipliers: rocket damage / splash, grenade damage / fuse / pool cap, self blast on.</summary>
        public void ApplyIdentity26(Identity26 id)
        {
            if (baseRocketSplash < 0)
            {
                baseRocketSplash = rocketSplash;
                baseRocketDamage = rocketDamage;
                baseGSplash = gSplash;
                baseGDamage = gDamage;
                baseGFuse = gFuse;
            }
            rocketSplash = baseRocketSplash * (id != null ? id.rocketSplashMult : 1);
            rocketDamage = baseRocketDamage * (id != null ? id.rocketDamageMult : 1);
            gSplash = baseGSplash * (id != null ? id.bouncerSplashMult : 1);
            gDamage = baseGDamage * (id != null ? id.bouncerDamageMult : 1);
            gFuse = id != null ? id.bouncerFuse : baseGFuse;
            grenadeCap26 = id != null ? id.bouncerPool : 0;
            selfBlast26 = id != null;
        }

        /// <summary>The fuse in whole steps now (the cook's cap).</summary>
        public int GrenadeFuseSteps(double dt) => MovementMath.StepsFor(gFuse, dt);

        void GrenadeBlast(Projectile p, Vec3 point, double mult = 1)
        {
            grenadeBlasts++;
            Blast(p, point, gSplash, gDamage * mult, gEdge, gKnockback, gMinUp, "bouncer", "grenadeBlast", "grenadeJump", blastColor, 0xfff6c8);
        }

        void EmitShotResult(IProjectileOwner owner, string weapon, int hits)
        {
            shotEvent.owner = owner.Id;
            shotEvent.weapon = weapon;
            shotEvent.hitCount = hits;
            events.Emit("shotResult", shotEvent);
            if (hits > 0) events.Emit("shotHit", weapon); // the port's run stats (shots hit), as the loadout's guns emit it
        }

        void Explode(Projectile r, Vec3 point) =>
            Blast(r, point, rocketSplash, rocketDamage, rocketEdge, rocketKnockback, rocketMinUp, "rocket", "rocketBlast", "rocketJump", 0xffa040, 0xfff0c0);

        /// <summary>A Vortex orb opens: it stops and starts pulling (NIGHT20 B5, projectiles.js openVortex).</summary>
        void OpenVortex(Projectile p)
        {
            p.open = true;
            p.openSteps = 0;
            p.velocity = Vec3.Zero;
            p.pulled.Clear();
            vortexOpens++;
            vortexEvent.owner = p.owner.Id;
            vortexEvent.position = p.position;
            events.Emit(G20Events.VortexOpen, vortexEvent);
        }

        /// <summary>NIGHT20 B5, projectiles.js burstVortex: the blast (no knockback on its owner, no jump event).</summary>
        void BurstVortex(Projectile p)
        {
            vortexBursts++;
            vortexPulled += p.pulled.Count;
            p.pulled.Clear();
            Blast(p, p.position, vBurstRadius, vBurstDamage, vEdge, 0, 0, "vortex", G20Events.VortexBurst, null, gravityColor, 0xe6dcff);
        }

        /// <summary>Fill a pooled projectile's common fields.</summary>
        Projectile Arm(Projectile p, IProjectileLoadout loadout, IProjectileOwner owner, Vec3 from, Vec3 velocity, HomingTarget? homing, bool assisted, double forward)
        {
            p.alive = true;
            p.loadout = loadout;
            p.owner = owner;
            p.velocity = velocity;
            p.dir = Vec3.Normalize(velocity);
            p.position = new Vec3(from.x + p.dir.x * forward, from.y + p.dir.y * forward, from.z + p.dir.z * forward);
            p.previous = p.position;
            p.ageSteps = 0;
            p.trail = 0;
            p.hasHoming = homing.HasValue;
            p.homing = homing ?? default;
            p.shot = loadout != null ? loadout.Shot : 0; // the trigger pull it came from (its hits share one hit award, OVERNIGHT2 N1)
            p.assisted = assisted; // homing that really turns it counts too (steer)
            p.turned = 0;
            p.spin = rng.Next() * Math.PI * 2;
            p.radiusMult = 1;
            p.pierce = 0;
            p.pierced.Clear();
            p.fragments = 0;
            p.hasSteer = false;
            p.bounces = 0; // NIGHT19: projectiles.js arm (bounces, rest, spike)
            p.rest = false;
            p.spike = false;
            p.open = false; // NIGHT20 B5: projectiles.js arm (open, openSteps, pulled, repel, pullMult)
            p.openSteps = 0;
            p.pulled.Clear();
            p.repel = false;
            p.pullMult = 1;
            return p;
        }

        /// <summary>
        /// Fire a projectile of `kind` ('nail' | 'rocket') from `from` along unit `dir`. `homing`: aim assist (POWER P2), turn
        /// toward it (rad/s); `assisted`: the shot was assisted (0.8x points + style on its hits). Null when the pool is full.
        /// </summary>
        public Projectile Fire(string kind, IProjectileLoadout loadout, IProjectileOwner owner, Vec3 from, Vec3 dir, HomingTarget? homing = null, bool assisted = false)
        {
            bool rocket = kind == "rocket";
            Projectile p = Take(rocket ? rockets : nails);
            if (p == null) return null;
            tmp = dir * (rocket ? rocketSpeed : nailSpeed);
            return Arm(p, loadout, owner, from, tmp, homing, assisted, rocket ? rocketSpawnForward : 0);
        }

        /// <summary>
        /// Launch a POWER P4 projectile with a full velocity: 'grenade' (Bouncer; NIGHT19 U3-33.6, ported), 'disc' (Discus:
        /// not ported, null) or 'vortex' (NIGHT20 B5, ported). Null when its pool is full.
        /// </summary>
        public Projectile Launch(string kind, IProjectileLoadout loadout, IProjectileOwner owner, Vec3 from, Vec3 velocity, HomingTarget? homing = null, bool assisted = false)
        {
            if (kind != "grenade" && kind != "vortex") return null;
            bool vortex = kind == "vortex";
            if (!vortex && grenadeCap26 > 0 && Count("grenade", loadout) >= grenadeCap26) return null; // NIGHT26 weaponIdentity: three in the air at a time
            Projectile p = Take(vortex ? vortexes : grenades);
            if (p == null) return null;
            return Arm(p, loadout, owner, from, velocity, homing, assisted, vortex ? vSpawnForward : gSpawnForward);
        }

        /// <summary>Everything in flight or stuck (a restart).</summary>
        public void Clear()
        {
            foreach (Projectile p in nails) p.alive = false;
            foreach (Projectile p in rockets) p.alive = false;
            foreach (Projectile p in grenades) p.alive = false;
            foreach (Projectile p in vortexes) p.alive = false; // NIGHT20 B5
            foreach (Stuck s in stuck) s.alive = false;
            foreach (Fragment f in fragments) spare.Push(f);
            fragments.Clear();
        }

        // --- OVERNIGHT2 N2 specials ------------------------------------------------------------------------------------

        /// <summary>N11: the Rattler's volley (homing darts). Not ported yet.</summary>
        public int Volley(IProjectileLoadout loadout, IProjectileOwner owner, Vec3 from, Vec3 dir, List<Enemy> targets, bool assisted = false) => 0;

        readonly DetonateEvent detonateEvent = new DetonateEvent();

        /// <summary>The Stitcher's alt: every nail this loadout has stuck in an enemy detonates now. Returns the nails.</summary>
        public int Detonate(IProjectileLoadout loadout, IProjectileOwner owner)
        {
            int n = 0;
            int shot = loadout.NewShot();
            foreach (Stuck s in stuck)
            {
                if (!s.alive || s.loadout != loadout || !s.enemy.alive || s.enemy.id != s.id) continue;
                tmp = s.enemy.AimPoint + s.offset;
                p2 = tmp;
                s.alive = false;
                n++;
                enemies.Within(p2, detonateRadius, near);
                for (int i = 0; i < near.Count; i++)
                {
                    Enemy enemy = near[i];
                    if (enemy.alive) loadout.Damage(enemy, detonateDamage * detonateMult33, false, "nailgun", p2, false, 1, shot, false);
                }
                burst?.Invoke(p2, voltColor, 6, 6, 0.25, 0.8);
            }
            if (n > 0)
            {
                detonateEvent.owner = owner.Id;
                detonateEvent.nails = n;
                events.Emit("nailDetonate", detonateEvent);
            }
            return n;
        }

        /// <summary>
        /// The Bouncer's alt: every grenade of this loadout in flight bursts now (NIGHT19 U3-33.6, ported). The web's
        /// airburst event puts the count in `owner` (:320, a web slip); here its payload is the count (an int).
        /// </summary>
        public int Airburst(IProjectileLoadout loadout)
        {
            int n = 0;
            foreach (Projectile p in grenades)
            {
                if (!p.alive || p.loadout != loadout) continue;
                blastPoint = p.position;
                GrenadeBlast(p, blastPoint);
                n++;
            }
            if (n > 0) events.Emit("airburst", n);
            return n;
        }

        // --- NIGHT19 U3-33.6 (design.arsenal): the combos' handles on the projectiles ----------------------------------

        /// <summary>
        /// POP: the nearest live rocket or grenade of `loadout` whose `radius` m sphere the ray (unit `dir`) meets within
        /// `maxDistance` m; null if none. `distance`: along the ray to its nearest point.
        /// </summary>
        public Projectile PopTarget(IProjectileLoadout loadout, Vec3 origin, Vec3 dir, double maxDistance, double radius, out double distance)
        {
            Projectile best = null;
            distance = maxDistance;
            Nearest(rockets, loadout, origin, dir, radius, ref best, ref distance);
            Nearest(grenades, loadout, origin, dir, radius, ref best, ref distance);
            return best;
        }

        static void Nearest(Projectile[] list, IProjectileLoadout loadout, Vec3 origin, Vec3 dir, double radius, ref Projectile best, ref double distance)
        {
            foreach (Projectile p in list)
            {
                if (!p.alive || p.loadout != loadout) continue;
                Vec3 oc = p.position - origin;
                double t = Vec3.Dot(oc, dir);
                if (t < 0 || t > distance) continue;
                double d2 = Vec3.Dot(oc, oc) - t * t;
                if (d2 > radius * radius) continue;
                best = p;
                distance = t;
            }
        }

        /// <summary>POP: burst a live rocket or grenade where it is, at x `mult` radius and damage.</summary>
        public void Pop(Projectile p, double mult)
        {
            p.radiusMult *= mult;
            blastPoint = p.position;
            if (Array.IndexOf(grenades, p) >= 0) GrenadeBlast(p, blastPoint, mult);
            else Blast(p, blastPoint, rocketSplash, rocketDamage * mult, rocketEdge, rocketKnockback, rocketMinUp, "rocket", "rocketBlast", "rocketJump", 0xffa040, 0xfff0c0);
        }

        /// <summary>The alt rule (AltUse): a live grenade of `loadout` has a body within its blast radius (an airburst would hit).</summary>
        public bool GrenadeByBody(IProjectileLoadout loadout)
        {
            foreach (Projectile p in grenades)
            {
                if (!p.alive || p.loadout != loadout) continue;
                enemies.Within(p.position, gSplash * p.radiusMult, near);
                if (near.Count > 0) return true;
            }
            return false;
        }

        /// <summary>This loadout's live nails stuck in `e` (CONDUCTOR's battery).</summary>
        public int StuckOn(Enemy e, IProjectileLoadout loadout)
        {
            int n = 0;
            foreach (Stuck s in stuck)
                if (s.alive && s.loadout == loadout && s.enemy == e && e.alive && e.id == s.id) n++;
            return n;
        }

        /// <summary>CONDUCTOR spends the nails in `e`.</summary>
        public void ConsumeStuck(Enemy e, IProjectileLoadout loadout)
        {
            foreach (Stuck s in stuck)
                if (s.alive && s.loadout == loadout && s.enemy == e) s.alive = false;
        }

        /// <summary>Each enemy's ConductiveNails += this loadout's live nails in it (the caller zeroed them).</summary>
        public void CountStuck(IProjectileLoadout loadout)
        {
            foreach (Stuck s in stuck)
                if (s.alive && s.loadout == loadout && s.enemy.alive && s.enemy.id == s.id) s.enemy.ConductiveNails++;
        }

        /// <summary>NAILBOMB: the Stitcher's detonate (Detonate, above) for the nails of `loadout` in `e` only. Returns the nails.</summary>
        public int DetonateOn(Enemy e, IProjectileLoadout loadout)
        {
            int n = 0;
            int shot = loadout.Shot;
            foreach (Stuck s in stuck)
            {
                if (!s.alive || s.loadout != loadout || s.enemy != e || !e.alive || e.id != s.id) continue;
                tmp = s.enemy.AimPoint + s.offset;
                p2 = tmp;
                s.alive = false;
                n++;
                enemies.Within(p2, detonateRadius, near);
                for (int i = 0; i < near.Count; i++)
                {
                    Enemy enemy = near[i];
                    if (enemy.alive) loadout.Damage(enemy, detonateDamage * detonateMult33, false, "nailgun", p2, false, 1, shot, false);
                }
                burst?.Invoke(p2, voltColor, 6, 6, 0.25, 0.8);
            }
            if (n > 0)
            {
                detonateEvent.owner = loadout is IProjectileOwner o ? o.Id : null;
                detonateEvent.nails = n;
                events.Emit("nailDetonate", detonateEvent);
            }
            return n;
        }

        /// <summary>The Comet's alt (held): this loadout's rockets turn toward `point` this step.</summary>
        public int SteerRockets(IProjectileLoadout loadout, Vec3 point)
        {
            int n = 0;
            foreach (Projectile r in rockets)
            {
                if (!r.alive || r.loadout != loadout) continue;
                r.hasSteer = true;
                r.steer = point;
                n++;
            }
            return n;
        }

        /// <summary>N11: the Discus's alt (recall). Not ported yet.</summary>
        public int Recall(IProjectileLoadout loadout) => 0;

        /// <summary>The Vortex's alt: a tap collapses this loadout's orbs now; held, open ones repel instead of pull (NIGHT20 B5).</summary>
        public int VortexAlt(IProjectileLoadout loadout, string mode)
        {
            int n = 0;
            foreach (Projectile p in vortexes)
            {
                if (!p.alive || p.loadout != loadout) continue;
                if (mode == "collapse")
                {
                    if (!p.open) OpenVortex(p);
                    BurstVortex(p);
                }
                else p.repel = mode == "repel";
                n++;
            }
            return n;
        }

        /// <summary>NIGHT20 B5 (the bot's alt rule, AltUse): live bodies within the burst radius of this loadout's open orbs.</summary>
        public int VortexGathered(IProjectileLoadout loadout)
        {
            int n = 0;
            foreach (Projectile p in vortexes)
            {
                if (!p.alive || !p.open || p.loadout != loadout) continue;
                enemies.Within(p.position, vBurstRadius * p.radiusMult, near);
                n += near.Count;
            }
            return n;
        }

        /// <summary>Live projectiles of a kind for a loadout (null: any loadout).</summary>
        public int Count(string kind, IProjectileLoadout loadout = null)
        {
            Projectile[] list = kind == "nail" ? nails : kind == "rocket" ? rockets : kind == "grenade" ? grenades : kind == "vortex" ? vortexes : null;
            if (list == null) return 0; // disc, dart: N11
            int n = 0;
            foreach (Projectile p in list)
                if (p.alive && (loadout == null || p.loadout == loadout)) n++;
            return n;
        }

        public int StuckCount(IProjectileLoadout loadout = null)
        {
            int n = 0;
            foreach (Stuck s in stuck)
                if (s.alive && (loadout == null || s.loadout == loadout)) n++;
            return n;
        }

        /// <summary>Pending Comet fragments (debug, tests).</summary>
        public int FragmentCount => fragments.Count;

        readonly FragmentEvent fragmentEvent = new FragmentEvent();

        /// <summary>
        /// NIGHT19 U3-33.6, projectiles.js stepGrenades (ported line for line): Bouncer grenades: gravity, bounces, a burst
        /// on an enemy or at the end of the fuse. The sweep keeps the wall's normal (the web's hit.triangle.getNormal).
        /// </summary>
        void StepGrenades(double dt)
        {
            int fuse = MovementMath.StepsFor(gFuse, dt);
            foreach (Projectile p in grenades)
            {
                if (!p.alive) continue;
                p.previous = p.position;
                if (!p.rest)
                {
                    p.velocity.y -= gGravity * dt;
                    seg = p.velocity * dt;
                    double len = seg.Length;
                    if (len > 1e-6)
                    {
                        p.dir = Vec3.DivideScalar(seg, len);
                        double reach = len + gRadius;
                        bool wallHit = world.RayIntersect(p.previous, p.dir, out RayHit wall) && wall.distance <= reach;
                        EnemyRayHit? enemy = enemies.Raycast(p.previous, p.dir, reach);
                        if (enemy.HasValue && (!wallHit || enemy.Value.distance < wall.distance))
                        {
                            double along = JsMath.Max(0, enemy.Value.distance - gRadius * 0.5);
                            blastPoint = new Vec3(p.previous.x + p.dir.x * along, p.previous.y + p.dir.y * along, p.previous.z + p.dir.z * along);
                            GrenadeBlast(p, blastPoint);
                            continue;
                        }
                        if (wallHit)
                        {
                            double along = JsMath.Max(0, wall.distance - gRadius);
                            p.position = new Vec3(p.previous.x + p.dir.x * along, p.previous.y + p.dir.y * along, p.previous.z + p.dir.z * along);
                            Vec3 normal = wall.normal;
                            if (p.spike)
                            {
                                // A spiked grenade sticks where it lands and pops shortly after (the grenade jump).
                                p.velocity = Vec3.Zero;
                                p.rest = true;
                                p.ageSteps = Math.Max(p.ageSteps, fuse - MovementMath.StepsFor(gSpikeFuse, dt));
                            }
                            else
                            {
                                GunMath.BounceVelocity(ref p.velocity, normal, gBounce, gFriction);
                                p.bounces++;
                                grenadeBounces++;
                                if (normal.y > 0.7 && p.velocity.Length < gRestSpeed)
                                {
                                    p.velocity = Vec3.Zero;
                                    p.rest = true;
                                }
                            }
                        }
                        else
                        {
                            p.position = p.position + seg;
                        }
                    }
                }
                if (enemies.HitSphere(p.position, gRadius) != null)
                {
                    blastPoint = p.position;
                    GrenadeBlast(p, blastPoint);
                    continue;
                }
                if (++p.ageSteps >= fuse)
                {
                    blastPoint = p.position;
                    GrenadeBlast(p, blastPoint);
                }
            }
        }

        /// <summary>
        /// NIGHT20 B5, projectiles.js stepVortexes (ported line for line): Vortex orbs: fly, open on contact (or at the end of
        /// the flight), pull everything in, burst. The sweep is the port's (the world and the bodies: no Drone orbs, as for
        /// every projectile here). e.isBoss: no boss is ported (Traps.cs's rule), so every live body is pulled.
        /// </summary>
        void StepVortexes(double dt)
        {
            int flight = MovementMath.StepsFor(vLifetime, dt);
            int pull = MovementMath.StepsFor(vPullTime, dt);
            foreach (Projectile p in vortexes)
            {
                if (!p.alive) continue;
                p.previous = p.position;
                if (!p.open)
                {
                    seg = p.velocity * dt;
                    double len = seg.Length;
                    Sweep hit = SweepStep(p, len + vRadius);
                    if (hit.type != HitType.None)
                    {
                        double along = JsMath.Max(0, hit.distance - vRadius);
                        p.position = new Vec3(p.previous.x + p.dir.x * along, p.previous.y + p.dir.y * along, p.previous.z + p.dir.z * along);
                        OpenVortex(p);
                    }
                    else
                    {
                        p.position = p.position + seg;
                        if (++p.ageSteps >= flight) OpenVortex(p);
                    }
                    continue;
                }
                p.openSteps++;
                double left = (pull - p.openSteps) * dt;
                double pullRadius = vPullRadius * p.pullMult; // the Vortex's momentum variant: a wider pull
                enemies.Within(p.position, pullRadius, near);
                for (int i = 0; i < near.Count; i++)
                {
                    Enemy enemy = near[i];
                    if (!enemy.alive) continue;
                    double dx = p.position.x - enemy.position.x;
                    double dz = p.position.z - enemy.position.z;
                    // Held alt (the Vortex's special): it repels instead of pulling.
                    double speed = p.repel ? vRepelSpeed : GunMath.PullSpeed(JsMath.Hypot(dx, dz), pullRadius, vPullSpeed, vPullCore);
                    if (speed > 0) enemy.Knockback(p.repel ? -dx : dx, p.repel ? -dz : dz, speed, vPullHold, dt);
                    if (!p.pulled.Contains(enemy))
                    {
                        p.pulled.Add(enemy);
                        if (p.loadout.ArsenalEnabled && p.loadout is Loadout lo) lo.arsenal.Status(enemy, "pull", left + vPullGrace);
                    }
                }
                if (p.openSteps >= pull) BurstVortex(p);
            }
        }

        public void Step(double dt)
        {
            int nailLife = MovementMath.StepsFor(nailRange / nailSpeed, dt);
            foreach (Projectile p in nails)
            {
                if (!p.alive) continue;
                p.previous = p.position;
                Steer(p, dt);
                seg = p.velocity * dt;
                Sweep hit = SweepStep(p, seg.Length);
                if (hit.type != HitType.None)
                {
                    tmp = new Vec3(p.previous.x + p.dir.x * hit.distance, p.previous.y + p.dir.y * hit.distance, p.previous.z + p.dir.z * hit.distance);
                    if (hit.type == HitType.Enemy && p.pierced.Contains(hit.enemy))
                    {
                        p.position = p.position + seg; // already through this one (a piercing nail)
                        continue;
                    }
                    if (hit.type == HitType.Enemy)
                    {
                        bool killed = p.loadout.Damage(hit.enemy, nailDamage, false, "nailgun", p.previous, p.assisted, 1, p.shot, false);
                        if (!killed)
                        {
                            Stick(hit.enemy, tmp, p.dir, p.loadout);
                            if (p.loadout.ArsenalEnabled) p.loadout.Status(hit.enemy, "stuck");
                        }
                        EmitShotResult(p.owner, "nailgun", 1); // no tracer
                        if (p.pierce > 0)
                        {
                            // The Stitcher's momentum variant: the nail goes on through.
                            p.pierce--;
                            p.pierced.Add(hit.enemy);
                            p.position = p.position + seg;
                            continue;
                        }
                    }
                    else
                    {
                        burst?.Invoke(tmp, sparkColor, 2, 3, 0.12, 0.35);
                    }
                    p.alive = false;
                    continue;
                }
                p.position = p.position + seg;
                if (++p.ageSteps >= nailLife) p.alive = false;
            }
            int rocketLife = MovementMath.StepsFor(rocketLifetime, dt);
            foreach (Projectile r in rockets)
            {
                if (!r.alive) continue;
                r.previous = r.position;
                if (r.hasSteer)
                {
                    // The Comet's alt: held, the rocket turns toward the crosshair (the assist's homing waits).
                    p2 = r.steer;
                    r.turned += AssistMath.HomeVelocity(ref r.velocity, r.position, p2, steerTurn * Math.PI / 180 * dt);
                    r.dir = Vec3.Normalize(r.velocity);
                    r.hasSteer = false;
                }
                else Steer(r, dt);
                seg = r.velocity * dt;
                Sweep hit = SweepStep(r, seg.Length + rocketRadius);
                if (hit.type != HitType.None)
                {
                    double along = JsMath.Max(0, hit.distance - rocketRadius * 0.5);
                    blastPoint = new Vec3(r.previous.x + r.dir.x * along, r.previous.y + r.dir.y * along, r.previous.z + r.dir.z * along);
                    Explode(r, blastPoint);
                    continue;
                }
                r.position = r.position + seg;
                if (++r.ageSteps >= rocketLife)
                {
                    blastPoint = r.position;
                    Explode(r, blastPoint);
                }
            }
            int stickLife = MovementMath.StepsFor(nailStickTime, dt);
            foreach (Stuck s in stuck)
            {
                if (!s.alive) continue;
                if (!s.enemy.alive || s.enemy.id != s.id || ++s.ageSteps >= stickLife) s.alive = false;
            }
            StepGrenades(dt); // NIGHT19 U3-33.6 (the BOUNCER; nothing in flight with design.arsenal off)
            // stepDiscs, stepDarts: N11 (their pools are empty).
            StepVortexes(dt); // NIGHT20 B5 (the VORTEX; nothing in flight with design.adds.b5 off)
            // Comet fragments (its momentum variant) burst a moment after the rocket.
            for (int i = fragments.Count - 1; i >= 0; i--)
            {
                Fragment f = fragments[i];
                if (--f.steps > 0) continue;
                int hits = 0;
                enemies.Within(f.at, fragmentRadius, near);
                for (int k = 0; k < near.Count; k++)
                {
                    f.loadout.Damage(near[k], fragmentDamage, false, "rocket", f.at, f.assisted, 1, f.shot, true);
                    hits++;
                }
                burst?.Invoke(f.at, 0xffa040, 10, 6, 0.3, 1);
                fragmentEvent.owner = f.owner.Id;
                fragmentEvent.position = f.at;
                fragmentEvent.hits = hits;
                events.Emit("rocketFragment", fragmentEvent);
                fragments.RemoveAt(i);
                spare.Push(f);
            }
        }
    }
}
