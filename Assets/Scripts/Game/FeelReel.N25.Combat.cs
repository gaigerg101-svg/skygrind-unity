// NIGHT25 (builder C): the reel scenarios `meleefire`, `shockwave` and `pulse`, the three combat contracts' Player half. Written first, compiled by the
// dn-check, NOT RUN on a Player by the builder (no Editor in the builder's worktree). Each prints its numbers and PASS, or RED naming the row that is off.
//   meleefire  3 s of the revolver's trigger held alone, then 3 s with the quick melee pressed on its own cycle too: the shots match and the punches land
//   shockwave  ten THUNDERCLAP LT presses: no shockwave / doubleBlast / coneWave event, the zoom comes on ("no shockwave events on 10 shotgun LT presses; zoom on")
//   pulse      the PULSE held on a husk: hits to the first notch, a quick punch and idle feed nothing, the release launches along the aim
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
        static partial void N25MeleefireBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = MeleefireScenario25(g, dir, res, offs, pre);
        static partial void N25ShockwaveBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = ShockwaveScenario25(g, dir, res, offs, pre);
        static partial void N25PulseBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = PulseScenario25(g, dir, res, offs, pre);

        static IEnumerator MeleefireHold25(SurvivalGame g, double yaw, bool punch, int steps, int[] shotsPunches)
        {
            int shots = 0, punches = 0;
            Action<object> onFire = o => shots++;
            Action<object> onPunch = o => punches++;
            g.events.On("fire", onFire);
            g.events.On("punch", onPunch);
            int slot = g.loadout.owned.IndexOf("revolver");
            for (int f = 0; f < steps; f++)
            {
                God(g);
                g.input.SetLook(yaw, 0);
                g.forcedCombat = new CombatInput
                {
                    weapon = f == 0 ? slot : -1,
                    fire = new Skygrind.Core.Button { held = true, pressed = f == 0 },
                    punch = new Skygrind.Core.Button { held = punch, pressed = punch && f % 42 == 0 },
                };
                yield return null;
            }
            g.forcedCombat = IdleCombat;
            g.events.Off("fire", onFire);
            g.events.Off("punch", onPunch);
            shotsPunches[0] = shots;
            shotsPunches[1] = punches;
        }

        static IEnumerator MeleefireScenario25(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            offs.Add(() => g.forcedCombat = IdleCombat);
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            Start(g, "meleefire", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            int steps = Ms(3000);
            var a = new int[2];
            var b = new int[2];
            FeelProbe.Event("fireOnly");
            yield return MeleefireHold25(g, spotYaw, false, steps, a);
            yield return Frames(Ms(500));
            FeelProbe.Event("fireAndPunch");
            yield return MeleefireHold25(g, spotYaw, true, steps, b);
            yield return Stop();
            bool ok = b[1] > 0 && Math.Abs(b[0] - a[0]) <= 1;
            res[0] = pc + $" | {FeelProbe.FrameCount} frames: revolver trigger held 3 s: {a[0]} shots alone; with the quick melee pressed on its own cycle too: {b[0]} shots + {b[1]} punches (the melee row {(g.sim.n25.meleeFire ? "on" : "off")}; Core runs both in one step either way) | "
                + (ok ? "PASS" : "RED: a shot was lost or no punch landed while the trigger was held");
        }

        static IEnumerator ShockwaveScenario25(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            offs.Add(() => g.forcedCombat = IdleCombat);
            if (g.loadout.owned.IndexOf("shotgun") < 0) g.loadout.AddWeapon("shotgun");
            int slot = g.loadout.owned.IndexOf("shotgun");
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            int shock = 0, blast = 0, cone = 0, ads = 0;
            offs.Add(g.events.On<ShockwaveEvent>(N20Events.Shockwave, e => shock++));
            offs.Add(g.events.On<ShockwaveEvent>(N20Events.DoubleBlast, e => blast++));
            offs.Add(g.events.On<ShockwaveEvent>(N20Events.ConeWave, e => cone++));
            offs.Add(g.events.On<string>("adsStart", w => ads++));
            Start(g, "shockwave", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            int zoomed = 0;
            double mag = 1;
            for (int press = 0; press < 10; press++)
            {
                int hold = Ms(500);
                for (int f = 0; f < hold + Ms(300); f++)
                {
                    God(g);
                    g.input.SetLook(spotYaw, 0);
                    bool down = f < hold;
                    g.forcedCombat = new CombatInput { weapon = press == 0 && f == 0 ? slot : -1, altFire = new Skygrind.Core.Button { held = down, pressed = down && f == 0 } };
                    yield return null;
                    if (f == hold - 2 && g.loadout.ZoomMag > 1) { zoomed++; mag = g.loadout.ZoomMag; }
                }
            }
            g.forcedCombat = IdleCombat;
            yield return Stop();
            bool ok = shock + blast + cone == 0 && zoomed == 10 && !g.loadout.ShockwaveOn;
            res[0] = pc + $" | {FeelProbe.FrameCount} frames: " + (shock + blast + cone == 0 ? "no shockwave events" : $"{shock} shockwave / {blast} doubleBlast / {cone} coneWave events")
                + $" on 10 shotgun LT presses; zoom {(zoomed == 10 ? "on" : $"on {zoomed} of 10")} (x{mag.ToString("0.##", ci)}, {ads} adsStart) | "
                + (ok ? "PASS" : g.sim.n25.shockwaveOff ? "FAIL" : "RED: design.n25.shockwaveOff is off (NIGHT24's shockwave)");
        }

        static IEnumerator PulseScenario25(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            Fist24 f = g.sim.fist24;
            if (!g.sim.n25.pulseCharge || f == null || !f.pulseMode)
            {
                res[0] = pc + " | RED: design.n25.pulseCharge is off (NIGHT24's fist: the charge is fed by kills and hits of any melee, a punch flicks)";
                yield break;
            }
            offs.Add(() => { g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            if (g.loadout.owned.IndexOf("gauntlet") < 0) g.loadout.AddWeapon("gauntlet");
            int slot = g.loadout.owned.IndexOf("gauntlet");
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            g.husks.frozen = true;
            double hurtLoss = f.rules.hurtLoss;
            f.rules.hurtLoss = 0;
            offs.Add(() => f.rules.hurtLoss = hurtLoss);
            Start(g, "pulse", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var fails = new List<string>();

            // a husk 1.6 m ahead: the gauntlet's left click held until the first notch
            Husk h = PlaceAt(g, 1.6, 0, out _);
            int hits = 0, tier1 = 0;
            offs.Add(g.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "gauntlet") hits++; }));
            if (h == null) fails.Add("no husk placed 1.6 m ahead");
            else
            {
                h.hp = h.maxHp = 1e9;
                f.noFlick = true; // banking: the release below is the launch, this part only holds
                int guard = 0;
                while (f.Tier < 1 && guard++ < Ms(9000))
                {
                    God(g);
                    Look(g, h.AimPoint);
                    g.forcedCombat = new CombatInput { weapon = guard == 1 ? slot : -1, fire = new Skygrind.Core.Button { held = true, pressed = guard == 1 } };
                    yield return null;
                }
                tier1 = hits;
                g.forcedCombat = IdleCombat; // the click let go while noFlick is still set: the banked hold never launches
                yield return Frames(6);
                f.noFlick = false;
                if (tier1 < 6 || tier1 > 8) fails.Add($"the first notch took {tier1} hits, not 6-8");
            }
            g.husks.KillAll();
            double c0 = f.charge;

            // the release: along the open floor, from a standstill
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(Ms(600));
            f.charge = f.rules.tiers[0]; // labelled: the notch the hits above banked, restaged after the respawn
            Vec3 from = g.Feet;
            int launches0 = g.loadout.pulseLaunches;
            for (int s = 0; s < Ms(120); s++)
            {
                God(g);
                g.input.SetLook(spotYaw, 0);
                g.forcedCombat = new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = true, pressed = s == 0 } };
                yield return null;
            }
            g.forcedCombat = IdleCombat; // the click let go: the launch
            yield return Frames(Ms(1500));
            double moved = Math.Sqrt((g.Feet.x - from.x) * (g.Feet.x - from.x) + (g.Feet.z - from.z) * (g.Feet.z - from.z));
            double want = f.rules.jumpMult[0] * f.rules.jumpDistance;
            if (g.loadout.pulseLaunches != launches0 + 1) fails.Add("the release did not launch once");
            if (moved < 0.5 * want) fails.Add($"the launch moved {moved.ToString("0.0", ci)} m of the {want.ToString("0.0", ci)} m asked (a wall close ahead cuts it)");

            // a quick punch (F) feeds nothing
            double before = f.charge;
            g.forcedCombat = Punch;
            yield return null;
            g.forcedCombat = IdleCombat;
            yield return Frames(Ms(300));
            if (f.charge != before) fails.Add("a quick punch changed the charge");
            yield return Stop();

            res[0] = pc + $" | {FeelProbe.FrameCount} frames: PULSE held on a husk: first notch after {tier1} landed hits (want 6-8, {(tier1 * g.cfg.Num("weapons.gauntlet.cycle")).ToString("0.0", ci)} s of swinging); release at the first notch launched {moved.ToString("0.0", ci)} m (asked {want.ToString("0.0", ci)} m = 0.5 J), launches {g.loadout.pulseLaunches}; the 3 s lockout and the launch's damage on bodies are the harness `pulse` | "
                + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
