// NIGHT13 U3-26.10 (agent E): the run snapshot's composed proof on SurvivalSim (U3-26.6). A seeded run on the
// fingerprint's input script (SimFingerprint.Script through a ScriptedInput, the player at 1e6 HP, no LAB, no records,
// a fresh profile, OnScreen false: FingerprintTests.Headless), snapshotted at a round break, restored into a fresh
// headless composition (StartRun on the same seed builds the run's structure, then RunSnapshot.Load), and both stepped on
// the same script: the composed fingerprint (SimFingerprint.Hash) is equal at every checkpoint after, and so is a second
// snapshot. Three seeds, three breaks: round 2's; the break before a HOLD round (round 6, objectives.js's first HOLD:
// the continuation plays the HOLD); and a later one with swarmers in the pools (round 5's). The break before an exfil
// (round 11) can't be reached by a script: rounds 6 and 10 are HOLD rounds, which clear only after 30 s in the ring.
// Systems(sim) is SurvivalSim.SnapshotSections, the list SAVE & QUIT and RESUME use.
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class RunRestoreTests
    {
        /// <summary>The run snapshot's sections: the game's own list (SurvivalSim.SnapshotSections, SurvivalSim.Save.cs; NIGHT13:
        /// moved there from this test so SAVE & QUIT and RESUME use what this proves).</summary>
        public static List<(string name, IRunState state)> Systems(SurvivalSim sim) => sim.SnapshotSections();

        /// <summary>A headless composition playing the fingerprint's script from StartRun (SurvivalSim.RunFingerprint's setup).</summary>
        sealed class Scripted
        {
            public SurvivalSim sim;
            public ScriptedInput script;
            public int i; // the script's step

            public static Scripted Start(uint seed)
            {
                SurvivalSim sim = FingerprintTests.Headless();
                var script = new ScriptedInput();
                script.SetLook(sim.input.Yaw, sim.input.Pitch);
                sim.input = script;
                sim.noRecords = sim.noLab = true;
                sim.setup.mode = RunSetup.Survival;
                sim.setup.heat = 0;
                sim.setup.seed = null;
                sim.StartRun(seed, null, null);
                sim.health.max = sim.health.hp = 1e6;
                return new Scripted { sim = sim, script = script };
            }

            public void Step()
            {
                double yaw = script.Yaw, pitch = script.Pitch;
                SimFingerprint.Script(i, sim.Eye, sim.enemies, ref yaw, ref pitch, out InputFrame move, out CombatInput combat);
                script.SetLook(yaw, pitch);
                script.Set(move, combat);
                sim.Step();
                i++;
            }

            public bool AtBreakOf(int round) => sim.run.round == round && sim.run.state == SurvivalRun.State.Break;

            public string Save() => RunSnapshot.Save(Systems(sim));
        }

        /// <summary>
        /// Play `seed` to round `round`'s break, `into` steps into it; snapshot; restore into a fresh composition; step both
        /// `after` more steps, comparing the fingerprint every 600 steps and at the end.
        /// </summary>
        static void SnapshotRestoreContinue(uint seed, int round, int into, int after)
        {
            Scripted a = Scripted.Start(seed);
            for (int k = 0; k < 900 * 120 && !a.AtBreakOf(round); k++) a.Step();
            Assert.That(a.AtBreakOf(round), Is.True, $"seed {seed}: the script reaches round {round}'s break (it is in round {a.sim.run.round}, {a.sim.run.state})");
            for (int k = 0; k < into && a.AtBreakOf(round); k++) a.Step();
            Assert.That(a.AtBreakOf(round), Is.True, $"seed {seed}: still in round {round}'s break {into} steps in");
            int n = a.i;
            string json = a.Save();
            Scripted b = Scripted.Start(seed);
            RunSnapshot.Load(json, Systems(b.sim));
            b.i = a.i;
            Assert.That(b.Save(), Is.EqualTo(json), "the restored composition snapshots to the same bytes");
            Assert.That(SimFingerprint.Hash(b.sim), Is.EqualTo(SimFingerprint.Hash(a.sim)), "the same fingerprint at the snapshot");
            for (int k = 1; k <= after; k++)
            {
                a.Step();
                b.Step();
                if (k % 600 == 0 || k == after)
                {
                    ulong ha = SimFingerprint.Hash(a.sim), hb = SimFingerprint.Hash(b.sim);
                    Assert.That(hb, Is.EqualTo(ha), $"seed {seed}: the fingerprint differs at step {n + k} (snapshot at {n}, round {a.sim.run.round} {a.sim.run.state})");
                }
            }
            TestContext.WriteLine($"seed {seed}: snapshot at step {n} (round {round}'s break), equal through step {n + after}: round {a.sim.run.round} {a.sim.run.state}, kills {a.sim.run.stats.kills}, alive {a.sim.enemies.AliveCount}, objective {a.sim.objectives.Type}, {json.Length} chars");
        }

        /// <summary>
        /// NIGHT14 (the Player smoke, build 2: once SAVE & QUIT worked, the later seed-1 fingerprint runs on that composition
        /// differed from the baseline at checkpoint 6, two of them equal to each other): a run resumed from a save and left,
        /// then a new run on the same composition is the fresh composition's. The message names every snapshot section whose
        /// state right after the new run's StartRun differs from a fresh composition's (the carry-over).
        /// </summary>
        [Test]
        public void AfterAResume_TheNextRunIsAFreshCompositions()
        {
            Scripted a = Scripted.Start(727272);
            for (int k = 0; k < 900 * 120 && !a.AtBreakOf(2); k++) a.Step();
            for (int k = 0; k < 30 && a.AtBreakOf(2); k++) a.Step();
            Assert.That(a.AtBreakOf(2), Is.True, "the script reaches round 2's break");
            string json = a.Save();
            Scripted b = Scripted.Start(727272);
            RunSnapshot.Load(json, Systems(b.sim));
            for (int k = 0; k < 600; k++) b.Step();
            b.sim.ToTitle();
            Scripted fresh = Scripted.Start(SimFingerprint.Seed);
            b.sim.setup.mode = RunSetup.Survival;
            b.sim.setup.heat = 0;
            b.sim.StartRun(SimFingerprint.Seed, null, null);
            b.sim.health.max = b.sim.health.hp = 1e6;
            var differs = new List<string>();
            List<(string name, IRunState state)> fs = Systems(fresh.sim), bs = Systems(b.sim);
            for (int s = 0; s < fs.Count; s++)
            {
                string x, y;
                try { x = RunSnapshot.Save(new List<(string, IRunState)> { fs[s] }); } catch (Exception e) { x = "throws " + e.Message; }
                try { y = RunSnapshot.Save(new List<(string, IRunState)> { bs[s] }); } catch (Exception e) { y = "throws " + e.Message; }
                if (x != y) differs.Add(fs[s].name);
            }
            b.sim.ToTitle();
            List<string> lines = b.sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            int diff = FingerprintBaseline.FirstDifference(lines);
            Assert.That(diff, Is.EqualTo(-1), $"first difference at checkpoint {diff + 1}; sections that differ from a fresh composition's after StartRun: [{string.Join(", ", differs)}]; {summary}");
        }

        [Test]
        public void Restore_AtRound2Break_EqualsTheUninterruptedRun()
        {
            SnapshotRestoreContinue(11, 2, 30, 3000);
        }

        [Test]
        public void Restore_AtTheBreakBeforeAHoldRound_EqualsTheUninterruptedRun()
        {
            SnapshotRestoreContinue(23, 6, 30, 4800);
        }

        [Test]
        public void Restore_AtALaterBreak_EqualsTheUninterruptedRun()
        {
            // NIGHT16: with design.move's slide rules on (the default) the script reaches round 5's break at step 9362 (not
            // 10814) and READY starts the round after survival.readyHold's 72 steps (the flags off: a 957-step break), so the
            // snapshot is taken 30 steps in (it was 240); the continuation is the same 3600 steps.
            SnapshotRestoreContinue(37, 5, 30, 3600);
        }

    }
}
