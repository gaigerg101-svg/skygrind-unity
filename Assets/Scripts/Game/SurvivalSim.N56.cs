// NIGHT56 (builder G; docs/NIGHT-ROWS.md section 2): the night's sim rows (design.n56.*, sim: true; Core/NightRows.cs), each in its own partial:
//   G1 (SurvivalSim.N56.Rank.cs):   rank56   -> N56RankStartRun
//   G3 (SurvivalSim.N56.Tokens.cs): tokens56 -> N56TokensStartRun / N56TokensStep
//   G4 (SurvivalSim.N56.Wyrm.cs):   NIGHT55's wyrm55 (THE KILN WYRM's encounter numbers) -> N56WyrmStartRun / N56WyrmStep
// Off: -skygrindOff56 / --off56 (or any older switch): every id reads off. Rows of night 56 run off in the composed fingerprint until a freeze
// moves RowsBaseline.OnPathThrough (SurvivalSim.Rows.cs). Ids resolve once a run; rows.On(id) per use (an array read).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        // one id per row in rowsRegistry, resolved once a run (-1: this config lacks the row; rows.On(-1) is false, the older game)
        internal int idRank56 = -1, idTokens56 = -1;

        partial void Night56StartRun()
        {
            idRank56 = rowsRegistry.IndexOf("rank56");
            idTokens56 = rowsRegistry.IndexOf("tokens56");
            N56RankStartRun();
            N56TokensStartRun();
            N56WyrmStartRun(); // G4: design.n55.wyrm55 (its own id; a NIGHT55 row read from NIGHT56's hook: after wyrm46 / wyrm47)
        }

        partial void Night56Step(double dt)
        {
            N56TokensStep(dt);
            N56WyrmStep(dt);
        }

        partial void N56RankStartRun(); // G1: reset every per-run field here (rule 6); numbers from design.n56.rank56.numbers
        partial void N56TokensStartRun(); // G3: numbers from design.n56.tokens56.numbers
        partial void N56TokensStep(double dt);
    }
}
