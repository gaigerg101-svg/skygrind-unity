// NIGHT25 (builder C; design.n25.pulseCharge, shockwaveOff, meleeFire): the Loadout's half of the combat rows.
// - pulseCharge (GG: "the pulse is the weapon that should have the actual pulse for the left click that's built based off of how many
//   times you use it ... he's able to launch you but also do a little bit of damage"): Fist24 in pulse mode (Core/Fist24.cs) is the charge;
//   it is fed only by the PULSE GAUNTLET's own hits. Holding the left click swings the gauntlet over and over (each landed hit fills the
//   charge); LETTING GO of the left click with the PULSE in hand and a tier of charge launches the player along the aim
//   (Fist24.Flick: 0.5 / 1.0 / 2.25 jumps through Mover.Impulse, capped on the collider, all the charge spent, a 3 s lockout, one air flick)
//   and the launch hits whatever stands in the path: gauntlet damage x design.n25.pulseCharge.damageMult[tier - 1], a push, once each.
//   The long cooldown (CORE.md: a movement verb needs one) is the charge itself: a notch is hitsPerTier landed swings (about 3 s of
//   hitting) and the 3 s lockout follows every launch.
// - shockwaveOff (GG: "remove that shockwave effect from the shotgun"): the THUNDERCLAP keeps neither the shockwave nor NIGHT19's compress;
//   its LT zooms by design.n25.shockwaveOff.zoom. The switch only gates the existing code (Shockwave.cs, ShockwaveOn); off = NIGHT24.
// - meleeFire: the quick melee (StepPunch) and the gun's trigger already run in one step, each on its own cooldown (measured by the harness
//   `meleefire`: no row changes the sim there); the flag is the row's marker and the harness reads it.
using System;

namespace Skygrind.Core
{
    /// <summary>design.n25.pulseCharge's launch numbers.</summary>
    public sealed class Pulse25Params
    {
        public readonly double[] damageMult = new double[3];
        public double radius, hitGain;

        public static Pulse25Params From(GameConfig cfg)
        {
            const string p = "design.n25.pulseCharge.";
            var r = new Pulse25Params { radius = cfg.Num(p + "radius"), hitGain = cfg.Num(p + "gain.hit") };
            double[] m = cfg.Nums(p + "damageMult");
            for (int i = 0; i < 3; i++) r.damageMult[i] = m[i];
            return r;
        }
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT25 meleeFire's marker (set by the sim's hook; no rule reads it: the sim already runs both).</summary>
        public bool meleeFire25;

        /// <summary>NIGHT25 shockwaveOff: no shockwave and no compress on the THUNDERCLAP, its LT zooms shotgunZoom25 x.</summary>
        public bool shockwaveOff25;
        public double shotgunZoom25;

        /// <summary>NIGHT25 pulseCharge: the PULSE's release launch is live; `pulseFist` is the charge (Fist24 in pulse mode).</summary>
        public bool pulse25;
        public Fist24 pulseFist;
        public Pulse25Params pulseP;
        bool prevFire25;

        /// <summary>The run's pulse launches and the bodies they hit (the harness and the reel read them).</summary>
        public int pulseLaunches, pulseLaunchHits;

        /// <summary>One step of the PULSE's release: a left click let go with the gauntlet in hand launches the player if a notch is full.</summary>
        void StepPulse25(CombatInput input, double dt)
        {
            bool trigger = pulseLT26 ? input.altFire.held : input.fire.held; // NIGHT26 pulseLT: the blast is on LT, the click only swings
            bool release = prevFire25 && !trigger;
            prevFire25 = trigger;
            if (!release || pulseFist == null || !pulseFist.Active || pulseFist.noFlick || Weapon != "gauntlet" || pulseP == null) return; // noFlick: a bot or reel banking the charge
            if (!pulseFist.CanFlick) return;
            int tier = pulseFist.Tier;
            var (origin, aim) = getAim();
            if (!pulseFist.Flick()) return;
            pulseLaunches++;
            // The launch hits what is in its path: bodies within `radius` of the line from the eye along the aim, as far as the launch goes
            // (plus the radius), once each, the gauntlet's damage x the tier's share and the gauntlet's push. Its own damage does not feed the charge.
            double reach = pulseFist.lastAllowed + pulseP.radius;
            pulseFist.feedLocked = true;
            try
            {
                for (int i = 0; i < enemies.all.Count; i++)
                {
                    Enemy e = enemies.all[i];
                    if (!e.alive) continue;
                    Vec3 rel = e.AimPoint - origin;
                    double t = Vec3.Dot(rel, aim);
                    if (t < 0 || t > reach) continue;
                    Vec3 off = rel - aim * t;
                    if (off.Length > pulseP.radius + e.Radius + BossMargin34(e)) continue; // NIGHT34 reach34: + the boss margin (0 off: the same sum)
                    e.Knockback(aim.x, aim.z, gKnock, gKnockTime, dt);
                    Pushed20(e);
                    Damage(e, gDamage * pulseP.damageMult[tier - 1], false, "gauntlet", origin, false);
                    pulseLaunchHits++;
                }
            }
            finally { pulseFist.feedLocked = false; }
        }

        /// <summary>A run's start: the launch's edge detector and counters.</summary>
        public void ResetPulse25()
        {
            prevFire25 = false;
            pulseLaunches = pulseLaunchHits = 0;
        }

        // --- NIGHT34 reach34 (builder M; design.n34.reach34): the Pulse Gauntlet reaches a BOSS with bossMargin m more ---------------------------
        // GG 2026-10-11: "a slightly larger active hitbox when striking bosses like the worm". The enemy's hurtbox is inflated, the player's reach is
        // not (RESEARCH.md finding 7). The sim sets `reach34BossMargin` at each run's start (Game/SurvivalSim.Reach34.cs): the row's bossMargin, or 0
        // with the row off. 0 is NIGHT33's path exactly: GauntletTargets34 returns PunchTargets untouched and BossMargin34 adds +0.0 to a positive sum.
        // Used by the gauntlet's click swing and parry peek and the F punch (Combat.cs, the lead's / B's patch Tools/night/n34m-reach-patch.cjs), the
        // NIGHT25 release launch above and the NIGHT27 LT blast (PulseCharge27.cs).

        /// <summary>NIGHT34 reach34: metres added to a boss body's radius for the gauntlet's hits; 0 = off (NIGHT33).</summary>
        public double reach34BossMargin;

        /// <summary>NIGHT34 reach34: is this body a boss (the margin's bodies)? Every NIGHT27+ boss (IBossBody27) and the older boss bodies.</summary>
        public static bool IsBoss34(Enemy e) =>
            e is IBossBody27 || e is WyrmBody || e is GlacierBody || e is HulkBody || e is MonolithBody || e is RivalBody || e is BulwarkBody;

        /// <summary>NIGHT34 reach34: the margin a gauntlet hit adds to `e`'s radius (0 off or for a body that is not a boss).</summary>
        public double BossMargin34(Enemy e) => reach34BossMargin > 0 && IsBoss34(e) ? reach34BossMargin : 0;

        /// <summary>NIGHT34 reach34: the punch targets for the PULSE GAUNTLET: enemies.PunchTargets() with each boss's capsule inflated by the margin
        /// (the radius grows, the axis segment stays: the base drops and the height grows by the margin at each end). Off: PunchTargets as it is.
        /// The list is the registry's reused one, rebuilt on every PunchTargets call, so the inflation never outlives this call's user.</summary>
        public System.Collections.Generic.List<BodyTarget> GauntletTargets34()
        {
            System.Collections.Generic.List<BodyTarget> targets = enemies.PunchTargets();
            if (!(reach34BossMargin > 0)) return targets;
            double m = reach34BossMargin;
            for (int i = 0; i < targets.Count; i++)
            {
                BodyTarget t = targets[i];
                if (!(t.enemy is Enemy e) || !IsBoss34(e)) continue;
                t.basePos = new Vec3(t.basePos.x, t.basePos.y - m, t.basePos.z);
                t.height += 2 * m;
                t.radius += m;
            }
            return targets;
        }
    }
}
