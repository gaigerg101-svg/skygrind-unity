// NIGHT35 builder W (weapons): the sim side of charge35 (N35Blocks.charge35, design.n35.charge35; Core/Guns/Charge35.cs), through the lead's partial
// hooks (SurvivalSim.N35.cs). N35WeaponsStartRun runs after N31WeaponsStartRun (which put design.n31.charge's ChargeShots27 on the loadout): with the
// block on, that object gets charge35's trigger shape (c35); off, c35 is null and NIGHT31's rule runs as before (bit-exact). charge35 needs the n31
// charge object (design.n31.charge on, its default): without it there is nothing to charge and the block does nothing (charge35Missing counts it).
// The charge's clock and the swap guard keep running in N31WeaponsStep; N35WeaponsStep has nothing to do per step.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT35 W: charge35's numbers and hold state (made at the first run with the block on; null until then).</summary>
        public Charge35 charge35;

        /// <summary>NIGHT35 W: runs where charge35 was on but the loadout had no charge object (design.n31.charge off): the harness prints it.</summary>
        public int charge35Missing;

        partial void N35WeaponsStartRun()
        {
            ChargeShots27 c = loadout.charge27;
            if (n35.charge35 && c != null)
            {
                charge35 ??= new Charge35(cfg);
                charge35.Reset();
                c.c35 = charge35;
            }
            else
            {
                if (n35.charge35) charge35Missing++;
                // an object a previous run gave charge35 never keeps it
                if (c != null) c.c35 = null;
                if (charge31 != null) charge31.c35 = null;
                if (charge27 != null) charge27.c35 = null;
            }
        }

        partial void N35WeaponsStep(double dt) { }
    }
}
