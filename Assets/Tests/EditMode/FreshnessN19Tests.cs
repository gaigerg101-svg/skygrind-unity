// NIGHT19 U3-33.6 (builder W): freshness on points only (design.freshness, Core/Guns/Freshness19.cs): ULTRAKILL's table
// (FRESH x1.5, USED x1, STALE x0.5, DULL x0.25), the per-shot cost by the gun's interval (every gun FRESH -> STALE after
// about 3.6 s of fire), the gain to the other guns and the holstered regen, the FRESH floor with one gun owned, and S7's
// test: the damage is identical with the block on and off (only the points' multiplier moves).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class FreshnessN19Tests
    {
        static GameConfig cfg;
        static Freshness19Params p;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            p = Freshness19Params.From(cfg, WeaponCatalog.From(cfg));
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

            public Rig(bool freshOn, params string[] guns)
            {
                world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
                husks = new Husks(cfg, events, enemies, world, -30);
                mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (new Vec3(0, 1.6, 0), new Vec3(0, 0, -1)));
                loadout.freshDamage = false; // design.ultra.freshDamage (the game's default): FRESH never on damage
                loadout.n19 = new N19Blocks { freshness = freshOn };
                loadout.Reset(new List<string>(guns), 1);
                events.On<HitEvent>("hit", h => hits.Add(h));
            }

            public void Fire(int steps)
            {
                for (int i = 0; i < steps; i++) loadout.Step(new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } }, cfg.Dt);
            }
        }

        [Test]
        public void TheTable_IsUltrakillsWithDullAtAQuarter()
        {
            Assert.That(Freshness19.Mult(10, p), Is.EqualTo(1.5));
            Assert.That(Freshness19.Mult(7.5, p), Is.EqualTo(1.5));
            Assert.That(Freshness19.Mult(7.4, p), Is.EqualTo(1.0));
            Assert.That(Freshness19.Mult(5, p), Is.EqualTo(1.0));
            Assert.That(Freshness19.Mult(4.9, p), Is.EqualTo(p.staleMult)); // NIGHT20 (builder G, S7): ULTRAKILL's 0.5 -> design.freshness.staleMult 0.6
            Assert.That(Freshness19.Mult(1, p), Is.EqualTo(p.staleMult));
            Assert.That(Freshness19.Mult(0.9, p), Is.EqualTo(0.25));
            Assert.That(Freshness19.Tier(0, p), Is.EqualTo("DULL"));
        }

        [Test]
        public void EveryGun_GoesFreshToStale_AfterAboutTheSameSecondsOfFire()
        {
            foreach (string id in new[] { "revolver", "shotgun", "nailgun" })
            {
                double v = p.max, t = 0, iv = p.Interval(id);
                while (v >= p.usedAt)
                {
                    v = Freshness19.Spend(v, iv, 1, 0, p);
                    t += iv;
                }
                // NIGHT20 (builder G): the seconds follow design.freshness.shotCost (NIGHT19's 1.4: 3.0-4.6 s around 3.6 s);
                // every gun within the same band whatever its rate is the rule.
                double expect = (p.max - p.usedAt) / p.shotCost;
                Assert.That(t, Is.InRange(expect * 0.84, expect * 1.29), $"{id}: STALE after {t:0.00} s of fire (about {expect:0.00} s)");
            }
        }

        [Test]
        public void AKestrelHeld_GoesStale_TheOtherGunsGain_AndTheFloorHoldsWithOneGun()
        {
            var r = new Rig(true, "revolver", "shotgun", "gauntlet");
            r.loadout.arsenal.DebugSetFresh("shotgun", 1);
            r.Fire(120 * 5); // 5 s of KESTREL fire
            Assert.That(r.loadout.arsenal.Tier19("revolver"), Is.EqualTo("STALE").Or.EqualTo("DULL"));
            Assert.That(r.loadout.arsenal.Mult19("revolver"), Is.LessThan(1));
            var solo = new Rig(true, "revolver", "gauntlet");
            solo.Fire(120 * 8);
            // NIGHT20 (builder G, S7): the lone gun's floor is design.freshness.oneGunFloor (NIGHT19's FRESH floor held every
            // single gun at x1.5, so rotating could never pay: S7 0.81); it never drops under it.
            Freshness19Params fp = solo.loadout.arsenal.f19;
            Assert.That(solo.loadout.arsenal.Value19("revolver"), Is.EqualTo(fp.oneGunFloor).Within(1e-9), "one gun owned: held at the floor");
            Assert.That(solo.loadout.arsenal.Tier19("revolver"), Is.EqualTo(Freshness19.Tier(fp.oneGunFloor, fp)), "one gun owned: the floor's tier");
        }

        [Test]
        public void HolsteredGuns_RegainHolsterRegenPerSecond()
        {
            var r = new Rig(true, "revolver", "shotgun", "gauntlet");
            r.Fire(120 * 6);
            double v = r.loadout.arsenal.Value19("revolver");
            r.loadout.Step(new CombatInput { weapon = 1 }, cfg.Dt); // the THUNDERCLAP in hand
            for (int i = 0; i < 120 * 2; i++) r.loadout.Step(new CombatInput { weapon = -1 }, cfg.Dt);
            Assert.That(r.loadout.arsenal.Value19("revolver") - v, Is.EqualTo(2 * p.holsterRegen).Within(0.05));
        }

        [Test]
        public void TheDamage_IsIdenticalWithFreshnessOnAndOff_OnlyThePointsMove()
        {
            double Run(bool on, out double lastFresh)
            {
                var r = new Rig(on, "revolver", "shotgun", "gauntlet");
                Husk h = r.husks.Spawn(new Vec3(0, 0, -8), new Vec3(0, 0, 0));
                h.hp = h.maxHp = 1e9;
                r.Fire(120 * 6);
                double total = 0;
                foreach (HitEvent e in r.hits) total += e.damage;
                lastFresh = r.hits[r.hits.Count - 1].freshMult;
                return total;
            }
            double on = Run(true, out double freshOn), off = Run(false, out double freshOff);
            Assert.That(on, Is.EqualTo(off), "damage identical");
            Assert.That(freshOn, Is.Not.EqualTo(freshOff), "the points' multiplier differs");
        }
    }
}
