// NIGHT61 builder V (slate S1, the exit frame; view row design.n61.exitFrame61, OFF; ExitFrame61Math.N61V.cs, ExitFrame61.N61V.cs). Pinned:
//   1. the switch: off when the row is off, under -skygrindOff61 (or an older night's), -skygrindNoExitFrame61 (this row alone) and
//      -skygrindNoExit58 (no exit lamp at all: the slate's off side); on otherwise (a later night's switch or another row's leaves it on)
//   2. the frame: two posts and a lintel round the opening, UnityTuning.ExitFrame61BarW wide from its edges outward (never wider than the band
//      a157's clear shot counts as the door), standing just in front of room A's face of the passage (n57-kiln's d6; and a door along x)
//   3. the gate: two posts and a lintel inside the passage's far mouth, clear of its walls and ceiling; the passage light hangs under its lintel,
//      inside the passage, and its range never reaches room A's floor outside a157's door box (the GUARD's reason)
//   4. the floor line's run: it stops UnityTuning.ExitFrame61LineClearM short of the first thing on its strip (a low solid, a floor at another
//      height, lava), passes under a solid overhead, ignores one beside the strip or behind the face; on n57-kiln's eight lit doors from the
//      map's own data (d6: kb3's gallery ramp 3 m out; d7: the breach's lava 7 m out)
//   5. the dashes: on the door's axis from the run's end through the doorway to the far mouth, on the floor, brightening toward the door
//   6. the Player's proof line (L-089): "[exit61] door lit: room <id> (door <door>, frame <n>)"
// Written test first (NIGHT61.md section 4): RED on its commit (ExitFrame61Math's placeholders answer nothing); the Editor run is the lead's.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using M = Skygrind.Game.ExitFrame61Math;

namespace Skygrind.Game.Tests
{
    public class N61ExitFrameTests
    {
        const double E = 1e-9;

        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        /// <summary>The repo's config with design.n61.exitFrame61 set in memory to `on` (the switch's cases, whatever the file's row says).</summary>
        static GameConfig WithRow(bool on)
        {
            GameConfig c = GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));
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
        public void Switch_OffUnderTheRowAndItsSwitches()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff; // T-126: whatever an earlier test left
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(M.OffFor(off, new string[0]), Is.True, "the row off: off");
                Assert.That(M.OffFor(on, new string[0]), Is.False, "the row on, no switch: on");
                Assert.That(M.OffFor(on, new[] { "-skygrindNoExitFrame61" }), Is.True, "-skygrindNoExitFrame61: this row alone off");
                Assert.That(M.OffFor(on, new[] { "-skygrindnoexitframe61" }), Is.True, "the one-row switch ignores case (Rim58.No's rule)");
                Assert.That(M.OffFor(on, new[] { "-skygrindNoExit58" }), Is.True, "-skygrindNoExit58: no exit lamp at all (the slate's off side)");
                Assert.That(M.OffFor(on, new[] { "-skygrindOff61" }), Is.True, "-skygrindOff61: NIGHT60's game");
                Assert.That(M.OffFor(on, new[] { "-skygrindOff58" }), Is.True, "an older night's switch covers this night");
                Assert.That(M.OffFor(on, new[] { "-skygrindOff62" }), Is.False, "a later night's switch leaves it on");
                Assert.That(M.OffFor(on, new[] { "-skygrindNoRim56", "-skygrindClearShots60" }), Is.False, "another row's switch leaves it on");
                Assert.That(M.OffReason(on, new string[0]), Is.Null, "on: no reason");
                Assert.That(M.OffReason(off, new string[0]), Is.Not.Null.And.Not.Empty, "off: a reason for the reel's line");
                NightRows.ForceOffFrom = 61; // the harnesses' and PlayerChecks' process switch (-skygrindOff61 at boot)
                Assert.That(M.OffFor(on, new string[0]), Is.True, "ForceOffFrom 61: off");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        // n57-kiln's d6 (kb3 -> kb3c): rect x -2..2, z -229.5..-225.5 (a 4 m deep passage), height 4.5; room B (kb3c) at lower z: ahead (0, -1),
        // the opening spans x; room A's (kb3's) face of the passage is z -225.5
        static M.Door D6() => M.DoorOf(new[] { -2.0, -229.5, 2.0, -225.5 }, 4.5, 0, -1, true);

        static void Check(M.Part p, double x, double y, double z, double sx, double sy, double sz, string what)
        {
            Assert.That(p.x, Is.EqualTo(x).Within(E), what + ": x");
            Assert.That(p.y, Is.EqualTo(y).Within(E), what + ": y");
            Assert.That(p.z, Is.EqualTo(z).Within(E), what + ": z");
            Assert.That(p.sx, Is.EqualTo(sx).Within(E), what + ": size x");
            Assert.That(p.sy, Is.EqualTo(sy).Within(E), what + ": size y");
            Assert.That(p.sz, Is.EqualTo(sz).Within(E), what + ": size z");
        }

        [Test]
        public void Frame_FlanksTheOpening_InFrontOfRoomAsFace()
        {
            double w = UnityTuning.ExitFrame61BarW, d = UnityTuning.ExitFrame61BarD, gap = UnityTuning.ExitFrame61FaceGap;
            M.Part[] f = M.Frame(D6(), w, d, gap);
            Assert.That(f.Length, Is.EqualTo(3), "two posts and a lintel");
            double z = -225.5 + gap + d / 2; // in front of kb3's face (room A at higher z)
            Check(f[0], -(2 + w / 2), 2.25, z, w, 4.5, d, "the west post, from the opening's edge outward");
            Check(f[1], 2 + w / 2, 2.25, z, w, 4.5, d, "the east post");
            Check(f[2], 0, 4.5 + w / 2, z, 4 + 2 * w, w, d, "the lintel over the opening and both posts");
            Assert.That(f[0].z - f[0].sz / 2, Is.GreaterThan(-225.5), "the frame's back stands in front of the wall, never in it");
            Assert.That(w, Is.LessThanOrEqualTo(UnityTuning.Exit58FrameOut + UnityTuning.Exit58FrameW + E), "the bars stay inside the band a157's clear shot counts as the door (FeelReel.L60ScreenRect)");
        }

        [Test]
        public void Frame_AlongX()
        {
            // a door spanning z (its passage along x), 4 m deep: rect x 10..14, z -4..0, height 4; room B at higher x: ahead (1, 0); room A's face x 10
            M.Door door = M.DoorOf(new[] { 10.0, -4.0, 14.0, 0.0 }, 4.0, 1, 0, false);
            double w = UnityTuning.ExitFrame61BarW, d = UnityTuning.ExitFrame61BarD, gap = UnityTuning.ExitFrame61FaceGap;
            M.Part[] f = M.Frame(door, w, d, gap);
            Assert.That(f.Length, Is.EqualTo(3), "two posts and a lintel");
            double x = 10 - gap - d / 2; // in front of room A's face (room A at lower x)
            Check(f[0], x, 2, -2 - (2 + w / 2), d, 4, w, "the post at lower z");
            Check(f[1], x, 2, -2 + (2 + w / 2), d, 4, w, "the post at higher z");
            Check(f[2], x, 4 + w / 2, -2, d, w, 4 + 2 * w, "the lintel");
        }

        [Test]
        public void Gate_InsideTheFarMouth_TheLightUnderIt_OutOfRoomAsFloor()
        {
            M.Door door = D6();
            double g = UnityTuning.ExitFrame61GateW, d = UnityTuning.ExitFrame61BarD, inset = UnityTuning.ExitFrame61GateInset;
            M.Part[] gate = M.Gate(door, g, d, inset);
            Assert.That(gate.Length, Is.EqualTo(3), "two posts and a lintel");
            double z = -229.5 + inset + d / 2; // just inside kb3c's face of the passage (room B at lower z)
            double postH = 4.5 - inset - g;
            Check(gate[0], -(2 - inset - g / 2), postH / 2, z, g, postH, d, "the west post, inside the opening");
            Check(gate[1], 2 - inset - g / 2, postH / 2, z, g, postH, d, "the east post");
            Check(gate[2], 0, 4.5 - inset - g / 2, z, 4 - 2 * inset, g, d, "the lintel under the passage's ceiling");
            Vec3 l = M.LightSpot(door, g, d, inset, UnityTuning.ExitFrame61LightBack, UnityTuning.ExitFrame61LightDrop);
            Assert.That(l.x, Is.EqualTo(0).Within(E), "on the door's axis");
            Assert.That(l.z, Is.EqualTo(-229.5 + inset + d + UnityTuning.ExitFrame61LightBack).Within(E), "on room A's side of the gate");
            Assert.That(l.y, Is.EqualTo(4.5 - inset - g - UnityTuning.ExitFrame61LightDrop).Within(E), "under the gate's lintel");
            Assert.That(l.z, Is.LessThan(-225.5).And.GreaterThan(-229.5), "inside the passage");
            // the GUARD's reason: room A's floor just outside the band a157 counts as the door (that far in front of the face) is out of range
            double edge = UnityTuning.Exit58FrameOut + UnityTuning.Exit58FrameW;
            double dz = -225.5 + edge - l.z, dy = l.y;
            Assert.That(Math.Sqrt(dz * dz + dy * dy), Is.GreaterThan(UnityTuning.ExitFrame61LightRange), "room A's floor outside the door's box is never lit");
            Assert.That(M.Along(door, 0, -225.5 + edge), Is.EqualTo(edge).Within(E), "Along: metres from room A's face into room A");
            Assert.That(M.Along(door, 0, l.z), Is.LessThan(0), "Along: negative in the passage");
        }

        [Test]
        public void FloorRun_StopsShortOfTheFirstThing()
        {
            M.Door door = D6();
            const double max = 10, half = 0.5, clear = 0.75, head = 2, step = 0.05;
            var floor = new M.Block(-12, -0.5, -227.5, 12, 0, -195.5, true); // kb3's floor slab (top 0: the floor itself)
            Func<M.Block[], double> run = b => M.FloorRun(door, b, 0, max, half, clear, head, step);
            Assert.That(run(new[] { floor }), Is.EqualTo(max).Within(E), "the open floor: the whole run");
            Assert.That(run(new[] { floor, new M.Block(-1, 0, -220.5, 0.5, 0.9, -218, false) }), Is.EqualTo(5 - clear).Within(E), "a low solid 5 m out");
            Assert.That(run(new[] { floor, new M.Block(0.5, 3.4, -220.5, 2, 4.5, -217.5, false) }), Is.EqualTo(max).Within(E), "a ledge overhead: the line runs under it");
            Assert.That(run(new[] { floor, new M.Block(3, 0, -224, 5, 4.5, -218.5, false) }), Is.EqualTo(max).Within(E), "a solid beside the strip");
            Assert.That(run(new[] { floor, new M.Block(-1.5, -1.8, -218.5, 1, -0.8, -209.5, true) }), Is.EqualTo(7 - clear).Within(E), "a floor at another height (lava, a pit) 7 m out");
            Assert.That(run(new[] { floor, new M.Block(-1, 0, -229, 1, 3, -226, false) }), Is.EqualTo(max).Within(E), "a solid behind the face (in the passage)");
            Assert.That(run(new[] { floor, new M.Block(-1, 0, -226, 1, 1, -225, false) }), Is.EqualTo(0).Within(E), "a solid across the face: no run");
            Assert.That(run(new[] { floor, new M.Block(-1, 0, -220.5, 1, 1, -218, false), new M.Block(-1, 0, -223, 1, 1, -222, false) }), Is.EqualTo(2.5 - clear).Within(E), "the nearer of two");
        }

        [Test]
        public void FloorRun_OnN57Kiln_TheEightLitDoors()
        {
            var json = PortData.Obj(PortData.LoadJson(Path.Combine(Repo(), "Design", "levels", "n57-kiln.json")));
            List<M.Block> blocks = M.Blocks(json);
            Assert.That(blocks.Count, Is.GreaterThan(0), "the map's boxes, barriers, ramps and lava");
            var camp = PortData.Obj(json["campaign"]);
            var rooms = Doors38Math.Rooms(camp);
            // the first thing on each door's approach along its axis in the map's data (NIGHT61 V's notes): d1 kb1's ledge 7 m, d2 kb1b's ledge
            // 6.5 m, d3 kb2's step (0.9 m high) 5 m, d5 kb2c's far ramp 9 m, d6 kb3's gallery ramp 3 m, d7 kb3d's breach (its floor 0.8 m down) 7 m;
            // d4 and d6b: nothing within the run
            var first = new Dictionary<string, double>
            {
                ["d1"] = 7, ["d2"] = 6.5, ["d3"] = 5, ["d4"] = double.PositiveInfinity,
                ["d5"] = 9, ["d6"] = 3, ["d6b"] = double.PositiveInfinity, ["d7"] = 7,
            };
            double max = UnityTuning.ExitFrame61LineMaxM, clear = UnityTuning.ExitFrame61LineClearM;
            int seen = 0;
            foreach (object o in PortData.Arr(camp["doors"]))
            {
                var d = PortData.Obj(o);
                string id = (string)d["id"];
                var between = PortData.Arr(d["between"]);
                string a = (string)between[0], b = (string)between[1];
                if (rooms[b].kind == "boss") continue; // d8 into k5: the boss door keeps its boundary tone (no frame)
                double[] rect = Doors38Math.Rect(d["rect"]);
                bool spansX = Doors38Math.SpansX(rect, rooms[a].rect, rooms[b].rect);
                var (ax, az) = Doors38Math.Ahead(rect, rooms[b].rect, spansX);
                M.Door door = M.DoorOf(rect, PortData.Num(d["height"]), ax, az, spansX);
                double got = M.FloorRun(door, blocks, 0, max, UnityTuning.ExitFrame61LineHalfM, clear, UnityTuning.ExitFrame61LineHeadM, UnityTuning.ExitFrame61LineStepM);
                Assert.That(first.ContainsKey(id), Is.True, "a lit door this test does not know: " + id);
                Assert.That(got, Is.EqualTo(Math.Max(0, Math.Min(max, first[id] - clear))).Within(1e-6), id + "'s floor line");
                seen++;
            }
            Assert.That(seen, Is.EqualTo(8), "n57-kiln's eight doors that are not into the boss room");
        }

        [Test]
        public void Line_RunsFromTheRoomThroughTheDoorway_BrighteningTowardTheDoor()
        {
            M.Door door = D6();
            double run = 6.25, wd = UnityTuning.ExitFrame61LineW, lift = UnityTuning.ExitFrame61LineLift, thick = UnityTuning.ExitFrame61LineThick;
            List<M.Part> dashes = M.Line(door, run, 0, wd, UnityTuning.ExitFrame61LineDashM, UnityTuning.ExitFrame61LineGapM, lift, thick);
            Assert.That(dashes.Count, Is.GreaterThan(4), "dashes from the room through the doorway");
            double nearest = double.MaxValue, farthest = double.MinValue;
            foreach (M.Part p in dashes)
            {
                Assert.That(p.x, Is.EqualTo(0).Within(E), "on the door's axis");
                Assert.That(p.sx, Is.EqualTo(wd).Within(E), "the line's width across the passage");
                Assert.That(p.y - p.sy / 2, Is.EqualTo(lift).Within(E), "lifted over the floor");
                Assert.That(p.sy, Is.EqualTo(thick).Within(E), "a flat stripe");
                Assert.That(p.sz, Is.GreaterThan(0).And.LessThanOrEqualTo(UnityTuning.ExitFrame61LineDashM + E), "a dash");
                double a0 = M.Along(door, p.x, p.z) - p.sz / 2, a1 = M.Along(door, p.x, p.z) + p.sz / 2;
                nearest = Math.Min(nearest, a0);
                farthest = Math.Max(farthest, a1);
            }
            Assert.That(nearest, Is.EqualTo(-4).Within(E), "it reaches the passage's far mouth (kb3c's face)");
            Assert.That(farthest, Is.LessThanOrEqualTo(run + E).And.GreaterThan(run - UnityTuning.ExitFrame61LineDashM - UnityTuning.ExitFrame61LineGapM), "it ends at the run's end, in room A");
            double dim = UnityTuning.ExitFrame61LineDim;
            Assert.That(M.LineGlow(-2, run, dim), Is.EqualTo(1).Within(E), "full in the passage");
            Assert.That(M.LineGlow(0, run, dim), Is.EqualTo(1).Within(E), "full at the door");
            Assert.That(M.LineGlow(run, run, dim), Is.EqualTo(dim).Within(1e-6), "dim at the room end");
            Assert.That(M.LineGlow(run / 2, run, dim), Is.GreaterThan(dim).And.LessThan(1), "brightening toward the door");
            Assert.That(M.Line(door, 0, 0, wd, UnityTuning.ExitFrame61LineDashM, UnityTuning.ExitFrame61LineGapM, lift, thick).Count, Is.GreaterThan(0), "no run in the room: the doorway's dashes still show");
        }

        [Test]
        public void LitLine_ThePlayersProof()
        {
            Assert.That(M.LitLine("kb3", "d6", 1234), Is.EqualTo("[exit61] door lit: room kb3 (door d6, frame 1234)"));
            Assert.That(M.LitLine("kb3c", "d6b", 7).StartsWith("[exit61] door lit: room kb3c", StringComparison.Ordinal), Is.True, "the reel greps this prefix");
        }
    }
}
