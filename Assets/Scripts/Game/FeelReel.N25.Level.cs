// NIGHT25 reel `tutorial` (builder L): the TUTORIAL walked on the Player, stage 1.1 to 1.6, by the TutorialPilot25 (scripted movement stages, the autopilot's fights)
// through the sim's public hooks only (no Editor). Boot with -skygrindMap tutorial -skygrindReelOnly tutorial. It prints, per stage: the game time it took,
// the frames recorded, the lessonBegin / lessonClear order with game times, the prompt text the lesson widget (Ui.Lesson25TextNow) showed, the stage respawns,
// and a soft-lock verdict (a stage over 6x its par + 60 s of game time, or the run over its budget, ends the run as SOFT-LOCK).
// RED: "NOT WRITTEN (RED)" before this file; "SKIPPED (RED)" while the campaign row is off or the booted map is not a campaign level or not the tutorial.
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N25TutorialBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = N25TutorialScenario(g, dir, res, offs, pre);
        }

        const int StageFrameCap25 = 240; // frames recorded per stage (the first 4 s of each stage at 60 fps); the probe caps one recording at 1800

        static IEnumerator N25TutorialScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            string why = N24CampaignGate(g);
            if (why != null)
            {
                res[0] = why;
                yield break;
            }
            string mapId = g.MapId; // NIGHT25 (lead): the generated map JSON has `name`, not `id` (the gate read `id` and always skipped)
            if (mapId != "tutorial")
            {
                res[0] = "SKIPPED (RED): the booted map is '" + mapId + "', not the tutorial (boot the Player with -skygrindMap tutorial)";
                yield break;
            }
            var order = new List<string>(); // "s2 jumpedPit begin 3.1" / "clear 5.0": the lesson order with game seconds
            var stageFrames = new Dictionary<string, int>();
            var stageStartFrame = new Dictionary<string, int>();
            var stageSeconds = new Dictionary<string, double>();
            var stageNames = new List<string>();
            var prompts = new List<string>();
            int respawns = 0, retries = 0, layerClears = 0;
            double Now() => g.sim.run.steps * g.sim.step;
            offs.Add(g.events.On<LessonEvent>("lessonBegin", e => order.Add($"{e.id}:{e.kind} begin {Now():0.0}")));
            offs.Add(g.events.On<LessonEvent>("lessonClear", e => order.Add($"{e.id}:{e.kind} CLEAR {Now():0.0}")));
            string wantStage = null; // the stage the player is in now: its own recording (the probe's 1800-frame cap is per recording, so one per stage)
            offs.Add(g.events.On<RoomEvent>("roomEnter", e =>
            {
                wantStage = e.id;
                stageNames.Add(e.id);
            }));
            offs.Add(g.events.On<RoomEvent>("roomClear", e => stageSeconds[e.id] = e.time));
            offs.Add(g.events.On<RoomEvent>("stageRespawn", _ => respawns++));
            offs.Add(g.events.On<RoomEvent>("roomRetry", _ => retries++));
            offs.Add(g.events.On<RoomEvent>("layerClear", _ => layerClears++));
            AutopilotInput ap = N24StartCampaign(g, offs);
            g.sim.UseInput(new TutorialPilot25(g.sim, ap)); // the scripted movement stages over the autopilot
            string pre0 = Preconditions(g, g.Feet);
            if (!g.sim.campaignOn)
            {
                res[0] = "SKIPPED (RED): the run did not start as a CAMPAIGN";
                yield break;
            }
            LevelDef lv = g.sim.campaignLevel;
            yield return Frames(20);
            yield return Frames(pre);
            string recStage = null;
            var recorded = new HashSet<string>();
            int totalFrames = 0;
            string lastPrompt = "";
            string softLock = null;
            double budget = 0;
            foreach (RoomDef r in lv.rooms) budget += r.par * 6 + 60;
            // NIGHT26: THE BUDGET IS GAME TIME, not frames (the NIGHT25 levelwalk bug again: unrecorded frames run at the display's rate, a few ms of game time each, so a frame-count
            // loop ended the n26-6 tutorial reel at ~120 s wall with stage 1.8 still running). Game time runs at wall rate when nothing records: the cap is
            // UnityTuning.ReelTutorialGameCap26 s of game time (or the sum of 6 x par + 60 s per stage when that is less), with a wall-clock guard besides.
            budget = Math.Min(budget, UnityTuning.ReelTutorialGameCap26);
            float wall0 = UnityEngine.Time.realtimeSinceStartup;
            for (int i = 0; UnityEngine.Time.realtimeSinceStartup - wall0 < UnityTuning.ReelTutorialWallGuard26; i++)
            {
                AutopilotInput.TakeDraft(g.sim);
                Campaign24 c = g.sim.campaign;
                if (recStage != null && (recStage != wantStage || FeelProbe.FrameCount >= StageFrameCap25))
                {
                    stageFrames[recStage] = FeelProbe.FrameCount;
                    totalFrames += FeelProbe.FrameCount;
                    yield return Stop();
                    recStage = null;
                }
                if (recStage == null && wantStage != null && !recorded.Contains(wantStage))
                {
                    recorded.Add(wantStage);
                    recStage = wantStage;
                    Start(g, "tutorial-" + recStage, "ui", dir + "/" + recStage);
                    FeelProbe.MarkInput();
                }
                if (c != null && c.finished) break;
                if (g.flow == Flow.Results) break;
                if (Ui.Lesson25Visible && Ui.Lesson25TextNow != lastPrompt)
                {
                    lastPrompt = Ui.Lesson25TextNow;
                    string room = c != null && c.room >= 0 ? lv.rooms[c.room].id : "-";
                    prompts.Add($"{room} \"{lastPrompt}\" ({Ui.Lesson25PxNow:0} px) at {Now():0.0} s");
                }
                if (c != null && c.room >= 0 && c.phase == Campaign24.Phase.Fight && c.roomSteps * g.sim.step > lv.rooms[c.room].par * 6 + 60)
                {
                    softLock = $"{lv.rooms[c.room].id} ({lv.rooms[c.room].name}) open for {c.roomSteps * g.sim.step:0} s (par {lv.rooms[c.room].par:0}), lesson {(c.lesson != null && !c.lesson.Done ? LessonDef.KindName(c.lesson.Current.kind) : "-")}, enemies {g.sim.enemies.AliveCount}, top-ups {c.lessonTopUps}";
                    break;
                }
                if (Now() > budget) { softLock = $"the run passed its budget of {budget:0} s"; break; }
                yield return null;
            }
            yield return Frames(Ms(1000));
            if (recStage != null)
            {
                stageFrames[recStage] = Math.Min(StageFrameCap25, FeelProbe.FrameCount);
                totalFrames += FeelProbe.FrameCount;
                yield return Stop();
            }
            Campaign24 end = g.sim.campaign;
            var rows = new List<string>();
            int under12 = 0;
            foreach (RoomDef r in lv.rooms)
            {
                bool cleared = stageSeconds.TryGetValue(r.id, out double t);
                int fr = stageFrames.TryGetValue(r.id, out int n) ? n : 0;
                if (cleared && fr < 12) under12++;
                rows.Add($"{r.id} {r.name}: {(cleared ? t.ToString("0.0") + " s" : "NOT CLEARED")}, {fr} frames, par {r.par:0}");
            }
            bool cleared8 = end != null && end.clearSteps.Count == lv.rooms.Count;
            string verdict = softLock != null ? "SOFT-LOCK: " + softLock : cleared8 && end.finished ? "TUTORIAL CLEARED" : "stopped before the end";
            res[0] = $"{pre0}; {totalFrames} frames in {recorded.Count} per-stage recordings (dir/<stage>, up to {StageFrameCap25} frames each); stages: {string.Join("; ", rows)}; lesson order: {string.Join(" | ", order)}; prompts shown: {(prompts.Count == 0 ? "NONE" : string.Join("; ", prompts))}; " +
                     $"stage respawns {respawns}, deaths {(end != null ? end.deaths : 0)}, retries {retries}, wave top-ups {(end != null ? end.lessonTopUps : 0)}, layerClear {layerClears}x; stages under 12 frames {under12}; whole tutorial {(end != null ? end.TotalSeconds : 0):0.0} s of game time; {verdict}; flow {g.flow}";
        }
    }
}
