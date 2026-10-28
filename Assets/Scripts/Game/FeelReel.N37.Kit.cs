// NIGHT37 builder K: the Player scenario "verbs37" (FeelReel.N27.cs PhaseN37K; the body is the partial N37KitBody). NIGHT37.md method 4: nothing
// lost on every level whose boss changed. verbs37 = verbs36's scenario (docs/VERBS.md rows 1-43, FeelReel.N35.Verbs.cs + FeelReel.N36.Move.cs)
// plus rows 44-47, the boss verbs (the climb, the slipstream, the thread tear, the ring pass), one line each:
//   `[reel] verbs37: <#> <verb> PASS|RED|SKIP <measured>`, then `[reel] verbs37: total <pass>/<performed>`.
// Boot one Player per map, as verbs36 (L-046): -skygrindMap <the level's id> -skygrindReelOnly verbs37, for levels 1, 2, 4, 5 with their n37 levels.
// Not run by its author (no Player in the worktree).
//
// Rows 44-47 here: each verb belongs to one level and one boss row (VERBS.md). On another level's map: SKIP (not this level). On its level with the
// row off (the old level loads): SKIP (the row off: NIGHT36's boss). On its level with the row on: the walk-in is armed (the boss builder's StartRun
// set boss37WalkInHere on this run) and the level is the row's levelTo: PASS, else RED. The verb itself (the climb reaching the cap, the
// slipstream's gain, a torn knee, a ring pass) is measured by the boss's own reel (wick37 / stag37 / loom37 / crown37) and named in the line.
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N37KitBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "verbs37") body = Verbs37Scenario(g, dir, res, offs, pre);
        }

        /// <summary>docs/VERBS.md's NIGHT37 rows: #, verb, the boss row, the boss's reel.</summary>
        static readonly (int n, string verb, string row, string reel)[] Verbs37Table =
        {
            (44, "the climb", "wick37", "wick37"), (45, "the slipstream", "stag37", "stag37"), (46, "the thread tear", "loom37", "loom37"),
            (47, "the ring pass", "crown37", "crown37"),
        };

        /// <summary>verbs37: verbs36's scenario with verbsTag "verbs37" (rows 40-43 run for it too) and rows 44-47 before the close.</summary>
        static IEnumerator Verbs37Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            verbsTag = "verbs37";
            offs.Add(() => verbsTag = "verbs35");
            yield return Verbs35Scenario(g, dir, res, offs, pre);
            verbsTag = "verbs35";
            res[0] = "verbs37: " + res[0];
        }

        /// <summary>Rows 44-47 inside the verbs scenario (after rows 40-43, before its close). No input: these read the run's state.</summary>
        static IEnumerator Verbs37Rows(SurvivalGame g, Verbs35State v, Vec3 open, double openYaw, double wallEnd)
        {
            SurvivalSim sim = g.sim;
            GameConfig cfg = g.cfg;
            string level = sim.campaignLevel?.id ?? "(no campaign level)";
            foreach (var r in Verbs37Table)
            {
                string p = "design.n37." + r.row;
                string from = cfg.Has(p + ".levelFrom") ? cfg.Str(p + ".levelFrom") : "", to = cfg.Has(p + ".levelTo") ? cfg.Str(p + ".levelTo") : "";
                bool rowOn = RowOn37(sim, r.row);
                if (level != from && level != to) { Verb35(v, r.n, "SKIP", $"not this map's level ({level}; the verb is {to}'s)"); continue; }
                if (!rowOn) { Verb35(v, r.n, "SKIP", $"design.n37.{r.row} off on {level}: NIGHT36's boss (the verb comes with the row)"); continue; }
                bool armed = sim.boss37WalkInHere, onTo = level == to;
                Verb35(v, r.n, armed && onTo ? "PASS" : "RED",
                    $"{level} (want {to}), the walk-in armed {armed} (boss37WalkInHere: the {r.row} StartRun), n37 {sim.n37}; the verb itself: the {r.reel} reel's line");
            }
            yield break;
        }

        static bool RowOn37(SurvivalSim sim, string row) =>
            row == "wick37" ? sim.n37.wick37 : row == "stag37" ? sim.n37.stag37 : row == "loom37" ? sim.n37.loom37 : row == "crown37" ? sim.n37.crown37 : false;
    }
}
