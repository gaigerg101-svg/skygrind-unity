// NIGHT43 builder M1 (NIGHT43.md M1; the dispatch is the lead's FeelReel.N43.cs): the Player scenario `move43`, chimney43's / ride43's /
// boost43's proof frames, BEFORE and AFTER in one run: the reel sets the Mover's NIGHT43 rules (Mover.SetMove43) and the Loadout's boost43 rules
// directly for each half (Neutral = NIGHT42's game; the rows' numbers = the rows on), whatever the config says, and puts the run's own rules back
// at the end. Presses are edges by SIM STEP (Edge35Input, L-053). Not run by its author (no Player in the worktree).
// Boot: a campaign level (reel.sh + -skygrindMap <level> + -skygrindReelOnly move43, L-062). The lead's pick: n43-foundry with foundry43 on (L1's
// FLUE: two facing walls ~4.5 m apart); any level with two facing walls 3-6.5 m apart in a room works (the rooms are scanned for one).
// Frames (shot-N30-<name>.png):
//   M43-chimney-off-p4 / -on-p4    the 4th press in the chimney (off: today the press is the double jump; on: a wall jump, still climbing)
//   M43-chimney-off-top / -on-top  1.5 s after the 4th press (off: falling / landed; on: higher up the walls)
//   M43-bdrop-off-000 / -150 / -300, M43-bdrop-on-...   a held ride at 12 m/s on a long wall (VerbsWall39's pick), released at 0.4 s, B (slide)
//                                  0.1 s later; +0 = the B press (off: a slam straight down; on: the drop, the speed kept)
//   M43-dash-off-000 / -on-000     a dash at 45 deg into that wall from 2.5 m, in the air: the frame 0.25 s after the dash press (on: riding)
//   M43-boost-off-apex / -on-apex  on SafeSpot's floor, the THUNDERCLAP in hand, looking 70 deg down: jump and fire pressed on the same frame;
//                                  the frame at the apex (the rise printed)
// Line: GREEN when every part found its place and every frame was saved; it prints each half's numbers (the chimney's top and wall jumps, the
// speed 0.3 s after B and what B did, the dash's state, the boost's rise).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using Btn = Skygrind.Core.Button;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N43Move(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Move43Scenario(g, dir, res, offs, pre);

        /// <summary>Two facing walls through o along +-d: the gap between their faces (NaN when not a pair), the -d side's hit point and normal.</summary>
        static double Move43Pair(SurvivalSim sim, Vec3 o, Vec3 d, out Vec3 pointA, out Vec3 normalA)
        {
            pointA = normalA = Vec3.Zero;
            if (!sim.world.RayIntersect(o, d, out RayHit hp) || !sim.world.RayIntersect(o, new Vec3(-d.x, 0, -d.z), out RayHit hm)) return double.NaN;
            if (Math.Abs(hp.normal.y) > 0.2 || Math.Abs(hm.normal.y) > 0.2) return double.NaN;
            if (hp.normal.x * hm.normal.x + hp.normal.z * hm.normal.z > -0.95) return double.NaN;
            pointA = o - d * hm.distance;
            double l = Math.Sqrt(hm.normal.x * hm.normal.x + hm.normal.z * hm.normal.z);
            normalA = new Vec3(hm.normal.x / l, 0, hm.normal.z / l);
            return hp.distance + hm.distance;
        }

        /// <summary>A chimney in the level's rooms: two facing walls 3-6.5 m apart at 2 m and 6 m over the room's floor (within 0.3 m), one
        /// 3.5-4.0 m apart first, else the closest to that band (NIGHT44 R2; was the closest to 4.5 m). at: wall A's face at 2 m; nA: its normal (toward wall B).</summary>
        static bool Move43Chimney(SurvivalSim sim, out Vec3 at, out Vec3 nA, out double gap, out string room)
        {
            at = nA = Vec3.Zero;
            gap = 0;
            room = null;
            double best = double.MaxValue;
            if (sim.campaignLevel == null) return false;
            foreach (RoomDef r in sim.campaignLevel.rooms)
                for (double x = r.x0 + 0.5; x <= r.x1 - 0.5; x += 0.5)
                    for (double z = r.z0 + 0.5; z <= r.z1 - 0.5; z += 0.5)
                        for (int axis = 0; axis < 2; axis++)
                        {
                            Vec3 d = axis == 0 ? new Vec3(1, 0, 0) : new Vec3(0, 0, 1);
                            double g1 = Move43Pair(sim, new Vec3(x, r.spawn.y + 2, z), d, out Vec3 pa, out Vec3 na);
                            if (double.IsNaN(g1) || g1 < 3 || g1 > 6.5) continue;
                            double g2 = Move43Pair(sim, new Vec3(x, r.spawn.y + 6, z), d, out _, out _);
                            if (double.IsNaN(g2) || Math.Abs(g2 - g1) > 0.3) continue;
                            // NIGHT44 R2 (T-66; SPIKE.md 2): prefer a 3.5-4.0 m gap (the harness's box is 4.0 m, THE FLUE is 3.5 m now); at
                            // 4.5 m a pressed wall jump's ~5 m/s fling falls across the gap (M1.md:26-29). Was |gap - 4.5|.
                            double score = (g1 < 3.5 ? 3.5 - g1 : g1 > 4.0 ? g1 - 4.0 : 0) + Math.Abs(g1 - 3.75) * 0.01;
                            // and a TALL one: from floor + 3 m four presses climb ~8 m (n44proof chim: in an 8 m box the climber leaves the top
                            // after 3 presses and the 4th never comes); walls still facing at floor + 12 m first
                            double g3 = Move43Pair(sim, new Vec3(x, r.spawn.y + 12, z), d, out _, out _);
                            if (double.IsNaN(g3) || Math.Abs(g3 - g1) > 0.3) score += 1;
                            if (score >= best) continue;
                            best = score;
                            gap = g1;
                            at = pa;
                            nA = na;
                            room = r.id;
                        }
            return best < double.MaxValue;
        }

        /// <summary>The stick (moveX, moveZ) that points along world direction d (horizontal) for a camera at yaw (0 looks -z).</summary>
        static (double mx, double mz) Move43Stick(Vec3 d, double yaw) =>
            (d.x * Math.Cos(yaw) - d.z * Math.Sin(yaw), -d.x * Math.Sin(yaw) - d.z * Math.Cos(yaw));

        static IEnumerator Move43Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var okc = new bool[1];
            yield return Campaign41(g, offs, true, okc);
            SurvivalSim sim = g.sim;
            Move43.State runMove = g.mover.m43;
            Boost43.Params runBoost = g.loadout.boost43;
            bool runBoostBit = sim.n43.boost43;
            offs.Add(() => { g.mover.SetMove43(runMove.p); g.loadout.boost43 = runBoost; sim.n43.boost43 = runBoostBit; g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + $" | map {g.MapId} | campaign {okc[0]} | the run's rows: chimney43 {runMove.p.chimney}, ride43 {runMove.p.ride}, boost43 {runBoost.on}";
            g.husks.KillAll();
            g.husks.frozen = true;
            IPlayerInput plainInput = sim.input;
            sim.UseInput(new Edge35Input(plainInput));
            offs.Add(() => { if (sim.input is Edge35Input ei) sim.UseInput(ei.Inner); });
            Start(g, "move43", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 150;
            var lines = new List<string>();
            int framesSaved = 0, partsOk = 0;
            double r = sim.cfg.movement.body.capsuleRadius;
            int wallJumps = 0, doubles = 0, rides = 0;
            var moveLog = new List<string>();
            Action<MoverEvent> onMove = e =>
            {
                if (moveLog.Count < 400) moveLog.Add(e.type + (e.reason != null ? ":" + e.reason : ""));
                if (e.type == MoverEventType.WallJump) wallJumps++;
                if (e.type == MoverEventType.DoubleJump) doubles++;
                if (e.type == MoverEventType.WallRideStart) rides++;
            };
            g.mover.Events += onMove;
            offs.Add(() => g.mover.Events -= onMove);
            Move43.Params on = Move43.Params.From(sim.cfg, true, true, true);

            // ---- chimney43: a press at each wall ----
            bool chimOk = Move43Chimney(sim, out Vec3 cA, out Vec3 cN, out double cGap, out string cRoom);
            if (!chimOk) lines.Add("M43-chimney: NO two facing walls 3-6.5 m apart in the level's rooms");
            for (int half = 0; half < 2 && chimOk; half++)
            {
                string tag = half == 0 ? "off" : "on";
                g.mover.SetMove43(half == 0 ? Move43.Params.Neutral : on);
                Vec3 along = new Vec3(-cN.z, 0, cN.x);
                double yaw = YawOf35(along);
                // NIGHT44 R2 (T-66): start at the room's floor + 3 m (cA is the wall's face at floor + 2 m; was cA.y - 0.5 = floor + 1.5 m)
                Vec3 start = new Vec3(cA.x + cN.x * (r + 0.02), cA.y + 1.0, cA.z + cN.z * (r + 0.02));
                g.Respawn(start, yaw * 180 / Math.PI);
                g.input.SetLook(yaw, 15 * Math.PI / 180);
                int wj0 = wallJumps, dj0 = doubles, presses = 0, s0 = sim.simStep, p4 = -1;
                Vec3 target = new Vec3(-cN.x, 0, -cN.z); // the first press is off wall A: the stick into it
                bool armed = true;
                double top = start.y, tapUntil = -1;
                string shotP4 = null, shotTop = null;
                var pl = new System.Text.StringBuilder();
                int ev0 = moveLog.Count;
                while (T35(g, s0) < 6 && Time.realtimeSinceStartup < wallEnd)
                {
                    God(g);
                    g.input.SetLook(yaw, 15 * Math.PI / 180);
                    Mover m = g.mover;
                    bool onTarget = (m.touchingWall || m.state == MoveState.WallRide) && -(m.wallNormal.x * target.x + m.wallNormal.z * target.z) > 0.7;
                    bool press = false;
                    if (!onTarget) armed = true;
                    else if (armed && !m.grounded)
                    {
                        press = true;
                        armed = false;
                        presses++;
                        tapUntil = T35(g, s0) + 0.03;
                        pl.Append($" p{presses}@{T35(g, s0).ToString("0.00", ci)}s y {(m.position.y - start.y).ToString("0.00", ci)} {m.state} wj {m.wallJumpsLeft}");
                        if (presses == 4) p4 = sim.simStep;
                    }
                    var (mx, mz) = Move43Stick(target, yaw);
                    g.forcedMove = new InputFrame { moveX = mx, moveZ = mz, jump = new Btn { pressed = press, held = press || T35(g, s0) < tapUntil } };
                    int wjBefore = wallJumps;
                    yield return null;
                    if (wallJumps > wjBefore) target = new Vec3(-target.x, 0, -target.z);
                    top = Math.Max(top, g.mover.position.y);
                    if (p4 >= 0 && shotP4 == null)
                    {
                        shotP4 = "M43-chimney-" + tag + "-p4";
                        yield return Shot30(dir, shotP4);
                        framesSaved++;
                    }
                    if (p4 >= 0 && shotTop == null && (sim.simStep - p4) * sim.step >= 1.5)
                    {
                        shotTop = "M43-chimney-" + tag + "-top";
                        yield return Shot30(dir, shotTop);
                        framesSaved++;
                        break;
                    }
                    if (g.mover.grounded && T35(g, s0) > 0.5 && p4 < 0) break;
                }
                g.forcedMove = new InputFrame();
                lines.Add($"M43-chimney-{tag}: room {cRoom}, gap {cGap.ToString("0.00", ci)} m, presses {presses}, WallJump {wallJumps - wj0}, DoubleJump {doubles - dj0}, top {(top - start.y).ToString("0.00", ci)} m over the start, frames {(shotP4 != null ? 1 : 0) + (shotTop != null ? 1 : 0)}/2");
                if (shotP4 != null && shotTop != null) partsOk++;
                lines.Add($"M43-chimney-{tag} presses:{pl} | events {string.Join(",", moveLog.GetRange(ev0, Math.Min(24, moveLog.Count - ev0)))} | end {g.mover.state} y {(g.mover.position.y - start.y).ToString("0.00", ci)} touching {g.mover.touchingWall}");
                yield return Wait27(g, 1.0, wallEnd, () => God(g));
            }

            // ---- ride43 (d): B 0.1 s after a ride; (g) a dash into the wall ----
            bool wallOk = Wall35(sim, out Vec3 wStart, out Vec3 wAlong, out Vec3 wNormal, out double wLen);
            if (!wallOk) lines.Add("M43-bdrop / dash: no straight wall of 8 m or more (VerbsWall39)");
            for (int half = 0; half < 2 && wallOk; half++)
            {
                string tag = half == 0 ? "off" : "on";
                g.mover.SetMove43(half == 0 ? Move43.Params.Neutral : on);
                double yaw = YawOf35(wAlong);
                Vec3 start = new Vec3(wStart.x + wNormal.x * (r + 0.02), wStart.y + 2.5, wStart.z + wNormal.z * (r + 0.02));
                g.Respawn(start, yaw * 180 / Math.PI);
                g.input.SetLook(yaw, 0);
                g.mover.velocity = new Vec3(wAlong.x * 12 - wNormal.x, 0, wAlong.z * 12 - wNormal.z);
                int s0 = sim.simStep, attach = -1, left = -1, bAt = -1;
                var sb = new System.Text.StringBuilder("M43-bdrop-" + tag + ":");
                var (ix, iz) = Move43Stick(new Vec3(-wNormal.x * 0.4 + wAlong.x, 0, -wNormal.z * 0.4 + wAlong.z), yaw);
                string was = "none";
                int shots = 0;
                while (T35(g, s0) < 3 && Time.realtimeSinceStartup < wallEnd)
                {
                    God(g);
                    g.input.SetLook(yaw, 0);
                    Mover m = g.mover;
                    if (attach < 0 && m.state == MoveState.WallRide) attach = sim.simStep;
                    if (attach < 0 && T35(g, s0) < 0.4) m.velocity = new Vec3(wAlong.x * 12 - wNormal.x, m.velocity.y, wAlong.z * 12 - wNormal.z);
                    bool held = attach < 0 || (sim.simStep - attach) * sim.step < 0.4;
                    if (left < 0 && attach >= 0 && !held && m.state != MoveState.WallRide) left = sim.simStep;
                    bool b = left >= 0 && bAt < 0 && (sim.simStep - left) * sim.step >= 0.1;
                    if (b) bAt = sim.simStep;
                    g.forcedMove = new InputFrame { moveX = left < 0 ? ix : 0, moveZ = left < 0 ? iz : 0, jump = new Btn { held = left < 0 && held }, slide = new Btn { pressed = b, held = b } };
                    yield return null;
                    if (b) was = g.mover.state == MoveState.Slam ? "slam" : "drop (" + g.mover.state + ")";
                    if (bAt >= 0)
                    {
                        double t = (sim.simStep - bAt) * sim.step;
                        double want = shots == 0 ? 0 : shots == 1 ? 0.15 : 0.30;
                        if (shots < 3 && t >= want)
                        {
                            string label = "M43-bdrop-" + tag + "-" + ((int)Math.Round(want * 1000)).ToString("000", ci);
                            sb.Append(' ').Append(label.Substring(label.Length - 3)).Append(" speed ").Append(g.mover.Speed.ToString("0.00", ci));
                            yield return Shot30(dir, label);
                            framesSaved++;
                            shots++;
                        }
                        if (shots >= 3) break;
                    }
                }
                g.forcedMove = new InputFrame();
                sb.Append($" | ride {(attach >= 0 ? "yes" : "NO")}, left {(left >= 0 ? "yes" : "NO")}, B was {was}, frames {shots}/3");
                lines.Add(sb.ToString());
                if (shots == 3) partsOk++;
                yield return Wait27(g, 1.0, wallEnd, () => God(g));

                // (g) the dash at 45 deg into the wall from 2.5 m, in the air
                Vec3 from = new Vec3(wStart.x + wNormal.x * (r + 2.5), wStart.y + 2.5, wStart.z + wNormal.z * (r + 2.5));
                g.Respawn(from, yaw * 180 / Math.PI);
                g.input.SetLook(yaw, 0);
                g.mover.velocity = Vec3.Zero;
                Vec3 dd = new Vec3(wAlong.x * 0.7071 - wNormal.x * 0.7071, 0, wAlong.z * 0.7071 - wNormal.z * 0.7071);
                var (dx, dz) = Move43Stick(dd, yaw);
                int d0 = sim.simStep;
                bool dashShot = false;
                while (T35(g, d0) < 1.0 && Time.realtimeSinceStartup < wallEnd)
                {
                    God(g);
                    g.input.SetLook(yaw, 0);
                    double t = T35(g, d0);
                    g.forcedMove = new InputFrame { moveX = dx, moveZ = dz, dash = new Btn { pressed = t < 0.01, held = t < 0.05 } };
                    yield return null;
                    if (!dashShot && T35(g, d0) >= 0.25)
                    {
                        string label = "M43-dash-" + tag + "-000";
                        lines.Add($"{label}: state {g.mover.state}, speed {g.mover.Speed.ToString("0.00", ci)}, dash attaches {g.mover.m43.dashAttaches}");
                        yield return Shot30(dir, label);
                        framesSaved++;
                        dashShot = true;
                        partsOk++;
                    }
                }
                g.forcedMove = new InputFrame();
                yield return Wait27(g, 0.8, wallEnd, () => God(g));
            }

            // ---- boost43: jump + shot on the same frame ----
            yield return Equip27(g, "shotgun"); // presses stay sim-step edges (Edge35Input): a repeated jump press was the double jump that erased the boost (n43-r2)
            g.forcedCombat = IdleCombat;
            int fires = 0;
            offs.Add(g.events.On<string>("fire", w => { if (w == "shotgun") fires++; }));
            for (int half = 0; half < 2; half++)
            {
                string tag = half == 0 ? "off" : "on";
                g.loadout.boost43 = half == 0 ? Boost43.Params.Neutral : Boost43.Params.From(sim.cfg, true);
                sim.n43.boost43 = half == 1;
                g.loadout.b43.Reset();
                g.Respawn(spot, spotYaw * 180 / Math.PI);
                double pitch = -70 * Math.PI / 180;
                g.input.SetLook(spotYaw, pitch);
                yield return Wait27(g, 1.2, wallEnd, () => { God(g); g.input.SetLook(spotYaw, pitch); });
                double y0 = g.mover.position.y, top = y0;
                int f0 = fires, s0 = sim.simStep;
                bool airborne = false, shot = false;
                while (T35(g, s0) < 3 && Time.realtimeSinceStartup < wallEnd)
                {
                    double t = T35(g, s0);
                    God(g);
                    g.input.SetLook(spotYaw, pitch);
                    g.forcedMove = new InputFrame { jump = new Btn { pressed = t < 0.01, held = g.mover.velocity.y > 0 || t < 0.05 } };
                    g.forcedCombat = new CombatInput { weapon = -1, fire = new Btn { pressed = t < 0.01, held = t < 0.05 } };
                    yield return null;
                    if (!g.mover.grounded) airborne = true;
                    top = Math.Max(top, g.mover.position.y);
                    if (airborne && !shot && g.mover.velocity.y <= 0 && T35(g, s0) > 0.1)
                    {
                        string label = "M43-boost-" + tag + "-apex";
                        lines.Add($"{label}: the rise {(top - y0).ToString("0.00", ci)} m, shots {fires - f0}, late pushes {g.loadout.b43.latePushes}, takeoff fires {g.loadout.b43.holdFires}, air boosts {g.loadout.b43.airBoosts}");
                        yield return Shot30(dir, label);
                        framesSaved++;
                        shot = true;
                        partsOk++;
                        break;
                    }
                }
                g.forcedMove = new InputFrame();
                g.forcedCombat = IdleCombat;
                if (!shot) lines.Add($"M43-boost-{tag}: NO APEX in 3 s (shots {fires - f0})");
                yield return Wait27(g, 1.5, wallEnd, () => God(g));
            }
            g.mover.SetMove43(runMove.p);
            g.loadout.boost43 = runBoost;
            sim.n43.boost43 = runBoostBit;
            yield return Stop();
            bool ok = partsOk == 8 && framesSaved == 14;
            res[0] = $"{(ok ? "GREEN" : "RED")} move43 | parts {partsOk}/8, frames {framesSaved}/14 || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
