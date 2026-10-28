// NIGHT22 U3-36.4 THE STAMP + T-11 (builder S): the reel scenario `stamp` (contract `stamp`), written FIRST and RED while the row's
// pieces are absent. UNCOMPILED when written (the builder has no Editor). The scenario, on the Player:
//   1. a SafeSpot respawn; the aim bot clears round 1 with the health held and the style meter fed to the top rank every frame
//      ("scripted": the bot never reaches SS, A-4), the first break ended at once (like `roundend`);
//   2. the recording starts with one enemy left and runs through the round clear: every frame the stamp's alpha and scale are
//      sampled (Ui.Stamp22Alpha / Stamp22Scale), and the entrance is measured the way feel-metrics M9 does (first visible frame to
//      the first frame within 3 % of rest scale at 95 % alpha that stays so for 6 frames) and the exit from the first frame under full
//      alpha to the last visible one;
//   3. the player is downed and RESULTS opens in the same recording: every RESULTS frame Ui.ResultsGradeOverPoints22() (the largest
//      overlap in px squared of a visible GRADE-card graphic with a visible POINTS graphic) is read: T-11 wants 0 on every frame.
// The result line starts with the preconditions (SafeSpot's), then the numbers, then PASS or RED with every failed part named. With
// design.n21.stamp off (-skygrindOff21) there is no roundGrade, no stamp, and the old POINTS box: that is the RED the lead records.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N22StampBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = N22StampRun(g, dir, res, offs, pre);
        }

        static IEnumerator N22StampRun(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            g.run.phaseSteps = 1; // the first break ends at once (as `roundend`)
            g.husks.frozen = false;
            yield return Frames(10);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (UNDER 12 m)");
            res[0] = pc + "; RED: the scenario did not reach the round clear";
            Ui ui = g.UiView;
            if (ui == null || !ui.Stamp22Built)
            {
                res[0] = pc + "; RED: no stamp element (the HUD has no round stamp: design.n21.stamp off or the widget not built)";
                yield break;
            }
            try
            {
                if (g.settings.Bool(UnityTuning.RecapSetting))
                {
                    g.settings.Set(UnityTuning.RecapSetting, false);
                    offs.Add(() => g.settings.Set(UnityTuning.RecapSetting, true));
                }
            }
            catch (KeyNotFoundException) { }

            int said = 0, pSaid = 0;
            string grade = "none";
            bool cleared = false;
            offs.Add(g.events.On(Grades21.EventName, o =>
            {
                var e = (RoundGradeEvent)o;
                said++;
                if (e.p) pSaid++;
                grade = e.grade;
            }));
            offs.Add(g.events.On("roundClear", _ => cleared = true));
            int top = g.style.Params.letters.Count - 1;

            // 1. clear round 1 (scripted), recording from the last enemy
            int guard = 0;
            bool recording = false;
            while (!cleared && guard++ < 7200)
            {
                God(g);
                g.style.core.DebugSetRank(top); // SCRIPTED: the style meter at the top rank
                FeelStrip.AimBot(g);
                int alive = 0;
                foreach (Enemy e in g.enemies.all) if (e.alive) alive++;
                if (!recording && alive == 1 && g.run.stats.kills >= UnityTuning.ReelRoundEndKillsBeforeRecord)
                {
                    Start(g, "stamp", "ui", dir);
                    recording = true;
                    FeelProbe.MarkInput();
                }
                yield return null;
            }
            if (!recording)
            {
                Start(g, "stamp", "ui", dir);
                recording = true;
            }
            if (!cleared)
            {
                g.forcedCombat = IdleCombat;
                yield return Stop();
                res[0] = pc + "; RED: round 1 was not cleared in 2 minutes";
                yield break;
            }

            // 2. the stamp's frames after the clear
            var al = new List<float>();
            var sc = new List<float>();
            int span = Ms(6000); // the stamp lives about 2.7 s (design.n21.stamp in + hold + out)
            bool seen = false;
            for (int i = 0; i < span; i++)
            {
                God(g);
                g.style.core.DebugSetRank(top);
                g.forcedCombat = IdleCombat;
                al.Add(ui.Stamp22Alpha);
                sc.Add(ui.Stamp22Scale);
                yield return null;
                if (al[al.Count - 1] > 0) seen = true;
                if (seen && AllZeroTail(al, 10)) break; // it has come and gone
                if (!seen && i > Ms(1500)) break; // it never came (the row off)
            }
            // entrance and exit, M9's rule at the reel's fixed frame rate
            int first = -1, settle = -1, run = 0, fall = -1, last = -1;
            for (int i = 0; i < al.Count; i++)
            {
                float a = al[i], s = sc[i];
                if (first < 0 && a > 0) first = i;
                if (first < 0) continue;
                if (settle < 0)
                {
                    bool ok = Math.Abs(s - 1) <= 0.03f && a >= 0.95f;
                    run = ok ? run + 1 : 0;
                    if (run >= 6) settle = i - 5;
                }
                else if (fall < 0 && a < 0.999f && a > 0) fall = i;
                if (a > 0) last = i;
            }
            double fps = UnityTuning.ReelFps;
            double inMs = first >= 0 && settle >= 0 ? (settle - first) * 1000.0 / fps : -1;
            double outMs = fall >= 0 && last >= fall ? (last - fall + 1) * 1000.0 / fps : -1;
            float s0 = first >= 0 ? sc[first] : 0, a0 = first >= 0 ? al[first] : 0;
            bool pop = first >= 0 && s0 >= 0.95f && s0 <= 1.05f && a0 >= 0.95f;
            string stampGrade = ui.Stamp22Grade ?? "none";

            // 3. DOWN, then RESULTS in the same recording: the GRADE card against POINTS on every frame
            Vec3 f0 = g.Feet;
            g.health.max = g.health.hp = 100;
            g.health.Damage(g.health.max * 2, "husk", new Vec3(f0.x, f0.y, f0.z - 3));
            int n = 0, resultsFrames = 0, after = 0;
            float worst = 0;
            bool noRects = false;
            while (n++ < 1800 && after < Ms(3500))
            {
                if (g.flow == Flow.Results)
                {
                    resultsFrames++;
                    float o = ui.ResultsGradeOverPoints22();
                    if (o < 0) noRects = true;
                    else if (o > worst) worst = o;
                    after++;
                }
                yield return null;
            }
            yield return Stop();

            var why = new StringBuilder();
            if (said != 1) why.Append(" roundGrade said ").Append(said).Append("x (want 1);");
            if (pSaid != 1) why.Append(" p=true ").Append(pSaid).Append("x (want 1);");
            if (first < 0) why.Append(" the stamp never appeared;");
            else
            {
                if (inMs < 200 || inMs > 300) why.Append(" entrance ").Append(inMs.ToString("0")).Append(" ms;");
                if (outMs < 150 || outMs > 200) why.Append(" exit ").Append(outMs.ToString("0")).Append(" ms;");
                if (pop) why.Append(" first frame is a pop-in;");
            }
            if (resultsFrames == 0) why.Append(" RESULTS never opened;");
            else if (noRects) why.Append(" the grade / POINTS rects are missing;");
            else if (worst > 0) why.Append(" the grade card overlaps POINTS by ").Append(worst.ToString("0")).Append(" px2;");
            res[0] = $"{pc}; scripted (style meter fed to SS, health held) stamp {stampGrade} (event grade {grade}, roundGrade said {said}x, p=true {pSaid}x), first visible frame scale {s0:0.00} alpha {a0:0.00}, entrance {inMs:0} ms (want 200-300), exit {outMs:0} ms (want 150-200); grade card vs POINTS: max overlap {worst:0} px2 over {resultsFrames} RESULTS frames (want 0); {(why.Length == 0 ? "PASS" : "RED:" + why)}";
            yield break;
        }

        static bool AllZeroTail(List<float> a, int n)
        {
            if (a.Count < n) return false;
            for (int i = a.Count - n; i < a.Count; i++) if (a[i] > 0) return false;
            return true;
        }
    }
}
