// The Survival director, ported from skygrind/src/ai/directorMath.js (pure: intensity, phases, camping, round budget,
// HP scaling, boss / swarm rounds, the introduction table, squad planning, out-of-view) and ai/director.js (each round's
// squads stream in: at most director.maxAlive alive, paced by intensity, out of view, 15-40 m away, one squad per round
// from behind, open zones only) with enemies/spawner.js's telegraph (a beam for spawner.telegraphTime, then the group
// spawns spread spawner.groupSpread around the point). Two seeded streams per round (the round's deal; the pacing).
// The kinds ported so far are the Husk, the Swarmer (from round 3, swarm rounds from 8, the hunter squad's six) and the
// Spitter (from round 6, NIGHT8 U2-10):
// planSquads keeps the templates of those two (its fallback is 3 husks); a round the web gives other kinds or a boss
// plays with what exists (logged).
// NIGHT11 U3-24.3 / U3-24.4: directorMath.js roundType (the objective rounds) and director.js addSquads (protocol.js's
// SURGE). Port difference (logged): AddSquads turns every kind that has no port yet (Available false: charger, lancer,
// warden, drone, a boss) into a "husk", the planner's fallback kind, so a squad keeps its size and its ported kinds (the
// web queues the kinds as they are; Husks.Spawn would place a husk for an unknown kind anyway, but the queue, the
// telegraph and its spawnTelegraph event would name a kind that never appears; the planner and the hunter squad already
// keep to the ported kinds). With no AddSquads call the director is the web's. `fullPace` (NIGHT11 design.exfil,
// port-only; false: the web's director): the spawn mult is 1 whatever the phase (no PEAK / RELAX pause).
// NIGHT19 U3-33.7 (builder E, design.enemies.n19): with the run's enemies block on, the ported kinds are Spawnable (the
// CHARGER), so the planner deals them at the web's rounds (director.rounds.introduce), AddSquads keeps them and the hunter
// squad gets its charger; each spawn's affixes roll after the HP scale (n19OnSpawn: main.js's spawner onSpawn). Off:
// NIGHT18's director exactly (Available everywhere). Buyables gains
// buyables.js's `owned` (a hold on an owned item is denied) and `remove` (NIGHT11 U3-24.4: the relays).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class DirectorState
    {
        public double intensity;
        public string phase = "BUILD_UP";
        public double phaseTime, relaxTime, camp, lingerTime;
        public Vec3? anchor, home;
        public bool hunted, exposed;
    }

    public static class DirectorMath
    {
        /// <summary>directorMath.js PHASES (NIGHT9 U3-07: the constant's vector is checked too).</summary>
        public static readonly string[] Phases = { "BUILD_UP", "PEAK", "RELAX" };

        public sealed class D
        {
            public double intensityPerHp, intensityPerKill, intensityDecay, peakAt, peakTime, relaxMin, relaxMax, relaxSpawnMult, relaxAggressionSpeedup;
            public double campRadius, lingerTime, lowDamage, riseTime, decayTime, relocate, relocateDrop, killChainDrop, exposedAt;
            public double baseCount, perRound, hpFromRound, hpPerRound, bossEvery, bossFrom, swarmEvery, swarmOffset, swarmFrom;
            public double objectiveFrom, objectiveEvery, holdOffset, goldOffset; // NIGHT11 U3-24.3: roundType's (NIGHT6 B7)
            public Dictionary<string, double> introduce = new Dictionary<string, double>(), weights = new Dictionary<string, double>();
            public List<List<string>> squads = new List<List<string>>();
        }

        public static D From(GameConfig c)
        {
            var d = new D
            {
                intensityPerHp = c.Num("director.intensityPerHp"), intensityPerKill = c.Num("director.intensityPerKill"), intensityDecay = c.Num("director.intensityDecay"),
                peakAt = c.Num("director.peakAt"), peakTime = c.Num("director.peakTime"), relaxMin = c.Num("director.relaxMin"), relaxMax = c.Num("director.relaxMax"),
                relaxSpawnMult = c.Num("director.relaxSpawnMult"), relaxAggressionSpeedup = c.Num("director.relaxAggressionSpeedup"),
                campRadius = c.Num("director.camp.radius"), lingerTime = c.Num("director.camp.lingerTime"), lowDamage = c.Num("director.camp.lowDamage"),
                riseTime = c.Num("director.camp.riseTime"), decayTime = c.Num("director.camp.decayTime"), relocate = c.Num("director.camp.relocate"),
                relocateDrop = c.Num("director.camp.relocateDrop"), killChainDrop = c.Num("director.camp.killChainDrop"), exposedAt = c.Num("director.camp.exposedAt"),
                baseCount = c.Num("director.rounds.base"), perRound = c.Num("director.rounds.perRound"), hpFromRound = c.Num("director.rounds.hpFromRound"),
                hpPerRound = c.Num("director.rounds.hpPerRound"), bossEvery = c.Num("director.rounds.bossEvery"), bossFrom = c.Num("director.rounds.bossFrom"),
                swarmEvery = c.Num("director.rounds.swarmEvery"), swarmOffset = c.Num("director.rounds.swarmOffset"), swarmFrom = c.Num("director.rounds.swarmFrom"),
                objectiveFrom = c.Num("director.rounds.objectiveFrom"), objectiveEvery = c.Num("director.rounds.objectiveEvery"),
                holdOffset = c.Num("director.rounds.holdOffset"), goldOffset = c.Num("director.rounds.goldOffset"),
            };
            foreach (var kv in c.Obj("director.rounds.introduce")) d.introduce[kv.Key] = PortData.Num(kv.Value);
            foreach (var kv in c.Obj("director.rounds.weights")) d.weights[kv.Key] = PortData.Num(kv.Value);
            foreach (object s in c.Arr("director.rounds.squads")) d.squads.Add(PortData.Arr(s).ConvertAll(o => (string)o));
            return d;
        }

        /// <summary>Intensity + phase for one step; the new phase when it changed, else null.</summary>
        public static string StepDirector(DirectorState s, double dt, double damageTaken, double nearbyKills, double aggression, Func<double> rng, D d)
        {
            s.intensity = Math.Min(1, Math.Max(0, s.intensity + damageTaken * d.intensityPerHp + nearbyKills * d.intensityPerKill - d.intensityDecay * dt));
            s.phaseTime += dt;
            if (s.phase == "BUILD_UP" && s.intensity >= d.peakAt) return Enter(s, "PEAK");
            if (s.phase == "PEAK" && s.phaseTime >= d.peakTime)
            {
                s.relaxTime = d.relaxMin + (d.relaxMax - d.relaxMin) * rng();
                return Enter(s, "RELAX");
            }
            if (s.phase == "RELAX")
            {
                s.phaseTime += dt * d.relaxAggressionSpeedup * aggression;
                if (s.phaseTime >= s.relaxTime) return Enter(s, "BUILD_UP");
            }
            return null;
        }

        /// <summary>design.director (Unity-only, NIGHT8 U2-08): PEAK FADE's numbers.</summary>
        public struct Fade
        {
            public double engagedRange, fadeBelow, fadeEngaged, fadeMaxTime;

            public static Fade From(GameConfig c) => new Fade
            {
                engagedRange = c.Num("design.director.engagedRange"), fadeBelow = c.Num("design.director.fadeBelow"),
                fadeEngaged = c.Num("design.director.fadeEngaged"), fadeMaxTime = c.Num("design.director.fadeMaxTime"),
            };
        }

        /// <summary>
        /// NIGHT8 U2-08 PEAK FADE (Booth, GDC 2009; design.director.peakFade): StepDirector, except that intensity doesn't
        /// decay while `engaged` > 0 (token holders committed within engagedRange), and PEAK ends in PEAK_FADE (no spawns)
        /// instead of RELAX: RELAX starts once intensity < fadeBelow with at most fadeEngaged engaged, or after fadeMaxTime.
        /// The web's StepDirector (golden-tested) is untouched; with the flag off the director calls it.
        /// </summary>
        public static string StepDirectorFade(DirectorState s, double dt, double damageTaken, double nearbyKills, double aggression, Func<double> rng, D d, Fade f, int engaged)
        {
            double decay = engaged > 0 ? 0 : d.intensityDecay;
            s.intensity = Math.Min(1, Math.Max(0, s.intensity + damageTaken * d.intensityPerHp + nearbyKills * d.intensityPerKill - decay * dt));
            s.phaseTime += dt;
            if (s.phase == "BUILD_UP" && s.intensity >= d.peakAt) return Enter(s, "PEAK");
            if (s.phase == "PEAK" && s.phaseTime >= d.peakTime) return Enter(s, "PEAK_FADE");
            if (s.phase == "PEAK_FADE" && ((s.intensity < f.fadeBelow && engaged <= f.fadeEngaged) || s.phaseTime >= f.fadeMaxTime))
            {
                s.relaxTime = d.relaxMin + (d.relaxMax - d.relaxMin) * rng();
                return Enter(s, "RELAX");
            }
            if (s.phase == "RELAX")
            {
                s.phaseTime += dt * d.relaxAggressionSpeedup * aggression;
                if (s.phaseTime >= s.relaxTime) return Enter(s, "BUILD_UP");
            }
            return null;
        }

        static string Enter(DirectorState s, string phase)
        {
            s.phase = phase;
            s.phaseTime = 0;
            if (phase == "RELAX") s.intensity = Math.Min(s.intensity, 0.6);
            return phase;
        }

        public static double PhaseSpawnMult(string phase, D d) => phase == "PEAK" || phase == "PEAK_FADE" ? 0 : phase == "RELAX" ? d.relaxSpawnMult : 1; // PEAK_FADE: Unity-only (U2-08)

        /// <summary>The camp score for one step: edges (started EXPOSED, ended, just hunted).</summary>
        public static (bool started, bool ended, bool hunted) StepCamp(DirectorState s, double dt, double x, double z, int enemiesAlive, double dealtRecent, bool killChain, D c)
        {
            if (!s.anchor.HasValue) s.anchor = new Vec3(x, 0, z);
            if (!s.home.HasValue || s.camp <= 0) s.home = new Vec3(s.anchor.Value.x, 0, s.anchor.Value.z);
            double away = JsMath.Hypot(x - s.anchor.Value.x, z - s.anchor.Value.z);
            if (JsMath.Hypot(x - s.home.Value.x, z - s.home.Value.z) > c.relocate)
            {
                s.anchor = s.home = new Vec3(x, 0, z);
                s.lingerTime = 0;
                s.camp = Math.Max(0, s.camp - c.relocateDrop);
                s.hunted = false;
            }
            else if (away > c.campRadius)
            {
                s.anchor = new Vec3(x, 0, z);
                s.lingerTime = 0;
            }
            else s.lingerTime += dt;
            bool lingering = s.lingerTime > c.lingerTime && enemiesAlive > 0 && dealtRecent < c.lowDamage;
            if (lingering) s.camp = Math.Min(1, s.camp + dt / c.riseTime);
            else s.camp = Math.Max(0, s.camp - dt / c.decayTime);
            if (killChain) s.camp = Math.Max(0, s.camp - c.killChainDrop);
            bool exposed = s.camp >= c.exposedAt;
            bool started = exposed && !s.exposed, ended = !exposed && s.exposed, hunted = false;
            s.exposed = exposed;
            if (s.camp >= 1 && !s.hunted)
            {
                s.hunted = true;
                hunted = true;
            }
            return (started, ended, hunted);
        }

        public static double RoundBudget(int round, D r) => r.baseCount + r.perRound * round;

        public static double RoundHpMult(int round, D r) => 1 + Math.Max(0, round - r.hpFromRound) * r.hpPerRound;

        // NIGHT11: the web's `??` fallbacks (bossFrom ?? bossEvery, swarmFrom ?? swarmEvery, swarmOffset ?? 0), seen by the p20
        // roundType vectors with those keys absent (NaN in D stands for an absent key: config JSON can't hold a NaN).
        static double Or(double v, double fallback) => double.IsNaN(v) ? fallback : v;

        public static bool IsBossRound(int round, D r) => round >= Or(r.bossFrom, r.bossEvery) && round % (int)r.bossEvery == 0;

        public static bool IsSwarmRound(int round, D r) => round >= Or(r.swarmFrom, r.swarmEvery) && round % (int)r.swarmEvery == (int)Or(r.swarmOffset, 0) && !IsBossRound(round, r);

        /// <summary>
        /// NIGHT6 B7: the round's kind: 'boss', 'swarm', an objective ('hold': stand in a ring; 'gold': kills pay double, chains
        /// pay a bonus) on its offset every `objectiveEvery` rounds from `objectiveFrom` (never a boss or swarm round), else
        /// 'normal'. Pure, by the round number only (a seed replays it).
        /// </summary>
        public static string RoundType(int round, D r)
        {
            if (IsBossRound(round, r)) return "boss";
            if (IsSwarmRound(round, r)) return "swarm";
            if (r.objectiveFrom != 0 && !double.IsNaN(r.objectiveFrom) && round >= r.objectiveFrom) // `r.objectiveFrom &&`: 0 / NaN are falsy
            {
                double k = (double)round % r.objectiveEvery; // JS % on doubles (NaN for an objectiveEvery of 0, as in JS)
                if (k == r.holdOffset) return "hold";
                if (k == r.goldOffset) return "gold";
            }
            return "normal";
        }

        public static Dictionary<string, double> RoundKinds(int round, D r)
        {
            var output = new Dictionary<string, double>();
            if (IsSwarmRound(round, r))
            {
                output["swarmer"] = 1;
                return output;
            }
            foreach (var kv in r.introduce) if (round >= kv.Value) output[kv.Key] = r.weights[kv.Key];
            return output;
        }

        /// <summary>The round's squads: the budget dealt into the templates the round allows (seeded).</summary>
        public static List<List<string>> PlanSquads(int round, Func<double> rng, D r, Func<string, bool> available)
        {
            double budget = RoundBudget(round, r);
            var kinds = RoundKinds(round, r);
            bool swarm = IsSwarmRound(round, r);
            var templates = r.squads.FindAll(t => t.TrueForAll(k => kinds.ContainsKey(k) && available(k)) && (!swarm || t.TrueForAll(k => k == "swarmer")));
            List<string> fallback = available("husk") ? new List<string> { "husk", "husk", "husk" } : new List<string>();
            if (fallback.Count == 0)
            {
                foreach (string k in kinds.Keys) if (available(k)) { fallback.Add(k); break; }
            }
            var squads = new List<List<string>>();
            double left = budget;
            int guard = 0;
            while (left > 0 && guard++ < 200)
            {
                var pool = templates.Count > 0 ? templates : new List<List<string>> { fallback };
                double total = 0;
                var w = new double[pool.Count];
                for (int i = 0; i < pool.Count; i++)
                {
                    double v = 0;
                    foreach (string k in pool[i]) v += kinds.TryGetValue(k, out double kw) ? kw : 1;
                    w[i] = v / pool[i].Count;
                    total += w[i];
                }
                double pick = rng() * total;
                var t = pool[0];
                for (int i = 0; i < pool.Count; i++)
                {
                    pick -= w[i];
                    if (pick <= 0)
                    {
                        t = pool[i];
                        break;
                    }
                }
                var squad = t.GetRange(0, (int)Math.Min(t.Count, left));
                squads.Add(squad);
                left -= squad.Count;
            }
            return squads;
        }

        public static bool OutOfView(double px, double pz, double yaw, double sx, double sz, double halfFovDeg, bool visible)
        {
            double fx = -JsMath.Sin(yaw), fz = -JsMath.Cos(yaw);
            double dx = sx - px, dz = sz - pz;
            double len = JsMath.Hypot(dx, dz);
            if (len == 0) len = 1;
            double cos = (dx * fx + dz * fz) / len;
            return cos < JsMath.Cos(halfFovDeg * Math.PI / 180) || !visible;
        }

        public static bool Behind(double px, double pz, double yaw, double sx, double sz) => (sx - px) * -JsMath.Sin(yaw) + (sz - pz) * -JsMath.Cos(yaw) < 0;
    }

    public sealed class Telegraph
    {
        public Vec3 point;
        public List<string> kinds;
        public int steps;
        public double age; // s (visual)
        /// <summary>NIGHT20 (design.director.n20's failsafe, Core/Enemies/Director20.cs): each kind's HP when it lands (a
        /// recycled walker keeps its own); null: the round's HP scale, as ever.</summary>
        public List<double> hps;
        /// <summary>NIGHT20 (design.adds.elite, Core/Enemies/SpecialRounds.cs): THE ELITE's telegraph (Director.n20OnElite
        /// gets its body when it lands).</summary>
        public bool elite;
    }

    public sealed partial class Director : IRoundSpawner, IRunState
    {
        /// <summary>NIGHT19 (the shared commit): the run's NIGHT19 blocks (SurvivalSim sets them at the run's start, before
        /// NewRun). The new kinds' spawns read design.enemies.n19 here; all off: NIGHT18's director.</summary>
        public N19Blocks n19;
        /// <summary>NIGHT20 (the shared commit): the run's NIGHT20 blocks (SurvivalSim sets them at the run's start, before
        /// NewRun): design.director.n20 and the adds' enemy rows read here; all off: NIGHT19's director.</summary>
        public N20Blocks n20;

        // NIGHT13 U3-26.10: the run snapshot: the pacing state (intensity, phase, camp, the anchors), the round's plan and
        // both streams (StartWave reseeds them from the run seed each round; mid-round they have advanced, so they are
        // saved), the queue and telegraphs (empty at a break unless a SURGE queued squads), the dealt / kill windows, the
        // clocks and flags, this round's interval, and whether the LAB's SQUADS TIGHTEN hook was on (Lab.Apply sets it;
        // Load sets the same function back from config: LabMath.SquadInterval on design.lab's numbers).
        public void Save(StateWriter w)
        {
            w.D("intensity", state.intensity);
            w.S("phase", state.phase);
            w.D("phaseTime", state.phaseTime);
            w.D("relaxTime", state.relaxTime);
            w.D("camp", state.camp);
            w.D("lingerTime", state.lingerTime);
            w.VN("anchor", state.anchor);
            w.VN("home", state.home);
            w.B("hunted", state.hunted);
            w.B("exposed", state.exposed);
            w.Begin("queue");
            w.I("n", queue.Count);
            for (int i = 0; i < queue.Count; i++) w.Strings(i.ToString(System.Globalization.CultureInfo.InvariantCulture), queue[i]);
            w.End();
            w.Begin("telegraphs");
            w.I("n", telegraphs.Count);
            for (int i = 0; i < telegraphs.Count; i++)
            {
                Telegraph t = telegraphs[i];
                w.Begin(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                w.V("point", t.point);
                w.Strings("kinds", t.kinds);
                w.I("steps", t.steps);
                w.D("age", t.age);
                if (t.hps != null) w.Doubles("hps", t.hps); // NIGHT20: a recycled walker's HP (only then)
                w.End();
            }
            w.End();
            var dt = new List<double>(dealt.Count);
            var da = new List<double>(dealt.Count);
            foreach (var d in dealt)
            {
                dt.Add(d.t);
                da.Add(d.amount);
            }
            w.Doubles("dealtT", dt);
            w.Doubles("dealtAmount", da);
            w.Doubles("kills", kills);
            w.Rng("rng", rng);
            w.Rng("paceRng", paceRng);
            w.B("fullPace", fullPace);
            w.I("round", round);
            w.I("cooldown", cooldown);
            w.B("active", active);
            w.B("behindDone", behindDone);
            w.D("time", time);
            w.D("damageTaken", damageTaken);
            w.D("nearbyKills", nearbyKills);
            w.D("hpMult", hpMult);
            w.I("squadsSpawned", squadsSpawned);
            w.D("interval", interval);
            w.B("labSquads", squadIntervalOf != null);
        }

        public void Load(StateReader r)
        {
            StateReader q = r.Section("queue"), tg = r.Section("telegraphs");
            var newQueue = new List<List<string>>();
            int qn = q.Int("n");
            for (int i = 0; i < qn; i++) newQueue.Add(q.Strings(i.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            var newTelegraphs = new List<Telegraph>();
            int tn = tg.Int("n");
            for (int i = 0; i < tn; i++)
            {
                StateReader t = tg.Section(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                newTelegraphs.Add(new Telegraph { point = t.V("point"), kinds = t.Strings("kinds"), steps = t.Int("steps"), age = t.D("age"), hps = t.Has("hps") ? t.Doubles("hps") : null });
            }
            List<double> dt = r.Doubles("dealtT"), da = r.Doubles("dealtAmount"), k = r.Doubles("kills");
            if (dt.Count != da.Count) throw new StateException("snapshot: director.dealt");
            string phase = r.S("phase");
            if (Array.IndexOf(DirectorMath.Phases, phase) < 0 && phase != "PEAK_FADE") throw new StateException($"snapshot: director.phase '{phase}'");
            state = new DirectorState
            {
                intensity = r.D("intensity"), phase = phase, phaseTime = r.D("phaseTime"), relaxTime = r.D("relaxTime"), camp = r.D("camp"),
                lingerTime = r.D("lingerTime"), anchor = r.VN("anchor"), home = r.VN("home"), hunted = r.B("hunted"), exposed = r.B("exposed"),
            };
            queue.Clear();
            queue.AddRange(newQueue);
            telegraphs.Clear();
            telegraphs.AddRange(newTelegraphs);
            dealt.Clear();
            for (int i = 0; i < dt.Count; i++) dealt.Add((dt[i], da[i]));
            kills.Clear();
            kills.AddRange(k);
            rng = r.Rng("rng");
            paceRng = r.Rng("paceRng");
            paceNext = paceRng.Next;
            fullPace = r.B("fullPace");
            round = r.Int("round");
            cooldown = r.Int("cooldown");
            active = r.B("active");
            behindDone = r.B("behindDone");
            time = r.D("time");
            damageTaken = r.D("damageTaken");
            nearbyKills = r.D("nearbyKills");
            hpMult = r.D("hpMult");
            squadsSpawned = r.Int("squadsSpawned");
            interval = r.D("interval");
            if (r.B("labSquads"))
            {
                LabMath.P lab = LabMath.P.From(cfg);
                squadIntervalOf = (n, iv) => LabMath.SquadInterval(iv, n, lab);
            }
            else squadIntervalOf = null;
        }

        readonly GameConfig cfg;
        readonly EventBus events;
        readonly Husks husks;
        readonly HuskBrain brain;
        readonly Zones zones;
        readonly CoreGrid core;
        readonly ICollisionWorld geometry;
        readonly Func<Vec3> playerPos;
        readonly Func<double> playerYaw;
        readonly Func<double> playerSpeed;
        readonly NavField nav;
        readonly DirectorMath.D d;
        public DirectorState state = new DirectorState();
        readonly List<List<string>> queue = new List<List<string>>();
        public readonly List<Telegraph> telegraphs = new List<Telegraph>();
        readonly List<(double t, double amount)> dealt = new List<(double, double)>();
        readonly List<double> kills = new List<double>();
        readonly List<SpawnNode> ground = new List<SpawnNode>(), high = new List<SpawnNode>();
        Mulberry32 rng = new Mulberry32(1), paceRng = new Mulberry32(2);
        Func<double> paceNext; // paceRng.Next, made once per stream (a method group allocates a delegate per use; U2-02)
        public bool peakFade; // design.director.peakFade (U2-08); false: the web's director
        /// <summary>NIGHT11 design.exfil (port-only): while true the squads stream at a spawn mult of 1 whatever the phase (no PEAK / RELAX pause); false: the web's director. NewRun clears it.</summary>
        public bool fullPace;
        DirectorMath.Fade fade;
        readonly List<(SpawnNode n, double dist, bool behind)> scored = new List<(SpawnNode, double, bool)>(), cands = new List<(SpawnNode, double, bool)>();
        int round, cooldown;
        bool active, behindDone;
        double time, damageTaken, nearbyKills, hpMult = 1;
        readonly double maxAlive, squadInterval, spawnMin, spawnMax, spawnFallbackMin, viewHalfAngle, nearbyKillRange, aggressiveSpeed, dealtWindow, killChainTime, groupSpread, eyeHeight, seedStride, paceSeedOffset;
        readonly int openingSquads, killChain, telegraphSteps;
        public int squadsSpawned;
        public Func<int, double, double> squadIntervalOf; // NIGHT8 U2-16 LAB SQUADS TIGHTEN: (round, director.squadInterval) -> the interval; null: the web's
        double interval;
        public double SquadInterval => interval; // this round's (director.squadInterval, or the LAB's)
        readonly List<string> hunterSquad;

        /// <summary>The kinds with a port (main.js: !!enemies.systems[kind] && !boss).</summary>
        public static bool Available(string kind) => kind == "husk" || kind == "swarmer" || kind == "spitter";

        /// <summary>
        /// NIGHT19 U3-33.7 (builder E): the kinds a run spawns: NIGHT18's three (Available), and with design.enemies.n19 the
        /// ported CHARGER and DRONE (Core/Enemies/). The planner (PlanSquads' `available`), AddSquads and the hunter squad
        /// read it; with the block off it is Available, so every plan is NIGHT18's.
        /// </summary>
        public static bool Spawnable(string kind, N19Blocks n19) => Available(kind) || (n19.enemies && (kind == "charger" || kind == "drone"));

        /// <summary>NIGHT20 U3-34.9 C2 (builder E; design.adds.lancer): Spawnable, and the ported LANCER (Core/Enemies/Lancer.cs)
        /// with its row on; off: Spawnable's kinds, so every plan is NIGHT19's.</summary>
        public static bool Spawnable(string kind, N19Blocks n19, bool lancer) => Spawnable(kind, n19) || (lancer && kind == "lancer");

        /// <summary>NIGHT20 C4 (design.adds.warden): Spawnable with the LANCER's row, and the ported WARDEN (Core/Enemies/Warden.cs)
        /// with its row on.</summary>
        public static bool Spawnable(string kind, N19Blocks n19, bool lancer, bool warden) => Spawnable(kind, n19, lancer) || (warden && kind == "warden");

        /// <summary>NIGHT20 C4: this run deals WARDENs (the adds block and its row).</summary>
        bool WardenOn => n20.Add(cfg, "warden");

        /// <summary>NIGHT20: this run deals LANCERs (the adds block and its row).</summary>
        bool LancerOn => n20.Add(cfg, "lancer");

        /// <summary>
        /// NIGHT20 (builder E): a kind with a port on this build (the pools spawn it), whatever the blocks: NIGHT18's three
        /// and the ported CHARGER and DRONE. THE RANGE's "no port on this build" check reads it (Available is NIGHT18's plan
        /// and stays so); a cell spawns its kinds itself, as the tests do.
        /// </summary>
        public static bool Ported(string kind) => Available(kind) || kind == "charger" || kind == "drone";

        /// <summary>director.js addSquads with the port's fallback (the header): a kind the run can't spawn becomes a husk.</summary>
        public static List<string> PortedSquad(List<string> s, N19Blocks n19)
        {
            var squad = new List<string>(s.Count);
            foreach (string kind in s) squad.Add(Spawnable(kind, n19) ? kind : "husk");
            return squad;
        }

        /// <summary>NIGHT20: PortedSquad with the run's LANCER row.</summary>
        List<string> PortedSquad20(List<string> s, bool lancer, bool warden)
        {
            var squad = new List<string>(s.Count);
            foreach (string kind in s) squad.Add(Spawnable(kind, n19, lancer, warden) ? kind : "husk");
            return squad;
        }

        /// <summary>NIGHT19: main.js spawner onSpawn's affix roll (affixes.onSpawn, after the HP scale), the run's hook (SurvivalSim.
        /// Enemies19.cs); null: none. Called only with design.enemies.n19 on.</summary>
        public Action<Husk> n19OnSpawn;

        /// <summary>NIGHT20 A1 (design.adds.bulwark): director.js startRound's `boss` (a boss round has no regular squads: the
        /// boss calls its adds in) for round n; null (the row off): the planner's round, as ever.</summary>
        public Func<int, bool> bossRound;

        /// <summary>NIGHT20 A1: main.js survivalHooks.isDone's `!bossKit.anyAlive()` (a boss round lasts while its boss lives);
        /// null: Done is the squads' alone.</summary>
        public Func<bool> bossAlive;

        public Director(GameConfig cfg, EventBus events, Husks husks, HuskBrain brain, Zones zones, CoreGrid core, ICollisionWorld geometry, NavField nav, Func<Vec3> playerPos, Func<double> playerYaw, Func<double> playerSpeed)
        {
            this.cfg = cfg;
            this.events = events;
            this.husks = husks;
            this.brain = brain;
            this.zones = zones;
            this.core = core;
            this.geometry = geometry;
            this.nav = nav;
            this.playerPos = playerPos;
            this.playerYaw = playerYaw;
            this.playerSpeed = playerSpeed;
            d = DirectorMath.From(cfg);
            maxAlive = cfg.Num("director.maxAlive");
            squadInterval = interval = cfg.Num("director.squadInterval");
            openingSquads = cfg.Int("director.openingSquads");
            spawnMin = cfg.Num("director.spawnMin");
            spawnMax = cfg.Num("director.spawnMax");
            spawnFallbackMin = cfg.Num("director.spawnFallbackMin");
            viewHalfAngle = cfg.Num("director.viewHalfAngle");
            nearbyKillRange = cfg.Num("director.nearbyKillRange");
            aggressiveSpeed = cfg.Num("director.aggressiveSpeed");
            dealtWindow = cfg.Num("director.camp.dealtWindow");
            killChainTime = cfg.Num("director.camp.killChainTime");
            killChain = cfg.Int("director.camp.killChain");
            groupSpread = cfg.Num("spawner.groupSpread");
            telegraphSteps = MovementMath.StepsFor(cfg.Num("spawner.telegraphTime"), cfg.Dt);
            eyeHeight = cfg.movement.body.eyeHeight;
            seedStride = cfg.Num("director.rounds.seedStride");
            paceSeedOffset = cfg.Num("director.rounds.paceSeedOffset");
            hunterSquad = cfg.Strs("director.hunterSquad");
            paceNext = paceRng.Next;
            peakFade = cfg.Design("director.peakFade");
            if (peakFade) fade = DirectorMath.Fade.From(cfg);
            events.On<HitEvent>("hit", e =>
            {
                if (e.owner == "player" && active) dealt.Add((time, e.damage));
            });
            events.On<KillEvent>("kill", e =>
            {
                if (e.owner != "player" || !active) return;
                kills.Add(time);
                Vec3 p = playerPos();
                if (JsMath.Hypot(e.position.x - p.x, e.position.z - p.z) <= nearbyKillRange) nearbyKills++;
            });
            events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner == "player" && active) damageTaken += e.amount;
            });
        }

        public bool Exposed => active && state.exposed;
        public bool Hunted => active && state.hunted; // the brain's +1 tokens (main.js getCamping)
        public string Phase => state.phase;
        public double HpMult => hpMult;
        public int Round => round;

        int Telegraphed()
        {
            int n = 0;
            foreach (Telegraph t in telegraphs) n += t.kinds.Count;
            return n;
        }

        /// <summary>Enemies still to kill this round: queued + telegraphed + alive.</summary>
        public int Remaining
        {
            get
            {
                int n = husks.Count + Telegraphed();
                foreach (var s in queue) n += s.Count;
                return n;
            }
        }

        public bool Done => Remaining == 0 && !(bossAlive != null && bossAlive()); // NIGHT20 A1: and no boss alive (main.js isDone)

        /// <summary>NIGHT20 A1: spawner.js spawnGroupAt(kinds, point): a group at a point after the telegraph (the BULWARK's
        /// hatches), through the same beam and spawn as the squads' (the round's HP scale, the affix roll, the brain).</summary>
        public void SpawnGroupAt(List<string> kinds, Vec3 point)
        {
            telegraphs.Add(new Telegraph { point = point, kinds = new List<string>(kinds), steps = telegraphSteps });
            events.Emit("spawnTelegraph", new SpawnEvent { kind = kinds[0], position = point });
        }

        /// <summary>The squads still to spawn this round, in order (read only; NIGHT11: what AddSquads queued).</summary>
        public IReadOnlyList<List<string>> Queue => queue;

        public void Clear()
        {
            queue.Clear();
            telegraphs.Clear();
            husks.KillAll();
            brain.Clear();
        }

        public void NewRun()
        {
            state = new DirectorState();
            Clear();
            dealt.Clear();
            kills.Clear();
            time = 0;
            active = true;
            fullPace = false; // a new run starts at the web's pace (design.exfil sets it for its round)
            // NIGHT14 U3-27.2 (port-only: a clean slate per run): the streams as the constructor made them, so the break before
            // round 1 starts like a fresh composition's (StartWave reseeds both from the run seed at each round).
            rng = new Mulberry32(1);
            paceRng = new Mulberry32(2);
            paceNext = paceRng.Next;
            // NIGHT16 (the NIGHT14 residual, port-only like the streams above): the per-step accumulators Step reads and zeroes.
            // A hurt or a nearby kill after the director's last Step of a run (the killing blow, a hit in the step a run was left
            // in) stayed in them, and the next run's first fighting step added it to the intensity (a played composition's
            // seed-1 run left the baseline). A fresh composition starts with both 0.
            damageTaken = 0;
            nearbyKills = 0;
            ResetN20(); // NIGHT20: design.director.n20's clocks (Core/Enemies/Director20.cs; nothing else reads them)
        }

        public void Stop()
        {
            active = false;
            queue.Clear();
            state.exposed = false;
            state.hunted = false;
        }

        bool VisibleFromPlayer(SpawnNode n)
        {
            Vec3 p = playerPos();
            var eye = new Vec3(p.x, p.y + eyeHeight, p.z);
            Vec3 dir = new Vec3(n.x - eye.x, n.y + 1 - eye.y, n.z - eye.z);
            double len = dir.Length;
            dir = Vec3.DivideScalar(dir, len > 0 ? len : 1);
            return !geometry.RayIntersect(eye, dir, out RayHit hit) || hit.distance >= len - 0.5;
        }

        bool FitsHusk(SpawnNode n)
        {
            HuskParams h = husks.p;
            var capsule = new Capsule { start = new Vec3(n.x, n.y + h.spawnLift + h.radius, n.z), end = new Vec3(n.x, n.y + h.spawnLift + h.height - h.radius, n.z), radius = h.radius };
            return !WorldCollider.CapsuleIntersect(huskWorld ?? geometry, capsule, out _, out _);
        }

        public ICollisionWorld huskWorld; // the live world (with the gate layer) the husks spawn into

        (SpawnNode n, double dist)? PickNode(bool wantBehind)
        {
            zones.SpawnPoints(ground, high, core);
            if (ground.Count == 0) return null;
            return PickFrom(ground, wantBehind, true, false);
        }

        /// <summary>pickNode's scoring and tiers over a pool (NIGHT20: the pool, the route check and the inside-a-solid check
        /// are parameters for design.director.n20's picks; NIGHT19's PickNode is the ground with the route check, as ever).</summary>
        (SpawnNode n, double dist)? PickFrom(List<SpawnNode> pool, bool wantBehind, bool needRoute, bool notInside)
        {
            if (pool.Count == 0) return null;
            Vec3 p = playerPos();
            double yaw = playerYaw();
            scored.Clear();
            foreach (SpawnNode n in pool)
            {
                if (needRoute && !(nav.DistanceAt(n.x, n.y, n.z) < double.PositiveInfinity)) continue; // a route to you on the grid
                // NIGHT7 fix of a web bug (logged): nodes are sampled before the gate fields join the collider, so a node on a
                // zone's edge (LANDING's x = +-14) can't fit a husk while the next zone is locked; the web lost those spawns.
                if (!FitsHusk(n)) continue;
                if (notInside && InsideSolid(n)) continue; // NIGHT20 (design.director.n20): never under the top of a solid
                scored.Add((n, JsMath.Hypot(n.x - p.x, n.z - p.z), DirectorMath.Behind(p.x, p.z, yaw, n.x, n.z)));
            }
            for (int ti = 0; ti < 4; ti++)
            {
                cands.Clear();
                foreach (var s in scored) if (Tier(ti, s.dist, s.behind, wantBehind)) cands.Add(s);
                if (cands.Count == 0) continue;
                for (int i = 0; i < Math.Min(6, cands.Count); i++)
                {
                    var c = cands[(int)Math.Floor(rng.Next() * cands.Count)];
                    if (DirectorMath.OutOfView(p.x, p.z, yaw, c.n.x, c.n.z, viewHalfAngle, VisibleFromPlayer(c.n))) return (c.n, c.dist);
                }
                if (ti != 0)
                {
                    var c = cands[(int)Math.Floor(rng.Next() * cands.Count)];
                    return (c.n, c.dist);
                }
            }
            return null;
        }

        /// <summary>The node tiers, best first: in range (and behind, when asked), in range, past the fallback minimum, any.</summary>
        bool Tier(int ti, double dist, bool behind, bool wantBehind)
        {
            switch (ti)
            {
                case 0: return dist >= spawnMin && dist <= spawnMax && (!wantBehind || behind);
                case 1: return dist >= spawnMin && dist <= spawnMax;
                case 2: return dist >= spawnFallbackMin;
                default: return true;
            }
        }

        bool SpawnSquad(List<string> kinds)
        {
            if (On20) return SpawnSquad20(kinds); // NIGHT20: design.director.n20's picks (Core/Enemies/Director20.cs)
            var pick = PickNode(!behindDone);
            if (!pick.HasValue) return false;
            Vec3 p = playerPos();
            if (DirectorMath.Behind(p.x, p.z, playerYaw(), pick.Value.n.x, pick.Value.n.z)) behindDone = true;
            telegraphs.Add(new Telegraph { point = new Vec3(pick.Value.n.x, pick.Value.n.y, pick.Value.n.z), kinds = kinds, steps = telegraphSteps });
            events.Emit("spawnTelegraph", new SpawnEvent { kind = kinds[0], position = new Vec3(pick.Value.n.x, pick.Value.n.y, pick.Value.n.z) });
            squadsSpawned++;
            return true;
        }

        void SpawnGroup(Telegraph t)
        {
            Vec3 target = playerPos();
            for (int i = 0; i < t.kinds.Count; i++)
            {
                double angle = (double)i / t.kinds.Count * Math.PI * 2;
                double ring = i == 0 ? 0 : groupSpread;
                var at = new Vec3(t.point.x + JsMath.Cos(angle) * ring, t.point.y, t.point.z + JsMath.Sin(angle) * ring);
                // main.js validSpawn: an offset with no route to you on the nav grid uses the point. NIGHT7 fix of a web bug
                // (logged): the web's check takes the nearest node within a cell, so an offset inside a 2 m wall (a node next
                // to it) passed, the 0.4 m capsule fits inside the wall, and the husk fell through it; here it needs a node
                // in its own column.
                int node = nav.grid.NodeAt(at.x, at.y, at.z, cfg.Num("ai.grid.lookBelow"), cfg.Num("ai.grid.lookAbove"));
                bool valid = node >= 0 && nav.DistanceAt(at.x, at.y, at.z) < double.PositiveInfinity;
                string kind = t.kinds[i];
                Husk h = (valid ? husks.Spawn(kind, at, target) : null) ?? husks.Spawn(kind, t.point, target);
                if (h == null) continue;
                h.hp = t.hps != null ? t.hps[i] : JsMath.Round(h.hp * hpMult); // main.js onSpawn: rounded, maxHp stays the kind's (NIGHT20: a recycled walker's own HP)
                if (n19.enemies) n19OnSpawn?.Invoke(h); // NIGHT19: affixes.onSpawn, after the HP scale
                if (t.elite) n20OnElite?.Invoke(h); // NIGHT20 A4: THE ELITE's affixes and name (SpecialRounds.cs)
                if (h is Drone dr) Drone20(dr); // NIGHT20: the drone's climb rules with design.director.n20 (Director20.cs)
                if (!(h is Lancer)) brain.Attach(h); // NIGHT20: the brain has no stationary kind yet; a lancer runs lancer.js's own path (Lancer.cs)
            }
        }

        /// <summary>Plan round `n` (the ported kinds) and send its opening squads.</summary>
        public void StartWave(int n, uint seed)
        {
            if (!active) NewRun();
            round = n;
            rng = new Mulberry32(seed + n * seedStride);
            paceRng = new Mulberry32(seed + n * seedStride + paceSeedOffset);
            paceNext = paceRng.Next;
            hpMult = DirectorMath.RoundHpMult(n, d);
            queue.Clear();
            bool lancer = LancerOn, warden = WardenOn; // NIGHT20: design.adds.lancer / .warden deal the web's LANCER / WARDEN from director.rounds.introduce
            if (!(bossRound != null && bossRound(n))) // NIGHT20 A1: director.js `queue = boss ? [] : planSquads(...)`
            queue.AddRange(DirectorMath.PlanSquads(n, rng.Next, d, n19.enemies || lancer || warden ? (Func<string, bool>)(k => Spawnable(k, n19, lancer, warden)) : Available)); // NIGHT19: the ported kinds with the block on
            behindDone = false;
            squadsSpawned = 0;
            interval = squadIntervalOf != null ? squadIntervalOf(n, squadInterval) : squadInterval;
            cooldown = 0;
            for (int i = 0; i < openingSquads && queue.Count > 0; i++)
            {
                if (SpawnSquad(queue[0])) queue.RemoveAt(0);
            }
            cooldown = MovementMath.StepsFor(interval, cfg.Dt);
            events.Emit("roundPlan", new RoundEvent { number = n });
        }

        /// <summary>
        /// director.js addSquads: add squads to the current round (the boss's adds, S7; protocol.js's SURGE). Port difference
        /// (logged in the header): a kind with no port yet (Available false) becomes a "husk", the planner's fallback kind,
        /// so the squad keeps its size and its ported kinds. The caller's lists are not changed (each squad is a new list).
        /// </summary>
        public void AddSquads(List<List<string>> list)
        {
            bool lancer = LancerOn, warden = WardenOn; // NIGHT20: the rows' LANCER / WARDEN stay themselves
            foreach (List<string> s in list) queue.Add(lancer || warden ? PortedSquad20(s, lancer, warden) : PortedSquad(s, n19)); // NIGHT19: Spawnable (Available with the block off)
        }

        public void Step(double dt)
        {
            if (!active) return;
            time += dt;
            // The telegraphs land (spawner.js step).
            for (int i = telegraphs.Count - 1; i >= 0; i--)
            {
                if (--telegraphs[i].steps <= 0)
                {
                    SpawnGroup(telegraphs[i]);
                    telegraphs.RemoveAt(i);
                }
            }
            while (dealt.Count > 0 && time - dealt[0].t > dealtWindow) dealt.RemoveAt(0);
            while (kills.Count > 0 && time - kills[0] > killChainTime) kills.RemoveAt(0);
            double aggression = Math.Min(1, playerSpeed() / aggressiveSpeed + nearbyKills * 0.5);
            string changed = peakFade
                ? DirectorMath.StepDirectorFade(state, dt, damageTaken, nearbyKills, aggression, paceNext, d, fade, husks.EngagedNear(playerPos(), fade.engagedRange))
                : DirectorMath.StepDirector(state, dt, damageTaken, nearbyKills, aggression, paceNext, d);
            if (changed != null) events.Emit("directorPhase", changed);
            double dealtRecent = 0;
            foreach (var e in dealt) dealtRecent += e.amount;
            Vec3 p = playerPos();
            var edges = On20 ? StepCamp20(dt, p, dealtRecent, kills.Count >= killChain) // NIGHT20: the height-aware camp score
                : DirectorMath.StepCamp(state, dt, p.x, p.z, husks.Count, dealtRecent, kills.Count >= killChain, d);
            if (kills.Count >= killChain) kills.Clear();
            if (edges.started) events.Emit("campStart", state.camp);
            if (edges.ended) events.Emit("campEnd", state.camp);
            if (edges.hunted)
            {
                // director.js spawnHunters: the hunter squad's ported kinds (six swarmers; the charger isn't ported), four
                // husks if fewer than 3 exist, never past maxAlive, from behind.
                var hunters = n19.enemies ? hunterSquad.FindAll(k => Spawnable(k, n19)) : hunterSquad.FindAll(Available); // NIGHT19: the CHARGER joins with the block on
                if (hunters.Count < 3) hunters = new List<string> { "husk", "husk", "husk", "husk" };
                int room = (int)Math.Max(0, maxAlive - (husks.Count + Telegraphed()));
                if (room > 0)
                {
                    behindDone = false;
                    hunters = hunters.GetRange(0, Math.Min(room, hunters.Count));
                    spawningHunters = true; // NIGHT20 (design.director.n20): perched, the hunters come up (Director20.cs)
                    SpawnSquad(hunters);
                    spawningHunters = false;
                    events.Emit("hunters", string.Join(",", hunters));
                }
            }
            damageTaken = 0;
            nearbyKills = 0;
            if (On20) Recycle20(); // NIGHT20: the unreachable failsafe (Director20.cs)
            if (cooldown > 0) cooldown--;
            double mult = fullPace ? 1 : DirectorMath.PhaseSpawnMult(state.phase, d); // fullPace: NIGHT11 design.exfil (port-only)
            if (queue.Count > 0 && cooldown <= 0 && mult > 0 && husks.Count + Telegraphed() + queue[0].Count <= maxAlive)
            {
                if (SpawnSquad(queue[0])) queue.RemoveAt(0);
                cooldown = MovementMath.StepsFor(interval / mult, dt);
            }
        }
    }

    /// <summary>survival/buyables.js: hold BUY (buy.holdTime) at the nearest thing for sale in reach and in front.</summary>
    public sealed class Buyables : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the hold in progress (the candidate by its place in the list, the hold and
        // denied clocks, the latch). The items themselves are the run's structure (Init's, then StartRun's relays and
        // traps): their ids are saved and checked, so a load into a run built differently fails instead of misplacing.
        public void Save(StateWriter w)
        {
            var ids = new List<string>(list.Count);
            foreach (Item it in list) ids.Add(it.id);
            w.Strings("items", ids);
            w.I("candidate", candidate == null ? -1 : list.IndexOf(candidate));
            w.I("holdSteps", holdSteps);
            w.I("deniedSteps", deniedSteps);
            w.B("latched", latched);
        }

        public void Load(StateReader r)
        {
            List<string> ids = r.Strings("items");
            bool same = ids.Count == list.Count;
            for (int i = 0; same && i < ids.Count; i++) same = ids[i] == list[i].id;
            if (!same) throw new StateException($"snapshot: the buy list differs (saved [{string.Join(", ", ids)}]; the run's has {list.Count}): restore after StartRun on the same map and config");
            int c = r.Int("candidate");
            if (c < -1 || c >= list.Count) throw new StateException($"snapshot: buyables.candidate {c}");
            candidate = c < 0 ? null : list[c];
            holdSteps = r.Int("holdSteps");
            deniedSteps = r.Int("deniedSteps");
            latched = r.B("latched");
        }

        public sealed class Item
        {
            public string id, name, kind;
            public Vec3 position;
            public double reach;
            public Func<double> price;
            public Func<bool> available;
            /// <summary>buyables.js owned?() (NIGHT11: protocol.js's lit relays): already bought; a hold is denied (buyDenied), nothing is spent. Null: never.</summary>
            public Func<bool> owned;
            public Action buy;
        }

        readonly GameConfig cfg;
        readonly EventBus events;
        readonly Economy economy;
        public readonly List<Item> list = new List<Item>();
        public Item candidate;
        int holdSteps, deniedSteps;
        bool latched;
        readonly double holdTime, reach, reachHeight, coneDeg, deniedTime;

        public Buyables(GameConfig cfg, EventBus events, Economy economy)
        {
            this.cfg = cfg;
            this.events = events;
            this.economy = economy;
            holdTime = cfg.Num("buy.holdTime");
            reach = cfg.Num("buy.reach");
            reachHeight = cfg.Num("buy.reachHeight");
            coneDeg = cfg.Num("buy.coneDeg");
            deniedTime = cfg.Num("buy.deniedFlashTime");
        }

        public bool Denied => deniedSteps > 0;

        /// <summary>buyables.js remove (NIGHT11: protocol.js's relays on a new run): drop an item, and the candidate if it was that one.</summary>
        public void Remove(Item item)
        {
            int i = list.IndexOf(item);
            if (i >= 0) list.RemoveAt(i);
            if (candidate == item) candidate = null;
        }

        public double Progress => Math.Min(1, holdSteps / (double)MovementMath.StepsFor(holdTime, cfg.Dt));

        Item Pick(Vec3 p, double yaw)
        {
            double fx = -JsMath.Sin(yaw), fz = -JsMath.Cos(yaw);
            double cos = JsMath.Cos(coneDeg * Math.PI / 180);
            Item best = null;
            double bestScore = double.PositiveInfinity;
            foreach (Item item in list)
            {
                if (item.available != null && !item.available()) continue;
                double dx = item.position.x - p.x, dz = item.position.z - p.z;
                double dd = JsMath.Hypot(dx, dz);
                if (dd > (item.reach > 0 ? item.reach : reach) || Math.Abs(item.position.y - p.y) > reachHeight) continue;
                double facing = dd < 0.3 ? 1 : (dx * fx + dz * fz) / dd;
                if (facing < cos) continue;
                double score = dd * (2 - facing);
                if (score < bestScore)
                {
                    best = item;
                    bestScore = score;
                }
            }
            return best;
        }

        public void Step(Vec3 feet, double yaw, bool interactHeld, double dt)
        {
            if (deniedSteps > 0) deniedSteps--;
            Item next = Pick(feet, yaw);
            if (next != candidate) holdSteps = 0;
            candidate = next;
            if (!interactHeld)
            {
                latched = false;
                holdSteps = 0;
                return;
            }
            if (candidate == null || latched) return;
            holdSteps++;
            if (holdSteps >= MovementMath.StepsFor(holdTime, dt))
            {
                latched = true;
                holdSteps = 0;
                double price = economy.Price(candidate.price()); // a pending REFUND halves it (buyables.js)
                if (candidate.owned != null && candidate.owned()) // buyables.js attempt: owned (NIGHT11: a lit relay)
                {
                    events.Emit("buyDenied", candidate.id);
                    return;
                }
                if (!economy.Spend(price, candidate.id))
                {
                    deniedSteps = MovementMath.StepsFor(deniedTime, dt);
                    events.Emit("buyDenied", candidate.id);
                    return;
                }
                candidate.buy();
                events.Emit("buy", new SpentEvent { price = price, what = candidate.name, total = economy.total });
            }
        }
    }
}
