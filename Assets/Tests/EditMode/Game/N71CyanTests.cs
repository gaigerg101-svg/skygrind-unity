// NIGHT71 builder B (slate B4 "boss weak points in bone while closed, cyan only while open", design.n71.cyan71; Game/Cyan71.cs, its hook in
// Game/BossColours63.N63D.cs Routes). Pinned:
//   1. the switch: off when the row is off, under -skygrindOff71 (or older), -skygrindNoCyan71; a FrameRows row
//   2. the bone: at the site's own luminance, never past its brightest channel, the hue of numbers.boneHex
//   3. the parts: every weak part of bossColours63's table belongs to one of the five bosses (a part of no boss keeps its cyan)
//   4. with the row off (the repo), a weak part's role is "weak" exactly as NIGHT63's (no game, so its boss reads closed)
// Not run in Unity by its author (the Editor run is the lead's).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using C = Skygrind.Game.Cyan71;

namespace Skygrind.Game.Tests
{
    public class N71CyanTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        static GameConfig WithRow(bool on)
        {
            GameConfig c = Cfg();
            var design = (Dictionary<string, object>)c.root["design"];
            var n71 = (Dictionary<string, object>)design["n71"];
            ((Dictionary<string, object>)n71[C.Row])["enabled"] = on;
            return c;
        }

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitches()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRows.ReelOverride.Remove(C.Row);
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(C.OffReason(off, new string[0]), Is.Not.Null);
                Assert.That(C.OffReason(on, new string[0]), Is.Null);
                Assert.That(C.OffReason(on, new[] { "-skygrindNoCyan71" }), Is.Not.Null, "the play card's comparison switch");
                Assert.That(C.OffReason(on, new[] { "-skygrindOff71" }), Is.Not.Null);
                Assert.That(C.OffReason(on, new[] { "-skygrindNoBossColours63" }), Is.Null, "bossColours63's own switch is its own");
            }
            finally
            {
                NightRows.ReelOverride.Remove(C.Row);
                NightRows.ForceOffFrom = was;
            }
            Assert.That(SameRun63Math.KindOf(C.Row), Is.EqualTo(SameRun63Math.Kind.Frame));
        }

        [Test]
        public void Bone_AtTheSitesLuminance_NeverPastItsBrightestChannel()
        {
            double[] bone = { 0.949, 0.918, 0.847 };
            // the player's cyan #19D3E6 at x2.2 (an HDR weak point)
            double r = 0.098 * 2.2, g = 0.827 * 2.2, b = 0.902 * 2.2;
            C.ToBone(bone, r, g, b, out double R, out double G, out double B);
            double l0 = 0.2126 * r + 0.7152 * g + 0.0722 * b, l1 = 0.2126 * R + 0.7152 * G + 0.0722 * B;
            Assert.That(l1, Is.EqualTo(l0).Within(1e-9).Or.LessThan(l0), "the site's luminance (or less, capped)");
            Assert.That(Math.Max(R, Math.Max(G, B)), Is.LessThanOrEqualTo(Math.Max(r, Math.Max(g, b)) + 1e-9), "never past the site's brightest channel");
            Assert.That(R / B, Is.EqualTo(bone[0] / bone[2]).Within(1e-9), "the bone's hue");
            C.ToBone(bone, 0, 0, 0, out R, out G, out B);
            Assert.That(R + G + B, Is.EqualTo(0), "black stays black");
        }

        [Test]
        public void Parts_EveryWeakPartHasItsBoss()
        {
            GameConfig c = Cfg();
            var parts = BossColours63Math.From(c);
            Assert.That(parts, Is.Not.Null);
            foreach (string p in parts.Names)
                if (parts.RoleOf(p) == "weak") Assert.That(C.BossOf(p), Is.GreaterThanOrEqualTo(0), p + ": one of the five bosses");
            Assert.That(C.BossOf("wyrm.open"), Is.EqualTo(Array.IndexOf(C.Bosses, "wyrm")));
            Assert.That(C.BossOf("wyrmx.open"), Is.EqualTo(-1));
            Assert.That(C.BossOf(""), Is.EqualTo(-1));
            Assert.That(c.Str(C.Root + "boneHex"), Does.StartWith("#"));
        }

        [Test]
        public void RowOff_TheWeakRoleAsNight63()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRows.ReelOverride.Remove(C.Row);
                Assert.That(C.WeakRole(WithRow(false), "wick.core"), Is.EqualTo("weak"));
                Assert.That(C.WeakRole(WithRow(false), "nobody.part"), Is.EqualTo("weak"), "a part of no boss keeps its cyan");
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
