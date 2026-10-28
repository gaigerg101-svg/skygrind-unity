// NIGHT13 U3-26.8 (agent S): CONTROLS (menus.js :505-558, controlRows :101-110; audit-input section 4 row 14), from TITLE
// and PAUSED (their CONTROLS buttons call OpenControls), on agent I's input layer (U3-26.7).
// - KEYBOARD + MOUSE: MenusMath.ControlRows from the live bindings (config.ui.controlGroups with a key cell per rebindable
//   action, MOUSE + FIXED KEYS, TECHNIQUES with {action} filled by the live keys, READING THE HUD), flowed into columns. A
//   key cell waits for a key ("PRESS A KEY"): the keyboard is polled while waiting (KeyCodes.PressedCode; Esc or the pad's
//   B cancels); the key binds through SurvivalGame.bindings (a key in use swaps: "E WAS BUY (HOLD): SWAPPED"; Ctrl,
//   Meta, Esc, F5, F11, F12 refused: "... CAN NOT BE BOUND"), which saves itself (InputPrefs). RESET KEYS: the defaults.
// - GAMEPAD (a tab while a pad is connected, menus.js :534-537): each pad action with its button in the pad's glyph set
//   (Glyphs.PadBindRows over SurvivalGame.padReader.layout); select one, press a pad button (PadReader.rawPressed), the two
//   buttons swap their actions ("LB SWAPPED WITH Y"), the layout goes live and is saved (InputPrefs.SavePadLayout); RESET
//   PAD: the default layout.
// - The binding press is swallowed: SurvivalGame.HotKeys skips its keys while ControlsCapturing (the lead's line), and the
//   menu navigation stays off through the frame after the press (UiNav), so Enter / Space / A don't also click.
// - ControlsRebind (below, no UnityEngine use, tested): a captured key or pad button through the stores, and its note.
// Texts are written when CONTROLS opens, on a binding / layout / glyph-set change and on a capture's start or end, never
// per frame; the per-frame part is the capture's poll (an indexed key walk, a bit test).
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        enum CtlKind { Key, Fixed, Text, Pad }

        sealed class CtlRow
        {
            public CtlKind kind;
            public string action; // Key: the bound action
            public int padRow = -1; // Pad: its row in GamepadMath.LabelOrderV2 (NIGHT19: the v1 table hides SPRINT's)
            public PadAction padAction; // Pad: the action (NIGHT19)
            public int stick = -1; // NIGHT20 row 6: a stick's row on the GAMEPAD page (0 MOVE, 1 LOOK; -1: not one)
            public bool sprintMode; // NIGHT20 row 6: the L3 SPRINT TOGGLE / HOLD row (its cell flips the setting)
            public TextMeshProUGUI label, value;
            public RectTransform bind; // Key / Pad: the cell
        }

        sealed class CtlGroup
        {
            public RectTransform rt;
            public TextMeshProUGUI head;
            public readonly List<CtlRow> rows = new List<CtlRow>();
        }

        GameObject controlsScreen, keysPage, padPage, ctlTabs, resetKeysButton, resetPadButton;
        TextMeshProUGUI ctlHint, ctlNote, ctlTabKeys, ctlTabPad;
        readonly List<CtlGroup> keyGroups = new List<CtlGroup>();
        readonly List<CtlGroup> padGroups = new List<CtlGroup>();
        System.Func<string, string> ctlKeyOf, ctlNoteLabel; // menus.js keyOf (the live key's name) / label (the notes'), made once
        System.Func<PadButtonName, string> ctlGlyph; // a pad button in the connected pad's glyph set
        Dictionary<string, string> ctlLabels;
        bool controlsOpen, ctlPadPage, shownPadAvailable;
        string waiting; // the action waiting for a key
        string padWaiting; // the pad action waiting for a button (its web name; ControlsCapturing reads it)
        int waitingPadRow = -1;
        int waitingSince = -1; // the frame the capture began (its own Enter / Space / A press doesn't bind)
        int controlsSwallowUntil = -1; // the navigation stays off through this frame (the binding press)
        int shownBindVersion = -1;
        PadLayout shownLayout;
        GlyphSet shownGlyphs;
        string ctlNoteText = "";

        /// <summary>CONTROLS is open: Esc closes it (SurvivalGame.HotKeys calls CloseControls).</summary>
        public bool ControlsOpen => controlsOpen;

        /// <summary>A key or a pad button is being captured: SurvivalGame.HotKeys skips its keys (the binding press is swallowed).</summary>
        public bool ControlsCapturing => controlsOpen && (waiting != null || padWaiting != null);

        /// <summary>The menu navigation is off: a capture, or the frame after its binding press.</summary>
        bool ControlsSwallowing => ControlsCapturing || Time.frameCount <= controlsSwallowUntil;

        /// <summary>menus.js :534: the GAMEPAD table only while a pad is connected (and the pad reader is wired).</summary>
        bool PadPageAvailable => game.padReader != null && PlayerDevices.PadConnected;

        /// <summary>Close CONTROLS and show TITLE / PAUSED again (BACK, Esc, the pad's B).</summary>
        public void CloseControls()
        {
            if (!controlsOpen) return;
            controlsOpen = false;
            waiting = padWaiting = null;
            waitingPadRow = -1;
            controlsScreen.SetActive(false);
            shownFlow = (Flow)(-1); // re-pick the screens
            if (settingsControlsReturn)
            {
                settingsControlsReturn = false; // NIGHT17: opened from SETTINGS > CONTROLS > KEYS: back there
                OpenSettings();
            }
        }

        partial void OpenControls()
        {
            if (controlsScreen == null || settingsOpen || MetaOpen || (game.flow != Flow.Title && game.flow != Flow.Paused)) return;
            controlsOpen = true;
            waiting = padWaiting = null;
            waitingPadRow = -1;
            shownPadAvailable = PadPageAvailable;
            ctlTabs.SetActive(shownPadAvailable);
            ShowControlsPage(shownPadAvailable && PlayerDevices.Last == InputDevice.Pad);
            RefreshControls("");
            title.SetActive(false);
            pause.SetActive(false);
            controlsScreen.SetActive(true);
        }

        partial void BuildControls()
        {
            ctlLabels = new Dictionary<string, string>();
            foreach (var kv in cfg.Obj("input.labels")) ctlLabels[kv.Key] = (string)kv.Value;
            ctlKeyOf = CtlKeyOf;
            ctlNoteLabel = a => ctlLabels.TryGetValue(a, out string l) ? l : a; // menus.js: config.input.labels[a] ?? a
            ctlGlyph = b => ControlsPad20.Glyph(b, PlayerDevices.Glyphs, game.padReader != null && game.padReader.V2); // NIGHT20 row 6: the swap note drawn
            controlsScreen = Screen("Controls", UiTheme.A(UiTheme.InkSolid, UnityTuning.ThemeInkAlpha)); // NIGHT17 U3-30.10: the menus' INK
            MenuHead(controlsScreen.transform, "CONTROLS", UnityTuning.ControlsHeadY); // NIGHT17 U3-30.10: the display head on the column
            ctlHint = Text(controlsScreen.transform, "Hint", "", tSmall, muted, TextAlignmentOptions.Center, new Vector2(0, UnityTuning.ControlsHintY), new Vector2(1600, 26), new Vector2(0.5f, 0.5f));
            // The two pages' tabs (only while a pad is connected)
            ctlTabs = new GameObject("Tabs", typeof(RectTransform));
            var tabsRt = ctlTabs.GetComponent<RectTransform>();
            tabsRt.SetParent(controlsScreen.transform, false);
            tabsRt.anchorMin = tabsRt.anchorMax = new Vector2(0.5f, 0.5f);
            tabsRt.sizeDelta = Vector2.zero;
            var tabSize = new Vector2(UnityTuning.ControlsTabWidth, UnityTuning.ControlsTabHeight);
            float tabX = (UnityTuning.ControlsTabWidth + 8) * 0.5f;
            ctlTabKeys = SmallButton(ctlTabs.transform, "KEYBOARD + MOUSE", new Vector2(-tabX, UnityTuning.ControlsTabsY), tabSize, () => ShowControlsPage(false));
            ctlTabPad = SmallButton(ctlTabs.transform, "GAMEPAD", new Vector2(tabX, UnityTuning.ControlsTabsY), tabSize, () => ShowControlsPage(true));
            ctlTabs.SetActive(false);
            keysPage = Page("Keyboard + mouse");
            padPage = Page("Gamepad");
            BuildKeyGroups();
            BuildPadGroups();
            ctlNote = Text(controlsScreen.transform, "Note", "", tBody, text, TextAlignmentOptions.Center, new Vector2(0, UnityTuning.ControlsNoteY), new Vector2(1600, 30), new Vector2(0.5f, 0.5f));
            float bx = UnityTuning.ControlsButtonSpacing * 0.5f;
            resetKeysButton = Button(controlsScreen.transform, "RESET KEYS", new Vector2(-bx, UnityTuning.ControlsButtonsY), () =>
            {
                waiting = null;
                game.bindings?.Reset();
                RefreshControls("");
            }).transform.parent.gameObject;
            resetPadButton = Button(controlsScreen.transform, "RESET PAD", new Vector2(-bx, UnityTuning.ControlsButtonsY), () =>
            {
                padWaiting = null;
                waitingPadRow = -1;
                if (game.padReader != null)
                {
                    int version = game.padReader.LayoutVersion; // NIGHT19 / NIGHT20: the block's default and store (v3: RB scrolls)
                    PadLayout def = game.padReader.DefaultLayoutNow;
                    game.padReader.layout = def; // main.js padBinds.reset
                    InputPrefs.SavePadLayout(def, version);
                }
                RefreshControls("");
            }).transform.parent.gameObject;
            Button(controlsScreen.transform, "BACK", new Vector2(bx, UnityTuning.ControlsButtonsY), CloseControls);
            ShowControlsPage(false);
            RefreshControls("");
        }

        /// <summary>menus.js keyOf: the live binding's key name, upper-cased (config.input.bindings' default until the store is wired).</summary>
        string CtlKeyOf(string action)
        {
            string code = game.bindings != null ? game.bindings.Code(action) : cfg.Has($"input.bindings.{action}") ? cfg.Str($"input.bindings.{action}") : null;
            return KeyCodes.Label(code);
        }

        GameObject Page(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(controlsScreen.transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.zero;
            return go;
        }

        /// <summary>
        /// menus.js renderRebind's grid: MenusMath.ControlRows' groups; a row of the controlGroups is a key cell when its action
        /// is in config.input.bindings (menus.js rebindable), MOUSE + FIXED KEYS a key text, TECHNIQUES and READING THE HUD a
        /// name over its text. RefreshControls rewrites the texts from ControlRows again.
        /// </summary>
        void BuildKeyGroups()
        {
            List<ControlGroup> rows = MenusMath.ControlRows(cfg, ctlKeyOf);
            List<object> bound = cfg.Arr("ui.controlGroups");
            Dictionary<string, object> rebindable = cfg.Obj("input.bindings");
            for (int g = 0; g < rows.Count; g++)
            {
                ControlGroup src = rows[g];
                CtlGroup group = Group(keysPage.transform, src.title, UnityTuning.ControlsColumnWidth);
                List<object> actions = g < bound.Count ? PortData.Arr(PortData.Arr(bound[g])[1]) : null;
                for (int i = 0; i < src.rows.Count; i++)
                {
                    var (what, key) = src.rows[i];
                    string action = actions != null && i < actions.Count ? (string)actions[i] : null;
                    if (action != null && rebindable.ContainsKey(action)) group.rows.Add(KeyRow(group, action, what));
                    else if (actions != null || src.title == "MOUSE + FIXED KEYS") group.rows.Add(FixedRow(group, what, key));
                    else group.rows.Add(TextRow(group, what, key));
                }
                keyGroups.Add(group);
            }
        }

        /// <summary>menus.js :534-537: the pad's actions (PAD_ACTION_LABELS order) with their buttons.</summary>
        void BuildPadGroups()
        {
            CtlGroup pad = Group(padPage.transform, "GAMEPAD", UnityTuning.ControlsPadColumnWidth);
            // NIGHT20 row 6: the sticks first (MOVE, LOOK: their drawn glyphs; with the v2 rules live, RefreshControls)
            float padKeyW = UnityTuning.ControlsPadBindWidth, padLabelW = UnityTuning.ControlsPadColumnWidth - padKeyW - UnityTuning.ControlsCellGap;
            for (int s = 0; s < 2; s++)
            {
                var row = new CtlRow { kind = CtlKind.Fixed, stick = s };
                row.label = Text(pad.rt, "Label", s == 0 ? UnityTuning.ControlsPadMove : UnityTuning.ControlsPadLook, tBody, text, TextAlignmentOptions.TopLeft, Vector2.zero, new Vector2(padLabelW, 0), new Vector2(0, 1));
                UiTheme.Style(row.label, UiTheme.Px(UiTheme.Size.S24), UiTheme.White, UiTheme.BodyBold);
                row.value = Text(pad.rt, "Key", "", tBody, text, TextAlignmentOptions.TopRight, Vector2.zero, new Vector2(padKeyW, 0), new Vector2(0, 1));
                pad.rows.Add(row);
            }
            // NIGHT19: every row of the v2 table (a superset: the v1 table hides SPRINT's row, RefreshControls)
            for (int i = 0; i < GamepadMath.LabelOrderV2.Length; i++)
            {
                int padRow = i;
                var row = new CtlRow { kind = CtlKind.Pad, padRow = i, padAction = GamepadMath.LabelOrderV2[i] };
                row.label = RowLabel(pad, GamepadMath.ActionLabels[(int)GamepadMath.LabelOrderV2[i]], UnityTuning.ControlsPadColumnWidth - UnityTuning.ControlsPadBindWidth - UnityTuning.ControlsCellGap);
                row.bind = BindButton(pad.rt, UnityTuning.ControlsPadBindWidth, () => StartPadCapture(padRow), out row.value);
                pad.rows.Add(row);
                if (GamepadMath.LabelOrderV2[i] != PadAction.Sprint) continue;
                // NIGHT20 row 6: under SPRINT, its TOGGLE / HOLD (the controller research's row 12); the cell flips the setting
                var mode = new CtlRow { kind = CtlKind.Pad, sprintMode = true };
                mode.label = RowLabel(pad, UnityTuning.PadSprintModeLabel, UnityTuning.ControlsPadColumnWidth - UnityTuning.ControlsPadBindWidth - UnityTuning.ControlsCellGap);
                mode.bind = BindButton(pad.rt, UnityTuning.ControlsPadBindWidth, FlipSprintMode, out mode.value);
                pad.rows.Add(mode);
            }
            padGroups.Add(pad);
        }

        /// <summary>NIGHT20 row 6: L3 SPRINT's cell: TOGGLE <-> HOLD (saved and applied through the settings: ReaderInput reads it).</summary>
        void FlipSprintMode()
        {
            if (game.settings == null || !ControlsPad20.Has(game.settings.store) || ControlsCapturing) return;
            game.settings.Set(UnityTuning.PadSprintModeSetting, ControlsPad20.NextSprintMode(game.settings.store));
            RefreshControls("");
        }

        CtlGroup Group(Transform page, string title, float width)
        {
            var go = new GameObject($"Group {title}", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(page, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(width, 0);
            var group = new CtlGroup { rt = rt };
            group.head = Text(rt, "Head", title, tH3, text, TextAlignmentOptions.Left, Vector2.zero, new Vector2(width, UnityTuning.ControlsHeaderHeight), new Vector2(0, 1));
            UiTheme.Style(group.head, UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, UiTheme.BodyBold); // NIGHT17: a group's label
            group.head.characterSpacing = UnityTuning.ThemeLabelTracking;
            return group;
        }

        TextMeshProUGUI RowLabel(CtlGroup group, string s, float width)
        {
            TextMeshProUGUI t = Text(group.rt, "Label", s, tBody, text, TextAlignmentOptions.Left, Vector2.zero, new Vector2(width, UnityTuning.ControlsKeyRowHeight), new Vector2(0, 1));
            UiTheme.Style(t, UiTheme.Px(UiTheme.Size.S24), UiTheme.White, UiTheme.BodyBold); // NIGHT17: the row style
            t.overflowMode = TextOverflowModes.Ellipsis;
            return t;
        }

        CtlRow KeyRow(CtlGroup group, string action, string what)
        {
            var row = new CtlRow { kind = CtlKind.Key, action = action };
            row.label = RowLabel(group, what, UnityTuning.ControlsColumnWidth - UnityTuning.ControlsBindWidth - UnityTuning.ControlsCellGap);
            row.bind = BindButton(group.rt, UnityTuning.ControlsBindWidth, () => StartKeyCapture(action), out row.value);
            return row;
        }

        CtlRow FixedRow(CtlGroup group, string what, string key)
        {
            float keyW = UnityTuning.ControlsBindWidth, labelW = UnityTuning.ControlsColumnWidth - keyW - UnityTuning.ControlsCellGap;
            var row = new CtlRow { kind = CtlKind.Fixed };
            row.label = Text(group.rt, "Label", what, tBody, text, TextAlignmentOptions.TopLeft, Vector2.zero, new Vector2(labelW, 0), new Vector2(0, 1));
            row.label.textWrappingMode = TextWrappingModes.Normal;
            row.value = Text(group.rt, "Key", key, tBody, muted, TextAlignmentOptions.TopRight, Vector2.zero, new Vector2(keyW, 0), new Vector2(0, 1));
            row.value.textWrappingMode = TextWrappingModes.Normal;
            return row;
        }

        /// <summary>A technique or a HUD legend: the name on its line, what it means (wrapped, muted) under it.</summary>
        CtlRow TextRow(CtlGroup group, string what, string means)
        {
            float w = UnityTuning.ControlsColumnWidth;
            var row = new CtlRow { kind = CtlKind.Text };
            row.label = Text(group.rt, "Label", what, tBody, text, TextAlignmentOptions.TopLeft, Vector2.zero, new Vector2(w, 0), new Vector2(0, 1));
            row.label.textWrappingMode = TextWrappingModes.Normal;
            row.value = Text(group.rt, "Means", means, tSmall, muted, TextAlignmentOptions.TopLeft, Vector2.zero, new Vector2(w, 0), new Vector2(0, 1));
            row.value.textWrappingMode = TextWrappingModes.Normal;
            return row;
        }

        /// <summary>A key / pad button cell: the panel, a label (the key, or PRESS A KEY in cyan while waiting).</summary>
        RectTransform BindButton(RectTransform parent, float width, System.Action onClick, out TextMeshProUGUI label)
        {
            var size = new Vector2(width, UnityTuning.ControlsBindHeight);
            var img = Img(parent, "Bind", UiTheme.InkSoft, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, size); // NIGHT17: a solid cut chip
            img.sprite = UiTheme.Cut;
            img.type = UnityEngine.UI.Image.Type.Sliced;
            img.rectTransform.pivot = new Vector2(0, 1);
            img.raycastTarget = true;
            var b = img.gameObject.AddComponent<UnityEngine.UI.Button>();
            var colors = b.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = UiTheme.CyanPale;
            colors.pressedColor = UiTheme.Cyan;
            b.colors = colors;
            b.onClick.AddListener(() => onClick());
            label = Text(img.transform, "Label", "", tLabel, text, TextAlignmentOptions.Center, Vector2.zero, size, new Vector2(0.5f, 0.5f));
            label.overflowMode = TextOverflowModes.Ellipsis;
            return img.rectTransform;
        }

        void ShowControlsPage(bool pad)
        {
            ctlPadPage = pad && PadPageAvailable;
            keysPage.SetActive(!ctlPadPage);
            padPage.SetActive(ctlPadPage);
            resetKeysButton.SetActive(!ctlPadPage);
            resetPadButton.SetActive(ctlPadPage);
            ctlTabKeys.color = ctlPadPage ? text : usable;
            ctlTabPad.color = ctlPadPage ? usable : text;
            // menus.js: the keyboard hint; the pad table's header line
            ctlHint.text = ctlPadPage ? ControlsPad20.HintLine(game.padReader != null && game.padReader.V2) : "CLICK A KEY, THEN PRESS THE NEW ONE (CTRL CAN NOT BE BOUND)"; // NIGHT20 row 6: drawn A / B
            if (!ctlPadPage)
            {
                padWaiting = null;
                waitingPadRow = -1;
            }
            else waiting = null;
            if (controlsOpen) RefreshControls(ctlNoteText);
        }

        /// <summary>NIGHT19 U3-33.3: LB / RB on CONTROLS (a pad connected, nothing being captured): KEYBOARD + MOUSE and GAMEPAD
        /// are two tabs, either bumper switches (wrapping), the new page sliding in from the bumper's side.</summary>
        void ControlsTab19(int dir)
        {
            if (!PadPageAvailable || ControlsCapturing) return;
            ShowControlsPage(!ctlPadPage);
            TabSlideStart19((RectTransform)(ctlPadPage ? padPage : keysPage).transform, dir);
            navDirty = true; // the page's cells are the screen's controls now
        }

        void StartKeyCapture(string action)
        {
            if (!controlsOpen || game.bindings == null) return;
            waiting = action;
            padWaiting = null;
            waitingPadRow = -1;
            waitingSince = Time.frameCount;
            RefreshControls("");
        }

        void StartPadCapture(int padRow)
        {
            if (!controlsOpen || !ctlPadPage) return;
            padWaiting = GamepadMath.ActionNames[(int)GamepadMath.LabelOrderV2[padRow]];
            waitingPadRow = padRow;
            waiting = null;
            waitingSince = Time.frameCount;
            RefreshControls("");
        }

        partial void RenderControls(double frameDt)
        {
            if (!controlsOpen) return;
            Flow f = game.flow;
            if (f != Flow.Title && f != Flow.Paused)
            {
                CloseControls(); // the run went on (or ended) under it
                return;
            }
            if (title.activeSelf) title.SetActive(false); // the flow block re-picked the screens
            if (pause.activeSelf) pause.SetActive(false);
            bool padOn = PadPageAvailable;
            if (padOn != shownPadAvailable)
            {
                shownPadAvailable = padOn;
                ctlTabs.SetActive(padOn);
                if (!padOn && ctlPadPage) ShowControlsPage(false);
                navDirty = true;
            }
            int bindVersion = game.bindings != null ? game.bindings.Version : -1;
            PadLayout layout = game.padReader?.layout;
            if (bindVersion != shownBindVersion || layout != shownLayout || PlayerDevices.Glyphs != shownGlyphs) RefreshControls(ctlNoteText); // a change from elsewhere
            if (Time.frameCount <= waitingSince) return; // the press that started the capture
            if (waiting != null) PollKeyCapture();
            else if (padWaiting != null) PollPadCapture();
        }

        /// <summary>menus.js keydown capture: Esc (or the pad's B) cancels; any other key the table knows binds (a note when refused or swapped).</summary>
        void PollKeyCapture()
        {
            Keyboard kb = Keyboard.current;
            if (PlayerDevices.Menu.back || (kb != null && kb.escapeKey.wasPressedThisFrame) || game.bindings == null)
            {
                waiting = null;
                Swallow();
                RefreshControls("");
                return;
            }
            string code = KeyCodes.PressedCode(kb);
            if (code == null) return;
            string action = waiting;
            waiting = null;
            Swallow();
            RefreshControls(ControlsRebind.Key(game.bindings, action, code, ctlNoteLabel));
        }

        /// <summary>menus.js input:padButton: the next pad button pressed takes the action (the two buttons swap). Esc cancels.</summary>
        void PollPadCapture()
        {
            Keyboard kb = Keyboard.current;
            PadReader pad = game.padReader;
            if ((kb != null && kb.escapeKey.wasPressedThisFrame) || pad == null)
            {
                padWaiting = null;
                waitingPadRow = -1;
                Swallow();
                RefreshControls("");
                return;
            }
            int raw = pad.rawPressed;
            if (raw == 0) return;
            int button = 0;
            while ((raw & (1 << button)) == 0) button++; // the first in PAD_BUTTONS' order (rawPresses' pressed list)
            PadAction action = GamepadMath.LabelOrderV2[waitingPadRow];
            padWaiting = null;
            waitingPadRow = -1;
            Swallow();
            PadLayout layout = pad.layout;
            string note = ControlsRebind.Pad(ref layout, action, (PadButtonName)button, ctlGlyph, out bool changed);
            if (!changed && GamepadMath.ButtonFor(action, layout) == null)
            {
                // NIGHT19: an UNBOUND action (SPRINT after a customised v1's migration) takes the button: the button's
                // actions go unbound only if none of them is needed (ControlsRebind.PadUnbound)
                note = ControlsRebind.PadUnbound(ref layout, action, (PadButtonName)button, ctlGlyph, out changed);
            }
            if (changed)
            {
                pad.layout = layout; // main.js padBinds.set: input.setPadLayout, padStore.save
                InputPrefs.SavePadLayout(layout, pad.LayoutVersion); // NIGHT19 / NIGHT20: the block's store
            }
            RefreshControls(note);
        }

        void Swallow() => controlsSwallowUntil = Time.frameCount + 1;

        /// <summary>menus.js renderRebind: every key and button cell, the techniques' keys, the note; then the columns again.</summary>
        void RefreshControls(string note)
        {
            if (controlsScreen == null) return;
            shownBindVersion = game.bindings != null ? game.bindings.Version : -1;
            shownLayout = game.padReader?.layout;
            shownGlyphs = PlayerDevices.Glyphs;
            ctlNoteText = note ?? "";
            ctlNote.text = ctlNoteText;
            List<ControlGroup> rows = MenusMath.ControlRows(cfg, ctlKeyOf);
            for (int g = 0; g < keyGroups.Count && g < rows.Count; g++)
            {
                List<CtlRow> cells = keyGroups[g].rows;
                for (int i = 0; i < cells.Count && i < rows[g].rows.Count; i++)
                {
                    CtlRow r = cells[i];
                    string key = rows[g].rows[i].key;
                    if (r.kind == CtlKind.Key)
                    {
                        bool wait = waiting == r.action;
                        r.value.text = wait ? "PRESS A KEY" : key;
                        r.value.color = wait ? usable : text;
                    }
                    else if (r.value.text != key) r.value.text = key; // the techniques' filled keys
                }
            }
            bool v2 = game.padReader != null && game.padReader.V2; // NIGHT19: the v2 table (SPRINT's row, UNBOUND, drawn glyphs)
            List<(string what, string glyph)> sticks = v2 ? ControlsPad20.StickRows() : null; // NIGHT20 row 6
            foreach (CtlGroup g in padGroups)
            {
                foreach (CtlRow r in g.rows)
                {
                    if (r.stick >= 0 || r.sprintMode)
                    {
                        // NIGHT20 row 6: the sticks' drawn glyphs and L3 SPRINT's mode, with the v2 rules live only
                        bool on = v2 && (r.stick >= 0 || (game.settings != null && ControlsPad20.Has(game.settings.store)));
                        if (r.label.gameObject.activeSelf != on)
                        {
                            r.label.gameObject.SetActive(on);
                            if (r.bind != null) r.bind.gameObject.SetActive(on);
                            else r.value.gameObject.SetActive(on);
                        }
                        if (!on) continue;
                        r.value.text = r.stick >= 0 ? sticks[r.stick].glyph : ControlsPad20.SprintModeLabel(game.settings.store);
                        r.value.color = text;
                        continue;
                    }
                    bool shown = v2 || r.padAction != PadAction.Sprint;
                    if (r.label.gameObject.activeSelf != shown)
                    {
                        r.label.gameObject.SetActive(shown);
                        r.bind.gameObject.SetActive(shown);
                    }
                    if (!shown) continue;
                    bool wait = waitingPadRow == r.padRow;
                    r.value.text = wait ? "PRESS A PAD BUTTON" : Glyphs.PadBindGlyph(r.padAction, shownLayout, shownGlyphs, v2);
                    r.value.color = wait ? usable : text;
                }
            }
            LayoutGroups(keyGroups, UnityTuning.ControlsColumns, UnityTuning.ControlsColumnWidth);
            LayoutGroups(padGroups, 1, UnityTuning.ControlsPadColumnWidth);
        }

        /// <summary>
        /// The groups flowed into columns (menus.js .ctlgrid): each group's rows measured (wrapped text at the column's width)
        /// and stacked under its header; a group that doesn't fit the column's height starts the next column.
        /// </summary>
        void LayoutGroups(List<CtlGroup> groups, int columns, float width)
        {
            float gap = UnityTuning.ControlsColumnGap, maxH = UnityTuning.ControlsGridTop - UnityTuning.ControlsGridBottom;
            float x0 = -(columns * width + (columns - 1) * gap) * 0.5f;
            int col = 0;
            float y = 0;
            foreach (CtlGroup g in groups)
            {
                float h = LayoutRows(g, width);
                if (y > 0 && y + h > maxH && col < columns - 1)
                {
                    col++;
                    y = 0;
                }
                g.rt.anchoredPosition = new Vector2(x0 + col * (width + gap), UnityTuning.ControlsGridTop - y);
                g.rt.sizeDelta = new Vector2(width, h);
                y += h + UnityTuning.ControlsGroupGap;
            }
        }

        const float Unbounded = 10000f; // the height a wrapped text is measured in (no limit: its lines decide)

        /// <summary>One group's rows under its header (positions from the group's top left); returns its height.</summary>
        float LayoutRows(CtlGroup g, float width)
        {
            float y = UnityTuning.ControlsHeaderHeight;
            foreach (CtlRow r in g.rows)
            {
                if (!r.label.gameObject.activeSelf) continue; // NIGHT19: a hidden row (the v1 table's SPRINT) takes no room
                float h;
                switch (r.kind)
                {
                    case CtlKind.Key:
                    case CtlKind.Pad:
                        h = UnityTuning.ControlsKeyRowHeight;
                        r.label.rectTransform.anchoredPosition = new Vector2(0, -y);
                        r.bind.anchoredPosition = new Vector2(width - r.bind.sizeDelta.x, -y - (h - UnityTuning.ControlsBindHeight) * 0.5f);
                        break;
                    case CtlKind.Fixed:
                    {
                        float keyW = r.value.rectTransform.sizeDelta.x, labelW = r.label.rectTransform.sizeDelta.x;
                        float lh = r.label.GetPreferredValues(r.label.text, labelW, Unbounded).y, kh = r.value.GetPreferredValues(r.value.text, keyW, Unbounded).y;
                        h = Mathf.Max(lh, kh) + UnityTuning.ControlsRowGap;
                        r.label.rectTransform.sizeDelta = new Vector2(labelW, lh);
                        r.value.rectTransform.sizeDelta = new Vector2(keyW, kh);
                        r.label.rectTransform.anchoredPosition = new Vector2(0, -y);
                        r.value.rectTransform.anchoredPosition = new Vector2(width - keyW, -y);
                        break;
                    }
                    default:
                    {
                        float lh = r.label.GetPreferredValues(r.label.text, width, Unbounded).y, vh = r.value.GetPreferredValues(r.value.text, width, Unbounded).y;
                        h = lh + vh + UnityTuning.ControlsRowGap;
                        r.label.rectTransform.sizeDelta = new Vector2(width, lh);
                        r.value.rectTransform.sizeDelta = new Vector2(width, vh);
                        r.label.rectTransform.anchoredPosition = new Vector2(0, -y);
                        r.value.rectTransform.anchoredPosition = new Vector2(0, -y - lh);
                        break;
                    }
                }
                y += h;
            }
            return y;
        }
    }

    /// <summary>
    /// NIGHT13 U3-26.8: CONTROLS' rebinding through the live stores (no UnityEngine use; ControlsRebindTests). The stores save
    /// themselves (Bindings.persist) or are saved by the caller (the pad layout: InputPrefs.SavePadLayout).
    /// </summary>
    public static class ControlsRebind
    {
        /// <summary>
        /// menus.js keydown capture: `code` binds `action` (bindings.set: a key in use swaps; UNBINDABLE refused); the note:
        /// "" when free, "E WAS BUY (HOLD): SWAPPED" on a swap, "CONTROLLEFT CAN NOT BE BOUND" when refused (keyName(code),
        /// `label` = config.input.labels[a] ?? a).
        /// </summary>
        public static string Key(Bindings bindings, string action, string code, System.Func<string, string> label)
        {
            RebindResult r = bindings.Set(action, code);
            string name = Bindings.KeyName(code);
            return !r.ok ? name + " CAN NOT BE BOUND" : r.conflict != null ? name + " WAS " + label(r.conflict) + ": SWAPPED" : "";
        }

        /// <summary>
        /// menus.js input:padButton + main.js padBinds.set: `button` takes `action` (gamepad.js rebindPad: the two buttons swap
        /// their whole action lists); `layout` becomes the new one; `changed` when it did (the caller makes it live and saves
        /// it); the note "LB SWAPPED WITH Y" when another button's actions moved, else "".
        /// </summary>
        public static string Pad(ref PadLayout layout, PadAction action, PadButtonName button, System.Func<PadButtonName, string> glyph, out bool changed)
        {
            PadRebind r = GamepadMath.RebindPad(layout, action, button);
            changed = r.ok && !ReferenceEquals(r.layout, layout);
            if (r.ok) layout = r.layout;
            return r.ok && r.swapped.HasValue ? glyph(button) + " SWAPPED WITH " + glyph(r.swapped.Value) : "";
        }

        /// <summary>
        /// NIGHT19 U3-33.3: an UNBOUND action (SPRINT, after a customised v1 layout's migration) takes `button` (rebindPad
        /// refuses an action no button carries). The button's own actions go unbound ("X-RAY UNBOUND"), unless one of them
        /// must stay bound (REQUIRED): then nothing changes and the note says so.
        /// </summary>
        public static string PadUnbound(ref PadLayout layout, PadAction action, PadButtonName button, System.Func<PadButtonName, string> glyph, out bool changed)
        {
            changed = false;
            IReadOnlyList<PadAction> there = layout.ActionsOf(button);
            for (int i = 0; i < there.Count; i++)
            {
                if (GamepadMath.IsRequired(there[i]) && CarriedOnlyBy(layout, there[i], button))
                    return glyph(button) + " IS " + GamepadMath.ActionLabels[(int)there[i]] + ": PICK ANOTHER";
            }
            string note = "";
            for (int i = 0; i < there.Count; i++)
            {
                if (CarriedOnlyBy(layout, there[i], button)) note += (note.Length > 0 ? " · " : "") + GamepadMath.ActionLabels[(int)there[i]];
            }
            layout = layout.With(button, action);
            changed = true;
            return note.Length > 0 ? note + " UNBOUND" : "";
        }

        static bool CarriedOnlyBy(PadLayout layout, PadAction a, PadButtonName button)
        {
            uint bit = GamepadMath.Bit(a);
            for (int i = 0; i < layout.Count; i++)
                if (layout[i] != button && (layout.MaskOf(layout[i]) & bit) != 0) return false;
            return true;
        }
    }
}
