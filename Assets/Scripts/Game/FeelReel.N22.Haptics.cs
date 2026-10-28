// NIGHT22 U3-36.8 (builder M): the reel scenario `haptics`, the contract `haptics`'s Player half. UNCOMPILED and UNRUN when written (no
// Editor in the builder's worktree). The rule (patterns, priority, log, silence) is tested headless: `SimHarness arcline2 --selftest`.
// It plays the four classes on the Player, in order, and prints the haptics log (Haptics22.LogLine: event, low / high motor, ms) in the
// result line and in `haptics.log` beside the frames:
//   shockwave  the THUNDERCLAP's blast (a real LT press, as the `shockwave` scenario)
//   attach     ARCLINE owned (Own(true): the buy is `arcline2`'s), a husk yanked (arclinePull -> the yank pattern), then the hook at the floor (ropeAttach)
//   kill       a real kill by the aim bot on a husk 6 m ahead
//   down       the player takes a lethal blow (death -> DOWN), as `ui-down`
// Then VIBRATION is switched off in the scenario and the same four events are fired again: the log must not grow (a silence check; the
// scenario restores the setting). Keyboard pass: PlayerDevices.Last is the keyboard, so nothing may be logged and nothing may throw
// (the contract's "a pad pass with no pad connected logs nothing and throws nothing"); pad pass: a VIRTUAL PAD is added and held as the device in use.
// Every result line starts with FeelReel.Preconditions(g, pos) from SafeSpot(g). RED: design.n22.haptics off, or the view not built.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N22HapticsBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = HapticsScenario(g, dir, res, offs, pre);
        }

        static IEnumerator HapticsScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            if (!g.sim.n22.haptics)
            {
                res[0] = pc + " | RED: design.n22.haptics is off";
                yield break;
            }
            Haptics22View view = Haptics22View.Instance;
            if (view == null)
            {
                res[0] = pc + " | RED: no Haptics22View (SurvivalGame.N22InitViews does not build it)";
                yield break;
            }
            Haptics22 h = view.haptics;
            Rumble rumble = g.rumble;
            bool wasOn = rumble.enabled;
            double wasStrength = rumble.strength;
            offs.Add(() =>
            {
                rumble.enabled = wasOn;
                rumble.strength = wasStrength;
            });
            Gamepad pad = null;
            if (PadPass)
            {
                pad = InputSystem.AddDevice<Gamepad>("ReelPadHaptics");
                offs.Add(() => InputSystem.RemoveDevice(pad));
                PlayerDevices.Last = InputDevice.Pad;
            }
            rumble.enabled = true;
            if (rumble.strength <= 0) rumble.strength = 1;
            h.ClearLog();

            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(20);
            int slot = SlotOf(g, "shotgun");
            if (g.sim.arcline21 != null) g.sim.arcline21.Own(true);
            Start(g, "haptics", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var marks = new List<string>();
            var fails = new List<string>();

            // shockwave
            int before = h.Log.Count;
            if (slot >= 0)
            {
                g.forcedCombat = new CombatInput { weapon = slot };
                yield return Frames(45);
                g.forcedCombat = IdleCombat;
                g.input.SetLook(spotYaw, 0);
                FeelProbe.Event("hapticsShockwave");
                g.forcedCombat = Lt(true);
                yield return null;
                g.forcedCombat = IdleCombat;
                yield return Frames(Ms(700));
            }
            marks.Add($"shockwave {h.Log.Count - before} steps");
            if (slot >= 0 && h.Log.Count == before) fails.Add("the shockwave logged nothing");

            // attach (and the yank before it)
            before = h.Log.Count;
            Husk target = PlaceAt(g, 12, 0, out double hy);
            if (target != null)
            {
                g.husks.frozen = false;
                Look(g, new Vec3(target.position.x, target.position.y + 1, target.position.z));
                yield return Frames(10);
                FeelProbe.Event("hapticsYank");
                g.forcedMove = new InputFrame { grapple = new Skygrind.Core.Button { held = true, pressed = true } };
                yield return null;
                g.forcedMove = new InputFrame();
                yield return Frames(Ms(700));
                g.husks.KillAll();
            }
            int afterYank = h.Log.Count;
            g.input.SetLook(spotYaw, -0.2); // down at the floor ahead: any surface swings
            yield return Frames(10);
            FeelProbe.Event("hapticsAttach");
            for (int i = 0; i < Ms(800); i++)
            {
                God(g);
                g.forcedMove = new InputFrame { grapple = new Skygrind.Core.Button { held = true, pressed = i == 0 } };
                yield return null;
            }
            g.forcedMove = new InputFrame();
            yield return Frames(Ms(700));
            marks.Add($"yank {afterYank - before} steps");
            marks.Add($"attach {h.Log.Count - afterYank} steps");
            if (target != null && afterYank == before) fails.Add("the yank logged nothing");
            if (h.Log.Count == afterYank) fails.Add("the attach logged nothing");

            // kill: a real one
            before = h.Log.Count;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(20);
            Husk victim = PlaceAt(g, 6, 0, out _);
            int kills = 0;
            offs.Add(g.events.On<KillEvent>("kill", e => { if (e.owner == "player") kills++; }));
            if (victim != null)
            {
                g.husks.frozen = true;
                FeelProbe.Event("hapticsKill");
                int guard = 0;
                while (kills == 0 && guard++ < 480)
                {
                    God(g);
                    FeelStrip.AimBot(g);
                    yield return null;
                }
                g.forcedCombat = IdleCombat;
                yield return Frames(Ms(500));
            }
            marks.Add($"kill {h.Log.Count - before} steps (kills {kills})");
            if (kills == 0) fails.Add("no kill was made (the aim bot): kill not exercised");
            else if (h.Log.Count == before) fails.Add("the kill logged nothing");

            // VIBRATION off: the same events again, the log must not grow
            int total = h.Log.Count;
            rumble.enabled = false;
            h.Cancel();
            g.events.Emit("kill", new KillEvent { owner = "player" });
            g.events.Emit(N20Events.Shockwave, new ShockwaveEvent { owner = "player" });
            g.events.Emit("death", new DeathEvent { owner = "player", source = "reel" });
            yield return Frames(Ms(600));
            bool silent = h.Log.Count == total;
            marks.Add($"VIBRATION off: log {(silent ? "unchanged" : "GREW")}");
            if (!silent) fails.Add("VIBRATION off did not silence the patterns");
            rumble.enabled = true;

            // DOWN: a real lethal blow (as `ui-down`)
            before = h.Log.Count;
            Vec3 f0 = g.Feet;
            g.health.max = g.health.hp = 100;
            FeelProbe.Event("hapticsDown");
            g.health.Damage(g.health.max * 2, "husk", new Vec3(f0.x, f0.y, f0.z - 3));
            yield return Frames(Ms(1500));
            marks.Add($"down {h.Log.Count - before} steps");
            if (h.Log.Count == before) fails.Add("DOWN logged nothing");
            yield return Stop();

            string line = h.LogLine();
            try { File.WriteAllText(Path.Combine(dir, "haptics.log"), line + "\n"); }
            catch (Exception) { /* the log line in the result is the record */ }
            bool padPlaying = PadPass;
            if (!padPlaying)
            {
                // keyboard pass: PlayerDevices.Last is the keyboard, so the sink cannot play: the log must be empty and nothing threw
                res[0] = pc + $" | keyboard pass: {(h.Log.Count == 0 ? "PASS" : "FAIL")} (nothing may be logged without a pad in use; logged {h.Log.Count}) | {line}";
                yield break;
            }
            res[0] = pc + $" | pad pass (virtual pad): {string.Join(", ", marks)} | {line} | " + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
