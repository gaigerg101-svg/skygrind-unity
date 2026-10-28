// NIGHT17 U3-30.4 (builder M owns this file): design.stamina in the sim and on THE TAPE.
//   1. a run reads design.stamina's flags at its start (the bar full); a tape's override sets them (all off: the web's pips)
//   2. the bus's kill (the player's) and parry refill the bar; the mover's stamina events reach the bus for GG's sounds
//   3. THE TAPE's format 3 carries the stamina flags byte (round trip); a format 2 tape is read with every flag off and
//      replays EQUAL (recorded on the web's pips); a format 3 tape with the flags on replays EQUAL
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class StaminaTapeTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static readonly bool[] AllOff = new bool[Stamina.FlagCount];

        /// <summary>A headless composition run as automation runs (noLab: the design defaults; noRecords: nothing stored).</summary>
        static SurvivalSim Sim(GameConfig cfg)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noRecords = sim.noLab = true;
            return sim;
        }

        [Test]
        public void ARun_ReadsDesignStamina_AndATapesOverride()
        {
            GameConfig cfg = Cfg();
            SurvivalSim sim = Sim(cfg);
            sim.StartRun(1, null, null);
            Stamina.Params d = Stamina.Params.From(cfg);
            Assert.That(d.Any, Is.True, "design.stamina.bar is on by default");
            for (int i = 0; i < Stamina.FlagCount; i++) Assert.That(sim.StaminaFlag(i), Is.EqualTo(d.Flag(i)), "flag " + i);
            Assert.That(sim.mover.stamina.bar, Is.EqualTo(sim.mover.StaminaMax));
            Assert.That(sim.mover.StaminaMax, Is.EqualTo(cfg.Num("design.stamina.bar.max")));
            SurvivalSim web = Sim(cfg);
            web.staminaOverride = (bool[])AllOff.Clone();
            web.StartRun(1, null, null);
            Assert.That(web.mover.stamina.rules.Any, Is.False, "a tape's flags off: the web's pips");
        }

        [Test]
        public void KillAndParry_OnTheBus_RefillTheBar()
        {
            GameConfig cfg = Cfg();
            SurvivalSim sim = Sim(cfg);
            sim.StartRun(1, null, null);
            Mover m = sim.mover;
            m.stamina.bar = 0.2;
            // NIGHT20: the kill at the player's eye; with design.stamina.n19 on (NIGHT19's range reward) a kill within
            // nearRange pays killNear, with it off NIGHT18's flat rewards.kill
            double pay = sim.n19.stamina ? cfg.Num("design.stamina.n19.killNear") : cfg.Num("design.stamina.rewards.kill");
            sim.events.Emit("kill", new KillEvent { owner = "player", weapon = "revolver", kind = "husk", position = sim.Eye });
            Assert.That(m.stamina.bar, Is.EqualTo(0.2 + pay).Within(1e-12));
            sim.events.Emit("kill", new KillEvent { owner = "rival", weapon = "revolver", kind = "husk", position = sim.Eye });
            Assert.That(m.stamina.bar, Is.EqualTo(0.2 + pay).Within(1e-12), "only the player's kills");
            sim.events.Emit("parry", new HitEvent { owner = "player", weapon = "punch", kind = "husk" });
            Assert.That(m.stamina.bar, Is.EqualTo(m.StaminaMax), "a parry: the whole bar");
        }

        [Test]
        public void TheMoversStaminaEvents_ReachTheBus()
        {
            SurvivalSim sim = Sim(Cfg());
            sim.StartRun(1, null, null);
            var seen = new List<string>();
            int partial = -1;
            foreach (string t in new[] { Stamina.FullEvent, Stamina.SprintStartEvent, Stamina.SprintEndEvent })
            {
                string type = t;
                sim.events.On(type, p =>
                {
                    Assert.That(p, Is.EqualTo("player"));
                    seen.Add(type);
                });
            }
            sim.events.On(Stamina.BoostPartialEvent, p => partial = (int)p);
            Mover m = sim.mover;
            double dt = 1 / sim.cfg.movement.loop.simHz;
            for (int i = 0; i < 60; i++) m.Step(new InputFrame { moveZ = 1 }, dt);
            m.Step(new InputFrame { moveZ = 1, dash = new Button { held = true, pressed = true } }, dt);
            for (int i = 0; i < 60; i++) m.Step(new InputFrame { moveZ = 1, dash = new Button { held = true } }, dt);
            m.Step(new InputFrame { moveZ = 1 }, dt); // key up: the sprint ends
            for (int i = 0; i < 400; i++) m.Step(new InputFrame { moveZ = 1 }, dt); // the refill to full
            Assert.That(seen, Is.EqualTo(new List<string> { Stamina.SprintStartEvent, Stamina.SprintEndEvent, Stamina.FullEvent }));
            m.stamina.bar = 0.25;
            m.stamina.sinceSpend = 0;
            m.Step(new InputFrame { moveZ = 1, dash = new Button { held = true, pressed = true } }, dt);
            Assert.That(partial, Is.EqualTo(50), "boostPartial: the percent of a full boost");
        }

        [Test]
        public void Format3_CarriesTheStaminaFlagsByte()
        {
            var t = new RunTape { stamina = new[] { true, false, true, true, false } };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion));
            Assert.That(RunTape.FormatVersion, Is.GreaterThanOrEqualTo(3)); // NIGHT19: format 4 adds the NIGHT19 byte after this one
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.stamina, Is.EqualTo(t.stamina));
            Assert.That(RunTape.FromBytes(AsFormat2(b)).stamina, Is.EqualTo(AllOff), "format 2: every stamina flag off");
        }

        [Test]
        public void AFormat2Tape_RecordedOnTheWebsPips_ReplaysEqual()
        {
            SurvivalSim sim = Sim(Cfg());
            sim.staminaOverride = (bool[])AllOff.Clone(); // as a NIGHT16 build recorded it: the web's pips
            sim.n19Override = N19Blocks.Off; // NIGHT20: and no NIGHT19 / NIGHT20 block (a format 2 tape reads them off)
            sim.n20Override = N20Blocks.Off;
            RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "tests", null, TapeScript.God);
            RunTape old = RunTape.FromBytes(AsFormat2(tape.ToBytes()));
            Assert.That(old.stamina, Is.EqualTo(AllOff));
            TapeResult r = TapeReplay.Replay(Cfg(), Map(), old);
            Assert.That(r.Equal, Is.True, r.Line);
        }

        [Test]
        public void AFormat3Tape_WithTheStaminaFlagsOn_ReplaysEqual()
        {
            SurvivalSim sim = Sim(Cfg());
            RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "tests", null, TapeScript.God);
            Assert.That(tape.stamina[0], Is.True, "recorded with design.stamina's defaults");
            TapeResult r = TapeReplay.Replay(Cfg(), Map(), RunTape.FromBytes(tape.ToBytes()));
            Assert.That(r.Equal, Is.True, r.Line);
        }

        /// <summary>A format 3+ tape's bytes as a format 2 build wrote them: the version 2, no stamina byte (after the move flags)
        /// and (NIGHT19, format 4) no NIGHT19 byte after it.</summary>
        static byte[] AsFormat2(byte[] b3)
        {
            b3 = N24SharedTests.AsFormat7(b3); // NIGHT24: the current format is 8; the cut below counts format 7's bytes
            var r = new TapeReader(b3, RunTape.Magic.Length, b3.Length);
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
            int at = r.Position; // the stamina flags (format 3), then the NIGHT19 blocks (format 4)
            int cut = RunTape.FormatVersion >= 7 ? 5 : RunTape.FormatVersion >= 6 ? 4 : RunTape.FormatVersion >= 5 ? 3 : RunTape.FormatVersion >= 4 ? 2 : 1; // NIGHT20: and the NIGHT20 byte (format 5); NIGHT21: and the NIGHT21 byte (format 6); NIGHT22: and the NIGHT22 byte (format 7, a premise change: the format number moved)
            var b2 = new byte[b3.Length - cut];
            Buffer.BlockCopy(b3, 0, b2, 0, at);
            Buffer.BlockCopy(b3, at + cut, b2, at, b3.Length - at - cut);
            var w = new TapeWriter(8);
            w.U32(2);
            Buffer.BlockCopy(w.data, 0, b2, RunTape.Magic.Length, 4);
            return b2;
        }
    }
}
