// Port of skygrind/src/style/styleCore.js (SPEC 2.0 §7, OVERNIGHT2 N1 pacing), NIGHT9 U3-07/U3-08: the style meter's
// scoring core. Pure: no three.js, no DOM, no event bus, so the live meter (StyleMeter, styleMeter.js wires the bus into
// it) and the headless pace simulation (StyleSim, styleSim.js) run exactly the same rules:
// - named awards for kills, hits, parries, tricks, cross combos and swap kills, x the state multiplier (airborne,
//   sliding, wall riding), x the weapon's freshness, x the aim-assist payout;
// - hit awards pay once per weapon shot or swing (not per pellet, nail, round or projectile), and all hit-award
//   income is capped at a rate (a refilling bucket);
// - per-source diminishing returns: the same award from the same source within 4 s pays x0.6 per repeat (floor
//   x0.15), recovering as the repeats age out, so punch, parry and one-weapon spam stop paying quickly;
// - uneven, rising rank thresholds; drain rises with the rank; the shown rank drops only after a delay.
// Golden vectors: PortExport/vectors/styleCore.json (a session of operations, the state compared after each).
// Unity-only, off = the web's core (NIGHT8): PERFECT PARRY (design.parry, U2-07) and the LAB's FRESHNESS TEETH and
// STYLE RAMP (LabMath, U2-16).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class StyleCore : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the meter and the shown rank with its delay, the totals and the best award,
        // the breakdown, freshness, the trick clocks, the drain and FLOW's numbers, the kill times, the clock, the
        // diminishing-return windows, the hit-income bucket, the last paid shot per weapon, the stats, and the LAB's
        // switches (Lab.Apply; its numbers come back from config when a switch is on).
        public void Save(StateWriter w)
        {
            w.B("enabled", enabled);
            w.D("meter", meter);
            w.I("shownRank", shownRank);
            w.D("shownBelow", shownBelow);
            w.D("total", total);
            w.I("bestRank", bestRank);
            w.S("bestName", bestName);
            w.D("bestPoints", bestPoints);
            w.SD("breakdown", breakdown);
            w.SD("fresh", fresh);
            w.SD("tricks", tricks);
            w.D("drainMult", drainMult);
            w.D("perkDrain", perkDrain);
            w.D("perkGain", perkGain);
            w.Doubles("killTimes", killTimes);
            w.D("time", time);
            w.Begin("dim");
            w.Strings("k", new List<string>(dim.Keys));
            int i = 0;
            foreach (List<double> times in dim.Values) w.Doubles((i++).ToString(System.Globalization.CultureInfo.InvariantCulture), times);
            w.End();
            w.D("tokens", tokens);
            w.SD("lastShot", lastShot);
            w.Ints("stats", new[] { statAwards, statCapped, statDimmed, statSameShot });
            w.B("labFreshness", labFreshness);
            w.B("labRamp", labRamp);
            w.I("inStateSteps", inStateSteps);
        }

        public void Load(StateReader r)
        {
            List<int> stats = r.Ints("stats");
            if (stats.Count != 4) throw new StateException("snapshot: style.stats");
            StateReader d = r.Section("dim");
            List<string> dk = d.Strings("k");
            var dv = new List<List<double>>(dk.Count);
            for (int i = 0; i < dk.Count; i++) dv.Add(d.Doubles(i.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            if (new HashSet<string>(dk).Count != dk.Count) throw new StateException("snapshot: style.dim keys");
            var f = new Dictionary<string, double>();
            r.SD("fresh", f);
            enabled = r.B("enabled");
            meter = r.D("meter");
            shownRank = r.Int("shownRank");
            shownBelow = r.D("shownBelow");
            total = r.D("total");
            bestRank = r.Int("bestRank");
            bestName = r.S("bestName");
            bestPoints = r.D("bestPoints");
            r.SD("breakdown", breakdown);
            fresh = f;
            r.SD("tricks", tricks);
            drainMult = r.D("drainMult");
            perkDrain = r.D("perkDrain");
            perkGain = r.D("perkGain");
            killTimes.Clear();
            killTimes.AddRange(r.Doubles("killTimes"));
            time = r.D("time");
            dim.Clear();
            for (int i = 0; i < dk.Count; i++) dim.Add(dk[i], dv[i]);
            tokens = r.D("tokens");
            r.SD("lastShot", lastShot);
            statAwards = stats[0];
            statCapped = stats[1];
            statDimmed = stats[2];
            statSameShot = stats[3];
            labFreshness = r.B("labFreshness");
            labRamp = r.B("labRamp");
            inStateSteps = r.Int("inStateSteps");
            if ((labFreshness || labRamp) && lab == null && s.c != null) lab = LabMath.P.From(s.c);
        }

        readonly StyleMath.S s;
        readonly Action<string, double, bool> onAward;
        readonly Action<int, bool> onRank;

        // --- st ---
        public bool enabled;
        public double meter;
        int shownRank;
        double shownBelow;
        public double total; // the run's style points (the score)
        public int bestRank;
        public string bestName = ""; // the biggest single award
        public double bestPoints;
        public readonly Dictionary<string, double> breakdown = new Dictionary<string, double>(); // name -> points, for the end screen
        public Dictionary<string, double> fresh;
        readonly Dictionary<string, double> tricks = new Dictionary<string, double>(); // trick -> last time (s)
        public double drainMult = 1; // x drain (EXPOSED: x2, SURVIVAL S5)
        public double perkDrain = 1; // x drain (the FLOW perk: 0.6, POWER P6)
        public double perkGain = 1; // x meter gain (FLOW: 1.2)
        /// <summary>NIGHT34 B (design.n34.air34, Core/Air34.cs): while on, every award pays x mult34 (the sim sets it each step) and the web's flat
        /// state multiplier is 1 (one movement multiplier, not two). Off: false and 1, NIGHT33's meter.</summary>
        public bool air34;
        public double mult34 = 1;
        readonly List<double> killTimes = new List<double>(); // recent kill times (s)
        public double time; // s of simulated time
        readonly Dictionary<string, List<double>> dim = new Dictionary<string, List<double>>(); // diminishing key -> earlier award times
        double tokens; // hit-award income (the bucket)
        readonly Dictionary<string, double> lastShot = new Dictionary<string, double>(); // weapon -> the last shot id whose hit awards were paid

        // --- stats ---
        public int statAwards, statCapped, statDimmed, statSameShot;

        // --- Unity-only (NIGHT8): design.parry's PERFECT PARRY award, the LAB (set at a run's start by Lab.Apply) ---
        readonly double perfectPoints;
        public LabMath.P lab;
        public bool labFreshness, labRamp;
        public int rampSteps, inStateSteps;

        /// <param name="onAward">(name, points, quiet)</param>
        /// <param name="onRank">(rank, up)</param>
        public StyleCore(StyleMath.S s, Action<string, double, bool> onAward = null, Action<int, bool> onRank = null)
        {
            this.s = s;
            this.onAward = onAward ?? ((n, p, q) => { });
            this.onRank = onRank ?? ((r, u) => { });
            fresh = StyleMath.CreateFreshness(s);
            tokens = s.hitCapBurst;
            GameConfig c = s.c;
            perfectPoints = c != null && c.Has("design.parry.perfectPoints") ? c.Num("design.parry.perfectPoints") : s.baseP["parry"];
            if (c != null && c.Has("design.lab.styleRamp.rampTime")) rampSteps = MovementMath.StepsFor(c.Num("design.lab.styleRamp.rampTime"), c.Dt);
        }

        public StyleMath.S Params => s;

        public int Rank => shownRank;

        /// <summary>Put `points` on the meter (after FLOW's gain), keep the totals, report it.</summary>
        double Pay(string name, double points, bool quiet)
        {
            if (!enabled) return 0;
            if (air34 && mult34 != 1 && points > 0) points = JsMath.Round(points * mult34); // NIGHT34 B air34: the movement multiplier on the award
            if (points <= 0)
            {
                if (!quiet) onAward(name, 0, quiet);
                return 0;
            }
            meter = JsMath.Min(StyleMath.MeterMax(s), meter + points * perkGain);
            total += points;
            breakdown[name] = (breakdown.TryGetValue(name, out double b) ? b : 0) + points;
            if (points > bestPoints)
            {
                bestName = name;
                bestPoints = points;
            }
            statAwards++;
            onAward(name, points, quiet);
            return points;
        }

        /// <summary>The diminishing multiplier for an award from `weapon` (per source), counted now.</summary>
        double DimFor(string name, string weapon)
        {
            string key = StyleMath.DiminishKey(name, weapon);
            if (!dim.TryGetValue(key, out List<double> times)) dim[key] = times = new List<double>();
            double k = StyleMath.Diminish(times, time, s.dimWindow, s.dimRepeat, s.dimFloor);
            if (k < 1) statDimmed++;
            return k;
        }

        /// <summary>A weapon's freshness multiplier: the arsenal's FRESH (0..1, POWER P3) when given, else the meter's own.</summary>
        double FreshFor(string weapon, double? arsenalFresh)
        {
            if (arsenalFresh.HasValue) return FreshMult(arsenalFresh.Value * s.max);
            string key = StyleMath.FreshKey(weapon);
            return key != null ? FreshMult(fresh[key]) : 1;
        }

        /// <summary>freshnessTier(value).mult, or the LAB's FRESHNESS TEETH (Unity-only).</summary>
        double FreshMult(double value) => labFreshness ? LabMath.FreshnessMult(value, s, lab) : StyleMath.FreshnessMult(value, s);

        /// <summary>stateMultiplier(state), or the LAB's STYLE RAMP (Unity-only).</summary>
        double StateMult(string state) => air34 ? 1 : labRamp ? LabMath.RampMult(StyleMath.IsMultState(state), inStateSteps, rampSteps, s, lab) : StyleMath.StateMultiplier(state, s);

        /// <summary>Awards from one weapon: x state, x freshness, x the assist payout, x diminishing (per source).</summary>
        double WeaponAwards(List<StyleMath.Award> list, string weapon, string state, double assist, double? arsenalFresh, bool isHit)
        {
            double fm = FreshFor(weapon, arsenalFresh);
            double mult = StateMult(state) * assist;
            double paid = 0;
            foreach (StyleMath.Award a in list)
            {
                double points = StyleMath.ApplyMultipliers(a.points * DimFor(a.name, weapon), mult, fm);
                if (isHit)
                {
                    double before = points;
                    points = StyleMath.TakeHitIncome(ref tokens, points);
                    if (points < before) statCapped++;
                }
                paid += Pay(a.name, points, a.quiet);
            }
            return paid;
        }

        /// <summary>Start scoring (a run or match); resets everything.</summary>
        public void Start()
        {
            enabled = true;
            meter = 0;
            shownRank = 0;
            shownBelow = 0;
            total = 0;
            bestRank = 0;
            bestName = "";
            bestPoints = 0;
            breakdown.Clear();
            fresh = StyleMath.CreateFreshness(s);
            tricks.Clear();
            killTimes.Clear();
            time = 0;
            tokens = s.hitCapBurst;
            dim.Clear();
            lastShot.Clear();
            statAwards = statCapped = statDimmed = statSameShot = 0;
            inStateSteps = 0;
        }

        public void Stop() => enabled = false;

        /// <summary>A kill. assist: the payout multiplier (1 = aimed); fresh: the arsenal's FRESH (null: the meter's own).</summary>
        public double Kill(string kind, string weapon, string state, double distance, bool melee = false, bool bounced = false, double assist = 1, double? arsenalFresh = null)
        {
            int n = 0;
            for (int i = 0; i < killTimes.Count; i++) if (time - killTimes[i] <= s.multikillWindow) killTimes[n++] = killTimes[i];
            killTimes.RemoveRange(n, killTimes.Count - n);
            killTimes.Add(time);
            var list = StyleMath.KillAwards(kind, weapon, state, distance, killTimes.Count, melee, bounced, s);
            return WeaponAwards(list, weapon, state, assist, arsenalFresh, false);
        }

        /// <summary>
        /// A hit (a killing hit too). Hit awards pay once per shot / swing: `shot` is the id of the trigger pull that made
        /// it (0 = none: secondary damage such as burns, which pay through the cap and the diminishing only).
        /// </summary>
        public double Hit(string weapon, string state, bool headshot = false, double pointScale = 1, bool melee = false, double shot = 0, double assist = 1, double? arsenalFresh = null)
        {
            if (shot > 0)
            {
                if (lastShot.TryGetValue(weapon, out double last) && last == shot)
                {
                    statSameShot++;
                    return 0;
                }
                lastShot[weapon] = shot;
            }
            return WeaponAwards(StyleMath.HitAwards(weapon, headshot, pointScale, melee, s), weapon, state, assist, arsenalFresh, true);
        }

        /// <summary>A parry (the punch's source: F is the Gauntlet). Unity-only: a PERFECT one pays design.parry.perfectPoints.</summary>
        public double Parry(string state, double? arsenalFresh = null, bool perfect = false)
        {
            var award = perfect ? new StyleMath.Award { name = "PERFECT PARRY", points = perfectPoints } : new StyleMath.Award { name = "PARRY", points = s.baseP["parry"] };
            return WeaponAwards(new List<StyleMath.Award> { award }, "punch", state, 1, arsenalFresh, false);
        }

        /// <summary>A movement trick: scores only with an enemy near, not again within the repeat window, diminishing too.</summary>
        public double Trick(string name, string label, bool enemyNear, string state)
        {
            double points = StyleMath.TrickPoints(name, time, tricks, enemyNear, s);
            if (points <= 0) return Pay(label, 0, false);
            return Pay(label, StyleMath.ApplyMultipliers(points * DimFor(label, null), StateMult(state), 1), false);
        }

        /// <summary>A cross-family hit or a named combo (CROSS, COLLAPSE, OVERLOAD: POWER P3).</summary>
        public double Cross(string combo, string weapon, string state)
        {
            string name = combo ?? "CROSS";
            double b = s.bonus.TryGetValue(name.ToLowerInvariant(), out double v) ? v : s.bonus["cross"];
            return Pay(name, StyleMath.ApplyMultipliers(b * DimFor(name, weapon), StateMult(state), 1), false);
        }

        /// <summary>A kill soon after a swap (SWAP, POWER P3).</summary>
        public double SwapKill(string weapon, string state) =>
            Pay("SWAP", StyleMath.ApplyMultipliers(s.bonus["swap"] * DimFor("SWAP", weapon), StateMult(state), 1), false);

        /// <summary>A named bonus with its own base (IMPACT, TRAPPED, ...): x state, diminishing per source.</summary>
        public double Bonus(string name, double b, string weapon = null, string state = "GROUND", double assist = 1) =>
            Pay(name, StyleMath.ApplyMultipliers(b * DimFor(name, weapon), StateMult(state) * assist, 1), false);

        /// <summary>The meter's own freshness: a use of a weapon (fire events; slams, punches).</summary>
        public void Use(string weapon, double scale = 1) => StyleMath.UseFreshness(fresh, StyleMath.FreshKey(weapon), s, scale);

        public void UseKey(string key) => StyleMath.UseFreshness(fresh, key, s);

        /// <summary>Taking damage costs meter.</summary>
        public void Hurt()
        {
            if (enabled) meter = JsMath.Max(0, meter - s.damagePenalty);
        }

        /// <summary>x drain (camping doubles it).</summary>
        public void SetDrainMult(double value) => drainMult = value;

        /// <summary>The FLOW perk (POWER P6): x drain, x meter gain (null: off).</summary>
        public void SetPerkMods(double? drain, double? gain)
        {
            perkDrain = drain ?? 1;
            perkGain = gain ?? 1;
        }

        /// <summary>Debug: the meter in the middle of a rank, shown at once.</summary>
        public void DebugSetRank(int rank)
        {
            int r = (int)JsMath.Max(0, JsMath.Min(s.ranks.Count - 1, rank));
            meter = JsMath.Min(StyleMath.MeterMax(s), StyleMath.RankMid(r, s));
            shownRank = r;
            shownBelow = 0;
        }

        /// <summary>Debug: put the meter at a value (the shown rank follows on the next step).</summary>
        public void DebugSetMeter(double value) => meter = JsMath.Max(0, JsMath.Min(StyleMath.MeterMax(s), value));

        /// <summary>Unity-only (the LAB's STYLE RAMP): count the steps spent in a multiplier state; any other resets it.</summary>
        public void RampState(string state) => inStateSteps = StyleMath.IsMultState(state) ? inStateSteps + 1 : 0;

        /// <summary>One fixed step: freshness regen, the hit-income refill, drain, the shown rank.</summary>
        public void Step(double dt)
        {
            if (!enabled) return;
            time += dt;
            StyleMath.RegenFreshness(fresh, dt, s);
            tokens = StyleMath.RefillHitIncome(tokens, dt, s.hitCapBurst, s.hitCapPerSecond);
            int before = shownRank;
            meter = JsMath.Max(0, meter - StyleMath.DrainRate(before, s) * drainMult * perkDrain * dt);
            int rank = StyleMath.StepShownRank(ref shownRank, ref shownBelow, meter, dt, s);
            if (rank != before) onRank(rank, rank > before);
            bestRank = Math.Max(bestRank, rank);
        }

        /// <summary>The HUD numbers: the fill of the shown rank's band.</summary>
        public double Fill() => StyleMath.RankFill(meter, shownRank, s);

        /// <summary>The meter's freshness tier per weapon, written into `output`.</summary>
        public Dictionary<string, string> FreshTiers(Dictionary<string, string> output)
        {
            foreach (var kv in fresh) output[kv.Key] = StyleMath.FreshnessTierName(kv.Value, s);
            return output;
        }
    }
}
