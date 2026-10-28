// NIGHT12 U3-25.9: mutators.js (runModes.test.js's mutator tests, ported as they are: the web's expected values) and the
// HEAT ladder (design.heat): Apply then Clear restores every target exactly, a rung's cumulative mutators and multiplier,
// the rung unlock on an exfil (none without one, none when not recording), rung 0 / the ladder off is the web's game,
// LOW GRAVITY unavailable (no gravity factor in Mover.cs), HuskBrain.speedMult defaults to 1.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public sealed class HeatTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        /// <summary>The systems a set changes, built as SurvivalGame builds them (one player).</summary>
        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly Profile profile = new Profile();
            public readonly Health health;
            public readonly Regen regen;
            public readonly Economy economy;
            public readonly Loadout loadout;
            public readonly HuskBrain brain;
            public readonly Mutators.Targets t;
            public readonly Heat heat;
            public readonly List<string> log = new List<string>();

            public Rig()
            {
                var world = new TriangleWorld();
                world.AddBox(new Vec3(-10, -1, -10), new Vec3(10, 0, 10));
                var mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                health = new Health(100, events, "player");
                regen = new Regen(cfg, events, health, () => 0);
                economy = new Economy(cfg, events);
                loadout = new Loadout(cfg, events, new EnemyRegistry(), world, mover, health, () => (new Vec3(0, 1.6, 0), new Vec3(0, 0, -1)));
                brain = new HuskBrain(cfg, events, null, null);
                t = new Mutators.Targets { loadout = loadout, health = health, brain = brain, economy = economy, regen = regen };
                heat = new Heat(cfg, events, profile);
                events.On<MutatorEvent>(MetaEvents.MutatorUnlock, e => log.Add("mutatorUnlock " + e.id));
                events.On<HeatEvent>(MetaEvents.HeatUnlock, e => log.Add("heatUnlock " + e.rung));
            }
        }

        static double Rung(int i) => PortData.Num(PortData.Obj(cfg.Arr("design.heat.rungs")[i - 1])["scoreMult"]);

        // --- runModes.test.js: mutators (OVERNIGHT2 N6) -----------------------------------------------------------------

        [Test]
        public void Mutators_UnlockByTheBestRound()
        {
            Assert.That(MutatorMath.IsUnlocked("lowGravity", 2, cfg), Is.False);
            Assert.That(MutatorMath.IsUnlocked("lowGravity", cfg.Num("mutators.kinds.lowGravity.unlockRound"), cfg), Is.True);
            Assert.That(MutatorMath.IsUnlocked("noRegen", 14, cfg), Is.False);
        }

        [Test]
        public void ASet_HasAnOrderFreeBoardKey_AndCombinedNumbers()
        {
            Assert.That(MutatorMath.MutatorKey(new[] { "speedDemon", "glassCannon" }), Is.EqualTo(MutatorMath.MutatorKey(new[] { "glassCannon", "speedDemon" })));
            Assert.That(MutatorMath.MutatorKey(new string[0]), Is.EqualTo(""));
            MutatorMath.Stats s = MutatorMath.MutatorStatsOf(new List<string> { "glassCannon", "noRegen", "speedDemon" }, cfg);
            Assert.That(s.dealt, Is.EqualTo(2));
            Assert.That(s.taken, Is.EqualTo(2));
            Assert.That(s.regen, Is.False);
            Assert.That(s.points, Is.EqualTo(cfg.Num("mutators.kinds.speedDemon.points")));
        }

        // --- the port ------------------------------------------------------------------------------------------------------

        [Test]
        public void TheBoardKey_IsSortedAndFiltered_AsJsSort()
        {
            Assert.That(MutatorMath.MutatorKey(new[] { "speedDemon", "glassCannon", "nope", "lowGravity" }), Is.EqualTo("glassCannon+lowGravity+speedDemon"));
            MutatorMath.Stats none = MutatorMath.MutatorStatsOf(new List<string>(), cfg);
            Assert.That(none.gravity == 1 && none.dealt == 1 && none.taken == 1 && none.enemySpeed == 1 && none.points == 1 && none.regen, Is.True);
        }

        [Test]
        public void ApplyThenClear_RestoresEveryTargetExactly()
        {
            var r = new Rig();
            r.regen.SetEnabled(true); // the run's start (SurvivalRun.Start)
            MutatorMath.Stats s = r.heat.mutators.Apply(r.t, new List<string> { "glassCannon", "speedDemon", "noRegen" });
            Assert.That(r.loadout.damageMult, Is.EqualTo(cfg.Num("mutators.kinds.glassCannon.dealt")));
            Assert.That(r.health.takenMult, Is.EqualTo(cfg.Num("mutators.kinds.glassCannon.taken")));
            Assert.That(r.brain.speedMult, Is.EqualTo(cfg.Num("mutators.kinds.speedDemon.enemySpeed")));
            Assert.That(r.economy.boost, Is.EqualTo(cfg.Num("mutators.kinds.speedDemon.points")));
            Assert.That(r.regen.enabled, Is.False);
            Assert.That(s.regen, Is.False);
            Assert.That(r.heat.mutators.Active, Is.EqualTo(new[] { "glassCannon", "speedDemon", "noRegen" }));
            Assert.That(r.heat.mutators.Key, Is.EqualTo("glassCannon+noRegen+speedDemon"));

            r.heat.mutators.Clear(r.t);
            Assert.That(r.loadout.damageMult, Is.EqualTo(1));
            Assert.That(r.health.takenMult, Is.EqualTo(1));
            Assert.That(r.brain.speedMult, Is.EqualTo(1));
            Assert.That(r.economy.boost, Is.EqualTo(1));
            Assert.That(r.heat.mutators.Active, Is.Empty);
            Assert.That(r.regen.enabled, Is.False, "mutators.js clear leaves regen; the next run's start enables it");
            r.regen.SetEnabled(true);
            r.heat.mutators.Apply(r.t, new List<string>());
            Assert.That(r.regen.enabled, Is.True, "a set without NO REGEN never touches regen");
        }

        [Test]
        public void ARung_PlaysTheRungsBelowIt_AndPaysItsMultiplier()
        {
            var r = new Rig();
            Assert.That(r.heat.enabled, Is.True);
            Assert.That(r.heat.Rungs, Is.EqualTo(3));
            Assert.That(r.heat.MutatorsOf(0), Is.Empty);
            Assert.That(r.heat.MutatorsOf(1), Is.EqualTo(new[] { "glassCannon" }));
            Assert.That(r.heat.MutatorsOf(2), Is.EqualTo(new[] { "glassCannon", "speedDemon" }));
            Assert.That(r.heat.MutatorsOf(3), Is.EqualTo(new[] { "glassCannon", "speedDemon", "noRegen" }));
            Assert.That(r.heat.MutatorsOf(9), Is.EqualTo(r.heat.MutatorsOf(3)));
            Assert.That(r.heat.ScoreMult(0), Is.EqualTo(1));
            for (int i = 1; i <= 3; i++) Assert.That(r.heat.ScoreMult(i), Is.EqualTo(Rung(i)));
            Assert.That(1 < r.heat.ScoreMult(1) && r.heat.ScoreMult(1) < r.heat.ScoreMult(2) && r.heat.ScoreMult(2) < r.heat.ScoreMult(3), Is.True);
            Assert.That(r.heat.Name(0), Is.EqualTo(""));
            Assert.That(r.heat.Name(2), Is.EqualTo("HEAT 2"));
            Assert.That(r.heat.Rule(1), Is.EqualTo(cfg.Str("mutators.kinds.glassCannon.name") + ": " + cfg.Str("mutators.kinds.glassCannon.desc")));
            Assert.That(r.heat.Rule(1), Is.EqualTo("GLASS CANNON: x2 damage dealt and taken"));
            Assert.That(r.heat.RulesOf(3).Count, Is.EqualTo(3));

            r.regen.SetEnabled(true);
            r.heat.ApplyRung(r.t, 2);
            Assert.That(r.heat.mutators.Active, Is.EqualTo(new[] { "glassCannon", "speedDemon" }));
            Assert.That(r.loadout.damageMult, Is.EqualTo(cfg.Num("mutators.kinds.glassCannon.dealt")));
            Assert.That(r.brain.speedMult, Is.EqualTo(cfg.Num("mutators.kinds.speedDemon.enemySpeed")));
            Assert.That(r.economy.boost, Is.EqualTo(cfg.Num("mutators.kinds.speedDemon.points") * Rung(2)));
            Assert.That(r.regen.enabled, Is.True);

            // The DAILY's twist and the title's toggles come first, the rung's after, without repeats (st.mutators already
            // holding the rung's ids is fine).
            r.heat.ApplyRung(r.t, 1, new List<string> { "noRegen", "glassCannon" });
            Assert.That(r.heat.mutators.Active, Is.EqualTo(new[] { "noRegen", "glassCannon" }));
            Assert.That(r.economy.boost, Is.EqualTo(Rung(1)));
            Assert.That(r.regen.enabled, Is.False);

            r.heat.Clear(r.t);
            Assert.That(r.economy.boost, Is.EqualTo(1));
            Assert.That(r.loadout.damageMult == 1 && r.health.takenMult == 1 && r.brain.speedMult == 1, Is.True);
        }

        [Test]
        public void RungZero_AndTheLadderOff_AreTheWebsGame()
        {
            var r = new Rig();
            r.regen.SetEnabled(true);
            r.heat.ApplyRung(r.t, 0);
            Assert.That(r.heat.mutators.Active, Is.Empty);
            Assert.That(r.loadout.damageMult == 1 && r.health.takenMult == 1 && r.brain.speedMult == 1 && r.economy.boost == 1 && r.regen.enabled, Is.True);

            r.heat.enabled = false;
            Assert.That(r.heat.Rungs, Is.EqualTo(0));
            r.heat.ApplyRung(r.t, 3);
            Assert.That(r.heat.mutators.Active, Is.Empty);
            Assert.That(r.economy.boost, Is.EqualTo(1));
            Assert.That(r.heat.Name(3), Is.EqualTo(""));
            Assert.That(r.heat.RunEnd(0, true, true), Is.False);
            Assert.That(r.profile.heat, Is.EqualTo(0));
        }

        [Test]
        public void AnExfil_OpensTheNextRung_OnlyWhenRecording()
        {
            var r = new Rig();
            Assert.That(r.heat.Unlocked, Is.EqualTo(0));
            Assert.That(r.heat.RunEnd(0, false, true), Is.False, "a death opens nothing");
            Assert.That(r.heat.RunEnd(0, true, false), Is.False, "a LAB / automation run writes nothing");
            Assert.That(r.profile.heat, Is.EqualTo(0));
            Assert.That(r.log, Is.Empty);

            Assert.That(r.heat.RunEnd(0, true, true), Is.True);
            Assert.That(r.profile.heat, Is.EqualTo(1));
            Assert.That(r.heat.Unlocked, Is.EqualTo(1));
            Assert.That(r.heat.RunEnd(0, true, true), Is.False, "already open");
            Assert.That(r.heat.RunEnd(1, true, true), Is.True);
            Assert.That(r.heat.RunEnd(2, true, true), Is.True);
            Assert.That(r.heat.RunEnd(3, true, true), Is.False, "capped at the top rung");
            Assert.That(r.profile.heat, Is.EqualTo(3));
            Assert.That(r.log, Is.EqualTo(new[] { "heatUnlock 1", "heatUnlock 2", "heatUnlock 3" }));
        }

        [Test]
        public void TheTitlesMutators_UnlockByBestRound_AndLowGravityIsUnavailable()
        {
            var r = new Rig();
            Mutators m = r.heat.mutators;
            Assert.That(m.Toggle("glassCannon"), Is.False, "locked");
            Assert.That(m.RunEnd(6, false), Is.Empty, "not recording");
            Assert.That(r.profile.mutatorBest, Is.EqualTo(0));

            Assert.That(m.RunEnd(6, true), Is.EqualTo(new[] { "glassCannon" }), "LOW GRAVITY unlocks too, but can't be played: no event");
            Assert.That(r.profile.mutatorBest, Is.EqualTo(6));
            Assert.That(m.RunEnd(5, true), Is.Empty);
            Assert.That(m.RunEnd(20, true), Is.EqualTo(new[] { "speedDemon", "noRegen" }));
            Assert.That(r.log, Is.EqualTo(new[] { "mutatorUnlock glassCannon", "mutatorUnlock speedDemon", "mutatorUnlock noRegen" }));

            Assert.That(MutatorMath.IsUnlocked("lowGravity", r.profile.mutatorBest, cfg), Is.True);
            Assert.That(m.Toggle("lowGravity"), Is.False);
            Assert.That(Mutators.Available("lowGravity"), Is.False);
            Assert.That(m.Toggle("noRegen"), Is.True);
            Assert.That(m.Toggle("glassCannon"), Is.True);
            Assert.That(m.Picks(), Is.EqualTo(new[] { "glassCannon", "noRegen" }), "MUTATORS order");
            Assert.That(m.Toggle("noRegen"), Is.False);
            Assert.That(m.Picks(), Is.EqualTo(new[] { "glassCannon" }));

            List<Mutators.Row> rows = m.List();
            Assert.That(rows.Count, Is.EqualTo(4));
            Assert.That(rows[0].id == "lowGravity" && rows[0].unlocked && !rows[0].available, Is.True);
            Assert.That(rows[1].picked && rows[1].name == cfg.Str("mutators.kinds.glassCannon.name"), Is.True);

            m.Apply(r.t, new List<string> { "lowGravity", "glassCannon", "glassCannon", "nope" });
            Assert.That(m.Active, Is.EqualTo(new[] { "glassCannon" }), "LOW GRAVITY, repeats and unknown ids are dropped");
        }

        [Test]
        public void TheProfileIsReadLive()
        {
            var r = new Rig(); // built on an empty profile; the smoke refills the same object in place (Profile.CopyFrom)
            r.profile.heat = 2;
            r.profile.mutatorBest = 10;
            r.profile.mutatorPicks.Add("speedDemon");
            Assert.That(r.heat.Unlocked, Is.EqualTo(2));
            Assert.That(r.heat.mutators.Picks(), Is.EqualTo(new[] { "speedDemon" }));
        }

        [Test]
        public void HuskBrain_PaceDefaultsToOne()
        {
            var brain = new HuskBrain(cfg, new EventBus(), null, null);
            Assert.That(brain.speedMult, Is.EqualTo(1));
        }
    }
}
