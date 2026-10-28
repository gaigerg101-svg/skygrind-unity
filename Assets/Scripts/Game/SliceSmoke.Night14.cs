// NIGHT14 smoke steps (after NIGHT13's): each item's proof in the built Player and the Editor smoke.
// 17. U3-27.2: a clean slate per run: on the live composition, after every run the smoke played before, two seed-1
//     fingerprint runs back to back print the same 12 hashes, and both equal the baseline (RETRY SAME SEED is the same
//     run in one session).
// 18. U3-27.8: a short run taped and replayed in this process (EQUAL); the tape's path logged for -skygrindReplay.
// 19. U3-27.12: the AUTOPILOT plays 150 s twice on fresh compositions in the Player: 2+ rounds, the same hashes.
// 20. U3-27.7: the attract 20 s behind TITLE, stopped as an input stops it; the next run equals the baseline.
// 21. U3-27.9 / .10 / .11: a husk's blow: WHAT GOT YOU, RESULTS with THE ROUTE, the ghost stored, the card's PNG;
//     RETRY SAME SEED shows the ghost and a split.
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class SliceSmoke
    {
        IEnumerator Night14(SurvivalGame g)
        {
            // 17. Two seed-1 runs on this composition (it has played the smoke's runs: its pools, ids and streams were used).
            List<string> first = g.RunFingerprint(SimFingerprint.Seed);
            yield return null;
            List<string> second = g.RunFingerprint(SimFingerprint.Seed);
            yield return null;
            int same = 0;
            for (int i = 0; i < first.Count && i < second.Count; i++)
                if (first[i] == second[i]) same++;
            int baseDiff = FingerprintBaseline.FirstDifference(first);
            Check(same == first.Count && first.Count == second.Count && baseDiff < 0,
                $"clean slate: seed 1 twice on the played composition: {same} of {first.Count} checkpoints equal, the first run {(baseDiff < 0 ? "EQUAL to the baseline" : $"differs from the baseline at checkpoint {baseDiff + 1}")}");
            yield return Tape(g);
        }

        /// <summary>
        /// 19. U3-27.8: a short run taped (automation's temporary folder), driven only through the forced inputs (the tape's
        /// rule: nothing else may change the run between steps), stopped with a last checkpoint, replayed in this process on a
        /// fresh composition; its path is logged ([smoke] tape path) for the separate -skygrindReplay check.
        /// </summary>
        IEnumerator Tape(SurvivalGame g)
        {
            if (g.TapeLive == null)
            {
                Check(false, "tape: design.tape off (no recorder)");
                yield break;
            }
            g.StartRun(737373);
            yield return null;
            float t0 = Time.realtimeSinceStartup;
            int f = 0;
            while (Time.realtimeSinceStartup - t0 < 4)
            {
                g.forcedMove = new InputFrame { moveX = (f / 60) % 2 == 0 ? 1 : -1, moveZ = 1, jump = new Button { pressed = f % 90 == 0, held = f % 90 < 5 } };
                g.forcedCombat = new CombatInput { fire = new Button { held = true, pressed = f % 20 == 0 }, weapon = -1 };
                f++;
                yield return null;
            }
            g.forcedMove = null;
            g.forcedCombat = null;
            bool recording = g.TapeLive.Recording;
            RunTape tape = recording ? g.TapeLive.Finish(TapeEnd.Stopped) : null;
            string path = TapeFiles.LastPath;
            float w0 = Time.realtimeSinceStartup;
            while ((path == null || !System.IO.File.Exists(path)) && Time.realtimeSinceStartup - w0 < 10)
            {
                yield return null;
                path = TapeFiles.LastPath;
            }
            yield return new WaitForSecondsRealtime(0.5f); // the worker's write finishes
            string line = path != null && System.IO.File.Exists(path) ? TapeReplay.ReplayFile(g.cfg, g.map, path) : "no file";
            Debug.Log("[smoke] tape path " + path);
            Check(recording && line.StartsWith("EQUAL"), $"tape: {(tape != null ? tape.steps : 0)} steps taped ({(path != null && System.IO.File.Exists(path) ? new System.IO.FileInfo(path).Length : 0)} B), replayed in this process: {line}");
            g.ToTitle();
            yield return null;
            yield return Autopilot(g);
            yield return Attract(g);
            yield return RouteGhost(g);
        }

        /// <summary>
        /// 20. U3-27.12 / U3-27.5: the AUTOPILOT (BotBrain, normal, real HP) plays seed 4242 for 150 s of sim time on a fresh
        /// headless composition, twice, in this Player (Mono): at least 2 rounds, and the same hash every 600 steps both times.
        /// </summary>
        IEnumerator Autopilot(SurvivalGame g)
        {
            var a = new List<string>();
            var b = new List<string>();
            float t0 = Time.realtimeSinceStartup;
            Farm.Record ra = Farm.RunOne(g.cfg, g.map, 4242, "normal", 150, a);
            yield return null;
            Farm.Record rb = Farm.RunOne(g.cfg, g.map, 4242, "normal", 150, b);
            int same = 0;
            for (int i = 0; i < a.Count && i < b.Count; i++)
                if (a[i] == b[i]) same++;
            Check(ra.brain == "botBrain" && ra.round >= 2 && a.Count > 0 && same == a.Count && a.Count == b.Count && ra.round == rb.round,
                $"autopilot: seed 4242 normal ({ra.brain}) twice for 150 s: round {ra.round} / {rb.round}, {ra.kills} kills, cause {ra.cause}; {same} of {a.Count} hashes equal ({Time.realtimeSinceStartup - t0:0.0} s); last {(a.Count > 0 ? a[a.Count - 1] : "-")}");
            yield return null;
        }

        /// <summary>
        /// 21. U3-27.7: the attract plays behind TITLE for 20 s (the AUTOPILOT, metaSilent, god mode), stops as an input
        /// stops it, and the next run starts clean: a seed-1 fingerprint run equals the baseline.
        /// </summary>
        IEnumerator Attract(SurvivalGame g)
        {
            g.ToTitle();
            yield return null;
            System.Func<string, string> assistBefore = g.sim.assistModeFor;
            bool silentBefore = g.sim.metaSilent, noLabBefore = g.sim.noLab;
            bool started = g.AttractStart();
            float t0 = Time.realtimeSinceStartup;
            bool heldTitle = true, silent = true;
            int steps0 = g.simStep;
            while (started && Time.realtimeSinceStartup - t0 < 20)
            {
                heldTitle &= g.UiFlow == Flow.Title && g.attractOn;
                silent &= g.sim.metaSilent;
                yield return null;
            }
            int played = g.simStep - steps0;
            int kills = g.run.stats.kills;
            g.AttractStop();
            yield return null;
            yield return null;
            bool released = !g.attractOn && g.sim.metaSilent == silentBefore && g.sim.noLab == noLabBefore && g.sim.input == g.playerInput && g.flow == Flow.Title && ReferenceEquals(g.sim.assistModeFor, assistBefore);
            List<string> lines = g.RunFingerprint(SimFingerprint.Seed);
            int diff = FingerprintBaseline.FirstDifference(lines);
            Check(started && heldTitle && silent && played > 0 && released && diff < 0,
                $"attract: started {started}, 20 s behind TITLE {heldTitle} ({played} steps, {kills} kills, metaSilent {silent}); stopped: released {released}; the next run {(diff < 0 ? "EQUAL to the baseline (12 of 12)" : $"differs at checkpoint {diff + 1}")}");
            yield return null;
        }

        /// <summary>
        /// 22. U3-27.9 / U3-27.10 / U3-27.11: a run (seed 515151) to round 1's fight, a husk's killing blow: WHAT GOT YOU plays
        /// (the killer named), then RESULTS with THE ROUTE drawn and the best stored as the seed's ghost (temporary folders);
        /// SAVE CARD writes its PNG (path logged); RETRY SAME SEED shows the ghost, and round 1's start a split.
        /// </summary>
        IEnumerator RouteGhost(SurvivalGame g)
        {
            string tmp = System.IO.Path.Combine(Application.temporaryCachePath, "smoke-n14");
            GhostStore.FolderOverride = System.IO.Path.Combine(tmp, "ghosts");
            RouteCard.FolderOverride = System.IO.Path.Combine(tmp, "cards");
            try
            {
                if (System.IO.Directory.Exists(GhostStore.FolderOverride)) System.IO.Directory.Delete(GhostStore.FolderOverride, true);
            }
            catch (System.Exception)
            {
                // a leftover folder only holds an older best
            }
            g.StartRun(515151);
            float t0 = Time.realtimeSinceStartup;
            Husk husk = null;
            while (Time.realtimeSinceStartup - t0 < 30)
            {
                if (g.run.round >= 1 && g.run.state == SurvivalRun.State.Fight)
                {
                    foreach (Enemy e in g.enemies.all)
                        if (e.alive && e.kind == "husk") { husk = (Husk)e; break; }
                    if (husk != null) break;
                }
                g.health.hp = g.health.max; // alive until the blow (the tape isn't this step's subject)
                yield return null;
            }
            if (husk == null)
            {
                Check(false, "route / ghost: no husk alive in round 1 within 30 s");
                g.ToTitle();
                yield break;
            }
            g.health.Damage(g.health.max * 2, "husk", husk.position);
            yield return null;
            bool recap = g.RecapPlaying;
            string killer = g.RecapKillerName;
            t0 = Time.realtimeSinceStartup;
            while (g.flow != Flow.Results && Time.realtimeSinceStartup - t0 < 20) yield return null;
            yield return null;
            yield return null;
            bool results = g.flow == Flow.Results;
            bool drawn = g.UiView != null && g.UiView.RouteDrawn;
            int points = g.UiView != null ? g.UiView.RouteDrawnPoints : 0;
            string stored = g.GhostStoredPath;
            g.UiView?.RouteSaveCardNow();
            t0 = Time.realtimeSinceStartup;
            while ((RouteCard.LastPath == null || !System.IO.File.Exists(RouteCard.LastPath)) && Time.realtimeSinceStartup - t0 < 10) yield return null;
            string card = RouteCard.LastPath;
            bool cardOk = card != null && System.IO.File.Exists(card);
            Debug.Log("[smoke] card path " + card + (RouteCard.LastError != null ? " (error: " + RouteCard.LastError + ")" : ""));
            Check(recap && killer != null && results && drawn && stored != null && cardOk,
                $"WHAT GOT YOU + THE ROUTE: recap played {recap} (killer {killer ?? "-"}), RESULTS {results}, the route drawn {drawn} ({points} points), the ghost stored {stored != null}, the card {(cardOk ? new System.IO.FileInfo(card).Length + " B" : "not written: " + RouteCard.LastError)}");
            g.RetrySameSeed();
            yield return null;
            yield return null;
            bool loaded = g.GhostLoaded;
            bool visible = false;
            t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 20 && !(g.GhostSplitVersion >= 1 && g.UiView != null && g.UiView.GhostSplitVisible))
            {
                visible |= g.GhostVisible;
                g.health.hp = g.health.max;
                yield return null;
            }
            visible |= g.GhostVisible;
            Check(loaded && visible && g.GhostSplitVersion >= 1 && g.UiView.GhostSplitVisible,
                $"ghost: a second run on seed 515151: loaded {loaded}, the runner shown {visible}, a split at round {g.GhostSplitRound}: '{g.UiView?.GhostSplitShownText ?? "-"}'");
            g.ToTitle();
            GhostStore.FolderOverride = null;
            RouteCard.FolderOverride = null;
            yield return null;
        }
    }
}
