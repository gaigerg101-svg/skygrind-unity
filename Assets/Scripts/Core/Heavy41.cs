// NIGHT41 builder E6 (NIGHT41.md card E6; INVENTORY C7; GG w40: "the gauntlet excels against large enemies"): heavy41, the gauntlet's harder
// answer on a big enemy. The body of the lead's pre-placed hook Loadout.Heavy41 (Core/Combat.cs, at the gauntlet swing's knock), called only
// while loadout.heavy41 is on (the sim sets it from sim.n41 at a run's start). A big kind (design.n41.heavy41.kinds: the TANK body
// (Enemy.tank29), the WARDEN, the CHARGER, THE ICEBREAKER's body "brawler") takes the knock-back x knockMult for at least knockTime s, and
// the swing asks the sim for a hitstop of hitstop s (SurvivalSim.Hitstop through loadout.hitstop41, cause "heavy41"). A kind's own knock
// rule still applies after it (the CHARGER's and the WARDEN's knockbackScale; a boss body that ignores knocks keeps ignoring them, so THE
// ICEBREAKER answers with the hitstop alone). Event (no audio, GG's): `heavy41` {the enemy id} on each such hit.
// Every number is design.n41.heavy41's (read once, at the first call); nothing changes with the row off.
namespace Skygrind.Core
{
    public sealed partial class Loadout
    {
        bool heavy41Read;
        string[] heavy41Kinds;
        double heavy41KnockMult, heavy41KnockTime, heavy41Hitstop;

        /// <summary>NIGHT41 E6: gauntlet hits that took heavy41's answer this run's life (the harness reads it).</summary>
        public int heavyHits41;

        void Heavy41Read()
        {
            heavy41Read = true;
            const string row = "design.n41.heavy41.";
            heavy41Kinds = cfg.Has(row + "kinds") ? cfg.Strs(row + "kinds").ToArray() : new string[0];
            heavy41KnockMult = cfg.Has(row + "knockMult") ? cfg.Num(row + "knockMult") : 1;
            heavy41KnockTime = cfg.Has(row + "knockTime") ? cfg.Num(row + "knockTime") : 0;
            heavy41Hitstop = cfg.Has(row + "hitstop") ? cfg.Num(row + "hitstop") : 0;
        }

        /// <summary>NIGHT41 E6: is `enemy` a big kind to heavy41 (the TANK by its tank29 flag, any other by its kind)?</summary>
        public bool IsHeavy41(Enemy enemy)
        {
            if (!heavy41Read) Heavy41Read();
            if (enemy == null) return false;
            string key = enemy.tank29 ? "tank" : enemy.kind;
            for (int i = 0; i < heavy41Kinds.Length; i++) if (heavy41Kinds[i] == key) return true;
            return false;
        }

        partial void Heavy41(Enemy enemy, ref double knock, ref double knockTime, ref double hitstop)
        {
            if (!IsHeavy41(enemy)) return;
            knock *= heavy41KnockMult;
            if (knockTime < heavy41KnockTime) knockTime = heavy41KnockTime;
            if (hitstop < heavy41Hitstop) hitstop = heavy41Hitstop;
            heavyHits41++;
            events.Emit("heavy41", enemy.id);
        }
    }
}
