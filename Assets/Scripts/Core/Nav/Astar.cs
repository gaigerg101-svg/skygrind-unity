// Port of skygrind/src/nav/astar.js (NIGHT14 U3-27.4), line for line with its comments:
// A* over the nav graph (SPEC 2.0 §6 Duel). Pure: no three.js, unit tested.
// graph = { nodes: [{x, y, z}], edges: Array<Array<{to: number, cost: number, kind: string}>> }
// The port allocates nothing per search (PORT-RULES 6): the web's per-call arrays (g, from, via, closed, the heap) are
// scratch sized to the graph once per thread, and the path is written into a caller's NavPath. The heap keeps the web's
// exact push / pop order (ties pop in the same order, so the same path is found).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class Astar
    {
        /// <summary>Binary min-heap of [priority, value] (createHeap), on two parallel arrays.</summary>
        sealed class Heap
        {
            double[] pri = new double[64];
            int[] val = new int[64];
            int size;

            public int Size => size;

            public void Clear() => size = 0;

            void Up(int i)
            {
                while (i > 0)
                {
                    int p = (i - 1) >> 1;
                    if (pri[p] <= pri[i]) break;
                    Swap(p, i);
                    i = p;
                }
            }

            void Down(int i)
            {
                for (;;)
                {
                    int l = 2 * i + 1;
                    int r = l + 1;
                    int m = i;
                    if (l < size && pri[l] < pri[m]) m = l;
                    if (r < size && pri[r] < pri[m]) m = r;
                    if (m == i) return;
                    Swap(m, i);
                    i = m;
                }
            }

            void Swap(int a, int b)
            {
                (pri[a], pri[b]) = (pri[b], pri[a]);
                (val[a], val[b]) = (val[b], val[a]);
            }

            public void Push(double priority, int value)
            {
                if (size == pri.Length)
                {
                    Array.Resize(ref pri, size * 2);
                    Array.Resize(ref val, size * 2);
                }
                pri[size] = priority;
                val[size] = value;
                size++;
                Up(size - 1);
            }

            public int Pop()
            {
                int top = val[0];
                size--; // items.pop(): the last
                if (size > 0)
                {
                    pri[0] = pri[size];
                    val[0] = val[size];
                    Down(0);
                }
                return top;
            }
        }

        /// <summary>One search's arrays (the web allocates them per call), grown to the graph's node count.</summary>
        sealed class Scratch
        {
            public double[] g = new double[0];
            public int[] from = new int[0];
            public NavEdge[] via = new NavEdge[0];
            public byte[] closed = new byte[0];
            public readonly Heap heap = new Heap();
            public readonly List<int> nodes = new List<int>();
            public readonly List<NavEdge> edges = new List<NavEdge>();

            public void Size(int n)
            {
                if (g.Length >= n) return;
                g = new double[n];
                from = new int[n];
                via = new NavEdge[n];
                closed = new byte[n];
            }
        }

        [ThreadStatic] static Scratch scratch;

        /// <summary>Straight-line distance between two nodes (an admissible heuristic when costs are ≥ distance / max speed).</summary>
        public static double Distance(NavNode a, NavNode b) => JsMath.Hypot(a.x - b.x, a.y - b.y, a.z - b.z);

        /// <summary>
        /// Shortest path by cost. start / goal: node indices; speed: m/s used to scale the heuristic (costs are seconds).
        /// Writes the path (node indices, the edge kind and edge record into each node after the first) into `into` and
        /// returns true; false (where the web returns null, `into` untouched) when unreachable.
        /// </summary>
        public static bool Path(NavGraph graph, int start, int goal, double speed, NavPath into)
        {
            Scratch s = scratch ??= new Scratch();
            int n = graph.nodes.Count;
            s.Size(n);
            double[] g = s.g;
            int[] from = s.from;
            NavEdge[] via = s.via;
            byte[] closed = s.closed;
            for (int i = 0; i < n; i++)
            {
                g[i] = double.PositiveInfinity;
                from[i] = -1;
                via[i] = null;
                closed[i] = 0;
            }
            Heap heap = s.heap;
            heap.Clear();
            NavNode goalNode = graph.nodes[goal];
            g[start] = 0;
            heap.Push(Distance(graph.nodes[start], goalNode) / speed, start);
            while (heap.Size > 0)
            {
                int i = heap.Pop();
                if (i == goal) break;
                if (closed[i] != 0) continue;
                closed[i] = 1;
                List<NavEdge> list = graph.edges[i];
                for (int k = 0; k < list.Count; k++)
                {
                    NavEdge e = list[k];
                    double cost = g[i] + e.cost;
                    if (cost < g[e.to])
                    {
                        g[e.to] = cost;
                        from[e.to] = i;
                        via[e.to] = e;
                        heap.Push(cost + Distance(graph.nodes[e.to], goalNode) / speed, e.to);
                    }
                }
            }
            if (g[goal] == double.PositiveInfinity) return false;
            List<int> nodes = s.nodes;
            List<NavEdge> edges = s.edges;
            nodes.Clear();
            edges.Clear();
            for (int i = goal; i != -1; i = from[i])
            {
                nodes.Add(i);
                if (i != start) edges.Add(via[i]);
            }
            // edges.reverse(); nodes.reverse(); kinds = edges.map((e) => e.kind)
            into.nodes.Clear();
            into.kinds.Clear();
            into.edges.Clear();
            for (int k = nodes.Count - 1; k >= 0; k--) into.nodes.Add(nodes[k]);
            for (int k = edges.Count - 1; k >= 0; k--)
            {
                into.edges.Add(edges[k]);
                into.kinds.Add(edges[k].kind);
            }
            into.cost = g[goal];
            return true;
        }

        /// <summary>
        /// Nearest node to a point; vertical distance weighs double so a point under a bridge picks the floor, not the
        /// bridge. -1 for an empty graph.
        /// </summary>
        public static int NearestNode(NavGraph graph, Vec3 p)
        {
            int best = -1;
            double bestD = double.PositiveInfinity;
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                NavNode q = graph.nodes[i];
                double d = JsMath.Hypot(q.x - p.x, 2 * (q.y - p.y), q.z - p.z);
                if (d < bestD)
                {
                    best = i;
                    bestD = d;
                }
            }
            return best;
        }
    }
}
