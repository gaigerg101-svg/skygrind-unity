// NIGHT61 builder V (slate S4; Tools/night/n61-v4-rows.cjs): design.n61.fixture61 as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. fixture61 is a VIEW row (no tape bit: the mounts are drawn, the sim never reads them) with a _why, a bool enabled, an enabled_why and a
//      numbers object, off unless a freeze turned it on (its enabled_why then starts with "ON"); the checker finds no problem
//   2. NightRows.ViewOn reads it; --off61 (and older) turns it off
// Written test first (NIGHT61.md section 4): RED on its commit (the row does not exist yet); the Editor run is the lead's.
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N61FixtureRowTests
    {
        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        [Test]
        public void Fixture61_ViewRow_OffByDefault_ViewOnReadsIt()
        {
            GameConfig c = Load();
            const string p = "design.n61.fixture61.";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, "fixture61._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, "fixture61.enabled is a bool");
            Assert.That(c.Has(p + "numbers") && c.Get(p + "numbers") is Dictionary<string, object>, Is.True, "fixture61.numbers is an object");
            bool on = c.Design("n61.fixture61.enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(ew, Is.Not.Null.And.Not.Empty, "fixture61.enabled_why");
            Assert.That(!on || ew.StartsWith("ON", System.StringComparison.Ordinal), Is.True, "off until a freeze turns it on (its enabled_why then starts with ON)");
            Assert.That(NightRows.From(c).IndexOf("fixture61"), Is.EqualTo(-1), "fixture61 has no tape bit (a view row)");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                Assert.That(NightRows.ViewOn(c, "fixture61"), Is.EqualTo(on), "ViewOn reads design.n61.fixture61");
                foreach (int from in new[] { 61, 60 })
                {
                    NightRows.ForceOffFrom = from;
                    Assert.That(NightRows.ViewOn(c, "fixture61"), Is.False, "--off" + from + ": fixture61 off");
                }
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
