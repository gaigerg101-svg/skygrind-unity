// NIGHT20 U3-34.9 C3 (builder E owns this file; tests first): the rest of affixes.js (VOLATILE, BLINKER, REGENERATING;
// SWIFT's speed waits for the brain's per-enemy pace: N's lines) in Core/Enemies/Affixes.cs, behind design.adds.affixes
// (Affixes.all; off: NIGHT19's SHIELDED only, a rolled other dropped). On a floor with a husk and a still player:
//   1. the row on: every affix is assigned and pays the kill bonus (bonusPoints x its affixes); off: the others dropped
//   2. REGENERATING: out of damage for regenerating.delay s it heals perSecond x its spawn HP a second, to that HP
//   3. BLINKER: it blinks within blinker.every x (0.3 .. 0.7) s of its spawn, a short way (<= distance), `affixBlink`
//   4. VOLATILE: its kill arms a burst (`affixVolatile`: the tell circle of radius, delay) that lands after delay s:
//      damage to a player within radius (none outside), `affixBurst`
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Core.Tests
{
    public class AffixC3Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")), "config.json"));
        static double K(string path) => Cfg.Num("affixes.kinds." + path);
        static double Dt => Cfg.Dt;

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Health health;
            public readonly Husks pool;
            public readonly Affixes affixes;
            public readonly List<string> said = new List<string>();
            public readonly List<double> hurts = new List<double>();
            public Vec3 player = new Vec3(0, 0, 20);

            public Rig(bool all = true)
            {
                world.AddBox(new Vec3(-120, -1, -120), new Vec3(120, 0, 120));
                health = new Health(100000, events, "player");
                pool = new Husks(Cfg, events, enemies, world, -30);
                pool.NewRun(5);
                affixes = new Affixes(Cfg);
                affixes.Wire(events, world, Cfg);
                affixes.NewRun(9);
                affixes.all = all;
                events.On<HurtEvent>("hurt", e => hurts.Add(e.amount));
                foreach (string name in new[] { "affixBlink", "affixVolatile", "affixBurst" })
                {
                    string n = name;
                    events.On(n, _ => said.Add(n));
                }
            }

            public Husk Husk(params string[] list)
            {
                Husk h = pool.Spawn("husk", new Vec3(0, 0, 0), player);
                Assert.That(h, Is.Not.Null);
                affixes.OnSpawn(h, list);
                return h;
            }

            public void Run(double seconds)
            {
                int n = MovementMath.StepsFor(seconds, Dt);
                for (int i = 0; i < n; i++) affixes.Step(player, health, Dt);
            }
        }

        [Test]
        public void TheRowOn_EveryAffixPlays_AndPaysTheKillBonus()
        {
            var on = new Rig();
            Husk h = on.Husk("volatile", "blinker");
            Assert.That(on.affixes.Tracked(h.id), Is.True);
            Assert.That(on.affixes.dropped, Is.EqualTo(0));
            Assert.That(on.affixes.OnKill(h.id), Is.EqualTo(Cfg.Num("affixes.bonusPoints") * 2), "bonusPoints x its two affixes");
            var off = new Rig(false);
            Husk o = off.Husk("volatile", "blinker");
            Assert.That(off.affixes.Tracked(o.id), Is.False, "NIGHT19: SHIELDED only, the others dropped");
            Assert.That(off.affixes.dropped, Is.EqualTo(2));
        }

        [Test]
        public void Swift_SetsItsOwnPace_TheRowOnOnly()
        {
            // NIGHT20 (builder EN): affixes.js assign: e.speedMult = def.mult; the brain reads it as Husk.paceMult.
            var on = new Rig();
            Husk s = on.Husk("swift");
            Husk plain = on.Husk("blinker");
            Assert.That(s.paceMult, Is.EqualTo(K("swift.mult")), "SWIFT: x affixes.kinds.swift.mult");
            Assert.That(plain.paceMult, Is.EqualTo(1), "another affix: its kind's pace");
            var off = new Rig(false);
            Assert.That(off.Husk("swift").paceMult, Is.EqualTo(1), "the row off: SWIFT dropped (NIGHT19)");
        }

        [Test]
        public void Regenerating_HealsBackAfterItsDelay_ToItsSpawnHp()
        {
            var r = new Rig();
            Husk h = r.Husk("regenerating");
            double max = h.hp;
            h.hp = max * 0.5;
            r.events.Emit("hit", new HitEvent { owner = "player", enemy = h.id, damage = 1 });
            r.Run(K("regenerating.delay") - 0.05);
            Assert.That(h.hp, Is.EqualTo(max * 0.5), "nothing inside the calm");
            r.Run(1.05);
            Assert.That(h.hp, Is.EqualTo(max * 0.5 + max * K("regenerating.perSecond")).Within(max * K("regenerating.perSecond") * 0.05 + 1e-9), "perSecond x its spawn HP a second");
            r.Run(30);
            Assert.That(h.hp, Is.EqualTo(max), "back to its spawn HP, never past it");
        }

        [Test]
        public void Blinker_BlinksAShortWay_WithinItsFirstWindow()
        {
            var r = new Rig();
            Husk h = r.Husk("blinker");
            Vec3 at = h.position;
            r.Run(K("blinker.every") * 0.7 + 0.05);
            Assert.That(r.said, Does.Contain("affixBlink"));
            double moved = System.Math.Sqrt((h.position.x - at.x) * (h.position.x - at.x) + (h.position.z - at.z) * (h.position.z - at.z));
            Assert.That(moved, Is.GreaterThan(0.5).And.LessThanOrEqualTo(K("blinker.distance") + 0.01), "a short way");
            int blinks = r.said.FindAll(s => s == "affixBlink").Count;
            r.Run(K("blinker.every") * 1.2 + 0.05);
            Assert.That(r.said.FindAll(s => s == "affixBlink").Count, Is.GreaterThan(blinks), "again within every x 1.2");
        }

        [Test]
        public void Volatile_ItsKillArmsABurst_ThatHurtsInsideItsRadius()
        {
            foreach (double away in new[] { 2.0, 5.0 })
            {
                var r = new Rig();
                r.player = new Vec3(away, 0, 0);
                Husk h = r.Husk("volatile");
                h.alive = false;
                r.affixes.OnKill(h.id);
                Assert.That(r.said, Does.Contain("affixVolatile"), "the tell at once");
                r.Run(K("volatile.delay") - 0.02);
                Assert.That(r.hurts, Is.Empty, "nothing before delay");
                r.Run(0.05);
                Assert.That(r.said, Does.Contain("affixBurst"));
                if (away <= K("volatile.radius")) Assert.That(r.hurts, Is.EqualTo(new[] { K("volatile.damage") }), "inside the radius");
                else Assert.That(r.hurts, Is.Empty, "outside the radius");
            }
        }
    }
}
