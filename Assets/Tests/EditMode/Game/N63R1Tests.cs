// NIGHT63 builder D (leftover 4: R1's N1 and N5; Logs/n62/R1.md, Logs/n61/review-n61.md). Pinned:
//   N5. every row's own off switch reads through NightRows.HasSwitch (case ignored): bossRule61 and text61 (case-sensitive until now) turn off
//       under their switch in any case, and the rows that already ignored case still do; another row's switch still leaves each on
//   N1. the exit frame makes its one-time cube and material only when it builds: never while the row is off or on another level
// Written test first (NIGHT63.md section 4): RED on its commit (HasSwitch's placeholder is case-sensitive; MakesAssets' placeholder always true).
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N63R1Tests
    {
        [Test]
        public void N5_EveryRowsOwnSwitch_InAnyCase()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                Assert.That(BossRule61.Off(new[] { "-skygrindNoBossRule61" }), Is.True, "bossRule61 as written");
                Assert.That(BossRule61.Off(new[] { "-skygrindnobossrule61" }), Is.True, "bossRule61 in lower case (case-sensitive until now)");
                Assert.That(BossRule61.Off(new[] { "-skygrindNoText61" }), Is.False, "another row's switch leaves bossRule61 on");
                Assert.That(Text61.Off(new[] { "-skygrindNoText61" }), Is.True, "text61 as written");
                Assert.That(Text61.Off(new[] { "-SKYGRINDNOTEXT61" }), Is.True, "text61 in upper case (case-sensitive until now)");
                Assert.That(Text61.Off(new[] { "-skygrindNoBossRule61" }), Is.False, "another row's switch leaves text61 on");
                Assert.That(Colours63Math.Off(new[] { "-skygrindnocolours63" }), Is.True, "colours63 (already ignored case)");
                Assert.That(Colours63Math.Off(new[] { "-skygrindNoBossColours63" }), Is.False, "B3's switch leaves colours63 on");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void N1_TheExitFramesAssets_OnlyWhenItBuilds()
        {
            Assert.That(ExitFrame61Math.MakesAssets("design.n61.exitFrame61 off", ExitFrame61.Level), Is.False, "the row off: no cube, no material");
            Assert.That(ExitFrame61Math.MakesAssets(ExitFrame61Math.NoSwitch, ExitFrame61.Level), Is.False, "its switch: none");
            Assert.That(ExitFrame61Math.MakesAssets(null, "n52-span"), Is.False, "another level: none (nothing is framed there)");
            Assert.That(ExitFrame61Math.MakesAssets(null, null), Is.False, "no map: none");
            Assert.That(ExitFrame61Math.MakesAssets(null, ExitFrame61.Level), Is.True, "on, on its level: made once");
        }
    }
}
