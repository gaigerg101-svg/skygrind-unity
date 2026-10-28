// NIGHT24 (the shared commit, the lead owns this file): the hooks the NIGHT24 world views hang on, placed once so builders never
// wait on SurvivalGame.Init.cs / SurvivalGame.Render.cs. A builder implements the partial in its own file and names the one call in
// its report. Nothing runs without a body. (The HUD widgets use Ui's BuildN24Hud / RenderN24Hud the same way, Ui.N24.cs.)
namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        /// <summary>Once at game init, after the NIGHT22 views: the NIGHT24 world views (the wyrm's, the room lighting, ...).</summary>
        void N24InitViews()
        {
            Wyrm24InitView(); // NIGHT24 builder B
            Prewarm24.Start(this); // NIGHT24 switch-lag (builder F; off unless design.n24.prewarm.enabled)
            N24InitViewsHook();
        }

        /// <summary>Every render frame beside the other world views (worldDt: the slowed world frame time; alpha: the sim's render interpolation).</summary>
        void N24RenderViews(double worldDt, double alpha)
        {
            Wyrm24RenderView(worldDt, alpha); // NIGHT24 builder B
            N24RenderViewsHook(worldDt, alpha);
        }

        partial void N24InitViewsHook();
        partial void N24RenderViewsHook(double worldDt, double alpha);
    }
}
