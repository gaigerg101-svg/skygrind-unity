// Port of skygrind/src/modes/survival.js: the round loop of a Survival run. A short first break, then the round's
// enemies; clear them all to advance (+100 x round), a break (survival.breakTime), the next round. Death ends the run.
// The spawner is the director (Director implements IRoundSpawner). Emits roundBreak, roundBegin, roundClear, runEnd.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public interface IRoundSpawner
    {
        void StartWave(int round, uint seed);
        void Step(double dt);
        bool Done { get; }
        void Clear();
    }

    public sealed class RoundEvent
    {
        public int number;
        public double time;
        public double points;
    }

    public sealed class RunStats : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot (SurvivalRun's "stats" section): every field, lists in order.
        public void Save(StateWriter w)
        {
            w.S("mode", mode);
            w.I("round", round);
            w.I("cleared", cleared);
            w.D("points", points);
            w.D("earned", earned);
            w.D("spent", spent);
            w.D("time", time);
            w.I("kills", kills);
            w.I("headshots", headshots);
            w.I("meleeKills", meleeKills);
            w.I("shots", shots);
            w.I("shotsHit", shotsHit);
            w.Strings("zones", zones);
            w.Strings("forged", forged);
            w.S("killedBy", killedBy);
            w.D("style", style);
            w.S("bestRank", bestRank);
            w.I("bestRankIndex", bestRankIndex);
            w.S("grade", grade);
            w.S("line", line);
            w.B("hasProtocol", hasProtocol);
            w.I("protocolLit", protocolLit);
            w.I("protocolTotal", protocolTotal);
            w.B("protocolComplete", protocolComplete);
            w.S("bestAwardName", bestAwardName);
            w.D("bestAwardPoints", bestAwardPoints);
            w.D("longestSwing", longestSwing);
            w.D("topSpeed", topSpeed);
            w.I("bestMulti", bestMulti);
            w.S("exfil", exfil);
            w.I("heat", heat);
            w.D("scoreMult", scoreMult);
            w.Strings("mutators", mutators);
            w.I("seed", seed);
            w.S("seedCode", seedCode);
            w.B("revived", revived);
            w.B("recorded", recorded);
            w.Strings("contractsDone", contractsDone);
        }

        public void Load(StateReader r)
        {
            mode = r.S("mode");
            round = r.Int("round");
            cleared = r.Int("cleared");
            points = r.D("points");
            earned = r.D("earned");
            spent = r.D("spent");
            time = r.D("time");
            kills = r.Int("kills");
            headshots = r.Int("headshots");
            meleeKills = r.Int("meleeKills");
            shots = r.Int("shots");
            shotsHit = r.Int("shotsHit");
            List<string> z = r.Strings("zones"), f = r.Strings("forged"), m = r.Strings("mutators"), c = r.Strings("contractsDone");
            zones.Clear();
            zones.AddRange(z);
            forged.Clear();
            forged.AddRange(f);
            killedBy = r.S("killedBy");
            style = r.D("style");
            bestRank = r.S("bestRank");
            bestRankIndex = r.Int("bestRankIndex");
            grade = r.S("grade");
            line = r.S("line");
            hasProtocol = r.B("hasProtocol");
            protocolLit = r.Int("protocolLit");
            protocolTotal = r.Int("protocolTotal");
            protocolComplete = r.B("protocolComplete");
            bestAwardName = r.S("bestAwardName");
            bestAwardPoints = r.D("bestAwardPoints");
            longestSwing = r.D("longestSwing");
            topSpeed = r.D("topSpeed");
            bestMulti = r.Int("bestMulti");
            exfil = r.S("exfil");
            heat = r.Int("heat");
            scoreMult = r.D("scoreMult");
            mutators.Clear();
            mutators.AddRange(m);
            seed = r.U("seed");
            seedCode = r.S("seedCode");
            revived = r.B("revived");
            recorded = r.B("recorded");
            contractsDone.Clear();
            contractsDone.AddRange(c);
        }

        public string mode = "survival";
        public int round;
        public int cleared;
        public double points;
        public double earned;
        public double spent;
        public double time;
        public int kills;
        public int headshots;
        public int meleeKills;
        public int shots;
        public int shotsHit;
        public double Accuracy => shots > 0 ? (double)shotsHit / shots : 0;
        public readonly List<string> zones = new List<string>();
        public readonly List<string> forged = new List<string>(); // NIGHT8 U2-17: the forged guns ("KESTREL+")
        public string killedBy;
        public double style; // the run's style points (the style meter's total)
        public string bestRank = "";
        // NIGHT11: the run's shape on RESULTS (main.js endRun: stats.grade, stats.moments, stats.protocol, stats.line).
        public int bestRankIndex; // the best style rank, 0..5 (gradeOf's bestRank)
        public string grade; // runMoments.js gradeOf: S / A / B / C / D
        public string line; // runMoments.js scoreLine
        public bool hasProtocol; // protocol.js summary() (null outside a quest run)
        public int protocolLit, protocolTotal;
        public bool protocolComplete;
        public string bestAwardName; // runMoments.js summary(): the biggest style award (null: none)
        public double bestAwardPoints, longestSwing, topSpeed;
        public int bestMulti;
        public string exfil; // design.exfil (port-only): null, "EXFIL" (a win) or "EXFIL FAILED"
        // NIGHT12 (the shared types): the meta's part of a run (RESULTS, the PAYOFF strip, the profile).
        public int heat; // the HEAT rung played (0: none; design.heat)
        public double scoreMult = 1; // the rung's score multiplier, shown on RESULTS (1: none)
        public readonly List<string> mutators = new List<string>(); // mutators.js active set (the rung's and the toggles')
        public uint seed; // the run seed
        public string seedCode; // seeds.js seedCode
        public bool revived; // a revive was used this run (design.grade.p needs none)
        public bool recorded; // the run changed the profile (not LAB, not noRecords)
        public readonly List<string> contractsDone = new List<string>(); // contracts completed this run (ids, in order)
    }

    public sealed class SurvivalRun : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot. Everything that changes in a run: the phase, the round, the counters, the
        // seed, READY's first round (the LAB may lower it at the start) and the stats (a new RunStats on load). The hooks
        // (holdRound, stepHook) are wiring.
        public void Save(StateWriter w)
        {
            w.I("state", (int)state);
            w.I("round", round);
            w.I("cleared", cleared);
            w.I("steps", steps);
            w.I("phaseSteps", phaseSteps);
            w.I("seed", seed);
            w.D("readyFrom", readyFrom);
            w.Sub("stats", stats);
        }

        public void Load(StateReader r)
        {
            int s = r.Int("state");
            if (s < (int)State.Idle || s > (int)State.Over) throw new StateException($"snapshot: run.state {s}");
            var st = new RunStats();
            r.Sub("stats", st);
            state = (State)s;
            round = r.Int("round");
            cleared = r.Int("cleared");
            steps = r.Int("steps");
            phaseSteps = r.Int("phaseSteps");
            seed = r.U("seed");
            readyFrom = r.D("readyFrom");
            stats = st;
        }

        public enum State { Idle, Break, Fight, Over }

        readonly GameConfig cfg;
        readonly EventBus events;
        readonly IRoundSpawner spawner;
        readonly Economy economy;
        readonly Regen regen;
        readonly double simHz;
        public State state = State.Idle;
        public int round;
        public int cleared;
        public int steps;
        public int phaseSteps;
        public uint seed = 1;
        public RunStats stats = new RunStats();

        public SurvivalRun(GameConfig cfg, EventBus events, IRoundSpawner spawner, Economy economy, Regen regen)
        {
            this.cfg = cfg;
            this.events = events;
            this.spawner = spawner;
            this.economy = economy;
            this.regen = regen;
            simHz = cfg.movement.loop.simHz;
            readyFrom = cfg.Num("survival.readyFrom");
            events.On<KillEvent>("kill", e =>
            {
                if (e.owner != "player" || !Live) return;
                stats.kills++;
                if (e.headshot) stats.headshots++;
                if (e.melee || e.weapon == "punch") stats.meleeKills++;
            });
            events.On("fire", _ =>
            {
                if (Live) stats.shots++;
            });
            events.On("shotHit", _ =>
            {
                if (Live) stats.shotsHit++;
            });
            events.On<GateOpenEvent>("gateOpen", e =>
            {
                if (Live) stats.zones.Add(e.name);
            });
            events.On<DeathEvent>("death", e =>
            {
                if (e.owner == "player" && Live) stats.killedBy = e.source;
            });
        }

        /// <summary>NIGHT11 (survival.js hooks.holdRound): while it returns true the round can't clear (a HOLD not held yet).</summary>
        public Func<bool> holdRound;

        /// <summary>NIGHT11 (survival.js hooks.step): each fight step, after the spawner's.</summary>
        public Action<double> stepHook;

        public bool Live => state != State.Idle && state != State.Over;

        public double Time => steps / simHz;

        /// <summary>Seconds left in the break (the HUD countdown).</summary>
        public double BreakLeft => state == State.Break ? phaseSteps / simHz : 0;

        void ToBreak(double seconds, double dt)
        {
            round++;
            state = State.Break;
            phaseSteps = MovementMath.StepsFor(seconds, dt);
            events.Emit("roundBreak", new RoundEvent { number = round, time = seconds });
        }

        void Begin()
        {
            state = State.Fight;
            spawner.StartWave(round, seed);
            events.Emit("roundBegin", new RoundEvent { number = round });
        }

        /// <summary>A new run: the start bank, round 1 after a short break.</summary>
        public void Start(uint runSeed)
        {
            state = State.Idle;
            round = cleared = steps = phaseSteps = 0;
            seed = runSeed;
            stats = new RunStats();
            spawner.Clear();
            economy.Start();
            regen.SetEnabled(true);
            ToBreak(cfg.Num("survival.firstBreakTime"), 1 / simHz);
        }

        /// <summary>The player died (or quit): stop and report.</summary>
        public RunStats End()
        {
            if (!Live) return null;
            state = State.Over;
            spawner.Clear();
            stats.round = round;
            stats.cleared = cleared;
            stats.points = economy.total;
            stats.earned = economy.earned;
            stats.spent = economy.spent;
            stats.time = Time;
            economy.Stop();
            regen.SetEnabled(false);
            events.Emit("runEnd", stats);
            return stats;
        }

        public void Step(double dt)
        {
            if (!Live) return;
            steps++;
            if (state == State.Break)
            {
                if (--phaseSteps <= 0) Begin();
                return;
            }
            spawner.Step(dt);
            stepHook?.Invoke(dt); // survival.js hooks.step (objectives.js's hold clock)
            if (spawner.Done && !(holdRound != null && holdRound()))
            {
                cleared++;
                economy.RoundClear(round);
                events.Emit("roundClear", new RoundEvent { number = round, points = economy.total });
                ToBreak(cfg.Num("survival.breakTime"), dt);
            }
        }

        /// <summary>
        /// Debug (the smoke, F9; NIGHT8): clear the field and start round `n` after the first break's length. No clear bonus,
        /// no roundClear event; the stats keep counting.
        /// </summary>
        public void DebugSkipTo(int n)
        {
            if (!Live || n < 1) return;
            spawner.Clear();
            round = n - 1;
            ToBreak(cfg.Num("survival.firstBreakTime"), 1 / simHz);
        }

        /// <summary>READY's first round: survival.readyFrom (NIGHT8 U2-16: the LAB's READY FROM ROUND 1 sets design.lab.ready.readyFrom).</summary>
        public double readyFrom;

        /// <summary>NIGHT6 READY: end the break now (from survival.readyFrom).</summary>
        public bool Ready()
        {
            if (state != State.Break || round < readyFrom || phaseSteps <= 1) return false;
            phaseSteps = 1;
            events.Emit("ready", new RoundEvent { number = round });
            return true;
        }
    }
}
