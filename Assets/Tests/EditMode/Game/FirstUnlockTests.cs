// NIGHT20 E1 (builder P; Research/2026-10-06-additions-backlog.md E1's done-when: "the first unlock <= 5 min in >= 90 % of
// 20 new-profile autopilot seeds"; Vampire Survivors' first unlock at minute 1, Research/2026-10-03-retention.md P3): a new
// profile's first run reaches its first unlock (design.contracts.first: FIRST FIFTEEN, 15 kills, granting the DAILY)
// inside 5 minutes of sim time. Measured with the AUTOPILOT playing a recording run (metaSilent cleared for the
// measurement: a bot's run normally records nothing) on a fresh profile, 20 seeds; the time is the sim's at the
// unlockEarned event.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class FirstUnlockTests
    {
        static string PortRoot()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        static GameConfig cfg;
        static SurvivalMap map;
        static GameConfig Cfg() => cfg ??= GameConfig.Load(Path.Combine(PortRoot(), "config.json"));
        static SurvivalMap Map() => map ??= SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(PortRoot(), "maps", "survival.json"))));

        /// <summary>The sim seconds to the first unlock on a fresh profile (infinity: none within `cap` s, or the run ended).</summary>
        static double FirstUnlock(uint seed, double cap)
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.TellTheTruth();
            sim.UseInput(ap);
            sim.metaSilent = false; // the measurement: this run records, as a new player's would
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            double at = double.PositiveInfinity;
            int step = 0;
            sim.events.On(MetaEvents.UnlockEarned, _ =>
            {
                if (double.IsPositiveInfinity(at)) at = step * sim.step;
            });
            sim.StartRun(seed, null, null);
            Assert.That(sim.Recording, Is.True, "a recording run");
            int max = MovementMath.StepsFor(cap, sim.step);
            for (; step < max && double.IsPositiveInfinity(at) && sim.flow != Flow.Results; step++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
            }
            ap.Detach();
            return at;
        }

        [Test]
        public void TwentyNewProfiles_TheFirstUnlockLandsInsideFiveMinutes_AtLeast90Percent()
        {
            const double five = 300;
            var times = new List<double>();
            int inside = 0;
            for (uint seed = 1; seed <= 20; seed++)
            {
                double t = FirstUnlock(seed, five);
                times.Add(t);
                if (t <= five) inside++;
            }
            times.Sort();
            string all = string.Join(", ", times.ConvertAll(t => double.IsPositiveInfinity(t) ? "none" : t.ToString("0.0")));
            Assert.That(inside, Is.GreaterThanOrEqualTo(18), $"{inside} of 20 inside 5 minutes: {all}");
            Assert.Pass($"{inside} of 20 inside 5 minutes: {all}");
        }
    }
}
