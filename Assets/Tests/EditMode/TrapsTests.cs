// NIGHT11 U3-24.5: traps.js (Core/Traps.cs) on the real Survival map and config, with the real loadout, economy and
// style meter. traps.js has no vitest; these follow its debug fun script (src/debug/funScripts.js :119-195, the traps
// check): the plate bought at its switch kills the husks standing on it, pays TRAPPED, then can't be bought until its
// cooldown has run out; the crusher kills on the drop, the fan throws then kills after killAfter, the pylon zaps the
// nearest `targets` every `every` s; an elite (a spitter) takes spec.elite instead of traps.kill; the switch is for sale
// only in an open zone and, with the port-only `powered` hook (NIGHT11 design.power), only while powered. Steps come
// from config through stepsFor. Traps.Step allocates nothing.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class TrapsTests
    {
        static GameConfig cfg;
        static SurvivalMap map;
        static double dt;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
            dt = cfg.Dt;
        }

        static int S(double seconds) => MovementMath.StepsFor(seconds, dt);

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly GameWorld world;
            public readonly Zones zones;
            public readonly Economy economy;
            public readonly Buyables buyables;
            public readonly Husks husks;
            public readonly Mover mover;
            public readonly Health health;
            public readonly Loadout loadout;
            public readonly StyleMeter style;
            public readonly Traps traps;
            public readonly List<string> log = new List<string>();
            public readonly List<(Vec3 from, Vec3 to, uint hex)> zaps = new List<(Vec3, Vec3, uint)>();
            public readonly List<(Vec3 at, uint hex)> dust = new List<(Vec3, uint)>();
            public int trapped; // TRAPPED style awards paid by the meter

            public Rig(Func<Enemy, double, Vec3, bool> damage = null, bool logEvents = true)
            {
                world = new GameWorld(map.BuildOctree(), map.CreateCore(cfg.movement));
                zones = new Zones(cfg, map, world, events);
                economy = new Economy(cfg, events);
                economy.Start(5000);
                buyables = new Buyables(cfg, events, economy);
                husks = new Husks(cfg, events, enemies, world, map.killPlaneY);
                mover = new Mover(cfg.movement, world);
                mover.Reset(map.playerSpawn.x, map.playerSpawn.y, map.playerSpawn.z);
                health = new Health(100, events, "player", () => true);
                loadout = new Loadout(cfg, events, enemies, world, mover, health, () => (new Vec3(0, 1.6, 0), new Vec3(0, 0, -1)));
                style = new StyleMeter(cfg, events, () => "GROUND", () => false);
                style.Start();
                traps = new Traps(cfg, events, buyables, zones.ZoneAt, Traps.IsOpenOf(zones, map), enemies, damage ?? Traps.DamageBy(loadout), null, economy);
                traps.burst = (at, hex, count, speed, life, size) => dust.Add((at, hex));
                traps.arcs = a =>
                {
                    Assert.That((a.intensity, a.life, a.jag, a.width), Is.EqualTo((3.0, 0.16, 0.14, 0.07)), "traps.js :149's zap");
                    zaps.Add((a.from, a.to, a.hex));
                };
                traps.Load(map.trapSpots);
                if (!logEvents) return; // the allocation test: no handler that builds strings
                events.On<TrapEvent>("trap", e => log.Add($"trap {e.id} {(e.on ? "on" : "off")}"));
                events.On<TrapKillEvent>("trapKill", e => log.Add($"trapKill {e.id} {e.kind}"));
                events.On<TrapSlamEvent>("trapSlam", e => log.Add($"trapSlam {e.id}"));
                events.On<StyleBonusEvent>("styleBonus", e => log.Add($"styleBonus {e.owner} {e.name} {e.points} {e.weapon}"));
                events.On<StyleAwardEvent>("styleAward", e =>
                {
                    if (e.name == "TRAPPED" && e.points > 0) trapped++;
                });
            }

            /// <summary>funScripts.js put(): n enemies of `kind` on a circle of `r` m round the trap's centre.</summary>
            public List<Husk> Put(Traps.Trap t, int n, string kind = "husk", double r = 0.9)
            {
                var list = new List<Husk>();
                for (int i = 0; i < n; i++)
                {
                    double a = (double)i / n * Math.PI * 2;
                    Husk h = husks.Spawn(kind, new Vec3(t.spot.pos.x + Math.Cos(a) * r, t.spot.pos.y, t.spot.pos.z + Math.Sin(a) * r), t.Switch);
                    Assert.That(h, Is.Not.Null, $"a {kind} fits on the {t.id}");
                    list.Add(h);
                }
                return list;
            }

            public void Steps(int n)
            {
                for (int i = 0; i < n; i++) traps.Step(dt);
            }

            public int Count(string prefix) => log.FindAll(l => l.StartsWith(prefix)).Count;
        }

        static int Alive(List<Husk> list) => list.FindAll(h => h.alive).Count;

        [Test]
        public void Load_PlacesTheFourSwitches_AndClearsThem()
        {
            var r = new Rig();
            Assert.That(r.traps.items.ConvertAll(t => t.id), Is.EquivalentTo(new[] { "plate", "crusher", "fan", "pylon" }));
            Assert.That(r.buyables.list.ConvertAll(b => b.id), Is.EquivalentTo(new[] { "trap:plate", "trap:crusher", "trap:fan", "trap:pylon" }));
            foreach (Traps.Trap t in r.traps.items)
            {
                Assert.That(t.buyable.kind, Is.EqualTo("trap"));
                Assert.That(t.buyable.name, Is.EqualTo(cfg.Str($"traps.kinds.{t.id}.name")));
                Assert.That(t.buyable.price(), Is.EqualTo(cfg.Num($"traps.kinds.{t.id}.price")));
                Assert.That(t.buyable.reach, Is.EqualTo(cfg.Num("perks.reach")));
                Assert.That(t.zone, Is.EqualTo(r.zones.ZoneAt(new Vec3(t.spot.sx, t.spot.pos.y + 1, t.spot.sz))));
            }
            Assert.That(r.traps.Get("plate").zone, Is.EqualTo(map.startZone), "the plate's switch stands in LANDING");
            r.traps.Load(map.trapSpots); // a new run: the old switches go first
            Assert.That(r.buyables.list.Count, Is.EqualTo(4));
            r.traps.Load(null);
            Assert.That(r.traps.items.Count, Is.EqualTo(0));
            Assert.That(r.buyables.list.Count, Is.EqualTo(0));
        }

        [Test]
        public void Plate_BoughtAtItsSwitch_KillsWhatStandsOnIt_PaysTrapped_ThenCoolsDown()
        {
            var r = new Rig();
            Traps.Trap plate = r.traps.Get("plate");
            TrapSpec spec = plate.spec;
            List<Husk> onPlate = r.Put(plate, 3);
            // funScripts.js: stand 1.4 m short of the switch, facing it (+x: yaw -90 deg), hold BUY.
            var feet = new Vec3(plate.spot.sx - 1.4, plate.spot.pos.y, plate.spot.sz);
            double yaw = -Math.PI / 2;
            double before = r.economy.total;
            int hold = S(cfg.Num("buy.holdTime"));
            for (int i = 0; i < hold; i++)
            {
                r.buyables.Step(feet, yaw, true, dt);
                r.traps.Step(dt);
            }
            Assert.That(plate.On, Is.True, "bought after buy.holdTime");
            Assert.That(r.economy.total, Is.EqualTo(before - spec.price));
            Assert.That(r.log, Does.Contain("trap plate on"));
            Assert.That(r.traps.stats.Fired("plate"), Is.EqualTo(1));
            Assert.That(plate.tick, Is.EqualTo(1));
            r.buyables.Step(feet, yaw, false, dt); // let go (not a fixed step of the traps)
            r.Steps(S(spec.every) - 2);
            Assert.That(Alive(onPlate), Is.EqualTo(3), "nothing before the first shock");
            r.Steps(1);
            Assert.That(Alive(onPlate), Is.EqualTo(0), "the first shock (every s) kills every husk standing on it (traps.kill)");
            Assert.That(r.Count("trapKill plate husk"), Is.EqualTo(3));
            Assert.That(r.Count($"styleBonus player TRAPPED {cfg.Num("style.bonus.trapped")} trap"), Is.EqualTo(3));
            Assert.That(r.trapped, Is.EqualTo(3), "the style meter pays TRAPPED for each");
            Assert.That(r.traps.stats.kills, Is.EqualTo(3));
            Assert.That(r.economy.total, Is.GreaterThan(before - spec.price), "the kills pay their points");
            // It runs out after `time`, then cools down for `cooldown` (no buy meanwhile).
            r.Steps(spec.timeSteps - S(spec.every));
            Assert.That(plate.On, Is.False);
            Assert.That(plate.cooldown, Is.EqualTo(S(spec.cooldown)));
            Assert.That(r.log, Does.Contain("trap plate off"));
            Assert.That(plate.buyable.available(), Is.False, "cooling down");
            double bank = r.economy.total;
            for (int i = 0; i < hold * 2; i++)
            {
                r.buyables.Step(feet, yaw, true, dt);
                r.traps.Step(dt);
            }
            r.buyables.Step(feet, yaw, false, dt);
            Assert.That(plate.On, Is.False, "no second buy while it cools down");
            Assert.That(r.economy.total, Is.EqualTo(bank));
            r.Steps(plate.cooldown - 1);
            Assert.That(plate.buyable.available(), Is.False);
            r.Steps(1);
            Assert.That(plate.cooldown, Is.EqualTo(0));
            Assert.That(plate.buyable.available(), Is.True, "for sale again once the cooldown has run out");
            for (int i = 0; i < hold; i++)
            {
                r.buyables.Step(feet, yaw, true, dt);
                r.traps.Step(dt);
            }
            Assert.That(plate.On, Is.True, "bought again");
            Assert.That(r.economy.total, Is.EqualTo(bank - spec.price));
        }

        [Test]
        public void Plate_AnEliteTakesSpecElite_NotTheKill()
        {
            var r = new Rig();
            Traps.Trap plate = r.traps.Get("plate");
            Husk spitter = r.Put(plate, 1, "spitter")[0];
            Assert.That(cfg.Strs("powerUps.eliteKinds"), Does.Contain("spitter"));
            double hp = spitter.hp;
            Assert.That(r.traps.DebugFire("plate"), Is.True);
            r.Steps(S(plate.spec.every));
            Assert.That(spitter.alive, Is.True, "an elite is hurt, not killed outright");
            Assert.That(spitter.hp, Is.EqualTo(hp - plate.spec.elite), "spec.elite, not traps.kill");
            Assert.That(r.traps.stats.eliteHits, Is.EqualTo(1));
            Assert.That(r.Count("trapKill"), Is.EqualTo(0));
            // Shock after shock, it goes down: a trap kill like any other.
            int shocks = (int)Math.Ceiling(hp / plate.spec.elite);
            r.Steps(S(plate.spec.every) * (shocks - 1));
            Assert.That(spitter.alive, Is.False);
            Assert.That(r.Count("trapKill plate spitter"), Is.EqualTo(1));
            Assert.That(r.traps.stats.eliteHits, Is.EqualTo(shocks));
        }

        [Test]
        public void Crusher_KillsOnTheDrop_DustAndSlam()
        {
            var r = new Rig();
            Traps.Trap crusher = r.traps.Get("crusher");
            TrapSpec spec = crusher.spec;
            List<Husk> under = r.Put(crusher, 3);
            r.traps.DebugFire("crusher");
            r.Steps(S(spec.drop) - 1);
            Assert.That(Alive(under), Is.EqualTo(3), "the slab is still falling");
            Assert.That(r.traps.SlabDown(crusher), Is.LessThan(1));
            r.Steps(1);
            Assert.That(Alive(under), Is.EqualTo(0), "the drop crushes everything under it");
            Assert.That(r.Count("trapSlam crusher"), Is.EqualTo(1));
            Assert.That(r.dust.Count, Is.EqualTo(1));
            Assert.That(r.dust[0].at, Is.EqualTo(new Vec3(crusher.spot.pos.x, crusher.spot.pos.y + 0.3, crusher.spot.pos.z)));
            Assert.That(r.dust[0].hex, Is.EqualTo((uint)cfg.Num("colors.slamDust")));
            r.Steps(1); // the hit lands on step S(drop) (0.1167 s); the drawn slab reaches the bottom at drop s
            Assert.That(r.traps.SlabDown(crusher), Is.EqualTo(1), "down for the hold");
            r.Steps(S(spec.every) - 1);
            Assert.That(r.Count("trapSlam crusher"), Is.EqualTo(2), "it drops again every `every` s");
            Assert.That(r.Count("trapKill crusher husk"), Is.EqualTo(3));
        }

        [Test]
        public void Fan_ThrowsWhatWalksOverIt_ThenKillsItAfterKillAfter()
        {
            var r = new Rig();
            Traps.Trap fan = r.traps.Get("fan");
            TrapSpec spec = fan.spec;
            List<Husk> over = r.Put(fan, 3);
            r.traps.DebugFire("fan");
            r.Steps(1);
            foreach (Husk h in over)
            {
                Assert.That(h.velocity.y, Is.EqualTo(spec.launch), "thrown up at spec.launch (enemies.launch)");
                Assert.That(h.grounded, Is.False);
                Assert.That(h.alive, Is.True);
            }
            Assert.That(r.traps.stats.launched, Is.EqualTo(3));
            Assert.That(fan.Thrown, Is.EqualTo(over), "thrown in the order they were met");
            Assert.That(fan.PendingKills.Count, Is.EqualTo(3));
            r.Steps(1);
            Assert.That(r.traps.stats.launched, Is.EqualTo(3), "each is thrown once");
            r.Steps(S(spec.killAfter) - 3);
            Assert.That(Alive(over), Is.EqualTo(3), "not before killAfter");
            r.Steps(1);
            Assert.That(Alive(over), Is.EqualTo(0), "killAfter s after the throw");
            Assert.That(r.Count("trapKill fan husk"), Is.EqualTo(3));
            // The thrown ones fly (the husk's own move, under gravity).
            var r2 = new Rig();
            Traps.Trap fan2 = r2.traps.Get("fan");
            Husk flyer = r2.Put(fan2, 1)[0];
            double y0 = flyer.position.y;
            r2.traps.DebugFire("fan");
            for (int i = 0; i < 20; i++)
            {
                r2.traps.Step(dt);
                r2.husks.Step(fan2.Switch, r2.health, dt);
            }
            Assert.That(flyer.position.y, Is.GreaterThan(y0 + 1), "up into the air");
        }

        [Test]
        public void Fan_APendingKill_WaitsForTheNextRun()
        {
            var r = new Rig();
            Traps.Trap fan = r.traps.Get("fan");
            TrapSpec spec = fan.spec;
            r.traps.DebugFire("fan");
            r.Steps(spec.timeSteps - 10);
            Husk late = r.Put(fan, 1)[0]; // walks over it 10 steps before the end
            r.Steps(1);
            Assert.That(fan.PendingKills.Count, Is.EqualTo(1));
            r.Steps(9);
            Assert.That(fan.On, Is.False);
            Assert.That(fan.Thrown.Count, Is.EqualTo(0), "the thrown set clears at the end");
            Assert.That(fan.PendingKills.Count, Is.EqualTo(1), "the pending kill stays (traps.js never clears it)");
            int left = fan.PendingKills[0].steps;
            Assert.That(left, Is.EqualTo(spec.killAfterSteps - 10));
            r.Steps(left * 2);
            Assert.That(late.alive, Is.True, "nothing counts down while the fan is off");
            r.traps.DebugFire("fan");
            r.Steps(left - 1);
            Assert.That(late.alive, Is.True);
            r.Steps(1);
            Assert.That(late.alive, Is.False, "it resumes with the next run");
        }

        [Test]
        public void Pylon_ZapsTheNearestTargets_EveryEvery()
        {
            var r = new Rig();
            Traps.Trap pylon = r.traps.Get("pylon");
            TrapSpec spec = pylon.spec;
            Assert.That(spec.targets, Is.EqualTo(4));
            // Five husks toward the switch, the farthest first in the pool (the zaps go nearest first).
            var line = new List<Husk>();
            double sz = Math.Sign(pylon.spot.sz - pylon.spot.pos.z);
            foreach (double d in new[] { 5.5, 1.5, 4.5, 2.5, 3.5 })
            {
                Husk h = r.husks.Spawn("husk", new Vec3(pylon.spot.pos.x, pylon.spot.pos.y, pylon.spot.pos.z + sz * d), pylon.Switch);
                Assert.That(h, Is.Not.Null);
                line.Add(h);
            }
            r.traps.DebugFire("pylon");
            r.Steps(S(spec.every) - 1);
            Assert.That(Alive(line), Is.EqualTo(5));
            Assert.That(r.zaps.Count, Is.EqualTo(0));
            r.Steps(1);
            Assert.That(r.zaps.Count, Is.EqualTo(4), "up to `targets` per zap");
            Husk[] nearest = { line[1], line[3], line[4], line[2] };
            var top = new Vec3(pylon.spot.pos.x, pylon.spot.pos.y + spec.height + 0.3, pylon.spot.pos.z);
            for (int i = 0; i < 4; i++)
            {
                Assert.That(nearest[i].alive, Is.False);
                Assert.That(r.zaps[i].from, Is.EqualTo(top), "from the orb");
                Assert.That(r.zaps[i].to, Is.EqualTo(nearest[i].AimPoint), "to the target, nearest first");
                Assert.That(r.zaps[i].hex, Is.EqualTo(spec.color));
            }
            Assert.That(line[0].alive, Is.True, "the fifth waits for the next zap");
            r.Steps(S(spec.every));
            Assert.That(line[0].alive, Is.False);
            Assert.That(r.zaps.Count, Is.EqualTo(5));
        }

        [Test]
        public void Switch_ForSaleOnlyInAnOpenZone_AndWhilePowered()
        {
            var r = new Rig();
            Traps.Trap plate = r.traps.Get("plate");
            Assert.That(plate.buyable.available(), Is.True, "LANDING is open, the plate is ready");
            r.traps.powered = () => false;
            Assert.That(plate.buyable.available(), Is.False, "NIGHT11 design.power: no power, no trap");
            r.traps.powered = () => true;
            Assert.That(plate.buyable.available(), Is.True);
            r.traps.powered = null;
            Assert.That(plate.buyable.available(), Is.True, "null: always powered (the web's rule)");
            Traps.Trap crusher = r.traps.Get("crusher");
            Assert.That(crusher.zone, Is.Not.EqualTo(map.startZone));
            Assert.That(r.zones.IsOpen(crusher.zone), Is.False);
            Assert.That(crusher.buyable.available(), Is.False, "its zone is locked");
            // Standing at the locked switch with the points: no buy.
            var feet = new Vec3(crusher.spot.sx, crusher.spot.pos.y, crusher.spot.sz);
            double yaw = Math.Atan2(-(crusher.spot.pos.x - crusher.spot.sx), -(crusher.spot.pos.z - crusher.spot.sz)); // facing the trap
            for (int i = 0; i < S(cfg.Num("buy.holdTime")) * 2; i++) r.buyables.Step(feet, yaw, true, dt);
            Assert.That(crusher.On, Is.False);
            Assert.That(r.zones.Open(crusher.zone), Is.True);
            Assert.That(crusher.buyable.available(), Is.True, "for sale once its zone is open");
            r.traps.powered = () => false;
            Assert.That(crusher.buyable.available(), Is.False);
        }

        [Test]
        public void Step_WithEveryTrapRunning_AllocatesNothing()
        {
            int hits = 0, offs = 0, slams = 0;
            var r = new Rig((e, amount, from) =>
            {
                hits++;
                return false; // nothing dies: the traps' own path only (a kill's hit / kill payloads are the loadout's)
            }, logEvents: false);
            r.events.On("trap", _ => offs++);
            r.events.On("trapSlam", _ => slams++);
            foreach (Traps.Trap t in r.traps.items) r.Put(t, 3);
            r.traps.burst = null;
            r.traps.arcs = null;
            foreach (Traps.Trap t in r.traps.items) r.traps.DebugFire(t.id);
            r.Steps(130); // warm-up: the fan's thrown set and pending list, the pylon's lists, the JIT
            int hitsBefore = hits;
            int allocs = Allocs(() =>
            {
                for (int i = 0; i < 600; i++) r.traps.Step(dt);
            });
            Assert.That(hits, Is.GreaterThan(hitsBefore), "the traps hurt inside the window");
            Assert.That(slams, Is.GreaterThan(0));
            Assert.That(offs, Is.GreaterThan(0), "a trap ran out inside the window (the crusher's 4.8 s)");
            Assert.That(allocs, Is.EqualTo(0), "GC allocations over 600 Traps.Step");
        }

#if UNITY_2017_1_OR_NEWER
        /// <summary>GC allocation samples made by `a` on this thread (ZeroAllocTests.Allocs).</summary>
        static int Allocs(Action a)
        {
            var recorder = UnityEngine.Profiling.Recorder.Get("GC.Alloc");
            recorder.enabled = false;
            recorder.FilterToCurrentThread();
            recorder.enabled = true;
            try
            {
                a();
            }
            finally
            {
                recorder.enabled = false;
                recorder.CollectFromAllThreads();
            }
            return recorder.sampleBlockCount;
        }
#else
        /// <summary>Bytes allocated by `a` on this thread (outside Unity: the scratch .NET run).</summary>
        static int Allocs(Action a)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();
            a();
            return (int)(GC.GetAllocatedBytesForCurrentThread() - before);
        }
#endif
    }
}
