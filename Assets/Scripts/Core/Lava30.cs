// NIGHT30 (builder S; design.n30.lava, item 3, D-6): LAVA, not pits. The level JSON's top-level `lava` array (builder L's LevelPlan29.AddLava,
// Design/levels/LEVEL-FORMAT.md): [{ "rect": [x0, z0, x1, z1], "y": surfaceY, "room": "f2" }]. The rule (pure; the sim drives it from
// SurvivalSim.Systems30.cs after the enemies' step):
//   - the player's feet inside a rect at or below y + touchMargin: damageFraction of max health (Health.Damage, source "lava"), an upward
//     knock of knockUp m/s (Mover.Launch, the jump pad's own path: the Mover is never edited), the event lavaBurn {damage, flashSeconds, x, y, z}
//     (the view's hurtFlashSeconds flash), then no damage again for graceSeconds. NO teleport, NO respawn (a lava room has no respawnY).
//   - an enemy whose feet touch lava burns (lavaEnemyBurn {id, kind, x, y, z}) for enemyBurnSeconds and dies (lavaEnemyDeath, same payload).
// Timers in whole steps (MovementMath.StepsFor). No randomness.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Lava30Params
    {
        public double damageFraction, knockUp, hurtFlashSeconds, graceSeconds, enemyBurnSeconds, touchMargin;

        public static Lava30Params From(GameConfig cfg) => new Lava30Params
        {
            damageFraction = cfg.Num("design.n30.lava.damageFraction"),
            knockUp = cfg.Num("design.n30.lava.knockUp"),
            hurtFlashSeconds = cfg.Num("design.n30.lava.hurtFlashSeconds"),
            graceSeconds = cfg.Num("design.n30.lava.graceSeconds"),
            enemyBurnSeconds = cfg.Num("design.n30.lava.enemyBurnSeconds"),
            touchMargin = cfg.Num("design.n30.lava.touchMargin"),
        };
    }

    /// <summary>One lava surface: a rect in world x / z (min, max) at surface height y, in a room.</summary>
    public struct Lava30Zone
    {
        public double x0, z0, x1, z1, y;
        public string room;

        public bool Touches(Vec3 feet, double margin) => feet.x >= x0 && feet.x <= x1 && feet.z >= z0 && feet.z <= z1 && feet.y <= y + margin;
    }

    /// <summary>lavaBurn's payload (the player) and lavaEnemyBurn / lavaEnemyDeath's (an enemy).</summary>
    public sealed class Lava30Event
    {
        public int id = -1;
        public string kind;
        public double damage, flashSeconds, x, y, z;
    }

    public sealed class Lava30
    {
        public readonly Lava30Params p;
        public readonly List<Lava30Zone> zones = new List<Lava30Zone>();
        readonly double dt;
        readonly Dictionary<int, int> burning = new Dictionary<int, int>();
        readonly List<int> done = new List<int>();
        readonly Lava30Event ev = new Lava30Event();

        /// <summary>Steps until the lava can hurt the player again; steps of the hurt flash left (the view reads FlashShare).</summary>
        public int graceSteps, flashSteps;

        /// <summary>Counters (harness, reel): player touches that hurt, enemies that burned, enemies the lava killed.</summary>
        public int touches, enemyBurns, enemyDeaths;

        public Lava30(Lava30Params p, double dt)
        {
            this.p = p;
            this.dt = dt;
        }

        /// <summary>The zones from a level JSON's `lava` array (none: no key, older maps).</summary>
        public void Load(Dictionary<string, object> mapJson)
        {
            zones.Clear();
            if (mapJson == null || !mapJson.TryGetValue("lava", out object arr) || arr == null) return;
            foreach (object o in PortData.Arr(arr))
            {
                var d = PortData.Obj(o);
                var r = PortData.Arr(d["rect"]);
                double ax = PortData.Num(r[0]), az = PortData.Num(r[1]), bx = PortData.Num(r[2]), bz = PortData.Num(r[3]);
                zones.Add(new Lava30Zone
                {
                    x0 = Math.Min(ax, bx), z0 = Math.Min(az, bz), x1 = Math.Max(ax, bx), z1 = Math.Max(az, bz),
                    y = PortData.Num(d["y"]),
                    room = d.TryGetValue("room", out object rm) ? rm as string : null,
                });
            }
        }

        public void Reset()
        {
            graceSteps = flashSteps = 0;
            touches = enemyBurns = enemyDeaths = 0;
            spared33 = 0;
            burning.Clear();
        }

        /// <summary>The zone the feet touch, or -1.</summary>
        public int TouchAt(Vec3 feet)
        {
            for (int i = 0; i < zones.Count; i++) if (zones[i].Touches(feet, p.touchMargin)) return i;
            return -1;
        }

        /// <summary>The hurt flash's share left, 1 at the touch and 0 when it is over (the view's flash).</summary>
        public double FlashShare
        {
            get
            {
                int full = MovementMath.StepsFor(p.hurtFlashSeconds, dt);
                return full <= 0 ? 0 : Math.Min(1.0, flashSteps / (double)full);
            }
        }

        /// <summary>One fixed step. `hurt(amount)` lands the damage (false: it did not: invulnerable, dead); `knock(vy)` launches the player.</summary>
        /// <summary>NIGHT34 B (design.n34.bend34.split): true on a step whose world block is held (SurvivalSim.Systems30 sets it from n34HoldWorld): the
        /// player's touch runs, the enemies' burns wait for the world's step. False with the row off.</summary>
        public bool holdEnemies34;

        public void Step(Vec3 feet, double maxHp, bool playing, Func<double, bool> hurt, Action<double> knock, List<Enemy> enemies, EventBus events)
        {
            if (graceSteps > 0) graceSteps--;
            if (flashSteps > 0) flashSteps--;
            if (zones.Count == 0) return;
            if (playing && graceSteps == 0 && TouchAt(feet) >= 0)
            {
                double amount = maxHp * p.damageFraction;
                hurt(amount); // landed or not (a shield, the god mode), the touch knocks and starts the grace: one rule
                knock(p.knockUp);
                touches++;
                graceSteps = MovementMath.StepsFor(p.graceSeconds, dt);
                flashSteps = MovementMath.StepsFor(p.hurtFlashSeconds, dt);
                ev.id = -1;
                ev.kind = "player";
                ev.damage = amount;
                ev.flashSeconds = p.hurtFlashSeconds;
                ev.x = feet.x;
                ev.y = feet.y;
                ev.z = feet.z;
                events.Emit("lavaBurn", ev);
            }
            if (holdEnemies34) return; // NIGHT34 B: a held world step (the split bend): the enemies' part waits for the world
            // enemies: a touch starts the burn; the burn runs out and kills
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy e = enemies[i];
                if (!e.alive || burning.ContainsKey(e.id)) continue;
                int zt = TouchAt(e.position);
                if (zt < 0) continue;
                if (spare33 != null && spare33(e, zt))
                {
                    spared33++;
                    continue;
                }
                burning[e.id] = Math.Max(1, MovementMath.StepsFor(p.enemyBurnSeconds, dt));
                enemyBurns++;
                Emit(events, "lavaEnemyBurn", e);
            }
            if (burning.Count == 0) return;
            done.Clear();
            foreach (KeyValuePair<int, int> kv in burning) done.Add(kv.Key);
            for (int k = 0; k < done.Count; k++)
            {
                int id = done[k];
                Enemy e = Find(enemies, id);
                if (e == null || !e.alive)
                {
                    burning.Remove(id);
                    continue;
                }
                int left = burning[id] - 1;
                if (left > 0)
                {
                    burning[id] = left;
                    continue;
                }
                burning.Remove(id);
                e.Damage(e.hp + 1, 0.2);
                e.alive = false; // a shielded or overridden body dies too: lava never lets go
                enemyDeaths++;
                Emit(events, "lavaEnemyDeath", e);
            }
        }

        /// <summary>NIGHT33 E (design.n33.ai33): asked before an enemy's touch starts its burn; true spares it (the edge guard put it back on the bank).
        /// Null (the row off) = every touch burns, NIGHT30's rule.</summary>
        public Func<Enemy, int, bool> spare33;

        /// <summary>The harness's counts (ai33): burns the edge guard spared.</summary>
        public int spared33;

        /// <summary>NIGHT33 E (design.n33.ai33): the nav grid's lava mask: a node is lava when its column's centre lies in a zone's rect and the node stands at
        /// most `above` m over the surface (a walkway higher over the lava stays open). Null when no node is lava.</summary>
        public byte[] NavMask(NavGrid g, double above)
        {
            if (g == null || zones.Count == 0) return null;
            byte[] m = null;
            for (int n = 0; n < g.count; n++)
            {
                double x = g.NodeX(n), z = g.NodeZ(n), y = g.y[n];
                for (int i = 0; i < zones.Count; i++)
                {
                    Lava30Zone zn = zones[i];
                    if (x < zn.x0 || x > zn.x1 || z < zn.z0 || z > zn.z1 || y > zn.y + above) continue;
                    m ??= new byte[g.count];
                    m[n] = 1;
                    break;
                }
            }
            return m;
        }

        /// <summary>Is enemy `id` burning now (the view's burn effect)?</summary>
        public bool Burning(int id) => burning.ContainsKey(id);

        static Enemy Find(List<Enemy> list, int id)
        {
            for (int i = 0; i < list.Count; i++) if (list[i].id == id) return list[i];
            return null;
        }

        void Emit(EventBus events, string name, Enemy e)
        {
            ev.id = e.id;
            ev.kind = e.kind;
            ev.damage = 0;
            ev.flashSeconds = 0;
            ev.x = e.position.x;
            ev.y = e.position.y;
            ev.z = e.position.z;
            events.Emit(name, ev);
        }
    }
}
