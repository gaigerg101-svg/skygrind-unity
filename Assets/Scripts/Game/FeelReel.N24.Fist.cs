// NIGHT24 (builder F): the reel scenario `fist`, the contract charge-fist's Player half. UNCOMPILED and UNRUN when written (no Editor in the
// builder's worktree); the same story ran headless as `SimHarness fist` (PASS; `fist --off` is the RED run).
// The story (keyboard pass: forcedCombat punch presses; nothing forced on the sim itself):
//   idle    3 s standing about: the charge reads 0 (it never rises with time)
//   kill    a frozen husk 1.5 m ahead punched until dead: the charge rose by the hit and kill rates (fistCharge events)
//   flick   the charge set to FULL (labelled in the line: the harness proved the kill count, a reel does not farm 31 kills), one punch at the
//           open floor: the player is flicked along the aim; the distance moved in 1.5 s is printed beside 2.25 J; all the charge spent
//   lock    a second punch at once does not flick (the 3 s lockout)
// Every result line starts with FeelReel.Preconditions(g, pos) from SafeSpot(g). RED: the row off (no Fist24 wired), or any check failing.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N24FistBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = FistScenario(g, dir, res, offs, pre);
        }

        static IEnumerator FistScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            var fails = new List<string>();
            Fist24 f = g.sim.fist24;
            if (!g.sim.n24.chargeFist || f == null || !f.Active)
            {
                res[0] = pc + " | RED: design.n24.chargeFist is off (no charge, a plain punch)";
                yield break;
            }
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(20);
            g.husks.frozen = true;
            double hit = 0, kill = 0;
            int flickEvents = 0;
            double flickDistance = 0;
            offs.Add(g.events.On<Fist24ChargeEvent>("fistCharge", e =>
            {
                if (e.why == "hit") hit += e.gain;
                else if (e.why == "fistKill") kill += e.gain;
                FeelProbe.Event("fistCharge");
            }));
            offs.Add(g.events.On<Fist24FlickEvent>("fistFlick", e =>
            {
                flickEvents++;
                flickDistance = e.distance;
                FeelProbe.Event("fistFlick");
            }));
            Start(g, "fist", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();

            // --- idle: the charge does not rise with time -----------------------------------------------------------------------------
            for (int i = 0; i < Ms(3000); i++)
            {
                God(g);
                yield return null;
            }
            double idleCharge = f.charge;
            if (idleCharge != 0) fails.Add($"the charge read {idleCharge.ToString("0.000", ci)} after 3 s of standing about");

            // --- kill: a husk 1.5 m ahead punched until dead --------------------------------------------------------------------------
            Husk h = PlaceAt(g, 1.5, 0, out _);
            if (h == null) fails.Add("no husk placed 1.5 m ahead");
            else
            {
                f.noFlick = true; // this part banks the charge: the punches must not spend it
                int guard = 0;
                while (h.alive && guard++ < Ms(8000))
                {
                    God(g);
                    Look(g, h.AimPoint);
                    g.forcedCombat = (guard % 45) == 1 ? Punch : IdleCombat;
                    yield return null;
                }
                g.forcedCombat = IdleCombat;
                f.noFlick = false;
                yield return Frames(Ms(300));
                if (h.alive) fails.Add("the husk did not die to the punches");
                if (Math.Abs(kill - f.rules.fistKill) > 1e-9) fails.Add($"the kill paid {kill.ToString("0.000", ci)}, not {f.rules.fistKill.ToString("0.000", ci)}");
            }
            g.husks.KillAll();
            double chargeAfterKill = f.charge;

            // --- flick: FULL (set, labelled), one punch along the open floor -----------------------------------------------------------
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(Ms(600));
            f.charge = 1.0; // labelled: a reel does not farm the 31 kills the harness proved
            Vec3 from = g.Feet;
            g.forcedCombat = Punch;
            yield return null;
            g.forcedCombat = IdleCombat;
            int firstFlicks = f.flicks;
            yield return Frames(Ms(1500));
            double moved = Math.Sqrt((g.Feet.x - from.x) * (g.Feet.x - from.x) + (g.Feet.z - from.z) * (g.Feet.z - from.z));
            double want = f.rules.jumpMult[2] * f.rules.jumpDistance;
            if (firstFlicks != 1 || flickEvents != 1) fails.Add($"the FULL punch flicked {firstFlicks} times (fistFlick events {flickEvents}), not 1");
            if (f.charge != 0) fails.Add($"the charge read {f.charge.ToString("0.000", ci)} after the flick, not 0 (all spent)");
            if (moved < 0.5 * want) fails.Add($"the flick moved {moved.ToString("0.0", ci)} m of the {want.ToString("0.0", ci)} m asked (a wall close ahead cuts it: lastAllowed {f.lastAllowed.ToString("0.0", ci)} m)");

            // --- lock: a second punch at once ----------------------------------------------------------------------------------------
            f.charge = 0.5;
            g.forcedCombat = Punch;
            yield return null;
            g.forcedCombat = IdleCombat;
            if (f.flicks != 1) fails.Add($"a second punch inside the lockout flicked ({f.flicks} flicks)");
            yield return Frames(Ms(400));
            yield return Stop();

            res[0] = pc + $" | {FeelProbe.FrameCount} frames; idle 3 s charge {idleCharge.ToString("0.000", ci)}; one fist kill paid hit {hit.ToString("0.000", ci)} + kill {kill.ToString("0.000", ci)} (charge {chargeAfterKill.ToString("0.000", ci)}); FULL set (labelled), one punch flicked {moved.ToString("0.0", ci)} m in 1.5 s (asked {flickDistance.ToString("0.0", ci)} m of {want.ToString("0.0", ci)} = 2.25 J), charge after {f.charge.ToString("0.000", ci)}, a second punch inside {f.rules.lockout.ToString("0.#", ci)} s flicked {f.flicks - firstFlicks} more | "
                + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
