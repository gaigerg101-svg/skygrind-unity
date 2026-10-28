// NIGHT19 U3-33.6 (builder W): every gun has a job (design.arsenal, design.forge) through the real loadout, Projectiles
// and real HUSKs on a flat floor: the ported ZAPPER (the chain, the discharge, the heat) and BOUNCER (the lob, the burst,
// the airburst) with the web's numbers; the THUNDERCLAP's point blank (the falloff, the parry, the compress); the
// combos (POP, LAUNCH + SNIPE, CONDUCTOR, NAILBOMB, SWAP-CANCEL), each a `combo` event; the FORGE's sidegrade; the box's
// ports; and the off switch (NIGHT18's guns with design.arsenal off).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class ArsenalN19Tests
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
            public readonly Health health;
            public readonly Loadout loadout;
            public readonly Projectiles projectiles;
            public Vec3 eye = new Vec3(0, 1.6, 0);
            public Vec3 dir = new Vec3(0, 0, -1);
            public readonly List<string> log = new List<string>();
            public readonly List<string> combos = new List<string>();
            public readonly List<HitEvent> hits = new List<HitEvent>();
            public int zapPoints = -1, discharged = -1;

            public Rig(bool arsenalOn, params string[] guns)
            {
                world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
                husks = new Husks(cfg, events, enemies, world, -30);
                mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                health = new Health(100, events, "player");
                loadout = new Loadout(cfg, events, enemies, world, mover, health, () => (eye, dir));
                projectiles = new Projectiles(cfg, events, world, enemies, 3);
                loadout.projectiles = projectiles;
                loadout.n19 = new N19Blocks { arsenal = arsenalOn };
                loadout.freshDamage = false; // design.ultra.freshDamage (the game's): FRESH pays points only
                loadout.Reset(new List<string>(guns), 1);
                foreach (string e in new[] { "fire", "grenadeBlast", "rocketBlast", "parry", "overheat", "airburst", "doubleBlast", "nailDetonate", "kill" })
                {
                    string name = e;
                    events.On(name, _ => log.Add(name));
                }
                events.On<ComboEvent>(N19Events.Combo, c => combos.Add(c.name));
                events.On<ZapEvent>(N19Events.Zap, z => zapPoints = z.count);
                events.On<ZapEvent>(N19Events.Discharge, z => discharged = z.hits);
                events.On<HitEvent>("hit", h => hits.Add(h));
                for (int i = 0; i < 20; i++) mover.Step(default, cfg.Dt); // on the floor
            }

            public Husk Husk(double x, double z, double hp = 1000)
            {
                Husk h = husks.Spawn(new Vec3(x, 0, z), new Vec3(0, 0, 0));
                Assert.That(h, Is.Not.Null, "a husk spawns");
                h.hp = h.maxHp = hp;
                return h;
            }

            public void Step(bool fire, bool special = false, int weapon = -1)
            {
                var input = new CombatInput
                {
                    weapon = weapon, fire = new Button { held = fire, pressed = fire }, special = new Button { held = special, pressed = special }, altFire = new Button { held = special, pressed = special },
                };
                loadout.Step(input, cfg.Dt);
                projectiles.Step(cfg.Dt);
                loadout.Step19();
            }

            /// <summary>Steps with the enemies standing (their wind-ups and knockbacks run, they don't chase: a dummy far target).</summary>
            public void StepWorld(bool fire, bool special = false, int weapon = -1)
            {
                Step(fire, special, weapon);
                husks.Step(new Vec3(0, -50, 0), new Health(1e9, events, "dummy"), cfg.Dt);
            }

            public double DamageTo(Enemy e)
            {
                double d = 0;
                foreach (HitEvent h in hits) if (h.enemy == e.id) d += h.damage;
                return d;
            }

            public int Count(string e) => log.FindAll(x => x == e).Count;
        }

        static double N(string path) => cfg.Num(path);

        // --- the ZAPPER ------------------------------------------------------------------------------------------

        [Test]
        public void Zapper_ArcsToTheFirstBody_ThenChainsToTheNearest_EachLinkSeventyPercentOfTheLast()
        {
            var r = new Rig(true, "zapper");
            Husk a = r.Husk(0, -10), b = r.Husk(2, -11), c = r.Husk(4, -12), d = r.Husk(6, -13);
            r.dir = Vec3.Normalize(new Vec3(0, -0.6, -10)); // the body, not the head (x headshotMult)
            r.Step(true);
            Assert.That(r.Count("fire"), Is.EqualTo(1));
            double z = N("weapons.zapper.damage"), f = N("weapons.zapper.chainFalloff");
            Expect.CloseTo(r.DamageTo(a), z, 6);
            Expect.CloseTo(r.DamageTo(b), z * f, 6);
            Expect.CloseTo(r.DamageTo(c), z * f * f, 6);
            Expect.CloseTo(r.DamageTo(d), z * f * f * f, 6);
            Assert.That(r.zapPoints, Is.EqualTo(4), "the arc's points: the first body and 3 links");
        }

        [Test]
        public void Zapper_Discharge_StunsEveryShockedBodyInSight()
        {
            var r = new Rig(true, "zapper");
            Husk a = r.Husk(0, -8), b = r.Husk(2, -9);
            r.Step(true); // shock both
            for (int i = 0; i < 3; i++) r.Step(false);
            r.Step(false, true); // LT: the discharge
            Assert.That(r.discharged, Is.EqualTo(2));
            Assert.That(a.staggerSteps, Is.GreaterThan(0));
            Assert.That(b.staggerSteps, Is.GreaterThan(0));
        }

        [Test]
        public void Zapper_HeldTrigger_OverheatsAfterItsHeatTime()
        {
            var r = new Rig(true, "zapper");
            int steps = 0;
            while (r.Count("overheat") == 0 && steps < 10 * 120)
            {
                r.Step(true);
                steps++;
            }
            Assert.That(steps * cfg.Dt, Is.EqualTo(N("weapons.zapper.heatTime")).Within(0.05));
        }

        // --- the BOUNCER -----------------------------------------------------------------------------------------

        [Test]
        public void Bouncer_LobsAGrenade_ThatBurstsOnABody()
        {
            var r = new Rig(true, "bouncer");
            Husk h = r.Husk(0, -6);
            r.dir = Vec3.Normalize(new Vec3(0, -0.15, -1));
            r.Step(true);
            Assert.That(r.projectiles.Count("grenade"), Is.EqualTo(1));
            for (int i = 0; i < 240 && r.Count("grenadeBlast") == 0; i++) r.Step(false);
            Assert.That(r.Count("grenadeBlast"), Is.EqualTo(1));
            Assert.That(r.DamageTo(h), Is.GreaterThan(N("weapons.bouncer.damage") * 0.3));
        }

        [Test]
        public void Bouncer_Airburst_BurstsEveryGrenadeInFlight()
        {
            var r = new Rig(true, "bouncer");
            r.dir = Vec3.Normalize(new Vec3(0, 0.5, -1));
            r.Step(true);
            for (int i = 0; i < 5; i++) r.Step(false);
            Assert.That(r.projectiles.Count("grenade"), Is.EqualTo(1));
            r.Step(false, true);
            Assert.That(r.Count("airburst"), Is.EqualTo(1));
            Assert.That(r.projectiles.Count("grenade"), Is.EqualTo(0));
        }

        // --- the THUNDERCLAP -------------------------------------------------------------------------------------

        [Test]
        public void Thunderclap_PointBlank_FullPellets_AtTwelveMetres_TheSteepFalloff()
        {
            var near = new Rig(true, "shotgun");
            Husk hn = near.Husk(0, -3);
            near.Step(true);
            var far = new Rig(true, "shotgun");
            Husk hf = far.Husk(0, -12);
            far.Step(true);
            double full = N("weapons.shotgun.pellets") * N("weapons.shotgun.pelletDamage");
            Assert.That(near.DamageTo(hn), Is.GreaterThan(full * 0.6), "most pellets at 3 m, full damage each");
            Assert.That(far.DamageTo(hf), Is.LessThan(near.DamageTo(hn) * 0.5), "past falloffStart the pellets fall off");
        }

        [Test]
        public void Thunderclap_ABlastAtPointBlankOnAWindUp_IsAParry_HealAndStamina()
        {
            var r = new Rig(true, "shotgun");
            Husk h = r.Husk(0, -2);
            h.windupSteps = 30; // mid wind-up
            r.health.hp = 50;
            r.Step(true);
            Assert.That(r.Count("parry"), Is.EqualTo(1), "a shotgun parry");
            Assert.That(r.health.hp, Is.GreaterThan(50), "healed");
            Assert.That(h.windupSteps, Is.EqualTo(0), "the wind-up cancelled");
        }

        [Test]
        public void ThunderclapOff_NoParry_NoFalloff()
        {
            var r = new Rig(false, "shotgun");
            Husk h = r.Husk(0, -2);
            h.windupSteps = 30;
            r.Step(true);
            Assert.That(r.Count("parry"), Is.EqualTo(0), "NIGHT18's THUNDERCLAP has no parry");
        }

        [Test]
        public void Thunderclap_LT_CompressThenRelease_FiresTheDoubleBlast()
        {
            var r = new Rig(true, "shotgun");
            Husk h = r.Husk(0, -4);
            for (int i = 0; i < 60; i++) r.Step(false, true); // hold LT 0.5 s
            Assert.That(r.loadout.Compress, Is.GreaterThan(0.5));
            r.Step(false, false); // release
            Assert.That(r.Count("doubleBlast"), Is.EqualTo(1));
            Assert.That(r.DamageTo(h), Is.GreaterThan(0));
        }

        // --- the combos ------------------------------------------------------------------------------------------

        [Test]
        public void Pop_AKestrelShotOnALiveRocket_BurstsItAtTwiceTheRadius()
        {
            var r = new Rig(true, "rocket", "revolver");
            double radius = -1;
            r.events.On<BlastEvent>("rocketBlast", b => radius = b.radius);
            r.Step(true);
            Assert.That(r.projectiles.Count("rocket"), Is.EqualTo(1));
            r.Step(false, false, 1); // swap to the KESTREL
            for (int i = 0; i < 40 && !r.loadout.ready; i++) r.Step(false);
            for (int i = 0; i < 40 && r.projectiles.Count("rocket") > 0 && !r.combos.Contains(Combos19.Pop); i++) r.Step(true);
            Assert.That(r.combos, Does.Contain(Combos19.Pop));
            Assert.That(radius, Is.EqualTo(N("weapons.rocket.splashRadius") * N("design.arsenal.pop.mult")).Within(0.6));
        }

        [Test]
        public void LaunchSnipe_ACometBlastThrowsAHuskUp_TheNextHitDealsDouble()
        {
            var r = new Rig(true, "rocket", "revolver");
            Husk h = r.Husk(0, -10);
            r.dir = Vec3.Normalize(new Vec3(0, -0.12, -1));
            r.StepWorld(true);
            for (int i = 0; i < 60 && r.Count("rocketBlast") == 0; i++) r.StepWorld(false);
            Assert.That(r.Count("rocketBlast"), Is.EqualTo(1));
            Assert.That(h.Airborne, Is.True, "launched");
            Assert.That(r.loadout.IsLaunched(h), Is.True);
            double before = r.DamageTo(h);
            r.loadout.arsenal.enabled = false; // no CROSS on this hit: the airshot alone
            r.loadout.Damage(h, 10, false, "revolver", r.eye);
            Expect.CloseTo(r.DamageTo(h) - before, 10 * N("design.arsenal.launch.airshotMult"), 6);
            Assert.That(r.combos, Does.Contain(Combos19.LaunchSnipe));
        }

        [Test]
        public void Conductor_AZapperHitOnANailedBody_ArcsToItsNeighbours()
        {
            var r = new Rig(true, "nailgun", "zapper");
            Husk a = r.Husk(0, -10), b = r.Husk(30, -10);
            for (int i = 0; i < 60; i++) r.Step(true); // nails into A
            Assert.That(r.projectiles.StuckOn(a, r.loadout), Is.GreaterThan(0));
            Assert.That(a.ConductiveNails, Is.GreaterThan(0), "the battery shows on the enemy");
            b.position = new Vec3(3, 0, -10); // B beside A, out of the arc's chain line
            r.Step(false, false, 1);
            for (int i = 0; i < 40 && !r.loadout.ready; i++) r.Step(false);
            double bBefore = r.DamageTo(b);
            r.Step(true);
            Assert.That(r.combos, Does.Contain(Combos19.Conductor));
            Assert.That(r.DamageTo(b), Is.GreaterThan(bBefore));
            Assert.That(r.projectiles.StuckOn(a, r.loadout), Is.EqualTo(0), "the nails are spent");
        }

        [Test]
        public void Nailbomb_ACometBlastOnANailedBody_DetonatesItsNails()
        {
            var r = new Rig(true, "nailgun", "rocket");
            Husk a = r.Husk(0, -10);
            for (int i = 0; i < 30; i++) r.Step(true);
            Assert.That(r.projectiles.StuckOn(a, r.loadout), Is.GreaterThan(0));
            r.Step(false, false, 1);
            for (int i = 0; i < 40 && !r.loadout.ready; i++) r.Step(false);
            r.Step(true);
            for (int i = 0; i < 60 && r.Count("rocketBlast") == 0; i++) r.Step(false);
            Assert.That(r.combos, Does.Contain(Combos19.Nailbomb));
            Assert.That(r.Count("nailDetonate"), Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void SwapCancel_FireSwapAndHitWithinTheWindow_PaysTheCombo()
        {
            var r = new Rig(true, "shotgun", "revolver");
            Husk h = r.Husk(0, -4);
            r.Step(true); // THUNDERCLAP
            r.Step(false, false, 1); // swap to the KESTREL at once
            for (int i = 0; i < 40 && !r.loadout.ready; i++) r.Step(false);
            r.Step(true);
            Assert.That(r.combos, Does.Contain(Combos19.SwapCancel));
        }

        [Test]
        public void ArsenalOff_ThePortsDontFire_AndNoComboEvents()
        {
            var r = new Rig(false, "zapper", "shotgun");
            r.Husk(0, -6);
            for (int i = 0; i < 30; i++) r.Step(true);
            Assert.That(r.Count("fire"), Is.EqualTo(0), "NIGHT18's loadout fires no ZAPPER");
            Assert.That(r.combos.Count, Is.EqualTo(0));
        }

        // --- the FORGE and the box -------------------------------------------------------------------------------

        [Test]
        public void ForgeSidegrade_AFlatMultForEveryGun_NoFamilyBonus()
        {
            var forge = new Forge(cfg, new EventBus());
            forge.Reset(1);
            forge.Give("revolver");
            forge.Give("rocket");
            forge.n19Sidegrade = true;
            Expect.CloseTo(forge.Mult("revolver", true, null), N("design.forge.mult"), 9); // no kinetic headshot x1.25
            Expect.CloseTo(forge.Mult("rocket", false, null), N("design.forge.mult"), 9);
            Assert.That(forge.Mult("shotgun", false, null), Is.EqualTo(1), "not forged");
            forge.n19Sidegrade = false;
            Assert.That(forge.Mult("revolver", true, null), Is.EqualTo(N("forge.damage") * N("forge.families.kinetic.headshot")).Within(1e-9), "the web's forge");
        }

        [Test]
        public void Box_ThePortsJoinThePool_OnlyWithTheArsenalBlock()
        {
            var box = new MysteryBox(cfg, new EventBus(), null, WeaponCatalog.From(cfg), null);
            box.Reset(1);
            System.Func<string, bool> fires = w => Loadout.Fires(w) || Loadout.IsPort19(w);
            var owned = new List<string> { "revolver" };
            int off = box.PoolCount(owned, fires);
            box.ports = true;
            int on = box.PoolCount(owned, fires);
            Assert.That(on - off, Is.EqualTo(2), "ZAPPER and BOUNCER");
        }
    }
}
