// NIGHT63 builder D (leftover 4: R1's N5, Logs/n62/R1.md; Logs/n61/review-n61.md N5 "the one-row off switches disagree on case"): one helper for
// every row's own off switch (-skygrindNo<Row>NN), NightRows.HasSwitch, Rim58.No's rule. Pinned:
//   1. the switch itself turns it on, in any case (typed by hand into a reel script: "-skygrindnobossrule61" is the same switch)
//   2. only that switch: a longer or shorter word, another row's switch, a value after it, nothing, a null list: not the switch
// Written test first (NIGHT63.md section 4): RED on its commit (HasSwitch's placeholder is the old case-sensitive Array.IndexOf).
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N63SwitchTests
    {
        [Test]
        public void TheSwitch_InAnyCase()
        {
            Assert.That(NightRows.HasSwitch(new[] { "x.exe", "-skygrindNoBossRule61" }, "-skygrindNoBossRule61"), Is.True, "as written");
            Assert.That(NightRows.HasSwitch(new[] { "x.exe", "-skygrindnobossrule61" }, "-skygrindNoBossRule61"), Is.True, "lower case");
            Assert.That(NightRows.HasSwitch(new[] { "-SKYGRINDNOTEXT61" }, "-skygrindNoText61"), Is.True, "upper case");
        }

        [Test]
        public void OnlyThatSwitch()
        {
            Assert.That(NightRows.HasSwitch(new[] { "-skygrindNoText61x" }, "-skygrindNoText61"), Is.False, "a longer word");
            Assert.That(NightRows.HasSwitch(new[] { "-skygrindNoText6" }, "-skygrindNoText61"), Is.False, "a shorter word");
            Assert.That(NightRows.HasSwitch(new[] { "-skygrindNoBossRule61" }, "-skygrindNoText61"), Is.False, "another row's switch");
            Assert.That(NightRows.HasSwitch(new[] { "-skygrindMap", "-skygrindNoText61x" }, "-skygrindNoText61"), Is.False);
            Assert.That(NightRows.HasSwitch(new string[0], "-skygrindNoText61"), Is.False, "nothing");
            Assert.That(NightRows.HasSwitch(null, "-skygrindNoText61"), Is.False, "no list");
            Assert.That(NightRows.HasSwitch(new[] { "-skygrindNoText61" }, null), Is.False, "no switch asked");
        }
    }
}
