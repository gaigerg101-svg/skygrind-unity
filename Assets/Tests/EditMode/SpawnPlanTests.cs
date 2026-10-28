// The enemy bench's spawn plan against the web bench's (skygrind port-export/bench/spawnPlan.mjs) on the exported
// Survival map: the ground spawns inside config.director's spawn band of the player, then squads of 5.
using System.Collections.Generic;
using NUnit.Framework;
using static Skygrind.Core.PortData;

namespace Skygrind.Core.Tests
{
    public class SpawnPlanTests
    {
        static List<Vec3> Nodes()
        {
            var map = Obj(Port.Json("maps/survival.json"));
            var spawns = Obj(map["spawns"]);
            var director = Obj(Obj(Port.Json("config.json"))["director"]);
            return SpawnPlan.Nodes(Arr(spawns["ground"]).ConvertAll(V3), V3(spawns["playerSpawn"]), Num(director["spawnMin"]), Num(director["spawnMax"]));
        }

        [Test]
        public void Survival_FourGroundNodesInTheSpawnBand()
        {
            // 15-40 m from (0, 0, 40) at y 0, in file order (RESULTS.md: "200 bodies spawned at 4 points").
            List<Vec3> nodes = Nodes();
            Assert.AreEqual(4, nodes.Count);
            Assert.AreEqual(new Vec3(20, 0, 44), nodes[0]);
            Assert.AreEqual(new Vec3(-20, 0, 44), nodes[1]);
            Assert.AreEqual(new Vec3(-28, 0, 27), nodes[2]);
            Assert.AreEqual(new Vec3(27, 0, 28), nodes[3]);
        }

        [Test]
        public void Plan_SquadsOfFiveAtNodeSTimesSeven()
        {
            List<Vec3> plan = SpawnPlan.Plan(12, Nodes());
            Assert.AreEqual(12, plan.Count);
            // Squad 0 at node 0: offsets (i % 2) * 1.5 in x, floor(i / 2) * 1.5 in z.
            Assert.AreEqual(new Vec3(20, 0, 44), plan[0]);
            Assert.AreEqual(new Vec3(21.5, 0, 44), plan[1]);
            Assert.AreEqual(new Vec3(20, 0, 45.5), plan[2]);
            Assert.AreEqual(new Vec3(21.5, 0, 45.5), plan[3]);
            Assert.AreEqual(new Vec3(20, 0, 47), plan[4]);
            // Squad 1 at node 7 % 4 = 3, squad 2 at node 14 % 4 = 2.
            Assert.AreEqual(new Vec3(27, 0, 28), plan[5]);
            Assert.AreEqual(new Vec3(27, 0, 31), plan[9]);
            Assert.AreEqual(new Vec3(-28, 0, 27), plan[10]);
            Assert.AreEqual(new Vec3(-26.5, 0, 27), plan[11]);
        }
    }
}
