// NIGHT19 (the shared commit, the lead): the contract every builder compiles against.
//   1. the Player's design config is a byte-identical copy of Design/unity-config.json (none of the NIGHT19 blocks may
//      drift between the two: the Player plays what the tests read)
//   2. the sprint hook (Mover.StaminaStep): InputFrame.sprint wants the sprint only with design.stamina.n19.ownKey; with it
//      off the frame's bit changes nothing (NIGHT18's rule: the boost key's hold)
//   3. THE TAPE's format 4 carries the NIGHT19 byte and the sprint bit; a format 3 tape is read with every block off and
//      replays EQUAL (recorded on NIGHT18's game)
//   4. the composed fingerprint runs with every NIGHT19 block off until the close (FingerprintBaseline.N19OnPath), so it
//      prints NIGHT18's baseline whatever a run's blocks say
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N19SharedTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static SurvivalSim Sim(GameConfig cfg)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noRecords = sim.noLab = true;
            return sim;
        }

        [Test]
        public void ThePlayersDesignConfig_IsAByteIdenticalCopy()
        {
            string repo = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Design", "unity-config.json"));
            string player = Path.GetFullPath(Path.Combine(Application.dataPath, "StreamingAssets", "Design", "unity-config.json"));
            byte[] a = File.ReadAllBytes(repo), b = File.ReadAllBytes(player);
            Assert.That(b.Length, Is.EqualTo(a.Length), "the Player's copy: run SKYGRIND/Sync Port Data (or copy the file)");
            Assert.That(b, Is.EqualTo(a), "the Player's copy differs from Design/unity-config.json");
        }

        [Test]
        public void N19Blocks_Bits_RoundTrip_AndOffIsNight18()
        {
            for (int bits = 0; bits < 1 << N19Blocks.Count; bits++)
                Assert.That(N19Blocks.FromBits(bits).Bits, Is.EqualTo(bits));
            Assert.That(N19Blocks.Off.Any, Is.False);
            Assert.That(N19Blocks.Off.Bits, Is.EqualTo(0));
        }

        /// <summary>A run's mover on the floor, then `steps` steps of the stick forward with the sprint key held (no boost key).</summary>
        static Mover SprintKeyOnly(bool ownKey, int steps)
        {
            SurvivalSim sim = Sim(Cfg());
            sim.n19Override = N19Blocks.Off;
            sim.StartRun(1, null, null);
            Mover m = sim.mover;
            Assert.That(m.stamina.rules.Any && m.stamina.rules.sprint, Is.True, "design.stamina's bar and sprint are on by default");
            m.stamina.rules.ownKey = ownKey;
            double dt = 1.0 / sim.cfg.movement.loop.simHz;
            for (int i = 0; i < 60; i++) m.Step(new InputFrame { yaw = 0 }, dt); // settle on the floor
            for (int i = 0; i < steps; i++)
                m.Step(new InputFrame { moveZ = 1, yaw = 0, sprint = new Button { held = true, pressed = i == 0 } }, dt);
            return m;
        }

        [Test]
        public void TheSprintKey_WithoutOwnKey_ChangesNothing()
        {
            Mover m = SprintKeyOnly(false, 60);
            Assert.That(m.stamina.sprinting, Is.False, "NIGHT18's rule: only the boost key's hold sprints");
            Assert.That(m.stamina.bar, Is.EqualTo(m.StaminaMax));
        }

        [Test]
        public void TheSprintKey_WithOwnKey_SprintsWithoutABoost()
        {
            Mover m = SprintKeyOnly(true, 60);
            Assert.That(m.stamina.sprinting, Is.True, "the own key sprints on the ground while the stick moves");
            Assert.That(m.stamina.bar, Is.LessThan(m.StaminaMax), "the sprint drains");
            Assert.That(m.state, Is.Not.EqualTo(MoveState.Dash), "no boost fired");
        }

        [Test]
        public void TheTape_Format4_CarriesTheN19Byte_AndTheSprintBit()
        {
            var t = new RunTape { n19 = 0b1010101 };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT20: format 5 keeps format 4's byte
            Assert.That(RunTape.FromBytes(b).n19, Is.EqualTo(0b1010101));
            Assert.That(RunTape.FromBytes(AsFormat3(b)).n19, Is.EqualTo(0), "format 3: every NIGHT19 block off");
            var f = new InputFrame { sprint = new Button { held = true, pressed = true } };
            var s = new TapeStepState { moveButtons = TapeStepState.PackMove(f) };
            Assert.That(s.Frame().sprint.held && s.Frame().sprint.pressed, Is.True);
            Assert.That(TapeStepState.PackMove(default) & (3 << 10), Is.EqualTo(0));
        }

        [Test]
        public void AFormat3Tape_RecordedOnNight18sGame_ReplaysEqual()
        {
            SurvivalSim sim = Sim(Cfg());
            sim.n19Override = N19Blocks.Off; // as a NIGHT18 build recorded it
            sim.n20Override = N20Blocks.Off; // NIGHT20: and no NIGHT20 block (a format 3 tape reads them off)
            sim.n21Override = N21Blocks.Off; // NIGHT21: and no NIGHT21 block
            RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "tests", null, TapeScript.God);
            RunTape old = RunTape.FromBytes(AsFormat3(tape.ToBytes()));
            Assert.That(old.n19, Is.EqualTo(0));
            TapeResult r = TapeReplay.Replay(Cfg(), Map(), old);
            Assert.That(r.Equal, Is.True, r.Line);
        }

        [Test]
        public void TheFingerprint_RunsEveryN19BlockOff_UntilTheClose()
        {
            // NIGHT20's close re-recorded the on-baseline (FingerprintBaseline, N19OnPath true): the off path, both nights'
            // blocks off, still prints NIGHT18's lines (the Player's second [fingerprint] line).
            Assert.That(FingerprintBaseline.N19OnPath, Is.True, "NIGHT20's close re-recorded the on-baseline");
            SurvivalSim sim = Sim(Cfg());
            sim.n19Override = N19Blocks.Off;
            sim.n20Override = N20Blocks.Off;
            sim.n21Override = N21Blocks.Off; // NIGHT21: the on-baseline now has the NIGHT21 blocks as configured
            var lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            Assert.That(FingerprintBaselineN18.FirstDifference(lines), Is.EqualTo(-1), summary);
            Assert.That(sim.n19Override.Value.Bits, Is.EqualTo(0), "the override handed back");
        }

        /// <summary>A format 4 tape's bytes as a format 3 build wrote them: the version 3, no NIGHT19 byte (after the stamina flags).</summary>
        static byte[] AsFormat3(byte[] b4)
        {
            b4 = N24SharedTests.AsFormat7(b4); // NIGHT24: the current format is 8; the cut below counts format 7's bytes
            var r = new TapeReader(b4, RunTape.Magic.Length, b4.Length);
            r.U32(); // the version
            r.Str(); // the build stamp
            r.Str(); // the baseline id
            r.U32(); // the seed
            r.Str(); // the mode
            r.VarI(); // the HEAT rung
            r.U64(); // the clock
            r.Byte(); // the flags
            r.Byte(); // the LAB toggles
            r.Byte(); // the move flags (format 2)
            r.Byte(); // the stamina flags (format 3)
            int at = r.Position; // the NIGHT19 blocks (format 4)
            int cut = 4; // NIGHT24: the tape is format 7 here (AsFormat7 above): the NIGHT19, NIGHT20, NIGHT21 and NIGHT22 bytes (it was 3 at format 7, a stale premise); // NIGHT20: the NIGHT20 byte (format 5) follows the NIGHT19 byte; NIGHT21: and the NIGHT21 byte (format 6)
            var b3 = new byte[b4.Length - cut];
            Buffer.BlockCopy(b4, 0, b3, 0, at);
            Buffer.BlockCopy(b4, at + cut, b3, at, b4.Length - at - cut);
            var w = new TapeWriter(8);
            w.U32(3);
            Buffer.BlockCopy(w.data, 0, b3, RunTape.Magic.Length, 4);
            return b3;
        }
    }
}
