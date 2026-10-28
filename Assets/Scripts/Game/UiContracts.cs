// NIGHT12 U3-25.7 (agent F): the CONTRACTS screen on TITLE, after menus.js renderChallenges (the CHALLENGES screen: the
// count, then a badge per challenge: its name, its desc, a progress bar and "value / target"). Here the rows of
// game.contracts.rows (the web's 24, then design.contracts' list; NIGHT13 S7: the countable ones only, in progress
// first, each with its reward, the dormant ones counted on one line under them), in a scrolling list (the wheel and
// drags; 30-40 rows); a finished one says DONE in cyan (with what it unlocked), an unfinished one with an unlock shows
// the lock (NIGHT13 S1: a padlock Image, UiMeta.PlaceLocks) and what it opens in muted text (Research/retention section 5
// #1: show the locks). The head counts them; BACK (and Esc, through SurvivalGame.HotKeys -> CloseMeta) returns to TITLE.
// Rows are built on first need and filled when the screen opens, never per frame.
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        sealed class ContractRowView
        {
            public UnityEngine.UI.Image back, edge, fill;
            public TextMeshProUGUI name, desc, value, status;
            // NIGHT19 U3-33.8 (builder H; NIGHT18's leftover: the CONTRACTS body appeared at once): the row's cascade
            public CanvasGroup group;
            public Vector2 home;
            public float shownAlpha = -1;
            public Vector2 shownOffset = new Vector2(float.NaN, 0);
        }

        double contractsEnterAge19 = double.MaxValue;
        int contractsShownRows19;

        /// <summary>
        /// NIGHT19 U3-33.8: frame time while CONTRACTS shows (UiMenus.RenderMenus): the rows cascade in along the slant as
        /// WEAPONS' do (RowCascade19; REDUCED MOTION: the fade only). Written only while it moves.
        /// </summary>
        void RenderContracts19(double frameDt)
        {
            if (contractsScreen == null || !contractsScreen.activeSelf || contractsEnterAge19 == double.MaxValue) return;
            double age = contractsEnterAge19;
            bool still = game.settings.View.reducedMotion;
            int n = System.Math.Min(contractsShownRows19, contractRows.Count);
            for (int i = 0; i < n; i++)
            {
                ContractRowView v = contractRows[i];
                if (v.group == null) v.group = v.back.gameObject.AddComponent<CanvasGroup>();
                float a = RowCascade19.Alpha(age, i);
                if (a != v.shownAlpha)
                {
                    v.shownAlpha = a;
                    v.group.alpha = a;
                }
                Vector2 off = RowCascade19.Offset(age, i, still);
                if (off != v.shownOffset)
                {
                    v.shownOffset = off;
                    v.back.rectTransform.anchoredPosition = v.home + off;
                }
            }
            // the dormant line under the rows enters last, with them
            if (contractsFooter != null && contractsFooter.gameObject.activeSelf)
            {
                float fa = RowCascade19.Alpha(age, n);
                if (contractsFooter.alpha != fa) contractsFooter.alpha = fa;
            }
            contractsEnterAge19 = RowCascade19.Done(age, n + 1) ? double.MaxValue : age + frameDt;
        }

        GameObject contractsScreen;
        TextMeshProUGUI contractsHead, contractsEmpty, contractsFooter;
        UnityEngine.UI.ScrollRect contractsScroll;
        RectTransform contractsContent;
        readonly List<ContractRowView> contractRows = new List<ContractRowView>();

        void BuildContracts()
        {
            contractsScreen = Screen("Contracts", UiTheme.A(UiTheme.InkSolid, UnityTuning.ThemeInkAlpha)); // NIGHT17 U3-30.10: the menus' INK
            contractsHead = MenuHead(contractsScreen.transform, "CONTRACTS", UnityTuning.MetaHeadY); // NIGHT17 U3-30.10
            (contractsScroll, contractsContent) = ScrollList(contractsScreen.transform, "Contract list");
            contractsEmpty = Text(contractsScreen.transform, "Empty", "NO CONTRACTS YET", tBody, muted, TextAlignmentOptions.Center, Vector2.zero, new Vector2(UnityTuning.MetaListWidth, UnityTuning.LineBody), new Vector2(0.5f, 0.5f));
            Button(contractsScreen.transform, "BACK", new Vector2(-UnityTuning.MetaSwitchX, UnityTuning.MetaBackY), CloseMeta);
            Button(contractsScreen.transform, "WEAPONS", new Vector2(UnityTuning.MetaSwitchX, UnityTuning.MetaBackY), () => OpenWeapons()); // NIGHT17: TITLE's PROGRESS holds both
            // NIGHT13 S7: the line under the rows that counts the dormant ones (their events aren't in this build).
            contractsFooter = Text(contractsContent, "Dormant", "", tBody, muted, TextAlignmentOptions.Center, Vector2.zero, new Vector2(UnityTuning.MetaListWidth, UnityTuning.LineBody), new Vector2(0.5f, 1));
            contractsFooter.overflowMode = TextOverflowModes.Ellipsis;
            contractsFooter.gameObject.SetActive(false);
            // NIGHT12 (the lead): the rows are built with the screen (hidden), not on the first open: the opening frame drew
            // the new rows as solid cyan bars in Play check 1.
            if (game.contracts != null && game.contracts.rows.Count > 0)
            {
                ContractRow(game.contracts.rows.Count - 1);
                foreach (ContractRowView v in contractRows) v.back.gameObject.SetActive(false);
            }
        }

        readonly List<Contracts.Row> contractOrder = new List<Contracts.Row>();

        partial void OpenContracts()
        {
            OpenMeta(MetaScreen.Contracts);
            if (metaOpen == MetaScreen.Contracts) RefreshContracts();
        }

        ContractRowView ContractRow(int i)
        {
            while (contractRows.Count <= i)
            {
                var v = new ContractRowView();
                v.back = Row(contractsContent, $"Contract {contractRows.Count}", UnityTuning.ContractRowHeight, out v.edge);
                Transform t = v.back.transform;
                float pad = UnityTuning.MetaRowPad;
                v.name = Text(t, "Name", "", tBody, text, TextAlignmentOptions.Left, new Vector2(pad, -UnityTuning.ContractNameY), new Vector2(UnityTuning.ContractNameWidth, UnityTuning.LineBody), new Vector2(0, 1));
                v.name.fontStyle = FontStyles.Bold;
                v.name.characterSpacing = UnityTuning.NameSpacing;
                v.name.overflowMode = TextOverflowModes.Ellipsis;
                v.desc = Text(t, "Desc", "", tSmall, muted, TextAlignmentOptions.Left, new Vector2(pad, -UnityTuning.ContractDescY), new Vector2(UnityTuning.ContractNameWidth, UnityTuning.LineSmall), new Vector2(0, 1));
                v.desc.overflowMode = TextOverflowModes.Ellipsis;
                v.value = Text(t, "Value", "", tSmall, text, TextAlignmentOptions.Right, new Vector2(-pad, -UnityTuning.ContractValueY), new Vector2(UnityTuning.ContractBarWidth, UnityTuning.LineSmall), new Vector2(1, 1));
                v.fill = Bar(t, new Vector2(1, 1), new Vector2(-pad, -UnityTuning.ContractBarY), UnityTuning.ContractBarWidth, out _);
                v.status = Text(t, "Status", "", tSmall, muted, TextAlignmentOptions.Right, new Vector2(-pad, -UnityTuning.ContractStatusY), new Vector2(UnityTuning.ContractStatusWidth, UnityTuning.LineSmall), new Vector2(1, 1));
                v.status.overflowMode = TextOverflowModes.Ellipsis;
                contractRows.Add(v);
            }
            return contractRows[i];
        }

        /// <summary>
        /// Fill the screen from game.contracts (on open). NIGHT13 S7: the rows this build can count, in progress first (the
        /// nearest to done on top), then the ones not started, then the done ones, each in display order; every row says
        /// its reward (what it unlocks, or the badge alone); the dormant web badges (Contracts.Dormant: their events aren't
        /// emitted in this build) are hidden behind one line that counts them. The head counts the rows shown.
        /// </summary>
        void RefreshContracts()
        {
            Contracts c = game.contracts;
            int n = c != null ? c.rows.Count : 0;
            contractOrder.Clear();
            int done = 0, dormant = 0;
            for (int i = 0; i < n; i++)
            {
                Contracts.Row r = c.rows[i];
                if (!r.live) dormant++;
                else if (r.done) done++;
            }
            for (int i = 0; i < n; i++) if (c.rows[i].live && !c.rows[i].done && c.rows[i].value > 0) contractOrder.Add(c.rows[i]);
            int started = contractOrder.Count;
            contractOrder.Sort(ByNearest); // only the started rows are in the list yet
            for (int i = 0; i < n; i++) if (c.rows[i].live && !c.rows[i].done && !(c.rows[i].value > 0)) contractOrder.Add(c.rows[i]);
            for (int i = 0; i < n; i++) if (c.rows[i].live && c.rows[i].done) contractOrder.Add(c.rows[i]);
            int shown = contractOrder.Count;

            metaSb.Clear();
            metaSb.Append("CONTRACTS");
            if (shown > 0) metaSb.Append(" · ").Append(done).Append(" / ").Append(shown).Append(" DONE");
            contractsHead.SetText(metaSb);
            contractsEmpty.gameObject.SetActive(n == 0);
            float step = UnityTuning.ContractRowHeight + UnityTuning.ContractRowGap;
            for (int i = 0; i < shown; i++)
            {
                Contracts.Row r = contractOrder[i];
                ContractRowView v = ContractRow(i);
                v.back.gameObject.SetActive(true);
                v.home = new Vector2(0, -i * step);
                v.back.rectTransform.anchoredPosition = v.home;
                v.name.text = r.name ?? r.id;
                v.desc.text = r.desc ?? string.Empty;
                v.value.text = Progress(r.done ? r.target : r.value, r.target);
                SetBar(v.fill, r.done ? 1 : r.Fraction);
                v.edge.color = r.done ? UiTheme.Cyan : i < started ? UiTheme.CyanDeep : faint; // NIGHT17: done CYAN, in progress CYAN-DEEP, the rest INK-SOFT
                if (r.done)
                {
                    v.status.text = r.unlockName != null ? "DONE · UNLOCKED: " + r.unlockName : "DONE";
                    v.status.color = usable;
                }
                else if (r.unlock != null)
                {
                    v.status.text = "REWARD: " + LockMark + " " + (r.unlockName ?? r.unlock.ToUpperInvariant());
                    v.status.color = text;
                }
                else
                {
                    v.status.text = UnityTuning.ContractBadgeReward17;
                    v.status.color = muted;
                }
                PlaceLocks(v.status, !r.done && r.unlock != null); // NIGHT13 S1: the padlock over its mark
            }
            for (int i = shown; i < contractRows.Count; i++) contractRows[i].back.gameObject.SetActive(false);
            float height = shown * step;
            bool footer = dormant > 0;
            contractsFooter.gameObject.SetActive(footer);
            if (footer)
            {
                metaSb.Clear();
                metaSb.Append(LockMark).Append(' ').AppendFormat(CultureInfo.InvariantCulture, UnityTuning.ContractDormantLine17, dormant);
                contractsFooter.SetText(metaSb);
                contractsFooter.rectTransform.anchoredPosition = new Vector2(0, -height - UnityTuning.ContractFooterGap);
                PlaceLocks(contractsFooter, true);
                height += UnityTuning.ContractFooterGap + UnityTuning.LineBody;
            }
            ResetList(contractsScroll, contractsContent, height);
            // NIGHT19 U3-33.8: the rows cascade in from here, drawn at their start this frame
            contractsShownRows19 = shown;
            contractsEnterAge19 = 0;
            RenderContracts19(0);
        }

        /// <summary>Started contracts: the nearest to done first, then display order.</summary>
        static int ByNearest(Contracts.Row a, Contracts.Row b)
        {
            int f = b.Fraction.CompareTo(a.Fraction);
            return f != 0 ? f : a.order.CompareTo(b.order);
        }
    }
}
