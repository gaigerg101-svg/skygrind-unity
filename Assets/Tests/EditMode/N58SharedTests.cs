// NIGHT58 (the shared commit; Tools/night/n58-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. design.n58's rows exist, each with a _why and a bool enabled; off unless a freeze turned it on (its enabled_why then starts with "ON")
//   2. span58 is a level row (levelFrom n52-span, levelTo n58-span), listed, its two files exist, and, while off, the campaign order is the
//      same with and without design.n58
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N58SharedTests
    {
        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        static bool CheckRow(GameConfig c, string row)
        {
            string p = "design.n58." + row + ".";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
            bool on = c.Design("n58." + row + ".enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            return on;
        }

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n58._why"), Is.True, "design.n58._why");
            CheckRow(c, "span58");
            Assert.That(c.Get("design.n58.span58.levelFrom") as string, Is.EqualTo("n52-span"), "span58.levelFrom");
            Assert.That(c.Get("design.n58.span58.levelTo") as string, Is.EqualTo("n58-span"), "span58.levelTo");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
        }

        [Test]
        public void Span58_IsListed_AndItsFilesExist()
        {
            GameConfig c = Load();
            var listed = new List<string>();
            foreach (object o in c.Arr("design.maps.list")) listed.Add(PortData.Obj(o)["id"] as string);
            Assert.That(listed.Contains("n58-span"), Is.True, "n58-span is in design.maps.list (an unlisted level ends the campaign)");
            string repo = System.IO.Path.GetDirectoryName(Port.Root);
            Assert.That(System.IO.File.Exists(System.IO.Path.Combine(repo, "Design", "levels", "n58-span.json")), Is.True, "Design/levels/n58-span.json");
            Assert.That(System.IO.File.Exists(System.IO.Path.Combine(repo, "Assets", "StreamingAssets", "Design", "levels", "n58-span.json")), Is.True, "the Player copy");
            Assert.That(NightRows.BaseId(c, "n58-span"), Is.Not.EqualTo("n58-span"), "older systems know n58-span by its chain (NightRows.BaseId)");
        }

        [Test]
        public void Span58_Off_KeepsNight57sCampaign()
        {
            GameConfig c = Load();
            if (c.Design("n58.span58.enabled")) Assert.Ignore("span58 is ON (a freeze turned it on): the order now names n58-span");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig without = Load();
                PortData.Obj(without.root["design"]).Remove("n58");
                Assert.That(Curriculum25.Order(c, N25Blocks.From(c)), Is.EqualTo(Curriculum25.Order(without, N25Blocks.From(without))), "the campaign order with design.n58 = without it");
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
