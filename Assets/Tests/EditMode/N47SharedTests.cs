// NIGHT47 (the shared commit; Tools/night/n47-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. every design.n47 row exists, has a _why and a bool enabled, and is off unless a freeze turned it on (its enabled_why then starts with
//      "ON"); the sim rows are in the registry with a numbers object, --off47 turns them all off; the level and view rows carry no tape bit;
//      the level rows' files exist (both copies) and are listed
//   2. with every n47 row off, the campaign order is NIGHT46's (the config without design.n47)
//   3. the off switch (NightRows.ForceOffFrom 47 = -skygrindOff47 / --off47, or 46) leaves NIGHT46's order with every n47 level row ON
//   4. every n47 level reads as its levelFrom's base to every older system (n47-foundry -> n46-foundry -> n45-foundry -> n43-foundry)
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N47SharedTests
    {
        static readonly string[] LevelRows = { "foundry47", "iceline47", "kiln47", "span47", "spire47" };
        static readonly string[] ViewRows = { "wickLook47", "chargerLook47", "brawlLook47", "wyrmLook47", "loomLook47", "crownLook47", "fx47", "grapple47", "zoom47", "hud47", "copy47", "visor47", "foes47", "door47", "lava47" };
        static readonly string[] SimRows = { "wick47", "climb47", "wickTop47", "charger47", "brawl47", "wyrm47", "loom47", "crown47", "pulse47", "overheal47" };

        static string Repo => Path.GetDirectoryName(Port.Root);

        static GameConfig Load() => GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static Dictionary<string, object> N47(GameConfig c) => PortData.Obj(PortData.Obj(c.root["design"])["n47"]);

        static GameConfig WithLevelRows(bool on)
        {
            GameConfig c = Load();
            foreach (string row in LevelRows) PortData.Obj(N47(c)[row])["enabled"] = on;
            return c;
        }

        static GameConfig WithoutN47()
        {
            GameConfig c = Load();
            PortData.Obj(c.root["design"]).Remove("n47");
            return c;
        }

        static List<string> Order(GameConfig c) => Curriculum25.Order(c, N25Blocks.From(c));

        static void Switches(int offFrom, System.Action body)
        {
            int was = NightRows.ForceOffFrom;
            bool was45 = N45Levels.ForceOff;
            bool w38 = N38Blocks.ForceOff, w40 = N40Blocks.ForceOff, w41 = N41Blocks.ForceOff, w42 = N42Blocks.ForceOff, w43 = N43Blocks.ForceOff;
            try
            {
                NightRows.ForceOffFrom = offFrom;
                N45Levels.ForceOff = false;
                // Lesson25Tests leaves these set (no restore); NIGHT46's order needs them as the config says (NIGHT46's N46SharedTests fix, L-082)
                N38Blocks.ForceOff = N40Blocks.ForceOff = N41Blocks.ForceOff = N42Blocks.ForceOff = N43Blocks.ForceOff = false;
                body();
            }
            finally
            {
                NightRows.ForceOffFrom = was;
                N45Levels.ForceOff = was45;
                N38Blocks.ForceOff = w38; N40Blocks.ForceOff = w40; N41Blocks.ForceOff = w41; N42Blocks.ForceOff = w42; N43Blocks.ForceOff = w43;
            }
        }

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n47._why"), Is.True, "design.n47._why");
            var all = new List<string>(LevelRows);
            all.AddRange(ViewRows);
            all.AddRange(SimRows);
            foreach (string row in all)
            {
                string p = "design.n47." + row + ".";
                Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
                Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
                bool on = c.Design("n47." + row + ".enabled");
                string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
                Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            }
            NightRows reg = NightRows.From(c);
            foreach (string row in SimRows)
            {
                Assert.That(reg.IndexOf(row) >= 0, Is.True, row + " is a sim row (" + reg + ")");
                Assert.That(reg.rows[reg.IndexOf(row)].design, Is.EqualTo(c.Design("n47." + row + ".enabled")), row + ": the registry's default is the config's");
                Assert.That(c.Has("design.n47." + row + ".numbers") && c.Get("design.n47." + row + ".numbers") is Dictionary<string, object>, Is.True, row + ".numbers (the builder's)");
            }
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = 47; // -skygrindOff47: every n47 sim row off whatever the config says
                NightRowState d = NightRows.From(c).Defaults();
                foreach (string row in SimRows) Assert.That(d.On(reg.IndexOf(row)), Is.False, "--off47: " + row + " off");
            }
            finally { NightRows.ForceOffFrom = was; }
            foreach (string row in LevelRows) Assert.That(reg.IndexOf(row), Is.EqualTo(-1), row + " is a level row, no tape bit");
            foreach (string row in ViewRows) Assert.That(reg.IndexOf(row), Is.EqualTo(-1), row + " is a view row, no tape bit");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");

            var listed = new HashSet<string>();
            foreach (object o in c.Arr("design.maps.list")) listed.Add(PortData.Obj(o)["id"] as string);
            foreach (string row in LevelRows)
            {
                string to = c.Str("design.n47." + row + ".levelTo"), from = c.Str("design.n47." + row + ".levelFrom");
                Assert.That(to.StartsWith("n47-", System.StringComparison.Ordinal), Is.True, row + "'s levelTo " + to);
                Assert.That(listed.Contains(to) && listed.Contains(from), Is.True, row + ": " + from + " and " + to + " are in design.maps.list");
                Assert.That(File.Exists(Path.Combine(Repo, "Design", "levels", to + ".json")), Is.True, to);
                Assert.That(File.Exists(Path.Combine(Repo, "Assets", "StreamingAssets", "Design", "levels", to + ".json")), Is.True, to + " (Player copy)");
            }
        }

        [Test]
        public void EveryRowOff_IsNight46sOrder()
        {
            Switches(NightRows.NoOff, () =>
            {
                List<string> night46 = Order(WithoutN47());
                Assert.That(Order(WithLevelRows(false)), Is.EqualTo(night46), "every n47 level row off: NIGHT46's order (" + string.Join(", ", night46.ToArray()) + ")");
                GameConfig c = Load();
                foreach (string row in LevelRows)
                {
                    string from = c.Str("design.n47." + row + ".levelFrom");
                    Assert.That(night46.Contains(from), Is.True, row + ": its levelFrom " + from + " is in NIGHT46's order (else the row could never swap)");
                }
            });
        }

        [Test]
        public void TheOffSwitch_LeavesNight46sOrder()
        {
            List<string> night46 = null;
            Switches(NightRows.NoOff, () => night46 = Order(WithoutN47()));
            foreach (int from in new[] { 47 })
                Switches(from, () => Assert.That(Order(WithLevelRows(true)), Is.EqualTo(night46), "ForceOffFrom " + from + " with every n47 level row on: NIGHT46's order"));
            foreach (int from in new[] { 48, NightRows.NoOff })
                Switches(from, () =>
                {
                    GameConfig c = WithLevelRows(true);
                    List<string> on = Order(c);
                    Assert.That(on.Count, Is.EqualTo(night46.Count), "a swap keeps the length");
                    foreach (string row in LevelRows)
                    {
                        // NIGHT49 P0: climbTop48 (n48-foundry, levelFrom n47-foundry) takes this level's swap; the n48 chain is EveryN48Level_ReadsAsItsBase's job
                        if (row == "foundry47") continue;
                        int at = night46.IndexOf(c.Str("design.n47." + row + ".levelFrom"));
                        Assert.That(at >= 0 && on[at] == N52Chain.Final(c, c.Str("design.n47." + row + ".levelTo")), Is.True, // NIGHT52 freeze: a later ON row may swap the slot again
                             "ForceOffFrom " + from + ": " + row + " swaps its level");
                    }
                });
        }

        [Test]
        public void EveryN47Level_ReadsAsItsBase()
        {
            GameConfig c = Load();
            foreach (string row in LevelRows)
            {
                string from = c.Str("design.n47." + row + ".levelFrom"), to = c.Str("design.n47." + row + ".levelTo");
                Assert.That(NightRows.BaseId(c, to), Is.EqualTo(NightRows.BaseId(c, from)), row + ": NightRows.BaseId chains through " + from);
                Assert.That(N45Levels.BaseId(c, to), Is.EqualTo(N45Levels.BaseId(c, from)), row + ": the chain through N45Levels.BaseId");
            }
            Assert.That(N45Levels.BaseId(c, "n47-foundry"), Is.EqualTo("n43-foundry"), "n47 -> n46 -> n45 -> n43");
            Assert.That(N45Levels.BaseId(c, "n47-span"), Is.EqualTo("n40-span"), "level 4: THE LOOM's rooms and the walk pilot");
            Assert.That(N45Levels.BaseId(c, "n47-spire"), Is.EqualTo("n41-spire"), "level 5");
        }
    }
}
