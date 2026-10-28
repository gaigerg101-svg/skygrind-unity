// NIGHT40 builder W1 (the weapons): the sim side of charge40 (N40Blocks.charge40, design.n40.charge40; Core/Guns/Charge35.cs), through the lead's
// partial hook (SurvivalSim.N40.cs). N40StartRun runs after N35 / N36 / N39 / N33's start (charge35's object, tap36's and p39's flags, shop33's
// KESTREL ratio are in place). With charge40 on: FULL comes at design.n40.charge40.revolverFullS / shotgunFullS from the press (ChargeShots27.FullSteps,
// a shop33 ratio kept), and the THUNDERCLAP takes p39's hold rule on the ground (a press in the air fires at once). Off (or a tape / --off40):
// NIGHT39's rules, bit-exact. charge40 needs charge35's object (design.n35.charge35 on): c40Missing counts runs where it was on without it.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT40 W1: runs where charge40 was on but charge35 had no live object (the harness prints it).</summary>
        public int c40Missing;

        partial void N40ChargeStartRun()
        {
            if (charge35 == null)
            {
                if (n40.charge40) c40Missing++;
                return;
            }
            ChargeShots27 c = loadout.charge27;
            bool live = n40.charge40 && c != null && ReferenceEquals(c.c35, charge35);
            charge35.c40 = live;
            const string p = "design.n40.charge40.";
            string bp = live && ReferenceEquals(c, charge31) ? "design.n31.charge." : "design.n27.chargeShots27.";
            charge35.c40FullS[0] = live ? cfg.Num(p + "revolverFullS") : 0;
            charge35.c40FullS[1] = live ? cfg.Num(p + "shotgunFullS") : 0;
            charge35.c40BaseS[0] = live ? cfg.Num(bp + "revolver.fullSeconds") : 0;
            charge35.c40BaseS[1] = live ? cfg.Num(bp + "shotgun.fullSeconds") : 0;
            charge35.c40TapS = live ? cfg.Num(p + "shotgunTapS") : 0;
            charge35.c40CorrectFullS = live ? cfg.Num(p + "correctFullS") : 0;
            charge35.c40CorrectFadeS = live ? cfg.Num(p + "correctFadeS") : 0;
            charge35.c40PartialFires = live && cfg.Design("n40.charge40.partialFires");
            charge35.c40Ground = false;
            if (n40.charge40 && !live) c40Missing++;
        }
    }
}
