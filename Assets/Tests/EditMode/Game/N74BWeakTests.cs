// NIGHT74 builder B (slate B1, design.n74.weak74, rule W; Game/Weak74.cs, its hook in Game/BossColours63.N63D.cs Routes). Pinned:
//   1. the row: in the config's design (design.n74.weak74.enabled exists), read through NightRows.ViewOn both ways (the reel's live override), a FrameRows row (same-run pairs)
//   2. the identity colour: at the site's own luminance, never past its brightest channel, the identity's hue; black stays black; HDR sites keep their glow
//   3. the identity colours stay out of every role's arc: not cyan (the player's), not danger (345-35, S >= 0.35), not the parry yellow (45-70, S >= 0.35), not RIVAL's magenta (285-335),
//      not the boundary's pale violet-white (230-300, S under 0.4, V >= 0.75); and a closed part routed through them reads as no cyan at all
//   4. the cyan test: the player's cyan (plain, HDR, pale pulse) reads cyan; bone, steel, the danger red-orange, the parry yellow, the boundary's violet-white and magenta do not
//   5. every weak part of bossColours63's table has a boss, so the role "id74" can name its identity colour (a part of THE KILN WYRM keeps NIGHT71's answer, never "id74")
//   6. the direction word of a point off screen (target74 shares it): BEHIND behind the camera, else the larger overshoot, null on screen
// Not run in Unity by its author (the Editor run is the lead's).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using W = Skygrind.Game.Weak74;
using M = Skygrind.Game.Weak74Math;

namespace Skygrind.Game.Tests
{
    public class N74BWeakTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        [SetUp]
        public void SetUp() { NightRows.ReelOverride.Clear(); NightRows.ForceOffFrom = NightRows.NoOff; }

        [TearDown]
        public void TearDown() { NightRows.ReelOverride.Clear(); NightRows.ForceOffFrom = NightRows.NoOff; }

        [Test]
        public void Row_InTheDesign_ReadBothWays_AFrameRow()
        {
            GameConfig c = Cfg();
            Assert.That(c.Has("design.n74.weak74.enabled"), Is.True, "the shared commit placed the row");
            NightRows.ReelOverride[W.Row] = true;
            Assert.That(W.Decide(c), Is.True, "the reel's override turns it on");
            NightRows.ReelOverride[W.Row] = false;
            Assert.That(W.Decide(c), Is.False, "and off");
            Assert.That(W.Decide(null), Is.False, "no config, no row");
            Assert.That(SameRun63Math.KindOf(W.Row), Is.EqualTo(SameRun63Math.Kind.Frame), "a same-run SPOT pair can toggle it");
        }

        [Test]
        public void Identity_AtTheSitesLuminance_NeverPastItsBrightestChannel()
        {
            for (int boss = 0; boss < 5; boss++)
            {
                Color id = W.IdentityColour(boss);
                // the player's cyan #19D3E6 at x2.2 (an HDR weak point)
                double r = 0.098 * 2.2, g = 0.827 * 2.2, b = 0.902 * 2.2;
                M.ToIdentity(new double[] { id.r, id.g, id.b }, r, g, b, out double R, out double G, out double B);
                double l0 = 0.2126 * r + 0.7152 * g + 0.0722 * b, l1 = 0.2126 * R + 0.7152 * G + 0.0722 * B;
                Assert.That(l1, Is.EqualTo(l0).Within(1e-9).Or.LessThan(l0), "the site's luminance (or less, capped), boss " + boss);
                Assert.That(Math.Max(R, Math.Max(G, B)), Is.LessThanOrEqualTo(Math.Max(r, Math.Max(g, b)) + 1e-9), "never past the site's brightest channel, boss " + boss);
                Assert.That(R / B, Is.EqualTo(id.r / id.b).Within(1e-6), "the identity's red / blue, boss " + boss);
                M.ToIdentity(new double[] { id.r, id.g, id.b }, 0, 0, 0, out R, out G, out B);
                Assert.That(R + G + B, Is.EqualTo(0), "black stays black");
                Color k = W.ClosedBoss(boss, new Color(0.2f, 0.9f, 1f, 0.5f));
                Assert.That(k.a, Is.EqualTo(0.5f), "alpha kept");
            }
        }

        [Test]
        public void IdentityColours_StayOutOfEveryRolesArc()
        {
            for (int boss = 0; boss < 5; boss++)
            {
                Color id = W.IdentityColour(boss);
                double h = M.HueOf(id.r, id.g, id.b, out double s);
                double v = Math.Max(id.r, Math.Max(id.g, id.b));
                string who = "boss " + boss + " hue " + h.ToString("0") + " S " + s.ToString("0.00") + " V " + v.ToString("0.00");
                Assert.That(M.IsCyanish(id.r, id.g, id.b, UnityTuning.N74CyanHueLo, UnityTuning.N74CyanHueHi, UnityTuning.N74CyanSatMin), Is.False, "not cyan: " + who);
                if (s >= 0.35)
                {
                    Assert.That(h >= 345 || h <= 35, Is.False, "not danger red-orange: " + who);
                    Assert.That(h >= 45 && h <= 70, Is.False, "not the parry yellow: " + who);
                }
                Assert.That(h >= 285 && h <= 335 && s >= 0.35, Is.False, "not RIVAL's magenta: " + who);
                Assert.That(h >= 230 && h <= 300 && s < 0.4 && v >= 0.75 && s >= 0.15, Is.False, "not the boundary's pale violet-white: " + who);
                // a closed part routed through it reads as no cyan, whatever the site's colour (the player's cyan at several gains)
                foreach (float gain in new[] { 0.4f, 1f, 2.2f })
                {
                    Color k = W.ClosedBoss(boss, new Color(0.098f * gain, 0.827f * gain, 0.902f * gain, 1f));
                    Assert.That(M.IsCyanish(k.r, k.g, k.b, UnityTuning.N74CyanHueLo, UnityTuning.N74CyanHueHi, UnityTuning.N74CyanSatMin), Is.False, "routed colour is not cyan: " + who + " gain " + gain);
                }
            }
            // the bosses' own: three distinct identities (each boss its own palette, GG 2026-10-20), the crown's shared by the two with no routed part
            Assert.That(W.IdentityColour(W.Wick), Is.Not.EqualTo(W.IdentityColour(W.Crown)));
            Assert.That(W.IdentityColour(W.Loom), Is.Not.EqualTo(W.IdentityColour(W.Crown)));
            Assert.That(W.IdentityColour(W.Wick), Is.Not.EqualTo(W.IdentityColour(W.Loom)));
        }

        [Test]
        public void CyanTest_ThePlayersCyanOnly()
        {
            double lo = UnityTuning.N74CyanHueLo, hi = UnityTuning.N74CyanHueHi, sat = UnityTuning.N74CyanSatMin;
            Assert.That(M.IsCyanish(0.098, 0.827, 0.902, lo, hi, sat), Is.True, "colors.usable #19D3E6");
            Assert.That(M.IsCyanish(0.098 * 2.2, 0.827 * 2.2, 0.902 * 2.2, lo, hi, sat), Is.True, "HDR cyan (the window's pulse)");
            Assert.That(M.IsCyanish(0.60, 1.0, 1.0, lo, hi, sat), Is.True, "a pale cyan-white pulse (the lit ring)");
            Assert.That(M.IsCyanish(0.211, 0.878, 1.0, lo, hi, sat), Is.True, "THE WICK's open core #36E0FF");
            Assert.That(M.IsCyanish(0.949, 0.918, 0.847, lo, hi, sat), Is.False, "crown40's gem bone #F2EAD8");
            Assert.That(M.IsCyanish(0.722, 0.698, 0.800, lo, hi, sat), Is.False, "the crown's steel #B8B2CC");
            Assert.That(M.IsCyanish(1.0, 0.29, 0.11, lo, hi, sat), Is.False, "danger #FF4A1C");
            Assert.That(M.IsCyanish(1.0, 0.816, 0.0, lo, hi, sat), Is.False, "the parry yellow #FFD000");
            Assert.That(M.IsCyanish(0.788, 0.737, 1.0, lo, hi, sat), Is.False, "the boundary #C9BCFF");
            Assert.That(M.IsCyanish(1.0, 0.169, 0.839, lo, hi, sat), Is.False, "RIVAL's magenta #FF2BD6");
            Assert.That(M.IsCyanish(0, 0, 0, lo, hi, sat), Is.False, "black");
        }

        [Test]
        public void Parts_EveryWeakPartHasItsBoss_TheWyrmKeepsNight71sAnswer()
        {
            GameConfig c = Cfg();
            var parts = BossColours63Math.From(c);
            Assert.That(parts, Is.Not.Null);
            int weak = 0;
            foreach (string p in parts.Names)
            {
                if (parts.RoleOf(p) != "weak") continue;
                weak++;
                int i = Cyan71.BossOf(p);
                Assert.That(i, Is.GreaterThanOrEqualTo(0), p + " belongs to one of the five bosses");
            }
            Assert.That(weak, Is.GreaterThan(0));
            // with the row off (no game: every boss reads closed) a part's role is NIGHT71's: "weak" or "bone", never "id74"
            NightRows.ReelOverride[W.Row] = false;
            foreach (string p in parts.Names)
                if (parts.RoleOf(p) == "weak") Assert.That(W.WeakRole(c, p), Is.Not.EqualTo(W.Role), p + " with the row off");
            // with the row on, a closed part of THE WICK, THE CROWN or THE LOOM is "id74"; THE KILN WYRM's never is
            NightRows.ReelOverride[W.Row] = true;
            foreach (string p in parts.Names)
            {
                if (parts.RoleOf(p) != "weak") continue;
                int i = Cyan71.BossOf(p);
                string role = W.WeakRole(c, p);
                if (i == W.Wyrm) Assert.That(role, Is.Not.EqualTo(W.Role), p + ": THE KILN WYRM's weak parts are core73's");
                else Assert.That(role, Is.EqualTo(W.Role).Or.EqualTo("weak"), p + ": closed -> id74, open -> weak");
            }
        }

        [Test]
        public void Direction_OfAPointOffScreen()
        {
            Assert.That(M.Direction(0.5, 0.5, 5), Is.Null, "on screen: nothing");
            Assert.That(M.Direction(0.5, 0.5, -1), Is.EqualTo("BEHIND"), "behind the camera");
            Assert.That(M.Direction(0.5, 1.4, 5), Is.EqualTo("ABOVE"));
            Assert.That(M.Direction(0.5, -0.3, 5), Is.EqualTo("BELOW"));
            Assert.That(M.Direction(-0.2, 0.5, 5), Is.EqualTo("LEFT"));
            Assert.That(M.Direction(1.6, 0.5, 5), Is.EqualTo("RIGHT"));
            Assert.That(M.Direction(1.6, 1.2, 5), Is.EqualTo("RIGHT"), "the larger overshoot wins (0.6 over 0.2)");
            Assert.That(M.Direction(1.2, 1.6, 5), Is.EqualTo("ABOVE"));
        }

        [Test]
        public void HueOf_Basics()
        {
            Assert.That(M.HueOf(1, 0, 0, out double s), Is.EqualTo(0).Within(1e-9));
            Assert.That(s, Is.EqualTo(1).Within(1e-9));
            Assert.That(M.HueOf(0, 1, 0, out _), Is.EqualTo(120).Within(1e-9));
            Assert.That(M.HueOf(0, 0, 1, out _), Is.EqualTo(240).Within(1e-9));
            Assert.That(M.HueOf(0, 1, 1, out _), Is.EqualTo(180).Within(1e-9));
            Assert.That(M.HueOf(1, 0, 1, out _), Is.EqualTo(300).Within(1e-9));
            Assert.That(M.HueOf(0.5, 0.5, 0.5, out s), Is.EqualTo(0));
            Assert.That(s, Is.EqualTo(0));
        }
    }
}
