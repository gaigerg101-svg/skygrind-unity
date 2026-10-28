// NIGHT20 U3-34.5 (builder G): the retune's knobs (design.arsenal.n20.tune, Core/Guns/Tune20.cs) through the real
// loadout: with design.arsenal.n20 on a gun's damage knob multiplies its hits and its interval knob its cooldown; off, the
// web's numbers bit for bit. The knobs' values come from the design config (the balance pass moves them; the ledger is in
// NIGHT20-LOG.md), so the tests read them rather than repeat them.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class G20TuneTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static double KnobOf(string gun, string knob)
        {
            string p = $"design.arsenal.n20.tune.{gun}.{knob}";
            return cfg.Has(p) ? cfg.Num(p) : 1;
        }

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Husks husks;
            public readonly Mover mover;
            public readonly Loadout loadout;
            public readonly List<HitEvent> hits = new List<HitEvent>();
            public readonly List<int> fires = new List<int>();
            int step;

            public Rig(bool arsenalN20, string gun)
            {
                world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
                husks = new Husks(cfg, events, enemies, world, -30);
                mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (new Vec3(0, 1.6, 0), new Vec3(0, 0, -1)));
                loadout.n20 = new N20Blocks { arsenal = arsenalN20 };
                loadout.freshDamage = false;
                loadout.Reset(new List<string> { gun }, 1);
                events.On<HitEvent>("hit", h => hits.Add(new HitEvent { weapon = h.weapon, damage = h.damage }));
                events.On("fire", _ => fires.Add(step));
            }

            public void Fire(int steps)
            {
                for (int i = 0; i < steps; i++, step++)
                {
                    mover.Step(default, cfg.Dt);
                    loadout.Step(new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } }, cfg.Dt);
                }
            }
        }

        [Test]
        public void TheKnobs_AreLiveOnlyWithTheBlock()
        {
            foreach (string gun in new[] { "revolver", "shotgun", "nailgun", "rocket", "rail", "zapper", "bouncer" })
            {
                var on = new Rig(true, gun);
                var off = new Rig(false, gun);
                Assert.That(on.loadout.TuneDamage(gun), Is.EqualTo(KnobOf(gun, "damage")), gun + " damage on");
                Assert.That(on.loadout.TuneInterval(gun), Is.EqualTo(KnobOf(gun, "interval")), gun + " interval on");
                Assert.That(on.loadout.TuneRadius(gun), Is.EqualTo(KnobOf(gun, "radius")), gun + " radius on");
                Assert.That(off.loadout.TuneDamage(gun), Is.EqualTo(1), gun + " damage off");
                Assert.That(off.loadout.TuneInterval(gun), Is.EqualTo(1), gun + " interval off");
            }
            Assert.That(cfg.Has("design.arsenal.n20.tune"), Is.True, "the retune's table is in the design config");
        }

        [Test]
        public void ADamageKnob_MultipliesTheHit_AnIntervalKnob_TheCooldown()
        {
            // The KESTREL on a husk 10 m ahead, then its shots over 2 s: the hit's damage and the shots' spacing.
            string gun = "revolver";
            var on = new Rig(true, gun);
            var off = new Rig(false, gun);
            foreach (Rig r in new[] { on, off })
            {
                r.loadout.SetKnob(gun, "damage", 1.5); // a test knob (the config's own are read by the test above)
                r.loadout.SetKnob(gun, "interval", 2);
                Husk h = (Husk)r.husks.Spawn("husk", new Vec3(0, 0, -10), r.mover.position);
                h.hp = h.maxHp = 1e6; // never dies: every shot hits it
                r.Fire(240);
            }
            Assert.That(off.hits.Count, Is.GreaterThan(0));
            Assert.That(on.hits[0].damage, Is.EqualTo(off.hits[0].damage * 1.5).Within(1e-9), "damage x the knob");
            int gapOn = on.fires[1] - on.fires[0], gapOff = off.fires[1] - off.fires[0];
            Assert.That(gapOn, Is.EqualTo(MovementMath.StepsFor(cfg.Num("weapons.revolver.fireInterval") * 2, cfg.Dt)), "the interval x the knob");
            Assert.That(gapOff, Is.EqualTo(MovementMath.StepsFor(cfg.Num("weapons.revolver.fireInterval"), cfg.Dt)), "off: the web's interval");
        }

        [Test]
        public void TheTuneTable_KnowsOnlyItsFourKnobs()
        {
            foreach (KeyValuePair<string, object> gun in cfg.Obj("design.arsenal.n20.tune"))
            {
                if (gun.Key.StartsWith("_")) continue;
                foreach (KeyValuePair<string, object> knob in PortData.Obj(gun.Value))
                    if (!knob.Key.StartsWith("_")) Assert.That(new[] { "damage", "interval", "radius", "charge" }, Does.Contain(knob.Key), $"{gun.Key}.{knob.Key}");
            }
        }
    }
}
