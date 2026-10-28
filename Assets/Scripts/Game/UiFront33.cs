// NIGHT33 builder U (design.n33.hud33, the same view row: no sim bit): the FRONT UI in the HUD33 language (DESIGN33 U "Front UI"; DECIDE33 D-1). With the row on,
// once at build (the end of the Ui constructor), the menu screens drop their fills and take the slash:
//   - every menu item (TITLE, PAUSED, RESULTS, the LEVEL SELECT rows, CONTINUE / RETRY LEVEL): no plate fill (the plate stays as the click target at alpha 0);
//     the focused item's label turns cyan (player-usable) and two cyan slash ticks stand left of it (the pad's focus mark), the rest are bone; the old accent
//     stroke goes (the ticks replace it). The focus still slides along the slant (MenuStepItems' nudge).
//   - the screens' INK slabs and the wordmark's slab and bar go; the world stays darkened by each screen's sheet (a full-screen shade, not a panel). The
//     wordmark is bone with a shadow-coloured hero offset (cyan is for what the player can use).
//   - LEVEL SELECT: a locked row is its text dimmed (no veil plate); the focused row's cells turn cyan, not dark on a cyan plate.
//   - RESULTS: the survival tally's INK card and its accent go (its texts keep their own shadows); the CAMPAIGN results show POINTS BANKED (and the WALLET) from
//     builder P's Shop33.LastBanked / Shop33.Wallet (the line hides while LastBanked is -1: the shop row off or no level banked yet).
// Off: the row off or -skygrindOff33: nothing changes (NIGHT32's menus). Numbers: UnityTuning.Hud33.cs.
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        bool front33;
        readonly Dictionary<MenuItemView, Image[]> front33Ticks = new Dictionary<MenuItemView, Image[]>();
        UiStyle33.Text33 front33Banked;
        int front33BankedShown = int.MinValue, front33WalletShown = int.MinValue;

        /// <summary>For the reels: the front screens are in the HUD33 language; the POINTS BANKED line now ("" hidden); the focused item's name.</summary>
        public static bool Front33On;
        public static string Front33BankedNow = "", Front33FocusNow = "";

        /// <summary>From the Ui constructor's end (every screen is built by then).</summary>
        void BuildFront33()
        {
            front33 = Front33On = Hud33On;
            if (!front33) return;
            foreach (MenuSlabView s in menuSlabs)
            {
                UiShape sh = s.rt != null ? s.rt.GetComponent<UiShape>() : null;
                if (sh != null) sh.enabled = false;
            }
            HideGraphic33(title.transform.Find("Wordmark slab"));
            HideGraphic33(title.transform.Find("Wordmark bar"));
            if (wordmark != null) wordmark.color = UiStyle33.Bone;
            if (wordmarkBack != null) wordmarkBack.color = UiStyle33.Shadow;
            foreach (MenuItemView it in menuItems) Front33Item(it);
            // TITLE's run setup card (MODE / HEAT / MAP): no plate (the lead's n33-r5 title frame); its texts take the theme's hard shadow
            if (setupCardPlate != null) setupCardPlate.color = UiTheme.A(setupCardPlate.color, 0f);
            if (setupRow != null) Shadow33(setupRow);
            if (bestRow != null) Shadow33(bestRow.transform);
            foreach (LevelRow26 r in levelRows26)
                if (r.veil != null) r.veil.enabled = false; // RefreshLevelSelect26 still toggles its object; drawn, it is nothing
            if (tallyCardG != null)
                foreach (UiShape sh in tallyCardG.GetComponentsInChildren<UiShape>(true))
                    if (sh.name == "Plate" || sh.name == "Accent") sh.enabled = false;
            foreach (UiShape sh in new[] { tallyHeatPlate, tallyStampPlate, tallyGradePlate })
                if (sh != null) sh.enabled = false;
            // the campaign results' POINTS BANKED line, over the head (it fades with the summary)
            if (campRes25SumG != null)
            {
                float y = UnityTuning.Front33BankedY;
                front33Banked = UiStyle33.Label(campRes25SumG.transform, "Points banked", UnityTuning.Hud33Name, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.Left,
                    new Vector2(0.5f, 0.5f), new Vector2(0, 0.5f), new Vector2(MenuLeft(y), y), new Vector2(UnityTuning.CampRes25Width, UnityTuning.Hud33Name + 8));
            }
            BuildMenus43(); // NIGHT43 U1 screens43 (UiMenus43.cs): the menus' panels, the pause backdrop; nothing with the row off
        }

        /// <summary>Every text under `t` with its face's hard-shadow material (UiTheme.ShadowFor; a face without one keeps its own).</summary>
        static void Shadow33(Transform t)
        {
            foreach (TextMeshProUGUI tx in t.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                Material m = UiTheme.ShadowFor(tx.font);
                if (m != null) tx.fontSharedMaterial = m;
            }
        }

        static void HideGraphic33(Transform t)
        {
            if (t == null) return;
            foreach (Graphic g in t.GetComponentsInChildren<Graphic>(true)) g.enabled = false;
        }

        /// <summary>One item: the plate invisible (still the click target), the accent gone, two slash ticks left of it for the focus.</summary>
        void Front33Item(MenuItemView it)
        {
            if (it == null || front33Ticks.ContainsKey(it)) return;
            it.plate.color = UiTheme.A(it.plate.color, 0f);
            it.bar.enabled = false;
            var ticks = new Image[2];
            for (int i = 0; i < 2; i++)
            {
                ticks[i] = UiStyle33.Slash(it.rt, "Focus tick " + i, UiTheme.A(UiStyle33.Cyan, 0f), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(-UnityTuning.Front33TickLead + i * UnityTuning.Front33TickPitch, 0), new Vector2(UnityTuning.Hud33TickW, UnityTuning.Front33TickH));
            }
            front33Ticks[it] = ticks;
            it.lit = -1; // re-applied on its next step
        }

        /// <summary>MenuStepItems' colours for an item with the row on (lit 0 rest .. 1 focused, eased).</summary>
        void Front33Paint(MenuItemView it, float lit)
        {
            if (!front33Ticks.TryGetValue(it, out Image[] ticks)) Front33Item(it); // an item made after the build (a late screen)
            if (!front33Ticks.TryGetValue(it, out ticks)) return;
            it.plate.color = UiTheme.A(it.plate.color, 0f);
            bool locked = it.go.TryGetComponent(out UnityEngine.UI.Button b) && !b.interactable; // a locked level row: dimmed text (the veil plate is gone)
            it.label.color = locked ? UiTheme.Muted : Color.Lerp(UiStyle33.Bone, UiStyle33.Cyan, lit);
            Color tc = UiTheme.A(UiStyle33.Cyan, lit);
            ticks[0].color = ticks[1].color = tc;
            if (lit > 0.5f) Front33FocusNow = it.go.name;
        }

        /// <summary>Every frame (RenderMenus): the wordmark's colours (its breath wrote cyan) and the POINTS BANKED line.</summary>
        void RenderFront33()
        {
            if (!front33) return;
            RenderMenus43(); // NIGHT43 U1 screens43: a screen that just opened sizes its panel; PAUSED's capture
            if (wordmark != null && title.activeSelf && wordmark.color != UiStyle33.Bone)
            {
                wordmark.color = UiStyle33.Bone;
                wordmarkBack.color = UiStyle33.Shadow;
            }
            if (front33Banked == null) return;
            int banked = CampRes25Open ? Shop33.LastBanked : -1, wallet = Shop33.Wallet;
            if (banked == front33BankedShown && wallet == front33WalletShown) return;
            front33BankedShown = banked;
            front33WalletShown = wallet;
            string s = banked < 0 ? "" : "POINTS BANKED  " + banked.ToString("N0", CultureInfo.InvariantCulture) + (wallet >= 0 ? "  ·  WALLET  " + wallet.ToString("N0", CultureInfo.InvariantCulture) : "");
            front33Banked.Set(s);
            Front33BankedNow = s;
        }
    }
}
