// NIGHT33 builder P: the Player scenario of the shop row (FeelReel.N27.cs PhaseN33P "shop33"; the body is the partial N33ShopBody). Not run by its author
// (no Player in the worktree): the lead runs it. Boot: -skygrindMap n32-foundry -skygrindReelOnly shop33 (any campaign level with a next level works).
//   setup    the shop forced on (SurvivalGame.Shop33ReelForce, the sim's n33Override with shop33 on), the wallet in memory only (Shop33NoSave: the real profile
//            is never written) and emptied: wallet 0, nothing owned, no level best
//   play     a CAMPAIGN run, the bot plays ~20 s; then the level's earn is topped up to the cheapest price + 400 (economy.Grant "reel": the bot's 20 s earn
//            is under a price) and the level is cleared the way Campaign24 clears it (layerClear on the bus, campaign.finished): the run ends on RESULTS
//   results  Shop33.LastBanked > 0 and the wallet = LastBanked (U's RESULTS line reads these)                          shot shop33-results
//   shop     the pad's A on the RESULTS' focus (CONTINUE; its click as the fallback) opens the SHOP, not the next level   shot shop33-open
//   look     on shop33-open and shop33-bought: the RESULTS hidden (Ui.Shop33ResultsDrawn false), 0 overlapping text pairs (glyph boxes, UiShop33.OverlapCount),
//            no text under 16 px; the line prints the max overlap count over the shop frames
//   buy      by the virtual pad only: d-pad right x2 (to the WILDCARD column: PLATING, +20 MAX HEALTH), A: bought, equipped, the wallet down by its price
//                                                                                                                      shot shop33-bought
//   continue B (the focus to CONTINUE), A: PendingMapId = the next level (the scene reload skipped: SkipSceneReload25), the shop closed
//   next     a new CAMPAIGN run on the booted level: sim.shop33Owned carries PLATING and the health is 120 (max 100 + 20)  shot shop33-next
//   PASS: every step above; the real profile's wallet, upgrades and level bests unchanged. The RED half: -skygrindOff33 (CONTINUE goes straight to the next
//   level, no shop, health 100).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N33ShopBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "shop33") body = Shop33Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Shop33Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            var fails = new List<string>();
            var lines = new List<string>();
            bool off = N33Blocks.ForceOff;

            // the real profile's shop fields, to prove they are never touched
            int realWallet = g.profile != null ? g.profile.bank33 : -1, realCode = g.profile != null ? g.profile.shop33 : -1, realBests = g.profile != null ? g.profile.banked33.Count : -1;

            SurvivalGame.Shop33NoSave = true;
            SurvivalGame.Shop33ReelForce = true;
            SurvivalGame.SkipSceneReload25 = true;
            N33Blocks? n33Was = g.sim.n33Override;
            N33Blocks n33 = off ? N33Blocks.Off : N33Blocks.From(g.cfg);
            if (!off) n33.shop33 = true;
            g.sim.n33Override = n33;
            offs.Add(() =>
            {
                SurvivalGame.Shop33NoSave = false;
                SurvivalGame.Shop33ReelForce = false;
                SurvivalGame.SkipSceneReload25 = false;
                SurvivalGame.PendingMapId = null;
                SurvivalGame.PendingMode = null;
                SurvivalGame.AutoStartCampaign25 = false;
                g.sim.n33Override = n33Was;
                if (g.Shop33Ui != null) g.Shop33Ui.Close();
            });
            Profile store = g.Shop33Store;
            store.bank33 = 0;
            store.shop33 = 0;
            store.banked33.Clear();
            Shop33.LastBanked = Shop33.LastEarned = Shop33.Wallet = -1;

            // ORDER- AND FOCUS-INDEPENDENCE (n33-r6: run after hud33 + feel33 in one Player, the virtual pad's presses never arrived, here and in feel33,
            // while n33-r5 ran the shop alone and passed). The Input System's default backgroundBehavior (ResetAndDisableNonBackgroundDevices; the project
            // has no input settings asset) disables a non-background device such as a virtual Gamepad while the Player window is not focused, and a device
            // added then starts disabled; the reel Player keeps running unfocused (runInBackground), so a focus change mid-reel silences the pad. The reel
            // takes IgnoreFocus for its length and enables its own pad before every press; it also starts from a clean front: no meta screen (hud33's
            // front pass ends on TITLE after the level select), the EventSystem navigating with nothing selected, time running, no shop open.
            bool focused0 = Application.isFocused;
            InputSettings.BackgroundBehavior bgWas = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            offs.Add(() => InputSystem.settings.backgroundBehavior = bgWas);
            float tsWas = Time.timeScale;
            if (tsWas <= 0f) Time.timeScale = 1f;
            g.UiView?.CloseMeta();
            if (g.Shop33Ui != null) g.Shop33Ui.Close();
            if (EventSystem.current != null)
            {
                EventSystem.current.sendNavigationEvents = true;
                EventSystem.current.SetSelectedGameObject(null);
            }
            Pad30 pad = NewPad30(offs);
            bool padWasOff = !pad.pad.enabled;
            int reEnabled = 0;
            void EnsurePad()
            {
                if (pad.pad.enabled) return;
                InputSystem.EnableDevice(pad.pad);
                reEnabled++;
            }
            EnsurePad();
            pad.pad.MakeCurrent();
            lines.Add($"input: focused {focused0} -> {Application.isFocused}, backgroundBehavior {bgWas} -> IgnoreFocus, timeScale {tsWas.ToString("0.##", ci)}, pads {Gamepad.all.Count}, the reel pad {(padWasOff ? "added DISABLED (enabled)" : "added enabled")}");

            // play ~20 s, then the level's clear the way Campaign24 does it
            AutopilotInput ap = N24StartCampaign(g, offs);
            string pc = Preconditions(g, g.Feet);
            if (!g.sim.campaignOn) { res[0] = pc + " | SKIPPED (RED): the run did not start as a CAMPAIGN"; yield break; }
            string next = g.CampaignNextLevel25();
            if (next == null) { res[0] = pc + $" | SKIPPED (RED): '{g.MapId}' has no next level (boot a level the campaign continues from)"; yield break; }
            yield return Frames(20 + pre);
            float wall = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - wall < 20f && g.flow == Flow.Playing)
            {
                AutopilotInput.TakeDraft(g.sim);
                yield return null;
            }
            Shop33Catalogue cat = g.Shop33Cat;
            int plating = cat.Find("maxHealth").index;
            int cheapest = int.MaxValue;
            foreach (Shop33Item it in cat.items) cheapest = Math.Min(cheapest, it.price);
            double earned0 = g.sim.economy.earned;
            if (earned0 < cheapest + 400) g.sim.economy.Grant(cheapest + 400 - earned0, "reel");
            double earned = g.sim.economy.earned;
            if (g.flow == Flow.Down || g.flow == Flow.Playing)
            {
                g.sim.events.Emit("layerClear", new RoomEvent { room = g.sim.campaign != null ? g.sim.campaign.room : 0, id = g.sim.campaignLevel.id, name = g.sim.campaignLevel.name });
                if (g.sim.campaign != null) g.sim.campaign.finished = true;
            }
            int n = 0;
            while (g.flow != Flow.Results && n++ < Ms(5000)) yield return null;
            if (g.flow != Flow.Results) { res[0] = pc + $" | FAIL (RED): no RESULTS after the clear (flow {g.flow})"; yield break; }
            Start(g, "shop33", "ui", dir);
            FeelProbe.MarkInput();
            for (int w = 0; w < Ms(4000) && Ui.CampRes25FocusedAt < 0; w++) yield return null; // the RESULTS' buttons and CONTINUE's focus
            yield return Frames(Ms(300));
            lines.Add($"RESULTS: earned {earned.ToString("0", ci)} (bot {earned0.ToString("0", ci)} + reel top-up), LastEarned {Shop33.LastEarned}, LastBanked {Shop33.LastBanked}, wallet {g.Shop33Wallet}");
            if (!off && (Shop33.LastBanked <= 0 || g.Shop33Wallet != Shop33.LastBanked)) fails.Add($"nothing banked (LastBanked {Shop33.LastBanked}, wallet {g.Shop33Wallet})");
            yield return Shot30(dir, "shop33-results");

            // CONTINUE on the RESULTS: the pad's A on the focus (the click as the fallback)
            SurvivalGame.PendingMapId = null;
            // the pad's A only when CONTINUE holds the focus (an A on RETRY LEVEL would restart the level); else CONTINUE's click
            string resFocus = Ui.CampRes25FocusNow;
            bool viaPad = false;
            if (resFocus == Ui.CampRes25PrimaryNow && resFocus.Length > 0)
            {
                EnsurePad();
                yield return Tap30(pad, GamepadButton.South);
                yield return Frames(Ms(200));
                viaPad = (g.Shop33Ui != null && g.Shop33Ui.IsOpen) || SurvivalGame.PendingMapId != null;
            }
            if (!viaPad && g.UiView != null) g.UiView.CampRes25PressPrimary();
            yield return Frames(Ms(400));
            bool open = g.Shop33Ui != null && g.Shop33Ui.IsOpen;
            lines.Add($"CONTINUE ({(viaPad ? "pad A" : "click fallback")}; RESULTS focus '{resFocus}', pad enabled {pad.pad.enabled}, focused {Application.isFocused}): shop {(open ? "OPEN" : "closed")}, PendingMapId '{SurvivalGame.PendingMapId}'");
            if (off)
            {
                // the RED half: no shop, CONTINUE went on
                if (open) fails.Add("-skygrindOff33: the shop opened");
                if (SurvivalGame.PendingMapId != next) fails.Add($"-skygrindOff33: CONTINUE went to '{SurvivalGame.PendingMapId}', not '{next}'");
                yield return Stop();
                res[0] = pc + " | " + string.Join(" | ", lines) + " | " + (fails.Count == 0 ? "PASS (RED half: -skygrindOff33, no shop)" : "FAIL (RED): " + string.Join("; ", fails));
                yield break;
            }
            if (!open) { yield return Stop(); res[0] = pc + " | " + string.Join(" | ", lines) + " | FAIL (RED): CONTINUE did not open the SHOP"; yield break; }
            if (SurvivalGame.PendingMapId != null) fails.Add($"CONTINUE went on to '{SurvivalGame.PendingMapId}' with the shop open");
            UiShop33 shop = g.Shop33Ui;
            yield return Frames(Ms(400)); // the fade and the open guard
            yield return Shot30(dir, "shop33-open");
            int maxOverlap = 0;
            void Look(string frame)
            {
                int ov = shop.OverlapCount(out string worst, out float minPx);
                bool drawn = g.UiView != null && g.UiView.Shop33ResultsDrawn;
                maxOverlap = Math.Max(maxOverlap, ov);
                lines.Add($"{frame}: text overlaps {ov}{(ov > 0 ? " (worst " + worst + ")" : "")}, smallest text {minPx.ToString("0", ci)} px, RESULTS {(drawn ? "DRAWN under the shop" : "hidden")}");
                if (ov > 0) fails.Add($"{frame}: {ov} overlapping text pair(s), worst {worst}");
                if (minPx < 16f) fails.Add($"{frame}: a text at {minPx.ToString("0", ci)} px (< 16)");
                if (drawn) fails.Add($"{frame}: the RESULTS screen is drawn under the shop");
            }
            Look("shop33-open");
            string focus0 = shop.FocusNow;
            int wallet0 = g.Shop33Wallet;

            // buy PLATING by the pad: d-pad right twice (the WILDCARD column), A. A pad step that does nothing is RED, and the shop's own verb
            // (UiShop33.Move / Confirm / Back: what the pad calls) then does the step so the rest of the flow is still checked.
            var verbs = new List<string>();
            EnsurePad();
            yield return Tap30(pad, GamepadButton.DpadRight);
            yield return Frames(Ms(150));
            EnsurePad();
            yield return Tap30(pad, GamepadButton.DpadRight);
            yield return Frames(Ms(150));
            string focus1 = shop.FocusNow;
            if (focus1 != "maxHealth")
            {
                fails.Add($"pad: d-pad right x2 focused '{focus1}', not maxHealth (pad enabled {pad.pad.enabled}, focused {Application.isFocused})");
                for (int k = 0; k < 4 && shop.FocusNow != "maxHealth"; k++) shop.Move(1, 0);
                verbs.Add("move");
            }
            EnsurePad();
            yield return Tap30(pad, GamepadButton.South);
            yield return Frames(Ms(300));
            if (!Shop33Catalogue.Owns(g.Shop33Code, plating) && shop.FocusNow == "maxHealth")
            {
                fails.Add($"pad: A did not buy PLATING (pad enabled {pad.pad.enabled})");
                shop.Confirm();
                verbs.Add("buy");
            }
            int code = g.Shop33Code, wallet1 = g.Shop33Wallet;
            bool bought = Shop33Catalogue.Active(code, plating);
            lines.Add($"SHOP: focus at open '{focus0}', after d-pad right x2 '{focus1}', A -> message '{shop.MessageNow}', code {code}, wallet {wallet0} -> {wallet1}");
            if (!bought) fails.Add("PLATING not owned and equipped after A");
            else if (wallet0 - wallet1 != cat.items[plating].price) fails.Add($"the wallet fell by {wallet0 - wallet1}, not the price {cat.items[plating].price}");
            yield return Shot30(dir, "shop33-bought");
            Look("shop33-bought");
            lines.Add($"max text overlap count on the shop frames: {maxOverlap}");

            // CONTINUE: B then A
            EnsurePad();
            yield return Tap30(pad, GamepadButton.East);
            yield return Frames(Ms(150));
            string focus2 = shop.FocusNow;
            if (focus2 != "continue")
            {
                fails.Add($"pad: B focused '{focus2}', not continue");
                shop.Back();
                verbs.Add("back");
            }
            EnsurePad();
            yield return Tap30(pad, GamepadButton.South);
            yield return Frames(Ms(300));
            if (shop.IsOpen && SurvivalGame.PendingMapId == null)
            {
                fails.Add("pad: A on CONTINUE did nothing");
                shop.Confirm();
                verbs.Add("continue");
                yield return Frames(Ms(300));
            }
            lines.Add($"CONTINUE: B focus '{focus2}', A -> PendingMapId '{SurvivalGame.PendingMapId}' (next '{next}'), shop {(shop.IsOpen ? "still OPEN" : "closed")}; the reel pad re-enabled {reEnabled} time(s); shop verbs called directly (pad steps that did nothing) [{string.Join(", ", verbs)}]");
            if (SurvivalGame.PendingMapId != next) fails.Add($"the shop's CONTINUE went to '{SurvivalGame.PendingMapId}', not '{next}'");
            if (shop.IsOpen) fails.Add("the shop stayed open after its CONTINUE");
            yield return Stop();

            // the next run (the same level: the reel skips the scene reload) starts with the upgrade
            SurvivalGame.PendingMapId = null;
            SurvivalGame.AutoStartCampaign25 = false;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(Ms(500));
            bool carried = Shop33Catalogue.Active(g.sim.shop33Owned, plating);
            lines.Add($"NEXT RUN: shop33Owned {g.sim.shop33Owned} (PLATING {(carried ? "on" : "OFF")}), health {g.health.hp.ToString("0", ci)} / {g.health.max.ToString("0", ci)}");
            if (!carried) fails.Add("the next run did not carry PLATING (is the n33p-startrun line in SurvivalGame.StartRun?)");
            if (g.health.max != cfg0(g) + cat.items[plating].value) fails.Add($"max health {g.health.max}, want {cfg0(g) + cat.items[plating].value}");
            yield return Shot30(dir, "shop33-next");

            // the real profile untouched
            if (g.profile != null && (g.profile.bank33 != realWallet || g.profile.shop33 != realCode || g.profile.banked33.Count != realBests))
                fails.Add($"the real profile changed: wallet {realWallet} -> {g.profile.bank33}, code {realCode} -> {g.profile.shop33}, bests {realBests} -> {g.profile.banked33.Count}");
            res[0] = pc + $" | {FeelProbe.FrameCount} frames | " + string.Join(" | ", lines) + " | " + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }

        static double cfg0(SurvivalGame g) => g.cfg.Num("health.max");
    }
}
