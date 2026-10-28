// NIGHT56 builder L (L2, NIGHT56.md section 5 L; the hook FeelReel.N56.cs placed by the shared commit): the Player phase `a156`, THE ROOM METER.
// Not run by its author (builders never run a Player). Boot one Player per level (bash Tools/night/n56-reels.sh <tag> l2on,n52-foundry,a156; L-062:
// a campaign level boots only via reel.sh + -skygrindReelOnly). For the loaded level, every fight room (kind room) with a `frames52.entry` spot is
// entered as a452 enters it (FeelReel.N52.Rooms.cs: DOORFREE, the camera at the entry spot, the frame 0.5 s after the room's first wave is up, at
// most 8 s), and ONE line per room is printed, exactly:
//   [reel] a156: <room> entry <file> | wave1 <n> | alive <a> | seen <s> | nearest <d1> | second <d2> | nearestAlive <dA> | drawn <r> | campaign <id>
//   seen          live enemies (stand-ins apart) whose body (+L56BodyH) or head (+L56HeadH) is inside the camera's view frustum (the camera's own
//                 viewport, not a452's 45 deg yaw cone) AND reached by the web collider's ray from the camera (GameWorld.RayIntersect: the same
//                 boxes, wedges and layers the sim's rays use; nothing hit before the target, L56RaySlack m of slack): what the frame SHOWS
//   nearest / second   the two smallest camera-to-body distances (m) among the seen enemies ("-" when fewer); nearestAlive: among every live one
//   drawn         the enemy rects FeelProbe recorded for the last rendered frame (FeelProbe.Enemy: each drawn enemy, spawn tell or not; FeelProbe.cs:116)
// The verdict is n56-room.py's, not this file's: `python -I Tools/night/n56-room.py agree <Player log>` reads these lines, applies each room's
// opening (Design/levels/n52/openings53.json: held / vista seen >= 4; ambushed / ambusher seen >= 1 and nearest <= 15 m) and prints the
// agreement with the checker's IN VIEW column (the L2 doneWhen: >= 80 % of the foundry / iceline / kiln rooms).
// The scenario's line `done: GREEN a156 | ...` when at least one room was framed, every framed room had a live wave in that room, and the screen is
// 1920x1080; RED otherwise. It changes nothing in the game: a reel phase only (the rooms are level data; no row).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelProbe
    {
        /// <summary>NIGHT56 L (a156): the last recorded frame's drawn enemy rects (FeelProbe.Enemy's, as RecordFrame stored them) into
        /// `into`; the count. 0 outside a recording.</summary>
        public static int N56LastEnemyRects(List<Rect> into)
        {
            into.Clear();
            if (!Recording || frames <= 0 || frameRecs == null || enemyRecs == null) return 0;
            FrameRec f = frameRecs[frames - 1];
            for (int i = 0; i < f.enemyCount && f.enemyStart + i < enemyRecs.Length; i++) into.Add(enemyRecs[f.enemyStart + i].rect);
            return into.Count;
        }
    }

    public static partial class FeelReel
    {
        static partial void N56Room(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = L56RoomScenario(g, dir, res, offs, pre);

        static readonly List<Rect> l56Rects = new List<Rect>();
        static readonly List<double> l56Seen = new List<double>();

        /// <summary>The frame's enemies: alive, seen (frustum + the collider ray from the camera), the seen distances (sorted), the nearest live one.</summary>
        static void L56Count(SurvivalGame g, out int alive, out int seen, out double nearestAlive)
        {
            alive = seen = 0;
            nearestAlive = double.PositiveInfinity;
            l56Seen.Clear();
            Camera cam = g.cam;
            if (cam == null) return;
            Vec3 eye = WebSpace.FromUnity(cam.transform.position);
            foreach (Enemy e in g.sim.enemies.all)
            {
                if (!e.alive || e.standIn29) continue;
                alive++;
                Vec3 bodyAt = e.position + new Vec3(0, UnityTuning.L56BodyH, 0);
                double dist = (bodyAt - eye).Length;
                if (dist < nearestAlive) nearestAlive = dist;
                bool vis = false;
                for (int k = 0; k < 2 && !vis; k++)
                {
                    Vec3 t = e.position + new Vec3(0, k == 0 ? UnityTuning.L56BodyH : UnityTuning.L56HeadH, 0);
                    Vector3 vp = cam.WorldToViewportPoint(WebSpace.ToUnity(t));
                    if (vp.z <= 0f || vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f) continue;
                    Vec3 d = t - eye;
                    double len = d.Length;
                    vis = len < 1e-6 || !g.world.RayIntersect(eye, d / len, out RayHit hit) || hit.distance >= len - UnityTuning.L56RaySlack;
                }
                if (!vis) continue;
                seen++;
                l56Seen.Add(dist);
            }
            l56Seen.Sort();
        }

        static IEnumerator L56RoomScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot the level with reel.sh + -skygrindMap <id> -skygrindReelOnly a156 (L-062)";
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "done: RED a156: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + "; " + boot + ")"; yield break; }
            LevelDef lv = sim.campaignLevel;
            string id = lv?.id ?? "?";
            God(g);
            g.forcedCombat = IdleCombat;
            Start(g, "a156", "camera", dir);
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
                string name = $"L56-{id}-{r.id}-entry";
                yield return Shot29(dir, name, st);
                framed++;
                string file = "shot-N29-" + name + ".png";
                int w1 = r.WaveCount > 0 ? r.waves[0].Count : 0;
                string line = $"{r.id} entry {file} | wave1 {w1} | alive {alive} | seen {seen} | nearest {m(l56Seen.Count > 0 ? l56Seen[0] : double.PositiveInfinity)}"
                    + $" | second {m(l56Seen.Count > 1 ? l56Seen[1] : double.PositiveInfinity)} | nearestAlive {m(nearA)} | drawn {drawn} | campaign {room}";
                Debug.Log("[reel] a156: " + line);
                lines.Add(line);
            }
            g.husks.KillAll();
            g.forcedMove = null;
            yield return Stop();
            bool screen = Screen.width == 1920 && Screen.height == 1080;
            bool green = framed > 0 && framed == rooms && wrongRoom == 0 && noWave == 0 && screen && ended == null;
            res[0] = $"done: {(green ? "GREEN" : "RED")} a156 | map {g.MapId}, level {id} | fight rooms framed {framed}/{rooms}, in the wrong room {wrongRoom}, with no wave {noWave}"
                + $" | screen {Screen.width}x{Screen.height}{(screen ? "" : " (NOT 1920x1080)")}{(ended != null ? " | ended: " + ended : "")}"
                + $" | fight rooms without frames52.entry (not framed): {(unframed.Count > 0 ? string.Join(", ", unframed) : "none")}"
                + " | the verdicts: python -I Tools/night/n56-room.py agree <this log> || " + string.Join(" || ", lines);
        }
    }
}
