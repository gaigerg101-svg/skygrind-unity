// NIGHT20 U3-34.9 row B5 (builder G2, design.adds.b5): the VORTEX, ported line for line from skygrind/src/combat/guns.js
// (p17-night6) with the web's comments, as a part of the loadout (guns.js's `kit` is the loadout here):
// - VORTEX (gravity): a slow orb that opens into a 1.5 s pull (PULL), then bursts. Alt: a tap collapses it early;
//   held, it repels instead of pulling. Variant: a wider pull. Synergy: after a dash it flies 60% faster.
// Explosions (Bouncer, Vortex) grow their radius with the momentum at the trigger pull instead of their damage.
// Port notes:
// - guns.js orb() (:203-215), the alt in step() (:495-506), the trigger (:555-559), meter() (:589-592) and reset()'s
//   vortexAlt; the orbs fly in Projectiles (projectiles.js launch('vortex'), stepVortexes, openVortex, burstVortex,
//   vortexAlt: ported there tonight).
// - kit.motion.sinceDash is weapons.js's motion clock (:231: 0 in a DASH, else + dt), kept here while the row is on.
// - kit.aimShot() is the assist's Aim(); `every('vortex', dt)` is stepsFor(fireInterval(w.vortex.fireInterval, mods)):
//   the loadout's own cooldown table (SetCooldown / CooldownOf), / the QUICK perk, x the retune's interval knob.
// - The web emits no event for a collapse or a repel (stats only); the port adds vortexCollapse / vortexRepel (the orbs'
//   count) for GG's sounds. vortexOpen / vortexBurst are the web's (Projectiles).
// - No orbs hit (the Drone's): the sweep is the port's (the world and the bodies), as for every projectile here.
// Behind design.adds.b5 (the run's b5On): off, the VORTEX is not a port (Fires refuses it, the trigger does nothing).
using System;

namespace Skygrind.Core
{
    /// <summary>projectiles.js vortexOpen's payload: the orb's owner and where it opened.</summary>
    public sealed class VortexEvent
    {
        public string owner = "player";
        public Vec3 position;
    }

    public sealed partial class Loadout
    {
        public int vortexCollapses, vortexRepels; // guns.js stats.collapses / stats.repels

        bool vortexOk; // the web's VORTEX numbers are in the config (b5On also needs them)
        double vInterval, vSpeed, vDashWindow, vDashSpeedMult, vVariantPull, vRepelHold;
        double vortexPressAt = -1; // guns.js vortexAlt.pressAt (the guns' clock; -1: no press)
        bool vortexRepel; // guns.js vortexAlt.repel
        double sinceDash = double.PositiveInfinity; // weapons.js motion.sinceDash

        /// <summary>LT is down on the VORTEX (a press waiting for its release: a tap collapses, a hold repels).</summary>
        public bool VortexPressing => vortexPressAt >= 0;

        /// <summary>The web's VORTEX numbers, read once (the constructor; nothing without them: the golden sets).</summary>
        void Vortex20Init(GameConfig cfg)
        {
            if (!cfg.Has("weapons.vortex.fireInterval")) return;
            vortexOk = true;
            string w = "weapons.vortex.", s = "weapons.specials.vortex.";
            vInterval = cfg.Num(w + "fireInterval");
            vSpeed = cfg.Num(w + "speed");
            vDashWindow = cfg.Num(w + "dashWindow");
            vDashSpeedMult = cfg.Num(w + "dashSpeedMult");
            vVariantPull = cfg.Num(s + "variantPull");
            vRepelHold = cfg.Num(s + "repelHold");
        }

        /// <summary>guns.js reset()'s VORTEX part (a respawn, setSlot): no press, no repel.</summary>
        void Vortex20Reset()
        {
            vortexPressAt = -1;
            vortexRepel = false;
        }

        /// <summary>A new run: the stats and the dash clock too.</summary>
        void Vortex20RunReset()
        {
            Vortex20Reset();
            vortexCollapses = vortexRepels = 0;
            sinceDash = double.PositiveInfinity;
        }

        /// <summary>weapons.js step's motion clock, whatever is in hand (the row on).</summary>
        void StepVortexMotion(double dt) => sinceDash = mover.state == MoveState.Dash ? 0 : sinceDash + dt;

        /// <summary>
        /// guns.js step (ready, the VORTEX in hand): a tap collapses the orbs early; held past repelHold, open ones repel
        /// instead of pull.
        /// </summary>
        void VortexAltStep(CombatInput input)
        {
            if (input.special.pressed) vortexPressAt = clock;
            if (input.special.held && vortexPressAt >= 0 && clock - vortexPressAt >= vRepelHold)
            {
                if (!vortexRepel)
                {
                    int n = projectiles != null ? projectiles.VortexAlt(this, "repel") : 0;
                    if (n > 0)
                    {
                        vortexRepels++;
                        events.Emit(G20Events.VortexRepel, n);
                    }
                }
                vortexRepel = true;
            }
            else if (!input.special.held && vortexPressAt >= 0)
            {
                if (vortexRepel) projectiles?.VortexAlt(this, "pull");
                else
                {
                    int n = projectiles != null ? projectiles.VortexAlt(this, "collapse") : 0;
                    if (n > 0)
                    {
                        vortexCollapses++;
                        events.Emit(G20Events.VortexCollapse, n);
                    }
                }
                vortexPressAt = -1;
                vortexRepel = false;
            }
        }

        /// <summary>The trigger (guns.js step: case 'vortex'): an orb every weapons.vortex.fireInterval s while held.</summary>
        void FireVortexIfReady(double dt)
        {
            if (CooldownOf("vortex") == 0)
            {
                SetCooldown("vortex", MovementMath.StepsFor(vInterval * TuneInterval("vortex") / fireRateMult, dt));
                Orb();
            }
        }

        void Orb()
        {
            NewShot();
            MarkShot("vortex");
            var (origin, dir) = getAim();
            assisted = assist != null && assist.Aim();
            events.Emit("fire", "vortex");
            if (arsenal.enabled) arsenal.Use("vortex");
            Vec3 vel = dir * (vSpeed * (sinceDash <= vDashWindow ? vDashSpeedMult : 1));
            Projectiles.Projectile p = projectiles?.Launch("vortex", this, this, origin, vel, null, assisted);
            if (p != null)
            {
                p.radiusMult = BlastMult("vortex", 0);
                p.pullMult = Variant19("vortex") ? vVariantPull : 1;
            }
        }

        /// <summary>guns.js meter('vortex'): the reload (1 = ready).</summary>
        bool VortexMeter(out double value)
        {
            double full = MovementMath.StepsFor(vInterval / fireRateMult, cfg.Dt);
            value = 1 - JsMath.Min(1, CooldownOf("vortex") / JsMath.Max(1, full));
            return true;
        }
    }
}
