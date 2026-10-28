// NIGHT14 U3-27.9 (agent V): WHAT GOT YOU's screen (the Ui partial's hooks BuildRecap / RenderRecap). While the replay
// plays (SurvivalGame.RecapPlaying): the heading WHAT GOT YOU at the top, the killer's name (the recap's, e.g. HUSK) in
// red-orange (colors.enemy: danger) on a tag above its head as the orbit camera sees it (under the heading when it is
// off screen or there is no enemy: THE FALL), and the skip hint at the bottom; the DOWN screen and the HUD layers are
// hidden under it and given back as they were when it ends in the same flow (a flow change re-shows the screens itself).
// Sizes from the theme (h1, h3, body: nothing under 16 px at 1080p), layout numbers in UnityTuning.Recap.cs; texts set
// only when they change. Every glyph is in the static atlas (ASCII and the middle dot).
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform recapRoot, recapTagRect;
        TextMeshProUGUI recapHead, recapTag, recapHint;
        UiShape recapTagPlate; // NIGHT17: the killer's DANGER plate (sized to the name)
        string recapTagShown;
        bool recapHid, recapHadDown, recapHadFrame, recapHadValues, recapHadSight, recapTagOnHead;
        Flow recapHidFlow;
        Vector2 recapTagAnchor = new Vector2(-1, -1);

        partial void BuildRecap()
        {
            if (!FlightRecorder.Enabled(cfg)) return; // design.recap off: no screen at all (RenderRecap returns)
            var go = new GameObject("Recap", typeof(RectTransform));
            recapRoot = go.GetComponent<RectTransform>();
            recapRoot.SetParent(menus, false);
            recapRoot.anchorMin = Vector2.zero;
            recapRoot.anchorMax = Vector2.one;
            recapRoot.offsetMin = recapRoot.offsetMax = Vector2.zero;
            // NIGHT17 U3-30.8: WHAT GOT YOU in the display face (S75, the hard ink shadow); the killer's name in INK on a slanted
            // DANGER plate (enemies are DANGER: CLAUDE.md rule 8), not red text floating over the world; the skip as glyph + verb.
            float hp = UiTheme.Px(UiTheme.Size.S75);
            recapHead = Text(recapRoot, "Head", "WHAT GOT YOU", hp, UiTheme.White, TextAlignmentOptions.Center, new Vector2(0, -UnityTuning.RecapHeadTop),
                new Vector2(UnityTuning.RecapHeadWidth, UnityTuning.RecapHeadHeight + 30), new Vector2(0.5f, 1));
            UiTheme.Style(recapHead, hp, UiTheme.White, UiTheme.Display, shadow: true);
            recapHead.fontStyle = FontStyles.Normal;
            // Round 2 (the reviewer: "white on a white-rimmed box"): the replay a little darker (an INK sheet), and the heading
            // and the killer's plate on one INK block (opaque, slanted, cut), so the heading never sits on the bright world.
            var sheet = Img(recapRoot, "Sheet", UiTheme.A(UiTheme.InkSolid, UnityTuning.RecapSheetAlpha), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            sheet.transform.SetAsFirstSibling();
            float pad = UnityTuning.RecapBlockPad;
            float blockW = recapHead.GetPreferredValues("WHAT GOT YOU").x + 6 * pad;
            float blockH = UnityTuning.RecapHeadHeight + 30 + UnityTuning.RecapTagGap + UiTheme.Px(UiTheme.Size.S32) + 2 * UnityTuning.RecapTagPadY + 2 * pad;
            var blockGo = new GameObject("Block", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
            var brt = (RectTransform)blockGo.transform;
            brt.SetParent(recapRoot, false);
            brt.SetSiblingIndex(1); // over the sheet, under the heading
            brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(0.5f, 1);
            brt.anchoredPosition = new Vector2(0, -(UnityTuning.RecapHeadTop - pad));
            brt.sizeDelta = new Vector2(blockW, blockH);
            var block = blockGo.GetComponent<UiShape>();
            block.color = UiTheme.InkSolid;
            block.raycastTarget = false;
            block.SetCuts(UnityTuning.ThemeCut * 2, UnityTuning.ThemeCut * 2);
            var plateGo = new GameObject("Killer plate", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
            recapTagRect = (RectTransform)plateGo.transform;
            recapTagRect.SetParent(recapRoot, false);
            recapTagRect.anchorMin = recapTagRect.anchorMax = new Vector2(0.5f, 0.5f);
            recapTagRect.pivot = new Vector2(0.5f, 0); // the tag stands on the killer's head point
            recapTagRect.sizeDelta = new Vector2(UnityTuning.RecapTagWidth, UnityTuning.RecapTagHeight);
            recapTagPlate = plateGo.GetComponent<UiShape>();
            recapTagPlate.color = UiTheme.Danger;
            recapTagPlate.raycastTarget = false;
            float tp = UiTheme.Px(UiTheme.Size.S32);
            recapTag = Text(recapTagRect, "Killer", "", tp, UiTheme.InkSolid, TextAlignmentOptions.Center, Vector2.zero,
                new Vector2(UnityTuning.RecapTagWidth, UnityTuning.RecapTagHeight), new Vector2(0.5f, 0.5f));
            UiTheme.Style(recapTag, tp, UiTheme.InkSolid, UiTheme.Display);
            recapTag.fontStyle = FontStyles.Normal;
            float sp = UiTheme.Px(UiTheme.Size.S24);
            // Round 3 (the reviewer: ASCII brackets): a drawn key chip "ANY KEY" and the verb beside it, centred as one.
            recapHint = Text(recapRoot, "Skip", "SKIP", sp, UiTheme.CyanPale, TextAlignmentOptions.Left, Vector2.zero,
                new Vector2(UnityTuning.RecapHintWidth, UnityTuning.RecapHintHeight), new Vector2(0.5f, 0.5f));
            UiTheme.Style(recapHint, sp, UiTheme.CyanPale, UiTheme.BodyBold, shadow: true);
            UiShape chip = KeyChip(recapRoot, Vector2.zero, out TextMeshProUGUI chipText);
            KeyChipSet(chip, chipText, "ANY KEY");
            float chipW = chip.rectTransform.sizeDelta.x, verbW = recapHint.GetPreferredValues("SKIP").x;
            float left = -(chipW + UnityTuning.KeyChipGap + verbW) * 0.5f, hy = -540f + UnityTuning.RecapHintBottom + UnityTuning.KeyChipSize * 0.5f;
            chip.rectTransform.anchoredPosition = new Vector2(left, hy);
            MenuPinLeft(recapHint, left + chipW + UnityTuning.KeyChipGap, hy);
            go.SetActive(false);
        }

        partial void RenderRecap(double frameDt)
        {
            if (recapRoot == null) return;
            bool on = game.RecapPlaying;
            if (on != recapRoot.gameObject.activeSelf)
            {
                recapRoot.gameObject.SetActive(on);
                if (on) RecapHideScreens();
                else RecapGiveScreensBack();
            }
            if (!on) return;
            string name = game.RecapKillerName;
            if (!ReferenceEquals(name, recapTagShown))
            {
                recapTagShown = name;
                recapTag.SetText(name ?? "");
                float w = string.IsNullOrEmpty(name) ? 0 : recapTag.GetPreferredValues(name).x + 2 * UnityTuning.RecapTagPadX; // on a change of the name only
                recapTagRect.sizeDelta = new Vector2(w, UiTheme.Px(UiTheme.Size.S32) + 2 * UnityTuning.RecapTagPadY);
                recapTagPlate.enabled = w > 0;
            }
            // NIGHT17 round 1 (the lead: "WHAT [HUSK] YOU"): the heading and the killer's plate are one block, the plate always
            // centred under the heading's box (RecapStage outlines the killer in the scene; the tag no longer follows its head).
            bool onHead = false;
            Vector2 anchor = new Vector2(0.5f, 1);
            float plateH = recapTagRect.sizeDelta.y;
            Vector2 pos = new Vector2(0, -(UnityTuning.RecapHeadTop + UnityTuning.RecapHeadHeight + 30 + UnityTuning.RecapTagGap + plateH));
            if (anchor != recapTagAnchor || onHead != recapTagOnHead || recapTagRect.anchoredPosition != pos)
            {
                recapTagAnchor = anchor;
                recapTagOnHead = onHead;
                recapTagRect.anchorMin = recapTagRect.anchorMax = anchor;
                recapTagRect.anchoredPosition = pos;
            }
        }

        /// <summary>The replay starts: DOWN's screen and the HUD layers off (kept as they were, for the end).</summary>
        void RecapHideScreens()
        {
            recapHid = true;
            recapHidFlow = shownFlow;
            recapHadDown = down.activeSelf;
            recapHadFrame = frame.gameObject.activeSelf;
            recapHadValues = values.gameObject.activeSelf;
            recapHadSight = sight.gameObject.activeSelf;
            down.SetActive(false);
            frame.gameObject.SetActive(false);
            values.gameObject.SetActive(false);
            sight.gameObject.SetActive(false);
            recapTagAnchor = new Vector2(-1, -1); // placed again on its first frame
        }

        /// <summary>The replay ended: in the same flow, the screens as they were; after a flow change Render already set them.</summary>
        void RecapGiveScreensBack()
        {
            if (!recapHid) return;
            recapHid = false;
            if (shownFlow != recapHidFlow) return;
            down.SetActive(recapHadDown);
            frame.gameObject.SetActive(recapHadFrame);
            values.gameObject.SetActive(recapHadValues);
            sight.gameObject.SetActive(recapHadSight);
        }
    }
}
