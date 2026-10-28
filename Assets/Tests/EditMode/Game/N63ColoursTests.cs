// NIGHT63 builder D (slate C1, THE COLOUR TABLE; view row design.n63.colours63, OFF; Colours63Math.N63D.cs, Colours63.N63D.cs). Pinned:
//   1. the table is the row's numbers: the roles' colours read from config's role colours (player = colors.usable, danger = colors.enemy, the
//      boundary = barrier.boundaryColor) and the table's own two (parry yellow: DECIDE63 D-1, the parry tell only; RIVAL magenta); the role bands;
//      the world tier's saturation; the routes (THE CROWN's goal column -> the boundary, the lava's gold -> danger)
//   2. a colour's role: in a band (player, danger, parry, rival), the boundary (pale violet-white), neutral (no hue), or off
//   3. what is routed: an off-palette colour at or over the route floor (the lime and green accents, the kiln's blue trims, the kit lamp's amber
//      edge line); never a role colour, a neutral one, or a dark material under the floor
//   4. the world tier: the hue kept, the saturation down to the table's, the luminance (Rec. 709) kept, HDR included; under it unchanged
//   5. a route to a role: the role's hue and saturation at the original's luminance (the lava's gold -> the danger red-orange)
//   6. the switch: off with the row off, under -skygrindOff63 (or an older night's) and -skygrindNoColours63; read every frame through NightRows.Live
//   7. the meter's view: a routed piece leaves the vivid tier n63-c1-meter.py counts (S under its 0.55 floor)
//   8. NIGHT63 review S2 (Logs/n63/review-n63.md): the two scans run on decor54's own clock (FeelClock.UnscaledDelta summed from the map's
//      first frame, 0 in a held frame), never the wall clock: the first Colours63FirstScanFrames frames after the map is seen, the second
//      after decor54's second pass on the same clock, at any frame rate (on a slow Player the wall clock ran ahead of the reel's fixed clock,
//      so the second scan could come before the pass it waits for and two runs could differ). Written test first: RED on its commit.
// Written test first (NIGHT63.md section 4): RED on its commit (Colours63Math's placeholders answer nothing; the row's numbers are empty).
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using M = Skygrind.Game.Colours63Math;

namespace Skygrind.Game.Tests
{
    public class N63ColoursTests
    {
        const double E = 1e-9;

        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        static double[] Hex(uint x) => new[] { ((x >> 16) & 255) / 255.0, ((x >> 8) & 255) / 255.0, (x & 255) / 255.0 };

        static double Luma(double[] c) => 0.2126 * c[0] + 0.7152 * c[1] + 0.0722 * c[2];

        static void Hsv(double[] c, out double h, out double s, out double v)
        {
            double mx = Math.Max(c[0], Math.Max(c[1], c[2])), mn = Math.Min(c[0], Math.Min(c[1], c[2])), d = mx - mn;
            h = 0;
            if (d > 1e-12)
            {
                if (mx == c[0]) h = (60 * ((c[1] - c[2]) / d) + 360) % 360;
                else if (mx == c[1]) h = 60 * ((c[2] - c[0]) / d) + 120;
                else h = 60 * ((c[0] - c[1]) / d) + 240;
            }
            s = mx > 0 ? d / mx : 0;
            v = mx;
        }

        static string Role(M.Table t, uint x) { double[] c = Hex(x); return M.RoleOf(t, c[0], c[1], c[2]); }

        static bool Routes(M.Table t, uint x) { double[] c = Hex(x); return M.Routes(t, c[0], c[1], c[2]); }

        [Test]
        public void Table_IsTheRowsNumbers_RolesFromConfigsRoleColours()
        {
            GameConfig c = Cfg();
            M.Table t = M.From(c);
            Assert.That(t, Is.Not.Null, "design.n63.colours63.numbers reads as a table");
            var (ur, ug, ub) = c.Rgb("colors.usable");
            var (er, eg, eb) = c.Rgb("colors.enemy");
            var (br, bg, bb) = c.Rgb("barrier.boundaryColor");
            Assert.That(t.player, Is.EqualTo(new double[] { ur, ug, ub }).Within(E), "player = colors.usable");
            Assert.That(t.danger, Is.EqualTo(new double[] { er, eg, eb }).Within(E), "danger = colors.enemy");
            Assert.That(t.boundary, Is.EqualTo(new double[] { br, bg, bb }).Within(E), "boundary = barrier.boundaryColor");
            Assert.That(M.RoleOf(t, t.parry[0], t.parry[1], t.parry[2]), Is.EqualTo("parry"), "the table's parry colour sits in its own band");
            Assert.That(M.RoleOf(t, t.rival[0], t.rival[1], t.rival[2]), Is.EqualTo("rival"), "the table's RIVAL colour sits in its own band");
            Assert.That(t.worldSat, Is.GreaterThan(0).And.LessThan(0.55), "the world tier sits under the meter's vivid floor");
            Assert.That(t.Route("goal47"), Is.EqualTo("boundary"), "THE CROWN's goal column: the boundary (gg-proxy's danger-red line on spire's exit)");
            Assert.That(t.Route("lavaGold"), Is.EqualTo("danger"), "the lava's gold: danger");
            Assert.That(t.Route("nothing"), Is.Null);
        }

        [Test]
        public void RoleOf_EachBand()
        {
            M.Table t = M.From(Cfg());
            Assert.That(Role(t, 0x19D3E6), Is.EqualTo("player"));
            Assert.That(Role(t, 0xFF4A1C), Is.EqualTo("danger"));
            Assert.That(Role(t, 0xFF8A3A), Is.EqualTo("danger"), "THE CROWN's red-gold column is in the danger band (a role misused: a route, not a band, moves it)");
            Assert.That(Role(t, 0xFFD000), Is.EqualTo("parry"));
            Assert.That(Role(t, 0xFF2BD6), Is.EqualTo("rival"));
            Assert.That(Role(t, 0xC9BCFF), Is.EqualTo("boundary"));
            Assert.That(Role(t, 0xFFFFFF), Is.EqualTo("neutral"));
            Assert.That(Role(t, 0x3A3F4A), Is.EqualTo("neutral"), "a dark gunmetal (S 0.22, V 0.29) has no hue that counts");
            Assert.That(Role(t, 0x8DD957), Is.EqualTo("off"), "span's lime accent");
            Assert.That(Role(t, 0x5A82E0), Is.EqualTo("off"), "kiln's blue accent");
            Assert.That(Role(t, 0x3CFF9A), Is.EqualTo("off"), "the green fixture");
        }

        [Test]
        public void Routes_OffPaletteAtOrOverTheFloor_NeverARoleOrNeutral()
        {
            M.Table t = M.From(Cfg());
            foreach (uint x in new uint[] { 0x8DD957, 0x86D9A2, 0x5A82E0, 0xA267E6, 0xE67793, 0x3CFF9A, 0xFFB347, 0xE8C48A })
                Assert.That(Routes(t, x), Is.True, "routed: #" + x.ToString("X6"));
            foreach (uint x in new uint[] { 0x19D3E6, 0xFF4A1C, 0xFFD000, 0xFF2BD6, 0xC9BCFF, 0xFFFFFF, 0x808080, 0x3A3F4A, 0x15161A })
                Assert.That(Routes(t, x), Is.False, "kept: #" + x.ToString("X6"));
        }

        [Test]
        public void World_HueKept_SaturationToTheTables_LuminanceKept()
        {
            M.Table t = M.From(Cfg());
            foreach (uint x in new uint[] { 0x8DD957, 0x5A82E0, 0xE8C48A, 0x3CFF9A })
            {
                double[] c = Hex(x);
                foreach (double k in new[] { 1.0, 2.5 }) // an HDR emissive too
                {
                    double[] a = { c[0] * k, c[1] * k, c[2] * k };
                    M.World(t, a[0], a[1], a[2], out double r, out double g, out double b);
                    double[] o = { r, g, b };
                    Hsv(a, out double h0, out _, out _);
                    Hsv(o, out double h1, out double s1, out _);
                    Assert.That(Math.Abs(((h1 - h0) % 360 + 540) % 360 - 180), Is.LessThan(0.5), "hue kept #" + x.ToString("X6"));
                    Assert.That(s1, Is.EqualTo(t.worldSat).Within(1e-6), "saturation to the table's #" + x.ToString("X6"));
                    Assert.That(Luma(o), Is.EqualTo(Luma(a)).Within(1e-9), "luminance kept #" + x.ToString("X6") + " x" + k);
                }
            }
            double[] pale = Hex(0xB4BFA8); // S 0.12: under the tier
            M.World(t, pale[0], pale[1], pale[2], out double pr, out double pg, out double pb);
            Assert.That(new[] { pr, pg, pb }, Is.EqualTo(pale).Within(E), "a colour already under the tier is unchanged");
        }

        [Test]
        public void ToRole_TheRolesHueAtTheOriginalsLuminance()
        {
            M.Table t = M.From(Cfg());
            double[] gold = Hex(0xFFB84A);
            M.ToRole(t, "danger", gold[0], gold[1], gold[2], out double r, out double g, out double b);
            double[] o = { r, g, b };
            Assert.That(M.RoleOf(t, r / Math.Max(r, Math.Max(g, b)), g / Math.Max(r, Math.Max(g, b)), b / Math.Max(r, Math.Max(g, b))), Is.EqualTo("danger"));
            // NIGHT63 C1 fix round (b4: foundry's lava still amber with the row on, 0.496 -> 0.447 %): the gold's luminance in danger's red-orange
            // overflows (HDR x1.8: (1.8, 0.92, 0.34) clips on screen to (1, 0.92, 0.34), hue 54, amber-yellow again). The spec changes: a route keeps the
            // original's luminance UNLESS that pushes a channel past the original's brightest; then it keeps the brightest channel (no clip, no hue shift)
            Assert.That(Math.Max(r, Math.Max(g, b)), Is.EqualTo(Math.Max(gold[0], Math.Max(gold[1], gold[2]))).Within(1e-9), "the gold's brightest channel kept (its luminance would overflow)");
            Assert.That(Luma(o), Is.LessThanOrEqualTo(Luma(gold) + 1e-9), "never brighter than the gold");
            M.ToRole(t, "boundary", 1, 0.54, 0.23, out r, out g, out b);
            Assert.That(M.RoleOf(t, r / Math.Max(r, Math.Max(g, b)), g / Math.Max(r, Math.Max(g, b)), b / Math.Max(r, Math.Max(g, b))), Is.EqualTo("boundary").Or.EqualTo("neutral"), "the goal column to the pale violet-white");
            Assert.Throws<ArgumentException>(() => M.ToRole(t, "nope", 1, 1, 1, out _, out _, out _), "an unknown role is refused");
        }

        [Test]
        public void ToRole_NeverPastTheOriginalsBrightestChannel_LuminanceKeptWhenItFits()
        {
            M.Table t = M.From(Cfg());
            foreach (string role in new[] { "danger", "player", "boundary", "parry", "rival" })
                foreach (uint x in new uint[] { 0xFFB84A, 0xFFD23A, 0xFFA040, 0xFFC65A, 0x5A82E0, 0x8DD957, 0xFFFFFF, 0x604010 })
                    foreach (double k in new[] { 0.3, 1.0, 2.0 }) // a dim, a plain and an HDR source
                    {
                        double[] c = Hex(x);
                        double r0 = c[0] * k, g0 = c[1] * k, b0 = c[2] * k, m0 = Math.Max(r0, Math.Max(g0, b0));
                        M.ToRole(t, role, r0, g0, b0, out double r, out double g, out double b);
                        double m1 = Math.Max(r, Math.Max(g, b));
                        string at = role + " from #" + x.ToString("X6") + " x" + k;
                        Assert.That(m1, Is.LessThanOrEqualTo(m0 + 1e-9), at + ": never past the source's brightest channel (no clip into another hue)");
                        bool lumaKept = Math.Abs(Luma(new[] { r, g, b }) - Luma(new[] { r0, g0, b0 })) <= 1e-9;
                        Assert.That(lumaKept || Math.Abs(m1 - m0) <= 1e-9, Is.True, at + ": the luminance kept, or (when that would overflow) the brightest channel kept");
                        Assert.That(M.RoleOf(t, r / m1, g / m1, b / m1), Is.EqualTo(role).Or.EqualTo("neutral"), at + ": in the role's own band");
                    }
        }

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitches()
        {
            Assert.That(M.Decide(false, new string[0]), Is.False, "the row off");
            Assert.That(M.Decide(true, new string[0]), Is.True, "the row on, no switch");
            Assert.That(M.Decide(true, new[] { "-skygrindOff63" }), Is.False);
            Assert.That(M.Decide(true, new[] { "-skygrindOff61" }), Is.False, "an older night's switch covers it");
            Assert.That(M.Decide(true, new[] { "-skygrindOff64" }), Is.True, "a later night's leaves it on");
            Assert.That(M.Decide(true, new[] { "-skygrindnocolours63" }), Is.False, "the one-row switch ignores case");
            Assert.That(M.Decide(true, new[] { "-skygrindNoBossColours63" }), Is.True, "another row's switch leaves it on");
            NightRows.ReelOverride.Clear();
            try
            {
                NightRows.ReelOverride[M.Row] = false;
                Assert.That(NightRows.Live(M.Decide(true, new string[0]), M.Row), Is.False, "D1's held frame turns it off");
            }
            finally { NightRows.ReelOverride.Clear(); }
        }

        [Test]
        public void TheScans_OnDecor54sOwnClock_TheSecondAfterItsSecondPass_AtAnyFrameRate()
        {
            var runs = new[] { new[] { 1 / 60.0 }, new[] { 1 / 144.0 }, new[] { 1 / 30.0 }, new[] { 0.004, 0.031, 0.012, 0.050, 0.008 } }; // the reel's fixed clock, fast, slow, jittery
            foreach (double[] deltas in runs)
            {
                int scans = 0, frames = 0, first = -1, second = -1, decorPass = -1;
                double since = 0, decorSince = 0;
                for (int f = 0; f < 5000 && second < 0; f++)
                {
                    double dt = deltas[f % deltas.Length];
                    decorSince += dt; // Decor54.Update: since += FeelClock.UnscaledDelta from the map's first frame, its second pass at Decor54SecondPassS
                    if (decorPass < 0 && decorSince >= UnityTuning.Decor54SecondPassS) decorPass = f;
                    if (f == 0) continue; // the map's first frame: Colours63.NewMap (the seen frame: frame 0 of the count)
                    frames++;
                    since += dt;
                    int due = M.ScanDue(scans, frames, since);
                    if (due == 1) { scans = 1; first = f; }
                    else if (due == 2) { scans = 2; second = f; }
                }
                string at = "frame time " + deltas[0].ToString("0.###");
                Assert.That(first, Is.EqualTo(UnityTuning.Colours63FirstScanFrames), at + ": the first scan Colours63FirstScanFrames frames after the map is seen");
                Assert.That(second, Is.GreaterThan(decorPass), at + ": the second scan after decor54's second pass on the same clock");
                Assert.That(decorPass, Is.GreaterThan(0));
            }
            Assert.That(M.ScanDue(0, 0, 100), Is.EqualTo(0), "no time makes the first scan: it counts frames");
            Assert.That(M.ScanDue(2, 1000, 100), Is.EqualTo(0), "two scans a map");
        }

        [Test]
        public void TheScans_TheHook_ReadsFeelClockNeverTheWallClock_NeverInAHeldFrame()
        {
            string src = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "Colours63.N63D.cs"));
            string decor = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "Decor54.cs"));
            Assert.That(src, Does.Not.Contain("realtimeSinceStartup"), "no wall clock");
            Assert.That(src, Does.Contain("scanSince += FeelClock.UnscaledDelta;"), "decor54's clock");
            Assert.That(decor, Does.Contain("since += FeelClock.UnscaledDelta;"), "decor54 still on that clock (if it moves, move this too)");
            Assert.That(src, Does.Contain("Colours63Math.ScanDue(scans, scanFrames, scanSince)"), "the rule the test above pins");
            Assert.That(src, Does.Contain("if (!FeelClock.Holding)"), "a held frame neither counts nor scans");
        }

        [Test]
        public void Meter_ARoutedPieceLeavesTheVividTier()
        {
            M.Table t = M.From(Cfg());
            foreach (uint x in new uint[] { 0x8DD957, 0x5A82E0, 0x3CFF9A })
            {
                double[] c = Hex(x);
                M.World(t, c[0], c[1], c[2], out double r, out double g, out double b);
                Hsv(new[] { r, g, b }, out _, out double s, out _);
                Assert.That(s, Is.LessThan(0.55), "n63-c1-meter.py's vivid floor S 0.55: #" + x.ToString("X6") + " routed is not counted");
            }
        }
    }
}
