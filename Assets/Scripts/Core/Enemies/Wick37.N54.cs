// NIGHT54 builder W (wick54, sim row; NIGHT54.md row W, GG 2026-10-20 ~14:05: "the first boss fight ... is honestly much harder than it should
// be, so that entire encounter needs to be adjusted to make the fight relatively easy while keeping all the attacks exactly the same except for
// the one where it covers half the arena before becoming vulnerable—remove that attack entirely and just cycle through the others").
// Behind the wick54 bit (the sim sets `w54` and `p54` before StartFight; off: every line here is skipped, NIGHT53's fight bit for bit).
//   NO FLARE      the FLARE (wickFlare: the ring of fire on the half of the shaft wall the player is on) leaves the brain's list in every leg;
//                 wick43's cap flare cannot bring it back (SetFlareLeg3 is a no-op with the row on). The pick cycles through the rest.
//   EASIER        the ENCOUNTER, never an attack: every attack keeps its id, kind, wind-up / active / recover, cost, weight, legs, habit, damage,
//                 knock and radius. What changes (design.n54.wick54.numbers, each with its _why; a key left out keeps NIGHT53's value): the pool,
//                 the chip outside windows (wick47's chip), the phase gates (wick47's gate), how many crack-open punches P3 lasts (wick47's
//                 charge.punches: the charge itself unchanged), the brain's rest per leg (restByLeg) / burst / budget caps / window / stagger meter
//                 (design.n37.boss37 + wick37.kit), the retry's health cost. Applied to this fight's own copies (the brain's params are new each
//                 fight; p47 is copied, never edited: the sim caches it across runs).
// Pure (no UnityEngine), whole steps, no randomness here (the pick keeps the brain's Mulberry32), no allocation per step.
using System;

namespace Skygrind.Core
{
    /// <summary>design.n54.wick54.numbers (each number's _why there). NaN / null: the key is absent (NIGHT53's value stays).</summary>
    public sealed class Wick54Params
    {
        public double pool = double.NaN, chip = double.NaN, restartCost = double.NaN;
        public int punches = -1;
        public double[] gate;
        public double restSeconds = double.NaN, budgetRestSeconds = double.NaN, windowSeconds = double.NaN, legChangeSeconds = double.NaN, restartRestSeconds = double.NaN;
        public int burst = -1;
        public double[] budgetCap, stagger, restByLeg;

        public const string Row = "design.n54.wick54.numbers.";

        public static Wick54Params From(GameConfig c)
        {
            double N(string k) => c.Has(Row + k) ? c.Num(Row + k) : double.NaN;
            double[] A(string k) => c.Has(Row + k) ? c.Nums(Row + k) : null;
            return new Wick54Params
            {
                pool = N("pool"), chip = N("chip"), restartCost = N("restartCost"), gate = A("gate"), punches = c.Has(Row + "punches") ? c.Int(Row + "punches") : -1,
                restSeconds = N("brain.restSeconds"), budgetRestSeconds = N("brain.budgetRestSeconds"), windowSeconds = N("brain.windowSeconds"),
                legChangeSeconds = N("brain.legChangeSeconds"), restartRestSeconds = N("brain.restartRestSeconds"),
                burst = c.Has(Row + "brain.burst") ? c.Int(Row + "brain.burst") : -1,
                budgetCap = A("brain.budgetCap"), stagger = A("brain.stagger"), restByLeg = A("brain.restByLeg"),
            };
        }
    }

    public sealed partial class Wick37
    {
        /// <summary>wick54 on for this fight (the sim sets it, and p54, before StartFight).</summary>
        public bool w54;
        public Wick54Params p54;
        /// <summary>Counts for the harness and the reels: FLARE tells and burns this fight (0 with the row on), every tell by attack id.</summary>
        public int flareTells54, flareBurns54;

        /// <summary>The pool this fight starts with (wick54's when its row sets one).</summary>
        double Pool54 => w54 && p54 != null && !double.IsNaN(p54.pool) ? p54.pool : p.pool;

        /// <summary>The retry's health cost (wick54's when its row sets one).</summary>
        double RestartCost54 => w54 && p54 != null && !double.IsNaN(p54.restartCost) ? p54.restartCost : p.restartCost;

        void StartFight54()
        {
            flareTells54 = flareBurns54 = 0;
            if (!w54 || p54 == null || brain == null) return;
            // NO FLARE: out of the list in every leg (the cap flare's re-arm finds nothing to arm)
            brain.attacks.RemoveAll(IsFlare54);
            // the brain's encounter numbers (this fight's own Boss37Params: From() made it new in StartFight)
            Boss37Params bp = brain.p;
            if (!double.IsNaN(p54.restSeconds)) bp.restSeconds = p54.restSeconds;
            if (!double.IsNaN(p54.budgetRestSeconds)) bp.budgetRestSeconds = p54.budgetRestSeconds;
            if (!double.IsNaN(p54.windowSeconds)) bp.windowSeconds = p54.windowSeconds;
            if (!double.IsNaN(p54.legChangeSeconds)) bp.legChangeSeconds = p54.legChangeSeconds;
            if (!double.IsNaN(p54.restartRestSeconds)) bp.restartRestSeconds = p54.restartRestSeconds;
            if (p54.burst >= 1) bp.burst = p54.burst;
            if (p54.budgetCap != null && p54.budgetCap.Length > 0) bp.budgetCap = p54.budgetCap;
            if (p54.stagger != null && p54.stagger.Length > 0) { bp.stagger = p54.stagger; brain.stagger = bp.StaggerOf(brain.leg); }
            // wick47's chip and gates: on a copy (the sim's Wick47Params is cached across runs; the row off must read NIGHT53's)
            if (w47 && p47 != null && (!double.IsNaN(p54.chip) || p54.gate != null || p54.punches >= 1))
            {
                Wick47Params c = p47.Copy54();
                if (!double.IsNaN(p54.chip)) c.chip = p54.chip;
                if (p54.punches >= 1) c.punches = p54.punches; // P3 lasts this many crack-open punches (the charge itself unchanged)
                if (p54.gate != null && p54.gate.Length > 0) c.gate = p54.gate;
                p47 = c;
                hpLock = p47.gate.Length > 0 ? p47.gate[0] : hpLock;
            }
        }

        static bool IsFlare54(Boss37Attack a) => a.id == Flare;

        /// <summary>Each step before the brain's: the rest of the leg the fight is in (restByLeg; the brain reads restSeconds when it rests).</summary>
        void Step54()
        {
            if (p54 == null || p54.restByLeg == null || p54.restByLeg.Length == 0 || brain == null) return;
            brain.p.restSeconds = p54.restByLeg[Math.Max(1, Math.Min(p54.restByLeg.Length, brain.leg)) - 1];
        }
    }

    public sealed partial class Wick47Params
    {
        /// <summary>NIGHT54 W (wick54): a copy for one fight (the arrays are replaced, never edited, by the copy's owner).</summary>
        public Wick47Params Copy54() => (Wick47Params)MemberwiseClone();
    }
}
