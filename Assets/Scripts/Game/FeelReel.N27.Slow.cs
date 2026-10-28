// NIGHT27 (builder S): the Player scenario `slowworld` (the lead's FeelReel.N27.cs calls N27SlowworldBody). The wall-ride aim slow-down (design.n27.slowAim27, SurvivalGame.SlowAim27.cs):
// THE LANTERN's room is started through BossRoomBegin("hulk") and the pilot of the `lantern` scenario drives (FeelReel.N27.Bosses.cs: the same per-sim-step stepping and merged presses). On top of
// the pilot's input the scenario HOLDS altFire (LT, the aim) through g.forcedCombat on every frame the mover is wall-riding and lets go otherwise: the aim button a player has. The first two
// rides are a CONTROL: LT stays off, the scale must stay 1. Each frame: g.WorldScale, g.SlowAim01, the mover's state, loadout.adsActive, Time.timeScale and the sim steps that passed.
// Bounded by game time (120 s) and a wall guard (120 s); the boss's HP is never written. Nothing is read from a dictionary.
// NIGHT39 F1: with design.n39.slow39 on, the expected numbers are slow39's (scale, ease in / out). The row is judged on its OWN share
// (SurvivalGame.SlowAimShare), not WorldScale: THE LANTERN's floor (NIGHT29, design.n29.boss1.slowScale 0.55) slows the world without a rider, so
// WorldScale counted it as this row's leak since NIGHT29 (n39-r1: 4397 frames; n39slow lantern headless: 4640 of 4658 with the row's share at 1).
// Frames slowed by another system, the whip latch and the parry window are reported apart.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using Btn = Skygrind.Core.Button;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N27SlowworldBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Slowworld27Scenario(g, dir, res, offs, pre);

        static IEnumerator Slowworld27Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.Lantern27On) { res[0] = "RED: design.n27.boss1 is off (no lantern to ride around) | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            g.husks.frozen = true;
            if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yawDeg) || sim.lantern27 == null || !sim.lantern27.Alive) { res[0] = "RED: BossRoomBegin(hulk) did not start the lantern | " + pc; yield break; }
            Lantern27 l = sim.lantern27;
            g.Respawn(spawn, yawDeg);
            pc = Preconditions(g, spawn);
            yield return Frames(20);
            Start(g, "slowworld", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double scaleWant = g.cfg.Num("design.n27.slowAim27.scale"), easeIn = g.cfg.Num("design.n27.slowAim27.easeInS"), easeOut = g.cfg.Num("design.n27.slowAim27.easeOutS");
            // NIGHT39 F1: with design.n39.slow39 on (not -skygrindOff39) the row's own numbers
            Slow39Rules r39 = Slow39Rules.From(g.cfg);
            if (r39.on) { scaleWant = r39.scale; easeIn = r39.easeInS; easeOut = r39.easeOutS; }
            double fps = UnityTuning.ReelFps;
            int outFrames = (int)Math.Ceiling(easeOut * fps) + 3;
            double wallEnd = Time.realtimeSinceStartup + 120;
            var pilot = new LanternPilot27 { hand = new Mulberry32(Seed * 7919u + 13u) };
            var drive = new PilotDrive27 { tick = sim.simStep };
            double yawL = yawDeg * Math.PI / 180;
            int step0 = sim.simStep, frame = 0, deaths = 0, rides = 0, lastTargetFrame = -100000, frozenFrames = 0;
            bool wasRiding = false, wasTarget = false, easing = false;
            double minScale = 1, easeStartT = 0, firstEase = -1, easeSum = 0, tsMin = 1, tsMax = 1;
            int easeN = 0, targetFrames = 0, leakFrames = 0, ctlFrames = 0, ctlWrong = 0, ridingNoAdsOne = 0, ridingNoAds = 0;
            long slowSteps = 0, outSteps = 0;
            double slowReal = 0, outReal = 0;
            int slowFrames = 0, outFrames2 = 0;
            double maxSlow01 = 0;
            int prevStep = sim.simStep;
            // NIGHT39 F1: the row is judged on its OWN share (g.SlowAimShare), not WorldScale: THE LANTERN's floor (design.n29.boss1.slowScale 0.55,
            // NIGHT29, live most of this fight) slows WorldScale while nobody rides; n39-r1 counted 4397 such frames as this row's (n39slow lantern:
            // 4640 of 4658 "leaks" headless had the row's own share at 1). Those frames are counted apart (otherFrames), and so are the whip latch
            // and the parry window (slow39's own conditions, not leaks).
            double minOwn = 1;
            int otherFrames = 0, latchFrames = 0, parryFrames = 0, lastLatchFrame = -100000;
            while (l.Alive && (sim.simStep - step0) * sim.step < 120.0 && Time.realtimeSinceStartup < wallEnd)
            {
                frame++;
                if (Down27(g, "hulk")) { deaths++; drive.tick = sim.simStep; }
                drive.Advance(sim.simStep, t =>
                {
                    pilot.Step(sim, yawL, t);
                    yawL = pilot.Move.yaw;
                    return (pilot.Move, pilot.Combat);
                });
                InputFrame mv = drive.mv;
                CombatInput cb = drive.cb;
                if (cb.fire.held) { bool tap = frame % 2 == 0; cb.fire = new Btn { held = tap, pressed = tap }; }
                bool riding = sim.mover.state == MoveState.WallRide;
                if (riding && !wasRiding) rides++;
                wasRiding = riding;
                bool control = rides <= 2; // the first two rides: LT stays off
                bool lt = riding && !control;
                cb.altFire = lt ? new Btn { held = true, pressed = !wasTarget && !sim.loadout.adsActive } : default;
                g.input.SetLook(mv.yaw, mv.pitch);
                g.forcedMove = mv;
                g.forcedCombat = cb;
                yield return null;
                // what this frame did (the loop's Update has run)
                double scale = g.WorldScale, slow01 = g.SlowAim01;
                bool ads = sim.loadout.adsActive;
                bool nowRiding = sim.mover.state == MoveState.WallRide;
                bool target = nowRiding && ads;
                double ts = Time.timeScale;
                tsMin = Math.Min(tsMin, ts);
                tsMax = Math.Max(tsMax, ts);
                int stepsNow = sim.simStep - prevStep;
                prevStep = sim.simStep;
                double real = Time.unscaledDeltaTime;
                maxSlow01 = Math.Max(maxSlow01, slow01);
                if (scale <= 0) { frozenFrames++; continue; } // a hitstop freeze: not this row's
                minScale = Math.Min(minScale, scale);
                double own = g.SlowAimShare; // NIGHT39 F1: the row's own share this frame
                minOwn = Math.Min(minOwn, own);
                if (scale < 0.999 && own >= 0.999) otherFrames++;
                bool latched = r39.on && r39.latch && sim.Slow39Latched;
                if (latched) { latchFrames++; lastLatchFrame = frame; }
                bool parryWin = g.Slow39State != null && g.Slow39State.ParryWindow;
                if (parryWin) parryFrames++;
                if (target)
                {
                    targetFrames++;
                    lastTargetFrame = frame;
                    if (!wasTarget) { easing = true; easeStartT = Time.unscaledTime; }
                }
                if (easing && own <= scaleWant + 0.01)
                {
                    double dtE = Time.unscaledTime - easeStartT;
                    if (firstEase < 0) firstEase = dtE;
                    easeSum += dtE;
                    easeN++;
                    easing = false;
                }
                if (!target && !nowRiding) easing = false;
                wasTarget = target;
                if (own < 0.999 && !target && frame - lastTargetFrame > outFrames && frame - lastLatchFrame > outFrames && !parryWin) leakFrames++;
                if (nowRiding && !ads)
                {
                    ridingNoAds++;
                    if (own >= 0.999) ridingNoAdsOne++;
                }
                if (control && nowRiding && !ads && lastTargetFrame < 0)
                {
                    ctlFrames++;
                    if (own < 0.999) ctlWrong++;
                }
                if (own <= scaleWant + 0.01) { slowSteps += stepsNow; slowReal += real; slowFrames++; }
                else if (scale >= 0.999) { outSteps += stepsNow; outReal += real; outFrames2++; }
            }
            bool killed = !l.Alive;
            double gameSeconds = (sim.simStep - step0) * sim.step;
            Quiet(g);
            yield return Frames(Ms(500));
            yield return Stop();
            double slowRate = slowReal > 0 ? slowSteps / slowReal : 0, outRate = outReal > 0 ? outSteps / outReal : 0;
            var fails = new List<string>();
            if (targetFrames == 0) fails.Add("never riding AND aiming (no slow-down could be measured)");
            if (targetFrames > 0 && (minOwn < Math.Min(scaleWant, r39.on && r39.parry ? r39.parryScale : 1) - 0.01 || minOwn > scaleWant + 0.01)) fails.Add($"the row's minimum own share was {minOwn.ToString("0.000", ci)}, not {scaleWant.ToString("0.00", ci)}{(r39.on ? " (slow39)" : "")}");
            if (leakFrames > 0) fails.Add($"{leakFrames} frames with the world slowed while not riding + aiming (outside the {easeOut.ToString("0.00", ci)} s ease-out + 3 frames)");
            if (firstEase < 0 && targetFrames > 0) fails.Add("the scale never reached the slow after the first riding + aiming frame");
            if (firstEase > easeIn + 0.1) fails.Add($"the ease-in took {firstEase.ToString("0.000", ci)} s, expected about {easeIn.ToString("0.00", ci)} s");
            if (ctlFrames == 0) fails.Add("no control frames (riding without LT before the first aimed ride)");
            if (ctlWrong > 0) fails.Add($"{ctlWrong} of {ctlFrames} control frames (riding, LT off) had a scale under 1");
            if (tsMin != 1 || tsMax != 1) fails.Add($"Time.timeScale left 1 ({tsMin.ToString("0.00", ci)}..{tsMax.ToString("0.00", ci)})");
            if (slowFrames > 0 && outFrames2 > 0 && !(slowRate < outRate * 0.9)) fails.Add($"the sim ran {slowRate.ToString("0", ci)} steps per real second in the slow against {outRate.ToString("0", ci)} outside: not slower");
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, pilot as in `lantern` (rides {rides}), LT held through g.forcedCombat on riding frames after 2 control rides, boss HP never written: "
                + $"row {(r39.on ? "slow39" : "slowAim27")}: min own share {minOwn.ToString("0.000", ci)} (design {scaleWant.ToString("0.00", ci)}), min WorldScale {minScale.ToString("0.000", ci)}, max SlowAim01 {maxSlow01.ToString("0.00", ci)}; riding + aiming frames {targetFrames}; "
                + $"frames slowed by another system with the row's own share at 1 (THE LANTERN's floor design.n29.boss1.slowScale, the bend, the impact layer; not this row's): {otherFrames}; whip-latched frames {latchFrames}, parry-window frames {parryFrames}; "
                + $"ease-in to <= {(scaleWant + 0.01).ToString("0.00", ci)}: first {(firstEase < 0 ? "never" : firstEase.ToString("0.000", ci) + " s")}, mean {(easeN > 0 ? (easeSum / easeN).ToString("0.000", ci) + " s" : "n/a")} over {easeN} rides (design {easeIn.ToString("0.00", ci)} s); "
                + $"frames slowed while NOT riding + aiming (past ease-out {easeOut.ToString("0.00", ci)} s + 3 frames): {leakFrames}; hitstop frames skipped {frozenFrames}; "
                + $"control (LT off, first 2 rides): {ctlFrames} riding frames, scale under 1 in {ctlWrong}; all riding frames without ads: {ridingNoAds}, at scale 1: {ridingNoAdsOne}; "
                + $"sim steps per real second: {slowRate.ToString("0", ci)} in the slow ({slowFrames} frames, {(slowFrames > 0 ? (double)slowSteps / slowFrames : 0).ToString("0.00", ci)} per frame) vs {outRate.ToString("0", ci)} outside ({outFrames2} frames, {(outFrames2 > 0 ? (double)outSteps / outFrames2 : 0).ToString("0.00", ci)} per frame); Time.timeScale {tsMin.ToString("0.00", ci)}..{tsMax.ToString("0.00", ci)}; "
                + $"{gameSeconds.ToString("0.0", ci)} s of game time, lantern {(killed ? "killed" : "alive")}, deaths {deaths}";
        }
    }
}
