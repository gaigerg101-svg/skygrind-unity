// NIGHT24 builder B: THE KILN WYRM (Core/Enemies/Wyrm*.cs), the rules one by one. No UnityEngine: SimHarness runs the same assertions
// (`wyrm --selftest`, compiled with -p:Extra=../../Assets/Tests/EditMode/WyrmTests.cs). The fight itself (bot, tape, fingerprint) is
// `wyrm` in the harness.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class WyrmTests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static string Repo => Path.GetFullPath(Path.Combine(Port.Root, ".."));

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly Health health;
            public readonly Wyrm wyrm;
            public readonly BossKit kit;
            public Vec3 player = new Vec3(0, 0, 27.5);
            public readonly List<(double x, double y, double z)> pushes = new List<(double, double, double)>();
            public readonly List<Vec3> trail = new List<Vec3>();
            readonly double dt;

            public Rig(uint seed = 3, bool shed = false)
            {
                dt = Cfg.Dt;
                var arena = WyrmArena.Load(Path.Combine(Repo, "Design", "maps", "wyrm-arena.json"));
                health = new Health(100000, events, "player");
                var target = new BossTarget { position = () => player, health = health, impulse = (x, y, z) => pushes.Add((x, y, z)) };
                kit = new BossKit(Cfg, events, null, target);
                int ids = 0;
                WyrmParams wp = WyrmParams.From(Cfg, Wyrm.LoadBoss(Path.Combine(Repo, "Design", "bosses", "wyrm.json")));
                wp.tailShed = shed;
                wyrm = new Wyrm(Cfg, wp, events, arena.BuildWorld(Vec3.Zero), target, () => ++ids);
                kit.bosses["wyrm"] = wyrm;
                wyrm.NewRun(seed);
                kit.Start("wyrm");
            }

            public void Step()
            {
                wyrm.Step(dt);
                kit.Step(dt);
                trail.Add(wyrm.HeadPosition);
            }

            public void Until(Func<bool> done, int max)
            {
                for (int i = 0; i < max && !done(); i++) Step();
                Assert.That(done(), Is.True, "the condition was not reached in " + max + " steps");
            }
        }

        [Test]
        public void ARetryInPhase2Or3_RestoresTheHpToThatPhasesThreshold_NotToFull_AndKeepsTheStream()
        {
            var r = new Rig(7);
            Assert.That(r.wyrm.RestorePhaseStart(), Is.False, "the intro: nothing to restore (a full restart is the start of phase 1)");
            r.Until(() => r.wyrm.phase == "p1", 120 * 12);
            Assert.That(r.wyrm.RetryPhase, Is.Null, "phase 1");
            r.wyrm.body.hp = r.wyrm.body.maxHp * 0.6;
            r.wyrm.Apply(0); // under 66 %: the beat into phase 2
            r.Until(() => r.wyrm.phase == "p2", 120 * 8);
            r.wyrm.Apply(r.wyrm.body.maxHp * 0.2); // 66 % to about 46 %... a hit mid-phase
            uint stream = r.wyrm.RngState;
            Assert.That(r.wyrm.RestorePhaseStart(), Is.True);
            Assert.That(Math.Abs(r.wyrm.HpFrac - r.wyrm.p.phase2At) < 1e-9, Is.True, "HP at phase 2 threshold");
            Assert.That(r.wyrm.RngState, Is.EqualTo(stream), "the same stream continued");
            Assert.That(r.wyrm.phase, Is.EqualTo("p2"));
            Assert.That(r.wyrm.shots.Count, Is.EqualTo(0));
            r.wyrm.body.hp = r.wyrm.body.maxHp * 0.3;
            r.wyrm.Apply(0);
            Assert.That(r.wyrm.phase, Is.EqualTo("beat"));
            Assert.That(r.wyrm.RetryPhase, Is.EqualTo("p3"), "a death inside the beat restores the phase it is entering");
            Assert.That(r.wyrm.RestorePhaseStart(), Is.True);
            Assert.That(r.wyrm.phase, Is.EqualTo("p3"));
            Assert.That(Math.Abs(r.wyrm.HpFrac - r.wyrm.p.phase3At) < 1e-9, Is.True);
        }

        [Test]
        public void RayAgainstAnOrientedBox_HitsTheTurnedBox_AndMissesWhereTheAxisBoxWouldHit()
        {
            var b = new WyrmBox { c = new Vec3(0, 0, 0), hr = 0.5, hu = 0.5, hf = 3 };
            b.Orient(Vec3.Normalize(new Vec3(1, 0, 1))); // a long box lying along the diagonal
            double along = b.Ray(new Vec3(-5, 0, -5), Vec3.Normalize(new Vec3(1, 0, 1)));
            Assert.That(Math.Abs(along - (Math.Sqrt(50) - 3)) < 1e-9, Is.True, "down the box's length");
            Assert.That(b.Ray(b.r * 1.0 - b.f * 10, b.f), Is.EqualTo(-1.0), "a ray along the box one metre to the side misses (half width 0.5)");
            Assert.That(b.Ray(b.r * 0.4 - b.f * 10, b.f) > 0, Is.True, "and 0.4 m to the side it hits");
            Assert.That(b.Ray(new Vec3(0, 0, 0), new Vec3(1, 0, 0)), Is.EqualTo(0.0), "from inside");
            Assert.That(b.Touches(new Vec3(0.6, 0, -0.6), 0.9), Is.True);
            Assert.That(b.Touches(new Vec3(3, 0, -3), 0.9), Is.False);
        }

        [Test]
        public void ThePath_ClampsItsPitch_AndHasAConstantArcLength()
        {
            var wps = new List<Vec3> { new Vec3(0, 20, 5), new Vec3(0, 40, 6) };
            WyrmPath free = WyrmMath.BuildPath(new Vec3(0, 0, 0), new Vec3(0, 0, 1), wps, 89, 0.5);
            WyrmPath flat = WyrmMath.BuildPath(new Vec3(0, 0, 0), new Vec3(0, 0, 1), wps, 30, 0.5);
            double maxFree = 0, maxFlat = 0;
            for (int i = 1; i < flat.pts.Length - 1; i++)
            {
                Vec3 d = flat.pts[i] - flat.pts[i - 1];
                maxFlat = Math.Max(maxFlat, Math.Abs(d.y) / d.Length);
                Assert.That(Math.Abs(d.Length - 0.5) < 0.06, Is.True, "constant arc length " + d.Length);
            }
            for (int i = 1; i < free.pts.Length; i++)
            {
                Vec3 d = free.pts[i] - free.pts[i - 1];
                maxFree = Math.Max(maxFree, Math.Abs(d.y) / d.Length);
            }
            Assert.That(maxFlat <= 0.5 + 1e-6, Is.True, "sin 30 degrees");
            Assert.That(maxFree > 0.8, Is.True, "unclamped, it climbs steeply");
        }

        [Test]
        public void ThePathsWaypointArcLengths_PointAtTheWaypoints()
        {
            var wps = new List<Vec3> { new Vec3(10, 0, 0), new Vec3(10, 0, 10), new Vec3(0, 0, 10) };
            WyrmPath path = WyrmMath.BuildPath(new Vec3(0, 0, 0), new Vec3(1, 0, 0), wps, 89, 0.25);
            for (int i = 0; i < wps.Count; i++)
            {
                Vec3 at = path.At(path.wpS[i], out _);
                Assert.That(Vec3.DistanceTo(at, wps[i]) < 0.3, Is.True, "waypoint " + i + " is " + Vec3.DistanceTo(at, wps[i]) + " m from its arc length");
            }
        }

        [Test]
        public void EveryAttackTell_IsAtLeastHalfASecond_AndTheyAreAllInTheTable()
        {
            WyrmParams p = WyrmParams.From(Cfg, Wyrm.LoadBoss(Path.Combine(Repo, "Design", "bosses", "wyrm.json")));
            int min = MovementMath.StepsFor(0.5, Cfg.Dt);
            Assert.That(MovementMath.StepsFor(p.resolveGap, Cfg.Dt) >= min, Is.True);
            foreach (string need in new[] { "lunge", "dive", "crack", "spit", "core", "squeeze" })
            {
                Assert.That(p.tells.ContainsKey(need), Is.True, need);
                Assert.That(MovementMath.StepsFor(p.tells[need], Cfg.Dt) >= min, Is.True, need);
            }
            var all = new List<WyrmPattern>(p.patterns.Values) { p.emerge };
            foreach (WyrmPattern pat in all)
                foreach (WyrmVariant v in pat.variants)
                    foreach (WyrmMarkDef m in v.marks)
                        Assert.That(p.tells.ContainsKey(m.tell), Is.True, m.name);
            Assert.That(string.Join(",", p.cycle), Is.EqualTo("coil,dive,lunge,loop"));
        }

        [Test]
        public void EachPiece_SitsOnTheHeadsTrail_SpacingStepsBack()
        {
            var r = new Rig();
            r.Until(() => r.wyrm.stepNo >= 600, 700);
            int sp = r.wyrm.p.spacingSteps;
            for (int i = 1; i < r.wyrm.PieceCount; i += 5)
            {
                Vec3 want = r.trail[r.trail.Count - 1 - i * sp];
                Assert.That(Vec3.DistanceTo(r.wyrm.boxes[i].c, want) < 1e-9, Is.True, "piece " + i);
            }
            Assert.That(r.wyrm.PieceCount, Is.EqualTo(26), "24 body + head + tail");
        }

        [Test]
        public void ContactHurts_LittleOnTheBody_MoreOnTheHead_WithACooldownAndAPush()
        {
            var r = new Rig();
            r.Until(() => r.wyrm.phase == "p1", 600);
            for (int i = 0; i < 40; i++) r.Step(); // out of the floor: pieces above ground
            Assert.That(r.wyrm.buried[8], Is.False);
            // far from everything: no contact
            r.player = new Vec3(0, 0, 27.5);
            r.Step();
            double hp0 = r.health.hp;
            Assert.That(hp0, Is.EqualTo(100000.0));
            // stand in a body piece: the body's contact damage
            WyrmBox b = r.wyrm.boxes[8];
            r.player = new Vec3(b.c.x, b.c.y - 0.9, b.c.z);
            r.Step();
            double bodyHit = hp0 - r.health.hp;
            Assert.That(Math.Abs(bodyHit - r.wyrm.p.contactBody) < 1e-9, Is.True, "body contact");
            Assert.That(r.pushes.Count, Is.EqualTo(1), "and a push");
            Assert.That(r.pushes[0].y, Is.EqualTo(r.wyrm.p.pushUp));
            // the cooldown: no second hit at once
            double hp1 = r.health.hp;
            r.Step();
            Assert.That(r.health.hp, Is.EqualTo(hp1), "cooldown");
            r.player = new Vec3(0, 0, 27.5);
            for (int i = 0; i < MovementMath.StepsFor(r.wyrm.p.contactCooldown, Cfg.Dt) + 2; i++) r.Step();
            // the head: more than the body
            double hp2 = r.health.hp;
            WyrmBox h = r.wyrm.boxes[0];
            r.player = new Vec3(h.c.x, h.c.y - 0.9, h.c.z);
            r.Step();
            double headHit = hp2 - r.health.hp;
            Assert.That(headHit > bodyHit, Is.True, "the head hurts more than the body");
            Assert.That(Math.Abs(headHit - r.wyrm.p.contactHead) < 1e-9 || Math.Abs(headHit - r.wyrm.p.contactLunge) < 1e-9, Is.True);
        }

        [Test]
        public void ShotsShareOnePool_BodyOne_HeadTwo_ExposedCoreThree_AndBlockedInTheIntro()
        {
            var r = new Rig();
            Wyrm w = r.wyrm;
            // the intro: nothing lands
            Assert.That(w.phase, Is.EqualTo("intro"));
            r.Until(() => !w.buried[0], 600);
            Vec3 above = new Vec3(w.boxes[0].c.x, w.boxes[0].c.y + 15, w.boxes[0].c.z);
            Assert.That(w.TakeHit(above, new Vec3(0, -1, 0), 25, "revolver", out _), Is.EqualTo(0.0), "blocked in the intro");
            r.Until(() => w.phase == "p1", 600);
            double hp = w.body.hp;
            Assert.That(w.TakeHit(new Vec3(w.boxes[0].c.x, w.boxes[0].c.y + 15, w.boxes[0].c.z), new Vec3(0, -1, 0), 25, "revolver", out double dist), Is.EqualTo(25 * w.p.headMult), "head x2");
            Assert.That(w.hitPart, Is.Null);
            Assert.That(hp - w.body.hp, Is.EqualTo(50.0));
            hp = w.body.hp;
            WyrmBox mid = w.boxes[10];
            Assert.That(w.TakeHit(new Vec3(mid.c.x, mid.c.y + 15, mid.c.z), new Vec3(0, -1, 0), 25, "revolver", out _), Is.EqualTo(25 * w.p.bodyMult), "body x1");
            Assert.That(hp - w.body.hp, Is.EqualTo(25.0));
            // a miss
            Assert.That(w.TakeHit(new Vec3(0, 28, 0), new Vec3(0, 1, 0), 25, "revolver", out _), Is.EqualTo(-1.0));
            // the core: phase 2, after the roar
            w.body.hp = w.body.maxHp * 0.6;
            w.Apply(0);
            Assert.That(w.phase, Is.EqualTo("beat"));
            double beatShot = w.TakeHit(new Vec3(0, 28, 0), new Vec3(0, -1, 0), 25, "revolver", out _);
            Assert.That(beatShot == -1 || beatShot == 0, Is.True, "blocked in the beat (or a miss)");
            r.Until(() => w.CoreOpen, 120 * 40);
            WyrmBox hb = w.boxes[0];
            Vec3 core = hb.c + hb.u * w.p.coreUp - hb.f * w.p.coreBack;
            double before = w.body.hp;
            double dmg = w.TakeHit(core + new Vec3(0, 6, 0), new Vec3(0, -1, 0), 25, "revolver", out _);
            Assert.That(dmg, Is.EqualTo(25 * w.p.coreMult), "core x3");
            Assert.That(before - w.body.hp, Is.EqualTo(75.0));
        }

        [Test]
        public void ThePhases_ComeAt66And33Percent_WithABeat_AndTheSpeedRises25PercentEach()
        {
            var r = new Rig();
            Wyrm w = r.wyrm;
            r.Until(() => w.phase == "p1", 600);
            Assert.That(w.PhaseSpeedMult, Is.EqualTo(1.0));
            w.body.hp = w.body.maxHp * 0.65;
            w.Apply(0);
            Assert.That(w.phase, Is.EqualTo("beat"));
            Assert.That(r.events.Emitted.ContainsKey("bossBeat"), Is.True);
            r.Until(() => w.phase == "p2", 400);
            Assert.That(Math.Abs(w.PhaseSpeedMult - 1.25) < 1e-12, Is.True);
            w.body.hp = w.body.maxHp * 0.3;
            w.Apply(0);
            r.Until(() => w.phase == "p3", 400);
            Assert.That(Math.Abs(w.PhaseSpeedMult - 1.25 * 1.25) < 1e-12, Is.True);
            w.body.hp = 1;
            w.Apply(5);
            Assert.That(w.phase, Is.EqualTo("dying"));
            Assert.That(r.events.Emitted.ContainsKey("bossDying"), Is.True);
            r.Until(() => !w.Alive, 400);
            Assert.That(r.events.Emitted.ContainsKey("bossDown"), Is.True);
        }

        [Test]
        public void TailShed_IsOffByDefault_AndWhenOnCutsTheTailLooseAtTheBeat_ItsOwnHp_ABonusWhenShotDown()
        {
            Assert.That(Cfg.Design("n24.wyrm.tailShed.enabled"), Is.False);
            var off = new Rig(3, false);
            off.Until(() => off.wyrm.phase == "p1", 600);
            off.wyrm.body.hp = off.wyrm.body.maxHp * 0.6;
            off.wyrm.Apply(0);
            off.Until(() => off.wyrm.phase == "p2", 400);
            Assert.That(off.wyrm.ShedActive, Is.False, "off: no shed");
            var r = new Rig(3, true);
            Wyrm w = r.wyrm;
            r.Until(() => w.phase == "p1", 600);
            r.Until(() => !w.buried[w.PieceCount - 1] && w.stepNo > 500, 900);
            w.body.hp = w.body.maxHp * 0.6;
            w.Apply(0);
            r.Until(() => w.ShedActive, 400);
            int mid = w.ShedFirst + 1;
            Assert.That(w.buried[mid], Is.False);
            double hp = w.body.hp, shed = w.ShedHp;
            Vec3 c = w.boxes[mid].c;
            r.Until(() => w.phase == "p2", 400);
            c = w.boxes[mid].c;
            w.TakeHit(new Vec3(c.x, c.y + 14, c.z), new Vec3(0, -1, 0), 25, "revolver", out _);
            Assert.That(w.ShedHp, Is.EqualTo(shed - 25.0), "the hit went to the tail's own pool");
            Assert.That(w.body.hp, Is.EqualTo(hp), "and not to the worm's");
            w.TakeHit(new Vec3(c.x, c.y + 14, c.z), new Vec3(0, -1, 0), 1e6, "revolver", out _);
            Assert.That(w.ShedActive, Is.False);
            r.Step();
            Assert.That(w.buried[mid], Is.True, "gone for good");
            Assert.That(w.body.hp < hp, Is.True, "the bonus took HP off the worm");
        }

        [Test]
        public void TwoFightsOnOneSeed_AreStepExact_AndAnotherSeedDrawsItsOwnStream()
        {
            var a = new Rig(11);
            var b = new Rig(11);
            for (int i = 0; i < 4000; i++)
            {
                a.Step();
                b.Step();
            }
            Assert.That(a.wyrm.HeadPosition.x == b.wyrm.HeadPosition.x && a.wyrm.HeadPosition.y == b.wyrm.HeadPosition.y && a.wyrm.HeadPosition.z == b.wyrm.HeadPosition.z, Is.True);
            Assert.That(a.wyrm.RngState, Is.EqualTo(b.wyrm.RngState));
            Assert.That(a.wyrm.PatternName, Is.EqualTo(b.wyrm.PatternName));
        }

        [Test]
        public void TheWorm_KeepsClearOfThePlayerSpawn_AllThroughACycle()
        {
            var r = new Rig(5);
            Vec3 spawn = new Vec3(0, 0.9, 27.5);
            double min = 1e9;
            for (int i = 0; i < 10000; i++)
            {
                r.Step();
                for (int k = 0; k < r.wyrm.PieceCount; k++)
                    if (!r.wyrm.buried[k]) min = Math.Min(min, Vec3.DistanceTo(spawn, r.wyrm.boxes[k].c));
            }
            Assert.That(min >= r.wyrm.p.spawnClear, Is.True, "nearest " + min);
        }
    }
}
