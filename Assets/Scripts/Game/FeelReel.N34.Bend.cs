// NIGHT34 builder B (B1): the Player scenario of the grapple BEND (the name is FeelReel.N27.cs PhaseN34B; the body is the partial N34BendBody). Not run by
// its author (no Player in the worktree). Input: the forced frames of the player's own input path (g.forcedMove for the grapple, g.forcedCombat for the
// trigger and the slot), never the virtual pad, so an unfocused Player window cannot drop a press (L-046: the failure b8655b3c hardened against was the
// Input System disabling a virtual pad while unfocused; forced frames do not go through a device).
//   bend34  any boot (the default SURVIVAL map, or -skygrindMap n34-foundry / n33-foundry: a campaign level). A safe spot, invulnerable, the ARCLINE owned,
//     the KESTREL, the THUNDERCLAP and the NAILGUN owned, 6 husks 8-14 m around (1 hp each, so any hit inside the bend is a kill). The grapple target: the
//     nearest grapple node in sight (8-45 m), else the farthest wall in reach (16 headings, +8 deg). The grapple is HELD (a swing latches on a wall and a
//     node alike) until the rope attaches, 1.2 s at most.
//     Frames: f1 the latch (bend34Start), f2 +0.2 s real time, f3 the first kill inside the bend (bend34Kill), f4 the exit (bend34End + the ease out).
//     Inside the bend: one slot press per 0.25 s real time to the next owned gun (three swaps), then the trigger on the nearest husk until a kill.
//     Lines: the mode (global / split), the tier at the latch, the meter before / after, the share (min over the bend; global path) and Depth01 (max), the
//     swap steps (sim steps from each press to the new gun in hand: 0 = the press step), the kills inside, the bend's real length, the world steps held
//     (split), the frame numbers. PASS (bend34 on): a bend34Start, every swap 0 steps, a kill inside, a bend34End, and (global) the share min 0.25 +- 0.02.
//     With -skygrindOff34 (the RED run): no bend, the swaps take NIGHT33's timeline (15 steps), share 1: RED with the numbers (the before frames).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using Btn = Skygrind.Core.Button;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N34BendBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "bend34") body = Bend34Scenario(g, dir, res, offs, pre);
        }

        /// <summary>The grapple target from the eye: the nearest node in sight 8-45 m away, else the farthest wall hit under 38 m (16 headings, +8 deg).</summary>
        static Vec3? Bend34Target(SurvivalGame g, out string kind)
        {
            Vec3 eye = g.Eye;
            Vec3? best = null;
            double bd = double.MaxValue;
            kind = "none";
            foreach (Vec3 n in g.sim.map.grappleNodes)
            {
                Vec3 to = n - eye;
                double d = to.Length;
                if (d < 8 || d > 45) continue;
                if (g.world.RayIntersect(eye, to / d, out RayHit hit) && hit.distance < d - 0.5) continue;
                if (d < bd) { bd = d; best = n; kind = "node"; }
            }
            if (best.HasValue) return best;
            double far = 0;
            for (int k = 0; k < 16; k++)
            {
                double yaw = k * Math.PI / 8;
                Vec3 dirv = GrappleAim.AimDirection(yaw, 8 * Math.PI / 180);
                if (g.world.RayIntersect(eye, dirv, out RayHit h) && h.distance <= 38 && h.distance > far) { far = h.distance; best = eye + dirv * h.distance; kind = "wall"; }
            }
            return best;
        }

        static IEnumerator Bend34Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            bool on = sim.Bend34On;
            string mode = !on ? "off" : sim.n34.bendSplit34 ? "split" : "global";
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | n34.bend34 {sim.n34.bend34}, split {sim.n34.bendSplit34}, map {g.MapId}, campaign {sim.campaignOn}";
            offs.Add(() => { g.forcedCombat = IdleCombat; g.forcedMove = null; g.husks.frozen = false; });
            g.forcedCombat = null;
            foreach (string gun in new[] { "revolver", "shotgun", "nailgun" })
                if (g.loadout.owned.IndexOf(gun) < 0) g.loadout.AddWeapon(gun);
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            if (sim.arcline21 == null) { res[0] = "RED: no ARCLINE in this run (design.n21.arcline off) | " + pc; yield break; }
            if (!sim.arcline21.Owned) sim.arcline21.Own(true);
            g.husks.KillAll();
            // six husks 8-14 m around (a fight), 1 hp each
            var husks = new List<Husk>();
            double[] dists = { 8, 10, 12, 14, 9, 11 };
            double[] lats = { -3, 3, -5, 5, 0, -1 };
            for (int i = 0; i < dists.Length; i++)
            {
                Husk h = PlaceAt(g, dists[i], lats[i], out _);
                if (h == null) continue;
                h.hp = h.maxHp = 1;
                husks.Add(h);
            }
            Start(g, "bend34", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 90;
            yield return Equip27(g, "revolver");
            God(g);

            int fLatch = -1, fPlus = -1, fKill = -1, fExit = -1, starts = 0, ends = 0, kills = 0, heldWorld = 0, bendSteps = 0;
            string tierAtLatch = "-";
            double meterBefore = sim.Bend34Meter, meterAfter = double.NaN, shareMin = 1, depthMax = 0, tLatch = 0, tEnd = 0;
            offs.Add(g.events.On<Bend34Event>("bend34Start", e => { starts++; if (fLatch < 0) { fLatch = FeelProbe.FrameCount; tierAtLatch = e.tier; meterAfter = e.meter; tLatch = Time.realtimeSinceStartup; } }));
            offs.Add(g.events.On<Bend34Event>("bend34Kill", e => { kills++; if (fKill < 0) fKill = FeelProbe.FrameCount; }));
            offs.Add(g.events.On<Bend34Event>("bend34End", e => { ends++; if (tEnd == 0) tEnd = Time.realtimeSinceStartup; }));

            // ---- the latch ----
            Vec3? target = Bend34Target(g, out string targetKind);
            if (!target.HasValue) { res[0] = $"RED: no grapple node or wall in reach from the spot | {pc}"; yield return Stop(); yield break; }
            double tPress = Time.realtimeSinceStartup;
            bool attached = false;
            while (!attached && Time.realtimeSinceStartup - tPress < 1.2 && Time.realtimeSinceStartup < wallEnd)
            {
                God(g);
                Look(g, target.Value);
                g.forcedMove = new InputFrame { grapple = new Btn { held = true, pressed = Time.realtimeSinceStartup - tPress < 0.05 } };
                yield return null;
                attached = sim.mover.rope.phase == RopePhase.Attached;
            }
            if (attached && fLatch < 0) fLatch = FeelProbe.FrameCount; // the row off: the latch frame all the same (the RED f1)
            if (tLatch == 0) tLatch = Time.realtimeSinceStartup;
            yield return Shot30(dir, "bend34-f1-latch");

            // ---- inside: three swaps at full speed, f2 at +0.2 s, then a kill ----
            var swapSteps = new List<int>();
            int pressStep = -1, wantSlot = -1, swaps = 0, swapAt = -1;
            // the swap steps from the loadout's own events: weaponSwap on the press step, weaponRaise when the new gun is in hand (sim steps between)
            offs.Add(g.events.On("weaponSwap", _ => { if (pressStep >= 0 && swapAt < 0) swapAt = sim.simStep; }));
            offs.Add(g.events.On("weaponRaise", _ =>
            {
                if (swapAt < 0) return;
                swapSteps.Add(sim.simStep - swapAt);
                swapAt = -1;
                pressStep = -1;
            }));
            double nextPress = Time.realtimeSinceStartup;
            bool shotPlus = false, shotKill = false, firing = false;
            Husk aimAt = null;
            double tInside = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - tInside < 6 && Time.realtimeSinceStartup < wallEnd && (on ? ends == 0 : Time.realtimeSinceStartup - tInside < 2.5))
            {
                God(g);
                g.forcedMove = new InputFrame { grapple = new Btn { held = Time.realtimeSinceStartup - tInside < 0.4 } }; // the swing held a moment, then let go
                CombatInput c = new CombatInput { weapon = -1 };
                if (pressStep >= 0 && swapAt < 0 && g.loadout.targetSlot != wantSlot) c.weapon = wantSlot; // the ask not taken yet (a frame with no step): ask again
                if (swaps < 3 && pressStep < 0 && Time.realtimeSinceStartup >= nextPress)
                {
                    wantSlot = (g.loadout.slot + 1) % g.loadout.owned.Count;
                    c.weapon = wantSlot;
                    pressStep = sim.simStep;
                    swaps++;
                    nextPress = Time.realtimeSinceStartup + 0.25;
                }
                else if (swaps >= 3 && pressStep < 0)
                {
                    if (aimAt == null || !aimAt.alive)
                    {
                        aimAt = null;
                        double bd = double.MaxValue;
                        foreach (Husk h in husks)
                            if (h.alive && Vec3.DistanceTo(h.AimPoint, g.Eye) < bd) { bd = Vec3.DistanceTo(h.AimPoint, g.Eye); aimAt = h; }
                    }
                    if (aimAt != null)
                    {
                        Look(g, aimAt.AimPoint);
                        firing = !firing; // a tap a frame: every gun fires on the press
                        c.fire = new Btn { held = firing, pressed = firing };
                    }
                }
                g.forcedCombat = c;
                yield return null;
                if (sim.Bend34Inside) bendSteps++;
                if (sim.Bend34Inside && !sim.Bend34WorldStepped) heldWorld++;
                shareMin = Math.Min(shareMin, sim.Bend34Share);
                depthMax = Math.Max(depthMax, sim.Bend34Depth01);
                if (!shotPlus && Time.realtimeSinceStartup - tLatch >= 0.2)
                {
                    shotPlus = true;
                    fPlus = FeelProbe.FrameCount;
                    yield return Shot30(dir, "bend34-f2-plus0.2s");
                }
                if (!shotKill && fKill >= 0)
                {
                    shotKill = true;
                    yield return Shot30(dir, "bend34-f3-kill");
                }
            }
            g.forcedCombat = IdleCombat;
            g.forcedMove = null;
            // ---- the exit: the ease out, then f4 ----
            double outS = sim.bend34Rules != null ? sim.bend34Rules.outS : 0.3;
            double tOut = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - tOut < outS + 0.05 && Time.realtimeSinceStartup < wallEnd) { God(g); yield return null; }
            fExit = FeelProbe.FrameCount;
            yield return Shot30(dir, "bend34-f4-exit");
            yield return Frames(Ms(300));
            yield return Stop();

            double lenReal = tEnd > 0 ? tEnd - tLatch : double.NaN;
            bool swapsInstant = swapSteps.Count == 3 && swapSteps.TrueForAll(x => x == 0);
            bool shareOk = mode != "global" || Math.Abs(shareMin - (sim.bend34Rules != null ? sim.bend34Rules.scale : 0.25)) <= 0.02;
            bool ok = on && starts > 0 && swapsInstant && kills > 0 && ends > 0 && shareOk;
            res[0] = $"bend34 {(ok ? "PASS" : "RED")}: mode {mode}; target {targetKind}, latched {attached}; tier at the latch {tierAtLatch}, meter {meterBefore.ToString("0.00", ci)} -> {(double.IsNaN(meterAfter) ? "-" : meterAfter.ToString("0.00", ci))} (now {sim.Bend34Meter.ToString("0.00", ci)}); share min {shareMin.ToString("0.000", ci)}, depth max {depthMax.ToString("0.00", ci)}; swap steps [{string.Join(", ", swapSteps)}] (0 = the press step; NIGHT33 15); kills inside {kills}; bend real length {(double.IsNaN(lenReal) ? "-" : lenReal.ToString("0.00", ci))} s (to bend34End); steps inside {bendSteps}, world held on {heldWorld}; starts {starts}, ends {ends}; husks placed {husks.Count} | frames f{fLatch:0000} latch, f{fPlus:0000} +0.2 s, f{fKill:0000} kill, f{fExit:0000} exit; shots shot-N30-bend34-f1-latch / -f2-plus0.2s / -f3-kill / -f4-exit.png | {pc}";
        }
    }
}
