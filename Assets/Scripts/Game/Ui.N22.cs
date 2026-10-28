// NIGHT22 (the lead owns this file): the one body of Ui's BuildN22Hud / RenderN22Hud hooks (Ui.cs), calling each builder's widget
// in its own partial file. A builder's widget is never called from anywhere else; a row whose switch is off builds nothing
// visible (each widget reads the sim's switch itself).
namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        partial void BuildN22Hud()
        {
            BuildStamp22(); // S: THE STAMP (UiStamp22.cs)
            BuildDeadline22Bar(); // S: the DEADLINE fuse bar (UiDeadline22.cs)
            BuildArcline22Chip(); // M: the ARCLINE chip (UiArcline22.cs)
            BuildDirected22Cue(); // M: THE HUNT / THE BEACON's directed steps (UiDirected22.cs)
        }

        partial void RenderN22Hud(double frameDt)
        {
            RenderStamp22(frameDt);
            RenderDeadline22Bar(frameDt);
            RenderArcline22Chip(frameDt);
            RenderCode22(frameDt); // W2: the CODE sheet and the RESULTS code line (Code22Field.cs)
            RenderDirected22Cue(frameDt);
        }
    }
}
