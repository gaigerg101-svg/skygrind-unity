// NIGHT20 U3-34.3 (builder G, design.shockwave): the THUNDERCLAP's shockwave on LT. GG 2026-10-06 evening: "the left was
// supposed to be for the first option and that was supposed to do the shockwave, not the pistol ... There needs to be a
// long, long cooldown, like maybe every 15 seconds or even longer." The SHOCKWAVE switch: 18 s, one charge.
// One press (LT, or the SPECIAL key: the THUNDERCLAP doesn't zoom, so ads.js routeSpecial makes LT its special), full
// strength (no compress hold: a 0.6 s hold on an 18 s ability is a second tax):
// - the web's double blast (weapons.js fireDoubleBlast, :606-637) at full compress: weapons.specials.shotgun.pellets (16)
//   pellets of the THUNDERCLAP's pelletDamage in a coneDeg (10) cone, every body hit pushed `push` (16) m/s for pushTime
//   (0.3) s; the primary then waits one pump (its own fire interval: the web's 1.4 s `cooldown` was the cooldown of a
//   special you could fire every 1.4 s, and THE RANGE's C2 lost 1.4 s to it); with the NIGHT19 sandbox on
//   (Loadout.Arsenal19On) its pellets take the THUNDERCLAP's point-blank falloff like the primary's;
// - the shock cone (weapons.js coneWave, :640-656; weapons.specials.shotgun.shock: 30 deg, 8 m, push 12, stagger 1 s) on
//   design.shockwave.lightKinds for design.shockwave.pushTime s (the gust's measured numbers; the heavy kinds stand);
// - the self push design.shockwave.selfPush m/s opposite the aim through Mover.Impulse (the gust's: looking down it lifts
//   the player the measured +2.6 m; SHOCKWAVE LIFT: KEEP), instead of the web's 22 x 0.35 ground shove.
// Never in a dash (no charge spent). Charges: design.shockwave.charges (1), forgeCharges (2) with the THUNDERCLAP forged
// (Loadout.forgedAny); each charge comes back after design.shockwave.cooldown s (18; never below minCooldown, 12),
// ticking while holstered (holsterTicks); no air charge. Events: the web's `doubleBlast` and `coneWave`, plus `shockwave`
// (each a ShockwaveEvent) and `shockwaveReady` (the charges now) for GG's sounds and the HUD's ring. The run's charges are
// not in the run snapshot (as NIGHT19's gust: a snapshot restores a full charge). Allocation-free (reused list, payload).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.shockwave plus the web's double blast and cone (weapons.specials.shotgun), read once. Null when the
    /// design config has no shockwave numbers (the web's config alone: the golden sets), so it never fires there.</summary>
    public sealed class ShockwaveParams
    {
        public double cooldown, minCooldown, selfPush, pushTime;
        public int charges, forgeCharges;
        public bool holsterTicks;
        public readonly List<string> lightKinds = new List<string>();
        // the web's double blast (weapons.specials.shotgun) and its cone (.shock)
        public int dbPellets;
        public double dbConeDeg, dbPush, dbPushTime, dbCooldown;
        public double coneDeg, range, push, stagger;

        public static ShockwaveParams From(GameConfig cfg)
        {
            if (!cfg.Has("design.shockwave.cooldown")) return null;
            string s = "design.shockwave.", w = "weapons.specials.shotgun.";
            var p = new ShockwaveParams
            {
                cooldown = cfg.Num(s + "cooldown"),
                minCooldown = cfg.Num(s + "minCooldown"),
                selfPush = cfg.Num(s + "selfPush"),
                pushTime = cfg.Num(s + "pushTime"),
                charges = cfg.Int(s + "charges"),
                forgeCharges = cfg.Int(s + "forgeCharges"),
                holsterTicks = cfg.Design("shockwave.holsterTicks"),
                dbPellets = cfg.Int(w + "pellets"),
                dbConeDeg = cfg.Num(w + "coneDeg"),
                dbPush = cfg.Num(w + "push"),
                dbPushTime = cfg.Num(w + "pushTime"),
                dbCooldown = cfg.Num(w + "cooldown"),
                coneDeg = cfg.Num(w + "shock.coneDeg"),
                range = cfg.Num(w + "shock.range"),
                push = cfg.Num(w + "shock.push"),
                stagger = cfg.Num(w + "shock.stagger"),
            };
            p.lightKinds.AddRange(cfg.Strs(s + "lightKinds"));
            return p;
        }

        /// <summary>The cooldown per charge, never below minCooldown.</summary>
        public double EffectiveCooldown => JsMath.Max(minCooldown, cooldown);
    }

    /// <summary>The shockwave's charges and cooldown (one per loadout; Loadout.shockwave): the HUD's ring reads it.</summary>
    public sealed partial class Shockwave
    {
        public readonly ShockwaveParams p;
        readonly double dt;
        int charges, max, cooldownSteps;
        /// <summary>Shockwaves fired this run (the stats, the farm's per-round count, the tests).</summary>
        public int fired;

        public Shockwave(ShockwaveParams p, double dt)
        {
            this.p = p;
            this.dt = dt;
            Reset();
        }

        /// <summary>Charges ready now.</summary>
        public int Charges => charges;
        /// <summary>Charges when full (forgeCharges with the THUNDERCLAP forged).</summary>
        public int MaxCharges => max;
        /// <summary>Seconds to the next charge (0 when full).</summary>
        public double CooldownLeft => charges >= max ? 0 : cooldownSteps * dt;
        /// <summary>Seconds per charge.</summary>
        public double Cooldown => p != null ? p.EffectiveCooldown : 0;
        /// <summary>A charge is ready.</summary>
        public bool Ready => charges > 0;

        int FullSteps => MovementMath.StepsFor(p.EffectiveCooldown, dt);

        /// <summary>A new run: full (the base charges).</summary>
        public void Reset()
        {
            max = charges = p != null ? p.charges : 0;
            cooldownSteps = 0;
            fired = 0;
        }

        /// <summary>
        /// One fixed step (before the fire): `maxNow` charges when full (the forge's second charge), `inHand` the THUNDERCLAP
        /// is in hand (the clock runs holstered too with holsterTicks). A charge that comes back emits shockwaveReady.
        /// </summary>
        public void Step(int maxNow, bool inHand, EventBus events)
        {
            max = maxNow;
            if (charges > max) charges = max;
            if (charges >= max)
            {
                cooldownSteps = 0;
                return;
            }
            if (!p.holsterTicks && !inHand) return;
            if (cooldownSteps <= 0) cooldownSteps = FullSteps; // a charge's clock starts (a forge's new charge)
            cooldownSteps--;
            if (cooldownSteps > 0) return;
            charges++;
            if (charges < max) cooldownSteps = FullSteps;
            events.Emit(N20Events.ShockwaveReady, charges);
        }

        /// <summary>Spend a charge (false: none ready). The clock starts if it wasn't running.</summary>
        public bool TrySpend()
        {
            if (charges <= 0) return false;
            if (charges >= max) cooldownSteps = FullSteps;
            charges--;
            fired++;
            return true;
        }
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT20 (design.shockwave): the THUNDERCLAP's shockwave (null without its numbers: the golden sets).</summary>
        public readonly Shockwave shockwave;
        /// <summary>NIGHT20: is a gun forged at all (Forge.IsForged; the shockwave's second charge)? Null: none.</summary>
        public Func<string, bool> forgedAny;
        readonly ShockwaveEvent shockwaveEvent = new ShockwaveEvent();
        readonly List<Enemy> nearShock = new List<Enemy>();

        /// <summary>The THUNDERCLAP's shockwave is this run's LT (design.shockwave on, its numbers there).</summary>
        public bool ShockwaveOn => n20.shockwave && shockwave != null && shockwave.p != null && !shockwaveOff25; // NIGHT25 shockwaveOff: GG took it off the shotgun

        /// <summary>NIGHT19's KESTREL gust: its own block, and only while design.shockwave is off (the shockwave replaces it).</summary>
        public bool GustOn => n19.gust && !n20.shockwave;

        int ShockwaveMax() => forgedAny != null && forgedAny("shotgun") ? shockwave.p.forgeCharges : shockwave.p.charges;

        /// <summary>The shockwave's clock (Step, every step whatever is in hand).</summary>
        void StepShockwave() => shockwave.Step(ShockwaveMax(), Weapon == "shotgun", events);

        /// <summary>LT on the THUNDERCLAP: the double blast, the cone and the self push, if a charge is ready and no dash runs.</summary>
        void FireShockwave(double dt)
        {
            if (mover.state == MoveState.Dash || !shockwave.TrySpend()) return;
            ShockwaveParams p = shockwave.p;
            var (origin, aim) = getAim();
            // 1. weapons.js fireDoubleBlast at full compress (compress = 1).
            NewShot();
            MarkShot("shotgun");
            assisted = assist != null && assist.Aim();
            events.Emit("fire", "shotgun");
            double half = p.dbConeDeg * Math.PI / 180;
            bool falloff = Arsenal19On && a19 != null;
            perEnemy.Clear();
            order.Clear();
            Vec3 firstEnd = origin + aim * sgRange;
            for (int i = 0; i < p.dbPellets; i++)
            {
                Vec3 d = CombatMath.SampleCone(aim, half, rng.Next(), rng.Next());
                bool wallHit = world.RayIntersect(origin, d, out RayHit wall) && wall.distance <= sgRange;
                EnemyRayHit? hit = enemies.Raycast(origin, d, sgRange);
                if (hit.HasValue && (!wallHit || hit.Value.distance < wall.distance))
                {
                    Enemy e = hit.Value.enemy;
                    if (!perEnemy.ContainsKey(e)) order.Add(e);
                    perEnemy[e] = (perEnemy.TryGetValue(e, out double v) ? v : 0) + sgPelletDamage * (falloff ? a19.Falloff(hit.Value.distance) : 1);
                    if (i == 0) firstEnd = origin + d * hit.Value.distance;
                }
                else if (wallHit && i == 0) firstEnd = origin + d * wall.distance;
            }
            int hits = order.Count;
            foreach (Enemy e in order)
            {
                Damage(e, perEnemy[e], false, "shotgun", origin, assisted);
                if (e.alive)
                {
                    e.Knockback(e.position.x - origin.x, e.position.z - origin.z, p.dbPush, p.dbPushTime, dt);
                    Pushed20(e); // NIGHT20 D1
                }
            }
            if (hits > 0) events.Emit("shotHit", "shotgun");
            // 2. weapons.js coneWave on the light kinds.
            int pushed = ShockCone(p, origin, aim, dt);
            // 3. The self push opposite the aim (mover.js impulse; never in a dash: refused above).
            mover.Impulse(-aim.x * p.selfPush, -aim.y * p.selfPush, -aim.z * p.selfPush);
            if (arsenal.enabled) arsenal.Use("shotgun", 2);
            SetCooldown("shotgun", MovementMath.StepsFor(sgInterval * TuneInterval("shotgun") / fireRateMult, dt)); // the primary waits one pump
            shockwaveEvent.owner = owner;
            shockwaveEvent.origin = origin;
            shockwaveEvent.dir = aim;
            shockwaveEvent.selfPush = p.selfPush;
            shockwaveEvent.pushed = pushed;
            shockwaveEvent.chargesLeft = shockwave.Charges;
            shockwaveEvent.cooldownLeft = shockwave.CooldownLeft;
            events.Emit(N20Events.DoubleBlast, shockwaveEvent);
            events.Emit(N20Events.ConeWave, shockwaveEvent);
            events.Emit(N20Events.Shockwave, shockwaveEvent);
            events.Emit("shotResult", new ShotEvent { owner = owner, weapon = "shotgun", origin = origin, dir = aim, end = firstEnd, surface = hits > 0 ? "enemy" : "world", rayCount = p.dbPellets });
        }

        /// <summary>weapons.js coneWave on design.shockwave.lightKinds: pushed and staggered. Returns how many.</summary>
        int ShockCone(ShockwaveParams p, Vec3 origin, Vec3 dir, double dt)
        {
            double cos = JsMath.Cos((p.coneDeg * Math.PI) / 180);
            int n = 0;
            enemies.Within(origin, p.range, nearShock);
            for (int i = 0; i < nearShock.Count; i++)
            {
                Enemy enemy = nearShock[i];
                if (!enemy.alive || !p.lightKinds.Contains(enemy.kind)) continue;
                Vec3 aim = enemy.AimPoint;
                Vec3 coneDir = aim - origin;
                double d = coneDir.Length;
                if (d > p.range || Vec3.Dot(coneDir, dir) / (d != 0 ? d : 1) < cos || !HasLos(origin, aim)) continue;
                enemy.Knockback(enemy.position.x - origin.x, enemy.position.z - origin.z, p.push, p.pushTime, dt);
                enemy.Stun(p.stagger, dt);
                Pushed20(enemy); // NIGHT20 D1
                n++;
            }
            return n;
        }
    }
}
