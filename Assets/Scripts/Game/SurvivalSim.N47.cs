// NIGHT47 (the shared commit; Tools/night/n47-shared.cjs): the night's sim rows (design.n47.*, sim: true; Core/NightRows.cs,
// docs/NIGHT-ROWS.md section 2), each builder in its own partial file:
//   W (SurvivalSim.N47.Wick.cs):  wick47, climb47, wickTop47 -> N47WStartRun / N47WStep / N47WCampaign
//   B (SurvivalSim.N47.Brawl.cs): charger47, brawl47, wyrm47 -> N47BStartRun / N47BStep / N47BCampaign
//   L (SurvivalSim.N47.Loom.cs):  loom47, crown47            -> N47LStartRun / N47LStep / N47LCampaign
//   F (SurvivalSim.N47.Feel.cs):  pulse47, overheal47        -> N47FStartRun / N47FStep / N47FCampaign
// Off: -skygrindOff47 / --off47 (or any older switch): every id reads off. The level rows (foundry47 ... spire47) and the view rows are data only:
// Curriculum25 swaps the levels; a view asks NightRows.ViewOn(cfg, "<row>").
// A builder resolves its ids once a run in its StartRun and reads rows.On(id) per step (an array read, no allocation). Each builder owns its bosses'
// Core files (Core/Enemies/*) and calls them from its own partial; nobody edits SurvivalSim.Rows.cs, FeelReel.N27.cs or this file tonight.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        // one id per row in rowsRegistry, resolved once a run (-1: this config lacks the row; rows.On(-1) is false, the older game)
        internal int idWick47 = -1, idClimb47 = -1, idWickTop47 = -1; // W
        internal int idCharger47 = -1, idBrawl47 = -1, idWyrm47 = -1; // B
        internal int idLoom47 = -1, idCrown47 = -1; // L
        internal int idPulse47 = -1, idOverheal47 = -1; // F

        partial void Night47StartRun()
        {
            idWick47 = rowsRegistry.IndexOf("wick47");
            idClimb47 = rowsRegistry.IndexOf("climb47");
            idWickTop47 = rowsRegistry.IndexOf("wickTop47");
            idCharger47 = rowsRegistry.IndexOf("charger47");
            idBrawl47 = rowsRegistry.IndexOf("brawl47");
            idWyrm47 = rowsRegistry.IndexOf("wyrm47");
            idLoom47 = rowsRegistry.IndexOf("loom47");
            idCrown47 = rowsRegistry.IndexOf("crown47");
            idPulse47 = rowsRegistry.IndexOf("pulse47");
            idOverheal47 = rowsRegistry.IndexOf("overheal47");
            N47WStartRun(); // builder W
            N47BStartRun(); // builder B
            N47LStartRun(); // builder L
            N47FStartRun(); // builder F
        }

        partial void Night47Step(double dt)
        {
            N47WStep(dt); // builder W
            N47BStep(dt); // builder B
            N47LStep(dt); // builder L
            N47FStep(dt); // builder F
        }

        /// <summary>Once a run, right after N46Campaign (the campaign is made): the builders set their Campaign24 delegates here, only for rows
        /// that are on (a null delegate is NIGHT46's rule).</summary>
        partial void N47Campaign()
        {
            N47WCampaign();
            N47BCampaign();
            N47LCampaign();
            N47FCampaign();
        }

        partial void N47Campaign(); // the declaration (the call: SurvivalSim.Campaign24.cs)
        partial void N47WStartRun(); // W: reset every per-run field here (rule 6); numbers from design.n47.<row>.numbers
        partial void N47WStep(double dt);
        partial void N47WCampaign();
        partial void N47BStartRun(); // B
        partial void N47BStep(double dt);
        partial void N47BCampaign();
        partial void N47LStartRun(); // L
        partial void N47LStep(double dt);
        partial void N47LCampaign();
        partial void N47FStartRun(); // F
        partial void N47FStep(double dt);
        partial void N47FCampaign();
    }
}
