// NIGHT25 (builder F): the BIG PICK's head names the round of the pick that is open. The N24 Player reel printed the head after the
// round-10 pick had opened, so the round-5 pick read "ROUND 10" (the reel's read, not the screen); the head is now one Core function
// and Mods.offerRound is the open pick's own round.
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class BigPickHeadTests
    {
        [Test]
        public void Head_NamesTheRound_AndTheLayerEnd()
        {
            Assert.That(BigPick24.Head(5), Is.EqualTo("BIG PICK  ·  ROUND 5"));
            Assert.That(BigPick24.Head(10), Is.EqualTo("BIG PICK  ·  ROUND 10"));
            Assert.That(BigPick24.Head(0), Is.EqualTo("BIG PICK  ·  LAYER CLEARED"));
        }

        [Test]
        public void EachPick_OpensForItsOwnRound_NotTheLatestOne()
        {
            var cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            var events = new EventBus();
            var mods = new Mods(cfg, events);
            mods.big = new BigPick24(cfg, events);
            mods.big.WantsOn = true;
            mods.Start(7);
            if (!mods.big.Active) Assert.Ignore("design.n24.bigPick is off in this config");
            events.Emit("roundClear", new RoundEvent { number = 5 });
            Assert.That(mods.Pending, Is.True);
            mods.Open();
            Assert.That(mods.offerRound, Is.EqualTo(5));
            Assert.That(BigPick24.Head(mods.offerRound), Does.EndWith("ROUND 5"));
            // (no Pick: with no sim hooks wired the cards may be unavailable; a fresh start stands for the first pick being over)
            mods.Start(7);
            events.Emit("roundClear", new RoundEvent { number = 10 });
            Assert.That(mods.Pending, Is.True, "the round-10 clear queued its own pick");
            mods.Open();
            Assert.That(mods.offerRound, Is.EqualTo(10));
            Assert.That(BigPick24.Head(mods.offerRound), Does.EndWith("ROUND 10"));
        }
    }
}
