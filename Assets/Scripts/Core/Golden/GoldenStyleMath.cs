// The style-math oracle (NIGHT9 U3-04): every record of PortExport/vectors/styleMath.json replayed through StyleMath,
// numbers bit for bit (results, and the mutated arguments where recorded). Moved here from StyleGoldenTests (one
// public method per test there) so the EditMode tests, the Player's -skygrindGolden and the dotnet harness run one
// evaluation. The params come from config.json alone (GoldenData.Config): StyleMath.From reads only style.*, so the
// Unity design config the tests used to load never changed a number here.
using System.Collections.Generic;
using System.Linq;

namespace Skygrind.Core
{
    public static partial class Golden
    {
        static partial void AddStyle(Dictionary<string, GoldenCheck> d)
        {
            d["styleMath"] = GoldenStyleMath.CheckSet;
        }
    }

    /// <summary>styleMath.json, in the groups StyleGoldenTests checks (each a public method), and the whole set.</summary>
    public static class GoldenStyleMath
    {
        public const string Set = "styleMath";

        static readonly string[] Handled =
        {
            "freshKey", "createFreshness", "useFreshness", "regenFreshness", "freshnessTierName", "freshnessTier",
            "stateMultiplier", "killAwards", "hitAwards", "trickPoints", "applyMultipliers",
            "rankOf", "meterMax", "rankBand", "rankMid", "rankFill", "drainRate", "staminaRewardMult", "rankPulseRate", "rankWarmth", "rankEnergy", "stepShownRank",
            "diminish", "diminishKey", "takeHitIncome", "refillHitIncome",
            "constant:FRESH_WEAPONS",
        };

        /// <summary>Every record: each group, then a mismatch for any record no group handles.</summary>
        public static void CheckSet(GoldenData data, GoldenResult r)
        {
            Freshness(data, r);
            Awards(data, r);
            Ranks(data, r);
            DiminishingAndTheHitCap(data, r);
            Constants(data, r);
            GoldenWalk.Unhandled(data, r, Set, Handled);
        }

        /// <summary>The style params the web's calls were recorded with ({"$config": "style"}: config.json's style section).</summary>
        public static StyleMath.S Params(GoldenData data) => StyleMath.From(data.Config);

        public static void Freshness(GoldenData data, GoldenResult r)
        {
            StyleMath.S s = Params(data);
            GoldenWalk.Each(data, r, Set, "freshKey", c => r.SameStr(c["result"], StyleMath.FreshKey((string)Args(c)[0]), "result"));
            GoldenWalk.Each(data, r, Set, "createFreshness", c => r.SameJson(c["result"], Json(StyleMath.CreateFreshness(s)), "result"));
            GoldenWalk.Each(data, r, Set, "useFreshness", c =>
            {
                var a = Args(c);
                var f = Fresh(a[0]);
                StyleMath.UseFreshness(f, a[1] as string, s, a.Count > 3 ? N(a[3]) : 1);
                r.SameJson(c["result"], Json(f), "result");
            });
            GoldenWalk.Each(data, r, Set, "regenFreshness", c =>
            {
                var a = Args(c);
                var f = Fresh(a[0]);
                StyleMath.RegenFreshness(f, N(a[1]), s);
                r.SameJson(c["result"], Json(f), "result");
            });
            GoldenWalk.Each(data, r, Set, "freshnessTierName", c => r.SameStr(c["result"], StyleMath.FreshnessTierName(N(Args(c)[0]), s), "result"));
            GoldenWalk.Each(data, r, Set, "freshnessTier", c =>
            {
                var res = PortData.Obj(c["result"]);
                double v = N(Args(c)[0]);
                r.SameStr(res["tier"], StyleMath.FreshnessTierName(v, s), "result.tier");
                r.Same(res["mult"], StyleMath.FreshnessMult(v, s), "result.mult");
            });
        }

        public static void Awards(GoldenData data, GoldenResult r)
        {
            StyleMath.S s = Params(data);
            GoldenWalk.Each(data, r, Set, "stateMultiplier", c => r.Same(c["result"], StyleMath.StateMultiplier((string)Args(c)[0], s), "result"));
            GoldenWalk.Each(data, r, Set, "killAwards", c =>
            {
                var k = PortData.Obj(Args(c)[0]);
                var got = StyleMath.KillAwards((string)k["kind"], (string)k["weapon"], (string)k["state"], N(k["distance"]), (int)N(k["recentKills"]), Flag(k, "melee"), Flag(k, "bounced"), s);
                SameAwards(r, c["result"], got);
            });
            GoldenWalk.Each(data, r, Set, "hitAwards", c =>
            {
                var h = PortData.Obj(Args(c)[0]);
                double scale = h.TryGetValue("pointScale", out object ps) ? N(ps) : 1;
                SameAwards(r, c["result"], StyleMath.HitAwards((string)h["weapon"], Flag(h, "headshot"), scale, Flag(h, "melee"), s));
            });
            GoldenWalk.Each(data, r, Set, "trickPoints", c =>
            {
                var a = Args(c);
                var last = Fresh(a[2]);
                r.Same(c["result"], StyleMath.TrickPoints((string)a[0], N(a[1]), last, (bool)a[3], s), "result");
                r.SameJson(GoldenWalk.After(c)[2], Json(last), "argsAfter.last");
            });
            GoldenWalk.Each(data, r, Set, "applyMultipliers", c =>
            {
                var a = Args(c);
                r.Same(c["result"], StyleMath.ApplyMultipliers(N(a[0]), N(a[1]), N(a[2])), "result");
            });
        }

        public static void Ranks(GoldenData data, GoldenResult r)
        {
            StyleMath.S s = Params(data);
            GoldenWalk.Each(data, r, Set, "rankOf", c => r.Same(c["result"], StyleMath.RankOf(N(Args(c)[0]), s), "result"));
            GoldenWalk.Each(data, r, Set, "meterMax", c => r.Same(c["result"], StyleMath.MeterMax(s), "result"));
            GoldenWalk.Each(data, r, Set, "rankBand", c =>
            {
                var (lo, hi) = StyleMath.RankBand((int)N(Args(c)[0]), s);
                r.Same(PortData.Obj(c["result"])["lo"], lo, "result.lo");
                r.Same(PortData.Obj(c["result"])["hi"], hi, "result.hi");
            });
            GoldenWalk.Each(data, r, Set, "rankMid", c => r.Same(c["result"], StyleMath.RankMid((int)N(Args(c)[0]), s), "result"));
            GoldenWalk.Each(data, r, Set, "rankFill", c =>
            {
                var a = Args(c);
                r.Same(c["result"], StyleMath.RankFill(N(a[0]), (int)N(a[1]), s), "result");
            });
            GoldenWalk.Each(data, r, Set, "drainRate", c => r.Same(c["result"], StyleMath.DrainRate((int)N(Args(c)[0]), s), "result"));
            GoldenWalk.Each(data, r, Set, "staminaRewardMult", c => r.Same(c["result"], StyleMath.StaminaRewardMult((int)N(Args(c)[0]), s), "result"));
            GoldenWalk.Each(data, r, Set, "rankPulseRate", c => r.Same(c["result"], StyleMath.RankPulseRate((int)N(Args(c)[0]), s), "result"));
            GoldenWalk.Each(data, r, Set, "rankWarmth", c => r.Same(c["result"], StyleMath.RankWarmth((int)N(Args(c)[0]), s), "result"));
            GoldenWalk.Each(data, r, Set, "rankEnergy", c =>
            {
                var a = Args(c);
                r.Same(c["result"], StyleMath.RankEnergy((int)N(a[0]), N(a[1]), s), "result");
            });
            GoldenWalk.Each(data, r, Set, "stepShownRank", c =>
            {
                var a = Args(c);
                var shown = PortData.Obj(a[0]);
                int rank = (int)N(shown["rank"]);
                double below = N(shown["below"]);
                r.Same(c["result"], StyleMath.StepShownRank(ref rank, ref below, N(a[1]), N(a[2]), s), "result");
                var after = PortData.Obj(GoldenWalk.After(c)[0]);
                r.Same(after["rank"], rank, "argsAfter.rank");
                r.Same(after["below"], below, "argsAfter.below");
            });
        }

        public static void DiminishingAndTheHitCap(GoldenData data, GoldenResult r)
        {
            StyleMath.S s = Params(data);
            GoldenWalk.Each(data, r, Set, "diminish", c =>
            {
                var a = Args(c);
                var times = PortData.Arr(a[0]).ConvertAll(PortData.Num);
                r.Same(c["result"], StyleMath.Diminish(times, N(a[1]), s.dimWindow, s.dimRepeat, s.dimFloor), "result");
                r.SameJson(GoldenWalk.After(c)[0], times, "argsAfter.times");
            });
            GoldenWalk.Each(data, r, Set, "diminishKey", c =>
            {
                var a = Args(c);
                r.SameStr(c["result"], StyleMath.DiminishKey((string)a[0], a[1] as string), "result");
            });
            GoldenWalk.Each(data, r, Set, "takeHitIncome", c =>
            {
                var a = Args(c);
                double tokens = N(PortData.Obj(a[0])["tokens"]);
                r.Same(c["result"], StyleMath.TakeHitIncome(ref tokens, N(a[1])), "result");
                r.Same(PortData.Obj(GoldenWalk.After(c)[0])["tokens"], tokens, "argsAfter.tokens");
            });
            // The web refills the {tokens} object in place and returns undefined; the C# returns the refilled count.
            GoldenWalk.Each(data, r, Set, "refillHitIncome", c =>
            {
                var a = Args(c);
                double tokens = StyleMath.RefillHitIncome(N(PortData.Obj(a[0])["tokens"]), N(a[1]), s.hitCapBurst, s.hitCapPerSecond);
                r.Same(PortData.Obj(GoldenWalk.After(c)[0])["tokens"], tokens, "argsAfter.tokens");
            });
        }

        /// <summary>The recorded constants (FRESH_WEAPONS: the freshness keys, in order).</summary>
        public static void Constants(GoldenData data, GoldenResult r) =>
            GoldenWalk.Each(data, r, Set, "constant:FRESH_WEAPONS", c => r.SameJson(c["value"], StyleMath.FreshWeapons, "value", true));

        // --- decoding -----------------------------------------------------------------------------------------------

        static List<object> Args(Dictionary<string, object> c) => GoldenWalk.Args(c);

        static double N(object o) => PortData.Num(o);

        static bool Flag(Dictionary<string, object> d, string key) => d.TryGetValue(key, out object v) && v is bool b && b;

        /// <summary>A recorded {key: number} object (freshness, trick times) as the C# dictionary.</summary>
        static Dictionary<string, double> Fresh(object o) => PortData.Obj(o).ToDictionary(kv => kv.Key, kv => N(kv.Value));

        static Dictionary<string, object> Json(Dictionary<string, double> d) => d.ToDictionary(kv => kv.Key, kv => (object)kv.Value);

        /// <summary>Award lists: the same names in the same order, points bit for bit, `quiet` where recorded (else false).</summary>
        static void SameAwards(GoldenResult r, object want, List<StyleMath.Award> got)
        {
            var w = PortData.Arr(want).ConvertAll(PortData.Obj);
            if (w.Count != got.Count)
                r.Fail($"result: expected [{string.Join(", ", w.Select(x => (string)x["name"]))}] got [{string.Join(", ", got.Select(x => x.name))}]");
            for (int i = 0; i < System.Math.Min(w.Count, got.Count); i++)
            {
                r.SameStr(w[i]["name"], got[i].name, $"result[{i}].name");
                r.Same(w[i]["points"], got[i].points, $"result[{i}].points");
                if (w[i].TryGetValue("quiet", out object q)) r.SameBool(q, got[i].quiet, $"result[{i}].quiet");
                else if (got[i].quiet) r.Fail($"result[{i}].quiet: expected none got true");
            }
        }
    }
}
