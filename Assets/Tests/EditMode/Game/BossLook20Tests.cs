// NIGHT20 A1 (builder HV): the BULWARK's look and its bar on builder EN's port (Core/Enemies/Bulwark.cs, BossKit.cs,
// ITelegraph.cs), and VOLATILE's death in the same telegraph language:
// - the telegraph view IS the sim's ITelegraph (Telegraph20 and its model): bulwark.js's kit.telegraph calls (the RAM's line,
//   the STOMP's circle, the hatches' circles, the beat's flash, the off-screen threat) land in the web's shapes; the sim's
//   hex 0 is TELEGRAPH.danger;
// - the STOMP's shock ring (bulwark.js render :621-626): an open cylinder at the ring's centre, radius ring.r, stomp.height
//   tall, standing on the pen's floor (centre y + height / 2), brightest at the floor;
// - the SWEEP BEAM (:627-635): from the boss at beam.height along its angle (web (cos a, 0, sin a)), beam.range long, 0.3 m
//   thick x 0.4 and x 1.2 bright while it charges, whole and x 4 while it sweeps;
// - the boss bar (UiThreat20): the BULWARK's BossBar (name, hp / max, the phase mark p2At, the plates as pips);
// - VOLATILE's fuse: affixes.js onKill's telegraph.circle({r: radius, time: delay}) at the body, full at the burst.
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class BossLook20Tests
    {
        static TelegraphModel Model() => new TelegraphModel(new TelegraphModel.Settings
        {
            max = 48, maxFlashes = 12, maxThreats = 8, hold = 0.12, arrowTime = 1.2, flashTime = 0.15, // config.telegraph
        });

        [Test]
        public void TheTelegraphView_IsTheSimsITelegraph_ItsZeroHexDanger()
        {
            Assert.IsTrue(typeof(ITelegraph).IsAssignableFrom(typeof(Telegraph20)), "the view the BULWARK's kit draws through");
            Assert.IsTrue(typeof(ITelegraph).IsAssignableFrom(typeof(TelegraphModel)), "its model too (the tests, a view-less run)");
            ITelegraph t = Model();
            var m = (TelegraphModel)t;
            int ram = t.Line(1, 0, 2, 0, 1, 20, 2.5, 1.2); // bulwark.js's RAM: hex 0
            int stomp = t.Circle(0, 0, 0, 6.5, 0.9);
            int own = t.Circle(0, 0, 0, 2, 1, 0x40ff80);
            t.Threat(5, 4, 5);
            for (int i = 0; i < m.Slots; i++)
            {
                ref readonly TelegraphModel.Shape s = ref m.ShapeAt(i);
                if (s.id == ram || s.id == stomp) Assert.AreEqual(UnityTuning.TelegraphDanger, s.hex, "hex 0 is TELEGRAPH.danger");
                if (s.id == own) Assert.AreEqual(0x40ff80u, s.hex, "any other hex as it is");
            }
            Assert.AreEqual(UnityTuning.TelegraphDanger, m.ThreatAt(0).hex);
            t.Clear();
            Assert.AreEqual(0, m.Count, "bossKit.clear / down: every shape gone");
        }

        [Test]
        public void TheStompRing_RunsOutAlongTheFloor_AsBulwarkJsDrawsIt()
        {
            Matrix4x4 m = BulwarkFx20Math.Ring(3, 4, 1, 7.5, 1.2);
            Vector3 at = m.GetColumn(3);
            Assert.That(Vector3.Distance(at, new Vector3(3, 1.6f, -4)), Is.LessThan(1e-5f), "its centre, stomp.height / 2 over the pen's floor (z flipped)");
            Assert.That(m.GetColumn(0).magnitude, Is.EqualTo(7.5f).Within(1e-4), "radius ring.r");
            Assert.That(m.GetColumn(1).magnitude, Is.EqualTo(1.2f).Within(1e-4), "stomp.height tall");
            Assert.That(m.GetColumn(2).magnitude, Is.EqualTo(7.5f).Within(1e-4));
            Assert.That(BulwarkFx20Math.RingRamp(0), Is.GreaterThan(0.9f), "brightest at the floor (a wave along it)");
            Assert.That(BulwarkFx20Math.RingRamp(1), Is.LessThan(0.05f), "gone at its top");
            for (int i = 1; i <= 10; i++)
                Assert.That(BulwarkFx20Math.RingRamp(i / 10f), Is.LessThanOrEqualTo(BulwarkFx20Math.RingRamp((i - 1) / 10f)), "fading upward");
        }

        [Test]
        public void TheSweepBeam_DimWhileCharging_BlazingWhileItSweeps()
        {
            Vector3 d = BulwarkFx20Math.BeamDir(Mathf.PI / 2);
            Assert.That(Vector3.Distance(d, new Vector3(0, 0, -1)), Is.LessThan(1e-5f), "web (cos a, 0, sin a), z flipped");
            Assert.That(Vector3.Distance(BulwarkFx20Math.BeamDir(0), Vector3.right), Is.LessThan(1e-5f));
            Assert.That(BulwarkFx20Math.BeamThick(true), Is.EqualTo(0.12f).Within(1e-6), "0.3 x 0.4 while it charges");
            Assert.That(BulwarkFx20Math.BeamThick(false), Is.EqualTo(0.3f).Within(1e-6));
            Assert.That(BulwarkFx20Math.BeamGain(true), Is.EqualTo(1.2f).Within(1e-6));
            Assert.That(BulwarkFx20Math.BeamGain(false), Is.EqualTo(4f).Within(1e-6));
        }

        [Test]
        public void TheBossBar_ReadsTheBulwarksBar()
        {
            var t = new HudReads20.Threat();
            BossThreat20.From(new BossBar { valid = false }, ref t);
            Assert.IsFalse(t.valid, "no fight: no boss bar (the elite may have it)");
            BossThreat20.From(new BossBar { valid = true, id = 77, name = "BULWARK", phase = "p2", hp = 1800, max = 4000, segment = 0.5, pips = 5, pipsOn = 3 }, ref t);
            Assert.IsTrue(t.valid);
            Assert.AreEqual(77, t.enemy);
            Assert.AreEqual("BULWARK", t.name);
            Assert.AreEqual(1800, t.hp);
            Assert.AreEqual(4000, t.maxHp);
            Assert.AreEqual(0.5, t.phaseAt, "the phase mark at p2At");
            Assert.AreEqual(5, t.pips, "the plates");
            Assert.AreEqual(3, t.pipsOn, "still on");
            Assert.IsNull(t.affixA);
        }

        [Test]
        public void VolatilesFuse_IsTheWebsTelegraphCircle_FullAtTheBurst()
        {
            TelegraphModel m = Model();
            var e = new AffixFxEvent { id = 9, position = new Vec3(2, 0.5, -3), radius = 4, time = 0.8 };
            int id = AffixFx20.VolatileTell(m, e);
            Assert.That(id, Is.GreaterThan(0));
            bool found = false;
            for (int i = 0; i < m.Slots; i++)
            {
                ref readonly TelegraphModel.Shape s = ref m.ShapeAt(i);
                if (s.id != id) continue;
                found = true;
                Assert.IsFalse(s.line);
                Assert.AreEqual(2, s.x);
                Assert.AreEqual(0.5, s.y);
                Assert.AreEqual(-3, s.z);
                Assert.AreEqual(4, s.r, "affixes.kinds.volatile.radius");
                Assert.AreEqual(UnityTuning.TelegraphDanger, s.hex);
            }
            Assert.IsTrue(found);
            for (int i = 0; i < 100; i++) m.Update(1.0 / 120); // 0.83 s: past the fuse, inside its hold
            Assert.That(m.Progress(id), Is.EqualTo(1).Within(1e-9), "full at the burst (the fuse: affixes.kinds.volatile.delay)");
        }
    }
}
