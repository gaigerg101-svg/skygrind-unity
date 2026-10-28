// NIGHT26 (builder W): the Player scenarios of the weapon rows (the lead's FeelReel.N26.cs calls the two hooks).
//   pulselt   contract pulse-lt: the PULSE held on a husk: LT held charges the first notch by its landed hits, LT released launches once; the left click held and
//             released launches nothing. With design.n26.pulseLT off (NIGHT25's click version) LT builds nothing: the line says RED with its numbers.
//   weapons   contract weapon-lt: LT held on each gun in the list: which ones zoom. ON: the KESTREL, the THUNDERCLAP and the LONGWAVE zoom, the STITCHER's, the
//             COMET's, the BOUNCER's and the ZAPPER's LT are their own alts (no zoom); off, the STITCHER zooms (NIGHT20) and the line says RED.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N26PulseltBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = PulseltScenario26(g, dir, res, offs, pre);

        static partial void N26WeaponsBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = WeaponsScenario26(g, dir, res, offs, pre);

        static IEnumerator PulseltScenario26(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            Fist24 f = g.sim.fist24;
            if (f == null || !f.pulseMode)
            {
                res[0] = pc + " | RED: the PULSE charge is not wired (design.n25.pulseCharge and design.n26.pulseLT both off: NIGHT24's fist)";
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
            Start(g, "pulselt", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var fails = new List<string>();

            // 1: LT held on a husk 1.6 m ahead until the first notch (9 s at most)
            Husk h = PlaceAt(g, 1.6, 0, out _);
            int hits = 0, reachedTier = 0;
            double reachedCharge = 0, loopSeconds = 0;
            string weaponInHand = "";
            offs.Add(g.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "gauntlet") hits++; }));
            if (h == null) fails.Add("no husk placed 1.6 m ahead");
            else
            {
                h.hp = h.maxHp = 1e9;
                f.noFlick = true; // banking: the release below is the launch
                // A player's input: the PULSE selected, LT held and nothing else (the gauntlet swings by itself while LT is held; each landed hit fills the charge).
                // Bound by GAME time (the sim's steps, hitstop freezes them), not by frames: a hit's freeze would cut a frame-count loop short.
                int guard = 0;
                int step0 = g.sim.simStep;
                while (f.Tier < 1 && (g.sim.simStep - step0) * g.sim.step < 12 && guard++ < Ms(60000))
                {
                    God(g);
                    Look(g, h.AimPoint);
                    g.forcedCombat = new CombatInput { weapon = guard == 1 ? slot : -1, altFire = new Skygrind.Core.Button { held = true, pressed = guard == 1 } };
                    yield return null;
                }
                reachedTier = f.Tier;
                reachedCharge = f.charge;
                loopSeconds = (g.sim.simStep - step0) * g.sim.step;
                weaponInHand = g.loadout.Weapon;
                g.forcedCombat = IdleCombat; // LT let go while noFlick is still set: the banked hold never launches (NIGHT25's fix: the order matters)
                yield return Frames(6);
                f.noFlick = false;
            }
            int ltHits = hits;
            int notch = Math.Max(reachedTier, f.Tier);
            g.husks.KillAll();
            if (notch < 1) fails.Add($"LT held for {loopSeconds.ToString("0.0", ci)} s of game time landed {ltHits} hits and built no notch (charge {reachedCharge.ToString("0.00", ci)}, {weaponInHand} in hand)");

            // 2: the release: stage the notch, hold LT in the open, let go
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(Ms(600));
            f.charge = f.rules.tiers[0];
            Vec3 from = g.Feet;
            int launches0 = g.loadout.pulseLaunches;
            for (int s = 0; s < Ms(120); s++)
            {
                God(g);
                g.input.SetLook(spotYaw, 0);
                g.forcedCombat = new CombatInput { weapon = -1, altFire = new Skygrind.Core.Button { held = true, pressed = s == 0 } };
                yield return null;
            }
            // the state the release will read (printed: a release that does nothing says why)
            string beforeRelease = $"launches so far {g.loadout.pulseLaunches}, weapon {g.loadout.Weapon}, charge {f.charge.ToString("0.00", ci)} notch {f.Tier}, canFlick {f.CanFlick}, lockout {f.LockSteps} steps, grounded {g.sim.mover.grounded}, pulseLT {g.loadout.pulseLT26}";
            int blocked0 = f.blocked, flicks0 = f.flicks;
            g.forcedCombat = IdleCombat;
            yield return Frames(Ms(1500));
            double moved = Math.Sqrt((g.Feet.x - from.x) * (g.Feet.x - from.x) + (g.Feet.z - from.z) * (g.Feet.z - from.z));
            double want = f.rules.jumpMult[0] * f.rules.jumpDistance;
            int ltLaunches = g.loadout.pulseLaunches - launches0;
            string releaseInfo = $"[at the release: {beforeRelease}; after: {f.flicks - flicks0} flicks, {f.blocked - blocked0} blocked (room {f.lastAllowed.ToString("0.0", ci)} m of {f.lastWanted.ToString("0.0", ci)})]";
            if (ltLaunches != 1) fails.Add($"LT released made {ltLaunches} launches, not 1");
            if (ltLaunches == 1 && moved < 0.5 * want) fails.Add($"the launch moved {moved.ToString("0.0", ci)} m of the {want.ToString("0.0", ci)} m asked");

            // 3: the left click held and let go with a notch launches nothing
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(Ms(600));
            for (int w = 0; f.LockSteps > 0 && w < Ms(6000); w++) yield return null; // the 3 s lockout of the launch above must be over, or "no launch" would be the lockout's
            f.charge = f.rules.tiers[0];
            int launches1 = g.loadout.pulseLaunches;
            for (int s = 0; s < Ms(120); s++)
            {
                God(g);
                g.input.SetLook(spotYaw, 0);
                g.forcedCombat = new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = true, pressed = s == 0 } };
                yield return null;
            }
            g.forcedCombat = IdleCombat;
            yield return Frames(Ms(1000));
            int clickLaunches = g.loadout.pulseLaunches - launches1;
            if (clickLaunches != 0) fails.Add($"the left click released made {clickLaunches} launches (it must only swing)");
            yield return Stop();

            res[0] = pc + $" | {FeelProbe.FrameCount} frames: PULSE held on a husk, LT held ({weaponInHand} in hand, {loopSeconds.ToString("0.0", ci)} s of game time): {ltHits} landed hits to notch {notch} (charge {reachedCharge.ToString("0.00", ci)}); LT released launched {ltLaunches}x over {moved.ToString("0.0", ci)} m (asked {want.ToString("0.0", ci)} m = 0.5 J); the left click held and released launched {clickLaunches}x {releaseInfo} | "
                + (fails.Count == 0 ? "PASS" : (g.sim.n26.pulseLT ? "FAIL: " : "RED (design.n26.pulseLT off: NIGHT25's click version): ") + string.Join("; ", fails));
        }

        static IEnumerator WeaponsScenario26(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            offs.Add(() => g.forcedCombat = IdleCombat);
            string[] guns = { "revolver", "shotgun", "rail", "nailgun", "rocket", "bouncer", "zapper" };
            foreach (string w in guns) if (g.loadout.owned.IndexOf(w) < 0) g.loadout.AddWeapon(w);
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            Start(g, "weapons", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var zoomers = new List<string>();
            var own = new List<string>();
            foreach (string w in guns)
            {
                int slot = g.loadout.owned.IndexOf(w);
                for (int f = 0; f < Ms(700); f++)
                {
                    God(g);
                    g.input.SetLook(spotYaw, 0);
                    g.forcedCombat = new CombatInput { weapon = f == 0 ? slot : -1, altFire = new Skygrind.Core.Button { held = f > Ms(350), pressed = f == Ms(350) + 1 } };
                    yield return null;
                }
                (g.loadout.ZoomMag > 1 ? zoomers : own).Add(w);
                g.forcedCombat = IdleCombat;
                yield return Frames(Ms(300));
            }
            yield return Stop();
            bool ok = zoomers.Count == 3 && zoomers.Contains("revolver") && zoomers.Contains("shotgun") && zoomers.Contains("rail");
            res[0] = pc + $" | {FeelProbe.FrameCount} frames: LT held zooms {string.Join(" ", zoomers)}; its own alt on {string.Join(" ", own)} | "
                + (ok ? "PASS" : g.sim.n26.weaponIdentity ? "FAIL" : "RED (design.n26.weaponIdentity off: NIGHT25's LTs, the STITCHER zooms)");
        }
    }
}
