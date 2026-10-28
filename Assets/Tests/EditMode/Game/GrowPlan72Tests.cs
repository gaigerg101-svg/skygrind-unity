// NIGHT72 builder L (L5; Game/GrowPlan72.cs, Tools/night/n72-zscan.cjs). Pinned:
//   1. a planted coplanar overlapping pair (two tops on y = 3, 4 m2 of overlap) is a conflict raw; a 0.01 m gap is not
//   2. indices 0 and 3 share a class under NIGHT71's mod 3 and stay coplanar; the plan separates them by at least 4 mm
//   3. a 2 mm raw gap that one class difference would cancel is kept open by the plan
//   4. touching boxes (A max x = B min x) are not a same-facing pair
//   5. on every campaign level file of the repo, the plan leaves 0 coplanar overlapping pairs (under 1 mm), and never more than the index mod 3 growth did
//   6. the plan's classes are all in 0..K-1 and a box with no conflict stays at class 0
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class GrowPlan72Tests
    {
        static MapBox Box(double x0, double y0, double z0, double x1, double y1, double z1) => new MapBox { min = new Vec3(x0, y0, z0), max = new Vec3(x1, y1, z1), kind = "t" };

        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (System.Exception) { return Path.GetFullPath("."); }
        }

        [Test]
        public void ThePlantedPair_IsFound_AndAGapOfOneCentimetreIsNot()
        {
            var planted = new List<MapBox> { Box(0, 0, 0, 4, 3, 4), Box(2, 1, 2, 8, 3, 8) };
            GrowPlan72.Report r = GrowPlan72.Count(planted, new int[2]);
            Assert.AreEqual(1, r.raw, "two tops on y = 3 overlapping 4 m2");
            var gap = new List<MapBox> { Box(0, 0, 0, 4, 3, 4), Box(2, 1, 2, 8, 3.01, 8) };
            Assert.AreEqual(0, GrowPlan72.Count(gap, new int[2]).raw, "a 0.01 m gap is not a pair");
        }

        [Test]
        public void IndicesZeroAndThree_ShareAClassUnderMod3_ThePlanSeparatesThem()
        {
            var boxes = new List<MapBox> { Box(0, 0, 0, 4, 3, 4), Box(10, 0, 0, 12, 1, 2), Box(20, 0, 0, 22, 1, 2), Box(2, 1, 2, 8, 3, 8) };
            int[] cls = GrowPlan72.Classes(boxes);
            GrowPlan72.Report r = GrowPlan72.Count(boxes, cls);
            Assert.AreEqual(1, r.mod3, "indices 0 and 3 are the same class under mod 3: still one plane");
            Assert.AreEqual(0, r.plan, "the plan leaves none");
            Assert.AreNotEqual(cls[0], cls[3]);
            Assert.GreaterOrEqual(System.Math.Abs(GrowPlan72.Growth(cls[3]) - GrowPlan72.Growth(cls[0])), UnityTuning.L72GrowSepGood - 1e-9, "at least 4 mm apart");
        }

        [Test]
        public void ARawGapOneClassDifferenceWouldCancel_StaysOpen()
        {
            var boxes = new List<MapBox> { Box(0, 0, 0, 4, 3, 4), Box(2, 1, 2, 8, 3.002, 8) }; // box 1's top is 2 mm farther out
            Assert.AreEqual(1, GrowPlan72.Count(boxes, new[] { 1, 0 }).plan, "classes (1, 0) would cancel it: grown tops at 3.004 and 3.004");
            int[] cls = GrowPlan72.Classes(boxes);
            Assert.AreEqual(0, GrowPlan72.Count(boxes, cls).plan);
            double gap = System.Math.Abs(3.002 + GrowPlan72.Growth(cls[1]) - (3.0 + GrowPlan72.Growth(cls[0])));
            Assert.GreaterOrEqual(gap, UnityTuning.L72GrowSepGood - 1e-9, "kept at least 4 mm open");
        }

        [Test]
        public void TouchingBoxes_AreNotASameFacingPair()
        {
            var boxes = new List<MapBox> { Box(0, 0, 0, 4, 3, 4), Box(4, 0, 0, 8, 3, 4) };
            // they share the faces y = 0 and y = 3 and z = 0 and z = 4 (same side, overlapping 0 m2 across the shared x edge): only touching on x
            Assert.AreEqual(0, GrowPlan72.Count(boxes, new int[2]).raw);
        }

        [Test]
        public void ClassesStayInRange_AndAnUnconflictedBoxStaysAtZero()
        {
            var boxes = new List<MapBox> { Box(0, 0, 0, 4, 3, 4), Box(100, 0, 0, 104, 3, 4), Box(2, 1, 2, 8, 3, 8) };
            int[] cls = GrowPlan72.Classes(boxes);
            foreach (int c in cls) Assert.That(c, Is.InRange(0, UnityTuning.L72GrowClasses - 1));
            Assert.AreEqual(0, cls[1], "far from every other box: class 0");
            Assert.AreEqual(0, GrowPlan72.Classes(new List<MapBox>()).Length);
            Assert.AreEqual(1, GrowPlan72.Classes(new List<MapBox> { Box(0, 0, 0, 1, 1, 1) }).Length);
        }

        [Test]
        public void OnEveryCampaignLevelFile_ThePlanLeavesNoCoplanarOverlappingPair()
        {
            string dir = Path.Combine(Repo(), "Assets", "StreamingAssets", "Design", "levels");
            int checkedLevels = 0;
            foreach (string name in new[] { "n52-foundry", "n52-iceline", "n57-kiln", "n58-span", "n52-spire", "n65-foundry", "n67-iceline", "n67-kiln", "n67-spire" })
            {
                string path = Path.Combine(dir, name + ".json");
                if (!File.Exists(path)) continue;
                SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
                int[] cls = GrowPlan72.Classes(map.boxes);
                GrowPlan72.Report r = GrowPlan72.Count(map.boxes, cls);
                TestContext.WriteLine(GrowPlan72.LogLine(name, r));
                Assert.AreEqual(0, r.plan, name + ": pairs left after the plan");
                Assert.LessOrEqual(r.plan, r.mod3, name + ": never worse than the index mod 3");
                Assert.Greater(r.raw, 0, name + ": the scan finds the level's own coplanar pairs (a scan that finds none proves nothing)");
                checkedLevels++;
            }
            Assert.GreaterOrEqual(checkedLevels, 5, "the five shipped levels were read");
        }
    }
}
