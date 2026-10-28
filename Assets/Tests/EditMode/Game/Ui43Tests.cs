// NIGHT43 builder U1: the dusk instrument's pure rules (Ui43Math, Hud43Math; UiKit43.cs, UiHud33.Hud43.cs). Pure: no scene, no sim.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Ui43Tests
    {
        const double Dt = 1.0 / 60;

        [Test]
        public void Pop_GrowsXThenY_AndEndsAtOne()
        {
            Ui43Math.Pop(0, out float sx, out float sy);
            Assert.That(sx, Is.EqualTo(UnityTuning.Ui43OpenFrom).Within(1e-4));
            Assert.That(sy, Is.EqualTo(UnityTuning.Ui43OpenFrom).Within(1e-4));
            Ui43Math.Pop(UnityTuning.Ui43OpenX * 0.5, out sx, out sy);
            Assert.That(sx, Is.GreaterThan(0.5f), "X grows first");
            Assert.That(sy, Is.EqualTo(UnityTuning.Ui43OpenFrom).Within(1e-4), "Y waits for X");
            Ui43Math.Pop(UnityTuning.Ui43OpenX + UnityTuning.Ui43OpenY * 0.5, out sx, out sy);
            Assert.That(sx, Is.EqualTo(1f));
            Assert.That(sy, Is.InRange(0.5f, 1f));
            Ui43Math.Pop(Ui43Math.PopTime + Dt, out sx, out sy);
            Assert.That(sx, Is.EqualTo(1f));
            Assert.That(sy, Is.EqualTo(1f));
            Assert.That(Ui43Math.PopTime, Is.InRange(0.15, 0.2), "ULTRAKILL's ~0.16 s pop");
        }

        [Test]
        public void Close_CollapsesYThenX_ThenDone()
        {
            Ui43Math.Close(0, out float sx, out float sy, out bool done);
            Assert.That(sx, Is.EqualTo(1f));
            Assert.That(sy, Is.EqualTo(1f).Within(1e-4));
            Assert.That(done, Is.False);
            Ui43Math.Close(UnityTuning.Ui43CloseY + UnityTuning.Ui43CloseX * 0.5, out sx, out sy, out done);
            Assert.That(sy, Is.EqualTo(UnityTuning.Ui43OpenFrom).Within(1e-4));
            Assert.That(done, Is.False);
            Ui43Math.Close(UnityTuning.Ui43CloseY + UnityTuning.Ui43CloseX + Dt, out sx, out sy, out done);
            Assert.That(done, Is.True);
        }

        [Test]
        public void After_WaitsThenDrains_NeverUnderTheFill()
        {
            double wait = 0;
            float after = Ui43Math.After(1f, 1f, 0.7f, ref wait, Dt); // a 30 % hit
            Assert.That(after, Is.EqualTo(1f));
            Assert.That(wait, Is.EqualTo(UnityTuning.Ui43AfterWait));
            double t = 0;
            while (t < UnityTuning.Ui43AfterWait - 1.5 * Dt) { after = Ui43Math.After(after, 0.7f, 0.7f, ref wait, Dt); t += Dt; }
            Assert.That(after, Is.EqualTo(1f), "held through the 0.15 s wait");
            for (int i = 0; i < 60; i++) after = Ui43Math.After(after, 0.7f, 0.7f, ref wait, Dt);
            Assert.That(after, Is.EqualTo(0.7f), "drained to the fill within a second (ULTRAKILL's rate: a 0.3 gap drains in ~0.4 s)");
            // a heal lifts it with the fill
            after = Ui43Math.After(after, 0.7f, 0.9f, ref wait, Dt);
            Assert.That(after, Is.EqualTo(0.9f));
        }

        [Test]
        public void Tick_PunchesAndSettles()
        {
            Assert.That(Ui43Math.Tick(double.MaxValue), Is.EqualTo(1f));
            Assert.That(Ui43Math.Tick(UnityTuning.Ui43TickTime * 0.25), Is.EqualTo(UnityTuning.Ui43TickScale).Within(1e-3));
            Assert.That(Ui43Math.Tick(UnityTuning.Ui43TickTime), Is.EqualTo(1f));
        }

        [Test]
        public void TeachSplit_FirstSentenceIsTheHead()
        {
            Ui43Math.TeachSplit("HOLD RT to CHARGE the KESTREL. Release to fire.", out string h, out string s);
            Assert.That(h, Is.EqualTo("HOLD RT to CHARGE the KESTREL"));
            Assert.That(s, Is.EqualTo("Release to fire."));
            Ui43Math.TeachSplit("HOLD <sprite name=\"b.x\">: CHAIN THE WALLS.", out h, out s);
            Assert.That(h, Is.EqualTo("HOLD <sprite name=\"b.x\">: CHAIN THE WALLS"), "a dot inside a tag never splits; the lone full stop goes");
            Assert.That(s, Is.EqualTo(""));
            Ui43Math.TeachSplit("HOLD LT: SLOW. RT: PIERCE", out h, out s);
            Assert.That(h, Is.EqualTo("HOLD LT: SLOW"));
            Assert.That(s, Is.EqualTo("RT: PIERCE"));
            Ui43Math.TeachSplit("MOVE. LOOK. <sprite name=\"b\"> JUMP.", out h, out s);
            Assert.That(h, Is.EqualTo("MOVE. LOOK. <sprite name=\"b\"> JUMP"), "the n42-r4 first line: a chain of short verbs stays one line");
            Assert.That(s, Is.EqualTo(""));
            Ui43Math.TeachSplit(null, out h, out s);
            Assert.That(h, Is.EqualTo(""));
        }

        [Test]
        public void Mode_OpenOverLocked()
        {
            Assert.That(Ui43Math.Mode(false, true, true), Is.EqualTo(Ui43Math.BossMode.None));
            Assert.That(Ui43Math.Mode(true, true, true), Is.EqualTo(Ui43Math.BossMode.Open));
            Assert.That(Ui43Math.Mode(true, false, true), Is.EqualTo(Ui43Math.BossMode.Locked));
            Assert.That(Ui43Math.Mode(true, false, false), Is.EqualTo(Ui43Math.BossMode.Normal));
        }

        [Test]
        public void Strings_AreCachedAndRight()
        {
            Assert.That(Ui43Math.Num(7), Is.EqualTo("7"));
            Assert.That(ReferenceEquals(Ui43Math.Num(7), Ui43Math.Num(7)), Is.True, "no allocation per frame");
            Assert.That(Ui43Math.Leg(2, 3), Is.EqualTo("LEG 2 / 3"));
            Assert.That(Ui43Math.Leg(0, 3), Is.EqualTo(""));
            Assert.That(Ui43Math.Tenths(1.41), Is.EqualTo("1.5"));
            Assert.That(Ui43Math.Tenths(0), Is.EqualTo("0.0"));
            Assert.That(Ui43Math.Pips(30), Is.EqualTo(UnityTuning.Counter43MaxPips));
            Assert.That(Ui43Math.Pips(-1), Is.EqualTo(0));
        }

        [Test]
        public void ShowAward_HidesZero()
        {
            Assert.That(Ui43Math.ShowAward(0), Is.False, "T-64: '+0 WALL JUMP'");
            Assert.That(Ui43Math.ShowAward(0.4), Is.False);
            Assert.That(Ui43Math.ShowAward(25), Is.True);
        }

        [Test]
        public void Cells_FillLeftToRight()
        {
            Assert.That(Hud43Math.Cell(0.35f, 2, 10), Is.EqualTo(1f));
            Assert.That(Hud43Math.Cell(0.35f, 3, 10), Is.EqualTo(0.5f).Within(1e-4));
            Assert.That(Hud43Math.Cell(0.35f, 4, 10), Is.EqualTo(0f));
            Assert.That(Hud43Math.Cells(0.35f, 10), Is.EqualTo(4));
            Assert.That(Hud43Math.Cells(1f, 10), Is.EqualTo(10));
            Assert.That(Hud43Math.Cells(0f, 10), Is.EqualTo(0));
        }

        [Test]
        public void Fall43_BlinksToDuskAndBack()
        {
            Assert.That(Ui.Fall43Alpha(0), Is.EqualTo(0f));
            Assert.That(Ui.Fall43Alpha(UnityTuning.Fall43Out + 0.01), Is.EqualTo(UnityTuning.Fall43Peak), "held dark");
            Assert.That(Ui.Fall43Alpha(UnityTuning.Fall43Out + UnityTuning.Fall43Hold + UnityTuning.Fall43In * 0.5), Is.InRange(0.01f, UnityTuning.Fall43Peak - 0.01f));
            Assert.That(Ui.Fall43Alpha(UnityTuning.Fall43Out + UnityTuning.Fall43Hold + UnityTuning.Fall43In + 0.01), Is.EqualTo(0f));
            Assert.That(UnityTuning.Fall43Out + UnityTuning.Fall43Hold + UnityTuning.Fall43In, Is.EqualTo(0.5).Within(1e-9), "the lead's 0.25 + 0.25 s envelope");
        }

        [Test]
        public void Shown_ClampsToTheMaxAndFourDigits()
        {
            Assert.That(Hud43Math.Shown(87.2, 120), Is.EqualTo(88));
            Assert.That(Hud43Math.Shown(1e6, 1e6), Is.EqualTo(UnityTuning.Hud43NumCap), "n43-r1 t-hold: god mode's 1000000 ran into the bar");
            Assert.That(Hud43Math.Shown(130, 120), Is.EqualTo(120));
            Assert.That(Hud43Math.Shown(double.NaN, 100), Is.EqualTo(0));
        }
    }
}
