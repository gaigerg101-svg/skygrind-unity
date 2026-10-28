// NIGHT72 builder N, slice N3: the reel phase stuck72 (registered by the shared commit: FeelReel.N72.cs declares N72StuckBody, FeelReel.N57.cs dispatches "stuck72"). goal72's proof, the L-014 / L-115
// way: a Player line that reads one way with the row OFF and the other ON, and prints that its input happened.
//   boot   reel.sh + -skygrindReelOnly stuck72 on a CAMPAIGN map (n52-foundry, or any level), -skygrindRows72 goal72:1 for the ON side (the plain config is the OFF side)
//   1      room 1 is cleared BY THE CAMPAIGN (a taught link: its lessons are marked done, as Campaign24's lessonCap does; a fight room: its enemies are killed), the reel's bot is detached (a bot's run is
//          sim.metaSilent and goal72 draws for a human's), the player stands at the room's spawn (the exit far away), god mode, the reel's own idle input (g.forcedMove). n72-t2 was RED on both sides
//          because this step did neither (room 1 not cleared in 90 s; the bot attached): the sim half is proven by `SimHarness n72goal --campaign n52-foundry`
//   A      a SCRIPTED player idle for Goal72Stuck72WindowS (22 s of game time) with the planted switch off: a reel never earns a hint (a verbs37 run shows none): the line shows (stage 1) and no hint
//   B      the planted stuck run (Goal72View.PlantedOverride): the same idle player: stage 2 at about 10 s, stage 3 at about 20 s, each line <= 6 words, >= 22 px; a frame of each stage
//   C      progress: the player put a few metres closer (a node of the room, or a point on the line exit -> spawn): the hint on screen is RETIRED (counted) and the stage falls back to 1
// Line: "<GREEN|RED> stuck72 | row on <bool> | scripted idle 22 s: max stage a, hints b | planted: stage 2 at t s '<text>', stage 3 at t s '<text>' (<n> words, <px> px) | progress: retired r, stage s | ..."
// GREEN: the row on, A shows the line and no hint, B reaches 2 and 3 at their times with <= 6 words and >= 22 px, C retires. With the row OFF nothing is ever visible: RED.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N72StuckBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Stuck72Scenario(g, dir, res, offs, pre);

        static IEnumerator Shot72N(string dir, string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = null;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) yield break;
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-N72-" + name + ".png"), t.EncodeToPNG());
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }

        static IEnumerator Stuck72Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot);
            if (!g.sim.campaignOn && N24CampaignGate(g) == null) { N24StartCampaign(g, offs); for (int w = 0; w < 5; w++) yield return null; }
            Campaign24 c = g.sim.campaignOn ? g.sim.campaign : null;
            int guard0 = 0;
            while (g.sim.campaignOn && g.sim.campaign == null && guard0++ < 600) yield return null;
            c = g.sim.campaignOn ? g.sim.campaign : null;
            if (c == null)
            {
                res[0] = pc + " | SKIPPED: no CAMPAIGN run (boot a campaign map: reel.sh <map> stuck72)";
                yield break;
            }
            offs.Add(() =>
            {
                Goal72View.PlantedOverride = null;
                g.forcedMove = null;
            });
            Start(g, "stuck72", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var fails = new List<string>();
            bool rowOn = NightRows.ViewOn(g.cfg, Goal72Math.Row);

            // 1: room 1 cleared BY THE CAMPAIGN, then the player at the room's spawn, the exit as far as the room allows.
            // n72-t2 (sfon / sfoff) read "max stage 0 ... room 1 was not cleared in 90 s": goal72 draws only for a cleared room (Goal72View: phase Cleared) and only for a HUMAN's run
            // (!sim.metaSilent), and this scenario broke both. (a) the foundry's room 1 is a taught LINK: it clears when its lessons are done, which an idle player and the cue reel's KillAll
            // never do; the lessons are marked done here the way Campaign24's own lessonCap does it (lesson.index = defs.Count) and the campaign clears the room on its next step (a fight room
            // is killed out as before). (b) N24StartCampaign leaves the AUTOPILOT attached and AutopilotInput sets sim.metaSilent: it is detached once the room is clear, so the run is a human's with
            // a scripted input (g.forcedMove). The sim half of both is proven headless: `SimHarness n72goal --campaign n52-foundry` (evidence/n3-stuck72-campaign.txt).
            int w0 = 0;
            while (c.room < 0 && w0++ < Ms(10000)) { God(g); yield return null; }
            if (c.room < 0 || c.Room == null)
            {
                yield return Stop();
                res[0] = $"RED stuck72 | {pc} | the campaign never entered room 1 in 10 s (room {c.room}, phase {c.phase})";
                yield break;
            }
            int n = 0;
            bool lessonsForced = false;
            while (c.phase != Campaign24.Phase.Cleared && n++ < Ms(30000))
            {
                God(g);
                if (c.lesson != null && !c.lesson.Done) { c.lesson.index = c.lesson.defs.Count; lessonsForced = true; }
                if (c.wave >= c.Room.WaveCount) g.husks.KillAll();
                else if (g.sim.enemies.AliveCount > 0) g.husks.KillAll();
                yield return null;
            }
            if (c.phase != Campaign24.Phase.Cleared) { fails.Add("room 1 was not cleared in 30 s"); }
            if (g.sim.input is AutopilotInput bot) { bot.Detach(); g.sim.UseInput(g.playerInput); } // the bot's run rules (metaSilent) go back: a human's run
            bool human = !g.sim.metaSilent;
            if (!human) fails.Add("sim.metaSilent is still on: goal72 draws for a human's run only");
            RoomDef room = c.Room;
            Vec3 exit = room.exit;
            Vec3 from = room.spawn;
            g.Respawn(from, room.spawnYawDeg);
            g.forcedMove = new InputFrame(); // the reel's own idle input: a scripted player
            yield return Frames(Ms(600));
            double distFar = Math.Sqrt((exit.x - g.Feet.x) * (exit.x - g.Feet.x) + (exit.z - g.Feet.z) * (exit.z - g.Feet.z));
            // the level's first-seconds rule: wait it out (the game seconds since the campaign began)
            int w5 = 0;
            while (c.steps * g.sim.step < UnityTuning.Goal72FirstLevelS + 0.5 && w5++ < Ms(20000)) { God(g); g.forcedMove = new InputFrame(); yield return null; }

            // A: a scripted player idle: the line, never a hint
            Goal72View.PlantedOverride = false;
            int maxStageA = 0;
            double t0 = g.sim.simStep * g.sim.step, tA = 0;
            int lines0 = Goal72Probe.Lines, h10a = Goal72Probe.Hint10, h20a = Goal72Probe.Hint20;
            bool shotA = false;
            while (tA < UnityTuning.Goal72Stuck72WindowS)
            {
                God(g);
                g.forcedMove = new InputFrame();
                yield return null;
                tA = g.sim.simStep * g.sim.step - t0;
                maxStageA = Math.Max(maxStageA, Goal72Probe.Stage);
                if (!shotA && Goal72Probe.Stage >= 1 && tA > 1.0) { shotA = true; yield return Shot72N(dir, "stage1-line"); }
            }
            int hintsA = (Goal72Probe.Hint10 - h10a) + (Goal72Probe.Hint20 - h20a);
            string lineA = Goal72Probe.TextNow;
            if (rowOn && maxStageA != 1) fails.Add($"the scripted idle reached stage {maxStageA}, not 1");
            if (hintsA != 0) fails.Add($"the scripted idle showed {hintsA} hint(s)");

            // B: the planted stuck run
            Goal72View.PlantedOverride = true;
            double tB0 = g.sim.simStep * g.sim.step, tB = 0, t2 = -1, t3 = -1;
            string text2 = "", text3 = "";
            int words2 = 0, words3 = 0;
            float px2 = 0, px3 = 0;
            bool shot2 = false, shot3 = false;
            while (tB < UnityTuning.Goal72Stuck72WindowS)
            {
                God(g);
                g.forcedMove = new InputFrame();
                yield return null;
                tB = g.sim.simStep * g.sim.step - tB0;
                if (Goal72Probe.Stage >= 2 && t2 < 0) { t2 = tB; text2 = Goal72Probe.TextNow; words2 = Goal72Probe.Words; px2 = Goal72Probe.PxNow; }
                if (Goal72Probe.Stage >= 3 && t3 < 0) { t3 = tB; text3 = Goal72Probe.TextNow; words3 = Goal72Probe.Words; px3 = Goal72Probe.PxNow; }
                if (t2 >= 0 && !shot2 && tB > t2 + 0.8) { shot2 = true; yield return Shot72N(dir, "stage2-where"); }
                if (t3 >= 0 && !shot3 && tB > t3 + 0.8) { shot3 = true; yield return Shot72N(dir, "stage3-how"); }
            }
            if (rowOn)
            {
                if (t2 < 0 || Math.Abs(t2 - UnityTuning.Goal72Hint1S) > 1.5) fails.Add($"stage 2 at {(t2 < 0 ? "never" : t2.ToString("0.0", ci) + " s")}, not about {UnityTuning.Goal72Hint1S:0} s");
                if (t3 < 0 || Math.Abs(t3 - UnityTuning.Goal72Hint2S) > 1.5) fails.Add($"stage 3 at {(t3 < 0 ? "never" : t3.ToString("0.0", ci) + " s")}, not about {UnityTuning.Goal72Hint2S:0} s");
                if (words2 > 6 || words3 > 6) fails.Add($"a hint over 6 words ({words2} / {words3})");
                if ((t2 >= 0 && px2 < 22f) || (t3 >= 0 && px3 < 22f)) fails.Add($"a hint under 22 px ({px2:0} / {px3:0})");
            }
            int h10 = Goal72Probe.Hint10 - h10a, h20 = Goal72Probe.Hint20 - h20a;

            // C: progress retires the hint: a node of the room clearly closer to the exit
            int retired0 = Goal72Probe.Retired;
            // the spots to try, closest to the exit first: the room's nodes (a fight room), else points on the line from the exit back to the spawn (a link has no nodes), each clearly closer than
            // the player is now and outside the hide radius; the first one the feet stay on (not respawned or dropped away) is the progress
            double distNow = Math.Sqrt((exit.x - g.Feet.x) * (exit.x - g.Feet.x) + (exit.z - g.Feet.z) * (exit.z - g.Feet.z));
            var spots = new List<Vec3>();
            foreach (Vec3 nd in room.nodes)
            {
                double dn = Math.Sqrt((exit.x - nd.x) * (exit.x - nd.x) + (exit.z - nd.z) * (exit.z - nd.z));
                if (dn < distNow - 2 * UnityTuning.Goal72ProgressM && dn > 8) spots.Add(nd);
            }
            spots.Sort((a, b) => ((exit.x - a.x) * (exit.x - a.x) + (exit.z - a.z) * (exit.z - a.z)).CompareTo((exit.x - b.x) * (exit.x - b.x) + (exit.z - b.z) * (exit.z - b.z)));
            double ux = from.x - exit.x, uz = from.z - exit.z, ul = Math.Sqrt(ux * ux + uz * uz);
            if (ul > 1e-6)
                foreach (double back in new[] { 9.0, 12.0, 16.0 })
                    if (back < distNow - 2 * UnityTuning.Goal72ProgressM) spots.Add(new Vec3(exit.x + ux / ul * back, from.y, exit.z + uz / ul * back));
            int stageAfter = Goal72Probe.Stage;
            int tried = 0;
            string spotNote = "none";
            foreach (Vec3 sp in spots)
            {
                tried++;
                g.Respawn(sp, room.spawnYawDeg);
                for (int i = 0; i < Ms(1500); i++) { God(g); g.forcedMove = new InputFrame(); yield return null; }
                stageAfter = Goal72Probe.Stage;
                double moved = Math.Sqrt((g.Feet.x - sp.x) * (g.Feet.x - sp.x) + (g.Feet.z - sp.z) * (g.Feet.z - sp.z));
                spotNote = $"({sp.x.ToString("0.0", ci)}, {sp.y.ToString("0.0", ci)}, {sp.z.ToString("0.0", ci)}) feet ended {moved.ToString("0.0", ci)} m off it";
                if (Goal72Probe.Retired > retired0 || !rowOn) break;
            }
            bool haveNear = tried > 0;
            if (haveNear)
            {
                if (rowOn && Goal72Probe.Retired <= retired0) fails.Add($"a closer position did not retire the hint ({tried} spot(s) tried, the last {spotNote})");
                if (rowOn && stageAfter > 1) fails.Add($"after progress the stage is {stageAfter}, not 1");
            }
            else if (rowOn) fails.Add("no spot of the room is clearly closer to the exit (progress not checked)");
            if (!rowOn) fails.Add("goal72 is OFF here (-skygrindRows72 goal72:1 turns it on): nothing visible, as it should be");
            Goal72View.PlantedOverride = null;
            g.forcedMove = null;
            yield return Frames(Ms(300));
            yield return Stop();
            res[0] = $"{(fails.Count == 0 ? "GREEN" : "RED")} stuck72 | {pc} | row on {rowOn} | room 1 '{room.id}' ({room.kind}) cleared by the campaign {c.phase == Campaign24.Phase.Cleared || c.room > 0} (lessons forced done {lessonsForced}), human run {human} (bot detached; metaSilent {g.sim.metaSilent}), flow {g.flow}, attract {g.attractOn} | the exit {distFar.ToString("0", ci)} m away | scripted idle {UnityTuning.Goal72Stuck72WindowS:0} s: max stage {maxStageA}, hints {hintsA}, line '{lineA}' | planted: stage 2 at {(t2 < 0 ? "never" : t2.ToString("0.0", ci) + " s")} '{text2}' ({words2} words, {px2.ToString("0", ci)} px), stage 3 at {(t3 < 0 ? "never" : t3.ToString("0.0", ci) + " s")} '{text3}' ({words3} words, {px3.ToString("0", ci)} px), hints shown 10 s {h10}, 20 s {h20} | progress: retired {Goal72Probe.Retired - retired0}, stage {stageAfter}, closer spots tried {tried} (the last {spotNote}) | {Goal72Probe.Line()}"
                + (fails.Count == 0 ? "" : " | " + string.Join("; ", fails));
        }
    }
}
