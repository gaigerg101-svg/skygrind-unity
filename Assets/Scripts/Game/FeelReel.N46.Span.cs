// NIGHT46 builder A1 (span46: level 4's arenas rebuilt from their identity cards + flow plans; docs/ROOMS.md "NIGHT46 identity cards"): the
// Player scenario phase `a146` (dispatched by FeelReel.N46.cs). Not run by its author (no Player in a worktree). Boot: reel.sh + -skygrindMap
// n46-span + -skygrindReelOnly a146 (the campaign plays the booted map: L-062), or level 4 with design.n46.span46 ON in the build's copy
// (n39-build-rows.cjs <tag> on n46.span46). On n40-span the line says so (RED: the frames are of the old rooms).
// The doors freed (DoorFree41); for each rebuilt room in the level's order (p1 THE HOOK YARD, p3 THE TIE-OFF, p5 THE SPOOL HALL), the frames the
// card gate and the ON call need (NIGHT46.md method 3 / 4: entrance, mid-fight):
//   A146-<room>-entrance  the entry view (the room's spawn, its yaw, level look) before the first wave (rooms43's commit gate holds it): the
//                         landmark (the GREAT HOOK / the KNOT / the GREAT SPOOL) and wave 1's spots read from the door;
//   A146-<room>-mid       mid-fight, the walk pilot playing: the room's second wave alive (>= 3 bodies): the 30 s problem in play;
//   A146-<room>-late      (p3, p5) the third wave alive: the card's state change (p3: along the cable; p5: the gallery and the spool).
// The still frame holds the look (A1HoldLook45, no move, no press) for 10 render frames first. Each live section waits at most 90 s of game
// time. Line: GREEN when every frame was saved and the level is n46-span; per frame the wave, the alive count and the feet.
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
        static partial void N46Span(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = A1Span46Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator A1Span46Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
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
            bool n46 = lv.id == "n46-span";
            string pc = Preconditions(g, g.Feet);
            float wallEnd = Time.realtimeSinceStartup + 600;
            var notes = new List<string>();
            int frames = 0, want = 0;
            while ((sim.campaign == null || sim.campaign.room < 0 || sim.campaign.phase == Campaign24.Phase.None) && Time.realtimeSinceStartup < wallEnd - 580)
            {
                AutopilotInput.TakeDraft(sim);
                yield return null;
            }
            yield return Frames(20);
            DoorFree41(g);
            Start(g, "a146-span", "camera", dir + "/span");
            string[] rooms = { "p1", "p3", "p5" };
            foreach (string id in rooms)
            {
                RoomDef rd = lv.rooms.Find(r => r.id == id);
                if (rd == null) { notes.Add($"{id}: NOT IN {lv.id}"); continue; }
                bool late = id != "p1";
                want += late ? 3 : 2;
                sim.husks.KillAll();
                g.Respawn(rd.spawn, rd.spawnYawDeg);
                float placeEnd = Time.realtimeSinceStartup + 5;
                while ((sim.campaign == null || sim.campaign.room < 0 || lv.rooms[sim.campaign.room].id != id) && Time.realtimeSinceStartup < placeEnd)
                {
                    AutopilotInput.TakeDraft(sim);
                    yield return null;
                }
                string inRoom = sim.campaign != null && sim.campaign.room >= 0 ? lv.rooms[sim.campaign.room].id : "?";
                if (inRoom != id) { notes.Add($"{id}: NOT PLACED (the campaign's room {inRoom})"); continue; }
                // the still: the entry view from 1.5 m inside the door (p3 / p5's spawn is the first foothold, 6.5-7.5 m in: A1 round 2), before wave 1
                // (rooms43's commit gate holds it until the player is 3 m in); the door's local x: p5's is 10 m east of the room's centre line
                double cx = (Math.Min(rd.x0, rd.x1) + Math.Max(rd.x0, rd.x1)) / 2, zS = Math.Max(rd.z0, rd.z1);
                g.Respawn(new Vec3(cx + (id == "p5" ? 10 : 0), 0, zS - 1.5), rd.spawnYawDeg);
                yield return Frames(2);
                var hold = new A1HoldLook45(rd.spawnYawDeg * Math.PI / 180, -0.02);
                sim.UseInput(hold);
                g.input.SetLook(rd.spawnYawDeg * Math.PI / 180, -0.02);
                // round 3 (the lead's r2 frame: no enemy in the entrance): held at the door, wave 1 comes in on rooms43's commit gate (3 s) or start40's
                // grace (the level's first room); the still waits for its first bodies (at most 8 s of game time), then 0.6 s for the spawn tells
                double tw = sim.SimTime;
                while (sim.enemies.AliveCount == 0 && sim.SimTime - tw < 8 && Time.realtimeSinceStartup < wallEnd) { AutopilotInput.TakeDraft(sim); yield return null; }
                double tt = sim.SimTime;
                while (sim.SimTime - tt < 0.6 && Time.realtimeSinceStartup < wallEnd) { AutopilotInput.TakeDraft(sim); yield return null; }
                yield return Shot30(dir, $"A146-{id}-entrance");
                frames++;
                notes.Add($"A146-{id}-entrance: alive {sim.enemies.AliveCount}, wave {sim.campaign?.wave}, waited {(sim.SimTime - tw).ToString("0.0", ci)} s");
                sim.UseInput(pilot);
                // the live frames: the pilot plays; each saved once when its test first holds; a room that clears (or a campaign that moves on) ends it
                foreach (var (name, minWave) in late ? new[] { ("mid", 2), ("late", 3) } : new[] { ("mid", 2) })
                {
                    double t0 = sim.SimTime;
                    bool saved = false, left = false;
                    while (sim.SimTime - t0 < 90 && Time.realtimeSinceStartup < wallEnd)
                    {
                        AutopilotInput.TakeDraft(sim);
                        bool here = sim.campaign != null && sim.campaign.room >= 0 && lv.rooms[sim.campaign.room].id == id && sim.campaign.phase == Campaign24.Phase.Fight;
                        if (!here) { left = true; break; }
                        if (sim.campaign.wave >= minWave && sim.enemies.AliveCount >= (minWave == 2 ? 3 : 1))
                        {
                            yield return Shot30(dir, $"A146-{id}-{name}");
                            frames++;
                            saved = true;
                            notes.Add($"A146-{id}-{name}: alive {sim.enemies.AliveCount}, wave {sim.campaign.wave}, t {(sim.SimTime - t0).ToString("0.0", ci)} s, feet ({g.Feet.x.ToString("0.0", ci)}, {g.Feet.y.ToString("0.0", ci)}, {g.Feet.z.ToString("0.0", ci)})");
                            break;
                        }
                        yield return null;
                    }
                    if (!saved) notes.Add($"A146-{id}-{name}: NOT SAVED ({(left ? "the room cleared or the campaign moved on" : "90 s")}; wave {sim.campaign?.wave}, alive {sim.enemies.AliveCount}, phase {sim.campaign?.phase})");
                    if (left) break;
                }
                // round 3: the pilot held still between rooms (r2: it played on through p4 into o1 during p3's wait, so p5 could not be placed)
                sim.UseInput(new A1HoldLook45(rd.spawnYawDeg * Math.PI / 180, 0));
                sim.husks.KillAll();
                yield return Frames(2);
            }
            yield return Stop();
            bool ok = frames == want && want == 8 && n46;
            res[0] = $"{(ok ? "GREEN" : "RED")} a146 | level {lv.id}{(n46 ? "" : " (NOT n46-span: boot -skygrindMap n46-span or turn design.n46.span46 on in this build's copy)")} (BaseId {N45Levels.BaseId(g.cfg, lv.id)}), "
                + $"rooms43 {sim.n43.rooms43} | frames {frames}/{want} | {string.Join(" | ", notes)} | {pc}";
        }
    }
}
