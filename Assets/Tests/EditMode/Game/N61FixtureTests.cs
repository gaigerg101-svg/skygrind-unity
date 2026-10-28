// NIGHT61 builder V (slate S4, lights hang from real fixtures; view row design.n61.fixture61, OFF; Fixture61Math.N61V4.cs, Fixture61.N61V4.cs). Pinned:
//   1. the switch: off when the row is off, under -skygrindOff61 (or an older night's) and -skygrindNoFixture61; on otherwise
//   2. a ceiling light (the room's key panel) gets a housing (four walls UnityTuning.Fixture61RimW outside its frame, Fixture61Drop under the
//      ceiling) and a hanger to the nearest joist on each side (lights35's beams); with no joist, one hanger to the nearest wall face
//   3. nothing moves: the plan's lights and fixtures are the same after the mounts are made; every part stays within Rods40WallClear under its
//      ceiling (rods40's play volume never meets one)
//   4. THE METER on the five campaign levels (the plan the views draw: LightPlan33.GreenFor with the config's rows): the red is every ceiling
//      light unmounted (n52-foundry 9 since NIGHT72's roofless a3 (NIGHT61: 10), n52-iceline 10, n57-kiln 11, n52-span 8, n52-spire 10: a flush panel with only a 0.08 m frame, no
//      hanger); with the mounts, 0 unmounted (<= 10 % of the red), and rods40 hides none of the parts
//   5. the Player's proof line: "[fixture61] mounts on <level>: ceiling lights <n>, housed <n>, held by a beam <n>, by a wall <n>, unmounted <n>"
// Written test first (NIGHT61.md section 4): RED on its commit (Fixture61Math's placeholders answer nothing); the Editor run is the lead's.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using M = Skygrind.Game.Fixture61Math;
using P = Skygrind.Game.LightPlan33;

namespace Skygrind.Game.Tests
{
    public class N61FixtureTests
    {
        const double E = 1e-9;

        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        /// <summary>The repo's config with design.n61.fixture61 set in memory to `on` (the switch's cases, whatever the file's row says).</summary>
        static GameConfig WithRow(bool on)
        {
            GameConfig c = Cfg();
            var design = (Dictionary<string, object>)c.root["design"];
            Dictionary<string, object> n61;
            if (design.TryGetValue("n61", out object n) && n is Dictionary<string, object> d61) n61 = d61;
            else
            {
                n61 = new Dictionary<string, object>();
                design["n61"] = n61;
            }
            n61[M.Row] = new Dictionary<string, object> { ["_why"] = "test", ["enabled"] = on, ["enabled_why"] = "test", ["numbers"] = new Dictionary<string, object>() };
            return c;
        }

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitch()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff; // T-126: whatever an earlier test left
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(M.OffFor(off, new string[0]), Is.True, "the row off: off");
                Assert.That(M.OffFor(on, new string[0]), Is.False, "the row on, no switch: on");
                Assert.That(M.OffFor(on, new[] { "-skygrindNoFixture61" }), Is.True, "-skygrindNoFixture61: this row alone off");
                Assert.That(M.OffFor(on, new[] { "-skygrindnofixture61" }), Is.True, "the one-row switch ignores case (Rim58.No's rule)");
                Assert.That(M.OffFor(on, new[] { "-skygrindOff61" }), Is.True, "-skygrindOff61: NIGHT60's game");
                Assert.That(M.OffFor(on, new[] { "-skygrindOff58" }), Is.True, "an older night's switch covers this night");
                Assert.That(M.OffFor(on, new[] { "-skygrindOff62" }), Is.False, "a later night's switch leaves it on");
                Assert.That(M.OffFor(on, new[] { "-skygrindNoExitFrame61", "-skygrindNoRim56" }), Is.False, "another row's switch leaves it on");
                Assert.That(M.OffReason(on, new string[0]), Is.Null, "on: no reason");
                Assert.That(M.OffReason(off, new string[0]), Is.Not.Null.And.Not.Empty, "off: a reason for the log line");
                NightRows.ForceOffFrom = 61; // the harnesses' and PlayerChecks' process switch (-skygrindOff61 at boot)
                Assert.That(M.OffFor(on, new string[0]), Is.True, "ForceOffFrom 61: off");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        // A roofed room 10 x 20 m (x -5..5, z -20..0), its roof at y 10 (the box y 10..11), walls to the roof; the key panel 1.2 m square flush at its
        // middle (LightPlan33's Panel: y 9.95..10) with the room light 0.45 m under the ceiling; two joists (lights35's: 0.35 wide, 0.45 deep) 2 m either
        // side of the middle along z
        static P.Level Room(bool walls)
        {
            var lv = new P.Level { name = "test" };
            lv.rooms.Add(new P.Room { id = "r", kind = "room", x0 = -5, z0 = -20, x1 = 5, z1 = 0, height = 10, roof = true });
            lv.solid.Add(new P.Box(-6, 10, -21, 6, 11, 1)); // the roof
            lv.solid.Add(new P.Box(-6, -0.5, -21, 6, 0, 1)); // the floor
            if (walls)
            {
                lv.solid.Add(new P.Box(-6, 0, -21, -5, 10, 1));
                lv.solid.Add(new P.Box(5, 0, -21, 6, 10, 1));
                lv.solid.Add(new P.Box(-5, 0, -21, 5, 10, -20));
                lv.solid.Add(new P.Box(-5, 0, 0, 5, 10, 1));
            }
            return lv;
        }

        static P.Plan Plan(bool joists)
        {
            var p = new P.Plan();
            p.fixes.Add(new P.Fix { kind = "panel", type = "panel", room = "r", color = "warm", emissive = true, hdr = 1.2, box = new P.Box(-0.6, 9.95, -10.6, 0.6, 10, -9.4) });
            p.lights.Add(new P.Lit { kind = "room", room = "r", color = "warm", x = 0, y = 9.55, z = -10, range = 38, intensity = 6 });
            if (joists)
            {
                p.fixes.Add(new P.Fix { kind = "body", type = "joist", id = "joist-r-0", room = "r", box = new P.Box(-5, 9.55, -12.175, 5, 10, -11.825) });
                p.fixes.Add(new P.Fix { kind = "body", type = "joist", id = "joist-r-1", room = "r", box = new P.Box(-5, 9.55, -8.175, 5, 10, -7.825) });
            }
            return p;
        }

        static M.Dims D => M.Dims.Tuning;

        [Test]
        public void Mounts_HousingAndAHangerToEachBeam()
        {
            P.Plan p = Plan(true);
            List<M.Mount> m = M.Mounts(Room(true), p, D);
            double fw = UnityTuning.Look33FrameWidth, rim = UnityTuning.Fixture61RimW, cy = 10;
            double ox0 = -0.6 - fw - rim, ox1 = 0.6 + fw + rim, oz0 = -10.6 - fw - rim, oz1 = -9.4 + fw + rim;
            var housing = m.FindAll(x => x.kind == "housing");
            Assert.That(housing.Count, Is.EqualTo(4), "four housing walls");
            foreach (M.Mount h in housing)
            {
                Assert.That(h.light, Is.EqualTo(0), "the housing holds the panel");
                Assert.That(h.box.y1, Is.EqualTo(cy).Within(E), "up to the ceiling");
                Assert.That(h.box.y0, Is.EqualTo(cy - UnityTuning.Fixture61Drop).Within(E), "Fixture61Drop under it");
                Assert.That(h.box.x0, Is.GreaterThanOrEqualTo(ox0 - E));
                Assert.That(h.box.x1, Is.LessThanOrEqualTo(ox1 + E));
                Assert.That(h.box.z0, Is.GreaterThanOrEqualTo(oz0 - E));
                Assert.That(h.box.z1, Is.LessThanOrEqualTo(oz1 + E));
                bool outside = h.box.x1 <= -0.6 - fw + E || h.box.x0 >= 0.6 + fw - E || h.box.z1 <= -10.6 - fw + E || h.box.z0 >= -9.4 + fw - E;
                Assert.That(outside, Is.True, "outside the panel's frame (the lens stays in view)");
            }
            var beams = m.FindAll(x => x.kind == "beam hanger");
            Assert.That(beams.Count, Is.EqualTo(2), "a hanger to the nearest joist on each side");
            Assert.That(m.FindAll(x => x.kind == "wall hanger").Count, Is.EqualTo(0), "no wall hanger where a beam holds it");
            double hy1 = cy - UnityTuning.Fixture61HangerTop, hy0 = hy1 - UnityTuning.Fixture61HangerH, hw = UnityTuning.Fixture61HangerW / 2;
            var south = beams.Find(x => x.box.z1 <= oz0 + E);
            var north = beams.Find(x => x.box.z0 >= oz1 - E);
            Assert.That(south.kind, Is.EqualTo("beam hanger"), "one to the south joist");
            Assert.That(north.kind, Is.EqualTo("beam hanger"), "one to the north joist");
            Assert.That(south.box.z0, Is.EqualTo(-11.825).Within(E), "from the joist's face");
            Assert.That(south.box.z1, Is.EqualTo(oz0).Within(E), "to the housing");
            Assert.That(north.box.z0, Is.EqualTo(oz1).Within(E));
            Assert.That(north.box.z1, Is.EqualTo(-8.175).Within(E));
            foreach (M.Mount b in beams)
            {
                Assert.That(b.box.x0, Is.EqualTo(-hw).Within(E), "on the panel's line");
                Assert.That(b.box.x1, Is.EqualTo(hw).Within(E));
                Assert.That(b.box.y0, Is.EqualTo(hy0).Within(E), "in the hanger's band");
                Assert.That(b.box.y1, Is.EqualTo(hy1).Within(E));
            }
            M.Report red = M.Count(p, new List<M.Mount>(), UnityTuning.Fixture61MinRimM, UnityTuning.Look33FrameWidth);
            M.Report green = M.Count(p, m, UnityTuning.Fixture61MinRimM, UnityTuning.Look33FrameWidth);
            Assert.That((red.lights, red.housed, red.unmounted), Is.EqualTo((1, 0, 1)), "the red: a flush panel with a 0.08 m frame and no hanger is unmounted");
            Assert.That((green.lights, green.housed, green.beam, green.wall, green.unmounted), Is.EqualTo((1, 1, 1, 0, 0)), "housed and held by a beam");
        }

        [Test]
        public void Mounts_NoJoist_OneHangerToTheNearestWall()
        {
            P.Plan p = Plan(false);
            List<M.Mount> m = M.Mounts(Room(true), p, D);
            Assert.That(m.FindAll(x => x.kind == "housing").Count, Is.EqualTo(4), "the housing");
            Assert.That(m.FindAll(x => x.kind == "beam hanger").Count, Is.EqualTo(0), "no joist: no beam hanger");
            var wall = m.FindAll(x => x.kind == "wall hanger");
            Assert.That(wall.Count, Is.EqualTo(1), "one hanger to the nearest wall face (the side walls are 4.16 m away, the end walls 9.16 m: past Fixture61WallMaxM)");
            double reach = 0.6 + UnityTuning.Look33FrameWidth + UnityTuning.Fixture61RimW;
            bool side = Math.Abs(wall[0].box.x0 - -5) < E && Math.Abs(wall[0].box.x1 - -reach) < E || Math.Abs(wall[0].box.x0 - reach) < E && Math.Abs(wall[0].box.x1 - 5) < E;
            Assert.That(side, Is.True, "from the housing to a side wall's face: " + wall[0].box.x0 + ".." + wall[0].box.x1);
            M.Report green = M.Count(p, m, UnityTuning.Fixture61MinRimM, UnityTuning.Look33FrameWidth);
            Assert.That((green.beam, green.wall, green.unmounted), Is.EqualTo((0, 1, 0)), "held by a wall");
            List<M.Mount> none = M.Mounts(Room(false), p, D);
            Assert.That(none.FindAll(x => x.kind != "housing").Count, Is.EqualTo(0), "no joist and no wall within reach: the housing only");
            Assert.That(M.Count(p, none, UnityTuning.Fixture61MinRimM, UnityTuning.Look33FrameWidth).unmounted, Is.EqualTo(1), "a housing alone is not a mount");
        }

        [Test]
        public void NothingMoves_AndEveryPartIsFlushUnderItsCeiling()
        {
            P.Plan p = Plan(true);
            var before = new List<string>();
            foreach (P.Lit l in p.lights) before.Add(l.x + "," + l.y + "," + l.z + "," + l.range + "," + l.intensity);
            int fixes = p.fixes.Count;
            List<M.Mount> m = M.Mounts(Room(true), p, D);
            Assert.That(m.Count, Is.GreaterThan(0), "mounts made");
            Assert.That(p.fixes.Count, Is.EqualTo(fixes), "no fixture added to or taken from the plan");
            for (int i = 0; i < p.lights.Count; i++)
            {
                P.Lit l = p.lights[i];
                Assert.That(l.x + "," + l.y + "," + l.z + "," + l.range + "," + l.intensity, Is.EqualTo(before[i]), "no light moved or dimmed");
            }
            Assert.That(UnityTuning.Fixture61Drop, Is.LessThanOrEqualTo(UnityTuning.Rods40WallClear), "the housing within rods40's flush band");
            Assert.That(UnityTuning.Fixture61HangerTop + UnityTuning.Fixture61HangerH, Is.LessThanOrEqualTo(UnityTuning.Rods40WallClear), "the hangers within it");
            foreach (M.Mount x in m) Assert.That(x.box.y0, Is.GreaterThanOrEqualTo(10 - UnityTuning.Rods40WallClear - E), x.kind + " flush under the ceiling");
        }

        [Test]
        public void TheMeter_OnTheFiveLevels_RedEveryCeilingLight_GreenNone()
        {
            // NIGHT72 N0b (6b73792e6): n52-foundry has 9 ceiling lights, not NIGHT61's 10. Room a3 is roofless now (its plan says roof false: the 1.3 RIDE slab at 10 m is out of the
            // shipped level, GG's D-4), and the light plan draws a ceiling panel only under a roof, so the key panel that hung from that slab went with it. The nine left are all
            // housed and held by a beam (Research/2026-10-27-wave72/L/evidence/l6-fixture61-stage3.txt); the other four levels are as NIGHT61 counted.
            var red = new Dictionary<string, int> { ["n52-foundry"] = 9, ["n52-iceline"] = 10, ["n57-kiln"] = 11, ["n52-span"] = 8, ["n52-spire"] = 10 };
            int was = NightRows.ForceOffFrom;
            Func<GameConfig, bool> was35 = P.Lights35On;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                P.Lights35On = c => N35Fixes.ViewRowOn(c, "lights35"); // Fixtures33.Hook35's hook (it runs only in Play mode)
                GameConfig cfg = Cfg();
                foreach (var kv in red)
                {
                    var json = PortData.Obj(PortData.LoadJson(Path.Combine(Repo(), "Design", "levels", kv.Key + ".json")));
                    P.Plan p = P.GreenFor(json, cfg, N30Fixes.ViewRowOn(cfg, "look30"), out P.Level lv);
                    Assert.That(p, Is.Not.Null, kv.Key + ": a light plan");
                    M.Report before = M.Count(p, new List<M.Mount>(), UnityTuning.Fixture61MinRimM, UnityTuning.Look33FrameWidth);
                    List<M.Mount> mounts = M.Mounts(lv, p, D);
                    M.Report after = M.Count(p, mounts, UnityTuning.Fixture61MinRimM, UnityTuning.Look33FrameWidth);
                    TestContext.WriteLine($"{kv.Key}: red unmounted {before.unmounted} of {before.lights}; with fixture61 unmounted {after.unmounted} (housed {after.housed}, held by a beam {after.beam}, by a wall {after.wall}); parts {mounts.Count}");
                    Assert.That(before.lights, Is.EqualTo(kv.Value), kv.Key + ": the ceiling lights in the plan");
                    Assert.That(before.unmounted, Is.EqualTo(kv.Value), kv.Key + ": the red: every one unmounted");
                    Assert.That(after.unmounted, Is.LessThanOrEqualTo(kv.Value / 10), kv.Key + ": <= 10 % of the red");
                    Assert.That(after.housed, Is.EqualTo(after.lights), kv.Key + ": every ceiling light housed");
                    foreach (M.Mount x in mounts)
                    {
                        double cy = p.fixes[x.light].box.y1;
                        Assert.That(x.box.y0, Is.GreaterThanOrEqualTo(cy - UnityTuning.Rods40WallClear - E), kv.Key + ": " + x.kind + " flush under its ceiling");
                        Assert.That(Rods40.BoxHidden(lv, x.box, out _), Is.False, kv.Key + ": rods40 would hide a " + x.kind + " (it is in the play volume)");
                    }
                }
            }
            finally
            {
                NightRows.ForceOffFrom = was;
                P.Lights35On = was35;
            }
        }

        [Test]
        public void LogLine_ThePlayersProof()
        {
            var r = new M.Report { lights = 10, housed = 10, beam = 9, wall = 1, unmounted = 0 };
            Assert.That(M.LogLine("n52-iceline", r), Is.EqualTo("[fixture61] mounts on n52-iceline: ceiling lights 10, housed 10, held by a beam 9, by a wall 1, unmounted 0"));
        }
    }
}
