// NIGHT61 (the lead, A1; the shared commit, modelled on N60SharedTests.cs): the night's design block as data (docs/NIGHT-ROWS.md section 2),
// pinned before any row is added. Pinned:
//   1. design.n61 exists with a _why, and the checker finds no problem in the design object
//   2. the off switch reads the smallest night given: -skygrindOff61 is 61, an older -skygrindOff60 covers 61, none is NoOff
// Written in a session with no Editor answer (NIGHT61-LOG.md): compiled by dn-check, not run; the Editor run is owed.
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N61SharedTests
    {
        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        [Test]
        public void DesignN61_HasAWhy_AndTheCheckerFindsNoProblem()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n61._why") && c.Get("design.n61._why") is string w && w.Length > 0, Is.True, "design.n61._why");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
        }

        [Test]
        public void OffSwitch_ReadsTheSmallestNightGiven()
        {
            Assert.That(NightRows.OffFrom(new[] { "-skygrindOff61" }, "-skygrindOff"), Is.EqualTo(61));
            Assert.That(NightRows.OffFrom(new[] { "-skygrindOff61", "-skygrindOff60" }, "-skygrindOff"), Is.EqualTo(60), "an older switch covers this night");
            Assert.That(NightRows.OffFrom(new string[0], "-skygrindOff"), Is.EqualTo(NightRows.NoOff));
        }
    }
}
