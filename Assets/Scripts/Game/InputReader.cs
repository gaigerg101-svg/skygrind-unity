// Keyboard + mouse -> InputFrame, after skygrind/src/input/keyboardMouse.js: look updates at input rate (every
// frame here), presses are latched and consumed by the next fixed step, so a press between steps is never lost.
// NIGHT13 U3-26.7 (agent I): the keys come from the live bindings (Core/Bindings.cs, config.input.bindings; rebound on
// CONTROLS), resolved through KeyCodes when the store's version moves (no lookup per frame): forward / back / left /
// right, jump, dash, slide, grapple, reel; right shift is always a second dash (keyboardMouse.js :45-51, :133: it
// latches dash unless a rebind gave it to another button action, and holds dash always). Without a store (the Arena
// scene) the web's defaults as they were hard-coded: WASD, Space, Shift, C, G, Z. While the radial is open the mouse
// moves its pointer, not the view (lookPaused, :221). The mouse look's maths are unchanged.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public sealed class InputReader
    {
        readonly MovementConfig config;
        public double yaw;
        public double pitch;
        public double sensMult = 1; // x the look sensitivity (the ADS zoom: ads.js sensitivity, the tangent ratio)
        bool jumpLatch, dashLatch, slideLatch, grappleLatch, reelLatch;

        /// <summary>The live key bindings (null: the web's defaults, as hard-coded before NIGHT13).</summary>
        public Bindings bindings;

        /// <summary>The radial holds the mouse this frame (keyboardMouse.js :221: the view holds still). Set before Poll.</summary>
        public bool lookPaused;

        Key forward = Key.W, back = Key.S, left = Key.A, right = Key.D, jump = Key.Space, dash = Key.LeftShift, slide = Key.C, grapple = Key.G, reel = Key.Z;
        bool shiftRightDashes = true; // keyboardMouse.js buttonOf: right shift latches dash unless bound to another button action
        int keysVersion = -1;

        public InputReader(MovementConfig config) => this.config = config;

        public bool Locked => Cursor.lockState == CursorLockMode.Locked;

        /// <summary>The bound keys again when the store changed (a rebind, RESET, a load).</summary>
        void RefreshKeys()
        {
            if (bindings == null || bindings.Version == keysVersion) return;
            keysVersion = bindings.Version;
            forward = KeyCodes.Bound(bindings, "forward");
            back = KeyCodes.Bound(bindings, "back");
            left = KeyCodes.Bound(bindings, "left");
            right = KeyCodes.Bound(bindings, "right");
            jump = KeyCodes.Bound(bindings, "jump");
            dash = KeyCodes.Bound(bindings, "dash");
            slide = KeyCodes.Bound(bindings, "slide");
            grapple = KeyCodes.Bound(bindings, "grapple");
            reel = KeyCodes.Bound(bindings, "reel");
            shiftRightDashes = !IsKeyButton(bindings.ActionOf("ShiftRight"));
        }

        /// <summary>keyboardMouse.js KEY_BUTTONS (:16): the bound keys that are InputFrame buttons.</summary>
        static bool IsKeyButton(string action) =>
            action == "jump" || action == "dash" || action == "slide" || action == "special" || action == "punch" || action == "interact" || action == "grapple" || action == "reel";

        /// <summary>Once per rendered frame, after the Input System has processed the frame's events.</summary>
        public void Poll()
        {
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (kb == null || mouse == null) return;
            if (!Locked)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                return;
            }
            if (kb.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                jumpLatch = dashLatch = slideLatch = grappleLatch = reelLatch = false;
                return;
            }
            if (!lookPaused)
            {
                Vector2 d = mouse.delta.ReadValue();
                double sens = config.settings.sensitivity * sensMult;
                double limit = config.mouse.pitchLimitDeg * Mathf.Deg2Rad;
                yaw -= d.x * sens;
                // Web movementY is + downward and pitch − looks down; Unity's delta.y is + upward.
                pitch = System.Math.Max(-limit, System.Math.Min(limit, pitch + d.y * sens * (config.settings.invertY ? -1 : 1)));
            }
            RefreshKeys();
            if (KeyCodes.Pressed(kb, jump)) jumpLatch = true;
            if (KeyCodes.Pressed(kb, dash) || (shiftRightDashes && kb.rightShiftKey.wasPressedThisFrame)) dashLatch = true;
            if (KeyCodes.Pressed(kb, slide)) slideLatch = true;
            if (KeyCodes.Pressed(kb, grapple)) grappleLatch = true;
            if (KeyCodes.Pressed(kb, reel)) reelLatch = true;
        }

        /// <summary>One InputFrame per fixed step. Consumes the latched presses. Neutral while the cursor is free.</summary>
        public InputFrame Sample()
        {
            Keyboard kb = Keyboard.current;
            bool active = Locked && kb != null;
            var f = new InputFrame { yaw = yaw, pitch = pitch };
            if (active)
            {
                RefreshKeys();
                f.moveX = (KeyCodes.Held(kb, right) ? 1 : 0) - (KeyCodes.Held(kb, left) ? 1 : 0);
                f.moveZ = (KeyCodes.Held(kb, forward) ? 1 : 0) - (KeyCodes.Held(kb, back) ? 1 : 0);
                f.jump = new Button { held = KeyCodes.Held(kb, jump), pressed = jumpLatch };
                f.dash = new Button { held = KeyCodes.Held(kb, dash) || kb.rightShiftKey.isPressed, pressed = dashLatch };
                f.slide = new Button { held = KeyCodes.Held(kb, slide), pressed = slideLatch };
                f.grapple = new Button { held = KeyCodes.Held(kb, grapple), pressed = grappleLatch };
                f.reel = new Button { held = KeyCodes.Held(kb, reel), pressed = reelLatch };
            }
            jumpLatch = dashLatch = slideLatch = grappleLatch = reelLatch = false;
            return f;
        }

        public void SetLook(double newYaw, double newPitch)
        {
            yaw = newYaw;
            pitch = newPitch;
        }
    }
}
