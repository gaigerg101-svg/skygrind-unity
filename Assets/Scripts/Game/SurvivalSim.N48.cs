// NIGHT48 (the shared commit; Tools/night/n48-shared.cjs): the night's sim rows (design.n48.*, sim: true; Core/NightRows.cs,
// docs/NIGHT-ROWS.md section 2), each builder in its own partial file:
//   W (SurvivalSim.N48.Wick.cs):  no sim row (climbTop48 is a level row) -> N48WStartRun / N48WStep / N48WCampaign
//   B (SurvivalSim.N48.Brawl.cs): wyrmTell48, wyrmDive48, kilnBoost48   -> N48BStartRun / N48BStep / N48BCampaign
//   L (SurvivalSim.N48.Loom.cs):  no sim row (loomHull48 is a view row) -> N48LStartRun / N48LStep / N48LCampaign
//   F (SurvivalSim.N48.Feel.cs):  no sim row (blast48, guns48, foes48, lineup48 are view rows) -> N48FStartRun / N48FStep / N48FCampaign
// Off: -skygrindOff48 / --off48 (or any older switch): every id reads off. The level row (climbTop48) and the view rows are data only:
// Curriculum25 swaps the level; a view asks NightRows.ViewOn(cfg, "<row>"); lineup48 is read by the lineup reel only.
// A builder resolves its ids once a run in its StartRun and reads rows.On(id) per step (an array read, no allocation). Each builder owns its bosses'
// Core files (Core/Enemies/*) and calls them from its own partial; nobody edits SurvivalSim.Rows.cs, FeelReel.N27.cs or this file tonight.
// A builder that needs a new row asks the lead (NIGHT48.md section 0).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        // one id per row in rowsRegistry, resolved once a run (-1: this config lacks the row; rows.On(-1) is false, the older game)
        internal int idWyrmTell48 = -1, idWyrmDive48 = -1, idKilnBoost48 = -1; // B

        partial void Night48StartRun()
        {
            idWyrmTell48 = rowsRegistry.IndexOf("wyrmTell48");
            idWyrmDive48 = rowsRegistry.IndexOf("wyrmDive48");
            idKilnBoost48 = rowsRegistry.IndexOf("kilnBoost48");
            N48WStartRun(); // builder W
            N48BStartRun(); // builder B
            N48LStartRun(); // builder L
            N48FStartRun(); // builder F
        }

        partial void Night48Step(double dt)
        {
            N48WStep(dt); // builder W
            N48BStep(dt); // builder B
            N48LStep(dt); // builder L
            N48FStep(dt); // builder F
        }

        /// <summary>Once a run, right after N47Campaign (the campaign is made): the builders set their Campaign24 delegates here, only for rows
        /// that are on (a null delegate is NIGHT47's rule).</summary>
        partial void N48Campaign()
        {
            N48WCampaign();
            N48BCampaign();
            N48LCampaign();
            N48FCampaign();
        }

        partial void N48Campaign(); // the declaration (the call: SurvivalSim.Campaign24.cs)
        partial void N48WStartRun(); // W: reset every per-run field here (rule 6); numbers from design.n48.<row>.numbers
        partial void N48WStep(double dt);
        partial void N48WCampaign();
        partial void N48BStartRun(); // B
        partial void N48BStep(double dt);
        partial void N48BCampaign();
        partial void N48LStartRun(); // L
        partial void N48LStep(double dt);
        partial void N48LCampaign();
        partial void N48FStartRun(); // F
        partial void N48FStep(double dt);
        partial void N48FCampaign();
    }
}
