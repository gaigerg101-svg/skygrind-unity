// NIGHT47 builder L: reel phase a447 (FeelReel.N47.cs routes it here): THE LOOM's proof frames (G36-G40). Not run by its author (no Player in a
// worktree). Input through the game's forced move / combat frames only (L-046); the player kept alive (God); the rows: loom47 forced on for the run
// (sim.rowsOverride, before the run starts); span47 (the level) and loomLook47 (the look) come from the build copy's config
// (node Tools/night/n46-build-rows.cjs <tag> on n47.span47 n47.loomLook47) or the map: boot with -skygrindMap n47-span -skygrindReelOnly a447.
//   A. THE SPACE: L47-door (THE LOOM FRAME from the start ledge, looking north: the holds, the frame beam, the machine).
//   B. THE FIGHT: the player on the start ledge; BossRoomBegin("loom37"); Loom37Pilot + the seam policy (as n47loom --seam / n47lhuman's
//      H47PolicyLoom47), one pilot step per sim step of the frame (L-053). Frames (shot-N29-<name>.png):
//        L47-reveal          0.8 s in: the machine over the well
//        L47-shuttle-10 / -20  then the machine held still and seen from 10 m / 20 m south of and under its drum (the look, G36)
//        L47-tell-<id>       each attack's first tell at 70 % of its wind-up (lash, spool, sweep, web, dive, cage, bobbin); reel-only Force of the
//                            new attacks when a phase has not shown them 4 s in (dive / cage in phase 2, bobbin in phase 3; the game never forces)
//        L47-tell-<id>-late  the same tell in its last 0.2 s (the blink: "now")
//        L47-seam            the first FULL bend with a knee latched (the yellow seam), 0.3 s in
//        L47-tear            3 frames after the first tear (the knee gone, the warp plucked with loomLook47)
//        L47-window          0.5 s into the first CYAN window (the body sunk toward you)
//        L47-posts-fall      0.4 s after P1 -> P2's cut: the near posts tumbling into the void
//        L47-net             (round 4, THE WARP NET) after the first window, the player dropped from y 5 between the phase's two holds, looking
//                            down: 2 m over the net, the amber threads under him; L47-net-throw 0.3 s after the catch (the threads plucked)
//        L47-phase2 / -phase3  1 s after each phase change
//        L47-kill            0.5 s after the kill
//   Line: GREEN when the begin kept his feet, a FULL bend, a tear, a window, the kill and 7 attack ids were seen and every wind-up was 1.0-1.5 s
//   (the sweep its own 2.0 s); RED otherwise; the line carries the counters (seam tears, bends, dives / cages / bobbins).
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
        static partial void N47Loom(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = L47LoomScenario(g, dir, res, offs, pre);
        }

        static IEnumerator L47LoomScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; sim.rowsOverride = null; });
            sim.rowsOverride = sim.rowsRegistry.Defaults().Set("loom47", true).Set("kneeHold47", true); // + builder L's R4 row kneeHold47 (the L47-seam frame: the hooked knee held still)
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
            string pc = $"{(campaign ? "CAMPAIGN" : "survival")} run, map {g.MapId}, level side {sim.Loom37Level}, loom47 {sim.rows.On(sim.idLoom47)}, loomLook47 {NightRows.ViewOn(g.cfg, "loomLook47")}";
            if (!sim.Loom37Level || sim.loom37Ledges.Count == 0)
            {
                res[0] = "RED: not THE LOOM's level (boot -skygrindMap n47-span) | " + pc;
                yield break;
            }
            Start(g, "loom47", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            SurvivalSim.Loom37Ledge start = sim.loom37Ledges.Find(l => l.id == "start");
            Vec3 startFeet = new Vec3(start.at.x, 0.05, start.at.z);
            g.Respawn(startFeet, 0);
            yield return Frames(60);
            if (sim.loom37 != null && sim.loom37.Alive) sim.loom37.Frozen = true;
            yield return L37Shot(g, startFeet, new Vec3(start.at.x, 14, start.at.z - 40), dir, "L47-door", st);
            // B. the fight
            g.Respawn(startFeet, 0);
            g.input.SetLook(0, 0);
            g.forcedMove = new InputFrame { yaw = 0, pitch = 0 };
            yield return Frames(30);
            Vec3 before = sim.mover.position;
            if (sim.loom37 != null) { sim.loom37.Frozen = false; sim.loom37.Clear(); sim.bossKit?.Clear(); }
            if (!sim.BossRoomBegin("loom37", out _, out _) || sim.loom37 == null)
            {
                yield return Stop();
                res[0] = "RED: BossRoomBegin(loom37) returned false | " + pc;
                yield break;
            }
            yield return null;
            double moved = Vec3.DistanceTo(before, sim.mover.position);
            Loom37 L = sim.loom37;
            var pilot = new Loom37Pilot();
            double t0 = sim.SimTime, legAt = 0, windowAt = -1, killAt = -1, seamAt = -1;
            var told = new HashSet<string>();
            var late = new HashSet<string>();
            var forced = new HashSet<string>();
            int lastLeg = 1, tearF = -1, frame = 0, lastSimStep = sim.simStep, fulls = 0, bends = 0;
            bool postShot = false, netDrop = false, netShot = false, netThrowShot = false;
            int netCatches0 = 0;
            double netAt = -1;
            bool reveal = false, seamShot = false, tearShot = false, windowShot = false, p2Shot = false, p3Shot = false, killShot = false;
            sim.events.On<Bend34Event>("bend34Start", e => { bends++; if (e.tier == "full") fulls++; });
            double wallEnd = Time.realtimeSinceStartup + 200;
            while (Time.realtimeSinceStartup < wallEnd && sim.SimTime - t0 < 150)
            {
                God(g);
                frame++;
                double now = sim.SimTime - t0;
                if (L.Alive && !L.Falling && !(netDrop && !netThrowShot))
                {
                    int ran = Math.Max(1, sim.simStep - lastSimStep);
                    lastSimStep = sim.simStep;
                    InputFrame mv = default;
                    CombatInput cb = new CombatInput { weapon = -1 };
                    for (int k = 0; k < ran; k++)
                    {
                        pilot.Step(sim, sim.simStep - ran + 1 + k);
                        InputFrame m = pilot.Move;
                        CombatInput c = pilot.Combat;
                        // the seam policy (L-cards.md section 1): inside a BEND with a knee latched, look at it and fire
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
                }
                else { g.forcedMove = new InputFrame { yaw = 0, pitch = netDrop && !netThrowShot ? -1.1 : -0.4 }; g.forcedCombat = IdleCombat; if (netDrop && !netThrowShot) g.input.SetLook(0, -1.1); }
                if (!reveal && now >= 0.8)
                {
                    reveal = true;
                    yield return Shot29(dir, "L47-reveal", st);
                    // THE SHUTTLE at 10 m and 20 m (the lead's n47-r1c note 2: "no frame shows THE LOOM's body"): the machine held still, the camera put
                    // south of the drum and a little under it, looking at it; then the player back where he was (God: the two frames' fall is nothing)
                    Vec3 back = sim.mover.position;
                    double backYaw = 0; // the pilot sets the look again on its next step
                    L.Frozen = true;
                    foreach (double m in new[] { 10.0, 20.0 })
                    {
                        Vec3 feet = new Vec3(L.centre.x + m * 0.35, L.centre.y - 3.5, L.centre.z + m * 0.94);
                        Vec3 eye = new Vec3(feet.x, feet.y + 1.6, feet.z), dv = L.centre - eye;
                        double yaw = Math.Atan2(-dv.x, -dv.z), pitch = Math.Atan2(dv.y, Math.Sqrt(dv.x * dv.x + dv.z * dv.z));
                        g.Respawn(feet, yaw * 180 / Math.PI);
                        g.input.SetLook(yaw, pitch);
                        g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
                        yield return Frames(3);
                        yield return Shot29(dir, "L47-shuttle-" + m.ToString("0", ci), st);
                    }
                    L.Frozen = false;
                    g.Respawn(back, backYaw);
                    yield return Frames(2);
                    continue;
                }
                Boss37Brain b = L.brain;
                if (L.Alive && !L.Falling)
                {
                    // reel-only: a phase that has not shown its new attacks 4 s in queues them (Boss37Brain.Force, the brain's own staple queue)
                    if (b.Fighting && b.state == Boss37State.Rest && now - legAt > 4)
                    {
                        // (round 2, the lead's n47-r0d line: "attacks told 5 [spool,lash,dive,cage,bobbin]": the sweep's 1.5 s gap and the web's camp
                        // habit never came in a reel fight, so the reel queues those too: the sweep in phase 1, the web in phase 2)
                        string want = b.leg == 1 && !told.Contains(Loom37.Sweep) ? Loom37.Sweep
                            : b.leg == 2 && !told.Contains(Loom37.Dive) ? Loom37.Dive : b.leg == 2 && !told.Contains(Loom37.Cage) ? Loom37.Cage
                            : b.leg == 2 && !told.Contains(Loom37.Web) ? Loom37.Web : b.leg == 3 && !told.Contains(Loom37.Bobbin) ? Loom37.Bobbin : null;
                        if (want != null && forced.Add(want + b.leg)) b.Force(want);
                    }
                    if (b.state == Boss37State.Tell && b.Current != null)
                    {
                        string id = b.Current.id;
                        double left = (b.stateTotal - b.stateSteps) * sim.step;
                        if (b.Progress >= 0.7 && told.Add(id)) { yield return Shot29(dir, "L47-tell-" + id, st); continue; }
                        if (left <= 0.2 && told.Contains(id) && late.Add(id)) { yield return Shot29(dir, "L47-tell-" + id + "-late", st); continue; }
                    }
                    bool seam = sim.Bend34TierNow == Bend34Tier.Full && !L.Open;
                    if (seam) { bool latched = false; for (int i = 0; i < 6; i++) latched |= L.Latched40(i); seam = latched; }
                    if (seam && seamAt < 0) seamAt = now;
                    if (!seamShot && seamAt >= 0 && now - seamAt >= 0.3) { seamShot = true; yield return Shot29(dir, "L47-seam", st); continue; }
                    if (L.tears > 0 && tearF < 0) tearF = frame;
                    if (!tearShot && tearF > 0 && frame - tearF >= 3) { tearShot = true; yield return Shot29(dir, "L47-tear", st); continue; }
                    if (b.Open && windowAt < 0) windowAt = now;
                    if (!windowShot && windowAt >= 0 && now - windowAt >= 0.5) { windowShot = true; yield return Shot29(dir, "L47-window", st); continue; }
                    if (b.leg != lastLeg) { lastLeg = b.leg; legAt = now; }
                    // THE WARP NET's frames (round 4): dropped between the phase's two holds once, after the first window closed
                    if (windowShot && !netDrop && !b.Open && b.state == Boss37State.Rest && !double.IsNaN(sim.Loom47NetY))
                    {
                        SurvivalSim.Loom37Ledge hw = sim.loom37Ledges.Find(l => l.id == "t" + b.leg + "w"), he = sim.loom37Ledges.Find(l => l.id == "t" + b.leg + "e");
                        if (hw != null && he != null)
                        {
                            netDrop = true;
                            netCatches0 = sim.loom47NetCatches;
                            g.Respawn(new Vec3((hw.at.x + he.at.x) * 0.5, 5, (hw.at.z + he.at.z) * 0.5), 0);
                            g.input.SetLook(0, -1.1);
                            g.forcedMove = new InputFrame { yaw = 0, pitch = -1.1 };
                            g.forcedCombat = IdleCombat;
                            yield return null;
                            continue;
                        }
                    }
                    if (netDrop && !netShot && sim.mover.velocity.y < 0 && sim.mover.position.y < sim.Loom47NetY + 2) { netShot = true; yield return Shot29(dir, "L47-net", st); continue; }
                    if (netDrop && netAt < 0 && sim.loom47NetCatches > netCatches0) netAt = now;
                    if (netDrop && !netThrowShot && netAt >= 0 && now - netAt >= 0.3) { netThrowShot = true; yield return Shot29(dir, "L47-net-throw", st); continue; }
                    if (netDrop && !netThrowShot && now - legAt > 0 && sim.mover.position.y < sim.Loom47NetY - 8) netThrowShot = true; // it fell through: give the fight back
                    if (!postShot && sim.loom47PostsFellStep >= 0 && (sim.simStep - sim.loom47PostsFellStep) * sim.step >= 0.4) { postShot = true; yield return Shot29(dir, "L47-posts-fall", st); continue; }
                    if (!p2Shot && b.leg == 2 && now - legAt >= 1) { p2Shot = true; yield return Shot29(dir, "L47-phase2", st); continue; }
                    if (!p3Shot && b.leg == 3 && now - legAt >= 1) { p3Shot = true; yield return Shot29(dir, "L47-phase3", st); continue; }
                }
                if ((L.Falling || !L.Alive) && killAt < 0) killAt = now;
                if (!killShot && killAt >= 0 && now - killAt >= 0.5) { killShot = true; yield return Shot29(dir, "L47-kill", st); break; }
                yield return null;
            }
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            var sb = new System.Text.StringBuilder();
            bool tellsOk = true;
            foreach (var (a, steps) in L.brain.tellLog)
            {
                double len = steps * sim.step;
                bool inBand = a == Loom37.Sweep ? len >= 0.89 && len <= 2.01 : len >= 0.99 && len <= 1.51;
                tellsOk &= inBand;
                if (!inBand) sb.Append($" {a} {len.ToString("0.00", ci)} s RED;");
            }
            bool ok = moved < 0.05 && fulls > 0 && L.tears > 0 && windowAt >= 0 && killAt >= 0 && told.Count >= 7 && tellsOk;
            res[0] = $"{(ok ? "GREEN" : "RED")}: the begin kept the player where he stood (moved {moved.ToString("0.000", ci)} m); attacks told {told.Count} [{string.Join(",", told)}] (reel-forced {forced.Count}); "
                + $"bends {bends} (FULL {fulls}); tears {L.tears} (seam {L.seamTears47}); first window {(windowAt >= 0 ? windowAt.ToString("0.0", ci) + " s" : "none")}; "
                + $"{(killAt >= 0 ? "killed at " + killAt.ToString("0.0", ci) + " s" : "NOT killed")}; dives {L.dives47} cages {L.cages47} bobbins {L.bobbins47}; tells{(sb.Length > 0 ? sb.ToString() : " all in band")}; "
                + $"look47 shown {(Loom37View.Current != null && Loom37View.Current.Look47Shown)}, leg steps {(Loom37View.Current != null ? Loom37View.Current.Steps47 : -1)}, posts fell {(sim.loom47PostsFellStep >= 0)} (frame {postShot}) | {pc}";
        }
    }
}
