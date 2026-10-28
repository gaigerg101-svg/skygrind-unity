// NIGHT20 U3-34.6 step 5 (builder E owns this file; design.director.n20, GG: "you can kind of just camp out ... there
// always really needs to be a route that enemies can climb up"; Research/2026-10-06-enemy-routes.md 5.4): the director's
// side of "enemies reach every floor", every rule behind the run's director block (off: NIGHT19's director, line for line;
// Director.cs calls in here only with n20.director on).
// - THE PERCH: the player stands on a grid node that no walker from the floor reaches: a breadth-first pass over the
//   grid's links (every kind: walk, jump, drop, gap, and any link the routes layer adds to the grid) from the player's
//   spawn node (the start zone's floor), past no blocked node; refreshed when your node isn't in it (a zone opened),
//   at most every checkEvery s. Exposed as Perched (the HUD may show it).
// - THE CAMP SCORE NOTICES HEIGHT: perched, the web's camp score (directorMath.js stepCamp, unchanged) runs with
//   lingerTime = campHighStart and riseTime = campHighRise, and it counts while you shoot: the ground rule's dealt-damage
//   and kill-chain reliefs are what made a perch free (5.4.2: "use lingerTime 3 s instead of 8 and riseTime 3 s instead
//   of 6").
// - THE HUNTERS CAN ALWAYS COME (5.4.3): perched, the hunter squad spawns first on a high spawn node routed to you (the
//   perch itself), then on the floor.
// - NO DEAD PERCHES, NO SPAWNS INSIDE: a squad with no routed ground node spawns at the floor that fits (the tiers as
//   ever), and no spawn node inside a solid is used (Zones.SampleNodes samples the floor under a box: the SE spire's
//   footprint held 16 "ground" spawn nodes at y 0 that NIGHT19's director telegraphed squads into when you stood on top).
// - THE WEB'S HIGH KINDS (director.js spawnSquad): a squad of director.highKinds (DRONE, LANCER) spawns on a high node with
//   no route check; a mixed squad's high kinds get a high node of their own.
// - THE UNREACHABLE FAILSAFE (5.4.4; CoD BO3's round_spawn_failsafe, KF2, L4D's Tank frustration): a melee walker
//   (recycleKinds) that can't reach you (no field distance, or it walks the floor while you're perched) and is on screen
//   (the director's view cone and line of sight) for recycleAfter s is taken out silently (`enemyRecycle`) and comes back
//   through a telegraph out of sight with its HP kept (Telegraph.hps). Off screen the brain's watchdog already relocates it.
// No allocation per step: the lists and the payload are made once; a recycle allocates its telegraph (rare).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed partial class Director
    {
        /// <summary>Port-only: an unreachable walker was recycled out of sight (payload SpawnEvent: its id, kind and where it stood).</summary>
        public const string RecycleEvent = "enemyRecycle";

        /// <summary>The player stands on a node no walker from the floor reaches (design.director.n20 on; false otherwise).</summary>
        public bool Perched { get; private set; }

        /// <summary>Where the reach pass starts: the map's player spawn (SurvivalSim sets it); null: the reach pass is off.</summary>
        public Vec3? reachFrom;

        bool n20Read, n20Known;
        double campHighStart, campHighRise;
        int recycleSteps, checkSteps;
        string[] recycleKinds = new string[0];
        readonly List<string> highKinds = new List<string>();
        DirectorMath.D dHigh;
        double lookBelow, lookAbove;

        byte[] reached = new byte[0];
        int[] reachQueue = new int[0];
        bool reachValid;
        int reachWait, recycleTick;
        bool spawningHunters;
        readonly Dictionary<long, bool> insideSolid = new Dictionary<long, bool>(1024);
        readonly Dictionary<Husk, int> stuckSteps = new Dictionary<Husk, int>(64);
        readonly List<Husk> recycleNow = new List<Husk>(8);
        readonly SpawnEvent recyclePayload = new SpawnEvent();

        void Read20()
        {
            n20Read = true;
            n20Known = cfg.Has("design.director.n20.campHighStart");
            lookBelow = cfg.Num("ai.grid.lookBelow");
            lookAbove = cfg.Num("ai.grid.lookAbove");
            if (cfg.Has("director.highKinds")) highKinds.AddRange(cfg.Strs("director.highKinds"));
            if (!n20Known) return;
            campHighStart = cfg.Num("design.director.n20.campHighStart");
            campHighRise = cfg.Num("design.director.n20.campHighRise");
            recycleSteps = MovementMath.StepsFor(cfg.Num("design.director.n20.recycleAfter"), cfg.Dt);
            checkSteps = MovementMath.StepsFor(cfg.Num("design.director.n20.checkEvery"), cfg.Dt);
            recycleKinds = cfg.Strs("design.director.n20.recycleKinds").ToArray();
            if (cfg.Has("design.director.n20.droneFar"))
            {
                droneFar = cfg.Num("design.director.n20.droneFar");
                droneGain = cfg.Num("design.director.n20.droneGain");
                droneClimbStall = cfg.Num("design.director.n20.droneClimbStall");
                droneClimbHold = cfg.Num("design.director.n20.droneClimbHold");
            }
            dHigh = DirectorMath.From(cfg);
            dHigh.lingerTime = campHighStart;
            dHigh.riseTime = campHighRise;
        }

        /// <summary>A new run: no perch, no stuck clocks (Director.NewRun).</summary>
        void ResetN20()
        {
            reachValid = false;
            Perched = false;
            reachWait = recycleTick = 0;
            spawningHunters = false;
            stuckSteps.Clear();
            droneBest.Clear();
        }

        /// <summary>NIGHT20 fix EN2 (the run snapshot, at a break): the NIGHT20 clocks that carry from a round into the next
        /// (the failsafe's check phase, the reach pass's wait, the perch, THE HUNT's ring flag); the reach set is recomputed on
        /// load (the same grid and spawn; the stuck clocks are the dead walkers', none at a break).</summary>
        public void SaveState20(StateWriter w)
        {
            w.I("recycleTick", recycleTick);
            w.I("reachWait", reachWait);
            w.B("reachValid", reachValid);
            w.B("perched", Perched);
            w.B("huntRingOn", huntRingOn);
        }

        public void LoadState20(StateReader r)
        {
            recycleTick = r.Int("recycleTick");
            reachWait = r.Int("reachWait");
            stuckSteps.Clear();
            if (r.B("reachValid")) ComputeReach();
            else reachValid = false;
            Perched = r.B("perched");
            huntRingOn = r.B("huntRingOn");
        }

        bool On20 => n20.director && (n20Read || ReadOnce()) && n20Known;

        bool ReadOnce()
        {
            Read20();
            return true;
        }

        /// <summary>NIGHT20 (the LANCER's perches, main.js getPerches: zones.spawnPoints().high): the open zones' high spawn
        /// nodes, refreshed (the list is the director's own, reused).</summary>
        public List<SpawnNode> HighNodes()
        {
            zones.SpawnPoints(ground, high, core);
            return high;
        }

        bool IsHigh(string kind) => highKinds.Contains(kind);

        bool AllHigh(List<string> kinds)
        {
            if (kinds.Count == 0) return false;
            for (int i = 0; i < kinds.Count; i++) if (!IsHigh(kinds[i])) return false;
            return true;
        }

        bool AnyHigh(List<string> kinds)
        {
            for (int i = 0; i < kinds.Count; i++) if (IsHigh(kinds[i])) return true;
            return false;
        }

        /// <summary>The reach pass: every grid node a walker from the player's spawn node reaches over the grid's links.</summary>
        void ComputeReach()
        {
            NavGrid g = nav.grid;
            reachValid = false;
            if (g == null || !reachFrom.HasValue) return;
            if (reached.Length != g.count)
            {
                reached = new byte[g.count];
                reachQueue = new int[g.count];
            }
            Array.Clear(reached, 0, reached.Length);
            Vec3 s = reachFrom.Value;
            int src = g.NodeAt(s.x, s.y, s.z, lookBelow, lookAbove);
            if (src < 0) src = g.NearestNode(s.x, s.y, s.z);
            if (src < 0) return;
            int head = 0, tail = 0;
            reached[src] = 1;
            reachQueue[tail++] = src;
            while (head < tail)
            {
                int a = reachQueue[head++];
                for (int i = g.linkStart[a]; i < g.linkStart[a + 1]; i++)
                {
                    int b = g.linkTo[i];
                    if (reached[b] != 0 || g.blocked[b] != 0) continue;
                    reached[b] = 1;
                    reachQueue[tail++] = b;
                }
            }
            reachValid = true;
        }

        /// <summary>Each step with the block: is your node outside the reach set (refreshed on a miss, at most every checkEvery s)?</summary>
        void UpdatePerch()
        {
            if (reachWait > 0) reachWait--;
            int node = nav.goal;
            NavGrid g = nav.grid;
            if (g == null || node < 0 || node >= g.count)
            {
                Perched = false;
                return;
            }
            if (!reachValid || (reached[node] == 0 && reachWait <= 0))
            {
                ComputeReach();
                reachWait = checkSteps;
            }
            Perched = reachValid && reached[node] == 0;
        }

        /// <summary>The camp score with the block (directorMath.js stepCamp unchanged): perched, dHigh's linger and rise, and no
        /// relief for the damage you deal or a kill chain.</summary>
        (bool started, bool ended, bool hunted) StepCamp20(double dt, Vec3 p, double dealtRecent, bool killChainNow)
        {
            UpdatePerch();
            return Perched
                ? DirectorMath.StepCamp(state, dt, p.x, p.z, husks.Count, 0, false, dHigh)
                : DirectorMath.StepCamp(state, dt, p.x, p.z, husks.Count, dealtRecent, killChainNow, d);
        }

        static long NodeKey(SpawnNode n) =>
            ((long)Math.Round(n.x * 10) & 0xFFFFF) << 40 | ((long)Math.Round(n.z * 10) & 0xFFFFF) << 20 | ((long)Math.Round(n.y * 10) & 0xFFFFF);

        /// <summary>A spawn node under the top of a solid (the sampler went through a box): the first surface over the node,
        /// then a ray down from just under it; an up-facing surface above the node means the node is inside. Cached per node
        /// (the map's static geometry; the core's nodes move and are never inside).</summary>
        bool InsideSolid(SpawnNode n)
        {
            if (n.core) return false;
            long key = NodeKey(n);
            if (insideSolid.TryGetValue(key, out bool known)) return known;
            var from = new Vec3(n.x, n.y + 0.1, n.z);
            double top = geometry.RayIntersect(from, new Vec3(0, 1, 0), out RayHit up) ? from.y + up.distance : from.y + 40;
            var under = new Vec3(n.x, top - 0.01, n.z);
            bool inside = geometry.RayIntersect(under, new Vec3(0, -1, 0), out RayHit down) && under.y - down.distance > n.y + 0.5;
            insideSolid[key] = inside;
            return inside;
        }

        /// <summary>The block's node pick: a high squad on a high node (no route check, director.js); perched hunters on a high
        /// node routed to you; then the ground's routed tiers; then the floor that fits (no dead perches). Never inside a solid.</summary>
        (SpawnNode n, double dist)? PickN20(bool wantBehind, List<string> kinds)
        {
            zones.SpawnPoints(ground, high, core);
            if (AllHigh(kinds)) return PickFrom(high.Count > 0 ? high : ground, wantBehind, false, true);
            if (huntRingOn)
            {
                var ring = PickRing(); // NIGHT20 A2: THE HUNT's squads come in a ring around you (SpecialRounds.cs)
                if (ring.HasValue) return ring;
            }
            if (spawningHunters && Perched)
            {
                var up = PickFrom(high, wantBehind, true, true);
                if (up.HasValue) return up;
            }
            return PickFrom(ground, wantBehind, true, true) ?? PickFrom(ground, wantBehind, false, true);
        }

        void AddTelegraph(SpawnNode n, List<string> kinds, List<double> hps, bool elite = false)
        {
            var at = new Vec3(n.x, n.y, n.z);
            telegraphs.Add(new Telegraph { point = at, kinds = kinds, hps = hps, steps = telegraphSteps, elite = elite });
            events.Emit("spawnTelegraph", new SpawnEvent { kind = kinds[0], position = at });
        }

        /// <summary>NIGHT20 A2 (design.adds.hunt): THE HUNT's ring (SpecialRounds sets it) and whether this round spawns in it.</summary>
        public SpecialRoundMath.HuntP huntRing;
        public bool huntRingOn;

        /// <summary>NIGHT20 A4 (design.adds.elite): the elite's body when its telegraph lands (SpecialRounds' hook).</summary>
        public Action<Husk> n20OnElite;

        /// <summary>THE HUNT's pick: a routed ground node in the ring (ringMin .. ringMax m flat from you), not inside a solid,
        /// one of them at random (the director's stream); none: null (the usual tiers).</summary>
        (SpawnNode n, double dist)? PickRing()
        {
            Vec3 p = playerPos();
            cands.Clear();
            foreach (SpawnNode n in ground)
            {
                double dist = JsMath.Hypot(n.x - p.x, n.z - p.z);
                if (!SpecialRoundMath.InRing(dist, huntRing)) continue;
                if (!(nav.DistanceAt(n.x, n.y, n.z) < double.PositiveInfinity) || !FitsHusk(n) || InsideSolid(n)) continue;
                cands.Add((n, dist, false));
            }
            if (cands.Count == 0) return null;
            var c = cands[(int)Math.Floor(rng.Next() * cands.Count)];
            return (c.n, c.dist);
        }

        /// <summary>NIGHT20 A4: THE ELITE's telegraph (one `kind`, out of view as a squad's, flagged); false: nowhere to spawn.</summary>
        public bool SpawnElite(string kind)
        {
            var kinds = new List<string> { kind };
            bool ring = huntRingOn;
            huntRingOn = false;
            var pick = PickN20(true, kinds);
            huntRingOn = ring;
            if (!pick.HasValue) return false;
            AddTelegraph(pick.Value.n, kinds, null, true);
            return true;
        }

        /// <summary>director.js spawnSquad with the block: the high kinds' own node (a mixed squad's perch kinds), then PickN20.</summary>
        bool SpawnSquad20(List<string> kinds)
        {
            bool wantBehind = !behindDone;
            if (!AllHigh(kinds) && AnyHigh(kinds))
            {
                zones.SpawnPoints(ground, high, core);
                var perched = new List<string>();
                var rest = new List<string>();
                foreach (string k in kinds) (IsHigh(k) ? perched : rest).Add(k);
                var perch = PickFrom(high.Count > 0 ? high : ground, wantBehind, false, true);
                if (perch.HasValue) AddTelegraph(perch.Value.n, perched, null);
                kinds = rest;
            }
            var pick = PickN20(wantBehind, kinds);
            if (!pick.HasValue) return false;
            Vec3 p = playerPos();
            if (DirectorMath.Behind(p.x, p.z, playerYaw(), pick.Value.n.x, pick.Value.n.z)) behindDone = true;
            AddTelegraph(pick.Value.n, kinds, null);
            squadsSpawned++;
            return true;
        }

        bool VisibleFromPlayerAt(Vec3 target)
        {
            Vec3 p = playerPos();
            var eye = new Vec3(p.x, p.y + eyeHeight, p.z);
            Vec3 dir = new Vec3(target.x - eye.x, target.y - eye.y, target.z - eye.z);
            double len = dir.Length;
            dir = Vec3.DivideScalar(dir, len > 0 ? len : 1);
            return !geometry.RayIntersect(eye, dir, out RayHit hit) || hit.distance >= len - 0.5;
        }

        /// <summary>Can't this walker reach you: no field distance at it, or it stands on the floor's network while you're perched.</summary>
        bool CantReach(Husk h)
        {
            if (!(nav.DistanceAt(h.position.x, h.position.y, h.position.z) < double.PositiveInfinity)) return true;
            if (!Perched || !reachValid) return false;
            int node = nav.grid.NodeAt(h.position.x, h.position.y, h.position.z, lookBelow, lookAbove);
            return node >= 0 && reached[node] != 0;
        }

        /// <summary>The unreachable failsafe, every checkEvery s: the on-screen clock of each stuck melee walker; at recycleAfter it
        /// goes back through a telegraph out of sight with its HP.</summary>
        void Recycle20()
        {
            if (++recycleTick < checkSteps) return;
            recycleTick = 0;
            Vec3 p = playerPos();
            double yaw = playerYaw();
            for (int k = 0; k < recycleKinds.Length; k++)
            {
                List<Husk> list = husks.PoolOf(recycleKinds[k]);
                for (int i = 0; i < list.Count; i++)
                {
                    Husk h = list[i];
                    if (!h.alive || !CantReach(h))
                    {
                        stuckSteps.Remove(h);
                        continue;
                    }
                    if (DirectorMath.OutOfView(p.x, p.z, yaw, h.position.x, h.position.z, viewHalfAngle, VisibleFromPlayerAt(h.AimPoint))) continue;
                    int s = (stuckSteps.TryGetValue(h, out int v) ? v : 0) + checkSteps;
                    if (s >= recycleSteps)
                    {
                        stuckSteps.Remove(h);
                        recycleNow.Add(h);
                    }
                    else stuckSteps[h] = s;
                }
            }
            // NIGHT20 (THE FARM's drone stall): a DRONE further than droneFar that hasn't closed droneGain m for recycleAfter s,
            // on screen or off (a flier has no watchdog to relocate it), goes back on a high node with its HP
            List<Husk> drones = husks.PoolOf("drone");
            for (int i = 0; i < drones.Count; i++)
            {
                Husk h = drones[i];
                double dist = h.alive ? JsMath.Hypot(h.position.x - p.x, h.position.z - p.z) : 0;
                if (!h.alive || dist <= droneFar)
                {
                    droneBest.Remove(h);
                    stuckSteps.Remove(h);
                    continue;
                }
                if (!droneBest.TryGetValue(h, out double best) || dist < best - droneGain)
                {
                    droneBest[h] = dist;
                    stuckSteps.Remove(h);
                    continue;
                }
                int s = (stuckSteps.TryGetValue(h, out int v) ? v : 0) + checkSteps;
                if (s >= recycleSteps)
                {
                    stuckSteps.Remove(h);
                    droneBest.Remove(h);
                    recycleNow.Add(h);
                }
                else stuckSteps[h] = s;
            }
            for (int i = 0; i < recycleNow.Count; i++) Recycle(recycleNow[i]);
            recycleNow.Clear();
        }

        readonly Dictionary<Husk, double> droneBest = new Dictionary<Husk, double>(16);
        double droneFar = double.PositiveInfinity, droneGain, droneClimbStall, droneClimbHold;

        /// <summary>NIGHT20: a drone's climb rules at its spawn (Drone.roofCheck / climbStall / climbHold), with the block on;
        /// off: drone.js's climb exactly.</summary>
        void Drone20(Drone dr)
        {
            bool on = On20;
            dr.roofCheck = on;
            dr.climbStall = on ? droneClimbStall : 0;
            dr.climbHold = on ? droneClimbHold : 0;
        }

        void Recycle(Husk h)
        {
            zones.SpawnPoints(ground, high, core);
            var pick = (h is Drone ? PickFrom(high, true, false, true) : null) ?? PickFrom(ground, true, true, true) ?? PickFrom(ground, true, false, true); // a drone: director.highKinds' high node
            if (!pick.HasValue) return; // nowhere to send it: it stays
            recyclePayload.id = h.id;
            recyclePayload.kind = h.kind;
            recyclePayload.position = h.position;
            double hp = h.hp;
            h.alive = false; // out silently: no kill, no points (as the kill plane takes one)
            events.Emit(RecycleEvent, recyclePayload);
            AddTelegraph(pick.Value.n, new List<string> { h.kind }, new List<double> { hp });
        }
    }
}
