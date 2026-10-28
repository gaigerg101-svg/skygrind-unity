// NIGHT65 builder T (T2 tour65): the reel's pure parts (FeelReel.N65.Tour.cs, Tour65): a room row's frames65 poses in the file's order (doc keys
// skipped), the fallback frames36.mid (every fight room of n52-foundry has it, so rc0's level is framed by the same poses), line65's 8 frames by
// arc length, the planted pose's flag, and the WARN test (a pose inside a collision box of the level). The Player run itself (one per level,
// a proof line per pose) is the lead's: not checked by this test.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N65TourTests
    {
        static readonly string Repo = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        static Dictionary<string, object> Json(string text) => PortData.Obj(JsJson.Parse(text));

        static Dictionary<string, object> Level(string id) => PortData.Obj(PortData.LoadJson(Path.Combine(Repo, "Design", "levels", id + ".json")));

        [Test]
        public void Frames65_AreThePoses_InTheFilesOrder_DocKeysSkipped()
        {
            var row = Json("{\"id\":\"a2\",\"frames65\":{\"_why\":\"x\",\"entry\":[0,0,-36,0,0.1],\"deck\":[-4,2,-51,160],\"bad\":[1]},\"frames36\":{\"mid\":[5,0,-39,37.875,-0.053]}}");
            List<Tour65.Pose> p = Tour65.Poses(row, out string source);
            Assert.That(source, Is.EqualTo("frames65"));
            Assert.That(p.ConvertAll(q => q.name), Is.EqualTo(new[] { "entry", "deck" }), "the file's order; _why and a pose of fewer than 3 numbers skipped");
            Assert.That(p[0].Spot, Is.EqualTo(new[] { 0, 0, -36, 0, 0.1 }));
            Assert.That(p[1].pitch, Is.EqualTo(0), "a pose without its pitch looks level");
            Assert.That(p[1].yawDeg, Is.EqualTo(160));
        }

        [Test]
        public void NoFrames65_FallsBackToFrames36Mid_InEveryFightRoomOfTheOldLevel()
        {
            foreach (object o in PortData.Arr(PortData.Obj(Level("n52-foundry")["campaign"])["rooms"]))
            {
                var row = PortData.Obj(o);
                if ((string)row["kind"] != "room") continue;
                List<Tour65.Pose> p = Tour65.Poses(row, out string source);
                Assert.That(source, Is.EqualTo("frames36.mid"), (string)row["id"]);
                Assert.That(p.Count, Is.EqualTo(1));
                List<object> mid = PortData.Arr(PortData.Obj(row["frames36"])["mid"]);
                Assert.That(p[0].Spot, Is.EqualTo(new[] { PortData.Num(mid[0]), PortData.Num(mid[1]), PortData.Num(mid[2]), PortData.Num(mid[3]), PortData.Num(mid[4]) }), (string)row["id"] + ": its frames36.mid");
                Assert.That(p[0].name, Is.EqualTo("mid36"));
            }
            Assert.That(Tour65.Poses(Json("{\"id\":\"x\"}"), out string none).Count, Is.EqualTo(0));
            Assert.That(none, Is.EqualTo("none"));
            Assert.That(Tour65.Poses(Json("{\"frames65\":{\"_why\":\"only notes\"},\"frames36\":{\"mid\":[1,0,2,0,0]}}"), out string s2)[0].name, Is.EqualTo("mid36"), "a frames65 with no pose falls back too");
            Assert.That(s2, Is.EqualTo("frames36.mid"));
        }

        [Test]
        public void Line65_GivesEightFramesByArcLength_LookingAlongTheLine()
        {
            var row = Json("{\"line65\":[[0,0,0],[0,0,-10],[10,0,-10]]}");
            List<double[]> pts = Tour65.Line(row);
            Assert.That(pts.Count, Is.EqualTo(3));
            List<Tour65.Pose> f = Tour65.LineFrames(pts, UnityTuning.T65LineFrames);
            Assert.That(f.Count, Is.EqualTo(8));
            Assert.That(f[0].x, Is.EqualTo(0).Within(1e-9)); Assert.That(f[0].z, Is.EqualTo(0).Within(1e-9));
            Assert.That(f[7].x, Is.EqualTo(10).Within(1e-9)); Assert.That(f[7].z, Is.EqualTo(-10).Within(1e-9));
            double step = 20.0 / 7;
            for (int k = 1; k < 8; k++)
            {
                double along = (f[k].x - f[0].x) + (f[0].z - f[k].z); // on this L-shaped line: metres north, then metres east
                Assert.That(along, Is.EqualTo(step * k).Within(1e-9), "frame " + (k + 1) + " evenly spaced by arc length");
            }
            Assert.That(f[0].yawDeg, Is.EqualTo(0).Within(1e-9), "north (-z) is yaw 0");
            Assert.That(f[7].yawDeg, Is.EqualTo(-90).Within(1e-9), "east (+x) is yaw -90 (the sim's: yaw = atan2(-dx, -dz))");
            Assert.That(f[0].pitch, Is.EqualTo(UnityTuning.T65LinePitchBias).Within(1e-9), "a level line looks the bias below level");
            Assert.That(f.ConvertAll(q => q.name), Is.EqualTo(new[] { "line1", "line2", "line3", "line4", "line5", "line6", "line7", "line8" }));
            Assert.That(Tour65.Line(Json("{\"line65\":{\"_why\":\"x\",\"points\":[[0,0,0],[1,0,0]]}}")).Count, Is.EqualTo(2), "{points: [...]} reads too");
            Assert.That(Tour65.LineFrames(Tour65.Line(Json("{\"line65\":[[0,0,0]]}")), 8).Count, Is.EqualTo(0), "one point: no sequence");
            var steep = Tour65.LineFrames(Tour65.Line(Json("{\"line65\":[[0,0,0],[0,10,-1]]}")), 8);
            Assert.That(steep[0].pitch, Is.EqualTo(UnityTuning.T65LinePitchMax + UnityTuning.T65LinePitchBias).Within(1e-9), "a steep climb's pitch is clamped");
        }

        [Test]
        public void PlantedPose_ParsesFromTheCommandLine()
        {
            var p = Tour65.Plant(new[] { "SKYGRIND.exe", "-skygrindReelOnly", "tour65", "-skygrindTourPlant65", "a2:1.5,0,-45,90,0.1" });
            Assert.That(p.HasValue, Is.True);
            Assert.That(p.Value.room, Is.EqualTo("a2"));
            Assert.That(p.Value.pose.Spot, Is.EqualTo(new[] { 1.5, 0, -45, 90, 0.1 }));
            Assert.That(p.Value.pose.source, Is.EqualTo("planted"));
            var one = Tour65.Plant(new[] { "-skygrindReelOnly", "tour65", "-skygrindTourPlant65=a8:-4/0/-352.5/180/-0.1" });
            Assert.That(one.HasValue, Is.True, "the one-token form n65-reels.sh can pass (its extra args split on commas)");
            Assert.That(one.Value.room, Is.EqualTo("a8"));
            Assert.That(one.Value.pose.Spot, Is.EqualTo(new[] { -4, 0, -352.5, 180, -0.1 }));
            Assert.That(Tour65.Plant(new[] { "-skygrindTourPlant65", "a2" }).HasValue, Is.False, "no coordinates: no plant");
            Assert.That(Tour65.Plant(new[] { "-skygrindTourPlant65", "a2:1,x,3" }).HasValue, Is.False, "a bad number: no plant");
            Assert.That(Tour65.Plant(new[] { "-skygrindReelOnly", "tour65" }).HasValue, Is.False);
        }

        [Test]
        public void APoseInsideASolid_IsFlagged_APoseOnIt_IsNot()
        {
            List<double[]> solids = Tour65.Solids(Level("n52-foundry"));
            Assert.That(solids.Count, Is.GreaterThan(100), "n52-foundry's collision boxes");
            // a2's north deck: collision box [-7, 0, -55] .. [-1, 2, -50] (2 m high)
            var inDeck = new Tour65.Pose { name = "planted", x = -4, y = 0, z = -52.5 };
            Assert.That(Tour65.InsideSolid(solids, inDeck), Is.EqualTo("feet + eye"), "the feet on the floor under the deck: both points inside it");
            var onDeck = new Tour65.Pose { name = "deck", x = -4, y = 2, z = -52.5 };
            Assert.That(Tour65.InsideSolid(solids, onDeck), Is.Null, "standing ON the deck's top");
            var spawn = new Tour65.Pose { name = "spawn", x = 0, y = 0, z = -37 };
            Assert.That(Tour65.InsideSolid(solids, spawn), Is.Null, "a2's spawn");
            var underLip = new Tour65.Pose { name = "low", x = -4, y = 0.6, z = -52.5 };
            Assert.That(Tour65.InsideSolid(solids, underLip), Is.Not.Null);
        }

        // NIGHT66 builder T (T1a, code-scout.md section 3's S3): the GREEN rule needs no unframed room and no WARN; the old `framed == poses`
        // term was always true. Planted: RED once, GREEN once fixed (the harness twin: `n65tour`'s T66TourCases).
        const string Floor66 = "\"boxes\":[{\"min\":[-50,-1,-50],\"max\":[50,0,50],\"kind\":\"floor\"}]";
        const string Wedge66 = "\"wedges\":[{\"x\":0,\"z\":0,\"yaw\":0,\"length\":8,\"width\":4,\"rise\":2.9}]";

        [Test]
        public void T66_AnUnframedRoom_ReadsRed_ThenGreenOnceItHasAPose()
        {
            var red = Tour65.Dry(Json("{\"collision\":{" + Floor66 + "},\"campaign\":{\"rooms\":[{\"id\":\"p1\",\"kind\":\"room\",\"frames65\":{\"entry\":[10,0,10,0,0]}},{\"id\":\"p2\",\"kind\":\"room\"}]}}"), null);
            Assert.That(red.unframed, Is.EqualTo(new[] { "p2" }), "the fight room with no frames65 and no frames36.mid");
            Assert.That(red.rooms, Is.EqualTo(1));
            Assert.That(red.Green, Is.False, "an unframed room is RED (the old rule read GREEN: framed == poses over the framed rooms only)");
            var green = Tour65.Dry(Json("{\"collision\":{" + Floor66 + "},\"campaign\":{\"rooms\":[{\"id\":\"p1\",\"kind\":\"room\",\"frames65\":{\"entry\":[10,0,10,0,0]}},{\"id\":\"p2\",\"kind\":\"room\",\"frames65\":{\"entry\":[20,0,20,0,0]}}]}}"), null);
            Assert.That(green.unframed.Count, Is.EqualTo(0));
            Assert.That(green.Green, Is.True, "fixed: every fight room has a pose");
            Assert.That(Tour65.Green(1, 0, 1, 0, 0, true, false), Is.False, "a WARN is RED");
            Assert.That(Tour65.Green(0, 0, 0, 0, 0, true, false), Is.False, "no room toured is RED");
            Assert.That(Tour65.Green(1, 0, 0, 0, 0, false, false), Is.False, "not 1920x1080 is RED");
        }

        [Test]
        public void T66_APoseInsideASlideWedge_IsAWarn_ThenGreenOnTheRamp()
        {
            var inWedge = Json("{\"collision\":{" + Floor66 + "," + Wedge66 + "},\"campaign\":{\"rooms\":[{\"id\":\"p1\",\"kind\":\"room\",\"frames65\":{\"ramp\":[0,0,-6,0,0]}}]}}");
            List<double[]> solids = Tour65.Solids(inWedge);
            Assert.That(solids.Count, Is.EqualTo(2), "the floor box and the wedge");
            Assert.That(solids[1].Length, Is.EqualTo(7), "a wedge entry");
            var red = Tour65.Dry(inWedge, null);
            Assert.That(red.warns, Is.EqualTo(1), "6 m up a 2.9 m / 8 m ramp the top is 2.175 m: feet (0.3) and eye (1.5) inside it");
            Assert.That(red.Green, Is.False);
            var green = Tour65.Dry(Json("{\"collision\":{" + Floor66 + "," + Wedge66 + "},\"campaign\":{\"rooms\":[{\"id\":\"p1\",\"kind\":\"room\",\"frames65\":{\"ramp\":[0,2.2,-6,0,0]}}]}}"), null);
            Assert.That(green.warns, Is.EqualTo(0), "standing on the ramp");
            Assert.That(green.Green, Is.True);
            Assert.That(Tour65.InsideSolid(solids, new Tour65.Pose { x = 3, y = 0, z = -6 }), Is.Null, "beside the ramp (across 3 m, half width 2)");
            Assert.That(Tour65.InsideSolid(solids, new Tour65.Pose { x = 0, y = 0, z = 1 }), Is.Null, "behind its low end");
        }
    }
}
