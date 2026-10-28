// NIGHT63 builder V (the lead's real-play bug: THE ICEBREAKER covers the whole screen when the player is close; view row design.n63.nearFade63,
// the same rule as C2: an enemy never fills the screen from inside). Measured red (read only): boss63 on n52-iceline, every build b1-b4, the
// flinch and stagger frames with the row off are black but for the HUD and the gauntlet (Logs/feel/n63-b4/b63iS-off/boss63/
// shot-N30-B63-brawl40-stagger.png). After its leap lands on the player (nothing in the sim keeps the player out of its body) the camera sits
// about 1.7 m from its centre, and the stuck pose leans the torso 50 deg toward the player: the dark violet torso and hump boxes
// (Brawl40BodyHex 0x2a2438 / 0x1a1622) at or around the eye. Its parts draw with the Stone / URP Lit material (no dissolve), so the rule here
// HIDES a body part whose world bounds come within numbers.bossHideM of the eye, and draws it again once the eye is clear. Pinned:
//   1. NearFade63.Hide: true with the eye within bossHideM of the bounds (inside: 0 m), false farther; false with no or invalid numbers
//   2. the row's numbers carry bossHideM (sane)
//   3. NearFade63.HideNear on real renderers: it hides the near parts (Renderer.forceRenderingOff: the component stays enabled, its bounds live),
//      draws the far ones, and with the row off draws again exactly the parts it hid (a part its own view disabled is never marked: the helper
//      owns only what it hid); no state beyond that set
//   4. the hook: Brawl40View.cs calls its NearHide63 (one line), over the body's parts only (bodyRoot, the arms, the fists)
//   5. (the lead's optional item) the boss63 reel skips a same-run HELD frame (FeelClock.Holding): no damage event, flinch frame or rms on it
// Written test first (NIGHT63.md section 4): RED on its commit (Hide is a placeholder answering false; HideNear does nothing; no number; no hook);
// the Editor run is the lead's.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using N = Skygrind.Game.NearFade63;

namespace Skygrind.Game.Tests
{
    public class N63NearHideTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        static readonly Bounds Torso = new Bounds(new Vector3(0, 1.7f, 2), new Vector3(2.2f, 1.7f, 1.5f));

        [Test]
        public void Hide_TheEyeWithinBossHideMOfAPart()
        {
            var p = new N.Params { farM = 1f, nearM = 0.3f, floor = 0.1f, bossHideM = 0.6f };
            Assert.That(N.Hide(p, Torso, new Vector3(0, 1.7f, 2)), Is.True, "the eye inside the part");
            Assert.That(N.Hide(p, Torso, new Vector3(0, 1.7f, 2 - 0.75f - 0.4f)), Is.True, "0.4 m in front of it");
            Assert.That(N.Hide(p, Torso, new Vector3(0, 1.7f, 2 - 0.75f - 0.8f)), Is.False, "0.8 m in front: drawn");
            Assert.That(N.Hide(p, Torso, new Vector3(0, 1.7f, -10)), Is.False, "far: drawn");
            Assert.That(N.Hide(null, Torso, new Vector3(0, 1.7f, 2)), Is.False, "no numbers (the row off): drawn");
            Assert.That(N.Hide(new N.Params { farM = 1f, nearM = 0.3f, floor = 0.1f, bossHideM = 0f }, Torso, new Vector3(0, 1.7f, 2)), Is.False, "bossHideM 0: drawn");
        }

        [Test]
        public void Row_CarriesBossHideM()
        {
            N.Params q = N.Params.From(Cfg());
            Assert.That(q.Valid, Is.True, "the C2 numbers still valid");
            Assert.That(q.bossHideM, Is.InRange(0.2f, 1.5f), "a part is hidden only when the eye is about at it");
            Assert.That(Cfg().Has("design.n63.nearFade63.numbers.bossHideM_why"), Is.True, "with its why");
        }

        static Renderer Box(string name, Vector3 at, Vector3 size, List<GameObject> made)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = at;
            go.transform.localScale = size;
            made.Add(go);
            return go.GetComponent<Renderer>();
        }

        [Test]
        public void HideNear_HidesTheNearParts_AndDrawsAgainOnlyWhatItHid()
        {
            var made = new List<GameObject>();
            try
            {
                Renderer torso = Box("torso", new Vector3(0, 1.7f, 2), new Vector3(2.2f, 1.7f, 1.5f), made);
                Renderer leg = Box("leg", new Vector3(0, 0.6f, 6), new Vector3(0.8f, 1.2f, 0.8f), made);
                Renderer other = Box("someone else's", new Vector3(0, 1.7f, 2.5f), new Vector3(0.5f, 0.5f, 0.5f), made);
                other.enabled = false; // disabled by its own view: never the helper's to draw
                var parts = new List<Renderer> { torso, leg, other };
                var hidden = new HashSet<Renderer>();
                var p = new N.Params { farM = 1f, nearM = 0.3f, floor = 0.1f, bossHideM = 0.6f };
                Vector3 eye = new Vector3(0, 1.6f, 1.5f); // inside the torso's box
                // the helper hides with Renderer.forceRenderingOff (the component stays enabled and its bounds live, so a hidden part is still
                // measured each frame); it never touches .enabled, which stays its view's
                N.HideNear(parts, hidden, true, p, eye);
                Assert.That(torso.forceRenderingOff, Is.True, "the torso at the eye: hidden");
                Assert.That(torso.enabled, Is.True, "its component untouched");
                Assert.That(leg.forceRenderingOff, Is.False, "the leg 4 m away: drawn");
                Assert.That(other.enabled, Is.False, "a part its view turned off stays off");
                Assert.That(other.forceRenderingOff, Is.False, "and the helper never marks it");
                Assert.That(hidden.Contains(other), Is.False, "nor owns it");
                N.HideNear(parts, hidden, true, p, new Vector3(0, 1.6f, -5)); // the eye clear
                Assert.That(torso.forceRenderingOff, Is.False, "the eye clear: the torso drawn again");
                N.HideNear(parts, hidden, true, p, eye);
                Assert.That(torso.forceRenderingOff, Is.True);
                N.HideNear(parts, hidden, false, p, eye); // the row off (a same-run toggle): every part it hid drawn again
                Assert.That(torso.forceRenderingOff, Is.False, "the row off: drawn again");
                Assert.That(other.enabled, Is.False, "never someone else's");
                Assert.That(hidden.Count, Is.EqualTo(0), "no state left");
            }
            finally { foreach (GameObject go in made) UnityEngine.Object.DestroyImmediate(go); }
        }

        [Test]
        public void Hook_TheIcebreakerHidesItsNearParts()
        {
            string dir = Path.Combine(Repo(), "Assets", "Scripts", "Game");
            string view = File.ReadAllText(Path.Combine(dir, "Brawl40View.cs"));
            Assert.That(view.Contains("NearHide63("), Is.True, "Brawl40View.Render calls its NearHide63");
            string hook = File.ReadAllText(Path.Combine(dir, "Brawl40View.N63V.cs"));
            Assert.That(hook.Contains("NearFade63.HideNear("), Is.True, "the hook uses the shared helper");
            Assert.That(hook.Contains("bodyRoot") && hook.Contains("armR") && hook.Contains("fistR"), Is.True, "over the body's parts");
            Assert.That(hook.Contains("arenaRoot"), Is.False, "never the arena");
            Assert.That(hook.Contains("NearFade63.On(game.cfg)"), Is.True, "the row read every frame");
        }

        [Test]
        public void Reel_Boss63SkipsAHeldFrame()
        {
            string reel = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "FeelReel.N63V.cs"));
            Assert.That(reel.Contains("FeelClock.Holding"), Is.True, "boss63 skips a same-run held frame (no damage event, flinch frame or rms on it)");
        }
    }
}
