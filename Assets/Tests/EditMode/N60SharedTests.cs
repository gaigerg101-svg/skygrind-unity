// NIGHT60 builder V (B8, the exit lamp; Tools/night/n60-v-rows.cjs): the night's first row as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. design.n60 exists with a _why; lamp60 is a VIEW row (no tape bit) with a _why, a bool enabled, an enabled_why and a numbers object, off
//      unless a freeze turned it on (its enabled_why then starts with "ON"); the checker finds no problem
//   2. NightRows.ViewOn reads design.n60.lamp60; --off60 (and older) turns it off
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N60SharedTests
    {
        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        [Test]
        public void Lamp60_ViewRow_OffByDefault_ViewOnReadsIt()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n60._why") && c.Get("design.n60._why") is string nw && nw.Length > 0, Is.True, "design.n60._why");
            const string p = "design.n60.lamp60.";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, "lamp60._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, "lamp60.enabled is a bool");
            Assert.That(c.Get(p + "numbers") is Dictionary<string, object>, Is.True, "lamp60.numbers is an object");
            bool on = c.Design("n60.lamp60.enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(ew, Is.Not.Null.And.Not.Empty, "lamp60.enabled_why");
            Assert.That(!on || ew.StartsWith("ON", System.StringComparison.Ordinal), Is.True, "off until a freeze turns it on (its enabled_why then starts with ON)");
            Assert.That(NightRows.From(c).IndexOf("lamp60"), Is.EqualTo(-1), "lamp60 has no tape bit (a view row)");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                Assert.That(NightRows.ViewOn(c, "lamp60"), Is.EqualTo(on), "ViewOn reads design.n60.lamp60");
                foreach (int from in new[] { 60, 59 })
                {
                    NightRows.ForceOffFrom = from;
                    Assert.That(NightRows.ViewOn(c, "lamp60"), Is.False, "--off" + from + ": lamp60 off");
                }
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
