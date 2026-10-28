// NIGHT22 U3-36.3 (builder M): the reel scenario `arcline2`, the contract `arcline`'s Player half. UNCOMPILED and UNRUN when written (no
// Editor in the builder's worktree); the same story, scripted on the sim through the pad path, ran headless as `SimHarness arcline2` (PASS;
// `arcline2 --off` is the RED run).
// The story, on the Player (keyboard pass: forcedMove presses; pad pass: a VIRTUAL PAD, `InputSystem.AddDevice<Gamepad>` + QueueStateEvent,
// the button the live layout binds to GRAPPLE: v3 = Y, PadReader.GrappleInputButton; nothing forced while the pad drives):
//   setup   (not recorded) the terminal BUY played: ISLANDS opened, the player 1.2 m in front of the cabinet, BUY (interact) held until
//           ARCLINE is owned; then three husks in a line ahead of SafeSpot(g) (10 / 12.5 / 15 m), frozen
//   READY   the chip before the press (Ui.Arcline22LookNow == Ready), 0.7 s
//   yank    one press at the nearest: arclinePull 1; the chip reads COOLDOWN with ~15 s
//   swing   the husks removed, the player at a seeded swing spot (harness' spot 0), dash + forward + a held press at a node 3 s after the yank,
//           still inside the 15 s cooldown: ropeAttach 1, the speed after the attach >= 20 m/s
//   NO TARGET  the cooldown fast-forwarded (a.Step(): labelled in the line), the player at a spot where the hook has nothing to bite
//           (Arcline22Chip.FindEmptyLook): the chip reads NO TARGET and the crosshair arms are at 40 % alpha
// Every result line starts with FeelReel.Preconditions(g, pos) from SafeSpot(g). RED: row off, no chip built (Ui.Arcline22Built false), or
// any check below failing: the line says which.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N22ArclineBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Arcline2Scenario(g, dir, res, offs, pre);
        }

        static CombatInput Arc2Interact(bool held) => new CombatInput { weapon = -1, interact = new Skygrind.Core.Button { held = held } };

        /// <summary>The highest grapple node within 45 m that is not straight overhead (8 m off) with a clear line and 8 m of run toward it (the harness' ArclineSwingNode).</summary>
        static Vec3? Arc2SwingNode(SurvivalGame g, Vec3 eye)
        {
            Vec3? node = null;
            double bestY = double.MinValue, best = double.MaxValue;
            foreach (Vec3 n in g.sim.map.grappleNodes)
            {
                double d = Vec3.DistanceTo(n, eye);
                double flat = Math.Sqrt((n.x - eye.x) * (n.x - eye.x) + (n.z - eye.z) * (n.z - eye.z));
                if (d > 45 || flat < 8) continue;
                if (g.world.RayIntersect(eye, (n - eye) / d, out RayHit los) && los.distance < d - 0.4) continue;
                var run = new Vec3((n.x - eye.x) / flat, 0, (n.z - eye.z) / flat);
                if (g.world.RayIntersect(new Vec3(eye.x, eye.y - 1.2, eye.z), run, out RayHit rh) && rh.distance < 8) continue;
                if (n.y > bestY || (n.y == bestY && d < best)) { best = d; bestY = n.y; node = n; }
            }
            return node;
        }

        /// <summary>The first seeded floor spot (Mulberry32 2135, 12-26 m from the centre) with a swing node: the harness' swing 0.</summary>
        static bool Arc2SwingSpot(SurvivalGame g, out Vec3 at)
        {
            var rng = new Mulberry32(2135);
            double fy = g.sim.map.floor.max.y;
            for (int tries = 0; tries < 2000; tries++)
            {
                double ang = rng.Next() * Math.PI * 2, rad = 12 + rng.Next() * 14;
                double sx = Math.Cos(ang) * rad, sz = Math.Sin(ang) * rad;
                if (!g.world.RayIntersect(new Vec3(sx, 26, sz), new Vec3(0, -1, 0), out RayHit hit) || Math.Abs(26 - hit.distance - fy) > 0.2) continue;
                if (Arc2SwingNode(g, new Vec3(sx, fy + 1.6, sz)) == null) continue;
                at = new Vec3(sx, fy, sz);
                return true;
            }
            at = default;
            return false;
        }

        static IEnumerator Arcline2Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            var fails = new List<string>();
            Arcline21 a = g.sim.arcline21;
            if (!g.sim.n21.arcline || a == null)
            {
                res[0] = pc + " | RED: design.n21.arcline is off (no ARCLINE to buy, no yank)";
                yield break;
            }
            if (!Ui.Arcline22Built) fails.Add("the ARCLINE chip was not built (Ui.BuildN22Hud does not call BuildArcline22Chip)");

            // --- setup: the terminal BUY played -------------------------------------------------------------------------------------
            PerkSpot? gs = g.sim.Arcline21Spot();
            if (!gs.HasValue)
            {
                res[0] = pc + " | NO gadget spot on this map";
                yield break;
            }
            PerkSpot term = gs.Value;
            g.sim.zones.Open("islands", true);
            g.economy.total = Math.Max(g.economy.total, a.rules.price + 100);
            double spent0 = g.economy.spent;
            g.Respawn(new Vec3(term.pos.x + Math.Sin(term.yaw) * 1.2, term.pos.y, term.pos.z + Math.Cos(term.yaw) * 1.2), 0);
            yield return Frames(20);
            int guard = 0;
            while (!a.Owned && guard++ < 600)
            {
                God(g);
                Look(g, new Vec3(term.pos.x, g.Eye.y, term.pos.z));
                g.forcedCombat = Arc2Interact(true);
                yield return null;
            }
            g.forcedCombat = IdleCombat;
            bool bought = a.Owned;
            double spent = g.economy.spent - spent0;
            if (!bought || Math.Abs(spent - a.rules.price) > 0.5) fails.Add($"the terminal buy was not played (owned {bought}, spent {spent:0})");

            // --- setup: three husks in a line ahead of the safe spot -----------------------------------------------------------------
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(20);
            g.husks.frozen = true;
            var husks = new List<Husk>();
            foreach (double d in new[] { 10.0, 12.5, 15.0 })
            {
                Husk h = PlaceAt(g, d, 0, out _);
                if (h != null) husks.Add(h);
            }
            if (husks.Count < 3) fails.Add($"only {husks.Count} husks placed in a line");
            Husk nearest = null;
            double nd = double.MaxValue;
            foreach (Husk h in husks)
            {
                double d = Vec3.DistanceTo(h.position, g.Feet);
                if (d < nd) { nd = d; nearest = h; }
            }

            int pulls = 0, attaches = 0;
            double peak = 0;
            offs.Add(g.events.On("arclinePull", _ => { pulls++; FeelProbe.Event("arclinePull"); }));
            Action<MoverEvent> onMove = e => { if (e.type == MoverEventType.RopeAttach) { attaches++; FeelProbe.Event("ropeAttach"); } };
            g.mover.Events += onMove;
            offs.Add(() => g.mover.Events -= onMove);

            Gamepad pad = null;
            var state = new GamepadState();
            if (PadPass)
            {
                pad = InputSystem.AddDevice<Gamepad>("ReelPadArcline");
                offs.Add(() => InputSystem.RemoveDevice(pad));
                yield return Frames(10);
            }
            GamepadButton grapple = PadPass ? g.padReader.GrappleInputButton : GamepadButton.North;
            GamepadButton dash = PadPass ? g.padReader.InputButtonFor(PadAction.Dash) : GamepadButton.LeftShoulder;
            string input = PadPass ? $"virtual pad {grapple}" : "keyboard grapple";
            if (nearest != null) Look(g, nearest.AimPoint);
            Start(g, "arcline2", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();

            // --- READY: the chip before the press ------------------------------------------------------------------------------------
            yield return Frames(Ms(700));
            Arcline22Look ready = Ui.Arcline22LookNow;
            if (ready != Arcline22Look.Ready) fails.Add($"the chip before the press read {ready}, not Ready");

            // --- yank: one press at the nearest ---------------------------------------------------------------------------------------
            g.husks.frozen = false; // the yank needs the husk to move
            if (PadPass) { g.forcedMove = null; g.forcedCombat = null; }
            FeelProbe.Event("arclinePress1");
            if (PadPass)
            {
                state = state.WithButton(grapple, true);
                InputSystem.QueueStateEvent(pad, state);
                yield return Frames(4);
                state = state.WithButton(grapple, false);
                InputSystem.QueueStateEvent(pad, state);
            }
            else
            {
                g.forcedMove = new InputFrame { grapple = new Skygrind.Core.Button { held = true, pressed = true } };
                yield return null;
                g.forcedMove = new InputFrame();
            }
            yield return Frames(Ms(600));
            Arcline22Look cool = Ui.Arcline22LookNow;
            int secs = Ui.Arcline22SecondsNow;
            if (pulls != 1) fails.Add($"the first press yanked {pulls} times, not 1");
            if (cool != Arcline22Look.Cooldown || secs < 13 || secs > 15) fails.Add($"the chip after the press read {cool} {secs} s, not Cooldown ~15 s");
            yield return Frames(Ms(900));

            // --- swing: a second press 3 s after the yank, inside the cooldown ----------------------------------------------------
            g.husks.KillAll(); // out of the way (as the harness)
            g.husks.frozen = true;
            int cdLeft = 0;
            if (!Arc2SwingSpot(g, out Vec3 swingAt)) fails.Add("no swing spot with a node");
            else
            {
                FeelProbe.Event("swingTeleport");
                g.Respawn(swingAt, 0);
                yield return Frames(20); // land
                Vec3 node = Arc2SwingNode(g, g.Eye) ?? g.sim.map.grappleNodes[0];
                Look(g, node);
                if (PadPass) { g.forcedMove = null; g.forcedCombat = null; }
                else g.forcedMove = new InputFrame { moveZ = 1 };
                state = default;
                if (PadPass) { state.leftStick = new Vector2(0, 1); InputSystem.QueueStateEvent(pad, state); }
                yield return Frames(4);
                // the dash, then the hook ~8 frames later (the harness: dash 15 steps before the press), held 75 frames (1.25 s), forward throughout
                if (PadPass)
                {
                    state = state.WithButton(dash, true);
                    InputSystem.QueueStateEvent(pad, state);
                    yield return Frames(2);
                    state = state.WithButton(dash, false);
                    InputSystem.QueueStateEvent(pad, state);
                    yield return Frames(6);
                }
                else
                {
                    g.forcedMove = new InputFrame { moveZ = 1, dash = new Skygrind.Core.Button { held = true, pressed = true } };
                    yield return Frames(2);
                    g.forcedMove = new InputFrame { moveZ = 1 };
                    yield return Frames(6);
                }
                cdLeft = a.YankSteps;
                FeelProbe.Event("arclinePress2");
                for (int f = 0; f < 170; f++)
                {
                    God(g);
                    bool hold = f < 75;
                    if (PadPass)
                    {
                        state = state.WithButton(grapple, hold);
                        InputSystem.QueueStateEvent(pad, state);
                    }
                    else g.forcedMove = new InputFrame { moveZ = 1, grapple = new Skygrind.Core.Button { held = hold, pressed = f == 0 } };
                    yield return null;
                    if (attaches > 0) peak = Math.Max(peak, g.mover.velocity.Length);
                }
                if (PadPass)
                {
                    state = default;
                    InputSystem.QueueStateEvent(pad, state);
                }
                g.forcedMove = new InputFrame();
                g.forcedCombat = IdleCombat;
                yield return Frames(Ms(500));
            }
            if (cdLeft <= 0) fails.Add("the second press was not inside the cooldown");
            if (attaches != 1) fails.Add($"the second press attached {attaches} times, not 1");
            if (pulls != 1) fails.Add($"arclinePull ended at {pulls}, not 1");
            if (peak < 20) fails.Add($"the speed after the attach peaked at {peak.ToString("0.0", ci)} m/s, under 20");

            // --- NO TARGET: the cooldown fast-forwarded, a spot where the hook has nothing to bite ------------------------------------
            int skipped = a.YankSteps;
            while (a.YankSteps > 0) a.Step(); // labelled in the line: the reel does not wait out 15 s
            double eyeH = g.cfg.movement.body.eyeHeight;
            double below = g.cfg.Num("ai.grid.lookBelow"), above = g.cfg.Num("ai.grid.lookAbove");
            double fy = g.sim.map.floor.max.y;
            List<Vec3> eyes = Arcline22Chip.EyeGrid(g.sim.map.floor, eyeH, 6, (x, z) => g.sim.nav.grid.NodeAt(x, fy, z, below, above) >= 0);
            Arcline22Look noTarget = Arcline22Look.Hidden;
            float dimAlpha = 1f;
            if (Arcline22Chip.FindEmptyLook(a, eyes, out Vec3 emptyEye, out double ey, out double ep))
            {
                FeelProbe.Event("noTargetTeleport");
                g.Respawn(new Vec3(emptyEye.x, fy, emptyEye.z), 0);
                g.input.SetLook(ey, ep);
                yield return Frames(Ms(900));
                noTarget = Ui.Arcline22LookNow;
                dimAlpha = Ui.Arcline22ReticleAlphaNow;
                if (noTarget != Arcline22Look.NoTarget) fails.Add($"the chip with nothing to hook read {noTarget}, not NoTarget");
                if (dimAlpha > UnityTuning.Arcline22NoTargetAlpha + 0.05f) fails.Add($"the reticle alpha on NO TARGET was {dimAlpha.ToString("0.00", ci)}, not ~0.4");
            }
            else fails.Add("no spot on this map where the hook has nothing to bite");
            yield return Frames(Ms(300));
            yield return Stop();

            string looks = $"chip looks {ready} / {cool} {secs} s / {noTarget} (reticle alpha {dimAlpha.ToString("0.00", ci)})";
            res[0] = pc + $" | {FeelProbe.FrameCount} frames; input {input}; terminal bought {bought} for {spent:0}; {husks.Count} husks in a line; arclinePull {pulls}, ropeAttach {attaches}, peak speed after the attach {peak.ToString("0.0", ci)} m/s, cooldown left at the second press {(cdLeft / 120.0).ToString("0.0", ci)} s; {looks}; cooldown fast-forwarded {(skipped / 120.0).ToString("0.0", ci)} s for NO TARGET | "
                + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
