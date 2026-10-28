// NIGHT19 U3-33.7 (builder E): the SHIELDED affix (affixes.js, Core/Enemies/Affixes.cs): rollAffixes on a scripted
// stream (none before affixes.from; one at `chance`; the second from twoFrom never repeats the first), frontHit's
// half-angle, and the damage filter EnemyHits.Filter: a front body hit is blocked (affixBlock, nothing lands), a front
// headshot breaks the plate and spends itself (affixBreak + plateBreak, 0 lands), a front melee hit breaks it and lands
// in full, a hit from behind lands and keeps the plate, an enemy without a plate gets every hit unchanged. Affixes.Assign
// drops the affixes the port doesn't play (SHIELDED only tonight) and the kill bonus is bonusPoints per ported affix.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Core.Tests
{
    public class AffixN19Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")), "config.json"));

        static Func<double> Script(params double[] values)
        {
            int i = 0;
            return () => values[i++];
        }

        [Test]
        public void Roll_IsRollAffixes()
        {
            AffixMath.P a = AffixMath.P.From(Cfg);
            var output = new List<string>();
            int from = (int)a.from;
            Assert.That(AffixMath.Roll(from - 1, Script(), a, output), Is.Empty, "none before affixes.from (no roll drawn)");
            Assert.That(AffixMath.Roll(from, Script(a.chance - 0.01, 0.0), a, output), Is.EqualTo(new[] { "shielded" }));
            Assert.That(AffixMath.Roll(from, Script(a.chance + 0.01), a, output), Is.Empty, "the roll over the chance");
            Assert.That(AffixMath.Roll(from, Script(0.0, 0.99), a, output), Is.EqualTo(new[] { "swift" }));
            int two = (int)a.twoFrom;
            // first 'blinker' (index 2), the second roll picks from the four others: 0.5 x 4 = 2 -> 'regenerating'
            Assert.That(AffixMath.Roll(two, Script(0.0, 0.4, 0.0, 0.5), a, output), Is.EqualTo(new[] { "blinker", "regenerating" }));
            Assert.That(AffixMath.Roll(two, Script(0.0, 0.0, 0.0, 0.0), a, output), Is.EqualTo(new[] { "shielded", "volatile" }), "never the first again");
            double cap = Math.Min(a.maxChance, a.chance + a.perRound * (40 - a.from));
            Assert.That(cap, Is.EqualTo(a.maxChance), "the chance stops at maxChance");
        }

        [Test]
        public void FrontHit_IsWithinTheHalfAngle()
        {
            double half = Cfg.Num("affixes.kinds.shielded.halfDeg");
            var at = new Vec3(0, 0, 0);
            double yaw = 0; // facing -z
            Assert.That(AffixMath.FrontHit(at, yaw, new Vec3(0, 1, -5), half), Is.True, "dead ahead");
            Assert.That(AffixMath.FrontHit(at, yaw, new Vec3(0, 1, 5), half), Is.False, "behind");
            double inside = (half - 5) * Math.PI / 180, outside = (half + 5) * Math.PI / 180;
            Assert.That(AffixMath.FrontHit(at, yaw, new Vec3(-Math.Sin(inside) * 5, 1, -Math.Cos(inside) * 5), half), Is.True);
            Assert.That(AffixMath.FrontHit(at, yaw, new Vec3(-Math.Sin(outside) * 5, 1, -Math.Cos(outside) * 5), half), Is.False);
        }

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly List<string> said = new List<string>();
            public readonly Husk husk;
            public readonly Affixes affixes = new Affixes(Cfg);

            public Rig(bool shielded)
            {
                var world = new TriangleWorld();
                world.AddBox(new Vec3(-40, -1, -40), new Vec3(40, 0, 40));
                var pool = new Husks(Cfg, events, new EnemyRegistry(), world, -30);
                husk = pool.Spawn("husk", new Vec3(0, 0, 0), new Vec3(0, 0, -10)); // faces -z, toward (0, 0, -10)
                if (shielded) affixes.Assign(husk, new[] { "shielded" });
                foreach (string n in new[] { EnemyHits.BlockEvent, EnemyHits.BreakEvent, "plateBreak" })
                {
                    string name = n;
                    events.On(name, _ => said.Add(name));
                }
            }

            public double Hit(double amount, bool headshot, string weapon, bool melee, Vec3 from, out bool blocked) =>
                EnemyHits.Filter(husk, amount, headshot, weapon, melee, from, events, out blocked);
        }

        static readonly Vec3 Front = new Vec3(0, 1.6, -8), Behind = new Vec3(0, 1.6, 8);

        [Test]
        public void AFrontBodyHit_IsBlocked()
        {
            var r = new Rig(true);
            Assert.That(r.husk.Shielded, Is.True);
            Assert.That(r.Hit(25, false, "revolver", false, Front, out bool blocked), Is.EqualTo(0));
            Assert.That(blocked, Is.True, "no hit at all");
            Assert.That(r.husk.Shielded, Is.True, "the plate holds");
            Assert.That(r.said, Is.EqualTo(new[] { EnemyHits.BlockEvent }));
        }

        [Test]
        public void AFrontHeadshot_BreaksThePlate_AndSpendsItself()
        {
            var r = new Rig(true);
            Assert.That(r.Hit(25, true, "revolver", false, Front, out bool blocked), Is.EqualTo(0));
            Assert.That(blocked, Is.False, "a hit (0 lands)");
            Assert.That(r.husk.Shielded, Is.False);
            Assert.That(r.said, Is.EqualTo(new[] { EnemyHits.BreakEvent, "plateBreak" }));
            Assert.That(r.Hit(25, false, "revolver", false, Front, out _), Is.EqualTo(25), "the next body hit lands");
        }

        [Test]
        public void AFrontMeleeHit_BreaksThePlate_AndLands()
        {
            var r = new Rig(true);
            Assert.That(r.Hit(10, false, "punch", true, Front, out bool blocked), Is.EqualTo(10));
            Assert.That(blocked, Is.False);
            Assert.That(r.husk.Shielded, Is.False);
        }

        [Test]
        public void AHitFromBehind_Lands_AndKeepsThePlate()
        {
            var r = new Rig(true);
            Assert.That(r.Hit(25, false, "revolver", false, Behind, out bool blocked), Is.EqualTo(25));
            Assert.That(blocked, Is.False);
            Assert.That(r.husk.Shielded, Is.True);
            Assert.That(r.said, Is.Empty);
        }

        [Test]
        public void WithoutAPlate_EveryHitIsUnchanged()
        {
            var r = new Rig(false);
            foreach (double amount in new[] { 0.1, 25.0, 1e6 })
            {
                Assert.That(r.Hit(amount, false, "revolver", false, Front, out bool b), Is.EqualTo(amount));
                Assert.That(b, Is.False);
                Assert.That(r.Hit(amount, true, "revolver", false, Front, out _), Is.EqualTo(amount));
            }
        }

        [Test]
        public void Assign_PlaysShieldedOnly_AndTheKillBonusCountsIt()
        {
            var r = new Rig(false);
            r.affixes.Assign(r.husk, new[] { "volatile", "shielded" });
            Assert.That(r.husk.Shielded, Is.True);
            Assert.That(r.affixes.dropped, Is.EqualTo(1), "VOLATILE is not ported tonight");
            Assert.That(r.affixes.OnKill(r.husk.id), Is.EqualTo(Cfg.Num("affixes.bonusPoints")));
            Assert.That(r.affixes.OnKill(r.husk.id), Is.EqualTo(0), "paid once");
        }
    }
}
