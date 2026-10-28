// NIGHT22 U3-36.5 DEADLINE bar (builder S): the reel scenario `deadline` (contract `deadline`), written FIRST and RED while the bar is
// absent. UNCOMPILED when written (the builder has no Editor). On the Player:
//   1. a SafeSpot respawn; the DEADLINE twist forced on round 2 (specials.ForceTwist); the aim bot clears round 1 with the health held,
//      the first break ended at once (as `roundend`), and again before round 2;
//   2. at the twist's start (specialRound "twist:deadline" "start") the recording begins: 5 s of fighting (the kills pay "+N" pops),
//      then no fire and no move until the fuse burns out once (at most 22 s: the fuse is 18 s) so the pulse and the burn flash show;
//   3. every frame the bar is sampled (Ui.Deadline22*): visible, the number's size in px (want 18 or more), its text, the fill's colour
//      (DANGER at rest), the pulse (on in the last 5 s), the live pops and the flashes.
// The result line starts with the preconditions (SafeSpot's), then the numbers, then PASS or RED with every failed part named. The
// feel-metrics half (M9 entrance, M10 text size) reads the recording; the chase half (50 % at rounds 10 and 15) is the harness's
// `deadline2 --chase`. With design.n21.deadline off (-skygrindOff21) there is no twist and no bar: that is the RED.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N22DeadlineBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = N22DeadlineRun(g, dir, res, offs, pre);
        }

        static IEnumerator N22DeadlineRun(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            g.run.phaseSteps = 1;
            g.husks.frozen = false;
            yield return Frames(10);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (UNDER 12 m)");
            res[0] = pc + "; RED: the scenario did not reach the twist";
            Ui ui = g.UiView;
            if (!g.sim.n21.deadline || g.sim.deadline21 == null || !g.sim.deadline21.on || g.sim.specials == null)
            {
                res[0] = pc + "; RED: design.n21.deadline is off (the twist is not in the draw: a round with no bar)";
                yield break;
            }
            if (ui == null || !ui.Deadline22Built)
            {
                res[0] = pc + "; RED: the HUD has no fuse bar (Deadline22 not built)";
                yield break;
            }
            g.sim.specials.ForceTwist(2, Deadline21.TwistId);
            bool cleared1 = false, started = false;
            int burns = 0, kills = 0;
            offs.Add(g.events.On("roundClear", _ => cleared1 = true));
            offs.Add(g.events.On(N20Events.SpecialRound, o =>
            {
                var e = (SpecialRoundEvent)o;
                if (e.name == "twist:" + Deadline21.TwistId && e.phase == "start") started = true;
            }));
            offs.Add(g.events.On(Deadline21.Burn, _ => burns++));
            offs.Add(g.events.On("kill", _ => { if (started) kills++; }));

            // 1. round 1 off the record, then through its break to round 2's twist
            int guard = 0;
            while (!started && guard++ < 14400)
            {
                God(g);
                if (!cleared1) FeelStrip.AimBot(g);
                else
                {
                    g.forcedCombat = IdleCombat;
                    g.run.phaseSteps = 1; // the break ends at once
                }
                yield return null;
            }
            if (!started)
            {
                g.forcedCombat = IdleCombat;
                res[0] = pc + $"; RED: the DEADLINE twist never started on round 2 (round 1 cleared {cleared1})";
                yield break;
            }

            // 2. the recording: fight 5 s, then stand until the burn
            Start(g, "deadline", "ui", dir);
            FeelProbe.MarkInput();
            int visibleFrames = 0, pulseFrames = 0, maxPops = 0, firstVisible = -1, f = 0;
            double pulseAt = -1, fuseFull = g.sim.deadline21.Full * g.cfg.Dt;
            float minPx = float.MaxValue;
            bool dangerSeen = false, textSeen = false, burnedBy = false;
            string sample = "";
            int fightFrames = Ms(5000), maxFrames = Ms(5000) + Ms(22000);
            while (f < maxFrames && !(burns >= 1 && f > fightFrames + 20))
            {
                God(g);
                if (f < fightFrames) FeelStrip.AimBot(g);
                else
                {
                    g.forcedCombat = IdleCombat;
                    g.forcedMove = new InputFrame();
                }
                yield return null;
                f++;
                if (ui.Deadline22Visible)
                {
                    visibleFrames++;
                    if (firstVisible < 0) firstVisible = f;
                    if (ui.Deadline22NumberPx < minPx) minPx = ui.Deadline22NumberPx;
                    string t = ui.Deadline22NumberText;
                    if (!string.IsNullOrEmpty(t))
                    {
                        textSeen = true;
                        sample = t;
                    }
                    if (ui.Deadline22Pulsing)
                    {
                        pulseFrames++;
                        if (pulseAt < 0) pulseAt = g.sim.deadline21.LeftSeconds;
                    }
                    else if (ColorNear(ui.Deadline22FillColor, UiTheme.Danger)) dangerSeen = true;
                    if (ui.Deadline22PopsLive > maxPops) maxPops = ui.Deadline22PopsLive;
                }
            }
            burnedBy = burns >= 1;
            g.forcedCombat = IdleCombat;
            yield return Frames(Ms(600)); // the flash and the last frames
            yield return Stop();

            var why = new StringBuilder();
            if (visibleFrames == 0) why.Append(" the bar never showed;");
            else
            {
                if (minPx < 18) why.Append(" the number is ").Append(minPx.ToString("0")).Append(" px (want 18 or more);");
                if (!textSeen) why.Append(" the number was empty;");
                if (!dangerSeen) why.Append(" the fill was never DANGER at rest;");
                if (pulseFrames == 0) why.Append(" no pulse in the last 5 s;");
                else if (pulseAt > UnityTuning.Deadline22PulseSeconds + 0.5) why.Append(" the pulse began at ").Append(pulseAt.ToString("0.0")).Append(" s left (want about 5);");
            }
            if (ui.Deadline22PopsStarted == 0) why.Append(" no +N pop (kills ").Append(kills).Append(");");
            if (!burnedBy) why.Append(" the fuse never burned out;");
            else if (ui.Deadline22Flashes == 0) why.Append(" no burn flash (FLASH INTENSITY 0 or the gate?);");
            res[0] = $"{pc}; fuse {fuseFull:0} s, bar visible {visibleFrames} frames from frame {firstVisible}, number {(minPx == float.MaxValue ? 0 : minPx):0} px (want 18 or more), last text \"{sample}\", fill DANGER at rest {dangerSeen}; pulse {pulseFrames} frames from {pulseAt:0.0} s left (want about 5); +N pops started {ui.Deadline22PopsStarted} (kills {kills}, most live {maxPops}); burns {burns}, flashes {ui.Deadline22Flashes}; {(why.Length == 0 ? "PASS" : "RED:" + why)}";
            yield break;
        }

        static bool ColorNear(Color a, Color b) =>
            Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b) < 0.03f;
    }
}
