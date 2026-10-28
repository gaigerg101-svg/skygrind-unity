// EXT53 builder N (row N, the sprint contract written before the sim): RIDE KILL's pure rule (Core/RideKill53.cs) and its config row
// (design.n53.rideKill53). No UnityEngine, so it also runs headless on the harness's mini NUnit:
//   dotnet run --project Tools/SimHarness -c Release -p:Extra=../../Assets/Tests/EditMode/RideKill53Tests.cs -- n53ridekill selftest
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class RideKill53Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root(), "config.json"));
        const double Dt = 1.0 / 120;

        static string Root()
        {
            try { return Port.Root; }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        [Test]
        public void TheRow_IsASimRowOfNight53_OffUnlessAFreezeSaysWhy_WithItsNumbers()
        {
            const string p = "design.n53.rideKill53.";
            Assert.That(Cfg.Has(p + "_why"), Is.True, "rideKill53 has its _why");
            Assert.That(Cfg.Get(p + "sim") is bool s && s, Is.True, "a sim row (a tape bit)");
            Assert.That(Cfg.Get(p + "enabled") is bool, Is.True, "a bool switch");
            if (Cfg.Design("n53.rideKill53.enabled"))
                Assert.That(Cfg.Str(p + "enabled_why").StartsWith("ON"), Is.True, "turned on only at a freeze, with its reason");
            foreach (string k in new[] { "graceS", "charges" }) Assert.That(Cfg.Has(p + "numbers." + k + "_why"), Is.True, k + "_why");
            NightRows reg = NightRows.From(Cfg);
            Assert.That(reg.IndexOf("rideKill53") >= 0, Is.True, "the registry reads it");
            Assert.That(NightRows.Problems(Cfg.Obj("design")).Count, Is.EqualTo(0), "the design checker is clean");
        }

        [Test]
        public void TheOffSwitch53_AndOlder_TurnItOff()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows reg = NightRows.From(Cfg);
                foreach (int from in new[] { 53, 52, 47 })
                {
                    NightRows.ForceOffFrom = from;
                    Assert.That(reg.Defaults().On("rideKill53"), Is.False, "--off" + from);
                }
                NightRows.ForceOffFrom = NightRows.NoOff;
                Assert.That(reg.Defaults().Set("rideKill53", true).On("rideKill53"), Is.True, "a harness / reel forces it on");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void TheGrace_IsWholeSteps_FromTheConfig()
        {
            RideKill53Params r = RideKill53Params.From(Cfg, Dt);
            Assert.That(r.graceSteps, Is.EqualTo(MovementMath.StepsFor(Cfg.Num("design.n53.rideKill53.numbers.graceS"), Dt)));
            Assert.That(r.graceSteps, Is.EqualTo(48), "0.4 s at 120 Hz");
            Assert.That(r.charges, Is.EqualTo(1));
        }

        [Test]
        public void AKillOnTheWall_Counts_OnFootWithNoRide_DoesNot()
        {
            var c = new RideKill53Clock();
            c.Reset();
            Assert.That(c.Counts(false, 500, 48), Is.False, "on foot, no ride this run");
            Assert.That(c.Counts(true, 500, 48), Is.True, "on the wall at the kill");
            Assert.That(c.StepsSince(true, 500), Is.EqualTo(0));
            Assert.That(c.StepsSince(false, 500), Is.EqualTo(-1), "never rode");
        }

        [Test]
        public void TheGrace_CountsWholeSteps_AfterTheLastRidingStep()
        {
            var c = new RideKill53Clock();
            c.Reset();
            c.Rode(1000);
            Assert.That(c.Counts(false, 1000, 48), Is.True, "the step the ride ended");
            Assert.That(c.Counts(false, 1048, 48), Is.True, "48 whole steps after: inside");
            Assert.That(c.Counts(false, 1049, 48), Is.False, "49 steps after: outside");
            Assert.That(c.StepsSince(false, 1030), Is.EqualTo(30));
            Assert.That(c.Counts(false, 999, 48), Is.False, "a kill before the ride is not in its grace");
            c.Rode(990); // never back in time
            Assert.That(c.rideStep, Is.EqualTo(1000));
            c.Rode(1040); // a later ride moves the window
            Assert.That(c.Counts(false, 1088, 48), Is.True);
            Assert.That(c.Counts(false, 1089, 48), Is.False);
            Assert.That(c.Counts(false, 1001, 0), Is.False, "grace 0: only the riding step");
            c.Reset();
            Assert.That(c.Counts(false, 1041, 48), Is.False, "a run's start forgets the ride (rule 6)");
        }

        [Test]
        public void TheRefund_IsChargesTimesTheUnit_CappedAtTheMax()
        {
            double cost = Cfg.Num("design.stamina.boost.cost");
            Assert.That(RideKill53.Refund(0.25, cost, 1, 1.0), Is.EqualTo(0.25 + cost), "the bar: one boost back");
            Assert.That(RideKill53.Refund(0.8, cost, 1, 1.0), Is.EqualTo(1.0), "capped at the bar's max");
            Assert.That(RideKill53.Refund(1.0, cost, 1, 2.0), Is.EqualTo(1.0 + cost), "VIGOR's bigger bar");
            Assert.That(RideKill53.Refund(1.0, 1, 1, 3), Is.EqualTo(2.0), "the web's pips: one pip back");
            Assert.That(RideKill53.Refund(2.5, 1, 1, 3), Is.EqualTo(3.0), "capped at the pips' max");
        }
    }
}
