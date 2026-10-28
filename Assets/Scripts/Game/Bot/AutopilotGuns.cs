// NIGHT14 U3-27.5: the AUTOPILOT's gun choice, BotBrain's pickWeapon hook (RIVAL PRIME's: (distance, current slot) =>
// slot). botBrain.js picks slot 1 near and slot 0 far: the duel's revolver and shotgun. In SURVIVAL slot 1 is the
// GAUNTLET (weapons.survivalStart) and bought guns take the next slots, so the web's rule would punch at range and never
// fire a bought gun. This picks the first owned gun of design.autopilot.guns.near / .far by distance, with the web's own
// hysteresis around bot.shotgunRange (design.autopilot.gunHysteresisMeters). Allocation-free (an indexed loop over the
// loadout). No UnityEngine use.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class AutopilotGuns
    {
        readonly Loadout loadout;
        readonly List<string> near, far;
        readonly double nearBelow, farAbove;
        bool wantNear;

        /// <summary>The pick as a delegate made once (BotBrain.pickWeapon; a method group per step would allocate).</summary>
        public readonly Func<double, int, int> Pick;

        public AutopilotGuns(Loadout loadout, AutopilotParams p)
        {
            this.loadout = loadout;
            near = p.gunsNear;
            far = p.gunsFar;
            nearBelow = p.shotgunRange - p.gunHysteresisMeters;
            farAbove = p.shotgunRange + p.gunHysteresisMeters;
            Pick = Choose;
        }

        /// <summary>A new run: far (the brain's weaponWant starts at slot 0, the revolver).</summary>
        public void Reset() => wantNear = false;

        // --- NIGHT20 U3-34.8 (builder G): the LT and SPECIAL rule (Core/Guns/AltUse.cs, NIGHT19 W's, with tonight's tools:
        // the THUNDERCLAP's shockwave as the panic button, the zooms on far targets, the zoom guns' old LT jobs on SPECIAL),
        // one rule for the AUTOPILOT (Alt / Special, called by AutopilotInput.SampleCombat) and THE RANGE
        // (SandboxRange.AltUse = AltHeld). design.arsenal.altUse holds the numbers; without them nothing is pressed.

        /// <summary>The AUTOPILOT: hold LT this step against `target` (null: none)?</summary>
        public bool Alt(Enemy target) => Hold(loadout, target, false);

        /// <summary>The AUTOPILOT: press SPECIAL this step against `target` (null: none)?</summary>
        public bool Special(Enemy target) => Hold(loadout, target, true);

        /// <summary>THE RANGE's hook (SandboxRange.AltUse's signature: sim, the gun in hand, the target, its distance, enemies
        /// within 4 m) =&gt; hold LT this step.</summary>
        public static bool AltHeld(SurvivalSim sim, string weapon, Enemy target, double distance, int near4) => Hold(sim.loadout, target, false);

        /// <summary>THE RANGE's SPECIAL hook (the same signature) =&gt; press SPECIAL this step.</summary>
        public static bool SpecialHeld(SurvivalSim sim, string weapon, Enemy target, double distance, int near4) => Hold(sim.loadout, target, true);

        static bool Hold(Loadout lo, Enemy target, bool special)
        {
            AltUseParams p = lo?.AltParams;
            if (p == null) return false;
            AltSituation s = lo.AltSituation(target, p.near);
            return special ? AltUse.Special(in s, p) : AltUse.Hold(in s, p);
        }

        /// <summary>(distance to the target, the slot wanted last step) =&gt; the slot to hold.</summary>
        public int Choose(double distance, int current)
        {
            if (distance < nearBelow) wantNear = true;
            else if (distance > farAbove) wantNear = false;
            List<string> order = wantNear ? near : far;
            List<string> owned = loadout.owned;
            for (int i = 0; i < order.Count; i++)
            {
                string want = order[i];
                for (int s = 0; s < owned.Count; s++)
                    if (owned[s] == want) return s;
            }
            return current >= 0 && current < owned.Count ? current : 0;
        }
    }
}
