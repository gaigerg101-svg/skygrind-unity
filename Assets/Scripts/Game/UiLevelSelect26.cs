// NIGHT26 (builder L, contract level-select): the CAMPAIGN's LEVEL SELECT, ULTRAKILL's way (GG: "it should be like ULTRAKILL where it kind of shows all the levels what
// rank you got what time ... copy that and go beyond this"). TITLE > PLAY with the mode on CAMPAIGN opens this screen instead of starting a run (design.n26.levelSelect):
//   head      CAMPAIGN
//   headers   RANK / TIME / KILLS / STYLE / UNLOCKS (S18 muted)
//   a column  ten slots (design.n26.levelSelect.slots): "1  THE FOUNDRY", the best OVERALL letter (S42), the best time, kills %, style letter, and the weapon the level
//             unlocks (Design/levels/unlocks.json, else a stub list). Levels past design.n26.levelSelect.playable show LOCKED, veiled and skipped by the focus.
//   cursor    on the first level not yet cleared (LevelSelect26.Cursor): A / Enter / a click starts it, B / Esc goes back (CloseMeta, as CONTRACTS), up / down move.
// Starting a level is CONTINUE's way (the scene reloads on the level and its run starts at once): SurvivalGame.PendingMapId / PendingMode / AutoStartCampaign25; the level
// the booted map already is starts at once. The screen is a normal menu screen (the menus' entrance machinery fades and slides its head and rows: no pop-in, M9); the rows'
// cell texts are children of the row's plate, tinted dark while the row is lit (the plate turns cyan). Text is 18 px or more (UiTheme's floor; the smallest is read).
// The reel scenarios `levelselect` and `ranks` read the static mirrors below. Not run in the Editor when written.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Skygrind.Game
{
    /// <summary>NIGHT26: the weapon a level unlocks, by its id (builder W's Design/levels/unlocks.json when present, else the stub list), and its display name.</summary>
    public static class LevelSel26
    {
        // the stub list the prompt names (tutorial / L1 shotgun, L2 nailgun, L3 rattler) until builder W's unlocks.json lands; the file wins when it names a level
        static readonly Dictionary<string, string> Stub = new Dictionary<string, string> { { "tutorial", "shotgun" }, { "level2", "nailgun" }, { "layer1", "rattler" } };
        static Dictionary<string, string> file;

        public static string UnlockOf(SurvivalGame g, string levelId)
        {
            if (file == null) file = ReadFile(g);
            if (file.TryGetValue(levelId, out string w)) return w;
            return Stub.TryGetValue(levelId, out w) ? w : null;
        }

        static Dictionary<string, string> ReadFile(SurvivalGame g)
        {
            try
            {
                foreach (SurvivalGame.MapEntry m in g.Maps)
                {
                    if (!m.campaignOnly) continue;
                    string p = Path.Combine(Path.GetDirectoryName(m.file) ?? ".", "unlocks.json");
                    if (File.Exists(p)) return UnlocksFile26.Read(File.ReadAllText(p));
                }
            }
            catch (Exception) { }
            return new Dictionary<string, string>();
        }

        /// <summary>The weapon's card name ("THUNDERCLAP"), the id upper-cased when the config names none; "" for none.</summary>
        public static string WeaponName(SurvivalGame g, string weapon)
        {
            if (string.IsNullOrEmpty(weapon)) return "";
            return g.cfg.Has("weapons.names." + weapon) ? ((string)g.cfg.Get("weapons.names." + weapon)).ToUpperInvariant() : weapon.ToUpperInvariant();
        }

        public static bool On(SurvivalGame g) => !N26Blocks.ForceOff && N26Blocks.From(g.cfg).levelSelect;

        /// <summary>NIGHT40 S1: is cut40 on (design.n40.cut40.enabled, not -skygrindOff40 / --off40 or an older switch)? The campaign order reads the same flag.</summary>
        public static bool Cut40On(GameConfig cfg) => cfg != null && !N40Blocks.ForceOff && cfg.Design("n40.cut40.enabled");

        /// <summary>NIGHT30 (builder V; the lead's round 4): the select's numbers, with LEVEL 4 THE SPAN playable while design.n30.span is on (NIGHT26's
        /// design.n26.levelSelect.playable 3 kept levels 4+ LOCKED); -skygrindOff30 or the row off: NIGHT29's three.</summary>
        public static LevelSelect26Params Params(GameConfig cfg)
        {
            LevelSelect26Params p = LevelSelect26Params.From(cfg);
            if (N30Fixes.ViewRowOn(cfg, "span") && p.playable < UnityTuning.LevelSel30SpanPlayable) p.playable = UnityTuning.LevelSel30SpanPlayable;
            // NIGHT32 F: LEVEL 5 THE SPIRE playable while design.n32.level5 is on (Curriculum25.N32Levels puts it in slot 5); -skygrindOff32 or the row off: the 4 above
            if (N32Fixes.ViewRowOn(cfg, "level5") && !N32Blocks.ForceOff && p.playable < UnityTuning.LevelSel32Level5Playable) p.playable = UnityTuning.LevelSel32Level5Playable;
            // NIGHT35 A: LEVEL 6 (the finale) playable while design.n35.campaign6 is on (Curriculum25.N35Levels); -skygrindOff35 or the row off: the 5 above
            if (N35Fixes.ViewRowOn(cfg, "campaign6") && !N35Blocks.ForceOff && p.playable < UnityTuning.LevelSel35Playable) p.playable = UnityTuning.LevelSel35Playable;
            // NIGHT40 S1 (cut40): level 6 out of the campaign (Curriculum25.N40Levels): six slots, the sixth LOCKED and unnamed, slots 7-10 gone, 5 playable;
            // -skygrindOff40 or the row off: the numbers above
            if (Cut40On(cfg))
            {
                p.slots = UnityTuning.LevelSel40Slots;
                if (p.playable > UnityTuning.LevelSel40Playable) p.playable = UnityTuning.LevelSel40Playable;
            }
            return p;
        }
    }

    public sealed partial class Ui
    {
        sealed class LevelRow26
        {
            public MenuItemView item;
            public TextMeshProUGUI rank, time, kills, style, unlock;
            public UiShape veil;
            public LevelSlot26 slot;
            public float litShown = -2;
            public Color rankBase, textBase, unlockBase;
        }

        GameObject levelsScreen26;
        readonly List<LevelRow26> levelRows26 = new List<LevelRow26>();
        readonly List<TextMeshProUGUI> levelTexts26 = new List<TextMeshProUGUI>();
        TextMeshProUGUI levelHint26;
        List<LevelSlot26> levelSlots26;
        bool levelFocusPending26, levelFocusHolding26;
        double levelFocusAge26;
        public bool LevelSelectOpen26 => levelsScreen26 != null && levelsScreen26.activeSelf;
        public static bool NoRankSave26; // the reels set it: a scenario's seeded profile is restored, never saved

        // what the screen shows now (the reel reads these; the Ui instance is private to SurvivalGame)
        public static bool LevelSel26Open;
        public static readonly List<string> LevelSel26RowsNow = new List<string>();
        public static string LevelSel26CursorNow = "", LevelSel26WentTo = "";
        public static float LevelSel26MinPx, LevelSel26FirstAlpha = -1;
        public static int LevelSel26Locked;
        public static string LevelSel26SlotsNow = "", LevelSel26HeldBy = ""; // the slot states the cursor was chosen from; what ended the focus hold

        bool LevelSelectOn26 => LevelSel26.On(game);

        void BuildLevelSelect26()
        {
            levelsScreen26 = Screen("Level select", UiTheme.A(UiTheme.InkSolid, UnityTuning.ThemeInkAlpha));
            TextMeshProUGUI head = MenuHead(levelsScreen26.transform, UnityTuning.LevelSel26Head, UnityTuning.MetaHeadY);
            levelTexts26.Add(head);
            float pad = UnityTuning.ThemeCut * 2.4f, w = UnityTuning.LevelSel26RowWidth;
            float y0 = UnityTuning.LevelSel26FirstRowY;
            // the column labels sit over their cells, by the first row's left edge
            string[] labels = { "RANK", "TIME", "KILLS", "STYLE", "UNLOCKS" };
            float[] xs = { UnityTuning.LevelSel26RankX, UnityTuning.LevelSel26TimeX, UnityTuning.LevelSel26KillsX, UnityTuning.LevelSel26StyleX, UnityTuning.LevelSel26UnlockX };
            float[] ws = { UnityTuning.LevelSel26RankW, UnityTuning.LevelSel26TimeW, UnityTuning.LevelSel26KillsW, UnityTuning.LevelSel26StyleW, UnityTuning.LevelSel26UnlockW };
            for (int i = 0; i < labels.Length; i++)
            {
                TextMeshProUGUI t = Text(levelsScreen26.transform, "Header " + labels[i], labels[i], UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, i == 4 ? TextAlignmentOptions.Left : TextAlignmentOptions.Center, Vector2.zero, new Vector2(ws[i], 28), new Vector2(0.5f, 0.5f));
                UiTheme.Style(t, UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, UiTheme.BodyBold);
                t.characterSpacing = UnityTuning.ThemeLabelTracking;
                float hy = UnityTuning.LevelSel26HeadersY;
                MenuPinLeft(t, MenuLeft(y0) + xs[i] - (i == 4 ? 0 : 0), hy);
                t.rectTransform.pivot = new Vector2(0, 0.5f);
                MenuPart18Add(levelsScreen26, t);
                levelTexts26.Add(t);
            }
            int n = LevelSel26.Params(cfg).slots;
            for (int i = 0; i < n; i++)
            {
                float y = y0 - i * UnityTuning.LevelSel26RowPitch;
                var row = new LevelRow26();
                row.item = MenuItem(levelsScreen26.transform, "Level " + (i + 1), MenuAt(y, w), new Vector2(w, UnityTuning.MenuItemHeight), UiTheme.Size.S32, null, hero: false);
                int idx = i;
                var b = row.item.go.GetComponent<UnityEngine.UI.Button>();
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(() => PressLevel26(idx));
                RectTransform lrt = row.item.label.rectTransform;
                lrt.sizeDelta = new Vector2(UnityTuning.LevelSel26NameW, UnityTuning.MenuItemHeight);
                lrt.anchoredPosition = new Vector2(-w * 0.5f + pad + UnityTuning.LevelSel26NameW * 0.5f, 0);
                row.item.label.alignment = TextAlignmentOptions.Left;
                row.rank = Cell26(row.item.go.transform, "Rank", UiTheme.Size.S42, UiTheme.Rank, TextAlignmentOptions.Center, w, UnityTuning.LevelSel26RankX, UnityTuning.LevelSel26RankW);
                row.time = Cell26(row.item.go.transform, "Time", UiTheme.Size.S24, UiTheme.BodyBold, TextAlignmentOptions.Center, w, UnityTuning.LevelSel26TimeX, UnityTuning.LevelSel26TimeW);
                row.kills = Cell26(row.item.go.transform, "Kills", UiTheme.Size.S24, UiTheme.BodyBold, TextAlignmentOptions.Center, w, UnityTuning.LevelSel26KillsX, UnityTuning.LevelSel26KillsW);
                row.style = Cell26(row.item.go.transform, "Style", UiTheme.Size.S24, UiTheme.BodyBold, TextAlignmentOptions.Center, w, UnityTuning.LevelSel26StyleX, UnityTuning.LevelSel26StyleW);
                row.unlock = Cell26(row.item.go.transform, "Unlock", UiTheme.Size.S24, UiTheme.BodyBold, TextAlignmentOptions.Left, w, UnityTuning.LevelSel26UnlockX, UnityTuning.LevelSel26UnlockW);
                var veilGo = new GameObject("Locked veil", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
                var vrt = (RectTransform)veilGo.transform;
                vrt.SetParent(row.item.go.transform, false);
                vrt.anchorMin = vrt.anchorMax = new Vector2(0.5f, 0.5f);
                vrt.sizeDelta = new Vector2(w, UnityTuning.MenuItemHeight);
                row.veil = veilGo.GetComponent<UiShape>();
                row.veil.color = UiTheme.A(UiTheme.InkSolid, UnityTuning.LevelSel26LockedDim);
                row.veil.raycastTarget = false;
                veilGo.SetActive(false);
                levelRows26.Add(row);
                levelTexts26.Add(row.item.label);
            }
            levelHint26 = Text(levelsScreen26.transform, "Hint", "ENTER / A  START      ESC / B  BACK", UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, TextAlignmentOptions.Left, Vector2.zero, new Vector2(900, 28), new Vector2(0.5f, 0.5f));
            UiTheme.Style(levelHint26, UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, UiTheme.BodyBold);
            MenuPinLeft(levelHint26, MenuLeft(UnityTuning.LevelSel26HintY) + 6, UnityTuning.LevelSel26HintY);
            MenuPart18Add(levelsScreen26, levelHint26);
            levelTexts26.Add(levelHint26);
            Button(levelsScreen26.transform, "BACK", new Vector2(UnityTuning.LevelSel26BackX, UnityTuning.MetaBackY), CloseMeta);
        }

        TextMeshProUGUI Cell26(Transform row, string name, UiTheme.Size px, TMP_FontAsset face, TextAlignmentOptions align, float rowW, float x, float w)
        {
            TextMeshProUGUI t = Text(row, name, "", UiTheme.Px(px), UiTheme.White, align, new Vector2(-rowW * 0.5f + x + w * 0.5f, 0), new Vector2(w, UnityTuning.MenuItemHeight), new Vector2(0.5f, 0.5f));
            UiTheme.Style(t, UiTheme.Px(px), UiTheme.White, face);
            t.fontStyle = FontStyles.Normal;
            t.overflowMode = TextOverflowModes.Overflow;
            levelTexts26.Add(t);
            return t;
        }

        /// <summary>TITLE > PLAY: on CAMPAIGN the level select opens (block on); every other mode starts its run as before.</summary>
        void PlayPressed26()
        {
            if (LevelSelectOn26 && game.flow == Flow.Title && game.setup.mode == RunSetup.Campaign && levelsScreen26 != null) OpenLevelSelect26();
            else game.StartRun();
        }

        /// <summary>FeelReel `levelselect` / `ranks`: press TITLE's PLAY button itself (its onClick), so the wiring PLAY > level select is what is tested. False: no PLAY button or not on TITLE.</summary>
        public bool ReelPressPlay26()
        {
            if (titlePlay == null || game.flow != Flow.Title || !titlePlay.TryGetComponent(out UnityEngine.UI.Button b)) return false;
            b.onClick.Invoke();
            return true;
        }

        /// <summary>NIGHT29 (LevelSelect29Check): click slot `slot`'s Button through its onClick, the path a mouse click takes. Returns the level id, "" when the slot is not playable.</summary>
        public string ReelClickSlot29(int slot)
        {
            if (levelSlots26 == null || slot < 0 || slot >= levelSlots26.Count || slot >= levelRows26.Count || !levelSlots26[slot].playable) return "";
            if (!levelRows26[slot].item.go.TryGetComponent(out UnityEngine.UI.Button b)) return "";
            string id = levelSlots26[slot].id;
            b.onClick.Invoke();
            return id;
        }

        /// <summary>The slot rows' focus state for the reel: the selected row's name ("Level 3"), "" when none.</summary>
        public string ReelFocusedRow26()
        {
            int i = CursorRow26();
            return i < 0 ? "" : levelRows26[i].item.go.name;
        }

        /// <summary>The reel's A: start the level under the cursor (the pad's submit on the focused row). Returns the level id, "" when none.</summary>
        public string ReelPressLevel26()
        {
            int i = CursorRow26();
            if (i < 0) return "";
            string id = levelSlots26[i].playable ? levelSlots26[i].id : "";
            PressLevel26(i);
            return id;
        }

        int CursorRow26()
        {
            GameObject sel = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            for (int i = 0; i < levelRows26.Count; i++) if (levelRows26[i].item.go == sel) return i;
            return -1;
        }

        void OpenLevelSelect26()
        {
            if (game.flow != Flow.Title || settingsOpen) return;
            OpenMeta(MetaScreen.Levels);
            if (metaOpen != MetaScreen.Levels) return;
            RefreshLevelSelect26();
            levelFocusPending26 = true;
        }

        void RefreshLevelSelect26()
        {
            N25Blocks n25 = N25Blocks.ForceOff ? N25Blocks.Off : N25Blocks.From(cfg);
            var order = Curriculum25.Order(cfg, n25);
            levelSlots26 = LevelSelect26.Slots(order, game.profile, LevelSel26.Params(cfg),
                id => TryMap26(id, out _),
                id => TryMap26(id, out SurvivalGame.MapEntry me) ? me.label : null,
                id => LevelSel26.UnlockOf(game, id));
            LevelSel26RowsNow.Clear();
            LevelSel26Locked = 0;
            bool sel41 = Select41On(); // NIGHT41 J2 (select41, UiLevelSelect41.cs): the boss after each name, slot 6 REBUILDING
            Select41Probe.Bosses = Select41Probe.Rebuilding = 0;
            float min = float.MaxValue;
            for (int i = 0; i < levelRows26.Count; i++)
            {
                LevelRow26 r = levelRows26[i];
                LevelSlot26 s = i < levelSlots26.Count ? levelSlots26[i] : new LevelSlot26 { number = i + 1 };
                r.slot = s;
                bool locked = !s.playable;
                if (locked) LevelSel26Locked++;
                r.item.go.GetComponent<UnityEngine.UI.Button>().interactable = !locked;
                r.veil.gameObject.SetActive(locked);
                r.item.label.text = s.number.ToString(CultureInfo.InvariantCulture) + "  " + (s.exists ? s.name : UnityTuning.LevelSel26Locked);
                if (sel41) Select41Slot(r, s, locked);
                LevelBest26 b = s.best;
                bool played = !locked && b != null && b.Played;
                r.rank.text = played ? b.aggregate.ToString() : locked ? "" : UnityTuning.LevelSel26None;
                r.time.text = played ? Rank26.TimeText(b.seconds) : locked ? "" : UnityTuning.LevelSel26None;
                r.kills.text = played ? ((int)Math.Round(b.killPct)).ToString(CultureInfo.InvariantCulture) + "%" : locked ? "" : UnityTuning.LevelSel26None;
                r.style.text = played ? b.style.ToString() : locked ? "" : UnityTuning.LevelSel26None;
                r.unlock.text = s.exists ? LevelSel26.WeaponName(game, s.unlock) : "";
                r.rankBase = played && (b.aggregate == 'S' || b.aggregate == 'P') ? UiTheme.CyanHot : played && b.aggregate == 'A' ? UiTheme.White : UiTheme.Muted;
                r.textBase = played ? UiTheme.White : UiTheme.Muted;
                r.unlockBase = UiTheme.CyanPale;
                r.litShown = -2; // retint on the next frame
                if (s.exists) LevelSel26RowsNow.Add($"{s.number} | {s.name} | {(locked ? "LOCKED" : r.rank.text)} | {r.time.text} | {r.kills.text} | {r.style.text} | {r.unlock.text}");
                else LevelSel26RowsNow.Add($"{s.number} | LOCKED");
            }
            foreach (TextMeshProUGUI t in levelTexts26) min = Mathf.Min(min, t.fontSize);
            LevelSel26MinPx = min == float.MaxValue ? 999f : min;
            LevelSel26FirstAlpha = -1;
            LevelSel26CursorNow = levelSlots26[LevelSelect26.Cursor(levelSlots26)].id;
            LevelSel26SlotsNow = "cursor slot " + (LevelSelect26.Cursor(levelSlots26) + 1) + "; " + string.Join(" ", levelSlots26.ConvertAll(q => q.number + ":" + (q.exists ? q.id : "-") + (q.playable ? " playable" : " LOCKED") + (q.cleared ? " cleared" : "")).GetRange(0, Math.Min(4, levelSlots26.Count)));
        }

        bool TryMap26(string id, out SurvivalGame.MapEntry found)
        {
            foreach (SurvivalGame.MapEntry m in game.Maps)
                if (m.campaignOnly && m.id == id)
                {
                    found = m;
                    return true;
                }
            found = default;
            return false;
        }

        /// <summary>A slot's press: a playable level starts (the scene reloads on it, or the booted level's run starts now); LOCKED does nothing.</summary>
        void PressLevel26(int i)
        {
            if (levelSlots26 == null || i < 0 || i >= levelSlots26.Count || !levelSlots26[i].playable) return;
            if (N29Fixes.LevelSelectIgnoreRepress && SurvivalGame.PendingMapId != null && SurvivalGame.AutoStartCampaign25 && !SurvivalGame.SkipSceneReload25) return; // NIGHT29: a reload is queued already
            string id = levelSlots26[i].id;
            Room71Press(id); // NIGHT71 B room71 (UiRoom71.cs): the run starts in the picked room (Room71.PendingRoom; none with the row off)
            LevelSel26WentTo = id;
            CloseMeta();
            if (game.MapId == id)
            {
                game.setup.mode = RunSetup.Campaign;
                game.StartRun();
                return;
            }
            SurvivalGame.PendingMapId = id;
            SurvivalGame.PendingMode = RunSetup.Campaign;
            SurvivalGame.AutoStartCampaign25 = true;
            Debug.Log($"[skygrind] level select: {game.MapId} -> {id} (the scene reloads)");
            if (!Application.isPlaying || SurvivalGame.SkipSceneReload25) return; // a reel reads PendingMapId and does not reload
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void RenderLevelSelect26(double frameDt)
        {
            bool on = LevelSelectOpen26;
            LevelSel26Open = on;
            if (!on) return;
            if (LevelSel26FirstAlpha < 0 && levelRows26.Count > 0) LevelSel26FirstAlpha = levelRows26[0].item.group.alpha;
            if (levelFocusPending26) // the menu nav re-picks the column's first row after the screen shows (the n26-1 reel read 'Level 1'): the cursor row is re-asserted for LevelSel26FocusHold s
            {
                levelFocusAge26 = 0;
                levelFocusPending26 = false;
                levelFocusHolding26 = true;
            }
            if (levelFocusHolding26 && EventSystem.current != null)
            {
                levelFocusAge26 += frameDt;
                MenuInput mi = PlayerDevices.Menu;
                int c = LevelSelect26.Cursor(levelSlots26);
                if (!(mi.up || mi.down) && EventSystem.current.currentSelectedGameObject != levelRows26[c].item.go) EventSystem.current.SetSelectedGameObject(levelRows26[c].item.go);
                if (mi.up || mi.down) { levelFocusHolding26 = false; LevelSel26HeldBy = "menu direction at " + levelFocusAge26.ToString("0.00"); }
                else if (levelFocusAge26 >= UnityTuning.LevelSel26FocusHold) { levelFocusHolding26 = false; LevelSel26HeldBy = "timeout"; }
            }
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                int c = CursorRow26();
                if (c >= 0 && levelSlots26[c].exists) LevelSel26CursorNow = levelSlots26[c].id;
            }
            Room71Select(); // NIGHT71 B room71 (UiRoom71.cs): left / right pick the focused level's start room, named on the hint line (nothing with the row off)
            for (int i = 0; i < levelRows26.Count; i++)
            {
                LevelRow26 r = levelRows26[i];
                if (r.litShown == r.item.lit) continue;
                r.litShown = r.item.lit;
                float lit = Mathf.Clamp01(r.item.lit);
                Color litTo = front33 ? UiStyle33.Cyan : UiTheme.InkSolid; // NIGHT33 U (hud33): no cyan plate under a lit row: its cells turn cyan, not dark
                r.rank.color = Color.Lerp(r.rankBase, litTo, lit);
                r.time.color = r.kills.color = r.style.color = Color.Lerp(r.textBase, litTo, lit);
                r.unlock.color = Color.Lerp(r.unlockBase, litTo, lit);
            }
        }
    }
}
