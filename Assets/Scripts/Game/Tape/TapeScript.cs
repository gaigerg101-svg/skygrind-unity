// NIGHT14 U3-27.8 (agent T): taped runs without the Game (the tests, Tools/SimHarness, the farm if it wants tapes). Record
// starts a run on a composition, begins a tape, then steps it: on the fingerprint's input script (SimFingerprint.Script
// through a ScriptedInput, the look its own), or on whatever input the sim has (the AUTOPILOT); `between` runs before
// each step (what the Game does between steps: a lock toggle, a MODS draft taken). Fingerprint is the fingerprint run
// (SurvivalSim.RunFingerprint's setup and its 1e6 HP), taped: its checkpoints must be the baseline's. No UnityEngine use.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class TapeScript
    {
        /// <summary>The fingerprint's (and the smoke's) god mode after StartRun: the tape keeps it as the health at the first step.</summary>
        public static readonly Action<SurvivalSim> God = s => s.health.max = s.health.hp = 1e6;

        /// <summary>SurvivalSim.RunFingerprint's run on this composition (Survival, HEAT 0, no LAB, no records, seed 1, 1e6 HP, the script), taped.</summary>
        public static RunTape Fingerprint(SurvivalSim sim, int steps, string stamp)
        {
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.setup.seed = null;
            // NIGHT20: as RunFingerprint, every NIGHT19 / NIGHT20 block off until the close re-records the on-baseline
            // (FingerprintBaseline.N19OnPath), so the taped run prints NIGHT18's baseline; the overrides handed back.
            N19Blocks? n19Saved = sim.n19Override;
            N20Blocks? n20Saved = sim.n20Override;
            N21Blocks? n21Saved = sim.n21Override; // NIGHT21: and every NIGHT21 block off until the close (N21OnPath)
            N22Blocks? n22Saved = sim.n22Override; // NIGHT22: and every NIGHT22 block off until the close (N22OnPath)
            N27Blocks? n27Saved = sim.n27Override; // NIGHT27: and every NIGHT27 block off until the freeze (N27OnPath)
            if (!FingerprintBaseline.N27OnPath) sim.n27Override = N27Blocks.Off;
            N29Blocks? n29Saved = sim.n29Override; // NIGHT29: and every NIGHT29 block off until the freeze (N29OnPath)
            if (!FingerprintBaseline.N29OnPath) sim.n29Override = N29Blocks.Off;
            N30Blocks? n30Saved = sim.n30Override; // NIGHT30: and every NIGHT30 block off until the freeze (N30OnPath)
            if (!FingerprintBaseline.N30OnPath) sim.n30Override = N30Blocks.Off;
            N31Blocks? n31Saved = sim.n31Override; // NIGHT31: and every NIGHT31 block off until the freeze (N31OnPath)
            if (!FingerprintBaseline.N31OnPath) sim.n31Override = N31Blocks.Off;
            N32Blocks? n32Saved = sim.n32Override; // NIGHT32: and every NIGHT32 block off until the freeze (N32OnPath)
            if (!FingerprintBaseline.N32OnPath) sim.n32Override = N32Blocks.Off;
            N33Blocks? n33Saved = sim.n33Override; // NIGHT33: and every NIGHT33 block off until the freeze (N33OnPath); nothing bought or banked
            if (!FingerprintBaseline.N33OnPath) sim.n33Override = N33Blocks.Off;
            int shop33Saved = sim.shop33Owned, bank33Saved = sim.bank33;
            sim.shop33Owned = sim.bank33 = 0;
            N34Blocks? n34Saved = sim.n34Override; // NIGHT34: and every NIGHT34 block off until the freeze (N34OnPath)
            if (!FingerprintBaseline.N34OnPath) sim.n34Override = N34Blocks.Off;
            N35Blocks? n35Saved = sim.n35Override; // NIGHT35: and every NIGHT35 block off until the freeze (N35OnPath)
            if (!FingerprintBaseline.N35OnPath) sim.n35Override = N35Blocks.Off;
            N36Blocks? n36Saved = sim.n36Override; // NIGHT36: and every NIGHT36 block off until the freeze (N36OnPath)
            if (!FingerprintBaseline.N36OnPath) sim.n36Override = N36Blocks.Off;
            N37Blocks? n37Saved = sim.n37Override; // NIGHT37: and every NIGHT37 block off until the freeze (N37OnPath)
            if (!FingerprintBaseline.N37OnPath) sim.n37Override = N37Blocks.Off;
            N38Blocks? n38Saved = sim.n38Override; // NIGHT38: and every NIGHT38 block off until the freeze (N38OnPath)
            if (!FingerprintBaseline.N38OnPath) sim.n38Override = N38Blocks.Off;
            N39Blocks? n39Saved = sim.n39Override; // NIGHT39: and every NIGHT39 block off until the freeze (N39OnPath)
            if (!FingerprintBaseline.N39OnPath) sim.n39Override = N39Blocks.Off;
            N40Blocks? n40Saved = sim.n40Override; // NIGHT40: and every NIGHT40 block off until the freeze (N40OnPath)
            if (!FingerprintBaseline.N40OnPath) sim.n40Override = N40Blocks.Off;
            N41Blocks? n41Saved = sim.n41Override; // NIGHT41: and every NIGHT41 block off until the freeze (N41OnPath)
            if (!FingerprintBaseline.N41OnPath) sim.n41Override = N41Blocks.Off;
            N42Blocks? n42Saved = sim.n42Override; // NIGHT42: and every NIGHT42 block off until the freeze (N42OnPath)
            if (!FingerprintBaseline.N42OnPath) sim.n42Override = N42Blocks.Off;
            N43Blocks? n43Saved = sim.n43Override; // NIGHT43: and every NIGHT43 block off until the freeze (N43OnPath)
            if (!FingerprintBaseline.N43OnPath) sim.n43Override = N43Blocks.Off;
            sim.RowsFingerprintEnter(); // NIGHT46 rows: rows of nights after RowsBaseline.OnPathThrough off until their freeze
            N26Blocks? n26Saved = sim.n26Override; // NIGHT26: and every NIGHT26 block off until the freeze (N26OnPath)
            if (!FingerprintBaseline.N26OnPath) sim.n26Override = N26Blocks.Off;
            N25Blocks? n25Saved = sim.n25Override; // NIGHT25: and every NIGHT25 block off until the freeze (N25OnPath)
            if (!FingerprintBaseline.N25OnPath) sim.n25Override = N25Blocks.Off;
            N24Blocks? n24Saved = sim.n24Override; // NIGHT24: and every NIGHT24 block off until the freeze (N24OnPath)
            if (!FingerprintBaseline.N19OnPath)
            {
                sim.n19Override = N19Blocks.Off;
                sim.n20Override = N20Blocks.Off;
            }
            if (!FingerprintBaseline.N21OnPath) sim.n21Override = N21Blocks.Off;
            if (!FingerprintBaseline.N22OnPath) sim.n22Override = N22Blocks.Off;
            if (!FingerprintBaseline.N24OnPath) sim.n24Override = N24Blocks.Off;
            try { return Record(sim, SimFingerprint.Seed, steps, stamp, null, God); }
            finally
            {
                sim.n19Override = n19Saved;
                sim.n20Override = n20Saved;
                sim.n21Override = n21Saved;
                sim.n22Override = n22Saved;
                sim.n24Override = n24Saved;
                sim.n26Override = n26Saved;
                sim.n27Override = n27Saved;
                sim.n29Override = n29Saved;
                sim.n30Override = n30Saved;
                sim.n31Override = n31Saved;
                sim.n32Override = n32Saved;
                sim.n33Override = n33Saved;
                sim.shop33Owned = shop33Saved;
                sim.bank33 = bank33Saved;
                sim.n34Override = n34Saved;
                sim.n35Override = n35Saved;
                sim.n36Override = n36Saved;
                sim.n37Override = n37Saved;
                sim.n38Override = n38Saved;
                sim.n39Override = n39Saved;
                sim.n40Override = n40Saved;
                sim.n41Override = n41Saved;
                sim.n42Override = n42Saved;
                sim.n43Override = n43Saved;
                sim.RowsFingerprintExit(); // NIGHT46 rows
                sim.n25Override = n25Saved;
            }
        }

        /// <summary>
        /// StartRun(seed), a tape begun, `afterStart`, then up to `steps` steps (fewer when the run reaches RESULTS), each after
        /// `between(sim, i)`; scripted: the fingerprint's input script drives a ScriptedInput (the sim's own if it has one).
        /// The tape is stopped with a last checkpoint (or ended by the run itself).
        /// </summary>
        public static RunTape Record(SurvivalSim sim, uint? seed, int steps, string stamp, Action<SurvivalSim, int> between = null, Action<SurvivalSim> afterStart = null, bool scripted = true)
        {
            var rec = new TapeRecorder(sim, stamp);
            ScriptedInput script = null;
            if (scripted)
            {
                script = sim.input as ScriptedInput;
                if (script == null)
                {
                    script = new ScriptedInput();
                    sim.UseInput(script); // the look carried over, as RunFingerprint's
                }
            }
            sim.StartRun(seed, null, null);
            rec.Begin();
            afterStart?.Invoke(sim);
            Drive(sim, steps, between, script);
            return rec.Recording ? rec.Finish(TapeEnd.Stopped) : rec.Tape;
        }

        /// <summary>Steps `first` .. `steps` - 1 of the run (the harness measures the recorder's allocations against this loop with no tape).</summary>
        public static void Drive(SurvivalSim sim, int steps, Action<SurvivalSim, int> between, ScriptedInput script, int first = 0)
        {
            for (int i = first; i < steps; i++)
            {
                if (sim.flow != Flow.Playing && sim.flow != Flow.Down) break; // RESULTS: the run is over
                between?.Invoke(sim, i);
                if (script != null)
                {
                    double yaw = script.Yaw, pitch = script.Pitch;
                    SimFingerprint.Script(i, sim.Eye, sim.enemies, ref yaw, ref pitch, out InputFrame move, out CombatInput combat);
                    script.SetLook(yaw, pitch);
                    script.Set(move, combat);
                }
                sim.Step();
            }
        }
    }
}
