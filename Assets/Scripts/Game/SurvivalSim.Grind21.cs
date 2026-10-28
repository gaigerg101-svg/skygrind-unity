// NIGHT21 U3-35.7 THE GRIND (builder W): the sim's side of Core/Endless21.cs. The hooks N21GrindStartRun / N21GrindStep (called by
// the lead's SurvivalSim.N21.cs / SurvivalSim.Run.cs, in simulate() order after the enemies' step) run only with
// design.n21.grind.enabled (this run's n21.grind) AND the run mode TITLE's MODE row picked (RunSetup.Grind). Otherwise nothing
// here does anything: SURVIVAL keeps its flat core and its own rounds.
//
// How it rides on SURVIVAL's engine: SurvivalRun still spawns each wave (the Director), counts the stats and pays the clear bonus;
// Endless21 owns the phases (a 4 s break with a full heal + stamina, the core's reshape for 1.5 s, the fight) and the score. The
// round clock is held in its break while Endless21 breaks / reshapes (its phaseSteps mirrors Endless21's, plus one) and let go
// the step Endless21 starts the fight, so SurvivalRun.Begin starts the wave `run.round` == Endless21's wave. READY does nothing
// in the GRIND (the break is the web's fixed 4 s). The records of SURVIVAL are never touched by a GRIND run (noRecords is held
// for the run); the GRIND's own best per HEAT is Records21 (grindRecords).
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>This run is a GRIND (set at the run's start; the Endless21 is built on its first step, once the seed is known).</summary>
        public bool grindOn;

        public Endless21 grind;
        public readonly Records21 grindRecords = new Records21();
        /// <summary>The last GRIND run's results line (records.js judge) and stats; null before one ends.</summary>
        public Records21.Judgement? lastGrindJudgement;
        public EndlessStats lastGrindStats;
        /// <summary>The preset the core wears now (the reshape's last pick; "flat" at the run's start).</summary>
        public string grindPreset = "flat";

        // NIGHT22 U3-36.6: the telegraph. The next shape is picked when the break begins (the same draws from the same stream, only
        // earlier than the reshape used to draw them), so the last design.n21.grind.telegraphTime s of the break can mark the pillars
        // it will lift. The reshape then wears exactly what was marked. A view reads GrindTelegraphing / GrindRising / GrindNextHeights.
        string grindNextPreset;
        double[] grindNextHeights;
        bool[] grindRisingMarks;
        int grindPickedWave;
        bool grindTelegraphing;

        /// <summary>The rising-edge marks are on now (the end of the break and the reshape; false outside the GRIND).</summary>
        public bool GrindTelegraphing => grindOn && grindTelegraphing && grindRisingMarks != null;
        /// <summary>Per core pillar: the next shape lifts it by at least one height step. Null: none picked.</summary>
        public bool[] GrindRising => grindRisingMarks;
        /// <summary>The heights the next reshape goes to (the marks' top edge). Null: none picked.</summary>
        public double[] GrindNextHeights => grindNextHeights;
        /// <summary>The preset the next reshape wears (null: none picked).</summary>
        public string GrindNextPreset => grindNextPreset;

        // NIGHT22 U3-36.6: THE GRIND's bests survive a restart. The Game gives the sim a file path (persistent data path); the sim
        // reads it at the first GRIND run and writes it when a run ends with a new best. Null path: memory only (the harness, the smoke).
        string grindRecordsPath;

        /// <summary>The file the GRIND's records are kept in (null: not kept). Loading happens now.</summary>
        public void GrindRecordsFile(string path)
        {
            grindRecordsPath = path;
            if (path == null) return;
            grindRecords.CopyFrom(Records21.Load(path));
        }

        /// <summary>The file the GRIND's records are kept in (null: memory only).</summary>
        public string GrindRecordsPath => grindRecordsPath;

        /// <summary>A scenario that ran a GRIND with its own records flag gives the hold back (the GRIND holds SURVIVAL's records off for its run and restores the flag at the next non-GRIND run's start).</summary>
        public void GrindReleaseHold()
        {
            if (!grindHeldNoRecords) return;
            noRecords = grindPrevNoRecords;
            grindHeldNoRecords = false;
        }

        /// <summary>
        /// The personal-best line RESULTS reads (SurvivalGame.lastPb): in a GRIND it is the GRIND's own judgement (wave and score,
        /// records.js endless), not SURVIVAL's round-based one; otherwise the sim's lastPb as it was.
        /// </summary>
        public Records.Judgement PbShown
        {
            get
            {
                if (grindOn && lastGrindJudgement.HasValue)
                    return new Records.Judgement { newBest = lastGrindJudgement.Value.newBest, first = lastGrindJudgement.Value.first, text = lastGrindJudgement.Value.text };
                return lastPb;
            }
        }

        Mulberry32 grindRng;
        List<string> grindPresets;
        readonly HashSet<string> grindUsed = new HashSet<string>();
        bool grindHeldNoRecords, grindPrevNoRecords, grindEnded;

        sealed class GrindSpawner : IRoundSpawner
        {
            readonly SurvivalSim sim;
            public GrindSpawner(SurvivalSim sim) => this.sim = sim;

            // The wave itself is spawned and stepped by SurvivalRun (the Director) in this same step: letting the round clock go starts it.
            public void StartWave(int round, uint seed) => sim.run.phaseSteps = 1;
            public void Step(double dt) { }
            public bool Done => sim.run.state == SurvivalRun.State.Break; // SurvivalRun cleared the wave (roundClear) and began the next break
            public void Clear() { }
        }

        partial void N21GrindStartRun()
        {
            grind = null;
            grindEnded = false;
            lastGrindJudgement = null;
            grindPreset = "flat";
            grindUsed.Clear();
            grindNextPreset = null;
            grindNextHeights = null;
            grindRisingMarks = null;
            grindPickedWave = 0;
            grindTelegraphing = false;
            grindOn = n21.grind && setup.mode == RunSetup.Grind;
            if (grindOn)
            {
                if (!grindHeldNoRecords)
                {
                    grindPrevNoRecords = noRecords;
                    grindHeldNoRecords = true;
                }
                noRecords = true; // SURVIVAL's records and profile are not a GRIND run's
                grindPresets = cfg.Strs("design.n21.grind.presets");
            }
            else if (grindHeldNoRecords)
            {
                noRecords = grindPrevNoRecords;
                grindHeldNoRecords = false;
            }
        }

        partial void N21GrindStep(double dt)
        {
            if (!grindOn) return;
            if (grind == null)
            {
                if (!run.Live) return;
                grindRng = new Mulberry32(runSeed ^ 0x6e21u); // rule 6: the reshape's picks, the run's own stream
                grind = new Endless21(Endless21.Params.From(cfg), cfg.movement.loop.simHz, events, new EndlessDeps
                {
                    spawner = new GrindSpawner(this),
                    healthReset = () => health.Reset(),
                    restoreStamina = () => mover.RestoreStamina(),
                    styleTotal = () => style.total,
                    styleBestRank = () => style.Params.ranks[style.bestRank],
                    reshape = GrindReshape,
                }, GrindEmit);
                grind.Start(runSeed); // style is already running (StartRun); the first break is the web's 4 s
            }
            else if (!grindEnded)
            {
                if (flow == Flow.Down || flow == Flow.Results || !run.Live) GrindEnd(); // death ends the run
                else grind.Step(dt);
            }
            if (!grindEnded && run.state == SurvivalRun.State.Break && (grind.Phase == Endless21.State.Break || grind.Phase == Endless21.State.Reshape))
                run.phaseSteps = grind.PhaseSteps + 1; // hold the round clock; the countdown reads Endless21's
            if (grind != null && !grindEnded)
            {
                if (grind.Phase == Endless21.State.Break && grindPickedWave != grind.CurrentWave) GrindPickNext(grind.CurrentWave); // a break just began
                grindTelegraphing = grind.Telegraphing(cfg.Num("design.n21.grind.telegraphTime"), step);
            }
            else grindTelegraphing = false;
        }

        /// <summary>The break's start: draw the next shape (the reshape's own two draws, just earlier) and mark what it lifts.</summary>
        void GrindPickNext(int wave)
        {
            grindPickedWave = wave;
            grindNextPreset = null;
            grindNextHeights = null;
            grindRisingMarks = null;
            if (core == null || grindPresets == null) return;
            string pick = Endless21.PickPreset(grindPresets, grindPreset, grindRng, grindUsed);
            if (pick == null) return;
            grindNextPreset = pick;
            grindNextHeights = CoreGrid.PresetHeights(pick, core.spec.cells, cfg.movement.core.maxHeight, cfg.movement.core.heightStep, grindRng);
            var now = new double[core.Count];
            for (int i = 0; i < now.Length; i++) now[i] = core.Height(i);
            grindRisingMarks = Endless21.Rising(now, grindNextHeights, cfg.movement.core.heightStep);
        }

        void GrindEnd()
        {
            grindEnded = true;
            lastGrindStats = grind.End();
            if (lastGrindStats != null && !metaSilent && !lab.Any && !grindPrevNoRecords)
            {
                lastGrindJudgement = grindRecords.Submit(setup.heat, lastGrindStats.lastWave, lastGrindStats.score);
                if (grindRecordsPath != null && lastGrindJudgement.Value.newBest) grindRecords.Save(grindRecordsPath); // NIGHT22: kept across a restart
            }
            else if (lastGrindStats != null)
                lastGrindJudgement = Records21.Judge(new Records21.Entry { wave = lastGrindStats.lastWave, score = lastGrindStats.score }, grindRecords.Best(setup.heat, out var b) ? b : (Records21.Entry?)null);
        }

        /// <summary>The web's events (waveBreak, waveStart, waveClear); runEnd is SurvivalRun's own emit with the same name in the port, so Endless21's is not repeated.</summary>
        void GrindEmit(string type, object payload)
        {
            if (type == "runEnd") return;
            events.Emit(type, payload);
            // The WAVE banner (endless.js: "a 4 s break (full heal + stamina, then the WAVE N banner)"); the Ui raises it for a "banner" event.
            if (type == "waveBreak") events.Emit("banner", new BannerEvent { text = string.Format(UnityTuning.GrindBannerFormat, ((WaveEvent)payload).number) });
        }

        /// <summary>endless.js reshape(wave): picks the core's next heights and starts the tween; returns its length in s (0: no core).</summary>
        double GrindReshape(int wave)
        {
            if (core == null || grindPresets == null) return 0;
            if (grindNextHeights == null || grindPickedWave != wave) GrindPickNext(wave); // a reshape with no break before it (a debug skip)
            if (grindNextHeights == null) return 0;
            double seconds = cfg.Num("endless.reshapeTime");
            core.Reshape(grindNextHeights, seconds, step);
            grindPreset = grindNextPreset;
            return seconds;
        }

        /// <summary>The GRIND's score now (style + pointsPerWave x waves cleared; the last run's after it ends); 0 outside a GRIND.</summary>
        public double GrindScore => grind != null ? grind.Score : 0;
    }
}
