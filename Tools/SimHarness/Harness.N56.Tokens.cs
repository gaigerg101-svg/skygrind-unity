// NIGHT56 builder G (G3 tokens56's meter; NIGHT56.md 5 G3): the enemy columns of `n46flow`, fed from the style column's hooks (Harness.N56.Style.cs:
// F56StyleBegin / F56StyleStep / F56StyleTable call F56Tok*; no new call-site line in Harness.N46.Flow.cs). In the measured rooms' fights (the
// same in-fight steps as stall % and the style column):
//   attack starts  every `enemyCommit` (a wind-up, a pounce, a spit's tell, a charge, a drone's charge): inside the SIM cone or not. The cone is
//                  the sim's own look (SurvivalSim.Aim(): input.Yaw / Pitch, the tape's look) with the half angle camera.fovDeg / 2 (74 / 2 = 37 deg)
//                  round it, from the sim's eye to the enemy's aim point: never the player's FOV setting (rule 4). Per kind.
//   ranged hits    every spitter volley (`spitterVolley`) and drone shot (`enemyShot` from a drone): the player's horizontal speed at the shot,
//                  bucketed (stand < 2, run 2-10, fast 10-18, dash >= 18 m/s; design dash.speed 24), and whether it hurt the player: a `hurt` of
//                  source "spitter" / "drone" within 2.5 s goes to the oldest open shot of that source (one hit per shot). Hit rate per bucket.
//   idle in place  per enemy alive in a fight (no boss, not a stationary kind: the lancer blinks, never walks): its steps with a horizontal speed
//                  under 0.5 m/s while not in an attack (committed, a wind-up) over its alive steps; the pooled share, the median enemy and the
//                  share of enemies idle more than 20 % of their time (enemies alive >= 2 s).
// Reads only (no sim state is written): the flow numbers stay equal.
using System;
using System.Collections.Generic;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static readonly string[] F56SpeedNames = { "stand", "run", "fast", "dash" };
    static double F56CosHalf = -2; // cos(camera.fovDeg / 2), read once per level
    static readonly Dictionary<string, int[]> f56Starts = new Dictionary<string, int[]>(); // kind -> [all, in cone]
    static readonly int[] f56Shots = new int[4], f56ShotHits = new int[4];
    static readonly List<(int step, string src, int bucket, bool hit)> f56Open = new List<(int, string, int, bool)>();
    static readonly Dictionary<int, int[]> f56Idle = new Dictionary<int, int[]>(); // enemy id (this seed) -> [alive, idle]
    static readonly List<double> f56IdleShares = new List<double>();
    static long f56IdleAll, f56AliveAll;
    static readonly Dictionary<string, long[]> f56IdleKind = new Dictionary<string, long[]>(); // kind -> [alive, idle] steps
    static SurvivalSim f56TokSim;
    static long f56Waits, f56Advances; // tokens56's counters (brain.coneWaits56 / advances56), summed over the seeds
    static int f56Room = -1; // the measured room of the last in-fight step

    /// <summary>The handlers count only while the last measured room's fight runs (its campaign phase Fight, the player not down).</summary>
    static bool F56Live(SurvivalSim sim) => sim.campaign != null && sim.campaign.phase == Campaign24.Phase.Fight && sim.campaign.room == f56Room && !sim.health.Dead;

    static void F56TokBegin()
    {
        f56Starts.Clear();
        Array.Clear(f56Shots, 0, 4); Array.Clear(f56ShotHits, 0, 4);
        f56Open.Clear(); f56Idle.Clear(); f56IdleShares.Clear();
        f56IdleAll = f56AliveAll = 0;
        f56Waits = f56Advances = 0;
        f56IdleKind.Clear();
        f56TokSim = null;
        F56CosHalf = -2;
    }

    /// <summary>Is the enemy's aim point inside the sim cone (the sim's look, half angle fovDeg / 2)?</summary>
    static bool F56InCone(SurvivalSim sim, Enemy e)
    {
        if (F56CosHalf < -1) F56CosHalf = Math.Cos(sim.cfg.Num("camera.fovDeg") * 0.5 * Math.PI / 180);
        (Vec3 eye, Vec3 look) = sim.Aim();
        Vec3 at = e.AimPoint;
        double dx = at.x - eye.x, dy = at.y - eye.y, dz = at.z - eye.z, len = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        if (len < 1e-6) return true;
        return (dx * look.x + dy * look.y + dz * look.z) / len >= F56CosHalf;
    }

    static int F56Bucket(double v) => v < 2 ? 0 : v < 10 ? 1 : v < 18 ? 2 : 3;

    static Enemy F56Find(SurvivalSim sim, int id)
    {
        foreach (Enemy e in sim.enemies.all) if (e.id == id) return e;
        return null;
    }

    /// <summary>The seed's end (a new sim): the open enemies' idle shares go to the pool.</summary>
    static void F56TokFlush()
    {
        if (f56TokSim?.brain != null) { f56Waits += f56TokSim.brain.coneWaits56; f56Advances += f56TokSim.brain.advances56; f56TokSim.brain.coneWaits56 = f56TokSim.brain.advances56 = 0; }
        foreach (int[] a in f56Idle.Values)
            if (a[0] >= 240) f56IdleShares.Add((double)a[1] / a[0]);
        f56Idle.Clear();
        f56Open.Clear();
    }

    static void F56TokHook(SurvivalSim sim)
    {
        if (ReferenceEquals(sim, f56TokSim)) return;
        F56TokFlush();
        f56TokSim = sim;
        sim.events.On("enemyCommit", o =>
        {
            if (!F56Live(sim) || !(o is int id)) return;
            Enemy e = F56Find(sim, id);
            if (e == null) return;
            if (!f56Starts.TryGetValue(e.kind, out int[] a)) f56Starts[e.kind] = a = new int[2];
            a[0]++;
            if (F56InCone(sim, e)) a[1]++;
        });
        void Shot(string src)
        {
            double v = Math.Sqrt(sim.mover.velocity.x * sim.mover.velocity.x + sim.mover.velocity.z * sim.mover.velocity.z);
            int b = F56Bucket(v);
            f56Shots[b]++;
            f56Open.Add((sim.simStep, src, b, false));
        }
        sim.events.On("spitterVolley", o => { if (F56Live(sim)) Shot("spitter"); });
        sim.events.On("enemyShot", o => { if (F56Live(sim) && o is int id && F56Find(sim, id) is Enemy e && e.kind == "drone") Shot("drone"); });
        sim.events.On<HurtEvent>("hurt", h =>
        {
            if (h.owner != "player" || (h.source != "spitter" && h.source != "drone")) return;
            int win = MovementMath.StepsFor(2.5, sim.step);
            for (int i = 0; i < f56Open.Count; i++)
            {
                var s = f56Open[i];
                if (s.hit || s.src != h.source || sim.simStep - s.step > win) continue;
                f56Open[i] = (s.step, s.src, s.bucket, true);
                f56ShotHits[s.bucket]++;
                break;
            }
        });
    }

    /// <summary>One in-fight step (F56StyleStep): the idle count; the handlers count only on these steps.</summary>
    static void F56TokStep(int room, SurvivalSim sim)
    {
        F56TokHook(sim);
        F57PressStep(sim); // NIGHT57 G press57's PRESS57 line (Harness.N57.Press.cs; silent with the row off)
        f56Room = room;
        int win = MovementMath.StepsFor(2.5, sim.step);
        if (f56Open.Count > 0 && sim.simStep - f56Open[0].step > win) f56Open.RemoveAll(s => sim.simStep - s.step > win);
        foreach (Enemy e in sim.enemies.all)
        {
            if (!e.alive || e.standIn29 || e.kind == "lancer" || e.kind == "burrower" || !(e is Husk h)) continue; // the lancer blinks, the burrower waits under the floor: both by design
            if (!f56Idle.TryGetValue(e.id, out int[] a)) f56Idle[e.id] = a = new int[2];
            a[0]++;
            f56AliveAll++;
            if (!f56IdleKind.TryGetValue(e.kind, out long[] ik)) f56IdleKind[e.kind] = ik = new long[2];
            ik[0]++;
            double v = Math.Sqrt(e.velocity.x * e.velocity.x + e.velocity.z * e.velocity.z);
            if (v < 0.5 && !h.committed && e.windupSteps <= 0) { a[1]++; f56IdleAll++; ik[1]++; if (e.kind == "drone") F57DroneIdle(sim, h); }
        }
    }

    static string F56Pct(long a, long b) => b == 0 ? "-" : (100.0 * a / b).ToString("0.0", F1Inv);

    static void F56TokTable(string id, Action<string> Line)
    {
        F56TokFlush();
        int all = f56Starts.Values.Sum(a => a[0]), cone = f56Starts.Values.Sum(a => a[1]);
        string kinds = string.Join(", ", f56Starts.OrderByDescending(kv => kv.Value[0]).Select(kv => $"{kv.Key} {kv.Value[1]}/{kv.Value[0]}"));
        Line($"n46flow {id} TOKENS: attack starts inside the sim cone (camera.fovDeg {f56TokSim?.cfg.Num("camera.fovDeg") ?? 0:0} / 2) {cone}/{all} = {F56Pct(cone, all)} % (by kind: {kinds}); tokens56 thinks waited out of view {f56Waits}, advances {f56Advances}");
        string hits = string.Join(", ", Enumerable.Range(0, 4).Select(b => $"{F56SpeedNames[b]} {f56ShotHits[b]}/{f56Shots[b]} = {F56Pct(f56ShotHits[b], f56Shots[b])} %"));
        double standRate = f56Shots[0] > 0 ? (double)f56ShotHits[0] / f56Shots[0] : double.NaN, dashRate = f56Shots[3] > 0 ? (double)f56ShotHits[3] / f56Shots[3] : double.NaN;
        double fastRate = f56Shots[2] + f56Shots[3] > 0 ? (double)(f56ShotHits[2] + f56ShotHits[3]) / (f56Shots[2] + f56Shots[3]) : double.NaN;
        Line($"n46flow {id} RANGED: hit rate by the player's speed at the shot: {hits}; dash / stand {(standRate > 0 ? (dashRate / standRate * 100).ToString("0", F1Inv) + " %" : "-")}, fast+dash / stand {(standRate > 0 ? (fastRate / standRate * 100).ToString("0", F1Inv) + " %" : "-")}");
        var sh = f56IdleShares.OrderBy(x => x).ToList();
        string med = sh.Count > 0 ? (sh[sh.Count / 2] * 100).ToString("0.0", F1Inv) : "-", p90 = sh.Count > 0 ? (sh[Math.Min(sh.Count - 1, (int)(0.9 * sh.Count))] * 100).ToString("0.0", F1Inv) : "-";
        int over = sh.Count(x => x > 0.2);
        Line($"n46flow {id} IDLE: idle in place {F56Pct(f56IdleAll, f56AliveAll)} % of enemy-alive steps; per enemy (alive >= 2 s, {sh.Count}): median {med} %, p90 {p90} %, over 20 %: {over} ({F56Pct(over, sh.Count)} %); by kind: {string.Join(", ", f56IdleKind.OrderByDescending(kv => kv.Value[0]).Select(kv => kv.Key + " " + F56Pct(kv.Value[1], kv.Value[0]) + " %"))}");
        F57PressTable(id, Line); // NIGHT57 G press57 (Harness.N57.Press.cs; no line with the row off)
        F56TokBegin();
        f56Room = -1;
    }
}
