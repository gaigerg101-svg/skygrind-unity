// NIGHT54 builder M (row M, move54): the a354 phase (FeelReel.N54.cs routes it here): design.n54.move54's Player proof (the camera's
// smoothness, the view only). Not run by its author (builders never run a Player). A campaign level (L-062), e.g.:
//   bash Tools/night/n47-reels.sh <tag> mv1,n52-foundry,a354                    (the build copy's config with design.n54.move54 on: GREEN)
//   bash Tools/night/n47-reels.sh <tag> mv1off,n52-foundry,a354,-skygrindOff54  (the row off, NIGHT53's camera: RED)
// (n52-iceline the same.) The CAMPAIGN on the booted map, DOORFREE, God, enemies frozen and cleared, the player's own input back (nothing
// pressed but the reel's), the KESTREL in hand. The run line: from a non-boss room's spawn, Move54ReelHeadings headings scanned for
// Move54ReelRun m of floor (a down ray every Move54ReelScanStep m; no ledge, clear at 0.6 and 1.4 m); a line with a step (a floor change of
// Move54ReelStepMin-StepMax m between two flat samples) first, a drop before a rise; else the longest clear line (the step "not crossed").
//   1. OUT: a run along the line, Move54ReelHusks husks (1 hp, held in place) beside it, shot with the KESTREL while running: the kills freeze
//      the loop (the impact layer's kill tiers).
//   2. BACK: the run back, no shots (the step the other way).
// Each rendered frame: the camera's position (Camera.main, after the render half), the feet, the feet's horizontal speed (the sim's
// velocity), HeldThisFrame and the world's scale. It prints the input first (L-089: metres run, kills, freezes and their causes, the step
// crossed), then: frames, HOLD frames (moving and the camera moved under Move54ReelHoldShare of the expected distance) and their seconds per
// freeze and over NIGHT53's freeze time for the same freezes (the game's own config: the gun kill tiers' old lengths), the step's largest
// one-frame camera height change against the feet's, and the jitter (std / mean of the camera's per-frame distance over the expected, moving
// frames). GREEN: the row on, it moved, a kill froze, the OUT leg's held time <= Move54ReelHoldRatio x NIGHT53's, and (the step crossed)
// camera / feet <= Move54ReelVyRatio. One line: "GREEN a354 MOVE | ..." (or RED), the full one in the log as "[a354] ...".
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
        static partial void N54Move(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Move54Scenario(g, dir, res, offs, pre);

        /// <summary>The floor under (x, z) from `top` down: its height and whether it is flat (false: no floor within 4 m).</summary>
        static bool Move54Floor(GameWorld world, double x, double z, double top, out double y, out bool flat)
        {
            y = 0;
            flat = false;
            if (!world.RayIntersect(new Vec3(x, top, z), new Vec3(0, -1, 0), out RayHit h) || h.distance > 4) return false;
            y = top - h.distance;
            flat = h.normal.y >= UnityTuning.Move54ReelFlat;
            return true;
        }

        /// <summary>A heading's run line from `f0` along `u`: its clear length, and the first step on it (at m, signed rise; 0: none).</summary>
        static double Move54Line(GameWorld world, Vec3 f0, Vec3 u, out double stepAt, out double stepRise)
        {
            stepAt = stepRise = 0;
            double run = UnityTuning.Move54ReelRun;
            foreach (double hy in new[] { 0.6, 1.4 })
                if (world.RayIntersect(new Vec3(f0.x, f0.y + hy, f0.z), u, out RayHit block) && block.distance < run) run = block.distance - 0.6;
            double top = f0.y + 2, prevY = f0.y;
            bool prevFlat = true;
            double clear = 0;
            for (double d = UnityTuning.Move54ReelScanStep; d <= run; d += UnityTuning.Move54ReelScanStep)
            {
                if (!Move54Floor(world, f0.x + u.x * d, f0.z + u.z * d, top, out double y, out bool flat)) break;
                double rise = y - prevY;
                if (Math.Abs(rise) > UnityTuning.Move54ReelLedge) break;
                if (stepRise == 0 && flat && prevFlat && Math.Abs(rise) >= UnityTuning.Move54ReelStepMin && Math.Abs(rise) <= UnityTuning.Move54ReelStepMax
                    && d >= UnityTuning.Move54ReelStepFrom) { stepAt = d; stepRise = rise; }
                prevY = y;
                prevFlat = flat;
                top = y + 2;
                clear = d;
            }
            if (stepRise != 0 && stepAt > clear - UnityTuning.Move54ReelStepBefore) stepAt = stepRise = 0; // too near the line's end to cross at a run
            return clear;
        }

        static IEnumerator Move54Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            var spawned = new List<Husk>();
            int kills = 0, freezes = 0, skipped = 0;
            double freezeSum = 0, base53Sum = 0;
            var causes = new Dictionary<string, int>();
            // NIGHT53's freeze for each granted freeze (the game's own config, never the row's view): the gun kill tiers' old lengths, the rest as granted
            string killName = ImpactRules.TierNames[(int)ImpactTier.Kill], hsName = ImpactRules.TierNames[(int)ImpactTier.HeadshotKill];
            bool on42 = sim.n42.impact42 && g.cfg.Has("design.n42.impact42.killS");
            double base53Kill = on42 ? g.cfg.Num("design.n42.impact42.killS") : g.cfg.Num("design.impact.freeze.kill");
            double base53Hs = on42 ? g.cfg.Num("design.n42.impact42.headshotKillS") : g.cfg.Num("design.impact.freeze.headshotKill");
            offs.Add(g.events.On<KillEvent>("kill", e => { if (e.owner == "player") kills++; }));
            offs.Add(g.events.On<ImpactFreezeEvent>(FeelKeys.ImpactFreeze, e =>
            {
                if (e.skipped) { skipped++; return; }
                freezes++;
                freezeSum += e.seconds;
                base53Sum += e.cause == killName ? Math.Max(e.seconds, base53Kill) : e.cause == hsName ? Math.Max(e.seconds, base53Hs) : e.seconds;
                causes[e.cause] = causes.TryGetValue(e.cause, out int c) ? c + 1 : 1;
            }));
            offs.Add(() =>
            {
                foreach (Husk h in spawned) if (h != null) h.alive = false;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
                g.forcedMove = null;
            });
            if (!sim.campaignOn) { res[0] = "RED a354 MOVE: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 240;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 220) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput);
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            g.husks.frozen = true;
            sim.husks.KillAll();
            bool on = Move54.On(g.cfg);
            string map = PostFx.LevelIdOf(g.map.json);

            // ---- the run line ----
            LevelDef lv = sim.campaignLevel;
            var rooms = new List<RoomDef>();
            if (lv != null)
            {
                if (sim.campaign != null && sim.campaign.room >= 0 && sim.campaign.room < lv.rooms.Count) rooms.Add(lv.rooms[sim.campaign.room]);
                foreach (RoomDef r in lv.rooms) if (r.kind != "boss" && !rooms.Contains(r)) rooms.Add(r);
            }
            RoomDef best = null;
            Vec3 f0 = g.Feet, u = new Vec3(0, 0, -1);
            double bestLen = -1, bestAt = 0, bestRise = 0;
            int bestRank = -1; // 2 a drop, 1 a rise, 0 no step
            foreach (RoomDef r in rooms)
            {
                Vec3 s0 = r.spawn;
                if (!Move54Floor(g.world, s0.x, s0.z, s0.y + 1.5, out double sy, out _)) continue;
                s0 = new Vec3(s0.x, sy, s0.z);
                for (int k = 0; k < UnityTuning.Move54ReelHeadings; k++)
                {
                    double a = 2 * Math.PI * k / UnityTuning.Move54ReelHeadings;
                    var uk = new Vec3(Math.Sin(a), 0, Math.Cos(a));
                    double len = Move54Line(g.world, s0, uk, out double at, out double rise);
                    if (len < 8) continue;
                    int rank = rise < 0 ? 2 : rise > 0 ? 1 : 0;
                    if (rank > bestRank || (rank == bestRank && len > bestLen))
                    {
                        best = r; f0 = s0; u = uk; bestLen = len; bestAt = at; bestRise = rise; bestRank = rank;
                    }
                }
                if (bestRank == 2) break;
            }
            if (best == null) { res[0] = $"RED a354 MOVE: no clear run line of 8 m from any room's spawn on {map}"; yield break; }
            string F2(double v) => v.ToString("0.00", ci);
            string F3(double v) => v.ToString("0.000", ci);
            string lineWord = $"line room {best.id} from ({f0.x.ToString("0.0", ci)}, {F2(f0.y)}, {f0.z.ToString("0.0", ci)}) along ({F2(u.x)}, {F2(u.z)}), {bestLen.ToString("0.0", ci)} m, "
                + (bestRank == 0 ? "no step on it" : $"step {(bestRise < 0 ? "down" : "up")} {F2(Math.Abs(bestRise))} m at {bestAt.ToString("0.0", ci)} m");
            yield return Equip27(g, "revolver"); // the KESTREL
            g.forcedCombat = IdleCombat;
            string pc = Preconditions(g, f0);
            Start(g, "a354-move", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();

            // ---- the samples (each rendered frame, after the render half) ----
            int frames = 0, moving = 0, holds = 0, heldFrames = 0;
            double holdS = 0, camPath = 0, feetPath = 0, sumQ = 0, sumQ2 = 0, maxCamVy = 0, maxFeetVy = 0;
            Vector3 lastCam = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            Vec3 lastFeet = g.Feet;
            bool lastGrounded = sim.mover.grounded;
            void Sample()
            {
                Vector3 cp = Camera.main != null ? Camera.main.transform.position : lastCam;
                Vec3 fp = g.Feet;
                double dt = Math.Max(1e-6, g.FrameDt);
                Vector3 dc = cp - lastCam;
                double camD = dc.magnitude, camVy = Math.Abs(dc.y), feetVy = Math.Abs(fp.y - lastFeet.y);
                double vx = sim.mover.velocity.x, vz = sim.mover.velocity.z, speed = Math.Sqrt(vx * vx + vz * vz);
                bool grounded = sim.mover.grounded;
                frames++;
                camPath += camD;
                feetPath += Math.Sqrt((fp.x - lastFeet.x) * (fp.x - lastFeet.x) + (fp.z - lastFeet.z) * (fp.z - lastFeet.z));
                if (g.HeldThisFrame) heldFrames++;
                if (speed >= UnityTuning.Move54ReelMoving)
                {
                    moving++;
                    double expected = speed * dt * (g.HeldThisFrame ? 1 : g.WorldScale);
                    double q = expected > 1e-9 ? camD / expected : 1;
                    sumQ += q;
                    sumQ2 += q * q;
                    if (g.HeldThisFrame || camD < UnityTuning.Move54ReelHoldShare * expected) { holds++; holdS += dt; }
                }
                if (grounded && lastGrounded)
                {
                    if (camVy > maxCamVy) maxCamVy = camVy;
                    if (feetVy > maxFeetVy) { maxFeetVy = feetVy; }
                }
                lastCam = cp;
                lastFeet = fp;
                lastGrounded = grounded;
            }
            // the camera's largest height change within a few frames of the step (the spring spreads it): the max over the whole run (grounded)
            // is printed as maxCamVy; the step's own feet change as maxFeetVy

            // ---- 1. OUT (the kills) ----
            double yawU = Math.Atan2(-u.x, -u.z);
            var n = new Vec3(u.z, 0, -u.x);
            g.Respawn(new Vec3(f0.x, f0.y + 0.05, f0.z), yawU * 180 / Math.PI);
            for (int i = 0; i < 40; i++) { God(g); g.forcedMove = new InputFrame { yaw = yawU }; g.input.SetLook(yawU, 0); yield return null; }
            var spots = new List<Vec3>();
            for (int k = 0; k < UnityTuning.Move54ReelHusks; k++)
            {
                double d = UnityTuning.Move54ReelHuskFrom + UnityTuning.Move54ReelHuskEvery * k;
                if (d > bestLen) break;
                double side = (k % 2 == 0 ? 1 : -1) * UnityTuning.Move54ReelHuskSide;
                double sx = f0.x + u.x * d + n.x * side, sz = f0.z + u.z * d + n.z * side;
                double sy = Move54Floor(g.world, sx, sz, f0.y + 2, out double fy, out _) ? fy : f0.y;
                var spot = new Vec3(sx, sy, sz);
                Husk h = null;
                try { h = sim.husks.Spawn("husk", spot, f0); } catch (Exception) { }
                if (h == null) continue;
                spawned.Add(h);
                spots.Add(spot);
            }
            for (int i = 0; i < 60; i++) // their spawn tells out (a153 / a253 wait 45-60)
            {
                for (int k = 0; k < spawned.Count; k++) { Husk h = spawned[k]; h.position = h.previous = spots[k]; h.hp = h.maxHp = 1; h.alive = true; }
                God(g);
                g.forcedMove = new InputFrame { yaw = yawU };
                g.input.SetLook(yawU, 0);
                yield return null;
            }
            lastCam = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            lastFeet = g.Feet;
            lastGrounded = sim.mover.grounded;
            int shots = 0, lastPress = -100, kills0 = kills, freezes0 = freezes;
            double base0 = base53Sum;
            double startDist = 0;
            for (int i = 0; i < UnityTuning.Move54ReelLegFrames; i++)
            {
                Vec3 fp = g.Feet;
                double along = (fp.x - f0.x) * u.x + (fp.z - f0.z) * u.z;
                if (i == 0) startDist = along;
                if (along >= bestLen - 1) break;
                Husk target = null;
                for (int k = 0; k < spawned.Count; k++)
                {
                    Husk h = spawned[k];
                    if (!h.alive) continue;
                    h.position = h.previous = spots[k];
                    h.hp = Math.Min(h.hp, 1);
                    double ahead = (spots[k].x - fp.x) * u.x + (spots[k].z - fp.z) * u.z;
                    if (ahead < 0.5) { h.alive = false; continue; } // passed: out of the line silently (as KillAll)
                    if (target == null) target = h;
                }
                God(g);
                double yaw = yawU, pitch = 0;
                if (target != null) RideKill53Look(sim.Eye, target.AimPoint, out yaw, out pitch);
                var mv = new InputFrame { yaw = yaw, pitch = pitch, moveZ = 1 };
                RideKill53Stick(yaw, u, out mv.moveX, out mv.moveZ);
                g.input.SetLook(yaw, pitch);
                g.forcedMove = mv;
                bool press = target != null && i - lastPress >= UnityTuning.Move54ReelFireEvery && RideKill53Wall.LineClear(g.world, sim.Eye, target.AimPoint);
                if (press) { lastPress = i; shots++; }
                g.forcedCombat = press ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } } : IdleCombat;
                yield return null;
                Sample();
            }
            g.forcedCombat = IdleCombat;
            int outFrames = frames, outHolds = holds, outKills = kills - kills0, outFreezes = freezes - freezes0;
            double outHoldS = holdS, outBase53 = base53Sum - base0;

            // ---- 2. BACK (no shots) ----
            double yawB = yawU + Math.PI;
            var back = new Vec3(-u.x, 0, -u.z);
            for (int i = 0; i < UnityTuning.Move54ReelLegFrames; i++)
            {
                Vec3 fp = g.Feet;
                double along = (fp.x - f0.x) * u.x + (fp.z - f0.z) * u.z;
                if (along <= 0.5 && i > 10) break;
                God(g);
                var mv = new InputFrame { yaw = yawB, moveZ = 1 };
                RideKill53Stick(yawB, back, out mv.moveX, out mv.moveZ);
                g.input.SetLook(yawB, 0);
                g.forcedMove = mv;
                yield return null;
                Sample();
            }
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            yield return Frames(5);
            yield return Stop();

            // ---- the numbers ----
            double mean = moving > 0 ? sumQ / moving : 0;
            double sd = moving > 0 ? Math.Sqrt(Math.Max(0, sumQ2 / moving - mean * mean)) : 0;
            double jitter = mean > 1e-9 ? sd / mean : 0;
            double holdPerFreeze = outFreezes > 0 ? outHoldS / outFreezes : double.NaN;
            bool crossed = bestRank > 0 && maxFeetVy >= UnityTuning.Move54ReelStepMin * 0.8;
            double vyRatio = maxFeetVy > 1e-6 ? maxCamVy / maxFeetVy : 0;
            bool moved = feetPath >= 8;
            bool froze = outKills > 0 && outFreezes > 0;
            double holdRatio = outBase53 > 1e-9 ? outHoldS / outBase53 : double.NaN; // the camera's held time over NIGHT53's freezes for the same moments
            bool holdOk = froze && holdRatio <= UnityTuning.Move54ReelHoldRatio;
            bool stepOk = !crossed || vyRatio <= UnityTuning.Move54ReelVyRatio;
            var cw = new System.Text.StringBuilder();
            foreach (var kv in causes) cw.Append(cw.Length > 0 ? ", " : "").Append(kv.Key).Append(' ').Append(kv.Value);
            Move54Step st = g.Move54Steps;
            string input = $"input: ran {feetPath.ToString("0.0", ci)} m (camera {camPath.ToString("0.0", ci)} m), shots {shots}, kills {outKills}, freezes {outFreezes} ({(cw.Length > 0 ? cw.ToString() : "none")}; skipped {skipped}; {F3(freezeSum)} s granted in all), "
                + $"step {(crossed ? $"crossed (feet {F3(maxFeetVy)} m in one frame)" : bestRank > 0 ? "NOT CROSSED" : "none on the line (not checked)")}, move54 steps seen {(st != null ? st.StepsSeen.ToString(ci) : "n/a (row off)")}";
            string nums = $"frames {frames} (out {outFrames}, moving {moving}, frozen {heldFrames}), HOLD frames {holds} (out {outHolds}: {F3(outHoldS)} s, {(double.IsNaN(holdPerFreeze) ? "n/a" : F3(holdPerFreeze))} s per freeze; x{(double.IsNaN(holdRatio) ? "n/a" : F2(holdRatio))} of NIGHT53's {F3(outBase53)} s for the same freezes, want <= x{F2(UnityTuning.Move54ReelHoldRatio)}), "
                + $"step: camera max {F3(maxCamVy)} m in one frame vs the feet {F3(maxFeetVy)} (x{F2(vyRatio)}, want <= x{F2(UnityTuning.Move54ReelVyRatio)}), jitter {F3(jitter)} (std / mean of camera / expected, mean {F3(mean)})";
            bool ok = on && moved && holdOk && stepOk;
            Debug.Log($"[a354] move54 on {on} (impact paths {Move54.ImpactPathsSet}); map {map}; {lineWord}; {input}; {nums}; {pc}");
            res[0] = $"{(ok ? "GREEN" : "RED")} a354 MOVE | move54 on {on} (map {map}) | {input} | {nums} | moved {(moved ? "yes" : "NO")}, hold {(holdOk ? "ok" : "NO")}, step {(stepOk ? (crossed ? "ok" : "not checked") : "NO")} | {lineWord} | {pc}";
        }
    }
}
