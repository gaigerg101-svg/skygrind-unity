// NIGHT56 builder G (G4, THE KILN WYRM's tail; design.n55.wyrm55): n47human's wyrm STATS policy (the H47Policy hook: no pilot change, it only
// reads): per fight, the player's hurt (HP, hits) and the worm's own hit counters (Wyrm.stats: contactsBody / contactsHead / contactsLunge,
// shotsHit, aggroHits; Wyrm31: sweepHits, spitHits), copied every step; the line "wyrm56 stats after N fights" at each fight's START covers the
// fights before it (n47human ends a fight's loop on the step the worm dies, so no policy step sees it dead): a chunk's last fight is not in it.
using System;
using System.Collections.Generic;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static readonly string[] W56Keys = { "contactsBody", "contactsHead", "contactsLunge", "shotsHit", "aggroHits", "aggros", "coreOpens", "lunges", "dives", "squeezes" };

    sealed class W56Rec
    {
        public double hurt;
        public int hurtHits;
        public readonly Dictionary<string, int> stat = new Dictionary<string, int>();
        public bool hooked;
    }

    static readonly List<W56Rec> W56Recs = new List<W56Rec>();

    static void H47PolicyWyrm56Stats(H47Ctx ctx, ref InputFrame move, ref CombatInput combat)
    {
        if (ctx.boss != "wyrm") return;
        SurvivalSim sim = ctx.sim;
        if (!ctx.state.TryGetValue("w56rec", out object o))
        {
            if (W56Recs.Count > 0) W56Print();
            o = new W56Rec();
            ctx.state["w56rec"] = o;
            W56Recs.Add((W56Rec)o);
        }
        var r = (W56Rec)o;
        if (!r.hooked)
        {
            r.hooked = true;
            sim.events.On<HurtEvent>("hurt", h => { if (h.owner == "player") { r.hurt += h.amount; r.hurtHits++; } });
        }
        Wyrm w = sim.wyrm24;
        if (w != null) foreach (string k in W56Keys) r.stat[k] = w.stats.TryGetValue(k, out int v) ? v : 0;
        if (sim.wyrm31 != null) { r.stat["sweepHits"] = sim.wyrm31.sweepHits; r.stat["spitHits"] = sim.wyrm31.spitHits; }
    }

    static void W56Print()
    {
        var sum = new Dictionary<string, int>();
        foreach (W56Rec r in W56Recs) foreach (var kv in r.stat) sum[kv.Key] = (sum.TryGetValue(kv.Key, out int v) ? v : 0) + kv.Value;
        Console.WriteLine($"  wyrm56 stats after {W56Recs.Count} fights: hurt {W56Recs.Sum(x => x.hurt):0} HP in {W56Recs.Sum(x => x.hurtHits)} hits; the worm's counters " +
            string.Join(", ", sum.Select(kv => $"{kv.Key} {kv.Value}")));
    }
}
