// NIGHT8 U2-04: the SETTINGS screen (menus.js settingsScreen: tabs over the rows of config.ui.settings, BACK; reachable
// from TITLE and PAUSED; Esc goes back). Every change saves and applies at once (GameSettings).
// NIGHT13 U3-26.8 (agent S): pad / keyboard navigation (each row one NavRow: up / down walk the rows, left / right change a
// range or a select and switch the tab on a toggle, a tab or BACK; confirm flips a toggle, steps a range or a select).
// NIGHT17 U3-30.8 (builder N; GG: "way too cluttered and has way too many options"): rebuilt.
// - At most 5 tabs: GAME / CONTROLS / VIDEO / ACCESSIBILITY, GAMEPAD while a pad is connected; the LAB only in a developer
//   build (MenuRules.Tabs). Rows regrouped by UnityTuning.MenuSettingsRows (MenuRules.TabOf): 50 rows in 7 tabs became at
//   most 8 a tab, the rest behind an ADVANCED row that opens them. UI SCALE gives way to HUD SCALE (settings.hudScale,
//   builder H applies it); RESET TAB is gone.
// - CONTROLS holds the mouse's rows, AIM ASSIST (from PAUSED) and KEYS (the rebinding screen, UiControls.cs, which lists the
//   pad's buttons after a pad input: u-small2-n16's 99db5e1 idea, PAUSED's keys card is gone).
// - A range is a slider (a slanted bar: drag it or click it; left / right step it), a select "<  VALUE  >", a toggle a chip
//   (CYAN ON / INK OFF). The focused row's label turns WHITE (the rest MUTED) on the focus frame; one footer line under the
//   rows says what it does (at most 8 words: MenuRules.FootOf, never the web's long description).
// - Mouse hover selects (UiNav's NavHover); a click on a row presses it.
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        /// <summary>The settings Unity applies now (plus the Unity-only rows); MenuRules.TabOf decides where each shows.</summary>
        static readonly HashSet<string> Shown = new HashSet<string>
        {
            "sensitivity", "invertY", "aimAssist", "snapStrength", "adsSensitivity", // aim
            "fovDeg", "fpsCap", "vsync", // video
            "quality", "filmGrain", "toneMapping", // NIGHT12 U3-25.2: PostFx reads them (post.js / quality.js)
            "renderScale", SettingsView.WindowModeKey, SettingsView.ResolutionKey, // NIGHT13 U3-26.8 (WINDOW MODE replaces FULLSCREEN)
            "crosshairColor", "damageDirection", "showPrices", // hud
            "xray", "damageNumbers", // NIGHT12: the enemy shader's x-ray pass (U3-25.3), damageNumbers.js
            UnityTuning.HudScaleSetting, // NIGHT17: HUD SCALE (builder H applies it); UI SCALE's row is gone
            "effectsIntensity", "reducedMotion", "hurtVignette", "cameraRoll", "flashIntensity", // access
            "slideMode", "arclineMode", // NIGHT13 U3-26.8: holdToggle.js (U3-26.7 reads them)
            "padAimAssist", "padSensX", "padSensY", "padCurve", "padDeadzoneInner", "padDeadzoneOuter", "padInvertY", "padVibration", // NIGHT13 U3-26.8: the GAMEPAD tab
            UnityTuning.PadRumbleSetting, // NIGHT15 U3-28.8: RUMBLE (the motors' strength; design.pad)
            LabMath.CloseKillHeal, LabMath.Freshness, LabMath.StyleRamp, LabMath.ReadyFromOne, LabMath.SquadTighten, // the LAB (NIGHT8 U2-16)
            LabMath.UltraHealthKey, // NIGHT11 U3-24.9: ULTRA HEALTH (off by default)
            UnityTuning.RunLogSetting, // NIGHT13 U3-26.12: RUN LOG (a local log of each run; GAME tab)
            UnityTuning.RecapSetting, // NIGHT14 U3-27.9: RECAP (WHAT GOT YOU; GAME tab)
            FeelKeys.HitFreezeSetting, FeelKeys.SlowMoSetting, // NIGHT16 U3-29.3: HIT FREEZE, SLOW-MO (ACCESS tab; design.impact)
            FeelKeys.MoveSlideChargeSetting, FeelKeys.MoveSlideDecaySetting, FeelKeys.MoveHopFalloffSetting, // NIGHT16: the LAB's movement rows (design.move)
            UnityTuning.FovKickSetting, UnityTuning.CameraShakeSetting, // NIGHT18: FOV KICK, CAMERA SHAKE (ACCESS; builder N places them)
            UnityTuning.FeelABSetting, // NIGHT18: the LAB's FEEL N18 (FeelAB)
            UnityTuning.AssistSetting, // NIGHT20 E2: ASSIST (ACCESS; Hades' God Mode, design.adds.assist)
            UnityTuning.PadSprintModeSetting, // NIGHT20 row 6: L3 SPRINT TOGGLE / HOLD (GAMEPAD)
            UnityTuning.TextSize47Setting, // NIGHT47 F: TEXT SIZE (ACCESS; its item exists only with design.n47.hud47 on)
        };

        // NIGHT30 item 8 (builder V): the CRT rows (NIGHT29 registered them, never listed them), PIXEL SIZE and HUD FLICKER, shown unless -skygrindOff30
        // (a row shows only when its store item exists too: the CRT's with design.n29.crt, the two new ones with design.n30.look30). Right after Shown
        // in this file, so the static initialisers run in this order.
        static readonly bool Shown30 = AddShown30();

        static bool AddShown30()
        {
            if (!N30Fixes.On) return false;
            Shown.Add(UnityTuning.CrtSetting);
            Shown.Add(UnityTuning.CrtOverlaySetting);
            Shown.Add(UnityTuning.Pixel30Setting);
            Shown.Add(UnityTuning.HudFx30Setting);
            return true;
        }

        // --- NIGHT20 E2 (builder P): ASSIST on the game's runs (Assist20Run: the rule applied at each run's start, the downs
        // counted while it's on in PlayerPrefs, the run stamped). Built with SETTINGS (the setting's store exists by then).
        Assist20Run assist20;

        /// <summary>NIGHT20 E2: ASSIST on the runs (null before BuildSettings): RESULTS reads its Stamp.</summary>
        public Assist20Run Assist20 => assist20;

        void BuildAssist20()
        {
            if (game.sim == null || !cfg.Has("design.adds.assist")) return;
            SettingsStore store = game.settings.store;
            assist20 = new Assist20Run(game.sim, () => store.Bool(UnityTuning.AssistSetting),
                () => PlayerPrefs.GetInt(UnityTuning.AssistDownsKey, 0),
                n =>
                {
                    PlayerPrefs.SetInt(UnityTuning.AssistDownsKey, n);
                    PlayerPrefs.Save();
                });
        }

        sealed class SettingRow
        {
            public SettingItem item; // null: a screen row (KEYS, ADVANCED)
            public string key, tab, foot;
            public bool advanced, fold;
            public GameObject go;
            public TextMeshProUGUI label, value;
            public UiShape track, fill, chip;
            public UiChevron prev, next; // a select's chevrons (round 5: INK on the focused row)
            public CanvasGroup group; // NIGHT17 round 1: each row enters along the slant when its tab opens
            public UiEnter enter;
            public double wait;
            public bool entering;
            public Vector2 home;
            public float nudge18; // NIGHT18: the focus slide's progress (0 .. 1, linear in time; eased when drawn)
            public float entered = 1; // the entrance's eased value last drawn (1: at rest)
        }

        readonly GameObject settingsScreen;
        bool settingsOpen;
        string settingsTab;
        bool settingsAdvanced; // the current tab's ADVANCED rows are open
        int settingsEnterIndex; // NIGHT17 round 1: the next entering row's place in the stagger
        bool settingsControlsReturn; // KEYS opened CONTROLS from here: closing it comes back
        readonly List<SettingRow> settingRows = new List<SettingRow>();
        readonly List<(string id, TextMeshProUGUI label, UiShape line)> tabButtons = new List<(string, TextMeshProUGUI, UiShape)>();
        readonly List<string> settingsTabsShown = new List<string>(6);
        bool settingsPadShown, settingsDevShown;
        TextMeshProUGUI labNote; // the LAB tab's note
        TextMeshProUGUI settingsDesc; // the focused row's footer
        SettingRow selectedRow; // the row the navigation has selected (its label WHITE)

        public bool SettingsOpen => settingsOpen;

        public void OpenSettings()
        {
            settingsOpen = true;
            SettingsTabsRefresh(null);
            if (settingsTab == null || !settingsTabsShown.Contains(settingsTab)) settingsTab = settingsTabsShown.Count > 0 ? settingsTabsShown[0] : null;
            ShowTab(settingsTab, enterAll: true); // every row enters (the screen opens)
            shownFlow = (Flow)(-1); // re-pick the screens
        }

        public void CloseSettings()
        {
            SettingsDescStep18(0, true); // NIGHT18: a footer mid-fade goes to rest (the next opening starts from there)
            settingsOpen = false;
            shownFlow = (Flow)(-1);
        }

        Color CrosshairColor(string name) => cfg.Has($"ui.crosshairColors.{name}") ? Look.C(cfg, $"ui.crosshairColors.{name}") : usable;

        void BuildSettings()
        {
            BuildAssist20(); // NIGHT20 E2
            SettingsStore store = game.settings.store;
            var head = Text(settingsScreen.transform, "Head", "SETTINGS", UiTheme.Px(UiTheme.Size.S75), UiTheme.White, TextAlignmentOptions.Left, Vector2.zero, new Vector2(1000, 90), new Vector2(0.5f, 0.5f));
            UiTheme.Style(head, UiTheme.Px(UiTheme.Size.S75), UiTheme.White, UiTheme.Display, shadow: true);
            head.fontStyle = FontStyles.Normal;
            MenuPinLeft(head, MenuLeft(UnityTuning.Settings17HeadY), UnityTuning.Settings17HeadY); // NIGHT17 round 1: by its left edge ("TTINGS")
            settingsScreen.GetComponent<UnityEngine.UI.Image>().color = UiTheme.A(UiTheme.InkSolid, UnityTuning.MenuSheetAlpha); // the world dimmed
            // Round 2: the rows on one solid INK slab (opaque: the world's bright trim never shows through), slanted, cut
            var slabGo = new GameObject("Rows slab", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
            var slabRt = (RectTransform)slabGo.transform;
            slabRt.SetParent(settingsScreen.transform, false);
            slabRt.SetAsFirstSibling();
            slabRt.anchorMin = slabRt.anchorMax = new Vector2(0.5f, 0.5f);
            slabRt.sizeDelta = new Vector2(UnityTuning.SettingsSlabHalfWidth * 2, UnityTuning.SettingsSlabTop - UnityTuning.SettingsSlabBottom);
            slabRt.anchoredPosition = new Vector2(0, (UnityTuning.SettingsSlabTop + UnityTuning.SettingsSlabBottom) * 0.5f);
            var slab = slabGo.GetComponent<UiShape>();
            slab.color = UiTheme.InkSolid;
            slab.raycastTarget = false;
            slab.SetCuts(UnityTuning.ThemeCut * 3, UnityTuning.ThemeCut * 3);

            // The rows of every tab (one tab shown at a time), in the table's order (the LAB's in the store's)
            foreach (var r in UnityTuning.MenuSettingsRows)
            {
                if (r.key == UnityTuning.KeysRowKey)
                {
                    settingRows.Add(SettingsScreenRow(r.key, r.tab, r.label, r.foot, ">", () => SettingsOpenKeys()));
                    continue;
                }
                SettingItem item = store.Item(r.key);
                if (item == null || !Shown.Contains(r.key)) continue;
                settingRows.Add(SettingsItemRow(item, r.tab));
            }
            foreach (SettingItem item in store.items)
            {
                if (MenuRules.TabOf(item.key, item.tab) != UnityTuning.LabTabId || !Shown.Contains(item.key)) continue;
                settingRows.Add(SettingsItemRow(item, UnityTuning.LabTabId));
            }
            // ADVANCED: one fold row per tab that has rows behind it
            foreach (var (id, _) in UnityTuning.MenuSettingsTabs)
            {
                bool any = false;
                foreach (SettingRow r in settingRows) any |= r.tab == id && r.advanced;
                if (!any) continue;
                SettingRow fold = SettingsScreenRow("advanced:" + id, id, UnityTuning.AdvancedLabel, "MORE ROWS FOR THIS TAB", "+", () =>
                {
                    settingsAdvanced = !settingsAdvanced;
                    ShowTab(settingsTab, focusFirst: false); // the focus stays on ADVANCED; the rows behind it enter
                });
                fold.fold = true;
                settingRows.Add(fold);
            }

            // The tabs that have a row, as text tabs (the current one WHITE over a 4 px CYAN bar, the rest MUTED)
            foreach (var (id, label) in UnityTuning.MenuSettingsTabs)
            {
                bool any = false;
                foreach (SettingRow r in settingRows) any |= r.tab == id;
                if (!any) continue;
                string tabId = id;
                var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
                var rt = (RectTransform)go.transform;
                rt.SetParent(settingsScreen.transform, false);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(UnityTuning.SettingsTab17Width, UnityTuning.SettingsTab17Height);
                var hit = go.GetComponent<UiShape>();
                hit.color = UiTheme.A(UiTheme.Cyan, 0f);
                var b = go.AddComponent<UnityEngine.UI.Button>();
                b.transition = UnityEngine.UI.Selectable.Transition.None;
                b.onClick.AddListener(() => ShowTab(tabId)); // ShowTab lights the tab's first row (NIGHT17 round 1)
                float px = UiTheme.Px(UiTheme.Size.S24);
                TextMeshProUGUI t = Text(rt, "Label", label, px, UiTheme.Muted, TextAlignmentOptions.Center, Vector2.zero, rt.sizeDelta, new Vector2(0.5f, 0.5f));
                UiTheme.Style(t, px, UiTheme.Muted, UiTheme.BodyBold);
                t.characterSpacing = UnityTuning.ThemeLabelTracking;
                float tabW = t.GetPreferredValues(label).x + 2 * UnityTuning.SettingsTabPad; // round 3: sized to its word (even gaps)
                rt.sizeDelta = new Vector2(tabW, UnityTuning.SettingsTab17Height);
                t.rectTransform.sizeDelta = rt.sizeDelta;
                var lineGo = new GameObject("On", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
                var lrt = (RectTransform)lineGo.transform;
                lrt.SetParent(rt, false);
                lrt.anchorMin = new Vector2(0.1f, 0);
                lrt.anchorMax = new Vector2(0.9f, 0);
                lrt.pivot = new Vector2(0.5f, 0);
                lrt.sizeDelta = new Vector2(0, UnityTuning.ThemeAccentBar);
                var line = lineGo.GetComponent<UiShape>();
                line.color = UiTheme.Cyan;
                line.raycastTarget = false;
                line.SetCuts(0, 0);
                NavSideOf(go, SwitchTab);
                tabButtons.Add((tabId, t, line));
            }

            labNote = Text(settingsScreen.transform, "LabNote", "LAB RUNS SET NO BEST", UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, TextAlignmentOptions.Left, Vector2.zero, new Vector2(UnityTuning.SettingsRow17Width, 30), new Vector2(0.5f, 0.5f));
            UiTheme.Style(labNote, UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, UiTheme.BodyBold);
            // The focused row's footer (one line, at most 8 words)
            settingsDesc = Text(settingsScreen.transform, "Description", "", UiTheme.Px(UiTheme.Size.S24), UiTheme.Muted, TextAlignmentOptions.Left, new Vector2(0, UnityTuning.SettingsFooterY), new Vector2(UnityTuning.SettingsRow17Width, 36), new Vector2(0.5f, 0.5f));
            UiTheme.Style(settingsDesc, UiTheme.Px(UiTheme.Size.S24), UiTheme.Muted, UiTheme.BodyBold);
            var back = MenuItem(settingsScreen.transform, "BACK", MenuAt(UnityTuning.SettingsBackY, UnityTuning.MenuItemWidth * 0.7f), new Vector2(UnityTuning.MenuItemWidth * 0.7f, UnityTuning.MenuItemHeight), UiTheme.Size.S42, CloseSettings);
            NavSideOf(back.go, SwitchTab);
            // NIGHT18 U3-31.7: the screen's parts enter with it (build 1: the head, the slab, the tabs and the footer were whole
            // on the input frame); BACK enters as an item after them (UiMenus.MenuScreenBegin18)
            MenuPart18Add(settingsScreen, head);
            MenuPart18Add(settingsScreen, slabRt);
            foreach (var (_, tabLabel, _) in tabButtons) MenuPart18Add(settingsScreen, tabLabel.transform.parent);
            MenuPart18Add(settingsScreen, settingsDesc);
            settingsDescGroup18 = settingsDesc.GetComponent<CanvasGroup>(); // added just above
            settingsDescHome18 = settingsDesc.rectTransform.anchoredPosition;
            uiScaleBase = root.GetComponent<UnityEngine.UI.CanvasScaler>().referenceResolution;
            RefreshSettingRows();
        }

        /// <summary>A row's frame: a transparent slanted hit area (click, hover), the label on the left, one NavRow.</summary>
        SettingRow SettingsRowFrame(string key, string tab, string label)
        {
            var go = new GameObject($"Row {key}", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
            var rt = (RectTransform)go.transform;
            rt.SetParent(settingsScreen.transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(UnityTuning.SettingsRow17Width, UnityTuning.SettingsRow17Height);
            var hit = go.GetComponent<UiShape>();
            hit.color = UiTheme.InkSoft; // round 2: each row on its own slanted plate (the focus frame lights it)
            var row = new SettingRow { key = key, tab = tab, go = go, group = go.AddComponent<CanvasGroup>() };
            float px = UiTheme.Px(UiTheme.Size.S24);
            row.label = Text(rt, "Label", label, px, UiTheme.Muted, TextAlignmentOptions.Left, new Vector2(-UnityTuning.SettingsRow17Width * 0.5f + 28 + 280, 0), new Vector2(560, UnityTuning.SettingsRow17Height), new Vector2(0.5f, 0.5f));
            UiTheme.Style(row.label, px, UiTheme.Muted, UiTheme.BodyBold);
            row.value = Text(rt, "Value", "", px, UiTheme.White, TextAlignmentOptions.Right, new Vector2(UnityTuning.SettingsRow17Width * 0.5f - 24 - (UnityTuning.SettingsValueWidth + 60) * 0.5f, 0), new Vector2(UnityTuning.SettingsValueWidth + 60, UnityTuning.SettingsRow17Height), new Vector2(0.5f, 0.5f));
            UiTheme.Style(row.value, px, UiTheme.White, UiTheme.BodyBold);
            return row;
        }

        /// <summary>A row of a SettingItem: a slider (range), "&lt; VALUE &gt;" (select) or a chip (toggle).</summary>
        SettingRow SettingsItemRow(SettingItem item, string tab)
        {
            SettingRow row = SettingsRowFrame(item.key, tab, MenuRules.LabelOf(item.key, item.label));
            row.item = item;
            row.advanced = MenuRules.Advanced(item.key);
            row.foot = MenuRules.FootOf(item.key);
            var rt = (RectTransform)row.go.transform;
            if (item.type == "range")
            {
                float x = UnityTuning.SettingsValueX;
                row.track = SettingsShape(rt, "Track", UiTheme.InkSolid, new Vector2(x, 0), new Vector2(UnityTuning.SettingsSliderWidth, UnityTuning.SettingsSliderHeight));
                row.track.raycastTarget = true;
                row.fill = SettingsShape(row.track.transform, "Fill", UiTheme.Cyan, Vector2.zero, new Vector2(UnityTuning.SettingsSliderWidth, UnityTuning.SettingsSliderHeight));
                // round 3: one face for every value (BODY-BOLD white, inside the row), the slider's too
                SettingItem it = item;
                var slider = row.track.gameObject.AddComponent<SettingsSlider>();
                slider.onValue = t =>
                {
                    double v = it.min + (it.max - it.min) * t;
                    double step = it.fine > 0 ? it.fine : it.step;
                    if (step > 0) v = it.min + System.Math.Round((v - it.min) / step) * step;
                    v = System.Math.Max(it.min, System.Math.Min(it.max, v));
                    SettingsSet(it.key, v);
                    SelectNav(row.go);
                };
            }
            else if (item.type == "toggle")
            {
                row.chip = SettingsShape(rt, "Chip", UiTheme.InkSolid, new Vector2(UnityTuning.SettingsRow17Width * 0.5f - 24 - UnityTuning.SettingsToggleWidth * 0.5f, 0), new Vector2(UnityTuning.SettingsToggleWidth, UnityTuning.SettingsRow17Height - 10));
                row.value.rectTransform.SetParent(row.chip.transform, false);
                row.value.rectTransform.anchoredPosition = Vector2.zero;
                row.value.rectTransform.sizeDelta = new Vector2(UnityTuning.SettingsToggleWidth, UnityTuning.SettingsRow17Height);
                row.value.alignment = TextAlignmentOptions.Center;
            }
            else if (item.type == "select")
            {
                // round 2: drawn chevrons round the value (not ASCII "<  >"), the value centred between them
                float cx = UnityTuning.SettingsRow17Width * 0.5f - 24 - UnityTuning.SettingsSelectWidth * 0.5f - 22;
                row.value.rectTransform.anchoredPosition = new Vector2(cx, 0);
                row.value.rectTransform.sizeDelta = new Vector2(UnityTuning.SettingsSelectWidth, UnityTuning.SettingsRow17Height);
                row.value.alignment = TextAlignmentOptions.Center;
                row.prev = SettingsChevron(rt, false, new Vector2(cx - UnityTuning.SettingsSelectWidth * 0.5f - 14, 0));
                row.next = SettingsChevron(rt, true, new Vector2(cx + UnityTuning.SettingsSelectWidth * 0.5f + 14, 0));
            }
            SettingRow r = row;
            var nav = row.go.AddComponent<NavRow>();
            nav.side = d => RowSide(r, d);
            nav.submit = () => RowSubmit(r);
            return row;
        }

        /// <summary>A screen row (KEYS, ADVANCED): a press runs `act`; left / right switch the tab.</summary>
        SettingRow SettingsScreenRow(string key, string tab, string label, string foot, string value, System.Action act)
        {
            SettingRow row = SettingsRowFrame(key, tab, label);
            row.foot = foot;
            row.value.text = value == ">" ? "" : value;
            if (value == ">") SettingsChevron(row.go.transform, true, new Vector2(UnityTuning.SettingsRow17Width * 0.5f - 24 - UnityTuning.ChevronWidth, 0)); // round 2: drawn
            row.advanced = false;
            var nav = row.go.AddComponent<NavRow>();
            nav.side = SwitchTab;
            nav.submit = act;
            return row;
        }

        UiShape SettingsShape(Transform parent, string name, Color c, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var s = go.GetComponent<UiShape>();
            s.color = c;
            s.raycastTarget = false;
            s.SetCuts(Mathf.Min(UnityTuning.ThemeCut, size.y * 0.4f), Mathf.Min(UnityTuning.ThemeCut, size.y * 0.4f));
            return s;
        }

        /// <summary>KEYS: the rebinding screen (UiControls.cs); closing it comes back to SETTINGS.</summary>
        void SettingsOpenKeys()
        {
            settingsControlsReturn = true;
            CloseSettings();
            OpenControls();
            if (!controlsOpen)
            {
                settingsControlsReturn = false;
                OpenSettings(); // CONTROLS couldn't open here: stay
            }
        }

        void Change(SettingItem item, int dir)
        {
            object v = game.settings.store.values[item.key];
            if (item.type == "range")
            {
                double from = item.key == UnityTuning.PadRumbleSetting && !SettingsPadVibration ? item.min : (double)v; // shown at 0 while VIBRATION is off
                SettingsSet(item.key, SettingsMath.Nudge(item, from, dir));
            }
            else if (item.type == "select") SettingsSet(item.key, SettingsMath.Cycle(item, v, dir));
        }

        bool SettingsPadVibration => !game.settings.store.values.ContainsKey("padVibration") || game.settings.Bool("padVibration");

        /// <summary>Round 2: set a value; RUMBLE is the pad's one feel row (VIBRATION folds into it: 0 is off, above 0 on).</summary>
        void SettingsSet(string key, object value)
        {
            game.settings.Set(key, value);
            if (key == UnityTuning.PadRumbleSetting && value is double d && game.settings.store.values.ContainsKey("padVibration") && game.settings.Bool("padVibration") != d > 0)
                game.settings.Set("padVibration", d > 0);
        }

        /// <summary>Round 2: a select's drawn chevron (CYAN-PALE), left or right of its value.</summary>
        UiChevron SettingsChevron(Transform row, bool right, Vector2 pos)
        {
            var go = new GameObject(right ? "Next" : "Prev", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiChevron));
            var rt = (RectTransform)go.transform;
            rt.SetParent(row, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(UnityTuning.ChevronWidth, UnityTuning.ChevronHeight);
            var c = go.GetComponent<UiChevron>();
            c.right = right;
            c.color = UiTheme.CyanPale;
            c.raycastTarget = false;
            return c;
        }

        /// <summary>Round 5: a row's colours lit (on the focus' full CYAN fill: the label, the value and the chevrons INK) or at
        /// rest (the label MUTED, the value WHITE, the chevrons CYAN-PALE). A toggle's chip keeps its own (ink, ON cyan).</summary>
        static void SettingsRowLit(SettingRow r, bool lit)
        {
            r.label.color = lit ? UiTheme.InkSolid : UiTheme.Muted;
            if (r.item == null || r.item.type != "toggle") r.value.color = lit ? UiTheme.InkSolid : UiTheme.White;
            if (r.prev != null) r.prev.color = lit ? UiTheme.InkSolid : UiTheme.CyanPale;
            if (r.next != null) r.next.color = lit ? UiTheme.InkSolid : UiTheme.CyanPale;
        }

        /// <summary>Round 5: `go` is a settings row (the focus frame fills it with the full CYAN).</summary>
        bool SettingsIsRow(GameObject go)
        {
            if (go == null) return false;
            for (int i = 0; i < settingRows.Count; i++) if (settingRows[i].go == go) return true;
            return false;
        }

        /// <summary>menus.js padChange / the side branch: left / right change a range or a select; on a toggle, the tab.</summary>
        void RowSide(SettingRow row, int dir)
        {
            if (row.item != null && (row.item.type == "range" || row.item.type == "select")) Change(row.item, dir);
            else SwitchTab(dir);
        }

        /// <summary>menus.js padInput confirm: a toggle flips; a range or a select steps up (padChange(el, 1)).</summary>
        void RowSubmit(SettingRow row)
        {
            if (row.item == null) return;
            if (row.item.type == "toggle") SettingsSet(row.item.key, !game.settings.Bool(row.item.key));
            else Change(row.item, 1);
        }

        /// <summary>The next / previous tab shown (wrapping), its button selected (menus.js: left / right switch the settings tab).</summary>
        void SwitchTab(int dir)
        {
            if (settingsTabsShown.Count == 0) return;
            int at = Mathf.Max(0, settingsTabsShown.IndexOf(settingsTab));
            int next = ((at + dir) % settingsTabsShown.Count + settingsTabsShown.Count) % settingsTabsShown.Count;
            ShowTab(settingsTabsShown[next]); // lights the new tab's first row (NIGHT17 round 1: by any device)
        }

        /// <summary>NIGHT15: SETTINGS opened on one tab (the Player's capture tour, ShotTour). The LAB opens only in a developer
        /// build (else GAME); GAMEPAD opens even without a pad (asked for by name).</summary>
        public void OpenSettingsTab(string id)
        {
            OpenSettings();
            if (id == UnityTuning.LabTabId && !game.devMode) id = settingsTabsShown.Count > 0 ? settingsTabsShown[0] : id;
            SettingsTabsRefresh(id);
            ShowTab(id);
        }

        /// <summary>The tabs shown now (MenuRules.Tabs: a pad, a developer build), laid out left to right; `also` stays shown.</summary>
        void SettingsTabsRefresh(string also)
        {
            settingsPadShown = PlayerDevices.PadConnected;
            settingsDevShown = game.devMode;
            bool hasLab = false;
            foreach (SettingRow r in settingRows) hasLab |= r.tab == UnityTuning.LabTabId;
            List<string> tabs = MenuRules.Tabs(settingsPadShown, settingsDevShown, hasLab);
            settingsTabsShown.Clear();
            foreach (var (id, _, _) in tabButtons)
                if (tabs.Contains(id) || id == also) settingsTabsShown.Add(id);
            float x = MenuLeft(UnityTuning.SettingsTabsY); // NIGHT17 round 1: the tab row from the column's left edge
            foreach (var (id, label, _) in tabButtons)
            {
                GameObject go = label.transform.parent.gameObject;
                bool on = settingsTabsShown.Contains(id);
                go.SetActive(on);
                if (!on) continue;
                float tw = ((RectTransform)go.transform).sizeDelta.x; // round 3: each tab as wide as its word, one even gap
                ((RectTransform)go.transform).anchoredPosition = new Vector2(x + tw * 0.5f, UnityTuning.SettingsTabsY);
                x += tw + UnityTuning.SettingsTabGap;
            }
        }

        /// <summary>Per frame while SETTINGS is open (UiNav): a pad plugged in or out, or the developer flag, re-picks the tabs.</summary>
        void SettingsTick(double frameDt)
        {
            if (!settingsOpen) return;
            SettingsStepRows(frameDt);
            SettingsTabLineStep18(frameDt); // NIGHT18
            SettingsDescStep18(frameDt, false);
            if (PlayerDevices.PadConnected == settingsPadShown && game.devMode == settingsDevShown) return;
            SettingsTabsRefresh(settingsTab);
            if (!settingsTabsShown.Contains(settingsTab)) ShowTab(settingsTabsShown.Count > 0 ? settingsTabsShown[0] : null);
        }

        /// <summary>NIGHT17 round 1: the rows entering (a tab opened, ADVANCED opened): alpha and a slide along the slant, one
        /// after another (MenuEnterStagger); written only while a row moves.</summary>
        void SettingsStepRows(double frameDt)
        {
            bool still = game.settings.View.reducedMotion;
            bool feel = FeelAB.On; // NIGHT18: the focused row slides along the slant (NIGHT17: it only changed colour)
            float step = (float)(frameDt / UnityTuning.MenuFocusTime18);
            foreach (SettingRow r in settingRows)
            {
                if (!r.go.activeSelf) continue;
                float target = feel && r == selectedRow ? 1f : 0f;
                bool nudging = r.nudge18 != target;
                if (!r.entering && !nudging) continue;
                if (r.entering)
                {
                    float e;
                    if (r.wait > 0)
                    {
                        r.wait -= frameDt;
                        e = 0;
                    }
                    else e = r.enter.Step(frameDt);
                    if (r.wait <= 0 && !r.enter.Running) r.entering = false;
                    if (still && r.wait <= 0) e = 1;
                    r.group.alpha = Mathf.Clamp01(e);
                    r.entered = e;
                }
                if (nudging) r.nudge18 = still ? target : Mathf.MoveTowards(r.nudge18, target, step);
                Vector2 nudge = r.nudge18 <= 0 ? Vector2.zero
                    : target > 0 ? MenuMotion18.FocusNudge(r.nudge18, UnityTuning.SettingsRowNudge18)
                    : MenuMotion18.FocusNudge(1f, UnityTuning.SettingsRowNudge18) * (1f - UiTween.OutCubic(1f - r.nudge18));
                float d = (1 - r.entered) * UnityTuning.ThemeEnterDistance;
                ((RectTransform)r.go.transform).anchoredPosition = r.home + nudge + new Vector2(-d, -d * MenuSlantK);
            }
        }

        /// <summary>The current tab's first shown row (null: none): where the focus lands when a tab opens.</summary>
        GameObject SettingsFirstRow()
        {
            SettingRow best = null;
            foreach (SettingRow r in settingRows)
                if (r.tab == settingsTab && r.go.activeSelf && (best == null || r.home.y > best.home.y)) best = r;
            return best?.go;
        }

        /// <summary>Show tab `id`: its rows placed (newly shown ones enter along the slant; all of them with `enterAll`), the
        /// tab lit, and (NIGHT17 round 1, by any device) the focus on its first row unless `focusFirst` is false.</summary>
        void ShowTab(string id, bool focusFirst = true, bool enterAll = false)
        {
            if (id != settingsTab) settingsAdvanced = false; // a new tab opens folded
            settingsTab = id;
            // NIGHT18: FOV KICK / CAMERA SHAKE up front on ACCESSIBILITY with FeelAB on (MenuRules.Advanced(key, n18))
            foreach (SettingRow r in settingRows) if (r.item != null && r.tab == id) r.advanced = MenuRules.Advanced(r.key, FeelAB.On);
            foreach (SettingRow r in settingRows) if (r.tab != id && r.go.activeSelf) r.go.SetActive(false);
            // the front rows (at most 8), then the ADVANCED row, then (open) the rows behind it
            settingsEnterIndex = 0;
            float y = UnityTuning.SettingsFirstRowY;
            y = SettingsPlace(id, 0, y, enterAll);
            y = SettingsPlace(id, 1, y, enterAll);
            y = SettingsPlace(id, 2, y, enterAll);
            if (focusFirst && settingsOpen)
            {
                GameObject first = SettingsFirstRow();
                if (first != null) SelectNav(first);
            }
            if (labNote != null)
            {
                labNote.gameObject.SetActive(id == UnityTuning.LabTabId);
                labNote.rectTransform.anchoredPosition = new Vector2(0, y);
            }
            foreach (var (tid, label, line) in tabButtons)
            {
                bool on = tid == id;
                label.color = on ? UiTheme.White : UiTheme.Muted;
                line.enabled = on;
                if (!on || id == settingsTabShown18) continue;
                // NIGHT18: a tab switch draws the new tab's underline out from its centre (ease-out-back; NIGHT17: at once).
                // Opening SETTINGS isn't a switch: the line enters with its tab.
                bool animate = FeelAB.On && settingsTabShown18 != null && settingsOpen && !enterAll;
                settingsTabLine18 = line;
                settingsTabLineAge18 = animate ? 0 : double.MaxValue;
                line.rectTransform.localScale = new Vector3(animate ? 0f : 1f, 1, 1);
            }
            settingsTabShown18 = id;
        }

        string settingsTabShown18;
        UiShape settingsTabLine18;
        double settingsTabLineAge18 = double.MaxValue;

        /// <summary>NIGHT18: the switched-to tab's underline growing out (written only while it grows).</summary>
        void SettingsTabLineStep18(double frameDt)
        {
            if (settingsTabLine18 == null || settingsTabLineAge18 >= UnityTuning.ThemeInTime) return;
            settingsTabLineAge18 += frameDt;
            bool still = game.settings.View.reducedMotion;
            float e = !FeelAB.On || still || settingsTabLineAge18 >= UnityTuning.ThemeInTime ? 1f : Mathf.Max(0f, MenuMotion18.Enter(settingsTabLineAge18));
            settingsTabLine18.rectTransform.localScale = new Vector3(e, 1, 1);
        }

        /// <summary>Tab `id`'s rows of one kind (0 the front rows, 1 the ADVANCED row, 2 the rows behind it) from `y` down, one
        /// pitch apart on half the slant; the next row's y.</summary>
        float SettingsPlace(string id, int kind, float y, bool enterAll)
        {
            foreach (SettingRow r in settingRows)
            {
                if (r.tab != id) continue;
                int k = r.fold ? 1 : r.advanced ? 2 : 0;
                if (k != kind) continue;
                bool on = kind != 2 || settingsAdvanced;
                bool was = r.go.activeSelf;
                if (was != on) r.go.SetActive(on);
                if (!on) continue;
                // the full slant (NIGHT17 round 1: the rows drifted on half of it while the buttons leaned on all of it)
                r.home = new Vector2(0, y); // round 2: labels and values at one x (the slant lives on the slab's edge and each plate)
                ((RectTransform)r.go.transform).anchoredPosition = r.home;
                r.nudge18 = 0; // NIGHT18: placed at rest; the focused one slides out again
                r.entered = 1;
                if (!was || enterAll)
                {
                    // NIGHT18: a quick cascade, each row a 60 px slide from alpha 0 (round 2: NIGHT17's 420 ms "too slow"; round 3:
                    // one group read as "the whole list at once, faint"); NIGHT17: 30 ms apart over ThemeInTime
                    bool feel = FeelAB.On;
                    r.enter.In(feel ? UnityTuning.SettingsRowIn18 : UnityTuning.ThemeInTime);
                    r.wait = feel ? MenuMotion18.RowWait(settingsEnterIndex) : settingsEnterIndex * UnityTuning.MenuEnterStagger;
                    settingsEnterIndex++;
                    r.entering = true;
                    r.group.alpha = 0;
                }
                if (r.fold)
                {
                    r.label.text = settingsAdvanced ? UnityTuning.AdvancedOpenLabel : UnityTuning.AdvancedLabel;
                    r.value.text = settingsAdvanced ? "-" : "+";
                }
                y -= UnityTuning.SettingsRowPitch;
            }
            return y;
        }

        /// <summary>The current tab's button (SETTINGS' primary control: the navigation lands on it when the screen opens).</summary>
        GameObject CurrentTabButton()
        {
            foreach (var (tid, label, _) in tabButtons) if (tid == settingsTab) return label.transform.parent.gameObject;
            return tabButtons.Count > 0 ? tabButtons[0].label.transform.parent.gameObject : null;
        }

        /// <summary>The navigation selected `go`: a settings row's label turns WHITE and its footer shows (UiNav calls it).</summary>
        void SettingsSelected(GameObject go)
        {
            SettingRow row = null;
            if (go != null) for (int i = 0; i < settingRows.Count; i++) if (settingRows[i].go == go) row = settingRows[i];
            if (row == selectedRow) return;
            if (selectedRow != null) SettingsRowLit(selectedRow, false);
            selectedRow = row;
            if (row != null) SettingsRowLit(row, true); // round 5: INK on the full CYAN fill
            if (settingsDesc == null) return;
            string foot = row?.foot ?? "";
            bool swap = settingsDesc.text != foot;
            settingsDesc.text = foot;
            // NIGHT18: round 2's M9: the footer came back whole on a focus move (ui-settings f98): a new line fades in along
            // the slant, unless the screen's own entrance is still bringing it in
            if (swap && foot.Length > 0 && FeelAB.On && settingsOpen && settingsDescGroup18 != null && settingsDescGroup18.alpha >= 1f)
            {
                settingsDescAge18 = 0;
                settingsDescSwap18 = true;
                settingsDescGroup18.alpha = 0;
            }
        }

        CanvasGroup settingsDescGroup18;
        Vector2 settingsDescHome18;
        double settingsDescAge18;
        bool settingsDescSwap18;

        /// <summary>NIGHT18: the footer's swap fade (frame time, only while it runs); `end` puts it at rest at once.</summary>
        void SettingsDescStep18(double frameDt, bool end)
        {
            if (!settingsDescSwap18) return;
            settingsDescAge18 += frameDt;
            bool done = end || !FeelAB.On || settingsDescAge18 >= UnityTuning.ThemeInTime;
            settingsDescGroup18.alpha = done ? 1f : MenuMotion18.Alpha(settingsDescAge18);
            bool still = game.settings.View.reducedMotion;
            settingsDesc.rectTransform.anchoredPosition = settingsDescHome18 + (done || still ? Vector2.zero : MenuMotion18.Offset(settingsDescAge18));
            if (done) settingsDescSwap18 = false;
        }

        void RefreshSettingRows()
        {
            ApplyUiScale();
            if (settingRows.Count == 0) return;
            foreach (SettingRow r in settingRows)
            {
                if (r.item == null) continue;
                object v = game.settings.store.values[r.item.key];
                if (r.item.type == "toggle")
                {
                    bool on = (bool)v;
                    r.value.text = on ? "ON" : "OFF";
                    r.value.color = on ? UiTheme.Cyan : UiTheme.Muted; // round 5: ON is cyan text on ink (the focused row is the one cyan fill)
                    r.chip.color = UiTheme.InkSolid;
                }
                else if (r.item.type == "range")
                {
                    double d = (double)v;
                    if (r.item.key == UnityTuning.PadRumbleSetting && !SettingsPadVibration) d = r.item.min; // VIBRATION off: RUMBLE at 0
                    r.value.text = r.item.key == UnityTuning.PadRumbleSetting && d <= 0 ? "OFF" : MenuRules.RangeShown(d, r.item.fine > 0 ? r.item.fine : r.item.step, UnityTuning.SettingsShownScale.TryGetValue(r.item.key, out double k) ? k : 1) + (UnityTuning.SettingsShownSuffix.TryGetValue(r.item.key, out string suffix) ? suffix : ""); // one format (round 5: HUD SCALE in %)
                    double span = r.item.max - r.item.min;
                    r.fill.Fill = span > 0 ? (float)((d - r.item.min) / span) : 1f;
                }
                else r.value.text = r.item.OptionLabel(v); // the chevrons are drawn (round 2)
            }
        }

        // --- UI SCALE (config.ui.settings uiScale, in %) ------------------------------------------------------------------
        Vector2 uiScaleBase; // the canvas scaler's reference resolution at 100%
        float shownUiScale = -1;
        Vector2 shownUiPixels;

        /// <summary>
        /// UI SCALE: the canvas' reference resolution / scale (the HUD, the toasts, the markers grow with it), the menus layer
        /// scaled back. NIGHT17: its row is gone (HUD SCALE replaces it), so a saved UI SCALE no longer applies (100%): a
        /// value nobody can see or change must not stay in force.
        /// </summary>
        void ApplyUiScale()
        {
            if (uiScaleBase == Vector2.zero) return; // before BuildSettings
            double pct = MenuRules.TabOf("uiScale", "hud") != null ? game.settings.View.uiScale : UnityTuning.UiScalePercent;
            float s = pct > 0 ? (float)(pct / UnityTuning.UiScalePercent) : 1;
            Vector2 pixels = root.rect.size * (canvas.scaleFactor > 0 ? canvas.scaleFactor : 1);
            pixels = new Vector2(Mathf.Round(pixels.x), Mathf.Round(pixels.y)); // whole pixels: no re-apply on float noise
            if (s == shownUiScale && pixels == shownUiPixels) return;
            shownUiScale = s;
            shownUiPixels = pixels;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.referenceResolution = uiScaleBase / s;
            float refH = uiScaleBase.y / s;
            Vector2 scaled = pixels.y > 0 ? new Vector2(pixels.x / pixels.y * refH, refH) : Vector2.zero;
            menus.localScale = new Vector3(1 / s, 1 / s, 1);
            menus.sizeDelta = scaled * (s - 1);
        }

        /// <summary>A small button (CONTROLS' and WEAPONS' chips): a solid INK-SOFT cut plate (no translucent grey), a label.</summary>
        TextMeshProUGUI SmallButton(Transform parent, string label, Vector2 pos, Vector2 size, System.Action onClick)
        {
            var img = Img(parent, label.Length > 0 ? label : "Toggle", UiTheme.InkSoft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            img.sprite = UiTheme.Cut;
            img.type = UnityEngine.UI.Image.Type.Sliced;
            img.raycastTarget = true;
            var b = img.gameObject.AddComponent<UnityEngine.UI.Button>();
            b.transition = UnityEngine.UI.Selectable.Transition.None; // the focus frame shows it
            b.onClick.AddListener(() => onClick());
            float px = UiTheme.Px(UiTheme.Size.S18);
            TextMeshProUGUI t = Text(img.transform, "Label", label, px, UiTheme.White, TextAlignmentOptions.Center, Vector2.zero, size, new Vector2(0.5f, 0.5f));
            UiTheme.Style(t, px, UiTheme.White, UiTheme.BodyBold);
            t.characterSpacing = 4;
            return t;
        }
    }

    /// <summary>NIGHT17 round 2: a drawn chevron (a solid triangle pointing left or right in its rect), for the selectors.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UiChevron : UnityEngine.UI.MaskableGraphic
    {
        public bool right;

        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            Color32 c = color;
            float tip = right ? r.xMax : r.xMin, back = right ? r.xMin : r.xMax;
            vh.AddVert(new Vector3(back, r.yMin), c, Vector2.zero);
            vh.AddVert(new Vector3(back, r.yMax), c, Vector2.zero);
            vh.AddVert(new Vector3(tip, r.center.y), c, Vector2.zero);
            vh.AddTriangle(0, 1, 2);
        }
    }

    /// <summary>NIGHT17 U3-30.8: a settings slider's track: a press or a drag sets the value at the pointer (0..1 along it).</summary>
    public sealed class SettingsSlider : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerClickHandler
    {
        public System.Action<float> onValue;

        public void OnPointerDown(PointerEventData e) => At(e);

        /// <summary>The click is the slider's (the row's NavRow above would otherwise step the value again).</summary>
        public void OnPointerClick(PointerEventData e) { }

        public void OnDrag(PointerEventData e) => At(e);

        void At(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left) return;
            var rt = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, e.position, e.pressEventCamera, out Vector2 local)) return;
            Rect r = rt.rect;
            float t = r.width > 0 ? Mathf.Clamp01((local.x - r.xMin) / r.width) : 0;
            onValue?.Invoke(t);
        }
    }
}
