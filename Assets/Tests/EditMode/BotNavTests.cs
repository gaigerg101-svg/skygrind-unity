// NIGHT14 U3-27.4 (agent N): the bot's nav and brain (Core/Nav, Core/Bot). The web's own tests (tests/nav.test.js:
// jump reach, A* on a toy graph, the Spire nav graph) ported as they are, plus the port's checks: the Spire graph built
// here equals the one the web built (maps/spire.json duelNavGraph: navWorker.js's graph, core 'towers', with the nav
// grid's jump links), a follower walks a planned path to its end, and a bot run is a pure function of its seed.
// The Spire graph is built once for the fixture with the duel's nav grid (maps/spire-duel-navgrid.json, the web's grid
// with the core at config.core.duelPreset); nav.test.js builds it without the grid: the graph with the grid only adds
// nodes and edges, so its reach tests ask at least as much of the port.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using static Skygrind.Core.PortData;

namespace Skygrind.Core.Tests
{
    /// <summary>The Spire arena as navWorker.js builds it for the duel: the map, the core at 'towers', its grid and graph.</summary>
    public static class BotSpire
    {
        static GameConfig cfg;
        static Dictionary<string, object> json;
        static NavMap map;
        static GameWorld world;
        static CoreGrid core;
        static NavGraph graph;
        public static double buildSeconds;

        public static GameConfig Config => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        public static Dictionary<string, object> Json => json ??= Obj(LoadJson(Path.Combine(Port.Root, "maps", "spire.json")));

        public static NavMap Map => map ??= NavMap.FromExport(Json);

        public static CoreGrid Core
        {
            get
            {
                if (core != null) return core;
                MovementConfig m = Config.movement;
                CoreSpec spec = Map.core.Value;
                core = new CoreGrid(m, spec);
                core.SetHeights(CoreGrid.PresetHeights(Config.Str("core.duelPreset"), spec.cells, m.core.maxHeight, m.core.heightStep, new Mulberry32(1)));
                return core;
            }
        }

        public static GameWorld World => world ??= new GameWorld(BuildMapOctree(Json), Core);

        public static NavGrid Grid() => NavGrid.FromExport(Obj(LoadJson(Path.Combine(Port.Root, "maps", "spire-duel-navgrid.json"))));

        public static NavGraph Graph
        {
            get
            {
                if (graph != null) return graph;
                var t = System.Diagnostics.Stopwatch.StartNew();
                graph = NavGraph.Build(Map, World, Config, Core, Grid());
                buildSeconds = t.Elapsed.TotalSeconds;
                return graph;
            }
        }

        public static Vec3 DuelSpawn(int i) => V3(Arr(Obj(Json["spawns"])["duelSpawns"])[i]);

        /// <summary>A Mover on the Spire's world at `p` (the bot's body).</summary>
        public static Mover Body(Vec3 p)
        {
            var mover = new Mover(Config.movement, World);
            mover.Reset(p.x, p.y, p.z);
            return mover;
        }
    }

    public class BotNavTests
    {
        static MovementConfig C => BotSpire.Config.movement;

        // --- tests/nav.test.js: jump reach -------------------------------------------------------------------------

        [Test]
        public void JumpReach_OrdersTheKinds_OnFlatGround()
        {
            var r = new List<double>();
            foreach (string k in new[] { "jump", "double", "dash", "dashDouble" }) r.Add(Reach.JumpReach(k, 0, C));
            for (int i = 1; i < r.Count; i++) Assert.Greater(r[i], r[i - 1], $"kind {i}");
            Assert.Greater(r[0], 4); // 8 m/s × ~0.67 s
        }

        [Test]
        public void JumpReach_CannotClimbPastTheKindsPeak()
        {
            Assert.AreEqual(0, Reach.JumpReach("jump", C.jump.height + 0.1, C));
            Assert.Greater(Reach.JumpReach("double", C.jump.height + 0.1, C), 0);
            Assert.AreEqual(0, Reach.JumpReach("double", 4, C));
        }

        [Test]
        public void JumpReach_FurtherWhenLandingLower_AndDropsAreGentleArcs()
        {
            Assert.Greater(Reach.JumpReach("jump", -3, C), Reach.JumpReach("jump", 0, C));
            Assert.AreEqual(0, Reach.DropReach(0, C));
            Assert.Greater(Reach.DropReach(4, C), Reach.DropReach(1, C));
        }

        // --- tests/nav.test.js: A* ---------------------------------------------------------------------------------

        static NavGraph Toy()
        {
            // 0 - 1 - 2 - 3 cheap chain, plus a direct 0 -> 3 edge that costs more.
            var g = new NavGraph();
            foreach (double x in new double[] { 0, 1, 2, 3, 50 }) g.nodes.Add(new NavNode { x = x, y = 0, z = 0 });
            NavEdge E(int to, double cost, string kind) => new NavEdge { to = to, cost = cost, kind = kind };
            g.edges.Add(new List<NavEdge> { E(1, 1, "walk"), E(3, 5, "dash") });
            g.edges.Add(new List<NavEdge> { E(2, 1, "walk") });
            g.edges.Add(new List<NavEdge> { E(3, 1, "jump") });
            g.edges.Add(new List<NavEdge>());
            g.edges.Add(new List<NavEdge>());
            return g;
        }

        [Test]
        public void Astar_FindsTheCheapestPathWithItsEdgeKinds()
        {
            var path = new NavPath();
            Assert.IsTrue(Astar.Path(Toy(), 0, 3, 1, path));
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, path.nodes);
            CollectionAssert.AreEqual(new[] { "walk", "walk", "jump" }, path.kinds);
            Assert.AreEqual(3, path.cost);
        }

        [Test]
        public void Astar_NullWhenUnreachable_AndAOneNodePathToItself()
        {
            var path = new NavPath();
            Assert.IsFalse(Astar.Path(Toy(), 0, 4, 1, path));
            Assert.IsTrue(Astar.Path(Toy(), 2, 2, 1, path));
            CollectionAssert.AreEqual(new[] { 2 }, path.nodes);
        }

        [Test]
        public void NearestNode_WeighsHeightDouble()
        {
            var g = new NavGraph();
            g.nodes.Add(new NavNode { x = 0, y = 0, z = 0 });
            g.nodes.Add(new NavNode { x = 0, y = 3, z = 2 });
            g.edges.Add(new List<NavEdge>());
            g.edges.Add(new List<NavEdge>());
            Assert.AreEqual(0, Astar.NearestNode(g, new Vec3(0, 1, 1.9))); // plain distance would pick node 1
        }

        // --- tests/nav.test.js: the Spire nav graph (Duel: core on towers) ------------------------------------------

        [Test]
        public void Spire_HasNodesOnEveryZone_AndEdgesOfEveryKind()
        {
            NavGraph graph = BotSpire.Graph;
            TestContext.WriteLine($"spire graph: {graph.nodes.Count} nodes, built in {BotSpire.buildSeconds:0.00} s");
            var kinds = new HashSet<string>();
            foreach (NavNode n in graph.nodes) kinds.Add(n.kind);
            foreach (string k in new[] { "floor", "cover", "ledge", "bridge", "island", "spire", "pillar", "pad" }) Assert.IsTrue(kinds.Contains(k), k);
            var edgeKinds = new HashSet<string>();
            foreach (List<NavEdge> list in graph.edges)
                foreach (NavEdge e in list) edgeKinds.Add(e.kind);
            foreach (string k in new[] { "walk", "drop", "jump", "hop", "double", "climb", "dash", "dashDouble", "pad" }) Assert.IsTrue(edgeKinds.Contains(k), k);
        }

        [Test]
        public void Spire_FliesTheLedgeStairs_OnlyWithAStandingClimbFromFarEnoughBack()
        {
            NavGraph graph = BotSpire.Graph;
            // West ledge at 10 m -> the south ledge at 12.5 m (a 1 m gap, a 2.5 m rise, the next ledge above).
            NavNode from = graph.nodes.Find(n => n.kind == "ledge" && n.y == 10 && n.x > 30 && n.x < 31 && n.z > -31 && n.z < -30);
            NavNode to = graph.nodes.Find(n => n.kind == "ledge" && n.y == 12.5 && n.x > 33 && n.x < 34 && n.z > -31 && n.z < -30);
            Assert.IsNotNull(from);
            Assert.IsNotNull(to);
            Assert.AreEqual(0, graph.Fly(from, to, "jump")); // a running jump hits the ledge's side
            Assert.AreEqual(0, graph.Fly(from, to, "hop")); // one jump is too low
            bool any = false;
            foreach (double back in new[] { 0, 0.9, 1.8 }) any |= graph.Fly(from, to, "climb", back) > 0;
            Assert.IsTrue(any);
        }

        [Test]
        public void Spire_ReachesEverySpireTopAndIsland_FromADuelSpawn()
        {
            NavGraph graph = BotSpire.Graph;
            int spawn = Astar.NearestNode(graph, BotSpire.DuelSpawn(0));
            double speed = BotSpire.Config.movement.dash.speed;
            var path = new NavPath();
            var reached = new HashSet<int>();
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                NavNode n = graph.nodes[i];
                if ((n.kind == "spire" || n.kind == "island") && Astar.Path(graph, spawn, i, speed, path)) reached.Add(n.box);
            }
            // 4 spire tops + 6 islands, each reachable on at least one of its nodes.
            var boxes = new List<int>();
            for (int i = 0; i < BotSpire.Map.boxes.Count; i++)
                if (BotSpire.Map.boxes[i].kind == "spire" || BotSpire.Map.boxes[i].kind == "island") boxes.Add(i);
            Assert.AreEqual(10, boxes.Count);
            var missed = boxes.FindAll(i => !reached.Contains(i)).ConvertAll(i => $"{BotSpire.Map.boxes[i].kind} {BotSpire.Map.boxes[i].min}");
            CollectionAssert.IsEmpty(missed);
        }

        [Test]
        public void Spire_EveryFloorNodeCanGetBackToTheOtherSpawn()
        {
            NavGraph graph = BotSpire.Graph;
            int other = Astar.NearestNode(graph, BotSpire.DuelSpawn(1));
            double speed = BotSpire.Config.movement.dash.speed;
            var path = new NavPath();
            int stranded = 0;
            for (int i = 0; i < graph.nodes.Count; i++)
                if (graph.nodes[i].kind == "floor" && !Astar.Path(graph, i, other, speed, path)) stranded++;
            Assert.AreEqual(0, stranded);
        }

        // --- the port's checks -------------------------------------------------------------------------------------

        [Test, Description("buildNavGraph on the Spire (core 'towers', the duel's nav grid) equals maps/spire.json duelNavGraph: every node and edge, bit for bit")]
        public void Spire_Graph_EqualsTheWebsDuelNavGraph()
        {
            NavGraph graph = BotSpire.Graph;
            var want = Obj(BotSpire.Json["duelNavGraph"]);
            var r = new GoldenResult("spire duelNavGraph");
            GoldenBot.CompareGraph(want, graph, BotSpire.Map, r);
            TestContext.WriteLine($"{r.Summary}; built in {BotSpire.buildSeconds:0.00} s");
            Assert.IsTrue(r.Ok, r.Summary + "\n" + string.Join("\n", r.mismatches.GetRange(0, System.Math.Min(40, r.mismatches.Count))));
        }

        [Test, Description("a follower walks an A* path from a duel spawn to the other spawn's node and arrives")]
        public void Follower_FollowsAPlannedPath_ToItsEnd()
        {
            NavGraph graph = BotSpire.Graph;
            MovementConfig m = BotSpire.Config.movement;
            Mover body = BotSpire.Body(BotSpire.DuelSpawn(0));
            var follower = new Follower(BotSpire.Config);
            var path = new NavPath();
            Assert.IsTrue(Astar.Path(graph, Astar.NearestNode(graph, body.position), Astar.NearestNode(graph, BotSpire.DuelSpawn(1)), m.dash.speed, path));
            follower.SetPath(graph, path);
            FollowStatus status = FollowStatus.Moving;
            int steps = 0;
            for (; steps < 120 * 60 && status == FollowStatus.Moving; steps++)
            {
                var frame = new InputFrame();
                status = follower.Step(body, ref frame, 1 / m.loop.simHz);
                body.Step(frame, 1 / m.loop.simHz);
            }
            TestContext.WriteLine($"{path.nodes.Count} nodes, cost {path.cost:0.00} s: {status} after {steps} steps");
            Assert.AreEqual(FollowStatus.Arrived, status);
        }

        /// <summary>A bot on the Spire chasing a target that runs a fixed loop; the frames it wrote, one line per step.</summary>
        static List<string> BotRun(uint seed, string difficulty, int steps)
        {
            MovementConfig m = BotSpire.Config.movement;
            double dt = 1 / m.loop.simHz;
            Mover body = BotSpire.Body(BotSpire.DuelSpawn(0));
            var health = new Health(100, new EventBus(), "rival");
            var target = new BotTarget();
            var deps = new BotDeps { getPlayer = () => target, ray = BotSpire.World.RayIntersect, getGraph = () => BotSpire.Graph, rng = new Mulberry32(seed) };
            var brain = new BotBrain(BotSpire.Config, new BotSelf { mover = body, health = health, weapon = () => "revolver" }, deps);
            brain.Reset(difficulty);
            Vec3 spawn = BotSpire.DuelSpawn(1);
            var lines = new List<string>();
            for (int i = 0; i < steps; i++)
            {
                double a = i * dt * 0.4;
                target.position = new Vec3(spawn.x + 6 * JsMath.Sin(a), 0, spawn.z + 6 * JsMath.Cos(a));
                target.velocity = new Vec3(2.4 * JsMath.Cos(a), 0, -2.4 * JsMath.Sin(a));
                var frame = new BotFrame();
                brain.Step(ref frame, dt);
                body.Step(frame.move, dt);
                lines.Add($"{brain.State} {frame.move.moveX:R} {frame.move.moveZ:R} {frame.move.yaw:R} {frame.move.pitch:R} {frame.move.jump.pressed} {frame.move.dash.pressed} {frame.fire.pressed} {frame.weapon} {body.position}");
            }
            return lines;
        }

        [Test, Description("a bot run is a pure function of (seed, difficulty): the same seed twice writes the same frames")]
        public void BotBrain_SameSeedTwice_SameFrames()
        {
            List<string> a = BotRun(7, "hard", 1200);
            List<string> b = BotRun(7, "hard", 1200);
            CollectionAssert.AreEqual(a, b);
            var states = new HashSet<string>();
            foreach (string line in a) states.Add(line.Split(' ')[0]);
            TestContext.WriteLine("states seen: " + string.Join(", ", states));
            Assert.IsTrue(states.Contains("chase"), "the bot chased");
        }
    }
}
