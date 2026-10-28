// NIGHT14 U3-27.6: THE FARM. Seeded AUTOPILOT runs, each on a FRESH headless composition (SurvivalSim.Headless: never a
// reused one, state carries across runs), at real HP (no god mode: NIGHT14 rule 6), the meta silent (rule 7), stepped
// until RESULTS or design.farm.maxMinutes of sim time ("timeout"; the run is then ended as a quit would end it). Nothing
// opens a MODS draft headless, so the loop takes them (AutopilotInput.TakeDraft). One JSONL line per run, appended and
// flushed per line (PlayerChecks' -skygrindFarm, Tools/SimHarness's farm; Tools/farm-report.cjs reads them):
//   seed, difficulty, build stamp, the brain (stand-in or the ported BotBrain), round reached, rounds cleared, sim time,
//   the cause of DOWN (the recap's killer name, or "exfil", "exfil failed", "timeout") and the hurt source behind it,
//   points earned and spent per round, guns bought (round, slot), kills per gun, perks / mods / THE FORGE / gates with their
//   rounds, the style rank histogram (steps per rank), downs, exfil, grade, shots and hits, the shopping entries dropped.
// The run's numbers come from the sim's events, subscribed once per composition (FarmCollector; the payload classes'
// field names). Per run it allocates per event, never per step. A report only: GG owns the numbers (D-55).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Farm
    {
        /// <summary>The build stamp each JSONL line carries (the harness sets "harness"; the Player's PlayerChecks sets its own).</summary>
        public static string BuildStamp = "unknown";

        /// <summary>BotNav's disk cache under the temp folder when the Game set none (a folder name, not a gameplay number).</summary>
        const string NavCacheFolder = "skygrind-botnav";

        /// <summary>
        /// NIGHT20 P0-3: one round's pace: its number and type (DirectorMath.RoundType; "exfil" for the round whose exfil the
        /// bot called), the break before it and its fight (s), the fight's seconds by director phase (BUILD_UP, PEAK, PEAK_FADE,
        /// RELAX: they add up to the fight), its downs and the damage the player took in it.
        /// </summary>
        public sealed class PaceRow
        {
            public int round, downs;
            public string type;
            public double brk, fight, build, peak, fade, relax, hurt;
        }

        /// <summary>NIGHT20 A3: one beacon of the run: the round it stood before, whether the bot took it, the elites its round
        /// called, whether its elite died (the `specialRound` elite "end" in that round: the reveal earned) and the round cleared.</summary>
        public sealed class BeaconRow
        {
            public int round, elites;
            public bool taken, eliteKilled, cleared;
        }

        /// <summary>NIGHT20 A3: the share of standing beacons the farm's bot takes (THE BEACON's measure; 0: never, so every
        /// other judge's farm is unchanged). RunOne's own argument wins when given.</summary>
        public static double BeaconShare;

        /// <summary>NIGHT20 E2 (ASSIST's measure): -1 (the default): off. n >= 0: the run takes ASSIST's damage at the resistance
        /// of n saved downs (Core/Assist20.cs: TakenMult; Game/Assist20Run.cs applies the same at the run's start for a human's
        /// run with the setting on; a bot's run never has it, so the farm applies it here, after HEAT's ApplyRung as there).</summary>
        public static int AssistDowns = -1;

        /// <summary>NIGHT20 (a depth measure, THE MARATHON's choice): >= 0 replaces the honest bot's exfilFrom (0: it never
        /// calls the exfil, so the run's round measures depth); -1 (the default): design.autopilot.n20.exfilFrom.</summary>
        public static int ExfilFromOverride = -1;

        /// <summary>One run's JSONL line (U3-27.6).</summary>
        public sealed class Record
        {
            /// <summary>NIGHT20 A3: the run's beacons (farm-report.cjs's beacon table).</summary>
            public readonly List<BeaconRow> beacons = new List<BeaconRow>();
            /// <summary>NIGHT20 P0-3: the pacing trace, one row per round reached (farm-report.cjs's pacing table).</summary>
            public readonly List<PaceRow> pace = new List<PaceRow>();
            public uint seed;
            public string difficulty, build, cause;
            public int round, kills, steps;
            public double simTime;
            public string brain, killer, exfil, grade;
            /// <summary>The last round's objective (objectives.js: normal | hold | gold) and whether its HOLD was still not held.</summary>
            public string roundType;
            public bool holding;
            public int cleared, downs, shots, hits, dropped;
            public double earned, spent;
            public readonly List<double> earnedByRound = new List<double>();
            public readonly List<double> spentByRound = new List<double>();
            public readonly List<(int round, int slot, string id)> guns = new List<(int, int, string)>();
            public readonly SortedDictionary<string, int> killsByGun = new SortedDictionary<string, int>(StringComparer.Ordinal);
            public readonly List<(string id, int round, bool free)> perks = new List<(string, int, bool)>();
            public readonly List<(string id, int round, bool free)> mods = new List<(string, int, bool)>();
            public readonly List<(string id, int round, bool free)> forge = new List<(string, int, bool)>();
            public readonly List<(string id, int round, bool free)> gates = new List<(string, int, bool)>();
            /// <summary>NIGHT20 P0-2: the round whose exfil the bot called (exfilCall; 0: none).</summary>
            public int exfilCalled;
            /// <summary>NIGHT20 P0-2: each HOLD round: its steps while the hold ran after the bot first stood inside (the game's
            /// own test), and how many of them it was inside (U3-28.3's judge: >= 90 %).</summary>
            public readonly List<(int round, int steps, int inside)> holdRounds = new List<(int, int, int)>();
            public long[] rankSteps = new long[0];
            public List<string> rankNames = new List<string>();

            static string N(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);

            static void S(StringBuilder b, string s)
            {
                if (s == null)
                {
                    b.Append("null");
                    return;
                }
                b.Append('"');
                foreach (char c in s)
                {
                    if (c == '"' || c == '\\') b.Append('\\').Append(c);
                    else if (c < ' ') b.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    else b.Append(c);
                }
                b.Append('"');
            }

            static void Picks(StringBuilder b, string key, List<(string id, int round, bool free)> list)
            {
                b.Append(",\"").Append(key).Append("\":[");
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) b.Append(',');
                    b.Append("{\"id\":");
                    S(b, list[i].id);
                    b.Append(",\"round\":").Append(list[i].round);
                    if (list[i].free) b.Append(",\"free\":true");
                    b.Append('}');
                }
                b.Append(']');
            }

            static void Nums(StringBuilder b, string key, List<double> list)
            {
                b.Append(",\"").Append(key).Append("\":[");
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) b.Append(',');
                    b.Append(N(list[i]));
                }
                b.Append(']');
            }

            /// <summary>The JSONL line (one line, no newline).</summary>
            public string ToJson()
            {
                var b = new StringBuilder(512);
                b.Append("{\"seed\":").Append(seed);
                b.Append(",\"difficulty\":");
                S(b, difficulty);
                b.Append(",\"build\":");
                S(b, build);
                b.Append(",\"brain\":");
                S(b, brain);
                b.Append(",\"round\":").Append(round);
                b.Append(",\"cleared\":").Append(cleared);
                b.Append(",\"simTime\":").Append(N(simTime));
                b.Append(",\"steps\":").Append(steps);
                b.Append(",\"cause\":");
                S(b, cause);
                b.Append(",\"killer\":");
                S(b, killer);
                b.Append(",\"kills\":").Append(kills);
                b.Append(",\"earned\":").Append(N(earned));
                b.Append(",\"spent\":").Append(N(spent));
                Nums(b, "earnedByRound", earnedByRound);
                Nums(b, "spentByRound", spentByRound);
                b.Append(",\"guns\":[");
                for (int i = 0; i < guns.Count; i++)
                {
                    if (i > 0) b.Append(',');
                    b.Append("{\"id\":");
                    S(b, guns[i].id);
                    b.Append(",\"round\":").Append(guns[i].round).Append(",\"slot\":").Append(guns[i].slot).Append('}');
                }
                b.Append(']');
                b.Append(",\"killsByGun\":{");
                bool first = true;
                foreach (var kv in killsByGun)
                {
                    if (!first) b.Append(',');
                    first = false;
                    S(b, kv.Key);
                    b.Append(':').Append(kv.Value);
                }
                b.Append('}');
                Picks(b, "perks", perks);
                Picks(b, "mods", mods);
                Picks(b, "forge", forge);
                Picks(b, "gates", gates);
                b.Append(",\"rankSteps\":[");
                for (int i = 0; i < rankSteps.Length; i++)
                {
                    if (i > 0) b.Append(',');
                    b.Append(rankSteps[i]);
                }
                b.Append("],\"rankNames\":[");
                for (int i = 0; i < rankNames.Count; i++)
                {
                    if (i > 0) b.Append(',');
                    S(b, rankNames[i]);
                }
                b.Append(']');
                b.Append(",\"downs\":").Append(downs);
                b.Append(",\"exfil\":");
                S(b, exfil);
                b.Append(",\"grade\":");
                S(b, grade);
                b.Append(",\"shots\":").Append(shots);
                b.Append(",\"hits\":").Append(hits);
                b.Append(",\"shopDropped\":").Append(dropped);
                b.Append(",\"roundType\":");
                S(b, roundType);
                b.Append(",\"holding\":").Append(holding ? "true" : "false");
                b.Append(",\"pace\":["); // NIGHT20 P0-3
                for (int i = 0; i < pace.Count; i++)
                {
                    PaceRow p = pace[i];
                    if (i > 0) b.Append(',');
                    b.Append("{\"round\":").Append(p.round).Append(",\"type\":");
                    S(b, p.type);
                    b.Append(",\"break\":").Append(N(p.brk)).Append(",\"fight\":").Append(N(p.fight)).Append(",\"build\":").Append(N(p.build))
                        .Append(",\"peak\":").Append(N(p.peak)).Append(",\"fade\":").Append(N(p.fade)).Append(",\"relax\":").Append(N(p.relax))
                        .Append(",\"downs\":").Append(p.downs).Append(",\"hurt\":").Append(N(p.hurt)).Append('}');
                }
                b.Append(']');
                b.Append(",\"exfilCalled\":").Append(exfilCalled); // NIGHT20 P0-2
                b.Append(",\"holdRounds\":[");
                for (int i = 0; i < holdRounds.Count; i++)
                {
                    if (i > 0) b.Append(',');
                    b.Append("{\"round\":").Append(holdRounds[i].round).Append(",\"steps\":").Append(holdRounds[i].steps).Append(",\"inside\":").Append(holdRounds[i].inside).Append('}');
                }
                b.Append(']');
                b.Append(",\"beacons\":["); // NIGHT20 A3
                for (int i = 0; i < beacons.Count; i++)
                {
                    BeaconRow k = beacons[i];
                    if (i > 0) b.Append(',');
                    b.Append("{\"round\":").Append(k.round).Append(",\"taken\":").Append(k.taken ? "true" : "false").Append(",\"elites\":").Append(k.elites)
                        .Append(",\"eliteKilled\":").Append(k.eliteKilled ? "true" : "false").Append(",\"cleared\":").Append(k.cleared ? "true" : "false").Append('}');
                }
                b.Append(']');
                b.Append('}');
                return b.ToString();
            }
        }

        /// <summary>The run's numbers from the sim's events (subscribed once, on a fresh composition).</summary>
        sealed class FarmCollector
        {
            readonly SurvivalSim sim;
            readonly Record r;
            double earnedMark, spentMark;

            public FarmCollector(SurvivalSim sim, Record r)
            {
                this.sim = sim;
                this.r = r;
                EventBus ev = sim.events;
                ev.On<KillEvent>("kill", e =>
                {
                    if (e.owner != "player") return;
                    string w = e.weapon ?? "unknown";
                    r.killsByGun[w] = (r.killsByGun.TryGetValue(w, out int n) ? n : 0) + 1;
                    r.kills++;
                });
                ev.On<RoundEvent>("roundBreak", e =>
                {
                    if (e.number <= 1) return;
                    CloseRound();
                    ClosePace(e.number - 1); // NIGHT20 P0-3
                });
                ev.On<HurtEvent>("hurt", e =>
                {
                    if (e.owner == "player") pHurt += e.amount; // NIGHT20 P0-3
                });
                d = DirectorMath.From(sim.cfg);
                ev.On<string>("weaponBought", id => r.guns.Add((Round, sim.loadout.owned.IndexOf(id), id)));
                ev.On<PerkEvent>("perk", e => r.perks.Add((e.id, Round, e.free)));
                ev.On<ModEvent>("modPick", e => r.mods.Add((e.id, Round, e.free)));
                ev.On<ForgeEvent>("forge", e =>
                {
                    if (e.owner == "player") r.forge.Add((e.weapon, Round, e.free));
                });
                ev.On<GateOpenEvent>("gateOpen", e => r.gates.Add((e.zone, Round, e.free)));
                ev.On<DeathEvent>("death", e =>
                {
                    if (e.owner != "player") return;
                    r.downs++;
                    pDowns++; // NIGHT20 P0-3
                    r.killer = e.source;
                });
                ev.On<ExfilEvent>("exfilCall", e => r.exfilCalled = e.round); // NIGHT20 P0-2
                // NIGHT20 A3: the beacons (stood, taken, their round's elites, the elite's kill, the round's clear)
                ev.On<BeaconEvent>(Beacon20.StandEvent, e => r.beacons.Add(new BeaconRow { round = e.round }));
                ev.On<BeaconEvent>(Beacon20.TakeEvent, e =>
                {
                    BeaconRow k = BeaconOf(e.round);
                    if (k != null) k.taken = true;
                });
                ev.On<BeaconEvent>(Beacon20.EscortEvent, e =>
                {
                    BeaconRow k = BeaconOf(e.round);
                    if (k != null) k.elites = e.elites;
                });
                ev.On<SpecialRoundEvent>(N20Events.SpecialRound, e =>
                {
                    if (e.name != SpecialRounds.Elite || e.phase != "end") return;
                    BeaconRow k = BeaconOf(sim.run.round);
                    if (k != null && k.taken) k.eliteKilled = true;
                });
                ev.On<RoundEvent>("roundClear", e =>
                {
                    BeaconRow k = BeaconOf(e.number);
                    if (k != null) k.cleared = true;
                });
            }

            BeaconRow BeaconOf(int round)
            {
                for (int i = r.beacons.Count - 1; i >= 0; i--)
                    if (r.beacons[i].round == round) return r.beacons[i];
                return null;
            }

            // NIGHT20 P0-3: the round in progress's pace (steps: the break before it, its fight, the fight by director phase),
            // its downs and the damage taken; the director's round types.
            readonly DirectorMath.D d;
            int pBreak, pFight, pBuild, pPeak, pFade, pRelax, pDowns;
            double pHurt;

            /// <summary>Once a step (after sim.Step): the step into the break or the fight, the fight by director phase.</summary>
            public void StepPace()
            {
                SurvivalRun.State st = sim.run.state;
                if (st == SurvivalRun.State.Break)
                {
                    pBreak++;
                    return;
                }
                if (st != SurvivalRun.State.Fight) return;
                pFight++;
                string ph = sim.director.Phase; // Core's literals (DirectorMath: BUILD_UP, PEAK, PEAK_FADE, RELAX)
                if (ph == "PEAK") pPeak++;
                else if (ph == "PEAK_FADE") pFade++;
                else if (ph == "RELAX") pRelax++;
                else pBuild++;
            }

            /// <summary>Round `round`'s pace row (its break and fight so far), into the record; the counters start again.</summary>
            public void ClosePace(int round)
            {
                if (round < 1) return;
                double dt = sim.step;
                r.pace.Add(new PaceRow
                {
                    round = round, type = r.exfilCalled == round ? "exfil" : DirectorMath.RoundType(round, d),
                    brk = pBreak * dt, fight = pFight * dt, build = pBuild * dt, peak = pPeak * dt, fade = pFade * dt, relax = pRelax * dt,
                    downs = pDowns, hurt = pHurt,
                });
                pBreak = pFight = pBuild = pPeak = pFade = pRelax = pDowns = 0;
                pHurt = 0;
            }

            // NIGHT20 P0-2: the HOLD round in progress (its round, steps after the bot first stood inside, steps inside).
            int holdRound, holdSteps, holdInside;
            bool holdArrived;

            /// <summary>Once a step (after sim.Step): the HOLD's inside share, by the game's own test (Objectives.Inside).</summary>
            public void StepHold()
            {
                Objectives o = sim.objectives;
                if (!o.Holding) return;
                if (o.Round != holdRound)
                {
                    FlushHold();
                    holdRound = o.Round;
                }
                holdArrived |= o.Inside;
                if (!holdArrived) return;
                holdSteps++;
                if (o.Inside) holdInside++;
            }

            /// <summary>The HOLD round in progress, into the record (its round's end, or the run's).</summary>
            public void FlushHold()
            {
                if (holdRound > 0) r.holdRounds.Add((holdRound, holdSteps, holdInside));
                holdRound = holdSteps = holdInside = 0;
                holdArrived = false;
            }

            int Round => sim.run.round;

            /// <summary>The round just left: what it earned and spent (economy.earned / spent since its break began).</summary>
            public void CloseRound()
            {
                r.earnedByRound.Add(sim.economy.earned - earnedMark);
                r.spentByRound.Add(sim.economy.spent - spentMark);
                earnedMark = sim.economy.earned;
                spentMark = sim.economy.spent;
            }
        }

        /// <summary>One autopilot run on a fresh composition, to RESULTS or `maxSeconds` of sim time ("timeout"); a hash every
        /// `hashEvery` steps into `hashes` when given (the determinism check).</summary>
        /// NIGHT19 THE PARITY FARM: `lockGun` (a gun id) starts the run with that gun and the GAUNTLET, and the shopper never
        /// buys a gun (it still buys perks, the forge and gates); null: the run as always.
        /// NIGHT20 A3: the run has THE BEACON (Beacon20Ensure: idempotent, the sim's own once the lead's constructor line lands);
        /// `beaconShare` (&lt; 0: Farm.BeaconShare) of its beacons the bot takes (0: none, an ignored beacon changes nothing).
        public static Record RunOne(GameConfig cfg, SurvivalMap map, uint seed, string difficulty, double maxSeconds, List<string> hashes = null, int hashEvery = 600, string lockGun = null, double beaconShare = -1)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.Beacon20Ensure(); // NIGHT20 A3
            var ap = new AutopilotInput(sim, difficulty, seed);
            ap.TellTheTruth(); // NIGHT20 P0-2: the farm's bot is the honest one (design.autopilot.n20; off: NIGHT19's)
            if (ExfilFromOverride >= 0) ap.ExfilFrom = ExfilFromOverride;
            ap.Shopper.beaconShare = beaconShare < 0 ? BeaconShare : beaconShare; // NIGHT20 A3
            if (AssistDowns >= 0) // NIGHT20 E2: ASSIST's measure (RunStarted: after HEAT's ApplyRung, as Assist20Run)
            {
                double taken = Assist20.TakenMult(Assist20.P.From(cfg), AssistDowns);
                sim.RunStarted += () => sim.health.takenMult *= taken;
            }
            sim.UseInput(ap);
            var r = new Record { seed = seed, difficulty = difficulty, build = BuildStamp, brain = ap.StandIn ? "stand-in" : "botBrain" };
            var collect = new FarmCollector(sim, r);
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(seed, null, null);
            if (lockGun != null)
            {
                sim.loadout.Reset(new List<string> { lockGun, "gauntlet" }, seed);
                ap.Shopper.noGuns = true;
            }
            r.rankNames.AddRange(sim.style.Params.ranks);
            long[] ranks = r.rankSteps = new long[r.rankNames.Count];
            int maxSteps = MovementMath.StepsFor(maxSeconds, sim.step);
            int steps = 0;
            while (steps < maxSteps && sim.flow != Flow.Results)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                steps++;
                collect.StepHold(); // NIGHT20 P0-2
                collect.StepPace(); // NIGHT20 P0-3
                int rank = sim.style.rank;
                ranks[rank < 0 ? 0 : rank >= ranks.Length ? ranks.Length - 1 : rank]++;
                if (hashes != null && steps % hashEvery == 0) hashes.Add($"step {steps} hash {SimFingerprint.Hash(sim):x16}");
            }
            bool timeout = sim.flow != Flow.Results;
            int round = sim.run.round;
            r.roundType = sim.objectives.Active ? sim.objectives.Type : Objectives.Normal;
            r.holding = sim.objectives.Holding;
            if (timeout) sim.EndRun(); // a run that outlives the cap ends as a quit would (the stats judged, nothing stored)
            collect.CloseRound();
            collect.FlushHold(); // NIGHT20 P0-2
            collect.ClosePace(sim.run.round); // NIGHT20 P0-3: the last round reached
            RunStats st = sim.lastStats;
            r.steps = steps;
            r.simTime = steps * sim.step;
            r.round = st?.round ?? round;
            r.cleared = st?.cleared ?? sim.run.cleared;
            r.earned = sim.economy.earned;
            r.spent = sim.economy.spent;
            r.exfil = st?.exfil;
            r.grade = st?.grade;
            r.shots = st?.shots ?? 0;
            r.hits = st?.shotsHit ?? 0;
            r.dropped = ap.Shopper.Dropped;
            r.cause = timeout ? "timeout" : r.exfil == Exfil.Win ? "exfil" : r.exfil != null ? "exfil failed" : sim.lastDeath?.killer ?? "unknown";
            if (timeout || r.exfil == Exfil.Win) r.killer = null;
            ap.Detach();
            return r;
        }

        /// <summary>`runs` seeded runs from `seedStart` at `difficulty`, each on a fresh composition of (cfg, map), one JSONL line
        /// per run appended to `jsonlPath`; `log` gets a progress line per run. Returns the summary line. `maxMinutes` &lt; 0 reads
        /// design.farm.maxMinutes.</summary>
        public static string Run(GameConfig cfg, SurvivalMap map, int runs, string difficulty, uint seedStart, string jsonlPath, Action<string> log, double maxMinutes = -1)
        {
            if (!AutopilotInput.Enabled(cfg)) return "farm refused: design.autopilot.enabled is false";
            // agent N: without a disk cache every process pays BotNav's graph build (3 s CoreCLR); the farm's Player processes share one
            if (BotNav.CacheDir == null) BotNav.CacheDir = Path.Combine(Path.GetTempPath(), NavCacheFolder);
            if (maxMinutes < 0) maxMinutes = cfg.Num("design.farm.maxMinutes");
            string dir = Path.GetDirectoryName(Path.GetFullPath(jsonlPath));
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            var wall = System.Diagnostics.Stopwatch.StartNew();
            int done = 0, roundSum = 0, roundMax = 0, timeouts = 0, exfilCalls = 0; // NIGHT20 P0-2: exfilCalls
            string brain = "?";
            var causes = new SortedDictionary<string, int>(StringComparer.Ordinal);
            using (var w = new StreamWriter(jsonlPath, true, new UTF8Encoding(false)))
            {
                for (int i = 0; i < runs; i++)
                {
                    uint seed = unchecked(seedStart + (uint)i);
                    long t0 = wall.ElapsedMilliseconds;
                    Record r = RunOne(cfg, map, seed, difficulty, maxMinutes * 60);
                    w.Write(r.ToJson());
                    w.Write('\n');
                    w.Flush();
                    done++;
                    roundSum += r.round;
                    roundMax = Math.Max(roundMax, r.round);
                    if (r.cause == "timeout") timeouts++;
                    if (r.exfilCalled > 0) exfilCalls++;
                    causes[r.cause] = (causes.TryGetValue(r.cause, out int n) ? n : 0) + 1;
                    brain = r.brain;
                    log?.Invoke($"run {i + 1}/{runs} seed {seed} {difficulty}: round {r.round}, {r.simTime:0.0} s, {r.cause}, kills {r.kills} ({wall.ElapsedMilliseconds - t0} ms)");
                }
            }
            var parts = new List<string>();
            foreach (var kv in causes) parts.Add($"{kv.Key} {kv.Value}");
            return string.Format(CultureInfo.InvariantCulture, "farm {0} runs, {1}, seeds {2}..{3}, brain {4}: mean round {5:0.00}, max {6}, timeouts {7}; causes: {8}; exfil called in {11}; {9:0.0} s wall -> {10}",
                done, difficulty, seedStart, unchecked(seedStart + (uint)Math.Max(0, runs - 1)), brain, done > 0 ? (double)roundSum / done : 0, roundMax, timeouts, string.Join(", ", parts), wall.Elapsed.TotalSeconds, jsonlPath, exfilCalls);
        }
    }
}
