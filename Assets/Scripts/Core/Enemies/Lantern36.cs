// NIGHT36 builder B (boss36; NIGHT36.md B1): THE LANTERN rebuilt on the shared boss shape (Core/Boss36.cs), fought in level 1's last room
// (Lantern36Arena, the hall builder L leads into: Design/levels/n36/lantern-hall.spec.md). The level's verb is the WALL RIDE; the boss is its exam.
//   THE CORE     hovers over the centre well, coreY m up: level with a rider. Its SHIELD lets through only a light share of a hit (lightRide on the
//                wall, lightFloor from the floor). Kept from n31: the CHARGED KESTREL from the wall breaks it (the core is OPEN for coreOpenSeconds:
//                full damage from the wall, openFloor from the floor). New: the orbs are YELLOW; parried back, each cracks the shield and
//                lands parryDamage; cracksToBreak cracks break it.
//   ATTACKS      (one tell each, never two at once; Boss36Brain's order):
//                ORB FAN  YELLOW: the cage opens, the core flares; fanCount slow orbs at where the player will be when they arrive (his motion round the hall for the flight time, capped at leadMaxSeconds). PARRY them.
//                FLOOR    RED: the floor glows from the well out, then is LAVA for activeSeconds (a rider or a player above safeY is safe). RIDE.
//                         Then a PUNISH: the lantern sags to punishY with the core open.
//                SWEEP    RED: an arm points at the wall aheadDeg ahead of you; a beam sweeps the wall band against your direction. DODGE (drop off
//                         the wall or jump it). Then a PUNISH (the arm is stuck in the wall).
//                CAMP     RED (anti-camp): a slam aimed at the spot a player has camped on for campSeconds (Boss36Brain's clock).
//   THE STUN     at 50 % (Boss36Params.stunAt) the lantern drops onto the well for stunSeconds: every hit lands in full. Then ONE change: the
//                sweep becomes TWO beams, opposite each other.
//   THE REVEAL   (walkin36) the lantern rises out of the well (riseFrom -> coreY over riseSeconds) inside the reveal; no damage until the fight.
// No added health (bar = pool; band 15-22 s for the bot: harness boss36). Fixed 120 Hz, whole steps, no randomness needed (the brain's order),
// JsMath, no audio (events: boss36Tell / boss36Attack / boss36Punish / boss36Stun / boss36Phase / boss36Camp, lantern36Floor, lantern36Sweep,
// lantern36Shield, parry36), no UnityEngine.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Lantern36Params
    {
        public string name;
        public double pool, coreY, bodyR, stunY, punishY, riseFrom, riseSeconds, moveRate, dieSeconds, revealSeconds;
        public double lightRide, lightFloor, openFloor, chargedMin, coreOpenSeconds, rideGraceSeconds, parryDamage, parryReach, parryConeDeg, parryBufferSeconds;
        public int cracksToBreak;
        public double fanTell, fanActive, orbSpeed, orbRadius, orbDamage, orbLife, fanDeg, leadSeconds, leadMaxSeconds, returnSpeed;
        public int fanCount;
        public double floorTell, floorLive, floorPunish, floorDps, floorTick, safeY, floorR;
        public double sweepTell, sweepActive, sweepPunish, sweepDegPerS, sweepHalfDeg, sweepAheadDeg, bandLow, bandHigh, wallBand, sweepDamage;
        public int beamsP1, beamsP2;
        public double campTell, campActive, campRadius, campDamage;
        public double capsuleHeight, capsuleRadius;

        public static Lantern36Params From(GameConfig c)
        {
            const string g = "design.n36.boss36.lantern.";
            double N(string k) => c.Num(g + k);
            return new Lantern36Params
            {
                name = c.Str(g + "name"),
                pool = N("pool"), coreY = N("coreY"), bodyR = N("bodyR"), stunY = N("stunY"), punishY = N("punishY"), riseFrom = N("riseFrom"), riseSeconds = N("riseSeconds"),
                moveRate = N("moveRate"), dieSeconds = N("dieSeconds"), revealSeconds = N("revealSeconds"),
                lightRide = N("shield.lightRide"), lightFloor = N("shield.lightFloor"), openFloor = N("shield.openFloor"), chargedMin = N("shield.chargedMin"),
                coreOpenSeconds = N("shield.coreOpenSeconds"), rideGraceSeconds = N("shield.rideGraceSeconds"), parryDamage = N("shield.parryDamage"),
                parryReach = N("shield.parryReach"), parryConeDeg = N("shield.parryConeDeg"), parryBufferSeconds = N("shield.parryBufferSeconds"), cracksToBreak = (int)N("shield.cracksToBreak"),
                fanTell = N("fan.tellSeconds"), fanActive = N("fan.activeSeconds"), orbSpeed = N("fan.speed"), orbRadius = N("fan.radius"), orbDamage = N("fan.damage"), orbLife = N("fan.life"),
                fanDeg = N("fan.deg"), leadSeconds = N("fan.leadSeconds"), leadMaxSeconds = N("fan.leadMaxSeconds"), returnSpeed = N("fan.returnSpeed"), fanCount = (int)N("fan.count"),
                floorTell = N("floor.tellSeconds"), floorLive = N("floor.activeSeconds"), floorPunish = N("floor.punishSeconds"), floorDps = N("floor.damagePerSecond"), floorTick = N("floor.tickSeconds"),
                safeY = N("floor.safeY"), floorR = N("floor.radius"),
                sweepTell = N("sweep.tellSeconds"), sweepActive = N("sweep.activeSeconds"), sweepPunish = N("sweep.punishSeconds"), sweepDegPerS = N("sweep.degPerSecond"), sweepHalfDeg = N("sweep.halfWidthDeg"),
                sweepAheadDeg = N("sweep.aheadDeg"), bandLow = N("sweep.bandLow"), bandHigh = N("sweep.bandHigh"), wallBand = N("sweep.wallBand"), sweepDamage = N("sweep.damage"),
                beamsP1 = (int)N("sweep.beamsP1"), beamsP2 = (int)N("sweep.beamsP2"),
                campTell = N("camp.tellSeconds"), campActive = N("camp.activeSeconds"), campRadius = N("camp.radius"), campDamage = N("camp.damage"),
                capsuleHeight = c.Num("body.capsuleHeight"), capsuleRadius = c.Num("body.capsuleRadius"),
            };
        }
    }

    public sealed class Lantern36Body : Enemy, IBossBody27
    {
        public readonly Lantern36 system;

        public Lantern36Body(Lantern36 system)
        {
            this.system = system;
            kind = "lantern";
        }

        public override double Height => system.p.bodyR * 2;
        public override double Radius => system.p.bodyR;
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

    public sealed class Lantern36Orb
    {
        public Vec3 pos, prev, vel;
        public int life;
        public bool alive, back;
    }

    /// <summary>The payload of lantern36Floor / lantern36Sweep / lantern36Shield (pooled: read it, don't keep it).</summary>
    public sealed class Lantern36Event
    {
        public string stage;
        public int count;
        public double angleDeg;
        public Vec3 at;
    }

    public sealed class Lantern36 : IBossSystem
    {
        public const int Fan = 0, Floor = 1, Sweep = 2, Camp = 3;
        public readonly Lantern36Params p;
        public readonly Lantern36Body body;
        public readonly Boss36Brain brain;
        readonly EventBus events;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt0;
        readonly BossEvent said = new BossEvent();
        readonly Lantern36Event l36 = new Lantern36Event();
        readonly HitEvent parried = new HitEvent();
        BossKit kit;
        BossScale scale;

        public Vec3 origin;
        public Func<bool> playerRiding;
        public Func<Capsule> playerBody;
        /// <summary>A returned orb reached the core: the sim lands `damage` as a parry hit (weapon parry31: Filter takes it in full).</summary>
        public Action<double, Vec3> parryHit;
        public readonly List<Lantern36Orb> orbs = new List<Lantern36Orb>();

        public double coreHeight; // the core's centre above the floor now (the rise, the sag, the stun drop)
        public int coreOpenLeft, cracks, floorTickLeft, lastRideStep = -100000, stepNo, lastPunchStep = -100000;
        bool punchUsed;
        public bool floorLive;
        public double sweepAngle; // radians, arena-local atan2(z, x) of the first beam
        public int sweepDir = 1;
        public double hpLock = 1;

        // stats for the harness and the reels
        public int orbHits, parries, cracksTotal, breaks, chargedBreaks, floorHits, sweepHits, campHits, fullHits, lightHits, blockedHits, stunHits;
        public double dealt;

        public Lantern36(GameConfig cfg, Lantern36Params p, Boss36Params bp, EventBus events, BossTarget target, Func<int> nextId)
        {
            this.p = p;
            this.events = events;
            this.target = target;
            this.nextId = nextId;
            dt0 = cfg.Dt;
            body = new Lantern36Body(this);
            brain = new Boss36Brain(bp, "lantern", dt0, events);
            brain.attacks.Add(new Boss36Attack { id = "lanternOrbFan", kind = Tell36Kind.Yellow, tellSeconds = p.fanTell, activeSeconds = p.fanActive });
            brain.attacks.Add(new Boss36Attack { id = "lanternFloor", kind = Tell36Kind.Red, tellSeconds = p.floorTell, activeSeconds = p.floorLive, punishSeconds = p.floorPunish });
            brain.attacks.Add(new Boss36Attack { id = "lanternSweep", kind = Tell36Kind.Red, tellSeconds = p.sweepTell, activeSeconds = p.sweepActive, punishSeconds = p.sweepPunish });
            brain.attacks.Add(new Boss36Attack { id = "lanternCamp", kind = Tell36Kind.Red, tellSeconds = p.campTell, activeSeconds = p.campActive, camp = true });
            brain.orderP1 = new[] { Floor, Fan, Sweep, Fan }; // the level's verb first (ride), then the parry, then the dodge: all three before the stun
            brain.orderP2 = new[] { Fan, Sweep, Fan, Floor };
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;
        public Vec3 Centre => new Vec3(origin.x, origin.y + coreHeight, origin.z);
        public bool Hurtable => body.alive && brain.Fighting;
        /// <summary>The core is open to full hits from the wall now (a punish, the stun, or the broken shield).</summary>
        public bool CoreOpen => brain.Open || coreOpenLeft > 0;
        public int Beams => brain.phase >= 2 ? p.beamsP2 : p.beamsP1;
        /// <summary>0..1 through the reveal (the view's rise and beam); 1 after it.</summary>
        public double RevealProgress => brain.state == Boss36State.Reveal ? brain.Progress : 1;

        /// <summary>Set by the sim for the one BossKit.Start that only shows the name card mid-reveal: that Start's Clear and StartFight keep the running
        /// fight (the reveal began at the walk-in; the card comes walkin36.cardSeconds later, hall spec step 4).</summary>
        public bool resume;

        public void StartFight(BossFight ctx)
        {
            if (resume) { kit = ctx.kit; scale = ctx.scale; return; }
            kit = ctx.kit;
            scale = ctx.scale;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(p.pool * ctx.scale.hp);
            body.hp = body.maxHp;
            stepNo = 0;
            orbs.Clear();
            coreOpenLeft = cracks = floorTickLeft = 0;
            floorLive = false;
            lastRideStep = lastPunchStep = -100000;
            punchUsed = true;
            orbHits = parries = cracksTotal = breaks = chargedBreaks = floorHits = sweepHits = campHits = fullHits = lightHits = blockedHits = stunHits = 0;
            dealt = 0;
            coreHeight = p.riseFrom;
            brain.source = body.id;
            brain.Begin(S(p.revealSeconds), target.position());
            body.position = Centre;
            body.previous = body.position;
            said.Reset("lantern");
            said.name = p.name;
            said.hp = body.maxHp;
            events.Emit("bossRise", said);
        }

        public void Clear()
        {
            if (resume) return;
            body.alive = false;
            orbs.Clear();
            floorLive = false;
            brain.tells?.End(brain.source);
            brain.Reset();
        }

        public BossBar Bar() => new BossBar
        {
            valid = body.alive, id = body.id, name = p.name, phase = brain.state.ToString(), hp = Math.Max(0, body.hp), max = body.maxHp, segment = brain.p.stunAt,
            pips = p.cracksToBreak, pipsOn = cracks,
        };

        /// <summary>The ray's distance to the core's sphere (-1: a miss).</summary>
        public double Raycast(Vec3 o, Vec3 d)
        {
            if (!body.alive || brain.state == Boss36State.Dead) return -1;
            Vec3 c = Centre, oc = o - c;
            double b = Vec3.Dot(oc, d), cc = Vec3.Dot(oc, oc) - p.bodyR * p.bodyR, disc = b * b - cc;
            if (disc < 0) return -1;
            double t = -b - Math.Sqrt(disc);
            if (t < 0) t = -b + Math.Sqrt(disc);
            return t >= 0 ? t : -1;
        }

        bool RidingNow => stepNo - lastRideStep <= S(p.rideGraceSeconds);

        /// <summary>The share that lands (blocked: the reveal or the finisher).</summary>
        public double Filter(double amount, string weapon, out bool blocked)
        {
            blocked = false;
            if (!Hurtable) { blocked = true; blockedHits++; return 0; }
            if (weapon == "parry31" || weapon == "parry36") { fullHits++; return amount; }
            if (brain.state == Boss36State.Stun) { stunHits++; fullHits++; return amount; }
            bool wall = RidingNow;
            if (!CoreOpen && wall && weapon == "revolver" && amount >= p.chargedMin)
            {
                chargedBreaks++;
                Break("charged");
                fullHits++;
                return amount;
            }
            if (CoreOpen)
            {
                if (wall) { fullHits++; return amount; }
                lightHits++;
                return amount * p.openFloor;
            }
            lightHits++;
            return amount * (wall ? p.lightRide : p.lightFloor);
        }

        void Break(string why)
        {
            breaks++;
            cracks = 0;
            coreOpenLeft = S(p.coreOpenSeconds);
            l36.stage = "break";
            l36.count = breaks;
            l36.at = Centre;
            events.Emit("lantern36Shield", l36);
        }

        public void Apply(double dmg)
        {
            if (!body.alive) return;
            body.hp -= dmg;
            dealt += dmg;
            body.flash = 0.06;
            said.Reset("lantern");
            said.damage = JsMath.Round(dmg);
            said.part = CoreOpen ? "core" : "shield";
            said.core = CoreOpen;
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            if (body.hp <= 0 && brain.state != Boss36State.Dying && brain.state != Boss36State.Dead)
            {
                body.hp = 0;
                orbs.Clear();
                floorLive = false;
                brain.Die(S(p.dieSeconds));
                kit?.Dying("lantern", Centre);
            }
        }

        /// <summary>A punch from `eye` along `dir` (the punch event): the nearest live yellow orb within parryReach in the cone goes back. A punch that
        /// meets no orb is remembered for parryBufferSeconds (ULTRAKILL's early buffer, Enemy.cs:2698-2842: a punch ~0.12 s early still parries).</summary>
        public bool Punch(Vec3 eye, Vec3 dir)
        {
            lastPunchStep = stepNo;
            punchUsed = false;
            return TryParry(eye, dir);
        }

        Vec3 punchEye, punchDir;

        bool TryParry(Vec3 eye, Vec3 dir)
        {
            punchEye = eye;
            punchDir = dir;
            if (!body.alive) return false;
            double cosCone = JsMath.Cos(p.parryConeDeg * Math.PI / 180), best = p.parryReach + p.orbRadius;
            int idx = -1;
            for (int i = 0; i < orbs.Count; i++)
            {
                Lantern36Orb o = orbs[i];
                if (!o.alive || o.back) continue;
                Vec3 to = o.pos - eye;
                double d = to.Length;
                if (d > best) continue;
                if (d > 0.5 && Vec3.Dot(to, dir) / d < cosCone) continue;
                best = d;
                idx = i;
            }
            if (idx < 0) return false;
            Lantern36Orb ob = orbs[idx];
            ob.back = true;
            ob.life = S(p.orbLife);
            Vec3 home = Centre - ob.pos;
            double len = Math.Max(1e-6, home.Length);
            ob.vel = home * (p.returnSpeed / len);
            parries++;
            punchUsed = true;
            parried.owner = "player";
            parried.weapon = "punch";
            parried.kind = "lanternOrbFan";
            parried.enemy = body.id;
            parried.damage = p.parryDamage;
            parried.position = ob.pos;
            events.Emit("parry36", parried);
            return true;
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
            return h.Damage(amount * (scale.damage), "boss");
        }

        static double SegDist(Vec3 pt, Vec3 a, Vec3 b)
        {
            Vec3 ab = b - a, ap = pt - a;
            double len2 = Vec3.Dot(ab, ab), t = len2 < 1e-12 ? 0 : Math.Max(0, Math.Min(1, Vec3.Dot(ap, ab) / len2));
            return Vec3.DistanceTo(pt, a + ab * t);
        }

        static double Wrap(double a)
        {
            while (a > Math.PI) a -= 2 * Math.PI;
            while (a < -Math.PI) a += 2 * Math.PI;
            return a;
        }

        double PlayerAngle(Vec3 pw) => JsMath.Atan2(pw.z - origin.z, pw.x - origin.x);

        void FireFan(Vec3 pw)
        {
            Vec3 c = Centre;
            Capsule cap = PlayerCapsule();
            Vec3 aim = (cap.start + cap.end) * 0.5;
            Vec3 aimPt = LeadPoint(c, aim, pw);
            lastLead = aimPt;
            Vec3 d0 = aimPt - c;
            double len = Math.Max(1e-6, d0.Length);
            double baseYaw = JsMath.Atan2(d0.z, d0.x), pitch = JsMath.Asin(Math.Max(-1, Math.Min(1, d0.y / len)));
            int n = p.fanCount;
            for (int i = 0; i < n; i++)
            {
                double yaw = baseYaw + (i - (n - 1) * 0.5) * p.fanDeg * Math.PI / 180;
                double cp = JsMath.Cos(pitch);
                var v = new Vec3(JsMath.Cos(yaw) * cp, JsMath.Sin(pitch), JsMath.Sin(yaw) * cp) * p.orbSpeed;
                orbs.Add(new Lantern36Orb { pos = c, prev = c, vel = v, life = S(p.orbLife), alive = true });
            }
        }

        Vec3 prevPlayer;
        /// <summary>The last fan's aim point (the harness traces it).</summary>
        public Vec3 lastLead;

        /// <summary>Where the fan aims: the player's point carried along his motion for the orb's flight time (capped at leadMaxSeconds). His motion is read
        /// in the hall's polar terms (angular speed round the centre, radial and vertical speed), so a rider circling the wall is led ALONG the curve,
        /// not off it into the wall: he must parry or dodge, never be missed for free. Two passes on the flight time.</summary>
        Vec3 LeadPoint(Vec3 core, Vec3 aim, Vec3 pw)
        {
            double ax = aim.x - origin.x, az = aim.z - origin.z, r = JsMath.Hypot(ax, az);
            double a0 = JsMath.Atan2(az, ax), aPrev = JsMath.Atan2(prevPlayer.z - origin.z, prevPlayer.x - origin.x);
            double w = Wrap(a0 - aPrev) / dt0; // rad/s round the centre
            double rPrev = JsMath.Hypot(prevPlayer.x - origin.x, prevPlayer.z - origin.z), vr = (JsMath.Hypot(pw.x - origin.x, pw.z - origin.z) - rPrev) / dt0, vy = (pw.y - prevPlayer.y) / dt0;
            Vec3 at = aim;
            for (int k = 0; k < 2; k++)
            {
                double tf = Math.Min(p.leadMaxSeconds, (at - core).Length / p.orbSpeed);
                double a = a0 + w * tf, rr = Math.Max(0, Math.Min(p.floorR - 0.2, r + vr * tf));
                at = new Vec3(origin.x + JsMath.Cos(a) * rr, Math.Max(origin.y + 0.5, aim.y + vy * tf), origin.z + JsMath.Sin(a) * rr);
            }
            return at;
        }

        void StepOrbs(double dt)
        {
            Capsule cap = PlayerCapsule();
            double lim = p.floorR + 0.6;
            for (int i = orbs.Count - 1; i >= 0; i--)
            {
                if (i >= orbs.Count) continue; // a returned orb's hit killed the lantern (the list was cleared)
                Lantern36Orb o = orbs[i];
                o.prev = o.pos;
                if (o.back)
                {
                    Vec3 home = Centre - o.pos;
                    double len = home.Length;
                    if (len <= p.bodyR + p.orbRadius || len <= p.returnSpeed * dt)
                    {
                        orbs.RemoveAt(i);
                        cracks++;
                        cracksTotal++;
                        l36.stage = "crack";
                        l36.count = cracks;
                        l36.at = o.pos;
                        events.Emit("lantern36Shield", l36);
                        parryHit?.Invoke(p.parryDamage, o.pos);
                        if (cracks >= p.cracksToBreak && coreOpenLeft <= 0) Break("parry");
                        continue;
                    }
                    o.vel = home * (p.returnSpeed / len);
                }
                o.pos = o.pos + o.vel * dt;
                double rx = o.pos.x - origin.x, rz = o.pos.z - origin.z;
                bool dead = --o.life <= 0 || (!o.back && (rx * rx + rz * rz > lim * lim || o.pos.y < origin.y + 0.1 || o.pos.y > origin.y + 14));
                if (!dead && !o.back && SegDist(o.pos, cap.start, cap.end) < p.orbRadius + cap.radius)
                {
                    if (HurtPlayer(p.orbDamage)) orbHits++;
                    dead = true;
                }
                if (dead) orbs.RemoveAt(i);
            }
        }

        void StepFloor(Vec3 pw, bool riding)
        {
            if (!floorLive) return;
            if (floorTickLeft > 0 && --floorTickLeft > 0) return;
            floorTickLeft = Math.Max(1, S(p.floorTick));
            double rx = pw.x - origin.x, rz = pw.z - origin.z;
            if (riding || pw.y > origin.y + p.safeY || rx * rx + rz * rz > p.floorR * p.floorR) return;
            if (HurtPlayer(p.floorDps * p.floorTick)) floorHits++;
        }

        void StepSweep(Vec3 pw, bool riding, double dt)
        {
            sweepAngle = Wrap(sweepAngle + sweepDir * p.sweepDegPerS * Math.PI / 180 * dt);
            double rx = pw.x - origin.x, rz = pw.z - origin.z, r = JsMath.Hypot(rx, rz), y = pw.y - origin.y;
            if (r < arenaApothem - p.wallBand || y + p.capsuleHeight < p.bandLow || y > p.bandHigh) return;
            double a = JsMath.Atan2(rz, rx), half = p.sweepHalfDeg * Math.PI / 180;
            for (int k = 0; k < Beams; k++)
            {
                double b = sweepAngle + k * 2 * Math.PI / Beams;
                if (Math.Abs(Wrap(a - b)) > half) continue;
                if (sweepHitCool > 0) return;
                if (HurtPlayer(p.sweepDamage)) sweepHits++;
                sweepHitCool = S(p.sweepActive); // one hit per sweep
                return;
            }
        }

        int sweepHitCool;
        /// <summary>The hall's apothem (the sim sets it from Lantern36Arena; the sweep's wall band is measured from it).</summary>
        public double arenaApothem = 15;

        void StartActive(Vec3 pw)
        {
            l36.at = brain.campAt;
            switch (brain.attack)
            {
                case Fan:
                    FireFan(pw);
                    break;
                case Floor:
                    floorLive = true;
                    floorTickLeft = 0;
                    l36.stage = "live";
                    events.Emit("lantern36Floor", l36);
                    break;
                case Sweep:
                    sweepHitCool = 0;
                    l36.stage = "burn";
                    l36.angleDeg = sweepAngle * 180 / Math.PI;
                    events.Emit("lantern36Sweep", l36);
                    break;
                case Camp:
                    {
                        double dx = pw.x - brain.campAt.x, dz = pw.z - brain.campAt.z;
                        if (dx * dx + dz * dz <= p.campRadius * p.campRadius && pw.y < brain.campAt.y + 2.5 && HurtPlayer(p.campDamage)) campHits++;
                        l36.stage = "slam";
                        events.Emit("boss36Camp", l36);
                        break;
                    }
            }
        }

        void StartTell(Vec3 pw)
        {
            switch (brain.attack)
            {
                case Floor:
                    l36.stage = "tell";
                    events.Emit("lantern36Floor", l36);
                    break;
                case Sweep:
                    {
                        // the arm points aheadDeg ahead of the player (in the way he is circling), and the beam sweeps BACK toward him
                        double a = PlayerAngle(pw), prev = PlayerAngle(prevPlayer);
                        int dir = Wrap(a - prev) >= 0 ? 1 : -1;
                        sweepAngle = Wrap(a + dir * p.sweepAheadDeg * Math.PI / 180);
                        sweepDir = -dir;
                        l36.stage = "tell";
                        l36.angleDeg = sweepAngle * 180 / Math.PI;
                        events.Emit("lantern36Sweep", l36);
                        break;
                    }
                case Camp:
                    l36.stage = "tell";
                    l36.at = brain.campAt;
                    events.Emit("boss36Camp", l36);
                    break;
            }
        }

        void EndActive()
        {
            if (floorLive)
            {
                floorLive = false;
                l36.stage = "end";
                events.Emit("lantern36Floor", l36);
            }
        }

        public void Step(double dt)
        {
            if (!body.alive) return;
            body.previous = body.position;
            if (Frozen) return;
            stepNo++;
            Vec3 pw = target.position();
            bool riding = playerRiding != null && playerRiding();
            if (riding) lastRideStep = stepNo;
            if (stepNo == 1) prevPlayer = pw;
            Boss36State before = brain.state;
            int attackBefore = brain.attack;
            Vec3 aim = Centre;
            brain.Step(pw, riding, HpFrac, aim);
            if (brain.entered)
            {
                if (before == Boss36State.Active && attackBefore >= 0) EndActive();
                if (brain.state == Boss36State.Tell) StartTell(pw);
                else if (brain.state == Boss36State.Active) StartActive(pw);
                else if (brain.state == Boss36State.Stun) { EndActive(); orbs.Clear(); coreOpenLeft = 0; }
                else if (brain.state == Boss36State.Rest && before == Boss36State.Active) { }
            }
            if (brain.state == Boss36State.Dead)
            {
                body.alive = false;
                kit?.Down("lantern", body.id, Centre);
                return;
            }
            if (coreOpenLeft > 0 && --coreOpenLeft == 0)
            {
                l36.stage = "close";
                events.Emit("lantern36Shield", l36);
            }
            // the parry's early buffer: a punch that met no orb still parries one that comes into reach within parryBufferSeconds
            if (!punchUsed && stepNo - lastPunchStep <= S(p.parryBufferSeconds)) TryParry(punchEye, punchDir);
            if (brain.Fighting)
            {
                StepOrbs(dt);
                StepFloor(pw, riding);
                if (brain.state == Boss36State.Active && brain.attack == Sweep) StepSweep(pw, riding, dt);
                if (sweepHitCool > 0) sweepHitCool--;
            }
            // the core's height: the rise in the reveal, the sag in a punish, the drop in the stun
            double want = brain.state == Boss36State.Stun ? p.stunY : brain.state == Boss36State.Punish ? p.punishY : p.coreY;
            if (brain.state == Boss36State.Reveal)
            {
                double t = Math.Min(1, brain.stateSteps * dt / Math.Max(1e-6, p.riseSeconds));
                coreHeight = p.riseFrom + (p.coreY - p.riseFrom) * (1 - (1 - t) * (1 - t));
            }
            else
            {
                double stp = p.moveRate * dt, dh = want - coreHeight;
                coreHeight += Math.Max(-stp, Math.Min(stp, dh));
            }
            prevPlayer = pw;
            body.position = Centre;
            body.velocity = default;
        }
    }
}
