// NIGHT53 (the shared commit; Tools/night/n53-shared.cjs): the night's one row as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. design.n53.polish53 exists, has a _why, a bool enabled and a numbers object holding U's placeholders; off unless a freeze turned it on
//      (its enabled_why then starts with "ON"); no tape bit; ViewOn reads it; --off53 and --off52 turn it off
//   2. design.n53 holds no level row: the campaign order is the same with and without it (rooms are edited in place in the n52 levels)
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N53SharedTests
    {
        static readonly string[] Items = { "firstShot", "killWash", "scorchCore", "icelineGrade", "killPop" };

        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        [Test]
        public void ThePolishRow_Exists_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n53._why"), Is.True, "design.n53._why");
            const string p = "design.n53.polish53.";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, "polish53._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, "polish53.enabled is a bool");
            bool on = c.Design("n53.polish53.enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, "off until a freeze turns it on (its enabled_why then starts with ON)");
            Assert.That(c.Get(p + "numbers") is Dictionary<string, object>, Is.True, "polish53.numbers is an object");
            foreach (string k in Items) Assert.That(c.Get(p + "numbers." + k) is Dictionary<string, object>, Is.True, "polish53.numbers." + k + " (U's placeholder)");
            Assert.That(NightRows.From(c).IndexOf("polish53"), Is.EqualTo(-1), "polish53 has no tape bit (a view row)");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                Assert.That(NightRows.ViewOn(c, "polish53"), Is.EqualTo(on), "ViewOn reads design.n53");
                foreach (int from in new[] { 53, 52 })
                {
                    NightRows.ForceOffFrom = from;
                    Assert.That(NightRows.ViewOn(c, "polish53"), Is.False, "--off" + from + ": polish53 off");
                }
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void N53_HoldsNoLevelRow()
        {
            GameConfig c = Load();
            foreach (KeyValuePair<string, object> kv in c.Obj("design.n53"))
                if (kv.Value is Dictionary<string, object> row)
                    Assert.That(row.ContainsKey("levelFrom") || row.ContainsKey("levelTo"), Is.False, kv.Key + ": NIGHT53 edits the n52 levels in place (no n53 level ids)");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig without = Load();
                PortData.Obj(without.root["design"]).Remove("n53");
                Assert.That(Curriculum25.Order(c, N25Blocks.From(c)), Is.EqualTo(Curriculum25.Order(without, N25Blocks.From(without))), "the campaign order with design.n53 = without it");
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
