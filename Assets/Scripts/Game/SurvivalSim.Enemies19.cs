// NIGHT19 U3-33.7 (builder E owns this file): design.enemies.n19 in the sim, through the shared commit's hooks
// (SurvivalSim.N19.cs: N19EnemiesStartRun at the run's start, N19EnemiesStep right after husks.Step). The new kinds live
// in the Husks' pools and the director spawns them (Director.Spawnable reads the run's blocks); this file wires what
// main.js wires around them:
// - the affixes (Core/Enemies/Affixes.cs, SHIELDED): main.js's spawner onSpawn rolls them after the HP scale
//   (director.n19OnSpawn), `roundBegin` sets their round (affixes.setRound), a player's kill of an affixed enemy pays
//   affixes.bonusPoints x its affixes (economy.grant(bonus, 'affix')). The roll's stream is the run seed ^
//   design.enemies.n19.affixSalt (the web rolls Math.random), seeded at the run's first break (round 1's roundBreak:
//   the run seed is known then).
// - the drones' orbs (Husks.orbs): a parried orb damages through the player's loadout (main.js orbs.setReflectHandler;
//   weapon "orb", as the reflected glob's).
// With the block off nothing is rolled, filtered or paid (each handler reads the run's n19.enemies): NIGHT18's run.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT19: the run's affixes (SHIELDED); made once, reset per run; used only with n19.enemies.</summary>
        public Affixes affixes;

        uint affixSalt;
        bool enemies19Wired;

        void Enemies19Wire()
        {
            enemies19Wired = true;
            affixes = new Affixes(cfg);
            affixes.Wire(events, world, cfg); // NIGHT20 C3: the bus (calm, blinks, bursts) and the live collider (blinkTarget)
            affixSalt = cfg.Has("design.enemies.n19.affixSalt") ? (uint)cfg.Num("design.enemies.n19.affixSalt") : 0x4ff1u;
            director.n19OnSpawn = h => affixes.OnSpawn(h);
            // main.js orbs.setReflectHandler: a parried orb's hit is the player's (weapon "orb", as the reflected glob's)
            husks.orbs.onReflectHit = (e, damage, at) => loadout.Damage(e, damage, false, "orb", at);
            events.On<RoundEvent>("roundBreak", e =>
            {
                if (n19.enemies && e.number == 1) affixes.NewRun(runSeed ^ affixSalt); // the run seed is set by now
            });
            events.On<RoundEvent>("roundBegin", e =>
            {
                if (n19.enemies) affixes.round = e.number; // affixes.setRound
            });
            events.On<KillEvent>("kill", k =>
            {
                if (!n19.enemies || k.owner != "player") return;
                double bonus = affixes.OnKill(k.enemy); // affixes.onKill: an affixed kill's bonus
                if (bonus > 0) economy.Grant(bonus, "affix");
            });
        }

        partial void N19EnemiesStartRun()
        {
            if (!enemies19Wired) Enemies19Wire();
            affixes.NewRun(runSeed ^ affixSalt); // a clean slate (the stream is seeded again at the run's first break)
        }

        /// <summary>NIGHT20 C3 (design.adds.affixes, set in N20DirectorStartRun): affixes.js step after the enemies' (main.js
        /// simulate: affixes after the enemies), the player's feet and health the burst's target.</summary>
        partial void N19EnemiesStep(double dt)
        {
            if (affixes != null && affixes.all) affixes.Step(mover.position, health, dt);
        }

        /// <summary>NIGHT19 (THE RANGE's cells, the tests): give a spawned enemy these affixes (affixes.onSpawn's `forced`).</summary>
        public void AffixAssign(Husk h, params string[] list)
        {
            if (!enemies19Wired) Enemies19Wire();
            affixes.Assign(h, list);
        }

        /// <summary>
        /// NIGHT20 (builder E): THE RANGE's affix hook (SandboxRange.ApplyAffix's shape): give one ported affix to a spawned
        /// husk; false for an enemy that isn't a husk or an affix with no port (AffixMath.Ported), so the cell says n/a.
        /// </summary>
        public bool RangeAffix(Enemy e, string id)
        {
            if (!(e is Husk h) || !AffixMath.Ported(id)) return false;
            AffixAssign(h, id);
            return id != "shielded" || h.Shielded;
        }
    }
}
