// The style meter, ported from skygrind/src/style: styleMath.js (pure: awards, multipliers, weapon freshness, ranks,
// drain, the rank-down delay), styleCore.js (the scoring core: named awards x state x freshness x diminishing per
// source, hit awards once per shot under an income cap, uneven rising thresholds) and styleMeter.js (the bus wiring:
// kills, hits, parries, movement tricks, uses, damage; NIGHT9: StyleCore.cs and StyleMeter.cs). Six ranks DULL .. SKYGRIND; the rank multiplies points
// (economy), speeds regen (regen.rankPerRank) and stamina (style.rankRewardPerRank). Golden vectors:
// PortExport/vectors/styleMath.json.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class StyleMath
    {
        public static readonly string[] FreshWeapons = { "revolver", "shotgun", "slam", "punch", "nailgun", "rocket", "rail" };
        static readonly Dictionary<string, string> FreshKeyOf = new Dictionary<string, string>
        {
            ["revolver"] = "revolver", ["ricochet"] = "revolver", ["shotgun"] = "shotgun", ["slam"] = "slam", ["punch"] = "punch", ["orb"] = "punch",
            ["nailgun"] = "nailgun", ["rocket"] = "rocket", ["rail"] = "rail",
        };
        static readonly HashSet<string> AirStates = new HashSet<string> { "AIR", "DASH", "WALLRIDE", "SLAM", "SWING" };
        static readonly HashSet<string> MultStates = new HashSet<string> { "AIR", "DASH", "WALLRIDE", "SLAM", "SLIDE", "SWING" };
        static readonly Dictionary<string, string> MeleeKills = new Dictionary<string, string>
        {
            ["gauntlet"] = "FISTFUL", ["sunder"] = "CLEAVED", ["flicker"] = "PERFORATED", ["thumper"] = "FLATTENED", ["tether"] = "REELED IN",
        };

        public sealed class S
        {
            public GameConfig c;
            public double start, useCost, otherGain, max, regen, freshAt, usedAt, freshMult, usedMult, staleMult, nailgunUseScale;
            public double stateMult, closeCallDistance, trickRepeatWindow, multikillWindow, damagePenalty, topBand, drainBase, drainPerRank, rankDownDelay;
            public double rankRewardPerRank, pulseBase, pulsePerRank, trickEnemyRange, hitCapBurst, hitCapPerSecond, dimWindow, dimRepeat, dimFloor;
            public int warmRank;
            public double[] thresholds;
            public List<string> ranks; // names (DULL .. SKYGRIND)
            public List<string> letters = new List<string>();
            public List<uint> colors = new List<uint>();
            public Dictionary<string, double> baseP = new Dictionary<string, double>(), bonus = new Dictionary<string, double>(), tricks = new Dictionary<string, double>();
        }

        public static S From(GameConfig c)
        {
            var s = new S
            {
                c = c,
                start = c.Num("style.freshness.start"), useCost = c.Num("style.freshness.useCost"), otherGain = c.Num("style.freshness.otherGain"),
                max = c.Num("style.freshness.max"), regen = c.Num("style.freshness.regen"), freshAt = c.Num("style.freshness.freshAt"),
                usedAt = c.Num("style.freshness.usedAt"), freshMult = c.Num("style.freshness.freshMult"), usedMult = c.Num("style.freshness.usedMult"),
                staleMult = c.Num("style.freshness.staleMult"), nailgunUseScale = c.Num("style.freshness.nailgunUseScale"),
                stateMult = c.Num("style.stateMult"), closeCallDistance = c.Num("style.closeCallDistance"), trickRepeatWindow = c.Num("style.trickRepeatWindow"),
                multikillWindow = c.Num("style.multikillWindow"), damagePenalty = c.Num("style.damagePenalty"), topBand = c.Num("style.topBand"),
                drainBase = c.Num("style.drainBase"), drainPerRank = c.Num("style.drainPerRank"), rankDownDelay = c.Num("style.rankDownDelay"),
                rankRewardPerRank = c.Num("style.rankRewardPerRank"), pulseBase = c.Num("style.pulseBase"), pulsePerRank = c.Num("style.pulsePerRank"),
                trickEnemyRange = c.Num("style.trickEnemyRange"), hitCapBurst = c.Num("style.hitCap.burst"), hitCapPerSecond = c.Num("style.hitCap.perSecond"),
                dimWindow = c.Num("style.diminish.window"), dimRepeat = c.Num("style.diminish.repeat"), dimFloor = c.Num("style.diminish.floor"),
                warmRank = c.Int("style.warmRank"), thresholds = c.Nums("style.thresholds"), ranks = new List<string>(),
            };
            foreach (object r in c.Arr("style.ranks"))
            {
                var d = PortData.Obj(r);
                s.ranks.Add((string)d["name"]);
                s.letters.Add((string)d["letter"]);
                s.colors.Add((uint)PortData.Num(d["color"]));
            }
            foreach (var kv in c.Obj("style.base")) s.baseP[kv.Key] = PortData.Num(kv.Value);
            foreach (var kv in c.Obj("style.bonus")) s.bonus[kv.Key] = PortData.Num(kv.Value);
            foreach (var kv in c.Obj("style.tricks")) s.tricks[kv.Key] = PortData.Num(kv.Value);
            return s;
        }

        public static string FreshKey(string weapon) => weapon != null && FreshKeyOf.TryGetValue(weapon, out string k) ? k : null;

        public static Dictionary<string, double> CreateFreshness(S s)
        {
            var d = new Dictionary<string, double>();
            foreach (string w in FreshWeapons) d[w] = s.start;
            return d;
        }

        public static void UseFreshness(Dictionary<string, double> fresh, string key, S s, double scale = 1)
        {
            foreach (string w in FreshWeapons) fresh[w] = w == key ? Math.Max(0, fresh[w] - s.useCost * scale) : Math.Min(s.max, fresh[w] + s.otherGain * scale);
        }

        public static void RegenFreshness(Dictionary<string, double> fresh, double dt, S s)
        {
            foreach (string w in FreshWeapons) fresh[w] = Math.Min(s.max, fresh[w] + s.regen * dt);
        }

        public static string FreshnessTierName(double value, S s) => value >= s.freshAt ? "FRESH" : value >= s.usedAt ? "USED" : "STALE";

        public static double FreshnessMult(double value, S s) => value >= s.freshAt ? s.freshMult : value >= s.usedAt ? s.usedMult : s.staleMult;

        public static double StateMultiplier(string state, S s) => MultStates.Contains(state) ? s.stateMult : 1;

        /// <summary>A state that earns the state multiplier (NIGHT8 U2-16: the LAB's STYLE RAMP counts its time).</summary>
        public static bool IsMultState(string state) => MultStates.Contains(state);

        public struct Award
        {
            public string name;
            public double points;
            public bool quiet;
        }

        public static List<Award> KillAwards(string kind, string weapon, string state, double distance, int recentKills, bool melee, bool bounced, S s)
        {
            var output = new List<Award> { new Award { name = "KILL", points = s.baseP.TryGetValue(kind + "Kill", out double kp) ? kp : 0 } };
            if (weapon == "punch") output.Add(new Award { name = "FISTFUL", points = s.baseP["fistful"] });
            else if (melee) output.Add(new Award { name = MeleeKills.TryGetValue(weapon, out string mk) ? mk : "FISTFUL", points = s.baseP["fistful"] });
            if (weapon == "orb") output.Add(new Award { name = "RETURN TO SENDER", points = s.baseP["returnToSender"] });
            if (weapon == "ricochet" || bounced) output.Add(new Award { name = "RICOSHOT", points = s.bonus["ricoshot"] });
            if (weapon == "slam") output.Add(new Award { name = "SLAMDOWN", points = s.bonus["slamdown"] });
            if (AirStates.Contains(state)) output.Add(new Award { name = "AIRSHOT", points = s.bonus["airshot"] });
            if (state == "WALLRIDE") output.Add(new Award { name = "WALLKILL", points = s.bonus["wallkill"] });
            if (recentKills >= 2) output.Add(new Award { name = "MULTIKILL", points = s.bonus["multikill"] });
            if (distance <= s.closeCallDistance && !melee && weapon != "punch") output.Add(new Award { name = "CLOSE CALL", points = s.bonus["closeCall"] });
            return output;
        }

        public static List<Award> HitAwards(string weapon, bool headshot, double pointScale, bool melee, S s)
        {
            double b = melee || weapon == "punch" ? s.baseP["meleeHit"] : s.baseP["hit"];
            var output = new List<Award> { new Award { name = "HIT", points = b * pointScale, quiet = true } };
            if (headshot && weapon == "revolver") output.Add(new Award { name = "HEADSHOT", points = s.bonus["headshot"] });
            return output;
        }

        public static double TrickPoints(string trick, double now, Dictionary<string, double> last, bool enemyNear, S s)
        {
            bool had = last.TryGetValue(trick, out double previous);
            last[trick] = now;
            if (!enemyNear || !s.tricks.ContainsKey(trick)) return 0;
            if (had && now - previous < s.trickRepeatWindow) return 0;
            return s.tricks[trick];
        }

        public static double ApplyMultipliers(double points, double stateMult, double freshMult) => JsMath.Round(points * stateMult * freshMult);

        public static int RankOf(double meter, S s)
        {
            int r = 0;
            for (int i = 1; i < s.thresholds.Length; i++) if (meter >= s.thresholds[i]) r = i;
            return Math.Min(s.ranks.Count - 1, r);
        }

        public static double MeterMax(S s) => s.thresholds[s.thresholds.Length - 1] + s.topBand;

        public static (double lo, double hi) RankBand(int rank, S s)
        {
            int r = Math.Max(0, Math.Min(s.thresholds.Length - 1, rank));
            return (s.thresholds[r], r + 1 < s.thresholds.Length ? s.thresholds[r + 1] : s.thresholds[r] + s.topBand);
        }

        public static double RankFill(double meter, int rank, S s)
        {
            var (lo, hi) = RankBand(rank, s);
            return Math.Max(0, Math.Min(1, (meter - lo) / (hi - lo)));
        }

        public static double RankMid(int rank, S s)
        {
            var (lo, hi) = RankBand(rank, s);
            return (lo + hi) / 2;
        }

        /// <summary>Per-source diminishing returns: `times` pruned in place, this award appended; the multiplier.</summary>
        public static double Diminish(List<double> times, double now, double window, double repeat, double floor)
        {
            int n = 0;
            for (int i = 0; i < times.Count; i++) if (now - times[i] < window) times[n++] = times[i];
            times.RemoveRange(n, times.Count - n);
            times.Add(now);
            return Math.Max(floor, JsMath.Pow(repeat, n));
        }

        public static string DiminishKey(string name, string weapon) => !string.IsNullOrEmpty(weapon) ? $"{name}:{weapon}" : name;

        public static double TakeHitIncome(ref double tokens, double points)
        {
            double paid = Math.Max(0, Math.Min(points, Math.Floor(tokens)));
            tokens -= paid;
            return paid;
        }

        public static double RefillHitIncome(double tokens, double dt, double burst, double perSecond) => Math.Min(burst, tokens + perSecond * dt);

        public static double DrainRate(int rank, S s) => s.drainBase + s.drainPerRank * rank;

        public static int StepShownRank(ref int shownRank, ref double below, double meter, double dt, S s)
        {
            int target = RankOf(meter, s);
            if (target >= shownRank)
            {
                shownRank = target;
                below = 0;
            }
            else
            {
                below += dt;
                if (below >= s.rankDownDelay)
                {
                    shownRank = target;
                    below = 0;
                }
            }
            return shownRank;
        }

        public static double StaminaRewardMult(int rank, S s) => 1 + s.rankRewardPerRank * Math.Max(0, Math.Min(s.ranks.Count - 1, rank));

        public static double RankPulseRate(int rank, S s) => s.pulseBase + s.pulsePerRank * Math.Max(0, Math.Min(s.ranks.Count - 1, rank));

        public static double RankEnergy(int rank, double fill, S s) => Math.Max(0, Math.Min(1, (rank + Math.Max(0, Math.Min(1, fill))) / s.ranks.Count));

        public static double RankWarmth(int rank, S s)
        {
            int span = s.ranks.Count - 1 - s.warmRank;
            if (rank < s.warmRank) return 0;
            return span <= 0 ? 1 : Math.Min(1, (double)(rank - s.warmRank + 1) / (span + 1));
        }
    }
}
