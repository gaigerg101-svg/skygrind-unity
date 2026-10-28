// NIGHT35 builder M: the Player scenarios of design.n35.wallride35 and design.n35.grapple35 (the names are FeelReel.N27.cs PhaseN35M; the body is
// the partial N35MoveBody). Not run by its author (no Player in the worktree). Input through the game's forced move frame only (L-046), husks
// frozen, the player kept alive. Any map, SURVIVAL, from the safe spot (SafeSpot: 12 m from pads and buy stations).
//   wallride35  finds a long wall near the spot (rays in 16 directions at chest height: a wall 3-12 m away with 8 m of wall along it), puts the
//     player 0.6 m off it, 1.5 m up, moving along it at 8 m/s, jump HELD (never pressed: no double jump) for 2.5 s or until the ride ends, then
//     RELEASED at a level look. Line: the ride's seconds, its end reason, its speed, and whether the release jumped (a WallJump event within 0.1 s
//     of the release, the Mover's jump-off count). GREEN (row on): ridden past the web's 1.2 s timer (or ended by the wall, not by "timeout") and
//     the release jumped off. RED (-skygrindOff35 or the row off): "timeout" at 1.2 s, the release does nothing.
//   grapple35   finds a wall 10-30 m ahead of the spot (16 directions at chest height), fires the grapple at it and HOLDS the button 2 s (a natural
//     press-and-hold). Line: arrived (the sim's grappleArrive), the seconds to it, the chest path's largest distance off the straight line to the
//     hit point, the closest the chest came to it. GREEN (row on): arrived in under 1.5 s within 0.3 m of the line. RED (off): the hold is
//     NIGHT30's swing (no arrival; a hold on a bare wall).
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
        static partial void N35MoveBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "wallride35") body = Ride35Scenario(g, dir, res, offs, pre);
            else if (s == "grapple35") body = Grapple35Scenario(g, dir, res, offs, pre);
        }

        /// <summary>A wall seen from `from` at chest height in one of 16 directions, minD..maxD away (|normal.y| under 0.3); with `along` > 0 the
        /// wall must also be there `along` m along it (one way) and the way along it clear: returns the hit point, its normal and the direction along.</summary>
        static bool FindWall35(SurvivalGame g, Vec3 from, double minD, double maxD, double along, out Vec3 point, out Vec3 normal, out Vec3 dirAlong)
        {
            point = normal = dirAlong = default;
            double best = double.MaxValue;
            for (int i = 0; i < 16; i++)
            {
                double a = i * Math.PI / 8;
                var d = new Vec3(Math.Sin(a), 0, Math.Cos(a));
                if (!g.world.RayIntersect(from, d, out RayHit h) || h.distance < minD || h.distance > maxD || Math.Abs(h.normal.y) > 0.3) continue;
                Vec3 p = from + d * h.distance;
                Vec3 n = Vec3.Normalize(new Vec3(h.normal.x, 0, h.normal.z));
                if (along <= 0)
                {
                    if (h.distance < best) { best = h.distance; point = p; normal = n; dirAlong = d; }
                    continue;
                }
                foreach (double sgn in new[] { 1.0, -1.0 })
                {
                    var t = new Vec3(-n.z * sgn, 0, n.x * sgn);
                    Vec3 o = p + n * 0.6;
                    if (g.world.RayIntersect(o, t, out RayHit c) && c.distance < along) continue; // something across the way
                    bool wall = true;
                    for (double k = 2; k <= along && wall; k += 2)
                        wall = g.world.RayIntersect(o + t * k, n * -1, out RayHit w) && w.distance < 1.2 && Math.Abs(w.normal.y) < 0.3;
                    if (!wall || h.distance >= best) continue;
                    best = h.distance;
                    point = p;
                    normal = n;
                    dirAlong = t;
                }
            }
            return best < double.MaxValue;
        }

        static IEnumerator Ride35Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            Mover m = sim.mover;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            Ride35.Params rules = m.flow34.ride35.rules;
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | n35.wallride35 {sim.n35.wallride35} (on {rules.on}, safety {rules.safetyCap} s, buff {rules.speedBuff})";
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = IdleCombat;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Vec3 chest = new Vec3(spot.x, spot.y + 1.2, spot.z);
            if (!FindWall35(g, chest, 3, 12, 8, out Vec3 wp, out Vec3 wn, out Vec3 wt))
            {
                res[0] = "RED: no wall with 8 m along it within 12 m of the safe spot (boot another map) | " + pc;
                yield break;
            }
            double startedAt = double.NaN, endedAt = double.NaN, releaseAt = double.NaN, jumpAt = double.NaN, rideSpeed = 0;
            string endReason = "none";
            int jumpOffs0 = m.flow34.ride35.jumpOffs;
            Action<MoverEvent> onMove = e =>
            {
                double now = sim.SimTime;
                if (e.type == MoverEventType.WallRideStart && double.IsNaN(startedAt)) startedAt = now;
                if (e.type == MoverEventType.WallRideEnd && !double.IsNaN(startedAt) && double.IsNaN(endedAt)) { endedAt = now; endReason = e.reason; }
                if (e.type == MoverEventType.WallJump && !double.IsNaN(releaseAt) && double.IsNaN(jumpAt)) jumpAt = now;
            };
            m.Events += onMove;
            offs.Add(() => m.Events -= onMove);
            double yaw = Math.Atan2(-wt.x, -wt.z);
            Vec3 start = new Vec3(wp.x + wn.x * 0.6 - wt.x * 1.0, spot.y + 1.5, wp.z + wn.z * 0.6 - wt.z * 1.0);
            g.Respawn(start, yaw * 180 / Math.PI);
            g.input.SetLook(yaw, 0);
            Start(g, "wallride35", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            m.velocity = new Vec3(wt.x * 8 - wn.x * 1, 0, wt.z * 8 - wn.z * 1);
            double wallEnd = Time.realtimeSinceStartup + 30;
            double t0 = sim.SimTime;
            yield return Wait27(g, 3.5, wallEnd, () =>
            {
                God(g);
                double now = sim.SimTime;
                bool riding = m.state == MoveState.WallRide;
                if (riding) rideSpeed = Math.Max(rideSpeed, m.Speed);
                bool release = !double.IsNaN(startedAt) && (now - startedAt >= 2.5 || !double.IsNaN(endedAt));
                if (release && double.IsNaN(releaseAt)) releaseAt = now;
                g.input.SetLook(yaw, 0);
                g.forcedMove = new InputFrame { moveZ = 1, yaw = yaw, jump = new Btn { held = !release } };
            });
            g.forcedMove = new InputFrame();
            yield return Frames(Ms(300));
            yield return Stop();
            double ride = double.IsNaN(startedAt) ? 0 : (double.IsNaN(endedAt) ? (double.IsNaN(releaseAt) ? sim.SimTime : releaseAt) : endedAt) - startedAt;
            bool jumped = !double.IsNaN(jumpAt) && jumpAt - releaseAt <= 0.1 && m.flow34.ride35.jumpOffs > jumpOffs0;
            bool longRide = ride > 1.25 || (endReason != "timeout" && endReason != "none" && !double.IsNaN(startedAt));
            bool ok = !double.IsNaN(startedAt) && longRide && jumped;
            res[0] = $"{(ok ? "GREEN" : "RED")}: ride {ride.ToString("0.00", ci)} s (end '{endReason}'), speed {rideSpeed.ToString("0.0", ci)} m/s, release jumped {jumped} (jump-offs {m.flow34.ride35.jumpOffs - jumpOffs0}, up {m.flow34.ride35.lastUp.ToString("0.00", ci)} m/s, out {m.flow34.ride35.lastSpeed.ToString("0.0", ci)} m/s) | wall at {wp.x.ToString("0.0", ci)}, {wp.z.ToString("0.0", ci)} | {pc}";
        }

        static IEnumerator Grapple35Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            Mover m = sim.mover;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | n35.grapple35 {sim.n35.grapple35} (pull {sim.Grapple35Pull}), n31.grapplePull {sim.n31.grapplePull}, zip {g.cfg.movement.arcline.zip.reelSpeed.ToString("0", ci)} m/s";
            bool ropeWas = m.ropeEnabled;
            offs.Add(() => { m.ropeEnabled = ropeWas; g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = IdleCombat;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Vec3 eye = sim.Eye;
            if (!FindWall35(g, eye, 10, 30, 0, out Vec3 wp, out Vec3 wn, out Vec3 wd))
            {
                res[0] = "RED: no wall 10-30 m from the safe spot at eye height (boot another map) | " + pc;
                yield break;
            }
            m.ropeEnabled = true;
            int arrivals = 0;
            double arriveAt = double.NaN, arriveSpeed = double.NaN;
            offs.Add(g.events.On<GrapplePull31Event>("grappleArrive", e => { if (arrivals++ == 0) { arriveAt = sim.SimTime; arriveSpeed = e.speed; } }));
            double yaw = Math.Atan2(-wd.x, -wd.z);
            g.input.SetLook(yaw, 0);
            Start(g, "grapple35", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            Vec3 from = m.RopePoint;
            Vec3 line = wp - from;
            double len = line.Length;
            Vec3 u = line * (1 / Math.Max(1e-6, len));
            double maxOff = 0, closest = double.MaxValue, t0 = sim.SimTime;
            g.forcedMove = new InputFrame { yaw = yaw, grapple = new Btn { pressed = true, held = true }, grappleTarget = new GrappleTarget { point = wp } };
            yield return null;
            double wallEnd = Time.realtimeSinceStartup + 30;
            yield return Wait27(g, 2.0, wallEnd, () =>
            {
                God(g);
                Vec3 c = m.RopePoint;
                closest = Math.Min(closest, Vec3.DistanceTo(c, wp));
                if (arrivals == 0 && m.rope.phase == RopePhase.Attached)
                {
                    Vec3 q = c - from;
                    maxOff = Math.Max(maxOff, (q - u * Vec3.Dot(q, u)).Length);
                }
                g.input.SetLook(yaw, 0);
                g.forcedMove = new InputFrame { yaw = yaw, grapple = new Btn { held = true } };
            });
            g.forcedMove = new InputFrame();
            yield return Frames(Ms(300));
            yield return Stop();
            double secs = arrivals > 0 ? arriveAt - t0 : double.NaN;
            bool ok = arrivals > 0 && secs < 1.5 && maxOff <= 0.3;
            res[0] = $"{(ok ? "GREEN" : "RED")}: {(arrivals > 0 ? $"arrived in {secs.ToString("0.00", ci)} s at {arriveSpeed.ToString("0.0", ci)} m/s" : "no arrival (a hold swings)")}, off the line max {maxOff.ToString("0.00", ci)} m, closest {closest.ToString("0.00", ci)} m to the point {len.ToString("0.0", ci)} m away, rope {m.rope.phase} | {pc}";
        }
    }
}
