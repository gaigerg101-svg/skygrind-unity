// NIGHT19 U3-33.6 (builder W, design.arsenal): the BOUNCER, ported line for line from skygrind/src/combat/guns.js
// (p17-night6) with the web's comments, as a part of the loadout (guns.js's `kit` is the loadout here):
// - BOUNCER (blast): lobbed grenades that bounce and burst on an enemy or after the fuse (the grenade jump). Alt:
//   every grenade of yours in flight bursts now. Variant: +30% blast radius.
// Explosions (Bouncer, Vortex) grow their radius with the momentum at the trigger pull instead of their damage.
// The grenades fly in Projectiles (projectiles.js launch('grenade'), stepGrenades, airburst: ported there tonight).
// With design.arsenal the alt (LT) is the airburst (the BOUNCER does not zoom: right mouse is its special, ads.js).
namespace Skygrind.Core
{
    public sealed partial class Loadout
    {
        double bInterval, bSpeed, bUpward, bSpikeDirY, bVariantRadius;

        /// <summary>The web's BOUNCER numbers, read once (the constructor).</summary>
        void InitBouncer(GameConfig cfg)
        {
            bInterval = cfg.Num("weapons.bouncer.fireInterval");
            bSpeed = cfg.Num("weapons.bouncer.speed");
            bUpward = cfg.Num("weapons.bouncer.upward");
            bSpikeDirY = cfg.Num("weapons.bouncer.spikeDirY");
            bVariantRadius = cfg.Num("weapons.specials.bouncer.variantRadius");
        }

        /// <summary>The trigger (guns.js step: case 'bouncer').</summary>
        void FireBouncerIfReady(double dt)
        {
            if (CooldownOf("bouncer") == 0)
            {
                SetCooldown("bouncer", MovementMath.StepsFor(bInterval * TuneInterval("bouncer") / fireRateMult, dt));
                Lob();
            }
        }

        /// <summary>The blast radius multiplier of a grenade fired now: momentum (+25% at M = 1) x a weapon's variant.</summary>
        double BlastMult(string id, double variantMult) =>
            MomentumMath.MomentumRadius(momentum.enabled ? AimMomentum() : 0, momentum.Params) * (variantMult != 0 && Variant19(id) ? variantMult : 1);

        void Lob()
        {
            NewShot();
            MarkShot("bouncer");
            var (origin, dir) = getAim();
            assisted = assist != null && assist.Aim();
            events.Emit("fire", "bouncer");
            if (arsenal.enabled) arsenal.Use("bouncer");
            // Spiked at your feet (aimed steeply down) it doesn't bounce: it sticks and pops a moment later (the grenade jump).
            bool spike = dir.y < -bSpikeDirY;
            Vec3 vel = dir * bSpeed;
            if (!spike) vel.y += bUpward;
            Projectiles.Projectile p = projectiles?.Launch("grenade", this, this, origin, vel, null, assisted);
            if (p != null)
            {
                p.spike = spike;
                CookArm26(p); // NIGHT26 weaponIdentity: a cooked grenade starts that far into its fuse
                p.radiusMult = BlastMult("bouncer", bVariantRadius);
                if (TuneOn) p.radiusMult *= TuneRadius("bouncer"); // NIGHT20 U3-34.5: the retune's radius knob
            }
        }

        /// <summary>The BOUNCER's alt (guns.js step: special pressed): every grenade of yours in flight bursts now.</summary>
        void BouncerAlt()
        {
            if (projectiles != null && projectiles.Airburst(this) > 0) bouncerAirbursts++;
        }

        public int bouncerAirbursts; // guns.js stats.airbursts
    }
}
