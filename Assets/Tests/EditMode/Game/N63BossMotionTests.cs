// NIGHT63 builder V (slate B1; view row design.n63.bossMotion63, OFF; Game/BossMotion63.cs, Game/BossMotion63.Layer.cs, the five views' hooks;
// Tools/night/n63-v-rows.cjs writes the numbers). Pinned:
//   1. the switch: off when the row is off, under -skygrindOff63 (or an older night's), -skygrindNoBossMotion63 (any case), ForceOffFrom 63; on
//      otherwise (a later night's switch, another row's switch leave it on)
//   2. the row: a VIEW row (no tape bit), OFF unless a freeze turned it on; its numbers hold every key the layer reads, in the research's bands
//      (springs f 1.5-3 Hz, z 0.4-0.7, follow r 1-2), and an entry for each of the five bosses; the wyrm has no follow lag (its boxes are hitboxes)
//   3. the math: the spring's k1 / k2 / k3 (t3ssel8r), a step settles on its target and overshoots it once (z < 1, r > 1), stable at a 30 fps frame;
//      the impulse envelope peaks at 1 at x = 1/k; the squash keeps the volume
//   4. the rig: off draws the rigid pose (zero offset, no tilt, unit scale); idle breathes; a wind-up leans the top AWAY from the player and is
//      past Boss63PoseShare >= 250 ms before the hit of the shortest wind-up (0.55 s); the strike tips it TOWARD the player; a damage event flinches
//      on its first frame (a volume-keeping squash) even in hitstop (world dt 0); a stagger slumps (sinks, droops toward the player); the guard: no
//      offset past limitShare x size, no tilt past limitDeg, no squash past limit.squash, whatever is stacked; a rigid jump past snap resets
//   5. the hooks: each of the five boss views drives the layer (one line each), and the layer reads the clock, never writes the sim
// Written test first (NIGHT63.md section 4): RED on its commit (the layer's placeholders answer the rigid pose; the numbers are {}); the Editor run
// is the lead's.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using M = Skygrind.Game.BossMotion63;

namespace Skygrind.Game.Tests
{
    public class N63BossMotionTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        /// <summary>The repo's config with design.n63.bossMotion63.enabled set in memory to `on` (its numbers kept).</summary>
        static GameConfig WithRow(bool on)
        {
            GameConfig c = Cfg();
            var design = (Dictionary<string, object>)c.root["design"];
            var n63 = (Dictionary<string, object>)design["n63"];
            var row = (Dictionary<string, object>)n63[M.Row];
            row["enabled"] = on;
            return c;
        }

        const float Dt = 1f / 60f;

        static BossAttack63 Phase(BossPhase63 ph, double hp = 1000, string attack = null, int stepsToHit = -1, int windupSteps = 0) =>
            new BossAttack63 { boss = "wick", phase = ph, hp = hp, maxHp = 1000, attack = attack, stepsToHit = stepsToHit, windupSteps = windupSteps };

        static float TiltAngle(Quaternion q) { q.ToAngleAxis(out float a, out Vector3 _); return a > 180 ? 360 - a : a; }

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitches()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(M.OffFor(off, new string[0]), Is.True, "the row off: off");
                Assert.That(M.OffFor(on, new string[0]), Is.False, "the row on, no switch: on");
                Assert.That(M.OffFor(on, new[] { "-skygrindNoBossMotion63" }), Is.True, "-skygrindNoBossMotion63: this row alone off");
                Assert.That(M.OffFor(on, new[] { "-skygrindnobossmotion63" }), Is.True, "the one-row switch ignores case");
                Assert.That(M.OffFor(on, new[] { "-skygrindOff63" }), Is.True, "-skygrindOff63");
                Assert.That(M.OffFor(on, new[] { "-skygrindOff60" }), Is.True, "an older night's switch covers this night");
                Assert.That(M.OffFor(on, new[] { "-skygrindOff64" }), Is.False, "a later night's switch leaves it on");
                Assert.That(M.OffFor(on, new[] { "-skygrindNoBossTell63", "-skygrindNoColours63" }), Is.False, "another row's switch leaves it on");
                Assert.That(M.OffReason(on, new string[0]), Is.Null);
                Assert.That(M.OffReason(off, new string[0]), Is.Not.Null.And.Not.Empty);
                NightRows.ForceOffFrom = 63;
                Assert.That(M.OffFor(on, new string[0]), Is.True, "ForceOffFrom 63: off");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void Row_AViewRowOff_WithEveryNumberTheLayerReads()
        {
            GameConfig c = Cfg();
            const string p = "design.n63.bossMotion63.";
            Assert.That(c.Has(p + "_why") && c.Has(p + "enabled_why"), Is.True);
            bool enabled = c.Design("n63.bossMotion63.enabled");
            string ew = c.Get(p + "enabled_why") as string;
            Assert.That(!enabled || (ew != null && ew.StartsWith("ON", StringComparison.Ordinal)), Is.True, "off until a freeze turns it on");
            Assert.That(NightRows.From(c).IndexOf(M.Row), Is.EqualTo(-1), "a view row (no tape bit)");
            Assert.That(c.Has(p + "numbers._why"), Is.True, "the numbers carry a _why");
            foreach (string boss in BossAttackClock63.Bosses)
            {
                BossMotionParams63 bp = BossMotionParams63.From(c, boss);
                Assert.That(bp, Is.Not.Null, boss);
                Assert.That(bp.followF, Is.InRange(1.5f, 3f), boss + " follow f (the research's band)");
                Assert.That(bp.followZ, Is.InRange(0.4f, 0.7f), boss + " follow z");
                Assert.That(bp.followR, Is.InRange(1f, 2f), boss + " follow r");
                Assert.That(bp.leanF, Is.GreaterThan(0f));
                Assert.That(bp.slumpF, Is.GreaterThan(0f));
                Assert.That(bp.riseShare, Is.InRange(0.05f, 0.6f), boss + " the wind-up's ease-in share");
                Assert.That(bp.flinchPeakS, Is.InRange(0.02f, 0.06f), boss + " the flinch peaks about 40 ms after the hit");
                // NIGHT63 B1d (Logs/n63/verdict-B1.md): THE WICK's guard raised to 0.9 of its radius and 45 deg for its own slump; the bound is now
                // the collider itself (the drawn centre inside it: under 1) and a keel short of toppling (was 0.6 and 30)
                Assert.That(bp.limitShare, Is.InRange(0.05f, 0.95f), boss + " the guard's share of its collider radius (the drawn centre inside the collider)");
                Assert.That(bp.limitDeg, Is.InRange(5f, 45f), boss + " the guard's tilt");
                Assert.That(bp.limitSquash, Is.InRange(0.05f, 0.35f), boss + " the guard's squash");
                Assert.That(bp.gain, Is.GreaterThan(0f), boss + " gain");
                Assert.That(bp.windupDeg, Is.GreaterThan(0f));
                Assert.That(bp.sink, Is.GreaterThan(0f));
                Assert.That(bp.breathScale, Is.GreaterThan(0f));
            }
            Assert.That(BossMotionParams63.From(c, BossAttackClock63.Wyrm).follow, Is.EqualTo(0f), "the wyrm's boxes are its hitboxes: no follow lag");
        }

        [Test]
        public void Math_TheSpringSettlesOvershootsAndStaysStable()
        {
            BossMotion63Math.K(2f, 0.5f, 1.6f, out float k1, out float k2, out float k3);
            Assert.That(k1, Is.EqualTo(0.5f / (Mathf.PI * 2f)).Within(1e-6f));
            Assert.That(k2, Is.EqualTo(1f / ((2f * Mathf.PI * 2f) * (2f * Mathf.PI * 2f))).Within(1e-7f));
            Assert.That(k3, Is.EqualTo(1.6f * 0.5f / (2f * Mathf.PI * 2f)).Within(1e-6f));
            foreach (float dt in new[] { 1f / 60f, 1f / 30f })
            {
                var s = new Spring63();
                s.Set(2f, 0.5f, 1.6f);
                s.Reset(0f);
                float peak = 0, y = 0;
                for (int i = 0; i < (int)(3f / dt); i++) { y = s.Step(1f, dt); peak = Mathf.Max(peak, y); Assert.That(float.IsNaN(y) || Mathf.Abs(y) > 10f, Is.False, "stable at dt " + dt); }
                Assert.That(y, Is.EqualTo(1f).Within(0.02f), "settles on its target (dt " + dt + ")");
                Assert.That(peak, Is.GreaterThan(1.02f), "overshoots once (z < 1, r > 1)");
                Assert.That(peak, Is.LessThan(1.8f), "never wild");
            }
            var frozen = new Spring63();
            frozen.Set(2f, 0.5f, 1f);
            frozen.Reset(0f);
            Assert.That(frozen.Step(1f, 0f), Is.EqualTo(0f), "dt 0 (the sim's hitstop): the spring holds");
            Assert.That(BossMotion63Math.ExpImpulse(1f / 25f, 25f), Is.EqualTo(1f).Within(1e-5f), "the impulse peaks at 1 at x = 1/k");
            Assert.That(BossMotion63Math.ExpImpulse(0.02f, 25f), Is.LessThan(1f));
            Assert.That(BossMotion63Math.ExpImpulse(0.4f, 25f), Is.LessThan(0.01f), "gone by 0.4 s");
            Vector3 v = BossMotion63Math.Volume(0.8f);
            Assert.That(v.x * v.y * v.z, Is.EqualTo(1f).Within(1e-4f), "the squash keeps the volume");
            Assert.That(v.y, Is.EqualTo(0.8f).Within(1e-6f));
            Assert.That(BossMotion63Math.WindupPose(0, 0.3f), Is.EqualTo(0f).Within(1e-6f));
            Assert.That(BossMotion63Math.WindupPose(0.3, 0.3f), Is.EqualTo(1f).Within(1e-6f), "eased in over the rise share");
            Assert.That(BossMotion63Math.WindupPose(0.9, 0.3f), Is.EqualTo(1f).Within(1e-6f), "then held to the hit");
        }

        static BossMotionParams63 P() => BossMotionParams63.From(Cfg(), "wick");

        [Test]
        public void Rig_OffDrawsTheRigidPose()
        {
            var rig = new BossMotionRig63("wick");
            BossMotionParams63 p = P();
            Motion63 m = default;
            for (int i = 0; i < 120; i++) m = rig.Step(p, Phase(BossPhase63.Stagger, 1000 - i), new Vector3(0, 5, 0), Vector3.forward, 1.7f, Dt, Dt, false);
            Assert.That(m.on, Is.False);
            Assert.That(m.offset, Is.EqualTo(Vector3.zero));
            Assert.That(m.tilt, Is.EqualTo(Quaternion.identity));
            Assert.That(m.squash, Is.EqualTo(Vector3.one));
        }

        /// <summary>NIGHT63 fix round (D1's contract, the lead's message): a row read every frame keeps its state while off and only skips drawing,
        /// so the frame after a same-run toggle equals a run without it. Two rigs are fed one fight (an idle, a wind-up, a strike, hits, a stagger)
        /// with world time running; one has the row off for 20 frames inside the wind-up and the stagger. Off frames draw the rigid pose; every
        /// frame after the row comes back is EXACTLY the never-toggled rig's.</summary>
        [Test]
        public void Rig_OffKeepsItsState_TheFrameAfterAToggleEqualsARunWithoutIt()
        {
            BossMotionParams63 p = P();
            var always = new BossMotionRig63("wick");
            var toggled = new BossMotionRig63("wick");
            double hp = 1000;
            for (int f = 0; f < 240; f++)
            {
                BossPhase63 ph = f < 40 ? BossPhase63.Idle : f < 73 ? BossPhase63.Windup : f < 90 ? BossPhase63.Strike : f < 120 ? BossPhase63.Recover : BossPhase63.Stagger;
                if (f % 7 == 0) hp -= 4;
                var a = new BossAttack63 { boss = "wick", phase = ph, hp = hp, maxHp = 1000, attack = ph == BossPhase63.Idle || ph == BossPhase63.Stagger ? null : "slam", stepsToHit = ph == BossPhase63.Windup ? 66 - (f - 40) * 2 : ph == BossPhase63.Strike ? 0 : -1, windupSteps = ph == BossPhase63.Windup ? 66 : 0 };
                Vector3 rigid = new Vector3(Mathf.Sin(f * 0.05f) * 2f, 5f, f * 0.02f);
                bool off = (f >= 50 && f < 60) || (f >= 130 && f < 140);
                Motion63 m1 = always.Step(p, a, rigid, Vector3.forward, 1.7f, Dt, Dt, true);
                Motion63 m2 = toggled.Step(p, a, rigid, Vector3.forward, 1.7f, Dt, Dt, !off);
                if (off)
                {
                    Assert.That(m2.on, Is.False, "frame " + f + ": off draws the rigid pose");
                    Assert.That(m2.offset, Is.EqualTo(Vector3.zero));
                    continue;
                }
                Assert.That(m2.offset.x, Is.EqualTo(m1.offset.x), "frame " + f + ": offset x as a run without the toggle");
                Assert.That(m2.offset.y, Is.EqualTo(m1.offset.y), "frame " + f + ": offset y");
                Assert.That(m2.offset.z, Is.EqualTo(m1.offset.z), "frame " + f + ": offset z");
                Assert.That(m2.tilt.x == m1.tilt.x && m2.tilt.y == m1.tilt.y && m2.tilt.z == m1.tilt.z && m2.tilt.w == m1.tilt.w, Is.True, "frame " + f + ": tilt");
                Assert.That(m2.squash.y, Is.EqualTo(m1.squash.y), "frame " + f + ": squash");
                Assert.That(m2.flinch, Is.EqualTo(m1.flinch), "frame " + f + ": the flinch's envelope (a hit while off still ages)");
            }
            Assert.That(toggled.Flinches, Is.EqualTo(always.Flinches), "hits while off counted as in a run without the toggle");
        }

        /// <summary>NIGHT63 fix round 3 (n63-b2 b63p, THE CROWN: rms 0 in 23 of 23 seconds, posed 0 of 6, flinches drawn 0 of 49, limit share 0.00):
        /// Crown37View draws in LateUpdate, after the boss63 reel's coroutine has read the probe for the frame, so the reel's same-frame test
        /// (DrewThisFrame) never saw a Crown draw. The reel now takes each draw once: this frame's (an Update view's) or the frame before's (a LateUpdate
        /// view's), never a stale one.</summary>
        [Test]
        public void Probe_TheReelTakesEachDrawOnce_ALateUpdateDrawTheFrameAfter()
        {
            int was = BossMotion63Probe.Frame;
            try
            {
                int taken = -1;
                BossMotion63Probe.Frame = -1;
                Assert.That(BossMotion63Probe.Take(10, ref taken), Is.False, "nothing drawn");
                BossMotion63Probe.Frame = 10;
                Assert.That(BossMotion63Probe.Take(10, ref taken), Is.True, "an Update view's draw, read the same frame");
                Assert.That(BossMotion63Probe.Take(10, ref taken), Is.False, "taken once");
                Assert.That(BossMotion63Probe.Take(11, ref taken), Is.False, "the frame after: already taken");
                BossMotion63Probe.Frame = 11;
                Assert.That(BossMotion63Probe.Take(12, ref taken), Is.True, "THE CROWN draws in LateUpdate: its draw is read the frame after");
                int fresh = -1;
                BossMotion63Probe.Frame = 11;
                Assert.That(BossMotion63Probe.Take(13, ref fresh), Is.False, "two frames old: stale (the layer stopped drawing)");
            }
            finally { BossMotion63Probe.Frame = was; }
        }

        [Test]
        public void Reel_ReadsTheProbeByTake()
        {
            string reel = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "FeelReel.N63V.cs"));
            Assert.That(reel.Contains("BossMotion63Probe.Take("), Is.True, "the reel takes each draw once");
            Assert.That(reel.Contains("DrewThisFrame"), Is.False, "the same-frame test missed THE CROWN's LateUpdate draws");
        }

        /// <summary>The same contract in the five views' hooks: no hook resets its own state (its springs, its live flag) on the row-off path.</summary>
        [Test]
        public void Hooks_TheOffPathKeepsTheirState()
        {
            string dir = Path.Combine(Repo(), "Assets", "Scripts", "Game");
            foreach (string f in new[] { "Wick37View.N63V.cs", "Brawl40View.N63V.cs", "WyrmView.N63V.cs", "Loom37View.N63V.cs", "Crown37View.N63V.cs" })
            {
                string src = File.ReadAllText(Path.Combine(dir, f));
                int at = 0;
                while ((at = src.IndexOf("if (!m.on)", at, StringComparison.Ordinal)) >= 0)
                {
                    int open = src.IndexOf('{', at), semi = src.IndexOf(';', at);
                    int end = open >= 0 && open < semi ? src.IndexOf('}', open) : semi;
                    string body = src.Substring(at, Math.Max(0, end - at));
                    Assert.That(body.Contains("Reset(") || body.Contains("Live63 = false") || body.Contains("= null"), Is.False, f + ": the off path resets state: " + body);
                    at += 10;
                }
            }
        }

        [Test]
        public void Rig_IdleBreathes()
        {
            var rig = new BossMotionRig63("wick");
            BossMotionParams63 p = P();
            float maxDev = 0;
            for (int i = 0; i < 180; i++)
            {
                Motion63 m = rig.Step(p, Phase(BossPhase63.Idle), new Vector3(0, 5, 0), Vector3.forward, 1.7f, Dt, Dt, true);
                maxDev = Mathf.Max(maxDev, Mathf.Abs(m.squash.y - 1f));
                Assert.That(m.on, Is.True);
            }
            Assert.That(maxDev, Is.GreaterThan(p.breathScale * 0.5f), "the height pulses with the breath at rest");
        }

        [Test]
        public void Rig_TheWindupLeansBackEarly_TheStrikeSnapsForward()
        {
            var rig = new BossMotionRig63("wick");
            BossMotionParams63 p = P();
            Vector3 toward = Vector3.forward;
            for (int i = 0; i < 60; i++) rig.Step(p, Phase(BossPhase63.Idle), Vector3.zero, toward, 1.7f, Dt, Dt, true);
            const int windup = 66; // 0.55 s at 120 Hz: the shortest wind-up in config
            double visibleAt = -1;
            Motion63 m = default;
            int frames = (int)Math.Round(windup / 2.0); // 2 sim steps a frame at the reel's 60 fps
            for (int f = 0; f <= frames; f++)
            {
                int left = Math.Max(0, windup - f * 2);
                m = rig.Step(p, Phase(BossPhase63.Windup, attack: "slam", stepsToHit: left, windupSteps: windup), Vector3.zero, toward, 1.7f, Dt, Dt, true);
                if (visibleAt < 0 && m.windup >= UnityTuning.Boss63PoseShare) visibleAt = f * Dt;
            }
            Assert.That(visibleAt, Is.GreaterThanOrEqualTo(0), "the wind-up pose shows");
            Assert.That(0.55 - visibleAt, Is.GreaterThanOrEqualTo(0.25), "visible >= 250 ms before the hit");
            Assert.That(Vector3.Dot(m.tilt * Vector3.up, toward), Is.LessThan(-0.05f), "the top leans AWAY from the player");
            Assert.That(Vector3.Dot(m.offset, toward), Is.LessThan(0f), "pulled back");
            Assert.That(m.squash.y, Is.LessThan(1f), "crouched");
            float best = -1;
            for (int f = 0; f < 12; f++)
            {
                m = rig.Step(p, Phase(BossPhase63.Strike, attack: "slam", stepsToHit: 0), Vector3.zero, toward, 1.7f, Dt, Dt, true);
                best = Mathf.Max(best, Vector3.Dot(m.tilt * Vector3.up, toward));
            }
            Assert.That(best, Is.GreaterThan(0.05f), "within 0.2 s of the strike the top tips TOWARD the player (the snap through rest)");
        }

        [Test]
        public void Rig_ADamageEventFlinchesOnItsFirstFrame_EvenInHitstop()
        {
            var rig = new BossMotionRig63("wick");
            BossMotionParams63 p = P();
            for (int i = 0; i < 30; i++) rig.Step(p, Phase(BossPhase63.Idle, 1000), Vector3.zero, Vector3.forward, 1.7f, Dt, Dt, true);
            Motion63 m = rig.Step(p, Phase(BossPhase63.Idle, 990), Vector3.zero, Vector3.forward, 1.7f, 0f, Dt, true); // the hit's frame: the sim's hitstop (world dt 0)
            Assert.That(m.flinch, Is.GreaterThan(0.1f), "drawn on the damage event's own frame");
            Assert.That(m.squash.y, Is.LessThan(1f - p.flinchSquash * 0.2f), "squashed");
            Assert.That(m.squash.x * m.squash.y * m.squash.z, Is.EqualTo(1f).Within(2e-3f), "the volume kept");
            Assert.That(Vector3.Dot(m.offset, Vector3.forward), Is.LessThan(0f), "recoils away from the player");
            float peak = m.flinch;
            int peakFrame = 0;
            for (int f = 1; f < 30; f++)
            {
                m = rig.Step(p, Phase(BossPhase63.Idle, 990), Vector3.zero, Vector3.forward, 1.7f, 0f, Dt, true);
                if (m.flinch > peak) { peak = m.flinch; peakFrame = f; }
            }
            Assert.That(peakFrame, Is.LessThanOrEqualTo(2), "peaks within about 40 ms (2.4 frames)");
            Assert.That(m.flinch, Is.LessThan(0.05f), "gone by half a second");
            Assert.That(rig.Flinches, Is.EqualTo(1));
        }

        /// <summary>NIGHT63 rule-8 round (n63-b4's same-run pair on THE WICK's stagger: 37,345 px, under 41,472; at boss63's 12.5 m the cage is about
        /// 200 x 300 px): the slump must move in the SCREEN's plane, not in depth. A lean toward the player (the droop) turns about the screen's
        /// horizontal axis and barely shows; the sink, the side keel and the squash show. At the reel's stagger frame (0.6 s in), on THE WICK's
        /// numbers: a keel of 20 deg or more, a sink of 0.9 m or more (its collider radius 1.7 m), the height squashed to 0.85 or less, the guard held.</summary>
        [Test]
        public void Rig_TheWicksSlumpReadsOnScreen()
        {
            var rig = new BossMotionRig63("wick");
            BossMotionParams63 p = P();
            Vector3 toward = Vector3.forward;
            for (int i = 0; i < 30; i++) rig.Step(p, Phase(BossPhase63.Idle), Vector3.zero, toward, 1.7f, Dt, Dt, true);
            Motion63 m = default;
            for (int i = 0; i < 36; i++) m = rig.Step(p, Phase(BossPhase63.Stagger), Vector3.zero, toward, 1.7f, Dt, Dt, true);
            Assert.That(Mathf.Abs(m.sideDeg), Is.GreaterThanOrEqualTo(20f), "the keel in the screen's plane");
            // NIGHT63 B1d: part of the sink moved into the slide (the 1.02 m sink sat the orb on the floor): the move in the screen's plane, the
            // sink and the slide together, is what is pinned now (was: a sink of 0.9 m or more)
            Vector3 axis = Vector3.Cross(Vector3.up, toward).normalized;
            Assert.That(m.sinkM, Is.GreaterThanOrEqualTo(0.7f), "the sink");
            Assert.That(new Vector2(Vector3.Dot(m.offset, axis), m.offset.y).magnitude, Is.GreaterThanOrEqualTo(1.2f), "the sink and the slide in the screen's plane");
            Assert.That(m.squash.y, Is.LessThanOrEqualTo(0.85f), "a squash you can see");
            Assert.That(m.limitShare, Is.LessThanOrEqualTo(1f + 1e-4f), "within the guard");
            Assert.That(m.offset.magnitude, Is.LessThanOrEqualTo(p.limitShare * 1.7f + 1e-4f), "the offset within THE WICK's limit");
        }

        [Test]
        public void Rig_AStaggerSlumps_AndRisesAfter()
        {
            var rig = new BossMotionRig63("wick");
            BossMotionParams63 p = P();
            Vector3 toward = Vector3.forward;
            for (int i = 0; i < 30; i++) rig.Step(p, Phase(BossPhase63.Idle), Vector3.zero, toward, 1.7f, Dt, Dt, true);
            Motion63 m = default;
            for (int i = 0; i < 36; i++) m = rig.Step(p, Phase(BossPhase63.Stagger), Vector3.zero, toward, 1.7f, Dt, Dt, true); // 0.6 s: the SPOT frame
            Assert.That(m.slump, Is.GreaterThan(0.6f), "slumped by the stagger frame");
            Assert.That(m.offset.y, Is.LessThan(-0.4f * p.sink * 1.7f), "sunk");
            Assert.That(Vector3.Dot(m.tilt * Vector3.up, toward), Is.GreaterThan(0.15f), "drooped toward the player");
            Assert.That(TiltAngle(m.tilt), Is.GreaterThan(8f), "a big pose: a player sees it");
            for (int i = 0; i < 150; i++) m = rig.Step(p, Phase(BossPhase63.Idle), Vector3.zero, toward, 1.7f, Dt, Dt, true);
            Assert.That(m.slump, Is.LessThan(0.1f), "risen 2.5 s after the stagger");
        }

        [Test]
        public void Rig_TheGuardHoldsWhateverIsStacked_AndAJumpSnaps()
        {
            foreach (string boss in BossAttackClock63.Bosses)
            {
                BossMotionParams63 p = BossMotionParams63.From(Cfg(), boss);
                var rig = new BossMotionRig63(boss);
                const float size = 1.5f;
                double hp = 1000;
                float worstShare = 0;
                for (int i = 0; i < 600; i++)
                {
                    BossPhase63 ph = (i / 40) % 3 == 0 ? BossPhase63.Windup : (i / 40) % 3 == 1 ? BossPhase63.Strike : BossPhase63.Stagger;
                    hp -= 3; // a hit every frame
                    Vector3 rigid = new Vector3(Mathf.Sin(i * 0.3f) * 3f, 0, i * 0.2f); // moving fast and turning
                    Motion63 m = rig.Step(p, new BossAttack63 { boss = boss, phase = ph, hp = hp, maxHp = 1000, attack = "x", stepsToHit = ph == BossPhase63.Windup ? 40 - (i % 40) : 0, windupSteps = 40 },
                        rigid, i % 2 == 0 ? Vector3.forward : Vector3.right, size, Dt, Dt, true);
                    Assert.That(m.offset.magnitude, Is.LessThanOrEqualTo(p.limitShare * size + 1e-4f), boss + ": offset within its limit");
                    Assert.That(TiltAngle(m.tilt), Is.LessThanOrEqualTo(p.limitDeg + 0.05f), boss + ": tilt within its limit");
                    foreach (float s in new[] { m.squash.x, m.squash.y, m.squash.z })
                        Assert.That(Mathf.Abs(s - 1f), Is.LessThanOrEqualTo(p.limitSquash * 1.25f + 1e-4f), boss + ": squash within its limit (the volume's 1/sqrt on x / z)");
                    Assert.That(m.limitShare, Is.LessThanOrEqualTo(1f + 1e-4f), boss + ": the probe's share");
                    worstShare = Mathf.Max(worstShare, m.limitShare);
                }
                Assert.That(worstShare, Is.GreaterThan(0f), boss + ": it moved");
            }
            var snap = new BossMotionRig63("wick");
            BossMotionParams63 pw = BossMotionParams63.From(Cfg(), "wick");
            for (int i = 0; i < 30; i++) snap.Step(pw, Phase(BossPhase63.Recover), Vector3.zero, Vector3.forward, 1.7f, Dt, Dt, true);
            Motion63 after = snap.Step(pw, Phase(BossPhase63.Recover), new Vector3(40, 0, 0), Vector3.forward, 1.7f, Dt, Dt, true);
            Assert.That(Mathf.Abs(after.offset.x), Is.LessThan(0.3f), "a 40 m jump (a teleport) resets the follow: no slide across the room");
        }

        [Test]
        public void Hooks_EveryBossViewDrivesTheLayer_TheLayerNeverWritesTheSim()
        {
            string dir = Path.Combine(Repo(), "Assets", "Scripts", "Game");
            var callers = new SortedSet<string>(StringComparer.Ordinal);
            foreach (string f in Directory.GetFiles(dir, "*.cs"))
            {
                string name = Path.GetFileName(f);
                if (name.StartsWith("BossMotion63", StringComparison.Ordinal) || name.StartsWith("FeelReel", StringComparison.Ordinal)) continue;
                string text = File.ReadAllText(f);
                if (text.Contains("Motion63(")) callers.Add(name);
            }
            foreach (string view in new[] { "Wick37View.cs", "Brawl40View.cs", "WyrmView.cs", "Loom37View.cs", "Crown37View.Crown.cs" })
                Assert.That(callers.Contains(view), Is.True, view + " calls its Motion63 hook (" + string.Join(", ", callers) + ")");
            string layer = File.ReadAllText(Path.Combine(dir, "BossMotion63.Layer.cs"));
            foreach (string write in new[] { "brain.", "sim.Step", ".Force(", ".Drain(", ".OnStagger(", ".Damage(", "BossRoomBegin(", "Hitstop(" })
                Assert.That(layer.Contains(write), Is.False, "the layer never reaches into the sim: " + write);
            Assert.That(layer.Contains("BossAttackClock63.Read("), Is.True, "the layer reads the clock");
        }
    }
}
