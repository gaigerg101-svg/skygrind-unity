// NIGHT26 (the shared commit, the lead owns this file): the HUD hooks for NIGHT26's widgets (the level select, the ranks, the gap meter).
// A builder implements the declaration-only partials in its own file (UiLevelSelect26.cs, ...). Wiring: Ui.BuildHud calls BuildN26Hud()
// after BuildN25Hud(); Ui.Render calls RenderN26Hud(frameDt) after RenderN25Hud(frameDt).
namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        void BuildN26Hud() => BuildN26HudHook();
        void RenderN26Hud(double frameDt) => RenderN26HudHook(frameDt);

        partial void BuildN26HudHook();
        partial void RenderN26HudHook(double frameDt);
    }
}
