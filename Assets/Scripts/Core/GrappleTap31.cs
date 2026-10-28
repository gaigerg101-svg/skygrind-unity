// NIGHT31 item 1 (builder G, DESIGN31 B + R3, D-3): THE PULL-IN GRAPPLE. A TAP of the grapple pulls you in; a HOLD is NIGHT30's swing.
// GG (2026-10-10): "the grapple mechanic just doesn't work at all It doesn't pull you in". The Mover is frozen (CLAUDE.md rule 3) and
// already has the web's ZIP mode (mover.js r.zip: straight at the anchor at arcline.zip.reelSpeed, rope taut, released at
// arcline.zip.arrive or after arcline.zip.maxTime with the velocity kept), so this file only shapes the Mover's input and its public
// rope flag from OUTSIDE it, once per fixed step BEFORE mover.Step:
// - The Mover lets go the moment grapple.held is false ("letGo", Mover.cs:542) and takes rope.zip only at the fire (Mover.cs:537). So
//   when the button comes up inside the tap window while the rope is out (flying or attached), this latch sets rope.zip true (the
//   anchor stays; zipAnchor stays null for a node or a surface) and keeps grapple.held true until the rope is gone (the zip's own
//   arrival, a jump in the air, a dash, a break). A hold past the window is the swing, released on button-up as today.
// - Cancels (R2: ULTRAKILL and Titanfall 2 keep momentum): a jump in the air and a dash are the Mover's own releases (velocity kept);
//   a second press while a pull is latched lets go ("letGo", velocity kept) and does not fire a new hook.
// - The zip speed per run (GrapplePull31Rules.Apply, the way Arcline21Rules.Apply writes the zip numbers); the arrival pop (an upward
//   velocity added after the "zip" release) is applied by the owner (SurvivalSim.Grapple31.cs) after the Mover's step.
// Pure and deterministic: the state is a function of the frames and the Mover's rope; Reset at every run start (a tape replays it).
namespace Skygrind.Core
{
    /// <summary>design.n31.grapplePull's numbers (Unity-only: the web's grapple pulls only toward a hooked big enemy).</summary>
    public sealed class GrapplePull31Rules
    {
        /// <summary>The pull's speed (m/s), written on the Mover's config.arcline.zip.reelSpeed for the run (the web's enemy zip: 34).</summary>
        public double speed;
        /// <summary>The tap window (s): the button up within it after the press = a pull; still held = the swing.</summary>
        public double tapWindow;
        /// <summary>The arrival pop (m/s, upward), added to the velocity after the zip's "zip" release (clears a ledge).</summary>
        public double arrivePop;

        /// <summary>Round 3 (L's pull31: every level-4 gap was one tap from the door at a kept 42-51 m/s): the horizontal speed kept after the zip's own
        /// "zip" release at the anchor, clamped from outside the Mover (0 or less: no clamp). A cancel before the arrival (jump, dash, a second press) keeps it all.</summary>
        public double arriveKeepMax;

        public static GrapplePull31Rules From(GameConfig cfg) => new GrapplePull31Rules
        {
            speed = cfg.Num("design.n31.grapplePull.speed"),
            tapWindow = cfg.Num("design.n31.grapplePull.tapWindow"),
            arrivePop = cfg.Num("design.n31.grapplePull.arrivePop"),
            arriveKeepMax = cfg.Has("design.n31.grapplePull.arriveKeepMax") ? cfg.Num("design.n31.grapplePull.arriveKeepMax") : 0,
        };

        /// <summary>The velocity after a pull's arrival: the horizontal part clamped to arriveKeepMax (direction kept), then the pop added upward.</summary>
        public Vec3 Arrive(Vec3 v)
        {
            if (arriveKeepMax > 0)
            {
                double h = JsMath.Hypot(v.x, v.z);
                if (h > arriveKeepMax)
                {
                    double k = arriveKeepMax / h;
                    v.x *= k;
                    v.z *= k;
                }
            }
            v.y += arrivePop;
            return v;
        }
    }

    /// <summary>What the latch did on a step (the owner's events and the harness).</summary>
    public enum GrappleTapAction
    {
        None,
        /// <summary>The button came up inside the tap window with the rope out: rope.zip set, the hold latched.</summary>
        Pull,
        /// <summary>A second press while a pull was latched: the rope let go (velocity kept), no new hook.</summary>
        Cancel,
        /// <summary>NIGHT32 (design.n32.grapple32, D-3, tapNodesOnly): a tap on a bare surface (no node, no big enemy's zip): no pull; the button
        /// comes up, so the Mover lets go (a HOLD on a surface is still the swing). The owner gives the spent charge back (no target).</summary>
        Refused,
    }

    /// <summary>The tap / hold latch (one per player). Apply before mover.Step on the player's own frame.</summary>
    public sealed class GrappleTap31
    {
        /// <summary>The tap window in whole steps (stepsFor(tapWindow)); 0 = off (the frame is never touched).</summary>
        public int tapSteps;

        int sincePress = -1; // steps since a press that may fire a hook (-1: not watching)
        bool latched; // a pull is on: grapple.held kept true until the rope is gone
        bool upSinceLatch; // NIGHT35 grapple35 (pullOnFire): the button was seen up since the latch (only then is a press a NEW press: a cancel)

        /// <summary>Is a pull latched (the rope is reeling you in because of a tap)?</summary>
        public bool Pulling => latched;

        /// <summary>The latched pull's zip was set by this latch (a node or a surface), not by a big enemy's pick (the arrival pop is for these).</summary>
        public bool OwnZip { get; private set; }

        /// <summary>The pulls and cancels this run (the harness's counters); NIGHT32: taps refused on a bare surface.</summary>
        public int pulls, cancels, refused;

        /// <summary>NIGHT32 (design.n32.grapple32.tapNodesOnly, D-3; set by the owner at each run start, false with the row off): a TAP pulls only
        /// to a node (a ring, a boss joint) or a big enemy's zip, never to a bare surface. False: NIGHT31's latch bit for bit.</summary>
        public bool tapNodesOnly;

        /// <summary>NIGHT35 grapple35 (design.n35.grapple35 on, its `swing` sub-switch false; set by the owner at each run start, false with the row
        /// off): ANY hook that goes out is a pull at once, held or not (ULTRAKILL's whiplash: a constant pull, no swing: rea-ultrakill.md HookArm
        /// :1545), and a bare surface is never refused (the owner also clears tapNodesOnly). False: NIGHT31 / 32's latch bit for bit.</summary>
        public bool pullOnFire;

        public void Reset(int tapSteps)
        {
            this.tapSteps = tapSteps;
            sincePress = -1;
            latched = false;
            upSinceLatch = false;
            OwnZip = false;
            pulls = cancels = refused = 0;
        }

        /// <summary>
        /// One fixed step, BEFORE mover.Step: `frame` the player's frame (mutated: grapple.held / pressed), `rope` the Mover's rope as it
        /// stands before this step (mutated: zip on a tap). Nothing happens with tapSteps 0.
        /// </summary>
        public GrappleTapAction Apply(ref InputFrame frame, Rope rope)
        {
            if (tapSteps <= 0) return GrappleTapAction.None;
            if (latched)
            {
                if (rope.phase == RopePhase.None)
                {
                    latched = false; // the rope is gone (the zip arrived, a jump, a dash, a break): the button is the player's again
                }
                else if (pullOnFire && !upSinceLatch && frame.grapple.held)
                {
                    // NIGHT35 grapple35: the fire's own press still down (a frame's press is on every sim step of that frame; a hold): no cancel
                    frame.grapple.pressed = false;
                    return GrappleTapAction.None;
                }
                else if (frame.grapple.pressed)
                {
                    latched = false; // a second press lets go (the Mover's letGo keeps the velocity) ...
                    frame.grapple.held = false;
                    frame.grapple.pressed = false; // ... and does not fire a new hook
                    sincePress = -1;
                    cancels++;
                    return GrappleTapAction.Cancel;
                }
                else
                {
                    if (!frame.grapple.held) upSinceLatch = true; // NIGHT35 grapple35: the button came up (read before the latch holds it)
                    frame.grapple.held = true;
                    return GrappleTapAction.None;
                }
            }
            if (frame.grapple.pressed && rope.phase == RopePhase.None)
            {
                sincePress = 0; // the Mover may fire on this step (a target, off cooldown); watch the release
                return GrappleTapAction.None;
            }
            if (sincePress < 0) return GrappleTapAction.None;
            sincePress++;
            if (rope.phase == RopePhase.None || (sincePress > tapSteps && !pullOnFire))
            {
                sincePress = -1; // no hook went out (no target, the cooldown, a yank), or the hold passed the window: the swing
                return GrappleTapAction.None;
            }
            if (!frame.grapple.held || pullOnFire) // NIGHT35 grapple35: a hook that went out is a pull whatever the button does
            {
                // A tap: the rope is out (flying or attached) and the button came up inside the window. Reel in.
                sincePress = -1;
                if (tapNodesOnly && !pullOnFire && rope.node == 0 && !rope.zip)
                {
                    refused++; // NIGHT32 D-3: a bare surface: no pull; the button is up, the Mover lets go
                    return GrappleTapAction.Refused;
                }
                OwnZip = !rope.zip; // a big enemy's zip is already a zip (Arcline21's pick): the latch only keeps it held
                rope.zip = true;
                latched = true;
                upSinceLatch = !frame.grapple.held; // NIGHT35 grapple35: under pullOnFire the latch can come with the button still down
                frame.grapple.held = true;
                pulls++;
                return GrappleTapAction.Pull;
            }
            return GrappleTapAction.None;
        }
    }
}
