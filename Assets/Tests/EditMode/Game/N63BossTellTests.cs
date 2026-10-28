// NIGHT63 builder G (slate B2's fix, design.n63.bossTell63; Game/BossTell63.cs): the danger cue's pure rules. Pinned:
//   1. the switch: off with the row off, under -skygrindOff63 (or an older night's), under -skygrindNoBossTell63; another row's switch or a
//      later night's leaves it on
//   2. which attacks get the cue: every non-parry attack in its wind-up (the sim's tell kind red or unknown), never a parryable one (kind yellow:
//      THE WICK's ember, THE LOOM's spool, THE CROWN's gem shard, THE ICEBREAKER's block) and never the wyrm's punch window (aggro, aggroLean: its
//      head goes yellow, WyrmView.cs:125) or its roar (the core opening for the player, not an attack); no attack id: no cue
//   3. the level: from BossTell63From at the wind-up's first step to 1 at the hit, never falling
//   4. the cue is one the meter reads (L-096): its drawn colour is in the danger hue class at every level and, at the wind-up's first step, rises
//      above nothing by more than the meter's Boss63CueRiseAbs: so the boss63 cue line reads it from the first frame (lead = the wind-up)
//   5. (B2c, DECIDE63 D-11) the fix list in config is the one BossCue63.Verdict computes from the before measure's cue lines (a fixture the
//      tool writes with the config), and the C# parse lists the same attacks as the tool's self-test
//   6. (review S3 / S7, Logs/n63/review-n63.md) the late pull takes only the hues between the colour table's danger and parry bands (35-45):
//      the amber 31.8 is danger in the table, the parry role (45-70: the parry yellow, the wyrm's punch window #FFD23A, the spool) never pulled;
//      the fix list's danger is recomputed from each line's hue with the table's bands (the spit's 32 deg riser is danger)
//   7. (B2d, n63-b9: crownFire still off-hue after the fix, "hue 41 sat 0.14 ... obj THE CROWN/crown clapper", no recolour line) a WHITE-HOT
//      warm glow on a listed attack (the crown's clapper at its fire, Crown47WhiteHex x 2.4) is pulled to the danger hue at its brightness: pale
//      (no hue the meter reads) but warm (the danger band's start to the parry band's start) and HDR-hot; a pale surface (not hot), a pure
//      white, a pale yellow (the parry band), the boundary's violet-white stay as they are
// Written test-first: RED on its own commit (the placeholder wants nothing and draws black). Not run in Unity by its author.
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N63BossTellTests
    {
        [Test]
        public void Switch_RowAndSwitches()
        {
            // the row's config state is not read here (the switch's command-line half): a null config reads as the row off
            Assert.IsNotNull(BossTell63.OffReason(null, new string[0]), "no config: off");
            Assert.AreEqual("-skygrindNoBossTell63", BossTell63.NoSwitch);
            Assert.AreEqual("bossTell63", BossTell63.Row);
        }

        [Test]
        public void WhichAttacks_EveryNonParryAttack_NeverAParryTell()
        {
            Assert.IsTrue(BossTell63.Wants("wick", "wickSlam", "red"), "THE WICK's slam (red)");
            Assert.IsTrue(BossTell63.Wants("loom37", "lash", "red"));
            Assert.IsTrue(BossTell63.Wants("loom37", "cage", "red"));
            Assert.IsTrue(BossTell63.Wants("brawl40", "brawlLeap", "red"));
            Assert.IsTrue(BossTell63.Wants("crown37", "bladeRing", "red"));
            Assert.IsTrue(BossTell63.Wants("wyrm", "spit", "?"), "the wyrm's spit: no tell kind in the sim, an attack");
            Assert.IsTrue(BossTell63.Wants("wyrm", "shed", "?"));
            Assert.IsFalse(BossTell63.Wants("wick", "wickEmber", "yellow"), "a parryable ember: its yellow tell untouched");
            Assert.IsFalse(BossTell63.Wants("loom37", "spool", "yellow"));
            Assert.IsFalse(BossTell63.Wants("crown37", "gemShard", "yellow"));
            Assert.IsFalse(BossTell63.Wants("brawl40", "brawlBlock", "yellow"));
            Assert.IsFalse(BossTell63.Wants("wyrm", "aggro", "?"), "the wyrm's punch window (its head yellow)");
            Assert.IsFalse(BossTell63.Wants("wyrm", "aggroLean", "?"));
            Assert.IsFalse(BossTell63.Wants("wyrm", "roar", "?"), "the core opening: not an attack");
            Assert.IsFalse(BossTell63.Wants("wick", null, "red"), "no attack");
        }

        [Test]
        public void Level_FromItsFloorToOne_NeverFalling()
        {
            Assert.That(BossTell63.Level(0), Is.EqualTo(UnityTuning.BossTell63From).Within(1e-6f));
            Assert.That(BossTell63.Level(1), Is.EqualTo(1f).Within(1e-6f));
            float last = -1f;
            for (int i = 0; i <= 20; i++)
            {
                float l = BossTell63.Level(i / 20.0);
                Assert.That(l, Is.GreaterThanOrEqualTo(last - 1e-6f), "never falling at " + i / 20.0);
                last = l;
            }
            Assert.That(BossTell63.Level(-1), Is.EqualTo(UnityTuning.BossTell63From).Within(1e-6f), "clamped");
            Assert.That(BossTell63.Level(2), Is.EqualTo(1f).Within(1e-6f), "clamped");
        }

        // ---- NIGHT63 G B2b (the lead: the fix only on the attacks the before measure lists, computed from the rule; the listed attack's own cue
        // in the danger hue). Added test-first (RED on its own commit: the placeholder has no list and pulls no colour) ----

        static Skygrind.Core.GameConfig Cfg()
        {
            string root;
            try { root = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { root = System.IO.Path.GetFullPath("PortExport"); }
            return Skygrind.Core.GameConfig.Load(System.IO.Path.Combine(root, "config.json"));
        }

        // ---- NIGHT63 G B2c (DECIDE63 D-11): the fix list is checked against the before MEASURE, never a hand list. The before set's
        // "[boss63] cue" lines are a fixture (Fixtures/n63-b2-before.txt, written by Tools/night/n63-b2-fixlist.cjs --write together with the
        // config's list, its source logs on its first line); the test computes the list from it with BossCue63.Verdict (the reel's own rule)
        // and compares it to design.n63.bossTell63.numbers.fix. A re-measure rewrites both; a hand edit of either fails here. (The hand list
        // this replaces pinned n63-b4's spit / beamCut / lowFire: the meter corrected on b6 reads beamCut and lowFire ok before the fix.)
        // Added test-first (RED on its own commit: no fixture yet) ----

        static readonly System.Text.RegularExpressions.Regex CueRx = new System.Text.RegularExpressions.Regex(
            @"\[boss63\] cue (\S+) (\S+) start (\S+) hit (\d+) lead (\d+) hue (\S+) sat (\S+) parry (yes|no) danger (yes|no) tell (\S+) kind (\S+)");

        /// <summary>The fix list from boss63 cue lines (the tool's parse): each attack whose BossCue63.Verdict is not Ok, once, in line order.
        /// Lines starting '#' and the "cue roots" lines are skipped.</summary>
        static System.Collections.Generic.List<string> FixFrom(System.Collections.Generic.IEnumerable<string> lines, out int cues)
        {
            var ids = new System.Collections.Generic.List<string>();
            cues = 0;
            foreach (string l in lines)
            {
                if (l == null || l.StartsWith("#")) continue;
                System.Text.RegularExpressions.Match m = CueRx.Match(l);
                if (!m.Success) continue;
                cues++;
                string tell = m.Groups[10].Value;
                // review S3: danger is recomputed from the printed hue and saturation with the bands in use (the colour table's), not read from
                // the line (a line printed under older bands says what those bands said); the tell is the line's own
                bool found = m.Groups[3].Value != "none";
                var inv = System.Globalization.CultureInfo.InvariantCulture;
                var c = new Cue63
                {
                    found = found,
                    parry = m.Groups[8].Value == "yes",
                    danger = found && m.Groups[6].Value != "-" && BossCue63.DangerHue(double.Parse(m.Groups[6].Value, inv), double.Parse(m.Groups[7].Value, inv)),
                    tellFound = tell != "none",
                    tellLeadMs = tell == "none" ? 0 : double.Parse(tell, inv),
                };
                string id = m.Groups[1].Value + ":" + m.Groups[2].Value;
                if (BossCue63.Verdict(c, m.Groups[11].Value) != BossCue63.Verdict63.Ok && !ids.Contains(id)) ids.Add(id);
            }
            return ids;
        }

        /// <summary>The list in config is the one computed from the before measure's fixture (n63-b6's five before logs at this commit).</summary>
        [Test]
        public void TheFixList_IsTheOneComputedFromTheBeforeMeasure()
        {
            string assets;
            try { assets = Application.dataPath; }
            catch (System.Exception) { assets = "Assets"; } // outside Unity (a scratch runner from the repo root)
            string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(assets, "Tests", "EditMode", "Game", "Fixtures", "n63-b2-before.txt"));
            Assert.IsTrue(System.IO.File.Exists(path), "the before measure's fixture " + path + " (node Tools/night/n63-b2-fixlist.cjs <before logs> --write)");
            string[] lines = System.IO.File.ReadAllLines(path);
            Assert.That(lines.Length > 0 && lines[0].StartsWith("# source:"), Is.True, "its first line names its source logs");
            System.Collections.Generic.List<string> want = FixFrom(lines, out int cues);
            Assert.That(cues, Is.GreaterThan(0), "the fixture holds the before set's cue lines");
            CollectionAssert.AreEqual(want, BossTell63.FixList(Cfg()),
                "design.n63.bossTell63.numbers.fix is the list BossCue63.Verdict computes from the fixture (" + string.Join(", ", want) + ")");
        }

        /// <summary>The C# parse + BossCue63.Verdict list the same attacks as the tool's own self-test (Tools/night/n63-b2-fixlist.cjs
        /// --selftest: its six planted lines and its expected list), so the tool and this test apply one rule.</summary>
        [Test]
        public void TheFixListRule_IsTheTools()
        {
            string[] lines =
            {
                "[boss63] cue crown37 beamCut start 7650 hit 9133 lead 1483 hue 39 sat 0.65 parry yes danger no tell 1483 kind red objects 169 obj crownLook47/crown point edge",
                "[boss63] cue crown37 gemShard start 3950 hit 5125 lead 1175 hue 46 sat 0.76 parry yes danger no tell 1175 kind yellow objects 143 obj light THE CROWN/crown light",
                "[boss63] cue wick wickSlam start 4367 hit 5467 lead 1100 hue 10 sat 0.92 parry no danger yes tell 1100 kind red objects 2 obj the wick/flame core",
                "[boss63] cue wyrm spit start 23750 hit 24492 lead 742 hue 32 sat 0.84 parry no danger no tell 742 kind ? objects 6 obj KILN WYRM/trim 9",
                "[boss63] cue wyrm aggro start none hit 13408 lead 0 hue - sat - parry no danger no tell none kind ? objects 0 obj -",
                "[boss63] cue loom37 lash start 7067 hit 8392 lead 1325 hue 16 sat 0.88 parry no danger yes tell 200 kind red objects 46 obj x",
                "[boss63] cue roots wick: NIGHT37 THE WICK | at 0 ms, phase Reveal",
                "# a comment line",
            };
            // review S3: under the colour table's danger band (345-35) the spit's amber riser (hue 32) is danger, so it leaves the list
            CollectionAssert.AreEqual(new[] { "crown37:beamCut", "wyrm:aggro", "loom37:lash" }, FixFrom(lines, out int cues),
                "a red attack with a 39 deg cue is off-hue, no cue and a 200 ms tell are short; a parryable yellow, a danger cue and the 32 deg amber (danger in the table) are fine");
            Assert.AreEqual(6, cues, "six cue lines; the roots line and the comment skipped");
        }

        [Test]
        public void OnlyListedAttacks_NeverAParryableOne()
        {
            var fix = new[] { "wyrm:spit", "crown37:beamCut", "crown37:gemShard", "wyrm:aggro" };
            Assert.IsTrue(BossTell63.Wants("wyrm", "spit", "?", fix), "listed");
            Assert.IsTrue(BossTell63.Wants("crown37", "beamCut", "red", fix), "listed");
            Assert.IsFalse(BossTell63.Wants("wick", "wickSlam", "red", fix), "already fine: untouched (the slate)");
            Assert.IsFalse(BossTell63.Wants("crown37", "gemShard", "yellow", fix), "parryable: never, even if listed");
            Assert.IsFalse(BossTell63.Wants("wyrm", "aggro", "?", fix), "the wyrm's punch window: never, even if listed");
            Assert.IsFalse(BossTell63.Wants("wyrm", "spit", "?", null), "no list: nothing");
        }

        static Color Hex(uint hex, float gain) => new Color(((hex >> 16) & 255) / 255f * gain, ((hex >> 8) & 255) / 255f * gain, (hex & 255) / 255f * gain, 1f);

        [Test]
        public void TheBossesOwnWarmOffHueGlow_IsPulledToTheDangerHue_AtTheSameBrightness()
        {
            // review S3 / S7 (Logs/n63/review-n63.md): the pull takes only the hues between the colour table's danger band and its parry band
            // (35-45): the wyrm's amber (31.8) is already danger in the table, and the parry role (45-70) is never pulled (a punch or parry tell in
            // yellow stays yellow, NIGHT63.md). Before the review: the amber and the parry yellow were pulled (the meter's own 30 / 62 bands).
            foreach ((uint hex, float gain, string what) in new[] { (0xFFBB3Cu, 1.2f, "THE CROWN's point edge (39 deg)"), (0xFFAE30u, 1.0f, "an amber at 36.5 deg, just past the table's danger top"), (0xFFC23Cu, 1.0f, "41 deg") })
            {
                Color c = Hex(hex, gain);
                Assert.IsTrue(BossTell63.ToDanger(c, out Color d), what + " is pulled");
                Assert.AreEqual(BossCue63.Hue63.Danger, BossCue63.Class(d.r, d.g, d.b), what + " now in the danger hue: " + d);
                Assert.That(Mathf.Max(d.r, Mathf.Max(d.g, d.b)), Is.EqualTo(Mathf.Max(c.r, Mathf.Max(c.g, c.b))).Within(1e-4f), what + " at the same brightness");
                Assert.That(d.a, Is.EqualTo(c.a).Within(1e-6f));
            }
            foreach ((uint hex, float gain, string what) in new[] { (0xFF3A14u, 1f, "already danger"), (0xFF9A28u, 2.4f, "THE KILN WYRM's amber seams (31.8 deg: danger in the table)"),
                (0xFFD23Cu, 1f, "the parry yellow (46 deg: the parry role)"), (0xFFD23Au, 1f, "THE KILN WYRM's punch window #FFD23A (46 deg)"), (0xFFE14Au, 1f, "the loom's spool (50 deg)"),
                (0x36E0FFu, 1f, "the player's cyan"), (0xD8D0FFu, 1f, "the boundary's pale"), (0x101010u, 1f, "too dark") })
                Assert.IsFalse(BossTell63.ToDanger(Hex(hex, gain), out _), what + " is left alone");
        }

        [Test]
        public void TheCrownsWhiteHotClapper_IsPulledToTheDangerHue_APaleSurfaceOrAPaleYellowIsNot()
        {
            // B2d (n63-b9 b2p: "cue crown37 crownFire ... hue 41 sat 0.14 ... verdict off-hue ... obj THE CROWN/crown clapper", recoloured 0): the
            // clapper's fire colour, Crown47WhiteHex 0xFFF4DC (hue 41, sat 0.14) at gain 2.4 (Crown37View.Look47.cs), outshines the column
            Color clapper = Hex(UnityTuning.Crown47WhiteHex, 2.4f);
            Assert.IsTrue(BossTell63.ToDanger(clapper, out Color d), "the white-hot clapper on a listed attack is pulled");
            Assert.AreEqual(BossCue63.Hue63.Danger, BossCue63.Class(d.r, d.g, d.b), "now the meter reads it as danger: " + d);
            Assert.That(Mathf.Max(d.r, Mathf.Max(d.g, d.b)), Is.EqualTo(Mathf.Max(clapper.r, Mathf.Max(clapper.g, clapper.b))).Within(1e-4f), "at the same brightness");
            foreach ((uint hex, float gain, string what) in new[] {
                (UnityTuning.Crown47WhiteHex, 0.6f, "the same warm white as a surface, not hot"),
                (0xFFFFFFu, 2.4f, "a pure white-hot (no warm tint)"),
                (0xFFF0B0u, 2.0f, "a pale yellow (49 deg: the parry band)"),
                (0xD8D0FFu, 2.4f, "the boundary's pale violet-white, hot"),
                (0xBFEFFFu, 2.4f, "a pale cyan, hot") })
                Assert.IsFalse(BossTell63.ToDanger(Hex(hex, gain), out _), what + " is left alone");
        }

        [Test]
        public void TheCue_IsInTheDangerHue_AndTheMeterReadsItFromTheFirstStep()
        {
            for (int i = 0; i <= 10; i++)
            {
                Color c = BossTell63.Colour(BossTell63.Level(i / 10.0));
                Assert.AreEqual(BossCue63.Hue63.Danger, BossCue63.Class(c.r, c.g, c.b), "danger hue at " + i / 10.0 + ": " + c);
            }
            Color first = BossTell63.Colour(BossTell63.Level(0));
            Assert.That(Mathf.Max(first.r, Mathf.Max(first.g, first.b)), Is.GreaterThanOrEqualTo(UnityTuning.Boss63CueRiseAbs),
                "at the wind-up's first step the meter counts it as a riser over nothing");
            Assert.That(UnityTuning.BossTell63From, Is.GreaterThan(0f));
        }
    }
}
