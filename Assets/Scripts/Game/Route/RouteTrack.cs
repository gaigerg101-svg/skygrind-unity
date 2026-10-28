// NIGHT14 U3-27.10 (agent V2): THE ROUTE, a whole-run coarse track (design.route; port-only: the web's RESULTS has no map).
// Unity-free (it runs on a headless composition in tests). What it keeps, all preallocated at construction for
// design.route.maxMinutes of run time, nothing allocated after:
// - a point every design.route.every s of RUN time (whole steps, MovementMath.StepsFor: 30 at 120 Hz): the feet (x, y, z)
//   and the horizontal speed (Mover.Speed). Point 0 is the spawn at the run's start (run step 0); point i is at run step
//   startStep + i * everySteps. The run's clock is SurvivalRun.steps (it advances only while the run plays: not in DOWN,
//   a pause or hitstop), so a track is a function of run time, which the GHOST replays by.
// - the player's kills (KillEvent owner "player" while the run is live) where they fell, and the kills per gun (the card's
//   TOP GUN); each round's start (roundBegin: its number, run step and place); the DOWN (the death's place, or where an
//   exfil failed on the clock); the exfil's site (exfilStart) and a win (exfilDone).
// - the run's seed, HEAT rung and DAILY flag (read at its start).
// Recorded after every step (SurvivalSim.StepEnded) and reset at every run's start (SurvivalSim.RunStarted). Once per ended
// run (EndRun set new RunStats: RESULTS) it raises Ended, before the next run's reset, whether the run ended inside a step
// (a DOWN's clock, an exfil) or outside one (R from DOWN, PAUSED's RESTART: EndRun then StartRun in one call).
// A RESUMED run (SAVE & QUIT's snapshot restored after StartRun) jumps the run's clock: the track restarts at the next
// sample step after the jump and is marked `resumed` (its route is partial: the GHOST never stores it).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class RouteTrack
    {
        readonly SurvivalSim sim;

        /// <summary>design.route.enabled: false records nothing (the old RESULTS).</summary>
        public readonly bool enabled;
        /// <summary>Run steps between points (design.route.every in whole steps) and the points kept (design.route.maxMinutes).</summary>
        public readonly int everySteps, capacity;
        /// <summary>The sim's steps per second (movement.loop.simHz): run steps to seconds.</summary>
        public readonly double simHz;
        /// <summary>The points: feet x, y, z (web space) and the horizontal speed (m/s), point i at run step startStep + i * everySteps.</summary>
        public readonly float[] px, py, pz, speed;
        public int count, startStep;
        /// <summary>resumed: the run's clock jumped (a restored save); full: maxMinutes of points are in, later ones are dropped.</summary>
        public bool resumed, full;
        public uint seed;
        public int heat;
        /// <summary>The run started as a bot's (metaSilent at its start: the GHOST never stores it).</summary>
        public bool metaSilentAtStart;
        /// <summary>The player's kills where they fell (killCount kept, at most capacity); kills: every player kill of the run.</summary>
        public readonly float[] killX, killZ;
        public int killCount, kills;
        /// <summary>Each round's start: its number, the run step it began at, where the player stood.</summary>
        public readonly int[] roundNumber, roundStep;
        public readonly float[] roundX, roundZ;
        public int roundCount;
        /// <summary>The DOWN: where the run died (or an exfil failed on the clock) and the run step.</summary>
        public bool down;
        public float downX, downZ;
        public int downStep;
        /// <summary>The exfil: its site (exfilStart) and whether it was won (exfilDone).</summary>
        public bool exfilSite, exfilWin;
        public float exfilX, exfilZ;
        /// <summary>Bumped at each reset (a new run's track): the RESULTS map redraws on a new version.</summary>
        public int Version;

        // Kills per gun (KillEvent.weapon), in first-kill order; a gun past the table's size is not counted.
        readonly string[] gunIds;
        readonly int[] gunKills;
        int gunCount;

        int nextStep, lastRunSteps;
        bool recording;
        RunStats endedStats; // the RunStats Ended last reported (or the one present when this run started)

        /// <summary>The run reached RESULTS (once per ended run), before the next run resets the track.</summary>
        public event Action<RunStats> Ended;

        /// <summary>From design.route (enabled, every, maxMinutes).</summary>
        public RouteTrack(SurvivalSim sim)
            : this(sim, sim.cfg.Design("route.enabled"), sim.cfg.Num("design.route.every"), sim.cfg.Num("design.route.maxMinutes"))
        {
        }

        public RouteTrack(SurvivalSim sim, bool enabled, double every, double maxMinutes)
        {
            this.sim = sim;
            this.enabled = enabled;
            everySteps = MovementMath.StepsFor(every, sim.step);
            simHz = sim.cfg.movement.loop.simHz;
            capacity = (int)Math.Ceiling(maxMinutes * 60 / every) + 1; // the spawn's point, then one per `every`
            px = new float[capacity];
            py = new float[capacity];
            pz = new float[capacity];
            speed = new float[capacity];
            killX = new float[capacity]; // at most one kept per point's worth of run time on average: a horde's kills fit
            killZ = new float[capacity];
            // A round lasts at least its break (survival.breakTime; the first, survival.firstBreakTime): no more rounds fit.
            int rounds = (int)Math.Ceiling(maxMinutes * 60 / Math.Max(1e-3, Math.Min(sim.cfg.Num("survival.breakTime"), sim.cfg.Num("survival.firstBreakTime")))) + 1;
            roundNumber = new int[rounds];
            roundStep = new int[rounds];
            roundX = new float[rounds];
            roundZ = new float[rounds];
            int guns = UnityTuning.RouteGunSlots;
            gunIds = new string[guns];
            gunKills = new int[guns];
            if (!enabled) return;
            EventBus ev = sim.events;
            ev.On<KillEvent>("kill", OnKill);
            ev.On<RoundEvent>("roundBegin", OnRoundBegin);
            ev.On<DeathEvent>("death", OnDeath);
            ev.On<ExfilEvent>("exfilStart", OnExfilStart);
            ev.On<ExfilEvent>("exfilDone", OnExfilDone);
            sim.StepEnded += Record;
            sim.RunStarted += OnRunStarted;
            endedStats = sim.lastStats;
        }

        /// <summary>The last round that began (0: none began).</summary>
        public int ReachedRound => roundCount > 0 ? roundNumber[roundCount - 1] : 0;

        /// <summary>The run step of the last point (startStep: one point or none).</summary>
        public int LastStep => startStep + Math.Max(0, count - 1) * everySteps;

        /// <summary>The gun with the most player kills this run (the first to reach that count on a tie), or null.</summary>
        public string TopGun(out int gunKillCount)
        {
            int best = -1;
            gunKillCount = 0;
            for (int i = 0; i < gunCount; i++)
            {
                if (gunKills[i] <= gunKillCount) continue;
                gunKillCount = gunKills[i];
                best = i;
            }
            return best >= 0 ? gunIds[best] : null;
        }

        /// <summary>A new run (RunStarted): report the last one if it ended unreported, then an empty track and point 0.</summary>
        void OnRunStarted()
        {
            CheckEnded();
            Reset();
        }

        /// <summary>An empty track for the run that just started (its seed, rung and DAILY; point 0 at the spawn).</summary>
        public void Reset()
        {
            Version++;
            count = killCount = kills = roundCount = gunCount = 0;
            for (int i = 0; i < gunKills.Length; i++)
            {
                gunKills[i] = 0;
                gunIds[i] = null;
            }
            down = exfilSite = exfilWin = resumed = full = false;
            downStep = 0;
            seed = sim.runSeed;
            RunStats st = sim.run.stats;
            heat = st.heat;
            metaSilentAtStart = sim.metaSilent;
            endedStats = sim.lastStats; // an ended run before this one was reported (or never will be)
            recording = enabled && sim.run.Live;
            lastRunSteps = sim.run.steps;
            startStep = nextStep = lastRunSteps;
            if (recording) Sample();
        }

        /// <summary>After each step (StepEnded): a point when the run's clock reaches the next sample, the DOWN, the run's end.</summary>
        public void Record()
        {
            if (!recording) return;
            SurvivalRun run = sim.run;
            int s = run.steps;
            if (s != lastRunSteps && s != lastRunSteps + 1)
            {
                // The clock jumped (a restored save): the route goes on from the next sample step, marked partial.
                resumed = true;
                count = 0;
                startStep = nextStep = (s + everySteps - 1) / everySteps * everySteps;
            }
            lastRunSteps = s;
            if (run.Live && s >= nextStep) Sample();
            if (!down && sim.flow == Flow.Down) MarkDown(sim.current);
            if (!ReferenceEquals(sim.lastStats, endedStats)) CheckEnded();
        }

        /// <summary>Raise Ended once for new RunStats (EndRun ran since the last report). Cheap: a reference compare.</summary>
        public void CheckEnded()
        {
            RunStats s = sim.lastStats;
            if (s == null || ReferenceEquals(s, endedStats)) return;
            endedStats = s;
            if (!recording) return;
            recording = false; // the run is over: nothing more is added (RESULTS reads the track as it ended)
            Ended?.Invoke(s);
        }

        void Sample()
        {
            nextStep += everySteps;
            if (count >= capacity)
            {
                full = true;
                return;
            }
            Vec3 p = sim.current;
            px[count] = (float)p.x;
            py[count] = (float)p.y;
            pz[count] = (float)p.z;
            speed[count] = (float)sim.mover.Speed;
            count++;
        }

        void MarkDown(Vec3 at)
        {
            down = true;
            downX = (float)at.x;
            downZ = (float)at.z;
            downStep = sim.run.steps;
        }

        void OnKill(KillEvent e)
        {
            if (!recording || e.owner != "player" || !sim.run.Live) return;
            kills++;
            if (killCount < killX.Length)
            {
                killX[killCount] = (float)e.position.x;
                killZ[killCount] = (float)e.position.z;
                killCount++;
            }
            string w = e.weapon;
            if (w == null) return;
            for (int i = 0; i < gunCount; i++)
            {
                if (gunIds[i] != w) continue;
                gunKills[i]++;
                return;
            }
            if (gunCount >= gunIds.Length) return;
            gunIds[gunCount] = w;
            gunKills[gunCount] = 1;
            gunCount++;
        }

        void OnRoundBegin(RoundEvent e)
        {
            if (!recording || roundCount >= roundNumber.Length) return;
            Vec3 p = sim.mover.position;
            roundNumber[roundCount] = e.number;
            roundStep[roundCount] = sim.run.steps;
            roundX[roundCount] = (float)p.x;
            roundZ[roundCount] = (float)p.z;
            roundCount++;
        }

        void OnDeath(DeathEvent e)
        {
            if (!recording || down || e.owner != "player" || !sim.run.Live) return;
            MarkDown(sim.mover.position);
        }

        void OnExfilStart(ExfilEvent e)
        {
            if (!recording || e.owner != "player") return;
            exfilSite = true;
            exfilX = (float)e.position.x;
            exfilZ = (float)e.position.z;
        }

        void OnExfilDone(ExfilEvent e)
        {
            if (recording && e.owner == "player") exfilWin = true;
        }
    }
}
