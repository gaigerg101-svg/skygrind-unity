// NIGHT32 builder A (design.n32.arenas32): the Player scenarios of the round ring and the new arenas (the lead's FeelReel.N27.cs dispatches PhaseN32A to N32ArenasBody).
//   ring32    THE LANTERN RING as a circle (GG 2026-10-10: "it needs to be a circle"). God mode. Two frames from the ring's MIDDLE at eye height (looking east and looking north:
//             the lead looks for a ROUND wall, a disc floor and arc platforms, no visible flat panels), then LanternPilot30 rides the wall for up to 25 s and two frames are taken
//             DURING a ride (0.4 s and 1.4 s into the stay on the wall: the curve ahead must read round). The line prints the facets, the ring frames drawn, the wall-ride frames,
//             the longest stay on the wall, and how close the rider kept to the wall (|WallDistance - 0.35| over ride frames). RED when the run's ring is not round.
//   arenas32  the new and rebuilt rooms of the BOOTED level at eye height (boot -skygrindMap n32-foundry | n32-kiln | n32-iceline, one set per map): f9b + f10 |
//             k2 + k2b | i6 + i6b, each framed from its entry facing in and from its exit facing back (Arenas32Scenario).
// Boss HP is only READ. Shots go to <dir>/shot-N32-*.png (Shot29's sampled numbers). Every result line starts with PASS or RED. Not run by its author (no Player in the worktree).
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
        static partial void N32ArenasBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "ring32") body = Ring32Scenario(g, dir, res, offs, pre);
            else body = Arenas32Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Shot32(string dir, string name, double[] st, List<string> lines, CultureInfo ci)
        {
            yield return Shot29(dir, "N32-" + name, st);
            lines.Add(ShotLine29(name, st, ci));
        }

        static IEnumerator Ring32Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.Lantern27On) { res[0] = "RED: design.n27.boss1 is off (no lantern) | " + pc; yield break; }
            if (!sim.Lantern30On) { res[0] = "RED: the ring is off in this run (n30.lantern30 off): NIGHT29's courtyard | " + pc; yield break; }
            if (!sim.n32.arenas32) { res[0] = "RED: design.n32.arenas32 is off in this run (NIGHT31's 16 flat panels) | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            g.husks.frozen = true;
            if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yawDeg) || sim.lantern27 == null || !sim.lantern27.Alive) { res[0] = "RED: BossRoomBegin(hulk) did not start the lantern | " + pc; yield break; }
            Lantern27 l = sim.lantern27;
            g.Respawn(spawn, yawDeg);
            pc = Preconditions(g, spawn);
            yield return Frames(20);
            Lantern30Arena a = sim.lantern30Arena;
            if (a == null || !a.Round) { res[0] = $"RED: the run's ring is not round (lantern30Arena {(a == null ? "null" : $"{a.Facets} facets, facetsPerPanel {a.p.facetsPerPanel}")}): the lead's n32-ring-wire.cjs or the config | " + pc; yield break; }
            Start(g, "ring32", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 120;
            var shots = new List<string>();
            var st = new double[4];
            Vec3 o = l.OriginWorld;
            Boss30View v = Boss30View.Current;
            // ---------------- the middle: two frames at eye height, the lantern behind the camera's side ----------------
            int ringFrames = 0, frames = 0;
            Vec3 mid = new Vec3(o.x - 3.0, o.y, o.z - 3.0);
            double[] looks = { 0.0, Math.PI / 2 }; // east (+x), north (+z on the arena's atan2 circle)
            string[] names = { "ring-middle-east", "ring-middle-north" };
            for (int k = 0; k < looks.Length; k++)
            {
                Vec3 d = new Vec3(Math.Cos(looks[k]), 0, Math.Sin(looks[k]));
                double wy = JsMath.Atan2(-d.x, -d.z), wp = 0.12;
                g.Respawn(mid, wy * 180 / Math.PI);
                for (int i = 0; i < Ms(600); i++)
                {
                    God(g);
                    frames++;
                    if (v != null && v.RingShown) ringFrames++;
                    g.input.SetLook(wy, wp);
                    g.forcedMove = new InputFrame { yaw = wy, pitch = wp };
                    g.forcedCombat = new CombatInput { weapon = -1 };
                    yield return null;
                }
                yield return Shot32(dir, names[k], st, shots, ci);
            }
            // ---------------- a wall ride: LanternPilot30 on the round wall ----------------
            g.Respawn(spawn, yawDeg);
            var pilot = new LanternPilot30 { hand = new Mulberry32(Seed * 7919u + 29u) };
            var drive = new PilotDrive27 { tick = sim.simStep };
            double yawL = yawDeg * Math.PI / 180;
            int step0 = sim.simStep, rideFrames = 0, stay = 0, bestStay = 0, rideShots = 0;
            double hugSum = 0, hugMax = 0;
            while (l.Alive && (sim.simStep - step0) * sim.step < 25.0 && rideShots < 2 && Time.realtimeSinceStartup < wallEnd)
            {
                God(g);
                frames++;
                if (v != null && v.RingShown) ringFrames++;
                drive.Advance(sim.simStep, t =>
                {
                    pilot.Step(sim, yawL, t);
                    yawL = pilot.Move.yaw;
                    return (pilot.Move, pilot.Combat);
                });
                g.input.SetLook(drive.mv.yaw, 0.05); // look along the wall, not at the lantern: the curve ahead is what the frame must show
                g.forcedMove = drive.mv;
                g.forcedCombat = new CombatInput { weapon = -1 };
                bool riding = sim.mover.state == MoveState.WallRide;
                if (riding)
                {
                    rideFrames++;
                    stay++;
                    bestStay = Math.Max(bestStay, stay);
                    Vec3 me = sim.mover.position - o;
                    double hug = Math.Abs(a.WallDistance(me.x, me.z) - 0.35);
                    hugSum += hug;
                    hugMax = Math.Max(hugMax, hug);
                    if ((rideShots == 0 && stay == Ms(400)) || (rideShots == 1 && stay == Ms(1400))) { rideShots++; yield return Shot32(dir, "ring-wallride-" + rideShots, st, shots, ci); }
                }
                else stay = 0;
                yield return null;
            }
            Quiet(g);
            yield return Frames(Ms(500));
            yield return Stop();
            var fails = new List<string>();
            if (ringFrames * 2 < frames) fails.Add($"the ring was drawn in {ringFrames} of {frames} frames");
            if (shots.Count < 2) fails.Add("the middle frames were not taken");
            if (rideShots < 2) fails.Add($"only {rideShots} of 2 wall-ride frames (longest stay on the wall {(bestStay / (double)UnityTuning.ReelFps).ToString("0.00", ci)} s)");
            if (rideFrames > 0 && hugMax > 0.3) fails.Add($"the rider left the wall by up to {hugMax.ToString("0.00", ci)} m while riding");
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, the ring {a.Facets} facets ({a.p.panels} panels x {a.p.facetsPerPanel}), god mode: "
                + $"ring drawn {ringFrames}/{frames}; wall-ride frames {rideFrames}, longest stay {(bestStay / (double)UnityTuning.ReelFps).ToString("0.00", ci)} s, off the wall mean {(rideFrames > 0 ? hugSum / rideFrames : 0).ToString("0.000", ci)} m / max {hugMax.ToString("0.000", ci)} m; "
                + $"LOOK: a round wall (no flat panels), a disc floor, arc platforms; shots: {string.Join("; ", shots)}";
        }

        /// <summary>The new and rebuilt rooms per n32 level id (builder A, DESIGN32 A): the rooms arenas32 frames.</summary>
        static readonly Dictionary<string, string[]> Arenas32Rooms = new Dictionary<string, string[]>
        {
            { "n32-foundry", new[] { "f9b", "f10" } },
            { "n32-kiln", new[] { "k2", "k2b" } },
            { "n32-iceline", new[] { "i6", "i6b" } },
        };

        /// <summary>arenas32: boot the Player with -skygrindMap n32-foundry | n32-kiln | n32-iceline (a CAMPAIGN level; one set per map). The reel starts a campaign
        /// run, sets DOORFREE (a room is entered from any room), god mode, and for each new / rebuilt room of the booted level: respawns at the room's ENTRY facing in
        /// (frame -a at 0.6 s, eye height) and at its EXIT facing back south over the room (frame -b at 0.6 s). One line per room: the lesson on screen, enemies alive,
        /// the room's size. PASS when every room of the level's list was entered and framed twice. LOOK: two or more heights, the lava edge, the ramps (the climb
        /// route), at most 24 m across.</summary>
        static IEnumerator Arenas32Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why + " (boot -skygrindMap n32-foundry | n32-kiln | n32-iceline)"; yield break; }
            SurvivalSim sim = g.sim;
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(30);
            God(g);
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN (boot -skygrindMap n32-foundry | n32-kiln | n32-iceline)"; yield break; }
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            sim.campaign?.SetDoorFree29(true);
            LevelDef lv = sim.campaignLevel;
            if (!Arenas32Rooms.TryGetValue(lv.id, out string[] ids)) { res[0] = $"RED: level '{lv.id}' is not an arenas32 level (boot -skygrindMap n32-foundry | n32-kiln | n32-iceline)"; yield break; }
            var want = new List<RoomDef>();
            foreach (string id in ids) { RoomDef r = lv.rooms.Find(q => q.id == id); if (r != null) want.Add(r); }
            string pc = Preconditions(g, g.Feet) + $" | level {lv.id}, n32.arenas32 {sim.n32.arenas32}";
            Start(g, "arenas32", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();
            var shots = new List<string>();
            var st = new double[4];
            int entered = 0;
            string ended = null;
            foreach (RoomDef r in want)
            {
                if (ended != null) break;
                // -a: the entry, facing in
                g.Respawn(r.spawn, r.spawnYawDeg);
                yield return Settle31(g, 0.6, () => (ended = Ended30(g)) != null);
                Vec3 ft = g.Feet;
                bool inRoom = ft.x >= r.x0 && ft.x <= r.x1 && ft.z >= r.z0 && ft.z <= r.z1;
                if (inRoom) entered++;
                string lesson = Ui.Lesson25Visible && !string.IsNullOrEmpty(Ui.Lesson25TextNow) ? Ui.Lesson25TextNow : "(none)";
                if (ended == null) yield return Shot32(dir, $"arenas32-{r.id}-a", st, shots, ci);
                // -b: the exit, facing back over the room (south)
                g.Respawn(new Vec3(r.exit.x, r.exit.y, r.exit.z), r.spawnYawDeg + 180);
                g.input.SetLook((r.spawnYawDeg + 180) * Math.PI / 180, -0.08);
                yield return Settle31(g, 0.6, () => (ended = Ended30(g)) != null);
                if (ended == null) yield return Shot32(dir, $"arenas32-{r.id}-b", st, shots, ci);
                lines.Add($"{r.id} '{r.name}' entered {inRoom}: {(r.x1 - r.x0).ToString("0", ci)} x {(r.z1 - r.z0).ToString("0", ci)} m, lesson '{lesson}', enemies alive {Alive30(sim)}, health {sim.health.hp.ToString("0", ci)}{(ended != null ? " | then " + ended : "")}");
            }
            yield return Stop();
            bool ok = want.Count == ids.Length && entered == want.Count && shots.Count >= 2 * want.Count && ended == null;
            res[0] = $"{(ok ? "PASS" : "RED")}: {entered} of {ids.Length} arenas32 rooms entered by respawn, {shots.Count} frames (shot-N32-arenas32-<room>-a/b.png){(ended != null ? " | ended: " + ended : "")} | {pc} | LOOK: two or more heights, the lava edge, the ramps, at most 24 m || "
                + string.Join(" || ", lines) + " || shots: " + string.Join("; ", shots);
        }
    }
}
