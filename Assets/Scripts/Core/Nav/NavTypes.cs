// NIGHT14 U3-27 (the shared types): the nav graph's data (src/nav/navGraph.js's {nodes, edges}, astar.js's path), placed
// before the port so the Game side builds against it. Agent N owns src/nav's ports (Astar.cs, Follower.cs, Reach.cs,
// NavGraph.cs) and may add fields here; readers outside N use only the fields below.
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>A graph node: {x, y, z, kind, group} (kind: floor | cover | ledge | bridge | island | spire | pillar | pad | link).</summary>
    public sealed class NavNode
    {
        public double x, y, z;
        public string kind;
        public string group;
        /// <summary>A pad node's index into the map's pads (the web keeps the pad object), else -1.</summary>
        public int pad = -1;
        /// <summary>A box top's index into map.boxes (the web's node.box), else -1. NIGHT14 agent N (additive).</summary>
        public int box = -1;
    }

    /// <summary>An out-edge: {to, cost (s), kind, back?} (kind: walk | drop | jump | hop | double | climb | dash | dashDouble | pad).</summary>
    public sealed class NavEdge
    {
        public int to;
        public double cost;
        public string kind;
        /// <summary>A standing technique's back-off (m); 0 when the web's edge has none.</summary>
        public double back;
    }

    /// <summary>buildNavGraph's result: edges[i] are node i's out-edges.</summary>
    public sealed partial class NavGraph
    {
        public readonly List<NavNode> nodes = new List<NavNode>();
        public readonly List<List<NavEdge>> edges = new List<List<NavEdge>>();
    }

    /// <summary>astar()'s result ({nodes, kinds, edges, cost}), filled in place (reused: no allocation per plan).</summary>
    public sealed class NavPath
    {
        public readonly List<int> nodes = new List<int>();
        public readonly List<string> kinds = new List<string>();
        public readonly List<NavEdge> edges = new List<NavEdge>();
        public double cost;
    }
}
