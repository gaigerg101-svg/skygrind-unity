// NIGHT63 builder V (B1e, a measurement card after B1 PASS on n63-b10; Logs/n63/verdict-B1.md and DECIDE63 D-12: THE WICK runs at its full 1.53 m
// cap in s14 / s15, so during its OPEN window the drawn orb can sit up to 1.53 m from the centre of its 1.7 m hit sphere, and a shot at the drawn
// orb's edge may miss: "a shot finds what you see"). No behaviour change: a boss63 proof line per boss,
//   [boss63] reach <boss> open frames <n> | drawn weak point to the sim's hit sphere centre max <m> m, p95 <m> m | outside its <r> m sphere <k>
//   frames (<share> %)
// over the frames of the OPEN window (the stagger phase: THE WICK's cyan window, THE ICEBREAKER stuck / dazed, THE KILN WYRM staggered, THE
// LOOM's window, THE CROWN's window) that the layer drew. The drawn weak point: the layer's body transform applied to the sim's weak point
// (BossAttackClock63's `weak`): pivot + offset + tilt x (squash x (weak - pivot)); THE ICEBREAKER's view reports its own (its knees take the
// slump's sink). The sphere: the size the guard scales by (the boss's collider radius).
// Pinned: the pure drawn point and the book's numbers on planted motions (max, p95 by nearest rank, the share outside, the line); the layer
// records it each drawn frame; the reel adds the open frames and prints the line per boss; a real WICK rig at the reel's stagger frame (Editor
// only: the engine's Quaternion). Written test first: RED on its commit (placeholders answer nothing); the Editor run is the lead's.
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N63ReachTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static Motion63 Planted(Vector3 offset, Vector3 squash)
        {
            Motion63 m = Motion63.Rest;
            m.on = true;
            m.offset = offset;
            m.squash = squash;
            return m;
        }

        [Test]
        public void DrawnPoint_ThePivotsOffset_TheSquashAboutThePivot()
        {
            Vector3 pivot = new Vector3(2f, 3f, -4f);
            Motion63 m = Planted(new Vector3(1.19f, -0.84f, 0f), new Vector3(1.2f, 0.7f, 1.2f));
            Vector3 core = BossMotion63Math.DrawnPoint(pivot, m, pivot);
            Assert.That((core - pivot).magnitude, Is.EqualTo(Mathf.Sqrt(1.19f * 1.19f + 0.84f * 0.84f)).Within(1e-4f), "a weak point at the pivot moves by the offset");
            Vector3 top = BossMotion63Math.DrawnPoint(pivot, m, pivot + new Vector3(0f, 1f, 0f));
            Assert.That(top - pivot, Is.EqualTo(new Vector3(1.19f, -0.84f + 0.7f, 0f)).Using(new V3(1e-4f)), "a point a metre up: the offset plus the squashed metre");
            Assert.That(BossMotion63Math.DrawnPoint(pivot, Motion63.Rest, pivot + Vector3.one), Is.EqualTo(pivot + Vector3.one).Using(new V3(1e-6f)), "at rest: the sim's point");
        }

        sealed class V3 : System.Collections.Generic.IEqualityComparer<Vector3>
        {
            readonly float tol;
            public V3(float tol) { this.tol = tol; }
            public bool Equals(Vector3 a, Vector3 b) => Mathf.Abs(a.x - b.x) <= tol && Mathf.Abs(a.y - b.y) <= tol && Mathf.Abs(a.z - b.z) <= tol;
            public int GetHashCode(Vector3 v) => 0;
        }

        [Test]
        public void Book_MaxP95TheShareOutside_OnPlantedMotions()
        {
            var book = new ReachBook63();
            Vector3 pivot = Vector3.zero;
            // 20 open frames: the drawn weak point 0.1, 0.2 ... 2.0 m from the sim's (a slide along x), the sphere 1.75 m
            for (int i = 1; i <= 20; i++) book.Add(pivot, Planted(new Vector3(0.1f * i, 0f, 0f), Vector3.one), pivot, 1.75f);
            Assert.That(book.Frames, Is.EqualTo(20));
            Assert.That(book.Max, Is.EqualTo(2.0).Within(1e-5));
            Assert.That(book.P95, Is.EqualTo(1.9).Within(1e-5), "the 95th percentile by nearest rank: the 19th of 20");
            Assert.That(book.Outside, Is.EqualTo(3), "1.8, 1.9 and 2.0 m lie outside the 1.75 m sphere");
            Assert.That(book.Line("wick"), Is.EqualTo("[boss63] reach wick open frames 20 | drawn weak point to the sim's hit sphere centre max 2.00 m, p95 1.90 m | outside its 1.75 m sphere 3 frames (15.0 %)"));
            var none = new ReachBook63();
            Assert.That(none.Line("wyrm"), Is.EqualTo("[boss63] reach wyrm open frames 0 | no open frame drawn (not measured)"));
        }

        [Test]
        public void Rig_TheWicksSlumpAtTheStaggerFrame_InsideItsSphere()
        {
            GameConfig cfg = GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));
            BossMotionParams63 p = BossMotionParams63.From(cfg, BossAttackClock63.Wick);
            var rig = new BossMotionRig63(BossAttackClock63.Wick);
            const float dt = 1f / 60f, r = 1.7f;
            BossAttack63 A(BossPhase63 ph) => new BossAttack63 { boss = BossAttackClock63.Wick, phase = ph, hp = 1000, maxHp = 1000 };
            for (int i = 0; i < 30; i++) rig.Step(p, A(BossPhase63.Idle), Vector3.zero, Vector3.forward, r, dt, dt, true);
            var book = new ReachBook63();
            for (int i = 0; i < 90; i++) book.Add(Vector3.zero, rig.Step(p, A(BossPhase63.Stagger), Vector3.zero, Vector3.forward, r, dt, dt, true), Vector3.zero, r);
            Assert.That(book.Max, Is.GreaterThan(1.2).And.LessThanOrEqualTo(p.limitShare * r + 1e-4), "the slide and the sink, capped by the guard");
            Assert.That(book.Outside, Is.EqualTo(0), "the drawn core never leaves the 1.7 m hit sphere (its guard 1.53 m)");
        }

        [Test]
        public void Hooks_TheLayerRecords_TheIcebreakerReportsItsOwn_TheReelPrints()
        {
            string layer = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "BossMotion63.Layer.cs"));
            Assert.That(layer.Contains("BossMotion63Probe.Weak("), Is.True, "the layer records the drawn weak point each drawn frame");
            string brawl = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "Brawl40View.N63V.cs"));
            Assert.That(brawl.Contains("BossMotion63Probe.Weak("), Is.True, "THE ICEBREAKER reports its own (its knees take the sink)");
            string reel = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "FeelReel.N63V.cs"));
            Assert.That(reel.Contains("if (drew && BossMotion63Probe.WeakSet && a.phase == BossPhase63.Stagger) reach.AddDistance(BossMotion63Probe.WeakDist, BossMotion63Probe.WeakR);"), Is.True, "the open frames the layer drew");
            Assert.That(reel.Contains("Say(reach.Line(boss));"), Is.True, "the proof line per boss");
        }
    }
}
