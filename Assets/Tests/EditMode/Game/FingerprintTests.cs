// NIGHT13 U3-26.6: the composed simulate() order pinned by a test (audit-perf section 5 moves 1 and 6). SurvivalSim is
// built headless from PortExport (no scene, no views, no camera: OnScreen pinned false, the settings at their defaults, a
// fresh profile), taken to TITLE as the Player's boot does (with the perk machines' buy points on the flat core, as
// PerkMachines.Render puts them on TITLE's frames), and plays the fingerprint run: its twelve hashes must equal the
// baseline the Player recorded on the unchanged u3-n12 composition (FingerprintBaseline, build 1).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class FingerprintTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        /// <summary>A headless composition: the Player's inputs pinned the way SurvivalGame.RunFingerprint pins them.</summary>
        public static SurvivalSim Headless()
        {
            GameConfig cfg = GameConfig.Load(Path.Combine(Root, "config.json"));
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));
            var store = new SettingsStore(cfg, new List<(SettingItem, object)>(), new List<(string, string)>());
            store.Load("");
            var sim = new SurvivalSim(cfg, map, new EventBus(), new Lab(cfg), new ScriptedInput(), new Profile(), store.Num, store.Bool, store.Str, _ => false, a => a);
            sim.ToTitle();
            foreach (SurvivalSim.Machine m in sim.machines) sim.RideCore(m);
            return sim;
        }

        [Test]
        public void HeadlessComposition_EqualsTheBaseline()
        {
            SurvivalSim sim = Headless();
            List<string> lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            int diff = FingerprintBaseline.FirstDifference(lines);
            Assert.That(diff, Is.EqualTo(-1), $"first difference at checkpoint {diff + 1}; {summary}\n" + string.Join("\n", lines));
        }

        /// <summary>
        /// NIGHT14 U3-27.2 (a clean slate per run: RETRY SAME SEED, the attract, the farm): two seed-1 runs back to back on ONE
        /// composition print the same 12 hashes (HANDOFF section 3: the second differed from step 600 on: the enemy pools'
        /// reused dead and their ids, the shot ids, the director's pre-round streams carried over).
        /// </summary>
        [Test]
        public void TwoRunsOnOneComposition_PrintTheSameHashes()
        {
            SurvivalSim sim = Headless();
            List<string> first = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string a);
            List<string> second = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string b);
            Assert.That(FingerprintBaseline.FirstDifference(first), Is.EqualTo(-1), "the first run is the baseline's");
            Assert.That(second, Is.EqualTo(first), $"the second run on the same composition: {b} (first: {a})");
        }

        /// <summary>
        /// NIGHT14 (the Player smoke's step 18, build 1: `the first run differs from the baseline at checkpoint 6`, right after
        /// the virtual pad step): the fingerprint run is the scripted run on the mouse's aim assist whatever device played
        /// last. The pad's SNAP MODE and breakaway cap (PadReader.AimMode / BreakawayCap: gamepad.yawRate 220 x padSensX 1 x
        /// breakawayShare 0.8 = 176 deg/s) are set aside for the run and handed back after it.
        /// </summary>
        /// <summary>
        /// NIGHT15 U3-28.7: the Unity defaults put the mouse's SNAP MODE at OFF (design.aimAssist), but the fingerprint run is
        /// the web's: RunFingerprint pins the web's default ('snap', config settings.aimAssist) for its run, whatever the
        /// player's setting, and hands the hook back after.
        /// </summary>
        [Test]
        public void TheMouseAssistOff_TheFingerprintIsStillTheBaseline()
        {
            GameConfig cfg = GameConfig.Load(Path.Combine(Root, "config.json"));
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));
            var store = new SettingsStore(cfg, new List<(SettingItem, object)>(), new List<(string, string)>());
            store.Load("");
            store.Set("aimAssist", "off");
            var sim = new SurvivalSim(cfg, map, new EventBus(), new Lab(cfg), new ScriptedInput(), new Profile(), store.Num, store.Bool, store.Str, _ => false, a => a);
            sim.ToTitle();
            foreach (SurvivalSim.Machine m in sim.machines) sim.RideCore(m);
            List<string> lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            int diff = FingerprintBaseline.FirstDifference(lines);
            Assert.That(diff, Is.EqualTo(-1), $"first difference at checkpoint {diff + 1}; {summary}");
            Assert.That(sim.assistModeFor, Is.Null, "the hook is handed back (none was set)");
            Assert.That(sim.AssistMode, Is.EqualTo("off"), "the player's setting is untouched");
        }

        [Test]
        public void ThePadPlayedLast_TheFingerprintIsTheBaseline()
        {
            SurvivalSim sim = Headless();
            System.Func<string, string> padMode = mouse => "snap";
            System.Func<double> padCap = () => 176;
            sim.assistModeFor = padMode;
            sim.breakawayCap = padCap;
            List<string> lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            int diff = FingerprintBaseline.FirstDifference(lines);
            Assert.That(diff, Is.EqualTo(-1), $"first difference at checkpoint {diff + 1}; {summary}");
            Assert.That(sim.assistModeFor, Is.SameAs(padMode), "the pad's mode is handed back");
            Assert.That(sim.breakawayCap, Is.SameAs(padCap), "the pad's cap is handed back");
        }

        /// <summary>
        /// NIGHT14 (the Player smoke's save step, build 1: `[save] not saved: snapshot: saving 'husks': snapshot: a reference to
        /// an enemy (spitter 8) outside the registry`): the clean slate empties the registry, so the acid's dead glob and puddle
        /// slots drop their shooters and targets with it (a fresh composition's slots hold none); before, the first save at a
        /// round break after a run with spitters looked the last run's spitter up and refused.
        /// </summary>
        [Test]
        public void ANewRun_TheAcidHoldsNoEnemyFromTheLastRun()
        {
            SurvivalSim sim = Headless();
            Husk spitter = sim.husks.Spawn("spitter", sim.map.playerSpawn, sim.map.playerSpawn);
            Assert.That(spitter, Is.Not.Null, "the last run's spitter");
            SpitterAcid acid = sim.husks.acid;
            acid.globs[0].shooter = spitter;
            acid.globs[0].target = spitter;
            acid.puddles[0].shooter = spitter;
            sim.noRecords = sim.noLab = true; // automation's rules (as RunFingerprint sets them)
            sim.StartRun(5u, null, null);
            Assert.That(acid.globs[0].shooter, Is.Null, "the glob's shooter");
            Assert.That(acid.globs[0].target, Is.Null, "the glob's target");
            Assert.That(acid.puddles[0].shooter, Is.Null, "the puddle's shooter");
            Assert.That(() => sim.husks.Save(new StateWriter()), Throws.Nothing, "the husks section saves");
        }

        [Test]
        public void AnotherSeed_PrintsOtherHashes()
        {
            SurvivalSim sim = Headless();
            List<string> lines = sim.RunFingerprint(SimFingerprint.RedSeed, SimFingerprint.Steps, SimFingerprint.Every, out _);
            Assert.That(FingerprintBaseline.FirstDifference(lines), Is.EqualTo(0));
        }
    }
}
