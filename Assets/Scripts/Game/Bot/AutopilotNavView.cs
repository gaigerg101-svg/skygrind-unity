// NIGHT14 U3-27.5: the AUTOPILOT's view of the bot's nav graph. BotNav's Survival graph is built from the map data alone
// (navGraph.js on the static map, no live layers), so it crosses the locked zones' force fields: a path through a locked
// zone walks the bot into a wall. This view drops every node in a locked zone (no edge into or out of it; the node indices
// stay, so the graph's nearestNode and A* answers keep their meaning) and is rebuilt when the zones change (Zones.Version:
// a gate bought, a new run), never per step. The view, its lists, the lifted nodes and the node -> zone table are made once
// per graph and refilled in place (no allocation after the first). No UnityEngine use.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class AutopilotNavView
    {
        readonly SurvivalSim sim;
        readonly Func<SurvivalSim, NavGraph> source;
        NavGraph full, view;
        int[] zoneOf;
        NavNode[] lifted;
        int version = -1;

        /// <summary>The view as deps.getGraph (made once).</summary>
        public readonly Func<NavGraph> Get;

        /// <summary>The view as the target adapter's graph source (made once).</summary>
        public readonly Func<SurvivalSim, NavGraph> Source;

        /// <summary>Views built so far (each a gate bought or a new run's reset).</summary>
        public int builds;

        public AutopilotNavView(SurvivalSim sim, Func<SurvivalSim, NavGraph> source)
        {
            this.sim = sim;
            this.source = source;
            Get = Current;
            Source = _ => Current();
        }

        /// <summary>The graph without the locked zones' nodes (null: no graph).</summary>
        public NavGraph Current()
        {
            NavGraph g = source != null ? source(sim) : null;
            if (g == null) return null;
            if (!ReferenceEquals(g, full))
            {
                // A new graph: the view, its lists (each sized for every edge it could hold) and the lifted nodes, made once.
                full = g;
                int count = g.nodes.Count;
                zoneOf = new int[count];
                lifted = new NavNode[count];
                view = new NavGraph();
                for (int i = 0; i < count; i++)
                {
                    NavNode n = g.nodes[i];
                    zoneOf[i] = ZoneMath.ZoneIndexAt(sim.map.zoneParts, n.x, n.y + 1, n.z);
                    lifted[i] = new NavNode { x = n.x, y = double.PositiveInfinity, z = n.z, kind = n.kind, group = n.group, pad = n.pad };
                    view.nodes.Add(n);
                    view.edges.Add(new List<NavEdge>(g.edges[i].Count));
                }
                version = -1;
            }
            if (version != sim.zones.Version)
            {
                version = sim.zones.Version;
                Mask();
                builds++;
            }
            return view;
        }

        bool Open(int i) => zoneOf[i] < 0 || !sim.zones.IsLocked(zoneOf[i]);

        /// <summary>Refill the view in place (no allocation: the lists keep their capacity): the open nodes as they are; a locked
        /// one lifted to +infinity (astar.js nearestNode never picks it, so a plan never starts or ends behind a force field)
        /// with no edges.</summary>
        void Mask()
        {
            for (int i = 0; i < full.nodes.Count; i++)
            {
                bool open = Open(i);
                view.nodes[i] = open ? full.nodes[i] : lifted[i];
                List<NavEdge> list = view.edges[i];
                list.Clear();
                if (!open) continue;
                List<NavEdge> from = full.edges[i];
                for (int k = 0; k < from.Count; k++)
                    if (Open(from[k].to)) list.Add(from[k]);
            }
        }
    }
}
