// NIGHT16 polish #46-#49 (builder U): the award feed's lines (hud.js :450-460, :487), the rank bar's pulse (hud.js
// :179-182, :602; styleMath rankPulseRate), R from PAUSED (menus.js :786-790), RESULTS' RETRY (R) label (menus.js :1088).
// Pure rules only: FeedLine / RankPulse / RetryLabel (UiFeelHud.cs), RestartRule (SurvivalGame.Flow.cs).
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class SmallUiTests
    {
        // --- #46 the feed ----------------------------------------------------------------------------------------------

        [Test]
        public void AScoringAwardReadsPlusNameThenPointsAndANothingAwardJustItsName()
        {
            // hud.js :487: e.points > 0 ? `+ ${e.name} ${e.points}` : e.name
            Assert.AreEqual("+ HEADSHOT 30", FeedLine.Body("HEADSHOT", 30));
            Assert.AreEqual("WALL JUMP", FeedLine.Body("WALL JUMP", 0));
            Assert.AreEqual("WALL JUMP", FeedLine.Body("WALL JUMP", -1));
        }

        [Test]
        public void ANothingAwardIsDimmedAndEveryLineFadesOverItsTime()
        {
            // hud.js :487: zero = e.points <= 0 (.hud-feed div.zero opacity 0.5, :216); the line's alpha 1 - age / feedFadeTime
            Assert.IsTrue(FeedLine.Dim(0));
            Assert.IsFalse(FeedLine.Dim(5));
            Assert.AreEqual(1, FeedLine.Alpha(0, 2, false), 1e-12);
            Assert.AreEqual(0.5, FeedLine.Alpha(0, 2, true), 1e-12);
            Assert.AreEqual(0.25, FeedLine.Alpha(1, 2, true), 1e-12);
            Assert.AreEqual(0, FeedLine.Alpha(3, 2, false), 1e-12);
        }

        // --- #47 the rank bar's pulse ------------------------------------------------------------------------------------

        [Test]
        public void TheRankBarBreathesFrom075To145AtTheRanksRate()
        {
            // hud.js :179-182: brightness 0.75 -> 1.45, ease-in-out, alternate, each way 0.5 / rankPulseRate s (:602)
            double rate = 2; // a way lasts 0.25 s
            Assert.AreEqual(0.75, RankPulse.Brightness(0, rate), 1e-9);
            Assert.AreEqual(1.10, RankPulse.Brightness(0.125, rate), 1e-6, "ease-in-out is half way at half time");
            Assert.AreEqual(1.45, RankPulse.Brightness(0.25, rate), 1e-9);
            Assert.AreEqual(1.10, RankPulse.Brightness(0.375, rate), 1e-6, "alternate: back down");
            Assert.AreEqual(0.75, RankPulse.Brightness(0.5, rate), 1e-9);
            Assert.That(RankPulse.Brightness(0.025, rate), Is.LessThan(0.75 + 0.7 * 0.1), "eased: slow out of the end");
            Assert.AreEqual(1, RankPulse.Brightness(0.1, 0), 1e-12, "no rate: steady");
        }

        [Test]
        public void TheClockWrapsSoItNeverGrows()
        {
            double rate = 0.8; // a whole cycle is 2 x 0.5 / 0.8 = 1.25 s
            Assert.AreEqual(0.25, RankPulse.Wrap(1.5, rate), 1e-9);
            Assert.AreEqual(RankPulse.Brightness(0.25, rate), RankPulse.Brightness(1.5, rate), 1e-9);
        }

        // --- #48 R from PAUSED ---------------------------------------------------------------------------------------------

        [Test]
        public void RRestartsFromDownResultsAndAPlainPause()
        {
            Assert.AreEqual(RestartRule.Kind.EndAndStart, RestartRule.For(Flow.Down, false, false));
            Assert.AreEqual(RestartRule.Kind.Start, RestartRule.For(Flow.Results, false, false));
            Assert.AreEqual(RestartRule.Kind.FromPause, RestartRule.For(Flow.Paused, false, false), "menus.js :786: from pause too");
            Assert.AreEqual(RestartRule.Kind.None, RestartRule.For(Flow.Paused, true, false), "the MODS draft: pick first");
            Assert.AreEqual(RestartRule.Kind.None, RestartRule.For(Flow.Paused, false, true), "SETTINGS / CONTROLS over PAUSED: not pause");
            Assert.AreEqual(RestartRule.Kind.None, RestartRule.For(Flow.Playing, false, false));
            Assert.AreEqual(RestartRule.Kind.None, RestartRule.For(Flow.Title, false, false));
        }

        // --- #49 RESULTS' RETRY (R) ------------------------------------------------------------------------------------

        [Test]
        public void TheRetryButtonNamesTheKeyOnlyForKeyboardPlayers()
        {
            Assert.AreEqual("RETRY (R)", RetryLabel.For(false, "R"));
            Assert.AreEqual("RETRY (F5)", RetryLabel.For(false, "F5"), "the live binding");
            Assert.AreEqual("RETRY", RetryLabel.For(true, "R"), "a pad has no restart button: no R");
            Assert.AreEqual("RETRY", RetryLabel.For(false, null), "unbound");
        }
    }
}
