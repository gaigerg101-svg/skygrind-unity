// The Survival brain for the ground enemies (Husks, Swarmers), after skygrind/src/ai/brain.js (think at ai.thinkHz, staggered; a line-of-sight check
// on a budget of ai.losPerStep rays a step, cached ai.losMaxAge s), ai/tokens.js (attack tokens: a husk must hold a
// melee token to wind up; pools ai.tokens.base + roundSteps + campBonus; a better-placed husk steals an uncommitted
// token), ai/watchdog.js (the stuck ladder: hop, side-step, re-path; relocation after ai.watchdog.relocateAt s while
// off-screen), ai/crowd.js (the hard body-to-body separation after every move) and enemies/navJump.js (a jump / gap
// link or the watchdog's hop: a crouch, then a ballistic arc).
// Ported exactly: tokens, the watchdog, crowd separation, nav jumps, route / direct / path navigation.
// Simplified tonight (logged in PORT-STATUS.md): Yuka's Arrive + Separation + OffsetPursuit + Wander steering becomes
// Arrive (speed = distance / ai.steer.arriveDecel, capped) blended at ai.steer.gain; slot rings become "wait on
// ring ai.rings[waitRing] at your own angle, with the strafe" (a flankOnly kind's angle kept out of your front cone, as
// its slots are); no squads, formations or flankers yet. Per kind (ai.kinds.<kind>): the token pool, speed, engage
// range, wait ring, radius; the Swarmer's flank rule (brain.js: pounce from outside the front cone, holders run for a
// flank point beside / behind you) is ported as it is.
// NIGHT19 U3-33.7 (builder E): the CHARGER joins the kinds with a brain (config.ai.crowd.mass order: husk, swarmer,
// spitter, charger; its heavy token, ai.kinds.charger). brain.js jumpsOf(kind) (ai.jumps.<kind>.jumps, true when absent):
// a kind that can't jump routes without jump links, and where its way needs one (a path's link, the field's link, the
// watchdog's hop: once per stuck spell, later rungs side-step) the brain sets ai.blocked (charger.js stops dazed) instead
// of asking a jump. Every NIGHT18 kind jumps, so their thinks are unchanged; only design.enemies.n19 spawns a charger.
// The DRONE (ai.kinds.drone, flies): brain.js's `k.flies` branch: straight at its goal (its own 3D steering avoids the
// level: Drone.cs), no ground navigation, side-step or watchdog; not in the crowd pass (ai.crowd.mass has no drone).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>
    /// One token pool's holders: the web's Map id -> {score, committed}, in its iteration order (a new holder goes last;
    /// a refreshed score keeps its place). The order decides who loses a token on a tie (NIGHT9 U3-07: a C# Dictionary
    /// reuses a removed entry's slot, so after a steal or a release it no longer iterated as the Map does).
    /// </summary>
    public sealed class TokenHolders
    {
        public readonly List<int> ids = new List<int>();
        public readonly List<double> scores = new List<double>();
        public readonly List<bool> committed = new List<bool>();

        public int Count => ids.Count;

        public int IndexOf(int id)
        {
            for (int i = 0; i < ids.Count; i++) if (ids[i] == id) return i;
            return -1;
        }

        /// <summary>Map.set of an id it doesn't hold: appended.</summary>
        public void Add(int id, double score, bool isCommitted)
        {
            ids.Add(id);
            scores.Add(score);
            committed.Add(isCommitted);
        }

        /// <summary>Map.delete.</summary>
        public bool Delete(int id)
        {
            int i = IndexOf(id);
            if (i < 0) return false;
            ids.RemoveAt(i);
            scores.RemoveAt(i);
            committed.RemoveAt(i);
            return true;
        }
    }

    /// <summary>ai/tokens.js: attack tokens per type (holders: id -> score, committed).</summary>
    public sealed class TokenPools
    {
        public readonly Dictionary<string, int> sizes = new Dictionary<string, int>();
        public readonly Dictionary<string, TokenHolders> holders = new Dictionary<string, TokenHolders>();
        /// <summary>Object.keys(pools.holders): the pools in creation order (resize walks them in this order).</summary>
        public readonly List<string> types = new List<string>();

        public static Dictionary<string, int> PoolSizes(GameConfig cfg, int round, bool camping)
        {
            var baseSizes = new List<(string, int)>();
            foreach (var kv in cfg.Obj("ai.tokens.base")) baseSizes.Add((kv.Key, (int)PortData.Num(kv.Value)));
            return PoolSizes(round, camping, baseSizes, cfg.Nums("ai.tokens.roundSteps"), cfg.Int("ai.tokens.campBonus"));
        }

        /// <summary>tokenPoolSizes(round, camping, t): pool sizes for a round (every pool in t.base).</summary>
        public static Dictionary<string, int> PoolSizes(int round, bool camping, IList<(string type, int size)> baseSizes, double[] roundSteps, int campBonus)
        {
            int bonus = 0;
            foreach (double r in roundSteps) bonus += round >= r ? 1 : 0;
            bonus += camping ? campBonus : 0;
            var output = new Dictionary<string, int>();
            foreach (var (type, size) in baseSizes) output[type] = size + bonus;
            return output;
        }

        /// <summary>createTokenPools(sizes)</summary>
        public TokenPools(Dictionary<string, int> s)
        {
            foreach (var kv in s)
            {
                sizes[kv.Key] = kv.Value;
                holders[kv.Key] = new TokenHolders();
                types.Add(kv.Key);
            }
        }

        /// <summary>resizeTokenPools: new sizes (a round change, camping); tokens over a shrunk pool go back, uncommitted worst first.</summary>
        public List<int> Resize(Dictionary<string, int> s, List<int> lost = null)
        {
            lost ??= new List<int>();
            sizes.Clear();
            foreach (var kv in s) sizes[kv.Key] = kv.Value;
            foreach (string type in types)
            {
                TokenHolders h = holders[type];
                // h.size > undefined is false: a pool missing from the new sizes keeps its holders.
                while (sizes.TryGetValue(type, out int cap) && h.Count > cap)
                {
                    int worst = Worst(h, true) ?? Worst(h, false) ?? -1;
                    h.Delete(worst);
                    lost.Add(worst);
                }
            }
            return lost;
        }

        /// <summary>The holder with the lowest score (only uncommitted ones when `uncommittedOnly`); the first in Map order on a tie.</summary>
        static int? Worst(TokenHolders h, bool uncommittedOnly)
        {
            int? worst = null;
            double worstScore = double.PositiveInfinity;
            for (int i = 0; i < h.Count; i++)
            {
                if (uncommittedOnly && h.committed[i]) continue;
                if (h.scores[i] < worstScore)
                {
                    worst = h.ids[i];
                    worstScore = h.scores[i];
                }
            }
            return worst;
        }

        /// <summary>
        /// requestToken: granted when the pool has room, or by stealing from the worst uncommitted holder whose score is
        /// below `score` by more than `margin` (relative). Holding one already refreshes its score. Returns who lost it to
        /// a steal (or null).
        /// </summary>
        public (bool granted, int? stolenFrom) Request(string type, int id, double score, double margin = 0.25)
        {
            TokenHolders h = holders[type];
            int mine = h.IndexOf(id);
            if (mine >= 0)
            {
                h.scores[mine] = score;
                return (true, null);
            }
            if (sizes.TryGetValue(type, out int cap) && h.Count < cap)
            {
                h.Add(id, score, false);
                return (true, null);
            }
            int? worst = Worst(h, true);
            if (!worst.HasValue) return (false, null);
            double worstScore = h.scores[h.IndexOf(worst.Value)];
            if (score <= worstScore + Math.Abs(worstScore) * margin) return (false, null);
            h.Delete(worst.Value);
            h.Add(id, score, false);
            return (true, worst);
        }

        /// <summary>commitToken: the attack started (a wind-up, a charge): the token can no longer be stolen.</summary>
        public bool Commit(string type, int id)
        {
            TokenHolders h = holders[type];
            int i = h.IndexOf(id);
            if (i >= 0) h.committed[i] = true;
            return i >= 0;
        }

        public bool Release(string type, int id) => holders[type].Delete(id);

        /// <summary>releaseAll: drop every token an enemy holds (death, despawn).</summary>
        public void ReleaseAll(int id)
        {
            foreach (string type in types) holders[type].Delete(id);
        }

        public bool Holds(string type, int id) => holders[type].IndexOf(id) >= 0;

        public int HolderCount(string type) => holders[type].Count;
    }

    /// <summary>ai/watchdog.js (pure): progress or the ladder.</summary>
    public sealed class Watch
    {
        public double stuck, cycleStart, ax, az, best = double.PositiveInfinity;
        public int next, events;

        /// <summary>createWatch(x = 0, z = 0): a fresh watch for one enemy.</summary>
        public Watch(double x = 0, double z = 0)
        {
            ax = x;
            az = z;
        }

        /// <summary>canRelocate: stuck long enough to be relocated (the caller checks it is off-screen)?</summary>
        public bool CanRelocate(double relocateAt) => stuck >= relocateAt;

        public void Reset(double x, double z, double dist = double.PositiveInfinity)
        {
            stuck = 0;
            next = 0;
            cycleStart = 0;
            ax = x;
            az = z;
            best = dist;
        }

        public string Step(double x, double z, double dist, bool wants, double dt, double minMove, double minGain, double[] ladder, List<string> actions)
        {
            bool onField = dist < double.PositiveInfinity;
            bool gained = onField && best < double.PositiveInfinity && dist <= best - minGain;
            bool moved = !onField && JsMath.Hypot(x - ax, z - az) >= minMove;
            if (!wants || gained || moved)
            {
                Reset(x, z, wants ? JsMath.Min(dist, best) : double.PositiveInfinity);
                return null;
            }
            if (!(best < double.PositiveInfinity)) best = dist;
            stuck += dt;
            if (stuck - cycleStart >= ladder[next])
            {
                string action = actions[next];
                next++;
                events++;
                if (next >= ladder.Length)
                {
                    next = 0;
                    cycleStart += ladder[ladder.Length - 1];
                }
                return action;
            }
            return null;
        }
    }

    /// <summary>A crowd body (ai/crowd.js): plain {x, y, z, vx, vz, r, m}.</summary>
    public struct CrowdBody
    {
        public double x, y, z, vx, vz, r, m;
    }

    /// <summary>
    /// ai/crowd.js (POLISH V6, pure, line for line): a hard body-to-body push between walking enemies, applied once per
    /// fixed step after every enemy system has moved. Two bodies closer than their radii plus a gap are moved apart along
    /// the line between them, the lighter one more, and the approach speed along that line is removed (no bounce, no
    /// jitter), so a group fans out and keeps its spacing. Flyers, bosses and anything mid-leap or mid-knockback are left
    /// out by the caller. Pairwise and allocation-free.
    /// </summary>
    public static class Crowd
    {
        /// <summary>
        /// One separation pass over the first `count` of `bodies` (mutated in place: x, z and vx, vz). gap: m added to the
        /// radii; stiffness: 0..1 of the overlap corrected per step; maxPush: m per step; height: bodies further apart
        /// vertically don't touch. Returns how many pairs overlapped (before this pass).
        /// </summary>
        public static int SeparateBodies(CrowdBody[] bodies, double gap, double stiffness, double maxPush, double height, int count)
        {
            int overlaps = 0;
            for (int i = 0; i < count; i++)
            {
                ref CrowdBody a = ref bodies[i];
                for (int j = i + 1; j < count; j++)
                {
                    ref CrowdBody b = ref bodies[j];
                    if (Math.Abs(a.y - b.y) > height) continue;
                    double dx = b.x - a.x;
                    double dz = b.z - a.z;
                    double min = a.r + b.r + gap;
                    if (Math.Abs(dx) >= min || Math.Abs(dz) >= min) continue;
                    double d = JsMath.Hypot(dx, dz);
                    if (d >= min) continue;
                    overlaps++;
                    if (d < 1e-6)
                    {
                        // Exactly on top of each other: split along a fixed, index-dependent direction (deterministic).
                        double ang = (i * 2.399963 + j * 0.7) % (Math.PI * 2);
                        dx = JsMath.Cos(ang);
                        dz = JsMath.Sin(ang);
                    }
                    else
                    {
                        dx /= d;
                        dz /= d;
                    }
                    double push = JsMath.Min(maxPush, (min - d) * stiffness);
                    double wa = b.m / (a.m + b.m); // the lighter body moves more
                    double wb = 1 - wa;
                    a.x -= dx * push * wa;
                    a.z -= dz * push * wa;
                    b.x += dx * push * wb;
                    b.z += dz * push * wb;
                    // Remove the closing speed along the line (keep the sideways part: they slide past each other).
                    double closing = (b.vx - a.vx) * dx + (b.vz - a.vz) * dz;
                    if (closing < 0)
                    {
                        a.vx += dx * closing * wa;
                        a.vz += dz * closing * wa;
                        b.vx -= dx * closing * wb;
                        b.vz -= dz * closing * wb;
                    }
                }
            }
            return overlaps;
        }
    }

    /// <summary>enemies/navJump.js: crouch (ai.jumps.husk.tell), then a leap to the landing (+ ai.jumpLandIn past it).</summary>
    public sealed class HuskNavJumper : NavJumper
    {
        readonly double tell, apex, swarmerTell, swarmerApex, spitterTell, spitterApex, landIn, gravity;
        readonly EventBus events;

        public HuskNavJumper(GameConfig cfg, EventBus events)
        {
            tell = cfg.Num("ai.jumps.husk.tell");
            apex = cfg.Num("ai.jumps.husk.apex");
            swarmerTell = cfg.Num("ai.jumps.swarmer.tell");
            swarmerApex = cfg.Num("ai.jumps.swarmer.apex");
            spitterTell = cfg.Num("ai.jumps.spitter.tell");
            spitterApex = cfg.Num("ai.jumps.spitter.apex");
            landIn = cfg.Num("ai.jumpLandIn");
            gravity = MovementMath.GravityFor(cfg.movement.jump.height, cfg.movement.jump.apexTime);
            this.events = events;
        }

        public override bool Step(Husk e, double dt, bool busy)
        {
            NavJumpState nav = e.jump;
            if (nav == null) return false;
            if (!nav.flying && nav.crouchSteps < 0)
            {
                // A fresh request: only grounded and free.
                if (!(e.grounded || e.onEdge) || busy)
                {
                    e.jump = null;
                    return false;
                }
                nav.crouchSteps = Math.Max(1, MovementMath.StepsFor(e.kind == "swarmer" ? swarmerTell : e.kind == "spitter" ? spitterTell : tell, dt));
                events.Emit("navJump", e.id);
            }
            if (!nav.flying)
            {
                e.velocity.x = 0;
                e.velocity.z = 0;
                if (busy)
                {
                    e.jump = null;
                    return false;
                }
                if (--nav.crouchSteps > 0) return true;
                double dx = nav.to.x - e.position.x, dz = nav.to.z - e.position.z;
                double len = JsMath.Hypot(dx, dz);
                if (len == 0) len = 1;
                double extra = nav.hop ? 0 : landIn;
                var to = new Vec3(nav.to.x + dx / len * extra, nav.to.y, nav.to.z + dz / len * extra);
                e.velocity = CombatMath.LeapVelocity(e.position, to, nav.apex > 0 ? nav.apex : e.kind == "swarmer" ? swarmerApex : e.kind == "spitter" ? spitterApex : apex, gravity, out _);
                nav.vx = e.velocity.x;
                nav.vz = e.velocity.z;
                e.grounded = false;
                nav.flying = true;
                nav.flightSteps = 0;
                return true;
            }
            nav.flightSteps++;
            if (!e.grounded)
            {
                e.velocity.x = nav.vx;
                e.velocity.z = nav.vz;
            }
            if ((e.grounded && nav.flightSteps > 2) || nav.flightSteps > MovementMath.StepsFor(2.5, dt))
            {
                e.jump = null;
                return false;
            }
            return true;
        }
    }

    public sealed partial class HuskBrain : IRunState // NIGHT56 G3: partial (tokens56: HuskBrain.N56.cs)
    {
        // NIGHT13 U3-26.10: the run snapshot, at a break (no enemy alive): the stream, the step count (the think phase), the
        // pool sizing key, the pace (SPEED DEMON), the counters, the token pools (sizes, holders in their Map order), the
        // line-of-sight queue, and the records' Dictionary: by a break every record is a dead enemy's, dropped (its tokens
        // released) at the next step, but the Dictionary's free slots decide the order the next round's records iterate in
        // (thinks and their draws), so its slot layout is saved (DictLayout) and the dead records come back as stand-ins
        // that the next step drops the same way. A record of a live enemy refuses the save.
        static readonly Husk DeadStandIn = new Husk(new HuskParams()) { alive = false };

        public void Save(StateWriter w)
        {
            var ids = new List<int>(recs.Count);
            foreach (KeyValuePair<int, Rec> kv in recs)
            {
                if (kv.Value.e.alive && kv.Value.e.id == kv.Value.id) throw new StateException($"snapshot: the brain drives a live enemy ({kv.Key}): a run is saved at a round break only");
                ids.Add(kv.Key);
            }
            w.Ints("recs", ids);
            w.Layout("recSlots", recs);
            var queued = new List<int>(losQueue.Count);
            foreach (Rec r in losQueue) queued.Add(r.id);
            w.Ints("losQueue", queued);
            w.Rng("rng", rng);
            w.I("steps", steps);
            w.I("poolKey", poolKey);
            w.D("speedMult", speedMult);
            w.Ints("counters", new[] { relocations, hops, sidesteps, repaths, linkJumps });
            w.Begin("pools");
            w.SI("sizes", pools.sizes);
            w.Strings("types", pools.types);
            foreach (string type in pools.types)
            {
                TokenHolders h = pools.holders[type];
                w.Begin(type);
                w.Ints("ids", h.ids);
                w.Doubles("scores", h.scores);
                w.Bools("committed", h.committed);
                w.End();
            }
            w.End();
        }

        public void Load(StateReader r)
        {
            List<int> ids = r.Ints("recs"), queued = r.Ints("losQueue"), counters = r.Ints("counters");
            r.Layout("recSlots", out int slots, out List<int> chain);
            if (counters.Count != 5) throw new StateException("snapshot: brain.counters");
            StateReader p = r.Section("pools");
            List<string> types = p.Strings("types");
            var holders = new List<(List<int>, List<double>, List<bool>)>();
            foreach (string type in types)
            {
                StateReader s = p.Section(type);
                List<int> hi = s.Ints("ids");
                List<double> hs = s.Doubles("scores");
                List<bool> hc = s.Bools("committed");
                if (hs.Count != hi.Count || hc.Count != hi.Count) throw new StateException($"snapshot: brain token pool {type}");
                holders.Add((hi, hs, hc));
            }
            var standIns = new List<Rec>(ids.Count);
            foreach (int id in ids) standIns.Add(new Rec { e = DeadStandIn, k = kinds["husk"], id = id });
            DictLayout.Rebuild(recs, ids, standIns, slots, chain, i => int.MinValue + i);
            losQueue.Clear();
            foreach (int id in queued) losQueue.Enqueue(recs.TryGetValue(id, out Rec q) ? q : new Rec { e = DeadStandIn, k = kinds["husk"], id = id, losQueued = true });
            foreach (Rec q in losQueue) q.losQueued = true;
            rng = r.Rng("rng");
            steps = r.Int("steps");
            poolKey = r.Int("poolKey");
            speedMult = r.D("speedMult");
            relocations = counters[0];
            hops = counters[1];
            sidesteps = counters[2];
            repaths = counters[3];
            linkJumps = counters[4];
            p.SI("sizes", pools.sizes);
            pools.types.Clear();
            pools.holders.Clear();
            for (int i = 0; i < types.Count; i++)
            {
                var h = new TokenHolders();
                (List<int> hi, List<double> hs, List<bool> hc) = holders[i];
                for (int j = 0; j < hi.Count; j++) h.Add(hi[j], hs[j], hc[j]);
                pools.types.Add(types[i]);
                pools.holders[types[i]] = h;
            }
        }

        /// <summary>ai.kinds.(kind) (+ ai.crowd.mass): what the brain needs per kind.</summary>
        sealed class KindAi
        {
            public string token;
            public double speed, engageRange, waitRadius, radius, mass;
            public bool flankOnly;
            public bool jumps = true; // NIGHT19: brain.js jumpsOf (ai.jumps.<kind>.jumps; the CHARGER can't)
            public bool flies; // NIGHT19: ai.kinds.<kind>.flies (the DRONE)
            public bool stationary; // NIGHT20: ai.kinds.<kind>.stationary (the LANCER: brain.js never steers it)
        }

        sealed class Rec
        {
            public Husk e;
            public KindAi k;
            public int flankSide;
            public int id, offset;
            public bool los, losQueued;
            public double losAge = double.PositiveInfinity;
            public int strafe = 1;
            public double strafeT;
            public double gx, gz; // the goal
            public double ax, az; // the arrive target
            public Watch watch = new Watch();
            public int sideUntil;
            public double sideX, sideZ;
            public List<(double x, double y, double z, int link)> path; // pathBuf while a repath's route is live, else null
            public readonly List<(double x, double y, double z, int link)> pathBuf = new List<(double, double, double, int)>();
            public readonly NavJumpState jumpState = new NavJumpState(); // reused by each nav jump (U2-02)
            public int pathI, pathUntil;
            public string mode = "field";
            public bool stuck;
            public bool blockedOnce; // NIGHT19: brain.js rec.blockedOnce (a non-jumper's daze, once per stuck spell)
            public int tell43 = -1; // NIGHT43 E1: the step its on-screen relocation fires (-1: no tell running)
            public double stillX43, stillZ43, stillT43; // NIGHT43 E1: Still43's anchor and its still time
            public double offCone56, idle56, advT56; public int adv56 = -1; // NIGHT56 G3 tokens56 (HuskBrain.N56.cs): out-of-view wait, idle time, the advance
            public HuskAi ai = new HuskAi();
        }

        readonly GameConfig cfg;
        readonly EventBus events;
        readonly NavField nav;
        readonly ICollisionWorld world;
        readonly Dictionary<int, Rec> recs = new Dictionary<int, Rec>();
        readonly Queue<Rec> losQueue = new Queue<Rec>();
        readonly List<Rec> order = new List<Rec>();
        public TokenPools pools;
        readonly int thinkEvery;
        readonly double speed, engageRange, directRange, carrotAhead, arriveDecel, gain, holdSpeed, losMaxAge, approachSlack, strafeRate, strafeAngle, strafeFlipTime;
        readonly double waitRadius, losBonus, stealMargin, minMove, minGain, relocateAt, relocateMin, relocateCone, sideTime, repathTime, hopDistance, hopApex, eyeHeight;
        readonly int losPerStep;
        readonly double[] ladder;
        readonly List<string> actions;
        readonly double gap, stiffness, maxPush, crowdHeight, frontCone, flankRadius;
        readonly int crowdMax;
        readonly Dictionary<string, KindAi> kinds = new Dictionary<string, KindAi>();
        readonly List<Husk> bodies = new List<Husk>();
        readonly CrowdBody[] crowdBodies; // ai.crowd.max, reused every step
        // The players as this step saw them (NIGHT9 Players): feet, facing, alive. Each enemy reads its nearest living one
        // (Players.NearestOf) when it thinks, checks its line of sight and asks flankNow; one player: [0], as the web.
        Vec3[] seenAt = new Vec3[4];
        double[] seenYaw = new double[4];
        bool[] seenAlive = new bool[4];
        int seenCount;
        Players stepPlayers; // null: the one-target Step
        Mulberry32 rng = new Mulberry32(7);
        int steps;
        int poolKey = -1; // round >= 10, round >= 20, camping as bits (-1: not sized yet)
        static readonly string[] CrowdKinds = { "husk", "swarmer", "spitter", "charger" }; // the crowd pass's kinds, in config.ai.crowd.mass order (NIGHT19: the charger)
        static readonly string[] BrainKinds = { "husk", "swarmer", "spitter", "charger", "drone", "lancer", "warden" }; // NIGHT19: the kinds with a brain (the crowd's and the DRONE); NIGHT20: the LANCER (stationary), the WARDEN (ai.kinds.warden: heavy, 3 m/s)
        readonly AimMath.A aim;
        public Func<double> playerSpeed = () => 0; // the hit roll's speed (ai/aim.js hitChance) with one target (Players: the target's own)
        public Func<Vec3, bool> onScreen = _ => true;
        public int relocations, hops, sidesteps, repaths, linkJumps;
        /// <summary>NIGHT33 E: thinks in which a lost walker held its spot because lava lay between it and the player (0 with ai33 off).</summary>
        public int lavaHolds33;
        /// <summary>
        /// NIGHT12 U3-25.9: brain.js pace (e.speedMult ?? 1: the SPEED DEMON mutator, affixes.js setSpeedMult) for every enemy
        /// this brain drives (the port has no affixes). It scales the speed cap as brain.js's max does; the side-step and the
        /// carrot keep k.speed, as on the web. 1 (the default): every result bit-identical (x * 1 is exact).
        /// </summary>
        public double speedMult = 1;
        /// <summary>NIGHT20 U3-34.6 (builder N): design.routes' jumper (Core/Nav/RouteLinks.cs), null with the layer off: a husk
        /// walks to its leap's approach point (Navigate), and the layer's climbs are asked of it.</summary>
        public RouteJumper routes;

        public HuskBrain(GameConfig cfg, EventBus events, NavField nav, ICollisionWorld world)
        {
            this.cfg = cfg;
            this.events = events;
            this.nav = nav;
            this.world = world;
            pools = new TokenPools(TokenPools.PoolSizes(cfg, 1, false));
            thinkEvery = Math.Max(1, (int)JsMath.Round(cfg.movement.loop.simHz / cfg.Num("ai.thinkHz")));
            aim = AimMath.A.From(cfg);
            foreach (string kind in BrainKinds)
            {
                kinds[kind] = new KindAi
                {
                    token = cfg.Str($"ai.kinds.{kind}.token"), speed = cfg.Num($"ai.kinds.{kind}.speed"), engageRange = cfg.Num($"ai.kinds.{kind}.engageRange"),
                    waitRadius = PortData.Num(PortData.Obj(cfg.Arr("ai.rings")[cfg.Int($"ai.kinds.{kind}.waitRing")])["radius"]),
                    radius = cfg.Num($"ai.kinds.{kind}.radius"), mass = cfg.Has($"ai.crowd.mass.{kind}") ? cfg.Num($"ai.crowd.mass.{kind}") : 1,
                    flankOnly = cfg.Obj($"ai.kinds.{kind}").TryGetValue("flankOnly", out object fo) && fo is bool b && b,
                    jumps = !cfg.Has($"ai.jumps.{kind}.jumps") || cfg.Bool($"ai.jumps.{kind}.jumps"),
                    flies = cfg.Obj($"ai.kinds.{kind}").TryGetValue("flies", out object fl) && fl is bool fb && fb,
                    stationary = cfg.Obj($"ai.kinds.{kind}").TryGetValue("stationary", out object st) && st is bool sb && sb,
                };
            }
            frontCone = JsMath.Cos(cfg.Num("swarmer.frontConeDeg") * Math.PI / 180);
            flankRadius = PortData.Num(PortData.Obj(cfg.Arr("ai.rings")[0])["radius"]) + 0.8;
            crowdMax = cfg.Int("ai.crowd.max");
            crowdBodies = new CrowdBody[crowdMax];
            speed = kinds["husk"].speed;
            engageRange = kinds["husk"].engageRange;
            directRange = cfg.Num("ai.directRange");
            carrotAhead = cfg.Num("ai.carrotAhead");
            arriveDecel = cfg.Num("ai.steer.arriveDecel");
            gain = cfg.Num("ai.steer.gain");
            holdSpeed = cfg.Num("ai.holdSpeed");
            losMaxAge = cfg.Num("ai.losMaxAge");
            losPerStep = cfg.Int("ai.losPerStep");
            approachSlack = cfg.Num("ai.approachSlack");
            strafeRate = cfg.Num("ai.strafeRate");
            strafeAngle = cfg.Num("ai.strafeAngle");
            strafeFlipTime = cfg.Num("ai.strafeFlipTime");
            waitRadius = PortData.Num(PortData.Obj(cfg.Arr("ai.rings")[cfg.Int("ai.kinds.husk.waitRing")])["radius"]);
            losBonus = cfg.Num("ai.tokens.losBonus");
            stealMargin = cfg.Num("ai.tokens.stealMargin");
            minMove = cfg.Num("ai.watchdog.minMove");
            minGain = cfg.Num("ai.watchdog.minGain");
            ladder = cfg.Nums("ai.watchdog.ladder");
            actions = cfg.Strs("ai.watchdog.actions");
            relocateAt = cfg.Num("ai.watchdog.relocateAt");
            relocateMin = cfg.Num("ai.watchdog.relocateMin");
            relocateCone = cfg.Num("ai.watchdog.relocateCone");
            sideTime = cfg.Num("ai.watchdog.sideTime");
            repathTime = cfg.Num("ai.watchdog.repathTime");
            hopDistance = cfg.Num("ai.watchdog.hopDistance");
            hopApex = cfg.Num("ai.watchdog.hopApex");
            gap = cfg.Num("ai.crowd.gap");
            stiffness = cfg.Num("ai.crowd.stiffness");
            maxPush = cfg.Num("ai.crowd.maxPush");
            crowdHeight = cfg.Num("ai.crowd.height");
            eyeHeight = cfg.movement.body.eyeHeight;
            events.On("enemyCommit", o =>
            {
                if (recs.TryGetValue((int)o, out Rec r)) pools.Commit(r.k.token, r.id);
                if (windupCap41 > 0 && r != null) Cap41AfterCommit(r.id);
            });
            events.On("enemyAttackDone", o =>
            {
                if (!recs.TryGetValue((int)o, out Rec r)) return;
                pools.Release(r.k.token, r.id);
                r.ai.attack = false;
                r.strafe = -r.strafe;
            });
        }

        public void NewRun(uint seed)
        {
            Clear();
            rng = new Mulberry32((seed ^ 0xb7a1u) >> 0);
            steps = 0;
            poolKey = -1;
        }

        public void Attach(Husk e)
        {
            var r = new Rec { e = e, k = kinds.TryGetValue(e.kind, out KindAi ka) ? ka : kinds["husk"], id = e.id, offset = recs.Count * 7 % thinkEvery, strafe = rng.Next() < 0.5 ? -1 : 1 };
            r.watch.Reset(e.position.x, e.position.z);
            if (r.k.flankOnly) r.ai.flankNow = OutsideFront;
            e.ai = r.ai;
            recs[e.id] = r;
        }

        public void Clear()
        {
            foreach (Rec r in recs.Values)
            {
                pools.ReleaseAll(r.id);
                if (r.e.ai == r.ai) r.e.ai = null;
            }
            recs.Clear();
            losQueue.Clear();
        }

        /// <summary>
        /// NIGHT41 E6 tokens41 (DECIDE41 D-5; Doom 2016's attack tokens, Research/2026-10-14-wave40/research/enemies-web.md section 3): a cap
        /// ACROSS the token pools on how many enemies this brain drives are in an attack at once (Husk.committed: a wind-up, a pounce, a
        /// spit, a charge, a tell). 0 = off (survival, and the campaign with tokens41 off): the web's pools alone, bit for bit. The sim sets
        /// it every step (SurvivalSim.N41.Foes.cs: design.n41.tokens41.cap while tokens41 is on in a campaign run).
        /// </summary>
        public int windupCap41;

        /// <summary>NIGHT41 E6: thinks in which an enemy in reach waited because the cap was full (0 with tokens41 off; the harness reads it).</summary>
        public int capWaits41;

        /// <summary>NIGHT43 E1 (design.n43.rooms43, GG G22 "fail to move"; CODE-FACTS-B: the watchdog relocated only OFF screen, after
        /// ai.watchdog.relocateAt): an enemy stuck this long (s) while ON screen shows a tell (event navTell43, its id) for onScreenTellSteps43,
        /// then steps to the nearest free node at least onScreenStepMin43 m away that still reaches the player (NavField.Relocate with a full
        /// circle round the enemy; event navRelocate43). 0 = off (survival, and the campaign with rooms43 off): the web's watchdog, bit for bit.
        /// The sim sets them every step (SurvivalSim.N43.Rooms.cs).</summary>
        public double onScreenRelocateAt43;
        public int onScreenTellSteps43;
        public double onScreenStepMin43;
        /// <summary>NIGHT43 E1: on-screen tells and relocations so far (the harness reads them).</summary>
        public int tells43, relocations43;

        /// <summary>NIGHT41 E6: the living enemies this brain drives that are in an attack now (`also`: one more id counted as in it: the
        /// enemyCommit handler runs before a Husk sets its own flag). No allocation (the Dictionary's struct enumerator).</summary>
        int CommittedNow41(int also)
        {
            int n = 0;
            foreach (Rec r in recs.Values) if (r.e.alive && (r.e.committed || r.id == also)) n++;
            return n;
        }

        /// <summary>NIGHT41 E6: an enemy just committed; when that fills the cap, every other enemy not in an attack is barred from starting
        /// one until its next think (where the cap's check in Think holds it on the ring), so no two commits in the same step pass the cap.</summary>
        void Cap41AfterCommit(int id)
        {
            if (CommittedNow41(id) < windupCap41) return;
            foreach (Rec r in recs.Values)
                if (r.id != id && !r.e.committed) r.ai.attack = false;
        }

        public int StuckNow
        {
            get
            {
                int n = 0;
                foreach (Rec r in recs.Values) if (r.stuck) n++;
                return n;
            }
        }

        /// <summary>NIGHT20 (builder N, for builder E's recycle): how long (s) the watchdog has seen enemy `id` make no progress
        /// toward you while it wants to (0: moving, or not driven by this brain). Read-only.</summary>
        public double StuckFor(int id) => recs.TryGetValue(id, out Rec r) ? r.watch.stuck : 0;

        /// <summary>NIGHT20 (builder N, for builder E): the enemy's navigation mode ("field", "direct", "path", "lost", "leap"), or null.</summary>
        public string ModeOf(int id) => recs.TryGetValue(id, out Rec r) ? r.mode : null;

        /// <summary>One fixed step: drop the dead, resize the pools, the fields, due thinks, the LOS budget.</summary>
        /// <summary>Outside the player's front cone (swarmer.frontConeDeg) as seen from (x, z)?</summary>
        bool OutsideFront(Vec3 pos)
        {
            int t = Players.NearestOf(pos, seenAt, seenAlive, seenCount);
            double x = pos.x, z = pos.z, px = seenAt[t].x, pz = seenAt[t].z, pyaw = seenYaw[t];
            double len = JsMath.Hypot(x - px, z - pz);
            if (len == 0) len = 1;
            return ((x - px) * -JsMath.Sin(pyaw) + (z - pz) * -JsMath.Cos(pyaw)) / len < frontCone;
        }

        /// <summary>One fixed step against one target (the tests, the bench).</summary>
        public void Step(double dt, Vec3 player, double playerYaw, int round, bool camping)
        {
            stepPlayers = null;
            seenAt[0] = player;
            seenYaw[0] = playerYaw;
            seenAlive[0] = true;
            seenCount = 1;
            StepCore(dt, round, camping);
        }

        /// <summary>One fixed step against every player (NIGHT9): each enemy works toward its nearest living one.</summary>
        public void Step(double dt, Players players, int round, bool camping)
        {
            stepPlayers = players;
            int n = players.Count;
            if (seenAt.Length < n)
            {
                Array.Resize(ref seenAt, n);
                Array.Resize(ref seenYaw, n);
                Array.Resize(ref seenAlive, n);
            }
            for (int i = 0; i < n; i++)
            {
                seenAt[i] = players[i].Feet;
                seenYaw[i] = players[i].yaw;
                seenAlive[i] = players[i].Alive;
            }
            seenCount = n;
            StepCore(dt, round, camping);
        }

        int TargetOf(Rec r) => Players.NearestOf(r.e.position, seenAt, seenAlive, seenCount);

        void StepCore(double dt, int round, bool camping)
        {
            steps++;
            order.Clear();
            foreach (Rec r in recs.Values) order.Add(r);
            foreach (Rec r in order)
            {
                if (!r.e.alive || r.e.id != r.id)
                {
                    pools.ReleaseAll(r.id);
                    recs.Remove(r.id);
                }
            }
            // NIGHT20 U3-34.6 row 4 R3 (builder N): with the route layer, a living player on a perch (NavField.Perched: no
            // ground walker reaches it on the web's own links) grows the melee and swarm pools by design.routes.perchTokens,
            // so the climbers come up together, not one file into a standing gun. Off: the bit is never set.
            bool perched = false;
            if (routes != null)
                for (int i = 0; i < seenCount && !perched; i++)
                    perched = seenAlive[i] && nav.Perched(seenAt[i].x, seenAt[i].y, seenAt[i].z);
            int key = (round >= 10 ? 1 : 0) | (round >= 20 ? 2 : 0) | (camping ? 4 : 0) | (perched ? 8 : 0);
            if (key != poolKey)
            {
                poolKey = key;
                Dictionary<string, int> sizes = TokenPools.PoolSizes(cfg, round, camping);
                if (perched)
                {
                    int extra = cfg.Int("design.routes.perchTokens");
                    if (sizes.ContainsKey("melee")) sizes["melee"] += extra;
                    if (sizes.ContainsKey("swarm")) sizes["swarm"] += extra;
                }
                pools.Resize(sizes);
            }
            nav.Step(dt, seenAt, seenAlive, seenCount);
            double dtThink = thinkEvery * dt;
            foreach (Rec r in recs.Values)
            {
                if ((steps + r.offset) % thinkEvery == 0)
                {
                    int t = TargetOf(r);
                    Think(r, dtThink, seenAt[t], seenYaw[t], stepPlayers != null ? stepPlayers[t].mover.Speed : playerSpeed());
                }
            }
            ProcessLos();
        }

        void ProcessLos()
        {
            for (int i = 0; i < losPerStep && losQueue.Count > 0; i++)
            {
                Rec r = losQueue.Dequeue();
                r.losQueued = false;
                if (!recs.ContainsKey(r.id)) continue;
                Vec3 p = seenAt[TargetOf(r)];
                Vec3 from = r.e.AimPoint;
                Vec3 eye = new Vec3(p.x, p.y + eyeHeight * 0.8, p.z);
                Vec3 d = eye - from;
                double len = d.Length;
                d = Vec3.DivideScalar(d, len > 0 ? len : 1);
                r.los = !world.RayIntersect(from, d, out RayHit hit) || hit.distance >= len - 0.3;
                r.ai.los = r.los; // brain.js rec.out.los (a spitter tells only in sight)
                r.losAge = 0;
            }
        }

        void AskJump(Rec r, double x, double y, double z, bool hop, double apexOverride = 0)
        {
            if (r.e.jump != null) return;
            NavJumpState j = r.jumpState;
            j.to = new Vec3(x, y, z);
            j.crouchSteps = -1;
            j.hop = hop;
            j.apex = apexOverride;
            j.flying = false;
            j.flightSteps = 0;
            j.vx = j.vz = 0;
            r.e.jump = j;
            if (!hop) linkJumps++;
        }

        void Think(Rec r, double dtThink, Vec3 p, double yaw, double targetSpeed)
        {
            Husk e = r.e;
            double d = JsMath.Hypot(p.x - e.position.x, p.z - e.position.z);
            r.losAge += dtThink;
            if (r.losAge > losMaxAge && !r.losQueued)
            {
                r.losQueued = true;
                losQueue.Enqueue(r);
            }
            r.strafeT += dtThink * strafeRate;
            if (rng.Next() < dtThink / strafeFlipTime) r.strafe = -r.strafe;
            if (stillNudgeS43 > 0) Still43(r, e, d, dtThink);

            // Tokens: ask when in range (+ line of sight for ranged); the closer (and in sight), the better the score.
            KindAi k = r.k;
            bool committed = e.committed;
            bool ranged = k.token == "ranged";
            bool wants = d <= k.engageRange && (!ranged || r.los);
            // NIGHT41 E6 tokens41: with the campaign cap full, an enemy not already in its attack stops wanting one (it gives its
            // token back and waits on the ring, as a token-less enemy does). 0 = off: nothing below changes.
            if (windupCap41 > 0 && wants && !committed && CommittedNow41(-1) >= windupCap41)
            {
                wants = false;
                capWaits41++;
            }
            if (on56) { if (committed) r.offCone56 = 0; else if (wants) wants = Wants56(r, dtThink); } // NIGHT56 G3 tokens56: on screen first (off: skipped)
            bool holding = pools.Holds(k.token, r.id);
            if (wants)
            {
                double score = -d + (r.los ? losBonus : 0);
                var res = pools.Request(k.token, r.id, score, stealMargin);
                holding = res.granted;
            }
            else if (holding && !committed)
            {
                pools.Release(k.token, r.id);
                holding = false;
            }
            r.ai.attack = holding;
            if (k.stationary)
            {
                // NIGHT20 (builder N, for builder E's LANCER; brain.js): "Lancers never walk: they blink between perches on
                // their own": no hit roll, goal, navigation, side-step or watchdog (brain.js returns before its hit roll).
                r.ai.vx = 0;
                r.ai.vz = 0;
                return;
            }
            // brain.js: a ranged holder rolls this attack's hit (ai/aim.js; the web's Math.random(), here the run's stream).
            if (holding && ranged && !committed) r.ai.hit = rng.Next() < AimMath.HitChance(targetSpeed, aim);
            if (k.flankOnly)
            {
                // Pounce only from your sides / back (outside your front cone).
                double len = d == 0 ? 1 : d;
                r.ai.flank = ((e.position.x - p.x) * -JsMath.Sin(yaw) + (e.position.z - p.z) * -JsMath.Cos(yaw)) / len < frontCone;
            }

            // The goal: an attacker closes on you; the others wait on the ring at their own angle and strafe.
            string phase;
            if (holding && k.flankOnly)
            {
                // Swarmers attack from your side or back: run for a point beside / behind you, then pounce.
                if (r.flankSide == 0) r.flankSide = r.strafe;
                // ai/slots.js flankPoint (NIGHT9 U3-07: yaw + (PI + side PI/3) as the web groups it, not (yaw + PI) + side PI/3).
                Slots.FlankPoint(p.x, p.z, yaw, flankRadius, r.flankSide * (d < 6 ? 0.9 : 1.3), out r.gx, out r.gz);
                // NIGHT20 U3-34.6 row 4 (builder N): with the route layer, a flank point with no floor at your height (a
                // 2.5 m ledge: it is in the air beside you) is you: a scratch perch test counted 88 swarmers in 60 s
                // climbing onto a 17.5 m ledge and falling off it toward the flank point.
                if (routes != null && nav.grid.NodeAt(r.gx, p.y, r.gz, 1.0, 0.6) < 0)
                {
                    r.gx = p.x;
                    r.gz = p.z;
                }
                phase = "attack";
            }
            else if (holding && !ranged)
            {
                // An attacker closes on you (a ranged holder keeps its ring: brain.js chooseGoal).
                r.gx = p.x;
                r.gz = p.z;
                phase = "attack";
            }
            else
            {
                double ang = JsMath.Atan2(e.position.z - p.z, e.position.x - p.x);
                double tri = 1 - 4 * Math.Abs(r.strafeT % 1 - 0.5);
                ang += r.strafe * strafeAngle * tri;
                if (k.flankOnly)
                {
                    // Its wait spot stays out of your front cone (the web refuses it the slots there).
                    double front = JsMath.Atan2(-JsMath.Cos(yaw), -JsMath.Sin(yaw));
                    double off = Math.IEEERemainder(ang - front, Math.PI * 2);
                    double cone = JsMath.Acos(frontCone) + 0.05;
                    if (Math.Abs(off) < cone) ang = front + (off == 0 ? r.strafe : Math.Sign(off)) * cone;
                }
                r.gx = p.x + JsMath.Cos(ang) * k.waitRadius;
                r.gz = p.z + JsMath.Sin(ang) * k.waitRadius;
                // NIGHT20 U3-34.6 row 4 (builder N): with the route layer, a ring spot with no floor at your height (a
                // 2.5 m ledge: the ring is in the air round it) is you: the waiters crowd your ledge instead of walking off
                // it (a scratch perch test: 88 swarmers in 60 s climbed a 17.5 m ledge and fell off it).
                if (routes != null && nav.grid.NodeAt(r.gx, p.y, r.gz, 1.0, 0.6) < 0)
                {
                    r.gx = p.x;
                    r.gz = p.z;
                }
                phase = d > k.waitRadius + approachSlack ? "approach" : "hold";
                if (on56) Advance56(r, e, holding, dtThink, ref phase); // NIGHT56 G3 tokens56: an idle waiter advances to a loop node in view (off: skipped)
            }
            double dist;
            if (k.flies)
            {
                // NIGHT19 (brain.js): drones fly over things: straight to the goal (their own 3D steering avoids obstacles).
                r.ax = r.gx;
                r.az = r.gz;
                r.mode = "fly";
                dist = double.PositiveInfinity;
            }
            else dist = Navigate(r, e, p, d, phase);

            // Arrive toward the target, blended at the steering gain; the watchdog side-step overrides for a moment.
            double tx = r.ax - e.position.x, tz = r.az - e.position.z;
            double tl = JsMath.Hypot(tx, tz);
            double max = (phase == "hold" ? k.speed * holdSpeed : k.speed) * speedMult * e.paceMult; // brain.js: (hold ? k.speed * a.holdSpeed : k.speed) * pace; NIGHT20: x the enemy's own pace (SWIFT; x 1 is exact)
            double want = Math.Min(max, tl / arriveDecel);
            double dvx = tl > 1e-6 ? tx / tl * want : 0, dvz = tl > 1e-6 ? tz / tl * want : 0;
            double blend = Math.Min(1, gain * dtThink);
            double vx = e.velocity.x + (dvx - e.velocity.x) * blend;
            double vz = e.velocity.z + (dvz - e.velocity.z) * blend;
            if (r.sideUntil > steps && !k.flies) // NIGHT19: brain.js: a flier has no side-step
            {
                vx = r.sideX * k.speed;
                vz = r.sideZ * k.speed;
            }
            double sp = JsMath.Hypot(vx, vz);
            if (sp > max)
            {
                vx *= max / sp;
                vz *= max / sp;
            }
            bool goalFar = JsMath.Hypot(r.ax - e.position.x, r.az - e.position.z) > 1.2 && d > 2.2;
            bool attacking = committed || e.windupSteps > 0;
            bool trying = goalFar && !attacking && phase != "hold";
            if (!k.flies) Watchdog(r, e, r.mode == "field" ? dist : double.PositiveInfinity, trying, dtThink, vx, vz, p, yaw); // NIGHT19: no watchdog for a flier (brain.js)
            r.ai.vx = vx;
            r.ai.vz = vz;
        }

        double Navigate(Rec r, Husk e, Vec3 p, double d, string phase)
        {
            double tx = r.gx, tz = r.gz;
            bool push = false;
            double dist = double.PositiveInfinity;
            if (routes != null && e.leapFromSteps > 0)
            {
                // NIGHT20 U3-34.6 step 2 (builder N): to the leap's approach point (RouteJumper.PlanLeap), straight there.
                r.path = null;
                r.mode = "leap";
                r.ax = e.leapFrom.x;
                r.az = e.leapFrom.z;
                e.leapOk = true;
                return dist;
            }
            if (r.path != null && r.pathUntil > steps)
            {
                var w = r.path;
                while (r.pathI < w.Count - 1 && JsMath.Hypot(w[r.pathI].x - e.position.x, w[r.pathI].z - e.position.z) < 0.9 && Math.Abs(w[r.pathI].y - e.position.y) < 1.2) r.pathI++;
                var next = w[r.pathI];
                tx = next.x;
                tz = next.z;
                push = r.pathI < w.Count - 1;
                if ((next.link == Link.Jump || next.link == Link.Gap || (routes != null && next.link == Link.LeapUp)) && r.pathI > 0)
                {
                    var from = w[r.pathI - 1];
                    if (JsMath.Hypot(from.x - e.position.x, from.z - e.position.z) < 1 && Math.Abs(from.y - e.position.y) < 0.8)
                    {
                        if (r.k.jumps) AskJump(r, next.x, next.y, next.z, false);
                        else r.ai.blocked = true; // NIGHT19 (brain.js): a kind that can't jump is blocked
                    }
                }
                r.mode = "path";
                if (routes != null) e.leapOk = true; // NIGHT20: a repath (the watchdog): the leap may help
            }
            else
            {
                r.path = null;
                double far = JsMath.Hypot(r.gx - e.position.x, r.gz - e.position.z);
                bool toPlayer = phase == "attack" || phase == "approach" || d > directRange || far > directRange;
                // NIGHT20 U3-34.6 row 4 (builder N): with the route layer, a waiter below you climbs to your level before it
                // holds its ring (THE CLIMB's perch tests: the token-less husks held their ring on the ground 15 m under a
                // perch, no threat; on the ground nothing changes: the rise is 0).
                if (routes != null && p.y - e.position.y > routes.layer.p.jumpUp) toPlayer = true;
                var route = new Route();
                // NIGHT20 U3-34.6 (builder N): with the route layer on, a swarmer reads its own field (its tall climbs).
                int field = !r.k.jumps ? NavField.FieldWalk : routes != null && e.kind == "swarmer" ? NavField.FieldSwarm : NavField.FieldJump;
                bool ok = nav.RouteFrom(e.position.x, e.position.y, e.position.z, field, ref route);
                dist = route.dist;
                bool direct = !toPlayer && ok && nav.WalkableAt(r.gx, e.position.y, r.gz) && nav.LineWalkable(e.position.x, e.position.y, e.position.z, r.gx, r.gz);
                if (routes != null) e.leapOk = !ok || direct || !nav.Reached(p.x, p.y, p.z); // NIGHT20: leap only at a perch no link chain reaches
                if (direct) r.mode = "direct";
                else if (ok)
                {
                    r.mode = "field";
                    if (routes != null && route.link == Link.LeapUp)
                    {
                        // NIGHT20 U3-34.6 row 4 (builder N): the layer's leap (a DANGER crouch stepping to the node's centre,
                        // then the arc flown at build); a charger's field has no leap-up.
                        routes.StartLeapLink(e, route.linkIndex);
                        tx = route.lx;
                        tz = route.lz;
                    }
                    else if (route.link == Link.Jump || route.link == Link.Gap)
                    {
                        if (r.k.jumps) AskJump(r, route.lx, route.ly, route.lz, false);
                        else r.ai.blocked = true; // NIGHT19 (brain.js)
                        tx = route.lx;
                        tz = route.lz;
                    }
                    else if (routes != null && route.link >= Link.Climb)
                    {
                        // NIGHT20 U3-34.6 step 3 (builder N): a climb link onward from here (CLIMB, CLIMB HIGH, AUTHORED).
                        routes.StartClimb(e, route.linkIndex);
                        tx = route.lx;
                        tz = route.lz;
                    }
                    else if (nav.IsGoal(route.node) || route.dist < cfg.Num("ai.flow.lookahead") * 0.5)
                    {
                        tx = phase == "attack" || phase == "approach" ? p.x : r.gx;
                        tz = phase == "attack" || phase == "approach" ? p.z : r.gz;
                    }
                    else
                    {
                        tx = route.tx;
                        tz = route.tz;
                        push = true;
                    }
                }
                else
                {
                    r.mode = "lost";
                    // NIGHT33 E (design.n33.ai33.goalLine): no route reaches you and lava lies on the straight line: hold this spot (a spitter still shoots) instead of
                    // walking at you into the lava's edge guard. Off (hazardGoalLine false): the web's straight steer.
                    if (nav.grid.hazardGoalLine && nav.grid.hazard != null)
                    {
                        int c = nav.grid.ColOf(e.position.x, e.position.z);
                        if (c >= 0 && nav.grid.HazardBetween(c, e.position.y, tx, p.y, tz))
                        {
                            tx = e.position.x;
                            tz = e.position.z;
                            lavaHolds33++;
                        }
                    }
                }
            }
            if (push)
            {
                double dx = tx - e.position.x, dz = tz - e.position.z;
                double len = JsMath.Hypot(dx, dz);
                if (len == 0) len = 1;
                double ahead = Math.Max(carrotAhead, r.k.speed * arriveDecel * 1.2);
                r.ax = e.position.x + dx / len * ahead;
                r.az = e.position.z + dz / len * ahead;
            }
            else
            {
                r.ax = tx;
                r.az = tz;
            }
            return dist;
        }

        void Watchdog(Rec r, Husk e, double dist, bool wants, double dtThink, double vx, double vz, Vec3 p, double yaw)
        {
            string action = r.watch.Step(e.position.x, e.position.z, dist, wants, dtThink, minMove, minGain, ladder, actions);
            r.stuck = wants && r.watch.stuck >= ladder[0];
            if (r.watch.stuck == 0) r.blockedOnce = false; // NIGHT19 (brain.js)
            if (action != null)
            {
                double sp = JsMath.Hypot(vx, vz);
                if (sp == 0) sp = 1;
                double dx = vx / sp, dz = vz / sp;
                if (action == "jump" && r.k.jumps)
                {
                    AskJump(r, e.position.x + dx * hopDistance, e.position.y, e.position.z + dz * hopDistance, true, hopApex);
                    hops++;
                }
                else if (action == "jump" && !r.blockedOnce)
                {
                    // NIGHT19 (brain.js): the Charger can't hop: once per stuck spell it stops, dazed (visible); later rungs side-step instead.
                    r.ai.blocked = true;
                    r.blockedOnce = true;
                }
                else if (action == "jump")
                {
                    double s = rng.Next() < 0.5 ? -1 : 1;
                    r.sideX = -dz * s;
                    r.sideZ = dx * s;
                    r.sideUntil = steps + MovementMath.StepsFor(sideTime, cfg.Dt);
                    sidesteps++;
                }
                else if (action == "sidestep")
                {
                    double s = rng.Next() < 0.5 ? -1 : 1;
                    r.sideX = -dz * s;
                    r.sideZ = dx * s;
                    r.sideUntil = steps + MovementMath.StepsFor(sideTime, cfg.Dt);
                    sidesteps++;
                }
                else if (action == "repath")
                {
                    r.path = nav.Path(e.position.x, e.position.y, e.position.z, p.x, p.y, p.z, r.k.jumps, r.pathBuf) ? r.pathBuf : null;
                    r.pathI = r.path != null && r.path.Count > 1 ? 1 : 0;
                    r.pathUntil = steps + MovementMath.StepsFor(repathTime, cfg.Dt);
                    repaths++;
                }
                events.Emit("navStuck", action);
            }
            if (wants && r.watch.CanRelocate(relocateAt) && !onScreen(new Vec3(e.position.x, e.position.y + 1, e.position.z)))
            {
                if (nav.Relocate(p.x, p.z, yaw, relocateMin, relocateCone, r.k.jumps, out Vec3 spot))
                {
                    e.Teleport(spot);
                    r.watch.Reset(e.position.x, e.position.z);
                    r.path = null;
                    relocations++;
                    events.Emit("navRelocate", e.id);
                }
            }
            if (onScreenRelocateAt43 > 0) Watchdog43(r, e, wants);
        }

        /// <summary>NIGHT43 E1 (G22: "no enemy stands still > 3 s while the player is in reach"): an enemy not in an attack that moved less than
        /// stillMoveM43 in stillNudgeS43 s within stillRangeM43 of its target takes a new wait spot: its strafe flips, its ring angle jumps half
        /// a strafe cycle and a swarmer's flank side flips (the ring waiters re-place instead of holding one spot; no roll of the stream).
        /// 0 = off: the web's ring.</summary>
        public double stillNudgeS43, stillMoveM43, stillRangeM43;
        public int nudges43;

        void Still43(Rec r, Husk e, double d, double dtThink)
        {
            double mv = JsMath.Hypot(e.position.x - r.stillX43, e.position.z - r.stillZ43);
            if (mv > stillMoveM43 || e.committed || d > stillRangeM43 || r.k.stationary)
            {
                r.stillX43 = e.position.x;
                r.stillZ43 = e.position.z;
                r.stillT43 = 0;
                return;
            }
            r.stillT43 += dtThink;
            if (r.stillT43 < stillNudgeS43) return;
            r.strafe = -r.strafe;
            r.strafeT += 0.5;
            if (r.flankSide != 0) r.flankSide = -r.flankSide;
            r.stillT43 = 0;
            nudges43++;
        }

        /// <summary>NIGHT43 E1: the on-screen relocation (see onScreenRelocateAt43): a tell, then a short step to a free node.</summary>
        void Watchdog43(Rec r, Husk e, bool wants)
        {
            if (!wants || !r.watch.CanRelocate(onScreenRelocateAt43) || !onScreen(new Vec3(e.position.x, e.position.y + 1, e.position.z)))
            {
                r.tell43 = -1;
                return;
            }
            if (r.tell43 < 0)
            {
                r.tell43 = steps + onScreenTellSteps43;
                tells43++;
                events.Emit("navTell43", e.id);
                return;
            }
            if (steps < r.tell43) return;
            r.tell43 = -1;
            if (nav.Relocate(e.position.x, e.position.z, 0, onScreenStepMin43, 180, r.k.jumps, out Vec3 spot))
            {
                e.Teleport(spot);
                r.path = null;
                relocations43++;
                events.Emit("navRelocate43", e.id);
            }
            r.watch.Reset(e.position.x, e.position.z);
        }

        /// <summary>
        /// enemies.js crowdPass + ai/crowd.js separateBodies: one hard separation pass over the standing walkers (husks,
        /// then swarmers; not mid-leap, mid-knockback or in a nav jump; at most ai.crowd.max), the lighter body moved more.
        /// </summary>
        public int Separate(Husks husks)
        {
            bodies.Clear();
            foreach (string kind in CrowdKinds)
            {
                foreach (Husk e in husks.PoolOf(kind))
                {
                    if (bodies.Count >= crowdMax) break;
                    if (!e.alive || !e.grounded || e.leaping || e.knockSteps > 0 || e.jump != null) continue;
                    bodies.Add(e);
                }
            }
            // The bodies as ai/crowd.js sees them ({x, y, z, vx, vz, r, m}), one pass of Crowd.SeparateBodies (the function
            // golden-checked against crowd.json), then the moved positions and velocities back.
            int n = bodies.Count;
            for (int i = 0; i < n; i++)
            {
                Husk e = bodies[i];
                KindAi k = kinds[e.kind];
                crowdBodies[i] = new CrowdBody { x = e.position.x, y = e.position.y, z = e.position.z, vx = e.velocity.x, vz = e.velocity.z, r = k.radius, m = k.mass };
            }
            int overlaps = Crowd.SeparateBodies(crowdBodies, gap, stiffness, maxPush, crowdHeight, n);
            for (int i = 0; i < n; i++)
            {
                Husk e = bodies[i];
                e.position.x = crowdBodies[i].x;
                e.position.z = crowdBodies[i].z;
                e.velocity.x = crowdBodies[i].vx;
                e.velocity.z = crowdBodies[i].vz;
            }
            return overlaps;
        }
    }
}
