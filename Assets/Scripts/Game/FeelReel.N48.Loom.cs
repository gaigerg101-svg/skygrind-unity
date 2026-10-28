// NIGHT48 builder L: reel phase a448 (FeelReel.N48.cs routes it here): THE LOOM's look and motion frames (G36, G37) and kneeHold47's frames
// (R4). Not run by its author (no Player in a worktree). Boot: -skygrindMap n47-span -skygrindReelOnly a448 (THE LOOM FRAME). The rows:
// loom47 and kneeHold47 forced on for the run (sim.rowsOverride, before the run starts); the three LOOKS are switched per frame by the reel's
// own switches (Loom37View.Force47 / Force48: -1 the config, 0 off, 1 on), so one run takes every set with the same moments:
//   n46 = loomLook47 off (NIGHT46's iron box), n47 = loomLook47 on, loomHull48 off (NIGHT47's hull), n48 = both on (tonight's shuttle).
// The player kept alive (God); between shots the player is held where the camera must be (a Respawn each frame of a strip: a fixed camera).
// Input through the game's forced move / combat frames only (L-046). Frames (shot-N29-<name>.png):
//   L48-<v>-door        from the start ledge, looking at the machine (the first view)                                     v = n46 / n47 / n48
//   L48-<v>-shuttle-10 / -20   the machine held still (Frozen), the camera 10 / 20 m south-east of and 3.5 m under its centre (a447's place)
//   L48-<v>-long-30 / -above-15 / -end-20  round 3: due south 30 m nearly level (the broadside), 15 m south-east 6 m above (the top), 20 m due
//                       east (end-on, the point at the camera)
//   L48-<v>-rear        an attack's tell held at 45 % (Frozen after it: the anticipation's pose settled), from 20 m (the blind pair's moment)
//   L48-step-<v>-00..11 the 12-frame LEG-STEP strip (G37): the east legs from ~18 m, the machine Frozen; one frame every 2 render frames
//                       (1/30 s: 12 frames = 0.37 s, one step is Look47StepS 0.35 s) from a step's lift on leg 1 / 3 / 5 (n48: leg 1 forced)
//   L48-antic-<v>-00..11 the ANTICIPATION strip: a forced FLOOR SWEEP (2.0 s tell) from its tell's start to 0.4 s after the strike, 12
//                       frames evenly spaced, from 20 m
//   L48-flinch-<v>-00..07 the FLINCH strip: one shot at the body from ~12 m; frames at +0, 2, 4, 6, 8, 12, 18, 30 render frames after the
//                       first bossHit (0-0.5 s)
//   L48-hold-a / -b / -c  kneeHold47 ON: the first BEND with a knee latched, +0.05 s, +0.45 s (both inside holdS 0.5) and +0.9 s (after)
//   L48-seam            the first FULL bend with a knee latched, 0.3 s in (a447's L47-seam, here with kneeHold47 on)
// Line: GREEN when every set was taken, the n48 frames drew the hull (Hull48Shown), a step lifted and planted inside the strip, the rear reached
// >= 8 deg, a flinch peaked >= 3 deg within 100 ms of its hit, and the held knee moved < 0.05 m over -a .. -b; RED otherwise. The line
// carries each number.
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
        static partial void N48Loom(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = L48LoomScenario(g, dir, res, offs, pre);
        }

        static readonly string[] L48Looks = { "n46", "n47", "n48" };

        static void L48Look(string v)
        {
            Loom37View.Force47 = v == "n46" ? 0 : 1;
            Loom37View.Force48 = v == "n48" ? 1 : 0;
        }

        /// <summary>The player's feet so that the eye looks at `at` from `dist` m on the bearing (dx, dz) (sim axes) and `drop` m under it.</summary>
        static Vec3 L48Feet(Vec3 at, double dist, double dx, double dz, double drop)
        {
            double n = Math.Sqrt(dx * dx + dz * dz);
            return new Vec3(at.x + dist * dx / n, at.y - drop - 1.6, at.z + dist * dz / n);
        }

        static void L48Hold(SurvivalGame g, Vec3 feet, Vec3 at)
        {
            Vec3 eye = new Vec3(feet.x, feet.y + 1.6, feet.z), dv = at - eye;
            double yaw = Math.Atan2(-dv.x, -dv.z), pitch = Math.Atan2(dv.y, Math.Sqrt(dv.x * dv.x + dv.z * dv.z));
            g.Respawn(feet, yaw * 180 / Math.PI);
            g.input.SetLook(yaw, pitch);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
        }

        static IEnumerator L48LoomScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; sim.rowsOverride = null; Loom37View.Force47 = Loom37View.Force48 = -1; });
            sim.rowsOverride = sim.rowsRegistry.Defaults().Set("loom47", true).Set("kneeHold47", true);
            bool campaign = N24CampaignGate(g) == null;
            if (campaign)
            {
                offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
                Quiet(g);
                g.noRecords = g.noLab = true;
                g.setup.mode = Campaign24.Mode;
                g.StartRun(Seed);
                yield return Frames(240);
                for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
                DoorFree41(g);
                campaign = sim.campaignOn && sim.campaign != null;
            }
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            string pc = $"{(campaign ? "CAMPAIGN" : "survival")} run, map {g.MapId}, loom47 {sim.rows.On(sim.idLoom47)}, kneeHold47 {sim.rows.On(sim.rowsRegistry.IndexOf("kneeHold47"))}, config loomLook47 {NightRows.ViewOn(g.cfg, "loomLook47")} loomHull48 {NightRows.ViewOn(g.cfg, "loomHull48")}";
            if (!sim.Loom37Level || sim.loom37Ledges.Count == 0)
            {
                res[0] = "RED: not THE LOOM's level (boot -skygrindMap n47-span) | " + pc;
                yield break;
            }
            Start(g, "loom48", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            int shots = 0;
            SurvivalSim.Loom37Ledge start = sim.loom37Ledges.Find(l => l.id == "start");
            Vec3 startFeet = new Vec3(start.at.x, 0.05, start.at.z);
            g.Respawn(startFeet, 0);
            g.input.SetLook(0, 0);
            g.forcedMove = new InputFrame { yaw = 0, pitch = 0 };
            yield return Frames(30);
            if (sim.loom37 != null) { sim.loom37.Frozen = false; sim.loom37.Clear(); sim.bossKit?.Clear(); }
            if (!sim.BossRoomBegin("loom37", out _, out _) || sim.loom37 == null)
            {
                yield return Stop();
                res[0] = "RED: BossRoomBegin(loom37) returned false | " + pc;
                yield break;
            }
            Loom37 L = sim.loom37;
            Boss37Brain b = L.brain;
            yield return Frames(180); // the reveal (revealS 2) and the first glide onto its station
            Loom37View V = Loom37View.Current;
            if (V == null) { yield return Stop(); res[0] = "RED: no Loom37View | " + pc; yield break; }

            // A. THE LOOKS (G36; the blind pair's sets): the door, 10 m, 20 m, each look, the machine held still
            L.Frozen = true;
            bool hullShown = true;
            foreach (string v in L48Looks)
            {
                L48Look(v);
                L48Hold(g, startFeet, L.centre);
                yield return Frames(6);
                yield return Shot29(dir, "L48-" + v + "-door", st); shots++;
                foreach (double m in new[] { 10.0, 20.0 })
                {
                    Vec3 feet = L48Feet(L.centre, m, 0.35, 0.94, 3.5);
                    for (int f = 0; f < 4; f++) { L48Hold(g, feet, L.centre); yield return null; }
                    if (v == "n48") hullShown &= V.Hull48Shown;
                    yield return Shot29(dir, "L48-" + v + "-shuttle-" + m.ToString("0", ci), st); shots++;
                }
                // round 3 (the lead: "show the length in a side or rear frame"): the hull lies across the well (world x), so the broadside is from
                // the south; -long-30 is due south 30 m and nearly level (2 m under), -above-15 is 15 m south-east and 6 m ABOVE it (the lit top,
                // the bands, the ridge), -end-20 is 20 m due east (the point toward the camera: the end-on view, for contrast)
                foreach (var (nm, dist, bx, bz, drop) in new[] { ("long-30", 30.0, 0.0, 1.0, 2.0), ("above-15", 15.0, 0.35, 0.94, -6.0), ("end-20", 20.0, 1.0, 0.15, 3.0) })
                {
                    Vec3 feet = L48Feet(L.centre, dist, bx, bz, drop);
                    for (int f = 0; f < 4; f++) { L48Hold(g, feet, L.centre); yield return null; }
                    yield return Shot29(dir, "L48-" + v + "-" + nm, st); shots++;
                }
            }

            // B. THE LEG-STEP STRIP (G37): leg 1 (east, the near pair) from ~18 m, the machine still, ONE FULL CYCLE in 12 frames for each look:
            //    00 planted (before), the reel's forced step (Loom37View.Hull48ForceStep: its goal moved 1.8 m along the wall, the look's OWN step
            //    rule lifts it: n47 = NIGHT47's, whose idle shuffle can stall for good: round 1 saw "no step in 10 s"), 01..10 from the lift
            //    at the step's time / 10 apart (whole render frames), 11 planted (after). Round 1's strip started at a lift already 0-0.12 in,
            //    shot every render frame (0.2 s of a 0.35 s step), and n48's step ran short (the lead's velocity left stale by the look switch).
            bool stepSeen = true;
            string stepLine = "";
            foreach (string v in new[] { "n47", "n48" })
            {
                L48Look(v);
                const int leg = 1;
                Vec3 eastAt = new Vec3(L.centre.x + 15, L.centre.y - 9, L.centre.z);
                Vec3 feet = new Vec3(L.centre.x + 8, L.centre.y - 7 - 1.6, L.centre.z + 16);
                for (int f = 0; f < 30; f++) { L48Hold(g, feet, eastAt); yield return null; } // the look's springs settle, the lead's velocity fresh
                for (int f = 0; f < 120 && V.StepPhaseOf(leg) >= 0f; f++) { L48Hold(g, feet, eastAt); yield return null; } // planted first
                Vector3 f0 = V.FootDrawn(leg);
                var phases = new List<string> { "planted" };
                L48Hold(g, feet, eastAt);
                yield return Shot29(dir, "L48-step-" + v + "-00", st); shots++;
                V.Hull48ForceStep(leg);
                int wait = 0;
                for (; wait < 120 && V.StepPhaseOf(leg) < 0f; wait++) { L48Hold(g, feet, eastAt); yield return null; } // the other group may be lifting
                if (V.StepPhaseOf(leg) < 0f) { stepSeen = false; stepLine += $" {v}: the forced step never lifted in 2 s;"; continue; }
                float dur = V.StepDurOf(leg);
                int every = Math.Max(1, Mathf.CeilToInt(dur * UnityTuning.ReelFps / 10f));
                float lift = 0f;
                for (int n = 1; n <= 10; n++)
                {
                    L48Hold(g, feet, eastAt);
                    float p = V.StepPhaseOf(leg);
                    phases.Add(p < 0f ? "planted" : p.ToString("0.00", ci));
                    lift = Mathf.Max(lift, V.FootDrawn(leg).y - f0.y);
                    yield return Shot29(dir, "L48-step-" + v + "-" + n.ToString("00", ci), st); shots++;
                    for (int k = 0; k < every; k++) { L48Hold(g, feet, eastAt); yield return null; }
                }
                int after = 0;
                for (; after < 60 && V.StepPhaseOf(leg) >= 0f; after++) { L48Hold(g, feet, eastAt); yield return null; }
                bool planted = V.StepPhaseOf(leg) < 0f;
                for (int k = 0; k < 2; k++) { L48Hold(g, feet, eastAt); yield return null; } // the overshoot's settle
                phases.Add(planted ? "planted" : "STILL LIFTING");
                Vector3 f1 = V.FootDrawn(leg);
                yield return Shot29(dir, "L48-step-" + v + "-11", st); shots++;
                stepSeen &= planted && lift >= 0.8f * UnityTuning.Look47StepLift;
                stepLine += $" {v}: leg {leg}, waited {wait} frames to lift, step {dur.ToString("0.00", ci)} s, a frame every {every} render frames, phases [{string.Join(" ", phases)}], "
                    + $"lift {lift.ToString("0.00", ci)} m (of {UnityTuning.Look47StepLift.ToString("0.0", ci)}), moved {(f1 - f0).magnitude.ToString("0.00", ci)} m;";
                V.Hull48ForceStep(leg); // the goal back: the foot returns later on its own rule (not framed)
            }
            L.Frozen = false;

            // C. THE REAR (blind-pair moment) and THE ANTICIPATION STRIP: a forced FLOOR SWEEP per look (n47, n48; the n46 rear frame too)
            double rearMax = 0;
            string anticLine = "";
            Vec3 camFeet = L48Feet(L.centre, 20, 0.35, 0.94, 3.5);
            // C1. the rear: one forced tell held at 45 % (Frozen), each look in turn, its springs settled (40 frames) before its frame
            foreach (string v in new[] { "rear", "n47", "n48" })
            {
                if (v != "rear") L48Look(v);
                // wait out any attack running, then queue the sweep (or the lash) at the next rest (up to 15 s each)
                for (int f = 0; f < 900 && (b.state == Boss37State.Tell || b.state == Boss37State.Active); f++) { God(g); L48Hold(g, camFeet, L.centre); yield return null; }
                bool forced = false;
                for (int f = 0; f < 900 && !(b.state == Boss37State.Tell && b.Current != null); f++)
                {
                    God(g);
                    L48Hold(g, camFeet, L.centre);
                    if (!forced && b.Fighting && b.state == Boss37State.Rest) forced = b.Force(Loom37.Sweep) || b.Force(Loom37.Lash);
                    yield return null;
                }
                if (!(b.state == Boss37State.Tell && b.Current != null)) { anticLine += $" {v}: no tell;"; continue; }
                string id = b.Current.id;
                if (v == "rear")
                {
                    for (int f = 0; f < 900 && b.state == Boss37State.Tell && b.Progress < 0.45; f++) { God(g); L48Hold(g, camFeet, L.centre); yield return null; }
                    L.Frozen = true;
                    foreach (string lv in L48Looks)
                    {
                        L48Look(lv);
                        for (int f = 0; f < 40; f++) { L48Hold(g, camFeet, L.centre); yield return null; }
                        if (lv == "n48") rearMax = -V.Hull48TiltDeg;
                        yield return Shot29(dir, "L48-" + lv + "-rear", st); shots++;
                    }
                    L.Frozen = false;
                    anticLine += $" rear frames on {id} at {b.Progress.ToString("0.00", ci)};";
                    continue;
                }
                // C2. the strip: 12 frames evenly over the tell + 0.4 s from its start
                double tellS = b.stateTotal * sim.step, t0 = sim.SimTime, span = tellS + 0.4, vRear = 0;
                int nShot = 0;
                anticLine += $" {v} {id} tell {tellS.ToString("0.00", ci)} s:";
                while (nShot < 12 && sim.SimTime - t0 < span + 1.0)
                {
                    God(g);
                    L48Hold(g, camFeet, L.centre);
                    vRear = Math.Max(vRear, -V.Hull48TiltDeg);
                    if (sim.SimTime - t0 >= nShot * span / 12.0)
                    {
                        anticLine += $" {b.state.ToString()[0]}{b.Progress.ToString("0.00", ci)}/{V.Hull48TiltDeg.ToString("0.0", ci)}";
                        yield return Shot29(dir, "L48-antic-" + v + "-" + nShot.ToString("00", ci), st); shots++;
                        nShot++;
                        continue;
                    }
                    yield return null;
                }
                if (v == "n48") rearMax = Math.Max(rearMax, vRear);
                anticLine += ";";
            }

            // D. THE FLINCH STRIP: one shot at the body from ~12 m; frames after the first bossHit
            int hitStep = -1;
            Action<BossEvent> onHit = e => { if (e != null && e.boss == "loom37" && hitStep < 0) hitStep = Time.frameCount; };
            Action offHit = sim.events.On<BossEvent>("bossHit", onHit);
            offs.Add(offHit);
            double flinchPeak = 0, flinchPeakMs = -1;
            string flinchLine = "";
            int[] at = { 0, 2, 4, 6, 8, 12, 18, 30 };
            foreach (string v in new[] { "n47", "n48" })
            {
                L48Look(v);
                Vec3 feet = L48Feet(L.centre, 12, 0.2, 0.98, 2.0);
                for (int f = 0; f < 20; f++) { God(g); L48Hold(g, feet, L.centre); g.forcedCombat = IdleCombat; yield return null; }
                hitStep = -1;
                int fired = 0;
                for (int f = 0; f < 240 && hitStep < 0; f++)
                {
                    God(g);
                    L48Hold(g, feet, L.centre);
                    g.forcedCombat = f % 30 == 0 ? Fire : IdleCombat; // a press every 0.5 s until one lands
                    if (f % 30 == 0) fired++;
                    yield return null;
                }
                g.forcedCombat = IdleCombat;
                if (hitStep < 0) { flinchLine += $" {v}: no hit in {fired} shots;"; continue; }
                int h0 = hitStep, k = 0;
                double peak = 0, peakMs = -1;
                for (int f = 0; f <= 30; f++)
                {
                    God(g);
                    L48Hold(g, feet, L.centre);
                    double ms = (Time.frameCount - h0) * 1000.0 / UnityTuning.ReelFps;
                    if (Math.Abs(V.Hull48FlinchDeg) > peak) { peak = Math.Abs(V.Hull48FlinchDeg); peakMs = ms; }
                    if (k < at.Length && f == at[k]) { yield return Shot29(dir, "L48-flinch-" + v + "-" + k.ToString("00", ci), st); shots++; k++; continue; }
                    yield return null;
                }
                if (v == "n48") { flinchPeak = peak; flinchPeakMs = peakMs; }
                flinchLine += $" {v}: peak {peak.ToString("0.0", ci)} deg at {peakMs.ToString("0", ci)} ms;";
            }

            // E. kneeHold47 (ON for this run): the pilot + the seam policy until the first latched BEND, frames inside / after the hold
            L48Look("n48");
            g.Respawn(startFeet, 0);
            yield return Frames(4);
            var pilot = new Loom37Pilot();
            int lastSimStep = sim.simStep, latchedKnee = -1;
            double bendAt = -1, seamAt = -1, tStart = sim.SimTime;
            bool hA = false, hB = false, hC = false, seamShot = false;
            Vec3 kneeA = default, kneeB = default;
            Vector3 scrA = default, scrB = default, scrC = default;
            int fulls = 0;
            Action<Bend34Event> onBend = e => { if (e.tier == "full") fulls++; };
            Action offBend = sim.events.On<Bend34Event>("bend34Start", onBend);
            offs.Add(offBend);
            while (sim.SimTime - tStart < 60 && !(hA && hB && hC && seamShot) && L.Alive && !L.Falling)
            {
                God(g);
                int ran = Math.Max(1, sim.simStep - lastSimStep);
                lastSimStep = sim.simStep;
                InputFrame mv = default;
                CombatInput cb = new CombatInput { weapon = -1 };
                for (int k = 0; k < ran; k++)
                {
                    pilot.Step(sim, sim.simStep - ran + 1 + k);
                    InputFrame m = pilot.Move;
                    CombatInput c = pilot.Combat;
                    if (sim.Bend34Inside && !L.Open)
                        for (int i = 0; i < 6; i++)
                            if (L.Latched40(i))
                            {
                                Vec3 me = sim.mover.position;
                                (m.yaw, m.pitch) = Loom37Pilot.LookAt(new Vec3(me.x, me.y + 1.6, me.z), L.knees[i].at);
                                c.fire = new Button { held = true, pressed = (sim.simStep + k) % 4 == 0 };
                                break;
                            }
                    m.jump.pressed |= mv.jump.pressed; m.dash.pressed |= mv.dash.pressed; m.grapple.pressed |= mv.grapple.pressed; m.sprint.pressed |= mv.sprint.pressed;
                    m.grapple.held |= mv.grapple.pressed;
                    c.fire.pressed |= cb.fire.pressed; c.special.pressed |= cb.special.pressed; c.punch.pressed |= cb.punch.pressed;
                    c.special.held |= cb.special.pressed; c.punch.held |= cb.punch.pressed;
                    if (c.weapon < 0) c.weapon = cb.weapon;
                    mv = m;
                    cb = c;
                }
                g.input.SetLook(mv.yaw, mv.pitch);
                g.forcedMove = mv;
                g.forcedCombat = cb;
                double now = sim.SimTime - tStart;
                int lk = -1;
                if (sim.Bend34Inside && !L.Open) for (int i = 0; i < 6; i++) if (L.Latched40(i)) { lk = i; break; }
                if (lk >= 0 && bendAt < 0) { bendAt = now; latchedKnee = lk; }
                if (bendAt >= 0 && latchedKnee >= 0)
                {
                    Vector3 sp = g.cam != null ? g.cam.WorldToScreenPoint(new Vector3((float)L.knees[latchedKnee].at.x, (float)L.knees[latchedKnee].at.y, (float)-L.knees[latchedKnee].at.z)) : Vector3.zero;
                    if (!hA && now - bendAt >= 0.05) { hA = true; kneeA = L.knees[latchedKnee].at; scrA = sp; yield return Shot29(dir, "L48-hold-a", st); shots++; continue; }
                    if (!hB && now - bendAt >= 0.45) { hB = true; kneeB = L.knees[latchedKnee].at; scrB = sp; yield return Shot29(dir, "L48-hold-b", st); shots++; continue; }
                    if (!hC && now - bendAt >= 0.9) { hC = true; scrC = sp; yield return Shot29(dir, "L48-hold-c", st); shots++; continue; }
                }
                bool full = sim.Bend34TierNow == Bend34Tier.Full && lk >= 0;
                if (full && seamAt < 0) seamAt = now;
                if (!seamShot && seamAt >= 0 && now - seamAt >= 0.3) { seamShot = true; yield return Shot29(dir, "L48-seam", st); shots++; continue; }
                yield return null;
            }
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            double held = hA && hB ? Vec3.DistanceTo(kneeA, kneeB) : double.NaN;
            bool ok = shots >= 18 + 24 + 3 + 24 + 16 + 4 && hullShown && V.Hull48Aspect >= 2.5f && stepSeen && rearMax >= 8 && flinchPeak >= 3 && flinchPeakMs >= 0 && flinchPeakMs <= 100 && hA && hB && held < 0.05;
            res[0] = $"{(ok ? "GREEN" : "RED")}: frames {shots}; hull drawn at the n48 frames {hullShown}, its length / width {V.Hull48Aspect.ToString("0.00", ci)} (>= 2.5); step strip{stepLine} rear (n48) {rearMax.ToString("0.0", ci)} deg; antic{anticLine} flinch{flinchLine} "
                + $"kneeHold47: knee {latchedKnee} moved {(double.IsNaN(held) ? "n/a" : held.ToString("0.000", ci))} m over hold-a .. -b, screen a->b {(scrB - scrA).magnitude.ToString("0", ci)} px, b->c {(scrC - scrB).magnitude.ToString("0", ci)} px; "
                + $"FULL bends {fulls}, seam frame {seamShot}; leg steps {V.Steps47}, hits {V.Hull48Hits} (flinched {V.Hull48Flinches}) | {pc}";
        }
    }
}
