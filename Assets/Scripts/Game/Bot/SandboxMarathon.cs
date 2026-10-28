// NIGHT20 U3-34.2 (the lead owns this file; builders never edit it): THE MARATHON. The free-play farm (normal, the
// AUTOPILOT, every run as Farm.RunOne plays it: a fresh composition, the bot's own shopping and drafts, the farm's time cap)
// read for balance instead of guns. Per run it records, from the bus and the health each step:
// - the round reached and how the run ended (death / exfil / timeout); a death's round and its seconds into that round;
// - each round's start HP and the HP at its clear (fractions of max);
// - every HP gained, split by when it came: in a round's fight (in-round: passive regen, earned heals, parries, perks) or in
//   a break (the round-end refill, and any regen that finishes there); the HP lost (the bus's `hurt` on the player).
// The rows (MARATHON.md, Harness.Climb.cs): M-1 the median round reached; M-2 deaths by band (1-5 / 6-10 / 11+); M-3 the
// median start HP of rounds 2+; M-4 the in-round heal / damage taken; R1 the median HP at a clear (rounds 5-15); R2 the
// break heal's share of all HP gained; R3 the deaths in a round's first quarter (seconds into the round under a quarter of
// that round number's median clear time over the batch). Deterministic: a seed plays the same run.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class SandboxMarathon
    {
        public sealed class Run
        {
            public uint seed;
            public int round;
            public string end; // death | exfil | timeout
            public int deathRound = -1;
            public double deathInRound = -1; // s from that round's start
            public readonly List<double> startHp = new List<double>(); // per round from 1 (fraction of max)
            public readonly List<double> clearHp = new List<double>(); // per cleared round (fraction of max)
            public readonly List<int> clearRound = new List<int>();
            public readonly List<double> clearTime = new List<double>(); // s from the round's start to its clear
            public double healFight, healBreak, hurt;
            public double simTime;

            public string ToJson()
            {
                var sb = new StringBuilder();
                string F(double v) => double.IsNaN(v) || double.IsInfinity(v) ? "null" : v.ToString("0.####", CultureInfo.InvariantCulture);
                sb.Append("{\"seed\":").Append(seed).Append(",\"round\":").Append(round).Append(",\"end\":\"").Append(end).Append('"');
                sb.Append(",\"deathRound\":").Append(deathRound).Append(",\"deathInRound\":").Append(F(deathInRound));
                sb.Append(",\"healFight\":").Append(F(healFight)).Append(",\"healBreak\":").Append(F(healBreak)).Append(",\"hurt\":").Append(F(hurt));
                sb.Append(",\"simTime\":").Append(F(simTime));
                sb.Append(",\"startHp\":[");
                for (int i = 0; i < startHp.Count; i++) sb.Append(i > 0 ? "," : "").Append(F(startHp[i]));
                sb.Append("],\"clears\":[");
                for (int i = 0; i < clearRound.Count; i++) sb.Append(i > 0 ? "," : "").Append('[').Append(clearRound[i]).Append(',').Append(F(clearHp[i])).Append(',').Append(F(clearTime[i])).Append(']');
                sb.Append("]}");
                return sb.ToString();
            }
        }

        /// <summary>One run: the farm's normal free play from `seed` (Farm.RunOne's loop), measured.</summary>
        public static Run RunOne(GameConfig cfg, SurvivalMap map, uint seed, double maxSeconds)
        {
            if (BotNav.CacheDir == null) BotNav.CacheDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "skygrind-botnav");
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.TellTheTruth(); // NIGHT20 round 2: builder P's honest bot (P0-2: the HOLD ring by the game's own test, the straight approach)
            ap.ExfilFrom = 0; // ... without the exfil call: THE MARATHON measures depth
            sim.UseInput(ap);
            var r = new Run { seed = seed };
            double now = 0, roundStart = -1;
            int curRound = 0;
            sim.events.On<RoundEvent>("roundBegin", e =>
            {
                curRound = e.number;
                roundStart = now;
                r.startHp.Add(sim.health.max > 0 ? sim.health.hp / sim.health.max : 0);
            });
            sim.events.On<RoundEvent>("roundClear", e =>
            {
                r.clearRound.Add(e.number);
                r.clearHp.Add(sim.health.max > 0 ? sim.health.hp / sim.health.max : 0);
                r.clearTime.Add(roundStart >= 0 ? now - roundStart : -1);
            });
            sim.events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner == "player") r.hurt += e.amount;
            });
            sim.events.On<DeathEvent>("death", e =>
            {
                if (e.owner != "player" || r.deathRound >= 0) return;
                r.deathRound = curRound;
                r.deathInRound = roundStart >= 0 ? now - roundStart : -1;
            });
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(seed, null, null);
            int maxSteps = MovementMath.StepsFor(maxSeconds, sim.step);
            int steps = 0;
            double lastHp = sim.health.hp, lastMax = sim.health.max;
            while (steps < maxSteps && sim.flow != Flow.Results)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                steps++;
                now += sim.step;
                double hp = sim.health.hp;
                // A max change (a perk) moves hp with it: count only the gain at the same max.
                if (sim.health.max == lastMax && hp > lastHp)
                {
                    if (sim.run.state == SurvivalRun.State.Fight) r.healFight += hp - lastHp;
                    else r.healBreak += hp - lastHp;
                }
                lastHp = hp;
                lastMax = sim.health.max;
            }
            bool timeout = sim.flow != Flow.Results;
            if (timeout) sim.EndRun();
            RunStats st = sim.lastStats;
            r.round = st?.round ?? sim.run.round;
            r.simTime = now;
            r.end = timeout ? "timeout" : st?.exfil == Exfil.Win ? "exfil" : r.deathRound >= 0 ? "death" : st?.exfil != null ? "exfil failed" : "other";
            ap.Detach();
            return r;
        }

        static double Median(List<double> v) => v.Count == 0 ? double.NaN : Sandbox.Median(v);

        /// <summary>MARATHON.md's rows from the runs (and the JSON summary).</summary>
        public static string Report(List<Run> runs, string tag, string side, out string json)
        {
            var rounds = new List<double>();
            int deaths = 0, d1 = 0, d2 = 0, d3 = 0;
            var start = new List<double>();
            var clear515 = new List<double>();
            double healFight = 0, healBreak = 0, hurt = 0;
            var clearTimes = new Dictionary<int, List<double>>();
            foreach (Run r in runs)
            {
                rounds.Add(r.round);
                for (int i = 1; i < r.startHp.Count; i++) start.Add(r.startHp[i]); // rounds 2+
                for (int i = 0; i < r.clearRound.Count; i++)
                {
                    if (r.clearRound[i] >= 5 && r.clearRound[i] <= 15) clear515.Add(r.clearHp[i]);
                    if (r.clearTime[i] >= 0)
                    {
                        if (!clearTimes.TryGetValue(r.clearRound[i], out var l)) clearTimes[r.clearRound[i]] = l = new List<double>();
                        l.Add(r.clearTime[i]);
                    }
                }
                healFight += r.healFight;
                healBreak += r.healBreak;
                hurt += r.hurt;
                if (r.deathRound >= 0)
                {
                    deaths++;
                    if (r.deathRound <= 5) d1++;
                    else if (r.deathRound <= 10) d2++;
                    else d3++;
                }
            }
            int early = 0, earlyOf = 0;
            foreach (Run r in runs)
            {
                if (r.deathRound < 0 || r.deathInRound < 0) continue;
                if (!clearTimes.TryGetValue(r.deathRound, out var l) || l.Count == 0) continue;
                earlyOf++;
                if (r.deathInRound < 0.25 * Sandbox.Median(l)) early++;
            }
            double m1 = Median(rounds), m3 = Median(start), m4 = hurt > 0 ? healFight / hurt : double.NaN;
            double r1 = Median(clear515), r2 = healFight + healBreak > 0 ? healBreak / (healFight + healBreak) : double.NaN;
            double r3 = earlyOf > 0 ? (double)early / earlyOf : double.NaN;
            double maxBand = deaths > 0 ? Math.Max(d1, Math.Max(d2, d3)) / (double)deaths : double.NaN;
            string P(double v) => double.IsNaN(v) ? "n/a" : (100 * v).ToString("0.0", CultureInfo.InvariantCulture) + " %";
            string N(double v) => double.IsNaN(v) ? "n/a" : v.ToString("0.0#", CultureInfo.InvariantCulture);
            var md = new StringBuilder();
            md.Append($"# THE MARATHON ({tag}, {side})\n\n{Sandbox.Version}; the free-play farm (normal), {runs.Count} runs (seeds {(runs.Count > 0 ? runs[0].seed : 0)}..); NIGHT19's median round 9.0 is the reference (M-1).\n\n");
            md.Append("| Row | Value | Target |\n| --- | --- | --- |\n");
            md.Append($"| M-1 the median round reached | {N(m1)} ({N(m1 / 9.0)} x NIGHT19's 9.0) | 0.8-1.3 x (7.2-11.7) |\n");
            md.Append($"| M-2 deaths by band (1-5 / 6-10 / 11+) | {d1} / {d2} / {d3} of {deaths} (the largest band {P(maxBand)}) | no band > 50 % |\n");
            md.Append($"| M-3 the median HP at a round's start (rounds 2+) | {P(m3)} | >= 90 % |\n");
            md.Append($"| M-4 the in-round heal / damage taken | {N(m4)} ({N(healFight)} HP of {N(hurt)}) | 0.15-0.4 (inferred) |\n");
            md.Append($"| R1 the median HP at a round's clear (rounds 5-15) | {P(r1)} ({clear515.Count} clears) | 35-70 % |\n");
            md.Append($"| R2 the break heal's share of all HP gained | {P(r2)} ({N(healBreak)} of {N(healFight + healBreak)} HP) | >= 50 % |\n");
            md.Append($"| R3 deaths in a round's first quarter | {P(r3)} ({early} of {earlyOf}) | <= 25 % |\n");
            var ends = new SortedDictionary<string, int>(StringComparer.Ordinal);
            foreach (Run r in runs) ends[r.end] = (ends.TryGetValue(r.end, out int k) ? k : 0) + 1;
            md.Append("\nHow the runs ended: ");
            bool first = true;
            foreach (var kv in ends)
            {
                md.Append(first ? "" : ", ").Append(kv.Key).Append(' ').Append(kv.Value);
                first = false;
            }
            md.Append(".\n");
            string J(double v) => double.IsNaN(v) ? "null" : v.ToString("0.####", CultureInfo.InvariantCulture);
            json = "{\"version\":" + Sandbox.Q(Sandbox.Version) + ",\"side\":" + Sandbox.Q(side) + ",\"runs\":" + runs.Count + ",\"m1\":" + J(m1) + ",\"deaths\":[" + d1 + "," + d2 + "," + d3 + "],\"m3\":" + J(m3) + ",\"m4\":" + J(m4) + ",\"r1\":" + J(r1) + ",\"r2\":" + J(r2) + ",\"r3\":" + J(r3) + "}";
            return md.ToString();
        }
    }
}
