// NIGHT25 (builder F): the reel scenario `campresults`, the CAMPAIGN RESULTS screen on the Player (UiCampaignResults25.cs). Boot with -skygrindMap tutorial -skygrindReelOnly
// campresults. The tutorial is played through by the TutorialPilot25 (as `tutorial` does), layerClear ends the run on RESULTS, and the screen is read:
//   head     "TUTORIAL CLEARED" (not CAMPAIGN COMPLETE: a level follows), the summary line (OVERALL rank, time, DEATHS), one row per room (name | rank | time vs par)
//   look     the smallest text in px (18 or more), the first drawn frame's alpha (under 0.95: no pop-in)
//   focus    CONTINUE is the focused button once the buttons appear (the seconds it took are printed)
//   press    a submit on the focus (the pad's A) records the level it loads: PendingMapId must be the curriculum's next level (the scene reload is skipped for the reel:
//            SurvivalGame.SkipSceneReload25); survival records are not written (sim.noRecords true)
// Every result line starts with FeelReel.Preconditions. RED: not booted on the tutorial, the screen not drawn (Ui.CampRes25Open), no rows, a text under 18 px, CONTINUE not
// focused, or the press not naming the next level.
// Wiring (the lead's FeelReel.N25.cs): `case "campresults": body = CampResults25Scenario(g, dir, res, offs, pre); break;` and "campresults" in the phase's list.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.EventSystems;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static IEnumerator CampResults25Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null)
            {
                res[0] = why;
                yield break;
            }
            if (g.MapId != "tutorial")
            {
                res[0] = "SKIPPED (RED): the booted map is '" + g.MapId + "', not the tutorial (boot the Player with -skygrindMap tutorial)";
                yield break;
            }
            var fails = new List<string>();
            SurvivalGame.SkipSceneReload25 = true;
            AutopilotInput ap = N24StartCampaign(g, offs);
            g.sim.UseInput(new TutorialPilot25(g.sim, ap));
            string pc = Preconditions(g, g.Feet);
            if (!g.sim.campaignOn)
            {
                SurvivalGame.SkipSceneReload25 = false;
                res[0] = pc + " | SKIPPED (RED): the run did not start as a CAMPAIGN";
                yield break;
            }
            LevelDef lv = g.sim.campaignLevel;
            yield return Frames(20 + pre);
            double budget = 0;
            foreach (RoomDef r in lv.rooms) budget += r.par * 6 + 60;
            float wall26 = UnityEngine.Time.realtimeSinceStartup;
            for (int i = 0; UnityEngine.Time.realtimeSinceStartup - wall26 < UnityTuning.ReelTutorialWallGuard26 && g.flow != Flow.Results; i++) // NIGHT26: a wall guard, not a frame count (unrecorded frames run at the display rate)
            {
                AutopilotInput.TakeDraft(g.sim);
                if (g.sim.run.steps * g.sim.step > budget) break;
                yield return null;
            }
            if (g.flow != Flow.Results)
            {
                SurvivalGame.SkipSceneReload25 = false;
                res[0] = pc + " | FAIL (RED): the tutorial did not reach RESULTS (flow " + g.flow + ")";
                yield break;
            }
            Start(g, "campresults", "ui", dir);
            FeelProbe.MarkInput();
            yield return Frames(Ms(500));
            if (!Ui.CampRes25Open) fails.Add("the campaign results are not drawn (Ui.CampRes25Open false)");
            if (!Ui.CampRes25HeadNow.EndsWith("CLEARED", StringComparison.Ordinal)) fails.Add($"the head reads '{Ui.CampRes25HeadNow}'");
            if (Ui.CampRes25RowsNow.Count != lv.rooms.Count) fails.Add($"{Ui.CampRes25RowsNow.Count} rows for {lv.rooms.Count} rooms");
            if (Ui.CampRes25MinPx < 18f) fails.Add($"the smallest text is {Ui.CampRes25MinPx.ToString("0", ci)} px, under 18");
            if (Ui.CampRes25FirstAlpha < 0 || Ui.CampRes25FirstAlpha >= 0.95f) fails.Add($"the head's first drawn frame is at alpha {Ui.CampRes25FirstAlpha.ToString("0.00", ci)} (a pop-in)");
            for (int w = 0; w < Ms(4000) && Ui.CampRes25FocusedAt < 0; w++) yield return null; // the buttons appear and CONTINUE takes the focus
            yield return Frames(Ms(300)); // read inside the move grace: the focus is still where the screen put it
            if (Ui.CampRes25FocusedAt < 0) fails.Add("the buttons never appeared");
            string focusBefore = Ui.CampRes25FocusNow; // read before the press: the press may move the focus
            if (focusBefore != Ui.CampRes25PrimaryNow || focusBefore != "CONTINUE") fails.Add($"the focus is on '{focusBefore}', not CONTINUE (primary '{Ui.CampRes25PrimaryNow}')");
            bool noRecords = g.sim.noRecords;
            if (!noRecords) fails.Add("the campaign run is not holding noRecords (survival records could be written)");
            string next = Ui.CampRes25NextNow;
            SurvivalGame.PendingMapId = null;
            FeelProbe.Event("campContinue");
            // verdict 1: the focused button, pressed as the pad's A does (it must be CONTINUE: checked above); verdict 2: CONTINUE's own click goes to the next level
            string focusedName = EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null ? EventSystem.current.currentSelectedGameObject.name : "none";
            bool pressedPrimary = g.UiView != null && g.UiView.CampRes25PressPrimary();
            yield return Frames(Ms(300));
            string went = SurvivalGame.PendingMapId;
            bool pressed = pressedPrimary;
            if (!pressed) fails.Add("CONTINUE could not be clicked (not shown)");
            if (next == null) fails.Add("no next level is known (the curriculum's map is not in the list)");
            else if (went != next) fails.Add($"CONTINUE went to '{went}', not '{next}'");
            yield return Stop();
            SurvivalGame.SkipSceneReload25 = false;
            SurvivalGame.PendingMapId = null;
            SurvivalGame.PendingMode = null;
            SurvivalGame.AutoStartCampaign25 = false;
            res[0] = pc + $" | {FeelProbe.FrameCount} frames; head '{Ui.CampRes25HeadNow}'; summary '{Ui.CampRes25SummaryNow}'; rows [{string.Join(" ;; ", Ui.CampRes25RowsNow)}]; smallest text {Ui.CampRes25MinPx.ToString("0", ci)} px; first alpha {Ui.CampRes25FirstAlpha.ToString("0.00", ci)}; buttons and focus at {Ui.CampRes25FocusedAt.ToString("0.0", ci)} s, focus before the press '{focusBefore}' (selected object '{focusedName}', focus moved by '{Ui.CampRes25MovedBy}'), primary '{Ui.CampRes25PrimaryNow}'; noRecords {noRecords}; CONTINUE -> PendingMapId '{went}' (next level '{next}') | "
                     + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
