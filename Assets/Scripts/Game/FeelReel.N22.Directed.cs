// NIGHT22 leftover 1 (builder M): the reel scenario `directed`. UNCOMPILED and UNRUN when written. NEEDS THE LEAD: add "directed" to
// FeelReel.PhaseF, the declaration `static partial void N22DirectedBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);`
// and `case "directed": N22DirectedBody(g, dir, res, offs, pre, ref body); break;` in N22Scenario (FeelReel.N22.cs). Until then this file is inert (a
// partial method with no declaration would not compile: it is wrapped in #if NIGHT22_DIRECTED_REEL, which the lead removes with that change).
// Contract line: when the hunt is announced / the beacon is lit, the player sees a directed cue (a step list / marker naming where to go) and the
// cue reads as DIRECTED, never red for "out of range"; with the row off the game is bit-identical (the cue is render side: Ui.Directed22*).
//   beacon  Fresh run, the beacon forced on, skipped to its break: the cue reads step 1 with the beacon's distance and side; the player placed beside it:
//           step 2 (HOLD E); BUY held until it is lit: step 3
//   hunt    a fresh run skipped to a swarm round's break (design.adds.hunt must be on): the cue reads THE HUNT, step 1, with no distance
// Checks: the cue was built (Ui.Directed22Built), its kind/step per stage, and its colour is not red-orange (r > 0.7 and g < 0.45 fails).
// Every result line starts with FeelReel.Preconditions(g, pos) from SafeSpot(g).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N22DirectedBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = DirectedScenario(g, dir, res, offs, pre);
        }

        static bool Directed22Red(Color c) => c.r > 0.7f && c.g < 0.45f;

        static IEnumerator DirectedScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            if (!g.sim.n22.directed)
            {
                res[0] = pc + " | RED: design.n22.directed is off";
                yield break;
            }
            var fails = new List<string>();
            if (!Ui.Directed22Built) fails.Add("the directed cue was not built (Ui.BuildN22Hud does not call BuildDirected22Cue)");

            // --- beacon -----------------------------------------------------------------------------------------------------------------
            Beacon20Run b = g.sim.Beacon20Ensure();
            b.Force = true;
            offs.Add(() => b.Force = false);
            Fresh(g);
            Beacon20.P p = Beacon20.P.From(g.cfg);
            DirectorMath.D dm = DirectorMath.From(g.cfg);
            int r = p.from;
            while (!Beacon20.Stands(g.sim.runSeed, r, p, dm) && r < 60) r++;
            g.run.DebugSkipTo(r);
            g.run.phaseSteps = UnityTuning.ReelHoldBreakSteps;
            yield return Frames(20);
            Start(g, "directed", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            yield return Frames(Ms(900));
            Directed22Kind k0 = Ui.Directed22KindNow;
            int st0 = Ui.Directed22StepNow;
            string t0 = Ui.Directed22TextNow;
            bool red0 = Directed22Red(Ui.Directed22ColorNow);
            if (!b.Standing) fails.Add("no beacon stood in the break");
            if (k0 != Directed22Kind.Beacon || st0 != 0) fails.Add($"the cue at the stand read {k0} step {st0 + 1}, not the beacon's step 1");
            if (red0) fails.Add("the cue is red");

            FeelProbe.Event("directedNear");
            g.Respawn(new Vec3(b.Spot.x + 1.0, b.Spot.y, b.Spot.z), 0);
            Look(g, new Vec3(b.Spot.x, g.Eye.y, b.Spot.z));
            yield return Frames(Ms(700));
            int st1 = Ui.Directed22StepNow;
            if (st1 != 1) fails.Add($"beside the beacon the cue read step {st1 + 1}, not 2");

            FeelProbe.Event("directedLit");
            int guard = 0;
            while (b.Standing && guard++ < 600)
            {
                God(g);
                g.run.phaseSteps = UnityTuning.ReelHoldBreakSteps;
                Look(g, new Vec3(b.Spot.x, g.Eye.y, b.Spot.z));
                g.forcedCombat = new CombatInput { weapon = -1, interact = new Skygrind.Core.Button { held = true } };
                yield return null;
            }
            g.forcedCombat = IdleCombat;
            yield return Frames(Ms(700));
            int st2 = Ui.Directed22StepNow;
            string t2 = Ui.Directed22TextNow;
            if (b.Standing || st2 != 2) fails.Add($"after the buy the cue read step {st2 + 1}, not 3 (standing {b.Standing})");
            yield return Stop();

            // --- hunt -------------------------------------------------------------------------------------------------------------------
            b.Force = false;
            int hunts = 0;
            offs.Add(g.events.On<SpecialRoundEvent>(N20Events.SpecialRound, e => { if (e.name == "hunt" && e.phase == "announce") hunts++; }));
            Fresh(g);
            int hr = 3;
            while (!SpecialRoundMath.IsHunt(hr, DirectorMath.From(g.cfg)) && hr < 60) hr++;
            g.run.DebugSkipTo(hr);
            g.run.phaseSteps = UnityTuning.ReelHoldBreakSteps;
            yield return Frames(Ms(900));
            string hunt;
            if (hunts == 0) hunt = "hunt not exercised (no announce: design.adds.hunt is off)";
            else
            {
                Directed22Kind hk = Ui.Directed22KindNow;
                int hs = Ui.Directed22StepNow;
                bool hred = Directed22Red(Ui.Directed22ColorNow);
                hunt = $"hunt round {hr}: cue {hk} step {hs + 1}, \"{Ui.Directed22TextNow}\"";
                if (hk != Directed22Kind.Hunt || hs != 0) fails.Add($"the hunt's cue read {hk} step {hs + 1}");
                if (hred) fails.Add("the hunt's cue is red");
            }
            g.run.phaseSteps = 1;
            yield return Frames(Ms(300));

            res[0] = pc + $" | {FeelProbe.FrameCount} frames; beacon round {r}: \"{t0}\" -> step {st1 + 1} beside it -> \"{t2}\"; {hunt} | "
                + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
