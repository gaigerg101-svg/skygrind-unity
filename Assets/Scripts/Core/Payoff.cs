// NIGHT12 U3-25.8: the grade chase. design.grade.p (port-only; ULTRAKILL's P-rank, Research/retention P8 and section 5 #7):
// a "P" grade for an exfil (Exfil.Win) at grade design.grade.pGrade ("S") with a style peak of at least design.grade.pPeak
// (the "SS" rank, SKYGRIND, in config.style.ranks) and no down (no revive used). The PAYOFF strip's personal-best delta, in
// records.js judge's voice ("ROUND 9 · +2 ON YOUR BEST", "BEST ROUND 12 · 3 SHORT").
using System.Collections.Generic;
using System.Globalization;

namespace Skygrind.Core
{
    public static class Payoff
    {
        /// <summary>The design.grade.p rule's numbers, read once.</summary>
        public sealed class P
        {
            public bool enabled;
            /// <summary>The run's grade the rule needs (design.grade.pGrade).</summary>
            public string grade;
            /// <summary>The best style rank index the rule needs: design.grade.pPeak's letter in config.style.ranks (the top rank when it isn't there).</summary>
            public int peak;

            public static P From(GameConfig cfg)
            {
                var p = new P { enabled = cfg.Design("grade.p") };
                p.grade = cfg.Has("design.grade.pGrade") ? cfg.Str("design.grade.pGrade") : null;
                string letter = cfg.Has("design.grade.pPeak") ? cfg.Str("design.grade.pPeak") : null;
                List<object> ranks = cfg.Arr("style.ranks");
                p.peak = ranks.Count - 1;
                for (int i = 0; i < ranks.Count; i++)
                    if (PortData.Obj(ranks[i]).TryGetValue("letter", out object l) && l as string == letter)
                    {
                        p.peak = i;
                        break;
                    }
                if (p.grade == null) p.enabled = false; // no rule without its grade
                return p;
            }
        }

        /// <summary>Does the run earn "P" (design.grade.p)?</summary>
        public static bool IsP(RunStats s, P p) =>
            s != null && p != null && p.enabled && s.exfil == Exfil.Win && s.grade == p.grade && s.bestRankIndex >= p.peak && !s.revived;

        /// <summary>The run's grade with design.grade.p applied: "P" when the rule holds, else the run's own grade.</summary>
        public static string GradeWithP(RunStats s, P p) => IsP(s, p) ? "P" : s?.grade;

        static string N(double v) => v.ToString("R", CultureInfo.InvariantCulture);

        /// <summary>
        /// One personal-best delta line for the PAYOFF strip, or null when there is no best yet. `best` is the record from
        /// before this run (read it before Records.Submit). Rounds first, then points (records.js's order); points are the
        /// run's earned points, rounded (the record keeps whole points).
        /// </summary>
        public static string BestDelta(RunStats s, Records.Run best)
        {
            if (s == null || best == null) return null;
            double round = s.round, points = JsMath.Round(s.earned);
            if (round > best.round) return $"ROUND {N(round)} · +{N(round - best.round)} ON YOUR BEST";
            if (round < best.round) return $"BEST ROUND {N(best.round)} · {N(best.round - round)} SHORT";
            if (points > best.points) return $"ROUND {N(round)} · +{N(points - best.points)} POINTS ON YOUR BEST";
            if (points < best.points) return $"BEST ROUND {N(best.round)} · {N(best.points - points)} POINTS SHORT";
            return $"ROUND {N(round)} · YOUR BEST MATCHED";
        }
    }
}
