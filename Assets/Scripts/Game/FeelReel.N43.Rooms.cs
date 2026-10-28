// NIGHT43 builder E1 (rooms43, iceline43; NIGHT43.md E1's proof reel): the Player scenario `rooms43`. Boot one Player per level (reel.sh +
// -skygrindMap <level> + -skygrindReelOnly rooms43; L-062): level 1 (n42-foundry or n43-foundry) for parts 1-2, level 2 (n43-iceline) for all
// three. The reel forces rooms43 on for its run (and the level row whose levelTo is the booted map: foundry43 / iceline43 / kiln43), handed
// back after. The run: a CAMPAIGN run, DOORFREE to reach the room, then DOORFREE off so the room locks; the player kept alive, idle.
//   1. THE COUNTER: the level's first fight room with 3+ enemies (its waves' sum); the player at its spawn; the room's enemies are killed one at
//      a time, 1.2 s apart (the loadout's damage: real kills); frames E43-count-<n> each time SurvivalSim.N43RoomLeft drops to 3, 2, 1, 0
//      (U1's counter43 draws it when its row is on), then E43-door-0 / E43-door-1 (+0.0 / +0.5 s after roomClear43, the look on the door).
//   2. THE PIT: the next fight room; its first enemy is put 3 m under the room's floor (a dry pit's knock), the others killed; frames E43-pit-0
//      (+0.2 s: the counter still counts it) and E43-pit-1 (+2.4 s: rooms43 ended it, the counter one lower).
//   3. LEVEL 2 (n43-iceline only): g6's FUSION husk (E43-g6-0: the KESTREL husks killed, +1.5 s: the husk and the lesson line), g8's first
//      CHARGER beside a wall (E43-g8-0 +1.5 s after entry, E43-g8-1 +3.0 s).
// Line: GREEN when part 1 saw the counter tick to 1 (0 is never read: the room clears on its last kill and the counter hides) and the door open,
// and part 2's body (frozen, pinned 1.7 m under the floor) was ended by rooms43's rule (rooms43Ended > 0); RED names what was missing.
// Not run by its author (builders never run a Player).
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N43Rooms(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Rooms43Scenario(g, dir, res, offs, pre);

        /// <summary>rooms43 on for the reel's run, with the n43 level row whose levelTo is the booted map.</summary>
        static N43Blocks Rooms43Bits(SurvivalGame g)
        {
            int bits = N43Blocks.From(g.cfg).Bits | 1 << Array.IndexOf(N43Blocks.Names, "rooms43");
            foreach (string row in new[] { "foundry43", "iceline43", "kiln43" })
            {
                string key = "design.n43." + row + ".levelTo";
                if (g.cfg.Has(key) && g.cfg.Str(key) == N45Levels.BaseId(g.cfg, g.MapId)) bits |= 1 << Array.IndexOf(N43Blocks.Names, row);
            }
            return N43Blocks.FromBits(bits);
        }

        /// <summary>DOORFREE into room `i`, then DOORFREE off once the campaign entered it (its door locks behind).</summary>
        static IEnumerator Rooms43Enter(SurvivalGame g, int i)
        {
            SurvivalSim sim = g.sim;
            RoomDef r = sim.campaignLevel.rooms[i];
            sim.campaign.SetDoorFree29(true);
            g.husks.KillAll();
            g.Respawn(r.spawn + new Vec3(0, 0.05, 0), r.spawnYawDeg);
            g.forcedMove = new InputFrame { yaw = r.spawnYawDeg * Math.PI / 180, pitch = -0.05 };
            for (int k = 0; k < 240 && sim.campaign.room != i; k++) { God(g); yield return null; }
            sim.campaign.SetDoorFree29(false);
            g.UiView?.ClearBanners41();
        }

        /// <summary>The room's living enemies (not stand-ins), in the registry's order.</summary>
        static List<Husk> Rooms43Alive(SurvivalGame g)
        {
            var l = new List<Husk>();
            foreach (Enemy e in g.sim.enemies.all)
                if (e.alive && !e.standIn29 && e is Husk h) l.Add(h);
            return l;
        }

        static IEnumerator Rooms43Wait(SurvivalGame g, double s)
        {
            double t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < s) { God(g); yield return null; }
        }

        static IEnumerator Rooms43Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            var ok = new bool[1];
            // the override must be in place before Campaign41's StartRun reads it
            if (g.sim != null)
            {
                SurvivalSim s0 = g.sim;
                N43Blocks? was = s0.n43Override;
                s0.n43Override = Rooms43Bits(g);
                offs.Add(() => s0.n43Override = was);
            }
            yield return Campaign41(g, offs, true, ok);
            SurvivalSim sim = g.sim;
            if (!ok[0] || sim == null || !sim.Rooms43On) { res[0] = $"RED rooms43: {(ok[0] ? "rooms43 not on for this run" : "no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + ")")}"; yield break; }
            g.forcedCombat = IdleCombat;
            God(g);
            int clears43 = 0, frames = 0;
            offs.Add(g.events.On(N43Events.RoomClear, _ => clears43++));
            Start(g, "rooms43", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var notes = new List<string>();
            LevelDef lv = sim.campaignLevel;
            var fights = new List<int>();
            for (int i = 0; i < lv.rooms.Count; i++)
                if (lv.rooms[i].kind != "boss" && lv.rooms[i].WaveCount > 0) fights.Add(i);

            // ---- 1. THE COUNTER ----
            int a = fights.Find(i => lv.rooms[i].EnemyCount >= 3);
            bool counterTo0 = false, doorOpened = false;
            if (fights.Count > 0 && lv.rooms[a].EnemyCount >= 3)
            {
                yield return Rooms43Enter(g, a);
                var seen = new HashSet<int>();
                int c0 = clears43;
                double t0 = Time.realtimeSinceStartup, lastKill = t0;
                while (Time.realtimeSinceStartup - t0 < 60 && clears43 == c0)
                {
                    God(g);
                    int left = sim.N43RoomLeft;
                    if (left >= 0 && left <= 3 && seen.Add(left))
                    {
                        yield return Shot30(dir, "E43-count-" + left);
                        frames++;
                        if (left == 0) counterTo0 = true;
                    }
                    if (Time.realtimeSinceStartup - lastKill > 1.2)
                    {
                        List<Husk> alive = Rooms43Alive(g);
                        if (alive.Count > 0) { sim.loadout.Damage(alive[0], alive[0].hp + 100000, false, "revolver", sim.mover.position, shot: 0); lastKill = Time.realtimeSinceStartup; }
                    }
                    yield return null;
                }
                doorOpened = clears43 > c0;
                // round 2 fix (n43-r2 RED: counts [3,2,1]): the room clears in the same sim step its last body dies (lessons done), and N43RoomLeft
                // is -1 from that step (U1's counter hides as the door opens), so a frame never reads 0. The last tick is 1 -> the door.
                if (doorOpened && seen.Contains(1)) counterTo0 = true;
                if (doorOpened && !seen.Contains(0)) { yield return Shot30(dir, "E43-count-0"); frames++; }
                if (doorOpened)
                {
                    RoomDef r = lv.rooms[a];
                    Vec3 me = sim.mover.position, to = r.exit;
                    double yaw = Math.Atan2(-(to.x - me.x), -(to.z - me.z));
                    g.input.SetLook(yaw, -0.05);
                    g.forcedMove = new InputFrame { yaw = yaw, pitch = -0.05 };
                    yield return Shot30(dir, "E43-door-0");
                    yield return Rooms43Wait(g, 0.5);
                    yield return Shot30(dir, "E43-door-1");
                    frames += 2;
                }
                notes.Add($"counter: room {lv.rooms[a].id} ({lv.rooms[a].EnemyCount} planned), counts seen [{string.Join(",", seen)}], door {(doorOpened ? "opened" : "SHUT")}");
            }
            else notes.Add("counter: no fight room with 3+ enemies on this level");

            // ---- 2. THE PIT ----
            int b = fights.Find(i => i > a);
            int ended0 = sim.rooms43Ended;
            if (b > a)
            {
                yield return Rooms43Enter(g, b);
                double t0 = Time.realtimeSinceStartup;
                List<Husk> alive = Rooms43Alive(g);
                while (alive.Count == 0 && Time.realtimeSinceStartup - t0 < 8) { God(g); yield return null; alive = Rooms43Alive(g); }
                if (alive.Count > 0)
                {
                    RoomDef r = lv.rooms[b];
                    double floor = r.spawn.y;
                    foreach (Vec3 n in r.nodes) floor = Math.Min(floor, n.y);
                    Husk h = alive[0];
                    // round 2 fix (n43-r2 RED: 'ended 0'): a body dropped 3 m under a solid floor fell through the void to the -30 m kill plane in
                    // under 2 s (dead and counted, but not by rooms43's rule). A dry pit HOLDS the body: pin it 1.7 m under the floor (its head
                    // at floor level: in view) every frame, so it stays under the floor until the rule ends it.
                    // the bodies are frozen meanwhile (Husks.frozen: no tick, no kill plane; rooms43's step still runs), so the pin holds between frames
                    var pin = new Vec3(h.position.x, floor - 1.7, h.position.z);
                    int id0 = h.id;
                    g.husks.frozen = true;
                    offs.Add(() => g.husks.frozen = false);
                    h.Teleport(pin);
                    int before = -1;
                    bool shot0 = false;
                    double t1 = Time.realtimeSinceStartup, endedAt = -1;
                    while (Time.realtimeSinceStartup - t1 < 4 && h.alive && h.id == id0)
                    {
                        God(g);
                        h.Teleport(pin);
                        h.velocity = default;
                        if (!shot0 && Time.realtimeSinceStartup - t1 >= 0.2) { before = sim.N43RoomLeft; yield return Shot30(dir, "E43-pit-0"); shot0 = true; }
                        yield return null;
                    }
                    if (!(h.alive && h.id == id0)) endedAt = Time.realtimeSinceStartup - t1;
                    g.husks.frozen = false;
                    yield return Rooms43Wait(g, 0.2);
                    yield return Shot30(dir, "E43-pit-1");
                    frames += 2;
                    notes.Add($"pit: room {r.id}, held 1.7 m under the floor, gone after {endedAt:0.00} s (real), counter {before} -> {sim.N43RoomLeft}, ended by the rule {sim.rooms43Ended - ended0}");
                    foreach (Husk o in Rooms43Alive(g)) sim.loadout.Damage(o, o.hp + 100000, false, "revolver", sim.mover.position, shot: 0);
                }
                else notes.Add($"pit: room {lv.rooms[b].id} spawned nothing in 8 s");
            }
            else notes.Add("pit: no second fight room");

            // ---- 3. LEVEL 2: the FUSION husk, the wall CHARGER ----
            if (N45Levels.BaseId(g.cfg, g.MapId) == (g.cfg.Has("design.n43.iceline43.levelTo") ? g.cfg.Str("design.n43.iceline43.levelTo") : "n43-iceline"))
            {
                int g6 = lv.rooms.FindIndex(r => r.id == "g6"), g8 = lv.rooms.FindIndex(r => r.id == "g8");
                if (g6 >= 0)
                {
                    yield return Rooms43Enter(g, g6);
                    yield return Rooms43Wait(g, 1.0);
                    foreach (Husk o in Rooms43Alive(g)) sim.loadout.Damage(o, o.hp + 100000, false, "revolver", sim.mover.position, shot: 0); // the KESTREL's two
                    yield return Rooms43Wait(g, 1.5);
                    yield return Shot30(dir, "E43-g6-0");
                    frames++;
                    notes.Add($"g6: alive after the KESTREL lesson {Rooms43Alive(g).Count} (the FUSION husk)");
                }
                if (g8 >= 0)
                {
                    yield return Rooms43Enter(g, g8);
                    yield return Rooms43Wait(g, 1.5);
                    yield return Shot30(dir, "E43-g8-0");
                    yield return Rooms43Wait(g, 1.5);
                    yield return Shot30(dir, "E43-g8-1");
                    frames += 2;
                    int ch = 0;
                    foreach (Husk o in Rooms43Alive(g)) if (o.kind == "charger") ch++;
                    notes.Add($"g8: chargers alive {ch}");
                }
            }
            g.forcedMove = null;
            yield return Stop();
            bool pitEnded = sim.rooms43Ended > ended0;
            bool green = counterTo0 && doorOpened && pitEnded;
            res[0] = $"{(green ? "GREEN" : "RED")} rooms43 | map {g.MapId} | frames {frames} | {string.Join(" | ", notes)} | ended {sim.rooms43Ended}, tracked {sim.rooms43Tracked}, unsticks {sim.rooms43Unsticks}, falls {sim.campaign?.fallReturns43}";
        }
    }
}
