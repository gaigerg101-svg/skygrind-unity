// NIGHT54 (the shared commit; Tools/night/n54-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. design.n54's four rows exist, each with a _why, a bool enabled and a numbers object; off unless a freeze turned it on (its
//      enabled_why then starts with "ON"); wick54 is a sim row with a tape bit, the view rows have none; ViewOn reads the view rows;
//      --off54 and --off53 turn every one off
//   2. design.n54 holds no level row: the campaign order is the same with and without it
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N54SharedTests
    {
        static readonly string[] Views = { "light54", "grade54", "move54", "decor54" };

        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        static bool CheckRow(GameConfig c, string row)
        {
            string p = "design.n54." + row + ".";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
            bool on = c.Design("n54." + row + ".enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            Assert.That(c.Get(p + "numbers") is Dictionary<string, object>, Is.True, row + ".numbers is an object");
            return on;
        }

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n54._why"), Is.True, "design.n54._why");
            CheckRow(c, "wick54");
            Assert.That(NightRows.From(c).IndexOf("wick54"), Is.GreaterThanOrEqualTo(0), "wick54 is a sim row (a tape bit)");
            var on = new Dictionary<string, bool>();
            foreach (string v in Views)
            {
                on[v] = CheckRow(c, v);
                Assert.That(NightRows.From(c).IndexOf(v), Is.EqualTo(-1), v + " has no tape bit (a view row)");
            }
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                foreach (string v in Views) Assert.That(NightRows.ViewOn(c, v), Is.EqualTo(on[v]), "ViewOn reads design.n54." + v);
                foreach (int from in new[] { 54, 53 })
                {
                    NightRows.ForceOffFrom = from;
                    foreach (string v in Views) Assert.That(NightRows.ViewOn(c, v), Is.False, "--off" + from + ": " + v + " off");
                }
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void N54_HoldsNoLevelRow()
        {
            GameConfig c = Load();
            foreach (KeyValuePair<string, object> kv in c.Obj("design.n54"))
                if (kv.Value is Dictionary<string, object> row)
                    Assert.That(row.ContainsKey("levelFrom") || row.ContainsKey("levelTo"), Is.False, kv.Key + ": NIGHT54 changes no level file");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig without = Load();
                PortData.Obj(without.root["design"]).Remove("n54");
                Assert.That(Curriculum25.Order(c, N25Blocks.From(c)), Is.EqualTo(Curriculum25.Order(without, N25Blocks.From(without))), "the campaign order with design.n54 = without it");
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
