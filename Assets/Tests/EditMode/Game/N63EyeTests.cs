// NIGHT63 builder D (DECIDE63 D-10, M1's proposal 2: THE EASED EYE; view row design.n63.eye63, OFF; Eye63Math.N63D.cs, CameraFeel.cs,
// UnityTuning.N63Eye.cs; Logs/n63/m1.md). GG 2026-10-20: "the movement needs to feel cleaner, more fluid, and less choppy". M1 measured the
// camera's designed events: the slide's eye ease (0.70 m in 0.10 s, CameraFeel's MoveTowards), the slide dip and the landing dip start and stop
// at full speed in one frame. Pinned, on a 60 fps path built from the game's own functions (the web's eye ease, CrunchMath.Tilt's slide dip,
// CameraMove.LandDip's landing dip) and n63-m1-chop.cjs's measure (the residual against a centred 9-frame quadratic Savitzky-Golay fit):
//   1. the row: design.n63.eye63 is a VIEW row (a _why, enabled a bool, an enabled_why, a numbers object, no sim key: no tape bit), the
//      checker clean, the two config copies byte-identical; off under -skygrindOff63 (or older) and -skygrindNoEye63 (any case); read every
//      frame (NightRows.Live: D1's held off frame). Its position is the row script's check at its own run (Tools/night/n63-d-eye63-row.cjs
//      throws unless eye63 is design.n63's last key after its write), never pinned here: a later row's append moves it (V's exit63, L-065)
//   2. the follow: a held frame (dt 0) moves nothing; a cut snaps to the standing eye at rest; it ends exactly where today's eye ends
//   3. the measure: the residual's peak halved at a slide's entry (with the slide dip), at its exit and at a landing dip (3 to 48 m/s falls);
//      the start and the stop eased (the largest frame-to-frame change of the per-frame step halved)
//   4. not a slower crouch: half-way within one frame of today's, 90 % of the exit within one frame, the drop 83 ms into a slide within 3 cm
//      of today's; the landing dip keeps 85 % of its depth or more
//   5. the hook: CameraFeel steps the follow every frame (before the row is read), reads the row every frame, draws today's exact expression
//      with the row off, cuts the follow on a respawn and decides the row once at Wire
// Written test first (NIGHT63.md section 4): RED on its commit (Eye63Math's placeholders: the follow is today's path, the switch never on; no
// row in the config; no hook).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using E = Skygrind.Game.Eye63Math;

namespace Skygrind.Game.Tests
{
    public class N63EyeTests
    {
        const double H = 1.0 / 60; // the reel's fixed clock (verbs37: one frame line per 1/60 s)
        static readonly double[] SG = { -21 / 231.0, 14 / 231.0, 39 / 231.0, 54 / 231.0, 59 / 231.0, 54 / 231.0, 39 / 231.0, 14 / 231.0, -21 / 231.0 };

        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        /// <summary>A path at 60 fps through CameraFeel.Apply's arithmetic: today's eye less the dips, and the eased eye's follow of the same.
        /// crouched from frame c0 to c1 (c0 &lt; 0: never); the slide's dip starts at frame c0; a landing at frame land with fall speed `fall`.</summary>
        static (double[] today, double[] eased) Run(int n, int c0, int c1, int land = -1, double fall = 0)
        {
            GameConfig cfg = Cfg();
            double stand = cfg.Num("body.eyeHeight"), drop = cfg.Num("slide.cameraDrop"), ease = cfg.Num("camera.eyeEaseTime");
            double depth = CameraMove.LandDepth(fall, UnityTuning.LandDipPerSpeedN18, UnityTuning.LandDipMaxN18);
            double eye = stand, slideAge = double.MaxValue, landAge = double.MaxValue;
            var f = new Eye63Follow(stand);
            var today = new double[n];
            var eased = new double[n];
            for (int i = 0; i < n; i++)
            {
                if (i == c0) slideAge = 0;
                if (i == land) landAge = 0;
                double target = c0 >= 0 && i >= c0 && i < c1 ? stand - drop : stand;
                eye = E.MoveTowards(eye, target, (drop / ease) * H);
                if (landAge < double.MaxValue) landAge = landAge >= UnityTuning.LandDipAttackN18 + UnityTuning.LandDipBackN18 ? double.MaxValue : landAge + H;
                double dip = landAge < double.MaxValue ? CameraMove.LandDip(landAge, depth, UnityTuning.LandDipAttackN18, UnityTuning.LandDipBackN18) : 0;
                if (slideAge < double.MaxValue) slideAge += H;
                if (slideAge < UnityTuning.SlideDipIn + UnityTuning.SlideDipOut)
                    dip += UnityTuning.SlideDip * CrunchMath.Tilt((float)slideAge, (float)UnityTuning.SlideDipIn, (float)UnityTuning.SlideDipOut);
                today[i] = eye - dip;
                eased[i] = f.Step(target, drop, ease, dip, H);
            }
            return (today, eased);
        }

        /// <summary>n63-m1-chop.cjs's measure: the largest |residual| against the centred 9-frame quadratic fit, frames a..b-1 (m).</summary>
        static double Residual(double[] p, int a, int b)
        {
            double best = 0;
            for (int i = Math.Max(4, a); i < Math.Min(p.Length - 4, b); i++)
            {
                double s = 0;
                for (int k = 0; k < 9; k++) s += SG[k] * p[i - 4 + k];
                best = Math.Max(best, Math.Abs(p[i] - s));
            }
            return best;
        }

        /// <summary>The largest change of the per-frame step (the jump in speed a frame shows), frames a..b-1 (m per frame per frame).</summary>
        static double Jerk(double[] p, int a, int b)
        {
            double best = 0;
            for (int i = Math.Max(1, a); i < Math.Min(p.Length - 1, b); i++) best = Math.Max(best, Math.Abs(p[i + 1] - 2 * p[i] + p[i - 1]));
            return best;
        }

        /// <summary>Frames after `from` until the path first passes `level` (1: the first frame of the change).</summary>
        static int Reach(double[] p, int from, double level, bool down)
        {
            for (int i = from; i < p.Length; i++) if (down ? p[i] <= level : p[i] >= level) return i - from + 1;
            return int.MaxValue;
        }

        const int S0 = 30, S1 = 110, N = 200; // a slide from frame 30 to 110 (1.33 s crouched), 3.3 s in all

        [Test]
        public void Row_AViewRow_OffUnderItsSwitches_TheScriptAppendsItLastAtItsRun()
        {
            GameConfig c = Cfg();
            const string at = "design.n63.eye63";
            Assert.That(c.Has(at + "._why") && c.Get(at + "._why") is string w && w.Length > 0, Is.True, at + "._why");
            Assert.That(c.Get(at + ".enabled") is bool, Is.True, at + ".enabled is a bool (false in the repo; a freeze may turn it on in its build's copy)");
            Assert.That(c.Has(at + ".enabled_why") && c.Get(at + ".enabled_why") is string ew && ew.Length > 0, Is.True, at + ".enabled_why");
            Assert.That(c.Get(at + ".numbers") is Dictionary<string, object>, Is.True, at + ".numbers is an object (its numbers are Unity-only: UnityTuning.N63Eye.cs)");
            Assert.That(c.Has(at + ".sim"), Is.False, "a view row: no sim, no tape bit");
            Assert.That(NightRows.From(c).IndexOf(E.Row), Is.EqualTo(-1), "no tape bit");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
            // NIGHT63 (the lead, after V's exit63 was appended after eye63 by V's own row script, L-065): the row's POSITION is not pinned here. A
            // position pin breaks on every later append; "the last key" holds only at the script's own run, so the script checks it there
            // (it throws unless eye63 is design.n63's last key after its write, and refuses a second run).
            string script = File.ReadAllText(Path.Combine(Repo(), "Tools", "night", "n63-d-eye63-row.cjs"));
            Assert.That(script, Does.Contain("if (nk[nk.length - 1] !== \"eye63\") throw new Error(\"eye63 is not design.n63's last key\");"), "the row script's own self-check: the last key at its run");
            Assert.That(script, Does.Contain("if (before.design.n63.eye63) throw new Error(\"design.n63.eye63 already exists (already run?)\");"), "a second run refused");
            string a = File.ReadAllText(Path.Combine(Repo(), "Design", "unity-config.json"));
            string b = File.ReadAllText(Path.Combine(Repo(), "Assets", "StreamingAssets", "Design", "unity-config.json"));
            Assert.That(a == b, Is.True, "the two config copies byte-identical");
            int saved = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = 63;
                Assert.That(NightRows.ViewOn(c, E.Row), Is.False, "off under -skygrindOff63");
            }
            finally { NightRows.ForceOffFrom = saved; }
        }

        [Test]
        public void Switch_TheRowAndItsSwitches_ReadEveryFrame()
        {
            Assert.AreEqual("-skygrindNoEye63", E.NoSwitch);
            Assert.That(E.Decide(false, new string[0]), Is.False, "the row off");
            Assert.That(E.Decide(true, new string[0]), Is.True, "the row on, no switch");
            Assert.That(E.Decide(true, new[] { "-skygrindOff63" }), Is.False);
            Assert.That(E.Decide(true, new[] { "-skygrindOff61" }), Is.False, "an older night's switch covers it");
            Assert.That(E.Decide(true, new[] { "-skygrindOff64" }), Is.True, "a later night's leaves it on");
            Assert.That(E.Decide(true, new[] { "-skygrindNoEye63" }), Is.False);
            Assert.That(E.Decide(true, new[] { "-SKYGRINDNOEYE63" }), Is.False, "the one-row switch ignores case (NightRows.HasSwitch)");
            Assert.That(E.Decide(true, new[] { "-skygrindNoColours63" }), Is.True, "another row's switch leaves it on");
            NightRows.ReelOverride.Clear();
            try
            {
                NightRows.ReelOverride[E.Row] = false;
                Assert.That(NightRows.Live(E.Decide(true, new string[0]), E.Row), Is.False, "D1's held off frame");
            }
            finally { NightRows.ReelOverride.Clear(); }
            Assert.That(E.Line(true, true), Does.Contain("[eye63]").And.Contain("ON"));
            Assert.That(E.Line(false, true), Does.Contain("[eye63]").And.Contain("OFF"));
            Assert.That(E.Line(false, false), Does.Contain("[eye63]").And.Contain("OFF"));
            Assert.That(E.FiredLine(1.2, 1.1, 42), Does.Contain("[eye63] fired"));
        }

        [Test]
        public void Follow_AHeldFrameMovesNothing_ACutRests_ItEndsWhereTodayEnds()
        {
            var f = new Eye63Follow(1.6);
            for (int i = 0; i < 4; i++) f.Step(0.9, 0.7, 0.1, 0.05, H);
            double y = f.Y, v = f.V, e = f.Eye;
            Assert.That(y, Is.LessThan(1.6), "it moved");
            Assert.That(f.Step(0.9, 0.7, 0.1, 0.05, 0), Is.EqualTo(y), "a held frame (dt 0): the same eye");
            Assert.That(f.Y == y && f.V == v && f.Eye == e, Is.True, "a held frame: no state moves (D1's held frames)");
            f.Cut(1.6);
            Assert.That(f.Y == 1.6 && f.V == 0 && f.Eye == 1.6, Is.True, "a cut: the standing eye at rest");
            var (today, eased) = Run(N, S0, S1);
            Assert.That(eased[S1 - 1], Is.EqualTo(today[S1 - 1]).Within(1e-6), "crouched: where today's eye ends");
            Assert.That(eased[N - 1], Is.EqualTo(today[N - 1]).Within(1e-6), "standing again: where today's eye ends");
        }

        [Test]
        public void TheMeasure_HalvedAtTheSlidesEntryAndExit_TheStartAndStopEased()
        {
            var (today, eased) = Run(N, S0, S1);
            double rt = Residual(today, S0 - 5, S0 + 14), re = Residual(eased, S0 - 5, S0 + 14);
            Assert.That(re, Is.LessThanOrEqualTo(rt / 2), $"the slide's entry with its dip: {re * 100:0.00} cm against today's {rt * 100:0.00}");
            rt = Residual(today, S1 - 5, S1 + 14);
            re = Residual(eased, S1 - 5, S1 + 14);
            Assert.That(re, Is.LessThanOrEqualTo(rt / 2), $"the slide's exit: {re * 100:0.00} cm against today's {rt * 100:0.00}");
            double jt = Jerk(today, S0 - 3, S0 + 14), je = Jerk(eased, S0 - 3, S0 + 14);
            Assert.That(je, Is.LessThanOrEqualTo(jt / 2), $"the entry's start and stop: {je * 100:0.00} cm / frame^2 against {jt * 100:0.00}");
            jt = Jerk(today, S1 - 3, S1 + 14);
            je = Jerk(eased, S1 - 3, S1 + 14);
            Assert.That(je, Is.LessThanOrEqualTo(jt / 2), $"the exit's start and stop: {je * 100:0.00} cm / frame^2 against {jt * 100:0.00}");
        }

        [Test]
        public void TheMeasure_HalvedAtALandingDip_EveryFall()
        {
            foreach (double fall in new[] { 3.0, 8.15, 13.5, 48.0 }) // a hop, verbs37's 8.15 m/s landings, the dip's cap, the slam's 48 m/s
            {
                var (today, eased) = Run(N, -1, -1, S0, fall);
                double rt = Residual(today, S0 - 5, S0 + 20), re = Residual(eased, S0 - 5, S0 + 20);
                Assert.That(rt, Is.GreaterThan(0.005), "today's dip is a measurable step at " + fall + " m/s");
                Assert.That(re, Is.LessThanOrEqualTo(rt / 2), $"a landing at {fall} m/s: {re * 100:0.00} cm against today's {rt * 100:0.00}");
                double dt = 1.6 - Min(today), de = 1.6 - Min(eased);
                Assert.That(de, Is.GreaterThanOrEqualTo(0.85 * dt), $"the dip keeps its depth at {fall} m/s: {de * 100:0.0} cm against {dt * 100:0.0}");
            }
        }

        static double Min(double[] p)
        {
            double m = double.MaxValue;
            foreach (double v in p) m = Math.Min(m, v);
            return m;
        }

        [Test]
        public void NotASlowerCrouch()
        {
            GameConfig cfg = Cfg();
            double stand = cfg.Num("body.eyeHeight"), drop = cfg.Num("slide.cameraDrop");
            var (today, eased) = Run(N, S0, S1);
            double half = stand - drop / 2;
            Assert.That(Reach(eased, S0, half, true) - Reach(today, S0, half, true), Is.InRange(0, 1), "the entry's half-way within one frame of today's");
            Assert.That(Reach(eased, S1, half, false) - Reach(today, S1, half, false), Is.InRange(0, 1), "the exit's half-way within one frame of today's");
            double ninety = stand - drop * 0.1;
            Assert.That(Reach(eased, S1, ninety, false) - Reach(today, S1, ninety, false), Is.InRange(-1, 1), "90 % of the exit within one frame of today's (0.10 s)");
            Assert.That((stand - eased[S0 + 4]) - (stand - today[S0 + 4]), Is.GreaterThanOrEqualTo(-0.03), "the drop 83 ms (5 frames) into the slide within 3 cm of today's");
        }

        [Test]
        public void TheHook_CameraFeelStepsEveryFrame_ReadsTheRowEveryFrame_OffIsTodaysExpression()
        {
            string src = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "CameraFeel.cs"));
            int apply = src.IndexOf("public void Apply(Camera camera", StringComparison.Ordinal);
            Assert.That(apply, Is.GreaterThan(0));
            int step = src.IndexOf("eye63.Step(target, standEye - slideEye, c.camera.eyeEaseTime, dip, frameDt)", apply, StringComparison.Ordinal);
            int live = src.IndexOf("NightRows.Live(eye63On, Eye63Math.Row)", apply, StringComparison.Ordinal);
            int pos = src.IndexOf("camera.transform.position = WebSpace.ToUnity(new Vec3(drawn.x, eye63Live ? drawn.y + eye63Y : drawn.y + eye - dip, drawn.z));", apply, StringComparison.Ordinal);
            Assert.That(step, Is.GreaterThan(apply), "the follow steps in Apply");
            Assert.That(live, Is.GreaterThan(step), "every frame: the follow steps before (and whatever) the row reads");
            Assert.That(pos, Is.GreaterThan(live), "the camera: the eased eye with the row on, today's exact expression with it off");
            string stepLine = src.Substring(src.LastIndexOf('\n', step) + 1, step - src.LastIndexOf('\n', step) - 1);
            Assert.That(stepLine.Trim(), Does.StartWith("double eye63Y ="), "the step is not inside a branch on the row");
            int cut = src.IndexOf("public void Cut()", StringComparison.Ordinal);
            Assert.That(src.IndexOf("eye63.Cut(config.body.eyeHeight);", cut, StringComparison.Ordinal), Is.GreaterThan(cut), "a respawn cuts the follow");
            int wire = src.IndexOf("public void Wire(EventBus events, GameConfig cfg)", StringComparison.Ordinal);
            Assert.That(src.IndexOf("Eye63Math.Decide(", wire, StringComparison.Ordinal), Is.GreaterThan(wire), "decided once at Wire");
        }
    }
}
