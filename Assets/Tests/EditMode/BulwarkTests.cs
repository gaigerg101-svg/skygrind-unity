// NIGHT20 U3-34.9 A1 (builder EN; tests first): BULWARK, src/enemies/bulwark.js + bossKit.js + bossMath.js ported as they
// are (Core/Enemies/Bulwark.cs, BossKit.cs, BossMath.cs), on a floor with a walled pen of the web's arenaHalf around the
// kit's arena centre (the Grinder's 'pen' walls: their inner faces 20 m out) and a still or moving player:
//   1. a ram into a wall: stunned ram.stun (3) s, the weakest plate drops (`bossPart` reason wall), the vent is open,
//      `bossStun` {seconds}; then it walks again with the vent shut
//   2. the plates: x plateMult (0.1) until broken (the plate takes the whole hit), armour-piercing (a listed weapon or
//      momentum over apMomentum) x apMult (3); a broken plate's socket x1; the hull x0.35; the core x2; nothing in the intro
//   3. passivity: standing within passive.radius of one spot for passive.time (4) s makes it ram (`bossPunish`); moving
//      never does
//   4. the schedule: bossKit.schedule's rounds, then the rotation with its mutators; the scale
//   5. phase 2 and the kill: under p2At the beat (`bossBeat` PHASE 2, the push), the hatches' husks after hatchTell, then the
//      finisher: `bossDying`, 0.5 s later the kill (kind boss) and `bossDown` with its reward
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Core.Tests
{
    public class BulwarkTests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")), "config.json"));
        static double B(string path) => Cfg.Num("bulwark." + path);
        static int S(double sec) => MovementMath.StepsFor(sec, Cfg.Dt);

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Health health;
            public readonly BossKit kit;
            public readonly Bulwark bw;
            public Vec3 player;
            public Vec3 pushed;
            public readonly List<string> said = new List<string>();
            public readonly List<BossEvent> parts = new List<BossEvent>();
            public readonly List<(string kind, Vec3 at)> spawned = new List<(string, Vec3)>();
            public KillEvent kill;
            public double stunSeconds, downPoints;
            int ids;

            public Rig(Vec3 at)
            {
                player = at;
                world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
                double inner = B("arenaHalf") + 2; // the pen's walls: the Grinder's edge cells (their inner faces at +-20)
                world.AddBox(new Vec3(-inner - 4, 0, -inner - 4), new Vec3(inner + 4, 6, -inner));
                world.AddBox(new Vec3(-inner - 4, 0, inner), new Vec3(inner + 4, 6, inner + 4));
                world.AddBox(new Vec3(-inner - 4, 0, -inner), new Vec3(-inner, 6, inner));
                world.AddBox(new Vec3(inner, 0, -inner), new Vec3(inner + 4, 6, inner));
                health = new Health(1e9, events, "player");
                var target = new BossTarget { position = () => player, health = health, impulse = (x, y, z) => pushed = new Vec3(pushed.x + x, pushed.y + y, pushed.z + z) };
                kit = new BossKit(Cfg, events, new NullTelegraph(), target);
                bw = new Bulwark(Cfg, events, world, target, () => ++ids, (kinds, p) =>
                {
                    foreach (string k in kinds) spawned.Add((k, p));
                });
                bw.NewRun(7);
                kit.bosses["bulwark"] = bw;
                foreach (string name in new[] { "bossIntro", "bossRise", "bossPhase", "bossTell", "bossPunish", "bossStun", "bossBeat", "bossDying", "bossDown", "bossPart", "bossHit" })
                {
                    string n = name;
                    events.On(n, _ => said.Add(n));
                }
                events.On<BossEvent>("bossStun", e => stunSeconds = e.seconds);
                events.On<BossEvent>("bossPart", e => parts.Add(new BossEvent { part = e.part, reason = e.reason, points = e.points }));
                events.On<BossEvent>("bossDown", e => downPoints = e.reward?.points ?? -1);
                events.On<KillEvent>("kill", e => kill = e);
            }

            public void Start() => kit.Start("bulwark", 5);

            public void Step()
            {
                bw.Step(Cfg.Dt);
                kit.Step(Cfg.Dt);
            }

            public void Run(double seconds)
            {
                int n = S(seconds);
                for (int i = 0; i < n; i++) Step();
            }

            /// <summary>Steps until `done` (true) or `seconds` pass (false).</summary>
            public bool RunUntil(System.Func<bool> done, double seconds)
            {
                int n = S(seconds);
                for (int i = 0; i < n; i++)
                {
                    if (done()) return true;
                    Step();
                }
                return done();
            }

            public int Count(string name) => said.FindAll(s => s == name).Count;
        }

        [Test]
        public void AWallRam_StunsIt_DropsAPlate_OpensTheVent()
        {
            var r = new Rig(new Vec3(0, 0, 15));
            r.Start();
            Assert.That(r.RunUntil(() => r.bw.phase == "p1", 3), Is.True, "the intro ends in p1");
            r.bw.DebugAct("ram");
            Assert.That(r.bw.action, Is.EqualTo("ramTell"), "the ram's tell first");
            Assert.That(r.RunUntil(() => r.bw.action == "stunned", 5), Is.True, "it rams into the pen's wall and is stunned");
            Assert.That(r.stunSeconds, Is.EqualTo(B("ram.stun")), "bossStun {seconds: ram.stun}");
            Assert.That(r.parts.Count, Is.EqualTo(1), "one plate drops");
            Assert.That(r.parts[0].reason, Is.EqualTo("wall"));
            Assert.That(r.parts[0].points, Is.EqualTo(B("platePoints")));
            Assert.That(r.bw.VentOpen, Is.True, "the vent is open while it is stunned");
            Assert.That(r.Count("bossTell"), Is.EqualTo(1), "one tell (ram)");
            r.Run(B("ram.stun") - 2 * Cfg.Dt);
            Assert.That(r.bw.action, Is.EqualTo("stunned"), "stunned for ram.stun s");
            r.Run(3 * Cfg.Dt);
            Assert.That(r.bw.action, Is.Not.EqualTo("stunned"), "then it recovers");
            Assert.That(r.bw.VentOpen, Is.False, "the vent shuts (the back plate is still on)");
        }

        [Test]
        public void ThePlates_TakeATenthUntilBroken_ArmourPiercingTriples()
        {
            var intro = new Rig(new Vec3(0, 0, 15));
            intro.Start();
            double hp0 = intro.bw.body.hp;
            intro.bw.Hit(100, "revolver", 0, "frontL", out bool blocked);
            Assert.That(blocked, Is.True, "no damage in the intro");
            Assert.That(intro.bw.body.hp, Is.EqualTo(hp0));

            var r = new Rig(new Vec3(0, 0, 15));
            r.Start();
            Assert.That(r.RunUntil(() => r.bw.phase == "p1", 3), Is.True);
            Assert.That(r.bw.body.maxHp, Is.EqualTo(B("hp")), "round 5: the scale is 1");
            BulwarkPart frontL = r.bw.plates.Find(p => p.name == "frontL");
            Assert.That(frontL.hp, Is.EqualTo(B("plateHp")));
            double hp = r.bw.body.hp;
            Assert.That(r.bw.Hit(100, "revolver", 0, "frontL", out _), Is.EqualTo(100 * B("plateMult")).Within(1e-9), "a plate: x plateMult");
            Assert.That(r.bw.body.hp, Is.EqualTo(hp - 100 * B("plateMult")).Within(1e-9));
            Assert.That(frontL.hp, Is.EqualTo(B("plateHp") - 100).Within(1e-9), "the plate takes the whole hit");
            Assert.That(r.bw.Hit(100, "rocket", 0, "frontL", out _), Is.EqualTo(100 * B("plateMult") * B("apMult")).Within(1e-9), "armour-piercing: x apMult");
            Assert.That(frontL.broken, Is.True, "160 - 300: broken");
            Assert.That(r.parts.Count, Is.EqualTo(1));
            Assert.That(r.parts[0].reason, Is.EqualTo("shot"));
            Assert.That(r.bw.Hit(100, "revolver", 0, "frontL", out _), Is.EqualTo(100 * B("exposedMult")).Within(1e-9), "the bare socket: x1");
            Assert.That(r.bw.Hit(100, "revolver", B("apMomentum") + 0.1, "frontR", out _), Is.EqualTo(100 * B("plateMult") * B("apMult")).Within(1e-9), "momentum over apMomentum: armour-piercing");
            Assert.That(r.bw.Hit(100, "revolver", 0, "torso", out _), Is.EqualTo(100 * B("hullMult")).Within(1e-9), "the hull: x hullMult");
            Assert.That(r.bw.Hit(100, "revolver", 0, "core", out _), Is.EqualTo(100 * B("coreMult")).Within(1e-9), "the core, the vent shut: x coreMult");
        }

        [Test]
        public void AShotFromTheFront_HitsAFrontPlate()
        {
            var r = new Rig(new Vec3(0, 0, 15));
            r.Start();
            Assert.That(r.RunUntil(() => r.bw.phase == "p1", 3), Is.True);
            Vec3 p = r.bw.body.position;
            Vec3 plate = r.bw.PartPoint("frontL");
            var o = new Vec3(plate.x, plate.y, p.z + 12);
            double d = r.bw.body.RayHit(o, new Vec3(0, 0, -1), out bool head);
            Assert.That(d, Is.GreaterThan(0), "the ray hits it");
            Assert.That(head, Is.False);
            Assert.That(r.bw.hitPart, Is.EqualTo("frontL"), "the plate it faces");
            double miss = r.bw.body.RayHit(new Vec3(p.x + 10, p.y + 4, p.z + 12), new Vec3(0, 0, -1), out _);
            Assert.That(miss, Is.LessThan(0), "a ray beside it misses");
        }

        [Test]
        public void StandingStill_MakesItRam_MovingNever()
        {
            var r = new Rig(new Vec3(-6, 0, 12));
            r.Start();
            bool left = true;
            for (int i = 0; i < S(12); i++)
            {
                if (i % S(1) == 0)
                {
                    left = !left;
                    r.player = new Vec3(left ? -6 : 6, 0, 12); // 12 m apart: the passivity clock restarts
                }
                r.Step();
            }
            Assert.That(r.Count("bossPunish"), Is.EqualTo(0), "moving: never punished");
            int still = 0;
            while (r.Count("bossPunish") == 0 && still < S(12))
            {
                r.Step();
                still++;
            }
            Assert.That(r.Count("bossPunish"), Is.EqualTo(1), "standing still: punished");
            System.Console.WriteLine($"punished after {still * Cfg.Dt:0.00} s still");
            Assert.That(still * Cfg.Dt, Is.GreaterThanOrEqualTo(B("passive.time") - Cfg.Dt), "not before passive.time");
            Assert.That(r.bw.action, Is.EqualTo("ramTell"), "the punishment is a ram");
        }

        [Test]
        public void TheSchedule_ThenTheRotationWithItsMutators()
        {
            var k = BossKitParams.From(Cfg);
            var d = DirectorMath.From(Cfg);
            Assert.That(BossMath.BossForRound(5, k, d)?.boss, Is.EqualTo("bulwark"));
            Assert.That(BossMath.BossForRound(5, k, d)?.cycle, Is.EqualTo(0));
            Assert.That(BossMath.BossForRound(10, k, d)?.boss, Is.EqualTo("monolith"));
            Assert.That(BossMath.BossForRound(4, k, d).HasValue, Is.False);
            Assert.That(BossMath.BossForRound(6, k, d).HasValue, Is.False);
            BossDue r30 = BossMath.BossForRound(30, k, d).Value;
            Assert.That(r30.boss, Is.EqualTo("bulwark"));
            Assert.That(r30.cycle, Is.EqualTo(1));
            Assert.That(r30.mutator.id, Is.EqualTo("armored"));
            BossDue r55 = BossMath.BossForRound(55, k, d).Value;
            Assert.That(r55.boss, Is.EqualTo("bulwark"));
            Assert.That(r55.cycle, Is.EqualTo(2));
            Assert.That(r55.mutator.id, Is.EqualTo("swift"));
            BossScale s = BossMath.Scale("bulwark", 30, 1, r30.mutator, k);
            Assert.That(s.hp, Is.EqualTo((1 + k.hpPerRound * 25) * (1 + k.hpPerCycle) * 1.3 * k.coop).Within(1e-12));
            Assert.That(s.damage, Is.EqualTo(1 + k.damagePerCycle).Within(1e-12));
        }

        [Test]
        public void PhaseTwo_BeatsAndOpensTheHatches_ThenTheFinisherAndTheReward()
        {
            var r = new Rig(new Vec3(0, 0, 15));
            r.Start();
            Assert.That(r.RunUntil(() => r.bw.phase == "p1", 3), Is.True);
            r.bw.Hit(1e6, "revolver", 0, "torso", out _);
            Assert.That(r.bw.phase, Is.EqualTo("beat"), "under p2At: the beat");
            Assert.That(r.bw.body.hp, Is.EqualTo(r.bw.body.maxHp * B("p2At")).Within(1e-9), "held at the mark");
            Assert.That(r.Count("bossBeat"), Is.EqualTo(1));
            Assert.That(r.pushed.z, Is.GreaterThan(0), "the beat pushes you away from it");
            r.Run(B("hatchTell") + 2 * Cfg.Dt);
            Assert.That(r.spawned.Count, Is.EqualTo(r.bw.b.hatches.Count), "a husk up through every hatch");
            Assert.That(r.spawned[0].kind, Is.EqualTo(B("hp") > 0 ? Cfg.Str("bulwark.hatchKind") : ""));
            Assert.That(r.RunUntil(() => r.bw.phase == "p2", 3), Is.True, "then phase 2");
            r.bw.Hit(1e6, "revolver", 0, "torso", out _);
            Assert.That(r.bw.phase, Is.EqualTo("dying"));
            Assert.That(r.Count("bossDying"), Is.EqualTo(1));
            Assert.That(r.kill, Is.Null, "the kill waits for the finisher");
            Assert.That(r.RunUntil(() => !r.bw.Alive, 1), Is.True);
            Assert.That(r.kill, Is.Not.Null);
            Assert.That(r.kill.kind, Is.EqualTo("boss"));
            Assert.That(r.downPoints, Is.EqualTo(Cfg.Num("bossKit.rewards.bulwark.points")), "bossDown's reward");
            Assert.That(r.kit.Alive, Is.False);
        }
    }
}
