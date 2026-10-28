// NIGHT22 U3-36.4 THE STAMP (builder S): the inked stamp's motion (Core/Grades21.cs StampMotion), numbers design.n21.stamp. The
// contract: the entrance reads 200-300 ms the way feel-metrics M9 counts it (the first visible frame to the settled frame, at 60 fps),
// the exit 150-200 ms, and the first visible frame is not a pop-in. Seen red once: with the NIGHT21 inTime 0.16 the entrance read
// 140 ms (and outTime / fromScale were absent). The Player reel (FeelReel.N22.Stamp.cs) measures the same on screen.
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class Stamp22Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        [Test]
        public void TheEntrance_ReadsTwoToThreeHundredMs_AtSixtyFps()
        {
            StampMotion.P p = StampMotion.P.From(Cfg);
            double ms = StampMotion.EntranceMs(p, 60);
            Assert.That(ms, Is.InRange(200, 300), "NIGHT22.md U3-36.4: the inked letter enters in 200 to 300 ms");
        }

        [Test]
        public void TheExit_ReadsOneFiftyToTwoHundredMs_AtSixtyFps()
        {
            StampMotion.P p = StampMotion.P.From(Cfg);
            Assert.That(StampMotion.ExitMs(p, 60), Is.InRange(150, 200));
        }

        [Test]
        public void TheFirstVisibleFrame_IsNotAPopIn()
        {
            StampMotion.P p = StampMotion.P.From(Cfg);
            StampMotion.At(1 / 60.0, p, out double s, out double a);
            Assert.That(s, Is.GreaterThan(1.05), "far from rest scale");
            Assert.That(a, Is.LessThan(0.95), "and not at full alpha");
            StampMotion.At(-0.01, p, out _, out double before);
            Assert.That(before, Is.EqualTo(0), "nothing before the event");
            StampMotion.At(p.Total + 0.01, p, out _, out double after);
            Assert.That(after, Is.EqualTo(0), "nothing after the exit");
        }

        [Test]
        public void TheHold_IsAtRest()
        {
            StampMotion.P p = StampMotion.P.From(Cfg);
            StampMotion.At(p.inTime + p.holdTime * 0.5, p, out double s, out double a);
            Assert.That(s, Is.EqualTo(1).Within(1e-9));
            Assert.That(a, Is.EqualTo(1).Within(1e-9));
        }

        [Test]
        public void TheConfig_HasAWhyForEveryStampMotionKey()
        {
            string path = Path.Combine(Port.Root, "..", "Design", "unity-config.json");
            if (!File.Exists(path)) Assert.Ignore("Design/unity-config.json not beside PortExport");
            string text = File.ReadAllText(path);
            foreach (string k in new[] { "inTime", "holdTime", "outTime", "fromScale", "outScale" })
                Assert.That(text, Does.Contain("\"" + k + "_why\""), "design.n21.stamp." + k + " has its why");
        }
    }
}
