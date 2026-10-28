// NIGHT16 (builder M, the NIGHT14 residual): the Player smoke's steps 18 / 21 failed now and then ("the first run differs
// from the baseline at checkpoint 6"): a composition that had played runs carried state StartRun's clean slate didn't
// reset. Found headless (SimHarness `residual check`): the aim assist's scan clock (a scan when its step count is even,
// aim.lock.scanEvery 2: an odd number of steps played before moved every scan of the next run by one step) and the
// director's per-step accumulators (a hurt or a nearby kill after its last Step of a run, the killing blow's HP, added to the
// next run's intensity). Each played history here must leave the seed-1 fingerprint run equal to the baseline.
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class ResidualTests
    {
        static void AssertBaseline(SurvivalSim sim, string after)
        {
            List<string> lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            int diff = FingerprintBaseline.FirstDifference(lines);
            Assert.That(diff, Is.EqualTo(-1), $"after {after}: first difference at checkpoint {diff + 1}; {summary}");
        }

        /// <summary>The attract (the AUTOPILOT, god mode, metaSilent) for an ODD number of steps, stopped by ToTitle (AttractStop).</summary>
        [Test]
        public void AnAttractOfAnOddStepCount_TheNextRunIsTheBaseline()
        {
            SurvivalSim sim = FingerprintTests.Headless();
            IPlayerInput before = sim.input;
            bool silent = sim.metaSilent, noLab = sim.noLab;
            var pilot = new AutopilotInput(sim, "normal", 1);
            sim.metaSilent = sim.noLab = true;
            sim.UseInput(pilot);
            sim.StartRun(1, null, null);
            sim.health.max = sim.health.hp = 1e9;
            for (int s = 0; s < 2401 && sim.flow != Flow.Results; s++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
            }
            sim.UseInput(before);
            sim.ToTitle();
            pilot.Detach();
            sim.metaSilent = silent;
            sim.noLab = noLab;
            AssertBaseline(sim, "a 2401-step attract");
        }

        /// <summary>A run that ends in a death (the killing blow's HP is a hurt after the director's last Step), to RESULTS.</summary>
        [Test]
        public void ADeathToResults_TheNextRunIsTheBaseline()
        {
            SurvivalSim sim = FingerprintTests.Headless();
            sim.noLab = sim.noRecords = true;
            sim.StartRun(5, null, null);
            for (int s = 0; s < 2400 && sim.flow != Flow.Results; s++)
            {
                if (s == 10) sim.health.hp = 1;
                sim.Step();
            }
            Assert.That(sim.flow, Is.EqualTo(Flow.Results), "the run ended in a death");
            sim.noLab = sim.noRecords = false;
            AssertBaseline(sim, "a death");
        }
    }
}
