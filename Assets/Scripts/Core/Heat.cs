// NIGHT12 U3-25.9: run modifiers. Mutators is a port of skygrind/src/modes/mutators.js (config.mutators: LOW GRAVITY,
// GLASS CANNON, SPEED DEMON, NO REGEN; the best round and picks in the JSON profile). Heat is port-only (design.heat;
// Research/retention P2, D-31: the web's mutators only): rungs 1-3 = GLASS CANNON, SPEED DEMON, NO REGEN, each rung adding
// its rule to the ones below, with a score multiplier and a badge on the grade; an EXFIL on rung N opens rung N + 1.
// LOW GRAVITY stays a fun toggle, not a rung. Opt-in by choice on the run setup: rung 0 is the web's game.
// The port:
// - MutatorMath is mutators.js's pure part (MUTATORS, mutatorKey, isUnlocked, mutatorStats), line for line.
// - Mutators is createMutators with the profile as its store (profile.mutatorBest = state.best, profile.mutatorPicks =
//   state.picked). The web listens to runEnd {round}; here the Game calls RunEnd(round, recording), so a LAB run or an
//   automation run (recording false) writes nothing.
// - LOW GRAVITY is unavailable: it needs mover.gravityFactor, which Mover.cs doesn't have (Mover.cs changes only when a
//   task names it: CLAUDE.md rule 3). It is listed (locked or not), but it can't be picked, Apply drops it, and its
//   unlock emits nothing. MutatorStatsOf still computes its gravity (the web's pure numbers).
// - SPEED DEMON's enemy pace is HuskBrain.speedMult (brain.js pace = e.speedMult ?? 1, set by affixes.js setSpeedMult on
//   each new spawn; the port has no affixes, so the brain holds one x for every enemy it drives).
// - Heat's score multiplier is an Economy boost named "heat" (points earned x the rung's scoreMult, the way the web's
//   SPEED DEMON boosts points: economy.setBoost), stacked with SPEED DEMON's own x points as every boost stacks.
// Nothing here runs per step: Apply / Clear at a run's start and end, the rest on TITLE and RESULTS.
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class MutatorMath
    {
        /// <summary>mutators.js MUTATORS.</summary>
        public static readonly string[] All = { "lowGravity", "glassCannon", "speedDemon", "noRegen" };

        /// <summary>mutators.js mutatorStats: the combined numbers of a set.</summary>
        public struct Stats
        {
            public double gravity, dealt, taken, enemySpeed, points;
            public bool regen;
        }

        static bool Known(string id)
        {
            foreach (string m in All)
                if (m == id) return true;
            return false;
        }

        /// <summary>Pure: the board key of a set of mutators ('' for none), order-free (mutators.js mutatorKey).</summary>
        public static string MutatorKey(IEnumerable<string> list)
        {
            // [...list].filter((m) => MUTATORS.includes(m)).sort().join('+'): JS sort compares UTF-16 code units (ordinal).
            var keep = new List<string>();
            if (list != null)
                foreach (string m in list)
                    if (Known(m)) keep.Add(m);
            keep.Sort(string.CompareOrdinal);
            return string.Join("+", keep);
        }

        /// <summary>Pure: is a mutator unlocked at this best round? (mutators.js isUnlocked: no unlockRound = never.)</summary>
        public static bool IsUnlocked(string id, double bestRound, GameConfig cfg)
        {
            string path = "mutators.kinds." + id + ".unlockRound";
            double need = id != null && cfg.Has(path) ? cfg.Num(path) : double.PositiveInfinity;
            return bestRound >= need;
        }

        /// <summary>Pure: the combined effect numbers of a set (mutators.js mutatorStats).</summary>
        public static Stats MutatorStatsOf(IList<string> list, GameConfig cfg)
        {
            bool On(string id) => list != null && list.Contains(id);
            return new Stats
            {
                gravity = On("lowGravity") ? cfg.Num("mutators.kinds.lowGravity.gravity") : 1,
                dealt = On("glassCannon") ? cfg.Num("mutators.kinds.glassCannon.dealt") : 1,
                taken = On("glassCannon") ? cfg.Num("mutators.kinds.glassCannon.taken") : 1,
                enemySpeed = On("speedDemon") ? cfg.Num("mutators.kinds.speedDemon.enemySpeed") : 1,
                points = On("speedDemon") ? cfg.Num("mutators.kinds.speedDemon.points") : 1,
                regen = !On("noRegen"),
            };
        }
    }

    /// <summary>mutators.js createMutators: the picks on TITLE, the unlocks by best round, the running run's set.</summary>
    public sealed class Mutators
    {
        /// <summary>What a set changes (mutators.js apply's t; the web's mover is missing: no gravity factor in the port).</summary>
        public sealed class Targets
        {
            public Loadout loadout; // damageMult (GLASS CANNON dealt)
            public Health health; // takenMult (GLASS CANNON taken)
            public HuskBrain brain; // speedMult (SPEED DEMON enemy pace; the web's affixes.setSpeedMult)
            public Economy economy; // SetBoost("speedDemon", points); Heat's SetBoost("heat", scoreMult)
            public Regen regen; // SetEnabled(false) (NO REGEN)
        }

        /// <summary>One row of the title's list (mutators.js list(), plus `available` and the description).</summary>
        public sealed class Row
        {
            public string id, name, desc;
            public bool picked, unlocked, available;
            public double unlockRound;
        }

        public const string BoostName = "speedDemon"; // mutators.js economy.setBoost('speedDemon', ...)

        readonly GameConfig cfg;
        readonly EventBus events;
        readonly Profile profile;
        readonly List<string> active = new List<string>();

        public Mutators(GameConfig cfg, EventBus events, Profile profile)
        {
            this.cfg = cfg;
            this.events = events;
            this.profile = profile ?? new Profile();
        }

        /// <summary>The port can play it (false for LOW GRAVITY: Mover.cs has no gravity factor tonight).</summary>
        public static bool Available(string id) => id != "lowGravity" && System.Array.IndexOf(MutatorMath.All, id) >= 0;

        /// <summary>Best Survival round so far (the unlocks): the profile's.</summary>
        public double Best => profile.mutatorBest;

        bool Unlocked(string id) => MutatorMath.IsUnlocked(id, profile.mutatorBest, cfg);

        string Name(string id) => cfg.Str("mutators.kinds." + id + ".name");

        /// <summary>What the title shows: every mutator, picked and unlocked (mutators.js list).</summary>
        public List<Row> List()
        {
            var rows = new List<Row>(MutatorMath.All.Length);
            foreach (string id in MutatorMath.All)
            {
                string unlockPath = "mutators.kinds." + id + ".unlockRound";
                rows.Add(new Row
                {
                    id = id, name = Name(id), desc = cfg.Has("mutators.kinds." + id + ".desc") ? cfg.Str("mutators.kinds." + id + ".desc") : "",
                    picked = profile.mutatorPicks.Contains(id), unlocked = Unlocked(id), available = Available(id),
                    unlockRound = cfg.Has(unlockPath) ? cfg.Num(unlockPath) : double.PositiveInfinity,
                });
            }
            return rows;
        }

        /// <summary>mutators.js toggle: pick / unpick an unlocked (and available) mutator; true when it is now picked. The Game saves the profile.</summary>
        public bool Toggle(string id)
        {
            if (!Unlocked(id) || !Available(id)) return false;
            if (profile.mutatorPicks.Contains(id)) profile.mutatorPicks.Remove(id);
            else profile.mutatorPicks.Add(id);
            return profile.mutatorPicks.Contains(id);
        }

        /// <summary>mutators.js picks: the set a run starts with (unlocked, available picks only, in MUTATORS order).</summary>
        public List<string> Picks()
        {
            var list = new List<string>();
            foreach (string id in MutatorMath.All)
                if (profile.mutatorPicks.Contains(id) && Unlocked(id) && Available(id)) list.Add(id);
            return list;
        }

        /// <summary>The running run's set (the end screen, the board key). Read only.</summary>
        public IReadOnlyList<string> Active => active;

        /// <summary>NIGHT13 U3-26.10 (the run snapshot, Heat's section): the active list itself.</summary>
        internal List<string> ActiveList => active;

        /// <summary>mutators.js key: the running set's board key.</summary>
        public string Key => MutatorMath.MutatorKey(active);

        /// <summary>
        /// Lists merged in first-seen order, without repeats (main.js startSurvival's [...new Set([...picks, daily.mutator])]).
        /// Null lists are skipped.
        /// </summary>
        public static List<string> Union(params IEnumerable<string>[] lists)
        {
            var list = new List<string>();
            if (lists == null) return list;
            foreach (IEnumerable<string> l in lists)
                if (l != null)
                    foreach (string id in l)
                        if (id != null && !list.Contains(id)) list.Add(id);
            return list;
        }

        /// <summary>
        /// mutators.js runEnd: a Survival run ended at `round`. A new best round unlocks mutators (emits mutatorUnlock {id} for
        /// each newly unlocked, available one) and is stored in the profile. Not recording (a LAB run, automation): nothing.
        /// Returns the ids it unlocked (empty: none).
        /// </summary>
        public List<string> RunEnd(double round, bool recording)
        {
            var fresh = new List<string>();
            if (!recording || !(round > profile.mutatorBest)) return fresh; // (e.round ?? 0) <= state.best: return (NaN too)
            var before = new List<string>();
            foreach (string id in MutatorMath.All)
                if (Unlocked(id)) before.Add(id);
            profile.mutatorBest = round;
            foreach (string id in MutatorMath.All)
            {
                if (before.Contains(id) || !Unlocked(id) || !Available(id)) continue;
                fresh.Add(id);
                events?.Emit(MetaEvents.MutatorUnlock, new MutatorEvent { id = id });
            }
            return fresh;
        }

        /// <summary>mutators.js apply with the default list: the picks.</summary>
        public MutatorMath.Stats Apply(Targets t) => Apply(t, Picks());

        /// <summary>
        /// mutators.js apply: start a run with a set: damage both ways, enemy pace, points, regen (no gravity: LOW GRAVITY is
        /// dropped, as are unknown ids and repeats). Every target is set, not multiplied, as the web does; regen is only
        /// switched off (NO REGEN), never on: the run's own start enables it (SurvivalRun.Start, the web's startSurvival).
        /// </summary>
        public MutatorMath.Stats Apply(Targets t, List<string> list)
        {
            active.Clear();
            if (list != null)
                foreach (string id in list)
                    if (Available(id) && !active.Contains(id)) active.Add(id);
            MutatorMath.Stats s = MutatorMath.MutatorStatsOf(active, cfg);
            if (t == null) return s;
            if (t.loadout != null) t.loadout.damageMult = s.dealt;
            if (t.health != null) t.health.takenMult = s.taken;
            if (t.brain != null) t.brain.speedMult = s.enemySpeed;
            t.economy?.SetBoost(BoostName, s.points);
            if (!s.regen) t.regen?.SetEnabled(false);
            return s;
        }

        /// <summary>mutators.js clear: back to normal (a mode switch). Regen is left as it is (the web's clear doesn't touch it).</summary>
        public void Clear(Targets t)
        {
            active.Clear();
            if (t == null) return;
            if (t.loadout != null) t.loadout.damageMult = 1;
            if (t.health != null) t.health.takenMult = 1;
            if (t.brain != null) t.brain.speedMult = 1;
            t.economy?.SetBoost(BoostName, 1);
        }

        /// <summary>mutators.js debugSetBest: unlock by a best round (debug; writes the profile).</summary>
        public void DebugSetBest(double round) => profile.mutatorBest = round;
    }

    /// <summary>The HEAT ladder (design.heat): rung N plays rungs 1..N's mutators and pays x its score multiplier.</summary>
    public sealed class Heat : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the mutators active this run (the rung's and the DAILY's twist). The rung
        // itself is the run's stats (RunStats.heat, scoreMult); the numbers the mutators set (damage both ways, the enemy
        // pace, the "heat" points boost, NO REGEN) are the target systems' own fields, restored there.
        public void Save(StateWriter w) => w.Strings("active", mutators.ActiveList);

        public void Load(StateReader r)
        {
            List<string> a = r.Strings("active");
            foreach (string id in a) if (!Mutators.Available(id)) throw new StateException($"snapshot: heat: an unknown mutator '{id}'");
            mutators.ActiveList.Clear();
            mutators.ActiveList.AddRange(a);
        }

        /// <summary>design.heat.enabled (the ladder exists; picking a rung is still the player's choice).</summary>
        public bool enabled;

        /// <summary>The mutators (mutators.js) this ladder applies its rungs through; the title's toggles use it too.</summary>
        public readonly Mutators mutators;

        public const string BoostName = "heat"; // the Economy boost carrying the rung's score multiplier

        readonly GameConfig cfg;
        readonly EventBus events;
        readonly Profile profile;
        readonly List<string> rungMutator = new List<string>(); // design.heat.rungs[i].mutator (rung i + 1)
        readonly List<double> rungMult = new List<double>(); // design.heat.rungs[i].scoreMult

        public Heat(GameConfig cfg, EventBus events, Profile profile)
        {
            this.cfg = cfg;
            this.events = events;
            this.profile = profile ?? new Profile();
            mutators = new Mutators(cfg, events, this.profile);
            enabled = cfg.Design("heat.enabled");
            if (cfg.Has("design.heat.rungs"))
                foreach (object o in cfg.Arr("design.heat.rungs"))
                {
                    Dictionary<string, object> d = PortData.Obj(o);
                    rungMutator.Add((string)d["mutator"]);
                    rungMult.Add(PortData.Num(d["scoreMult"]));
                }
        }

        /// <summary>The number of rungs (3 tonight; 0 with the ladder off).</summary>
        public int Rungs => enabled ? rungMutator.Count : 0;

        /// <summary>The highest rung the player may pick (the profile's, at most Rungs).</summary>
        public int Unlocked => System.Math.Max(0, System.Math.Min(profile.heat, Rungs));

        /// <summary>A rung inside the ladder: 0 for none, at most Rungs (with the ladder off, always 0).</summary>
        public int Clamp(int rung) => rung <= 0 ? 0 : System.Math.Min(rung, Rungs);

        /// <summary>A rung's name for the run setup and the grade badge ("HEAT 2").</summary>
        public string Name(int rung)
        {
            int r = Clamp(rung);
            return r > 0 ? "HEAT " + r : "";
        }

        /// <summary>The rule it adds ("GLASS CANNON: x2 damage dealt and taken"): config.mutators.kinds.(id).name / desc.</summary>
        public string Rule(int rung)
        {
            int r = Clamp(rung);
            if (r == 0) return "";
            string id = rungMutator[r - 1];
            return cfg.Str("mutators.kinds." + id + ".name") + ": " + cfg.Str("mutators.kinds." + id + ".desc");
        }

        /// <summary>Every rule a rung plays (rungs 1..N), in rung order.</summary>
        public List<string> RulesOf(int rung)
        {
            var list = new List<string>();
            for (int i = 1; i <= Clamp(rung); i++) list.Add(Rule(i));
            return list;
        }

        /// <summary>The rung's score multiplier (1 for rung 0).</summary>
        public double ScoreMult(int rung)
        {
            int r = Clamp(rung);
            return r > 0 ? rungMult[r - 1] : 1;
        }

        /// <summary>The mutators a rung plays with (its own and every rung below it), in rung order.</summary>
        public List<string> MutatorsOf(int rung)
        {
            var list = new List<string>();
            for (int i = 0; i < Clamp(rung); i++) list.Add(rungMutator[i]);
            return list;
        }

        /// <summary>
        /// Start a run on `rung` in one call: its mutators plus `extra` (the title's toggles, the DAILY's twist: first in the
        /// list, the rung's after, without repeats) through Mutators.Apply, and the rung's score multiplier as the Economy
        /// boost "heat". Rung 0 with no extras: every target at 1, no boost (the web's game).
        /// </summary>
        public MutatorMath.Stats ApplyRung(Mutators.Targets t, int rung, IEnumerable<string> extra = null)
        {
            int r = Clamp(rung);
            MutatorMath.Stats s = mutators.Apply(t, Mutators.Union(extra, MutatorsOf(r)));
            t?.economy?.SetBoost(BoostName, ScoreMult(r));
            return s;
        }

        /// <summary>Back to normal (a mode switch): Mutators.Clear and no "heat" boost.</summary>
        public void Clear(Mutators.Targets t)
        {
            mutators.Clear(t);
            t?.economy?.SetBoost(BoostName, 1);
        }

        /// <summary>A run on `rung` ended; an exfil win opens rung + 1 (emits heatUnlock; true when it did).</summary>
        public bool RunEnd(int rung, bool exfilWin, bool recording)
        {
            if (!enabled || !recording || !exfilWin) return false;
            int next = System.Math.Min(System.Math.Max(rung, 0) + 1, Rungs);
            if (next <= profile.heat) return false;
            profile.heat = next;
            events?.Emit(MetaEvents.HeatUnlock, new HeatEvent { rung = next });
            return true;
        }
    }
}
