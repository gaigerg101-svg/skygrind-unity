// NIGHT56 builder G (G1 rank56's meter; NIGHT56.md 5 G1, section 4's last bullet: "a style column for n46flow ... baselined with every n56 row
// off"): the style column of `n46flow`. Three call-site lines in Harness.N46.Flow.cs (F1Level): F56StyleBegin after the pools, F56StyleStep
// beside `r.fight++` (the in-fight steps, the player not down: the same steps stall % and air % count), F56StyleTable after the level's SUMMARY.
// Per fight room (pooled over the seeds) and per level:
//   rank share   the share of in-fight steps at each shown rank (StyleMeter.rank: DULL .. SKYGRIND), DULL first
//   top rank     the highest rank shown during the room's fight, per seed: median / max over the seeds that entered the room
// A room the pilot did not clear is counted too (its steps are combat time). The numbers are read, never written: the column changes no sim
// state (the fingerprint and every older column are unchanged).
using System;
using System.Collections.Generic;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    const int F56Ranks = 6; // style.ranks: DULL, COOL, BOLD, AIRBORNE, SAVAGE, SKYGRIND
    static readonly string[] F56RankNames = { "DULL", "COOL", "BOLD", "AIRBORNE", "SAVAGE", "SKYGRIND" };
    static long[,] f56Steps; // [room, rank]
    static int[,] f56Top; // [room, seed]: the highest rank shown in the room's fight (-1: not in its fight that seed)
    static double f56Dt; // the sim's step (s)

    /// <summary>A level's start (F1Level, after the pools).</summary>
    static void F56StyleBegin(int rooms, int seeds)
    {
        f56Steps = new long[rooms, F56Ranks];
        f56Top = new int[rooms, Math.Max(1, seeds)];
        for (int i = 0; i < rooms; i++) for (int s = 0; s < seeds; s++) f56Top[i, s] = -1;
        F56TokBegin(); // G3: the enemy columns (Harness.N56.Tokens.cs)
    }

    // the payoffs by the rank shown when they land (every seed's sim; subscribed once per sim): heal36's heals and RIDE KILL's refunds
    static SurvivalSim f56Sim;
    static readonly int[] f56Heals = new int[F56Ranks], f56Refunds = new int[F56Ranks];
    static readonly double[] f56Healed = new double[F56Ranks], f56Refunded = new double[F56Ranks];

    static int F56RankOf(SurvivalSim sim) => sim.style == null ? 0 : Math.Max(0, Math.Min(F56Ranks - 1, sim.style.rank));

    static void F56Hook(SurvivalSim sim)
    {
        if (ReferenceEquals(sim, f56Sim)) return;
        f56Sim = sim;
        sim.events.On<Heal36Event>(Heal36.Event, e => { int k = F56RankOf(sim); f56Heals[k]++; f56Healed[k] += e.amount; });
        sim.events.On<RideKill53Event>(RideKill53.Event, e => { int k = F56RankOf(sim); f56Refunds[k]++; f56Refunded[k] += e.after - e.before; });
    }

    /// <summary>One in-fight step of room `room` on seed index `s` (beside r.fight++).</summary>
    static void F56StyleStep(int room, int s, SurvivalSim sim)
    {
        if (f56Steps == null || sim.style == null) return;
        F56Hook(sim);
        F56TokStep(room, sim); // G3: the enemy columns
        f56Dt = sim.step;
        int k = Math.Max(0, Math.Min(F56Ranks - 1, sim.style.rank));
        f56Steps[room, k]++;
        if (s < f56Top.GetLength(1) && k > f56Top[room, s]) f56Top[room, s] = k;
    }

    static string F56Share(long a, long b) => b == 0 ? "-" : (100.0 * a / b).ToString("0.0", F1Inv);

    /// <summary>The level's style table (after its SUMMARY line): one row per measured room, then LEVEL, then one summary line.</summary>
    static void F56StyleTable(string id, LevelDef level, bool[] measured, Action<string> Line)
    {
        if (f56Steps == null) return;
        int n = level.rooms.Count, seeds = f56Top.GetLength(1);
        Line($"  style      kind | {string.Join(" ", F56RankNames.Select(r => r.Substring(0, Math.Min(4, r.Length)).PadLeft(5)))} (% of fight steps) | top rank med / max (seeds)");
        var lvl = new long[F56Ranks];
        int levelTop = -1, roomsSavage = 0, roomsMeasured = 0;
        for (int i = 0; i < n; i++)
        {
            if (!measured[i]) continue;
            long tot = 0;
            for (int k = 0; k < F56Ranks; k++) { tot += f56Steps[i, k]; lvl[k] += f56Steps[i, k]; }
            var tops = new List<int>();
            for (int s = 0; s < seeds; s++) if (f56Top[i, s] >= 0) tops.Add(f56Top[i, s]);
            tops.Sort();
            int med = tops.Count > 0 ? tops[tops.Count / 2] : -1, max = tops.Count > 0 ? tops[tops.Count - 1] : -1;
            if (max > levelTop) levelTop = max;
            roomsMeasured++;
            if (max >= 4) roomsSavage++;
            var cells = new string[F56Ranks];
            for (int k = 0; k < F56Ranks; k++) cells[k] = F56Share(f56Steps[i, k], tot).PadLeft(5);
            Line($"  {level.rooms[i].id,-10} {level.rooms[i].kind,4} | {string.Join(" ", cells)}                    | {(med >= 0 ? F56RankNames[med] : "-")} / {(max >= 0 ? F56RankNames[max] : "-")} ({tops.Count})");
        }
        long all = lvl.Sum();
        var lc = new string[F56Ranks];
        for (int k = 0; k < F56Ranks; k++) lc[k] = F56Share(lvl[k], all).PadLeft(5);
        Line($"  {"LEVEL",-10} {"",4} | {string.Join(" ", lc)}                    | top {(levelTop >= 0 ? F56RankNames[levelTop] : "-")}");
        Line($"n46flow {id} STYLE: DULL {F56Share(lvl[0], all)} % of {all * f56Dt:0} s of fight; above DULL {F56Share(all - lvl[0], all)} %; rooms reaching SAVAGE {roomsSavage}/{roomsMeasured}; top rank {(levelTop >= 0 ? F56RankNames[levelTop] : "-")}");
        // the payoffs (heals and refunds anywhere in the walk, fights or not, by the rank shown when each landed): heal per close kill / refund per ride kill
        string Pay(int[] cnt, double[] amt) => string.Join(", ", Enumerable.Range(0, F56Ranks).Select(k => $"{F56RankNames[k]} {(cnt[k] > 0 ? (amt[k] / cnt[k]).ToString("0.00", F1Inv) : "-")} ({cnt[k]})"));
        Line($"n46flow {id} PAYOFFS: heal36 HP per close kill by rank: {Pay(f56Heals, f56Healed)}; RIDE KILL refund per ride kill by rank: {Pay(f56Refunds, f56Refunded)}");
        Array.Clear(f56Heals, 0, F56Ranks); Array.Clear(f56Healed, 0, F56Ranks); Array.Clear(f56Refunds, 0, F56Ranks); Array.Clear(f56Refunded, 0, F56Ranks);
        F56TokTable(id, Line); // G3: the enemy columns
        f56Steps = null;
        f56Top = null;
    }
}
