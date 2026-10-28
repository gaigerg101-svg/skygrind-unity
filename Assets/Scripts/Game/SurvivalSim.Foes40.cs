// NIGHT40 builder E3 (NIGHT40.md card E3, foes40): the DRONE, the WARDEN and the CHARGER as campaign enemies, through the lead's hooks
// (SurvivalSim.N40.cs: N40FoesStartRun at a run's start, after N36's; N40FoesStep every step after N39Step). A campaign wave already spawns any pooled
// kind (Campaign24.SpawnWave -> CampaignWorld.Spawn -> Husks.Spawn: `n40foes spawn`); this file only retunes them while foes40 is on, by setting the
// shared per-kind numbers at the run's start (their survival numbers in config.json are never edited). With the row off every number goes back to the
// config's (or tells36's) at the next run start: NIGHT39's game.
// - the DRONE (design.n40.foes40.drone): the glow chargeS (over tells36's), the orb's speed, a slower approach (approachSpeed / ai.kinds.drone.speed
//   x its goal velocity), and its own reflected orb kills it (reflectKillsShooter).
// - the gauntlet's swing reflects a projectile in reach (Loadout.swingReflect40, Core/Enemies/Foes40.cs): with the gauntlet in hand the punch is
//   dropped (rules30 / lockout29), so before this a gauntlet-only player had no parry at all.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        const string Foes40Row = "design.n40.foes40";

        partial void N40FoesStartRun()
        {
            bool on = n40.foes40;
            // the DRONE (N36CoreStartRun set chargeTime to tells36's or the web's just before this)
            DroneParams d = husks.drone;
            d.moveMult = on ? cfg.Num(Foes40Row + ".drone.approachSpeed") / cfg.Num("ai.kinds.drone.speed") : 1;
            d.orbSpeed = on ? cfg.Num(Foes40Row + ".drone.orbSpeed") : cfg.Num("orb.speed");
            if (on) d.chargeTime = cfg.Num(Foes40Row + ".drone.chargeS");
            husks.orbs.reflectKillsShooter = on && cfg.Bool(Foes40Row + ".drone.reflectKillsShooter");
            // the WARDEN: only a melee knock opens its shield
            husks.wardenAttack.meleeShield40 = on && cfg.Bool(Foes40Row + ".warden.meleeShield");
            husks.wardenAttack.frontSoft40 = on ? cfg.Num(Foes40Row + ".warden.frontSoft") : 1;
            // the CHARGER: its armoured face, the stun after a charge, backing out to charge range
            ChargerParams ch = husks.chargerAttack;
            ch.frontMult40 = on ? cfg.Num(Foes40Row + ".charger.frontMult") : 1;
            ch.skidStun40 = on ? cfg.Num(Foes40Row + ".charger.skidStunS") : 0;
            ch.backOff40 = on && cfg.Bool(Foes40Row + ".charger.backOff");
            ch.stunTime = on ? cfg.Num(Foes40Row + ".charger.stunS") : cfg.Num("charger.stunTime");
            // the gauntlet's swing parries a projectile (Core/Enemies/Foes40.cs: with the gauntlet in hand the punch is dropped, so nothing else could)
            loadout.swingReflect40 = on;
        }
    }
}
