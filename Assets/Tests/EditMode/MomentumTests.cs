// NIGHT9 U3-09: momentum (combat/momentum.js) and the impact waves (combat/impact.js). The web's system test
// (tests/momentum.test.js: a dash toward a target from a walk reaches M above the impact threshold), then the port's
// through the loadout: a fast Gauntlet hit sends its ring wave (a share of the hit, a push, a stagger) and an IMPACT, and
// carries the momentum the hitstop reads; guns scale by lerp(1, gunMax, M); a wave's hit sets off no second wave.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class MomentumTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Mover mover;
            public readonly Loadout loadout;
            public readonly Husks husks;
            public readonly List<HitEvent> hits = new List<HitEvent>();
            public readonly List<ImpactWaveEvent> waves = new List<ImpactWaveEvent>();
            public readonly List<StyleBonusEvent> bonuses = new List<StyleBonusEvent>();

            public Rig()
            {
                world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
                mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (new Vec3(0, 1.6, 0), new Vec3(0, 0, -1)));
                loadout.arsenal.enabled = false; // momentum alone (ArsenalTests covers the arsenal)
                husks = new Husks(cfg, events, enemies, world, -30);
                events.On<HitEvent>("hit", e => hits.Add(e));
                events.On<ImpactWaveEvent>("impactWave", e => waves.Add(e));
                events.On<StyleBonusEvent>("styleBonus", e => bonuses.Add(e));
            }

            public Husk Husk(double x, double z)
            {
                Husk h = husks.Spawn(new Vec3(x, 0, z), new Vec3(0, 0, 0));
                h.hp = h.maxHp = 1e9;
                return h;
            }
        }

        [Test, Description("momentum.test.js: a dash toward a target from a walk reaches M above the impact threshold; reset clears it")]
        public void ADashTowardATarget_ReachesMAboveTheImpactThreshold()
        {
            var r = new Rig();
            var mo = new Momentum(cfg, r.mover);
            r.mover.velocity = new Vec3(0, 0, -8);
            for (int i = 0; i < 60; i++) mo.Step(1.0 / 120); // walking 8 m/s
            r.mover.velocity = new Vec3(0, 0, -24); // a dash
            for (int i = 0; i < 12; i++) mo.Step(1.0 / 120);
            double M = mo.At(new Vec3(0, 1.6, 0), new Vec3(0, 1, -3));
            Assert.That(M, Is.GreaterThan(cfg.Num("momentum.shock.at")));
            Assert.That(mo.Surging, Is.True);
            mo.Reset();
            Assert.That(mo.At(new Vec3(0, 1.6, 0), new Vec3(0, 1, -3)), Is.EqualTo(0));
        }

        [Test, Description("a Gauntlet hit at M 0.9: x lerp(1, 3, M); IMPACT; the ring wave hits the neighbour (shock share, no second wave), staggers it; the hit carries M (the hitstop's test)")]
        public void AFastGauntletHit_SendsAnImpactWave_AndAnImpact()
        {
            var r = new Rig();
            Husk target = r.Husk(0, -2);
            Husk near = r.Husk(1.5, -2.5);
            Husk far = r.Husk(12, -12);
            r.loadout.momentum.force = 0.9;
            double meleeMax = cfg.Num("momentum.meleeMax");
            r.loadout.Damage(target, 10, false, "gauntlet", new Vec3(0, 1.6, 0));
            HitEvent first = r.hits[0];
            Assert.That(first.momentum, Is.EqualTo(0.9));
            Assert.That(first.damage, Is.EqualTo(10 * (1 + (meleeMax - 1) * 0.9)).Within(1e-9), "melee x lerp(1, meleeMax, M)");
            Assert.That(first.melee && !first.shock && first.momentum > cfg.Num("feel.momentumHitstopAt"), Is.True, "main.js: this hit freezes feel.momentumHitstop");
            Assert.That(r.bonuses.Exists(b => b.name == "IMPACT"), Is.True, "M > impactBonusAt pays IMPACT");
            Assert.That(r.waves.Count, Is.EqualTo(1));
            Assert.That(r.waves[0].kind, Is.EqualTo("ring"));
            Assert.That(r.waves[0].hits, Is.EqualTo(1), "the neighbour inside the ring, not the far one");
            HitEvent shock = r.hits.Find(h => h.enemy == near.id);
            Assert.That(shock, Is.Not.Null);
            Assert.That(shock.shock, Is.True);
            Assert.That(shock.momentum, Is.EqualTo(0), "a wave's hit carries no momentum");
            Assert.That(near.staggerSteps, Is.GreaterThan(0), "the wave staggers");
            Assert.That(r.hits.Exists(h => h.enemy == far.id), Is.False);
            // On cooldown (momentum.shock.cooldown): the next fast hit sends no wave.
            r.loadout.Damage(target, 10, false, "gauntlet", new Vec3(0, 1.6, 0));
            Assert.That(r.waves.Count, Is.EqualTo(1));
            int cd = MovementMath.StepsFor(cfg.Num("momentum.shock.cooldown"), cfg.Dt);
            for (int i = 0; i < cd; i++) r.loadout.Step(new CombatInput { weapon = -1 }, cfg.Dt);
            r.loadout.Damage(target, 10, false, "punch", new Vec3(0, 1.6, 0));
            Assert.That(r.waves.Count, Is.EqualTo(2), "the punch is the Gauntlet's ring too");
        }

        [Test, Description("a slow hit (M under shock.at) sends no wave and pays no IMPACT; a gun scales by lerp(1, gunMax, M); secondary damage keeps its number")]
        public void ASlowHit_NoWave_GunsScaleByGunMax()
        {
            var r = new Rig();
            Husk target = r.Husk(0, -2);
            r.Husk(1.5, -2.5);
            r.loadout.momentum.force = 0.4;
            r.loadout.Damage(target, 10, false, "gauntlet", new Vec3(0, 1.6, 0));
            Assert.That(r.waves.Count, Is.EqualTo(0));
            Assert.That(r.bonuses.Count, Is.EqualTo(0));
            r.loadout.momentum.force = 1;
            r.loadout.Damage(target, 10, false, "revolver", new Vec3(0, 1.6, 0));
            Assert.That(r.hits[1].damage, Is.EqualTo(10 * cfg.Num("momentum.gunMax")).Within(1e-9));
            r.loadout.Damage(target, 10, false, "burn", new Vec3(0, 1.6, 0));
            Assert.That(r.hits[2].damage, Is.EqualTo(10), "burn is secondary: no momentum");
            Assert.That(r.hits[2].momentum, Is.EqualTo(0));
        }
    }
}
