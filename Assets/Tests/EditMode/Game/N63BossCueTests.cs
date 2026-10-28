// NIGHT63 builder G (slate B2's measurement; Game/BossCue63.cs, the Unity half FeelReel.N63G.cs): the pure cue measure. Pinned:
//   1. the hue classes on the palette's own colours: Tell36RedHex 0xFF3A14 and the bosses' 0xFF5A1F danger, Tell36YellowHex 0xFFD23C parry,
//      the player's cyan 0x36E0FF neither, a pale 0xD8D0FF pale
//   2. a cue drawn from 600 ms before the hit (an object off before the wind-up, red from its first frame): start, lead 600 ms (one frame's
//      slack), danger yes, parry no; not short
//   3. an object already pulsing before the wind-up (the reference window's own maximum) is no cue while it keeps pulsing at that level
//   4. a parry-yellow tell: parry yes, danger no, its hue in the parry band
//   5. a cue only 200 ms before the hit: short (under 300 ms); no cue at all: not found, short
//   6. a red object brightening through the wind-up: found when it passes its reference by the rise threshold
//   7. the lines: the cue line names the boss, attack, start, hit, lead, hue, sat, parry, danger; the end line counts the short and off-hue ids
//   8. (review S3, Logs/n63/review-n63.md) ONE band set: the meter's danger and parry bands are the colour table's
//      (design.n63.colours63.numbers.bands through Colours63Math.From), so B2's verdict, C1's role and B3's table call a pixel the same thing:
//      the wyrm's amber 31.8 deg is danger (345-35), a yellow at 66 deg parry (45-70), 39 deg between; at every hue the meter's class and
//      Colours63Math.RoleOf agree
// Written test-first: RED on its own commit (the placeholder answers nothing). Not run in Unity by its author.
using System.Collections.Generic;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    /// <summary>Review S3: the cue meter reads the colour table's bands; every Game test in this namespace runs with the repo config's table
    /// in use (as the Player does: the reel and bossTell63 hand BossCue63 the game's config).</summary>
    [SetUpFixture]
    public sealed class N63BandsBaseline
    {
        [OneTimeSetUp]
        public void Up() => BossCue63.Use(N63BossCueTests.Cfg());
    }

    public class N63BossCueTests
    {
        internal static Skygrind.Core.GameConfig Cfg()
        {
            string root;
            try { root = System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { root = System.IO.Path.GetFullPath("PortExport"); }
            return Skygrind.Core.GameConfig.Load(System.IO.Path.Combine(root, "config.json"));
        }

        static double Hue(uint hex)
        {
            BossCue63.Hsv(((hex >> 16) & 255) / 255.0, ((hex >> 8) & 255) / 255.0, (hex & 255) / 255.0, out double h, out _, out _);
            return h;
        }

        static BossCue63.Hue63 ClassOf(uint hex) => BossCue63.Class(((hex >> 16) & 255) / 255f, ((hex >> 8) & 255) / 255f, (hex & 255) / 255f);

        [Test]
        public void TheBands_AreTheColourTables_OneSetForTheMeterTheTellAndC1()
        {
            Skygrind.Core.GameConfig cfg = Cfg();
            Colours63Math.Table t = Colours63Math.From(cfg);
            Assert.IsNotNull(t, "the colour table (design.n63.colours63.numbers)");
            BossCue63.Bands63 b = BossCue63.Use(cfg);
            Assert.IsTrue(b.ok, "the bands read");
            Assert.AreEqual(t.dangerLo, b.dangerLo, "danger lo");
            Assert.AreEqual(t.dangerHi, b.dangerHi, "danger hi");
            Assert.AreEqual(t.parryLo, b.parryLo, "parry lo");
            Assert.AreEqual(t.parryHi, b.parryHi, "parry hi");
            Assert.AreEqual(b.dangerHi, BossCue63.Bands.dangerHi, "the bands in use");
            Assert.That(Hue(0xFF9A28), Is.EqualTo(31.8).Within(0.1), "the wyrm's amber seams");
            Assert.AreEqual(BossCue63.Hue63.Danger, ClassOf(0xFF9A28), "the wyrm's amber 31.8 deg: danger in the table (345-35), as C1 counts it");
            Assert.That(Hue(0xDFF040), Is.EqualTo(65.8).Within(0.2));
            Assert.AreEqual(BossCue63.Hue63.Parry, ClassOf(0xDFF040), "a yellow at 66 deg: parry in the table (45-70)");
            Assert.AreEqual(BossCue63.Hue63.Other, ClassOf(0xFFBB3C), "39 deg: between the danger and the parry bands");
            for (int i = 0; i < 360; i++)
            {
                // a saturated, bright colour at hue h (half degrees: never on a band's edge, where float and double round apart): the meter's
                // class and C1's role agree on danger and parry
                double h = i + 0.5, x = 1 - System.Math.Abs((h / 60.0) % 2 - 1);
                double r, g, bb;
                if (h < 60) { r = 1; g = x; bb = 0; } else if (h < 120) { r = x; g = 1; bb = 0; } else if (h < 180) { r = 0; g = 1; bb = x; }
                else if (h < 240) { r = 0; g = x; bb = 1; } else if (h < 300) { r = x; g = 0; bb = 1; } else { r = 1; g = 0; bb = x; }
                r = 0.1 + 0.9 * r; g = 0.1 + 0.9 * g; bb = 0.1 + 0.9 * bb; // saturation 0.9
                BossCue63.Hue63 k = BossCue63.Class((float)r, (float)g, (float)bb);
                string role = Colours63Math.RoleOf(t, r, g, bb);
                Assert.AreEqual(role == "danger", k == BossCue63.Hue63.Danger, "danger at hue " + h + " (role " + role + ", class " + k + ")");
                Assert.AreEqual(role == "parry", k == BossCue63.Hue63.Parry, "parry at hue " + h + " (role " + role + ", class " + k + ")");
            }
        }

        const double Frame = 1000.0 / 60;

        static Lit63 L(int id, bool drawn, uint hex, float gain = 1f) =>
            new Lit63 { id = id, drawn = drawn, r = ((hex >> 16) & 255) / 255f * gain, g = ((hex >> 8) & 255) / 255f * gain, b = (hex & 255) / 255f * gain };

        /// <summary>Frames at 60 fps from `fromMs` to `toMs`; each frame's things from `at(ms)`.</summary>
        static void Make(double fromMs, double toMs, System.Func<double, Lit63[]> at, List<double> ms, List<Lit63[]> frames)
        {
            for (double t = fromMs; t <= toMs + 1e-9; t += Frame) { ms.Add(t); frames.Add(at(t)); }
        }

        [Test]
        public void HueClasses_OnThePalettesOwnColours()
        {
            Assert.AreEqual(BossCue63.Hue63.Danger, BossCue63.Class(1f, 58 / 255f, 20 / 255f), "Tell36RedHex 0xFF3A14");
            Assert.AreEqual(BossCue63.Hue63.Danger, BossCue63.Class(1f, 90 / 255f, 31 / 255f), "the bosses' 0xFF5A1F");
            Assert.AreEqual(BossCue63.Hue63.Parry, BossCue63.Class(1f, 210 / 255f, 60 / 255f), "Tell36YellowHex 0xFFD23C");
            Assert.AreEqual(BossCue63.Hue63.Other, BossCue63.Class(54 / 255f, 224 / 255f, 1f), "the player's cyan 0x36E0FF");
            Assert.AreEqual(BossCue63.Hue63.Pale, BossCue63.Class(216 / 255f, 208 / 255f, 1f), "the boundary's pale 0xD8D0FF");
            Assert.AreEqual(BossCue63.Hue63.None, BossCue63.Class(0.02f, 0.01f, 0.01f), "too dark to read");
            BossCue63.Hsv(1, 58 / 255.0, 20 / 255.0, out double h, out double s, out double v);
            Assert.That(h, Is.EqualTo(9.7).Within(0.2), "0xFF3A14's hue");
            Assert.That(s, Is.EqualTo(1 - 20 / 255.0).Within(1e-6));
            Assert.That(v, Is.EqualTo(1.0).Within(1e-9));
            Assert.IsTrue(BossCue63.DangerHue(h, s));
            Assert.IsFalse(BossCue63.ParryHue(h, s));
        }

        [Test]
        public void ACueFrom600msBeforeTheHit_LeadAndDangerHue()
        {
            double windup = 1000, hit = 2000, cueAt = 1400;
            var ms = new List<double>();
            var frames = new List<Lit63[]>();
            Make(500, hit, t => new[] { L(1, true, 0x4a3f48), L(2, t >= cueAt, 0xFF3A14, 1.5f) }, ms, frames);
            Cue63 c = BossCue63.Analyse(ms, frames, windup, hit);
            Assert.IsTrue(c.found, "found");
            Assert.That(c.startMs, Is.InRange(cueAt - 1e-6, cueAt + Frame + 1e-6), "the cue's first drawn frame");
            Assert.That(c.leadMs, Is.InRange(hit - cueAt - Frame - 1e-6, hit - cueAt + 1e-6), "lead ~600 ms");
            Assert.AreEqual(2, c.strongest, "the red object");
            Assert.IsTrue(c.danger, "danger hue");
            Assert.IsFalse(c.parry, "no parry yellow");
            Assert.That(c.hue, Is.EqualTo(9.7).Within(0.5));
            Assert.IsFalse(BossCue63.Short(c), "600 ms is not short");
        }

        [Test]
        public void AnObjectPulsingBeforeTheWindup_IsNoCueAtItsOwnLevel()
        {
            double windup = 1000, hit = 2000;
            var ms = new List<double>();
            var frames = new List<Lit63[]>();
            // a ~100 ms pulse: the 250 ms reference window holds two and a half of them
            Make(500, hit, t => new[] { L(5, true, 0xFF5A1F, 0.3f + 0.6f * (float)(0.5 + 0.5 * System.Math.Sin(t / 16.0))) }, ms, frames);
            Cue63 c = BossCue63.Analyse(ms, frames, windup, hit);
            Assert.IsFalse(c.found, "a lamp pulsing as before is not the attack's cue");
            Assert.IsTrue(BossCue63.Short(c), "no cue counts as short");
        }

        [Test]
        public void AParryYellowTell_ParryYes_DangerNo()
        {
            double windup = 1000, hit = 1800;
            var ms = new List<double>();
            var frames = new List<Lit63[]>();
            Make(500, hit, t => new[] { L(7, t >= windup, 0xFFD23C, 1.2f) }, ms, frames);
            Cue63 c = BossCue63.Analyse(ms, frames, windup, hit);
            Assert.IsTrue(c.found);
            Assert.IsTrue(c.parry, "the parry tell drawn");
            Assert.IsFalse(c.danger, "yellow is not the danger hue");
            Assert.IsTrue(BossCue63.ParryHue(c.hue, c.sat), "its hue in the parry band: " + c.hue);
            Assert.That(c.leadMs, Is.InRange(800 - Frame - 1e-6, 800 + 1e-6));
        }

        [Test]
        public void ACueOnly200msBeforeTheHit_IsShort_NoCueIsShort()
        {
            double windup = 1000, hit = 2000;
            var ms = new List<double>();
            var frames = new List<Lit63[]>();
            Make(500, hit, t => new[] { L(3, t >= 1800, 0xFF3A14) }, ms, frames);
            Cue63 c = BossCue63.Analyse(ms, frames, windup, hit);
            Assert.IsTrue(c.found);
            Assert.That(c.leadMs, Is.LessThan(UnityTuning.Boss63CueLeadMinMs), "200 ms");
            Assert.IsTrue(BossCue63.Short(c), "under 300 ms");
            var ms2 = new List<double>();
            var frames2 = new List<Lit63[]>();
            Make(500, hit, t => new[] { L(3, true, 0x3a3f4a) }, ms2, frames2);
            Cue63 none = BossCue63.Analyse(ms2, frames2, windup, hit);
            Assert.IsFalse(none.found, "nothing rose");
            Assert.AreEqual(0, none.leadMs);
            Assert.IsTrue(BossCue63.Short(none));
        }

        [Test]
        public void ARedObjectBrightening_FoundWhenItPassesItsReference()
        {
            double windup = 1000, hit = 2000;
            var ms = new List<double>();
            var frames = new List<Lit63[]>();
            // 0.3 before the wind-up, then +0.9 a second through it: it passes 0.3 + 0.15 at ~167 ms after the wind-up's start
            Make(500, hit, t => new[] { L(4, true, 0xFF3A14, t < windup ? 0.3f : 0.3f + 0.9f * (float)((t - windup) / 1000)) }, ms, frames);
            Cue63 c = BossCue63.Analyse(ms, frames, windup, hit);
            Assert.IsTrue(c.found);
            Assert.That(c.startMs, Is.InRange(windup + 150, windup + 200), "when it passes its reference by the rise threshold");
            Assert.IsTrue(c.danger);
        }

        /// <summary>Added with the implementation (not on the RED commit): the tell is the first danger- or parry-hue riser; an earlier riser of
        /// another colour (a cyan flash 800 ms out) does not make a red cue 200 ms out long enough.</summary>
        [Test]
        public void AnEarlyOffPaletteRiser_DoesNotHideALateRedTell()
        {
            double windup = 1000, hit = 2000;
            var ms = new List<double>();
            var frames = new List<Lit63[]>();
            Make(500, hit, t => new[] { L(8, t >= 1200, 0x36E0FF), L(9, t >= 1800, 0xFF3A14, 2f) }, ms, frames);
            Cue63 c = BossCue63.Analyse(ms, frames, windup, hit);
            Assert.IsTrue(c.found);
            Assert.That(c.leadMs, Is.InRange(800 - Frame - 1e-6, 800 + 1e-6), "something visible from 800 ms");
            Assert.IsTrue(c.tellFound);
            Assert.That(c.tellLeadMs, Is.InRange(200 - Frame - 1e-6, 200 + 1e-6), "the red tell only 200 ms out");
            Assert.IsTrue(BossCue63.Short(c), "short by its tell");
            StringAssert.Contains(" tell " + System.Math.Round(c.tellLeadMs).ToString("0", System.Globalization.CultureInfo.InvariantCulture) + " kind red", BossCue63.CueLine("wick", "ember", c, "red", null));
        }

        /// <summary>Added after n63-b2's red (every cue read "start none" while its tell column read 1008-1625 ms): Unity's instance ids of objects
        /// made at run time are negative, and the analysis took a negative strongest id for "no riser". A riser with a negative id is a riser.</summary>
        [Test]
        public void ARiserWithANegativeInstanceId_IsFound()
        {
            double windup = 1000, hit = 2000;
            var ms = new List<double>();
            var frames = new List<Lit63[]>();
            Make(500, hit, t => new[] { L(-41872, t >= windup, 0xFF3A14, 1.5f) }, ms, frames);
            Cue63 c = BossCue63.Analyse(ms, frames, windup, hit);
            Assert.IsTrue(c.found, "found");
            Assert.AreEqual(-41872, c.strongest, "the strongest riser is the runtime object");
            Assert.IsTrue(c.danger, "its hue read");
            Assert.That(c.leadMs, Is.InRange(1000 - Frame - 1e-6, 1000 + 1e-6));
            StringAssert.Contains("danger yes", BossCue63.CueLine("wick", "wickSlam", c, "red", "NIGHT37 THE WICK/glow"));
        }

        // ---- NIGHT63 G B2b (n63-b4's before lines, Logs/player/n47-reel-n63-b4-b63?B.log): the verdict by the sim's tell kind, the reference from the
        // previous hit. Added test-first (RED on its own commit: the placeholder keeps n63-b2's verdict and the 250 ms reference) ----

        /// <summary>THE CROWN's beamCut / lowFire (kind red): "hue 39 ... parry yes danger no obj crownLook47/crown point edge": a parry-yellow cue on
        /// an attack that cannot be parried is off the danger hue (DECIDE63 D-1: yellow is the parry tell's only); the end line read 0 because a
        /// yellow riser counted as parryable. Parryable is the sim's kind.</summary>
        [Test]
        public void ARedAttackWithAParryYellowCue_IsOffTheDangerHue()
        {
            double windup = 1000, hit = 2400;
            var ms = new List<double>();
            var frames = new List<Lit63[]>();
            Make(500, hit, t => new[] { L(11, t >= windup, 0xFFD23C, 1.2f) }, ms, frames); // hue 46: the colour table's parry band (45-70; review S3: was 0xFFC23C, 41 deg, inside the meter's old 38-62)
            Cue63 c = BossCue63.Analyse(ms, frames, windup, hit);
            Assert.IsTrue(c.found && c.parry && !c.danger, "a parry-yellow cue");
            Assert.AreEqual(BossCue63.Verdict63.OffHue, BossCue63.Verdict(c, "red"), "a dodge-only attack showing yellow");
            Assert.AreEqual(BossCue63.Verdict63.OffHue, BossCue63.Verdict(c, "?"), "no kind in the sim (the wyrm): not parryable");
            Assert.AreEqual(BossCue63.Verdict63.Ok, BossCue63.Verdict(c, "yellow"), "a parryable attack's yellow tell is right");
            Cue63 none = BossCue63.Analyse(new List<double>(), new List<Lit63[]>(), windup, hit);
            Assert.AreEqual(BossCue63.Verdict63.Short, BossCue63.Verdict(none, "red"), "no cue: short");
        }

        /// <summary>THE KILN WYRM's spit: "obj KILN WYRM/trim 9" hue 32 is the molten seam wave (WyrmView: Wyrm47AmberHex on a ~2.1 s sine, always
        /// on): a 250 ms reference cannot hold it, so its peak inside a wind-up read as the cue. With the reference from the previous hit (at
        /// most Boss63CueRefLongMs) a 2.1 s ambient wave is no cue.</summary>
        [Test]
        public void ATwoSecondAmbientWave_IsNoCue_WithTheReferenceFromThePreviousHit()
        {
            // the wave's trough sits in the short reference (3550-3783 ms), its peak (4725 ms) in the wind-up; the long reference holds the
            // previous peak (2625 ms)
            double windup = 3800, hit = 4800;
            var ms = new List<double>();
            var frames = new List<Lit63[]>();
            Make(0, hit, t => new[] { L(12, true, 0xFF9A28, 0.6f + 1.8f * (float)(0.5 + 0.5 * System.Math.Sin(2 * System.Math.PI * t / 2100.0))) }, ms, frames);
            Cue63 longRef = BossCue63.Analyse(ms, frames, windup, hit, double.NegativeInfinity);
            Assert.IsFalse(longRef.found, "the wave already reached its peak within the reference");
            Cue63 shortRef = BossCue63.Analyse(ms, frames, windup, hit);
            Assert.IsTrue(shortRef.found, "the short reference reads the wave as a cue (n63-b4's spit)");
        }

        /// <summary>A cue re-lit on the same object after the previous attack's hit is still a cue: the reference starts at that hit.</summary>
        [Test]
        public void ACueReLitAfterThePreviousHit_IsStillACue()
        {
            double prevHit = 1800, windup = 2800, hit = 3800;
            var ms = new List<double>();
            var frames = new List<Lit63[]>();
            Make(0, hit, t => new[] { L(13, (t >= 1000 && t < prevHit) || t >= windup, 0xFF3A14, 1.5f) }, ms, frames);
            Cue63 c = BossCue63.Analyse(ms, frames, windup, hit, prevHit);
            Assert.IsTrue(c.found, "re-lit for this attack");
            Assert.That(c.leadMs, Is.InRange(1000 - Frame - 1e-6, 1000 + 1e-6));
            Assert.That(UnityTuning.Boss63CueRefLongMs, Is.GreaterThanOrEqualTo(2100), "the reference holds one molten-wave cycle");
        }

        /// <summary>B2e (the lead, n63-b9: crownFire's cue line reads its strongest riser, the white-hot clapper (hue 41, saturation 0.14: off-hue),
        /// and never the danger column, though the column's pair changes 766,631 px): the meter also names the EARLIEST danger-hue cue, its own
        /// start, lead, hue and object, so doneWhen 2's "a danger-hue cue >= 300 ms before the damage frame" is read, not inferred; the verdict is
        /// unchanged (still the strongest riser's).</summary>
        [Test]
        public void TheEarliestDangerCue_HasItsOwnStartLeadHueAndObject_TheVerdictUnchanged()
        {
            double windup = 1000, hit = 2300;
            var ms = new List<double>();
            var frames = new List<Lit63[]>();
            // a white-hot pale clapper (#FFF4DC x 2.4: hue 41, saturation 0.14) and a red column (Tell36Red x 0.3), both from the wind-up
            Make(400, hit, t => new[] { L(1, t >= windup, 0xFFF4DC, 2.4f), L(2, t >= windup, 0xFF3A14, 0.3f) }, ms, frames);
            Cue63 c = BossCue63.Analyse(ms, frames, windup, hit);
            Assert.IsTrue(c.found);
            Assert.AreEqual(1, c.strongest, "the clapper is the strongest riser");
            Assert.IsFalse(c.danger, "its hue is not danger");
            Assert.AreEqual(BossCue63.Verdict63.OffHue, BossCue63.Verdict(c, "red"), "the verdict is the strongest riser's, unchanged");
            Assert.IsTrue(c.dangerCueFound, "a danger-hue cue rose");
            Assert.AreEqual(2, c.dangerCueId, "the column");
            Assert.That(c.dangerCueStartMs, Is.InRange(windup - Frame, windup + Frame), "from the wind-up");
            Assert.That(c.dangerCueLeadMs, Is.EqualTo(hit - c.dangerCueStartMs).Within(1e-9));
            Assert.That(c.dangerCueHue, Is.EqualTo(9.7).Within(0.2));
            string line = BossCue63.CueLine("crown37", "crownFire", c, "red", "THE CROWN/crown clapper", "NIGHT37 CROWN/tell63 cue");
            StringAssert.Contains("verdict off-hue", line);
            StringAssert.Contains(" | danger cue start " + System.Math.Round(c.dangerCueStartMs).ToString("0", System.Globalization.CultureInfo.InvariantCulture)
                + " lead " + System.Math.Round(c.dangerCueLeadMs).ToString("0", System.Globalization.CultureInfo.InvariantCulture) + " hue 10 obj NIGHT37 CROWN/tell63 cue", line);
            var ms2 = new List<double>();
            var frames2 = new List<Lit63[]>();
            Make(400, hit, t => new[] { L(1, t >= windup, 0xFFF4DC, 2.4f) }, ms2, frames2);
            Cue63 none = BossCue63.Analyse(ms2, frames2, windup, hit);
            Assert.IsFalse(none.dangerCueFound, "no danger-hue riser");
            StringAssert.Contains(" | danger cue none", BossCue63.CueLine("crown37", "crownFire", none, "red", "THE CROWN/crown clapper"));
        }

        [Test]
        public void TheLines_NameEveryField()
        {
            var c = new Cue63 { found = true, startMs = 1400, hitMs = 2000, leadMs = 600, hue = 9.7, sat = 0.92, danger = true, parry = false, objects = 2, strongest = 2 };
            string line = BossCue63.CueLine("wick", "ember", c, "red", "Fist R");
            StringAssert.StartsWith("[boss63] cue wick ember start 1400 hit 2000 lead 600 hue 10 sat 0.92 parry no", line);
            StringAssert.Contains("danger yes", line);
            StringAssert.Contains("kind red", line);
            StringAssert.Contains("Fist R", line);
            string none = BossCue63.CueLine("wyrm", "lunge", new Cue63 { hitMs = 3000 }, "?", null);
            StringAssert.StartsWith("[boss63] cue wyrm lunge start none hit 3000 lead 0 hue - sat - parry no", none);
            string end = BossCue63.EndLine("wick", 5, 4, new List<string> { "lunge" }, new List<string> { "sweep" }, 1, 1);
            StringAssert.StartsWith("[boss63] cues wick attacks 5 found 4", end);
            StringAssert.Contains("under 300 ms 1 (lunge)", end);
            StringAssert.Contains("off the danger hue 1 (sweep)", end);
            StringAssert.Contains("parry drawn 1 of 1 yellow", end);
        }
    }
}
