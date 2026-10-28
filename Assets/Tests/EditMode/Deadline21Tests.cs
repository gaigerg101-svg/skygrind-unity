// NIGHT21 U3-35.6 DEADLINE (builder S): the fuse (Core/Deadline21.cs) on a bare event bus with a real Health, and the twist's
// draw (SpecialRoundMath.TwistRounds with the extra id). Numbers: design.n21.deadline (Design/unity-config.json). The sim's wiring
// is SurvivalSim.Deadline21.cs; its short scripted run is the SimHarness `deadline` command.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class Deadline21Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        sealed class Rig
        {
            public readonly EventBus bus = new EventBus();
            public readonly Health health;
            public readonly Deadline21 fuse;
            public bool sliding;
            public readonly List<(double left, bool refill)> ticks = new List<(double, bool)>();
            public readonly List<(double hit, int burns)> burns = new List<(double, int)>();

            public Rig(bool on = true)
            {
                health = new Health(100, bus, "player");
                fuse = new Deadline21(Cfg, bus, health, () => sliding);
                bus.On<DeadlineEvent>("deadlineTick", e => ticks.Add((e.left, e.refill)));
                bus.On<DeadlineEvent>("deadlineBurn", e => burns.Add((e.hit, e.burns)));
                fuse.NewRun(on);
            }

            public void Twist(string phase, int round = 9) =>
                bus.Emit(N20Events.SpecialRound, new SpecialRoundEvent { name = "twist:deadline", round = round, phase = phase });

            public void Steps(int n)
            {
                for (int i = 0; i < n; i++) fuse.Step();
            }

            public void Kill(string weapon = "rifle") => bus.Emit("kill", new KillEvent { weapon = weapon });
        }

        static int S(double seconds) => MovementMath.StepsFor(seconds, Cfg.Dt);

        [Test]
        public void TheNumbers_AreTheRowsOwn()
        {
            Deadline21.P p = Deadline21.P.From(Cfg);
            Assert.That(p.fuse, Is.GreaterThan(0));
            Assert.That(p.slideRefill, Is.GreaterThan(p.killRefill), "a slide kill pays more than a plain kill");
            Assert.That(p.slamRefill, Is.GreaterThan(p.slideRefill), "a slam kill pays the most");
            Assert.That(p.hitFrac, Is.InRange(0.1, 0.9), "heavy, never the run");
            Assert.That(p.pointsMult, Is.EqualTo(2), "the round's points double");
            Assert.That(p.burnFuse, Is.LessThanOrEqualTo(p.fuse));
        }

        [Test]
        public void TheFuse_StartsFullAtTheTwistsStart_AndOnlyRunsDuringTheRound()
        {
            var rig = new Rig();
            rig.Steps(100);
            Assert.That(rig.fuse.Running, Is.False, "no twist: no fuse");
            rig.Twist("start");
            Assert.That(rig.fuse.Running, Is.True);
            Assert.That(rig.fuse.LeftSeconds, Is.EqualTo(Deadline21.P.From(Cfg).fuse).Within(Cfg.Dt));
            rig.Steps(S(1));
            Assert.That(rig.fuse.LeftSeconds, Is.EqualTo(Deadline21.P.From(Cfg).fuse - 1).Within(Cfg.Dt * 2));
            rig.Twist("end");
            Assert.That(rig.fuse.Running, Is.False);
            double left = rig.fuse.LeftSeconds;
            rig.Steps(S(5));
            Assert.That(rig.fuse.LeftSeconds, Is.EqualTo(left), "the round over: the fuse stands");
            Assert.That(rig.burns, Is.Empty);
        }

        [Test]
        public void BurningOut_IsOneHeavyHit_AndAShorterFuse_NeverTheRun()
        {
            var rig = new Rig();
            Deadline21.P p = Deadline21.P.From(Cfg);
            rig.Twist("start");
            rig.Steps(S(p.fuse) + 2);
            Assert.That(rig.burns.Count, Is.EqualTo(1), "deadlineBurn once");
            Assert.That(rig.burns[0].hit, Is.EqualTo(100 * p.hitFrac).Within(1e-9));
            Assert.That(rig.health.hp, Is.EqualTo(100 - 100 * p.hitFrac).Within(1e-9));
            Assert.That(rig.fuse.LeftSeconds, Is.LessThanOrEqualTo(p.burnFuse + Cfg.Dt), "the fuse restarts at burnFuse");
            // burn out again and again: the hit stops at 1 HP
            for (int i = 0; i < 6; i++) rig.Steps(S(p.burnFuse) + 2);
            Assert.That(rig.health.Dead, Is.False, "burning out never kills");
            Assert.That(rig.health.hp, Is.GreaterThanOrEqualTo(1 - 1e-9));
            Assert.That(rig.burns.Count, Is.GreaterThan(3));
        }

        [Test]
        public void Kills_RefillTheFuse_SlideAndSlamMost_CappedAtItsLength()
        {
            var rig = new Rig();
            Deadline21.P p = Deadline21.P.From(Cfg);
            rig.Twist("start");
            rig.Steps(S(p.fuse - 5)); // all but 5 s burnt (was a fixed 20 s for the 25 s fuse; the fuse is configured, 18 s after d710b81)
            double left = rig.fuse.LeftSeconds;
            rig.Kill();
            double plain = rig.fuse.LeftSeconds - left;
            Assert.That(plain, Is.EqualTo(p.killRefill).Within(Cfg.Dt * 2));
            left = rig.fuse.LeftSeconds;
            rig.sliding = true;
            rig.Kill();
            double slide = rig.fuse.LeftSeconds - left;
            Assert.That(slide, Is.EqualTo(Deadline21.P.From(Cfg).slideRefill).Within(Cfg.Dt * 2).Or.EqualTo(p.fuse - left).Within(Cfg.Dt * 2), "a slide kill: the slide refill (or what is left of the cap)");
            rig.sliding = false;
            rig.Steps(S(10)); // 10 s burnt from the slide's refill, short of a burn-out
            left = rig.fuse.LeftSeconds;
            rig.Kill("slam");
            double slam = rig.fuse.LeftSeconds - left;
            Assert.That(slam, Is.GreaterThan(plain), "a slam kill pays more than a plain one");
            for (int i = 0; i < 20; i++) rig.Kill("slam");
            Assert.That(rig.fuse.LeftSeconds, Is.EqualTo(p.fuse).Within(Cfg.Dt * 2), "never past the fuse's length");
            Assert.That(rig.ticks.Exists(t => t.refill), Is.True, "a refill says deadlineTick");
        }

        [Test]
        public void TheFuse_Ticks_OnceASecond()
        {
            var rig = new Rig();
            rig.Twist("start");
            rig.ticks.Clear();
            rig.Steps(S(5) + 1);
            Assert.That(rig.ticks.Count, Is.InRange(4, 6), "deadlineTick every tickEvery s");
        }

        [Test]
        public void TheRowOff_DoesNothing()
        {
            var rig = new Rig(false);
            rig.Twist("start");
            rig.Steps(S(Deadline21.P.From(Cfg).fuse) + 10);
            rig.Kill();
            Assert.That(rig.fuse.Running, Is.False);
            Assert.That(rig.burns, Is.Empty);
            Assert.That(rig.ticks, Is.Empty);
            Assert.That(rig.health.hp, Is.EqualTo(100));
        }

        [Test]
        public void TheDraw_WithoutTheExtra_IsNight20sBitForBit_AndWithIt_AddsDeadlineNeverTwiceRunning()
        {
            SpecialRoundMath.TwistP p = SpecialRoundMath.TwistP.From(Cfg);
            DirectorMath.D d = DirectorMath.From(Cfg);
            int deadlines = 0;
            for (uint seed = 1; seed <= 60; seed++)
            {
                var plain = SpecialRoundMath.TwistRounds(seed, 200, p, d, null);
                var plain2 = SpecialRoundMath.TwistRounds(seed, 200, p, d, null, null);
                Assert.That(plain2, Is.EqualTo(plain), "no extra id: NIGHT20's draw");
                foreach (var t in plain) Assert.That(t.id, Is.Not.EqualTo("deadline"));
                var with = SpecialRoundMath.TwistRounds(seed, 200, p, d, null, "deadline", 1);
                Assert.That(with.Count, Is.EqualTo(plain.Count), "the same rounds twist");
                string last = null;
                for (int i = 0; i < with.Count; i++)
                {
                    Assert.That(with[i].round, Is.EqualTo(plain[i].round));
                    Assert.That(with[i].id, Is.Not.EqualTo(last), "never the same twist twice in a row");
                    Assert.That(with[i].id == "deadline" || System.Array.IndexOf(p.ids, with[i].id) >= 0);
                    if (with[i].id == "deadline") deadlines++;
                    last = with[i].id;
                }
                var none = SpecialRoundMath.TwistRounds(seed, 200, p, d, null, "deadline", 0);
                foreach (var t in none) Assert.That(t.id, Is.Not.EqualTo("deadline"), "weight 0: never drawn");
            }
            Assert.That(deadlines, Is.GreaterThan(30), "one twist among five: about a fifth of the draws");
        }
    }
}
