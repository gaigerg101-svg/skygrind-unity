// Port of skygrind/tests/survivalS1.test.js (regen and points; same expectations and tolerances), plus NIGHT7 S4's run
// loop: a fake spawner proves break -> fight -> clear -> break, the round-clear bonus, and death ending the run.
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class SurvivalS1Tests
    {
        static GameConfig cfg;
        static RegenMath.Params r;
        static double dt;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            r = RegenMath.From(cfg);
            dt = cfg.Dt;
        }

        /// <summary>Run steps until `until(state, hp)` or a cap; returns seconds and the final hp.</summary>
        static (double t, double hp) Run(RegenState s, double speed, bool camping, double hp, double max, System.Func<RegenState, double, bool> until, double cap = 20, RegenMath.Perk? perk = null)
        {
            double t = 0;
            while (t < cap)
            {
                hp += RegenMath.Step(s, dt, speed, camping, hp, max, r, perk ?? RegenMath.Perk.None);
                t += dt;
                if (until(s, hp)) break;
            }
            return (t, hp);
        }

        static RegenState Damaged(RegenMath.Perk? perk = null)
        {
            var s = new RegenState();
            RegenMath.OnDamage(s, r, perk ?? RegenMath.Perk.None);
            return s;
        }

        [Test]
        public void WaitsTheFullDelay_ThenRegeneratesAtTheRate()
        {
            RegenState s = Damaged();
            var wait = Run(s, 0, false, 40, 100, (st, _) => st.regenerating);
            Assert.That(wait.t, Is.EqualTo(r.delay).Within(0.05));
            var fill = Run(s, 0, false, 40, 100, (_, hp) => hp >= 100);
            Assert.That(fill.t, Is.EqualTo(60 / r.rate).Within(0.05));
            Assert.That(fill.hp, Is.EqualTo(100));
        }

        [Test]
        public void TheDelayCountsDownFaster_MovingFastOrAfterDealing()
        {
            Assert.That(RegenMath.CountdownRate(r.fastSpeed + 1, double.PositiveInfinity, r), Is.EqualTo(r.fastMult));
            Assert.That(RegenMath.CountdownRate(r.fastSpeed - 1, double.PositiveInfinity, r), Is.EqualTo(1));
            Assert.That(RegenMath.CountdownRate(0, r.dealtWindow * 0.5, r), Is.EqualTo(r.fastMult));
            var t = Run(Damaged(), 12, false, 40, 100, (st, _) => st.regenerating).t;
            Assert.That(t, Is.EqualTo(r.delay / r.fastMult).Within(0.05));
            RegenState dealt = Damaged();
            RegenMath.OnDealt(dealt);
            var t2 = Run(dealt, 0, false, 40, 100, (st, _) => st.regenerating).t;
            Assert.That(t2, Is.EqualTo(r.delay - r.dealtWindow * (r.fastMult - 1)).Within(0.05));
        }

        [Test]
        public void PausedWhileCamping()
        {
            RegenState s = Damaged();
            var camped = Run(s, 0, true, 40, 100, (_, __) => false, 10);
            Assert.That(camped.hp, Is.EqualTo(40));
            Assert.That(s.delay, Is.EqualTo(r.delay));
            s.delay = 0;
            Assert.That(RegenMath.Step(s, dt, 0, true, 40, 100, r, RegenMath.Perk.None), Is.EqualTo(0));
            Assert.That(RegenMath.Step(s, dt, 0, false, 40, 100, r, RegenMath.Perk.None), Is.EqualTo(r.rate * dt).Within(1e-6));
        }

        [Test]
        public void NeverOverfills_NeverRevives_MendScales()
        {
            var s = new RegenState();
            Assert.That(RegenMath.Step(s, dt, 0, false, 99.9, 100, r, RegenMath.Perk.None), Is.EqualTo(0.1).Within(1e-6));
            Assert.That(RegenMath.Step(s, dt, 0, false, 0, 100, r, RegenMath.Perk.None), Is.EqualTo(0));
            var mend = new RegenMath.Perk { delay = 0.6, rate = 1.5 };
            RegenState m = Damaged(mend);
            Assert.That(m.delay, Is.EqualTo(r.delay * 0.6).Within(1e-6));
            m.delay = 0;
            Assert.That(RegenMath.Step(m, dt, 0, false, 10, 100, r, mend), Is.EqualTo(r.rate * 1.5 * dt).Within(1e-6));
        }

        [Test]
        public void BasePoints_AndRankAndCamping()
        {
            PointsMath.Params p = PointsMath.From(cfg);
            Assert.That(PointsMath.BasePoints(false, false, false, "revolver", false, p), Is.EqualTo(10));
            Assert.That(PointsMath.BasePoints(false, true, false, "shotgun", false, p), Is.EqualTo(60));
            Assert.That(PointsMath.BasePoints(false, true, true, "revolver", false, p), Is.EqualTo(100));
            Assert.That(PointsMath.BasePoints(false, true, false, "punch", false, p), Is.EqualTo(130));
            Assert.That(PointsMath.BasePoints(false, false, false, "punch", false, p), Is.EqualTo(10));
            Assert.That(PointsMath.BasePoints(true, false, false, null, false, p), Is.EqualTo(50));
            Assert.That(p.start, Is.EqualTo(500));
            Assert.That(PointsMath.RoundClearPoints(7, p), Is.EqualTo(700));
            Assert.That(new[] { 0, 1, 2, 3, 4, 5 }, Has.All.Matches<int>(k => PointsMath.RankMultiplier(k, p) == new[] { 1, 1.1, 1.25, 1.5, 1.75, 2 }[k]));
            Assert.That(PointsMath.RankMultiplier(9, p), Is.EqualTo(2));
            Assert.That(PointsMath.ApplyMultipliers(60, PointsMath.RankMultiplier(3, p)), Is.EqualTo(90));
            Assert.That(PointsMath.ApplyMultipliers(100, PointsMath.RankMultiplier(5, p)), Is.EqualTo(200));
            Assert.That(PointsMath.ApplyMultipliers(10, PointsMath.RankMultiplier(1, p)), Is.EqualTo(11));
            Assert.That(PointsMath.ApplyMultipliers(60, PointsMath.RankMultiplier(5, p), p.campMult), Is.EqualTo(60));
            Assert.That(PointsMath.ApplyMultipliers(130, 1, p.campMult), Is.EqualTo(65));
            Assert.That(PointsMath.CanAfford(750, 750), Is.True);
            Assert.That(PointsMath.CanAfford(749, 750), Is.False);
        }

        sealed class FakeSpawner : IRoundSpawner
        {
            public int started;
            public bool done;
            public void StartWave(int round, uint seed) { started = round; done = false; }
            public void Step(double dt) { }
            public bool Done => done;
            public void Clear() { done = false; }
        }

        [Test]
        public void TheRoundLoop_BreakFightClear_AndDeathEnds()
        {
            var events = new EventBus();
            var health = new Health(cfg.Num("health.max"), events, "player");
            var regen = new Regen(cfg, events, health, () => 0);
            var economy = new Economy(cfg, events);
            var spawner = new FakeSpawner();
            var run = new SurvivalRun(cfg, events, spawner, economy, regen);
            run.Start(42);
            Assert.That(run.state, Is.EqualTo(SurvivalRun.State.Break));
            Assert.That(run.round, Is.EqualTo(1));
            Assert.That(economy.total, Is.EqualTo(500));
            int firstBreak = MovementMath.StepsFor(cfg.Num("survival.firstBreakTime"), dt);
            for (int i = 0; i < firstBreak; i++) run.Step(dt);
            Assert.That(run.state, Is.EqualTo(SurvivalRun.State.Fight));
            Assert.That(spawner.started, Is.EqualTo(1));
            events.Emit("hit", new HitEvent { weapon = "revolver", killed = true, headshot = true });
            Assert.That(economy.total, Is.EqualTo(600));
            spawner.done = true;
            run.Step(dt);
            Assert.That(run.state, Is.EqualTo(SurvivalRun.State.Break));
            Assert.That(run.round, Is.EqualTo(2));
            Assert.That(economy.total, Is.EqualTo(700), "+100 x round 1");
            Assert.That(run.BreakLeft, Is.EqualTo(cfg.Num("survival.breakTime")).Within(dt));
            health.Damage(1000, "husk");
            RunStats stats = run.End();
            Assert.That(stats.round, Is.EqualTo(2));
            Assert.That(stats.killedBy, Is.EqualTo("husk"));
            Assert.That(run.Live, Is.False);
        }
    }
}
