// NIGHT13 U3-26.8 (agent S): menu navigation on the keyboard and the pad (menus.js :756-791 keyboard, :885-968 padInput,
// :1013 show() focusing primaryIndex; audit-input section 4 row 15 and section 5 "uGUI"; controller research section 7
// and checklist items 1, 31).
// - The EventSystem's InputSystemUIInputModule gets an actions asset built here instead of the package's defaults
//   (which navigate with WASD too, while WASD moves the player): Navigate = the arrows, the d-pad and the left stick
//   (past gamepad.menuStick), Submit = Enter / Space / the pad's south button, Cancel = Esc / the pad's east button (by
//   position, as the web: a Switch pad's own usages would swap them); the pointer actions as the defaults. A held
//   direction repeats after gamepad.menuDelay, then every gamepad.menuEvery (gamepad.js menuNav's numbers).
// - Every Selectable of the open screen (the top active screen of the menus layer) gets explicit links: up / down go to
//   the nearest control that way, the same column first (CONTROLS' grid walks down its columns), wrapping to the far
//   row (padFocusOn's modulo); left / right go to the neighbour in the row (centres within NavRowTolerance), else the
//   previous / next in reading order (rows top to bottom, then left to right), wrapping. On SETTINGS, left / right change
//   a row (NavRow) or switch the tab (NavSide), as padInput's side branch. The links are rebuilt when the open screen or
//   the number of live Selectables changes, never per frame otherwise.
// - When a screen opens its primary control is selected (primaryIndex): TITLE's PLAY, PAUSED's RESUME, RESULTS' RESTART,
//   SETTINGS' current tab, else the first in reading order. A lost selection (a click on nothing, a hidden button) goes
//   back to it, so the pad's focus is never lost; the module never deselects on a background click.
// - The selected control shows a cyan frame (colors.usable: player-usable) with a faint cyan fill, on every button and
//   row; a settings row's label turns cyan and its description shows.
// - The pad's B goes back (closes CONTROLS, SETTINGS, CONTRACTS / WEAPONS) or resumes from PAUSED; START resumes from
//   PAUSED (PlayerDevices.Menu, U3-26.7's menuNav edges); neither acts on the frame the pause began. Esc stays
//   SurvivalGame.HotKeys' (the lead adds CONTROLS there).
// - CONTROLS' key and pad capture turn the navigation events off until the frame after the binding press (the press that
//   binds doesn't also click, menus.js padSwallow).
// Per frame: a walk of the menus layer's children, two compares and the selection check; nothing allocates.
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Skygrind.Game
{
    /// <summary>
    /// One settings row as one Selectable (menus.js padTargets: the row's control): left / right call `side` (a range or a
    /// select changes, a toggle switches the tab), up / down follow the explicit links, Submit calls `submit`.
    /// </summary>
    public sealed class NavRow : UnityEngine.UI.Selectable, ISubmitHandler, IPointerClickHandler
    {
        public System.Action<int> side;
        public System.Action submit;

        protected override void Awake()
        {
            base.Awake();
            transition = Transition.None; // the cyan frame shows the selection (UiNav)
        }

        /// <summary>NIGHT17 U3-30.8: a click on a row is its press (a toggle flips, a range or a cycle steps on).</summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !IsActive() || !IsInteractable()) return;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(gameObject);
            submit?.Invoke();
        }

        public override void OnMove(AxisEventData eventData)
        {
            if (eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right)
            {
                if (IsActive() && IsInteractable()) side?.Invoke(eventData.moveDir == MoveDirection.Right ? 1 : -1);
                eventData.Use();
                return;
            }
            base.OnMove(eventData);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (IsActive() && IsInteractable()) submit?.Invoke();
        }
    }

    /// <summary>Left / right on a control that has no value to change (SETTINGS' tabs, RESET TAB, BACK): `side` (the tab).</summary>
    public sealed class NavSide : MonoBehaviour, IMoveHandler
    {
        public System.Action<int> side;

        public void OnMove(AxisEventData eventData)
        {
            if (eventData.moveDir != MoveDirection.Left && eventData.moveDir != MoveDirection.Right) return;
            side?.Invoke(eventData.moveDir == MoveDirection.Right ? 1 : -1);
            eventData.Use();
        }
    }

    /// <summary>
    /// NIGHT17 U3-30.8: mouse hover selects (the focus frame / the lit item follow the pointer), as a pad's move would. Only
    /// when the mouse moved this frame, so a pointer resting where a new screen opens never steals the pad's focus.
    /// </summary>
    public sealed class NavHover : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            // NIGHT18 (the harness, both sides): a reel's focus moves are its own; a real mouse moving on the machine during a
            // recording must not steal them (round 2's ui-settings: the focus walked the rows in the off and on reels alike)
            if (FeelReel.Running) return;
            Mouse m = Mouse.current;
            if (m == null || m.delta.ReadValue().sqrMagnitude < UnityTuning.NavHoverMinMove * UnityTuning.NavHoverMinMove) return; // round 5: a real move, not jitter
            if (!TryGetComponent(out UnityEngine.UI.Selectable s) || !s.IsActive() || !s.IsInteractable()) return;
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != gameObject) EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public sealed partial class Ui
    {
        InputSystemUIInputModule navModule;
        UiShape navCursorFill; // NIGHT17: the frame is a solid slanted fill (no 1 px outline) and a 4 px accent bar
        UnityEngine.UI.Image navCursorBar;
        TMPro.TextMeshProUGUI navTintLabel; // NIGHT16 polish #55 (u-small2-n16, 91caa6b): the focused button's label, and its colour before
        Color navTintWas;
        RectTransform navCursor; // the cyan frame, parented to the selected control
        CanvasGroup navCursorGroup; // NIGHT25: its alpha
        double navFadeAge25 = double.MaxValue;
        GameObject navScreen, navSelected, navPrimaryTitle, navPrimaryPause;
        bool navDraftWaits; // the MODS draft is open and no menu move has come yet (nothing selected)
        int navSelectableCount = -1, navPausedFrame = -1;
        bool navDirty; // a screen rebuilt its controls (CONTROLS): relink on the next frame
        Flow navFlow = (Flow)(-1);
        Vector2 navRootSize;
        readonly List<UnityEngine.UI.Selectable> navScratch = new List<UnityEngine.UI.Selectable>(96);
        readonly List<UnityEngine.UI.Selectable> navList = new List<UnityEngine.UI.Selectable>(96);
        float[] navX = new float[96], navY = new float[96], navMinX = new float[96], navMaxX = new float[96];
        int[] navOrder = new int[96], navRowOf = new int[96], navRowStart = new int[97];
        // NIGHT16 polish #8: the frame slides from the last control to the new one (visual only: the selection, and so the
        // next press, moved at once). The slide's start offsets in the new control's space, and its age (s; past the time: done).
        Vector2 navSlideMin, navSlideMax;
        float navSlideAge = float.MaxValue;
        readonly Vector3[] navCorners = new Vector3[4];

        partial void BuildNav()
        {
            navPrimaryTitle = title.transform.Find("PLAY")?.gameObject;
            navPrimaryPause = pause.transform.Find("RESUME")?.gameObject;
            BuildNavCursor();
            ConfigureUiModule();
        }

        /// <summary>The UI module's actions, built in code (WASD out; the web's Submit / Cancel keys and buttons; menuNav's repeat).</summary>
        void ConfigureUiModule()
        {
            navModule = UnityEngine.Object.FindFirstObjectByType<InputSystemUIInputModule>();
            if (navModule == null)
            {
                Debug.LogWarning("[ui] no InputSystemUIInputModule: menu navigation keeps its defaults");
                return;
            }
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "SKYGRIND UI actions (UiNav)";
            InputActionMap map = asset.AddActionMap("UI"); // the defaults' map and action names, so the module's references remap by name
            InputAction point = map.AddAction("Point", InputActionType.PassThrough, expectedControlLayout: "Vector2");
            point.AddBinding("<Mouse>/position");
            point.AddBinding("<Pen>/position");
            InputAction click = map.AddAction("Click", InputActionType.PassThrough, expectedControlLayout: "Button");
            click.AddBinding("<Mouse>/leftButton");
            click.AddBinding("<Pen>/tip");
            InputAction rightClick = map.AddAction("RightClick", InputActionType.PassThrough, "<Mouse>/rightButton", expectedControlLayout: "Button");
            InputAction middleClick = map.AddAction("MiddleClick", InputActionType.PassThrough, "<Mouse>/middleButton", expectedControlLayout: "Button");
            InputAction scroll = map.AddAction("ScrollWheel", InputActionType.PassThrough, "<Mouse>/scroll", expectedControlLayout: "Vector2");
            InputAction navigate = map.AddAction("Navigate", InputActionType.PassThrough, expectedControlLayout: "Vector2");
            navigate.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            navigate.AddBinding("<Gamepad>/dpad");
            // The left stick counts past gamepad.menuStick (menuNav's tilt). The control's own dead zone (0.125-0.925, rescaled)
            // runs first, so this threshold on the rescaled value sits at ~0.6 of the raw tilt.
            navigate.AddBinding("<Gamepad>/leftStick", processors: string.Format(CultureInfo.InvariantCulture, "stickDeadzone(min={0},max=1)", cfg.Num("gamepad.menuStick")));
            InputAction submit = map.AddAction("Submit", InputActionType.Button);
            submit.AddBinding("<Keyboard>/enter");
            submit.AddBinding("<Keyboard>/numpadEnter");
            submit.AddBinding("<Keyboard>/space");
            submit.AddBinding("<Gamepad>/buttonSouth");
            InputAction cancel = map.AddAction("Cancel", InputActionType.Button);
            cancel.AddBinding("<Keyboard>/escape");
            cancel.AddBinding("<Gamepad>/buttonEast");
            navModule.actionsAsset = asset;
            navModule.point = InputActionReference.Create(point);
            navModule.leftClick = InputActionReference.Create(click);
            navModule.rightClick = InputActionReference.Create(rightClick);
            navModule.middleClick = InputActionReference.Create(middleClick);
            navModule.scrollWheel = InputActionReference.Create(scroll);
            navModule.move = InputActionReference.Create(navigate);
            navModule.submit = InputActionReference.Create(submit);
            navModule.cancel = InputActionReference.Create(cancel);
            navModule.moveRepeatDelay = (float)cfg.Num("gamepad.menuDelay");
            navModule.moveRepeatRate = (float)cfg.Num("gamepad.menuEvery");
            navModule.deselectOnBackgroundClick = false; // a click on nothing keeps the focus (checklist 31)
        }

        void BuildNavCursor()
        {
            navCursor = new GameObject("Nav focus", typeof(RectTransform)).GetComponent<RectTransform>();
            navCursor.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().ignoreLayout = true;
            navCursorGroup = navCursor.gameObject.AddComponent<CanvasGroup>(); // NIGHT25 (T-12, M9): the frame fades in where it first appears
            navCursorGroup.interactable = navCursorGroup.blocksRaycasts = false;
            // NIGHT17 U3-30.8: a solid slanted fill (CYAN behind a menu button's INK label, CYAN-DEEP behind a row) and a
            // 4 px accent bar on its left: no 1 px outline, no faint tint.
            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
            var frt = (RectTransform)fillGo.transform;
            frt.SetParent(navCursor, false);
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = frt.offsetMax = Vector2.zero;
            navCursorFill = fillGo.GetComponent<UiShape>();
            navCursorFill.color = UiTheme.CyanDeep;
            navCursorFill.raycastTarget = false;
            navCursorBar = Img(navCursor, "Bar", UiTheme.Cyan, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(UnityTuning.ThemeAccentBar, 0));
            navCursorBar.rectTransform.pivot = new Vector2(0, 0.5f);
            navCursor.SetParent(menus, false);
            navCursor.gameObject.SetActive(false);
        }

        partial void RenderNav(double frameDt)
        {
            EventSystem es = EventSystem.current;
            if (es == null) return;
            if (root.rect.size != navRootSize)
            {
                navRootSize = root.rect.size; // the window changed size: UI SCALE's menus compensation follows
                ApplyUiScale();
            }
            Flow f = game.flow;
            if (f != navFlow)
            {
                if (f == Flow.Paused) navPausedFrame = Time.frameCount; // START / B wait for a press after the one that paused
                navFlow = f;
            }

            GameObject screen = TopScreen();
            int count = UnityEngine.UI.Selectable.allSelectableCount;
            if (screen != navScreen || count != navSelectableCount || navDirty)
            {
                bool opened = screen != navScreen;
                navScreen = screen;
                navSelectableCount = count;
                navDirty = false;
                navList.Clear();
                if (screen != null) Relink(screen);
                if (opened)
                {
                    // NIGHT13 review: the MODS draft opens mid-break, so a jump's A (the pad's submit) would take the first
                    // card; it selects nothing until a menu move (1 / 2 / 3 and clicks need no selection).
                    navDraftWaits = screen != null && screen == draftScreen;
                    es.SetSelectedGameObject(screen != null && !navDraftWaits ? PrimaryOf(screen) : null);
                }
            }
            MenuInput m = PlayerDevices.Menu;
            if (navDraftWaits && (screen != draftScreen || m.up || m.down || m.left || m.right)) navDraftWaits = false;
            GameObject sel = es.currentSelectedGameObject;
            if (screen != null && !(navDraftWaits && sel == null))
            {
                if (sel == null || !sel.activeInHierarchy || !sel.transform.IsChildOf(screen.transform) || !NavLive(sel)) // NIGHT16 polish #57: a control disabled under the focus gives it up
                {
                    GameObject p = PrimaryOf(screen);
                    if (p != sel)
                    {
                        es.SetSelectedGameObject(p);
                        sel = p;
                    }
                }
            }
            else if (sel != null)
            {
                es.SetSelectedGameObject(null); // no menu: nothing to submit while playing
                sel = null;
            }
            if (sel != navSelected)
            {
                navSelected = sel;
                FocusOn(sel);
                SettingsSelected(sel);
            }
            // Round 3: the MODS draft shows card 1 focused when it opens (the frame parked on it; nothing selected until a move,
            // so a jump's A never takes a card)
            if (navDraftWaits && sel == null && screen == draftScreen && cards.Count > 0 && cards[0].go.activeInHierarchy && navCursor != null && navCursor.parent != cards[0].go.transform)
                FocusOn(cards[0].go);
            StepNavSlide(frameDt); // NIGHT16 polish #8
            TabSlideStep19(frameDt); // NIGHT19: LB / RB's tab entrance
            SettingsTick(frameDt); // NIGHT17: the rows' entrances; a pad plugged in or out re-picks SETTINGS' tabs

            bool swallow = ControlsSwallowing;
            if (es.sendNavigationEvents == swallow) es.sendNavigationEvents = !swallow;
            if ((m.back || m.start) && !swallow && Time.frameCount > navPausedFrame) PadBack(m.back);
            if ((m.tabPrev || m.tabNext) && !swallow && screen != null) PadTab19(m.tabNext ? 1 : -1); // NIGHT19: LB / RB
        }

        /// <summary>
        /// NIGHT19 U3-33.3 (design.pad.n19: PlayerDevices.Menu's tabPrev / tabNext): LB / RB switch the open screen's tabs, with
        /// each screen's own entrance: SETTINGS' tabs (SwitchTab), CONTROLS' KEYBOARD + MOUSE / GAMEPAD pages, RESULTS'
        /// SUMMARY / DETAILS, TITLE's run setup MODE (SURVIVAL / DAILY). Other screens have no tabs: nothing.
        /// </summary>
        void PadTab19(int dir)
        {
            if (controlsOpen) ControlsTab19(dir);
            else if (settingsOpen) SwitchTab(dir);
            else if (MetaOpen) { }
            else if (navScreen == results) ResultsTab19(dir);
            else if (navScreen == title) SetupTab19(dir);
        }

        // NIGHT19: a tab's content slides in from the side the bumper points to (RB, the next tab: from the right), fading in
        // with it over ThemeInTime (MenuMotion18.Enter: the menus' ease-out-back); REDUCED MOTION: the fade alone. One at a
        // time (a new switch takes over); stepped by RenderNav's caller each frame (TabSlideStep19).
        RectTransform tabSlideRt19;
        CanvasGroup tabSlideGroup19;
        Vector2 tabSlideHome19;
        double tabSlideAge19 = double.MaxValue;
        int tabSlideDir19;

        void TabSlideStart19(RectTransform rt, int dir)
        {
            if (tabSlideRt19 != null && tabSlideAge19 < UnityTuning.ThemeInTime) TabSlideEnd19(); // the last one lands at once
            if (rt == null || !FeelAB.On) return;
            tabSlideRt19 = rt;
            tabSlideGroup19 = rt.GetComponent<CanvasGroup>();
            if (tabSlideGroup19 == null) tabSlideGroup19 = rt.gameObject.AddComponent<CanvasGroup>();
            tabSlideHome19 = rt.anchoredPosition;
            tabSlideDir19 = dir;
            tabSlideAge19 = 0;
            TabSlideStep19(0);
        }

        void TabSlideStep19(double frameDt)
        {
            if (tabSlideRt19 == null || tabSlideAge19 >= UnityTuning.ThemeInTime) return;
            tabSlideAge19 += frameDt;
            if (tabSlideAge19 >= UnityTuning.ThemeInTime || !tabSlideRt19.gameObject.activeInHierarchy)
            {
                TabSlideEnd19();
                return;
            }
            float e = MenuMotion18.Enter(tabSlideAge19);
            bool still = game.settings.View.reducedMotion;
            tabSlideRt19.anchoredPosition = tabSlideHome19 + (still ? Vector2.zero : new Vector2(tabSlideDir19 * (1f - e) * UnityTuning.TabSlideDistance19, 0));
            tabSlideGroup19.alpha = Mathf.Clamp01(e);
        }

        void TabSlideEnd19()
        {
            tabSlideAge19 = double.MaxValue;
            if (tabSlideRt19 == null) return;
            tabSlideRt19.anchoredPosition = tabSlideHome19;
            if (tabSlideGroup19 != null) tabSlideGroup19.alpha = 1;
            tabSlideRt19 = null;
        }

        /// <summary>menus.js padInput: B goes back from a sub-screen or resumes from PAUSED; START (or B) resumes from PAUSED.</summary>
        void PadBack(bool back)
        {
            if (back) Pg3Audio.Ui("ui-back"); // PG3 sound74: the back blip (Pg3Audio.cs; silent with the row off)
            if (back)
            {
                if (controlsOpen)
                {
                    CloseControls();
                    return;
                }
                if (settingsOpen)
                {
                    CloseSettings();
                    return;
                }
                if (MetaOpen)
                {
                    CloseMeta();
                    return;
                }
            }
            if (game.flow == Flow.Paused && !game.DraftOpen && !settingsOpen && !controlsOpen) game.Resume();
        }

        /// <summary>The open screen: the last active child of the menus layer (drawn on top).</summary>
        GameObject TopScreen()
        {
            for (int i = menus.childCount - 1; i >= 0; i--)
            {
                Transform c = menus.GetChild(i);
                if (c != navCursor && c.gameObject.activeSelf) return c.gameObject;
            }
            return null;
        }

        /// <summary>menus.js primaryIndex: the control a screen selects when it opens.</summary>
        GameObject PrimaryOf(GameObject screen)
        {
            GameObject want = screen == title ? navPrimaryTitle
                : screen == pause ? navPrimaryPause
                : screen == results ? restartButton
                : screen == settingsScreen ? (SettingsFirstRow() ?? CurrentTabButton()) // NIGHT17 round 1: the first row lit
                : null;
            if (want != null && want.activeInHierarchy)
            {
                for (int i = 0; i < navList.Count; i++) if (navList[i].gameObject == want) return want;
            }
            return navList.Count > 0 ? navList[0].gameObject : null;
        }

        /// <summary>The explicit links of every live Selectable under `screen`, in reading order (the header).</summary>
        void Relink(GameObject screen)
        {
            screen.GetComponentsInChildren(false, navScratch);
            for (int i = 0; i < navScratch.Count; i++)
            {
                UnityEngine.UI.Selectable s = navScratch[i];
                if (s.IsActive() && s.IsInteractable())
                {
                    navList.Add(s);
                    if (!s.TryGetComponent(out NavHover _)) s.gameObject.AddComponent<NavHover>(); // NIGHT17: hover selects (once per control)
                }
            }
            int n = navList.Count;
            if (n == 0) return;
            if (navX.Length < n)
            {
                int size = n * 2;
                navX = new float[size];
                navY = new float[size];
                navMinX = new float[size];
                navMaxX = new float[size];
                navOrder = new int[size];
                navRowOf = new int[size];
                navRowStart = new int[size + 1];
            }
            for (int i = 0; i < n; i++)
            {
                var rt = (RectTransform)navList[i].transform;
                Rect r = rt.rect;
                Vector3 c = rt.TransformPoint(r.center);
                navX[i] = c.x;
                navY[i] = c.y;
                navMinX[i] = rt.TransformPoint(new Vector3(r.xMin, r.center.y, 0)).x;
                navMaxX[i] = rt.TransformPoint(new Vector3(r.xMax, r.center.y, 0)).x;
                navOrder[i] = i;
            }
            // Top to bottom by centre (insertion sort: a handful of controls, no delegate)
            for (int i = 1; i < n; i++)
            {
                int k = navOrder[i], j = i - 1;
                while (j >= 0 && navY[navOrder[j]] < navY[k])
                {
                    navOrder[j + 1] = navOrder[j];
                    j--;
                }
                navOrder[j + 1] = k;
            }
            // Rows: centres within the tolerance of the row's first; each row left to right
            float tol = UnityTuning.NavRowTolerance * Mathf.Abs(menus.lossyScale.y);
            int rows = 0;
            for (int i = 0; i < n; i++)
            {
                if (i == 0 || navY[navOrder[navRowStart[rows - 1]]] - navY[navOrder[i]] > tol) navRowStart[rows++] = i;
            }
            navRowStart[rows] = n;
            for (int r = 0; r < rows; r++)
            {
                for (int i = navRowStart[r] + 1; i < navRowStart[r + 1]; i++)
                {
                    int k = navOrder[i], j = i - 1;
                    while (j >= navRowStart[r] && navX[navOrder[j]] > navX[k])
                    {
                        navOrder[j + 1] = navOrder[j];
                        j--;
                    }
                    navOrder[j + 1] = k;
                }
                for (int i = navRowStart[r]; i < navRowStart[r + 1]; i++) navRowOf[i] = r;
            }
            bool sides = screen != settingsScreen; // SETTINGS: left / right change a row or switch the tab
            for (int i = 0; i < n; i++)
            {
                UnityEngine.UI.Selectable s = navList[navOrder[i]];
                UnityEngine.UI.Selectable up = rows > 1 ? Vertical(navOrder[i], 1, n, tol, rows) : null;
                UnityEngine.UI.Selectable down = rows > 1 ? Vertical(navOrder[i], -1, n, tol, rows) : null;
                UnityEngine.UI.Selectable left = null, right = null;
                if (sides && n > 1)
                {
                    left = navList[navOrder[(i - 1 + n) % n]];
                    right = navList[navOrder[(i + 1) % n]];
                }
                s.navigation = new UnityEngine.UI.Navigation
                {
                    mode = UnityEngine.UI.Navigation.Mode.Explicit,
                    selectOnUp = up,
                    selectOnDown = down,
                    selectOnLeft = left,
                    selectOnRight = right,
                };
            }
        }

        /// <summary>
        /// Up (dir +1) or down (-1) from control `i` (an index into navList): the best control beyond the row tolerance that
        /// way, scored by the distance that way plus UnityTuning.NavColumnWeight x the horizontal gap between the two controls
        /// (0 when they overlap: the same column wins, as uGUI's automatic navigation favours it), ties to the nearest centre.
        /// Nothing that way: wrap to the far end's row (the top going down, the bottom going up), the least gap there.
        /// </summary>
        UnityEngine.UI.Selectable Vertical(int i, int dir, int n, float tol, int rows)
        {
            int best = -1;
            float bestScore = float.MaxValue, bestTie = float.MaxValue;
            for (int j = 0; j < n; j++)
            {
                if (j == i) continue;
                float dy = (navY[j] - navY[i]) * dir;
                if (dy <= tol) continue;
                float score = dy + UnityTuning.NavColumnWeight * Gap(i, j), tie = Mathf.Abs(navX[j] - navX[i]);
                if (score < bestScore || (score == bestScore && tie < bestTie))
                {
                    best = j;
                    bestScore = score;
                    bestTie = tie;
                }
            }
            if (best < 0)
            {
                // the far row: the first row going down past the bottom, the last going up past the top
                int far = dir < 0 ? 0 : rows - 1;
                for (int k = navRowStart[far]; k < navRowStart[far + 1]; k++)
                {
                    int j = navOrder[k];
                    if (j == i) continue;
                    float gap = Gap(i, j), tie = Mathf.Abs(navX[j] - navX[i]);
                    if (gap < bestScore || (gap == bestScore && tie < bestTie))
                    {
                        best = j;
                        bestScore = gap;
                        bestTie = tie;
                    }
                }
            }
            return best >= 0 ? navList[best] : null;
        }

        /// <summary>The horizontal gap between two controls' spans (0 when they overlap).</summary>
        float Gap(int a, int b) => Mathf.Max(0, Mathf.Max(navMinX[a], navMinX[b]) - Mathf.Min(navMaxX[a], navMaxX[b]));

        /// <summary>NIGHT16 polish #57: a control focus may rest on (no Selectable: anything; else an interactable one).</summary>
        static bool NavLive(GameObject go) => !go.TryGetComponent(out UnityEngine.UI.Selectable s) || s.IsInteractable();

        /// <summary>
        /// NIGHT16 polish #57 (u-small2-n16, 91caa6b; menus.js :203, :670): a button on or off: off, it is at 35 % (a
        /// CanvasGroup's alpha; the uGUI tint left white so it isn't dimmed twice) and the focus order skips it (relinked next
        /// frame). Set on a change only.
        /// </summary>
        void NavSetEnabled(UnityEngine.UI.Selectable s, bool on)
        {
            if (s == null || s.interactable == on) return;
            var colors = s.colors;
            colors.disabledColor = Color.white;
            s.colors = colors;
            s.interactable = on;
            // Round 3 (the reviewer: REROLL's plate read translucent at 35 %): the plate stays opaque; its label dims to MUTED.
            Transform lt = s.transform.Find("Label");
            if (lt != null && lt.TryGetComponent(out TMPro.TextMeshProUGUI label)) label.color = on ? UiTheme.White : UiTheme.Muted;
            navDirty = true;
        }

        /// <summary>
        /// NIGHT16 polish #55 (u-small2-n16, 91caa6b; menus.js :201 / :205), in NIGHT17's look: the focused menu button's label
        /// (the 420 px buttons, which carry UiPress) turns INK on the frame's CYAN fill; its colour comes back on blur unless
        /// its owner changed it meanwhile. True when `go` is such a button (the frame then takes CYAN).
        /// </summary>
        bool NavTintLabel(GameObject go)
        {
            if (navTintLabel != null)
            {
                if (navTintLabel.color == UiTheme.InkSolid) navTintLabel.color = navTintWas;
                navTintLabel = null;
            }
            if (go == null || !go.TryGetComponent(out UiPress _) || !go.TryGetComponent(out UnityEngine.UI.Selectable s)) return false;
            if (!NavLook.TintsLabel(true, s.IsInteractable())) return false;
            Transform lt = go.transform.Find("Label");
            if (lt == null || !lt.TryGetComponent(out TMPro.TextMeshProUGUI label)) return false;
            navTintWas = label.color;
            navTintLabel = label;
            label.color = UiTheme.InkSolid;
            return true;
        }

        /// <summary>The focus onto `go`: a menu item lights itself (MenuFocus); anything else gets the frame behind its label.</summary>
        void FocusOn(GameObject go)
        {
            Pg3Audio.Ui("ui-move"); // PG3 sound74: the cursor tick (Pg3Audio.cs; silent with the row off)
            if (navCursor == null) BuildNavCursor(); // its last control was destroyed with it
            if (MenuFocus(go)) // NIGHT17 U3-30.8: the Persona item: lit plate, INK label (no frame)
            {
                NavTintLabel(null);
                navCursor.gameObject.SetActive(false);
                navCursor.SetParent(menus, false);
                return;
            }
            bool button = NavTintLabel(go); // NIGHT16 polish #55
            if (navCursorFill != null)
            {
                bool row = SettingsIsRow(go); // round 5: a settings row is lit with the full CYAN (INK text), as a button
                navCursorFill.color = button || row ? UiTheme.Cyan : UiTheme.CyanDeep;
                navCursorBar.color = button || row ? UiTheme.CyanHot : UiTheme.Cyan;
            }
            if (go == null)
            {
                navCursor.gameObject.SetActive(false);
                navCursor.SetParent(menus, false);
                return;
            }
            // NIGHT16 polish #8: where the frame was (a visible frame on the same screen), in the new control's space: the
            // slide starts there. A frame parked or on another screen appears in place.
            bool wasShown25 = navCursor.gameObject.activeInHierarchy;
            bool slide = navCursor.gameObject.activeInHierarchy && navScreen != null && navCursor.parent != null && navCursor.parent.IsChildOf(navScreen.transform)
                && go.transform is RectTransform && !ReducedMotionNav;
            if (slide)
            {
                navCursor.GetWorldCorners(navCorners);
                var to = (RectTransform)go.transform;
                Vector2 min = to.InverseTransformPoint(navCorners[0]), max = to.InverseTransformPoint(navCorners[2]);
                navSlideMin = min - to.rect.min;
                navSlideMax = max - to.rect.max;
                navSlideAge = 0;
            }
            else navSlideAge = float.MaxValue;
            navCursor.SetParent(go.transform, false);
            navCursor.anchorMin = Vector2.zero;
            navCursor.anchorMax = Vector2.one;
            float o = UnityTuning.NavFocusOutset;
            navCursor.offsetMin = slide ? navSlideMin : new Vector2(-o, -o);
            navCursor.offsetMax = slide ? navSlideMax : new Vector2(o, o);
            navCursor.SetAsFirstSibling(); // over the control's panel, under its label
            navCursor.gameObject.SetActive(true);
            if (!wasShown25 && !ReducedMotionNav)
            {
                navFadeAge25 = 0; // appears in place (no slide from a frame on screen): a short fade-in, not a full-alpha first frame
                navCursorGroup.alpha = 0;
            }
            else if (navCursorGroup != null) navCursorGroup.alpha = 1f;
        }

        /// <summary>NIGHT16 polish #8: REDUCED MOTION shows the frame in place (no slide).</summary>
        bool ReducedMotionNav => game.settings != null && game.settings.View.reducedMotion;

        /// <summary>NIGHT16 polish #8: the frame's slide toward its control's outset (ease-out over UnityTuning.NavFocusSlideTime, frame time).</summary>
        void StepNavSlide(double frameDt)
        {
            if (navCursorGroup != null && navFadeAge25 < UnityTuning.Fade25In)
            {
                navFadeAge25 += frameDt;
                navCursorGroup.alpha = UiFade25.In(navFadeAge25, frameDt);
            }
            if (navCursor == null || navSlideAge >= UnityTuning.NavFocusSlideTime) return;
            navSlideAge += (float)frameDt;
            float t = Mathf.Clamp01(navSlideAge / UnityTuning.NavFocusSlideTime);
            float k = 1 - (1 - t) * (1 - t) * (1 - t); // ease-out cubic: most of the travel in the first frames
            float o = UnityTuning.NavFocusOutset;
            navCursor.offsetMin = Vector2.LerpUnclamped(navSlideMin, new Vector2(-o, -o), k);
            navCursor.offsetMax = Vector2.LerpUnclamped(navSlideMax, new Vector2(o, o), k);
        }

        /// <summary>Select `go` (a click on a row's - / + selects its row; a tab switch selects the new tab).</summary>
        static void SelectNav(GameObject go)
        {
            if (EventSystem.current != null && go != null) EventSystem.current.SetSelectedGameObject(go);
        }

        /// <summary>Left / right on `go` call `side` (SETTINGS' tabs, RESET TAB and BACK switch the tab).</summary>
        static void NavSideOf(GameObject go, System.Action<int> side) => go.AddComponent<NavSide>().side = side;
    }
}
