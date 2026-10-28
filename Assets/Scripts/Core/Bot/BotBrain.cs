// Port of skygrind/src/bot/botBrain.js (NIGHT14 U3-27.4), line for line with its comments:
// BotBrain (SPEC 2.0 §6 Duel): RIVAL's decisions, written into an InputFrame for the shared Mover and
// loadout. ENGAGE (line of sight: strafe, jump, keep the weapon's preferred range, fire when the
// look settles inside the aim error), CHASE (A* to where the player was last seen), EVADE (a
// sideways dash after the player's shot passes close, at most once per cooldown), RECOVER (low HP:
// a health orb, else a spot out of sight, for a few seconds). It perceives the player through a
// reaction delay and aims with an error that grows with the player's speed, so moving fast is how
// you beat it. Fixed step only.
// The port: the web's one InputFrame is BotFrame (the Mover's frame + fire + weapon); getCollider().rayIntersect is
// deps.ray; rng (Math.random) is deps.rng, a Mulberry32, drawn in the web's short-circuit order. config.bot and the
// weapons' ranges are read once (the per-weapon numbers cached on first use, the difficulty's on change). Nothing is
// allocated per step: the reaction history is two Vec3 rings, A* writes into two NavPath buffers (the follower keeps the
// one it follows, so a failed plan leaves it as the web leaves it), hideSpot's map / filter / sort / slice is a
// selection over reused arrays in the same order (by distance, ties by index: JS's stable sort), brain.plan is one
// reused object.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>brain.plan (debug): the last plan's kind, goal node, cost and node count.</summary>
    public sealed class BotPlan
    {
        public string kind;
        public NavNode goal;
        public double cost;
        public int nodes;
    }

    public sealed class BotBrain : IBotBrain
    {
        const double DEG = Math.PI / 180;

        static double Wrap(double a) => JsMath.Atan2(JsMath.Sin(a), JsMath.Cos(a));

        readonly BotSelf self;
        readonly BotDeps deps;
        readonly double dt0;
        readonly int historySize;
        readonly Vec3[] historyP, historyV;
        int head;
        int filled;
        readonly Follower follower;
        Vec3 eye;
        Vec3 seen; // the perceived (delayed) player chest
        Vec3 seenVel;
        Vec3 aimAt;
        Vec3 lastSeen;

        // config.bot (read once)
        readonly Dictionary<string, double> reaction = new Dictionary<string, double>();
        readonly Dictionary<string, double> aimErrorMult = new Dictionary<string, double>();
        readonly Dictionary<string, double> preferredRange = new Dictionary<string, double>();
        readonly Dictionary<string, object> weapons;
        readonly Dictionary<string, double> rangeCache = new Dictionary<string, double>();
        readonly Dictionary<string, double> preferredCache = new Dictionary<string, double>();
        readonly double eyeHeight, losInterval, aimErrorBase, aimErrorPerSpeed, aimTolerance, errorReroll, shotgunRange, rangeSlack;
        readonly double strafeSwitch0, strafeSwitch1, jumpChance, evadeCooldownTime, evadeRadius, recoverHp, recoverTime, wallJumpRate;
        readonly double chestHeight, crouchChest, turnRate, replanInterval, replanDistance, searchAfter, approachHopSpeed;
        readonly double hideRadius, hideCandidates, recoverCooldownTime, slideJumpGap, slideJumpChance, revolverRange, revolverPreferred;
        readonly double crouchEye, dashPipCost, dashSpeed, walkSpeed;

        public string difficulty = "normal";
        public string state = "chase";
        /// <summary>Where RIVAL looks (and shoots).</summary>
        public readonly BotLook look = new BotLook();
        public bool visible;
        /// <summary>brain.stats.shots (debug).</summary>
        public int statsShots;
        readonly BotPlan planObj = new BotPlan();
        bool hasPlan;

        double losTimer;
        double visibleFor; // s of continuous line of sight
        double lostFor; // s without it
        bool hasSeen;
        double errYaw;
        double errPitch;
        double errTimer;
        int strafeDir = 1;
        double strafeTimer;
        double evadeCooldown;
        double evadeIn = -1; // s until a scheduled dodge
        int evadeSide = 1;
        double recoverLeft;
        double recoverCooldown;
        double replanTimer;
        int planGoal = -1; // node index (-1: the web's null)
        bool slidePending;
        int weaponWant;

        readonly NavPath pathA = new NavPath(), pathB = new NavPath();
        double[] hideD = new double[0];
        int[] hideI = new int[0];

        string diffCached;
        double reactionNow, aimMultNow;

        public BotBrain(GameConfig config, BotSelf self, BotDeps deps)
        {
            this.self = self;
            this.deps = deps;
            MovementConfig c = config.movement;
            foreach (var kv in config.Obj("bot.reaction")) reaction[kv.Key] = PortData.Num(kv.Value);
            foreach (var kv in config.Obj("bot.aimErrorMult")) aimErrorMult[kv.Key] = PortData.Num(kv.Value);
            foreach (var kv in config.Obj("bot.preferredRange")) preferredRange[kv.Key] = PortData.Num(kv.Value);
            weapons = config.Obj("weapons");
            eyeHeight = config.Num("bot.eyeHeight");
            losInterval = config.Num("bot.losInterval");
            aimErrorBase = config.Num("bot.aimErrorBase");
            aimErrorPerSpeed = config.Num("bot.aimErrorPerSpeed");
            aimTolerance = config.Num("bot.aimTolerance");
            errorReroll = config.Num("bot.errorReroll");
            shotgunRange = config.Num("bot.shotgunRange");
            rangeSlack = config.Num("bot.rangeSlack");
            double[] strafe = config.Nums("bot.strafeSwitch");
            strafeSwitch0 = strafe[0];
            strafeSwitch1 = strafe[1];
            jumpChance = config.Num("bot.jumpChance");
            evadeCooldownTime = config.Num("bot.evadeCooldown");
            evadeRadius = config.Num("bot.evadeRadius");
            recoverHp = config.Num("bot.recoverHp");
            recoverTime = config.Num("bot.recoverTime");
            wallJumpRate = config.Num("bot.wallJumpRate");
            chestHeight = config.Num("bot.chestHeight");
            crouchChest = config.Num("bot.crouchChest");
            turnRate = config.Num("bot.turnRate");
            replanInterval = config.Num("bot.replanInterval");
            replanDistance = config.Num("bot.replanDistance");
            searchAfter = config.Num("bot.searchAfter");
            approachHopSpeed = config.Num("bot.approachHopSpeed");
            hideRadius = config.Num("bot.hideRadius");
            hideCandidates = config.Num("bot.hideCandidates");
            recoverCooldownTime = config.Num("bot.recoverCooldown");
            slideJumpGap = config.Num("bot.slideJumpGap");
            slideJumpChance = config.Num("bot.slideJumpChance");
            revolverRange = config.Num("weapons.revolver.range");
            revolverPreferred = preferredRange.TryGetValue("revolver", out double rp) ? rp : double.NaN;
            crouchEye = c.slide.colliderHeight - 0.2;
            dashPipCost = c.dash.pipCost;
            dashSpeed = c.dash.speed;
            walkSpeed = c.ground.walkSpeed;

            dt0 = 1 / c.loop.simHz;
            double maxReaction = double.NegativeInfinity;
            foreach (double r in reaction.Values) maxReaction = JsMath.Max(maxReaction, r); // Math.max(...Object.values(b.reaction))
            historySize = (int)(Math.Ceiling(maxReaction / dt0) + 2);
            historyP = new Vec3[historySize];
            historyV = new Vec3[historySize];
            follower = new Follower(config);
        }

        public string Difficulty => difficulty;
        public string State => state;
        public BotLook Look => look;
        public bool Visible => visible;

        /// <summary>brain.follower.</summary>
        public Follower Follower => follower;

        /// <summary>brain.plan: the last plan (debug), or null.</summary>
        public BotPlan Plan => hasPlan ? planObj : null;

        /// <summary>brain.override: walk the graph to this point and ignore the target (the botNav debug preset).</summary>
        public Vec3? overrideGoal;
        /// <summary>brain.pickWeapon (RIVAL PRIME): (distance, current slot) =&gt; slot; null: by distance.</summary>
        public Func<double, int, int> pickWeapon;
        /// <summary>brain.directChase (RIVAL PRIME): chase straight at the target, no graph.</summary>
        public bool directChase;
        /// <summary>brain.navStats.</summary>
        public int navPlans, navStuck;

        double Rng() => deps.rng.Next();

        /// <summary>b.reaction / b.aimErrorMult at the current difficulty (looked up when it changes).</summary>
        void Tables()
        {
            if (ReferenceEquals(difficulty, diffCached)) return;
            diffCached = difficulty;
            reactionNow = difficulty != null && reaction.TryGetValue(difficulty, out double r) ? r : double.NaN;
            aimMultNow = difficulty != null && aimErrorMult.TryGetValue(difficulty, out double m) ? m : double.NaN;
        }

        int ReactionSteps()
        {
            Tables();
            return (int)JsMath.Round(reactionNow / dt0);
        }

        /// <summary>Reset for a new round (null keeps the difficulty: the web's default argument).</summary>
        public void Reset(string newDifficulty)
        {
            difficulty = newDifficulty ?? difficulty;
            state = "chase";
            filled = 0;
            head = 0;
            losTimer = 0;
            visibleFor = 0;
            lostFor = 0;
            hasSeen = false;
            evadeCooldown = 0;
            evadeIn = -1;
            recoverLeft = 0;
            recoverCooldown = 0;
            replanTimer = 0;
            planGoal = -1;
            slidePending = false;
            weaponWant = 0;
            follower.Clear();
            visible = false;
            hasPlan = false;
        }

        /// <summary>
        /// The player fired: if the shot passed close and RIVAL knows where they are, dodge after the
        /// reaction time (at most once per cooldown).
        /// </summary>
        public void OnPlayerFire(Vec3 origin, Vec3 dir)
        {
            if (evadeCooldown > 0 || evadeIn >= 0 || !visible) return;
            Vec3 miss = Chest() - origin;
            double along = Vec3.Dot(miss, dir);
            if (along <= 0) return;
            miss = new Vec3(miss.x + dir.x * -along, miss.y + dir.y * -along, miss.z + dir.z * -along); // addScaledVector(dir, -along)
            if (miss.Length > evadeRadius) return;
            // Dodge to the side the shot missed on (away from the ray).
            double rightX = JsMath.Cos(look.yaw);
            double rightZ = -JsMath.Sin(look.yaw);
            evadeSide = miss.x * rightX + miss.z * rightZ >= 0 ? 1 : -1;
            Tables();
            evadeIn = reactionNow;
        }

        Vec3 Chest()
        {
            Vec3 p = self.mover.position;
            return new Vec3(p.x, p.y + chestHeight, p.z);
        }

        Vec3 EyePoint()
        {
            Vec3 p = self.mover.position;
            return new Vec3(p.x, p.y + (self.mover.crouched ? crouchEye : eyeHeight), p.z);
        }

        /// <summary>Clear line from a to b through the world collider.</summary>
        bool ClearLine(Vec3 a, Vec3 to)
        {
            Vec3 direction = to - a;
            double length = direction.Length;
            direction = Vec3.DivideScalar(direction, length);
            bool hit = deps.ray(a, direction, out RayHit h);
            return !hit || h.distance >= length - 0.05;
        }

        /// <summary>Plan a path to a world point; false when there is none.</summary>
        bool PlanTo(Vec3 point, string kind)
        {
            hasPlan = false;
            NavGraph graph = deps.getGraph?.Invoke();
            if (graph == null) return false;
            int from = Astar.NearestNode(graph, self.mover.position);
            int goal = Astar.NearestNode(graph, point);
            if (from < 0 || goal < 0) return false;
            NavPath path = follower.Path == pathA ? pathB : pathA; // the one the follower doesn't hold
            bool found = Astar.Path(graph, from, goal, dashSpeed, path);
            planGoal = goal;
            if (!found) return false;
            follower.SetPath(graph, path);
            planObj.kind = kind;
            planObj.goal = graph.nodes[goal];
            planObj.cost = path.cost;
            planObj.nodes = path.nodes.Count;
            hasPlan = true;
            return true;
        }

        /// <summary>A node near RIVAL the player can't see (RECOVER without an orb).</summary>
        NavNode HideSpot(Vec3 playerEye)
        {
            NavGraph graph = deps.getGraph?.Invoke();
            if (graph == null) return null;
            Vec3 p = self.mover.position;
            int n = graph.nodes.Count;
            if (hideD.Length < n)
            {
                hideD = new double[n];
                hideI = new int[n];
            }
            // graph.nodes.map((n, i) => ({n, i, d})).filter((o) => o.d < b.hideRadius)
            int count = 0;
            for (int i = 0; i < n; i++)
            {
                NavNode q = graph.nodes[i];
                double d = JsMath.Hypot(q.x - p.x, q.y - p.y, q.z - p.z);
                if (!(d < hideRadius)) continue;
                hideD[count] = d;
                hideI[count] = i;
                count++;
            }
            // .sort((x, y) => x.d - y.d).slice(0, b.hideCandidates): the next nearest each time (ties by index: a stable sort)
            int take = (int)JsMath.Min(count, hideCandidates);
            for (int k = 0; k < take; k++)
            {
                int m = k;
                for (int j = k + 1; j < count; j++)
                    if (hideD[j] < hideD[m] || (hideD[j] == hideD[m] && hideI[j] < hideI[m])) m = j;
                (hideD[k], hideD[m]) = (hideD[m], hideD[k]);
                (hideI[k], hideI[m]) = (hideI[m], hideI[k]);
                NavNode o = graph.nodes[hideI[k]];
                if (!ClearLine(playerEye, new Vec3(o.x, o.y + chestHeight, o.z))) return o;
            }
            return null;
        }

        /// <summary>Movement toward a world yaw, expressed in the look frame (so RIVAL can aim while it runs).</summary>
        void MoveAlong(ref BotFrame frame, double moveYaw, double amount = 1)
        {
            double rel = moveYaw - look.yaw;
            frame.move.moveZ = JsMath.Cos(rel) * amount;
            frame.move.moveX = -JsMath.Sin(rel) * amount;
        }

        /// <summary>One fixed step: perceive, decide, write `frame` (a fresh neutral frame).</summary>
        public void Step(ref BotFrame frame, double dt)
        {
            Mover mover = self.mover;
            if (overrideGoal.HasValue)
            {
                NavOnly(ref frame, dt);
                return;
            }
            Tables();
            BotTarget player = deps.getPlayer();
            // --- Perception: the player's chest + velocity, delayed by the reaction time --------------------
            int slot = head;
            historyP[slot] = new Vec3(player.position.x, player.position.y + (player.crouched ? crouchChest : chestHeight), player.position.z);
            historyV[slot] = player.velocity;
            head = (head + 1) % historySize;
            filled = Math.Min(historySize, filled + 1);
            int lag = Math.Min(ReactionSteps(), filled - 1);
            int past = (head - 1 - lag + historySize * 2) % historySize;
            seen = historyP[past];
            seenVel = historyV[past];

            eye = EyePoint();
            losTimer -= dt;
            if (losTimer <= 0)
            {
                losTimer = losInterval;
                visible = !player.dead && ClearLine(eye, historyP[slot]);
            }
            if (visible)
            {
                visibleFor += dt;
                lostFor = 0;
                lastSeen = player.position;
                hasSeen = true;
            }
            else
            {
                visibleFor = 0;
                lostFor += dt;
            }
            bool reacted = visibleFor >= reactionNow;
            evadeCooldown = JsMath.Max(0, evadeCooldown - dt);
            recoverCooldown = JsMath.Max(0, recoverCooldown - dt);

            // --- State ----------------------------------------------------------------------------------------
            double hp = self.health.hp;
            if (state != "recover" && hp < recoverHp && recoverCooldown <= 0)
            {
                state = "recover";
                recoverLeft = recoverTime;
                int orb = NearestOrb();
                Vec3? spot = null;
                if (orb >= 0) spot = deps.orbs[orb].position;
                else
                {
                    NavNode hide = HideSpot(historyP[slot]);
                    if (hide != null) spot = new Vec3(hide.x, hide.y, hide.z);
                }
                follower.Clear();
                if (!spot.HasValue || !PlanTo(spot.Value, orb >= 0 ? "orb" : "hide")) recoverLeft = 0;
            }
            if (state == "recover")
            {
                recoverLeft -= dt;
                if (recoverLeft <= 0 || hp >= recoverHp || !follower.Active)
                {
                    recoverCooldown = recoverCooldownTime;
                    state = reacted ? "engage" : "chase";
                    follower.Clear();
                }
            }
            else if (evadeIn >= 0)
            {
                evadeIn -= dt;
                if (evadeIn < 0 && mover.BoostReady(dashPipCost, false)) // NIGHT17 U3-30.4: the web's pips test, or a full boost's cost on the bar
                {
                    state = "evade";
                    evadeCooldown = evadeCooldownTime;
                }
            }
            else if (state == "evade")
            {
                state = reacted ? "engage" : "chase";
            }
            else if (reacted)
            {
                if (state != "engage") follower.Clear();
                state = "engage";
            }
            else if (lostFor > reactionNow || !hasSeen)
            {
                state = "chase";
            }

            // --- Aim (every state: RIVAL shoots whenever it can see you) -------------------------------------
            double horiz = JsMath.Hypot(seen.x - eye.x, seen.z - eye.z);
            double distance = JsMath.Hypot(horiz, seen.y - eye.y);
            double lead = lag * dt0; // hitscan: lead only the perception lag
            aimAt = new Vec3(seen.x + seenVel.x * lead, seen.y + seenVel.y * lead, seen.z + seenVel.z * lead);
            errTimer -= dt;
            if (errTimer <= 0)
            {
                errTimer = errorReroll;
                double speed = JsMath.Hypot(seenVel.x, seenVel.z);
                double errDeg = (aimErrorBase + aimErrorPerSpeed * speed) * aimMultNow;
                double r = errDeg * DEG * Math.Sqrt(Rng());
                double a = Rng() * Math.PI * 2;
                errYaw = r * JsMath.Cos(a);
                errPitch = r * JsMath.Sin(a);
            }
            double wantYaw = JsMath.Atan2(-(aimAt.x - eye.x), -(aimAt.z - eye.z)) + errYaw;
            double wantPitch = JsMath.Atan2(aimAt.y - eye.y, JsMath.Hypot(aimAt.x - eye.x, aimAt.z - eye.z)) + errPitch;
            bool engaged = visible && hasSeen;
            double turn = turnRate * DEG * dt;
            if (engaged)
            {
                double dYaw = Wrap(wantYaw - look.yaw);
                look.yaw = Wrap(look.yaw + JsMath.Max(-turn, JsMath.Min(turn, dYaw)));
                look.pitch += JsMath.Max(-turn, JsMath.Min(turn, wantPitch - look.pitch));
            }

            // Weapon by distance, with a little hysteresis (RIVAL PRIME picks from its own loadout).
            if (pickWeapon != null) weaponWant = pickWeapon(distance, weaponWant);
            else if (distance < shotgunRange - 1) weaponWant = 1;
            else if (distance > shotgunRange + 1) weaponWant = 0;
            frame.weapon = weaponWant;
            bool settled = Math.Abs(Wrap(wantYaw - look.yaw)) < aimTolerance * DEG && Math.Abs(wantPitch - look.pitch) < aimTolerance * DEG;
            double range = WeaponRange(self.weapon());
            if (engaged && reacted && settled && distance < range && !player.dead)
            {
                frame.fire.held = true;
                frame.fire.pressed = true;
            }

            // --- Movement ---------------------------------------------------------------------------------
            frame.move.yaw = look.yaw;
            frame.move.pitch = look.pitch;
            if (state == "evade")
            {
                frame.move.moveX = evadeSide;
                frame.move.moveZ = 0;
                frame.move.dash.pressed = frame.move.dash.held = true;
            }
            else if (state == "engage")
            {
                Engage(ref frame, dt, distance);
            }
            else
            {
                // CHASE / RECOVER: follow a path; look along it unless RIVAL can see you.
                bool hasTarget = false;
                Vec3 target = default;
                if (state == "chase" && directChase)
                {
                    // RIVAL PRIME's arena has no graph: straight at you, sliding along cover it runs into.
                    target = lostFor > searchAfter || !hasSeen ? player.position : lastSeen;
                    hasTarget = true;
                    follower.Clear();
                }
                else if (state == "chase")
                {
                    replanTimer -= dt;
                    target = lostFor > searchAfter || !hasSeen ? player.position : lastSeen;
                    hasTarget = true;
                    NavGraph graph = deps.getGraph?.Invoke();
                    bool moved = planGoal < 0 || graph == null || JsMath.Hypot(graph.nodes[planGoal].x - target.x, graph.nodes[planGoal].z - target.z) > replanDistance;
                    if (!follower.Active || (replanTimer <= 0 && moved))
                    {
                        replanTimer = replanInterval;
                        PlanTo(target, "chase");
                    }
                }
                // A one-node path (you are nearest the node RIVAL is nearest) has no edge to follow.
                bool trivial = state == "chase" && (directChase || (hasPlan && planObj.nodes <= 1));
                FollowStatus status = trivial ? FollowStatus.Idle : follower.Step(mover, ref frame.move, dt);
                if (status == FollowStatus.Stuck) replanTimer = 0;
                if (hasTarget && !visible && (status == FollowStatus.Arrived || status == FollowStatus.Idle || status == FollowStatus.Stuck))
                {
                    // The graph got RIVAL as close as it can (the node nearest you can be a top beside you with no
                    // line of sight): walk straight at you, hopping over whatever is in the way.
                    double dx = target.x - mover.position.x;
                    double dz = target.z - mover.position.z;
                    frame.move.yaw = JsMath.Atan2(-dx, -dz);
                    frame.move.moveX = directChase && mover.touchingWall ? strafeDir : 0;
                    frame.move.moveZ = 1;
                    if (mover.grounded && (mover.touchingWall || mover.Speed < approachHopSpeed)) frame.move.jump.pressed = frame.move.jump.held = true;
                    else if (!mover.grounded) frame.move.jump.held = true;
                    status = FollowStatus.Moving;
                }
                if (engaged) MoveAlong(ref frame, frame.move.yaw, JsMath.Hypot(frame.move.moveX, frame.move.moveZ));
                if (engaged || status == FollowStatus.Idle)
                {
                    frame.move.yaw = look.yaw;
                    frame.move.pitch = look.pitch;
                }
                else
                {
                    look.yaw = frame.move.yaw;
                    look.pitch *= 0.9;
                    frame.move.pitch = look.pitch;
                }
            }

            // Hard: wall jumps off anything it touches in the air.
            if (difficulty == "hard" && !mover.grounded && (mover.state == MoveState.WallRide || mover.touchingWall) && Rng() < wallJumpRate * dt)
            {
                frame.move.jump.pressed = frame.move.jump.held = true;
            }
        }

        /// <summary>
        /// The weapon's range: the shotgun's is b.shotgunRange + 2, any other config.weapons[weapon]?.range ??
        /// config.weapons.revolver.range (cached per weapon id on first use).
        /// </summary>
        double WeaponRange(string weapon)
        {
            if (weapon == null) return revolverRange;
            if (rangeCache.TryGetValue(weapon, out double r)) return r;
            if (weapon == "shotgun") r = shotgunRange + 2;
            else if (weapons.TryGetValue(weapon, out object w) && w is Dictionary<string, object> d && d.TryGetValue("range", out object v) && v != null) r = PortData.Num(v);
            else r = revolverRange;
            rangeCache[weapon] = r;
            return r;
        }

        /// <summary>b.preferredRange[weapon] ?? b.preferredRange.revolver (cached per weapon id on first use).</summary>
        double Preferred(string weapon)
        {
            if (weapon == null) return revolverPreferred;
            if (preferredCache.TryGetValue(weapon, out double r)) return r;
            r = preferredRange.TryGetValue(weapon, out double p) ? p : revolverPreferred;
            preferredCache[weapon] = r;
            return r;
        }

        /// <summary>Debug: walk the graph to `goal` (null returns to normal play).</summary>
        public void SetOverride(Vec3? goal)
        {
            overrideGoal = goal;
            ClearPath();
        }

        /// <summary>Drop the current path (a teleport); the next step replans.</summary>
        public void ClearPath()
        {
            follower.Clear();
            replanTimer = 0;
            planGoal = -1;
        }

        /// <summary>
        /// Debug (botNav preset): ignore the player and walk the nav graph to `brain.override` {x, y, z}.
        /// brain.navStats counts replans.
        /// </summary>
        void NavOnly(ref BotFrame frame, double dt)
        {
            if (!follower.Active || replanTimer <= 0)
            {
                replanTimer = double.PositiveInfinity;
                if (PlanTo(overrideGoal.Value, "nav")) navPlans++;
            }
            FollowStatus status = follower.Step(self.mover, ref frame.move, dt);
            if (status == FollowStatus.Stuck)
            {
                navStuck++;
                replanTimer = 0;
            }
            if (status == FollowStatus.Arrived || status == FollowStatus.Idle)
            {
                frame.move.moveX = frame.move.moveZ = 0;
                frame.move.yaw = look.yaw;
            }
            look.yaw = frame.move.yaw;
            look.pitch = 0;
            frame.move.pitch = 0;
        }

        /// <summary>Duel countdown: stand still, facing the player.</summary>
        public void Face(ref BotFrame frame)
        {
            Vec3 p = deps.getPlayer().position;
            eye = EyePoint();
            look.yaw = JsMath.Atan2(-(p.x - eye.x), -(p.z - eye.z));
            look.pitch = 0;
            frame.move.yaw = look.yaw;
            frame.move.pitch = 0;
        }

        /// <summary>ENGAGE movement: face the player, keep the preferred range, strafe, hop.</summary>
        void Engage(ref BotFrame frame, double dt, double distance)
        {
            Mover mover = self.mover;
            double preferred = Preferred(self.weapon());
            frame.move.moveZ = distance > preferred + rangeSlack ? 1 : distance < preferred - rangeSlack ? -1 : 0;
            strafeTimer -= dt;
            if (strafeTimer <= 0 || (mover.grounded && mover.touchingWall))
            {
                strafeTimer = strafeSwitch0 + Rng() * (strafeSwitch1 - strafeSwitch0);
                strafeDir = Rng() < 0.5 ? -1 : 1;
                if (mover.grounded && Rng() < jumpChance) frame.move.jump.pressed = frame.move.jump.held = true;
            }
            frame.move.moveX = strafeDir;
            // Closing a long gap: slide, then jump out of it (a slide jump keeps the speed).
            if (slidePending && mover.state == MoveState.Slide)
            {
                frame.move.jump.pressed = frame.move.jump.held = true;
                slidePending = false;
            }
            else if (mover.grounded && frame.move.moveZ > 0 && distance > preferred + slideJumpGap && mover.Speed > walkSpeed * 0.8 && Rng() < slideJumpChance * dt)
            {
                frame.move.slide.pressed = frame.move.slide.held = true;
                slidePending = true;
            }
            else if (!mover.grounded)
            {
                frame.move.jump.held = true;
                slidePending = false;
            }
        }

        /// <summary>The nearest active orb's index in deps.orbs, or -1.</summary>
        int NearestOrb()
        {
            Vec3 p = self.mover.position;
            int best = -1;
            double bestD = double.PositiveInfinity;
            List<BotOrb> orbs = deps.orbs;
            for (int i = 0; i < orbs.Count; i++)
            {
                if (!orbs[i].active) continue;
                double d = Vec3.DistanceTo(orbs[i].position, p);
                if (d < bestD)
                {
                    bestD = d;
                    best = i;
                }
            }
            return best;
        }
    }
}
