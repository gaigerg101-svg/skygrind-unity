// NIGHT47 builder L: reel phase a547 (FeelReel.N47.cs routes it here): level 5's route and THE CROWN's proof frames (G41, G42, G43 boss part). Not
// run by its author (no Player in a worktree). Input through the game's forced move / combat frames only (L-046); the player kept alive (God);
// crown47 forced on for the run (sim.rowsOverride); boot with -skygrindMap n47-spire -skygrindReelOnly a547 (spire47's level: THE CROWN'S WINDOWS).
//   A. THE ROUTE (G42): L47-route-v1 .. L47-route-v10: from each room's spawn (v1's is the level's start), looking ahead (-z) at the room's exit wall
//      (the lit window over or beside its door).
//   B. THE FLAME (G43, boss part): the player on the summit's checkpoint; BossRoomBegin("crown37"); the player stands and looks at the ring; the reel
//      queues CROWN FIRE (Boss37Brain.Force, reel-only) at each rest until its frames are taken:
//        L47-fire-tell        the tell at 50 % (the nozzle heating, the floor line half drawn)
//        L47-fire-tell-late   its last 0.2 s (the line whole, blinking)
//        L47-fire-sweep       the sweep at 50 % (the cone, embers, the light, the shudder)
//        L47-fire-vent        0.3 s into the vent (the ember pulse, the scorch marks)
//        L47-fire-scorch      1.5 s after the vent began (the scorches fading)
//        L47-fire-sweep-B     the next sweep at 50 % drawn in the blind pair's B look (Crown37View.FlameVariant47 1: a particle cone)
//        L47-tell-<id>        each other attack's first tell at 70 %
//   Line: GREEN when the ten route frames, the five flame frames were taken and the view drew the flame (Crown37View.FlameShown47 > 0 at the sweep
//   frame) and its floor line (LineShown47 > 0 at the tell frame); RED otherwise.
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
        static partial void N47Crown(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = L47CrownScenario(g, dir, res, offs, pre);
        }

        static IEnumerator L47CrownScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; sim.rowsOverride = null; });
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
            string pc = $"{(campaign ? "CAMPAIGN" : "survival")} run, map {g.MapId}, crown47 {sim.rows.On(sim.idCrown47)}, spire {sim.spire37Row != null}";
            if (sim.orbit37 == null || sim.spire37Row == null || sim.campaignLevel == null)
            {
                res[0] = "RED: no spire (boot -skygrindMap n47-spire) | " + pc;
                yield break;
            }
            var st = new double[4];
            Start(g, "crown47", "camera", dir);
            yield return Frames(pre);
            // A. the route
            int route = 0;
            foreach (RoomDef r in sim.campaignLevel.rooms)
            {
                if (r.id == "v11") continue;
                Vec3 feet = new Vec3(r.spawn.x, r.spawn.y + 0.02, r.spawn.z);
                yield return Stand37(g, feet, new Vec3(r.spawn.x, 4.5, r.z0 + 0.5), 40);
                yield return Shot29(dir, "L47-route-" + r.id, st);
                route++;
            }
            // B. the flame (round 2, the lead's n47-r1 frames: v9's "FALLING STONES" banner hung over both flame frames: the route frames teleport
            // through ten rooms and each queued its banner; the queue is silenced before the fight, as DoorFree41 does after DOORFREE)
            g.UiView?.ClearBanners41();
            Vec3 stub = sim.spire37Checkpoint[0];
            yield return Stand37(g, new Vec3(stub.x, stub.y + 0.02, stub.z - 9), new Vec3(sim.spire37Axis.x, 2, sim.spire37Axis.z), 20);
            if (sim.crown37 != null && sim.crown37.Alive) { sim.crown37.Clear(); sim.bossKit.Clear(); }
            if (!sim.BossRoomBegin("crown37", out _, out _) || sim.crown37 == null)
            {
                yield return Stop();
                res[0] = "RED: BossRoomBegin(crown37) returned false | " + pc;
                yield break;
            }
            yield return null;
            Crown37 c = sim.crown37;
            double t0 = sim.SimTime, ventAt = -1;
            bool tell = false, tellLate = false, sweep = false, vent = false, scorch = false, sweepB = false;
            offs.Add(() => Crown37View.FlameVariant47 = 0);
            int lineSeen = 0, flameSeen = 0;
            var told = new HashSet<string>();
            double wallEnd = Time.realtimeSinceStartup + 120;
            while (Time.realtimeSinceStartup < wallEnd && sim.SimTime - t0 < 80 && !(tell && tellLate && sweep && vent && scorch && sweepB))
            {
                God(g);
                double now = sim.SimTime - t0;
                Look37(g, sim.mover.position, c.centre);
                Boss37Brain b = c.brain;
                if (b.state == Boss37State.Rest && !(tell && sweep && vent && sweepB)) b.Force(Crown37.CrownFire);
                if (c.FireTelling && !tell && b.Progress >= 0.5) { tell = true; lineSeen = Crown37View.LineShown47; yield return Shot29(dir, "L47-fire-tell", st); continue; }
                if (c.FireTelling && tell && !tellLate && (b.stateTotal - b.stateSteps) * sim.step <= 0.2) { tellLate = true; yield return Shot29(dir, "L47-fire-tell-late", st); continue; }
                if (c.FireLive && !sweep && b.Progress >= 0.5) { sweep = true; flameSeen = Crown37View.FlameShown47; yield return Shot29(dir, "L47-fire-sweep", st); continue; }
                // the blind pair's B look (Crown37View.FlameVariant47 1): the next sweep after the scorch frame, at the same 50 %
                if (scorch && !sweepB && c.FireLive && b.Progress >= 0.5) { sweepB = true; Crown37View.FlameVariant47 = 1; yield return null; yield return Shot29(dir, "L47-fire-sweep-B", st); Crown37View.FlameVariant47 = 0; continue; }
                if (c.Venting && ventAt < 0) ventAt = now;
                if (!vent && ventAt >= 0 && now - ventAt >= 0.3) { vent = true; yield return Shot29(dir, "L47-fire-vent", st); continue; }
                if (!scorch && ventAt >= 0 && now - ventAt >= 1.5) { scorch = true; yield return Shot29(dir, "L47-fire-scorch", st); continue; }
                if (b.state == Boss37State.Tell && b.Current != null && b.Current.id != Crown37.CrownFire && b.Progress >= 0.7 && told.Add(b.Current.id)) { yield return Shot29(dir, "L47-tell-" + b.Current.id, st); continue; }
                yield return null;
            }
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            bool ok = route == 10 && tell && tellLate && sweep && vent && scorch && flameSeen > 0 && lineSeen > 0;
            res[0] = $"{(ok ? "GREEN" : "RED")}: route frames {route} / 10; flame frames tell {tell} late {tellLate} sweep {sweep} vent {vent} scorch {scorch}; "
                + $"drawn: floor line segments {lineSeen}, flame layers {flameSeen}; fires {c.fires47}, fire ticks {c.fireTicks47}; other tells [{string.Join(",", told)}] | {pc}";
        }
    }
}
