// Port of skygrind/src/combat/impact.js (OVERNIGHT2 N2 §3.2), NIGHT9 U3-09: impact shockwaves: a melee hit that lands
// with momentum above 0.55 sets off its weapon's own wave (one per 0.25 s). The wave deals 35..60% of the hit to the
// bodies around it (lerp over M), knocks them back 6..14 m/s and staggers them:
// - PULSE GAUNTLET (and the F punch): a flat ring round the target, 2.5..5 m, pushing everything outward.
// - SUNDER BLADE: a gold crescent that flies forward 10 m at 40 m/s and cuts through everything in its path.
// - FLICKER DAGGERS: three phantom echo stabs, on the target and the two nearest bodies within 5 m, at once.
// - THUMPER HAMMER: a 70 deg, 8 m ground quake cone that launches bodies upward (air targets), with a fissure decal.
// - TETHER WHIP: a sonic crack ring that drags the small ones to the impact point and staggers the big ones.
// Wave hits never set off another wave and carry no extra momentum (their share is of a hit that already had it).
// Fixed step; emits impactWave {owner, kind, weapon, position, dir, radius, M, hits} for fx/impactFx.js.
// Port notes: the ring (the PULSE GAUNTLET and the punch, the only melee in the port) is ported; the crescent, echo,
// quake and crack belong to the SUNDER BLADE, FLICKER DAGGERS, THUMPER HAMMER and TETHER WHIP, which arrive with
// their weapons (PORT-PLAN N12): until then trigger() refuses those kinds before it spends the cooldown.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>impactWave's payload (fx/impactFx.js draws it; the HUD and audio read it).</summary>
    public sealed class ImpactWaveEvent
    {
        public string owner = "player";
        public string kind, weapon;
        public double M, radius;
        public Vec3 position, feet, dir;
        public int hits;
    }

    public sealed class Impact : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot (the loadout's "impact" part): the wave's cooldown and the stats.
        public void Save(StateWriter w)
        {
            w.I("cooldown", cooldown);
            w.I("statWaves", statWaves);
            w.I("statHits", statHits);
            w.SI("byKind", byKind);
        }

        public void Load(StateReader r)
        {
            cooldown = r.Int("cooldown");
            statWaves = r.Int("statWaves");
            statHits = r.Int("statHits");
            r.SI("byKind", byKind);
        }

        static readonly Dictionary<string, string> Kind = new Dictionary<string, string>
        {
            ["gauntlet"] = "ring", ["punch"] = "ring", ["sunder"] = "crescent", ["flicker"] = "echo", ["thumper"] = "quake", ["tether"] = "crack",
        };

        readonly ImpactWaveParams w;
        readonly double cooldownTime, stagger, ringMin, ringMax;
        readonly EventBus events;
        readonly EnemyRegistry enemies;
        readonly Mover mover;
        readonly Loadout loadout;
        readonly double dt0;
        readonly List<Enemy> within = new List<Enemy>();
        int cooldown; // steps
        public int statWaves, statHits;
        public readonly Dictionary<string, int> byKind = new Dictionary<string, int>();

        public Impact(GameConfig cfg, EventBus events, EnemyRegistry enemies, Mover mover, Loadout loadout)
        {
            w = ImpactWaveParams.From(cfg);
            cooldownTime = cfg.Num("momentum.shock.cooldown");
            stagger = cfg.Num("momentum.shock.stagger");
            ringMin = cfg.Num("momentum.shock.ring.radiusMin");
            ringMax = cfg.Num("momentum.shock.ring.radiusMax");
            this.events = events;
            this.enemies = enemies;
            this.mover = mover;
            this.loadout = loadout;
            dt0 = cfg.Dt;
        }

        /// <summary>One wave hit: a share of the blow, a push away from `from` (or toward it: pull), a stagger.</summary>
        void WaveHit(Enemy enemy, double amount, string weapon, Vec3 from, double push, bool pull = false)
        {
            if (!enemy.alive) return;
            loadout.Damage(enemy, amount, false, weapon, from, false, 1, loadout.Shot, shock: true);
            statHits++;
            if (!enemy.alive) return;
            double dx = pull ? from.x - enemy.position.x : enemy.position.x - from.x;
            double dz = pull ? from.z - enemy.position.z : enemy.position.z - from.z;
            enemy.Knockback(dx, dz, push, 0.25, dt0);
            enemy.Stun(stagger, dt0);
            if (loadout.arsenal.enabled) loadout.arsenal.Status(enemy, "stagger");
        }

        /// <summary>
        /// A melee hit landed: `dealt` damage on `enemy` from `weapon` at momentum M. Sets off the weapon's wave when M is
        /// high enough and the wave is off cooldown. Returns whether a wave went off.
        /// </summary>
        public bool Trigger(string weapon, Enemy enemy, double M, double dealt)
        {
            if (!Kind.TryGetValue(weapon, out string kind)) return false;
            if (kind != "ring") return false; // the crescent / echo / quake / crack come with their weapons (N12)
            ImpactWaveResult wave = MomentumMath.ImpactWave(M, cooldown, w);
            if (!wave.fires) return false;
            cooldown = MovementMath.StepsFor(cooldownTime, dt0);
            statWaves++;
            byKind[kind] = (byKind.TryGetValue(kind, out int n) ? n : 0) + 1;
            double amount = dealt * wave.share;
            double k = JsMath.Max(0, JsMath.Min(1, (M - w.at) / (1 - w.at)));
            Vec3 at = enemy.AimPoint;
            int hits = 0;
            double radius = ringMin + (ringMax - ringMin) * k;
            Vec3 tmp = enemy.position;
            enemies.Within(at, radius, within);
            for (int i = 0; i < within.Count; i++)
            {
                Enemy e = within[i];
                if (e == enemy) continue;
                hits++;
                WaveHit(e, amount, weapon, tmp, wave.knockback);
            }
            events.Emit("impactWave", new ImpactWaveEvent { owner = loadout.owner, kind = kind, weapon = weapon, M = M, position = at, feet = mover.position, dir = default, radius = radius, hits = hits });
            return true;
        }

        /// <summary>One fixed step: the cooldown.</summary>
        public void Step(double dt)
        {
            if (cooldown > 0) cooldown--;
        }

        public void Reset() => cooldown = 0;
    }
}
