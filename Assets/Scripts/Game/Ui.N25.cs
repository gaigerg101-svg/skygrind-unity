// NIGHT25 (the shared commit, the lead owns this file): the HUD hooks for NIGHT25's widgets (the boss bar, the lesson prompts).
// A builder implements the declaration-only partials in its own file (Ui.Lesson25.cs, ...). Wiring: Ui.BuildHud calls BuildN25Hud()
// after BuildN24Hud(); Ui.Render calls RenderN25Hud(frameDt) after RenderN24Hud(frameDt).
namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        void BuildN25Hud() => BuildN25HudHook();
        void RenderN25Hud(double frameDt) => RenderN25HudHook(frameDt);

        partial void BuildN25HudHook();
        partial void RenderN25HudHook(double frameDt);
    }
}
