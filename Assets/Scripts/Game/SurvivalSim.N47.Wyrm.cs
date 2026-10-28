// NIGHT47 builder B, row wyrm47 (sim; G31 / G34; Research/2026-10-18-wave47/B-cards.md section 4.1, the KILN WYRM card; DECIDE47 D-9).
// Called from SurvivalSim.N47.Brawl.cs's hooks. GG 2026-10-18: the worm "feels fine overall, but its attacks need clearer telegraphs ... and it
// should burrow like the previous version": the fight's core is kept; what changes here:
//   the tells (G31): each named tell of the worm (WyrmParams.tells: crack, aggro, spit, core, lunge, squeeze) and the tail sweep's
//      (Wyrm31Params.sweepTellSeconds) take design.n47.wyrm47.numbers' seconds (0.8-1.5 s; today aggro 0.5, crack / roar / spit / sweep 0.6:
//      n47b wyrm, 23:12), so each says where for long enough to answer;
//   the burrow (G34): the DIVE is no longer cut by a proximity charge (Wyrm.noAggroInDive47; today 13 of 28 cracks were cut and replayed) and,
//      with diveAfterLunge 1, the cycle gets a second DIVE after each LUNGE (coil, dive, lunge, dive, loop, finale: NIGHT24's dive twice a cycle).
// The worm is built in N24StartRun (after RowsStartRun), so the change is made on the first step that has a worm this sim has not changed yet,
// and written back at the next run's start (the params may be reused by the next fight). Off: nothing changed, nothing per step. No
// allocation per step (the saved copies are made once a fight). No UnityEngine: SimHarness compiles this file.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        const string Wyrm47Root = "design.n47.wyrm47.numbers.";
        static readonly string[] Wyrm47Tells = { "crack", "aggro", "spit", "core", "lunge", "squeeze" };
        bool w47On;
        Wyrm w47For;
        Dictionary<string, double> w47TellsWas;
        List<string> w47CycleWas;
        Wyrm31 w47SweepFor;
        double w47AggroCoolSet, w47AggroCoolWas, w47WindowCap, w47StagFrom, w47SpitWas, w47HeadWas;
        bool w47WasStag;
        double w47SweepWas, w47StaggerWas, w47StaggerSet = double.NaN, w47OpenWas, w47OpenSet = double.NaN;

        /// <summary>wyrm47 is on this run (views and harnesses read it).</summary>
        public bool Wyrm47On => w47On;

        void Wyrm47StartRun()
        {
            Wyrm47Restore();
            w47On = rows.On(idWyrm47);
        }

        /// <summary>Put back what an earlier run changed on its worm (the tells, the cycle, the charge rule) and on Wyrm31 (the sweep's tell).</summary>
        void Wyrm47Restore()
        {
            if (w47For != null)
            {
                foreach (var kv in w47TellsWas) w47For.p.tells[kv.Key] = kv.Value;
                w47For.p.cycle.Clear();
                w47For.p.cycle.AddRange(w47CycleWas);
                w47For.noAggroInDive47 = false;
                w47For.advanceAfterAggro47 = false;
                w47For.aggroLock47 = 0;
                w47For.coreBelowMult47 = 0;
                w47For.aggroLean47 = 0;
                w47For.coilCut47 = 0;
                w47For.p.spitDamage = w47SpitWas;
                w47For.p.contactHead = w47HeadWas;
                w47For.coreAboveShare47 = 0;
                if (w47AggroCoolSet > 0 && w47For.aggroCool31 == w47AggroCoolSet) w47For.aggroCool31 = w47AggroCoolWas;
                w47For = null;
            }
            if (w47SweepFor != null)
            {
                w47SweepFor.p.sweepTellSeconds = w47SweepWas;
                // the window levers are wyrm46's too (it writes them at its own StartRun, before this one): put ours back only where nobody
                // has written them since (else wyrm46's own value stands)
                if (w47SweepFor.p.staggerSeconds == w47StaggerSet) w47SweepFor.p.staggerSeconds = w47StaggerWas;
                if (w47SweepFor.open32 == w47OpenSet) w47SweepFor.open32 = w47OpenWas;
                w47SweepFor = null;
            }
        }

        /// <summary>The head-punch window (s before contact) SurvivalSim.Boss30.cs uses: wyrm47's punchWindowS while the row is on (> 0), else the
        /// design's (design.n30.wyrm30.windowSeconds). The lead's round-2 line: keep the charge's tell at 0.9 s and give the punch a matching width.</summary>
        double Wyrm47PunchWindow(double design)
        {
            if (!w47On || !cfg.Has(Wyrm47Root + "punchWindowS")) return design;
            double v = cfg.Num(Wyrm47Root + "punchWindowS");
            return v > 0 ? v : design;
        }

        /// <summary>The head-punch line (SurvivalSim.Boss30.cs): wyrm47's (sentence case, the punch glyph: F's COPY47 rules) while the row is on.</summary>
        string Wyrm47Line(string design) => w47On && cfg.Has(Wyrm47Root + "linePunch") ? cfg.Str(Wyrm47Root + "linePunch") : design;

        void Wyrm47Step()
        {
            if (!w47On) return;
            if (wyrm24 != null && wyrm24 != w47For)
            {
                Wyrm47Restore();
                Wyrm w = wyrm24;
                w47TellsWas = new Dictionary<string, double>(w.p.tells); // once a fight, not per step
                w47CycleWas = new List<string>(w.p.cycle);
                foreach (string t in Wyrm47Tells)
                    if (w.p.tells.ContainsKey(t) && cfg.Has(Wyrm47Root + t + "S")) w.p.tells[t] = cfg.Num(Wyrm47Root + t + "S");
                if (cfg.Num(Wyrm47Root + "diveAfterLunge") > 0 && w.p.patterns.ContainsKey("dive"))
                {
                    w.p.cycle.Clear();
                    foreach (string c in w47CycleWas)
                    {
                        w.p.cycle.Add(c);
                        if (c == "lunge") w.p.cycle.Add("dive");
                    }
                }
                w.noAggroInDive47 = cfg.Num(Wyrm47Root + "noAggroInDive") > 0;
                w.advanceAfterAggro47 = cfg.Num(Wyrm47Root + "advanceAfterAggro") > 0;
                w.aggroLock47 = cfg.Num(Wyrm47Root + "aggroLockS");
                w.coreBelowMult47 = cfg.Has(Wyrm47Root + "coreBelowMult") ? cfg.Num(Wyrm47Root + "coreBelowMult") : 0;
                w.aggroLean47 = cfg.Has(Wyrm47Root + "aggroLeanS") ? cfg.Num(Wyrm47Root + "aggroLeanS") : 0;
                w.coilCut47 = cfg.Has(Wyrm47Root + "coilCut") ? cfg.Num(Wyrm47Root + "coilCut") : 0;
                // the tail's killers (n47human --b47-tally: the spit's shots ~40 % of the damage, the head's contact ~30 %): cut their damage
                w47SpitWas = w.p.spitDamage;
                w47HeadWas = w.p.contactHead;
                if (cfg.Has(Wyrm47Root + "spitDamage")) w.p.spitDamage = cfg.Num(Wyrm47Root + "spitDamage");
                if (cfg.Has(Wyrm47Root + "contactHead")) w.p.contactHead = cfg.Num(Wyrm47Root + "contactHead");
                w.coreAboveShare47 = cfg.Has(Wyrm47Root + "coreAboveShare") ? cfg.Num(Wyrm47Root + "coreAboveShare") : 0;
                w47AggroCoolSet = cfg.Has(Wyrm47Root + "aggroCooldownS") ? cfg.Num(Wyrm47Root + "aggroCooldownS") : 0;
                w47WindowCap = cfg.Has(Wyrm47Root + "windowCap") ? cfg.Num(Wyrm47Root + "windowCap") : 0;
                w47WasStag = false;
                w47For = w;
            }
            // the seconds between the head's charges (each charge is a punch window; Wyrm.aggroCool31, which SurvivalSim.Boss31 hooks from
            // Wyrm31Params.aggroCooldown): 0 = left as it is; a compare per step (whoever hooks it later, ours stands while the row is on)
            if (w47For != null && w47AggroCoolSet > 0 && w47For.aggroCool31 >= 0 && w47For.aggroCool31 != w47AggroCoolSet)
            {
                w47AggroCoolWas = w47For.aggroCool31;
                w47For.aggroCool31 = w47AggroCoolSet;
            }
            if (wyrm31 != null && wyrm31 != w47SweepFor)
            {
                w47SweepFor = wyrm31;
                w47SweepWas = wyrm31.p.sweepTellSeconds;
                wyrm31.p.sweepTellSeconds = cfg.Num(Wyrm47Root + "sweepS");
                w47StaggerSet = cfg.Num(Wyrm47Root + "staggerS");
                w47OpenSet = cfg.Num(Wyrm47Root + "open");
                w47StaggerWas = wyrm31.p.staggerSeconds;
                w47OpenWas = wyrm31.open32;
            }
            // the window's share (the tail / the floor of the band): one STAGGER takes at most windowCap of the pool, then it ends
            // (event wyrm47WindowSpent): with 10 s windows x3 the fastest fights ended in 3 windows (n47human min 45.6 s)
            if (w47WindowCap > 0 && wyrm31 != null && wyrm24 != null && wyrm24.Alive)
            {
                bool stag = wyrm31.Staggered;
                if (stag && !w47WasStag) w47StagFrom = wyrm24.body.hp;
                if (stag && w47StagFrom - wyrm24.body.hp >= w47WindowCap * wyrm24.body.maxHp)
                {
                    wyrm31.EndStagger47();
                    stag = false;
                    events.Emit("wyrm47WindowSpent", null);
                }
                w47WasStag = stag;
            }
            if (w47SweepFor != null)
            {
                // the length lever (the lead's card-gate line: "if it grows, the lever is the longer windows, not removing tells"); wyrm46 writes
                // the same two levers once (its own first step): whoever wrote last, ours stand while the row is on (a compare per step, no allocation)
                if (w47SweepFor.p.staggerSeconds != w47StaggerSet) { w47StaggerWas = w47SweepFor.p.staggerSeconds; w47SweepFor.p.staggerSeconds = w47StaggerSet; }
                if (w47SweepFor.open32 != w47OpenSet) { w47OpenWas = w47SweepFor.open32; w47SweepFor.open32 = w47OpenSet; }
            }
        }
    }
}
