// NIGHT64 (the lead, A1; the shared commit, modelled on N63SharedTests.cs): the night's design block as data (docs/NIGHT-ROWS.md section 2),
// pinned before any builder patches a row. Pinned:
//   1. design.n64 exists with a _why, and the checker finds no problem in the design object
//   2. the three rows NIGHT64 names (Logs/n64/slate.json: goal64 C4, order64 U1; fightLine64 NIGHT64.md section 3 leftover 2) exist as VIEW
//      rows (no `sim`: no tape bit), each with a _why, an enabled_why and a bool enabled; whatever their design default, the night's off switch
//      (-skygrindOff64, ForceOffFrom 64) turns each one off
//   3. the off switch reads the smallest night given: -skygrindOff64 is 64, an older -skygrindOff63 covers 64, none is NoOff
// The same-run toggle's claim on the three rows is pinned in Game/N64SharedGameTests.cs (this assembly cannot see Skygrind.Game).
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N64SharedTests
    {
        static readonly string[] Rows = { "goal64", "order64", "fightLine64" };

        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        [Test]
        public void DesignN64_HasAWhy_AndTheCheckerFindsNoProblem()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n64._why") && c.Get("design.n64._why") is string w && w.Length > 0, Is.True, "design.n64._why");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
        }

        [Test]
        public void TheNightsThreeRows_AreViewRows_AndTheNightsOffSwitchTurnsThemOff()
        {
            GameConfig c = Load();
            int saved = NightRows.ForceOffFrom;
            try
            {
                foreach (string row in Rows)
                {
                    string at = "design.n64." + row;
                    Assert.That(c.Has(at + "._why") && c.Get(at + "._why") is string w && w.Length > 0, Is.True, at + "._why");
                    Assert.That(c.Has(at + ".enabled_why") && c.Get(at + ".enabled_why") is string ew && ew.Length > 0, Is.True, at + ".enabled_why");
                    Assert.That(c.Has(at + ".enabled") && c.Get(at + ".enabled") is bool, Is.True, at + ".enabled is a bool");
                    Assert.That(c.Has(at + ".sim"), Is.False, at + " is a view row (no sim, no tape bit)");
                    NightRows.ForceOffFrom = 64;
                    Assert.That(NightRows.ViewOn(c, row), Is.False, at + " under -skygrindOff64");
                    NightRows.ForceOffFrom = NightRows.NoOff;
                    Assert.That(NightRows.ViewOn(c, row), Is.EqualTo(c.Design("n64." + row + ".enabled")), at + " reads its design default");
                }
                Assert.That(NightRows.From(c).IndexOf("goal64"), Is.EqualTo(-1), "no NIGHT64 row is a sim row");
            }
            finally
            {
                NightRows.ForceOffFrom = saved;
            }
        }

        [Test]
        public void OffSwitch_ReadsTheSmallestNightGiven()
        {
            Assert.That(NightRows.OffFrom(new[] { "-skygrindOff64" }, "-skygrindOff"), Is.EqualTo(64));
            Assert.That(NightRows.OffFrom(new[] { "-skygrindOff64", "-skygrindOff63" }, "-skygrindOff"), Is.EqualTo(63), "an older switch covers this night");
            Assert.That(NightRows.OffFrom(new string[0], "-skygrindOff"), Is.EqualTo(NightRows.NoOff));
        }
    }
}
