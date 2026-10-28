// NIGHT37 builder W: the Player scenario of wick37 (FeelReel.N27.cs PhaseN37W; the body is the partial N37WickBody). Not run by its author (no
// Player in the worktree). Input through the game's forced move / combat frames only (L-046); the player kept alive (God).
//   wick37  THE STACK: the level's own (n37-foundry with the wick37 row: boot -skygrindMap n37-foundry) or, on any other map, the stack put at
//     (2600, 0, 0) (SurvivalSim.Wick37Place). Two parts.
//     A. THE SPACE (no fight; the slag still under the floor). Frames (shot-N29-<name>.png):
//        W37-space-mouth    from the tunnel's mouth, looking up the spiral: the first panel ahead-left on the floor, the next ones climbing
//                           counter-clockwise, every panel's ride face trimmed cyan, the shaft's iron dark with red-orange hot seams
//        W37-space-up       from the centre of the floor straight up the 40 m well: the spiral of panels, the two ledge rings (cyan lips), the cap
//        W37-space-ledge1   standing on ledge 1 (7.8 m), looking across and up at the next panels (cyan faces) over the drop
//        W37-space-ledge2   standing on ledge 2 (21.1 m), looking down the well: the floor far below, the slag's glow under it
//        W37-space-cap      on the cap's catwalk, looking along it: the ring wall's cyan band (ride the lap), the shaft's hole on the left
//     B. THE FIGHT: the player at the mouth; Wick37Begin runs as the campaign's walk-in would (no Respawn: his feet are checked); then
//        Wick37Pilot climbs (NIGHT44 R2: one pilot step per SIM step through Wick44ProofInput as sim.input). Frames:
//        W37-fight-reveal   0.8 s in: THE WICK rising out of the slag on the axis, its core warm
//        W37-fight-tell-<id> each attack's first tell at 70 % of its wind-up: ember (core YELLOW), flare (core RED + the red half of the wall),
//                           slam (the red decal on the ledge), dive (core RED on the cap)
//        W37-fight-parry    3 frames after the first parry: the ember white, flying back into the cage
//        W37-fight-crack    3 frames after the first crack: a plate tilted open, its seam CYAN
//        W37-fight-window   0.5 s into the first CYAN window: the core CYAN
//        W37-fight-leg2 / -leg3  1 s after each leg change (ledge 2; the cap)
//        W37-fight-stall    0.3 s into the first dive stall: THE WICK stuck at the cap's wall, the core CYAN
//        W37-fight-kill     0.5 s after the kill: it falls into the shaft
//     Line: GREEN when the begin kept his feet (< 0.05 m), the pilot reached ledge 1, a tell of ember and flare was seen with lengths 1.0-1.5 s,
//     and the view drew the stack (panels trimmed > 0); RED otherwise, with the measures.
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
        static partial void N37WickBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "wick37") body = Wick37Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator W37Shot(SurvivalGame g, Vec3 feet, Vec3 lookAt, string dir, string name, double[] st)
        {
            Vec3 eye = new Vec3(feet.x, feet.y + 1.6, feet.z), dv = lookAt - eye;
            double yaw = Math.Atan2(-dv.x, -dv.z), pitch = Math.Atan2(dv.y, Math.Sqrt(dv.x * dv.x + dv.z * dv.z));
            g.Respawn(feet, yaw * 180 / Math.PI);
            g.input.SetLook(yaw, pitch);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
            yield return Frames(20);
            yield return Shot29(dir, name, st);
        }

        static IEnumerator Wick37Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            // a CAMPAIGN run when the map is a campaign level (as O's loom37 and C's crown37 reels), DOORFREE
            bool campaign = N24CampaignGate(g) == null;
            if (campaign)
            {
                offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
                Quiet(g);
                g.noRecords = g.noLab = true;
                g.setup.mode = Campaign24.Mode;
                g.StartRun(Seed);
                yield return Frames(240); // the run-start title has run out before any frame
                sim = g.sim;
                for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
                DoorFree41(g); // NIGHT41 T2 (A9): DOORFREE + its queued room banners cleared (FeelReel.N41.cs)
                campaign = sim.campaignOn && sim.campaign != null;
            }
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            bool levelStack = sim.wick37Centre.HasValue;
            if (!levelStack) sim.Wick37Place(new Vec3(2600, 0, 0));
            Vec3 c = sim.wick37Centre.Value;
            Wick37Stack s = sim.wick37Stack;
            if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
            string pc = $"{(campaign ? "CAMPAIGN" : "survival")} run, map {g.MapId}, stack {(levelStack ? "the level's" : "placed at 2600,0,0")}, n37.wick37 {sim.n37.wick37}, view {(Wick37View.Current != null ? "on" : "MISSING (the lead's two lines in SurvivalGame.Boss26.cs)")}";
            Start(g, "wick37", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            // A. the space
            double rL = s.p.radius - s.p.ledgeWidth * 0.5;
            yield return W37Shot(g, c + new Vec3(0, 0.05, s.p.radius - 0.5), c + new Vec3(-s.p.panelApothem, 6, 0), dir, "W37-space-mouth", st);
            yield return W37Shot(g, c + new Vec3(0.3, 0.05, 0.3), c + new Vec3(0, s.p.capY, 0.01), dir, "W37-space-up", st);
            Vec3 OnArc(int i, double y) => new Vec3(Math.Cos(s.ledgeAngle[i]) * rL, y, Math.Sin(s.ledgeAngle[i]) * rL);
            // ledge 1: on its arc, looking up across the shaft at the spiral's next panels
            yield return W37Shot(g, c + OnArc(0, s.p.ledges[0] + 0.05), c + new Vec3(0, s.p.ledges[0] + 5, 0) - OnArc(0, 0), dir, "W37-space-ledge1", st);
            // ledge 2: on its arc, looking down the well
            if (s.p.ledges.Length > 2) yield return W37Shot(g, c + OnArc(1, s.p.ledges[1] + 0.05), c + new Vec3(0, 0, 0) - OnArc(1, 0) * 0.3, dir, "W37-space-ledge2", st);
            double rc = (s.p.radius + s.p.capRadius) / 2;
            yield return W37Shot(g, c + new Vec3(rc, s.p.capY + 0.05, 0), c + new Vec3(0, s.p.capY + 1.2, -rc), dir, "W37-space-cap", st);
            int trimmed = Wick37View.Current?.PanelsTrimmed ?? -1;
            string space = $"space {(trimmed > 0 ? "GREEN" : "RED")} (5 frames W37-space-*, panels trimmed {trimmed} of {s.panels.Count}, slag drawn {Wick37View.Current?.SlagShown})";
            res[0] = space + " | fight: not reached (the scenario stopped after part A) | " + pc;
            // B. the fight
            Vec3 mouth = c + new Vec3(0, 0.05, s.p.radius + 1.0);
            g.Respawn(mouth, 0);
            g.input.SetLook(0, 0);
            g.forcedMove = new InputFrame { yaw = 0, pitch = 0 };
            yield return Frames(20);
            Vec3 before = sim.mover.position;
            sim.Wick37Reset(); // a campaign run may have begun it when the space frames stood in the stack: a fresh fight
            if (!sim.Wick37Begin(out Vec3 spawn, out double yawDeg))
            {
                yield return Stop();
                res[0] = space + " | fight: RED (Wick37Begin returned false: the wick37 row off?) | " + pc;
                yield break;
            }
            yield return null;
            double moved = Vec3.DistanceTo(before, sim.mover.position);
            Wick37 w = sim.wick37;
            var pilot = new Wick37Pilot { hand = new Mulberry32(1u * 7919u + 13u) }; // the harness's seed-1 hand (n44proof wick, n43wick band)
            // NIGHT44 R2 (T-59; SPIKE.md 1): the pilot as the sim's input, one pilot step per SIM step, as the harness runs it (n44proof wick: per
            // step kills 3/3 at ~50 s, the old per-frame OR 0/3 with rides 0). The reader's forced frames off (the adapter consumes them unused).
            IPlayerInput prevInput = sim.input;
            var pilotIn = new Wick44ProofInput(sim, pilot, prevInput);
            g.forcedMove = null;
            g.forcedCombat = null;
            sim.UseInput(pilotIn);
            offs.Add(() => { if (sim.input == pilotIn) sim.UseInput(prevInput); });
            double t0 = sim.SimTime, ledge1 = -1;
            var told = new HashSet<string>();
            bool reveal = false, parryShot = false, crackShot = false, windowShot = false, stallShot = false, killShot = false;
            int parryF = -1, crackF = -1, frame = 0, leg2F = -1, leg3F = -1, lastLeg = 1;
            bool leg2Shot = false, leg3Shot = false;
            double windowAt = -1, stallAt = -1, killAt = -1;
            double wallEnd = Time.realtimeSinceStartup + 150;
            while (Time.realtimeSinceStartup < wallEnd && sim.SimTime - t0 < 75)
            {
                God(g);
                frame++;
                double now = sim.SimTime - t0;
                // NIGHT44 R2 (T-59): the pilot steps inside every sim step (Wick44ProofInput as sim.input), never per frame; after the kill it looks
                // down into the shaft (the adapter's neutral frame)
                Vec3 l = sim.mover.position - c;
                if (ledge1 < 0 && sim.mover.grounded && Math.Abs(l.y - s.p.ledges[0]) < 0.3) ledge1 = now;
                if (!reveal && now >= 0.8) { reveal = true; yield return Shot29(dir, "W37-fight-reveal", st); continue; }
                Boss37Brain b = w.brain;
                if (b != null && w.Alive)
                {
                    if (b.state == Boss37State.Tell && b.Current != null && b.Progress >= 0.7 && told.Add(b.Current.id))
                    {
                        yield return Shot29(dir, "W37-fight-tell-" + b.Current.id.Replace("wick", "").ToLowerInvariant(), st);
                        continue;
                    }
                    if (w.parries > 0 && parryF < 0) parryF = frame;
                    if (!parryShot && parryF > 0 && frame - parryF >= 3) { parryShot = true; yield return Shot29(dir, "W37-fight-parry", st); continue; }
                    if (w.cracks > 0 && crackF < 0) crackF = frame;
                    if (!crackShot && crackF > 0 && frame - crackF >= 3) { crackShot = true; yield return Shot29(dir, "W37-fight-crack", st); continue; }
                    if (b.Open && windowAt < 0) windowAt = now;
                    if (!windowShot && windowAt >= 0 && now - windowAt >= 0.5) { windowShot = true; yield return Shot29(dir, "W37-fight-window", st); continue; }
                    if (b.leg != lastLeg) { if (b.leg == 2) leg2F = frame; if (b.leg == 3) leg3F = frame; lastLeg = b.leg; }
                    if (!leg2Shot && leg2F > 0 && frame - leg2F >= 60) { leg2Shot = true; yield return Shot29(dir, "W37-fight-leg2", st); continue; }
                    if (!leg3Shot && leg3F > 0 && frame - leg3F >= 60) { leg3Shot = true; yield return Shot29(dir, "W37-fight-leg3", st); continue; }
                    if (w.stalled && stallAt < 0) stallAt = now;
                    if (!stallShot && stallAt >= 0 && now - stallAt >= 0.3) { stallShot = true; yield return Shot29(dir, "W37-fight-stall", st); continue; }
                }
                if (!w.Alive && killAt < 0) killAt = now;
                if (!killShot && killAt >= 0 && now - killAt >= 0.5) { killShot = true; yield return Shot29(dir, "W37-fight-kill", st); break; }
                yield return null;
            }
            if (sim.input == pilotIn) sim.UseInput(prevInput);
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            var sb = new System.Text.StringBuilder();
            bool tellsOk = told.Contains(Wick37.Ember) && told.Contains(Wick37.Flare);
            var seen = new HashSet<string>();
            foreach (var (a, steps) in w.brain.tellLog)
            {
                if (!seen.Add(a)) continue;
                double len = steps * sim.step;
                bool inBand = len >= 0.99 && len <= 1.51;
                tellsOk &= inBand;
                sb.Append($" {a} {len.ToString("0.00", ci)} s{(inBand ? "" : " RED")};");
            }
            bool ok = moved < 0.05 && ledge1 >= 0 && tellsOk && trimmed > 0;
            res[0] = space + $" | fight: {(ok ? "GREEN" : "RED")}: begin moved the feet {moved.ToString("0.000", ci)} m; pilot ledge 1 at {(ledge1 < 0 ? "never" : ledge1.ToString("0.0", ci) + " s")}, panels ridden [{string.Join(" ", pilot.ridden)}], leg {w.brain.leg}, {(w.Alive ? "alive" : "killed at " + killAt.ToString("0.0", ci) + " s")}; tells seen {told.Count}:{sb} parries {w.parries} cracks {w.cracks} pierces {w.pierces} windows {w.brain.windows} launches {w.launches}; hot burns {sim.wick37HotBurns} slag touches {sim.wick37SlagTouches}; panels trimmed {trimmed}, core colour at the end {Wick37View.Current?.CoreColour} | {pc}";
        }
    }
}
