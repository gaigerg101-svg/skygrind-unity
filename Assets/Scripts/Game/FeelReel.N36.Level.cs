// NIGHT36 builder L (NIGHT36.md L1; row design.n36.level36): the Player scenario `level36` (the lead's FeelReel.N27.cs dispatches PhaseN36L to
// N36LevelBody). The BOOT CHECK of the from-scratch level 1 (L-043: the Editor never loads a campaign level) and the lead's three frames per room.
//   level36  boot -skygrindMap n36-foundry (GREEN) or -skygrindMap n35-foundry (RED: the old level, no arena36 room, no arena flow).
//            A campaign run, DOORFREE, god mode. For every room in order: the player is put at the room's spawn (DOORFREE enters it), then
//            -a ENTRY: the spawn's view, 1.2 s after the entry in an arena room (the first enemies' spawn bubbles and the red doors are up),
//            -b MIDDLE: the room's frames36.mid viewpoint (the level file's; the beat sheet's movement line seen from inside),
//            -c EXIT: the room's frames36.exit viewpoint (looking back over the room).
//            Then one `[reel] level36:` line: per room what was entered and seen (arena locks, switch-ons, bubbles and door slabs drawn at -a).
// Shots go to <dir>/shot-N36-level36-<room>-a/-b/-c.png (Shot29's sampled numbers). The result starts with PASS or RED. Not run by its author (no Player
// in the worktree).
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
        static partial void N36LevelBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Level36Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Shot36L(string dir, string name, double[] st, List<string> lines, CultureInfo ci)
        {
            yield return Shot29(dir, "N36-level36-" + name, st);
            lines.Add(ShotLine29(name, st, ci));
        }

        static double[] Level36Frame(Dictionary<string, Dictionary<string, object>> rows, string room, string key)
        {
            if (!rows.TryGetValue(room, out var r) || !r.TryGetValue("frames36", out object fo) || !(fo is Dictionary<string, object> f) || !f.TryGetValue(key, out object v) || !(v is List<object> a) || a.Count < 4) return null;
            return new[] { PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2]), PortData.Num(a[3]), a.Count > 4 ? PortData.Num(a[4]) : -0.04 }; // x, y, z, yawDeg, pitch (rad, up +)
        }

        static IEnumerator Level36Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot -skygrindMap n36-foundry (GREEN) or -skygrindMap n35-foundry (RED)";
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why + " (" + boot + ")"; yield break; }
            SurvivalSim sim = g.sim;
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(30);
            God(g);
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN (" + boot + ")"; yield break; }
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            sim.campaign?.SetDoorFree29(true);
            LevelDef lv = sim.campaignLevel;
            var rows = Levels35Rows(g);
            int arenaRooms = 0;
            foreach (var kv in rows) if (kv.Value.TryGetValue("arena36", out object a) && a is bool ab && ab) arenaRooms++;
            int locks0 = Arena36View.LocksSeen, ins0 = Arena36View.SpawnInsSeen;
            string pc = Preconditions(g, g.Feet) + $" | level {lv.id}, level36 row {g.cfg.Design("n36.level36.enabled")}, off36 {N36Blocks.ForceOff}, arena rooms in the file {arenaRooms}, Arena36Level {sim.Arena36Level}";
            Start(g, "level36", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();
            var shots = new List<string>();
            var st = new double[4];
            int entered = 0, framed = 0, arenaSeen = 0, bubbleFrames = 0, bubbleWant = 0, doorFrames = 0, titled = 0;
            string ended = null;
            foreach (RoomDef r in lv.rooms)
            {
                if (ended != null) break;
                bool arena = rows.TryGetValue(r.id, out var row) && row.TryGetValue("arena36", out object ao) && ao is bool ab2 && ab2;
                int locksBefore = Arena36View.LocksSeen, insBefore = Arena36View.SpawnInsSeen;
                // -a ENTRY: the spawn, facing in (an arena room: 1.2 s, the first wave's bubbles and the red doors)
                g.Respawn(r.spawn, r.spawnYawDeg);
                g.input.SetLook(r.spawnYawDeg * Math.PI / 180, -0.04);
                yield return Settle31(g, arena ? 1.2 : 0.6, () => (ended = Ended30(g)) != null);
                Vec3 ft = g.Feet;
                bool inRoom = ft.x >= r.x0 && ft.x <= r.x1 && ft.z >= r.z0 && ft.z <= r.z1;
                if (inRoom) entered++;
                int bubbles = Arena36View.BubblesShown, doorSlabs = Arena36View.DoorsShown;
                bool held = r.standIns.Count > 0; // the KESTREL room: its pair waits for the stand-in line to die (the reel shoots nothing): no bubble at -a by design
                if (arena && !held) bubbleWant++;
                if (arena && !held && bubbles > 0) bubbleFrames++;
                if (arena && doorSlabs > 0) doorFrames++;
                string titleA = Ui.Hud33SlotNow ?? "";
                if (ended == null) { yield return Shot36L(dir, $"{r.id}-a", st, shots, ci); framed++; }
                // the room's title: the HUD's message slot queues its posts (each held >= 1.2 s), so a fast reel sees the room before's title at -a; -b / -c
                // wait for this room's own (at most 3 s)
                double tw0 = sim.SimTime;
                yield return Settle31(g, 3.0, () => (ended = Ended30(g)) != null || (Ui.Hud33SlotNow ?? "").Contains(r.name));
                string titleB = (Ui.Hud33SlotNow ?? "").Contains(r.name) ? $"after {(sim.SimTime - tw0).ToString("0.0", ci)} s" : "NOT SHOWN in 3 s";
                if (titleB.StartsWith("after")) titled++;
                // -b MIDDLE and -c EXIT: the level file's viewpoints
                foreach (string key in new[] { "mid", "exit" })
                {
                    if (ended != null) break;
                    double[] f = Level36Frame(rows, r.id, key);
                    if (f == null) continue;
                    var at = new Vec3(f[0], f[1], f[2]);
                    g.Respawn(at, f[3]);
                    g.input.SetLook(f[3] * Math.PI / 180, f[4]);
                    yield return Settle31(g, 0.5, () => (ended = Ended30(g)) != null);
                    if (ended == null) { yield return Shot36L(dir, $"{r.id}-{(key == "mid" ? "b" : "c")}", st, shots, ci); framed++; }
                }
                int locks = Arena36View.LocksSeen - locksBefore, ins = Arena36View.SpawnInsSeen - insBefore;
                if (arena && locks > 0) arenaSeen++;
                lines.Add($"{r.id} '{r.name}' {(arena ? "ARENA" : r.kind)}{(held ? " (wave held by stand-ins)" : "")} entered {inRoom}: locks {locks}, switch-ons {ins}, at -a bubbles {bubbles} door slabs {doorSlabs} title '{titleA}', own title {titleB}, alive {Alive30(sim)}");
            }
            yield return Stop();
            int frames = 3 * lv.rooms.Count;
            bool ok = arenaRooms > 0 && entered == lv.rooms.Count && framed == frames && arenaSeen == arenaRooms && bubbleFrames == bubbleWant && doorFrames == arenaRooms && titled == lv.rooms.Count && ended == null;
            res[0] = $"{(ok ? "PASS" : "RED")}: level {lv.id}: {entered}/{lv.rooms.Count} rooms entered, {framed}/{frames} frames, arena rooms {arenaSeen}/{arenaRooms} locked, bubbles at -a in {bubbleFrames}/{bubbleWant} (rooms whose wave is not held by stand-ins), red doors at -a in {doorFrames}, own title before -b in {titled}/{lv.rooms.Count}, ride trim panels {RideTrim36.Panels}; "
                + $"switch-ons {Arena36View.SpawnInsSeen - ins0}, locks {Arena36View.LocksSeen - locks0}{(ended != null ? " | ended: " + ended : "")} | {pc} | "
                + "LOOK: -a of a2..a5: red-orange bubbles shrinking round the first enemies, the doors behind and ahead red; a1-a: a lit hall, the 1 m step's cyan edge, no enemy; "
                + "a2-b: three stand-ins in a row under a white light, the deck's ramp at the back left; a3-b: two cyan-striped panels, one over the floor, one in the lava, the bridge on the right; "
                + "a4-a: the perch straight ahead under a yellow pool, one spitter on it, bare floor between; a5-b: the lava across the middle with two striped panels standing in it, the deck and its ramps behind; a7-a / a8-a: the ring's well and trim through the door || "
                + string.Join(" || ", lines) + " || shots: " + string.Join("; ", shots);
        }
    }
}
