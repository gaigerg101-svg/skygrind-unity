// Mouse + keyboard -> CombatInput, after skygrind/src/input/keyboardMouse.js (config.input.bindings): fire (left mouse),
// ADS / alt fire (right mouse), the special (B, input.bindings.special: KeyB; NIGHT10), punch (F), buy (E, held). Presses
// are latched every rendered frame and consumed by the next fixed step, so a press between steps is never lost.
// NIGHT13 U3-26.7 (agent I): the keys come from the live bindings (special, punch, interact, quickSwap, radial, and the
// bound commands lockOn / xray / restart / perkList: keyboardMouse.js :29-33, :159-201), resolved through KeyCodes when the
// store's version moves; the weapon selection is WeaponSelect (keyboardMouse.js's: 1-9 and 0, the wheel with
// weapons.wheelCooldown, quick swap, the hold-Tab radial with the mouse pointing, the pad's Y / d-pad cycle through
// ReaderInput). Until Init (no config: the Arena scene, old callers) the keys are the web's defaults as hard-coded before
// and there is no radial and no wheel cooldown.
// NIGHT15 U3-28.8 (builder K): the pad's weapon wheel shares the radial (RadialState.pad): the cursor let go doesn't
// close it (ReleaseCursor), the mouse doesn't point it but still holds the view, and the radial key's release closes only
// the keyboard's; a menu (ReleaseAll) closes either without a pick.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    /// <summary>This frame's bound keyboard commands (keyboardMouse.js BOUND_COMMANDS / BOUND_HOLDS :32-33): edges, cursor captured only.</summary>
    public struct KeyCommands
    {
        public bool lockOn, xray, restart, perkListOpen, perkListClose;
    }

    public sealed class CombatReader
    {
        bool fireLatch, altLatch, specialLatch, punchLatch, interactLatch;

        /// <summary>NIGHT13 (the shared types): the radial wheel for the HUD (filled every Poll; UiRadial draws it).</summary>
        public RadialState radial = new RadialState { highlighted = -1 };

        /// <summary>The weapon selection (keyboardMouse.js weapon / lastWeapon / radial): the slot request each step takes.</summary>
        public WeaponSelect select = new WeaponSelect(0, 0, 0);

        /// <summary>This frame's bound commands (the lead routes them: assist.ToggleLock, the X-ray toggle, Restart, the perk list).</summary>
        public KeyCommands Commands;

        /// <summary>The radial took this frame's mouse movement: the view holds still (InputReader.lookPaused).</summary>
        public bool RadialTookMouse;

        Bindings bindings;
        int keysVersion = -1;
        Key special = Key.B, punch = Key.F, interact = Key.E, quickSwap = Key.Q, radialKey = Key.None, lockOn = Key.None, xray = Key.None, restart = Key.None, perkList = Key.None;

        static readonly Key[] DigitKeys = { Key.Digit0, Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9 };

        /// <summary>The config (weapons.wheelCooldown, weapons.radial) and the live bindings (null: config.input.bindings' defaults).</summary>
        public void Init(GameConfig cfg, Bindings liveBindings)
        {
            select = WeaponSelect.From(cfg);
            bindings = liveBindings;
            if (bindings == null)
            {
                bindings = new Bindings(cfg);
                bindings.Load(null);
            }
            keysVersion = -1;
        }

        /// <summary>A new run: slot `slot` in hand, the last weapon forgotten, nothing latched (keyboardMouse.js selectWeapon resetLast; main.js respawnAt).</summary>
        public void ResetRun(int slot)
        {
            select.ResetRun(slot);
            ReleaseAll();
        }

        /// <summary>
        /// keyboardMouse.js releaseAll (:116-122): the cursor let go (or a menu took over): the latches clear and the radial
        /// closes without a pick; this frame's commands and the radial's mouse are cleared.
        /// </summary>
        public void ReleaseAll()
        {
            fireLatch = altLatch = specialLatch = punchLatch = interactLatch = false;
            int count = select.radial.slots;
            select.CloseRadial(false, count);
            Commands = default;
            RadialTookMouse = false;
            radial = select.radial;
        }

        /// <summary>
        /// NIGHT15 U3-28.8: the cursor let go (or no keyboard / mouse at all) while still in game: ReleaseAll but for the pad's
        /// wheel, which isn't the cursor's (a pad plays with the cursor free; a console has neither device).
        /// </summary>
        void ReleaseCursor()
        {
            if (!select.radial.pad)
            {
                ReleaseAll();
                return;
            }
            fireLatch = altLatch = specialLatch = punchLatch = interactLatch = false;
            Commands = default;
            RadialTookMouse = false;
            radial = select.radial;
        }

        void RefreshKeys()
        {
            if (bindings == null || bindings.Version == keysVersion) return;
            keysVersion = bindings.Version;
            special = KeyCodes.Bound(bindings, "special");
            punch = KeyCodes.Bound(bindings, "punch");
            interact = KeyCodes.Bound(bindings, "interact");
            quickSwap = KeyCodes.Bound(bindings, "quickSwap");
            radialKey = KeyCodes.Bound(bindings, "radial");
            lockOn = KeyCodes.Bound(bindings, "lockOn");
            xray = KeyCodes.Bound(bindings, "xray");
            restart = KeyCodes.Bound(bindings, "restart");
            perkList = KeyCodes.Bound(bindings, "perkList");
        }

        /// <summary>
        /// Once per rendered frame (with InputReader.Poll, before it: the radial takes the mouse first), only while the
        /// cursor is locked; `currentSlot` the loadout's slot (a wall buy's equip is remembered), `slotCount` the owned guns.
        /// </summary>
        public void Poll(bool locked, int currentSlot, int slotCount)
        {
            Commands = default;
            RadialTookMouse = false;
            select.Sync(currentSlot, slotCount);
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (!locked || kb == null || mouse == null)
            {
                ReleaseCursor();
                return;
            }
            RefreshKeys();
            if (mouse.leftButton.wasPressedThisFrame) fireLatch = true;
            if (mouse.rightButton.wasPressedThisFrame) altLatch = true;
            if (KeyCodes.Pressed(kb, special)) specialLatch = true;
            if (KeyCodes.Pressed(kb, punch)) punchLatch = true;
            if (KeyCodes.Pressed(kb, interact)) interactLatch = true;
            for (int d = 1; d <= 10; d++)
            {
                int digit = d % 10; // Digit1 .. Digit9, then Digit0 (the tenth slot)
                if (kb[DigitKeys[digit]].wasPressedThisFrame) select.Digit(digit, slotCount);
            }
            if (KeyCodes.Pressed(kb, quickSwap)) select.QuickSwap(slotCount);
            if (KeyCodes.Pressed(kb, radialKey)) select.OpenRadial(slotCount);
            if (KeyCodes.Pressed(kb, lockOn)) Commands.lockOn = true;
            if (KeyCodes.Pressed(kb, xray)) Commands.xray = true;
            if (KeyCodes.Pressed(kb, restart)) Commands.restart = true;
            if (KeyCodes.Pressed(kb, perkList)) Commands.perkListOpen = true;
            if (KeyCodes.Released(kb, perkList)) Commands.perkListClose = true;
            // The wheel: Unity's scroll is + up, the web's deltaY + down.
            float wheel = mouse.scroll.ReadValue().y;
            if (wheel != 0) select.Wheel(-wheel, FeelClock.Now, slotCount); // NIGHT18: FeelClock
            if (select.radial.open)
            {
                // The radial takes the mouse (the view holds still); Unity's delta.y is + up, movementY + down. NIGHT15: the
                // pad's wheel is pointed by the right stick (ReaderInput.ApplyPad); the mouse only holds still there.
                if (!select.radial.pad)
                {
                    Vector2 m = mouse.delta.ReadValue();
                    select.MoveRadial(m.x, -m.y, slotCount);
                }
                RadialTookMouse = true;
            }
            if (KeyCodes.Released(kb, radialKey) && !select.radial.pad) select.CloseRadial(true, slotCount);
            radial = select.radial;
        }

        /// <summary>One CombatInput per fixed step; consumes the latched presses. Neutral while the cursor is free, but for a weapon selection (the pad's).</summary>
        public CombatInput Sample(bool locked)
        {
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;
            var c = new CombatInput { weapon = -1 };
            int slot = select.Consume();
            if (locked && kb != null && mouse != null)
            {
                RefreshKeys();
                c.fire = new Button { held = mouse.leftButton.isPressed, pressed = fireLatch };
                c.altFire = new Button { held = mouse.rightButton.isPressed, pressed = altLatch };
                c.special = new Button { held = KeyCodes.Held(kb, special), pressed = specialLatch };
                c.punch = new Button { held = KeyCodes.Held(kb, punch), pressed = punchLatch };
                c.interact = new Button { held = KeyCodes.Held(kb, interact), pressed = interactLatch };
            }
            c.weapon = slot; // NIGHT13 review: outside the lock (the pad's Next / Prev move the selection without the cursor; the digits latch only while locked)
            fireLatch = altLatch = specialLatch = punchLatch = interactLatch = false;
            return c;
        }
    }
}
