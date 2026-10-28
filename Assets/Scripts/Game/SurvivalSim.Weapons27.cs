// NIGHT27 (builder W): the sim's half of the two weapon rows (design.n27.pulse27, chargeShots27; the rules are Core/PulseCharge27.cs and
// Core/Guns/ChargeShots27.cs). The hooks are the declaration-only ones in SurvivalSim.N27.cs: N27WeaponsStartRun / N27WeaponsStep.
// The start hook runs before loadout.Reset (SurvivalSim.Run.cs), so it only hands the loadout its two objects; both keep their own per-run state
// (reset here). Off (a row off, --off27, a tape without the bit): the loadout's fields are null and NIGHT26's game runs bit for bit.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT27 pulse27: the glove (created at the first run with the row on; its kills listener lives on the sim's bus).</summary>
        public PulseCharge27 pulse27;

        /// <summary>NIGHT27 chargeShots27: the RT charge's state (created at the first run with the row on).</summary>
        public ChargeShots27 charge27;

        partial void N27WeaponsStartRun()
        {
            if (n27.pulse27)
            {
                pulse27 ??= new PulseCharge27(cfg, events);
                pulse27.Reset(true);
                loadout.pulse27 = pulse27;
            }
            else
            {
                pulse27?.Reset(false);
                loadout.pulse27 = null;
            }
            if (n27.chargeShots27)
            {
                charge27 ??= new ChargeShots27(cfg);
                charge27.Reset(true);
                loadout.charge27 = charge27;
            }
            else
            {
                charge27?.Reset(false);
                loadout.charge27 = null;
            }
        }

        partial void N27WeaponsStep(double dt)
        {
            if (!n27.pulse27 && !n27.chargeShots27) return;
            loadout.StepCharge27Clock(); // the charged shots' cooldowns run whatever is in hand
            loadout.Charge27WeaponGuard(); // a gun swap drops a building charge
            if (Active) loadout.StepPulse27(lastCombat, dt); // the glove's boost and LT's blast (after the loadout's own step, before the enemies)
        }
    }
}
