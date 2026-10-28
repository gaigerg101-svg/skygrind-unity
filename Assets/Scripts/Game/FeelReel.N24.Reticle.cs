// NIGHT24 (builder F): the reel scenario `reticle`, the contract reticle-arc's Player half. UNCOMPILED and UNRUN when written (no Editor in the
// builder's worktree). The story:
//   own     the ARCLINE owned (a.Own: the terminal's buy was proven by NIGHT22's `arcline2`; labelled), one husk in line ahead
//   empty   one grapple press yanks it: the arc's ARCLINE cell reads 0 (fill printed)
//   half    the cooldown fast-forwarded to half (a.Step() loops, labelled: the reel does not wait out 15 s): the fill reads about 0.5
//   ready   fast-forwarded to 0 left: the fill reads 1 and the cell flashes once; the flash's length in frames x the reel frame time is printed
//           (250 ms or less) and the alpha after it settles at the idle value (0.15-0.25)
// The result line prints the geometry (width x thickness, the px under the crosshair), the three fills, the idle alpha and the flash length.
// Every result line starts with FeelReel.Preconditions(g, pos) from SafeSpot(g). RED: the row off, the arc not built, or any check failing.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N24ReticleBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = ReticleScenario(g, dir, res, offs, pre);
        }

        static IEnumerator ReticleScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            var fails = new List<string>();
            Arcline21 a = g.sim.arcline21;
            if (!g.sim.n24.reticleArc)
            {
                res[0] = pc + " | RED: design.n24.reticleArc is off (no arc under the crosshair)";
                yield break;
            }
            if (!Ui.Arc24Built) fails.Add("the arc was not built (Ui.BuildN24Hud does not call BuildReticleArc24)");
            if (a == null || !g.sim.n21.arcline)
            {
                res[0] = pc + " | RED: design.n21.arcline is off (no held ability to show)";
                yield break;
            }
            a.Own(true); // labelled: the buy is NIGHT22's `arcline2`
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(20);
            g.husks.frozen = true;
            Husk h = PlaceAt(g, 10, 0, out _);
            if (h == null)
            {
                res[0] = pc + " | no husk could be placed 10 m ahead";
                yield break;
            }
            int pulls = 0;
            offs.Add(g.events.On("arclinePull", _ => { pulls++; FeelProbe.Event("arclinePull"); }));
            Look(g, h.AimPoint);
            Start(g, "reticle", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            yield return Frames(Ms(700));
            float readyFirst = Ui.Arc24FillNow[0];
            if (readyFirst < 0.99f) fails.Add($"the ARCLINE cell read {readyFirst.ToString("0.00", ci)} before any press, not 1");

            // --- empty: one press, the yank ----------------------------------------------------------------------------------------------
            g.husks.frozen = false;
            g.forcedMove = new InputFrame { grapple = new Skygrind.Core.Button { held = true, pressed = true } };
            yield return null;
            g.forcedMove = new InputFrame();
            yield return Frames(Ms(300));
            float empty = Ui.Arc24FillNow[0];
            if (pulls != 1) fails.Add($"the press yanked {pulls} times, not 1");
            if (empty > 0.1f) fails.Add($"just after the yank the cell read {empty.ToString("0.00", ci)}, not about 0");

            // --- half: the cooldown fast-forwarded -------------------------------------------------------------------------------------------
            int total = a.YankSteps;
            int skipped = 0;
            while (a.YankSteps > total / 2) { a.Step(); skipped++; }
            yield return Frames(4);
            float half = Ui.Arc24FillNow[0];
            if (half < 0.4f || half > 0.6f) fails.Add($"at half the cooldown the cell read {half.ToString("0.00", ci)}, not about 0.5");

            // --- ready: to 0 left, the flash --------------------------------------------------------------------------------------------------
            int flashes0 = Ui.Arc24FlashCount;
            while (a.YankSteps > 0) { a.Step(); skipped++; }
            int flashFrames = 0;
            float peak = 0f;
            for (int i = 0; i < Ms(700); i++)
            {
                yield return null;
                if (Ui.Arc24FlashNow > 0f) flashFrames++;
                peak = Math.Max(peak, Ui.Arc24FlashNow);
            }
            float ready = Ui.Arc24FillNow[0];
            float idleAlpha = Ui.Arc24AlphaNow[0];
            double flashMs = flashFrames * 1000.0 / UnityTuning.ReelFps;
            if (ready < 0.99f) fails.Add($"ready: the cell read {ready.ToString("0.00", ci)}, not 1");
            if (Ui.Arc24FlashCount - flashes0 != 1 || peak < 0.9f) fails.Add($"the ready flash fired {Ui.Arc24FlashCount - flashes0} times (peak {peak.ToString("0.00", ci)}), not once to full");
            if (flashMs > 250 + 1000.0 / UnityTuning.ReelFps) fails.Add($"the ready flash lasted {flashMs.ToString("0", ci)} ms, over 250");
            if (idleAlpha < 0.15f || idleAlpha > 0.25f) fails.Add($"the idle alpha after the flash was {idleAlpha.ToString("0.00", ci)}, not 0.15-0.25");
            yield return Frames(Ms(300));
            yield return Stop();

            res[0] = pc + $" | {FeelProbe.FrameCount} frames; arc {UnityTuning.Arc24Width.ToString("0", ci)} x {UnityTuning.Arc24Thick.ToString("0", ci)} px, {UnityTuning.Arc24EndsBelow.ToString("0", ci)}-{UnityTuning.Arc24MidBelow.ToString("0", ci)} px under the crosshair; ARCLINE cell fill empty {empty.ToString("0.00", ci)} / half {half.ToString("0.00", ci)} / ready {ready.ToString("0.00", ci)} (cooldown fast-forwarded {(skipped / 120.0).ToString("0.0", ci)} s, labelled); ready flash {flashMs.ToString("0", ci)} ms (peak {peak.ToString("0.00", ci)}); idle alpha {idleAlpha.ToString("0.00", ci)}; no text, no red | "
                + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
