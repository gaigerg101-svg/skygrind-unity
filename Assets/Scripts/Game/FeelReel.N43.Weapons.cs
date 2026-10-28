// NIGHT43 builder W1: the Player scenario `weapons43` (the dispatch: FeelReel.N43.cs). Not run by its author (no Player in the worktree).
// Boot as weapons42 (reel.sh + -skygrindMap <a campaign level> + -skygrindReelOnly weapons43, L-062). The BEFORE pass: the same boot with
// -skygrindOff43 (NIGHT42's game: the reel never forces a row). The AFTER pass: without it; the reel turns W1's three rows on for the run
// (sim.N43WeaponsForceOn: gauntlet43, parry43, fusion43, set up as a run's start would). The player on SafeSpot's floor, God; the trigger is a
// virtual pad's RT (Pad27); the dash a forcedMove press. Frames (shot-N30-<name>.png), t = GAME time:
//   W43-parry-000 / -050 / -150   G18: the PULSE gauntlet in hand, a DRONE orb (12 m/s) fired at the chest from 8 m, RT pressed when the orb
//                                 is 3.5 m from the eye (inside parry43's catch sphere, outside NIGHT42's 2 m): after = PARRIED at ~3.5 m
//                                 (the orb going back), before = missed (the orb lands; the line prints parried / hurt and the distance)
//   W43-fusion-dash               G19: the moment the dash starts at a husk ~5 m ahead (the gauntlet in hand, RT 0.1 s after the dash)
//   W43-fusion-000 / -100         the gauntlet's hit frame and +0.1 s: after = a FUSION (fusion43 event: F1's burst; the husk thrown ~4 m),
//                                 before = a plain gauntlet hit
// The line also prints the gauntlet's press-to-next-swing gap (RT held 3 s at the husk: 1.25 s after, 1.8 s before) and the FUSION's
// payload (speed, damage, killed). GREEN when every frame was saved and the swing landed.
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
        static partial void N43Weapons(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Weapons43Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Weapons43Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            bool after = !N43Blocks.ForceOff;
            if (after) sim.N43WeaponsForceOn();
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | map {g.MapId} | pass {(after ? "AFTER (W1's rows forced on)" : "BEFORE (-skygrindOff43)")} | n43 {sim.n43}";
            offs.Add(() => { g.forcedCombat = IdleCombat; g.forcedMove = null; g.husks.frozen = false; });
            g.forcedCombat = null;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "weapons43", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 150;
            int parries = 0, hurts = 0, fusions = 0, swings = 0;
            var swingAt = new List<double>();
            double lastSpeed = 0, lastDamage = 0;
            bool lastKilled = false;
            offs.Add(g.events.On<HitEvent>("parry", e => parries++));
            offs.Add(g.events.On<DroneOrbs.DestroyedEvent>("orbDestroyed", e => { if (e.reason == "player") hurts++; }));
            offs.Add(g.events.On<Fusion43Event>(N43Events.Fusion, e => { fusions++; lastSpeed = e.speed; lastDamage = e.damage; lastKilled = e.killed; }));
            offs.Add(g.events.On<MeleeEvent>("meleeSwing", e => { if (e.weapon == "gauntlet") { swings++; swingAt.Add(sim.SimTime); } }));
            int gHits = 0;
            double gHitAt = -1, gHitDamage = 0;
            offs.Add(g.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "gauntlet") { gHits++; if (gHitAt < 0) { gHitAt = sim.SimTime; gHitDamage = e.damage; } } }));
            var lines = new List<string>();
            int framesSaved = 0;
            IEnumerator Snap(string name)
            {
                lines.Add(name);
                yield return Shot30(dir, name);
                framesSaved++;
            }
            IEnumerator At(double t0, double off, Action each)
            {
                while (sim.SimTime - t0 < off && Time.realtimeSinceStartup < wallEnd) { each?.Invoke(); yield return null; }
            }
            Action god = () => God(g);

            // ---- G18: the gauntlet in hand, a DRONE orb at the chest from 8 m, RT when it is 3.5 m from the eye ----
            yield return Equip27(g, "gauntlet");
            g.forcedCombat = null;
            yield return Wait27(g, 2.0, wallEnd, god); // any swing's cooldown is over
            Vec3 eye = sim.Eye;
            Vec3 fwd = new Vec3(-Math.Sin(spotYaw), 0, -Math.Cos(spotYaw));
            var src = new Vec3(eye.x + fwd.x * 8, eye.y - 0.4, eye.z + fwd.z * 8);
            DroneOrbs orbs = sim.husks.orbs;
            DroneOrbs.Orb orb = orbs != null ? orbs.Fire(src, Vec3.Normalize(new Vec3(eye.x, eye.y - 0.4, eye.z) - src), null, 12) : null;
            Look(g, src);
            int p0 = parries, h0 = hurts;
            string parryLine;
            if (orb == null) parryLine = "parry: NO ORB (no pool)";
            else
            {
                // n43-r1 iteration: in both passes the orb never came within 3.5 m ("pressed at -1.00 m"; weapons42 the same): the reel now
                // logs what ended it, gives it its flight time ((8 - 3.5) / 12 = 0.375 s, + 0.25 s), and when it did not arrive fires a second
                // orb 3.6 m out (inside parry43's sphere, outside NIGHT42's 2 m) and presses at once: the frame pair exists either way.
                string firstEnd = null;
                offs.Add(g.events.On<DroneOrbs.DestroyedEvent>("orbDestroyed", e => { if (firstEnd == null) firstEnd = e.reason; }));
                double w0 = sim.SimTime, dPress = -1, d0 = Vec3.DistanceTo(orb.position, sim.Eye), dLast = d0;
                int framesWaited = 0;
                while (orb.alive && !orb.reflected && sim.SimTime - w0 < 0.625 && Time.realtimeSinceStartup < wallEnd)
                {
                    God(g);
                    Look(g, orb.position);
                    double d = Vec3.DistanceTo(orb.position, sim.Eye);
                    dLast = d;
                    if (d <= 3.5) { dPress = d; break; }
                    framesWaited++;
                    yield return null;
                }
                string how = $"flew 8 m (start {d0.ToString("0.00", ci)} m, last {dLast.ToString("0.00", ci)} m after {framesWaited} frames / {(sim.SimTime - w0).ToString("0.000", ci)} s sim; alive {orb.alive}, ended by {firstEnd ?? "nothing"})";
                if (dPress < 0)
                {
                    orb.alive = false;
                    Vec3 e2 = sim.Eye;
                    var src2 = new Vec3(e2.x + fwd.x * 3.6, e2.y - 0.4, e2.z + fwd.z * 3.6);
                    orb = orbs.Fire(src2, Vec3.Normalize(new Vec3(e2.x, e2.y - 0.4, e2.z) - src2), null, 12);
                    Look(g, src2);
                    dPress = orb != null ? Vec3.DistanceTo(src2, e2) : -1;
                    how += " -> a second orb fired 3.6 m out, pressed at once";
                    if (orb == null) { parryLine = "parry: NO SECOND ORB"; yield return Stop(); res[0] = $"RED weapons43 | {parryLine} | {how}"; yield break; }
                }
                pad.Send(0f, 1f);
                double tPress = sim.SimTime;
                yield return Snap("W43-parry-000");
                yield return Frames(1);
                pad.Send(0f, 0f);
                yield return At(tPress, 0.05, god);
                yield return Snap("W43-parry-050");
                yield return At(tPress, 0.15, god);
                yield return Snap("W43-parry-150");
                yield return At(tPress, 0.6, god);
                parryLine = $"parry: {(parries > p0 ? "PARRIED" : "missed")} (pressed at {dPress.ToString("0.00", ci)} m of the eye; orb reflected {orb.reflected}; caught by body / sphere / window {orbs.caughtBody43} / {orbs.caughtAhead43} / {orbs.caughtSoon43}; buffer parries {g.loadout.orbBufferParries42}), hurt {hurts - h0}; the orb {how}";
            }

            // ---- G19: a dash at a husk ~5 m ahead, RT 0.1 s after the dash ----
            yield return Wait27(g, 1.6, wallEnd, god); // the parry swing's cooldown (1.25 s after, 1.8 s before) is over
            Husk bag = PlaceAt(g, 5, 0, out double bagYaw);
            if (bag != null) bag.hp = bag.maxHp = 1e9;
            string fusionLine;
            if (bag == null) fusionLine = "fusion: NO HUSK placed 5 m ahead";
            else
            {
                Action lookBag = () => { God(g); if (bag.alive) { bag.hp = 1e9; Look(g, bag.AimPoint); } };
                yield return Wait27(g, 0.3, wallEnd, lookBag);
                g.husks.frozen = false; // the knock shows (the husk walks at you meanwhile; God)
                int f0 = fusions;
                gHitAt = -1;
                // n43-r1 iteration: the hit missed in both passes (speed 0): the forcedMove dash is logged now (the mover's dash steps seen), and
                // while it has not dashed the reel carries the player at the dash's 24 m/s toward the husk for the dash's 0.25 s (a FUSION by
                // speed: >= 14 m/s), so the swing lands at speed either way; the line says which happened.
                double gap0 = JsMath.Hypot(bag.position.x - sim.mover.position.x, bag.position.z - sim.mover.position.z);
                g.forcedMove = new InputFrame { moveZ = 1, dash = new Skygrind.Core.Button { pressed = true, held = true } };
                double tDash = sim.SimTime;
                int dashSeen = 0, carried = 0;
                double peak = 0, gapPress = -1;
                Action dashing = () =>
                {
                    lookBag();
                    if (sim.mover.DashStepsLeft > 0) dashSeen++;
                    else if (sim.SimTime - tDash < 0.25 && dashSeen == 0 && bag.alive)
                    {
                        double dx = bag.position.x - sim.mover.position.x, dz = bag.position.z - sim.mover.position.z, l = Math.Max(1e-6, JsMath.Hypot(dx, dz));
                        sim.mover.velocity = new Vec3(dx / l * 24, sim.mover.velocity.y, dz / l * 24);
                        carried++;
                    }
                    peak = Math.Max(peak, JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z));
                };
                yield return Snap("W43-fusion-dash");
                g.forcedMove = new InputFrame { moveZ = 1 };
                yield return At(tDash, 0.1, dashing);
                gapPress = JsMath.Hypot(bag.position.x - sim.mover.position.x, bag.position.z - sim.mover.position.z);
                pad.Send(0f, 1f);
                dashing();
                yield return null;
                dashing();
                yield return null;
                pad.Send(0f, 0f);
                while (gHitAt < 0 && sim.SimTime - tDash < 0.8 && Time.realtimeSinceStartup < wallEnd) { dashing(); yield return null; }
                double tHit = gHitAt >= 0 ? gHitAt : sim.SimTime;
                yield return Snap("W43-fusion-000");
                g.forcedMove = new InputFrame();
                yield return At(tHit, 0.1, lookBag);
                yield return Snap("W43-fusion-100");
                g.forcedMove = null;
                fusionLine = $"fusion: gauntlet hit {(gHitAt >= 0 ? "LANDED" : "missed")} +{(gHitAt >= 0 ? gHitAt - tDash : -1).ToString("0.000", ci)} s after the dash, damage {gHitDamage.ToString("0", ci)}; FUSION {(fusions > f0 ? "YES" : "no")} (speed {lastSpeed.ToString("0.0", ci)} m/s, damage {lastDamage.ToString("0", ci)}, killed {lastKilled}); the husk {gap0.ToString("0.0", ci)} m away at the dash, {gapPress.ToString("0.0", ci)} m at the press; dash steps seen {dashSeen}, frames carried at 24 m/s {carried}, peak speed {peak.ToString("0.0", ci)} m/s";

                // ---- G17: RT held 3 s at the husk: the press-to-next-swing gap ----
                g.husks.frozen = true;
                int s0 = swingAt.Count;
                yield return Wait27(g, 1.6, wallEnd, lookBag);
                pad.Send(0f, 1f);
                yield return Wait27(g, 3.2, wallEnd, () => { lookBag(); pad.Send(0f, 1f); });
                pad.Send(0f, 0f);
                var gaps = new List<string>();
                for (int i = s0 + 1; i < swingAt.Count; i++) gaps.Add((swingAt[i] - swingAt[i - 1]).ToString("0.000", ci));
                fusionLine += $" | swing gaps held {(gaps.Count > 0 ? string.Join(", ", gaps) : "none")} s";
            }
            yield return Wait27(g, 0.3, wallEnd, god);
            yield return Stop();

            bool ok = framesSaved == 6 && gHits > 0;
            res[0] = $"{(ok ? "GREEN" : "RED")} weapons43 | {parryLine} | {fusionLine} | frames {framesSaved}/6 || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
