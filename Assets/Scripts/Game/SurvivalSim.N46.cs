// NIGHT46 (the shared commit; Tools/night/n46-shared.cjs): the night's sim rows (design.n46.*, sim: true; Core/NightRows.cs,
// docs/NIGHT-ROWS.md section 2), each builder in its own partial file:
//   F1 (SurvivalSim.N46.Flow.cs): retry46, overlap46, teach46, reveal46 -> N46FlowStartRun / N46FlowStep / N46FlowCampaign
//   B1 (SurvivalSim.N46.Wyrm.cs): wyrm46, walkin46                   -> N46WyrmStartRun / N46WyrmStep / N46WyrmCampaign
// Off: -skygrindOff46 / --off46 (or any older switch): every id reads off. The level rows (foundry46 ... spire46) and the view rows (shift46,
// feel46) are data only: no hook here (Curriculum25 swaps the levels; a view asks NightRows.ViewOn).
// Campaign24 (Core) is never edited by a builder: its NIGHT46 hooks are null delegates (retryWave46, waveRelease46, teachHold46; null = NIGHT45's
// rooms bit for bit), set in N46FlowCampaign / N46WyrmCampaign, which run once a run right after the campaign is made
// (SurvivalSim.Campaign24.cs, N24CampaignStep). walkIn46 (false = NIGHT45) joins Boss36WalkIn (ICampaignWorld.BossWalkIn: no boss Respawn).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        // one id per row in rowsRegistry, resolved once a run (-1: this config lacks the row; rows.On(-1) is false, the older game)
        internal int idRetry46 = -1, idOverlap46 = -1, idTeach46 = -1, idReveal46 = -1; // F1
        internal int idWyrm46 = -1, idWalkin46 = -1; // B1

        /// <summary>NIGHT46 B1 (walkin46): the boss room keeps the player where they walked in (Boss36WalkIn reads it). Reset every run here;
        /// B1 sets it (N46WyrmStartRun / N46WyrmCampaign) only while rows.On(idWalkin46).</summary>
        internal bool walkIn46;

        partial void Night46StartRun()
        {
            idRetry46 = rowsRegistry.IndexOf("retry46");
            idOverlap46 = rowsRegistry.IndexOf("overlap46");
            idTeach46 = rowsRegistry.IndexOf("teach46");
            idReveal46 = rowsRegistry.IndexOf("reveal46");
            idWyrm46 = rowsRegistry.IndexOf("wyrm46");
            idWalkin46 = rowsRegistry.IndexOf("walkin46");
            walkIn46 = false;
            N46FlowStartRun(); // builder F1
            N46WyrmStartRun(); // builder B1
        }

        partial void Night46Step(double dt)
        {
            N46FlowStep(dt); // builder F1
            N46WyrmStep(dt); // builder B1
        }

        /// <summary>Once a run, right after N24CampaignStep made the campaign (campaign is not null): the builders set its NIGHT46 delegates
        /// here, only for rows that are on (a null delegate is NIGHT45's rule).</summary>
        partial void N46Campaign()
        {
            N46FlowCampaign(); // builder F1: campaign.retryWave46 / waveRelease46 / teachHold46
            N46WyrmCampaign(); // builder B1
        }

        partial void N46Campaign(); // the declaration (the call: SurvivalSim.Campaign24.cs)
        partial void N46FlowStartRun(); // F1: reset every per-run field here (rule 6); numbers from design.n46.<row>.numbers
        partial void N46FlowStep(double dt); // F1: rows.On(idRetry46) ... per step (an array read, no allocation)
        partial void N46FlowCampaign(); // F1
        partial void N46WyrmStartRun(); // B1: reset every per-run field here (rule 6); numbers from design.n46.wyrm46.numbers / walkin46.numbers
        partial void N46WyrmStep(double dt); // B1: rows.On(idWyrm46), rows.On(idWalkin46)
        partial void N46WyrmCampaign(); // B1
    }
}
