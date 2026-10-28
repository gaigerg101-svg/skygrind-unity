// NIGHT74 builder B (leftover 2, design.n74.crownroom74; Game/CrownRoom74.cs, hooked into Game/Palette72.cs by the lead's Research/2026-10-27-wave74/B/crownroom74-hook.cjs). Pinned:
//   1. the row: read through NightRows.ViewOn both ways (the reel's override), a name the shared commit may not hold yet reads off
//   2. the room test: a point inside the room's floor rect grown by the margin, in WEB coordinates (the stone is Unity space: web z is -z): in, on the edge, just outside, the mirror image out
//   3. Wall: inactive (no row or no room) it returns palette72's multiplier untouched; active it returns the replacement inside the room and palette72's outside, and counts both
//   4. no game, no level: Begin leaves it inactive and says so (an EditMode run has no campaign)
//   5. the numbers' reasons: the replacement keeps palette72's spire wall luminance weight (within 10 %), and is the slate ramp's own colour: on colour56's wallUpper (0x6e7280) it stays under S 0.35 and
//      out of the boundary's arc, where palette72's x(1.23, 0.93, 2.92) takes the blue past 1.0 (the violet)
// Not run in Unity by its author (the Editor run is the lead's).
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using C = Skygrind.Game.CrownRoom74;
using M = Skygrind.Game.CrownRoom74Math;

namespace Skygrind.Game.Tests
{
    public class N74BCrownRoomTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        [SetUp]
        public void SetUp() { NightRows.ReelOverride.Clear(); NightRows.ForceOffFrom = NightRows.NoOff; C.Active = false; C.Seen = C.Changed = 0; }

        [TearDown]
        public void TearDown() { NightRows.ReelOverride.Clear(); NightRows.ForceOffFrom = NightRows.NoOff; C.Active = false; C.Seen = C.Changed = 0; }

        [Test]
        public void Row_ReadBothWays()
        {
            GameConfig c = Cfg();
            NightRows.ReelOverride[C.Row] = true;
            Assert.That(C.Decide(c), Is.True);
            NightRows.ReelOverride[C.Row] = false;
            Assert.That(C.Decide(c), Is.False);
            Assert.That(C.Decide(null), Is.False);
            NightRows.ReelOverride.Clear();
            Assert.That(C.Decide(c), Is.EqualTo(c.Has("design.n74.crownroom74.enabled") && c.Design("n74.crownroom74.enabled")), "the config's own answer (false until the lead adds and flips it)");
        }

        [Test]
        public void Inside_WebCoordinates_WithTheMargin()
        {
            // the spire's boss room rect (n52 / n67 spire v11): x -44..24, z -323..-255
            Assert.That(M.Inside(-10, -289, -44, -323, 24, -255, 0.75), Is.True, "the middle");
            Assert.That(M.Inside(24.5, -289, -44, -323, 24, -255, 0.75), Is.True, "half a metre past the east edge: inside the margin");
            Assert.That(M.Inside(25, -289, -44, -323, 24, -255, 0.75), Is.False, "a metre past it");
            Assert.That(M.Inside(-10, -254.5, -44, -323, 24, -255, 0.75), Is.True, "the south edge's margin");
            Assert.That(M.Inside(-10, -250, -44, -323, 24, -255, 0.75), Is.False, "the room before it (v10)");
            Assert.That(M.Inside(-10, 289, -44, -323, 24, -255, 0.75), Is.False, "the mirror image (a Unity z used as web z) is outside");
            Assert.That(M.Inside(-10, -289, 24, -255, -44, -323, 0.75), Is.True, "the corners in either order");
        }

        [Test]
        public void Wall_InactiveUntouched_ActiveReplacesInsideOnly()
        {
            var palette = new Vector3(1.23f, 0.93f, 2.92f);
            Assert.That(C.Wall(palette, new Vector3(-10, 5, 289)), Is.EqualTo(palette), "inactive: palette72's multiplier");
            Assert.That(C.Seen, Is.EqualTo(0), "and nothing counted");
            C.Active = true;
            C.X0 = -44; C.Z0 = -323; C.X1 = 24; C.Z1 = -255;
            Vector3 inside = C.Wall(palette, new Vector3(-10, 5, 289)); // Unity z 289 = web z -289
            Assert.That(inside.x, Is.EqualTo(UnityTuning.N74CrownRoomWallMul[0]));
            Assert.That(inside.z, Is.EqualTo(UnityTuning.N74CrownRoomWallMul[2]));
            Assert.That(C.ChangedFlipped, Is.EqualTo(1), "found by the flipped z (the stone is Unity space)");
            Assert.That(C.ChangedRaw, Is.EqualTo(0));
            // round 2: the raw sign is tried too (and counted apart), in case the stone is in web space; the spire has nothing standing where the other sign looks
            Vector3 raw = C.Wall(palette, new Vector3(-10, 5, -289));
            Assert.That(raw.z, Is.EqualTo(UnityTuning.N74CrownRoomWallMul[2]), "the raw sign inside the rect is taken too");
            Assert.That(C.ChangedRaw, Is.EqualTo(1));
            Assert.That(C.Wall(palette, new Vector3(-10, 5, 250)), Is.EqualTo(palette), "the room before the boss room (Unity z 250 = web z -250, web z 250 raw): palette72's");
            Assert.That(C.Wall(palette, new Vector3(-10, 5, 0)), Is.EqualTo(palette), "the middle of the level: palette72's");
            Assert.That(C.Wall(palette, new Vector3(60, 5, 289)), Is.EqualTo(palette), "east of the room: palette72's");
            Assert.That(C.Seen, Is.EqualTo(5));
            Assert.That(C.Changed, Is.EqualTo(2));
            Assert.That(C.SeenMinZ, Is.EqualTo(-289f));
            Assert.That(C.SeenMaxZ, Is.EqualTo(289f));
        }

        static System.Collections.Generic.Dictionary<string, object> Room(string kind, string boss, params double[] rect)
        {
            var rl = new System.Collections.Generic.List<object>();
            foreach (double d in rect) rl.Add(d);
            var r = new System.Collections.Generic.Dictionary<string, object> { { "kind", kind }, { "rect", rl } };
            if (boss != null) r["boss"] = boss;
            return r;
        }

        static System.Collections.Generic.Dictionary<string, object> Level(string name, params object[] rooms)
        {
            var camp = new System.Collections.Generic.Dictionary<string, object> { { "rooms", new System.Collections.Generic.List<object>(rooms) } };
            return new System.Collections.Generic.Dictionary<string, object> { { "name", name }, { "campaign", camp } };
        }

        [Test]
        public void BossRect_FromTheMapsOwnJson_NoCampaignRunNeeded()
        {
            var json = Level("n52-spire", Room("link", null, -17, 87, -3, 115), Room("boss", "wyrm", 0, 0, 10, 10), Room("boss", "crown37", -44, -323, 24, -255));
            Assert.That(M.BossRect(json, "crown37", out string name, out double x0, out double z0, out double x1, out double z1), Is.True);
            Assert.That(name, Is.EqualTo("n52-spire"));
            Assert.That((x0, z0, x1, z1), Is.EqualTo((-44.0, -323.0, 24.0, -255.0)), "the crown37 boss room's rect, not the other boss room's");
            Assert.That(M.BossRect(json, "wick", out _, out _, out _, out _, out _), Is.False, "no such boss");
            Assert.That(M.BossRect(Level("x", Room("link", "crown37", 0, 0, 1, 1)), "crown37", out _, out _, out _, out _, out _), Is.False, "a room of another kind is not the boss room");
            Assert.That(M.BossRect(Level("x", Room("boss", "crown37", 0, 0, 1)), "crown37", out _, out _, out _, out _, out _), Is.False, "a rect that is not four numbers");
            Assert.That(M.BossRect(new System.Collections.Generic.Dictionary<string, object> { { "name", "survival" } }, "crown37", out string n2, out _, out _, out _, out _), Is.False, "a map with no campaign (survival)");
            Assert.That(n2, Is.EqualTo("survival"), "the name is still handed back");
            Assert.That(M.BossRect(null, "crown37", out _, out _, out _, out _, out _), Is.False, "no map");
        }

        [Test]
        public void Begin_NoCampaign_StaysInactiveAndSays()
        {
            NightRows.ReelOverride[C.Row] = true;
            C.Begin(Cfg());
            Assert.That(C.Active, Is.False, "an EditMode run has no campaign level");
            Assert.That(C.Line, Does.Contain("crownroom74"));
            Assert.That(C.Line, Does.Contain("no crown37 boss room"));
        }

        [Test]
        public void Numbers_TheirReasons()
        {
            float[] mul = UnityTuning.N74CrownRoomWallMul, pal = { UnityTuning.Palette72WallMul[4 * 3], UnityTuning.Palette72WallMul[4 * 3 + 1], UnityTuning.Palette72WallMul[4 * 3 + 2] };
            Assert.That(UnityTuning.Palette72Levels[4], Is.EqualTo("spire"));
            double w = M.Weight(mul[0], mul[1], mul[2]), wp = M.Weight(pal[0], pal[1], pal[2]);
            Assert.That(w, Is.EqualTo(wp).Within(wp * 0.05), "the replacement keeps palette72's luminance weight (" + w.ToString("0.00") + " against " + wp.ToString("0.00") + ")");
            // ROUND 2 (the t4 idle pair): the screen's sRGB channels scale linearly with the vertex multiplier. The t4 sample: OFF wall (79, 79, 118) under palette72's multiplier; round 1's uniform 1.15 read
            // (72, 96, 48): olive. The prediction for a multiplier is OFF x mul / palette; the retake's must be a desaturated slate (S under the meter's vivid floor 0.35, hue in the blue-slate arc, not olive)
            double[] off = { 79, 79, 118 };
            double[] r1 = { off[0] * 1.15 / pal[0], off[1] * 1.15 / pal[1], off[2] * 1.15 / pal[2] };
            Assert.That(r1[0], Is.EqualTo(72).Within(4), "the model reproduces round 1's red (74 predicted, 72 read)");
            Assert.That(r1[1], Is.EqualTo(96).Within(4), "and green (98 predicted, 96 read)");
            Assert.That(r1[2], Is.EqualTo(48).Within(4), "and blue (47 predicted, 48 read): olive");
            double[] now = { off[0] * mul[0] / pal[0], off[1] * mul[1] / pal[1], off[2] * mul[2] / pal[2] };
            double hue = Weak74Math.HueOf(now[0], now[1], now[2], out double sat);
            Assert.That(sat, Is.LessThan(0.20), "a desaturated slate, far under the vivid floor (S " + sat.ToString("0.00") + ")");
            Assert.That(hue >= 200 && hue <= 260, Is.True, "in the blue-slate arc, not olive (hue " + hue.ToString("0") + ")");
            double lumOff = 0.2126 * off[0] + 0.7152 * off[1] + 0.0722 * off[2], lumNow = 0.2126 * now[0] + 0.7152 * now[1] + 0.0722 * now[2];
            Assert.That(lumNow, Is.EqualTo(lumOff).Within(lumOff * 0.08), "at the wall's own brightness (" + lumNow.ToString("0") + " against " + lumOff.ToString("0") + ")");
            // the lighter wall of the frame, OFF (118, 110, 141): a grey, never a vivid family
            hue = Weak74Math.HueOf(118 * mul[0] / pal[0], 110 * mul[1] / pal[1], 141 * mul[2] / pal[2], out sat);
            Assert.That(sat, Is.LessThan(0.20), "the lighter walls land on a grey too (S " + sat.ToString("0.00") + ")");
            Assert.That(UnityTuning.N74CrownRoomMarginM, Is.GreaterThan(0.0));
        }

        [Test]
        public void SkyBlend_StepsInAndOut_AndHoldsInAHeldFrame()
        {
            float inS = UnityTuning.N74CrownSkyInS, outS = UnityTuning.N74CrownSkyOutS;
            Assert.That(CrownSky74Math.Step(0f, true, 0f, inS, outS), Is.EqualTo(0f), "a held frame (dt 0) holds it");
            float b = 0f;
            for (int i = 0; i < 60; i++) b = CrownSky74Math.Step(b, true, 1f / 60f, inS, outS);
            Assert.That(b, Is.EqualTo(Math.Min(1f, 1f / inS)).Within(1e-4), "one second of fight: 1 / fadeIn");
            for (int i = 0; i < 120; i++) b = CrownSky74Math.Step(b, true, 1f / 60f, inS, outS);
            Assert.That(b, Is.EqualTo(1f), "and no further than 1");
            for (int i = 0; i < 60; i++) b = CrownSky74Math.Step(b, false, 1f / 60f, inS, outS);
            Assert.That(b, Is.EqualTo(1f - 1f / outS).Within(1e-4), "one second after the fight: 1 - 1 / fadeOut (slower out than in)");
            for (int i = 0; i < 300; i++) b = CrownSky74Math.Step(b, false, 1f / 60f, inS, outS);
            Assert.That(b, Is.EqualTo(0f), "and no further than 0");
            Assert.That(outS, Is.GreaterThan(inS), "a room's look arrives faster than it leaves");
            Assert.That(CrownSky74Math.Smooth(0f), Is.EqualTo(0f));
            Assert.That(CrownSky74Math.Smooth(1f), Is.EqualTo(1f));
            Assert.That(CrownSky74Math.Smooth(0.5f), Is.EqualTo(0.5f).Within(1e-6));
            Assert.That(CrownSky74Math.DimRatio(0.3f, 0.6f), Is.EqualTo(0.5f).Within(1e-6), "PostFx's fog dimming as a ratio");
            Assert.That(CrownSky74Math.DimRatio(0.3f, 0f), Is.EqualTo(1f), "a dark sky: no ratio");
            Assert.That(CrownSky74Math.DimRatio(5f, 0.5f), Is.EqualTo(1.5f), "clamped");
        }

        [Test]
        public void SkyColours_SlateAndIvory_OutOfEveryRoleAndUnderTheVividFloor()
        {
            foreach (uint hex in new[] { UnityTuning.N74CrownSkyZenithHex, UnityTuning.N74CrownSkyMidHex, UnityTuning.N74CrownSkyHorizonHex })
            {
                var (r, g, b) = GameConfig.Hex(hex);
                double hue = Weak74Math.HueOf(r, g, b, out double sat);
                string who = "#" + hex.ToString("X6") + " hue " + hue.ToString("0") + " S " + sat.ToString("0.00");
                Assert.That(sat, Is.LessThan(0.35), "under the meter's vivid floor: " + who);
                Assert.That(Weak74Math.IsCyanish(r, g, b, UnityTuning.N74CyanHueLo, UnityTuning.N74CyanHueHi, UnityTuning.N74CyanSatMin), Is.False, "not cyan: " + who);
                Assert.That(hue >= 230 && hue <= 344 && sat >= 0.35, Is.False, "not violet or magenta: " + who);
            }
            var (zr, zg, zb) = GameConfig.Hex(UnityTuning.N74CrownSkyZenithHex);
            var (mr, mg, mb) = GameConfig.Hex(UnityTuning.N74CrownSkyMidHex);
            var (hr, hg, hb) = GameConfig.Hex(UnityTuning.N74CrownSkyHorizonHex);
            Assert.That(CrownSky74Math.Lum(new Color(zr, zg, zb)), Is.LessThan(CrownSky74Math.Lum(new Color(mr, mg, mb))), "zenith darker than mid");
            Assert.That(CrownSky74Math.Lum(new Color(mr, mg, mb)), Is.LessThan(CrownSky74Math.Lum(new Color(hr, hg, hb))), "mid darker than the horizon: a sky that brightens toward its edge, as the dusk's does");
        }
    }
}
