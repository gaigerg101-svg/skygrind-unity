// NIGHT63 (the lead, A1; the shared commit, modelled on N61SharedTests.cs): the night's design block as data (docs/NIGHT-ROWS.md section 2),
// pinned before any builder patches a row. Pinned:
//   1. design.n63 exists with a _why, and the checker finds no problem in the design object
//   2. the four rows the slate names (Logs/n63/slate.json: bossMotion63 B1, bossTell63 B2, bossColours63 B3, colours63 C1) exist as VIEW rows
//      (no `sim`: no tape bit), each with a _why, an enabled_why and a bool enabled; whatever their design default, the night's off switch
//      (-skygrindOff63, ForceOffFrom 63) turns each one off
//   3. the off switch reads the smallest night given: -skygrindOff63 is 63, an older -skygrindOff62 covers 63, none is NoOff
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N63SharedTests
    {
        static readonly string[] Rows = { "bossMotion63", "bossTell63", "bossColours63", "colours63" };

        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        [Test]
        public void DesignN63_HasAWhy_AndTheCheckerFindsNoProblem()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n63._why") && c.Get("design.n63._why") is string w && w.Length > 0, Is.True, "design.n63._why");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
        }

        [Test]
        public void TheSlatesFourRows_AreViewRows_AndTheNightsOffSwitchTurnsThemOff()
        {
            GameConfig c = Load();
            int saved = NightRows.ForceOffFrom;
            try
            {
                foreach (string row in Rows)
                {
                    string at = "design.n63." + row;
                    Assert.That(c.Has(at + "._why") && c.Get(at + "._why") is string w && w.Length > 0, Is.True, at + "._why");
                    Assert.That(c.Has(at + ".enabled_why") && c.Get(at + ".enabled_why") is string ew && ew.Length > 0, Is.True, at + ".enabled_why");
                    Assert.That(c.Has(at + ".enabled") && c.Get(at + ".enabled") is bool, Is.True, at + ".enabled is a bool");
                    Assert.That(c.Has(at + ".sim"), Is.False, at + " is a view row (no sim, no tape bit)");
                    NightRows.ForceOffFrom = 63;
                    Assert.That(NightRows.ViewOn(c, row), Is.False, at + " under -skygrindOff63");
                    NightRows.ForceOffFrom = NightRows.NoOff;
                    Assert.That(NightRows.ViewOn(c, row), Is.EqualTo(c.Design("n63." + row + ".enabled")), at + " reads its design default");
                }
                Assert.That(NightRows.From(c).IndexOf("bossMotion63"), Is.EqualTo(-1), "no NIGHT63 row is a sim row");
            }
            finally
            {
                NightRows.ForceOffFrom = saved;
            }
        }

        [Test]
        public void OffSwitch_ReadsTheSmallestNightGiven()
        {
            Assert.That(NightRows.OffFrom(new[] { "-skygrindOff63" }, "-skygrindOff"), Is.EqualTo(63));
            Assert.That(NightRows.OffFrom(new[] { "-skygrindOff63", "-skygrindOff62" }, "-skygrindOff"), Is.EqualTo(62), "an older switch covers this night");
            Assert.That(NightRows.OffFrom(new string[0], "-skygrindOff"), Is.EqualTo(NightRows.NoOff));
        }
    }
}
