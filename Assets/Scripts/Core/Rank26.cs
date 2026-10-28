// NIGHT26 (builder L, contract ranks): a campaign level's rank, pure (no engine; tested in EditMode and by SimHarness). ULTRAKILL's way (RESEARCH.md S3):
// three letters per level, TIME, KILLS and STYLE, each D..S by the level's own thresholds (the plan file's `rank` block, read by LevelDef.From), and one
// AGGREGATE: S 5 / A 4 / B 3 / C 2 / D 1 per letter, summed, minus 1 per checkpoint restart (a death's room restart: Campaign24.retries); the sum is cut into
// the aggregate letter by design.n26.rank.aggregateMin (S, A, B, C minimums; below C is D). P = S in all three AND no restart: the aggregate letter is 'P'.
// Thresholds in a plan: `"rank": { "time": [s, a, b, c], "kills": [s, a, b, c], "style": [s, a, b, c] }`: time = the most seconds for S / A / B / C (slower: D),
// kills = the least percent of the level's planned enemies killed, style = the least style points. A level without the block gets the defaults from its pars
// (Defaults: time = the sum of the rooms' pars times design.n26.rank.timeShares, kills and style from design.n26.rank.*). Unity-only: the web has no campaign.
// The best per level is kept in the profile (LevelBest26, Profile.levelBest): a better letter in any one column replaces that column, so a player's card is
// the best of each, as ULTRAKILL's select shows it; an old profile has none (every level unplayed).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Skygrind.Core
{
    /// <summary>One level's thresholds: four numbers per column (S, A, B, C).</summary>
    public sealed class RankTable26
    {
        public double[] time = new double[4], kills = new double[4], style = new double[4];

        /// <summary>A plan's `rank` block (a JSON object); null when the block has no usable row. A column with fewer or more than four numbers is ignored.</summary>
        public static RankTable26 From(object json, RankTable26 fallback)
        {
            if (!(json is Dictionary<string, object> o)) return fallback;
            var t = new RankTable26();
            bool any = false;
            Column(o, "time", fallback?.time, out t.time, ref any);
            Column(o, "kills", fallback?.kills, out t.kills, ref any);
            Column(o, "style", fallback?.style, out t.style, ref any);
            return any || fallback != null ? t : null;
        }

        static void Column(Dictionary<string, object> o, string key, double[] fallback, out double[] into, ref bool any)
        {
            into = fallback != null ? (double[])fallback.Clone() : new double[4];
            if (o.TryGetValue(key, out object v) && v is List<object> l && l.Count == 4)
            {
                for (int i = 0; i < 4; i++) into[i] = PortData.Num(l[i]);
                any = true;
            }
        }
    }

    /// <summary>The numbers a rank needs from the config (design.n26.rank).</summary>
    public sealed class Rank26Params
    {
        public int[] aggregateMin = new int[4]; // the least summed points for S, A, B, C
        public double[] timeShares = new double[4]; // x the level's summed par: the default time cuts
        public double[] killsDefault = new double[4], styleDefault = new double[4];

        public static Rank26Params From(GameConfig cfg)
        {
            var p = new Rank26Params();
            Fill(cfg, "design.n26.rank.aggregateMin", p.aggregateMin);
            Fill(cfg, "design.n26.rank.timeShares", p.timeShares);
            Fill(cfg, "design.n26.rank.killsDefault", p.killsDefault);
            Fill(cfg, "design.n26.rank.styleDefault", p.styleDefault);
            return p;
        }

        static void Fill(GameConfig cfg, string path, int[] into)
        {
            var d = new double[4];
            Fill(cfg, path, d);
            for (int i = 0; i < 4; i++) into[i] = (int)d[i];
        }

        static void Fill(GameConfig cfg, string path, double[] into)
        {
            if (!cfg.Has(path) || !(cfg.Get(path) is List<object> l)) return;
            for (int i = 0; i < 4 && i < l.Count; i++) into[i] = PortData.Num(l[i]);
        }

        /// <summary>The defaults for a level that names no `rank` block: its pars decide the time cuts.</summary>
        public RankTable26 DefaultsFor(double parSum)
        {
            var t = new RankTable26();
            for (int i = 0; i < 4; i++)
            {
                t.time[i] = Math.Max(1, parSum) * timeShares[i];
                t.kills[i] = killsDefault[i];
                t.style[i] = styleDefault[i];
            }
            return t;
        }
    }

    /// <summary>A finished run's three letters and the aggregate.</summary>
    public sealed class LevelRank26
    {
        public char time = 'D', kills = 'D', style = 'D', aggregate = 'D';
        public int points, restarts;
        public bool p;
        public double seconds, killPct, styleScore;
        /// <summary>The summed points before the restarts' cost (15 is the most).</summary>
        public int raw;

        /// <summary>"TIME S  KILLS A  STYLE B  OVERALL A" (the stamp and the RESULTS summary use the pieces).</summary>
        public string Line => "TIME " + time + "  KILLS " + kills + "  STYLE " + style + "  OVERALL " + aggregate;
    }

    public static class Rank26
    {
        static readonly char[] Letters = { 'S', 'A', 'B', 'C', 'D' };

        /// <summary>The points of a letter: S 5, A 4, B 3, C 2, D 1 (P counts as S).</summary>
        public static int Points(char letter) => letter == 'S' || letter == 'P' ? 5 : letter == 'A' ? 4 : letter == 'B' ? 3 : letter == 'C' ? 2 : letter == 'D' ? 1 : 0;

        /// <summary>The order of letters for "best": D 1 .. S 5, P 6, none 0.</summary>
        public static int Order(char letter) => letter == 'P' ? 6 : Points(letter);

        /// <summary>A column's letter: with lowerIsBetter (time) the first cut the value is at or under, else the first it is at or over; D when none.</summary>
        public static char Letter(double v, double[] cuts, bool lowerIsBetter)
        {
            for (int i = 0; i < 4; i++)
                if (lowerIsBetter ? v <= cuts[i] : v >= cuts[i]) return Letters[i];
            return 'D';
        }

        /// <summary>The aggregate letter of a summed point total (restarts already taken off): S, A, B or C at the config's minimums, else D.</summary>
        public static char AggregateLetter(int points, int[] mins)
        {
            for (int i = 0; i < 4; i++) if (points >= mins[i]) return Letters[i];
            return 'D';
        }

        /// <summary>Rank a finished level. killPct is 0..100 (kills over the plan's enemies, capped); restarts = the checkpoint restarts the run took.</summary>
        public static LevelRank26 Compute(RankTable26 table, Rank26Params p, double seconds, double killPct, double styleScore, int restarts)
        {
            var r = new LevelRank26 { seconds = seconds, killPct = killPct, styleScore = styleScore, restarts = Math.Max(0, restarts) };
            r.time = Letter(seconds, table.time, true);
            r.kills = Letter(killPct, table.kills, false);
            r.style = Letter(styleScore, table.style, false);
            r.raw = Points(r.time) + Points(r.kills) + Points(r.style);
            r.points = Math.Max(0, r.raw - r.restarts);
            r.p = r.time == 'S' && r.kills == 'S' && r.style == 'S' && r.restarts == 0;
            r.aggregate = r.p ? 'P' : AggregateLetter(r.points, p.aggregateMin);
            return r;
        }

        /// <summary>m:ss, rounded to the second ("--:--" for none).</summary>
        public static string TimeText(double seconds)
        {
            if (seconds <= 0) return "--:--";
            int t = (int)Math.Round(seconds, MidpointRounding.AwayFromZero);
            return (t / 60).ToString(CultureInfo.InvariantCulture) + ":" + (t % 60).ToString("00", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>One level's best, as the profile keeps it: the best letter of each column, the best raw numbers, the aggregate and P.</summary>
    public sealed class LevelBest26
    {
        public char time = '\0', kills = '\0', style = '\0', aggregate = '\0';
        public double seconds, killPct, styleScore; // best (least time, most kills, most style); 0 = none
        public int clears;
        public bool Played => clears > 0;

        /// <summary>Keep a finished run when it beats any column; true when something changed (the caller saves).</summary>
        public bool Submit(LevelRank26 r)
        {
            clears++; // a clear is always news (the count changes), so a submit always asks for a save
            if (Order(r.time) > Order(time)) time = r.time;
            if (Order(r.kills) > Order(kills)) kills = r.kills;
            if (Order(r.style) > Order(style)) style = r.style;
            if (Order(r.aggregate) > Order(aggregate)) aggregate = r.aggregate;
            if (seconds <= 0 || r.seconds < seconds) seconds = r.seconds;
            if (r.killPct > killPct) killPct = r.killPct;
            if (r.styleScore > styleScore) styleScore = r.styleScore;
            return true;
        }

        static int Order(char c) => Rank26.Order(c);

        /// <summary>The compact JSON object a profile writes (keys fixed, so the same card is the same bytes).</summary>
        public string ToJson()
        {
            var sb = new StringBuilder(96);
            sb.Append("{\"agg\":\"").Append(aggregate == '\0' ? '-' : aggregate).Append("\",\"t\":\"").Append(time == '\0' ? '-' : time)
              .Append("\",\"k\":\"").Append(kills == '\0' ? '-' : kills).Append("\",\"s\":\"").Append(style == '\0' ? '-' : style)
              .Append("\",\"sec\":").Append(seconds.ToString("R", CultureInfo.InvariantCulture))
              .Append(",\"kp\":").Append(killPct.ToString("R", CultureInfo.InvariantCulture))
              .Append(",\"sty\":").Append(styleScore.ToString("R", CultureInfo.InvariantCulture))
              .Append(",\"n\":").Append(clears.ToString(CultureInfo.InvariantCulture)).Append('}');
            return sb.ToString();
        }

        public static LevelBest26 FromObj(Dictionary<string, object> o)
        {
            var b = new LevelBest26
            {
                aggregate = Ch(o, "agg"), time = Ch(o, "t"), kills = Ch(o, "k"), style = Ch(o, "s"),
                seconds = Num(o, "sec"), killPct = Num(o, "kp"), styleScore = Num(o, "sty"), clears = (int)Num(o, "n"),
            };
            return b;
        }

        static char Ch(Dictionary<string, object> o, string k)
        {
            if (!o.TryGetValue(k, out object v) || !(v is string s) || s.Length != 1) return '\0';
            char c = s[0];
            return c == 'S' || c == 'A' || c == 'B' || c == 'C' || c == 'D' || c == 'P' ? c : '\0';
        }

        static double Num(Dictionary<string, object> o, string k) => o.TryGetValue(k, out object v) && v is double d && !double.IsNaN(d) && d >= 0 ? d : 0;
    }
}
