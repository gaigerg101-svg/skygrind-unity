// NIGHT36 builder F (gun feel): the sim side of tap36 (N36Blocks.tap36, design.n36.tap36; Core/Guns/Charge35.cs StepTap36), through the lead's partial
// hooks (SurvivalSim.N36.cs). N36StartRun runs after N35StartRun, so charge35's object (made by N35WeaponsStartRun with design.n35.charge35 on) is in
// place here: with tap36 on it takes the press-fires rule and holdStartS; off (or a tape / --off36), it is charge35 as NIGHT35 left it, bit-exact.
// tap36 needs charge35 (without it NIGHT31's rule already fires a tap at the press): tap36Missing counts runs where it was on with no charge35 object.
// crunch36 is the Game's freezes (ImpactRules through ImpactFrames), so it needs nothing per step here.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT36 F: runs where tap36 was on but charge35 had no object (the harness prints it).</summary>
        public int tap36Missing;

        partial void N36GunsStartRun()
        {
            if (charge35 == null)
            {
                if (n36.tap36) tap36Missing++;
                return;
            }
            bool live = n36.tap36 && loadout.charge27 != null && ReferenceEquals(loadout.charge27.c35, charge35);
            charge35.tap36 = live;
            charge35.holdStartS = live ? cfg.Num("design.n36.tap36.holdStartS") : 0;
            if (n36.tap36 && !live) tap36Missing++;
        }

        partial void N36GunsStep(double dt) { }
    }
}
