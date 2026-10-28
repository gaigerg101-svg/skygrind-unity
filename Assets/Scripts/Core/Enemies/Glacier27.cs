// NIGHT27 builder B (boss2): THE GLACIER as a PULSE-ONLY CHASE (GG 2026-10-09: "I want this one to only use the pulse ... you can only attack him if you are able to catch up to him and hit him
// with the pulse"; "the arena should also curve a little bit"; "areas where you're forced to wall ride"). The verb is SKATE + PULSE LAUNCH; the weapon is the gauntlet only.
// A large blocky skater on one blade skates ahead of you down Design/maps/glacier-course.json (the 40 m ice tile, three lanes, recycled: the sim keeps a sliding window, as Glacier26 does).
//   GAP CONTROLLER  gap = boss course distance - player course distance. The boss's speed = base + clamp(k x (gap - target), +-clampFrac x top): top is the player's top speed on the ice, base
//                   baseFrac x top. Gap-based, never a timer; the clamp keeps it catchable (never more than 15 % of the player's top speed faster or slower than its base).
//   HIT WINDOW      when the gap falls under windowAt (12 m) a PULSE-READY flash shows and a window of windowTime (1.5 s) opens: a gauntlet hit (the PULSE launch that lands on the boss) counts ONCE.
//                   Nothing else hurts it (a gun, or a hit outside the window, is blocked). A miss (the window runs out) costs missCost (20 m) of gap: the boss skates on; NEVER health.
//                   Three hits kill: each takes a third of the pool; the first two start phase 2 and 3; after a hit the boss surges hitBack m ahead.
//   LANE ATTACKS    a floor decal (tell, >= 0.6 s: 0.75 s) then the attack arrives: P1 the LOW BAR (slide) and the SWEEP (jump); P2 adds the ROLLER (a log that rolls across the lanes: jump);
//                   P3 adds the ICICLE FALL (ceiling spikes over the lanes, safe in the wall columns: ride the wall). Never two lethal attacks inside resolveGap.
//   THE CURVE       the course bends in the VIEW only (Glacier27Curve: heading change <= maxTurnDeg per tile, from (runSeed, tileIndex)); the sim and the collider stay straight so golden and replays
//                   are untouched.
// Fixed 120 Hz, timers in whole steps, Mulberry32 + JsMath, no UnityEngine, no audio, no PhysX. A new module (the web has no glacier).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.n27.boss2, read once.</summary>
    public sealed class Glacier27Params
    {
        public string name = "THE GLACIER";
        public double pool, hitBack;
        public int hitsToKill;
        public double tileLen, laneHalf, railX, checkpointTiles;
        public int winBack, winAhead;
        public double[] laneX = new double[3];
        public double top, baseFrac, k, clampFrac, targetGap, targetStep, gapStart, windowAt, windowTime, missCost, failAt, armTime;
        public double laneMove, laneEvery, introHold;
        public double tellLane, tellRoller, tellIcicle, resolveGap, life, atkAhead, atkDamage, atkPush, thick;
        public double sweepTop, lowBottom, lowTop, rollerSpeed, rollerH, rollerLife, icicleTop, icicleSafe, icicleLife;
        public double every1, every2, every3;
        public double boxW, boxH, boxL, dieTime, contactDamage;
        public double curveMaxDeg, curveTiles, assistReach, assistCone, refill;
        public uint curveSalt;
        public double capsuleHeight, capsuleRadius;

        public static Glacier27Params From(GameConfig c)
        {
            const string g = "design.n27.boss2.";
            var p = new Glacier27Params
            {
                name = c.Str(g + "name"), pool = c.Num(g + "hits.pool"), hitsToKill = c.Int(g + "hits.toKill"),
                tileLen = c.Num(g + "course.tileLen"), laneHalf = c.Num(g + "course.halfWidth"), railX = c.Num(g + "course.railX"), checkpointTiles = c.Num(g + "course.checkpointTiles"),
                winBack = c.Int(g + "course.windowBack"), winAhead = c.Int(g + "course.windowAhead"),
                top = c.Num(g + "speed.top"), baseFrac = c.Num(g + "speed.baseFrac"), k = c.Num(g + "speed.k"), clampFrac = c.Num(g + "speed.clampFrac"), targetGap = c.Num(g + "speed.targetGap"), targetStep = c.Num(g + "speed.targetStep"),
                gapStart = c.Num(g + "gap.start"), windowAt = c.Num(g + "gap.windowAt"), windowTime = c.Num(g + "gap.windowTime"), missCost = c.Num(g + "gap.missCost"), failAt = c.Num(g + "gap.failAt"),
                armTime = c.Num(g + "gap.armTime"), hitBack = c.Num(g + "gap.hitBack"),
                laneMove = c.Num(g + "speed.laneMove"), laneEvery = c.Num(g + "speed.laneEvery"), introHold = c.Num(g + "speed.introHold"),
                tellLane = c.Num(g + "tells.lane"), tellRoller = c.Num(g + "tells.roller"), tellIcicle = c.Num(g + "tells.icicle"), resolveGap = c.Num(g + "tells.resolveGap"),
                life = c.Num(g + "attack.life"), atkAhead = c.Num(g + "attack.ahead"), atkDamage = c.Num(g + "attack.damage"), atkPush = c.Num(g + "attack.push"), thick = c.Num(g + "attack.thick"),
                sweepTop = c.Num(g + "attack.sweepTop"), lowBottom = c.Num(g + "attack.lowBottom"), lowTop = c.Num(g + "attack.lowTop"),
                rollerSpeed = c.Num(g + "attack.rollerSpeed"), rollerH = c.Num(g + "attack.rollerH"), rollerLife = c.Num(g + "attack.rollerLife"),
                icicleTop = c.Num(g + "attack.icicleTop"), icicleSafe = c.Num(g + "attack.icicleSafe"), icicleLife = c.Num(g + "attack.icicleLife"),
                every1 = c.Num(g + "attack.every1"), every2 = c.Num(g + "attack.every2"), every3 = c.Num(g + "attack.every3"),
                boxW = c.Num(g + "body.boxW"), boxH = c.Num(g + "body.boxH"), boxL = c.Num(g + "body.boxL"), dieTime = c.Num(g + "body.dieTime"), contactDamage = c.Num(g + "body.contactDamage"),
                refill = c.Num(g + "launch.refill"), assistReach = c.Num(g + "launch.reach"), assistCone = c.Num(g + "launch.coneDeg"), curveMaxDeg = c.Num(g + "curve.maxTurnDeg"), curveTiles = c.Num(g + "curve.tiles"), curveSalt = (uint)c.Num(g + "curve.salt"),
                capsuleHeight = c.Num("body.capsuleHeight"), capsuleRadius = c.Num("body.capsuleRadius"),
            };
            double[] lx = c.Nums(g + "course.laneX");
            for (int i = 0; i < 3; i++) p.laneX[i] = lx[i];
            return p;
        }
    }

    /// <summary>The curve of the course, VIEW data only (the sim stays straight). Tile k turns the heading by TurnDeg(k) degrees (|.| &lt;= maxTurnDeg, from the run seed and k; one
    /// Mulberry32 draw per tile, so a tile's turn never depends on how far you have been): the position and heading at any course distance s are an arc-length table of those
    /// circular arcs. Math only (JsMath); the sim never reads it. A point (x, s) of the straight course maps to Pose(s) + the left normal x.</summary>
    public sealed class Glacier27Curve
    {
        readonly uint seed;
        readonly double tileLen, maxDeg;
        readonly List<double> turn = new List<double>(), px = new List<double>(), pz = new List<double>(), yaw = new List<double>();

        public Glacier27Curve(uint runSeed, uint salt, double tileLen, double maxDeg)
        {
            seed = unchecked(runSeed ^ salt);
            this.tileLen = tileLen;
            this.maxDeg = maxDeg;
            px.Add(0); pz.Add(0); yaw.Add(0);
        }

        /// <summary>Tile k's heading change in degrees.</summary>
        public double TurnDeg(int k)
        {
            while (turn.Count <= k)
            {
                int i = turn.Count;
                var r = new Mulberry32(unchecked(seed + 7919u * (uint)(i + 1)));
                r.Next();
                double t = (r.Next() * 2 - 1) * maxDeg;
                if (i > 0 && Math.Abs(turn[i - 1] + t) > maxDeg * 1.5) t = -t; // an S, not a spiral
                turn.Add(t);
            }
            return turn[k];
        }

        void Extend(int k)
        {
            while (px.Count <= k)
            {
                int i = px.Count - 1;
                double d = TurnDeg(i) * Math.PI / 180, y0 = yaw[i];
                // heading 0 runs along -z (the course); the arc of tile i from its start pose
                double sx, sz;
                if (Math.Abs(d) < 1e-9) { sx = 0; sz = -tileLen; }
                else
                {
                    double R = tileLen / d;
                    sx = R * (1 - JsMath.Cos(d)) * -1; // local: left turn (d > 0) curves to -x
                    sz = -R * JsMath.Sin(d);
                }
                // rotate the local step by the start heading (yaw around +y; heading 0 looks -z)
                double c = JsMath.Cos(y0), s = JsMath.Sin(y0);
                px.Add(px[i] + sx * c + sz * s);
                pz.Add(pz[i] - sx * s + sz * c);
                yaw.Add(y0 + d);
            }
        }

        /// <summary>A point of the straight course (distance s along it, `lateral` m to the RIGHT of the centre line) on the bent course: the world offset (x, z) from the course's start and the
        /// heading there. Right of the course forward (-sin h, -cos h) is (cos h, -sin h) (+x when h = 0): the sim's convention, yaw 0 looks -z. The VIEW's camera yaw is the player's yaw PLUS this heading.</summary>
        public void Bend(double s, double lateral, out double x, out double z, out double heading)
        {
            Pose(s, out double cx, out double cz, out heading);
            x = cx + JsMath.Cos(heading) * lateral;
            z = cz - JsMath.Sin(heading) * lateral;
        }

        /// <summary>The pose at course distance s (m, 0 at the start): the world position of the centre line (x, z) and the heading (rad; 0 looks down -z; positive turns left).</summary>
        public void Pose(double s, out double x, out double z, out double heading)
        {
            if (s < 0) s = 0;
            int k = (int)Math.Floor(s / tileLen);
            Extend(k + 1);
            double u = s - k * tileLen, d = TurnDeg(k) * Math.PI / 180, y0 = yaw[k];
            double lx, lz;
            if (Math.Abs(d) < 1e-9) { lx = 0; lz = -u; }
            else
            {
                double R = tileLen / d, a = u / R;
                lx = -R * (1 - JsMath.Cos(a));
                lz = -R * JsMath.Sin(a);
            }
            double c = JsMath.Cos(y0), sn = JsMath.Sin(y0);
            x = px[k] + lx * c + lz * sn;
            z = pz[k] - lx * sn + lz * c;
            heading = y0 + (Math.Abs(d) < 1e-9 ? 0 : u / tileLen * d);
        }
    }

    public sealed class Glacier27Body : Enemy, IBossBody27
    {
        public readonly Glacier27 system;

        public Glacier27Body(Glacier27 system)
        {
            this.system = system;
            kind = "glacier";
        }

        public override double Height => system.p.boxH;
        public override double Radius => Math.Max(system.p.boxW, system.p.boxL) * 0.6;
        public override double AimHeight => system.p.boxH * 0.5;
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
        public double Filter(double amount, string weapon, Vec3 from, out bool blocked) => system.Filter(amount, weapon, out blocked);
        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt) { }
        public override void Stagger(double time, double dt) { }
        public override void Stun(double time, double dt) { }
    }

    /// <summary>A lane attack of the chase: low bar, sweep, roller, icicle fall.</summary>
    public sealed class Glacier27Obstacle
    {
        public string kind; // low, sweep, roller, icicle
        public double z, x0, x1, y0, y1;
        public int tellLeft, lifeLeft, total;
        public bool hit;
        public double rollX, rollDir; // roller: its x now and the way it rolls
        public bool Live => tellLeft <= 0 && lifeLeft > 0;
    }

    public struct Glacier27Log
    {
        public int step;
        public string name;
        public double seconds;
        public bool resolve;
    }

    public sealed class Glacier27 : IBossSystem
    {
        public readonly Glacier27Params p;
        public readonly Glacier27Body body;
        readonly EventBus events;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt0;
        readonly BossEvent said = new BossEvent();
        BossKit kit;
        BossScale scale = BossScale.One;
        Vec3 origin;
        public Vec3? arenaCenter;
        public readonly Fsm27 fsm;

        public WyrmBox box;
        public Vec3 prevCenter;
        public readonly List<Glacier27Obstacle> obstacles = new List<Glacier27Obstacle>();
        public readonly List<Glacier27Log> log = new List<Glacier27Log>();
        public readonly Dictionary<string, int> stats = new Dictionary<string, int>();
        static readonly string[] StatNames = { "attacks", "attackHits", "softFails", "blocked", "pulseHits", "windows", "misses", "laneChanges" };

        public string phase = "dead";
        public int stepNo, phaseSteps, hits;
        int beatSteps, dieSteps, lastResolve = -100000, atkLeft, atkIndex, laneLeft, introSteps, windowLeft, armLeft;
        double bossS, bossX, laneGoal;
        int lane = 1;
        public int checkpoint;
        public double gap, playerS;
        public Action<Vec3> teleport;
        public Func<Capsule> playerBody;
        public Glacier27Curve curve;

        double Dist(double worldZ) => origin.z - worldZ;

        public Glacier27(GameConfig cfg, Glacier27Params p, EventBus events, BossTarget target, Func<int> nextId)
        {
            this.p = p;
            this.events = events;
            this.target = target;
            this.nextId = nextId;
            dt0 = cfg.Dt;
            body = new Glacier27Body(this);
            foreach (string n in StatNames) stats[n] = 0;
            fsm = BuildFsm();
            events.On<Fist24FlickEvent>("fistFlick", e => LaunchLanded(target.position(), playerVelocity != null ? playerVelocity() : default));
            events.On("pulse27Blast", _ => LaunchLanded(target.position(), playerAim != null ? playerAim() : (playerVelocity != null ? playerVelocity() : default))); // NIGHT27 (the lead): the pulse27 LT blast is the launch; it is thrown along the aim
        }

        /// <summary>The player's velocity (the sim hands it in): a PULSE launch is read from the direction the player was thrown.</summary>
        public Func<Vec3> playerVelocity;
        /// <summary>The player's aim direction (the sim hands it in): the pulse27 blast launches along it.</summary>
        public Func<Vec3> playerAim;
        /// <summary>The PULSE's kill pips (builder W's PulseCharge27): a hit on the boss refills launch.refill of them, so the chase can be won with three launches (the pips only fill from kills, and there are none on the ice).</summary>
        public IPulseKillSink27 pulseSink;
        /// <summary>Is a PULSE launch ready right now (the sim hands it in; null: always)? The window opens only when it is: the PULSE-READY flash is the window.</summary>
        public Func<bool> pulseReady;

        /// <summary>A PULSE launch from `from` along `dir` (the way the player was thrown): it LANDS on the boss when the window is open, the boss is within launch.reach m and the throw is
        /// within launch.coneDeg of the line to it (the launch's own reach is short, so the window's 12 m gap is made fair by this lock-on: GG's rule 'a PULSE launch that lands counts').
        /// Builder W's blast can call this too (the lead wires it if the event name changes).</summary>
        public bool LaunchLanded(Vec3 from, Vec3 dir)
        {
            if (!Hurtable) return false;
            double dl = dir.Length;
            if (dl < 1e-6) return false;
            Vec3 to = box.c - new Vec3(from.x, from.y + 1.0, from.z);
            double d = to.Length;
            if (d > p.assistReach || d < 1e-6) return false;
            double cos = Vec3.Dot(to / d, dir / dl);
            if (cos < JsMath.Cos(p.assistCone * Math.PI / 180)) return false;
            Apply(body.maxHp / p.hitsToKill);
            return true;
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;
        public double BossS => bossS;
        public bool WindowOpen => windowLeft > 0;
        public double WindowFrac => windowLeft > 0 ? (double)windowLeft / S(p.windowTime) : 0;
        public Vec3 BossWorld => new Vec3(origin.x + bossX, origin.y, origin.z - bossS);
        public Vec3 CheckpointSpawn => new Vec3(origin.x + p.laneX[1], origin.y, origin.z - checkpoint);
        public int Phase => hits >= 2 ? 3 : hits == 1 ? 2 : 1;
        public uint RngState { get => fsm.rng.State; set => fsm.rng.State = value; }
        public void NewRun(uint seed)
        {
            fsm.rng = new Mulberry32(seed);
            curve = new Glacier27Curve(seed, p.curveSalt, p.tileLen, p.curveMaxDeg);
        }

        /// <summary>The boss's speed at this gap and phase: base + clamp(k x (gap - target), +-clampFrac x top), the target growing targetStep m per phase (it runs further ahead as the fight goes on);
        /// a player who has RUN PAST it (gap under 0) meets its fastest speed: it skates away, never waits behind you (pure: the harness asserts the clamp).</summary>
        public static double SpeedAt(Glacier27Params p, double gap, int phase = 1)
        {
            double lim = p.clampFrac * p.top;
            if (gap < 0) return p.baseFrac * p.top + lim;
            double d = p.k * (gap - (p.targetGap + (phase - 1) * p.targetStep));
            return p.baseFrac * p.top + Math.Max(-lim, Math.Min(lim, d));
        }

        public double CurrentSpeed => SpeedAt(p, gap, Phase);
        public bool Hurtable => body.alive && windowLeft > 0 && (phase == "p1" || phase == "p2" || phase == "p3");

        public void WindowFor(double s, out int first, out int last)
        {
            int k = (int)Math.Floor(Math.Max(0, s) / p.tileLen);
            first = Math.Max(0, k - p.winBack);
            last = k + p.winAhead;
        }

        // --- the machine: intro -> p1 -> (a hit) beat -> p2 -> beat -> p3 -> dying ---------------------------------------------------------

        string nextPhase;

        Fsm27 BuildFsm()
        {
            var top = new Fsm27("glacier") { dt = dt0 };
            top.Add("intro", enter: s => SetPhase("intro"), needsExitTime: true, canExit: s => s.steps >= introSteps);
            top.Add("p1", enter: s => { nextPhase = null; SetPhase("p1"); });
            top.Add("p2", enter: s => { nextPhase = null; SetPhase("p2"); });
            top.Add("p3", enter: s => { nextPhase = null; SetPhase("p3"); });
            top.Add("beat", enter: s => { SetPhase("beat"); beatSteps = kit.Beat("glacier", nextPhase == "p3" ? "PHASE 3" : "PHASE 2", new Vec3(body.position.x, body.position.y + 2, body.position.z)); obstacles.Clear(); atkLeft = S(p.every1); },
                needsExitTime: true, canExit: s => s.steps >= beatSteps);
            top.Add("dying", enter: s => { SetPhase("dying"); dieSteps = S(p.dieTime); obstacles.Clear(); windowLeft = 0; kit.Dying("glacier", body.position); });
            top.SetStart("intro");
            top.Transit("intro", "p1", () => true);
            top.Transit("p1", "beat", () => nextPhase == "p2" && body.alive);
            top.Transit("p2", "beat", () => nextPhase == "p3" && body.alive);
            top.Transit("beat", "p2", () => nextPhase == "p2");
            top.Transit("beat", "p3", () => nextPhase == "p3");
            top.Transit(null, "dying", () => body.alive && hits >= p.hitsToKill);
            return top;
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
            log.Add(new Glacier27Log { step = stepNo, name = name, seconds = seconds });
            kit.telegraph.Threat(at.x, at.y, at.z);
        }

        void Resolve(string name)
        {
            lastResolve = stepNo;
            log.Add(new Glacier27Log { step = stepNo, name = name, resolve = true });
        }

        public bool RestorePhaseStart()
        {
            if (!body.alive || phase == "dying" || phase == "dead") return false;
            string ph = phase == "beat" ? nextPhase : phase;
            if (ph != "p2" && ph != "p3") return false;
            hits = ph == "p3" ? 2 : 1;
            body.hp = body.maxHp * (1.0 - (double)hits / p.hitsToKill);
            ResetChase();
            nextPhase = null;
            fsm.Go(ph);
            return true;
        }

        void ResetChase()
        {
            obstacles.Clear();
            bossS = checkpoint + p.gapStart;
            bossX = p.laneX[1];
            lane = 1;
            atkLeft = S(p.every1);
            laneLeft = S(p.laneEvery);
            windowLeft = 0;
            armLeft = S(p.armTime);
            Pose();
            prevCenter = box.c;
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
            hits = 0;
            stepNo = 0;
            log.Clear();
            foreach (string n in StatNames) stats[n] = 0;
            lastResolve = -100000;
            checkpoint = 0;
            atkIndex = 0;
            nextPhase = null;
            ResetChase();
            body.position = BossWorld;
            body.previous = body.position;
            introSteps = Math.Max(kit.IntroSteps(), S(p.introHold));
            fsm.Reset();
            fsm.Enter();
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
            windowLeft = 0;
            fsm.Reset();
        }

        void Pose()
        {
            prevCenter = box.c;
            var b = default(WyrmBox);
            b.Orient(new Vec3(0, 0, -1));
            b.c = new Vec3(origin.x + bossX, origin.y + p.boxH * 0.5 + 0.2, origin.z - bossS);
            b.hr = p.boxW / 2; b.hu = p.boxH / 2; b.hf = p.boxL / 2;
            box = b;
        }

        public double Raycast(Vec3 o, Vec3 d)
        {
            if (!body.alive || phase == "dying" || phase == "dead") return -1;
            return box.Ray(o, d);
        }

        /// <summary>The hit rule: ONLY a gauntlet hit (the PULSE, swung or launched) inside the window lands, once per window; everything else is blocked (the HUD says why).</summary>
        public double Filter(double amount, string weapon, out bool blocked)
        {
            blocked = false;
            if (!Hurtable || weapon != "gauntlet")
            {
                blocked = true;
                stats["blocked"]++;
                return 0;
            }
            return body.maxHp / p.hitsToKill; // one hit is a third of the pool, whatever the swing's damage
        }

        public void Apply(double dmg)
        {
            if (!body.alive || windowLeft <= 0) return;
            hits++;
            stats["pulseHits"]++;
            windowLeft = 0;
            bossS += p.hitBack;
            armLeft = S(p.armTime);
            body.hp = Math.Max(0, body.maxHp * (1.0 - (double)hits / p.hitsToKill));
            body.flash = 0.08;
            if (hits < p.hitsToKill) pulseSink?.PulseKillFilled((int)p.refill);
            said.Reset("glacier");
            said.damage = JsMath.Round(dmg);
            said.part = "pulse";
            said.hp = body.hp;
            events.Emit("bossHit", said);
            if (hits < p.hitsToKill) nextPhase = hits == 1 ? "p2" : "p3";
        }

        public BossBar Bar() => new BossBar
        {
            valid = body.alive, id = body.id, name = p.name, phase = phase, hp = Math.Max(0, body.hp), max = body.maxHp, segment = 1.0 - 1.0 / p.hitsToKill,
            pips = p.hitsToKill, pipsOn = Math.Max(0, p.hitsToKill - hits),
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

        double EverySec => Phase >= 3 ? p.every3 : Phase == 2 ? p.every2 : p.every1;

        void SpawnAttack()
        {
            // P1: low, sweep; P2: low, sweep, roller; P3: sweep, roller, icicle (one new kind per phase)
            string[] kinds = Phase >= 3 ? new[] { "sweep", "roller", "icicle" } : Phase == 2 ? new[] { "low", "sweep", "roller" } : new[] { "low", "sweep", "low" };
            string kind = kinds[atkIndex % 3];
            atkIndex++;
            double tell = kind == "roller" ? p.tellRoller : kind == "icicle" ? p.tellIcicle : p.tellLane;
            double ahead = Math.Min(p.atkAhead, Math.Max(8, gap - 4));
            double zc = origin.z - (playerS + ahead);
            var o = new Glacier27Obstacle { kind = kind, z = zc, tellLeft = S(tell), lifeLeft = S(kind == "roller" ? p.rollerLife : kind == "icicle" ? p.icicleLife : p.life), total = S(tell) };
            double half = p.laneHalf;
            o.x0 = origin.x - half; o.x1 = origin.x + half;
            if (kind == "sweep") { o.y0 = 0; o.y1 = p.sweepTop; }
            else if (kind == "low") { o.y0 = p.lowBottom; o.y1 = p.lowTop; }
            else if (kind == "roller")
            {
                o.y0 = 0; o.y1 = p.rollerH;
                o.rollDir = fsm.Roll() < 0.5 ? -1 : 1;
                o.rollX = origin.x - o.rollDir * half;
                o.x0 = o.x1 = o.rollX;
            }
            else { o.y0 = 0; o.y1 = p.icicleTop; o.x0 = origin.x - (half - p.icicleSafe); o.x1 = origin.x + (half - p.icicleSafe); }
            obstacles.Add(o);
            Tell(kind, tell, new Vec3(origin.x, origin.y, zc));
            if (kind == "icicle") kit.telegraph.Line(o.x0, origin.y, zc, 1, 0, o.x1 - o.x0, p.thick * 2, tell);
            else if (kind == "roller") kit.telegraph.Line(origin.x - half, origin.y, zc, 1, 0, 2 * half, p.thick, tell);
            else kit.telegraph.Line(o.x0, origin.y, zc, 1, 0, o.x1 - o.x0, p.thick, tell);
        }

        bool AnyTelling()
        {
            foreach (Glacier27Obstacle o in obstacles) if (o.tellLeft > 0) return true;
            return false;
        }

        void StepObstacles(double dt)
        {
            Capsule c = PlayerCapsule();
            double feet = Math.Min(c.start.y, c.end.y) - c.radius - origin.y, top = Math.Max(c.start.y, c.end.y) + c.radius - origin.y;
            double px = c.start.x, pz = c.start.z;
            for (int i = obstacles.Count - 1; i >= 0; i--)
            {
                Glacier27Obstacle o = obstacles[i];
                if (o.tellLeft > 0)
                {
                    if (--o.tellLeft == 0) { Resolve(o.kind); stats["attacks"]++; }
                    continue;
                }
                if (--o.lifeLeft <= 0) { obstacles.RemoveAt(i); continue; }
                if (o.kind == "roller")
                {
                    o.rollX += o.rollDir * p.rollerSpeed * dt;
                    o.x0 = o.rollX - 1.0; o.x1 = o.rollX + 1.0;
                }
                if (o.hit || phase == "intro") continue;
                bool inZ = Math.Abs(pz - o.z) < p.thick * 0.5 + c.radius;
                bool inY = top > o.y0 && feet < o.y1;
                bool inX = px + c.radius > o.x0 && px - c.radius < o.x1;
                if (inZ && inY && inX)
                {
                    o.hit = true;
                    if (HurtPlayer(p.atkDamage))
                    {
                        stats["attackHits"]++;
                        target.impulse?.Invoke(0, 3, p.atkPush);
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
            fsm.Step();
            if (phase == "dying")
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
                bossS += SpeedAt(p, gap, Phase) * dt;
                if (--laneLeft <= 0)
                {
                    laneLeft = S(p.laneEvery);
                    int nl = fsm.RollInt(3);
                    if (nl != lane) { lane = nl; stats["laneChanges"]++; }
                    laneGoal = p.laneX[lane];
                }
                double dx = laneGoal - bossX;
                bossX += Math.Max(-p.laneMove * dt, Math.Min(p.laneMove * dt, dx));
                gap = bossS - playerS;
                // the window: the gap under windowAt opens it (a PULSE-ready flash); a window that runs out is a miss: the boss skates on, 20 m, never health
                if (windowLeft > 0)
                {
                    if (--windowLeft == 0)
                    {
                        bossS += p.missCost;
                        stats["misses"]++;
                        armLeft = S(p.armTime);
                        said.Reset("glacier");
                        said.name = "miss";
                        events.Emit("bossPunish", said);
                    }
                }
                else if (armLeft > 0) armLeft--;
                else if (gap <= p.windowAt && gap >= -1.5 && (pulseReady == null || pulseReady()))
                {
                    windowLeft = S(p.windowTime);
                    stats["windows"]++;
                    said.Reset("glacier");
                    said.name = "window";
                    said.seconds = p.windowTime;
                    events.Emit("bossWindow", said);
                }
                gap = bossS - playerS;
                if (--atkLeft <= 0 && stepNo - lastResolve >= S(p.resolveGap) && !AnyTelling())
                {
                    SpawnAttack();
                    atkLeft = S(EverySec);
                }
                else if (atkLeft < 0) atkLeft = 0;
                if (gap >= p.failAt)
                {
                    stats["softFails"]++;
                    said.Reset("glacier");
                    events.Emit("bossPunish", said);
                    ResetChase();
                    teleport?.Invoke(CheckpointSpawn);
                    playerS = checkpoint;
                    gap = bossS - playerS;
                }
                double cp = Math.Floor(playerS / (p.checkpointTiles * p.tileLen)) * p.checkpointTiles * p.tileLen;
                if (cp > checkpoint) checkpoint = (int)cp;
            }
            body.position = BossWorld;
            body.velocity = new Vec3(0, 0, -CurrentSpeed);
            Pose();
            if (phase == "p1" || phase == "p2" || phase == "p3") StepObstacles(dt);
        }
    }
}
