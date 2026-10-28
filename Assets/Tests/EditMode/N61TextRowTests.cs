// NIGHT61 builder G (C1, one line of text at a time; Tools/night/n61-g-rows.cjs text61): design.n61.text61 as data (docs/NIGHT-ROWS.md
// section 2). Pinned:
//   1. text61 is a VIEW row (no tape bit: when a line shows is the HUD's, the sim never reads it) with a _why, a bool enabled, an enabled_why
//      and a numbers object, off unless a freeze turned it on (its enabled_why then starts with "ON"); the checker finds no problem
//   2. NightRows.ViewOn reads it; --off61 (and older) turns it off; a later night's switch does not
// Written test-first: RED on its own commit (the row is not in the config yet). Not run by its author (no Editor in the worktree).
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N61TextRowTests
    {
        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        [Test]
        public void Text61_ViewRow_OffByDefault_ViewOnReadsIt()
        {
            GameConfig c = Load();
            const string p = "design.n61.text61.";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, "text61._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, "text61.enabled is a bool");
            Assert.That(c.Get(p + "numbers") is Dictionary<string, object>, Is.True, "text61.numbers is an object");
            bool on = c.Design("n61.text61.enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(ew, Is.Not.Null.And.Not.Empty, "text61.enabled_why");
            Assert.That(!on || ew.StartsWith("ON", System.StringComparison.Ordinal), Is.True, "off until a freeze turns it on (its enabled_why then starts with ON)");
            Assert.That(NightRows.From(c).IndexOf("text61"), Is.EqualTo(-1), "text61 has no tape bit (a view row)");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                Assert.That(NightRows.ViewOn(c, "text61"), Is.EqualTo(on), "ViewOn reads design.n61.text61");
                foreach (int from in new[] { 61, 60, 47 })
                {
                    NightRows.ForceOffFrom = from;
                    Assert.That(NightRows.ViewOn(c, "text61"), Is.False, "--off" + from + ": text61 off");
                }
                NightRows.ForceOffFrom = 62;
                Assert.That(NightRows.ViewOn(c, "text61"), Is.EqualTo(on), "--off62 (a later night) leaves it as the config says");
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
