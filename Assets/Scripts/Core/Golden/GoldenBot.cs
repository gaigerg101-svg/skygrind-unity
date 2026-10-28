// NIGHT14 U3-27.4 (agent N): the bot's oracle checks (src/nav and src/bot), the p22 sets (port-export-p22 README
// section H): astar.json (astar / nearestNode on fixed graphs), reach.json (jumpReach / dropReach), navGraph.json
// (buildNavGraph on the Spire with the duel's core and grid, and on the Survival map: flat core, no live layers),
// botBrain.json (six bot sessions on the Spire: every step's frame, look, state, visibility, rng draws and the bot's
// position) and follower.json (twelve paths followed on the Spire). The bot and follower sessions run on the graph the
// file navGraph.json records (the navGraph set checks the builder itself) and the Spire's world: the map Octree, the
// core at the duel preset, arena.checkPads after each Mover step. CompareGraph holds a built NavGraph against a
// recorded buildNavGraph result (also maps/spire.json duelNavGraph) node by node and edge by edge, bit for bit.
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static partial class Golden
    {
        static partial void AddBot(Dictionary<string, GoldenCheck> d)
        {
            d["astar"] = GoldenBot.CheckAstar;
            d["reach"] = GoldenBot.CheckReach;
            d["navGraph"] = GoldenBot.CheckNavGraph;
            d["botBrain"] = GoldenBot.CheckBotBrain;
            d["follower"] = GoldenBot.CheckFollower;
        }
    }

    public static class GoldenBot
    {
        static int I(object o) => (int)PortData.Num(o);

        static Vec3 V(object o)
        {
            if (o is Dictionary<string, object>) return PortData.V3(o);
            var a = PortData.Arr(o);
            return new Vec3(PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2]));
        }

        /// <summary>A recorded graph ({nodes: [{x, y, z, kind?, group?, box?, pad?}], edges: [[{to, cost, kind, back?}]]}).</summary>
        public static NavGraph GraphFromJson(Dictionary<string, object> g)
        {
            var graph = new NavGraph();
            foreach (object o in PortData.Arr(g["nodes"]))
            {
                var d = PortData.Obj(o);
                graph.nodes.Add(new NavNode
                {
                    x = PortData.Num(d["x"]), y = PortData.Num(d["y"]), z = PortData.Num(d["z"]),
                    kind = d.TryGetValue("kind", out object k) ? (string)k : null,
                    group = d.TryGetValue("group", out object gr) ? (string)gr : null,
                    box = d.TryGetValue("box", out object b) ? I(b) : -1,
                    pad = d.TryGetValue("pad", out object p) && !(p is Dictionary<string, object>) ? I(p) : -1,
                });
            }
            foreach (object o in PortData.Arr(g["edges"]))
            {
                var list = new List<NavEdge>();
                foreach (object e in PortData.Arr(o))
                {
                    var d = PortData.Obj(e);
                    list.Add(new NavEdge { to = I(d["to"]), cost = PortData.Num(d["cost"]), kind = (string)d["kind"], back = d.TryGetValue("back", out object bk) ? PortData.Num(bk) : 0 });
                }
                graph.edges.Add(list);
            }
            return graph;
        }

        static void SamePath(Dictionary<string, object> want, NavPath got, GoldenResult r, string at)
        {
            var nodes = PortData.Arr(want["nodes"]);
            var kinds = PortData.Arr(want["kinds"]);
            if (nodes.Count != got.nodes.Count || kinds.Count != got.kinds.Count)
            {
                r.Fail($"{at}: expected {nodes.Count} nodes / {kinds.Count} kinds got {got.nodes.Count} / {got.kinds.Count} ([{string.Join(",", got.nodes)}])");
                return;
            }
            for (int i = 0; i < nodes.Count; i++) r.Same(nodes[i], got.nodes[i], $"{at}.nodes[{i}]");
            for (int i = 0; i < kinds.Count; i++) r.SameStr(kinds[i], got.kinds[i], $"{at}.kinds[{i}]");
            r.Same(want["cost"], got.cost, at + ".cost");
        }

        // --- astar.json ----------------------------------------------------------------------------------------------

        public static void CheckAstar(GoldenData data, GoldenResult r)
        {
            var v = data.Vectors("astar");
            var graphs = new List<NavGraph>();
            var names = new List<string>();
            foreach (object o in PortData.Arr(v["graphs"]))
            {
                graphs.Add(GraphFromJson(PortData.Obj(o)));
                names.Add((string)PortData.Obj(o)["name"]);
            }
            var path = new NavPath();
            foreach (object o in PortData.Arr(v["records"]))
            {
                var d = PortData.Obj(o);
                int gi = I(d["graph"]), start = I(d["start"]), goal = I(d["goal"]);
                double speed = PortData.Num(d["speed"]);
                r.cases++;
                r.where = $"astar {names[gi]} {start}->{goal} at {GoldenResult.Fmt(speed)}";
                bool found = Astar.Path(graphs[gi], start, goal, speed, path);
                if (d["out"] == null)
                {
                    if (found) r.Fail($"expected null got [{string.Join(",", path.nodes)}]");
                }
                else if (!found) r.Fail("expected a path got null");
                else SamePath(PortData.Obj(d["out"]), path, r, "out");
            }
            foreach (object o in PortData.Arr(v["nearest"]))
            {
                var d = PortData.Obj(o);
                int gi = I(d["graph"]);
                var p = PortData.Arr(d["p"]);
                r.cases++;
                r.where = $"nearestNode {names[gi]} ({Golden.Describe(d["p"])})";
                r.Same(d["out"], Astar.NearestNode(graphs[gi], new Vec3(PortData.Num(p[0]), PortData.Num(p[1]), PortData.Num(p[2]))), "out");
            }
            r.where = "";
        }

        // --- reach.json ----------------------------------------------------------------------------------------------

        public static void CheckReach(GoldenData data, GoldenResult r)
        {
            MovementConfig m = data.Movement;
            foreach (object o in PortData.Arr(data.Vectors("reach")["records"]))
            {
                var d = PortData.Obj(o);
                r.cases++;
                if ((string)d["fn"] == "jumpReach")
                {
                    r.where = $"jumpReach({d["kind"]}, {Golden.Describe(d["dy"])})";
                    r.Same(d["out"], Reach.JumpReach((string)d["kind"], PortData.Num(d["dy"]), m), "out");
                }
                else
                {
                    r.where = $"dropReach({Golden.Describe(d["drop"])})";
                    r.Same(d["out"], Reach.DropReach(PortData.Num(d["drop"]), m), "out");
                }
            }
            r.where = "";
        }

        // --- navGraph.json -------------------------------------------------------------------------------------------

        /// <summary>A map's world as the recorder built it: the map's static Octree and its core at `preset`.</summary>
        static (NavMap map, GameWorld world, CoreGrid core) World(GoldenData data, string mapName, string preset)
        {
            var json = PortData.Obj(data.Json($"maps/{mapName}.json"));
            NavMap map = NavMap.FromExport(json);
            CoreGrid core = null;
            if (map.core.HasValue)
            {
                MovementConfig m = data.Movement;
                core = new CoreGrid(m, map.core.Value);
                core.SetHeights(CoreGrid.PresetHeights(preset, map.core.Value.cells, m.core.maxHeight, m.core.heightStep, new Mulberry32(1)));
            }
            return (map, new GameWorld(PortData.BuildMapOctree(json), core), core);
        }

        public static void CheckNavGraph(GoldenData data, GoldenResult r)
        {
            foreach (object o in PortData.Arr(data.Vectors("navGraph")["graphs"]))
            {
                var g = PortData.Obj(o);
                string mapName = (string)g["map"];
                string preset = (string)g["corePreset"];
                r.where = $"navGraph {mapName}";
                var (map, world, core) = World(data, mapName, preset);
                // The grid navWorker.js / main.js hand it: the Spire's duel grid (core 'towers'), Survival's own (flat).
                var gridJson = mapName == "spire"
                    ? PortData.Obj(data.Json("maps/spire-duel-navgrid.json"))
                    : PortData.Obj(PortData.Obj(data.Json($"maps/{mapName}.json"))["navGrid"]);
                NavGrid grid = NavGrid.FromExport(gridJson);
                r.Same(g["gridCount"], grid.count, "gridCount");
                NavGraph graph = NavGraph.Build(map, world, data.Config, core, grid);
                CompareGraph(g, graph, map, r);
            }
            r.where = "";
        }

        // --- botBrain.json and follower.json: sessions on the Spire ----------------------------------------------------

        /// <summary>The draws a Mulberry32 made since `seed` (its state is seed + draws * 0x6D2B79F5, mod 2^32).</summary>
        static long Draws(Mulberry32 rng, uint seed)
        {
            unchecked
            {
                const uint a = 0x6D2B79F5;
                uint inv = a; // Newton's iteration for the inverse of an odd number mod 2^32
                for (int i = 0; i < 5; i++) inv *= 2 - a * inv;
                return (uint)((rng.State - seed) * inv);
            }
        }

        static ArenaMap Pads(GoldenData data)
        {
            var arena = new ArenaMap();
            foreach (object o in PortData.Arr(PortData.Obj(data.Json("maps/spire.json"))["pads"]))
            {
                var d = PortData.Obj(o);
                var pad = new PadSpec { pos = new Vec3(PortData.Num(d["x"]), PortData.Num(d["y"]), PortData.Num(d["z"])) };
                if (d.TryGetValue("target", out object target) && target != null)
                {
                    pad.aimed = true;
                    pad.target = PortData.V3(target);
                    pad.apexAbove = PortData.Num(d["apexAbove"]);
                }
                arena.pads.Add(pad);
            }
            return arena;
        }

        static bool SameButton(object want, Button got, string what, GoldenResult r)
        {
            var w = PortData.Obj(want);
            bool a = r.SameBool(w["pressed"], got.pressed, what + ".pressed");
            bool b = r.SameBool(w["held"], got.held, what + ".held");
            return a && b;
        }

        /// <summary>The recorded frame {moveX, moveZ, yaw, pitch, jump, dash, slide, fire, weapon}.</summary>
        static bool SameFrame(object want, in InputFrame move, Button fire, int weapon, GoldenResult r)
        {
            int before = r.mismatches.Count;
            var w = PortData.Obj(want);
            r.Same(w["moveX"], move.moveX, "frame.moveX");
            r.Same(w["moveZ"], move.moveZ, "frame.moveZ");
            r.Same(w["yaw"], move.yaw, "frame.yaw");
            r.Same(w["pitch"], move.pitch, "frame.pitch");
            SameButton(w["jump"], move.jump, "frame.jump", r);
            SameButton(w["dash"], move.dash, "frame.dash", r);
            SameButton(w["slide"], move.slide, "frame.slide", r);
            SameButton(w["fire"], fire, "frame.fire", r);
            r.Same(w["weapon"], weapon, "frame.weapon");
            return r.mismatches.Count == before;
        }

        static bool SameBody(Dictionary<string, object> rec, Mover mover, GoldenResult r)
        {
            int before = r.mismatches.Count;
            var pos = PortData.Arr(rec["pos"]);
            r.Same(pos[0], mover.position.x, "pos.x");
            r.Same(pos[1], mover.position.y, "pos.y");
            r.Same(pos[2], mover.position.z, "pos.z");
            r.SameStr(rec["mstate"], PortData.Name(mover.state), "mstate");
            return r.mismatches.Count == before;
        }

        public static void CheckBotBrain(GoldenData data, GoldenResult r)
        {
            GameConfig cfg = data.Config;
            MovementConfig m = cfg.movement;
            double dt = 1 / m.loop.simHz;
            NavGraph graph = GraphFromJson(PortData.Obj(PortData.Arr(data.Vectors("navGraph")["graphs"])[0]));
            var (_, world, _) = World(data, "spire", cfg.Str("core.duelPreset"));
            ArenaMap pads = Pads(data);
            foreach (object so in PortData.Arr(data.Vectors("botBrain")["sessions"]))
            {
                var s = PortData.Obj(so);
                string name = (string)s["session"];
                var spawn = PortData.Obj(s["spawn"]);
                var mover = new Mover(m, world);
                mover.Reset(PortData.Num(spawn["x"]), PortData.Num(spawn["y"]), PortData.Num(spawn["z"]));
                var health = new Health(100, new EventBus(), "rival");
                health.hp = PortData.Num(PortData.Obj(s["health0"])["hp"]);
                var t = PortData.Obj(s["target"]);
                var target = new BotTarget { dead = (bool)PortData.Obj(t["health"])["dead"], crouched = (bool)t["crouched"] };
                uint seed = (uint)PortData.Num(s["seed"]);
                var rng = new Mulberry32(seed);
                var deps = new BotDeps { getPlayer = () => target, ray = world.RayIntersect, getGraph = () => graph, rng = rng };
                foreach (object oo in PortData.Arr(s["orbs"]))
                {
                    var od = PortData.Obj(oo);
                    deps.orbs.Add(new BotOrb { position = V(od["position"]), active = (bool)od["active"] });
                }
                string weapon = (string)PortData.Obj(s["loadout"])["weapon"];
                var brain = new BotBrain(cfg, new BotSelf { mover = mover, health = health, weapon = () => weapon }, deps);
                brain.look.yaw = PortData.Num(s["lookYaw0"]); // rival.spawn: look.yaw = yawDeg * PI / 180, pitch 0, reset
                brain.look.pitch = 0;
                brain.Reset((string)s["difficulty"]);
                foreach (object ro in PortData.Arr(s["records"]))
                {
                    var rec = PortData.Obj(ro);
                    r.cases++;
                    r.where = $"botBrain {name} step {PortData.Num(rec["i"])}";
                    int before = r.mismatches.Count;
                    var tp = PortData.Obj(rec["target"]);
                    target.position = V(tp["p"]);
                    target.velocity = V(tp["v"]);
                    health.hp = PortData.Num(rec["hp"]);
                    if (rec["playerFire"] is Dictionary<string, object> fire) brain.OnPlayerFire(V(fire["origin"]), V(fire["dir"]));
                    var frame = new BotFrame();
                    brain.Step(ref frame, dt);
                    SameFrame(rec["frame"], frame.move, frame.fire, frame.weapon, r);
                    var look = PortData.Obj(rec["look"]);
                    r.Same(look["yaw"], brain.look.yaw, "look.yaw");
                    r.Same(look["pitch"], brain.look.pitch, "look.pitch");
                    r.SameStr(rec["state"], brain.state, "state");
                    r.SameBool(rec["visible"], brain.visible, "visible");
                    r.Same(rec["draws"], Draws(rng, seed), "draws");
                    mover.Step(frame.move, dt);
                    pads.CheckPads(mover, m);
                    SameBody(rec, mover, r);
                    if (r.mismatches.Count > before) break; // the session's first differing step (the rest follows from it)
                }
            }
            r.where = "";
        }

        static string StatusName(FollowStatus s) => s == FollowStatus.Moving ? "moving" : s == FollowStatus.Arrived ? "arrived" : s == FollowStatus.Stuck ? "stuck" : "idle";

        public static void CheckFollower(GoldenData data, GoldenResult r)
        {
            GameConfig cfg = data.Config;
            MovementConfig m = cfg.movement;
            double dt = 1 / m.loop.simHz;
            NavGraph graph = GraphFromJson(PortData.Obj(PortData.Arr(data.Vectors("navGraph")["graphs"])[0]));
            var (_, world, _) = World(data, "spire", cfg.Str("core.duelPreset"));
            ArenaMap pads = Pads(data);
            foreach (object so in PortData.Arr(data.Vectors("follower")["sessions"]))
            {
                var s = PortData.Obj(so);
                string name = (string)s["session"];
                r.where = $"follower {name}";
                Vec3 at = V(s["startPosition"]);
                var mover = new Mover(m, world);
                mover.Reset(at.x, at.y, at.z);
                var follower = new Follower(cfg);
                var path = new NavPath();
                r.cases++;
                if (!Astar.Path(graph, I(s["start"]), I(s["goal"]), PortData.Num(s["speed"]), path))
                {
                    r.Fail("astar: no path");
                    continue;
                }
                var wp = PortData.Obj(s["path"]);
                SamePath(wp, path, r, "path");
                var backs = PortData.Arr(wp["backs"]);
                for (int i = 0; i < backs.Count && i < path.edges.Count; i++) r.Same(backs[i], path.edges[i].back, $"path.backs[{i}]");
                follower.SetPath(graph, path);
                FollowStatus status = FollowStatus.Moving;
                foreach (object ro in PortData.Arr(s["records"]))
                {
                    var rec = PortData.Obj(ro);
                    r.cases++;
                    r.where = $"follower {name} step {PortData.Num(rec["i"])}";
                    int before = r.mismatches.Count;
                    var frame = new InputFrame();
                    status = follower.Step(mover, ref frame, dt);
                    r.SameStr(rec["status"], StatusName(status), "status");
                    r.SameStr(rec["phase"], follower.Phase, "phase");
                    r.SameStr(rec["edge"], follower.Edge, "edge");
                    r.SameBool(rec["active"], follower.Active, "active");
                    SameFrame(rec["frame"], frame, default, 0, r);
                    mover.Step(frame, dt);
                    pads.CheckPads(mover, m);
                    SameBody(rec, mover, r);
                    if (r.mismatches.Count > before) break;
                }
                r.where = $"follower {name}";
                r.SameStr(s["end"], StatusName(status), "end");
            }
            r.where = "";
        }

        /// <summary>A recorded {nodes, edges, stats?} against `got` (pads compared by position: the web keeps the pad object).</summary>
        public static void CompareGraph(Dictionary<string, object> want, NavGraph got, NavMap map, GoldenResult r)
        {
            var nodes = PortData.Arr(want["nodes"]);
            var edges = PortData.Arr(want["edges"]);
            if (nodes.Count != got.nodes.Count) r.Fail($"nodes: expected {nodes.Count} got {got.nodes.Count}");
            if (edges.Count != got.edges.Count) r.Fail($"edges: expected {edges.Count} lists got {got.edges.Count}");
            int n = System.Math.Min(nodes.Count, got.nodes.Count);
            for (int i = 0; i < n; i++)
            {
                r.cases++;
                var w = PortData.Obj(nodes[i]);
                NavNode g = got.nodes[i];
                string at = $"node {i}";
                r.Same(w["x"], g.x, at + ".x");
                r.Same(w["y"], g.y, at + ".y");
                r.Same(w["z"], g.z, at + ".z");
                r.SameStr(w["kind"], g.kind, at + ".kind");
                r.SameStr(w["group"], g.group, at + ".group");
                if (w.TryGetValue("box", out object box)) r.Same(box, g.box, at + ".box");
                else if (g.box >= 0) r.Fail($"{at}.box: expected none got {g.box}");
                if (w.TryGetValue("pad", out object pad))
                {
                    if (!(pad is Dictionary<string, object>)) r.Same(pad, g.pad, at + ".pad"); // p22 navGraph.json: the index
                    else if (g.pad < 0 || map == null) r.Fail($"{at}.pad: expected a pad got {g.pad}");
                    else
                    {
                        var p = PortData.Obj(pad);
                        PadSpec s = map.pads[g.pad];
                        r.Same(p["x"], s.pos.x, at + ".pad.x");
                        r.Same(p["y"], s.pos.y, at + ".pad.y");
                        r.Same(p["z"], s.pos.z, at + ".pad.z");
                    }
                }
                else if (g.pad >= 0) r.Fail($"{at}.pad: expected none got {g.pad}");
            }
            int m = System.Math.Min(edges.Count, got.edges.Count);
            for (int i = 0; i < m; i++)
            {
                var wl = PortData.Arr(edges[i]);
                List<NavEdge> gl = got.edges[i];
                r.cases++;
                if (wl.Count != gl.Count)
                {
                    r.Fail($"edges[{i}]: expected {wl.Count} got {gl.Count} ({Kinds(wl)} vs {Kinds(gl)})");
                    continue;
                }
                for (int k = 0; k < wl.Count; k++)
                {
                    var w = PortData.Obj(wl[k]);
                    NavEdge e = gl[k];
                    string at = $"edges[{i}][{k}]";
                    r.Same(w["to"], e.to, at + ".to");
                    r.Same(w["cost"], e.cost, at + ".cost");
                    r.SameStr(w["kind"], e.kind, at + ".kind");
                    r.Same(w.TryGetValue("back", out object back) ? back : 0.0, e.back, at + ".back");
                }
            }
            if (want.TryGetValue("stats", out object st) && st is Dictionary<string, object> stats)
            {
                r.Same(stats["flights"], got.statsFlights, "stats.flights");
                if (stats.TryGetValue("gridLinks", out object gridLinks) && gridLinks is Dictionary<string, object> gl2)
                {
                    r.Same(gl2["tried"], got.gridTried, "stats.gridLinks.tried");
                    r.Same(gl2["added"], got.gridAdded, "stats.gridLinks.added");
                }
            }
        }

        static string Kinds(List<object> list)
        {
            var s = new List<string>();
            foreach (object o in list) s.Add((string)PortData.Obj(o)["kind"] + ">" + PortData.Num(PortData.Obj(o)["to"]));
            return string.Join(",", s);
        }

        static string Kinds(List<NavEdge> list)
        {
            var s = new List<string>();
            foreach (NavEdge e in list) s.Add(e.kind + ">" + e.to);
            return string.Join(",", s);
        }
    }
}
