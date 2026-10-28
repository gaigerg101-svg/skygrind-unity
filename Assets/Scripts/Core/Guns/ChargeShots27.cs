// NIGHT27 (builder W; design.n27.chargeShots27): RT held on the PISTOL (revolver, the KESTREL) and the SHOTGUN (the THUNDERCLAP). Unity-only.
// GG 2026-10-09: "the pistol, when you hold the right trigger, it'll do a charge attack but it's on a longer cool down. Same thing with the shotgun".
// - A tap fires at the press as today (the normal path, no latency). A hold past holdSeconds (0.4 s) starts a charge; the normal auto-fire waits while
//   it charges. The pistol is full fullSeconds (0.6 s) after that, the shotgun 0.8 s; the release of a FULL charge fires it. Let go before full: the
//   charge is cancelled, nothing is spent, no cooldown.
// - The charged pistol shot is damageMult (4) x a normal shot through up to `pierce` (3) bodies; the charged shotgun volley is the normal pellets at
//   damageMult (3) x the damage with a forward knock (knockMult x the gauntlet's). After a charged shot there is a cooldown of cooldownSeconds
//   (10 s / 14 s, whole steps) in which no charge can start; the normal shot is never blocked (the cooldown gates only the charge).
// - No movement slow while charging. Per run: reset at a run's start. Off (design.n27.chargeShots27 off, --off27): the normal shot only (charge27 null).
// Events (new names): chargeShot27Start {gun}, chargeShot27Fire {gun, hits}, chargeShot27Cancel {gun}.
using System;

namespace Skygrind.Core
{
    /// <summary>design.n27.chargeShots27's numbers (gun 0 = revolver, 1 = shotgun).</summary>
    public sealed class ChargeShots27Params
    {
        public double holdSeconds;
        public readonly double[] fullSeconds = new double[2], damageMult = new double[2], cooldownSeconds = new double[2];
        public int pierce;
        public double shotgunKnockMult;
        /// <summary>NIGHT31 W (design.n31.charge; 0 = never, NIGHT27's rule): a FULL charge still held this long fires itself, so a held trigger never
        /// stalls the gun (P0 a: the smoke's virtual pad and the attract bot hold RT for seconds and killed nothing while the charge waited for a release).</summary>
        public double autoReleaseSeconds;
        /// <summary>NIGHT31 W (R7 / R8; false = NIGHT27's rule): the slug's forward knock acts on husks only (never a boss), and a melee press cancels a building charge.</summary>
        public bool knockHusksOnly, meleeCancels;
        /// <summary>NIGHT31 item 2c (design.n31.charge.comboNails; 0 = off): a charged KESTREL shot through a body with stuck nails detonates them at this x.</summary>
        public double comboNailsMult;
        /// <summary>NIGHT31 (design.n31.charge.shotgun.slug; false = NIGHT27's charged pellet volley): the THUNDERCLAP's charged shot is one pellet line (FireSlug31).</summary>
        public bool slug;

        public static ChargeShots27Params From(GameConfig cfg) => From(cfg, "design.n27.chargeShots27.");

        /// <summary>The numbers under `p` (with its trailing dot): design.n27.chargeShots27. or design.n31.charge. (the NIGHT31 row adds autoReleaseSeconds,
        /// knockHusksOnly and meleeCancels; absent keys keep NIGHT27's rule).</summary>
        public static ChargeShots27Params From(GameConfig cfg, string p)
        {
            var r = new ChargeShots27Params { holdSeconds = cfg.Num(p + "holdSeconds"), pierce = cfg.Int(p + "revolver.pierce"), shotgunKnockMult = cfg.Num(p + "shotgun.knockMult") };
            if (cfg.Has(p + "autoReleaseSeconds")) r.autoReleaseSeconds = cfg.Num(p + "autoReleaseSeconds");
            if (cfg.Has(p + "knockHusksOnly")) r.knockHusksOnly = cfg.Bool(p + "knockHusksOnly");
            if (cfg.Has(p + "meleeCancels")) r.meleeCancels = cfg.Bool(p + "meleeCancels");
            if (cfg.Has(p + "shotgun.slug")) r.slug = cfg.Bool(p + "shotgun.slug");
            if (cfg.Has(p + "comboNails.enabled") && cfg.Bool(p + "comboNails.enabled")) r.comboNailsMult = cfg.Num(p + "comboNails.mult");
            string[] g = { "revolver", "shotgun" };
            for (int i = 0; i < 2; i++)
            {
                r.fullSeconds[i] = cfg.Num(p + g[i] + ".fullSeconds");
                r.damageMult[i] = cfg.Num(p + g[i] + ".damageMult");
                r.cooldownSeconds[i] = cfg.Num(p + g[i] + ".cooldownSeconds");
            }
            return r;
        }
    }

    public sealed class ChargeShot27Event
    {
        public string gun;
        public int hits;
    }

    /// <summary>The charge's state (the rule; the shots are Loadout.StepCharge27 below).</summary>
    public sealed class ChargeShots27
    {
        public readonly ChargeShots27Params p;
        readonly double dt;
        public bool active;

        /// <summary>Consecutive steps RT has been held on the gun in hand; the steps charged past the hold threshold; charging now.</summary>
        public int holdSteps, chargeSteps;
        public bool charging;

        /// <summary>Steps of each gun's charged-shot cooldown left (0 revolver, 1 shotgun).</summary>
        public readonly int[] cooldown = new int[2];

        /// <summary>The run's counters (the harness reads them).</summary>
        public readonly int[] fired = new int[2];
        public readonly int[] firedHits = new int[2];
        public int cancelled, normalFiredWhileCooling;

        public ChargeShots27(GameConfig cfg) : this(cfg, "design.n27.chargeShots27.") { }

        /// <summary>NIGHT31 W: the same rule with another config block's numbers (design.n31.charge.).</summary>
        public ChargeShots27(GameConfig cfg, string prefix)
        {
            p = ChargeShots27Params.From(cfg, prefix);
            dt = cfg.Dt;
        }

        /// <summary>NIGHT31: the steps a FULL charge may be held before it fires itself (0: never).</summary>
        public int AutoReleaseSteps => p.autoReleaseSeconds > 0 ? MovementMath.StepsFor(p.autoReleaseSeconds, dt) : 0;

        /// <summary>NIGHT31: the run's counters of charged shots that fired themselves and charges a melee press cancelled.</summary>
        public int autoReleased, meleeCancelled;

        public void Reset(bool on)
        {
            active = on;
            holdSteps = chargeSteps = 0;
            charging = false;
            cooldown[0] = cooldown[1] = 0;
            fired[0] = fired[1] = 0;
            firedHits[0] = firedHits[1] = 0;
            cancelled = normalFiredWhileCooling = 0;
            autoReleased = meleeCancelled = 0;
            c35?.Reset();
        }

        public static int Index(string weapon) => weapon == "revolver" ? 0 : weapon == "shotgun" ? 1 : -1;

        public int HoldStepsNeeded => MovementMath.StepsFor(p.holdSeconds, dt);
        public int FullSteps(int i) => MovementMath.StepsFor(c35 != null && c35.c40 ? c35.c40FullS[i] * (p.fullSeconds[i] / c35.c40BaseS[i]) : p.fullSeconds[i], dt); // NIGHT40 W1 charge40: its FULL x a shop33 ratio (1 exactly without one)
        public int CooldownSteps(int i) => c35 != null ? c35.CooldownSteps(i) : MovementMath.StepsFor(p.cooldownSeconds[i], dt); // NIGHT35 charge35: its own short cooldown

        /// <summary>NIGHT35 charge35 (Charge35.cs; the sim sets it at a run's start with the row on, null otherwise = this file's rule as before).</summary>
        public Charge35 c35;

        /// <summary>The charge's level 0..1 while charging with gun `i`.</summary>
        public double Level(int i) => charging ? Math.Min(1.0, chargeSteps / (double)FullSteps(i)) : 0;

        /// <summary>The share of gun `i`'s cooldown left, 0..1 (the crosshair arc's drain).</summary>
        public double CooldownShare(int i) => cooldown[i] <= 0 ? 0 : Math.Min(1.0, cooldown[i] / (double)CooldownSteps(i));

        public void Step()
        {
            if (cooldown[0] > 0) cooldown[0]--;
            if (cooldown[1] > 0) cooldown[1]--;
        }

        public void ClearHold()
        {
            holdSteps = chargeSteps = 0;
            charging = false;
        }
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT27 chargeShots27 (null with the row off).</summary>
        public ChargeShots27 charge27;

        readonly ChargeShot27Event chargeEv27 = new ChargeShot27Event();

        // A charged volley's damage and forward knock, live only while the shot's Damage calls run (Combat.cs Damage reads them).
        double sgChargeMult27 = 1, sgChargeKnock27;

        /// <summary>NIGHT31 item 2c: the nails' damage x while a charged shot's combo detonates them (Combat.cs Damage reads it; 1 otherwise), and the run's
        /// count of nails the combo set off (the harness reads it; reset with the loadout).</summary>
        double nailComboMult31 = 1;
        public int comboNails31;

        /// <summary>Per step, whatever is in hand (the sim's N27WeaponsStep): the charged-shot cooldowns run.</summary>
        public void StepCharge27Clock()
        {
            if (charge27 != null && charge27.active) charge27.Step();
        }

        /// <summary>The fire gate for the pistol and the shotgun (Combat.Step, before the normal trigger path). True: this step is spent (the charge is
        /// building, or the charged shot went); false: the normal path runs (a tap, a held auto-fire before the threshold, or a charge on cooldown).</summary>
        bool StepCharge27(string weapon, CombatInput input, double dt)
        {
            ChargeShots27 c = charge27;
            int i = ChargeShots27.Index(weapon);
            if (c == null || !c.active || i < 0) return false;
            if (c.c35 != null) return StepCharge35(weapon, i, input, dt); // NIGHT35 charge35: hold charges with no shots first (Charge35.cs)
            if (c.p.meleeCancels && input.punch.pressed && c.charging)
            {
                // NIGHT31 R8: a melee press while a charge builds cancels it, nothing spent (the w29 lockout: fire and melee never overlap)
                c.ClearHold();
                c.cancelled++;
                c.meleeCancelled++;
                chargeEv27.gun = weapon;
                events.Emit("chargeShot27Cancel", chargeEv27);
                return true; // this step is the punch's
            }
            if (!input.fire.held)
            {
                bool wasCharging = c.charging;
                int steps = c.chargeSteps;
                c.ClearHold();
                if (!wasCharging) return false;
                if (steps >= c.FullSteps(i)) return FireCharged27(weapon, i, dt);
                c.cancelled++;
                chargeEv27.gun = weapon;
                events.Emit("chargeShot27Cancel", chargeEv27);
                return false;
            }
            // A hold is ONE press: a new press edge (a bot, the smoke or the fingerprint script pressing every few steps while it holds the trigger, or a tapping
            // thumb) restarts the hold, so only an unbroken single-press hold charges: a tap is never delayed and a pressing script keeps its normal auto-fire.
            if (input.fire.pressed) c.ClearHold();
            if (c.cooldown[i] > 0)
            {
                if (!c.charging) c.holdSteps = 0; // no charge can start while the last charged shot cools: the normal fire runs
                return false;
            }
            c.holdSteps++;
            if (c.holdSteps <= c.HoldStepsNeeded) return false; // a tap, or a hold not yet long enough: the normal shot, no latency
            if (!c.charging)
            {
                c.charging = true;
                chargeEv27.gun = weapon;
                events.Emit("chargeShot27Start", chargeEv27);
            }
            c.chargeSteps++;
            int auto = c.AutoReleaseSteps;
            if (auto > 0 && c.chargeSteps >= c.FullSteps(i) + auto)
            {
                // NIGHT31 P0 a: a full charge held autoReleaseSeconds more fires itself; the hold then runs the normal auto-fire through the cooldown
                c.ClearHold();
                c.autoReleased++;
                return FireCharged27(weapon, i, dt);
            }
            return true; // charging: the auto-fire waits, the trigger is a charge
        }

        /// <summary>Switching guns while a charge builds drops it (called by the sim's hook each step; nothing spent).</summary>
        public void Charge27WeaponGuard()
        {
            ChargeShots27 c = charge27;
            if (c == null || !c.active) return;
            if (c.c35 != null)
            {
                // NIGHT35 charge35: any swap drops the hold and a queued tap (StepCharge35 also checks the gun each step)
                int gi = ChargeShots27.Index(Weapon);
                if (c.c35.holding && c.c35.gun != gi) { c.c35.Clear(); c.ClearHold(); }
                if (c.c35.tapQueued && c.c35.tapGun != gi) c.c35.tapQueued = false;
                return;
            }
            if (!c.charging) return;
            string w = Weapon;
            if (w != "revolver" && w != "shotgun") c.ClearHold();
        }

        bool FireCharged27(string weapon, int i, double dt)
        {
            ChargeShots27 c = charge27;
            int hits;
            if (weapon == "revolver")
            {
                SetCooldown("revolver", MovementMath.StepsFor(revInterval * TuneInterval("revolver") / fireRateMult, dt));
                // NIGHT33 pierce33 (Pierce33.cs): the LONGWAVE's pierce at its own damage x; off: n31's numbers as before
                hits = pierce33 != null ? FireRevolverCharged27(pierce33.damageMult, pierce33.pierce) : FireRevolverCharged27(c.p.damageMult[0], c.p.pierce);
            }
            else
            {
                SetCooldown("shotgun", MovementMath.StepsFor(sgInterval * TuneInterval("shotgun") * sgIntervalMult24 * sgIntervalMult29 / fireRateMult, dt));
                sgChargeMult27 = c.p.damageMult[1];
                sgChargeKnock27 = c.p.shotgunKnockMult;
                try
                {
                    if (c.p.slug) hits = FireSlug31(); // NIGHT31 (design.n31.charge.shotgun.slug): one pellet line
                    else
                    {
                        if (Arsenal19On && a19 != null) FireShotgun19(dt);
                        else FireShotgun();
                        hits = order.Count;
                    }
                }
                finally
                {
                    sgChargeMult27 = 1;
                    sgChargeKnock27 = 0;
                }
            }
            c.cooldown[i] = c.CooldownSteps(i);
            c.fired[i]++;
            c.firedHits[i] += hits;
            chargeEv27.gun = weapon;
            chargeEv27.hits = hits;
            events.Emit("chargeShot27Fire", chargeEv27);
            return true;
        }

        /// <summary>NIGHT31 the THUNDERCLAP's SLUG (DESIGN31 A): one pellet line along the aim (no spread), the first body in reach before a wall takes a whole
        /// volley's damage (pellets x pellet damage) x damageMult, with the charged forward knock (Damage: husks only while knockHusksOnly). Returns the hits (0 / 1).
        /// The run's slug hits on a non-Husk body (a boss) are counted for the harness.</summary>
        int FireSlug31()
        {
            NewShot();
            var (origin, aim) = getAim();
            assisted = assist != null && assist.Aim();
            events.Emit("fire", "shotgun");
            perEnemy.Clear();
            order.Clear();
            bool wallHit = world.RayIntersect(origin, aim, out RayHit wall) && wall.distance <= sgRange;
            EnemyRayHit? hit = enemies.Raycast(origin, aim, sgRange);
            var shot = new ShotEvent { owner = owner, weapon = "shotgun", origin = origin, dir = aim, end = origin + aim * sgRange, surface = "world" };
            int hits = 0;
            if (hit.HasValue && (!wallHit || hit.Value.distance < wall.distance))
            {
                Enemy e = hit.Value.enemy;
                order.Add(e);
                Damage(e, sgPelletDamage * sgPellets, false, "shotgun", origin, assisted);
                if (!(e is Husk)) slugBossHits31++;
                hits = 1;
                shot.end = origin + aim * hit.Value.distance;
                shot.surface = "enemy";
                events.Emit("shotHit", "shotgun");
                if (slugPierce33 > 0) hits += SlugPierce33(origin, aim, e, hit.Value.distance, wallHit ? wall.distance : double.PositiveInfinity, ref shot); // NIGHT33 P
            }
            else if (wallHit) shot.end = origin + aim * wall.distance;
            if (slug43 != null) Slug43Impact(shot.end, aim, wallHit && shot.surface != "enemy"); // NIGHT43 W1 slug43: the throw, the stun, the blast, `slug43` (Core/Slug43.cs; off: nothing)
            ShotgunJump43(aim); // NIGHT43 boost43 (Core/Boost43.cs): airborne the shotgun jump, a ground slug waits for a takeoff; the shotgun jump, as the volley's (NIGHT42 boost42: Core/Boost42.cs, the web's line with the row off)
            if (arsenal.enabled) arsenal.Use("shotgun");
            shot.hitCount = hits;
            events.Emit("shotResult", shot);
            return hits;
        }

        /// <summary>NIGHT31: the run's slug hits on a non-Husk body (the harness reads it).</summary>
        public int slugBossHits31;

        /// <summary>NIGHT33 P (design.n33.shop33 BORE SLUG; the sim sets it at a run's start, 0 otherwise): the bodies the charged slug goes on through after
        /// its first. 0 = NIGHT31's slug (one body; FireSlug31 never calls SlugPierce33).</summary>
        public int slugPierce33;

        /// <summary>NIGHT33 P: the slug past its first body: up to slugPierce33 more bodies along the same line before the wall, each taking the slug's full
        /// damage (the charged multiplier and knock still live: FireCharged27's try). Returns the extra hits; moves the tracer's end to the last body.</summary>
        int SlugPierce33(Vec3 origin, Vec3 aim, Enemy first, double firstDist, double wallDist, ref ShotEvent shot)
        {
            int extra = 0;
            Enemy last = first;
            double travelled = firstDist + 0.9; // past the body, as the charged KESTREL's pierce steps
            for (int k = 0; k < slugPierce33 && travelled < sgRange; k++)
            {
                EnemyRayHit? next = enemies.Raycast(origin + aim * travelled, aim, sgRange - travelled);
                if (!next.HasValue || next.Value.enemy == last) break;
                double at = travelled + next.Value.distance;
                if (at > wallDist) break;
                Enemy e = next.Value.enemy;
                order.Add(e);
                if (e.alive) Damage(e, sgPelletDamage * sgPellets, false, "shotgun", origin, assisted);
                if (!(e is Husk)) slugBossHits31++;
                extra++;
                last = e;
                shot.end = origin + aim * at;
                travelled = at + 0.9;
            }
            return extra;
        }

        /// <summary>The charged pistol shot: a ray that goes through up to `pierce` bodies (a wall stops it), each taking mult x a normal shot.</summary>
        /// <summary>NIGHT31 (the sim sets it: design.n31.unlocks and the room in play has builder L's standIns31, f8's pierce line): a charged KESTREL shot hurts
        /// the room's stand-ins from anywhere (NIGHT29's stand-ins take only shots fired from a wall ride). chargedShot31: a charged KESTREL shot is running.</summary>
        public bool standInPierce31;
        bool chargedShot31;

        int FireRevolverCharged27(double mult, int pierce)
        {
            chargedShot31 = true;
            try { return FireRevolverCharged27Body(mult, pierce); }
            finally { chargedShot31 = false; }
        }

        int FireRevolverCharged27Body(double mult, int pierce)
        {
            NewShot();
            var (origin, aim) = getAim();
            assisted = assist != null && assist.Aim();
            Vec3 dir = aim;
            events.Emit("fire", "revolver");
            double wallDist = world.RayIntersect(origin, dir, out RayHit wall) && wall.distance <= revRange ? wall.distance : double.PositiveInfinity;
            var shot = new ShotEvent { owner = owner, weapon = "revolver", origin = origin, dir = dir, end = origin + dir * revRange };
            Vec3 from = origin;
            double travelled = 0;
            int hits = 0;
            Enemy last = null;
            for (int k = 0; k < pierce; k++)
            {
                EnemyRayHit? next = enemies.Raycast(from, dir, revRange - travelled);
                if (!next.HasValue || next.Value.enemy == last) break;
                double at = travelled + next.Value.distance;
                if (at > wallDist) break;
                EnemyRayHit h = next.Value;
                double combo = charge27 != null ? charge27.p.comboNailsMult : 0;
                if (combo > 0 && projectiles != null && projectiles.StuckOn(h.enemy, this) > 0)
                {
                    // NIGHT31 item 2c: the body's stuck nails go off first (so they also go off on a kill), at combo x their detonate damage
                    nailComboMult31 = combo;
                    try { comboNails31 += projectiles.DetonateOn(h.enemy, this); }
                    finally { nailComboMult31 = 1; }
                    events.Emit("chargeCombo31", h.enemy);
                }
                if (h.enemy.alive) Damage(h.enemy, revDamage * mult * (h.headshot ? revHeadMult : 1), h.headshot, "revolver", origin, assisted);
                hits++;
                last = h.enemy;
                shot.end = origin + dir * at;
                shot.surface = "enemy";
                double step = h.distance + 0.9; // past the body, as RevolverPierce does
                travelled += step;
                from = from + dir * step;
            }
            if (pierce33 != null) Pierce33Finish(shot, origin, dir, wallDist, wall, hits); // NIGHT33: the tracer ends at the wall, the beam event
            else if (hits == 0 && !double.IsInfinity(wallDist))
            {
                shot.end = origin + dir * wallDist;
                shot.surface = "world";
                shot.normal = wall.normal;
            }
            if (hits > 0) events.Emit("shotHit", "revolver");
            if (arsenal.enabled) arsenal.Use("revolver");
            shot.hitCount = hits;
            events.Emit("shotResult", shot);
            return hits;
        }
    }
}
