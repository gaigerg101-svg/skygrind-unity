// NIGHT46 builder A2 (NIGHT46.md card A2; docs/ROOMS.md's NIGHT46 identity cards, level 5): the Player phase `a246`: level 5's v2 THE FALLEN BELL and
// v6 THE ROPE CHOIR as rebuilt in n46-spire, framed for the lead's ON decision (NIGHT46.md method 3 / 6: from the lip or the door, mid-fight, after the
// state change; the greyscale check from the mid frames). Boot one Player on level 5 (reel.sh + -skygrindMap n46-spire + -skygrindReelOnly a246; L-062),
// or a build whose copy has design.n46.spire46 on. A copy of NIGHT45's a245 (FeelReel.N45.Mix.cs) with A2's rooms: a CAMPAIGN run, DOORFREE, the
// player kept alive (God), every frame from a placed camera (no pilot: L-046 / L-053), bodies placed at the rooms' own waveNodes43 spots.
//   v2: A246-v2-lip (the lip: the bell 10 m ahead, wave 1 round it), A246-v2-mid-0 / -1 (the south lane, the files coming round both sides of the bell,
//       the spitters on the aisles' north ends), A246-v2-tank (wave 3: the TANK and its swarmers at the exit apse, from the east lane), and the room's
//       frames36 (A246-v2-frames-mid / -exit).
//   v6: A246-v6-door (the door: wave 1 standing on the cantors' island, the organ beyond), A246-v6-mid-0 / -1 (on the island: the spitter on the high
//       stalls, the drones, the TANK in the north lane), A246-v6-drop (wave 3 on the organ loft and at the exit apse, from the island), and frames36.
// Line: GREEN when the run is on n46-spire and all 12 frames were saved. Not run by its author (builders never run a Player).
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N46Spire(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Spire46Scenario(g, dir, res, offs, pre);

        /// <summary>A2: a room row of the booted map's campaign block (null when absent).</summary>
        static Dictionary<string, object> Spire46Row(SurvivalGame g, string id)
        {
            if (g.map == null || g.map.json == null || !g.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return null;
            if (!camp.TryGetValue("rooms", out object ro) || !(ro is List<object> rows)) return null;
            foreach (object o in rows)
                if (o is Dictionary<string, object> r && r.TryGetValue("id", out object v) && v as string == id) return r;
            return null;
        }

        /// <summary>A2: stand at (x, y, z) looking along yawDeg / pitch (rad), wait `settle` frames, save the frame.</summary>
        static IEnumerator Spire46Shot(SurvivalGame g, string dir, string name, Vec3 at, double yawDeg, double pitch, int settle, int[] frames, bool save = true)
        {
            double yaw = yawDeg * Math.PI / 180;
            g.Respawn(at + new Vec3(0, 0.05, 0), yawDeg);
            g.input.SetLook(yaw, pitch);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
            yield return Frames(settle);
            God(g);
            if (!save) yield break;
            yield return Shot30(dir, name);
            frames[0]++;
        }

        /// <summary>A2: hold the camera where it is for `seconds` of real time (God every frame), then save the frame.</summary>
        static IEnumerator Spire46Hold(SurvivalGame g, string dir, string name, double seconds, int[] frames)
        {
            double t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < seconds)
            {
                God(g);
                yield return null;
            }
            yield return Shot30(dir, name);
            frames[0]++;
        }

        /// <summary>A2: wave w's waveNodes43 spots of a room (empty when absent).</summary>
        static List<Vec3> Spire46Wave(RoomDef r, int w) => r.WaveNodes43(w) ?? new List<Vec3>();

        /// <summary>A2: spawn the kinds at the spots in order (a kind the husk pool cannot make is skipped and named).</summary>
        static void Spire46Put(SurvivalGame g, string[] kinds, List<Vec3> spots, Vec3 player, List<string> placed)
        {
            for (int i = 0; i < kinds.Length && spots.Count > 0; i++)
            {
                Husk h = g.husks.Spawn(kinds[i], spots[i % spots.Count], player);
                placed.Add(h != null ? kinds[i] : kinds[i] + "(not placed)");
            }
        }

        static IEnumerator Spire46Frames36(SurvivalGame g, string dir, string room, int[] frames)
        {
            Dictionary<string, object> row = Spire46Row(g, room);
            if (row == null || !row.TryGetValue("frames36", out object fo) || !(fo is Dictionary<string, object> f36)) yield break;
            foreach (string which in new[] { "mid", "exit" })
                if (f36.TryGetValue(which, out object vo))
                {
                    List<object> v = PortData.Arr(vo);
                    yield return Spire46Shot(g, dir, $"A246-{room}-frames-{which}", new Vec3(PortData.Num(v[0]), PortData.Num(v[1]), PortData.Num(v[2])), PortData.Num(v[3]), PortData.Num(v[4]), 20, frames);
                }
        }

        static IEnumerator Spire46Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED a246: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + ")"; yield break; }
            string id = sim.campaignLevel?.id;
            if (sim.campaignLevel == null || NightRows.BaseId(sim.cfg, id) != "n41-spire")
            {
                res[0] = $"RED a246: the run is on {id ?? "?"}, not n46-spire (boot -skygrindMap n46-spire, L-062)";
                yield break;
            }
            RoomDef v2 = sim.campaignLevel.rooms.Find(q => q.id == "v2");
            RoomDef v6 = sim.campaignLevel.rooms.Find(q => q.id == "v6");
            if (v2 == null || v6 == null) { res[0] = "RED a246: no room v2 / v6"; yield break; }
            g.forcedCombat = IdleCombat;
            God(g);
            Start(g, "a246", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var frames = new int[1];
            var notes = new List<string>();
            var placed = new List<string>();

            // ---- v2 THE FALLEN BELL ----
            g.husks.KillAll();
            var lip = new Vec3(-10, 2.4, 75);
            yield return Spire46Shot(g, dir, "A246-v2-lip-pre", lip, 0, -0.12, 20, frames, false);
            Spire46Put(g, new[] { "husk", "husk", "husk", "swarmer", "swarmer" }, Spire46Wave(v2, 0), lip, placed);
            yield return Frames(6);
            God(g);
            yield return Shot30(dir, "A246-v2-lip");
            frames[0]++;
            g.husks.KillAll();
            var lane = new Vec3(-4, 0, 70);
            yield return Spire46Shot(g, dir, "A246-v2-mid-pre", lane, 0, 0.02, 20, frames, false);
            Spire46Put(g, new[] { "husk", "husk", "husk", "swarmer", "swarmer" }, Spire46Wave(v2, 0), lane, placed);
            Spire46Put(g, new[] { "spitter", "spitter" }, Spire46Wave(v2, 1), lane, placed);
            yield return Frames(6);
            God(g);
            yield return Shot30(dir, "A246-v2-mid-0");
            frames[0]++;
            yield return Spire46Hold(g, dir, "A246-v2-mid-1", 1.5, frames);
            g.husks.KillAll();
            var east = new Vec3(-3, 0, 66);
            yield return Spire46Shot(g, dir, "A246-v2-tank-pre", east, 15, 0.0, 20, frames, false);
            Spire46Put(g, new[] { "tank", "swarmer", "swarmer", "swarmer" }, Spire46Wave(v2, 2), east, placed);
            yield return Frames(6);
            God(g);
            yield return Shot30(dir, "A246-v2-tank");
            frames[0]++;
            g.husks.KillAll();
            yield return Spire46Frames36(g, dir, "v2", frames);

            // ---- v6 THE ROPE CHOIR ----
            g.husks.KillAll();
            yield return Spire46Shot(g, dir, "A246-v6-door-pre", v6.spawn, v6.spawnYawDeg, 0.06, 20, frames, false);
            Spire46Put(g, new[] { "husk", "husk", "husk", "husk", "swarmer", "swarmer" }, Spire46Wave(v6, 0), v6.spawn, placed);
            yield return Frames(6);
            God(g);
            yield return Shot30(dir, "A246-v6-door");
            frames[0]++;
            g.husks.KillAll();
            var isle = new Vec3(-10, 2.4, -88);
            yield return Spire46Shot(g, dir, "A246-v6-mid-pre", isle, 30, 0.08, 20, frames, false);
            Spire46Put(g, new[] { "spitter", "tank", "drone", "drone" }, Spire46Wave(v6, 1), isle, placed);
            yield return Frames(6);
            God(g);
            yield return Shot30(dir, "A246-v6-mid-0");
            frames[0]++;
            yield return Spire46Hold(g, dir, "A246-v6-mid-1", 1.5, frames);
            g.husks.KillAll();
            yield return Spire46Shot(g, dir, "A246-v6-drop-pre", isle, 20, 0.1, 20, frames, false);
            Spire46Put(g, new[] { "swarmer", "swarmer", "swarmer", "tank", "husk", "husk", "swarmer" }, Spire46Wave(v6, 2), isle, placed);
            yield return Frames(6);
            God(g);
            yield return Shot30(dir, "A246-v6-drop");
            frames[0]++;
            g.husks.KillAll();
            yield return Spire46Frames36(g, dir, "v6", frames);

            notes.Add("placed " + string.Join(",", placed));
            g.forcedMove = null;
            yield return Stop();
            bool green = frames[0] >= 12; // 6 a room: 4 placed + frames36 mid / exit (round 1 asked for 14: A2's miscount; build r1 wrote all 12)
            res[0] = $"{(green ? "GREEN" : "RED")} a246 | map {g.MapId}, level {id} | frames {frames[0]} of 12 | {string.Join(" | ", notes)}";
        }
    }
}
