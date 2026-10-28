// NIGHT9 U3-07: the web's tests/weapons.test.js ported (every test there is on the pure weaponMath.js): nailgun heat,
// rocket splash and knockback through WeaponMath, and the perks' numbers through PerkMath.Effects (Perks.cs, where
// weaponMath.js's perkEffects was ported on NIGHT8) with WeaponMods.Of for the `weapons` part. Nothing skipped. The
// bit-exact replay of weaponMath.json is GoldenPorted.cs.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class WeaponMathTests
    {
        static GameConfig cfg;
        static NailgunParams n;
        static double dt;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            n = NailgunParams.From(cfg);
            dt = 1 / cfg.Num("loop.simHz");
        }

        // --- nailgun heat (SURVIVAL §4) ----------------------------------------------------------------------------

        [Test]
        public void OverheatsAfter3sOfFire_LocksFor1_5s_ThenFiresAgain()
        {
            var h = new HeatState();
            double t = 0;
            while (!WeaponMath.HeatStep(h, true, dt, n) && t < 10) t += dt;
            Expect.CloseTo(t + dt, n.heatTime, 1);
            Expect.Exactly(h.@lock, n.lockTime);
            double locked = 0;
            while (h.@lock > 0)
            {
                WeaponMath.HeatStep(h, true, dt, n); // holding the trigger through the lock does nothing
                locked += dt;
            }
            Expect.CloseTo(locked, n.lockTime, 1);
            Expect.Exactly(h.heat, 0);
        }

        [Test]
        public void CoolsWhileNotFiring()
        {
            var h = new HeatState { heat = 0.5, @lock = 0 };
            WeaponMath.HeatStep(h, false, 0.25, n);
            Expect.CloseTo(h.heat, 0.5 - n.coolRate * 0.25, 6);
        }

        // --- rockets (SURVIVAL §4) ---------------------------------------------------------------------------------

        static double SplashRadius => cfg.Num("weapons.rocket.splashRadius");
        static double Damage => cfg.Num("weapons.rocket.damage");
        static double EdgeDamage => cfg.Num("weapons.rocket.edgeDamage");
        static double Knockback => cfg.Num("weapons.rocket.knockback");
        static double MinUp => cfg.Num("weapons.rocket.knockbackMinUp");

        [Test]
        public void Splash_70AtTheCenterFallingTo30PercentAt4m_NothingBeyond()
        {
            Expect.Exactly(WeaponMath.SplashDamage(0, SplashRadius, Damage, EdgeDamage), 70);
            Expect.CloseTo(WeaponMath.SplashDamage(SplashRadius, SplashRadius, Damage, EdgeDamage), 21, 6);
            Expect.Exactly(WeaponMath.SplashDamage(SplashRadius + 0.1, SplashRadius, Damage, EdgeDamage), 0);
        }

        [Test]
        public void ABlastAtYourFeetThrowsYouUpAtUpTo16mps()
        {
            double speed = WeaponMath.BlastKnockback(new Vec3(0, 0, 0), new Vec3(0, 0.9, 0), SplashRadius, Knockback, MinUp, out Vec3 output);
            Expect.CloseTo(speed, Knockback * (1 - 0.9 / SplashRadius), 6);
            Expect.CloseTo(output.y, speed, 6);
            Expect.Exactly(WeaponMath.BlastKnockback(new Vec3(0, 0, 0), new Vec3(0, 0, 0), SplashRadius, Knockback, MinUp, out output), Knockback);
        }

        [Test]
        public void ABlastBesideYouPushesYouAwayButAlwaysPartlyUp_NoneOutsideTheRadius()
        {
            double speed = WeaponMath.BlastKnockback(new Vec3(-2, 0.9, 0), new Vec3(0, 0.9, 0), SplashRadius, Knockback, MinUp, out Vec3 output);
            Assert.That(output.x, Is.GreaterThan(0));
            Expect.CloseTo(output.y / speed, MinUp, 6);
            Expect.CloseTo(JsMath.Hypot(output.x, output.y, output.z), speed, 6);
            Expect.Exactly(WeaponMath.BlastKnockback(new Vec3(-5, 0.9, 0), new Vec3(0, 0.9, 0), SplashRadius, Knockback, MinUp, out output), 0);
        }

        // --- perks (SURVIVAL §4) -----------------------------------------------------------------------------------

        [Test]
        public void MapsOwnedPerksToNumbers()
        {
            PerkEffects none = PerkMath.Effects(new List<string>(), cfg);
            // toMatchObject({bonusPips: 0, regen: null, maxHp: null, bonusRideTime: 0, bonusWallJumps: 0, weapons: {1, 1, 1}})
            Assert.That(none.bonusPips, Is.EqualTo(0));
            Assert.That(none.regen.HasValue, Is.False);
            Assert.That(none.maxHp.HasValue, Is.False);
            Expect.Exactly(none.bonusRideTime, 0);
            Assert.That(none.bonusWallJumps, Is.EqualTo(0));
            WeaponMods w0 = WeaponMods.Of(none);
            Expect.Exactly(w0.fireRate, 1);
            Expect.Exactly(w0.swap, 1);
            Expect.Exactly(w0.charge, 1);

            PerkEffects all = PerkMath.Effects(new List<string> { "vigor", "mend", "plate", "quick", "grip" }, cfg);
            Assert.That(all.bonusPips, Is.EqualTo(1));
            Assert.That(all.regen.HasValue, Is.True);
            Expect.Exactly(all.regen.Value.delay, 0.6);
            Expect.Exactly(all.regen.Value.rate, 1.5);
            Expect.Exactly(all.maxHp.Value, 150);
            Expect.Exactly(all.bonusRideTime, 0.6);
            Assert.That(all.bonusWallJumps, Is.EqualTo(1));
            WeaponMods mods = WeaponMods.Of(all);
            Expect.CloseTo(WeaponMath.FireInterval(0.4, mods), 0.4 / 1.15, 6);
            Expect.CloseTo(WeaponMath.SwapTime(0.25, mods), 0.125, 6);
            Expect.CloseTo(WeaponMath.ChargeTime(0.6, mods), 0.45, 6);
        }
    }
}
