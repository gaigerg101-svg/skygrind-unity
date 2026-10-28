// NIGHT20 U3-34.3 / U3-34.8 (builder G): the bots' LT rule (Core/Guns/AltUse.cs, read by the AUTOPILOT through
// AutopilotGuns and by THE RANGE through SandboxRange.AltUse) with tonight's tools: the THUNDERCLAP's shockwave is the panic
// button (a crowd or a wind-up within altUse.near m, only with a charge ready), the zoom guns zoom on a far target with
// nothing close (altUse.zoomMin m), and the zoom guns' old LT jobs move to SPECIAL (the STITCHER's detonate, the ZAPPER's
// discharge). Pure rule tests on AltSituation, plus the loadout's situation (the shockwave's charge, the zoom).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class AltUseTests
    {
        static GameConfig cfg;
        static AltUseParams p;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            p = AltUseParams.From(cfg);
        }

        static AltSituation S(string weapon) => new AltSituation { weapon = weapon, targetDistance = double.PositiveInfinity };

        [Test]
        public void TheNumbers_AreInTheDesignConfig()
        {
            Assert.That(p, Is.Not.Null, "design.arsenal.altUse");
            Assert.That(p.near, Is.GreaterThan(0));
            Assert.That(p.crowd, Is.GreaterThanOrEqualTo(1));
            Assert.That(p.zoomMin, Is.GreaterThan(p.near));
        }

        [Test]
        public void TheShockwave_IsThePanicButton_OnlyWithAChargeReady()
        {
            var s = S("shotgun");
            s.shockwave = true;
            s.shockwaveReady = true;
            s.bodiesNear = p.crowd;
            Assert.That(AltUse.Hold(in s, p), Is.True, "a crowd within near m: press");
            s.bodiesNear = 0;
            s.windupNear = true;
            Assert.That(AltUse.Hold(in s, p), Is.True, "a wind-up within near m: press");
            s.windupNear = false;
            Assert.That(AltUse.Hold(in s, p), Is.False, "nothing close: keep the charge");
            s.bodiesNear = p.crowd;
            s.shockwaveReady = false;
            Assert.That(AltUse.Hold(in s, p), Is.False, "no charge: no press");
        }

        [Test]
        public void TheZoomGuns_ZoomOnAFarTarget_WithNothingClose()
        {
            foreach (string gun in new[] { "revolver", "nailgun", "zapper" })
            {
                var s = S(gun);
                s.zoom = 1.3;
                s.targetDistance = p.zoomMin + 1;
                Assert.That(AltUse.Hold(in s, p), Is.True, gun + ": far, nothing close: zoom");
                s.bodiesNear = 1;
                Assert.That(AltUse.Hold(in s, p), Is.False, gun + ": something close: hip fire");
                s.bodiesNear = 0;
                s.targetDistance = p.zoomMin - 1;
                Assert.That(AltUse.Hold(in s, p), Is.False, gun + ": close target: hip fire");
            }
            var rail = S("rail");
            rail.zoom = 3.5;
            Assert.That(AltUse.Hold(in rail, p), Is.True, "the LONGWAVE scopes whenever nothing is close");
            rail.bodiesNear = 1;
            Assert.That(AltUse.Hold(in rail, p), Is.False);
        }

        [Test]
        public void TheZoomGuns_OldLTJobs_MoveToSpecial()
        {
            var n = S("nailgun");
            n.zoom = 1.25;
            n.targetNails = p.detonateNails;
            Assert.That(AltUse.Special(in n, p), Is.True, "the STITCHER's detonate on SPECIAL");
            Assert.That(AltUse.Hold(in n, p), Is.False, "never on LT: LT zooms (no far target here)");
            var z = S("zapper");
            z.zoom = 1.3;
            z.shockedInSight = p.shocked;
            Assert.That(AltUse.Special(in z, p), Is.True, "the ZAPPER's discharge on SPECIAL");
            var unzoomed = S("nailgun");
            unzoomed.targetNails = p.detonateNails;
            Assert.That(AltUse.Hold(in unzoomed, p), Is.True, "NIGHT19: the detonate on LT when the STITCHER doesn't zoom");
            Assert.That(AltUse.Special(in unzoomed, p), Is.False);
        }

        [Test]
        public void TheTorch_LaysItsFireWall_WhenTheTargetStandsWhereTheWallWouldBurn()
        {
            // NIGHT20 B5: the wall burns from weapons.specials.torch.ahead m to ahead + length m: a target there gets the wall.
            double ahead = cfg.Num("weapons.specials.torch.ahead"), far = ahead + cfg.Num("weapons.specials.torch.length");
            var s = S("torch");
            s.wallFrom = ahead;
            s.wallTo = far;
            s.targetDistance = (ahead + far) / 2;
            Assert.That(AltUse.Hold(in s, p), Is.True, "in the wall's strip");
            s.targetDistance = ahead - 1;
            Assert.That(AltUse.Hold(in s, p), Is.False, "too close: the flame");
            s.targetDistance = far + 1;
            Assert.That(AltUse.Hold(in s, p), Is.False, "past the wall");
        }

        [Test]
        public void TheLoadoutsSituation_CarriesTheShockwavesChargeAndTheZoom()
        {
            var events = new EventBus();
            var enemies = new EnemyRegistry();
            var world = new TriangleWorld();
            world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
            var mover = new Mover(cfg.movement, world);
            mover.Reset(0, 0, 0);
            var lo = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (new Vec3(0, 1.6, 0), new Vec3(0, 0, -1)));
            lo.n19 = new N19Blocks { gust = true };
            lo.n20 = new N20Blocks { shockwave = true, zoom = true };
            lo.Reset(new List<string> { "shotgun", "nailgun" }, 1);
            AltSituation s = lo.AltSituation(null, p.near);
            Assert.That(s.shockwave, Is.True);
            Assert.That(s.shockwaveReady, Is.True);
            Assert.That(s.gust, Is.False, "the shockwave turns the gust off");
            Assert.That(s.zoom, Is.EqualTo(0), "the THUNDERCLAP doesn't zoom");
            lo.SetSlot(1);
            s = lo.AltSituation(null, p.near);
            Assert.That(s.zoom, Is.EqualTo(1.25));
        }
    }
}
