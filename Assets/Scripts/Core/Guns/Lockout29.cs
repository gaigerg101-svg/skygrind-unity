// NIGHT29 (builder S; design.n29.lockout): the FIRE AND MELEE LOCKOUT, both ways. GG, twice: "You cannot shoot a weapon and melee at the same
// time, you have to wait for the shot to finish; with the right stick melee you can't be in the process of it and then shoot, you have to wait
// for the animation to finish". This REVERSES NIGHT25's meleeFire (which let the quick punch and the trigger run in one step).
// The rule (D-1), pure and engine-free; the Loadout reads it (Combat.cs StepPunch / Step / StepSwap) through a `lockout29` object, null = NIGHT28's game:
//   - while the gun in hand is inside its attack cycle (the cooldown its own fire interval set: the revolver's, the shotgun's pump, the rail's...)
//     a melee press is IGNORED;
//   - while a melee swing is inside its cycle (punch.cycleTime) the trigger is IGNORED;
//   - the second input is DROPPED: nothing is buffered (GG wants to wait); a held trigger simply carries on when the swing ends;
//   - with the PULSE gauntlet in hand there is no separate melee: its own swing (the trigger) is the melee, a melee press is ignored;
//   - a weapon switch asked inside either cycle WAITS (it is queued, the latest ask wins) and runs when the cycle ends.
// Cycle lengths are never typed here: they are the cooldowns the Loadout already sets from each weapon's own interval and punch.cycleTime.
using System;

namespace Skygrind.Core
{
    public sealed class Lockout29
    {
        /// <summary>A switch asked inside a cycle, waiting for it to end (-1: none).</summary>
        public int queuedSwitch = -1;

        /// <summary>Counters (the harness and the reel read them): melee presses ignored, trigger steps ignored, switches that waited.</summary>
        public int droppedMelee, droppedFireSteps, waitedSwitches;

        public void Reset()
        {
            queuedSwitch = -1;
            droppedMelee = droppedFireSteps = waitedSwitches = 0;
        }

        /// <summary>Is a melee press ignored now? True with the gauntlet in hand (no separate melee) or while the gun's attack cycle runs.</summary>
        public static bool MeleeBlocked(bool gauntletInHand, bool gunCycleRunning) => gauntletInHand || gunCycleRunning;

        /// <summary>Is the trigger ignored now? True while a melee swing's cycle runs.</summary>
        public static bool FireBlocked(bool meleeCycleRunning) => meleeCycleRunning;

        /// <summary>A melee press that the rule dropped (counted).</summary>
        public void DropMelee() => droppedMelee++;

        /// <summary>The trigger was cleared for this step (counted when it was down).</summary>
        public void DropFire(bool wasDown)
        {
            if (wasDown) droppedFireSteps++;
        }

        /// <summary>
        /// The weapon switch for this step. `want` is the slot asked now (-1 none). While a cycle runs (`busy`) an ask is queued (not run) and -1
        /// comes back; when the cycle is over a queued ask runs (a newer ask this step wins over it).
        /// </summary>
        public int FilterSwitch(int want, bool busy)
        {
            if (busy)
            {
                if (want >= 0)
                {
                    queuedSwitch = want;
                    waitedSwitches++;
                }
                return -1;
            }
            if (want < 0 && queuedSwitch >= 0) want = queuedSwitch;
            queuedSwitch = -1;
            return want;
        }
    }
}
