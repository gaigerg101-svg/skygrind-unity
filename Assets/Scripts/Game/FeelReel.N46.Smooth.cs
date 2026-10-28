// NIGHT46 builder A3 (foundry46 / iceline46: levels 1 and 2 smoothed; docs/ROOMS.md "NIGHT46 A3" and THE TIPPLE's card): the Player phase `a346`
// (dispatched by FeelReel.N46.cs). Not run by its author (builders never run a Player). Boot one Player per level (reel.sh + -skygrindMap n46-foundry or
// n46-iceline + -skygrindReelOnly a346; L-062), or a build whose copy has design.n46.foundry46 / iceline46 on; the phase frames whichever level runs.
// A2's a245 way (L-046 / L-053): a CAMPAIGN run, DOORFREE, the player kept alive (God), every frame from a placed camera, bodies placed where the
// level's own spots are (no pilot).
//   level 1 (BaseId chain reaches n43-foundry):
//     A346-a3-entrance  a3's spawn: the lava pit with a walk on both sides (F-1)
//     A346-a3-west      the near floor's west corner looking north: the new 2 m west strip beside the pit
//     A346-b1-entrance  b1's spawn with three husks on the ladle's rim front (wave 1's spots, F-2): the room reads occupied from the door
//     A346-b2-landing   gate 1's landing where the long wall lands, three husks on its far half (F-4), gate 2's lava beyond
//     A346-a8-foot      a8's spawn looking at P1's foot (the ride lesson's new rect, F-3)
//   level 2 (BaseId chain reaches n43-iceline):
//     A346-g3-entrance  g3's spawn: THE HOPPER, its chute, the trestle, the exit's cyan pool past the hopper; the three husks of wave 1 placed (I-3)
//     A346-g3-swarm-0/1 the swarm (8 swarmers) at the chute's two sides, the player 4 m in: placed, then +1.5 s (the pack in the lanes)
//     A346-g3-trestle   from the trestle's north end (the pickup) looking back over the room
//     A346-g3-frames-mid / -exit  the room's frames36
//     A346-g4-entrance  g4's spawn: two bridges over the canal (I-1)
//     A346-g8-entrance  g8's spawn with the charger 2 m off the east wall in front of the near pillars (I-2)
// Line: GREEN when the level is one of the two, every frame of its list was saved (level 1: 5, level 2: 8), every placed body appeared and every
// frame was taken with the campaign in the room the camera stands in (rooms: each frame and its room, for the title on screen).
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N46Smooth(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = A3Smooth46Scenario(g, dir, res, offs, pre);

        /// <summary>A3: stand at (x, y, z) looking along yawDeg / pitch (rad), wait `settle` frames, save the frame (A2's Mix45Shot, A3's copy).</summary>
        static IEnumerator A3Shot46(SurvivalGame g, string dir, string name, Vec3 at, double yawDeg, double pitch, int settle, int[] frames, bool save = true)
        {
            double yaw = yawDeg * Math.PI / 180;
            g.Respawn(at + new Vec3(0, 0.05, 0), yawDeg);
            g.input.SetLook(yaw, pitch);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
            yield return Frames(settle);
            // build r1 (the lead): b1's frame carried a3's room title. Wait (at most 5 s) until the campaign is in the room the camera stands in
            // (DOORFREE enters any room ahead from its zone), then clear the banners so the title is that room's
            string want = g.sim.campaignLevel != null ? A3ZoneRoom46(g, at) : null;
            float end = Time.realtimeSinceStartup + 5;
            while (want != null && A3CurRoom46(g) != want && Time.realtimeSinceStartup < end) { God(g); yield return null; }
            g.UiView?.ClearBanners41();
            God(g);
            if (!save) yield break;
            yield return Shot30(dir, name);
            frames[0]++;
            A3Rooms46.Add($"{name} in {A3CurRoom46(g)}{(want != null && A3CurRoom46(g) != want ? " (NOT " + want + ")" : "")}");
        }

        static readonly List<string> A3Rooms46 = new List<string>();

        /// <summary>A3: the campaign's current room id ("?" when none).</summary>
        static string A3CurRoom46(SurvivalGame g)
        {
            Campaign24 c = g.sim.campaign;
            LevelDef lv = g.sim.campaignLevel;
            return c != null && lv != null && c.room >= 0 && c.room < lv.rooms.Count ? lv.rooms[c.room].id : "?";
        }

        /// <summary>A3: the room whose rect holds (x, z) (null when none).</summary>
        static string A3ZoneRoom46(SurvivalGame g, Vec3 p)
        {
            foreach (RoomDef r in g.sim.campaignLevel.rooms)
                if (p.x >= Math.Min(r.x0, r.x1) && p.x <= Math.Max(r.x0, r.x1) && p.z >= Math.Min(r.z0, r.z1) && p.z <= Math.Max(r.z0, r.z1)) return r.id;
            return null;
        }

        /// <summary>A3: hold the camera for `seconds` of real time (God every frame), then save the frame.</summary>
        static IEnumerator A3Hold46(SurvivalGame g, string dir, string name, double seconds, int[] frames)
        {
            double t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < seconds) { God(g); yield return null; }
            yield return Shot30(dir, name);
            frames[0]++;
        }

        /// <summary>A3: the yaw (deg) from (x, z) toward (tx, tz) in the game's convention (0 = -z), as the generators' yawTo.</summary>
        static double A3Yaw46(double x, double z, double tx, double tz) => -Math.Atan2(tx - x, -(tz - z)) * 180 / Math.PI;

        static IEnumerator A3Smooth46Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED a346: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + ")"; yield break; }
            LevelDef lv = sim.campaignLevel;
            string id = lv?.id;
            string root = lv == null ? "?" : N45Levels.BaseId(sim.cfg, id);
            bool foundry = root == "n43-foundry", iceline = root == "n43-iceline";
            bool n46 = id == "n46-foundry" || id == "n46-iceline";
            if (!foundry && !iceline) { res[0] = $"RED a346: the run is on {id ?? "?"} (BaseId {root}), not level 1 or 2 (boot -skygrindMap n46-foundry or n46-iceline, L-062)"; yield break; }
            g.forcedCombat = IdleCombat;
            God(g);
            Start(g, "a346", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var frames = new int[1];
            var notes = new List<string>();
            A3Rooms46.Clear();
            int want = 0, placedAll = 0, askedAll = 0;
            RoomDef Room(string rid) => lv.rooms.Find(q => q.id == rid);
            int Put(string kind, double x, double y, double z, Vec3 target)
            {
                askedAll++;
                Husk h = g.husks.Spawn(kind, new Vec3(x, y, z), target);
                if (h != null) placedAll++;
                return h != null ? 1 : 0;
            }

            if (foundry)
            {
                want = 5;
                RoomDef a3 = Room("a3"), b1 = Room("b1"), b2 = Room("b2"), a8 = Room("a8");
                if (a3 == null || b1 == null || b2 == null || a8 == null) { res[0] = $"RED a346: {id} lacks a3 / b1 / b2 / a8"; yield break; }
                g.husks.KillAll();
                yield return A3Shot46(g, dir, "A346-a3-entrance", a3.spawn, a3.spawnYawDeg, -0.12, 30, frames);
                g.husks.KillAll();
                yield return A3Shot46(g, dir, "A346-a3-west", new Vec3(-7, 0, -67), A3Yaw46(-7, -67, -7, -84), -0.08, 20, frames);
                // b1: wave 1's spots on the rim front (gen-n46-foundry.cjs F-2), the camera at the door
                g.husks.KillAll();
                yield return A3Shot46(g, dir, "A346-b1-pre", b1.spawn, b1.spawnYawDeg, 0.02, 20, frames, false);
                int n = Put("husk", -5.5, 2.5, -158.5, b1.spawn) + Put("husk", 5.5, 2.5, -158.5, b1.spawn) + Put("husk", -4.5, 2.5, -158, b1.spawn);
                yield return Frames(6);
                God(g);
                yield return Shot30(dir, "A346-b1-entrance");
                A3Rooms46.Add("A346-b1-entrance in " + A3CurRoom46(g));
                frames[0]++;
                notes.Add($"b1: {n}/3 husks on the rim");
                // b2: the landing where the long wall lands, three husks on its far half
                g.husks.KillAll();
                var land = new Vec3(-2.2, 0, -231);
                yield return A3Shot46(g, dir, "A346-b2-pre", land, 0, -0.04, 20, frames, false);
                n = Put("husk", 9, 0, -240, land) + Put("husk", -9, 0, -240, land) + Put("husk", 2, 0, -240, land);
                yield return Frames(6);
                God(g);
                yield return Shot30(dir, "A346-b2-landing");
                A3Rooms46.Add("A346-b2-landing in " + A3CurRoom46(g));
                frames[0]++;
                notes.Add($"b2: {n}/3 husks on the landing");
                g.husks.KillAll();
                yield return A3Shot46(g, dir, "A346-a8-foot", a8.spawn, A3Yaw46(a8.spawn.x, a8.spawn.z, -4.75, -344.1), -0.15, 30, frames);
            }
            else
            {
                want = 8; // g3 entrance, swarm-0, swarm-1, trestle, frames-mid, frames-exit, g4, g8 (r1 asked 9: the count was wrong, not a frame)
                RoomDef g3 = Room("g3"), g4 = Room("g4"), g8 = Room("g8");
                if (g3 == null || g4 == null || g8 == null) { res[0] = $"RED a346: {id} lacks g3 / g4 / g8"; yield break; }
                // g3 THE TIPPLE: the entrance with wave 1's three husks either side of the hopper
                g.husks.KillAll();
                yield return A3Shot46(g, dir, "A346-g3-pre", g3.spawn, g3.spawnYawDeg, 0.06, 20, frames, false);
                int n = Put("husk", -9.5, 0, -74, g3.spawn) + Put("husk", 5, 0, -77, g3.spawn) + Put("husk", 8, 0, -75, g3.spawn);
                yield return Frames(6);
                God(g);
                yield return Shot30(dir, "A346-g3-entrance");
                A3Rooms46.Add("A346-g3-entrance in " + A3CurRoom46(g));
                frames[0]++;
                notes.Add($"g3 wave 1: {n}/3");
                // the swarm at the chute's two sides, the player 4 m in
                g.husks.KillAll();
                var p3 = new Vec3(0, 0, -68);
                yield return A3Shot46(g, dir, "A346-g3-swarm-pre", p3, A3Yaw46(0, -68, -4, -78), 0.0, 20, frames, false);
                n = 0;
                foreach (var (x, z) in new[] { (-8.5, -75.0), (-9.5, -77.5), (-8.5, -77.0), (-9.5, -75.5), (0.0, -76.5), (0.5, -78.5), (0.0, -77.5), (1.0, -77.0) }) n += Put("swarmer", x, 0, z, p3); // the chute's two sides (gen-n46-iceline.cjs CHUTE)
                yield return Frames(6);
                God(g);
                yield return Shot30(dir, "A346-g3-swarm-0");
                A3Rooms46.Add("A346-g3-swarm-0 in " + A3CurRoom46(g));
                frames[0]++;
                yield return A3Hold46(g, dir, "A346-g3-swarm-1", 1.5, frames);
                notes.Add($"g3 swarm: {n}/8, alive after 1.5 s {sim.enemies.AliveCount}");
                g.husks.KillAll();
                yield return A3Shot46(g, dir, "A346-g3-trestle", new Vec3(11.5, 2, -84), A3Yaw46(11.5, -84, -4, -70), -0.1, 30, frames);
                Dictionary<string, object> row = Mix45Row(g, "g3");
                if (row != null && row.TryGetValue("frames36", out object fo) && fo is Dictionary<string, object> f36)
                    foreach (string which in new[] { "mid", "exit" })
                        if (f36.TryGetValue(which, out object vo))
                        {
                            List<object> v = PortData.Arr(vo);
                            yield return A3Shot46(g, dir, "A346-g3-frames-" + which, new Vec3(PortData.Num(v[0]), PortData.Num(v[1]), PortData.Num(v[2])), PortData.Num(v[3]), PortData.Num(v[4]), 20, frames);
                        }
                g.husks.KillAll();
                yield return A3Shot46(g, dir, "A346-g4-entrance", g4.spawn, g4.spawnYawDeg, -0.08, 30, frames);
                g.husks.KillAll();
                yield return A3Shot46(g, dir, "A346-g8-pre", g8.spawn, g8.spawnYawDeg, 0.0, 20, frames, false);
                n = Put("charger", 10, 0, -232, g8.spawn); // wave 1's spot (gen-n46-iceline.cjs I-2)
                yield return Frames(6);
                God(g);
                yield return Shot30(dir, "A346-g8-entrance");
                A3Rooms46.Add("A346-g8-entrance in " + A3CurRoom46(g));
                frames[0]++;
                notes.Add($"g8 charger: {n}/1");
            }
            g.husks.KillAll();
            g.forcedMove = null;
            yield return Stop();
            bool roomsOk = !A3Rooms46.Exists(s => s.Contains("(NOT"));
            bool green = n46 && frames[0] >= want && placedAll == askedAll && roomsOk;
            res[0] = $"{(green ? "GREEN" : "RED")} a346 | map {g.MapId}, level {id} (BaseId {root}){(n46 ? "" : " (NOT an n46 level: the n45 rooms)")} | frames {frames[0]}/{want} | bodies {placedAll}/{askedAll} | rooms: {string.Join(", ", A3Rooms46)} | {string.Join(" | ", notes)}";
        }
    }
}
