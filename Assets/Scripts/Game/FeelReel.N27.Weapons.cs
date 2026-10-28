// NIGHT27 (builder W): the Player scenarios of the two weapon rows (the lead's FeelReel.N27.cs calls the two hooks; the bodies live here).
//   pulse27   contract pulse27: the PULSE gauntlet in hand, driven by a VIRTUAL PAD (InputSystem.AddDevice<Gamepad> + QueueStateEvent on the right and left
//             triggers: the path a real pad takes through PadReader, not g.forcedCombat). LT with 0 pips is DENIED (why "pips", the HUD says NEED 5 KILLS); five
//             kills by the 2.5 s swing fill five pips (printed after each kill, sim and HUD); then LT: PULSE READY shows, the blast goes once, the glove empties,
//             the speed peaks, the cooldown is 15 s or longer; a second LT is denied again. Row off: the RED line says so.
//   charge27  contract chargeShots27: the same virtual pad, RT. The pistol (revolver, the KESTREL) and the shotgun (the THUNDERCLAP): a tap fires at once; a hold
//             past 0.4 s + the full time (0.6 s / 0.8 s) then release fires ONE charged shot (damage vs a normal shot on a standing bag husk); the cooldown
//             (10 s / 14 s) then runs and a hold during it fires no charge while a tap still fires normally. The wait out of the cooldown is NOT walked here
//             (the harness `weapons --charge27` walks it); the line says so.
// Every loop is bounded by GAME time (the sim's steps: a hitstop freezes them) and by a wall-clock guard. Dictionaries: none read.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N27Pulse27Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Pulse27Scenario(g, dir, res, offs, pre);

        static partial void N27Charge27Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Charge27Scenario(g, dir, res, offs, pre);

        /// <summary>The virtual pad of these two scenarios: LT and RT as analog values (PadReader's TriggerDown / TriggerUp read them as a player's would).</summary>
        sealed class Pad27
        {
            public Gamepad pad;
            public GamepadState st;

            public void Send(float lt, float rt)
            {
                st.leftTrigger = lt;
                st.rightTrigger = rt;
                InputSystem.QueueStateEvent(pad, st);
            }
        }

        static Pad27 NewPad27(List<Action> offs)
        {
            var p = new Pad27 { pad = InputSystem.AddDevice<Gamepad>("ReelPadN27") };
            offs.Add(() =>
            {
                try
                {
                    InputSystem.QueueStateEvent(p.pad, new GamepadState());
                    InputSystem.RemoveDevice(p.pad);
                }
                catch (Exception) { /* the Player is closing: nothing to undo */ }
            });
            return p;
        }

        /// <summary>Frames until `seconds` of GAME time passed (the sim's steps) or the wall clock is out; `each` runs every frame.</summary>
        static IEnumerator Wait27(SurvivalGame g, double seconds, double wallEnd, Action each)
        {
            int s0 = g.sim.simStep;
            while ((g.sim.simStep - s0) * g.sim.step < seconds && Time.realtimeSinceStartup < wallEnd)
            {
                each?.Invoke();
                yield return null;
            }
        }

        /// <summary>Put `gun` in hand through the combat input's weapon slot for one frame (a swap, not a trigger), then hand the input back to the pad.</summary>
        static IEnumerator Equip27(SurvivalGame g, string gun)
        {
            if (g.loadout.owned.IndexOf(gun) < 0) g.loadout.AddWeapon(gun);
            int slot = g.loadout.owned.IndexOf(gun);
            // A swap takes the loadout's swapTime (lower, then raise): ask for the slot every frame until it is the one in hand (asking for the slot
            // already targeted is a no-op), then let the raise finish. A just-added weapon's pendingSlot is overridden by the explicit slot.
            double wallEnd = Time.realtimeSinceStartup + 20;
            int s0 = g.sim.simStep;
            while ((g.loadout.Weapon != gun || (g.sim.simStep - s0) * g.sim.step < 0.2) && (g.sim.simStep - s0) * g.sim.step < 3.0 && Time.realtimeSinceStartup < wallEnd)
            {
                g.forcedCombat = new CombatInput { weapon = slot };
                yield return null;
            }
            g.forcedCombat = null;
            yield return Wait27(g, 1.0, wallEnd, null); // the raise finishes before the first shot
        }

        static IEnumerator Pulse27Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            PulseCharge27 p = g.sim.pulse27;
            if (!g.sim.n27.pulse27 || p == null || !p.active)
            {
                res[0] = "RED: design.n27.pulse27 is off (NIGHT26's pulseLT: no pips, no gate) | " + pc;
                yield break;
            }
            offs.Add(() => { g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = null;
            if (g.loadout.owned.IndexOf("gauntlet") < 0) g.loadout.AddWeapon("gauntlet");
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.frozen = true;
            yield return Equip27(g, "gauntlet");
            double wallEnd = Time.realtimeSinceStartup + 150;
            var fails = new List<string>();
            if (g.loadout.Weapon != "gauntlet") fails.Add($"{g.loadout.Weapon} in hand, not the gauntlet");
            Start(g, "pulse27", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            int kills = 0;
            offs.Add(g.events.On<KillEvent>("kill", e => { if (e.owner == "player") kills++; }));
            Pulse27Event blastEv = null;
            offs.Add(g.events.On<Pulse27Event>("pulse27Blast", e => blastEv = new Pulse27Event { hits = e.hits, boostSteps = e.boostSteps, pips = e.pips }));
            int pipsAtStart = p.pips;
            if (pipsAtStart != 0) fails.Add($"the glove held {pipsAtStart} pips at the start (a fresh run has none)");

            // 1: LT with 0 pips is DENIED (nothing spent, no blast), and the HUD says why
            int denied0 = p.denied, blasts0 = p.blasts;
            pad.Send(1f, 0f);
            yield return Frames(6);
            pad.Send(0f, 0f);
            yield return Frames(6);
            int denied1 = p.denied - denied0;
            string deniedWhy0 = p.deniedWhy;
            string deniedText = Ui.Pulse27TextNow;
            int blastsDenied = p.blasts - blasts0;
            if (denied1 != 1 || deniedWhy0 != "pips") fails.Add($"LT with 0 pips: {denied1} refusals, why '{deniedWhy0}' (want 1, 'pips')");
            if (blastsDenied != 0) fails.Add("LT with 0 pips blasted");
            if (!Ui.Weapons27Built) fails.Add("the glove's HUD is not built");
            else if (deniedText == null || deniedText.IndexOf("NEED", StringComparison.Ordinal) < 0) fails.Add($"the HUD read '{deniedText}' after the refusal, not NEED n KILLS");
            yield return Frames(Ms(300));

            // 2: five kills by the swing (RT held: the gauntlet swings by itself every 2.5 s; a 1 hp husk 1.6 m ahead each time)
            var trail = new List<int>();
            var hudTrail = new List<int>();
            int step0 = g.sim.simStep;
            int lastKillStep = g.sim.simStep;
            int want = p.p.pips;
            Husk h = null;
            pad.Send(0f, 1f);
            while (kills < want && Time.realtimeSinceStartup < wallEnd)
            {
                if (h == null || !h.alive)
                {
                    h = PlaceAt(g, 1.6, 0, out _);
                    if (h == null) { fails.Add("no husk placed 1.6 m ahead"); break; }
                    h.hp = h.maxHp = 1;
                }
                if ((g.sim.simStep - lastKillStep) * g.sim.step > 6.0) { fails.Add($"no kill in 6 s of game time (kill {kills + 1}; weapon {g.loadout.Weapon})"); break; }
                God(g);
                Look(g, h.AimPoint);
                int before = kills;
                yield return null;
                if (kills > before)
                {
                    lastKillStep = g.sim.simStep;
                    yield return Frames(3); // the HUD draws the pip a frame later
                    trail.Add(p.pips);
                    hudTrail.Add(Ui.Pulse27PipsLitNow);
                    h = null;
                }
            }
            pad.Send(0f, 0f);
            double killSeconds = (g.sim.simStep - step0) * g.sim.step;
            string trailS = string.Join(",", trail), hudS = string.Join(",", hudTrail);
            if (trailS != "1,2,3,4,5") fails.Add($"the pips after each kill were {trailS}, not 1,2,3,4,5");
            if (hudTrail.Count == trail.Count && hudS != trailS) fails.Add($"the HUD's lit pips were {hudS} against the sim's {trailS}");
            g.husks.KillAll();
            yield return Frames(Ms(300));
            bool ready = p.Ready, hudReady = Ui.Pulse27ReadyNow;
            string readyText = Ui.Pulse27TextNow;
            if (!ready) fails.Add($"the glove is not READY after the kills (pips {p.pips}, cooldown {p.cooldownSteps} steps)");
            // NIGHT30 (builder V, design.n30.hud30): with the weapon-state widget the glove's words are gone; its BLAST slot says READY instead
            if (Hud30.On)
            {
                readyText = "BLAST " + Ui.Weapon30BlastState + " '" + Ui.Weapon30BlastWord + "'";
                if (!hudReady || Ui.Weapon30BlastState != "READY") fails.Add($"the widget's BLAST slot read {readyText} (ready flag {hudReady}), not READY");
            }
            else if (!hudReady || readyText == null || readyText.IndexOf("PULSE READY", StringComparison.Ordinal) < 0) fails.Add($"the HUD read '{readyText}' (ready flag {hudReady}), not PULSE READY");

            // 3: LT with the full glove: the blast once, the boost, the cooldown
            Husk bag = PlaceAt(g, 3.0, 0, out _);
            if (bag != null) bag.hp = bag.maxHp = 1e9;
            else fails.Add("no bag husk placed 3 m ahead for the blast");
            Vec3 v0 = g.mover.velocity;
            double speed0 = v0.Length, peak = speed0;
            int blastsBefore = p.blasts;
            int cdAtBlast = -1;
            pad.Send(1f, 0f);
            yield return Wait27(g, 0.25, wallEnd, () =>
            {
                God(g);
                if (bag != null && bag.alive) Look(g, bag.AimPoint);
                peak = Math.Max(peak, g.mover.velocity.Length);
                if (cdAtBlast < 0 && p.blasts > blastsBefore) cdAtBlast = p.cooldownSteps;
            });
            pad.Send(0f, 0f);
            yield return Wait27(g, 1.0, wallEnd, () =>
            {
                God(g);
                peak = Math.Max(peak, g.mover.velocity.Length);
                if (cdAtBlast < 0 && p.blasts > blastsBefore) cdAtBlast = p.cooldownSteps;
            });
            int blasted = p.blasts - blastsBefore;
            double cdSeconds = cdAtBlast < 0 ? 0 : cdAtBlast * g.sim.step;
            int blastHits = blastEv != null ? blastEv.hits : -1;
            int pipsAfter = p.pips;
            if (blasted != 1) fails.Add($"LT with a full glove made {blasted} blasts, not 1");
            if (pipsAfter != 0) fails.Add($"the glove held {pipsAfter} pips after the blast, not 0");
            if (peak < speed0 + 1.0) fails.Add($"the blast's boost: peak speed {peak.ToString("0.0", ci)} m/s from {speed0.ToString("0.0", ci)} (no boost seen)");
            if (p.p.blastCooldownSeconds < 15.0 || cdSeconds < 14.0) fails.Add($"the blast's cooldown {cdSeconds.ToString("0.0", ci)} s at the blast (config {p.p.blastCooldownSeconds.ToString("0.0", ci)} s), want 15 s or longer");
            if (blastHits < 1) fails.Add($"the blast hit {blastHits} bodies with a bag 3 m ahead");

            // 4: LT again at once: refused (the glove is empty; the cooldown runs)
            int denied2 = p.denied;
            pad.Send(1f, 0f);
            yield return Frames(6);
            pad.Send(0f, 0f);
            yield return Frames(6);
            string deniedWhy1 = p.deniedWhy;
            int blastsAfterSecond = p.blasts - blastsBefore;
            if (p.denied != denied2 + 1) fails.Add($"the second LT was refused {p.denied - denied2} times, not 1");
            if (blastsAfterSecond != 1) fails.Add($"the second LT blasted ({blastsAfterSecond} blasts in all)");
            yield return Stop();

            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, virtual pad (LT / RT analog), gauntlet in hand: "
                + $"LT with {pipsAtStart} pips: {denied1} refusal (why {deniedWhy0}), HUD '{deniedText}', blasts {blastsDenied}; "
                + $"swing kills filled pips {trailS} (HUD {hudS}) in {killSeconds.ToString("0.0", ci)} s of game time; before LT: ready {ready}, HUD '{readyText}'; "
                + $"LT with the full glove: {blasted} blast hit {blastHits}, glove {pipsAfter} pips, peak speed {peak.ToString("0.0", ci)} m/s (was {speed0.ToString("0.0", ci)}), cooldown {cdSeconds.ToString("0.0", ci)} s at the blast (config {p.p.blastCooldownSeconds.ToString("0.0", ci)} s); "
                + $"a second LT: refused ({deniedWhy1}), blasts {blastsAfterSecond}";
        }

        static IEnumerator Charge27Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            ChargeShots27 c = g.sim.charge27;
            if (!g.sim.n27.chargeShots27 || c == null || !c.active)
            {
                res[0] = "RED: design.n27.chargeShots27 is off (the normal shot only: RT held never charges) | " + pc;
                yield break;
            }
            offs.Add(() => { g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = null;
            foreach (string w in new[] { "revolver", "shotgun" }) if (g.loadout.owned.IndexOf(w) < 0) g.loadout.AddWeapon(w);
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.frozen = true;
            Start(g, "charge27", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 170;
            var fails = new List<string>();
            var lines = new List<string>();
            string curGun = "";
            var fires = new List<int>();
            var hits = new List<(int step, double dmg)>();
            offs.Add(g.events.On<string>("fire", w => { if (w == curGun) fires.Add(g.sim.simStep); }));
            offs.Add(g.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == curGun) hits.Add((g.sim.simStep, e.damage)); }));

            foreach (string gun in new[] { "revolver", "shotgun" })
            {
                int gi = ChargeShots27.Index(gun);
                string name = gun == "revolver" ? "KESTREL" : "THUNDERCLAP";
                curGun = gun;
                fires.Clear();
                hits.Clear();
                g.husks.KillAll();
                yield return Equip27(g, gun);
                if (g.loadout.Weapon != gun) fails.Add($"{g.loadout.Weapon} in hand, not the {gun}");
                Husk bag = PlaceAt(g, gun == "revolver" ? 4.0 : 3.0, 0, out _);
                if (bag == null) { fails.Add($"{name}: no bag husk placed"); continue; }
                bag.hp = bag.maxHp = 1e12;
                Action each = () => { God(g); if (bag.alive) Look(g, bag.AimPoint); };
                double holdS = c.p.holdSeconds, fullS = c.c35 != null && c.c35.c40 ? c.FullSteps(gi) * g.cfg.Dt : c.p.fullSeconds[gi], cdS = c.p.cooldownSeconds[gi]; // NIGHT40 W1: charge40's FULL while live
                double fullHold = holdS + fullS + 0.15;
                int fired0 = c.fired[gi], cancelled0 = c.cancelled;

                // a: three taps 2 s apart: each fires at the press; the mean normal damage (a pistol's first body hit, a shotgun's whole volley)
                var taps = new List<(int press, int fire)>();
                double normalSum = 0;
                int normalN = 0;
                for (int t = 0; t < 3 && Time.realtimeSinceStartup < wallEnd; t++)
                {
                    int f0 = fires.Count, h0 = hits.Count;
                    int pressStep = g.sim.simStep;
                    pad.Send(0f, 1f);
                    yield return Wait27(g, 0.1, wallEnd, each);
                    pad.Send(0f, 0f);
                    yield return Wait27(g, 0.4, wallEnd, each);
                    if (fires.Count > f0) taps.Add((pressStep, fires[f0]));
                    double sum = 0;
                    for (int k = h0; k < hits.Count; k++) { if (gun == "revolver") { sum = hits[k].dmg; break; } sum += hits[k].dmg; }
                    if (sum > 0) { normalSum += sum; normalN++; }
                    yield return Wait27(g, 1.5, wallEnd, each);
                }
                double normalDmg = normalN > 0 ? normalSum / normalN : 0;
                double latencyMs = taps.Count > 0 ? taps[0].fire - taps[0].press > 0 ? (taps[0].fire - taps[0].press) * g.sim.step * 1000 : 0 : -1;
                if (taps.Count != 3) fails.Add($"{name}: {taps.Count} of 3 taps fired a shot");
                if (latencyMs > 100) fails.Add($"{name}: the tap fired {latencyMs.ToString("0", ci)} ms after the press");
                if (c.fired[gi] != fired0) fails.Add($"{name}: a tap made a charged shot");

                // b: a hold past holdSeconds + fullSeconds, then release: ONE charged shot
                int hStart = hits.Count;
                int fires0 = fires.Count;
                double arcMid = 0;
                pad.Send(0f, 1f);
                yield return Wait27(g, holdS + fullS + 0.1, wallEnd, () => { each(); arcMid = Math.Max(arcMid, Ui.Charge27ArcFillNow); });
                int firesInHold = fires.Count - fires0;
                int releaseStep = g.sim.simStep;
                pad.Send(0f, 0f);
                yield return Wait27(g, 0.6, wallEnd, each);
                int chargedFired = c.fired[gi] - fired0;
                double chargedDmg = 0;
                int chargedBodies = 0;
                foreach (var hh in hits) if (hh.step >= releaseStep - 1) { chargedDmg += hh.dmg; chargedBodies++; }
                int cdLeftSteps = c.cooldown[gi];
                double cdLeft = cdLeftSteps * g.sim.step;
                double want = c.p.damageMult[gi];
                double ratio = normalDmg > 0 ? (gun == "revolver" ? chargedDmg / Math.Max(1, chargedBodies) : chargedDmg) / normalDmg : 0;
                if (chargedFired != 1) fails.Add($"{name}: {chargedFired} charged shots after the hold and release, not 1");
                if (gun == "revolver" ? Math.Abs(ratio - want) > 0.3 : (ratio < want * 0.75 || ratio > want * 1.35))
                    fails.Add($"{name}: the charged shot did {ratio.ToString("0.00", ci)}x a normal {(gun == "revolver" ? "hit" : "volley")}, want about {want.ToString("0.#", ci)}x");
                if (cdLeft < cdS - 2.5 || cdLeft > cdS) fails.Add($"{name}: {cdLeft.ToString("0.0", ci)} s of cooldown left 0.6 s after the shot, config {cdS.ToString("0", ci)} s");
                if (arcMid < 0.9) fails.Add($"{name}: the charge arc filled to {arcMid.ToString("0.00", ci)} by the end of the hold");

                // c: inside the cooldown a hold charges nothing (the normal shot fires), and a tap still fires
                int fiCd0 = fires.Count, firedCd0 = c.fired[gi], cancelledCd0 = c.cancelled;
                pad.Send(0f, 1f);
                yield return Wait27(g, fullHold, wallEnd, each);
                pad.Send(0f, 0f);
                yield return Wait27(g, 0.3, wallEnd, each);
                int normalInCooldownHold = fires.Count - fiCd0;
                int chargedInCooldown = c.fired[gi] - firedCd0;
                int tapF0 = fires.Count;
                pad.Send(0f, 1f);
                yield return Wait27(g, 0.1, wallEnd, each);
                pad.Send(0f, 0f);
                yield return Wait27(g, 0.3, wallEnd, each);
                int tapInCooldown = fires.Count - tapF0;
                double cdLeftEnd = c.cooldown[gi] * g.sim.step;
                if (chargedInCooldown != 0) fails.Add($"{name}: a hold inside the cooldown fired {chargedInCooldown} charged shot(s)");
                if (normalInCooldownHold < 1) fails.Add($"{name}: the normal shot was blocked during the cooldown ({normalInCooldownHold} shots in a {fullHold.ToString("0.0", ci)} s hold)");
                if (tapInCooldown < 1) fails.Add($"{name}: a tap inside the cooldown fired nothing");
                lines.Add($"{name} ({gun}): tap {taps.Count}/3 fired, first {latencyMs.ToString("0", ci)} ms after the press (normal {normalDmg.ToString("0.0", ci)}); hold {holdS.ToString("0.0", ci)}+{fullS.ToString("0.0", ci)} s then release: {chargedFired} charged shot, {firesInHold} normal shot(s) before it, {chargedBodies} body hit(s) for {chargedDmg.ToString("0.0", ci)} = {ratio.ToString("0.00", ci)}x a normal {(gun == "revolver" ? "hit per body" : "volley")} (want {want.ToString("0.#", ci)}x), arc peak {arcMid.ToString("0.00", ci)}; cooldown {cdLeft.ToString("0.0", ci)} s left (config {cdS.ToString("0", ci)} s); a {fullHold.ToString("0.00", ci)} s hold inside it: {chargedInCooldown} charged, {normalInCooldownHold} normal shot(s); a tap inside it fired {tapInCooldown}; cooldown {cdLeftEnd.ToString("0.0", ci)} s left at the end (cancelled +{c.cancelled - cancelledCd0})");
                pad.Send(0f, 0f);
                yield return Frames(Ms(200));
            }
            yield return Stop();
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, virtual pad (RT analog): " + string.Join(" || ", lines)
                + " || the wait out of the cooldown (10 s / 14 s) and a second charged shot: not walked here (the harness `weapons --charge27` walks it)";
        }
    }
}
