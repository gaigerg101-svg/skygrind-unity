// NIGHT36 builder F (gun feel): the Player scenarios of F's rows (FeelReel.N27.cs PhaseN36F -> N36GunsBody). On the player's own input path (a virtual
// pad's RT, PadReader reads it as a player's; L-031), whole Player frames (L-039). One Player per scenario and map (L-046). Not run by its author (no
// Player in the worktree).
//   tap36     boot on the campaign's LEVEL 1 (the KESTREL; `-skygrindMap <Curriculum25.Order[0]>`) or LEVEL 3 (THE THUNDERCLAP, Order[2]); any other map: the
//             KESTREL. A safe spot, husks killed and frozen, god mode; one bag husk (1e9 hp) 6 m ahead, aimed at.
//     TAP: RT down 0.06 s (a real click), up. Measured: the frames from the press to the gun's "fire" (want 0-1: the press's own frame or the next),
//       the shots in the next 0.5 s (want 1), charge35Start (want 0). Shots: shot-N30-tap36-<gun>-press (the first frame after the press: the muzzle flash,
//       the kick started, the tracer from the barrel), -press2 (the next frame).
//     HOLD: RT down 0.8 s, up. Measured: the press shot's frame (want 0-1 after the press), shots while held after it (want 0), the rail's word at 0.35 s
//       (want CHARGING) and at 0.1 s (want empty: a tap never reads CHARGING), the charged shot at the release (charge35Fire 1).
//       Shots: -hold-early (0.1 s: no CHARGING), -hold-charging (0.35 s), -release (the frame after the release: the beam / slug).
//     GREEN (tap36 on): TAP shot within 1 frame, 1 shot, no charge; HOLD press shot within 1 frame, 0 shots while held, 1 charged on release, the words.
//     RED (-skygrindOff36): charge35's rule: the tap's shot comes at the release (~4 frames at 60 fps after the press), the hold has no press shot.
//     NIGHT39 P1 (design.n39.p39 live, the KESTREL only): TAP's shot is measured from the RELEASE frame (want 0-2), charge35Start 0; HOLD wants NO
//       press shot (0 normal before the charge), 0 while held, 1 charged on release; the words as before. -skygrindOff39: tap36's lines above.
//   crunch36, fist36: later slices (NOT WRITTEN until then).
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
        static partial void N36GunsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "tap36") body = Tap36Scenario(g, dir, res, offs, pre);
            else if (s == "crunch36") body = Crunch36Scenario(g, dir, res, offs, pre);
            else if (s == "fist36") body = Fist36Scenario(g, dir, res, offs, pre);
        }

        //   fist36  any map with a safe spot. Husks killed and frozen, god mode, nothing ahead (the fist in the open air).
        //     QUICK PUNCH with the KESTREL in hand: shots -quick-out (2 frames after the press: the hand, knuckles, thumb across the fingers) and -quick-late.
        //     GAUNTLET: equip it; -gauntlet-rest; one RT press (its jab), shots -gauntlet-lunge (0.06 s: the lunge and the corkscrew) and -gauntlet-follow
        //       (0.12 s: the follow-through); then a `parry36` event emitted by the reel (P's event; the view's pose only) and -gauntlet-parry (0.1 s:
        //       the guard, up and palm-out).
        //     GREEN (fist36 on): the gauntlet's drawn lunge > its web jab (Viewmodel.Fist36LungeMax > 1.2 x), the guard reached (Fist36ParryMax > 0.9).
        //     RED (-skygrindOff36): the block fist; no lunge past the web's, no guard. The frames are the proof: look at the hand.
        static IEnumerator Fist36Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            bool row = N36Fixes.ViewRowOn(g.cfg, "fist36");
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | map {g.MapId} | fist36 {row}, BlockyKit.Fist36 {BlockyKit.Fist36}";
            offs.Add(() => { g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = null;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "fist36", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 90;
            Action idle = () => God(g);
            // ---- the quick punch (the arm) ----
            yield return Equip27(g, "revolver");
            g.forcedCombat = null;
            yield return Wait27(g, 0.5, wallEnd, idle);
            g.forcedCombat = Punch;
            yield return Frames(1);
            g.forcedCombat = null;
            yield return Frames(1);
            yield return Shot30(dir, "fist36-quick-out");
            yield return Wait27(g, 0.12, wallEnd, idle);
            yield return Shot30(dir, "fist36-quick-late");
            yield return Wait27(g, 0.6, wallEnd, idle);
            // ---- the gauntlet ----
            yield return Equip27(g, "gauntlet");
            g.forcedCombat = null;
            yield return Wait27(g, 0.6, wallEnd, idle);
            yield return Shot30(dir, "fist36-gauntlet-rest");
            Viewmodel.Fist36LungeMax = 0;
            Viewmodel.Fist36ParryMax = 0;
            pad.Send(0f, 1f);
            yield return Wait27(g, 0.06, wallEnd, idle);
            pad.Send(0f, 0f);
            yield return Shot30(dir, "fist36-gauntlet-lunge");
            yield return Wait27(g, 0.06, wallEnd, idle);
            yield return Shot30(dir, "fist36-gauntlet-follow");
            yield return Wait27(g, 1.2, wallEnd, idle);
            float lunge = Viewmodel.Fist36LungeMax;
            g.events.Emit("parry36", new HitEvent { owner = "player", position = g.Eye });
            yield return Wait27(g, 0.1, wallEnd, idle);
            yield return Shot30(dir, "fist36-gauntlet-parry");
            yield return Wait27(g, 0.5, wallEnd, idle);
            float guard = Viewmodel.Fist36ParryMax;
            yield return Frames(Ms(300));
            yield return Stop();
            bool ok = row && lunge > 1.2f && guard > 0.9f;
            res[0] = $"{(ok ? "GREEN" : "RED")} || lunge {lunge.ToString("0.00", ci)} x the web's jab (want > 1.2), guard weight {guard.ToString("0.00", ci)} (want > 0.9), punches seen {Viewmodel.Fist36Punches}, parry poses {Viewmodel.Fist36Parries} | shots shot-N30-fist36-quick-out / -quick-late / -gauntlet-rest / -gauntlet-lunge / -gauntlet-follow / -gauntlet-parry.png | {pc}";
        }

        //   crunch36  any map with a safe spot (the KESTREL added if not owned). Husks killed and frozen, god mode; one bag husk (1e9 hp) 5 m ahead.
        //     SHOT: one KESTREL tap at the bag. Measured: the impactFreeze events (want one bodyHit of 15-20 ms), the stagger drawn (EnemyLook.StaggerMax36,
        //       want > 6 deg), the hammer (Viewmodel.Hammer36Deg on the shot's frame, want > 20 deg, back to 0 by 0.4 s).
        //       Shots: shot-N30-crunch36-shot (the frame after the shot: the freeze, the flash, the body tipping back), -shot-75ms (the stagger's peak).
        //     PUNCH: walk the bag to 1.6 m, one melee press. Measured: punchHit freeze (want 100 ms), the stagger. Shot: -punch (the punch's hit frame).
        //     GREEN (crunch36 on): all of the above. RED (-skygrindOff36 / the row off): no bodyHit freeze, the punch 60 ms, no stagger, no hammer part.
        static IEnumerator Crunch36Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | map {g.MapId} | n36.crunch36 {sim.n36.crunch36}, N36Fixes.On {N36Fixes.On}, FeelAB {FeelAB.On}";
            offs.Add(() => { g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = null;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "crunch36", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 90;
            yield return Equip27(g, "revolver");
            g.forcedCombat = null;
            Husk bag = PlaceAt(g, 5, 0, out double _);
            if (bag != null) bag.hp = bag.maxHp = 1e9;
            Action aim = () => { God(g); if (bag != null && bag.alive) Look(g, bag.AimPoint); };
            var freezes = new List<string>();
            double bodyMs = -1, punchMs = -1;
            offs.Add(g.events.On<ImpactFreezeEvent>(FeelKeys.ImpactFreeze, e =>
            {
                if (e == null) return;
                freezes.Add(e.cause + (e.skipped ? " skipped" : " " + (e.seconds * 1000).ToString("0", ci) + " ms"));
                if (!e.skipped && e.cause == "bodyHit") bodyMs = e.seconds * 1000;
                if (!e.skipped && e.cause == "punchHit") punchMs = e.seconds * 1000;
            }));
            yield return Wait27(g, 0.8, wallEnd, aim);

            // ---- SHOT ----
            EnemyLook.StaggerMax36 = 0;
            EnemyLook.TwistMax36 = 0;
            EnemyLook.SquashMax36 = 0;
            pad.Send(0f, 1f);
            yield return Frames(1);
            float hammerAtShot = Viewmodel.Hammer36Deg;
            yield return Shot30(dir, "crunch36-shot");
            pad.Send(0f, 0f);
            yield return Wait27(g, 0.06, wallEnd, aim);
            yield return Shot30(dir, "crunch36-shot-75ms");
            yield return Wait27(g, 0.4, wallEnd, aim);
            float stagShot = EnemyLook.StaggerMax36, hammerLater = Viewmodel.Hammer36Deg, twistShot = EnemyLook.TwistMax36, squashShot = EnemyLook.SquashMax36;
            string shotFreezes = string.Join(", ", freezes);
            freezes.Clear();
            bool shotOk = bodyMs >= 15 && bodyMs <= 20.5 && stagShot > 6f && twistShot > 5f && squashShot > 0.1f && hammerAtShot > 20f && hammerLater < 1f;

            // ---- PUNCH ----
            if (bag != null && bag.alive)
            {
                Vec3 eye = g.Eye, to = bag.position - eye;
                double len = Math.Sqrt(to.x * to.x + to.z * to.z);
                if (len > 1e-3) bag.position = new Vec3(eye.x + to.x / len * 1.6, bag.position.y, eye.z + to.z / len * 1.6);
            }
            yield return Wait27(g, 0.3, wallEnd, aim);
            EnemyLook.StaggerMax36 = 0;
            g.forcedCombat = Punch;
            yield return Frames(1);
            g.forcedCombat = null;
            double tP = sim.SimTime;
            while (punchMs < 0 && sim.SimTime - tP < 0.4 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
            yield return Shot30(dir, "crunch36-punch");
            yield return Wait27(g, 0.3, wallEnd, aim);
            float stagPunch = EnemyLook.StaggerMax36;
            bool punchOk = Math.Abs(punchMs - 100) < 1 && stagPunch > 10f;
            g.husks.KillAll();
            yield return Frames(Ms(300));
            yield return Stop();
            bool ok = shotOk && punchOk && sim.n36.crunch36;
            res[0] = $"{(ok ? "GREEN" : "RED")} || SHOT {(shotOk ? "PASS" : "RED")}: bodyHit freeze {bodyMs.ToString("0.0", ci)} ms (want 15-20), stagger peak {stagShot.ToString("0.0", ci)} deg (want > 6), twist {twistShot.ToString("0.0", ci)} deg (want 6-10), squash {squashShot.ToString("0.00", ci)} (want ~0.15), hammer {hammerAtShot.ToString("0", ci)} deg on the shot frame -> {hammerLater.ToString("0", ci)} at 0.45 s; freezes [{shotFreezes}]"
                + $" || PUNCH {(punchOk ? "PASS" : "RED")}: punchHit freeze {punchMs.ToString("0.0", ci)} ms (want 100), stagger {stagPunch.ToString("0.0", ci)} deg (want > 10); freezes [{string.Join(", ", freezes)}] | shots shot-N30-crunch36-shot / -shot-75ms / -punch.png | {pc}";
        }

        static IEnumerator Tap36Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            List<string> order = Curriculum25.Order(g.cfg, sim.n25);
            int li = order.IndexOf(g.MapId);
            string gun = li == 2 ? "shotgun" : "revolver";
            string name = gun == "revolver" ? "KESTREL" : "THUNDERCLAP";
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | map {g.MapId} (order index {li}; level 1 = {(order.Count > 0 ? order[0] : "?")}, level 3 = {(order.Count > 2 ? order[2] : "?")}) | n36.tap36 {sim.n36.tap36}, n35.charge35 {sim.n35.charge35}, n31.charge {sim.n31.charge}, muzzle36 {N36Fixes.ViewRowOn(g.cfg, "muzzle36")}";
            offs.Add(() => { g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = null;
            bool hadGun = g.loadout.owned.IndexOf(gun) >= 0;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "tap36", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 90;
            yield return Equip27(g, gun);
            g.forcedCombat = null;
            Husk bag = PlaceAt(g, 6, 0, out double _);
            if (bag != null) bag.hp = bag.maxHp = 1e9;
            Action aim = () => { God(g); if (bag != null && bag.alive) Look(g, bag.AimPoint); };
            int fires = 0, starts = 0, charged = 0, lastFireFrame = -1;
            offs.Add(g.events.On<string>("fire", w => { if (w != gun) return; fires++; lastFireFrame = Time.frameCount; }));
            offs.Add(g.events.On<ChargeShot27Event>("charge35Start", e => starts++));
            offs.Add(g.events.On<ChargeShot27Event>("chargeShot27Fire", e => charged++));
            ChargeShots27 c = g.loadout.charge27;
            int gi = ChargeShots27.Index(gun);
            yield return Wait27(g, 0.8, wallEnd, aim);

            // ---- TAP ----
            int f0 = fires, st0 = starts, ch0 = charged;
            lastFireFrame = -1;
            int pressFrame = Time.frameCount;
            int tapPressStep = sim.simStep; // NIGHT39 P1: the press's length in sim steps (p39's tap / hold split is in steps)
            int tr0 = WeaponFx.Tracers36;
            pad.Send(0f, 1f);
            yield return Frames(1);
            float tracerToStar = WeaponFx.Tracers36 > tr0 ? Vector3.Distance(WeaponFx.Tracer36From, WeaponFx.Star36At) : -1f;
            yield return Shot30(dir, "tap36-" + gun + "-press");
            yield return Shot30(dir, "tap36-" + gun + "-press2");
            yield return Wait27(g, 0.06, wallEnd, aim);
            // NIGHT39 P1 (design.n39.p39, VERBS.md row 25 "changed by p39"): with p39 live the KESTREL's tap fires on the RELEASE (aimed from the press), so
            // the shot is measured from the release frame (want 0-2: the pad is read on the next frame); tap36's press rule otherwise
            bool p39 = gun == "revolver" && sim.charge35 != null && sim.charge35.p39;
            int heldSteps39 = sim.simStep - tapPressStep;
            int releaseFrame = Time.frameCount;
            pad.Send(0f, 0f);
            int tapFireFrame = lastFireFrame;
            yield return Wait27(g, 0.5, wallEnd, aim);
            if (p39) tapFireFrame = lastFireFrame;
            int tapShots = fires - f0, tapStarts = starts - st0, tapCharged = charged - ch0;
            int tapLag = tapFireFrame >= 0 ? tapFireFrame - pressFrame : -1;
            int relLag = tapFireFrame >= 0 ? tapFireFrame - releaseFrame : -1;
            bool tapOk = tapShots == 1 && tapStarts == 0 && tapCharged == 0 && (p39 ? relLag >= 0 && relLag <= 2 : tapLag >= 0 && tapLag <= 1);
            string tapLine = p39
                ? $"TAP {(tapOk ? "PASS" : "RED")} (p39): the shot {relLag} frame(s) after the release (want 0-2; {tapLag} after the press; the press held ~{heldSteps39} sim steps, tapS {sim.charge35.P39TapSteps}), {tapShots} shot(s), charge35Start {tapStarts} (want 0: the press shorter than tapS), charged {tapCharged}"
                : $"TAP {(tapOk ? "PASS" : "RED")}: the shot {tapLag} frame(s) after the press (want 0-1), {tapShots} shot(s), charge35Start {tapStarts}, charged {tapCharged}";

            // ---- HOLD ----
            if (c != null && gi >= 0) yield return Wait27(g, c.cooldown[gi] * sim.step + 0.1, wallEnd, aim); // a cooling charge cannot start
            yield return Wait27(g, 0.6, wallEnd, aim); // the gun's own interval
            f0 = fires;
            st0 = starts;
            ch0 = charged;
            lastFireFrame = -1;
            pressFrame = Time.frameCount;
            pad.Send(0f, 1f);
            yield return Frames(2);
            int holdFireFrame = lastFireFrame;
            int afterPress = fires;
            yield return Wait27(g, 0.1, wallEnd, aim);
            string earlyRail = Ui.Charge35RailText;
            yield return Shot30(dir, "tap36-" + gun + "-hold-early");
            yield return Wait27(g, 0.25, wallEnd, aim);
            string midRail = Ui.Charge35RailText;
            float midFill = Ui.Charge35RailFill;
            yield return Shot30(dir, "tap36-" + gun + "-hold-charging");
            yield return Wait27(g, 0.8 - 0.35 - 2 / 60.0, wallEnd, aim);
            int heldShots = fires - afterPress;
            pad.Send(0f, 0f);
            double tRel = sim.SimTime;
            while (charged == ch0 && sim.SimTime - tRel < 0.3 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
            int fRel = FeelProbe.FrameCount;
            yield return Shot30(dir, "tap36-" + gun + "-release");
            yield return Wait27(g, 0.4, wallEnd, aim);
            int holdLag = holdFireFrame >= 0 ? holdFireFrame - pressFrame : -1;

            // ---- muzzle36: the STITCHER's nails from the barrel ----
            bool m36 = N36Fixes.ViewRowOn(g.cfg, "muzzle36");
            yield return Equip27(g, "nailgun");
            g.forcedCombat = null;
            yield return Wait27(g, 0.5, wallEnd, aim);
            int blendFrames = 0;
            float blendMax = 0f;
            pad.Send(0f, 1f);
            for (int k = 0; k < 6; k++)
            {
                yield return Frames(1);
                if (ProjectileViews.Blending36 > 0) blendFrames++;
                if (ProjectileViews.BlendMax36 > blendMax) blendMax = ProjectileViews.BlendMax36;
                if (k == 1) yield return Shot30(dir, "tap36-stitcher-volley-early"); // n36-r2: the first nails still at the barrel
            }
            yield return Shot30(dir, "tap36-stitcher-volley");
            yield return Wait27(g, 0.3, wallEnd, aim);
            pad.Send(0f, 0f);
            yield return Wait27(g, 0.3, wallEnd, aim);
            bool muzzleOk = m36 ? tracerToStar >= 0f && tracerToStar < 0.01f && blendFrames > 0 && blendMax > 0.05f : blendFrames == 0;
            string muzzleLine = $"MUZZLE {(muzzleOk ? "PASS" : "RED")} (muzzle36 {m36}): the tap's tracer start to the muzzle star {(tracerToStar < 0 ? "no tracer flushed" : (tracerToStar * 100).ToString("0.00", ci) + " cm")} (want < 1 cm); nails drawn off the eye line in {blendFrames} of the volley's first 6 frames, the largest offset {(blendMax * 100).ToString("0", ci)} cm (want > 5) | shot shot-N30-tap36-stitcher-volley.png";
            // NIGHT39 P1 (p39, VERBS.md row 27 "changed by p39"): with p39 live the KESTREL's hold fires NO press shot (0 normal before the charge)
            bool holdOk = (p39 ? holdLag < 0 && afterPress - f0 == 0 : holdLag >= 0 && holdLag <= 1 && afterPress - f0 == 1) && heldShots == 0 && charged - ch0 == 1 && starts - st0 == 1;
            bool wordOk = string.IsNullOrEmpty(earlyRail) && midRail == UnityTuning.Charge35RailCharging;
            string holdLine = p39
                ? $"HOLD {(holdOk ? "PASS" : "RED")} (p39): shots in the press's first 2 frames {afterPress - f0} (want 0: no tap first), shots while held {heldShots} (want 0), charged on release {charged - ch0} (want 1), charge35Start {starts - st0}"
                : $"HOLD {(holdOk ? "PASS" : "RED")}: the press shot {holdLag} frame(s) after the press (want 0-1), shots while held after it {heldShots} (want 0), charged on release {charged - ch0} (want 1), charge35Start {starts - st0}";
            string wordLine = $"WORD {(wordOk ? "PASS" : "RED")}: rail at 0.1 s '{earlyRail}' (want empty), at 0.35 s '{midRail}' fill {midFill.ToString("0.00", ci)} (want CHARGING) | release frame f{fRel:0000}; shots shot-N30-tap36-{gun}-press / -press2 / -hold-early / -hold-charging / -release.png";
            g.husks.KillAll();
            yield return Frames(Ms(300));
            yield return Stop();
            bool ok = tapOk && holdOk && wordOk && (sim.n36.tap36 || p39);
            res[0] = $"{(ok ? "GREEN" : "RED")} {name}{(hadGun ? "" : " (added by the reel)")} || {tapLine} || {holdLine} || {wordLine} || {muzzleLine} | {pc}";
        }
    }
}
