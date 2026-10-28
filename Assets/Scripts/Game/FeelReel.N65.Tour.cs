// NIGHT65 builder T (T2 tour65; NIGHT65.md section 3; slate T2): the Player phase `tour65`, every room's CARD POSES and its VERB LINE framed with
// wave 1 live, so the reviewers see each rebuilt room from its card's own spots (verbs37's fixed frames are often corridors, a157 frames only
// rooms with frames52.entry). Not run by its author (builders never run a Player; NIGHT65 branch C: no Player tonight unless the disk allows).
// Boot one Player per level (L-062: a campaign level boots only via reel.sh + -skygrindMap; the row foundry65 need not be on: -skygrindMap picks
// the file): `bash Tools/night/n65-reels.sh <tag> tour,n65-foundry,tour65` (or n52-foundry for the red).
// For the loaded level, every fight room (kind room), and any other room whose row has frames65, is entered as a157 enters it (DOORFREE, the
// camera at its first pose, the frames once the room's first wave is up and settled), then:
//   - each pose of the row's `frames65` ({name: [x, y, z, yawDeg, pitchRad]}, keys starting "_" skipped), or, with no frames65, the fallback
//     `frames36.mid` (every room of n52-foundry has it), framed, one line per pose:
//       [reel] tour65: <room> pose <name> (<frames65|frames36.mid|planted>) <file> | feet (x, y, z) yaw <deg> pitch <rad> | camera (x, y, z)
//       | alive <a> seen <s> | campaign <room>[ | WARN inside a solid ...]
//   - the row's `line65` ([[x, y, z], ...] waypoints, or {points: [...]}) resampled into 8 frames by arc length, each looking along the line,
//     one line per frame (`<room> line65 <i>/8 ...`) and one for the sequence (`<room> line65: 8 frames <first>..<last> | length <m> m`)
//   - `-skygrindTourPlant65=<room>:x/y/z/yawDeg/pitchRad` (one token: n65-reels.sh splits a side's extra args on commas; e.g.
//     `tour,n65-foundry,tour65,-skygrindTourPlant65=a2:-4/0/-52.5/0/0`, inside a2's north deck) adds a pose "planted" to that room (the proof's planted case); a pose whose feet
//     or eye is inside a collision box of the level prints WARN (the camera in a wall).
// The scenario's line: `done: GREEN tour65 | ...` when every toured room was entered in its own room with a live wave, every pose framed
// on a 1920x1080 screen, no toured room left with no pose and no pose inside a solid (a box or, NIGHT66 T1a, a wedge): Tour65.Green, S3 of
// Research/2026-10-23-forge66/code-scout.md; RED otherwise. A reel phase only: it changes nothing in the game (verbs37 and a157 untouched).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static IEnumerator Tour65Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot the level with reel.sh + -skygrindMap <id> (bash Tools/night/n65-reels.sh <tag> tour,n65-foundry,tour65; L-062)";
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "done: RED tour65: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + "; " + boot + ")"; yield break; }
            LevelDef lv = sim.campaignLevel;
            string id = lv?.id ?? "?";
            God(g);
            g.forcedCombat = IdleCombat;
            Start(g, "tour65", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var rows = Levels35Rows(g);
            List<double[]> solids = Tour65.Solids(g.map?.json);
            var plant = Tour65.Plant(Environment.GetCommandLineArgs());
            var st = new double[4];
            var lines = new List<string>();
            var unframed = new List<string>();
            var fallback = new List<string>();
            int rooms = 0, poses = 0, framed = 0, lineFrames = 0, lineRooms = 0, warns = 0, wrongRoom = 0, noWave = 0;
            string ended = null;
            Func<double, string> f1 = v => v.ToString("0.0", ci);
            foreach (RoomDef r in lv.rooms)
            {
                rows.TryGetValue(r.id, out var row);
                List<Tour65.Pose> list = Tour65.Poses(row, out string source);
                bool planted = plant.HasValue && plant.Value.room == r.id;
                if (planted) list.Add(plant.Value.pose);
                if (!Tour65.Toured(r.kind, source, planted)) continue; // the fight rooms, and any room whose card gave it poses
                if (list.Count == 0) { unframed.Add(r.id); continue; }
                if (source == "frames36.mid") fallback.Add(r.id);
                rooms++;
                poses += list.Count;
                g.husks.KillAll();
                yield return Frames(4);
                K52Place(g, list[0].Spot);
                float end = Time.realtimeSinceStartup + UnityTuning.L56RoomWaitS;
                while (K52Room(g) != r.id && Time.realtimeSinceStartup < end) { God(g); yield return null; }
                double t0 = sim.SimTime;
                bool waves = r.WaveCount > 0; // a link toured for its frames65 may have no wave: nothing to wait for
                while (waves && sim.enemies.AliveCount == 0 && sim.SimTime - t0 < UnityTuning.L56WaveWaitS && (ended = Ended30(g)) == null) { God(g); yield return null; }
                if (waves && sim.enemies.AliveCount == 0) noWave++;
                double t1 = sim.SimTime;
                while (sim.SimTime - t1 < UnityTuning.L56SettleS && (ended = Ended30(g)) == null) { God(g); yield return null; }
                if (ended != null) break;
                var shots = new List<Tour65.Pose>(list);
                List<Tour65.Pose> lf = Tour65.LineFrames(Tour65.Line(row), UnityTuning.T65LineFrames);
                shots.AddRange(lf);
                string firstLine = null, lastLine = null;
                foreach (Tour65.Pose p in shots)
                {
                    K52Place(g, p.Spot);
                    yield return null; // the respawn's camera cut lands
                    yield return null; // one rendered frame at the pose
                    God(g);
                    g.UiView?.ClearBanners41();
                    string room = K52Room(g);
                    if (room != r.id) wrongRoom++;
                    L56Count(g, out int alive, out int seen, out double nearA);
                    string name = $"T65-{id}-{r.id}-{p.name}";
                    yield return Shot29(dir, name, st);
                    string file = "shot-N29-" + name + ".png";
                    Vec3 cam = g.cam != null ? WebSpace.FromUnity(g.cam.transform.position) : new Vec3(double.NaN, double.NaN, double.NaN);
                    string inside = Tour65.InsideSolid(solids, p);
                    if (inside != null) warns++;
                    string what = p.source == "line65" ? $"line65 {p.name.Substring(4)}/{lf.Count}" : $"pose {p.name} ({p.source})";
                    string line = $"{r.id} {what} {file} | feet ({f1(p.x)}, {f1(p.y)}, {f1(p.z)}) yaw {p.yawDeg.ToString("0", ci)} pitch {p.pitch.ToString("0.00", ci)}"
                        + $" | camera ({f1(cam.x)}, {f1(cam.y)}, {f1(cam.z)}) | alive {alive} seen {seen} | campaign {room}"
                        + (inside != null ? $" | WARN inside a solid ({inside}): the camera in a wall" : "");
                    Debug.Log("[reel] tour65: " + line);
                    lines.Add(line);
                    if (p.source == "line65") { lineFrames++; if (firstLine == null) firstLine = file; lastLine = file; }
                    else framed++;
                }
                if (lf.Count > 0)
                {
                    lineRooms++;
                    List<double[]> pts = Tour65.Line(row);
                    double len = 0;
                    for (int k = 1; k < pts.Count; k++) { double dx = pts[k][0] - pts[k - 1][0], dy = pts[k][1] - pts[k - 1][1], dz = pts[k][2] - pts[k - 1][2]; len += Math.Sqrt(dx * dx + dy * dy + dz * dz); }
                    string seq = $"{r.id} line65: {lf.Count} frames {firstLine} .. {lastLine} | {pts.Count} waypoints, length {f1(len)} m";
                    Debug.Log("[reel] tour65: " + seq);
                    lines.Add(seq);
                }
                else Debug.Log($"[reel] tour65: {r.id} line65: none in the row (no sequence)");
            }
            g.husks.KillAll();
            g.forcedMove = null;
            yield return Stop();
            bool screen = Screen.width == 1920 && Screen.height == 1080;
            bool green = Tour65.Green(rooms, unframed.Count, warns, wrongRoom, noWave, screen, ended != null); // NIGHT66 T1a (S3): unframed 0 and WARN 0 needed
            res[0] = $"done: {(green ? "GREEN" : "RED")} tour65 | map {g.MapId}, level {id} | rooms toured {rooms}, poses framed {framed}/{poses}, line65 frames {lineFrames} in {lineRooms} room(s)"
                + $" | on the fallback frames36.mid: {(fallback.Count > 0 ? string.Join(", ", fallback) : "none")} | WARN (a pose inside a solid) {warns}"
                + $" | in the wrong room {wrongRoom}, with no wave {noWave} | screen {Screen.width}x{Screen.height}{(screen ? "" : " (NOT 1920x1080)")}{(ended != null ? " | ended: " + ended : "")}"
                + $" | rooms with no pose (not framed): {(unframed.Count > 0 ? string.Join(", ", unframed) : "none")}{(plant.HasValue ? " | planted pose in " + plant.Value.room : "")} || " + string.Join(" || ", lines);
        }
    }
}
