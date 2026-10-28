// EXT53 (NIGHT53's extension; builder N, docs/NIGHT-ROWS.md section 2): the night's sim rows (design.n53.*, sim: true; Core/NightRows.cs),
// each builder in its own partial file:
//   N (SurvivalSim.N53.RideKill.cs): rideKill53 -> N53RideKillStartRun / N53RideKillStep
// Off: -skygrindOff53 / --off53 (or any older switch): every id reads off. Rows of night 53 run off in the composed fingerprint until a freeze
// moves RowsBaseline.OnPathThrough (SurvivalSim.Rows.cs). A builder resolves its ids once a run and reads rows.On(id) (an array read).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        // one id per row in rowsRegistry, resolved once a run (-1: this config lacks the row; rows.On(-1) is false, the older game)
        internal int idRideKill53 = -1; // N

        partial void Night53StartRun()
        {
            idRideKill53 = rowsRegistry.IndexOf("rideKill53");
            N53RideKillStartRun(); // builder N
        }

        partial void Night53Step(double dt)
        {
            N53RideKillStep(dt); // builder N
        }

        partial void N53RideKillStartRun(); // N: reset every per-run field here (rule 6); numbers from design.n53.rideKill53.numbers
        partial void N53RideKillStep(double dt);
    }
}
