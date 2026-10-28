// NIGHT34 builder M: the Player scenario of design.n34.flow34 (the name is FeelReel.N27.cs PhaseN34M "flow34"; the body is the partial N34FlowBody).
// Not run by its author (no Player in the worktree). Input through the game's forced move frame (the reel's movement path, FeelReel.cs's dash rows),
// the run's own flow34 rules (the row on: GREEN; -skygrindOff34 or the row off: RED, the web's mover), husks frozen, the player kept alive.
//   flow34  any map, SURVIVAL, a safe spot (SafeSpot: 12 m from pads and buy stations).
//     BHOP: forward held (up to 4 s of sim time, 3 hops); a ground jump, then on each descent ONE press under 0.05 s before the touchdown (inside the
//       0.1 s buffer: an earlier press is the double jump), held 0.15 s. Line: the hops (MoverEvent
//       Jump with buffered true), the speed kept on each hop (speedAfter / speedBefore, min and mean, %). The no-friction landing is the web's own
//       (table M 2 exists): the line is the Player proof that it holds with the row on, not a change.
//     ZIP CARRY: in the air, the player given 10 m/s to his right, a hook fired at a point 12 m ahead and 3 m up and held as a ZIP (rope.zip set each
//       frame, as the NIGHT31 tap latch does), let go by a jump after 0.25 s attached. Line: the sideways (right) speed after the release, the
//       Mover's carry count. GREEN (zipCarry on): sideways >= 5 m/s and carries 1; RED (off): the pull erased it (about 0).
// The result line: "[reel] flow34: BHOP ... || ZIP ... | preconditions".
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using Btn = Skygrind.Core.Button;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N34FlowBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "flow34") body = Flow34Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Flow34Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            SurvivalSim sim = g.sim;
            Mover m = sim.mover;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            Flow34.Params rules = m.flow34.rules;
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | n34.flow34 {sim.n34.flow34} (wallCoyote {rules.wallCoyote}, wallBuffer {rules.wallBuffer}, zipCarry {rules.zipCarry})";
            var hops = new List<double>();
            int buffered = 0;
            double releaseSide = double.NaN;
            Vec3 rightDir = new Vec3(Math.Cos(spotYaw), 0, -Math.Sin(spotYaw));
            Action<MoverEvent> onMove = e =>
            {
                // the zip's release: the velocity as ReleaseRope leaves it (the carry is added before the event)
                if (e.type == MoverEventType.RopeRelease && e.attached && double.IsNaN(releaseSide)) releaseSide = m.velocity.x * rightDir.x + m.velocity.z * rightDir.z;
                // a hop: a ground jump taken while moving (the first, from a standstill, is the takeoff); buffered: its press came before the landing
                if (e.type != MoverEventType.Jump || e.speedBefore <= 0.5 || hops.Count >= 3) return;
                if (e.buffered) buffered++;
                hops.Add(100 * e.speedAfter / e.speedBefore);
            };
            m.Events += onMove;
            bool ropeWas = m.ropeEnabled;
            offs.Add(() => { m.Events -= onMove; m.ropeEnabled = ropeWas; g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = IdleCombat;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "flow34", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 60;

            // ---- BHOP ----
            // The press must land INSIDE the 0.1 s buffer before touchdown: earlier, a press in the air with the double jump left IS the double jump
            // (the web's landingSoon holds it only when the landing is predicted within the buffer). So: press when the time to the floor (the spot's
            // height, a flat floor; t = h / |vy|, a slight over-estimate under gravity) is under 0.05 s, hold the key 0.15 s, once per airtime.
            bool pressedThisAir = false, first = true;
            double floorY = spot.y, heldUntil = -1, lead = 0.05;
            yield return Wait27(g, 4.0, wallEnd, () =>
            {
                God(g);
                bool press = false;
                double now = sim.simStep * sim.step;
                if (hops.Count >= 3) { g.forcedMove = new InputFrame { moveZ = 1 }; return; }
                if (first && m.grounded) { press = true; first = false; }
                else if (!m.grounded && m.velocity.y < 0 && !pressedThisAir && m.position.y - floorY <= -m.velocity.y * lead) { press = true; pressedThisAir = true; }
                else if (m.grounded && !pressedThisAir && now >= heldUntil) press = true; // a frame stepped past the window: a plain ground jump (not buffered; its friction shows in the line)
                if (m.grounded && !press) pressedThisAir = false;
                if (press) heldUntil = now + 0.15;
                g.forcedMove = new InputFrame { moveZ = 1, jump = new Btn { pressed = press, held = press || now < heldUntil } };
            });
            g.forcedMove = new InputFrame();
            double min = double.PositiveInfinity, sum = 0, all = 1;
            foreach (double h in hops) { min = Math.Min(min, h); sum += h; all *= h / 100; }
            string bhopLine = hops.Count == 0
                ? $"BHOP RED: no landing jump in 4 s"
                : $"BHOP {(hops.Count >= 3 && min >= 99.9 ? "PASS" : "RED")}: {hops.Count} hops ({buffered} buffered), speed kept per hop {string.Join(" / ", hops.ConvertAll(h => h.ToString("0.0")))} %, over the {hops.Count} hops {100 * all:0.0} % (the web's no-friction landing; table M 2 exists)";
            yield return Frames(Ms(300));

            // ---- ZIP CARRY ----
            g.Respawn(spot + new Vec3(0, 3, 0), spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            m.ropeEnabled = true;
            yield return Frames(2);
            Vec3 fwd = new Vec3(-Math.Sin(spotYaw), 0, -Math.Cos(spotYaw)), right = new Vec3(Math.Cos(spotYaw), 0, -Math.Sin(spotYaw));
            Vec3 eye = sim.Eye;
            var target = new GrappleTarget { point = new Vec3(eye.x + fwd.x * 12, eye.y + 3, eye.z + fwd.z * 12) };
            int carries0 = m.flow34.carries;
            releaseSide = double.NaN;
            m.velocity = new Vec3(right.x * 10, 0, right.z * 10);
            g.forcedMove = new InputFrame { grapple = new Btn { pressed = true, held = true }, grappleTarget = target };
            yield return null;
            int attachedAt = -1;
            yield return Wait27(g, 1.0, wallEnd, () =>
            {
                God(g);
                if (m.rope.phase != RopePhase.None) m.rope.zip = true;
                if (attachedAt < 0 && m.rope.phase == RopePhase.Attached) attachedAt = sim.simStep;
                bool release = attachedAt >= 0 && (sim.simStep - attachedAt) * sim.step >= 0.25;
                g.forcedMove = new InputFrame { grapple = new Btn { held = !release }, jump = new Btn { pressed = release, held = release } };
                if (release) attachedAt = int.MaxValue / 2; // once
            });
            g.forcedMove = new InputFrame();
            double side = releaseSide;
            int carries = m.flow34.carries - carries0;
            bool zipOk = rules.zipCarry && side >= 5 && carries == 1;
            string zipLine = $"ZIP {(zipOk ? "PASS" : "RED")}: sideways speed after the jump release {side:0.00} m/s (10 m/s before the pull), carries {carries}, rope {m.rope.phase}";
            yield return Frames(Ms(300));
            yield return Stop();
            res[0] = $"{bhopLine} || {zipLine} | {pc}";
        }
    }
}
