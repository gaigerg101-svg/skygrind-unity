// NIGHT20 (the shared commit, the lead owns this file): the view hooks for tonight's builders, one pair per builder file
// so nobody edits SurvivalGame's Init or Render (a hook nobody implements compiles to nothing). Init runs after NIGHT19's
// views are made; Render runs in the enemies' block (the same frame time: 0 inside a hitstop) and the effects' block.
// - N: Game/EnemyViews.Climb.cs (N20ClimbView*: the climb's pose and tell)
// - E: Game/EnemyViews.N20.cs (N20EnemyView*: the new kinds' views, LANCER / BULWARK / WARDEN, as partials)
// - V: Game/SpeedLines20.cs and the shockwave's look (N20Fx*: the AIR streaks, the travel blur's strength)
// - G: Game/HudReads19.Arsenal.cs needs no hook (H reads it)
namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        partial void N20ClimbViewInit();
        partial void N20ClimbViewRender(double alpha, double frameDt);
        partial void N20EnemyViewInit();
        partial void N20EnemyViewRender(double alpha, double frameDt);
        partial void N20FxInit();
        partial void N20FxRender(double alpha, double frameDt);

        void N20ViewsInit()
        {
            N20ClimbViewInit();
            N20EnemyViewInit();
            N20FxInit();
        }

        void N20EnemyViewsRender(double alpha, double frameDt)
        {
            N20ClimbViewRender(alpha, frameDt);
            N20EnemyViewRender(alpha, frameDt);
        }

        void N20EffectsRender(double alpha, double frameDt) => N20FxRender(alpha, frameDt);
    }
}
