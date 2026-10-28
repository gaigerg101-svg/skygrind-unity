// NIGHT20 U3-34.10 (builder H): tonight's HUD, built once after the rest of the HUD (Ui.cs calls BuildHud20 last), each part
// in its own file: the shockwave chip (UiCooldown20.cs), the zoom reticle and the scope (UiZoom20.cs), the segments and the
// refill (UiVitals20.cs). Every part reads the sim through HudReads20 and shows only while its NIGHT20 block is live.
namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        void BuildHud20()
        {
            HudReads20.Bind(game);
            BuildCooldown20();
            BuildZoom20();
            BuildVitals20();
            BuildAnnouncer20(); // F1 (UiAnnouncer20.cs)
            BuildNext20(); // A8 (UiNext20.cs)
            BuildDebut20(); // C1 (UiDebut20.cs)
            BuildClimbArrow20(); // the climb's off-screen arrow (UiClimbArrow20.cs)
            BuildThreat20(); // A1 / A4: the boss bar and the elite's plate (UiThreat20.cs)
            BuildLancer20(); // C2: the LANCER's off-screen tell (UiLancer20.cs)
            BuildCodex20(); // E3: the CODEX tab inside WEAPONS (UiCodex20.cs)
            BuildDownTell20(); // F4: WHAT GOT YOU's and DOWN's tell (UiDownTell20.cs; after BuildMenus and BuildRecap)
            BuildReveal20(); // F3: the reward reveal (UiReveal20.cs; a menus-layer screen, built last: the top one)
        }
    }
}
