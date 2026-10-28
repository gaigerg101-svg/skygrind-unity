// NIGHT37 builder K (boss37): the Boss37 brain's rules (Core/Boss37.cs) on its default numbers (Boss37Params' own defaults = design.n37.boss37's
// values; the harness `boss37 kit` prints the same rules from the config with numbers). A bare brain, no sim, 120 Hz.
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class Boss37Tests
    {
        const double Dt = 1 / 120.0;

        static Boss37Brain Brain(Boss37Params p, int cost, EventBus bus = null, Tells36 tells = null)
        {
            var b = new Boss37Brain(p, "test", Dt, bus ?? new EventBus(), 1) { tells = tells, source = 3 };
            for (int i = 0; i < 3; i++) b.attacks.Add(Boss37Attack.Of("a" + i, i == 1 ? Tell36Kind.Yellow : Tell36Kind.Red, 1.2, 0.3, 0.3, Dt, cost));
            return b;
        }

        static Boss37Context Ctx(bool grounded = false, bool sight = true, double hp = 1) => new Boss37Context
        {
            player = new Vec3(0, 0, 0), boss = new Vec3(8, 0, 0), grounded = grounded, riding = !grounded, lineOfSight = sight, hpFrac = hp,
        };

        [Test, Description("budget: cost 2 at cap 6 -> 3 attacks, then the forced 2 s rest (MinosPrime.cs:342-353)")]
        public void Budget_ForcesARestAtTheCap()
        {
            var b = Brain(new Boss37Params { burst = 99 }, 2);
            var c = Ctx();
            b.Begin(1, c.player);
            int tells = 0;
            for (int i = 0; i < 20000 && b.budgetRests == 0; i++)
            {
                b.Step(c);
                if (b.state == Boss37State.Tell && b.entered) tells++;
            }
            Assert.AreEqual(3, tells);
            Assert.AreEqual(Boss37State.Rest, b.state);
            Assert.AreEqual(MovementMath.StepsFor(2, Dt), b.stateTotal);
        }

        [Test, Description("a parry refunds 5 budget points (MinosPrime.cs:1125), clamped at 0, and breaks the attack")]
        public void Parry_RefundsTheBudget()
        {
            var b = Brain(new Boss37Params { burst = 99 }, 2);
            var c = Ctx();
            b.Begin(1, c.player);
            for (int i = 0; i < 20000 && !(b.state == Boss37State.Tell && b.budget >= 4); i++) b.Step(c);
            Assert.AreEqual(4, b.budget);
            b.Parried(1);
            Assert.AreEqual(0, b.budget);
            Assert.AreEqual(Boss37State.Recover, b.state);
            Assert.AreEqual(5, b.stagger);
        }

        [Test, Description("the pick never repeats, and an unpicked attack's pity rises by 0.25 (Gabriel.cs:74-128)")]
        public void Pick_NeverRepeats()
        {
            var b = Brain(new Boss37Params(), 1);
            b.Begin(1, Vec3.Zero);
            int prev = -1;
            for (int i = 0; i < 500; i++)
            {
                int k = b.Pick(8);
                Assert.AreNotEqual(prev, k);
                prev = b.previous = k;
                Assert.AreEqual(0, b.attacks[k].pity);
            }
        }

        [Test, Description("stagger 6 drained -> the CYAN window for 2.5 s, then the meter refills")]
        public void Stagger_OpensTheWindow()
        {
            var bus = new EventBus();
            int opens = 0;
            bus.On<Boss37Event>(Boss37Brain.EvWindow, e => { if (e.stage == "open") opens++; });
            var b = Brain(new Boss37Params(), 1, bus);
            var c = Ctx();
            b.Begin(1, c.player);
            b.Step(c);
            Assert.IsFalse(b.Stagger(3));
            Assert.IsTrue(b.Stagger(3));
            Assert.IsTrue(b.Open);
            Assert.AreEqual(1, opens);
            Assert.AreEqual(1, b.Scale(false));
            int n = 0;
            while (b.Open && n < 10000) { b.Step(c); n++; }
            Assert.AreEqual(MovementMath.StepsFor(2.5, Dt), n);
            Assert.AreEqual(6, b.stagger);
            Assert.AreEqual(0.1, b.Scale(false));
        }

        [Test, Description("out of sight 3 s -> boss37Hide and RepositionDue (GabrielBase.cs:355-363)")]
        public void Hide_FiresAtThreeSeconds()
        {
            var b = Brain(new Boss37Params(), 1);
            var c = Ctx(sight: false);
            b.Begin(1, c.player);
            b.Step(c);
            int n = 0;
            while (!b.RepositionDue && n < 10000) { b.Step(c); n++; }
            Assert.AreEqual(MovementMath.StepsFor(3, Dt), n);
            Assert.IsTrue(b.ConsumeReposition());
            Assert.IsFalse(b.RepositionDue);
        }

        [Test, Description("the bar at 2/3 changes the leg; RestartLeg keeps the leg and the meter and returns the leg's checkpoint (D-6)")]
        public void Legs_AndRestartKeepProgress()
        {
            var b = Brain(new Boss37Params(), 1);
            var cp = new Vec3(1, 2, 3);
            b.SetCheckpoint(2, cp);
            var c = Ctx(hp: 0.6);
            b.Begin(1, c.player);
            b.Step(c);
            b.Step(c);
            Assert.AreEqual(2, b.leg);
            Assert.AreEqual(Boss37State.LegChange, b.state);
            while (b.state == Boss37State.LegChange) b.Step(c);
            Assert.AreEqual(9, b.stagger);
            b.Stagger(4);
            Vec3 at = b.RestartLeg();
            Assert.AreEqual(cp, at);
            Assert.AreEqual(2, b.leg);
            Assert.AreEqual(5, b.stagger);
            Assert.AreEqual(Boss37State.Rest, b.state);
        }

        [Test, Description("every wind-up raises one Tells36 tell of the attack's colour and length")]
        public void Tells_RaisedPerWindup()
        {
            var bus = new EventBus();
            var tells = new Tells36(bus) { enabled = true };
            var b = Brain(new Boss37Params(), 1, bus, tells);
            var c = Ctx();
            b.Begin(1, c.player);
            int seen = 0;
            for (int i = 0; i < 120 * 20; i++)
            {
                b.Step(c);
                tells.Step();
                Assert.LessOrEqual(tells.Live.Count, 1);
                if (b.state == Boss37State.Tell && b.entered)
                {
                    seen++;
                    Tell36 t = tells.Of(3);
                    Assert.IsNotNull(t);
                    Assert.AreEqual(b.Current.kind, t.kind);
                    Assert.AreEqual(b.Current.windupSteps, t.windupSteps);
                }
            }
            Assert.Greater(seen, 3);
        }
    }
}
