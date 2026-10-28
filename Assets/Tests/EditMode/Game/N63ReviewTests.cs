// NIGHT63 builder V (the night's review, Logs/n63/review-n63.md; branch n63-RV): two should-fix items in builder V's code.
//   S1: with bossMotion63 on, Brawl40View.N63V.cs rebuilt THE ICEBREAKER's position from its feet (`bodyRoot.position = fu + m.offset + ...`),
//       wiping what the view placed before it this frame: Render42's leap height (Brawl40View.N42.cs: "the body rides the leap's height (the
//       sim's hit spheres do)") and Punch42's recoil (Brawl40View.N42.Hit.cs). The body was drawn on the floor while its hit spheres were in
//       the air (B1's GUARD: "the drawn body never leaves its collider"). Pinned: the layer starts from the transform the view already placed
//       (as the Wick's and the Crown's hooks do), position and rotation, and only adds its offset; Placed63 keeps a leap's height exactly.
//   S8: the boss63 reel's walk-in waited 3 s and 4 s of WALL time (copied from boss61), so a slow Player fit fewer fixed frames and could begin
//       the fight another way than a fast one (the ON and OFF sides of a pair too). Pinned: both windows on FeelClock.Now (the reel's fixed
//       clock), the wall clock only as the safety cap (wallEnd).
// Written test first: RED on its commit (Placed63 a placeholder; the old code); the Editor run is the lead's.
using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N63ReviewTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static string Src(string file) => File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", file));

        /// <summary>A method's text, from its signature to the next method's signature at the same indent (or the file's end).</summary>
        static string Method(string src, string signature)
        {
            int at = src.IndexOf(signature, StringComparison.Ordinal);
            Assert.That(at, Is.GreaterThanOrEqualTo(0), signature);
            int end = src.IndexOf("\n        void ", at + signature.Length, StringComparison.Ordinal);
            int end2 = src.IndexOf("\n        public ", at + signature.Length, StringComparison.Ordinal);
            if (end < 0 || (end2 >= 0 && end2 < end)) end = end2;
            return end < 0 ? src.Substring(at) : src.Substring(at, end - at);
        }

        [Test]
        public void S1_Placed63_KeepsTheLeapsHeightAndTheRecoil_AddsOnlyTheOffset()
        {
            Motion63 m = Motion63.Rest;
            m.on = true;
            m.offset = new Vector3(0.1f, -0.2f, 0.05f); // the layer's lag / breath with the slump's sink in it (the knees take the sink back)
            m.sinkM = 0.2f;
            Vector3 fu = new Vector3(3f, 0f, -5f);
            Vector3 leap = fu + Vector3.up * 2.5f; // Render42: the body on the leap's height
            Vector3 p = Brawl40View.Placed63(leap, m);
            Assert.That(p.y, Is.EqualTo(2.5f).Within(1e-5f), "the leap's height kept (the sink goes to the knees, not the body)");
            Assert.That(p.x, Is.EqualTo(3.1f).Within(1e-5f), "the layer's offset added");
            Assert.That(p.z, Is.EqualTo(-4.95f).Within(1e-5f));
            Vector3 recoil = fu + new Vector3(0.3f, 0f, -0.1f); // Punch42: the recoil on the feet
            Motion63 rest = Motion63.Rest;
            rest.on = true;
            Assert.That(Brawl40View.Placed63(recoil, rest), Is.EqualTo(recoil), "a layer at rest leaves the view's placement exactly");
        }

        [Test]
        public void S1_TheHook_StartsFromTheViewsPlacement_AfterRender42()
        {
            string src = Src("Brawl40View.N63V.cs");
            string body = Method(src, "void DrawMotion63(");
            int rigid = body.IndexOf("Vector3 rigid = bodyRoot.position;", StringComparison.Ordinal);
            int rigidRot = body.IndexOf("Quaternion rigidRot = bodyRoot.rotation;", StringComparison.Ordinal);
            Assert.That(rigid, Is.GreaterThanOrEqualTo(0), "the position the view placed this frame (the feet, the leap's height, the recoil)");
            Assert.That(rigidRot, Is.GreaterThanOrEqualTo(0), "and its rotation");
            int firstWrite = body.IndexOf("bodyRoot.", Math.Max(rigid, rigidRot) + 1, StringComparison.Ordinal);
            int scaleWrite = body.IndexOf("bodyRoot.localScale", StringComparison.Ordinal);
            Assert.That(rigid, Is.LessThan(scaleWrite), "read before the hook writes the transform");
            Assert.That(rigidRot, Is.LessThan(scaleWrite));
            Assert.That(firstWrite, Is.GreaterThan(0));
            Assert.That(body.Contains("bodyRoot.position = Placed63(rigid, m);"), Is.True, "the layer adds its offset to that placement");
            Assert.That(body.Contains("* rigidRot;"), Is.True, "the sway on the view's own rotation");
            Assert.That(body.Contains("fu + m.offset"), Is.False, "never rebuilt from the feet (S1)");
            string view = Src("Brawl40View.cs");
            int placed = view.IndexOf("bodyRoot.position = fu;", StringComparison.Ordinal);
            int r42 = view.IndexOf("Render42(g, st, atk, prog,", StringComparison.Ordinal);
            int m63 = view.IndexOf("DrawMotion63(g, ref leanDeg", StringComparison.Ordinal);
            Assert.That(placed, Is.GreaterThanOrEqualTo(0).And.LessThan(r42), "the view places the feet first");
            Assert.That(r42, Is.LessThan(m63), "Render42 (the leap, the recoil) before the motion layer");
        }

        [Test]
        public void S8_Boss63sWalkIn_OnTheReelsClock_TheWallOnlyTheCap()
        {
            string src = Src("FeelReel.N63V.cs");
            int walk = src.IndexOf("boss61's walk-in", StringComparison.Ordinal);
            Assert.That(walk, Is.GreaterThanOrEqualTo(0), "the walk-in");
            int end = src.IndexOf("FORCED BEGIN", walk, StringComparison.Ordinal);
            string w = src.Substring(walk, end - walk);
            Assert.That(w.Contains("Time.realtimeSinceStartup - t0"), Is.False, "no window on the wall clock");
            Assert.That(w.Contains("FeelClock.Now - t0 < 3.0 && Time.realtimeSinceStartup < wallEnd"), Is.True, "the room's own entry: 3 s on the reel's clock, the wall the cap");
            Assert.That(w.Contains("FeelClock.Now - t0 < 4.0 && Time.realtimeSinceStartup < wallEnd"), Is.True, "the walk: 4 s on the reel's clock, the wall the cap");
        }
    }
}
