// NIGHT14 U3-27.5: the AUTOPILOT's target adapter. BotBrain's getPlayer() (botBrain.js deps.getPlayer: {position,
// velocity, health.dead, crouched}) returns the chosen enemy of the horde instead of RIVAL's player:
// - the nearest live enemy in line of sight (a GameWorld ray from the bot's eye, bot.eyeHeight, to the enemy's aim point,
//   the web's clearLine with weapons.losSlack), checked at the brain's own cadence (bot.losInterval);
// - else the cheapest by nav-graph cost among the design.autopilot.graphCandidates nearest (A*, at most once per
//   bot.replanInterval), or the nearest by straight distance when there is no graph;
// - the current target is kept unless another is design.autopilot.targetSwitchMeters nearer (one in line of sight beats
//   one out of it), and dropped when it dies.
// The enemy is perceived at its aim point: position.y is lowered by bot.chestHeight so the brain's chest (feet +
// chestHeight) lands on AimPoint, the place a shot counts (a swarmer's is far below a player's chest). The brain sees no
// more than RIVAL does: the target through its own reaction-delayed history and the collider (NIGHT14 rule 6).
// Allocation-free per step (reused BotTarget, fixed candidate arrays, a reused NavPath). No UnityEngine use.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class AutopilotTarget
    {
        readonly SurvivalSim sim;
        readonly AutopilotParams p;
        readonly Func<SurvivalSim, NavGraph> graphOf;
        readonly int losSteps, graphSteps;
        readonly double losSlack, dashSpeed;
        readonly Enemy[] nearK;
        readonly double[] nearKD;
        readonly NavPath path = new NavPath();
        int losLeft, graphLeft;
        Enemy current;

        /// <summary>What getPlayer() returns: one instance, refilled each step.</summary>
        public readonly BotTarget view = new BotTarget();

        /// <summary>deps.getPlayer, made once.</summary>
        public readonly Func<BotTarget> Get;

        /// <summary>The target changed at the last Update (the caller drops the brain's path).</summary>
        public bool switched;

        /// <summary>The target is in line of sight (at the last check).</summary>
        public bool visible;

        public AutopilotTarget(SurvivalSim sim, AutopilotParams p, Func<SurvivalSim, NavGraph> graphOf)
        {
            this.sim = sim;
            this.p = p;
            this.graphOf = graphOf;
            losSteps = Math.Max(1, MovementMath.StepsFor(p.losInterval, sim.step));
            graphSteps = Math.Max(1, MovementMath.StepsFor(p.replanInterval, sim.step));
            losSlack = sim.cfg.Num("weapons.losSlack");
            dashSpeed = sim.cfg.movement.dash.speed;
            int k = Math.Max(1, p.graphCandidates);
            nearK = new Enemy[k];
            nearKD = new double[k];
            Get = () => view;
        }

        public Enemy Current => current;

        public bool Has => current != null && current.alive;

        /// <summary>A new run: no target.</summary>
        public void Reset()
        {
            current = null;
            visible = false;
            switched = false;
            losLeft = 0;
            graphLeft = 0;
        }

        /// <summary>Once per step before the brain: choose (at the line-of-sight cadence, or at once when the target died) and
        /// fill the view. False: no enemy lives.</summary>
        public bool Update()
        {
            switched = false;
            if (current != null && !current.alive)
            {
                current = null;
                losLeft = 0;
            }
            if (--losLeft <= 0)
            {
                losLeft = losSteps;
                Choose();
            }
            if (graphLeft > 0) graphLeft--;
            if (current == null) return false;
            Vec3 aim = current.AimPoint;
            view.position = new Vec3(current.position.x, aim.y - p.chestHeight, current.position.z);
            view.velocity = current.velocity;
            view.dead = !current.alive;
            view.crouched = false;
            return true;
        }

        Vec3 Eye()
        {
            Vec3 f = sim.mover.position;
            return new Vec3(f.x, f.y + p.eyeHeight, f.z);
        }

        /// <summary>botBrain.js clearLine: nothing in the collider short of the point (weapons.losSlack).</summary>
        bool Clear(Vec3 from, Vec3 to, double length)
        {
            if (length <= 0) return true;
            var dir = new Vec3((to.x - from.x) / length, (to.y - from.y) / length, (to.z - from.z) / length);
            return !sim.world.RayIntersect(from, dir, out RayHit hit) || hit.distance >= length - losSlack;
        }

        void Choose()
        {
            Vec3 eye = Eye();
            Enemy vis = null, any = null;
            double visD = double.PositiveInfinity, anyD = double.PositiveInfinity, curD = double.PositiveInfinity;
            bool curVis = false;
            int k = nearK.Length, kn = 0;
            var all = sim.enemies.all;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy e = all[i];
                if (!e.alive) continue;
                Vec3 a = e.AimPoint;
                double d = Vec3.DistanceTo(eye, a);
                if (d < anyD)
                {
                    anyD = d;
                    any = e;
                }
                // the k nearest by straight distance (insertion into a fixed array)
                if (kn < k || d < nearKD[kn - 1])
                {
                    int at = kn < k ? kn++ : k - 1;
                    while (at > 0 && nearKD[at - 1] > d)
                    {
                        nearK[at] = nearK[at - 1];
                        nearKD[at] = nearKD[at - 1];
                        at--;
                    }
                    nearK[at] = e;
                    nearKD[at] = d;
                }
                bool isCur = ReferenceEquals(e, current);
                if (!isCur && d >= visD) continue; // a farther one can't be the nearest in sight
                bool clear = Clear(eye, a, d);
                if (isCur)
                {
                    curD = d;
                    curVis = clear;
                }
                if (clear && d < visD)
                {
                    visD = d;
                    vis = e;
                }
            }
            Enemy cand;
            double candD;
            bool candVis = vis != null;
            if (candVis)
            {
                cand = vis;
                candD = visD;
            }
            else
            {
                cand = any;
                candD = anyD;
                NavGraph graph = graphOf != null ? graphOf(sim) : null;
                if (graph != null && kn > 1 && graphLeft <= 0)
                {
                    graphLeft = graphSteps;
                    if (path.nodes.Capacity < graph.nodes.Count)
                    {
                        // sized once for the longest path the graph can hold (A*'s answers fill it in place)
                        path.nodes.Capacity = graph.nodes.Count;
                        path.kinds.Capacity = graph.nodes.Count;
                        path.edges.Capacity = graph.nodes.Count;
                    }
                    int from = Astar.NearestNode(graph, sim.mover.position);
                    double best = double.PositiveInfinity;
                    for (int i = 0; from >= 0 && i < kn; i++)
                    {
                        int goal = Astar.NearestNode(graph, nearK[i].position);
                        if (goal < 0 || !Astar.Path(graph, from, goal, dashSpeed, path)) continue;
                        if (path.cost < best)
                        {
                            best = path.cost;
                            cand = nearK[i];
                            candD = nearKD[i];
                        }
                    }
                }
                else if (graph != null && current != null && current.alive)
                {
                    cand = current; // between graph costings (bot.replanInterval) the costed choice stands
                    candD = curD;
                }
            }
            for (int i = 0; i < kn; i++) nearK[i] = null; // no reference kept past the choice
            if (current == null || !current.alive)
            {
                switched = cand != null;
                current = cand;
                visible = candVis;
                return;
            }
            if (cand == null || ReferenceEquals(cand, current))
            {
                visible = curVis;
                return;
            }
            bool take = candVis && !curVis || candVis == curVis && candD < curD - p.targetSwitchMeters;
            if (take)
            {
                current = cand;
                visible = candVis;
                switched = true;
            }
            else visible = curVis;
        }
    }
}
