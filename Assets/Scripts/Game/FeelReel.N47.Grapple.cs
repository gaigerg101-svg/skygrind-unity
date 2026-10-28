// NIGHT47 builder F: the grapple's proof frames (G23, grapple47), run inside a747 when the booted level owns the grapple:
//   reel.sh + -skygrindMap n46-span + -skygrindReelOnly a747
// The nearest grapple node ahead of the room's spawn (within 30 m); per pass (grapple47 forced off, then on), from the same spot: AIM (held on
// the node, 20 frames), FIRE (RB on a virtual pad: the live layout's GrappleInputButton, v4 = RB; frames +2), PULL (+12), RELEASE (2 frames
// after the rope lets go), then BEND (a husk stood 8 m ahead, RB on it: the whip's latch starts the short bend; +10 frames). Frames:
// shot-N30-X47-grapple-<off|on>-<aim|fire|pull|release|bend>.png; the line prints Grapple47's phase / pulse / wave and sim.Bend34Inside.
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
        static IEnumerator Grapple47Frames(SurvivalGame g, string dir, List<string> lines, List<Action> offs, double wallEnd)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            if (sim.arcline21 == null || !sim.arcline21.Owned) { lines.Add("grapple: not owned on this level (run a747 on n46-span for G23's frames)"); yield break; }
            Vec3 at0 = g.Feet;
            double yaw0 = g.input != null ? g.input.yaw : 0;
            Vec3 node = default;
            double best = double.MaxValue;
            foreach (Vec3 n in sim.map.grappleNodes)
            {
                double d = Math.Sqrt((n.x - at0.x) * (n.x - at0.x) + (n.z - at0.z) * (n.z - at0.z));
                if (d > 4 && d < 30 && d < best) { best = d; node = n; }
            }
            if (best == double.MaxValue) { lines.Add("grapple: no node within 30 m of the spawn"); yield break; }
            var pad = InputSystem.AddDevice<Gamepad>("ReelPadGrapple47");
            offs.Add(() => { try { InputSystem.RemoveDevice(pad); } catch (Exception) { } });
            bool? was = Grapple47.Force;
            offs.Add(() => Grapple47.Force = was);
            GamepadButton rb = g.padReader != null ? g.padReader.GrappleInputButton : GamepadButton.RightShoulder;
            var state = new GamepadState();
            int[] mag = new int[1];
            var sb = new System.Text.StringBuilder($"grapple (button {rb}, node {best.ToString("0.0", ci)} m):");
            for (int pass = 0; pass < 2 && Time.realtimeSinceStartup < wallEnd; pass++)
            {
                string tag = pass == 1 ? "on" : "off";
                Grapple47.Force = pass == 1;
                g.Respawn(at0, yaw0 * 180 / Math.PI);
                g.forcedMove = null;
                g.forcedCombat = null;
                for (int i = 0; i < 20; i++) { God(g); Look(g, node); yield return null; }
                yield return Capture47(dir, $"X47-grapple-{tag}-aim", mag);
                state = state.WithButton(rb, true);
                pad.MakeCurrent(); // PadReader reads Gamepad.current
                InputSystem.QueueStateEvent(pad, state);
                for (int i = 0; i < 2; i++) { God(g); yield return null; }
                yield return Capture47(dir, $"X47-grapple-{tag}-fire", mag);
                string fire = $"{Grapple47.Phase}/{Grapple47.WaveNow.ToString("0.00", ci)}";
                for (int i = 0; i < 2; i++) { God(g); yield return null; }
                state = state.WithButton(rb, false);
                pad.MakeCurrent(); // PadReader reads Gamepad.current
                InputSystem.QueueStateEvent(pad, state);
                for (int i = 0; i < 8; i++) { God(g); yield return null; }
                yield return Capture47(dir, $"X47-grapple-{tag}-pull", mag);
                string pull = $"{Grapple47.Phase}/pulse {Grapple47.PulseAt.ToString("0.00", ci)} (bend inside {sim.Bend34Inside}, latches {sim.bend34.latches})";
                float end = Time.realtimeSinceStartup + 4;
                while (sim.mover.rope.phase != RopePhase.None && Time.realtimeSinceStartup < end) { God(g); yield return null; }
                for (int i = 0; i < 2; i++) { God(g); yield return null; }
                yield return Capture47(dir, $"X47-grapple-{tag}-release", mag);
                string rel = Grapple47.Phase;
                // the BEND: a husk ahead, RB on it (the whip's latch)
                g.Respawn(at0, yaw0 * 180 / Math.PI);
                double fx = -Math.Sin(yaw0), fz = -Math.Cos(yaw0);
                Husk h = sim.husks.Spawn("husk", new Vec3(at0.x + fx * 8, at0.y, at0.z + fz * 8), at0);
                if (h != null) { h.hp = h.maxHp = 1e6; }
                g.husks.frozen = true;
                // n47-r1b "bend inside False": the press came 2 frames after the release, inside the rope's cooldown (GrappleAim refuses a press
                // while rope.cooldownSteps > 0): wait it out (and the meter full, for a FULL bend), then press; the frame is the first one inside
                float cdEnd = Time.realtimeSinceStartup + 3;
                while ((sim.mover.rope.cooldownSteps > 0 || sim.mover.rope.phase != RopePhase.None) && Time.realtimeSinceStartup < cdEnd) { God(g); if (h != null) Look(g, h.AimPoint); yield return null; }
                sim.bend34.meter = 1;
                for (int i = 0; i < 12; i++) { God(g); if (h != null) Look(g, h.AimPoint); yield return null; }
                int latch0 = sim.bend34.latches;
                state = state.WithButton(rb, true);
                pad.MakeCurrent(); // PadReader reads Gamepad.current
                InputSystem.QueueStateEvent(pad, state);
                for (int i = 0; i < 3; i++) { God(g); yield return null; }
                state = state.WithButton(rb, false);
                pad.MakeCurrent(); // PadReader reads Gamepad.current
                InputSystem.QueueStateEvent(pad, state);
                int waited = 0;
                while (!sim.Bend34Inside && waited < 40) { God(g); yield return null; waited++; }
                for (int i = 0; i < 4; i++) { God(g); yield return null; }
                yield return Capture47(dir, $"X47-grapple-{tag}-bend", mag);
                sb.Append($" {tag}: fire {fire}, pull {pull}, release {rel}, bend inside {sim.Bend34Inside} after {waited} frames (latches +{sim.bend34.latches - latch0}, rope {sim.mover.rope.phase}, cooldown {sim.mover.rope.cooldownSteps}) ({Grapple47.Phase});");
                if (h != null && h.alive) h.hp = 0.0001;
                g.husks.frozen = false;
                float settle = Time.realtimeSinceStartup + 3;
                while (sim.mover.rope.phase != RopePhase.None && Time.realtimeSinceStartup < settle) { God(g); yield return null; }
                yield return Wait27(g, 0.6, wallEnd, () => God(g));
            }
            lines.Add(sb.ToString());
        }
    }
}
