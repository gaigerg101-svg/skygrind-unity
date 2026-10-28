// NIGHT43 builder L1 (foundry43; GG G1 / G2 / G4 / G11): the Player scenario `foundry43`. Not run by its author (no Player in the worktree).
// Boot: level 1 in the campaign (reel.sh + -skygrindReelOnly foundry43, L-062); with foundry43 on the campaign's level 1 is n43-foundry, with
// -skygrindOff43 it is n42-foundry (the BEFORE: the same scenario; positions are room-relative, so it frames either file; n42's rooms have no
// perch / dash gap, so some AFTER-only frames stay unsaved there and the line says which).
// DOORFREE (FeelReel.N41's DoorFree41), the player put at each room's spawn, the walk pilot (Walk38Pilot: Walk43Foundry.cs's scripts) plays:
//   THE GANTRY (b2): L43-gantry-lip (gate 1's run-up, lz >= 6.5 grounded), L43-gantry-mid (riding gate 1's one wall, lz 22-34),
//     L43-gantry-dash (gate 2: the dash across the gap, the Mover in Dash at lz 72-82), L43-gantry-land (the far landing, lz >= 94.5 grounded);
//   THE FLUE (a7): L43-flue-mid (riding in the chimney 3-6 m up), L43-flue-deck (on the deck or past it);
//   PIERCE (a8): L43-pierce-ride1 (on P1), L43-pierce-perch (grounded on the perch), L43-pierce-hatch (the first husk alive: its spawn tell),
//     L43-pierce-shot (riding P2 with a husk alive: the shot from the wall), L43-pierce-door (the room cleared, the far floor toward d10).
// Each section waits at most 40 s of game time. Line: GREEN when all eleven frames were saved; the level id, foundry43 / rooms43, per room the
// time, rides, dashes, pit falls.
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
        static partial void N43Foundry(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Foundry43Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Foundry43Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            AutopilotInput ap = N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            var pilot = new Walk38Pilot(sim, ap);
            sim.UseInput(pilot);
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "SKIPPED (RED): the run did not start as a CAMPAIGN"; yield break; }
            LevelDef lv = sim.campaignLevel;
            string pc = Preconditions(g, g.Feet);
            float wallEnd = Time.realtimeSinceStartup + 300;
            int frames = 0;
            var notes = new List<string>();
            // n43-r3 (RED 0/11, the player never left (0, -2)): a Respawn sent before the campaign's first room had begun was undone by the run's
            // start. Wait for the campaign's room 0 to be in its fight, then free the doors and place the player (as first42's gantry part did after
            // its first minute)
            while ((sim.campaign == null || sim.campaign.room < 0 || sim.campaign.phase == Campaign24.Phase.None) && Time.realtimeSinceStartup < wallEnd - 280)
            {
                AutopilotInput.TakeDraft(sim);
                yield return null;
            }
            yield return Frames(20);
            notes.Add($"start: flow {sim.flow}, room {(sim.campaign != null && sim.campaign.room >= 0 ? lv.rooms[sim.campaign.room].id : "?")}, phase {sim.campaign?.phase}, step {sim.simStep}");
            DoorFree41(g);

            // one room: put the player at its spawn, step until `done` or 40 s, saving each named frame once when its test first holds
            IEnumerator Room(string id, string[] names, Func<RoomDef, double, int, bool>[] tests)
            {
                RoomDef rd = lv.rooms.Find(r => r.id == id);
                if (rd == null) { notes.Add($"{id}: no such room"); yield break; }
                DoorFree41(g);
                g.Respawn(rd.spawn, rd.spawnYawDeg); // the game's own placement (the view snapped with the body)
                g.input.SetLook(rd.spawnYawDeg * Math.PI / 180, 0);
                // the campaign takes the room when the feet are in its zone: wait for it (the room's scripts in Walk43Foundry.cs key on it)
                float placeEnd = Time.realtimeSinceStartup + 5;
                while ((sim.campaign == null || sim.campaign.room < 0 || lv.rooms[sim.campaign.room].id != id) && Time.realtimeSinceStartup < placeEnd)
                {
                    AutopilotInput.TakeDraft(sim);
                    yield return null;
                }
                string inRoom = sim.campaign != null && sim.campaign.room >= 0 ? lv.rooms[sim.campaign.room].id : "?";
                if (inRoom != id) { notes.Add($"{id}: NOT PLACED (the campaign's room {inRoom}, feet ({g.Feet.x:0.0}, {g.Feet.y:0.0}, {g.Feet.z:0.0}))"); yield break; }
                Start(g, "foundry43-" + id, "camera", dir + "/" + id);
                double t0 = sim.SimTime;
                var saved = new bool[names.Length];
                int got = 0, rides0 = pilot.rides, falls0 = pilot.pitFalls, dashes = 0;
                MoveState was = sim.mover.state;
                while (sim.SimTime - t0 < 40 && Time.realtimeSinceStartup < wallEnd && got < names.Length)
                {
                    AutopilotInput.TakeDraft(sim);
                    double lz = rd.z1 - sim.mover.position.z;
                    if (sim.mover.state == MoveState.Dash && was != MoveState.Dash) dashes++;
                    was = sim.mover.state;
                    bool shot = false;
                    for (int k = 0; k < names.Length; k++)
                    {
                        if (saved[k] || !tests[k](rd, lz, k)) continue;
                        saved[k] = true;
                        got++;
                        frames++;
                        shot = true;
                        yield return Shot30(dir, names[k]);
                        break;
                    }
                    if (!shot) yield return null;
                }
                yield return Stop();
                var missing = new List<string>();
                for (int k = 0; k < names.Length; k++) if (!saved[k]) missing.Add(names[k]);
                notes.Add($"{id}: {got}/{names.Length} frames in {(sim.SimTime - t0).ToString("0.0", ci)} s, rides {pilot.rides - rides0}, dashes {dashes}, pit falls {pilot.pitFalls - falls0}"
                    + (missing.Count > 0 ? " (not saved: " + string.Join(", ", missing) + ")" : ""));
            }

            bool Riding() => sim.mover.state == MoveState.WallRide;
            bool Grounded() => sim.mover.grounded;
            Vec3 P() => sim.mover.position;
            yield return Room("b2", new[] { "L43-gantry-lip", "L43-gantry-mid", "L43-gantry-dash", "L43-gantry-land" }, new Func<RoomDef, double, int, bool>[]
            {
                (r, lz, k) => lz >= 6.5 && Grounded(),
                (r, lz, k) => Riding() && lz >= 22 && lz <= 34,
                (r, lz, k) => sim.mover.state == MoveState.Dash && lz >= 72 && lz <= 82,
                (r, lz, k) => lz >= 94.5 && Grounded(),
            });
            yield return Room("a7", new[] { "L43-flue-mid", "L43-flue-deck" }, new Func<RoomDef, double, int, bool>[]
            {
                (r, lz, k) => Riding() && P().y >= 3 && P().y <= 6,
                (r, lz, k) => Grounded() && (P().y > 7.4 || lz > 26),
            });
            yield return Room("a8", new[] { "L43-pierce-ride1", "L43-pierce-perch", "L43-pierce-hatch", "L43-pierce-shot", "L43-pierce-door" }, new Func<RoomDef, double, int, bool>[]
            {
                (r, lz, k) => Riding() && sim.mover.wallNormal.x > 0 && lz >= 4,
                (r, lz, k) => Grounded() && P().y > -0.3 && lz > 10.5 && lz < 15.8,
                (r, lz, k) => sim.enemies.AliveCount > 0,
                (r, lz, k) => Riding() && sim.mover.wallNormal.x < 0 && sim.enemies.AliveCount > 0,
                (r, lz, k) => sim.campaign != null && sim.campaign.phase == Campaign24.Phase.Cleared && lz >= 19,
            });
            bool ok = frames == 11;
            res[0] = $"{(ok ? "GREEN" : "RED")} foundry43 | level {lv.id} (N41BaseId {sim.N41BaseId(lv.id)}), foundry43 {sim.n43.foundry43}, rooms43 {sim.n43.rooms43}, "
                + $"chimney43 {sim.n43.chimney43}, ride43 {sim.n43.ride43} | frames {frames}/11 | {string.Join(" | ", notes)} | {pc}";
        }
    }
}
