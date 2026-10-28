// NIGHT45 builder A1 (foundry45: b1 THE YARD rebuilt from its identity card as THE SPILL; docs/ROOMS.md "NIGHT45 identity cards"): the Player
// scenario phase `a145` (dispatched by FeelReel.N45.cs). Not run by its author (no Player in a worktree). Boot: level 1 in the campaign (reel.sh +
// -skygrindReelOnly, L-062) with design.n45.foundry45 ON in the build's copy (n39-build-rows.cjs <tag> on n45.foundry45), so the campaign's level 1
// is n45-foundry; with the row off it is n43-foundry and the line says so (RED: the frames are of the old b1).
// The doors freed (DoorFree41), the player put at b1's spawn; the frames the card's A-9 check needs (NIGHT45.md method 4):
//   A145-yard-entrance  the entry view (b1's spawn, its yaw, level look) before the first wave: the landmark (the ladle) reads from the door;
//   A145-yard-landmark  from the near bank by the west rail (frames36.mid): the ladle, its lip and lamp, the west bridge and rail, the slag;
//   A145-yard-mid       mid-fight, the walk pilot playing (Walk38Pilot, sampled by the sim every step): the second wave alive (the rim's spitters and
//                       the husks on the bridges: the 30 s problem in play);
//   A145-yard-wave3     the third wave alive with a body on the near bank (the state change: the wave from the side you came in).
// The two still frames hold the look with A1HoldLook45 (no move, no press) for 10 render frames first. Each live section waits at most 90 s of
// game time. Line: GREEN when all four frames were saved; the level id, foundry45, rooms43, the waves, the alive count at each frame.
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
        /// <summary>A1's still-frame input: a fixed look, no move, no press (combat neutral).</summary>
        sealed class A1HoldLook45 : IPlayerInput
        {
            double yaw, pitch;
            public A1HoldLook45(double yaw, double pitch) { this.yaw = yaw; this.pitch = pitch; }
            public double Yaw => yaw;
            public double Pitch => pitch;
            public void SetLook(double y, double p) { }
            public bool Captured => true;
            public InputFrame SampleMove(bool active) => new InputFrame { yaw = yaw, pitch = pitch };
            public CombatInput SampleCombat(bool active) => new CombatInput { weapon = -1 };
        }

        static partial void N45Yard(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = A1Yard45Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator A1Yard45Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
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
            bool n45 = NightRows.BaseId(g.cfg, lv.id) == "n45-foundry"; // NIGHT46: n46-foundry runs as its n45 rooms
            string pc = Preconditions(g, g.Feet);
            float wallEnd = Time.realtimeSinceStartup + 360;
            var notes = new List<string>();
            int frames = 0;
            while ((sim.campaign == null || sim.campaign.room < 0 || sim.campaign.phase == Campaign24.Phase.None) && Time.realtimeSinceStartup < wallEnd - 340)
            {
                AutopilotInput.TakeDraft(sim);
                yield return null;
            }
            yield return Frames(20);
            DoorFree41(g);
            RoomDef rd = lv.rooms.Find(r => r.id == "b1");
            if (rd == null) { res[0] = $"RED a145 | level {lv.id}: no room b1 | {pc}"; yield break; }
            g.Respawn(rd.spawn, rd.spawnYawDeg);
            float placeEnd = Time.realtimeSinceStartup + 5;
            while ((sim.campaign == null || sim.campaign.room < 0 || lv.rooms[sim.campaign.room].id != "b1") && Time.realtimeSinceStartup < placeEnd)
            {
                AutopilotInput.TakeDraft(sim);
                yield return null;
            }
            string inRoom = sim.campaign != null && sim.campaign.room >= 0 ? lv.rooms[sim.campaign.room].id : "?";
            if (inRoom != "b1") { res[0] = $"RED a145 | level {lv.id}: NOT PLACED in b1 (the campaign's room {inRoom}) | {pc}"; yield break; }
            Start(g, "a145-yard", "camera", dir + "/b1");

            // the two still frames: the entry view, then the landmark from the near bank by the west rail (rooms43's commit gate holds wave 1 until
            // the player is 3 m in, so the entry frame is before any enemy)
            IEnumerator Still(string name, Vec3 at, double yawDeg, double pitch)
            {
                if (name != "A145-yard-entrance") g.Respawn(at, yawDeg);
                var hold = new A1HoldLook45(yawDeg * Math.PI / 180, pitch);
                sim.UseInput(hold);
                g.input.SetLook(yawDeg * Math.PI / 180, pitch);
                yield return Frames(10);
                yield return Shot30(dir, name);
                frames++;
                notes.Add($"{name}: alive {sim.enemies.AliveCount}, wave {sim.campaign?.wave}, feet ({g.Feet.x.ToString("0.0", ci)}, {g.Feet.y.ToString("0.0", ci)}, {g.Feet.z.ToString("0.0", ci)})");
                sim.UseInput(pilot);
            }
            yield return Still("A145-yard-entrance", rd.spawn, rd.spawnYawDeg, -0.02);
            double mx = -11.5, mz = -150; // b1's frames36.mid in gen-n45-foundry.cjs (the west bridge's near end, 4 m short of the slag)
            double myaw = -Math.Atan2(0 - mx, -(-163 - mz)) * 180 / Math.PI;
            yield return Still("A145-yard-landmark", new Vec3(mx, 0, mz), myaw, 0.08);

            // the live frames: the pilot plays; save each once when its test first holds
            IEnumerator Live(string name, Func<bool> test)
            {
                double t0 = sim.SimTime;
                while (sim.SimTime - t0 < 90 && Time.realtimeSinceStartup < wallEnd)
                {
                    AutopilotInput.TakeDraft(sim);
                    if (sim.campaign != null && sim.campaign.room >= 0 && lv.rooms[sim.campaign.room].id == "b1" && test())
                    {
                        yield return Shot30(dir, name);
                        frames++;
                        notes.Add($"{name}: alive {sim.enemies.AliveCount}, wave {sim.campaign.wave}, t {(sim.SimTime - t0).ToString("0.0", ci)} s, feet ({g.Feet.x.ToString("0.0", ci)}, {g.Feet.y.ToString("0.0", ci)}, {g.Feet.z.ToString("0.0", ci)})");
                        yield break;
                    }
                    yield return null;
                }
                notes.Add($"{name}: NOT SAVED in 90 s (wave {sim.campaign?.wave}, alive {sim.enemies.AliveCount})");
            }
            bool NearBody()
            {
                foreach (Enemy e in sim.enemies.all) if (e.alive && e.position.z > -154) return true;
                return false;
            }
            g.Respawn(rd.spawn, rd.spawnYawDeg);
            yield return Live("A145-yard-mid", () => sim.campaign.wave >= 2 && sim.enemies.AliveCount >= 3);
            yield return Live("A145-yard-wave3", () => sim.campaign.wave >= 3 && sim.enemies.AliveCount >= 2 && NearBody());
            yield return Stop();
            bool ok = frames == 4 && n45;
            res[0] = $"{(ok ? "GREEN" : "RED")} a145 | level {lv.id}{(n45 ? "" : " (NOT n45-foundry: design.n45.foundry45 off in this build's copy?)")} (BaseId {N45Levels.BaseId(g.cfg, lv.id)}), "
                + $"foundry45 {g.cfg.Design("n45.foundry45.enabled")}, rooms43 {sim.n43.rooms43} | frames {frames}/4 | {string.Join(" | ", notes)} | {pc}";
        }
    }
}
