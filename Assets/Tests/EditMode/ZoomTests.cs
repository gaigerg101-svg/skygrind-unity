// NIGHT20 (the shared commit places this empty fixture; G owns it): zoom on most guns (design.zoom): each gun's LT is a zoom or its unique alt (the THUNDERCLAP's shockwave, the COMET's steer); the zoom's FOV by the web's optical formula (1.25 -> 62.2 deg, 1.5 -> 53.3, 3.5 -> 24.3 at base 74); the STITCHER's detonate on SPECIAL; zoom ends the sprint; only the 3.5 scope descopes on a hit.
// Builder G (U3-34.3): through the real loadout and Mover on a flat floor, with NIGHT19's config for the gust (design.gust
// on, as shipped) and design.zoom + design.shockwave on (the ZOOM switch MOST: KESTREL 1.5, STITCHER 1.25, LONGWAVE a 3.5
// scope; the ports' zooms from the web's ads.weapons; unique LTs on the THUNDERCLAP and the COMET). Not tested here: "zoom
// ends the sprint" (the sprint lives in Mover.cs, the lead's: a request in G's report).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class ZoomTests
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
            public readonly Health health;
            public readonly Loadout loadout;
            public readonly Projectiles projectiles;
            public Vec3 dir = new Vec3(0, 0, -1);
            public readonly List<ShotEvent> shots = new List<ShotEvent>();

            public Rig(N20Blocks n20, params string[] guns)
            {
                world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
                mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                health = new Health(100, events, "player");
                loadout = new Loadout(cfg, events, enemies, world, mover, health, () => (Eye, dir));
                projectiles = new Projectiles(cfg, events, world, enemies, 3);
                loadout.projectiles = projectiles;
                loadout.n19 = new N19Blocks { gust = true }; // NIGHT19's config as shipped: the KESTREL's gust on
                loadout.n20 = n20;
                loadout.Reset(new List<string>(guns), 1);
                events.On<ShotEvent>("shotResult", s => shots.Add(new ShotEvent { weapon = s.weapon, dir = s.dir }));
                for (int i = 0; i < 30; i++) Step(false, false); // on the floor
            }

            public static N20Blocks Tonight => new N20Blocks { zoom = true, shockwave = true };

            public Vec3 Eye => new Vec3(mover.position.x, mover.position.y + cfg.movement.body.eyeHeight, mover.position.z);

            /// <summary>One fixed step: the mover, then the loadout (LT held / pressed as asked; fire held).</summary>
            public void Step(bool lt, bool fire, InputFrame frame = default, bool ltPressed = false)
            {
                mover.Step(frame, cfg.Dt);
                var input = new CombatInput { weapon = -1, altFire = new Button { held = lt, pressed = ltPressed }, fire = new Button { held = fire, pressed = fire } };
                loadout.Step(input, cfg.Dt);
            }
        }

        [Test]
        public void EachGunsLT_IsAZoomOrItsUniqueAlt()
        {
            var r = new Rig(Rig.Tonight, "revolver");
            Assert.That(r.loadout.ZoomGun("revolver"), Is.EqualTo(1.5), "KESTREL: the web's 1.5");
            Assert.That(r.loadout.ZoomGun("nailgun"), Is.EqualTo(cfg.Num("design.zoom.nailgunMag")), "STITCHER: 1.25");
            Assert.That(r.loadout.ZoomGun("rail"), Is.EqualTo(3.5), "LONGWAVE: the web's 3.5 scope");
            Assert.That(r.loadout.ZoomGun("shotgun"), Is.EqualTo(0), "THUNDERCLAP: the shockwave");
            Assert.That(r.loadout.ZoomGun("rocket"), Is.EqualTo(0), "COMET: steer");
            Assert.That(r.loadout.ZoomGun("gauntlet"), Is.EqualTo(0), "the gauntlet: no zoom");
            var ports = new Rig(new N20Blocks { zoom = true, shockwave = true, arsenal = true }, "revolver");
            Assert.That(ports.loadout.ZoomGun("zapper"), Is.EqualTo(1.3), "ZAPPER: the web's 1.3");
            Assert.That(ports.loadout.ZoomGun("bouncer"), Is.EqualTo(0), "BOUNCER: its airburst (the web never zoomed it)");
        }

        [Test]
        public void TheZoomsFov_IsTheWebsOpticalFormula_AtBase74()
        {
            var r = new Rig(Rig.Tonight, "revolver");
            double baseFov = cfg.Num("camera.fovDeg");
            Assert.That(baseFov, Is.EqualTo(74));
            Assert.That(CombatMath.ZoomedFov(baseFov, r.loadout.ZoomGun("nailgun")), Is.EqualTo(62.2).Within(0.05));
            Assert.That(CombatMath.ZoomedFov(baseFov, r.loadout.ZoomGun("revolver")), Is.EqualTo(53.3).Within(0.05));
            Assert.That(CombatMath.ZoomedFov(baseFov, r.loadout.ZoomGun("rail")), Is.EqualTo(24.3).Within(0.05));
        }

        [Test]
        public void TheStitcher_ZoomsOnLT_HalfTheSpread_ItsDetonateStaysOnSpecial()
        {
            var r = new Rig(Rig.Tonight, "nailgun");
            Assert.That(r.loadout.ZoomMag, Is.EqualTo(1), "not zoomed: x1");
            for (int i = 0; i < 5; i++) r.Step(true, false, default, i == 0);
            Assert.That(r.loadout.adsActive, Is.True, "LT zooms the STITCHER");
            Assert.That(r.loadout.ZoomMag, Is.EqualTo(1.25));
            Assert.That(r.mover.walkMult, Is.EqualTo(cfg.Num("ads.walkMult")));
            // zoomed nails: the spread x design.zoom.nailgunSpreadMult (every nail inside the narrower cone)
            double maxZoomed = 0;
            for (int i = 0; i < 60; i++)
            {
                r.Step(true, true);
                foreach (Projectiles.Projectile p in r.projectiles.nails)
                    if (p != null && p.alive && p.ageSteps <= 1) maxZoomed = System.Math.Max(maxZoomed, System.Math.Acos(System.Math.Min(1, -p.dir.z)) * 180 / System.Math.PI);
            }
            double spread = cfg.Num("weapons.nailgun.spreadDeg");
            Assert.That(maxZoomed, Is.GreaterThan(0));
            Assert.That(maxZoomed, Is.LessThanOrEqualTo(spread * cfg.Num("design.zoom.nailgunSpreadMult") + 1e-6), $"zoomed nails within {spread * 0.5} deg");
            Assert.That(r.loadout.detonated, Is.EqualTo(0), "LT never detonates on a zoom gun");
        }

        [Test]
        public void TheLongwave_ScopesOnLT_WalkX06()
        {
            var r = new Rig(Rig.Tonight, "rail");
            for (int i = 0; i < 5; i++) r.Step(true, false, default, i == 0);
            Assert.That(r.loadout.adsActive, Is.True, "LT scopes the LONGWAVE");
            Assert.That(r.loadout.ZoomMag, Is.EqualTo(3.5));
            Assert.That(r.mover.walkMult, Is.EqualTo(cfg.Num("design.zoom.railWalkMult")));
            for (int i = 0; i < 3; i++) r.Step(false, false);
            Assert.That(r.loadout.adsActive, Is.False);
            Assert.That(r.mover.walkMult, Is.EqualTo(1));
        }

        [Test]
        public void TheKestrel_Zooms1_5_AndItsHipSpreadIsTheWebs0_8Again()
        {
            var r = new Rig(Rig.Tonight, "revolver");
            double hipMax = 0;
            for (int i = 0; i < 40; i++)
            {
                r.Step(false, true);
                foreach (ShotEvent s in r.shots) hipMax = System.Math.Max(hipMax, System.Math.Acos(System.Math.Min(1, -s.dir.z)) * 180 / System.Math.PI);
                r.shots.Clear();
            }
            Assert.That(hipMax, Is.GreaterThan(0), "hip fire spreads again (the gust's 0 is gone)");
            Assert.That(hipMax, Is.LessThanOrEqualTo(cfg.Num("ads.hipSpreadDeg.revolver") + 1e-6));
            for (int i = 0; i < 60; i++) r.Step(false, false); // the cooldown
            for (int i = 0; i < 5; i++) r.Step(true, false, default, i == 0);
            Assert.That(r.loadout.adsActive, Is.True);
            Assert.That(r.loadout.ZoomMag, Is.EqualTo(1.5));
            r.shots.Clear();
            for (int i = 0; i < 60; i++) r.Step(true, true);
            Assert.That(r.shots.Count, Is.GreaterThan(0));
            foreach (ShotEvent s in r.shots) Assert.That(s.dir.z, Is.EqualTo(-1), "zoomed: no spread");
        }

        [Test]
        public void TheThunderclapAndTheComet_NeverZoom()
        {
            foreach (string gun in new[] { "shotgun", "rocket" })
            {
                var r = new Rig(Rig.Tonight, gun);
                for (int i = 0; i < 10; i++) r.Step(true, false, default, i == 0);
                Assert.That(r.loadout.adsActive, Is.False, gun);
                Assert.That(r.loadout.ZoomMag, Is.EqualTo(1), gun);
            }
        }

        [Test]
        public void OnlyTheScope_DescopesOnAHit_UntilLTIsPressedAgain()
        {
            var r = new Rig(Rig.Tonight, "rail", "revolver");
            for (int i = 0; i < 5; i++) r.Step(true, false, default, i == 0);
            Assert.That(r.loadout.adsActive, Is.True);
            r.health.Damage(10, "test");
            r.Step(true, false);
            Assert.That(r.loadout.adsActive, Is.False, "the scope descopes on a hit");
            for (int i = 0; i < 10; i++) r.Step(true, false);
            Assert.That(r.loadout.adsActive, Is.False, "and stays out while LT is still held");
            r.Step(false, false);
            r.Step(true, false, default, true);
            Assert.That(r.loadout.adsActive, Is.True, "a new press scopes again");
            var k = new Rig(Rig.Tonight, "revolver");
            for (int i = 0; i < 5; i++) k.Step(true, false, default, i == 0);
            k.health.Damage(10, "test");
            k.Step(true, false);
            Assert.That(k.loadout.adsActive, Is.True, "a 1.5 zoom keeps its zoom on a hit (Halo's split)");
        }

        [Test]
        public void ADash_SuspendsTheZoom_ItResumesWhileLTIsHeld()
        {
            var r = new Rig(Rig.Tonight, "nailgun");
            for (int i = 0; i < 5; i++) r.Step(true, false, default, i == 0);
            Assert.That(r.loadout.adsActive, Is.True);
            r.Step(true, false, new InputFrame { moveZ = 1, dash = new Button { held = true, pressed = true } });
            Assert.That(r.mover.state, Is.EqualTo(MoveState.Dash));
            Assert.That(r.loadout.adsActive, Is.False, "suspended in the dash");
            for (int i = 0; i < 120 && r.mover.state == MoveState.Dash; i++) r.Step(true, false);
            r.Step(true, false);
            Assert.That(r.loadout.adsActive, Is.True, "resumed: LT still held");
        }

        [Test]
        public void ZoomAndShockwaveOff_NoGunZooms_AsNight19()
        {
            foreach (string gun in new[] { "revolver", "nailgun", "rail" })
            {
                var r = new Rig(N20Blocks.Off, gun);
                for (int i = 0; i < 10; i++) r.Step(true, false, default, i == 0);
                Assert.That(r.loadout.adsActive, Is.False, gun + ": NIGHT19 (the gust on the KESTREL, no other zoom)");
            }
            Assert.That(new Rig(N20Blocks.Off, "revolver").loadout.ZoomGun("revolver"), Is.EqualTo(0), "NIGHT19: the KESTREL's LT is the gust");
        }
    }
}
