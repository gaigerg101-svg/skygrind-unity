// NIGHT55 builder B (wick55, sim row; slate P1-wickWindow; critic-veteran #1: "THE WICK is a damage race, not a read-the-tell, punish-the-window
// fight"; DECIDE55 D-7: encounter numbers only). Behind the wick55 bit (the sim sets `w55` and `p55` before StartFight; off: every line here is
// skipped, NIGHT54's fight bit for bit). On top of wick54 (which stays as it ships: no FLARE, the pool, the gates, the punch, the rests).
//   THE WINDOW PAYS   outside a window a hit lands `chip` of itself (wick47's chip, wick54's 0.6: the steady fire was 94 % of the damage);
//                     the stagger WINDOW (the meter emptied by parries and pierces) lasts windowSeconds and a hit inside it lands windowGain x
//                     full; the meter's size per leg (stagger) sets how often it opens.
//   THE HOT WALL      (SurvivalSim.N55.Wick.cs) the touch's burn: its cooldown and whether a brush in the air burns or only pushes.
// Every attack keeps its id, kind, wind-up / active / recover, cost, weight, legs, habit, damage, knock and radius (Wick55Tests pins them).
// Pure (no UnityEngine), whole steps, no randomness here, no allocation per step.
using System;

namespace Skygrind.Core
{
    /// <summary>design.n55.wick55.numbers (each number's _why there). NaN / null: the key is absent (NIGHT54's value stays).</summary>
    public sealed class Wick55Params
    {
        public double chip = double.NaN, windowSeconds = double.NaN, windowGain = double.NaN, hotCooldown = double.NaN, hotDamage = double.NaN;
        public bool hotBrushBurns = true;
        public double[] stagger;

        public const string Row = "design.n55.wick55.numbers.";

        public static Wick55Params From(GameConfig c)
        {
            double N(string k) => c.Has(Row + k) ? c.Num(Row + k) : double.NaN;
            return new Wick55Params
            {
                chip = N("chip"), windowSeconds = N("windowSeconds"), windowGain = N("windowGain"), hotCooldown = N("hot.cooldown"), hotDamage = N("hot.damage"),
                hotBrushBurns = !c.Has(Row + "hot.brushBurns") || c.Bool(Row + "hot.brushBurns"),
                stagger = c.Has(Row + "stagger") ? c.Nums(Row + "stagger") : null,
            };
        }
    }

    public sealed partial class Wick37
    {
        /// <summary>wick55 on for this fight (the sim sets it, and p55, before StartFight).</summary>
        public bool w55;
        public Wick55Params p55;
        /// <summary>Counts for the harness and the reels: hits landed inside the stagger window, and the damage they dealt (after windowGain).</summary>
        public int windowHits55;
        public double windowDealt55;

        /// <summary>From StartFight, after StartFight54: this fight's window, meter and chip.</summary>
        void StartFight55()
        {
            windowHits55 = 0;
            windowDealt55 = 0;
            if (!w55 || p55 == null || brain == null) return;
            Boss37Params bp = brain.p; // this fight's own copy (From() made it new in StartFight)
            if (!double.IsNaN(p55.windowSeconds)) bp.windowSeconds = p55.windowSeconds;
            if (p55.stagger != null && p55.stagger.Length > 0) { bp.stagger = p55.stagger; brain.stagger = bp.StaggerOf(brain.leg); }
            if (w47 && p47 != null && !double.IsNaN(p55.chip))
            {
                Wick47Params c = p47.Copy54(); // a copy (p47 may be wick54's copy already; the sim's cached one is never edited)
                c.chip = p55.chip;
                p47 = c;
            }
        }

        /// <summary>THE BAR TELLS THE TRUTH (B-1): with the row on, the core bare after the PLATE RING (bare47) or cracked open by the CHARGE
        /// (coreOpen47) takes every hit in full (Filter47), so the bar reads OPEN then too (Open43) and shows its time (CrackLeftMax). False / 0 off.</summary>
        bool Core55 => w55 && (coreOpen47 > 0 || bare47 > 0);
        int CoreLeft55 => w55 ? Math.Max(coreOpen47, bare47) : 0;

        /// <summary>A hit's share inside the stagger window (Filter's window line): x windowGain with the row on; `share` unchanged off.</summary>
        double Window55(double share)
        {
            if (!w55 || p55 == null || brain == null || !brain.Open) return share;
            double s = double.IsNaN(p55.windowGain) ? share : share * p55.windowGain;
            windowHits55++;
            windowDealt55 += s;
            return s;
        }
    }
}
