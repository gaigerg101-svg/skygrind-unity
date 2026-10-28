// NIGHT45 builder A2 (NIGHT45.md card A2; docs/ROOMS.md's identity card THE THAW LOCK): the Player phase `a245`: level 2's g7 THE MIX as rebuilt in
// n45-iceline, framed for the lead's ON decision (NIGHT45.md method 4: the entrance frame, a mid-fight frame, the greyscale check from them). Boot one
// Player on level 2 (reel.sh + -skygrindMap n45-iceline + -skygrindReelOnly a245; L-062), or a build whose copy has design.n45.iceline45 on.
// The run: a CAMPAIGN run, DOORFREE, the player kept alive (God), every frame from a placed camera (no pilot: L-046 / L-053's input-path faults avoided).
//   1. A245-entrance: g7's spawn on the door landing, the spawn's yaw, the look a little up: THE BOILER dead ahead in the grate, the fire under it, the
//      gantries beyond (the landmark from the entry, A-6 / A-9).
//   2. the 30 s problem in play: a TANK at the west bridge's far end, two SPITTERS on the gantries, four SWARMERS at the east bridge's far end, all
//      walking at the player, who stands on the quay 3 m short of the grate: A245-mid-0 (placed), A245-mid-1 (+1.5 s: the pack on the bridges),
//      A245-mid-2 (+3.0 s).
//   3. the hazard both ways: a SWARMER set down in the middle pit beside the boiler: A245-burn (+0.3 s: its burn, lavaEnemyBurn); the line counts
//      lavaEnemyBurn / lavaEnemyDeath (GREEN needs the burn).
//   4. A245-back: from the far floor before the chute, looking back at the door side (wave 2's coal bunkers, the grate, the boiler's back).
//   5. the room's frames36: A245-frames-mid, A245-frames-exit.
// Line: GREEN when the run is on n45-iceline, all 8 frames were saved and the grate burned the swarmer. Not run by its author (builders never run a Player).
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N45Mix(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Mix45Scenario(g, dir, res, offs, pre);

        /// <summary>A2: a room row of the booted map's campaign block (null when absent).</summary>
        static Dictionary<string, object> Mix45Row(SurvivalGame g, string id)
        {
            if (g.map == null || g.map.json == null || !g.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return null;
            if (!camp.TryGetValue("rooms", out object ro) || !(ro is List<object> rows)) return null;
            foreach (object o in rows)
                if (o is Dictionary<string, object> r && r.TryGetValue("id", out object v) && v as string == id) return r;
            return null;
        }

        /// <summary>A2: stand at (x, y, z) looking along yawDeg / pitch (rad), wait `settle` frames, save the frame.</summary>
        static IEnumerator Mix45Shot(SurvivalGame g, string dir, string name, Vec3 at, double yawDeg, double pitch, int settle, int[] frames, bool save = true)
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
        static IEnumerator Mix45Hold(SurvivalGame g, string dir, string name, double seconds, int[] frames)
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

        static IEnumerator Mix45Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED a245: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + ")"; yield break; }
            string id = sim.campaignLevel?.id;
            if (sim.campaignLevel == null || N45Levels.BaseId(sim.cfg, id) != "n43-iceline")
            {
                res[0] = $"RED a245: the run is on {id ?? "?"}, not n45-iceline (boot -skygrindMap n45-iceline, L-062)";
                yield break;
            }
            RoomDef r7 = sim.campaignLevel.rooms.Find(q => q.id == "g7");
            if (r7 == null) { res[0] = "RED a245: no room g7"; yield break; }
            g.forcedCombat = IdleCombat;
            God(g);
            int burns = 0, deaths = 0;
            offs.Add(g.events.On<Lava30Event>("lavaEnemyBurn", _ => burns++));
            offs.Add(g.events.On<Lava30Event>("lavaEnemyDeath", _ => deaths++));
            Start(g, "a245", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var frames = new int[1];
            var notes = new List<string>();

            // ---- 1. the entrance: the boiler from the door landing ----
            g.husks.KillAll();
            yield return Mix45Shot(g, dir, "A245-entrance", r7.spawn, r7.spawnYawDeg, 0.06, 30, frames);

            // ---- 2. the 30 s problem in play ----
            g.husks.KillAll();
            var player = new Vec3(-3, 0, -192.5);
            yield return Mix45Shot(g, dir, "A245-mid-pre", player, 0, -0.02, 20, frames, false); // the camera settles on the apron before the bodies appear (no frame)
            var placed = new List<string>();
            void Put(string kind, double x, double y, double z)
            {
                Husk h = g.husks.Spawn(kind, new Vec3(x, y, z), player);
                if (h != null) placed.Add(kind);
            }
            Put("tank", -9, 0, -205.5);
            Put("spitter", -12, 2, -213);
            Put("spitter", 12, 2, -213);
            foreach (var (x, z) in new[] { (9.0, -204.0), (9.6, -205.0), (8.4, -205.0), (9.0, -206.0) }) Put("swarmer", x, 0, z);
            notes.Add("placed " + string.Join(",", placed));
            yield return Frames(6);
            God(g);
            yield return Shot30(dir, "A245-mid-0");
            frames[0]++;
            yield return Mix45Hold(g, dir, "A245-mid-1", 1.5, frames);
            yield return Mix45Hold(g, dir, "A245-mid-2", 1.5, frames);
            Vec3 p = g.sim.mover.position;
            notes.Add($"mid: the player held at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), alive {g.sim.enemies.AliveCount}, burns so far {burns}");

            // ---- 3. the hazard both ways: a swarmer in the fire ----
            g.husks.KillAll();
            yield return Mix45Shot(g, dir, "A245-burn-pre", new Vec3(-4.5, 0, -193.5), 0, -0.22, 20, frames, false); // the camera on the apron's lip looking down into the middle pit's west lane (no frame)
            int burns0 = burns;
            Husk sw = g.husks.Spawn("swarmer", new Vec3(-4.5, -0.75, -198.5), new Vec3(-4.5, 0, -193.5));
            yield return Mix45Hold(g, dir, "A245-burn", 0.3, frames);
            double t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 1.5 && deaths == 0) { God(g); yield return null; }
            notes.Add($"grate: the swarmer {(sw != null ? "placed" : "NOT placed")}, lavaEnemyBurn {burns - burns0}, lavaEnemyDeath {deaths}");

            // ---- 4. looking back at the door side ----
            g.husks.KillAll();
            yield return Mix45Shot(g, dir, "A245-back", new Vec3(0, 0, -207), 180, 0.04, 30, frames);

            // ---- 5. the room's frames36 ----
            Dictionary<string, object> row = Mix45Row(g, "g7");
            if (row != null && row.TryGetValue("frames36", out object fo) && fo is Dictionary<string, object> f36)
                foreach (string which in new[] { "mid", "exit" })
                    if (f36.TryGetValue(which, out object vo))
                    {
                        List<object> v = PortData.Arr(vo);
                        yield return Mix45Shot(g, dir, "A245-frames-" + which, new Vec3(PortData.Num(v[0]), PortData.Num(v[1]), PortData.Num(v[2])), PortData.Num(v[3]), PortData.Num(v[4]), 20, frames);
                    }

            g.forcedMove = null;
            yield return Stop();
            bool onN45 = NightRows.BaseId(sim.cfg, id) == "n45-iceline"; // NIGHT46: n46-iceline runs as its n45 room
            bool green = onN45 && frames[0] >= 8 && burns > burns0;
            res[0] = $"{(green ? "GREEN" : "RED")} a245 | map {g.MapId}, level {id}{(onN45 ? "" : " (not n45-iceline: the n43 room)")} | frames {frames[0]} | {string.Join(" | ", notes)}";
        }
    }
}
