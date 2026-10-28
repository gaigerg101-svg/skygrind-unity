// NIGHT20 (the shared commit places this empty fixture; G owns it): the THUNDERCLAP's shockwave (design.shockwave): two presses 17.9 s apart fire one, 18.0 s apart two; the cooldown ticks while holstered; none in a dash (no self push); the FORGE's second charge; the cone's push and stagger on light kinds (C3: >= 3 of 8 SWARMERs back >= 3 m); the doubleBlast / coneWave / shockwave / shockwaveReady events; the KESTREL's gust off with the block on.
// Builder G (U3-34.3): through the real loadout, the real Mover and real SWARMERs on a flat floor, with NIGHT19's config
// for the gust (design.gust on, as shipped) and design.shockwave on: LT on the THUNDERCLAP is one press = the web's double
// blast (weapons.js fireDoubleBlast at full compress) + the shock cone (weapons.js coneWave on design.shockwave.lightKinds)
// + the self push design.shockwave.selfPush opposite the aim (Mover.Impulse); 18 s a charge, ticking while holstered.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class ShockwaveTests
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
            public Vec3 dir = new Vec3(0, 0, -1);
            public readonly List<ShockwaveEvent> shockwaves = new List<ShockwaveEvent>();
            public readonly List<string> log = new List<string>();
            public readonly List<int> ready = new List<int>();

            public Rig(bool shockwaveOn = true, params string[] guns)
            {
                world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
                husks = new Husks(cfg, events, enemies, world, -30);
                mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                health = new Health(100, events, "player");
                loadout = new Loadout(cfg, events, enemies, world, mover, health, () => (Eye, dir));
                loadout.n19 = new N19Blocks { gust = true }; // NIGHT19's config as shipped: the KESTREL's gust on
                loadout.n20 = new N20Blocks { shockwave = shockwaveOn };
                loadout.Reset(new List<string>(guns.Length > 0 ? guns : new[] { "shotgun", "revolver" }), 1);
                events.On<ShockwaveEvent>(N20Events.Shockwave, s => shockwaves.Add(new ShockwaveEvent { origin = s.origin, dir = s.dir, selfPush = s.selfPush, pushed = s.pushed, chargesLeft = s.chargesLeft, cooldownLeft = s.cooldownLeft }));
                foreach (string e in new[] { N20Events.DoubleBlast, N20Events.ConeWave, N20Events.Shockwave, N19Events.Gust, "fire" })
                {
                    string name = e;
                    events.On(name, _ => log.Add(name));
                }
                events.On<int>(N20Events.ShockwaveReady, n => ready.Add(n));
                for (int i = 0; i < 30; i++) Step(false); // on the floor
            }

            public Vec3 Eye => new Vec3(mover.position.x, mover.position.y + cfg.movement.body.eyeHeight, mover.position.z);

            /// <summary>One fixed step: the mover, then the loadout with LT pressed and held (or not), then the enemies.</summary>
            public void Step(bool lt, InputFrame frame = default, int weapon = -1)
            {
                mover.Step(frame, cfg.Dt);
                var input = new CombatInput { weapon = weapon, altFire = new Button { held = lt, pressed = lt } };
                loadout.Step(input, cfg.Dt);
                husks.Step(new Vec3(mover.position.x, mover.position.y, mover.position.z), new Health(1e9, events, "dummy"), cfg.Dt);
            }

            public void Wait(double seconds)
            {
                int n = MovementMath.StepsFor(seconds, cfg.Dt);
                for (int i = 0; i < n; i++) Step(false);
            }

            public int Count(string name)
            {
                int n = 0;
                foreach (string s in log) if (s == name) n++;
                return n;
            }

            public Swarmer Swarmer(double x, double z)
            {
                var s = (Swarmer)husks.Spawn("swarmer", new Vec3(x, 0, z), mover.position);
                Assert.That(s, Is.Not.Null, "a swarmer spawns");
                return s;
            }
        }

        static double G => 2 * cfg.Num("jump.height") / (cfg.Num("jump.apexTime") * cfg.Num("jump.apexTime"));

        [Test]
        public void TwoPresses_17_9SecondsApart_FireOne_18_0SecondsApart_FireTwo()
        {
            foreach (double gap in new[] { 17.9, 18.0 })
            {
                var r = new Rig();
                r.Step(true);
                Assert.That(r.shockwaves.Count, Is.EqualTo(1), "the first press fires");
                int steps = MovementMath.StepsFor(gap, cfg.Dt);
                for (int i = 1; i < steps; i++) r.Step(false);
                r.Step(true);
                Assert.That(r.shockwaves.Count, Is.EqualTo(gap < 18 ? 1 : 2), $"a second press {gap} s after the first");
                Assert.That(r.Count(N20Events.DoubleBlast), Is.EqualTo(r.shockwaves.Count), "LT while not ready fires nothing");
            }
        }

        [Test]
        public void TheCooldown_TicksWhileHolstered()
        {
            var r = new Rig();
            r.Step(true);
            Assert.That(r.loadout.shockwave.Charges, Is.EqualTo(0));
            r.Step(false, default, 1); // to the KESTREL
            r.Wait(1);
            Assert.That(r.loadout.Weapon, Is.EqualTo("revolver"));
            r.Wait(17);
            Assert.That(r.loadout.shockwave.Charges, Is.EqualTo(1), "the charge came back with the THUNDERCLAP holstered");
            Assert.That(r.loadout.shockwave.CooldownLeft, Is.EqualTo(0));
            Assert.That(r.loadout.shockwave.Ready, Is.True);
            r.Step(false, default, 0); // back to the THUNDERCLAP
            r.Wait(1);
            r.Step(true);
            Assert.That(r.shockwaves.Count, Is.EqualTo(2));
        }

        [Test]
        public void NoneInADash_NoChargeSpent()
        {
            var r = new Rig();
            r.Step(false, new InputFrame { moveZ = 1, dash = new Button { held = true, pressed = true } });
            Assert.That(r.mover.state, Is.EqualTo(MoveState.Dash), "dashing");
            r.Step(true, new InputFrame { moveZ = 1, dash = new Button { held = true } });
            Assert.That(r.mover.state, Is.EqualTo(MoveState.Dash));
            Assert.That(r.shockwaves.Count, Is.EqualTo(0), "no shockwave in a dash");
            Assert.That(r.loadout.shockwave.Charges, Is.EqualTo(1), "the charge is kept");
        }

        [Test]
        public void TheForgedThunderclap_HasASecondCharge_EachOnItsOwn18Seconds()
        {
            var r = new Rig();
            Assert.That(r.loadout.shockwave.MaxCharges, Is.EqualTo(cfg.Int("design.shockwave.charges")));
            r.loadout.forgedAny = id => id == "shotgun";
            r.Step(false);
            Assert.That(r.loadout.shockwave.MaxCharges, Is.EqualTo(cfg.Int("design.shockwave.forgeCharges")));
            r.Wait(18);
            Assert.That(r.loadout.shockwave.Charges, Is.EqualTo(2), "the second charge filled in 18 s");
            r.Step(true);
            r.Step(true);
            r.Step(true);
            Assert.That(r.shockwaves.Count, Is.EqualTo(2), "two charges, two shockwaves, the third press refused");
            Assert.That(r.loadout.shockwave.CooldownLeft, Is.GreaterThan(17.9));
        }

        [Test]
        public void TheCone_PushesAndStaggersLightKinds_AtLeast3Of8Swarmers3MetresBack()
        {
            var r = new Rig();
            var swarm = new List<Swarmer>();
            for (int i = 0; i < 8; i++) swarm.Add(r.Swarmer(-1.75 + i * 0.5, -4 - (i % 2)));
            Swarmer behind = r.Swarmer(0, 4);
            var start = new List<Vec3>();
            foreach (Swarmer s in swarm) start.Add(s.position);
            r.Step(true);
            Assert.That(r.shockwaves.Count, Is.EqualTo(1));
            Assert.That(r.shockwaves[0].pushed, Is.GreaterThanOrEqualTo(3), "pushed in the cone");
            r.Wait(0.5); // the push is over, the stagger holds them
            int back = 0;
            for (int i = 0; i < swarm.Count; i++)
            {
                if (!swarm[i].alive) continue; // the double blast's pellets may kill the ones dead ahead
                double before = JsMath.Hypot(start[i].x, start[i].z), after = JsMath.Hypot(swarm[i].position.x, swarm[i].position.z);
                if (after - before >= 3) back++;
            }
            Assert.That(back, Is.GreaterThanOrEqualTo(3), $"{back} of 8 swarmers 3 m back");
            Assert.That(behind.staggerSteps, Is.EqualTo(0), "the one behind is outside the cone");
        }

        [Test]
        public void ThePlayer_IsPushedOppositeTheAim_AndLookingDownLifts2To4Metres()
        {
            var r = new Rig();
            Vec3 before = r.mover.velocity;
            r.Step(true); // aim level toward -z
            Assert.That(r.mover.velocity.z - before.z, Is.GreaterThan(cfg.Num("design.shockwave.selfPush") * 0.8), "pushed along +z");
            var down = new Rig();
            down.dir = new Vec3(0, -1, 0);
            double y0 = down.mover.position.y, top = y0;
            down.Step(true);
            for (int i = 0; i < 240; i++)
            {
                down.Step(false);
                if (down.mover.position.y > top) top = down.mover.position.y;
            }
            Assert.That(top - y0, Is.InRange(2.0, 4.0), $"the shockwave jump {top - y0:0.00} m (SHOCKWAVE LIFT: KEEP the gust's +2.6 m)");
        }

        [Test]
        public void APress_EmitsDoubleBlast_ConeWave_Shockwave_AndTheChargeBack_ShockwaveReady()
        {
            var r = new Rig();
            r.Step(true);
            Assert.That(r.Count(N20Events.DoubleBlast), Is.EqualTo(1));
            Assert.That(r.Count(N20Events.ConeWave), Is.EqualTo(1));
            Assert.That(r.Count(N20Events.Shockwave), Is.EqualTo(1));
            Assert.That(r.Count(N19Events.Gust), Is.EqualTo(0), "the gust is the KESTREL's, off with the shockwave");
            ShockwaveEvent s = r.shockwaves[0];
            Assert.That(s.selfPush, Is.EqualTo(cfg.Num("design.shockwave.selfPush")));
            Assert.That(s.chargesLeft, Is.EqualTo(0));
            Assert.That(s.cooldownLeft, Is.EqualTo(cfg.Num("design.shockwave.cooldown")).Within(0.01));
            Assert.That(s.dir.z, Is.EqualTo(-1));
            Assert.That(r.ready.Count, Is.EqualTo(0));
            r.Wait(18);
            Assert.That(r.ready.Count, Is.EqualTo(1), "shockwaveReady once the charge is back");
            Assert.That(r.ready[0], Is.EqualTo(1));
        }

        [Test]
        public void TheHudContract_CooldownSeconds_ChargesAndReady()
        {
            var r = new Rig();
            Shockwave sw = r.loadout.shockwave;
            Assert.That(sw, Is.Not.Null);
            Assert.That(sw.Cooldown, Is.EqualTo(18));
            Assert.That(sw.Charges, Is.EqualTo(1));
            Assert.That(sw.MaxCharges, Is.EqualTo(1));
            Assert.That(sw.Ready, Is.True);
            Assert.That(sw.CooldownLeft, Is.EqualTo(0));
            r.Step(true);
            Assert.That(sw.Ready, Is.False);
            Assert.That(sw.CooldownLeft, Is.InRange(17.9, 18.0));
            r.Wait(9);
            Assert.That(sw.CooldownLeft, Is.EqualTo(9).Within(0.02));
        }

        [Test]
        public void AfterAShockwave_ThePrimaryWaitsOnePump_NotTheWebsSpecialCooldown()
        {
            // The web holds the primary for its double blast's own 1.4 s cooldown (a special you could fire every 1.4 s); the
            // shockwave's cooldown is 18 s, so the primary only waits its fire interval (one pump).
            var r = new Rig();
            var fires = new List<int>();
            int step = 0;
            r.events.On("fire", _ => fires.Add(step));
            r.Step(true); // the shockwave (its own "fire")
            int pump = MovementMath.StepsFor(cfg.Num("weapons.shotgun.fireInterval") * r.loadout.TuneInterval("shotgun"), cfg.Dt);
            for (step = 1; step <= pump + 2; step++)
            {
                r.mover.Step(default, cfg.Dt);
                r.loadout.Step(new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } }, cfg.Dt);
            }
            Assert.That(fires.Count, Is.EqualTo(2), "the shockwave, then one primary shot");
            Assert.That(fires[1] - fires[0], Is.EqualTo(pump), "the primary fires one pump after the shockwave");
        }

        [Test]
        public void NothingLowersTheCooldown_BelowMinCooldown()
        {
            var p = ShockwaveParams.From(cfg);
            Assert.That(p.EffectiveCooldown, Is.EqualTo(18));
            p.cooldown = 6;
            Assert.That(p.EffectiveCooldown, Is.EqualTo(cfg.Num("design.shockwave.minCooldown")));
        }

        [Test]
        public void WithTheShockwaveOn_TheKestrelsGustIsOff_AndItZooms()
        {
            var r = new Rig(true, "revolver", "shotgun");
            for (int i = 0; i < 10; i++) r.Step(true);
            Assert.That(r.Count(N19Events.Gust), Is.EqualTo(0), "no gust");
            Assert.That(r.loadout.adsActive, Is.True, "the KESTREL zooms again");
        }

        [Test]
        public void ShockwaveOff_TheThunderclapsLT_IsNight19s()
        {
            var r = new Rig(false);
            r.Step(true);
            Assert.That(r.shockwaves.Count, Is.EqualTo(0));
            Assert.That(r.Count(N20Events.DoubleBlast), Is.EqualTo(0));
            var k = new Rig(false, "revolver", "shotgun");
            k.Step(true);
            Assert.That(k.Count(N19Events.Gust), Is.EqualTo(1), "NIGHT19's gust on the KESTREL");
        }
    }
}
