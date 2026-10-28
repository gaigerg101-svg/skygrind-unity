// NIGHT52 (the shared commit; Tools/night/n52-shared.cjs): the night's rows as data (docs/NIGHT-ROWS.md section 2). Pinned:
//   1. every design.n52 row exists, has a _why and a bool enabled, and is off unless a freeze turned it on (its enabled_why then starts with
//      "ON"); the level and view rows carry no tape bit; ViewOn reads the view rows and --off52 turns them off; each level row's files exist
//      (both copies) and are listed
//   2. with every n52 row off, the campaign order is NIGHT51's (the config without design.n52)
//   3. the off switch: ForceOffFrom 52 (-skygrindOff52 / --off52) with every n52 level row ON leaves NIGHT51's order; 53 and no switch swap
//   4. every n52 level reads as its levelFrom's base to every older system; the look49 grade list holds an n52 id exactly when it holds its levelFrom
// Switches() clears / restores N38-N43Blocks.ForceOff and N45Levels.ForceOff from the start: Lesson25Tests leaves them set (L-082).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    /// <summary>NIGHT52 freeze (n52-freeze.cjs): the level a levelTo id ends up as in the campaign order once later nights' ON level rows
    /// (under the off switch) swap it in turn. With every later row off it returns the id itself.</summary>
    internal static class N52Chain
    {
        public static string Final(GameConfig c, string id)
        {
            var design = PortData.Obj(c.root["design"]);
            for (int hops = 0; hops < 16 && id != null; hops++)
            {
                string next = null;
                foreach (KeyValuePair<string, object> kv in design)
                {
                    if (kv.Key.Length < 2 || kv.Key[0] != 'n' || !int.TryParse(kv.Key.Substring(1), out int nn) || nn >= NightRows.ForceOffFrom) continue;
                    if (!(kv.Value is Dictionary<string, object> night)) continue;
                    foreach (KeyValuePair<string, object> r in night)
                        if (r.Value is Dictionary<string, object> row && row.TryGetValue("levelFrom", out object f) && f as string == id
                            && row.TryGetValue("enabled", out object e) && e is bool on && on && row.TryGetValue("levelTo", out object t) && t is string ts) { next = ts; break; }
                    if (next != null) break;
                }
                if (next == null) return id;
                id = next;
            }
            return id;
        }
    }

    public class N52SharedTests
    {
        static readonly string[] LevelRows = { "kiln52", "foundry52", "iceline52", "spire52", "span52" };
        static readonly string[] ViewRows = { "hud52", "skin52", "guns52", "look52" }; // look52: n52-row-look52.cjs (D-17)

        static string Repo => Path.GetDirectoryName(Port.Root);

        static GameConfig Load() => GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static Dictionary<string, object> N52(GameConfig c) => PortData.Obj(PortData.Obj(c.root["design"])["n52"]);

        static GameConfig WithLevelRows(bool on)
        {
            GameConfig c = Load();
            foreach (string row in LevelRows) PortData.Obj(N52(c)[row])["enabled"] = on;
            return c;
        }

        static GameConfig WithoutN52()
        {
            GameConfig c = Load();
            PortData.Obj(c.root["design"]).Remove("n52");
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
            Assert.That(c.Has("design.n52._why"), Is.True, "design.n52._why");
            var all = new List<string>(LevelRows);
            all.AddRange(ViewRows);
            foreach (string row in all)
            {
                string p = "design.n52." + row + ".";
                Assert.That(c.Has(p + "_why") && c.Get(p + "_why") is string w && w.Length > 0, Is.True, row + "._why");
                Assert.That(c.Has(p + "enabled") && c.Get(p + "enabled") is bool, Is.True, row + ".enabled is a bool");
                bool on = c.Design("n52." + row + ".enabled");
                string ew = c.Has(p + "enabled_why") ? c.Get(p + "enabled_why") as string : null;
                Assert.That(!on || (ew != null && ew.StartsWith("ON", System.StringComparison.Ordinal)), Is.True, row + ": off until a freeze turns it on (its enabled_why then starts with ON)");
            }
            NightRows reg = NightRows.From(c);
            foreach (string row in all) Assert.That(reg.IndexOf(row), Is.EqualTo(-1), row + " has no tape bit (level and view rows are data)");
            int was = NightRows.ForceOffFrom;
            try
            {
                // NIGHT52 close: the batch suite runs Lesson25Tests.TheOrder_FollowsTheBlocks first, which leaves ForceOffFrom at 46 (T-126):
                // the read is pinned to no off switch here, so the ON rows read ON
                NightRows.ForceOffFrom = NightRows.NoOff;
                foreach (string row in ViewRows) Assert.That(NightRows.ViewOn(c, row), Is.EqualTo(c.Design("n52." + row + ".enabled")), row + ": ViewOn reads design.n52");
                NightRows.ForceOffFrom = 52; // -skygrindOff52 / --off52
                foreach (string row in ViewRows) Assert.That(NightRows.ViewOn(c, row), Is.False, "--off52: " + row + " off");
            }
            finally { NightRows.ForceOffFrom = was; }
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");

            var listed = new HashSet<string>();
            foreach (object o in c.Arr("design.maps.list")) listed.Add(PortData.Obj(o)["id"] as string);
            foreach (string row in LevelRows)
            {
                string to = c.Str("design.n52." + row + ".levelTo"), from = c.Str("design.n52." + row + ".levelFrom");
                Assert.That(to.StartsWith("n52-", System.StringComparison.Ordinal), Is.True, row + "'s levelTo " + to);
                Assert.That(listed.Contains(to) && listed.Contains(from), Is.True, row + ": " + from + " and " + to + " are in design.maps.list");
                Assert.That(File.Exists(Path.Combine(Repo, "Design", "levels", to + ".json")), Is.True, to);
                Assert.That(File.Exists(Path.Combine(Repo, "Assets", "StreamingAssets", "Design", "levels", to + ".json")), Is.True, to + " (Player copy)");
            }
        }

        [Test]
        public void EveryRowOff_IsNight51sOrder()
        {
            Switches(NightRows.NoOff, () =>
            {
                List<string> night51 = Order(WithoutN52());
                Assert.That(Order(WithLevelRows(false)), Is.EqualTo(night51), "every n52 level row off: NIGHT51's order (" + string.Join(", ", night51.ToArray()) + ")");
                GameConfig c = Load();
                foreach (string row in LevelRows)
                {
                    string from = c.Str("design.n52." + row + ".levelFrom");
                    Assert.That(night51.Contains(from), Is.True, row + ": its levelFrom " + from + " is in NIGHT51's order (else the row could never swap)");
                }
            });
        }

        [Test]
        public void TheOffSwitch_LeavesNight51sOrder()
        {
            List<string> night51 = null;
            Switches(NightRows.NoOff, () => { night51 = Order(WithoutN52()); });
            Switches(52, () => Assert.That(Order(WithLevelRows(true)), Is.EqualTo(night51), "ForceOffFrom 52 (--off52) with every n52 level row on: NIGHT51's order"));
            foreach (int from in new[] { 53, NightRows.NoOff })
                Switches(from, () =>
                {
                    GameConfig c = WithLevelRows(true);
                    List<string> on = Order(c);
                    Assert.That(on.Count, Is.EqualTo(night51.Count), "a swap keeps the length");
                    foreach (string row in LevelRows)
                    {
                        string lf = c.Str("design.n52." + row + ".levelFrom"), lt = c.Str("design.n52." + row + ".levelTo");
                        int at = night51.IndexOf(lf);
                        // NIGHT58 lead: with no off switch a LATER level row that is ON (kiln57: n52-kiln -> n57-kiln since NIGHT57's freeze)
                        // swaps the slot on again; the n52 row still swapped it when the slot left its levelFrom for a level on the same chain
                        bool swapped = at >= 0 && (on[at] == lt || (from == NightRows.NoOff && on[at] != lf && NightRows.BaseId(c, on[at]) == NightRows.BaseId(c, lt)));
                        Assert.That(swapped, Is.True, "ForceOffFrom " + from + ": " + row + " swaps its level");
                    }
                });
        }

        [Test]
        public void EveryN52Level_ReadsAsItsBase()
        {
            GameConfig c = Load();
            var look = c.Get("design.n49.look49.numbers.levels") as List<object>;
            foreach (string row in LevelRows)
            {
                string from = c.Str("design.n52." + row + ".levelFrom"), to = c.Str("design.n52." + row + ".levelTo");
                Assert.That(NightRows.BaseId(c, to), Is.EqualTo(NightRows.BaseId(c, from)), row + ": NightRows.BaseId chains through " + from);
                Assert.That(N45Levels.BaseId(c, to), Is.EqualTo(N45Levels.BaseId(c, from)), row + ": the chain through N45Levels.BaseId");
                Assert.That(look.Contains(to), Is.EqualTo(look.Contains(from)), row + ": the look49 grade list holds " + to + " exactly when it holds " + from);
            }
        }
    }
}
