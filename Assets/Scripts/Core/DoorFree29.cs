// NIGHT29 (builder S; design.n29.doorFree): GG's own dev tool. "Hold the right bumper for three seconds and every door opens, but the enemies
// still spawn." This is the rule's pure half: a hold detector. Update(held, dt) is called once per fixed step with the button's state;
// it returns true on the one step the hold reaches design.n29.doorFree.holdSeconds (3.0 s) and the toggle flips. The hold must be unbroken
// (a release resets it), a long hold toggles once (the button has to be let go and pressed again for the next flip), and a normal press is
// untouched: this class only watches, the press does its own job (punch / switch) at once; `Revealing` turns true only after
// `revealSeconds` (0.4 s) of holding, so nothing about a normal press changes inside that window (the HUD's ring starts there).
// Timers are whole steps (stepsFor) as every timer of the sim. `Used` stays true for the level once the toggle has been ON: the run is then
// UNRANKED and writes no record (D-6). Reset() at a level load: off, unused.
using System;

namespace Skygrind.Core
{
    public sealed class DoorFree29
    {
        readonly double holdSeconds, revealSeconds;
        int heldSteps;
        bool spent;

        /// <summary>The toggle: every door of the level stands open.</summary>
        public bool On { get; private set; }

        /// <summary>The toggle has been on at some point this level: the run is UNRANKED and saves no record.</summary>
        public bool Used { get; private set; }

        public DoorFree29(double holdSeconds, double revealSeconds)
        {
            this.holdSeconds = holdSeconds;
            this.revealSeconds = revealSeconds;
        }

        public static DoorFree29 From(GameConfig cfg) =>
            new DoorFree29(cfg.Num("design.n29.doorFree.holdSeconds"), cfg.Num("design.n29.doorFree.revealSeconds"));

        /// <summary>A level load: off, unused, no hold in progress.</summary>
        public void Reset()
        {
            On = Used = spent = false;
            heldSteps = 0;
        }

        /// <summary>Is the hold far enough along for the HUD to show its ring (past the reveal time, not yet spent)?</summary>
        public bool Revealing => !spent && heldSteps > 0 && heldSteps >= Math.Max(1, MovementMath.StepsFor(revealSeconds, lastDt));

        /// <summary>The hold's progress 0..1 once revealing (0 before: a normal press shows nothing).</summary>
        public double Progress => Revealing ? Math.Min(1, heldSteps / (double)Math.Max(1, MovementMath.StepsFor(holdSeconds, lastDt))) : 0;

        double lastDt = 1.0 / 120;

        /// <summary>One fixed step with the button's held state. True on the step the hold completes (the toggle has just flipped).</summary>
        public bool Update(bool held, double dt)
        {
            lastDt = dt;
            if (!held)
            {
                heldSteps = 0;
                spent = false;
                return false;
            }
            if (spent) return false; // held on past the toggle: once per hold
            heldSteps++;
            if (heldSteps < Math.Max(1, MovementMath.StepsFor(holdSeconds, dt))) return false;
            spent = true;
            On = !On;
            if (On) Used = true;
            return true;
        }
    }
}
