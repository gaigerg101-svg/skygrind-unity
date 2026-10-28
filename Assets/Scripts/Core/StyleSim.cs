// Port of skygrind/src/style/styleSim.js (OVERNIGHT2 N1), NIGHT9 U3-07: the headless style-meter pace simulation.
// Pure: it drives the same scoring core as the live meter (StyleCore, style/styleCore.js) with scripted play at the
// fixed 120 Hz step, modelling each weapon's FRESH the way the arsenal does (a use costs the weapon's freshCost; out of
// hand, and a moment after its last use, it recovers).
// Profiles:
// - spam: one weapon (or the F punch) on a stationary target, standing still: a dummy that never dies, or a stream
//   of Husks it kills. Target: never past BOLD.
// - varied: kills every 2-3 s, a weapon swap before each kill (the kill crosses families and is a SWAP kill), half
//   of them airborne, a couple of aimed hits per kill, a movement trick every few seconds. Target: SAVAGE in 35-60 s,
//   SKYGRIND only after 75-120 s.
// - camping: from SAVAGE, standing still (EXPOSED drain x2), sniping now and then. Target: decays to DULL.
// `node scripts/styleSim.mjs` prints the pace table; tests/styleSim.test.js asserts the targets.
// Port notes: the web's `fresh` / `lastUse` objects are dictionaries (their key order never decides anything); the
// result is a StyleSimResult (Golden/GoldenStyleCore.cs compares it with styleSim.json's records).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class StyleSimOptions
    {
        public string profile; // spam | varied | camping
        public string weapon; // null: the web's undefined
        public string target; // dummy | husks
        public bool parry;
        public double? seconds, seed, startRank, snipeEvery;
    }

    public sealed class StyleSimResult
    {
        public string profile, weapon;
        public int maxRank, finalRank;
        public double[] reached; // s when rank r was first shown (-1: never)
        public readonly List<(double second, int rank)> timeline = new List<(double, int)>();
        public double total;
        public int awards, capped, dimmed;
        public Dictionary<string, double> breakdown;
    }

    public static class StyleSim
    {
        const double Respawn = 1; // s between a kill and the next Husk on the spot (spam profiles)

        sealed class Spam
        {
            public Func<GameConfig, double> every, damage;
            public bool headshot, group, melee;
        }

        /// <summary>Seconds per trigger pull / swing for the spam profiles, and the damage each pull deals a Husk (40 HP).</summary>
        static readonly Dictionary<string, Spam> SpamSpecs = new Dictionary<string, Spam>
        {
            ["revolver"] = new Spam { every = c => c.Num("weapons.revolver.fireInterval"), damage = c => c.Num("weapons.revolver.damage") * c.Num("weapons.revolver.headshotMult"), headshot = true },
            ["shotgun"] = new Spam { every = c => c.Num("weapons.shotgun.fireInterval"), damage = c => c.Num("weapons.shotgun.pellets") * c.Num("weapons.shotgun.pelletDamage") },
            ["nailgun"] = new Spam { every = c => 1 / c.Num("weapons.nailgun.rate"), damage = c => c.Num("weapons.nailgun.damage"), group = true },
            ["rattler"] = new Spam { every = c => c.Num("weapons.rattler.burstInterval"), damage = c => c.Num("weapons.rattler.damage") * c.Num("weapons.rattler.rounds") },
            ["gauntlet"] = new Spam { every = c => c.Num("weapons.gauntlet.cycle"), damage = c => c.Num("weapons.gauntlet.damage"), melee = true },
            ["punch"] = new Spam { every = c => c.Num("punch.cycleTime"), damage = c => c.Num("punch.damage"), melee = true },
        };

        /// <param name="c">the game config (only read)</param>
        public static StyleSimResult SimulateStyle(GameConfig c, StyleSimOptions opts)
        {
            StyleMath.S s = StyleMath.From(c);
            ArsenalParams a = ArsenalParams.From(c);
            WeaponCatalog catalog = WeaponCatalog.From(c);
            double dt = c.Dt;
            double seconds = opts.seconds ?? 180;
            var rng = new Mulberry32(opts.seed ?? 1);
            var core = new StyleCore(s);
            core.Start();
            var fresh = new Dictionary<string, double>(); // catalog id -> FRESH
            var lastUse = new Dictionary<string, double>(); // catalog id -> s
            string inHand = opts.weapon ?? "revolver";
            double t = 0;
            double shot = 0;
            var reached = new double[s.ranks.Count];
            for (int i = 0; i < reached.Length; i++) reached[i] = -1;
            reached[0] = 0;
            var result = new StyleSimResult { profile = opts.profile, weapon = opts.weapon, reached = reached };
            var timeline = result.timeline;

            double FreshOf(string w) => fresh.TryGetValue(ArsenalMath.WeaponOf(w), out double f) ? f : 1;
            void Use(string w)
            {
                string id = ArsenalMath.WeaponOf(w);
                double? cost = catalog.Get(id)?.freshCost;
                if (cost == null) return;
                fresh[id] = ArsenalMath.SpendFresh(FreshOf(id), cost.Value);
                lastUse[id] = t;
            }
            void Recover()
            {
                string held = ArsenalMath.WeaponOf(inHand);
                foreach (string id in new List<string>(fresh.Keys))
                {
                    if (id == held || fresh[id] >= 1 || t - (lastUse.TryGetValue(id, out double u) ? u : double.NegativeInfinity) < a.recoverDelay) continue;
                    fresh[id] = ArsenalMath.RecoverFresh(fresh[id], dt, a.recover);
                }
            }
            bool Melee(string w) => w == "punch" || catalog.Get(ArsenalMath.WeaponOf(w))?.kind == "melee";
            // One hit (a killing one too), through the core like the live meter (fresh is read before the use is paid).
            // shotId: the web's default parameter `++shot` (null here) is counted only when the caller leaves it out.
            void Hit(string w, bool killed = false, string kind = "husk", string state = "GROUND", bool headshot = false, double? shotId = null, double distance = 8)
            {
                double id = shotId ?? ++shot;
                double f = FreshOf(w);
                core.Hit(w, state, headshot, 1, Melee(w), id, 1, f);
                if (killed) core.Kill(kind, w, state, distance, Melee(w), false, 1, f);
            }
            void StepTo(double until)
            {
                while (t < until - 1e-9)
                {
                    Recover();
                    core.Step(dt);
                    t += dt;
                    int r = core.Rank;
                    if (reached[r] < 0) reached[r] = JsMath.Round(t * 100) / 100;
                    if (timeline.Count == 0 || Math.Floor(t) > timeline[timeline.Count - 1].second) timeline.Add((Math.Floor(t), r));
                }
            }

            if (opts.profile == "spam")
            {
                Spam spec = SpamSpecs[inHand];
                double every = spec.every(c);
                double dmg = spec.damage(c);
                double huskHp = c.Num("husk.hp");
                double hp = huskHp;
                double groupAt = double.NegativeInfinity;
                double groupId = 0;
                double nextParry = 1.7;
                double arrive = 0; // the next Husk walks in this long after a kill (a stationary target, not an instant stream)
                for (double time = 0.5; time < seconds; time += every)
                {
                    StepTo(time);
                    if (t < arrive)
                    {
                        Use(inHand == "punch" ? "gauntlet" : inHand); // still spamming: a miss wears the weapon, pays nothing
                        continue;
                    }
                    if (spec.group)
                    {
                        if (t - groupAt >= c.Num("style.hitShotGroup"))
                        {
                            groupAt = t;
                            groupId = ++shot;
                        }
                    }
                    bool killed = opts.target == "husks" && (hp -= dmg) <= 0;
                    Hit(inHand, killed, headshot: spec.headshot, shotId: spec.group ? groupId : ++shot, distance: Melee(inHand) ? 1.5 : 10);
                    Use(inHand == "punch" ? "gauntlet" : inHand);
                    core.Use(inHand, inHand == "nailgun" ? s.nailgunUseScale : 1);
                    if (killed)
                    {
                        hp = huskHp;
                        arrive = t + Respawn;
                    }
                    if (opts.parry && t >= nextParry)
                    {
                        core.Parry("GROUND", FreshOf("gauntlet"));
                        nextParry = t + 1.7; // a Husk's wind-up + cooldown
                    }
                }
                StepTo(seconds);
            }
            else if (opts.profile == "varied")
            {
                string[] pool = { "revolver", "shotgun", "rattler", "zapper", "sunder", "bouncer", "flicker", "discus" };
                string[] kinds = { "husk", "swarmer", "husk", "drone", "spitter", "husk", "charger", "lancer" };
                (string name, string label)[] tricks = { ("wallJump", "WALL JUMP"), ("dashJump", "DASH JUMP"), ("slideJump", "SLIDE JUMP"), ("boostJump", "BOOST") };
                int wi = 0;
                double time = 1;
                double nextTrick = 2;
                int kill = 0;
                while (time < seconds)
                {
                    double gap = 2 + rng.Next();
                    string prev = pool[wi % pool.Length];
                    wi++;
                    string next = pool[wi % pool.Length];
                    // Two aimed hits with the weapon in hand, then a swap, then the kill with the next one.
                    for (int k = 0; k < 2; k++)
                    {
                        StepTo(time + k * 0.35);
                        Hit(prev, state: k == 0 ? "GROUND" : "SLIDE");
                        Use(prev);
                    }
                    StepTo(time + 0.9);
                    inHand = next;
                    StepTo(time + 1.2);
                    bool airborne = rng.Next() < 0.5;
                    string state = airborne ? "AIR" : rng.Next() < 0.5 ? "SLIDE" : "GROUND";
                    string kind = kinds[kill++ % kinds.Length];
                    Hit(next, true, kind, state, distance: Melee(next) ? 1.8 : 9);
                    Use(next);
                    // A competent melee kill lands with momentum (OVERNIGHT2 N2): an IMPACT on top.
                    if (Melee(next)) core.Bonus("IMPACT", s.bonus["impact"], next, state);
                    if (catalog.Get(next).family != catalog.Get(prev).family) core.Cross(null, next, state);
                    core.SwapKill(next, state);
                    while (nextTrick < time + gap)
                    {
                        StepTo(nextTrick);
                        var (name, label) = tricks[(int)Math.Floor(rng.Next() * tricks.Length)];
                        core.Trick(name, label, true, "AIR");
                        nextTrick += 2.5 + rng.Next() * 2;
                    }
                    time += gap;
                }
                StepTo(seconds);
            }
            else if (opts.profile == "camping")
            {
                core.DebugSetRank((int)(opts.startRank ?? 4));
                core.SetDrainMult(c.Num("director.camp.styleDrainMult")); // EXPOSED
                double every = opts.snipeEvery ?? 0;
                for (double time = 0.5; time < seconds; time += every != 0 ? every : seconds)
                {
                    StepTo(time);
                    if (every != 0)
                    {
                        Hit("revolver", headshot: true, distance: 30);
                        Use("revolver");
                    }
                }
                StepTo(seconds);
            }

            int maxRank = 0;
            for (int i = 0; i < reached.Length; i++) if (reached[i] >= 0) maxRank = i;
            result.maxRank = maxRank;
            result.finalRank = core.Rank;
            result.total = core.total;
            result.awards = core.statAwards;
            result.capped = core.statCapped;
            result.dimmed = core.statDimmed;
            result.breakdown = new Dictionary<string, double>(core.breakdown);
            return result;
        }

        /// <summary>The pace report: every profile the targets name, as rows (the REPORT table and the test read these).</summary>
        public static List<(string name, StyleSimResult result)> PaceReport(GameConfig c, double[] seeds = null)
        {
            seeds ??= new double[] { 1, 2, 3, 4, 5 };
            var rows = new List<(string, StyleSimResult)>();
            foreach (string weapon in new[] { "revolver", "shotgun", "nailgun", "rattler", "gauntlet", "punch" })
            {
                foreach (string target in new[] { "dummy", "husks" })
                {
                    StyleSimResult r = SimulateStyle(c, new StyleSimOptions { profile = "spam", weapon = weapon, target = target, parry = weapon == "punch" });
                    rows.Add(($"spam {weapon} ({target}{(weapon == "punch" ? " + parries" : "")})", r));
                }
            }
            foreach (double seed in seeds) rows.Add(($"varied (seed {seed})", SimulateStyle(c, new StyleSimOptions { profile = "varied", seed = seed, seconds = 150 })));
            rows.Add(("camping (idle, EXPOSED)", SimulateStyle(c, new StyleSimOptions { profile = "camping", seconds = 90 })));
            rows.Add(("camping (sniping 1/s, EXPOSED)", SimulateStyle(c, new StyleSimOptions { profile = "camping", snipeEvery = 1, seconds = 90 })));
            return rows;
        }
    }
}
