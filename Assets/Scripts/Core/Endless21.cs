// NIGHT21 U3-35.7 THE GRIND: a port of skygrind/src/modes/endless.js (frozen web, tag p17-night6), kept line for line with its
// comments. "Endless wave loop (SPEC 2.0 section 5): a 4 s break (full heal + stamina, then the WAVE N banner), the arena reshape
// hook, telegraphed spawns, clear the wave, repeat. Death ends the run. Score = style points + 100 x waves cleared. Fixed step only."
// The web numbers (breakTime 4, bannerTime 1.6, reshapeTime 1.5, pointsPerWave 100) are read from PortExport/config.json `endless`
// through GameConfig by the caller (Endless21.Params); nothing is typed here. Events keep the web's names: waveBreak, waveStart,
// waveClear, runEnd. Pure C# (no engine): the sim hooks it in Game/SurvivalSim.Grind21.cs, the tests drive it with a fake spawner.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>waveBreak { number, time } / waveStart { number } / waveClear { number, score } (the web's payloads).</summary>
    public sealed class WaveEvent
    {
        public int number;
        public double time;
        public double score;
    }

    /// <summary>endless.js stats(): the run's summary (runEnd's payload).</summary>
    public sealed class EndlessStats
    {
        public string mode = "endless";
        public double score;
        public int lastWave, cleared, kills, shots;
        public double time, accuracy, style;
        public string bestRank;
    }

    /// <summary>The web's createEndless deps: the player (health.reset, mover.restoreStamina), the style meter and the optional reshape.</summary>
    public sealed class EndlessDeps
    {
        public IRoundSpawner spawner;
        /// <summary>player.health.reset()</summary>
        public Action healthReset;
        /// <summary>player.mover.restoreStamina()</summary>
        public Action restoreStamina;
        /// <summary>style.start() / style.stop()</summary>
        public Action styleStart, styleStop;
        /// <summary>style.total, style.summary().bestRank</summary>
        public Func<double> styleTotal;
        public Func<string> styleBestRank;
        /// <summary>
        /// reshape starts the arena reshape and returns its length in s (Phase 2); without it the fight starts right after the break
        /// </summary>
        public Func<int, double> reshape;
    }

    public sealed class Endless21
    {
        /// <summary>config.endless: breakTime, bannerTime, reshapeTime, pointsPerWave.</summary>
        public struct Params
        {
            public double breakTime, bannerTime, reshapeTime, pointsPerWave;

            public static Params From(GameConfig cfg) => new Params
            {
                breakTime = cfg.Num("endless.breakTime"),
                bannerTime = cfg.Num("endless.bannerTime"),
                reshapeTime = cfg.Num("endless.reshapeTime"),
                pointsPerWave = cfg.Num("endless.pointsPerWave"),
            };
        }

        public enum State { Idle, Break, Reshape, Fight, Over } // 'idle' | 'break' | 'reshape' | 'fight' | 'over'

        readonly Params e;
        readonly double simHz;
        readonly EndlessDeps d;
        readonly Action<string, object> emit;

        // const run = { ... }
        State state = State.Idle;
        int current; // the wave number
        int cleared; // waves cleared
        int steps; // the whole run
        int phaseSteps; // left in the break / reshape
        uint seed = 1;
        int kills, shots, shotsHit;

        public Endless21(Params p, double simHz, EventBus events, EndlessDeps deps, Action<string, object> emit)
        {
            e = p;
            this.simHz = simHz;
            d = deps;
            this.emit = emit;
            if (events == null) return;
            // const mine = (fn) => (ev) => ev.owner === 'player' && run.state !== 'idle' && run.state !== 'over' && fn(ev);
            events.On<KillEvent>("kill", ev =>
            {
                if (ev.owner == "player" && Running) kills++;
            });
            events.On("fire", _ =>
            {
                if (Running) shots++;
            });
            // The web counts a revolver / shotgun shotResult with hits; the port's SurvivalRun counts the "shotHit" event the same way.
            events.On("shotHit", _ =>
            {
                if (Running) shotsHit++;
            });
        }

        bool Running => state != State.Idle && state != State.Over;

        void NextWave(double dt)
        {
            current++;
            state = State.Break;
            phaseSteps = MovementMath.StepsFor(e.breakTime, dt);
            d.healthReset();
            d.restoreStamina();
            emit("waveBreak", new WaveEvent { number = current, time = e.breakTime });
        }

        void StartFight()
        {
            state = State.Fight;
            d.spawner.StartWave(current, seed);
            emit("waveStart", new WaveEvent { number = current });
        }

        public State Phase => state;
        public int CurrentWave => current;
        public int Cleared => cleared;
        /// <summary>Steps left in the break / reshape (the sim mirrors it into the round clock's countdown).</summary>
        public int PhaseSteps => phaseSteps;
        public double Score => d.styleTotal() + e.pointsPerWave * cleared;
        public double Time => steps / simHz;

        /// <summary>A new run (the seed makes every wave's mix repeatable).</summary>
        public void Start(uint seed)
        {
            state = State.Idle;
            current = cleared = steps = phaseSteps = 0;
            this.seed = seed;
            kills = shots = shotsHit = 0;
            d.spawner.Clear();
            d.styleStart?.Invoke();
            NextWave(1 / simHz);
        }

        /// <summary>The player died: stop and report.</summary>
        public EndlessStats End()
        {
            if (state == State.Over || state == State.Idle) return null;
            state = State.Over;
            d.spawner.Clear();
            d.styleStop?.Invoke();
            EndlessStats stats = Stats();
            emit("runEnd", stats);
            return stats;
        }

        public EndlessStats Stats() => new EndlessStats
        {
            score = Score,
            lastWave = current,
            cleared = cleared,
            time = Time,
            kills = kills,
            shots = shots,
            accuracy = shots > 0 ? (double)shotsHit / shots : 0,
            bestRank = d.styleBestRank?.Invoke(),
            style = d.styleTotal(),
        };

        /// <summary>One fixed step.</summary>
        public void Step(double dt)
        {
            if (state == State.Idle || state == State.Over) return;
            steps++;
            if (state == State.Break || state == State.Reshape)
            {
                if (--phaseSteps > 0) return;
                if (state == State.Break && d.reshape != null)
                {
                    double time = d.reshape(current);
                    if (time > 0)
                    {
                        state = State.Reshape;
                        phaseSteps = MovementMath.StepsFor(time, dt);
                        return;
                    }
                }
                StartFight();
                return;
            }
            d.spawner.Step(dt);
            if (d.spawner.Done)
            {
                cleared++;
                emit("waveClear", new WaveEvent { number = current, score = Score });
                NextWave(dt);
            }
        }

        /// <summary>Skip to a wave (debug presets): clears the current one and starts the break before `n`.</summary>
        public void DebugSetWave(int n)
        {
            d.spawner.Clear();
            current = n - 1;
            cleared = n - 1;
            NextWave(1 / simHz);
        }

        // ---- Unity-only: the core's reshape (design.n21.grind), pure so the tests can run it -----------------------------------

        /// <summary>
        /// The reshape hook's pick (THE GRIND, Unity-only; the web left `reshape` as "Phase 2"): the next preset out of `presets`,
        /// drawn on the run's Mulberry32, never the one the core already wears and, with `used`, none twice in a lap of the list (so the heights differ from wave to wave, and every shape shows once before one returns). Null: no presets.
        /// </summary>
        public static string PickPreset(IList<string> presets, string last, Mulberry32 rng, ISet<string> used = null)
        {
            var pool = new List<string>();
            foreach (string p in presets)
                if (p != last && (used == null || !used.Contains(p))) pool.Add(p);
            if (pool.Count == 0 && used != null && used.Count > 0) // every shape worn once: a new lap (the shape on now stays out)
            {
                used.Clear();
                foreach (string p in presets)
                    if (p != last) pool.Add(p);
            }
            if (pool.Count == 0) return null;
            int i = (int)Math.Floor(rng.Next() * pool.Count);
            string pick = pool[i >= pool.Count ? pool.Count - 1 : i];
            used?.Add(pick);
            return pick;
        }

        // ---- NIGHT22 U3-36.6: the rising-edge telegraph (ULTRAKILL's Cyber Grind announces what is coming; R6) ------------------------------

        /// <summary>The pillars the next shape lifts: `next` is at least `minRise` above `now` (the sim passes the core's heightStep, the web's number).</summary>
        public static bool[] Rising(IList<double> now, IList<double> next, double minRise)
        {
            var r = new bool[Math.Min(now.Count, next.Count)];
            for (int i = 0; i < r.Length; i++) r[i] = next[i] - now[i] >= minRise;
            return r;
        }

        /// <summary>The marks are shown for the last `seconds` of the break and through the reshape, never in the fight or outside a run.</summary>
        public bool Telegraphing(double seconds, double dt)
        {
            if (state == State.Reshape) return true;
            return state == State.Break && phaseSteps <= MovementMath.StepsFor(seconds, dt);
        }

        /// <summary>FNV-1a over the heights' bits: the harness's "the core's heights differ on every wave" check.</summary>
        public static ulong HeightsHash(IList<double> heights)
        {
            ulong h = 14695981039346656037UL;
            foreach (double v in heights)
            {
                ulong b = unchecked((ulong)BitConverter.DoubleToInt64Bits(v));
                for (int i = 0; i < 8; i++)
                {
                    h ^= (b >> (8 * i)) & 0xff;
                    h *= 1099511628211UL;
                }
            }
            return h;
        }
    }
}
