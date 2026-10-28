// NIGHT43 builder U1 (design.n43.screens43, a view row; GG's G24: "The shop screen, post-level score screen, and other full-screen UI also need
// substantial redesign"). With the row on, the campaign results in the dusk instrument's language (Research/2026-10-16-wave43/U1.md), laid out as
// components (round 2, the lead's n43-r1 frame s-res: "one crammed cyan line ... a huge empty slanted panel; the menu items outside the panel"):
//   - the backdrop: the game's last frame blurred once and multiplied toward the dusk violet, with scanlines (UiBlur43), in place of the 0.88 ink sheet;
//     the screen is hidden on its first frame so the capture is the game alone;
//   - the head (the level's name + CLEARED) in the HERO face at 75;
//   - THE HERO: the OVERALL grade (the level's aggregate rank, else the rooms' overall) in the rank face at 180 px in its own INK panel, slammed in;
//   - four STAT TILES: TIME / KILLS / STYLE / DEATHS, each its value and its own grade (the level's ranks; DEATHS has none: its count is the value);
//   - POINTS BANKED and the WALLET in gold (points) under the tiles;
//   - the rooms (number + name, grade, time vs par) in a panel sized to the rows there are (none: no panel);
//   - the menu (CONTINUE / RETRY LEVEL / TITLE) on its own INK blade panel that leans with the menu column;
//   - the entrance: hero, tiles, banked, rooms, menu panel in turn along the slant (0.15 s each, 0.05 s apart); the head slides as it fades.
// Every source text, rank, button and reel mirror of UiCampaignResults25.cs is unchanged (the old summary / ranks / row texts are hidden, not
// rewritten). The survival RESULTS (the tally) are untouched: the sheet comes back when this screen ends. Off: nothing here is built.
// Numbers: UnityTuning.Ui43.cs (Results43*).
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        sealed class Tile43
        {
            public RectTransform rt;
            public TextMeshProUGUI label, value, grade;
        }

        sealed class Room43
        {
            public RectTransform rt;
            public TextMeshProUGUI name, grade, time;
        }

        bool r43Read, r43On, r43Built;
        RawImage r43Back, r43Scan;
        Image r43Sheet, r43RoomsPanel;
        Color r43SheetWas;
        CanvasGroup r43Hide;
        RectTransform r43HeroRt, r43RoomsRt, r43MenuRt, r43TilesRt, r43BankRt;
        TextMeshProUGUI r43Rank, r43Bank;
        readonly List<Tile43> r43Tiles = new List<Tile43>();
        readonly List<Room43> r43Rooms = new List<Room43>();
        readonly List<RectTransform> r43Groups = new List<RectTransform>();
        readonly List<CanvasGroup> r43GroupAlpha = new List<CanvasGroup>();
        double r43Age = -1;
        bool r43Settled;

        /// <summary>For the reels: the hero grade, the tiles' texts, the rooms drawn.</summary>
        public static string Results43HeroNow = "", Results43TilesNow = "";
        public static int Results43RoomsNow;

        bool Results43On
        {
            get
            {
                if (!r43Read && cfg != null) { r43Read = true; r43On = N43View.RowOn(cfg, "screens43"); }
                return r43On;
            }
        }

        RectTransform Group43R(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(campRes25Root, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.zero;
            var g = go.GetComponent<CanvasGroup>();
            g.interactable = g.blocksRaycasts = false;
            r43Groups.Add(rt);
            r43GroupAlpha.Add(g);
            return rt;
        }

        static Color Ink43 => UiTheme.Ready ? UiTheme.InkSolid : new Color(0.047f, 0.043f, 0.086f, 1f);

        /// <summary>A grade's colour (UiCampaignResults25's rule: S cyan-hot, A white, the rest muted; never red, never gold).</summary>
        static Color Grade43(char g) => g == 'S' || g == 'P' ? UiTheme.CyanHot : g == 'A' ? UiTheme.White : UiKit43.Muted;

        void BuildResults43()
        {
            r43Built = true;
            Screens43Probe.ResultsBuilt = true;
            var rrt = (RectTransform)results.transform;
            r43Sheet = results.GetComponent<Image>();
            if (r43Sheet != null) r43SheetWas = r43Sheet.color;
            r43Hide = results.GetComponent<CanvasGroup>();
            if (r43Hide == null) r43Hide = results.AddComponent<CanvasGroup>();
            r43Back = UiBlur43.Backdrop(rrt, out r43Scan);
            r43Back.raycastTarget = false; // the sheet under it keeps the clicks it always took
            r43Back.gameObject.SetActive(false);
            Color panel = UiTheme.A(Ink43, UnityTuning.Screens43PanelAlpha), muted = UiKit43.Muted;
            var mid = new Vector2(0.5f, 0.5f);
            var tl = new Vector2(0, 1);

            // the hero: the OVERALL grade in its own panel
            r43HeroRt = Group43R("Hero 43");
            float hs = UnityTuning.Results43HeroSize;
            Image hp = UiKit43.Plate(r43HeroRt, "Panel", panel, mid, tl, new Vector2(UnityTuning.Results43HeroX, UnityTuning.Results43TopY), new Vector2(hs, hs));
            UiStyle33.Line(hp.rectTransform, "Accent", UiStyle33.Cyan, tl, tl, new Vector2(UnityTuning.Results43AccentInset, -UnityTuning.Results43AccentInset), new Vector2(UnityTuning.Screens43AccentW, UnityTuning.Results43AccentH));
            UiKit43.Scan(hp.rectTransform, "Scan", UnityTuning.Ui43ScanAlpha);
            r43Rank = TallyText(hp.rectTransform, "Grade", "", UiTheme.Size.S133, UiTheme.White, UiTheme.Rank, TextAlignmentOptions.Center, new Vector2(0, UnityTuning.Results43HeroLift), new Vector2(hs, hs * 0.75f), true);
            r43Rank.fontSize = UnityTuning.Results43HeroPx;
            r43Rank.rectTransform.anchorMin = r43Rank.rectTransform.anchorMax = mid;
            TextMeshProUGUI hl = TallyText(hp.rectTransform, "Label", "OVERALL", UiTheme.Size.S18, muted, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0, -hs * 0.5f + UnityTuning.Results43LabelFromBottom), new Vector2(hs, 26));
            hl.rectTransform.anchorMin = hl.rectTransform.anchorMax = mid;
            hl.characterSpacing = UnityTuning.ThemeLabelTracking;

            // the four stat tiles
            r43TilesRt = Group43R("Tiles 43");
            string[] names = { "TIME", "KILLS", "STYLE", "DEATHS" };
            float tw = (UnityTuning.Results43RightW - 3 * UnityTuning.Results43Gap) / 4f, th = UnityTuning.Results43TileH;
            for (int i = 0; i < 4; i++)
            {
                var t = new Tile43();
                Image p = UiKit43.Plate(r43TilesRt, names[i], panel, mid, tl, new Vector2(UnityTuning.Results43RightX + i * (tw + UnityTuning.Results43Gap), UnityTuning.Results43TopY), new Vector2(tw, th));
                t.rt = p.rectTransform;
                UiStyle33.Line(t.rt, "Accent", UiStyle33.Cyan, tl, tl, new Vector2(UnityTuning.Results43AccentInset, -UnityTuning.Results43AccentInset), new Vector2(UnityTuning.Screens43AccentW, UnityTuning.Results43AccentH));
                float pad = UnityTuning.Screens43Pad;
                t.label = TallyText(t.rt, "Label", names[i], UiTheme.Size.S18, muted, UiTheme.BodyBold, TextAlignmentOptions.TopLeft, Vector2.zero, new Vector2(tw - 2 * pad, 26));
                Pin43(t.label.rectTransform, tl, new Vector2(pad, -pad * 0.75f));
                t.label.characterSpacing = UnityTuning.ThemeLabelTracking;
                t.value = TallyText(t.rt, "Value", "", UiTheme.Size.S42, UiTheme.White, UiTheme.Display, TextAlignmentOptions.BottomLeft, Vector2.zero, new Vector2(tw - 2 * pad - UnityTuning.Results43GradeW, 54));
                Pin43(t.value.rectTransform, new Vector2(0, 0), new Vector2(pad, pad * 0.6f));
                t.grade = TallyText(t.rt, "Grade", "", UiTheme.Size.S56, UiTheme.White, UiTheme.Rank, TextAlignmentOptions.BottomRight, Vector2.zero, new Vector2(UnityTuning.Results43GradeW, 76));
                Pin43(t.grade.rectTransform, new Vector2(1, 0), new Vector2(-pad, pad * 0.4f));
                t.grade.fontSize = UnityTuning.Results43GradePx;
                r43Tiles.Add(t);
            }

            // points banked / the wallet (gold: points)
            r43BankRt = Group43R("Banked 43");
            r43Bank = TallyText(r43BankRt, "Banked", "", UiTheme.Size.S24, UiTheme.Gold, UiTheme.BodyBold, TextAlignmentOptions.Left, Vector2.zero, new Vector2(UnityTuning.Results43RightW, 34), true);
            Pin43(r43Bank.rectTransform, mid, new Vector2(UnityTuning.Results43RightX, UnityTuning.Results43TopY - th - UnityTuning.Results43BankGap), new Vector2(0, 1));
            r43Bank.characterSpacing = UnityTuning.ThemeLabelTracking;

            // the rooms: one panel, sized at open to the rows there are
            r43RoomsRt = Group43R("Rooms 43");
            r43RoomsPanel = UiKit43.Plate(r43RoomsRt, "Panel", panel, mid, tl, new Vector2(UnityTuning.Results43RoomsX, UnityTuning.Results43RoomsTopY), new Vector2(UnityTuning.Results43RoomsW, 100));
            UiStyle33.Line(r43RoomsPanel.rectTransform, "Accent", UiStyle33.Cyan, tl, tl, new Vector2(UnityTuning.Results43AccentInset, -UnityTuning.Results43AccentInset), new Vector2(UnityTuning.Screens43AccentW, UnityTuning.Results43AccentH));
            for (int i = 0; i < UnityTuning.CampRes25MaxRows; i++)
            {
                var r = new Room43 { rt = UiStyle33.Rect(r43RoomsPanel.rectTransform, "Room " + (i + 1), tl, tl, new Vector2(UnityTuning.Screens43Pad, -(UnityTuning.Screens43Pad * 0.75f + i * UnityTuning.Results43RowPitch)), new Vector2(UnityTuning.Results43RoomsW - 2 * UnityTuning.Screens43Pad, UnityTuning.Results43RowPitch)) };
                r.name = TallyText(r.rt, "Name", "", UiTheme.Size.S24, UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Left, Vector2.zero, new Vector2(UnityTuning.Results43RoomNameW, UnityTuning.Results43RowPitch));
                Pin43(r.name.rectTransform, tl, Vector2.zero);
                r.grade = TallyText(r.rt, "Grade", "", UiTheme.Size.S32, UiTheme.White, UiTheme.Rank, TextAlignmentOptions.Center, Vector2.zero, new Vector2(UnityTuning.Results43RoomGradeW, UnityTuning.Results43RowPitch));
                Pin43(r.grade.rectTransform, tl, new Vector2(UnityTuning.Results43RoomNameW, 0));
                r.time = TallyText(r.rt, "Time", "", UiTheme.Size.S24, muted, UiTheme.BodyBold, TextAlignmentOptions.Right, Vector2.zero, new Vector2(UnityTuning.Results43RoomsW - 2 * UnityTuning.Screens43Pad - UnityTuning.Results43RoomNameW - UnityTuning.Results43RoomGradeW, UnityTuning.Results43RowPitch));
                Pin43(r.time.rectTransform, tl, new Vector2(UnityTuning.Results43RoomNameW + UnityTuning.Results43RoomGradeW, 0));
                r43Rooms.Add(r);
            }

            // the menu's panel: a blade leaning with the menu column, behind CONTINUE / RETRY LEVEL / TITLE
            r43MenuRt = Group43R("Menu panel 43");
            float top = UnityTuning.ResultsFirstRowY + UnityTuning.MenuItemHeight * 0.5f + UnityTuning.Results43MenuPad;
            float bottom = UnityTuning.ResultsFirstRowY - 2 * UnityTuning.MenuItemPitch - UnityTuning.MenuItemHeight * 0.5f - UnityTuning.Results43MenuPad;
            float cy = (top + bottom) * 0.5f, w = UnityTuning.MenuItemWidth + 2 * UnityTuning.Results43MenuPad + UnityTuning.Results43MenuExtraW;
            Image mp = UiKit43.Blade(r43MenuRt, "Panel", panel, mid, mid, new Vector2(MenuLeft(cy) - UnityTuning.Results43MenuPad + w * 0.5f, cy), new Vector2(w, top - bottom));
            UiKit43.Scan(mp.rectTransform, "Scan", UnityTuning.Ui43ScanAlpha);
            // behind the menu items (they are the results screen's children, built before this): the panel goes to the screen's back, over the backdrop
            r43MenuRt.SetParent(results.transform, true);
            r43MenuRt.SetSiblingIndex(1);
        }

        static void Pin43(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2? pivot = null)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot ?? anchor;
            rt.anchoredPosition = pos;
        }

        /// <summary>From OpenCampRes25 (after its texts are set): the backdrop's capture (this screen hidden this frame), the components filled.</summary>
        void Open43Results()
        {
            if (!Results43On) return;
            if (!r43Built) BuildResults43();
            CultureInfo ci = CultureInfo.InvariantCulture;
            r43Back.gameObject.SetActive(true);
            r43Back.texture = null;
            r43Back.color = UiBlur43.Tint(false);
            if (r43Sheet != null) r43Sheet.color = new Color(r43SheetWas.r, r43SheetWas.g, r43SheetWas.b, 0f);
            r43Hide.alpha = 0f; // the capture frame: the game alone
            Screens43Probe.ResultsBackdrop = false;
            Screens43Probe.ResultsEnterDone = -1;
            campRes25Head.fontSize = UnityTuning.Screens43HeroPx;
            // the old summary, ranks and rows: hidden (their texts and the reel mirrors stay set); the banked line moves into the layout as r43Bank
            campRes25Sum.gameObject.SetActive(false);
            campRes26G.gameObject.SetActive(false);
            foreach (CampRow25 row in campRows25) row.group.gameObject.SetActive(false);
            if (front33Banked != null) front33Banked.Alpha = 0f; // UiFront33 re-activates it on a change: hidden by its alpha

            var d = new CampaignSim25(game);
            Campaign24 c = game.sim != null ? game.sim.campaign : null;
            LevelRank26 rk = d.rank26;
            char overall = rk != null ? rk.aggregate : c != null ? c.OverallRank() : '\0';
            string hero = overall != '\0' ? overall.ToString() : "";
            r43Rank.text = hero;
            r43Rank.color = Grade43(overall);
            r43HeroRt.gameObject.SetActive(hero.Length > 0);
            Results43HeroNow = hero;
            int deaths = 0;
            if (c != null) foreach (int x in c.roomDeaths) deaths += x;
            double secs = rk != null ? rk.seconds : c != null ? c.TotalSeconds : 0;
            int s = (int)System.Math.Round(secs);
            string[] values =
            {
                (s / 60).ToString(ci) + ":" + (s % 60).ToString("00", ci),
                rk != null ? System.Math.Round(rk.killPct).ToString(ci) + "%" : "-",
                rk != null ? System.Math.Round(rk.styleScore).ToString("N0", ci) : "-",
                deaths.ToString(ci),
            };
            char[] grades = { rk != null ? rk.time : '\0', rk != null ? rk.kills : '\0', rk != null ? rk.style : '\0', '\0' };
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < r43Tiles.Count; i++)
            {
                r43Tiles[i].value.text = values[i];
                r43Tiles[i].grade.text = grades[i] != '\0' ? grades[i].ToString() : "";
                r43Tiles[i].grade.color = Grade43(grades[i]);
                sb.Append(r43Tiles[i].label.text).Append(' ').Append(values[i]).Append(' ').Append(r43Tiles[i].grade.text).Append(" | ");
            }
            Results43TilesNow = sb.ToString();
            int banked = Shop33.LastBanked, wallet = Shop33.Wallet;
            r43Bank.text = banked >= 0 ? "+" + banked.ToString("N0", ci) + " BANKED" + (wallet >= 0 ? "   WALLET " + wallet.ToString("N0", ci) : "") : "";
            r43BankRt.gameObject.SetActive(r43Bank.text.Length > 0);

            // the rooms: the rows there are, the panel to fit
            int n = 0;
            for (int i = 0; i < r43Rooms.Count; i++)
            {
                bool on = i < d.rows.Count;
                r43Rooms[i].rt.gameObject.SetActive(on);
                if (!on) continue;
                n++;
                r43Rooms[i].name.text = d.rows[i].name;
                r43Rooms[i].grade.text = d.rows[i].rank;
                r43Rooms[i].grade.color = Grade43(d.rows[i].rank.Length > 0 ? d.rows[i].rank[0] : '\0');
                r43Rooms[i].time.text = d.rows[i].time;
            }
            Results43RoomsNow = n;
            r43RoomsRt.gameObject.SetActive(n > 0);
            r43RoomsPanel.rectTransform.sizeDelta = new Vector2(UnityTuning.Results43RoomsW, n * UnityTuning.Results43RowPitch + 1.5f * UnityTuning.Screens43Pad);
            r43MenuRt.gameObject.SetActive(true);

            r43Age = 0;
            r43Settled = false;
            Tick43Results(0);
            game.StartCoroutine(UiBlur43.Capture(tex =>
            {
                if (r43Back == null) return;
                r43Back.texture = tex;
                r43Back.color = UiBlur43.Tint(tex != null);
                Screens43Probe.ResultsBackdrop = tex != null;
            }));
        }

        /// <summary>From RenderCampRes25 (every frame it shows): the screen back on after the capture frame, the staggered entrance, the hero's slam.</summary>
        void Tick43Results(double frameDt)
        {
            if (!r43Built || r43Age < 0) return;
            if (r43Age > 0 && r43Hide.alpha < 1f) r43Hide.alpha = 1f;
            r43Age += frameDt;
            if (r43Settled) return;
            float dx = -UnityTuning.Ui43EnterFrom * UiKit43.SlantTan, dy = -UnityTuning.Ui43EnterFrom;
            // the head slides as it fades (its own fade)
            float hk = UiTween.OutBack(campRes25HeadG.alpha);
            ((RectTransform)campRes25HeadG.transform).anchoredPosition = new Vector2(Mathf.Round(dx * (1 - hk)), Mathf.Round(dy * (1 - hk)));
            bool all = campRes25HeadG.alpha >= 1f;
            for (int i = 0; i < r43Groups.Count; i++)
            {
                double a = (r43Age - UnityTuning.Results43EnterAt - i * UnityTuning.Ui43EnterGap) / UnityTuning.Ui43EnterTime;
                float k = a <= 0 ? 0f : UiTween.OutBack(a);
                if (a < 1) all = false;
                r43Groups[i].anchoredPosition = new Vector2(Mathf.Round(dx * (1 - k)), Mathf.Round(dy * (1 - k)));
                r43GroupAlpha[i].alpha = UiTween.Clamp01(a);
            }
            // the hero grade slams (2x -> 1) as its panel lands
            double ra = (r43Age - UnityTuning.Results43EnterAt) / UnityTuning.ThemeSlamTime;
            float sc = ra <= 0 ? UnityTuning.ThemeSlamFrom : Mathf.Lerp(UnityTuning.ThemeSlamFrom, 1f, UiTween.OutCubic(ra));
            r43Rank.rectTransform.localScale = new Vector3(sc, sc, 1f);
            if (all)
            {
                r43Settled = true;
                Screens43Probe.ResultsEnterDone = r43Age;
            }
        }

        /// <summary>From EndCampRes25: the sheet and the old texts back for the survival RESULTS.</summary>
        void End43Results()
        {
            if (!r43Built) return;
            r43Age = -1;
            r43Back.gameObject.SetActive(false);
            r43MenuRt.gameObject.SetActive(false);
            if (r43Sheet != null) r43Sheet.color = r43SheetWas;
            r43Hide.alpha = 1f;
            campRes25Sum.gameObject.SetActive(true);
            campRes26G.gameObject.SetActive(true);
            if (front33Banked != null) front33Banked.Alpha = 1f;
            ((RectTransform)campRes25HeadG.transform).anchoredPosition = Vector2.zero;
        }
    }
}
