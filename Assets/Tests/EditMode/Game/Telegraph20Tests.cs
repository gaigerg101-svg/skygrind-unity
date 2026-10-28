// NIGHT20 (builder V): the web's telegraph language ported (Game/Telegraph20.cs, src/fx/telegraph.js): a line (the BULWARK's
// RAM) fills over its wind-up and holds config.telegraph.hold s full, then goes; a circle the same; a hazard turns hot after
// its warn; a lane is evenly full for its life; move / cancel by id; a flash lasts flashTime; a threat arrow arrowTime; the
// pools never overflow (the oldest flash / threat gives way); the rim pulses faster as the moment nears and stops on it.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Telegraph20Tests
    {
        static TelegraphModel Make() => new TelegraphModel(new TelegraphModel.Settings
        {
            max = 48, maxFlashes = 12, maxThreats = 8, hold = 0.12, arrowTime = 1.2, flashTime = 0.15, // config.telegraph
        });

        static void Run(TelegraphModel t, double seconds)
        {
            for (double s = 0; s < seconds - 1e-9; s += 1.0 / 120) t.Update(1.0 / 120);
        }

        [Test]
        public void ARamLineFillsOverItsWindUpHoldsThenGoes()
        {
            TelegraphModel t = Make();
            int id = t.Line(0, 0, 0, 3, 4, 20, 2.5, 1.2);
            Assert.That(id, Is.GreaterThan(0));
            Assert.That(t.Progress(id), Is.EqualTo(0));
            Run(t, 0.6);
            Assert.That(t.Progress(id), Is.EqualTo(0.5).Within(0.01), "half full half way through the 1.2 s wind-up");
            Run(t, 0.6);
            Assert.That(t.Progress(id), Is.EqualTo(1).Within(1e-9), "full: it hits now");
            Run(t, 0.1);
            Assert.That(t.Count, Is.EqualTo(1), "held full for hold s (the hit moment)");
            Run(t, 0.05);
            Assert.That(t.Count, Is.EqualTo(0), "then gone");
            Assert.That(t.Progress(id), Is.EqualTo(-1), "an unknown id: -1");
        }

        [Test]
        public void AHazardTurnsHotAfterItsWarnAndALaneStaysFaint()
        {
            TelegraphModel t = Make();
            int h = t.Hazard(1, 0, 1, 2, 0.5, 1);
            Run(t, 0.4);
            Assert.That(t.Hot(h), Is.False);
            Run(t, 0.2);
            Assert.That(t.Hot(h), Is.True, "hot after its warn");
            Run(t, 1.0);
            Assert.That(t.Hot(h), Is.False, "gone after warn + life");
            Assert.That(TelegraphMath.FillLength(10, 0.3, true), Is.EqualTo(10), "a lane: evenly full from the start");
            Assert.That(TelegraphMath.FillLength(10, 0.3, false), Is.EqualTo(3).Within(1e-12), "a danger line fills along its length");
        }

        [Test]
        public void ShapesMoveAndCancelByIdAndThePoolsHold()
        {
            TelegraphModel t = Make();
            int a = t.Circle(0, 0, 0, 3, 1);
            int b = t.Line(0, 0, 0, 1, 0, 10, 2, 1);
            t.Move(b, 5, 0, 5, 0, -2);
            t.Cancel(a);
            Assert.That(t.Count, Is.EqualTo(1));
            Assert.That(t.Progress(a), Is.EqualTo(-1));
            for (int i = 0; i < 100; i++) t.Circle(i, 0, 0, 1, 5);
            Assert.That(t.Count, Is.LessThanOrEqualTo(48), "never past config.telegraph.max");
            for (int i = 0; i < 30; i++) t.Flash(0, 0, 0);
            Assert.That(t.FlashCount, Is.EqualTo(12), "the oldest flash gives way");
            for (int i = 0; i < 30; i++) t.Threat(0, 0, 0);
            Assert.That(t.ThreatCount, Is.EqualTo(8));
            Run(t, 0.16);
            Assert.That(t.FlashCount, Is.EqualTo(0), "a flash lasts flashTime");
            Run(t, 1.1);
            Assert.That(t.ThreatCount, Is.EqualTo(0), "a threat arrow arrowTime");
            t.Clear();
            Assert.That(t.Count, Is.EqualTo(0));
        }

        [Test]
        public void TheRimPulsesFasterAsTheMomentNears()
        {
            Assert.That(TelegraphMath.Pulse(0.3, 0.5, true), Is.EqualTo(0.75), "full: steady (the hit moment)");
            Assert.That(TelegraphMath.Pulse(0, 0, false), Is.EqualTo(0.75).Within(1e-12), "0.75 + 0.25 sin(...)");
            double max = 0, min = 1;
            for (double s = 0; s < 1; s += 0.001)
            {
                double p = TelegraphMath.Pulse(s, 0.5, false);
                max = System.Math.Max(max, p);
                min = System.Math.Min(min, p);
            }
            Assert.That(max, Is.EqualTo(1).Within(0.001));
            Assert.That(min, Is.EqualTo(0.5).Within(0.001));
            Assert.That(TelegraphMath.ClosingRing(4, 0), Is.EqualTo(4).Within(1e-12), "the closing ring starts at the rim ...");
            Assert.That(TelegraphMath.ClosingRing(4, 0.5), Is.EqualTo(4 * (1 - 0.35 * 0.75)).Within(1e-9), "... and tightens toward 0.65 r");
        }
    }
}
