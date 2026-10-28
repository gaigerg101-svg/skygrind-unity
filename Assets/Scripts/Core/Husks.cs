// The ground enemy pools (husk.js createHusks, enemyKind.js for the Swarmer and the Spitter: up to <kind>.maxCount each,
// spawn with a clear capsule, step every live one, the kill plane). Each fixed step a nav jump (NavJumper) may own a body;
// otherwise the kind's own step runs, with its brain's velocity when it has one (HuskBrain). Registered in the
// EnemyRegistry so weapons and the director see them. Stepped in enemies.js order: the husks, the swarmers, then the
// spitters and their globs and puddles (spitter.js runs its acid after its enemies).
// NIGHT19 U3-33.7 (builder E): the CHARGER's pool (Core/Enemies/Charger.cs; charger.maxCount), stepped after the spitters
// (main.js createEnemies: husk, drone, swarmer, spitter, charger). Only design.enemies.n19 spawns one (the director), so
// with the block off the pool stays empty and every step, count and snapshot is NIGHT18's.
// NIGHT19 U3-33.7: the DRONE's pool (Core/Enemies/Drone.cs; drone.maxCount), stepped right after the husks (that same
// order), and the drones' orbs (DroneOrbs, orb.poolSize), stepped before every pool (main.js simulate: orbs.step, then
// enemies.step); the acid's punch reflect reaches the orbs (SpitterAcid.orbs).
// NIGHT20 U3-34.9 C2 (builder E): the LANCER's pool (Core/Enemies/Lancer.cs; lancer.maxCount), stepped after the chargers
// (main.js createEnemies: husk, drone, swarmer, spitter, charger, lancer); its rolls are a stream from the run seed ^
// design.adds.lancer.salt; nothing spawns one unless the director deals it (design.adds.lancer). The drones' rolls (the web's
// Math.random) are a stream from the run seed ^ design.enemies.n19.droneSalt. Live orbs are not part of the run snapshot
// (a break's save restores none: an orb lives orb.lifetime s).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Husks : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot. Only at a break: an enemy alive refuses the save (StateException; a live
        // fight is not restorable). What is saved is the pools' layout, which decides what later rounds do: the dead
        // enemies in the registry's order (a spawn reuses the first dead one of its kind's pool, and the registry's order
        // is the order every system walks the enemies in), each one's id and last body (a reference by index lands on
        // it), the STASIS freeze and the spitters' acid (its stream, globs and puddles: live ones at a break are saved,
        // with their shooters by index). Load rebuilds the pools and the registry's list with fresh dead enemies.
        public void Save(StateWriter w)
        {
            int n = 0;
            foreach (List<Husk> list in lists) n += list.Count;
            if (registry.all.Count != n) throw new StateException($"snapshot: the registry holds {registry.all.Count} enemies, the pools {n}");
            var kinds = new List<string>(n);
            var ids = new List<int>(n);
            var hp = new List<double>(n);
            var maxHp = new List<double>(n);
            var yaw = new List<double>(n);
            var at = new List<Vec3>(n);
            var prev = new List<Vec3>(n);
            var vel = new List<Vec3>(n);
            var seen = new int[KindCount];
            foreach (Enemy e in registry.all)
            {
                if (e.alive) throw new StateException($"snapshot: a {e.kind} (id {e.id}) is alive: a run is saved at a round break only");
                if (!(e is Husk h)) throw new StateException($"snapshot: a {e.GetType().Name} in the registry");
                int k = Math.Max(0, KindIndex(h.kind));
                if (seen[k] >= lists[k].Count || !ReferenceEquals(lists[k][seen[k]], h)) throw new StateException("snapshot: the registry's order is not the pools' creation order");
                seen[k]++;
                kinds.Add(h.kind);
                ids.Add(h.id);
                hp.Add(h.hp);
                maxHp.Add(h.maxHp);
                yaw.Add(h.yaw);
                at.Add(h.position);
                prev.Add(h.previous);
                vel.Add(h.velocity);
            }
            w.Strings("kinds", kinds);
            w.Ints("ids", ids);
            w.Doubles("hp", hp);
            w.Doubles("maxHp", maxHp);
            w.Doubles("yaw", yaw);
            w.Vecs("position", at);
            w.Vecs("previous", prev);
            w.Vecs("velocity", vel);
            w.B("frozen", frozen);
            w.Begin("acid");
            acid.SaveState(w, registry);
            w.End();
            // NIGHT20 fix EN2 (RunRestoreTests, seed 37 at round 5's break): the drones' and the lancers' streams carry across a
            // break (a restore restarted them at the run seed), and the NIGHT19 / NIGHT20 enemy systems the sim hands in (n20:
            // the affixes, the special rounds, the director's NIGHT20 clocks, the BULWARK's stream). Absent in an older save:
            // read as the run's start (the old behaviour).
            w.I("droneRng", droneRng.State);
            w.I("lancerRng", lancerRng.State);
            if (n20State != null)
            {
                w.Begin("n20");
                n20State.Save(w);
                w.End();
            }
        }

        /// <summary>NIGHT20 fix EN2: the run's other enemy systems' state, saved inside this section (SurvivalSim.Director20.cs
        /// sets it; null: none).</summary>
        public IRunState n20State;

        public void Load(StateReader r)
        {
            List<string> kinds = r.Strings("kinds");
            List<int> ids = r.Ints("ids");
            List<double> hp = r.Doubles("hp"), maxHp = r.Doubles("maxHp"), yaw = r.Doubles("yaw");
            List<Vec3> at = r.Vecs("position"), prev = r.Vecs("previous"), vel = r.Vecs("velocity");
            int n = kinds.Count;
            if (ids.Count != n || hp.Count != n || maxHp.Count != n || yaw.Count != n || at.Count != n || prev.Count != n || vel.Count != n) throw new StateException("snapshot: husks: the pools' lists differ in length");
            var counts = new int[KindCount];
            foreach (string kind in kinds)
            {
                int k = KindIndex(kind);
                if (k < 0) throw new StateException($"snapshot: husks: an unknown kind '{kind}'");
                counts[k]++;
            }
            if (counts[0] > capacity || counts[1] > swarmerCapacity || counts[2] > spitterCapacity || counts[3] > chargerCapacity || counts[4] > droneCapacity || counts[5] > lancerCapacity || counts[6] > wardenCapacity || (burrow43 != null && counts[7] > burrow43.maxCount)) throw new StateException("snapshot: husks: more enemies than the pools hold");
            foreach (Enemy e in registry.all) if (e.alive) throw new StateException("snapshot: husks: loading over a live enemy");
            foreach (List<Husk> list in lists) list.Clear();
            registry.all.Clear();
            for (int i = 0; i < n; i++)
            {
                Husk h = Make(kinds[i]);
                h.alive = false;
                h.id = ids[i];
                h.hp = hp[i];
                h.maxHp = maxHp[i];
                h.yaw = yaw[i];
                h.position = at[i];
                h.previous = prev[i];
                h.velocity = vel[i];
                ListOf(kinds[i]).Add(h);
                registry.all.Add(h);
            }
            frozen = r.B("frozen");
            acid.LoadState(r.Section("acid"), registry);
            if (r.Has("droneRng")) droneRng.State = r.U("droneRng"); // NIGHT20 fix EN2
            if (r.Has("lancerRng")) lancerRng.State = r.U("lancerRng");
            if (n20State != null && r.Has("n20")) n20State.Load(r.Section("n20"));
        }

        readonly GameConfig cfg;
        readonly EventBus events;
        readonly EnemyRegistry registry;
        readonly ICollisionWorld world;
        public readonly HuskParams p; // the husk's
        public readonly HuskParams swarmer;
        public readonly HuskParams spitter;
        public readonly SpitterAcid acid; // the spitters' globs and puddles
        public readonly DroneOrbs orbs; // NIGHT19: the drones' orbs (pooled)
        public readonly DroneParams drone; // NIGHT19: config.drone
        Mulberry32 droneRng = new Mulberry32(1); // NIGHT19: the drones' rolls (the web's Math.random)
        readonly Func<double> droneRandom;
        readonly uint droneSalt;
        readonly List<Husk> pool = new List<Husk>(); // husks
        readonly List<Husk> swarmers = new List<Husk>();
        readonly List<Husk> spitters = new List<Husk>();
        readonly List<Husk> chargers = new List<Husk>(); // NIGHT19: the CHARGER's
        readonly List<Husk> drones = new List<Husk>(); // NIGHT19: the DRONE's
        readonly List<Husk> lancers = new List<Husk>(); // NIGHT20: the LANCER's
        readonly List<Husk> wardens = new List<Husk>(); // NIGHT20 C4: the WARDEN's
        readonly List<Husk> burrowers = new List<Husk>(); // NIGHT43 A1: the BURROWER's (Core/Enemies/Burrower43.cs; empty unless burrow43 is on)
        /// <summary>NIGHT43 A1 burrow43 (the sim sets both at a run's start, null with the row off: "burrower" is then a plain husk, as every unknown kind).</summary>
        public Burrow43Params burrow43;
        public HuskParams burrowBody43;
        /// <summary>NIGHT20 C4: config.warden (its body through enemyKind.js, its attack and shield numbers).</summary>
        public readonly HuskParams warden;
        public readonly WardenParams wardenAttack;
        /// <summary>NIGHT20: config.lancer.</summary>
        public readonly LancerParams lancer;
        Mulberry32 lancerRng = new Mulberry32(1); // NIGHT20: the lancers' rolls (the web's Math.random)
        readonly Func<double> lancerRandom;
        readonly uint lancerSalt;
        readonly List<Husk>[] lists; // the pools (EngagedNear walks them without allocating)
        readonly int capacity, swarmerCapacity, spitterCapacity, chargerCapacity, droneCapacity, lancerCapacity, wardenCapacity;
        /// <summary>NIGHT19: config.charger (its body through enemyKind.js, its attack numbers).</summary>
        public readonly HuskParams charger;
        public readonly ChargerParams chargerAttack;

        /// <summary>The pools (husk, swarmer, spitter, charger, drone): the snapshot's kind index.</summary>
        const int KindCount = 8; // NIGHT20: the lancer's, the warden's; NIGHT43 A1: the burrower's

        static int KindIndex(string kind) => kind == "husk" ? 0 : kind == "swarmer" ? 1 : kind == "spitter" ? 2 : kind == "charger" ? 3 : kind == "drone" ? 4 : kind == "lancer" ? 5 : kind == "warden" ? 6 : kind == "burrower" ? 7 : -1;
        readonly double gravity;
        /// <summary>NIGHT20 (design.adds.lancer): the LANCER's perches (main.js getPerches: the director's high spawn nodes) and
        /// whether you're hunted (main.js isHunted: the director's camp score at 100 %); the sim sets both.</summary>
        public Func<List<SpawnNode>> lancerPerches = () => null;
        public Func<bool> lancerHunted = () => false;
        public bool frozen; // debug: nobody moves
        public NavJumper jumper; // S6: jump / gap links

        /// <summary>NIGHT29 design.n29.tank.hp (the sim sets it at a run start): the hit points of a body spawned as kind "tank"; 0 = no tank (the block is off).</summary>
        public double tankHp;

        public Husks(GameConfig cfg, EventBus events, EnemyRegistry registry, ICollisionWorld world, double killPlaneY)
        {
            this.cfg = cfg;
            this.events = events;
            this.registry = registry;
            this.world = world;
            p = HuskParams.From(cfg, killPlaneY);
            swarmer = HuskParams.Swarmer(cfg, killPlaneY);
            spitter = HuskParams.Spitter(cfg, killPlaneY);
            acid = new SpitterAcid(cfg, events, killPlaneY);
            capacity = cfg.Int("husk.maxCount");
            swarmerCapacity = cfg.Int("swarmer.maxCount");
            spitterCapacity = cfg.Int("spitter.maxCount");
            charger = ChargerParams.Body(cfg, killPlaneY);
            chargerAttack = ChargerParams.From(cfg);
            chargerCapacity = cfg.Int("charger.maxCount");
            drone = DroneParams.From(cfg, killPlaneY);
            droneCapacity = cfg.Int("drone.maxCount");
            orbs = new DroneOrbs(cfg, events);
            acid.orbs = orbs; // the punch's reflect reaches the orbs
            droneRandom = () => droneRng.Next();
            droneSalt = cfg.Has("design.enemies.n19.droneSalt") ? (uint)cfg.Num("design.enemies.n19.droneSalt") : 0xd201u;
            lancer = LancerParams.From(cfg, killPlaneY); // NIGHT20
            lancerCapacity = lancer.maxCount;
            lancerRandom = () => lancerRng.Next();
            lancerSalt = cfg.Has("design.adds.lancer.salt") ? (uint)cfg.Num("design.adds.lancer.salt") : 0x1a9cu;
            warden = WardenParams.Body(cfg, killPlaneY); // NIGHT20 C4
            wardenAttack = WardenParams.From(cfg);
            wardenCapacity = cfg.Int("warden.maxCount");
            lists = new[] { pool, swarmers, spitters, chargers, drones, lancers, wardens, burrowers }; // NIGHT43 A1: + the burrowers'
            gravity = MovementMath.GravityFor(cfg.movement.jump.height, cfg.movement.jump.apexTime);
        }

        /// <summary>A new run: the acid's seeded stream (the spitters' miss sides; the web rolls Math.random()).</summary>
        public void NewRun(uint seed)
        {
            acid.rng = new Mulberry32((seed ^ 0x5917u) >> 0);
            droneRng = new Mulberry32(seed ^ droneSalt); // NIGHT19: the drones' rolls
            lancerRng = new Mulberry32(seed ^ lancerSalt); // NIGHT20: the lancers' rolls
        }

        List<Husk> ListOf(string kind) => kind == "swarmer" ? swarmers : kind == "spitter" ? spitters : kind == "charger" ? chargers : kind == "drone" ? drones : kind == "lancer" ? lancers : kind == "warden" ? wardens
            : kind == "burrower" && burrow43 != null ? burrowers : pool; // NIGHT43 A1: the burrowers' while burrow43 is on

        int CapacityOf(string kind) => kind == "swarmer" ? swarmerCapacity : kind == "spitter" ? spitterCapacity : kind == "charger" ? chargerCapacity : kind == "drone" ? droneCapacity : kind == "lancer" ? lancerCapacity : kind == "warden" ? wardenCapacity
            : kind == "burrower" && burrow43 != null ? burrow43.maxCount : capacity; // NIGHT43 A1

        /// <summary>A new body of the kind (an unknown kind is a husk, as before; NIGHT43 A1: "burrower" a BURROWER while burrow43 is on).</summary>
        Husk Make(string kind) => kind == "burrower" && burrow43 != null ? new Burrower43(burrowBody43, burrow43, events) // NIGHT43 A1
            : kind == "swarmer" ? new Swarmer(swarmer, events) : kind == "spitter" ? new Spitter(spitter, events, acid)
            : kind == "charger" ? new Charger(charger, chargerAttack, events) : kind == "drone" ? new Drone(drone, orbs, events, droneRandom)
            : kind == "lancer" ? new Lancer(lancer, events, lancerRandom, () => lancerPerches(), () => lancerHunted())
            : kind == "warden" ? new Warden(warden, wardenAttack, events) : (Husk)new Husk(p);

        public int Count
        {
            get
            {
                int n = 0;
                foreach (Husk h in pool) if (h.alive) n++;
                foreach (Husk h in swarmers) if (h.alive) n++;
                foreach (Husk h in spitters) if (h.alive) n++;
                foreach (Husk h in chargers) if (h.alive) n++;
                foreach (Husk h in drones) if (h.alive) n++;
                foreach (Husk h in lancers) if (h.alive) n++;
                foreach (Husk h in wardens) if (h.alive) n++;
                foreach (Husk h in burrowers) if (h.alive) n++; // NIGHT43 A1
                return n;
            }
        }

        public int CountOf(string kind)
        {
            int n = 0;
            foreach (Husk h in ListOf(kind)) if (h.alive) n++;
            return n;
        }

        /// <summary>PEAK FADE's "engaged": live enemies committed to an attack (a token holder's wind-up, crouch, tell) within `range` m.</summary>
        public int EngagedNear(Vec3 p, double range)
        {
            int n = 0;
            foreach (List<Husk> list in lists)
            {
                foreach (Husk h in list)
                {
                    if (!h.alive || !h.committed) continue;
                    double dx = h.position.x - p.x, dy = h.position.y - p.y, dz = h.position.z - p.z;
                    if (dx * dx + dy * dy + dz * dz <= range * range) n++;
                }
            }
            return n;
        }

        /// <summary>The live ones of a kind, in pool order (the crowd pass gathers husks, then swarmers, then spitters).</summary>
        public List<Husk> PoolOf(string kind) => ListOf(kind);

        /// <summary>Try to place a Husk at `pos`; null if the pool is full or the capsule would start inside geometry.</summary>
        public Husk Spawn(Vec3 pos, Vec3 target) => Spawn("husk", pos, target);

        /// <summary>Try to place one of `kind` ("husk" | "swarmer" | "spitter"); null if its pool is full or the capsule would start inside geometry.</summary>
        public Husk Spawn(string kind, Vec3 pos, Vec3 target)
        {
            bool tank = kind == "tank" && tankHp > 0; // NIGHT29 design.n29.tank: a husk body with its own hp (tankHp 0 = block off: an unknown kind is a husk, as before)
            List<Husk> list = ListOf(kind);
            Husk husk = null;
            foreach (Husk h in list) if (!h.alive) { husk = h; break; }
            if (husk == null)
            {
                if (list.Count >= CapacityOf(kind)) return null;
                husk = Make(kind);
                list.Add(husk);
                registry.all.Add(husk);
            }
            if (!husk.Spawn(registry.NextId(), pos, target, world)) return null;
            if (tank)
            {
                husk.tank29 = true;
                husk.hp = husk.maxHp = tankHp;
            }
            events.Emit("enemySpawn", new SpawnEvent { id = husk.id, kind = husk.kind, position = husk.position });
            return husk;
        }

        /// <summary>NIGHT14 U3-27.2 (port-only: a clean slate per run): the three pools emptied (their enemies dropped, not reused),
        /// the registry with them, so a run spawns into fresh pools in its own order as a fresh composition does. Reused dead
        /// enemies from the last run reordered the registry (the crowd pass, the brain, every nearest-enemy tie).</summary>
        public void ClearPools()
        {
            KillAll();
            pool.Clear();
            swarmers.Clear();
            spitters.Clear();
            chargers.Clear();
            drones.Clear();
            lancers.Clear();
            wardens.Clear();
            burrowers.Clear(); // NIGHT43 A1
            registry.Reset();
            acid.DropEnemies(); // the acid's dead slots held the last run's spitters (the snapshot refused them)
        }

        public void KillAll()
        {
            foreach (Husk h in pool) h.alive = false;
            foreach (Husk h in swarmers) h.alive = false;
            foreach (Husk h in spitters) h.alive = false;
            foreach (Husk h in chargers) h.alive = false;
            foreach (Husk h in drones) h.alive = false;
            foreach (Husk h in lancers) h.alive = false;
            foreach (Husk h in wardens) h.alive = false;
            foreach (Husk h in burrowers) h.alive = false; // NIGHT43 A1
            acid.Clear();
            orbs.Clear(); // NIGHT19: the drones' orbs go with them // spitter.js killAll: the globs and puddles go too
        }

        /// <summary>One fixed step against one target (the web's getTarget(); the tests and the bench).</summary>
        public void Step(Vec3 targetPos, Health target, double dt) => Step(null, targetPos, target, dt);

        /// <summary>One fixed step: each enemy hunts the nearest living player (Players.Nearest from its feet, NIGHT9).</summary>
        public void Step(Players players, double dt) => Step(players, default, null, dt);

        void Step(Players players, Vec3 targetPos, Health target, double dt)
        {
            orbs.Step(players, targetPos, target, world, registry, dt); // NIGHT19 (main.js simulate: orbs before the enemies; none without drones)
            StepPool(pool, players, targetPos, target, dt);
            StepPool(drones, players, targetPos, target, dt); // NIGHT19 (main.js createEnemies: husk, drone, swarmer, spitter, charger)
            StepPool(swarmers, players, targetPos, target, dt);
            StepPool(spitters, players, targetPos, target, dt);
            if (players != null) acid.Step(players, world, registry, dt);
            else acid.Step(targetPos, target, world, registry, dt);
            StepPool(chargers, players, targetPos, target, dt); // NIGHT19 (main.js createEnemies order: after the spitters)
            StepPool(lancers, players, targetPos, target, dt); // NIGHT20 (main.js createEnemies order: after the chargers)
            StepPool(wardens, players, targetPos, target, dt); // NIGHT20 C4 (main.js createEnemies order: after the lancers)
            StepPool(burrowers, players, targetPos, target, dt); // NIGHT43 A1 (port-only, last; empty unless burrow43 spawned one)
        }

        void StepPool(List<Husk> list, Players players, Vec3 targetPos, Health target, double dt)
        {
            foreach (Husk husk in list)
            {
                if (!husk.alive) continue;
                husk.previous = husk.position;
                if (frozen || husk.standIn29) continue; // (NIGHT29: a stand-in stands: the sim moves it)
                if (players != null)
                {
                    Player t = players.Nearest(husk.position);
                    targetPos = t.Feet;
                    target = t.health;
                    acid.leadTarget = t; // a spitter's volley leads its own target's velocity
                    if (husk is Charger ch) ch.targetMover = t.mover; // NIGHT19: the throw's impulse (charger.js pm)
                    else if (husk is Drone dr) dr.targetMover = t.mover; // NIGHT19: the orb's lead (brain.js aim)
                    else if (husk is Lancer ln) ln.targetMover = t.mover; // NIGHT20: the crouch lowers its chest (lancer.js pm.crouched)
                    else if (husk is Burrower43 bu) bu.targetMover = t.mover; // NIGHT43 A1: its ground contact and the throw
                }
                husk.Tick(jumper, targetPos, target, registry.all, world, events, dt, gravity);
                if (husk.position.y < p.killPlaneY) husk.alive = false;
            }
            acid.leadTarget = null;
        }
    }

    public sealed class SpawnEvent
    {
        public int id;
        public string kind;
        public Vec3 position;
    }
}
