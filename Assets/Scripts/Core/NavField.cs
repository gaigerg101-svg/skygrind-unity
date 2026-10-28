// Survival navigation, ported from skygrind/src/ai: navGrid.js (the layered grid's lookups: 1 m columns, walk / jump /
// drop / gap links; the grid itself is the web's own build, read from PortExport/maps/survival.json navGrid),
// flowField.js (the resumable Dijkstra over the reverse links, bestLink, gridPath A*) and navField.js (the facade:
// two fields toward the player, recomputed at ai.flow.every when the player's node changes, on a pop budget; route,
// walkableAt, lineWalkable, relocate, path). Distances and link costs are float32 as on the web (Float32Array).
// Locked zones are masked by tag (no rebuild when a gate opens). The core stays 'flat' in Survival tonight, so the
// exported grid (corePreset 'flat') stays valid; a reshape rebuild is later.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class Link
    {
        public const byte Walk = 0, Jump = 1, Drop = 2, Gap = 3;
        // NIGHT20 U3-34.6 (builder N, design.routes; Core/Nav/RouteLinks.cs): the extra layer's kinds, which exist only with
        // the layer on (the web's grid has 0-3 only): a long drop (every walker), a climb (husks, spitters, swarmers), a tall
        // climb (swarmers only), an authored link (a map's ladder or chain: every climber).
        public const byte LongDrop = 4, Climb = 5, ClimbHigh = 6, Authored = 7;
        /// <summary>NIGHT20 (builder N): a leap up past the web's 2.2 m jump to leapUpMax with no face to climb (a slab's
        /// edge), flown by the web's nav jump (husks, spitters, swarmers).</summary>
        public const byte LeapUp = 8;

        /// <summary>The kinds a field may use, one bit per kind. The web's flowField.js usable(kind, jumps): a jumper every
        /// kind, a walker (ai.jumps.&lt;kind&gt;.jumps false: the CHARGER) walk and drop; the layer's kinds added per verb.</summary>
        public const int MaskWalker = 1 << Walk | 1 << Drop | 1 << LongDrop;
        public const int MaskJumper = 1 << Walk | 1 << Jump | 1 << Drop | 1 << Gap | 1 << LongDrop | 1 << Climb | 1 << Authored | 1 << LeapUp;
        public const int MaskSwarmer = MaskJumper | 1 << ClimbHigh;
        /// <summary>The web's own verbs for a jumper (walk, jump, drop, gap) and the long drop: what reaches a node without a
        /// climb, a ladder or a leap (NavField.Perched).</summary>
        public const int MaskWebJumper = 1 << Walk | 1 << Jump | 1 << Drop | 1 << Gap | 1 << LongDrop;

        public static bool Usable(byte kind, int mask) => ((mask >> kind) & 1) != 0;

        public static int MaskOf(bool jumps) => jumps ? MaskJumper : MaskWalker;
    }

    public sealed class NavGrid
    {
        public int count, size;
        public double origin, cell;
        public int[] col;
        public float[] y;
        public int[] tag;
        public byte[] blocked;
        public int[] colStart;
        public int[] linkStart, linkTo;
        public byte[] linkKind;
        public float[] linkCost;
        public int[] revStart, revFrom, revLink;
        public int version = 1;

        static T[] Arr<T>(object o, Func<object, T> f) => PortData.Arr(o).ConvertAll(x => f(x)).ToArray();

        public static NavGrid FromExport(Dictionary<string, object> g)
        {
            var s = new NavGrid
            {
                count = (int)PortData.Num(g["count"]),
                size = (int)PortData.Num(g["size"]),
                origin = PortData.Num(g["origin"]),
                cell = PortData.Num(g["cell"]),
            };
            var nodes = PortData.Obj(g["nodes"]);
            s.col = Arr(nodes["col"], o => (int)PortData.Num(o));
            s.y = Arr(nodes["y"], o => (float)PortData.Num(o));
            s.tag = Arr(nodes["tag"], o => (int)PortData.Num(o));
            s.blocked = new byte[s.count];
            var links = PortData.Obj(g["links"]);
            s.linkStart = Arr(links["linkStart"], o => (int)PortData.Num(o));
            s.linkTo = Arr(links["linkTo"], o => (int)PortData.Num(o));
            s.linkKind = Arr(links["linkKind"], o => (byte)PortData.Num(o));
            s.linkCost = Arr(links["linkCost"], o => (float)PortData.Num(o));
            // Columns: nodes come grouped by column, in column order.
            s.colStart = new int[s.size * s.size + 1];
            for (int n = 0; n < s.count; n++) s.colStart[s.col[n] + 1]++;
            for (int c = 0; c < s.size * s.size; c++) s.colStart[c + 1] += s.colStart[c];
            // Reverse links (navGrid.js: counted, prefix-summed, filled in link order).
            int m = s.linkTo.Length;
            s.revStart = new int[s.count + 1];
            s.revFrom = new int[m];
            s.revLink = new int[m];
            for (int i = 0; i < m; i++) s.revStart[s.linkTo[i] + 1]++;
            for (int i = 0; i < s.count; i++) s.revStart[i + 1] += s.revStart[i];
            var fill = new int[s.count];
            for (int from = 0; from < s.count; from++)
            {
                for (int i = s.linkStart[from]; i < s.linkStart[from + 1]; i++)
                {
                    int to = s.linkTo[i];
                    int slot = s.revStart[to] + fill[to]++;
                    s.revFrom[slot] = from;
                    s.revLink[slot] = i;
                }
            }
            return s;
        }

        public double ColX(int c) => origin + (c % size + 0.5) * cell;
        public double ColZ(int c) => origin + (c / size + 0.5) * cell;
        public double NodeX(int n) => ColX(col[n]);
        public double NodeZ(int n) => ColZ(col[n]);

        public int ColOf(double x, double z)
        {
            int i = (int)Math.Floor((x - origin) / cell);
            int k = (int)Math.Floor((z - origin) / cell);
            return i >= 0 && i < size && k >= 0 && k < size ? k * size + i : -1;
        }

        /// <summary>The highest open node under a point: its floor within `below` m under y (or `above` over it); -1 if none.</summary>
        public int NodeAt(double x, double yy, double z, double below, double above)
        {
            int c = ColOf(x, z);
            if (c < 0) return -1;
            int best = -1;
            for (int n = colStart[c]; n < colStart[c + 1]; n++)
            {
                if (blocked[n] != 0) continue;
                if (y[n] <= yy + above && y[n] >= yy - below) best = n;
            }
            return best;
        }

        public int NearestNode(double x, double yy, double z, int radius = 2, double heightWeight = 2)
        {
            int ci = (int)Math.Floor((x - origin) / cell);
            int ck = (int)Math.Floor((z - origin) / cell);
            int best = -1;
            double bestD = double.PositiveInfinity;
            for (int k = ck - radius; k <= ck + radius; k++)
            {
                if (k < 0 || k >= size) continue;
                for (int i = ci - radius; i <= ci + radius; i++)
                {
                    if (i < 0 || i >= size) continue;
                    int c = k * size + i;
                    for (int n = colStart[c]; n < colStart[c + 1]; n++)
                    {
                        if (blocked[n] != 0) continue;
                        double dx = ColX(c) - x, dz = ColZ(c) - z, dy = (y[n] - yy) * heightWeight;
                        double d = dx * dx + dy * dy + dz * dz;
                        if (d < bestD)
                        {
                            bestD = d;
                            best = n;
                        }
                    }
                }
            }
            return best;
        }

        /// <summary>NIGHT33 E (design.n33.ai33): the nodes no walker may use whatever the zones say (the lava surfaces: Lava30.NavMask), OR-ed into every
        /// SetBlocked; null (the row off, every older map) = the web's mask by tag alone, bit for bit.</summary>
        public byte[] hazard;

        /// <summary>NIGHT33 E (design.n33.ai33.goalLine; the lead 11:06 "the lava-edge stall"): a goal seed (a node within the seed radius of the player, where a walker
        /// counts as "arrived" and steers straight at him) is dropped when lava lies on the straight line from it to the player, so the field routes walkers round the
        /// lava instead of parking them on the far bank. False (and with hazard null) = the web's seeds.</summary>
        public bool hazardGoalLine;

        /// <summary>NIGHT33 E (design.n33.ai33.goalLine): links whose straight run passes over lava (a drop or a jump from an island or a ledge across the pit: spire v6's
        /// swarmers stood on one and walked off it into the edge guard every step); the fields and BestLink skip them. Null = the web's links.</summary>
        public byte[] hazardLink;

        /// <summary>The links over lava (HazardBetween from each link's start to its end), or null when none is.</summary>
        public byte[] HazardLinks()
        {
            if (hazard == null) return null;
            byte[] m = null;
            for (int a = 0; a < count; a++)
                for (int li = linkStart[a]; li < linkStart[a + 1]; li++)
                {
                    int to = linkTo[li];
                    if (!HazardBetween(col[a], y[a], NodeX(to), y[to], NodeZ(to))) continue;
                    m ??= new byte[linkTo.Length];
                    m[li] = 1;
                }
            return m;
        }

        /// <summary>Does a lava node lie on the straight line from column c (feet at y) to the point (gx, gy, gz)? A lava node counts when it is no more than 1.5 m under
        /// the lower of the two ends (a walkway higher over the lava is not "across lava"). Sampled every half cell.</summary>
        public bool HazardBetween(int c, double y0, double gx, double gy, double gz)
        {
            double x0 = ColX(c), z0 = ColZ(c), dx = gx - x0, dz = gz - z0;
            double len = Math.Sqrt(dx * dx + dz * dz), floor = Math.Min(y0, gy) - 1.5;
            int steps = (int)Math.Ceiling(len / (cell * 0.5));
            int last = -1;
            for (int s = 1; s <= steps; s++)
            {
                double t = (double)s / steps;
                int cc = ColOf(x0 + dx * t, z0 + dz * t);
                if (cc < 0 || cc == last) continue;
                last = cc;
                for (int n = colStart[cc]; n < colStart[cc + 1]; n++) if (hazard[n] != 0 && y[n] >= floor) return true;
            }
            return false;
        }

        /// <summary>Mask nodes by tag (a locked zone): no rebuild.</summary>
        public void SetBlocked(Func<int, bool> fn)
        {
            if (hazard == null)
            {
                for (int i = 0; i < count; i++) blocked[i] = fn(tag[i]) ? (byte)1 : (byte)0;
                return;
            }
            for (int i = 0; i < count; i++) blocked[i] = fn(tag[i]) || hazard[i] != 0 ? (byte)1 : (byte)0;
        }
    }

    /// <summary>flowField.js: one field (dist: the finished distance-to-goal; work: the one being computed).</summary>
    public sealed class Flow : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot (NavField's two fields): the finished field whole, the one being computed
        // while it runs (a finished field's `work` is never read: Start overwrites it), the heap's live part at the
        // capacity Ensure gave it (Continue pushes without growing it), the counters and the last pop.
        public void Save(StateWriter w)
        {
            w.Floats("dist", dist, dist.Length);
            w.Floats("work", work, running ? work.Length : 0);
            w.I("heapCapacity", heapNode.Length);
            w.IntsPacked("heapNode", heapNode, len);
            w.Floats("heapKey", heapKey, len);
            w.I("len", len);
            w.I("pops", pops);
            w.B("jumps", jumps);
            w.B("running", running);
            w.I("popNode", popNode);
            w.D("popKey", popKey);
        }

        public void Load(StateReader r)
        {
            float[] d = r.Floats("dist"), wk = r.Floats("work"), hk = r.Floats("heapKey");
            int[] hn = r.IntsPacked("heapNode");
            int cap = r.Int("heapCapacity"), l = r.Int("len");
            bool run = r.B("running");
            if (l < 0 || hn.Length != l || hk.Length != l || cap < l || (run && wk.Length != d.Length)) throw new StateException("snapshot: a nav field's heap or work");
            if (heapNode.Length != cap)
            {
                heapNode = new int[cap];
                heapKey = new float[cap];
            }
            Array.Copy(hn, heapNode, l);
            Array.Copy(hk, heapKey, l);
            dist = d;
            if (run) work = wk;
            else if (work.Length != d.Length) work = new float[d.Length]; // unread until the next Start fills it
            len = l;
            pops = r.Int("pops");
            jumps = r.B("jumps");
            running = run;
            popNode = r.Int("popNode");
            popKey = (float)r.D("popKey");
        }

        public float[] dist = new float[0], work = new float[0];
        public int[] heapNode = new int[0];
        public float[] heapKey = new float[0];
        public int len, pops;
        public bool jumps = true, running;
        /// <summary>NIGHT20 (builder N): the link kinds this field uses (Link.Mask*); -1: the web's, from `jumps`. Set once per
        /// field (the swarmers' field: Link.MaskSwarmer); not part of the snapshot.</summary>
        public int mask = -1;
        public int popNode;
        public float popKey;

        // NIGHT20 (builder N): the masks (Link.Mask*); for the web's kinds 0-3 the same answer as the web's
        // `jumps || (kind !== jump && kind !== gap)`, and a walker never takes a climb. Public for THE CLIMB (the lead's judge:
        // Usable(kind, true) is the husk's field, the swarmer-only tall climbs left out).
        public static bool Usable(byte kind, bool jumps) => Link.Usable(kind, Link.MaskOf(jumps));

        void Ensure(int n, int m)
        {
            if (dist.Length < n)
            {
                var old = dist;
                dist = new float[n];
                for (int i = 0; i < n; i++) dist[i] = i < old.Length ? old[i] : float.PositiveInfinity;
            }
            if (work.Length < n) work = new float[n];
            int cap = m + n + 16;
            if (heapNode.Length < cap)
            {
                heapNode = new int[cap];
                heapKey = new float[cap];
            }
        }

        /// <summary>
        /// heapPush: the key is the web's double (a seed's cost, dv + cost, g + h), compared unrounded against the stored
        /// float32 keys and rounded only by the store (NIGHT9 U3-07: rounding it first could order equal-looking keys
        /// differently from the web's heap).
        /// </summary>
        int Push(int l, int node, double key)
        {
            int i = l;
            while (i > 0)
            {
                int p = (i - 1) >> 1;
                if (heapKey[p] <= key) break;
                heapNode[i] = heapNode[p];
                heapKey[i] = heapKey[p];
                i = p;
            }
            heapNode[i] = node;
            heapKey[i] = (float)key;
            return l + 1;
        }

        int Pop(int l)
        {
            popNode = heapNode[0];
            popKey = heapKey[0];
            l--;
            if (l > 0)
            {
                int node = heapNode[l];
                float key = heapKey[l];
                int i = 0;
                for (; ; )
                {
                    int lc = 2 * i + 1;
                    if (lc >= l) break;
                    int r = lc + 1;
                    int mm = r < l && heapKey[r] < heapKey[lc] ? r : lc;
                    if (heapKey[mm] >= key) break;
                    heapNode[i] = heapNode[mm];
                    heapKey[i] = heapKey[mm];
                    i = mm;
                }
                heapNode[i] = node;
                heapKey[i] = key;
            }
            return l;
        }

        /// <summary>Start a new field toward the seeds (flat [node, cost, ...]); the finished `dist` keeps answering.</summary>
        public void Start(NavGrid s, List<double> seeds, bool withJumps)
        {
            int n = s.count;
            Ensure(n, s.linkTo.Length);
            for (int i = 0; i < n; i++) work[i] = float.PositiveInfinity;
            int l = 0;
            for (int i = 0; i < seeds.Count; i += 2)
            {
                int node = (int)seeds[i];
                if (node < 0 || node >= n || s.blocked[node] != 0) continue;
                double cost = seeds[i + 1];
                if (cost < work[node])
                {
                    work[node] = (float)cost;
                    l = Push(l, node, cost);
                }
            }
            len = l;
            jumps = withJumps;
            pops = 0;
            running = true;
        }

        /// <summary>Run up to `maxPops` settled nodes; true when done (`dist` then holds it).</summary>
        public bool Continue(NavGrid s, int maxPops = int.MaxValue)
        {
            if (!running) return true;
            int l = len;
            int popped = 0;
            int use = mask >= 0 ? mask : Link.MaskOf(jumps); // NIGHT20: the web's two masks unless the field has its own
            while (l > 0 && popped < maxPops)
            {
                l = Pop(l);
                int v = popNode;
                float dv = popKey;
                if (dv > work[v]) continue;
                popped++;
                for (int r = s.revStart[v]; r < s.revStart[v + 1]; r++)
                {
                    int li = s.revLink[r];
                    if (!Link.Usable(s.linkKind[li], use)) continue;
                    if (s.hazardLink != null && s.hazardLink[li] != 0) continue; // NIGHT33 E: a link over lava (null: the web's links)
                    int u = s.revFrom[r];
                    if (s.blocked[u] != 0) continue;
                    double du = (double)dv + s.linkCost[li]; // JS: a double, compared before the Float32Array store rounds it
                    if (du < work[u])
                    {
                        work[u] = (float)du;
                        l = Push(l, u, du);
                    }
                }
            }
            len = l;
            pops += popped;
            if (l > 0) return false;
            (work, dist) = (dist, work);
            running = false;
            return true;
        }

        /// <summary>The out-link from node a that minimises cost + distance at its end (strictly closer), or -1.</summary>
        public static int BestLink(NavGrid s, float[] dist, int a, bool jumps) => BestLink(s, dist, a, Link.MaskOf(jumps));

        /// <summary>NIGHT20 (builder N): BestLink over a field's own link kinds (Link.Mask*).</summary>
        public static int BestLink(NavGrid s, float[] dist, int a, int mask)
        {
            int best = -1;
            double bestD = double.PositiveInfinity;
            float here = dist[a];
            if (!(here < float.PositiveInfinity)) return -1;
            for (int li = s.linkStart[a]; li < s.linkStart[a + 1]; li++)
            {
                if (!Link.Usable(s.linkKind[li], mask)) continue;
                if (s.hazardLink != null && s.hazardLink[li] != 0) continue; // NIGHT33 E: a link over lava (null: the web's links)
                int to = s.linkTo[li];
                if (s.blocked[to] != 0 || !(dist[to] < here)) continue;
                double d = (double)s.linkCost[li] + dist[to];
                if (d < bestD)
                {
                    bestD = d;
                    best = li;
                }
            }
            return best;
        }

        /// <summary>
        /// GridPath's working memory, kept between searches (U2-02: a repath used to allocate a heap the size of the grid).
        /// g / from / closed are valid for a node only when its stamp is the current search's.
        /// </summary>
        public sealed class PathScratch
        {
            public float[] g = new float[0]; // a Float32Array, as the web's sc.g (NIGHT9 U3-07: it was double here)
            public int[] from = new int[0], via = new int[0], seen = new int[0], closed = new int[0];
            public int stamp;
            public readonly Flow heap = new Flow();
            public readonly List<int> nodes = new List<int>();
            /// <summary>The last path's link index into each node after the first (the web's route.links).</summary>
            public readonly List<int> links = new List<int>();
            /// <summary>The last path's cost (the web's route.cost: sc.g[b], a float32).</summary>
            public float cost;
            /// <summary>Nodes the last search expanded (the web's sc.expanded).</summary>
            public int expanded;
        }

        /// <summary>gridPath's heuristic: the straight-line distance to (bx, by, bz) (nodeXYZ: the column centre and the floor).</summary>
        static double H(NavGrid s, int i, double bx, double by, double bz) => JsMath.Hypot(s.NodeX(i) - bx, s.y[i] - by, s.NodeZ(i) - bz);

        /// <summary>
        /// gridPath: A* from node a to node b on the grid's links (heuristic: straight-line distance; every link costs at
        /// least its length, so it is admissible). Returns the node list [a, ..., b] (scratch.nodes when given; the link
        /// into each node after the first in scratch.links, the cost in scratch.cost), or null when there is no route or
        /// the search passes `maxExpand` nodes.
        /// </summary>
        public static List<int> GridPath(NavGrid s, int a, int b, bool jumps, int maxExpand = 20000, PathScratch scratch = null)
        {
            int n = s.count;
            PathScratch k = scratch ?? new PathScratch();
            if (k.g.Length < n)
            {
                k.g = new float[n];
                k.from = new int[n];
                k.via = new int[n];
                k.seen = new int[n];
                k.closed = new int[n];
            }
            Flow heap = k.heap;
            heap.Ensure(n, s.linkTo.Length);
            int stamp = ++k.stamp;
            double bx = s.NodeX(b), by = s.y[b], bz = s.NodeZ(b);
            k.g[a] = 0;
            k.seen[a] = stamp;
            k.from[a] = -1;
            int l = heap.Push(0, a, H(s, a, bx, by, bz));
            int expanded = 0;
            while (l > 0)
            {
                l = heap.Pop(l);
                int v = heap.popNode;
                if (v == b) break;
                if (k.closed[v] == stamp) continue;
                k.closed[v] = stamp;
                if (++expanded > maxExpand) break;
                double gv = k.g[v];
                for (int li = s.linkStart[v]; li < s.linkStart[v + 1]; li++)
                {
                    if (!Usable(s.linkKind[li], jumps)) continue;
                    if (s.hazardLink != null && s.hazardLink[li] != 0) continue; // NIGHT33 E: a link over lava (null: the web's links)
                    int u = s.linkTo[li];
                    if (s.blocked[u] != 0) continue;
                    double gu = gv + s.linkCost[li];
                    if (k.seen[u] != stamp || gu < k.g[u])
                    {
                        k.seen[u] = stamp;
                        k.g[u] = (float)gu;
                        k.from[u] = v;
                        k.via[u] = li;
                        if (l + 1 >= heap.heapNode.Length) break; // never: a search pushes at most (links + 1) entries
                        l = heap.Push(l, u, gu + H(s, u, bx, by, bz));
                    }
                }
            }
            k.expanded = expanded;
            if (k.seen[b] != stamp) return null;
            List<int> nodes = scratch != null ? k.nodes : new List<int>();
            nodes.Clear();
            k.links.Clear();
            for (int i = b; i != -1; i = k.from[i])
            {
                nodes.Add(i);
                if (i != a) k.links.Add(k.via[i]);
            }
            nodes.Reverse();
            k.links.Reverse();
            k.cost = k.g[b];
            return nodes;
        }
    }

    public struct Route
    {
        public int node;
        public double dist;
        public double tx, ty, tz;
        public int link; // -1, or Link.*
        public double lx, ly, lz;
        /// <summary>NIGHT20 (builder N): the link's index in the grid's arrays (RouteLayer.LinkAt: a climb's face), or -1.</summary>
        public int linkIndex;
        /// <summary>NIGHT20 (builder N): the field has no link onward from here (a local bottom: a seed under you).</summary>
        public bool bottom;
    }

    /// <summary>navField.js: the two fields toward the player, recomputed on a schedule; the queries the brain uses.</summary>
    public sealed class NavField : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot. The fields advance on a pop budget across steps (ai.flow.popsPerStep), so a
        // restore needs the work in progress, not only the finished `dist`: both Flows whole (dist, work, the heap's live
        // part at its capacity, the counters), the goals, the schedule (sinceUpdate, dirty, fieldVersion) and the grid's
        // blocked mask (the locked zones' nodes). The Float32 arrays are packed as their bits (exact).
        public void Save(StateWriter w)
        {
            w.Bytes("blocked", grid.blocked);
            w.I("goal", goal);
            w.I("goalCount", goalCount);
            var gx = new double[goalCount];
            var gy = new double[goalCount];
            var gz = new double[goalCount];
            var gn = new int[goalCount];
            for (int g = 0; g < goalCount; g++)
            {
                gx[g] = goalXs[g];
                gy[g] = goalYs[g];
                gz[g] = goalZs[g];
                gn[g] = goalNodes[g];
            }
            w.Doubles("goalXs", gx);
            w.Doubles("goalYs", gy);
            w.Doubles("goalZs", gz);
            w.Ints("goalNodes", gn);
            w.I("fieldVersion", fieldVersion);
            w.B("dirty", dirty);
            w.D("sinceUpdate", sinceUpdate);
            w.Sub("jumpField", jumpField);
            w.Sub("walkField", walkField);
            if (SwarmOn) w.Sub("swarmField", swarmField); // NIGHT20: only with the route layer's tall climbs on
        }

        public void Load(StateReader r)
        {
            byte[] blocked = r.Bytes("blocked");
            if (blocked.Length != grid.blocked.Length) throw new StateException($"snapshot: nav.blocked has {blocked.Length} nodes, the grid {grid.blocked.Length}");
            int count = r.Int("goalCount");
            List<double> gx = r.Doubles("goalXs"), gy = r.Doubles("goalYs"), gz = r.Doubles("goalZs");
            List<int> gn = r.Ints("goalNodes");
            if (count < 0 || gx.Count != count || gy.Count != count || gz.Count != count || gn.Count != count) throw new StateException("snapshot: nav goals");
            jumpField.Load(r.Section("jumpField"));
            walkField.Load(r.Section("walkField"));
            if (SwarmOn) swarmField.Load(r.Section("swarmField"));
            Array.Copy(blocked, grid.blocked, blocked.Length);
            routes?.ComputeReach(grid); // NIGHT20 (builder N): the layer's reach follows the restored gates
            EnsureGoals(count);
            goalCount = count;
            for (int g = 0; g < count; g++)
            {
                goalXs[g] = gx[g];
                goalYs[g] = gy[g];
                goalZs[g] = gz[g];
                goalNodes[g] = gn[g];
            }
            goal = r.Int("goal");
            fieldVersion = r.Int("fieldVersion");
            dirty = r.B("dirty");
            sinceUpdate = r.D("sinceUpdate");
        }

        public readonly NavGrid grid;
        public readonly Flow jumpField = new Flow();
        public readonly Flow walkField = new Flow();
        /// <summary>NIGHT20 (builder N): the swarmers' field (their tall climbs too), run only with the route layer's CLIMB HIGH
        /// links on (SwarmOn); seeded as the husks' field.</summary>
        public readonly Flow swarmField = new Flow { mask = Link.MaskSwarmer };
        public bool SwarmOn => routes != null && routes.counts[Link.ClimbHigh] > 0;
        readonly List<double> seeds = new List<double>();
        readonly double every, goalBelow, seedRadius, seedHeight, seedBase, lookahead, lookTurn, step, lookBelow, lookAbove;
        readonly int popsPerStep;
        public int goal = -1; // the first goal's node
        // NIGHT9 (Players): the goals, one per living player (COOP-RESEARCH: "the flow field takes seeds; a multi-player
        // goal set is the same cost as one"). One player: one goal, as the web.
        int goalCount;
        double[] goalXs = new double[4], goalYs = new double[4], goalZs = new double[4];
        int[] goalNodes = new int[4];
        Vec3[] want = new Vec3[4]; // this step's goal spots
        int fieldVersion = -1;
        bool dirty = true;
        double sinceUpdate = double.PositiveInfinity;

        public NavField(GameConfig cfg, NavGrid grid)
        {
            this.grid = grid;
            every = cfg.Num("ai.flow.every");
            goalBelow = cfg.Num("ai.flow.goalBelow");
            seedRadius = cfg.Num("ai.flow.seedRadius");
            seedHeight = cfg.Num("ai.flow.seedHeight");
            seedBase = cfg.Num("ai.flow.seedBase");
            lookahead = cfg.Num("ai.flow.lookahead");
            lookTurn = cfg.Num("ai.flow.lookTurn");
            popsPerStep = cfg.Int("ai.flow.popsPerStep");
            step = cfg.Num("ai.grid.step");
            lookBelow = cfg.Num("ai.grid.lookBelow");
            lookAbove = cfg.Num("ai.grid.lookAbove");
        }

        /// <summary>
        /// NIGHT20 U3-34.6 (builder N): design.routes' extra-link layer (Core/Nav/RouteLinks.cs), or null (the web's field).
        /// With it: the grid carries the layer's links, and a goal whose seeds no ground walker reaches (a dead perch: the
        /// 24 m spire tops) is also seeded at the nearest reached floor under it (SeedFallback), per field.
        /// </summary>
        public RouteLayer routes;
        readonly List<double> walkSeeds = new List<double>(); // the walker field's seeds with the layer on

        /// <summary>NIGHT20 (builder N): the layer on (its links swapped in, its reach computed) or off (the web's grid).</summary>
        public void SetRoutes(RouteLayer layer, bool on)
        {
            layer.Apply(grid, on);
            routes = on ? layer : null;
            if (on) layer.ComputeReach(grid);
            dirty = true;
        }

        /// <summary>NIGHT20 (builder N): does a ground husk reach the floor under (x, y, z) over the grid's links (the route
        /// layer's reach; true with the layer off or no floor found)? The brain leaps only at a perch it doesn't.</summary>
        public bool Reached(double x, double y, double z)
        {
            if (routes == null) return true;
            int n = grid.NodeAt(x, y, z, goalBelow, lookAbove);
            return n < 0 || routes.reachJump[n] != 0;
        }

        /// <summary>NIGHT20 (builder N, row 4 R3): a perch: the floor under (x, y, z) is one no ground walker reaches on the
        /// web's own links (a climb, a ladder or a leap is needed, or nothing reaches it). False with the layer off.</summary>
        public bool Perched(double x, double y, double z)
        {
            if (routes == null) return false;
            int n = grid.NodeAt(x, y, z, goalBelow, lookAbove);
            return n >= 0 && routes.reachWeb[n] == 0;
        }

        /// <summary>Locked zones changed (a gate opened, a new run): re-mask by tag and recompute.</summary>
        public void SetBlocked(Func<int, bool> fn)
        {
            grid.SetBlocked(fn);
            routes?.ComputeReach(grid); // NIGHT20: what a ground walker reaches changes with the gates
            dirty = true;
        }

        void Recompute(bool sync)
        {
            seeds.Clear();
            if (routes != null) walkSeeds.Clear();
            for (int g = 0; g < goalCount; g++)
            {
                if (routes == null)
                {
                    SeedGoal(g);
                    continue;
                }
                // NIGHT20: where a ground walker reaches your node (the layer's links), its field is seeded only where it can
                // hit you (your node and the next columns' within 1 m of its height: THE CLIMB's own seeds), so it takes
                // the climb rather than wait in the web's seed ring under you (a hand check: a husk stood under a 9 m
                // perch for 40 s, its route 13 s long); where it doesn't, the web's seeds and the fallback under a dead perch.
                int from = seeds.Count, walkFrom = walkSeeds.Count;
                int gn = grid.NodeAt(goalXs[g], goalYs[g], goalZs[g], goalBelow, lookAbove);
                bool jumpReach = gn >= 0 && routes.reachJump[gn] != 0, walkReach = gn >= 0 && routes.reachWalk[gn] != 0;
                SeedGoal(g); // the web's (it sets goalNodes[g])
                if (jumpReach || walkReach)
                {
                    if (!walkReach) for (int i = from; i < seeds.Count; i++) walkSeeds.Add(seeds[i]);
                    if (jumpReach)
                    {
                        seeds.RemoveRange(from, seeds.Count - from);
                        SeedTight(gn, seeds);
                    }
                    if (walkReach) SeedTight(gn, walkSeeds);
                    else SeedFallback(g, walkSeeds, walkFrom, routes.reachWalk);
                    if (!jumpReach) SeedFallback(g, seeds, from, routes.reachJump);
                    continue;
                }
                for (int i = from; i < seeds.Count; i++) walkSeeds.Add(seeds[i]);
                SeedFallback(g, seeds, from, routes.reachJump);
                SeedFallback(g, walkSeeds, walkFrom, routes.reachWalk);
            }
            goal = goalCount > 0 ? goalNodes[0] : -1;
            jumpField.Start(grid, seeds, true);
            walkField.Start(grid, routes != null ? walkSeeds : seeds, false);
            if (SwarmOn) swarmField.Start(grid, seeds, true);
            dirty = false;
            Advance(sync ? int.MaxValue : popsPerStep);
        }

        /// <summary>One goal's seeds: its node at 0, and the nodes round it (seedRadius columns) at their distance.</summary>
        void SeedGoal(int g)
        {
            double goalX = goalXs[g], goalY = goalYs[g], goalZ = goalZs[g];
            int goal = grid.NodeAt(goalX, goalY, goalZ, goalBelow, lookAbove);
            goalNodes[g] = goal;
            if (goal >= 0)
            {
                seeds.Add(goal);
                seeds.Add(0);
            }
            int r = (int)seedRadius;
            int c0 = grid.ColOf(goalX, goalZ);
            if (c0 >= 0)
            {
                int ci = c0 % grid.size, ck = (c0 - ci) / grid.size;
                for (int k = Math.Max(0, ck - r); k <= Math.Min(grid.size - 1, ck + r); k++)
                {
                    for (int i = Math.Max(0, ci - r); i <= Math.Min(grid.size - 1, ci + r); i++)
                    {
                        int c = k * grid.size + i;
                        for (int n = grid.colStart[c]; n < grid.colStart[c + 1]; n++)
                        {
                            if (n == goal || grid.blocked[n] != 0 || grid.y[n] > goalY + lookAbove) continue;
                            if (grid.hazardGoalLine && grid.hazard != null && grid.HazardBetween(c, grid.y[n], goalX, goalY, goalZ)) continue; // NIGHT33 E: no "arrived" across lava
                            double d = JsMath.Hypot(grid.ColX(c) - goalX, grid.ColZ(c) - goalZ) + (goalY - grid.y[n]) * seedHeight + seedBase;
                            seeds.Add(n);
                            seeds.Add(d);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// NIGHT20 U3-34.6 step 5 (builder N): when none of goal g's seeds (list[from..]) is reached by a ground walker
        /// (`reach`, RouteLayer.ComputeReach), also seed the reached floors under and beside it, within seedFallbackRadius
        /// columns and not above it, at the web's seed cost (hypot + drop x seedHeight + seedBase), those within seedSpread
        /// of the cheapest: husks gather under the perch (spitters lob, climbers start there) and the director's spawns keep
        /// a route (Director.PickNode's Infinity test). Nothing changes where a seed is reached.
        /// </summary>
        void SeedTight(int goal, List<double> list)
        {
            int c0 = grid.col[goal], ci = c0 % grid.size, ck = c0 / grid.size;
            for (int k = Math.Max(0, ck - 1); k <= Math.Min(grid.size - 1, ck + 1); k++)
            {
                for (int i = Math.Max(0, ci - 1); i <= Math.Min(grid.size - 1, ci + 1); i++)
                {
                    int c = k * grid.size + i;
                    for (int n = grid.colStart[c]; n < grid.colStart[c + 1]; n++)
                    {
                        if (grid.blocked[n] != 0 || Math.Abs(grid.y[n] - grid.y[goal]) > 1.0) continue;
                        list.Add(n);
                        list.Add(n == goal ? 0 : JsMath.Hypot(grid.ColX(c) - grid.NodeX(goal), grid.ColZ(c) - grid.NodeZ(goal)));
                    }
                }
            }
        }

        void SeedFallback(int g, List<double> list, int from, byte[] reach)
        {
            for (int i = from; i < list.Count; i += 2) if (reach[(int)list[i]] != 0) return;
            double goalX = goalXs[g], goalY = goalYs[g], goalZ = goalZs[g];
            int c0 = grid.ColOf(goalX, goalZ);
            if (c0 < 0) return;
            int r = (int)routes.p.seedFallbackRadius;
            int ci = c0 % grid.size, ck = (c0 - ci) / grid.size;
            double best = double.PositiveInfinity;
            for (int pass = 0; pass < 2; pass++)
            {
                for (int k = Math.Max(0, ck - r); k <= Math.Min(grid.size - 1, ck + r); k++)
                {
                    for (int i = Math.Max(0, ci - r); i <= Math.Min(grid.size - 1, ci + r); i++)
                    {
                        int c = k * grid.size + i;
                        for (int n = grid.colStart[c]; n < grid.colStart[c + 1]; n++)
                        {
                            if (reach[n] == 0 || grid.blocked[n] != 0 || grid.y[n] > goalY + lookAbove) continue;
                            double d = JsMath.Hypot(grid.ColX(c) - goalX, grid.ColZ(c) - goalZ) + (goalY - grid.y[n]) * seedHeight + seedBase;
                            if (pass == 0) best = Math.Min(best, d);
                            else if (d <= best + routes.p.seedSpread)
                            {
                                list.Add(n);
                                list.Add(d);
                            }
                        }
                    }
                }
                if (!(best < double.PositiveInfinity)) return;
            }
        }

        void Advance(int pops)
        {
            jumpField.Continue(grid, pops);
            walkField.Continue(grid, pops);
            if (SwarmOn) swarmField.Continue(grid, pops);
        }

        /// <summary>One fixed step: at ai.flow.every, restart the fields when the player's node changed; else advance.</summary>
        public void Step(double dt, double px, double py, double pz)
        {
            want[0] = new Vec3(px, py, pz);
            StepGoals(dt, 1);
        }

        /// <summary>
        /// One fixed step toward several players (NIGHT9): the goals are the living ones' feet (nobody alive: the first
        /// one's, as Players.NearestOf); the fields restart when the set or any goal's node changed.
        /// </summary>
        public void Step(double dt, Vec3[] at, bool[] alive, int count)
        {
            EnsureGoals(count);
            int m = 0;
            for (int i = 0; i < count; i++) if (alive[i]) want[m++] = at[i];
            if (m == 0 && count > 0) want[m++] = at[0];
            StepGoals(dt, m);
        }

        void StepGoals(double dt, int m)
        {
            sinceUpdate += dt;
            bool stale = fieldVersion != grid.version;
            bool moved = m != goalCount;
            for (int g = 0; g < m && !moved; g++) moved = grid.NodeAt(want[g].x, want[g].y, want[g].z, goalBelow, lookAbove) != goalNodes[g];
            if (stale || dirty || (moved && sinceUpdate >= every))
            {
                SetGoals(m);
                sinceUpdate = 0;
                Recompute(stale);
                if (stale) fieldVersion = grid.version;
            }
            else if (jumpField.running || walkField.running || (SwarmOn && swarmField.running)) Advance(popsPerStep);
        }

        void EnsureGoals(int count)
        {
            if (want.Length >= count) return;
            Array.Resize(ref want, count);
            Array.Resize(ref goalXs, count);
            Array.Resize(ref goalYs, count);
            Array.Resize(ref goalZs, count);
            Array.Resize(ref goalNodes, count);
        }

        void SetGoals(int m)
        {
            goalCount = m;
            for (int g = 0; g < m; g++)
            {
                goalXs[g] = want[g].x;
                goalYs[g] = want[g].y;
                goalZs[g] = want[g].z;
            }
        }

        /// <summary>Is `node` one of the goals' nodes (the brain: a route that ends on a player)?</summary>
        public bool IsGoal(int node)
        {
            for (int g = 0; g < goalCount; g++) if (goalNodes[g] == node) return true;
            return goalCount == 0 && node == -1;
        }

        public void UpdateNow(double px, double py, double pz)
        {
            want[0] = new Vec3(px, py, pz);
            SetGoals(1);
            Recompute(true);
            fieldVersion = grid.version;
        }

        Flow FieldFor(bool jumps) => jumps ? jumpField : walkField;

        public int BodyNode(double x, double y, double z)
        {
            int n = grid.NodeAt(x, y, z, lookBelow, lookAbove);
            return n >= 0 ? n : grid.NearestNode(x, y, z, 1);
        }

        public double DistanceAt(double x, double y, double z, bool jumps = true)
        {
            int n = BodyNode(x, y, z);
            Flow f = FieldFor(jumps);
            return n < 0 || f.dist.Length <= n ? double.PositiveInfinity : f.dist[n];
        }

        bool Straight(int a, int b)
        {
            double ax = grid.NodeX(a), az = grid.NodeZ(a), bx = grid.NodeX(b), bz = grid.NodeZ(b);
            double len = JsMath.Hypot(bx - ax, bz - az);
            int n = (int)Math.Ceiling(len / (grid.cell * 0.5));
            double yy = grid.y[a];
            for (int i = 1; i < n; i++)
            {
                double t = (double)i / n;
                int m = grid.NodeAt(ax + (bx - ax) * t, yy, az + (bz - az) * t, step, step);
                if (m < 0) return false;
                yy = grid.y[m];
            }
            return true;
        }

        public bool LineWalkable(double ax, double ay, double az, double bx, double bz)
        {
            double yy = ay;
            double len = JsMath.Hypot(bx - ax, bz - az);
            int n = Math.Max(1, (int)Math.Ceiling(len / (grid.cell * 0.5)));
            for (int i = 1; i <= n; i++)
            {
                double t = (double)i / n;
                int m = grid.NodeAt(ax + (bx - ax) * t, yy, az + (bz - az) * t, step + 0.1, step);
                if (m < 0) return false;
                yy = grid.y[m];
            }
            return true;
        }

        public bool WalkableAt(double x, double y, double z)
        {
            int n = grid.NodeAt(x, y, z, 3, 1.5);
            return n >= 0 && jumpField.dist.Length > n && jumpField.dist[n] < float.PositiveInfinity;
        }

        /// <summary>navField.js route: where a ground enemy should go (the farthest straight-line node along the field, or a link).</summary>
        public bool RouteFrom(double x, double y, double z, bool jumps, ref Route output) => RouteFrom(x, y, z, jumps ? FieldJump : FieldWalk, ref output);

        /// <summary>NIGHT20 (builder N): the fields by index: the walkers' (the CHARGER), the jumpers' (husks, spitters),
        /// the swarmers' (FieldSwarm: the jumpers' unless the route layer's tall climbs are on).</summary>
        public const int FieldWalk = 0, FieldJump = 1, FieldSwarm = 2;

        Flow FieldAt(int field) => field == FieldSwarm && SwarmOn ? swarmField : field == FieldWalk ? walkField : jumpField;

        int MaskAt(int field) => field == FieldSwarm && SwarmOn ? Link.MaskSwarmer : Link.MaskOf(field != FieldWalk);

        /// <summary>RouteFrom on field `field` (FieldWalk / FieldJump / FieldSwarm).</summary>
        public bool RouteFrom(double x, double y, double z, int field, ref Route output)
        {
            output.link = -1;
            output.linkIndex = -1;
            output.bottom = false;
            if (fieldVersion != grid.version) return false;
            int node = BodyNode(x, y, z);
            output.node = node;
            if (node < 0)
            {
                output.dist = double.PositiveInfinity;
                return false;
            }
            int mask = MaskAt(field);
            float[] dist = FieldAt(field).dist;
            if (dist.Length <= node)
            {
                output.dist = double.PositiveInfinity;
                return false;
            }
            output.dist = dist[node];
            if (!(dist[node] < float.PositiveInfinity)) return false;
            int cur = node, target = node;
            double travelled = 0, fx = 0, fz = 0;
            for (int i = 0; i < 12; i++)
            {
                int li = Flow.BestLink(grid, dist, cur, mask);
                if (li < 0)
                {
                    output.bottom = i == 0; // NIGHT20: read with the route layer on only
                    break;
                }
                int to = grid.linkTo[li];
                byte kind = grid.linkKind[li];
                double sx = grid.NodeX(to) - grid.NodeX(cur), sz = grid.NodeZ(to) - grid.NodeZ(cur);
                double sl = JsMath.Hypot(sx, sz);
                if (sl == 0) sl = 1;
                if (i == 0)
                {
                    fx = sx / sl;
                    fz = sz / sl;
                }
                else if (kind == Link.Walk && (sx * fx + sz * fz) / sl < lookTurn) break;
                if (kind != Link.Walk)
                {
                    if (cur == node)
                    {
                        output.link = kind;
                        output.linkIndex = li; // NIGHT20: read with the layer on only
                        output.lx = grid.NodeX(to);
                        output.ly = grid.y[to];
                        output.lz = grid.NodeZ(to);
                        target = to;
                    }
                    break;
                }
                travelled += grid.linkCost[li];
                if (travelled > lookahead || !Straight(node, to))
                {
                    if (target == node) target = to;
                    break;
                }
                target = to;
                cur = to;
            }
            output.tx = grid.NodeX(target);
            output.ty = grid.y[target];
            output.tz = grid.NodeZ(target);
            return true;
        }

        readonly Flow.PathScratch pathScratch = new Flow.PathScratch();

        /// <summary>A* from a body to a point: waypoints (x, y, z, the link kind into it), or null.</summary>
        public List<(double x, double y, double z, int link)> Path(double ax, double ay, double az, double bx, double by, double bz, bool jumps = true)
        {
            var output = new List<(double, double, double, int)>();
            return Path(ax, ay, az, bx, by, bz, jumps, output) ? output : null;
        }

        /// <summary>Path into `output` (cleared first); false when there is no path. Allocates nothing once warm.</summary>
        public bool Path(double ax, double ay, double az, double bx, double by, double bz, bool jumps, List<(double x, double y, double z, int link)> output)
        {
            output.Clear();
            int a = BodyNode(ax, ay, az);
            int b = grid.NodeAt(bx, by, bz, 4, 1.5);
            if (b < 0) b = grid.NearestNode(bx, by, bz, 3);
            if (a < 0 || b < 0) return false;
            List<int> nodes = Flow.GridPath(grid, a, b, jumps, 20000, pathScratch);
            if (nodes == null) return false;
            for (int i = 0; i < nodes.Count; i++)
            {
                // navField.js: link: i === 0 ? LINK.walk : s.linkKind[route.links[i - 1]] (the link the search came by).
                int link = i == 0 ? Link.Walk : grid.linkKind[pathScratch.links[i - 1]];
                output.Add((grid.NodeX(nodes[i]), grid.y[nodes[i]], grid.NodeZ(nodes[i]), link));
            }
            return true;
        }

        /// <summary>The nearest reachable node in front of the player (within coneDeg of the facing), at least minDist away.</summary>
        public bool Relocate(double px, double pz, double yaw, double minDist, double coneDeg, bool jumps, out Vec3 spot)
        {
            spot = default;
            float[] dist = FieldFor(jumps).dist;
            double fx = -JsMath.Sin(yaw), fz = -JsMath.Cos(yaw);
            double cos = JsMath.Cos(coneDeg * Math.PI / 180);
            int best = -1;
            double bestD = double.PositiveInfinity;
            for (int n = 0; n < grid.count && n < dist.Length; n++)
            {
                if (grid.blocked[n] != 0 || !(dist[n] < float.PositiveInfinity)) continue;
                double dx = grid.NodeX(n) - px, dz = grid.NodeZ(n) - pz;
                double d = JsMath.Hypot(dx, dz);
                if (d < minDist || d >= bestD || (dx * fx + dz * fz) / d < cos) continue;
                best = n;
                bestD = d;
            }
            if (best < 0) return false;
            spot = new Vec3(grid.NodeX(best), grid.y[best], grid.NodeZ(best));
            return true;
        }
    }
}
