// NIGHT24 shotgun-rate (builder R): the reel scenario `shotgun`, the contract's Player half. Written FIRST, UNCOMPILED and UNRUN (no Editor in the
// builder's worktree). It measures the THUNDERCLAP's shot-to-shot time on the Player, in run steps (120 Hz), with the trigger held for 6 s per
// phase at SafeSpot(g) and the shotgun in hand:
//   BEFORE  the loadout's NIGHT24 multipliers set to 1 (the game NIGHT22 shipped)
//   AFTER   the multipliers as the row sets them (SurvivalSim.Shotgun24.cs): design.n24.shotgunTune.intervalMult and damageMult
// The result line prints both gaps (steps and seconds), their ratio (want 1.30 within a step), the pump delay (the viewmodel's, 0.45 s: it
// overlaps the shot in BEFORE, and not in AFTER) and, when BEFORE is not the web's 0.9 s, what shortened it (the NIGHT20 retune, Loadout.TuneInterval).
// RED: the row is off (the line says so and prints the equal gaps). The loadout's multipliers are put back after the scenario.
// Every result line starts with FeelReel.Preconditions(g, pos) from SafeSpot(g).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N24ShotgunBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = ShotgunScenario(g, dir, res, offs, pre);
        }

        static IEnumerator ShotgunPhase(SurvivalGame g, double yaw, List<int> gaps, int fireSteps)
        {
            var at = new List<int>();
            Action<object> onFire = o => { if (o as string == "shotgun") at.Add(g.run.steps); };
            g.events.On("fire", onFire);
            int slot = g.loadout.owned.IndexOf("shotgun");
            for (int f = 0; f < fireSteps; f++)
            {
                God(g);
                g.input.SetLook(yaw, 0);
                g.forcedCombat = new CombatInput { weapon = f == 0 ? slot : -1, fire = new Skygrind.Core.Button { held = true, pressed = true } };
                yield return null;
            }
            g.forcedCombat = IdleCombat;
            g.events.Off("fire", onFire);
            for (int i = 1; i < at.Count; i++) gaps.Add(at[i] - at[i - 1]);
        }

        static IEnumerator ShotgunScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            double hz = g.cfg.movement.loop.simHz;
            Loadout lo = g.loadout;
            double iMult = lo.sgIntervalMult24, dMult = lo.sgDamageMult24;
            offs.Add(() => { lo.sgIntervalMult24 = iMult; lo.sgDamageMult24 = dMult; g.forcedCombat = IdleCombat; });
            if (lo.owned.IndexOf("shotgun") < 0) lo.AddWeapon("shotgun");
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            Start(g, "shotgun", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            int phaseFrames = Ms(6000);

            lo.sgIntervalMult24 = lo.sgDamageMult24 = 1;
            var before = new List<int>();
            FeelProbe.Event("before");
            yield return ShotgunPhase(g, spotYaw, before, phaseFrames);
            lo.sgIntervalMult24 = g.sim.n24.shotgunTune ? g.cfg.Num("design.n24.shotgunTune.intervalMult") : 1;
            lo.sgDamageMult24 = g.sim.n24.shotgunTune ? g.cfg.Num("design.n24.shotgunTune.damageMult") : 1;
            var after = new List<int>();
            FeelProbe.Event("after");
            yield return ShotgunPhase(g, spotYaw, after, phaseFrames);
            yield return Stop();

            before.Sort();
            after.Sort();
            int b = before.Count == 0 ? 0 : before[before.Count / 2], a = after.Count == 0 ? 0 : after[after.Count / 2];
            double web = g.cfg.Num("weapons.shotgun.fireInterval"), pump = g.cfg.Num("weapons.shotgun.pumpDelay");
            double bs = b / hz, asec = a / hz, ratio = b > 0 ? a / (double)b : 0;
            string why = Math.Abs(bs - web) < 1.5 / hz ? "the web's interval" : $"not the web's {web.ToString("0.###", ci)} s: shortened by design.arsenal.n20.tune.shotgun (interval x{lo.TuneInterval("shotgun").ToString("0.###", ci)})";
            bool ok = g.sim.n24.shotgunTune && b > 0 && Math.Abs(a - b * g.cfg.Num("design.n24.shotgunTune.intervalMult")) <= 1;
            res[0] = pc + $" | {FeelProbe.FrameCount} frames: THUNDERCLAP shot-to-shot BEFORE {bs.ToString("0.000", ci)} s ({b} steps, {before.Count + 1} shots), AFTER {asec.ToString("0.000", ci)} s ({a} steps, {after.Count + 1} shots), ratio {ratio.ToString("0.000", ci)} (want 1.30); {why}; pump {pump.ToString("0.00", ci)} s"
                + (ok ? " | PASS" : g.sim.n24.shotgunTune ? " | RED: the ratio is not 1.30 within a step" : " | RED: design.n24.shotgunTune is off");
        }
    }
}
