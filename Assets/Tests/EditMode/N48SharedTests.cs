// NIGHT48 (the shared commit; Tools/night/n48-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. every design.n48 row exists, has a _why and a bool enabled, and is off unless a freeze turned it on (its enabled_why then starts with
//      "ON"); the sim rows are in the registry with a numbers object, --off48 turns them all off; the level and view rows carry no tape bit;
//      the level row's files exist (both copies) and are listed
//   2. with every n48 row off, the campaign order is NIGHT47's (the config without design.n48)
//   3. the off switch: ForceOffFrom 48 (-skygrindOff48 / --off48) with every n48 level row ON leaves NIGHT47's order; ForceOffFrom 47 leaves
//      NIGHT46's (the config without design.n47 and design.n48); 49 and no switch swap
//   4. every n48 level reads as its levelFrom's base to every older system (n48-foundry -> n47-foundry -> n46-foundry -> n45-foundry -> n43-foundry)
// Switches() clears / restores N38-N43Blocks.ForceOff and N45Levels.ForceOff from the start: Lesson25Tests leaves them set (no restore),
// and the orders here need them as the config says (NIGHT46's and NIGHT47's fix, L-082).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N48SharedTests
    {
        static readonly string[] LevelRows = { "climbTop48" };
        static readonly string[] ViewRows = { "loomHull48", "blast48", "guns48", "foes48", "lineup48" };
        static readonly string[] SimRows = { "wyrmTell48", "wyrmDive48", "kilnBoost48" };

        static string Repo => Path.GetDirectoryName(Port.Root);

        static GameConfig Load() => GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static Dictionary<string, object> N48(GameConfig c) => PortData.Obj(PortData.Obj(c.root["design"])["n48"]);

        static GameConfig WithLevelRows(bool on)
        {
            GameConfig c = Load();
            foreach (string row in LevelRows) PortData.Obj(N48(c)[row])["enabled"] = on;
            return c;
        }

        static GameConfig WithoutN48()
        {
            GameConfig c = Load();
            PortData.Obj(c.root["design"]).Remove("n48");
            return c;
        }

        static GameConfig WithoutN47N48()
        {
            GameConfig c = Load();
            PortData.Obj(c.root["design"]).Remove("n47");
            PortData.Obj(c.root["design"]).Remove("n48");
            return c;
        }

        static List<string> Order(GameConfig c) => Curriculum25.Order(c, N25Blocks.From(c));

        static void Switches(int offFrom, System.Action body)
        {
            int was = NightRows.ForceOffFrom;
            bool was45 = N45Levels.ForceOff;
            bool w38 = N38Blocks.ForceOff, w39 = N39Blocks.ForceOff, w40 = N40Blocks.ForceOff, w41 = N41Blocks.ForceOff, w42 = N42Blocks.ForceOff, w43 = N43Blocks.ForceOff;
            try
            {
                NightRows.ForceOffFrom = offFrom;
                N45Levels.ForceOff = false;
                // Lesson25Tests leaves these set (no restore); the orders here need them as the config says (L-082)
                N38Blocks.ForceOff = N39Blocks.ForceOff = N40Blocks.ForceOff = N41Blocks.ForceOff = N42Blocks.ForceOff = N43Blocks.ForceOff = false;
                body();
            }
            finally
            {
                NightRows.ForceOffFrom = was;
                N45Levels.ForceOff = was45;
                N38Blocks.ForceOff = w38; N39Blocks.ForceOff = w39; N40Blocks.ForceOff = w40; N41Blocks.ForceOff = w41; N42Blocks.ForceOff = w42; N43Blocks.ForceOff = w43;
            }
        }

        [Test]
        public void TheRows_Exist_OffByDefault()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n48._why"), Is.True, "design.n48._why");
            var all = new List<string>(LevelRows);
            all.AddRange(ViewRows);
            all.AddRange(SimRows);
            foreach (string row in all)
            {
                string p = "design.n48." + row + ".";
                Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
                Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
                bool on = c.Design("n48." + row + ".enabled");
                string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
                Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            }
            NightRows reg = NightRows.From(c);
            foreach (string row in SimRows)
            {
                Assert.That(reg.IndexOf(row) >= 0, Is.True, row + " is a sim row (" + reg + ")");
                Assert.That(reg.rows[reg.IndexOf(row)].design, Is.EqualTo(c.Design("n48." + row + ".enabled")), row + ": the registry's default is the config's");
                Assert.That(c.Has("design.n48." + row + ".numbers") && c.Get("design.n48." + row + ".numbers") is Dictionary<string, object>, Is.True, row + ".numbers (the builder's)");
            }
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = 48; // -skygrindOff48: every n48 sim row off whatever the config says
                NightRowState d = NightRows.From(c).Defaults();
                foreach (string row in SimRows) Assert.That(d.On(reg.IndexOf(row)), Is.False, "--off48: " + row + " off");
            }
            finally { NightRows.ForceOffFrom = was; }
            foreach (string row in LevelRows) Assert.That(reg.IndexOf(row), Is.EqualTo(-1), row + " is a level row, no tape bit");
            foreach (string row in ViewRows) Assert.That(reg.IndexOf(row), Is.EqualTo(-1), row + " is a view row, no tape bit");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");

            var listed = new HashSet<string>();
            foreach (object o in c.Arr("design.maps.list")) listed.Add(PortData.Obj(o)["id"] as string);
            foreach (string row in LevelRows)
            {
                string to = c.Str("design.n48." + row + ".levelTo"), from = c.Str("design.n48." + row + ".levelFrom");
                Assert.That(to.StartsWith("n48-", System.StringComparison.Ordinal), Is.True, row + "'s levelTo " + to);
                Assert.That(listed.Contains(to) && listed.Contains(from), Is.True, row + ": " + from + " and " + to + " are in design.maps.list");
                Assert.That(File.Exists(Path.Combine(Repo, "Design", "levels", to + ".json")), Is.True, to);
                Assert.That(File.Exists(Path.Combine(Repo, "Assets", "StreamingAssets", "Design", "levels", to + ".json")), Is.True, to + " (Player copy)");
            }
        }

        [Test]
        public void EveryRowOff_IsNight47sOrder()
        {
            Switches(NightRows.NoOff, () =>
            {
                List<string> night47 = Order(WithoutN48());
                Assert.That(Order(WithLevelRows(false)), Is.EqualTo(night47), "every n48 level row off: NIGHT47's order (" + string.Join(", ", night47.ToArray()) + ")");
                GameConfig c = Load();
                foreach (string row in LevelRows)
                {
                    string from = c.Str("design.n48." + row + ".levelFrom");
                    Assert.That(night47.Contains(from), Is.True, row + ": its levelFrom " + from + " is in NIGHT47's order (else the row could never swap)");
                }
            });
        }

        [Test]
        public void TheOffSwitch_LeavesNight47sOrder()
        {
            List<string> night46 = null, night47 = null;
            // NIGHT52 freeze: n52's ON level rows swap these slots again, so the reference orders are read with every later night off (49+)
            Switches(49, () => { night46 = Order(WithoutN47N48()); night47 = Order(WithoutN48()); });
            Switches(47, () => Assert.That(Order(WithLevelRows(true)), Is.EqualTo(night46), "ForceOffFrom 47 (--off47) with every n48 level row on: NIGHT46's order"));
            Switches(48, () => Assert.That(Order(WithLevelRows(true)), Is.EqualTo(night47), "ForceOffFrom 48 (--off48) with every n48 level row on: NIGHT47's order"));
            foreach (int from in new[] { 49, NightRows.NoOff })
                Switches(from, () =>
                {
                    GameConfig c = WithLevelRows(true);
                    List<string> on = Order(c);
                    Assert.That(on.Count, Is.EqualTo(night47.Count), "a swap keeps the length");
                    foreach (string row in LevelRows)
                    {
                        int at = night47.IndexOf(c.Str("design.n48." + row + ".levelFrom"));
                        Assert.That(at >= 0 && on[at] == N52Chain.Final(c, c.Str("design.n48." + row + ".levelTo")), Is.True, // NIGHT52 freeze: a later ON row may swap the slot again
                             "ForceOffFrom " + from + ": " + row + " swaps its level");
                    }
                });
        }

        [Test]
        public void EveryN48Level_ReadsAsItsBase()
        {
            GameConfig c = Load();
            foreach (string row in LevelRows)
            {
                string from = c.Str("design.n48." + row + ".levelFrom"), to = c.Str("design.n48." + row + ".levelTo");
                Assert.That(NightRows.BaseId(c, to), Is.EqualTo(NightRows.BaseId(c, from)), row + ": NightRows.BaseId chains through " + from);
                Assert.That(N45Levels.BaseId(c, to), Is.EqualTo(N45Levels.BaseId(c, from)), row + ": the chain through N45Levels.BaseId");
            }
            Assert.That(N45Levels.BaseId(c, "n48-foundry"), Is.EqualTo("n43-foundry"), "n48 -> n47 -> n46 -> n45 -> n43");
        }
    }
}
