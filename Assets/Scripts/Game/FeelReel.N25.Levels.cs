// NIGHT25 reel `levelwalk` (builder C): any campaign level walked door to door on the Player by the autopilot with the campaign's goal provider (N24StartCampaign), through
// the sim's public hooks only. Boot with -skygrindMap <tutorial | layer1 | levelNN> -skygrindReelOnly levelwalk. Per room it prints: roomEnter / roomClear game time against par,
// first try or deaths, enemies spawned, the LIMIT banner text, the lesson prompts shown, the frames recorded (a per-room recording of up to LevelwalkFrameCap25 frames,
// the probe's one-recording cap is 1800) and, at the end, LEVEL CLEARED or "stopped at room N". A boss room is not fought (the bot cannot): the scenario waits for the
// boss to start and prints "boss room entered" with whether the room view is drawn (g.wyrmRoomView.ShownFor(boss)), then stops there (verdict "stopped at room N (boss)").
// RED ("SKIPPED (RED)"): design.n24.campaign is off or the booted map is not a campaign level.
// The lead registers it: "levelwalk" in FeelReel.PhaseH, a `static partial void N25LevelwalkBody(...)` declaration beside the others and
//   case "levelwalk": body = N25LevelwalkScenario(g, dir, res, offs, pre); break;   in N25Scenario (FeelReel.N25.cs).
// Budget: game time, never a frame count (the first build stopped at ~65 s): 235 s of game time per level or the sum of 6 x par + 60 s per room, whichever is less, and a 240 s wall guard.
// The tutorial map is delegated to N25TutorialScenario (TutorialPilot25): the autopilot cannot do its lessons.
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        const int LevelwalkFrameCap25 = 150; // frames recorded per room, sparse: the first 150 frames of each room

        static IEnumerator N25LevelwalkScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            string why = N24CampaignGate(g);
            if (why != null)
            {
                res[0] = why;
                yield break;
            }
            string mapId = g.MapId;
            if (mapId == "tutorial")
            {
                // the autopilot cannot do the tutorial's lessons (a slide tunnel, a wall-jump flue): the TutorialPilot25 scenario walks it and prints its own verdict
                yield return N25TutorialScenario(g, dir, res, offs, pre);
                yield break;
            }
            var enter = new Dictionary<string, double>();
            var clear = new Dictionary<string, double>();
            var spawned = new Dictionary<string, int>();
            var banners = new Dictionary<string, string>();
            var prompts = new Dictionary<string, List<string>>();
            var frames = new Dictionary<string, int>();
            var deathsAt = new Dictionary<string, int>();
            var bossShown = new Dictionary<string, string>();
            string cur = null;
            string recRoom = null;
            string bossEntered = null;
            double Now() => g.sim.run.steps * g.sim.step;
            offs.Add(g.events.On<RoomEvent>("roomEnter", e =>
            {
                cur = e.id;
                enter[e.id] = Now();
                spawned[e.id] = 0;
                banners[e.id] = e.limits != 0 ? Limit24.Banner(e.limits) : "-";
                if (e.kind == "boss") bossEntered = e.id;
            }));
            offs.Add(g.events.On<RoomEvent>("roomClear", e => clear[e.id] = Now()));
            offs.Add(g.events.On<SpawnEvent>("enemySpawn", e => { if (cur != null) spawned[cur] = spawned[cur] + 1; }));
            AutopilotInput ap = N24StartCampaign(g, offs);
            if (!g.sim.campaignOn)
            {
                res[0] = "SKIPPED (RED): the run did not start as a CAMPAIGN";
                yield break;
            }
            LevelDef lv = g.sim.campaignLevel;
            string pre0 = Preconditions(g, g.Feet);
            yield return Frames(20);
            yield return Frames(pre);
            // THE BUDGET IS GAME TIME, not frames: unrecorded frames run at the display's rate (hundreds of fps, a few ms of game time each) and only a
            // recorded frame is exactly 1 / ReelFps, so a frame-count loop ended the first build's walks at ~65 s of game time. Game time runs at wall rate
            // when nothing records, so the cap is 235 s of game time (< 4 min wall, the lead's limit), or the sum of 6 x par + 60 s per room if that is less,
            // with a wall-clock guard besides.
            double budget = 235, rooms = 0;
            foreach (RoomDef r in lv.rooms) rooms += r.par * 6 + 60;
            budget = Math.Min(budget, rooms);
            float wall0 = UnityEngine.Time.realtimeSinceStartup;
            string verdict = null;
            string lastPrompt = "";
            var seenRecorded = new HashSet<string>();
            int totalFrames = 0;
            for (int i = 0; i < 4000000; i++)
            {
                AutopilotInput.TakeDraft(g.sim);
                Campaign24 c = g.sim.campaign;
                if (recRoom != null && (recRoom != cur || FeelProbe.FrameCount >= LevelwalkFrameCap25))
                {
                    frames[recRoom] = FeelProbe.FrameCount;
                    totalFrames += FeelProbe.FrameCount;
                    yield return Stop();
                    recRoom = null;
                }
                if (recRoom == null && cur != null && !seenRecorded.Contains(cur))
                {
                    seenRecorded.Add(cur);
                    recRoom = cur;
                    Start(g, "levelwalk-" + cur, "ui", dir + "/" + cur);
                    FeelProbe.MarkInput();
                }
                if (c != null && c.finished) { verdict = "LEVEL CLEARED"; break; }
                if (g.flow == Flow.Results) { verdict = c != null && c.finished ? "LEVEL CLEARED" : "stopped at the RESULTS screen"; break; }
                if (Ui.Lesson25Visible && Ui.Lesson25TextNow != lastPrompt && cur != null)
                {
                    lastPrompt = Ui.Lesson25TextNow;
                    if (!prompts.ContainsKey(cur)) prompts[cur] = new List<string>();
                    prompts[cur].Add("\"" + lastPrompt + "\"");
                }
                if (bossEntered != null)
                {
                    // the boss room: the boss starts, the room view is checked after a settle, and the walk stops (the bot does not fight a boss)
                    for (int k = 0; k < Ms(3000); k++) { God(g); yield return null; }
                    string bossId = c != null && c.room >= 0 ? lv.rooms[c.room].boss : "wyrm";
                    bool shown = g.wyrmRoomView != null && g.wyrmRoomView.ShownFor(bossId);
                    bossShown[bossEntered] = $"boss room entered ({bossId}), room view drawn {shown}";
                    verdict = $"stopped at room {(c != null ? c.room + 1 : 0)} of {lv.rooms.Count} ({bossEntered}, a boss room: not fought)";
                    break;
                }
                if (c != null && c.room >= 0 && c.phase == Campaign24.Phase.Fight && c.roomSteps * g.sim.step > lv.rooms[c.room].par * 6 + 60)
                {
                    verdict = $"stopped at room {c.room + 1} of {lv.rooms.Count} ({lv.rooms[c.room].id}): open {c.roomSteps * g.sim.step:0} s against a par of {lv.rooms[c.room].par:0} (soft-lock)";
                    break;
                }
                if (Now() > budget || UnityEngine.Time.realtimeSinceStartup - wall0 > 240f) { verdict = $"stopped at room {(c != null ? c.room + 1 : 0)} of {lv.rooms.Count}: the walk's budget ended it at {Now():0} s of game time (cap {budget:0} s of game time, 240 s of wall)"; break; }
                yield return null;
            }
            if (recRoom != null)
            {
                frames[recRoom] = FeelProbe.FrameCount;
                totalFrames += FeelProbe.FrameCount;
                yield return Stop();
            }
            Campaign24 end = g.sim.campaign;
            var rows = new List<string>();
            foreach (RoomDef r in lv.rooms)
            {
                string id = r.id;
                bool entered = enter.ContainsKey(id), cleared = clear.ContainsKey(id);
                string t = !entered ? "not entered" : cleared ? $"enter {enter[id]:0.0} s, clear {clear[id]:0.0} s (took {clear[id] - enter[id]:0.0} s, par {r.par:0})" : $"enter {enter[id]:0.0} s, NOT CLEARED (par {r.par:0})";
                string p = prompts.TryGetValue(id, out List<string> pl) ? string.Join(",", pl) : "none";
                int fr = frames.TryGetValue(id, out int n) ? n : 0;
                rows.Add($"{id} {r.name} [{r.kind}]: {t}; enemies spawned {(spawned.TryGetValue(id, out int sp) ? sp : 0)}; LIMIT {(banners.TryGetValue(id, out string b) ? b : "-")}; prompts {p}; {fr} frames"
                         + (bossShown.TryGetValue(id, out string bs) ? "; " + bs : ""));
            }
            int deaths = end != null ? end.deaths : 0;
            int clearedRooms = clear.Count;
            if (verdict == null) verdict = $"stopped at room {(end != null ? end.room + 1 : 0)} of {lv.rooms.Count}";
            res[0] = $"{pre0}; level {mapId}; {totalFrames} frames in {seenRecorded.Count} per-room recordings (dir/<room>, up to {LevelwalkFrameCap25} frames each); " + string.Join("; ", rows)
                   + $"; deaths {deaths} ({(deaths == 0 ? "every room first try" : "not every room first try")}), rooms cleared {clearedRooms} of {lv.rooms.Count}, {Now():0.0} s of game time | {verdict}";
        }
    }
}
