// NIGHT10 U3-13.4: the ARMORY's STITCHER (nailgun), COMET (rocket) and LONGWAVE (rail) through the real loadout,
// Projectiles and a real Husk on a floor, as weapons.js / projectiles.js fire them. The p19 vectors check projectiles.js
// and fireRail bit for bit; these check the loadout's wiring end to end: the heat meter (the web's weapons.test.js heat
// numbers, through the loadout), nails that stick and detonate, a rocket's splash and its rocket jump, the rail's charge,
// cancel, pierce and kill, the special on right mouse (ads.js routeSpecial) and the QUICK perk's charge time.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class ArmoryTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Husks husks;
            public readonly Mover mover;
            public readonly Loadout loadout;
            public readonly Projectiles projectiles;
            public Vec3 eye = new Vec3(0, 1.6, 0);
            public Vec3 dir = new Vec3(0, 0, -1);
            public readonly List<string> log = new List<string>();
            public readonly List<HitEvent> hits = new List<HitEvent>();

            public Rig(string weapon)
            {
                world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
                husks = new Husks(cfg, events, enemies, world, -30);
                mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (eye, dir));
                projectiles = new Projectiles(cfg, events, world, enemies, 3);
                loadout.projectiles = projectiles;
                loadout.Reset(new List<string> { weapon }, 1);
                loadout.arsenal.enabled = false; // base damage, as the web sandbox measures it
                foreach (string e in new[] { "fire", "overheat", "railCharge", "railCancel", "railFire", "rocketBlast", "rocketJump", "nailDetonate", "kill" })
                {
                    string name = e;
                    events.On(name, _ => log.Add(name));
                }
                events.On<HitEvent>("hit", h => hits.Add(h));
            }

            public Husk Husk(double x, double z, double hp)
            {
                Husk h = husks.Spawn(new Vec3(x, 0, z), new Vec3(0, 0, 0));
                h.hp = h.maxHp = hp;
                return h;
            }

            public void Step(bool fire, bool special = false, bool alt = false)
            {
                var input = new CombatInput
                {
                    weapon = -1, fire = new Button { held = fire, pressed = fire }, special = new Button { held = special, pressed = special }, altFire = new Button { held = alt, pressed = alt },
                };
                loadout.Step(input, cfg.Dt);
                projectiles.Step(cfg.Dt);
            }

            public int Count(string e) => log.FindAll(x => x == e).Count;
        }

        [Test]
        public void Stitcher_FiresAtItsRate_HeatsUp_OverheatsAfterHeatTime_AndLocks()
        {
            var r = new Rig("nailgun");
            int steps = 0;
            while (r.Count("overheat") == 0 && steps < 10 * 120)
            {
                r.Step(true);
                steps++;
            }
            Assert.That(r.Count("overheat"), Is.EqualTo(1));
            Assert.That(steps * cfg.Dt, Is.EqualTo(cfg.Num("weapons.nailgun.heatTime")).Within(0.05), "overheats after heatTime s of fire");
            Assert.That(r.loadout.Overheated, Is.True);
            double rate = r.Count("fire") / (steps * cfg.Dt);
            Assert.That(rate, Is.EqualTo(cfg.Num("weapons.nailgun.rate")).Within(0.5), "nails per second");
            int fired = r.Count("fire");
            for (int i = 0; i < 60; i++) r.Step(true); // holding through the lock fires nothing
            Assert.That(r.Count("fire"), Is.EqualTo(fired));
            Assert.That(r.loadout.Meter(out double heat, out _, out bool locked), Is.True);
            Assert.That(locked, Is.True);
            Assert.That(heat, Is.GreaterThan(0));
        }

        [Test]
        public void Stitcher_NailsStickInAHusk_AndRightMouseDetonatesThem()
        {
            var r = new Rig("nailgun");
            Husk h = r.Husk(0, -8, 1e6);
            for (int i = 0; i < 30; i++) r.Step(true); // a burst of nails
            for (int i = 0; i < 30; i++) r.Step(false); // let them land
            int nails = r.hits.Count;
            Assert.That(nails, Is.GreaterThan(0), "nails hit the husk");
            Assert.That(r.hits[0].damage, Is.EqualTo(cfg.Num("weapons.nailgun.damage")));
            Assert.That(r.projectiles.StuckCount(r.loadout), Is.EqualTo(nails), "each nail that didn't kill stuck");
            r.hits.Clear();
            r.Step(false, alt: true); // right mouse is the special on a gun that doesn't zoom
            Assert.That(r.Count("nailDetonate"), Is.EqualTo(1));
            Assert.That(r.loadout.detonated, Is.EqualTo(nails));
            Assert.That(r.projectiles.StuckCount(r.loadout), Is.EqualTo(0));
            Assert.That(r.hits.Count, Is.EqualTo(nails), "one detonation hit per stuck nail");
            Assert.That(r.hits[0].damage, Is.EqualTo(cfg.Num("weapons.specials.nailgun.detonateDamage")));
        }

        [Test]
        public void Comet_RocketSplashKillsAHusk_AndABlastAtYourFeetIsARocketJump()
        {
            var r = new Rig("rocket");
            Husk h = r.Husk(0, -10, cfg.Num("husk.hp"));
            r.Step(true);
            Assert.That(r.Count("fire"), Is.EqualTo(1));
            for (int i = 0; i < 120 && h.alive; i++) r.Step(false);
            Assert.That(r.Count("rocketBlast"), Is.EqualTo(1));
            Assert.That(h.alive, Is.False, "70 at the centre kills a husk");
            Assert.That(r.Count("kill"), Is.EqualTo(1));
            // At your feet: the knockback lifts you (0 self damage).
            r.dir = Vec3.Normalize(new Vec3(0, -1, -0.05));
            for (int i = 0; i < 120; i++) r.Step(false); // the cooldown
            r.Step(true);
            for (int i = 0; i < 30 && r.Count("rocketJump") == 0; i++) r.Step(false);
            Assert.That(r.Count("rocketJump"), Is.EqualTo(1));
            Assert.That(r.mover.velocity.y, Is.GreaterThan(0), "thrown up");
        }

        [Test]
        public void Longwave_ChargesFor0_6s_ThenPiercesAndKills_AReleaseCancels()
        {
            var r = new Rig("rail");
            Husk a = r.Husk(0, -8, cfg.Num("husk.hp"));
            Husk b = r.Husk(0, -14, cfg.Num("husk.hp"));
            // A short hold cancels.
            for (int i = 0; i < 20; i++) r.Step(true);
            r.Step(false);
            Assert.That(r.Count("railCancel"), Is.EqualTo(1));
            Assert.That(r.Count("railFire"), Is.EqualTo(0));
            // A full hold fires by itself when full (unzoomed).
            int steps = 0;
            while (r.Count("railFire") == 0 && steps < 200)
            {
                r.Step(true);
                steps++;
            }
            Assert.That(steps * cfg.Dt, Is.EqualTo(cfg.Num("weapons.rail.charge")).Within(cfg.Dt * 1.5));
            Assert.That(a.alive || b.alive, Is.False, "150 through both husks in line");
            Assert.That(r.Count("kill"), Is.EqualTo(2));
            Assert.That(r.loadout.CooldownFractionOf("rail"), Is.GreaterThan(0.9), "the cooldown starts");
        }

        [Test]
        public void Longwave_QuickPerkShortensTheCharge()
        {
            var r = new Rig("rail");
            r.loadout.chargeMult = cfg.Num("perks.quick.chargeMult");
            int steps = 0;
            while (r.Count("railFire") == 0 && steps < 200)
            {
                r.Step(true);
                steps++;
            }
            Assert.That(steps * cfg.Dt, Is.EqualTo(cfg.Num("weapons.rail.charge") * cfg.Num("perks.quick.chargeMult")).Within(cfg.Dt * 1.5));
        }
    }
}
