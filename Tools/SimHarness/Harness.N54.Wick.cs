// NIGHT54 builder W (wick54): n47human's WICK stats line (the H47Policy hook; Harness.N47.Human.cs's header: read-only here, it changes no input).
// Per run it copies THE WICK's counters each step; when the last seed's fight ends it prints one line (caps: no line): the FLARE count (tells, burns: 0 with wick54 on),
// the attacks picked by id, when P2 / P3 began (s from the fight's begin), the charge's crack-opens and core punches (the finish), and the
// player's damage by source (hits per attack, hot-wall burns, slag touches, retries).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class W54Rec
    {
        public bool w54, done;
        public int flareTells, flareBurns, p2At = -1, p3At = -1, opens, charges, corePunches, ember, flare, slam, dive, geyser, ring, charge, hot, slag, restarts, lastLeg = 1;
        public readonly Dictionary<string, int> picks = new Dictionary<string, int>();
    }

    static readonly List<W54Rec> W54Recs = new List<W54Rec>();
    static bool W54Hooked;

    static void H47PolicyWick54Stats(H47Ctx ctx, ref InputFrame move, ref CombatInput combat)
    {
        if (ctx.boss != "wick") return;
        Wick37 w = ctx.sim.wick37;
        if (w == null || w.brain == null) return;
        if (!W54Hooked) { W54Hooked = true; W54Seeds = int.TryParse(H47Arg(Environment.GetCommandLineArgs(), "--seeds", "40"), out int sd) ? sd : 40; }
        if (!ctx.state.TryGetValue("w54rec", out object o)) { o = new W54Rec(); ctx.state["w54rec"] = o; W54Recs.Add((W54Rec)o); }
        var r = (W54Rec)o;
        if (r.done) return;
        r.w54 = w.w54;
        r.flareTells = w.flareTells54;
        r.flareBurns = w.flareBurns54;
        if (w.brain.leg != r.lastLeg) { if (w.brain.leg == 2 && r.p2At < 0) r.p2At = ctx.step; if (w.brain.leg == 3 && r.p3At < 0) r.p3At = ctx.step; r.lastLeg = w.brain.leg; }
        r.opens = w.opens47; r.charges = w.charges47; r.corePunches = w.corePunches47;
        r.ember = w.emberHits; r.flare = w.flareHits; r.slam = w.slamHits; r.dive = w.diveHits; r.geyser = w.geyserHits47; r.ring = w.ringHits47; r.charge = w.chargeHits47;
        r.hot = ctx.sim.wick37HotBurns; r.slag = ctx.sim.wick37SlagTouches; r.restarts = w.restarts;
        foreach (Boss37Attack a in w.brain.attacks) r.picks[a.id] = a.picks;
        // the fight's end (the finisher's DYING: the loop still steps it): this run's record is final; the last run prints the line (a
        // process-exit hook never fired: n47human exits from its module initializer)
        if (w.brain.state == Boss37State.Dying || w.brain.state == Boss37State.Dead || !w.Alive)
        {
            r.done = true;
            if (W54Recs.Count(x => x.done) == W54Seeds) W54Print();
        }
    }

    static int W54Seeds = 40;

    static void W54Print()
    {
        if (W54Recs.Count == 0) return;
        CultureInfo ci = CultureInfo.InvariantCulture;
        double dt = 1.0 / 120;
        string Med(IEnumerable<int> xs)
        {
            var l = xs.Where(x => x >= 0).OrderBy(x => x).ToList();
            if (l.Count == 0) return "n/a";
            double m = l.Count % 2 == 1 ? l[l.Count / 2] : (l[l.Count / 2 - 1] + l[l.Count / 2]) * 0.5;
            return (m * dt).ToString("0.0", ci) + " s";
        }
        var ids = W54Recs.SelectMany(x => x.picks.Keys).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToList();
        string picks = string.Join(", ", ids.Select(id => id + " " + W54Recs.Sum(x => x.picks.TryGetValue(id, out int n) ? n : 0)));
        int n = W54Recs.Count;
        Console.WriteLine($"wick54 stats ({n} runs, wick54 {(W54Recs.All(x => x.w54) ? "ON" : W54Recs.Any(x => x.w54) ? "MIXED" : "OFF")}): flares {W54Recs.Sum(x => x.flareTells)} (tells; burns {W54Recs.Sum(x => x.flareBurns)}); picks [{picks}]; " +
            $"P2 reached {W54Recs.Count(x => x.p2At >= 0)}/{n} (median at {Med(W54Recs.Select(x => x.p2At))}), P3 reached {W54Recs.Count(x => x.p3At >= 0)}/{n} (median at {Med(W54Recs.Select(x => x.p3At))}); " +
            $"charges {W54Recs.Sum(x => x.charges)}, crack-opens {W54Recs.Sum(x => x.opens)}, core punches {W54Recs.Sum(x => x.corePunches)} (runs with a punch {W54Recs.Count(x => x.corePunches > 0)}/{n}); " +
            $"player hits by source (totals): ember {W54Recs.Sum(x => x.ember)}, flare {W54Recs.Sum(x => x.flare)}, slam {W54Recs.Sum(x => x.slam)}, dive {W54Recs.Sum(x => x.dive)}, geyser {W54Recs.Sum(x => x.geyser)}, ring {W54Recs.Sum(x => x.ring)}, charge {W54Recs.Sum(x => x.charge)}, hot wall {W54Recs.Sum(x => x.hot)}, slag {W54Recs.Sum(x => x.slag)}, retries {W54Recs.Sum(x => x.restarts)}");
    }
}
