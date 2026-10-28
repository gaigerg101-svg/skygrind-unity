// NIGHT16 polish #35 (builder U): the web's move row (hud.js :127-133, :329, :907-912): the speed's colour band, its
// tenths, the wall-jump ticks (how many, which are on). Pure: MoveRowMath (UiMoveRow.cs).
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class MoveRowTests
    {
        [Test]
        public void TheSpeedIsWhiteUnderMidCyanFromMidAndDangerAboveHigh()
        {
            // hud.js :907: s.speed > speedHigh ? enemy : s.speed >= speedMid ? usable : white (config hud.speedMid 12, speedHigh 24)
            Assert.AreEqual(MoveRowMath.White, MoveRowMath.SpeedBand(0, 12, 24));
            Assert.AreEqual(MoveRowMath.White, MoveRowMath.SpeedBand(11.99, 12, 24));
            Assert.AreEqual(MoveRowMath.Usable, MoveRowMath.SpeedBand(12, 12, 24), "mid is inclusive");
            Assert.AreEqual(MoveRowMath.Usable, MoveRowMath.SpeedBand(24, 12, 24), "high is exclusive");
            Assert.AreEqual(MoveRowMath.Danger, MoveRowMath.SpeedBand(24.01, 12, 24));
        }

        [Test]
        public void TheReadoutChangesOnlyWhenItsTenthsChange()
        {
            Assert.AreEqual(0, MoveRowMath.Tenths(0));
            Assert.AreEqual(123, MoveRowMath.Tenths(12.34));
            Assert.AreEqual(123, MoveRowMath.Tenths(12.29));
            Assert.AreEqual(240, MoveRowMath.Tenths(23.96));
            Assert.AreEqual(0, MoveRowMath.Tenths(double.NaN), "no speed yet: 0.0");
        }

        [Test]
        public void TheTicksAreTheAirtimesWallJumpsPlusGrips()
        {
            // hud.js :284: maxTicksShown = wallJump.maxPerAirtime + perks.grip.wallJumps
            Assert.AreEqual(3, MoveRowMath.TickCount(2, 1));
            Assert.AreEqual(2, MoveRowMath.TickCount(2, 0));
            Assert.AreEqual(0, MoveRowMath.TickCount(-1, 0), "never negative");
            // hud.js :912: tick i is on while i < wallJumpsLeft
            Assert.IsTrue(MoveRowMath.TickOn(0, 2));
            Assert.IsTrue(MoveRowMath.TickOn(1, 2));
            Assert.IsFalse(MoveRowMath.TickOn(2, 2));
            Assert.IsFalse(MoveRowMath.TickOn(0, 0));
        }
    }
}
