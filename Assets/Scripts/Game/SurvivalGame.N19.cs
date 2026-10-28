// NIGHT19 (the shared commit, the lead owns this file): the view hooks for the new enemy kinds and the ported guns'
// effects, one pair per builder file so nobody edits SurvivalGame's Init or Render (a hook nobody implements compiles to
// nothing). Init runs after the three kinds' views are made; Render runs in the enemies' block (the same frame time:
// 0 inside a hitstop) and the projectiles' (the ported guns' shots).
// - E: Game/EnemyViews.Charger.cs (N19ChargerView*), .Drone.cs (N19DroneView*), .Affix.cs (N19AffixView*)
// - V: Game/GunFx.Zapper.cs (N19ZapperFx*), GunFx.Bouncer.cs (N19BouncerFx*)
namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        partial void N19ChargerViewInit();
        partial void N19ChargerViewRender(double alpha, double frameDt);
        partial void N19DroneViewInit();
        partial void N19DroneViewRender(double alpha, double frameDt);
        partial void N19AffixViewInit();
        partial void N19AffixViewRender(double alpha, double frameDt);
        partial void N19ZapperFxInit();
        partial void N19ZapperFxRender(double alpha, double frameDt);
        partial void N19BouncerFxInit();
        partial void N19BouncerFxRender(double alpha, double frameDt);

        void N19ViewsInit()
        {
            N19ChargerViewInit();
            N19DroneViewInit();
            N19AffixViewInit();
            N19ZapperFxInit();
            N19BouncerFxInit();
        }

        void N19EnemyViewsRender(double alpha, double frameDt)
        {
            N19ChargerViewRender(alpha, frameDt);
            N19DroneViewRender(alpha, frameDt);
            N19AffixViewRender(alpha, frameDt);
        }

        void N19GunFxRender(double alpha, double frameDt)
        {
            N19ZapperFxRender(alpha, frameDt);
            N19BouncerFxRender(alpha, frameDt);
        }
    }
}
