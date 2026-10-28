// NIGHT60 builder L (B9 attempt 2; Tools/night/n60-l-rows.cjs): design.n60.focus60 as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. focus60 is a VIEW row (no tape bit: the pause is the Game's, the sim never reads it) with a _why, a bool enabled, an enabled_why and a
//      numbers object, off unless a freeze turned it on (its enabled_why then starts with "ON"); the checker finds no problem
//   2. NightRows.ViewOn reads it; --off60 (and older) turns it off
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N60FocusRowTests
    {
        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        [Test]
        public void Focus60_ViewRow_OffByDefault_ViewOnReadsIt()
        {
            GameConfig c = Load();
            const string p = "design.n60.focus60.";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, "focus60._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, "focus60.enabled is a bool");
            Assert.That(c.Get(p + "numbers") is Dictionary<string, object>, Is.True, "focus60.numbers is an object");
            bool on = c.Design("n60.focus60.enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(ew, Is.Not.Null.And.Not.Empty, "focus60.enabled_why");
            Assert.That(!on || ew.StartsWith("ON", System.StringComparison.Ordinal), Is.True, "off until a freeze turns it on (its enabled_why then starts with ON)");
            Assert.That(NightRows.From(c).IndexOf("focus60"), Is.EqualTo(-1), "focus60 has no tape bit (a view row)");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                Assert.That(NightRows.ViewOn(c, "focus60"), Is.EqualTo(on), "ViewOn reads design.n60.focus60");
                foreach (int from in new[] { 60, 59 })
                {
                    NightRows.ForceOffFrom = from;
                    Assert.That(NightRows.ViewOn(c, "focus60"), Is.False, "--off" + from + ": focus60 off");
                }
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
