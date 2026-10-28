// NIGHT20 U3-34.9 A1 (builder EN): BULWARK, src/enemies/bulwark.js ported as it is (the sim; the look is the view's,
// Game/EnemyViews.Bulwark.cs). The web's header, kept:
// "BULWARK (OVERNIGHT2 N5, round 5): a tall blocky armored walker. Five breakable plates (two front, two shoulders, one
// back) take 10% until they break (armor-piercing: the Comet, Longwave, Thumper or any hit with momentum over 0.6
// deal x3); a broken plate drops points and bares the socket under it; the back plate covers the vent core, which
// glows white once it is open. It fights in the Grinder raised into a walled pen with a gap in each side:
//   RAM: a long red-orange line fills for 1.2 s, then it charges along it. Into a wall: stunned 3 s, a plate drops, its
//        back (the vent) is open to you. Through a gap or short of a wall: it skids to a stop.
//   STOMP: a red-orange circle fills under it, then a shock ring runs out along the floor (jump or dash over it).
//   SWEEP BEAM: a low laser that rotates around it (jump or ride a wall over it).
// Phase 2 below 50%: the beat, six Husks up through floor hatches, and every ram comes twice in a row. Standing still
// near one spot for 4 s makes it ram at once (passivity is punished). Fixed step; the look runs on frame time."
// Port notes (logged): the attack pick draws a seeded stream (the run seed ^ design.adds.bulwark.salt: NewRun) where the web
// draws Math.random (CLAUDE.md rule 6). The hit boxes follow the pose bulwark.js's render() gives them (the legs' swing by
// the distance walked, the ram's lean, the stun's drop), computed here on the fixed step from the same sim state, so a shot
// finds the same part the web's would. The shots reach it as the registry's enemy (BulwarkBody: RayHit is its raycast,
// EnemyHits.Filter its damage(): the part's multiplier, Damage its HP and phases; the kill lands at the finisher's end
// through the kit). Its body is a box layer (rays: false) the player and the horde collide with. Events keep the web's
// names (BossEvent): bossRise, bossPhase, bossTell, bossPunish, bossStun, bossPart, bossHit; the kit's bossIntro,
// bossBeat, bossDying, bossDown.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>config.bulwark, read once.</summary>
    public sealed class BulwarkParams
    {
        public double hp, speed, turnRate, height, radius, plateHp, plateMult, apMult, apMomentum, hullMult, exposedMult, coreMult, coreOpenMult, platePoints, actionGap;
        public double coreX, coreY, coreZ, coreR;
        public double ramTell, ramSpeed, ramMaxLength, ramWidth, ramDamage, ramPush, ramPushUp, ramStun, ramWallProbe;
        public double stompTell, stompSpeed, stompWidth, stompHeight, stompDamage, stompMaxRadius;
        public double beamTell, beamTime, beamArc, beamHeight, beamRange, beamDamage, beamHitCooldown;
        public double p2At, hatchTell, passiveTime, passiveRadius, arenaHalf;
        public string hatchKind;
        public readonly List<string> apWeapons = new List<string>();
        public readonly List<(string name, double[] box)> parts = new List<(string, double[])>(), plates = new List<(string, double[])>();
        public readonly List<(double x, double z)> hatches = new List<(double, double)>();
        public readonly List<(string name, double w)> weightsP1 = new List<(string, double)>(), weightsP2 = new List<(string, double)>();

        public static BulwarkParams From(GameConfig c)
        {
            var b = new BulwarkParams
            {
                hp = c.Num("bulwark.hp"), speed = c.Num("bulwark.speed"), turnRate = c.Num("bulwark.turnRate"), height = c.Num("bulwark.height"),
                radius = c.Num("bulwark.radius"), plateHp = c.Num("bulwark.plateHp"), plateMult = c.Num("bulwark.plateMult"), apMult = c.Num("bulwark.apMult"),
                apMomentum = c.Num("bulwark.apMomentum"), hullMult = c.Num("bulwark.hullMult"), exposedMult = c.Num("bulwark.exposedMult"),
                coreMult = c.Num("bulwark.coreMult"), coreOpenMult = c.Num("bulwark.coreOpenMult"), platePoints = c.Num("bulwark.platePoints"),
                actionGap = c.Num("bulwark.actionGap"),
                coreX = c.Num("bulwark.core.x"), coreY = c.Num("bulwark.core.y"), coreZ = c.Num("bulwark.core.z"), coreR = c.Num("bulwark.core.r"),
                ramTell = c.Num("bulwark.ram.tell"), ramSpeed = c.Num("bulwark.ram.speed"), ramMaxLength = c.Num("bulwark.ram.maxLength"),
                ramWidth = c.Num("bulwark.ram.width"), ramDamage = c.Num("bulwark.ram.damage"), ramPush = c.Num("bulwark.ram.push"),
                ramPushUp = c.Num("bulwark.ram.pushUp"), ramStun = c.Num("bulwark.ram.stun"), ramWallProbe = c.Num("bulwark.ram.wallProbe"),
                stompTell = c.Num("bulwark.stomp.tell"), stompSpeed = c.Num("bulwark.stomp.speed"), stompWidth = c.Num("bulwark.stomp.width"),
                stompHeight = c.Num("bulwark.stomp.height"), stompDamage = c.Num("bulwark.stomp.damage"), stompMaxRadius = c.Num("bulwark.stomp.maxRadius"),
                beamTell = c.Num("bulwark.beam.tell"), beamTime = c.Num("bulwark.beam.time"), beamArc = c.Num("bulwark.beam.arc"),
                beamHeight = c.Num("bulwark.beam.height"), beamRange = c.Num("bulwark.beam.range"), beamDamage = c.Num("bulwark.beam.damage"),
                beamHitCooldown = c.Num("bulwark.beam.hitCooldown"),
                p2At = c.Num("bulwark.p2At"), hatchTell = c.Num("bulwark.hatchTell"), hatchKind = c.Str("bulwark.hatchKind"),
                passiveTime = c.Num("bulwark.passive.time"), passiveRadius = c.Num("bulwark.passive.radius"), arenaHalf = c.Num("bulwark.arenaHalf"),
            };
            b.apWeapons.AddRange(c.Strs("bulwark.apWeapons"));
            foreach (object o in c.Arr("bulwark.parts")) b.parts.Add(Row(o));
            foreach (object o in c.Arr("bulwark.plates")) b.plates.Add(Row(o));
            foreach (object o in c.Arr("bulwark.hatches"))
            {
                var a = PortData.Arr(o);
                b.hatches.Add((PortData.Num(a[0]), PortData.Num(a[1])));
            }
            foreach (var kv in c.Obj("bulwark.weights.p1")) b.weightsP1.Add((kv.Key, PortData.Num(kv.Value)));
            foreach (var kv in c.Obj("bulwark.weights.p2")) b.weightsP2.Add((kv.Key, PortData.Num(kv.Value)));
            return b;
        }

        /// <summary>[name, w, h, d, x, y, z]</summary>
        static (string, double[]) Row(object o)
        {
            var a = PortData.Arr(o);
            var box = new double[6];
            for (int i = 0; i < 6; i++) box[i] = PortData.Num(a[i + 1]);
            return ((string)a[0], box);
        }
    }

    /// <summary>A hull part or a plate (bulwark.js parts / plates): its local box (size, base, the posed pos), a plate's HP.</summary>
    public sealed class BulwarkPart
    {
        public string name;
        public Vec3 size, basePos, pos;
        public double hp, max, flash;
        public bool broken;
    }

    /// <summary>The boss as the enemies' registry sees it (bulwark.js `boss`: kind bulwark, isBoss): the shots' target.</summary>
    public sealed class BulwarkBody : Enemy
    {
        public readonly Bulwark system;

        public BulwarkBody(Bulwark system)
        {
            this.system = system;
            kind = "bulwark";
        }

        public override double Height => system.b.height;
        public override double Radius => system.b.radius;
        public override double AimHeight => 4.8; // bulwark.js aimPoint: the chest, 4.8 m up

        /// <summary>bulwark.js raycast: the nearest part (a hull box, an unbroken plate, a bare socket, the open vent's core).</summary>
        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            headshot = false;
            return system.Raycast(o, d);
        }

        /// <summary>The HP and the phases (bulwark.js damage()'s second half); EnemyHits.Filter applied the part first. The kill
        /// lands at the end of the finisher (the kit's down), never here.</summary>
        public override bool Damage(double amount, double flashTime)
        {
            system.Apply(amount);
            return false;
        }

        // bulwark.js: knockback() {}, stagger() {} (and no stun: nothing moves it but its own legs)
        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt) { }
        public override void Stagger(double time, double dt) { }
        public override void Stun(double time, double dt) { }
    }

    public sealed class Bulwark : IBossSystem
    {
        sealed class Ram
        {
            public double dx, dz, length, done;
            public bool hit;
            public int chain, id;
        }

        sealed class Ring
        {
            public double r, x, z;
            public bool hit;
        }

        sealed class Beam
        {
            public double angle;
            public int charge, hitCooldown;
        }

        public readonly BulwarkParams b;
        public readonly BulwarkBody body;
        public readonly List<BulwarkPart> parts = new List<BulwarkPart>(), plates = new List<BulwarkPart>();
        /// <summary>Its body as a solid box (updateLayer): the sim adds it to the world (rays: false).</summary>
        public readonly BoxLayer layer = new BoxLayer();
        readonly EventBus events;
        readonly ICollisionWorld world;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly Action<List<string>, Vec3> spawnAt;
        readonly double dt0, capsuleHeight;
        readonly BossEvent said = new BossEvent();
        readonly List<string> hatchKinds = new List<string>(1);
        Mulberry32 rng = new Mulberry32(1);
        BossKit kit;
        BossScale scale = BossScale.One;
        Vec3 center;
        Ram ram;
        Ring ring;
        Beam beam;
        int beatSteps, hatchSteps, dieSteps;
        double passiveX, passiveZ, passiveT;

        public string phase = "dead", action = "walk";
        public int stun, actionSteps, phaseSteps;
        public double lean, walk, age;
        public string hitPart; // boss.hitPart: the part the last ray (or blast) found
        public readonly Dictionary<string, int> stats = new Dictionary<string, int>();
        static readonly string[] StatNames = { "rams", "wallStuns", "stomps", "beams", "platesBroken", "ringHits", "beamHits", "ramHits", "hatches" };

        public Bulwark(GameConfig cfg, EventBus events, ICollisionWorld world, BossTarget target, Func<int> nextId, Action<List<string>, Vec3> spawnAt)
        {
            b = BulwarkParams.From(cfg);
            this.events = events;
            this.world = world;
            this.target = target;
            this.nextId = nextId;
            this.spawnAt = spawnAt;
            dt0 = cfg.Dt;
            capsuleHeight = cfg.Num("body.capsuleHeight");
            body = new BulwarkBody(this);
            foreach (var (name, x) in b.parts) parts.Add(new BulwarkPart { name = name, size = new Vec3(x[0], x[1], x[2]), basePos = new Vec3(x[3], x[4], x[5]), pos = new Vec3(x[3], x[4], x[5]) });
            foreach (var (name, x) in b.plates) plates.Add(new BulwarkPart { name = name, size = new Vec3(x[0], x[1], x[2]), basePos = new Vec3(x[3], x[4], x[5]), pos = new Vec3(x[3], x[4], x[5]) });
            hatchKinds.Add(b.hatchKind);
            foreach (string s in StatNames) stats[s] = 0;
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public int Id => body.id;
        public Vec3 Center => center;
        /// <summary>NIGHT25 W: where a campaign boss room sits in the world (design.n25.bosses.bulwark.room.origin); null: the fight context's centre (the Grinder).</summary>
        public Vec3? arenaCenter;
        public int RamTelegraph => ram?.id ?? 0;
        public bool Ramming => ram != null;
        public bool RingLive => ring != null;
        public double RingRadius => ring?.r ?? 0;
        public double RingX => ring?.x ?? 0;
        public double RingZ => ring?.z ?? 0;
        public bool BeamLive => beam != null;
        public double BeamAngle => beam?.angle ?? 0;
        public bool BeamCharging => beam != null && beam.charge > 0;

        double FwdX => -JsMath.Sin(body.yaw);
        double FwdZ => -JsMath.Cos(body.yaw);

        /// <summary>Local (boss frame) -> world.</summary>
        Vec3 ToWorld(Vec3 v)
        {
            double c = JsMath.Cos(body.yaw), sn = JsMath.Sin(body.yaw);
            Vec3 p = body.position;
            return new Vec3(p.x + v.x * c + v.z * sn, p.y + v.y, p.z - v.x * sn + v.z * c);
        }

        /// <summary>World -> local (the inverse yaw).</summary>
        Vec3 ToLocal(Vec3 v, bool isDir = false)
        {
            Vec3 p = body.position;
            double x = isDir ? v.x : v.x - p.x, y = isDir ? v.y : v.y - p.y, z = isDir ? v.z : v.z - p.z;
            double c = JsMath.Cos(body.yaw), sn = JsMath.Sin(body.yaw);
            return new Vec3(x * c - z * sn, y, x * sn + z * c);
        }

        /// <summary>Slab test of a local ray against an axis-aligned local box; the entry distance or -1.</summary>
        static double RayBox(Vec3 o, Vec3 d, Vec3 c, Vec3 size)
        {
            double t0 = 0, t1 = double.PositiveInfinity;
            for (int ax = 0; ax < 3; ax++)
            {
                double oa = ax == 0 ? o.x : ax == 1 ? o.y : o.z, da = ax == 0 ? d.x : ax == 1 ? d.y : d.z;
                double ca = ax == 0 ? c.x : ax == 1 ? c.y : c.z, sa = ax == 0 ? size.x : ax == 1 ? size.y : size.z;
                double lo = ca - sa / 2, hi = ca + sa / 2;
                if (Math.Abs(da) < 1e-9)
                {
                    if (oa < lo || oa > hi) return -1;
                    continue;
                }
                double a = (lo - oa) / da, e = (hi - oa) / da;
                if (a > e) (a, e) = (e, a);
                t0 = JsMath.Max(t0, a);
                t1 = JsMath.Min(t1, e);
                if (t0 > t1) return -1;
            }
            return t0;
        }

        public bool VentOpen => stun > 0 || plates[4].broken;
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;
        Vec3 CoreWorld() => ToWorld(new Vec3(b.coreX, b.coreY, b.coreZ));

        /// <summary>The run's action stream (bulwark.js picks its attacks with Math.random: the port draws a seeded stream).</summary>
        public void NewRun(uint seed) => rng = new Mulberry32(seed);

        /// <summary>The action stream's state (the run snapshot: a fight never spans a break, the stream does).</summary>
        public uint RngState
        {
            get => rng.State;
            set => rng.State = value;
        }

        void Tell(string name, double seconds)
        {
            said.Reset("bulwark");
            said.name = name;
            said.seconds = seconds;
            events.Emit("bossTell", said);
            kit.telegraph.Threat(body.position.x, body.position.y + 4, body.position.z);
        }

        bool HurtPlayer(double amount, string attack, double px = 0, double py = 0, double pz = 0, bool push = false)
        {
            Health h = target.health;
            if (h == null || h.Dead) return false;
            bool hit = h.Damage(amount * scale.damage, "boss");
            if (hit && push) target.impulse?.Invoke(px, py, pz);
            return hit;
        }

        void SetPhase(string p)
        {
            phase = p;
            phaseSteps = 0;
            said.Reset("bulwark");
            said.phase = p;
            said.hp = body.hp;
            said.maxHp = body.maxHp;
            events.Emit("bossPhase", said);
        }

        void BreakPlate(BulwarkPart plate, string reason)
        {
            if (plate.broken) return;
            plate.broken = true;
            plate.hp = 0;
            stats["platesBroken"]++;
            said.Reset("bulwark");
            said.part = plate.name;
            said.reason = reason;
            said.points = b.platePoints;
            said.position = ToWorld(plate.pos);
            events.Emit("bossPart", said); // the view bursts the plate's grey bits (particles.burst 0x9aa3b0, 26, 9, 0.7, 1.6)
        }

        // --- Attacks -------------------------------------------------------------------------------------------------
        void StartRam(int chain)
        {
            Vec3 t = target.position();
            double dx = t.x - body.position.x, dz = t.z - body.position.z;
            double len = JsMath.Hypot(dx, dz);
            if (len == 0) len = 1;
            double dirX = dx / len, dirZ = dz / len;
            var end = BossMath.ChargeLength(body.position.x - center.x, body.position.z - center.z, dirX, dirZ, b.arenaHalf, b.ramMaxLength);
            body.yaw = JsMath.Atan2(-dirX, -dirZ);
            ram = new Ram { dx = dirX, dz = dirZ, length = end.length, chain = chain };
            ram.id = kit.telegraph.Line(body.position.x, center.y, body.position.z, dirX, dirZ, end.length + b.radius, b.ramWidth, b.ramTell);
            action = "ramTell";
            actionSteps = S(b.ramTell);
            stats["rams"]++;
            Tell("ram", b.ramTell);
        }

        void StartStomp()
        {
            action = "stompTell";
            actionSteps = S(b.stompTell);
            kit.telegraph.Circle(body.position.x, center.y, body.position.z, b.radius + 3.5, b.stompTell);
            stats["stomps"]++;
            Tell("stomp", b.stompTell);
        }

        void StartBeam()
        {
            Vec3 t = target.position();
            double start = JsMath.Atan2(t.z - body.position.z, t.x - body.position.x) - b.beamArc * 0.15;
            beam = new Beam { angle = start, charge = S(b.beamTell), hitCooldown = 0 };
            kit.telegraph.Line(body.position.x, center.y, body.position.z, JsMath.Cos(start), JsMath.Sin(start), b.beamRange, 1.2, b.beamTell);
            action = "beam";
            actionSteps = S(b.beamTell + b.beamTime);
            stats["beams"]++;
            Tell("beam", b.beamTell);
        }

        void NextAction()
        {
            var w = phase == "p2" ? b.weightsP2 : b.weightsP1;
            string pick = BossMath.PickWeighted(w, rng.Next()); // bulwark.js: Math.random() (the port: the run's seeded stream)
            if (pick == "ram") StartRam(phase == "p2" ? 1 : 0);
            else if (pick == "stomp") StartStomp();
            else StartBeam();
        }

        void Recover(double seconds)
        {
            action = "walk";
            actionSteps = S(seconds / scale.speed);
        }

        void Recover() => Recover(b.actionGap);

        /// <summary>Is a wall straight ahead (the collider, not the boss's own layer: it has no rays)?</summary>
        bool WallAhead(double dx, double dz, double reach) =>
            world.RayIntersect(new Vec3(body.position.x, center.y + 1.5, body.position.z), new Vec3(dx, 0, dz), out RayHit hit) && hit.distance <= reach;

        void StepAction(double dt)
        {
            Vec3 tp = target.position();
            if (actionSteps > 0) actionSteps--;
            if (action == "walk")
            {
                // Walk at you, turning at a limited rate; stop a few metres short.
                double want = JsMath.Atan2(-(tp.x - body.position.x), -(tp.z - body.position.z));
                double d = JsMath.Atan2(JsMath.Sin(want - body.yaw), JsMath.Cos(want - body.yaw));
                double turn = b.turnRate * dt * scale.speed;
                body.yaw += JsMath.Max(-turn, JsMath.Min(turn, d));
                double dist = JsMath.Hypot(tp.x - body.position.x, tp.z - body.position.z);
                if (dist > b.radius + 4)
                {
                    double v = b.speed * scale.speed * dt;
                    body.position.x += FwdX * v;
                    body.position.z += FwdZ * v;
                    walk += v;
                }
                // Passivity: you stayed near one spot too long -> it rams now.
                if (passiveT >= b.passiveTime)
                {
                    passiveT = 0;
                    StartRam(phase == "p2" ? 1 : 0);
                    said.Reset("bulwark");
                    events.Emit("bossPunish", said);
                }
                else if (actionSteps == 0) NextAction();
            }
            else if (action == "ramTell")
            {
                lean = JsMath.Min(1, lean + dt * 4); // the wind-up: it drops low and leans in
                if (actionSteps == 0)
                {
                    action = "ram";
                    actionSteps = S(ram.length / b.ramSpeed + 0.1);
                    kit.telegraph.Flash(body.position.x, body.position.y + 3, body.position.z, 2.4);
                }
            }
            else if (action == "ram")
            {
                Ram r = ram;
                double step = JsMath.Min(b.ramSpeed * dt, JsMath.Max(0, r.length - r.done));
                bool wall = WallAhead(r.dx, r.dz, b.radius + step + 0.05);
                if (!wall)
                {
                    body.position.x += r.dx * step;
                    body.position.z += r.dz * step;
                    r.done += step;
                }
                kit.telegraph.Move(r.id, body.position.x, center.y, body.position.z);
                if (!r.hit && JsMath.Hypot(tp.x - body.position.x, tp.z - body.position.z) <= b.radius + 1 && tp.y < body.position.y + b.height * 0.6)
                {
                    r.hit = true;
                    stats["ramHits"]++;
                    HurtPlayer(b.ramDamage, "ram", r.dx * b.ramPush, b.ramPushUp, r.dz * b.ramPush, true);
                }
                bool atEnd = r.done >= r.length - 1e-6 || actionSteps == 0;
                if (wall || (atEnd && WallAhead(r.dx, r.dz, b.radius + b.ramWallProbe)))
                {
                    // Into a wall: stunned, a plate falls off, the vent is open.
                    kit.telegraph.Cancel(r.id);
                    ram = null;
                    stats["wallStuns"]++;
                    stun = S(b.ramStun);
                    action = "stunned";
                    actionSteps = stun;
                    BulwarkPart weakest = null; // plates.filter(!broken).sort(by hp)[0]: the first of the lowest HP
                    foreach (BulwarkPart p in plates)
                        if (!p.broken && (weakest == null || p.hp < weakest.hp)) weakest = p;
                    if (weakest != null) BreakPlate(weakest, "wall");
                    said.Reset("bulwark");
                    said.seconds = b.ramStun;
                    said.position = new Vec3(body.position.x + r.dx * b.radius, body.position.y + 2, body.position.z + r.dz * b.radius); // the view's slam dust
                    events.Emit("bossStun", said);
                }
                else if (atEnd)
                {
                    kit.telegraph.Cancel(r.id);
                    ram = null;
                    lean = 0;
                    if (r.chain > 0) StartRam(r.chain - 1);
                    else Recover();
                }
            }
            else if (action == "stunned")
            {
                if (actionSteps == 0)
                {
                    stun = 0;
                    lean = 0;
                    Recover(0.4);
                }
            }
            else if (action == "stompTell")
            {
                if (actionSteps == 0)
                {
                    ring = new Ring { r = b.radius + 0.5, x = body.position.x, z = body.position.z }; // it stays where it landed
                    kit.telegraph.Flash(body.position.x, center.y + 0.5, body.position.z, 3);
                    Recover();
                }
            }
            else if (action == "beam")
            {
                Beam B = beam;
                if (B.charge > 0) B.charge--;
                else
                {
                    double prev = B.angle;
                    B.angle += b.beamArc / b.beamTime * dt;
                    if (B.hitCooldown > 0) B.hitCooldown--;
                    double pa = JsMath.Atan2(tp.z - body.position.z, tp.x - body.position.x);
                    bool crossed = Norm(pa - prev) <= Norm(B.angle - prev) + 1e-6;
                    double beamY = center.y + b.beamHeight;
                    double d = JsMath.Hypot(tp.x - body.position.x, tp.z - body.position.z);
                    if (crossed && B.hitCooldown == 0 && d <= b.beamRange && tp.y < beamY && tp.y + capsuleHeight > beamY)
                    {
                        B.hitCooldown = S(b.beamHitCooldown);
                        stats["beamHits"]++;
                        HurtPlayer(b.beamDamage, "beam");
                    }
                }
                if (actionSteps == 0)
                {
                    beam = null;
                    Recover();
                }
            }
            // The stomp's ring runs out along the floor: on the ground inside its band = hit (a jump or dash clears it).
            if (ring != null)
            {
                ring.r += b.stompSpeed * dt;
                double d = JsMath.Hypot(tp.x - ring.x, tp.z - ring.z);
                if (!ring.hit && Math.Abs(d - ring.r) <= b.stompWidth && tp.y < center.y + b.stompHeight)
                {
                    ring.hit = true;
                    stats["ringHits"]++;
                    HurtPlayer(b.stompDamage, "stomp");
                }
                if (ring.r > b.stompMaxRadius) ring = null;
            }
        }

        static double Norm(double v) => (v % (2 * Math.PI) + 2 * Math.PI) % (2 * Math.PI);

        readonly (Vec3 min, Vec3 max)[] oneBox = new (Vec3, Vec3)[1];
        static readonly (Vec3 min, Vec3 max)[] NoBox = new (Vec3, Vec3)[0];
        Vec3 layerAt = new Vec3(double.NaN, 0, 0);
        bool layerOn;

        /// <summary>updateLayer: its body as one solid box while it fights (rebuilt only when it moved: the port's no-alloc rule).</summary>
        void UpdateLayer()
        {
            Vec3 p = body.position;
            bool on = body.alive && phase != "dying";
            if (on == layerOn && (!on || (p.x == layerAt.x && p.y == layerAt.y && p.z == layerAt.z))) return;
            layerOn = on;
            layerAt = p;
            double h = b.radius * 0.8;
            if (on)
            {
                oneBox[0] = (new Vec3(p.x - h, p.y - 1, p.z - h), new Vec3(p.x + h, p.y + b.height, p.z + h));
                layer.SetBoxes(oneBox);
            }
            else layer.SetBoxes(NoBox);
        }

        /// <summary>render()'s pose of the hit boxes (the legs swing with the distance walked; the ram's wind-up drops and leans
        /// the upper body in; the stun drops it), from the sim's state.</summary>
        void Pose()
        {
            double swing = JsMath.Sin(walk * 1.2) * 0.35;
            double st = stun > 0 ? 1 : 0;
            foreach (BulwarkPart p in parts)
            {
                Vec3 v = p.basePos;
                if (p.name == "legL") v.z += swing;
                else if (p.name == "legR") v.z -= swing;
                else
                {
                    v.y -= lean * 0.7 + st * 0.9;
                    v.z -= lean * 0.5 - st * 0.4;
                }
                p.pos = v;
            }
            foreach (BulwarkPart p in plates)
            {
                Vec3 v = p.basePos;
                v.y -= lean * 0.7 + st * 0.9;
                v.z -= lean * 0.5 - st * 0.4;
                p.pos = v;
            }
        }

        /// <summary>NIGHT25 W (design.n25.bosses): the phase a player's death restores ("p2", also a death inside the beat into it), or null in the intro and phase 1.</summary>
        public string RetryPhase => !body.alive ? null : phase == "beat" || phase == "p2" ? "p2" : null;

        /// <summary>NIGHT25 W: a player death inside the boss room: back to the START of phase 2 (the HP at p2At, the ram / ring / beam gone, the walk resumed); the plates
        /// keep what they lost and the seed stream continues. false: not in phase 2 (the caller starts the fight over).</summary>
        public bool RestorePhaseStart()
        {
            if (RetryPhase == null) return false;
            body.hp = body.maxHp * b.p2At;
            ram = null;
            ring = null;
            beam = null;
            stun = 0;
            hatchSteps = 0;
            if (phase != "p2") SetPhase("p2");
            Recover(b.actionGap);
            return true;
        }

        public BossBar Bar() => new BossBar
        {
            valid = body.alive, id = body.id, name = "BULWARK", phase = phase, hp = Math.Max(0, body.hp), max = body.maxHp, segment = b.p2At,
            pips = plates.Count, pipsOn = CountOn(),
        };

        int CountOn()
        {
            int n = 0;
            foreach (BulwarkPart p in plates) n += p.broken ? 0 : 1;
            return n;
        }

        /// <summary>The kit starts the fight: it walks in through the north gap of the pen (no damage for the intro).</summary>
        public void StartFight(BossFight ctx)
        {
            kit = ctx.kit;
            scale = ctx.scale;
            center = arenaCenter ?? ctx.center;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(b.hp * ctx.scale.hp);
            body.hp = body.maxHp;
            body.flash = 0;
            foreach (BulwarkPart p in plates)
            {
                p.max = JsMath.Round(b.plateHp * ctx.scale.hp);
                p.hp = p.max;
                p.broken = false;
                p.flash = 0;
            }
            body.position = new Vec3(center.x, center.y, center.z - b.arenaHalf + b.radius);
            body.previous = body.position;
            body.velocity = default;
            body.yaw = Math.PI; // facing +z, into the pen
            action = "walk";
            actionSteps = S(b.actionGap);
            ram = null;
            stun = 0;
            ring = null;
            beam = null;
            lean = 0;
            age = 0;
            walk = 0;
            hitPart = null;
            passiveT = 0;
            foreach (string s in StatNames) stats[s] = 0;
            SetPhase("intro");
            Pose();
            UpdateLayer();
            said.Reset("bulwark");
            said.name = "BULWARK";
            said.hp = body.maxHp;
            said.round = ctx.round;
            events.Emit("bossRise", said);
        }

        public void Clear()
        {
            body.alive = false;
            phase = "dead";
            ram = null;
            ring = null;
            beam = null;
            UpdateLayer();
        }

        /// <summary>
        /// bulwark.js damage()'s first half: hits by part: an unbroken plate (10%, x3 armor-piercing), a bare socket x1, the hull
        /// x0.35, the vent core x2 / x3. Blocked (nothing lands) in the intro, the beat and the finisher. `from`: where the hit
        /// came from (a blast's centre), for the part nearest it when no ray named one.
        /// </summary>
        public double Filter(double amount, string weapon, double momentum, Vec3 from, out bool blocked)
        {
            blocked = false;
            if (!body.alive || phase == "intro" || phase == "beat" || phase == "dying")
            {
                blocked = true;
                return 0;
            }
            string name = hitPart ?? NearestPart(from);
            hitPart = null;
            lastPart = name;
            BulwarkPart plate = null;
            foreach (BulwarkPart p in plates)
                if (p.name == name) plate = p;
            double dmg;
            if (name == "core") dmg = amount * (VentOpen ? b.coreOpenMult : b.coreMult);
            else if (plate != null && !plate.broken)
            {
                bool ap = BossMath.IsArmorPiercing(weapon, momentum, b);
                plate.hp -= amount * (ap ? b.apMult : 1);
                plate.flash = 0.08;
                dmg = amount * b.plateMult * (ap ? b.apMult : 1);
                if (plate.hp <= 0) BreakPlate(plate, "shot");
            }
            else if (plate != null) dmg = amount * b.exposedMult;
            else dmg = amount * b.hullMult;
            return dmg;
        }

        string lastPart;

        /// <summary>bulwark.js damage()'s second half: the HP, `bossHit`, phase 2's beat under p2At, the finisher at 0.</summary>
        public void Apply(double dmg)
        {
            if (!body.alive) return;
            body.hp -= dmg;
            body.flash = 0.06;
            said.Reset("bulwark");
            said.damage = JsMath.Round(dmg);
            said.part = lastPart;
            said.core = lastPart == "core";
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            if (phase == "p1" && HpFrac <= b.p2At)
            {
                body.hp = body.maxHp * b.p2At;
                SetPhase("beat");
                beatSteps = kit.Beat("bulwark", "PHASE 2", new Vec3(body.position.x, body.position.y + 4, body.position.z));
                action = "walk";
                ram = null;
                beam = null;
                foreach (var (x, z) in b.hatches) kit.telegraph.Circle(center.x + x, center.y, center.z + z, 1.6, b.hatchTell);
                hatchSteps = S(b.hatchTell);
            }
            else if (body.hp <= 0)
            {
                body.hp = 0;
                SetPhase("dying");
                dieSteps = S(0.5);
                kit.Dying("bulwark", new Vec3(body.position.x, body.position.y + 4, body.position.z));
            }
        }

        /// <summary>The web's damage(boss, amount, {part, weapon, momentum}) in one call (Filter, then the HP); the damage dealt.</summary>
        public double Hit(double amount, string weapon, double momentum, string part, out bool blocked)
        {
            hitPart = part;
            double dmg = Filter(amount, weapon, momentum, body.position, out blocked);
            if (blocked) return 0;
            Apply(dmg);
            return dmg;
        }

        /// <summary>bulwark.js raycast: the distance to the nearest part along a unit ray (-1: none); hitPart names it.</summary>
        public double Raycast(Vec3 origin, Vec3 dir)
        {
            if (!body.alive || phase == "dying") return -1;
            Vec3 lo = ToLocal(origin), ld = ToLocal(dir, true);
            double best = -1;
            string bestName = null;
            void Consider(double d, string name)
            {
                if (d >= 0 && (best < 0 || d < best))
                {
                    best = d;
                    bestName = name;
                }
            }
            foreach (BulwarkPart p in parts) Consider(RayBox(lo, ld, p.pos, p.size), p.name);
            foreach (BulwarkPart p in plates)
                if (!p.broken && !(p.name == "back" && stun > 0)) Consider(RayBox(lo, ld, p.pos, p.size), p.name);
            // Broken plates leave their socket (the same box, x1) just under where the plate was; the back plate leaves the
            // open vent (the core).
            foreach (BulwarkPart p in plates)
                if (p.broken && p.name != "back") Consider(RayBox(lo, ld, p.pos, new Vec3(p.size.x * 0.9, p.size.y * 0.9, p.size.z * 0.9)), p.name);
            if (VentOpen) Consider(CombatMath.RaySphere(origin, dir, CoreWorld(), b.coreR), "core");
            if (best < 0) return -1;
            hitPart = bestName;
            return best;
        }

        /// <summary>The part nearest a point (melee and blasts): the vent core when it is open and closest.</summary>
        string NearestPart(Vec3 point)
        {
            string best = "torso";
            double bestD = double.PositiveInfinity;
            void Check(string name, Vec3 local)
            {
                Vec3 w = ToWorld(local);
                double dx = w.x - point.x, dy = w.y - point.y, dz = w.z - point.z;
                double d = dx * dx + dy * dy + dz * dz;
                if (d < bestD)
                {
                    bestD = d;
                    best = name;
                }
            }
            foreach (BulwarkPart p in parts) Check(p.name, p.basePos);
            foreach (BulwarkPart p in plates) Check(p.name, p.basePos);
            if (VentOpen) Check("core", new Vec3(b.coreX, b.coreY, b.coreZ + 0.6));
            return best;
        }

        /// <summary>Debug: start an attack now (ram | stomp | beam), from any state of a fight phase.</summary>
        public void DebugAct(string name)
        {
            ram = null;
            beam = null;
            lean = 0;
            if (name == "ram") StartRam(0);
            else if (name == "stomp") StartStomp();
            else StartBeam();
        }

        /// <summary>A part's world point (a plate, 'core'), as the base pose has it.</summary>
        public Vec3 PartPoint(string name)
        {
            if (name == "core") return CoreWorld();
            foreach (BulwarkPart p in plates)
                if (p.name == name) return ToWorld(p.pos);
            foreach (BulwarkPart p in parts)
                if (p.name == name) return ToWorld(p.pos);
            return body.position;
        }

        public void Step(double dt)
        {
            if (!body.alive) return;
            body.previous = body.position;
            if (Frozen) return;
            age += dt;
            phaseSteps++;
            if (stun > 0) stun--;
            // Passivity clock: you linger within passive.radius of one spot.
            Vec3 tp = target.position();
            if (JsMath.Hypot(tp.x - passiveX, tp.z - passiveZ) > b.passiveRadius)
            {
                passiveX = tp.x;
                passiveZ = tp.z;
                passiveT = 0;
            }
            else passiveT += dt;
            if (phase == "intro")
            {
                // Walk in (no attacks, no damage) while the intro card shows.
                body.position.z += b.speed * 1.6 * dt;
                if (phaseSteps >= kit.IntroSteps()) SetPhase("p1");
            }
            else if (phase == "beat")
            {
                if (hatchSteps > 0 && --hatchSteps == 0)
                {
                    int n = (int)JsMath.Round(b.hatches.Count * scale.adds);
                    for (int i = 0; i < n; i++)
                    {
                        var (x, z) = b.hatches[i % b.hatches.Count];
                        spawnAt?.Invoke(hatchKinds, new Vec3(center.x + x, center.y, center.z + z));
                        stats["hatches"]++;
                    }
                }
                if (phaseSteps >= beatSteps)
                {
                    SetPhase("p2");
                    Recover(0.3);
                }
            }
            else if (phase == "dying")
            {
                lean = JsMath.Min(1, lean + dt * 3);
                if (--dieSteps <= 0)
                {
                    body.alive = false;
                    phase = "dead";
                    kit.Down("bulwark", body.id, CoreWorld()); // the view's white burst at the core (particles.burst 70, 16, 1, 2.4)
                }
            }
            else if (phase == "p1" || phase == "p2") StepAction(dt);
            // The pen keeps it in (a ram stops at the wall anyway).
            double half = b.arenaHalf;
            body.position.x = JsMath.Max(center.x - half, JsMath.Min(center.x + half, body.position.x));
            body.position.z = JsMath.Max(center.z - half, JsMath.Min(center.z + half, body.position.z));
            body.velocity = new Vec3((body.position.x - body.previous.x) / dt0, (body.position.y - body.previous.y) / dt0, (body.position.z - body.previous.z) / dt0);
            Pose();
            UpdateLayer();
        }
    }
}
