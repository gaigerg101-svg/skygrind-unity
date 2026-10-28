// NIGHT39 V: juice38's burst-or-tick rule (Juice39Hurt.cs; the lead's n39-r1 frame f0900: the full static refired on every 0.25 s floor tick).
// The streams are the sim's cadences (harness `n39views hurt` measured Lava30's real re-touch on n38-foundry: every 0.50 s). Pure.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Juice39HurtTests
    {
        static Juice39Hurt Stream(string source, double every, double seconds)
        {
            var r = new Juice39Hurt();
            for (double t = 0; t < seconds - 1e-9; t += every) r.Hit(t, source);
            return r;
        }

        [Test]
        public void AThreeSecondStandOnABurn_GivesAtMostOneBurst()
        {
            Assert.That(Stream("hazard", 0.25, 3).Bursts, Is.EqualTo(0), "a hazard floor tick (THE LANTERN, f0900): never the full burst");
            Assert.That(Stream("acid", 0.25, 3).Bursts, Is.EqualTo(0));
            Juice39Hurt lava = Stream("lava", 0.5, 3);
            Assert.That(lava.Bursts, Is.EqualTo(1), "Lava30's first touch is a hit; its 0.5 s re-touches are ticks");
            Assert.That(lava.Ticks, Is.EqualTo(5));
            Assert.That(Stream("boss", 0.5, 3).Bursts, Is.EqualTo(1), "THE WICK's hot wall: one burst, then ticks");
        }

        [Test]
        public void RealHits_KeepTheirBurst()
        {
            var r = new Juice39Hurt();
            Assert.That(r.Hit(0, "husk"), Is.True);
            Assert.That(r.Hit(2, "husk"), Is.True, "2 s apart: a new burst");
            Assert.That(r.Hit(4, "drone"), Is.True);
            Assert.That(r.Hit(4.4, "husk"), Is.False, "inside the last burst's cooldown: a tick (the burst is still on screen)");
            Assert.That(r.Hit(6, "husk"), Is.True);
        }

        [Test]
        public void Clear_ForgetsTheLastBurst()
        {
            var r = new Juice39Hurt();
            r.Hit(0, "lava");
            r.Clear();
            Assert.That(r.Hit(0.1, "lava"), Is.True, "a respawn: the next hit is a burst");
            Assert.That(Juice39Hurt.TickSource("hazard"), Is.True);
            Assert.That(Juice39Hurt.TickSource("husk"), Is.False);
            Assert.That(Juice39Hurt.TickSource(null), Is.False);
        }
    }
}
