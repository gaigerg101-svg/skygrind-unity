// NIGHT8 U2-16: the LAB (design.lab): every toggle on and off. Off is the web's value; on is the design config's.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class LabTests
    {
        static GameConfig cfg;
        static double dt;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            dt = cfg.Dt;
        }

        static Func<string, bool> Only(params string[] keys) => k => Array.IndexOf(keys, k) >= 0;

        sealed class FakeSpawner : IRoundSpawner
        {
            public void StartWave(int round, uint seed) { }
            public void Step(double dt) { }
            public bool Done => false;
            public void Clear() { }
        }

        [Test]
        public void TheToggles_AreSettingsRows_AllOffByDefault_AndRoundTripTheSettingsJson()
        {
            var lab = new Lab(cfg);
            Assert.That(lab.toggles.ConvertAll(t => t.key), Is.EqualTo(new[] { LabMath.CloseKillHeal, LabMath.Freshness, LabMath.StyleRamp, LabMath.ReadyFromOne, LabMath.SquadTighten, LabMath.UltraHealthKey }));
            Assert.That(lab.toggles.TrueForAll(t => !t.on), Is.True, "all off by default");
            var extras = new List<(SettingItem, object)>();
            foreach (Lab.Toggle t in lab.toggles) extras.Add((new SettingItem { key = t.key, tab = "lab", label = t.name, type = "toggle" }, t.on));
            var store = new SettingsStore(cfg, extras, new List<(string, string)> { ("lab", "LAB") });
            Assert.That(store.Bool(LabMath.StyleRamp), Is.False);
            Assert.That(store.Save(), Does.Contain("\"lab.styleRamp\":false"));
            store.Load("{\"lab.styleRamp\":true,\"lab.freshness\":1}"); // a number for a toggle is not taken
            Assert.That(store.Bool(LabMath.StyleRamp), Is.True);
            Assert.That(store.Bool(LabMath.Freshness), Is.False);
            store.ResetTab("lab");
            Assert.That(store.Bool(LabMath.StyleRamp), Is.False, "RESET TAB turns the LAB off");
            lab.Read(store.Bool);
            Assert.That(lab.Any, Is.False);
            lab.Read(Only(LabMath.CloseKillHeal, LabMath.StyleRamp));
            Assert.That(lab.Any, Is.True);
            Assert.That(lab.Names(), Is.EqualTo("CLOSE-KILL HEAL, STYLE RAMP"));
            lab.Read(null);
            Assert.That(lab.Any, Is.False, "null: the web's game");
        }

        [Test]
        public void CloseKillHeal_On_HealsCloseAndMeleeKills_AndSlowsRegen_Off_IsTheWebs()
        {
            var events = new EventBus();
            var health = new Health(100, events, "player");
            var regen = new Regen(cfg, events, health, () => 0);
            var lab = new Lab(cfg);
            lab.Wire(events, health);
            void Kill(string weapon, double distance) => events.Emit("kill", new KillEvent { weapon = weapon, kind = "husk", distance = distance });

            lab.Read(null);
            lab.Apply(cfg, regen, null, null, null);
            health.hp = 50;
            Kill("revolver", 2);
            Kill("punch", 1);
            Assert.That(health.hp, Is.EqualTo(50), "off: no kill heals");
            regen.SetEnabled(true);
            regen.Step(dt);
            Assert.That(health.hp, Is.EqualTo(50 + cfg.Num("regen.rate") * dt).Within(1e-9), "off: regen.rate");

            lab.Read(Only(LabMath.CloseKillHeal));
            lab.Apply(cfg, regen, null, null, null);
            health.hp = 50;
            Kill("revolver", 3.9);
            Assert.That(health.hp, Is.EqualTo(65), "a kill within 4 m heals 15");
            Kill("revolver", 4.5);
            Assert.That(health.hp, Is.EqualTo(65), "a far kill heals nothing");
            Kill("punch", 9);
            Kill("gauntlet", 9);
            Assert.That(health.hp, Is.EqualTo(95), "punch and gauntlet kills heal at any range");
            events.Emit("kill", new KillEvent { owner = "rival", weapon = "punch", distance = 1 });
            Assert.That(health.hp, Is.EqualTo(95), "only the player's kills");
            health.hp = 50;
            regen.Step(dt);
            Assert.That(health.hp, Is.EqualTo(50 + 12 * dt).Within(1e-9), "regen at 12 HP/s");
        }

        [Test]
        public void Freshness_On_FreshPays1p5_DullPaysNothing_Off_IsTheWebs()
        {
            var s = StyleMath.From(cfg);
            var p = LabMath.P.From(cfg);
            Assert.That(LabMath.FreshnessMult(10, s, p), Is.EqualTo(1.5));
            Assert.That(LabMath.FreshnessMult(6, s, p), Is.EqualTo(s.usedMult));
            Assert.That(LabMath.FreshnessMult(4, s, p), Is.EqualTo(s.staleMult));
            Assert.That(LabMath.FreshnessMult(2, s, p), Is.EqualTo(0));
            Assert.That(LabMath.FreshnessTierName(2, s, p), Is.EqualTo("DULL"));
            Assert.That(LabMath.FreshnessTierName(4, s, p), Is.EqualTo("STALE"));

            // Through the meter: a FRESH revolver's kill (the meter starts every weapon at style.freshness.start).
            var events = new EventBus();
            var meter = new StyleMeter(cfg, events, () => "GROUND", () => true);
            var paid = new List<double>();
            events.On<StyleAwardEvent>("styleAward", e => { if (e.name == "KILL") paid.Add(e.points); });
            var lab = new Lab(cfg);
            double killBase = s.baseP["huskKill"];
            void KillOnce()
            {
                meter.Start();
                events.Emit("kill", new KillEvent { weapon = "revolver", kind = "husk", distance = 10 });
            }
            lab.Read(null);
            lab.Apply(cfg, null, meter, null, null);
            KillOnce();
            Assert.That(meter.TierName(2), Is.EqualTo("STALE"), "off: no DULL");
            lab.Read(Only(LabMath.Freshness));
            lab.Apply(cfg, null, meter, null, null);
            KillOnce();
            Assert.That(meter.TierName(2), Is.EqualTo("DULL"));
            Assert.That(paid, Is.EqualTo(new[] { JsMath.Round(killBase * s.freshMult), JsMath.Round(killBase * 1.5) }));
        }

        [Test]
        public void StyleRamp_On_ClimbsTo3xOver1s_GroundResets_Off_IsTheWebsFlat()
        {
            var s = StyleMath.From(cfg);
            var p = LabMath.P.From(cfg);
            int ramp = MovementMath.StepsFor(p.rampTime, dt);
            Assert.That(LabMath.RampMult(true, 0, ramp, s, p), Is.EqualTo(s.stateMult));
            Assert.That(LabMath.RampMult(true, ramp / 2, ramp, s, p), Is.EqualTo(s.stateMult + (3 - s.stateMult) * 0.5).Within(1e-12));
            Assert.That(LabMath.RampMult(true, ramp * 2, ramp, s, p), Is.EqualTo(3));
            Assert.That(LabMath.RampMult(false, ramp, ramp, s, p), Is.EqualTo(1));

            // Through the meter: a wall jump's trick points after 1 s in the air, then a dash jump after one step on the ground.
            string state = "AIR";
            var events = new EventBus();
            var meter = new StyleMeter(cfg, events, () => state, () => true);
            var paid = new List<(string, double)>();
            events.On<StyleAwardEvent>("styleAward", e => paid.Add((e.name, e.points)));
            var lab = new Lab(cfg);
            List<(string, double)> Run(bool on)
            {
                paid.Clear();
                lab.Read(on ? Only(LabMath.StyleRamp) : null);
                lab.Apply(cfg, null, meter, null, null);
                meter.Start();
                state = "AIR";
                for (int i = 0; i < ramp; i++) meter.Step(dt);
                events.Emit("trick", "wallJump");
                state = "GROUND";
                meter.Step(dt);
                events.Emit("trick", "dashJump");
                return new List<(string, double)>(paid);
            }
            double wall = s.tricks["wallJump"], dash = s.tricks["dashJump"];
            Assert.That(Run(false), Is.EqualTo(new[] { ("WALL JUMP", JsMath.Round(wall * s.stateMult)), ("DASH JUMP", JsMath.Round(dash)) }), "off: flat 1.2 in the air");
            Assert.That(Run(true), Is.EqualTo(new[] { ("WALL JUMP", JsMath.Round(wall * 3)), ("DASH JUMP", JsMath.Round(dash)) }), "on: 3x after 1 s; the ground resets");
        }

        [Test]
        public void ReadyFromOne_On_ReadiesRoundOnesBreak_Off_WaitsForReadyFrom()
        {
            var events = new EventBus();
            var health = new Health(100, events, "player");
            var regen = new Regen(cfg, events, health, () => 0);
            var run = new SurvivalRun(cfg, events, new FakeSpawner(), new Economy(cfg, events), regen);
            var lab = new Lab(cfg);
            lab.Read(null);
            lab.Apply(cfg, regen, null, run, null);
            run.Start(7);
            Assert.That(run.readyFrom, Is.EqualTo(cfg.Num("survival.readyFrom")));
            Assert.That(run.Ready(), Is.False, "off: no READY before round 3");
            lab.Read(Only(LabMath.ReadyFromOne));
            lab.Apply(cfg, regen, null, run, null);
            run.Start(7);
            Assert.That(run.round, Is.EqualTo(1));
            Assert.That(run.Ready(), Is.True, "on: READY in round 1's break");
            run.Step(dt);
            Assert.That(run.state, Is.EqualTo(SurvivalRun.State.Fight));
        }

        [Test]
        public void SquadTighten_On_ShrinksTheIntervalPerRound_WithAFloor_Off_IsFlat()
        {
            var p = LabMath.P.From(cfg);
            double baseInterval = cfg.Num("director.squadInterval");
            Assert.That(LabMath.SquadInterval(baseInterval, 1, p), Is.EqualTo(baseInterval));
            Assert.That(LabMath.SquadInterval(baseInterval, 10, p), Is.EqualTo(baseInterval * Math.Pow(0.96, 9)).Within(1e-12));
            Assert.That(LabMath.SquadInterval(baseInterval, 40, p), Is.EqualTo(1.0), "the 1 s floor");

            var map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
            var events = new EventBus();
            var world = new GameWorld(map.BuildOctree(), map.CreateCore(cfg.movement));
            var zones = new Zones(cfg, map, world, events);
            var nav = new NavField(cfg, NavGrid.FromExport(map.navGrid));
            var brain = new HuskBrain(cfg, events, nav, world.Geometry) { onScreen = _ => true };
            var husks = new Husks(cfg, events, new EnemyRegistry(), world, map.killPlaneY);
            Vec3 player = map.playerSpawn;
            var director = new Director(cfg, events, husks, brain, zones, world.core, world.Geometry, nav, () => player, () => 0, () => 0) { huskWorld = world };
            var lab = new Lab(cfg);
            lab.Read(null);
            lab.Apply(cfg, null, null, null, director);
            director.NewRun();
            director.StartWave(10, 3);
            Assert.That(director.SquadInterval, Is.EqualTo(baseInterval), "off: flat");
            lab.Read(Only(LabMath.SquadTighten));
            lab.Apply(cfg, null, null, null, director);
            director.StartWave(10, 3);
            Assert.That(director.SquadInterval, Is.EqualTo(baseInterval * Math.Pow(0.96, 9)).Within(1e-12));
        }
    }
}
