// NIGHT42 builder K1 (arena42; G12, GG 2026-10-14: "way better arena deisgn for the gameplay. more elements that are not just decoration"; the
// reference sheet Research/2026-10-14-wave42/ARENA-KIT.md): arena elements that change play, read from a level JSON (L1 places them in n42-foundry):
//   1. THE GRINDER (`grinders42`: [{ "rect": [x0, z0, x1, z1], "y": floorY, "room": "a5" }]): a pit floor of blades. ULTRAKILL pays for using the
//      arena as a weapon (Style: an enemy into a pit, "Fall" 80; "Minced" / "Shredded" 80). Any enemy whose feet touch a grinder (inside the rect, at
//      or below y + touchMargin) is ground at once: the kill is the player's (Loadout.Damage, weapon "trap", the traps' own path: points and the kill
//      events), event `arena42Ground` (Arena42GroundEvent) and, when it went in knocked or in the air (the player's push, a launch, a fall), the
//      style bonus GROUND (`styleBonus` {name GROUND, points groundStyle, weapon "trap"}: StyleMeter pays it; U1 draws it). Walkers never walk in:
//      the nav mask (NavMask, OR-ed into NavGrid.hazard by the sim) and an edge guard (the sim's spare: a walker the player did not knock goes back
//      to its feet at the step's start). The player who falls in is hurt (playerDamageFraction of max health, source "grinder42"), knocked up
//      (knockUp m/s, the jump pad's path Mover.Launch) and spared for graceSeconds (Lava30's rule, its own numbers).
//   2. BREAKABLE COVER (`cover42`: [{ "min": [x, y, z], "max": [x, y, z], "hits": 6, "room": "a5" }]): a solid box in the world (the sim's BoxLayer
//      "arena42Cover": it stops the player, the enemies and every ray). Each player shot that ends on it (`shotResult`, surface world) takes one hit
//      (event `arena42CoverHit`); at 0 it SHATTERS (event `arena42CoverBreak`): the box leaves the world and its shards hit every enemy within
//      burstRadius of its centre for burstDamage (Loadout.Damage, weapon "trap") and push them away from it; a shatter that hits a body pays the
//      style bonus SHATTER (shatterStyle). Cover to hide behind, that you can spend as a weapon, and that is gone when you stood still too long.
// Numbers: design.n42.arena42 (never here). No randomness. Timers in whole steps (MovementMath.StepsFor). No allocation per step (the lists are
// filled at load; the events are reused).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Arena42Params
    {
        public double touchMargin, playerDamageFraction, knockUp, graceSeconds, groundStyle, maskAbove, guardMargin, guardAbove;
        public int coverHits;
        public double burstRadius, burstDamage, burstPush, burstPushSeconds, shatterStyle;

        public static Arena42Params From(GameConfig cfg)
        {
            const string g = "design.n42.arena42.grinder.", c = "design.n42.arena42.cover.";
            return new Arena42Params
            {
                touchMargin = cfg.Num(g + "touchMargin"),
                playerDamageFraction = cfg.Num(g + "playerDamageFraction"),
                knockUp = cfg.Num(g + "knockUp"),
                graceSeconds = cfg.Num(g + "graceSeconds"),
                groundStyle = cfg.Num(g + "groundStyle"),
                maskAbove = cfg.Num(g + "maskAbove"),
                guardMargin = cfg.Num(g + "guardMargin"),
                guardAbove = cfg.Num(g + "guardAbove"),
                coverHits = cfg.Int(c + "hits"),
                burstRadius = cfg.Num(c + "burstRadius"),
                burstDamage = cfg.Num(c + "burstDamage"),
                burstPush = cfg.Num(c + "burstPush"),
                burstPushSeconds = cfg.Num(c + "burstPushSeconds"),
                shatterStyle = cfg.Num(c + "shatterStyle"),
            };
        }
    }

    /// <summary>One grinder: a rect in world x / z (min, max) at floor height y, in a room.</summary>
    public struct Arena42Grinder
    {
        public double x0, z0, x1, z1, y;
        public string room;

        public bool InRect(double x, double z) => x >= x0 && x <= x1 && z >= z0 && z <= z1;

        /// <summary>Within m of the rect (the edge guard's band round the rim).</summary>
        public bool NearRect(double x, double z, double m) => x >= x0 - m && x <= x1 + m && z >= z0 - m && z <= z1 + m;

        public bool Touches(Vec3 feet, double margin) => InRect(feet.x, feet.z) && feet.y <= y + margin;
    }

    /// <summary>One breakable cover box: its bounds, the hits it has left, broken or standing, its room.</summary>
    public struct Arena42Cover
    {
        public Vec3 min, max;
        public int hits, maxHits;
        public bool broken;
        public string room;

        public Vec3 Centre => new Vec3((min.x + max.x) * 0.5, (min.y + max.y) * 0.5, (min.z + max.z) * 0.5);

        /// <summary>A point on (or in) the box, within eps m of its faces.</summary>
        public bool Holds(Vec3 p, double eps) =>
            p.x >= min.x - eps && p.x <= max.x + eps && p.y >= min.y - eps && p.y <= max.y + eps && p.z >= min.z - eps && p.z <= max.z + eps;
    }

    /// <summary>arena42Ground's payload (reused): the body, where, whether it went in knocked / airborne, the style paid (0: none).</summary>
    public sealed class Arena42GroundEvent
    {
        public int id, grinder;
        public string kind;
        public double x, y, z, style;
        public bool knocked;
    }

    /// <summary>arena42CoverHit / arena42CoverBreak's payload (reused): the cover, its centre, hits left of how many, the shatter's bodies hit / killed.</summary>
    public sealed class Arena42CoverEvent
    {
        public int index, hits, maxHits, bodies, kills;
        public double x, y, z;
        public bool broken;
    }

    public sealed class Arena42
    {
        public const string GroundEvent = "arena42Ground", CoverHitEvent = "arena42CoverHit", CoverBreakEvent = "arena42CoverBreak";
        public const string GroundStyleName = "GROUND", ShatterStyleName = "SHATTER", Weapon = "trap", PlayerSource = "grinder42";

        public readonly Arena42Params p;
        public readonly List<Arena42Grinder> grinders = new List<Arena42Grinder>();
        public readonly List<Arena42Cover> cover = new List<Arena42Cover>();
        readonly double dt;
        readonly Arena42GroundEvent groundEv = new Arena42GroundEvent();
        readonly Arena42CoverEvent coverEv = new Arena42CoverEvent();
        readonly StyleBonusEvent bonusEv = new StyleBonusEvent { owner = "player", weapon = Weapon };

        /// <summary>Steps until a grinder can hurt the player again.</summary>
        public int graceSteps;

        /// <summary>Counters (harness, reel): bodies ground, of them knocked / airborne (GROUND paid), walkers the edge guard put back, player touches,
        /// cover hits, cover broken, shatter bodies hit / killed.</summary>
        public int ground, groundKnocked, spared, touches, coverHits, coverBroken, shatterHits, shatterKills;

        /// <summary>The cover layer changed (a box broke): the sim rebuilds its BoxLayer and clears it.</summary>
        public bool coverDirty;

        public Arena42(Arena42Params p, double dt)
        {
            this.p = p;
            this.dt = dt;
        }

        public bool Any => grinders.Count > 0 || cover.Count > 0;

        /// <summary>The elements from a level JSON (`grinders42`, `cover42`; none: no key, every older map).</summary>
        public void Load(Dictionary<string, object> mapJson)
        {
            grinders.Clear();
            cover.Clear();
            if (mapJson == null) return;
            if (mapJson.TryGetValue("grinders42", out object ga) && ga != null)
                foreach (object o in PortData.Arr(ga))
                {
                    var d = PortData.Obj(o);
                    var r = PortData.Arr(d["rect"]);
                    double ax = PortData.Num(r[0]), az = PortData.Num(r[1]), bx = PortData.Num(r[2]), bz = PortData.Num(r[3]);
                    grinders.Add(new Arena42Grinder
                    {
                        x0 = Math.Min(ax, bx), z0 = Math.Min(az, bz), x1 = Math.Max(ax, bx), z1 = Math.Max(az, bz),
                        y = PortData.Num(d["y"]),
                        room = d.TryGetValue("room", out object rm) ? rm as string : null,
                    });
                }
            if (mapJson.TryGetValue("cover42", out object ca) && ca != null)
                foreach (object o in PortData.Arr(ca))
                {
                    var d = PortData.Obj(o);
                    var a = PortData.Arr(d["min"]);
                    var b = PortData.Arr(d["max"]);
                    double ax = PortData.Num(a[0]), ay = PortData.Num(a[1]), az = PortData.Num(a[2]);
                    double bx = PortData.Num(b[0]), by = PortData.Num(b[1]), bz = PortData.Num(b[2]);
                    int hits = d.TryGetValue("hits", out object h) && h != null ? (int)PortData.Num(h) : p.coverHits;
                    cover.Add(new Arena42Cover
                    {
                        min = new Vec3(Math.Min(ax, bx), Math.Min(ay, by), Math.Min(az, bz)),
                        max = new Vec3(Math.Max(ax, bx), Math.Max(ay, by), Math.Max(az, bz)),
                        hits = Math.Max(1, hits), maxHits = Math.Max(1, hits),
                        room = d.TryGetValue("room", out object rm) ? rm as string : null,
                    });
                }
        }

        /// <summary>A run's start: every cover box standing again, the counters at 0.</summary>
        public void Reset()
        {
            graceSteps = 0;
            ground = groundKnocked = spared = touches = coverHits = coverBroken = shatterHits = shatterKills = 0;
            for (int i = 0; i < cover.Count; i++)
            {
                Arena42Cover c = cover[i];
                c.hits = c.maxHits;
                c.broken = false;
                cover[i] = c;
            }
            coverDirty = true;
        }

        /// <summary>The grinder the feet touch, or -1.</summary>
        public int GrinderAt(Vec3 feet)
        {
            for (int i = 0; i < grinders.Count; i++) if (grinders[i].Touches(feet, p.touchMargin)) return i;
            return -1;
        }

        /// <summary>The edge guard's band: feet within guardMargin m of a grinder's rect and at most guardAbove m over its floor (a walkway higher up is
        /// solid ground). The guard puts back a walker that entered the band this step.</summary>
        public bool InGuard(Vec3 feet)
        {
            for (int i = 0; i < grinders.Count; i++)
            {
                Arena42Grinder g = grinders[i];
                if (g.NearRect(feet.x, feet.z, p.guardMargin) && feet.y <= g.y + p.guardAbove) return true;
            }
            return false;
        }

        /// <summary>The nav grid's grinder mask: a node whose column's centre lies in a grinder's rect, at most maskAbove m over its floor (a walkway
        /// higher up stays open). Null when no node is masked.</summary>
        public byte[] NavMask(NavGrid g)
        {
            if (g == null || grinders.Count == 0) return null;
            byte[] m = null;
            for (int n = 0; n < g.count; n++)
            {
                double x = g.NodeX(n), z = g.NodeZ(n), y = g.y[n];
                for (int i = 0; i < grinders.Count; i++)
                {
                    Arena42Grinder gr = grinders[i];
                    // the floor (at most maskAbove over it), and the guard's band round the rim (up to guardAbove: a pit's rim) so no path runs where the guard acts
                    bool inRect = gr.InRect(x, z);
                    if (!(inRect && y <= gr.y + p.maskAbove) && !(!inRect && gr.NearRect(x, z, p.guardMargin) && y <= gr.y + p.guardAbove)) continue;
                    m ??= new byte[g.count];
                    m[n] = 1;
                    break;
                }
            }
            return m;
        }

        /// <summary>The grinders' step. `hurt(amount)` lands the player's damage; `knock(vy)` launches the player; `spare(e)` is the edge guard (true: the
        /// walker was put back, no grind); `kill(e, amount, from)` is the player's kill path (Loadout.Damage, weapon "trap"); `knocked(e)` says the
        /// body went in knocked or in the air (GROUND pays).</summary>
        public void StepGrinders(Vec3 feet, double maxHp, bool playing, Func<double, bool> hurt, Action<double> knock, List<Enemy> enemies,
            Func<Enemy, bool> spare, Func<Enemy, double, Vec3, bool> kill, Func<Enemy, bool> knocked, EventBus events)
        {
            if (graceSteps > 0) graceSteps--;
            if (grinders.Count == 0) return;
            if (playing && graceSteps == 0 && GrinderAt(feet) >= 0)
            {
                hurt(maxHp * p.playerDamageFraction);
                knock(p.knockUp);
                touches++;
                graceSteps = MovementMath.StepsFor(p.graceSeconds, dt);
            }
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy e = enemies[i];
                if (!e.alive) continue;
                int gi = GrinderAt(e.position);
                if (gi < 0) continue;
                bool wasKnocked = knocked(e);
                if (!wasKnocked && spare != null && spare(e))
                {
                    spared++;
                    continue;
                }
                Vec3 at = e.position;
                kill(e, e.hp + 1, at);
                if (e.alive) // a shield, a plate, a block: the grinder never lets go
                {
                    e.Damage(e.hp + 1, 0.2);
                    e.alive = false;
                }
                ground++;
                groundEv.id = e.id;
                groundEv.kind = e.kind;
                groundEv.grinder = gi;
                groundEv.x = at.x;
                groundEv.y = at.y;
                groundEv.z = at.z;
                groundEv.knocked = wasKnocked;
                groundEv.style = wasKnocked ? p.groundStyle : 0;
                events.Emit(GroundEvent, groundEv);
                if (wasKnocked)
                {
                    groundKnocked++;
                    bonusEv.name = GroundStyleName;
                    bonusEv.points = p.groundStyle;
                    events.Emit("styleBonus", bonusEv);
                }
            }
        }

        /// <summary>The standing cover box a point lies on (a shot's end), or -1.</summary>
        public int CoverAt(Vec3 point)
        {
            for (int i = 0; i < cover.Count; i++) if (!cover[i].broken && cover[i].Holds(point, 0.05)) return i;
            return -1;
        }

        /// <summary>One hit on cover i (a player's shot ended on it): one hit off; at 0 it shatters (ShatterAt). Returns true when it broke.</summary>
        public bool Hit(int i, Vec3 playerAt, List<Enemy> enemies, Func<Enemy, double, Vec3, bool> kill, double dtStep, EventBus events)
        {
            if (i < 0 || i >= cover.Count || cover[i].broken) return false;
            Arena42Cover c = cover[i];
            c.hits--;
            coverHits++;
            bool broke = c.hits <= 0;
            if (broke)
            {
                c.hits = 0;
                c.broken = true;
                coverBroken++;
                coverDirty = true;
            }
            cover[i] = c;
            Vec3 at = c.Centre;
            int bodies = 0, kills = 0;
            if (broke)
            {
                for (int k = 0; k < enemies.Count; k++)
                {
                    Enemy e = enemies[k];
                    if (!e.alive) continue;
                    double dx = e.position.x - at.x, dz = e.position.z - at.z, dy = e.position.y + e.AimHeight - at.y;
                    if (dx * dx + dy * dy + dz * dz > p.burstRadius * p.burstRadius) continue;
                    bodies++;
                    if (kill(e, p.burstDamage, at)) kills++;
                    if (e.alive)
                    {
                        // away from the cover; a body at its very centre goes away from the player
                        if (dx * dx + dz * dz < 1e-6)
                        {
                            dx = e.position.x - playerAt.x;
                            dz = e.position.z - playerAt.z;
                        }
                        e.Knockback(dx, dz, p.burstPush, p.burstPushSeconds, dtStep);
                    }
                }
                shatterHits += bodies;
                shatterKills += kills;
            }
            coverEv.index = i;
            coverEv.hits = c.hits;
            coverEv.maxHits = c.maxHits;
            coverEv.broken = broke;
            coverEv.bodies = bodies;
            coverEv.kills = kills;
            coverEv.x = at.x;
            coverEv.y = at.y;
            coverEv.z = at.z;
            events.Emit(broke ? CoverBreakEvent : CoverHitEvent, coverEv);
            if (broke && bodies > 0 && p.shatterStyle > 0)
            {
                bonusEv.name = ShatterStyleName;
                bonusEv.points = p.shatterStyle;
                events.Emit("styleBonus", bonusEv);
            }
            return broke;
        }

        /// <summary>Fill the sim's cover layer with every standing box (clears coverDirty).</summary>
        public void FillLayer(BoxLayer layer, List<(Vec3 min, Vec3 max)> scratch)
        {
            scratch.Clear();
            for (int i = 0; i < cover.Count; i++) if (!cover[i].broken) scratch.Add((cover[i].min, cover[i].max));
            layer.SetBoxes(scratch);
            coverDirty = false;
        }
    }
}
