// NIGHT11 U3-24.3: objective rounds (Core/Objectives.cs, survival/objectives.js; DirectorMath.RoundType,
// ai/directorMath.js roundType). The web's tests (tests/night6.test.js, 'Objective rounds (B7)': the schedule, the hold
// spots clear of every box), then the port's: a HOLD round can't clear until it is held (SurvivalRun's holdRound and
// step hooks, wired as main.js wires them), its bonus is paid once, combat points pay x pointsMult inside the ring and x1
// outside; GOLD RUSH doubles kills and pays a chain of chainAt kills within chainWindow s, none past the window; a HOLD
// round with no open spot is 'normal'; roundClear ends the objective; Step allocates nothing.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints; // AllocatingGCMemory() is an extension method here
using AllocIs = UnityEngine.TestTools.Constraints.Is;
using Is = NUnit.Framework.Is;

namespace Skygrind.Core.Tests
{
    public class ObjectivesTests
    {
        static GameConfig cfg;
        static SurvivalMap map;
        static DirectorMath.D r;
        static double dt;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
            r = DirectorMath.From(cfg);
            dt = cfg.Dt;
        }

        // ---- night6.test.js :208-235 ----

        [Test]
        public void HoldAndGoldRush_OnTheirRounds_NeverABossOrSwarmRound_NoneBeforeObjectiveFrom()
        {
            var kinds = new Dictionary<int, string>();
            for (int i = 0; i < 30; i++) kinds[i + 1] = DirectorMath.RoundType(i + 1, r);
            Assert.That(kinds[6], Is.EqualTo("hold"));
            Assert.That(kinds[7], Is.EqualTo("gold"));
            Assert.That(kinds[5], Is.EqualTo("boss"));
            Assert.That(kinds[8], Is.EqualTo("swarm"));
            Assert.That(kinds[10], Is.EqualTo("boss")); // 10 % 4 = 2, but a boss round
            Assert.That(kinds[18], Is.EqualTo("swarm")); // 18 % 4 = 2, but a swarm round
            for (int n = 1; n < r.objectiveFrom; n++) Assert.That(new[] { "hold", "gold" }, Has.No.Member(kinds[n]), $"round {n}");
            int objective = 0;
            foreach (string k in kinds.Values) if (k == "hold" || k == "gold") objective++;
            Assert.That(objective, Is.GreaterThanOrEqualTo(6)); // about one round in four from round 6
        }

        [Test]
        public void HoldSpots_StandClearOfEveryBoxOnTheSurvivalMap()
        {
            // a 4 m ring on open floor: no box below 2 m and above 0.1 m (cover excepted) within objectives.radius of a spot
            double radius = cfg.Num("objectives.radius");
            Assert.That(map.holdSpots, Is.Not.Empty);
            foreach (Vec3 s in map.holdSpots)
            {
                var blocking = new List<string>();
                foreach (MapBox b in map.boxes)
                {
                    double ox = Math.Max(Math.Max(b.min.x - s.x, 0), s.x - b.max.x), oz = Math.Max(Math.Max(b.min.z - s.z, 0), s.z - b.max.z);
                    if (b.min.y < 2 && b.max.y > 0.1 && b.kind != "cover" && ox * ox + oz * oz < radius * radius) blocking.Add(b.kind);
                }
                Assert.That(blocking, Is.Empty, $"{s.x},{s.z}");
            }
        }

        // ---- the port's ----

        [Test]
        public void RoundType_ObjectiveFromZeroIsFalsy_NoObjectiveRounds()
        {
            DirectorMath.D off = DirectorMath.From(cfg);
            off.objectiveFrom = 0; // `r.objectiveFrom &&`: 0 turns the objective rounds off
            for (int n = 1; n <= 30; n++) Assert.That(new[] { "normal", "boss", "swarm" }, Has.Member(DirectorMath.RoundType(n, off)), $"round {n}");
            off.objectiveFrom = double.NaN; // undefined in the web's config
            Assert.That(DirectorMath.RoundType(6, off), Is.EqualTo("normal"));
        }

        sealed class FakeSpawner : IRoundSpawner
        {
            public Action<int> onStart; // main.js onRoundBegin: director.startRound(round), then objectives.begin(round)
            public void StartWave(int round, uint seed) => onStart?.Invoke(round);
            public void Step(double dt) { }
            public bool Done => true; // every enemy dead at once: only the HOLD keeps the round open
            public void Clear() { }
        }

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly GameWorld world;
            public readonly Zones zones;
            public readonly Economy economy;
            public readonly Objectives objectives;
            public readonly SurvivalRun run;
            public readonly FakeSpawner spawner = new FakeSpawner();
            public Vec3 player;
            public readonly List<string> log = new List<string>();
            public readonly List<ObjectiveDoneEvent> done = new List<ObjectiveDoneEvent>();
            public readonly List<string> banners = new List<string>();

            public Rig()
            {
                world = new GameWorld(map.BuildOctree(), map.CreateCore(cfg.movement));
                zones = new Zones(cfg, map, world, events);
                economy = new Economy(cfg, events);
                var health = new Health(1e9, events, "player");
                var regen = new Regen(cfg, events, health, () => 0);
                objectives = new Objectives(cfg, events, economy, zones, () => player);
                run = new SurvivalRun(cfg, events, spawner, economy, regen)
                {
                    holdRound = () => objectives.Holding, // main.js survivalHooks.holdRound
                    stepHook = _ => objectives.Step(), // main.js survivalHooks.step
                };
                spawner.onStart = n => objectives.Begin(n);
                events.On<ObjectiveStartEvent>("objectiveStart", e => log.Add($"start {e.type} {e.round}"));
                events.On<ObjectiveDoneEvent>("objectiveDone", e => done.Add(e));
                events.On<BannerEvent>("banner", e => banners.Add(e.text));
                events.On<RoundEvent>("roundClear", e => log.Add($"clear {e.number}"));
            }

            /// <summary>A run at round `n`'s fight (the debug skip, then its break).</summary>
            public void FightAt(int n)
            {
                run.Start(7);
                economy.Start(0);
                run.DebugSkipTo(n);
                for (int i = 0; i < 100000 && run.state != SurvivalRun.State.Fight; i++) run.Step(dt);
                Assert.That(run.state, Is.EqualTo(SurvivalRun.State.Fight));
                Assert.That(run.round, Is.EqualTo(n));
            }

            public void Steps(int k)
            {
                for (int i = 0; i < k; i++) run.Step(dt);
            }
        }

        [Test]
        public void AHoldRound_CantClearUntilHeld_TheBonusOnce_PointsX15InsideX1Outside()
        {
            var g = new Rig();
            g.zones.OpenAll();
            g.objectives.Load(map.holdSpots);
            g.FightAt(6);
            Assert.That(g.log, Is.EqualTo(new[] { "start hold 6" }));
            Assert.That(g.banners, Is.EqualTo(new[] { "HOLD ROUND · STAND IN THE RING FOR 30 S" }));
            Assert.That(g.objectives.Holding, Is.True);
            Vec3 spot = g.objectives.Spot.Value;
            Assert.That(spot, Is.EqualTo(map.holdSpots[(6 * 7) % map.holdSpots.Count]), "open[(n * 7) % open.length], every spot open");
            Assert.That(g.objectives.RingVisible, Is.True);
            int need = (int)JsMath.Round(cfg.Num("objectives.holdTime") * cfg.movement.loop.simHz);
            Assert.That(g.objectives.Need, Is.EqualTo(need));
            Assert.That(g.objectives.HudOn && g.objectives.HudType == "hold" && g.objectives.HudLeft == 30, Is.True);

            // Outside the ring: the enemies are all dead, yet the round stays open; points x1.
            g.player = new Vec3(spot.x + cfg.Num("objectives.radius") + 0.5, spot.y, spot.z);
            g.Steps(600);
            Assert.That(g.run.state, Is.EqualTo(SurvivalRun.State.Fight), "a HOLD round can't clear until held");
            Assert.That(g.objectives.Held, Is.EqualTo(0));
            Assert.That(g.economy.boost, Is.EqualTo(1));
            Assert.That(g.objectives.HudInside, Is.False);
            // In it, but 2.5 m above: still outside.
            g.player = new Vec3(spot.x, spot.y + 2.5, spot.z);
            g.Steps(1);
            Assert.That(g.objectives.Inside, Is.False);

            // Inside: points x pointsMult (a kill pays points.kill x 1.5).
            g.player = new Vec3(spot.x + 1, spot.y, spot.z - 1);
            g.Steps(1);
            Assert.That(g.objectives.Inside && g.objectives.HudInside, Is.True);
            Assert.That(g.economy.boost, Is.EqualTo(cfg.Num("objectives.pointsMult")));
            double before = g.economy.total;
            g.events.Emit("hit", new HitEvent { weapon = "revolver", killed = true });
            Assert.That(g.economy.total - before, Is.EqualTo(JsMath.Round(cfg.Num("points.kill") * cfg.Num("objectives.pointsMult"))));
            g.player = new Vec3(spot.x + 20, spot.y, spot.z); // out again: x1
            g.Steps(1);
            Assert.That(g.economy.boost, Is.EqualTo(1));
            before = g.economy.total;
            g.events.Emit("hit", new HitEvent { weapon = "revolver", killed = true });
            Assert.That(g.economy.total - before, Is.EqualTo(cfg.Num("points.kill")));

            // Hold it: the clock counts the steps inside; the HUD counts down.
            g.player = spot;
            g.Steps(120);
            Assert.That(g.objectives.Held, Is.EqualTo(121));
            Assert.That(g.objectives.HudLeft, Is.EqualTo((int)Math.Ceiling((need - 121) / cfg.movement.loop.simHz)));
            Assert.That(g.run.state, Is.EqualTo(SurvivalRun.State.Fight));
            g.Steps(need - 121 - 1);
            Assert.That(g.run.state, Is.EqualTo(SurvivalRun.State.Fight), "one step short");
            Assert.That(g.done, Is.Empty);
            g.Steps(1);
            // Held: the bonus (bonus x round), objectiveDone, the banner, and the round clears in the same step.
            double bonus = cfg.Num("objectives.bonus") * 6;
            Assert.That(g.done.Count, Is.EqualTo(1));
            Assert.That(g.done[0].owner == "player" && g.done[0].type == "hold" && g.done[0].round == 6 && g.done[0].bonus == bonus, Is.True);
            Assert.That(g.banners[g.banners.Count - 1], Is.EqualTo($"HELD · +{F(bonus)}"));
            Assert.That(g.economy.byReason["hold"], Is.EqualTo(bonus));
            Assert.That(g.log, Is.EqualTo(new[] { "start hold 6", "clear 6" }));
            Assert.That(g.run.state, Is.EqualTo(SurvivalRun.State.Break));
            Assert.That(g.run.round, Is.EqualTo(7));
            Assert.That(g.economy.boost, Is.EqualTo(1));
            Assert.That(g.objectives.RingVisible || g.objectives.Holding || g.objectives.HudOn, Is.False, "roundClear ended it");
            g.Steps(1000);
            Assert.That(g.done.Count, Is.EqualTo(1));
            Assert.That(g.economy.byReason["hold"], Is.EqualTo(bonus), "paid once");
        }

        [Test]
        public void AHoldRound_WithNoOpenSpot_IsNormal_ThenTheOpenSpotByRound()
        {
            var g = new Rig();
            var slabs = map.holdSpots.FindAll(s => g.zones.ZoneAt(new Vec3(s.x, s.y + 1, s.z)) == "slabs");
            Assert.That(slabs.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(g.zones.IsOpen("slabs"), Is.False);
            g.objectives.Load(slabs);
            g.economy.Start(0);
            Assert.That(g.objectives.Begin(6), Is.EqualTo("normal"));
            Assert.That(g.objectives.Holding || g.objectives.RingVisible || g.objectives.HudOn, Is.False);
            Assert.That(g.log, Is.Empty);
            Assert.That(g.banners, Is.Empty);
            g.zones.Open("slabs", free: true);
            Assert.That(g.objectives.Begin(6), Is.EqualTo("hold"));
            Assert.That(g.objectives.Spot.Value, Is.EqualTo(slabs[(6 * 7) % slabs.Count]));
            Assert.That(g.objectives.Begin(14), Is.EqualTo("hold"));
            Assert.That(g.objectives.Spot.Value, Is.EqualTo(slabs[(14 * 7) % slabs.Count]));
            // On the whole map with only the start zone open: the one landing spot.
            var h = new Rig();
            h.objectives.Load(map.holdSpots);
            h.economy.Start(0);
            Assert.That(h.objectives.Begin(6), Is.EqualTo("hold"));
            Assert.That(h.zones.ZoneAt(new Vec3(h.objectives.Spot.Value.x, 1, h.objectives.Spot.Value.z)), Is.EqualTo(map.startZone));
        }

        [Test]
        public void Inactive_WithNoMap_OrDisabled()
        {
            var g = new Rig();
            g.objectives.Load(null);
            g.economy.Start(0);
            Assert.That(g.objectives.Begin(6), Is.EqualTo("normal"));
            Assert.That(g.objectives.Begin(7), Is.EqualTo("normal"));
            Assert.That(g.economy.boost, Is.EqualTo(1));
            Assert.That(g.objectives.Holding || g.objectives.HudOn, Is.False);
            // a map with no hold spots is still active (the web's `!!map`): GOLD RUSH runs, a HOLD round is 'normal'
            g.objectives.Load(new List<Vec3>());
            Assert.That(g.objectives.Begin(6), Is.EqualTo("normal"));
            Assert.That(g.objectives.Begin(7), Is.EqualTo("gold"));
            Assert.That(g.objectives.Begin(5), Is.EqualTo("boss"), "begin returns the kind of a round with no objective");
            Assert.That(g.economy.boost, Is.EqualTo(1), "and ends the last objective");
        }

        [Test]
        public void GoldRush_KillsPayX2_AChainOf3Within3sPaysABonusPerKill_NonePastTheWindow()
        {
            var g = new Rig();
            g.objectives.Load(map.holdSpots);
            g.economy.Start(0);
            var chainPay = new List<double>();
            g.events.On<PointsEvent>("points", e =>
            {
                if (e.reason == "goldChain") chainPay.Add(e.delta);
            });
            Assert.That(g.objectives.Begin(7), Is.EqualTo("gold"));
            Assert.That(g.log, Is.EqualTo(new[] { "start gold 7" }));
            Assert.That(g.banners, Is.EqualTo(new[] { "GOLD RUSH · KILLS PAY x2 · CHAIN THEM" }));
            Assert.That(g.economy.boost, Is.EqualTo(cfg.Num("objectives.goldMult")));
            Assert.That(g.objectives.HudOn && g.objectives.HudType == "gold", Is.True);
            Assert.That(g.objectives.Holding, Is.False, "GOLD RUSH never holds the round");
            // a kill pays points.kill x 2
            double before = g.economy.total;
            g.events.Emit("hit", new HitEvent { weapon = "revolver", killed = true });
            Assert.That(g.economy.total - before, Is.EqualTo(cfg.Num("points.kill") * cfg.Num("objectives.goldMult")));

            int hz = (int)cfg.movement.loop.simHz;
            void Kill(string owner = "player") => g.events.Emit("kill", new KillEvent { owner = owner, weapon = "revolver", kind = "husk" });
            void StepTo(double seconds)
            {
                int target = (int)Math.Round(seconds * hz);
                while (stepped < target)
                {
                    g.objectives.Step();
                    stepped++;
                }
            }
            double chainBonus = cfg.Num("objectives.chainBonus");
            Kill(); // t 0
            StepTo(1);
            Kill(); // t 1
            Kill("rival"); // not the player's: no link
            Assert.That(chainPay, Is.Empty);
            StepTo(2);
            Kill(); // t 2: three within 3 s
            Assert.That(chainPay, Is.EqualTo(new[] { chainBonus * 3 }));
            StepTo(3);
            Kill(); // t 3: 3 - 0 = 3 <= the window: four
            Assert.That(chainPay, Is.EqualTo(new[] { chainBonus * 3, chainBonus * 4 }));
            StepTo(20);
            Kill(); // t 20: the chain starts over
            StepTo(21.5);
            Kill();
            StepTo(23.1);
            Kill(); // 23.1 - 20 > 3: the first fell out; two in the chain, no bonus
            Assert.That(g.objectives.ChainLength, Is.EqualTo(2));
            Assert.That(chainPay.Count, Is.EqualTo(2), "none past the window");
            Assert.That(g.economy.byReason["goldChain"], Is.EqualTo(chainBonus * 7));
            // roundClear ends it: kills pay x1 again
            g.events.Emit("roundClear", new RoundEvent { number = 7 });
            Assert.That(g.economy.boost, Is.EqualTo(1));
            Assert.That(g.objectives.HudOn, Is.False);
            Kill();
            Kill();
            Kill();
            Assert.That(chainPay.Count, Is.EqualTo(2));
        }

        int stepped;

        static string F(double v) => v.ToString("R", CultureInfo.InvariantCulture);

        [SetUp]
        public void Reset() => stepped = 0;

        [Test]
        public void WithFakes_TheCallsInTheWebsOrder_TimeCountsEveryStep_TheRingStaysUntilTheRoundEnds()
        {
            // The golden session's shape (port-export-p20 objectives.json): the deps as delegates that log their calls.
            var events = new EventBus();
            var calls = new List<string>();
            var open = new HashSet<string> { "west" };
            Vec3 player = default;
            string ZoneAt(Vec3 p)
            {
                string id = p.x < -10 ? "west" : p.x > 10 ? "east" : "middle";
                calls.Add($"zoneAt {F(p.x)},{F(p.y)},{F(p.z)} {id}");
                return id;
            }
            bool IsOpen(string id)
            {
                calls.Add($"isOpen {id} {open.Contains(id)}");
                return open.Contains(id);
            }
            void SetBoost(string name, double mult) => calls.Add($"setBoost {name} {F(mult)}");
            void Grant(double amount, string reason) => calls.Add($"grant {F(amount)} {reason}");
            var o = new Objectives(cfg, events, ZoneAt, IsOpen, SetBoost, Grant, () => player);
            events.On<BannerEvent>("banner", e => calls.Add($"banner {e.text}"));
            events.On<ObjectiveStartEvent>("objectiveStart", e => calls.Add($"objectiveStart {e.type} {e.round}"));

            // Inactive: begin returns normal with no call and no change to the round; step still counts time.
            Assert.That(o.Begin(6), Is.EqualTo("normal"));
            Assert.That(o.Round, Is.EqualTo(0));
            o.Step();
            o.Step();
            Assert.That(o.Time, Is.EqualTo(2));
            Assert.That(calls, Is.Empty);
            // load(null) ends the round all the same (setBoost hold 1, then gold 1), and stays inactive.
            o.Load(null);
            Assert.That(calls, Is.EqualTo(new[] { "setBoost hold 1", "setBoost gold 1" }));
            calls.Clear();

            var spots = new List<Vec3> { new Vec3(-30, 0, 5), new Vec3(30, 0, 5), new Vec3(-20, 1, -7) };
            o.Load(spots);
            calls.Clear();
            Assert.That(o.Begin(6), Is.EqualTo("hold"));
            Assert.That(calls, Is.EqualTo(new[]
            {
                "setBoost hold 1", "setBoost gold 1",
                "zoneAt -30,1,5 west", "isOpen west True",
                "zoneAt 30,1,5 east", "isOpen east False",
                "zoneAt -20,2,-7 west", "isOpen west True",
                "banner HOLD ROUND · STAND IN THE RING FOR 30 S", "objectiveStart hold 6",
            }));
            Vec3 at = spots[0]; // open = [spots[0], spots[2]]: open[(6 * 7) % 2] = open[0]
            Assert.That(o.Spot.Value, Is.EqualTo(at));
            Assert.That(o.RingPosition, Is.EqualTo(new Vec3(at.x, at.y + 0.04, at.z)));
            Assert.That(o.Time, Is.EqualTo(2), "begin doesn't step the clock");
            // `inside` carries over until the first step; the hold, done, keeps the ring until the round ends.
            player = at;
            o.Step();
            Assert.That(o.Inside, Is.True);
            var held = new Objectives.P { enabled = true, holdTime = 1 / cfg.movement.loop.simHz * 3, radius = 4, pointsMult = 1.5, bonus = 50, goldMult = 2, chainWindow = 3, chainAt = 3, chainBonus = 25 };
            var quick = new Objectives(cfg, events, ZoneAt, IsOpen, SetBoost, Grant, () => player, held);
            quick.Load(spots);
            Assert.That(quick.Begin(6), Is.EqualTo("hold"));
            Assert.That(quick.Need, Is.EqualTo(3));
            calls.Clear();
            quick.Step();
            quick.Step();
            quick.Step();
            Assert.That(quick.Done && quick.RingVisible && !quick.Holding && !quick.HudOn, Is.True, "held: the ring stays on");
            Assert.That(calls, Is.EqualTo(new[]
            {
                "setBoost hold 1.5", "setBoost hold 1.5", "setBoost hold 1.5", "setBoost hold 1", "grant 300 hold", "banner HELD · +300",
            }));
            events.Emit("roundClear", new RoundEvent { number = 6 });
            Assert.That(quick.RingVisible, Is.False);
            Assert.That(quick.RingPosition, Is.EqualTo(new Vec3(at.x, at.y + 0.04, at.z)), "ring.position is never reset");
        }

        [Test]
        public void Step_AllocatesNothing()
        {
            var g = new Rig();
            g.zones.OpenAll();
            g.objectives.Load(map.holdSpots);
            g.economy.Start(0);
            Assert.That(g.objectives.Begin(6), Is.EqualTo("hold"));
            Vec3 spot = g.objectives.Spot.Value;
            Vec3 inRing = spot, outside = new Vec3(spot.x + 10, spot.y, spot.z);
            int i = 0;
            TestDelegate run = () =>
            {
                for (int k = 0; k < 600; k++, i++)
                {
                    g.player = (i / 7) % 2 == 0 ? inRing : outside; // in and out: the boost set and cleared
                    g.objectives.Step();
                }
            };
            run(); // warm: the JIT, the boost dictionary's slot
            Assert.That(g.objectives.Holding, Is.True, "still short of the hold time");
            Assert.That(run, AllocIs.Not.AllocatingGCMemory());
            Assert.That(g.objectives.Held, Is.GreaterThan(0));
        }
    }
}
