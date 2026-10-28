// NIGHT22 (the shared commit, the lead owns this file): the hooks the NIGHT22 world views hang on, placed once so builders
// never wait on SurvivalGame.Init.cs / SurvivalGame.Render.cs. A builder implements the partial in its own file and names the
// one call in its report. Nothing runs without a body. (The HUD widgets use Ui's BuildN22Hud / RenderN22Hud the same way.)
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        GrindTelegraph22 grindTelegraph22; // NIGHT22 THE GRIND's rising-edge telegraph (builder W)
        DuskClock22 duskClock22; // NIGHT22 THE DUSK CLOCK (the lead; Game/DuskClock22.cs)

        /// <summary>The dusk clock's share now (the reel's read; 0 with the row off).</summary>
        public double DuskShare => duskClock22 != null ? duskClock22.Share : 0;

        /// <summary>Once at game init, after the NIGHT21 views: the NIGHT22 world views. Builders add their own view in a one-line call here (the lead wires it).</summary>
        void N22InitViews()
        {
            duskClock22 = new DuskClock22(this);
            grindTelegraph22 = new GrindTelegraph22(this);
            // THE GRIND's records: their own file beside the profile (builder W: Records21 Save / Load), memory-only in automation
            if (!sim.noRecords) sim.GrindRecordsFile(System.IO.Path.Combine(Application.persistentDataPath, "grind-records.txt"));
            new Haptics22View(this); // M: pad haptics (Haptics22View.cs keeps its own Instance)
        }

        /// <summary>Every render frame beside the other world views (worldDt: the slowed world frame time; alpha: the sim's render interpolation).</summary>
        void N22RenderViews(double worldDt, double alpha)
        {
            // the dusk clock moves on frame time (a slow-mo doesn't slow the sky), once per render frame
            duskClock22?.Render(RealFrameDt20(), flow == Flow.Title, run != null ? run.round : 1);
            Haptics22View.Instance?.Render();
            grindTelegraph22?.Render(worldDt, alpha);
        }
    }
}
