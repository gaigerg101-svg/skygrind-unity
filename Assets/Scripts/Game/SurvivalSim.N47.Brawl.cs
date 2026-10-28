// NIGHT47 builder B (NIGHT47.md G25-G35; DECIDE47 D-1; Research/2026-10-18-wave47/B-cards.md): B's three sim rows, read with rows.On through
// the ids SurvivalSim.N47.cs resolves once a run (idCharger47, idBrawl47, idWyrm47). This file fills the hooks N47BStartRun / N47BStep /
// N47BCampaign; each row's own work lives in its own partial (SurvivalSim.N47.Charger.cs, and later ones named for their row).
//   charger47 THE CHARGER's NIGHT47 rule (G26): numbers in design.n47.charger47.numbers, put into husks.chargerAttack at the run's start
//             (Core/Enemies/Charger.cs's lock47 / turn47 / burst47 / heavyStun47 / enrageTell47; with the row off they are written back to
//             their NIGHT46 values, so nothing is left over from an earlier run); per step the bursts its charges marked and the enrage.
//   wyrm47    THE KILN WYRM's tells 0.8-1.5 s and the burrow never cut by a charge (G31 / G34): SurvivalSim.N47.Wyrm.cs.
//   brawl47   THE ICEBREAKER in three phases, its tells over E-4's floor, phase 3's chargers (G28-G30): SurvivalSim.N47.Icebreaker.cs.
// Off (the rows off, --off47 / -skygrindOff47): every value written back to NIGHT46's and nothing per step. No allocation per step. No
// UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        partial void N47BStartRun()
        {
            Charger47StartRun();
            Wyrm47StartRun();
            Brawl47StartRun();
            Kiln47StartRun();
            R4StartRun(); // round 4: stampede47, trail47 (SurvivalSim.N47.R4.cs)
        }

        partial void N47BStep(double dt)
        {
            Charger47Step();
            Wyrm47Step();
            Brawl47Step();
            Kiln47Step();
            R4Step();
        }

        partial void N47BCampaign()
        {
        }
    }
}
