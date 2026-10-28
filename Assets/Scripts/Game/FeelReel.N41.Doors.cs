// NIGHT41 builder T2 (INVENTORY B1 / B2's door proof; NIGHT41.md card T2): the Player scenario `doors41`: the doorways as the real flow has them,
// WITHOUT DOORFREE (A9's DOORFREE opens every zone: the old reels never saw a locked door). Boot any campaign level (reel.sh + -skygrindMap <level>
// + -skygrindReelOnly doors41). Not run by its author. Input through forced frames only; the player God.
// The run: a CAMPAIGN run, doors as the flow keeps them. The door picked: the first door (json campaign.doors order) whose room before it (between[0])
// has a wave and whose room after it is not the boss room. The player is put at that room's spawn with DOORFREE on for that one entry (the campaign
// enters only room + 1 otherwise: n41-r3's RED), DOORFREE off at once after (the flow's locks back: SetDoorFree29(false)); its wave starts, the
// wave is FROZEN alive once it is out (husks.frozen), and the player is stood on the room's side of the doorway facing it:
//   D41-locked-far / D41-locked-near   12 m / 3.5 m from the doorway while the wave lives: the door's zone locked (want doors38's RED-ORANGE sheet)
// then every wave is killed until the room clears and the door opens (up to 60 s; husks.KillAll each 0.5 s); bodies in the line to the doorway are moved aside:
//   D41-open-far / D41-open-near       the same two spots, the zone open (want the CYAN veil and the lit room beyond)
// then the boss door (the door into the room of kind "boss"), from its room's side, whatever its state:
//   D41-boss-far / D41-boss-near       (want the PALE VIOLET-WHITE curtain and the boss room's sign over the door)
// Each frame's line: the zone's state (Zones.IsOpen), the live husks, the campaign room, Doors38View's counters (OpenShown / LockedShown / BossShown;
// all 0 with doors38 off in this build), the banner on screen.
// Line: GREEN when the locked frames saw the zone locked with husks alive, the open frames saw it open, and all 6 frames were saved.
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
        static IEnumerator Doors41Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var ok = new bool[1];
            yield return Campaign41(g, offs, false, ok);
            if (!ok[0]) { res[0] = "RED doors41: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + ")"; yield break; }
            SurvivalSim sim = g.sim;
            LevelDef lv = sim.campaignLevel;
            var camp = g.map.json["campaign"] as Dictionary<string, object>;
            var rooms = Doors38Math.Rooms(camp);
            // the doorways' geometry (web): id -> centre x / z, the unit vector from room a into room b, rooms a / b
            var geo = new List<(string id, double cx, double cz, double ax, double az, string a, string b)>();
            if (camp.TryGetValue("doors", out object dobj) && dobj is List<object> dl)
                foreach (object o in dl)
                {
                    if (!(o is Dictionary<string, object> d) || !(d.TryGetValue("id", out object io) && io is string id)) continue;
                    double[] rect = d.TryGetValue("rect", out object ro) ? Doors38Math.Rect(ro) : null;
                    if (rect == null) continue;
                    string a = null, b = null;
                    if (d.TryGetValue("between", out object bo) && bo is List<object> bl && bl.Count >= 2) { a = bl[0] as string; b = bl[1] as string; }
                    double[] ra = a != null && rooms.TryGetValue(a, out var rowA) ? rowA.rect : null;
                    double[] rb = b != null && rooms.TryGetValue(b, out var rowB) ? rowB.rect : null;
                    bool spansX = Doors38Math.SpansX(rect, ra, rb);
                    var (ax, az) = Doors38Math.Ahead(rect, rb, spansX);
                    geo.Add((id, (rect[0] + rect[2]) / 2, (rect[1] + rect[3]) / 2, ax, az, a, b));
                }
            RoomDef RoomOf(string id) => id == null ? null : lv.rooms.Find(r => r.id == id);
            int pick = geo.FindIndex(x => { RoomDef ra = RoomOf(x.a), rb = RoomOf(x.b); return ra != null && ra.waves.Count > 0 && ra.kind != "boss" && (rb == null || rb.kind != "boss"); });
            int bossPick = geo.FindIndex(x => { RoomDef rb = RoomOf(x.b); return rb != null && rb.kind == "boss"; });
            if (pick < 0) { res[0] = $"RED doors41: level {lv.id}: no door after a room with a wave ({geo.Count} doors)"; yield break; }
            g.forcedCombat = IdleCombat;
            God(g);
            double wallEnd = Time.realtimeSinceStartup + 180;
            var lines = new List<string>();
            int saved = 0;
            Start(g, "doors41", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();

            // stand `dist` m back from door k on its room-a side, facing it (the floor found by a ray down from the room's spawn height)
            void StandAt(int k, double dist)
            {
                var x = geo[k];
                RoomDef ra = RoomOf(x.a);
                double y0 = ra != null ? ra.spawn.y : 0;
                var p = new Vec3(x.cx - x.ax * dist, y0 + 3, x.cz - x.az * dist);
                if (sim.world.RayIntersect(p, new Vec3(0, -1, 0), out RayHit hit) && hit.distance < 12) p = new Vec3(p.x, p.y - hit.distance + 0.05, p.z);
                else p = new Vec3(p.x, y0 + 0.05, p.z);
                double yaw = Math.Atan2(-x.ax, -x.az); // the web's yaw: forward is (-sin yaw, -cos yaw)
                g.Respawn(p, yaw * 180 / Math.PI);
                g.input.SetLook(yaw, 0.08);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.08 };
            }
            string State(int k) => $"zone {geo[k].id} {(sim.zones.IsOpen(geo[k].id) ? "OPEN" : "LOCKED")}, husks {sim.husks.Count}, room {sim.campaign?.Room?.id ?? "none"}, doors38 open/locked/boss shown {Doors38View.OpenShown}/{Doors38View.LockedShown}/{Doors38View.BossShown}, banner '{Banner41(g)}'";
            // n41-r4: a frozen husk stood in the far frame's line to the doorway. Every live body within 2.5 m of the line from the camera's spot to the
            // doorway's centre is moved sideways to 4 m off it (to the side with more room, kept 0.8 m inside its room's rect); returns how many moved
            string[] kinds = { "husk", "swarmer", "spitter", "charger", "drone", "lancer", "warden" };
            int ClearSight(int k, double dist)
            {
                var x = geo[k];
                double[] rr = x.a != null && rooms.TryGetValue(x.a, out var rowR) ? rowR.rect : null;
                double sx = x.cx - x.ax * (dist + 0.5), sz = x.cz - x.az * (dist + 0.5);
                double lx = -x.az, lz = x.ax; // the line's sideways unit vector
                int moved = 0;
                foreach (string kind in kinds)
                    foreach (Husk h in g.husks.PoolOf(kind))
                    {
                        if (!h.alive) continue;
                        double px = h.position.x - sx, pz = h.position.z - sz;
                        double along = px * x.ax + pz * x.az, side = px * lx + pz * lz;
                        if (along < -1 || along > dist + 1 || Math.Abs(side) >= 2.5) continue;
                        double sign = side >= 0 ? 1 : -1;
                        if (rr != null)
                        {
                            // the side with more room inside the rect
                            double room(double sg) { double best = 0; for (double d = 0; d <= 8; d += 0.5) { double qx = h.position.x + lx * sg * d, qz = h.position.z + lz * sg * d; if (qx < rr[0] + 0.8 || qx > rr[2] - 0.8 || qz < rr[1] + 0.8 || qz > rr[3] - 0.8) break; best = d; } return best; }
                            if (room(-sign) > room(sign)) sign = -sign;
                        }
                        double push = sign * 4 - side; // the body's sideways offset becomes 4 m on the chosen side
                        double nx = h.position.x + lx * push, nz = h.position.z + lz * push;
                        if (rr != null) { nx = Math.Max(rr[0] + 0.8, Math.Min(rr[2] - 0.8, nx)); nz = Math.Max(rr[1] + 0.8, Math.Min(rr[3] - 0.8, nz)); }
                        h.position = h.previous = new Vec3(nx, h.position.y, nz);
                        moved++;
                    }
                return moved;
            }
            IEnumerator Pair(int k, string tag)
            {
                foreach (var (name, dist) in new[] { ("far", 12.0), ("near", 3.5) })
                {
                    StandAt(k, dist);
                    int moved = ClearSight(k, dist);
                    if (moved > 0) lines.Add($"D41-{tag}-{name}: {moved} body/bodies moved out of the line to the doorway");
                    for (int f = 0; f < 12; f++) { God(g); yield return null; }
                    lines.Add($"D41-{tag}-{name}: {State(k)}");
                    yield return Shot30(dir, $"D41-{tag}-{name}");
                    saved++;
                }
            }

            // ---- LOCKED: the room before the door entered, its wave out and frozen alive ----
            RoomDef roomA = RoomOf(geo[pick].a);
            g.husks.frozen = false;
            // n41-r3: a teleport alone never entered the room (without DOORFREE the campaign enters only room + 1, after the room before clears: "room a1,
            // husks 0"). So DOORFREE on for the entry (its rule: the zone the player stands in is entered and its wave spawns as usual), then OFF at once:
            // SetDoorFree29(false) puts the locks back as the flow has them (the door after a room in its fight locked; the zone he stands in kept open)
            DoorFree41(g);
            g.Respawn(roomA.spawn + new Vec3(0, 0.05, 0), roomA.spawnYawDeg);
            double t0 = Time.realtimeSinceStartup;
            while (sim.campaign.Room?.id != roomA.id && Time.realtimeSinceStartup - t0 < 4) { God(g); yield return null; }
            string entered = sim.campaign.Room?.id ?? "none";
            sim.campaign.SetDoorFree29(false);
            g.UiView?.ClearBanners41();
            t0 = Time.realtimeSinceStartup;
            while (sim.husks.Count == 0 && Time.realtimeSinceStartup - t0 < 8) { God(g); yield return null; }
            yield return Frames(30);
            g.husks.frozen = true;
            offs.Add(() => g.husks.frozen = false);
            bool lockedSeen = !sim.zones.IsOpen(geo[pick].id) && sim.husks.Count > 0;
            int huskAtLock = sim.husks.Count;
            yield return Pair(pick, "locked");
            lockedSeen &= !sim.zones.IsOpen(geo[pick].id);

            // ---- OPEN: every wave of the room killed (n41-r4: a2 has more waves; the room clears only when all are dead) until the door's zone is open,
            // cap 60 s ----
            g.husks.frozen = false;
            StandAt(pick, 6);
            t0 = Time.realtimeSinceStartup;
            int killRounds = 0;
            while (!sim.zones.IsOpen(geo[pick].id) && Time.realtimeSinceStartup - t0 < 60 && Time.realtimeSinceStartup < wallEnd)
            {
                God(g);
                if (sim.husks.Count > 0) { g.husks.KillAll(); killRounds++; }
                for (int f = 0; f < 15; f++) yield return null;
            }
            yield return Frames(20); // the curtain turns with the zone
            double openAfter = Time.realtimeSinceStartup - t0;
            bool openSeen = sim.zones.IsOpen(geo[pick].id);
            yield return Pair(pick, "open");

            // ---- the BOSS door ----
            string bossLine = "no boss door on this level";
            if (bossPick >= 0)
            {
                yield return Pair(bossPick, "boss");
                bossLine = $"boss door {geo[bossPick].id} ({geo[bossPick].a} -> {geo[bossPick].b})";
            }
            yield return Stop();
            bool green = lockedSeen && openSeen && saved == (bossPick >= 0 ? 6 : 4);
            res[0] = $"{(green ? "GREEN" : "RED")} doors41 | map {g.MapId}, room entered {entered} (want {roomA.id}; DOORFREE on for the entry, then off), door {geo[pick].id} ({geo[pick].a} -> {geo[pick].b}): locked with {huskAtLock} husks alive {lockedSeen}, open after the room cleared {openSeen} ({openAfter.ToString("0.0", ci)} s, {killRounds} kill rounds, campaign phase {sim.campaign.phase}) | {bossLine}"
                + $" | doors38 drawing {Doors38View.On}, doors41On {N41View.RowOn(g.cfg, "doors41On")} | frames {saved} || {string.Join(" || ", lines)}";
        }
    }
}
