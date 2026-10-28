// NIGHT45 builder S1 (rows as data; NIGHT45.md card S1): the sim side of the generic rows registry (Core/NightRows.cs). UNWIRED tonight:
// nothing calls RowsInit / RowsStartRun / RowsStep yet, so this file adds fields and uncalled methods only (no tape bit, no step cost; the
// fingerprint EQUAL plain and --off45). NIGHT46's G0 wires it once (docs/NIGHT-ROWS.md, the one-time steps):
//   SurvivalSim.cs, after N43Init():          RowsInit();
//   SurvivalSim.Run.cs, after N43StartRun():  RowsStartRun();
//   SurvivalSim.Run.cs, after N43Step(step):  RowsStep(step);
//   SurvivalSim.Run.cs / TapeScript.cs, the fingerprint's save / restore: RowsFingerprintEnter() / RowsFingerprintExit().
// After that a night NN's sim code lives in its own SurvivalSim.NNN.cs, which fills NightNNStartRun / NightNNStep below (pre-declared for
// nights 46-57: a partial with no body costs nothing) and asks rows.On(id) with ids resolved once in its StartRun (IndexOf). Nights run in
// order (46, 47, ...), after NIGHT43's step: the web's simulate() order with each night's systems appended, as N40Step-N43Step are.
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>NIGHT45 S1: the composed fingerprint's rows rule (FingerprintBaseline.N4xOnPath, generic): rows of nights up to and including
    /// this one run as configured; later nights' rows run off until their freeze records them. The freeze that records a night's rows ON moves
    /// this to that night (one number, one commit; no N46OnPath, N47OnPath ...).</summary>
    public static class RowsBaseline
    {
        /// <summary>NIGHT46's freeze: night 46's rows run as configured in the composed fingerprint (measured EQUAL: the survival run never
        /// reads them); NIGHT47+ rows run off until their freeze.
        /// NIGHT48 G0 (DECIDE47 D-17): moved to 47, NIGHT47's freeze rows (pulse47, overheal47, the view rows) run as configured. The
        /// composed fingerprint was measured after the move and is EQUAL (no hash moved; the commit message has the four checks). NIGHT48+ rows run off.
        /// NIGHT49 P0 (DECIDE49 D-7; NIGHT48's freeze, moved here on its own commit): NIGHT48's rows run as configured in the composed fingerprint.
        /// Every n48 row was OFF at the move, so no hash could move (measured EQUAL: plain / --off47 / --off48 / --off49). NIGHT49+ rows run off.</summary>
        public const int OnPathThrough = 48;
    }

    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT45 S1: every NIGHT46+ sim row in this config, read once (RowsInit).</summary>
        public NightRows rowsRegistry = NightRows.Empty;

        /// <summary>A replay's or a harness's rows (a tape's record, decoded against rowsRegistry); null = the design's, or off where the
        /// off switch covers the row's night (NightRows.ForceOffFrom).</summary>
        public NightRowState rowsOverride;

        /// <summary>The rows the current run reads (set at RowsStartRun; never null after RowsInit).</summary>
        public NightRowState rows;

        NightRowState rowsFingerprintSaved;

        /// <summary>Read the registry once (SurvivalSim's Init, after N43Init).</summary>
        void RowsInit()
        {
            rowsRegistry = NightRows.From(cfg);
            rows = rowsRegistry.AllOff();
        }

        /// <summary>The run's rows (StartRun, after N43StartRun): the override when handed, else the design with the off switch; then each
        /// night's StartRun hook in order.</summary>
        void RowsStartRun()
        {
            rows = rowsRegistry.Resolve(rowsOverride);
            Night46StartRun();
            Night47StartRun();
            Night48StartRun();
            Night49StartRun();
            Night50StartRun();
            Night51StartRun();
            Night52StartRun();
            Night53StartRun();
            Night54StartRun();
            Night55StartRun();
            Night56StartRun();
            Night57StartRun();
        }

        /// <summary>Every step, after N43Step: each night's step hook in order (an empty partial compiles to nothing).</summary>
        void RowsStep(double dt)
        {
            Night46Step(dt);
            Night47Step(dt);
            Night48Step(dt);
            Night49Step(dt);
            Night50Step(dt);
            Night51Step(dt);
            Night52Step(dt);
            Night53Step(dt);
            Night54Step(dt);
            Night55Step(dt);
            Night56Step(dt);
            Night57Step(dt);
        }

        /// <summary>The fingerprint's save (RunFingerprint / TapeScript, beside the N43OnPath line): rows of nights after
        /// RowsBaseline.OnPathThrough off until their freeze.</summary>
        public void RowsFingerprintEnter()
        {
            rowsFingerprintSaved = rowsOverride;
            rowsOverride = rowsRegistry.Resolve(rowsOverride).Through(RowsBaseline.OnPathThrough);
        }

        /// <summary>The fingerprint's restore (beside n43Override = n43Saved).</summary>
        public void RowsFingerprintExit()
        {
            rowsOverride = rowsFingerprintSaved;
            rowsFingerprintSaved = null;
        }

        /// <summary>The record a tape keeps of this run's rows (the recorder's header, beside tape.n43 = sim.n43.Bits).</summary>
        public byte[] RowsTapeRecord() => NightRowsTape.Encode(rows);

        partial void Night46StartRun();
        partial void Night46Step(double dt);
        partial void Night47StartRun();
        partial void Night47Step(double dt);
        partial void Night48StartRun();
        partial void Night48Step(double dt);
        partial void Night49StartRun();
        partial void Night49Step(double dt);
        partial void Night50StartRun();
        partial void Night50Step(double dt);
        partial void Night51StartRun();
        partial void Night51Step(double dt);
        partial void Night52StartRun();
        partial void Night52Step(double dt);
        partial void Night53StartRun();
        partial void Night53Step(double dt);
        partial void Night54StartRun();
        partial void Night54Step(double dt);
        partial void Night55StartRun();
        partial void Night55Step(double dt);
        partial void Night56StartRun();
        partial void Night56Step(double dt);
        partial void Night57StartRun();
        partial void Night57Step(double dt);
    }
}
