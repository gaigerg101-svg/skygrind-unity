// NIGHT42 builder W1: the Player scenario `weapons42` (the dispatch: FeelReel.N42.cs). Not run by its author (no Player in the worktree).
// Boot as weapons41 (reel.sh + -skygrindMap <a campaign level> + -skygrindReelOnly weapons42, L-062); the before = the same reel with
// -skygrindOff42 (or the rows off), the after = gauntlet42 / parry42 / charge42 / reticle42 on. The player on SafeSpot's floor, God, a 1e9-hp
// frozen husk at 6 m is the target; the trigger is a virtual pad's RT (Pad27). Frames (shot-N30-<name>.png), t = GAME time:
//   W42-tap-000 / -100 / -200              a KESTREL tap: the reticle's quarter turn (reticle42: over by 0.27 s; before 0.34 s)
//   W42-cannon-000 / -100 / -200 / -450    the KESTREL's charged shot: the cannon turn (reticle42: over by 0.46 s; before 1.82 s)
//   W42-held-cool                          G10: RT pressed again at once and HELD, 1.2 s into the charge's cooldown: before = auto taps
//                                          firing (the line counts them), after = nothing fires
//   W42-held-rearm / W42-held-full         the held trigger as the cooldown ends (+0.3 s after charge42Rearm; before: +2.6 s after the
//                                          re-press) and at FULL: after = the second charge glowing, before = still tapping
//   W42-held-charged-000                   the release: after = the second charged shot
//   W42-parry-000 / -050 / -150            G4: the PULSE gauntlet in hand, a DRONE orb (12 m/s) fired at the chest from 8 m, RT pressed the
//                                          frame the orb touches the player: after = parried inside the mercy window (the orb goes back),
//                                          before = the hit lands (the line prints parried / hurt)
// Line: GREEN when every frame was saved and the shots fired; the counts printed are the proof numbers (auto taps while held, rearms,
// parried) and each frame's telemetry spin / charge.
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
        static partial void N42Weapons(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Weapons42Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Weapons42Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)")
                + $" | map {g.MapId} | n42 {sim.n42} | reticle42 view {N42View.RowOn(g.cfg, "reticle42")}";
            offs.Add(() => { g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = null;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "weapons42", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 150;
            Husk bag = PlaceAt(g, 6, 0, out double _);
            if (bag != null) bag.hp = bag.maxHp = 1e9;
            bool lookBag = true, holdRt = false;
            Action aim = () => { God(g); if (holdRt) pad.Send(0f, 1f); if (lookBag && bag != null && bag.alive) { bag.hp = 1e9; Look(g, bag.AimPoint); } };
            int fires = 0, charged = 0, rearms = 0, parries = 0, hurts = 0;
            offs.Add(g.events.On<string>("fire", w => fires++));
            offs.Add(g.events.On<ChargeShot27Event>("chargeShot27Fire", e => charged++));
            offs.Add(g.events.On<ChargeShot27Event>("charge42Rearm", e => rearms++));
            offs.Add(g.events.On<HitEvent>("parry", e => parries++));
            offs.Add(g.events.On<DroneOrbs.DestroyedEvent>("orbDestroyed", e => { if (e.reason == "player") hurts++; })); // the orb landed on the player
            var lines = new List<string>();
            int framesSaved = 0;
            string Tele() => $"spin {(Hud38.Live ? Hud38.SpinDeg / UnityTuning.Hud38SpinDeg : -1f).ToString("0.00", ci)} kind {Hud40.SpinKind} charge {g.loadout.Charge35Level.ToString("0.00", ci)}";
            IEnumerator Snap(string name)
            {
                lines.Add(name + ": " + Tele());
                yield return Shot30(dir, name);
                framesSaved++;
            }
            IEnumerator At(double t0, double off)
            {
                while (sim.SimTime - t0 < off && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
            }

            // ---- the KESTREL: a tap, the reticle at +0 / +0.1 / +0.2 s ----
            yield return Equip27(g, "revolver");
            g.forcedCombat = null;
            yield return Wait27(g, 0.8, wallEnd, aim);
            int f0 = fires;
            pad.Send(0f, 1f);
            yield return Frames(2);
            pad.Send(0f, 0f);
            double w0 = sim.SimTime;
            while (fires == f0 && sim.SimTime - w0 < 1.5 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
            double tTap = sim.SimTime;
            foreach (double off in new[] { 0.0, 0.1, 0.2 })
            {
                yield return At(tTap, off);
                yield return Snap("W42-tap-" + ((int)Math.Round(off * 1000)).ToString("000", ci));
            }

            // ---- the KESTREL: FULL, release (the cannon turn), then RT again at once, HELD through the cooldown + FULL ----
            ChargeShots27 c = g.loadout.charge27;
            if (c != null) yield return Wait27(g, c.cooldown[0] * sim.step + 0.1, wallEnd, aim);
            yield return Wait27(g, 0.6, wallEnd, aim);
            double kFull = c != null ? c.FullSteps(0) * sim.step : 0.9;
            double kCool = sim.cfg.Num("design.n35.charge35.revolver.cooldownS");
            pad.Send(0f, 1f);
            yield return Wait27(g, kFull + 0.1, wallEnd, aim);
            int ch0 = charged;
            pad.Send(0f, 0f);
            w0 = sim.SimTime;
            while (charged == ch0 && sim.SimTime - w0 < 1.0 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
            double tCannon = sim.SimTime;
            int firesAtCannon = fires;
            yield return Frames(1);
            pad.Send(0f, 1f); // pressed again at once, and held
            holdRt = true; // n42-r2 iteration: RT re-sent every frame while held (a dropped pad state ended the hold: 3 re-arms, no second shot)
            foreach (double off in new[] { 0.0, 0.1, 0.2, 0.45 })
            {
                yield return At(tCannon, off);
                yield return Snap("W42-cannon-" + ((int)Math.Round(off * 1000)).ToString("000", ci));
            }
            yield return At(tCannon, 1.2);
            yield return Snap("W42-held-cool");
            int r0 = rearms;
            w0 = sim.SimTime;
            while (rearms == r0 && sim.SimTime - tCannon < kCool + 0.3 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
            double tRearm = sim.SimTime;
            int ch1 = charged; // n42-r2 iteration: the second charged shot counted from the re-arm (a release or holdMaxS's own fire)
            yield return At(tRearm, 0.3);
            yield return Snap("W42-held-rearm");
            yield return At(tRearm, kFull + 0.05);
            yield return Snap("W42-held-full");
            int tapsHeld = fires - firesAtCannon; // every shot while held (the re-press's own tap included)
            yield return At(tRearm, kFull + 0.4); // well past FULL (+0.4 s; holdMaxS 1.0 s would fire it itself)
            holdRt = false;
            pad.Send(0f, 0f);
            w0 = sim.SimTime;
            while (charged == ch1 && sim.SimTime - w0 < 1.5 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
            yield return Snap("W42-held-charged-000");
            bool second = charged > ch1;

            // ---- G4: the gauntlet in hand, a DRONE orb at the chest, RT the frame it touches ----
            yield return Equip27(g, "gauntlet");
            g.forcedCombat = null;
            yield return Wait27(g, 2.6, wallEnd, aim); // the swing's cooldown (2.5 s before gauntlet42) is over
            Vec3 eye = sim.Eye;
            Vec3 fwd = new Vec3(-Math.Sin(spotYaw), 0, -Math.Cos(spotYaw));
            var src = new Vec3(eye.x + fwd.x * 8, eye.y - 0.4, eye.z + fwd.z * 8);
            DroneOrbs orbs = sim.husks.orbs;
            DroneOrbs.Orb orb = orbs != null ? orbs.Fire(src, Vec3.Normalize(new Vec3(eye.x, eye.y - 0.4, eye.z) - src), null, 12) : null;
            lookBag = false;
            Look(g, src);
            int p0 = parries;
            string parryLine;
            if (orb == null) parryLine = "parry: NO ORB (no pool)";
            else
            {
                w0 = sim.SimTime;
                // n42-r2 iteration (the lead): the press when the orb is inside 1.5x the reach (orb.reflectRange x 1.5 = 3 m of the eye), read
                // every frame while the view follows the orb; RT held 2 frames (a press the sim sees on its steps)
                double pressAt = 1.5 * sim.cfg.Num("orb.reflectRange");
                double dPress = -1;
                while (orb.alive && !orb.reflected && sim.SimTime - w0 < 1.5 && Time.realtimeSinceStartup < wallEnd)
                {
                    God(g);
                    Look(g, orb.position);
                    double d = Vec3.DistanceTo(orb.position, sim.Eye);
                    if (d <= pressAt) { dPress = d; break; }
                    yield return null;
                }
                pad.Send(0f, 1f);
                double tPress = sim.SimTime;
                yield return Snap("W42-parry-000");
                yield return Frames(1);
                pad.Send(0f, 0f);
                yield return At(tPress, 0.05);
                yield return Snap("W42-parry-050");
                yield return At(tPress, 0.15);
                yield return Snap("W42-parry-150");
                parryLine = $"parry: {(parries > p0 ? "PARRIED" : "missed")} (pressed at {dPress.ToString("0.00", ci)} m of the eye, orb reflected {orb.reflected}, mercy held {orbs.mercyHeld42}, buffer parries {g.loadout.orbBufferParries42}), hurt {hurts}";
            }
            lookBag = true;
            yield return Wait27(g, 0.4, wallEnd, aim);
            yield return Stop();

            bool ok = framesSaved == 14 && charged >= 1;
            res[0] = $"{(ok ? "GREEN" : "RED")} weapons42 | G10: shots while RT held through the KESTREL's cooldown {tapsHeld} (the re-press's tap included), "
                + $"charge42Rearm {rearms}, second charged shot {second} | {parryLine} | frames {framesSaved}/14 || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
