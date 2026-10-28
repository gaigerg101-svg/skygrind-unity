// NIGHT26 (builder B, contract hulk): THE SLAG HULK, level 1's EASY boss (GG 2026-10-08: "the worm is too hard for a first boss ... add an easier boss
// first"). A slow walking bruiser built from oriented boxes (Design/bosses/hulk.json: head, torso, back plate, arms, legs), one shared HP pool of 1500
// (design.n26.bosses.hulk.hp), three phases at 66 / 33 % with a phase-start checkpoint like the worm's (RestorePhaseStart), and exactly two attacks:
//   SLAM    a 0.8 s tell (a circle at its feet), then a shockwave RING runs out across the floor; it is 1 m tall: jump it. Phase 3 sends a second ring 0.9 s behind.
//   CHARGE  a 0.8 s tell (a line to the wall it will hit, locked when the tell starts), then a straight fast run; it stops at a wall or a pillar, STUNNED
//           for 2 s: hittable (x stunMult).
// The weak points: the back plate and the head x2, the rest x1. Every tell is >= stepsFor(0.5 s) and no two lethal attacks resolve inside tells.resolveGap
// (0.5 s): the cycle is slam, charge, slam, charge with a walk between, and the log proves it. Fixed 120 Hz, timers in whole steps (stepsFor), no PhysX, no
// audio, no UnityEngine; sin / cos / atan2 through JsMath. A new module (the web has no hulk).
// ENTRY API (SurvivalSim.Hulk26.cs; Wyrm.cs is the template):
//   var hulk = new Hulk(cfg, HulkParams.From(cfg, Hulk.LoadBoss(path)), events, world, target, nextId);  hulk.arena = arena;
//   hulk.NewRun(seed); kit.bosses["hulk"] = hulk; kit.Start("hulk", 0, 0, null, hulk.p.name); hulk.Step(dt) every fixed step; hulk.Raycast / hulk.Filter.
// Events (the web's names): bossRise, bossPhase, bossTell {name, seconds}, bossHit {damage, part, hp}, bossStun {seconds}; the kit's bossIntro, bossBeat,
// bossDying, bossDown.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class HulkPartDef
    {
        public string name, part;
        public double a, b, c, hr, hu, hf;
    }

    /// <summary>design.n26.bosses.hulk and Design/bosses/hulk.json, read once.</summary>
    public sealed class HulkParams
    {
        public string name = "SLAG HULK";
        public double pool, phase2At, phase3At, bodyMult, headMult, backMult, stunMult, shellFront, guardHead;
        public double walk, phaseMult, turnRate, standoff;
        public double slamTell, slamRing, slamRingMax, slamRingThick, slamRingHeight, slamDamage, slamRecover, slamRange, slamTellR, ring2Delay;
        public double chargeTell, chargeSpeed, chargeDamage, chargeWidth, chargeMax, chargeStun, chargeRecover, chargeRadius, chargePush;
        public double walkGap1, walkGap2, walkGap3, resolveGap;
        public double contactBody, contactCooldown, push, pushUp, footRadius, dieTime, spawnZ;
        public double capsuleHeight, capsuleRadius;
        public readonly List<HulkPartDef> parts = new List<HulkPartDef>();

        public static HulkParams From(GameConfig c, Dictionary<string, object> boss)
        {
            const string h = "design.n26.bosses.hulk.";
            var p = new HulkParams
            {
                name = c.Str(h + "name"),
                pool = c.Num(h + "hp.pool"), phase2At = c.Num(h + "hp.phase2At"), phase3At = c.Num(h + "hp.phase3At"), bodyMult = c.Num(h + "hp.bodyMult"),
                headMult = c.Num(h + "hp.headMult"), backMult = c.Num(h + "hp.backMult"), stunMult = c.Num(h + "hp.stunMult"), shellFront = c.Num(h + "hp.shellFront"), guardHead = c.Num(h + "hp.guardHead"),
                walk = c.Num(h + "move.walk"), phaseMult = c.Num(h + "move.phaseMult"), turnRate = c.Num(h + "move.turnRate"), standoff = c.Num(h + "move.standoff"),
                slamTell = c.Num(h + "slam.tell"), slamRing = c.Num(h + "slam.ringSpeed"), slamRingMax = c.Num(h + "slam.ringMax"), slamRingThick = c.Num(h + "slam.ringThick"),
                slamRingHeight = c.Num(h + "slam.ringHeight"), slamDamage = c.Num(h + "slam.damage"), slamRecover = c.Num(h + "slam.recover"), slamRange = c.Num(h + "slam.range"),
                slamTellR = c.Num(h + "slam.tellRadius"), ring2Delay = c.Num(h + "slam.ring2Delay"),
                chargeTell = c.Num(h + "charge.tell"), chargeSpeed = c.Num(h + "charge.speed"), chargeDamage = c.Num(h + "charge.damage"), chargeWidth = c.Num(h + "charge.width"),
                chargeMax = c.Num(h + "charge.maxLength"), chargeStun = c.Num(h + "charge.stun"), chargeRecover = c.Num(h + "charge.recover"), chargeRadius = c.Num(h + "charge.radius"),
                chargePush = c.Num(h + "charge.push"),
                walkGap1 = c.Num(h + "cycle.walk1"), walkGap2 = c.Num(h + "cycle.walk2"), walkGap3 = c.Num(h + "cycle.walk3"), resolveGap = c.Num(h + "tells.resolveGap"),
                contactBody = c.Num(h + "contact.damage"), contactCooldown = c.Num(h + "contact.cooldown"), push = c.Num(h + "contact.push"), pushUp = c.Num(h + "contact.pushUp"),
                footRadius = c.Num(h + "body.footRadius"), dieTime = c.Num(h + "body.dieTime"), spawnZ = c.Num(h + "body.spawnZ"),
                capsuleHeight = c.Num("body.capsuleHeight"), capsuleRadius = c.Num("body.capsuleRadius"),
            };
            foreach (object o in PortData.Arr(boss["parts"]))
            {
                var j = PortData.Obj(o);
                List<object> half = PortData.Arr(j["half"]);
                p.parts.Add(new HulkPartDef
                {
                    name = (string)j["name"], part = (string)j["part"], a = PortData.Num(j["a"]), b = PortData.Num(j["b"]), c = PortData.Num(j["c"]),
                    hr = PortData.Num(half[0]), hu = PortData.Num(half[1]), hf = PortData.Num(half[2]),
                });
            }
            return p;
        }
    }

    /// <summary>The boss as the enemies' registry sees it: the shots' target.</summary>
    public sealed class HulkBody : Enemy
    {
        public readonly Hulk system;

        public HulkBody(Hulk system)
        {
            this.system = system;
            kind = "hulk";
        }

        public override double Height => 4.4;
        public override double Radius => 1.5;
        public override double AimHeight => 2.5;

        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            headshot = false; // the part multipliers are Hulk.Filter's (head x2, back x2): the revolver's own headshot rule must not double them
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

    /// <summary>A slam's shockwave ring.</summary>
    public sealed class HulkRing
    {
        public double r;
        public bool hit;
    }

    /// <summary>A telegraph marker for the view: the slam's circle, the charge's line.</summary>
    public sealed class HulkMarker
    {
        public string kind;
        public Vec3 pos, to;
        public int left, total;
    }

    public struct HulkLog
    {
        public int step;
        public string name;
        public double seconds;
        public bool resolve;
    }

    public sealed class Hulk : IBossSystem
    {
        public readonly HulkParams p;
        public readonly HulkBody body;
        readonly EventBus events;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt0;
        readonly BossEvent said = new BossEvent();
        Mulberry32 rng = new Mulberry32(1);
        BossKit kit;
        BossScale scale = BossScale.One;
        Vec3 center;

        /// <summary>The room (the walls and pillars the hulk is kept inside and its charge stops at).</summary>
        public WyrmArena arena;
        /// <summary>Where the room's origin sits in the world.</summary>
        public Vec3? arenaCenter;
        double[] solids = new double[0]; // minx, minz, maxx, maxz, maxy per solid (arena-local)

        public readonly WyrmBox[] boxes;
        public readonly string[] partOf;
        public readonly Vec3[] prevCenters;
        public readonly List<HulkRing> rings = new List<HulkRing>();
        public readonly List<HulkMarker> markers = new List<HulkMarker>();
        public readonly List<HulkLog> log = new List<HulkLog>();
        public readonly Dictionary<string, int> stats = new Dictionary<string, int>();
        static readonly string[] StatNames = { "slams", "charges", "stuns", "slamHits", "chargeHits", "contacts", "headHits", "backHits", "bodyHits", "stunHits" };

        public string phase = "dead";
        /// <summary>walk, slamTell, slamRecover, chargeTell, charge, stun, recover (the view reads it).</summary>
        public string state = "walk";
        string nextPhase, hitPart, lastPart;
        public int stepNo, phaseSteps, stateSteps, contactLeft;
        int beatSteps, dieSteps, lastResolve = -100000, cycleIndex, ring2Left, walkLeft, tellSteps, stunLeft;
        double ang, chargeLen, chargeDone;
        Vec3 posL, chargeDir;
        bool chargeHit;
        public Func<Capsule> playerBody;

        public Hulk(GameConfig cfg, HulkParams p, EventBus events, ICollisionWorld world, BossTarget target, Func<int> nextId)
        {
            this.p = p;
            this.events = events;
            this.target = target;
            this.nextId = nextId;
            dt0 = cfg.Dt;
            body = new HulkBody(this);
            boxes = new WyrmBox[p.parts.Count];
            partOf = new string[p.parts.Count];
            prevCenters = new Vec3[p.parts.Count];
            for (int i = 0; i < p.parts.Count; i++) partOf[i] = p.parts[i].part;
            foreach (string n in StatNames) stats[n] = 0;
        }

        public static Dictionary<string, object> LoadBoss(string path) => PortData.Obj(PortData.LoadJson(path));

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public Vec3 Center => center;
        public Vec3 HeadPosition => boxes[HeadIndex].c;
        public int PartCount => boxes.Length;
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;
        public bool Stunned => state == "stun";
        public int StunSteps => stunLeft;
        public double Yaw => ang;
        public Vec3 FeetWorld => ToWorld(posL);
        public bool Charging => state == "charge";
        /// <summary>The locked charge line the telegraph shows (arena-world direction, length m).</summary>
        public Vec3 ChargeDir => chargeDir;
        public double ChargeLen => chargeLen;
        public double TellFrac => tellSteps > 0 ? 1.0 - (double)stateSteps / tellSteps : 0;
        static int PhaseNo(string ph) => ph == "p2" ? 2 : ph == "p3" ? 3 : 1;
        int EffPhase => phase == "beat" ? PhaseNo(nextPhase) : PhaseNo(phase);
        public double PhaseSpeed => EffPhase >= 3 ? p.phaseMult * p.phaseMult : EffPhase == 2 ? p.phaseMult : 1;
        int HeadIndex
        {
            get
            {
                for (int i = 0; i < partOf.Length; i++) if (partOf[i] == "head") return i;
                return 0;
            }
        }

        public void NewRun(uint seed) => rng = new Mulberry32(seed);

        public uint RngState
        {
            get => rng.State;
            set => rng.State = value;
        }

        /// <summary>"p2" or "p3" when a death inside the room goes back to that phase's start; null in the intro and phase 1.</summary>
        public string RetryPhase => !body.alive ? null : phase == "beat" ? nextPhase : phase == "p2" || phase == "p3" ? phase : null;

        /// <summary>A player death inside the room: the hulk goes back to the START of its current phase (HP at the threshold, rings and marks cleared, a walk first).</summary>
        public bool RestorePhaseStart()
        {
            string ph = RetryPhase;
            if (ph == null) return false;
            body.hp = body.maxHp * (ph == "p3" ? p.phase3At : p.phase2At);
            rings.Clear();
            markers.Clear();
            ring2Left = 0;
            contactLeft = S(p.contactCooldown);
            if (phase != ph) SetPhase(ph);
            StartWalk(1.5);
            return true;
        }

        Vec3 ToWorld(Vec3 v) => new Vec3(v.x + center.x, v.y + center.y, v.z + center.z);

        void SetPhase(string ph)
        {
            phase = ph;
            phaseSteps = 0;
            said.Reset("hulk");
            said.phase = ph;
            said.hp = body.hp;
            said.maxHp = body.maxHp;
            events.Emit("bossPhase", said);
        }

        void Tell(string name, double seconds, Vec3 at)
        {
            said.Reset("hulk");
            said.name = name;
            said.seconds = seconds;
            events.Emit("bossTell", said);
            log.Add(new HulkLog { step = stepNo, name = name, seconds = seconds, resolve = false });
            kit.telegraph.Threat(at.x, at.y, at.z);
        }

        void Resolve(string name)
        {
            lastResolve = stepNo;
            log.Add(new HulkLog { step = stepNo, name = name, seconds = 0, resolve = true });
        }

        public void StartFight(BossFight ctx)
        {
            kit = ctx.kit;
            scale = ctx.scale;
            center = arenaCenter ?? ctx.center;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(p.pool * ctx.scale.hp);
            body.hp = body.maxHp;
            body.flash = 0;
            stepNo = 0;
            rings.Clear();
            markers.Clear();
            log.Clear();
            foreach (string n in StatNames) stats[n] = 0;
            contactLeft = 0;
            lastResolve = -100000;
            cycleIndex = 0;
            ring2Left = 0;
            BuildSolids();
            posL = new Vec3(0, 0, p.spawnZ);
            ang = 0; // faces +z, toward the player's spawn
            state = "walk";
            walkLeft = S(1.0);
            body.position = ToWorld(posL);
            body.previous = body.position;
            body.velocity = default;
            Pose();
            for (int i = 0; i < boxes.Length; i++) prevCenters[i] = boxes[i].c;
            SetPhase("intro");
            said.Reset("hulk");
            said.name = "HULK";
            said.hp = body.maxHp;
            said.round = ctx.round;
            events.Emit("bossRise", said);
        }

        public void Clear()
        {
            body.alive = false;
            phase = "dead";
            rings.Clear();
            markers.Clear();
        }

        void BuildSolids()
        {
            var l = new List<double>();
            if (arena != null)
                foreach (var (min, max, kind) in arena.boxes)
                {
                    if (kind == "floor" || kind == "ceiling") continue;
                    l.Add(min.x); l.Add(min.z); l.Add(max.x); l.Add(max.z); l.Add(max.y);
                }
            solids = l.ToArray();
        }

        /// <summary>Is a footprint circle at (x, z) (arena-local) inside a wall or a pillar?</summary>
        bool Blocked(double x, double z, double rad)
        {
            for (int i = 0; i < solids.Length; i += 5)
            {
                double cx = x < solids[i] ? solids[i] : x > solids[i + 2] ? solids[i + 2] : x;
                double cz = z < solids[i + 1] ? solids[i + 1] : z > solids[i + 3] ? solids[i + 3] : z;
                double dx = x - cx, dz = z - cz;
                if (dx * dx + dz * dz < rad * rad) return true;
            }
            return false;
        }

        /// <summary>Push a footprint out of every solid (the walk's collision: the pillars and the walls).</summary>
        void PushOut()
        {
            for (int pass = 0; pass < 3; pass++)
                for (int i = 0; i < solids.Length; i += 5)
                {
                    double x = posL.x, z = posL.z, rad = p.footRadius;
                    double cx = x < solids[i] ? solids[i] : x > solids[i + 2] ? solids[i + 2] : x;
                    double cz = z < solids[i + 1] ? solids[i + 1] : z > solids[i + 3] ? solids[i + 3] : z;
                    double dx = x - cx, dz = z - cz, d2 = dx * dx + dz * dz;
                    if (d2 >= rad * rad) continue;
                    if (d2 < 1e-12)
                    {
                        // the centre is inside the box: leave by the nearest face
                        double l = x - solids[i], r = solids[i + 2] - x, b = z - solids[i + 1], f = solids[i + 3] - z;
                        double m = Math.Min(Math.Min(l, r), Math.Min(b, f));
                        if (m == l) posL.x = solids[i] - rad;
                        else if (m == r) posL.x = solids[i + 2] + rad;
                        else if (m == b) posL.z = solids[i + 1] - rad;
                        else posL.z = solids[i + 3] + rad;
                        continue;
                    }
                    double d = Math.Sqrt(d2);
                    posL.x = cx + dx / d * rad;
                    posL.z = cz + dz / d * rad;
                }
        }

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

        // --- The pose ------------------------------------------------------------------------------------------------
        void Pose()
        {
            for (int i = 0; i < boxes.Length; i++) prevCenters[i] = boxes[i].c;
            double fx = JsMath.Sin(ang), fz = JsMath.Cos(ang);
            Vec3 feet = ToWorld(posL);
            for (int i = 0; i < boxes.Length; i++)
            {
                HulkPartDef d = p.parts[i];
                WyrmBox b = default;
                b.Orient(new Vec3(fx, 0, fz));
                // right = (fz, 0, -fx), up = (0, 1, 0), forward = (fx, 0, fz)
                b.c = new Vec3(feet.x + b.r.x * d.a + b.f.x * d.c, feet.y + d.b, feet.z + b.r.z * d.a + b.f.z * d.c);
                b.hr = d.hr;
                b.hu = d.hu;
                b.hf = d.hf;
                boxes[i] = b;
            }
        }

        // --- Hits ----------------------------------------------------------------------------------------------------
        /// <summary>The nearest part along a unit ray: the distance or -1; hitPart names it (head, back, body).</summary>
        public double Raycast(Vec3 o, Vec3 d)
        {
            if (!body.alive || phase == "dying" || phase == "dead") return -1;
            double best = -1;
            string name = null;
            for (int i = 0; i < boxes.Length; i++)
            {
                double t = boxes[i].Ray(o, d);
                if (t >= 0 && (best < 0 || t < best))
                {
                    best = t;
                    name = partOf[i];
                }
            }
            if (best < 0) return -1;
            hitPart = name;
            return best;
        }

        /// <summary>The part's multiplier (body x1, head x2, back x2; x stunMult while it is stunned). Blocked in the intro, the beat and the finisher.</summary>
        public double Filter(double amount, out bool blocked) => Filter(amount, null, out blocked);

        /// <summary>The same with where the hit came from: a hit on the BODY (not the head or the back plate), while it is not stunned, lands x shellFront (the slag shell:
        /// aim at the head or the back, or wait for the stun).</summary>
        public double Filter(double amount, Vec3? from, out bool blocked)
        {
            blocked = false;
            if (!body.alive || phase == "intro" || phase == "beat" || phase == "dying")
            {
                blocked = true;
                return 0;
            }
            string part = hitPart ?? "body";
            hitPart = null;
            lastPart = part;
            double m = part == "head" ? p.headMult : part == "back" ? p.backMult : p.bodyMult;
            if (part == "head" && state == "walk") m *= p.guardHead; // arms up while it walks: the head is covered until it attacks
            if (part == "body" && !Stunned) m *= p.shellFront; // the slag shell: from any side
            return amount * m * (Stunned ? p.stunMult : 1);
        }

        public double TakeHit(Vec3 origin, Vec3 dir, double amount, string weapon, out double distance)
        {
            distance = Raycast(origin, dir);
            if (distance < 0) return -1;
            double dmg = Filter(amount, out bool blocked);
            if (blocked) return 0;
            Apply(dmg);
            return dmg;
        }

        public void Apply(double dmg)
        {
            if (!body.alive) return;
            body.hp -= dmg;
            body.flash = 0.06;
            string part = lastPart ?? "body";
            if (part == "head") stats["headHits"]++;
            else if (part == "back") stats["backHits"]++;
            else stats["bodyHits"]++;
            if (Stunned) stats["stunHits"]++;
            said.Reset("hulk");
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
                rings.Clear();
                markers.Clear();
                kit.Dying("hulk", body.position);
            }
        }

        void EnterBeat(string to, double at, string label)
        {
            body.hp = body.maxHp * at;
            nextPhase = to;
            SetPhase("beat");
            beatSteps = kit.Beat("hulk", label, new Vec3(body.position.x, body.position.y + 4, body.position.z));
            rings.Clear();
            markers.Clear();
            ring2Left = 0;
            StartWalk(1.0);
        }

        public BossBar Bar() => new BossBar
        {
            valid = body.alive, id = body.id, name = p.name, phase = phase, hp = Math.Max(0, body.hp), max = body.maxHp, segment = p.phase2At,
            pips = 2, pipsOn = phase == "p3" || phase == "dying" ? 0 : phase == "p2" || (phase == "beat" && nextPhase == "p3") ? 1 : 2,
        };

        // --- The fight -----------------------------------------------------------------------------------------------
        void StartWalk(double sec)
        {
            state = "walk";
            stateSteps = 0;
            walkLeft = S(sec);
            tellSteps = 0;
        }

        double WalkSeconds => EffPhase >= 3 ? p.walkGap3 : EffPhase == 2 ? p.walkGap2 : p.walkGap1;

        void TurnToPlayer(double rate)
        {
            Vec3 t = target.position();
            Vec3 pl = new Vec3(t.x - center.x, 0, t.z - center.z);
            double want = JsMath.Atan2(pl.x - posL.x, pl.z - posL.z);
            double d = want - ang;
            while (d > Math.PI) d -= 2 * Math.PI;
            while (d < -Math.PI) d += 2 * Math.PI;
            double step = rate * dt0;
            ang += Math.Max(-step, Math.Min(step, d));
        }

        double DistToPlayer()
        {
            Vec3 t = target.position();
            return JsMath.Hypot(t.x - center.x - posL.x, t.z - center.z - posL.z);
        }

        void StepFight(double dt)
        {
            double sp = PhaseSpeed;
            stateSteps++;
            switch (state)
            {
                case "walk":
                {
                    TurnToPlayer(p.turnRate * sp);
                    if (DistToPlayer() > p.standoff)
                    {
                        posL.x += JsMath.Sin(ang) * p.walk * sp * dt;
                        posL.z += JsMath.Cos(ang) * p.walk * sp * dt;
                        PushOut();
                    }
                    if (--walkLeft > 0) break;
                    bool slam = cycleIndex % 2 == 0;
                    if (slam && DistToPlayer() > p.slamRange) { walkLeft = 1; break; } // too far to slam: keep walking
                    // nothing resolves inside the gap of the last attack
                    if (stepNo - lastResolve < S(p.resolveGap)) { walkLeft = 1; break; }
                    cycleIndex++;
                    if (slam)
                    {
                        state = "slamTell";
                        stateSteps = 0;
                        tellSteps = S(p.slamTell);
                        Vec3 at = ToWorld(posL);
                        Tell("slam", p.slamTell, at);
                        kit.telegraph.Circle(at.x, at.y, at.z, p.slamTellR, p.slamTell);
                        markers.Add(new HulkMarker { kind = "slam", pos = at, left = tellSteps + S(0.3), total = tellSteps });
                    }
                    else
                    {
                        TurnToPlayer(1e6); // face the player once, then the line is locked
                        chargeDir = new Vec3(JsMath.Sin(ang), 0, JsMath.Cos(ang));
                        chargeLen = ChargeLength();
                        chargeDone = 0;
                        chargeHit = false;
                        state = "chargeTell";
                        stateSteps = 0;
                        tellSteps = S(p.chargeTell);
                        Vec3 at = ToWorld(posL);
                        Tell("charge", p.chargeTell, at);
                        kit.telegraph.Line(at.x, at.y, at.z, chargeDir.x, chargeDir.z, chargeLen, p.chargeWidth, p.chargeTell);
                        markers.Add(new HulkMarker { kind = "charge", pos = at, to = new Vec3(at.x + chargeDir.x * chargeLen, at.y, at.z + chargeDir.z * chargeLen), left = tellSteps + S(0.3), total = tellSteps });
                    }
                    break;
                }
                case "slamTell":
                    if (stateSteps >= tellSteps)
                    {
                        Resolve("slam");
                        stats["slams"]++;
                        rings.Add(new HulkRing());
                        if (EffPhase >= 3) ring2Left = S(p.ring2Delay);
                        Vec3 at = ToWorld(posL);
                        kit.telegraph.Flash(at.x, at.y + 0.3, at.z, 3);
                        state = "slamRecover";
                        stateSteps = 0;
                        tellSteps = 0;
                    }
                    break;
                case "slamRecover":
                    if (stateSteps >= S(p.slamRecover)) StartWalk(WalkSeconds);
                    break;
                case "chargeTell":
                    if (stateSteps >= tellSteps)
                    {
                        Resolve("charge");
                        stats["charges"]++;
                        state = "charge";
                        stateSteps = 0;
                        tellSteps = 0;
                    }
                    break;
                case "charge":
                {
                    double v = p.chargeSpeed * sp * dt;
                    double step = Math.Min(v, chargeLen - chargeDone);
                    posL.x += chargeDir.x * step;
                    posL.z += chargeDir.z * step;
                    chargeDone += step;
                    if (chargeDone >= chargeLen - 1e-9)
                    {
                        state = "stun";
                        stateSteps = 0;
                        stunLeft = S(p.chargeStun);
                        stats["stuns"]++;
                        said.Reset("hulk");
                        said.seconds = p.chargeStun;
                        events.Emit("bossStun", said);
                        Vec3 at = ToWorld(posL);
                        kit.telegraph.Flash(at.x + chargeDir.x * 1.5, at.y + 1.5, at.z + chargeDir.z * 1.5, 3);
                    }
                    break;
                }
                case "stun":
                    if (--stunLeft <= 0)
                    {
                        state = "recover";
                        stateSteps = 0;
                    }
                    break;
                case "recover":
                    if (stateSteps >= S(p.chargeRecover)) StartWalk(WalkSeconds);
                    break;
            }
        }

        /// <summary>How far a charge from here along chargeDir runs: to the first wall or pillar (the hulk's feet circle touches it), at most chargeMax.</summary>
        double ChargeLength()
        {
            double len = 0;
            const double ds = 0.25;
            while (len < p.chargeMax)
            {
                double nx = posL.x + chargeDir.x * (len + ds), nz = posL.z + chargeDir.z * (len + ds);
                if (Blocked(nx, nz, p.footRadius)) break;
                len += ds;
            }
            return len;
        }

        void StepRings(double dt)
        {
            if (ring2Left > 0 && --ring2Left == 0)
            {
                Resolve("slam2");
                rings.Add(new HulkRing());
            }
            if (rings.Count == 0) return;
            Capsule c = PlayerCapsule();
            Vec3 f = target.position(); // feet
            double floorY = center.y;
            double px = f.x - center.x - posL.x, pz = f.z - center.z - posL.z, pd = JsMath.Hypot(px, pz);
            for (int i = rings.Count - 1; i >= 0; i--)
            {
                HulkRing r = rings[i];
                r.r += p.slamRing * dt;
                if (!r.hit && f.y - floorY < p.slamRingHeight && Math.Abs(pd - r.r) < p.slamRingThick * 0.5 + c.radius * 0.5 && phase != "intro")
                {
                    r.hit = true;
                    if (HurtPlayer(p.slamDamage))
                    {
                        stats["slamHits"]++;
                        double l = pd < 1e-6 ? 1 : pd;
                        target.impulse?.Invoke(px / l * p.push, p.pushUp, pz / l * p.push);
                    }
                }
                if (r.r > p.slamRingMax) rings.RemoveAt(i);
            }
        }

        void StepContact()
        {
            if (contactLeft > 0) contactLeft--;
            if (phase == "intro" || phase == "dying") return;
            Capsule c = PlayerCapsule();
            Vec3 mid = new Vec3((c.start.x + c.end.x) * 0.5, (c.start.y + c.end.y) * 0.5, (c.start.z + c.end.z) * 0.5);
            bool charging = state == "charge";
            if (!charging && contactLeft > 0) return;
            if (charging && chargeHit) return;
            for (int i = 0; i < boxes.Length; i++)
            {
                WyrmBox b = boxes[i];
                if (!(b.Touches(c.start, c.radius) || b.Touches(mid, c.radius) || b.Touches(c.end, c.radius))) continue;
                double dmg = charging ? p.chargeDamage : p.contactBody;
                if (!HurtPlayer(dmg)) return;
                double dx = mid.x - b.c.x, dz = mid.z - b.c.z, len = JsMath.Hypot(dx, dz);
                if (charging)
                {
                    chargeHit = true;
                    stats["chargeHits"]++;
                    // thrown to the side of its line, not along it (a hit must not carry the player to the wall with the hulk)
                    double sx = chargeDir.z, sz = -chargeDir.x;
                    double side = dx * sx + dz * sz < 0 ? -1 : 1;
                    dx = sx * side;
                    dz = sz * side;
                    len = 1;
                }
                else
                {
                    contactLeft = S(p.contactCooldown);
                    stats["contacts"]++;
                    if (len < 1e-6) { dx = 1; dz = 0; len = 1; }
                }
                target.impulse?.Invoke(dx / len * (charging ? p.chargePush : p.push), p.pushUp, dz / len * (charging ? p.chargePush : p.push));
                return;
            }
        }

        public void Step(double dt)
        {
            if (!body.alive) return;
            body.previous = body.position;
            if (Frozen) return;
            stepNo++;
            phaseSteps++;
            if (phase == "intro")
            {
                if (phaseSteps >= kit.IntroSteps()) SetPhase("p1");
            }
            else if (phase == "beat")
            {
                if (phaseSteps >= beatSteps) SetPhase(nextPhase);
            }
            else if (phase != "dying") StepFight(dt);
            body.position = ToWorld(posL);
            body.yaw = ang;
            body.velocity = new Vec3((body.position.x - body.previous.x) / dt0, 0, (body.position.z - body.previous.z) / dt0);
            Pose();
            if (phase == "p1" || phase == "p2" || phase == "p3")
            {
                StepRings(dt);
                StepContact();
            }
            for (int i = markers.Count - 1; i >= 0; i--)
                if (--markers[i].left <= 0) markers.RemoveAt(i);
            if (phase == "dying" && --dieSteps <= 0)
            {
                body.alive = false;
                phase = "dead";
                kit.Down("hulk", body.id, body.position);
            }
        }
    }
}
