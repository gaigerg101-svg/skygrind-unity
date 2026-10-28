// NIGHT64 builder D (slate B3, THE KILN WYRM's weak point in the player's cyan; NIGHT63's view row design.n63.bossColours63, OFF;
// WeakAim64Math.N64D.cs, BossColours63.N63D.cs, BossGrade54.RimBoss, Boss30View.cs). The red (n63-b11's same-run b3k stagger pair,
// `n63-b3-roles.py --hue 795,376,1121,684`): the OPEN head's median hue 158.5, RGB (8, 238, 157), against the wick's and crown's 180. RimAt
// pre-divides the grade's rim by the filter, but the head is drawn HDR (#0854FF x6.8, its shell x3.6): the screen clips green before THE WYRM's
// filter cuts blue to about 0.33 (linear), so green stays over blue. Code-scout option 1: the weak point's OWN rim colour, aimed bluer and
// drawn under the clip, in the bossColours63 table (design.n63.bossColours63.numbers.weakAim.<boss id>), measured in a Player (a tuning loop).
// Pinned:
//   1. the table: weakAim.wyrm's aim (sRGB, brightest channel 1) is bluer than the player's cyan (hue 200-240: aimed past 186 by the grade's
//      pull) and its scale is positive; every key is a graded boss's id (BossGrade54.Bosses)
//   2. the aim: a weak part's routed cyan -> the aim x (the routed colour's brightest channel x scale): its hue is the aim's, its brightness the
//      site's (dim while closed, bright when OPEN), scaled under the clip
//   3. RimAt's own input gate: only a player-cyan input (hue 160-215, S >= 0.3) is aimed; anything else, or no aim, passes unchanged
//   4. BossGrade54.RimBoss: RimAt's own gate (null with no graded fight), so the aim applies exactly where RimAt did
//   5. grade54's own numbers untouched (GG's NIGHT54 design): the wyrm's rim, rimGain, phase 1 filter and highlights
//   6. the sites: PartRim and ShellRim (wyrm.open, wyrm.punch, wyrm.core, wyrm.mark) go through the aim; the punch window's head glow at
//      Boss30View.cs (Tint(headR, punchC * pulse), never routed on NIGHT63) through PartRim "wyrm.punch"
// Written test first (NIGHT64.md section 4): RED on its commit (WeakAim64Math's placeholders answer nothing; the table is not in config yet).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using A = Skygrind.Game.WeakAim64Math;

namespace Skygrind.Game.Tests
{
    public class N64WeakAimTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        static string Src(string file) => File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", file));

        [Test]
        public void Table_TheWyrmsAim_BluerThanThePlayersCyan_KeysAreGradedBosses()
        {
            Dictionary<string, A.Aim> aims = A.From(Cfg());
            Assert.That(aims, Is.Not.Null, "design.n63.bossColours63.numbers.weakAim reads as a table");
            Assert.That(aims.ContainsKey("wyrm"), Is.True, "THE KILN WYRM's aim");
            A.Aim w = aims["wyrm"];
            Assert.That(Math.Max(w.r, Math.Max(w.g, w.b)), Is.EqualTo(1.0).Within(1e-9), "the aim's brightest channel is 1 (its brightness is the site's x scale)");
            Colours63Math.Hsv(w.r, w.g, w.b, out double h, out double s, out _);
            Assert.That(h, Is.InRange(200.0, 240.0), "aimed bluer than the player's cyan (186), past it by the grade's measured pull");
            Assert.That(s, Is.GreaterThan(0.5));
            Assert.That(w.scale, Is.GreaterThan(0.0).And.LessThanOrEqualTo(1.5));
            foreach (string id in aims.Keys) Assert.That(Array.IndexOf(BossGrade54.Bosses, id), Is.GreaterThanOrEqualTo(0), id + " is a graded boss's id");
        }

        // --- NIGHT64 B3 round 2 (Logs/n64/verdict-B3.md: on b2 the bright cube read ~190 and the dim translucent box (wyrm.mark) ~222, navy, most
        //     of the head's area: one aim per boss cannot put both in 175-200): an aim per part ---

        [Test]
        public void Parts_TheBoxHasItsOwnAim_GreenerAndBrighterThanTheCubes()
        {
            Dictionary<string, A.Aim> aims = A.From(Cfg());
            Assert.That(aims, Is.Not.Null);
            A.Aim w = aims["wyrm"];
            A.Aim box = w.For("wyrm.mark"), cube = w.For("wyrm.open");
            Assert.That(box, Is.Not.SameAs(cube), "the open-window box (wyrm.mark) reads its own aim");
            Assert.That(cube, Is.SameAs(w), "the cube (wyrm.open) keeps the boss's aim");
            Assert.That(w.For("wyrm.punch"), Is.SameAs(w), "a part with no entry: the boss's aim");
            Assert.That(box.g / box.b, Is.GreaterThan(w.g / w.b), "the box greener than the boss's aim (it read 220 deg, navy, at the boss's aim)");
            Assert.That(box.scale, Is.GreaterThan(w.scale), "the box brighter (it read V 0.45)");
            Assert.That(Math.Max(box.r, Math.Max(box.g, box.b)), Is.EqualTo(1.0).Within(1e-9));
            var parts = BossColours63Math.From(Cfg());
            foreach (string p in w.PartNames) Assert.That(parts.RoleOf(p), Is.EqualTo("weak"), p + " is a weak part of the bossColours63 table");
        }

        [Test]
        public void For_APartsOwnAim_ElseTheBosss()
        {
            var w = new A.Aim { r = 0, g = 0.3, b = 1, scale = 0.6 };
            var box = new A.Aim { r = 0, g = 0.42, b = 1, scale = 1.0 };
            w.SetPart("wyrm.mark", box);
            Assert.That(w.For("wyrm.mark"), Is.SameAs(box));
            Assert.That(w.For("wyrm.open"), Is.SameAs(w));
            Assert.That(w.For(null), Is.SameAs(w));
            Assert.That(A.At(w.For("wyrm.mark"), 0.1, 0.92, 1.0, out double r, out double g, out double b), Is.True);
            Assert.That(new[] { r, g, b }, Is.EqualTo(new[] { 0.0, 0.42 * 1.0, 1.0 }).Within(1e-12), "the box: its aim x its scale");
        }

        [Test]
        public void At_TheAimTimesTheSitesBrightness()
        {
            var a = new A.Aim { r = 0, g = 0.22, b = 1, scale = 0.9 };
            bool did = A.At(a, 0.1 * 2.2, 0.92 * 2.2, 1.0 * 2.2, out double r, out double g, out double b);
            Assert.That(did, Is.True);
            Assert.That(new[] { r, g, b }, Is.EqualTo(new[] { 0.0, 0.22 * 2.2 * 0.9, 2.2 * 0.9 }).Within(1e-12), "aim x (2.2 x 0.9)");
            did = A.At(a, 0.1 * 0.4, 0.92 * 0.4, 0.4, out r, out g, out b);
            Assert.That(b, Is.EqualTo(0.4 * 0.9).Within(1e-12), "dim while closed: the site's own brightness");
            Colours63Math.Hsv(r, g, b, out double h, out _, out _);
            Colours63Math.Hsv(a.r, a.g, a.b, out double ha, out _, out _);
            Assert.That(h, Is.EqualTo(ha).Within(1e-9), "the aim's hue");
        }

        [Test]
        public void At_OnlyAPlayerCyanInput_NoAimUnchanged()
        {
            var a = new A.Aim { r = 0, g = 0.22, b = 1, scale = 0.9 };
            Assert.That(A.At(a, 1, 0.29, 0.11, out double r, out double g, out double b), Is.False, "danger red-orange passes");
            Assert.That(new[] { r, g, b }, Is.EqualTo(new[] { 1, 0.29, 0.11 }));
            Assert.That(A.At(null, 0.1, 0.83, 0.9, out r, out g, out b), Is.False, "no aim: unchanged");
            Assert.That(new[] { r, g, b }, Is.EqualTo(new[] { 0.1, 0.83, 0.9 }));
            Assert.That(A.IsCyan(0.1, 0.83, 0.9), Is.True, "the player's cyan (RimAt's gate: hue 160-215, S >= 0.3)");
            Assert.That(A.IsCyan(0.95, 0.95, 0.95), Is.False, "white-hot: S under 0.3");
            Assert.That(A.IsCyan(0.2, 0.9, 0.55), Is.False, "hue 150: under 160");
            Assert.That(A.IsCyan(0.2, 0.4, 0.95), Is.False, "hue 222: over 215");
        }

        [Test]
        public void RimBoss_IsRimAtsGate_NullWithNoGradedFight()
        {
            Assert.That(BossGrade54.RimBoss(), Is.Null, "no graded fight in an EditMode test");
            var c = new Color(0.1f, 0.83f, 0.9f, 1f);
            Assert.That(BossGrade54.RimAt(c), Is.EqualTo(c), "RimAt passes it unchanged too");
        }

        [Test]
        public void Grade54sOwnNumbers_Untouched()
        {
            GameConfig c = Cfg();
            const string W = "design.n54.grade54.numbers.wyrm.";
            Assert.That(PortData.Arr(c.Get(W + "rim")).ConvertAll(o => PortData.Num(o)), Is.EqualTo(new List<double> { 0.08, 0.86, 1.0 }), "the wyrm's rim (GG's NIGHT54 grade)");
            Assert.That(c.Num(W + "rimGain"), Is.EqualTo(2.2).Within(1e-12));
            Assert.That(PortData.Arr(c.Get(W + "p1.filter")).ConvertAll(o => PortData.Num(o)), Is.EqualTo(new List<double> { 0.95, 0.96, 0.78 }));
            Assert.That(PortData.Arr(c.Get(W + "p1.highlights")).ConvertAll(o => PortData.Num(o)), Is.EqualTo(new List<double> { 0.54, 0.76, 0.3 }));
        }

        [Test]
        public void Sites_GoThroughTheAim_ThePunchWindowsHeadRouted()
        {
            string bc = Src("BossColours63.N63D.cs");
            StringAssert.Contains("WeakAim64Math.At(", bc, "the weak path reads the aim");
            // NIGHT64 B3 round 2 (Logs/n64/verdict-B3.md: one aim for the cube and the box): the two pins restated: the part is passed, so each
            // part reads its own aim (was Weak(Route(role, c)))
            StringAssert.Contains("Color o = Weak(part, Route(role, c));", bc, "PartRim (wyrm.open, wyrm.punch, wyrm.core) through the part's aim");
            StringAssert.Contains("Color k = Weak(part, Route(role, c));", bc, "ShellRim (wyrm.mark) through the part's aim");
            StringAssert.Contains(".For(part)", bc, "the part's own aim, else the boss's");
            StringAssert.Contains("Tint(headR, BossColours63.PartRim(game.cfg, \"wyrm.punch\", punchC * pulse));", Src("Boss30View.cs"),
                "the punch window's head glow routed with the head (never routed on NIGHT63: code-scout option 5)");
        }
    }
}
