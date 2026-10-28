// NIGHT71 builder F (F4; Research/2026-10-26-forge71/scout-feel.md: "Release edges are not latched, so whether a KESTREL tap pressed and
// released inside a freeze still fires is not checked"). Reel phase taps71 (registered beside same63 / tour65 in FeelReel.N57.cs): a check,
// not a row; nothing in the game changes. Boot: reel.sh + -skygrindReelOnly taps71 on any map (SafeSpot, as a546).
// What the code says (Research/2026-10-26-wave71/F/F4/card.md): both readers latch a PRESS until the next fixed step (CombatReader's
// fireLatch, PadReader's frame.latched) and read HELD live, so a tap pressed and released inside a freeze reaches the first step after it as
// pressed with held false, which p39 takes as "a click inside one step" (Charge35.StepP39: the tap queued, fired when the gun's interval
// allows). BUT the n35-r3 rule there (pressed = input.fire.pressed && !q.pressedLast) merges a press with the step before it when that step
// was itself pressed, and a freeze cuts the frame's step loop right after the step that started it (SurvivalGame.Update: freezeCut): so if
// the freeze-starting shot fired ON a press step, the next step after the freeze reads the tap's press as the same press, and nothing fires.
// The two kinds, Taps71Reps each, every one on a fresh 1-hp husk ahead, the husks frozen, god mode, the KESTREL, a virtual pad's RT (Pad27):
//   rel  a p39 tap (pressed two frames, released) kills: the freeze starts on its RELEASE step (that step's press: none);
//   prs  a FULL charged shot into the sky first (charge35's 2.5 s cooldown then runs), then a press (held) that fires ON ITS PRESS STEP
//        and kills: the freeze starts on a pressed step (the trigger let go on the freeze's first frame).
// Then the tap: its press polled on a frame that runs no sim step (inside), its release polled on the next frame (before any step of it), and
// the KESTREL's shots counted for Taps71FireWaitS of game time after the freeze. Line (L-089: it prints that its input happened):
//   "<GREEN|RED> taps71 | taps inside freezes N, fired M | release-tap kills: inside a, fired b | press-step kills: inside c, fired d | ..."
// GREEN when N > 0 and M == N (no tap lost); each tap's own line names the kill step's press bit and the p39 counters it moved.
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static IEnumerator Taps71Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + " | map " + g.MapId;
            offs.Add(() =>
            {
                g.forcedCombat = IdleCombat;
                g.husks.frozen = false;
            });
            g.forcedCombat = null;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            double wallEnd = Time.realtimeSinceStartup + UnityTuning.Taps71WallS;
            int shots = 0, kills = 0;
            offs.Add(g.events.On("fire", o => { if (o is string w && w == "revolver") shots++; }));
            offs.Add(g.events.On<KillEvent>("kill", e => { if (e.owner == "player") kills++; }));
            yield return Equip27(g, "revolver");
            ChargeShots27 cs = g.loadout.charge27; // the live charge object (later nights put charge31's here, not sim.charge27)
            Charge35 q = cs != null ? cs.c35 : null;
            var lines = new List<string>();
            var inside = new int[2];
            var fired = new int[2];
            var notInside = new int[2];
            var noKill = new int[2];
            string[] kindName = { "release-tap kill", "press-step kill" };
            for (int kind = 0; kind < 2; kind++)
                for (int rep = 0; rep < UnityTuning.Taps71Reps && Time.realtimeSinceStartup < wallEnd; rep++)
                {
                    string tag = (kind == 0 ? "rel" : "prs") + (rep + 1);
                    pad.Send(0f, 0f);
                    g.forcedCombat = null;
                    g.Respawn(spot, spotYaw * 180 / Math.PI);
                    g.input.SetLook(spotYaw, 0);
                    g.husks.KillAll();
                    Husk bag = null;
                    double placed = 0;
                    foreach (double d in UnityTuning.Taps71DistancesM)
                    {
                        bag = PlaceAt(g, d, 0, out double _);
                        if (bag != null)
                        {
                            placed = d;
                            break;
                        }
                    }
                    if (bag == null)
                    {
                        lines.Add(tag + ": NO HUSK PLACED");
                        continue;
                    }
                    bag.hp = bag.maxHp = 1;
                    Husk target = bag;
                    Action aim = () =>
                    {
                        God(g);
                        if (target.alive) Look(g, target.AimPoint);
                    };
                    yield return Wait27(g, UnityTuning.Taps71SettleS, wallEnd, aim);
                    if (kind == 1)
                    {
                        // a FULL charged shot away from the husk, so the charge cools (charge35 cooldownS): a press then fires on its press step
                        double yaw0 = g.input.yaw;
                        Action away = () =>
                        {
                            God(g);
                            g.input.SetLook(yaw0 + Math.PI, UnityTuning.Taps71AwayPitchRad);
                        };
                        pad.Send(0f, 1f);
                        yield return Wait27(g, UnityTuning.Taps71ChargeHoldS, wallEnd, away);
                        pad.Send(0f, 0f);
                        yield return Wait27(g, UnityTuning.Taps71AfterChargeS, wallEnd, aim);
                        if (cs == null || cs.cooldown[0] <= 0 || !target.alive)
                        {
                            lines.Add(tag + ": the charge is not cooling (or the husk died): no press-step shot to test");
                            continue;
                        }
                    }
                    int k0 = kills;
                    pad.Send(0f, 1f);
                    if (kind == 0)
                    {
                        // a p39 tap: pressed two frames, released (it fires on the release step)
                        yield return null;
                        yield return null;
                        pad.Send(0f, 0f);
                    }
                    double w0 = Time.realtimeSinceStartup;
                    while (kills == k0 && Time.realtimeSinceStartup - w0 < UnityTuning.Taps71KillWaitS)
                    {
                        aim();
                        yield return null;
                    }
                    if (kills == k0)
                    {
                        pad.Send(0f, 0f);
                        noKill[kind]++;
                        lines.Add(tag + ": NO KILL in " + UnityTuning.Taps71KillWaitS + " s");
                        continue;
                    }
                    // this frame's Update ran the kill's step; the freeze holds the frames after it
                    bool killStepPressed = q != null && q.pressedLast;
                    int sKill = shots, step0 = g.sim.simStep;
                    int pt0 = q != null ? q.pressTaps39 : 0, rt0 = q != null ? q.releaseTaps39 : 0, ts0 = q != null ? q.tapShots : 0;
                    if (kind == 1)
                    {
                        pad.Send(0f, 0f); // the press-step shot's trigger let go, polled inside the freeze
                        yield return null;
                    }
                    // the tap: its press polled on a frame that runs no step, its release polled on the next frame (before that frame's steps)
                    pad.Send(0f, 1f);
                    yield return null;
                    bool pressInside = g.HeldThisFrame && g.sim.simStep == step0;
                    pad.Send(0f, 0f);
                    yield return null;
                    if (!pressInside)
                    {
                        notInside[kind]++;
                        lines.Add(tag + ": the freeze ended before the tap's press was polled (not counted)");
                        continue;
                    }
                    inside[kind]++;
                    while (g.sim.simStep == step0 && Time.realtimeSinceStartup < wallEnd) yield return null; // the freeze ends
                    yield return Wait27(g, UnityTuning.Taps71FireWaitS, wallEnd, () => God(g));
                    bool shot = shots > sKill;
                    if (shot) fired[kind]++;
                    int dpt = q != null ? q.pressTaps39 - pt0 : -1, drt = q != null ? q.releaseTaps39 - rt0 : -1, dts = q != null ? q.tapShots - ts0 : -1;
                    lines.Add($"{tag}: {kindName[kind]} at {placed} m (the kill step's press bit {killStepPressed}); a tap pressed and released inside its freeze: fired {(shot ? "yes" : "NO")} (KESTREL shots after the kill {shots - sKill}; p39 press taps +{dpt}, release taps +{drt}, tap shots +{dts})");
                }
            pad.Send(0f, 0f);
            int n = inside[0] + inside[1], m = fired[0] + fired[1];
            bool ok = n > 0 && m == n;
            res[0] = $"{(ok ? "GREEN" : "RED")} taps71 | taps inside freezes {n}, fired {m} | release-tap kills: inside {inside[0]}, fired {fired[0]} | press-step kills: inside {inside[1]}, fired {fired[1]} | not inside {notInside[0] + notInside[1]}, no kill {noKill[0] + noKill[1]} || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
