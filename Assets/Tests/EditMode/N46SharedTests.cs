// NIGHT46 (the shared commit; Tools/night/n46-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. every design.n46 row exists, has a _why and a bool enabled, and is off unless a freeze turned it on (its enabled_why then starts
//      with "ON", as Tools/night/n45-rows-on.cjs writes); the six sim rows are in the registry with a numbers object, --off46 turns them all
//      off; the level and view rows carry no tape bit; the level rows' files exist (both copies) and are listed
//   2. with every n46 row off, the campaign order is NIGHT45's (the config without design.n46)
//   3. the off switch (NightRows.ForceOffFrom 46 = -skygrindOff46 / --off46, or 45) leaves NIGHT45's order with every level row ON; 47 swaps
//   4. every n46 level reads as its levelFrom's base to every older system (n46-foundry -> n45-foundry -> n43-foundry; n46-span -> n40-span)
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N46SharedTests
    {
        static readonly string[] LevelRows = { "foundry46", "iceline46", "kiln46", "span46", "spire46" };
        static readonly string[] ViewRows = { "shift46", "feel46" };
        static readonly string[] SimRows = { "retry46", "overlap46", "teach46", "reveal46", "wyrm46", "walkin46" };

        static string Repo => Path.GetDirectoryName(Port.Root);

        // a fresh config each time: GameConfig caches lookups and NightRows caches level rows by config, so a variant is edited before any read
        static GameConfig Load() => GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static Dictionary<string, object> N46(GameConfig c) => PortData.Obj(PortData.Obj(c.root["design"])["n46"]);

        static GameConfig WithLevelRows(bool on)
        {
            GameConfig c = Load();
            foreach (string row in LevelRows) PortData.Obj(N46(c)[row])["enabled"] = on;
            return c;
        }

        static GameConfig WithoutN46()
        {
            GameConfig c = Load();
            PortData.Obj(c.root["design"]).Remove("n46");
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
                // Lesson25Tests leaves these set (no restore); NIGHT45's order needs them as the config says
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
            Assert.That(c.Has("design.n46._why"), Is.True, "design.n46._why");
            var all = new List<string>(LevelRows);
            all.AddRange(ViewRows);
            all.AddRange(SimRows);
            foreach (string row in all)
            {
                string p = "design.n46." + row + ".";
                Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
                Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
                bool on = c.Design("n46." + row + ".enabled");
                string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
                Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            }
            NightRows reg = NightRows.From(c);
            foreach (string row in SimRows)
            {
                Assert.That(reg.IndexOf(row) >= 0, Is.True, row + " is a sim row (" + reg + ")");
                Assert.That(reg.rows[reg.IndexOf(row)].design, Is.EqualTo(c.Design("n46." + row + ".enabled")), row + ": the registry's default is the config's");
                Assert.That(c.Has("design.n46." + row + ".numbers") && c.Get("design.n46." + row + ".numbers") is Dictionary<string, object>, Is.True, row + ".numbers (the builder's)");
            }
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = 46; // -skygrindOff46: every n46 sim row off whatever the config says
                Assert.That(NightRows.From(c).Defaults().Any, Is.False, "--off46: no NIGHT46+ sim row on");
            }
            finally { NightRows.ForceOffFrom = was; }
            foreach (string row in LevelRows) Assert.That(reg.IndexOf(row), Is.EqualTo(-1), row + " is a level row, no tape bit");
            foreach (string row in ViewRows) Assert.That(reg.IndexOf(row), Is.EqualTo(-1), row + " is a view row, no tape bit");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");

            var listed = new HashSet<string>();
            foreach (object o in c.Arr("design.maps.list")) listed.Add(PortData.Obj(o)["id"] as string);
            foreach (string row in LevelRows)
            {
                string to = c.Str("design.n46." + row + ".levelTo"), from = c.Str("design.n46." + row + ".levelFrom");
                Assert.That(to.StartsWith("n46-", System.StringComparison.Ordinal), Is.True, row + "'s levelTo " + to);
                Assert.That(listed.Contains(to) && listed.Contains(from), Is.True, row + ": " + from + " and " + to + " are in design.maps.list");
                Assert.That(File.Exists(Path.Combine(Repo, "Design", "levels", to + ".json")), Is.True, to);
                Assert.That(File.Exists(Path.Combine(Repo, "Assets", "StreamingAssets", "Design", "levels", to + ".json")), Is.True, to + " (Player copy)");
            }
        }

        [Test]
        public void EveryRowOff_IsNight45sOrder()
        {
            Switches(NightRows.NoOff, () =>
            {
                List<string> night45 = Order(WithoutN46());
                Assert.That(Order(WithLevelRows(false)), Is.EqualTo(night45), "every n46 level row off: NIGHT45's order (" + string.Join(", ", night45.ToArray()) + ")");
                GameConfig c = Load();
                foreach (string row in LevelRows)
                {
                    string from = c.Str("design.n46." + row + ".levelFrom");
                    Assert.That(night45.Contains(from), Is.True, row + ": its levelFrom " + from + " is in NIGHT45's order (else the row could never swap)");
                }
            });
        }

        [Test]
        public void TheOffSwitch_LeavesNight45sOrder()
        {
            List<string> night45 = null;
            Switches(NightRows.NoOff, () => night45 = Order(WithoutN46()));
            foreach (int from in new[] { 46, 45, 19 })
                Switches(from, () => Assert.That(Order(WithLevelRows(true)), Is.EqualTo(night45), "ForceOffFrom " + from + " with every level row on: NIGHT45's order"));
            foreach (int from in new[] { 47 }) // NIGHT47 freeze (DECIDE47 D-7): with no off switch the ON n47 level rows replace the n46 levels by design; N47SharedTests pins that swap
                Switches(from, () =>
                {
                    GameConfig c = WithLevelRows(true);
                    List<string> on = Order(c);
                    Assert.That(on.Count, Is.EqualTo(night45.Count), "a swap keeps the length");
                    foreach (string row in LevelRows)
                    {
                        int at = night45.IndexOf(c.Str("design.n46." + row + ".levelFrom"));
                        Assert.That(at >= 0 && on[at] == c.Str("design.n46." + row + ".levelTo"), Is.True, "ForceOffFrom " + from + ": " + row + " swaps its level");
                    }
                });
        }

        [Test]
        public void EveryN46Level_ReadsAsItsBase()
        {
            GameConfig c = Load();
            foreach (string row in LevelRows)
            {
                string from = c.Str("design.n46." + row + ".levelFrom"), to = c.Str("design.n46." + row + ".levelTo");
                Assert.That(NightRows.BaseId(c, to), Is.EqualTo(from), row + ": NightRows.BaseId");
                Assert.That(N45Levels.BaseId(c, to), Is.EqualTo(N45Levels.BaseId(c, from)), row + ": the chain through N45Levels.BaseId");
            }
            Assert.That(N45Levels.BaseId(c, "n46-foundry"), Is.EqualTo("n43-foundry"), "n46 -> n45 -> n43 (N43BaseId then continues)");
            Assert.That(N45Levels.BaseId(c, "n46-span"), Is.EqualTo("n40-span"), "level 4: THE LOOM's rooms and the walk pilot");
            Assert.That(N45Levels.BaseId(c, "n46-spire"), Is.EqualTo("n41-spire"), "level 5 (N41BaseId then continues to n40-spire)");
            Assert.That(N45Levels.BaseId(c, "n45-kiln"), Is.EqualTo("n43-kiln"), "NIGHT45's mapping unchanged");
            Assert.That(NightRows.BaseId(c, "n40-span"), Is.EqualTo("n40-span"), "any other id is itself");
        }
    }
}
