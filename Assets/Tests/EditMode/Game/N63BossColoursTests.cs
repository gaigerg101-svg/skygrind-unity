// NIGHT63 builder D (slate B3, BOSS COLOURS; view row design.n63.bossColours63, OFF; BossColours63Math.N63D.cs, BossColours63.N63D.cs; it reads
// C1's colour table, design.n63.colours63.numbers). Pinned:
//   1. the parts table is the row's numbers: every named boss part goes to "weak" (the weak point: the player's cyan, CLAUDE.md rule 8 / V-2: the
//      thing the player acts on) or "danger" (an attack, an ember, a flame: red-orange), and nothing else (never yellow: DECIDE63 D-1 keeps it
//      for the parry tell; never magenta: RIVAL only); the five bosses each have a weak part
//   2. the parts the slate names: THE WICK's idle core, THE CROWN's gems, THE LOOM's knees, THE KILN WYRM's punch mark and THE ICEBREAKER's stuck
//      fists are weak; THE WICK's ember edges and THE CROWN's gold clap / edges / flame yellow are danger; no parry tell is in the table
//   3. a routed colour: the role's own colour (C1's table) at the original's luminance, so a closed weak point stays dim and an open one bright
//   4. on every boss the weak parts end in ONE role (the player's cyan) and the danger parts in the danger band
//   5. the switch: off with the row off, under -skygrindOff63 (or older) and -skygrindNoBossColours63; read every frame (NightRows.Live)
//   6. NIGHT63 B3b (the lead: on n63-b5's b3k the wyrm's same-run pairs changed 0 / 0 / 0 / 6 px: its one routed part, the punch window's head,
//      is on no boss63 shot): THE KILN WYRM's weak point is lit while it is OPEN (staggered, or its core exposed: the HUD's "OPEN") as the punch
//      window's head is, in the weak role (wyrm.open); its exposed core is weak (wyrm.core: red-orange today on the thing you shoot); its amber
//      seams and its spit / shards are danger (wyrm.seams, wyrm.spit). The hooks in WyrmView.cs, and the lit head as bright as the punch window
// Written test first (NIGHT63.md section 4): RED on its commit (BossColours63Math's placeholders answer nothing; the row's numbers are empty).
// B3b's tests (6) were written first too: RED on their commit (the four wyrm parts are not in the numbers, WyrmView has no hook, no Lit wrapper).
//   7. NIGHT63 B3c (B3's GATE FAIL, Logs/n63/verdict-B3.md; one fix round): (1) THE CROWN's red (dodge-only) tell, its ring edges and its
//      clapper read amber on screen because the views multiply the routed colour by an HDR gain and the screen clips each channel (a red with
//      green 0.23 at x2.8 shows green 0.64: hue ~38): a danger route is CLIP-SAFE (its brightest channel keeps the gain for the glow, the
//      others stop at the role's own share of it, so the clipped colour is the danger hue), applied after the site's gain (Hot); the parry
//      yellow is never routed; (2) THE KILN WYRM's open head and BossMarks32's open-window shell over it go through the fight's grade rim
//      (BossGrade54.RimAt: the wyrm's grade cuts blue to 0.78 and tones the highlights sulphur, so a plain cyan showed lime); (3) the proof
//      line prints, per part, the colour the shot's frame used (the last call that frame), not the first call ever. Written test first.
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using B = Skygrind.Game.BossColours63Math;
using C = Skygrind.Game.Colours63Math;

namespace Skygrind.Game.Tests
{
    public class N63BossColoursTests
    {
        static readonly string[] Weak = { "wick.core", "crown.gems", "crown.halo", "loom.knees", "loom.seams", "loom.nails", "wyrm.punch", "brawl.weak", "wyrm.open", "wyrm.core" };
        static readonly string[] Danger = { "wick.edges", "crown.clap", "crown.edge", "crown.flameYellow", "wyrm.seams", "wyrm.spit" };

        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        static double[] Hex(uint x) => new[] { ((x >> 16) & 255) / 255.0, ((x >> 8) & 255) / 255.0, (x & 255) / 255.0 };

        static double Luma(double r, double g, double b) => 0.2126 * r + 0.7152 * g + 0.0722 * b;

        [Test]
        public void Parts_TheRowsNumbers_WeakOrDangerOnly_EveryBossHasAWeakPart()
        {
            B.Parts p = B.From(Cfg());
            Assert.That(p, Is.Not.Null, "design.n63.bossColours63.numbers reads as a parts table");
            Assert.That(p.Count, Is.GreaterThan(0));
            foreach (string part in p.Names) Assert.That(p.RoleOf(part), Is.EqualTo("weak").Or.EqualTo("danger"), part + ": weak or danger only");
            foreach (string boss in new[] { "wick", "crown", "loom", "wyrm", "brawl" })
            {
                bool any = false;
                foreach (string part in p.Names) if (part.StartsWith(boss + ".", StringComparison.Ordinal) && p.RoleOf(part) == "weak") any = true;
                Assert.That(any, Is.True, boss + " has a weak part");
            }
            Assert.That(p.RoleOf("nothing.here"), Is.Null);
        }

        [Test]
        public void TheSlatesParts_WeakAndDanger_NoParryTell()
        {
            B.Parts p = B.From(Cfg());
            foreach (string w in Weak) Assert.That(p.RoleOf(w), Is.EqualTo("weak"), w);
            foreach (string d in Danger) Assert.That(p.RoleOf(d), Is.EqualTo("danger"), d);
            foreach (string tell in new[] { "wick.tellYellow", "wick.embers", "brawl.block", "loom.spoolTell" })
                Assert.That(p.RoleOf(tell), Is.Null, tell + " is a parry tell: never in the table (D-1)");
        }

        [Test]
        public void Routed_TheRolesColourAtTheOriginalsLuminance()
        {
            GameConfig cfg = Cfg();
            C.Table t = C.From(cfg);
            foreach (var (part, hex) in new[] { ("wick.core", 0xFFA040u), ("crown.gems", 0xFF5A1Fu), ("loom.knees", 0xD8D0FFu), ("wyrm.punch", 0xFFD23Au), ("brawl.weak", 0xFFD23Au), ("wick.edges", 0xFFA040u), ("crown.clap", 0xFFC65Au),
                ("wyrm.open", 0xFFD23Au), ("wyrm.core", 0xFF7A20u), ("wyrm.seams", 0xFF9A28u), ("wyrm.spit", 0xFF7A30u) }) // B3b: the lit head, the core, the seams, the spit (UnityTuning.Wyrm24 / Brawl47)
            {
                double[] c = Hex(hex);
                foreach (double k in new[] { 0.4, 1.0, 2.2 }) // a dim closed weak point, a plain one, an HDR glow
                {
                    B.Route(t, B.From(cfg).RoleOf(part), c[0] * k, c[1] * k, c[2] * k, out double r, out double g, out double b);
                    // NIGHT63 C1 fix round (the spec of C1's ToRole changes: see N63ColoursTests): the luminance kept, or, where that would push a
                    // channel past the original's brightest (amber -> danger), the brightest channel kept: never a clip into another hue
                    double m0 = Math.Max(c[0] * k, Math.Max(c[1] * k, c[2] * k)), m = Math.Max(r, Math.Max(g, b));
                    bool lumaKept = Math.Abs(Luma(r, g, b) - Luma(c[0] * k, c[1] * k, c[2] * k)) <= 1e-9;
                    Assert.That(lumaKept || Math.Abs(m - m0) <= 1e-9, Is.True, part + " x" + k + ": luminance kept, or the brightest channel kept");
                    Assert.That(m, Is.LessThanOrEqualTo(m0 + 1e-9), part + " x" + k + ": never past the original's brightest channel");
                    string role = C.RoleOf(t, r / m, g / m, b / m);
                    Assert.That(role, Is.EqualTo(B.From(cfg).RoleOf(part) == "weak" ? "player" : "danger"), part + " x" + k + ": in its band");
                }
            }
        }

        [Test]
        public void EveryBossesWeakPointsEndInOneRole()
        {
            GameConfig cfg = Cfg();
            C.Table t = C.From(cfg);
            B.Parts p = B.From(cfg);
            double hue0 = double.NaN;
            foreach (string part in p.Names)
            {
                if (p.RoleOf(part) != "weak") continue;
                B.Route(t, "weak", 1, 0.5, 0.2, out double r, out double g, out double b);
                C.Hsv(r, g, b, out double h, out _, out _);
                if (double.IsNaN(hue0)) hue0 = h;
                Assert.That(h, Is.EqualTo(hue0).Within(1e-6), part + ": one weak hue on every boss");
            }
            Assert.That(hue0, Is.Not.NaN);
        }

        [Test]
        public void TheWyrm_TheOpenHeadLitAsBrightAsThePunchWindow_InThePlayersBand()
        {
            GameConfig cfg = Cfg();
            C.Table t = C.From(cfg);
            B.Parts p = B.From(cfg);
            Assert.That(p.RoleOf("wyrm.open"), Is.EqualTo("weak"), "the open head is the weak point");
            double[] c = Hex(UnityTuning.Wyrm47PunchHex);
            double k = UnityTuning.Wyrm47PunchGain;
            B.Route(t, p.RoleOf("wyrm.open"), c[0] * k, c[1] * k, c[2] * k, out double r, out double g, out double b);
            double m = Math.Max(r, Math.Max(g, b));
            Assert.That(C.RoleOf(t, r / m, g / m, b / m), Is.EqualTo("player"), "the lit head in the player's cyan band");
            Assert.That(m, Is.EqualTo(k).Within(1e-6), "as bright as the punch window's head (its gain on the brightest channel)");
        }

        [Test]
        public void TheWyrm_Hooks_TheOpenHeadTheCoreTheSeamsTheSpit()
        {
            string view = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "WyrmView.cs"));
            string wrap = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "BossColours63.N63D.cs"));
            Assert.That(wrap, Does.Contain("public static Color Lit(GameConfig cfg, string part, Color off, Color lit)"), "a wrapper that lights a part only with the row on");
            // NIGHT63 B3c (B3's GATE: the open head never read cyan): the open head now goes through the fight's rim, LitRim (a Lit that also
            // passes the routed colour through BossGrade54.RimAt); the wrapper Lit stays
            Assert.That(view, Does.Contain("BossColours63.LitRim(game.cfg, \"wyrm.open\", "), "the head lit while OPEN");
            Assert.That(view, Does.Contain("Look.Hex(UnityTuning.Wyrm47PunchHex, UnityTuning.Wyrm47PunchGain))"), "lit as the punch window's head");
            Assert.That(view, Does.Contain("w.CoreOpen"), "OPEN: the core exposed");
            Assert.That(view, Does.Contain("sim.wyrm31.Staggered"), "OPEN: staggered");
            Assert.That(view, Does.Contain("BossColours63.PartRim(game.cfg, \"wyrm.core\", "), "the exposed core (B3c: through the fight's rim, as every wyrm weak part)");
            Assert.That(view, Does.Contain("BossColours63.PartRim(game.cfg, \"wyrm.punch\", "), "the punch window's head (B3c: through the fight's rim)");
            Assert.That(view, Does.Contain("BossColours63.Part(game.cfg, \"wyrm.seams\", "), "the amber seams");
            int spit = 0, at = 0;
            while ((at = view.IndexOf("BossColours63.Part(game.cfg, \"wyrm.spit\", ", at, StringComparison.Ordinal)) >= 0) { spit++; at++; }
            Assert.That(spit, Is.EqualTo(2), "the spit and the shards");
        }

        [Test]
        public void B3c_TheNewParts_TheCrownsTellDanger_TheWyrmsOpenMarkWeak()
        {
            B.Parts p = B.From(Cfg());
            Assert.That(p.RoleOf("crown.tell"), Is.EqualTo("danger"), "THE CROWN's red (dodge-only) tell");
            Assert.That(p.RoleOf("wyrm.mark"), Is.EqualTo("weak"), "BossMarks32's open-window shell over THE KILN WYRM's head");
        }

        [Test]
        public void B3c_ADangerRoute_StaysInTheDangerBandOnScreen_AtAnyGain()
        {
            C.Table t = C.From(Cfg());
            // the crown's red tell (Tell36RedHex), its gold edge / clapper (CLook47GoldHex), the wyrm's amber seams (Wyrm47AmberHex), the crown's danger
            foreach (uint hex in new uint[] { 0xFF3A14, 0xFFC65A, 0xFF9A28, 0xFF5A1F })
                foreach (double gain in new[] { 0.6, 1.0, 1.4, 2.8, 3.5 })
                {
                    double[] c = Hex(hex);
                    B.Hot(t, "danger", c[0], c[1], c[2], gain, out double r, out double g, out double b);
                    double dr = Math.Min(1, r), dg = Math.Min(1, g), db = Math.Min(1, b); // the screen clips each channel
                    string at = "#" + hex.ToString("X6") + " x" + gain;
                    Assert.That(C.RoleOf(t, dr, dg, db), Is.EqualTo("danger"), at + ": the colour on screen is in the danger band");
                    B.Route(t, "danger", c[0] * gain, c[1] * gain, c[2] * gain, out double r0, out double g0, out double b0);
                    double m0 = Math.Max(r0, Math.Max(g0, b0));
                    Assert.That(Math.Max(r, Math.Max(g, b)), Is.EqualTo(m0).Within(1e-9), at + ": the brightest channel the plain route's (the glow kept)");
                    if (m0 <= 1) Assert.That(r == r0 && g == g0 && b == b0, Is.True, at + ": within the screen's range, the plain route");
                }
            // the red the GATE measured: the plain route of the red tell at the gem's x2.8 shows amber once clipped
            C.ToRole(t, "danger", 2.8 * 1.0, 2.8 * 0x3A / 255.0, 2.8 * 0x14 / 255.0, out double pr, out double pg, out double pb);
            Assert.That(C.RoleOf(t, Math.Min(1, pr), Math.Min(1, pg), Math.Min(1, pb)), Is.Not.EqualTo("danger"), "the red: a plain route at x2.8 clips off the danger band");
        }

        [Test]
        public void B3c_TheHooks_TheCrownsRedTellAndEdges_TheWyrmsOpenHeadAndShell_TheFrameLine()
        {
            string Src(string f) => File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", f));
            string crown = Src("Crown37View.Crown.cs"), c38 = Src("Crown37View.Crown38.cs"), look = Src("Crown37View.Look47.cs");
            int Count(string s, string what) { int n = 0, at = 0; while ((at = s.IndexOf(what, at, StringComparison.Ordinal)) >= 0) { n++; at++; } return n; }
            Assert.That(Count(crown, "BossColours63.Hot(game.cfg, \"crown.tell\", ") + Count(c38, "BossColours63.Hot(game.cfg, \"crown.tell\", "), Is.EqualTo(5), "the red tell on the blades, the NIGHT37 gems, the gem crosses, their halos and the halo ring (the NIGHT38 gem cubes are never drawn in the tell colour)");
            Assert.That(crown, Does.Contain("bool redTell = telling && br.Current.kind != Tell36Kind.Yellow;"), "only a red (dodge-only) tell: the parry yellow untouched");
            Assert.That(look, Does.Contain("BossColours63.Hot(game.cfg, \"crown.edge\", "), "the ring's edges after their tell gain");
            Assert.That(look, Does.Contain("BossColours63.Hot(game.cfg, \"crown.clap\", "), "the clapper after its gain");
            Assert.That(Src("WyrmView.cs"), Does.Contain("BossColours63.LitRim(game.cfg, \"wyrm.open\", "), "the open head through the fight's rim");
            // NIGHT63 B3c, the lead's addition (4:14: the role measure FAILs THE LOOM, stagger cyan 5,832 + blue 422 px): on b5's frame the knees B3
            // routes to the weak cyan read hue 224 (blue), V 0.34, over 33,607 changed px (the 422 are their brightest px): the loom's grade (grade54
            // loom37, filter 0.86 / 0.86 / 1) cuts red and green. Every loom weak site goes through the fight's rim, as the wyrm's
            string loom = Src("Loom37View.cs") + Src("Loom37View.N47.cs");
            Assert.That(Count(loom, "BossColours63.PartRim(game.cfg, \"loom."), Is.EqualTo(4), "the knees, the nails (twice) and the seams through the fight's rim");
            Assert.That(Count(loom, "BossColours63.Part(game.cfg, \"loom."), Is.EqualTo(0), "no loom weak site left on the plain route");
            Assert.That(Src("BossMarks32.cs"), Does.Contain("BossColours63.ShellRim(game.cfg, \"wyrm.mark\", "), "the open-window shell over the head");
            Assert.That(Src("BossGrade54.cs"), Does.Contain("public static Color RimAt(Color c)"), "the fight's rim at a colour's own brightness");
            Assert.That(Src("FeelReel.N63Same.cs"), Does.Contain("BossColours63.FrameLine("), "the per-shot proof line");
        }

        [Test]
        public void B3c_TheFrameLine_PerPartTheColourTheFrameUsed()
        {
            var parts = new[]
            {
                new B.FramePart { part = "crown.gems", role = "weak", fr = 0.95, fg = 0.92, fb = 0.85, tr = 0.11, tg = 0.92, tb = 1.0, calls = 2 },
                new B.FramePart { part = "crown.tell", role = "danger", fr = 2.0, fg = 0.0, fb = 0.0, tr = 2.0, tg = 0.0, tb = 0.0, calls = 1 },
            };
            string l = B.FrameLine("shot-N30-B63-crown37-idle.png", 631, parts);
            StringAssert.StartsWith("[bossColours63] shot-N30-B63-crown37-idle.png (frame 631): ", l);
            StringAssert.Contains("crown.gems weak #F2EBD9 -> #1CEBFF (2 calls; the last drawn)", l);
            StringAssert.Contains("crown.tell danger #FF0000 x2.0 -> #FF0000 x2.0 (1 call)", l);
            Assert.That(B.FrameLine("x.png", 1, new B.FramePart[0]), Does.Contain("no boss part routed"));
            Assert.That(B.HexOf(1, 0, 0), Is.EqualTo("#FF0000"));
            Assert.That(B.HexOf(2, 1, 0), Is.EqualTo("#FF8000 x2.0"));
        }

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitches()
        {
            Assert.That(B.Decide(false, new string[0]), Is.False);
            Assert.That(B.Decide(true, new string[0]), Is.True);
            Assert.That(B.Decide(true, new[] { "-skygrindOff63" }), Is.False);
            Assert.That(B.Decide(true, new[] { "-skygrindOff60" }), Is.False);
            Assert.That(B.Decide(true, new[] { "-skygrindOff64" }), Is.True);
            Assert.That(B.Decide(true, new[] { "-skygrindNoBossColours63" }), Is.False);
            Assert.That(B.Decide(true, new[] { "-skygrindNoColours63" }), Is.True, "C1's own switch leaves B3 on (it only reads C1's table)");
            NightRows.ReelOverride.Clear();
            try
            {
                NightRows.ReelOverride[B.Row] = false;
                Assert.That(NightRows.Live(B.Decide(true, new string[0]), B.Row), Is.False, "D1's held off frame");
            }
            finally { NightRows.ReelOverride.Clear(); }
        }
    }
}
