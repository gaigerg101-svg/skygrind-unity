// NIGHT25 (builder F): the door cue waits for a banner, but never for ever (the Player's `cue` run read '' for the whole clear), and the overlap check is scoped to the campaign's own
// elements (the old HUD's banner-over-round-mark pair is not its business).
using System.Collections.Generic;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class CueWait25Tests
    {
        const double Max = UnityTuning.Cue25MaxWait;

        [Test]
        public void ABannerHoldsTheCueBack_ThenTheCueShows()
        {
            Assert.That(CueWait25.MayShow(false, 2.5, 0, 0.0, Max), Is.False, "a fresh banner: wait");
            Assert.That(CueWait25.MayShow(false, 0, 1.0, 0.5, Max), Is.False, "the room banner too");
            Assert.That(CueWait25.MayShow(false, 0, 0, 0.0, Max), Is.True, "no banner: show");
        }

        [Test]
        public void AStuckBannerTimer_NeverHidesTheCueForever()
        {
            double waited = 0;
            bool shown = false;
            for (int f = 0; f < 600 && !shown; f++) // a banner timer stuck at 99 s
            {
                shown = CueWait25.MayShow(false, 99, 99, waited, Max);
                if (!shown) waited += 1.0 / 60;
            }
            Assert.That(shown, Is.True);
            Assert.That(waited, Is.LessThanOrEqualTo(Max + 0.05));
        }

        [Test]
        public void ACueAlreadyShowing_StaysWhenABannerComes()
        {
            Assert.That(CueWait25.MayShow(true, 3, 3, 0, Max), Is.True);
        }

        [Test]
        public void TheOverlapCheck_IsScopedToTheCampaignsOwnElements()
        {
            HudOverlap25.Item Item(string path, double x) => new HudOverlap25.Item { path = path, text = "T", x = x, y = 100, w = 200, h = 40, alpha = 1 };
            var old = new List<HudOverlap25.Item> { Item("UI/HUD values/Banner", 100), Item("UI/HUD values/Round mark/Numeral", 120) };
            Assert.That(HudOverlap25.Overlaps(old), Is.Empty, "the old HUD's own pair is ignored");
            var own = new List<HudOverlap25.Item> { Item("UI/HUD values/Banner", 100), Item("UI/HUD values/Door cue 24/Way", 120) };
            Assert.That(HudOverlap25.Overlaps(own).Count, Is.EqualTo(1), "the cue on the banner is found");
            var lesson = new List<HudOverlap25.Item> { Item("UI/Meta overlay/Lesson25/Text", 100), Item("UI/HUD values/Weapon block/Name", 120) };
            Assert.That(HudOverlap25.Overlaps(lesson).Count, Is.EqualTo(1), "the lesson line on the weapon block is found");
        }
    }
}
