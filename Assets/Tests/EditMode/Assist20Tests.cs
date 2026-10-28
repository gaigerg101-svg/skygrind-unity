// NIGHT20 E2 (builder P; Research/2026-10-06-additions-backlog.md E2): ASSIST, Hades' God Mode (design.adds.assist): damage
// resistance that starts at 20 % and grows 2 % with each down, capped at 80 %; a run with it on is stamped (no records,
// like the LAB). Core/Assist20.cs is the rule: Resistance(downs) = min(cap, start + perDown x downs), the damage taken
// x (1 - resistance), the stamp's text.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Assist20Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root(), "config.json"));

        static string Root()
        {
            try { return Port.Root; }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        [Test]
        public void TheBlock_IsInTheDesignConfig_WithHadesNumbers()
        {
            Assert.That(Cfg.Has("design.adds.assist._why"), Is.True);
            Assert.That(Cfg.Design("adds.assist.enabled"), Is.True);
            Assist20.P p = Assist20.P.From(Cfg);
            Assert.That((p.enabled, p.start, p.perDown, p.cap), Is.EqualTo((true, 0.2, 0.02, 0.8)), "Hades' God Mode: 20 %, +2 % a death, 80 %");
        }

        [Test]
        public void Resistance_StartsAt20_Grows2PerDown_CapsAt80()
        {
            Assist20.P p = Assist20.P.From(Cfg);
            Assert.That(Assist20.Resistance(p, 0), Is.EqualTo(0.2).Within(1e-12));
            Assert.That(Assist20.Resistance(p, 1), Is.EqualTo(0.22).Within(1e-12));
            Assert.That(Assist20.Resistance(p, 10), Is.EqualTo(0.4).Within(1e-12));
            Assert.That(Assist20.Resistance(p, 30), Is.EqualTo(0.8).Within(1e-12), "the cap at 30 downs");
            Assert.That(Assist20.Resistance(p, 1000), Is.EqualTo(0.8).Within(1e-12));
            Assert.That(Assist20.Resistance(p, -3), Is.EqualTo(0.2).Within(1e-12), "never below the start");
            Assert.That(Assist20.TakenMult(p, 5), Is.EqualTo(0.7).Within(1e-12), "the damage taken x (1 - resistance)");
        }

        [Test]
        public void Off_NoResistance()
        {
            Assist20.P p = Assist20.P.From(Cfg);
            p.enabled = false;
            Assert.That(Assist20.Resistance(p, 12), Is.EqualTo(0));
            Assert.That(Assist20.TakenMult(p, 12), Is.EqualTo(1));
        }

        [Test]
        public void TheStamp_ReadsThePercent()
        {
            Assert.That(Assist20.Stamp(0.2), Is.EqualTo("ASSIST 20%"));
            Assert.That(Assist20.Stamp(0.24000000000000002), Is.EqualTo("ASSIST 24%"));
            Assert.That(Assist20.Stamp(0.8), Is.EqualTo("ASSIST 80%"));
        }
    }
}
