// NIGHT20 U3-34.9 row B5 (builder G2; Research/2026-10-06-additions-backlog.md row 25): the VORTEX, ported from the web's
// guns.js (orb() :203-215, the alt in step() :495-506, the trigger :555-559, meter() :589) and projectiles.js
// (openVortex / burstVortex :181-197, vortexAlt :345-357, stepVortexes :728-767) with the web's numbers (weapons.vortex,
// weapons.specials.vortex), through the real loadout and Projectiles on a flat floor, behind design.adds.b5:
// - the trigger: an orb every weapons.vortex.fireInterval s while held (the `fire` event), flying at weapons.vortex.speed
//   along the aim; it opens (`vortexOpen`) on the first body or wall it meets, or at the end of its lifetime;
// - open, it pulls every body within pullRadius toward its middle (gunMath pullSpeed: pullSpeed, slower inside pullCore)
//   and marks it PULLED (the arsenal's `pull` status); after pullTime s it bursts (`vortexBurst`): burstDamage with the
//   splash falloff (edgeDamage) out to burstRadius;
// - LT / SPECIAL tapped: every orb of yours collapses now (opens if it was flying, then bursts; `vortexCollapse`); held past
//   weapons.specials.vortex.repelHold s, the open orbs repel at repelSpeed instead of pulling (`vortexRepel`), and letting
//   go turns them back to a pull (no burst);
// - off (no design.adds.b5): the VORTEX fires nothing; on, it is a port (Loadout.IsPort20) whose LT is its own alt (no zoom);
// - the bot's alt rule (AltUse): tap LT once your open orb holds altUse.crowd bodies inside its burst; never held on (a held
//   LT would repel).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class G20VortexTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static double N(string path) => cfg.Num(path);

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Husks husks;
            public readonly Mover mover;
            public readonly Loadout loadout;
            public readonly Projectiles projectiles;
            public Vec3 dir = new Vec3(0, 0, -1);
            public readonly List<HitEvent> hits = new List<HitEvent>();
            public readonly List<string> log = new List<string>();
            public readonly List<Vec3> opens = new List<Vec3>();
            public readonly List<double> burstRadii = new List<double>();
            public readonly List<Vec3> bursts = new List<Vec3>();

            public Rig(bool b5 = true)
            {
                world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
                husks = new Husks(cfg, events, enemies, world, -30);
                mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (Eye, dir));
                projectiles = new Projectiles(cfg, events, world, enemies, 3);
                loadout.projectiles = projectiles;
                loadout.n19 = new N19Blocks { gust = true };
                loadout.n20 = new N20Blocks { shockwave = true, zoom = true, arsenal = true, adds = b5 };
                loadout.freshDamage = false;
                loadout.Reset(new List<string> { "vortex", "revolver" }, 1);
                events.On<HitEvent>("hit", h => hits.Add(new HitEvent { weapon = h.weapon, damage = h.damage, enemy = h.enemy }));
                events.On("fire", w => log.Add("fire:" + w));
                foreach (string e in new[] { G20Events.VortexCollapse, G20Events.VortexRepel })
                {
                    string name = e;
                    events.On(name, _ => log.Add(name));
                }
                events.On<VortexEvent>(G20Events.VortexOpen, v => opens.Add(v.position));
                events.On<BlastEvent>(G20Events.VortexBurst, b =>
                {
                    bursts.Add(b.position);
                    burstRadii.Add(b.radius);
                });
                for (int i = 0; i < 30; i++) Step(false, false);
            }

            public Vec3 Eye => new Vec3(mover.position.x, mover.position.y + cfg.movement.body.eyeHeight, mover.position.z);

            /// <summary>One step: the player, the loadout, the projectiles (the husks stand where they spawned unless `world`).</summary>
            public void Step(bool fire, bool lt, bool stepHusks = false)
            {
                mover.Step(default, cfg.Dt);
                loadout.Step(new CombatInput { weapon = -1, fire = new Button { held = fire, pressed = fire }, altFire = new Button { held = lt, pressed = lt && !ltWas } }, cfg.Dt);
                ltWas = lt;
                projectiles.Step(cfg.Dt);
                if (stepHusks) husks.Step(new Vec3(0, -50, 0), new Health(1e9, events, "dummy"), cfg.Dt);
            }

            bool ltWas;

            public Husk Husk(double x, double z, double hp = 1e6)
            {
                var h = (Husk)husks.Spawn("husk", new Vec3(x, 0, z), new Vec3(1000, 0, 1000));
                h.hp = h.maxHp = hp;
                return h;
            }

            public int Count(string name) => log.FindAll(s => s == name).Count;

            public double DamageOn(Enemy e)
            {
                double d = 0;
                foreach (HitEvent h in hits) if (h.enemy == e.id && h.weapon == "vortex") d += h.damage;
                return d;
            }

            /// <summary>The first live orb (null: none).</summary>
            public Projectiles.Projectile Orb()
            {
                foreach (Projectiles.Projectile p in projectiles.vortexes) if (p.alive) return p;
                return null;
            }

            /// <summary>Fire one orb at the empty floor ahead and step until it opens (at the end of its flight).</summary>
            public Projectiles.Projectile OpenOrb()
            {
                Step(true, false);
                for (int i = 0; i < 400 && opens.Count == 0; i++) Step(false, false);
                Assert.That(opens.Count, Is.EqualTo(1), "the orb opened");
                return Orb();
            }
        }

        [Test]
        public void TheTrigger_FiresAnOrb_EveryFireInterval_AtTheWebsSpeed()
        {
            var r = new Rig();
            r.Step(true, false);
            Assert.That(r.Count("fire:vortex"), Is.EqualTo(1), "an orb on the pull");
            Assert.That(r.projectiles.Count("vortex", r.loadout), Is.EqualTo(1), "one orb in flight");
            Vec3 a = r.Orb().position;
            r.Step(true, false);
            Vec3 b = r.Orb().position;
            Assert.That(Vec3.DistanceTo(a, b), Is.EqualTo(N("weapons.vortex.speed") * cfg.Dt).Within(1e-6), "weapons.vortex.speed along the aim");
            Assert.That(b.z, Is.LessThan(a.z), "along the aim");
            int interval = MovementMath.StepsFor(N("weapons.vortex.fireInterval"), cfg.Dt);
            for (int i = 2; i < interval; i++) r.Step(true, false);
            Assert.That(r.Count("fire:vortex"), Is.EqualTo(1), "nothing inside the interval");
            r.Step(true, false);
            Assert.That(r.Count("fire:vortex"), Is.EqualTo(2), "the next orb at weapons.vortex.fireInterval");
        }

        [Test]
        public void InTheOpen_TheOrbOpens_AtTheEndOfItsFlight()
        {
            var r = new Rig();
            Vec3 eye = r.Eye;
            r.OpenOrb();
            double flown = N("weapons.vortex.spawnForward") + N("weapons.vortex.speed") * N("weapons.vortex.lifetime");
            Assert.That(r.opens[0].z, Is.EqualTo(eye.z - flown).Within(0.25), "spawnForward + speed x lifetime ahead");
        }

        [Test]
        public void OnABody_TheOrbOpensWhereItMeetsIt()
        {
            var r = new Rig();
            Husk h = r.Husk(0, -6);
            r.dir = Vec3.Normalize(new Vec3(0, -0.6, -6)); // at the body
            r.Step(true, false);
            for (int i = 0; i < 120 && r.opens.Count == 0; i++) r.Step(false, false);
            Assert.That(r.opens.Count, Is.EqualTo(1));
            Assert.That(r.opens[0].z, Is.GreaterThan(-6.5).And.LessThan(-4.5), "at the body, not past it");
            Assert.That(r.Orb(), Is.Not.Null, "open, it stays (pulling) until its burst");
        }

        [Test]
        public void Open_ItPullsEveryBodyInItsRadius_TowardItsMiddle_AndMarksItPulled()
        {
            var r = new Rig();
            Projectiles.Projectile orb = r.OpenOrb();
            Vec3 c = orb.position;
            Husk inside = r.Husk(c.x + 4, c.z);
            Husk outside = r.Husk(c.x - 9, c.z);
            r.Step(false, false);
            double expect = GunMath.PullSpeed(4, N("weapons.vortex.pullRadius"), N("weapons.vortex.pullSpeed"), N("weapons.vortex.pullCore"));
            Assert.That(inside.knockSteps, Is.GreaterThan(0), "pulled (a knockback toward the middle)");
            Assert.That(inside.velocity.x, Is.EqualTo(-expect).Within(1e-9), "pullSpeed toward the middle (-x)");
            Assert.That(inside.velocity.z, Is.EqualTo(0).Within(1e-9));
            Assert.That(r.loadout.arsenal.HasStatus(inside, "pull"), Is.True, "PULLED");
            Assert.That(outside.knockSteps, Is.EqualTo(0), "outside weapons.vortex.pullRadius: untouched");
            Assert.That(r.loadout.arsenal.HasStatus(outside, "pull"), Is.False);
            // stepped, the pull moves it in
            double d0 = 4;
            for (int i = 0; i < 30; i++) r.Step(false, false, true);
            double d1 = JsMath.Hypot(inside.position.x - c.x, inside.position.z - c.z);
            Assert.That(d1, Is.LessThan(d0 - 1), $"0.25 s of pull moves it in (4 m -> {d1:0.00} m)");
        }

        [Test]
        public void AfterPullTime_ItBursts_WithTheWebsDamageAndFalloff()
        {
            var r = new Rig();
            Projectiles.Projectile orb = r.OpenOrb();
            Vec3 c = orb.position;
            Husk near = r.Husk(c.x + 1, c.z);
            Husk edge = r.Husk(c.x + 4, c.z);
            Husk far = r.Husk(c.x + 8, c.z); // outside the 7 m pull too
            int pull = MovementMath.StepsFor(N("weapons.vortex.pullTime"), cfg.Dt);
            for (int i = 0; i < pull - 2; i++) r.Step(false, false);
            Assert.That(r.bursts.Count, Is.EqualTo(0), "still pulling");
            for (int i = 0; i < 3; i++) r.Step(false, false);
            Assert.That(r.bursts.Count, Is.EqualTo(1), "the burst at weapons.vortex.pullTime");
            Assert.That(r.Orb(), Is.Null, "gone after its burst");
            double radius = r.burstRadii[0];
            Assert.That(radius, Is.EqualTo(N("weapons.vortex.burstRadius")).Within(1e-9), "burstRadius (standing still: no momentum)");
            foreach (Husk h in new[] { near, edge })
            {
                double d = Vec3.DistanceTo(h.AimPoint, r.bursts[0]);
                double expect = JsMath.Round(WeaponMath.SplashDamage(JsMath.Min(d, radius), radius, N("weapons.vortex.burstDamage"), N("weapons.vortex.edgeDamage")));
                Assert.That(r.DamageOn(h), Is.EqualTo(expect).Within(1e-9), $"the splash at {d:0.00} m");
            }
            Assert.That(r.DamageOn(near), Is.GreaterThan(r.DamageOn(edge)), "falloff");
            Assert.That(r.DamageOn(far), Is.EqualTo(0), "outside burstRadius");
        }

        [Test]
        public void LT_Tapped_CollapsesTheOrbNow_EvenInFlight()
        {
            var r = new Rig();
            r.Step(true, false);
            for (int i = 0; i < 20; i++) r.Step(false, false); // in flight (1.2 s lifetime)
            Assert.That(r.opens.Count, Is.EqualTo(0));
            r.Step(false, true); // press
            Assert.That(r.bursts.Count, Is.EqualTo(0), "the collapse waits for the release (a hold would repel)");
            r.Step(false, false); // release before repelHold
            Assert.That(r.opens.Count, Is.EqualTo(1), "it opened where it was");
            Assert.That(r.bursts.Count, Is.EqualTo(1), "and burst at once");
            Assert.That(r.Count(G20Events.VortexCollapse), Is.EqualTo(1));
            Assert.That(r.loadout.vortexCollapses, Is.EqualTo(1));
            Assert.That(r.projectiles.Count("vortex", r.loadout), Is.EqualTo(0));
        }

        [Test]
        public void LT_HeldPastRepelHold_Repels_AndLettingGo_PullsAgain_WithNoBurst()
        {
            var r = new Rig();
            Projectiles.Projectile orb = r.OpenOrb();
            Vec3 c = orb.position;
            int hold = MovementMath.StepsFor(N("weapons.specials.vortex.repelHold"), cfg.Dt);
            for (int i = 0; i < hold + 2; i++) r.Step(false, true);
            Assert.That(r.Count(G20Events.VortexRepel), Is.EqualTo(1), "repel once the hold passes repelHold");
            Assert.That(r.loadout.vortexRepels, Is.EqualTo(1));
            Assert.That(orb.repel, Is.True);
            Husk h = r.Husk(c.x + 4, c.z);
            r.Step(false, true);
            Assert.That(h.velocity.x, Is.EqualTo(N("weapons.specials.vortex.repelSpeed")).Within(1e-9), "pushed out (+x) at repelSpeed");
            r.Step(false, false); // let go
            Assert.That(orb.repel, Is.False, "back to a pull");
            Assert.That(r.bursts.Count, Is.EqualTo(0), "letting go of a repel is no collapse");
            r.Step(false, false);
            Assert.That(h.velocity.x, Is.LessThan(0), "pulled again");
        }

        [Test]
        public void WithoutB5_TheVortexFiresNothing()
        {
            var r = new Rig(false);
            for (int i = 0; i < 60; i++) r.Step(true, false);
            Assert.That(r.Count("fire:vortex"), Is.EqualTo(0));
            Assert.That(r.projectiles.Count("vortex"), Is.EqualTo(0));
        }

        [Test]
        public void TheVortex_IsAPort_ItsLTIsItsOwnAlt_NotAZoom_AndTheMeterIsItsReload()
        {
            var r = new Rig();
            Assert.That(Loadout.IsPort20("vortex"), Is.True);
            Assert.That(r.loadout.ZoomGun("vortex"), Is.EqualTo(0));
            Assert.That(G20Events.VortexOpen, Is.EqualTo("vortexOpen"), "the web's name");
            Assert.That(G20Events.VortexBurst, Is.EqualTo("vortexBurst"), "the web's name");
            r.Step(true, false);
            Assert.That(r.loadout.Meter(out double value, out _, out _), Is.True);
            Assert.That(value, Is.LessThan(0.05), "just fired: reloading");
        }

        [Test]
        public void TheBotsAltRule_TapsLT_OnceTheOrbHoldsACrowd_AndNeverHoldsIt()
        {
            var p = AltUseParams.From(cfg);
            var s = new AltSituation { weapon = "vortex", targetDistance = 10, vortexGathered = p.crowd };
            Assert.That(AltUse.Hold(s, p), Is.True, "a crowd inside the burst: collapse it");
            s.vortexPressing = true;
            Assert.That(AltUse.Hold(s, p), Is.False, "pressed already: let go (the release collapses; held on, it would repel)");
            s.vortexPressing = false;
            s.vortexGathered = p.crowd - 1;
            Assert.That(AltUse.Hold(s, p), Is.False, "too few yet: let it pull");
            // the loadout fills the count from its open orbs
            var r = new Rig();
            Projectiles.Projectile orb = r.OpenOrb();
            r.Husk(orb.position.x + 1, orb.position.z);
            r.Husk(orb.position.x - 1, orb.position.z);
            AltSituation live = r.loadout.AltSituation(null, p.near);
            Assert.That(live.vortexGathered, Is.EqualTo(2), "two bodies inside the burst radius");
        }
    }
}
