// NIGHT33 builder W (design.n33.pierce33, DESIGN33 W, D-4): the charged KESTREL pierces everything in a line (Core/Guns/Pierce33.cs).
// The run's start puts the row's numbers on the loadout (null with the row off: NIGHT32's charged shot); the shot's `kestrelPierce33` event, when it
// hit a body, asks for hitstopSteps whole steps of hitstop (the frame loop's freeze, the longest request wins).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        Pierce33Params pierce33P;
        bool pierce33Hooked;

        partial void N33PierceStartRun()
        {
            loadout.pierce33Shots = loadout.pierce33Hits = 0;
            if (n33.pierce33)
            {
                pierce33P ??= Pierce33Params.From(cfg);
                loadout.pierce33 = pierce33P;
                if (!pierce33Hooked)
                {
                    pierce33Hooked = true;
                    events.On<RailFireEvent>(Loadout.KestrelPierce33, OnKestrelPierce33);
                }
            }
            else loadout.pierce33 = null;
        }

        void OnKestrelPierce33(RailFireEvent e)
        {
            if (!n33.pierce33 || e.owner != "player" || e.hits <= 0) return;
            Hitstop(pierce33P.hitstopSteps * cfg.Dt, Loadout.KestrelPierce33);
        }
    }
}
