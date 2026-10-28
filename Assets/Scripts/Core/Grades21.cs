// NIGHT21 U3-35.5 THE STAMP (builder S owns this file): a grade for every round, Zombies' round clear read the way
// ULTRAKILL's level grades are (damage taken, style, time). Pure: the sim hands it the player's rank, max HP and whether a
// revive was used (SurvivalSim.Stamp21.cs). Port-only: the web has a grade for the run (runMoments.js gradeOf) and none for a
// round, so the blend's numbers are design.n21.stamp's (Unity-only; each with its why in Design/unity-config.json).
// - The round's score: damageWeight x (1 - damage taken / (damageFull x max HP)) + rankWeight x (peak style rank / top rank)
//   + timeWeight x the clear's pace (1 at or under the par, 0 at slowMult x par; par = parFlat + parPerKill x the round's kills),
//   each part held to 0..1. The letter is the first of config.results.grade.bands the score reaches (the run grade's own
//   bands: S A B C, else D), through RunMoments.Grade.
// - THE ROUND P is the run P rule's (design.grade.p, Core/Payoff.cs) with the round for the run: the round's grade is the
//   rule's grade (S), its peak style rank is the rule's peak (SS) or better, and no revive has been used. design.grade.p off
//   or absent: no round P.
// The tracker listens to roundBegin / hurt / kill / roundClear and says "roundGrade" (RoundGradeEvent) at each round clear,
// after the clear's own events. It keeps the run's grades (RESULTS' round strip). Nothing allocates per step.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>The "roundGrade" event: the round just cleared and its grade (a round P is "P").</summary>
    public sealed class RoundGradeEvent
    {
        public int round;
        /// <summary>"P" | "S" | "A" | "B" | "C" | "D".</summary>
        public string grade;
        /// <summary>The letter the blend reached (the P rule not applied).</summary>
        public string letter;
        public bool p;
        public double score, damage, time;
        public int peakRank, kills;
    }

    public static class Grades21
    {
        public const string EventName = "roundGrade";

        /// <summary>design.n21.stamp's numbers, read once.</summary>
        public sealed class Params
        {
            public double damageWeight, rankWeight, timeWeight, damageFull, parFlat, parPerKill, slowMult;

            public static Params From(GameConfig cfg) => new Params
            {
                damageWeight = cfg.Num("design.n21.stamp.damageWeight"),
                rankWeight = cfg.Num("design.n21.stamp.rankWeight"),
                timeWeight = cfg.Num("design.n21.stamp.timeWeight"),
                damageFull = cfg.Num("design.n21.stamp.damageFull"),
                parFlat = cfg.Num("design.n21.stamp.parFlat"),
                parPerKill = cfg.Num("design.n21.stamp.parPerKill"),
                slowMult = cfg.Num("design.n21.stamp.slowMult"),
            };
        }

        static double Clamp01(double v) => v < 0 ? 0 : v > 1 ? 1 : v;

        /// <summary>The round's par: the seconds a clean clear takes.</summary>
        public static double Par(int kills, Params p) => p.parFlat + p.parPerKill * kills;

        /// <summary>The blend: 0..1 (the weights sum to 1).</summary>
        public static double Score(double damage, double maxHp, int peakRank, int topRank, double time, int kills, Params p)
        {
            double dmg = maxHp > 0 && p.damageFull > 0 ? Clamp01(1 - damage / (p.damageFull * maxHp)) : 1;
            double rank = topRank > 0 ? Clamp01((double)peakRank / topRank) : 0;
            double par = Par(kills, p);
            double slow = par * p.slowMult;
            double pace = slow > par ? Clamp01((slow - time) / (slow - par)) : (time <= par ? 1 : 0);
            return p.damageWeight * dmg + p.rankWeight * rank + p.timeWeight * pace;
        }

        /// <summary>The first band the score reaches (the run grade's bands), else the last letter's floor "D".</summary>
        public static string Letter(double score, RunMoments.Grade g)
        {
            for (int i = 0; i < g.letters.Count; i++) if (score >= g.at[i]) return g.letters[i];
            return "D";
        }

        /// <summary>The run P rule on a round: its letter is the rule's grade, its peak rank the rule's or better, no revive used.</summary>
        public static bool IsRoundP(string letter, int peakRank, bool revived, Payoff.P rule) =>
            rule != null && rule.enabled && letter == rule.grade && peakRank >= rule.peak && !revived;
    }

    /// <summary>
    /// NIGHT22 U3-36.4 (builder S): the inked stamp's motion, pure, so the UI, the harness and the tests share one curve. The
    /// stamp enters slamming from `fromScale` to 1 with its alpha rising over the first half (OutQuad on the scale: it is within
    /// 3 % of rest at 85 % of inTime, which is the time feel-metrics M9 counts as "settled"), holds, then leaves over outTime
    /// shrinking to `outScale` while it fades. Numbers: design.n21.stamp (Unity-only; each with its why). Nothing allocates.
    /// </summary>
    public static class StampMotion
    {
        public struct P
        {
            public double inTime, holdTime, outTime, fromScale, outScale;

            public static P From(GameConfig cfg) => new P
            {
                inTime = cfg.Num("design.n21.stamp.inTime"),
                holdTime = cfg.Num("design.n21.stamp.holdTime"),
                outTime = cfg.Num("design.n21.stamp.outTime"),
                fromScale = cfg.Num("design.n21.stamp.fromScale"),
                outScale = cfg.Num("design.n21.stamp.outScale"),
            };

            public double Total => inTime + holdTime + outTime;
        }

        /// <summary>The stamp's scale and alpha `age` s after its event; alpha 0 before it and after the exit.</summary>
        public static void At(double age, in P p, out double scale, out double alpha)
        {
            if (age < 0 || age >= p.Total)
            {
                scale = 1;
                alpha = 0;
                return;
            }
            if (age < p.inTime)
            {
                double u = age / p.inTime;
                double e = 1 - (1 - u) * (1 - u); // OutQuad
                scale = p.fromScale + (1 - p.fromScale) * e;
                double a = u * 2;
                alpha = a >= 1 ? 1 : 1 - (1 - a) * (1 - a) * (1 - a); // OutCubic over the first half
                return;
            }
            age -= p.inTime;
            if (age < p.holdTime)
            {
                scale = 1;
                alpha = 1;
                return;
            }
            double v = (age - p.holdTime) / p.outTime;
            scale = 1 + (p.outScale - 1) * v * v; // InQuad: it leaves quick
            alpha = 1 - v;
        }

        /// <summary>M9's entrance time at `fps`: from the first frame the stamp is visible to the first frame it is within 3 % of rest scale
        /// at 95 % alpha and stays so for 6 frames (feel-metrics.py m9); ms.</summary>
        public static double EntranceMs(in P p, double fps)
        {
            int first = -1, run = 0, settle = -1;
            for (int k = 1; k < (int)(p.inTime * fps * 3) + 12; k++)
            {
                At(k / fps, p, out double s, out double a);
                if (first < 0 && a > 0) first = k;
                if (first < 0) continue;
                bool ok = Math.Abs(s - 1) <= 0.03 && a >= 0.95;
                run = ok ? run + 1 : 0;
                if (run >= 6)
                {
                    settle = k - 5;
                    break;
                }
            }
            return settle < 0 ? -1 : (settle - first) * 1000.0 / fps;
        }

        /// <summary>The exit's length at `fps`: the frames from the first one under full alpha after the hold to the last visible one; ms.</summary>
        public static double ExitMs(in P p, double fps)
        {
            int from = -1, last = -1;
            for (int k = (int)((p.inTime + p.holdTime) * fps) - 2; k < (int)(p.Total * fps) + 4; k++)
            {
                if (k < 1) continue;
                At(k / fps, p, out _, out double a);
                if (a > 0 && a < 1 - 1e-9 && from < 0 && k / fps >= p.inTime + p.holdTime) from = k;
                if (a > 0) last = k;
            }
            return from < 0 ? -1 : (last - from + 1) * 1000.0 / fps;
        }
    }

    /// <summary>One run's round grades.</summary>
    public sealed class RoundGrader21
    {
        readonly EventBus events;
        readonly Grades21.Params p;
        readonly RunMoments.Grade bands;
        readonly Payoff.P rule;
        readonly int topRank;
        readonly Func<int> rank;
        readonly Func<double> maxHp;
        readonly Func<bool> revived;
        readonly double dt;

        /// <summary>The run's rows are on (a run's start sets it; off: nothing is counted or said).</summary>
        public bool on;
        bool round;
        int peak, kills, number;
        double damage;
        int steps;

        /// <summary>The run's round grades, in round order (RESULTS' strip).</summary>
        public readonly List<RoundGradeEvent> Rounds = new List<RoundGradeEvent>();

        /// <summary>The last round's grade (null: none yet this run).</summary>
        public RoundGradeEvent Last => Rounds.Count > 0 ? Rounds[Rounds.Count - 1] : null;

        public RoundGrader21(GameConfig cfg, EventBus events, Func<int> rank, Func<double> maxHp, Func<bool> revived)
        {
            this.events = events;
            this.rank = rank;
            this.maxHp = maxHp;
            this.revived = revived;
            p = Grades21.Params.From(cfg);
            bands = RunMoments.Grade.From(cfg);
            rule = Payoff.P.From(cfg);
            topRank = cfg.Arr("style.ranks").Count - 1;
            dt = cfg.Dt;
            events.On<RoundEvent>("roundBegin", OnBegin);
            events.On<HurtEvent>("hurt", OnHurt);
            events.On<KillEvent>("kill", OnKill);
            events.On<RoundEvent>("roundClear", OnClear);
        }

        /// <summary>A new run: the row on or off; no rounds, nothing counting.</summary>
        public void NewRun(bool row)
        {
            on = row;
            round = false;
            peak = kills = number = steps = 0;
            damage = 0;
            Rounds.Clear();
        }

        void OnBegin(RoundEvent e)
        {
            if (!on) return;
            round = true;
            number = e.number;
            peak = rank();
            kills = steps = 0;
            damage = 0;
        }

        void OnHurt(HurtEvent h)
        {
            if (on && round && h.owner == "player") damage += h.amount;
        }

        void OnKill(KillEvent k)
        {
            if (on && round && (k.owner == null || k.owner == "player")) kills++;
        }

        /// <summary>One fixed step: the round's clock and the best rank it reached.</summary>
        public void Step()
        {
            if (!on || !round) return;
            steps++;
            int r = rank();
            if (r > peak) peak = r;
        }

        void OnClear(RoundEvent e)
        {
            if (!on || !round) return;
            round = false;
            double time = steps * dt;
            double score = Grades21.Score(damage, maxHp(), peak, topRank, time, kills, p);
            string letter = Grades21.Letter(score, bands);
            bool isP = Grades21.IsRoundP(letter, peak, revived(), rule);
            var g = new RoundGradeEvent
            {
                round = number, letter = letter, grade = isP ? "P" : letter, p = isP, score = score, damage = damage,
                time = time, peakRank = peak, kills = kills,
            };
            Rounds.Add(g);
            events.Emit(Grades21.EventName, g);
        }
    }
}
