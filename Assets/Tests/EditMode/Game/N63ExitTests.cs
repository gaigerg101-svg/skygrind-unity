// NIGHT63 builder V (exit63: the doorway itself is the way out; view row design.n63.exit63, OFF; Game/Exit63Math.N63V.cs, Game/Exit63.N63V.cs;
// the doneWhen in Research/2026-10-22-wave63/V-EXIT.md, written before any Player run). GG 2026-10-13: doors "not visible now and you just run
// through it and it's like dark"; P1 (exitFrame61) SHORT: its frame's own gain on the door 1.07-1.56x, a brighter frame lit the gun (the GUARD).
// Pinned:
//   1. the switch: the row, -skygrindOff63 (or older), -skygrindNoExit63 (any case), ForceOffFrom 63; exitFrame61's / lamp60's switches leave it
//   2. the row: a VIEW row (no tape bit), OFF unless a freeze turned it on, its numbers valid, every gain in (0, 1] (no bloom past the door)
//   3. the geometry on n57-kiln's door d3 (4 m wide, 4 m deep, 4.5 m tall): every part lies inside the door's passage (so inside a157's door
//      box: the passage grown 0.37 m, and never in the GUARD's frame), clear of its walls, its floor and its ceiling (never coplanar); the veil
//      fills >= 90 % of the opening just inside room A's face; the liners line both jambs and the lintel; the threshold strip lies just inside
//      the face; the dashes run along the axis toward room B; nothing in room A
//   4. one rise: 0 at the open, 1 at riseS, held after; monotone; never above 1
//   5. the proof line's text; the view reads the row every frame (NightRows.ViewOn), uses no light, and rises on FeelClock.UnscaledDelta
//   6. the veil steps aside as the eye reaches it (never a screen-wide cyan wash as you run through the door): 1 from veilFarM out, 0 at
//      veilNearM or nearer, monotone
//   7. the SPOT pose's reel option (a157's clear shots' boxes are 15,004-37,810 px on b3, under rule 8's 41,472 even if every px changed):
//      -skygrindExitShots63 [room] frames the door from numbers' distance on its axis, facing it, after the room's clear shot; without the
//      switch a157 is unchanged
// Written test first (NIGHT63.md section 4): RED on its commit (Exit63Math's placeholders answer nothing; no row; no view; no reel hook); the
// Editor run is the lead's.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using X = Skygrind.Game.Exit63Math;
using F = Skygrind.Game.ExitFrame61Math;

namespace Skygrind.Game.Tests
{
    public class N63ExitTests
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
            var n63 = (Dictionary<string, object>)design["n63"];
            if (n63.TryGetValue(X.Row, out object r) && r is Dictionary<string, object> row) row["enabled"] = on;
            else n63[X.Row] = new Dictionary<string, object> { ["_why"] = "test", ["enabled"] = on, ["enabled_why"] = "test", ["numbers"] = new Dictionary<string, object>() };
            return c;
        }

        // n57-kiln's d3 (Design/levels/n57-kiln.json): rect [-2, -107.5, 2, -103.5], height 4.5, kb2 -> kb2b (room B toward -z)
        static readonly double[] D3 = { -2, -107.5, 2, -103.5 };
        const double D3H = 4.5;
        static F.Door Door3() => F.DoorOf(D3, D3H, 0, -1, true);

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitches()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(X.OffFor(off, new string[0]), Is.True, "the row off");
                Assert.That(X.OffFor(on, new string[0]), Is.False, "the row on");
                Assert.That(X.OffFor(on, new[] { "-skygrindNoExit63" }), Is.True, "-skygrindNoExit63");
                Assert.That(X.OffFor(on, new[] { "-skygrindnoexit63" }), Is.True, "any case");
                Assert.That(X.OffFor(on, new[] { "-skygrindOff63" }), Is.True, "-skygrindOff63");
                Assert.That(X.OffFor(on, new[] { "-skygrindOff58" }), Is.True, "an older night's switch");
                Assert.That(X.OffFor(on, new[] { "-skygrindOff64" }), Is.False, "a later night's switch");
                Assert.That(X.OffFor(on, new[] { "-skygrindNoExitFrame61", "-skygrindNoExit58" }), Is.False, "exitFrame61's and lamp60's switches leave it on");
                NightRows.ForceOffFrom = 63;
                Assert.That(X.OffFor(on, new string[0]), Is.True, "ForceOffFrom 63");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void Row_AViewRowOff_ItsNumbersValid_EveryGainAtMostOne()
        {
            GameConfig c = Cfg();
            const string p = "design.n63.exit63.";
            Assert.That(c.Has(p + "_why") && c.Has(p + "enabled_why") && c.Has(p + "numbers._why"), Is.True);
            bool enabled = c.Design("n63.exit63.enabled");
            Assert.That(!enabled || ((string)c.Get(p + "enabled_why")).StartsWith("ON", StringComparison.Ordinal), Is.True, "off until a freeze turns it on");
            Assert.That(NightRows.From(c).IndexOf(X.Row), Is.EqualTo(-1), "a view row (no tape bit)");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
            X.Params q = X.Params.From(c);
            Assert.That(q.Valid, Is.True, "the numbers valid");
            Assert.That(q.riseS, Is.InRange(0.2f, 1.5f), "one rise, never a flash");
            foreach (float g in new[] { q.veilGain, q.linerGain, q.stripGain, q.dashGain })
                Assert.That(g, Is.GreaterThan(0f).And.LessThanOrEqualTo(1f), "a gain over 1 blooms past the door's box (the GUARD)");
        }

        static void InsidePassage(F.Part part, string what)
        {
            const double eps = 1e-6;
            Assert.That(part.x - part.sx / 2, Is.GreaterThan(D3[0] + eps), what + ": clear of the passage's -x wall");
            Assert.That(part.x + part.sx / 2, Is.LessThan(D3[2] - eps), what + ": clear of its +x wall");
            Assert.That(part.z - part.sz / 2, Is.GreaterThanOrEqualTo(D3[1] - eps), what + ": not past room B's face");
            Assert.That(part.z + part.sz / 2, Is.LessThanOrEqualTo(D3[3] + eps), what + ": not in room A");
            Assert.That(part.y - part.sy / 2, Is.GreaterThan(0 + eps), what + ": clear of the floor");
            Assert.That(part.y + part.sy / 2, Is.LessThan(D3H - eps), what + ": clear of the ceiling");
        }

        [Test]
        public void Geometry_EveryPartInsideThePassage_TheVeilFillsTheOpening()
        {
            X.Params p = X.Params.From(Cfg());
            F.Door d = Door3();
            F.Part[] all = X.All(d, p);
            Assert.That(all.Length, Is.GreaterThanOrEqualTo(1 + 3 + 1 + 1), "the veil, three liners, the strip, a dash or more");
            for (int i = 0; i < all.Length; i++) InsidePassage(all[i], "part " + i + " " + all[i]);

            F.Part veil = X.Veil(d, p);
            Assert.That(D3[3] - (veil.z + veil.sz / 2), Is.LessThan(0.2), "the veil just inside room A's face");
            Assert.That(veil.sz, Is.LessThan(0.1), "a thin sheet");
            Assert.That(veil.sx * veil.sy, Is.GreaterThanOrEqualTo(0.9 * d.width * d.height * 0.9), "it fills most of the opening (>= 90 % of the opening less its edge insets)");
            Assert.That(veil.sx * veil.sy / (d.width * d.height), Is.GreaterThanOrEqualTo(0.85), "and at least 85 % of the opening's area");

            F.Part[] liners = X.Liners(d, p);
            Assert.That(liners.Length, Is.EqualTo(3), "both jambs and the lintel");
            Assert.That(liners[0].x, Is.LessThan(-1.5), "a jamb on the -x side");
            Assert.That(liners[1].x, Is.GreaterThan(1.5), "a jamb on the +x side");
            Assert.That(liners[2].y, Is.GreaterThan(D3H - 0.5), "the lintel up at the ceiling");
            foreach (F.Part l in liners) Assert.That(l.sz, Is.GreaterThanOrEqualTo(Math.Min(p.linerDepthM, d.depth) * 0.9), "the passage's first metres");

            F.Part strip = X.Strip(d, p);
            Assert.That(D3[3] - (strip.z + strip.sz / 2), Is.LessThan(0.3), "the threshold strip just inside the face");
            Assert.That(strip.y, Is.LessThan(0.2), "on the floor");

            F.Part[] dashes = X.Dashes(d, p);
            Assert.That(dashes.Length, Is.GreaterThanOrEqualTo(1));
            double last = double.MaxValue;
            foreach (F.Part s in dashes)
            {
                Assert.That(Math.Abs(s.x), Is.LessThan(1e-6), "on the door's axis");
                Assert.That(s.z, Is.LessThan(last), "each dash farther toward room B (-z)");
                Assert.That(s.y, Is.LessThan(0.2), "on the floor");
                last = s.z;
            }
        }

        [Test]
        public void Rise_OnceToOne_Held_NeverAFlash()
        {
            Assert.That(X.Rise(0f, 0.6f), Is.EqualTo(0f));
            Assert.That(X.Rise(0.6f, 0.6f), Is.EqualTo(1f).Within(1e-6f));
            Assert.That(X.Rise(5f, 0.6f), Is.EqualTo(1f), "held");
            Assert.That(X.Rise(1f, 0f), Is.EqualTo(1f), "no rise time: on at once");
            float prev = -1f;
            for (float t = 0f; t <= 1.2f; t += 0.01f)
            {
                float k = X.Rise(t, 0.6f);
                Assert.That(k, Is.GreaterThanOrEqualTo(prev - 1e-6f), "monotone");
                Assert.That(k, Is.InRange(0f, 1f), "never above 1: no flash");
                prev = k;
            }
        }

        [Test]
        public void TheLine_AndTheView_ReadTheRowEveryFrame_NoLight()
        {
            Assert.That(X.LitLine("kb3", "d6", 1234), Is.EqualTo("[exit63] door lit: room kb3 (door d6, frame 1234)"));
            string view = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "Exit63.N63V.cs"));
            int late = view.IndexOf("void LateUpdate()", StringComparison.Ordinal);
            Assert.That(late, Is.GreaterThanOrEqualTo(0));
            Assert.That(view.Substring(late).Contains("NightRows.ViewOn("), Is.True, "the row read every frame in LateUpdate (D1's override)");
            Assert.That(view.Contains("FeelClock.UnscaledDelta"), Is.True, "the rise on the held-frame-aware clock");
            Assert.That(view.Contains("AddComponent<Light>") || view.Contains("LightType."), Is.False, "the doorway's own geometry, never a light (the GUARD: no spill on the room or the gun)");
        }

        [Test]
        public void Veil_StepsAsideAsTheEyeReachesIt()
        {
            X.Params p = X.Params.From(Cfg());
            Assert.That(p.veilFarM, Is.GreaterThan(p.veilNearM));
            Assert.That(X.VeilK(p.veilNearM, p), Is.EqualTo(0f), "at veilNearM: gone");
            Assert.That(X.VeilK(0f, p), Is.EqualTo(0f), "at the sheet: gone");
            Assert.That(X.VeilK(p.veilFarM, p), Is.EqualTo(1f), "at veilFarM: whole");
            Assert.That(X.VeilK(30f, p), Is.EqualTo(1f), "across the room: whole");
            float prev = -1f;
            for (float m = 0f; m <= p.veilFarM + 1f; m += 0.05f)
            {
                float k = X.VeilK(m, p);
                Assert.That(k, Is.GreaterThanOrEqualTo(prev - 1e-6f), "monotone");
                Assert.That(k, Is.InRange(0f, 1f));
                prev = k;
            }
            Assert.That(p.veilNearM, Is.GreaterThanOrEqualTo(0.5f), "gone before the camera's near plane meets it");
        }

        [Test]
        public void Shot_TheSpotOnTheDoorsAxis_FacingIt_TheSwitch()
        {
            F.Door d = Door3();
            Vec3 at = X.ShotSpot(d, 10);
            Assert.That(at.x, Is.EqualTo(0).Within(1e-9), "on the axis");
            Assert.That(at.z, Is.EqualTo(-93.5).Within(1e-9), "10 m into room A from its face (z -103.5)");
            Assert.That(at.y, Is.EqualTo(0).Within(1e-9), "on the floor (K52Place lifts the feet)");
            Assert.That(X.ShotYawDeg(d, at), Is.EqualTo(0).Within(1e-9), "facing -z, toward the door (web yaw 0)");
            F.Door ex = F.DoorOf(new double[] { 10, -2, 14, 2 }, 4.5, 1, 0, false); // a passage along x, room B toward +x
            Vec3 ax = X.ShotSpot(ex, 10);
            Assert.That(ax.x, Is.EqualTo(0).Within(1e-9), "10 m into room A from its face (x 10)");
            Assert.That(ax.z, Is.EqualTo(0).Within(1e-9));
            Assert.That(X.ShotYawDeg(ex, ax), Is.EqualTo(-90).Within(1e-9), "facing +x (web yaw -90)");
            Assert.That(X.ShotRoom(new string[0]), Is.Null, "no switch: a157 unchanged");
            Assert.That(X.ShotRoom(new[] { "-skygrindClearShots60" }), Is.Null);
            Assert.That(X.ShotRoom(new[] { "-skygrindExitShots63" }), Is.EqualTo(""), "every room");
            Assert.That(X.ShotRoom(new[] { "-skygrindExitShots63", "-skygrindClearShots60" }), Is.EqualTo(""), "a switch after it is not a room");
            Assert.That(X.ShotRoom(new[] { "-skygrindExitShots63", "kb3d" }), Is.EqualTo("kb3d"), "one room");
            Assert.That(UnityTuning.Exit63ShotM, Is.InRange(6f, 14f), "the door's opening (4 x 4.5 m) over 41,472 px at 1080p and fov 74 (<= 14.3 m), from the room's middle");
            string reel = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "FeelReel.N57.Level.cs"));
            Assert.That(reel.Contains("if (ClearShots60 && Exit63Math.ShotRoom(Environment.GetCommandLineArgs()) != null) yield return L63ExitShot("), Is.True,
                "a157's hook, after the clear shot, only with both switches");
        }
    }
}
