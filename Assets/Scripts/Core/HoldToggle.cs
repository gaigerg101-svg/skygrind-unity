// Port of skygrind/src/input/holdToggle.js: hold or toggle for the held actions (OVERNIGHT3 P2, docs/FRICTION.md #6):
// SLIDE and the ARCLINE swing. In TOGGLE a press latches the hold on and the next press lets go, so nothing has to stay
// pressed; the Mover still reads the same {held, pressed} buttons it reads from a held key, so movement never knows which
// mode is on. HOLD (the default) leaves the frame untouched. Pure (unit tested); the input layer keeps one state object
// per player.
//   SLIDE:    a press slides (or slams in the air) and keeps sliding; the next press or a jump stands you up.
//   ARCLINE:  a press fires the hook and keeps the swing; the next press lets go (the press is spent on the release,
//             it doesn't fire a new hook); a jump lets go as always. The latch drops once the rope is gone.
// NIGHT13 U3-26.7 (Unity): the modes are settings.slideMode / arclineMode ('hold' | 'toggle'); the rope phase is the
// Mover's RopePhase (none | flying | attached) before this step. Applied to the player's own input only (main.js :1807).
namespace Skygrind.Core
{
    /// <summary>createToggleState: a fresh latch state (Reset puts it back: main.js respawnAt, :853).</summary>
    public sealed class ToggleState
    {
        public bool slide, grapple, grappleLive;

        /// <summary>NIGHT31: steps since a TOGGLE press that started a hook, while the tap window is open (-1: not watching).</summary>
        public int grappleSince = -1;

        /// <summary>NIGHT31: the TOGGLE mode's tap window in steps (0 = NIGHT30's toggle). The reader copies HoldToggle.grappleTapSteps (the run's) here
        /// before each ApplyToggles, so a state the tests make stays NIGHT30's whatever a run set.</summary>
        public int tapSteps;

        public void Reset()
        {
            slide = false;
            grapple = false;
            grappleLive = false;
            grappleSince = -1;
        }
    }

    public static class HoldToggle
    {
        /// <summary>
        /// NIGHT31 item 1 (builder G, DESIGN31 R3): the TOGGLE mode's tap / hold rule while design.n31.grapplePull is on (the run sets it;
        /// 0 = NIGHT30's toggle). For this many steps after the press that starts a hook the button passes through as it is: up inside
        /// the window, the latch never engages and the sim's tap latch (GrappleTap31) makes it a pull; still down at the window's end,
        /// the toggle latches as before (the swing, let go by the next press). A process-wide number: the reader applies the toggles
        /// before the sim (and the tape) see the frame. The reader copies it into its ToggleState.tapSteps (ApplyToggles reads the state's).
        /// </summary>
        public static int grappleTapSteps;

        /// <summary>
        /// Apply the toggle modes to one InputFrame, in place: `frame` the InputFrame of this fixed step (mutated), `state`
        /// the latches (mutated), the modes (true: 'toggle'), the player's rope phase before this step.
        /// </summary>
        public static void ApplyToggles(ref InputFrame frame, ToggleState state, bool slideToggle, bool grappleToggle, RopePhase ropePhase = RopePhase.None)
        {
            if (slideToggle)
            {
                if (frame.slide.pressed) state.slide = !state.slide;
                else if (frame.jump.pressed) state.slide = false;
                frame.slide.held = state.slide;
            }
            else state.slide = false;

            if (grappleToggle)
            {
                RopePhase phase = ropePhase;
                bool rawHeld = frame.grapple.held;
                if (state.grapple && phase != RopePhase.None) state.grappleLive = true;
                if (frame.grapple.pressed)
                {
                    if (state.grapple && phase != RopePhase.None)
                    {
                        state.grapple = false; // the press lets go of the rope ...
                        frame.grapple.pressed = false; // ... and doesn't fire a new hook
                        state.grappleSince = -1;
                    }
                    else
                    {
                        state.grapple = true;
                        state.grappleLive = false;
                        state.grappleSince = state.tapSteps > 0 ? 0 : -1; // NIGHT31: watch the tap window
                    }
                }
                else if (state.grapple && state.grappleLive && phase == RopePhase.None)
                {
                    state.grapple = false; // the rope is gone (a jump, a break): the next press hooks again
                }
                if (!frame.grapple.pressed && state.grappleSince >= 0)
                {
                    // NIGHT31: up inside the window = a tap (the latch lets go; the sim's GrappleTap31 makes it a pull); past it = the toggle's swing.
                    state.grappleSince++;
                    if (!rawHeld)
                    {
                        state.grapple = false;
                        state.grappleSince = -1;
                    }
                    else if (state.grappleSince > state.tapSteps) state.grappleSince = -1;
                }
                frame.grapple.held = state.grapple;
            }
            else
            {
                state.grapple = false;
                state.grappleLive = false;
                state.grappleSince = -1;
            }
        }

        /// <summary>applyToggles with the settings' texts: modes.slide / modes.grapple === 'toggle'.</summary>
        public static void ApplyToggles(ref InputFrame frame, ToggleState state, string slideMode, string grappleMode, RopePhase ropePhase = RopePhase.None) =>
            ApplyToggles(ref frame, state, slideMode == "toggle", grappleMode == "toggle", ropePhase);
    }
}
