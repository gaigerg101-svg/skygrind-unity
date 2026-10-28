// NIGHT63 builder V (B1d, the lead's fix round after B1's gate FAIL, Logs/n63/verdict-B1.md):
//   1. THE WICK's stagger slump clears rule 8 with margin. b8's same-run pair at shot-N30-B63-wick-stagger changed 40,503 px, and every boss ran
//      pinned at its guard through the stagger ("limit share max 1.00": the clamp set the slump's size, not its spring). B1c's raise mostly went
//      into the clamp (b4 37,345 -> b8 40,503 px), and its 1.02 m sink already sat the orb on the floor. B1d: THE WICK's own slump numbers
//      (bosses.wick.slump: a shallower sink, a deeper keel, a stronger squash and a SLIDE: the cage slides the way it keels, in the screen's
//      plane) within a raised guard (bosses.wick limitShare 0.9 of its 1.7 m collider radius: the drawn centre stays inside the collider;
//      limitDeg 45), so the full slump sits under the guard instead of on it. The other bosses' slumps are unchanged (no slide, the shared
//      numbers).
//   2. The reel's flinch book: THE CROWN draws in LateUpdate, so the reel takes its draw of a frame on the next frame; two damage events on
//      consecutive frames (b8's 8983 and 9000 ms) counted the first "drawn off" because the new event replaced it before its draw was judged
//      (not a held frame: no shot was near 8983 ms on b8). FlinchBook63 judges the pending event on this frame's take first.
// Written test first: RED on its commit (FlinchBook63 a placeholder; no per-boss slump numbers; the old reel); the Editor run is the lead's
// (the rig test calls the engine's Quaternion: it cannot run outside Unity).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N63B1dTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        const float WickCageR = 1.7f; // design.n37.wick37.boss.cageR: THE WICK's collider radius (the sim's hit sphere)

        [Test]
        public void Wick_ItsOwnSlump_UnderARaisedGuard_TheCentreInsideItsCollider()
        {
            GameConfig c = Cfg();
            Assert.That(c.Num("design.n37.wick37.boss.cageR"), Is.EqualTo(WickCageR).Within(1e-6), "the collider radius the guard scales by");
            BossMotionParams63 w = BossMotionParams63.From(c, BossAttackClock63.Wick);
            Assert.That(w.Valid, Is.True);
            Assert.That(w.slide, Is.InRange(0.5f, 0.85f), "the slide in the screen's plane (a share of the collider radius)");
            Assert.That(w.sink, Is.InRange(0.35f, 0.6f), "a shallower sink: the orb rests on the floor, it does not sink through it");
            Assert.That(w.slumpSide, Is.GreaterThanOrEqualTo(35f), "a deeper keel");
            Assert.That(w.slumpSquash, Is.InRange(0.25f, 0.32f), "a stronger squash, inside the shared squash guard");
            Assert.That(w.limitShare, Is.GreaterThan(0.8f).And.LessThan(1f), "the drawn centre stays inside the collider (under 1 x its radius)");
            Assert.That(w.limitDeg, Is.InRange(40f, 45f));
            float full = Mathf.Sqrt(w.sink * w.sink + w.slide * w.slide);
            Assert.That(full, Is.LessThanOrEqualTo(w.limitShare), "the full slump sits under the guard: the clamp no longer sets its size");
            Assert.That(full * WickCageR, Is.LessThan(WickCageR), "and inside the collider");
            float shared = (float)c.Num(BossMotion63.Numbers + "slump.sink");
            foreach (string boss in new[] { BossAttackClock63.Brawl, BossAttackClock63.Wyrm, BossAttackClock63.Loom, BossAttackClock63.Crown })
            {
                BossMotionParams63 o = BossMotionParams63.From(c, boss);
                Assert.That(o.slide, Is.EqualTo(0f), boss + ": no slide (unchanged)");
                Assert.That(o.sink, Is.EqualTo(shared), boss + ": the shared sink (unchanged)");
                Assert.That(o.slumpSide, Is.EqualTo((float)c.Num(BossMotion63.Numbers + "slump.sideDeg")), boss + ": the shared keel (unchanged)");
            }
        }

        [Test]
        public void Rig_TheWicksSlump_SlidesTheWayItKeels_InsideItsGuard()
        {
            BossMotionParams63 p = BossMotionParams63.From(Cfg(), BossAttackClock63.Wick);
            var rig = new BossMotionRig63(BossAttackClock63.Wick);
            Vector3 toward = Vector3.forward, axis = Vector3.Cross(Vector3.up, toward).normalized;
            const float dt = 1f / 60f;
            BossAttack63 A(BossPhase63 ph) => new BossAttack63 { boss = BossAttackClock63.Wick, phase = ph, hp = 1000, maxHp = 1000 };
            for (int i = 0; i < 30; i++) rig.Step(p, A(BossPhase63.Idle), Vector3.zero, toward, WickCageR, dt, dt, true);
            float side = 0;
            for (int round = 0; round < 2; round++)
            {
                Motion63 m = default;
                for (int i = 0; i < 36; i++) m = rig.Step(p, A(BossPhase63.Stagger), Vector3.zero, toward, WickCageR, dt, dt, true); // 0.6 s: the SPOT frame
                float lateral = Vector3.Dot(m.offset, axis), topLateral = Vector3.Dot(m.tilt * Vector3.up, axis);
                Assert.That(Mathf.Abs(lateral), Is.GreaterThanOrEqualTo(0.8f), "a slide you can see (m)");
                Assert.That(Mathf.Sign(lateral), Is.EqualTo(Mathf.Sign(topLateral)), "it slides the way its top keels");
                Assert.That(Mathf.Abs(m.sideDeg), Is.GreaterThanOrEqualTo(30f), "the deeper keel");
                Assert.That(m.offset.magnitude, Is.LessThanOrEqualTo(p.limitShare * WickCageR + 1e-4f), "within the guard");
                Assert.That(m.offset.magnitude, Is.LessThan(WickCageR), "the drawn centre inside the collider");
                Assert.That(m.squash.y, Is.LessThanOrEqualTo(0.75f), "the squash");
                if (round == 1) Assert.That(Mathf.Sign(lateral), Is.EqualTo(-side), "the next stagger keels and slides to the other side");
                side = Mathf.Sign(lateral);
                for (int i = 0; i < 180; i++) rig.Step(p, A(BossPhase63.Idle), Vector3.zero, toward, WickCageR, dt, dt, true);
            }
        }

        static List<string> said;
        static void Say(string s) => said.Add(s);

        [Test]
        public void FlinchBook_TheCrownsLateDraw_TwoDamageEventsOnConsecutiveFrames_BothDrawn()
        {
            said = new List<string>();
            var book = new FlinchBook63();
            // frame 10: a damage event; the take is frame 9's draw (THE CROWN draws in LateUpdate, after the reel), no flinch on it
            book.Frame(10, true, 1, 0f, true, 8983, 1958.0, 1957.1, Say);
            Assert.That(book.Pending, Is.True);
            // frame 11: the take is frame 10's draw, the flinch on it; and a second damage event (b8: 9000 ms)
            book.Frame(11, true, 1, 0.75f, true, 9000, 1957.1, 1956.5, Say);
            // frame 12: frame 11's draw, its flinch
            book.Frame(12, true, 1, 0.8f, false, 9017, 0, 0, Say);
            Assert.That(book.Damage, Is.EqualTo(2));
            Assert.That(book.Drawn, Is.EqualTo(2), "both drawn (b8 counted the first off)");
            Assert.That(book.Off, Is.EqualTo(0));
            Assert.That(book.Late, Is.EqualTo(0));
            Assert.That(said, Is.EqualTo(new[] { "[boss63] flinch 8983 hp 1958.0 -> 1957.1 drawn +0 frames", "[boss63] flinch 9000 hp 1957.1 -> 1956.5 drawn +0 frames" }));
        }

        [Test]
        public void FlinchBook_AnUpdateView_TheNeverDrawn_TheReplaced_TheEnd()
        {
            said = new List<string>();
            var book = new FlinchBook63();
            book.Frame(1, true, 0, 0.75f, true, 100, 10, 9, Say); // drawn this frame (the view drew after the sim's step)
            Assert.That(book.Drawn, Is.EqualTo(1));
            Assert.That(book.Pending, Is.False);
            book.Frame(2, true, 0, 0f, true, 117, 9, 8, Say); // never drawn
            for (int f = 3; f <= 9; f++) book.Frame(f, true, 0, 0f, false, 100 + f * 17, 0, 0, Say);
            Assert.That(book.Off, Is.EqualTo(1), "off after 6 frames without a drawn flinch");
            Assert.That(book.Pending, Is.False);
            book.Frame(10, false, 0, 0f, true, 300, 8, 7, Say);
            book.Frame(11, false, 0, 0f, true, 317, 7, 6, Say); // replaced before any draw was taken
            Assert.That(book.Off, Is.EqualTo(2), "a damage event replaced before any draw of it is off");
            book.Frame(13, false, 0, 0f, false, 334, 0, 0, Say);
            book.Frame(14, true, 0, 0.6f, false, 350, 0, 0, Say);
            Assert.That(book.Drawn, Is.EqualTo(2));
            Assert.That(book.Late, Is.EqualTo(1), "drawn 3 frames after: late (the doneWhen: within 2)");
            book.Frame(15, false, 0, 0f, true, 367, 6, 5, Say);
            book.End(Say);
            Assert.That(book.Off, Is.EqualTo(3), "pending at the end: off");
            Assert.That(book.Damage, Is.EqualTo(5), "frames 1, 2, 10, 11 and 15");
        }

        [Test]
        public void Reel_CountsItsFlinchesWithTheBook()
        {
            string src = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "FeelReel.N63V.cs"));
            Assert.That(src.Contains("flinchBook.Frame(frame, drew, drewAgo, BossMotion63Probe.Flinch, damaged, ms, lastHp, a.hp, Say);"), Is.True, "every fight frame through the book");
            Assert.That(src.Contains("flinchBook.End(Say);"), Is.True);
            Assert.That(src.Contains("if (flinchFrame >= 0) { flinchesOff++;"), Is.False, "the old accounting gone");
        }
    }
}
