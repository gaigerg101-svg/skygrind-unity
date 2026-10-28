// NIGHT9 U3-07: the web's tests/gunMath.test.js ported (vitest semantics through Expect): sweepTargets (CombatMath
// .SweepTargets, its port since NIGHT5), the Zapper chain, bounces, the Vortex pull, melee combos + the burst, the
// Thumper wave, melee pay (pointsMath / styleMath, ported NIGHT8 as PointsMath / StyleMath) and the P4 catalog's config.
// Skipped: the catalog test's `expect(MODEL_IDS).toContain(id)` (assets/weaponModels.js is a three.js module; the
// Unity viewmodels are not checked here). The bit-exact replay of gunMath.json is GoldenPorted.cs.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class GunMathTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static BodyTarget Body(double x, double z, double y = 0) => new BodyTarget { basePos = new Vec3(x, y, z), height = 1.8, radius = 0.4 };

        // --- sweepTargets (melee sweeps, the Torch cone) -----------------------------------------------------------

        static readonly Vec3 Origin = new Vec3(0, 1.6, 0);
        static readonly Vec3 Dir = new Vec3(0, 0, -1);

        [Test]
        public void SweepTargets_TakesEveryBodyInReachAndInTheCone_NearestFirst()
        {
            var output = CombatMath.SweepTargets(Origin, Dir, new List<BodyTarget> { Body(0, -2.5), Body(0.6, -1.5), Body(0, 2) }, 3, (45 * Math.PI) / 180);
            Assert.That(output.Select(o => o.target.basePos.z).ToArray(), Is.EqualTo(new[] { -1.5, -2.5 }));
        }

        [Test]
        public void SweepTargets_WidensTheConeByTheBodyRadius_AndIgnoresBodiesOutOfReach()
        {
            var wide = CombatMath.SweepTargets(Origin, Dir, new List<BodyTarget> { Body(1.2, -1.2) }, 3, (35 * Math.PI) / 180); // 45 deg off: 35 + 13.6 deg of body
            Assert.That(wide.Count, Is.EqualTo(1));
            Assert.That(CombatMath.SweepTargets(Origin, Dir, new List<BodyTarget> { Body(0, -6) }, 3, 1).Count, Is.EqualTo(0));
        }

        // --- chainTargets (the Zapper) -----------------------------------------------------------------------------

        static readonly Vec3 Start = new Vec3(0, 1, 0);

        [Test]
        public void ChainTargets_HopsToTheNearestUnhitBodyInRange_EachLink70PercentOfTheLast()
        {
            var points = new List<Vec3> { new Vec3(5, 1, 0), new Vec3(2, 1, 0), new Vec3(20, 1, 0), new Vec3(8, 1, 0) };
            var links = GunMath.ChainTargets(Start, points, 3, 7, 0.7);
            Assert.That(links.Select(l => l.index).ToArray(), Is.EqualTo(new[] { 1, 0, 3 }));
            Assert.That(links.Select(l => JsMath.Round(l.mult * 1000) / 1000).ToArray(), Is.EqualTo(new[] { 0.7, 0.49, 0.343 }));
        }

        [Test]
        public void ChainTargets_StopsWhenNothingIsInRangeOrReachable()
        {
            Assert.That(GunMath.ChainTargets(Start, new List<Vec3> { new Vec3(9, 1, 0) }, 3, 7, 0.7), Is.Empty);
            Assert.That(GunMath.ChainTargets(Start, new List<Vec3> { new Vec3(2, 1, 0) }, 3, 7, 0.7, (i, from) => false), Is.Empty);
        }

        // --- bounces -----------------------------------------------------------------------------------------------

        [Test]
        public void Bounces_AGrenadeBounceFlipsTheNormalPart_AndSlowsTheTangent()
        {
            var v = new Vec3(4, -10, 0);
            GunMath.BounceVelocity(ref v, new Vec3(0, 1, 0), 0.5, 0.7);
            Expect.CloseTo(v.x, 2.8);
            Expect.CloseTo(v.y, 5);
        }

        [Test]
        public void Bounces_ADiscReflectsLikeAMirror_AndKeepsItsSpeed()
        {
            var d = new Vec3(1, 0, -1);
            GunMath.ReflectDir(ref d, new Vec3(0, 0, 1));
            Expect.Exactly(d.x, 1);
            Expect.Exactly(d.y, 0);
            Expect.Exactly(d.z, 1);
            Expect.CloseTo(GunMath.DiscDamage(52, 2, 0.15), 67.6);
        }

        // --- the Vortex pull ---------------------------------------------------------------------------------------

        [Test]
        public void VortexPull_FullSpeedOutsideTheCore_EasesInsideIt_NothingOutsideTheRadius()
        {
            Expect.Exactly(GunMath.PullSpeed(5, 7, 9, 1.5), 9);
            Expect.CloseTo(GunMath.PullSpeed(0.75, 7, 9, 1.5), 4.5);
            Expect.Exactly(GunMath.PullSpeed(8, 7, 9, 1.5), 0);
        }

        // --- melee combos + the burst ------------------------------------------------------------------------------

        [Test]
        public void TheSunderChainGoes0120_AndResetsAfterTheWindow()
        {
            Assert.That(GunMath.NextCombo(-1, 0, 0.7, 3), Is.EqualTo(0));
            Assert.That(GunMath.NextCombo(0, 0.4, 0.7, 3), Is.EqualTo(1));
            Assert.That(GunMath.NextCombo(2, 0.4, 0.7, 3), Is.EqualTo(0));
            Assert.That(GunMath.NextCombo(1, 0.9, 0.7, 3), Is.EqualTo(0));
        }

        [Test]
        public void AThreeRoundBurstFiresAt0_Gap_And2xGap()
        {
            var due = new List<int>();
            for (int s = 0; s < 30; s++) if (GunMath.BurstRoundAt(s, 7, 3) >= 0) due.Add(s);
            Assert.That(due, Is.EqualTo(new[] { 0, 7, 14 }));
        }

        [Test]
        public void TheThumperWaveIsBiggerOutOfASlam()
        {
            var t = ThumperParams.From(cfg);
            ThumperWaveResult normal = GunMath.ThumperWave(false, double.PositiveInfinity, t);
            Assert.That(normal.radius, Is.EqualTo(t.ringRadius));
            Assert.That(normal.big, Is.False);
            ThumperWaveResult big = GunMath.ThumperWave(true, double.PositiveInfinity, t);
            Assert.That(big.radius, Is.EqualTo(t.bigRingRadius));
            Assert.That(big.damage, Is.EqualTo(t.bigRingDamage));
            Assert.That(big.big, Is.True);
            Assert.That(GunMath.ThumperWave(false, 0.1, t).big, Is.True);
        }

        // --- melee pay (POWER P4) ----------------------------------------------------------------------------------

        [Test]
        public void MeleePay_EveryMeleeKillPaysThePunchKillPoints_GunsKeepTheirs()
        {
            PointsMath.Params p = PointsMath.From(cfg);
            // basePoints({type: 'hit', killed: true, weapon, melee}, p): parry false, headshot undefined (false).
            Expect.Exactly(PointsMath.BasePoints(false, true, false, "sunder", true, p), 130);
            Expect.Exactly(PointsMath.BasePoints(false, true, false, "punch", false, p), 130);
            Expect.Exactly(PointsMath.BasePoints(false, true, false, "rattler", false, p), p.kill);
        }

        [Test]
        public void MeleePay_MeleeKillsHaveTheirCallouts_AndATorchTickScoresAShareOfAHit()
        {
            StyleMath.S s = StyleMath.From(cfg);
            var names = StyleMath.KillAwards("husk", "thumper", "GROUND", 2.5, 1, true, false, s).Select(a => a.name).ToList();
            Assert.That(names, Does.Contain("FLATTENED"));
            Expect.CloseTo(StyleMath.HitAwards("torch", false, 0.3, false, s)[0].points, s.baseP["hit"] * 0.3);
        }

        // --- the P4 catalog ----------------------------------------------------------------------------------------

        [Test]
        public void TheP4Catalog_EveryWeaponHasAFamilyAPriceAndAName()
        {
            var catalog = WeaponCatalog.From(cfg);
            List<string> all = cfg.Strs("weapons.all");
            List<string> start = cfg.Strs("weapons.survivalStart");
            foreach (string id in all)
            {
                Assert.That(cfg.Get($"weapons.families.{catalog.Get(id).family}"), Is.TypeOf<double>(), id);
                Assert.That(cfg.Has($"weapons.names.{id}") && !string.IsNullOrEmpty(cfg.Get($"weapons.names.{id}") as string), Is.True, id);
                // Skipped: expect(MODEL_IDS).toContain(id) (assets/weaponModels.js, three.js).
                if (!start.Contains(id)) Assert.That(cfg.Num($"weapons.prices.{id}"), Is.GreaterThan(0), id);
            }
            Assert.That(all.Count, Is.EqualTo(18)); // POWER P4 sixteen + the OVERNIGHT2 N2 CHOIR and PRISM
        }
    }
}
