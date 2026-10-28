// NIGHT72 builder C (slate C2, design.n72.palette72; Game/Palette72.cs, UnityTuning.N72C.cs). Pinned:
//   1. a vertex's multiplier by class: a wall (a side or an underside) takes the wall's, a flat top the floor's, a ramp the floor's x the slope lift,
//      a deck (a flat top over DeckMinY) x the deck lift, a pit's floor x the pit's dark; a level with no palette is x1
//   2. the design rule on the baseline frames' own colours (the n71-forge72 tour medians, Tools/night/n72-c-calibrate.py): after the multiplier the
//      floor is a warm-neutral (hue 30-46, never in the danger band 345-35 at a vivid saturation, never in the parry arc 45-70 at S over 0.5) and the
//      wall is in no role hue (cyan 160-205, danger 345-35, parry 45-70, magenta 285-335), on all five levels; and the floor differs from the wall
//      in hue by at least 60 degrees (two families, never one tint)
//   3. the one accent: no palette accent is a role hue or the boundary's violet band (a pale one is told by saturation)
//   4. the stone round trip: the row reading off writes the colours as they came; reading on multiplies them; reading off again divides them back
//      (every channel within 1e-4), the walls' and floors' own multipliers; a level with no palette leaves the mesh alone
//   5. the accent pieces: found by their colour after decor54's second pass, moved to the level's accent at their own luminance while the row reads
//      on, back to their own colour when it reads off; a colour someone else changed since the scan is left alone (counted lost)
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using P = Skygrind.Game.Palette72;

namespace Skygrind.Game.Tests
{
    public class Palette72Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg() => cfg ??= GameConfig.Load(Path.Combine(PortRoot(), "config.json"));

        static string PortRoot()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        [SetUp]
        public void SetUp() => NightRows.ReelOverride.Clear();

        [TearDown]
        public void TearDown() => NightRows.ReelOverride.Clear();

        static float Gap(float a, float b)
        {
            float d = Mathf.Abs(a - b) % 360f;
            return d > 180f ? 360f - d : d;
        }

        static bool InArc(float h, float lo, float hi) => lo <= hi ? h >= lo && h <= hi : h >= lo || h <= hi;

        /// <summary>A material's colour round-trips through the engine (a non-HDR colour property may be stored in the other colour space): equal to within 1e-3 a channel.</summary>
        static void Near(Color a, Color b, string why)
        {
            Assert.That(Mathf.Max(Mathf.Abs(a.r - b.r), Mathf.Abs(a.g - b.g), Mathf.Abs(a.b - b.b), Mathf.Abs(a.a - b.a)), Is.LessThan(1e-3f), why + ": " + a + " vs " + b);
        }

        [Test]
        public void Multiplier_ByClass()
        {
            int sp = P.LevelIndex("spire"), fo = P.LevelIndex("foundry");
            Assert.That(sp, Is.GreaterThanOrEqualTo(0));
            Vector3 floor = P.Multiplier(sp, 1f, 0f, out string c0);
            Assert.That(c0, Is.EqualTo("floor"));
            Assert.That(floor.x, Is.EqualTo(UnityTuning.Palette72FloorMul[sp * 3]).Within(1e-5f), "a flat top at the floor's height is the floor's multiplier");
            Vector3 slope = P.Multiplier(sp, 0.8f, 1f, out string c1);
            Assert.That(c1, Is.EqualTo("slope"));
            Assert.That(slope.y / floor.y, Is.EqualTo(UnityTuning.Palette72SlopeLift).Within(1e-4f), "a ramp is lifted against the flat floor");
            Vector3 deck = P.Multiplier(sp, 1f, 3f, out string c2);
            Assert.That(c2, Is.EqualTo("deck"));
            Assert.That(deck.z / floor.z, Is.EqualTo(UnityTuning.Palette72DeckLift).Within(1e-4f));
            Vector3 pit = P.Multiplier(sp, 1f, -5f, out string c3);
            Assert.That(c3, Is.EqualTo("pit"));
            Assert.That(pit.x / floor.x, Is.EqualTo(UnityTuning.Palette72PitDark).Within(1e-4f), "a pit's floor is darker");
            Vector3 wall = P.Multiplier(sp, 0f, 4f, out string c4);
            Assert.That(c4, Is.EqualTo("wall"));
            Assert.That(wall.x, Is.EqualTo(UnityTuning.Palette72WallMul[sp * 3]).Within(1e-5f));
            P.Multiplier(sp, -1f, 9f, out string c5);
            Assert.That(c5, Is.EqualTo("wall"), "an underside takes the wall's multiplier");
            Vector3 none = P.Multiplier(-1, 1f, 0f, out string c6);
            Assert.That(c6, Is.EqualTo("none"));
            Assert.That(none, Is.EqualTo(Vector3.one), "a level with no palette: x1");
            Assert.That(P.LevelIndex("finale"), Is.EqualTo(-1), "the finale keeps its stone");
            Assert.That(fo, Is.GreaterThanOrEqualTo(0));
        }

        // the n71-forge72 baseline's median floor and wall colours per level (Research/2026-10-27-wave72/C/calibration.txt, Tools/night/n72-c-calibrate.py)
        static readonly (string key, int[] floor, int[] wall)[] Baseline =
        {
            ("foundry", new[] { 137, 128, 84 }, new[] { 38, 55, 41 }),
            ("iceline", new[] { 79, 97, 137 }, new[] { 27, 43, 71 }),
            ("kiln", new[] { 157, 119, 101 }, new[] { 55, 50, 69 }),
            ("span", new[] { 125, 109, 155 }, new[] { 62, 56, 94 }),
            ("spire", new[] { 92, 115, 40 }, new[] { 17, 43, 15 }),
        };

        static Color Predict(int[] srgb, Vector3 m)
        {
            // the multiplier acts on linear albedo: sRGB moves by m ^ (1 / 2.2) per channel
            return new Color(Mathf.Clamp01(srgb[0] / 255f * Mathf.Pow(m.x, 1f / 2.2f)), Mathf.Clamp01(srgb[1] / 255f * Mathf.Pow(m.y, 1f / 2.2f)), Mathf.Clamp01(srgb[2] / 255f * Mathf.Pow(m.z, 1f / 2.2f)), 1f);
        }

        [Test]
        public void TheDesignRule_OnTheBaselineColours()
        {
            foreach (var (key, floor, wall) in Baseline)
            {
                int li = P.LevelIndex(key);
                Assert.That(li, Is.GreaterThanOrEqualTo(0), key);
                Color f = Predict(floor, P.Multiplier(li, 1f, 0f, out _)), w = Predict(wall, P.Multiplier(li, 0f, 4f, out _));
                Color.RGBToHSV(f, out float fh, out float fs, out _);
                Color.RGBToHSV(w, out float wh, out float ws, out _);
                fh *= 360f;
                wh *= 360f;
                Assert.That(fh, Is.InRange(30f, 46f), $"{key}: the floor is a warm-neutral sand (hue {fh:0}, S {fs:0.00})");
                Assert.That(fs, Is.InRange(0.18f, 0.40f), $"{key}: a floor at S {fs:0.00} reads a hue without a vivid one");
                Assert.That(InArc(wh, 345f, 35f) || InArc(wh, 45f, 70f) || InArc(wh, 160f, 205f) || InArc(wh, 285f, 335f), Is.False, $"{key}: the wall's hue {wh:0} is in no role band");
                Assert.That(Gap(fh, wh), Is.GreaterThanOrEqualTo(60f), $"{key}: floor hue {fh:0} and wall hue {wh:0} are two families");
            }
        }

        [Test]
        public void TheOneAccent_IsNoRoleHue()
        {
            uint[] roles = { UnityTuning.Look49UsableHex, UnityTuning.Look49DangerHex, UnityTuning.Look49ParryHex, UnityTuning.Look49RivalHex };
            Assert.That(UnityTuning.Palette72Accent.Length, Is.EqualTo(UnityTuning.Palette72Levels.Length), "one accent entry per level");
            Color.RGBToHSV(Look.Hex(UnityTuning.Look49BoundaryHex), out float bh, out _, out _);
            for (int i = 0; i < UnityTuning.Palette72Accent.Length; i++)
            {
                uint hex = UnityTuning.Palette72Accent[i];
                if (hex == 0) continue;
                Color.RGBToHSV(Look.Hex(hex), out float h, out float s, out _);
                h *= 360f;
                if (s < UnityTuning.Decor54PaleSat) Assert.That(Gap(h, bh * 360f), Is.GreaterThan(UnityTuning.Decor54CyanBandDeg), UnityTuning.Palette72Levels[i] + ": a pale accent is never the boundary's violet");
                else foreach (uint r in roles)
                {
                    Color.RGBToHSV(Look.Hex(r), out float rh, out _, out _);
                    Assert.That(Gap(h, rh * 360f), Is.GreaterThanOrEqualTo(25f), $"{UnityTuning.Palette72Levels[i]}: accent hue {h:0} vs role #{r:x6}");
                }
            }
            // the spire's accent is what it replaces: colour56's rose sat in the danger band
            Color.RGBToHSV(Look.Hex((uint)Cfg().Num("design.n56.colour56.numbers.levels.spire.accentHex")), out float rose, out _, out _);
            // hue 344.9 (0xe67793): on the band's edge, 0.1 deg outside it, so the reach (the band widened by the margin) is the test, as colours63's C1 reads it
            float m = (float)UnityTuning.Colours72DangerMarginDeg;
            Assert.That(InArc(rose * 360f, 345f - m, 35f + m), Is.True, "the spire's colour56 accent (rose, hue " + rose * 360f + ") is in the danger band's reach: that is why palette72 replaces it");
        }

        static Mesh QuadMesh(out Transform t, out Color[] colours, out Vector3[] v, out Vector3[] n)
        {
            var go = new GameObject("Palette72 test stone");
            t = go.transform;
            v = new[]
            {
                new Vector3(0, 0, 0), new Vector3(4, 0, 0), new Vector3(4, 0, 4), new Vector3(0, 0, 4), // a floor (normal up, y 0)
                new Vector3(0, 0, 0), new Vector3(4, 0, 0), new Vector3(4, 5, 0), new Vector3(0, 5, 0), // a wall (normal -z)
                new Vector3(0, 3, 0), new Vector3(4, 3, 0), new Vector3(4, 3, 4), new Vector3(0, 3, 4), // a deck (normal up, y 3)
            };
            n = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up, -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward, Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            colours = new Color[12];
            for (int i = 0; i < 12; i++) colours[i] = new Color(0.20f + 0.01f * i, 0.30f, 0.15f, 1f);
            var m = new Mesh { name = "Palette72 test stone", vertices = v, normals = n, colors = colours };
            return m;
        }

        [Test]
        public void StoneRoundTrip_OffOnOff()
        {
            Mesh mesh = QuadMesh(out Transform t, out Color[] colours, out Vector3[] v, out Vector3[] n);
            try
            {
                int li = P.LevelIndex("spire");
                NightRows.ReelOverride["palette72"] = false;
                Color[] first = P.Offer(Cfg(), mesh, t, "spire", (Color[])colours.Clone(), v, n);
                for (int i = 0; i < colours.Length; i++) Assert.That(first[i], Is.EqualTo(colours[i]), "row off: the colours as they came, vertex " + i);
                Assert.That(P.Holds(mesh), Is.True, "Yellow56Floors stands down on a handled mesh");

                NightRows.ReelOverride["palette72"] = true;
                bool wrote = P.Tick(Cfg(), mesh, 0.016f, out Color[] now);
                Assert.That(wrote, Is.True, "the row reading on writes the stone");
                Assert.That(now[0].g, Is.EqualTo(colours[0].g * UnityTuning.Palette72FloorMul[li * 3 + 1]).Within(1e-4f), "a floor vertex: the floor's multiplier on green");
                Assert.That(now[4].r, Is.EqualTo(colours[4].r * UnityTuning.Palette72WallMul[li * 3]).Within(1e-4f), "a wall vertex: the wall's multiplier on red");
                Assert.That(now[8].g, Is.EqualTo(colours[8].g * UnityTuning.Palette72FloorMul[li * 3 + 1] * UnityTuning.Palette72DeckLift).Within(1e-4f), "a deck vertex: the floor's x the deck lift");
                Assert.That(P.Tick(Cfg(), mesh, 0.016f, out _), Is.False, "no change, no write");

                NightRows.ReelOverride["palette72"] = false;
                Assert.That(P.Tick(Cfg(), mesh, 0.016f, out Color[] back), Is.True, "the row reading off writes the stone back");
                for (int i = 0; i < colours.Length; i++)
                {
                    Assert.That(back[i].r, Is.EqualTo(colours[i].r).Within(1e-4f), "vertex " + i + " r");
                    Assert.That(back[i].g, Is.EqualTo(colours[i].g).Within(1e-4f), "vertex " + i + " g");
                    Assert.That(back[i].b, Is.EqualTo(colours[i].b).Within(1e-4f), "vertex " + i + " b");
                }
            }
            finally
            {
                Object.DestroyImmediate(t.gameObject);
                Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void StoneRoundTrip_BothRows_ComposeExactly()
        {
            Mesh mesh = QuadMesh(out Transform t, out Color[] colours, out Vector3[] v, out Vector3[] n);
            try
            {
                // the wall quad (vertices 4-7) is a 5 m face: uv3 (u, v up from its foot, width, height)
                var uv3 = new List<Vector4>();
                for (int i = 0; i < 12; i++) uv3.Add(Vector4.zero);
                uv3[4] = new Vector4(0, 0, 4, 5); uv3[5] = new Vector4(4, 0, 4, 5); uv3[6] = new Vector4(4, 5, 4, 5); uv3[7] = new Vector4(0, 5, 4, 5);
                mesh.SetUVs(3, uv3);
                int li = P.LevelIndex("iceline");
                NightRows.ReelOverride["palette72"] = false;
                NightRows.ReelOverride["decor72"] = false;
                Color[] off = P.Offer(Cfg(), mesh, t, "iceline", (Color[])colours.Clone(), v, n);
                Assert.That(off[4], Is.EqualTo(colours[4]), "both rows off: the colours as they came");

                NightRows.ReelOverride["decor72"] = true;
                Assert.That(P.Tick(Cfg(), mesh, 0.016f, out Color[] dec), Is.True, "decor72 reading on writes the stone");
                Assert.That(dec[4].r, Is.EqualTo(colours[4].r * UnityTuning.Decor72PlinthMul).Within(1e-5f), "the wall's foot x the plinth");
                Assert.That(dec[6].r, Is.EqualTo(colours[6].r * UnityTuning.Decor72UpperMul).Within(1e-5f), "the wall's top x the upper lift");
                Assert.That(dec[0], Is.EqualTo(colours[0]), "a floor vertex is not touched by decor72's value structure");

                NightRows.ReelOverride["palette72"] = true;
                Assert.That(P.Tick(Cfg(), mesh, 0.016f, out Color[] both), Is.True, "palette72 on top");
                float wallR = UnityTuning.Palette72WallMul[li * 3];
                Assert.That(both[6].r, Is.EqualTo(colours[6].r * wallR * UnityTuning.Decor72UpperMul).Within(1e-5f), "the wall's top: the palette's x the value structure");
                Assert.That(both[0].g, Is.EqualTo(colours[0].g * UnityTuning.Palette72FloorMul[li * 3 + 1]).Within(1e-5f), "the floor: the palette's only");

                NightRows.ReelOverride["decor72"] = false;
                Assert.That(P.Tick(Cfg(), mesh, 0.016f, out Color[] palOnly), Is.True);
                Assert.That(palOnly[6].r, Is.EqualTo(colours[6].r * wallR).Within(1e-5f), "decor72 off again: the palette alone");

                NightRows.ReelOverride["palette72"] = false;
                Assert.That(P.Tick(Cfg(), mesh, 0.016f, out Color[] back), Is.True);
                for (int i = 0; i < colours.Length; i++) Assert.That(back[i], Is.EqualTo(colours[i]), "both off again: exactly the original, vertex " + i);
            }
            finally
            {
                Object.DestroyImmediate(t.gameObject);
                Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void ALevelWithNoPalette_LeavesTheStoneAlone()
        {
            Mesh mesh = QuadMesh(out Transform t, out Color[] colours, out Vector3[] v, out Vector3[] n);
            try
            {
                NightRows.ReelOverride["palette72"] = true;
                Color[] shown = P.Offer(Cfg(), mesh, t, "finale", (Color[])colours.Clone(), v, n);
                for (int i = 0; i < colours.Length; i++) Assert.That(shown[i], Is.EqualTo(colours[i]), "finale vertex " + i);
                Assert.That(P.Tick(Cfg(), mesh, 0.016f, out _), Is.False, "nothing to flip");
            }
            finally
            {
                Object.DestroyImmediate(t.gameObject);
                Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void TheAccentPieces_FoundByColour_FlippedBothWays()
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
            if (sh == null) Assert.Ignore("the URP Unlit shader is not loaded in this run");
            var root = new GameObject("Palette72 test map");
            var mats = new System.Collections.Generic.List<Material>();
            try
            {
                Color rose = Look.Hex(0xE67793);
                var strip = new Material(sh) { name = "rose strip" };
                strip.SetColor("_BaseColor", rose * 0.5f);
                var cyan = new Material(sh) { name = "cyan strip" };
                cyan.SetColor("_BaseColor", Look.Hex(UnityTuning.Look49UsableHex));
                mats.Add(strip);
                mats.Add(cyan);
                foreach (Material m in new[] { strip, cyan })
                {
                    var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.SetParent(root.transform);
                    c.GetComponent<MeshRenderer>().sharedMaterial = m;
                }
                NightRows.ReelOverride["palette72"] = true;
                P.BeginAccent(root, "spire", rose);
                P.Tick(Cfg(), null, 1f, out _); // past AccentScanS: the scan, then the flip on
                Assert.That(P.AccentFound, Is.EqualTo(1), "the rose piece is found; the usable cyan is not an accent");
                Color on = strip.GetColor("_BaseColor");
                Color.RGBToHSV(new Color(on.r / Mathf.Max(on.r, on.g, on.b), on.g / Mathf.Max(on.r, on.g, on.b), on.b / Mathf.Max(on.r, on.g, on.b)), out float h, out _, out _);
                Assert.That(Gap(h * 360f, 42f), Is.LessThan(8f), "the piece wears the spire's bone accent (hue 42), hue " + h * 360f);
                Assert.That(Decor54.Lum(on), Is.EqualTo(Decor54.Lum(rose * 0.5f)).Within(0.02f), "at its own luminance");
                Near(cyan.GetColor("_BaseColor"), Look.Hex(UnityTuning.Look49UsableHex), "the usable cyan is untouched");
                NightRows.ReelOverride["palette72"] = false;
                P.Tick(Cfg(), null, 0.016f, out _);
                Near(strip.GetColor("_BaseColor"), rose * 0.5f, "the row reading off: the piece's own colour back");
                strip.SetColor("_BaseColor", Color.green); // someone else changes it
                NightRows.ReelOverride["palette72"] = true;
                P.Tick(Cfg(), null, 0.016f, out _);
                Near(strip.GetColor("_BaseColor"), Color.green, "a colour someone else changed since the scan is left alone");
                Assert.That(P.AccentLost, Is.EqualTo(1));
            }
            finally
            {
                P.BeginAccent(null, "", Color.white);
                foreach (Material m in mats) Object.DestroyImmediate(m);
                Object.DestroyImmediate(root);
            }
        }
    }
}
