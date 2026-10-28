// NIGHT16 U3-29.6 (builder V owns this file; the shared commit's empty fixture): the hands (every gun at its fire-ready
// pose by the step the sim opens fire, a shot mid-draw cancels the flourish the same frame, the left arm's punch, the
// glint and weaponReady, no allocation per frame).
// The draw maths are pure (HandsDraw / HandsDrawState / HandsArm: no scene, no UnityEngine native call), so every test
// here but TheDrawFrameAllocatesNothing (Unity's AllocatingGCMemory constraint: the Editor only) also runs under dotnet.
// The fire step is derived here from Combat.StepSwap's arithmetic (total = StepsFor(swapTime / swapMult), lower =
// round(total x swapLowerFraction), fire from swapFireFraction via CombatMath.SwapCanFire), independently of the code
// under test. The centre-box check projects every triangle of each kit gun (BlockyKit's meshes, read from MeshKit's
// private lists by reflection: building a Mesh is native) through the viewmodel camera (viewmodel.fovDeg, 16:9).
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.TestTools.Constraints; // AllocatingGCMemory() is an extension method here
using AllocIs = UnityEngine.TestTools.Constraints.Is;
using Is = NUnit.Framework.Is;

namespace Skygrind.Game.Tests
{
    public class HandsTests
    {
        static readonly string[] KitGuns = { "revolver", "shotgun", "nailgun", "rocket", "rail", "gauntlet" };
        static readonly string[] Signatures = { "revolver", "shotgun", "rocket", "rail", "gauntlet" };
        const double TolM = 0.002, TolDeg = 0.5;
        GameConfig cfg;
        double dt, swapTime, lowerFraction, fireFraction, respawnRaise, quick;

        static string ProjectRoot()
        {
            // the Editor runs in the project root; an agent's dotnet runner from the worktree (or under it)
            foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
                for (var d = new DirectoryInfo(start); d != null; d = d.Parent)
                    if (File.Exists(Path.Combine(d.FullName, "PortExport", "config.json"))) return d.FullName;
            throw new FileNotFoundException("PortExport/config.json above " + Directory.GetCurrentDirectory());
        }

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(ProjectRoot(), "PortExport", "config.json"));
            dt = cfg.Dt;
            swapTime = cfg.Num("weapons.swapTime");
            lowerFraction = cfg.Num("weapons.swapLowerFraction");
            fireFraction = cfg.Num("weapons.swapFireFraction");
            respawnRaise = cfg.Num("viewmodel.respawnRaiseTime");
            quick = cfg.Num("perks.quick.swapMult");
        }

        // --- Combat.StepSwap's arithmetic, independently ----------------------------------------------------------------
        static void SimSwap(double swapTime, double swapMult, double lowerFraction, double fireFraction, double dt, out int total, out int lower, out int fire)
        {
            total = MovementMath.StepsFor(swapTime / swapMult, dt);
            lower = Math.Min(total - 1, Math.Max(1, (int)JsMath.Round(total * lowerFraction)));
            fire = 0;
            while (!CombatMath.SwapCanFire(fire, lower, total, fireFraction)) fire++;
        }

        static void AssertAtHold(HandsDrawPose p, string what)
        {
            Assert.That(Math.Abs(p.x), Is.LessThanOrEqualTo(TolM), what + " x");
            Assert.That(Math.Abs(p.y), Is.LessThanOrEqualTo(TolM), what + " y");
            Assert.That(Math.Abs(p.z), Is.LessThanOrEqualTo(TolM), what + " z");
            Assert.That(Math.Abs(p.pitchDeg), Is.LessThanOrEqualTo(TolDeg), what + " pitch");
            Assert.That(Math.Abs(p.yawDeg), Is.LessThanOrEqualTo(TolDeg), what + " yaw");
            Assert.That(Math.Abs(p.rollDeg), Is.LessThanOrEqualTo(TolDeg), what + " roll");
            Assert.That(p.clench, Is.LessThanOrEqualTo(0.01), what + " clench (the gauntlet's scale)");
        }

        static bool OffHold(HandsDrawPose p) =>
            Math.Abs(p.x) > TolM || Math.Abs(p.y) > TolM || Math.Abs(p.z) > TolM || Math.Abs(p.pitchDeg) > TolDeg || Math.Abs(p.yawDeg) > TolDeg
            || Math.Abs(p.rollDeg) > TolDeg || p.spinRad != 0 || p.pump != 0 || p.glow != 0 || p.clench != 0;

        [Test]
        public void TheTimelineIsCombatsStepSwapArithmetic()
        {
            foreach (double mult in new[] { 1.0, quick })
            {
                SimSwap(swapTime, mult, lowerFraction, fireFraction, dt, out int total, out int lower, out int fire);
                HandsSwapTimeline tl = HandsSwapTimeline.Of(swapTime, mult, lowerFraction, fireFraction, dt);
                Assert.That(tl.total, Is.EqualTo(total), $"total x{mult}");
                Assert.That(tl.lower, Is.EqualTo(lower), $"lower x{mult}");
                Assert.That(tl.fireStep, Is.EqualTo(fire), $"fire step x{mult}");
                Assert.That(tl.openU, Is.EqualTo((double)(fire - lower) / (total - lower)).Within(1e-12), $"openU x{mult}");
            }
            HandsSwapTimeline one = HandsSwapTimeline.Of(swapTime, 1, lowerFraction, fireFraction, dt);
            Assert.That(one.total, Is.EqualTo(30), "0.25 s at 120 Hz");
            Assert.That(one.lower, Is.EqualTo(15));
            Assert.That(one.fireStep, Is.EqualTo(26), "fire opens 11 steps after the gun changes");
        }

        /// <summary>The respawn / run-start raise is a draw too: up by viewmodel.respawnRaiseTime (no fire gate in the sim).</summary>
        [Test]
        public void TheRespawnRaiseIsADrawThatEndsAtHold()
        {
            HandsSwapTimeline tl = HandsSwapTimeline.Of(swapTime, 1, lowerFraction, fireFraction, dt);
            foreach (string gun in KitGuns)
            {
                HandsDrawSpec spec = HandsDraw.SpecFor(cfg, gun);
                var st = new HandsDrawState();
                st.Respawn();
                st.Frame(spec, tl, gun, 0, 0, 1, 0, respawnRaise, 1);
                Assert.That(st.started, Is.True, gun + ": the respawn starts a draw (weaponDraw)");
                Assert.That(st.pose.lowered, Is.GreaterThan(0.99), gun + ": down at the start");
                int steps = (int)Math.Round(respawnRaise / dt);
                for (int i = 0; i < steps; i++) st.Frame(spec, tl, gun, 0, 0, 1, dt, respawnRaise, 1);
                AssertAtHold(st.pose, gun + " at the end of the respawn raise");
            }
        }

        /// <summary>A draw starts once per gun change (the view emits weaponDraw then), not on every frame of it.</summary>
        [Test]
        public void ADrawStartsOncePerGunChange()
        {
            HandsSwapTimeline tl = HandsSwapTimeline.Of(swapTime, 1, lowerFraction, fireFraction, dt);
            HandsDrawSpec a = HandsDraw.SpecFor(cfg, "revolver"), b = HandsDraw.SpecFor(cfg, "rocket");
            var st = new HandsDrawState();
            st.Frame(a, tl, "revolver", 0, 0, 1, dt, respawnRaise, 1);
            int starts = 0;
            double prev = 0;
            for (int t = 1; t <= tl.total + 40; t++)
            {
                double now = CombatMath.SwapLowered(t, tl.lower, tl.total);
                bool changed = t >= tl.lower;
                st.Frame(changed ? b : a, tl, changed ? "rocket" : "revolver", prev, now, 1, dt, respawnRaise, 1);
                if (st.started) starts++;
                prev = now;
            }
            Assert.That(starts, Is.EqualTo(1));
        }

        /// <summary>The holster (the lowering half) tips toward the right hip: right, rolled clockwise, the muzzle swung right.</summary>
        [Test]
        public void TheHolsterTipsTowardTheHip()
        {
            foreach (string gun in KitGuns)
            {
                HandsDrawPose p = HandsDraw.Sample(HandsDraw.SpecFor(cfg, gun), 0.8, false, 0.7, double.PositiveInfinity, false, 1);
                Assert.That(p.phase, Is.EqualTo(HandsDrawPhase.Holster), gun);
                Assert.That(p.x, Is.GreaterThan(0), gun + " x (right)");
                Assert.That(p.rollDeg, Is.LessThan(0), gun + " roll (clockwise)");
                Assert.That(p.yawDeg, Is.LessThan(0), gun + " yaw (muzzle right)");
                Assert.That(p.y, Is.LessThan(-0.1), gun + " dropped");
            }
        }

        // --- the render loop's shape: a 120 Hz sim under 144 fps frames with the accumulator's alpha ---------------------
        sealed class Loop
        {
            public HandsDrawState st = new HandsDrawState();
            public HandsSwapTimeline tl;
            public HandsDrawSpec from, to;
            public string fromId, toId;
            public double acc, prev, now, frameDt, dt;
            public int t; // the swap step (0: not started)

            public void Frame(double respawnRaise, double motion)
            {
                acc += frameDt;
                while (acc >= dt)
                {
                    acc -= dt;
                    prev = now;
                    t++;
                    now = CombatMath.SwapLowered(t, tl.lower, tl.total);
                }
                // NIGHT17 round 1: the view is fed the loadout's target from the press (step 1), the swap active to its end
                bool changed = t >= 1;
                st.Frame(changed ? to : from, tl, changed ? toId : fromId, prev, now, acc / dt, frameDt, respawnRaise, motion, t >= 1 && t < tl.total);
            }
        }

        Loop SwapLoop(string from, string to, double mult = 1)
        {
            var l = new Loop
            {
                tl = HandsSwapTimeline.Of(swapTime, mult, lowerFraction, fireFraction, dt), from = HandsDraw.SpecFor(cfg, from), to = HandsDraw.SpecFor(cfg, to),
                fromId = from, toId = to, frameDt = 1 / UnityTuning.HandsTraceFps, dt = dt,
            };
            l.st.Frame(l.from, l.tl, from, 0, 0, 1, l.frameDt, respawnRaise, 1);
            return l;
        }

        /// <summary>After fire opens the flourish moves the gun (the signature lives in the settle); a shot then cancels it
        /// on the same frame: the next frame drawn is the hold pose, every part at rest.</summary>
        [Test]
        public void AShotMidDrawCancelsTheFlourishTheSameFrame()
        {
            foreach (string gun in KitGuns)
            {
                Loop l = SwapLoop(gun == "revolver" ? "shotgun" : "revolver", gun);
                bool flourished = false;
                for (int f = 0; f < 300 && !flourished; f++)
                {
                    l.Frame(respawnRaise, 1);
                    if (l.t >= l.tl.fireStep && l.st.settleAge > 0.02 && OffHold(l.st.pose)) flourished = true;
                }
                Assert.That(flourished, Is.True, gun + ": a flourish after fire opens");
                l.st.Cancel(); // the sim's `fire` (a step inside this frame)
                l.Frame(respawnRaise, 1);
                Assert.That(OffHold(l.st.pose), Is.False, gun + ": cancelled on the same frame");
                Assert.That(l.st.pose.spinRad, Is.EqualTo(0), gun + " spin");
                Assert.That(l.st.pose.pump, Is.EqualTo(0), gun + " pump");
                Assert.That(l.st.pose.glow, Is.EqualTo(0), gun + " glow");
                for (int f = 0; f < 60; f++)
                {
                    l.Frame(respawnRaise, 1);
                    Assert.That(OffHold(l.st.pose), Is.False, gun + ": stays cancelled");
                }
            }
        }

        /// <summary>A swap out started mid-flourish doesn't pop: the flourish decays on through the holster; and a flourish a
        /// shot cancelled stays cancelled through it (no revival).</summary>
        [Test]
        public void ASwapOutMidFlourishDoesNotPop()
        {
            foreach (bool shot in new[] { false, true })
                foreach (string gun in Signatures)
                {
                    string other = gun == "revolver" ? "shotgun" : "revolver";
                    Loop l = SwapLoop(other, gun);
                    while (!(l.t >= l.tl.total && l.st.settleAge > 0.03)) l.Frame(respawnRaise, 1); // into the flourish
                    if (shot)
                    {
                        l.st.Cancel();
                        l.Frame(respawnRaise, 1);
                    }
                    HandsDrawPose before = l.st.pose;
                    // the swap back: the sim lowers from the next step (the same timeline, mirrored roles)
                    var back = new Loop { st = l.st, tl = l.tl, from = l.to, to = l.from, fromId = gun, toId = other, frameDt = l.frameDt, dt = l.dt };
                    back.Frame(respawnRaise, 1);
                    while (back.t == 0) back.Frame(respawnRaise, 1);
                    HandsDrawPose after = back.st.pose;
                    Assert.That(after.phase, Is.EqualTo(HandsDrawPhase.Holster), gun);
                    // NIGHT17 U3-30.6: the holster yanks (an ease-out from the press), so its first frame moves as fast as a clean
                    // holster's fastest frame; a pop would be more than that (+ the flourish's own motion)
                    CleanSwapMaxStep(gun, other, out double cy, out double cp, out double cr);
                    Assert.That(Math.Abs(after.pitchDeg - before.pitchDeg), Is.LessThan(1.25 * cp + 1.5), $"{gun} pitch pop (shot {shot})");
                    Assert.That(Math.Abs(after.rollDeg - before.rollDeg), Is.LessThan(1.25 * cr + 1.5), $"{gun} roll pop (shot {shot})");
                    Assert.That(Math.Abs(after.y - before.y), Is.LessThan(1.25 * cy + 0.006), $"{gun} y pop (shot {shot})");
                    if (shot) Assert.That(after.spinRad + after.pump + after.glow + after.clench, Is.EqualTo(0), gun + ": no revival");
                }
        }

        // --- the quick-swap flick (FEEL-DESIGN 5 #6) -----------------------------------------------------------------------

        /// <summary>Combat.StepSwap, step for step (the swap timer, the mirror, the gun change at `lower`, the fire gate).</summary>
        sealed class SwapSim
        {
            public HandsSwapTimeline tl;
            public string[] owned;
            public int slot, target;
            public bool active;
            public double t, now, prev;
            public bool ready = true;

            public string Weapon => owned[slot];

            public void Step(int want)
            {
                prev = now;
                if (active)
                {
                    double before = t;
                    t += 1;
                    if (before < tl.lower && t >= tl.lower) slot = target;
                    if (t >= tl.total) active = false;
                }
                if (want >= 0 && want < owned.Length && want != target)
                {
                    if (!active)
                    {
                        active = true;
                        t = 0;
                    }
                    else if (t >= tl.lower || want == slot) t = CombatMath.SwapMirror(t, tl.lower, tl.total);
                    target = want;
                }
                now = active ? CombatMath.SwapLowered(t, tl.lower, tl.total) : 0;
                ready = !active || CombatMath.SwapCanFire(t, tl.lower, tl.total, 0.7);
            }
        }

        // --- the idle flourish (FEEL-DESIGN 5 #14) -------------------------------------------------------------------------

        [Test]
        public void TheIdleFlourishStartsAfterEightSecondsInABreakOnly()
        {
            double frameDt = 1 / UnityTuning.HandsTraceFps;
            var idle = new HandsIdle();
            int starts = 0;
            double idleS = 0;
            for (int f = 0; f < (int)(20 * UnityTuning.HandsTraceFps); f++)
            {
                idleS += frameDt;
                idle.Frame(idleS, true, frameDt);
                if (idle.started)
                {
                    starts++;
                    Assert.That(idleS, Is.GreaterThanOrEqualTo(UnityTuning.HandsIdleAfter * starts - 1e-9), "not before 8 s per flourish");
                }
            }
            Assert.That(starts, Is.EqualTo(2), "at 8 s and 16 s of a 20 s idle: at most once per 8 s");
            var round = new HandsIdle();
            idleS = 0;
            for (int f = 0; f < (int)(20 * UnityTuning.HandsTraceFps); f++)
            {
                idleS += frameDt;
                round.Frame(idleS, false, frameDt);
                Assert.That(round.started, Is.False, "never in a round");
            }
        }

        [Test]
        public void InputTheRoundOrAShotCancelsTheIdleOnTheSameFrame()
        {
            double frameDt = 1 / UnityTuning.HandsTraceFps;
            HandsDrawSpec kestrel = HandsDraw.SpecFor(cfg, "revolver");
            foreach (int how in new[] { 0, 1, 2 })
            {
                var idle = new HandsIdle();
                double idleS = UnityTuning.HandsIdleAfter - 0.01;
                idle.Frame(idleS, true, frameDt);
                for (int f = 0; f < 30; f++) idle.Frame(idleS += frameDt, true, frameDt);
                Assert.That(OffHold(HandsIdle.Sample(kestrel, idle.age, 1)), Is.True, "mid-flourish");
                if (how == 0) idle.Frame(0, true, frameDt); // input: the idle clock drops to 0
                else if (how == 1) idle.Frame(idleS + frameDt, false, frameDt); // the round starts
                else
                {
                    idle.Cancel(); // a shot
                    idle.Frame(idleS + frameDt, true, frameDt);
                }
                Assert.That(OffHold(HandsIdle.Sample(kestrel, idle.age, 1)), Is.False, $"cancelled on the same frame ({how})");
            }
        }

        [Test]
        public void TheIdleFlourishesDoTheirThingAndStayOutOfTheCentreBox()
        {
            double peakSpin = 0, peakPump = 0, shift = 0;
            foreach (string gun in KitGuns)
            {
                HandsDrawSpec s = HandsDraw.SpecFor(cfg, gun);
                List<Vector3> tris = RigTriangles(gun);
                double end = 0;
                for (double a = 0; a < 2; a += 1 / UnityTuning.HandsTraceFps)
                {
                    HandsDrawPose p = HandsIdle.Sample(s, a, 1);
                    if (gun == "revolver") peakSpin = Math.Max(peakSpin, p.spinRad);
                    if (gun == "shotgun") peakPump = Math.Max(peakPump, p.pump);
                    if (gun == "rocket") shift = Math.Max(shift, Math.Abs(p.rollDeg));
                    if (OffHold(p)) end = a;
                    Assert.That(InBox(gun, tris, p), Is.EqualTo(0), $"{gun} idle at {a:0.000}");
                }
                Assert.That(end, Is.GreaterThan(0.2), gun + ": it moves");
                Assert.That(end, Is.LessThan(1.5), gun + ": short");
                AssertAtHold(HandsIdle.Sample(s, double.PositiveInfinity, 1), gun + " at rest");
            }
            Assert.That(peakSpin, Is.GreaterThan(Math.PI), "the KESTREL's cylinder turns");
            Assert.That(HandsIdle.Sample(HandsDraw.SpecFor(cfg, "revolver"), 1.9, 1).spinRad, Is.EqualTo(0), "a whole turn: back on its chamber");
            Assert.That(peakPump, Is.EqualTo(0.5).Within(0.05), "the THUNDERCLAP's pump: half a rack");
            Assert.That(shift, Is.GreaterThan(1), "a weight-class shift");
        }

        /// <summary>Each of the five signatures does its own thing in the settle (not just the class spring).</summary>
        [Test]
        public void TheFiveSignaturesShowTheirParts()
        {
            double peakSpin = 0, peakPump = 0, peakSweepGlow = 0, peakClench = 0, peakCometRoll = 0, peakKestrelPitch = 0;
            foreach (string gun in Signatures)
            {
                HandsDrawSpec s = HandsDraw.SpecFor(cfg, gun);
                for (double a = 0; a < s.life; a += 0.005)
                {
                    HandsDrawPose p = HandsDraw.Sample(s, 0, true, 0.7, a, false, 1);
                    switch (gun)
                    {
                        case "revolver": peakSpin = Math.Max(peakSpin, p.spinRad); peakKestrelPitch = Math.Max(peakKestrelPitch, p.pitchDeg); break;
                        case "shotgun": peakPump = Math.Max(peakPump, p.pump); break;
                        case "rail": peakSweepGlow = Math.Max(peakSweepGlow, p.glow); break;
                        case "gauntlet": peakClench = Math.Max(peakClench, p.clench); break;
                        case "rocket": peakCometRoll = Math.Max(peakCometRoll, Math.Abs(p.rollDeg)); break;
                    }
                }
            }
            Assert.That(peakSpin, Is.GreaterThan(Math.PI), "the KESTREL's cylinder spins");
            Assert.That(peakKestrelPitch, Is.GreaterThan(2), "the KESTREL flicks up");
            Assert.That(peakPump, Is.GreaterThan(0.9), "the THUNDERCLAP racks");
            Assert.That(peakSweepGlow, Is.GreaterThan(0.9), "the LONGWAVE's charge sweeps");
            Assert.That(peakClench, Is.GreaterThan(0.9), "the GAUNTLET clenches");
            Assert.That(peakCometRoll, Is.GreaterThan(1), "the COMET seats on the shoulder");
            Assert.That(HandsDraw.SpecFor(cfg, "nailgun").kind, Is.EqualTo(HandsDrawKind.Generic), "the STITCHER by its weight class");
            Assert.That(HandsDraw.SpecFor(cfg, "rattler").kind, Is.EqualTo(HandsDrawKind.Generic), "a rig-less gun by its class");
        }

        /// <summary>REDUCED MOTION damps the flourishes that move the gun (x UnityTuning.HandsReducedMotionScale).</summary>
        [Test]
        public void ReducedMotionDampsTheFlourish()
        {
            HandsDrawSpec s = HandsDraw.SpecFor(cfg, "revolver");
            double full = 0, calm = 0;
            for (double a = 0; a < s.life; a += 0.002)
            {
                full = Math.Max(full, HandsDraw.Sample(s, 0, true, 0.7, a, false, 1).pitchDeg);
                calm = Math.Max(calm, HandsDraw.Sample(s, 0, true, 0.7, a, false, UnityTuning.HandsReducedMotionScale).pitchDeg);
            }
            Assert.That(calm, Is.LessThan(full * 0.6));
        }

        // --- the centre box (the web's NIGHT5 rule): no viewmodel pixel in the centred 10 % x 10 % on any draw frame -------

        static readonly FieldInfo KitV = typeof(MeshKit).GetField("v", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo KitT = typeof(MeshKit).GetField("t", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>A rig's triangles in web rig space (Viewmodel.Build*: the body and its parts at their places).</summary>
        static List<Vector3> RigTriangles(string gun)
        {
            var parts = new List<(MeshKit kit, Vector3 at)>();
            var body = new MeshKit();
            var a = new MeshKit();
            var b = new MeshKit();
            switch (gun)
            {
                case "revolver": BlockyKit.Kestrel(body, a, Color.white); parts.Add((a, new Vector3(0, 0.032f, 0.062f))); break;
                case "shotgun": BlockyKit.Thunderclap(body, Color.white, a); parts.Add((a, BlockyKit.ThunderclapPumpAt)); break;
                case "gauntlet": BlockyKit.Gauntlet(body, a, Color.white); parts.Add((a, Vector3.zero)); break;
                case "nailgun": BlockyKit.Stitcher(body, Color.white, a, b); parts.Add((a, BlockyKit.StitcherDrumAt)); parts.Add((b, Vector3.zero)); break;
                case "rocket": BlockyKit.Comet(body, Color.white, a); parts.Add((a, BlockyKit.CometRingAt)); break;
                case "rail": BlockyKit.Longwave(body, Color.white, a, b); parts.Add((a, Vector3.zero)); parts.Add((b, BlockyKit.LongwaveRingAt)); break;
            }
            parts.Add((body, Vector3.zero));
            var tris = new List<Vector3>();
            foreach (var (kit, at) in parts)
            {
                var v = (List<Vector3>)KitV.GetValue(kit);
                var t = (List<int>)KitT.GetValue(kit);
                foreach (int i in t)
                {
                    Vector3 u = v[i] + at;
                    tris.Add(new Vector3(u.x, u.y, -u.z)); // Unity rig space -> web
                }
            }
            return tris;
        }

        /// <summary>three's Euler (radians) as a 3x3 matrix, order "XYZ" (M = Rx Ry Rz) or "YXZ" (M = Ry Rx Rz).</summary>
        static double[,] Euler(double x, double y, double z, bool yxz)
        {
            double cx = Math.Cos(x), sx = Math.Sin(x), cy = Math.Cos(y), sy = Math.Sin(y), cz = Math.Cos(z), sz = Math.Sin(z);
            var rx = new[,] { { 1, 0, 0 }, { 0, cx, -sx }, { 0, sx, cx } };
            var ry = new[,] { { cy, 0, sy }, { 0, 1, 0 }, { -sy, 0, cy } };
            var rz = new[,] { { cz, -sz, 0 }, { sz, cz, 0 }, { 0, 0, 1 } };
            return yxz ? Mul(Mul(ry, rx), rz) : Mul(Mul(rx, ry), rz);
        }

        static double[,] Mul(double[,] a, double[,] b)
        {
            var m = new double[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++) m[i, j] += a[i, k] * b[k, j];
            return m;
        }

        static void Apply(double[,] m, double x, double y, double z, out double ox, out double oy, out double oz)
        {
            ox = m[0, 0] * x + m[0, 1] * y + m[0, 2] * z;
            oy = m[1, 0] * x + m[1, 1] * y + m[1, 2] * z;
            oz = m[2, 0] * x + m[2, 1] * y + m[2, 2] * z;
        }

        const double Deg = Math.PI / 180, Box = 0.1; // the centred 10 % x 10 %: NDC -0.1 .. 0.1 on both axes

        /// <summary>Does the triangle (NDC) overlap the centred box? (separating axes: the box's two, the triangle's three)</summary>
        static bool Overlaps(double[] px, double[] py)
        {
            if (Math.Max(px[0], Math.Max(px[1], px[2])) < -Box || Math.Min(px[0], Math.Min(px[1], px[2])) > Box) return false;
            if (Math.Max(py[0], Math.Max(py[1], py[2])) < -Box || Math.Min(py[0], Math.Min(py[1], py[2])) > Box) return false;
            for (int e = 0; e < 3; e++)
            {
                int a = e, b = (e + 1) % 3, c = (e + 2) % 3;
                double nx = py[b] - py[a], ny = px[a] - px[b];
                double tri = nx * px[a] + ny * py[a], third = nx * px[c] + ny * py[c];
                double r = Box * (Math.Abs(nx) + Math.Abs(ny)); // the box's extent on the axis (centred at 0)
                double lo = Math.Min(tri, third), hi = Math.Max(tri, third);
                if (hi < -r || lo > r) return false; // the box lies wholly outside the triangle's span on this axis
            }
            return true;
        }

        /// <summary>Counts triangles of `tris` (web rig space) inside the centre box with the rig at the gun's kit offset and
        /// the root at the hold pose + the draw pose; also the nearest NDC distance to the box (for the message).</summary>
        int InBox(string gun, List<Vector3> tris, HandsDrawPose p)
        {
            double holdX = cfg.Num("viewmodel.holdX"), holdY = cfg.Num("viewmodel.holdY"), holdZ = cfg.Num("viewmodel.holdZ");
            double holdYaw = cfg.Num("viewmodel.holdYawDeg"), holdPitch = cfg.Num("viewmodel.holdPitchDeg");
            string ko = "viewmodel.kitOffsets." + gun;
            double kx = 0, ky = 0, kz = 0, kp = 0, kyaw = 0, kr = 0, ks = 1;
            if (cfg.Has(ko))
            {
                kx = cfg.Num(ko + ".x");
                ky = cfg.Num(ko + ".y");
                kz = cfg.Num(ko + ".z");
                kp = cfg.Has(ko + ".pitchDeg") ? cfg.Num(ko + ".pitchDeg") : 0;
                kyaw = cfg.Has(ko + ".yawDeg") ? cfg.Num(ko + ".yawDeg") : 0;
                kr = cfg.Has(ko + ".rollDeg") ? cfg.Num(ko + ".rollDeg") : 0;
                ks = cfg.Has(ko + ".scale") ? cfg.Num(ko + ".scale") : 1;
            }
            HandsDraw.HoldNudge(gun, out double ndx, out double ndy); // NIGHT17 D-69 (the GAUNTLET raised off the card)
            kx += ndx;
            ky += ndy;
            ks *= HandsDraw.Presence(gun); // round 3: the presence scale
            HandsDraw.HoldTurn(gun, out double turnP, out double turnY); // round 6: the THUNDERCLAP turned out of the centre
            kp += turnP;
            kyaw += turnY;
            double[,] kit = Euler(kp * Deg, kyaw * Deg, kr * Deg, false);
            double[,] root = Euler((holdPitch + p.pitchDeg) * Deg, (holdYaw + p.yawDeg) * Deg, p.rollDeg * Deg, true);
            double squash = 1 - UnityTuning.HandsGauntletClenchSquash * p.clench;
            double tanY = Math.Tan(cfg.Num("viewmodel.fovDeg") * Deg / 2), tanX = tanY * 16 / 9;
            var nx = new double[3];
            var ny = new double[3];
            int hits = 0;
            for (int i = 0; i + 2 < tris.Count; i += 3)
            {
                bool front = true;
                for (int k = 0; k < 3; k++)
                {
                    Vector3 v = tris[i + k];
                    Apply(kit, v.x * ks * squash, v.y * ks * squash, v.z * ks, out double ax, out double ay, out double az);
                    Apply(root, ax + kx, ay + ky, az + kz, out double cx, out double cy, out double cz);
                    cx += holdX + p.x;
                    cy += holdY + p.y;
                    cz += holdZ + p.z;
                    if (cz > -0.02) front = false; // behind the viewmodel camera's near plane (viewmodel.near)
                    nx[k] = cx / -cz / tanX;
                    ny[k] = cy / -cz / tanY;
                }
                if (front && Overlaps(nx, ny)) hits++;
            }
            return hits;
        }

        [Test]
        public void NoDrawFrameEntersTheCentreBox()
        {
            foreach (string gun in KitGuns)
            {
                List<Vector3> tris = RigTriangles(gun);
                Assert.That(tris.Count, Is.GreaterThan(30), gun + " mesh");
                // the control: the projection is live (the same gun moved to the middle of the view lands in the box)
                Assert.That(InBox(gun, tris, new HandsDrawPose { x = -cfg.Num("viewmodel.holdX"), y = -cfg.Num("viewmodel.holdY") }), Is.GreaterThan(0), gun + " control");
                // the swap into this gun (its raise and settle) and out of it (its holster), at 144 fps, then a respawn
                foreach (bool into in new[] { true, false })
                {
                    string other = gun == "revolver" ? "shotgun" : "revolver";
                    Loop l = into ? SwapLoop(other, gun) : SwapLoop(gun, other);
                    int frames = (int)((l.tl.total * dt + UnityTuning.HandsTraceAfterSeconds) * UnityTuning.HandsTraceFps);
                    for (int f = 0; f < frames; f++)
                    {
                        l.Frame(respawnRaise, 1);
                        bool gunInHand = l.st.held == gun; // the gun shown (NIGHT17: from the flick's end)
                        if (!gunInHand) continue;
                        int hits = InBox(gun, tris, l.st.pose);
                        Assert.That(hits, Is.EqualTo(0), $"{gun} {(into ? "draw" : "holster")} frame {f} (step {l.t}, phase {l.st.pose.phase})");
                    }
                }
                var st = new HandsDrawState();
                HandsDrawSpec spec = HandsDraw.SpecFor(cfg, gun);
                HandsSwapTimeline tl = HandsSwapTimeline.Of(swapTime, 1, lowerFraction, fireFraction, dt);
                st.Respawn();
                for (int f = 0; f < 150; f++)
                {
                    st.Frame(spec, tl, gun, 0, 0, 1, 1 / UnityTuning.HandsTraceFps, respawnRaise, 1);
                    Assert.That(InBox(gun, tris, st.pose), Is.EqualTo(0), $"{gun} respawn frame {f}");
                }
            }
        }

        [Test]
        public void TheLeftArmPunchStaysOutOfTheCentreBox()
        {
            HandsArmParams a = HandsArmParams.From(cfg);
            var kit = new MeshKit();
            BlockyKit.Arm(kit, a.parts, Color.gray, Color.gray, Color.cyan, a.segments);
            BlockyKit.ArmKnuckles(kit, a.parts[0], Color.white);
            var v = (List<Vector3>)KitV.GetValue(kit);
            var t = (List<int>)KitT.GetValue(kit);
            double tanY = Math.Tan(cfg.Num("viewmodel.fovDeg") * Deg / 2), tanX = tanY * 16 / 9;
            var nx = new double[3];
            var ny = new double[3];
            for (double age = 0; age < a.cycleTime; age += 1 / UnityTuning.HandsTraceFps)
            {
                HandsArmPose p = HandsArm.Pose(a, age, 0, 0);
                if (!p.visible) continue;
                double[,] m = Euler(p.pitchDeg * Deg, p.yawDeg * Deg, p.rollDeg * Deg, true);
                for (int i = 0; i + 2 < t.Count; i += 3)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        Vector3 u = v[t[i + k]];
                        Apply(m, u.x, u.y, -u.z, out double cx, out double cy, out double cz);
                        cx += p.x;
                        cy += p.y;
                        cz += p.z;
                        nx[k] = cx / -cz / tanX;
                        ny[k] = cy / -cz / tanY;
                    }
                    Assert.That(Overlaps(nx, ny), Is.False, $"the arm at {age:0.000} s");
                }
            }
        }

        // --- the left arm (viewmodel.js :925-928, poseArm :682-689) ------------------------------------------------------

        [Test]
        public void TheLeftArmFollowsTheWebsPunchEnvelope()
        {
            HandsArmParams a = HandsArmParams.From(cfg);
            Assert.That(a.restX, Is.EqualTo(cfg.Num("viewmodel.armRest.x")));
            Assert.That(a.punchZ, Is.EqualTo(cfg.Num("viewmodel.armPunch.z")));
            Assert.That(a.cycleTime, Is.EqualTo(cfg.Num("punch.cycleTime")));
            Assert.That(a.parts.Length, Is.EqualTo(cfg.Arr("viewmodel.arm").Count));
            Assert.That(HandsArm.Pose(a, double.PositiveInfinity, 0, 0).visible, Is.False, "at rest: hidden (arm.visible = punchW > 0)");
            Assert.That(HandsArm.Pose(a, 0, 0, 0).visible, Is.False, "envelope(0) = 0");
            HandsArmPose o = HandsArm.Pose(a, a.outTime, 0.01, -0.02);
            Assert.That(o.visible, Is.True);
            Assert.That(o.w, Is.EqualTo(1).Within(1e-12), "out: the full jab");
            Assert.That(o.x, Is.EqualTo(a.punchX + 0.01).Within(1e-12), "x + the sway and bob");
            Assert.That(o.y, Is.EqualTo(a.punchY - 0.02).Within(1e-12), "y + the sway and bob");
            Assert.That(o.z, Is.EqualTo(a.punchZ).Within(1e-12));
            Assert.That(o.rollDeg, Is.EqualTo(a.punchRoll).Within(1e-12));
            HandsArmPose half = HandsArm.Pose(a, a.outTime / 2, 0, 0);
            double w = Math.Sin(Math.PI / 2 * 0.5);
            Assert.That(half.w, Is.EqualTo(w).Within(1e-12), "the sine rise");
            Assert.That(half.pitchDeg, Is.EqualTo(a.restPitch + (a.punchPitch - a.restPitch) * w).Within(1e-12), "rest + (punch - rest) x jab");
            Assert.That(HandsArm.Pose(a, a.cycleTime, 0, 0).visible, Is.False, "gone by the punch's cycle");
        }

        /// <summary>Build 1's strips: a punch kill / parry freezes the world on the punch's own step. The fist's OUT travel
        /// runs on frame time then, so it is fully out by frame 9 (60 ms at 144 fps) and holds there through the freeze.</summary>
        [Test]
        public void TheFistReachesFullExtensionInsideAFreezeAndHolds()
        {
            HandsArmParams a = HandsArmParams.From(cfg);
            double frameDt = 1 / UnityTuning.HandsTraceFps;
            double age = 0; // the punch event (its step) set it
            for (int f = 1; f <= 20; f++)
            {
                age = HandsArm.AdvanceOut(age, 0, frameDt, a.outTime); // the world frozen
                HandsArmPose p = HandsArm.Pose(a, age, 0, 0);
                if (f >= 9)
                {
                    Assert.That(p.visible, Is.True, "frame " + f);
                    Assert.That(p.w, Is.EqualTo(1).Within(1e-12), $"frame {f}: fully out");
                    Assert.That(p.z, Is.EqualTo(a.punchZ).Within(1e-12), $"frame {f}: at armPunch");
                }
                else Assert.That(p.visible, Is.True, $"frame {f}: on its way out");
            }
            Assert.That(age, Is.EqualTo(a.outTime), "held at the end of the out travel while frozen");
            // the world running again: the hold and the return on world time, the web's envelope
            double back = HandsArm.AdvanceOut(age, a.holdTime + 0.01, frameDt, a.outTime);
            Assert.That(HandsArm.Pose(a, back, 0, 0).w, Is.LessThan(1), "returning after the freeze");
            // worldDt = frameDt: exactly the web's clock (punchAge += animDt)
            double web = 0, port = 0;
            for (int f = 0; f < 60; f++)
            {
                web += frameDt;
                port = HandsArm.AdvanceOut(port, frameDt, frameDt, a.outTime);
                Assert.That(port, Is.EqualTo(web), "frame " + f);
            }
            // a slow-mo (world at 0.3 x): the out travel at frame speed, then world time
            double s = HandsArm.AdvanceOut(0, 0.3 * frameDt, frameDt, a.outTime);
            Assert.That(s, Is.EqualTo(frameDt).Within(1e-15));
            // the gauntlet's jab rides the same clock (viewmodel.swing.gauntlet.out)
            double jab = 0, jabOut = cfg.Num("viewmodel.swing.gauntlet.out");
            for (int f = 0; f < 40; f++) jab = HandsArm.AdvanceOut(jab, 0, frameDt, jabOut);
            Assert.That(jab, Is.EqualTo(jabOut), "the gauntlet's jab: out and held in the freeze");
        }

        // --- the reviewer's findings ---------------------------------------------------------------------------------------

        /// <summary>A shot's own freeze: the recoil's attack lands inside it (frame time to the newest kick's peak) and holds;
        /// the world running: exactly the bank on world time, as before.</summary>
        [Test]
        public void TheRecoilKicksInsideItsOwnShotsFreezeAndHolds()
        {
            double frameDt = 1 / UnityTuning.HandsTraceFps;
            SpringShape heavy = SpringShape.Of(cfg.Num("viewmodel.recoilSprings.heavy.freqHz"), cfg.Num("viewmodel.recoilSprings.heavy.zeta"));
            double kick = cfg.Num("viewmodel.recoil.rocket.kickDeg");
            var bank = new SpringBank(cfg.Int("viewmodel.recoilSlots"), cfg.Nums("viewmodel.recoilSnap"));
            var clock = new HandsRecoilClock();
            bank.Add(heavy, kick, 0, 0, 0);
            clock.Kick(heavy.tPeak);
            int peakFrame = (int)Math.Ceiling(heavy.tPeak / frameDt);
            for (int f = 1; f <= 40; f++)
            {
                bank.Update(clock.Step(0, frameDt)); // the world frozen
                if (f >= peakFrame) Assert.That(bank.Sample(0), Is.EqualTo(kick).Within(1e-9), $"frame {f}: at the kick's peak, held");
                else Assert.That(bank.Sample(0), Is.GreaterThan(0), $"frame {f}: rising inside the freeze");
            }
            // the world runs again: the return on world time
            bank.Update(clock.Step(frameDt, frameDt));
            Assert.That(bank.Sample(0), Is.LessThan(kick), "returning after the freeze");
            // the world running all along: the same bank as before (dt = worldDt)
            var a = new SpringBank(cfg.Int("viewmodel.recoilSlots"), cfg.Nums("viewmodel.recoilSnap"));
            var b = new SpringBank(cfg.Int("viewmodel.recoilSlots"), cfg.Nums("viewmodel.recoilSnap"));
            var c2 = new HandsRecoilClock();
            a.Add(heavy, kick, 0, 0, 0);
            b.Add(heavy, kick, 0, 0, 0);
            c2.Kick(heavy.tPeak);
            for (int f = 0; f < 60; f++)
            {
                a.Update(frameDt);
                b.Update(c2.Step(frameDt, frameDt));
                Assert.That(b.Sample(0), Is.EqualTo(a.Sample(0)), "frame " + f);
            }
            Assert.That(new HandsRecoilClock().Step(0.004, frameDt), Is.EqualTo(0.004), "no kick yet: world time");
        }

        /// <summary>The knuckles ask the game's one FlashGate when the fist is out; a gauntlet parry (no fist) spends nothing;
        /// FLASH INTENSITY 0: no flash, no slot.</summary>
        [Test]
        public void TheKnucklesUseTheGamesFlashGateOnlyForAFistOnScreen()
        {
            HandsArmParams a = HandsArmParams.From(cfg);
            double frameDt = 1 / UnityTuning.HandsTraceFps;
            var gate = new FlashGate();
            var k = new HandsKnuckles();
            for (int i = 0; i < 5; i++)
            {
                k.Parry(false, 1); // a gauntlet parry: no punch out
                k.Frame(double.PositiveInfinity, a.outTime, a.cycleTime, frameDt, gate, i * 0.01);
                Assert.That(k.pending, Is.False, "dropped");
                Assert.That(k.Value(UnityTuning.HandsKnuckleFlashTime), Is.EqualTo(0));
            }
            k.Parry(false, 0); // FLASH INTENSITY 0
            k.Frame(a.outTime, a.outTime, a.cycleTime, frameDt, gate, 0.06);
            Assert.That(k.Value(UnityTuning.HandsKnuckleFlashTime), Is.EqualTo(0), "intensity 0: none");
            Assert.That(gate.TryFlash(0.1) && gate.TryFlash(0.11) && gate.TryFlash(0.12), Is.True, "no slot spent so far: three left");
            var g2 = new FlashGate();
            var p = new HandsKnuckles();
            p.Parry(true, 0.5);
            p.Frame(0.02, a.outTime, a.cycleTime, frameDt, g2, 0); // on its way out: waits
            Assert.That(p.pending, Is.True);
            p.Frame(a.outTime, a.outTime, a.cycleTime, 0, g2, 0.01); // out (in the freeze: worldDt 0)
            Assert.That(p.Value(UnityTuning.HandsKnuckleFlashTime), Is.EqualTo(0.5).Within(1e-12), "lit, x flashIntensity");
            for (int f = 0; f < 30; f++) p.Frame(a.outTime, a.outTime, a.cycleTime, 0, g2, 0.02);
            Assert.That(p.Value(UnityTuning.HandsKnuckleFlashTime), Is.EqualTo(0.5).Within(1e-12), "held through the freeze");
            Assert.That(g2.TryFlash(0.03) && g2.TryFlash(0.04), Is.True);
            Assert.That(g2.TryFlash(0.05), Is.False, "the knuckles took one of the three");
            var g3 = new FlashGate();
            g3.TryFlash(0);
            g3.TryFlash(0);
            g3.TryFlash(0);
            var full = new HandsKnuckles();
            full.Parry(false, 1);
            full.Frame(a.outTime, a.outTime, a.cycleTime, frameDt, g3, 0.5);
            Assert.That(full.Value(UnityTuning.HandsKnuckleFlashTime), Is.EqualTo(0), "the gate full: no flash");
        }

        /// <summary>A draw's glow is a flash: x flashIntensity (0: none, no slot), and it asks the game's gate once per draw.</summary>
        [Test]
        public void TheDrawGlowFollowsFlashIntensityAndTheGamesGate()
        {
            var gate = new FlashGate();
            var g = new HandsGlowGate();
            Assert.That(g.Frame(true, 0, 1, gate, 0), Is.EqualTo(0));
            Assert.That(g.Frame(false, 0.8, 0.5, gate, 0.01), Is.EqualTo(0.4).Within(1e-12), "x flashIntensity");
            Assert.That(g.Frame(false, 0.6, 0.5, gate, 0.02), Is.EqualTo(0.3).Within(1e-12), "the same draw: asked once");
            Assert.That(gate.TryFlash(0.03) && gate.TryFlash(0.04), Is.True);
            Assert.That(gate.TryFlash(0.05), Is.False, "the draw's glow took one slot");
            var none = new HandsGlowGate();
            var g2 = new FlashGate();
            none.Frame(true, 0, 0, g2, 0);
            Assert.That(none.Frame(false, 1, 0, g2, 0.01), Is.EqualTo(0), "FLASH INTENSITY 0: no glow");
            Assert.That(g2.TryFlash(0.02) && g2.TryFlash(0.03) && g2.TryFlash(0.04), Is.True, "and no slot spent");
            var refused = new HandsGlowGate();
            refused.Frame(true, 0, 1, g2, 0.05);
            Assert.That(refused.Frame(false, 1, 1, g2, 0.06), Is.EqualTo(0), "the gate full: no glow this draw");
            Assert.That(refused.Frame(true, 0, 1, g2, 2.0), Is.EqualTo(0), "a new draw");
            Assert.That(refused.Frame(false, 1, 1, g2, 2.01), Is.EqualTo(1), "asks again");
        }

        /// <summary>REDUCED MOTION / effects intensity damp the KESTREL's spin (whole chambers) and the THUNDERCLAP's roll.</summary>
        [Test]
        public void ReducedMotionDampsTheSpinAndThePumpRoll()
        {
            HandsDrawSpec k = HandsDraw.SpecFor(cfg, "revolver"), s = HandsDraw.SpecFor(cfg, "shotgun");
            double chamber = cfg.Num("viewmodel.cylinderStepDeg") * Deg;
            foreach (double motion in new[] { 1.0, UnityTuning.HandsReducedMotionScale, 0.0 })
            {
                double spin = 0, roll = 0, idleSpin = 0;
                for (double a = 0; a < 2; a += 0.002)
                {
                    spin = Math.Max(spin, HandsDraw.Sample(k, 0, true, 0.7, a, false, motion).spinRad);
                    idleSpin = Math.Max(idleSpin, HandsIdle.Sample(k, a, motion).spinRad);
                    roll = Math.Max(roll, Math.Abs(HandsDraw.Sample(s, 0, true, 0.7, a, false, motion).rollDeg));
                }
                double turns = Math.Round(2 * Math.PI * UnityTuning.HandsKestrelSpinTurns * motion / chamber) * chamber;
                Assert.That(spin, Is.LessThanOrEqualTo(turns + 1e-9), $"spin x{motion}: whole chambers of the scaled turn");
                Assert.That(spin, Is.GreaterThanOrEqualTo(turns * 0.95 - 1e-9), $"spin x{motion}");
                Assert.That(idleSpin, Is.LessThanOrEqualTo(Math.Round(2 * Math.PI * motion / chamber) * chamber + 1e-9), $"idle spin x{motion}");
                Assert.That(roll, Is.LessThanOrEqualTo(s.pumpTiltDeg * motion + 1e-9), $"the rack's roll x{motion}");
                Assert.That(HandsDraw.Sample(k, 0, true, 0.7, 1.9, false, motion).spinRad, Is.EqualTo(0), "back on a chamber at the end");
            }
        }

        // --- the momentum hit's hold spring (viewmodel.js :654-670, :740, :875-878, :956) ---------------------------------

        HitEvent MomentumHit(int shot, double momentum, bool melee = true, string owner = "player") =>
            new HitEvent { owner = owner, weapon = melee ? "gauntlet" : "revolver", melee = melee, momentum = momentum, shot = shot };

        [Test]
        public void AMomentumMeleeHitDrivesTheGunInOncePerSwing()
        {
            double at = cfg.Num("feel.momentumHitstopAt");
            var h = new HandsImpactHold(cfg);
            Assert.That(h.Active, Is.False);
            Assert.That(h.OnHit(MomentumHit(7, at + 0.3)), Is.True, "a momentum melee hit");
            Assert.That(h.OnHit(MomentumHit(7, at + 0.3)), Is.False, "the same swing's second hit: once per shot");
            Assert.That(h.OnHit(MomentumHit(8, at + 0.3)), Is.True, "the next swing");
            Assert.That(h.OnHit(MomentumHit(9, at)), Is.False, "at the threshold: not over it");
            Assert.That(h.OnHit(MomentumHit(10, at - 0.2)), Is.False, "below it");
            Assert.That(h.OnHit(MomentumHit(11, at + 1, melee: false)), Is.False, "a gun hit");
            Assert.That(h.OnHit(MomentumHit(12, at + 1, owner: "rival")), Is.False, "not mine");
            h.Clear();
            Assert.That(h.Active, Is.False, "respawn clears it");
        }

        [Test]
        public void TheMomentumHoldRunsOnFrameTimeToTheWebsPeaks()
        {
            var h = new HandsImpactHold(cfg);
            double[] peaks = cfg.Nums("viewmodel.impactHold.peaks");
            SpringShape shape = SpringShape.Of(cfg.Num("viewmodel.impactHold.freqHz"), cfg.Num("viewmodel.impactHold.zeta"));
            h.OnHit(MomentumHit(1, 2));
            Assert.That(h.Value(0), Is.EqualTo(0), "a kick starts at 0");
            // the world frozen (the momentum hit's own hitstop): the view advances it on frame time, so it still moves
            double frameDt = 1 / UnityTuning.HandsTraceFps, t = 0, prev = 0;
            while (t + frameDt <= shape.tPeak)
            {
                h.Advance(frameDt);
                t += frameDt;
                Assert.That(h.Value(0), Is.LessThan(prev), "driving in (pitch down) at " + t);
                prev = h.Value(0);
            }
            h.Advance(shape.tPeak - t);
            for (int c = 0; c < 4; c++) Assert.That(h.Value(c), Is.EqualTo(peaks[c]).Within(Math.Abs(peaks[c]) * 1e-9), "the first peak = impactHold.peaks[" + c + "]");
            Assert.That(h.Active, Is.True);
            for (int f = 0; f < 600; f++) h.Advance(frameDt);
            Assert.That(h.Active, Is.False, "settled under recoilSnap: dropped");
            Assert.That(h.Value(0), Is.EqualTo(0));
        }

        // --- the forge flourish (viewmodel.js :658-663) ---------------------------------------------------------------------

        [Test]
        public void TheForgeFlourishKicksTheGunInHandOnce()
        {
            var f = new HandsForgeFlourish(cfg);
            double[] flourish = cfg.Nums("forge.flourish");
            SpringShape medium = SpringShape.Of(cfg.Num("viewmodel.recoilSprings.medium.freqHz"), cfg.Num("viewmodel.recoilSprings.medium.zeta"));
            var bank = new SpringBank(cfg.Int("viewmodel.recoilSlots"), cfg.Nums("viewmodel.recoilSnap"));
            var clock = new HandsRecoilClock();
            Assert.That(f.OnForge(new ForgeEvent { weapon = "rocket" }, "revolver", bank, medium, clock), Is.False, "another gun: nothing now");
            Assert.That(bank.Active, Is.EqualTo(0));
            Assert.That(f.OnForge(new ForgeEvent { weapon = "revolver", owner = "rival" }, "revolver", bank, medium, clock), Is.False, "not mine");
            Assert.That(f.OnForge(new ForgeEvent { weapon = "revolver" }, "revolver", bank, medium, clock), Is.True, "the gun in hand");
            Assert.That(bank.Active, Is.EqualTo(1), "one kick");
            bank.Update(medium.tPeak);
            for (int c = 0; c < 4; c++) Assert.That(bank.Sample(c), Is.EqualTo(flourish[c]).Within(Math.Abs(flourish[c]) * 1e-9), "forge.flourish[" + c + "] at the first peak");
            Assert.That(clock.Step(0, 1 / UnityTuning.HandsTraceFps), Is.GreaterThan(0), "its attack runs in a freeze too (the recoil clock)");
            var gate = new FlashGate();
            Assert.That(HandsForgeFlourish.GemFlash(0, gate, 0), Is.False, "FLASH INTENSITY 0: no flash");
            Assert.That(HandsForgeFlourish.GemFlash(1, gate, 0.1), Is.True);
            Assert.That(gate.TryFlash(0.2) && gate.TryFlash(0.3), Is.True);
            Assert.That(gate.TryFlash(0.4), Is.False, "the flourish took one slot (and intensity 0 none)");
        }

        // --- STEADY (polish #42: player.js :124 / :160, viewmodel.js :151 / :571-574) -------------------------------------

        [Test]
        public void SteadyScalesTheKickAndThePunch()
        {
            Assert.That(HandsSteady.Mult(null), Is.EqualTo(1), "no perks (the Arena): as before");
            Assert.That(HandsSteady.Mult(PerkMath.Effects(new List<string>(), cfg)), Is.EqualTo(1), "no STEADY");
            double steady = cfg.Num("perks.steady.recoil");
            Assert.That(HandsSteady.Mult(PerkMath.Effects(new List<string> { "steady" }, cfg)), Is.EqualTo(steady), "STEADY: perks.steady.recoil");
            Assert.That(steady, Is.EqualTo(0.7).Within(1e-12), "the web's -30 %");
        }

        // --- movement on the hands (port-only) ---------------------------------------------------------------------------

        [Test]
        public void TheLandingIsWeightedByFallSpeed()
        {
            Assert.That(HandsMove.LandWeight(0), Is.EqualTo(0));
            Assert.That(HandsMove.LandWeight(UnityTuning.HandsLandMinFall), Is.EqualTo(0), "a hop: only the web's dip");
            Assert.That(HandsMove.LandWeight(UnityTuning.HandsLandMaxFall), Is.EqualTo(1));
            Assert.That(HandsMove.LandWeight(100), Is.EqualTo(1));
            double prev = 0;
            for (double f = UnityTuning.HandsLandMinFall; f <= UnityTuning.HandsLandMaxFall; f += 0.5)
            {
                double w = HandsMove.LandWeight(f);
                Assert.That(w, Is.GreaterThanOrEqualTo(prev), "monotonic at " + f);
                prev = w;
            }
        }

        [Test]
        public void TheFreeHandRestsOnTheWallAndPushesOff()
        {
            HandsArmParams a = HandsArmParams.From(cfg);
            // no wall, no push: exactly the web's arm
            foreach (double age in new[] { double.PositiveInfinity, 0.03, a.outTime, 0.2 })
            {
                HandsArmPose web = HandsArm.Pose(a, age, 0.01, 0.02), port = HandsArm.Pose(a, age, 0.01, 0.02, 0, double.PositiveInfinity);
                Assert.That(port.visible, Is.EqualTo(web.visible), "visible at " + age);
                Assert.That(port.x, Is.EqualTo(web.x), "x at " + age);
                Assert.That(port.rollDeg, Is.EqualTo(web.rollDeg), "roll at " + age);
            }
            HandsArmPose wall = HandsArm.Pose(a, double.PositiveInfinity, 0, 0, 1, double.PositiveInfinity);
            Assert.That(wall.visible, Is.True, "on the wall: shown");
            Assert.That(wall.x, Is.EqualTo(UnityTuning.HandsWallHandX).Within(1e-12));
            Assert.That(wall.rollDeg, Is.EqualTo(UnityTuning.HandsWallHandRollDeg).Within(1e-12));
            HandsArmPose punch = HandsArm.Pose(a, a.outTime, 0, 0, 1, double.PositiveInfinity);
            Assert.That(punch.x, Is.EqualTo(a.punchX).Within(1e-12), "a punch on the wall still lands where the web's does");
            HandsArmPose push = HandsArm.Pose(a, double.PositiveInfinity, 0, 0, 1, UnityTuning.HandsWallPushOut);
            Assert.That(push.x, Is.EqualTo(UnityTuning.HandsWallHandX + UnityTuning.HandsWallPushX).Within(1e-12), "the push-off at its peak");
            Assert.That(HandsArm.Pose(a, double.PositiveInfinity, 0, 0, 0, double.PositiveInfinity).visible, Is.False, "off the wall: hidden");
            // the wall pose and the push stay out of the centre box
            var kit = new MeshKit();
            BlockyKit.Arm(kit, a.parts, Color.gray, Color.gray, Color.cyan, a.segments);
            var v = (List<Vector3>)KitV.GetValue(kit);
            var t = (List<int>)KitT.GetValue(kit);
            double tanY = Math.Tan(cfg.Num("viewmodel.fovDeg") * Deg / 2), tanX = tanY * 16 / 9;
            var nx = new double[3];
            var ny = new double[3];
            foreach (HandsArmPose p in new[] { wall, push, HandsArm.Pose(a, double.PositiveInfinity, 0, 0, 0.5, double.PositiveInfinity) })
            {
                double[,] m = Euler(p.pitchDeg * Deg, p.yawDeg * Deg, p.rollDeg * Deg, true);
                for (int i = 0; i + 2 < t.Count; i += 3)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        Vector3 u = v[t[i + k]];
                        Apply(m, u.x, u.y, -u.z, out double cx, out double cy, out double cz);
                        cx += p.x;
                        cy += p.y;
                        cz += p.z;
                        nx[k] = cx / -cz / tanX;
                        ny[k] = cy / -cz / tanY;
                    }
                    Assert.That(Overlaps(nx, ny), Is.False, "the wall hand in the centre box");
                }
            }
        }

        // --- the glint (viewmodel.js :845-855) and the flash cap ---------------------------------------------------------

        [Test]
        public void TheGlintStartsOnceWhenASlowGunsCycleEnds()
        {
            double glintTime = cfg.Num("viewmodel.states.glintTime"), min = cfg.Num("viewmodel.states.glintMinCooldown");
            var rail = new HandsGlint();
            Assert.That(rail.Step(0.5, cfg.Num("weapons.rail.cooldown"), min, glintTime, dt), Is.False, "cooling");
            Assert.That(rail.Step(0.0, cfg.Num("weapons.rail.cooldown"), min, glintTime, dt), Is.True, "ready: weaponReady");
            Assert.That(rail.Step(0.0, cfg.Num("weapons.rail.cooldown"), min, glintTime, dt), Is.False, "once");
            Assert.That(rail.Value(glintTime, 1), Is.GreaterThan(0.9), "glinting");
            Assert.That(rail.Value(glintTime, 0), Is.EqualTo(0), "x flashIntensity (0: none)");
            for (int i = 0; i < 40; i++) rail.Step(0, cfg.Num("weapons.rail.cooldown"), min, glintTime, dt);
            Assert.That(rail.Value(glintTime, 1), Is.EqualTo(0), "short");
            var kestrel = new HandsGlint();
            kestrel.Step(0.5, cfg.Num("weapons.revolver.fireInterval"), min, glintTime, dt);
            Assert.That(kestrel.Step(0, cfg.Num("weapons.revolver.fireInterval"), min, glintTime, dt), Is.False, "a fast gun: no glint (glintMinCooldown)");
        }

        [Test]
        public void HandFlashesAreCappedAtThreeInAnySecond()
        {
            var gate = new HandsFlashGate();
            Assert.That(gate.Allow(0.0), Is.True);
            Assert.That(gate.Allow(0.1), Is.True);
            Assert.That(gate.Allow(0.2), Is.True);
            Assert.That(gate.Allow(0.3), Is.False, "a fourth inside one second");
            Assert.That(gate.Allow(0.99), Is.False);
            Assert.That(gate.Allow(1.05), Is.True, "the first has aged out");
        }

        // ================================================================================================================
        // NIGHT17 U3-30.6: the hands overhaul (snapping in-tos, the QUICKDRAW glint, per-gun kicks, the punch, the poses)
        // ================================================================================================================

        /// <summary>The largest per-frame change (y m, pitch deg, roll deg) while one gun is shown, over a clean swap at 144 fps:
        /// the in-to's and the holster's own top speed (the double-swap pop checks measure re-basing against it).</summary>
        void CleanSwapMaxStep(string from, string to, out double dy, out double dpitch, out double droll)
        {
            dy = dpitch = droll = 0;
            Loop l = SwapLoop(from, to);
            string shown = from;
            HandsDrawPose last = l.st.pose;
            int frames = (int)((l.tl.total * dt + 0.3) * UnityTuning.HandsTraceFps);
            for (int f = 0; f < frames; f++)
            {
                l.Frame(respawnRaise, 1);
                string held = l.st.held;
                HandsDrawPose p = l.st.pose;
                if (held == shown)
                {
                    dy = Math.Max(dy, Math.Abs(p.y - last.y));
                    dpitch = Math.Max(dpitch, Math.Abs(p.pitchDeg - last.pitchDeg));
                    droll = Math.Max(droll, Math.Abs(p.rollDeg - last.rollDeg));
                }
                shown = held;
                last = p;
            }
        }

        /// <summary>The in-to is an ease-out-back: the new gun is up (within 15 % of the drop) in under 45 % of the raise to
        /// the fire step (the web's smoothstep took ~75 %), goes past hold (the overshoot) and settles to hold on that step.</summary>
        [Test]
        public void TheInToSnapsUpOvershootsAndSettlesOnTheFireStep()
        {
            double frameDt = 1 / UnityTuning.HandsTraceFps;
            foreach (string gun in KitGuns)
            {
                Loop l = SwapLoop(gun == "revolver" ? "shotgun" : "revolver", gun);
                double changeMs = -1, upMs = -1, minLowered = 1, riseMs = -1;
                int frames = (int)((l.tl.total * dt + 0.2) * UnityTuning.HandsTraceFps);
                for (int f = 0; f < frames; f++)
                {
                    l.Frame(respawnRaise, 1);
                    double ms = (f + 1) * frameDt * 1000;
                    if (l.st.held != gun) continue; // NIGHT17 round 1: shown from the flick's end
                    if (changeMs < 0)
                    {
                        changeMs = ms;
                        riseMs = (l.tl.fireStep - (l.t - 1 + l.acc / l.dt)) * dt * 1000; // the rise's length on the sim's clock
                    }
                    if (l.t >= l.tl.fireStep) continue;
                    if (upMs < 0 && l.st.pose.lowered <= 0.15) upMs = ms;
                    minLowered = Math.Min(minLowered, l.st.pose.lowered);
                }
                Assert.That(upMs, Is.GreaterThan(0), gun + ": up before fire opens");
                Assert.That(upMs - changeMs, Is.LessThanOrEqualTo(0.45 * riseMs), $"{gun}: up {upMs - changeMs:0.0} ms after it shows (its rise to fire is {riseMs:0.0} ms)");
                Assert.That(minLowered, Is.LessThan(-0.03), gun + ": the in-to goes past hold (the ease-out-back's overshoot)");
            }
            Assert.That(HandsEase.InToBack("light"), Is.GreaterThan(HandsEase.InToBack("heavy")), "a light gun bounces more than a heavy one seats");
            foreach (double s in new[] { UnityTuning.HandsInToBackLight, UnityTuning.HandsInToBackMedium, UnityTuning.HandsInToBackHeavy })
            {
                Assert.That(HandsEase.OutBack(0, s), Is.EqualTo(0).Within(1e-12));
                Assert.That(HandsEase.OutBack(1, s), Is.EqualTo(1).Within(1e-12), "exactly at hold at the end");
                double peak = 0;
                for (double u = 0; u <= 1; u += 0.001) peak = Math.Max(peak, HandsEase.OutBack(u, s));
                Assert.That(peak - 1, Is.EqualTo(4 * s * s * s / (27 * (s + 1) * (s + 1))).Within(1e-4), "the overshoot's size");
            }
        }

        /// <summary>The holster yanks: the gun moves on the first frame after the press (no slow start: no visual latency).</summary>
        [Test]
        public void TheHolsterMovesTheGunOnTheFirstFrameAfterThePress()
        {
            foreach (string gun in KitGuns)
            {
                Loop l = SwapLoop(gun, gun == "revolver" ? "shotgun" : "revolver");
                while (l.t == 0) l.Frame(respawnRaise, 1);
                Assert.That(l.st.pose.y, Is.LessThan(-0.015), gun + ": down at least 1.5 cm on the first frame of the swap");
                Assert.That(l.st.pose.phase, Is.EqualTo(HandsDrawPhase.Holster));
            }
        }

        /// <summary>A shot during the respawn raise (no fire gate there): the raise yields, the gun at hold within 60 ms.</summary>
        [Test]
        public void AShotDuringTheRespawnRaiseYieldsAtOnce()
        {
            double frameDt = 1 / UnityTuning.HandsTraceFps;
            HandsSwapTimeline tl = HandsSwapTimeline.Of(swapTime, 1, lowerFraction, fireFraction, dt);
            foreach (string gun in KitGuns)
            {
                HandsDrawSpec spec = HandsDraw.SpecFor(cfg, gun);
                var st = new HandsDrawState();
                st.Respawn();
                st.Frame(spec, tl, gun, 0, 0, 1, 0, respawnRaise, 1);
                for (double t = 0; t < 0.3 * respawnRaise; t += frameDt) st.Frame(spec, tl, gun, 0, 0, 1, frameDt, respawnRaise, 1);
                Assert.That(OffHold(st.pose), Is.True, gun + ": still rising");
                double before = st.pose.lowered;
                st.Cancel(); // the sim's `fire`
                st.Frame(spec, tl, gun, 0, 0, 1, frameDt, respawnRaise, 1);
                Assert.That(st.pose.lowered, Is.LessThan(before), gun + ": yields on the frame of the shot (no pop: it keeps rising)");
                for (double t = frameDt; t < 0.06; t += frameDt) st.Frame(spec, tl, gun, 0, 0, 1, frameDt, respawnRaise, 1);
                AssertAtHold(st.pose, gun + " 60 ms after a shot in the respawn raise");
            }
        }

        /// <summary>QUICKDRAW: the glint lives the 0.5 s from the step fire opens on a swap, in passes along the gun; a shot
        /// ends it on the same frame; a respawn raise has none.</summary>
        [Test]
        public void TheQuickdrawGlintRunsHalfASecondFromFireOpenAndAShotEndsIt()
        {
            double w = UnityTuning.HandsQuickdrawWindow;
            Assert.That(HandsQuickdraw.Amount(-0.001), Is.EqualTo(0));
            Assert.That(HandsQuickdraw.Amount(w), Is.EqualTo(0));
            Assert.That(HandsQuickdraw.Amount(double.PositiveInfinity), Is.EqualTo(0));
            int passes = 0;
            double lastAlong = -1, peak = 0, lastPeak = double.PositiveInfinity;
            for (double a = 0; a < w; a += 0.001)
            {
                double along = HandsQuickdraw.Along(a), amt = HandsQuickdraw.Amount(a);
                Assert.That(along, Is.InRange(0.0, 1.0));
                Assert.That(amt, Is.InRange(0.0, 1.0));
                if (along < lastAlong)
                {
                    passes++;
                    Assert.That(peak, Is.LessThan(lastPeak), "each pass dimmer");
                    lastPeak = peak;
                    peak = 0;
                }
                peak = Math.Max(peak, amt);
                lastAlong = along;
            }
            Assert.That(passes + 1, Is.EqualTo(UnityTuning.HandsQuickdrawSweeps), "passes along the gun in the window");
            Assert.That(HandsQuickdraw.Amount(w / UnityTuning.HandsQuickdrawSweeps / 2), Is.GreaterThan(0.8), "the first pass is bright");

            Loop l = SwapLoop("revolver", "shotgun");
            while (l.t < l.tl.fireStep) l.Frame(respawnRaise, 1);
            Assert.That(l.st.fromSwap, Is.True, "a swap's draw");
            int extra = 0; // the frame interpolating up to the fire step shows it a frame late at most
            while (!(l.st.QuickdrawAge < double.PositiveInfinity) && extra < 3)
            {
                l.Frame(respawnRaise, 1);
                extra++;
            }
            Assert.That(extra, Is.LessThanOrEqualTo(2), "starts as the fire step is drawn");
            Assert.That(l.st.QuickdrawAge, Is.InRange(0.0, 2 / UnityTuning.HandsTraceFps), "from the step fire opens");
            l.Frame(respawnRaise, 1);
            Assert.That(HandsQuickdraw.Amount(l.st.QuickdrawAge), Is.GreaterThan(0), "glinting");
            l.st.Cancel(); // a shot
            Assert.That(l.st.QuickdrawAge, Is.EqualTo(double.PositiveInfinity), "a shot ends it on the same frame");
            l.Frame(respawnRaise, 1);
            Assert.That(HandsQuickdraw.Amount(l.st.QuickdrawAge), Is.EqualTo(0));

            Loop m = SwapLoop("rail", "rocket");
            for (int f = 0; f < 200; f++) m.Frame(respawnRaise, 1);
            Assert.That(HandsQuickdraw.Amount(m.st.QuickdrawAge), Is.EqualTo(0), "gone after the window");

            var r = new HandsDrawState();
            r.Respawn();
            HandsDrawSpec spec = HandsDraw.SpecFor(cfg, "revolver");
            HandsSwapTimeline tl = HandsSwapTimeline.Of(swapTime, 1, lowerFraction, fireFraction, dt);
            for (int f = 0; f < 60; f++) r.Frame(spec, tl, "revolver", 0, 0, 1, f == 0 ? 0 : 1 / UnityTuning.HandsTraceFps, respawnRaise, 1);
            Assert.That(r.fromSwap, Is.False);
            Assert.That(HandsQuickdraw.Amount(r.QuickdrawAge), Is.EqualTo(0), "no QUICKDRAW on a respawn raise");
        }

        sealed class KickTrace
        {
            public double pitchPeak, pitchPeakAt, pitchMinAfter, backPeak, rollPeak, settle, pitchLow, yawLow, pitchAbs;
        }

        KickTrace TraceKick(HandsKickSpec s, int shot = 0)
        {
            var bank = new SpringBank(UnityTuning.HandsKickSlots, HandsKick.Snaps());
            HandsKick.Add(bank, s, shot, 1, new double[HandsKick.Channels]);
            var k = new KickTrace();
            double step = 0.0005;
            for (double t = step; t < 1.5; t += step)
            {
                bank.Update(step);
                double p = bank.Sample(0), b = bank.Sample(1), r = bank.Sample(3);
                if (p > k.pitchPeak)
                {
                    k.pitchPeak = p;
                    k.pitchPeakAt = t;
                }
                if (t > k.pitchPeakAt) k.pitchMinAfter = Math.Min(k.pitchMinAfter, p);
                k.backPeak = Math.Max(k.backPeak, b);
                if (Math.Abs(r) > Math.Abs(k.rollPeak)) k.rollPeak = r;
                k.pitchLow = Math.Min(k.pitchLow, p);
                k.yawLow = Math.Min(k.yawLow, bank.Sample(4));
                k.pitchAbs = Math.Max(k.pitchAbs, Math.Abs(p));
                if (Math.Abs(p) > 0.01 * Math.Max(k.pitchAbs, 0.1) || Math.Abs(b) > 0.01 * Math.Max(k.backPeak, 0.001)) k.settle = t;
            }
            return k;
        }

        /// <summary>Recoil is a spring impulse per gun: pitch up and back, roll, an overshoot and a settle; each gun its own
        /// (the revolver snaps up and rolls, the shotgun shoves back hard, the STITCHER chatters small).</summary>
        [Test]
        public void EachGunKicksItsOwnWay()
        {
            string[] firing = { "revolver", "shotgun", "nailgun", "rocket", "rail" };
            var traces = new Dictionary<string, KickTrace>();
            var springs = new HashSet<string>();
            foreach (string g in firing)
            {
                HandsKickSpec s = HandsKick.SpecFor(g);
                Assert.That(s, Is.Not.Null, g);
                Assert.That(springs.Add($"{s.a.spring.freqHz}/{s.a.spring.zeta}"), Is.True, g + ": its own spring");
                KickTrace k = traces[g] = TraceKick(s);
                Assert.That(k.backPeak, Is.GreaterThan(0), g + ": back toward the eye");
                Assert.That(k.settle, Is.LessThan(0.9), g + ": settles");
                if (g == "shotgun")
                {
                    // round 5 (n17-strip-horde: the barrel lay across the horde): the THUNDERCLAP kicks DOWN and RIGHT, opening the centre
                    Assert.That(k.pitchLow, Is.LessThanOrEqualTo(-6), "the THUNDERCLAP's muzzle kicks 6-8 deg down");
                    Assert.That(k.pitchLow, Is.GreaterThanOrEqualTo(-8.5), "not more");
                    Assert.That(k.yawLow, Is.LessThanOrEqualTo(-4), "and to the right");
                    continue;
                }
                Assert.That(k.pitchPeak, Is.GreaterThan(0), g + ": muzzle up");
                Assert.That(k.pitchMinAfter, Is.LessThan(0), g + ": overshoots below hold after its peak");
            }
            Assert.That(HandsKick.SpecFor("gauntlet"), Is.Null, "the gauntlet's swing is its kick");
            Assert.That(HandsKick.SpecFor("rattler"), Is.Null, "a rig-less gun: the web's recoil");
            KickTrace rv = traces["revolver"], sg = traces["shotgun"], ng = traces["nailgun"], rk = traces["rocket"], rl = traces["rail"];
            Assert.That(rv.pitchPeak, Is.GreaterThan(12), "the KESTREL snaps up");
            Assert.That(rv.pitchPeakAt, Is.LessThan(0.035), "fast");
            Assert.That(Math.Abs(rv.rollPeak), Is.GreaterThan(5), "and rolls");
            Assert.That(sg.backPeak, Is.GreaterThan(0.15), "the THUNDERCLAP shoves back hard");
            Assert.That(sg.backPeak, Is.GreaterThan(2 * rv.backPeak), "harder than the KESTREL");
            Assert.That(ng.pitchPeak, Is.LessThan(2), "the STITCHER's kick is small");
            Assert.That(ng.settle, Is.LessThan(0.2), "and quick");
            Assert.That(rk.settle, Is.GreaterThan(0.35), "the COMET seats slowly");
            Assert.That(rl.pitchMinAfter / rl.pitchPeak, Is.LessThan(-0.3), "the LONGWAVE rings");
            // the chatter: consecutive STITCHER shots roll opposite ways
            Assert.That(Math.Sign(TraceKick(HandsKick.SpecFor("nailgun"), 0).rollPeak), Is.EqualTo(-Math.Sign(TraceKick(HandsKick.SpecFor("nailgun"), 1).rollPeak)), "the chatter alternates");
            // STEADY scales every channel
            var peaks = new double[HandsKick.Channels];
            HandsKickSpec rs = HandsKick.SpecFor("revolver");
            HandsKick.Peaks(rs.a, 0, false, 0, 0.7, peaks);
            Assert.That(peaks[0], Is.EqualTo(rs.a.pitchDeg * 0.7).Within(1e-12));
            Assert.That(peaks[3], Is.EqualTo(rs.a.rollDeg * 0.7).Within(1e-12));
        }

        /// <summary>Round 2 ("the KESTREL's flip peaks late and is still held at 217 ms"): the per-gun kick runs on frame
        /// time through its own shot's freeze (the kill freeze no longer holds it at its peak): the KESTREL peaks within
        /// 25 ms and is back under 15 % of its peak by 120 ms, frozen world or not; the world running: world time exactly.</summary>
        [Test]
        public void TheKickRunsOnFrameTimeThroughAFreeze()
        {
            double frameDt = 1 / UnityTuning.HandsTraceFps;
            Assert.That(HandsKick.ClockDt(0, frameDt), Is.EqualTo(frameDt), "frozen: frame time");
            Assert.That(HandsKick.ClockDt(0.3 * frameDt, frameDt), Is.EqualTo(frameDt), "slowed: frame time");
            Assert.That(HandsKick.ClockDt(frameDt, frameDt), Is.EqualTo(frameDt), "running: world time");
            foreach (string g in new[] { "revolver", "shotgun", "nailgun", "rocket", "rail" })
            {
                HandsKickSpec s = HandsKick.SpecFor(g);
                var bank = new SpringBank(UnityTuning.HandsKickSlots, HandsKick.Snaps());
                HandsKick.Add(bank, s, 0, 1, new double[HandsKick.Channels]);
                double peak = 0, peakAt = 0, at120 = double.NaN, ms = 0;
                for (int f = 1; f <= 60; f++)
                {
                    bank.Update(HandsKick.ClockDt(0, frameDt)); // the whole window inside a kill freeze
                    ms = f * frameDt * 1000;
                    if (bank.Sample(0) > peak)
                    {
                        peak = bank.Sample(0);
                        peakAt = ms;
                    }
                    if (double.IsNaN(at120) && ms >= 120) at120 = bank.Sample(0);
                }
                Assert.That(peak, Is.GreaterThan(0.8 * s.a.pitchDeg), g + ": up inside the freeze");
                if (g == "revolver")
                {
                    Assert.That(peakAt, Is.LessThanOrEqualTo(25), "the KESTREL peaks by ~25 ms");
                    Assert.That(Math.Abs(at120), Is.LessThan(0.15 * peak), "and is back down by 120 ms");
                }
            }
        }

        HandsArmPose Punch(HandsArmParams a, double age, bool hit) => HandsPunch.Pose(a, age, hit, 0, 0);

        /// <summary>The punch: a wind-up the eye can catch (the fist low left, pulled back), an accelerating strike, past the
        /// target on the impact frame (the web's punchOutTime), a snap back (the hard stop), a rebound off a target it hit,
        /// gone by the web's cycle.</summary>
        [Test]
        public void ThePunchWindsUpStrikesHardStopsAndRebounds()
        {
            HandsArmParams a = HandsArmParams.From(cfg);
            double frameDt = 1 / UnityTuning.HandsTraceFps;
            double tanY = Math.Tan(cfg.Num("viewmodel.fovDeg") * Deg / 2);
            Assert.That(Punch(a, 0, true).visible, Is.False, "the press frame: at rest");
            Assert.That(Punch(a, frameDt, true).visible, Is.True, "shown from the next frame");
            double cockT = a.outTime * UnityTuning.HandsPunchCockShare;
            HandsArmPose cock = Punch(a, cockT, true);
            Assert.That(cock.z, Is.EqualTo(UnityTuning.HandsPunchCockZ).Within(1e-9), "cocked: pulled back");
            Assert.That(cock.rollDeg, Is.EqualTo(UnityTuning.HandsPunchCockRollDeg).Within(1e-9), "the wrist cocked");
            Assert.That(cock.y / -cock.z / tanY, Is.GreaterThan(-1.1), "the wind-up is at the screen's bottom edge (the eye can catch it)");
            double strike = a.outTime - cockT, d = strike / 6;
            double v1 = Math.Abs(Punch(a, cockT + d, true).z - cock.z), v2 = Math.Abs(Punch(a, a.outTime, true).z - Punch(a, a.outTime - d, true).z);
            Assert.That(v2, Is.GreaterThan(2 * v1), "the strike accelerates into the target");
            HandsArmPose impact = Punch(a, a.outTime, true), whiff = Punch(a, a.outTime, false);
            Assert.That(impact.z, Is.EqualTo(a.punchZ - UnityTuning.HandsPunchOverZ).Within(1e-9), "past the target on the impact frame");
            Assert.That(whiff.z, Is.LessThan(impact.z), "a whiff follows through further");
            HandsArmPose snapped = Punch(a, a.outTime + UnityTuning.HandsPunchSnapTime, true);
            Assert.That(snapped.z, Is.GreaterThanOrEqualTo(a.punchZ - 0.002), "snapped back to the target (the hard stop) or off it");
            double reb = double.NegativeInfinity, whiffMax = double.NegativeInfinity;
            for (double t = a.outTime; t <= a.outTime + a.holdTime; t += 0.001)
            {
                reb = Math.Max(reb, Punch(a, t, true).z);
                whiffMax = Math.Max(whiffMax, Punch(a, t, false).z);
            }
            Assert.That(reb, Is.GreaterThan(a.punchZ + 0.4 * UnityTuning.HandsPunchReboundZ), "a hit rebounds off the target");
            Assert.That(whiffMax, Is.LessThanOrEqualTo(a.punchZ + 1e-9), "a whiff doesn't rebound (it follows through and comes back)");
            Assert.That(Punch(a, a.cycleTime, true).visible, Is.False, "gone by the web's cycle");
            Assert.That(Punch(a, a.cycleTime - 0.002, true).z, Is.EqualTo(a.restZ).Within(0.01), "back near rest at the end");
            // the wall variant with no wall: the same pose
            HandsArmPose wall0 = HandsPunch.Pose(a, a.outTime, true, 0, 0, 0, double.PositiveInfinity);
            Assert.That(wall0.z, Is.EqualTo(impact.z).Within(1e-12));
        }

        /// <summary>A punch kill / parry freezes on the punch's own step: the strike runs on frame time and the impact frame
        /// (past the target) holds through the freeze; the snap and the rebound play when the world runs again.</summary>
        [Test]
        public void TheFistHoldsItsImpactFrameInAFreeze()
        {
            HandsArmParams a = HandsArmParams.From(cfg);
            double frameDt = 1 / UnityTuning.HandsTraceFps, age = 0;
            for (int f = 1; f <= 20; f++)
            {
                age = HandsArm.AdvanceOut(age, 0, frameDt, a.outTime);
                HandsArmPose p = Punch(a, age, true);
                Assert.That(p.visible, Is.True, "frame " + f);
                if (f >= 9) Assert.That(p.z, Is.EqualTo(a.punchZ - UnityTuning.HandsPunchOverZ).Within(1e-9), $"frame {f}: the impact frame held");
            }
            age = HandsArm.AdvanceOut(age, UnityTuning.HandsPunchSnapTime, frameDt, a.outTime);
            Assert.That(Punch(a, age, true).z, Is.GreaterThanOrEqualTo(a.punchZ - 0.002), "snapped back (and off the target) when the world runs");
        }

        /// <summary>The punch (hit and whiff) never puts a pixel of the arm in the centre box.</summary>
        [Test]
        public void TheNewPunchStaysOutOfTheCentreBox()
        {
            HandsArmParams a = HandsArmParams.From(cfg);
            var kit = new MeshKit();
            BlockyKit.ArmFist(kit, a.parts, Color.gray, Color.gray, Color.cyan, a.segments); // NIGHT17 round 1: the fist the view draws
            BlockyKit.ArmKnuckles(kit, BlockyKit.FistOf(a.parts), Color.white);
            var v = (List<Vector3>)KitV.GetValue(kit);
            var t = (List<int>)KitT.GetValue(kit);
            double tanY = Math.Tan(cfg.Num("viewmodel.fovDeg") * Deg / 2), tanX = tanY * 16 / 9;
            var cx = new double[3];
            var cy = new double[3];
            var cz = new double[3];
            double near = cfg.Num("viewmodel.near");
            foreach (bool hit in new[] { true, false })
                for (double age = 0; age < a.cycleTime; age += 1 / UnityTuning.HandsTraceFps / 2)
                {
                    HandsArmPose p = Punch(a, age, hit);
                    if (!p.visible) continue;
                    double[,] m = Euler(p.pitchDeg * Deg, p.yawDeg * Deg, p.rollDeg * Deg, true);
                    for (int i = 0; i + 2 < t.Count; i += 3)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            Vector3 u = v[t[i + k]];
                            Apply(m, u.x, u.y, -u.z, out double ox, out double oy, out double oz);
                            cx[k] = ox + p.x;
                            cy[k] = oy + p.y;
                            cz[k] = oz + p.z;
                        }
                        // the forearm runs back past the camera: clip at the near plane as the renderer does
                        Assert.That(ClippedOverlaps(cx, cy, cz, near, tanX, tanY), Is.False, $"the arm at {age:0.000} s (hit {hit})");
                    }
                }
        }

        /// <summary>NIGHT19 U3-33.8: the R3 jab (HandsJab, tonight's punch pose) keeps the web's NIGHT5 rule too: hit and whiff,
        /// no pixel of the arm in the centre box.</summary>
        [Test]
        public void TheJabStaysOutOfTheCentreBox()
        {
            HandsArmParams a = HandsArmParams.From(cfg);
            var kit = new MeshKit();
            BlockyKit.ArmFist(kit, a.parts, Color.gray, Color.gray, Color.cyan, a.segments);
            BlockyKit.ArmKnuckles(kit, BlockyKit.FistOf(a.parts), Color.white);
            var v = (List<Vector3>)KitV.GetValue(kit);
            var t = (List<int>)KitT.GetValue(kit);
            double tanY = Math.Tan(cfg.Num("viewmodel.fovDeg") * Deg / 2), tanX = tanY * 16 / 9;
            var cx = new double[3];
            var cy = new double[3];
            var cz = new double[3];
            double near = cfg.Num("viewmodel.near");
            // the control: the same fist held in the middle of the view lands in the box (the projection is live)
            bool control = false;
            for (int i = 0; i + 2 < t.Count && !control; i += 3)
            {
                for (int k = 0; k < 3; k++)
                {
                    Vector3 u = v[t[i + k]];
                    cx[k] = u.x - a.parts[0].x;
                    cy[k] = u.y - a.parts[0].y;
                    cz[k] = -u.z + a.parts[0].z - 0.5;
                }
                control = ClippedOverlaps(cx, cy, cz, near, tanX, tanY);
            }
            Assert.That(control, Is.True, "control: a fist in the middle is in the box");
            foreach (bool hit in new[] { true, false })
                for (double age = 0; age < a.cycleTime; age += 1 / UnityTuning.HandsTraceFps / 2)
                {
                    HandsArmPose p = HandsJab.Pose(a, age, hit, 0, 0);
                    if (!p.visible) continue;
                    double[,] m = Euler(p.pitchDeg * Deg, p.yawDeg * Deg, p.rollDeg * Deg, true);
                    for (int i = 0; i + 2 < t.Count; i += 3)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            Vector3 u = v[t[i + k]];
                            Apply(m, u.x, u.y, -u.z, out double ox, out double oy, out double oz);
                            cx[k] = ox + p.x;
                            cy[k] = oy + p.y;
                            cz[k] = oz + p.z;
                        }
                        Assert.That(ClippedOverlaps(cx, cy, cz, near, tanX, tanY), Is.False, $"the jab at {age:0.000} s (hit {hit})");
                    }
                }
        }

        /// <summary>NIGHT19 U3-33.6: the ported ZAPPER and BOUNCER (weaponModels.js as they are) are real models at the web's
        /// muzzles, and stay out of the centre box at their hold pose.</summary>
        [Test]
        public void ThePortedGunsAreTheWebsModelsAndStayOutOfTheCentreBox()
        {
            foreach (string gun in new[] { "zapper", "bouncer" })
            {
                var body = new MeshKit();
                var part = new MeshKit();
                Vector3 muzzle = gun == "zapper" ? HandsPortKit.Zapper(body, Color.white, part) : HandsPortKit.Bouncer(body, Color.white, part);
                Vector3 web = gun == "zapper" ? new Vector3(0, 0.04f, -0.52f) : new Vector3(0, 0.04f, -0.4f); // weaponModels.js muzzle
                Assert.That((muzzle - new Vector3(web.x, web.y, -web.z)).magnitude, Is.LessThan(1e-5f), gun + " muzzle");
                var tris = new List<Vector3>();
                Vector3 at = gun == "bouncer" ? HandsPortKit.BouncerDrumAt : Vector3.zero;
                foreach (var (kit, off) in new[] { (body, Vector3.zero), (part, at) })
                {
                    var v = (List<Vector3>)KitV.GetValue(kit);
                    var t = (List<int>)KitT.GetValue(kit);
                    foreach (int i in t)
                    {
                        Vector3 u = v[i] + off;
                        tris.Add(new Vector3(u.x, u.y, -u.z));
                    }
                }
                Assert.That(tris.Count, Is.GreaterThan(300), gun + " mesh (not a grey box)");
                Assert.That(InBox(gun, tris, new HandsDrawPose { x = -cfg.Num("viewmodel.holdX"), y = -cfg.Num("viewmodel.holdY") }), Is.GreaterThan(0), gun + " control");
                Assert.That(InBox(gun, tris, new HandsDrawPose()), Is.EqualTo(0), gun + " at its hold pose");
            }
        }

        /// <summary>A camera-space triangle clipped to z &lt;= -near (Sutherland-Hodgman), projected, tested against the box.</summary>
        static bool ClippedOverlaps(double[] x, double[] y, double[] z, double near, double tanX, double tanY)
        {
            var px = new List<double>();
            var py = new List<double>();
            for (int k = 0; k < 3; k++)
            {
                int n = (k + 1) % 3;
                bool inK = z[k] <= -near, inN = z[n] <= -near;
                if (inK)
                {
                    px.Add(x[k] / -z[k] / tanX);
                    py.Add(y[k] / -z[k] / tanY);
                }
                if (inK != inN)
                {
                    double s = (-near - z[k]) / (z[n] - z[k]);
                    double ix = x[k] + (x[n] - x[k]) * s, iy = y[k] + (y[n] - y[k]) * s;
                    px.Add(ix / near / tanX);
                    py.Add(iy / near / tanY);
                }
            }
            for (int k = 1; k + 1 < px.Count; k++)
                if (Overlaps(new[] { px[0], px[k], px[k + 1] }, new[] { py[0], py[k], py[k + 1] })) return true;
            return false;
        }

        /// <summary>The sprint carry and the boost lean ease in and out (no pops), yield to a shot from its own frame, come
        /// back after the hold; the boost's swing lags the move and catches up past it; all out of the centre box.</summary>
        [Test]
        public void TheSprintAndBoostPosesEaseInOutAndYieldToAShot()
        {
            double frameDt = 1 / UnityTuning.HandsTraceFps;
            var m = new HandsMovePose();
            double last = 0;
            for (double t = 0; t < 0.8; t += frameDt)
            {
                m.Frame(true, false, frameDt);
                Assert.That(m.sprintW - last, Is.InRange(0.0, UnityTuning.HandsSprintInRate * frameDt + 1e-9), "eases in");
                last = m.sprintW;
            }
            Assert.That(m.sprintW, Is.GreaterThan(0.99));
            HandsDrawPose full = m.Sample(0, 1);
            Assert.That(full.pitchDeg, Is.LessThan(-30), "muzzle down: a clear running carry (round 3: ~20 deg lower)");
            double bobLo = double.PositiveInfinity, bobHi = double.NegativeInfinity, rollLo = double.PositiveInfinity, rollHi = double.NegativeInfinity;
            for (double ph = 0; ph < 2 * Math.PI; ph += 0.05)
            {
                HandsDrawPose bp = m.Sample(ph, 1);
                bobLo = Math.Min(bobLo, bp.y);
                bobHi = Math.Max(bobHi, bp.y);
                rollLo = Math.Min(rollLo, bp.rollDeg);
                rollHi = Math.Max(rollHi, bp.rollDeg);
            }
            Assert.That(bobHi - bobLo, Is.GreaterThan(0.025), "a strong bob in step (round 3)");
            Assert.That(rollHi - rollLo, Is.GreaterThan(10), "the carry swings");
            Assert.That(full.rollDeg, Is.GreaterThan(15), "canted");
            Assert.That(full.y, Is.LessThan(-0.03), "tucked");
            m.Shot();
            m.Frame(true, false, frameDt);
            Assert.That(m.gate, Is.LessThan(1), "yields on the shot's frame");
            Assert.That(m.gate, Is.GreaterThan(0.3), "no pop");
            for (double t = frameDt; t < UnityTuning.HandsPoseYieldTime; t += frameDt) m.Frame(true, false, frameDt);
            m.Frame(true, false, frameDt);
            Assert.That(m.gate, Is.EqualTo(0).Within(1e-9), "out by the yield time");
            Assert.That(Math.Abs(m.Sample(1.3, 1).pitchDeg) + Math.Abs(m.Sample(1.3, 1).rollDeg), Is.LessThan(1e-6), "the gun up to fire");
            double g = 0;
            for (double t = 0; t < UnityTuning.HandsPoseShotHold + UnityTuning.HandsPoseReturnTime + 0.05; t += frameDt)
            {
                m.Frame(true, false, frameDt);
                Assert.That(m.gate - g, Is.LessThanOrEqualTo(1.6 * frameDt / UnityTuning.HandsPoseReturnTime), "back in eased");
                g = m.gate;
            }
            Assert.That(m.gate, Is.EqualTo(1).Within(1e-9), "back after the hold");
            last = m.sprintW;
            for (double t = 0; t < 0.6; t += frameDt)
            {
                m.Frame(false, false, frameDt);
                Assert.That(last - m.sprintW, Is.InRange(0.0, UnityTuning.HandsSprintOutRate * frameDt + 1e-9), "eases out");
                last = m.sprintW;
            }
            Assert.That(m.sprintW, Is.LessThan(0.01));

            // the boost's swing: a boost to the right; the gun lags left, then catches up past
            var b = new HandsMovePose();
            b.Dash(1, 0, 1);
            double minX = 0, maxXAfter = 0, minAt = 0;
            for (double t = 0; t < 1.2; t += frameDt)
            {
                b.Frame(t < 0.15, t < 0.15, frameDt);
                double x = b.Sample(0, 1).x;
                if (x < minX)
                {
                    minX = x;
                    minAt = t;
                }
                if (t > minAt + 0.02) maxXAfter = Math.Max(maxXAfter, x);
                if (t > 1.1) Assert.That(Math.Abs(x), Is.LessThan(0.002), "settled");
            }
            Assert.That(minX, Is.LessThan(-0.6 * UnityTuning.HandsDashLagX), "lags the move");
            Assert.That(maxXAfter, Is.GreaterThan(0.002), "catches up past it");
            var lean = new HandsMovePose();
            lean.Dash(1, 0, 1);
            for (int f = 0; f < 20; f++) lean.Frame(false, true, frameDt);
            Assert.That(lean.boostW, Is.GreaterThan(0.9));
            // out of the centre box: the full carry at the bob's extremes, and the boost's lean with its swing
            foreach (string gun in KitGuns)
            {
                List<Vector3> tris = RigTriangles(gun);
                var s = new HandsMovePose();
                for (int f = 0; f < 200; f++) s.Frame(true, false, frameDt);
                foreach (double phase in new[] { 0, Math.PI / 2, Math.PI, 3 * Math.PI / 2 })
                    Assert.That(InBox(gun, tris, s.Sample(phase, 1)), Is.EqualTo(0), $"{gun} sprinting at phase {phase:0.00}");
                foreach (double side in new[] { 1.0, -1.0 })
                {
                    var d = new HandsMovePose();
                    d.Dash(side, 0.3, 1);
                    for (double t = 0; t < 0.6; t += frameDt)
                    {
                        d.Frame(false, t < 0.2, frameDt);
                        Assert.That(InBox(gun, tris, d.Sample(0, 1)), Is.EqualTo(0), $"{gun} boosting {side} at {t:0.000}");
                    }
                }
            }
        }

        [Test]
        public void TheBreathingMovesPitchAndRollSubtly()
        {
            double period = cfg.Num("viewmodel.breathPeriod"), pitch = 0, roll = 0;
            for (double t = 0; t < period * UnityTuning.HandsBreathRollPeriodMult * 2; t += 0.01)
            {
                HandsDrawPose p = HandsBreath.Sample(t, period, 1);
                pitch = Math.Max(pitch, Math.Abs(p.pitchDeg));
                roll = Math.Max(roll, Math.Abs(p.rollDeg));
                Assert.That(HandsBreath.Sample(t, period, 0).pitchDeg, Is.EqualTo(0), "REDUCED MOTION / intensity 0: still");
            }
            Assert.That(pitch, Is.InRange(0.2, 0.6), "alive, subtle");
            Assert.That(roll, Is.InRange(0.1, 0.4));
        }

        /// <summary>The rig's on-screen NDC bounds (triangles in front of the near plane, clamped to the screen); false: none on screen.</summary>
        bool ScreenBounds(string gun, List<Vector3> tris, HandsDrawPose p, out double x0, out double x1, out double y0, out double y1) =>
            ScreenBounds(gun, tris, p, HandsDraw.Presence(gun), out x0, out x1, out y0, out y1);

        static void Hsv(Color c, out double h, out double s, out double v)
        {
            double max = Math.Max(c.r, Math.Max(c.g, c.b)), min = Math.Min(c.r, Math.Min(c.g, c.b)), d = max - min;
            v = max;
            s = max > 0 ? d / max : 0;
            if (d <= 0) h = 0;
            else if (max == c.r) h = ((c.g - c.b) / d % 6 + 6) % 6 / 6;
            else if (max == c.g) h = ((c.b - c.r) / d + 2) / 6;
            else h = ((c.r - c.g) / d + 4) / 6;
        }

        /// <summary>Round 3 ("dark violet, small, in the same corner"): every kit gun ~7 % larger on screen at hold (its area
        /// over 1.1x), still out of the centre box and the card (the other tests); each gun's accent (its family colour on
        /// the body) brighter at the same hue.</summary>
        [Test]
        public void TheGunsHaveMorePresenceAndBrighterAccents()
        {
            foreach (string g in KitGuns)
            {
                List<Vector3> tris = RigTriangles(g);
                Assert.That(ScreenBounds(g, tris, default, 1, out double a0, out double a1, out double b0, out double b1), Is.True);
                Assert.That(ScreenBounds(g, tris, default, out double c0, out double c1, out double d0, out double d1), Is.True);
                double before = (a1 - a0) * (b1 - b0), after = (c1 - c0) * (d1 - d0);
                Assert.That(after, Is.GreaterThan(1.06 * before), $"{g}: {after / before:0.000} x the screen area (the screen edge clips some)");
                Color family = WeaponLook.FamilyColor(cfg, g, Color.white);
                Color accent = HandsLook.Accent(family);
                Hsv(family, out double h0, out double s0, out double v0);
                Hsv(accent, out double h1, out double s1, out double v1);
                Assert.That(Math.Abs(h1 - h0), Is.LessThan(0.003), g + ": the same hue");
                Assert.That(v1, Is.GreaterThanOrEqualTo(Math.Min(1, 1.2 * v0) - 1e-4), $"{g}: brighter (value {v0:0.00} -> {v1:0.00})");
                Assert.That(s1, Is.GreaterThanOrEqualTo(s0 - 1e-4), g + ": no greyer");
            }
        }

        bool ScreenBounds(string gun, List<Vector3> tris, HandsDrawPose p, double presence, out double x0, out double x1, out double y0, out double y1)
        {
            double holdX = cfg.Num("viewmodel.holdX"), holdY = cfg.Num("viewmodel.holdY"), holdZ = cfg.Num("viewmodel.holdZ");
            double holdYaw = cfg.Num("viewmodel.holdYawDeg"), holdPitch = cfg.Num("viewmodel.holdPitchDeg");
            string ko = "viewmodel.kitOffsets." + gun;
            double kx = 0, ky = 0, kz = 0, kp = 0, kyaw = 0, kr = 0, ks = 1;
            if (cfg.Has(ko))
            {
                kx = cfg.Num(ko + ".x");
                ky = cfg.Num(ko + ".y");
                kz = cfg.Num(ko + ".z");
                kp = cfg.Has(ko + ".pitchDeg") ? cfg.Num(ko + ".pitchDeg") : 0;
                kyaw = cfg.Has(ko + ".yawDeg") ? cfg.Num(ko + ".yawDeg") : 0;
                kr = cfg.Has(ko + ".rollDeg") ? cfg.Num(ko + ".rollDeg") : 0;
                ks = cfg.Has(ko + ".scale") ? cfg.Num(ko + ".scale") : 1;
            }
            HandsDraw.HoldNudge(gun, out double nx0, out double ny0);
            kx += nx0;
            ky += ny0;
            ks *= presence; // round 3: the presence scale
            HandsDraw.HoldTurn(gun, out double turnP, out double turnY); // round 6: the THUNDERCLAP turned out of the centre
            kp += turnP;
            kyaw += turnY;
            double[,] kit = Euler(kp * Deg, kyaw * Deg, kr * Deg, false);
            double[,] root = Euler((holdPitch + p.pitchDeg) * Deg, (holdYaw + p.yawDeg) * Deg, p.rollDeg * Deg, true);
            double tanY = Math.Tan(cfg.Num("viewmodel.fovDeg") * Deg / 2), tanX = tanY * 16 / 9;
            x0 = y0 = double.PositiveInfinity;
            x1 = y1 = double.NegativeInfinity;
            bool any = false;
            foreach (Vector3 v in tris)
            {
                Apply(kit, v.x * ks, v.y * ks, v.z * ks, out double ax, out double ay, out double az);
                Apply(root, ax + kx, ay + ky, az + kz, out double cx, out double cy, out double cz);
                cx += holdX + p.x;
                cy += holdY + p.y;
                cz += holdZ + p.z;
                if (cz > -0.02) continue;
                double sx = cx / -cz / tanX, sy = cy / -cz / tanY;
                if (sx < -1 || sx > 1 || sy < -1 || sy > 1) continue;
                any = true;
                x0 = Math.Min(x0, sx);
                x1 = Math.Max(x1, sx);
                y0 = Math.Min(y0, sy);
                y1 = Math.Max(y1, sy);
            }
            return any;
        }

        // --- round 1 (build 2's strips): the swap shows the new gun from the press, the punch on its contact frame --------

        /// <summary>A swap from `from` to `to` pressed on step 1, the sim stepped as Combat.StepSwap, the view fed the TARGET
        /// gun from the press (the loadout's targetSlot) at 144 fps. `frame` gets the state, the sim, ms since the press.</summary>
        void PressSwap(string from, string to, Action<HandsDrawState, SwapSim, double> frame, double seconds = 0.6)
        {
            HandsSwapTimeline tl = HandsSwapTimeline.Of(swapTime, 1, lowerFraction, fireFraction, dt);
            var sim = new SwapSim { tl = tl, owned = new[] { from, to } };
            var specs = new Dictionary<string, HandsDrawSpec> { [from] = HandsDraw.SpecFor(cfg, from), [to] = HandsDraw.SpecFor(cfg, to) };
            var st = new HandsDrawState();
            double frameDt = 1 / UnityTuning.HandsTraceFps, acc = 0, ms = 0;
            st.Frame(specs[from], tl, from, 0, 0, 1, frameDt, respawnRaise, 1, false);
            int step = 0;
            for (int f = 0; f < seconds * UnityTuning.HandsTraceFps; f++)
            {
                acc += frameDt;
                while (acc >= dt)
                {
                    acc -= dt;
                    step++;
                    sim.Step(step == 1 ? 1 : -1);
                }
                string want = sim.owned[sim.target];
                st.Frame(specs[want], tl, want, sim.prev, sim.now, acc / dt, frameDt, respawnRaise, 1, sim.active || sim.slot != sim.target);
                if (step >= 1) ms += frameDt * 1000;
                frame(st, sim, ms);
            }
        }

        /// <summary>No empty hands: the new gun is shown from the press after a short flick of the old one (the switch made
        /// off-screen), up within ~130 ms of the press, past hold, at hold on the sim's fire step; never more than one frame
        /// with no gun on screen.</summary>
        [Test]
        public void TheNewGunShowsFromThePressWithNoEmptyFrames()
        {
            double frameDt = 1 / UnityTuning.HandsTraceFps;
            var tris = new Dictionary<string, List<Vector3>>();
            foreach (string g in KitGuns) tris[g] = RigTriangles(g);
            foreach (string to in KitGuns)
            {
                string from = to == "revolver" ? "shotgun" : "revolver";
                double shownAt = -1, upAt = -1, minLowered = 1;
                int emptyRun = 0, worstEmpty = 0, bothFrames = 0;
                bool checkedFire = false, wasReady = true;
                PressSwap(from, to, (st, sim, ms) =>
                {
                    if (ms <= 0) return;
                    if (shownAt < 0 && st.held == to) shownAt = ms;
                    // round 2: the outgoing gun keeps flicking out (st.outHeld / outPose) while the new one rises: overlap
                    bool onNew = ScreenBounds(st.held, tris[st.held], st.pose, out _, out _, out _, out _);
                    bool onOut = st.outHeld != null && ScreenBounds(st.outHeld, tris[st.outHeld], st.outPose, out _, out _, out _, out _);
                    if (onNew && onOut) bothFrames++;
                    bool on = onNew || onOut;
                    emptyRun = on ? 0 : emptyRun + 1;
                    worstEmpty = Math.Max(worstEmpty, emptyRun);
                    if (st.held == to && !sim.ready) minLowered = Math.Min(minLowered, st.pose.lowered);
                    if (st.held == to && upAt < 0 && st.pose.lowered <= 0.15) upAt = ms;
                    if (sim.ready && !wasReady)
                    {
                        Assert.That(st.held, Is.EqualTo(to));
                        checkedFire = true;
                    }
                    wasReady = sim.ready;
                });
                Assert.That(shownAt, Is.InRange(0.0, UnityTuning.HandsFlickTime * 1000 + 2 * frameDt * 1000), $"{from}>{to}: the new gun from the flick's end ({shownAt:0.0} ms)");
                Assert.That(worstEmpty, Is.EqualTo(0), $"{from}>{to}: frames with no gun on screen");
                Assert.That(bothFrames * frameDt * 1000, Is.GreaterThanOrEqualTo(27), $"{from}>{to}: the old and new guns overlap ({bothFrames} frames: 4 at 144 fps, round 6: the turned THUNDERCLAP leaves sooner)");
                Assert.That(upAt, Is.InRange(0.0, 135.0), $"{from}>{to}: up {upAt:0.0} ms after the press");
                Assert.That(minLowered, Is.LessThan(-0.03), $"{from}>{to}: past hold before fire opens");
                Assert.That(checkedFire, Is.True);
            }
            // the switch is made off-screen: every rig at its flick depth (with its hip tip x motion) has no pixel on screen,
            // and is on screen a little above it (the switch sits at the screen's edge: no dead travel)
            foreach (string g in KitGuns)
                foreach (double motion in new[] { 1.0, UnityTuning.HandsReducedMotionScale, 0.0 })
                {
                    HandsDrawSpec s = HandsDraw.SpecFor(cfg, g);
                    double depth = HandsDraw.FlickDepth(s, motion);
                    HandsDrawPose p = HandsDraw.SampleW(s, HandsDrawPhase.Holster, depth, 1, 0, 0, double.PositiveInfinity, false, motion);
                    Assert.That(ScreenBounds(g, tris[g], p, out _, out _, out _, out _), Is.False, $"{g} at its flick depth {depth:0.000} (motion {motion}) is off-screen");
                    HandsDrawPose above = HandsDraw.SampleW(s, HandsDrawPhase.Holster, depth - 0.1, 1, 0, 0, double.PositiveInfinity, false, motion);
                    Assert.That(ScreenBounds(g, tris[g], above, out _, out _, out _, out _), Is.True, $"{g} 0.1 above its flick depth (motion {motion}) is on screen");
                }
        }
        /// <summary>Every gun at hold on the step the sim opens fire, with the view fed the target from the press (x1 and QUICK).</summary>
        [Test]
        public void EveryGunIsAtHoldOnTheFireStepWhenShownFromThePress()
        {
            foreach (double mult in new[] { 1.0, quick })
            {
                HandsSwapTimeline tl = HandsSwapTimeline.Of(swapTime, mult, lowerFraction, fireFraction, dt);
                foreach (string to in KitGuns)
                {
                    string from = to == "revolver" ? "shotgun" : "revolver";
                    var sim = new SwapSim { tl = tl, owned = new[] { from, to } };
                    var st = new HandsDrawState();
                    st.Frame(HandsDraw.SpecFor(cfg, from), tl, from, 0, 0, 1, dt, respawnRaise, 1, false);
                    bool wasReady = true, seen = false;
                    for (int step = 1; step <= tl.total + 5; step++)
                    {
                        sim.Step(step == 1 ? 1 : -1);
                        string want = sim.owned[sim.target];
                        st.Frame(HandsDraw.SpecFor(cfg, want), tl, want, sim.prev, sim.now, 1, dt, respawnRaise, 1, sim.active || sim.slot != sim.target);
                        if (sim.ready && !wasReady)
                        {
                            Assert.That(st.held, Is.EqualTo(to));
                            AssertAtHold(st.pose, $"{to} x{mult} on the fire step");
                            seen = true;
                        }
                        wasReady = sim.ready;
                    }
                    Assert.That(seen, Is.True);
                }
            }
        }

        /// <summary>A second press mid-swap (to a third gun, or back): the shown gun flicks out from where it is and the
        /// wanted one rises; no frame jumps more than a clean swap's fastest, and the last gun is at hold when fire opens.</summary>
        [Test]
        public void ADoubleSwapFromThePressFlicksWithoutPops()
        {
            HandsSwapTimeline tl = HandsSwapTimeline.Of(swapTime, 1, lowerFraction, fireFraction, dt);
            string[] set = { "revolver", "rocket", "shotgun" };
            var tris = new Dictionary<string, List<Vector3>>();
            foreach (string g in set) tris[g] = RigTriangles(g);
            double maxY = 0, maxP = 0;
            foreach (int gap in new[] { 3, 6, 10, 14, 18, 22, 26 })
                foreach (int second in new[] { 0, 2 }) // back to the first gun, or on to a third
                {
                    var sim = new SwapSim { tl = tl, owned = set };
                    var st = new HandsDrawState();
                    double frameDt = 1 / UnityTuning.HandsTraceFps, acc = 0;
                    st.Frame(HandsDraw.SpecFor(cfg, set[0]), tl, set[0], 0, 0, 1, frameDt, respawnRaise, 1, false);
                    int step = 0;
                    string shown = set[0];
                    HandsDrawPose last = st.pose;
                    bool wasReady = true, seen = false;
                    double ms = 0, readyMs = -1, upMs = -1;
                    for (int f = 0; f < 150; f++)
                    {
                        acc += frameDt;
                        while (acc >= dt)
                        {
                            acc -= dt;
                            step++;
                            sim.Step(step == 1 ? 1 : step == 1 + gap ? second : -1);
                        }
                        string want = sim.owned[sim.target];
                        st.Frame(HandsDraw.SpecFor(cfg, want), tl, want, sim.prev, sim.now, acc / dt, frameDt, respawnRaise, 1, sim.active || sim.slot != sim.target);
                        Assert.That(InBox(st.held, tris[st.held], st.pose), Is.EqualTo(0), $"gap {gap} > {set[second]}: {st.held} in the centre box");
                        if (st.held == shown)
                        {
                            maxY = Math.Max(maxY, Math.Abs(st.pose.y - last.y));
                            maxP = Math.Max(maxP, Math.Abs(st.pose.pitchDeg - last.pitchDeg));
                            Assert.That(Math.Abs(st.pose.y - last.y), Is.LessThan(0.09), $"gap {gap} > {set[second]}: a y pop on {shown} at {ms:0.0} ms ({last.y:0.000} -> {st.pose.y:0.000}, {last.phase} -> {st.pose.phase}, lowered {st.pose.lowered:0.000}, sim t-ready {sim.ready} active {sim.active} now {sim.now:0.000})");
                            Assert.That(Math.Abs(st.pose.pitchDeg - last.pitchDeg), Is.LessThan(12), $"gap {gap} > {set[second]}: a pitch pop on {shown}");
                        }
                        ms += frameDt * 1000;
                        if (sim.ready && !wasReady && readyMs < 0) readyMs = ms;
                        if (readyMs >= 0 && upMs < 0 && st.held == set[second] && Math.Abs(st.pose.y) < 0.02 && Math.Abs(st.pose.pitchDeg) < 3) upMs = ms;
                        if (sim.ready && !wasReady && second == 2)
                        {
                            // on to a third gun: the sim's fire step is a full raise away: up on it
                            Assert.That(st.held, Is.EqualTo(set[2]), $"gap {gap}: the wanted gun when fire opens");
                            Assert.That(Math.Abs(st.pose.y), Is.LessThan(0.02), $"gap {gap} > {set[second]}: up when fire opens");
                            seen = true;
                        }
                        wasReady = sim.ready;
                        shown = st.held;
                        last = st.pose;
                    }
                    Assert.That(st.held, Is.EqualTo(sim.Weapon), $"gap {gap}: the sim's gun in hand at the end");
                    AssertAtHold(st.pose, $"gap {gap} > {set[second]} at the end");
                    Assert.That(seen || second == 0, Is.True, $"gap {gap}: fire opened on the third gun");
                    // called back: the sim may open fire at once (its mirror); the gun is back up within the world-time rise
                    if (second == 0 && readyMs >= 0)
                        Assert.That(upMs - readyMs, Is.LessThanOrEqualTo(UnityTuning.HandsRiseMinTime * 1000 + 2 * frameDt * 1000), $"gap {gap}: called back, up {upMs - readyMs:0.0} ms after fire opened");
                }
        }

        /// <summary>The KESTREL's kick is snappy: settled within 0.2 s, a small ring (round 1: "takes a long time to settle").</summary>
        [Test]
        public void TheKestrelSettlesFast()
        {
            KickTrace k = TraceKick(HandsKick.SpecFor("revolver"));
            Assert.That(k.settle, Is.LessThan(0.2), "settled");
            Assert.That(k.pitchMinAfter / k.pitchPeak, Is.GreaterThan(-0.25), "a small ring");
            Assert.That(k.pitchPeak, Is.GreaterThan(12), "still snaps up");
        }

        /// <summary>A punch that connects shows the fist at its impact frame on the contact frame (the sim's hit step is the
        /// kill's and the freeze's first); a whiff winds up and strikes.</summary>
        [Test]
        public void APunchThatConnectsIsOnItsImpactFrameAtContact()
        {
            HandsArmParams a = HandsArmParams.From(cfg);
            Assert.That(HandsPunch.StartAge(a, true), Is.EqualTo(a.outTime));
            Assert.That(HandsPunch.StartAge(a, false), Is.EqualTo(0));
            HandsArmPose p = Punch(a, HandsPunch.StartAge(a, true), true);
            Assert.That(p.visible, Is.True, "on screen on the contact frame");
            Assert.That(p.z, Is.EqualTo(a.punchZ - UnityTuning.HandsPunchOverZ).Within(1e-9), "at full extension, past the target");
            double age = HandsPunch.StartAge(a, true);
            for (int f = 0; f < 12; f++) age = HandsArm.AdvanceOut(age, 0, 1 / UnityTuning.HandsTraceFps, a.outTime);
            Assert.That(Punch(a, age, true).z, Is.EqualTo(p.z), "held through the freeze");
        }

        /// <summary>The fist reads as a fist: its block bigger than the forearm's cross-section, knuckles on its front.</summary>
        [Test]
        public void TheFistIsBiggerThanTheForearm()
        {
            HandsArmParams a = HandsArmParams.From(cfg);
            BlockyKit.ArmPart fist = BlockyKit.FistOf(a.parts), fore = BlockyKit.ForearmOf(a.parts);
            Assert.That(fist.sx * fist.sy, Is.GreaterThan(1.6 * fore.sx * fore.sy), "the fist's face vs the forearm's cross-section");
            var web = new MeshKit();
            BlockyKit.Arm(web, a.parts, Color.gray, Color.gray, Color.cyan, a.segments);
            var port = new MeshKit();
            BlockyKit.ArmFist(port, a.parts, Color.gray, Color.gray, Color.cyan, a.segments);
            // round 5: + a cyan edge along the sleeve (the sleeve in the hands' ink, not the floor's brown)
            Assert.That(((List<int>)KitT.GetValue(port)).Count, Is.GreaterThanOrEqualTo(((List<int>)KitT.GetValue(web)).Count + 7 * 36), "knuckle ridges, finger folds, the sleeve's cyan edge");
            // round 5 (n17-strip-slide: the bracing forearm filled the left quarter): no brace while sliding
            Assert.That(HandsArm.BraceWeight(1), Is.EqualTo(0), "the slide's brace is off");
            Assert.That(HandsArm.BraceWeight(0.5), Is.EqualTo(0));
        }

        /// <summary>The NDC of a rig's furthest vertex at the hold pose (its muzzle end), via the same projection as InBox.</summary>
        void MuzzleNdc(string gun, out double nx, out double ny) => MuzzleNdc(gun, true, out nx, out ny);

        /// <summary>The NDC of the THUNDERCLAP's muzzle point (BlockyKit's) at the hold pose; `turned` false: without its
        /// round-6 turn (the baseline the capture showed).</summary>
        void MuzzleNdc(string gun, bool turned, out double nx, out double ny)
        {
            Vector3 mu = BlockyKit.Thunderclap(new MeshKit(), Color.white, new MeshKit()); // Unity rig space
            mu = new Vector3(mu.x, mu.y, -mu.z); // web
            double holdX0 = cfg.Num("viewmodel.holdX"), holdY0 = cfg.Num("viewmodel.holdY"), holdZ0 = cfg.Num("viewmodel.holdZ");
            double holdPitch0 = cfg.Num("viewmodel.holdPitchDeg"), holdYaw0 = cfg.Num("viewmodel.holdYawDeg");
            string k0 = "viewmodel.kitOffsets." + gun;
            double kx0 = cfg.Num(k0 + ".x"), ky0 = cfg.Num(k0 + ".y"), kz0 = cfg.Num(k0 + ".z"), ks0 = cfg.Num(k0 + ".scale") * HandsDraw.Presence(gun);
            HandsDraw.HoldNudge(gun, out double nudX, out double nudY);
            HandsDraw.HoldTurn(gun, out double tP, out double tY);
            if (!turned) tP = tY = 0;
            double[,] kitM = Euler(tP * Deg, tY * Deg, 0, false);
            double[,] rootM = Euler(holdPitch0 * Deg, holdYaw0 * Deg, 0, true);
            Apply(kitM, mu.x * ks0, mu.y * ks0, mu.z * ks0, out double mx, out double my, out double mz);
            Apply(rootM, mx + kx0 + nudX, my + ky0 + nudY, mz + kz0, out double qx, out double qy, out double qz);
            qx += holdX0;
            qy += holdY0;
            qz += holdZ0;
            double tanY0 = Math.Tan(cfg.Num("viewmodel.fovDeg") * Deg / 2), tanX0 = tanY0 * 16 / 9;
            nx = qx / -qz / tanX0;
            ny = qy / -qz / tanY0;
        }

        /// <summary>Round 6 (n17-r5e horde: the THUNDERCLAP's barrel lay across the cluster at rest): at hold its muzzle ends
        /// right of and below the crosshair's central box (NDC +-0.1), with a margin.</summary>
        [Test]
        public void TheThunderclapsMuzzleRestsRightOfAndBelowTheCentre()
        {
            MuzzleNdc("shotgun", true, out double x, out double y);
            MuzzleNdc("shotgun", false, out double x0, out double y0);
            // the capture put the old rest muzzle at about (0.15, -0.1) on screen; the turn moves it well right and down
            Assert.That(x - x0, Is.GreaterThan(0.08), $"turned right (muzzle NDC {x0:0.000} -> {x:0.000})");
            Assert.That(y - y0, Is.LessThan(-0.08), $"and down (muzzle NDC {y0:0.000} -> {y:0.000})");
            Assert.That(x, Is.GreaterThan(0.2), $"right of the centre box (muzzle NDC {x:0.000}, {y:0.000})");
            Assert.That(y, Is.LessThan(-0.18), $"below it (muzzle NDC {x:0.000}, {y:0.000})");
            foreach (string g in KitGuns)
                if (g != "shotgun")
                {
                    HandsDraw.HoldTurn(g, out double tp, out double ty);
                    Assert.That(tp == 0 && ty == 0, Is.True, g + ": only the THUNDERCLAP turns");
                }
        }

        /// <summary>D-69: no kit gun's hold pose sits under the weapon card (H's, bottom right: x > 1500, y > 800 at 1080p).</summary>
        [Test]
        public void NoGunAtHoldSitsUnderTheWeaponCard()
        {
            double cardX = 1500.0 / 1920 * 2 - 1, cardY = 1 - 800.0 / 1080 * 2;
            foreach (string g in KitGuns)
            {
                Assert.That(ScreenBounds(g, RigTriangles(g), default, out double x0, out double x1, out double y0, out double y1), Is.True, g + " on screen");
                double ox = Math.Max(0, x1 - Math.Max(x0, cardX)), oy = Math.Max(0, Math.Min(y1, cardY) - y0);
                double share = ox * oy / ((x1 - x0) * (y1 - y0));
                Assert.That(share, Is.LessThan(0.3), $"{g}: {share:P0} of its bounds under the card (x {x0:0.00}..{x1:0.00}, y {y0:0.00}..{y1:0.00})");
                Assert.That(y1, Is.GreaterThan(cardY + 0.1), g + ": its top clears the card's top");
            }
        }

        // --- the slide pose (round 1, rubric row 4: boost, sprint and slide distinct) --------------------------------------

        /// <summary>The slide pose eases in over ~80 ms and out over ~120 ms (no pops), drops the gun and cants it toward the
        /// slide's lean, trembles by speed (none at rest), yields to a shot from its frame, and a slide-jump snaps it back up
        /// past rest (the overshoot) and settles; out of the centre box.</summary>
        [Test]
        public void TheSlidePoseEasesTremblesSnapsAndYields()
        {
            double frameDt = 1 / UnityTuning.HandsTraceFps;
            double inStep = 1.6 * frameDt / UnityTuning.HandsSlideInTime, outStep = 1.6 * frameDt / UnityTuning.HandsSlideOutTime;
            var m = new HandsMovePose();
            double last = 0, t = 0;
            for (; t < UnityTuning.HandsSlideInTime + frameDt; t += frameDt)
            {
                m.Frame(false, false, true, 14, 0, frameDt);
                Assert.That(m.slideW - last, Is.InRange(0.0, inStep), $"eases in at {t * 1000:0.0} ms");
                last = m.slideW;
            }
            Assert.That(m.slideW, Is.EqualTo(1).Within(1e-9), "in by ~80 ms");
            HandsDrawPose full = m.Sample(0, 1);
            Assert.That(full.y, Is.LessThan(-0.03), "the gun low");
            Assert.That(full.rollDeg, Is.GreaterThan(5), "canted toward the lean (straight ahead: the bias, to the left)");
            Assert.That(full.pitchDeg, Is.LessThan(0), "muzzle dips");
            // the tremor: frames differ at speed, not at rest
            m.Frame(false, false, true, 14, 0, frameDt);
            HandsDrawPose a1 = m.Sample(0, 1);
            m.Frame(false, false, true, 14, 0, frameDt);
            HandsDrawPose a2 = m.Sample(0, 1);
            Assert.That(Math.Abs(a1.x - a2.x) + Math.Abs(a1.y - a2.y), Is.GreaterThan(1e-4), "skidding at speed");
            Assert.That(Math.Abs(a1.y - full.y), Is.LessThan(2 * UnityTuning.HandsSlideTremorY * 1.01 + 1e-9), "a tremor, small (both samples carry it)");
            m.Frame(false, false, true, 0, 0, frameDt);
            HandsDrawPose r1 = m.Sample(0, 1);
            m.Frame(false, false, true, 0, 0, frameDt);
            HandsDrawPose r2 = m.Sample(0, 1);
            Assert.That(Math.Abs(r1.x - r2.x) + Math.Abs(r1.y - r2.y), Is.LessThan(1e-12), "no tremor at rest");
            // the lean follows the slide's side: sliding right leans right
            var right = new HandsMovePose();
            for (int f = 0; f < 30; f++) right.Frame(false, false, true, 0, 1, frameDt);
            Assert.That(right.Sample(0, 1).rollDeg, Is.LessThan(0), "a slide to the right cants right");
            // out
            last = m.slideW;
            for (t = 0; t < UnityTuning.HandsSlideOutTime + frameDt; t += frameDt)
            {
                m.Frame(false, false, false, 14, 0, frameDt);
                Assert.That(last - m.slideW, Is.InRange(0.0, outStep), $"eases out at {t * 1000:0.0} ms");
                last = m.slideW;
            }
            Assert.That(m.slideW, Is.EqualTo(0).Within(1e-9), "out by ~120 ms");
            // a shot yields it from its frame
            var s = new HandsMovePose();
            for (int f = 0; f < 30; f++) s.Frame(false, false, true, 10, 0, frameDt);
            double before = s.Sample(0, 1).y;
            s.Shot();
            s.Frame(false, false, true, 10, 0, frameDt);
            Assert.That(s.Sample(0, 1).y, Is.GreaterThan(before + 0.003), "yields on the shot's frame");
            for (t = 0; t < UnityTuning.HandsPoseYieldTime + frameDt; t += frameDt) s.Frame(false, false, true, 10, 0, frameDt);
            Assert.That(Math.Abs(s.Sample(0, 1).y) + Math.Abs(s.Sample(0, 1).rollDeg), Is.LessThan(1e-9), "the gun up to fire");
            // a slide-jump: snaps back up past rest, then settles at rest
            var j = new HandsMovePose();
            for (int f = 0; f < 30; f++) j.Frame(false, false, true, 10, 0, frameDt);
            double prevY = j.Sample(0, 1).y, maxY = double.NegativeInfinity;
            j.SlideJump();
            for (t = 0; t < UnityTuning.HandsSlideSnapTime + 3 * frameDt; t += frameDt)
            {
                j.Frame(false, false, false, 10, 0, frameDt);
                double y = j.Sample(0, 1).y;
                Assert.That(Math.Abs(y - prevY), Is.LessThan(0.025), $"no pop at {t * 1000:0.0} ms");
                maxY = Math.Max(maxY, y);
                prevY = y;
            }
            Assert.That(maxY, Is.GreaterThan(0.001), "snaps back up past rest");
            Assert.That(j.slideW, Is.EqualTo(0).Within(1e-9), "settled by the snap's end");
            Assert.That(j.Sample(0, 1).y, Is.EqualTo(0).Within(1e-9));
            // out of the centre box: full slide, both leans, the tremor's extremes (sampled over a second)
            foreach (string gun in KitGuns)
            {
                List<Vector3> tris = RigTriangles(gun);
                foreach (double lateral in new[] { -1.0, 0.0, 1.0 })
                {
                    var c = new HandsMovePose();
                    for (int f = 0; f < 144; f++)
                    {
                        c.Frame(false, false, true, 20, lateral, frameDt);
                        if (f % 6 == 0) Assert.That(InBox(gun, tris, c.Sample(0, 1)), Is.EqualTo(0), $"{gun} sliding (lateral {lateral}) frame {f}");
                    }
                }
            }
        }

        /// <summary>The free hand braces toward the floor while sliding (at the screen's bottom-left edge), eased by the
        /// slide's weight; a punch still lands where it does; out of the centre box.</summary>
        [Test]
        public void TheFreeHandBracesTowardTheFloorWhileSliding()
        {
            HandsArmParams a = HandsArmParams.From(cfg);
            HandsArmPose rest = HandsPunch.Pose(a, double.PositiveInfinity, false, 0, 0, 0, double.PositiveInfinity, 0);
            Assert.That(rest.visible, Is.False, "no slide: hidden (the web's arm at rest)");
            HandsArmPose brace = HandsPunch.Pose(a, double.PositiveInfinity, false, 0, 0, 0, double.PositiveInfinity, 1);
            Assert.That(brace.visible, Is.True, "sliding: the hand shows");
            Assert.That(brace.x, Is.EqualTo(UnityTuning.HandsBraceX).Within(1e-12));
            Assert.That(brace.y, Is.EqualTo(UnityTuning.HandsBraceY).Within(1e-12));
            Assert.That(brace.pitchDeg, Is.EqualTo(UnityTuning.HandsBracePitchDeg).Within(1e-12));
            HandsArmPose half = HandsPunch.Pose(a, double.PositiveInfinity, false, 0, 0, 0, double.PositiveInfinity, 0.5);
            Assert.That(half.y, Is.EqualTo((a.restY + UnityTuning.HandsBraceY) / 2).Within(1e-12), "eased by the slide's weight");
            HandsArmPose punch = HandsPunch.Pose(a, a.outTime, true, 0, 0, 0, double.PositiveInfinity, 1);
            Assert.That(punch.z, Is.EqualTo(a.punchZ - UnityTuning.HandsPunchOverZ).Within(1e-9), "a punch mid-slide lands where it does");
            var kit = new MeshKit();
            BlockyKit.ArmFist(kit, a.parts, Color.gray, Color.gray, Color.cyan, a.segments);
            var v = (List<Vector3>)KitV.GetValue(kit);
            var tt = (List<int>)KitT.GetValue(kit);
            double tanY = Math.Tan(cfg.Num("viewmodel.fovDeg") * Deg / 2), tanX = tanY * 16 / 9, near = cfg.Num("viewmodel.near");
            var cx = new double[3];
            var cy = new double[3];
            var cz = new double[3];
            bool onScreen = false;
            foreach (double w in new[] { 0.3, 0.6, 1.0 })
            {
                HandsArmPose p = HandsPunch.Pose(a, double.PositiveInfinity, false, 0, 0, 0, double.PositiveInfinity, w);
                double[,] m = Euler(p.pitchDeg * Deg, p.yawDeg * Deg, p.rollDeg * Deg, true);
                for (int i = 0; i + 2 < tt.Count; i += 3)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        Vector3 u = v[tt[i + k]];
                        Apply(m, u.x, u.y, -u.z, out double ox, out double oy, out double oz);
                        cx[k] = ox + p.x;
                        cy[k] = oy + p.y;
                        cz[k] = oz + p.z;
                        if (w == 1.0 && cz[k] < -near && Math.Abs(cx[k] / -cz[k] / tanX) < 1 && Math.Abs(cy[k] / -cz[k] / tanY) < 1) onScreen = true;
                    }
                    Assert.That(ClippedOverlaps(cx, cy, cz, near, tanX, tanY), Is.False, $"the bracing hand at {w} in the centre box");
                }
            }
            Assert.That(onScreen, Is.True, "the bracing hand is on screen");
            // round 2: the fist reads above the HP card (H's, bottom left: its top at NDC y ~ -0.69 in the r2 strips)
            Assert.That(brace.y / -brace.z / tanY, Is.GreaterThan(-0.6), "the bracing fist above the HP card");
            Assert.That(brace.x / -brace.z / tanX, Is.GreaterThan(-0.6), "and in from the corner");
        }

        // --- round 3, beyond the ask (D-70): the style aura on the gun in hand, a cyan value ladder by rank ----------------

        [Test]
        public void TheStyleAuraClimbsTheCyanLadder()
        {
            Color cyan = Look.C(cfg, "colors.usable");
            cyan.a = 1;
            Color pale = Color.Lerp(cyan, Color.white, UnityTuning.ThemeCyanPaleMix), hot = Color.Lerp(cyan, Color.white, UnityTuning.ThemeCyanHotMix);
            int count = cfg.Arr("style.ranks").Count;
            Assert.That(count, Is.GreaterThanOrEqualTo(4), "style.ranks");
            Assert.That(HandsAura.Sample(0, count, 0, false, pale, cyan, hot).intensity, Is.EqualTo(0), "no style: no aura");
            Hsv(cyan, out double hc, out _, out _);
            double last = 0;
            for (int r = 1; r < count; r++)
            {
                HandsAuraLook a = HandsAura.Sample(r, count, 0, true, pale, cyan, hot); // REDUCED MOTION: no pulse, the glow stays
                Assert.That(a.intensity, Is.GreaterThan(last), $"rank {r} brighter than {r - 1}");
                last = a.intensity;
                Hsv(a.color, out double h, out double s, out _);
                Assert.That(Math.Abs(h - hc), Is.LessThan(0.01), $"rank {r}: the cyan family (never red-orange or magenta)");
            }
            HandsAuraLook low = HandsAura.Sample(1, count, 0, true, pale, cyan, hot), top = HandsAura.Sample(count - 1, count, 0, true, pale, cyan, hot);
            HandsAuraLook mid = HandsAura.Sample((count - 1 + 1) / 2, count, 0, true, pale, cyan, hot);
            Assert.That(low.color.r, Is.EqualTo(pale.r).Within(1e-4), "pale at the bottom");
            Assert.That(top.color.r, Is.EqualTo(hot.r).Within(1e-4), "CYAN-HOT at the top");
            Hsv(mid.color, out _, out double sm, out _);
            Hsv(cyan, out _, out double sc, out _);
            Assert.That(Math.Abs(sm - sc), Is.LessThan(0.2), "about CYAN in the middle");
            // the top rank pulses slowly; REDUCED MOTION stills the pulse (the glow stays); the ranks below don't pulse
            double p0 = HandsAura.Sample(count - 1, count, 0.0, false, pale, cyan, hot).intensity, p1 = HandsAura.Sample(count - 1, count, 0.3, false, pale, cyan, hot).intensity;
            Assert.That(Math.Abs(p0 - p1), Is.GreaterThan(0.05), "the top rank pulses");
            Assert.That(HandsAura.Sample(count - 1, count, 0.3, true, pale, cyan, hot).intensity, Is.EqualTo(top.intensity), "REDUCED MOTION: no pulse");
            Assert.That(HandsAura.Sample(count - 2, count, 0.0, false, pale, cyan, hot).intensity, Is.EqualTo(HandsAura.Sample(count - 2, count, 0.3, false, pale, cyan, hot).intensity), "no pulse below the top");
            // set only on change
            var set = new HandsAuraSetter();
            Assert.That(set.Changed(cyan * 1.2f), Is.True, "the first");
            Assert.That(set.Changed(cyan * 1.2f), Is.False, "the same colour: no set");
            Assert.That(set.Changed(cyan * 1.3f), Is.True, "a new rank's");
        }

        // --- U3-30.10: the kill flourish (a multikill or a rank-up, once the shooting pauses) -------------------------------

        /// <summary>The trigger: a MULTIKILL award or a rank-up arms it; it plays once the shooting has paused
        /// HandsFlourishQuiet s, once; a shot / swing / punch (Cancel) or a swap stops it on that frame; an armed one waits
        /// out the shots and expires after HandsFlourishPendingMax.</summary>
        [Test]
        public void TheKillFlourishWaitsForAPauseAndYields()
        {
            double frameDt = 1 / UnityTuning.HandsTraceFps;
            Assert.That(HandsFlourish.IsTrigger("MULTIKILL"), Is.True);
            Assert.That(HandsFlourish.IsTrigger("HEADSHOT"), Is.False);
            Assert.That(HandsFlourish.IsRankUp(2, 3), Is.True);
            Assert.That(HandsFlourish.IsRankUp(3, 2), Is.False, "a rank down: none");
            HandsDrawSpec spec = HandsDraw.SpecFor(cfg, "revolver");
            double dur = HandsFlourish.Duration(spec, false);
            Assert.That(dur, Is.InRange(0.3, 0.5), "300-500 ms");
            var f = new HandsFlourish();
            f.Cancel(); // the killing shot
            f.Arm();
            int starts = 0;
            double startedAt = -1, t = 0;
            for (; t < 0.3; t += frameDt)
            {
                if (t % 0.1 < frameDt) f.Cancel(); // still shooting
                f.Frame(false, frameDt, dur);
                if (f.started) starts++;
            }
            Assert.That(starts, Is.EqualTo(0), "not while shooting");
            double lastShot = t;
            f.Cancel();
            for (; t < 2; t += frameDt)
            {
                f.Frame(false, frameDt, dur);
                if (f.started)
                {
                    starts++;
                    startedAt = t - lastShot;
                }
            }
            Assert.That(starts, Is.EqualTo(1), "once");
            Assert.That(startedAt, Is.InRange(UnityTuning.HandsFlourishQuiet - frameDt, UnityTuning.HandsFlourishQuiet + 2 * frameDt), "after the pause");
            // a shot mid-flourish: stopped on that frame
            var g = new HandsFlourish();
            g.Arm();
            g.Frame(false, frameDt, dur);
            Assert.That(g.started, Is.True, "no shots before: at once");
            for (int i = 0; i < 10; i++) g.Frame(false, frameDt, dur);
            Assert.That(HandsFlourish.Playing(g.age, dur), Is.True);
            g.Cancel();
            Assert.That(HandsFlourish.Playing(g.age, dur), Is.False, "a shot stops it on its frame");
            // a swap: stopped, and never started while swapping
            var h = new HandsFlourish();
            h.Arm();
            h.Frame(false, frameDt, dur);
            h.Frame(true, frameDt, dur);
            Assert.That(HandsFlourish.Playing(h.age, dur), Is.False, "a swap stops it");
            var k = new HandsFlourish();
            k.Arm();
            for (int i = 0; i < 30; i++)
            {
                k.Frame(true, frameDt, dur);
                Assert.That(k.started, Is.False, "not while swapping");
            }
            // expiry: armed, the shooting never pauses long enough, then pauses after the window: nothing
            var e = new HandsFlourish();
            e.Arm();
            for (t = 0; t < UnityTuning.HandsFlourishPendingMax + 0.1; t += frameDt)
            {
                if (t % 0.2 < frameDt) e.Cancel();
                e.Frame(false, frameDt, dur);
            }
            for (int i = 0; i < 200; i++)
            {
                e.Frame(false, frameDt, dur);
                Assert.That(e.started, Is.False, "expired");
            }
        }

        /// <summary>Each gun's flourish does its own thing, starts and ends at hold (no pop), stays out of the centre box;
        /// REDUCED MOTION shortens it to a still pose-pop (no spin, no chatter).</summary>
        [Test]
        public void TheKillFlourishesDoTheirThingsAndEndAtHold()
        {
            double frameDt = 1 / UnityTuning.HandsTraceFps;
            foreach (string gun in KitGuns)
            {
                HandsDrawSpec s = HandsDraw.SpecFor(cfg, gun);
                List<Vector3> tris = RigTriangles(gun);
                double dur = HandsFlourish.Duration(s, false);
                Assert.That(dur, Is.InRange(0.3, 0.5), gun + " 300-500 ms");
                Assert.That(OffHold(HandsFlourish.Sample(s, 0, false, 1)), Is.False, gun + ": starts at hold");
                Assert.That(OffHold(HandsFlourish.Sample(s, dur, false, 1)), Is.False, gun + ": ends at hold");
                Assert.That(OffHold(HandsFlourish.Sample(s, double.PositiveInfinity, false, 1)), Is.False, gun + ": none");
                double spin = 0, pump = 0, glow = 0, clench = 0, move = 0;
                HandsDrawPose last = HandsFlourish.Sample(s, 0, false, 1);
                for (double a = 0; a < dur; a += frameDt)
                {
                    HandsDrawPose p = HandsFlourish.Sample(s, a, false, 1);
                    spin = Math.Max(spin, Math.Abs(p.spinRad));
                    pump = Math.Max(pump, p.pump);
                    glow = Math.Max(glow, p.glow);
                    clench = Math.Max(clench, p.clench);
                    move = Math.Max(move, Math.Abs(p.rollDeg) + Math.Abs(p.pitchDeg) + 100 * Math.Abs(p.y));
                    Assert.That(Math.Abs(p.y - last.y), Is.LessThan(0.02), $"{gun}: no y pop at {a:0.000}");
                    Assert.That(InBox(gun, tris, p), Is.EqualTo(0), $"{gun} flourish at {a:0.000} in the centre box");
                    last = p;
                }
                switch (gun)
                {
                    case "revolver": Assert.That(spin, Is.GreaterThan(Math.PI), "the KESTREL's cylinder spins"); break;
                    case "shotgun": Assert.That(pump, Is.GreaterThan(0.9), "the THUNDERCLAP racks"); break;
                    case "rail": Assert.That(glow, Is.GreaterThan(0.9), "the LONGWAVE hums"); break;
                    case "gauntlet": Assert.That(clench, Is.GreaterThan(0.9), "the GAUNTLET's knuckles crack"); break;
                    default: Assert.That(move, Is.GreaterThan(5), gun + " moves"); break;
                }
                double still = HandsFlourish.Duration(s, true);
                Assert.That(still, Is.LessThanOrEqualTo(0.25), gun + ": REDUCED MOTION shortens it");
                for (double a = 0; a < still; a += frameDt)
                {
                    HandsDrawPose p = HandsFlourish.Sample(s, a, true, UnityTuning.HandsReducedMotionScale);
                    Assert.That(p.spinRad, Is.EqualTo(0), gun + ": no spin");
                    Assert.That(Math.Abs(p.rollDeg), Is.LessThanOrEqualTo(10), gun + ": a still pose-pop");
                }
            }
        }

        // --- the per-frame path allocates nothing --------------------------------------------------------------------------

        TestDelegate FramePath(out Func<double> sink)
        {
            HandsSwapTimeline tl = HandsSwapTimeline.Of(swapTime, 1, lowerFraction, fireFraction, dt);
            HandsDrawSpec a = HandsDraw.SpecFor(cfg, "revolver"), b = HandsDraw.SpecFor(cfg, "rail");
            HandsArmParams arm = HandsArmParams.From(cfg);
            var st = new HandsDrawState();
            var gate = new HandsFlashGate();
            var glint = new HandsGlint[1];
            var idle = new HandsIdle();
            var rclock = new HandsRecoilClock();
            var knuckles = new HandsKnuckles();
            var glowGate = new HandsGlowGate();
            var fgate = new FlashGate();
            var hold = new HandsImpactHold(cfg);
            var hit = new HitEvent { melee = true, momentum = 2 };
            // NIGHT17 U3-30.6: the poses, the per-gun kicks, the punch, the glint, the breath
            var move = new HandsMovePose();
            var kbank = new SpringBank(UnityTuning.HandsKickSlots, HandsKick.Snaps());
            HandsKickSpec ks = HandsKick.SpecFor("nailgun"), kr = HandsKick.SpecFor("revolver");
            var kscratch = new double[HandsKick.Channels];
            var flourish = new HandsFlourish();
            double total = 0;
            double rr = respawnRaise, step = dt;
            sink = () => total;
            return () =>
            {
                double prev = 0;
                for (int t = 1; t <= tl.total + 60; t++)
                {
                    double now = CombatMath.SwapLowered(t, tl.lower, tl.total);
                    bool changed = t >= tl.lower;
                    st.Frame(changed ? b : a, tl, changed ? "rail" : "revolver", prev, now, 0.5, step, rr, 1);
                    HandsArmPose p = HandsArm.Pose(arm, t * step, 0.001, 0.001);
                    glint[0].Step(t < 20 ? 0.5 : 0, 3, 0.6, 0.22, step);
                    gate.Allow(t * step);
                    idle.Frame(t * 0.2, true, step); // crosses 8 s: a flourish starts
                    if (t % 20 == 1)
                    {
                        rclock.Kick(0.03);
                        knuckles.Parry(t % 40 == 1, 1);
                    }
                    total += rclock.Step(t % 3 == 0 ? 0 : step, step);
                    hit.shot = t / 10; // a new swing every 10 steps (the same one between: once per shot)
                    hold.OnHit(hit);
                    hold.Advance(step);
                    if (hold.Active) total += hold.Value(0) + hold.Value(3);
                    knuckles.Frame(t * step % 0.35, 0.06, 0.35, step, fgate, t * 0.4);
                    total += knuckles.Value(0.16) + glowGate.Frame(t % 25 == 0, t % 25 > 3 ? 0.5 : 0, 0.8, fgate, t * 0.4);
                    total += st.pose.pitchDeg + p.x + glint[0].Value(0.22, 1) + HandsIdle.Sample(a, idle.age, 1).rollDeg + HandsArm.AdvanceOut(t * step, 0, step, 0.06);
                    move.Frame(t % 50 < 30, t % 40 < 5, t % 60 < 25, t * 0.1, t % 3 - 1, step);
                    if (t % 33 == 0) move.Shot();
                    if (t % 60 == 25) move.SlideJump();
                    total += HandsAura.Sample(t % 6, 6, t * step, t % 2 == 0, Color.white, Color.cyan, Color.white).intensity;
                    if (t % 70 == 0) flourish.Arm();
                    if (t % 33 == 0) flourish.Cancel();
                    flourish.Frame(t % 50 < 3, step, 0.4);
                    total += HandsFlourish.Sample(t % 2 == 0 ? a : b, flourish.age, t % 3 == 0, 1).rollDeg;
                    total += HandsPunch.Pose(arm, t * step % 0.35, false, 0, 0, 0, double.PositiveInfinity, move.slideW).y;
                    if (t % 45 == 0) move.Dash(0.5, 0.5, 1);
                    if (t % 7 == 0) HandsKick.Add(kbank, t % 14 == 0 ? ks : kr, t, 1, kscratch);
                    kbank.Update(step);
                    HandsArmPose pp = HandsPunch.Pose(arm, t * step % 0.35, t % 2 == 0, 0.001, 0.001, 0, double.PositiveInfinity);
                    total += move.Sample(t * 0.1, 1).x + kbank.Sample(0) + pp.z + HandsQuickdraw.Amount(st.QuickdrawAge) + HandsQuickdraw.Along(t * step % 0.5)
                        + HandsBreath.Sample(t * step, 3.5, 1).pitchDeg + HandsEase.OutBack(t * 0.01 % 1, 1.7);
                    prev = now;
                }
                st.Cancel();
                st.Respawn();
            };
        }

        [Test]
        public void TheDrawFrameAllocatesNothing()
        {
            TestDelegate run = FramePath(out _);
            run(); // warm: the JIT
            Assert.That(run, AllocIs.Not.AllocatingGCMemory());
        }

        /// <summary>The same check by the managed counter (runs under dotnet too).</summary>
        [Test]
        public void TheDrawFrameAllocatesNothingByTheGcCounter()
        {
            TestDelegate run = FramePath(out _);
            run();
            run();
            long before = GC.GetAllocatedBytesForCurrentThread();
            run();
            long after = GC.GetAllocatedBytesForCurrentThread();
            Assert.That(after - before, Is.EqualTo(0));
        }

        // --- the viewmodel trace (the lead reads it: Logs/n16-vm-trace.csv) -----------------------------------------------

        [Test]
        public void WritesTheViewmodelTrace()
        {
            string path = Path.Combine(ProjectRoot(), "Logs", UnityTuning.HandsTraceFile);
            int rows = HandsDraw.WriteTrace(cfg, path);
            Assert.That(rows, Is.GreaterThan(KitGuns.Length * 40), "every gun's swap at 144 fps");
            string[] lines = File.ReadAllLines(path);
            Assert.That(lines[0], Is.EqualTo("frame,ms,weapon,lowered,phase,px,py,pz,rx,ry,rz,spin,pump,sweep,glow,clench,fireOpen"));
            Assert.That(lines.Length, Is.EqualTo(rows + 1), path);
        }
    }
}
