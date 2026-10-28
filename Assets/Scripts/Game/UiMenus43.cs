// NIGHT43 builder U1 round 3 (design.n43.screens43, a view row: the full-screen UI; GG's G6 names "menus"; the lead's round-3 card, DESIGN-RULES U-3).
// With screens43 on (and hud33's front language, UiFront33, under it), the menu screens in the dusk instrument's language (Research/2026-10-16-wave43/U1.md):
//   - EVERY MENU ITEM (TITLE, PAUSED, the LEVEL SELECT rows, RESULTS' CONTINUE / RETRY LEVEL / TITLE): at rest the label in MUTED bone on nothing; the
//     focused item grows to 1.08 (ui-art-web B6), stands on a raised INK-SOFT plate (its own slanted plate shape) with a CYAN edge bar on its left, its label
//     white (UiFront33's cyan label and two slash ticks give way to this). A locked level row stays muted, never lit.
//   - PAUSED: the game behind it blurred once and multiplied toward the dusk violet (UiBlur43, captured on the frame PAUSED opens with the screen hidden),
//     in place of the INK sheet; the items on an INK blade panel sized to them (leaning with the menu column).
//   - TITLE: the column (PLAY, the run setup card, PROGRESS / SETTINGS / QUIT) on an INK blade panel sized to it; the living world stays behind (no capture:
//     the attract moves).
//   - LEVEL SELECT: the rows and their headers on an INK blade panel sized to them.
//   - the panels enter with their screens (the NIGHT18 entrance: along the slant, before the items' cascade).
// Logic, input and pad navigation unchanged (paint and panels only: MenuStepItems calls Menu43Paint where it called Front33Paint). Off: nothing is built
// and the menus are NIGHT33's. Numbers: UnityTuning.Ui43.cs (Menu43*).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>NIGHT43 menus43's statics for the reels.</summary>
    public static class Menus43Probe
    {
        public static bool Built, PauseBackdrop;
        public static string FocusNow = "";
        public static float FocusScale;
        public static int PauseOpens;
    }

    public sealed partial class Ui
    {
        bool m43On;
        readonly Dictionary<MenuItemView, Image> m43Edge = new Dictionary<MenuItemView, Image>();
        RawImage m43PauseBack, m43PauseScan;
        Image m43PausePanel, m43TitlePanel, m43LevelsPanel;
        CanvasGroup m43PauseHide;
        bool m43PauseWas, m43TitleWas, m43LevelsWas;
        int m43PauseFrame = -1;

        /// <summary>From BuildFront33 (every screen is built; front33 on): the panels and the pause backdrop with screens43 on.</summary>
        void BuildMenus43()
        {
            m43On = front33 && Screens43On;
            if (!m43On) return;
            Menus43Probe.Built = true;
            Color panel = UiTheme.A(UiTheme.InkSolid, UnityTuning.Screens43PanelAlpha);
            // PAUSED: the backdrop in place of its sheet, then its panel
            var prt = (RectTransform)pause.transform;
            Image sheet = pause.GetComponent<Image>();
            if (sheet != null) sheet.color = UiTheme.A(sheet.color, 0f); // its rest is 0 now: the entrance's sheet fade reads it
            m43PauseBack = UiBlur43.Backdrop(prt, out m43PauseScan);
            m43PauseBack.raycastTarget = false;
            m43PauseHide = pause.GetComponent<CanvasGroup>();
            if (m43PauseHide == null) m43PauseHide = pause.AddComponent<CanvasGroup>();
            m43PausePanel = Panel43(pause, panel);
            m43TitlePanel = Panel43(title, panel);
            if (levelsScreen26 != null) m43LevelsPanel = Panel43(levelsScreen26, panel);
        }

        /// <summary>A screen's INK blade panel (behind its items, over its backdrop / slab), entering with the screen.</summary>
        Image Panel43(GameObject screen, Color c)
        {
            var mid = new Vector2(0.5f, 0.5f);
            Image p = UiKit43.Blade(screen.transform, "Panel 43", c, mid, mid, Vector2.zero, new Vector2(10, 10));
            p.raycastTarget = false;
            Transform back = screen.transform.Find("Backdrop 43");
            p.rectTransform.SetSiblingIndex(back != null ? back.GetSiblingIndex() + 1 : 0);
            UiSkew.Apply(UiKit43.Scan(p.rectTransform, "Scan", UnityTuning.Ui43ScanAlpha), UiKit43.SlantTan);
            MenuPart18Add(screen, p);
            p.gameObject.SetActive(false);
            return p;
        }

        /// <summary>Size `panel` to the items (their rest places: MenuItemView.home) plus `extraTop` px above, padded.</summary>
        void Fit43(Image panel, IList<RectTransform> items, float extraTop)
        {
            float x0 = float.MaxValue, x1 = float.MinValue, y0 = float.MaxValue, y1 = float.MinValue;
            int n = 0;
            for (int i = 0; i < items.Count; i++)
            {
                RectTransform rt = items[i];
                if (rt == null || !rt.gameObject.activeSelf) continue;
                Vector2 c = menuItemOf.TryGetValue(rt.gameObject, out MenuItemView it) ? it.home : rt.anchoredPosition;
                Vector2 s = rt.sizeDelta;
                // the menu column leans: an item's left edge is its centre minus half its width; the panel (a blade) leans the same way
                x0 = Mathf.Min(x0, c.x - s.x * 0.5f - (c.y) * UiKit43.SlantTan);
                x1 = Mathf.Max(x1, c.x + s.x * 0.5f - (c.y) * UiKit43.SlantTan);
                y0 = Mathf.Min(y0, c.y - s.y * 0.5f);
                y1 = Mathf.Max(y1, c.y + s.y * 0.5f);
                n++;
            }
            if (n == 0) { panel.gameObject.SetActive(false); return; }
            float pad = UnityTuning.Menu43Pad;
            x0 -= pad + UnityTuning.Menu43EdgeRoom;
            x1 += pad + UnityTuning.Menu43GrowRoom;
            y0 -= pad;
            y1 += pad + extraTop;
            float cy = (y0 + y1) * 0.5f;
            // x0 / x1 are on the column's line at y = 0; the blade's centre row is at cy: shift by the lean there
            panel.rectTransform.anchoredPosition = new Vector2((x0 + x1) * 0.5f + cy * UiKit43.SlantTan, cy);
            panel.rectTransform.sizeDelta = new Vector2(x1 - x0, y1 - y0);
            panel.gameObject.SetActive(true);
            foreach (Transform t in panel.transform) if (t.TryGetComponent(out RawImage ri)) UiKit43.FitScan(ri);
        }

        /// <summary>MenuStepItems' colours for an item with screens43 on (lit 0 rest .. 1 focused, eased): the 1.08 grow, the raised plate, the cyan edge.</summary>
        void Menu43Paint(MenuItemView it, float lit)
        {
            if (!front33Ticks.TryGetValue(it, out Image[] ticks)) { Front33Item(it); front33Ticks.TryGetValue(it, out ticks); }
            if (ticks != null) ticks[0].color = ticks[1].color = UiTheme.A(UiStyle33.Cyan, 0f); // the edge replaces the two slash ticks
            if (!m43Edge.TryGetValue(it, out Image edge))
            {
                edge = UiStyle33.Line(it.rt, "Edge 43", UiTheme.A(UiStyle33.Cyan, 0f), new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(-UnityTuning.Menu43EdgeGap, 0), new Vector2(UnityTuning.Menu43EdgeW, it.rt.sizeDelta.y));
                m43Edge[it] = edge;
            }
            bool locked = it.go.TryGetComponent(out Button b) && !b.interactable;
            float l = locked ? 0f : lit;
            it.plate.color = UiTheme.A(UiTheme.InkSoft, UnityTuning.Menu43PlateAlpha * l);
            edge.color = UiTheme.A(UiStyle33.Cyan, l);
            Color rest = UiKit43.Muted;
            it.label.color = locked ? UiTheme.A(rest, UnityTuning.Menu43LockedAlpha) : Color.Lerp(rest, UiTheme.White, l);
            float s = 1f + (UnityTuning.Menu43FocusScale - 1f) * l;
            var sv = new Vector3(s, s, 1f);
            if (it.rt.localScale != sv) it.rt.localScale = sv;
            if (lit > 0.5f)
            {
                Front33FocusNow = it.go.name;
                Menus43Probe.FocusNow = it.go.name;
                Menus43Probe.FocusScale = s;
            }
        }

        /// <summary>Every frame (RenderFront33): a screen that just opened sizes its panel; PAUSED captures its backdrop (hidden that frame).</summary>
        void RenderMenus43()
        {
            if (!m43On) return;
            bool p = pause.activeInHierarchy;
            if (p && !m43PauseWas)
            {
                Fit43(m43PausePanel, pauseColumn, 0f);
                m43PauseHide.alpha = 0f;
                m43PauseFrame = Time.frameCount;
                m43PauseBack.texture = null;
                m43PauseBack.color = UiBlur43.Tint(false);
                Menus43Probe.PauseBackdrop = false;
                Menus43Probe.PauseOpens++;
                game.StartCoroutine(UiBlur43.Capture(tex =>
                {
                    if (m43PauseBack == null) return;
                    m43PauseBack.texture = tex;
                    m43PauseBack.color = UiBlur43.Tint(tex != null);
                    Menus43Probe.PauseBackdrop = tex != null;
                }));
            }
            else if (p && m43PauseHide.alpha < 1f && Time.frameCount > m43PauseFrame) m43PauseHide.alpha = 1f;
            m43PauseWas = p;

            bool t = title.activeInHierarchy;
            if (t && !m43TitleWas)
            {
                m43TitleCol.Clear();
                if (titlePlay != null) m43TitleCol.Add((RectTransform)titlePlay.transform);
                if (titleResume != null) m43TitleCol.Add((RectTransform)titleResume.transform);
                m43TitleCol.AddRange(titleColumn);
                Fit43(m43TitlePanel, m43TitleCol, 0f);
            }
            m43TitleWas = t;

            bool lv = levelsScreen26 != null && levelsScreen26.activeInHierarchy;
            if (lv && !m43LevelsWas && m43LevelsPanel != null)
            {
                m43LevelCol.Clear();
                foreach (LevelRow26 r in levelRows26) if (r.item != null) m43LevelCol.Add(r.item.rt);
                Fit43(m43LevelsPanel, m43LevelCol, UnityTuning.Menu43HeaderRoom);
            }
            m43LevelsWas = lv;
        }

        /// <summary>The ui43 reel: focus the shown menu item named `name` (as a pad's move would); false when none shows.</summary>
        public bool Menus43Select(string name)
        {
            for (int i = 0; i < menuItems.Count; i++)
            {
                MenuItemView it = menuItems[i];
                if (it.go.name != name || !it.go.activeInHierarchy) continue;
                if (UnityEngine.EventSystems.EventSystem.current == null) return false;
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(it.go);
                return true;
            }
            return false;
        }

        readonly List<RectTransform> m43TitleCol = new List<RectTransform>(10), m43LevelCol = new List<RectTransform>(12);
    }
}
