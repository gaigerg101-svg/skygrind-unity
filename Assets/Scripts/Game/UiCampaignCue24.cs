// NIGHT24 (builder F, leftovers 4 and the room banner): two small HUD widgets of THE CAMPAIGN. RENDER SIDE ONLY: they listen to the bus and read
// Campaign24's state; the sim, the tape and the fingerprint are untouched. Both exist only in a campaign run (sim.campaignOn, so --off24 shows neither).
//   DIRECTED CUE   once a room is cleared (Campaign24.Phase.Cleared; roomClear opens the door the same step: gateOpen) a cue under the round banner:
//                  "DOOR OPEN" and the way to the room's exit, "34 m · TURN LEFT" (the words and the bearing maths of NIGHT22's DIRECTED STEPS:
//                  Directed22.BearingDeg / SideOf). It hides when the player is within directed.hideWithin m of the exit or the next room is entered.
//   ROOM BANNER    when a LIMIT room starts (limitOn, and again on a retry): "ROOM 2 · NO DASH" for directed.bannerSeconds, big at the top.
// Cyan only (player-usable), never red; text S32 / S24 (18 px at 1080p is the floor); both enter with HudEntrance18 (no pop-in).
// Numbers: design.n24.campaign.directed (hideWithin, bannerSeconds). Wiring: Ui.N24.cs BuildN24Hud / RenderN24Hud.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform cue24Root, ban24Root;
        CanvasGroup cue24Group, ban24Group;
        TextMeshProUGUI cue24Head, cue24Body, ban24Text;
        double cue24Enter = -1, ban24Enter = -1, ban24Left;
        int cue24Key = -1;
        double cue24Hide = 6, ban24Seconds = 3;
        double cue25Waited; // NIGHT25: seconds the cue was held back by a banner

        /// <summary>For the reel's scenarios: what the widgets show now.</summary>
        public static bool CampaignCue24Built;
        public static string Cue24TextNow = "", Banner24TextNow = "";
        public static float Banner24MinFontPx;

        void BuildCampaignCue24()
        {
            CampaignCue24Built = true;
            GameConfig cfg = game.cfg;
            if (cfg.Has("design.n24.campaign.directed.hideWithin")) cue24Hide = cfg.Num("design.n24.campaign.directed.hideWithin");
            if (cfg.Has("design.n24.campaign.directed.bannerSeconds")) ban24Seconds = cfg.Num("design.n24.campaign.directed.bannerSeconds");
            float px = UiTheme.Px(UiTheme.Size.S24), big = UiTheme.Px(UiTheme.Size.S32);
            cue24Root = Hud17Rect(values, "Door cue 24", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, UnityTuning.Cue24Y), new Vector2(UnityTuning.Cue24W, UnityTuning.Cue24LineH * 2));
            cue24Group = cue24Root.gameObject.AddComponent<CanvasGroup>();
            cue24Group.blocksRaycasts = cue24Group.interactable = false;
            cue24Group.alpha = 0;
            cue24Head = Hud17Text(cue24Root, "Head", "", px, UiTheme.Cyan, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(UnityTuning.Cue24W, UnityTuning.Cue24LineH), false);
            cue24Body = Hud17Text(cue24Root, "Way", "", px, UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -UnityTuning.Cue24LineH), new Vector2(UnityTuning.Cue24W, UnityTuning.Cue24LineH), false);
            cue24Root.gameObject.SetActive(false);

            ban24Root = Hud17Rect(values, "Room banner 24", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, UnityTuning.RoomBanner24Y), new Vector2(UnityTuning.RoomBanner24W, UnityTuning.RoomBanner24H));
            ban24Group = ban24Root.gameObject.AddComponent<CanvasGroup>();
            ban24Group.blocksRaycasts = ban24Group.interactable = false;
            ban24Group.alpha = 0;
            ban24Text = Hud17Text(ban24Root, "Text", "", big, UiTheme.White, UiTheme.Display, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(UnityTuning.RoomBanner24W, UnityTuning.RoomBanner24H), false);
            Banner24MinFontPx = Mathf.Min(big, px);
            ban24Root.gameObject.SetActive(false);

            var bus = game.events;
            bus.On<RoomEvent>("limitOn", e => ShowRoomBanner24(e.room, e.name));
            bus.On<RoomEvent>("roomRetry", e =>
            {
                if (e.limits != 0) ShowRoomBanner24(e.room, Limit24.Banner(e.limits));
            });
            game.OnNewRun += () =>
            {
                cue24Key = -1;
                ban24Left = 0;
            };
        }

        void ShowRoomBanner24(int room, string limit)
        {
            if (ban24Text == null || string.IsNullOrEmpty(limit)) return;
            ban24Text.SetText("ROOM " + (room + 1) + " · " + limit); // on a room start only
            ban24Left = ban24Seconds;
            ban24Enter = 0;
            ban24Root.gameObject.SetActive(true);
            Banner24TextNow = ban24Text.text;
            Post33(Banner24TextNow, ban24Seconds); // NIGHT33 U (design.n33.hud33): the ROOM n banner is a room-title line in the one message slot
            FeelProbe.Event("roomBanner");
        }

        void RenderCampaignCue24(double frameDt)
        {
            if (cue24Root == null) return;
            SurvivalSim sim = game.sim;
            Campaign24 c = sim != null && sim.campaignOn ? sim.campaign : null;
            bool playing = game.flow == Flow.Playing;

            // --- the DOOR cue --------------------------------------------------------------------------------------------------------------
            bool live = false;
            double dist = 0;
            Directed22Side side = Directed22Side.Ahead;
            if (c != null && playing && c.phase == Campaign24.Phase.Cleared && !c.finished && c.Room != null)
            {
                Vec3 exit = c.Room.exit;
                Vec3 feet = game.Feet;
                dist = Math.Sqrt((exit.x - feet.x) * (exit.x - feet.x) + (exit.z - feet.z) * (exit.z - feet.z));
                live = dist > cue24Hide;
                // NIGHT25: it waits for the room banner / title (they share the top centre), then enters; CueWait25: never for ever (a stuck banner timer)
                if (live)
                {
                    bool held = bannerLeft > 0 || ban24Left > 0;
                    if (held && !cue24Root.gameObject.activeSelf) cue25Waited += frameDt;
                    live = CueWait25.MayShow(cue24Root.gameObject.activeSelf, bannerLeft, ban24Left, cue25Waited, UnityTuning.Cue25MaxWait);
                }
                else cue25Waited = 0;
                side = Directed22.SideOf(Directed22.BearingDeg(feet, sim.input.Yaw, exit));
            }
            if (cue24Root.gameObject.activeSelf != live)
            {
                cue24Root.gameObject.SetActive(live);
                if (live) cue24Enter = 0; // M9: it enters, never whole on its first frame
                cue24Key = -1;
            }
            if (live)
            {
                int key = (int)Math.Min(4000, dist) * 4 + (int)side;
                if (key != cue24Key)
                {
                    cue24Key = key;
                    cue24Head.SetText("DOOR OPEN");
                    cue24Body.SetText(((int)dist) + " m · " + SideWord22(side)); // on change only
                    Cue24TextNow = cue24Head.text + " | " + cue24Body.text;
                }
                if (cue24Enter >= 0)
                {
                    cue24Enter = HudEntrance18.Step(cue24Enter, frameDt);
                    bool done = HudEntrance18.Done(cue24Enter);
                    float es = done || game.settings.View.reducedMotion ? 1f : HudEntrance18.Scale(cue24Enter);
                    cue24Root.localScale = new Vector3(es, es, 1);
                    cue24Group.alpha = done ? 1f : HudEntrance18.Alpha(cue24Enter);
                    if (done) cue24Enter = -1;
                }
                else if (cue24Group.alpha != 1f) cue24Group.alpha = 1f;
            }
            else Cue24TextNow = "";

            // --- the ROOM banner -----------------------------------------------------------------------------------------------------------------
            if (ban24Left > 0 && playing)
            {
                ban24Left -= frameDt;
                if (ban24Enter >= 0)
                {
                    ban24Enter = HudEntrance18.Step(ban24Enter, frameDt);
                    bool done = HudEntrance18.Done(ban24Enter);
                    float es = done || game.settings.View.reducedMotion ? 1f : HudEntrance18.Scale(ban24Enter);
                    ban24Root.localScale = new Vector3(es, es, 1);
                    ban24Group.alpha = done ? 1f : HudEntrance18.Alpha(ban24Enter);
                    if (done) ban24Enter = -1;
                }
                else ban24Group.alpha = ban24Left < 0.4 ? (float)(ban24Left / 0.4) : 1f;
                if (ban24Left <= 0)
                {
                    ban24Root.gameObject.SetActive(false);
                    Banner24TextNow = "";
                }
            }
            else if (ban24Left <= 0 && ban24Root.gameObject.activeSelf)
            {
                ban24Root.gameObject.SetActive(false);
                Banner24TextNow = "";
            }
        }
    }
}
