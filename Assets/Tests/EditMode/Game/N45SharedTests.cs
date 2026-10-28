// NIGHT45 (the shared commit): the level-only rows. With N45Levels forced off the order is NIGHT44's; every row has its design row and its
// level file (both copies); an n45 id maps to its n43 base, any other id is itself.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N45SharedTests
    {
        static GameConfig cfg;
        static string Root => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root, "PortExport", "config.json"), Path.Combine(Root, "Design", "unity-config.json"));

        [Test]
        public void TheRows_AreListed_WithTheirLevels()
        {
            var c = Cfg;
            foreach (string row in N45Levels.Rows)
            {
                Assert.That(c.Has("design.n45." + row + ".enabled"), Is.True, row);
                string to = c.Str("design.n45." + row + ".levelTo");
                Assert.That(File.Exists(Path.Combine(Root, "Design", "levels", to + ".json")), Is.True, to);
                Assert.That(File.Exists(Path.Combine(Root, "Assets", "StreamingAssets", "Design", "levels", to + ".json")), Is.True, to + " (Player copy)");
                Assert.That(N45Levels.BaseId(c, to), Is.EqualTo(c.Str("design.n45." + row + ".levelFrom")), row + "'s base");
            }
            Assert.That(N45Levels.BaseId(c, "n43-foundry"), Is.EqualTo("n43-foundry"));
        }

        [Test]
        public void TheSwap_OnlyWhenOn()
        {
            var c = Cfg;
            var order = new List<string> { "n43-foundry", "n43-iceline", "n43-kiln", "n40-span", "n41-spire" };
            bool was = N45Levels.ForceOff;
            try
            {
                N45Levels.ForceOff = true;
                var forced = new List<string>(order);
                N45Levels.Swap(c, forced);
                Assert.That(forced, Is.EqualTo(order), "forced off: NIGHT44's order");
                N45Levels.ForceOff = false;
                var on = new List<string>(order);
                N45Levels.Swap(c, on);
                foreach (string row in N45Levels.Rows)
                {
                    int at = order.IndexOf(c.Str("design.n45." + row + ".levelFrom"));
                    Assert.That(on[at], Is.EqualTo(c.Design("n45." + row + ".enabled") ? c.Str("design.n45." + row + ".levelTo") : order[at]), row);
                }
            }
            finally { N45Levels.ForceOff = was; }
        }
    }
}
