// NIGHT41 builder T2 (INVENTORY A9): the reels' DOORFREE silences the room banners. DOORFREE (Campaign24.SetDoorFree29) opens every zone in one
// step; each emits "gateOpen" and Ui.cs's handler queues "<name> OPEN" (up to 8, each zones.banner.time), so a boss reel that turned DOORFREE on
// showed "4.2 FIRST GAP OPEN" over THE LOOM and "2.2 THE SHIELD OPEN" over THE ICEBREAKER. A reel calls ClearBanners41 right after it turns
// DOORFREE on: the queue, the banner on screen and the HUD33 message slot are emptied. Reel-only (FeelReel.DoorFree41); the game never calls it.
namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        /// <summary>Empty the banner queue, end the banner on screen and clear the HUD33 message slot (the reels' DOORFREE only).</summary>
        public void ClearBanners41()
        {
            bannerQueue.Clear();
            bannerLeft = 0;
            h33Msg.Clear();
        }

        /// <summary>The banner on screen now and the HUD33 message slot's line ("" none; "a | b" both): the reels' A9 check.</summary>
        public string Banner41Now
        {
            get
            {
                string a = bannerLeft > 0 && banner != null ? banner.text : null, b = h33Msg.Text;
                return a != null && b != null && a != b ? a + " | " + b : a ?? b ?? "";
            }
        }
    }
}
