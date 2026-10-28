// NIGHT63 builder V (C2b, the lead's fix round after C2's gate FAIL, Logs/n63/verdict-C2.md section 6): nearFade63's near enemy must read as
// a deliberate see-through, not a shatter. The critic, from b5's near63 frames: at 0.5 m and 0.3 m the husk "reads as noise ... an exploding
// cloud of sheared flakes ... many of them BLACK (inner / back faces drawn dark)"; with kill55 on a live enemy at the eye may read as a kill
// shattering; and the 1.0 m pair moved 63,402 px at dissolve 0.99 (the fade already shows at the punch range's edge).
// The causes (read in Assets/Shaders/Enemy.shader): C2 drove the death's own dissolve (_Dissolve: 3D noise cells of 2.5 cm, sheared into
// quads of many sizes on a curved body), and the ink51 outline (an inverted hull drawn with Cull Front, dark ink) showed its far side through
// every hole: the black flakes.
// C2b: a SCREEN-DOOR, its own property (_NearFade, 1 whole .. 0 gone), never the death's dissolve:
//   1. the pattern: a 4 x 4 ordered (Bayer) dither in screen pixels (numbers.cellPx a cell at 1080p): even, small cells; every 4 x 4 tile keeps
//      exactly round(level x 16) cells, each 2 x 2 quarter within one of a quarter of them (no clumps); a level of 31 / 32 or more keeps all
//   2. the body keeps its silhouette: a pixel whose surface is at the rim (N.V under numbers.rimNdv) is never cut, so the enemy reads as a
//      see-through shape with its edge, where it is
//   3. no black: the ink outline is cut by the same screen-door at the same pixel as the body (never seen through a hole) and thins to nothing
//      at numbers.inkGone; the death's _Dissolve, its edge band and its edge colour are left to the kill
//   4. 1.0 m is whole: the repo's numbers give exactly 1 at 1.0 m (the punch reach is 2.0 m) and the screen-door keeps every pixel at 1
//   5. the hook: EnemyLook.Tint sets the part's _NearFade (not _Dissolve, not _DissolveBand); EnemyLook.Frame sets the screen-door's globals
//      once a frame while the row is on
// Written test first: RED on its commit (NearFade63's placeholders: Bayer 0, Kept false; no numbers; no shader property; the old hook); the
// Editor run is the lead's.
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using N = Skygrind.Game.NearFade63;

namespace Skygrind.Game.Tests
{
    public class N63ScreenDoorTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        [Test]
        public void Pattern_AnOrderedDither_EvenSmallCells_NoClumps()
        {
            var seen = new bool[16];
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                {
                    float t = N.Bayer(x, y);
                    int m = Mathf.RoundToInt(t * 16f - 0.5f);
                    Assert.That(t * 16f - 0.5f, Is.EqualTo(m).Within(1e-4f), "a threshold is (m + 0.5) / 16");
                    Assert.That(m, Is.InRange(0, 15));
                    Assert.That(seen[m], Is.False, "each of the 16 thresholds once in a 4 x 4 tile");
                    seen[m] = true;
                    Assert.That(N.Bayer(x + 4, y + 8), Is.EqualTo(t), "the tile repeats");
                }
            for (int k = 0; k <= 16; k++)
            {
                float level = k / 16f;
                int kept = 0;
                for (int y = 0; y < 4; y++)
                    for (int x = 0; x < 4; x++)
                        kept += N.Kept(level, x, y) ? 1 : 0;
                Assert.That(kept, Is.EqualTo(k), "level " + k + " / 16 keeps exactly " + k + " cells of a tile");
                for (int qy = 0; qy < 4; qy += 2)
                    for (int qx = 0; qx < 4; qx += 2)
                    {
                        int q = 0;
                        for (int y = qy; y < qy + 2; y++)
                            for (int x = qx; x < qx + 2; x++)
                                q += N.Kept(level, x, y) ? 1 : 0;
                        Assert.That(Math.Abs(q * 4 - k), Is.LessThanOrEqualTo(4), "level " + k + " / 16: each 2 x 2 quarter keeps within one of a quarter (no clumps)");
                    }
            }
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                {
                    Assert.That(N.Kept(31f / 32f, x, y), Is.True, "31 / 32 keeps every cell");
                    Assert.That(N.Kept(1f, x, y), Is.True, "1 keeps every cell");
                    Assert.That(N.Kept(0f, x, y), Is.False, "0 keeps none");
                }
        }

        [Test]
        public void Numbers_OneMetreWhole_TheScreenDoorsNumbersSane()
        {
            N.Params p = N.Params.From(Cfg());
            Assert.That(p.Valid, Is.True);
            var husk = new Bounds(new Vector3(0, 0.9f, 5), new Vector3(0.8f, 1.8f, 0.8f));
            Vector3 at1 = new Vector3(0, 1.6f, 5 - 0.4f - 1.0f);
            Assert.That(N.Dissolve(p, husk, at1), Is.EqualTo(1f), "1.0 m from the body: exactly whole (the critic's 1 % fade at the punch range's edge)");
            Assert.That(p.farM, Is.LessThanOrEqualTo(1f));
            Assert.That(p.floor, Is.InRange(0.15f, 0.5f), "at the eye a sparse but regular screen, never gone");
            Assert.That(p.cellPx, Is.InRange(1f, 4f), "small cells (px at 1080p)");
            Assert.That(p.rimNdv, Is.GreaterThan(0f).And.LessThanOrEqualTo(0.5f), "the rim kept: a thin edge, not the body");
            Assert.That(p.inkGone, Is.GreaterThan(p.floor).And.LessThan(1f), "the ink gone before the eye reaches the floor");
            Vector4 g = N.Globals(p);
            Assert.That(g.x, Is.EqualTo(p.cellPx));
            Assert.That(g.y, Is.EqualTo(p.rimNdv));
            Assert.That(g.z, Is.EqualTo(p.inkGone));
        }

        static string Shader() => File.ReadAllText(Path.Combine(Repo(), "Assets", "Shaders", "Enemy.shader"));

        /// <summary>The text of the pass named `name` (from its Name line to the next ENDHLSL).</summary>
        static string PassText(string src, string name)
        {
            int at = src.IndexOf("Name \"" + name + "\"", StringComparison.Ordinal);
            Assert.That(at, Is.GreaterThanOrEqualTo(0), "the pass " + name);
            int end = src.IndexOf("ENDHLSL", at, StringComparison.Ordinal);
            return src.Substring(at, end - at);
        }

        [Test]
        public void Shader_TheScreenDoorItsOwnProperty_InEveryDrawnPass_TheRimKept_NoInkThroughAHole()
        {
            string s = Shader();
            Assert.That(s.Contains("_NearFade (\""), Is.True, "_NearFade, a property of its own");
            int cb = s.IndexOf("CBUFFER_START(UnityPerMaterial)", StringComparison.Ordinal), cbEnd = s.IndexOf("CBUFFER_END", StringComparison.Ordinal);
            Assert.That(s.Substring(cb, cbEnd - cb).Contains("float _NearFade;"), Is.True, "in the per-material buffer (every pass reads the same)");
            int fn = s.IndexOf("void NearClip(", StringComparison.Ordinal);
            Assert.That(fn, Is.GreaterThanOrEqualTo(0), "NearClip");
            int fnEnd = s.IndexOf("\n        }", fn, StringComparison.Ordinal);
            string clip = s.Substring(fn, fnEnd - fn);
            Assert.That(clip.Contains("NearDither("), Is.True, "the screen-door's threshold");
            Assert.That(clip.Contains("_Dissolve") || clip.Contains("_EdgeColor") || clip.Contains("DissolveNoise"), Is.False, "never the death's dissolve");
            int dith = s.IndexOf("float NearDither(", StringComparison.Ordinal);
            Assert.That(dith, Is.GreaterThanOrEqualTo(0), "NearDither");
            string body = PassText(s, "UniversalForwardOnly"), ink = PassText(s, "UniversalForward"), depth = PassText(s, "DepthOnly"), normals = PassText(s, "DepthNormalsOnly");
            Assert.That(body.Contains("NearClip(i.positionCS, ndv, 1)"), Is.True, "the body: cut, its rim kept");
            Assert.That(depth.Contains("NearClip(") && normals.Contains("NearClip("), Is.True, "the depth passes cut the same (SSAO matches what is drawn)");
            Assert.That(ink.Contains("NearClip(i.positionCS, 1, 0)"), Is.True, "the ink: cut by the same screen-door, no rim kept (a hull's far side is all rim)");
            Assert.That(ink.Contains("InkNearK("), Is.True, "the ink thins with the fade and is gone at numbers.inkGone");
            string xray = PassText(s, "SRPDefaultUnlit");
            Assert.That(xray.Contains("NearClip("), Is.False, "the x-ray pass is another enemy's view: untouched");
        }

        [Test]
        public void Hook_TintSetsTheScreenDoor_NotTheDeathsDissolve_FrameSetsTheGlobals()
        {
            string src = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "EnemyViews.cs"));
            int tint = src.IndexOf("public void Tint(MaterialPropertyBlock block, Renderer r, Color c, float flash, float rim)", StringComparison.Ordinal);
            int end = src.IndexOf("public void Frame()", tint, StringComparison.Ordinal);
            string body = src.Substring(tint, end - tint);
            Assert.That(body.Contains("block.SetFloat(NearFade63.NearFadeId, near63)"), Is.True, "Tint sets the part's _NearFade every time (the block is shared)");
            Assert.That(body.Contains("block.SetFloat(Dissolve,") || body.Contains("block.SetFloat(DissolveBand,"), Is.False, "the death's dissolve and edge band are the kill's");
            int frame = src.IndexOf("public void Frame()", StringComparison.Ordinal);
            string frameBody = src.Substring(frame, Math.Min(3000, src.Length - frame));
            Assert.That(frameBody.Contains("Shader.SetGlobalVector(NearFade63.GlobalId, NearFade63.Globals(near63P))"), Is.True, "the screen-door's globals once a frame");
        }
    }
}
