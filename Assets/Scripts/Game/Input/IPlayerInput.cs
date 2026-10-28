// NIGHT13 U3-26.3 (the shared types): the sim's view of the player's input (audit-perf section 5 move 5). The sim samples
// one movement frame and one combat input per fixed step through this interface and owns nothing about devices; the
// look (yaw, pitch) lives behind it too, turned at frame rate by the devices and per step by the aim assist (SetLook).
// Implementations: ReaderInput (keyboard + mouse through InputReader / CombatReader, with the pad merged in by U3-26.7),
// ScriptedInput (automation: the fingerprint, the bench's play scenario, the headless tests).
// The device / glyph / radial types the input layer publishes to the UI are here too. No UnityEngine use.
using Skygrind.Core;

namespace Skygrind.Game
{
    public interface IPlayerInput
    {
        /// <summary>The look: rad, yaw 0 looks toward -z, + pitch looks up.</summary>
        double Yaw { get; }

        double Pitch { get; }

        void SetLook(double yaw, double pitch);

        /// <summary>Combat input counts: the cursor is captured (keyboard and mouse) or a pad is playing.</summary>
        bool Captured { get; }

        /// <summary>One per fixed step: the axes and buttons (yaw / pitch = the look now); consumes the latched presses.
        /// Inactive (DOWN, a screen): the look only, the presses still consumed.</summary>
        InputFrame SampleMove(bool active);

        /// <summary>One per fixed step; consumes the latched presses; neutral (weapon -1) when inactive. The implementation
        /// applies its own capture rule (the keyboard and mouse only while the cursor is captured; a playing pad always).</summary>
        CombatInput SampleCombat(bool active);
    }

    /// <summary>The device the player used last (keyboardMouse.js lastDevice): prompts and glyphs follow it, never the look.</summary>
    public enum InputDevice { KeyboardMouse, Pad }

    /// <summary>
    /// The pad's glyph set (U3-26.7): the web's neutral labels, Xbox letters, PlayStation shapes in words; NIGHT15 U3-28.8:
    /// Nintendo (A / B and X / Y at swapped positions, chosen by the device's name).
    /// </summary>
    public enum GlyphSet { Neutral, Xbox, PlayStation, Nintendo }

    /// <summary>
    /// This frame's menu input from the pad (gamepad.js menuNav: a held direction repeats after gamepad.menuDelay, then every
    /// menuEvery; A confirm, B back, START): edges, true for one frame. U3-26.7 fills it; UiNav (U3-26.8) reads it.
    /// </summary>
    public struct MenuInput
    {
        public bool up, down, left, right, confirm, back, start;

        /// <summary>NIGHT19 U3-33.3: LB / RB's press edges (design.pad.n19 only): the screen with tabs shows the previous / next.</summary>
        public bool tabPrev, tabNext;
    }

    /// <summary>
    /// The devices' state the UI reads (U3-26.7 writes it once per rendered frame, in every flow state): the last device
    /// used, the pad's glyph set, whether a pad is connected, and this frame's menu input.
    /// </summary>
    public static class PlayerDevices
    {
        public static InputDevice Last = InputDevice.KeyboardMouse;
        public static GlyphSet Glyphs = GlyphSet.Neutral;
        public static bool PadConnected;
        public static MenuInput Menu;

        /// <summary>NIGHT19 U3-33.4: the pad's live layout (remaps included; null without a pad reader): PromptText reads it.</summary>
        public static PadLayout Layout;

        /// <summary>NIGHT19: design.pad.n19 is live (PadReader.V2): the v2 layout, L3 SPRINT, the bumpers' tabs, the drawn glyphs.</summary>
        public static bool N19;

        /// <summary>NIGHT19: the live key bindings (the keyboard reader's; null: none): PromptText's keyboard keys.</summary>
        public static Bindings Bindings;
    }

    /// <summary>
    /// The radial weapon wheel's state for the HUD (keyboardMouse.js :170-236; NIGHT14-RESEARCH section 2): open while the
    /// radial key is held; the pointer offset in px (clamped to radial.maxOffset); the highlighted slot (-1: inside the
    /// dead zone); the slot count it was opened with. NIGHT15 U3-28.8: `pad`, the pad's wheel opened it (the right stick
    /// points, x / y are the stick's tilt x radial.maxOffset; the mouse neither points it nor turns the view).
    /// </summary>
    public struct RadialState
    {
        public bool open;
        public double x, y;
        public int highlighted;
        public int slots;
        public bool pad;
    }
}
