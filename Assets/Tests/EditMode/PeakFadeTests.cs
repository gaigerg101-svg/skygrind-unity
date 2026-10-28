// NIGHT8 U2-08: PEAK FADE (design.director.peakFade, Unity-only). BUILD_UP -> PEAK -> PEAK_FADE (no spawns) -> RELAX:
// intensity holds while enemies are engaged, PEAK_FADE waits for intensity < fadeBelow with at most fadeEngaged engaged,
// or ends at fadeMaxTime. With the flag off the director runs the web's StepDirector (DirectorGoldenTests cover it).
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class PeakFadeTests
    {
        static GameConfig cfg;
        static DirectorMath.D d;
        static DirectorMath.Fade f;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            d = DirectorMath.From(cfg);
            f = DirectorMath.Fade.From(cfg);
        }

        static string Run(DirectorState s, double seconds, int engaged, ref string last)
        {
            for (int i = 0; i < MovementMath.StepsFor(seconds, cfg.Dt); i++)
            {
                string c = DirectorMath.StepDirectorFade(s, cfg.Dt, 0, 0, 0, () => 0.5, d, f, engaged);
                if (c != null) last = c;
            }
            return s.phase;
        }

        [Test]
        public void ThePeak_FadesUntilTheFightCalms_ThenRelaxes()
        {
            var s = new DirectorState { intensity = 1 };
            string last = null;
            Assert.That(DirectorMath.StepDirectorFade(s, cfg.Dt, 0, 0, 0, () => 0.5, d, f, 3), Is.EqualTo("PEAK"));
            double held = s.intensity;
            Run(s, d.peakTime + 0.05, 3, ref last);
            Assert.That(s.phase, Is.EqualTo("PEAK_FADE"), "PEAK ends in PEAK_FADE, not RELAX");
            Assert.That(s.intensity, Is.EqualTo(held), "no decay while enemies are engaged");
            Assert.That(DirectorMath.PhaseSpawnMult("PEAK_FADE", d), Is.EqualTo(0), "PEAK_FADE spawns nothing");
            Run(s, 2, 3, ref last);
            Assert.That(s.phase, Is.EqualTo("PEAK_FADE"), "still engaged: it waits");
            Run(s, 30, 0, ref last); // nobody engaged: intensity decays under fadeBelow
            Assert.That(last, Is.EqualTo("RELAX").Or.EqualTo("BUILD_UP"));
            Assert.That(s.phase, Is.Not.EqualTo("PEAK_FADE"));
        }

        [Test]
        public void PeakFade_EndsAtItsCap_EvenWhileEngaged()
        {
            var s = new DirectorState { intensity = 1 };
            string last = null;
            Run(s, d.peakTime + 0.1, 5, ref last);
            Assert.That(s.phase, Is.EqualTo("PEAK_FADE"));
            Run(s, f.fadeMaxTime - 0.2, 5, ref last);
            Assert.That(s.phase, Is.EqualTo("PEAK_FADE"));
            Run(s, 0.3, 5, ref last);
            Assert.That(s.phase, Is.EqualTo("RELAX"), "fadeMaxTime caps it");
            Assert.That(s.intensity, Is.LessThanOrEqualTo(0.6), "RELAX starts at most at 0.6 (directorMath enter)");
        }

        [Test]
        public void WithoutEnemiesEngaged_ItDecaysAsTheWebs()
        {
            var a = new DirectorState { intensity = 0.5 };
            var b = new DirectorState { intensity = 0.5 };
            for (int i = 0; i < 120; i++)
            {
                DirectorMath.StepDirectorFade(a, cfg.Dt, 0, 0, 0, () => 0.5, d, f, 0);
                DirectorMath.StepDirector(b, cfg.Dt, 0, 0, 0, () => 0.5, d);
            }
            Assert.That(a.intensity, Is.EqualTo(b.intensity));
            Assert.That(a.phase, Is.EqualTo(b.phase));
        }
    }
}
