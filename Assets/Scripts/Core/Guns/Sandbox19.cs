// NIGHT19 U3-33.6 (builder W, design.arsenal): every gun has a job. The loadout's NIGHT19 rules, each behind Arsenal19On
// (n19.arsenal or NIGHT20's design.arsenal.n20; Forge19On for a forged gun's variant), each number in design.arsenal (Unity-only: the web's guns are tuned for a
// Zombies loop where one gun wins) or read where the web keeps it:
// - THUNDERCLAP, point blank: pellet damage x a steep falloff past design.arsenal.shotgun.falloffStart m; a blast at
//   point blank on a wind-up is a parry (ULTRAKILL's shotgun parry: the heal and the stamina refill of a punch parry);
//   its LT is the web's compress / double blast (weapons.js fireDoubleBlast, ported here line for line) and its momentum
//   variant the web's tighter cone + cone wave (weapons.js fireShotgun / coneWave), both unported until tonight.
// - The KESTREL's momentum variant (weapons.js fireRevolver: the bullet goes through one enemy), ported.
// - The combos (each a ComboEvent on N19Events.Combo: its name is H's callout):
//   POP: a KESTREL shot meeting a live COMET rocket or BOUNCER grenade of yours bursts it at x pop.mult radius and damage
//     (ULTRAKILL's core eject + shot);
//   LAUNCH + SNIPE: a COMET blast throws the light kinds it hits up (the web's enemies.launch, at the web's
//     momentum.shock.quake.launch speed); any hit on a launched enemy still in the air deals x launch.airshotMult
//     (ULTRAKILL's airshot);
//   CONDUCTOR: STITCHER nails in an enemy make it a battery: a ZAPPER or LONGWAVE hit on it arcs conductor.base +
//     perNail x its nails (up to max) to every enemy within conductor.radius, and spends the nails (ULTRAKILL's
//     conductor aftershock);
//   NAILBOMB: a COMET or BOUNCER blast on an enemy with STITCHER nails in it detonates them (the STITCHER's detonate, on
//     that enemy; ULTRAKILL's magnet nail cloud);
//   SWAP-CANCEL: the primed swap made the DOOM rhythm: the first hit of the gun you swapped to, within swapCancel.window
//     s of the last shot of the gun you left, deals x swapCancel.mult on top of PRIMED.
// Allocation-free per step (reused lists and payloads).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.arsenal, read once (null when the design config has no arsenal numbers: the golden sets).</summary>
    public sealed class Arsenal19Params
    {
        public double falloffStart, falloffEnd, falloffMin, parryRange, minCompress, coneWavePushTime;
        public double popRadius, popMult;
        public double launchUp, airshotMult;
        public readonly List<string> launchKinds = new List<string>();
        public double conductorRadius, conductorBase, conductorPerNail, conductorMax;
        public double swapCancelWindow, swapCancelMult;
        // the web's numbers the ported guns read (weapons.specials.shotgun)
        public double compressTime, dbPellets, dbConeDeg, dbSelfPush, dbPush, dbPushTime, dbCooldown, variantCone;
        public double shockConeDeg, shockRange, shockPush, shockStagger, revolverPierce;

        public static Arsenal19Params From(GameConfig cfg)
        {
            if (!cfg.Has("design.arsenal.shotgun")) return null;
            string s = "design.arsenal.";
            var p = new Arsenal19Params
            {
                falloffStart = cfg.Num(s + "shotgun.falloffStart"),
                falloffEnd = cfg.Num(s + "shotgun.falloffEnd"),
                falloffMin = cfg.Num(s + "shotgun.falloffMin"),
                parryRange = cfg.Num(s + "shotgun.parryRange"),
                minCompress = cfg.Num(s + "shotgun.minCompress"),
                coneWavePushTime = cfg.Num(s + "shotgun.coneWavePushTime"),
                popRadius = cfg.Num(s + "pop.radius"),
                popMult = cfg.Num(s + "pop.mult"),
                launchUp = cfg.Num("momentum.shock.quake.launch"),
                airshotMult = cfg.Num(s + "launch.airshotMult"),
                conductorRadius = cfg.Num(s + "conductor.radius"),
                conductorBase = cfg.Num(s + "conductor.base"),
                conductorPerNail = cfg.Num(s + "conductor.perNail"),
                conductorMax = cfg.Num(s + "conductor.max"),
                swapCancelWindow = cfg.Num(s + "swapCancel.window"),
                swapCancelMult = cfg.Num(s + "swapCancel.mult"),
                compressTime = cfg.Num("weapons.specials.shotgun.compressTime"),
                dbPellets = cfg.Num("weapons.specials.shotgun.pellets"),
                dbConeDeg = cfg.Num("weapons.specials.shotgun.coneDeg"),
                dbSelfPush = cfg.Num("weapons.specials.shotgun.selfPush"),
                dbPush = cfg.Num("weapons.specials.shotgun.push"),
                dbPushTime = cfg.Num("weapons.specials.shotgun.pushTime"),
                dbCooldown = cfg.Num("weapons.specials.shotgun.cooldown"),
                variantCone = cfg.Num("weapons.specials.shotgun.variantCone"),
                shockConeDeg = cfg.Num("weapons.specials.shotgun.shock.coneDeg"),
                shockRange = cfg.Num("weapons.specials.shotgun.shock.range"),
                shockPush = cfg.Num("weapons.specials.shotgun.shock.push"),
                shockStagger = cfg.Num("weapons.specials.shotgun.shock.stagger"),
                revolverPierce = cfg.Num("weapons.specials.revolver.pierce"),
            };
            p.launchKinds.AddRange(cfg.Strs(s + "launch.kinds"));
            return p;
        }

        /// <summary>The THUNDERCLAP's pellet multiplier at `distance` m: 1 up to falloffStart, then linear down to
        /// falloffMin at falloffEnd, falloffMin beyond.</summary>
        public double Falloff(double distance)
        {
            if (distance <= falloffStart) return 1;
            if (distance >= falloffEnd) return falloffMin;
            double t = (distance - falloffStart) / (falloffEnd - falloffStart);
            return 1 + (falloffMin - 1) * t;
        }
    }

    /// <summary>The combo names (the callouts' text, H's style meter; the `combo` event's payload name).</summary>
    public static class Combos19
    {
        public const string Pop = "POP";
        public const string LaunchSnipe = "LAUNCH + SNIPE";
        public const string Conductor = "CONDUCTOR";
        public const string Nailbomb = "NAILBOMB";
        public const string SwapCancel = "SWAP-CANCEL";
    }

    public sealed partial class Loadout
    {
        /// <summary>design.arsenal (null without its numbers: every rule here is off).</summary>
        public readonly Arsenal19Params a19;
        /// <summary>NIGHT19 (design.forge): is a gun forged (Forge.IsForged; null: none)? A forged gun fires its variant.</summary>
        public Func<string, bool> forgedVariant;
        double compress; // the THUNDERCLAP's compress 0..1 (weapons.js compress)
        int shotgunSpecialCooldown; // steps (weapons.js specialCooldowns.shotgun)
        readonly ComboEvent comboEvent = new ComboEvent();
        readonly List<Enemy> launched = new List<Enemy>(); // enemies a COMET blast threw up, until they land
        readonly List<bool> sniped = new List<bool>(); // launched[i] paid its LAUNCH + SNIPE callout
        readonly List<Enemy> near19 = new List<Enemy>(); // CONDUCTOR's targets (reused)
        readonly List<Enemy> parried19 = new List<Enemy>(); // the blast's parried bodies (reused)
        readonly List<bool> perfect19 = new List<bool>(); // ... and whether each parry was perfect (design.parry)
        string lastShotWeapon, swapFromWeapon; // SWAP-CANCEL: the last trigger pull's gun, and the gun a swap left
        double lastShotAt = double.NegativeInfinity, swapFromShotAt = double.NegativeInfinity;
        bool conducting; // CONDUCTOR's re-entry guard
        public int combos19; // combos landed this run (stats, tests)

        /// <summary>The THUNDERCLAP's compress 0..1 (the viewmodel, the HUD).</summary>
        public double Compress => compress;

        /// <summary>Is this enemy in the air from a COMET launch (LAUNCH + SNIPE's target)?</summary>
        public bool IsLaunched(Enemy e)
        {
            for (int i = 0; i < launched.Count; i++) if (launched[i] == e) return e.alive;
            return false;
        }

        void ComboEmit(string name, string weapon, Vec3 at, double bonus)
        {
            combos19++;
            comboEvent.owner = owner;
            comboEvent.name = name;
            comboEvent.weapon = weapon;
            comboEvent.at = at;
            comboEvent.bonus = bonus;
            events.Emit(N19Events.Combo, comboEvent);
            ComboAward20(name, weapon); // NIGHT20 B2: the combo's style award (Combos20.cs)
        }

        /// <summary>A new run's NIGHT19 state (Reset).</summary>
        void Reset19()
        {
            compress = 0;
            shotgunSpecialCooldown = 0;
            launched.Clear();
            sniped.Clear();
            lastShotWeapon = swapFromWeapon = null;
            lastShotAt = swapFromShotAt = double.NegativeInfinity;
            conducting = false;
            combos19 = 0;
            ResetZapper();
            if (arsenal != null) arsenal.fresh19On = Fresh19On; // design.freshness: ULTRAKILL's table for this run
        }

        /// <summary>The guns (catalog kind "gun") in the loadout: the freshness floor's count.</summary>
        int GunsOwned()
        {
            int n = 0;
            for (int i = 0; i < owned.Count; i++) if (arsenal.Catalog.Get(owned[i])?.kind == "gun") n++;
            return n;
        }

        /// <summary>
        /// NIGHT19: once a step after the sim's enemies moved (SurvivalSim.Arsenal19): launched enemies that landed come
        /// down (Airborne off), and each enemy's ConductiveNails is the number of this loadout's nails stuck in it.
        /// </summary>
        public void Step19()
        {
            if (!Arsenal19On) return;
            for (int i = launched.Count - 1; i >= 0; i--)
            {
                Enemy e = launched[i];
                if (!e.alive || (e is Husk h && h.grounded && h.velocity.y <= 0))
                {
                    e.Airborne = false;
                    launched.RemoveAt(i);
                    sniped.RemoveAt(i);
                }
            }
            if (projectiles == null) return;
            foreach (Enemy e in enemies.all) e.ConductiveNails = 0;
            projectiles.CountStuck(this);
        }

        /// <summary>A weapon's momentum variant fires now: momentum along the aim, or (design.forge) the gun is forged.</summary>
        bool Variant19(string weapon) => VariantNow() || (Forge19On && forgedVariant != null && forgedVariant(weapon));

        // --- the THUNDERCLAP ---------------------------------------------------------------------------------------

        /// <summary>The compress (weapons.js step, the THUNDERCLAP's alt): hold to compress, let go to fire the double blast.</summary>
        void StepCompress(CombatInput input, double dt)
        {
            if (shotgunSpecialCooldown > 0) shotgunSpecialCooldown--;
            if (Weapon != "shotgun" || !ready) compress = 0;
            if (!ready || Weapon != "shotgun") return;
            // Hold alt to compress; letting go fires the double blast (charged at least a little).
            if (input.special.held && shotgunSpecialCooldown == 0) compress = JsMath.Min(1, compress + dt / a19.compressTime);
            else if (compress > 0)
            {
                if (compress >= a19.minCompress) FireDoubleBlast(dt);
                compress = 0;
            }
        }

        /// <summary>
        /// The THUNDERCLAP with design.arsenal (weapons.js fireShotgun with its momentum variant, plus the point-blank
        /// falloff and the parry): pellets grouped per enemy, each pellet x Falloff(its distance).
        /// </summary>
        void FireShotgun19(double dt)
        {
            NewShot();
            MarkShot("shotgun");
            var (origin, aim) = ShotAim39(); // NIGHT40 W1 charge40 (Charge35.cs): getAim() exactly, unless a ground release volley fires aimed from its press
            assisted = assist != null && assist.Aim();
            events.Emit("fire", "shotgun");
            // The Thunderclap's momentum variant (N2): a tighter spread and a knockback cone shockwave.
            bool moving = Variant19("shotgun");
            double half = sgCone * Math.PI / 180 * (moving ? a19.variantCone : 1);
            if (moving) ConeWave19(origin, aim, dt);
            Vec3 firstEnd = origin + aim * sgRange;
            int hits = Pellets(origin, aim, half, sgPellets, sgPelletDamage, ref firstEnd);
            DealPellets(origin, dt, false);
            // Shotgun jump: airborne only, once per shot, opposite the aim.
            ShotgunJump43(aim); // NIGHT42 boost42 (Core/Boost42.cs): the web's line with the row off; NIGHT43 boost43 (Core/Boost43.cs): a ground volley waits for a takeoff
            if (arsenal.enabled) arsenal.Use("shotgun");
            events.Emit("shotResult", new ShotEvent { owner = owner, weapon = "shotgun", origin = origin, dir = aim, end = firstEnd, surface = hits > 0 ? "enemy" : "world" });
        }

        /// <summary>
        /// The Thunderclap's alt (N2): the compressed double blast: twice the pellets in a wider cone, a hard push on
        /// every body it hits, a big shotgun jump when airborne (a small shove on the ground), and a longer cooldown.
        /// </summary>
        void FireDoubleBlast(double dt)
        {
            NewShot();
            MarkShot("shotgun");
            var (origin, aim) = getAim();
            assisted = assist != null && assist.Aim();
            events.Emit("fire", "shotgun");
            double half = a19.dbConeDeg * Math.PI / 180;
            Vec3 firstEnd = origin + aim * sgRange;
            int hits = Pellets(origin, aim, half, (int)a19.dbPellets, sgPelletDamage * compress, ref firstEnd);
            DealPellets(origin, dt, true);
            double push = a19.dbSelfPush * compress * (mover.grounded ? 0.35 : 1);
            mover.Impulse(-aim.x * push, mover.grounded ? 0 : -aim.y * push, -aim.z * push);
            if (arsenal.enabled) arsenal.Use("shotgun", 2);
            SetCooldown("shotgun", MovementMath.StepsFor(a19.dbCooldown, dt));
            shotgunSpecialCooldown = MovementMath.StepsFor(a19.dbCooldown, dt);
            events.Emit("doubleBlast", compress);
            events.Emit("shotResult", new ShotEvent { owner = owner, weapon = "shotgun", origin = origin, dir = aim, end = firstEnd, surface = hits > 0 ? "enemy" : "world", rayCount = (int)a19.dbPellets });
        }

        /// <summary>Cast `count` pellets in a cone of half angle `half`; damage per enemy (x the falloff) into perEnemy / order. Returns the enemies hit.</summary>
        int Pellets(Vec3 origin, Vec3 aim, double half, int count, double pelletDamage, ref Vec3 firstEnd)
        {
            perEnemy.Clear();
            order.Clear();
            for (int i = 0; i < count; i++)
            {
                Vec3 d = CombatMath.SampleCone(aim, half, rng.Next(), rng.Next());
                bool wallHit = world.RayIntersect(origin, d, out RayHit wall) && wall.distance <= sgRange;
                EnemyRayHit? hit = enemies.Raycast(origin, d, sgRange);
                if (hit.HasValue && (!wallHit || hit.Value.distance < wall.distance))
                {
                    Enemy e = hit.Value.enemy;
                    if (!perEnemy.ContainsKey(e)) order.Add(e);
                    perEnemy[e] = (perEnemy.TryGetValue(e, out double v) ? v : 0) + pelletDamage * a19.Falloff(hit.Value.distance);
                    if (i == 0) firstEnd = origin + d * hit.Value.distance;
                }
                else if (wallHit && i == 0) firstEnd = origin + d * wall.distance;
            }
            return order.Count;
        }

        /// <summary>The grouped pellets land: a body at point blank in its wind-up is parried first (the shotgun parry), then the damage (and the double blast's push).</summary>
        void DealPellets(Vec3 origin, double dt, bool doubleBlast)
        {
            parried19.Clear();
            perfect19.Clear();
            foreach (Enemy e in order)
            {
                double dx = e.position.x - origin.x, dz = e.position.z - origin.z;
                if (CombatMath.IsParry(e.windupSteps) && JsMath.Hypot(dx, dz) <= a19.parryRange)
                {
                    parried19.Add(e);
                    perfect19.Add(IsPerfect(e.windupSteps));
                    Stagger(e, pStagger, dt);
                }
            }
            foreach (Enemy e in order)
            {
                Damage(e, perEnemy[e], false, "shotgun", origin, assisted);
                if (doubleBlast && e.alive) e.Knockback(e.position.x - origin.x, e.position.z - origin.z, a19.dbPush * compress, a19.dbPushTime, dt);
            }
            for (int i = 0; i < parried19.Count; i++) Parried(parried19[i], perfect19[i], "shotgun");
            if (order.Count > 0) events.Emit("shotHit", "shotgun");
        }

        /// <summary>The Thunderclap variant's cone shockwave: every body in a cone ahead is pushed and staggered (weapons.js coneWave).</summary>
        void ConeWave19(Vec3 origin, Vec3 dir, double dt)
        {
            double cos = JsMath.Cos((a19.shockConeDeg * Math.PI) / 180);
            enemies.Within(origin, a19.shockRange, near19);
            for (int i = 0; i < near19.Count; i++)
            {
                Enemy enemy = near19[i];
                Vec3 aim = enemy.AimPoint;
                Vec3 coneDir = aim - origin;
                double d = coneDir.Length;
                if (d > a19.shockRange || Vec3.Dot(coneDir, dir) / (d != 0 ? d : 1) < cos || !HasLos(origin, aim)) continue;
                enemy.Knockback(enemy.position.x - origin.x, enemy.position.z - origin.z, a19.shockPush, a19.coneWavePushTime, dt);
                enemy.Stun(a19.shockStagger, dt);
                Pushed20(enemy); // NIGHT20 D1
            }
            events.Emit("coneWave", owner);
        }

        // --- the KESTREL: its variant and POP ----------------------------------------------------------------------

        /// <summary>The Kestrel's momentum variant (weapons.js fireRevolver, N2): the bullet goes through one enemy (or more: specials.revolver.pierce).</summary>
        void RevolverPierce(Vec3 origin, Vec3 dir, EnemyRayHit hit)
        {
            Enemy from = hit.enemy;
            Vec3 beamFrom = origin + dir * (hit.distance + 0.9);
            double travelled = hit.distance + 0.9;
            for (int i = 0; i < a19.revolverPierce; i++)
            {
                EnemyRayHit? next = enemies.Raycast(beamFrom, dir, revRange - hit.distance);
                bool wall = world.RayIntersect(beamFrom, dir, out RayHit w);
                if (!next.HasValue || next.Value.enemy == from || (wall && w.distance < next.Value.distance)) break;
                Damage(next.Value.enemy, revDamage * (next.Value.headshot ? revHeadMult : 1), next.Value.headshot, "revolver", origin, assisted);
                from = next.Value.enemy;
                travelled += next.Value.distance + 0.9;
                beamFrom = beamFrom + dir * (next.Value.distance + 0.9);
            }
        }

        /// <summary>POP: the KESTREL's shot meets a live rocket / grenade of this loadout before anything else: it bursts at x pop.mult. True: the shot is spent.</summary>
        bool TryPop(Vec3 origin, Vec3 dir, double maxDistance, ShotEvent shot)
        {
            if (projectiles == null) return false;
            Projectiles.Projectile p = projectiles.PopTarget(this, origin, dir, maxDistance, a19.popRadius, out double distance);
            if (p == null) return false;
            Vec3 at = p.position;
            shot.end = origin + dir * distance;
            shot.surface = "none";
            projectiles.Pop(p, a19.popMult);
            ComboEmit(Combos19.Pop, "revolver", at, a19.popMult);
            return true;
        }

        // --- LAUNCH + SNIPE, NAILBOMB (a blast's hits, Projectiles.onBlastHit) and CONDUCTOR ------------------------

        /// <summary>After a blast of this loadout hit `e` (Projectiles calls it once the blast's damage is dealt).</summary>
        public void OnBlastHit19(Enemy e, string weapon, Vec3 point)
        {
            if (!Arsenal19On || !e.alive) return;
            if (weapon == "rocket" || weapon == "bouncer")
            {
                int nails = projectiles != null ? projectiles.DetonateOn(e, this) : 0;
                if (nails > 0) ComboEmit(Combos19.Nailbomb, weapon, e.AimPoint, nails);
            }
            if (weapon == "rocket" && e.alive && a19.launchKinds.Contains(e.kind) && e is Husk h)
            {
                h.Launch(a19.launchUp); // enemies.launch (husk.js / enemyKind.js launch: velocity.y = max(vy, up), off the ground)
                e.Airborne = true;
                Pushed20(e); // NIGHT20 D1: a COMET launch a trap can finish
                if (!IsLaunched(e))
                {
                    launched.Add(e);
                    sniped.Add(false);
                }
            }
        }

        /// <summary>LAUNCH + SNIPE: x airshotMult on a launched enemy still up (the first such hit pays the callout).</summary>
        double Airshot(Enemy e, string weapon, Vec3 at)
        {
            for (int i = 0; i < launched.Count; i++)
            {
                if (launched[i] != e) continue;
                if (!sniped[i])
                {
                    sniped[i] = true;
                    ComboEmit(Combos19.LaunchSnipe, ArsenalMath.WeaponOf(weapon), at, a19.airshotMult);
                }
                return a19.airshotMult;
            }
            return 1;
        }

        /// <summary>CONDUCTOR: a volt hit (ZAPPER, LONGWAVE) on an enemy with nails in it arcs to every enemy near it; the nails are spent.</summary>
        void Conduct(Enemy e, string weapon)
        {
            if (projectiles == null) return;
            int nails = projectiles.StuckOn(e, this);
            if (nails <= 0) return;
            double damage = JsMath.Min(a19.conductorMax, a19.conductorBase + a19.conductorPerNail * nails);
            Vec3 at = e.AimPoint;
            projectiles.ConsumeStuck(e, this);
            enemies.Within(at, a19.conductorRadius, near19);
            conducting = true;
            for (int i = 0; i < near19.Count; i++)
                if (near19[i].alive) Damage(near19[i], damage, false, weapon, at, false, 1, curShot);
            conducting = false;
            ComboEmit(Combos19.Conductor, weapon, at, damage);
        }

        // --- SWAP-CANCEL -------------------------------------------------------------------------------------------

        /// <summary>A trigger pull of `weapon` (SWAP-CANCEL's clock).</summary>
        void MarkShot(string weapon)
        {
            lastShotWeapon = weapon;
            lastShotAt = clock;
        }

        /// <summary>At a swap's raise: the gun left and when it last fired.</summary>
        void SwapIn19()
        {
            swapFromWeapon = lastShotWeapon;
            swapFromShotAt = lastShotAt;
        }

        /// <summary>SWAP-CANCEL: a primed hit within the window of the left gun's last shot pays x swapCancel.mult (once a swap).</summary>
        double SwapCancel(bool primed, string weapon, Vec3 at)
        {
            if (!primed || swapFromWeapon == null || swapFromWeapon == ArsenalMath.WeaponOf(weapon) || clock - swapFromShotAt > a19.swapCancelWindow) return 1;
            swapFromWeapon = null;
            ComboEmit(Combos19.SwapCancel, ArsenalMath.WeaponOf(weapon), at, a19.swapCancelMult);
            return a19.swapCancelMult;
        }
    }
}
