// NIGHT31 item 1 (builder G owns this file): THE PULL-IN GRAPPLE in the sim (design.n31.grapplePull; the rule is Core/GrappleTap31.cs).
// A TAP of the grapple pulls you in (the Mover's own zip on the node or surface the hook took), a HOLD is NIGHT30's swing.
// - Run start: the pull's speed goes on the Mover's config.arcline.zip.reelSpeed for the run (the way Arcline21Rules.Apply writes the zip's
//   numbers; golden vectors never hook, so they cannot see it); the web's number comes back on a run with the block off.
// - Before mover.Step (N31GrappleFrame, the lead's call in SurvivalSim.Run.cs): the tap / hold latch on the player's frame.
// - After mover.Step (N31GrappleStep): the arrival pop, an upward velocity added after the zip's "zip" release (Mover.velocity is public).
// Events (new, PORT-STATUS's list): grapplePull {speed, node, x, y, z} when a tap turns the rope into a pull; grappleArrive {speed, pop}
// when the pull lets go at the anchor; grappleCancel when a second press lets go of a pull.
// Off (the row, or --off31 and every older off switch): the latch never touches a frame, the zip speed is the web's: NIGHT30's game.
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>grapplePull / grappleArrive's payload.</summary>
    public sealed class GrapplePull31Event
    {
        public double speed, pop, x, y, z;
        public bool node;
    }

    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT31: the grapple's tap / hold latch (tapSteps 0 with the block off).</summary>
        public readonly GrappleTap31 grappleTap31 = new GrappleTap31();

        /// <summary>NIGHT31: design.n31.grapplePull's numbers (null until a run starts with the block on).</summary>
        public GrapplePull31Rules grapplePull31;

        double? g31WebZip; // the zip speed the config had before a run with the block on wrote its own (restored by a run with it off)
        bool g31Wired, g31Arrived;
        readonly GrapplePull31Event g31Event = new GrapplePull31Event();

        partial void N31GrappleStartRun()
        {
            if (!g31Wired)
            {
                mover.Events += G31OnMover;
                g31Wired = true;
            }
            g31Arrived = false;
            ArclineConfig a = cfg.movement.arcline;
            if (!n31.grapplePull)
            {
                if (g31WebZip.HasValue) a.zip.reelSpeed = g31WebZip.Value;
                g31WebZip = null;
                grappleTap31.Reset(0);
                HoldToggle.grappleTapSteps = 0;
                return;
            }
            grapplePull31 ??= GrapplePull31Rules.From(cfg);
            g31WebZip ??= a.zip.reelSpeed;
            a.zip.reelSpeed = grapplePull31.speed;
            int tap = MovementMath.StepsFor(grapplePull31.tapWindow, step);
            grappleTap31.Reset(tap);
            HoldToggle.grappleTapSteps = tap; // the TOGGLE mode's tap / hold rule (the reader applies it before the sim sees the frame)
        }

        /// <summary>Before mover.Step: the tap / hold latch on the player's frame.</summary>
        partial void N31GrappleFrame(ref InputFrame frame)
        {
            if (!n31.grapplePull) return;
            GrappleTapAction act = grappleTap31.Apply(ref frame, mover.rope);
            G31Hang();
            if (act == GrappleTapAction.Pull)
            {
                Rope r = mover.rope;
                g31Event.speed = cfg.movement.arcline.zip.reelSpeed;
                g31Event.pop = 0;
                g31Event.node = r.node != 0;
                g31Event.x = r.anchor.x;
                g31Event.y = r.anchor.y;
                g31Event.z = r.anchor.z;
                events.Emit("grapplePull", g31Event);
            }
            else if (act == GrappleTapAction.Cancel) events.Emit("grappleCancel");
        }

        /// <summary>Hangs this run (a pull that reached a Loom joint and became the short swing).</summary>
        public int grappleHangs31;

        /// <summary>Where THE LOOM's joints start in map.grappleNodes (-1: no fight); the harness reads it.</summary>
        public int loom30NodeStartPublic => loom30NodeStart;

        /// <summary>
        /// DESIGN31 R4 (round 4, with design.n31.loom31): a pull to one of THE LOOM's joints (a node the fight appended, index from loom30NodeStart) does not let
        /// go at the anchor: one step before the Mover's own arrival release, the zip turns into a swing on the shortest rope (arcline.minLen), and the latch keeps
        /// the button held: the HANG. A second press, a jump or a dash lets go as usual (the latch's own rules). Before mover.Step, after the latch.
        /// </summary>
        void G31Hang()
        {
            if (!n31.loom31 || loom30NodeStart < 0 || !grappleTap31.Pulling || !grappleTap31.OwnZip) return;
            Rope r = mover.rope;
            if (r.phase != RopePhase.Attached || !r.zip || r.node - 1 < loom30NodeStart) return;
            ArclineConfig a = cfg.movement.arcline;
            double dist = Vec3.DistanceTo(mover.RopePoint, r.anchor);
            if (dist >= a.zip.arrive + a.zip.reelSpeed * step * 1.5) return;
            r.zip = false; // the Mover's "zip" release never runs: the rope stays, the latch keeps it held
            r.L = a.minLen;
            r.pullSteps = 0;
            grappleHangs31++;
            events.Emit("grappleHang");
        }

        /// <summary>The Mover's "zip" release of a pull this latch started (a node or a surface; a big enemy's zip keeps NIGHT21's rule).</summary>
        void G31OnMover(MoverEvent e)
        {
            if (!n31.grapplePull || e.type != MoverEventType.RopeRelease || e.reason != "zip") return;
            if (grappleTap31.Pulling && grappleTap31.OwnZip) g31Arrived = true;
        }

        /// <summary>After mover.Step: the arrival pop.</summary>
        partial void N31GrappleStep(double dt)
        {
            if (!g31Arrived) return;
            g31Arrived = false;
            if (!n31.grapplePull) return;
            double pop = grapplePull31.arrivePop;
            mover.velocity = grapplePull31.Arrive(mover.velocity); // round 3: the horizontal carry clamped to arriveKeepMax, then the pop
            g31Event.speed = mover.velocity.Length;
            g31Event.pop = pop;
            g31Event.node = false;
            g31Event.x = mover.position.x;
            g31Event.y = mover.position.y;
            g31Event.z = mover.position.z;
            events.Emit("grappleArrive", g31Event);
        }
    }
}
