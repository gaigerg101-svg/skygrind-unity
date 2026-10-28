// NIGHT39 builder B1 (boss39; NIGHT39.md card B1, DECIDE39 D-1; GG 2026-10-13: "all bosses need more balanced health bars so each fight lasts
// around one to two minutes, maybe three if you're slow"; "the worm boss fight needs more health"). The B1 hooks of SurvivalSim.N39.cs.
// design.n39.boss39.<boss> holds per-boss overrides, read while the boss39 bit is on (N39Blocks; --off39 / -skygrindOff39: nothing here runs):
//   wick, stag, loom, crown (the NIGHT37 Boss37 bosses): the brain's own levers first (D-1: a longer fight is more stagger cycles and attacks,
//     not a sponge): stagger ([leg 1, 2, 3], the meter's full value), budgetCap ([leg 1, 2, 3]), windowSeconds, burst, restSeconds,
//     budgetRestSeconds; then hpMult (the fight's pool; THE CROWN's gemHp grows with it, so its gem count stays design.n37.crown37's).
//     THE LOOM's legs end when a window closes (its pool never gates the fight): also nailsToTear, tearDistM (a knee's health).
//     THE CROWN's health is its gems: also gems, legGemsLeft (the pool = gems x gemHp, as Crown37.StartFight sets it), torchGems.
//   wyrm (THE KILN WYRM, wyrm24), stag31 (level 6's old Stag, plan id "glacier"): hpMult only (their AI unchanged: GG on the worm; the worm's
//     phases are fractions of the pool, so they move with it).
// Where: each brain the boss builds (a new Boss37Brain at its fight's start for THE WICK and THE STAG; the one from the constructor for THE LOOM
// and THE CROWN) gets the brain overrides in the first step it exists (its Boss37Params object; the meter rescaled to the new full value); each
// new fight's body gets hpMult once, at full health, in the step it first lives (Bosses33's pattern; N39Step runs after N33BossStep and
// N37Step, so NIGHT29 / 32 / 33's cuts and NIGHT37's fight start are already in). design.n37.* and the worm's rows are not edited: the
// originals of every overridden Boss37Params / Crown37Params number come back at the next run's start.
// No Boss37.cs logic change (D-1), no AI change. Events: none new (bossHit / boss37* carry the numbers).
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>Harness only (Tools/SimHarness `boss39`): force the boss39 bit for every run this process starts (null: the run's own bits;
        /// --off39 wins). Never set by the game.</summary>
        public static bool? Force39Boss;

        /// <summary>Harness only (`boss39 ... --set boss.key=v[,v,v]`): a tuning probe's value in place of design.n39.boss39.&lt;boss&gt;.&lt;key&gt;.
        /// Null in the game.</summary>
        public static Dictionary<string, double[]> Probe39Boss;

        const string Boss39Root = "design.n39.boss39.";

        /// <summary>The bosses the row knows (its keys under design.n39.boss39).</summary>
        public static readonly string[] Boss39Names = { "wick", "stag", "wyrm", "loom", "crown", "stag31" };

        int b39Wick = -1, b39Stag = -1, b39Loom = -1, b39Crown = -1, b39Wyrm = -1, b39Stag31 = -1;
        Boss37Brain b39WickBrain, b39StagBrain, b39LoomBrain, b39CrownBrain;
        readonly List<System.Action> b39Undo = new List<System.Action>();

        /// <summary>The harness's counts this run: brains given the overrides, fight pools scaled.</summary>
        public int boss39Brains, boss39Pools;

        bool B39Has(string boss, string key) =>
            Probe39Boss != null && Probe39Boss.ContainsKey(boss + "." + key) || cfg.Has(Boss39Root + boss + "." + key);

        double[] B39Nums(string boss, string key)
        {
            if (Probe39Boss != null && Probe39Boss.TryGetValue(boss + "." + key, out double[] v)) return (double[])v.Clone();
            object o = cfg.Get(Boss39Root + boss + "." + key);
            return o is List<object> ? cfg.Nums(Boss39Root + boss + "." + key) : new[] { cfg.Num(Boss39Root + boss + "." + key) };
        }

        double B39Num(string boss, string key) => B39Nums(boss, key)[0];

        /// <summary>Harness only (`boss39 --p39`, the lead's ask: each band with P1's p39 on too): force the p39 bit as well; P1's start hook
        /// runs again with it (it only sets charge35's p39 fields from the bit). Never set by the game; --off39 wins.</summary>
        public static bool Force39Pistol;

        /// <summary>Harness only: runs started with Force39Pistol, and those where the KESTREL's p39 rule was live (charge35's p39 set).</summary>
        public static int p39ForcedRuns, p39LiveRuns;

        partial void N39BossStartRun()
        {
            if (Force39Boss.HasValue && !N39Blocks.ForceOff) n39.boss39 = Force39Boss.Value;
            if (Force39Pistol && !N39Blocks.ForceOff && !n39.p39) { n39.p39 = true; N39PistolStartRun(); }
            if (Force39Pistol) { p39ForcedRuns++; if (charge35 != null && charge35.p39) p39LiveRuns++; }
            // the originals back (a row that was on in this sim's last run, or a boss object kept across runs)
            for (int i = b39Undo.Count - 1; i >= 0; i--) b39Undo[i]();
            b39Undo.Clear();
            b39Wick = b39Stag = b39Loom = b39Crown = b39Wyrm = b39Stag31 = -1;
            b39WickBrain = b39StagBrain = b39LoomBrain = b39CrownBrain = null;
            boss39Brains = boss39Pools = 0;
        }

        partial void N39BossStep(double dt)
        {
            if (!n39.boss39) return;
            if (wick37 != null)
            {
                if (wick37.brain != null && !ReferenceEquals(wick37.brain, b39WickBrain)) { b39WickBrain = wick37.brain; Boss39Brain(wick37.brain, "wick"); }
                if (wick37.Alive && wick37.body.id != b39Wick) { b39Wick = wick37.body.id; Boss39Pool(wick37.body, "wick"); }
            }
            if (stag37 != null)
            {
                if (stag37.brain != null && !ReferenceEquals(stag37.brain, b39StagBrain)) { b39StagBrain = stag37.brain; Boss39Brain(stag37.brain, "stag"); }
                if (stag37.Alive && stag37.body.id != b39Stag) { b39Stag = stag37.body.id; Boss39Pool(stag37.body, "stag"); }
            }
            if (loom37 != null)
            {
                if (!ReferenceEquals(loom37.brain, b39LoomBrain)) { b39LoomBrain = loom37.brain; Boss39Brain(loom37.brain, "loom"); Boss39Knees(loom37.p); }
                if (loom37.Alive && loom37.body.id != b39Loom) { b39Loom = loom37.body.id; Boss39Pool(loom37.body, "loom"); }
            }
            if (crown37 != null)
            {
                if (!ReferenceEquals(crown37.brain, b39CrownBrain)) { b39CrownBrain = crown37.brain; Boss39Brain(crown37.brain, "crown"); }
                if (crown37.Alive && crown37.body.id != b39Crown)
                {
                    b39Crown = crown37.body.id;
                    Crown37Params cp = crown37.p;
                    Enemy body = crown37.body;
                    if (B39Has("crown", "torchGems"))
                    {
                        int torch = cp.torchGems;
                        b39Undo.Add(() => cp.torchGems = torch);
                        cp.torchGems = (int)B39Num("crown", "torchGems");
                    }
                    if (B39Has("crown", "gems") && body.hp >= body.maxHp)
                    {
                        // THE CROWN's health is its gems (a pass breaks passGems whole gems; the legs change at legGemsLeft): more gems = more
                        // passes through the ring, the fight's moving verb. The pool is gems x gemHp, as Crown37.StartFight sets it.
                        int gems = cp.gems;
                        double[] legGemsLeft = cp.legGemsLeft;
                        b39Undo.Add(() => { cp.gems = gems; cp.legGemsLeft = legGemsLeft; });
                        cp.gems = (int)B39Num("crown", "gems");
                        if (B39Has("crown", "legGemsLeft")) cp.legGemsLeft = B39Nums("crown", "legGemsLeft");
                        body.maxHp = cp.gems * cp.gemHp;
                        body.hp = body.maxHp;
                        boss39Pools++;
                    }
                    if (Boss39Pool(body, "crown") && cp.gems > 0)
                    {
                        // the gems keep their count: each gem holds its share of the larger pool (a pass still breaks whole gems)
                        double gemHp = cp.gemHp;
                        b39Undo.Add(() => cp.gemHp = gemHp);
                        cp.gemHp = body.maxHp / cp.gems;
                    }
                }
            }
            if (wyrm24 != null && wyrm24.Alive && wyrm24.body.id != b39Wyrm)
            {
                b39Wyrm = wyrm24.body.id;
                if (Boss39Pool(wyrm24.body, "wyrm") && wyrm31 != null && wyrm31.on32) wyrm31.maxHp32 = wyrm24.body.maxHp; // the one-hit cap's pool (N32BossStep sets it again next step)
            }
            if (stag31 != null && stag31.Alive && stag31.body.id != b39Stag31) { b39Stag31 = stag31.body.id; Boss39Pool(stag31.body, "stag31"); }
        }

        /// <summary>The brain's overrides on its own Boss37Params (the originals kept for the next run's start); the stagger meter rescaled to the
        /// new full value (in the reveal it is full: it becomes the new full).</summary>
        void Boss39Brain(Boss37Brain br, string boss)
        {
            Boss37Params p = br.p;
            double[] stagger = p.stagger, budgetCap = p.budgetCap;
            double windowSeconds = p.windowSeconds, restSeconds = p.restSeconds, budgetRestSeconds = p.budgetRestSeconds;
            int burst = p.burst;
            b39Undo.Add(() =>
            {
                p.stagger = stagger;
                p.budgetCap = budgetCap;
                p.windowSeconds = windowSeconds;
                p.restSeconds = restSeconds;
                p.budgetRestSeconds = budgetRestSeconds;
                p.burst = burst;
            });
            double fullBefore = br.StaggerFull;
            bool any = false;
            if (B39Has(boss, "stagger")) { p.stagger = B39Nums(boss, "stagger"); any = true; }
            if (B39Has(boss, "budgetCap")) { p.budgetCap = B39Nums(boss, "budgetCap"); any = true; }
            if (B39Has(boss, "windowSeconds")) { p.windowSeconds = B39Num(boss, "windowSeconds"); any = true; }
            if (B39Has(boss, "restSeconds")) { p.restSeconds = B39Num(boss, "restSeconds"); any = true; }
            if (B39Has(boss, "budgetRestSeconds")) { p.budgetRestSeconds = B39Num(boss, "budgetRestSeconds"); any = true; }
            if (B39Has(boss, "burst")) { p.burst = (int)B39Num(boss, "burst"); any = true; }
            double fullAfter = br.StaggerFull;
            if (fullBefore > 0 && fullAfter != fullBefore) br.stagger = br.stagger * fullAfter / fullBefore;
            if (any) boss39Brains++;
        }

        /// <summary>THE LOOM's knees (its legs are places: each leg ends when its one window closes, so its pool never gates the fight; a knee's
        /// nails are its health): nailsToTear, tearDistM over design.n37.loom37's (the originals kept for the next run's start).</summary>
        void Boss39Knees(Loom37Params lp)
        {
            int nails = lp.nailsToTear;
            double dist = lp.tearDistM;
            b39Undo.Add(() => { lp.nailsToTear = nails; lp.tearDistM = dist; });
            if (B39Has("loom", "nailsToTear")) lp.nailsToTear = (int)B39Num("loom", "nailsToTear");
            if (B39Has("loom", "tearDistM")) lp.tearDistM = B39Num("loom", "tearDistM");
        }

        /// <summary>One fight's pool x design.n39.boss39.&lt;boss&gt;.hpMult, at full health only. True if it changed.</summary>
        bool Boss39Pool(Enemy body, string boss)
        {
            if (!B39Has(boss, "hpMult") || body.maxHp <= 0 || body.hp < body.maxHp) return false;
            double m = B39Num(boss, "hpMult");
            if (m <= 0 || m == 1) return false;
            body.maxHp = JsMath.Round(body.maxHp * m);
            body.hp = body.maxHp;
            boss39Pools++;
            return true;
        }
    }
}
