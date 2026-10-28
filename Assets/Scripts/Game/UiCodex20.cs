// NIGHT20 U3-34.9 E3 (builder H): the CODEX tab inside WEAPONS (design.adds.codex; Research/2026-10-06-additions-backlog.md
// E3: RoR2's logbook, Hades' and DOOM's codex; "the meta stays at three screens": a tab, not a screen).
// - WEAPONS grows two tabs by its head: GUNS (NIGHT19's role cards, as before) and CODEX.
// - CODEX lists the web's kinds a run meets (Codex20.Enemies: each entry its debut card: the name over its one counter, a
//   DANGER edge), the affixes met (C3, builder E: affixes.js' five, each with the counter its header names, a DANGER edge)
//   and builder G's named combos (Codex20.Combos: the name over how it's made, a CYAN edge), each section under
//   its count ("ENEMIES 3 / 8"). A locked entry is a silhouette: its name drawn as a dark slanted bar, "NOT MET YET" under an
//   enemy, the how line (MUTED) under a combo, so the codex teaches what to try.
// - Recording (in a run, while the row is live): a kind on the field (a scan every Hud20CodexScan s), the player's `combo`
//   events (N19Events.Combo); Profile flags codex20.enemy.<kind> / codex20.combo.<NAME>, saved at the round's clear or the
//   run's end (not mid-fight). A debut card (C1) or a finished run (the husk) also counts as met.
// - The rows cascade in along the slant when the tab opens (RowCascade19, as the GUNS rows: M9); every text >= 18 px (M10);
//   each row is selectable (UiNav: the pad walks the list) and the list follows the selection.
// Off (the row, the adds bit, or -skygrindOff20): no tabs, WEAPONS as NIGHT19's. Built once; filled when the tab opens.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        sealed class CodexRowView
        {
            public UnityEngine.UI.Image back, edge;
            public TextMeshProUGUI name, line;
            public UiShape silhouette;
            public CanvasGroup group;
            public Vector2 home;
            public float shownAlpha = -1;
            public Vector2 shownOffset = new Vector2(float.NaN, 0);
        }

        bool codexCfg20, codexOpen20, codexSavePending20;
        TextMeshProUGUI codexTabGuns20, codexTabCodex20;
        UnityEngine.UI.ScrollRect codexScroll20;
        RectTransform codexContent20;
        readonly List<CodexRowView> codexRows20 = new List<CodexRowView>();
        int codexShownRows20;
        double codexEnterAge20 = double.MaxValue, codexScanLeft20;

        void BuildCodex20()
        {
            codexCfg20 = cfg.Design("adds.enabled") && cfg.Design("adds.codex.enabled") && !N20Blocks.ForceOff;
            if (weaponsScreen == null || !codexCfg20) return;
            float y = UnityTuning.MetaHeadY, right = UnityTuning.MetaListWidth * 0.5f;
            var size = new Vector2(UnityTuning.Hud20CodexTabW, UnityTuning.Hud20CodexTabH);
            codexTabGuns20 = SmallButton(weaponsScreen.transform, "GUNS", new Vector2(right - 1.5f * size.x - UnityTuning.Hud20CodexTabGap, y), size, () => CodexShow20(false));
            codexTabCodex20 = SmallButton(weaponsScreen.transform, "CODEX", new Vector2(right - 0.5f * size.x, y), size, () => CodexShow20(true));
            (codexScroll20, codexContent20) = ScrollList(weaponsScreen.transform, "Codex list");
            codexScroll20.gameObject.SetActive(false);
            CodexTabs20();

            game.events.On<ComboEvent>(N19Events.Combo, e =>
            {
                if (e == null || e.owner != "player" || !CodexLive20()) return;
                if (Codex20.RecordCombo(game.profile, e.name)) codexSavePending20 = true;
            });
            game.events.On("roundClear", _ => CodexSave20());
            game.OnNewRun += CodexSave20;
        }

        double codexTabsAge20;
        int codexGlyphVersion20 = -1;
        bool codexGlyphPad20;

        /// <summary>The tabs' words, with LB before GUNS and RB after CODEX on a pad (on a device or binding change only).</summary>
        void CodexTabGlyphs20()
        {
            bool pad = PlayerDevices.Last == InputDevice.Pad;
            int v = PromptText.Version;
            if (v == codexGlyphVersion20 && pad == codexGlyphPad20) return;
            codexGlyphVersion20 = v;
            codexGlyphPad20 = pad;
            codexTabGuns20.richText = codexTabCodex20.richText = true;
            codexTabGuns20.text = pad ? PromptText.Button(PadButtonName.LB, PlayerDevices.Glyphs) + " GUNS" : "GUNS";
            codexTabCodex20.text = pad ? "CODEX " + PromptText.Button(PadButtonName.RB, PlayerDevices.Glyphs) : "CODEX";
        }

        /// <summary>A tab's entrance: its group's alpha and its slide from home (its x as a share of the tab's width from the
        /// list's right edge).</summary>
        void CodexTab20(TextMeshProUGUI label, float a, Vector2 off, float xShare)
        {
            var plate = (RectTransform)label.transform.parent;
            CanvasGroup g = plate.GetComponent<CanvasGroup>();
            if (g == null) g = plate.gameObject.AddComponent<CanvasGroup>();
            g.alpha = a;
            plate.anchoredPosition = new Vector2(UnityTuning.MetaListWidth * 0.5f + xShare * UnityTuning.Hud20CodexTabW + (xShare < -1 ? -UnityTuning.Hud20CodexTabGap : 0), UnityTuning.MetaHeadY) + off;
        }

        bool CodexLive20() => codexCfg20 && game.profile != null && game.sim != null && game.sim.n20.adds;

        void CodexSave20()
        {
            if (!codexSavePending20) return;
            codexSavePending20 = false;
            game.SaveProfile();
        }

        /// <summary>In a run: a kind on the field is met (a scan every Hud20CodexScan s; no allocation).</summary>
        void RecordCodex20(double frameDt)
        {
            if (!codexCfg20 || game.flow != Flow.Playing || !CodexLive20()) return;
            codexScanLeft20 -= frameDt;
            if (codexScanLeft20 > 0) return;
            codexScanLeft20 = UnityTuning.Hud20CodexScan;
            List<Enemy> all = game.enemies != null ? game.enemies.all : null;
            if (all == null) return;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy e = all[i];
                if (e == null || !e.alive) continue;
                if (!game.profile.Flag(Codex20.EnemyFlag(e.kind)) && Codex20.RecordEnemy(game.profile, e.kind)) codexSavePending20 = true;
                if (e.Shielded && !game.profile.Flag(Codex20.AffixFlag("shielded")) && Codex20.RecordAffix(game.profile, "shielded")) codexSavePending20 = true; // NIGHT19's plate
            }
            // C3's affixes (builder E: Affixes.Live, each record's names)
            IReadOnlyList<AffixRec> live = game.sim.affixes != null ? game.sim.affixes.Live : null;
            if (live == null) return;
            for (int i = 0; i < live.Count; i++)
            {
                List<string> names = live[i].names;
                for (int k = 0; k < names.Count; k++)
                    if (!game.profile.Flag(Codex20.AffixFlag(names[k])) && Codex20.RecordAffix(game.profile, names[k])) codexSavePending20 = true;
            }
        }

        /// <summary>WEAPONS opens on GUNS; the tabs switch between the two lists.</summary>
        void CodexShow20(bool codex)
        {
            if (codexScroll20 == null) return;
            codexOpen20 = codex;
            weaponsScroll.gameObject.SetActive(!codex);
            if (codex) weaponsEmpty.gameObject.SetActive(false);
            codexScroll20.gameObject.SetActive(codex);
            CodexTabs20();
            if (codex) RefreshCodex20();
            else RefreshWeapons();
        }

        void CodexTabs20()
        {
            if (codexTabGuns20 == null) return;
            codexTabGuns20.transform.parent.GetComponent<UnityEngine.UI.Image>().color = codexOpen20 ? UiTheme.InkSoft : UiTheme.CyanDeep;
            codexTabCodex20.transform.parent.GetComponent<UnityEngine.UI.Image>().color = codexOpen20 ? UiTheme.CyanDeep : UiTheme.InkSoft;
        }

        CodexRowView CodexRow20(int i)
        {
            while (codexRows20.Count <= i)
            {
                var v = new CodexRowView();
                v.back = Row(codexContent20, $"Codex {codexRows20.Count}", UnityTuning.Hud20CodexRowH, out v.edge);
                v.back.raycastTarget = true;
                var b = v.back.gameObject.AddComponent<UnityEngine.UI.Button>();
                b.transition = UnityEngine.UI.Selectable.Transition.None; // UiNav's focus frame shows it; the pad walks the list
                v.group = v.back.gameObject.AddComponent<CanvasGroup>();
                Transform t = v.back.transform;
                float pad = UnityTuning.MetaRowPad + UnityTuning.ThemeAccentBar;
                float namePx = UiTheme.Px(UiTheme.Size.S32), linePx = UiTheme.Px(UiTheme.Size.S18);
                v.name = Hud17Text(t, "Name", "", namePx, UiTheme.White, UiTheme.Display, TextAlignmentOptions.Left, new Vector2(0, 1), new Vector2(0, 1), new Vector2(pad, -10), new Vector2(UnityTuning.MetaListWidth - 2 * pad, namePx * 1.2f), true);
                v.silhouette = Hud17Shape(t, "Silhouette", UiTheme.InkSolid, new Vector2(0, 1), new Vector2(0, 1), new Vector2(pad, -14), new Vector2(UnityTuning.Hud20CodexSilW, UnityTuning.Hud20CodexSilH));
                v.line = Hud17Text(t, "Line", "", linePx, UiTheme.CyanPale, UiTheme.BodyBold, TextAlignmentOptions.Left, new Vector2(0, 0), new Vector2(0, 0), new Vector2(pad, 12), new Vector2(UnityTuning.MetaListWidth - 2 * pad, linePx * 1.3f), false);
                v.line.characterSpacing = UnityTuning.ThemeLabelTracking;
                codexRows20.Add(v);
            }
            return codexRows20[i];
        }

        /// <summary>Fill the CODEX from the profile (on open): a head row per section, then its entries.</summary>
        void RefreshCodex20()
        {
            Profile p = game.profile;
            int row = 0;
            float y = 0;
            y = CodexHead20(row++, y, Codex20.EnemiesHead(Codex20.EnemiesMet(p)));
            foreach (string k in Codex20.Enemies)
            {
                bool met = Codex20.EnemyMet(p, k);
                y = CodexEntry20(row++, y, met ? Codex20.EnemyName(k) : null, met ? Codex20.EnemyLine(k) : UnityTuning.Hud20CodexLockedEnemy, met, UiTheme.Danger);
            }
            y = CodexHead20(row++, y, Codex20.AffixesHead(Codex20.AffixesMet(p)));
            foreach (string a in Codex20.Affixes)
            {
                bool met = Codex20.AffixMet(p, a);
                y = CodexEntry20(row++, y, met ? Codex20.AffixName(a) : null, met ? Codex20.AffixLine(a) : UnityTuning.Hud20CodexLockedEnemy, met, UiTheme.Danger);
            }
            y = CodexHead20(row++, y, Codex20.CombosHead(Codex20.CombosFound(p)));
            foreach (string c in Codex20.Combos)
            {
                bool found = Codex20.ComboFound(p, c);
                y = CodexEntry20(row++, y, found ? c : null, Codex20.ComboHow(c), found, UiTheme.Cyan);
            }
            for (int i = row; i < codexRows20.Count; i++) codexRows20[i].back.gameObject.SetActive(false);
            ResetList(codexScroll20, codexContent20, y);
            codexShownRows20 = row;
            codexEnterAge20 = 0;
            RenderCodex20(0);
        }

        /// <summary>A section's head row: its count, no plate (S24, CYAN-PALE).</summary>
        float CodexHead20(int i, float y, string head)
        {
            CodexRowView v = CodexRow20(i);
            v.back.gameObject.SetActive(true);
            v.back.color = UiTheme.A(UiTheme.InkSoft, 0f);
            v.edge.enabled = false;
            v.back.rectTransform.sizeDelta = new Vector2(0, UnityTuning.Hud20CodexHeadH);
            v.home = new Vector2(0, -y);
            v.back.rectTransform.anchoredPosition = v.home;
            UiTheme.Style(v.name, UiTheme.Px(UiTheme.Size.S24), UiTheme.CyanPale, UiTheme.BodyBold, true);
            v.name.characterSpacing = UnityTuning.ThemeLabelTracking;
            v.name.gameObject.SetActive(true);
            v.name.text = head;
            v.silhouette.gameObject.SetActive(false);
            v.line.gameObject.SetActive(false);
            v.back.GetComponent<UnityEngine.UI.Button>().interactable = false;
            return y + UnityTuning.Hud20CodexHeadH + UnityTuning.WeaponRowGap;
        }

        /// <summary>An entry: the name over its line (found), or a silhouette bar over its hint (locked); its edge's role colour.</summary>
        float CodexEntry20(int i, float y, string name, string line, bool found, Color role)
        {
            CodexRowView v = CodexRow20(i);
            v.back.gameObject.SetActive(true);
            v.back.color = UiTheme.InkSoft;
            v.edge.enabled = true;
            v.edge.color = found ? role : UiTheme.Muted;
            v.back.rectTransform.sizeDelta = new Vector2(0, UnityTuning.Hud20CodexRowH);
            v.home = new Vector2(0, -y);
            v.back.rectTransform.anchoredPosition = v.home;
            UiTheme.Style(v.name, UiTheme.Px(UiTheme.Size.S32), UiTheme.White, UiTheme.Display, true);
            v.name.gameObject.SetActive(found);
            if (found) v.name.text = name;
            v.silhouette.gameObject.SetActive(!found);
            v.line.gameObject.SetActive(line != null);
            v.line.text = line ?? string.Empty;
            v.line.color = found ? UiTheme.CyanPale : UiTheme.Muted;
            v.back.GetComponent<UnityEngine.UI.Button>().interactable = true;
            return y + UnityTuning.Hud20CodexRowH + UnityTuning.WeaponRowGap;
        }

        /// <summary>Frame time while the CODEX shows: the rows cascade in (RowCascade19); the list follows the pad's selection.
        /// While WEAPONS shows: the tabs enter with the screen (as its first row: M9), once per opening.</summary>
        void RenderCodex20(double frameDt)
        {
            if (codexScroll20 == null || weaponsScreen == null) return;
            bool screenOn = weaponsScreen.activeSelf;
            if (screenOn)
            {
                // banned 10: on a pad the tabs carry their bumpers' glyphs (PromptText's sprites), and LB / RB switch them
                // (UiNav leaves the meta screens' tabs to them: PadTab19)
                CodexTabGlyphs20();
                MenuInput m = PlayerDevices.Menu;
                if ((m.tabPrev && codexOpen20) || (m.tabNext && !codexOpen20)) CodexShow20(!codexOpen20);
            }
            if (!screenOn) codexTabsAge20 = 0; // the next opening enters again
            else if (codexTabsAge20 != double.MaxValue)
            {
                double ta = codexTabsAge20;
                float a = RowCascade19.Alpha(ta, 0);
                Vector2 off = RowCascade19.Offset(ta, 0, game.settings.View.reducedMotion);
                CodexTab20(codexTabGuns20, a, off, -1.5f);
                CodexTab20(codexTabCodex20, a, off, -0.5f);
                codexTabsAge20 = RowCascade19.Done(ta, 1) ? double.MaxValue : ta + frameDt;
            }
            if (!codexOpen20 || !screenOn) return;
            if (codexEnterAge20 != double.MaxValue)
            {
                double age = codexEnterAge20;
                bool still = game.settings.View.reducedMotion;
                int n = Math.Min(codexShownRows20, codexRows20.Count);
                for (int i = 0; i < n; i++)
                {
                    CodexRowView v = codexRows20[i];
                    float a = RowCascade19.Alpha(age, i);
                    if (a != v.shownAlpha) v.group.alpha = v.shownAlpha = a;
                    Vector2 off = RowCascade19.Offset(age, i, still);
                    if (off != v.shownOffset)
                    {
                        v.shownOffset = off;
                        v.back.rectTransform.anchoredPosition = v.home + off;
                    }
                }
                codexEnterAge20 = RowCascade19.Done(age, n) ? double.MaxValue : age + frameDt;
            }
            // the pad walks the rows: keep the selected one inside the viewport
            GameObject sel = UnityEngine.EventSystems.EventSystem.current != null ? UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject : null;
            if (sel == null || sel.transform.parent != codexContent20) return;
            var rt = (RectTransform)sel.transform;
            float top = -rt.anchoredPosition.y, bottom = top + rt.sizeDelta.y;
            float view = codexScroll20.viewport.rect.height, scrollY = codexContent20.anchoredPosition.y;
            float want = scrollY;
            if (top < scrollY) want = top;
            else if (bottom > scrollY + view) want = bottom - view;
            if (want != scrollY) codexContent20.anchoredPosition = new Vector2(codexContent20.anchoredPosition.x, Mathf.Max(0, want));
        }
    }
}
