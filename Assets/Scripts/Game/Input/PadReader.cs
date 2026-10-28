// NIGHT13 U3-26.7 (agent I; audit-input section 4 row 10): the gamepad, read once a rendered frame (keyboardMouse.js
// pollPad :338-409) into Core's PadState and run through the ported gamepad.js (Core/Gamepad.cs). In game its presses latch
// into the coming steps like keys (PadFrame, merged by ReaderInput's samples and consumed by the first step), its right
// stick turns the view at frame rate (ReaderInput adds it on top of the mouse: summed, never gated on the device), its
// commands come out as this frame's edges; in a menu only the menu input is filled.
// Unity: Gamepad.current (the last used; Gamepad.all[0] otherwise) for the web's first connected pad; the sticks through
// ReadUnprocessedValue (the layout's stick processor already dead-zones 0.125-0.925: never twice; the web's radial
// 0.15 / 0.95 runs once, in Core), y negated (Unity's up is +, the web's -1); every button's value with the web's
// TRIGGER_DOWN (not ButtonControl.isPressed's 0.5 press point). A pad the Input System sees only as a Joystick (no
// standard layout) is OddPad: the web's padOdd note (config ui.notices.padOdd), no input. The glyph set follows the
// device type (XInputController: Xbox; DualShockGamepad, DualSense included: PlayStation; else the web's neutral set).
// Nothing allocates per frame (the pressed list is a reused buffer).
// NIGHT15 U3-28.8 (builder K): Poll reads the device into `state`, Step reads `state` (the tests fill it by hand).
// - The weapon wheel on the pad (Design/unity-config.json design.pad; the web's picker is mouse-only): the NEXT WEAPON
//   button held design.pad.wheelHold s opens it (commands.wheelOpen); a shorter tap stays NEXT WEAPON, fired on the
//   release (listed in `pressed` then, never on the press); the release of a hold lets the wheel go (wheelClose).
//   A saved layout may bind WEAPON WHEEL (PadAction.Wheel) to its own button: that button opens it at once and its
//   release lets go, and NEXT WEAPON is then the web's (on the press). wheelX / wheelY carry the right stick (raw) that
//   points it (ReaderInput.ApplyPad, WeaponSelect.PointRadial). Without design.pad: the web's pad, no wheel.
// - The glyph set is worked out when the device changes: by type, then a Nintendo pad by its names (Glyphs.ForDevice).
// NIGHT20 U3-34.4 (builder P): with design.pad.n20 on (V3) the layout is v3 (RB NEXT WEAPON, Y SPECIAL + ARCLINE: the
// skygrind.padLayout.v3 store, a v2 migrated once); the wheel's hold above is then RB's, timed by design.pad.n20.wheelHold.
// InputButton maps a pad button to the Input System's (the smoke presses the live NEXT WEAPON button with it).
using Skygrind.Core;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

namespace Skygrind.Game
{
    /// <summary>The pad's settings (the GAMEPAD tab: config.settings' pad keys), read when the settings change.</summary>
    public struct PadSettings
    {
        public double inner, outer, sensX, sensY;
        public bool precise, invertY, vibration;
        public string aimAssist; // settings.padAimAssist: the pad's SNAP MODE

        /// <summary>config.settings' defaults.</summary>
        public static PadSettings FromConfig(GameConfig cfg) => new PadSettings
        {
            inner = cfg.Num("settings.padDeadzoneInner"),
            outer = cfg.Num("settings.padDeadzoneOuter"),
            precise = cfg.Str("settings.padCurve") == "precise",
            sensX = cfg.Num("settings.padSensX"),
            sensY = cfg.Num("settings.padSensY"),
            invertY = cfg.Bool("settings.padInvertY"),
            vibration = cfg.Bool("settings.padVibration"),
            aimAssist = cfg.Str("settings.padAimAssist"),
        };

        /// <summary>The live settings (the player's SETTINGS values).</summary>
        public static PadSettings FromStore(SettingsStore s) => new PadSettings
        {
            inner = s.Num("padDeadzoneInner"),
            outer = s.Num("padDeadzoneOuter"),
            precise = s.Str("padCurve") == "precise",
            sensX = s.Num("padSensX"),
            sensY = s.Num("padSensY"),
            invertY = s.Bool("padInvertY"),
            vibration = s.Bool("padVibration"),
            aimAssist = s.Str("padAimAssist"),
        };
    }

    /// <summary>This frame's pad commands in game (keyboardMouse.js pollPad :397-408): press edges (perkList: open on the press, close on the release).</summary>
    public struct PadCommands
    {
        public bool nextWeapon, prevWeapon, quickSwap, lockOn, xray, perkListOpen, perkListClose, pause;

        /// <summary>NIGHT15 U3-28.8: the pad's weapon wheel opens (held long enough, or its own button pressed) / is let go (take the pick).</summary>
        public bool wheelOpen, wheelClose;
    }

    public sealed class PadReader
    {
        public readonly PadState state = new PadState();

        /// <summary>The pad's part of the coming steps (mergePad's pad): ReaderInput merges it into each sample and clears the latches it consumed.</summary>
        public readonly PadFrame frame = new PadFrame();

        /// <summary>The button table in use (DEFAULT_PAD_LAYOUT, or the player's remap: InputPrefs.LoadPadLayout).</summary>
        public PadLayout layout = GamepadMath.DefaultLayout;

        public PadSettings settings;

        /// <summary>
        /// NIGHT19 U3-33.3: design.pad.n19.enabled (the v2 layout, L3 SPRINT, the bumpers' tabs, the drawn glyphs) and its
        /// l3Sprint ("toggle": the input layer's latch, ReaderInput; "hold": the button itself). Read once from the config;
        /// public so a test (or a later CONTROLS row) can set them.
        /// </summary>
        public bool n19, sprintToggle;

        /// <summary>NIGHT19: the v2 layout and its rules are live: design.pad.n19 on and not every block forced off (`--off19`,
        /// `-skygrindOff19`: NIGHT18's pad, the A side).</summary>
        public bool V2 => n19 && !N19Blocks.ForceOff;

        /// <summary>
        /// NIGHT20 U3-34.4: design.pad.n20.enabled (layout v3: RB NEXT WEAPON, a tap scrolls, held the wheel; Y SPECIAL +
        /// ARCLINE). Read once from the config; public so a test can set it.
        /// </summary>
        public bool n20;

        /// <summary>NIGHT20: the v3 layout is live: design.pad.n20 on over the v2 rules (V2: v3 is v2 with two rows swapped,
        /// L3 SPRINT and the drawn glyphs included), and the NIGHT20 blocks not forced off (`--off20`: NIGHT19's v2 pad).</summary>
        public bool V3 => n20 && V2 && !N20Blocks.ForceOff;

        /// <summary>NIGHT24: design.n24.padY.enabled (layout v4: Y NEXT WEAPON, a tap scrolls, held the wheel; RB SPECIAL + ARCLINE).
        /// Read once from the config; public so a test can set it.</summary>
        public bool padY;

        /// <summary>NIGHT24: the v4 layout is live: design.n24.padY on over the v3 rules, and the NIGHT24 blocks not forced off
        /// (`--off24`: NIGHT22's v3 pad).</summary>
        public bool V4 => padY && V3 && !N24Blocks.ForceOff;

        /// <summary>NIGHT25: design.n25.meleeFire.enabled (layout v5: PUNCH on RB so the quick melee can be pressed with both thumbs on the sticks and the
        /// trigger held). Read once from the config; public so a test can set it.</summary>
        public bool meleeFire;

        /// <summary>NIGHT25: the v5 layout is live: design.n25.meleeFire on over the v4 rules, and the NIGHT25 blocks not forced off (`--off25`: v4).</summary>
        public bool V5 => meleeFire && V4 && !N25Blocks.ForceOff;

        /// <summary>NIGHT20: the layout version in use (5, 4, 3 v3, 2 NIGHT19's v2, 1 the web's): the store and the RESET default.</summary>
        public int LayoutVersion => V5 ? 5 : V4 ? 4 : V3 ? 3 : V2 ? 2 : 1;

        /// <summary>NIGHT20: the default layout for the version in use (CONTROLS' RESET PAD).</summary>
        public PadLayout DefaultLayoutNow => GamepadMath.DefaultForVersion(LayoutVersion);

        /// <summary>NIGHT20: the NEXT WEAPON hold that opens the wheel (s): design.pad.n20.wheelHold on v3, else design.pad.wheelHold.</summary>
        public double WheelHoldNow => V3 && wheelHold3 > 0 ? wheelHold3 : wheelHold;

        readonly double wheelHold3; // s: design.pad.n20.wheelHold (0 when absent)

        bool layoutSynced; // the store `layout` was last loaded from (Poll: SyncLayout)
        int layoutVersion; // NIGHT20 / NIGHT24: 1, 2, 3 or 4 (LayoutVersion)

        readonly double yawRateDeg, yawRate, pitchRate, activity, menuDelay, menuEvery, menuStick, breakawayShare;
        public readonly double pitchLimit; // rad: config.mouse.pitchLimitDeg (degToRad, a double)
        readonly MenuNavState nav = new MenuNavState();
        readonly PadAction[] heldOrder = new PadAction[GamepadMath.ActionCount];
        uint prevHeld;
        int rawPrev;

        // NIGHT15 U3-28.8: the wheel (design.pad, read once; off without it), the layout the wheel's rule was worked out for,
        // and the hold in progress (NEXT WEAPON pressed, neither a tap nor a hold yet; what holds the wheel open).
        readonly bool wheelOn;
        readonly double wheelHold, wheelStickDeadzone; // s; the stick's tilt (0..1)
        PadLayout wheelLayout;
        bool nextHoldsWheel, nextPending, wheelByHold, wheelByButton;
        double nextHeldFor; // s
        Gamepad glyphPad; // the device `glyphs` was worked out for

        /// <summary>
        /// This frame's new presses in the layout's order (in game only; the pad's weapon commands run in this order).
        /// NIGHT15: a NEXT WEAPON that also holds the wheel is listed on its release (a tap), never on its press; WEAPON
        /// WHEEL is never listed (commands.wheelOpen / wheelClose).
        /// </summary>
        public readonly PadAction[] pressed = new PadAction[GamepadMath.ActionCount];

        public int pressedCount;

        /// <summary>A standard pad is connected; it changed this frame (input:padConnected); in game (merged, turning, commands).</summary>
        public bool Connected, ConnectedChanged, InGame;

        /// <summary>The pad was used this frame (padActivity past gamepad.activity): the device switches to it.</summary>
        public bool Active;

        /// <summary>Only a non-standard pad (a Joystick) is connected: the web's padOdd note.</summary>
        public bool OddPad;

        /// <summary>This frame's sticks (move axes and look rates, rad/s) in game; zero out of game.</summary>
        public Sticks sticks;

        /// <summary>This frame's menu input (menuNav), filled in every flow state while a pad is connected.</summary>
        public PadMenu menu;

        /// <summary>The standard buttons that went down this frame (bits by PadButtonName): CONTROLS' "press a pad button" (input:padButton).</summary>
        public int rawPressed;

        public PadCommands commands;

        /// <summary>The connected pad's glyph set (Neutral with none).</summary>
        public GlyphSet glyphs = GlyphSet.Neutral;

        /// <summary>The device read this frame (null: none): Rumble drives the same one.</summary>
        public Gamepad Device { get; private set; }

        public PadReader(GameConfig cfg)
        {
            yawRateDeg = cfg.Num("gamepad.yawRate");
            yawRate = GamepadMath.DegToRad(yawRateDeg);
            pitchRate = GamepadMath.DegToRad(cfg.Num("gamepad.pitchRate"));
            activity = cfg.Num("gamepad.activity");
            menuDelay = cfg.Num("gamepad.menuDelay");
            menuEvery = cfg.Num("gamepad.menuEvery");
            menuStick = cfg.Num("gamepad.menuStick");
            breakawayShare = cfg.Num("gamepad.breakawayShare");
            pitchLimit = GamepadMath.DegToRad(cfg.Num("mouse.pitchLimitDeg"));
            settings = PadSettings.FromConfig(cfg);
            wheelOn = cfg.Has("design.pad.wheelHold") && cfg.Has("design.pad.wheelStickDeadzone");
            wheelHold = wheelOn ? cfg.Num("design.pad.wheelHold") : 0;
            wheelStickDeadzone = wheelOn ? cfg.Num("design.pad.wheelStickDeadzone") : 0;
            n19 = cfg.Design("pad.n19.enabled");
            n20 = cfg.Design("pad.n20.enabled"); // NIGHT20 U3-34.4
            padY = cfg.Design("n24.padY.enabled"); // NIGHT24
            meleeFire = cfg.Design("n25.meleeFire.enabled"); // NIGHT25
            wheelHold3 = cfg.Has("design.pad.n20.wheelHold") ? cfg.Num("design.pad.n20.wheelHold") : 0;
            sprintToggle = !cfg.Has("design.pad.n19.l3Sprint") || cfg.Str("design.pad.n19.l3Sprint") != "hold"; // GG's L3 SPRINT default: TOGGLE
        }

        /// <summary>
        /// NIGHT19: the layout from the store that matches V2 (InputPrefs.LoadPadLayout(v2): the v2 store, migrating a v1
        /// once), on the first Poll and whenever V2 changes (`-skygrindOff19` is read after the game's Init has loaded the
        /// v1 store). A remap (CONTROLS) sets `layout` and saves it to the same store, so it stands.
        /// </summary>
        void SyncLayout()
        {
            int version = LayoutVersion; // NIGHT20: v3 too (InputPrefs.LoadPadLayout(3): the v3 store, migrating a v2 once)
            if (layoutSynced && layoutVersion == version) return;
            layoutSynced = true;
            layoutVersion = version;
            layout = InputPrefs.LoadPadLayout(version);
        }

        /// <summary>
        /// main.js :379 getBreakawayCap (deg/s): on the pad, gamepad.yawRate x settings.padSensX x gamepad.breakawayShare (a
        /// stick can't flick); +Infinity on the keyboard and mouse. Pass the method group once (cached) to AimAssist.
        /// </summary>
        public double BreakawayCap() => PlayerDevices.Last == InputDevice.Pad ? yawRateDeg * settings.sensX * breakawayShare : double.PositiveInfinity;

        /// <summary>main.js :378 getMode: the pad's SNAP MODE (settings.padAimAssist) while the pad is the device, else the mouse's.</summary>
        public string AimMode(string mouseMode) => PlayerDevices.Last == InputDevice.Pad ? settings.aimAssist : mouseMode;

        /// <summary>A fresh run or a menu: the pad's latches and held state go, so a press from before never fires (pollPad :380-385).</summary>
        public void Clear()
        {
            frame.held = 0;
            frame.latched = 0;
            frame.moveX = frame.moveZ = 0;
            sticks = default;
            pressedCount = 0;
            commands = default;
            wheelX = wheelY = 0;
            ResetWheel();
        }

        /// <summary>No hold in progress and no wheel held (a menu, a new run, the pad gone).</summary>
        void ResetWheel()
        {
            nextPending = wheelByHold = wheelByButton = false;
            nextHeldFor = 0;
        }

        /// <summary>The wheel's rule for the layout in use (worked out when the layout changes).</summary>
        void RefreshWheelLayout()
        {
            if (ReferenceEquals(layout, wheelLayout)) return;
            wheelLayout = layout;
            PadLayout l = layout ?? GamepadMath.DefaultLayout;
            PadButtonName? next = GamepadMath.ButtonFor(PadAction.NextWeapon, l), wheel = GamepadMath.ButtonFor(PadAction.Wheel, l);
            nextHoldsWheel = wheelOn && next.HasValue && (!wheel.HasValue || wheel.Value == next.Value);
        }

        static Gamepad Current()
        {
            Gamepad gp = Gamepad.current;
            if (gp != null) return gp;
            var all = Gamepad.all;
            return all.Count > 0 ? all[0] : null;
        }

        /// <summary>NIGHT15 U3-28.8: the right stick this frame in game (web axes: x right, y down; raw), for the pad's wheel.</summary>
        public double wheelX, wheelY;

        /// <summary>
        /// NIGHT15 U3-28.8: the NEXT WEAPON button also holds the wheel open (design.pad present, and no other button carries
        /// WEAPON WHEEL); false: NEXT WEAPON fires on the press (the web's).
        /// </summary>
        public bool NextHoldsWheel
        {
            get
            {
                RefreshWheelLayout();
                return nextHoldsWheel;
            }
        }

        /// <summary>NIGHT15 U3-28.8: design.pad.wheelStickDeadzone (the stick's tilt that points the wheel).</summary>
        public double WheelStickDeadzone => wheelStickDeadzone;

        /// <summary>Once a rendered frame, in every flow state: `dt` s since the last frame (frameDt), `inGame` the pad plays (merged, turning, commands).</summary>
        public void Poll(double dt, bool inGame)
        {
            SyncLayout(); // NIGHT19
            Gamepad gp = Current();
            Device = gp;
            bool was = Connected;
            Connected = gp != null;
            ConnectedChanged = was != Connected;
            OddPad = gp == null && Joystick.current != null;
            if (gp == null)
            {
                BeginFrame(inGame);
                frame.held = 0;
                prevHeld = 0;
                frame.moveX = frame.moveZ = 0;
                sticks = default;
                glyphs = GlyphSet.Neutral;
                glyphPad = null;
                ResetWheel();
                return;
            }
            if (!ReferenceEquals(gp, glyphPad))
            {
                glyphPad = gp; // a new device: its names are read once
                glyphs = Glyphs.ForDevice(gp is XInputController, gp is DualShockGamepad, gp.layout, gp.displayName, gp.description.product, gp.description.manufacturer);
            }
            Fill(state, gp);
            Step(dt, inGame);
        }

        void BeginFrame(bool inGame)
        {
            InGame = inGame;
            Active = false;
            menu = default;
            rawPressed = 0;
            pressedCount = 0;
            commands = default;
            wheelX = wheelY = 0;
        }

        /// <summary>
        /// The frame's reading of `state` (Poll fills it from the device first; the tests fill it by hand): activity, the
        /// sticks, the menu input, the held actions and this frame's presses and commands.
        /// </summary>
        public void Step(double dt, bool inGame)
        {
            BeginFrame(inGame);
            RefreshWheelLayout();
            Active = GamepadMath.PadActivity(state, activity);
            var s = new StickSettings
            {
                inner = settings.inner, outer = settings.outer, precise = settings.precise, sensX = settings.sensX, sensY = settings.sensY,
                invertY = settings.invertY, yawRate = yawRate, pitchRate = pitchRate,
            };
            Sticks read = GamepadMath.ReadSticks(state, s);
            menu = GamepadMath.MenuNav(state, nav, dt, menuDelay, menuEvery, menuStick);
            rawPressed = GamepadMath.RawPresses(state, rawPrev, out rawPrev);
            uint held = GamepadMath.HeldActions(state, layout, heldOrder, out int heldCount);
            int count = GamepadMath.PressedSince(heldOrder, heldCount, prevHeld, pressed);
            uint was = prevHeld;
            uint released = was & ~held;
            prevHeld = held;
            if (!inGame)
            {
                frame.held = 0;
                frame.latched = 0; // a press from before a pause or a menu never fires on resume
                frame.moveX = frame.moveZ = 0;
                sticks = default;
                ResetWheel(); // nor a hold (the menu closed the wheel without a pick: CombatReader.ReleaseAll)
                return;
            }
            frame.held = held;
            frame.moveX = read.moveX;
            frame.moveZ = read.moveZ;
            sticks = read;
            wheelX = state.axes[2];
            wheelY = state.axes[3];
            int n = 0;
            for (int i = 0; i < count; i++)
            {
                PadAction a = pressed[i];
                if (a == PadAction.NextWeapon && nextHoldsWheel)
                {
                    nextPending = true; // a tap (the release) or a hold (wheelHold): decided below, on a later frame
                    nextHeldFor = 0;
                    continue;
                }
                if (a == PadAction.Wheel)
                {
                    if (wheelOn && !nextHoldsWheel && !wheelByHold && !wheelByButton)
                    {
                        wheelByButton = true;
                        commands.wheelOpen = true;
                    }
                    continue;
                }
                pressed[n++] = a;
                uint bit = GamepadMath.Bit(a);
                if ((bit & GamepadMath.FrameButtons) != 0) frame.latched |= bit;
                else if (a == PadAction.NextWeapon) commands.nextWeapon = true;
                else if (a == PadAction.PrevWeapon) commands.prevWeapon = true;
                else if (a == PadAction.QuickSwap) commands.quickSwap = true;
                else if (a == PadAction.LockOn) commands.lockOn = true;
                else if (a == PadAction.Xray) commands.xray = true;
                else if (a == PadAction.PerkList) commands.perkListOpen = true;
                else if (a == PadAction.Pause) commands.pause = true;
            }
            if ((released & GamepadMath.Bit(PadAction.PerkList)) != 0) commands.perkListClose = true;
            if (nextHoldsWheel)
            {
                uint nextBit = GamepadMath.Bit(PadAction.NextWeapon);
                if (nextPending && (held & was & nextBit) != 0) // held since an earlier frame: the press frame starts the clock
                {
                    nextHeldFor += dt;
                    if (nextHeldFor >= WheelHoldNow) // NIGHT20: design.pad.n20.wheelHold on v3
                    {
                        nextPending = false;
                        wheelByHold = true;
                        commands.wheelOpen = true;
                    }
                }
                if ((released & nextBit) != 0)
                {
                    if (nextPending)
                    {
                        nextPending = false; // a tap: NEXT WEAPON now (the buffer has room: the released action isn't held)
                        pressed[n++] = PadAction.NextWeapon;
                        commands.nextWeapon = true;
                    }
                    if (wheelByHold)
                    {
                        wheelByHold = false;
                        commands.wheelClose = true;
                    }
                }
            }
            if (wheelByButton && (held & GamepadMath.Bit(PadAction.Wheel)) == 0)
            {
                wheelByButton = false;
                commands.wheelClose = true;
            }
            pressedCount = n;
        }

        /// <summary>The Input System pad as a standard-mapping Gamepad-like state (axes 0-3: lx, ly, rx, ry with up -1; buttons 0-15).</summary>
        static void Fill(PadState s, Gamepad gp)
        {
            UnityEngine.Vector2 l = gp.leftStick.ReadUnprocessedValue(), r = gp.rightStick.ReadUnprocessedValue();
            s.axes[0] = l.x;
            s.axes[1] = -l.y;
            s.axes[2] = r.x;
            s.axes[3] = -r.y;
            ReadButton(s, PadButtonName.A, gp.buttonSouth);
            ReadButton(s, PadButtonName.B, gp.buttonEast);
            ReadButton(s, PadButtonName.X, gp.buttonWest);
            ReadButton(s, PadButtonName.Y, gp.buttonNorth);
            ReadButton(s, PadButtonName.LB, gp.leftShoulder);
            ReadButton(s, PadButtonName.RB, gp.rightShoulder);
            ReadButton(s, PadButtonName.LT, gp.leftTrigger);
            ReadButton(s, PadButtonName.RT, gp.rightTrigger);
            ReadButton(s, PadButtonName.Select, gp.selectButton);
            ReadButton(s, PadButtonName.Start, gp.startButton);
            ReadButton(s, PadButtonName.LS, gp.leftStickButton);
            ReadButton(s, PadButtonName.RS, gp.rightStickButton);
            ReadButton(s, PadButtonName.Up, gp.dpad.up);
            ReadButton(s, PadButtonName.Down, gp.dpad.down);
            ReadButton(s, PadButtonName.Left, gp.dpad.left);
            ReadButton(s, PadButtonName.Right, gp.dpad.right);
            s.buttons[16] = default; // the home button: not on the Input System's Gamepad (padActivity skips it anyway)
        }

        /// <summary>
        /// NIGHT20 U3-34.4: a standard button as the Input System's GamepadButton (Fill's table read backwards), so a virtual
        /// pad (the smoke) presses the live binding: InputButton(GamepadMath.ButtonFor(PadAction.NextWeapon, layout) ?? Y).
        /// </summary>
        public static GamepadButton InputButton(PadButtonName b)
        {
            switch (b)
            {
                case PadButtonName.A: return GamepadButton.South;
                case PadButtonName.B: return GamepadButton.East;
                case PadButtonName.X: return GamepadButton.West;
                case PadButtonName.Y: return GamepadButton.North;
                case PadButtonName.LB: return GamepadButton.LeftShoulder;
                case PadButtonName.RB: return GamepadButton.RightShoulder;
                case PadButtonName.LT: return GamepadButton.LeftTrigger;
                case PadButtonName.RT: return GamepadButton.RightTrigger;
                case PadButtonName.Select: return GamepadButton.Select;
                case PadButtonName.Start: return GamepadButton.Start;
                case PadButtonName.LS: return GamepadButton.LeftStick;
                case PadButtonName.RS: return GamepadButton.RightStick;
                case PadButtonName.Up: return GamepadButton.DpadUp;
                case PadButtonName.Down: return GamepadButton.DpadDown;
                case PadButtonName.Left: return GamepadButton.DpadLeft;
                default: return GamepadButton.DpadRight;
            }
        }

        /// <summary>NIGHT20: the button the live layout's NEXT WEAPON is on (Y without one), as the Input System's.</summary>
        public GamepadButton NextWeaponInputButton => InputButton(GamepadMath.ButtonFor(PadAction.NextWeapon, layout ?? GamepadMath.DefaultLayout) ?? PadButtonName.Y);

        /// <summary>NIGHT22 U3-36.3 (builder M): the button the live layout's GRAPPLE (the ARCLINE) is on, as the Input System's (v3: Y, which also carries
        /// SPECIAL; v2: RB; the web's: LS); with no binding, the v3 default's. The reel's virtual pad presses it.</summary>
        public GamepadButton GrappleInputButton => InputButtonFor(PadAction.Grapple);

        /// <summary>NIGHT22: the button the live layout binds `a` to, as the Input System's (the v3 default's when the layout has none; Y at the last).</summary>
        public GamepadButton InputButtonFor(PadAction a) => InputButton(GamepadMath.ButtonFor(a, layout ?? GamepadMath.DefaultLayout) ?? GamepadMath.ButtonFor(a, GamepadMath.DefaultLayoutV3) ?? PadButtonName.Y);

        static void ReadButton(PadState s, PadButtonName b, ButtonControl c)
        {
            double v = c.ReadValue();
            bool was = s.buttons[(int)b].pressed;
            // NIGHT27: hysteresis (GamepadMath.TriggerUp): down at TriggerDown, up only below TriggerUp, so a chattering real trigger holds
            s.buttons[(int)b] = new PadButton { pressed = v >= GamepadMath.TriggerDown || (was && v > GamepadMath.TriggerUp), value = v };
        }
    }
}
