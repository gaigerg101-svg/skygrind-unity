// NIGHT7 S5: the player's combat against a real Husk on a floor (TriangleWorld). The KESTREL deals its config damage to
// the body and x headshotMult to the head (x ads.headshotMult more zoomed), a wall in the way stops the shot, the
// PULSE GAUNTLET jabs the nearest body in its cone, a punch during the husk's wind-up is a parry (stagger, heal,
// stamina), a kill emits hit + kill and pays points.
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class CombatTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        sealed class Rig
        {
            public EventBus events = new EventBus();
            public EnemyRegistry enemies = new EnemyRegistry();
            public TriangleWorld world = new TriangleWorld();
            public Mover mover;
            public Health health;
            public Loadout loadout;
            public Husks husks;
            public Economy economy;
            public Vec3 eye = new Vec3(0, 1.6, 0);
            public Vec3 dir = new Vec3(0, 0, -1);
        }

        static Rig Make(bool wall = false)
        {
            var r = new Rig();
            r.world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
            if (wall) r.world.AddBox(new Vec3(-3, 0, -3.2), new Vec3(3, 4, -3));
            r.mover = new Mover(cfg.movement, r.world);
            r.mover.Reset(0, 0, 0);
            r.health = new Health(100, r.events, "player");
            r.loadout = new Loadout(cfg, r.events, r.enemies, r.world, r.mover, r.health, () => (r.eye, r.dir));
            r.loadout.arsenal.enabled = false; // NIGHT9: base damage, as the web sandbox measures it (main.js: arsenal off); ArsenalTests covers the arsenal
            r.husks = new Husks(cfg, r.events, r.enemies, r.world, -30);
            r.economy = new Economy(cfg, r.events);
            r.economy.Start();
            return r;
        }

        static void Fire(Rig r, double dt)
        {
            var input = new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } };
            r.loadout.Step(input, dt);
        }

        [Test]
        public void Kestrel_BodyHeadAndZoomedHead()
        {
            double dt = cfg.Dt;
            Rig r = Make();
            Husk husk = r.husks.Spawn(new Vec3(0, 0, -6), new Vec3(0, 0, 0));
            Assert.That(husk, Is.Not.Null);
            // Body: aim at the chest (below the head sphere)
            r.dir = Vec3.Normalize(new Vec3(0, husk.AimHeight - 1.6, -6));
            Fire(r, dt);
            Assert.That(husk.hp, Is.EqualTo(cfg.Num("husk.hp") - cfg.Num("weapons.revolver.damage")));
            // Head: aim at the head centre (hunched forward, toward the player)
            Vec3 head = husk.HeadCenter;
            r.dir = Vec3.Normalize(head - r.eye);
            husk.hp = 1000;
            for (int i = 0; i < 60; i++) r.loadout.Step(new CombatInput { weapon = -1 }, dt); // the cooldown
            Fire(r, dt);
            Assert.That(1000 - husk.hp, Is.EqualTo(cfg.Num("weapons.revolver.damage") * cfg.Num("weapons.revolver.headshotMult")));
            // Zoomed head: ADS is on after a step with right mouse held (the hip spread goes too)
            husk.hp = 1000;
            for (int i = 0; i < 60; i++) r.loadout.Step(new CombatInput { weapon = -1, altFire = new Button { held = true } }, dt);
            Assert.That(r.loadout.adsActive, Is.True);
            Assert.That(r.mover.walkMult, Is.EqualTo(cfg.Num("ads.walkMult")));
            r.loadout.Step(new CombatInput { weapon = -1, altFire = new Button { held = true }, fire = new Button { held = true, pressed = true } }, dt);
            Assert.That(1000 - husk.hp, Is.EqualTo(cfg.Num("weapons.revolver.damage") * cfg.Num("weapons.revolver.headshotMult") * cfg.Num("ads.headshotMult")).Within(1e-9));
        }

        [Test]
        public void AWall_StopsTheShot()
        {
            Rig r = Make(wall: true);
            Husk husk = r.husks.Spawn(new Vec3(0, 0, -6), new Vec3(0, 0, 0));
            r.dir = Vec3.Normalize(new Vec3(0, husk.AimHeight - 1.6, -6));
            Fire(r, cfg.Dt);
            Assert.That(husk.hp, Is.EqualTo(cfg.Num("husk.hp")));
        }

        [Test]
        public void Gauntlet_Jab_AndAKillPays()
        {
            double dt = cfg.Dt;
            Rig r = Make();
            r.loadout.SetSlot(r.loadout.owned.IndexOf("gauntlet"));
            Assert.That(r.loadout.Weapon, Is.EqualTo("gauntlet"));
            Husk husk = r.husks.Spawn(new Vec3(0, 0, -1.8), new Vec3(0, 0, 0));
            r.dir = Vec3.Normalize(new Vec3(0, husk.AimHeight - 1.6, -1.8));
            int kills = 0;
            r.events.On<KillEvent>("kill", _ => kills++);
            Fire(r, dt);
            Assert.That(husk.hp, Is.EqualTo(cfg.Num("husk.hp") - cfg.Num("weapons.gauntlet.damage")));
            double before = r.economy.total;
            for (int i = 0; i < 120 && husk.alive; i++)
            {
                husk.position = new Vec3(0, 0, -1.8); // keep it in reach (no knockback drift)
                Fire(r, dt);
            }
            Assert.That(husk.alive, Is.False);
            Assert.That(kills, Is.EqualTo(1));
            Assert.That(r.economy.total - before, Is.EqualTo(cfg.Num("points.punchKill")), "a melee kill pays the punch-kill points");
        }

        [Test]
        public void Thunderclap_BoughtGoesInHand_PelletsGroupPerEnemy_AirJump()
        {
            double dt = cfg.Dt;
            Rig r = Make();
            Assert.That(r.loadout.AddWeapon("shotgun"), Is.True);
            for (int i = 0; i < 60; i++) r.loadout.Step(new CombatInput { weapon = -1 }, dt); // the swap
            Assert.That(r.loadout.Weapon, Is.EqualTo("shotgun"), "a bought weapon goes in hand");
            Husk husk = r.husks.Spawn(new Vec3(0, 0, -5), new Vec3(0, 0, 0));
            husk.hp = 1000;
            r.dir = Vec3.Normalize(new Vec3(0, husk.AimHeight - 1.6, -5));
            int hits = 0;
            r.events.On<HitEvent>("hit", _ => hits++);
            Fire(r, dt);
            double dealt = 1000 - husk.hp;
            double pellet = cfg.Num("weapons.shotgun.pelletDamage");
            Assert.That(hits, Is.EqualTo(1), "one hit event per enemy per shot");
            Assert.That(dealt, Is.GreaterThan(0).And.LessThanOrEqualTo(pellet * cfg.Num("weapons.shotgun.pellets")));
            Assert.That(dealt % pellet, Is.EqualTo(0));
            // Airborne: the shot pushes you opposite the aim by jumpSpeed.
            for (int i = 0; i < 200; i++) r.loadout.Step(new CombatInput { weapon = -1 }, dt);
            r.mover.Launch(5);
            Vec3 before = r.mover.velocity;
            r.dir = new Vec3(0, -1, 0);
            Fire(r, dt);
            Assert.That(r.mover.velocity.y - before.y, Is.EqualTo(cfg.Num("weapons.shotgun.jumpSpeed")).Within(1e-9), "the shotgun jump");
        }

        [Test]
        public void APunchInTheWindUp_IsAParry()
        {
            double dt = cfg.Dt;
            Rig r = Make();
            Husk husk = r.husks.Spawn(new Vec3(0, 0, -1.6), new Vec3(0, 0, 0));
            husk.windupSteps = 30; // mid wind-up
            r.health.hp = 50;
            r.mover.pips = 0;
            r.dir = Vec3.Normalize(new Vec3(0, husk.AimHeight - 1.6, -1.6));
            int parries = 0;
            r.events.On<HitEvent>("parry", _ => parries++);
            r.loadout.Step(new CombatInput { weapon = -1, punch = new Button { held = true, pressed = true } }, dt);
            Assert.That(parries, Is.EqualTo(1));
            Assert.That(husk.windupSteps, Is.EqualTo(0), "the wind-up is cancelled");
            Assert.That(husk.staggerSteps, Is.GreaterThan(0));
            Assert.That(r.health.hp, Is.EqualTo(50 + cfg.Num("punch.parryHeal")));
            Assert.That(r.mover.pips, Is.EqualTo(r.mover.MaxPips), "a parry refills stamina");
            Assert.That(husk.hp, Is.EqualTo(cfg.Num("husk.hp") - cfg.Num("punch.damage")));
        }
    }
}
