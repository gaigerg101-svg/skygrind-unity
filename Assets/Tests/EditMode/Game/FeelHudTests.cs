// NIGHT16 U3-29.7 (builder U): the movement HUD at the crosshair (the dash pips and the slide charge: shown while something
// is spent, faded 1 s after everything is full), the screens' transitions, the rank plate's zero state, and the strip
// tool's sheet layout and stamps. Pure functions only (no scene): FeelHudMath (UiFeelHud.cs), FeelStrip's layout (FeelStrip.cs).
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class FeelHudTests
    {
        const double Hold = 1.0, Fade = 0.25;

        // --- the movement HUD ------------------------------------------------------------------------------------------

        [Test]
        public void PipFillIsEachPipsShareOfTheStamina()
        {
            Assert.AreEqual(1.0, FeelHudMath.PipFill(2.5, 0), 1e-12);
            Assert.AreEqual(1.0, FeelHudMath.PipFill(2.5, 1), 1e-12);
            Assert.AreEqual(0.5, FeelHudMath.PipFill(2.5, 2), 1e-12);
            Assert.AreEqual(0.0, FeelHudMath.PipFill(0.4, 1), 1e-12);
            Assert.AreEqual(0.0, FeelHudMath.PipFill(-1, 0), 1e-12);
        }

        [Test]
        public void AllFullNeedsEveryPipAndTheSlideCharge()
        {
            Assert.IsTrue(FeelHudMath.AllFull(3, 3, 1));
            Assert.IsTrue(FeelHudMath.AllFull(4, 3, 1)); // a perk's extra pip lost: never "spent" above max
            Assert.IsFalse(FeelHudMath.AllFull(2.99, 3, 1));
            Assert.IsFalse(FeelHudMath.AllFull(3, 3, 0.5));
        }

        [Test]
        public void ShownWhileSpentThenHeldOneSecondThenFaded()
        {
            double t = FeelHudMath.HiddenFor(Hold, Fade); // a run starts full: hidden
            Assert.AreEqual(0, FeelHudMath.Alpha(t, Hold, Fade), 1e-12, "full at the start: hidden");
            t = FeelHudMath.FullFor(t, false, 1 / 60.0); // a dash spends a pip
            Assert.AreEqual(0, t, 1e-12);
            Assert.AreEqual(1, FeelHudMath.Alpha(t, Hold, Fade), 1e-12, "spent: shown");
            for (int i = 0; i < 30; i++) t = FeelHudMath.FullFor(t, true, 1 / 60.0); // full again for 0.5 s
            Assert.AreEqual(1, FeelHudMath.Alpha(t, Hold, Fade), 1e-12, "full for 0.5 s: still shown (the 1 s hold)");
            Assert.AreEqual(1, FeelHudMath.Alpha(Hold, Hold, Fade), 1e-12, "the hold's end: still full");
            Assert.AreEqual(0.5, FeelHudMath.Alpha(Hold + Fade / 2, Hold, Fade), 1e-9, "half way through the fade");
            Assert.AreEqual(0, FeelHudMath.Alpha(Hold + Fade, Hold, Fade), 1e-12, "faded");
            Assert.AreEqual(0, FeelHudMath.Alpha(Hold + 10, Hold, Fade), 1e-12, "stays faded");
            Assert.AreEqual(1, FeelHudMath.Alpha(FeelHudMath.FullFor(Hold + 10, false, 0.01), Hold, Fade), 1e-12, "a new spend shows it at once");
        }

        [Test]
        public void TheClockStopsGrowingOnceHidden()
        {
            // No unbounded growth (a run of hours): FullFor saturates at the hidden point.
            double t = FeelHudMath.HiddenFor(Hold, Fade);
            Assert.AreEqual(t, FeelHudMath.FullFor(t, true, 5), 1e-12);
        }

        [Test]
        public void APulseRisesToItsPeakAndSettlesBack()
        {
            Assert.AreEqual(1, FeelHudMath.Pulse(0, 0.15, 1.4), 1e-12);
            Assert.AreEqual(1.4, FeelHudMath.Pulse(0.075, 0.15, 1.4), 1e-9, "the peak half way");
            Assert.AreEqual(1, FeelHudMath.Pulse(0.15, 0.15, 1.4), 1e-12);
            Assert.AreEqual(1, FeelHudMath.Pulse(-1, 0.15, 1.4), 1e-12, "no pulse running");
        }

        // --- the transitions and the rank plate --------------------------------------------------------------------------

        [Test]
        public void TransitionAlphaRisesOverItsTimeAndNeverOvershoots()
        {
            Assert.AreEqual(0, FeelHudMath.TransitionAlpha(0, 0.2), 1e-12);
            double mid = FeelHudMath.TransitionAlpha(0.1, 0.2);
            Assert.That(mid, Is.GreaterThan(0.2).And.LessThan(1));
            Assert.AreEqual(1, FeelHudMath.TransitionAlpha(0.2, 0.2), 1e-12);
            Assert.AreEqual(1, FeelHudMath.TransitionAlpha(5, 0.2), 1e-12);
            Assert.AreEqual(1, FeelHudMath.TransitionAlpha(0, 0), 1e-12, "no time: no transition");
        }

        [Test]
        public void TheRankPlateWaitsForTheFirstStyle()
        {
            Assert.IsFalse(FeelHudMath.RankPlateShown(false, 0, 0), "the run's start: no DULL plate");
            Assert.IsTrue(FeelHudMath.RankPlateShown(true, 0, 0), "earned once this run: never hidden again");
            Assert.IsTrue(FeelHudMath.RankPlateShown(false, 0, 0.1), "the meter has something");
            Assert.IsTrue(FeelHudMath.RankPlateShown(false, 2, 0), "a rank above the lowest");
        }

        // --- the strip tool's sheet --------------------------------------------------------------------------------------

        [Test]
        public void TheSheetGridIsNearSquareForOneGroupAndRowsPerGroup()
        {
            FeelStrip.Grid(10, 1, out int cols, out int rows);
            Assert.AreEqual(4, cols);
            Assert.AreEqual(3, rows);
            FeelStrip.Grid(16, 1, out cols, out rows);
            Assert.AreEqual((4, 4), (cols, rows));
            FeelStrip.Grid(1, 1, out cols, out rows);
            Assert.AreEqual((1, 1), (cols, rows));
            FeelStrip.Grid(10, 6, out cols, out rows); // swap: one row per gun
            Assert.AreEqual((10, 6), (cols, rows));
        }

        [Test]
        public void CellsKeepTheFramesAspectAndReadTopLeftFirst()
        {
            Assert.AreEqual(270, FeelStrip.CellHeight(480, 1920, 1080));
            Assert.AreEqual(300, FeelStrip.CellHeight(480, 1920, 1200));
            // Texture rows go up from the bottom: frame 0 sits in the top row's left cell.
            FeelStrip.CellOrigin(0, 4, 3, 480, 270, out int x, out int y);
            Assert.AreEqual((0, 540), (x, y));
            FeelStrip.CellOrigin(3, 4, 3, 480, 270, out x, out y);
            Assert.AreEqual((1440, 540), (x, y));
            FeelStrip.CellOrigin(4, 4, 3, 480, 270, out x, out y);
            Assert.AreEqual((0, 270), (x, y));
            FeelStrip.CellOrigin(9, 4, 3, 480, 270, out x, out y);
            Assert.AreEqual((480, 0), (x, y));
        }

        [Test]
        public void AGroupWrapsIntoItsOwnRowsAndStartsOnANewRow()
        {
            // swap: 36 frames per gun at 12 a row: 3 rows per gun, 6 guns: 18 rows.
            FeelStrip.GroupGrid(36, 6, 12, out int cols, out int rowsPer, out int rows);
            Assert.AreEqual((12, 3, 18), (cols, rowsPer, rows));
            Assert.AreEqual(0, FeelStrip.GroupCell(0, 0, cols, rowsPer));
            Assert.AreEqual(13, FeelStrip.GroupCell(0, 13, cols, rowsPer)); // the gun's second row
            Assert.AreEqual(36, FeelStrip.GroupCell(1, 0, cols, rowsPer)); // the next gun starts its own row
            // draw: 40 frames at 10 a row, 2 guns: 4 rows each.
            FeelStrip.GroupGrid(40, 2, 10, out cols, out rowsPer, out rows);
            Assert.AreEqual((10, 4, 8), (cols, rowsPer, rows));
            Assert.AreEqual(40, FeelStrip.GroupCell(1, 0, cols, rowsPer));
            // a short group never makes empty columns
            FeelStrip.GroupGrid(5, 2, 12, out cols, out rowsPer, out rows);
            Assert.AreEqual((5, 1, 2), (cols, rowsPer, rows));
        }

        [Test]
        public void TheStampScalesSoItReadsTheSameOnEverySheet()
        {
            Assert.AreEqual(48, FeelStrip.StampSize(16, 640, 1920), 1e-4);
            Assert.AreEqual(80, FeelStrip.StampSize(16, 384, 1920), 1e-4);
            Assert.AreEqual(16, FeelStrip.StampSize(16, 1920, 1920), 1e-4, "a full-size cell: the sheet's size");
        }

        [Test]
        public void TurningRightMovesTheLookTowardTheRightHand()
        {
            // yaw 0 looks toward -z, where the right hand is +x: forward = (-sin yaw, -cos yaw).
            double yaw = FeelStrip.TurnRight(0, 12);
            Assert.That(-System.Math.Sin(yaw), Is.GreaterThan(0.2));
            Assert.AreEqual(-12 * System.Math.PI / 180, yaw, 1e-12);
            Assert.AreEqual(0.3 + 12 * System.Math.PI / 180, FeelStrip.TurnRight(0.3, -12), 1e-12);
        }

        [Test]
        public void TheStampCarriesRealTimeTheStepAndAHeldMark()
        {
            string s = FeelStrip.Stamp("freeze", 3, 10, 41.66, 120345, 120340, 120343);
            StringAssert.Contains("FREEZE", s);
            StringAssert.Contains("4/10", s); // frames count from 1 on the sheet
            StringAssert.Contains("+41.7 MS", s);
            StringAssert.Contains("STEP 120345", s);
            StringAssert.Contains("+5", s);
            StringAssert.DoesNotContain("HELD", s);
            string held = FeelStrip.Stamp("freeze", 4, 10, 48.1, 120345, 120340, 120345);
            StringAssert.Contains("HELD", held); // no sim step since the frame before: frozen (or slowed)
            StringAssert.DoesNotContain("HELD", FeelStrip.Stamp("freeze", 0, 10, 0, 120340, 120340, 120340)); // the trigger frame
        }
    }
}
