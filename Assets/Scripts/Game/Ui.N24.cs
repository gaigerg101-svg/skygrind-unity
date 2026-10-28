// NIGHT24 (the shared commit, the lead owns this file): the HUD hooks for NIGHT24's widgets (the reticle arc, the BIG PICK screen, the
// room banner). A builder implements the partials in its own file (UiReticleArc24.cs, UiDraft*.cs, ...). Nothing runs without a body.
// Wiring: Ui.BuildHud calls BuildN24Hud() after BuildN22Hud(); Ui.Render calls RenderN24Hud(frameDt) after RenderN22Hud(frameDt).
namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        void BuildN24Hud() { BuildReticleArc24(); BuildCampaignCue24(); } // NIGHT24 builder F: the arc, the door cue and the room banner
        void RenderN24Hud(double frameDt) { RenderReticleArc24(frameDt); RenderCampaignCue24(frameDt); }
    }
}
