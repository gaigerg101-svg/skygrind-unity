// NIGHT21 U3-35.4 (builder M): SLIDE BITES (design.n21.slideBites). The slide hits what it passes through: while the mover is in
// its slide, every live enemy the slide's path touches takes a bite (damage; a survivor staggers), at most once per cooldown each.
// Unity-only: the web's slide never hit anything (Turbo Overkill's slide is the evidence); every number is in design.n21.slideBites.
// - The contact test is the slide's path, not a physics query: the segment the player's feet travelled this step (the mover's
//   position last step to now) against each enemy's vertical capsule, flat (x, z) distance <= radius + the enemy's radius, with
//   the enemy's height overlapping the slide's collider (slide.colliderHeight above the feet). No Rigidbody, no PhysX.
// - A bite is the loadout's own damage path with the weapon "slide" (a secondary source, like "slam" and "trap": no trigger pull,
//   no momentum): the 'hit' and 'kill' events, points, style. A slide kill counts as a close kill: the hit is measured from the
//   player's side of the enemy (flat), so style's CLOSE CALL (distance <= style.closeCallDistance) pays.
// - Events: slideBite {id, kind, damage, killed, x, y, z}, and styleBonus "SLIDE BITE" (the style meter's named bonus).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.n21.slideBites' numbers.</summary>
    public sealed class SlideBite21Rules
    {
        public double damage, radius, cooldown, stagger, points;

        public static SlideBite21Rules From(GameConfig cfg) => new SlideBite21Rules
        {
            damage = cfg.Num("design.n21.slideBites.damage"),
            radius = cfg.Num("design.n21.slideBites.radius"),
            cooldown = cfg.Num("design.n21.slideBites.cooldown"),
            stagger = cfg.Num("design.n21.slideBites.stagger"),
            points = cfg.Num("design.n21.slideBites.points"),
        };
    }

    /// <summary>slideBite's payload.</summary>
    public sealed class SlideBite21Event
    {
        public int id;
        public string kind;
        public double damage;
        public bool killed;
        public double x, y, z;
    }

    public sealed class SlideBite21
    {
        public readonly SlideBite21Rules rules;
        readonly EventBus events;
        readonly Mover mover;
        readonly EnemyRegistry enemies;
        readonly Loadout loadout;
        readonly double dt;
        readonly double colliderHeight;
        readonly int cooldownSteps;
        readonly SlideBite21Event biteEvent = new SlideBite21Event();
        readonly StyleBonusEvent styleEvent = new StyleBonusEvent { owner = "player", name = "SLIDE BITE", weapon = "slide" };
        readonly List<int> bittenUntil = new List<int>(); // by enemy id: the step it can be bitten again
        Vec3 last;
        bool hasLast;
        int stepNo;

        /// <summary>The run's counters: bites landed, kills by a bite.</summary>
        public int bites, kills;

        public SlideBite21(GameConfig cfg, EventBus events, Mover mover, EnemyRegistry enemies, Loadout loadout)
        {
            rules = SlideBite21Rules.From(cfg);
            this.events = events;
            this.mover = mover;
            this.enemies = enemies;
            this.loadout = loadout;
            dt = 1 / cfg.movement.loop.simHz;
            colliderHeight = cfg.movement.slide.colliderHeight;
            cooldownSteps = MovementMath.StepsFor(rules.cooldown, dt);
            styleEvent.points = rules.points;
        }

        /// <summary>A run's start (and a respawn): no memory of the last step, no cooldowns.</summary>
        public void Reset()
        {
            hasLast = false;
            stepNo = 0;
            bittenUntil.Clear();
            bites = kills = 0;
        }

        /// <summary>One fixed step, after the mover: if the player is sliding, bite what the path since last step touched.</summary>
        public void Step()
        {
            stepNo++;
            Vec3 now = mover.position;
            Vec3 from = hasLast ? last : now;
            last = now;
            hasLast = true;
            if (mover.state != MoveState.Slide) return;
            for (int i = 0; i < enemies.all.Count; i++) // by index: a kill may add to the list
            {
                Enemy e = enemies.all[i];
                if (!e.alive || !Touches(e, from, now)) continue;
                int id = e.id;
                while (bittenUntil.Count <= id) bittenUntil.Add(0);
                if (stepNo < bittenUntil[id]) continue;
                bittenUntil[id] = stepNo + cooldownSteps;
                Bite(e, now);
            }
        }

        /// <summary>The slide's path (feet from `a` to `b`, flat) reaches the enemy's capsule, and their heights overlap.</summary>
        bool Touches(Enemy e, Vec3 a, Vec3 b)
        {
            double lo = Math.Min(a.y, b.y), hi = Math.Max(a.y, b.y) + colliderHeight;
            if (hi < e.position.y || lo > e.position.y + e.Height) return false;
            double abx = b.x - a.x, abz = b.z - a.z;
            double len2 = abx * abx + abz * abz;
            double t = len2 > 0 ? Math.Max(0, Math.Min(1, ((e.position.x - a.x) * abx + (e.position.z - a.z) * abz) / len2)) : 0;
            double cx = a.x + abx * t, cz = a.z + abz * t;
            double reach = rules.radius + e.Radius;
            double dx = e.position.x - cx, dz = e.position.z - cz;
            return dx * dx + dz * dz <= reach * reach;
        }

        void Bite(Enemy e, Vec3 player)
        {
            Vec3 aim = e.AimPoint;
            // Measured flat from the player's side (the same height as the enemy's aim point): a bite is a close kill.
            bool killed = loadout.Damage(e, rules.damage, false, "slide", new Vec3(player.x, aim.y, player.z), false, 1, 0);
            bites++;
            if (killed) kills++;
            else e.Stun(rules.stagger, dt);
            biteEvent.id = e.id;
            biteEvent.kind = e.kind;
            biteEvent.damage = rules.damage;
            biteEvent.killed = killed;
            biteEvent.x = aim.x;
            biteEvent.y = aim.y;
            biteEvent.z = aim.z;
            events.Emit("slideBite", biteEvent);
            events.Emit("styleBonus", styleEvent);
        }
    }
}
