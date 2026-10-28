// NIGHT33 P (design.n33.shop33; the lead's round 3, 11:59: the RESULTS showed through the SHOP, "N33-FOUNDRY CLEARED", the room list and the buttons
// ghosted across its columns): while the SHOP is open the RESULTS screen is HIDDEN (its root inactive), not only cut off from input; it comes back
// when the shop closes while the flow is still RESULTS. Ui.Render toggles `results` only on a flow change, so nothing turns it back on under the shop.
namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        bool shop33HidResults;

        /// <summary>The SHOP opens (on) or closes (off): hide the RESULTS screen under it, or bring it back if the run is still on RESULTS.</summary>
        public void Shop33Cover(bool on)
        {
            if (results == null) return;
            if (on)
            {
                if (results.activeSelf)
                {
                    shop33HidResults = true;
                    results.SetActive(false);
                }
                return;
            }
            if (!shop33HidResults) return;
            shop33HidResults = false;
            if (game.UiFlow == Flow.Results) results.SetActive(true);
        }

        /// <summary>Is the RESULTS screen drawn now (the reel's check: false while the shop is open)?</summary>
        public bool Shop33ResultsDrawn => results != null && results.activeInHierarchy;
    }
}
