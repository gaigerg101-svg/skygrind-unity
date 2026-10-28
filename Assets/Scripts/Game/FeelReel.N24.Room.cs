// NIGHT24 reel `room` (builder L; UNCOMPILED, written without the Editor): the CAMPAIGN's room loop on the Player. A bot plays layer 1's first
// two rooms (the AUTOPILOT with a goal provider, as the headless `campaign` command does): room 1 locks nothing behind it (the start),
// its encounter spawns only at the room's own nodes, the exit door opens free when the last wave is dead, entering room 2 LOCKS the door
// behind (Zones.Lock) and sets the checkpoint; then one death in room 2 and the room restarts in under 1.5 s of game time.
// Boot the Player on the level: `-skygrindMap layer1` (design.maps.list must hold {id: layer1, file: Design/levels/layer1.json}).
// RED: the scaffold's "NOT WRITTEN (RED)" until this file exists; "SKIPPED (RED)" while design.n24.campaign is off or the booted map is
// not a campaign level. The shared helpers below (gate, start, pilot) are used by the `limit` and `layer1` scenarios too.
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        /// <summary>Why the campaign scenarios cannot run, or null.</summary>
        static string N24CampaignGate(SurvivalGame g)
        {
            if (N24Blocks.ForceOff || !N24Blocks.From(g.cfg).campaign) return "SKIPPED (RED): design.n24.campaign is off";
            if (g.sim.map.json == null || !g.sim.map.json.ContainsKey("campaign")) return "SKIPPED (RED): the booted map is not a campaign level (boot the Player with -skygrindMap layer1)";
            return null;
        }

        /// <summary>A CAMPAIGN run on the level, the bot at the controls; the offs put the mode and the input back.</summary>
        static AutopilotInput N24StartCampaign(SurvivalGame g, List<Action> offs)
        {
            Quiet(g);
            g.noRecords = true;
            g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            IPlayerInput previous = g.sim.input;
            var ap = new AutopilotInput(g.sim, "normal", Seed);
            SurvivalSim sim = g.sim;
            ap.GoalProvider = () =>
            {
                Campaign24 c = sim.campaign;
                LevelDef lv = sim.campaignLevel;
                if (c == null || c.room < 0) return lv.rooms[0].spawn;
                if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].exit;
                return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
            };
            g.StartRun(Seed);
            g.sim.UseInput(ap);
            offs.Add(() =>
            {
                g.sim.UseInput(previous);
                ap.Detach();
                g.setup.mode = RunSetup.Survival;
            });
            return ap;
        }

        static partial void N24RoomBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = N24RoomScenario(g, dir, res, offs, pre);
        }

        static IEnumerator N24RoomScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            string why = N24CampaignGate(g);
            if (why != null)
            {
                res[0] = why;
                yield break;
            }
            int enters = 0, clears = 0, checkpoints = 0, locks = 0, opens = 0, retries = 0, spawnsOutside = 0, spawns = 0;
            string lockedWhenEntered = "-";
            double clearAt = -1;
            offs.Add(g.events.On<RoomEvent>("roomEnter", _ => enters++));
            offs.Add(g.events.On<RoomEvent>("roomClear", e => { clears++; if (clearAt < 0) clearAt = e.time; }));
            offs.Add(g.events.On<RoomEvent>("checkpoint", _ => checkpoints++));
            offs.Add(g.events.On("gateLock", _ => locks++));
            offs.Add(g.events.On<GateOpenEvent>("gateOpen", _ => opens++));
            offs.Add(g.events.On<RoomEvent>("roomRetry", _ => retries++));
            offs.Add(g.events.On<SpawnEvent>("enemySpawn", e =>
            {
                spawns++;
                Campaign24 c = g.sim.campaign;
                if (c == null || c.Room == null) return;
                bool atNode = c.Room.nodes.Exists(n => Math.Abs(n.x - e.position.x) < 4.5 && Math.Abs(n.z - e.position.z) < 4.5); // a group spreads around its node
                if (!atNode) spawnsOutside++;
            }));
            AutopilotInput ap = N24StartCampaign(g, offs);
            string pre0 = Preconditions(g, g.Feet);
            if (!g.sim.campaignOn)
            {
                res[0] = "SKIPPED (RED): the run did not start as a CAMPAIGN";
                yield break;
            }
            yield return Frames(20);
            Start(g, "room", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            // room 1 to its clear, then into room 2
            for (int i = 0; i < Ms(120000) && !(g.sim.campaign != null && g.sim.campaign.room >= 1); i++)
            {
                AutopilotInput.TakeDraft(g.sim);
                yield return null;
            }
            Campaign24 camp = g.sim.campaign;
            bool inRoom2 = camp != null && camp.room == 1;
            if (inRoom2) lockedWhenEntered = g.sim.zones.IsOpen(g.sim.campaignLevel.doors[0].id) ? "OPEN (WRONG)" : "locked";
            int checkpointAtEntry = camp != null ? camp.checkpoint : -1;
            yield return Frames(Ms(1500));
            // one death in room 2: the restart within 1.5 s of game time
            int deathFrame = 0, retryFrames = -1, stepsAtDeath = 0, retrySteps = -1;
            if (inRoom2)
            {
                g.health.Damage(g.health.max * 4, "husk", g.Feet);
                stepsAtDeath = g.sim.simStep;
                for (int i = 0; i < Ms(10000); i++) // the death RECAP (4 s) holds the sim: wait in frames, measure in sim steps
                {
                    deathFrame++;
                    yield return null;
                    if (retries > 0) { retryFrames = deathFrame; retrySteps = g.sim.simStep - stepsAtDeath; break; }
                }
                yield return Frames(Ms(1500));
            }
            yield return Stop();
            double retrySeconds = retrySteps < 0 ? double.NaN : retrySteps * g.sim.step; // GAME time (sim steps), not frames
            res[0] = $"{pre0}; {FeelProbe.FrameCount} frames; roomEnter {enters}x, roomClear {clears}x (room 1 in {clearAt:0.0} s), exit door opened free {opens}x, door behind when room 2 was entered: {lockedWhenEntered}, checkpoint {checkpointAtEntry} ({checkpoints} events), " +
                     $"{spawns} enemies spawned, {spawnsOutside} outside the room's own nodes; death: restart after {retrySeconds:0.00} s of game time ({retryFrames} frames) (want <= 1.5), retries {retries}, flow {g.flow}";
        }
    }
}
