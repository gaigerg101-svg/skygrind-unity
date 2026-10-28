// NIGHT42 builder U1 (design.n42.health42): the damage chunk and the number's count (Health42Math, UiHud33.Health42.cs). Pure: no scene, no sim.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Health42Tests
    {
        const double Dt = 1.0 / 60;

        [Test]
        public void Chunk_HoldsThenDrains_NeverUnderTheHealth()
        {
            double hold = 0;
            float chunk = Health42Math.Chunk(1f, 1f, 0.7f, ref hold, Dt); // a 30 % hit
            Assert.That(chunk, Is.EqualTo(1f), "the chunk keeps the old health");
            Assert.That(hold, Is.EqualTo(UnityTuning.Health42ChunkHold));
            float last = 0.7f;
            double t = 0;
            while (t < UnityTuning.Health42ChunkHold - Dt) { chunk = Health42Math.Chunk(chunk, last, last, ref hold, Dt); t += Dt; }
            Assert.That(chunk, Is.EqualTo(1f), "held through the hold");
            for (int i = 0; i < 120; i++) chunk = Health42Math.Chunk(chunk, last, last, ref hold, Dt);
            Assert.That(chunk, Is.EqualTo(0.7f), "drained to the health, not under it");
            // a second hit during a drain keeps the higher chunk
            hold = 0;
            chunk = Health42Math.Chunk(0.9f, 0.7f, 0.5f, ref hold, Dt);
            Assert.That(chunk, Is.EqualTo(0.9f));
        }

        [Test]
        public void Ticks_AsHud33()
        {
            Assert.That(Health42Math.Ticks(1f, 10), Is.EqualTo(10));
            Assert.That(Health42Math.Ticks(0.71f, 10), Is.EqualTo(8));
            Assert.That(Health42Math.Ticks(0.7f, 10), Is.EqualTo(7));
            Assert.That(Health42Math.Ticks(0f, 10), Is.EqualTo(0));
        }

        [Test]
        public void Number_CountsAndLands()
        {
            double v = Health42Math.EaseNumber(double.NaN, 100, Dt);
            Assert.That(v, Is.EqualTo(100));
            v = Health42Math.EaseNumber(100, 70, Dt);
            Assert.That(v, Is.LessThan(100).And.GreaterThan(70), "it counts down, not a snap");
            for (int i = 0; i < 60; i++) v = Health42Math.EaseNumber(v, 70, Dt);
            Assert.That(v, Is.EqualTo(70), "it lands within a second");
        }
    }
}
