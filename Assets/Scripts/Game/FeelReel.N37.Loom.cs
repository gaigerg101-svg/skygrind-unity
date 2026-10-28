// NIGHT37 builder O: the Player scenario of loom37 (FeelReel.N27.cs PhaseN37O; the body is the partial N37LoomBody). Not run by its author (no
// Player in the worktree). Input through the game's forced move / combat frames only (L-046); the player kept alive (God).
//   loom37  THE CANYON: boot -skygrindMap n37-span with design.n37.loom37 on. A CAMPAIGN run (as C's crown37 reel; DOORFREE): entering THE CANYON
//     makes the campaign begin the fight itself; the space frames hold it frozen, then the reel restarts it at the start ledge.
//     A. THE SPACE. Frames (shot-N29-<name>.png):
//        L37-space-o1      room o1 THREAD LINE from its start ledge: the thread ring over the void, the tower's cyan ride face beyond it
//        L37-space-o2      room o2 SWING ANCHOR from its start ledge: the swinging ring, the post, the far ledge
//        L37-space-start   THE CANYON from its start ledge (room o3, "4.10 THE CANYON"), looking north: the void, tower pairs 1 and 2 with cyan
//                          faces, thread rings over the tower tops, the dusk sky (no roof)
//        L37-space-mid     from tower 1 west's top, looking north along the canyon: tower pair 2, the mid-gap rings, pair 3 far off
//        L37-space-nest    from tower 2 west's top, looking at the nest (tower pair 3) and the end ledge
//     B. THE FIGHT: the player on the start ledge; BossRoomBegin("loom37") (no Respawn: his feet are checked); Loom37Pilot plays, stepped
//        once per SIM step of the frame's input (the harness's pilot, the harness's step count). If the pilot has torn no knee 7 s into a leg
//        the reel PUPPETS one tear (nails put in a live knee + the LT detonate; counted apart in the line). Frames:
//        L37-fight-reveal  0.8 s in: THE LOOM over the canyon between tower pairs 1 and 2: dark body, six legs, two south knees CYAN, threads
//        L37-fight-tell-<id> each attack's first tell at 70 % of its wind-up: lash (a red-orange line through the player's spot), spool
//                          (YELLOW at the body), web (a faint red-orange sheet between the towers ahead)
//        L37-fight-nailed  the first knee carrying its nails (warm, brighter)
//        L37-fight-tear    3 frames after the first tear: the knee gone, its lower leg hanging limp
//        L37-fight-window  0.5 s into the first CYAN window: the spinneret CYAN under the body
//        L37-fight-leg2 / -leg3  1 s after each leg change: the body moved north (leg 3: over the nest, tower pair 3)
//        L37-fight-cut     an anchor pulsing red-orange before it goes dark (forced on the start ledge's thread a0w after the first window if
//                          no cut has pulsed by then); L37-fight-cut-dark 1.2 s later: its ring and thread gone
//        L37-fight-kill    0.5 s after the kill: it falls into the canyon
//        L37-fall-back     a forced fall into the void after the kill: 5 frames later, back on the last ledge
//     Line: GREEN when the begin kept his feet (< 0.05 m), a tear, a window and the kill were seen, every tell's wind-up was 1.0-1.5 s, the view
//     drew the body; RED otherwise. The line carries the pilot's own counters (nails in, tears by pull / LT, pulls, weapon) and the puppet tears.
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
        static partial void N37LoomBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "loom37") body = Loom37Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator L37Shot(SurvivalGame g, Vec3 feet, Vec3 lookAt, string dir, string name, double[] st)
        {
            Vec3 eye = new Vec3(feet.x, feet.y + 1.6, feet.z), dv = lookAt - eye;
            double yaw = Math.Atan2(-dv.x, -dv.z), pitch = Math.Atan2(dv.y, Math.Sqrt(dv.x * dv.x + dv.z * dv.z));
            g.Respawn(feet, yaw * 180 / Math.PI);
            g.input.SetLook(yaw, pitch);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
            yield return Frames(20);
            yield return Shot29(dir, name, st);
        }

        /// <summary>A room-local point (lx east of the room's centre line, lz north of its south edge) from the level JSON's own room rows.</summary>
        static Vec3 L37RoomPoint(SurvivalSim sim, string room, double lx, double y, double lz)
        {
            if (sim.map?.json != null && sim.map.json.ContainsKey("campaign"))
                foreach (object ro in PortData.Arr(PortData.Obj(sim.map.json["campaign"])["rooms"]))
                {
                    var r = PortData.Obj(ro);
                    if (!r.ContainsKey("id") || (string)r["id"] != room) continue;
                    var rect = PortData.Arr(r["rect"]);
                    return new Vec3((PortData.Num(rect[0]) + PortData.Num(rect[2])) * 0.5 + lx, y, PortData.Num(rect[3]) - lz);
                }
            return new Vec3(lx, y, -lz);
        }

        static IEnumerator Loom37Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            // a CAMPAIGN run when the map is a campaign level (as C's crown37 and the level36 reels), DOORFREE
            bool campaign = N24CampaignGate(g) == null;
            if (campaign)
            {
                offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
                Quiet(g);
                g.noRecords = g.noLab = true;
                g.setup.mode = Campaign24.Mode;
                g.StartRun(Seed);
                yield return Frames(240); // the run-start title ("4.1 TOLLGATE", the level's first room) has run out before any frame
                for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
                DoorFree41(g); // NIGHT41 T2 (A9): DOORFREE + its queued room banners cleared (FeelReel.N41.cs)
                campaign = sim.campaignOn && sim.campaign != null;
            }
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            string pc = $"{(campaign ? "CAMPAIGN" : "survival")} run, map {g.MapId}, n37.loom37 {sim.n37.loom37}, level side {sim.Loom37Level}, view {(Loom37View.Current != null ? "on" : "MISSING (the lead's two lines in SurvivalGame.Boss26.cs)")}";
            if (!sim.Loom37Level)
            {
                res[0] = "RED: not THE LOOM's level (boot -skygrindMap n37-span with design.n37.loom37 on) | " + pc;
                yield break;
            }
            if (!sim.n40.loom40 && !sim.loadout.owned.Contains("nailgun")) sim.loadout.AddWeapon("nailgun"); // NIGHT40 B4: with loom40 the STITCHER is out of level 4 (the pilot hooks knees)
            Start(g, "loom37", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            SurvivalSim.Loom37Ledge LedgeOf(string id) { foreach (var l in sim.loom37Ledges) if (l.id == id) return l; return null; }
            // A. the space (entering the canyon begins the campaign's fight: held frozen for these frames)
            yield return L37Shot(g, L37RoomPoint(sim, "o1", 0, 0.05, 3), L37RoomPoint(sim, "o1", 2, 7, 22), dir, "L37-space-o1", st);
            yield return L37Shot(g, L37RoomPoint(sim, "o2", 0, 0.05, 3), L37RoomPoint(sim, "o2", 0, 9, 20), dir, "L37-space-o2", st);
            var start = LedgeOf("start");
            var t1 = LedgeOf("t1w");
            var t2 = LedgeOf("t2w");
            Vec3 startFeet = new Vec3(start.at.x, 0.05, start.at.z);
            g.Respawn(startFeet, 0);
            yield return Frames(60); // the campaign enters o3 (DOORFREE): its own title, "4.10 THE CANYON", is the one up
            string roomAtStart = sim.campaign?.Room?.id ?? "none";
            if (sim.loom37 != null && sim.loom37.Alive) sim.loom37.Frozen = true;
            yield return L37Shot(g, startFeet, new Vec3(start.at.x, 8, start.at.z - 40), dir, "L37-space-start", st);
            yield return L37Shot(g, new Vec3(t1.at.x, t1.y + 0.05, t1.z0 + 1), new Vec3(start.at.x, 10, t1.z0 - 30), dir, "L37-space-mid", st);
            yield return L37Shot(g, new Vec3(t2.at.x, t2.y + 0.05, t2.z0 + 1), new Vec3(start.at.x, 8, t2.z0 - 30), dir, "L37-space-nest", st);
            // B. the fight: a fresh begin on the start ledge
            g.Respawn(startFeet, 0);
            g.input.SetLook(0, 0);
            g.forcedMove = new InputFrame { yaw = 0, pitch = 0 };
            yield return Frames(30);
            Vec3 before = sim.mover.position;
            if (sim.loom37 != null)
            {
                sim.loom37.Frozen = false;
                sim.loom37.Clear();
                sim.bossKit?.Clear();
            }
            if (!sim.BossRoomBegin("loom37", out Vec3 spawn, out double yawDeg) || sim.loom37 == null)
            {
                yield return Stop();
                res[0] = "RED: BossRoomBegin(loom37) returned false | " + pc;
                yield break;
            }
            yield return null;
            double moved = Vec3.DistanceTo(before, sim.mover.position);
            Loom37 L = sim.loom37;
            var pilot = new Loom37Pilot();
            double t0 = sim.SimTime;
            var told = new HashSet<string>();
            var sweepLegs41 = new HashSet<int>(); // NIGHT41 B8 (sweep41 on): each leg's first sweep tell, and the line once
            bool sweepLine41 = false;
            var sweepForced41 = new HashSet<int>();
            int sweepForcedN41 = 0;
            bool reveal = false, nailShot = false, tearShot = false, windowShot = false, killShot = false, cutShot = false, cutDarkShot = false, leg2Shot = false, leg3Shot = false, bodySeen = false;
            int tearF = -1, frame = 0, leg2F = -1, leg3F = -1, lastLeg = 1, puppets = 0, maxNails = 0, lastSimStep = sim.simStep;
            double windowAt = -1, killAt = -1, legStartAt = 0, cutAt = -1;
            string forcedCut = null;
            int falls0 = sim.loom37Falls;
            double wallEnd = Time.realtimeSinceStartup + 150;
            while (Time.realtimeSinceStartup < wallEnd && sim.SimTime - t0 < 75)
            {
                God(g);
                frame++;
                double now = sim.SimTime - t0;
                if (L.Alive && !L.Falling)
                {
                    // the harness's pilot on the harness's clock: one pilot step per sim step that ran since the last frame (its counters and
                    // phases count steps), the frame's input = the last of them
                    int ran = Math.Max(1, sim.simStep - lastSimStep);
                    lastSimStep = sim.simStep;
                    InputFrame mv = default;
                    CombatInput cb = new CombatInput { weapon = -1 };
                    for (int k = 0; k < ran; k++)
                    {
                        pilot.Step(sim, sim.simStep - ran + 1 + k);
                        // a press decided on any of the frame's steps is kept (a Player press spans every sim step of its frame: L-053)
                        InputFrame m = pilot.Move;
                        m.jump.pressed |= mv.jump.pressed; m.dash.pressed |= mv.dash.pressed; m.grapple.pressed |= mv.grapple.pressed; m.sprint.pressed |= mv.sprint.pressed;
                        m.grapple.held |= mv.grapple.pressed;
                        CombatInput c = pilot.Combat;
                        c.fire.pressed |= cb.fire.pressed; c.special.pressed |= cb.special.pressed; c.punch.pressed |= cb.punch.pressed;
                        c.special.held |= cb.special.pressed; c.punch.held |= cb.punch.pressed;
                        if (c.weapon < 0) c.weapon = cb.weapon;
                        mv = m;
                        cb = c;
                    }
                    // NIGHT41 B8 (sweep41): for the sweep's frames the camera looks at the band's centre at its height (MEASURED n41-r3: the pilot's
                    // own look framed a safe tower top and the body, never the decal); only from 50 % of a tell still to frame, or the first pass
                    bool frameSweep = (L.SweepTelling && L.brain.Progress >= 0.5 && !sweepLegs41.Contains(L.brain.leg)) || (L.SweepLive && !sweepLine41);
                    if (frameSweep)
                    {
                        Vec3 eye = sim.mover.position + new Vec3(0, 1.6, 0);
                        Vec3 to = new Vec3((L.sweepFromX + L.sweepToX) * 0.5, L.sweepTopY - 1, (L.sweepZ0 + L.sweepZ1) * 0.5);
                        if (Vec3.DistanceTo(eye, to) < 4) to = new Vec3(L.sweepToX, L.sweepTopY - 1, (L.sweepZ0 + L.sweepZ1) * 0.5);
                        (mv.yaw, mv.pitch) = Loom37Pilot.LookAt(eye, to);
                    }
                    g.input.SetLook(mv.yaw, mv.pitch);
                    g.forcedMove = mv;
                    g.forcedCombat = cb;
                }
                else { g.forcedMove = new InputFrame { yaw = 0, pitch = -0.5 }; g.forcedCombat = IdleCombat; }
                bodySeen |= Loom37View.Current != null && Loom37View.Current.BodyShown;
                if (!reveal && now >= 0.8) { reveal = true; yield return Shot29(dir, "L37-fight-reveal", st); continue; }
                Boss37Brain b = L.brain;
                if (L.Alive && !L.Falling)
                {
                    foreach (var k in L.knees) if (!k.torn) maxNails = Math.Max(maxNails, k.nails);
                    // NIGHT41 B8 (sweep41, reel only): the game's pick is a roll and the 1.5 s gap + 2.0 s tell can leave a 36 s reel fight without
                    // a sweep (MEASURED n41-r5: none fired); so the reel queues one (Boss37Brain.Force, the brain's own staple queue) 1 s into legs
                    // 1 and 2 if that leg has shown none yet. The game never calls it.
                    if (L.sweep41 && b.leg <= 2 && b.Fighting && now - legStartAt >= 1.0 && !sweepLegs41.Contains(b.leg) && sweepForced41.Add(b.leg) && b.Force(Loom37.Sweep)) sweepForcedN41++;
                    // NIGHT41 B8 (sweep41): L41-sweep-tell-leg<N> at 70 % of each leg's first sweep wind-up (the red-orange decal on the ledges in its
                    // band, the top bar along the start wall), L41-sweep-line at 40 % of the first pass (the red-orange sheet crossing the canyon)
                    if (L.SweepTelling && b.Progress >= 0.7 && sweepLegs41.Add(b.leg)) { yield return Shot29(dir, "L41-sweep-tell-leg" + b.leg, st); continue; }
                    if (L.SweepLive && b.Progress >= 0.4 && !sweepLine41) { sweepLine41 = true; yield return Shot29(dir, "L41-sweep-line", st); continue; }
                    if (b.state == Boss37State.Tell && b.Current != null && b.Progress >= 0.7 && told.Add(b.Current.id)) { yield return Shot29(dir, "L37-fight-tell-" + b.Current.id, st); continue; }
                    bool nailed = false;
                    foreach (var k in L.knees) if (!k.torn && k.nails >= L.p.nailsToTear) nailed = true;
                    if (!nailShot && nailed) { nailShot = true; yield return Shot29(dir, "L37-fight-nailed", st); continue; }
                    if (L.tears > 0 && tearF < 0) tearF = frame;
                    if (!tearShot && tearF > 0 && frame - tearF >= 3) { tearShot = true; yield return Shot29(dir, "L37-fight-tear", st); continue; }
                    if (b.Open && windowAt < 0) windowAt = now;
                    if (!windowShot && windowAt >= 0 && now - windowAt >= 0.5) { windowShot = true; yield return Shot29(dir, "L37-fight-window", st); continue; }
                    if (b.leg != lastLeg) { if (b.leg == 2) leg2F = frame; if (b.leg == 3) leg3F = frame; lastLeg = b.leg; legStartAt = now; }
                    if (!leg2Shot && leg2F > 0 && frame - leg2F >= 60) { leg2Shot = true; yield return Shot29(dir, "L37-fight-leg2", st); continue; }
                    if (!leg3Shot && leg3F > 0 && frame - leg3F >= 60) { leg3Shot = true; yield return Shot29(dir, "L37-fight-leg3", st); continue; }
                    // the cut: a natural one, or forced on the start ledge's thread after the first window
                    if (!cutShot && forcedCut == null && windowShot && sim.Loom37CutNow("a0w")) forcedCut = "a0w";
                    bool pulsing = false;
                    foreach (var a in sim.loom37Anchors) if (sim.Loom37Pulse(a) > 0.4) pulsing = true;
                    if (!cutShot && pulsing) { cutShot = true; cutAt = now; yield return Shot29(dir, "L37-fight-cut", st); continue; }
                    if (cutShot && !cutDarkShot && now - cutAt >= 1.2) { cutDarkShot = true; yield return Shot29(dir, "L37-fight-cut-dark", st); continue; }
                    // the puppet: no window 7 s into a leg: one tear by the reel (nails in a live knee + the detonate)
                    if (b.Fighting && !b.Open && now - legStartAt > 7)
                    {
                        for (int i = 0; i < 6; i++)
                            if (L.ActiveKnee(i)) { L.knees[i].nails = L.p.nailsToTear; L.Detonated(); puppets++; break; }
                        legStartAt = now - 4; // the next puppet (if needed) 3 s later
                    }
                }
                if ((L.Falling || !L.Alive) && killAt < 0) killAt = now;
                if (!killShot && killAt >= 0 && now - killAt >= 0.5) { killShot = true; yield return Shot29(dir, "L37-fight-kill", st); break; }
                yield return null;
            }
            // the fall back to a ledge (forced after the fight)
            int falls = sim.loom37Falls - falls0;
            g.forcedCombat = IdleCombat;
            var last = sim.loom37LastLedge ?? start;
            g.Respawn(new Vec3(last.at.x + (last.id == "start" ? 0 : 6), last.y + 2, last.z0 - 6), 0);
            g.forcedMove = new InputFrame { yaw = 0, pitch = -0.3 };
            int fallF = 0, f0 = sim.loom37Falls;
            while (sim.loom37Falls == f0 && fallF < 400) { God(g); fallF++; yield return null; }
            yield return Frames(5);
            yield return Shot29(dir, "L37-fall-back", st);
            Vec3 back = sim.mover.position;
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            var sb = new System.Text.StringBuilder();
            bool tellsOk = told.Count > 0;
            foreach (var (a, steps) in L.brain.tellLog)
            {
                double len = steps * sim.step;
                // NIGHT41 B8: the sweep's tell is design.n41.sweep41.windup (2.0 s, MEASURED: the casual pilot needs it); its band is 0.9 s (boss_rules 1) to its own wind-up
                bool inBand = a == Loom37.Sweep ? len >= 0.89 && len <= (L.sp41 != null ? L.sp41.windup : 2.0) + 0.01 : len >= 0.99 && len <= 1.51;
                tellsOk &= inBand;
                if (!inBand || !sb.ToString().Contains(" " + a + " ")) sb.Append($" {a} {len.ToString("0.00", ci)} s{(inBand ? "" : " RED")};");
            }
            bool killed = killAt >= 0;
            bool ok = moved < 0.05 && L.tears > 0 && windowAt >= 0 && tellsOk && killed && bodySeen;
            res[0] = $"{(ok ? "GREEN" : "RED")}: begin moved the feet {moved.ToString("0.000", ci)} m; tears {L.tears} (pull {L.tears - L.ltTears}, LT {L.ltTears}; reel puppets {puppets}), first window at {(windowAt < 0 ? "never" : windowAt.ToString("0.0", ci) + " s")}, leg {L.brain.leg}, "
                + $"{(killed ? "killed at " + killAt.ToString("0.0", ci) + " s" : "NOT killed")}; tells{sb}; pilot: nails in {L.nailsIn} (max on a knee {maxNails}), loom40 {sim.n40.loom40} hooks latched {L.latches40}, pulls {pilot.fightPulls}, detonates {pilot.detonates}, weapon {sim.loadout.Weapon}, punches {pilot.punches}; "
                + $"parries {L.spoolParries}, lash hits {L.lashHits}, pilot falls {falls}, cuts {sim.loom37Cuts} (forced {forcedCut ?? "none"}), severs {sim.loom37Severs}; sweep41 {L.sweep41}: sweeps {L.sweeps} (reel-queued {sweepForcedN41}), hit {L.sweepHits}, frames legs [{string.Join(",", sweepLegs41)}] line {sweepLine41}; "
                + $"forced fall back at ({back.x.ToString("0.0", ci)}, {back.y.ToString("0.0", ci)}, {back.z.ToString("0.0", ci)}) on {sim.loom37LastLedge?.id}; body drawn {bodySeen}; campaign room at the canyon frames {roomAtStart} | {pc}";
        }
    }
}
