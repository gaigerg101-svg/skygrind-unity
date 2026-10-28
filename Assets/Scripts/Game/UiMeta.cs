// NIGHT12 U3-25.7 .. U3-25.11 (agent F): the meta's UI hooks (BuildMeta / RenderMeta, the shared types' partial methods),
// the run setup on TITLE (SURVIVAL | THE GRIND, the HEAT rungs: game.setup, which StartRun reads), the toast stack (contracts,
// unlocks, MASTERY tiers, HEAT rungs) and the helpers the meta screens share (a scrolling list, a bar).
// Display only: nothing here changes the sim. A choice writes game.setup, or the profile through a Core API followed by
// game.SaveProfile(). Every meta system (game.contracts, mastery, heat, tips, onboarding) may be null until the
// lead wires it: every use is guarded. Texts are built when a value changes, a screen opens or an event arrives, never
// per frame; the per-frame part (toasts' fades, the hint, the lesson card) allocates nothing.
// Layout numbers: UnityTuning.Meta.cs. Colours: cyan (usable) = player-usable and unlock highlights, gold = points and
// grades, muted = locks; red-orange never appears here (no danger on these screens).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        enum MetaScreen { None, Contracts, Weapons, Levels } // NIGHT26: Levels = the CAMPAIGN's level select (UiLevelSelect26.cs)

        MetaScreen metaOpen;
        RectTransform metaLayer; // the toasts, the hint and the lesson card: over every screen, no raycaster
        bool shownTitleOn;
        Flow metaFlow = (Flow)(-1);
        Color faint, barBack, rowBack;
        string usableHex;
        bool payoffOn;
        readonly StringBuilder metaSb = new StringBuilder(256);

        /// <summary>A meta screen (CONTRACTS, WEAPONS) is open: Esc should close it (SurvivalGame.HotKeys calls CloseMeta).</summary>
        public bool MetaOpen => metaOpen != MetaScreen.None;

        /// <summary>Close the open meta screen and show TITLE again (BACK, Esc).</summary>
        public void CloseMeta()
        {
            if (metaOpen == MetaScreen.None) return;
            metaOpen = MetaScreen.None;
            contractsScreen.SetActive(false);
            weaponsScreen.SetActive(false);
            if (levelsScreen26 != null) levelsScreen26.SetActive(false);
            title.SetActive(game.flow == Flow.Title && !settingsOpen);
        }

        void OpenMeta(MetaScreen which)
        {
            if (game.flow != Flow.Title || settingsOpen) return;
            metaOpen = which;
            title.SetActive(false);
            contractsScreen.SetActive(which == MetaScreen.Contracts);
            weaponsScreen.SetActive(which == MetaScreen.Weapons);
            if (levelsScreen26 != null) levelsScreen26.SetActive(which == MetaScreen.Levels);
        }

        partial void BuildMeta()
        {
            faint = UiTheme.InkSoft; // NIGHT17 U3-30.10: a quiet row's accent bar is solid (no low-alpha white)
            barBack = UiTheme.InkSoft; // NIGHT17: solid, no translucent white
            rowBack = UiTheme.InkSoft; // NIGHT17: solid, no translucent white
            usableHex = ColorUtility.ToHtmlStringRGB(usable);
            payoffOn = cfg.Design("payoff.enabled");
            hintTime = cfg.Num("ui.hintTime");
            BuildRunSetup();
            // NIGHT15 U3-28.5 the MAP group: since NIGHT17 the run setup card's MAP row (BuildRunSetup)
            BuildBestGrades();
            MenuPart18Add(title, setupRow); // NIGHT18: the run setup card and the best row enter after the wordmark
            MenuPart18Add(title, bestRow);
            BuildContracts();
            BuildWeapons();
            BuildLevelSelect26(); // NIGHT26 (builder L): the CAMPAIGN's level select (built hidden; PLAY on CAMPAIGN opens it)
            BuildPayoff();
            metaLayer = Layer("Meta overlay", false); // after the menus layer: drawn over TITLE, DOWN and RESULTS
            BuildOnboarding();
            BuildLesson25(); // NIGHT25 (builder F): the tutorial stage's one line
            BuildWeaponSwitch25(); // NIGHT25 (builder F): the weaponSwitch cue
            BuildToasts();
        }

        partial void RenderMeta(double frameDt)
        {
            Flow f = game.flow;
            if (metaOpen != MetaScreen.None)
            {
                if (f != Flow.Title) CloseMeta(); // a run started under it
                else if (title.activeSelf) title.SetActive(false); // the flow block re-picked the screens
            }
            bool titleOn = title.activeSelf;
            if (titleOn && !shownTitleOn) RefreshTitleMeta(); // the title shows (boot, BACK, SETTINGS closed, a run left)
            shownTitleOn = titleOn;
            if (f != metaFlow)
            {
                metaFlow = f;
                if (f == Flow.Down || f == Flow.Results) RefreshPayoff(f);
                if (f == Flow.Down || f == Flow.Results || f == Flow.Title) EndLesson(); // the lesson's time is over
                if (f == Flow.Title || f == Flow.Down || f == Flow.Results) ClearHints(); // NIGHT17 round 1: no fight tip on DOWN
            }
            else if (f == Flow.Results && game.lastStats != payoffStats) RefreshPayoff(f);
            RenderOnboarding(frameDt);
            RenderLesson25(frameDt); // NIGHT25 (builder F)
            RenderWeaponSwitch25(frameDt);
            RenderLevelSelect26(frameDt); // NIGHT26 (builder L)
            if (titleOn && setupRow.gameObject.activeSelf) SetupInfoTick(frameDt); // round 3: a lock line only with its row focused
            RenderToasts(frameDt);
        }

        /// <summary>TITLE shows: the run setup card under PLAY, the best grades under the column, PLAY focused.</summary>
        void RefreshTitleMeta()
        {
            RefreshSetup(); // first: it counts the card's rows
            float top = UnityTuning.SetupCardTopY;
            float h = setupCardRows * UnityTuning.SetupCardRow + 2 * UnityTuning.SetupCardPad;
            float cy = top - h * 0.5f;
            setupRow.anchoredPosition = new Vector2(MenuLeft(cy) + UnityTuning.SetupCardWidth * 0.5f, cy); // its left edge on the column
            // NIGHT24: under the LOWEST visible item of the column (the CODE item joined after QUIT in NIGHT22, and the best row, placed under QUIT,
            // sat on it: the n24-3 TITLE frame). A hidden item (QUIT in a kiosk) is skipped. Every item stays in the column, so pad nav is unchanged.
            RectTransform quit = null;
            foreach (RectTransform rt in titleColumn)
                if (rt.gameObject.activeSelf && (quit == null || rt.anchoredPosition.y < quit.anchoredPosition.y)) quit = rt;
            if (quit != null)
            {
                float by = quit.anchoredPosition.y - quit.sizeDelta.y * 0.5f - UnityTuning.BestRowGap - UnityTuning.BestRowHeight * 0.5f;
                bestRow.alignment = TextAlignmentOptions.Left;
                MenuPinLeft(bestRow, MenuLeft(by) + 6, by); // NIGHT17 round 1: on the column (it was centred off the left edge: "EST D")
                // NIGHT33 U (hud33; the lead's n33-r5 title frame: 'Best grades' inside the 54 px bottom margin): beside the lowest item instead of under it
                if (front33) MenuPinLeft(bestRow, quit.anchoredPosition.x + quit.sizeDelta.x * 0.5f + UnityTuning.Front33BestGap, quit.anchoredPosition.y);
            }
            RefreshBestGrades();
            // NIGHT13 U3-26.11: PLAY focused first (one press to play): Enter, Space or the pad's submit starts a run.
            if (titlePlay != null && UnityEngine.EventSystems.EventSystem.current != null)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(titlePlay);
        }

        // --- the run setup card (NIGHT17 U3-30.8: one card, three rows, a press cycles; was an 8-button strip) ----------------
        // MODE (SURVIVAL / THE GRIND), HEAT (0 .. the rungs open), MAP (design.maps.list: a
        // pick reloads the scene on that map). Each row is one NavRow: left / right cycle it, a press or a click cycles it
        // forward. One info line under the card says what the pick means (at most 8 words). A row with nothing to choose
        // hides; no row at all: no card.

        sealed class SetupCardRow
        {
            public GameObject go;
            public TextMeshProUGUI label, value;
        }

        RectTransform setupRow; // the card
        UiShape setupCardPlate;
        SetupCardRow setupMode, setupHeat, setupMap;
        TextMeshProUGUI setupInfo;
        int setupCardRows = 3;
        bool setupInfoLock; // round 3: the info line is a lock's (shown only while its row is focused)

        /// <summary>Round 3 (the reviewer: "HEAT 1: EXFIL ONCE" always on): a lock's line shows only while HEAT or MODE is
        /// focused; a rungs score stays. A compare a frame; SetActive on a change only.</summary>
        void SetupInfoTick(double frameDt = 0)
        {
            GameObject sel = UnityEngine.EventSystems.EventSystem.current != null ? UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject : null;
            bool on = !setupInfoLock || sel == setupHeat.go || sel == setupMode.go;
            if (setupInfo.gameObject.activeSelf != on)
            {
                setupInfo.gameObject.SetActive(on);
                if (on && FeelAB.On) // NIGHT18: round 2's M9: the lock line was whole on its first frame (ui-title f40, ui-settings f11)
                {
                    setupInfoAge18 = 0;
                    setupInfoHome18 = setupInfo.rectTransform.anchoredPosition;
                    setupInfoEntering18 = true;
                }
            }
            if (!setupInfoEntering18) return;
            // NIGHT18: it fades in along the slant (MenuMotion18; REDUCED MOTION: the fade alone); its padlock is its child
            setupInfoAge18 += frameDt;
            RectTransform irt = setupInfo.rectTransform;
            if (irt.anchoredPosition != setupInfoWritten18 && setupInfoAge18 > frameDt) setupInfoHome18 = irt.anchoredPosition; // a layout moved it
            bool done = !FeelAB.On || !setupInfo.gameObject.activeSelf || setupInfoAge18 >= UnityTuning.ThemeInTime;
            setupInfoGroup18.alpha = done ? 1f : MenuMotion18.Alpha(setupInfoAge18);
            irt.anchoredPosition = setupInfoWritten18 = setupInfoHome18 + (done || game.settings.View.reducedMotion ? Vector2.zero : MenuMotion18.Offset(setupInfoAge18));
            if (done) setupInfoEntering18 = false;
        }

        double setupInfoAge18;
        Vector2 setupInfoHome18, setupInfoWritten18;
        bool setupInfoEntering18;
        CanvasGroup setupInfoGroup18;

        void BuildRunSetup()
        {
            setupRow = new GameObject("Run setup", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape)).GetComponent<RectTransform>();
            setupRow.SetParent(title.transform, false);
            setupRow.anchorMin = setupRow.anchorMax = new Vector2(0.5f, 0.5f);
            setupRow.sizeDelta = new Vector2(UnityTuning.SetupCardWidth, 3 * UnityTuning.SetupCardRow + 2 * UnityTuning.SetupCardPad);
            setupCardPlate = setupRow.GetComponent<UiShape>();
            setupCardPlate.color = UiTheme.A(UiTheme.InkSoft, 1f); // round 2: opaque (the world's bright trim never shows through)
            setupCardPlate.raycastTarget = false;
            setupMode = SetupRow(UnityTuning.SetupModeLabel, CycleMode20); // SURVIVAL / THE GRIND (NIGHT24: the DAILY and the WEEKLY are gone)
            setupHeat = SetupRow(UnityTuning.SetupHeatLabel, d =>
            {
                if (game.heat != null) PickHeat(MenuRules.CycleHeat(game.setup.heat, Math.Max(0, game.heat.Unlocked), d));
            });
            setupMap = SetupRow(UnityTuning.SetupMapLabel, CycleMap);
            setupInfo = Text(setupRow, "Info", "", UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, TextAlignmentOptions.Left, Vector2.zero, new Vector2(UnityTuning.SetupCardWidth + 200, UnityTuning.LineSmall), new Vector2(0.5f, 0.5f));
            UiTheme.Style(setupInfo, UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, UiTheme.BodyBold);
            setupInfo.overflowMode = TextOverflowModes.Overflow;
            setupInfo.richText = true;
            setupInfoGroup18 = setupInfo.gameObject.AddComponent<CanvasGroup>(); // NIGHT18: its fade in (with its padlocks)
            setupRow.gameObject.SetActive(false);
        }

        SetupCardRow SetupRow(string label, Action<int> cycle)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
            var rt = (RectTransform)go.transform;
            rt.SetParent(setupRow, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(UnityTuning.SetupCardWidth - 2 * UnityTuning.SetupCardPad, UnityTuning.SetupCardRow - 4);
            var hit = go.GetComponent<UiShape>();
            hit.color = UiTheme.A(UiTheme.Cyan, 0f); // the click target (the nav frame shows the focus)
            hit.raycastTarget = true;
            float w = rt.sizeDelta.x;
            var row = new SetupCardRow { go = go };
            row.label = Text(rt, "Label", label, UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, TextAlignmentOptions.Left, new Vector2(-w * 0.5f + 16 + UnityTuning.SetupCardLabelWidth * 0.5f, 0), new Vector2(UnityTuning.SetupCardLabelWidth, UnityTuning.SetupCardRow), new Vector2(0.5f, 0.5f));
            UiTheme.Style(row.label, UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, UiTheme.BodyBold);
            row.label.characterSpacing = UnityTuning.ThemeLabelTracking;
            // round 2: the value centred between two drawn chevrons (the settings' selector), not ASCII "<  >"
            float vw = UnityTuning.SettingsSelectWidth, vx = w * 0.5f - 16 - UnityTuning.ChevronWidth - 10 - vw * 0.5f;
            row.value = Text(rt, "Value", "", UiTheme.Px(UiTheme.Size.S24), UiTheme.White, TextAlignmentOptions.Center, new Vector2(vx, 0), new Vector2(vw, UnityTuning.SetupCardRow), new Vector2(0.5f, 0.5f));
            SettingsChevron(rt, false, new Vector2(vx - vw * 0.5f - 10, 0));
            SettingsChevron(rt, true, new Vector2(vx + vw * 0.5f + 10, 0));
            UiTheme.Style(row.value, UiTheme.Px(UiTheme.Size.S24), UiTheme.White, UiTheme.BodyBold);
            row.value.richText = true;
            var nav = go.AddComponent<NavRow>();
            nav.side = d => cycle(d);
            nav.submit = () => cycle(1);
            return row;
        }

        /// <summary>NIGHT19 U3-33.3: LB / RB on TITLE: the run setup's MODE tabs (SURVIVAL / THE GRIND, as its row cycles), the value sliding in from the bumper's side and the info line entering with it.</summary>
        void SetupTab19(int dir)
        {
            if ((!GrindOn && !CampaignOn) || setupMode == null || !setupMode.go.activeInHierarchy) return;
            string was = game.setup.mode;
            CycleMode20(dir);
            if (game.setup.mode == was) return;
            TabSlideStart19(setupMode.value.rectTransform, dir);
            if (FeelAB.On && setupInfo.gameObject.activeSelf)
            {
                setupInfoAge18 = 0; // the info line (what the pick means) enters again with it
                setupInfoHome18 = setupInfo.rectTransform.anchoredPosition;
                setupInfoEntering18 = true;
            }
        }

        bool HeatShown => game.heat != null && game.heat.enabled && game.heat.Rungs > 0;

        void PickMode(string mode)
        {
            if (mode == RunSetup.Campaign) game.GoCampaign(); // NIGHT24: may reload the scene on the level
            else if (game.OnCampaignMap) game.LeaveCampaign(mode); // and back to the survival map
            else game.setup.mode = mode;
            RefreshSetup();
        }

        // NIGHT24 (builder L): the CAMPAIGN on the MODE row (design.n24.campaign with the n24 bit, and a level in design.maps.list).
        bool CampaignOn => !N24Blocks.ForceOff && N24Blocks.From(cfg).campaign && game.HasCampaignMap;

        // NIGHT21 U3-35.7 (builder W, UNCOMPILED): THE GRIND on the MODE row (design.n21.grind with the n21 bit; the web's endless).
        // game.setup.mode = RunSetup.Grind; the sim runs it only with the block on (SurvivalSim.Grind21.cs).
        bool GrindOn => !N21Blocks.ForceOff && N21Blocks.From(cfg).grind;

        /// <summary>MODE's cycle: SURVIVAL, THE GRIND (when it is on), wrapping either way.</summary>
        void CycleMode20(int dir)
        {
            var modes = new List<string>();
            if (CampaignOn) modes.Add(RunSetup.Campaign); // NIGHT24: CAMPAIGN first, then SURVIVAL, THE GRIND
            modes.Add(RunSetup.Survival);
            if (GrindOn) modes.Add(RunSetup.Grind); // NIGHT21: THE GRIND
            int at = Math.Max(0, modes.IndexOf(game.setup.mode));
            PickMode(modes[((at + (dir < 0 ? -1 : 1)) % modes.Count + modes.Count) % modes.Count]);
        }

        void PickHeat(int rung)
        {
            if (game.heat == null || rung > game.heat.Unlocked) return; // a lock
            game.setup.heat = rung;
            RefreshSetup();
        }

        /// <summary>The MAP row: the next map in design.maps.list (a pick reloads the scene on it).</summary>
        void CycleMap(int dir)
        {
            var maps = game.PickableMaps; // NIGHT24: a CAMPAIGN level is not on this row
            if (maps.Count < 2) return;
            int at = 0;
            for (int i = 0; i < maps.Count; i++) if (maps[i].id == game.MapId) at = i;
            int next = ((at + dir) % maps.Count + maps.Count) % maps.Count;
            game.PickMap(maps[next].id);
        }

        static string SetupValue(string v) => v; // round 2: the chevrons are drawn (SetupRow)

        /// <summary>The run setup's state from the systems and game.setup (on TITLE open and on a pick).</summary>
        void RefreshSetup()
        {
            RunSetup setup = game.setup;
            bool heatAvail = HeatShown && setup.mode != RunSetup.Campaign; // NIGHT24: no HEAT rung in a CAMPAIGN
            if (!heatAvail) setup.heat = 0;
            else if (setup.heat > game.heat.Unlocked) setup.heat = Math.Max(0, game.heat.Unlocked);
            if (setup.mode == RunSetup.Grind && !GrindOn) setup.mode = RunSetup.Survival; // NIGHT21: the block went off (--off21): no GRIND
            if (setup.mode == RunSetup.Campaign && !CampaignOn) setup.mode = RunSetup.Survival; // NIGHT24: the block went off (--off24): no CAMPAIGN
            bool grind = setup.mode == RunSetup.Grind;
            bool campaign = setup.mode == RunSetup.Campaign;
            bool maps = game.PickableMaps.Count > 1 && !campaign; // NIGHT24: the level is the map in a CAMPAIGN
            bool modeShown = GrindOn || CampaignOn;
            setupMode.go.SetActive(modeShown);
            setupHeat.go.SetActive(heatAvail);
            setupMap.go.SetActive(maps);
            setupCardRows = (modeShown ? 1 : 0) + (heatAvail ? 1 : 0) + (maps ? 1 : 0);
            if (setupCardRows == 0)
            {
                setupRow.gameObject.SetActive(false); // nothing to choose
                return;
            }
            setupRow.gameObject.SetActive(true);
            float h = setupCardRows * UnityTuning.SetupCardRow + 2 * UnityTuning.SetupCardPad;
            setupRow.sizeDelta = new Vector2(UnityTuning.SetupCardWidth, h);
            float y = h * 0.5f - UnityTuning.SetupCardPad - UnityTuning.SetupCardRow * 0.5f;
            foreach (SetupCardRow r in new[] { setupMode, setupHeat, setupMap })
            {
                if (!r.go.activeSelf) continue;
                ((RectTransform)r.go.transform).anchoredPosition = new Vector2(0, y);
                y -= UnityTuning.SetupCardRow;
            }
            setupInfo.rectTransform.anchoredPosition = new Vector2(100, -h * 0.5f - UnityTuning.SetupInfoGap * 0.5f - 6);

            setupMode.value.text = SetupValue(campaign ? UnityTuning.CampaignLabel : grind ? UnityTuning.GrindLabel : "SURVIVAL"); // NIGHT21 THE GRIND; NIGHT24 the CAMPAIGN
            setupHeat.value.text = heatAvail ? SetupValue(setup.heat.ToString(CultureInfo.InvariantCulture)) : "";
            string mapLabel = game.MapId;
            foreach (SurvivalGame.MapEntry m in game.Maps) if (m.id == game.MapId) mapLabel = m.label;
            setupMap.value.text = SetupValue(mapLabel ?? "");

            // The info line (at most 8 words): THE GRIND's pitch, the rung's score, else the next lock.
            metaSb.Clear();
            bool infoLocks = false;
            if (campaign) // NIGHT24: the CAMPAIGN's pitch
            {
                metaSb.Append("<color=#").Append(UiTheme.CyanHex).Append('>').Append(UnityTuning.CampaignInfo).Append("</color>");
            }
            else if (grind) // NIGHT21: THE GRIND's pitch, then this rung's best wave
            {
                metaSb.Append("<color=#").Append(UiTheme.CyanHex).Append('>').Append(UnityTuning.GrindInfo).Append("</color>");
                int bestWave = game.sim.grindRecords.BestWave(setup.heat);
                if (bestWave > 0) metaSb.Append("  ·  ").Append(UnityTuning.GrindResultsBest).Append(bestWave);
            }
            else if (heatAvail && setup.heat > 0)
            {
                metaSb.Append("<color=#").Append(UiTheme.CyanHex).Append('>').Append(game.heat.Name(setup.heat)).Append("</color>");
                metaSb.Append("  ·  x").Append(game.heat.ScoreMult(setup.heat).ToString("0.##", CultureInfo.InvariantCulture)).Append(" SCORE");
            }
            else if (heatAvail && game.heat.Unlocked < game.heat.Rungs)
            {
                int next = Math.Max(0, game.heat.Unlocked) + 1;
                metaSb.Append(LockMark).Append(" HEAT ").Append(next).Append(": ").Append(next == 1 ? "EXFIL ONCE" : "EXFIL ON HEAT " + (next - 1));
                infoLocks = true;
            }
            setupInfo.SetText(metaSb);
            setupInfoLock = infoLocks;
            SetupInfoTick();
            PlaceLocks(setupInfo, infoLocks);
        }


        // --- shared helpers -----------------------------------------------------------------------------------------------

        /// <summary>A number as the web prints it on a badge (whole numbers bare, else one decimal).</summary>
        static string MetaNum(double v) => v.ToString("0.#", CultureInfo.InvariantCulture);

        /// <summary>"value / target", the value capped at the target (menus.js renderChallenges).</summary>
        static string Progress(double value, double target) => MetaNum(Math.Min(value, target)) + " / " + MetaNum(target);

        /// <summary>A bar: its back (menus.css .badge .pb) and a cyan fill (anchorMax.x = the fraction, as SetFill does).</summary>
        UnityEngine.UI.Image Bar(Transform parent, Vector2 anchor, Vector2 pos, float width, out UnityEngine.UI.Image back)
        {
            back = Img(parent, "Bar", barBack, anchor, anchor, pos, new Vector2(width, UnityTuning.BarHeight));
            back.rectTransform.pivot = anchor;
            back.rectTransform.anchoredPosition = pos;
            var fill = Img(back.transform, "Fill", usable, Vector2.zero, new Vector2(0, 1), Vector2.zero, Vector2.zero);
            fill.rectTransform.pivot = new Vector2(0, 0.5f);
            return fill;
        }

        static void SetBar(UnityEngine.UI.Image fill, double fraction)
        {
            float v = (float)Math.Max(0, Math.Min(1, double.IsNaN(fraction) ? 0 : fraction));
            fill.rectTransform.anchorMax = new Vector2(v, 1);
        }

        // --- the lock mark (NIGHT13 U3-26.11 S1) ----------------------------------------------------------------------
        // The lock was U+25A1, which the theme fonts lack and which is also TMP's missing-glyph mark: a lock and a broken
        // glyph looked the same. Now a text holds LockMark where a lock goes: a transparent U+00A4 (Latin-1, in every static
        // atlas, never used in a UI string) that keeps the room; PlaceLocks then covers each one with a padlock Image (a
        // sprite drawn once in code) in the text's colour. Run when a text that may hold a lock is set, never per frame.

        /// <summary>Put this where a lock goes in a text, then call PlaceLocks on that text.</summary>
        const string LockMark = "<color=#00000000>¤</color>";
        const char LockChar = '¤';
        static Sprite padlock;
        readonly Dictionary<TMP_Text, List<UnityEngine.UI.Image>> lockPools = new Dictionary<TMP_Text, List<UnityEngine.UI.Image>>();

        /// <summary>
        /// Cover every LockMark in `t` with a padlock (pooled under the text), hide the rest of its pool. `hasMarks` false
        /// only hides them (no layout pass). Call after setting the text.
        /// </summary>
        void PlaceLocks(TMP_Text t, bool hasMarks)
        {
            lockPools.TryGetValue(t, out List<UnityEngine.UI.Image> pool);
            int used = 0;
            if (hasMarks)
            {
                t.ForceMeshUpdate(true);
                TMP_TextInfo info = t.textInfo;
                for (int k = 0; k < info.characterCount; k++)
                {
                    TMP_CharacterInfo ci = info.characterInfo[k];
                    if (ci.character != LockChar || !ci.isVisible) continue;
                    if (pool == null) lockPools[t] = pool = new List<UnityEngine.UI.Image>(2);
                    if (used == pool.Count)
                    {
                        if (padlock == null) padlock = PadlockSprite();
                        var img = Img(t.transform, "Lock", Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                        img.sprite = padlock;
                        pool.Add(img);
                    }
                    UnityEngine.UI.Image lk = pool[used++];
                    float h = ci.pointSize * UnityTuning.LockMarkEm;
                    lk.rectTransform.sizeDelta = new Vector2(h * UnityTuning.PadlockWidth / UnityTuning.PadlockHeight, h);
                    lk.rectTransform.localPosition = new Vector3((ci.origin + ci.xAdvance) * 0.5f, ci.baseLine + ci.pointSize * UnityTuning.LockMarkRiseEm, 0);
                    lk.color = t.color;
                    if (!lk.gameObject.activeSelf) lk.gameObject.SetActive(true);
                }
            }
            if (pool == null) return;
            for (int i = used; i < pool.Count; i++)
                if (pool[i].gameObject.activeSelf) pool[i].gameObject.SetActive(false);
        }

        /// <summary>A padlock (white, tinted by the Image): a rounded body with a keyhole under a round shackle, antialiased.</summary>
        static Sprite PadlockSprite()
        {
            int w = UnityTuning.PadlockWidth, h = UnityTuning.PadlockHeight;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Padlock" };
            var px = new Color32[w * h];
            // In texture pixels, y up (for 48 x 56: the body y 2..32, x 4..44, corner 5; the shackle's arc round (24, 39), radii
            // 9..15, so its top is at 54 and the drawing is centred); its legs run from the body's centre up to the arc.
            float bodyCx = w * 0.5f, bodyCy = h * (17f / 56f), bodyHx = w * (20f / 48f), bodyHy = h * (15f / 56f), corner = w * (5f / 48f);
            float ringCy = h * (39f / 56f), ringOut = w * (15f / 48f), ringIn = w * (9f / 48f);
            float legHx = (ringOut - ringIn) * 0.5f, legX = (ringOut + ringIn) * 0.5f;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float fx = x + 0.5f, fy = y + 0.5f;
                    float body = RoundBox(fx - bodyCx, fy - bodyCy, bodyHx, bodyHy, corner);
                    float shackle;
                    if (fy >= ringCy) shackle = Mathf.Abs(Mathf.Sqrt((fx - bodyCx) * (fx - bodyCx) + (fy - ringCy) * (fy - ringCy)) - legX) - legHx;
                    else
                    {
                        float legHy = (ringCy - bodyCy) * 0.5f, legCy = (ringCy + bodyCy) * 0.5f;
                        shackle = Mathf.Min(RoundBox(fx - (bodyCx - legX), fy - legCy, legHx, legHy, 0), RoundBox(fx - (bodyCx + legX), fy - legCy, legHx, legHy, 0));
                    }
                    float keyhole = Mathf.Min(Mathf.Sqrt((fx - bodyCx) * (fx - bodyCx) + (fy - bodyCy - h * 0.04f) * (fy - bodyCy - h * 0.04f)) - w * 0.08f,
                        RoundBox(fx - bodyCx, fy - (bodyCy - h * 0.06f), w * 0.035f, h * 0.08f, 0));
                    float d = Mathf.Max(Mathf.Min(body, shackle), -keyhole);
                    px[y * w + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(0.5f - d) * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
        }

        /// <summary>The signed distance (px) from a point (relative to the box's centre) to a rounded box of half size hx, hy.</summary>
        static float RoundBox(float px, float py, float hx, float hy, float r)
        {
            float qx = Mathf.Abs(px) - (hx - r), qy = Mathf.Abs(py) - (hy - r);
            float outside = Mathf.Sqrt(Mathf.Max(qx, 0) * Mathf.Max(qx, 0) + Mathf.Max(qy, 0) * Mathf.Max(qy, 0));
            return outside + Mathf.Min(Mathf.Max(qx, qy), 0) - r;
        }

        /// <summary>
        /// A scrolling list centred on the screen (ScrollRect > Viewport (RectMask2D, a raycast target so the wheel and drags
        /// reach the ScrollRect) > Content (top-stretched; its height set when the rows are laid out)).
        /// </summary>
        (UnityEngine.UI.ScrollRect scroll, RectTransform content) ScrollList(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.ScrollRect));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(UnityTuning.MetaListWidth, UnityTuning.MetaListHeight);
            rt.anchoredPosition = new Vector2(0, UnityTuning.MetaListTop);
            var viewport = Img(rt, "Viewport", UiTheme.A(UiTheme.White, 0f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            viewport.raycastTarget = true;
            viewport.canvasRenderer.cullTransparentMesh = true; // drawn as nothing, still hit by raycasts
            viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(viewport.transform, false);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var scroll = go.GetComponent<UnityEngine.UI.ScrollRect>();
            scroll.viewport = viewport.rectTransform;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = UnityTuning.MetaScrollSensitivity;
            return (scroll, content);
        }

        /// <summary>A list row: a top-stretched rect in the list's content with the badge's faint back and a left edge.</summary>
        UnityEngine.UI.Image Row(RectTransform content, string name, float height, out UnityEngine.UI.Image edge)
        {
            var row = Img(content, name, rowBack, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, height));
            row.sprite = UiTheme.Cut; // NIGHT17 U3-30.10: the row style (a solid INK-SOFT cut plate, a 4 px accent bar)
            row.type = UnityEngine.UI.Image.Type.Sliced;
            row.rectTransform.pivot = new Vector2(0.5f, 1);
            row.rectTransform.anchoredPosition = Vector2.zero;
            edge = Img(row.transform, "Edge", faint, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(UnityTuning.ThemeAccentBar, 0));
            edge.rectTransform.pivot = new Vector2(0, 0.5f);
            return row;
        }

        /// <summary>Scroll a list back to its top (a screen opens) after its content got `height` px tall.</summary>
        static void ResetList(UnityEngine.UI.ScrollRect scroll, RectTransform content, float height)
        {
            content.sizeDelta = new Vector2(0, height);
            content.anchoredPosition = Vector2.zero;
            scroll.velocity = Vector2.zero;
        }

        // --- the toast stack -----------------------------------------------------------------------------------------------

        sealed class ToastView
        {
            public RectTransform rt;
            public CanvasGroup group;
            public TextMeshProUGUI head, sub;
            public double age;
            public float height, alpha = -1;
            public string flag; // the unlock it announces (null: none): an UnlockEarned for it is the same news
            public bool fromContract;
        }

        struct ToastData
        {
            public string head, sub, flag;
            public bool fromContract;
        }

        readonly List<ToastView> toastPool = new List<ToastView>();
        readonly List<ToastView> toastLive = new List<ToastView>();
        readonly List<ToastData> toastQueue = new List<ToastData>();
        bool leftDirty;

        void BuildToasts()
        {
            for (int i = 0; i < UnityTuning.ToastMax; i++)
            {
                var back = Img(metaLayer, $"Toast {i}", UiTheme.Ink, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(UnityTuning.ToastWidth, UnityTuning.ToastHeadLine));
                back.rectTransform.pivot = new Vector2(0, 1);
                Img(back.transform, "Edge", UiTheme.Cyan, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(UnityTuning.ThemeAccentBar, 0)).rectTransform.pivot = new Vector2(0, 0.5f);
                var v = new ToastView { rt = back.rectTransform, group = back.gameObject.AddComponent<CanvasGroup>() };
                v.group.interactable = false;
                v.group.blocksRaycasts = false;
                float w = UnityTuning.ToastWidth - 2 * UnityTuning.ToastPadX;
                v.head = Text(back.transform, "Head", "", tBody, text, TextAlignmentOptions.Left, new Vector2(UnityTuning.ToastPadX, -UnityTuning.ToastPadY), new Vector2(w, UnityTuning.ToastHeadLine), new Vector2(0, 1));
                v.head.characterSpacing = UnityTuning.HintSpacing;
                v.head.overflowMode = TextOverflowModes.Ellipsis;
                v.sub = Text(back.transform, "Sub", "", tSmall, usable, TextAlignmentOptions.Left, new Vector2(UnityTuning.ToastPadX, -UnityTuning.ToastPadY - UnityTuning.ToastHeadLine), new Vector2(w, UnityTuning.ToastSubLine), new Vector2(0, 1));
                v.sub.overflowMode = TextOverflowModes.Ellipsis;
                back.gameObject.SetActive(false);
                toastPool.Add(v);
            }
            // The events (payloads are reused by their emitters: everything needed is copied here).
            game.events.On<ContractEvent>(MetaEvents.ContractDone, e =>
                Toast(Tagged("CONTRACT", e.name), e.unlockName != null ? "UNLOCKED: " + e.unlockName : null, e.unlock, true));
            game.events.On<UnlockEvent>(MetaEvents.UnlockEarned, e => Toast(Tagged("UNLOCKED", e.name ?? e.flag), null, e.flag, false));
            game.events.On<MasteryEvent>(MetaEvents.MasteryTier, e =>
            {
                string gun = game.mastery?.Of(e.weapon)?.name ?? WeaponName(e.weapon);
                Toast(Tagged(gun, "MASTERY " + e.tier), e.finish != null ? "FINISH: " + Finishes.Name(cfg, e.finish) : null, null, false);
            });
            game.events.On<HeatEvent>(MetaEvents.HeatUnlock, e =>
            {
                string name = game.heat != null ? game.heat.Name(e.rung) : "HEAT " + e.rung;
                string rule = game.heat?.Rule(e.rung);
                Toast(Tagged(name, "OPEN"), string.IsNullOrEmpty(rule) ? null : rule, null, false);
            });
        }

        /// <summary>A toast head: the tag in cyan, then the name ("CONTRACT  FIRST BLOOD").</summary>
        string Tagged(string tag, string name) => $"<color=#{usableHex}>{tag}</color>  {name}";

        /// <summary>Queue a toast; an unlock already announced by its contract (or the reverse) is one toast.</summary>
        void Toast(string head, string sub, string flag, bool fromContract)
        {
            if (flag != null)
            {
                foreach (ToastView v in toastLive)
                {
                    if (v.flag != flag) continue;
                    if (fromContract && !v.fromContract) Fill(v, head, sub, flag, true); // the contract's toast says more
                    return;
                }
                for (int i = 0; i < toastQueue.Count; i++)
                {
                    if (toastQueue[i].flag != flag) continue;
                    if (fromContract && !toastQueue[i].fromContract) toastQueue[i] = new ToastData { head = head, sub = sub, flag = flag, fromContract = true };
                    return;
                }
            }
            toastQueue.Add(new ToastData { head = head, sub = sub, flag = flag, fromContract = fromContract });
            PumpToasts();
        }

        void PumpToasts()
        {
            while (toastQueue.Count > 0 && toastPool.Count > 0)
            {
                ToastData d = toastQueue[0];
                toastQueue.RemoveAt(0);
                ToastView v = toastPool[toastPool.Count - 1];
                toastPool.RemoveAt(toastPool.Count - 1);
                v.age = 0;
                v.alpha = 0;
                v.group.alpha = 0; // invisible until the layout places it (an event may arrive mid-step)
                Fill(v, d.head, d.sub, d.flag, d.fromContract);
                v.rt.gameObject.SetActive(true);
                toastLive.Add(v);
                leftDirty = true;
            }
        }

        void Fill(ToastView v, string head, string sub, string flag, bool fromContract)
        {
            v.flag = flag;
            v.fromContract = fromContract;
            v.head.text = head;
            bool hasSub = !string.IsNullOrEmpty(sub);
            v.sub.text = hasSub ? sub : string.Empty;
            v.sub.gameObject.SetActive(hasSub);
            v.height = 2 * UnityTuning.ToastPadY + UnityTuning.ToastHeadLine + (hasSub ? UnityTuning.ToastSubLine : 0);
            v.rt.sizeDelta = new Vector2(UnityTuning.ToastWidth, v.height);
            leftDirty = true;
        }

        void RenderToasts(double frameDt)
        {
            for (int i = toastLive.Count - 1; i >= 0; i--)
            {
                ToastView v = toastLive[i];
                v.age += frameDt;
                if (v.age >= UnityTuning.ToastLife)
                {
                    v.rt.gameObject.SetActive(false);
                    toastLive.RemoveAt(i);
                    toastPool.Add(v);
                    leftDirty = true;
                    continue;
                }
                float a = (float)Math.Min(1, Math.Min(v.age / UnityTuning.CardFade, (UnityTuning.ToastLife - v.age) / UnityTuning.CardFade));
                if (a != v.alpha)
                {
                    v.alpha = a;
                    v.group.alpha = a;
                }
            }
            if (toastQueue.Count > 0) PumpToasts();
            if (leftDirty || Brief55LeftMoved()) LayoutLeft(); // NIGHT55 K brief55 (UiBrief55.cs): the column re-laid when the room counter opens or closes (leftDirty alone with the row off)
        }

        /// <summary>The top-left column: the lesson card, the hint, then the toasts, each under the one above.</summary>
        void LayoutLeft()
        {
            leftDirty = false;
            float y = -margin;
            y = Brief55LeftTop(y); // NIGHT55 K brief55 (UiBrief55.cs): under the room counter's plate while it shows (y itself with the row off)
            if (lessonRt.gameObject.activeSelf)
            {
                lessonRt.anchoredPosition = new Vector2(margin, y);
                y -= lessonH + UnityTuning.LeftColumnGap;
            }
            if (padHintRt != null && padHintRt.gameObject.activeSelf) // NIGHT20: the first-pad hint card
            {
                padHintRt.anchoredPosition = new Vector2(margin, y);
                y -= padHintH + UnityTuning.LeftColumnGap;
            }
            if (hintRt.gameObject.activeSelf)
            {
                hintRt.anchoredPosition = new Vector2(margin, y);
                y -= hintH + UnityTuning.LeftColumnGap;
            }
            for (int i = 0; i < toastLive.Count; i++)
            {
                toastLive[i].rt.anchoredPosition = new Vector2(margin, y);
                y -= toastLive[i].height + UnityTuning.LeftColumnGap;
            }
        }
    }
}
