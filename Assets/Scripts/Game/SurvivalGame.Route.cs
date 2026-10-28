// NIGHT14 U3-27.10 / U3-27.11 (agent V2): THE ROUTE's and the GHOST's boot and frame (SurvivalGame.Night14.cs hooks
// BootRoute / RenderGhost).
// design.route on: one RouteTrack on the sim (it records after every step and resets at every run's start), read by
// RESULTS' map and the SHARE CARD (UiRoute.cs). design.route off: nothing is built (the old RESULTS), and no ghost.
// design.ghost on (with the route): the store (GhostStore: <persistentDataPath>/ghosts/<seed code>.bin, design.ghost.maxSeeds),
// the runner (GhostRunner) and the splits:
// - a run's end (RouteTrack.Ended: RESULTS, once) offers its route as the seed's best, unless the meta rule says no
//   (GhostStore.MayStore: a bot's run, LAB, a resumed run, automation outside GhostStore.FolderOverride; a read-only profile
//   (the smoke's) also needs the override);
// - a run's start (RunStarted, after the track's reset) loads the seed's best (none for a bot's run or the attract);
// - each frame the runner follows the best by the run's clock (run.steps, plus the frame's interpolation while it plays),
//   in PLAYING and PAUSED only (the player's death hides it at once: the race is over, WHAT GOT YOU replays without it);
// - each roundBegin with a best that began that round: a split (run steps against the best's, in s) for the HUD line
//   (UiGhost.cs), kept as numbers here (no string per round; GhostSplitText builds one for automation on demand).
// design.ghost off: none of it (the old HUD).
using System;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        RouteTrack routeTrack;
        GhostStore ghostStore;
        GhostRunner ghostRunner;
        GhostTrack ghostBest;

        /// <summary>THE ROUTE's track of the running (or last ended) run; null with design.route off.</summary>
        public RouteTrack RouteTrackView => routeTrack;

        /// <summary>The GHOST's store (null with design.ghost off); automation points GhostStore.FolderOverride elsewhere.</summary>
        public GhostStore GhostStoreView => ghostStore;

        /// <summary>The running run races a stored best (its seed's), and the runner is drawn this frame.</summary>
        public bool GhostLoaded => ghostBest != null;
        public bool GhostVisible => ghostRunner != null && ghostRunner.Visible;

        /// <summary>The last split: bumped per split shown, its round and seconds (negative: ahead of the best).</summary>
        public int GhostSplitVersion { get; private set; }
        public int GhostSplitRound { get; private set; }
        public double GhostSplitSeconds { get; private set; }

        /// <summary>The last split as the HUD shows it ("ROUND 5 · -12.4 s"), or null (none yet). Built on each read.</summary>
        public string GhostSplitText => GhostSplitVersion == 0 ? null : GhostSplitLine(GhostSplitRound, GhostSplitSeconds);

        /// <summary>The path of the last best stored this session (null: none).</summary>
        public string GhostStoredPath { get; private set; }

        public static string GhostSplitLine(int round, double seconds) =>
            "ROUND " + round.ToString(CultureInfo.InvariantCulture) + " · " + (seconds < 0 ? "-" : "+") + Math.Abs(seconds).ToString("0.0", CultureInfo.InvariantCulture) + " s";

        partial void BootRoute()
        {
            if (!cfg.Design("route.enabled")) return;
            routeTrack = new RouteTrack(sim); // subscribes itself: StepEnded (Record), RunStarted (report the last run, reset)
            if (!cfg.Design("ghost.enabled")) return;
            // NIGHT15 U3-28.5: best runs keyed by map too: the arena's ghosts stay where they were, another map's under maps/<id>.
            string ghostRoot = Path.GetDirectoryName(profilePath);
            if (MapId != RunSetup.DefaultMap) ghostRoot = Path.Combine(ghostRoot, UnityTuning.MapProfileFolder, MapId);
            ghostStore = new GhostStore(ghostRoot, cfg.Int("design.ghost.maxSeeds"));
            ghostRunner = new GhostRunner(this);
            routeTrack.Ended += GhostOnRunEnded;
            sim.RunStarted += GhostOnRunStarted; // after the track's own handler: the new run's best
            events.On<RoundEvent>("roundBegin", GhostOnRoundBegin);
            events.On<DeathEvent>("death", e =>
            {
                if (e.owner == "player") ghostRunner.Hide(); // the race is over (and WHAT GOT YOU's replay has no ghost)
            });
        }

        /// <summary>A run ended (RESULTS): its route becomes the seed's best if it beats the stored one and may be stored.</summary>
        void GhostOnRunEnded(RunStats stats)
        {
            if (profileReadOnly && string.IsNullOrEmpty(GhostStore.FolderOverride)) return; // automation writes nothing of the player's
            if (!GhostStore.MayStore(sim, routeTrack)) return;
            try
            {
                if (ghostStore.Offer(GhostTrack.From(routeTrack))) GhostStoredPath = ghostStore.PathFor(routeTrack.seed);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ghost] the best could not be stored: " + e.Message);
            }
        }

        /// <summary>A run started: race the seed's stored best (not a bot's run, not the attract).</summary>
        void GhostOnRunStarted()
        {
            ghostBest = null;
            if (sim.metaSilent || attractOn) return;
            GhostTrack best = ghostStore.Load(sim.runSeed);
            if (best == null || best.everySteps <= 0 || best.simHz != cfg.movement.loop.simHz) return; // another clock: no race
            ghostBest = best;
        }

        /// <summary>A round began: the split against the best's start of that round (numbers only; the HUD formats them).</summary>
        void GhostOnRoundBegin(RoundEvent e)
        {
            if (ghostBest == null || !run.Live) return;
            if (!ghostBest.Split(e.number, run.steps, out double seconds)) return;
            GhostSplitRound = e.number;
            GhostSplitSeconds = seconds;
            GhostSplitVersion++;
        }

        /// <summary>The props pass: a run that ended outside a step is reported; the runner follows the best by the run's clock.</summary>
        partial void RenderGhost(double frameDt, double alpha)
        {
            if (routeTrack == null) return;
            routeTrack.CheckEnded(); // R from DOWN / PAUSED's RESTART end a run outside a step (a reference compare)
            if (ghostRunner == null) return;
            Flow f = flow;
            bool show = ghostBest != null && !attractOn && run.Live && (f == Flow.Playing || f == Flow.Paused); // not in DOWN: the race is over
            // The drawn feet are between the last two steps (previous + (current - previous) x alpha): so is the ghost.
            double at = run.steps + (f == Flow.Playing && sim.Active ? alpha - 1 : 0);
            ghostRunner.Render(ghostBest, at, show);
        }
    }
}
