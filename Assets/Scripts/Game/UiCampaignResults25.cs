// NIGHT25 (builder F): the CAMPAIGN RESULTS screen. After layerClear the run ends on RESULTS; with a campaign level cleared (sim.campaignOn and the campaign
// finished) the screen is this one, not the survival tally (ROUND / POINTS / GRADE mean nothing here):
//   head     the level's name and CLEARED (S56), or CAMPAIGN COMPLETE when no level follows
//   summary  OVERALL <rank>  ·  total time  ·  DEATHS n (S32)
//   rows     one per room: its number and name, its rank letter S / A / B / C, its time against par ("0:45  PAR 1:00"), its deaths when there were any (S32 / S24)
//   buttons  CONTINUE (the RETRY button, relabelled: the next level; BACK TO TITLE when none follows), RETRY LEVEL, TITLE. CONTINUE is focused by default on a pad
//            and a keyboard alike, once the rows are in (the press that ended the run, a jump's A, never takes it: the buttons appear UnityTuning.CampRes25ButtonsAt
//            seconds after the screen opens, and the focus is set then).
// Everything fades in (no pop-in: M9), rows staggered. Rank colours: S cyan-hot (the player's), A white, B / C muted; never red, never gold (gold is points).
// The survival tally, DETAILS, RETRY SAME SEED and the developer buttons stay hidden; a normal run's RESULTS is untouched. The reel `campresults` reads the static
// mirrors below. Not run in the Editor when written.
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
        sealed class CampRow25
        {
            public CanvasGroup group;
            public TextMeshProUGUI name, rank, time;
        }

        CanvasGroup campRes26G;
        TextMeshProUGUI campRes26Ranks;
        public static string CampRes26RanksNow = "";
        RectTransform campRes25Root;
        CanvasGroup campRes25HeadG, campRes25SumG;
        TextMeshProUGUI campRes25Head, campRes25Sum;
        readonly List<CampRow25> campRows25 = new List<CampRow25>();
        MenuItemView campRes25Retry;
        public static bool CampRes25Open;
        bool campRes25Buttons, campRes25ChipWas, campRes25Moved, campRes25DirWas;
        public static string CampRes25MovedBy = "";
        double campRes25T;
        string campRes25Next;
        Vector2 campRes25TitleHome;
        bool campRes25Swapped;

        // what the screen shows now (the reel reads these; the Ui instance is private to SurvivalGame)
        public static string CampRes25HeadNow = "", CampRes25SummaryNow = "", CampRes25PrimaryNow = "", CampRes25NextNow;
        /// <summary>NIGHT30: the next level's name on CONTINUE ("THE SPAN"), "" when none.</summary>
        public static string CampRes30NextName = "";

        /// <summary>NIGHT30: a level's display name from the map list ("LEVEL 4 THE SPAN" -> "THE SPAN"; the id upper-cased when it has no label).</summary>
        string NextLevelName30(string id)
        {
            foreach (SurvivalGame.MapEntry m in game.Maps)
            {
                if (m.id != id || string.IsNullOrEmpty(m.label)) continue;
                return LevelName30(m.label);
            }
            return id.ToUpperInvariant();
        }

        /// <summary>"LEVEL 4 THE SPAN" -> "THE SPAN" (a "LEVEL n " prefix dropped); any other label as written (pure: EditMode tested).</summary>
        public static string LevelName30(string label)
        {
            if (string.IsNullOrEmpty(label)) return "";
            var m = System.Text.RegularExpressions.Regex.Match(label, @"^LEVEL\s+\d+\s+(.+)$");
            return m.Success ? m.Groups[1].Value : label;
        }
        public static readonly List<string> CampRes25RowsNow = new List<string>();
        public static float CampRes25MinPx, CampRes25FirstAlpha = -1, CampRes25FocusedAt = -1;
        public static string CampRes25FocusNow = "";

        /// <summary>The campaign's results are showing: RESULTS, on a CAMPAIGN run whose level was cleared.</summary>
        bool CampRes25Wanted => game.flow == Flow.Results && game.sim.campaignOn && game.sim.campaign != null && game.sim.campaign.finished;

        void BuildCampRes25()
        {
            campRes25Root = new GameObject("Campaign results", typeof(RectTransform)).GetComponent<RectTransform>();
            campRes25Root.SetParent(results.transform, false);
            campRes25Root.anchorMin = campRes25Root.anchorMax = new Vector2(0.5f, 0.5f);
            campRes25Root.sizeDelta = Vector2.zero;
            float hy = UnityTuning.ResultsHeadY, w = UnityTuning.CampRes25Width;
            campRes25HeadG = NewGroup25(campRes25Root, "Head");
            campRes25Head = TallyText(campRes25HeadG.transform, "Text", "", UiTheme.Size.S56, UiTheme.White, UiTheme.Display, TextAlignmentOptions.Left, new Vector2(MenuLeft(hy) + w * 0.5f, hy), new Vector2(w, 80), true);
            campRes25SumG = NewGroup25(campRes25Root, "Summary");
            float sy = UnityTuning.CampRes25SummaryY;
            campRes25Sum = TallyText(campRes25SumG.transform, "Text", "", UiTheme.Size.S32, UiTheme.CyanPale, UiTheme.BodyBold, TextAlignmentOptions.Left, new Vector2(MenuLeft(sy) + w * 0.5f, sy), new Vector2(w, 46));
            campRes26G = NewGroup25(campRes25Root, "Ranks"); // NIGHT26 (builder L): TIME / KILLS / STYLE / OVERALL and the weapon unlocked
            float ry26 = UnityTuning.CampRes26RanksY;
            campRes26Ranks = TallyText(campRes26G.transform, "Text", "", UiTheme.Size.S24, UiTheme.CyanPale, UiTheme.BodyBold, TextAlignmentOptions.Left, new Vector2(MenuLeft(ry26) + UnityTuning.CampRes26Width * 0.5f, ry26), new Vector2(UnityTuning.CampRes26Width, 36));
            for (int i = 0; i < UnityTuning.CampRes25MaxRows; i++)
            {
                float y = UnityTuning.CampRes25FirstRowY - i * UnityTuning.CampRes25RowPitch, left = MenuLeft(y);
                var row = new CampRow25 { group = NewGroup25(campRes25Root, "Row " + (i + 1)) };
                row.name = TallyText(row.group.transform, "Name", "", UiTheme.Size.S32, UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Left, new Vector2(left + UnityTuning.CampRes25NameW * 0.5f, y), new Vector2(UnityTuning.CampRes25NameW, 44));
                row.rank = TallyText(row.group.transform, "Rank", "", UiTheme.Size.S42, UiTheme.White, UiTheme.Rank, TextAlignmentOptions.Center, new Vector2(left + UnityTuning.CampRes25NameW + UnityTuning.CampRes25RankW * 0.5f, y), new Vector2(UnityTuning.CampRes25RankW, 50));
                row.time = TallyText(row.group.transform, "Time", "", UiTheme.Size.S24, UiTheme.Muted, UiTheme.BodyBold, TextAlignmentOptions.Left, new Vector2(left + UnityTuning.CampRes25NameW + UnityTuning.CampRes25RankW + UnityTuning.CampRes25TimeW * 0.5f, y), new Vector2(UnityTuning.CampRes25TimeW, 44));
                campRows25.Add(row);
            }
            float ry = UnityTuning.ResultsFirstRowY - UnityTuning.MenuItemPitch;
            campRes25Retry = MenuItem(results.transform, "RETRY LEVEL", MenuAt(ry, UnityTuning.MenuItemWidth), new Vector2(UnityTuning.MenuItemWidth, UnityTuning.MenuItemHeight), UiTheme.Size.S42, () => game.StartRun());
            campRes25Retry.go.SetActive(false);
            campRes25Root.gameObject.SetActive(false);
        }

        CanvasGroup NewGroup25(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.zero;
            var g = go.GetComponent<CanvasGroup>();
            g.alpha = 0;
            g.interactable = g.blocksRaycasts = false;
            return g;
        }

        /// <summary>The primary button's action while this screen shows (the RETRY button's click calls this in place of a restart).</summary>
        void CampRes25Primary() => game.ContinueCampaign();

        /// <summary>RETRY's click: a restart, or CONTINUE while the campaign's results show.</summary>
        void RetryOrContinue25()
        {
            if (CampRes25Open) CampRes25Primary();
            else game.StartRun();
        }

        /// <summary>The CONTINUE button's own click (what a mouse click on it does): the reel's second verdict, apart from where the focus is.</summary>
        public bool CampRes25PressPrimary()
        {
            if (restartButton == null || !restartButton.activeInHierarchy || !restartButton.TryGetComponent(out UnityEngine.UI.Button b)) return false;
            b.onClick.Invoke();
            return true;
        }

        /// <summary>Submit the focused control as the pad's A does (the reel's press); false when nothing is focused.</summary>
        public bool CampRes25Submit()
        {
            EventSystem es = EventSystem.current;
            if (es == null || es.currentSelectedGameObject == null) return false;
            ExecuteEvents.Execute(es.currentSelectedGameObject, new BaseEventData(es), ExecuteEvents.submitHandler);
            return true;
        }

        void OpenCampRes25()
        {
            if (campRes25Root == null) BuildCampRes25();
            CampaignSim25 c = new CampaignSim25(game);
            CampRes25Open = true;
            campRes25T = 0;
            campRes25Buttons = false;
            campRes25Moved = false;
            campRes25DirWas = false;
            CampRes25MovedBy = "";
            CampRes25FirstAlpha = -1;
            CampRes25FocusedAt = -1;
            CampRes25FocusNow = "";
            resultsDetailsOpen = false;
            campRes25Next = game.CampaignNextLevel25();
            CampRes25NextNow = campRes25Next;
            tallyRoot.gameObject.SetActive(false);
            tallyDetailsG.gameObject.SetActive(false);
            if (payoffPanel != null && payoffPanel.parent == results.transform) payoffPanel.gameObject.SetActive(false);
            campRes25Root.gameObject.SetActive(true);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            // the buttons: none until the rows are in
            SetActiveIf(restartButton, false);
            SetActiveIf(titleButton, false);
            SetActiveIf(resultsDetailsItem.go, false);
            SetActiveIf(retryButton, false);
            SetActiveIf(copyButton, false);
            SetActiveIf(campRes25Retry.go, false);
            campRes25ChipWas = menuRetryChip != null && menuRetryChip.gameObject.activeSelf;
            if (menuRetryChip != null) menuRetryChip.gameObject.SetActive(false); // CONTINUE has no key chip
            MenuItemView primary = menuItemOf[restartButton], title = menuItemOf[titleButton];
            primary.label.text = campRes25Next != null ? "CONTINUE" : "BACK TO TITLE";
            CampRes25PrimaryNow = primary.label.text;
            // NIGHT30 (builder V; the lead's round 4): CONTINUE names the level it goes to ("CONTINUE: THE SPAN" after THE KILN), from the map list's label; the
            // reels keep reading the button as CONTINUE (CampRes25PrimaryNow). Off (-skygrindOff30): NIGHT29's bare CONTINUE.
            CampRes30NextName = campRes25Next != null ? NextLevelName30(campRes25Next) : "";
            if (N30Fixes.On && CampRes30NextName.Length > 0) primary.label.text = "CONTINUE: " + CampRes30NextName;
            // CONTINUE, RETRY LEVEL, TITLE in that order: RETRY LEVEL sits in TITLE's usual slot, TITLE one slot lower
            campRes25TitleHome = title.home;
            title.home = new Vector2(campRes25TitleHome.x, campRes25TitleHome.y - UnityTuning.MenuItemPitch);
            title.rt.anchoredPosition = title.home;
            campRes25Swapped = true;

            // the texts
            CampaignSim25 d = c;
            bool complete = campRes25Next == null;
            campRes25Head.text = complete ? "CAMPAIGN COMPLETE" : d.levelName + " CLEARED";
            campRes25Sum.text = d.summary;
            CampRes25HeadNow = campRes25Head.text;
            CampRes25SummaryNow = d.summary;
            CampRes25RowsNow.Clear();
            float min = float.MaxValue;
            for (int i = 0; i < campRows25.Count; i++)
            {
                CampRow25 r = campRows25[i];
                bool on = i < d.rows.Count;
                r.group.gameObject.SetActive(on);
                r.group.alpha = 0;
                if (!on) continue;
                var row = d.rows[i];
                r.name.text = row.name;
                r.rank.text = row.rank;
                r.rank.color = row.rank == "S" ? UiTheme.CyanHot : row.rank == "A" ? UiTheme.White : UiTheme.Muted;
                r.time.text = row.time;
                CampRes25RowsNow.Add($"{row.name} | {row.rank} | {row.time}");
                min = Mathf.Min(min, r.name.fontSize, r.rank.fontSize, r.time.fontSize);
            }
            // NIGHT26 (builder L): the three ranks, the aggregate and the weapon unlocked; the best per level goes into the profile (once: this opens once per cleared level)
            LevelRank26 rk = d.rank26;
            campRes26Ranks.text = rk != null ? rk.Line + (rk.restarts > 0 ? "  RESTARTS " + rk.restarts.ToString(CultureInfo.InvariantCulture) : "") + (!string.IsNullOrEmpty(d.unlockName) ? "      UNLOCKED  " + d.unlockName : "") : "";
            CampRes26RanksNow = campRes26Ranks.text;
            campRes26G.gameObject.SetActive(rk != null);
            if (rk != null)
            {
                min = Mathf.Min(min, campRes26Ranks.fontSize);
                if (!NoRankSave26 && !game.sim.Unranked29 && game.profile != null && game.profile.SubmitLevel(d.levelId, rk)) game.SaveProfile();
            }
            min = Mathf.Min(min == float.MaxValue ? 999f : min, campRes25Head.fontSize, campRes25Sum.fontSize);
            CampRes25MinPx = min;
            campRes25HeadG.alpha = campRes25SumG.alpha = campRes26G.alpha = 0;
            Open43Results(); // NIGHT43 U1 screens43 (UiCampaignResults25.Screens43.cs): the backdrop, the panel, the hero rank; nothing with the row off
        }

        void EndCampRes25()
        {
            CampRes25Open = false;
            End43Results(); // NIGHT43 U1 screens43: the sheet back for the survival RESULTS
            if (campRes25Root != null) campRes25Root.gameObject.SetActive(false);
            SetActiveIf(campRes25Retry != null ? campRes25Retry.go : null, false);
            if (campRes25Swapped)
            {
                MenuItemView title = menuItemOf[titleButton];
                title.home = campRes25TitleHome;
                title.rt.anchoredPosition = campRes25TitleHome;
                menuItemOf[restartButton].label.text = "RETRY";
                if (menuRetryChip != null) menuRetryChip.gameObject.SetActive(campRes25ChipWas);
                campRes25Swapped = false;
            }
        }

        void RenderCampRes25(double frameDt)
        {
            if (!CampRes25Open) OpenCampRes25();
            campRes25T += frameDt;
            float fade = UnityTuning.CampRes25FadeIn > 0 ? (float)(frameDt / UnityTuning.CampRes25FadeIn) : 1f;
            campRes25HeadG.alpha = Mathf.Min(1f, campRes25HeadG.alpha + fade);
            if (CampRes25FirstAlpha < 0) CampRes25FirstAlpha = campRes25HeadG.alpha;
            if (campRes25T >= UnityTuning.CampRes25SummaryAt) campRes25SumG.alpha = Mathf.Min(1f, campRes25SumG.alpha + fade);
            if (campRes25T >= UnityTuning.CampRes25SummaryAt + UnityTuning.CampRes25RowStagger) campRes26G.alpha = Mathf.Min(1f, campRes26G.alpha + fade);
            for (int i = 0; i < campRows25.Count; i++)
            {
                CampRow25 r = campRows25[i];
                if (!r.group.gameObject.activeSelf) continue;
                if (campRes25T >= UnityTuning.CampRes25RowsAt + i * UnityTuning.CampRes25RowStagger) r.group.alpha = Mathf.Min(1f, r.group.alpha + fade);
            }
            if (!campRes25Buttons && campRes25T >= UnityTuning.CampRes25ButtonsAt)
            {
                campRes25Buttons = true;
                SetActiveIf(restartButton, true);
                SetActiveIf(campRes25Retry.go, true);
                SetActiveIf(titleButton, true);
                MenuEnterItem(menuItemOf[restartButton], 0);
                MenuEnterItem(campRes25Retry, 1);
                MenuEnterItem(menuItemOf[titleButton], 2);
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(restartButton); // CONTINUE focused by default
                CampRes25FocusedAt = (float)campRes25T;
            }
            if (campRes25Buttons && EventSystem.current != null)
            {
                // CONTINUE holds the focus until the player moves it (a menu direction): nothing else (the nav's own re-pick, a relink) may take it
                // a menu direction moves the focus away for good, but not in the grace after the buttons appear (a stale press from the run) and only as a fresh edge
                MenuInput mi = PlayerDevices.Menu;
                bool dir = mi.up || mi.down || mi.left || mi.right;
                if (dir && !campRes25DirWas && campRes25T >= UnityTuning.CampRes25ButtonsAt + UnityTuning.CampRes25MoveGrace && !campRes25Moved)
                {
                    campRes25Moved = true;
                    CampRes25MovedBy = $"menu {(mi.up ? "up" : mi.down ? "down" : mi.left ? "left" : "right")} at {campRes25T:0.00} s";
                }
                campRes25DirWas = dir;
                if (!campRes25Moved && EventSystem.current.currentSelectedGameObject != restartButton) EventSystem.current.SetSelectedGameObject(restartButton);
            }
            if (campRes25Buttons && EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
                CampRes25FocusNow = EventSystem.current.currentSelectedGameObject.name == restartButton.name ? CampRes25PrimaryNow : EventSystem.current.currentSelectedGameObject.name;
            Tick43Results(frameDt); // NIGHT43 U1 screens43: the slide along the slant, the hero rank's slam
        }
    }

    /// <summary>NIGHT25: what the campaign results show, read once from the finished campaign (pure data; no UnityEngine).</summary>
    public sealed class CampaignSim25
    {
        public struct Row { public string name, rank, time; }

        public string levelName, summary, levelId, unlockName;
        /// <summary>NIGHT26: the level's ranks (null with design.n26.levelSelect off: the screen is NIGHT25's).</summary>
        public LevelRank26 rank26;
        public readonly List<Row> rows = new List<Row>();

        public CampaignSim25(SurvivalGame g)
        {
            Campaign24 c = g.sim.campaign;
            LevelDef level = c.Level;
            double dt = 1.0 / g.cfg.movement.loop.simHz;
            levelName = (level.name ?? level.id ?? "LEVEL").ToUpperInvariant();
            // NIGHT33 U (design.n33.hud33; the lead's n33-r5 camp-results: "N33-FOUNDRY CLEARED"): the n32 / n33 level files carry their id as "name", so the
            // head showed the id. With the row on the name is the map list's label without its "LEVEL n" ("LEVEL 1 THE FOUNDRY" -> "THE FOUNDRY"), as CONTINUE's.
            if (N33Fixes.ViewRowOn(g.cfg, "hud33"))
                foreach (SurvivalGame.MapEntry m in g.Maps)
                    if ((m.id == level.id || m.id == g.MapId) && !string.IsNullOrEmpty(m.label)) { levelName = Ui.LevelName30(m.label).ToUpperInvariant(); break; }
            int deaths = 0;
            foreach (int d in c.roomDeaths) deaths += d;
            for (int i = 0; i < level.rooms.Count && i < c.clearSteps.Count; i++)
            {
                RoomDef r = level.rooms[i];
                double t = c.clearSteps[i] * dt;
                string rank = i < c.roomRanks.Count ? c.roomRanks[i].ToString() : "-";
                int rd = i < c.roomDeaths.Count ? c.roomDeaths[i] : 0;
                string time = Time(t) + (r.par > 0 ? "  PAR " + Time(r.par) : "") + (rd > 0 ? "  " + rd.ToString(CultureInfo.InvariantCulture) + (rd == 1 ? " DEATH" : " DEATHS") : "");
                rows.Add(new Row { name = (i + 1).ToString(CultureInfo.InvariantCulture) + "  " + (r.name ?? r.id ?? "ROOM").ToUpperInvariant(), rank = rank, time = time });
            }
            if (rows.Count > UnityTuning.CampRes25MaxRows) // NIGHT26: level 1 has more rooms than rows: the last row folds the rest (the level's own ranks are on the line above)
            {
                int more = rows.Count - (UnityTuning.CampRes25MaxRows - 1);
                rows.RemoveRange(UnityTuning.CampRes25MaxRows - 1, more);
                rows.Add(new Row { name = "+" + more.ToString(CultureInfo.InvariantCulture) + " MORE ROOMS", rank = "", time = "" });
            }
            char overall = c.OverallRank();
            summary = (overall != '\0' ? "OVERALL " + overall + "  ·  " : "") + Time(c.TotalSeconds) + "  ·  DEATHS " + deaths.ToString(CultureInfo.InvariantCulture);
            levelId = level.id;
            if (LevelSel26.On(g)) // NIGHT26: TIME / KILLS / STYLE by the level's thresholds, the aggregate (restarts = the checkpoint restarts), the weapon unlocked
            {
                int planned = level.PlannedEnemies;
                double killPct = planned <= 0 ? 100 : System.Math.Min(100, 100.0 * g.run.stats.kills / planned);
                rank26 = Rank26.Compute(level.RankFor(Rank26Params.From(g.cfg)), Rank26Params.From(g.cfg), c.TotalSeconds, killPct, g.run.stats.style, c.retries);
                unlockName = LevelSel26.WeaponName(g, LevelSel26.UnlockOf(g, level.id) ?? level.LastAward);
            }
        }

        static string Time(double s)
        {
            int t = (int)System.Math.Round(s);
            return (t / 60).ToString(CultureInfo.InvariantCulture) + ":" + (t % 60).ToString("00", CultureInfo.InvariantCulture);
        }
    }
}
