// NIGHT26 builder B: the Player scenario `glacier`, driven (replaces the static-beats version in FeelReel.N26.Bosses2.cs, whose standing player was soft-failed every 3 s: the boss
// skates away at 7.5 m/s, the gap passes 80 m and the attack timer restarts, so no lane attack ever came). The player RUNS the course like the harness bot (Harness.N26.Glacier.cs):
// sprint forward on the ice, aim at the engine block, fire inside the hurt line, jump the sweeping bar, slide the low bar, cross to the spike wall's open lane. Game time, not
// frames, bounds every wait. The lane attacks seen (bossTell names) are printed; one player death in phase 2 shows the phase-start retry; the kill is real (pistol), with HP cuts only as a
// fallback after 170 s; then bossDown, the course drawn off, and in a campaign boot the LAYER CLEARED path. Marks: glacier:intro, glacier:attack, glacier:gap, glacier:death, glacier:finisher.
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static IEnumerator GlacierDriveScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            SurvivalSim sim = g.sim;
            bool campaign = N24CampaignGate(g) == null;
            if (campaign) N24StartCampaign(g, offs);
            var tells = new List<string>();
            int layerClears = 0, roomRetries = 0, softFails0 = 0;
            offs.Add(g.events.On<BossEvent>("bossTell", e => { if (e.boss == "glacier") tells.Add(e.name); }));
            offs.Add(g.events.On<RoomEvent>("layerClear", _ => layerClears++));
            offs.Add(g.events.On<RoomEvent>("roomRetry", _ => roomRetries++));
            yield return Frames(5);
            string pc = Preconditions(g, sim.glacier26Course == null ? default : sim.Glacier26Spawn);
            if (!sim.Glacier26On) { res[0] = pc + " | RED: design.n26 level2 is off (no GLACIER)"; yield break; }
            if (g.glacierView == null) { res[0] = pc + " | RED: no GlacierView (Boss26InitViews)"; yield break; }
            // a campaign boot starts the boss room itself (Campaign24 BeginBoss); otherwise start it here
            if (campaign && sim.campaignOn) for (int i = 0; i < Ms(4000) && (sim.glacier26 == null || !sim.glacier26.Alive); i++) yield return null;
            if (sim.glacier26 == null || !sim.glacier26.Alive)
            {
                if (!sim.BossRoomBegin("glacier", out Vec3 spawn0, out double yaw0)) { res[0] = pc + " | RED: BossRoomBegin(glacier) failed (the course file was not found, or no BossKit yet)"; yield break; }
                g.husks.frozen = true;
                g.Respawn(spawn0, yaw0);
            }
            Glacier gl = sim.glacier26;
            Vec3 o = sim.glacier26Origin;
            pc = Preconditions(g, g.Feet);
            var fails = new List<string>();
            var seen = new List<string>();
            g.health.max = 100;
            g.health.hp = 100;
            Start(g, "glacier", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            FeelProbe.Event("glacier:intro");
            double? laneX = null;
            int hits0 = g.events.Emitted.TryGetValue("bossHit", out int hh0) ? hh0 : 0, jumpHold = 0, slideHold = 0, lastLane = -100, f = 0;
            bool markedAttack = false, markedGap = false, deathDone = false, p2Seen = false;
            double minGap = 1e9, elapsed = 0, nextDiag = 20, cutAt = -1, gapSum = 0;
            int lastStep = gl.stepNo, gapN = 0, cuts = 0;
            string cutPhase = null;
            var diag = new List<string>();
            double dt = g.cfg.Dt;
            // bounded by GAME time (the boss's own steps): 120 s of the real fight, then HP cuts (labelled) and up to 90 s more of real fire to the kill
            for (int frame = 0; frame < Ms(600000) && gl.Alive && (cutAt < 0 || elapsed < cutAt + 90); frame++)
            {
                f++;
                if (gl.stepNo > lastStep) elapsed += (gl.stepNo - lastStep) * dt;
                lastStep = gl.stepNo;
                if (gl.phase.StartsWith("p")) { gapSum += gl.gap; gapN++; }
                if (elapsed >= nextDiag)
                {
                    nextDiag += 20;
                    Vec3 vv = g.sim.mover.velocity;
                    diag.Add($"t {elapsed:0}s HP {gl.HpFrac:0.00} {gl.phase} gap {gl.gap:0} (mean {gapSum / Math.Max(1, gapN):0}) speed {Math.Sqrt(vv.x * vv.x + vv.z * vv.z):0.0} fric x{g.sim.mover.groundFrictionMult:0.00} hits {((g.events.Emitted.TryGetValue("bossHit", out int hq) ? hq : 0) - hits0)}");
                }
                Vec3 me = g.Feet;
                Vec3 aim = gl.boxes[1].c;
                double dx = aim.x - me.x, dy = aim.y - (me.y + 1.6), dz = aim.z - me.z;
                g.input.SetLook(Math.Atan2(-dx, -dz), Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz)));
                g.forcedCombat = gl.gap <= gl.p.gapHurt + 1 && gl.phase != "intro" ? Fire : (CombatInput?)null;
                double targetX = laneX ?? o.x;
                bool jump = false, slide = false, nearBar = false;
                double vz = Math.Max(2, -g.sim.mover.velocity.z);
                foreach (GlacierObstacle ob in gl.obstacles)
                {
                    double dist = me.z - ob.z;
                    if (ob.kind != "spikes" ? dist > -3 && dist < 9 : dist > -3 && dist < 3) nearBar = true;
                    if (dist < -2.5) continue;
                    if (ob.kind == "spikes")
                    {
                        var (mn, mx) = gl.OpenRange;
                        targetX = (mn + mx) * 0.5;
                        if (ob.tellLeft < ob.total - 20) laneX = targetX;
                    }
                    else if (ob.kind == "sweep" && ob.tellLeft <= 0 && dist < vz * 0.2 + 0.3 && dist > vz * 0.1 && g.sim.mover.grounded) jump = true;
                    else if (ob.kind == "low" && ob.tellLeft <= 0 && dist < Math.Max(5, vz * 0.6) && dist > -1.5) slide = true;
                }
                if (!jump && !nearBar && g.sim.mover.grounded && Math.Abs(targetX - me.x) > 3.2 && f - lastLane > 50) { jump = true; lastLane = f; }
                if (jump) jumpHold = 14;
                if (slide) slideHold = 24;
                double mx2 = Math.Max(-1, Math.Min(1, (targetX - me.x) * 0.8 - g.sim.mover.velocity.x * 0.25));
                var inp = new InputFrame { moveZ = 1, moveX = mx2, sprint = new Skygrind.Core.Button { held = true, pressed = f % 60 == 0 } };
                if (jumpHold > 0) { inp.jump = new Skygrind.Core.Button { held = true, pressed = jumpHold == 14 }; jumpHold--; }
                if (slideHold > 0) { inp.slide = new Skygrind.Core.Button { held = true, pressed = slideHold == 24 }; slideHold--; }
                g.forcedMove = inp;
                if (gl.phase.StartsWith("p")) minGap = Math.Min(minGap, gl.gap);
                if (!markedAttack && tells.Count > 0) { markedAttack = true; FeelProbe.Event("glacier:attack"); }
                if (!markedGap && gl.Hurtable) { markedGap = true; FeelProbe.Event("glacier:gap"); seen.Add($"gap {gl.gap:0} m, hurtable (player {gl.playerS:0} m along the course, boss {gl.BossS:0} m)"); }
                if (gl.phase == "p2") p2Seen = true;
                // one player death in phase 2: the phase-start retry (a campaign boot goes through Campaign24.Retry; else the room's begin is called again)
                if (!deathDone && gl.phase == "p2" && gl.phaseSteps > 120 * 3)
                {
                    deathDone = true;
                    FeelProbe.Event("glacier:death");
                    double before = gl.HpFrac;
                    int rr0 = roomRetries, rt0 = sim.glacier26Retries;
                    g.health.hp = 1;
                    g.health.Damage(1e6, "boss");
                    for (int k = 0; k < Ms(8000) && roomRetries == rr0 && g.flow != Flow.Playing; k++) yield return null;
                    for (int k = 0; k < Ms(3000) && roomRetries == rr0; k++) yield return null;
                    if (roomRetries == rr0)
                    {
                        // not a campaign boot: what Campaign24.Retry does
                        g.health.Reset();
                        sim.downSteps = 0;
                        sim.flow = Skygrind.Game.Flow.Playing;
                        sim.BossRoomBegin("glacier", out Vec3 sp, out double yw);
                        g.Respawn(sp, yw);
                    }
                    g.health.max = 100;
                    g.health.hp = 100;
                    seen.Add($"death in p2: boss HP {before:0.00} -> {gl.HpFrac:0.00} (threshold {gl.p.phase2At:0.00}), retries {sim.glacier26Retries - rt0}, room retries {roomRetries - rr0}");
                    if (sim.glacier26Retries - rt0 < 1) fails.Add("no phase-start retry counted");
                }
                // the fallback (labelled): after 120 s of the real fight each phase is cut to 5 % HP once (the p2 death / retry first), then the real gun finishes it
                if (elapsed > 120 && cutAt < 0) { cutAt = elapsed; seen.Add($"HP cuts from t {elapsed:0} s (real fight: HP {gl.HpFrac:0.00}, mean gap {gapSum / Math.Max(1, gapN):0} m)"); }
                if (cutAt >= 0 && gl.phase != cutPhase && (gl.phase == "p1" || (gl.phase == "p2" && deathDone) || gl.phase == "p3") && gl.HpFrac > 0.06)
                {
                    cutPhase = gl.phase;
                    cuts++;
                    gl.body.hp = gl.body.maxHp * 0.05;
                    gl.Apply(0);
                }
                if (deathDone && cutPhase == "p2" && gl.phase == "p2" && gl.HpFrac > 0.6) cutPhase = null; // after the retry the HP is the threshold again: cut again
                yield return null;
            }
            g.forcedMove = null;
            g.forcedCombat = null;
            int realHits = (g.events.Emitted.TryGetValue("bossHit", out int hh1) ? hh1 : 0) - hits0;
            bool down = g.events.Emitted.ContainsKey("bossDown");
            FeelProbe.Event("glacier:finisher");
            seen.Add("every 20 s: " + string.Join(" | ", diag));
            seen.Add($"HP cuts {cuts}");
            if (gl.Alive) fails.Add($"the glacier did not die (phase {gl.phase}, HP {gl.HpFrac:0.00})");
            if (!down) fails.Add("no bossDown event");
            if (tells.Count == 0) fails.Add("no lane attack seen");
            if (!p2Seen) fails.Add("phase 2 never reached");
            yield return Frames(Ms(800));
            bool courseGone = !g.glacierView.Shown;
            if (!courseGone) fails.Add("the course is still drawn after the kill");
            string adv = "not a campaign boot";
            if (campaign && sim.campaignOn)
            {
                Campaign24 cb = sim.campaign;
                for (int k = 0; k < Ms(10000) && !(cb != null && cb.finished) && g.flow != Flow.Results; k++) yield return null;
                bool done = (cb != null && cb.finished) || g.flow == Flow.Results;
                adv = $"campaign {(cb != null && cb.finished ? "LAYER CLEARED" : "not finished")}, flow {g.flow}, layerClear x{layerClears}, room retries {roomRetries}";
                if (!done) fails.Add("the campaign did not advance after bossDown");
            }
            var kinds = new SortedDictionary<string, int>();
            foreach (string t in tells) kinds[t] = kinds.TryGetValue(t, out int c) ? c + 1 : 1;
            var kl = new List<string>();
            foreach (var kv in kinds) kl.Add(kv.Key + " x" + kv.Value);
            yield return Stop();
            res[0] = $"{FeelProbe.FrameCount} frames; {pc}; design.n26 level2 ON; lane attacks seen: {string.Join(", ", kl)} ({tells.Count}); real pistol hits {realHits}; closest gap {minGap:0} m; soft fails {gl.stats["softFails"] - softFails0}; {string.Join("; ", seen)}; {adv}; course drawn after: {(courseGone ? "no" : "YES")} | "
                + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
