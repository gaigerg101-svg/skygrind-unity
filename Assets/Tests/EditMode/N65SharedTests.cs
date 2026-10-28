// NIGHT65 (the lead, A1; the shared commit, Tools/night/n65-a1.cjs): level 1 rebuilt as n65-foundry behind design.n65.foundry65 (NIGHT57's
// recipe). Pinned:
//   1. design.n65.foundry65 is a level row (levelFrom n52-foundry, levelTo n65-foundry), with a _why, off unless a freeze turned it on (its
//      enabled_why then starts with "ON"); the row checker finds no problem
//   2. n65-foundry is in design.maps.list (an unlisted id boots the arena) and its two files exist (Design and the Player's copy)
//   3. while the row is off, the campaign order is the same with and without design.n65; older systems read n65-foundry as n52-foundry
//   4. look49's grade lists n65-foundry wherever it lists n52-foundry (the grade is per level id: PostFx.Look49Listed)
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N65SharedTests
    {
        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        [Test]
        public void Foundry65_IsALevelRow_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n65._why") && c.Get("design.n65._why") is string w && w.Length > 0, Is.True, "design.n65._why");
            const string p = "design.n65.foundry65.";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string rw && rw.Length > 0, Is.True, "foundry65._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, "foundry65.enabled is a bool");
            bool on = c.Design("n65.foundry65.enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, "off until a freeze turns it on (its enabled_why then starts with ON)");
            Assert.That(c.Get(p + "levelFrom") as string, Is.EqualTo("n52-foundry"), "foundry65.levelFrom");
            Assert.That(c.Get(p + "levelTo") as string, Is.EqualTo("n65-foundry"), "foundry65.levelTo");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
        }

        [Test]
        public void Foundry65_IsListed_AndItsFilesExist()
        {
            GameConfig c = Load();
            var listed = new List<string>();
            foreach (object o in c.Arr("design.maps.list")) listed.Add(PortData.Obj(o)["id"] as string);
            Assert.That(listed.Contains("n65-foundry"), Is.True, "n65-foundry is in design.maps.list (an unlisted level boots the arena)");
            string repo = System.IO.Path.GetDirectoryName(Port.Root);
            Assert.That(System.IO.File.Exists(System.IO.Path.Combine(repo, "Design", "levels", "n65-foundry.json")), Is.True, "Design/levels/n65-foundry.json");
            Assert.That(System.IO.File.Exists(System.IO.Path.Combine(repo, "Assets", "StreamingAssets", "Design", "levels", "n65-foundry.json")), Is.True, "the Player copy");
        }

        [Test]
        public void Foundry65_Off_KeepsNight64sCampaign_AndReadsAsN52Foundry()
        {
            GameConfig c = Load();
            Assert.That(NightRows.BaseId(c, "n65-foundry"), Is.EqualTo(NightRows.BaseId(c, "n52-foundry")), "older systems read n65-foundry as n52-foundry");
            if (c.Design("n65.foundry65.enabled")) Assert.Ignore("foundry65 is ON (a freeze turned it on): the order now names n65-foundry");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig without = Load();
                PortData.Obj(without.root["design"]).Remove("n65");
                List<string> order = Curriculum25.Order(c, N25Blocks.From(c));
                Assert.That(order, Is.EqualTo(Curriculum25.Order(without, N25Blocks.From(without))), "the campaign order with design.n65 = without it");
                Assert.That(order.Contains("n65-foundry"), Is.False, "the row off never loads n65-foundry");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void Look49_ListsN65FoundryWhereverItListsN52Foundry()
        {
            GameConfig c = Load();
            var levels = new List<string>();
            foreach (object o in c.Arr("design.n49.look49.numbers.levels")) levels.Add(o as string);
            Assert.That(levels.Contains("n65-foundry"), Is.EqualTo(levels.Contains("n52-foundry")), "look49's grade: n65-foundry as n52-foundry");
        }
    }
}
