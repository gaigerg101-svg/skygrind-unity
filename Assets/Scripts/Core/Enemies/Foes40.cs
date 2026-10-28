// NIGHT40 builder E3 (NIGHT40.md card E3, foes40): the loadout's side of the campaign roster. Root cause (`n40foes drone`, 2026-10-14): with the PULSE
// gauntlet in hand the punch is dropped (Rules30.MeleeBlocked / Lockout29: "none with the gauntlet in hand, its swing is the melee") and the gauntlet's
// swing (SwingGauntlet) never reflects a projectile, so a player holding the gauntlet alone (level 2's start, iceline40) could not parry the DRONE's
// orb at all: 0 reflects in 45 punches pressed. With foes40 on (SurvivalSim.Foes40.cs sets swingReflect40 at a run's start; false with the row off:
// the old swing bit for bit) the gauntlet's swing reflects a hostile glob or orb in the punch's reach first, exactly as the punch does (punch.js
// tryReflect through SpitterAcid.TryReflect; parry36 sends it back at its shooter), and that swing hits nothing else. Events: the web's "parry"
// (weapon "gauntlet") and "meleeSwing" {parry true}; VERBS.md: the gauntlet's swing gains the parry of a projectile (a verb added, none lost).
namespace Skygrind.Core
{
    public sealed partial class Loadout
    {
        /// <summary>NIGHT40 E3 foes40: the gauntlet's swing reflects a projectile in reach (false: the old swing).</summary>
        public bool swingReflect40;

        /// <summary>NIGHT40 E3: swings that reflected a projectile (the harness reads it).</summary>
        public int swingReflects40;

        /// <summary>A melee knock (the punch, the gauntlet's swing): a WARDEN knows it is one (its foes40 shield rule); every enemy is knocked as before.</summary>
        void MeleeKnock40(Enemy enemy, double dx, double dz, double speed, double time, double dt)
        {
            if (enemy is Warden w)
            {
                w.meleeKnock40 = true;
                w.Knockback(dx, dz, speed, time, dt);
                w.meleeKnock40 = false;
            }
            else enemy.Knockback(dx, dz, speed, time, dt);
        }

        /// <summary>The swing's reflect (true: a projectile went back and the swing is spent on it).</summary>
        bool SwingReflect40()
        {
            if (acid == null) return false;
            var (origin, dir) = getAim();
            SpitterAcid.Glob glob = acid.TryReflect(origin, dir, enemies);
            if (glob == null)
            {
                Parry42Miss("gauntlet"); // NIGHT42 W1 parry42: a swing that sent nothing back opens the orb buffer too (Core/Parry42.cs; off: nothing)
                return false;
            }
            swingReflects40++;
            events.Emit("meleeSwing", new MeleeEvent { owner = owner, weapon = "gauntlet", hits = 1, parry = true });
            double healed = health.Heal(pParryHeal);
            if (parryRefillsDash) mover.RestoreStamina();
            events.Emit("parry", new HitEvent { owner = owner, weapon = "gauntlet", kind = "orb", enemy = -1, damage = healed, position = glob.position });
            if (parry36 != null) Parry36Glob(glob, false);
            return true;
        }
    }
}
