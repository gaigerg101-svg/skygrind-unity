// NIGHT42 builder M1 (NIGHT42.md M1; the dispatch is the lead's FeelReel.N42.cs): the Player scenario `wall42`, G2's and G5's proof frames,
// BEFORE and AFTER in one run: the reel sets the Mover's wallfling42 rules (Mover.SetWall42) and the Loadout's boost42 rules directly for each
// half (Neutral = NIGHT41's game; the row's numbers = the row on), whatever the config says, and puts the run's own rules back at the end. Not
// run by its author (no Player in the worktree).
// Boot: any campaign level (reel.sh + -skygrindMap <level> + -skygrindReelOnly wall42, L-062); the lead's pick: n41-kiln (level 3, the boost's
// level). The wall: from SafeSpot, rays at chest height in 16 headings: the nearest wall face (|normal.y| small) 1.5-12 m away; the player is
// placed standing 0.05 m off it (the capsule touching it once: a step pushing into it), looking along it.
// Frames (shot-N30-<name>.png), GAME time after the SECOND press (+0 = that frame):
//   W42-wall-off-000 / -150 / -300   the row OFF: standing against the wall, the stick centred: a jump (2 frames), then a second press 0.25 s
//                                    later: today a wall jump flings the player 8 m/s off the wall (the view swings away from it)
//   W42-wall-on-000 / -150 / -300    the row ON, the same presses: the double jump (straight up, beside the wall)
//   W42-boost-off-000 / -150 / -300  the row OFF: on SafeSpot's floor, the THUNDERCLAP in hand, a jump held 0.2 s, then the trigger 0.1 s after
//                                    the takeoff, looking 45 deg down: +0 = the frame the "fire" event was seen
//   W42-boost-on-000 / -150 / -300   boost42 ON, the same
// Line: GREEN when each half's presses happened and every frame was saved; it prints, per half, the presses' kinds (WallJump / DoubleJump
// events), the most m/s out from the wall in the 0.30 s after the second press, and the boost's highest rise over the takeoff.
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
        static partial void N42Wall(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Wall42Scenario(g, dir, res, offs, pre);

        /// <summary>The nearest wall face from `from` (chest height) in 16 headings, 1.5..12 m away: the hit point and its horizontal normal.</summary>
        static bool Wall42Find(SurvivalGame g, Vec3 from, out Vec3 point, out Vec3 normal)
        {
            point = normal = Vec3.Zero;
            double best = double.MaxValue;
            for (int k = 0; k < 16; k++)
            {
                double a = k * Math.PI / 8;
                var d = new Vec3(Math.Sin(a), 0, Math.Cos(a));
                if (!g.mover.World.RayIntersect(from, d, out RayHit h)) continue;
                if (Math.Abs(h.normal.y) > 0.2 || h.distance < 1.5 || h.distance > 12 || h.distance >= best) continue;
                double l = Math.Sqrt(h.normal.x * h.normal.x + h.normal.z * h.normal.z);
                if (l < 1e-6) continue;
                best = h.distance;
                point = from + d * h.distance;
                normal = new Vec3(h.normal.x / l, 0, h.normal.z / l);
            }
            return best < double.MaxValue;
        }

        static IEnumerator Wall42Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            Wall42.Params runWall = g.mover.wall42;
            Boost42.Params runBoost = g.loadout.boost42;
            offs.Add(() => { g.mover.SetWall42(runWall); g.loadout.boost42 = runBoost; g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | map {g.MapId} | the run's rows: wallfling42 {runWall.on}, boost42 {runBoost.on}";
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "wall42", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 120;
            var lines = new List<string>();
            int framesSaved = 0, halvesOk = 0;
            double r = sim.cfg.movement.body.capsuleRadius;
            Vec3 chest = spot + new Vec3(0, 1.2, 0);
            bool wallOk = Wall42Find(g, chest, out Vec3 wp, out Vec3 wn);
            int wallJumps = 0, doubles = 0;
            Action<MoverEvent> onMove = e => { if (e.type == MoverEventType.WallJump) wallJumps++; if (e.type == MoverEventType.DoubleJump) doubles++; };
            g.mover.Events += onMove;
            offs.Add(() => g.mover.Events -= onMove);

            // ---- G2: a second press against the wall, the row off then on ----
            for (int half = 0; half < 2 && wallOk; half++)
            {
                string tag = half == 0 ? "off" : "on";
                g.mover.SetWall42(half == 0 ? Wall42.Params.Neutral : Wall42.Params.From(sim.cfg, true));
                Vec3 along = new Vec3(-wn.z, 0, wn.x);
                Vec3 stand = new Vec3(wp.x + wn.x * (r + 0.05), spot.y, wp.z + wn.z * (r + 0.05));
                g.Respawn(stand, YawOf35(along) * 180 / Math.PI);
                g.input.SetLook(YawOf35(along), 0);
                yield return Wait27(g, 0.5, wallEnd, () => God(g));
                // touch the wall once (the stick into it for 0.1 s), then stand still
                Vec3 intoLocal = new Vec3(-wn.x, 0, -wn.z);
                double yawA = YawOf35(along);
                double sx = intoLocal.x * Math.Cos(yawA) - intoLocal.z * Math.Sin(yawA); // the stick's right component toward the wall (yaw: 0 looks -z)
                yield return Do35(g, 0.1, t => g.forcedMove = new InputFrame { moveX = sx >= 0 ? 1 : -1 }, wallEnd);
                yield return Do35(g, 0.3, t => g.forcedMove = new InputFrame(), wallEnd);
                int wj0 = wallJumps, dj0 = doubles;
                double back = 0;
                // the first jump (2 frames pressed, held 0.1 s), then the second press 0.25 s later
                yield return Do35(g, 0.25, t => g.forcedMove = new InputFrame { jump = new Btn { pressed = t < 0.02, held = t < 0.1 } }, wallEnd);
                int s2 = sim.simStep;
                g.forcedMove = new InputFrame { jump = Press(true) };
                double[] offsets = { 0.0, 0.15, 0.30 };
                var sb = new System.Text.StringBuilder();
                sb.Append("W42-wall-").Append(tag).Append(':');
                for (int k = 0; k < offsets.Length; k++)
                {
                    while ((sim.simStep - s2) * sim.step < offsets[k] && Time.realtimeSinceStartup < wallEnd)
                    {
                        God(g);
                        g.forcedMove = new InputFrame { jump = new Btn { pressed = (sim.simStep - s2) * sim.step < 0.02, held = (sim.simStep - s2) * sim.step < 0.1 } };
                        back = Math.Max(back, g.mover.velocity.x * wn.x + g.mover.velocity.z * wn.z);
                        yield return null;
                    }
                    back = Math.Max(back, g.mover.velocity.x * wn.x + g.mover.velocity.z * wn.z);
                    string label = "W42-wall-" + tag + "-" + ((int)Math.Round(offsets[k] * 1000)).ToString("000", ci);
                    sb.Append(' ').Append(label.Substring(label.Length - 3)).Append(" back ").Append((g.mover.velocity.x * wn.x + g.mover.velocity.z * wn.z).ToString("0.00", ci));
                    yield return Shot30(dir, label);
                    framesSaved++;
                }
                g.forcedMove = new InputFrame();
                int wj = wallJumps - wj0, dj = doubles - dj0;
                sb.Append($" | WallJump {wj}, DoubleJump {dj}, the most m/s off the wall {back.ToString("0.00", ci)}");
                lines.Add(sb.ToString());
                if (wj + dj > 0) halvesOk++;
                yield return Wait27(g, 1.0, wallEnd, () => God(g));
            }
            if (!wallOk) lines.Add("W42-wall: NO WALL 1.5-12 m from SafeSpot in 16 headings");

            // ---- G5: a boost on the open floor, the row off then on ----
            yield return Equip27(g, "shotgun");
            g.forcedCombat = IdleCombat;
            int fires = 0;
            offs.Add(g.events.On<string>("fire", w => { if (w == "shotgun") fires++; }));
            for (int half = 0; half < 2; half++)
            {
                string tag = half == 0 ? "off" : "on";
                g.loadout.boost42 = half == 0 ? Boost42.Params.Neutral : Boost42.Params.From(sim.cfg, true);
                g.Respawn(spot, spotYaw * 180 / Math.PI);
                g.input.SetLook(spotYaw, -45 * Math.PI / 180);
                yield return Wait27(g, 1.0, wallEnd, () => { God(g); g.input.SetLook(spotYaw, -45 * Math.PI / 180); });
                double y0 = g.mover.position.y, top = y0;
                int f0 = fires, s0 = sim.simStep, takeoff = -1;
                while (fires == f0 && (sim.simStep - s0) * sim.step < 1.5 && Time.realtimeSinceStartup < wallEnd)
                {
                    double t = (sim.simStep - s0) * sim.step;
                    God(g);
                    g.input.SetLook(spotYaw, -45 * Math.PI / 180);
                    if (takeoff < 0 && !g.mover.grounded) takeoff = sim.simStep;
                    g.forcedMove = new InputFrame { jump = new Btn { pressed = t < 0.02, held = t < 0.2 } };
                    bool pull = takeoff >= 0 && (sim.simStep - takeoff) * sim.step >= 0.1;
                    g.forcedCombat = new CombatInput { weapon = -1, fire = new Btn { pressed = pull, held = pull } };
                    top = Math.Max(top, g.mover.position.y);
                    yield return null;
                }
                g.forcedCombat = IdleCombat;
                if (fires == f0) { lines.Add($"W42-boost-{tag}: NO SHOT in 1.5 s"); continue; }
                int sf = sim.simStep;
                var sb = new System.Text.StringBuilder();
                sb.Append("W42-boost-").Append(tag).Append(':');
                foreach (double off in new[] { 0.0, 0.15, 0.30 })
                {
                    while ((sim.simStep - sf) * sim.step < off && Time.realtimeSinceStartup < wallEnd)
                    {
                        God(g);
                        g.input.SetLook(spotYaw, -45 * Math.PI / 180);
                        g.forcedMove = new InputFrame();
                        top = Math.Max(top, g.mover.position.y);
                        yield return null;
                    }
                    string label = "W42-boost-" + tag + "-" + ((int)Math.Round(off * 1000)).ToString("000", ci);
                    sb.Append(' ').Append(label.Substring(label.Length - 3)).Append(" vy ").Append(g.mover.velocity.y.ToString("0.00", ci));
                    yield return Shot30(dir, label);
                    framesSaved++;
                }
                yield return Wait27(g, 1.5, wallEnd, () => { God(g); top = Math.Max(top, g.mover.position.y); });
                sb.Append($" | the highest rise over the start {(top - y0).ToString("0.00", ci)} m");
                lines.Add(sb.ToString());
                halvesOk++;
            }
            g.mover.SetWall42(runWall);
            g.loadout.boost42 = runBoost;
            yield return Stop();
            bool ok = halvesOk == 4 && framesSaved == 12;
            res[0] = $"{(ok ? "GREEN" : "RED")} wall42 | halves {halvesOk}/4, frames {framesSaved}/12 || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
