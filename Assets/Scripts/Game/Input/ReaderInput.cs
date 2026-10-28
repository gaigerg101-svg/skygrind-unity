// NIGHT13 U3-26.3: the player's devices as the sim's IPlayerInput: InputReader (keyboard + mouse movement and the look)
// and CombatReader (keyboard + mouse combat), polled by SurvivalGame once per rendered frame. forcedMove / forcedCombat are
// the older automation overrides (the slice smoke's): while set they replace the movement axes and buttons (the look stays
// the reader's) and the combat input of each step, exactly as SurvivalGame.SimStep applied them before.
// NIGHT13 U3-26.7 (agent I): the pad merged in behind the same interface (keyboardMouse.js :283-301, pollPad; main.js
// :1796-1811, :2444-2447). Poll, once a rendered frame in every flow state and before the steps: the device used last (a
// key, a click or a mouse move over 2 px: the keyboard; the pad's activity: the pad, read after them as the web's pollPad
// runs after the DOM's events), the pad (PadReader), then in game the keyboard and mouse readers (the radial first: it
// takes the mouse), the pad's weapon commands (Y / d-pad, in its press order) and the stick's look on top of the mouse's
// in the same frame (summed; skipped while the radial is open). Each step's samples: the keyboard's frame, the pad merged
// (MergeMove / MergeCombat: no pad, or a pad at rest, leaves it bit for bit) and its latches consumed, then hold / toggle on
// the player's own input only (never on forcedMove), then the inactive (DOWN) neutral frame. PlayerDevices is written
// every Poll for the UI. With no pad, HOLD and no radial open, every sample equals the readers' as before.
// NIGHT15 U3-28.8 (builder K): the pad's part of Poll is ApplyPad (the tests drive it after PadReader.Step): the weapon
// commands, then the pad's weapon wheel (open on PadReader's wheelOpen, the right stick points through
// WeaponSelect.PointRadial, the release takes the pick), then the stick's look, skipped while any radial is open (the
// view holds still, as the mouse's radial holds it through InputReader.lookPaused). RUMBLE (settings.padRumble) is
// applied with VIBRATION.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public sealed class ReaderInput : IPlayerInput
    {
        public readonly InputReader move;
        public readonly CombatReader combat;

        /// <summary>The pad (null: keyboard and mouse only, as before NIGHT13).</summary>
        public readonly PadReader pad;

        /// <summary>The pad's rumble (null without a pad reader). The lead calls Hurt / Kill / Parry and Stop.</summary>
        public readonly Rumble rumble;

        public InputFrame? forcedMove;
        public CombatInput? forcedCombat;
        /// <summary>NIGHT18 (the reel, FeelReel): only SetLook moves the look; the mouse's delta and the pad's sticks are ignored
        /// (a stale mouse delta or a drifting stick moved the camera and the aim every frame on the reel's fixed clock).</summary>
        public bool lookHeld;

        /// <summary>NIGHT24: the game's event bus, for the pad's "weaponSwitch" (null in tests and before the lead wires it: nothing emitted).</summary>
        public EventBus events;

        /// <summary>Hold / toggle's latches (holdToggle.js createToggleState): reset per run (ResetRun / ResetToggles).</summary>
        public readonly ToggleState toggles = new ToggleState();

        /// <summary>settings.slideMode / arclineMode == 'toggle' (ApplySettings).</summary>
        public bool slideToggle, arclineToggle;

        /// <summary>The player's Mover: the rope phase before each step (applyToggles' ctx.ropePhase). Null: 'none'.</summary>
        public Mover mover;

        /// <summary>The device used last changed this frame (main.js input:device: prompts and glyphs follow).</summary>
        public bool DeviceChanged { get; private set; }

        public ReaderInput(InputReader move, CombatReader combat) : this(move, combat, null, null)
        {
        }

        public ReaderInput(InputReader move, CombatReader combat, PadReader pad, Rumble rumble)
        {
            this.move = move;
            this.combat = combat;
            this.pad = pad;
            this.rumble = rumble;
        }

        public double Yaw => move.yaw;
        public double Pitch => move.pitch;
        public bool Captured => move.Locked;

        public void SetLook(double yaw, double pitch) => move.SetLook(yaw, pitch);

        /// <summary>The settings this layer reads (call on every settings change): SLIDE / ARCLINE hold or toggle, the pad's GAMEPAD tab, VIBRATION, RUMBLE.</summary>
        public void ApplySettings(SettingsStore s)
        {
            slideToggle = s.Str("slideMode") == "toggle";
            arclineToggle = s.Str("arclineMode") == "toggle";
            if (pad != null)
            {
                pad.settings = PadSettings.FromStore(s);
                pad.sprintToggle = ControlsPad20.SprintToggle(s, pad.sprintToggle); // NIGHT20 row 6: L3 SPRINT TOGGLE / HOLD (the row, when SETTINGS has it)
            }
            if (rumble != null)
            {
                rumble.enabled = s.Bool("padVibration");
                // NIGHT15 U3-28.8: RUMBLE (absent without design.pad: the constructor's strength stays).
                if (s.values.TryGetValue(UnityTuning.PadRumbleSetting, out object r) && r is double strength) rumble.strength = strength;
                if (!rumble.enabled || rumble.strength <= 0) rumble.Stop();
            }
        }

        /// <summary>NIGHT19 U3-33.3: L3 TOGGLE's latch (the sprint key held by a click; reset with the toggles).</summary>
        public bool sprintLatch;

        /// <summary>A toggled slide or swing never carries over (main.js respawnAt :853).</summary>
        public void ResetToggles()
        {
            toggles.Reset();
            sprintLatch = false;
        }

        /// <summary>A new run (main.js respawnAt): the toggles, slot `slot` in hand with no last weapon, no latch left on the pad or the keys.</summary>
        public void ResetRun(int slot)
        {
            toggles.Reset();
            sprintLatch = false;
            combat.ResetRun(slot);
            pad?.Clear();
            press71.Reset(); // NIGHT71 L10 press71: nothing carried into a run
        }

        /// <summary>NIGHT71 L10 press71's delivery state (Press71.cs; used only with -skygrindPress71).</summary>
        readonly Press71 press71 = new Press71();

        void SetDevice(InputDevice d)
        {
            if (PlayerDevices.Last == d) return;
            PlayerDevices.Last = d;
            DeviceChanged = true;
        }

        /// <summary>
        /// Once per rendered frame, in every flow state, before the steps (main.js :2444-2447): `frameDt` the frame's time
        /// (min(unscaled dt, loop.maxFrameDelta)), `inGame` the player plays (Playing or Down: the keyboard and mouse readers
        /// run, the pad merges, turns and runs its commands; else only its menu input), the loadout's slot and owned count.
        /// Set move.sensMult (the zoom x the lock's friction) first: the stick's look uses it too (main.js setLookScale).
        /// </summary>
        public void Poll(double frameDt, bool inGame, int currentSlot, int slotCount)
        {
            DeviceChanged = false;
            // The device rules (keyboardMouse.js :160 keydown, :204 mousedown, :220 a real mouse move, not a desk bump).
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;
            bool keyboardUsed = kb != null && kb.anyKey.wasPressedThisFrame;
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame) keyboardUsed = true;
                if (move.Locked)
                {
                    Vector2 d = mouse.delta.ReadValue();
                    if (System.Math.Abs(d.x) + System.Math.Abs(d.y) > GamepadMath.MouseDeviceMove) keyboardUsed = true;
                }
            }
            if (keyboardUsed) SetDevice(InputDevice.KeyboardMouse);
            if (pad != null)
            {
                pad.Poll(frameDt, inGame);
                if (pad.Active) SetDevice(InputDevice.Pad);
                else if (pad.ConnectedChanged && !pad.Connected) SetDevice(InputDevice.KeyboardMouse);
            }
            if (inGame)
            {
                combat.Poll(move.Locked, currentSlot, slotCount); // the radial takes the mouse first
                move.lookPaused = combat.RadialTookMouse || lookHeld;
                move.Poll();
                ApplyPad(frameDt, slotCount);
            }
            else combat.ReleaseAll();
            rumble?.Tick(frameDt);
            PlayerDevices.Glyphs = pad != null ? pad.glyphs : GlyphSet.Neutral;
            PlayerDevices.PadConnected = pad != null && pad.Connected;
            PlayerDevices.Layout = pad?.layout; // NIGHT19: PromptText's live binding
            PlayerDevices.N19 = pad != null && pad.V2;
            PlayerDevices.Bindings = move.bindings;
            if (PlayerDevices.N19) PromptGlyphs.Boot(); // NIGHT19: the drawn glyphs, built once (the first frame with the block on)
            PadMenu m = pad != null && pad.Connected ? pad.menu : default;
            PlayerDevices.Menu = new MenuInput
            {
                up = m.move < 0, down = m.move > 0, left = m.side < 0, right = m.side > 0, confirm = m.confirm, back = m.back, start = m.start,
                tabPrev = PlayerDevices.N19 && m.tab < 0, tabNext = PlayerDevices.N19 && m.tab > 0, // NIGHT19: LB / RB (v2 only)
            };
        }

        /// <summary>
        /// In game, after the keyboard and mouse readers (Poll; the tests call it after PadReader.Step): the pad's weapon
        /// commands in its press order, then the stick's look (skipped while the radial is open).
        /// </summary>
        public void ApplyPad(double frameDt, int slotCount)
        {
            if (pad == null) return;
            WeaponSelect sel = combat.select;
            if (!pad.Connected)
            {
                // NIGHT15: a pad pulled out with its wheel open: shut without a pick (as a lost focus).
                if (sel.radial.open && sel.radial.pad)
                {
                    sel.CloseRadial(false, slotCount);
                    combat.radial = sel.radial;
                }
                return;
            }
            for (int i = 0; i < pad.pressedCount; i++)
            {
                PadAction a = pad.pressed[i];
                if (a == PadAction.NextWeapon)
                {
                    int fromSlot = sel.weapon;
                    // NIGHT24 (design.n24.padY): the Y tap on layout v4 is announced as the port-only "weaponSwitch" (the bus is wired by the lead; null: silent)
                    if (sel.Next(slotCount) && events != null && pad.V4) events.Emit(WeaponSwitchEvent.Name, new WeaponSwitchEvent { fromSlot = fromSlot, toSlot = sel.weapon });
                }
                else if (a == PadAction.PrevWeapon) sel.Prev(slotCount);
                else if (a == PadAction.QuickSwap) sel.QuickSwap(slotCount);
            }
            // NIGHT15 U3-28.8: the pad's wheel (PadReader: NEXT WEAPON held, or its own button): open, the right stick points,
            // the release takes the pick. OpenRadial refuses while the keyboard's radial is open (that one stays the mouse's).
            if (pad.commands.wheelOpen) sel.OpenRadial(slotCount, pad: true);
            if (sel.radial.open && sel.radial.pad)
            {
                sel.PointRadial(pad.wheelX, pad.wheelY, pad.WheelStickDeadzone, slotCount);
                if (pad.commands.wheelClose) sel.CloseRadial(true, slotCount);
            }
            combat.radial = sel.radial;
            if (!combat.radial.open && !lookHeld) GamepadMath.ApplyLook(ref move.yaw, ref move.pitch, pad.sticks, move.sensMult, frameDt, pad.pitchLimit);
        }

        public InputFrame SampleMove(bool active)
        {
            InputFrame frame = move.Sample(); // the keyboard's (neutral while the cursor is free); consumes its latches
            if (pad != null)
            {
                GamepadMath.MergeMove(ref frame, pad.Connected && pad.InGame ? pad.frame : null);
                pad.frame.latched &= ~GamepadMath.MoveButtons;
            }
            toggles.tapSteps = HoldToggle.grappleTapSteps; // NIGHT31 G: the run's TOGGLE tap window (0 with design.n31.grapplePull off: NIGHT30's toggle)
            if (!forcedMove.HasValue) HoldToggle.ApplyToggles(ref frame, toggles, slideToggle, arclineToggle, mover != null ? mover.rope.phase : RopePhase.None);
            // NIGHT19 U3-33.3: L3 SPRINT TOGGLE (design.pad.n19.l3Sprint): the click latches the sprint key until the stick
            // centres, the bar empties (the run's Mover) or another click; HOLD leaves the merged button as it is.
            if (pad != null && pad.V2 && pad.sprintToggle && !forcedMove.HasValue && active)
                GamepadMath.ApplySprintToggle(ref frame, ref sprintLatch, mover != null && GamepadMath.SprintBarEmpty(mover.stamina));
            else sprintLatch = false;
            if (!active) return new InputFrame { yaw = frame.yaw, pitch = frame.pitch }; // DOWN: no control, the look kept
            if (forcedMove.HasValue)
            {
                InputFrame fm = forcedMove.Value;
                fm.yaw = frame.yaw;
                fm.pitch = frame.pitch;
                frame = fm;
            }
            return frame;
        }

        public CombatInput SampleCombat(bool active)
        {
            CombatInput c = combat.Sample(active && Captured);
            if (!active) c.weapon = -1; // a selection reaches the loadout only while playing
            if (pad != null)
            {
                if (active) GamepadMath.MergeCombat(ref c, pad.Connected && pad.InGame ? pad.frame : null);
                pad.frame.latched &= ~GamepadMath.CombatButtons;
            }
            // NIGHT71 L10 press71 (Press71.cs; -skygrindPress71, off by default): fire's press never on two steps in a row (a press latched for
            // the step after a pressed one, e.g. inside a freeze that step started, waits one step instead of merging with it)
            if (Press71.On && !forcedCombat.HasValue)
            {
                if (!active) press71.Reset();
                else c.fire.pressed = press71.Deliver(c.fire.pressed);
            }
            if (forcedCombat.HasValue) c = forcedCombat.Value;
            return c;
        }
    }
}
