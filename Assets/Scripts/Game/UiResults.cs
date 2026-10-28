// NIGHT17 U3-30.8 (builder N): RESULTS as a hero tally (NIGHT12 U3-25.6's staged tally, rebuilt; the research: "lines arrive
// 200 ms apart, each counting up, any input completes it"; ULTRAKILL's rank slam). The lines (MenuRules.LineAt, 200 ms
// apart): the outcome (RUN OVER / EXFIL / EXFIL FAILED, DISPLAY S56); the ROUND (S133, the RANK face, a slam); the POINTS
// (S100, GOLD, counting up with UiCount, a punch when they land); the grade (S133 INK on its results.gradeColors plate,
// slanted, slamming 2x -> 1x in 100 ms with one flash through game.flashGate x FLASH INTENSITY) with the HEAT badge beside
// it; NEW BEST (a GOLD plate) over the personal-best line; then RETRY (R) / TITLE / DETAILS enter along the slant, RETRY
// focused a frame later (the press that completed the tally never restarts). Any key, click or pad button completes it.
// DETAILS swaps the hero row for the rest: the run's numbers in short lines, THE ROUTE (UiRoute.cs), the PAYOFF strip
// (UiPayoff.cs) and RETRY SAME SEED; developer buttons (COPY SCORE LINE, SAVE CARD, COPY SEED) only in a developer build.
// The port-only event MetaEvents.ResultsStage at each line (outcome, round, card 0, grade, best, done; a skip: done only).
// Texts are built once when RESULTS opens; the frame writes numbers (SetText with a number), alphas and scales.
using System;
using System.Globalization;
using System.Text;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        /// <summary>The HEAT badge RESULTS shows ("HEAT 1"), or null (no rung, or not on RESULTS). Automation reads it.</summary>
        public string ResultsHeatBadge { get; private set; }

        static string HeatBadgeText(int heat) => "HEAT " + heat.ToString(CultureInfo.InvariantCulture);

        RectTransform tallyRoot;
        CanvasGroup tallyOutcomeG, tallyRoundG, tallyPointsG, tallyGradeG, tallyBestG, tallyDetailsG;
        TextMeshProUGUI tallyOutcome, tallyRound, tallyPoints, tallyGrade, tallyHeat, tallyBestText, tallyDetails;
        UiShape tallyHeatPlate, tallyStampPlate, tallyGradePlate;
        readonly Vector2[] tallyHomes = new Vector2[8]; // NIGHT17 round 1: each line's rest, for its slide in along the slant
        CanvasGroup tallyCardG, tallyCardFootG; // NIGHT17 U3-30.10: the RESULTS card (its frame; its foot)
        UnityEngine.UI.RawImage tallyCardMap;
        TextMeshProUGUI tallyCardBest, tallyCardStamp;
        UnityEngine.UI.Image tallyFlash;
        RectTransform tallyGradeRt, tallyRoundRt, tallyPointsRt, tallyStamp;
        MenuItemView resultsDetailsItem;
        bool resultsDetailsOpen;
        readonly StringBuilder tallySb = new StringBuilder(512);
        readonly ResultsStageEvent tallyEvent = new ResultsStageEvent();
        double tallyT, tallyPointsTarget;
        int tallyLine, tallyFocusFrame = -1;
        bool tallyDone, tallyNewBest, tallyFlashed;
        bool tallyFlashOk18; // NIGHT18: the gate's answer for this tally's one flash (kept, not just for its first frame)
        UiShape tallyFlash18;
        long tallyPointsShown = long.MinValue;
        UiCount tallyCount;
        UiSlam tallyRoundSlam, tallyGradeSlam;
        UiPunch tallyPointsPunch;
        readonly float[] tallyShown = new float[8];

        // The lines in order: outcome, round, points, grade, best, done.
        static readonly string[] TallyStages = { "outcome", "round", "card", "grade", "best", "done" };

        CanvasGroup TallyGroup(string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(tallyRoot, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var g = go.GetComponent<CanvasGroup>();
            g.alpha = 0;
            g.interactable = false;
            g.blocksRaycasts = false;
            return g;
        }

        TextMeshProUGUI TallyText(Transform parent, string name, string s, UiTheme.Size size, Color c, TMP_FontAsset face, TextAlignmentOptions align, Vector2 pos, Vector2 box, bool shadow = false)
        {
            float px = UiTheme.Px(size);
            TextMeshProUGUI t = Text(parent, name, s, px, c, align, pos, box, new Vector2(0.5f, 0.5f));
            UiTheme.Style(t, px, c, face, shadow);
            t.fontStyle = FontStyles.Normal;
            return t;
        }

        UiShape TallyPlate(Transform parent, string name, Color c, Vector2 pos, Vector2 size)
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

        void BuildTally()
        {
            tallyRoot = new GameObject("Tally", typeof(RectTransform)).GetComponent<RectTransform>();
            tallyRoot.SetParent(results.transform, false);
            tallyRoot.anchorMin = tallyRoot.anchorMax = new Vector2(0.5f, 0.5f);
            tallyRoot.sizeDelta = Vector2.zero;
            BuildCardFrame(); // NIGHT17 U3-30.10: behind the row
            float big = UiTheme.Px(UiTheme.Size.S133), hy = UnityTuning.ResultsHeroY;
            float L = MenuLeft(hy); // the tally row's left edge on the column (NIGHT17 round 1: nothing left of the safe inset)
            float lg = UnityTuning.ResultsHeroLabelGap;

            // The outcome, by its left edge on the column
            float oy = UnityTuning.ResultsHeadY, ow = 900;
            tallyOutcomeG = TallyGroup("Outcome", new Vector2(MenuLeft(oy) + ow * 0.5f, oy), new Vector2(ow, 80));
            tallyOutcome = TallyText(tallyOutcomeG.transform, "Text", "RUN OVER", UiTheme.Size.S56, UiTheme.White, UiTheme.Display, TextAlignmentOptions.Left, Vector2.zero, new Vector2(ow, 80), true);

            // One row on the slab: ROUND (the RANK face), POINTS (gold: points only, counting up), GRADE (its letter in its
            // results.gradeColors colour, on the INK slab: no pale plate over the world), each under a muted S24 label.
            float rw = UnityTuning.ResultsPointsFromLeft - 20;
            tallyRoundG = TallyGroup("Round", new Vector2(L + rw * 0.5f, hy), new Vector2(rw, big + 40));
            TallyText(tallyRoundG.transform, "Label", "ROUND", UiTheme.Size.S24, UiTheme.Muted, UiTheme.BodyBold, TextAlignmentOptions.Left, new Vector2(0, lg), new Vector2(rw, 34)).characterSpacing = UnityTuning.ThemeLabelTracking;
            tallyRound = TallyText(tallyRoundG.transform, "Number", "0", UiTheme.Size.S133, UiTheme.White, UiTheme.Rank, TextAlignmentOptions.Left, Vector2.zero, new Vector2(rw, big + 10), true);
            tallyRoundRt = tallyRound.rectTransform;

            float pw = UnityTuning.ResultsGradeFromLeft - UnityTuning.ResultsPointsFromLeft - 20;
            float pl = L + UnityTuning.ResultsPointsFromLeft;
            tallyPointsG = TallyGroup("Points", new Vector2(pl + pw * 0.5f, hy), new Vector2(pw, big + 40));
            TallyText(tallyPointsG.transform, "Label", "POINTS", UiTheme.Size.S24, UiTheme.Muted, UiTheme.BodyBold, TextAlignmentOptions.Left, new Vector2(0, lg), new Vector2(pw, 34)).characterSpacing = UnityTuning.ThemeLabelTracking;
            tallyPoints = TallyText(tallyPointsG.transform, "Number", "0", UiTheme.Size.S100, UiTheme.Gold, UiTheme.Display, TextAlignmentOptions.Left, Vector2.zero, new Vector2(pw, big), true);
            tallyPointsRt = tallyPoints.rectTransform;

            float gw = UnityTuning.ResultsGradeWidth, gl = L + UnityTuning.ResultsGradeFromLeft;
            tallyGradeG = TallyGroup("Grade", new Vector2(gl + gw * 0.5f, hy), new Vector2(gw, big + 20));
            tallyGradeRt = (RectTransform)tallyGradeG.transform;
            TallyText(tallyGradeRt, "Label", "GRADE", UiTheme.Size.S24, UiTheme.Muted, UiTheme.BodyBold, TextAlignmentOptions.Left, new Vector2(0, lg), new Vector2(gw, 34)).characterSpacing = UnityTuning.ThemeLabelTracking;
            // Round 3 (the reviewer: the grade was the dullest thing on screen): the letter in INK on its own plate in the grade's
            // colour, a square at the hero size, slanted and cut, slammed with the group.
            float gp = UnityTuning.ResultsGradePlate;
            tallyGradePlate = TallyPlate(tallyGradeRt, "Plate", UiTheme.Gold, new Vector2(-gw * 0.5f + gp * 0.5f, 0), new Vector2(gp, gp));
            tallyGradePlate.SetCuts(UnityTuning.ThemeCut * 2, UnityTuning.ThemeCut * 2);
            // NIGHT18: the grade's flash on its own plate (a white-hot copy behind the letter, shrinking onto it as it fades;
            // FeelAB off: NIGHT17's full-screen sheet, tallyFlash)
            tallyFlash18 = TallyPlate(tallyGradeRt, "Grade flash (plate)", UiTheme.A(UiTheme.White, 0), new Vector2(-gw * 0.5f + gp * 0.5f, 0), new Vector2(gp, gp));
            tallyFlash18.SetCuts(UnityTuning.ThemeCut * 2, UnityTuning.ThemeCut * 2);
            tallyFlash18.enabled = false;
            tallyGrade = TallyText(tallyGradeRt, "Letter", "", UiTheme.Size.S133, UiTheme.InkSolid, UiTheme.Display, TextAlignmentOptions.Center, new Vector2(-gw * 0.5f + gp * 0.5f, 0), new Vector2(gp, big + 10));
            tallyHeatPlate = TallyPlate(tallyGradeRt, "Heat", UiTheme.Danger, new Vector2(-gw * 0.5f + UnityTuning.TallyHeatWidth * 0.5f, -UnityTuning.ResultsGradePlate * 0.5f - UnityTuning.TallyHeatHeight * 0.5f - 10), new Vector2(UnityTuning.TallyHeatWidth, UnityTuning.TallyHeatHeight));
            tallyHeat = TallyText(tallyHeatPlate.transform, "Text", "", UiTheme.Size.S24, UiTheme.InkSolid, UiTheme.BodyBold, TextAlignmentOptions.Center, Vector2.zero, new Vector2(UnityTuning.TallyHeatWidth, UnityTuning.TallyHeatHeight));

            // NEW BEST: the gold plate alone under POINTS (the personal-best line only when it isn't a new best)
            float by = UnityTuning.ResultsBestY, bw = 700;
            tallyBestG = TallyGroup("Best", new Vector2(pl + bw * 0.5f, by), new Vector2(bw, 60));
            // Round 5 (the reviewer: a plate the buttons' shape reads as clickable): a smaller gold stamp, upright-cut and tilted
            // -8 deg, laid over the points' top (TallyColumns places it on the number).
            var stampSize = new Vector2(UnityTuning.ResultsStampWidth, UnityTuning.ResultsStampHeight);
            tallyStampPlate = TallyPlate(tallyBestG.transform, "Stamp", UiTheme.Gold, Vector2.zero, stampSize);
            tallyStampPlate.Slant = 0;
            tallyStampPlate.SetCuts(UnityTuning.ThemeCut * 0.6f, UnityTuning.ThemeCut * 0.6f);
            tallyStamp = (RectTransform)tallyStampPlate.transform;
            tallyStamp.localRotation = Quaternion.Euler(0, 0, UnityTuning.TallyStampTiltDeg);
            TallyText(tallyStamp, "Text", "NEW BEST", UiTheme.Size.S24, UiTheme.InkSolid, UiTheme.BodyBold, TextAlignmentOptions.Center, Vector2.zero, stampSize).characterSpacing = UnityTuning.ThemeLabelTracking;
            tallyBestText = TallyText(tallyBestG.transform, "Line", "", UiTheme.Size.S24, UiTheme.Muted, UiTheme.BodyBold, TextAlignmentOptions.Left, Vector2.zero, new Vector2(bw, 34));
            tallyHomes[0] = ((RectTransform)tallyOutcomeG.transform).anchoredPosition;
            tallyHomes[1] = ((RectTransform)tallyRoundG.transform).anchoredPosition;
            tallyHomes[2] = ((RectTransform)tallyPointsG.transform).anchoredPosition;
            tallyHomes[4] = ((RectTransform)tallyBestG.transform).anchoredPosition;
            BuildCardFoot(); // NIGHT17 U3-30.10: over the row's right and along its foot
            tallyHomes[5] = ((RectTransform)tallyCardFootG.transform).anchoredPosition; // NIGHT18: the foot slides in too

            // DETAILS: the rest, in short lines (shown instead of the hero row), by its top-left on the column
            var dg = new GameObject("Details", typeof(RectTransform), typeof(CanvasGroup));
            var drt = (RectTransform)dg.transform;
            drt.SetParent(results.transform, false);
            drt.anchorMin = drt.anchorMax = new Vector2(0.5f, 0.5f);
            drt.pivot = new Vector2(0, 1);
            drt.anchoredPosition = new Vector2(MenuLeft(UnityTuning.ResultsDetailsTop), UnityTuning.ResultsDetailsTop);
            drt.sizeDelta = new Vector2(UnityTuning.ResultsDetailsWidth, UnityTuning.ResultsDetailsHeight);
            tallyDetailsG = dg.GetComponent<CanvasGroup>();
            tallyDetailsG.interactable = false;
            tallyDetailsG.blocksRaycasts = false;
            tallyDetails = Text(drt, "Text", "", UiTheme.Px(UiTheme.Size.S24), UiTheme.White, TextAlignmentOptions.TopLeft, Vector2.zero, new Vector2(UnityTuning.ResultsDetailsWidth, UnityTuning.ResultsDetailsHeight), new Vector2(0.5f, 0.5f));
            UiTheme.Style(tallyDetails, UiTheme.Px(UiTheme.Size.S24), UiTheme.White, UiTheme.BodyBold);
            tallyDetails.textWrappingMode = TextWrappingModes.Normal;
            tallyDetails.lineSpacing = UnityTuning.TallyDetailsLineSpacing;
            tallyDetails.richText = true;
            dg.SetActive(false);

            // The grade's flash (one, through game.flashGate x FLASH INTENSITY), over the screen, taking no clicks
            tallyFlash = Img(results.transform, "Grade flash", UiTheme.A(UiTheme.White, 0), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            tallyFlash.enabled = false;
        }

        // --- NIGHT17 U3-30.10 (beyond the ask): the RESULTS card, the frame a player would post ---------------------------------
        // One slanted INK card round the tally row (UiShape on the theme's 12 deg, its corners cut), a CYAN accent bar down its
        // left, the wordmark small top-right, THE ROUTE's minimap (RouteMap's texture, drawn by UiRoute when RESULTS opens),
        // the best moment ("BEST +480 · TRIPLE": the points gold) and the date / seed along its foot. Inside the 56 px inset at
        // 16:9 and 16:10 (MenusTests). DETAILS swaps the whole card for the rest (the tally root hides).

        /// <summary>The card's frame (behind the row): the INK plate and the accent bar, one group fading and sliding in.</summary>
        void BuildCardFrame()
        {
            float top = UnityTuning.ResultsCardTop, bottom = UnityTuning.ResultsCardBottom;
            float left = MenuRules.ResultsCardLeft(), right = UnityTuning.ResultsCardRight;
            float cy = (top + bottom) * 0.5f, h = top - bottom;
            tallyCardG = TallyGroup("Card", new Vector2((left + right) * 0.5f, cy), new Vector2(right - left, h));
            UiShape plate = TallyPlate(tallyCardG.transform, "Plate", UiTheme.InkSolid, Vector2.zero, new Vector2(right - left, h));
            plate.SetCuts(UnityTuning.ThemeCut * 2, UnityTuning.ThemeCut * 2);
            UiShape bar = TallyPlate(tallyCardG.transform, "Accent", UiTheme.Cyan, new Vector2(-(right - left) * 0.5f + UnityTuning.ResultsCardAccent * 0.5f, 0), new Vector2(UnityTuning.ResultsCardAccent, h));
            bar.SetCuts(0, 0);
            tallyHomes[6] = ((RectTransform)tallyCardG.transform).anchoredPosition;
        }

        /// <summary>The card's foot: the wordmark, the minimap, the best moment and the stamp (one group, with the best line).</summary>
        void BuildCardFoot()
        {
            float top = UnityTuning.ResultsCardTop, bottom = UnityTuning.ResultsCardBottom;
            float cy = (top + bottom) * 0.5f;
            tallyCardFootG = TallyGroup("Card foot", Vector2.zero, Vector2.zero);
            Transform foot = tallyCardFootG.transform;
            float mapRight = UnityTuning.ResultsMapX + UnityTuning.ResultsMapSize * 0.5f;
            var mark = TallyText(foot, "Wordmark", "SKYGRIND", UiTheme.Size.S32, UiTheme.Cyan, UiTheme.Display, TextAlignmentOptions.Right, Vector2.zero, new Vector2(400, 44));
            mark.rectTransform.pivot = new Vector2(1, 0.5f);
            mark.rectTransform.anchoredPosition = new Vector2(mapRight, UnityTuning.ResultsCardFootY); // round 2: the foot's right (the bigger map takes the top)
            var mapGo = new GameObject("Minimap", typeof(RectTransform), typeof(UnityEngine.UI.RawImage));
            var mrt = (RectTransform)mapGo.transform;
            mrt.SetParent(foot, false);
            mrt.anchorMin = mrt.anchorMax = new Vector2(0.5f, 0.5f);
            mrt.anchoredPosition = new Vector2(UnityTuning.ResultsMapX, UnityTuning.ResultsMapY);
            mrt.sizeDelta = new Vector2(UnityTuning.ResultsMapSize, UnityTuning.ResultsMapSize);
            tallyCardMap = mapGo.GetComponent<UnityEngine.UI.RawImage>();
            tallyCardMap.raycastTarget = false;
            // the foot's line: the best moment from the card's left (on its slant) and the stamp right-aligned under the map
            float fy = UnityTuning.ResultsCardFootY;
            float leftAtFoot = MenuRules.ResultsCardLeft() + MenuSlantK * (fy - cy) + UnityTuning.ResultsCardAccent + 26;
            tallyCardBest = TallyText(foot, "Best", "", UiTheme.Size.S24, UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Left, Vector2.zero, new Vector2(700, 34));
            tallyCardBest.richText = true;
            MenuPinLeft(tallyCardBest, leftAtFoot, fy);
            tallyCardStamp = TallyText(foot, "Stamp", "", UiTheme.Size.S18, UiTheme.Muted, UiTheme.BodyBold, TextAlignmentOptions.Right, Vector2.zero, new Vector2(520, 30));
            tallyCardStamp.rectTransform.pivot = new Vector2(1, 0.5f);
            tallyCardStamp.rectTransform.anchoredPosition = new Vector2(mapRight - mark.GetPreferredValues("SKYGRIND").x - 20, fy); // left of the wordmark
            tallyCardStamp.characterSpacing = UnityTuning.ThemeLabelTracking;
        }

        /// <summary>RESULTS opened: the card's map (THE ROUTE's texture, or none), its best moment and its stamp (strings once).</summary>
        void CardFill(RunStats s)
        {
            Texture tex = routeMap != null ? routeMap.texture : null;
            tallyCardMap.texture = tex;
            tallyCardMap.enabled = tex != null;
            string best = MenuRules.BestMoment(s.bestAwardName, s.bestAwardPoints, s.topSpeed);
            if (s.bestAwardName != null)
            {
                string pts = "+" + Math.Round(s.bestAwardPoints).ToString(CultureInfo.InvariantCulture);
                best = best.Replace(pts, "<color=#" + UiTheme.GoldHex + ">" + pts + "</color>"); // gold: points only
            }
            tallyCardBest.text = best;
            tallyCardStamp.text = MenuRules.CardStamp(DateTime.Now, s.seedCode);
        }

        /// <summary>RESULTS' buttons on or off (off while the tally runs); the developer's only in a developer build.</summary>
        void MenuResultsButtons(bool on)
        {
            SetActiveIf(restartButton, on);
            SetActiveIf(titleButton, on);
            SetActiveIf(resultsDetailsItem.go, on);
            SetActiveIf(retryButton, on && resultsDetailsOpen);
            SetActiveIf(copyButton, on && game.devMode);
        }

        static void SetActiveIf(GameObject go, bool on)
        {
            if (go != null && go.activeSelf != on) go.SetActive(on);
        }

        /// <summary>DETAILS / SUMMARY: the hero row or the rest (the numbers, THE ROUTE, the PAYOFF strip, RETRY SAME SEED).</summary>
        void ToggleResultsDetails()
        {
            resultsDetailsOpen = !resultsDetailsOpen;
            ApplyResultsDetails();
            if (resultsDetailsOpen) MenuEnterItem(menuItemOf[retryButton], 0);
        }

        /// <summary>NIGHT19 U3-33.3: LB / RB on RESULTS (once the tally is done and DETAILS is offered): SUMMARY and DETAILS
        /// are two tabs, either bumper switches (wrapping), the new one sliding in from the bumper's side.</summary>
        void ResultsTab19(int dir)
        {
            if (!tallyDone || resultsDetailsItem.go == null || !resultsDetailsItem.go.activeInHierarchy) return;
            ToggleResultsDetails();
            TabSlideStart19(resultsDetailsOpen ? (RectTransform)tallyDetailsG.transform : tallyRoot, dir);
        }

        void ApplyResultsDetails()
        {
            bool d = resultsDetailsOpen;
            resultsDetailsItem.label.text = d ? UnityTuning.ResultsSummaryLabel : UnityTuning.ResultsDetailsLabel;
            tallyRoot.gameObject.SetActive(!d && game.lastStats != null);
            tallyDetailsG.gameObject.SetActive(d);
            if (payoffPanel != null && payoffPanel.parent == results.transform) payoffPanel.gameObject.SetActive(d && payoffWanted);
            SetActiveIf(retryButton, d && tallyDone);
        }

        /// <summary>RESULTS opened on new stats: fill every text once and start the tally from its first line.</summary>
        void BeginTally(RunStats s)
        {
            tallyT = 0;
            tallyLine = 0;
            tallyDone = false;
            tallyFlashed = false;
            tallyLead18 = FeelAB.On ? UnityTuning.ResultsCardLead18 : 0; // NIGHT18: the card first, then its lines
            tallyFocusFrame = -1;
            for (int i = 0; i < tallyShown.Length; i++) tallyShown[i] = -1;
            resultsDetailsOpen = false;
            MenuResultsButtons(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            tallyRoot.gameObject.SetActive(true);
            tallyDetailsG.gameObject.SetActive(false);
            ApplyResultsDetails();

            tallyOutcome.text = s.exfil ?? "RUN OVER";
            tallyOutcome.color = s.exfil == "EXFIL" ? UiTheme.Cyan : UiTheme.White;
            tallyRound.SetText("{0:0}", s.round);

            bool hasGrade = s.grade != null;
            tallyGradeRt.gameObject.SetActive(hasGrade || s.heat > 0);
            tallyGrade.gameObject.SetActive(hasGrade);
            tallyGradePlate.enabled = hasGrade;
            if (hasGrade)
            {
                tallyGrade.text = s.grade;
                Color c = UiTheme.Gold; // "P" (design.grade.p): the theme's gold
                if (gradeHex.TryGetValue(s.grade, out string gh) && ColorUtility.TryParseHtmlString("#" + gh, out Color gc)) c = gc;
                tallyGradePlate.color = c; // the plate in the grade's colour, the letter INK on it (round 3)
            }
            ResultsHeatBadge = s.heat > 0 ? HeatBadgeText(s.heat) : null;
            tallyHeatPlate.gameObject.SetActive(ResultsHeatBadge != null);
            if (ResultsHeatBadge != null)
                tallyHeat.text = s.scoreMult != 1 ? ResultsHeatBadge + "  x" + s.scoreMult.ToString("0.##", CultureInfo.InvariantCulture) : ResultsHeatBadge;

            tallyPointsTarget = Math.Round(s.earned);
            tallyCount.Snap(0);
            tallyPointsShown = long.MinValue;
            tallyPoints.SetText("{0:0}", 0);
            TallyColumns(s); // round 3: even gaps between ROUND, POINTS and GRADE (by their numbers' widths)

            tallyNewBest = game.lastPb.newBest;
            tallyStamp.gameObject.SetActive(tallyNewBest);
            tallyBestText.text = tallyNewBest ? "" : game.lastPb.text ?? ""; // NIGHT17 round 1: the plate says NEW BEST once (no line under it)
            if (assist20 != null && assist20.Stamp != null) tallyBestText.text = assist20.Stamp + UnityTuning.AssistResultsTail; // NIGHT20 E2: the ASSIST run's stamp
            else if (game.sim.grindOn && game.sim.lastGrindStats != null) // NIGHT21 U3-35.7 (UNCOMPILED): THE GRIND's wave, its records line, this HEAT's best wave
            {
                EndlessStats gs = game.sim.lastGrindStats;
                Records21.Judgement? gj = game.sim.lastGrindJudgement;
                tallyNewBest = gj != null && gj.Value.newBest && !gj.Value.first;
                tallyStamp.gameObject.SetActive(tallyNewBest);
                tallyBestText.text = UnityTuning.GrindResultsWave + gs.lastWave + " · " + (gj != null ? gj.Value.text : "") + " · " + UnityTuning.GrindResultsBest + game.sim.grindRecords.BestWave(s.heat);
            }
            tallyBestText.color = UiTheme.Muted;
            CardFill(s); // NIGHT17 U3-30.10: the card's map, best moment and stamp

            // DETAILS: the rest of menus.js renderSurvival's rows, one short line each
            var span = TimeSpan.FromSeconds(s.time);
            string hex = UiTheme.MutedHex;
            tallySb.Clear();
            tallySb.Append("<color=#").Append(hex).Append(">KILLS</color>  ").Append(s.kills)
                .Append("   <color=#").Append(hex).Append(">HEADSHOTS</color>  ").Append(s.headshots)
                .Append("   <color=#").Append(hex).Append(">ACCURACY</color>  ").Append(Math.Round(s.Accuracy * 100).ToString(CultureInfo.InvariantCulture)).Append('%');
            tallySb.Append("\n<color=#").Append(hex).Append(">TIME</color>  ").Append((int)span.TotalMinutes).Append(':').Append(span.Seconds.ToString("00", CultureInfo.InvariantCulture))
                .Append("   <color=#").Append(hex).Append(">STYLE</color>  ").Append(Math.Round(s.style).ToString(CultureInfo.InvariantCulture))
                .Append("   <color=#").Append(hex).Append(">BEST RANK</color>  ").Append(s.bestRank);
            if (s.killedBy != null) tallySb.Append("\n<color=#").Append(UiTheme.DangerHex).Append(">KILLED BY ").Append(s.killedBy.ToUpperInvariant()).Append("</color>");
            if (s.zones.Count > 0) tallySb.Append("\n<color=#").Append(hex).Append(">OPENED</color>  ").Append(string.Join(", ", s.zones));
            if (s.forged.Count > 0) tallySb.Append("\n<color=#").Append(hex).Append(">FORGED</color>  ").Append(string.Join(", ", s.forged));
            if (s.hasProtocol) tallySb.Append("\n<color=#").Append(hex).Append(">PROTOCOL</color>  ").Append(s.protocolComplete ? "COMPLETE" : s.protocolLit + " / " + s.protocolTotal);
            if (s.grade != null)
            {
                if (s.bestAwardName != null) tallySb.Append("\n<color=#").Append(hex).Append(">BIGGEST</color>  ").Append(s.bestAwardName).Append(" +").Append(s.bestAwardPoints.ToString("0", CultureInfo.InvariantCulture));
                tallySb.Append("\n<color=#").Append(hex).Append(">TOP SPEED</color>  ").Append(s.topSpeed.ToString(CultureInfo.InvariantCulture)).Append(" M/S")
                    .Append("   <color=#").Append(hex).Append(">MULTI-KILL</color>  ").Append(s.bestMulti);
                if (s.longestSwing > 0) tallySb.Append("   <color=#").Append(hex).Append(">SWING</color>  ").Append(s.longestSwing.ToString(CultureInfo.InvariantCulture)).Append(" M");
            }
            if (s.seedCode != null) tallySb.Append("\n<color=#").Append(hex).Append(">SEED</color>  ").Append(s.seedCode);
            if (game.devMode && s.line != null) tallySb.Append("\n<color=#").Append(hex).Append('>').Append(s.line).Append("</color>"); // the raw score line: a developer's
            tallyDetails.SetText(tallySb);
        }

        /// <summary>
        /// Round 3 (the reviewer: gaps of 230 and 390 px): ROUND, POINTS and GRADE one even gap apart (ResultsColumnGap) by
        /// their contents' widths (the round, the points' final value, each label), from the column's left edge. Once a run.
        /// </summary>
        void TallyColumns(RunStats s)
        {
            float L = MenuLeft(UnityTuning.ResultsHeroY), gap = UnityTuning.ResultsColumnGap, label = UnityTuning.ResultsLabelWidth;
            float roundW = Mathf.Max(label, tallyRound.GetPreferredValues(s.round.ToString(CultureInfo.InvariantCulture)).x);
            float pointsW = Mathf.Max(label, tallyPoints.GetPreferredValues(Math.Round(s.earned).ToString(CultureInfo.InvariantCulture)).x);
            float pointsLeft = L + roundW + gap, gradeLeft = pointsLeft + pointsW + gap;
            if (game.sim != null && game.sim.n21.stamp)
            {
                // NIGHT22 U3-36.4 / T-11 (open since w18: the grade card over POINTS): the ROUND and POINTS boxes were fixed widths
                // (210 / 370 px) while the grade moved by the numbers' widths, so a short number left its box under the grade card.
                // With design.n21.stamp on each box is exactly its content wide (left edges unchanged), so the rects end a gap apart.
                TallyFit22(tallyRoundG, roundW);
                tallyHomes[1] = new Vector2(L + roundW * 0.5f, tallyHomes[1].y);
                ((RectTransform)tallyRoundG.transform).anchoredPosition = tallyHomes[1];
                TallyFit22(tallyPointsG, pointsW);
            }
            var prt = (RectTransform)tallyPointsG.transform;
            tallyHomes[2] = new Vector2(pointsLeft + prt.sizeDelta.x * 0.5f, tallyHomes[2].y);
            prt.anchoredPosition = tallyHomes[2];
            var brt = (RectTransform)tallyBestG.transform;
            tallyHomes[4] = new Vector2(pointsLeft + brt.sizeDelta.x * 0.5f, tallyHomes[4].y);
            brt.anchoredPosition = tallyHomes[4];
            // round 5: the stamp over the points' top-right (its centre on the number's upper edge), in the best group's space
            var stampAt = new Vector2(pointsLeft + pointsW - UnityTuning.ResultsStampWidth * 0.3f, UnityTuning.ResultsHeroY + UnityTuning.ResultsStampRise);
            tallyStamp.anchoredPosition = stampAt - tallyHomes[4];
            tallyGradeRt.anchoredPosition = new Vector2(gradeLeft + tallyGradeRt.sizeDelta.x * 0.5f, tallyGradeRt.anchoredPosition.y);
        }

        /// <summary>NIGHT22 T-11: a tally group and its label / number boxes `w` px wide (their left edges stay where they were: the group's centre moves by the caller).</summary>
        static void TallyFit22(CanvasGroup group, float w)
        {
            var rt = (RectTransform)group.transform;
            rt.sizeDelta = new Vector2(w, rt.sizeDelta.y);
            for (int i = 0; i < rt.childCount; i++)
            {
                var c = rt.GetChild(i) as RectTransform;
                if (c != null) c.sizeDelta = new Vector2(w, c.sizeDelta.y);
            }
        }

        UnityEngine.UI.Graphic[] results22Grade, results22Points;

        /// <summary>
        /// NIGHT22 T-11 (the Player reel's `stamp` reads it every RESULTS frame): the largest overlap, in screen px squared, between a visible
        /// graphic of the GRADE card (plate, flash, letter, label, HEAT badge) and a visible graphic of POINTS (label, number), the way the
        /// probe's ui[] rects see them. 0 means the grade card is off POINTS.
        /// </summary>
        public float ResultsGradeOverPoints22()
        {
            if (tallyGradeRt == null || tallyPointsG == null) return -1;
            results22Grade ??= tallyGradeRt.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            results22Points ??= tallyPointsG.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            float worst = 0;
            foreach (UnityEngine.UI.Graphic a in results22Grade)
            {
                if (!Visible22(a)) continue;
                FeelProbe.UiScreenRect(a.rectTransform, out Rect ra);
                foreach (UnityEngine.UI.Graphic b in results22Points)
                {
                    if (!Visible22(b)) continue;
                    FeelProbe.UiScreenRect(b.rectTransform, out Rect rb);
                    float w = Mathf.Min(ra.xMax, rb.xMax) - Mathf.Max(ra.xMin, rb.xMin);
                    float h = Mathf.Min(ra.yMax, rb.yMax) - Mathf.Max(ra.yMin, rb.yMin);
                    if (w > 0 && h > 0) worst = Mathf.Max(worst, w * h);
                }
            }
            return worst;
        }

        static bool Visible22(UnityEngine.UI.Graphic g) =>
            g != null && g.enabled && g.gameObject.activeInHierarchy && g.canvasRenderer.GetInheritedAlpha() > 0.01f;

        void EmitStage(string stage, int index)
        {
            tallyEvent.stage = stage;
            tallyEvent.index = index;
            game.events.Emit(MetaEvents.ResultsStage, tallyEvent);
        }

        /// <summary>Frame time: the lines (events), each group's fade, the points' count and punch, the slams, the flash.</summary>
        void RenderTally(double frameDt)
        {
            int last = TallyStages.Length - 1;
            if (!tallyDone)
            {
                tallyT += frameDt;
                if (tallyT >= UnityTuning.TallySkipGuard && SkipPressed())
                {
                    tallyT = TallyAt(last) + 1; // everything at rest
                    tallyLine = last;
                    tallyCount.Snap(tallyPointsTarget);
                }
                while (tallyLine < TallyStages.Length && tallyT >= TallyAt(tallyLine))
                {
                    int line = tallyLine++;
                    if (line == 4 && !tallyNewBest && string.IsNullOrEmpty(tallyBestText.text)) continue; // no best line: no beat
                    if (line == 1) tallyRoundSlam.Hit(UnityTuning.ResultsRoundSlamFrom, UnityTuning.ThemeSlamTime * 1.5);
                    if (line == 2) tallyCount.Set(tallyPointsTarget);
                    if (line == 3) tallyGradeSlam.Hit(UnityTuning.ThemeSlamFrom, UnityTuning.Pick(FeelAB.On, UnityTuning.ResultsGradeSlam18, UnityTuning.ResultsGradeSlamN17)); // NIGHT18: settles past M9's 90 ms
                    EmitStage(TallyStages[line], 0);
                }
                if (tallyLine >= TallyStages.Length)
                {
                    tallyDone = true;
                    MenuResultsButtons(true);
                    int n = 0;
                    foreach (GameObject b in new[] { restartButton, titleButton, resultsDetailsItem.go, copyButton })
                        if (b.activeSelf) MenuEnterItem(menuItemOf[b], n++);
                    tallyFocusFrame = Time.frameCount + 1; // focus a frame later: the press that completed it never restarts
                }
            }
            else tallyT += frameDt;
            if (tallyFocusFrame >= 0 && Time.frameCount >= tallyFocusFrame)
            {
                tallyFocusFrame = -1;
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(restartButton); // RETRY focused
            }
            if (resultsDetailsOpen) return;

            double t = tallyT;
            SetGroupAlpha(tallyOutcomeG, 0, Fade(t, TallyAt(0)));
            SetGroupAlpha(tallyRoundG, 1, Fade(t, TallyAt(1)));
            SetGroupAlpha(tallyPointsG, 2, Fade(t, TallyAt(2)));
            SetGroupAlpha(tallyGradeG, 3, t >= TallyAt(3) ? 1 : 0);
            SetGroupAlpha(tallyBestG, 4, Fade(t, TallyAt(4)));
            // NIGHT17 round 1: each line slides in along the slant as it fades (ease-out-back over ThemeInTime)
            TallySlide(tallyOutcomeG, 0, t, TallyAt(0));
            TallySlide(tallyRoundG, 1, t, TallyAt(1));
            TallySlide(tallyPointsG, 2, t, TallyAt(2));
            TallySlide(tallyBestG, 4, t, TallyAt(4));
            // NIGHT17 U3-30.10: the card frames the row from the first line; its foot (the map, the best moment, the stamp) with the best line
            bool feel = FeelAB.On;
            if (tallyLead18 > 0)
            {
                // NIGHT18: the card's slab first, from MenuSlabEnter px down-left along the slant; its lines ResultsCardLead18 s later
                SetGroupAlpha(tallyCardG, 6, Fade(t, 0));
                TallySlideFar18(tallyCardG, 6, t);
            }
            else
            {
                SetGroupAlpha(tallyCardG, 6, Fade(t, TallyAt(0)));
                TallySlide(tallyCardG, 6, t, TallyAt(0));
            }
            // NIGHT18: the foot fades over ResultsFootFade18 and slides in along the slant (NIGHT17: an 83 ms fade in place)
            SetGroupAlpha(tallyCardFootG, 5, UiTween.OutCubic((t - TallyAt(4)) / UnityTuning.Pick(feel, UnityTuning.ResultsFootFade18, UnityTuning.ResultsFootFadeN17)));
            if (feel) TallySlide(tallyCardFootG, 5, t, TallyAt(4));
            float rs = tallyRoundSlam.Step(frameDt);
            if (tallyRoundRt.localScale.x != rs) tallyRoundRt.localScale = new Vector3(rs, rs, 1);
            long v = (long)Math.Round(tallyCount.Step(frameDt));
            if (v != tallyPointsShown)
            {
                tallyPointsShown = v;
                tallyPoints.SetText("{0:0}", v);
                if (!tallyCount.Moving && v == (long)tallyPointsTarget && t > TallyAt(2)) tallyPointsPunch.Kick(force: true); // the count lands
            }
            float ps = tallyPointsPunch.Step(frameDt);
            if (tallyPointsRt.localScale.x != ps) tallyPointsRt.localScale = new Vector3(ps, ps, 1);
            float gs = t >= TallyAt(3) ? tallyGradeSlam.Step(frameDt) : UnityTuning.ThemeSlamFrom;
            if (tallyGradeRt.localScale.x != gs) tallyGradeRt.localScale = new Vector3(gs, gs, 1);
            // The grade's one flash, through the flash gate (at most 3 a second) and FLASH INTENSITY.
            double g = t - TallyAt(3);
            if (g >= 0 && !tallyFlashed)
            {
                tallyFlashed = true;
                tallyFlashOk18 = game.settings.Num("flashIntensity") > 0 && game.flashGate.TryFlash(SurvivalGame.RealNow);
                if (!tallyFlashOk18) g = double.MaxValue;
            }
            float intensity = Mathf.Clamp01((float)game.settings.Num("flashIntensity"));
            float flash = !feel && g >= 0 && g < UnityTuning.TallyFlashTime
                ? UnityTuning.TallyFlashAlpha * (1 - (float)(g / UnityTuning.TallyFlashTime)) * intensity
                : 0;
            if (tallyFlash.enabled != flash > 0) tallyFlash.enabled = flash > 0;
            if (flash > 0) tallyFlash.color = UiTheme.A(UiTheme.White, flash);
            // NIGHT18: the flash on the grade's plate, shrinking onto it (a full-screen sheet read as a pop-in and too much)
            float plate = feel && tallyFlashOk18 && tallyGradePlate.enabled && g >= 0 && g < UnityTuning.TallyFlashTime18
                ? UnityTuning.TallyFlashAlpha18 * MenuMotion18.GradeFlashAlpha(g) * intensity
                : 0;
            if (tallyFlash18.enabled != plate > 0) tallyFlash18.enabled = plate > 0;
            if (plate > 0)
            {
                tallyFlash18.color = UiTheme.A(UiTheme.White, plate);
                float fs = MenuMotion18.GradeFlashScale(g);
                if (game.sim != null && game.sim.n21.stamp)
                {
                    // NIGHT22 T-11: the flash copy shrinks from a big scale (x the grade group's slam) and its rect crossed POINTS (4205 px2 on
                    // the Player, frame 523): keep its left edge a few px clear of POINTS' right edge (the column gap less 4 px)
                    float gpl = UnityTuning.ResultsGradePlate, gwl = UnityTuning.ResultsGradeWidth, off = (gpl - gwl) * 0.5f, sl = Mathf.Max(1f, gs);
                    float maxFs = (gwl * 0.5f + UnityTuning.ResultsColumnGap - 4f + off * sl) / (gpl * 0.5f * sl);
                    fs = Mathf.Min(fs, Mathf.Max(1f, maxFs));
                }
                tallyFlash18.rectTransform.localScale = new Vector3(fs, fs, 1);
            }
            if (tallyNewBest)
            {
                double b = t - TallyAt(4);
                float stamp = b < 0 ? UnityTuning.TallyStampFrom : Mathf.LerpUnclamped(UnityTuning.TallyStampFrom, 1, UiTween.OutBack(b / UnityTuning.TallyStampTime));
                if (tallyStamp.localScale.x != stamp) tallyStamp.localScale = new Vector3(stamp, stamp, 1);
            }
        }

        static float Fade(double t, double at) => UiTween.OutCubic((t - at) / UnityTuning.ResultsLineFade);

        /// <summary>NIGHT18: line `line`'s beat (MenuRules.LineAt), after the card's lead with FeelAB on (fixed per tally).</summary>
        double TallyAt(int line) => MenuRules.LineAt(line) + tallyLead18;

        double tallyLead18;

        /// <summary>NIGHT18: the card's slide in from MenuSlabEnter px down-left along the slant (ease-out-back over ThemeInTime).</summary>
        void TallySlideFar18(CanvasGroup g, int slot, double t)
        {
            double u = t / UnityTuning.ThemeInTime;
            var rt = (RectTransform)g.transform;
            if (u >= 1 || game.settings.View.reducedMotion)
            {
                if (rt.anchoredPosition != tallyHomes[slot]) rt.anchoredPosition = tallyHomes[slot];
                return;
            }
            float d = (1 - UiTween.OutBack(u)) * UnityTuning.MenuSlabEnter;
            rt.anchoredPosition = tallyHomes[slot] + new Vector2(-d, -d * MenuSlantK);
        }

        /// <summary>A tally line's slide along the slant from down-left to its rest (written only while it moves, once at rest).</summary>
        void TallySlide(CanvasGroup g, int slot, double t, double at)
        {
            double u = (t - at) / UnityTuning.ThemeInTime;
            var rt = (RectTransform)g.transform;
            if (u >= 1)
            {
                if (rt.anchoredPosition != tallyHomes[slot]) rt.anchoredPosition = tallyHomes[slot]; // at rest (a skip lands here too)
                return;
            }
            bool still = game.settings != null && game.settings.View.reducedMotion;
            float e = still ? 1f : UiTween.OutBack(u);
            float d = (1 - e) * UnityTuning.ThemeEnterDistance;
            rt.anchoredPosition = tallyHomes[slot] + new Vector2(-d, -d * MenuSlantK);
        }

        void SetGroupAlpha(CanvasGroup g, int slot, float a)
        {
            if (tallyShown[slot] == a) return;
            tallyShown[slot] = a;
            g.alpha = a;
        }

        /// <summary>Any key, a mouse button or a gamepad button pressed this frame.</summary>
        static bool SkipPressed()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.anyKey.wasPressedThisFrame) return true;
            Mouse m = Mouse.current;
            if (m != null && (m.leftButton.wasPressedThisFrame || m.rightButton.wasPressedThisFrame)) return true;
            Gamepad gp = Gamepad.current;
            return gp != null && (gp.buttonSouth.wasPressedThisFrame || gp.buttonEast.wasPressedThisFrame || gp.startButton.wasPressedThisFrame);
        }
    }
}
