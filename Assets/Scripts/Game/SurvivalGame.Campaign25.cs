// NIGHT25 (builder F): the CAMPAIGN RESULTS screen's game half (UiCampaignResults25.cs is the screen). After layerClear the run ends on RESULTS; CONTINUE loads the
// NEXT level of the curriculum the way GoCampaign loads the first (PendingMapId + PendingMode, the scene reloads and boots in CAMPAIGN on that level). With no next
// level (the last of the curriculum, or its map is not in design.maps.list yet) the primary button is BACK TO TITLE and the screen says CAMPAIGN COMPLETE.
// A campaign run never writes survival records (the sim holds noRecords for it: SurvivalSim.Campaign24.cs), so nothing here saves a score.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        /// <summary>The scene reloaded by CONTINUE should start the level's run at once (the lead's init reads and clears this: `if (AutoStartCampaign25) { AutoStartCampaign25 = false; StartRun(); }`).</summary>
        public static bool AutoStartCampaign25;

        /// <summary>The curriculum level after the one just cleared, when it has a map; else null (the campaign is complete).</summary>
        public string CampaignNextLevel25()
        {
            string current = sim.campaign != null ? sim.campaign.Level.id : MapId;
            N25Blocks n25 = N25Blocks.ForceOff ? N25Blocks.Off : N25Blocks.From(cfg);
            string next = Curriculum25.Next(cfg, n25, current);
            return next != null && mapList.FindIndex(m => m.campaignOnly && m.id == next) >= 0 ? next : null;
        }

        /// <summary>CONTINUE on the CAMPAIGN RESULTS: the next level (the scene reloads), or the title when there is none. Returns the level id it went to (null: the title).</summary>
        public string ContinueCampaign()
        {
            string next = CampaignNextLevel25();
            bool shopOpened = false;
            N33ShopBeforeContinue(next, ref shopOpened); // NIGHT33 P: with design.n33.shop33 on, the SHOP opens first (its CONTINUE calls back here); nothing without a body (SurvivalGame.Shop33.cs)
            if (shopOpened) return next;
            if (next == null)
            {
                ToTitle();
                return null;
            }
            PlayerPrefs.SetString(CampaignNextPref, next);
            PendingMapId = next;
            PendingMode = RunSetup.Campaign;
            AutoStartCampaign25 = true;
            Debug.Log($"[skygrind] campaign: continue {MapId} -> {next} (the scene reloads)");
            if (!Application.isPlaying || SkipSceneReload25) return next; // a reel / test reads PendingMapId and does not reload
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return next;
        }

        /// <summary>The reel `campresults` sets this so CONTINUE records where it would go without reloading the scene under the reel.</summary>
        public static bool SkipSceneReload25;

        /// <summary>NIGHT33 P: the shop between levels. Sets `opened` when it showed the SHOP instead of continuing (the shop's own CONTINUE
        /// calls ContinueCampaign again once it has been visited for this level).</summary>
        partial void N33ShopBeforeContinue(string next, ref bool opened);
    }
}
