// NIGHT8 U2-10: the SPITTER (spitter.js) and ai/aim.js on a floor: it tells, lobs a 3-glob volley that hits a still
// player for spitter.globDamage and leaves a puddle that ticks puddleDps; a punch reflects a glob in reach back into the
// spitter (homing, orb.reflectDamageMult x globDamage, weapon "orb"); hit chance falls with speed and a miss lands
// missBy to the side; rounds deal spitters from director.rounds.introduce.spitter.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class SpitterTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        sealed class Rig
        {
            public EventBus events = new EventBus();
            public EnemyRegistry enemies = new EnemyRegistry();
            public TriangleWorld world = new TriangleWorld();
            public Health health;
            public Husks pool;
            public readonly List<HurtEvent> hurts = new List<HurtEvent>();

            public Rig()
            {
                world.AddBox(new Vec3(-40, -1, -40), new Vec3(40, 0, 40));
                health = new Health(1000, events, "player");
                pool = new Husks(cfg, events, enemies, world, -30);
                events.On<HurtEvent>("hurt", e => hurts.Add(new HurtEvent { amount = e.amount, source = e.source }));
            }

            public void Step(Vec3 player) => pool.Step(player, health, cfg.Dt);
        }

        [Test]
        public void AVolley_HitsAStillPlayer_AndThePuddleTicks()
        {
            var r = new Rig();
            Vec3 player = new Vec3(0, 0, 0);
            Husk s = r.pool.Spawn("spitter", new Vec3(0, 0, -12), player);
            Assert.That(s, Is.InstanceOf<Spitter>());
            Assert.That(s.hp, Is.EqualTo(cfg.Num("spitter.hp")));
            int tells = 0;
            r.events.On("spitterTell", _ => tells++);
            for (int i = 0; i < 240 && !r.hurts.Exists(h => h.source == "spitter"); i++) r.Step(player);
            Assert.That(tells, Is.EqualTo(1), "one tell before the volley");
            Assert.That(r.pool.acid.volleys, Is.EqualTo(1));
            HurtEvent glob = r.hurts.Find(h => h.source == "spitter");
            Assert.That(glob, Is.Not.Null, "a glob hit the still player");
            Assert.That(glob.amount, Is.EqualTo(cfg.Num("spitter.globDamage")));
            // The puddle under you ticks puddleDps x puddleTick every puddleTick (overlapping ones don't stack).
            s.alive = false;
            r.hurts.Clear();
            for (int i = 0; i < MovementMath.StepsFor(1, cfg.Dt); i++) r.Step(player);
            List<HurtEvent> acid = r.hurts.FindAll(h => h.source == "acid");
            Assert.That(acid.Count, Is.GreaterThanOrEqualTo(3), "acid ticks while you stand in the puddle");
            Assert.That(acid[0].amount, Is.EqualTo(cfg.Num("spitter.puddleDps") * cfg.Num("spitter.puddleTick")).Within(1e-9));
        }

        [Test]
        public void APunch_ReflectsAGlob_BackIntoTheSpitter()
        {
            var r = new Rig();
            Vec3 player = new Vec3(0, 0, 0);
            var mover = new Mover(cfg.movement, r.world);
            mover.Reset(0, 0, 0);
            Husk s = r.pool.Spawn("spitter", new Vec3(0, 0, -12), player);
            Vec3 eye = new Vec3(0, cfg.movement.body.eyeHeight, 0);
            Vec3 dir = Vec3.Normalize(s.AimPoint - eye);
            var loadout = new Loadout(cfg, r.events, r.enemies, r.world, mover, r.health, () => (eye, dir)) { acid = r.pool.acid };
            loadout.arsenal.enabled = false; // NIGHT9: base damage, as the web sandbox measures it (main.js: arsenal off); ArsenalTests covers the arsenal
            var hits = new List<HitEvent>();
            r.events.On<HitEvent>("hit", e => hits.Add(e));
            r.pool.acid.onReflectHit = (e, damage, at) => loadout.Damage(e, damage, false, "orb", at);
            bool punched = false;
            for (int i = 0; i < 360 && hits.Count == 0; i++)
            {
                bool near = false;
                foreach (var g in r.pool.acid.globs) if (g.alive && !g.reflected && Vec3.DistanceTo(g.position, eye) < cfg.Num("orb.reflectRange") * 0.9) near = true;
                var input = new CombatInput { weapon = -1 };
                if (near && !punched)
                {
                    input.punch = new Button { pressed = true, held = true };
                    punched = true;
                }
                loadout.Step(input, cfg.Dt);
                r.Step(player);
            }
            Assert.That(punched, Is.True, "a glob came within reach");
            Assert.That(r.pool.acid.reflects, Is.EqualTo(1));
            Assert.That(hits.Count, Is.GreaterThanOrEqualTo(1), "the reflected glob hit");
            Assert.That(hits[0].weapon, Is.EqualTo("orb"));
            Assert.That(hits[0].kind, Is.EqualTo("spitter"));
            Assert.That(hits[0].damage, Is.EqualTo(cfg.Num("spitter.globDamage") * cfg.Num("orb.reflectDamageMult")));
            Assert.That(s.hp, Is.EqualTo(cfg.Num("spitter.hp") - hits[0].damage));
        }

        [Test]
        public void Aim_MissesMoreAtSpeed_AndAMissLandsToTheSide()
        {
            var a = AimMath.A.From(cfg);
            Assert.That(AimMath.HitChance(0, a), Is.EqualTo(cfg.Num("ai.aim.baseHit")));
            Assert.That(AimMath.HitChance(cfg.Num("ai.aim.fullSpeed"), a), Is.EqualTo(cfg.Num("ai.aim.minHit")).Within(1e-12));
            Assert.That(AimMath.HitChance(40, a), Is.EqualTo(cfg.Num("ai.aim.minHit")).Within(1e-12));
            Assert.That(AimMath.HitChance(12, a), Is.LessThan(AimMath.HitChance(6, a)));
            Vec3 miss = AimMath.MissPoint(new Vec3(0, 0, 0), new Vec3(0, 0, -10), a.missBy, 0.9);
            Assert.That(System.Math.Abs(miss.x), Is.EqualTo(a.missBy).Within(1e-12), "sideways by missBy");
            Assert.That(miss.z, Is.EqualTo(-10).Within(1e-12));
            double t = AimMath.LeadPoint(new Vec3(0, 0, 0), new Vec3(0, 0, -16), new Vec3(4, 2, 0), 16, a.maxLead, out Vec3 lead);
            Assert.That(t, Is.EqualTo(1).Within(1e-12));
            Assert.That(lead.x, Is.EqualTo(4).Within(1e-12));
            Assert.That(lead.y, Is.EqualTo(1).Within(1e-12), "vertical lead halved");
        }

        [Test]
        public void Rounds_DealSpittersFromTheirIntroductionRound()
        {
            DirectorMath.D d = DirectorMath.From(cfg);
            int from = cfg.Int("director.rounds.introduce.spitter");
            for (int round = 1; round < from; round++)
            {
                foreach (var squad in DirectorMath.PlanSquads(round, new Mulberry32(5).Next, d, Director.Available))
                    Assert.That(squad, Has.None.EqualTo("spitter"), $"round {round}");
            }
            int spitters = 0;
            for (uint seed = 1; seed <= 24; seed++)
            {
                int round = from;
                while (DirectorMath.IsSwarmRound(round, d) || DirectorMath.IsBossRound(round, d)) round++;
                foreach (var squad in DirectorMath.PlanSquads(round, new Mulberry32(seed).Next, d, Director.Available))
                    if (squad.Contains("spitter")) spitters++;
            }
            Assert.That(spitters, Is.GreaterThan(0), $"round {from}+ deals spitter squads");
        }
    }
}
