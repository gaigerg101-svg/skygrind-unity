// NIGHT47 builder F: hud47's pure rules (Hud47Math, UiHud47.cs) and copy47's line rules (Copy47, Copy47.cs). Pure: no scene, no sim.
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N47FeelTests
    {
        static float Dist(Color a, Color b) => Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);

        [Test]
        public void Oklab_RoundTrips()
        {
            foreach (uint hx in new uint[] { 0x35e05a, 0xff3326, 0x3fa0ff, 0x7a3dff, 0x19d3e6, 0xffffff, 0x000000 })
            {
                Color c = Look.Hex(hx);
                Assert.That(Dist(Hud47Math.FromOklab(Hud47Math.ToOklab(c)), c), Is.LessThan(0.01f), hx.ToString("x6"));
            }
        }

        [Test]
        public void HealthBar_HitsEachStop_AndStaysBetweenNeighbours()
        {
            float[] st = UnityTuning.Hud47HealthStops;
            for (int i = 0; i < st.Length; i++)
                Assert.That(Dist(Hud47Math.Health(st[i]), Look.Hex(UnityTuning.Hud47HealthHex[i])), Is.LessThan(0.01f), "stop " + st[i]);
            // full health is green (OKLab h ~146), empty red (~29); 50 % lies between lime and yellow in hue
            Assert.That(Hud47Math.HueDeg(Hud47Math.Health(1f)), Is.InRange(140f, 150f));
            Assert.That(Hud47Math.HueDeg(Hud47Math.Health(0f)), Is.InRange(25f, 35f));
            Assert.That(Hud47Math.HueDeg(Hud47Math.Health(0.5f)), Is.InRange(94f, 123f));
        }

        [Test]
        public void Overheal_StaysOutOfRivalsMagenta()
        {
            for (int i = 0; i <= 20; i++)
            {
                float h = Hud47Math.HueDeg(Hud47Math.Over(i / 20f));
                Assert.That(h, Is.InRange(245f, UnityTuning.Hud47OverHueMaxDeg), "over " + i / 20f);
                Assert.That(UnityTuning.Hud47RivalHueDeg - h, Is.GreaterThan(40f));
            }
            Assert.That(Hud47Math.HueDeg(Look.Hex(0xff2bd6)), Is.EqualTo(UnityTuning.Hud47RivalHueDeg).Within(1f), "RIVAL's measured hue");
        }

        [Test]
        public void Ghost_HoldsThenDrains()
        {
            double wait = 0;
            float g = Hud47Math.Ghost(-1f, 1f, 1f, ref wait, 0.016);
            Assert.That(g, Is.EqualTo(1f));
            g = Hud47Math.Ghost(g, 1f, 0.6f, ref wait, 0.016); // a hit: 1 -> 0.6
            Assert.That(g, Is.EqualTo(1f));
            for (int i = 0; i < 20; i++) g = Hud47Math.Ghost(g, 0.6f, 0.6f, ref wait, 0.016); // 0.32 s: still holding
            Assert.That(g, Is.EqualTo(1f));
            for (int i = 0; i < 120; i++) g = Hud47Math.Ghost(g, 0.6f, 0.6f, ref wait, 0.016); // then it drains to the fill
            Assert.That(g, Is.EqualTo(0.6f).Within(1e-4));
        }

        [Test]
        public void Copy_RoomTitlesLoseTheirNumbers()
        {
            Assert.That(Copy47.StripNumber("1.1 INTAKE"), Is.EqualTo("INTAKE"));
            Assert.That(Copy47.StripNumber("3.2b THE STACK"), Is.EqualTo("THE STACK"));
            Assert.That(Copy47.StripNumber("4.10 THE CANYON"), Is.EqualTo("THE CANYON"));
            Assert.That(Copy47.StripNumber("THE BRAWL"), Is.EqualTo("THE BRAWL"));
            Assert.That(Copy47.StripNumber("DOOR"), Is.EqualTo("DOOR"));
            Assert.That(Copy47.StripNumber("1.5"), Is.EqualTo("1.5"));
        }

        [Test]
        public void Zoom47_FactorGivesTheShare()
        {
            double z = Zoom47.FactorFor(74, 0.80);
            Assert.That(Skygrind.Core.CombatMath.ZoomedFov(74, z), Is.EqualTo(74 * 0.80).Within(1e-6));
            Assert.That(Zoom47.EaseOut(0), Is.EqualTo(0));
            Assert.That(Zoom47.EaseOut(1), Is.EqualTo(1));
            Assert.That(Zoom47.EaseOut(0.5), Is.EqualTo(0.875).Within(1e-9));
        }

        [Test]
        public void Counter_Words()
        {
            Assert.That(Hud47Math.CounterWord(0), Is.EqualTo(UnityTuning.Counter43ClearWord));
            Assert.That(Hud47Math.CounterWord(1), Is.EqualTo("LAST ONE"));
            Assert.That(Hud47Math.CounterWord(6), Is.EqualTo(UnityTuning.Counter43LeftWord));
        }
    }
}
