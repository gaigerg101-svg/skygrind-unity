// NIGHT28 (builder A2): the rink's triangles and its ONE ice rule (Core/Enemies/Rink28Arena.cs, Rink28Check.cs). The numbers are the design.n28.rink starting values written out here so the test does not
// depend on the config file; Tools/SimHarness `rink28` runs the same checks on the config's numbers and with the real Mover.
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Rink28ArenaTests
    {
        static Rink28Params Prm() => new Rink28Params
        {
            cx = 2800, cy = 0, cz = 0, straight = 24, centreRadius = 5.75, islandRadius = 3, islandHeight = 2.6, doorWidth = 4, laneWidth = 5.5, bankDeg = 28, bankRun = 4.5, lip = 0.4, rimHeight = 5,
            facetDeg = 7.5, lid = true, playerStartS = 4, spawnGap = 30,
        };

        static TriangleWorld WorldOf(Rink28Arena a)
        {
            var w = new TriangleWorld();
            a.FillWorld(w);
            return w;
        }

        [Test]
        public void EveryTriangle_FacesTheRightWay()
        {
            var a = new Rink28Arena(Prm());
            Assert.That(Rink28Check.WindingProblems(a, out string first), Is.EqualTo(0), first);
        }

        [Test]
        public void TheBank_IsFloorGradeAndTheRimIsAWall()
        {
            var a = new Rink28Arena(Prm());
            foreach (Rink28Tri t in a.tris)
            {
                Vec3 n = TriangleWorld.TriangleNormal(t.a, t.b, t.c);
                if (t.part == Rink28Part.Bank) Assert.That(n.y, Is.GreaterThanOrEqualTo(0.7), "the Mover reads normal.y >= 0.7 as floor");
                if (t.part == Rink28Part.Rim) Assert.That(System.Math.Abs(n.y), Is.LessThan(0.3), "the Mover reads |normal.y| < 0.3 as a wall");
            }
            Assert.That(a.bankRise, Is.EqualTo(2.39).Within(0.01));
        }

        [Test]
        public void TheFacets_AreFarUnderTheWallRidesCornerAngle()
        {
            Assert.That(Rink28Check.MaxFacetAngleDeg(new Rink28Arena(Prm())), Is.LessThanOrEqualTo(8.0));
        }

        [Test]
        public void OneIceZone_GivesEveryFloorGradePointTheSameMultiplier()
        {
            var a = new Rink28Arena(Prm());
            var surf = new Surfaces26(new SurfaceRules { iceMult = 0.25, mudMult = 2, boostImpulse = 6 });
            surf.zones.Add(a.IceZone());
            int n = Rink28Check.UniformIce(a, WorldOf(a), surf, 1.0, out int bad, out string first);
            Assert.That(n, Is.GreaterThan(1000), "the grid should find the lane, banks, island top and doors");
            Assert.That(bad, Is.EqualTo(0), first);
        }

        [Test]
        public void ASecondZoneOrAHole_IsCaught()
        {
            var a = new Rink28Arena(Prm());
            var surf = new Surfaces26(new SurfaceRules { iceMult = 0.25, mudMult = 2, boostImpulse = 6 });
            surf.zones.Add(a.IceZone());
            surf.zones.Add(new SurfaceZone { kind = SurfaceKind.Boost, x0 = 2790, x1 = 2800, z0 = -8, z1 = -7 });
            Rink28Check.UniformIce(a, WorldOf(a), surf, 1.0, out int bad, out _);
            Assert.That(bad, Is.GreaterThan(0));
            // an ice rect that stops short of the island leaves its top at x1 (friction 1)
            var surf2 = new Surfaces26(new SurfaceRules { iceMult = 0.25, mudMult = 2, boostImpulse = 6 });
            SurfaceZone z = a.IceZone();
            z.maxY = 1.0; // the island top (2.6 m) falls outside the zone's height range
            surf2.zones.Add(z);
            Rink28Check.UniformIce(a, WorldOf(a), surf2, 1.0, out int bad2, out _);
            Assert.That(bad2, Is.GreaterThan(0));
        }

        [Test]
        public void TheSpawn_IsOnTheLaneAndTheBossStartIsThirtyMetresAhead()
        {
            var a = new Rink28Arena(Prm());
            a.Spawn(out Vec3 pos, out double yaw);
            a.math.Project(pos.x, pos.z, out double s, out double lat);
            Assert.That(s, Is.EqualTo(4).Within(1e-6));
            Assert.That(lat, Is.EqualTo(0).Within(1e-6));
            Assert.That(a.RegionAt(pos.x, pos.z), Is.EqualTo(Rink28Region.Lane));
            Assert.That(yaw, Is.EqualTo(-90).Within(1e-6), "the bottom straight runs +x: yaw -90 looks toward +x");
            Assert.That(a.math.SignedGap(s, a.BossStartS), Is.EqualTo(30).Within(1e-9));
        }

        [Test]
        public void ATriangleLayer_BlocksRaysAndBodies_LikeTheWorld()
        {
            var a = new Rink28Arena(Prm());
            var layer = new BoxLayer();
            a.FillLayer(layer);
            Assert.That(layer.Count, Is.EqualTo(a.tris.Count));
            // a ray down onto the lane hits the floor; a ray along +z at lane height from the lane's middle hits the island wall
            Assert.That(layer.RayIntersect(new Vec3(2808, 3, -5.75), new Vec3(0, -1, 0), out RayHit h), Is.True);
            Assert.That(h.distance, Is.EqualTo(3).Within(1e-6));
            Assert.That(h.normal.y, Is.EqualTo(1).Within(1e-9));
            Assert.That(layer.RayIntersect(new Vec3(2808, 1, -5.75), new Vec3(0, 0, 1), out RayHit w), Is.True);
            Assert.That(w.distance, Is.EqualTo(2.75).Within(1e-6));
        }
    }
}
