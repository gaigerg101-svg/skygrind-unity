// NIGHT30 builder L: the Player scenarios of the rebuilt levels (the names are FeelReel.N27.cs PhaseN30L; the body is the partial N30LevelsBody).
// Not run by its author (no Player in the worktree).
//   rooms30  a CAMPAIGN level (boot -skygrindMap n30-foundry | n30-iceline | n30-kiln | n30-span): DOORFREE set, God on; for each room in order the player is
//            respawned at the room's entry (its spawn), the sim settles 0.5 s, the frame is saved as shot-N30-rooms30-<room>.png and one line is printed per
//            room: the lesson text on screen now (Ui.Lesson25TextNow) and the enemies alive (stand-ins apart). PASS when every room was entered (the
//            campaign's room index matched) and a frame saved per room.
//   span30   the same tour on n30-span (RED on another map), then in the first room with a grapple node: a grapple press aimed at that node must attach ON
//            the node (ropeAttach node).
//   walk30   not a Player scenario: the pilot runs in the harness (`walk30 <level>`, Tools/SimHarness/Harness.N30.Levels.cs); left NOT WRITTEN here.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Btn = Skygrind.Core.Button;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N30LevelsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "rooms30") body = Rooms30Scenario(g, dir, res, offs, pre, false);
            else if (s == "span30") body = Rooms30Scenario(g, dir, res, offs, pre, true);
        }

        /// <summary>Non-null when the run left play: Results (a level cleared or a death screen) or Down; the scenario stops instead of waiting on a SimTime that no longer moves.</summary>
        static string Ended30(SurvivalGame g) => g.sim.flow == Flow.Results ? $"Results at step {g.sim.simStep}" : g.sim.flow == Flow.Down ? $"Down at step {g.sim.simStep}" : null;

        static int Alive30(SurvivalSim sim)
        {
            int n = 0;
            foreach (Enemy e in sim.enemies.all) if (e.alive && !e.standIn29) n++;
            return n;
        }

        static IEnumerator Rooms30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, bool span)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string tag = span ? "span30" : "rooms30";
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            SurvivalSim sim = g.sim;
            if (span && sim.map.grappleNodes.Count == 0) { res[0] = $"RED: the booted map '{g.MapId}' has no grapple nodes (boot -skygrindMap n30-span)"; yield break; }
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
            string pc = Preconditions(g, g.Feet) + $" | level {sim.campaignLevel.id}, n30.levels {g.cfg.Design("n30.levels.enabled")}, n30.lava {sim.n30.lava}, n30.span {sim.n30.span}";
            int attachNode = 0;
            offs.Add(g.events.On<Arcline21RopeEvent>("ropeAttach", e => { if (e.node) attachNode++; }));
            Start(g, tag, "camera", dir);
            yield return Frames(pre);
            LevelDef lv = sim.campaignLevel;
            var lines = new List<string>();
            int entered = 0;
            string ended = null; // round 3: why the scenario stopped early (Results / Down / its own timeout), instead of waiting for ever
            int frameCap = 60 * 120; // the scenario's own timeout: 60 s of game time at 120 Hz, counted in frames (SimTime stops on Results)
            int frames = 0;
            string hook = "";
            // span30: the grapple FIRST (round 3: on the Player the tour's last respawn, into the boss room s8, cleared the level and the run went to
            // Results at step 519; the grapple waited on a SimTime that no longer moved)
            if (span)
            {
                Vec3 node = sim.map.grappleNodes[0];
                RoomDef nodeRoom = null;
                foreach (RoomDef r in lv.rooms)
                    if (node.x >= r.x0 && node.x <= r.x1 && node.z >= r.z0 && node.z <= r.z1) { nodeRoom = r; break; }
                if (nodeRoom != null) g.Respawn(nodeRoom.spawn, nodeRoom.spawnYawDeg);
                for (double t0 = sim.SimTime; sim.SimTime - t0 < 0.5 && ended == null && frames < frameCap; frames++) { God(g); ended = Ended30(g); yield return null; }
                bool owned = sim.arcline21 != null && sim.arcline21.Owned;
                for (int i = 0; i < 240 && attachNode == 0 && ended == null; i++, frames++)
                {
                    God(g);
                    Vec3 eye = sim.Eye, d = node - eye;
                    double yaw = Math.Atan2(-d.x, -d.z), pitch = Math.Asin(d.y / Math.Max(1e-6, d.Length));
                    g.input.SetLook(yaw, pitch);
                    g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch, grapple = new Btn { held = true, pressed = i == 0 } };
                    ended = Ended30(g);
                    yield return null;
                }
                yield return Frames(30);
                yield return Shot30(dir, $"{tag}-grapple");
                g.forcedMove = new InputFrame();
                hook = $" | grapple: ARCLINE owned {owned}, node ({node.x.ToString("0.0", ci)}, {node.y.ToString("0.0", ci)}, {node.z.ToString("0.0", ci)}) in room {(nodeRoom != null ? nodeRoom.id : "?")}, attached on a node {attachNode}";
            }
            // the tour: every room by a respawn at its spawn; the boss room last (entering it may clear the level: Results ends the tour as a note, not a hang)
            for (int k = 0; k < lv.rooms.Count && ended == null; k++)
            {
                RoomDef r = lv.rooms[k];
                g.Respawn(r.spawn, r.spawnYawDeg);
                for (double t0 = sim.SimTime; sim.SimTime - t0 < 0.5 && ended == null && frames < frameCap; frames++) { God(g); ended = Ended30(g); yield return null; }
                if (frames >= frameCap) ended ??= "TIMEOUT (60 s of frames)";
                // entered: the campaign's room index, or the feet inside the room's rect (the campaign only moves forward: span30's grapple in s2 comes first, so a
                // respawn back into s1 leaves the campaign at s2; the r5 Player run printed "s1 entered False" for that)
                Vec3 ft = g.Feet;
                bool inRect = ft.x >= r.x0 && ft.x <= r.x1 && ft.z >= r.z0 && ft.z <= r.z1;
                bool inRoom = sim.campaign != null && sim.campaign.room == k || inRect;
                if (inRoom) entered++;
                yield return Shot30(dir, $"{tag}-{r.id}");
                string lesson = Ui.Lesson25Visible && !string.IsNullOrEmpty(Ui.Lesson25TextNow) ? Ui.Lesson25TextNow : "(none)";
                lines.Add($"{r.id} [{r.kind}] entered {inRoom}: lesson '{lesson}', enemies alive {Alive30(sim)}, health {sim.health.hp.ToString("0", ci)}, feet ({g.Feet.x.ToString("0.0", ci)}, {g.Feet.y.ToString("0.0", ci)}, {g.Feet.z.ToString("0.0", ci)}){(ended != null ? " | then " + ended : "")}");
            }
            yield return Stop();
            // a boss room with no boss yet (s8 before THE LOOM) clears on entry and the level ends: that Results after the LAST room is expected, not a failure
            bool lastResults = ended != null && ended.StartsWith("Results") && entered == lv.rooms.Count;
            bool ok = entered == lv.rooms.Count && (ended == null || lastResults) && (!span || attachNode > 0);
            res[0] = $"{(ok ? "PASS" : "RED")}: {entered} of {lv.rooms.Count} rooms entered by respawn, a frame each (shot-N30-{tag}-<room>.png){hook}{(ended != null ? " | ended: " + ended : "")} | {pc} || " + string.Join(" || ", lines);
        }
    }
}
