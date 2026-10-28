// NIGHT61 builder G (S3, the boss's rule line; Tools/night/n61-g-rows.cjs): design.n61.bossRule61 as data (docs/NIGHT-ROWS.md section 2).
// Pinned:
//   1. bossRule61 is a VIEW row (no tape bit: the line is the HUD's, the sim never reads it) with a _why, a bool enabled, an enabled_why and a
//      numbers object, off unless a freeze turned it on (its enabled_why then starts with "ON"); the checker finds no problem
//   2. NightRows.ViewOn reads it; --off61 (and older) turns it off
//   3. its lines: one per campaign boss, keyed by the boss's bossRise id (wick, brawl40, wyrm, loom37, crown37), each with its _why (the rule it
//      states, with file:line); each at most 8 words (a {button} token counts as one word: one glyph) and naming at least one button with a
//      {token} the lesson widget resolves (UiLesson25.cs: move look jump dash slide fire punch special altFire grapple interact)
// Written test-first: RED on its own commit (the row is not in the config yet). Not run by its author (no Editor in the worktree).
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N61BossRuleRowTests
    {
        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        static readonly string[] Bosses = { "wick", "brawl40", "wyrm", "loom37", "crown37" };
        static readonly string[] Tokens = { "move", "look", "jump", "dash", "slide", "fire", "punch", "special", "altFire", "grapple", "interact" };

        [Test]
        public void BossRule61_ViewRow_OffByDefault_ViewOnReadsIt()
        {
            GameConfig c = Load();
            const string p = "design.n61.bossRule61.";
            Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, "bossRule61._why");
            Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, "bossRule61.enabled is a bool");
            Assert.That(c.Get(p + "numbers") is Dictionary<string, object>, Is.True, "bossRule61.numbers is an object");
            bool on = c.Design("n61.bossRule61.enabled");
            string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
            Assert.That(ew, Is.Not.Null.And.Not.Empty, "bossRule61.enabled_why");
            Assert.That(!on || ew.StartsWith("ON", System.StringComparison.Ordinal), Is.True, "off until a freeze turns it on (its enabled_why then starts with ON)");
            Assert.That(NightRows.From(c).IndexOf("bossRule61"), Is.EqualTo(-1), "bossRule61 has no tape bit (a view row)");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                Assert.That(NightRows.ViewOn(c, "bossRule61"), Is.EqualTo(on), "ViewOn reads design.n61.bossRule61");
                foreach (int from in new[] { 61, 60, 47 })
                {
                    NightRows.ForceOffFrom = from;
                    Assert.That(NightRows.ViewOn(c, "bossRule61"), Is.False, "--off" + from + ": bossRule61 off");
                }
                NightRows.ForceOffFrom = 62;
                Assert.That(NightRows.ViewOn(c, "bossRule61"), Is.EqualTo(on), "--off62 (a later night) leaves it as the config says");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void BossRule61_OneLinePerCampaignBoss_AtMost8Words_EachNamesAButton()
        {
            GameConfig c = Load();
            const string p = "design.n61.bossRule61.numbers.lines.";
            foreach (string b in Bosses)
            {
                Assert.That(c.Has(p + b) && c.Get(p + b) is string, Is.True, "a line for " + b);
                string line = (string)c.Get(p + b);
                Assert.That(c.Has(p + b + "_why") && c.Get(p + b + "_why") is string w && w.Length > 0, Is.True, b + "_why (the rule the line states)");
                string[] words = line.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                Assert.That(words.Length, Is.InRange(1, 8), b + ": at most 8 words, a {button} counting as one: \"" + line + "\"");
                int buttons = 0;
                for (int i = line.IndexOf('{'); i >= 0; i = line.IndexOf('{', i + 1))
                {
                    int j = line.IndexOf('}', i);
                    Assert.That(j, Is.GreaterThan(i + 1), b + ": a closed, named {token}: \"" + line + "\"");
                    string t = line.Substring(i + 1, j - i - 1);
                    Assert.That(System.Array.IndexOf(Tokens, t), Is.GreaterThanOrEqualTo(0), b + ": {" + t + "} is a bound action the lesson widget resolves");
                    buttons++;
                }
                Assert.That(buttons, Is.GreaterThanOrEqualTo(1), b + ": the line names its verb with the bound button's glyph");
            }
        }
    }
}
