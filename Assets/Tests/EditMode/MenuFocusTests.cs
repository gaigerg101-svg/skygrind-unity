// NIGHT13 U3-26.7: the web's tests/menuFocus.test.js :7, :12 (primaryIndex) and :48 (keyGrid; here it returns rows, not
// HTML spans) ported against Core/MenusMath.cs (tabDefaults :34 is SettingsTests; readLastPlay :20 is out of scope), plus
// controlRows (the CONTROLS table: every group from config, the techniques' {action} placeholders filled with live keys).
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class MenuFocusTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        [Test]
        public void ResultsScreensFocusRetryOrRematch_NotCopyScoreLine()
        {
            Assert.That(MenusMath.PrimaryIndex("endSurvival", new[] { "copyLine", "save", "retry", "menu" }), Is.EqualTo(2));
            Assert.That(MenusMath.PrimaryIndex("endDuel", new[] { "rematch", "menu" }), Is.EqualTo(0));
            Assert.That(MenusMath.PrimaryIndex("endBossRush", new[] { "saveRush", "retry", "menu" }), Is.EqualTo(1));
        }

        [Test]
        public void EveryOtherScreenKeepsItsFirstButton()
        {
            Assert.That(MenusMath.PrimaryIndex("title", new[] { "survival", "bossRush" }), Is.EqualTo(0));
            Assert.That(MenusMath.PrimaryIndex("pause", new[] { "resume", "restart" }), Is.EqualTo(0));
            Assert.That(MenusMath.PrimaryIndex("endSurvival", new[] { "menu" }), Is.EqualTo(0)); // no retry: the first
        }

        [Test]
        public void ThePauseKeyList_ListsEveryRebindableActionWithItsLiveKey()
        {
            var rows = MenusMath.KeyGrid(cfg, a => a == "jump" ? "N" : "K");
            Assert.That(rows, Does.Contain(("N", "JUMP")));
            Assert.That(rows.Count, Is.EqualTo(cfg.Obj("input.bindings").Count));
        }

        [Test]
        public void ControlRows_EveryGroup_TheTechniquesNameTheLiveKeys()
        {
            var store = new Bindings(cfg);
            store.Set("jump", "KeyN");
            var groups = MenusMath.ControlRows(cfg, a => Bindings.KeyName(store.Code(a)).ToUpperInvariant());
            Assert.That(groups.Count, Is.EqualTo(cfg.Arr("ui.controlGroups").Count + 3));
            Assert.That(groups[0].rows, Does.Contain(("JUMP", "N")));
            var techniques = groups.First(g => g.title == "TECHNIQUES");
            Assert.That(techniques.rows.Any(r => r.what == "WALL JUMP" && r.key == "N against a wall in the air"), Is.True);
            Assert.That(techniques.rows.All(r => !r.key.Contains("{")), Is.True, "every placeholder is a real action");
        }
    }
}
