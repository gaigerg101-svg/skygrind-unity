// NIGHT22 U3-36.8 CHALLENGE CODE (builder W2): the reel scenario `code` (contract `challenge`), written FIRST. UNCOMPILED when written
// (the builder has no Editor). The scenario, on the Player:
//   1. SafeSpot's line, then TITLE: the CODE field must exist (Ui.Code22Built); a code with GLASS CANNON + SPEED DEMON is driven in
//      the way a pad does (the sheet opens, the characters arrive, A reads it: Ui.Code22Drive) and must reach the run setup;
//   2. PLAY: the run's state is read, not its tape: RunStats.mutators holds both, the Loadout's damage dealt, the Health's damage
//      taken and the enemy pace carry the config's multipliers (mutators.kinds.*);
//   3. the player is downed and RESULTS opens: the CODE line must read the same code that was typed.
// The result starts with the preconditions, then the numbers, then PASS or RED with every failed part named. With design.n21.challenge
// off (-skygrindOff21) there is no CODE field, no reader and no RESULTS line: that is the RED the lead records.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N22CodeBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = N22CodeRun(g, dir, res, offs, pre);
        }

        static IEnumerator N22CodeRun(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (UNDER 12 m)");
            res[0] = pc + "; RED: the scenario did not finish";
            Ui ui = g.UiView;
            if (ui == null || !ui.Code22Built)
            {
                res[0] = pc + "; RED: no CODE field on TITLE (design.n21.challenge off or the field not built)";
                yield break;
            }
            GameConfig cfg = g.sim.cfg;
            var spec = new Challenge21.Spec
            {
                seed = 1234567u, heat = 0, mode = RunSetup.Survival, target = 0,
                mutators = new List<string> { "glassCannon", "speedDemon" },
            };
            string typed = Challenge21.Encode(spec, cfg);

            g.ToTitle();
            yield return Frames(10);
            bool applied = ui.Code22Drive(typed);
            yield return Frames(5);
            bool onSetup = g.setup.mutators.Contains("glassCannon") && g.setup.mutators.Contains("speedDemon") && g.setup.seed == spec.seed;

            g.StartRun();
            yield return Frames(30);
            var list = g.run.stats.mutators;
            bool listed = list.Contains("glassCannon") && list.Contains("speedDemon");
            double dealt = g.sim.loadout.damageMult, taken = g.sim.health.takenMult, pace = g.sim.brain.speedMult;
            double wd = cfg.Num("mutators.kinds.glassCannon.dealt"), wt = cfg.Num("mutators.kinds.glassCannon.taken"), wp = cfg.Num("mutators.kinds.speedDemon.enemySpeed");
            bool numbers = dealt == wd && taken == wt && pace == wp;
            bool seedOk = g.runSeed == spec.seed;

            Vec3 f0 = g.Feet;
            g.health.max = g.health.hp = 100;
            g.health.Damage(g.health.max * 2, "husk", new Vec3(f0.x, f0.y, f0.z - 3));
            int n = 0;
            while (n++ < 1800 && g.flow != Flow.Results) yield return null;
            yield return Frames(30);
            string shown = ui.Code22ResultsShown;
            bool resultsOk = g.flow == Flow.Results && shown == "CODE " + typed;

            var why = new StringBuilder();
            if (!applied) why.Append(" the sheet did not apply the typed code;");
            if (!onSetup) why.Append(" the run setup does not hold the code's mutators and seed;");
            if (!listed) why.Append(" RunStats.mutators is [").Append(string.Join(",", list)).Append("] (want both);");
            if (!numbers) why.Append(" dealt/taken/pace x").Append(dealt).Append("/x").Append(taken).Append("/x").Append(pace).Append(" (want x").Append(wd).Append("/x").Append(wt).Append("/x").Append(wp).Append(");");
            if (!seedOk) why.Append(" run seed ").Append(g.runSeed).Append(" (want ").Append(spec.seed).Append(");");
            if (!resultsOk) why.Append(" RESULTS line \"").Append(shown).Append("\" (want \"CODE ").Append(typed).Append("\");");
            res[0] = pc + $"; typed {typed}: stats.mutators [{string.Join(",", list)}], dealt x{dealt} taken x{taken} pace x{pace}, seed {g.runSeed}, RESULTS \"{shown}\": " + (why.Length == 0 ? "PASS" : "RED:" + why);
        }
    }
}
