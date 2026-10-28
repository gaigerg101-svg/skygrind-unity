// NIGHT17 U3-30.8 (builder N, menus and words; GG after NIGHT16: "the menus don't really follow proper game design ... way
// too wordy ... way too cluttered and has way too many options"). The menu half of Ui, rebuilt on Persona 5's grammar: one
// hero colour (CYAN) on near-black INK, text only WHITE / MUTED (or INK on CYAN), every column on the one 12 deg diagonal
// (each lower item further left by tan(slant) x its drop), the focused item lit (a CYAN plate, INK label, nudged along its
// row) and the rest dimmed (MUTED labels, no plate), an INK slab behind each column, the world behind a menu darkened by an
// INK sheet (no translucent grey plates), every screen entering along the slant (UiEnter: in 150 ms ease-out-back, the
// items 30 ms apart). Mouse hover selects (UiNav's NavHover).
// - TITLE: the wordmark, one big PLAY (RESUME beside it while a saved run exists), the run setup as one card (UiMeta.cs:
//   MODE / HEAT / MAP rows, a press cycles), PROGRESS (CONTRACTS, WEAPONS), SETTINGS, QUIT: 5 rows. No key line; the
//   version stamp only in a developer build (UiReport.cs).
// - PAUSED: a one-line run header (round · points · time) and RESUME / RESTART / SETTINGS / QUIT; QUIT asks once
//   (MenuRules.Quit) and saves at a break where a save is allowed (SAVE & QUIT). The keys live in SETTINGS > CONTROLS, AIM
//   ASSIST in SETTINGS > CONTROLS; COPY REPORT only in a developer build.
// - DOWN: DOWN in the display face in DANGER, the round and points, who got you, the restart glyph.
// - RESULTS: RETRY (R), TITLE, DETAILS (UiResults.cs: the hero tally); COPY SCORE LINE only in a developer build.
// Buttons keep their GameObject names (the label they were built with): the smoke finds "SETTINGS", TITLE's "PLAY" and
// PAUSED's "RESUME" by name. Per frame: the items' focus ease and entrances (a few floats each, written only while moving).
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        // Screens
        GameObject title, pause, down, results, restartButton, titleButton, retryButton, copyButton;
        TextMeshProUGUI wordmark, wordmarkBack;
        TextMeshProUGUI downHead, downBody, downRecap;
        Dictionary<string, string> gradeHex; // results.gradeColors as hex (NIGHT11)
        GameObject titleResume, pauseSaveQuit; // RESUME (a saved run); pauseSaveQuit: none since NIGHT17 (QUIT saves at a break)
        bool titleResumeShown;
        GameObject titlePlay; // TITLE's PLAY (focused first)
        GameObject resultsSaveCard, resultsCopySeed; // RESULTS' SAVE CARD / COPY SEED (UiRoute.cs places them; a developer's)

        /// <summary>One menu item: a slanted plate (hidden at rest, CYAN when focused), its label, its entrance and focus ease.</summary>
        sealed class MenuItemView
        {
            public GameObject go;
            public RectTransform rt;
            public UiShape plate, bar; // the plate (INK-SOFT at rest, CYAN lit) and the accent stroke left of it (lit only)
            public TextMeshProUGUI label;
            public CanvasGroup group;
            public Vector2 home;
            public bool hero; // PLAY / RETRY: a WHITE label at rest (the one focal item)
            public float lit = -1; // 0 rest .. 1 focused (-1: apply on the next step)
            public UiEnter enter;
            public double wait; // the stagger before its entrance starts
            public bool entering;
            public float nudge18; // NIGHT18: the focus slide's progress (0 .. 1, linear in time; eased when drawn)
        }

        /// <summary>NIGHT18 U3-31.7: a screen part that isn't an item (a head, the run line, a tab, the footer): its entrance.</summary>
        sealed class MenuPart18
        {
            public RectTransform rt;
            public CanvasGroup group;
            public Vector2 home, written; // its rest, and the position last written (another writer moved it: new rest)
            public double age, wait;
            public bool entering;
        }

        /// <summary>NIGHT18 U3-31.7: a screen under the menus layer: its INK sheet's fade and its parts' entrances.</summary>
        sealed class MenuScreen18
        {
            public GameObject screen;
            public UnityEngine.UI.Image sheet;
            public float sheetRest, sheetFrom;
            public double age;
            public bool wasActive, entering, ownSlab;
            public readonly List<MenuPart18> parts = new List<MenuPart18>(8);
            public float partDistance = UnityTuning.ThemeEnterDistance; // round 3: MODS' cards come from farther ...
            public double partCascade = -1; // ... and further apart (s; -1: MenuMotion18.Wait / SlabWait)
        }

        /// <summary>The parts of `screen` showing now (its items' cascade starts after them).</summary>
        int MenuPartsShown18(GameObject screen)
        {
            int n = 0;
            MenuScreen18 s = MenuScreenOf18(screen);
            for (int i = 0; i < s.parts.Count; i++) if (s.parts[i].rt.gameObject.activeInHierarchy) n++;
            return n;
        }

        readonly List<MenuScreen18> menuScreens18 = new List<MenuScreen18>(12);
        float menuSheetShown18; // the top screen's sheet alpha last frame: the next screen's sheet fades from it

        readonly List<MenuItemView> menuItems = new List<MenuItemView>(32);
        readonly Dictionary<GameObject, MenuItemView> menuItemOf = new Dictionary<GameObject, MenuItemView>(32);
        MenuItemView menuLit; // the item the focus is on (null: none)

        /// <summary>A screen's INK slab (a slanted UiShape from off the left edge) and its entrance.</summary>
        sealed class MenuSlabView
        {
            public GameObject screen;
            public RectTransform rt;
            public Vector2 home;
            public UiEnter enter;
            public bool wasActive;
            public bool sweep18; // NIGHT18 round 3: this entrance sweeps along the slant from MenuSlabEnter18 px
        }

        readonly List<MenuSlabView> menuSlabs = new List<MenuSlabView>(6);
        TextMeshProUGUI menuPauseRunLine;
        MenuItemView menuPauseQuit, menuPauseReport, menuResultsCopy;
        MenuRules.QuitConfirm menuQuitAsk;
        UiSlam menuDownSlam; // NIGHT17 round 1: DOWN's word slams in
        bool menuDownShown;
        bool menuQuitArmedShown, menuPauseSaveShown, menuDevShown;
        double menuWordmarkClock;
        float menuWordmarkShown = -1;
        static readonly float MenuSlantK = Mathf.Tan(UnityTuning.ThemeSlantDeg * Mathf.Deg2Rad);
        /// <summary>Round 2: an item's plate at rest: INK-SOFT, opaque (the world's HDR trim showed through 96 %).</summary>
        static Color MenuPlate => UiTheme.A(UiTheme.InkSoft, 1f);

        /// <summary>The column's left edge at height y (MenuRules.Left: the one diagonal, inside the safe inset at 16:10).</summary>
        static float MenuLeft(float y) => MenuRules.Left(y);

        /// <summary>An item `width` wide on the column at height y: its centre.</summary>
        static Vector2 MenuAt(float y, float width) => new Vector2(MenuLeft(y) + width * 0.5f, y);

        /// <summary>A text placed by its box's left edge at (x, y) (Text() centres the box on its position).</summary>
        static void MenuPinLeft(TMP_Text t, float x, float y)
        {
            RectTransform rt = t.rectTransform;
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
        }

        /// <summary>NIGHT17 U3-30.10: a sub-screen's head (CONTROLS, CONTRACTS, WEAPONS): DISPLAY S56 white with the ink shadow, by
        /// its left edge on the column, its top inside the safe inset.</summary>
        TextMeshProUGUI MenuHead(Transform screen, string s, float y)
        {
            float px = UiTheme.Px(UiTheme.Size.S56);
            y = Mathf.Min(y, 540f - UnityTuning.ThemeSafeInset - 35f);
            TextMeshProUGUI t = Text(screen, "Head", s, px, UiTheme.White, TextAlignmentOptions.Left, Vector2.zero, new Vector2(900, 70), new Vector2(0.5f, 0.5f));
            UiTheme.Style(t, px, UiTheme.White, UiTheme.Display, shadow: true);
            t.fontStyle = FontStyles.Normal;
            MenuPinLeft(t, MenuLeft(y), y);
            MenuPart18Add(screen.gameObject, t); // NIGHT18: the head enters with its screen (CONTROLS, CONTRACTS, WEAPONS)
            return t;
        }

        /// <summary>
        /// A menu item on `screen` (a direct child, named after its label so Find(label) and the smoke see it): the plate
        /// (UiShape, the click target), the label (DISPLAY at `px`, left-aligned on the plate), a CanvasGroup for its entrance.
        /// </summary>
        MenuItemView MenuItem(Transform screen, string label, Vector2 pos, Vector2 size, UiTheme.Size px, Action onClick, bool hero = false)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape), typeof(CanvasGroup));
            var rt = (RectTransform)go.transform;
            rt.SetParent(screen, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            var plate = go.GetComponent<UiShape>();
            plate.color = MenuPlate; // IDENTITY.md: the rest INK-SOFT (opaque: round 2), the focused one CYAN
            plate.raycastTarget = true;
            var b = go.AddComponent<UnityEngine.UI.Button>();
            b.transition = UnityEngine.UI.Selectable.Transition.None; // the item lights itself (MenuFocus)
            b.targetGraphic = plate;
            b.onClick.AddListener(() => onClick());
            go.AddComponent<UiPress>(); // the press dips it (visual only)
            float pad = UnityTuning.ThemeCut * 2.4f;
            TextMeshProUGUI t = Text(go.transform, "Label", label, UiTheme.Px(px), UiTheme.Muted, TextAlignmentOptions.Left, new Vector2(pad * 0.5f, 0), new Vector2(size.x - pad, size.y), new Vector2(0.5f, 0.5f));
            UiTheme.Style(t, UiTheme.Px(px), UiTheme.White, UiTheme.Display);
            t.fontStyle = FontStyles.Normal; // the display face is an italic cut already
            t.overflowMode = TextOverflowModes.Overflow;
            // The accent stroke (the tile's slanted bar left of the lit row): a narrow UiShape, CYAN-HOT when lit.
            var barGo = new GameObject("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
            var brt = (RectTransform)barGo.transform;
            brt.SetParent(go.transform, false);
            brt.anchorMin = brt.anchorMax = new Vector2(0, 0.5f);
            brt.sizeDelta = new Vector2(UnityTuning.ThemeAccentBar * 2, size.y);
            brt.anchoredPosition = new Vector2(-UnityTuning.ThemeAccentBar * 4, 0);
            var bar = barGo.GetComponent<UiShape>();
            bar.SetCuts(0, 0);
            bar.color = UiTheme.A(UiTheme.CyanHot, 0f);
            bar.raycastTarget = false;
            var item = new MenuItemView { go = go, rt = rt, plate = plate, bar = bar, label = t, group = go.GetComponent<CanvasGroup>(), home = pos, hero = hero };
            menuItems.Add(item);
            menuItemOf[go] = item;
            return item;
        }

        /// <summary>A screen's INK slab behind its column: a slanted solid from off the left edge to `right` (x at y = 0).</summary>
        void MenuSlab(GameObject screen, float right)
        {
            var go = new GameObject("Slab", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
            var rt = (RectTransform)go.transform;
            rt.SetParent(screen.transform, false);
            rt.SetAsFirstSibling();
            float left = -1200f, top = 600f, bottom = -600f; // past every edge at any aspect the scaler gives (it matches the height)
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(right - left, top - bottom);
            rt.anchoredPosition = new Vector2((right + left) * 0.5f, (top + bottom) * 0.5f);
            var shape = go.GetComponent<UiShape>();
            shape.color = UiTheme.InkSolid; // round 2: opaque (HDR trim showed through 96 %)
            shape.raycastTarget = false;
            shape.SetCuts(0, 0);
            menuSlabs.Add(new MenuSlabView { screen = screen, rt = rt, home = rt.anchoredPosition });
        }

        /// <summary>The constructor's screens block: TITLE, PAUSED, DOWN, RESULTS.</summary>
        void BuildMenus()
        {
            // --- TITLE: the wordmark, PLAY (+ RESUME), the run setup card (UiMeta.cs), PROGRESS, SETTINGS, QUIT -----------
            // NIGHT17 round 1: every text and plate by its left edge on the column (it was centred on a box that ran off the
            // left edge: "KYGRIND"), the slab wide enough that the column sits centred on it, the living world dimmed a little.
            title = Screen("Title", livingTitle ? UiTheme.A(UiTheme.InkSolid, UnityTuning.TitleSheetAlpha) : UiTheme.InkSolid);
            MenuSlab(title, UnityTuning.TitleSlabRight);
            float mark = UiTheme.Px(UiTheme.Size.S133), my = UnityTuning.TitleMarkY;
            wordmarkBack = Text(title.transform, "Wordmark (back)", "SKYGRIND", mark, UiTheme.CyanDeep, TextAlignmentOptions.Left, Vector2.zero, new Vector2(900, 160), new Vector2(0.5f, 0.5f));
            wordmark = Text(title.transform, "Wordmark", "SKYGRIND", mark, UiTheme.Cyan, TextAlignmentOptions.Left, Vector2.zero, new Vector2(900, 160), new Vector2(0.5f, 0.5f));
            UiTheme.Style(wordmarkBack, mark, UiTheme.CyanDeep, UiTheme.Display);
            UiTheme.Style(wordmark, mark, UiTheme.Cyan, UiTheme.Display);
            wordmarkBack.fontStyle = wordmark.fontStyle = FontStyles.Normal;
            MenuPinLeft(wordmarkBack, MenuLeft(my) + UnityTuning.ThemeHeroOffset, my - UnityTuning.ThemeHeroOffset);
            MenuPinLeft(wordmark, MenuLeft(my), my);
            MenuWordmarkSlab(my);
            var tag = Text(title.transform, "Tagline", cfg.Str("ui.tagline"), UiTheme.Px(UiTheme.Size.S24), UiTheme.Muted, TextAlignmentOptions.Left, Vector2.zero, new Vector2(900, 32), new Vector2(0.5f, 0.5f));
            UiTheme.Style(tag, UiTheme.Px(UiTheme.Size.S24), UiTheme.Muted, UiTheme.BodyBold);
            tag.characterSpacing = UnityTuning.ThemeLabelTracking;
            MenuPinLeft(tag, MenuLeft(UnityTuning.TitleTagY) + 6, UnityTuning.TitleTagY);
            // NIGHT18: the wordmark builds along the slant (slab, bar, its deep copy, the word, the tagline) under TITLE's own
            // whole-screen fade (UiFeelHud); the run setup card and the best row join in UiMeta.BuildMeta
            MenuPart18Add(title, title.transform.Find("Wordmark slab"));
            MenuPart18Add(title, title.transform.Find("Wordmark bar"));
            MenuPart18Add(title, wordmarkBack);
            MenuPart18Add(title, wordmark);
            MenuPart18Add(title, tag);

            float y0 = UnityTuning.TitlePlayRowY;
            var play = MenuItem(title.transform, "PLAY", MenuAt(y0, UnityTuning.TitlePlayWidth), new Vector2(UnityTuning.TitlePlayWidth, UnityTuning.TitlePlayHeight), UiTheme.Size.S75, () => PlayPressed26(), hero: true); // NIGHT26: PLAY on CAMPAIGN opens the level select
            titlePlay = play.go;
            float resumeX = MenuLeft(y0) + UnityTuning.TitlePlayWidth + UnityTuning.TitleResumeGap + UnityTuning.TitleResumeWidth * 0.5f;
            titleResume = MenuItem(title.transform, "RESUME", new Vector2(resumeX, y0), new Vector2(UnityTuning.TitleResumeWidth, UnityTuning.TitleResumeHeight), UiTheme.Size.S32, () => game.ResumeSave()).go;
            titleResume.SetActive(false);
            // The run setup card sits under PLAY (UiMeta.RefreshTitleMeta places it); the column goes on under it.
            Vector2 itemSize = new Vector2(UnityTuning.MenuItemWidth, UnityTuning.MenuItemHeight);
            titleColumn.Add(MenuItem(title.transform, UnityTuning.TitleProgressLabel, Vector2.zero, itemSize, UiTheme.Size.S42, () => OpenContracts()).rt);
            titleColumn.Add(MenuItem(title.transform, "SETTINGS", Vector2.zero, itemSize, UiTheme.Size.S42, OpenSettings).rt);
            titleColumn.Add(MenuItem(title.transform, "QUIT", Vector2.zero, itemSize, UiTheme.Size.S42, Quit).rt);
            BuildCode22(); // NIGHT22 (builder W2): the TITLE CODE item and its sheet (Code22Field.cs)
            LayoutTitleColumn();

            // --- PAUSED: the head, the run header, RESUME / RESTART / SETTINGS / QUIT (+ COPY REPORT for a developer) --------
            pause = Screen("Paused", UiTheme.A(UiTheme.InkSolid, UnityTuning.MenuSheetAlpha));
            MenuSlab(pause, UnityTuning.PauseSlabRight);
            var head = Text(pause.transform, "Head", "PAUSED", UiTheme.Px(UiTheme.Size.S100), UiTheme.White, TextAlignmentOptions.Left, Vector2.zero, new Vector2(800, 120), new Vector2(0.5f, 0.5f));
            UiTheme.Style(head, UiTheme.Px(UiTheme.Size.S100), UiTheme.White, UiTheme.Display, shadow: true);
            head.fontStyle = FontStyles.Normal;
            MenuPinLeft(head, MenuLeft(UnityTuning.PauseHead17Y), UnityTuning.PauseHead17Y);
            menuPauseRunLine = Text(pause.transform, "Run", "", UiTheme.Px(UiTheme.Size.S24), UiTheme.Muted, TextAlignmentOptions.Left, Vector2.zero, new Vector2(800, 36), new Vector2(0.5f, 0.5f));
            UiTheme.Style(menuPauseRunLine, UiTheme.Px(UiTheme.Size.S24), UiTheme.Muted, UiTheme.BodyBold); // the tile: BODY muted 24 px
            menuPauseRunLine.richText = true;
            MenuPinLeft(menuPauseRunLine, MenuLeft(UnityTuning.PauseRunLineY) + 6, UnityTuning.PauseRunLineY);
            string[] items = MenuRules.PauseItems(true);
            for (int i = 0; i < items.Length; i++)
            {
                string id = items[i];
                Action act = id == "RESUME" ? (Action)(() => game.Resume())
                    : id == "RESTART" ? () => game.RestartRun()
                    : id == "SETTINGS" ? OpenSettings
                    : id == "QUIT" ? PauseQuitPressed
                    : () => CopyReport();
                MenuItemView it = MenuItem(pause.transform, id, Vector2.zero, itemSize, UiTheme.Size.S42, act, hero: id == "RESUME");
                pauseColumn.Add(it.rt);
                if (id == "QUIT") menuPauseQuit = it;
                if (id == "COPY REPORT") menuPauseReport = it;
            }
            menuPauseReport.go.SetActive(game.devMode);
            menuDevShown = game.devMode;
            LayoutPauseColumn();
            MenuPart18Add(pause, head); // NIGHT18: the head and the run line enter with the items (build 1: whole at once)
            MenuPart18Add(pause, menuPauseRunLine);

            // --- DOWN: the word in the display face (DANGER: the run's danger), the round and points, who got you ----------
            down = Screen("Down", UiTheme.A(UiTheme.InkSolid, UnityTuning.DownSheetAlpha)); // round 2: the world darkened behind DOWN
            downHead = Text(down.transform, "Head", "DOWN", UiTheme.Px(UiTheme.Size.S133), UiTheme.Danger, TextAlignmentOptions.Center, new Vector2(0, UnityTuning.DownHeadY), new Vector2(900, 160), new Vector2(0.5f, 0.5f));
            UiTheme.Style(downHead, UiTheme.Px(UiTheme.Size.S133), UiTheme.Danger, UiTheme.Display, shadow: true);
            downHead.fontStyle = FontStyles.Normal;
            downBody = Text(down.transform, "Round", "", UiTheme.Px(UiTheme.Size.S32), UiTheme.White, TextAlignmentOptions.Center, new Vector2(0, UnityTuning.DownLineY), new Vector2(1200, 44), new Vector2(0.5f, 0.5f));
            UiTheme.Style(downBody, UiTheme.Px(UiTheme.Size.S32), UiTheme.White, UiTheme.BodyBold, shadow: true);
            downRecap = Text(down.transform, "Recap", "", UiTheme.Px(UiTheme.Size.S24), UiTheme.White, TextAlignmentOptions.Top, new Vector2(0, UnityTuning.DownRecapY), new Vector2(1400, 160), new Vector2(0.5f, 0.5f));
            UiTheme.Style(downRecap, UiTheme.Px(UiTheme.Size.S24), UiTheme.White, UiTheme.BodyBold, shadow: true);
            downRecap.rectTransform.pivot = new Vector2(0.5f, 1);
            downRecap.lineSpacing = 12;
            // Round 3: the retry as a drawn key chip and its verb under the recap (none on a pad: no restart button)
            menuDownChip = KeyChip(down.transform, Vector2.zero, out menuDownChipText);
            menuDownVerb = Text(down.transform, "Retry", "RETRY", UiTheme.Px(UiTheme.Size.S24), UiTheme.CyanPale, TextAlignmentOptions.Left, Vector2.zero, new Vector2(300, 40), new Vector2(0.5f, 0.5f));
            UiTheme.Style(menuDownVerb, UiTheme.Px(UiTheme.Size.S24), UiTheme.CyanPale, UiTheme.BodyBold, shadow: true);
            // NIGHT18: under the slamming word, its lines enter one after another (the word keeps its own slam)
            MenuPart18Add(down, downBody);
            MenuPart18Add(down, downRecap);
            MenuPart18Add(down, menuDownChip);
            MenuPart18Add(down, menuDownVerb);

            // --- RESULTS: the hero tally on its own INK slab (UiResults.cs) and RETRY (R) / TITLE / DETAILS -------------------
            results = Screen("Results", UiTheme.A(UiTheme.InkSolid, UnityTuning.MenuSheetAlpha));
            BuildCode22Results(); // NIGHT23: the CODE line on RESULTS (Code22Field.cs); it needs `results`, so it is not part of BuildCode22
            MenuSlab(results, UnityTuning.ResultsSlabRight);
            float ry = UnityTuning.ResultsFirstRowY;
            var retry = MenuItem(results.transform, "RETRY", MenuAt(ry, itemSize.x), itemSize, UiTheme.Size.S42, () => RetryOrContinue25(), hero: true); // NIGHT25: CONTINUE while the campaign's results show
            restartButton = retry.go;
            // Round 3 (the reviewer: "RETRY (R)" is an ASCII key hint): RETRY with a drawn key chip beside it (the restart key;
            // none on a pad, which has no restart button: RetryLabel's rule). NIGHT16 polish #49's label (UiFeelHud.cs writes it)
            // goes to a hidden text, so its writes land nowhere visible.
            var hiddenRetry = Text(retry.go.transform, "Retry label (NIGHT16)", "", UiTheme.Px(UiTheme.Size.S18), UiTheme.White, TextAlignmentOptions.Left, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            hiddenRetry.gameObject.SetActive(false);
            retryLabelText = hiddenRetry;
            float chipX = -itemSize.x * 0.5f + UnityTuning.ThemeCut * 2.4f + retry.label.GetPreferredValues("RETRY").x + UnityTuning.KeyChipGap; // its left edge
            menuRetryChip = KeyChip(retry.go.transform, new Vector2(chipX, 0), out menuRetryChipText);
            ry -= UnityTuning.MenuItemPitch;
            titleButton = MenuItem(results.transform, "TITLE", MenuAt(ry, itemSize.x), itemSize, UiTheme.Size.S42, () => game.ToTitle()).go;
            ry -= UnityTuning.MenuItemPitch;
            resultsDetailsItem = MenuItem(results.transform, UnityTuning.ResultsDetailsLabel, MenuAt(ry, itemSize.x), itemSize, UiTheme.Size.S42, ToggleResultsDetails);
            ry -= UnityTuning.MenuItemPitch;
            // NIGHT12 U3-25.9 (seeds.js: "Same seed, same run"): with the details (a second look at a run).
            retryButton = MenuItem(results.transform, "RETRY SAME SEED", MenuAt(ry, itemSize.x), itemSize, UiTheme.Size.S32, () => game.RetrySameSeed()).go;
            ry -= UnityTuning.MenuItemPitch;
            // menus.js COPY SCORE LINE: a developer's (NIGHT17.md rule 7).
            menuResultsCopy = MenuItem(results.transform, "COPY SCORE LINE", MenuAt(ry, itemSize.x), itemSize, UiTheme.Size.S32, CopyScoreLine);
            copyButton = menuResultsCopy.go;
            // NIGHT14: SAVE CARD and COPY SEED (UiRoute.cs places them in the route plate; a developer's).
            resultsSaveCard = Button(results.transform, "SAVE CARD", new Vector2(0, -570), () => SaveRouteCard()).transform.parent.gameObject;
            resultsSaveCard.SetActive(false);
            resultsCopySeed = Button(results.transform, "COPY SEED", new Vector2(0, -640), () => CopyRouteSeed()).transform.parent.gameObject;
            resultsCopySeed.SetActive(false);
            gradeHex = new Dictionary<string, string>();
            foreach (var kv in cfg.Obj("results.gradeColors")) gradeHex[kv.Key] = ColorUtility.ToHtmlStringRGB(Look.Hex((uint)PortData.Num(kv.Value)));
            BuildTally(); // the hero tally (UiResults.cs)
            BuildCampRes25(); // NIGHT25 (builder F): the campaign's results (UiCampaignResults25.cs)
        }

        /// <summary>Render's screens part, every frame after the flow's screens were shown or hidden.</summary>
        void RenderMenus(Flow f, double frameDt)
        {
            if (title.activeSelf) MenuWordmarkBreath(frameDt);
            RenderFront33(); // NIGHT33 U (hud33): the front screens' per-frame part (UiFront33.cs); off: nothing
            RenderMenusFx20(); // NIGHT20 U3-34.10: TITLE's wordmark and RESULTS' grade reveals (UiMenusFx20.cs)
            RenderWeapons19(frameDt); // NIGHT19 U3-33.8: WEAPONS' rows cascade in (UiWeapons.cs; a no-op at rest)
            RenderContracts19(frameDt); // NIGHT19 U3-33.8: CONTRACTS' rows too (UiContracts.cs)
            if (game.devMode != menuDevShown)
            {
                menuDevShown = game.devMode; // a Play check's eval may set it
                menuPauseReport.go.SetActive(menuDevShown);
                LayoutPauseColumn();
            }
            if (f == Flow.Paused) RenderPauseMenu();
            else if (menuQuitArmedShown) PauseQuitDisarm();
            // NIGHT17 round 1: DOWN slams in (2x -> 1x in 100 ms; REDUCED MOTION: in place), every screen enters
            bool downOn = down.activeInHierarchy;
            if (downOn && !menuDownShown) menuDownSlam.Hit();
            menuDownShown = downOn;
            if (downOn && menuDownSlam.Running)
            {
                float s = game.settings.View.reducedMotion ? 1f : menuDownSlam.Step(frameDt);
                downHead.rectTransform.localScale = new Vector3(s, s, 1);
            }
            else if (downOn && downHead.rectTransform.localScale.x != 1f) downHead.rectTransform.localScale = Vector3.one;
            if (f == Flow.Down)
            {
                int r = game.run.round, p = (int)Math.Round(game.economy.total);
                if (r != shownDownRound || p != shownDownPoints)
                {
                    shownDownRound = r;
                    shownDownPoints = p;
                    downBody.SetText("ROUND {0:0}  ·  {1:0}", r, p);
                }
                if (game.lastDeath != shownDeath)
                {
                    // hud.js setDown, short: who got you, the next target, the restart glyph.
                    shownDeath = game.lastDeath;
                    DeathRecap.Summary s = game.lastDeath;
                    // NIGHT20 F4: KILLED BY [KIND] over the kind's tell (UiDownTell20.cs) in place of the KILLED BY line
                    bool told = DownTell20Set(s?.killer);
                    sb.Clear();
                    if (s?.killer != null && !told) sb.Append("<color=#").Append(UiTheme.DangerHex).Append(">KILLED BY ").Append(s.killer).Append("</color>\n");
                    if (game.downTarget != null) sb.Append("<color=#").Append(UiTheme.GoldHex).Append('>').Append(game.downTarget).Append("</color>\n");
                    if (sb.Length > 0 && sb[sb.Length - 1] == '\n') sb.Length--; // round 3: the retry is a drawn key chip under the recap (MenuDownRetry)
                    downRecap.SetText(sb);
                }
                MenuDownRetry(); // round 3: the drawn retry chip (two compares a frame; after the recap: F4's tell may move it)
            }
            if (f == Flow.Results && CampRes25Wanted)
            {
                RenderCampRes25(frameDt); // NIGHT25 (builder F): a cleared campaign level's RESULTS (UiCampaignResults25.cs)
            }
            else if (f == Flow.Results)
            {
                if (CampRes25Open) EndCampRes25();
                MenuRetryChipRefresh(); // round 3: RETRY's key chip
                if (game.lastStats == null)
                {
                    MenuResultsButtons(true); // nothing to tally: the buttons at once
                    if (tallyRoot.gameObject.activeSelf) tallyRoot.gameObject.SetActive(false);
                }
                else
                {
                    if (game.lastStats != shownStats)
                    {
                        shownStats = game.lastStats;
                        BeginTally(shownStats);
                    }
                    RenderTally(frameDt);
                }
            }
            if (f != Flow.Results && CampRes25Open) EndCampRes25(); // NIGHT25: left RESULTS
            bool resumeOn = f == Flow.Title && game.HasSave; // NIGHT14: UiFlow (RESUME stays beside PLAY over the attract)
            if (resumeOn != titleResumeShown)
            {
                titleResume.SetActive(titleResumeShown = resumeOn);
                if (resumeOn) MenuEnterItem(menuItemOf[titleResume], 0);
            }
            MenuScreens18Step(frameDt); // NIGHT18: sheets and parts (FeelAB on)
            MenuEnterScreens();
            MenuStepItems(frameDt);
        }

        /// <summary>
        /// Round 3 (the reviewer: "the button font with a one-step drop shadow"): the wordmark built from the game's own shapes.
        /// A cut slab behind it on the 12 deg slant (INK-SOFT, opaque, its top-right and bottom-left corners cut deep), a CYAN
        /// accent bar along its lower edge (on the slant: shifted by its drop; round 5: the floating notch is gone); the
        /// DISPLAY face at S133 with its CYAN-DEEP offset copy over it. Whole and inside the inset (left at -590 at 16:10's -808).
        /// </summary>
        void MenuWordmarkSlab(float my)
        {
            float textW = wordmark.GetPreferredValues("SKYGRIND").x;
            float padX = UnityTuning.TitleMarkPadX, h = UnityTuning.TitleMarkPlateHeight;
            float left = MenuLeft(my) - padX * 0.6f, w = textW + padX * 2f;
            int at = wordmarkBack.transform.GetSiblingIndex();
            UiShape plate = MenuShape(title.transform, "Wordmark slab", MenuPlate, new Vector2(left + w * 0.5f, my), new Vector2(w, h));
            plate.SetCuts(UnityTuning.TitleMarkCut, UnityTuning.TitleMarkCut);
            plate.transform.SetSiblingIndex(at);
            float barH = UnityTuning.TitleMarkBar, barY = my - h * 0.5f + barH * 0.5f;
            float barW = w - UnityTuning.TitleMarkCut * 1.2f;
            UiShape bar = MenuShape(title.transform, "Wordmark bar", UiTheme.Cyan, new Vector2(left + UnityTuning.TitleMarkCut * 1.2f + barW * 0.5f - MenuSlantK * (my - barY), barY), new Vector2(barW, barH));
            bar.SetCuts(0, 0);
            bar.transform.SetSiblingIndex(at + 1);
        }

        UiShape menuRetryChip, menuDownChip;
        TextMeshProUGUI menuRetryChipText, menuDownChipText, menuDownVerb;
        InputDevice menuDownDevice = (InputDevice)(-1);
        string menuDownKey;

        /// <summary>Round 3: DOWN's retry row (a drawn key chip + RETRY, centred under the recap); hidden on a pad (no restart button).</summary>
        void MenuDownRetry()
        {
            string key = PromptText.For("restart", false, PlayerDevices.Last == InputDevice.Pad ? null : restartLabel); // NIGHT19: PromptText (builder P)
            if (PlayerDevices.Last == menuDownDevice && ReferenceEquals(key, menuDownKey)) return;
            menuDownDevice = PlayerDevices.Last;
            menuDownKey = key;
            KeyChipSet(menuDownChip, menuDownChipText, key);
            menuDownVerb.gameObject.SetActive(key != null);
            if (key == null) return;
            float chipW = menuDownChip.rectTransform.sizeDelta.x, verbW = menuDownVerb.GetPreferredValues("RETRY").x;
            float left = -(chipW + UnityTuning.KeyChipGap + verbW) * 0.5f, y = UnityTuning.DownRetryY - downTellRetryShift20; // NIGHT20 F4: under the tell
            menuDownChip.rectTransform.anchoredPosition = new Vector2(left, y);
            MenuPinLeft(menuDownVerb, left + chipW + UnityTuning.KeyChipGap, y);
        }
        InputDevice menuRetryDevice = (InputDevice)(-1);
        string menuRetryKey;

        /// <summary>
        /// Round 3: a drawn key chip (a key's name in BODY-BOLD white on a small opaque INK-SOFT cut plate, sized to it), for
        /// RETRY's restart key, DOWN's retry and WHAT GOT YOU's skip, in place of ASCII "[R]".
        /// </summary>
        UiShape KeyChip(Transform parent, Vector2 pos, out TextMeshProUGUI label)
        {
            UiShape chip = MenuShape(parent, "Key chip", MenuPlate, pos, new Vector2(UnityTuning.KeyChipSize, UnityTuning.KeyChipSize));
            chip.rectTransform.pivot = new Vector2(0, 0.5f); // `pos` is its left edge: it grows to the right with its key
            chip.rectTransform.anchoredPosition = pos;
            chip.SetCuts(UnityTuning.ThemeCut * 0.6f, UnityTuning.ThemeCut * 0.6f);
            chip.Slant = 0; // a key cap stands upright
            float px = UiTheme.Px(UiTheme.Size.S24);
            label = Text(chip.transform, "Key", "", px, UiTheme.White, TextAlignmentOptions.Center, Vector2.zero, new Vector2(UnityTuning.KeyChipSize, UnityTuning.KeyChipSize), new Vector2(0.5f, 0.5f));
            UiTheme.Style(label, px, UiTheme.White, UiTheme.BodyBold);
            return chip;
        }

        /// <summary>A chip's key text, its plate sized to it (a string set on a change only).</summary>
        static void KeyChipSet(UiShape chip, TextMeshProUGUI label, string key)
        {
            bool on = !string.IsNullOrEmpty(key);
            chip.gameObject.SetActive(on);
            if (!on) return;
            label.text = key;
            float w = Mathf.Max(UnityTuning.KeyChipSize, label.GetPreferredValues(key).x + UnityTuning.KeyChipPad * 2);
            chip.rectTransform.sizeDelta = new Vector2(w, UnityTuning.KeyChipSize);
            label.rectTransform.sizeDelta = new Vector2(w, UnityTuning.KeyChipSize);
        }

        /// <summary>RETRY's chip follows the device and the binding (checked per frame on RESULTS: two compares).</summary>
        void MenuRetryChipRefresh()
        {
            string key = PromptText.For("restart", false, PlayerDevices.Last == InputDevice.Pad ? null : KeyLabel(cfg, "restart")); // NIGHT19: PromptText (builder P)
            if (PlayerDevices.Last == menuRetryDevice && ReferenceEquals(key, menuRetryKey)) return;
            menuRetryDevice = PlayerDevices.Last;
            menuRetryKey = key;
            KeyChipSet(menuRetryChip, menuRetryChipText, key);
            // the chip grows to the right from its left edge (beside the word)
        }

        /// <summary>A solid slanted UiShape (no raycast) on `parent`, centred at `pos`.</summary>
        static UiShape MenuShape(Transform parent, string name, Color c, Vector2 pos, Vector2 size)
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
            return s;
        }

        /// <summary>The wordmark breathes (menus.js @keyframes logo: brightness 1 -> 1.18 -> 1), CYAN over its CYAN-DEEP copy.</summary>
        void MenuWordmarkBreath(double frameDt)
        {
            if (game.settings != null && game.settings.View.reducedMotion) return;
            if (front33) return; // NIGHT33 U (hud33): a still bone wordmark (motion only on events)
            menuWordmarkClock += frameDt;
            float k = 0.5f - 0.5f * Mathf.Cos((float)(menuWordmarkClock * 2 * Math.PI / UnityTuning.WordmarkBreathTime));
            float breath = 1 + (UnityTuning.WordmarkBreath - 1) * k;
            if (Mathf.Abs(breath - menuWordmarkShown) < UnityTuning.WordmarkBreathStep) return;
            menuWordmarkShown = breath;
            wordmark.color = MenuBrighten(UiTheme.Cyan, breath);
            wordmarkBack.color = MenuBrighten(UiTheme.CyanDeep, breath);
        }

        static Color MenuBrighten(Color c, float k) => UiTheme.A(c * k, c.a);

        // --- the items' look: entrances along the slant, the focus lit, the rest dimmed ----------------------------------------

        /// <summary>A screen just showed: its slab and items enter along the slant (the items one after another).</summary>
        void MenuEnterScreens()
        {
            for (int i = 0; i < menuSlabs.Count; i++)
            {
                MenuSlabView s = menuSlabs[i];
                bool on = s.screen.activeInHierarchy;
                if (on == s.wasActive) continue;
                s.wasActive = on;
                if (!on) continue;
                // NIGHT18 round 3 (PAUSED / RESULTS "nearly pixel-identical" to NIGHT17): the slab sweeps in first along the
                // slant, then the parts (MenuScreenBegin18) and the items cascade after its lead; TITLE keeps its round-2 look
                s.sweep18 = FeelAB.On && s.screen != title;
                s.enter.In(s.sweep18 ? UnityTuning.MenuSlabIn18 : UnityTuning.ThemeInTime);
                int n = s.sweep18 ? MenuPartsShown18(s.screen) : 0;
                for (int k = 0; k < menuItems.Count; k++)
                {
                    MenuItemView it = menuItems[k];
                    if (it.go.transform.parent != s.screen.transform || !it.go.activeSelf) continue;
                    MenuEnterItem(it, n);
                    if (s.sweep18) it.wait = MenuMotion18.SlabWait(n);
                    n++;
                }
            }
        }

        void MenuEnterItem(MenuItemView it, int index)
        {
            it.wait = MenuMotion18.Wait(index, FeelAB.On); // NIGHT18: 15 ms apart, in by ~200 ms (NIGHT17: 30 ms apart)
            it.enter.In();
            it.entering = true;
            it.group.alpha = 0;
            it.lit = -1; // re-applied on its first step
            it.nudge18 = 0; // NIGHT18: a focused item slides along the slant as it arrives
        }

        /// <summary>Frame time: each slab's slide, each visible item's entrance and its focus ease (written only while moving).</summary>
        void MenuStepItems(double frameDt)
        {
            bool still = game.settings != null && game.settings.View.reducedMotion;
            for (int i = 0; i < menuSlabs.Count; i++)
            {
                MenuSlabView s = menuSlabs[i];
                if (!s.wasActive || !s.enter.Running) continue;
                float e = s.enter.Step(frameDt);
                float far = (1 - e) * UnityTuning.MenuSlabEnter18;
                s.rt.anchoredPosition = s.home + (s.sweep18 && !still ? new Vector2(-far, -far * MenuSlantK) : new Vector2(-(1 - e) * UnityTuning.MenuSlabEnter, 0));
            }
            bool feel = FeelAB.On;
            float focusStep = (float)(frameDt / UnityTuning.Pick(feel, UnityTuning.MenuFocusTime18, UnityTuning.MenuFocusTime));
            // round 2: with nothing selected (the attract's lock, a pointer elsewhere) the screen's primary item still shows lit
            MenuItemView litNow = menuLit;
            if (litNow == null && (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null))
            {
                GameObject primary = title.activeInHierarchy ? titlePlay : pause.activeInHierarchy ? navPrimaryPause : results.activeInHierarchy && !resultsDetailsOpen ? restartButton : null; // round 5: RETRY on RESULTS
                if (primary != null) menuItemOf.TryGetValue(primary, out litNow);
            }
            for (int i = 0; i < menuItems.Count; i++)
            {
                MenuItemView it = menuItems[i];
                if (!it.go.activeInHierarchy) continue;
                float target = it == litNow ? 1f : 0f;
                if (!it.entering && it.lit == target && (!feel || it.nudge18 == target)) continue; // at rest: nothing to write
                float e = 1f;
                if (it.entering)
                {
                    if (it.wait > 0)
                    {
                        it.wait -= frameDt;
                        e = 0;
                    }
                    else e = it.enter.Step(frameDt);
                    if (it.wait <= 0 && !it.enter.Running) it.entering = false;
                    if (still && it.wait <= 0) e = 1f; // REDUCED MOTION: in place, no slide
                    it.group.alpha = Mathf.Clamp01(e);
                }
                // round 5: one lit item at a time (MenuRules.FocusLit: the one the focus left goes dark at once)
                it.lit = still ? target : MenuRules.FocusLit(it.lit, target > 0, focusStep);
                float lit = UiTween.OutCubic(it.lit);
                float d = (1 - e) * UnityTuning.ThemeEnterDistance;
                Vector2 nudge;
                if (feel)
                {
                    // NIGHT18: the slide along the slant eases both ways (in: ease-out-back; out: ease-out from where it was);
                    // the colour keeps FocusLit's rule (the item the focus left goes dark at once: never two lit)
                    float was = it.nudge18;
                    it.nudge18 = still ? target : Mathf.MoveTowards(was, target, focusStep);
                    nudge = still ? MenuMotion18.FocusNudge(target, UnityTuning.MenuFocusNudge18) // REDUCED MOTION: placed, not slid
                        : target > 0 ? MenuMotion18.FocusNudge(it.nudge18, UnityTuning.MenuFocusNudge18)
                        : MenuMotion18.FocusNudge(1f, UnityTuning.MenuFocusNudge18) * (1f - UiTween.OutCubic(1f - it.nudge18));
                }
                else nudge = new Vector2(lit * UnityTuning.MenuFocusNudge, 0);
                it.rt.anchoredPosition = it.home + nudge + new Vector2(-d, -d * MenuSlantK);
                if (front33) { if (m43On) Menu43Paint(it, lit); else Front33Paint(it, lit); continue; } // NIGHT33 U (hud33): no plate, cyan label + two slash ticks (UiFront33.cs); NIGHT43 U1 screens43: the 1.08 grow, the raised plate, the cyan edge (UiMenus43.cs)
                it.plate.color = Color.Lerp(MenuPlate, UiTheme.Cyan, lit);
                it.bar.color = UiTheme.A(UiTheme.CyanHot, lit);
                it.label.color = Color.Lerp(UiTheme.White, UiTheme.InkSolid, lit);
            }
        }

        // --- NIGHT18 U3-31.7: every screen enters (FeelAB on; off: NIGHT17, where only the items and slabs moved) ------------
        // Build 1's reel: SETTINGS' sheet, slab, head, tabs, footer and BACK, PAUSED's sheet, head and run line were whole on
        // the input frame (M9: 12 + 3 pop-ins). Now a screen's sheet fades from the sheet shown before it (no flash of the world
        // between two screens) and its parts enter like its items (MenuMotion18: ease-out-back along the slant, fading in,
        // MenuEnterStagger apart); a screen without a slab of its own enters its items too (SETTINGS' BACK). TITLE keeps its
        // own whole-screen fade. A part another writer moves mid-entrance (a layout) takes that as its new rest. Visual only:
        // the controls take input from their first frame (Pillar 1). Per frame: one bool per screen; while entering, a few
        // floats per part. No allocation after the first frame (the screens are found once).

        MenuScreen18 MenuScreenOf18(GameObject screen)
        {
            for (int i = 0; i < menuScreens18.Count; i++) if (menuScreens18[i].screen == screen) return menuScreens18[i];
            // TITLE keeps its own whole-screen fade (UiFeelHud): its parts slide, its sheet is left alone
            var s = new MenuScreen18 { screen = screen, sheet = screen == title ? null : screen.GetComponent<UnityEngine.UI.Image>() };
            for (int i = 0; i < menuSlabs.Count; i++) s.ownSlab |= menuSlabs[i].screen == screen;
            menuScreens18.Add(s);
            return s;
        }

        /// <summary>Register `part` (a direct or deeper child of `screen`) to enter with it; a CanvasGroup carries its fade.</summary>
        void MenuPart18Add(GameObject screen, Component part)
        {
            if (part == null) return;
            var rt = (RectTransform)part.transform;
            var g = rt.GetComponent<CanvasGroup>();
            if (g == null) g = rt.gameObject.AddComponent<CanvasGroup>();
            MenuScreenOf18(screen).parts.Add(new MenuPart18 { rt = rt, group = g });
        }

        /// <summary>Frame time: each screen that just showed starts its entrance; the entering ones step (FeelAB on).</summary>
        void MenuScreens18Step(double frameDt)
        {
            if (!menuScreens18Found)
            {
                menuScreens18Found = true;
                menuTitleSheet18 = title.GetComponent<UnityEngine.UI.Image>();
                menuTitleGroup18 = title.GetComponent<CanvasGroup>();
                for (int i = 0; i < menus.childCount; i++)
                {
                    GameObject c = menus.GetChild(i).gameObject;
                    if (c == title || (navCursor != null && c == navCursor.gameObject) || c.GetComponent<UnityEngine.UI.Image>() == null) continue;
                    MenuScreenOf18(c);
                }
            }
            bool on = FeelAB.On;
            bool still = game.settings != null && game.settings.View.reducedMotion;
            float shown = 0;
            int topIndex = -1;
            for (int i = 0; i < menuScreens18.Count; i++)
            {
                MenuScreen18 s = menuScreens18[i];
                bool active = s.screen.activeInHierarchy;
                if (active && !s.wasActive && on) MenuScreenBegin18(s);
                else if (!active && s.entering) MenuScreenFinish18(s); // left mid-entrance: back to rest for its next showing
                s.wasActive = active;
                if (s.entering)
                {
                    if (on) MenuScreenTick18(s, frameDt, still);
                    else MenuScreenFinish18(s); // F6 mid-entrance: NIGHT17's look at once
                }
                if (active && s.sheet != null && s.screen.transform.GetSiblingIndex() > topIndex)
                {
                    topIndex = s.screen.transform.GetSiblingIndex();
                    shown = s.sheet.color.a;
                }
            }
            if (title.activeInHierarchy && title.transform.GetSiblingIndex() > topIndex)
                shown = menuTitleSheet18 != null ? menuTitleSheet18.color.a * (menuTitleGroup18 != null ? menuTitleGroup18.alpha : 1f) : 0f;
            menuSheetShown18 = shown;
        }

        bool menuScreens18Found;
        UnityEngine.UI.Image menuTitleSheet18; // TITLE keeps its own fade; its sheet is read for the next screen's start
        CanvasGroup menuTitleGroup18;

        void MenuScreenBegin18(MenuScreen18 s)
        {
            s.entering = true;
            s.age = 0;
            if (s.sheet != null)
            {
                s.sheetRest = s.sheet.color.a; // at rest here (an interrupted entrance restored it)
                s.sheetFrom = MenuMotion18.SheetFrom(menuSheetShown18, s.sheetRest);
                s.sheet.color = UiTheme.A(s.sheet.color, s.sheetFrom);
            }
            int n = 0;
            for (int i = 0; i < s.parts.Count; i++)
            {
                MenuPart18 p = s.parts[i];
                if (!p.rt.gameObject.activeInHierarchy) continue;
                p.home = p.written = p.rt.anchoredPosition;
                p.age = 0;
                // round 2: whole by ~200 ms; round 3: after the slab's sweep on PAUSED / RESULTS, MODS' cards one by one
                p.wait = s.partCascade > 0 ? n * s.partCascade : s.ownSlab && s.screen != title ? MenuMotion18.SlabWait(n) : MenuMotion18.Wait(n, true);
                n++;
                p.entering = true;
                p.group.alpha = 0;
            }
            if (s.ownSlab) return; // its items enter with its slab (MenuEnterScreens)
            for (int k = 0; k < menuItems.Count; k++)
            {
                MenuItemView it = menuItems[k];
                if (it.go.transform.parent == s.screen.transform && it.go.activeSelf) MenuEnterItem(it, n++);
            }
        }

        void MenuScreenTick18(MenuScreen18 s, double frameDt, bool still)
        {
            s.age += frameDt;
            if (s.sheet != null) s.sheet.color = UiTheme.A(s.sheet.color, MenuMotion18.SheetAlpha(s.age, s.sheetFrom, s.sheetRest));
            bool any = s.age < UnityTuning.MenuSheetFade18;
            for (int i = 0; i < s.parts.Count; i++)
            {
                MenuPart18 p = s.parts[i];
                if (!p.entering) continue;
                if (p.rt.anchoredPosition != p.written) p.home = p.rt.anchoredPosition; // a layout moved it: its new rest
                if (p.wait > 0)
                {
                    p.wait -= frameDt;
                    any = true;
                    if (p.wait > 0) continue;
                    p.age = -p.wait;
                    p.wait = 0;
                }
                else p.age += frameDt;
                if (p.age >= UnityTuning.ThemeInTime)
                {
                    p.entering = false;
                    p.group.alpha = 1;
                    p.rt.anchoredPosition = p.written = p.home;
                    continue;
                }
                any = true;
                p.group.alpha = MenuMotion18.Alpha(p.age);
                p.written = p.home + (still ? Vector2.zero : MenuMotion18.Offset(p.age, s.partDistance)); // REDUCED MOTION: a fade in place
                p.rt.anchoredPosition = p.written;
            }
            if (!any) MenuScreenFinish18(s);
        }

        void MenuScreenFinish18(MenuScreen18 s)
        {
            s.entering = false;
            if (s.sheet != null) s.sheet.color = UiTheme.A(s.sheet.color, s.sheetRest);
            for (int i = 0; i < s.parts.Count; i++)
            {
                MenuPart18 p = s.parts[i];
                if (!p.entering) continue;
                p.entering = false;
                p.group.alpha = 1;
                if (p.rt.anchoredPosition == p.written) p.rt.anchoredPosition = p.home;
            }
        }

        /// <summary>The focus moved (UiNav.FocusOn): a menu item lights (true: the item handles the look, no frame).</summary>
        bool MenuFocus(GameObject go)
        {
            menuLit = go != null && menuItemOf.TryGetValue(go, out MenuItemView it) ? it : null;
            return menuLit != null;
        }

        // --- TITLE / PAUSED columns ------------------------------------------------------------------------------------------

        readonly List<RectTransform> titleColumn = new List<RectTransform>(8); // PROGRESS, SETTINGS, QUIT (UiKiosk may hide QUIT)
        readonly List<RectTransform> pauseColumn = new List<RectTransform>(8);

        /// <summary>TITLE's rows under the card one pitch apart on the diagonal, a hidden one (QUIT in a kiosk) skipped.</summary>
        void LayoutTitleColumn()
        {
            float w = UnityTuning.MenuItemWidth;
            float cardBottom = UnityTuning.SetupCardTopY - (3 * UnityTuning.SetupCardRow + 2 * UnityTuning.SetupCardPad);
            float y = cardBottom - UnityTuning.SetupInfoGap - UnityTuning.TitleAfterCardGap;
            foreach (RectTransform rt in titleColumn)
            {
                if (!rt.gameObject.activeSelf) continue;
                var p = MenuAt(y, w);
                rt.anchoredPosition = p;
                if (menuItemOf.TryGetValue(rt.gameObject, out MenuItemView it)) it.home = p;
                y -= UnityTuning.MenuItemPitch;
            }
        }

        /// <summary>PAUSED's items one pitch apart on the diagonal, the hidden ones (COPY REPORT off a developer build) skipped.</summary>
        void LayoutPauseColumn()
        {
            float w = UnityTuning.MenuItemWidth, y = UnityTuning.PauseFirstRowY;
            foreach (RectTransform rt in pauseColumn)
            {
                if (!rt.gameObject.activeSelf) continue;
                var p = MenuAt(y, w);
                rt.anchoredPosition = p;
                if (menuItemOf.TryGetValue(rt.gameObject, out MenuItemView it)) it.home = p;
                y -= UnityTuning.MenuItemPitch;
            }
        }

        /// <summary>PAUSED opens (Ui.Render): the run header, once (round · points · time; a string then, never per frame).</summary>
        void RefreshPauseKeys()
        {
            if (menuPauseRunLine == null || game.run == null) return;
            var span = TimeSpan.FromSeconds(game.run.Time);
            menuPauseRunLine.text = string.Format(CultureInfo.InvariantCulture, "ROUND {0}  ·  {1:0}  ·  {2}:{3:00}",
                game.run.round, Math.Round(game.economy.total), (int)span.TotalMinutes, span.Seconds);
            PauseQuitDisarm();
        }

        /// <summary>QUIT's label (SAVE & QUIT at a break where a save is allowed) and its ask running out or losing the focus.</summary>
        void RenderPauseMenu()
        {
            bool save = game.SaveAllowed;
            if (menuQuitArmedShown)
            {
                bool focused = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == menuPauseQuit.go;
                if (!MenuRules.Armed(menuQuitAsk, FeelClock.Now, UnityTuning.PauseQuitConfirmWindow) || !focused) PauseQuitDisarm(); // NIGHT18: FeelClock
            }
            else if (save != menuPauseSaveShown)
            {
                menuPauseSaveShown = save;
                menuPauseQuit.label.text = save ? UnityTuning.PauseSaveQuitLabel : UnityTuning.PauseQuitLabel;
            }
        }

        void PauseQuitPressed()
        {
            bool save = game.SaveAllowed;
            switch (MenuRules.Quit(ref menuQuitAsk, FeelClock.Now, UnityTuning.PauseQuitConfirmWindow, save)) // NIGHT18: FeelClock
            {
                case MenuRules.QuitAct.Arm:
                    menuQuitArmedShown = true;
                    menuPauseQuit.label.text = UnityTuning.PauseQuitSure;
                    break;
                case MenuRules.QuitAct.SaveAndQuit:
                    PauseQuitDisarm();
                    game.SaveAndQuit();
                    break;
                default:
                    PauseQuitDisarm();
                    game.ToTitle();
                    break;
            }
        }

        void PauseQuitDisarm()
        {
            menuQuitAsk.armed = false;
            menuQuitArmedShown = false;
            if (menuPauseQuit == null) return;
            menuPauseSaveShown = game.SaveAllowed;
            menuPauseQuit.label.text = menuPauseSaveShown ? UnityTuning.PauseSaveQuitLabel : UnityTuning.PauseQuitLabel;
        }

        /// <summary>menus.js copyLine: the last run's score line to the system clipboard (a developer's button).</summary>
        void CopyScoreLine()
        {
            string line = game.lastStats?.line;
            if (string.IsNullOrEmpty(line)) return;
            GUIUtility.systemCopyBuffer = line;
            ShowHint("SCORE LINE COPIED");
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    /// <summary>
    /// NIGHT17 U3-30.8 (builder N): the menus' pure rules (MenusTests): SETTINGS' tabs and rows, PAUSED's items and QUIT's
    /// confirm, the words' count, RESULTS' beats, the onboarding prompts' timing, the run setup's cycling.
    /// </summary>
    public static class MenuRules
    {
        static Dictionary<string, int> rowIndex;

        /// <summary>A key's row in UnityTuning.MenuSettingsRows (-1: none), the index built once.</summary>
        public static int RowOf(string key)
        {
            if (key == null) return -1;
            if (rowIndex == null)
            {
                var d = new Dictionary<string, int>();
                for (int i = 0; i < UnityTuning.MenuSettingsRows.Length; i++) d[UnityTuning.MenuSettingsRows[i].key] = i;
                rowIndex = d;
            }
            return rowIndex.TryGetValue(key, out int r) ? r : -1;
        }

        /// <summary>SETTINGS' tabs in order: GAME, CONTROLS, VIDEO, ACCESSIBILITY; GAMEPAD with a pad; the LAB in a developer build.</summary>
        public static List<string> Tabs(bool padConnected, bool devMode, bool hasLab)
        {
            var list = new List<string>(6);
            foreach (var (id, _) in UnityTuning.MenuSettingsTabs)
            {
                if (id == UnityTuning.PadTab17Id && !padConnected) continue;
                if (id == UnityTuning.LabTabId && !(devMode && hasLab)) continue;
                list.Add(id);
            }
            return list;
        }

        /// <summary>A setting's tab now (null: not shown): the table's, or the LAB for the store's LAB toggles.</summary>
        public static string TabOf(string key, string storeTab)
        {
            int r = RowOf(key);
            if (r >= 0) return UnityTuning.MenuSettingsRows[r].tab;
            return storeTab == UnityTuning.LabTabId ? UnityTuning.LabTabId : null;
        }

        public static bool Advanced(string key)
        {
            int r = RowOf(key);
            return r >= 0 && UnityTuning.MenuSettingsRows[r].advanced;
        }

        /// <summary>NIGHT18: a row behind ADVANCED with FeelAB on (`n18`: UnityTuning.SettingsFront18 up front,
        /// SettingsAdvanced18 behind it) or off (the table's flag, NIGHT17).</summary>
        public static bool Advanced(string key, bool n18)
        {
            if (n18)
            {
                if (Array.IndexOf(UnityTuning.SettingsFront18, key) >= 0) return false;
                if (Array.IndexOf(UnityTuning.SettingsAdvanced18, key) >= 0) return true;
            }
            return Advanced(key);
        }

        /// <summary>NIGHT18: a hint line led by an ASCII key ("[E] BUY GUN"): the key ("E") for a drawn chip and the words after
        /// it ("BUY GUN"); false (key null, rest = the line) when it has none.</summary>
        public static bool KeyChipSplit(string line, out string key, out string rest)
        {
            key = null;
            rest = line;
            if (string.IsNullOrEmpty(line) || line[0] != '[') return false;
            int close = line.IndexOf(']');
            if (close <= 1) return false;
            key = line.Substring(1, close - 1);
            rest = line.Substring(close + 1).Trim();
            return true;
        }

        public static string LabelOf(string key, string fallback)
        {
            int r = RowOf(key);
            return r >= 0 && UnityTuning.MenuSettingsRows[r].label != null ? UnityTuning.MenuSettingsRows[r].label : fallback;
        }

        /// <summary>The focused row's footer (at most 8 words), or "" (never the web's long description).</summary>
        public static string FootOf(string key)
        {
            int r = RowOf(key);
            return r >= 0 ? UnityTuning.MenuSettingsRows[r].foot ?? "" : "";
        }

        static readonly string[] PausePlayer = { "RESUME", "RESTART", "SETTINGS", "QUIT" };
        static readonly string[] PauseDev = { "RESUME", "RESTART", "SETTINGS", "QUIT", "COPY REPORT" };

        /// <summary>PAUSED's items (the research's four; COPY REPORT only in a developer build).</summary>
        public static string[] PauseItems(bool devMode) => devMode ? PauseDev : PausePlayer;

        public enum QuitAct { Arm, SaveAndQuit, ToTitle }

        public struct QuitConfirm
        {
            public bool armed;
            public double at;
        }

        /// <summary>QUIT pressed at `now`: the first press asks (Arm); a second inside `window` quits, saving at a break.</summary>
        public static QuitAct Quit(ref QuitConfirm c, double now, double window, bool saveAllowed)
        {
            if (Armed(c, now, window))
            {
                c.armed = false;
                return saveAllowed ? QuitAct.SaveAndQuit : QuitAct.ToTitle;
            }
            c.armed = true;
            c.at = now;
            return QuitAct.Arm;
        }

        public static bool Armed(QuitConfirm c, double now, double window) => c.armed && now - c.at < window;

        /// <summary>The words in a line: runs of letters or digits (a lone symbol like "·" or "&amp;" is no word).</summary>
        public static int Words(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            int n = 0;
            bool inWord = false;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                bool space = char.IsWhiteSpace(ch);
                if (space)
                {
                    inWord = false;
                    continue;
                }
                if (!inWord && char.IsLetterOrDigit(ch) || !inWord && ch == '{')
                {
                    n++;
                    inWord = true;
                }
            }
            return n;
        }

        /// <summary>RESULTS' hero lines: line i arrives at i x ResultsLineGap s.</summary>
        public static double LineAt(int line) => line * UnityTuning.ResultsLineGap;

        /// <summary>A prompt is due: useful for PromptUsefulFor s, and never used or unused for PromptReshowAfter s.</summary>
        public static bool PromptDue(bool useful, double usefulFor, bool everUsed, double sinceUse) =>
            useful && usefulFor >= UnityTuning.PromptUsefulFor && (!everUsed || sinceUse >= UnityTuning.PromptReshowAfter);

        /// <summary>The first `max` due prompts in their order into `picked`; how many.</summary>
        public static int PickPrompts(bool[] due, int max, int[] picked)
        {
            int n = 0;
            for (int i = 0; i < due.Length && n < max && n < picked.Length; i++)
                if (due[i]) picked[n++] = i;
            return n;
        }

        /// <summary>The HEAT row's next rung (0 .. unlocked, wrapping).</summary>
        public static int CycleHeat(int heat, int unlocked, int dir)
        {
            int n = Math.Max(0, unlocked) + 1;
            return ((heat + dir) % n + n) % n;
        }

        /// <summary>TITLE's rows: PLAY (RESUME beside it), the run setup card, PROGRESS, SETTINGS, QUIT.</summary>
        public static int TitleRows(bool hasSave) => 5;

        /// <summary>NIGHT17 round 1: a column's left edge at height y (the one diagonal, inside the safe inset at 16:10).</summary>
        public static float Left(float y) => UnityTuning.MenuColumnLeft + SlantK * y;

        static readonly float SlantK = (float)Math.Tan(UnityTuning.ThemeSlantDeg * Math.PI / 180.0);

        /// <summary>NIGHT17 round 1: a hint on the HUD at most 3 words: tip `id`'s short line, a known long notice's, the text
        /// itself when short enough, else null (the sentence toast is dropped).</summary>
        public static string HudHint(string id, string text)
        {
            if (id != null && UnityTuning.TipLines17.TryGetValue(id, out string tip)) return tip;
            if (text != null && UnityTuning.HintLines17.TryGetValue(text, out string hint)) return hint;
            return Words(text) <= 3 && !string.IsNullOrEmpty(text) ? text : null;
        }

        /// <summary>NIGHT17 U3-30.10: the RESULTS card's left edge at its centre line (the column's at the tally row, padded).</summary>
        public static float ResultsCardLeft() => Left(UnityTuning.ResultsHeroY) - UnityTuning.ResultsCardPad;

        /// <summary>NIGHT17 U3-30.10: the card's best moment: "BEST +480 · TRIPLE", else the top speed, else "".</summary>
        public static string BestMoment(string award, double points, double topSpeed)
        {
            if (!string.IsNullOrEmpty(award))
            {
                string burst = "BEST BURST +" + Math.Round(points).ToString(CultureInfo.InvariantCulture); // round 2: "BEST +33 · KILL" read as cryptic
                return award == "KILL" ? burst : burst + " · " + award;
            }
            if (topSpeed > 0) return "TOP SPEED " + Math.Round(topSpeed).ToString(CultureInfo.InvariantCulture) + " M/S";
            return "";
        }

        /// <summary>NIGHT17 U3-30.10: the card's small stamp: the date (5 OCT 2026) and the seed code.</summary>
        public static string CardStamp(DateTime day, string seed)
        {
            string d = day.ToString("d MMM yyyy", CultureInfo.InvariantCulture).ToUpperInvariant();
            return string.IsNullOrEmpty(seed) ? d : d + "  ·  SEED " + seed;
        }

        /// <summary>Round 2: a prompt only in a quiet moment: no enemy within PromptQuietRange m, PromptActionQuiet s after a shot, a
        /// swap or a hit, not while sprinting or sliding.</summary>
        public static bool PromptQuiet(double nearestEnemy, double sinceAction, bool sprinting, bool sliding) =>
            nearestEnemy >= UnityTuning.PromptQuietRange && sinceAction >= UnityTuning.PromptActionQuiet && !sprinting && !sliding;

        /// <summary>NIGHT17 round 1: PromptDue, and a `once` prompt (the WHEEL) never comes back after its first use.</summary>
        public static bool PromptDue(bool useful, double usefulFor, bool everUsed, double sinceUse, bool once, int shows) =>
            shows < UnityTuning.PromptMaxShows && !(once && everUsed) && PromptDue(useful, usefulFor, everUsed, sinceUse);

        /// <summary>Round 2: a range's value in one format: "1.00" for a fractional range, "90" for a whole one (x `scale` first).</summary>
        public static string RangeShown(double value, double step, double scale)
        {
            double v = value * scale, s = step * scale;
            bool whole = s >= 1 - 1e-9;
            return v.ToString(whole ? "0" : "0.00", CultureInfo.InvariantCulture);
        }

        /// <summary>Round 4: an item's lit value next frame: the one the focus left goes dark at once, the focused one lights by
        /// `step` (a fresh item, lit -1, takes its state at once): never two lit.</summary>
        public static float FocusLit(float lit, bool focused, float step)
        {
            if (!focused) return 0f;
            if (lit < 0) return 1f;
            return Math.Min(1f, lit + step);
        }

        /// <summary>Round 4: a mod card's line under its hero: ours (UnityTuning.ModLines17: the number's noun) or the cut sentence.</summary>
        public static string ModLine17(string id, string rest) =>
            id != null && UnityTuning.ModLines17.TryGetValue(id, out string line) ? line : rest;

        /// <summary>Round 4: the mod pays points (its glyph and hero may be gold: gold is points only).</summary>
        public static bool ModPaysPoints(string id) => Array.IndexOf(UnityTuning.ModsPayingPoints, id) >= 0;

        /// <summary>Round 3: a mod card's hero, its number ("+25%", "+100 POINTS", "5 HP"), out the line without it.</summary>
        public static string ModHero(string line, out string rest)
        {
            rest = line ?? "";
            if (string.IsNullOrEmpty(line)) return "";
            string[] t = line.Split(' ');
            int at = -1;
            for (int i = 0; i < t.Length && at < 0; i++) if (t[i].IndexOf('%') >= 0) at = i;
            for (int i = 0; i < t.Length && at < 0; i++) if (t[i].Length > 1 && t[i][0] == '+' && char.IsDigit(t[i][1])) at = i;
            for (int i = 0; i < t.Length && at < 0; i++) if (HasDigit(t[i])) at = i;
            if (at < 0) return "";
            string hero = t[at].Trim('(', ')');
            int used = 1;
            if (hero.IndexOf('%') < 0 && at + 1 < t.Length && Array.IndexOf(ModUnits, t[at + 1].Trim('(', ')')) >= 0)
            {
                hero += " " + t[at + 1].Trim('(', ')');
                used = 2;
            }
            var b = new System.Text.StringBuilder(line.Length);
            for (int i = 0; i < t.Length; i++)
            {
                if (i >= at && i < at + used) continue;
                string w = t[i].Replace("(", "").Replace(")", "");
                if (w.Length == 0) continue;
                if (b.Length > 0) b.Append(' ');
                b.Append(w);
            }
            rest = b.ToString();
            return hero;
        }

        static readonly string[] ModUnits = { "POINTS", "HP", "M", "S" };

        static bool HasDigit(string s)
        {
            for (int i = 0; i < s.Length; i++) if (char.IsDigit(s[i])) return true;
            return false;
        }

        /// <summary>Round 2: TITLE's best row: the rungs (0 = no HEAT) that have a grade, in order (none: no row).</summary>
        public static List<int> BestRungs(string[] grades)
        {
            var list = new List<int>(grades.Length);
            for (int i = 0; i < grades.Length; i++) if (!string.IsNullOrEmpty(grades[i])) list.Add(i);
            return list;
        }
    }
}
