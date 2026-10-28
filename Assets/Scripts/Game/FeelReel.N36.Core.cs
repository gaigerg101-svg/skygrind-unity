// NIGHT36 builder P: the Player scenarios of the P rows (FeelReel.N27.cs PhaseN36P -> N36CoreBody). Forced input (forcedCombat / forcedMove,
// one frame per press; L-046), whole Player frames. One Player per scenario (L-046). Not run by its author (no Player in the worktree).
// Each prints `[reel] <scenario>: PASS|RED ...` through res[0] and saves shot-N30-<name>.png frames for the lead.
//   parry36   a safe spot, every other enemy killed, god mode; a spitter 10 m ahead, pinned, is put into its wind-up (the YELLOW tell),
//             its volley thrown straight at you at the tell's end; the punch is pressed the first frame a glob is within orb.reflectRange.
//             Frames: parry36-tell (the spitter's yellow halo mid-wind-up), parry36-glob (the globs glowing yellow, the frame before the
//             punch), parry36-freeze (the first frame after the parry: the freeze and the yellow shell), parry36-burst (the return's burst at
//             the spitter). GREEN: a yellow halo seen, glob glows seen, parry36 1, the hitstop >= freezeS on the parry's frame, a burst,
//             the spitter dead. RED (-skygrindOff36): no halo, no parry36, no burst, the spitter alive.
//   launch36  husk A 6 m ahead and husk B 4 m beyond it (40 hp each, pinned until the punch): a dash at A, the punch the first frame A
//             is in reach. Frames: launch36-punch, launch36-flight (+0.1 s), launch36-burst (the burst's frame). GREEN: launch36 1, a
//             burst, two kills. RED: no launch, no kill.
//   heal36    hp 50 of 100 (no god; invulnerable); a 1 hp husk 2.5 m ahead shot with one KESTREL tap: heal36 +amount; frame heal36-number
//             (0.15 s after the kill: the cyan "+10"); then a 1 hp husk 12 m ahead: no heal. GREEN: +amount near, 0 far.
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
        static partial void N36CoreBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "parry36") body = Parry36Scenario(g, dir, res, offs, pre);
            else if (s == "launch36") body = Launch36Scenario(g, dir, res, offs, pre);
            else if (s == "heal36") body = Heal36Scenario(g, dir, res, offs, pre);
        }

        /// <summary>
        /// An open, flat lane (n36-r3: the parry's spitter stood past a1's 1 m step, the launch faced a wall 0.9 m ahead): from the safe spot and
        /// (a campaign run) every non-boss room's spawn, 16 headings each; the knee (0.4 m) and chest (1.2 m) rays clear and the floor at the
        /// start's height (+-0.15 m) every 1 m. The longest lane wins (16 m is enough). Returns its start (feet), heading and length.
        /// </summary>
        static (Vec3 at, double yaw, double len, string from) Lane36(SurvivalGame g)
        {
            var origins = new List<(Vec3 p, string name)>();
            var (safe, _, _) = SafeSpot(g);
            origins.Add((safe, "safe spot"));
            if (g.sim.campaignLevel != null)
                foreach (RoomDef r in g.sim.campaignLevel.rooms)
                    if (r.kind != "boss") origins.Add((r.spawn, "room " + r.id));
            Vec3 bestAt = safe;
            double bestYaw = 0, bestLen = 0;
            string bestFrom = "none";
            foreach (var (o0, name) in origins)
            {
                if (!g.world.RayIntersect(new Vec3(o0.x, o0.y + 1.0, o0.z), new Vec3(0, -1, 0), out RayHit fh) || fh.distance > 2.5) continue;
                var o = new Vec3(o0.x, o0.y + 1.0 - fh.distance, o0.z); // the floor under it
                for (int k = 0; k < 16; k++)
                {
                    double yaw = k * Math.PI / 8;
                    var fw = new Vec3(-Math.Sin(yaw), 0, -Math.Cos(yaw));
                    double clear = 20;
                    foreach (double h in new[] { 0.4, 1.2 })
                        if (g.world.RayIntersect(new Vec3(o.x, o.y + h, o.z), fw, out RayHit hit)) clear = Math.Min(clear, hit.distance);
                    double len = 0;
                    for (double d = 1; d <= clear - 1; d += 1)
                    {
                        var p = new Vec3(o.x + fw.x * d, o.y + 1.0, o.z + fw.z * d);
                        if (!g.world.RayIntersect(p, new Vec3(0, -1, 0), out RayHit dh) || Math.Abs(dh.distance - 1.0) > 0.15) break;
                        len = d;
                    }
                    if (len > bestLen) { bestLen = len; bestAt = o; bestYaw = yaw; bestFrom = name; }
                }
                if (bestLen >= 16) break;
            }
            return (bestAt, bestYaw, bestLen, bestFrom);
        }

        /// <summary>The impact layer's freeze left now (the sim's hitstop is drained into it every step: SurvivalGame.Impact.cs).</summary>
        static double Freeze36(SurvivalGame g) => Math.Max(g.sim.hitstopLeft, g.ImpactDirector != null ? g.ImpactDirector.FreezeLeft : 0);

        static string Rows36(SurvivalSim sim) => $"n36 tells36 {sim.n36.tells36} parry36 {sim.n36.parry36} launch36 {sim.n36.launch36} heal36 {sim.n36.heal36}";

        /// <summary>Kill every live enemy but `keep` (a run's own spawns would catch a glob or a flier).</summary>
        static void Only36(SurvivalGame g, params Enemy[] keep)
        {
            foreach (Enemy e in g.sim.enemies.all)
            {
                if (!e.alive) continue;
                if (Array.IndexOf(keep, e) < 0) e.alive = false;
            }
        }

        static IEnumerator Parry36Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            var (spot, spotYaw, laneLen, laneFrom) = Lane36(g);
            string pc = Preconditions(g, spot) + $" | lane {laneLen:0} m from {laneFrom}" + " | " + Rows36(sim);
            offs.Add(() => { g.forcedCombat = IdleCombat; g.forcedMove = new InputFrame(); });
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            g.forcedCombat = IdleCombat;
            g.forcedMove = new InputFrame();
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = false; // FeelReel.One froze every enemy before this body (n36-r2's RED: no Tick, so no tell clock and no flight)
            God(g);
            double spD = Math.Min(10, laneLen - 1);
            var fwP = new Vec3(-Math.Sin(spotYaw), 0, -Math.Cos(spotYaw));
            Spitter sp = spD >= 6 ? g.husks.Spawn("spitter", new Vec3(spot.x + fwP.x * spD, spot.y, spot.z + fwP.z * spD), g.Eye) as Spitter : null;
            if (sp == null)
            {
                res[0] = $"RED no spitter placed on the lane ({laneLen:0} m) | " + pc;
                yield break;
            }
            int orbHits = 0;
            double burstToSpitter = -1;
            int burstHits = -1;
            offs.Add(g.events.On<HitEvent>("hit", e => { if (e.weapon == "orb" && e.enemy == sp.id) orbHits++; }));
            offs.Add(g.events.On<Launch36BurstEvent>("parry36Burst", e => { burstToSpitter = Vec3.DistanceTo(e.position, sp.AimPoint); burstHits = e.hits; }));
            Vec3 pin = sp.position;
            double reach = g.cfg.Num("orb.reflectRange");
            int parries36 = 0, bursts = 0, yellowMax = 0, glowMax = 0, shellMax = 0;
            float poseMax = 0;
            double hitstopAtParry = 0;
            int fTell = -1, fGlob = -1, fParry = -1, fBurst = -1;
            offs.Add(g.events.On<HitEvent>(Parry36.Event, e => { parries36++; }));
            offs.Add(g.events.On<Launch36BurstEvent>("parry36Burst", e => { bursts++; }));
            Start(g, "parry36", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 40;
            bool thrown = false, punched = false;
            Action hold = () =>
            {
                God(g);
                Only36(g, sp);
                if (sp.alive)
                {
                    sp.position = pin;
                    sp.velocity = default;
                    if (sp.state != "tell") sp.cooldownSteps = 9999;
                    Look(g, sp.AimPoint);
                }
                yellowMax = Math.Max(yellowMax, Tell36View.YellowHalos);
                poseMax = Math.Max(poseMax, Tell36View.PoseMax);
                glowMax = Math.Max(glowMax, Tell36View.GlobGlows);
                shellMax = Math.Max(shellMax, Tell36View.ShellsShown);
            };
            yield return Wait27(g, 0.4, wallEnd, hold);
            // the wind-up: the spitter's tell state, counted down by the SIM (Spitter.StepSpitter; the scan raises the YELLOW tell). The reel only
            // starts it and, at its last 2 steps, throws the volley itself straight at you (the brain's hit roll could aim a volley beside you)
            int tellSteps = MovementMath.StepsFor(sim.husks.acid.p.tellTime, sim.step);
            int tellStart = sim.simStep, volleys0 = sim.husks.acid.volleys;
            sp.state = "tell";
            sp.stateSteps = sp.tellSteps = tellSteps;
            sp.committed = true;
            bool shotTell = false;
            int globsAfterThrow = 0, tellElapsed = 0, simCounted = 0;
            while (Time.realtimeSinceStartup < wallEnd && !thrown)
            {
                hold();
                tellElapsed = sim.simStep - tellStart;
                simCounted = tellSteps - sp.stateSteps; // the sim's own count (0 if the spitter is not ticking)
                if (sp.state != "tell" && !thrown) { thrown = true; break; } // the sim fired it (its own volley)
                if (tellElapsed > tellSteps * 3) break; // the sim never counted: say so (RED), never mask it
                if (!shotTell && simCounted >= tellSteps / 2)
                {
                    shotTell = true;
                    fTell = FeelProbe.FrameCount;
                    yield return Shot30(dir, "parry36-tell");
                    continue;
                }
                if (sp.state == "tell" && sp.stateSteps <= 2)
                {
                    // the volley straight at you (never the deliberate miss), at the tell's end
                    sp.state = "idle";
                    sp.committed = false;
                    sp.cooldownSteps = 9999;
                    HuskAi saved = sp.ai;
                    sp.ai = null;
                    sim.husks.acid.ThrowVolley(sp, g.Feet);
                    sp.ai = saved;
                    thrown = true;
                    foreach (SpitterAcid.Glob gl in sim.husks.acid.globs) if (gl.alive && !gl.reflected) globsAfterThrow++;
                }
                yield return null;
            }
            double nearestSeen = double.PositiveInfinity;
            while (Time.realtimeSinceStartup < wallEnd && !punched)
            {
                hold();
                double nearest = double.PositiveInfinity;
                bool any = false;
                foreach (SpitterAcid.Glob gl in sim.husks.acid.globs)
                    if (gl.alive && !gl.reflected) { any = true; nearest = Math.Min(nearest, Vec3.DistanceTo(gl.position, g.Eye)); }
                nearestSeen = Math.Min(nearestSeen, nearest);
                if (!any) break;
                if (fGlob < 0 && nearest <= reach * 2.5)
                {
                    fGlob = FeelProbe.FrameCount;
                    yield return Shot30(dir, "parry36-glob");
                    continue;
                }
                if (nearest <= reach)
                {
                    g.forcedCombat = Punch;
                    punched = true;
                    yield return null;
                    g.forcedCombat = IdleCombat;
                    hitstopAtParry = Freeze36(g);
                    fParry = FeelProbe.FrameCount;
                    yield return Shot30(dir, "parry36-freeze");
                    break;
                }
                yield return null;
            }
            double t0 = sim.SimTime;
            while (Time.realtimeSinceStartup < wallEnd && bursts == 0 && sim.SimTime - t0 < 1.5) { hold(); yield return null; }
            if (bursts > 0)
            {
                fBurst = FeelProbe.FrameCount;
                yield return Shot30(dir, "parry36-burst");
            }
            yield return Wait27(g, 0.6, wallEnd, () => { God(g); yellowMax = Math.Max(yellowMax, Tell36View.YellowHalos); shellMax = Math.Max(shellMax, Tell36View.ShellsShown); });
            yield return Stop();
            bool ok = yellowMax > 0 && poseMax > 0.5f && glowMax > 0 && parries36 == 1 && hitstopAtParry > 0 && bursts > 0 && !sp.alive;
            res[0] = $"{(ok ? "GREEN" : "RED")} yellow halos max {yellowMax}, pose max {poseMax.ToString("0.00", ci)} (the spitter tilted back + sac swelled; 1 = full), glob glows max {glowMax}, parry36 {parries36}, freeze left on the parry frame (the impact layer) {hitstopAtParry.ToString("0.000", ci)} s (freezeS {(sim.parry36Core != null ? sim.parry36Core.freezeS.ToString("0.00", ci) : "-")}), bursts {bursts}, shells max {shellMax}, spitter {(sp.alive ? $"ALIVE {sp.hp:0} hp" : "DEAD")}, nearest glob {nearestSeen.ToString("0.0", ci)} m; the tell counted by the sim {simCounted} / {tellSteps} steps in {tellElapsed} sim steps, volleys {sim.husks.acid.volleys - volleys0}, globs live after the throw {globsAfterThrow}, spitter {Vec3.DistanceTo(pin, g.Feet).ToString("0.0", ci)} m from you; the return: orb hits on the spitter {orbHits}, the burst {(burstToSpitter < 0 ? "none" : burstToSpitter.ToString("0.0", ci) + " m from it, hits " + burstHits)} | frames f{fTell:0000} tell, f{fGlob:0000} glob, f{fParry:0000} freeze, f{fBurst:0000} burst; shots shot-N30-parry36-tell / -glob / -freeze / -burst.png | {pc}";
        }

        static IEnumerator Launch36Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            var (spot, spotYaw, laneLen, laneFrom) = Lane36(g);
            string pc = Preconditions(g, spot) + $" | lane {laneLen:0} m from {laneFrom}" + " | " + Rows36(sim);
            offs.Add(() => { g.forcedCombat = IdleCombat; g.forcedMove = new InputFrame(); });
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            g.forcedCombat = IdleCombat;
            g.forcedMove = new InputFrame();
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = false; // FeelReel.One froze every enemy before this body (n36-r2's RED: no Tick, so no tell clock and no flight)
            God(g);
            double hp = g.cfg.Num("husk.hp");
            double yaw = spotYaw;
            var fwL = new Vec3(-Math.Sin(yaw), 0, -Math.Cos(yaw));
            Husk a = laneLen >= 11 ? g.husks.Spawn(new Vec3(spot.x + fwL.x * 6, spot.y, spot.z + fwL.z * 6), g.Eye) : null;
            Husk b = a != null ? g.husks.Spawn(new Vec3(spot.x + fwL.x * 10, spot.y, spot.z + fwL.z * 10), g.Eye) : null;
            if (a == null || b == null)
            {
                res[0] = $"RED husks not placed (A {a != null}, B {b != null}) | " + pc;
                yield break;
            }
            a.hp = a.maxHp = b.hp = b.maxHp = hp;
            Vec3 pinA = a.position, pinB = b.position;
            int launches = 0, bursts = 0, kills = 0;
            int fPunch = -1, fFlight = -1, fBurst = -1;
            string stateAtPunch = "";
            const double bLead = 3.5; // m: B on the flight's line (a 16 m/s flight covers it in ~0.2 s)
            bool bMoved = false;
            double bWall = -1;
            offs.Add(g.events.On(Launch36.LaunchEvent, o => launches++));
            offs.Add(g.events.On<Launch36BurstEvent>(Launch36.BurstEvent, e => bursts++));
            offs.Add(g.events.On<KillEvent>("kill", e => { if (e.owner == "player") kills++; }));
            Start(g, "launch36", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 30;
            bool punched = false;
            Action hold = () =>
            {
                God(g);
                Only36(g, a, b);
                if (!punched && a.alive) { a.position = pinA; a.velocity = default; a.cooldownSteps = 9999; }
                if (b.alive && bursts == 0) { b.position = pinB; b.velocity = default; b.cooldownSteps = 9999; }
            };
            hold();
            Look(g, a.AimPoint);
            yield return Wait27(g, 0.3, wallEnd, hold);
            Look(g, a.AimPoint);
            double pRange = g.cfg.Num("punch.range");
            int frame = 0;
            while (Time.realtimeSinceStartup < wallEnd && !punched && frame < 240)
            {
                hold();
                Look(g, a.AimPoint);
                g.forcedMove = new InputFrame { moveZ = 1, dash = new Skygrind.Core.Button { held = frame < 2, pressed = frame == 0 } };
                double flat = Math.Sqrt((a.position.x - g.Feet.x) * (a.position.x - g.Feet.x) + (a.position.z - g.Feet.z) * (a.position.z - g.Feet.z));
                if (frame > 0 && flat <= pRange * 0.9)
                {
                    stateAtPunch = sim.mover.state.ToString();
                    g.forcedCombat = Punch;
                    punched = true;
                    yield return null;
                    g.forcedCombat = IdleCombat;
                    g.forcedMove = new InputFrame();
                    // n36-r2: A flew but met nothing: put B on the launch's own line, bLead m beyond A (on A's floor), pinned until the burst
                    Launch36 lc = sim.launch36Core;
                    if (lc != null && lc.flights.Count > 0 && a.alive && b.alive)
                    {
                        Launch36.Flight fl = lc.flights[0];
                        var onLine = new Vec3(a.position.x + fl.dx * bLead, pinA.y, a.position.z + fl.dz * bLead);
                        if (!g.world.RayIntersect(new Vec3(a.position.x, pinA.y + 1, a.position.z), new Vec3(fl.dx, 0, fl.dz), out RayHit wl) || wl.distance > bLead + 0.5)
                        {
                            pinB = onLine;
                            b.position = onLine;
                            b.velocity = default;
                            bMoved = true;
                        }
                        else bWall = wl.distance;
                    }
                    fPunch = FeelProbe.FrameCount;
                    yield return Shot30(dir, "launch36-punch");
                    break;
                }
                frame++;
                yield return null;
            }
            g.forcedMove = new InputFrame();
            yield return Wait27(g, 0.1, wallEnd, hold);
            fFlight = FeelProbe.FrameCount;
            yield return Shot30(dir, "launch36-flight");
            double t0 = sim.SimTime;
            while (Time.realtimeSinceStartup < wallEnd && bursts == 0 && sim.SimTime - t0 < 1.2) { hold(); yield return null; }
            if (bursts > 0)
            {
                fBurst = FeelProbe.FrameCount;
                yield return Shot30(dir, "launch36-burst");
            }
            yield return Wait27(g, 0.6, wallEnd, () => God(g));
            yield return Stop();
            bool ok = punched && launches == 1 && bursts >= 1 && (bMoved ? kills == 2 : kills >= 1); // a wall within bLead: the wall burst is the rule (A dies)
            Launch36 l36 = sim.launch36Core;
            res[0] = $"{(ok ? "GREEN" : "RED")} punched {(punched ? "yes" : "NO")} in state {stateAtPunch}, launch36 {launches}, bursts {bursts}, kills {kills} (want 1 / >=1 / 2); B put on the line {(bMoved ? "yes" : bWall >= 0 ? $"NO (a wall {bWall.ToString("0.0", ci)} m along it: the flight should wall-burst)" : "NO")}; the flight ended: {(l36 != null ? $"{l36.lastEnd}, thrown {l36.lastThrown.ToString("0.0", ci)} m, closest body gap {l36.lastMinGap.ToString("0.00", ci)} m, wall bursts {l36.wallBursts}" : "no launch36 object")} | frames f{fPunch:0000} punch, f{fFlight:0000} flight, f{fBurst:0000} burst; shots shot-N30-launch36-punch / -flight / -burst.png | {pc}";
        }

        static IEnumerator Heal36Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + " | " + Rows36(sim);
            bool inv = sim.mover.invulnerable;
            offs.Add(() => { g.forcedCombat = IdleCombat; g.forcedMove = new InputFrame(); sim.mover.invulnerable = inv; });
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            g.forcedCombat = IdleCombat;
            g.forcedMove = new InputFrame();
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = false; // FeelReel.One froze every enemy before this body (n36-r2's RED: no Tick, so no tell clock and no flight)
            sim.mover.invulnerable = true;
            double max = g.cfg.Num("health.max");
            int heals = 0;
            double healed = 0;
            offs.Add(g.events.On<Heal36Event>(Heal36.Event, e => { heals++; healed += e.amount; }));
            Start(g, "heal36", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 30;
            var lines = new List<string>();
            int fNumber = -1;
            foreach (double d in new[] { 2.5, 12.0 })
            {
                g.husks.KillAll();
                g.health.max = max;
                g.health.hp = 50;
                Husk h = PlaceAt(g, d, 0, out double _);
                if (h == null) { lines.Add($"{d} m: no husk"); continue; }
                h.hp = h.maxHp = 1;
                Vec3 pin = h.position;
                int h0 = heals;
                double hp0 = g.health.hp;
                Action hold = () =>
                {
                    Only36(g, h);
                    if (h.alive) { h.position = pin; h.velocity = default; h.cooldownSteps = 9999; Look(g, h.AimPoint); }
                };
                yield return Wait27(g, 0.3, wallEnd, hold);
                for (int tap = 0; tap < 4 && h.alive && Time.realtimeSinceStartup < wallEnd; tap++)
                {
                    hold();
                    g.forcedCombat = Fire;
                    yield return Frames(3);
                    g.forcedCombat = IdleCombat;
                    double tt = sim.SimTime;
                    while (h.alive && sim.SimTime - tt < 0.5 && Time.realtimeSinceStartup < wallEnd) { hold(); yield return null; }
                }
                if (d < 4)
                {
                    // 0.15 s after the kill: the cyan number is up and rising (it lives Tell36HealNumberS)
                    yield return Wait27(g, 0.15, wallEnd, null);
                    fNumber = FeelProbe.FrameCount;
                    yield return Shot30(dir, "heal36-number");
                }
                else yield return Wait27(g, 0.3, wallEnd, null);
                lines.Add($"{d.ToString("0.0", ci)} m: killed {(!h.alive ? "yes" : "NO")}, heal36 {heals - h0}, hp {hp0:0} -> {g.health.hp:0}, numbers shown {Tell36View.HealNumbersShown}");
            }
            yield return Wait27(g, 0.5, wallEnd, null);
            yield return Stop();
            double want = sim.heal36Core != null ? sim.heal36Core.amount : 0;
            bool ok = heals == 1 && Math.Abs(healed - want) < 1e-9;
            res[0] = $"{(ok ? "GREEN" : "RED")} heal36 {heals} (+{healed:0}; want 1 near, 0 far, +{want:0}) || {string.Join(" || ", lines)} | frame f{fNumber:0000}; shot shot-N30-heal36-number.png | {pc}";
        }
    }
}
