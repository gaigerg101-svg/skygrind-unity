// NIGHT31 builder L: the Player scenarios of the level work (the names are FeelReel.N27.cs PhaseN31L, the body is the partial N31LevelsBody) and the body of
// builder G's id `spawn31` (PhaseN31G: the lead routes it here with one line in FeelReel.N27.cs). Not run by its author (no Player in the worktree).
// Boot each with -skygrindMap n30-foundry | n30-iceline | n30-kiln | n30-span (a CAMPAIGN level); DOORFREE is set (a room is entered from any room), God on.
//   rooms31   the WEAPON ROOMS of the booted level (f8, f9 | i1a, i2 | k1 | s1: DESIGN31 D and item 3): for each, the player is respawned at the room's entry, and two
//             frames are saved (iteration 1 at 0.5 s: shot-N31-rooms31-<room>-a.png; iteration 2 at 2.0 s: -b.png); one line per room: the lesson text on screen,
//             enemies alive, the nearest enemy to the entry (m), the owned weapons. PASS when every weapon room of the level was entered and framed.
//   arenas31  the BOSS ROOM of the booted level (f12 | g1 | k5 | s8): respawned at its entry; frames at 1.0 s (-a) and 4.0 s (-b); one line: the map id now, the
//             boss alive, the player's feet. PASS when the boss room was entered and both frames saved (a boss room with no boss clears: Results is a note).
//   spawn31   (builder G's id, L's body; P0 c) n30-foundry only: respawn at f9's entry; at the first frame its wave is alive, the nearest enemy's distance to the
//             entry (floor plan) and its angle off the entry's facing; frame shot-N31-spawn31-f9.png. PASS when the nearest is >= 6 m (the validator's spawn31 rule);
//             RED on NIGHT30's map (its f9 nodes were 1.4-7.2 m from the entry).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        /// <summary>The weapon rooms per level id (DESIGN31 D + item 3): the rooms rooms31 frames.</summary>
        static readonly Dictionary<string, string[]> WeaponRooms31 = new Dictionary<string, string[]>
        {
            { "n30-foundry", new[] { "f8", "f9" } },
            { "n30-iceline", new[] { "i1a", "i2" } },
            { "n30-kiln", new[] { "k1" } },
            { "n30-span", new[] { "s1" } },
            { "n32-foundry", new[] { "f8", "f9" } }, // NIGHT32: the rebuilt levels keep their weapon rooms (A, S); level 5 starts with all four (F)
            { "n32-kiln", new[] { "k1" } },
            { "n32-span", new[] { "s1" } },
            { "n32-iceline", new[] { "i1a", "i2" } },
        };

        /// <summary>The spawn31 rule (LevelValidate29.Spawn31's minimum distance from the entry; a layout rule of the report, not a gameplay number).</summary>
        const double Spawn31MinMetres = 6;

        static partial void N31LevelsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "rooms31") body = Tour31Scenario(g, dir, res, offs, pre, false);
            else if (s == "arenas31") body = Tour31Scenario(g, dir, res, offs, pre, true);
            else if (s == "spawn31") body = Spawn31Scenario(g, dir, res, offs, pre); // the lead moved spawn31 into PhaseN31L (11:08)
        }

        /// <summary>Round 2 (the lead, 11:27: rooms31 read 5.3 m in i1a, 4.9 m in i2, 1.0 m in s1 against the validator's >= 6 m): the nearest enemy at 0.5 s is an
        /// enemy that has already walked (i1a and s1 are their level's FIRST room, so the wave spawned at StartRun, seconds before the tour; i2's husks spawn at 6.1+ m
        /// and close 1-2 m in 0.5 s). This records where each enemy STOOD the first frame it was alive (a pooled enemy: each dead -> alive turn), so the line reads
        /// the spawn distance, the thing the validator checks.</summary>
        sealed class Spawns31
        {
            readonly Dictionary<Enemy, bool> was = new Dictionary<Enemy, bool>();
            public readonly List<Vec3> at = new List<Vec3>();
            public void Track(SurvivalSim sim)
            {
                foreach (Enemy e in sim.enemies.all)
                {
                    bool live = e.alive && !e.standIn29;
                    if (live && !(was.TryGetValue(e, out bool w) && w)) at.Add(e.position);
                    was[e] = live;
                }
            }
            /// <summary>The nearest recorded spawn inside the room's rect to its entry; -1 when none. n: the spawns in the room.</summary>
            public double Nearest(RoomDef r, out double angleDeg, out int n)
            {
                double best = -1, yaw = r.spawnYawDeg * Math.PI / 180, fx = -Math.Sin(yaw), fz = -Math.Cos(yaw);
                angleDeg = 0; n = 0;
                foreach (Vec3 p in at)
                {
                    if (p.x < r.x0 || p.x > r.x1 || p.z < r.z0 || p.z > r.z1) continue;
                    n++;
                    double dx = p.x - r.spawn.x, dz = p.z - r.spawn.z, d = Math.Sqrt(dx * dx + dz * dz);
                    if (best >= 0 && d >= best) continue;
                    best = d;
                    angleDeg = d > 1e-9 ? Math.Acos(Math.Max(-1, Math.Min(1, (dx * fx + dz * fz) / d))) * 180 / Math.PI : 0;
                }
                return best;
            }
        }

        static IEnumerator Wait31(SurvivalGame g, int frames, Spawns31 sp)
        {
            for (int i = 0; i < frames; i++) { sp?.Track(g.sim); yield return null; }
        }

        static IEnumerator Settle31(SurvivalGame g, double seconds, Func<bool> stop, Spawns31 sp = null)
        {
            SurvivalSim sim = g.sim;
            int frames = 0;
            for (double t0 = sim.SimTime; sim.SimTime - t0 < seconds && frames < 60 * 30 && !stop(); frames++) { God(g); sp?.Track(sim); yield return null; }
        }

        /// <summary>The nearest live enemy (stand-ins apart) to `at` on the floor plan; its angle off the facing yawDeg in degrees. -1 when none.</summary>
        static double Nearest31(SurvivalSim sim, Vec3 at, double yawDeg, out double angleDeg)
        {
            double best = -1;
            angleDeg = 0;
            double yaw = yawDeg * Math.PI / 180, fx = -Math.Sin(yaw), fz = -Math.Cos(yaw);
            foreach (Enemy e in sim.enemies.all)
            {
                if (!e.alive || e.standIn29) continue;
                double dx = e.position.x - at.x, dz = e.position.z - at.z, d = Math.Sqrt(dx * dx + dz * dz);
                if (best >= 0 && d >= best) continue;
                best = d;
                angleDeg = d > 1e-9 ? Math.Acos(Math.Max(-1, Math.Min(1, (dx * fx + dz * fz) / d))) * 180 / Math.PI : 0;
            }
            return best;
        }

        static IEnumerator Tour31Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, bool arenas)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string tag = arenas ? "arenas31" : "rooms31";
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            SurvivalSim sim = g.sim;
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            var spawns = new Spawns31();
            spawns.Track(sim); // round 2: the first room's wave spawns at StartRun
            yield return Wait31(g, 30, spawns);
            God(g);
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN"; yield break; }
            for (int k = 0; k < 120 && sim.campaign == null; k++) { spawns.Track(sim); yield return null; }
            sim.campaign?.SetDoorFree29(true);
            LevelDef lv = sim.campaignLevel;
            var want = new List<RoomDef>();
            foreach (RoomDef r in lv.rooms)
                if (arenas ? r.kind == "boss" : WeaponRooms31.TryGetValue(lv.id, out string[] ids) && Array.IndexOf(ids, r.id) >= 0) want.Add(r);
            if (want.Count == 0) { res[0] = $"RED: level '{lv.id}' has no {(arenas ? "boss room" : "weapon room")} in the list (boot -skygrindMap n30-foundry | n30-iceline | n30-kiln | n30-span)"; yield break; }
            string pc = Preconditions(g, g.Feet) + $" | level {lv.id}, n31.unlocks {g.cfg.Design("n31.unlocks.enabled")}";
            Start(g, tag, "camera", dir);
            yield return Wait31(g, pre, spawns);
            var lines = new List<string>();
            int entered = 0, shots = 0, nearBad = 0;
            string ended = null;
            foreach (RoomDef r in want)
            {
                if (ended != null) break;
                g.Respawn(r.spawn, r.spawnYawDeg);
                double tA = arenas ? 1.0 : 0.5, tB = arenas ? 4.0 : 2.0;
                yield return Settle31(g, tA, () => (ended = Ended30(g)) != null, spawns);
                Vec3 ft = g.Feet;
                bool inRoom = sim.campaign != null && sim.campaign.room == lv.rooms.IndexOf(r) || ft.x >= r.x0 && ft.x <= r.x1 && ft.z >= r.z0 && ft.z <= r.z1 || arenas && g.MapId != lv.id;
                if (inRoom) entered++;
                if (ended == null) { yield return Shot30(dir, $"{tag}-{r.id}-a"); shots++; }
                string lesson = Ui.Lesson25Visible && !string.IsNullOrEmpty(Ui.Lesson25TextNow) ? Ui.Lesson25TextNow : "(none)";
                // arenas: the boss lives in its own arena far from the plan room, so the distance is from the player's feet; rooms: from the entry
                double near = Nearest31(sim, arenas ? g.Feet : r.spawn, r.spawnYawDeg, out double ang);
                yield return Settle31(g, tB - tA, () => (ended = Ended30(g)) != null, spawns);
                if (ended == null) { yield return Shot30(dir, $"{tag}-{r.id}-b"); shots++; }
                string owned = sim.loadout != null ? string.Join("+", sim.loadout.owned) : "?";
                string spawnPart = "";
                if (!arenas)
                {
                    double sNear = spawns.Nearest(r, out double sAng, out int sN);
                    if (sNear >= 0 && sNear < Spawn31MinMetres - 1e-9) nearBad++;
                    spawnPart = $"nearest SPAWN to the entry {(sNear < 0 ? "none" : sNear.ToString("0.0", ci) + " m at " + sAng.ToString("0", ci) + " deg")} ({sN} spawns in the room; rule >= {Spawn31MinMetres.ToString("0", ci)} m), ";
                }
                lines.Add($"{r.id} [{r.kind}] entered {inRoom}: map {g.MapId}, lesson '{lesson}', enemies alive {Alive30(sim)}, {spawnPart}nearest enemy {(arenas ? "to the player" : "to the entry")} at {tA.ToString("0.0", ci)} s {(near < 0 ? "none" : near.ToString("0.0", ci) + " m at " + ang.ToString("0", ci) + " deg")} (they move), owned {owned}, health {sim.health.hp.ToString("0", ci)}, feet ({g.Feet.x.ToString("0.0", ci)}, {g.Feet.y.ToString("0.0", ci)}, {g.Feet.z.ToString("0.0", ci)}){(ended != null ? " | then " + ended : "")}");
            }
            yield return Stop();
            bool lastResults = ended != null && ended.StartsWith("Results") && entered == want.Count;
            bool ok = entered == want.Count && nearBad == 0 && shots >= (arenas ? 1 : 2) * want.Count - (lastResults ? 1 : 0) && (ended == null || lastResults);
            res[0] = $"{(ok ? "PASS" : "RED")}: {entered} of {want.Count} {(arenas ? "boss rooms" : "weapon rooms")} entered by respawn, {shots} frames (shot-N31-{tag}-<room>-a/b.png){(ended != null ? " | ended: " + ended : "")} | {pc} || " + string.Join(" || ", lines);
        }

        /// <summary>builder G's id spawn31, written by L (P0 c). The lead routes it in FeelReel.N27.cs (see the file header).</summary>
        public static IEnumerator Spawn31Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            SurvivalSim sim = g.sim;
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(30);
            God(g);
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN"; yield break; }
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            sim.campaign?.SetDoorFree29(true);
            LevelDef lv = sim.campaignLevel;
            RoomDef f9 = lv.rooms.Find(r => r.id == "f9");
            if (f9 == null) { res[0] = $"RED: level '{lv.id}' has no room f9 (boot -skygrindMap n30-foundry)"; yield break; }
            string pc = Preconditions(g, g.Feet) + $" | level {lv.id}";
            Start(g, "spawn31", "camera", dir);
            yield return Frames(pre);
            g.Respawn(f9.spawn, f9.spawnYawDeg);
            string ended = null;
            int frames = 0;
            while (Alive30(sim) == 0 && frames < 60 * 10 && (ended = Ended30(g)) == null) { God(g); frames++; yield return null; }
            double near = Nearest31(sim, f9.spawn, f9.spawnYawDeg, out double ang);
            int alive = Alive30(sim);
            yield return Shot30(dir, "spawn31-f9");
            yield return Stop();
            bool ok = near >= Spawn31MinMetres - 1e-9;
            res[0] = $"{(ok ? "PASS" : "RED")}: f9's first wave ({alive} alive after {frames} frames): the nearest enemy is {(near < 0 ? "none" : near.ToString("0.0", ci) + " m")} from the entry ({ang.ToString("0", ci)} deg off its facing; the rule: >= {Spawn31MinMetres.ToString("0", ci)} m), frame shot-N31-spawn31-f9.png{(ended != null ? " | ended: " + ended : "")} | {pc}";
        }
    }
}
