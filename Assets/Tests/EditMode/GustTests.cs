// NIGHT19 U3-33.5 (builder W): the KESTREL's wind gust (design.gust, Core/Guns/Gust.cs) through the real loadout, the
// real Mover and real SWARMERs on a flat floor: the self push opposite the aim through Mover.Impulse (none during a
// dash), the gust-jump's height (S10: 2-4 m, under the COMET's rocket jump), one charge per airtime plus one on the
// ground, the cone's push and stagger on light enemies (S10's C3 line: at least 3 of 8 SWARMERs back 3 m or more), the
// `gust` event, and the zoom: gone with the block on, the NIGHT18 KESTREL with it off.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class GustTests
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
            public Vec3 dir = new Vec3(0, 0, -1);
            public readonly List<GustEvent> gusts = new List<GustEvent>();

            public Rig(bool gustOn)
            {
                world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
                husks = new Husks(cfg, events, enemies, world, -30);
                mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (Eye, dir));
                loadout.n19 = new N19Blocks { gust = gustOn };
                loadout.Reset(new List<string> { "revolver", "gauntlet" }, 1);
                events.On<GustEvent>(N19Events.Gust, g => gusts.Add(new GustEvent { origin = g.origin, dir = g.dir, selfPush = g.selfPush, pushed = g.pushed, airborne = g.airborne }));
                Settle();
            }

            public Vec3 Eye => new Vec3(mover.position.x, mover.position.y + cfg.movement.body.eyeHeight, mover.position.z);

            /// <summary>Land the mover on the floor (a few idle steps).</summary>
            public void Settle()
            {
                for (int i = 0; i < 30; i++) Step(false);
            }

            /// <summary>One fixed step: the mover (no move input, or `frame`), then the loadout with the alt held or not, then the enemies.</summary>
            public void Step(bool alt, InputFrame frame = default)
            {
                mover.Step(frame, cfg.Dt);
                var input = new CombatInput { weapon = -1, altFire = new Button { held = alt, pressed = alt } };
                loadout.Step(input, cfg.Dt);
                husks.Step(new Vec3(mover.position.x, mover.position.y, mover.position.z), new Health(1e9, events, "dummy"), cfg.Dt);
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
        public void Gust_PushesThePlayerOppositeTheAim_ThroughMoverImpulse()
        {
            var r = new Rig(true);
            Assert.That(r.mover.grounded, Is.True);
            Vec3 before = r.mover.velocity;
            r.Step(true); // aim level toward -z
            Assert.That(r.gusts.Count, Is.EqualTo(1), "one gust");
            // Mover.Impulse added selfPush along +z (opposite the aim); the ground's friction may have taken a step's share.
            Assert.That(r.mover.velocity.z - before.z, Is.GreaterThan(cfg.Num("design.gust.selfPush") * 0.8));
            Assert.That(r.gusts[0].selfPush, Is.EqualTo(cfg.Num("design.gust.selfPush")));
            Assert.That(r.gusts[0].airborne, Is.False);
        }

        [Test]
        public void GustJump_AimedDown_Gains2To4Metres_BelowTheRocketJump()
        {
            var r = new Rig(true);
            r.dir = new Vec3(0, -1, 0);
            double y0 = r.mover.position.y, top = y0;
            r.Step(true);
            for (int i = 0; i < 240; i++)
            {
                r.Step(false);
                if (r.mover.position.y > top) top = r.mover.position.y;
            }
            double gain = top - y0;
            double rocketJump = cfg.Num("weapons.rocket.knockback") * cfg.Num("weapons.rocket.knockback") / (2 * G);
            Assert.That(gain, Is.InRange(2.0, 4.0), $"gust-jump height {gain:0.00} m");
            Assert.That(gain, Is.LessThan(rocketJump), $"under the rocket jump's {rocketJump:0.00} m");
        }

        [Test]
        public void Gust_OneChargePerAirtime_PlusOneOnTheGround_RefilledOnLanding()
        {
            var r = new Rig(true);
            r.dir = new Vec3(0, -1, 0);
            r.Step(true); // the ground gust: up into the air
            Assert.That(r.gusts.Count, Is.EqualTo(1));
            Assert.That(r.mover.grounded, Is.False);
            int cooldown = MovementMath.StepsFor(cfg.Num("design.gust.cooldown"), cfg.Dt);
            r.dir = new Vec3(0, 0, -1);
            for (int i = 0; i < cooldown + 2; i++) r.Step(false);
            Assert.That(r.mover.grounded, Is.False, "still in the air");
            r.Step(true); // the air charge
            Assert.That(r.gusts.Count, Is.EqualTo(2));
            Assert.That(r.gusts[1].airborne, Is.True);
            for (int i = 0; i < cooldown + 2 && !r.mover.grounded; i++) r.Step(false);
            if (!r.mover.grounded)
            {
                r.Step(true); // the same airtime: refused
                Assert.That(r.gusts.Count, Is.EqualTo(2), "one gust per airtime");
            }
            for (int i = 0; i < 360 && !r.mover.grounded; i++) r.Step(false);
            Assert.That(r.mover.grounded, Is.True, "landed");
            for (int i = 0; i < cooldown + 2; i++) r.Step(false);
            r.Step(true); // landed: the ground charge again
            Assert.That(r.gusts.Count, Is.EqualTo(3));
        }

        [Test]
        public void Gust_DuringADash_DoesNothing()
        {
            var r = new Rig(true);
            r.Step(false, new InputFrame { moveZ = 1, dash = new Button { held = true, pressed = true } });
            Assert.That(r.mover.state, Is.EqualTo(MoveState.Dash), "dashing");
            Vec3 v = r.mover.velocity;
            r.Step(true, new InputFrame { moveZ = 1, dash = new Button { held = true } });
            Assert.That(r.mover.state, Is.EqualTo(MoveState.Dash));
            Assert.That(r.gusts.Count, Is.EqualTo(0), "no gust in a dash");
        }

        [Test]
        public void Gust_PushesAndStaggersLightEnemiesInTheCone_AtLeast3Of8Swarmers3MetresBack()
        {
            var r = new Rig(true);
            var swarm = new List<Swarmer>();
            for (int i = 0; i < 8; i++) swarm.Add(r.Swarmer(-1.75 + i * 0.5, -4 - (i % 2)));
            Swarmer behind = r.Swarmer(0, 4);
            var start = new List<Vec3>();
            foreach (Swarmer s in swarm) start.Add(s.position);
            Vec3 behindStart = behind.position;
            r.Step(true);
            Assert.That(r.gusts.Count, Is.EqualTo(1));
            Assert.That(r.gusts[0].pushed, Is.GreaterThanOrEqualTo(3), "pushed in the cone");
            int staggered = 0;
            foreach (Swarmer s in swarm) if (s.staggerSteps > 0) staggered++;
            Assert.That(staggered, Is.GreaterThanOrEqualTo(3), "staggered");
            for (int i = 0; i < 60; i++) r.Step(false); // 0.5 s: the push is over, the stagger holds them
            int back = 0;
            for (int i = 0; i < swarm.Count; i++)
            {
                // Pushed back: the horizontal move away from the gust's origin (the player's spawn, x 0 z 0).
                double before = JsMath.Hypot(start[i].x, start[i].z), after = JsMath.Hypot(swarm[i].position.x, swarm[i].position.z);
                if (after - before >= 3) back++;
            }
            Assert.That(back, Is.GreaterThanOrEqualTo(3), $"{back} of 8 swarmers 3 m back");
            Assert.That(behind.staggerSteps, Is.EqualTo(0), "the one behind is outside the cone");
        }

        [Test]
        public void Gust_EmitsTheGustEvent_WithTheAimAndTheOrigin()
        {
            var r = new Rig(true);
            Vec3 eye = r.Eye;
            r.Step(true);
            Assert.That(r.gusts.Count, Is.EqualTo(1));
            GustEvent g = r.gusts[0];
            Assert.That(g.dir.z, Is.EqualTo(-1));
            Assert.That(g.origin.y, Is.EqualTo(eye.y).Within(1e-9));
            Assert.That(N19Events.Gust, Is.EqualTo("gust"));
        }

        [Test]
        public void GustOn_TheKestrelNeverZooms_AndItsAltIsTheGust()
        {
            var r = new Rig(true);
            for (int i = 0; i < 10; i++) r.Step(true);
            Assert.That(r.loadout.adsActive, Is.False, "no zoom with the gust");
            Assert.That(r.gusts.Count, Is.EqualTo(1), "the alt is the gust (one: the cooldown holds the rest)");
        }

        [Test]
        public void GustOff_TheKestrelZoomsAsOnNight18_AndNoGust()
        {
            var r = new Rig(false);
            for (int i = 0; i < 10; i++) r.Step(true);
            Assert.That(r.loadout.adsActive, Is.True, "NIGHT18's zoom");
            Assert.That(r.gusts.Count, Is.EqualTo(0));
        }
    }
}
