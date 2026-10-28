// NIGHT26 (builder B, contract glacier): THE GLACIER, level 2's chase boss (GG: "Ghiaccio from JoJo part 5 ... you're kind of like skating ... speed skating to get this
// boss and use movement, dodge these telegraph attacks and get close to the boss and do damage"; "you're only allowed to use the first [weapon] and you have to get close").
// A fast skating hull AHEAD of the player on a long straight course of ICE (Design/maps/glacier-course.json: one 40 m tile, 3 lanes, low rails, wall lines). The world does
// not scroll: tile k sits at z = -tileLen x k for ever (doubles, no drift) and the sim keeps a sliding WINDOW of tiles (back + ahead + 1 of them, 8 = 320 m) as one box layer:
// the tiles behind the player RECYCLE ahead (GlacierWindow). Everything is a function of the player's course distance.
//   GAP METER  gap = boss distance - player distance along the course (m). It starts at gap.start (60). The boss can be hurt only at gap <= gap.hurtAt (25). At gap >= gap.failAt
//              (80) a soft fail: the player goes back to the last checkpoint (every checkpointTiles tiles), the gap back to 60, the boss keeps its HP (bossPunish).
//   SPEED      the boss skates at speed.base x the phase's mult and RUBBER-BANDS IN ONE DIRECTION ONLY: x (1 + speed.rubber x clamp((gap - rubberNear) / (rubberFar - rubberNear)))
//              : faster when you are far, never slower when you are near (the HUD says so: RubberText).
//   ATTACKS    lane attacks placed ahead of the player, each with a telegraph of tells.* seconds (>= 0.6) and never two resolving inside tells.resolveGap (0.5 s):
//              SWEEP a low bar over every lane (jump), LOW BAR at head height (slide), SPIKES a wall across two lanes (be in the open lane: dash into it).
//   A hit on the hull x1, on the engine block at its back x2; a close-range bonus on the damage (closeBonus: 1 at range, up to x at point blank): get close and it pays.
// Fixed 120 Hz, timers in whole steps (stepsFor), JsMath, no audio, no UnityEngine, no PhysX. A new module (the web has no glacier).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.n26.bosses.glacier, read once.</summary>
    public sealed class GlacierParams
    {
        public string name = "THE GLACIER";
        public double pool, phase2At, phase3At, hullMult, engineMult;
        public double tileLen, laneHalf, railX, checkpointTiles;
        public int winBack, winAhead;
        public double[] laneX = new double[3];
        public double gapStart, gapHurt, gapFail, rubber, rubberNear, rubberFar;
        public double baseSpeed, phaseMult, laneMove, laneEvery, introHold;
        public double sweepTop, lowBottom, lowTop, spikeHeight;
        public double tellSweep, tellLow, tellSpikes, life, atkDist, atkDamage, atkPush, resolveGap, thick;
        public double every1, every2, every3;
        public double closeRange, closeMax;
        public double hullW, hullH, hullL, engW, engH, engL, contactDamage, contactCooldown, dieTime;
        public double capsuleHeight, capsuleRadius;

        public static GlacierParams From(GameConfig c)
        {
            const string g = "design.n26.bosses.glacier.";
            var p = new GlacierParams
            {
                name = c.Str(g + "name"),
                pool = c.Num(g + "hp.pool"), phase2At = c.Num(g + "hp.phase2At"), phase3At = c.Num(g + "hp.phase3At"), hullMult = c.Num(g + "hp.hullMult"), engineMult = c.Num(g + "hp.engineMult"),
                tileLen = c.Num(g + "course.tileLen"), laneHalf = c.Num(g + "course.halfWidth"), railX = c.Num(g + "course.railX"), checkpointTiles = c.Num(g + "course.checkpointTiles"),
                winBack = c.Int(g + "course.windowBack"), winAhead = c.Int(g + "course.windowAhead"),
                gapStart = c.Num(g + "gap.start"), gapHurt = c.Num(g + "gap.hurtAt"), gapFail = c.Num(g + "gap.failAt"), rubber = c.Num(g + "speed.rubber"),
                rubberNear = c.Num(g + "speed.rubberNear"), rubberFar = c.Num(g + "speed.rubberFar"),
                baseSpeed = c.Num(g + "speed.base"), phaseMult = c.Num(g + "speed.phaseMult"), laneMove = c.Num(g + "speed.laneMove"), laneEvery = c.Num(g + "speed.laneEvery"), introHold = c.Num(g + "speed.introHold"),
                tellSweep = c.Num(g + "tells.sweep"), tellLow = c.Num(g + "tells.low"), tellSpikes = c.Num(g + "tells.spikes"), resolveGap = c.Num(g + "tells.resolveGap"),
                life = c.Num(g + "attack.life"), atkDist = c.Num(g + "attack.ahead"), atkDamage = c.Num(g + "attack.damage"), atkPush = c.Num(g + "attack.push"), thick = c.Num(g + "attack.thick"),
                sweepTop = c.Num(g + "attack.sweepTop"), lowBottom = c.Num(g + "attack.lowBottom"), lowTop = c.Num(g + "attack.lowTop"), spikeHeight = c.Num(g + "attack.spikeHeight"),
                every1 = c.Num(g + "attack.every1"), every2 = c.Num(g + "attack.every2"), every3 = c.Num(g + "attack.every3"),
                closeRange = c.Num(g + "closeBonus.range"), closeMax = c.Num(g + "closeBonus.max"),
                hullW = c.Num(g + "body.hullW"), hullH = c.Num(g + "body.hullH"), hullL = c.Num(g + "body.hullL"), engW = c.Num(g + "body.engW"), engH = c.Num(g + "body.engH"), engL = c.Num(g + "body.engL"),
                contactDamage = c.Num(g + "body.contactDamage"), contactCooldown = c.Num(g + "body.contactCooldown"), dieTime = c.Num(g + "body.dieTime"),
                capsuleHeight = c.Num("body.capsuleHeight"), capsuleRadius = c.Num("body.capsuleRadius"),
            };
            double[] lx = c.Nums(g + "course.laneX");
            for (int i = 0; i < 3; i++) p.laneX[i] = lx[i];
            return p;
        }
    }

    /// <summary>One tile's boxes (Design/maps/glacier-course.json), tile-local: z from 0 to -tileLen.</summary>
    public sealed class GlacierCourse
    {
        public readonly List<(Vec3 min, Vec3 max, string kind)> tile = new List<(Vec3, Vec3, string)>();
        public readonly List<(Vec3 min, Vec3 max, string kind)> start = new List<(Vec3, Vec3, string)>();
        /// <summary>Boost strips per tile (tile-local rects x0, z0, x1, z1).</summary>
        public readonly List<double[]> boosts = new List<double[]>();
        public Vec3 spawn;
        public double spawnYawDeg;

        public static GlacierCourse Load(string path)
        {
            var j = PortData.Obj(PortData.LoadJson(path));
            var c = new GlacierCourse();
            foreach (object o in PortData.Arr(j["tile"]))
            {
                var b = PortData.Obj(o);
                c.tile.Add((PortData.V3(b["min"]), PortData.V3(b["max"]), (string)b["kind"]));
            }
            foreach (object o in PortData.Arr(j["start"]))
            {
                var b = PortData.Obj(o);
                c.start.Add((PortData.V3(b["min"]), PortData.V3(b["max"]), (string)b["kind"]));
            }
            foreach (object o in PortData.Arr(j["boosts"]))
            {
                var r = PortData.Arr(o);
                c.boosts.Add(new[] { PortData.Num(r[0]), PortData.Num(r[1]), PortData.Num(r[2]), PortData.Num(r[3]) });
            }
            var sp = PortData.Obj(j["playerSpawn"]);
            c.spawn = new Vec3(PortData.Num(sp["x"]), PortData.Num(sp["y"]), PortData.Num(sp["z"]));
            c.spawnYawDeg = PortData.Num(sp["yawDeg"]);
            return c;
        }

        /// <summary>The boxes of tiles first..last (world: origin added, tile k at z - tileLen x k), plus the start cap while tile 0 is in the window.</summary>
        public void Window(int first, int last, double tileLen, Vec3 origin, List<(Vec3 min, Vec3 max)> into)
        {
            into.Clear();
            if (first <= 0) foreach (var (mn, mx, _) in start) into.Add((mn + origin, mx + origin));
            for (int k = Math.Max(0, first); k <= last; k++)
            {
                double dz = -tileLen * k;
                foreach (var (mn, mx, _) in tile) into.Add((new Vec3(mn.x + origin.x, mn.y + origin.y, mn.z + origin.z + dz), new Vec3(mx.x + origin.x, mx.y + origin.y, mx.z + origin.z + dz)));
            }
        }
    }

    public sealed class GlacierBody : Enemy
    {
        public readonly Glacier system;

        public GlacierBody(Glacier system)
        {
            this.system = system;
            kind = "glacier";
        }

        public override double Height => 2;
        public override double Radius => 2;
        public override double AimHeight => 1;
        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            headshot = false;
            return system.Raycast(o, d);
        }
        public override bool Damage(double amount, double flashTime)
        {
            system.Apply(amount);
            return false;
        }
        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt) { }
        public override void Stagger(double time, double dt) { }
        public override void Stun(double time, double dt) { }
    }

    /// <summary>A lane attack: a bar / wall across the course, lethal for `life` after its tell.</summary>
    public sealed class GlacierObstacle
    {
        public string kind; // sweep, low, spikes
        public double z, x0, x1, y0, y1; // world z (centre) and extents
        public int tellLeft, lifeLeft, total;
        public bool hit;
        public bool Live => tellLeft <= 0 && lifeLeft > 0;
    }

    public struct GlacierLog
    {
        public int step;
        public string name;
        public double seconds;
        public bool resolve;
    }

    public sealed class Glacier : IBossSystem
    {
        public readonly GlacierParams p;
        public readonly GlacierBody body;
        readonly EventBus events;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt0;
        readonly BossEvent said = new BossEvent();
        BossKit kit;
        BossScale scale = BossScale.One;
        Vec3 origin;
        public Vec3? arenaCenter;
        Mulberry32 rng = new Mulberry32(1);

        public readonly WyrmBox[] boxes = new WyrmBox[2]; // 0 hull, 1 engine block at its back
        public readonly Vec3[] prevCenters = new Vec3[2];
        public readonly List<GlacierObstacle> obstacles = new List<GlacierObstacle>();
        public readonly List<GlacierLog> log = new List<GlacierLog>();
        public readonly Dictionary<string, int> stats = new Dictionary<string, int>();
        static readonly string[] StatNames = { "attacks", "attackHits", "softFails", "blocked", "hullHits", "engineHits", "laneChanges" };

        public string phase = "dead";
        public int stepNo, phaseSteps;
        int beatSteps, dieSteps, lastResolve = -100000, atkLeft, atkIndex, laneLeft, contactLeft, introSteps;
        string nextPhase, hitPart, lastPart;
        double bossS, bossX, laneGoal;
        int lane = 1;
        public int checkpoint; // the course distance (m) of the last checkpoint
        public double gap, playerS;
        public Action<Vec3> teleport;
        public Func<Capsule> playerBody;
        /// <summary>The player's course distance, world z -> m (the sim hands the layer's origin; 0 at the spawn's tile start).</summary>
        double Dist(double worldZ) => origin.z - worldZ;

        public Glacier(GameConfig cfg, GlacierParams p, EventBus events, BossTarget target, Func<int> nextId)
        {
            this.p = p;
            this.events = events;
            this.target = target;
            this.nextId = nextId;
            dt0 = cfg.Dt;
            body = new GlacierBody(this);
            foreach (string n in StatNames) stats[n] = 0;
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;
        public double BossS => bossS;
        /// <summary>The boss can be hurt now (gap within hurtAt, and in a phase that takes damage).</summary>
        public bool Hurtable => gap <= p.gapHurt && body.alive && phase != "intro" && phase != "beat" && phase != "dying";
        public double CurrentSpeed => p.baseSpeed * PhaseSpeed * RubberMult;
        double PhaseSpeed => EffPhase >= 3 ? p.phaseMult * p.phaseMult : EffPhase == 2 ? p.phaseMult : 1;
        public double RubberMult => 1 + p.rubber * Math.Max(0, Math.Min(1, (gap - p.rubberNear) / Math.Max(1e-6, p.rubberFar - p.rubberNear)));
        /// <summary>The HUD's line: the gap, the hurt line and the one-way rule (faster when you are far, never slower when you are near).</summary>
        public string RubberText => RubberMult > 1.001 ? "IT SPEEDS UP WHEN YOU FALL BEHIND" : "IT NEVER SLOWS DOWN";
        public Vec3 BossWorld => new Vec3(origin.x + bossX, origin.y, origin.z - bossS);
        public Vec3 CheckpointSpawn => new Vec3(origin.x + p.laneX[1], origin.y, origin.z - checkpoint);
        static int PhaseNo(string ph) => ph == "p2" ? 2 : ph == "p3" ? 3 : 1;
        int EffPhase => phase == "beat" ? PhaseNo(nextPhase) : PhaseNo(phase);
        public string RetryPhase => !body.alive ? null : phase == "beat" ? nextPhase : phase == "p2" || phase == "p3" ? phase : null;
        public uint RngState { get => rng.State; set => rng.State = value; }
        public void NewRun(uint seed) => rng = new Mulberry32(seed);
        /// <summary>The window of tiles the sim keeps for a player at course distance s: [first, last].</summary>
        public void WindowFor(double s, out int first, out int last)
        {
            int k = (int)Math.Floor(Math.Max(0, s) / p.tileLen);
            first = Math.Max(0, k - p.winBack);
            last = k + p.winAhead;
        }

        void SetPhase(string ph)
        {
            phase = ph;
            phaseSteps = 0;
            said.Reset("glacier");
            said.phase = ph;
            said.hp = body.hp;
            said.maxHp = body.maxHp;
            events.Emit("bossPhase", said);
        }

        void Tell(string name, double seconds, Vec3 at)
        {
            said.Reset("glacier");
            said.name = name;
            said.seconds = seconds;
            events.Emit("bossTell", said);
            log.Add(new GlacierLog { step = stepNo, name = name, seconds = seconds });
            kit.telegraph.Threat(at.x, at.y, at.z);
        }

        void Resolve(string name)
        {
            lastResolve = stepNo;
            log.Add(new GlacierLog { step = stepNo, name = name, resolve = true });
        }

        public bool RestorePhaseStart()
        {
            string ph = RetryPhase;
            if (ph == null) return false;
            body.hp = body.maxHp * (ph == "p3" ? p.phase3At : p.phase2At);
            ResetChase();
            if (phase != ph) SetPhase(ph);
            return true;
        }

        /// <summary>The boss back to gap.start ahead of the checkpoint, the attacks cleared.</summary>
        void ResetChase()
        {
            obstacles.Clear();
            bossS = checkpoint + p.gapStart;
            bossX = p.laneX[1];
            lane = 1;
            atkLeft = S(p.every1);
            laneLeft = S(p.laneEvery);
            contactLeft = S(p.contactCooldown);
            Pose();
            for (int i = 0; i < 2; i++) prevCenters[i] = boxes[i].c;
        }

        public void StartFight(BossFight ctx)
        {
            kit = ctx.kit;
            scale = ctx.scale;
            origin = arenaCenter ?? ctx.center;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(p.pool * ctx.scale.hp);
            body.hp = body.maxHp;
            stepNo = 0;
            log.Clear();
            foreach (string n in StatNames) stats[n] = 0;
            lastResolve = -100000;
            checkpoint = 0;
            atkIndex = 0;
            ResetChase();
            body.position = BossWorld;
            body.previous = body.position;
            SetPhase("intro");
            introSteps = Math.Max(kit.IntroSteps(), S(p.introHold));
            said.Reset("glacier");
            said.name = "GLACIER";
            said.hp = body.maxHp;
            events.Emit("bossRise", said);
        }

        public void Clear()
        {
            body.alive = false;
            phase = "dead";
            obstacles.Clear();
        }

        void Pose()
        {
            for (int i = 0; i < 2; i++) prevCenters[i] = boxes[i].c;
            Vec3 f = new Vec3(0, 0, -1);
            var h = default(WyrmBox);
            h.Orient(f);
            h.c = new Vec3(origin.x + bossX, origin.y + p.hullH * 0.5 + 0.2, origin.z - bossS);
            h.hr = p.hullW / 2; h.hu = p.hullH / 2; h.hf = p.hullL / 2;
            boxes[0] = h;
            var e = default(WyrmBox);
            e.Orient(f);
            e.c = new Vec3(h.c.x, h.c.y + p.hullH * 0.5 + p.engH * 0.5 - 0.1, h.c.z + p.hullL * 0.5 - p.engL * 0.5);
            e.hr = p.engW / 2; e.hu = p.engH / 2; e.hf = p.engL / 2;
            boxes[1] = e;
        }

        public double Raycast(Vec3 o, Vec3 d)
        {
            if (!body.alive || phase == "dying" || phase == "dead") return -1;
            double best = -1;
            string name = null;
            for (int i = 0; i < 2; i++)
            {
                double t = boxes[i].Ray(o, d);
                if (t >= 0 && (best < 0 || t < best)) { best = t; name = i == 0 ? "hull" : "engine"; }
            }
            if (best < 0) return -1;
            hitPart = name;
            return best;
        }

        /// <summary>The part's multiplier (hull x1, engine x2) x the close-range bonus (1 at closeRange m or more, up to closeMax at point blank). Blocked (nothing lands) while the
        /// gap is over hurtAt and in the intro, the beat and the finisher.</summary>
        public double Filter(double amount, Vec3? from, out bool blocked)
        {
            blocked = false;
            if (!Hurtable)
            {
                blocked = true;
                stats["blocked"]++;
                hitPart = null;
                return 0;
            }
            string part = hitPart ?? "hull";
            hitPart = null;
            lastPart = part;
            double bonus = 1;
            if (from.HasValue)
            {
                double d = Vec3.DistanceTo(from.Value, boxes[0].c);
                bonus = 1 + (p.closeMax - 1) * Math.Max(0, Math.Min(1, (p.closeRange - d) / p.closeRange));
            }
            return amount * (part == "engine" ? p.engineMult : p.hullMult) * bonus;
        }

        public double TakeHit(Vec3 origin0, Vec3 dir, double amount, string weapon, out double distance)
        {
            distance = Raycast(origin0, dir);
            if (distance < 0) return -1;
            double dmg = Filter(amount, origin0, out bool blocked);
            if (blocked) return 0;
            Apply(dmg);
            return dmg;
        }

        public void Apply(double dmg)
        {
            if (!body.alive) return;
            body.hp -= dmg;
            body.flash = 0.06;
            string part = lastPart ?? "hull";
            stats[part == "engine" ? "engineHits" : "hullHits"]++;
            said.Reset("glacier");
            said.damage = JsMath.Round(dmg);
            said.part = part;
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            if (phase == "p1" && HpFrac <= p.phase2At) EnterBeat("p2", p.phase2At, "PHASE 2");
            else if (phase == "p2" && HpFrac <= p.phase3At) EnterBeat("p3", p.phase3At, "PHASE 3");
            else if (body.hp <= 0)
            {
                body.hp = 0;
                SetPhase("dying");
                dieSteps = S(p.dieTime);
                obstacles.Clear();
                kit.Dying("glacier", body.position);
            }
        }

        void EnterBeat(string to, double at, string label)
        {
            body.hp = body.maxHp * at;
            nextPhase = to;
            SetPhase("beat");
            beatSteps = kit.Beat("glacier", label, new Vec3(body.position.x, body.position.y + 2, body.position.z));
            obstacles.Clear();
            atkLeft = S(p.every1);
        }

        public BossBar Bar() => new BossBar
        {
            valid = body.alive, id = body.id, name = p.name, phase = phase, hp = Math.Max(0, body.hp), max = body.maxHp, segment = p.phase2At,
            pips = 2, pipsOn = phase == "p3" || phase == "dying" ? 0 : phase == "p2" || (phase == "beat" && nextPhase == "p3") ? 1 : 2,
        };

        Capsule PlayerCapsule()
        {
            if (playerBody != null) return playerBody();
            Vec3 f = target.position();
            return new Capsule { start = new Vec3(f.x, f.y + p.capsuleRadius, f.z), end = new Vec3(f.x, f.y + p.capsuleHeight - p.capsuleRadius, f.z), radius = p.capsuleRadius };
        }

        bool HurtPlayer(double amount)
        {
            Health h = target.health;
            if (h == null || h.Dead) return false;
            return h.Damage(amount * scale.damage, "boss");
        }

        double EverySec => EffPhase >= 3 ? p.every3 : EffPhase == 2 ? p.every2 : p.every1;

        void SpawnAttack()
        {
            string[] kinds = { "sweep", "low", "spikes" };
            string kind = kinds[atkIndex % 3];
            atkIndex++;
            double tell = kind == "sweep" ? p.tellSweep : kind == "low" ? p.tellLow : p.tellSpikes;
            double ahead = Math.Min(p.atkDist, Math.Max(8, gap - 4));
            double zc = origin.z - (playerS + ahead);
            var o = new GlacierObstacle { kind = kind, z = zc, tellLeft = S(tell), lifeLeft = S(p.life), total = S(tell) };
            double half = p.laneHalf;
            if (kind == "sweep") { o.x0 = origin.x - half; o.x1 = origin.x + half; o.y0 = 0; o.y1 = p.sweepTop; }
            else if (kind == "low") { o.x0 = origin.x - half; o.x1 = origin.x + half; o.y0 = p.lowBottom; o.y1 = p.lowTop; }
            else
            {
                // a wall of spikes across two lanes: the open lane is the one NOT under the player's next lane change: drawn from the stream
                int open = (int)(rng.Next() * 3); if (open > 2) open = 2;
                o.y0 = 0; o.y1 = p.spikeHeight;
                double cw = 2 * half / 3;
                o.x0 = origin.x - half; o.x1 = origin.x + half;
                openLane = open;
                openMin = origin.x - half + cw * open;
                openMax = openMin + cw;
            }
            obstacles.Add(o);
            Tell(kind, tell, new Vec3(origin.x, origin.y, zc));
            double len = o.x1 - o.x0;
            if (kind == "spikes")
            {
                // two telegraph lines, the open lane left clear
                if (openMin > o.x0 + 0.01) kit.telegraph.Line(o.x0, origin.y, zc, 1, 0, openMin - o.x0, p.thick, tell);
                if (openMax < o.x1 - 0.01) kit.telegraph.Line(openMax, origin.y, zc, 1, 0, o.x1 - openMax, p.thick, tell);
            }
            else kit.telegraph.Line(o.x0, origin.y, zc, 1, 0, len, p.thick, tell);
        }

        public int openLane;
        double openMin, openMax;
        /// <summary>The open lane's world x range of the newest spikes wall (the harness bot steers into it).</summary>
        public (double min, double max) OpenRange => (openMin, openMax);

        void StepObstacles()
        {
            Capsule c = PlayerCapsule();
            double feet = Math.Min(c.start.y, c.end.y) - c.radius - origin.y, top = Math.Max(c.start.y, c.end.y) + c.radius - origin.y;
            double px = c.start.x, pz = c.start.z;
            for (int i = obstacles.Count - 1; i >= 0; i--)
            {
                GlacierObstacle o = obstacles[i];
                if (o.tellLeft > 0)
                {
                    if (--o.tellLeft == 0) { Resolve(o.kind); stats["attacks"]++; }
                    continue;
                }
                if (--o.lifeLeft <= 0) { obstacles.RemoveAt(i); continue; }
                if (o.hit || phase == "intro") continue;
                bool inZ = Math.Abs(pz - o.z) < p.thick * 0.5 + c.radius;
                bool inY = top > o.y0 && feet < o.y1;
                bool inX = px + c.radius > o.x0 && px - c.radius < o.x1;
                if (o.kind == "spikes" && px > openMin + c.radius * 0.5 && px < openMax - c.radius * 0.5) inX = false;
                if (inZ && inY && inX)
                {
                    o.hit = true;
                    if (HurtPlayer(p.atkDamage))
                    {
                        stats["attackHits"]++;
                        target.impulse?.Invoke(0, 3, p.atkPush); // thrown back up the course (+z)
                    }
                }
            }
        }

        public void Step(double dt)
        {
            if (!body.alive) return;
            body.previous = body.position;
            if (Frozen) return;
            stepNo++;
            phaseSteps++;
            Vec3 pw = target.position();
            playerS = Dist(pw.z);
            gap = bossS - playerS;
            if (phase == "intro")
            {
                if (phaseSteps >= introSteps) SetPhase("p1");
            }
            else if (phase == "beat")
            {
                if (phaseSteps >= beatSteps) SetPhase(nextPhase);
            }
            else if (phase == "dying")
            {
                if (--dieSteps <= 0)
                {
                    body.alive = false;
                    phase = "dead";
                    kit.Down("glacier", body.id, body.position);
                    return;
                }
            }
            if (phase == "p1" || phase == "p2" || phase == "p3")
            {
                // the boss skates ahead; the rubber band is one way
                bossS += p.baseSpeed * PhaseSpeed * RubberMult * dt;
                if (--laneLeft <= 0)
                {
                    laneLeft = S(p.laneEvery);
                    int nl = (int)(rng.Next() * 3); if (nl > 2) nl = 2;
                    if (nl != lane) { lane = nl; stats["laneChanges"]++; }
                    laneGoal = p.laneX[lane];
                }
                double dx = laneGoal - bossX;
                if (laneGoal != 0 || lane != 1 || bossX != p.laneX[1]) bossX += Math.Max(-p.laneMove * dt, Math.Min(p.laneMove * dt, dx));
                gap = bossS - playerS;
                // a lane attack, never inside the gap of another lethal attack
                if (--atkLeft <= 0 && stepNo - lastResolve >= S(p.resolveGap) && !AnyTelling())
                {
                    SpawnAttack();
                    atkLeft = S(EverySec);
                }
                else if (atkLeft < 0) atkLeft = 0;
                // soft fail
                if (gap >= p.gapFail)
                {
                    stats["softFails"]++;
                    said.Reset("glacier");
                    events.Emit("bossPunish", said);
                    ResetChase();
                    teleport?.Invoke(CheckpointSpawn);
                    playerS = checkpoint;
                    gap = bossS - playerS;
                }
                // a checkpoint every checkpointTiles tiles
                double cp = Math.Floor(playerS / (p.checkpointTiles * p.tileLen)) * p.checkpointTiles * p.tileLen;
                if (cp > checkpoint) checkpoint = (int)cp;
            }
            body.position = BossWorld;
            body.velocity = new Vec3(0, 0, -CurrentSpeed);
            Pose();
            if (phase == "p1" || phase == "p2" || phase == "p3") StepObstacles();
        }

        bool AnyTelling()
        {
            foreach (GlacierObstacle o in obstacles) if (o.tellLeft > 0) return true;
            return false;
        }
    }
}
