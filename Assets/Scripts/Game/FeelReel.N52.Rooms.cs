// NIGHT52 builder K (NIGHT52-BRIEF.md section 3, "The room frames reel"; NIGHT52.md section 2 item 8): the Player phase `a452` (dispatched by
// FeelReel.N52.cs), the room frames. Not run by its author (builders never run a Player). Boot one Player per level with the level row forced on in
// the build copy (bash Tools/night/n47-reels.sh <tag> k1,n52-kiln,a452; L-062: a campaign level boots only via reel.sh + -skygrindReelOnly).
// For the loaded level, every room row with a `frames52` object {entry, mid, high} (each [x, y, z, yawDeg, pitch]: feet coordinates, the yaw in
// degrees (0 = -z), the pitch in rad, up +, as frames36) gives three frames, in room order:
//   entry  the camera at `entry` (the room's door just crossed); the frame waits until the campaign is in that room and its first wave is up
//          (a live enemy, then 0.5 s for the rest of the wave to leave its spawn tell; at most 8 s);
//   mid    the camera at `mid` after 1.5 s more of the fight (god mode, nothing pressed: the enemies close in);
//   high   the camera at `high` (standing on the room's highest tier), 0.6 s to settle.
// A campaign run, DOORFREE (each room entered from its zone, as a245 / a346 / a449), the player kept alive, nothing pressed; between rooms the
// bodies are cleared. One line per frame, exactly `[reel] a452: <room> entry|mid|high <file>`, and the scenario's line `done: ...` at the end:
// GREEN when at least one room has frames52, every such room gave its three frames with the campaign in that room, the entry frame had a live
// wave, and the screen is 1920x1080; RED otherwise (RED with no frames52 in the level: nothing to frame). A fight room (kind room) without
// frames52 is listed (not framed); links and the boss room are never entered (a boss room's entry would start its fight).
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
        static partial void N52Rooms(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = K52RoomsScenario(g, dir, res, offs, pre);

        /// <summary>A room row's frames52 spot (entry / mid / high) as [x, y, z, yawDeg, pitch], or null.</summary>
        static double[] K52Frame(Dictionary<string, Dictionary<string, object>> rows, string room, string key)
        {
            if (!rows.TryGetValue(room, out var r) || !r.TryGetValue("frames52", out object fo) || !(fo is Dictionary<string, object> f)
                || !f.TryGetValue(key, out object v) || !(v is List<object> a) || a.Count < 4) return null;
            return new[] { PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2]), PortData.Num(a[3]), a.Count > 4 ? PortData.Num(a[4]) : 0.0 };
        }

        /// <summary>The campaign's current room id ("?" when none).</summary>
        static string K52Room(SurvivalGame g)
        {
            Campaign24 c = g.sim.campaign;
            LevelDef lv = g.sim.campaignLevel;
            return c != null && lv != null && c.room >= 0 && c.room < lv.rooms.Count ? lv.rooms[c.room].id : "?";
        }

        /// <summary>The camera at a frames52 spot: the feet there, the look along yawDeg / pitch, held there by the forced move (nothing pressed).</summary>
        static void K52Place(SurvivalGame g, double[] f)
        {
            double yaw = f[3] * Math.PI / 180;
            g.Respawn(new Vec3(f[0], f[1] + 0.05, f[2]), f[3]);
            g.input.SetLook(yaw, f[4]);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = f[4] };
            g.forcedCombat = IdleCombat;
        }

        /// <summary>The live enemies (stand-ins apart) and those within 45 deg of the look's yaw.</summary>
        static void K52Count(SurvivalGame g, double yawDeg, out int alive, out int inView)
        {
            alive = inView = 0;
            Vec3 at = g.sim.mover.position;
            double yaw = yawDeg * Math.PI / 180, fx = -Math.Sin(yaw), fz = -Math.Cos(yaw);
            foreach (Enemy e in g.sim.enemies.all)
            {
                if (!e.alive || e.standIn29) continue;
                alive++;
                double dx = e.position.x - at.x, dz = e.position.z - at.z, d = Math.Sqrt(dx * dx + dz * dz);
                if (d < 1e-6 || (dx * fx + dz * fz) / d >= Math.Cos(Math.PI / 4)) inView++;
            }
        }

        static IEnumerator K52RoomsScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot the level with reel.sh + -skygrindMap <id> -skygrindReelOnly a452, its level row on in the build copy (L-062)";
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "done: RED a452: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + "; " + boot + ")"; yield break; }
            LevelDef lv = sim.campaignLevel;
            string id = lv?.id ?? "?";
            God(g);
            g.forcedCombat = IdleCombat;
            Start(g, "a452", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var rows = Levels35Rows(g);
            var st = new double[4];
            var shots = new List<string>();
            var notes = new List<string>();
            var unframed = new List<string>();
            int rooms = 0, framed = 0, want = 0, wrongRoom = 0, noWave = 0;
            string ended = null;
            foreach (RoomDef r in lv.rooms)
            {
                double[] fe = K52Frame(rows, r.id, "entry"), fm = K52Frame(rows, r.id, "mid"), fh = K52Frame(rows, r.id, "high");
                if (fe == null || fm == null || fh == null)
                {
                    if (r.kind == "room") unframed.Add(r.id);
                    continue;
                }
                if (r.kind == "boss") { notes.Add($"{r.id}: a boss room's frames52 are not taken (its entry starts the fight)"); continue; }
                rooms++;
                want += 3;
                g.husks.KillAll();
                yield return Frames(4);
                // entry: the door just crossed, the room's first wave up
                K52Place(g, fe);
                float end = Time.realtimeSinceStartup + 6;
                while (K52Room(g) != r.id && Time.realtimeSinceStartup < end) { God(g); yield return null; }
                double t0 = sim.SimTime;
                while (sim.enemies.AliveCount == 0 && sim.SimTime - t0 < 8 && (ended = Ended30(g)) == null) { God(g); yield return null; }
                bool wave = sim.enemies.AliveCount > 0;
                if (!wave) noWave++;
                double t1 = sim.SimTime;
                while (sim.SimTime - t1 < 0.5 && (ended = Ended30(g)) == null) { God(g); yield return null; }
                if (ended != null) break;
                g.UiView?.ClearBanners41();
                string room = K52Room(g);
                if (room != r.id) wrongRoom++;
                string[] names = { "entry", "mid", "high" };
                double[][] spots = { fe, fm, fh };
                for (int k = 0; k < 3 && ended == null; k++)
                {
                    if (k == 1)
                    {
                        K52Place(g, fm);
                        double t2 = sim.SimTime;
                        while (sim.SimTime - t2 < 1.5 && (ended = Ended30(g)) == null) { God(g); yield return null; }
                    }
                    else if (k == 2)
                    {
                        K52Place(g, fh);
                        double t3 = sim.SimTime;
                        while (sim.SimTime - t3 < 0.6 && (ended = Ended30(g)) == null) { God(g); yield return null; }
                    }
                    if (ended != null) break;
                    God(g);
                    K52Count(g, spots[k][3], out int alive, out int inView);
                    string name = $"K52-{id}-{r.id}-{names[k]}";
                    yield return Shot29(dir, name, st);
                    framed++;
                    string file = "shot-N29-" + name + ".png";
                    Debug.Log($"[reel] a452: {r.id} {names[k]} {file}");
                    shots.Add($"{ShotLine29(name, st, ci)}, campaign room {K52Room(g)}, alive {alive}, in view {inView}, feet ({sim.mover.position.x.ToString("0.0", ci)}, {sim.mover.position.y.ToString("0.0", ci)}, {sim.mover.position.z.ToString("0.0", ci)})");
                }
                if (ended != null) break;
            }
            g.husks.KillAll();
            g.forcedMove = null;
            yield return Stop();
            bool screen = Screen.width == 1920 && Screen.height == 1080;
            bool green = rooms > 0 && framed == want && wrongRoom == 0 && noWave == 0 && screen && ended == null;
            res[0] = $"done: {(green ? "GREEN" : "RED")} a452 | map {g.MapId}, level {id} | rooms with frames52 {rooms}, frames {framed}/{want}, entry frames in the wrong room {wrongRoom}, entries with no wave {noWave}"
                + $" | screen {Screen.width}x{Screen.height}{(screen ? "" : " (NOT 1920x1080)")}{(ended != null ? " | ended: " + ended : "")}"
                + $" | fight rooms without frames52 (not framed): {(unframed.Count > 0 ? string.Join(", ", unframed) : "none")}{(rooms == 0 ? " (no room has frames52: nothing to frame)" : "")}"
                + (notes.Count > 0 ? " | " + string.Join("; ", notes) : "") + " || " + string.Join(" || ", shots);
        }
    }
}
