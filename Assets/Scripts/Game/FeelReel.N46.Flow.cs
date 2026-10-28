// NIGHT46 builder F1 (NIGHT46.md card F1, round 3; the lead's ruling: phase a646, one routing line in FeelReel.N46.cs): the Player frames of F1's
// flow rows. Not run by its author (no Player in a builder worktree). The rows are forced ON inside the reel, before the run starts
// (sim.rowsOverride = rowsRegistry.Defaults().Set(row, true) for retry46, overlap46, teach46, stuck46, reveal46), whatever the build's copy says.
// One Player per level (L-046); a campaign level boots only through reel.sh (L-062):
//   level 1  -skygrindMap n45-foundry -skygrindReelOnly a646   a2 at 8 s (teach46), b1 a death in wave 2 + the restart (retry46), a9 reveal
//   level 2  -skygrindMap n45-iceline -skygrindReelOnly a646   g7 wave 2 held at wave 1's last body, then wave 3 at the door (overlap46's holds)
//   level 3  -skygrindMap n45-kiln    -skygrindReelOnly a646   kb2 an overlapped wave entering: at the release and +1 s (overlap46)
//   level 4  -skygrindMap n40-span    -skygrindReelOnly a646   o3 reveal (reveal46)
//   level 5  -skygrindMap n41-spire   -skygrindReelOnly a646   v11 reveal (reveal46)
// (an n46-* level boots the same sections through NightRows.BaseId.) Frames: dir/shot-N29-F1-<name>.png (no numbered frames: DECIDE46 D-1).
//   F1-a2-8s            a2 KESTREL 8.2 s after the entry: teach46's cap let wave 1 in with the stand-ins still up (the line: stand-ins alive, wave)
//   F1-b1-wave2         b1 THE SPILL fighting wave 2 (just before the reel's death: the player's health set to 0, the reel's one cheat)
//   F1-b1-restart       1.0 s after the retry: back at b1's entry, the HUD counter (rooms43's counter43) showing wave 2's bodies, not wave 1's
//   F1-b1-restart-4s    4 s after the retry: wave 2 again in the room (the waves won stay won)
//   F1-a9-reveal-0.5 / F1-a9-reveal-1.2, F1-o3-reveal-0.5 / -1.2, F1-v11-reveal-0.5 / -1.2: after the boss door; at 1.2 s the brain is
//                       out of its Reveal (reveal46: 1 s; off, 2 s): the line prints the brain's state at each frame
//   F1-g7-wave2-held    g7 THE THAW LOCK with wave 1's last body standing and wave 2 NOT in (growthMax: 3 -> 7 waits for the clear)
//   F1-g7-wave3-door    0.5 s after wave 3 spawns, looking back at the entry (entryM: it waits for the clear and comes in by the door)
//   F1-kb2-overlap-release / F1-kb2-overlap-1s   kb2 POUR HALL the moment a wave is released with bodies of the last one standing, and 1 s on
// The player is God except in b1 (the death). The walk pilot (Walk38Pilot) plays every live section. Line: GREEN when every frame of the level
// was saved and every row read ON; each frame's note (wave, alive, the counter, the boss state).
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
        static readonly string[] F1Rows46 = { "retry46", "overlap46", "teach46", "stuck46", "reveal46" };

        static partial void N46Flow(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = F1Flow46Scenario(g, dir, res, offs, pre);

        /// <summary>The boss brains' states (reveal46's frames): "crown37 Reveal", or "-" when no boss module runs.</summary>
        static string F1BossState46(SurvivalSim sim)
        {
            var l = new List<string>();
            if (sim.lantern36 != null) l.Add("lantern36 " + sim.lantern36.brain.state);
            if (sim.wick37 != null && sim.wick37.brain != null) l.Add("wick37 " + sim.wick37.brain.state);
            if (sim.stag37 != null && sim.stag37.brain != null) l.Add("stag37 " + sim.stag37.brain.state);
            if (sim.loom37 != null) l.Add("loom37 " + sim.loom37.brain.state);
            if (sim.crown37 != null) l.Add("crown37 " + sim.crown37.brain.state);
            if (sim.brawl40 != null) l.Add("brawl40 " + sim.brawl40.state);
            return l.Count == 0 ? "-" : string.Join(" / ", l);
        }

        static IEnumerator F1Flow46Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = "RED a646: " + why; yield break; }
            // the rows ON for this run, set before StartRun reads them
            NightRowState rowsOn = sim.rowsRegistry.Defaults();
            var missing = new List<string>();
            foreach (string r in F1Rows46)
            {
                if (sim.rowsRegistry.IndexOf(r) < 0) missing.Add(r);
                else rowsOn = rowsOn.Set(r, true);
            }
            sim.rowsOverride = rowsOn;
            double maxHp = g.health.max;
            offs.Add(() => { sim.rowsOverride = null; g.health.max = maxHp; });
            AutopilotInput ap = N24StartCampaign(g, offs);
            var pilot = new Walk38Pilot(sim, ap);
            sim.UseInput(pilot);
            yield return Frames(240); // the run-start title has run out
            for (int k = 0; k < 240 && (sim.campaign == null || sim.campaign.room < 0); k++) { AutopilotInput.TakeDraft(sim); yield return null; }
            if (!sim.campaignOn || sim.campaign == null || sim.campaignLevel == null) { res[0] = "RED a646: the run did not start as a CAMPAIGN"; yield break; }
            LevelDef lv = sim.campaignLevel;
            string baseId = NightRows.BaseId(g.cfg, lv.id);
            string rowsLine = $"rows read: retry46 {sim.rows.On(sim.idRetry46)}, overlap46 {sim.rows.On(sim.idOverlap46)}, teach46 {sim.rows.On(sim.idTeach46)}, stuck46 {sim.rows.On(sim.idStuck46)}, reveal46 {sim.rows.On(sim.idReveal46)}{(missing.Count > 0 ? " (NOT IN THIS BUILD: " + string.Join(", ", missing) + ")" : "")}";
            bool rowsOk = missing.Count == 0 && sim.rows.On(sim.idRetry46) && sim.rows.On(sim.idOverlap46) && sim.rows.On(sim.idTeach46) && sim.rows.On(sim.idStuck46) && sim.rows.On(sim.idReveal46);
            float wallEnd = Time.realtimeSinceStartup + 900;
            var notes = new List<string>();
            var shots = new List<string>();
            var stats = new double[4];
            int want = 0;
            string Room() => sim.campaign != null && sim.campaign.room >= 0 && sim.campaign.room < lv.rooms.Count ? lv.rooms[sim.campaign.room].id : "?";
            string Feet() => $"({g.Feet.x.ToString("0.0", ci)}, {g.Feet.y.ToString("0.0", ci)}, {g.Feet.z.ToString("0.0", ci)})";
            int Alive() { int n = 0; foreach (Enemy e in sim.enemies.all) if (e.alive && !e.standIn29) n++; return n; }

            IEnumerator Shot(string name, string note)
            {
                want++;
                yield return Shot29(dir, "F1-" + name, stats);
                shots.Add(name);
                notes.Add($"{name}: room {Room()}, wave {sim.campaign?.wave}, alive {Alive()}, counter {sim.campaign?.RoomLeft43()}, feet {Feet()}{(note.Length > 0 ? ", " + note : "")}");
            }
            IEnumerator Place(string id, bool[] ok)
            {
                ok[0] = false;
                RoomDef rd = lv.rooms.Find(r => r.id == id);
                if (rd == null) { notes.Add($"{id}: NOT IN {lv.id}"); yield break; }
                DoorFree41(g);
                g.Respawn(rd.spawn, rd.spawnYawDeg);
                float end = Time.realtimeSinceStartup + 6;
                while (Room() != id && Time.realtimeSinceStartup < end) { AutopilotInput.TakeDraft(sim); yield return null; }
                sim.campaign.SetDoorFree29(false); // the room's own rules from here (its doors lock as in play)
                ok[0] = Room() == id;
                if (!ok[0]) notes.Add($"{id}: NOT PLACED (the campaign's room {Room()})");
            }
            IEnumerator Until(Func<bool> test, double maxS, bool[] hit, bool god)
            {
                hit[0] = false;
                double t0 = sim.SimTime;
                while (sim.SimTime - t0 < maxS && Time.realtimeSinceStartup < wallEnd)
                {
                    if (god) God(g);
                    AutopilotInput.TakeDraft(sim);
                    if (test()) { hit[0] = true; yield break; }
                    yield return null;
                }
            }
            IEnumerator Reveal(string id)
            {
                var ok = new bool[1];
                God(g);
                yield return Place(id, ok);
                if (!ok[0]) yield break;
                double t0 = sim.SimTime;
                var hit = new bool[1];
                yield return Until(() => sim.SimTime - t0 >= 0.5, 5, hit, true);
                yield return Shot(id + "-reveal-0.5", $"t {(sim.SimTime - t0).ToString("0.00", ci)} s, boss {F1BossState46(sim)}");
                yield return Until(() => sim.SimTime - t0 >= 1.2, 5, hit, true);
                yield return Shot(id + "-reveal-1.2", $"t {(sim.SimTime - t0).ToString("0.00", ci)} s, boss {F1BossState46(sim)}");
            }

            if (baseId == "n45-foundry")
            {
                // a2 at 8 s (teach46)
                var ok = new bool[1];
                var hit = new bool[1];
                God(g);
                yield return Place("a2", ok);
                if (ok[0])
                {
                    double t0 = sim.SimTime;
                    yield return Until(() => sim.SimTime - t0 >= 8.2, 12, hit, true);
                    int standIns = 0;
                    foreach (Enemy e in sim.enemies.all) if (e.alive && e.standIn29) standIns++;
                    yield return Shot("a2-8s", $"t {(sim.SimTime - t0).ToString("0.0", ci)} s, stand-ins alive {standIns}");
                }
                // b1: a death in wave 2, the restart (retry46)
                yield return Place("b1", ok);
                if (ok[0])
                {
                    g.health.max = maxHp;
                    g.health.hp = maxHp;
                    yield return Until(() => sim.campaign.wave >= 2 && Alive() >= 2, 120, hit, false);
                    if (hit[0])
                    {
                        yield return Shot("b1-wave2", "the reel kills the player next");
                        int retries = sim.campaign.retries, waveAtDeath = sim.campaign.wave;
                        g.health.hp = 0; // the reel's cheat: a death in wave 2
                        yield return Until(() => sim.campaign.retries > retries, 10, hit, false);
                        double tr = sim.SimTime;
                        yield return Until(() => sim.SimTime - tr >= 1.0, 3, hit, false);
                        yield return Shot("b1-restart", $"waves spawned at the death {waveAtDeath}, retries {sim.campaign.retries}");
                        yield return Until(() => sim.SimTime - tr >= 4.0, 6, hit, false);
                        yield return Shot("b1-restart-4s", $"waves spawned at the death {waveAtDeath}");
                    }
                    else notes.Add("b1: wave 2 not reached in 120 s");
                }
                yield return Reveal("a9");
            }
            else if (baseId == "n45-iceline")
            {
                var ok = new bool[1];
                var hit = new bool[1];
                God(g);
                yield return Place("g7", ok);
                if (ok[0])
                {
                    RoomDef rd = lv.rooms.Find(r => r.id == "g7");
                    yield return Until(() => sim.campaign.wave == 1 && Alive() == 1, 120, hit, true);
                    if (hit[0]) yield return Shot("g7-wave2-held", "wave 1's last body standing, wave 2 waiting (growthMax)");
                    else notes.Add("g7: wave 1's last body never seen alone");
                    yield return Until(() => sim.campaign.wave >= 3, 150, hit, true);
                    if (hit[0])
                    {
                        double t3 = sim.SimTime;
                        yield return Until(() => sim.SimTime - t3 >= 0.3, 2, hit, true);
                        double yaw = Math.Atan2(-(rd.spawn.x - g.Feet.x), -(rd.spawn.z - g.Feet.z));
                        sim.UseInput(new A1HoldLook45(yaw, 0.05));
                        g.input.SetLook(yaw, 0.05);
                        yield return Until(() => sim.SimTime - t3 >= 0.5, 2, hit, true);
                        yield return Shot("g7-wave3-door", "looking back at the entry (entryM)");
                        sim.UseInput(pilot);
                    }
                    else notes.Add("g7: wave 3 not reached in 150 s");
                }
            }
            else if (baseId == "n45-kiln")
            {
                var ok = new bool[1];
                var hit = new bool[1];
                God(g);
                yield return Place("kb2", ok);
                if (ok[0])
                {
                    int lastWave = sim.campaign.wave, lastAlive = Alive(), aliveAtRelease = 0;
                    yield return Until(() =>
                    {
                        bool released = sim.campaign.wave > lastWave && lastWave >= 1 && lastAlive > 0;
                        if (released) aliveAtRelease = lastAlive;
                        lastWave = sim.campaign.wave;
                        lastAlive = Alive();
                        return released;
                    }, 150, hit, true);
                    if (hit[0])
                    {
                        double tr = sim.SimTime;
                        yield return Shot("kb2-overlap-release", $"wave {sim.campaign.wave} released with {aliveAtRelease} of the last standing");
                        yield return Until(() => sim.SimTime - tr >= 1.0, 3, hit, true);
                        yield return Shot("kb2-overlap-1s", "1 s after the release");
                    }
                    else notes.Add("kb2: no overlapped release in 150 s");
                }
            }
            else if (baseId == "n40-span") yield return Reveal("o3");
            else if (baseId == "n41-spire") yield return Reveal("v11");
            else { res[0] = $"RED a646: level {lv.id} (BaseId {baseId}) has no a646 section (boot n45-foundry / n45-iceline / n45-kiln / n40-span / n41-spire)"; yield break; }

            bool green = rowsOk && want > 0 && shots.Count == want && notes.TrueForAll(n => !n.Contains("NOT") && !n.Contains("not reached") && !n.Contains("never"));
            int expected = baseId == "n45-foundry" ? 6 : baseId == "n45-iceline" || baseId == "n45-kiln" || baseId == "n40-span" || baseId == "n41-spire" ? 2 : 0;
            green &= shots.Count == expected;
            res[0] = $"{(green ? "GREEN" : "RED")} a646 | level {lv.id} (BaseId {baseId}) | {rowsLine} | frames {shots.Count}/{expected}: {string.Join(", ", shots)} | {string.Join(" | ", notes)}";
        }
    }
}
