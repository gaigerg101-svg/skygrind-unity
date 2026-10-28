// NIGHT43 builder U1 (design.n43.screens43, a view row; GG's G24: the shop "currently feel[s] like large amounts of plain text placed over the game.
// Blur or otherwise visually separate the gameplay background more effectively, organize information into concise and attractive interface
// components, establish stronger hierarchy, use the game's color palette more deliberately, and add tasteful animations"). With the row on, the
// SAME shop (every text, every rule, every input, the reel's mirrors: UiShop33.cs unchanged but four hook lines) drawn in the dusk instrument's
// language (Research/2026-10-16-wave43/U1.md):
//   - the backdrop: the game's last frame, blurred once and multiplied toward the dusk violet, with scanlines (UiBlur43); the NIGHT33 scrim thinned;
//   - hierarchy: SHOP in the HERO face at 75; the WALLET is the screen's hero number (75, GOLD: points), in an INK cut panel with a gold accent;
//     each slot kind an INK cut panel with a cyan accent (the player's things); prices in gold (dimmed when the wallet is short), EQUIPPED cyan, OWNED muted;
//   - the focused row: a raised INK-SOFT plate behind it and a cyan accent blade (the NIGHT33 slash tick stays);
//   - CONTINUE on a blade plate, cyan-deep when focused;
//   - the entrance: the title, the wallet, the three panels and the bottom row come in one after another along the slant (0.15 s each, 0.05 s apart).
// Off: nothing here is built and the shop is NIGHT33's. Numbers: UnityTuning.Ui43.cs.
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>NIGHT43 screens43's statics for the reels (the shop's and the results').</summary>
    public static class Screens43Probe
    {
        public static bool ShopBuilt, ShopBackdrop, ResultsBuilt, ResultsBackdrop;
        /// <summary>s from the open until the last entrance part settled (the shop / the results).</summary>
        public static double ShopEnterDone = -1, ResultsEnterDone = -1;
        public static string ShopFocus = "";
    }

    public sealed partial class UiShop33
    {
        bool s43On, s43Built;
        RawImage s43Back, s43Scan;
        readonly List<RectTransform> s43Groups = new List<RectTransform>();
        readonly List<CanvasGroup> s43GroupAlpha = new List<CanvasGroup>();
        Image s43Focus, s43FocusAccent, s43ContPlate;
        bool s43Settled;

        /// <summary>From Build (after NIGHT33's parts exist).</summary>
        void Build43()
        {
            s43On = game != null && N43View.RowOn(game.cfg, "screens43");
            if (!s43On) return;
            s43Built = true;
            Screens43Probe.ShopBuilt = true;
            Color ink = UiTheme.Ready ? UiTheme.InkSolid : new Color(0.047f, 0.043f, 0.086f, 1f);
            Color inkSoft = UiTheme.Ready ? UiTheme.InkSoft : ink;
            Color panel = UiTheme.A(ink, UnityTuning.Screens43PanelAlpha);
            Color gold = UiTheme.Ready ? UiTheme.Gold : new Color(1f, 0.76f, 0.24f, 1f);
            Color muted = UiKit43.Muted;
            Vector2 tl = new Vector2(0, 1), tr = new Vector2(1, 1), br = new Vector2(1, 0);

            // the backdrop behind everything; the scrim thinned over it
            s43Back = UiBlur43.Backdrop(root, out s43Scan);
            Transform scrimT = root.Find("Scrim");
            if (scrimT != null && scrimT.TryGetComponent(out Image scrim))
            {
                Color sc = scrim.color;
                scrim.color = new Color(sc.r, sc.g, sc.b, UnityTuning.Screens43ScrimAlpha);
            }

            // the entrance groups (full-screen rects: the parts keep their anchored places), in order
            RectTransform gHead = Group43("Enter head"), gWallet = Group43("Enter wallet");
            var gCols = new RectTransform[cols.Count];
            for (int c = 0; c < cols.Count; c++) gCols[c] = Group43("Enter column " + c);
            RectTransform gBottom = Group43("Enter bottom");

            // the title
            Move43(head.rt, gHead);
            Move43(sub.rt, gHead);
            Hero43(head, UnityTuning.Screens43HeroPx);
            sub.Color = muted;

            // the wallet: the hero number, gold, in a panel with a gold accent
            float side = UnityTuning.Shop33Side, top = UnityTuning.Shop33Top, pad = UnityTuning.Screens43Pad;
            float wh = UnityTuning.Shop33BankedY + 30 - top + 2 * pad * 0.5f;
            Image wp = UiKit43.Plate(gWallet, "Wallet panel", panel, tr, tr, new Vector2(-side + pad, -top + pad * 0.5f), new Vector2(UnityTuning.Shop33WalletW + 2 * pad, wh));
            UiStyle33.Line(wp.rectTransform, "Accent", gold, tl, tl, new Vector2(UnityTuning.Results43AccentInset, -UnityTuning.Results43AccentInset), new Vector2(UnityTuning.Screens43AccentW, UnityTuning.Shop43AccentH));
            Move43(walletLabel.rt, gWallet);
            Move43(wallet.rt, gWallet);
            Move43(banked.rt, gWallet);
            wallet.Color = gold;
            walletLabel.Color = muted;

            // the three slot panels (cyan accent: the player's things)
            for (int c = 0; c < cols.Count; c++)
            {
                Col col = cols[c];
                float x = side + c * UnityTuning.Shop33ColPitch;
                float ph = UnityTuning.Shop33RowTop - UnityTuning.Shop33ColTop + col.rows.Count * UnityTuning.Shop33RowPitch + pad * 0.5f;
                Image p = UiKit43.Plate(gCols[c], "Panel", panel, tl, tl, new Vector2(x - pad - UnityTuning.Shop33FocusTickX * 0.5f, -UnityTuning.Shop33ColTop + pad), new Vector2(UnityTuning.Shop33ColW + 2 * pad + UnityTuning.Shop33FocusTickX * 0.5f, ph + pad));
                // round 2 (n43-r1 s-shop-sel: a full-height 12 deg stroke leaned ~100 px across the items' first letters): a short straight bar in the left
                // margin beside the header only
                UiStyle33.Line(p.rectTransform, "Accent", UiStyle33.Cyan, tl, tl, new Vector2(UnityTuning.Results43AccentInset, -pad), new Vector2(UnityTuning.Screens43AccentW, UnityTuning.Shop43AccentH));
                UiKit43.Scan(p.rectTransform, "Scan", UnityTuning.Ui43ScanAlpha);
                Move43(col.head.rt, gCols[c]);
                foreach (Image t in col.ticks) Move43(t.rectTransform, gCols[c]);
                foreach (Row r in col.rows)
                {
                    Move43(r.rt, gCols[c]);
                    Move43(r.name.rt, gCols[c]);
                    Move43(r.line.rt, gCols[c]);
                    Move43(r.state.rt, gCols[c]);
                    Move43(r.focus.rectTransform, gCols[c]);
                    r.line.Color = muted;
                }
            }
            // the focused row's raised plate (moved by Refresh43)
            s43Focus = UiKit43.Plate(root, "Focus plate", UiTheme.A(inkSoft, UnityTuning.Screens43FocusPlateAlpha), tl, tl, Vector2.zero, Vector2.zero);
            s43FocusAccent = UiStyle33.Line(s43Focus.rectTransform, "Accent", UiStyle33.Cyan, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(UnityTuning.Screens43AccentW, 10));
            s43FocusAccent.gameObject.SetActive(false); // round 2: NIGHT33's cyan focus tick in the margin marks the row; a second stroke cut the name's first letter
            s43Focus.gameObject.SetActive(false);

            // the bottom row: the message, the hint, CONTINUE on a blade plate
            Move43(message.rt, gBottom);
            Move43(hint.rt, gBottom);
            hint.Color = muted;
            s43ContPlate = UiKit43.Blade(gBottom, "Continue plate", panel, br, br, new Vector2(-side + pad * 0.5f, UnityTuning.Shop33BottomRowY - 10), new Vector2(UnityTuning.Shop33ContinueW + pad, UnityTuning.Screens43ContinueH));
            Move43(cont.rt, gBottom);
            Move43(contFocus.rectTransform, gBottom);
        }

        RectTransform Group43(string name)
        {
            RectTransform g = UiStyle33.Rect(root, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UiKit43.Stretch(g);
            s43GroupAlpha.Add(g.gameObject.AddComponent<CanvasGroup>());
            s43Groups.Add(g);
            return g;
        }

        static void Move43(RectTransform rt, RectTransform group)
        {
            if (rt != null) rt.SetParent(group, false); // the group is the root's size: every anchored place holds
        }

        static void Hero43(UiStyle33.Text33 t, float px)
        {
            t.main.fontSize = t.back.fontSize = px;
            t.rt.sizeDelta = new Vector2(t.rt.sizeDelta.x, px + 10);
        }

        /// <summary>From Open (after its Refresh): the capture of this frame (the shop is at alpha 0 on its first frame), the entrance from the top.</summary>
        void Open43()
        {
            if (!s43Built) return;
            s43Back.texture = null;
            s43Back.color = UiBlur43.Tint(false);
            Screens43Probe.ShopBackdrop = false;
            Screens43Probe.ShopEnterDone = -1;
            s43Settled = false;
            Hero43(wallet, UnityTuning.Screens43HeroPx);
            Tick43(openedAt);
            StartCoroutine(UiBlur43.Capture(tex =>
            {
                if (this == null || s43Back == null) return;
                s43Back.texture = tex;
                s43Back.color = UiBlur43.Tint(tex != null);
                Screens43Probe.ShopBackdrop = tex != null;
            }));
        }

        /// <summary>From Refresh: the colours by meaning (prices gold, EQUIPPED cyan, OWNED muted) and the focused row's plate.</summary>
        void Refresh43()
        {
            if (!s43Built) return;
            Shop33Catalogue cat = game.Shop33Cat;
            int code = cat.Sanitize(game.Shop33Code);
            Color gold = UiTheme.Ready ? UiTheme.Gold : new Color(1f, 0.76f, 0.24f, 1f), muted = UiKit43.Muted;
            for (int c = 0; c < cols.Count; c++)
                foreach (Row row in cols[c].rows)
                {
                    int i = row.item.index;
                    bool owned = Shop33Catalogue.Owns(code, i), on = Shop33Catalogue.Active(code, i);
                    row.state.Color = on ? UiStyle33.Cyan : owned ? muted : gold;
                }
            bool show = fc >= 0;
            if (s43Focus.gameObject.activeSelf != show) s43Focus.gameObject.SetActive(show);
            if (show)
            {
                Row r = cols[fc].rows[fr];
                RectTransform g = r.rt.parent as RectTransform;
                if (s43Focus.rectTransform.parent != g) s43Focus.rectTransform.SetParent(g, false);
                s43Focus.rectTransform.SetSiblingIndex(1); // over the column's panel, under its texts
                float pad = UnityTuning.Screens43Pad * 0.5f;
                s43Focus.rectTransform.anchoredPosition = r.rt.anchoredPosition + new Vector2(-pad, pad * 0.5f);
                s43Focus.rectTransform.sizeDelta = r.rt.sizeDelta + new Vector2(2 * pad, pad);
                s43FocusAccent.rectTransform.sizeDelta = new Vector2(UnityTuning.Screens43AccentW, r.rt.sizeDelta.y);
                Screens43Probe.ShopFocus = r.item.id;
            }
            else Screens43Probe.ShopFocus = "continue";
            s43ContPlate.color = fc < 0 ? UiStyle33.CyanDeep : UiTheme.A(UiTheme.Ready ? UiTheme.InkSolid : Color.black, UnityTuning.Screens43PanelAlpha);
        }

        /// <summary>From Update: the staggered entrance along the slant.</summary>
        void Tick43(float now)
        {
            if (!s43Built || s43Settled) return;
            double t = now - openedAt;
            float dx = -UnityTuning.Ui43EnterFrom * UiKit43.SlantTan, dy = -UnityTuning.Ui43EnterFrom;
            bool all = true;
            for (int i = 0; i < s43Groups.Count; i++)
            {
                double a = (t - i * UnityTuning.Ui43EnterGap) / UnityTuning.Ui43EnterTime;
                float k = a <= 0 ? 0f : UiTween.OutBack(a);
                if (a < 1) all = false;
                s43Groups[i].anchoredPosition = new Vector2(Mathf.Round(dx * (1 - k)), Mathf.Round(dy * (1 - k)));
                s43GroupAlpha[i].alpha = UiTween.Clamp01(a);
            }
            if (all)
            {
                s43Settled = true;
                Screens43Probe.ShopEnterDone = t;
            }
        }
    }
}
