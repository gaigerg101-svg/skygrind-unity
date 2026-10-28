// NIGHT26 (the shared commit, the lead owns this file): the hooks the NIGHT26 world views hang on. A builder implements the
// declaration-only partial in its own file and names the one call in its report. Nothing runs without a body.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        /// <summary>Once at game init, after the NIGHT25 views.</summary>
        void N26InitViews() => N26InitViewsHook();

        /// <summary>Every render frame beside the other world views (worldDt: the slowed world frame time; alpha: the sim's render interpolation).</summary>
        Campaign24 unlocks26For; // NIGHT26: the campaign object the unlocks were applied to (one Apply per run)

        void N26RenderViews(double worldDt, double alpha)
        {
            if (sim.campaignOn && sim.campaign != null && !ReferenceEquals(sim.campaign, unlocks26For))
            {
                unlocks26For = sim.campaign; // NIGHT26 (builder L): weapons earned by the levels cleared; the campaign's loadout is the owned set
                CampaignUnlocks26.Apply(cfg, sim.campaign, sim.loadout, mapList, profile);
            }
            N26RenderViewsHook(worldDt, alpha);
        }

        partial void N26InitViewsHook();
        partial void N26RenderViewsHook(double worldDt, double alpha);
    
        /// <summary>NIGHT26 (GG: "the campaign ... should be the default"): a normal boot (no -skygrind automation arg, no pending reload) starts on the CAMPAIGN
        /// mode and a campaign level (the saved next level, else the curriculum's first), so TITLE shows CAMPAIGN and PLAY opens the level select.</summary>
        void N26DefaultCampaign(GameConfig config)
        {
            if (PendingMode != null || PendingMapId != null) return;
            foreach (string a in System.Environment.GetCommandLineArgs())
                if (N29Fixes.IsAutomationArg(a)) return; // NIGHT29: -skygrindOff29 / -skygrindLevelSelect29 do not count
            if (N26Blocks.ForceOff || !N26Blocks.From(config).levelSelect || !N24Blocks.From(config).campaign) return;
            N25Blocks n25 = N25Blocks.ForceOff ? N25Blocks.Off : N25Blocks.From(config);
            string saved = PlayerPrefs.GetString("skygrind.campaign.next", string.Empty);
            var order = Curriculum25.Order(config, n25);
            string id = !string.IsNullOrEmpty(saved) && order.Contains(saved) ? saved : Curriculum25.Start(config, n25);
            if (!mapList.Exists(m => m.id == id && m.campaignOnly)) return;
            PendingMode = RunSetup.Campaign;
            PendingMapId = id;
        }
}
}
