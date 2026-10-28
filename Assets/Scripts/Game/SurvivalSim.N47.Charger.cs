// NIGHT47 builder B, row charger47 (sim; G26; Research/2026-10-18-wave47/B-cards.md section 1, the CHARGER foe card). Called from
// SurvivalSim.N47.Brawl.cs's hooks. At the run's start the row's numbers go into husks.chargerAttack (the one ChargerParams every charger
// reads), after NIGHT36's tells36 has set the wind-up (RowsStartRun runs after N36StartRun): the wind-up tellS (in place of tells36's 1.2 s),
// the lock lockS, the turn turnDegS, the burst and its heavy stun, the enrage. With the row off the NIGHT47 fields are written back to 0 /
// false (Charger.cs then runs NIGHT46's rule) and the wind-up is left as tells36 set it. Per step (row on): a light body a charge ran
// through last step (Charger.burst47Target) is burst through launch36's damage path (the kill is the player's, as a launched body's burst),
// event chargerBurst47 {id, yaw}; a charger that died this step enrages every other live charger within enrageRadius (its next wind-up
// enrageTellS; Charger.cs emits chargerEnrage47 when it starts). No allocation per step.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        const string Charger47Root = "design.n47.charger47.numbers.";
        bool c47On;
        double c47BurstDamage, c47EnrageR2;
        readonly ChargerEvent c47Said = new ChargerEvent();

        /// <summary>charger47 is on this run (views and harnesses read it).</summary>
        public bool Charger47On => c47On;

        void Charger47StartRun()
        {
            // the harness's G27 policies by flag (another command's module initializer may run and exit before Harness.N47.Brawl.cs's: read here)
            string[] argv47 = System.Environment.GetCommandLineArgs();
            if (System.Array.IndexOf(argv47, "--b47-gauntlet-only") >= 0) Probe47GauntletOnly = true;
            if (System.Array.IndexOf(argv47, "--b47-mixed") >= 0) Probe47Mixed = true;
            c47On = rows.On(idCharger47);
            ChargerParams ch = husks.chargerAttack;
            if (c47On)
            {
                ch.tellTime = cfg.Num(Charger47Root + "tellS");
                ch.lock47 = cfg.Num(Charger47Root + "lockS");
                ch.turn47 = cfg.Num(Charger47Root + "turnDegS") * System.Math.PI / 180;
                ch.burst47 = cfg.Num(Charger47Root + "burst") > 0;
                ch.heavyStun47 = cfg.Num(Charger47Root + "heavyStunS");
                ch.enrageTell47 = cfg.Num(Charger47Root + "enrageTellS");
                ch.gloveFront0_47 = cfg.Has(Charger47Root + "gloveFront0") && cfg.Num(Charger47Root + "gloveFront0") > 0;
                ch.gunBack47 = cfg.Has(Charger47Root + "gunBack") ? cfg.Num(Charger47Root + "gunBack") : 0;
                c47BurstDamage = cfg.Num(Charger47Root + "burstDamage");
                double r = cfg.Num(Charger47Root + "enrageRadius");
                c47EnrageR2 = r * r;
            }
            else
            {
                ch.lock47 = 0;
                ch.turn47 = 0;
                ch.burst47 = false;
                ch.heavyStun47 = 0;
                ch.enrageTell47 = 0;
                ch.gloveFront0_47 = false;
                ch.gunBack47 = 0;
            }
        }

        /// <summary>Harness only (Tools/SimHarness, `--b47-gauntlet-only` on any command): the player owns the PULSE gauntlet alone (every other
        /// weapon taken each step), G27's gauntlet-only policy for THE STAMPEDE. False in the game.</summary>
        public static bool Probe47GauntletOnly;
        /// <summary>Harness only (`--b47-mixed`): the player owns the KESTREL and the gauntlet (a walk started at a late room has only what
        /// that room awards), G27's mixed policy. False in the game.</summary>
        public static bool Probe47Mixed;

        void Charger47Step()
        {
            if (Probe47Mixed && !loadout.owned.Contains("revolver")) loadout.AddWeapon("revolver");
            if (Probe47Mixed && !loadout.owned.Contains("gauntlet")) loadout.AddWeapon("gauntlet");
            if (Probe47GauntletOnly && loadout.owned.Count > 1 && loadout.owned.Contains("gauntlet"))
            {
                loadout.owned.RemoveAll(w => w != "gauntlet");
                loadout.slot = 0;
            }
            if (!c47On) return;
            var all = enemies.all;
            for (int i = 0; i < all.Count; i++)
            {
                if (!(all[i] is Charger c)) continue;
                Enemy t = c.burst47Target;
                if (t != null)
                {
                    c.burst47Target = null;
                    if (t.alive && c.alive)
                    {
                        loadout.Launch36Damage(t, c47BurstDamage, t.position);
                        c47Said.id = c.id;
                        c47Said.yaw = c.yaw;
                        events.Emit("chargerBurst47", c47Said);
                    }
                }
                if (c.wasAlive47 && !c.alive)
                {
                    for (int j = 0; j < all.Count; j++)
                    {
                        if (!(all[j] is Charger o) || o == c || !o.alive) continue;
                        double dx = o.position.x - c.position.x, dz = o.position.z - c.position.z;
                        if (dx * dx + dz * dz <= c47EnrageR2) o.enraged47 = true;
                    }
                }
                c.wasAlive47 = c.alive;
            }
        }
    }
}
