// NIGHT19 U3-33.6 (builder W, design.arsenal): the ZAPPER, ported line for line from skygrind/src/combat/guns.js
// (p17-night6) with the web's comments, as a part of the loadout (guns.js's `kit` is the loadout here):
// - ZAPPER (volt): a hitscan arc that chains to up to 3 more enemies (SHOCK). Alt: a discharge that stuns every SHOCKED
//   enemy you can see. Variant: 2 more chain links. Synergy: +1 chain while airborne.
// Port notes:
// - guns.js zap() / zapperAlt() / the heat in step(); the web's numbers (weapons.zapper, weapons.specials.zapper).
// - The web's zoom (ads.weapons.zapper.chainRange 1.15, POLISH V2) never applies: this port zooms only the KESTREL
//   (Loadout.StepAds), so the chain range is the unzoomed one; with design.arsenal the alt (LT) is the discharge.
// - No Drone orbs (`hit.type === 'orb'`): nothing can hit an orb in this port.
// - The arc's points go out in a reused ZapEvent (N19Events.Zap / .Discharge; at most ZapEvent.points.Length points,
//   the rest of a discharge's stunned bodies are stunned but not drawn). Allocation-free per shot.
// - kit.peekAim() (the aim without the assist's bend) is getAim(); kit.aimShot() is the assist's Aim().
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed partial class Loadout
    {
        HeatState zapHeat;
        NailgunParams zapParams;
        double zDamage, zInterval, zRange, zHeadMult, zChainRange, zChainFalloff;
        int zChains, zAirChains, zVariantChains;
        double zAltStun, zAltDamage, zAltRange, zAltCooldown;
        int zapAltCooldown; // steps (guns.js altCooldowns.zapper)
        readonly ZapEvent zapEvent = new ZapEvent();
        readonly ZapEvent dischargeEvent = new ZapEvent();
        readonly List<Vec3> chainPoints = new List<Vec3>();
        readonly List<Enemy> chainEnemies = new List<Enemy>();
        readonly List<ChainLink> links = new List<ChainLink>();
        Func<int, Vec3, bool> chainReach;
        public int zapDischarges, zapStunned; // guns.js stats.discharges / stunned

        /// <summary>The web's ZAPPER numbers, read once (the constructor).</summary>
        void InitZapper(GameConfig cfg)
        {
            zapHeat = new HeatState();
            zapParams = NailgunParams.From(cfg, "weapons.zapper");
            zDamage = cfg.Num("weapons.zapper.damage");
            zInterval = cfg.Num("weapons.zapper.fireInterval");
            zRange = cfg.Num("weapons.zapper.range");
            zHeadMult = cfg.Num("weapons.zapper.headshotMult");
            zChains = cfg.Int("weapons.zapper.chains");
            zChainRange = cfg.Num("weapons.zapper.chainRange");
            zChainFalloff = cfg.Num("weapons.zapper.chainFalloff");
            zAirChains = cfg.Int("weapons.zapper.airChains");
            zVariantChains = cfg.Int("weapons.specials.zapper.variantChains");
            zAltStun = cfg.Num("weapons.specials.zapper.stun");
            zAltDamage = cfg.Num("weapons.specials.zapper.damage");
            zAltRange = cfg.Num("weapons.specials.zapper.range");
            zAltCooldown = cfg.Num("weapons.specials.zapper.cooldown");
            chainReach = (i, from) => HasLos(from, chainPoints[i]);
        }

        void ResetZapper()
        {
            if (zapHeat == null) return;
            zapHeat.heat = 0;
            zapHeat.@lock = 0;
            zapAltCooldown = 0;
            zapDischarges = zapStunned = 0;
        }

        /// <summary>The ZAPPER's heat 0..1 and its overheat lock (the HUD, the viewmodel's glow).</summary>
        public double ZapperHeat => zapHeat.heat;
        public bool ZapperLocked => zapHeat.@lock > 0;

        /// <summary>guns.js step's ZAPPER part, whatever is in hand: its alt cooldown, and its heat (holding the trigger heats it).</summary>
        void StepZapperHeat(CombatInput input)
        {
            if (zapAltCooldown > 0) zapAltCooldown--;
            bool held = ready && input.fire.held;
            bool firing = held && Weapon == "zapper" && zapHeat.@lock <= 0;
            if (WeaponMath.HeatStep(zapHeat, firing, cfg.Dt, zapParams)) events.Emit("overheat", "zapper");
        }

        /// <summary>The trigger: an arc when cool and off cooldown (guns.js step: case 'zapper').</summary>
        void FireZapperIfReady(double dt)
        {
            if (zapHeat.@lock <= 0 && CooldownOf("zapper") == 0)
            {
                SetCooldown("zapper", MovementMath.StepsFor(zInterval * TuneInterval("zapper") / fireRateMult, dt));
                Zap();
            }
        }

        /// <summary>A Zapper arc: the first body along the aim, then a chain of the nearest others (each 70% of the last).</summary>
        void Zap()
        {
            NewShot(); // the chain's hits are one shot
            MarkShot("zapper");
            var (origin, dir) = getAim();
            assisted = assist != null && assist.Aim();
            events.Emit("fire", "zapper");
            int hits = 0;
            zapEvent.count = 0;
            bool wallHit = world.RayIntersect(origin, dir, out RayHit wall) && wall.distance <= zRange;
            EnemyRayHit? hit = enemies.Raycast(origin, dir, zRange);
            int extra = Variant19("zapper") ? zVariantChains : 0;
            if (hit.HasValue && (!wallHit || hit.Value.distance < wall.distance))
            {
                Enemy first = hit.Value.enemy;
                Vec3 hitAt = first.AimPoint;
                ZapPoint(zapEvent, hitAt);
                Damage(first, zDamage * (hit.Value.headshot ? zHeadMult : 1), hit.Value.headshot, "zapper", origin, assisted);
                hits++;
                if (first.alive && arsenal.enabled) arsenal.Status(first, "shock");
                // Candidates: every other live body near the chain's reach.
                chainPoints.Clear();
                chainEnemies.Clear();
                int chains = zChains + (mover.grounded ? 0 : zAirChains) + extra;
                double chainRange = zChainRange * ZapChainZoom; // POLISH V2: zoomed, +15% (NIGHT20: with design.zoom, Zoom20.cs)
                foreach (Enemy e in enemies.all)
                {
                    if (!e.alive || e == first) continue;
                    Vec3 aimAt = e.AimPoint;
                    if (Vec3.DistanceTo(aimAt, hitAt) > chainRange * chains) continue;
                    chainPoints.Add(aimAt);
                    chainEnemies.Add(e);
                }
                GunMath.ChainTargets(hitAt, chainPoints, chains, chainRange, zChainFalloff, chainReach, links);
                for (int k = 0; k < links.Count; k++)
                {
                    Enemy e = chainEnemies[links[k].index];
                    ZapPoint(zapEvent, chainPoints[links[k].index]);
                    Damage(e, zDamage * links[k].mult, false, "zapper", origin, assisted);
                    hits++;
                    if (e.alive && arsenal.enabled) arsenal.Status(e, "shock");
                }
            }
            else
            {
                ZapPoint(zapEvent, origin + dir * (wallHit ? wall.distance : zRange));
            }
            if (arsenal.enabled) arsenal.Use("zapper");
            zapEvent.owner = owner;
            zapEvent.hits = hits;
            events.Emit(N19Events.Zap, zapEvent);
            if (hits > 0) events.Emit("shotHit", "zapper");
            events.Emit("shotResult", new ShotEvent { owner = owner, weapon = "zapper", origin = origin, dir = dir, hitCount = hits, rayCount = 0 }); // the arc is the tracer
        }

        static void ZapPoint(ZapEvent e, Vec3 p)
        {
            if (e.count < e.points.Length) e.points[e.count++] = p;
        }

        /// <summary>ZAPPER: stun every SHOCKED enemy in sight (a small hit each, the shock is spent).</summary>
        void ZapperAlt(double dt)
        {
            if (zapAltCooldown > 0 || !arsenal.enabled) return;
            NewShot();
            var (origin, _) = getAim(); // kit.peekAim()
            dischargeEvent.count = 0;
            int n = 0;
            foreach (Enemy e in enemies.all)
            {
                if (!e.alive || !arsenal.HasStatus(e, "shock")) continue;
                Vec3 aimAt = e.AimPoint;
                if (Vec3.DistanceTo(aimAt, origin) > zAltRange || !HasLos(origin, aimAt)) continue;
                ZapPoint(dischargeEvent, aimAt);
                Damage(e, zAltDamage, false, "zapper", origin, false);
                if (e.alive) e.Stun(zAltStun, dt);
                n++;
            }
            zapAltCooldown = MovementMath.StepsFor(zAltCooldown, dt);
            zapDischarges++;
            zapStunned += n;
            if (arsenal.enabled) arsenal.Use("zapper");
            dischargeEvent.owner = owner;
            dischargeEvent.hits = n;
            events.Emit(N19Events.Discharge, dischargeEvent);
        }
    }
}
