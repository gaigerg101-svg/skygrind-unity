// NIGHT63 builder V (leftover 7, C2 the enemy glow up close; gg-proxy-start.md #9 "enemies read as glow domes and flat beans"; view row
// design.n63.nearFade63, OFF; Game/NearFade63.N63V.cs, its hook in EnemyViews.cs EnemyLook). The red (Tools/night/n63-v-near.py on n62-freeze's
// verbs37): an enemy at the eye in 348 of 9000 frames; vf/f0400 is 77 % enemy hue: the eye at husk 2's capsule (the dome) with husk 3's x-ray
// through it (the bean). Pinned:
//   1. the switch: off when the row is off, under -skygrindOff63 (or older), -skygrindNoNearFade63 (any case), ForceOffFrom 63; on otherwise
//   2. the row: a VIEW row (no tape bit), OFF unless a freeze turned it on, its numbers (farM, nearM, floor) present and sane
//   3. the level (1 whole .. 0 gone; C2 drove the shader's _Dissolve with it, C2b its own screen-door _NearFade: N63ScreenDoorTests): 1 with the
//      eye farM or more from the part's world bounds, the floor at nearM or nearer (inside the bounds: 0 m), easing down between; 1 whenever the
//      row is off
//   4. the hook: EnemyLook.Tint sets every live part's _NearFade from it (and compares it in its change cache), the row and the eye read once
//      a frame in EnemyLook.Frame (D1's contract: read every frame, never cached across frames; no allocation per part)
//   5. the SPOT pose's reel: near63 registered beside boss63
// Written test first (NIGHT63.md section 4): RED on its commit (NearFade63's placeholders answer off / whole; the row does not exist; no hook);
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
    public class N63NearFadeTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        /// <summary>The repo's config with design.n63.nearFade63 set in memory to `on` (its numbers kept when the row exists).</summary>
        static GameConfig WithRow(bool on)
        {
            GameConfig c = Cfg();
            var design = (Dictionary<string, object>)c.root["design"];
            var n63 = (Dictionary<string, object>)design["n63"];
            if (n63.TryGetValue(N.Row, out object r) && r is Dictionary<string, object> row) row["enabled"] = on;
            else n63[N.Row] = new Dictionary<string, object> { ["_why"] = "test", ["enabled"] = on, ["enabled_why"] = "test", ["numbers"] = new Dictionary<string, object>() };
            return c;
        }

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitches()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(N.OffFor(off, new string[0]), Is.True, "the row off: off");
                Assert.That(N.OffFor(on, new string[0]), Is.False, "the row on, no switch: on");
                Assert.That(N.OffFor(on, new[] { "-skygrindNoNearFade63" }), Is.True, "-skygrindNoNearFade63: this row alone off");
                Assert.That(N.OffFor(on, new[] { "-skygrindnonearfade63" }), Is.True, "any case");
                Assert.That(N.OffFor(on, new[] { "-skygrindOff63" }), Is.True, "-skygrindOff63");
                Assert.That(N.OffFor(on, new[] { "-skygrindOff60" }), Is.True, "an older night's switch");
                Assert.That(N.OffFor(on, new[] { "-skygrindOff64" }), Is.False, "a later night's switch leaves it on");
                Assert.That(N.OffFor(on, new[] { "-skygrindNoBossMotion63" }), Is.False, "another row's switch leaves it on");
                NightRows.ForceOffFrom = 63;
                Assert.That(N.OffFor(on, new string[0]), Is.True, "ForceOffFrom 63");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void Row_AViewRowOff_WithItsNumbers()
        {
            GameConfig c = Cfg();
            const string p = "design.n63.nearFade63.";
            Assert.That(c.Has(p + "_why") && c.Has(p + "enabled_why") && c.Has(p + "numbers._why"), Is.True, "the row and its numbers carry their whys");
            Assert.That(c.Get(p + "enabled") is bool, Is.True);
            bool enabled = c.Design("n63.nearFade63.enabled");
            Assert.That(!enabled || ((string)c.Get(p + "enabled_why")).StartsWith("ON", StringComparison.Ordinal), Is.True, "off until a freeze turns it on");
            Assert.That(NightRows.From(c).IndexOf(N.Row), Is.EqualTo(-1), "a view row (no tape bit)");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
            N.Params q = N.Params.From(c);
            Assert.That(q.Valid, Is.True, "farM > nearM >= 0, the floor in [0, 1)");
            Assert.That(q.farM, Is.InRange(0.5f, 3f), "the fade starts within arm's reach, never across a room");
            Assert.That(q.nearM, Is.InRange(0f, q.farM));
            Assert.That(q.floor, Is.InRange(0f, 0.5f), "at the eye most of the body is gone");
        }

        static readonly Bounds Husk = new Bounds(new Vector3(0, 0.9f, 5), new Vector3(0.8f, 1.8f, 0.8f));

        /// <summary>The eye at `m` metres in front of the husk's bounds (on the line through its centre, at 1.6 m).</summary>
        static Vector3 EyeAt(float m) => new Vector3(0, 1.6f, 5 - 0.4f - m);

        [Test]
        public void Dissolve_WholeFar_TheFloorAtTheEye_EasingBetween()
        {
            var p = new N.Params { farM = 1.0f, nearM = 0.3f, floor = 0.1f };
            Assert.That(p.Valid, Is.True);
            Assert.That(N.Dissolve(p, Husk, EyeAt(3f)), Is.EqualTo(1f), "3 m away: whole");
            Assert.That(N.Dissolve(p, Husk, EyeAt(1.0f)), Is.EqualTo(1f).Within(1e-5f), "at farM: whole");
            Assert.That(N.Dissolve(p, Husk, EyeAt(0.3f)), Is.EqualTo(0.1f).Within(1e-5f), "at nearM: the floor");
            Assert.That(N.Dissolve(p, Husk, EyeAt(0.05f)), Is.EqualTo(0.1f).Within(1e-5f), "nearer: the floor");
            Assert.That(N.Dissolve(p, Husk, Husk.center), Is.EqualTo(0.1f).Within(1e-5f), "inside the bounds: the floor");
            float last = 1.01f;
            for (float m = 1.0f; m >= 0.3f; m -= 0.05f)
            {
                float d = N.Dissolve(p, Husk, EyeAt(m));
                Assert.That(d, Is.LessThanOrEqualTo(last + 1e-6f), "easing down as the eye nears (" + m + " m)");
                Assert.That(d, Is.InRange(0.1f - 1e-5f, 1f + 1e-5f));
                last = d;
            }
            Assert.That(N.Dissolve(p, Husk, EyeAt(0.65f)), Is.InRange(0.3f, 0.8f), "halfway: partly dissolved");
            Assert.That(N.Dissolve(null, Husk, EyeAt(0.05f)), Is.EqualTo(1f), "no numbers (the row off): whole");
            Assert.That(N.Dissolve(new N.Params(), Husk, EyeAt(0.05f)), Is.EqualTo(1f), "invalid numbers: whole");
        }

        [Test]
        public void Hook_EnemyLookSetsEveryLivePartsDissolve_TheRowReadOnceAFrame()
        {
            string src = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "EnemyViews.cs"));
            int tint = src.IndexOf("public void Tint(MaterialPropertyBlock block, Renderer r, Color c, float flash, float rim)", StringComparison.Ordinal);
            Assert.That(tint, Is.GreaterThanOrEqualTo(0), "EnemyLook.Tint");
            int end = src.IndexOf("public void Frame()", tint, StringComparison.Ordinal);
            string body = src.Substring(tint, end - tint);
            Assert.That(body.Contains("NearFade63.Dissolve("), Is.True, "Tint asks the dissolve for the part");
            // NIGHT63 C2b (Logs/n63/verdict-C2.md section 6: the death's dissolve read as a shatter): the level now drives the screen-door's own
            // _NearFade, never the death's _Dissolve (N63ScreenDoorTests pins the rest)
            Assert.That(body.Contains("block.SetFloat(NearFade63.NearFadeId, near63)"), Is.True, "and sets it on the part's block every time (the block is shared across parts)");
            Assert.That(body.Contains("near63"), Is.True, "the change cache compares it");
            Assert.That(body.Contains("NearFade63.On("), Is.False, "the row is not read per part");
            int frame = src.IndexOf("public void Frame()", StringComparison.Ordinal);
            string frameBody = src.Substring(frame, Math.Min(2500, src.Length - frame));
            Assert.That(frameBody.Contains("NearFade63.On(game.cfg)"), Is.True, "the row read once a frame in Frame()");
        }

        [Test]
        public void Reel_Near63RegisteredBesideBoss63()
        {
            Assert.That(Array.IndexOf(FeelReel.PhaseN41T, "boss63"), Is.GreaterThanOrEqualTo(0), "the control");
            Assert.That(Array.IndexOf(FeelReel.PhaseN41T, "near63"), Is.GreaterThanOrEqualTo(0), "near63 (the SPOT pose's reel)");
        }
    }
}
