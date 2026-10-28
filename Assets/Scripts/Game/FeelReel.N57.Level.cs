// NIGHT57 builder L (row kiln57; NIGHT57.md section 5 L; the hook FeelReel.N57.cs placed by the shared commit): the Player phase `a157`, LEVEL 3 AS
// n57-kiln, room by room. Not run by its author (builders never run a Player). Boot one Player per side (L-062: a campaign level boots only via
// reel.sh + -skygrindMap): `bash Tools/night/n47-reels.sh <tag> l1on,n57-kiln,a157` (the row on) and `l1off,n52-kiln,a157,-skygrindOff57` (NIGHT56's).
// For the loaded level, every fight room (kind room) with a `frames52.entry` spot is entered as a156 / a452 enter it (FeelReel.N56.Room.cs,
// FeelReel.N52.Rooms.cs: DOORFREE, the camera at the entry spot, the frame once the room's first wave is up and settled), and ONE line per room:
//   [reel] a157: <room> "<card>" entry <file> | wave1 <n> | alive <a> | seen <s> | nearest <d1> | nearestAlive <dA> | drawn <r> | campaign <id>
//   | beats glimpse <g> pass <p> cameo <c>
// card: the room row's `card57` (the identity card's title; "n52" for a slot still holding n52's room); seen / nearest / drawn: a156's meter
// (L56Count: the frustum AND the web collider's ray from the camera; FeelProbe's drawn rects); beats: kiln47's counters (SurvivalSim.N47.Kiln.cs)
// so far in the run (the harness proof is `n47b beats --level n57-kiln`). The scenario's line: `done: GREEN a157 | ...` when every fight room was
// framed in its own room with a live wave on a 1920x1080 screen; RED otherwise. A reel phase only: it changes nothing in the game.
// NIGHT60 (the lead, backlog B1): with `-skygrindSpawnShots60` each room is ALSO framed from its spawn pose (RoomDef.spawn / spawnYawDeg: the
// view after a death in that room; the entry spot is the view on first entry), with the same live wave, one more line per room:
//   [reel] a157: <room> "<card>" spawn <file> | at (<x>, <z>) yaw <deg> | alive <a> | seen <s> | nearestAlive <dA> | drawn <r> | campaign <id>
// Without the flag a157 is unchanged.
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
        static partial void N57Level(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = L57LevelScenario(g, dir, res, offs, pre);

        /// <summary>NIGHT60 (B1): `-skygrindSpawnShots60` adds a frame from each room's spawn pose to a157 (a reel option, not a game change).</summary>
        static bool SpawnShots60 => Array.IndexOf(Environment.GetCommandLineArgs(), "-skygrindSpawnShots60") >= 0;

        static IEnumerator L57LevelScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot the level with reel.sh + -skygrindMap <id> (bash Tools/night/n47-reels.sh <tag> l1on,n57-kiln,a157; L-062)";
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "done: RED a157: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + "; " + boot + ")"; yield break; }
            LevelDef lv = sim.campaignLevel;
            string id = lv?.id ?? "?";
            God(g);
            g.forcedCombat = IdleCombat;
            Start(g, "a157", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var rows = Levels35Rows(g);
            var st = new double[4];
            var lines = new List<string>();
            var unframed = new List<string>();
            int rooms = 0, framed = 0, wrongRoom = 0, noWave = 0;
            string ended = null;
            Func<double, string> m = v => double.IsInfinity(v) ? "-" : v.ToString("0.0", ci);
            foreach (RoomDef r in lv.rooms)
            {
                if (r.kind != "room") continue;
                double[] fe = K52Frame(rows, r.id, "entry");
                if (fe == null) { unframed.Add(r.id); continue; }
                rooms++;
                string card = rows.TryGetValue(r.id, out var row) && row.TryGetValue("card57", out object co) && co is string cs ? cs : "n52";
                g.husks.KillAll();
                yield return Frames(4);
                K52Place(g, fe);
                float end = Time.realtimeSinceStartup + UnityTuning.L56RoomWaitS;
                while (K52Room(g) != r.id && Time.realtimeSinceStartup < end) { God(g); yield return null; }
                double t0 = sim.SimTime;
                while (sim.enemies.AliveCount == 0 && sim.SimTime - t0 < UnityTuning.L56WaveWaitS && (ended = Ended30(g)) == null) { God(g); yield return null; }
                bool wave = sim.enemies.AliveCount > 0;
                if (!wave) noWave++;
                double t1 = sim.SimTime;
                while (sim.SimTime - t1 < UnityTuning.L56SettleS && (ended = Ended30(g)) == null) { God(g); yield return null; }
                if (ended != null) break;
                g.UiView?.ClearBanners41();
                yield return null; // one rendered frame at the spot: the probe's rects are this view's
                God(g);
                string room = K52Room(g);
                if (room != r.id) wrongRoom++;
                L56Count(g, out int alive, out int seen, out double nearA);
                int drawn = FeelProbe.N56LastEnemyRects(l56Rects);
                string name = $"L57-{id}-{r.id}-entry";
                yield return Shot29(dir, name, st);
                framed++;
                string file = "shot-N29-" + name + ".png";
                int w1 = r.WaveCount > 0 ? r.waves[0].Count : 0;
                string line = $"{r.id} \"{card}\" entry {file} | wave1 {w1} | alive {alive} | seen {seen} | nearest {m(l56Seen.Count > 0 ? l56Seen[0] : double.PositiveInfinity)}"
                    + $" | nearestAlive {m(nearA)} | drawn {drawn} | campaign {room} | beats glimpse {sim.beats47Glimpse} pass {sim.beats47Pass} cameo {sim.beats47Cameo}";
                Debug.Log("[reel] a157: " + line);
                lines.Add(line);
                if (SpawnShots60)
                {
                    K52Place(g, new[] { r.spawn.x, r.spawn.y, r.spawn.z, r.spawnYawDeg, 0.0 });
                    yield return null; // the respawn's camera cut lands
                    yield return null; // one rendered frame at the spawn: the probe's rects are this view's
                    God(g);
                    g.UiView?.ClearBanners41();
                    L56Count(g, out int aliveS, out int seenS, out double nearS);
                    int drawnS = FeelProbe.N56LastEnemyRects(l56Rects);
                    string nameS = $"L57-{id}-{r.id}-spawn";
                    yield return Shot29(dir, nameS, st);
                    string lineS = $"{r.id} \"{card}\" spawn shot-N29-{nameS}.png | at ({r.spawn.x.ToString("0.0", ci)}, {r.spawn.z.ToString("0.0", ci)})"
                        + $" yaw {r.spawnYawDeg.ToString("0", ci)} | alive {aliveS} | seen {seenS} | nearestAlive {m(nearS)} | drawn {drawnS} | campaign {K52Room(g)}";
                    Debug.Log("[reel] a157: " + lineS);
                }
                if (ClearShots60) yield return L60ClearShot(g, r, fe, id, dir, st, lines); // NIGHT60 (B8): the room's clear frame, its exit lit or not (FeelReel.N60.Lamp.cs)
                if (ClearShots60 && Exit63Math.ShotRoom(Environment.GetCommandLineArgs()) != null) yield return L63ExitShot(g, r, id, dir, st, lines); // NIGHT63 V exit63's SPOT pose: the door from its axis (FeelReel.N63V.Exit.cs; nothing without -skygrindExitShots63)
            }
            g.husks.KillAll();
            g.forcedMove = null;
            yield return Stop();
            bool screen = Screen.width == 1920 && Screen.height == 1080;
            bool green = framed > 0 && framed == rooms && wrongRoom == 0 && noWave == 0 && screen && ended == null;
            res[0] = $"done: {(green ? "GREEN" : "RED")} a157 | map {g.MapId}, level {id} | fight rooms framed {framed}/{rooms}, in the wrong room {wrongRoom}, with no wave {noWave}"
                + $" | screen {Screen.width}x{Screen.height}{(screen ? "" : " (NOT 1920x1080)")}{(ended != null ? " | ended: " + ended : "")}"
                + $" | fight rooms without frames52.entry (not framed): {(unframed.Count > 0 ? string.Join(", ", unframed) : "none")}"
                + " | the IN VIEW verdicts: python -I Tools/night/n57-l-room.py table || " + string.Join(" || ", lines);
        }
    }
}
