// NIGHT17 round 5 (builder X): the kill freeze grows with the step's kills (ImpactRules.KillFreezeSeconds; design.impact.freeze
// killPerExtra / killCap): one THUNDERCLAP blast killing 5 husks froze 40-57 ms, less than one headshot (n17-r4 horde).
// A step with N kills: freeze.kill + killPerExtra x (N - 1), capped at killCap; the longest request still wins.
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class ImpactCountTests
    {
        static GameConfig cfg;
        const double Frame = 1.0 / 120;
        const double Eps = 1e-9;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Root(), "config.json"));

        /// <summary>Port.Root in the Editor; the working folder's PortExport under a plain .NET runner (no Application.dataPath).</summary>
        static string Root()
        {
            try { return Port.Root; }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        [Test]
        public void TheKillFreezeGrowsWithTheStepsKillsUpToItsCap()
        {
            Assert.That(ImpactRules.KillFreezeSeconds(0.05, 1, 0.025, 0.16), Is.EqualTo(0.05).Within(Eps), "one kill: the kill tier");
            Assert.That(ImpactRules.KillFreezeSeconds(0.05, 2, 0.025, 0.16), Is.EqualTo(0.075).Within(Eps));
            Assert.That(ImpactRules.KillFreezeSeconds(0.05, 5, 0.025, 0.16), Is.EqualTo(0.15).Within(Eps), "a 5-kill blast: 150 ms");
            Assert.That(ImpactRules.KillFreezeSeconds(0.05, 6, 0.025, 0.16), Is.EqualTo(0.16).Within(Eps), "capped");
            Assert.That(ImpactRules.KillFreezeSeconds(0.05, 20, 0.025, 0.16), Is.EqualTo(0.16).Within(Eps));
            Assert.That(ImpactRules.KillFreezeSeconds(0.05, 0, 0.025, 0.16), Is.EqualTo(0.05).Within(Eps), "no count: the base");
            Assert.That(ImpactRules.KillFreezeSeconds(0.05, 5, 0, 0), Is.EqualTo(0.05).Within(Eps), "no keys: the old rule");
            Assert.That(ImpactRules.KillFreezeSeconds(0.2, 3, 0.025, 0.16), Is.EqualTo(0.2).Within(Eps), "never under the base");
        }

        [Test]
        public void AFiveKillStepFreezesLongerThanAHeadshotKill()
        {
            var r = new ImpactRules(cfg);
            for (int i = 0; i < 1200; i++) r.BeginFrame(Frame, false); // past every cooldown and gap
            for (int k = 0; k < 5; k++)
            {
                r.OnHit(new HitEvent { owner = "player", weapon = "shotgun", kind = "husk", killed = true, shot = 1, damage = 10, distance = 6 });
                r.OnKill(new KillEvent { owner = "player", weapon = "shotgun", kind = "husk" });
            }
            Assert.That(r.EndStep(false), Is.True, "a freeze");
            double perExtra = cfg.Num("design.impact.freeze.killPerExtra"), cap = cfg.Num("design.impact.freeze.killCap");
            double want = ImpactRules.KillFreezeSeconds(cfg.Num("design.impact.freeze.kill"), 5, perExtra, cap);
            Assert.That(r.grantedSeconds, Is.EqualTo(want).Within(Eps), "50 ms + 25 ms x 4");
            Assert.That(r.grantedSeconds, Is.GreaterThan(cfg.Num("design.impact.freeze.headshotKill")), "more than one headshot kill");
        }
    }
}
