// NIGHT50 shared commit (Tools/night/n50-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. every design.n50 row exists with a _why and a bool enabled, is off unless a freeze turned it on (its enabled_why then starts with "ON"),
//      and NightRows.ViewOn reads the same switch; the checker finds no problem in design
//   2. the off switch: ForceOffFrom 50 (-skygrindOff50 / --off50) turns every n50 view row off whatever the config says
//   3. look49.levels exists and is an empty list in the shared commit (the grade is on for no level until the freeze lists one)
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N50SharedTests
    {
        static readonly string[] ViewRows = { "charge50", "gunglow50" };

        static GameConfig Load() => GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            // the switch is read with the off switch cleared: another test may leave ForceOffFrom below 50 (L-082, as N49SharedTests does)
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                CheckRows();
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        static void CheckRows()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n50._why"), Is.True, "design.n50._why");
            foreach (string row in ViewRows)
            {
                string p = "design.n50." + row + ".";
                Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
                Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
                bool on = c.Design("n50." + row + ".enabled");
                string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
                Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
                Assert.That(NightRows.ViewOn(c, row), Is.EqualTo(on), row + ": NightRows.ViewOn reads the design.n50 switch");
            }
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
        }

        [Test]
        public void OffSwitch50_TurnsEveryViewRowOff()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = 50; // -skygrindOff50 / --off50
                GameConfig c = Load();
                foreach (string row in ViewRows) Assert.That(NightRows.ViewOn(c, row), Is.False, "--off50: " + row + " off");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void Look49Levels_ListsOnlyCampaignLevels()
        {
            // the freeze lists the levels the grade is on for (DECIDE50 D-4); an empty list is also valid (the grade is off everywhere)
            GameConfig c = Load();
            Assert.That(c.Has("design.n49.look49.numbers.levels"), Is.True, "look49.numbers.levels exists");
            var levels = c.Get("design.n49.look49.numbers.levels") as List<object>;
            Assert.That(levels, Is.Not.Null, "look49.numbers.levels is a list");
            string[] campaign = { "n48-foundry", "n47-iceline", "n47-kiln", "n47-span", "n47-spire", "n52-foundry", "n52-iceline", "n52-kiln", "n52-span", "n52-spire" }; // NIGHT52 (n52-shared.cjs): the n52 copies
            foreach (object l in levels) Assert.That(System.Array.IndexOf(campaign, l as string) >= 0, Is.True, "a campaign level: " + l);
        }

    }
}
