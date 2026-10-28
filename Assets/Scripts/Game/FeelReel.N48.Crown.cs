// NIGHT48 builder L: reel phase a548 (FeelReel.N48.cs routes it here): THE CROWN's flame, the boss side of G43 (NIGHT48.md: "contact sheets
// of both, the blind pair L47-fire-sweep vs -sweep-B (3 judges)"; DECIDE48 D-11: "judged on the same moment, frozen at the same frame").
// Not run by its author (no Player in a worktree). Boot: -skygrindMap n47-spire -skygrindReelOnly a548. crown47 forced on (sim.rowsOverride).
// What a547 got wrong for a pair (read on Logs/feel/n47-r3/a547/a547/shot-N29-L47-fire-sweep(-B).png): its A and B were two DIFFERENT sweeps
// (in B the player stood in the cone: the hurt vignette over the whole frame), seen from 9 m in front of the ring (the ring fills the frame,
// the flame shows only at its edge). Here: the player stands where a547 stood while the tell aims (CROWN FIRE aims at the feet when its tell
// starts), then the camera moves to the SIDE of the cone (SideM m off the cone's middle line, out of its arc), so the stream is seen whole in
// profile, and nothing burns the camera.
//   L48-fire-A-tNN / -B-tNN  the CONTACT SHEETS: every render frame from 2 before the sweep's first tick to 30 after (t m02 .. p30), the
//                            A look (Crown37View.FlameVariant47 0, the mesh stream) on fire 1, the B look (1, the particle cone) on fire 2
//   L48-fire-pair-A / -B     THE BLIND PAIR: fire 1 frozen (Crown37.Frozen) at its sweep's 50 %, the A look, then the B look on the same
//                            frozen moment (3 frames apart, same camera)
//   (round 3, DECIDE48 D-30: the particle cone won the pair 3-0 and is now the flame48 view row: the looks are switched by
//   Crown37View.Flame48Force: A = flame48 off = NIGHT47's flame, B = flame48 on)
//   L48-fire-flame48-tm02..tp30    fire 3 (round 4), flame48 on: the cone's own 33-frame contact sheet (2 before the first tick to 30 after)
//   L48-fire-flame48-sweep-00..11  fire 4, flame48 on: the new default's FULL SWEEP, 12 frames at its progress 0, 1/12 .. 11/12
//   L48-fire-flame48-vent    fire 3's vent, 0.3 s in
//   L48-fire-tell / -late    fire 1's tell at 50 % and in its last 0.2 s (the floor line), from the side
//   L48-fire-vent / -scorch  0.3 s and 1.5 s into fire 1's vent (the lick, the scorch marks)
// Line: GREEN when both sheets (33 frames each), the pair (same brain progress on both frames) and the four tell / vent frames were taken and
// the view drew the flame at the pair (FlameShown47 > 0 each); RED otherwise. No event names added (crown47Fire's stages are NIGHT47's).
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
        static partial void N48Crown(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = L48CrownScenario(g, dir, res, offs, pre);
        }

        /// <summary>The side camera for a cone from `centre` along the flat unit `mid`: SideM m off the cone's middle point, at the stand's
        /// floor height, looking at that middle point.</summary>
        static (Vec3 feet, Vec3 at) L48FireSide(SurvivalSim sim, Vec3 centre, Vec3 mid, double floorY, double reach)
        {
            double n = Math.Sqrt(mid.x * mid.x + mid.z * mid.z);
            double mx = n > 1e-9 ? mid.x / n : 0, mz = n > 1e-9 ? mid.z / n : 1;
            Vec3 at = new Vec3(centre.x + mx * reach * 0.5, (centre.y + floorY) * 0.5, centre.z + mz * reach * 0.5);
            // the side: perpendicular to the cone's middle line, back toward the stand a little (the stream and the ring both in frame).
            // Round 5 (the lead's n48-r4 read of L48-fire-flame48-tm02 / L48-fire-B-tm02: "a whole-screen lilac and white wash with white bars";
            // read on the frames: the banner says THE RING, so the camera stood in v10 behind the shaft's wall, looking through the room's
            // pale boundary and its slabs; the cone itself drew small and orange in the middle): of the two sides take the one INSIDE THE
            // SHAFT's room rect (sim.spire37Rect) or, failing that, the one nearer the shaft's axis, then clamp it 2 m inside the rect.
            Vec3 Side(double s) => new Vec3(at.x - s * mz * UnityTuning.L48FireSideM + mx * UnityTuning.L48FireBackM, floorY, at.z + s * mx * UnityTuning.L48FireSideM + mz * UnityTuning.L48FireBackM);
            double[] r = sim.spire37Rect;
            bool rect = !double.IsNaN(r[0]);
            double x0 = Math.Min(r[0], r[2]) + 2, x1 = Math.Max(r[0], r[2]) - 2, z0 = Math.Min(r[1], r[3]) + 2, z1 = Math.Max(r[1], r[3]) - 2;
            bool In(Vec3 p) => rect && p.x >= x0 && p.x <= x1 && p.z >= z0 && p.z <= z1;
            double Ax(Vec3 p) => (p.x - sim.spire37Axis.x) * (p.x - sim.spire37Axis.x) + (p.z - sim.spire37Axis.z) * (p.z - sim.spire37Axis.z);
            Vec3 a = Side(1), b = Side(-1);
            Vec3 feet = In(a) && !In(b) ? a : In(b) && !In(a) ? b : Ax(a) <= Ax(b) ? a : b;
            if (rect) feet = new Vec3(Math.Max(x0, Math.Min(x1, feet.x)), feet.y, Math.Max(z0, Math.Min(z1, feet.z)));
            return (feet, at);
        }

        static IEnumerator L48CrownScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; sim.rowsOverride = null; Crown37View.FlameVariant47 = 0; if (sim.crown37 != null) sim.crown37.Frozen = false; });
            sim.rowsOverride = sim.rowsRegistry.Defaults().Set("crown47", true);
            bool campaign = N24CampaignGate(g) == null;
            if (campaign)
            {
                offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
                Quiet(g);
                g.noRecords = g.noLab = true;
                g.setup.mode = Campaign24.Mode;
                g.StartRun(Seed);
                yield return Frames(30);
                for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
                DoorFree41(g);
                campaign = sim.campaignOn && sim.campaign != null;
            }
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            string pc = $"{(campaign ? "CAMPAIGN" : "survival")} run, map {g.MapId}, crown47 {sim.rows.On(sim.idCrown47)}, crownLook47 {NightRows.ViewOn(g.cfg, "crownLook47")}";
            if (sim.orbit37 == null || sim.spire37Row == null || sim.campaignLevel == null)
            {
                res[0] = "RED: no spire (boot -skygrindMap n47-spire) | " + pc;
                yield break;
            }
            var st = new double[4];
            Start(g, "crown48", "camera", dir);
            yield return Frames(pre);
            g.UiView?.ClearBanners41();
            Vec3 stub = sim.spire37Checkpoint[0];
            Vec3 stand = new Vec3(stub.x, stub.y + 0.02, stub.z - 9);
            Vec3 axisAt = new Vec3(sim.spire37Axis.x, 2, sim.spire37Axis.z);
            yield return Stand37(g, stand, axisAt, 20);
            if (sim.crown37 != null && sim.crown37.Alive) { sim.crown37.Clear(); sim.bossKit.Clear(); }
            if (!sim.BossRoomBegin("crown37", out _, out _) || sim.crown37 == null)
            {
                yield return Stop();
                res[0] = "RED: BossRoomBegin(crown37) returned false | " + pc;
                yield break;
            }
            yield return null;
            g.UiView?.ClearBanners41();
            Crown37 c = sim.crown37;
            Boss37Brain b = c.brain;
            int shots = 0, sheetA = 0, sheetB = 0, sheet48 = 0, flameA = 0, flameB = 0;
            double progA = -1, progB = -1;
            bool tell = false, late = false, vent = false, scorch = false, pair = false, pairB48 = false, vent48 = false;
            int sweep48 = 0;
            bool camInside = true;
            string line = "";
            for (int fire = 1; fire <= 4; fire++) // fire 3 (round 4): the cone's own contact sheet L48-fire-flame48-t*; fire 4 (round 3): its FULL SWEEP, evenly spaced
            {
                string v = fire == 1 ? "A" : fire == 2 ? "B" : fire == 3 ? "flame48" : "flame48-sweep";
                Crown37View.Flame48Force = fire == 1 ? 0 : 1; // round 3 (DECIDE48 D-30): B is the flame48 row on (the particle cone), A NIGHT47's flame
                int sweepN = 0;
                var sweepAt = new List<string>();
                // the stand while the tell aims: wait out anything running, queue CROWN FIRE at a rest (up to 20 s)
                bool aimed = false;
                for (int f = 0; f < 1200 && !aimed; f++)
                {
                    God(g);
                    g.Respawn(stand, 0);
                    Look37(g, stand, c.centre);
                    if (b.state == Boss37State.Rest) b.Force(Crown37.CrownFire);
                    aimed = c.FireTelling;
                    yield return null;
                }
                if (!aimed) { line += $" fire {v}: no tell in 20 s;"; continue; }
                var (feet, at) = L48FireSide(sim, c.centre, c.fireMid47, stand.y, c.p47 != null ? c.p47.reachM : 9);
                double[] rr = sim.spire37Rect;
                bool inRoom = double.IsNaN(rr[0]) || (feet.x >= Math.Min(rr[0], rr[2]) && feet.x <= Math.Max(rr[0], rr[2]) && feet.z >= Math.Min(rr[1], rr[3]) && feet.z <= Math.Max(rr[1], rr[3]));
                camInside &= inRoom;
                line += $" fire {v} camera ({feet.x.ToString("0.0", ci)}, {feet.z.ToString("0.0", ci)}) inside the shaft's room {inRoom};";
                int tickStep = -1, ticks0 = c.fireTicks47, lastOff = -99;
                bool sweepSeen = false;
                int sweepStart = -1;
                double ventAt = -1;
                for (int f = 0; f < 900; f++)
                {
                    God(g);
                    L48Hold(g, feet, at);
                    if (fire == 1 && c.FireTelling && !tell && b.Progress >= 0.5) { tell = true; yield return Shot29(dir, "L48-fire-tell", st); shots++; continue; }
                    if (fire == 1 && c.FireTelling && tell && !late && (b.stateTotal - b.stateSteps) * sim.step <= 0.2) { late = true; yield return Shot29(dir, "L48-fire-late", st); shots++; continue; }
                    if (c.FireLive && !sweepSeen) { sweepSeen = true; sweepStart = sim.simStep; tickStep = sweepStart + Math.Max(1, (int)Math.Round((c.p47 != null ? c.p47.tickS : 0.25) * 120)); }
                    if (sweepSeen && c.FireLive)
                    {
                        // the contact sheet: render frames from 2 before the first tick to 30 after, named by the offset in render frames
                        int offF = (int)Math.Round((sim.simStep - tickStep) / (120.0 / UnityTuning.ReelFps));
                        if (fire <= 3 && offF >= -2 && offF <= 30 && offF != lastOff)
                        {
                            lastOff = offF;
                            string nm = (offF < 0 ? "m" : "p") + Math.Abs(offF).ToString("00", ci);
                            yield return Shot29(dir, "L48-fire-" + v + "-t" + nm, st); shots++;
                            if (offF == -2) line += $" {v}-tm02 mid-frame luminance {st[3].ToString("0.000", ci)};"; // the wash's measure (round 5)
                            if (fire == 1) sheetA++; else if (fire == 2) sheetB++; else sheet48++;
                            continue;
                        }
                        // THE PAIR: fire 1 at its sweep's 50 %, frozen, A then B on the same moment
                        if (fire == 1 && !pair && b.Progress >= 0.5 && offF > 30)
                        {
                            pair = true;
                            c.Frozen = true;
                            Crown37View.Flame48Force = 0;
                            for (int k = 0; k < 3; k++) { L48Hold(g, feet, at); yield return null; }
                            progA = b.Progress; flameA = Crown37View.FlameShown47;
                            yield return Shot29(dir, "L48-fire-pair-A", st); shots++;
                            Crown37View.Flame48Force = 1;
                            for (int k = 0; k < 3; k++) { L48Hold(g, feet, at); yield return null; }
                            progB = b.Progress; flameB = Crown37View.FlameShown47;
                            pairB48 = Crown37View.Flame48Shown && Crown37View.FlameVariant47 == 1;
                            yield return Shot29(dir, "L48-fire-pair-B", st); shots++;
                            Crown37View.Flame48Force = 0;
                            c.Frozen = false;
                            continue;
                        }
                        // THE FULL SWEEP of the new default (fire 2, flame48 on): 12 frames at the sweep's progress 0, 1/12 .. 11/12 (a frame
                        // the contact sheet already took moves to the next free frame; the line prints each frame's real progress)
                        if (fire == 4 && sweepN < 12 && b.Progress >= sweepN / 12.0)
                        {
                            sweepAt.Add(b.Progress.ToString("0.00", ci));
                            yield return Shot29(dir, "L48-fire-flame48-sweep-" + sweepN.ToString("00", ci), st); shots++;
                            sweepN++;
                            continue;
                        }
                    }
                    if (c.Venting && ventAt < 0) ventAt = sim.SimTime;
                    if (fire == 1 && !vent && ventAt >= 0 && sim.SimTime - ventAt >= 0.3) { vent = true; yield return Shot29(dir, "L48-fire-vent", st); shots++; continue; }
                    if (fire == 1 && !scorch && ventAt >= 0 && sim.SimTime - ventAt >= 1.5) { scorch = true; yield return Shot29(dir, "L48-fire-scorch", st); shots++; break; }
                    if ((fire == 2 || fire == 3) && ventAt >= 0) break;
                    if (fire == 4 && ventAt >= 0 && sim.SimTime - ventAt >= 0.3) { vent48 = true; yield return Shot29(dir, "L48-fire-flame48-vent", st); shots++; break; }
                    yield return null;
                }
                if (fire == 4) { sweep48 = sweepN; line += $" flame48 sweep frames {sweepN} at [{string.Join(" ", sweepAt)}];"; }
                // round 4 (the lead's n48-r3 read "fire flame48: sheet 0 frames": the old fire 3 took the full sweep only, by design, and fire B's
                // sheet was the cone's): fire 3 now takes the cone's OWN 33-frame sheet under its own name, fire 4 the full sweep; the line per fire
                bool cone = Crown37View.Flame48Shown && Crown37View.FlameVariant47 == 1;
                line += fire == 1 ? $" fire A (flame48 off, NIGHT47's flame): ticks {c.fireTicks47 - ticks0}, contact sheet L48-fire-A-t* {sheetA} frames;"
                    : fire == 2 ? $" fire B (flame48 ON): ticks {c.fireTicks47 - ticks0}, contact sheet L48-fire-B-t* {sheetB} frames, cone drawn {cone};"
                    : fire == 3 ? $" fire flame48 (ON): ticks {c.fireTicks47 - ticks0}, contact sheet L48-fire-flame48-t* {sheet48} frames, cone drawn {cone};"
                    : $" fire flame48-sweep (ON): ticks {c.fireTicks47 - ticks0}, the full sweep, cone drawn {cone};";
            }
            Crown37View.Flame48Force = -1;
            Crown37View.FlameVariant47 = 0;
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            bool same = pair && Math.Abs(progA - progB) < 1e-9;
            bool ok = camInside && sheetA == 33 && sheetB == 33 && sheet48 == 33 && same && flameA > 0 && flameB > 0 && tell && late && vent && scorch && pairB48 && sweep48 == 12 && vent48;
            line += $" flame48 drew the cone at pair-B {pairB48}, its vent frame {vent48};";
            res[0] = $"{(ok ? "GREEN" : "RED")}: frames {shots}; contact sheets A {sheetA} / B {sheetB} / flame48 {sheet48} (of 33);{line} pair {(pair ? "taken" : "NOT taken")} at progress A {progA.ToString("0.000", ci)} / B {progB.ToString("0.000", ci)} (same moment {same}), "
                + $"flame layers drawn A {flameA} / B {flameB}; tell {tell} late {late} vent {vent} scorch {scorch}; fires {c.fires47}, ticks {c.fireTicks47}, hits on the player {c.fireHits47} | {pc}";
        }
    }
}
