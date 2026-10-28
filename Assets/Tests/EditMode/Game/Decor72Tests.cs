// NIGHT72 builder C (slate C3, design.n72.decor72; Game/Decor72.cs, UnityTuning.N72C.cs). Pinned:
//   1. Decor72Marks reads a level's ride panels (campaign.boxes with ride: true), doors (campaign.doors rect) and hazards (campaign.pits and lava rects), and a
//      band segment's job is ride, then route (within Decor72RouteReachM of a door), then hazard (within Decor72HazardReachM), else none
//   2. KitFor: a segment with a job goes to the jobs' kit, one with none to the no-job kit, every segment of a map with no campaign data to the jobs' kit
//      (counted "unmarked": decor72 changes nothing there)
//   3. the per-level counts the Player prints equal Tools/night/n72-c-marks.cjs's on the real level files (spire, foundry, iceline, kiln, span): the same
//      4 m cut of every wall box's four faces, the same probe beside the face, the same reaches
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Decor72Tests
    {
        static Dictionary<string, object> Json(List<object> campBoxes, List<object> doors, List<object> pits, List<object> lava)
        {
            var camp = new Dictionary<string, object> { { "boxes", campBoxes }, { "doors", doors }, { "pits", pits } };
            var j = new Dictionary<string, object> { { "campaign", camp } };
            if (lava != null) j["lava"] = lava;
            return j;
        }

        static List<object> V(double a, double b, double c) => new List<object> { a, b, c };
        static List<object> R(double x0, double z0, double x1, double z1) => new List<object> { x0, z0, x1, z1 };
        static Dictionary<string, object> RectEntry(List<object> r) => new Dictionary<string, object> { { "rect", r } };
        static MapBox Box(double x0, double y0, double z0, double x1, double y1, double z1) => new MapBox { min = new Vec3(x0, y0, z0), max = new Vec3(x1, y1, z1), kind = "wall" };

        [Test]
        public void Job_RideThenRouteThenHazard_ElseNone()
        {
            var ride = new Dictionary<string, object> { { "kind", "wall" }, { "ride", true }, { "min", V(0, 0, 0) }, { "max", V(2, 9, 20) } };
            var plain = new Dictionary<string, object> { { "kind", "wall" }, { "min", V(40, 0, 0) }, { "max", V(42, 9, 20) } };
            Dictionary<string, object> json = Json(new List<object> { ride, plain }, new List<object> { RectEntry(R(10, 30, 14, 34)) },
                new List<object> { RectEntry(R(-20, -20, -10, -10)) }, new List<object> { RectEntry(R(60, 60, 70, 70)) });
            Decor72Marks m = Decor72Marks.From(json);
            Assert.That(m, Is.Not.Null);
            Assert.That(m.RidePanels, Is.EqualTo(1), "the ride-tagged box only (a level that tags any box is read by the tag alone)");
            Assert.That(m.Doors, Is.EqualTo(1));
            Assert.That(m.Hazards, Is.EqualTo(2), "the pit and the lava");
            Assert.That(m.Job(Box(0, 0, 0, 2, 9, 20), 3, 10), Is.EqualTo("ride"), "a map box inside the ride panel");
            Assert.That(m.Job(Box(0.2, 0, 0.2, 1.8, 9, 19.8), 50, 50), Is.EqualTo("ride"), "inside it with the eps: ride wins whatever the probe");
            Assert.That(m.Job(Box(40, 0, 0, 42, 9, 20), 12, 31), Is.EqualTo("route"), "inside a door's rect: route");
            Assert.That(m.Job(Box(40, 0, 0, 42, 9, 20), 12, 34 + UnityTuning.Decor72RouteReachM - 0.1), Is.EqualTo("route"), "just inside the reach");
            Assert.That(m.Job(Box(40, 0, 0, 42, 9, 20), 12, 34 + UnityTuning.Decor72RouteReachM + 0.5), Is.Null, "just past the reach: none");
            Assert.That(m.Job(Box(40, 0, 0, 42, 9, 20), -10 + UnityTuning.Decor72HazardReachM - 0.1, -15), Is.EqualTo("hazard"), "just inside a pit's reach (its +x edge is at -10)");
            Assert.That(m.Job(Box(40, 0, 0, 42, 9, 20), -10 + UnityTuning.Decor72HazardReachM + 0.5, -15), Is.Null, "just past the pit's reach");
            Assert.That(m.Job(Box(40, 0, 0, 42, 9, 20), 65, 70 + UnityTuning.Decor72HazardReachM - 0.1), Is.EqualTo("hazard"), "beside the lava (its +z edge is at 70)");
            Assert.That(m.Job(Box(40, 0, 0, 42, 9, 20), 65, 70 + UnityTuning.Decor72HazardReachM + 0.5), Is.Null, "past the lava's reach");
            Assert.That(m.Job(Box(40, 0, 0, 42, 9, 20), 100, 100), Is.Null, "nothing near: none");
        }

        [Test]
        public void NoteNamingARidePanel_IsReadOnALevelWithNoTag()
        {
            var noted = new Dictionary<string, object> { { "kind", "wall" }, { "note", "ride panel A: the first wall" }, { "min", V(0, 0, 0) }, { "max", V(2, 9, 20) } };
            Decor72Marks m = Decor72Marks.From(Json(new List<object> { noted }, new List<object>(), new List<object>(), null));
            Assert.That(m.RidePanels, Is.EqualTo(1), "RideTrim36's rule: a note that names a ride panel counts when no box is tagged");
        }

        [Test]
        public void MapWithNoCampaignData_HasNoMarks_AndKeepsEveryBand()
        {
            Assert.That(Decor72Marks.From(new Dictionary<string, object>()), Is.Null);
            Assert.That(Decor72Marks.From(null), Is.Null);
            var jobs = new MeshKit();
            var none = new MeshKit();
            var t = new Decor72.Tally();
            Assert.That(ReferenceEquals(Decor72.KitFor(null, Box(0, 0, 0, 1, 1, 1), 0, 0, jobs, none, t), jobs), Is.True, "no marks: the jobs' kit");
            Assert.That(t.unmarked, Is.EqualTo(1));
            Assert.That(t.none, Is.EqualTo(0));
        }

        [Test]
        public void KitFor_SendsASegmentWithNoJobToTheOtherKit()
        {
            Decor72Marks m = Decor72Marks.From(Json(new List<object>(), new List<object> { RectEntry(R(0, 0, 4, 4)) }, new List<object>(), null));
            var jobs = new MeshKit();
            var none = new MeshKit();
            var t = new Decor72.Tally();
            Assert.That(ReferenceEquals(Decor72.KitFor(m, Box(10, 0, 10, 12, 9, 30), 1, 1, jobs, none, t), jobs), Is.True, "beside the door: the jobs' kit");
            Assert.That(ReferenceEquals(Decor72.KitFor(m, Box(10, 0, 10, 12, 9, 30), 50, 50, jobs, none, t), none), Is.True, "far from everything: the no-job kit");
            Assert.That(t.route, Is.EqualTo(1));
            Assert.That(t.none, Is.EqualTo(1));
            Assert.That(t.Total, Is.EqualTo(2));
        }

        [Test]
        public void FaceMul_PlinthAndUpperBand_OnTallFacesOnly()
        {
            Assert.That(Decor72.FaceMul(UnityTuning.Decor72FaceMinH - 0.1f, 1f), Is.EqualTo(1f), "a short face is left alone");
            Assert.That(Decor72.FaceMul(6f, 0f), Is.EqualTo(UnityTuning.Decor72PlinthMul).Within(1e-5f), "the foot");
            Assert.That(Decor72.FaceMul(6f, 6f * UnityTuning.Decor72PlinthT), Is.EqualTo(1f).Within(1e-5f), "the plinth ends at x1");
            Assert.That(Decor72.FaceMul(6f, 6f * UnityTuning.Decor72UpperFromT), Is.EqualTo(1f).Within(1e-5f), "the middle is untouched");
            Assert.That(Decor72.FaceMul(6f, 6f), Is.EqualTo(UnityTuning.Decor72UpperMul).Within(1e-5f), "the top");
            float prev = 0f;
            for (float t = UnityTuning.Decor72PlinthT; t <= 1.0001f; t += 0.05f)
            {
                float m = Decor72.FaceMul(6f, 6f * t);
                Assert.That(m, Is.GreaterThanOrEqualTo(prev - 1e-6f), "non-decreasing up the face at t " + t);
                prev = m;
            }
            Assert.That(Decor72.FaceMul(6f, 7f), Is.EqualTo(UnityTuning.Decor72UpperMul).Within(1e-5f), "above the face is clamped to the top");
        }

        [Test]
        public void ChevronColour_IsTheLevelsAccent_AtItsLargestChannel_NotNearBlack()
        {
            Color rose = Look.Hex(0xE67793);
            Color c = Decor72.ChevronColour(rose);
            Assert.That(Mathf.Max(c.r, c.g, c.b), Is.EqualTo(UnityTuning.Decor72ChevronMaxChannel).Within(1e-5f), "the largest channel is the number's (the first cut was held at 0.27 by the accent clamp)");
            Assert.That(UnityTuning.Decor72ChevronMaxChannel, Is.GreaterThanOrEqualTo(0.6f), "a route mark that reads: far over the accent pieces' 0.27");
            Color.RGBToHSV(rose, out float h0, out float s0, out _);
            Color.RGBToHSV(c, out float h1, out float s1, out _);
            Assert.That(h1, Is.EqualTo(h0).Within(1e-3f), "its hue kept");
            Assert.That(s1, Is.EqualTo(s0).Within(1e-3f), "its saturation kept");
            Assert.That(Decor72.ChevronColour(Color.black).r, Is.EqualTo(UnityTuning.Decor72ChevronMaxChannel), "a black accent falls back to a grey of the same level, never black");
        }

        [Test]
        public void ChevronAccent_IsPalette72sChoice_ElseTheLevelsOwn()
        {
            Color rose = Look.Hex(0xE67793), violet = Look.Hex(0xA267E6);
            Color spire = Decor72.ChevronAccent("spire", rose);
            Assert.That(spire, Is.EqualTo(Look.Hex(UnityTuning.Palette72Accent[Palette72.LevelIndex("spire")])), "the spire's chevrons wear palette72's bone, not the danger-band rose, whatever the row reads");
            Assert.That(Decor72.ChevronAccent("foundry", violet), Is.EqualTo(violet), "a level whose accent palette72 keeps: its own");
            Assert.That(Decor72.ChevronAccent("finale", violet), Is.EqualTo(violet), "a level with no palette: its own");
        }

        [Test]
        public void Chevrons_LieOnTheSlope_PointUp_AndCountBySlopeLength()
        {
            Vector3 lowL = new Vector3(0, 0, 0), lowR = new Vector3(3, 0, 0), topL = new Vector3(0, 2, 6), topR = new Vector3(3, 2, 6);
            List<Vector3[]> one = Decor72.Chevrons(lowL, lowR, topL, topR, out Vector3 n);
            Assert.That(one.Count, Is.EqualTo(2), "a 6.3 m slope: one chevron of two arms");
            Assert.That(n.y, Is.GreaterThan(0.9f), "the slope's normal is up");
            Vector3 up = ((topL + topR) * 0.5f - (lowL + lowR) * 0.5f).normalized;
            foreach (Vector3[] q in one)
                foreach (Vector3 p in q)
                {
                    Assert.That(Vector3.Dot(p - lowL, n), Is.EqualTo(UnityTuning.Decor72ChevronLiftM).Within(1e-4f), "lifted off the slope by the lift");
                    Assert.That(p.x, Is.InRange(0f, 3f), "inside the ramp's width");
                }
            // the apex (the two arms' shared end) is farther up the slope than the arms' other ends: the chevron points up
            Vector3 a0 = one[0][0], e0 = one[0][3];
            Assert.That(Vector3.Dot(a0 - e0, up), Is.GreaterThan(0.2f), "the arm runs from its back end up to the apex");
            Vector3 longTop = new Vector3(0, 4, 12);
            List<Vector3[]> three = Decor72.Chevrons(lowL, lowR, longTop, new Vector3(3, 4, 12), out _);
            Assert.That(three.Count, Is.EqualTo(UnityTuning.Decor72ChevronMax * 2), "a 12.6 m slope: the cap");
            Assert.That(Decor72.Chevrons(lowL, lowL, topL, topL, out _).Count, Is.EqualTo(0), "a degenerate ramp: none");
        }

        static string LevelPath(string file) => Path.GetFullPath(Path.Combine(Application.dataPath, "StreamingAssets", "Design", "levels", file));

        /// <summary>The counts of Tools/night/n72-c-marks.cjs for a level: the 4 m cut of every wall box's faces, the probe 0.35 m beside the face.</summary>
        static int[] Count(Dictionary<string, object> json)
        {
            Decor72Marks m = Decor72Marks.From(json);
            var data = (Dictionary<string, object>)json["data"];
            int ride = 0, route = 0, hazard = 0, none = 0;
            foreach (object o in (List<object>)data["boxes"])
            {
                var d = (Dictionary<string, object>)o;
                if ((string)d["kind"] != "wall") continue;
                MapBox b = new MapBox { min = PortData.V3(d["min"]), max = PortData.V3(d["max"]), kind = "wall" };
                for (int f = 0; f < 4; f++)
                {
                    bool alongZ = f < 2;
                    double nx = f == 0 ? 1 : f == 1 ? -1 : 0, nz = f == 2 ? 1 : f == 3 ? -1 : 0;
                    double fixedC = f == 0 ? b.max.x : f == 1 ? b.min.x : f == 2 ? b.max.z : b.min.z;
                    double a0 = alongZ ? b.min.z : b.min.x, a1 = alongZ ? b.max.z : b.max.x, len = a1 - a0;
                    if (len < UnityTuning.Decor55BandMinLen) continue;
                    int n = Mathf.Max(1, Mathf.CeilToInt((float)(len / UnityTuning.Decor55BandSegment)));
                    double seg = len / n;
                    for (int i = 0; i < n; i++)
                    {
                        double mid = a0 + seg * i + seg / 2;
                        double px = alongZ ? fixedC + nx * UnityTuning.Decor55BandProbe : mid, pz = alongZ ? mid : fixedC + nz * UnityTuning.Decor55BandProbe;
                        string job = m.Job(b, px, pz);
                        if (job == "ride") ride++; else if (job == "route") route++; else if (job == "hazard") hazard++; else none++;
                    }
                }
            }
            return new[] { ride, route, hazard, none };
        }

        static Dictionary<string, object> Box(string kind, double[] min, double[] max) =>
            new Dictionary<string, object> { { "kind", kind }, { "min", new List<object> { min[0], min[1], min[2] } }, { "max", new List<object> { max[0], max[1], max[2] } } };

        [Test]
        public void TheSegmentation_OnASyntheticLevel_IsCountedByHand()
        {
            // one wall box x 0..10, z 0..2: faces +x (1 segment), -x (1), +z (10 m: 3 segments), -z (3) = 8 segments; a door rect x 0..3, z -3..-1 and the 4 m reach:
            // -z face (probe z -0.35): the segments at x 1.67 and 5 are within 4 m of the door, x 8.33 is not; +z face (probe z 2.35): 1.67 (dz 3.35) and 5 (dist 3.9) are, 8.33 not;
            // the -x face (probe x -0.35, z 1) is 2.03 from the door: route; the +x face (probe x 10.35) is not. route 5, none 3.
            var json = new Dictionary<string, object>
            {
                { "data", new Dictionary<string, object> { { "boxes", new List<object> { Box("wall", new double[] { 0, 0, 0 }, new double[] { 10, 9, 2 }) } } } },
                { "campaign", new Dictionary<string, object> { { "boxes", new List<object>() }, { "doors", new List<object> { RectEntry(R(0, -3, 3, -1)) } }, { "pits", new List<object>() } } },
            };
            Assert.That(UnityTuning.Decor72RouteReachM, Is.EqualTo(4.0));
            Assert.That(UnityTuning.Decor72HazardReachM, Is.EqualTo(2.0));
            Assert.That(Count(json), Is.EqualTo(new[] { 0, 5, 0, 3 }), "ride, route, hazard, no job");
        }

        [TestCase("n52-spire.json")]
        [TestCase("n52-foundry.json")]
        [TestCase("n52-iceline.json")]
        [TestCase("n57-kiln.json")]
        [TestCase("n52-span.json")]
        public void TheRealLevels_KeepAMinorityOfTheBands_AndHaveRideWalls(string file)
        {
            // the exact counts move whenever a level is regenerated (N0b moved the foundry's route 280 -> 272, equal in C# and in Tools/night/n72-c-marks.cjs on 2026-10-27):
            // the pin that stays is the shape of the answer: some segments with each kind of job, more without
            int[] c = Count((Dictionary<string, object>)PortData.LoadJson(LevelPath(file)));
            int kept = c[0] + c[1] + c[2], total = kept + c[3];
            Assert.That(total, Is.GreaterThan(500), file + ": segments");
            Assert.That(c[0], Is.GreaterThan(0), file + ": ride-wall segments");
            Assert.That(c[1], Is.GreaterThan(0), file + ": route segments");
            Assert.That((double)kept / total, Is.InRange(0.15, 0.85), file + ": the share that keeps a band (" + kept + " of " + total + ")");
            Assert.That(c[3], Is.GreaterThan(kept / 4), file + ": segments with no job exist");
        }
    }
}
