// Port of skygrind/src/combat/weaponMath.js (p17-night6), line for line, the web's comments kept (NIGHT9 U3-07).
// Pure maths for the Survival weapons and perks (SURVIVAL.md §4, S3). No three.js, no DOM, unit tested.
// Vectors are plain {x, y, z}.
// - perkEffects and perkPrice are already ported as PerkMath.Effects / PerkMath.PerkPrice (Perks.cs, NIGHT8 U2-11):
//   not duplicated here. PerkEffects flattens the web's `weapons` part into fireRate / swap / charge;
//   WeaponMods.Of(fx) rebuilds the `mods` object fireInterval / swapTime / chargeTime take.
// - heatStep's `h` {heat, lock} is a HeatState (mutated, as on the web); `n` (config.weapons.nailgun) a NailgunParams.
// - blastKnockback writes the velocity change into `output` (the web's `out`).
// Golden vectors: PortExport/vectors/weaponMath.json (Golden/GoldenPorted.cs). Web tests: tests/weapons.test.js.
using System;

namespace Skygrind.Core
{
    /// <summary>The Nailgun's heat: heat 0..1 and lock = s left of the overheat lock.</summary>
    public sealed class HeatState
    {
        public double heat;
        public double @lock;
    }

    /// <summary>config.weapons.nailgun: the numbers heatStep reads.</summary>
    public sealed class NailgunParams
    {
        public double heatTime, lockTime, coolRate;

        public static NailgunParams From(GameConfig cfg, string path = "weapons.nailgun") => new NailgunParams
        {
            heatTime = cfg.Num(path + ".heatTime"),
            lockTime = cfg.Num(path + ".lockTime"),
            coolRate = cfg.Num(path + ".coolRate"),
        };
    }

    /// <summary>The weapon-speed multipliers {fireRate, swap, charge} (1 = none).</summary>
    public struct WeaponMods
    {
        public double fireRate, swap, charge;

        public static readonly WeaponMods None = new WeaponMods { fireRate = 1, swap = 1, charge = 1 };

        /// <summary>perkEffects(...).weapons from the port's flattened PerkEffects.</summary>
        public static WeaponMods Of(PerkEffects fx) => new WeaponMods { fireRate = fx.fireRate, swap = fx.swap, charge = fx.charge };
    }

    public static class WeaponMath
    {
        /// <summary>
        /// Nailgun heat: firing adds 1 / heatTime per second; at 1 it overheats and locks for lockTime while the
        /// heat drains to 0; not firing cools at coolRate per second. Mutates `h` {heat, lock}.
        /// `firing`: the trigger is held and a nail went out this step. Returns true on the step it overheats.
        /// </summary>
        public static bool HeatStep(HeatState h, bool firing, double dt, NailgunParams n)
        {
            if (h.@lock > 0)
            {
                h.@lock = JsMath.Max(0, h.@lock - dt);
                h.heat = n.lockTime > 0 ? h.@lock / n.lockTime : 0;
                return false;
            }
            if (firing)
            {
                h.heat = JsMath.Min(1, h.heat + dt / n.heatTime);
                if (h.heat >= 1)
                {
                    h.@lock = n.lockTime;
                    return true;
                }
                return false;
            }
            h.heat = JsMath.Max(0, h.heat - n.coolRate * dt);
            return false;
        }

        /// <summary>Splash damage at distance d: `center` at 0, falling linearly to center × edge at the radius, 0 beyond.</summary>
        public static double SplashDamage(double d, double radius, double center, double edge)
        {
            if (d > radius) return 0;
            return center * (1 - (1 - edge) * (d / radius));
        }

        /// <summary>
        /// Rocket-jump knockback (0 self damage): a push away from the blast, maxSpeed at the center falling to 0
        /// at the radius; the push always has at least `minUp` of its size upward so a blast at your feet lifts
        /// you. Writes the velocity change into `output`; returns its size (0 outside the radius).
        /// </summary>
        public static double BlastKnockback(Vec3 from, Vec3 body, double radius, double maxSpeed, double minUp, out Vec3 output)
        {
            double dx = body.x - from.x;
            double dy = body.y - from.y;
            double dz = body.z - from.z;
            double d = JsMath.Hypot(dx, dy, dz);
            if (d > radius)
            {
                output = new Vec3(0, 0, 0);
                return 0;
            }
            if (d < 1e-6)
            {
                dx = 0;
                dy = 1;
                dz = 0;
            }
            else
            {
                dx /= d;
                dy /= d;
                dz /= d;
            }
            if (dy < minUp)
            {
                // Tilt the push upward, keeping its horizontal direction.
                double h = JsMath.Hypot(dx, dz);
                double k = h > 1e-6 ? Math.Sqrt(1 - minUp * minUp) / h : 0;
                dx *= k;
                dz *= k;
                dy = minUp;
            }
            double speed = maxSpeed * (1 - d / radius);
            output = new Vec3(dx * speed, dy * speed, dz * speed);
            return speed;
        }

        /// <summary>The QUICK perk and other weapon-speed modifiers. `mods`: multipliers (1 = none).</summary>
        public static double FireInterval(double baseInterval, WeaponMods mods) => baseInterval / mods.fireRate;

        public static double SwapTime(double baseTime, WeaponMods mods) => baseTime / mods.swap;

        public static double ChargeTime(double baseTime, WeaponMods mods) => baseTime * mods.charge;

        // perkEffects(owned, p, baseSwapTime): PerkMath.Effects(owned, cfg) (Perks.cs).
        // perkPrice(base, ownedCount, tax, free): PerkMath.PerkPrice (Perks.cs).
    }
}
