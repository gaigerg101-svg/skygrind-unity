// NIGHT14 U3-27.4 (agent N): the bot's harness commands. From the repo root:
//   dotnet run --project Tools/SimHarness -c Release -- <command>
//   spire-graph    build the Spire's duel graph (core 'towers', maps/spire-duel-navgrid.json) and compare it with
//                  maps/spire.json duelNavGraph, node by node and edge by edge (exit 1 on a mismatch)
//   graph-time     build the Survival map's graph (the run's geometry: static map + flat core, the nav grid), print
//                  nodes / edges / stats / ms; then BotNav.Survival on a headless sim (the cache)
//   reach          jumpReach / dropReach for a few kinds and heights (to hold against the web's numbers)
//   bot-session [seed] [difficulty] [steps]   a bot on the Spire chasing a target on a loop: states, plans, the end
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static Dictionary<string, object> spireJson;

    static Dictionary<string, object> SpireJson() => spireJson ??= PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "spire.json")));

    /// <summary>The Spire's duel world: the map's Octree and the core at config.core.duelPreset.</summary>
    static (NavMap map, GameWorld world, CoreGrid core) SpireWorld(GameConfig cfg)
    {
        NavMap map = NavMap.FromExport(SpireJson());
        MovementConfig m = cfg.movement;
        var core = new CoreGrid(m, map.core.Value);
        core.SetHeights(CoreGrid.PresetHeights(cfg.Str("core.duelPreset"), map.core.Value.cells, m.core.maxHeight, m.core.heightStep, new Mulberry32(1)));
        return (map, new GameWorld(PortData.BuildMapOctree(SpireJson()), core), core);
    }

    static NavGraph SpireGraph(GameConfig cfg, out double ms)
    {
        var (map, world, core) = SpireWorld(cfg);
        NavGrid grid = NavGrid.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "spire-duel-navgrid.json"))));
        var t = Stopwatch.StartNew();
        NavGraph graph = NavGraph.Build(map, world, cfg, core, grid);
        ms = t.Elapsed.TotalMilliseconds;
        return graph;
    }

    static int EdgeCount(NavGraph g)
    {
        int n = 0;
        foreach (List<NavEdge> l in g.edges) n += l.Count;
        return n;
    }

    static partial void Bot(string cmd, string[] args, ref int code)
    {
        switch (cmd)
        {
            case "spire-graph":
            {
                GameConfig cfg = Config();
                NavGraph graph = SpireGraph(cfg, out double ms);
                var r = new GoldenResult("spire duelNavGraph");
                GoldenBot.CompareGraph(PortData.Obj(SpireJson()["duelNavGraph"]), graph, NavMap.FromExport(SpireJson()), r);
                Console.WriteLine($"spire graph: {graph.nodes.Count} nodes, {EdgeCount(graph)} edges, stats flights {graph.statsFlights}, gridLinks {graph.gridTried} / {graph.gridAdded}, built in {ms:0} ms");
                Console.WriteLine((r.Ok ? "PASS " : "FAIL ") + r.Summary);
                for (int i = 0; i < Math.Min(30, r.mismatches.Count); i++) Console.WriteLine("   " + r.mismatches[i]);
                code = r.Ok ? 0 : 1;
                return;
            }
            case "graph-time":
            {
                GameConfig cfg = Config();
                SurvivalMap map = Map();
                CoreGrid core = map.CreateCore(cfg.movement);
                var world = new GameWorld(map.BuildOctree(), core);
                NavGrid grid = NavGrid.FromExport(map.navGrid);
                var t = Stopwatch.StartNew();
                NavGraph graph = NavGraph.Build(NavMap.From(map), world, cfg, core, grid);
                double ms = t.Elapsed.TotalMilliseconds;
                var kinds = new SortedDictionary<string, int>();
                foreach (NavNode n in graph.nodes) kinds[n.kind] = kinds.TryGetValue(n.kind, out int c) ? c + 1 : 1;
                Console.WriteLine($"survival graph: {graph.nodes.Count} nodes ({string.Join(", ", kinds)}), {EdgeCount(graph)} edges, stats flights {graph.statsFlights}, gridLinks {graph.gridTried} / {graph.gridAdded}, built in {ms:0} ms");
                var vectors = PortData.Obj(PortData.LoadJson(Path.Combine(Root, "vectors", "navGraph.json")));
                var want = PortData.Obj(PortData.Arr(vectors["graphs"])[1]);
                var r = new GoldenResult("survival graph vs p22 navGraph.json graphs[1]");
                GoldenBot.CompareGraph(want, graph, NavMap.From(map), r);
                Console.WriteLine((r.Ok ? "PASS " : "FAIL ") + r.Summary);
                if (args.Length > 1 && args[1] == "cache")
                {
                    BotNav.CacheDir = Path.GetFullPath(Path.Combine("Temp", "BotNav"));
                    for (int k = 0; k < 2; k++)
                    {
                        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
                        var t2 = Stopwatch.StartNew();
                        NavGraph g2 = BotNav.Survival(sim);
                        var r2 = new GoldenResult("BotNav.Survival vs p22");
                        GoldenBot.CompareGraph(want, g2, NavMap.From(map), r2);
                        Console.WriteLine($"BotNav.Survival (pass {k + 1}): {(g2 == null ? "null" : $"{g2.nodes.Count} nodes, {EdgeCount(g2)} edges")} in {t2.Elapsed.TotalMilliseconds:0} ms ({BotNav.LastSource}); {r2.Summary}");
                    }
                }
                code = 0;
                return;
            }
            case "reach":
            {
                MovementConfig m = Config().movement;
                foreach (string k in Reach.JUMP_KINDS)
                {
                    var line = new List<string>();
                    foreach (double dy in new[] { -6, -3, -1, 0, 0.5, 1, 1.5, 1.6, 2, 3, 4 }) line.Add($"{dy}:{Reach.JumpReach(k, dy, m):R}");
                    Console.WriteLine($"jumpReach {k}: {string.Join(" ", line)}");
                }
                var drops = new List<string>();
                foreach (double d in new[] { -1, 0, 1, 2.5, 4, 10 }) drops.Add($"{d}:{Reach.DropReach(d, m):R}");
                Console.WriteLine("dropReach: " + string.Join(" ", drops));
                code = 0;
                return;
            }
            case "bot-alloc":
            {
                // BotZeroAllocTests' runs on CoreCLR (GC.GetAllocatedBytesForCurrentThread around each counted step): a
                // fast check beside the Unity test, which is the gate (Mono's GC.Alloc samples).
                GameConfig cfg = Config();
                NavGraph graph = SpireGraph(cfg, out _);
                var (_, world, _) = SpireWorld(cfg);
                MovementConfig m = cfg.movement;
                double dt = 1 / m.loop.simHz;
                var spawns = PortData.Arr(PortData.Obj(SpireJson()["spawns"])["duelSpawns"]);
                Vec3 s0 = PortData.V3(spawns[0]), s1 = PortData.V3(spawns[1]);
                var body = new Mover(m, world);
                body.Reset(s0.x, s0.y, s0.z);
                var health = new Health(100, new EventBus(), "rival");
                var target = new BotTarget();
                var deps = new BotDeps { getPlayer = () => target, ray = world.RayIntersect, getGraph = () => graph, rng = new Mulberry32(3) };
                deps.orbs.Add(new BotOrb { position = new Vec3(0, 0, 0), active = false });
                var brain = new BotBrain(cfg, new BotSelf { mover = body, health = health, weapon = () => "revolver" }, deps);
                brain.Reset("hard");
                var states = new SortedDictionary<string, int>();
                long bytes = 0;
                for (int i = 0; i < 3600; i++)
                {
                    double a = i * dt * 0.4;
                    target.position = new Vec3(s1.x + 8 * JsMath.Sin(a), 0, s1.z + 8 * JsMath.Cos(a));
                    target.velocity = new Vec3(3.2 * JsMath.Cos(a), 0, -3.2 * JsMath.Sin(a));
                    if (i % 900 == 450) health.hp = 20;
                    if (i % 900 == 0) health.hp = 100;
                    if (i % 300 == 150 && brain.visible) brain.OnPlayerFire(new Vec3(target.position.x, 1.6, target.position.z), Vec3.Normalize(body.position - target.position));
                    var frame = new BotFrame();
                    long before = GC.GetAllocatedBytesForCurrentThread();
                    brain.Step(ref frame, dt);
                    long after = GC.GetAllocatedBytesForCurrentThread();
                    if (i >= 1800) bytes += after - before;
                    body.Step(frame.move, dt);
                    states[brain.State] = states.TryGetValue(brain.State, out int c) ? c + 1 : 1;
                }
                Console.WriteLine($"BotBrain.Step: {bytes} B allocated over steps 1800..3599 (states over the run: {string.Join(", ", states)})");
                var follower = new Follower(cfg);
                var path = new NavPath();
                body.Reset(s0.x, s0.y, s0.z);
                int goal = Astar.NearestNode(graph, s1), home = Astar.NearestNode(graph, s0), legs = 0;
                long fbytes = 0;
                for (int i = 0; i < 4800; i++)
                {
                    if (!follower.Active)
                    {
                        Astar.Path(graph, Astar.NearestNode(graph, body.position), legs % 2 == 0 ? goal : home, m.dash.speed, path);
                        follower.SetPath(graph, path);
                        legs++;
                    }
                    var frame = new InputFrame();
                    long before = GC.GetAllocatedBytesForCurrentThread();
                    follower.Step(body, ref frame, dt);
                    long after = GC.GetAllocatedBytesForCurrentThread();
                    if (i >= 1200) fbytes += after - before;
                    body.Step(frame, dt);
                }
                Console.WriteLine($"Follower.Step: {fbytes} B allocated over steps 1200..4799 ({legs} legs planned)");
                code = bytes == 0 && fbytes == 0 ? 0 : 1;
                return;
            }
            case "follow":
            {
                // BotNavTests.Follower_FollowsAPlannedPath_ToItsEnd: duel spawn 0 to duel spawn 1's node.
                GameConfig cfg = Config();
                NavGraph graph = SpireGraph(cfg, out _);
                var (_, world, _) = SpireWorld(cfg);
                MovementConfig m = cfg.movement;
                double dt = 1 / m.loop.simHz;
                var spawns = PortData.Arr(PortData.Obj(SpireJson()["spawns"])["duelSpawns"]);
                Vec3 s0 = PortData.V3(spawns[0]), s1 = PortData.V3(spawns[1]);
                var body = new Mover(m, world);
                body.Reset(s0.x, s0.y, s0.z);
                var follower = new Follower(cfg);
                var path = new NavPath();
                bool found = Astar.Path(graph, Astar.NearestNode(graph, body.position), Astar.NearestNode(graph, s1), m.dash.speed, path);
                follower.SetPath(graph, path);
                FollowStatus status = FollowStatus.Moving;
                int steps = 0;
                for (; steps < 120 * 60 && status == FollowStatus.Moving; steps++)
                {
                    var frame = new InputFrame();
                    status = follower.Step(body, ref frame, dt);
                    body.Step(frame, dt);
                }
                Console.WriteLine($"follow: path found {found}, {path.nodes.Count} nodes [{string.Join(",", path.kinds)}], cost {path.cost:0.00} s: {status} after {steps} steps at {body.position}");
                code = status == FollowStatus.Arrived ? 0 : 1;
                return;
            }
            case "bot-session":
            {
                uint seed = args.Length > 1 ? uint.Parse(args[1]) : 1;
                string difficulty = args.Length > 2 ? args[2] : "normal";
                int steps = args.Length > 3 ? int.Parse(args[3]) : 3600;
                GameConfig cfg = Config();
                NavGraph graph = SpireGraph(cfg, out _);
                var (_, world, _) = SpireWorld(cfg);
                MovementConfig m = cfg.movement;
                double dt = 1 / m.loop.simHz;
                var spawns = PortData.Arr(PortData.Obj(SpireJson()["spawns"])["duelSpawns"]);
                Vec3 s0 = PortData.V3(spawns[0]), s1 = PortData.V3(spawns[1]);
                var body = new Mover(m, world);
                body.Reset(s0.x, s0.y, s0.z);
                var health = new Health(100, new EventBus(), "rival");
                var target = new BotTarget();
                var deps = new BotDeps { getPlayer = () => target, ray = world.RayIntersect, getGraph = () => graph, rng = new Mulberry32(seed) };
                var brain = new BotBrain(cfg, new BotSelf { mover = body, health = health, weapon = () => "revolver" }, deps);
                brain.Reset(difficulty);
                var counts = new SortedDictionary<string, int>();
                int fires = 0;
                var t = Stopwatch.StartNew();
                for (int i = 0; i < steps; i++)
                {
                    double a = i * dt * 0.4;
                    target.position = new Vec3(s1.x + 6 * JsMath.Sin(a), 0, s1.z + 6 * JsMath.Cos(a));
                    target.velocity = new Vec3(2.4 * JsMath.Cos(a), 0, -2.4 * JsMath.Sin(a));
                    var frame = new BotFrame();
                    brain.Step(ref frame, dt);
                    body.Step(frame.move, dt);
                    counts[brain.State] = counts.TryGetValue(brain.State, out int c) ? c + 1 : 1;
                    if (frame.fire.pressed) fires++;
                }
                double d = JsMath.Hypot(body.position.x - target.position.x, body.position.z - target.position.z);
                Console.WriteLine($"bot-session seed {seed} {difficulty}: {steps} steps in {t.Elapsed.TotalMilliseconds:0} ms; states {string.Join(", ", counts)}; fire steps {fires}; end at {body.position}, {d:0.0} m from the target");
                code = 0;
                return;
            }
        }
    }
}
