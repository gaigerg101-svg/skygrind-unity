// NIGHT25 (builder F): the reel scenario `lesson`, the lesson line's Player half (UiLesson25.cs). Written without the Editor and not run.
// The story, in SURVIVAL on the safe spot:
//   one    ShowLesson25("{jump} TO CLIMB") (the pad glyph on a pad, the key name on a keyboard): the first drawn frame is at alpha ~0 (no pop-in),
//          alpha reaches 1 within 300 ms (the entrance's ms is printed), the text is 18 px or more (the px printed), at most 6 words
//   use    HideLesson25() (the verb was used): the line is gone (not visible, alpha 0) within 300 ms (printed), and stays gone for a second
//   two    ShowLesson25("SLIDE UNDER THE GAP"): the same checks; HideLesson25(); gone
// Every result line starts with FeelReel.Preconditions(g, pos) from SafeSpot(g). RED: the line is not built (the hook did nothing), a first
// frame at alpha 0.1 or more, an entrance over 300 ms, text under 18 px, over 6 words, or not gone after use.
// Wiring (the lead's FeelReel.N25.cs): `case "lesson": body = Lesson25Scenario(g, dir, res, offs, pre); break;` and "lesson" in the phase's list.
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N25LessonBody(SurvivalGame g, string dir, string[] res, List<System.Action> offs, int pre, ref IEnumerator body)
        {
            body = Lesson25Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Lesson25Scenario(SurvivalGame g, string dir, string[] res, List<System.Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            var fails = new List<string>();
            g.Respawn(spot, spotYaw * 180 / System.Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(20);
            g.husks.frozen = true;
            Ui ui = g.UiView;
            if (ui == null)
            {
                res[0] = pc + " | RED: the Ui is not built";
                yield break;
            }
            Start(g, "lesson", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();

            string[] lines = { "{jump} TO CLIMB", "SLIDE UNDER THE GAP" };
            var rows = new List<string>();
            for (int k = 0; k < lines.Length; k++)
            {
                int shownBefore = Ui.Lesson25Shown;
                ui.ShowLesson25(lines[k]);
                yield return Frames(Ms(700));
                string text = Ui.Lesson25TextNow;
                if (Ui.Lesson25Shown != shownBefore + 1) fails.Add($"line {k + 1}: ShowLesson25 did not draw a line ({Ui.Lesson25Shown - shownBefore} new)");
                if (!Ui.Lesson25Visible) fails.Add($"line {k + 1} '{text}' is not on screen 700 ms after the ask");
                if (Ui.Lesson25FirstAlpha < 0 || Ui.Lesson25FirstAlpha >= 0.1f) fails.Add($"line {k + 1}: its first drawn frame is at alpha {Ui.Lesson25FirstAlpha.ToString("0.00", ci)} (a pop-in)");
                if (Ui.Lesson25InMs < 0 || Ui.Lesson25InMs > 300) fails.Add($"line {k + 1}: entrance {Ui.Lesson25InMs.ToString("0", ci)} ms (not 1-300)");
                if (Ui.Lesson25PxNow < 18f) fails.Add($"line {k + 1}: text {Ui.Lesson25PxNow.ToString("0", ci)} px, under 18");
                int words = text.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
                if (words > 6) fails.Add($"line {k + 1}: {words} words, over 6");
                double inMs = Ui.Lesson25InMs;
                float px = Ui.Lesson25PxNow;
                float first = Ui.Lesson25FirstAlpha;

                FeelProbe.Event("lessonUse");
                ui.HideLesson25(); // the verb was used
                yield return Frames(Ms(500));
                double outMs = Ui.Lesson25OutMs;
                if (Ui.Lesson25Visible || Ui.Lesson25AlphaNow > 0f) fails.Add($"line {k + 1}: still on screen 500 ms after its use");
                if (outMs < 0 || outMs > 300) fails.Add($"line {k + 1}: exit {outMs.ToString("0", ci)} ms (not 1-300)");
                yield return Frames(Ms(1000));
                if (Ui.Lesson25Visible) fails.Add($"line {k + 1}: came back after its use");
                rows.Add($"'{text}' ({words} words, {px.ToString("0", ci)} px, first alpha {first.ToString("0.00", ci)}, in {inMs.ToString("0", ci)} ms, out {outMs.ToString("0", ci)} ms, gone after use {!Ui.Lesson25Visible})");
            }
            yield return Stop();
            res[0] = pc + $" | {FeelProbe.FrameCount} frames; {string.Join("; ", rows)} | " + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
