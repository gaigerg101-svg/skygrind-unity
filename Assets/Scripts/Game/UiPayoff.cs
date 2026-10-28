// NIGHT12 U3-25.8 (agent F): the PAYOFF strip on DOWN and RESULTS (design.payoff.enabled; Research/retention section 5
// #2, Hades' every run counts) and the best grade per HEAT rung on TITLE (#7, the grade chase).
// The strip is one panel in the right column (x +500..+900, y +330..-250 at 1080p, from the screen centre), parented to
// the DOWN or the RESULTS screen as the flow enters it, so it shows and hides with that screen. Filled once on entry:
// the personal-best delta (Payoff.BestDelta), the contracts completed this run, the 3 nearest contracts with their bars,
// the MASTERY gained this run (guns with runXp, the tiers reached), and the NEXT UNLOCK (its name, bar and what it opens).
// At DOWN the run hasn't ended (RunStats are filled by SurvivalRun.End), so the delta is judged on a scratch copy of the
// live round and points; RESULTS uses game.lastStats.
// The TITLE row: "BEST  B   HEAT 1  S   HEAT 2  -", the grades in results.gradeColors (gold when a grade has none: P).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        sealed class PayoffLine
        {
            public TextMeshProUGUI left, right;
            public UnityEngine.UI.Image back, fill;
        }

        RectTransform payoffPanel;
        readonly List<PayoffLine> payoffLines = new List<PayoffLine>();
        int payoffUsed;
        float payoffY;
        bool payoffWanted; // NIGHT17: the strip has something to show (RESULTS shows it with DETAILS)
        RunStats payoffStats; // the stats the strip was filled from (RESULTS refills on a new run's)
        readonly RunStats downScratch = new RunStats(); // DOWN's live round and points for the delta (never the sim's)
        TextMeshProUGUI bestRow;

        void BuildPayoff()
        {
            var back = Img(down.transform, "Payoff", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(UnityTuning.PayoffWidth, UnityTuning.PayoffTop - UnityTuning.PayoffBottom));
            payoffPanel = back.rectTransform;
            payoffPanel.pivot = new Vector2(0, 1);
            payoffPanel.anchoredPosition = new Vector2(UnityTuning.ResultsPayoffX, UnityTuning.PayoffTop); // NIGHT17 round 1: inside the right inset at 16:10
            Img(back.transform, "Edge", usable, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2((float)cfg.Num("ui.theme.edgeWidth"), 0)).rectTransform.pivot = new Vector2(0, 0.5f);
            int lines = (int)((UnityTuning.PayoffTop - UnityTuning.PayoffBottom) / UnityTuning.PayoffLine) + 1;
            float w = UnityTuning.PayoffWidth - 2 * UnityTuning.PayoffPad;
            for (int i = 0; i < lines; i++)
            {
                var l = new PayoffLine();
                l.left = Text(payoffPanel, $"Line {i}", "", tSmall, text, TextAlignmentOptions.Left, Vector2.zero, new Vector2(w, UnityTuning.PayoffLine), new Vector2(0, 1));
                l.left.overflowMode = TextOverflowModes.Ellipsis;
                l.right = Text(l.left.transform, "Value", "", tSmall, text, TextAlignmentOptions.Right, Vector2.zero, new Vector2(w, UnityTuning.PayoffLine), new Vector2(1, 1));
                l.fill = Bar(l.left.transform, new Vector2(0, 0), new Vector2(0, -UnityTuning.BarHeight), w, out l.back);
                l.left.gameObject.SetActive(false);
                payoffLines.Add(l);
            }
            payoffPanel.gameObject.SetActive(false);
        }

        /// <summary>The flow entered DOWN or RESULTS: move the strip there and fill it (once).</summary>
        void RefreshPayoff(Flow f)
        {
            payoffStats = f == Flow.Results ? game.lastStats : null;
            bool on = payoffOn && (game.contracts != null || game.mastery != null || game.records != null);
            payoffWanted = on;
            payoffPanel.gameObject.SetActive(on && (f != Flow.Results || resultsDetailsOpen)); // NIGHT17: on RESULTS with DETAILS only
            if (!on) return;
            payoffPanel.SetParent((f == Flow.Results ? results : down).transform, false);
            payoffPanel.SetAsLastSibling();
            payoffUsed = 0;
            payoffY = -UnityTuning.PayoffPad;
            PLine("THIS RUN", null, muted, tLabel, -1, UnityTuning.PayoffLine, true);

            // The personal-best delta (records.js judge per field).
            RunStats s = payoffStats;
            if (s == null)
            {
                SurvivalRun run = game.run;
                downScratch.round = run.round;
                downScratch.cleared = run.cleared;
                downScratch.points = game.economy.total;
                downScratch.earned = game.economy.earned;
                downScratch.spent = game.economy.spent;
                downScratch.time = run.Time;
                downScratch.kills = run.stats.kills;
                downScratch.headshots = run.stats.headshots;
                s = downScratch;
            }
            string delta = game.records != null ? Payoff.BestDelta(s, game.records.Best(game.sim.RecordsKey)) : null;
            if (delta != null) PLine(delta, null, gold, tBody, -1, UnityTuning.PayoffBodyLine);

            // NIGHT21 U3-35.5 (UNCOMPILED): the round strip, one grade per cleared round (Core/Grades21.cs; design.n21.stamp on):
            // "1 B   2 D   3 A ..." in rows of PayoffRoundsPerRow, the last PayoffRoundsRows rows; a P in the theme's gold.
            RoundGrader21 grades = game.sim?.grades21;
            if (grades != null && grades.Rounds.Count > 0)
            {
                Section();
                PLine("ROUNDS", grades.Rounds.Count.ToString(), muted, tLabel, -1, UnityTuning.PayoffLine, true);
                int per = UnityTuning.PayoffRoundsPerRow;
                int first = Math.Max(0, grades.Rounds.Count - per * UnityTuning.PayoffRoundsRows);
                first -= first % per; // whole rows
                var row = new System.Text.StringBuilder();
                for (int i = first; i < grades.Rounds.Count; i++)
                {
                    RoundGradeEvent g = grades.Rounds[i];
                    if (row.Length > 0) row.Append("   ");
                    row.Append(g.round).Append(' ').Append(g.grade);
                    if ((i - first) % per == per - 1 || i == grades.Rounds.Count - 1)
                    {
                        PLine(row.ToString(), null, text, tSmall, -1, UnityTuning.PayoffLine);
                        row.Length = 0;
                    }
                }
            }

            Contracts c = game.contracts;
            if (c != null)
            {
                // Contracts completed this run (their names in cyan: an unlock highlight).
                if (c.doneThisRun.Count > 0)
                {
                    Section();
                    PLine("CONTRACTS DONE", c.doneThisRun.Count.ToString(), muted, tLabel, -1, UnityTuning.PayoffLine, true);
                    int shown = Math.Min(c.doneThisRun.Count, UnityTuning.PayoffListMax);
                    for (int i = 0; i < shown; i++) PLine(ContractName(c, c.doneThisRun[i]), null, usable, tSmall, -1, UnityTuning.PayoffLine);
                    if (c.doneThisRun.Count > shown) PLine($"+{c.doneThisRun.Count - shown} MORE", null, muted, tSmall, -1, UnityTuning.PayoffLine);
                }
                // The 3 nearest contracts with their bars.
                List<Contracts.Row> near = c.Nearest(UnityTuning.PayoffNearest);
                if (near != null && near.Count > 0)
                {
                    Section();
                    PLine("CLOSEST", null, muted, tLabel, -1, UnityTuning.PayoffLine, true);
                    foreach (Contracts.Row r in near) PLine(r.name ?? r.id, Progress(r.value, r.target), text, tSmall, r.Fraction, UnityTuning.PayoffLine);
                }
            }

            // MASTERY gained this run: guns with progress, then the tiers reached.
            Mastery m = game.mastery;
            if (m != null)
            {
                List<Mastery.Gun> guns = m.Guns();
                int gained = 0;
                foreach (Mastery.Gun g in guns) if (g.runXp > 0) gained++;
                if (gained > 0 || m.tierUpsThisRun.Count > 0)
                {
                    Section();
                    PLine("MASTERY", null, muted, tLabel, -1, UnityTuning.PayoffLine, true);
                    int shown = 0;
                    foreach (var (weapon, tier) in m.tierUpsThisRun)
                    {
                        if (shown++ >= UnityTuning.PayoffListMax) break;
                        PLine((m.Of(weapon)?.name ?? WeaponName(weapon)) + " MASTERY " + tier, null, usable, tSmall, -1, UnityTuning.PayoffLine);
                    }
                    foreach (Mastery.Gun g in guns)
                    {
                        if (g.runXp <= 0) continue;
                        if (shown++ >= UnityTuning.PayoffListMax) break;
                        PLine(g.name ?? WeaponName(g.weapon), "+" + MetaNum(g.runXp), text, tSmall, -1, UnityTuning.PayoffLine);
                    }
                }
            }

            // NEXT UNLOCK: what it opens (cyan), the contract and its bar.
            Contracts.Row next = c?.NextUnlock();
            if (next != null)
            {
                Section();
                PLine("NEXT UNLOCK", null, muted, tLabel, -1, UnityTuning.PayoffLine, true);
                PLine(LockMark + " " + (next.unlockName ?? next.unlock?.ToUpperInvariant() ?? next.name), null, usable, tBody, -1, UnityTuning.PayoffBodyLine);
                PLine(next.name ?? next.id, Progress(next.value, next.target), text, tSmall, next.Fraction, UnityTuning.PayoffLine);
            }
            for (int i = payoffUsed; i < payoffLines.Count; i++) payoffLines[i].left.gameObject.SetActive(false);
        }

        void Section() => payoffY -= UnityTuning.PayoffSectionGap;

        // NIGHT18 U3-31.6 (FeelAB on): the strip enters whenever it comes on screen (DOWN, or RESULTS' DETAILS): it fades in and
        // slides ThemeEnterDistance px along the slant (a panel: no scale from its corner); off: it appears with its screen.
        CanvasGroup payoffGroup18;
        Vector2 payoffHome18;
        double payoffEnterAge18 = double.MaxValue;
        bool payoffWasOn18;
        float payoffShownAlpha18 = 1;

        /// <summary>Every frame, every flow (Ui.Render, before the HUD's early return).</summary>
        void Payoff18(double frameDt)
        {
            if (payoffPanel == null) return;
            bool on = payoffPanel.gameObject.activeInHierarchy;
            if (on && !payoffWasOn18)
            {
                payoffEnterAge18 = 0;
                if (payoffGroup18 == null)
                {
                    payoffGroup18 = payoffPanel.gameObject.AddComponent<CanvasGroup>();
                    payoffGroup18.interactable = payoffGroup18.blocksRaycasts = false;
                    payoffHome18 = payoffPanel.anchoredPosition;
                }
            }
            payoffWasOn18 = on;
            if (!on || payoffGroup18 == null) return;
            float a = 1;
            Vector2 off = default;
            if (FeelAB.On && !HudEntrance18.Done(payoffEnterAge18))
            {
                payoffEnterAge18 = HudEntrance18.Step(payoffEnterAge18, frameDt);
                a = HudEntrance18.Alpha(payoffEnterAge18);
                off = HudEntrance18.Offset(payoffEnterAge18, UnityTuning.ThemeEnterDistance);
            }
            if (a == payoffShownAlpha18 && off == Vector2.zero && payoffPanel.anchoredPosition == payoffHome18) return;
            payoffShownAlpha18 = a;
            payoffGroup18.alpha = a;
            payoffPanel.anchoredPosition = payoffHome18 + off;
        }

        /// <summary>
        /// One strip line at the cursor: a left text, an optional right value, an optional bar under it (fraction >= 0).
        /// `label`: a section label (theme.js .sk-label: wide tracking). NIGHT13 S2: an explicit flag; the size test it
        /// replaces (size == tLabel) also tracked every small line, because ui.theme.type.small == label == 16.
        /// </summary>
        void PLine(string left, string right, Color c, float size, double fraction, float height, bool label = false)
        {
            bool bar = fraction >= 0;
            float need = height + (bar ? UnityTuning.PayoffBarRoom : 0);
            if (payoffUsed >= payoffLines.Count || payoffY - need < UnityTuning.PayoffBottom - UnityTuning.PayoffTop + UnityTuning.PayoffPad) return; // full
            PayoffLine l = payoffLines[payoffUsed++];
            l.left.gameObject.SetActive(true);
            l.left.rectTransform.anchoredPosition = new Vector2(UnityTuning.PayoffPad, payoffY);
            l.left.rectTransform.sizeDelta = new Vector2(UnityTuning.PayoffWidth - 2 * UnityTuning.PayoffPad, height);
            l.left.text = left ?? string.Empty;
            l.left.color = c;
            l.left.fontSize = Mathf.Max(16, size); // the theme's 16 px floor
            // NIGHT18 U3-31.6 (FeelAB on): the theme's 18 px floor (M10; the line above set 16 after Text()'s build floor of 18)
            if (FeelAB.On) l.left.fontSize = Mathf.Max(UnityTuning.ThemeTextFloor, size);
            l.left.characterSpacing = label ? UnityTuning.LabelSpacing : 0;
            PlaceLocks(l.left, left != null && left.IndexOf(LockChar) >= 0); // NIGHT13 S1: the NEXT UNLOCK's padlock
            l.right.text = right ?? string.Empty;
            l.right.rectTransform.sizeDelta = new Vector2(UnityTuning.PayoffWidth - 2 * UnityTuning.PayoffPad, height);
            l.back.gameObject.SetActive(bar);
            if (bar) SetBar(l.fill, fraction);
            payoffY -= need;
        }

        static string ContractName(Contracts c, string id)
        {
            foreach (Contracts.Row r in c.rows) if (r.id == id) return r.name ?? id;
            return id;
        }

        // --- the best grade per HEAT rung on TITLE ------------------------------------------------------------------------

        void BuildBestGrades()
        {
            bestRow = Text(title.transform, "Best grades", "", tBody, text, TextAlignmentOptions.Center, Vector2.zero, new Vector2(UnityTuning.BestRowWidth, UnityTuning.BestRowHeight), new Vector2(0.5f, 0.5f));
            bestRow.characterSpacing = UnityTuning.LabelSpacing;
            bestRow.gameObject.SetActive(false);
        }

        void RefreshBestGrades()
        {
            Profile p = game.profile;
            int rungs = HeatShown ? game.heat.Rungs : 0;
            // Round 2: only the rungs with a grade ("BEST D" alone, never a row of dashes); none: no row (TITLE opening: strings then).
            var grades = new string[rungs + 1];
            if (p != null) for (int r = 0; r <= rungs; r++) grades[r] = p.BestGrade(r);
            List<int> shown = MenuRules.BestRungs(grades);
            bestRow.gameObject.SetActive(shown.Count > 0);
            if (shown.Count == 0) return;
            metaSb.Clear();
            for (int i = 0; i < shown.Count; i++)
            {
                int r = shown[i];
                if (i > 0) metaSb.Append("     ");
                metaSb.Append("<color=#").Append(mutedHex).Append('>').Append(r == 0 ? "BEST GRADE" : game.heat.Name(r) + " BEST") /* round 5: "BEST D" meant nothing to a new player */.Append("</color>  ");
                string g = grades[r];
                metaSb.Append("<color=#").Append(gradeHex.TryGetValue(g, out string hex) ? hex : goldHex).Append("><b>").Append(g).Append("</b></color>");
            }
            bestRow.SetText(metaSb);
        }
    }
}
