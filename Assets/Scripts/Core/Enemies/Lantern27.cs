// NIGHT27 builder B (boss1): THE LANTERN, level 1's boss (GG 2026-10-09: "the first boss fight is way too much like the Cerberus ... It needs to be original, unique, inspired
// and requires actual uses of our current gameplay mechanics and not just firing at it"). The verb is WALL RIDE; the weapon is any gun for the lit seconds and the PULSE to finish.
// A hovering lantern-box in the middle of a ring courtyard (Design/maps/lantern-arena.json: 32 m across, eight wall-ride segments, a roof with a skylight slot).
//   THE SHIELD   a plate turns to face the player (slowly: turnRate). The boss is hurt ONLY on a side face that looks at the wall segment the player is RIDING now
//                (Lantern27Shield.ShieldDown: a pure function of the face's wall id, the ridden wall id and the ride flag; tested in EditMode). A short grace (shieldGrace)
//                keeps the face lit for the shot already in the air. The boss never opens the window on a timer; a missed window costs time, never health; no stun-lock.
//   ATTACKS      slow orange orbs, each with a floor decal and a telegraph (tellFirst 0.8 s for the first attack of the fight, tellLater 0.6 s after): P1 FAN (three orbs
//                aimed at you), P2 RING (eight radial orbs at two heights: jump the low ones, slide the high ones), P3 RAIN (orbs fall on decals around you). Never two
//                attacks at once: rest -> tell -> fire -> recover.
//   PHASES       three, each with one new thing (never a new mechanic in the last): P1 the fan; P2 the ring, and the shield turns slower so a hop to the far lane
//                opens a window; P3 the rain, wall segments RETRACT in sequence (you chain rides around the ring) and five small SENTRIES fly in: their kills fill the PULSE's pips
//                (IPulseKillSink27: the teaching of the kill-charge). The last finisherAt of its HP is the CORE: guns land x finisherGunMult there, the PULSE x finisherPulseMult.
// The state machine is Core/Enemies/BossFsm27.cs (the UnityHFSM pattern). Fixed 120 Hz, timers in whole steps, Mulberry32, JsMath, no audio, no UnityEngine, no PhysX.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>The shield rule, pure: no scene, no state.</summary>
    public static class Lantern27Shield
    {
        /// <summary>The shield is down only on the face that looks at the wall segment the player is riding right now (wall id and ride state in, flag out).</summary>
        public static bool ShieldDown(int faceWallId, int ridingWallId, bool wallRiding) => wallRiding && ridingWallId >= 0 && faceWallId == ridingWallId;

        /// <summary>The wall segment (0..segments-1) a direction (dx, dz) from the lantern looks at: sector i holds atan2(dz, dx) in [i, i+1) x 2pi / segments, measured from +x.</summary>
        public static int SegmentAt(double dx, double dz, int segments)
        {
            if (dx == 0 && dz == 0) return -1;
            double ang = JsMath.Atan2(dz, dx);
            if (ang < 0) ang += 2 * Math.PI;
            int s = (int)Math.Floor(ang / (2 * Math.PI / segments));
            return s >= segments ? segments - 1 : s < 0 ? 0 : s;
        }

        /// <summary>The wall id face `face` (0 front, 1 right, 2 back, 3 left; a quarter turn each) looks at when the lantern's heading is `heading` rad.</summary>
        public static int FaceWall(int face, double heading, int segments)
        {
            double a = heading - face * (Math.PI / 2); // face 1 is the box's +right: forward turned a quarter clockwise seen from above (WyrmBox.Orient: r = up x f)
            return SegmentAt(JsMath.Cos(a), JsMath.Sin(a), segments);
        }
    }

    /// <summary>design.n27.boss1, read once.</summary>
    public sealed class Lantern27Params
    {
        public string name = "THE LANTERN";
        public double pool, phase2At, phase3At, finisherAt, finisherGunMult, finisherPulseMult;
        public double hoverY, half, turn1, turn2, turn3, dieTime, shieldGrace, arenaHalf, arenaHeight;
        public int segments;
        public double orbSpeed, orbRadius, orbDamage, orbLife, lowY, highY, tellFirst, tellLater, fanDeg, rainR, rainFall, rainHeight, rainSpread, recover, every1, every2, every3;
        public int fanCount, ringCount, rainCount;
        public double retractEvery, retractTell, retractTime;
        public int retractCount;
        public int sentries;
        public double sentryHp, sentryRadius, sentryOrbit, sentrySpeed, sentryLow, sentryHigh, introHold;
        public double capsuleHeight, capsuleRadius;

        public static Lantern27Params From(GameConfig c)
        {
            const string g = "design.n27.boss1.";
            return new Lantern27Params
            {
                name = c.Str(g + "name"),
                pool = c.Num(g + "hp.pool"), phase2At = c.Num(g + "hp.phase2At"), phase3At = c.Num(g + "hp.phase3At"), finisherAt = c.Num(g + "hp.finisherAt"),
                finisherGunMult = c.Num(g + "hp.finisherGunMult"), finisherPulseMult = c.Num(g + "hp.finisherPulseMult"),
                hoverY = c.Num(g + "body.hoverY"), half = c.Num(g + "body.half"), turn1 = c.Num(g + "body.turn1"), turn2 = c.Num(g + "body.turn2"), turn3 = c.Num(g + "body.turn3"),
                dieTime = c.Num(g + "body.dieTime"), shieldGrace = c.Num(g + "shield.grace"), segments = c.Int(g + "shield.segments"),
                arenaHalf = c.Num(g + "arena.half"), arenaHeight = c.Num(g + "arena.height"),
                orbSpeed = c.Num(g + "orb.speed"), orbRadius = c.Num(g + "orb.radius"), orbDamage = c.Num(g + "orb.damage"), orbLife = c.Num(g + "orb.life"),
                lowY = c.Num(g + "orb.lowY"), highY = c.Num(g + "orb.highY"), tellFirst = c.Num(g + "orb.tellFirst"), tellLater = c.Num(g + "orb.tellLater"),
                fanCount = c.Int(g + "orb.fanCount"), fanDeg = c.Num(g + "orb.fanDeg"), ringCount = c.Int(g + "orb.ringCount"),
                rainCount = c.Int(g + "orb.rainCount"), rainR = c.Num(g + "orb.rainR"), rainFall = c.Num(g + "orb.rainFall"), rainHeight = c.Num(g + "orb.rainHeight"), rainSpread = c.Num(g + "orb.rainSpread"),
                recover = c.Num(g + "orb.recover"), every1 = c.Num(g + "orb.every1"), every2 = c.Num(g + "orb.every2"), every3 = c.Num(g + "orb.every3"),
                retractEvery = c.Num(g + "walls.retractEvery"), retractTell = c.Num(g + "walls.retractTell"), retractTime = c.Num(g + "walls.retractTime"), retractCount = c.Int(g + "walls.retractCount"),
                sentries = c.Int(g + "sentry.count"), sentryHp = c.Num(g + "sentry.hp"), sentryRadius = c.Num(g + "sentry.radius"), sentryOrbit = c.Num(g + "sentry.orbit"),
                sentrySpeed = c.Num(g + "sentry.speed"), sentryLow = c.Num(g + "sentry.low"), sentryHigh = c.Num(g + "sentry.high"), introHold = c.Num(g + "body.introHold"),
                capsuleHeight = c.Num("body.capsuleHeight"), capsuleRadius = c.Num("body.capsuleRadius"),
            };
        }
    }

    public sealed class Lantern27Body : Enemy, IBossBody27
    {
        public readonly Lantern27 system;

        public Lantern27Body(Lantern27 system)
        {
            this.system = system;
            kind = "lantern";
        }

        public override double Height => system.p.half * 2;
        public override double Radius => system.p.half * 1.4;
        public override double AimHeight => 0;
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

    /// <summary>A small sentry (phase 3): hovers on a ring, dies to a shot or two; its kill fills a PULSE pip.</summary>
    public sealed class Sentry27 : Enemy
    {
        public double angle, orbit, height, bob;
        public bool counted;
        public double radius = 0.5;
        public Sentry27() { kind = "sentry"; }
        public override double Height => radius * 2;
        public override double Radius => radius;
        public override double AimHeight => 0;
        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            headshot = false;
            Vec3 oc = o - position;
            double b = Vec3.Dot(oc, d), c = Vec3.Dot(oc, oc) - radius * radius, disc = b * b - c;
            if (disc < 0) return -1;
            double t = -b - Math.Sqrt(disc);
            return t >= 0 ? t : (c <= 0 ? 0 : -1);
        }
    }

    /// <summary>A slow orb: lethal on touch, gone at a wall or the end of its life.</summary>
    public sealed class Lantern27Orb
    {
        public Vec3 pos, prev, vel;
        public double radius;
        public int life, total;
        public bool rain; // falls on a decal (bigger, red-orange core)
        public int kindId; // 0 fan, 1 ring-low, 2 ring-high, 3 rain
    }

    public struct Lantern27Log
    {
        public int step;
        public string name;
        public double seconds;
        public bool resolve;
    }

    public sealed class Lantern27 : IBossSystem
    {
        public readonly Lantern27Params p;
        public readonly Lantern27Body body;
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
        Fsm27 attacks;

        public readonly WyrmBox[] boxes = new WyrmBox[1];
        public readonly Vec3[] prevCenters = new Vec3[1];
        public readonly List<Lantern27Orb> orbs = new List<Lantern27Orb>();
        public readonly List<Sentry27> sentries = new List<Sentry27>();
        public readonly List<Lantern27Log> log = new List<Lantern27Log>();
        public readonly Dictionary<string, int> stats = new Dictionary<string, int>();
        static readonly string[] StatNames = { "attacks", "orbHits", "shieldBlocks", "litHits", "pulseHits", "retracts", "sentryKills", "windows" };

        /// <summary>The sim hands these in.</summary>
        public Func<bool> playerRiding;
        public Func<Capsule> playerBody;
        public Action<Enemy> addEnemy, removeEnemy;
        public Action walls; // the retracted mask changed: the sim rebuilds the wall layer
        /// <summary>NIGHT29 builder B: the reworked hit rule (null = NIGHT27's shield, bit for bit).</summary>
        public ILanternRule29 rule29;
        /// <summary>NIGHT31 B (design.n31.lantern31): the share of a hit (amount, weapon) that lands, set by the sim with the row on (null: rule29.HitScale()).</summary>
        public Func<double, string, double> scale31;
        /// <summary>NIGHT29: the shortest tell any attack gets with the rule on (design.n29.boss1.attackTellMinSeconds).</summary>
        public double tellMin29;
        public IPulseKillSink27 pulseSink;

        public string phase = "dead";
        public int stepNo, phaseSteps, retractedMask;
        public double heading = Math.PI / 2;
        public int litSeg = -1, ridingSeg = -1, hitFace = -1;
        int graceLeft, beatSteps, dieSteps, tellLeft, tellTotal, lastBlockSay = -1000, atkNo, lastPattern = -1, retractLeft, retractTellLeft, retractTimer, retractNext, introSteps, restLeft, recoverLeft, windowSteps;
        public int pendingTell; // the pattern in its telegraph (-1 none): the view draws its decal
        public int retractWarnMask;
        string nextPhase;
        string lastPart;
        readonly Vec3[] fanDir = new Vec3[8];
        readonly Vec3[] rainAt = new Vec3[8];
        double ringPhase;
        public int Phase => phase == "p3" || (phase == "beat" && nextPhase == "p3") ? 3 : phase == "p2" || (phase == "beat" && nextPhase == "p2") ? 2 : 1;

        public Lantern27(GameConfig cfg, Lantern27Params p, EventBus events, BossTarget target, Func<int> nextId)
        {
            this.p = p;
            this.events = events;
            this.target = target;
            this.nextId = nextId;
            dt0 = cfg.Dt;
            body = new Lantern27Body(this);
            foreach (string n in StatNames) stats[n] = 0;
            fsm = BuildFsm();
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;
        public bool Finisher => HpFrac <= p.finisherAt && body.alive;
        public Vec3 Centre => new Vec3(origin.x, origin.y + p.hoverY, origin.z);
        public Vec3 OriginWorld => origin;
        public uint RngState { get => fsm.rng.State; set => fsm.rng.State = value; }
        public void NewRun(uint seed) => fsm.rng = new Mulberry32(seed);
        public bool Hurtable => body.alive && phase != "intro" && phase != "beat" && phase != "dying" && phase != "dead";
        /// <summary>The wall segment whose face is lit now (-1: the shield is up everywhere).</summary>
        public int LitSegment => litSeg;
        public string Pattern => pendingTell == 0 ? "fan" : pendingTell == 1 ? "ring" : pendingTell == 2 ? "rain" : "";
        public double TellProgress => tellTotal > 0 && tellLeft > 0 ? 1.0 - (double)tellLeft / tellTotal : 0;
        public double Turn => Phase >= 3 ? p.turn3 : Phase == 2 ? p.turn2 : p.turn1;
        public Vec3 Forward => new Vec3(JsMath.Cos(heading), 0, JsMath.Sin(heading));

        // --- the machine ---------------------------------------------------------------------------------------------------------------

        Fsm27 BuildFsm()
        {
            var top = new Fsm27("lantern") { dt = dt0 };
            top.Add("intro", enter: s => { SetPhase("intro"); }, logic: s => { }, needsExitTime: true, canExit: s => s.steps >= introSteps);
            top.Add(PhaseMachine(1));
            top.Add(PhaseMachine(2));
            top.Add(PhaseMachine(3));
            top.Add("beat", enter: s => { SetPhase("beat"); beatSteps = kit.Beat("lantern", nextPhase == "p3" ? "PHASE 3" : "PHASE 2", new Vec3(body.position.x, body.position.y + 2, body.position.z)); orbs.Clear(); pendingTell = -1; },
                needsExitTime: true, canExit: s => s.steps >= beatSteps);
            top.Add("dying", enter: s => { SetPhase("dying"); dieSteps = S(p.dieTime); orbs.Clear(); pendingTell = -1; RemoveSentries(); kit.Dying("lantern", body.position); });
            top.SetStart("intro");
            top.Transit("intro", "p1", () => true);
            top.Transit("p1", "beat", () => nextPhase == "p2" && body.alive);
            top.Transit("p2", "beat", () => nextPhase == "p3" && body.alive);
            top.Transit("beat", "p2", () => nextPhase == "p2");
            top.Transit("beat", "p3", () => nextPhase == "p3");
            top.Transit(null, "dying", () => body.alive && body.hp <= 0);
            return top;
        }

        Fsm27 PhaseMachine(int n)
        {
            var m = new Fsm27("p" + n) { dt = dt0 };
            m.onEnter = s => { nextPhase = null; SetPhase("p" + n); if (n == 3) EnterP3(); restLeft = S(n == 3 ? p.every3 : n == 2 ? p.every2 : p.every1); };
            m.Add("rest", enter: s => { pendingTell = -1; restLeft = S(Phase >= 3 ? p.every3 : Phase == 2 ? p.every2 : p.every1); }, logic: s => { restLeft--; });
            m.Add("tell", enter: s => BeginTell(), logic: s => { tellLeft--; }, needsExitTime: true, canExit: s => tellLeft <= 0);
            m.Add("fire", enter: s => Fire());
            m.Add("recover", enter: s => { recoverLeft = S(p.recover); }, logic: s => { recoverLeft--; }, needsExitTime: true, canExit: s => recoverLeft <= 0);
            m.SetStart("rest");
            m.Transit("rest", "tell", () => restLeft <= 0);
            m.Transit("tell", "fire", () => tellLeft <= 0);
            m.Transit("fire", "recover", () => true);
            m.Transit("recover", "rest", () => recoverLeft <= 0);
            return m;
        }

        void SetPhase(string ph)
        {
            phase = ph;
            phaseSteps = 0;
            said.Reset("lantern");
            said.phase = ph;
            said.hp = body.hp;
            said.maxHp = body.maxHp;
            events.Emit("bossPhase", said);
        }

        // --- the attacks ---------------------------------------------------------------------------------------------------------------

        Vec3 ChestOf(Vec3 feet) => new Vec3(feet.x, feet.y + 1.0, feet.z);

        void BeginTell()
        {
            Vec3 pw = target.position();
            Vec3 c = Centre;
            int ph = Phase;
            int pat;
            do { pat = fsm.RollInt(ph); } while (ph > 1 && pat == lastPattern);
            // P1 only the fan; P2 fan or ring; P3 fan, ring or rain (never the same twice in a row)
            lastPattern = pat;
            pendingTell = pat;
            double sec = atkNo == 0 ? p.tellFirst : p.tellLater;
            if (rule29 != null) sec = Math.Max(sec, tellMin29); // NIGHT29: every attack tells at least tellMin29 s
            atkNo++;
            tellTotal = tellLeft = S(sec);
            string name = pat == 0 ? "fan" : pat == 1 ? "ring" : "rain";
            said.Reset("lantern");
            said.name = name;
            said.seconds = sec;
            events.Emit("bossTell", said);
            log.Add(new Lantern27Log { step = stepNo, name = name, seconds = sec });
            kit.telegraph.Threat(c.x, origin.y, c.z);
            double fy = origin.y;
            if (pat == 0)
            {
                double ax = pw.x - c.x, az = pw.z - c.z, len = JsMath.Hypot(ax, az);
                if (len < 1e-6) { ax = 1; az = 0; len = 1; }
                ax /= len; az /= len;
                double half = (p.fanCount - 1) * 0.5, step = p.fanDeg * Math.PI / 180;
                for (int i = 0; i < p.fanCount && i < fanDir.Length; i++)
                {
                    double a = (i - half) * step, ca = JsMath.Cos(a), sa = JsMath.Sin(a);
                    fanDir[i] = new Vec3(ax * ca - az * sa, 0, ax * sa + az * ca);
                    kit.telegraph.Line(c.x, fy, c.z, fanDir[i].x, fanDir[i].z, p.arenaHalf * 1.2, 0.9, sec);
                }
            }
            else if (pat == 1) kit.telegraph.Circle(c.x, fy, c.z, p.arenaHalf * 0.5, sec);
            else
            {
                for (int i = 0; i < p.rainCount && i < rainAt.Length; i++)
                {
                    double ang = fsm.Roll() * 2 * Math.PI, rad = i == 0 ? 0 : p.rainSpread * (0.4 + 0.6 * fsm.Roll());
                    double rx = pw.x + JsMath.Cos(ang) * rad, rz = pw.z + JsMath.Sin(ang) * rad;
                    double lim = p.arenaHalf - 1.5;
                    rx = Math.Max(origin.x - lim, Math.Min(origin.x + lim, rx));
                    rz = Math.Max(origin.z - lim, Math.Min(origin.z + lim, rz));
                    if (ringLimit30 > 0)
                    {
                        // NIGHT30 (design.n30.lantern30): the ring is round: a decal past ringLimit30 m from the centre comes in along its radius (0: NIGHT29's square clamp only)
                        double ox = rx - origin.x, oz = rz - origin.z, rr = JsMath.Hypot(ox, oz);
                        if (rr > ringLimit30) { rx = origin.x + ox * ringLimit30 / rr; rz = origin.z + oz * ringLimit30 / rr; }
                    }
                    rainAt[i] = new Vec3(rx, fy, rz);
                    kit.telegraph.Circle(rx, fy, rz, p.rainR, sec);
                }
            }
        }

        void Fire()
        {
            Resolve(pendingTell == 0 ? "fan" : pendingTell == 1 ? "ring" : "rain");
            stats["attacks"]++;
            Vec3 c = Centre;
            int pat = pendingTell;
            pendingTell = -1;
            if (pat == 0)
            {
                for (int i = 0; i < p.fanCount && i < fanDir.Length; i++)
                    SpawnOrb(c, fanDir[i] * p.orbSpeed, 0, false);
            }
            else if (pat == 1)
            {
                ringPhase += Math.PI / p.ringCount; // the second ring is offset: a gap in one is an orb in the other
                for (int h = 0; h < 2; h++)
                    for (int i = 0; i < p.ringCount; i++)
                    {
                        double a = ringPhase * h + i * 2 * Math.PI / p.ringCount;
                        Vec3 at = new Vec3(c.x, origin.y + (h == 0 ? p.lowY : p.highY), c.z);
                        SpawnOrb(at, new Vec3(JsMath.Cos(a), 0, JsMath.Sin(a)) * p.orbSpeed, h == 0 ? 1 : 2, false);
                    }
            }
            else
            {
                for (int i = 0; i < p.rainCount && i < rainAt.Length; i++)
                    SpawnOrb(new Vec3(rainAt[i].x, origin.y + p.rainHeight, rainAt[i].z), new Vec3(0, -p.rainFall, 0), 3, true);
            }
        }

        void SpawnOrb(Vec3 at, Vec3 v, int kindId, bool rain)
        {
            orbs.Add(new Lantern27Orb { pos = at, prev = at, vel = v, radius = rain ? p.rainR * 0.6 : p.orbRadius, life = S(p.orbLife), total = S(p.orbLife), rain = rain, kindId = kindId });
        }

        void Resolve(string name) => log.Add(new Lantern27Log { step = stepNo, name = name, resolve = true });

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

        static double SegDist(Vec3 pt, Vec3 a, Vec3 b)
        {
            Vec3 ab = b - a, ap = pt - a;
            double len2 = Vec3.Dot(ab, ab), t = len2 < 1e-12 ? 0 : Math.Max(0, Math.Min(1, Vec3.Dot(ap, ab) / len2));
            Vec3 q = a + ab * t;
            return Vec3.DistanceTo(pt, q);
        }

        void StepOrbs(double dt)
        {
            Capsule c = PlayerCapsule();
            double lim = p.arenaHalf + 0.5;
            for (int i = orbs.Count - 1; i >= 0; i--)
            {
                Lantern27Orb o = orbs[i];
                o.prev = o.pos;
                o.pos = o.pos + o.vel * dt;
                bool dead = --o.life <= 0 || Math.Abs(o.pos.x - origin.x) > lim || Math.Abs(o.pos.z - origin.z) > lim || o.pos.y < origin.y + (o.rain ? 0.3 : 0.1);
                if (phase != "intro" && SegDist(o.pos, c.start, c.end) < o.radius + c.radius)
                {
                    if (HurtPlayer(p.orbDamage)) stats["orbHits"]++;
                    dead = true;
                }
                if (dead) orbs.RemoveAt(i);
            }
        }

        // --- walls and sentries ---------------------------------------------------------------------------------------------------------

        void EnterP3()
        {
            retractTimer = S(p.retractEvery);
            retractLeft = 0;
            retractTellLeft = 0;
            retractedMask = 0;
            retractWarnMask = 0;
            SpawnSentries();
        }

        void SpawnSentries()
        {
            RemoveSentries();
            Vec3 c = Centre;
            for (int i = 0; i < p.sentries; i++)
            {
                var s = new Sentry27
                {
                    id = nextId(), alive = true, hp = p.sentryHp, maxHp = p.sentryHp, radius = p.sentryRadius,
                    angle = i * 2 * Math.PI / p.sentries, orbit = p.sentryOrbit, height = p.sentryLow + (p.sentryHigh - p.sentryLow) * (i % 2), bob = i,
                };
                s.position = new Vec3(c.x + JsMath.Cos(s.angle) * s.orbit, origin.y + s.height, c.z + JsMath.Sin(s.angle) * s.orbit);
                s.previous = s.position;
                sentries.Add(s);
                addEnemy?.Invoke(s);
            }
        }

        void RemoveSentries()
        {
            foreach (Sentry27 s in sentries) { s.alive = false; removeEnemy?.Invoke(s); }
            sentries.Clear();
        }

        void StepSentries(double dt)
        {
            Vec3 c = Centre;
            for (int i = sentries.Count - 1; i >= 0; i--)
            {
                Sentry27 s = sentries[i];
                s.previous = s.position;
                if (!s.alive)
                {
                    if (!s.counted)
                    {
                        s.counted = true;
                        stats["sentryKills"]++;
                        said.Reset("lantern");
                        said.part = "sentry";
                        said.reason = "kill";
                        said.position = s.position;
                        events.Emit("bossPart", said);
                        pulseSink?.PulseKillFilled(1);
                    }
                    removeEnemy?.Invoke(s);
                    sentries.RemoveAt(i);
                    continue;
                }
                s.angle += p.sentrySpeed * dt;
                s.position = new Vec3(c.x + JsMath.Cos(s.angle) * s.orbit, origin.y + s.height + 0.4 * JsMath.Sin(s.angle * 2 + s.bob), c.z + JsMath.Sin(s.angle) * s.orbit);
            }
        }

        /// <summary>NIGHT30 (design.n30.lantern30): the ring has no retracting segments; true stops phase 3's retract cycle (false: NIGHT29, bit for bit).</summary>
        public bool noRetract30;
        /// <summary>NIGHT30 (design.n30.lantern30): the rain decals stay within this many metres of the centre (the ring's inside); 0: NIGHT29's square clamp only.</summary>
        public double ringLimit30;

        void StepWalls()
        {
            if (noRetract30) return;
            if (Phase < 3 || phase == "beat" || phase == "dying") return;
            if (retractTellLeft > 0)
            {
                if (--retractTellLeft == 0)
                {
                    retractedMask = retractWarnMask;
                    retractWarnMask = 0;
                    retractLeft = S(p.retractTime);
                    stats["retracts"]++;
                    walls?.Invoke();
                }
            }
            else if (retractLeft > 0)
            {
                if (--retractLeft == 0)
                {
                    retractedMask = 0;
                    retractTimer = S(p.retractEvery);
                    walls?.Invoke();
                }
            }
            else if (--retractTimer <= 0)
            {
                // the next pair goes round the ring one way (a sequence you can chain rides along), never under the segment the player rides
                int start = retractNext % p.segments;
                retractNext += p.retractCount + 1;
                int mask = 0;
                for (int i = 0; i < p.retractCount; i++) mask |= 1 << ((start + i) % p.segments);
                if (ridingSeg >= 0 && (mask & (1 << ridingSeg)) != 0) { retractTimer = S(0.5); retractNext -= p.retractCount + 1; return; }
                retractWarnMask = mask;
                retractTellLeft = S(p.retractTell);
                said.Reset("lantern");
                said.name = "walls";
                said.seconds = p.retractTell;
                events.Emit("bossTell", said);
                log.Add(new Lantern27Log { step = stepNo, name = "walls", seconds = p.retractTell });
            }
        }

        // --- hits ----------------------------------------------------------------------------------------------------------------------

        public bool RestorePhaseStart()
        {
            if (!body.alive || phase == "dying" || phase == "dead") return false;
            string ph = phase == "beat" ? nextPhase : phase;
            if (ph != "p2" && ph != "p3") return false;
            body.hp = body.maxHp * (ph == "p3" ? p.phase3At : p.phase2At);
            nextPhase = null;
            orbs.Clear();
            pendingTell = -1;
            retractedMask = 0;
            walls?.Invoke();
            fsm.Go(ph);
            return true;
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
            orbs.Clear();
            RemoveSentries();
            nextPhase = null;
            atkNo = 0;
            lastPattern = -1;
            litSeg = ridingSeg = -1;
            graceLeft = 0;
            retractedMask = retractWarnMask = 0;
            retractNext = 0;
            ringPhase = 0;
            heading = Math.PI / 2; // facing +z: the player enters from the south (+z)
            introSteps = Math.Max(kit.IntroSteps(), S(p.introHold));
            body.position = Centre;
            body.previous = body.position;
            Pose();
            prevCenters[0] = boxes[0].c;
            fsm.Reset();
            fsm.Enter();
            said.Reset("lantern");
            said.name = "LANTERN";
            said.hp = body.maxHp;
            events.Emit("bossRise", said);
        }

        public void Clear()
        {
            body.alive = false;
            phase = "dead";
            orbs.Clear();
            RemoveSentries();
            retractedMask = 0;
            fsm.Reset();
        }

        void Pose()
        {
            prevCenters[0] = boxes[0].c;
            var b = default(WyrmBox);
            b.Orient(Forward);
            b.c = Centre;
            b.hr = b.hu = b.hf = p.half;
            boxes[0] = b;
        }

        /// <summary>The first hit on the box: the distance along the ray (-1: a miss); remembers which face it entered (0 front, 1 right, 2 back, 3 left, 4 top or bottom).</summary>
        public double Raycast(Vec3 o, Vec3 d)
        {
            if (!body.alive || phase == "dying" || phase == "dead") return -1;
            double t = boxes[0].Ray(o, d);
            if (t < 0) { hitFace = -1; return -1; }
            Vec3 at = o + d * t - boxes[0].c;
            double lf = Vec3.Dot(at, boxes[0].f), lr = Vec3.Dot(at, boxes[0].r), lu = Vec3.Dot(at, boxes[0].u);
            double af = Math.Abs(lf), ar = Math.Abs(lr), au = Math.Abs(lu);
            if (au >= af && au >= ar) hitFace = 4;
            else if (af >= ar) hitFace = lf >= 0 ? 0 : 2;
            else hitFace = lr >= 0 ? 1 : 3;
            return t;
        }

        /// <summary>The wall id face `face` looks at now.</summary>
        public int FaceWall(int face) => Lantern27Shield.FaceWall(face, heading, p.segments);

        /// <summary>Is the shield down on `face` now (the pure rule, with the grace).</summary>
        public bool ShieldDownOn(int face) => face >= 0 && face < 4 && Lantern27Shield.ShieldDown(FaceWall(face), litSeg, litSeg >= 0);

        /// <summary>The damage that lands (blocked: the shield is up, or the fight is in its intro, beat or finisher).</summary>
        public double Filter(double amount, string weapon, out bool blocked)
        {
            blocked = false;
            int face = hitFace;
            hitFace = -1;
            if (!Hurtable) { blocked = true; return 0; }
            if (rule29 != null)
            {
                // NIGHT29 builder B (design.n29.boss1): any gun anywhere lands lightDamage of a hit; full only with the core exposed and the shooter wall riding (Lantern29.HitScale)
                stats["litHits"]++;
                bool pulse29 = weapon == "gauntlet";
                if (pulse29) stats["pulseHits"]++;
                double m29 = 1;
                if (Finisher) m29 = pulse29 ? p.finisherPulseMult : p.finisherGunMult;
                lastPart = Finisher ? "core" : "face";
                return amount * m29 * (scale31 != null ? scale31(amount, weapon) : rule29.HitScale()); // NIGHT31 B (design.n31.lantern31): the shield rule replaces the hit share when the sim sets it
            }
            if (!ShieldDownOn(face))
            {
                blocked = true;
                stats["shieldBlocks"]++;
                if (stepNo - lastBlockSay > 30)
                {
                    lastBlockSay = stepNo;
                    said.Reset("lantern");
                    said.name = "shield";
                    events.Emit("bossShield", said);
                }
                return 0;
            }
            stats["litHits"]++;
            bool pulse = weapon == "gauntlet";
            if (pulse) stats["pulseHits"]++;
            double m = 1;
            if (Finisher) m = pulse ? p.finisherPulseMult : p.finisherGunMult;
            lastPart = Finisher ? "core" : "face";
            return amount * m;
        }

        public void Apply(double dmg)
        {
            if (!body.alive) return;
            body.hp -= dmg;
            body.flash = 0.06;
            said.Reset("lantern");
            said.damage = JsMath.Round(dmg);
            said.part = lastPart ?? "face";
            said.core = Finisher;
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            if (nextPhase == null)
            {
                if (phase == "p1" && HpFrac <= p.phase2At) { body.hp = body.maxHp * p.phase2At; nextPhase = "p2"; }
                else if (phase == "p2" && HpFrac <= p.phase3At) { body.hp = body.maxHp * p.phase3At; nextPhase = "p3"; }
            }
            if (body.hp <= 0) body.hp = 0;
        }

        public BossBar Bar() => new BossBar
        {
            valid = body.alive, id = body.id, name = p.name, phase = phase, hp = Math.Max(0, body.hp), max = body.maxHp, segment = p.phase2At,
            pips = p.sentries, pipsOn = sentries.Count,
        };

        // --- the step --------------------------------------------------------------------------------------------------------------------

        public void Step(double dt)
        {
            if (!body.alive) return;
            body.previous = body.position;
            if (Frozen) return;
            stepNo++;
            phaseSteps++;
            Vec3 pw = target.position();
            Vec3 c = Centre;
            // which wall is the player riding, and is the face looking at it lit
            bool riding = playerRiding != null && playerRiding();
            ridingSeg = riding ? Lantern27Shield.SegmentAt(pw.x - c.x, pw.z - c.z, p.segments) : -1;
            if (riding && ridingSeg >= 0) { litSeg = ridingSeg; graceLeft = S(p.shieldGrace); }
            else if (graceLeft > 0 && --graceLeft == 0) litSeg = -1;
            else if (graceLeft <= 0) litSeg = -1;
            // the plate turns to the player
            if (phase != "dying" && phase != "dead")
            {
                double want = JsMath.Atan2(pw.z - c.z, pw.x - c.x), dh = want - heading;
                while (dh > Math.PI) dh -= 2 * Math.PI;
                while (dh < -Math.PI) dh += 2 * Math.PI;
                double stp = Turn * dt;
                heading += Math.Max(-stp, Math.Min(stp, dh));
                if (heading > Math.PI) heading -= 2 * Math.PI;
                if (heading < -Math.PI) heading += 2 * Math.PI;
            }
            fsm.Step();
            if (phase == "dying")
            {
                if (--dieSteps <= 0)
                {
                    body.alive = false;
                    phase = "dead";
                    kit.Down("lantern", body.id, body.position);
                    return;
                }
            }
            if (phase == "p1" || phase == "p2" || phase == "p3")
            {
                StepOrbs(dt);
                StepWalls();
                StepSentries(dt);
            }
            body.position = Centre;
            body.velocity = default;
            Pose();
        }
    }
}
