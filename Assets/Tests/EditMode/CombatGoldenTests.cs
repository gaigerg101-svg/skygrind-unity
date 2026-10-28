// Golden vectors for the combat math (PortExport/vectors/combatMath.json, gunMath.json sweepTargets): every recorded
// call of the web game's pure functions, replayed through CombatMath; numbers compared bit for bit. The combatMath
// evaluation is Skygrind.Core's GoldenCombat (NIGHT9 U3-04), the code the Player's -skygrindGolden runs too, one part per
// test here. pickRicochetTarget's two cases inject a callback (README: "hasLOS = always true" and its text), run with
// that rule.
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Skygrind.Core.PortData;

namespace Skygrind.Core.Tests
{
    public class CombatGoldenTests
    {
        static IEnumerable<Dictionary<string, object>> Cases(string module, string fn) =>
            Arr(Obj(Port.Json($"vectors/{module}.json"))["cases"]).Select(Obj).Where(d => d.TryGetValue("fn", out object f) && (string)f == fn);

        static Vec3 V(object o) => V3(o);

        static void Same(double expected, double actual, string what) =>
            Assert.That(BitConverter.DoubleToInt64Bits(actual), Is.EqualTo(BitConverter.DoubleToInt64Bits(expected)), $"{what}: {actual:R} vs web {expected:R}");

        static List<BodyTarget> Targets(object o) => Arr(o).Select(t =>
        {
            var d = Obj(t);
            return new BodyTarget { enemy = d["id"], basePos = V(d["base"]), height = Num(d["height"]), radius = Num(d["radius"]), windupSteps = d.TryGetValue("windupSteps", out object w) ? (int)Num(w) : 0 };
        }).ToList();

        [Test]
        public void SampleCone()
        {
            GoldenResult r = PortGolden.Check("combatMath sampleCone", GoldenCombat.SampleCone);
            Assert.That(r.cases, Is.EqualTo(20));
        }

        [Test]
        public void RaySphere_AndRayVerticalCapsule() => PortGolden.Check("combatMath raySphere + rayVerticalCapsule", GoldenCombat.RaySphereAndCapsule);

        [Test]
        public void LeapVelocity() => PortGolden.Check("combatMath leapVelocity", GoldenCombat.LeapVelocity);

        [Test]
        public void SwapTimeline() => PortGolden.Check("combatMath swapLowered + swapCanFire + swapMirror", GoldenCombat.SwapTimeline);

        [Test]
        public void PunchPick_AndKind() => PortGolden.Check("combatMath pickPunchTarget + punchKind", GoldenCombat.PunchPickAndKind);

        [Test]
        public void RicochetPick_WithTheRecordedRule() => PortGolden.Check("combatMath pickRicochetTarget", GoldenCombat.RicochetPick);

        [Test]
        public void SweepTargets()
        {
            int n = 0;
            foreach (var c in Cases("gunMath", "sweepTargets"))
            {
                var a = Arr(c["args"]);
                var got = CombatMath.SweepTargets(V(a[0]), V(a[1]), Targets(a[2]), Num(a[3]), Num(a[4]));
                var want = Arr(c["result"]).Select(Obj).ToList();
                Assert.That(got.Count, Is.EqualTo(want.Count), "sweepTargets count");
                for (int i = 0; i < got.Count; i++)
                {
                    Assert.That(got[i].target.enemy, Is.EqualTo(Obj(want[i]["target"])["id"]));
                    Same(Num(want[i]["distance"]), got[i].distance, "sweepTargets.distance");
                }
                n++;
            }
            Assert.That(n, Is.EqualTo(3));
        }
    }
}
