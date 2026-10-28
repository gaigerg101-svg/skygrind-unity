// NIGHT8 U2-02: the sim's steady state allocates nothing. On the real map with every zone open, 30 enemies (24 husks and
// 6 swarmers) chase a player who holds the PULSE GAUNTLET (its parry sweep runs every step) with SNAP aim assist on.
// After a warm-up (pools, lists and boxes grow to size), 600 steps of Loadout.Step, AimAssist.Step, HuskBrain.Step,
// Husks.Step, the crowd pass and Director.Step allocate 0 B. The player is invulnerable (no hurt payloads) and never
// fires: a shot's hit / kill / shotResult payloads are still allocated per shot (a few hundred bytes), not per step.
// The player stands still, so the test holds the director's linger timer at 0 (no campStart event, whose payload is a
// boxed double; a real player who camps gets that one event).
// Also: the EventBus keeps the web's copy-on-emit semantics without copying.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints; // AllocatingGCMemory() is an extension method here
using AllocIs = UnityEngine.TestTools.Constraints.Is;
using Is = NUnit.Framework.Is;

namespace Skygrind.Core.Tests
{
    public class ZeroAllocTests
    {
        static GameConfig cfg;
        static SurvivalMap map;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
        }

        sealed class Sim
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly GameWorld world;
            public readonly CoreGrid core;
            public readonly Zones zones;
            public readonly NavField nav;
            public readonly HuskBrain brain;
            public readonly Husks husks;
            public readonly Health health;
            public readonly Mover mover;
            public readonly Loadout loadout;
            public readonly AimAssist assist;
            public readonly Director director;
            public Vec3 player;
            public double yaw, pitch;
            readonly double dt;

            public Sim()
            {
                dt = cfg.Dt;
                core = map.CreateCore(cfg.movement);
                world = new GameWorld(map.BuildOctree(), core);
                zones = new Zones(cfg, map, world, events);
                zones.OpenAll();
                nav = new NavField(cfg, NavGrid.FromExport(map.navGrid));
                nav.SetBlocked(zones.LockedTag);
                brain = new HuskBrain(cfg, events, nav, world.Geometry);
                brain.onScreen = _ => true;
                husks = new Husks(cfg, events, enemies, world, map.killPlaneY) { jumper = new HuskNavJumper(cfg, events) };
                health = new Health(100, events, "player", () => true); // invulnerable: no hurt payloads
                player = map.playerSpawn;
                yaw = map.playerSpawnYawDeg * System.Math.PI / 180;
                mover = new Mover(cfg.movement, world);
                mover.Reset(player.x, player.y, player.z);
                loadout = new Loadout(cfg, events, enemies, world, mover, health, Aim);
                assist = new AimAssist(cfg, events, enemies, world, Aim, () => loadout.Weapon, () => loadout.adsActive, () => "snap", () => 60);
                loadout.assist = assist;
                loadout.SetSlot(loadout.owned.IndexOf("gauntlet"));
                director = new Director(cfg, events, husks, brain, zones, core, world.Geometry, nav, () => player, () => yaw, () => 0) { huskWorld = world };
                director.NewRun();
                director.StartWave(1, 42);
            }

            (Vec3, Vec3) Aim()
            {
                double cp = JsMath.Cos(pitch);
                return (new Vec3(player.x, player.y + cfg.movement.body.eyeHeight, player.z), new Vec3(-JsMath.Sin(yaw) * cp, JsMath.Sin(pitch), -JsMath.Cos(yaw) * cp));
            }

            public int SpawnCrowd(int husksWanted, int swarmersWanted)
            {
                var spots = new List<SpawnNode>();
                var high = new List<SpawnNode>();
                zones.SpawnPoints(spots, high, core);
                int k = 0;
                for (int tries = 0; tries < 800 && (husks.CountOf("husk") < husksWanted || husks.CountOf("swarmer") < swarmersWanted); tries++)
                {
                    SpawnNode n = spots[(k++ * 7919) % spots.Count];
                    string kind = husks.CountOf("husk") < husksWanted ? "husk" : "swarmer";
                    Husk h = husks.Spawn(kind, new Vec3(n.x, n.y, n.z), player);
                    if (h != null) brain.Attach(h);
                }
                return husks.Count;
            }

            int turnIn;

            /// <summary>Every 30 steps the player turns to the nearest enemy, so the assist acquires, locks and pulls.</summary>
            void Turn()
            {
                if (--turnIn > 0) return;
                turnIn = 30;
                double best = double.PositiveInfinity;
                foreach (Enemy e in enemies.all)
                {
                    if (!e.alive) continue;
                    double dx = e.position.x - player.x, dz = e.position.z - player.z, d = dx * dx + dz * dz;
                    if (d < best && d > 1)
                    {
                        best = d;
                        yaw = System.Math.Atan2(-dx, -dz);
                    }
                }
            }

            public void Step()
            {
                Turn();
                var input = new CombatInput { weapon = -1 };
                brain.Step(dt, player, yaw, 1, false);
                assist.Step(dt, ref input, ref yaw, ref pitch);
                loadout.Step(input, dt);
                husks.Step(player, health, dt);
                brain.Separate(husks);
                director.Step(dt);
            }

            /// <summary>One step with each system's GC allocations counted on its own (a failure's breakdown).</summary>
            public void StepCounted(int[] counts)
            {
                Turn();
                var input = new CombatInput { weapon = -1 };
                counts[0] += Allocs(() => brain.Step(dt, player, yaw, 1, false));
                counts[1] += Allocs(() => assist.Step(dt, ref input, ref yaw, ref pitch));
                counts[2] += Allocs(() => loadout.Step(input, dt));
                counts[3] += Allocs(() => husks.Step(player, health, dt));
                counts[4] += Allocs(() => brain.Separate(husks));
                counts[5] += Allocs(() => director.Step(dt));
            }
        }

        // EXT53 B (the NIGHT33 red "HuskBrain.Step 1 allocation in 600 steps", AutopilotTests' flake since NIGHT14): ONE GC.Alloc recorder per test,
        // filtered to the test's thread once. Unity 6000.3's Recorder.Get makes a new wrapper on every call over the marker's shared native recorder
        // (UnityEngine.CoreModule IL: new ProfilerRecorder(handle, 1, 0x99 = StartImmediately | WrapAround | SumAllSamplesInFrame | SharedRecorder)),
        // started at once with no thread filter, and each wrapper's finalizer releases it from the finalizer thread; the old helper did that per
        // window (3,600 a run of SimSteps_With30Enemies). AutopilotTests.AllocWindows has the same summary and its check that the probe still sees one.
        static UnityEngine.Profiling.Recorder allocRec;

        [SetUp]
        public void AllocRecorderOn()
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers(); // earlier tests' wrappers released before the first window
            allocRec = UnityEngine.Profiling.Recorder.Get("GC.Alloc");
            allocRec.enabled = false;
            allocRec.FilterToCurrentThread();
            Allocs(() => { }); // one empty filtered window: what the unfiltered start caught is flushed here
        }

        [TearDown]
        public void AllocRecorderOff()
        {
            if (allocRec == null) return;
            allocRec.enabled = false;
            allocRec.CollectFromAllThreads(); // the shared recorder back to every thread (as the old helper left it)
            allocRec = null;
        }

        /// <summary>GC allocation samples made by `a` on this thread (the test's one recorder; AllocatingGCMemoryConstraint's logic otherwise).</summary>
        static int Allocs(System.Action a)
        {
            UnityEngine.Profiling.Recorder recorder = allocRec;
            if (recorder == null)
            {
                recorder = UnityEngine.Profiling.Recorder.Get("GC.Alloc");
                recorder.enabled = false;
                recorder.FilterToCurrentThread();
            }
            recorder.enabled = true;
            try
            {
                a();
            }
            finally
            {
                recorder.enabled = false;
                if (allocRec == null) recorder.CollectFromAllThreads();
            }
            return recorder.sampleBlockCount;
        }

        static readonly string[] Systems = { "HuskBrain.Step", "AimAssist.Step", "Loadout.Step", "Husks.Step", "HuskBrain.Separate", "Director.Step" };

        /// <summary>NIGHT10: Projectiles.Step with nails and rockets in flight, rockets ending in the air (a blast, its events).</summary>
        [Test]
        public void ProjectilesStep_InFlight_AllocatesNothing()
        {
            var events = new EventBus();
            var enemies = new EnemyRegistry();
            var world = new TriangleWorld();
            world.AddBox(new Vec3(-200, -1, -200), new Vec3(200, 0, 200));
            var mover = new Mover(cfg.movement, world);
            mover.Reset(0, 0, 0);
            var loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (new Vec3(0, 1.6, 0), new Vec3(0, 0, -1)));
            var projectiles = new Projectiles(cfg, events, world, enemies, 5);
            events.On("rocketBlast", _ => { });
            events.On("shotResult", _ => { });
            Vec3 up = Vec3.Normalize(new Vec3(0, 0.4, -1));
            double step = cfg.Dt;
            // Warm-up (NIGHT11: a whole first volley, so the JIT has compiled the blast path before the count: right after a
            // domain reload, a 2-step warm-up let the first blast's JIT show as 6 samples).
            for (int i = 0; i < 20; i++) projectiles.Fire("nail", loadout, loadout, new Vec3(0, 1.6, 0), up);
            for (int i = 0; i < 4; i++) projectiles.Fire("rocket", loadout, loadout, new Vec3(i, 1.6, 0), up);
            for (int i = 0; i < 600; i++) projectiles.Step(step);
            for (int i = 0; i < 20; i++) projectiles.Fire("nail", loadout, loadout, new Vec3(0, 1.6, 0), up);
            for (int i = 0; i < 4; i++) projectiles.Fire("rocket", loadout, loadout, new Vec3(i, 1.6, 0), up);
            projectiles.Step(step);
            projectiles.Step(step);
            int allocs = Allocs(() =>
            {
                for (int i = 0; i < 600; i++) projectiles.Step(step); // the nails' life ends, then the rockets' (each blasts)
            });
            Assert.That(projectiles.Count("rocket"), Is.EqualTo(0), "every rocket ran out its lifetime and blasted");
            Assert.That(allocs, Is.EqualTo(0), "GC allocation samples over 600 Projectiles.Step");
        }

        [Test]
        public void SimSteps_With30Enemies_AllocateNothing()
        {
            var sim = new Sim();
            int alive = sim.SpawnCrowd(24, 6);
            Assert.That(alive, Is.GreaterThanOrEqualTo(30), "30 enemies placed");
            int n = 480;
            TestDelegate run = () =>
            {
                for (int i = 0; i < n; i++)
                {
                    sim.director.state.lingerTime = 0; // the player stands still here: keep the camp timer from firing campStart (a boxed double)
                    sim.Step();
                }
            };
            run(); // warm-up: pools, lists, boxed ids, the first paths, the JIT
            Assert.That(sim.husks.Count, Is.GreaterThanOrEqualTo(24), "the crowd is still there");
            // Per system first (the breakdown a failure needs), then the whole step under the constraint.
            var counts = new int[Systems.Length];
            for (int i = 0; i < 2; i++) sim.StepCounted(counts); // warm the counted path's own lambdas
            System.Array.Clear(counts, 0, counts.Length);
            for (int i = 0; i < 600; i++)
            {
                sim.director.state.lingerTime = 0;
                sim.StepCounted(counts);
            }
            var breakdown = new List<string>();
            for (int i = 0; i < Systems.Length; i++) breakdown.Add($"{Systems[i]} {counts[i]}");
            string stats = $"alive {sim.husks.Count}; scans {sim.assist.scans}, locks {sim.assist.locks}; repaths {sim.brain.repaths}, link jumps {sim.brain.linkJumps}, hops {sim.brain.hops}, relocations {sim.brain.relocations}";
            Assert.That(counts, Has.All.EqualTo(0), "GC allocations over 600 steps: " + string.Join(", ", breakdown) + "; " + stats);
            Assert.That(sim.assist.locks, Is.GreaterThan(0), "the assist locked at least once (its pull ran): " + stats);
            n = 600;
            Assert.That(run, AllocIs.Not.AllocatingGCMemory());
            TestContext.WriteLine($"alive {sim.husks.Count}; scans {sim.assist.scans}, locks {sim.assist.locks}; repaths {sim.brain.repaths}, link jumps {sim.brain.linkJumps}, hops {sim.brain.hops}; windups {(sim.events.Emitted.TryGetValue("huskWindup", out int w) ? w : 0)}");
        }

        /// <summary>
        /// NIGHT11 U3-24.2: the Development Player's profile found SKYGRIND.Loadout allocating about 4 times a step with the
        /// KESTREL in hand (the bench's player; the test above holds the gauntlet). Loadout.Step idle with each ported gun
        /// in hand, on the same crowd, allocates nothing.
        /// </summary>
        [Test]
        public void LoadoutStep_EachGunInHand_Idle_AllocatesNothing()
        {
            var sim = new Sim();
            sim.SpawnCrowd(24, 6);
            var breakdown = new List<string>();
            int total = 0;
            foreach (string id in new[] { "revolver", "shotgun", "nailgun", "rocket", "rail", "gauntlet" })
            {
                if (!sim.loadout.owned.Contains(id)) sim.loadout.AddWeapon(id);
                sim.loadout.SetSlot(sim.loadout.owned.IndexOf(id));
                var input = new CombatInput { weapon = -1 };
                for (int i = 0; i < 240; i++) sim.loadout.Step(input, cfg.Dt); // warm-up: the swap, the JIT, the caches
                Assert.That(sim.loadout.Weapon, Is.EqualTo(id), "in hand");
                int n = Allocs(() =>
                {
                    for (int i = 0; i < 300; i++) sim.loadout.Step(input, cfg.Dt);
                });
                breakdown.Add($"{id} {n}");
                total += n;
            }
            Assert.That(total, Is.EqualTo(0), "GC allocation samples over 300 idle Loadout.Step per gun: " + string.Join(", ", breakdown));
        }

        [Test]
        public void EventBus_RemovalMidEmit_IsDeferred_AdditionWaitsForTheNextEmit()
        {
            var bus = new EventBus();
            var log = new List<string>();
            System.Action offB = null;
            bus.On("x", _ =>
            {
                log.Add("a");
                offB();
                bus.On("x", __ => log.Add("c"));
            });
            offB = bus.On("x", _ => log.Add("b"));
            bus.Emit("x");
            Assert.That(string.Join(",", log), Is.EqualTo("a,b"), "b still runs in the emit that removed it; c waits");
            log.Clear();
            bus.Emit("x");
            Assert.That(string.Join(",", log), Is.EqualTo("a,c"), "b is gone; c runs (and a adds another c for next time)");
        }

        [Test]
        public void EventBus_EmitWithAnIntPayload_AllocatesNothing()
        {
            var bus = new EventBus();
            int sum = 0;
            bus.On("id", o => sum += (int)o);
            int n = 1;
            TestDelegate emit = () =>
            {
                for (int i = 0; i < n; i++) bus.Emit("id", 7);
            };
            emit(); // warm: the first emit of a name and value may allocate (the counter's entry, the box), and the JIT
            n = 100;
            Assert.That(emit, AllocIs.Not.AllocatingGCMemory());
            Assert.That(sum, Is.EqualTo(707));
        }

        /// <summary>
        /// NIGHT16: the impact layer's rules (Core/ImpactRules.cs) run every frame (BeginFrame) and after every step (EndStep)
        /// of the Game's loop, and hear the sim's bus (hit, kill, the sim's own hitstop): none of it allocates (PORT-RULES
        /// section 6; a horde's kills inside the budget, cut and skipped requests included).
        /// </summary>
        [Test]
        public void ImpactRules_FramesStepsAndMoments_AllocateNothing()
        {
            var bus = new EventBus();
            var rules = new ImpactRules(cfg);
            rules.Watch(bus);
            var hit = new HitEvent { owner = "player", weapon = "revolver", kind = "husk", headshot = true, killed = true, shot = 1, damage = 30, distance = 5 };
            var kill = new KillEvent { owner = "player", weapon = "revolver", kind = "husk", headshot = true };
            int n = 1, shot = 1;
            TestDelegate run = () =>
            {
                for (int i = 0; i < n; i++)
                {
                    rules.BeginFrame(1.0 / 144, false);
                    hit.shot = ++shot;
                    bus.Emit("hit", hit);
                    bus.Emit("kill", kill);
                    bus.Emit("hitstop", "kill");
                    rules.EndStep(false, 0.04);
                }
            };
            run(); // warm: the JIT and the bus's first emit of each name
            n = 600;
            Assert.That(run, AllocIs.Not.AllocatingGCMemory());
        }
    }
}
