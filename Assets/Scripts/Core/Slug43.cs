// NIGHT43 builder W1 slug43 (N43Blocks.slug43, design.n43.slug43; GG 2026-10-16 G20: "Charged attacks, especially charged shotgun attacks,
// should produce more dramatic particles, explosions, enemy reactions"; combat-web.md B13: "the release frame needs the biggest single spike
// ... enemies launched"). The sim's half of the charged THUNDERCLAP slug (Guns/ChargeShots27.cs FireSlug31): one line hook before the
// slug's jump, Slug43Impact, with the row on:
//   - every body the slug struck (its first and any BORE SLUG pierce) that lives is thrown along the shot (knockSpeed m/s for knockTime s,
//     replacing the charged knock's 14 m/s for 0.2 s; any kind, not only husks: a boss body's Knockback / Stun are empty) and stunned staggerS;
//   - a BLAST at the slug's end (the last body struck, or the wall): every other live body whose hit sphere is within blastRadius of it
//     (and in line of sight of it) takes blastDamage x a linear falloff (1 at the centre, blastEdge at the radius), a knock outward
//     (blastKnock m/s, knockTime) and blastStaggerS; Combat.Damage with blast = true (no momentum), weapon "shotgun", the slug's shot id;
//   - ONE event `slug43` (Slug43.Event) a charged slug that struck a body or a wall, with the reused Slug43Event (F1's slug view: the
//     impact point, the direction, what it struck, the blast's hits and kills).
// Off (slug43 null or not on): FireSlug31 as NIGHT42 has it, bit for bit (the hook is never entered).
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>slug43's numbers and run counters (the sim makes it once, SurvivalSim.N43.Weapons.cs; Set at each run's start).</summary>
    public sealed class Slug43
    {
        /// <summary>The event a charged slug's impact emits (payload Slug43Event, reused). GG's sound hook too (CLAUDE.md rule 7).</summary>
        public const string Event = "slug43";

        public bool on;
        public double knockSpeed, knockTime, staggerS, blastRadius, blastDamage, blastEdge, blastKnock, blastStaggerS;

        /// <summary>This run's counters (the harness reads them): impacts, bodies thrown, blast hits, blast kills.</summary>
        public int impacts, thrown, blastHits, blastKills;

        public readonly Slug43Event payload = new Slug43Event();
        internal readonly List<Enemy> near = new List<Enemy>();

        public void Set(bool on, GameConfig cfg)
        {
            this.on = on;
            impacts = thrown = blastHits = blastKills = 0;
            if (!on) return;
            const string r = "design.n43.slug43.";
            knockSpeed = cfg.Num(r + "knockSpeed");
            knockTime = cfg.Num(r + "knockTime");
            staggerS = cfg.Num(r + "staggerS");
            blastRadius = cfg.Num(r + "blastRadius");
            blastDamage = cfg.Num(r + "blastDamage");
            blastEdge = cfg.Num(r + "blastEdge");
            blastKnock = cfg.Num(r + "blastKnock");
            blastStaggerS = cfg.Num(r + "blastStaggerS");
        }
    }

    /// <summary>A charged slug's impact: where (the slug's end), the shot's direction, surface "enemy" | "world", the first body struck (-1:
    /// none) and its kind, killed (any struck body died), the blast's hits and kills, the blast's radius.</summary>
    public sealed class Slug43Event
    {
        public string owner = "player";
        public Vec3 position, dir;
        public string surface, kind;
        public int enemy, struck, blastHits, blastKills;
        public bool killed;
        public double radius;
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT43 W1 slug43 (null or not on: off; the sim sets it, SurvivalSim.N43.Weapons.cs).</summary>
        public Slug43 slug43;

        /// <summary>slug43, at the end of FireSlug31 (the struck bodies are `order`): the throw, the stun, the blast, the event. `end` is the
        /// slug's end point; `world` true when a wall ended it with no body struck.</summary>
        void Slug43Impact(Vec3 end, Vec3 aim, bool wall)
        {
            if (slug43 == null || !slug43.on) return;
            Slug43 s = slug43;
            if (order.Count == 0 && !wall) return; // into the air: no impact
            double dt = dt26;
            bool killed = false;
            for (int i = 0; i < order.Count; i++)
            {
                Enemy e = order[i];
                if (!e.alive) { killed = true; continue; }
                e.Knockback(aim.x, aim.z, s.knockSpeed, s.knockTime, dt);
                e.Stun(s.staggerS, dt);
                s.thrown++;
            }
            // the blast: every other live body near the end (collected first: a kill mid-walk cannot shift it)
            List<Enemy> near = s.near;
            near.Clear();
            foreach (Enemy e in enemies.all)
            {
                if (!e.alive || order.Contains(e)) continue;
                if (Slug43Gap(e, end) <= s.blastRadius) near.Add(e);
            }
            int hits = 0, kills = 0;
            double mult27 = sgChargeMult27;
            sgChargeMult27 = 1; // the blast is not the slug: no charged x damageMult and no charged forward knock (FireCharged27's try sets them)
            for (int i = 0; i < near.Count; i++)
            {
                Enemy e = near[i];
                if (!e.alive || !HasLos(end, e.AimPoint)) continue;
                double gap = Slug43Gap(e, end);
                double k = 1 - (1 - s.blastEdge) * System.Math.Max(0, System.Math.Min(1, gap / s.blastRadius));
                bool died = Damage(e, s.blastDamage * k, false, "shotgun", end, false, 1, -1, false, true);
                hits++;
                if (died) kills++;
                else if (e.alive)
                {
                    e.Knockback(e.position.x - end.x, e.position.z - end.z, s.blastKnock, s.knockTime, dt);
                    e.Stun(s.blastStaggerS, dt);
                }
            }
            sgChargeMult27 = mult27;
            s.impacts++;
            s.blastHits += hits;
            s.blastKills += kills;
            Slug43Event ev = s.payload;
            ev.owner = owner;
            ev.position = end;
            ev.dir = aim;
            ev.surface = order.Count > 0 ? "enemy" : "world";
            ev.enemy = order.Count > 0 ? order[0].id : -1;
            ev.kind = order.Count > 0 ? order[0].kind : null;
            ev.struck = order.Count;
            ev.killed = killed;
            ev.blastHits = hits;
            ev.blastKills = kills;
            ev.radius = s.blastRadius;
            events.Emit(Slug43.Event, ev);
        }

        /// <summary>slug43: the gap from a point to a body's hit sphere (the nearest point of its vertical axis, minus its radius; a DRONE: its centre).</summary>
        static double Slug43Gap(Enemy e, Vec3 p)
        {
            if (e is Drone) return JsMath.Hypot(p.x - e.position.x, p.y - e.position.y, p.z - e.position.z) - e.Radius;
            double y = System.Math.Max(e.position.y + e.Radius, System.Math.Min(e.position.y + e.Height - e.Radius, p.y));
            return JsMath.Hypot(p.x - e.position.x, p.y - y, p.z - e.position.z) - e.Radius;
        }
    }
}
