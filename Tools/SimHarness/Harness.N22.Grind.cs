// NIGHT22 U3-36.6 (builder W): `grind2 [--selftest] [--seed n] [--tag t]` - THE GRIND wired: the contract `grind`'s headless lines.
//   --selftest   Grind22Tests' own assertions outside Unity (Tools/SimHarness/NunitMini.cs)
//   (default)    1. the telegraph: an AUTOPILOT GRIND run; every break's last design.n21.grind.telegraphTime s mark the pillars the next
//                   shape lifts, the reshape wears exactly the marked shape, the fight has no marks
//                2. the WAVE banner is raised and no ROUND banner reaches the GRIND (the sim's 'banner' events; the Ui's own ROUND
//                   banner is suppressed for the GRIND in Game/Ui.cs, checked on the Player)
//                3. the records survive a restart: a run ends with a best, a NEW composition reads the file
//                4. a GRIND tape: mode "grind" round trips format 7 and the replay is EQUAL (and differs from the same script as SURVIVAL)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void Grind2Command(string[] args, ref int code)
    {
        if (Array.IndexOf(args, "--selftest") >= 0)
        {
            code = NUnit.Framework.MiniRunner.Run(typeof(Skygrind.Core.Tests.Grind22Tests), Console.WriteLine) == 0 ? 0 : 1;
            return;
        }
        uint seed = 1;
        for (int i = 1; i < args.Length; i++)
            if (args[i] == "--seed" && i + 1 < args.Length) seed = uint.Parse(args[++i], CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        Farm.BuildStamp = "harness";
        bool ok = true;

        // ---- 1 + 2: the telegraph and the banners over a GRIND run to wave 4 ----
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.Beacon20Ensure();
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.TellTheTruth();
            ap.ExfilFrom = 0;
            sim.UseInput(ap);
            sim.noRecords = sim.noLab = true;
            sim.n21Override = new N21Blocks { grind = true };
            sim.setup.mode = RunSetup.Grind;
            sim.setup.heat = 0;
            int roundBanners = 0, waveBanners = 0;
            sim.events.On<BannerEvent>("banner", b =>
            {
                if (b.text != null && b.text.StartsWith("ROUND", StringComparison.Ordinal)) roundBanners++;
                else if (b.text != null && b.text.StartsWith("WAVE", StringComparison.Ordinal)) waveBanners++;
            });
            sim.StartRun(seed, null, null);
            double tele = cfg.Num("design.n21.grind.telegraphTime");
            int teleSteps = MovementMath.StepsFor(tele, sim.step);
            int maxSteps = MovementMath.StepsFor(300, sim.step);
            var waveRows = new List<string>();
            int wavesChecked = 0, badOn = 0, badShape = 0, fightOn = 0, noMarks = 0, onSteps = 0, firstOnLeft = -1;
            int lastWave = 0, markedForWave = -1, risingNow = 0;
            double[] marked = null;
            string markedPreset = null;
            bool wasOn = false;
            for (int s = 0; s < maxSteps && sim.flow == Skygrind.Game.Flow.Playing && (sim.grind == null || sim.grind.CurrentWave <= 4); s++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (sim.grind == null) continue;
                var ph = sim.grind.Phase;
                bool on = sim.GrindTelegraphing;
                if (on) onSteps++;
                if (ph == Endless21.State.Fight && on) fightOn++;
                if (on && !wasOn && ph == Endless21.State.Break)
                {
                    firstOnLeft = sim.grind.PhaseSteps;
                    if (Math.Abs(firstOnLeft - teleSteps) > 1) badOn++;
                    bool[] r = sim.GrindRising;
                    risingNow = 0;
                    if (r != null) foreach (bool b in r) if (b) risingNow++;
                    if (risingNow == 0) noMarks++;
                    marked = (double[])sim.GrindNextHeights.Clone();
                    markedPreset = sim.GrindNextPreset;
                    markedForWave = sim.grind.CurrentWave;
                }
                if (wasOn && ph == Endless21.State.Fight && marked != null) // the reshape just ended: the core wears what was marked
                {
                    bool same = true;
                    for (int i = 0; i < marked.Length; i++) if (sim.core.Height(i) != (double)(float)marked[i]) same = false;
                    if (!same) badShape++;
                    wavesChecked++;
                    waveRows.Add($"wave {markedForWave}: {markedPreset}, {risingNow} of {marked.Length} pillars marked, on {firstOnLeft} steps before the reshape, core {(same ? "wears the marked heights" : "DIFFERS")}");
                    marked = null;
                }
                wasOn = on;
                lastWave = sim.grind.CurrentWave;
            }
            foreach (string row in waveRows) Console.WriteLine("  " + row);
            bool tele1 = wavesChecked >= 3 && badOn == 0 && badShape == 0 && fightOn == 0 && noMarks == 0;
            Console.WriteLine($"telegraph (rising edges, DANGER colour in the view): {wavesChecked} reshapes checked, on {tele.ToString("0.#", CultureInfo.InvariantCulture)} s ({teleSteps} steps) before each (off by more than 1 step: {badOn}), no marks in a fight ({fightOn} steps), the core wore the marked heights ({badShape} differ), a wave with nothing marked: {noMarks}: {(tele1 ? "PASS" : "FAIL")}");
            bool ban = roundBanners == 0 && waveBanners >= 3;
            Console.WriteLine($"banners over {lastWave} waves: ROUND banners from the sim {roundBanners} (want 0), WAVE banners {waveBanners}: {(ban ? "PASS" : "FAIL")}");
            ok &= tele1 && ban;
            ap.Detach();
        }

        // ---- 3: the records survive a restart ----
        {
            string path = Path.Combine(Path.GetTempPath(), "skygrind-grind2-" + Guid.NewGuid().ToString("N"), "grind-records.txt");
            try
            {
                SurvivalSim a = SurvivalSim.Headless(cfg, map);
                a.n21Override = new N21Blocks { grind = true };
                a.setup.mode = RunSetup.Grind;
                a.noLab = true;
                a.metaSilent = false;
                a.GrindRecordsFile(path); // the path the Game gives (persistentDataPath/grind-records.txt)
                a.noRecords = false; // a player's run (no autopilot: a bot's run is metaSilent and keeps nothing)
                a.StartRun(seed, null, null);
                bool prevHeld = a.noRecords; // the GRIND holds SURVIVAL's records back (noRecords) and uses its own
                for (int s = 0; s < MovementMath.StepsFor(400, a.step) && a.flow == Skygrind.Game.Flow.Playing; s++)
                {
                    a.Step();
                    if (s * a.step >= 60 && a.flow == Skygrind.Game.Flow.Playing) a.health.Damage(a.health.max * 10, "harness");
                }
                int waveA = a.lastGrindStats != null ? a.lastGrindStats.lastWave : 0;
                bool fileThere = File.Exists(path);
                SurvivalSim b = SurvivalSim.Headless(cfg, map); // the restart: a new composition, nothing carried over
                int before = b.grindRecords.BestWave(0);
                b.GrindRecordsFile(path);
                int waveB = b.grindRecords.BestWave(0);
                bool p3 = waveA > 0 && fileThere && before == 0 && waveB == waveA;
                Console.WriteLine($"records: a GRIND run ended at wave {waveA} ({a.lastGrindJudgement?.text}); file written {fileThere}; a new composition read {before} before the file, best wave {waveB} after it: {(p3 ? "PASS" : "FAIL")}");
                ok &= p3;
            }
            finally
            {
                string dir = Path.GetDirectoryName(path);
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        // ---- 4: a GRIND tape round trips its mode and replays EQUAL ----
        {
            RunTape tape = GrindTape(cfg, map, RunSetup.Grind, seed, 1800);
            byte[] bytes = tape.ToBytes();
            int version = BitConverter.ToInt32(bytes, RunTape.Magic.Length);
            RunTape back = RunTape.FromBytes(bytes);
            TapeResult rep = TapeReplay.Replay(cfg, map, back);
            RunTape surv = GrindTape(cfg, map, RunSetup.Survival, seed, 1800);
            bool differs = LastHash(surv) != LastHash(tape);
            bool p4 = version == RunTape.FormatVersion && back.mode == RunSetup.Grind && rep.Equal && differs;
            Console.WriteLine($"GRIND tape: format {version}, mode '{back.mode}' read back (want '{RunSetup.Grind}'), {bytes.Length} bytes, replay {(rep.Equal ? "EQUAL" : "DIFFERS")}; the same script as SURVIVAL ends on a different hash {(differs ? "YES" : "NO")}: {(p4 ? "PASS" : "FAIL")}");
            ok &= p4;
        }

        code = ok ? 0 : 1;
        Console.WriteLine(ok ? "grind2 PASS" : "grind2 FAIL");
    }

    static ulong LastHash(RunTape t)
    {
        List<(int step, ulong hash)> c = t.Checkpoints();
        return c.Count > 0 ? c[c.Count - 1].hash : 0;
    }

    static RunTape GrindTape(GameConfig cfg, SurvivalMap map, string mode, uint seed, int steps)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n21Override = new N21Blocks { grind = mode == RunSetup.Grind };
        sim.setup.mode = mode;
        sim.setup.heat = 0;
        return TapeScript.Record(sim, seed, steps, "grind2", null, TapeScript.God);
    }
}
