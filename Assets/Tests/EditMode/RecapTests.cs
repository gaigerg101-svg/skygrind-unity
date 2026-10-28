// NIGHT8 U2-05: the death recap, the damage-direction angle and personal bests. Records: the web's tests/records.test.js
// (its Survival and readRecords cases; the other modes aren't ported). The recap and the arcs have no web tests; these
// check deathRecap.js / hitDirection.js's documented behaviour.
using System.Collections.Generic;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class RecapTests
    {
        static Records.Run R(double round, double points) => new Records.Run { round = round, points = points };

        [Test]
        public void Survival_RoundFirst_ThenPoints()
        {
            var a = Records.Judge(R(5, 100), null);
            Assert.That(a.newBest && a.first, Is.True);
            var b = Records.Judge(R(7, 100), R(5, 9000));
            Assert.That(b.newBest, Is.True);
            Assert.That(b.text, Is.EqualTo("NEW BEST · +2 ROUNDS"));
            Assert.That(Records.Judge(R(5, 900), R(5, 600)).text, Is.EqualTo("NEW BEST · ROUND 5, +300 POINTS"));
            var c = Records.Judge(R(3, 900), R(5, 600));
            Assert.That(c.newBest, Is.False);
            Assert.That(c.text, Is.EqualTo("BEST ROUND 5 · 2 SHORT"));
        }

        [Test]
        public void ARecordOfAnotherVersion_OrGarbage_IsDropped()
        {
            Records ok = Records.Read("{\"v\":1,\"modes\":{\"survival\":{\"round\":4,\"points\":1}}}");
            Assert.That(ok.Best("survival").round, Is.EqualTo(4));
            Assert.That(ok.Best("survival").points, Is.EqualTo(1));
            Assert.That(Records.Read("{\"v\":2,\"modes\":{\"survival\":{\"round\":4}}}").modes, Is.Empty);
            Assert.That(Records.Read("{not json").modes, Is.Empty);
            Assert.That(Records.Read(null).modes, Is.Empty);
            // The store round-trips through its JSON and keeps only better runs.
            var store = new Records();
            Assert.That(store.Submit("survival", R(4, 2000)).first, Is.True);
            Assert.That(store.Submit("survival", R(3, 9000)).newBest, Is.False);
            Records back = Records.Read(store.ToJson());
            Assert.That(back.Best("survival").round, Is.EqualTo(4));
            Assert.That(back.Best("survival").points, Is.EqualTo(2000));
        }

        [Test]
        public void TheRecap_NamesTheKiller_AndSumsTheLastWindowBySource()
        {
            var names = new Dictionary<string, string> { ["husk"] = "HUSK", ["swarmer"] = "SWARMER", ["fall"] = "THE FALL", ["unknown"] = "SOMETHING" };
            var recap = new DeathRecap(3);
            recap.Hurt(1.0, 40, DeathRecap.SourceName("husk", names)); // outside the window at t = 6
            recap.Hurt(4.0, 20, DeathRecap.SourceName("swarmer", names));
            recap.Hurt(5.0, 34.4, DeathRecap.SourceName("husk", names));
            recap.Hurt(5.5, 20, DeathRecap.SourceName("swarmer", names));
            recap.Hurt(6.0, 12, DeathRecap.SourceName("husk", names));
            DeathRecap.Summary s = recap.At(6.0);
            Assert.That(s.killer, Is.EqualTo("HUSK"));
            Assert.That(s.parts.Count, Is.EqualTo(2));
            Assert.That(s.parts[0].name, Is.EqualTo("HUSK"));
            Assert.That(s.parts[0].amount, Is.EqualTo(46)); // 34.4 + 12, rounded
            Assert.That(s.parts[1].amount, Is.EqualTo(40));
            Assert.That(s.total, Is.EqualTo(86));
            Assert.That(DeathRecap.SourceName(null, names), Is.EqualTo("SOMETHING"));
            Assert.That(DeathRecap.SourceName("charger", names), Is.EqualTo("CHARGER"));
        }

        [Test]
        public void HitAngle_AheadRightBehindLeft_AndThePool()
        {
            // yaw 0 looks toward -z: ahead is -z, right is +x.
            Assert.That(HitArcs.HitAngle(0, 0, 0, 0, -5), Is.EqualTo(0).Within(1e-9));
            Assert.That(HitArcs.HitAngle(0, 0, 0, 5, 0), Is.EqualTo(90).Within(1e-9));
            Assert.That(System.Math.Abs(HitArcs.HitAngle(0, 0, 0, 0, 5)), Is.EqualTo(180).Within(1e-9));
            Assert.That(HitArcs.HitAngle(0, 0, 0, -5, 0), Is.EqualTo(-90).Within(1e-9));
            var pool = new HitArcs(2);
            pool.Add(1, 1);
            pool.Add(1.2, 1); // the same side refreshes, no second arc
            int live = 0;
            foreach (var a in pool.arcs) if (a.age != double.PositiveInfinity) live++;
            Assert.That(live, Is.EqualTo(1));
            pool.Step(0.5, 0.7);
            pool.Step(0.3, 0.7);
            Assert.That(pool.arcs[0].age, Is.EqualTo(double.PositiveInfinity), "free after its life");
            Assert.That(HitArcs.Alpha(0.2, 0.35, 0.7), Is.EqualTo(1));
            Assert.That(HitArcs.Alpha(0.525, 0.35, 0.7), Is.EqualTo(0.5).Within(1e-9));
        }
    }
}
