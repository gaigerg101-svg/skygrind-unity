// NIGHT39 builder P1 (the pistol): the sim side of p39 (N39Blocks.p39, design.n39.p39; Core/Guns/Charge35.cs StepP39), through the lead's partial hooks
// (SurvivalSim.N39.cs). N39StartRun runs after N35StartRun / N36StartRun, so charge35's object (design.n35.charge35 on) and tap36's flag are in place:
// with p39 on, the KESTREL takes the hold rule (a quick release fires the tap, aimed from the press; a held press charges with no tap first) and the
// THUNDERCLAP keeps tap36. Off (or a tape / --off39): tap36 / charge35 as NIGHT38 left them, bit-exact. p39 needs charge35's object (without it the
// KESTREL has no hold to prioritise): p39Missing counts runs where it was on with no charge35 object.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT39 P1: runs where p39 was on but charge35 had no object (the harness prints it).</summary>
        public int p39Missing;

        partial void N39PistolStartRun()
        {
            if (charge35 == null)
            {
                if (n39.p39) p39Missing++;
                return;
            }
            bool live = n39.p39 && loadout.charge27 != null && ReferenceEquals(loadout.charge27.c35, charge35);
            charge35.p39 = live;
            const string p = "design.n39.p39.";
            charge35.p39TapS = live ? cfg.Num(p + "tapS") : 0;
            charge35.p39CorrectFullS = live ? cfg.Num(p + "correctFullS") : 0;
            charge35.p39CorrectFadeS = live ? cfg.Num(p + "correctFadeS") : 0;
            charge35.p39PartialFires = live && cfg.Design("n39.p39.partialFires");
            charge35.p39IntervalMult = live ? cfg.Num(p + "intervalMult") : 1;
            if (n39.p39 && !live) p39Missing++;
        }
    }
}
