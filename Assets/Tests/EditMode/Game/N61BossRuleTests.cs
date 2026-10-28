// NIGHT61 builder G (S3, design.n61.bossRule61; Game/BossRule61.cs, Game/UiBossRule61.cs): the boss rule line's pure rules. Pinned:
//   1. the switch: on only with the row on and no off switch: -skygrindOff61 or an older night's (-skygrindOff60, -skygrindOff47) turns it off,
//      -skygrindNoBossRule61 turns it alone off; a later night's switch (-skygrindOff62) or another row's (-skygrindNoText61) does not
//   2. the five campaign bosses by their bossRise ids (Wick37.cs:219 "wick", Brawl40.cs:297 "brawl40", Wyrm.cs:526 "wyrm", Loom37.cs:227
//      "loom37", Crown37.cs:202 "crown37"), each with a line in the config of at most BossRule61MaxWords words naming a button; THE STAG
//      ("stag37"), the survival bosses and any other id have none
//   3. Words: runs of non-space characters ("{fire}" one word, "embers," one word; repeated spaces count nothing; null and "" are 0);
//      HasGlyph: a closed, named {token}
//   4. the time on screen: fade in + hold + fade out <= BossRule61MaxS (5 s, the slate's S3) for any length; the hold the text system's
//      2.0 s + 0.3 s a word when that fits; the alpha 0 before the line and after it, 1 through its hold, between 0 and 1 in its fades; the
//      scale from BossRule61ScaleFrom to 1 over the fade-in
// Written test-first: RED on its own commit (the placeholder: always off, no bosses, no words, no time). Not run by its author (no Editor).
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N61BossRuleTests
    {
        static GameConfig Cfg()
        {
            string root;
            try { root = System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { root = System.IO.Path.GetFullPath("PortExport"); }
            return GameConfig.Load(System.IO.Path.Combine(root, "config.json"));
        }

        [Test]
        public void Switch_RowOn_OffSwitchesTurnItOff()
        {
            Assert.IsTrue(BossRule61.Decide(true, new string[0]), "the row on, no switch: on");
            Assert.IsFalse(BossRule61.Decide(false, new string[0]), "the row off: off");
            Assert.IsFalse(BossRule61.Decide(true, new[] { "-skygrindOff61" }), "-skygrindOff61");
            Assert.IsFalse(BossRule61.Decide(true, new[] { "-skygrindOff60" }), "an older night's switch covers night 61");
            Assert.IsFalse(BossRule61.Decide(true, new[] { "-skygrindOff47" }), "-skygrindOff47 (NIGHT46's game)");
            Assert.IsFalse(BossRule61.Decide(true, new[] { BossRule61.NoSwitch }), "the one-row switch");
            Assert.AreEqual("-skygrindNoBossRule61", BossRule61.NoSwitch);
            Assert.IsTrue(BossRule61.Decide(true, new[] { "-skygrindOff62" }), "a later night's switch leaves night 61's rows on");
            Assert.IsTrue(BossRule61.Decide(true, new[] { "-skygrindNoText61" }), "another row's switch leaves it on");
            Assert.IsTrue(BossRule61.Decide(true, null), "no arguments: on");
        }

        [Test]
        public void TheFiveCampaignBosses_ByTheirRiseIds_EachHasALine()
        {
            CollectionAssert.AreEqual(new[] { "wick", "brawl40", "wyrm", "loom37", "crown37" }, BossRule61.Bosses);
            GameConfig c = Cfg();
            foreach (string b in BossRule61.Bosses)
            {
                Assert.IsTrue(BossRule61.IsBoss(b), b);
                string line = BossRule61.Line(c, b);
                Assert.That(line, Is.Not.Null.And.Not.Empty, b + "'s line");
                Assert.That(BossRule61.Words(line), Is.InRange(1, UnityTuning.BossRule61MaxWords), b + ": \"" + line + "\"");
                Assert.IsTrue(BossRule61.HasGlyph(line), b + " names a button: \"" + line + "\"");
            }
            foreach (string other in new[] { "stag37", "lantern36", "bulwark", "" })
            {
                Assert.IsFalse(BossRule61.IsBoss(other), "not a campaign boss: " + other);
                Assert.IsNull(BossRule61.Line(c, other), "no line for " + other);
            }
            Assert.IsFalse(BossRule61.IsBoss(null), "null");
            Assert.IsNull(BossRule61.Line(c, null), "no line for null");
            Assert.IsNull(BossRule61.Line(null, "wick"), "no config: no line");
        }

        [Test]
        public void Words_CountsRunsOfNonSpace_HasGlyph_ANamedToken()
        {
            Assert.AreEqual(0, BossRule61.Words(null));
            Assert.AreEqual(0, BossRule61.Words(""));
            Assert.AreEqual(0, BossRule61.Words("   "));
            Assert.AreEqual(1, BossRule61.Words("{fire}"));
            Assert.AreEqual(7, BossRule61.Words("Parry {punch} the yellow embers, then shoot."));
            Assert.AreEqual(3, BossRule61.Words("  a  b   c "));
            Assert.IsTrue(BossRule61.HasGlyph("Hook a lit knee {grapple}."));
            Assert.IsFalse(BossRule61.HasGlyph("No glyph here."));
            Assert.IsFalse(BossRule61.HasGlyph("An empty {} token."));
            Assert.IsFalse(BossRule61.HasGlyph("An open {token"));
            Assert.IsFalse(BossRule61.HasGlyph(null));
        }

        [Test]
        public void TimeOnScreen_AtMostFiveSeconds_TheHoldTheTextSystems()
        {
            for (int w = 0; w <= 30; w++)
                Assert.That(BossRule61.OnScreenS(w), Is.LessThanOrEqualTo(UnityTuning.BossRule61MaxS + 1e-9), w + " words: on screen at most BossRule61MaxS");
            Assert.That(BossRule61.HoldS(5), Is.EqualTo(UnityTuning.Hud47LineHoldS + 5 * UnityTuning.Hud47LineHoldPerWord).Within(1e-9), "a 5-word line: 2.0 + 0.3 x 5");
            Assert.That(BossRule61.OnScreenS(8), Is.GreaterThan(4.0), "an 8-word line still stands 4 s or more");
            double hold = BossRule61.HoldS(7);
            Assert.AreEqual(0f, BossRule61.Alpha(-0.01, hold), "before it begins");
            Assert.That(BossRule61.Alpha(UnityTuning.BossRule61InS * 0.5, hold), Is.InRange(0.01f, 0.99f), "fading in");
            Assert.AreEqual(1f, BossRule61.Alpha(UnityTuning.BossRule61InS + hold * 0.5, hold), "the hold");
            Assert.That(BossRule61.Alpha(UnityTuning.BossRule61InS + hold + UnityTuning.BossRule61OutS * 0.5, hold), Is.InRange(0.01f, 0.99f), "fading out");
            Assert.AreEqual(0f, BossRule61.Alpha(BossRule61.OnScreenS(7) + 0.01, hold), "after it ends");
            Assert.That(BossRule61.Scale(0), Is.EqualTo(UnityTuning.BossRule61ScaleFrom).Within(1e-6f), "it begins a little large");
            Assert.That(BossRule61.Scale(UnityTuning.BossRule61InS), Is.EqualTo(1f).Within(1e-6f), "and settles to 1 as the fade-in ends");
        }
    }
}
