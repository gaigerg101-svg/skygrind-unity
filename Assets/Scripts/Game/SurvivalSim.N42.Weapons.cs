// NIGHT42 builder W1 (the weapons): the sim side of gauntlet42, parry42 and charge42 (N42Blocks; design.n42.*), through the lead's partial hook
// (SurvivalSim.N42.cs). N42StartRun runs after every older start hook (N27's glove, N35 / N36 / N39 / N40's charge flags, foes40's orbs are in place)
// and before loadout.Reset, so this only hands the objects their numbers; each keeps its own per-run state. Off (a row off, a tape without the bit,
// --off42 and older): every field below is 0 / false and NIGHT41's game runs bit for bit.
// - gauntlet42 (G3): the PULSE swing's cycle design.n42.gauntlet42.swingSeconds (1.8 s; the n27 row's 2.5 s untouched): PulseCharge27.swingSeconds42.
// - parry42 (G4): the DRONE orb's reach volume x reachMult, its mercy window (DroneOrbs) and the orb-only early buffer opened by a punch or a
//   gauntlet swing that sent nothing back (Core/Parry42.cs; stepped here every step).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        partial void N42WeaponsStartRun()
        {
            // G3 gauntlet42: the swing's cycle (PulseCharge27.SwingSteps reads it: Combat's cooldown, the HUD's punch look)
            if (pulse27 != null) pulse27.swingSeconds42 = n42.gauntlet42 ? cfg.Num("design.n42.gauntlet42.swingSeconds") : 0;
            // G4 parry42: the DRONE orb's reach volume, the mercy window, the orb buffer (Core/Parry42.cs; DroneOrbs)
            const string pp = "design.n42.parry42.";
            bool p42 = n42.parry42;
            DroneOrbs orbs = husks.orbs;
            if (orbs != null)
            {
                orbs.parry42 = p42;
                orbs.reach42 = p42 ? cfg.Num("orb.reflectRange") * cfg.Num(pp + "reachMult") : 0;
                orbs.mercySteps42 = p42 ? MovementMath.StepsFor(cfg.Num(pp + "mercyS"), cfg.Dt) : 0;
                orbs.mercyHeld42 = orbs.mercyParried42 = 0;
            }
            loadout.Parry42Reset(p42 ? MovementMath.StepsFor(cfg.Num(pp + "orbBufferS"), cfg.Dt) : 0);
            // G10 / G8 charge42: a held trigger re-arms the charge; the next shot waits for the reticle's cannon turn (Core/Guns/Charge35.cs C42Held)
            if (charge35 != null)
            {
                bool c42 = n42.charge42;
                charge35.c42 = c42;
                charge35.turnSteps42 = c42 ? MovementMath.StepsFor(cfg.Num("design.n42.charge42.cannonTurnS"), cfg.Dt) : 0;
            }
        }

        partial void N42WeaponsStep(double dt)
        {
            if (Active) loadout.Parry42Step(); // G4 parry42: the orb buffer (nothing while it is 0: the row off)
        }
    }
}
