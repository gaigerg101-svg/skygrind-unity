// NIGHT24 builder B: the KILN WYRM's two calls into the game (the lead owns SurvivalGame.N24.cs and has them in the hooks):
//   N24InitViewsHook:   Wyrm24InitView();
//   N24RenderViewsHook: Wyrm24RenderView(worldDt, alpha);
// (Builder L's lighting kit hangs on the same hooks, so neither of us implements the partial: the lead composes them.)
using System.IO;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        public WyrmView wyrmView;
        public MonolithView monolithView;
        public RivalView rivalView; // NIGHT25 W: RIVAL PRIME's body (the only magenta in the game)
        public WyrmRoomView wyrmRoomView; // NIGHT25 W: the room's stone and light (bug-void)

        /// <summary>NIGHT25 W: the worm fights now (the lead's Ui.cs hides the SURVIVAL round line while it does: roundText.SetText(string.Empty)).</summary>
        public bool WyrmFightLive => sim != null && sim.wyrm24 != null && sim.wyrm24.Alive;

        /// <summary>Once at game init: the worm's view, and the folder its files are read from (Design/ beside PortExport/).</summary>
        public void Wyrm24InitView()
        {
            if (!string.IsNullOrEmpty(dataRoot)) SurvivalSim.Wyrm24Dir = Path.GetDirectoryName(dataRoot);
            wyrmView = new WyrmView(this);
            wyrmRoomView = new WyrmRoomView(this);
            rivalView = new RivalView(this); // NIGHT25 W: RIVAL PRIME
            monolithView = new MonolithView(this); // MONOLITH (leftover 8)
            Boss26InitViews(); // NIGHT26 B: the Slag Hulk's view
            wyrm47Beats = new Wyrm47BeatsView(this); // NIGHT47 B: THE KILN WYRM's foreshadow beats (G35)
        }

        public Wyrm47BeatsView wyrm47Beats;

        /// <summary>Every render frame beside the other world views.</summary>
        public void Wyrm24RenderView(double worldDt, double alpha)
        {
            wyrmView?.Render(alpha, worldDt);
            wyrmRoomView?.Render();
            rivalView?.Render(alpha);
            monolithView?.Render(alpha, worldDt);
            Boss26RenderViews(alpha); // NIGHT26 B
            wyrm47Beats?.Render(); // NIGHT47 B (G35)
        }
    }
}
