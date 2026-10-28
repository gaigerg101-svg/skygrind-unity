// NIGHT14 U3-27.4 (agent N): the bot's nav graph on the Survival map. navGraph.js builds there with no change (agent R
// recorded it: p22 navGraph.json, 848 nodes / 9652 edges, equal to this build bit for bit), the way the web builds the
// duel's: the map's static Octree, the core at 'flat' (Survival's core), the run's nav grid, and no live layers (the
// locked zones' force fields are not in it, as the nav grid leaves them out: the graph crosses locked zones; Masked
// gives a view without the nodes a caller rules out). The build takes seconds (3.0 s CoreCLR, 3.1 to 7.7 s in Node), so it
// is built once per content hash (the map file, the config sections it reads, this port's version) and kept in memory
// for the process (the farm's runs share it) and, when CacheDir is set, on disk (a binary file of the graph, exact
// doubles). It is built from the map data alone (its own Octree and core), never from the sim's live world, so the result
// is a pure function of the data. Free of UnityEngine (Tools/SimHarness compiles it).
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class BotNav
    {
        /// <summary>The folder the graph cache lives in (null: memory only). The Game sets it (e.g. the persistent data path).</summary>
        public static string CacheDir;

        /// <summary>Where the last Survival() graph came from: "built in N ms", "memory", "disk", or why it is null (debug).</summary>
        public static string LastSource = "";

        /// <summary>Bump when the port (NavGraph.cs, Reach.cs) or the cache format changes: old cache files stop matching.</summary>
        const string Version = "navGraph.js port 1 (NIGHT14), cache format 1";

        static readonly Dictionary<ulong, NavGraph> memory = new Dictionary<ulong, NavGraph>();
        static readonly object gate = new object();

        /// <summary>The config sections buildNavGraph, reach.js and movementMath read.</summary>
        static readonly string[] ConfigSections = { "nav", "body", "jump", "ground", "air", "dash", "dashJump", "loop", "core" };

        /// <summary>The bot's nav graph on `sim`'s map, or null when it can't be built there (LastSource says why).</summary>
        public static NavGraph Survival(SurvivalSim sim) => Survival(sim.cfg, sim.map);

        /// <summary>The graph for a map and config (the same as Survival(sim): it reads only the data).</summary>
        public static NavGraph Survival(GameConfig cfg, SurvivalMap map)
        {
            lock (gate)
            {
                // The same loaded data again (a farm's next run): no hash (it reads the whole map, ~30 ms).
                if (ReferenceEquals(cfg, lastCfg) && ReferenceEquals(map, lastMap) && lastGraph != null)
                {
                    LastSource = "memory";
                    return lastGraph;
                }
            }
            NavGraph graph = Survival(cfg, map, ContentHash(cfg, map));
            lock (gate)
            {
                if (graph != null)
                {
                    lastCfg = cfg;
                    lastMap = map;
                    lastGraph = graph;
                }
            }
            return graph;
        }

        static GameConfig lastCfg;
        static SurvivalMap lastMap;
        static NavGraph lastGraph;

        static NavGraph Survival(GameConfig cfg, SurvivalMap map, ulong hash)
        {
            lock (gate)
            {
                if (memory.TryGetValue(hash, out NavGraph cached))
                {
                    LastSource = "memory";
                    return cached;
                }
                string file = CacheDir == null ? null : Path.Combine(CacheDir, $"botnav-{hash:x16}.bin");
                NavGraph graph = null;
                if (file != null && File.Exists(file))
                {
                    try
                    {
                        graph = Read(file, hash);
                        if (graph != null) LastSource = "disk";
                    }
                    catch (Exception e)
                    {
                        graph = null;
                        LastSource = "disk cache unreadable (" + e.GetType().Name + "), rebuilt";
                    }
                }
                if (graph == null)
                {
                    try
                    {
                        var t = System.Diagnostics.Stopwatch.StartNew();
                        graph = Build(cfg, map);
                        LastSource = $"built in {t.Elapsed.TotalMilliseconds:0} ms";
                    }
                    catch (Exception e)
                    {
                        LastSource = "navGraph.js could not be built on " + map.name + ": " + e.GetType().Name + ": " + e.Message;
                        return null;
                    }
                    if (file != null)
                    {
                        try
                        {
                            Directory.CreateDirectory(CacheDir);
                            Write(file + ".tmp", graph, hash);
                            if (File.Exists(file)) File.Delete(file);
                            File.Move(file + ".tmp", file);
                        }
                        catch (Exception)
                        {
                            // a cache that can't be written is only slower next time
                        }
                    }
                }
                memory[hash] = graph;
                return graph;
            }
        }

        /// <summary>buildNavGraph on the map's own world: its static Octree, the core at 'flat', its nav grid.</summary>
        public static NavGraph Build(GameConfig cfg, SurvivalMap map)
        {
            CoreGrid core = map.CreateCore(cfg.movement);
            var world = new GameWorld(map.BuildOctree(), core);
            return NavGraph.Build(NavMap.From(map), world, cfg, core, NavGrid.FromExport(map.navGrid));
        }

        /// <summary>
        /// A view of `full` without the nodes `open` rules out (no edge into or out of them; the node indices stay). For a
        /// caller that keeps the bot out of locked zones: build it again when the open set changes, not per step.
        /// </summary>
        public static NavGraph Masked(NavGraph full, Func<int, bool> open)
        {
            var view = new NavGraph();
            view.nodes.AddRange(full.nodes);
            for (int i = 0; i < full.edges.Count; i++)
            {
                var list = new List<NavEdge>();
                if (open(i))
                    foreach (NavEdge e in full.edges[i])
                        if (open(e.to)) list.Add(e);
                view.edges.Add(list);
            }
            view.statsFlights = full.statsFlights;
            view.gridTried = full.gridTried;
            view.gridAdded = full.gridAdded;
            return view;
        }

        // --- the content hash (FNV-1a 64 over the parsed data, in its JSON order) -----------------------------------------

        static ulong ContentHash(GameConfig cfg, SurvivalMap map)
        {
            ulong h = 14695981039346656037UL;
            Str(ref h, Version);
            foreach (string s in ConfigSections)
            {
                Str(ref h, s);
                Value(ref h, cfg.root.TryGetValue(s, out object o) ? o : null);
            }
            Value(ref h, map.json);
            return h;
        }

        static void Byte(ref ulong h, byte b)
        {
            h ^= b;
            h *= 1099511628211UL;
        }

        static void Long(ref ulong h, long v)
        {
            for (int i = 0; i < 8; i++) Byte(ref h, (byte)(v >> (8 * i)));
        }

        static void Str(ref ulong h, string s)
        {
            Long(ref h, s.Length);
            foreach (char ch in s)
            {
                Byte(ref h, (byte)ch);
                Byte(ref h, (byte)(ch >> 8));
            }
        }

        static void Value(ref ulong h, object o)
        {
            switch (o)
            {
                case null:
                    Byte(ref h, 0);
                    return;
                case bool b:
                    Byte(ref h, (byte)(b ? 2 : 1));
                    return;
                case string s:
                    Byte(ref h, 3);
                    Str(ref h, s);
                    return;
                case double d:
                    Byte(ref h, 4);
                    Long(ref h, BitConverter.DoubleToInt64Bits(d));
                    return;
                case List<object> list:
                    Byte(ref h, 5);
                    Long(ref h, list.Count);
                    foreach (object e in list) Value(ref h, e);
                    return;
                case Dictionary<string, object> dict:
                    Byte(ref h, 6);
                    Long(ref h, dict.Count);
                    foreach (var kv in dict)
                    {
                        Str(ref h, kv.Key);
                        Value(ref h, kv.Value);
                    }
                    return;
                default:
                    Byte(ref h, 7);
                    Long(ref h, BitConverter.DoubleToInt64Bits(PortData.Num(o)));
                    return;
            }
        }

        // --- the cache file ---------------------------------------------------------------------------------------------

        const string Magic = "SKYGRIND-BOTNAV";

        static void Write(string path, NavGraph g, ulong hash)
        {
            using (var w = new BinaryWriter(File.Create(path)))
            {
                w.Write(Magic);
                w.Write(Version);
                w.Write(hash);
                w.Write(g.nodes.Count);
                foreach (NavNode n in g.nodes)
                {
                    w.Write(n.x);
                    w.Write(n.y);
                    w.Write(n.z);
                    w.Write(n.kind ?? "");
                    w.Write(n.group ?? "");
                    w.Write(n.box);
                    w.Write(n.pad);
                }
                foreach (List<NavEdge> list in g.edges)
                {
                    w.Write(list.Count);
                    foreach (NavEdge e in list)
                    {
                        w.Write(e.to);
                        w.Write(e.cost);
                        w.Write(e.kind);
                        w.Write(e.back);
                    }
                }
                w.Write(g.statsFlights);
                w.Write(g.gridTried);
                w.Write(g.gridAdded);
            }
        }

        static NavGraph Read(string path, ulong hash)
        {
            using (var r = new BinaryReader(File.OpenRead(path)))
            {
                if (r.ReadString() != Magic || r.ReadString() != Version || r.ReadUInt64() != hash) return null;
                var g = new NavGraph();
                int count = r.ReadInt32();
                var kinds = new Dictionary<string, string>(); // one string per kind, as the build's literals
                string K(string s)
                {
                    if (!kinds.TryGetValue(s, out string k)) kinds[s] = k = s;
                    return k;
                }
                for (int i = 0; i < count; i++)
                {
                    g.nodes.Add(new NavNode { x = r.ReadDouble(), y = r.ReadDouble(), z = r.ReadDouble(), kind = K(r.ReadString()), group = r.ReadString(), box = r.ReadInt32(), pad = r.ReadInt32() });
                }
                for (int i = 0; i < count; i++)
                {
                    int n = r.ReadInt32();
                    var list = new List<NavEdge>(n);
                    for (int k = 0; k < n; k++) list.Add(new NavEdge { to = r.ReadInt32(), cost = r.ReadDouble(), kind = K(r.ReadString()), back = r.ReadDouble() });
                    g.edges.Add(list);
                }
                g.statsFlights = r.ReadInt32();
                g.gridTried = r.ReadInt32();
                g.gridAdded = r.ReadInt32();
                return g;
            }
        }
    }
}
