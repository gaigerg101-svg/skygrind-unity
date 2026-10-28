// NIGHT20 (the shared commit places this empty fixture; E owns it): the LANCER (lancer.js, ported as it is): the lagging aim beam from a perch, its damage, the blink between perches, from round 8, at most 6; it prioritises campers.
// Builder E (U3-34.9 C2, tests first): src/enemies/lancer.js in Core/Enemies/Lancer.cs, behind design.adds.lancer. On a
// floor with three 10 m perch boxes (the director's high nodes are its perches): it spawns as config.lancer's body and
// never walks; with line of sight it aims for aimTime, its laser following your chest with trackLag (half while you're
// hunted), then fires a hitscan beam for damage that a still player takes and a fast strafe outruns; cover blocks the
// beam; it blinks (blinkTime) to a perch at least relocateMin m away after every shot, waits cooldown, and a perch that
// can't see you for blindBlink s sends it elsewhere; the director deals lancers from director.rounds.introduce.lancer
// with the row on, never with it off; the pool holds lancer.maxCount.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Core.Tests
{
    public class LancerTests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")), "config.json"));
        static double L(string k) => Cfg.Num("lancer." + k);
        static double Dt => Cfg.Dt;

        static readonly SpawnNode P1 = new SpawnNode { x = 0, y = 10, z = 0, high = true };
        static readonly SpawnNode P2 = new SpawnNode { x = 30, y = 10, z = 0, high = true };
        static readonly SpawnNode P3 = new SpawnNode { x = -30, y = 10, z = 0, high = true };

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Health health;
            public readonly Husks pool;
            public readonly List<double> hurts = new List<double>();
            public readonly List<string> said = new List<string>();
            public readonly List<SpawnNode> perches = new List<SpawnNode> { P1, P2, P3 };
            public bool hunted;
            public Vec3 player = new Vec3(0, 0, 40);

            public Rig(params Vec3[] wallMins)
            {
                world.AddBox(new Vec3(-120, -1, -120), new Vec3(120, 0, 120));
                foreach (SpawnNode p in new[] { P1, P2, P3 }) world.AddBox(new Vec3(p.x - 2, 0, p.z - 2), new Vec3(p.x + 2, p.y, p.z + 2));
                foreach (Vec3 w in wallMins) world.AddBox(w, new Vec3(w.x + 8, w.y + 6, w.z + 1)); // an 8 x 6 m wall, 1 m thick
                health = new Health(100000, events, "player");
                pool = new Husks(Cfg, events, enemies, world, -30);
                pool.NewRun(11);
                pool.lancerPerches = () => perches;
                pool.lancerHunted = () => hunted;
                events.On<HurtEvent>("hurt", e =>
                {
                    if (e.source == "lancer") hurts.Add(e.amount);
                });
                foreach (string name in new[] { "lancerAim", "lancerFire", "lancerBlink" })
                {
                    string n = name;
                    events.On<LancerEvent>(n, e => said.Add(n + (n == "lancerFire" ? (e.hit ? ":hit" : ":miss") : n == "lancerBlink" && e.blind ? ":blind" : "")));
                }
            }

            public Lancer Spawn(SpawnNode at)
            {
                Husk h = pool.Spawn("lancer", new Vec3(at.x, at.y, at.z), player);
                Assert.That(h, Is.InstanceOf<Lancer>(), "the pool spawns a Lancer for 'lancer'");
                return (Lancer)h;
            }

            public void Step() => pool.Step(player, health, Dt);

            public void Run(double seconds, double vx = 0)
            {
                int n = MovementMath.StepsFor(seconds, Dt);
                for (int i = 0; i < n; i++)
                {
                    player = new Vec3(player.x + vx * Dt, player.y, player.z);
                    Step();
                }
            }
        }

        [Test]
        public void ItSpawnsAsTheConfigsLancer_AndNeverWalks()
        {
            var r = new Rig();
            Lancer l = r.Spawn(P1);
            Assert.That(l.kind, Is.EqualTo("lancer"));
            Assert.That(l.hp, Is.EqualTo(L("hp")));
            Assert.That(l.Radius, Is.EqualTo(L("radius")));
            Assert.That(l.Height, Is.EqualTo(L("height")));
            Vec3 at = l.position;
            r.Run(1.0);
            Assert.That(l.position.x, Is.EqualTo(at.x));
            Assert.That(l.position.y, Is.EqualTo(at.y), "no gravity drift on its perch");
            Assert.That(l.position.z, Is.EqualTo(at.z));
        }

        [Test]
        public void WithLineOfSight_ItAimsForAimTime_ThenTheBeamHitsAStillPlayer()
        {
            var r = new Rig();
            Lancer l = r.Spawn(P1);
            r.Step();
            Assert.That(r.said, Does.Contain("lancerAim"), "it starts aiming at once (cooldown 0, in range, line of sight)");
            Assert.That(l.state, Is.EqualTo(Lancer.State.Aim));
            Assert.That(l.committed, Is.True);
            r.Run(L("aimTime") - 0.05);
            Assert.That(r.hurts, Is.Empty, "nothing before aimTime");
            r.Run(0.1);
            Assert.That(r.hurts, Is.EqualTo(new[] { L("damage") }), "one beam for lancer.damage");
            Assert.That(r.said, Does.Contain("lancerFire:hit"));
            Assert.That(r.said, Does.Contain("lancerBlink"), "it relocates after every shot");
        }

        [Test]
        public void AFastStrafe_OutrunsTheLaggingLaser()
        {
            var r = new Rig();
            r.Spawn(P1);
            r.Run(L("aimTime") + 0.1, 12);
            Assert.That(r.hurts, Is.Empty, "at 12 m/s across the beam the lag (trackLag) leaves the laser metres behind");
            Assert.That(r.said, Does.Contain("lancerFire:miss"));
        }

        [Test]
        public void WhileYoureHunted_TheLaserFollowsTwiceAsFast()
        {
            foreach (bool hunted in new[] { false, true })
            {
                var r = new Rig { hunted = hunted };
                r.Spawn(P1);
                r.Run(L("aimTime") + 0.1, 1.8); // 1.8 m/s: the lag's offset ~0.75 m (a miss), ~0.40 m hunted (a hit)
                if (hunted) Assert.That(r.hurts, Is.EqualTo(new[] { L("damage") }), "hunted: trackLag x 0.5 keeps up");
                else Assert.That(r.hurts, Is.Empty, "not hunted: trackLag leaves it ~0.75 m off (hitRadius 0.55)");
            }
        }

        [Test]
        public void Cover_BlocksIt_AndABlindPerch_BlinksAfterBlindBlink()
        {
            var r = new Rig(new Vec3(-4, 0, 20)); // a wall across the line from P1 to the player (x -4..4, y 0..6, z 20..21)
            r.player = new Vec3(0, 0, 24);
            Lancer l = r.Spawn(P1);
            r.Run(L("blindBlink") - 0.1);
            Assert.That(r.said, Does.Not.Contain("lancerAim"), "no line of sight: no aim");
            Assert.That(l.state, Is.Not.EqualTo(Lancer.State.Blink));
            r.Run(0.2);
            Assert.That(r.said, Does.Contain("lancerBlink:blind"), "a perch that can't see you for blindBlink s: it blinks");
        }

        [Test]
        public void AfterAShot_ItBlinksToAPerchRelocateMinAway_ThenWaitsTheCooldown()
        {
            var r = new Rig();
            Lancer l = r.Spawn(P1);
            r.Run(L("aimTime") + 0.05);
            Assert.That(l.state, Is.EqualTo(Lancer.State.Blink));
            r.Run(L("blinkTime") + 0.05);
            Assert.That(l.state, Is.Not.EqualTo(Lancer.State.Blink));
            double moved = System.Math.Sqrt(l.position.x * l.position.x + l.position.z * l.position.z);
            Assert.That(moved, Is.GreaterThanOrEqualTo(L("relocateMin")), "a perch at least relocateMin m from the last");
            Assert.That(l.position.y, Is.EqualTo(10.05).Within(1e-9), "on the perch (+0.05)");
            int aims = r.said.FindAll(s => s == "lancerAim").Count;
            r.Run(L("cooldown") - 0.1);
            Assert.That(r.said.FindAll(s => s == "lancerAim").Count, Is.EqualTo(aims), "no new aim inside the cooldown");
            r.Run(0.2);
            Assert.That(r.said.FindAll(s => s == "lancerAim").Count, Is.EqualTo(aims + 1));
        }

        [Test]
        public void ThePool_HoldsLancerMaxCount()
        {
            var r = new Rig();
            int max = (int)L("maxCount");
            int made = 0;
            for (int i = 0; i < max + 2; i++)
                if (r.pool.Spawn("lancer", new Vec3(P1.x, P1.y, P1.z), r.player) != null) made++;
            Assert.That(made, Is.EqualTo(max));
        }

        [Test]
        public void TheDirector_DealsLancers_FromTheWebsRound_WithTheRowOn()
        {
            var n19 = new N19Blocks { enemies = true };
            Assert.That(Director.Spawnable("lancer", n19, true), Is.True);
            Assert.That(Director.Spawnable("lancer", n19, false), Is.False, "the row off: NIGHT19's kinds");
            Assert.That(Director.Spawnable("drone", n19, false), Is.EqualTo(Director.Spawnable("drone", n19)));
            DirectorMath.D d = DirectorMath.From(Cfg);
            int from = (int)Cfg.Num("director.rounds.introduce.lancer");
            int first = from;
            while (DirectorMath.IsSwarmRound(first, d)) first++; // a swarm round deals swarmers only (round 8 is one)
            bool seen = false, seenOff = false;
            for (uint seed = 1; seed <= 60; seed++)
            {
                foreach (List<string> s in DirectorMath.PlanSquads(from - 1, new Mulberry32(seed).Next, d, k => Director.Spawnable(k, n19, true)))
                    Assert.That(s, Does.Not.Contain("lancer"), "round " + (from - 1));
                foreach (List<string> s in DirectorMath.PlanSquads(first, new Mulberry32(seed).Next, d, k => Director.Spawnable(k, n19, true)))
                    if (s.Contains("lancer")) seen = true;
                foreach (List<string> s in DirectorMath.PlanSquads(first, new Mulberry32(seed).Next, d, k => Director.Spawnable(k, n19, false)))
                    if (s.Contains("lancer")) seenOff = true;
            }
            Assert.That(seen, Is.True, "a lancer squad within 60 seeds of round " + first);
            Assert.That(seenOff, Is.False, "the row off: no lancer");
            Assert.That(Cfg.Strs("director.highKinds"), Does.Contain("lancer"), "the web's highKinds: it spawns on a high node");
        }
    }
}
