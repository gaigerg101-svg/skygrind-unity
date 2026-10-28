// NIGHT26 (builder W; design.n26.pulseLT, design.n26.weaponIdentity): weapon identity, the Loadout's half. GG 2026-10-08: "the grenade and rocket launcher are
// a little too broken they just clear enemies too fast ... almost all the weapons should really have a left trigger that either zooms in or does something else
// ... every single weapon has a positive a weakness and they work in certain scenarios ... you really don't feel incentivized at all to switch weapons".
// The table (strength, weakness, LT, the enemy it beats and the one it is bad at) is Design/weapons/WEAPON-IDENTITY.md; the numbers are config:
//   design.n26.identity     the ROCKET's and the BOUNCER's nerfs (D-8) and the grenade's cook: Identity26 below. Applied to the web's numbers at a run's start
//                           (Projectiles.ApplyIdentity26; nothing typed here), undone when the row is off.
//   design.n26.weapons.<w>  each gun's LT in words (`lt`, `_why`) and, where the LT stops being a zoom, `zoom` 0 (the STITCHER's and the ZAPPER's LT is their
//                           own alt: the detonate and the discharge that NIGHT20 left on the SPECIAL key).
//   design.n26.vulnerability  per enemy kind x weapon (or weapon class) damage multipliers: Vulnerability26 below, read in Loadout.Damage before the kind's filter.
//   design.n26.pulseLT      the PULSE's blast on LT: LT held swings the gauntlet (the hits fill the notches), LT released launches (Pulse25.cs StepPulse25).
// Off (--off26, a row off, a tape without the bit): every field below is null / false and the loadout is NIGHT25's bit for bit.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.n26.identity, read once: the ROCKET's and the BOUNCER's cost.</summary>
    public sealed class Identity26
    {
        public double rocketDamageMult, rocketSplashMult, rocketReload, rocketSelf;
        public double bouncerSplashMult = 1, bouncerDamageMult, bouncerFuse, bouncerSelf, cookMax;
        public int bouncerPool;
        public double selfRadiusFrac;

        /// <summary>A gun's LT zoom under the row (0: its LT is its own alt); only weapons whose row has a `zoom` key.</summary>
        public readonly Dictionary<string, double> zoom = new Dictionary<string, double>();

        public static Identity26 From(GameConfig cfg)
        {
            const string p = "design.n26.weaponIdentity.identity.";
            var r = new Identity26
            {
                rocketDamageMult = cfg.Num(p + "rocket.damageMult"),
                rocketSplashMult = cfg.Num(p + "rocket.splashMult"),
                rocketReload = cfg.Num(p + "rocket.reload"),
                rocketSelf = cfg.Num(p + "rocket.selfDamage"),
                bouncerDamageMult = cfg.Num(p + "bouncer.damageMult"),
                bouncerFuse = cfg.Num(p + "bouncer.fuse"),
                bouncerPool = cfg.Int(p + "bouncer.pool"),
                bouncerSelf = cfg.Num(p + "bouncer.selfDamage"),
                cookMax = cfg.Num(p + "bouncer.cookMax"),
                selfRadiusFrac = cfg.Num(p + "selfRadiusFrac"),
            };
            if (cfg.Has(p + "bouncer.splashMult")) r.bouncerSplashMult = cfg.Num(p + "bouncer.splashMult");
            if (cfg.Has("design.n26.weaponIdentity.weapons"))
                foreach (var kv in cfg.Obj("design.n26.weaponIdentity.weapons"))
                    if (kv.Value is Dictionary<string, object> row && row.TryGetValue("zoom", out object z)) r.zoom[kv.Key] = PortData.Num(z);
            return r;
        }
    }

    /// <summary>design.n26.vulnerability: the damage a weapon deals a kind of enemy, as a multiplier on what the hit would have dealt (x1 where there is no row).
    /// A kind's row maps a weapon (or the weapon's class, `class`) to a multiplier; its `windup` row is a second multiplier while the body is in its wind-up
    /// (the CHARGER's tell); the WARDEN's shield: a front hit lands `wardenSoft` x instead of nothing, in full from the weapons in `wardenFull`.</summary>
    public sealed class Vulnerability26
    {
        readonly Dictionary<string, string> weaponClass = new Dictionary<string, string>();
        readonly Dictionary<string, Dictionary<string, double>> kinds = new Dictionary<string, Dictionary<string, double>>();
        readonly Dictionary<string, Dictionary<string, double>> windups = new Dictionary<string, Dictionary<string, double>>();
        readonly HashSet<string> wardenFull = new HashSet<string>();
        public double wardenSoft;

        public static Vulnerability26 From(GameConfig cfg)
        {
            const string p = "design.n26.weaponIdentity.vulnerability";
            if (!cfg.Has(p)) return null;
            var v = new Vulnerability26 { wardenSoft = cfg.Num(p + ".warden.soft") };
            foreach (var kv in cfg.Obj(p + ".class")) if (kv.Value is string s) v.weaponClass[kv.Key] = s;
            foreach (string w in cfg.Strs(p + ".warden.full")) v.wardenFull.Add(w);
            foreach (var kv in cfg.Obj(p + ".kind"))
            {
                if (!(kv.Value is Dictionary<string, object> row)) continue;
                var m = new Dictionary<string, double>();
                foreach (var e in row)
                {
                    if (e.Key == "windup" && e.Value is Dictionary<string, object> wu)
                    {
                        var wm = new Dictionary<string, double>();
                        foreach (var x in wu) if (!x.Key.StartsWith("_")) wm[x.Key] = PortData.Num(x.Value);
                        v.windups[kv.Key] = wm;
                    }
                    else if (!e.Key.StartsWith("_") && !(e.Value is string)) m[e.Key] = PortData.Num(e.Value);
                }
                v.kinds[kv.Key] = m;
            }
            return v;
        }

        double Look(Dictionary<string, double> row, string weapon)
        {
            if (row.TryGetValue(weapon, out double a)) return a;
            if (weaponClass.TryGetValue(weapon, out string c) && row.TryGetValue(c, out double b)) return b;
            return 1;
        }

        /// <summary>The multiplier for `weapon` on a body of `kind` (windup: it is in its wind-up).</summary>
        public double Mult(string kind, string weapon, bool windup)
        {
            double m = 1;
            if (kinds.TryGetValue(kind, out var row)) m = Look(row, weapon);
            if (windup && windups.TryGetValue(kind, out var wu)) m *= Look(wu, weapon);
            return m;
        }

        /// <summary>What a hit that the WARDEN's shield would have absorbed lands as (x): 1 from the weapons that break it, `wardenSoft` from the rest.</summary>
        public double ShieldFactor(string weapon) => wardenFull.Contains(weapon) ? 1 : wardenSoft;

        /// <summary>The weapon's class (single / spread / pierce / chain / area / slow), or null.</summary>
        public string ClassOf(string weapon) => weaponClass.TryGetValue(weapon, out string c) ? c : null;
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT26 weaponIdentity: the rocket's and the BOUNCER's cost (null: the web's numbers).</summary>
        public Identity26 id26;

        /// <summary>NIGHT26 weaponIdentity: the per-enemy multipliers (null: x1 everywhere).</summary>
        public Vulnerability26 vuln26;

        /// <summary>NIGHT26 pulseLT: the PULSE's blast is on LT (the left click is a plain swing).</summary>
        public bool pulseLT26;

        int cookSteps;
        double dt26, midHeight26; // half the capsule: where a blast measures its distance to you (Projectiles.Blast's knockback point)

        /// <summary>The BOUNCER's cook in whole steps held so far (the grenade the next throw arms starts that old).</summary>
        public int CookSteps => cookSteps;

        /// <summary>The run's NIGHT26 loadout rows (called once at the run's start, after the fist is wired; a row off passes false).</summary>
        public void Apply26(GameConfig cfg, bool pulseLT, bool identity)
        {
            pulseLT26 = pulseLT;
            id26 = identity ? Identity26.From(cfg) : null;
            vuln26 = identity ? Vulnerability26.From(cfg) : null;
            rocketInterval = id26 != null ? id26.rocketReload : cfg.Num("weapons.rocket.fireInterval");
            projectiles?.ApplyIdentity26(id26);
            cookSteps = 0;
            midHeight26 = cfg.Num("body.capsuleHeight") * 0.5;
            dt26 = cfg.Dt;
        }

        /// <summary>LT held on the PULSE swings the gauntlet too (pulseLT): "hold to charge by use" - the landed hits fill the notches, the release launches.</summary>
        bool PulseLtSwing26(string weapon, CombatInput input) => pulseLT26 && !Pulse27Live && weapon == "gauntlet" && input.altFire.held; // NIGHT27 pulse27: LT is the gated blast, not a swing

        /// <summary>The BOUNCER's cook: LT held with the BOUNCER in hand (and no grenade of yours in the air: a press then airbursts them) counts steps, up to
        /// design.n26.identity.bouncer.cookMax s; the next throw's grenade is armed that many steps into its fuse. Let go of LT: the count is lost.</summary>
        void StepCook26(CombatInput input, double dt)
        {
            if (Weapon != "bouncer" || !input.altFire.held || projectiles == null || projectiles.Count("grenade", this) > 0)
            {
                cookSteps = 0;
                return;
            }
            int cap = MovementMath.StepsFor(id26.cookMax, dt);
            if (cookSteps < cap) cookSteps++;
        }

        /// <summary>A grenade thrown now is armed cookSteps into its fuse (never past its last step), and the count is spent.</summary>
        void CookArm26(Projectiles.Projectile p)
        {
            if (id26 == null || cookSteps <= 0 || p == null) return;
            p.ageSteps = Math.Min(cookSteps, projectiles.GrenadeFuseSteps(dt26) - 1);
            cookSteps = 0;
        }

        /// <summary>NIGHT26 weaponIdentity: a blast of yours that goes off within `frac` of its radius hurts you: selfDamage x the share of the radius left.</summary>
        public void SelfBlast26(string weapon, Vec3 point, double radius)
        {
            if (id26 == null) return;
            double max = weapon == "rocket" ? id26.rocketSelf : weapon == "bouncer" ? id26.bouncerSelf : 0;
            if (max <= 0 || radius <= 0) return;
            Vec3 m = mover.position;
            Vec3 mid = new Vec3(m.x, m.y + midHeight26, m.z);
            double d = Vec3.DistanceTo(point, mid);
            double reach = radius * id26.selfRadiusFrac;
            if (d >= reach) return;
            health.Damage(max * (1 - d / reach), "selfBlast", point);
        }
    }
}
