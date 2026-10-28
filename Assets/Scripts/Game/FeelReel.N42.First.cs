// NIGHT42 builder L1 (foundry42; GG G1, G6): the Player scenario `first42`. Not run by its author (no Player in the worktree).
// Boot: level 1 in the campaign (reel.sh + -skygrindReelOnly first42, L-062); with foundry42 on the campaign's level 1 is n42-foundry, with
// -skygrindOff42 it is n41-foundry (the BEFORE, the same scenario: every position below is room-relative, so it frames either file).
// Part 1, the first minute (G6): a campaign run from level 1's start, the walk pilot (Walk38Pilot over the autopilot: the rooms' scripts, the
// fights), framed at GAME 5 / 15 / 30 / 60 s after the run's first step: L42-first-05 / -15 / -30 / -60. Every teaching line that starts
// (Ui.Teach35TextNow / Ui.Lesson25TextNow changing to a new line) is listed with its game second; the count inside 60 s is the G6 number.
// Part 2, THE GANTRY (G1): DOORFREE, the player put at b2's spawn, the pilot rides the chain: L42-gantry-lip (gate 1's run-up, lz >= 6.5 grounded:
// the bot jumps at 7.1), L42-gantry-mid (riding, 20-30 m past the lip: on W2), L42-gantry-g2 (gate 2's run-up on L1, lz >= 55.5 grounded). Waits at most 40 s of game time.
// Line: GREEN when all seven frames were saved; the lines, the level id, foundry42 printed.
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
        static partial void N42First(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = First42Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator First42Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
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
            double t0 = sim.SimTime;
            var lines = new List<string>();
            int in60 = 0, frames = 0;
            string lastT = "", lastL = "";
            float wallEnd = Time.realtimeSinceStartup + 240;
            Start(g, "first42", "ui", dir);
            FeelProbe.MarkInput();
            void Watch()
            {
                double t = sim.SimTime - t0;
                string tn = Ui.Teach35TextNow ?? "", ln = Ui.Lesson25Visible ? Ui.Lesson25TextNow ?? "" : "";
                if (tn != "" && tn != lastT) { lines.Add($"{t.ToString("0.0", ci)} s teach \"{tn}\""); if (t < 60) in60++; }
                if (ln != "" && ln != lastL) { lines.Add($"{t.ToString("0.0", ci)} s lesson \"{ln}\""); if (t < 60) in60++; }
                lastT = tn;
                lastL = ln;
            }
            foreach (double at in new[] { 5.0, 15, 30, 60 })
            {
                while (sim.SimTime - t0 < at && Time.realtimeSinceStartup < wallEnd) { AutopilotInput.TakeDraft(sim); Watch(); yield return null; }
                yield return Shot30(dir, "L42-first-" + ((int)at).ToString("00", ci));
                frames++;
            }
            yield return Stop();

            // ---- THE GANTRY ----
            RoomDef b2 = lv.rooms.Find(r => r.id == "b2");
            string gantry = "no room b2";
            if (b2 != null)
            {
                DoorFree41(g);
                sim.Respawn(b2.spawn, b2.spawnYawDeg);
                Start(g, "first42-gantry", "camera", dir + "/gantry");
                double g0 = sim.SimTime;
                bool lip = false, mid = false, lip2 = false;
                bool placed = false;
                while (sim.SimTime - g0 < 40 && Time.realtimeSinceStartup < wallEnd && !(lip && mid && lip2))
                {
                    AutopilotInput.TakeDraft(sim);
                    Campaign24 c = sim.campaign;
                    if (!placed && c != null && c.room >= 0 && lv.rooms[c.room].id == "b2") { c.SetDoorFree29(false); placed = true; }
                    double lz = b2.z1 - sim.mover.position.z;
                    bool riding = sim.mover.state == MoveState.WallRide;
                    if (!lip && lz >= 6.5 && sim.mover.grounded) { lip = true; frames++; yield return Shot30(dir, "L42-gantry-lip"); continue; }
                    if (lip && !mid && riding && lz >= 28 && lz <= 38) { mid = true; frames++; yield return Shot30(dir, "L42-gantry-mid"); continue; }
                    if (mid && !lip2 && lz >= 55.5 && sim.mover.grounded) { lip2 = true; frames++; yield return Shot30(dir, "L42-gantry-g2"); continue; }
                    yield return null;
                }
                yield return Stop();
                gantry = $"gantry frames lip {lip} mid {mid} g2 {lip2} in {(sim.SimTime - g0).ToString("0.0", ci)} s, rides {pilot.rides}, pit falls {pilot.pitFalls}";
            }
            bool ok = frames == 7;
            res[0] = $"{(ok ? "GREEN" : "RED")} first42 | level {lv.id} (N41BaseId {sim.N41BaseId(lv.id)}), foundry42 {sim.n42.foundry42}"
                + $" | lines started in the first 60 s: {in60} | frames {frames}/7 | {gantry}"
                + $" || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
