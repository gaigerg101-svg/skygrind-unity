// NIGHT20 U3-34.9 A1 (builder EN): src/enemies/bossMath.js ported as it is (OVERNIGHT2 N5): "Pure boss maths: the schedule
// (which boss on which round, the rotation and its mutators), HP + damage scaling, BULWARK's plate damage, ORBITER's pod
// sectors, HIVE's blast rule." Every number is config.bossKit's / config.bulwark's (BossKitParams, BulwarkParams).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>A rotation mutator (config.bossKit.mutators): absent multipliers are 1 (the web's `mutator?.hp ?? 1`).</summary>
    public sealed class BossMutator
    {
        public string id, name;
        public double hp = 1, damage = 1, speed = 1, adds = 1;
    }

    /// <summary>bossForRound's {boss, cycle, mutator}.</summary>
    public struct BossDue
    {
        public string boss;
        public int cycle;
        public BossMutator mutator;
    }

    /// <summary>bossScale's {hp, damage, speed, adds}.</summary>
    public struct BossScale
    {
        public double hp, damage, speed, adds;

        public static BossScale One => new BossScale { hp = 1, damage = 1, speed = 1, adds = 1 };
    }

    /// <summary>A boss's reward (config.bossKit.rewards[boss]): points, a free perk token, a guaranteed power-up drop.</summary>
    public sealed class BossReward
    {
        public double points;
        public bool token, powerUp;
    }

    /// <summary>config.bossKit, read once.</summary>
    public sealed class BossKitParams
    {
        public readonly List<(int round, string boss)> schedule = new List<(int, string)>();
        public readonly List<BossMutator> mutators = new List<BossMutator>();
        public readonly Dictionary<string, string> names = new Dictionary<string, string>(), subtitles = new Dictionary<string, string>(), arenas = new Dictionary<string, string>();
        public readonly Dictionary<string, BossReward> rewards = new Dictionary<string, BossReward>();
        public double rotationFrom, hpPerRound, hpPerCycle, damagePerCycle, coop, introTime, beatTime, beatPush, beatPushUp, killTime, killSlowMo;
        public Vec3 arena;

        public static BossKitParams From(GameConfig c)
        {
            var k = new BossKitParams
            {
                rotationFrom = c.Num("bossKit.rotationFrom"), hpPerRound = c.Num("bossKit.hpPerRound"), hpPerCycle = c.Num("bossKit.hpPerCycle"),
                damagePerCycle = c.Num("bossKit.damagePerCycle"), coop = c.Num("bossKit.coop"), introTime = c.Num("bossKit.introTime"),
                beatTime = c.Num("bossKit.beatTime"), beatPush = c.Num("bossKit.beatPush"), beatPushUp = c.Num("bossKit.beatPushUp"),
                killTime = c.Num("bossKit.killTime"), killSlowMo = c.Num("bossKit.killSlowMo"),
                arena = new Vec3(c.Num("bossKit.arena.x"), c.Num("bossKit.arena.y"), c.Num("bossKit.arena.z")),
            };
            foreach (object o in c.Arr("bossKit.schedule"))
            {
                var s = PortData.Obj(o);
                k.schedule.Add(((int)PortData.Num(s["round"]), (string)s["boss"]));
            }
            foreach (object o in c.Arr("bossKit.mutators"))
            {
                var m = PortData.Obj(o);
                k.mutators.Add(new BossMutator
                {
                    id = (string)m["id"], name = (string)m["name"],
                    hp = m.TryGetValue("hp", out object hp) ? PortData.Num(hp) : 1, damage = m.TryGetValue("damage", out object dm) ? PortData.Num(dm) : 1,
                    speed = m.TryGetValue("speed", out object sp) ? PortData.Num(sp) : 1, adds = m.TryGetValue("adds", out object ad) ? PortData.Num(ad) : 1,
                });
            }
            foreach (var kv in c.Obj("bossKit.names")) k.names[kv.Key] = (string)kv.Value;
            foreach (var kv in c.Obj("bossKit.subtitles")) k.subtitles[kv.Key] = (string)kv.Value;
            foreach (var kv in c.Obj("bossKit.arenas")) k.arenas[kv.Key] = (string)kv.Value;
            foreach (var kv in c.Obj("bossKit.rewards"))
            {
                var r = PortData.Obj(kv.Value);
                k.rewards[kv.Key] = new BossReward { points = PortData.Num(r["points"]), token = r["token"] is bool t && t, powerUp = r["powerUp"] is bool p && p };
            }
            return k;
        }
    }

    public static class BossMath
    {
        /// <summary>
        /// The boss of a round, or null. Rounds 5-25 follow the schedule; from `rotationFrom` the five come around again in
        /// order, one per boss round, with a mutator per cycle (cycle 1 = the first rotation).
        /// </summary>
        public static BossDue? BossForRound(int round, BossKitParams k, DirectorMath.D r)
        {
            if (!DirectorMath.IsBossRound(round, r)) return null;
            foreach (var s in k.schedule)
                if (s.round == round) return new BossDue { boss = s.boss, cycle = 0, mutator = null };
            if (round < k.rotationFrom) return null;
            int n = k.schedule.Count;
            int index = (int)Math.Floor((round - k.rotationFrom) / r.bossEvery);
            int cycle = index / n + 1;
            BossMutator mutator = k.mutators.Count > 0 ? k.mutators[(cycle - 1) % k.mutators.Count] : null;
            return new BossDue { boss = k.schedule[index % n].boss, cycle = cycle, mutator = mutator };
        }

        /// <summary>The first round a boss appears on (its schedule slot).</summary>
        public static int FirstRound(string boss, BossKitParams k)
        {
            foreach (var s in k.schedule)
                if (s.boss == boss) return s.round;
            return 0;
        }

        /// <summary>
        /// HP and damage multipliers for a boss on a round: + hpPerRound per round past its first appearance, + hpPerCycle and
        /// damagePerCycle per rotation cycle, x the mutator, x the coop factor (1: single player).
        /// </summary>
        public static BossScale Scale(string boss, int round, int cycle, BossMutator mutator, BossKitParams k)
        {
            double past = Math.Max(0, round - FirstRound(boss, k));
            double hp = (1 + k.hpPerRound * past) * (1 + k.hpPerCycle * cycle) * (mutator?.hp ?? 1) * k.coop;
            double damage = (1 + k.damagePerCycle * cycle) * (mutator?.damage ?? 1);
            return new BossScale { hp = hp, damage = damage, speed = mutator?.speed ?? 1, adds = mutator?.adds ?? 1 };
        }

        /// <summary>
        /// BULWARK: how much a hit on a plate deals (to the plate and to the boss). 10% until broken; armor-piercing weapons
        /// and any hit with momentum above apMomentum x3.
        /// </summary>
        public static double PlateDamage(double amount, string weapon, double momentum, BulwarkParams b)
        {
            bool ap = IsArmorPiercing(weapon, momentum, b);
            return amount * b.plateMult * (ap ? b.apMult : 1);
        }

        /// <summary>Is a hit armor-piercing on BULWARK?</summary>
        public static bool IsArmorPiercing(string weapon, double momentum, BulwarkParams b) => b.apWeapons.Contains(weapon) || momentum > b.apMomentum;

        /// <summary>HIVE: does a hit count as a blast (x3 on sacs)? Explosions, the listed weapons, the Thumper's quake.</summary>
        public static bool IsBlast(string weapon, bool blast, bool shock, IList<string> blastWeapons) => blast || blastWeapons.Contains(weapon) || (weapon == "thumper" && shock);

        /// <summary>ORBITER: which of `n` pods' sectors an angle (rad, world) falls in, given the ring's spin.</summary>
        public static int SectorOf(double angle, double spin, int n)
        {
            double a = ((angle - spin) % (2 * Math.PI) + 2 * Math.PI) % (2 * Math.PI);
            return (int)Math.Floor(a / (2 * Math.PI) * n + 0.5) % n;
        }

        /// <summary>
        /// The end of a charge along (dx, dz) from (x, z): the first of the arena bound (a square of half-size `half`) and
        /// `maxLength`. bound: it ends on the square's edge.
        /// </summary>
        public static (double length, bool bound) ChargeLength(double x, double z, double dx, double dz, double half, double maxLength)
        {
            double t = maxLength;
            bool bound = false;
            for (int i = 0; i < 2; i++)
            {
                double p = i == 0 ? x : z, d = i == 0 ? dx : dz;
                if (Math.Abs(d) < 1e-9) continue;
                double edge = ((d > 0 ? half : -half) - p) / d;
                if (edge >= 0 && edge < t)
                {
                    t = edge;
                    bound = true;
                }
            }
            return (Math.Max(0, t), bound);
        }

        /// <summary>A weighted pick from {name: weight} (in the config's order) with a 0..1 roll.</summary>
        public static string PickWeighted(IList<(string name, double w)> weights, double roll)
        {
            double total = 0;
            foreach (var e in weights) total += e.w;
            double x = roll * total;
            foreach (var e in weights)
            {
                if ((x -= e.w) < 0) return e.name;
            }
            return weights[weights.Count - 1].name;
        }
    }
}
