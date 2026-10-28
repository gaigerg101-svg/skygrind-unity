// Port of skygrind/src/nav/follower.js (NIGHT14 U3-27.4), line for line with its comments:
// Path follower (SPEC 2.0 §6 Duel): turns an A* path into InputFrames for the shared Mover. It steers
// toward the next node and performs each edge the way the nav graph flew it (nav/navGraph.js):
// walk, drop, running jump / double (from a short run-up), standing hop / climb (settle on the node,
// jump from still and air-accelerate in), dash jump / dash jump + double, and pads. In the air it
// holds forward and brakes once above the target, like the graph's simulation. Long floor straights
// get slide hops. No progress for a while, or a missed jump, reports 'stuck' so the brain replans.
// Fixed step only; no movement code of its own.
// The port: the step's status is FollowStatus; the debug trace is a ring of reused entries (the web pushes objects and
// arrays per note); config.nav / config.bot are read once at construction. Nothing is allocated per step.
using System;

namespace Skygrind.Core
{
    /// <summary>follower.step's result: 'moving' | 'arrived' | 'stuck' | 'idle'.</summary>
    public enum FollowStatus
    {
        Moving,
        Arrived,
        Stuck,
        Idle,
    }

    public sealed class Follower
    {
        // const RUNNING = new Set(['jump', 'double']); STANDING, DASHING, DOUBLE, AIRBORNE (the three + 'pad')
        public static bool IsRunning(string k) => k == "jump" || k == "double";

        public static bool IsStanding(string k) => k == "hop" || k == "climb";

        public static bool IsDashing(string k) => k == "dash" || k == "dashDouble";

        public static bool IsDouble(string k) => k == "double" || k == "climb" || k == "dashDouble";

        static bool IsAirborne(string k) => IsRunning(k) || IsStanding(k) || IsDashing(k) || k == "pad";

        public const string RunUp = "runup", Run = "run", Takeoff = "takeoff", Air = "air";

        /// <summary>One trace entry (debug): an edge started (`edge` toward `to`), or why a path was dropped (`stuck`).</summary>
        public struct Note
        {
            public string edge; // the edge started, or null
            public string stuck; // 'missed' | 'timeout', or null
            public string kind, phase; // stuck: the edge kind and phase
            public double toX, toY, toZ;
            public string toKind;
            public double atX, atY, atZ; // stuck: where the body was (rounded to cm, as the web)
        }

        // config.nav and the numbers the step reads (read once).
        readonly double arriveRadius, arriveHeight, stuckProgress, stuckTime, stepHeight, runup, settleRadius, settleSpeed;
        readonly double dashJumpDelay, airBrakeSlack;
        readonly double slideStraight; // config.bot.slideStraight
        readonly double walkSpeed, dashNeed;
        readonly double brake; // m/s² of Quake air accel

        NavGraph graph;
        NavPath path; // {nodes, kinds}
        int k; // index of the node we are heading to
        string phase = Run; // runup | run | takeoff | air
        double best = double.PositiveInfinity; // closest horizontal distance to the current node so far
        double sinceProgress; // s
        int dashSteps; // steps since the takeoff dash (dash jumps)
        bool doubleUsed;

        /// <summary>debug: the last edges started and why a path was dropped (a ring of 60, oldest first from TraceStart).</summary>
        public readonly Note[] trace = new Note[60];
        public int traceCount, traceStart;

        void Push(in Note entry)
        {
            if (traceCount < trace.Length) trace[(traceStart + traceCount++) % trace.Length] = entry;
            else
            {
                trace[traceStart] = entry; // trace.shift() + push
                traceStart = (traceStart + 1) % trace.Length;
            }
        }

        public Follower(GameConfig config)
        {
            MovementConfig c = config.movement;
            arriveRadius = config.Num("nav.arriveRadius");
            arriveHeight = config.Num("nav.arriveHeight");
            stuckProgress = config.Num("nav.stuckProgress");
            stuckTime = config.Num("nav.stuckTime");
            stepHeight = config.Num("nav.stepHeight");
            runup = config.Num("nav.runup");
            settleRadius = config.Num("nav.settleRadius");
            settleSpeed = config.Num("nav.settleSpeed");
            dashJumpDelay = config.Num("nav.dashJumpDelay");
            airBrakeSlack = config.Num("nav.airBrakeSlack");
            slideStraight = config.Num("bot.slideStraight");
            walkSpeed = c.ground.walkSpeed;
            dashNeed = c.dash.pipCost + c.dashJump.extraPipCost;
            brake = c.air.accel * c.ground.walkSpeed;
        }

        void Begin(int index)
        {
            k = index;
            phase = IsRunning(path.kinds[k - 1]) ? RunUp : Run;
            best = double.PositiveInfinity;
            sinceProgress = 0;
            dashSteps = 0;
            doubleUsed = false;
        }

        /// <summary>Face and move toward (x, z); ease to a stop within `stopAt` m. Returns the distance.</summary>
        static double Steer(ref InputFrame frame, Vec3 p, double x, double z, double stopAt = 0)
        {
            double dx = x - p.x;
            double dz = z - p.z;
            double d = JsMath.Hypot(dx, dz);
            frame.yaw = JsMath.Atan2(-dx, -dz); // yaw 0 looks toward -z
            frame.moveX = 0;
            frame.moveZ = stopAt > 0 ? (d < stopAt ? 0 : JsMath.Min(1, d / (stopAt * 4))) : 1;
            return d;
        }

        public string Phase => phase;

        /// <summary>The edge being flown, or null.</summary>
        public string Edge => path != null && k > 0 ? path.kinds[k - 1] : null;

        public bool Active => path != null && k < path.nodes.Count;

        /// <summary>The node being approached (debug).</summary>
        public NavNode Target => Active ? graph.nodes[path.nodes[k]] : null;

        /// <summary>The final node, or null.</summary>
        public NavNode Goal => path != null ? graph.nodes[path.nodes[path.nodes.Count - 1]] : null;

        /// <summary>The path being followed (the NavPath given to SetPath), or null.</summary>
        public NavPath Path => path;

        /// <summary>Follow a new path from its first node (the one we are at or next to). `p` is kept (not copied).</summary>
        public void SetPath(NavGraph g, NavPath p)
        {
            graph = g;
            path = p;
            if (p.nodes.Count > 1) Begin(1);
            else k = 0;
        }

        public void Clear() => path = null;

        /// <summary>
        /// Fill the movement part of `frame` (moveX / moveZ / yaw / jump / dash / slide) for this step (buttons are reset by
        /// the caller each step).
        /// </summary>
        public FollowStatus Step(Mover mover, ref InputFrame frame, double dt)
        {
            if (!Active) return FollowStatus.Idle;
            Vec3 p = mover.position;
            NavNode node = graph.nodes[path.nodes[k]];
            NavNode from = k > 0 ? graph.nodes[path.nodes[k - 1]] : node;
            string kind = k > 0 ? path.kinds[k - 1] : "walk";
            double dx = node.x - p.x;
            double dz = node.z - p.z;
            double dist = JsMath.Hypot(dx, dz);
            double dy = node.y - p.y;

            // Arrival: close in plan and height (an airborne edge must also have landed).
            bool landed = mover.grounded || mover.state == MoveState.WallRide;
            if (dist < arriveRadius && Math.Abs(dy) < arriveHeight && (landed || kind == "walk"))
            {
                if (k == path.nodes.Count - 1)
                {
                    path = null;
                    return FollowStatus.Arrived;
                }
                Begin(k + 1);
                NavNode next = graph.nodes[path.nodes[k]];
                Push(new Note { edge = path.kinds[k - 1], toX = next.x, toY = next.y, toZ = next.z, toKind = next.kind });
                return Step(mover, ref frame, dt);
            }
            // Cut corners on walk chains: close to a walk node whose next edge is also a walk.
            if (kind == "walk" && k < path.nodes.Count - 1 && path.kinds[k] == "walk" && dist < arriveRadius * 2 && Math.Abs(dy) < arriveHeight)
            {
                Begin(k + 1);
                return Step(mover, ref frame, dt);
            }

            // Progress watchdog + a missed jump (landed well below where the edge goes).
            if (dist < best - stuckProgress)
            {
                best = dist;
                sinceProgress = 0;
            }
            else
            {
                sinceProgress += dt;
            }
            bool missed = phase == Air && mover.grounded && dy > arriveHeight && IsAirborne(kind);
            if (sinceProgress > stuckTime || missed)
            {
                Push(new Note
                {
                    stuck = missed ? "missed" : "timeout", kind = kind, phase = phase, toX = node.x, toY = node.y, toZ = node.z, toKind = node.kind,
                    atX = JsMath.Round(p.x * 100) / 100, atY = JsMath.Round(p.y * 100) / 100, atZ = JsMath.Round(p.z * 100) / 100,
                });
                path = null;
                return FollowStatus.Stuck;
            }

            Steer(ref frame, p, node.x, node.z);

            if (kind == "pad")
            {
                // Step onto the pad until it fires, let the aimed arc carry us (no steering), then walk
                // from the landing spot to the node.
                if (mover.grounded && phase == Air) return FollowStatus.Moving;
                if (mover.grounded)
                {
                    Steer(ref frame, p, from.x, from.z, 0.3);
                }
                else
                {
                    frame.moveZ = 0;
                    phase = Air;
                }
                return FollowStatus.Moving;
            }
            if (kind == "walk" || (kind == "drop" && mover.grounded))
            {
                // Slide hops on long floor straights.
                bool straight = kind == "walk" && dist > slideStraight && Math.Abs(dy) < stepHeight;
                if (straight && mover.grounded && mover.Speed > walkSpeed * 0.9 && mover.state != MoveState.Slide) frame.slide.pressed = frame.slide.held = true;
                else if (mover.state == MoveState.Slide) frame.jump.pressed = frame.jump.held = true;
                return FollowStatus.Moving;
            }
            if (kind == "drop") phase = Air;

            if (phase == RunUp)
            {
                // Running jumps start from a short run-up behind the node.
                double span = JsMath.Hypot(node.x - from.x, node.z - from.z);
                if (span == 0 || double.IsNaN(span)) span = 1; // || 1
                double rx = from.x - ((node.x - from.x) / span) * runup;
                double rz = from.z - ((node.z - from.z) / span) * runup;
                if (Steer(ref frame, p, rx, rz, 0.3) < 0.4 || !mover.grounded) phase = Run;
                return FollowStatus.Moving;
            }
            if (phase == Run)
            {
                double need = IsDashing(kind) ? dashNeed : 0;
                if (IsRunning(kind) && !mover.grounded && mover.state != MoveState.Dash)
                {
                    phase = Takeoff; // ran off the edge first: coyote jump
                }
                else if (IsRunning(kind))
                {
                    // Take off as we pass the node, at run speed.
                    if (dist <= JsMath.Hypot(node.x - from.x, node.z - from.z) + 0.1) phase = Takeoff;
                }
                else
                {
                    // Standing + dash techniques start still on their takeoff spot (the node, or backed off
                    // from it so the jump has its height before the target's edge), then face the target.
                    double back = k - 1 < path.edges.Count && path.edges[k - 1] != null ? path.edges[k - 1].back : 0; // ?.back ?? 0
                    double span = JsMath.Hypot(node.x - from.x, node.z - from.z);
                    if (span == 0 || double.IsNaN(span)) span = 1; // || 1
                    double settle = Steer(ref frame, p, from.x - ((node.x - from.x) / span) * back, from.z - ((node.z - from.z) / span) * back, settleRadius);
                    // NIGHT17 U3-30.4: BoostReady is the web's `pips >= need`, or on the bar a full boost and its dash-jump (a dash link
                    // was planned for the full-speed, full-length boost: a partial one would fall short of the gap)
                    if (mover.grounded && settle < settleRadius && mover.Speed < settleSpeed && mover.BoostReady(need, true))
                    {
                        Steer(ref frame, p, node.x, node.z);
                        phase = Takeoff;
                    }
                }
                if (phase == Run) return FollowStatus.Moving;
            }
            if (phase == Takeoff)
            {
                if (IsDashing(kind) && dashSteps == 0)
                {
                    frame.dash.pressed = frame.dash.held = true;
                    dashSteps = 1;
                }
                else if (IsDashing(kind) && dashSteps < dashJumpDelay)
                {
                    dashSteps++; // jump a few steps into the dash -> dash jump
                }
                else
                {
                    frame.jump.pressed = frame.jump.held = true;
                    phase = Air;
                }
                return FollowStatus.Moving;
            }

            // In the air: hold jump (apex hang), double jump at the apex, brake once above the target.
            frame.jump.held = kind != "drop";
            if (IsDouble(kind) && !doubleUsed && mover.velocity.y <= 0 && mover.doubleJumpsLeft > 0)
            {
                frame.jump.pressed = true;
                doubleUsed = true;
            }
            double toward = (mover.velocity.x * dx + mover.velocity.z * dz) / JsMath.Max(dist, 1e-6);
            bool above = p.y >= node.y - 0.1; // braking below a ledge would drop us short
            if (above && (dist < 0.4 || (toward > 0 && (toward * toward) / (2 * brake) > dist + airBrakeSlack))) frame.moveZ = -1;
            return FollowStatus.Moving;
        }
    }
}
