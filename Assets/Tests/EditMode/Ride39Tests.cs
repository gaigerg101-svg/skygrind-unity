// NIGHT39 builder R1 (design.n39.ride39, Ride35.Params.With39 / Ride35.Boost39): the row's numbers, the override leaving wallride35's other rules and
// a neutral ride alone, and the jump-off cap that holds a chain at chainCap. The harness (n39ride chain) measures the chain wall by wall.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Ride39Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root(), "config.json"));

        static string Root()
        {
            try { return Port.Root; }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        [Test]
        public void TheRow_HasEveryNumber_WithItsWhy()
        {
            Assert.That(Cfg.Has("design.n39.ride39._why"), Is.True);
            Assert.That(Cfg.Get("design.n39.ride39.enabled"), Is.InstanceOf<bool>());
            foreach (string k in new[] { "speedBuff", "exitBoost", "chainCap", "capJumpOff", "upLow", "upHigh" })
            {
                Assert.That(Cfg.Has("design.n39.ride39." + k), Is.True, k);
                Assert.That(Cfg.Has("design.n39.ride39." + k + "_why"), Is.True, k + "_why");
            }
        }

        [Test]
        public void With39_OverridesOnlyItsFive_AndLeavesNeutralNeutral()
        {
            Ride35.Params b = Ride35.Params.From(Cfg, true), a = b.With39(Cfg);
            Assert.That(b.capJumpOff39, Is.False, "wallride35's own rules never cap the jump-off");
            Assert.That(a.speedBuff, Is.EqualTo(Cfg.Num("design.n39.ride39.speedBuff")));
            Assert.That(a.exitBoost, Is.EqualTo(Cfg.Num("design.n39.ride39.exitBoost")));
            Assert.That(a.chainCap, Is.EqualTo(Cfg.Num("design.n39.ride39.chainCap")));
            Assert.That(a.upLow, Is.EqualTo(Cfg.Num("design.n39.ride39.upLow")));
            Assert.That(a.upHigh, Is.EqualTo(Cfg.Num("design.n39.ride39.upHigh")));
            Assert.That(a.capJumpOff39, Is.EqualTo(Cfg.Design("n39.ride39.capJumpOff")));
            Assert.That(a.outSpeed, Is.EqualTo(b.outSpeed));
            Assert.That(a.safetySteps, Is.EqualTo(b.safetySteps));
            Assert.That(a.cosFollowMax, Is.EqualTo(b.cosFollowMax));
            Assert.That(Ride35.Params.Neutral.With39(Cfg).on, Is.False);
            Assert.That(Ride35.Params.Neutral.With39(Cfg).speedBuff, Is.EqualTo(0));
        }

        [Test]
        public void Boost39_HoldsTheChainAtTheCap_AndKeepsAFasterRide()
        {
            Ride35.Params p = Ride35.Params.From(Cfg, true).With39(Cfg);
            double H(Vec3 v) => JsMath.Hypot(v.x, v.z);
            // a slow jump-off: the exit boost added, under the cap (as Boost)
            Vec3 slow = new Vec3(8, 5, 6); // 10 m/s
            Assert.That(H(Ride35.Boost39(slow, p, 6)), Is.EqualTo(H(Ride35.Boost(slow, p))).Within(1e-9));
            // a jump-off whose out push lifts it past the cap: held at the cap, the direction and the up speed kept
            Vec3 fast = new Vec3(8, 5, 16); // along 16 + out 8: 17.9 m/s
            Vec3 c = Ride35.Boost39(fast, p, 16);
            Assert.That(H(c), Is.EqualTo(p.chainCap).Within(1e-9));
            Assert.That(c.y, Is.EqualTo(5));
            Assert.That(c.x / c.z, Is.EqualTo(fast.x / fast.z).Within(1e-12));
            // a ride already faster than the cap (a slide-hop entry) keeps its own speed, never gains from the push
            Vec3 faster = new Vec3(8, 5, 22);
            Assert.That(H(Ride35.Boost39(faster, p, 22)), Is.EqualTo(22).Within(1e-9));
        }
    }
}
