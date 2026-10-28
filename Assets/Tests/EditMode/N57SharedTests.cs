// NIGHT57 (the shared commit; Tools/night/n57-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. design.n57's rows exist, each with a _why and a bool enabled; off unless a freeze turned it on (its enabled_why then starts with
//      "ON"); the sim rows have a tape bit and a numbers object; --off57 and --off56 turn every sim row off
//   2. kiln57 is a level row (levelFrom n52-kiln, levelTo n57-kiln) and, while off, the campaign order is the same with and without design.n57
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N57SharedTests
    {
        static readonly string[] Sims = { "press57" };

        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        static bool CheckRow(GameConfig c, string row)
        {
            string p = "design.n57." + row + ".";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
            bool on = c.Design("n57." + row + ".enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            return on;
        }

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n57._why"), Is.True, "design.n57._why");
            foreach (string s in Sims)
            {
                CheckRow(c, s);
                Assert.That(c.Get("design.n57." + s + ".numbers") is Dictionary<string, object>, Is.True, s + ".numbers is an object");
                Assert.That(NightRows.From(c).IndexOf(s), Is.GreaterThanOrEqualTo(0), s + " is a sim row (a tape bit)");
            }
            CheckRow(c, "kiln57");
            Assert.That(c.Get("design.n57.kiln57.levelFrom") as string, Is.EqualTo("n52-kiln"), "kiln57.levelFrom");
            Assert.That(c.Get("design.n57.kiln57.levelTo") as string, Is.EqualTo("n57-kiln"), "kiln57.levelTo");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
        }

        [Test]
        public void Kiln57_IsListed_AndItsFilesExist()
        {
            GameConfig c = Load();
            var listed = new List<string>();
            foreach (object o in c.Arr("design.maps.list")) listed.Add(PortData.Obj(o)["id"] as string);
            Assert.That(listed.Contains("n57-kiln"), Is.True, "n57-kiln is in design.maps.list (an unlisted level ends the campaign)");
            string repo = System.IO.Path.GetDirectoryName(Port.Root);
            Assert.That(System.IO.File.Exists(System.IO.Path.Combine(repo, "Design", "levels", "n57-kiln.json")), Is.True, "Design/levels/n57-kiln.json");
            Assert.That(System.IO.File.Exists(System.IO.Path.Combine(repo, "Assets", "StreamingAssets", "Design", "levels", "n57-kiln.json")), Is.True, "the Player copy");
        }

        [Test]
        public void Kiln57_Off_KeepsNight56sCampaign()
        {
            GameConfig c = Load();
            if (c.Design("n57.kiln57.enabled")) Assert.Ignore("kiln57 is ON (a freeze turned it on): the order now names n57-kiln");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig without = Load();
                PortData.Obj(without.root["design"]).Remove("n57");
                Assert.That(Curriculum25.Order(c, N25Blocks.From(c)), Is.EqualTo(Curriculum25.Order(without, N25Blocks.From(without))), "the campaign order with design.n57 = without it");
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
