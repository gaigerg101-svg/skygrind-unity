// NIGHT24 pad-y-weapons (builder R): the reel scenario `padweapons`, the contract's Player half. Written FIRST, UNCOMPILED and UNRUN
// (no Editor in the builder's worktree); its RED run is design.n24.padY off (layout v3): the line says "RED: ..." and what it saw.
// A VIRTUAL PAD (InputSystem.AddDevice<Gamepad> + QueueStateEvent) is the device, nothing forced while it drives; the keyboard pass
// prints SKIPPED. The story, on layout v4 (design.n24.padY on):
//   Y tap  x3   the loadout slot advances each time (g.loadout.targetSlot), and the port-only "weaponSwitch" is counted (it needs the
//               lead's one wiring line, playerInput.events = events; the line says so when the slot moved but no event came)
//   Y hold      past design.pad.n20.wheelHold the pad's wheel opens (g.combatReader.select.radial.open) and the release closes it
//   RB          SPECIAL + ARCLINE: with the gadget owned and a husk 10 m ahead, one press yanks (arclinePull 1); no weapon change
//   LB          still DASH: the dash fires (a "dash" mover event) from the LB press
// Every result line starts with FeelReel.Preconditions(g, pos) from SafeSpot(g).
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N24PadWeaponsBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = PadWeaponsScenario(g, dir, res, offs, pre);
        }

        static IEnumerator PadWeaponsScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            if (!PadPass)
            {
                res[0] = pc + " | SKIPPED: the pad pass only (a virtual pad)";
                yield break;
            }
            if (!g.sim.n24.padY || g.padReader == null || g.padReader.LayoutVersion != 4)
            {
                res[0] = pc + $" | RED: design.n24.padY is off or forced off (layout version {(g.padReader != null ? g.padReader.LayoutVersion : -1)}, not 4)";
                yield break;
            }
            var fails = new List<string>();
            foreach (string id in UnityTuning.StripSwapGuns) SlotOf(g, id);
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Gamepad pad = InputSystem.AddDevice<Gamepad>("ReelPadY24");
            offs.Add(() => InputSystem.RemoveDevice(pad));
            PlayerDevices.Last = InputDevice.Pad;
            var state = new GamepadState();
            GamepadButton yBtn = g.padReader.NextWeaponInputButton; // the live layout's NEXT WEAPON (v4: Y), not a hardcoded button
            int switches = 0, pulls = 0, dashes = 0;
            offs.Add(g.events.On(WeaponSwitchEvent.Name, _ => { switches++; FeelProbe.Event("weaponSwitch"); }));
            offs.Add(g.events.On("arclinePull", _ => { pulls++; FeelProbe.Event("arclinePull"); }));
            Action<MoverEvent> onMove = e => { if (e.type == MoverEventType.Dash) dashes++; };
            g.mover.Events += onMove;
            offs.Add(() => g.mover.Events -= onMove);
            yield return Frames(30);
            Start(g, "padweapons", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            g.forcedCombat = null; // the pad drives combat for the presses (the reel's IdleCombat would mask them)
            offs.Add(() => g.forcedCombat = IdleCombat);

            // Y taps
            int slot0 = g.loadout.targetSlot, moved = 0;
            for (int k = 0; k < 3; k++)
            {
                int was = g.loadout.targetSlot;
                FeelProbe.Event("yTap" + (k + 1));
                state = state.WithButton(yBtn, true);
                InputSystem.QueueStateEvent(pad, state);
                yield return Frames(4); // a tap (~0.07 s), well under the wheel's hold
                state = state.WithButton(yBtn, false);
                InputSystem.QueueStateEvent(pad, state);
                yield return Frames(Ms(500));
                if (g.loadout.targetSlot != was) moved++;
            }
            if (moved != 3) fails.Add($"Y taps moved the slot {moved} of 3 times ({slot0} -> {g.loadout.targetSlot})");
            if (switches != 3) fails.Add($"weaponSwitch counted {switches} of 3" + (moved == 3 ? " (the slot moved: ReaderInput.events is not wired: playerInput.events = events)" : ""));
            // NIGHT25 carry-over 7: the weapon card's cue answers each switch (a wipe, alpha never opaque, no first-frame pop, the new name at 18 px or more)
            if (Ui.WeaponSwitch25Count < switches) fails.Add($"the weapon card saw {Ui.WeaponSwitch25Count} of {switches} weaponSwitch events");
            if (Ui.WeaponSwitch25Peak <= 0f) fails.Add("the weaponSwitch wipe never drew (peak alpha 0)");
            if (Ui.WeaponSwitch25FirstAlpha >= 0.95f) fails.Add($"the wipe's first frame is at alpha {Ui.WeaponSwitch25FirstAlpha:0.00} (a pop-in)");
            if (Ui.WeaponSwitch25NamePx > 0 && Ui.WeaponSwitch25NamePx < 18f) fails.Add($"the switched gun's name is {Ui.WeaponSwitch25NamePx:0} px, under 18");

            // Y hold opens the wheel
            FeelProbe.Event("yHold");
            state = state.WithButton(yBtn, true);
            InputSystem.QueueStateEvent(pad, state);
            double hold = g.cfg.Num("design.pad.n20.wheelHold");
            yield return Frames(Ms((int)(hold * 1000) + 250));
            bool wheel = g.combatReader.select.radial.open;
            state = state.WithButton(yBtn, false);
            InputSystem.QueueStateEvent(pad, state);
            yield return Frames(Ms(300));
            bool closed = !g.combatReader.select.radial.open;
            if (!wheel) fails.Add($"a Y hold of {hold + 0.25:0.00} s did not open the wheel");
            if (!closed) fails.Add("the wheel stayed open after the Y release");

            // RB: SPECIAL + ARCLINE
            bool arc = g.sim.arcline21 != null && g.sim.n21.arcline;
            string rbNote;
            if (arc)
            {
                // NIGHT22's arcline2 setup: the gadget owned, the player on the safe spot, a husk in range straight ahead and the look ON it
                // (the yank needs a target in the crosshair), the husk free to move (the pull moves it), the press on the layout's GRAPPLE button.
                g.sim.arcline21.Own(true);
                g.Respawn(spot, spotYaw * 180 / Math.PI);
                g.forcedMove = null;
                yield return Frames(20);
                g.husks.KillAll();
                g.husks.frozen = true;
                Husk h = PlaceAt(g, 10, 0, out _);
                if (h != null) Look(g, h.AimPoint);
                yield return Frames(Ms(700)); // the chip is READY
                g.husks.frozen = false;
                int slotBefore = g.loadout.targetSlot;
                GamepadButton grapple = g.padReader.GrappleInputButton;
                FeelProbe.Event("rb");
                state = state.WithButton(grapple, true);
                InputSystem.QueueStateEvent(pad, state);
                yield return Frames(4);
                state = state.WithButton(grapple, false);
                InputSystem.QueueStateEvent(pad, state);
                yield return Frames(Ms(600));
                if (h == null) fails.Add("no husk placed 10 m ahead for the yank");
                if (pulls != 1) fails.Add($"RB: arclinePull {pulls}, wanted 1");
                if (g.loadout.targetSlot != slotBefore) fails.Add("RB changed the weapon (the scroll should be dropped)");
                rbNote = $"RB arclinePull {pulls}";
            }
            else
            {
                rbNote = "RB yank NOT RUN (ARCLINE block off: design.n21.arcline)";
            }

            // LB: DASH
            // open ground with a full bar (Respawn refills it), a run-up of the left stick forward, then the dash button the layout binds (LB), as arcline2's swing does
            g.husks.KillAll();
            g.husks.frozen = true;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.forcedCombat = IdleCombat;
            g.forcedMove = null;
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            GamepadButton dashBtn = g.padReader.InputButtonFor(PadAction.Dash);
            state = default;
            state.leftStick = new Vector2(0, 1);
            InputSystem.QueueStateEvent(pad, state);
            yield return Frames(Ms(300));
            FeelProbe.Event("lb");
            state = state.WithButton(dashBtn, true);
            InputSystem.QueueStateEvent(pad, state);
            yield return Frames(3);
            state = state.WithButton(dashBtn, false);
            InputSystem.QueueStateEvent(pad, state);
            yield return Frames(Ms(300));
            state = default;
            InputSystem.QueueStateEvent(pad, state);
            if (dashes < 1) fails.Add("LB did not dash");
            yield return Stop();
            res[0] = pc + $" | {FeelProbe.FrameCount} frames: Y taps moved {moved}/3, weaponSwitch {switches}, wheel {(wheel ? "opened" : "NOT opened")}/{(closed ? "closed" : "STUCK")}, {rbNote}, LB dashes {dashes}"
                + (fails.Count == 0 ? " | PASS" : " | RED: " + string.Join("; ", fails));
        }
    }
}
