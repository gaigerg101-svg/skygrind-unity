// NIGHT24 leftover 8 (builder B): MONOLITH, src/enemies/boss.js (OVERNIGHT2 S7 / N5) ported through BossKit, FIRST SLICE, standalone like the worm
// (Research/2026-10-08-wave24/MONOLITH-PORT-PLAN.md). The web's header, kept:
// "MONOLITH (SURVIVAL.md section 8, S7): the boss. A 12 m construct of stacked stone pillars with a glowing orange core, rising out of THE GRINDER
// on boss rounds. 6000 HP (+40% each later boss round), a 3-segment boss bar, a core weak point (x2, x3 while it kneels / is vulnerable; the stone
// body takes x0.5). Intro (2 s, no damage) -> PHASE 1 (slams with 2 jumpable shockwave rings, 8-orb fans to parry into the core for 200, a 3 s
// kneel after every 3rd slam) -> MIDPOINT at 66% (roar, knockback pulse, the Grinder reshapes to its ring, 2 Husk squads) -> PHASE 2 (+ pillar
// surges under you, sweep charges along a painted line, adds every 20 s) -> PHASE 3 at 33% (the Grinder floor sinks except platforms, triple
// slams, a rotating laser, a 4 s window after the combo) -> the kill (slow-mo, the core bursts, "MONOLITH DOWN"). Fixed step."
// IN this slice: the state machine (intro, p1, mid, p2, p3, dying), slam + rings, the orb volley, the kneel (core x3), surge, sweep, laser, the
// core and body rays (raySphere / rayVerticalCapsule as the web), the damage rule, the BossKit intro / beat / finisher / reward, the body as a box
// layer. CUT (logged in the plan): the Grinder reshape, the Husk squads at the midpoint and the p2/p3 adds, the phase 3 floor sink (the floor stays),
// the punch on the kneeling core and the orb parry (200 into the core), Grinder columns under the rings. Port notes: nextAction draws a seeded
// stream (NewRun) where the web uses Math.random (CLAUDE.md rule 6); the orbs are the boss's own small projectiles (config orb.*), not the drones' pool.
// Numbers: config.boss (the web's), design.n24.monolith (the row's _why). Events keep the web's names: bossRise, bossPhase, bossTell, bossHit,
// bossRoar, bossSlam, bossVolley, bossSurge, bossKneel; the kit's bossIntro, bossBeat, bossDying, bossDown.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>config.boss (MONOLITH), read once.</summary>
    public sealed class MonolithParams
    {
        public double hp, hpPerBoss, height, bodyHalf, coreRadius, coreHeight, coreForward, kneelDrop, bodyMult, coreMult, coreVulnerableMult, introTime,
            midAt, p3At, midTime, actionGap, slamTell, slamTellFast, slamWarnRadius, ringGap, ringSpeed, ringSpeedFast, ringWidth, ringHeight, ringDamage,
            ringMaxRadius, kneelTime, p3Window, volleyTell, volleyFanRad, orbSpeed, surgeTell, surgeLead, surgeRadius, surgeDamage, surgeLaunch, surgeHitHeight,
            surgeErupt, surgeHeight, surgeRecover, sweepTell, sweepSpeed, sweepDistance, sweepDamage, sweepOvershoot, sweepPush, sweepPushUp, laserTell, laserTime,
            laserArc, laserHeight, laserRange, laserDamage, laserHitCooldown, killTime, arenaHalf;
        public int slamRings, volleyCount, surgeCount;
        public double orbDamage, orbRadius, orbLifetime, capsuleHeight, capsuleRadius;
        public double salt;
        public readonly List<double[]> body = new List<double[]>();

        public static MonolithParams From(GameConfig c)
        {
            var m = new MonolithParams
            {
                hp = c.Num("boss.hp"), hpPerBoss = c.Num("boss.hpPerBoss"), height = c.Num("boss.height"), bodyHalf = c.Num("boss.bodyHalf"), coreRadius = c.Num("boss.coreRadius"),
                coreHeight = c.Num("boss.coreHeight"), coreForward = c.Num("boss.coreForward"), kneelDrop = c.Num("boss.kneelDrop"), bodyMult = c.Num("boss.bodyMult"),
                coreMult = c.Num("boss.coreMult"), coreVulnerableMult = c.Num("boss.coreVulnerableMult"), introTime = c.Num("boss.introTime"), midAt = c.Num("boss.midAt"),
                p3At = c.Num("boss.p3At"), midTime = c.Num("boss.midTime"), actionGap = c.Num("boss.actionGap"), slamTell = c.Num("boss.slamTell"),
                slamTellFast = c.Num("boss.slamTellFast"), slamWarnRadius = c.Num("boss.slamWarnRadius"), slamRings = c.Int("boss.slamRings"), ringGap = c.Num("boss.ringGap"),
                ringSpeed = c.Num("boss.ringSpeed"), ringSpeedFast = c.Num("boss.ringSpeedFast"), ringWidth = c.Num("boss.ringWidth"), ringHeight = c.Num("boss.ringHeight"),
                ringDamage = c.Num("boss.ringDamage"), ringMaxRadius = c.Num("boss.ringMaxRadius"), kneelTime = c.Num("boss.kneelTime"), p3Window = c.Num("boss.p3Window"),
                volleyTell = c.Num("boss.volleyTell"), volleyCount = c.Int("boss.volleyCount"), volleyFanRad = c.Num("boss.volleyFanRad"), orbSpeed = c.Num("boss.orbSpeed"),
                surgeTell = c.Num("boss.surgeTell"), surgeCount = c.Int("boss.surgeCount"), surgeLead = c.Num("boss.surgeLead"), surgeRadius = c.Num("boss.surgeRadius"),
                surgeDamage = c.Num("boss.surgeDamage"), surgeLaunch = c.Num("boss.surgeLaunch"), surgeHitHeight = c.Num("boss.surgeHitHeight"), surgeErupt = c.Num("boss.surgeErupt"), surgeHeight = c.Num("boss.surgeHeight"),
                surgeRecover = c.Num("boss.surgeRecover"), sweepTell = c.Num("boss.sweepTell"), sweepSpeed = c.Num("boss.sweepSpeed"), sweepDistance = c.Num("boss.sweepDistance"),
                sweepDamage = c.Num("boss.sweepDamage"), sweepOvershoot = c.Num("boss.sweepOvershoot"), sweepPush = c.Num("boss.sweepPush"), sweepPushUp = c.Num("boss.sweepPushUp"),
                laserTell = c.Num("boss.laserTell"), laserTime = c.Num("boss.laserTime"), laserArc = c.Num("boss.laserArc"), laserHeight = c.Num("boss.laserHeight"),
                laserRange = c.Num("boss.laserRange"), laserDamage = c.Num("boss.laserDamage"), laserHitCooldown = c.Num("boss.laserHitCooldown"), killTime = c.Num("boss.killTime"),
                arenaHalf = c.Num("boss.arenaHalf"),
                orbDamage = c.Num("orb.damage"), orbRadius = c.Num("orb.radius"), orbLifetime = c.Num("orb.lifetime"),
                capsuleHeight = c.Num("body.capsuleHeight"), capsuleRadius = c.Num("body.capsuleRadius"),
                salt = c.Has("design.n24.monolith.salt") ? c.Num("design.n24.monolith.salt") : 0,
            };
            foreach (object o in c.Arr("boss.body"))
            {
                var a = PortData.Arr(o);
                var row = new double[7];
                for (int i = 0; i < 7; i++) row[i] = PortData.Num(a[i]);
                m.body.Add(row);
            }
            return m;
        }
    }

    /// <summary>The boss as the enemies' registry sees it (boss.js `boss`: kind boss, isBoss).</summary>
    public sealed class MonolithBody : Enemy
    {
        public readonly Monolith system;

        public MonolithBody(Monolith system)
        {
            this.system = system;
            kind = "boss";
        }

        public override double Height => system.b.height;
        public override double Radius => system.b.bodyHalf;
        public override double AimHeight => system.b.coreHeight;

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

    sealed class MonoRing
    {
        public double r, speed, y;
        public int delay;
        public bool hit;
    }

    sealed class MonoWarn
    {
        public double x, y, z, r;
        public int steps, total;
    }

    sealed class MonoSurge
    {
        public double x, y, z;
        public int steps, erupt;
    }

    /// <summary>A boss orb in flight.</summary>
    public sealed class MonolithOrb
    {
        public Vec3 pos, vel;
        public int life;
    }

    public sealed class Monolith : IBossSystem
    {
        public readonly MonolithParams b;
        public readonly MonolithBody body;
        /// <summary>Its body as a solid box (boss.js updateLayer): the sim adds it to the world (rays: false).</summary>
        public readonly BoxLayer layer = new BoxLayer();
        readonly EventBus events;
        readonly ICollisionWorld world;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt0;
        readonly BossEvent said = new BossEvent();
        Mulberry32 rng = new Mulberry32(1);
        BossKit kit;
        BossScale scale = BossScale.One;
        Vec3 center;

        public string phase = "dead", action = "idle";
        public int phaseSteps, actionSteps, slams, combo, kneel, vulnerable, killSteps, beatSteps;
        public double rise, age;
        public bool comboDone;
        readonly HashSet<int> kneltAt3 = new HashSet<int>();
        readonly List<MonoRing> rings = new List<MonoRing>();
        readonly List<MonoWarn> warns = new List<MonoWarn>();
        readonly List<MonoSurge> surges = new List<MonoSurge>();
        public readonly List<MonolithOrb> orbs = new List<MonolithOrb>();
        // sweep / laser
        bool sweepOn, sweepHit, laserOn;
        double sweepDx, sweepDz, sweepDist, sweepDone, laserAngle;
        int laserCharge, laserCool;
        public string hitPart;
        string lastPart;
        public Func<Capsule> playerBody;
        /// <summary>The player's velocity (the surges lead it, boss.js getTarget().mover.velocity); null: zero.</summary>
        public Func<Vec3> playerVelocity;
        public readonly List<(int step, string name, double seconds)> tells = new List<(int, string, double)>();
        public readonly Dictionary<string, int> stats = new Dictionary<string, int>();
        static readonly string[] StatNames = { "slams", "ringHits", "volleys", "orbHits", "surges", "surgeHits", "sweeps", "sweepHits", "lasers", "laserHits", "kneels", "coreHits", "bodyHits" };
        int stepNo;

        public Monolith(GameConfig cfg, EventBus events, ICollisionWorld world, BossTarget target, Func<int> nextId)
        {
            b = MonolithParams.From(cfg);
            this.events = events;
            this.world = world;
            this.target = target;
            this.nextId = nextId;
            dt0 = cfg.Dt;
            body = new MonolithBody(this);
            foreach (string s in StatNames) stats[s] = 0;
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public Vec3 Center => center;
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;
        public bool Kneeling => kneel > 0;
        public bool Vulnerable => vulnerable > 0;
        public bool SweepLive => sweepOn;
        public bool LaserLive => laserOn;
        public double LaserAngle => laserAngle;
        public bool LaserCharging => laserOn && laserCharge > 0;
        public int RingCount => rings.Count;
        public double RingRadius(int i) => rings[i].r;
        public bool RingLive(int i) => rings[i].delay <= 0;
        public double RingY(int i) => rings[i].y;
        public int WarnCount => warns.Count;
        public int SurgeCount => surges.Count;

        /// <summary>Surge i's spot and how far its pillar has risen (0..1, 0 while it is only a warning).</summary>
        public double Surge(int i, out double x, out double y, out double z)
        {
            MonoSurge g = surges[i];
            x = g.x; y = g.y; z = g.z;
            double total = Math.Max(1, S(b.surgeErupt));
            return g.erupt > 0 ? JsMath.Sin(Math.PI * (1 - g.erupt / total)) : 0;
        }
        public void Warn(int i, out double x, out double y, out double z, out double r, out double share)
        {
            MonoWarn w = warns[i];
            x = w.x; y = w.y; z = w.z; r = w.r;
            share = w.total > 0 ? 1 - (double)w.steps / w.total : 1;
        }

        public void NewRun(uint seed) => rng = new Mulberry32(seed);

        public uint RngState
        {
            get => rng.State;
            set => rng.State = value;
        }

        /// <summary>coreAt: the core's world point (it drops while the boss kneels).</summary>
        public Vec3 CoreWorld() => new Vec3(body.position.x - JsMath.Sin(body.yaw) * b.coreForward, body.position.y + b.coreHeight - (kneel > 0 ? b.kneelDrop : 0), body.position.z - JsMath.Cos(body.yaw) * b.coreForward);

        void SetPhase(string ph)
        {
            phase = ph;
            phaseSteps = 0;
            action = "idle";
            actionSteps = S(b.actionGap);
            said.Reset("monolith");
            said.phase = ph;
            said.hp = body.hp;
            said.maxHp = body.maxHp;
            events.Emit("bossPhase", said);
        }

        void Tell(string name, double seconds)
        {
            tells.Add((stepNo, name, seconds));
            said.Reset("monolith");
            said.name = name;
            said.seconds = seconds;
            events.Emit("bossTell", said);
            kit.telegraph.Threat(body.position.x, body.position.y + b.coreHeight, body.position.z);
        }

        bool HurtPlayer(double amount, double px = 0, double py = 0, double pz = 0, bool push = false)
        {
            Health h = target.health;
            if (h == null || h.Dead) return false;
            bool hit = h.Damage(amount * scale.damage, "boss");
            if (hit && push) target.impulse?.Invoke(px, py, pz);
            return hit;
        }

        double FloorY => center.y; // the web's floorY(): the sunk floor is CUT in this slice
        double SurfaceY => center.y;

        // --- the start ---------------------------------------------------------------------------------------------------
        /// <summary>The kit starts the fight: it rises out of the floor at `center` (the round's HP scale applies).</summary>
        public void StartFight(BossFight ctx)
        {
            kit = ctx.kit;
            scale = ctx.scale;
            center = arenaCenter ?? ctx.center;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(b.hp * ctx.scale.hp);
            body.hp = body.maxHp;
            body.position = new Vec3(center.x, center.y - b.height, center.z);
            body.previous = body.position;
            body.yaw = 0;
            slams = 0;
            rise = 0;
            kneel = vulnerable = 0;
            rings.Clear();
            warns.Clear();
            surges.Clear();
            orbs.Clear();
            sweepOn = laserOn = false;
            age = 0;
            stepNo = 0;
            kneltAt3.Clear();
            comboDone = false;
            tells.Clear();
            foreach (string s in StatNames) stats[s] = 0;
            shownOn = false;
            shownTop = double.NaN;
            SetPhase("intro");
            said.Reset("monolith");
            said.name = "MONOLITH";
            said.hp = body.maxHp;
            said.round = ctx.round;
            events.Emit("bossRise", said);
        }

        /// <summary>Where the room's origin sits in the world, when it is not the fight context's centre.</summary>
        public Vec3? arenaCenter;

        public void Clear()
        {
            body.alive = false;
            phase = "dead";
            rings.Clear();
            warns.Clear();
            surges.Clear();
            orbs.Clear();
            UpdateLayer();
        }

        // --- the layer (boss.js updateLayer) -----------------------------------------------------------------------------------
        bool shownOn;
        double shownTop;
        Vec3 shownAt;
        readonly (Vec3 min, Vec3 max)[] oneBox = new (Vec3, Vec3)[1];
        static readonly (Vec3 min, Vec3 max)[] NoBox = new (Vec3, Vec3)[0];

        void UpdateLayer()
        {
            Vec3 p = body.position;
            bool on = body.alive && phase != "dying";
            double top = p.y + b.height * Math.Min(1, rise);
            if (on == shownOn && (!on || (p.x == shownAt.x && p.y == shownAt.y && p.z == shownAt.z && top == shownTop))) return;
            shownOn = on;
            shownAt = p;
            shownTop = top;
            if (on)
            {
                oneBox[0] = (new Vec3(p.x - b.bodyHalf, p.y - 2, p.z - b.bodyHalf), new Vec3(p.x + b.bodyHalf, top, p.z + b.bodyHalf));
                layer.SetBoxes(oneBox);
            }
            else layer.SetBoxes(NoBox);
        }

        /// <summary>While it rises: a player standing where it comes up is pushed out sideways (never carried to its top). The web moves the
        /// target; the port gives it a shove through the target's impulse.</summary>
        void PushOutOfBody()
        {
            Vec3 tp = target.position();
            double r = b.bodyHalf + b.capsuleRadius + 0.05;
            double dx = tp.x - body.position.x, dz = tp.z - body.position.z;
            if (Math.Abs(dx) >= r || Math.Abs(dz) >= r || tp.y > center.y + b.height) return;
            if (Math.Abs(dx) >= Math.Abs(dz)) target.impulse?.Invoke(dx < 0 ? -8 : 8, 0, 0);
            else target.impulse?.Invoke(0, 0, dz < 0 ? -8 : 8);
        }

        // --- attacks (boss.js, in order) -----------------------------------------------------------------------------------
        void StartSlam(int count, bool faster)
        {
            action = "slamTell";
            combo = count;
            actionSteps = S(faster ? b.slamTellFast : b.slamTell);
            warns.Add(new MonoWarn { x = body.position.x, z = body.position.z, y = FloorY, r = b.slamWarnRadius, steps = actionSteps, total = actionSteps });
            kit.telegraph.Circle(body.position.x, FloorY, body.position.z, b.slamWarnRadius, faster ? b.slamTellFast : b.slamTell);
            Tell("slam", faster ? b.slamTellFast : b.slamTell);
        }

        void SlamImpact(bool faster)
        {
            for (int i = 0; i < b.slamRings; i++) rings.Add(new MonoRing { r = b.bodyHalf + 0.5, delay = i * S(b.ringGap), speed = faster ? b.ringSpeedFast : b.ringSpeed, y = FloorY });
            slams++;
            stats["slams"]++;
            said.Reset("monolith");
            said.points = slams;
            events.Emit("bossSlam", said);
            kit.telegraph.Flash(body.position.x, FloorY + 0.3, body.position.z, 3);
        }

        void FireVolley()
        {
            Vec3 core = CoreWorld();
            Vec3 t = target.position();
            double dx = t.x - core.x, dz = t.z - core.z;
            double flat = JsMath.Hypot(dx, dz);
            if (flat == 0) flat = 1;
            double drop = (t.y + 1.2 - core.y) / flat;
            double baseA = JsMath.Atan2(dx, dz);
            for (int i = 0; i < b.volleyCount; i++)
            {
                double a = baseA + ((double)i / (b.volleyCount - 1) - 0.5) * b.volleyFanRad;
                Vec3 d = Vec3.Normalize(new Vec3(JsMath.Sin(a), drop, JsMath.Cos(a)));
                orbs.Add(new MonolithOrb { pos = core + d * (b.coreRadius + 0.5), vel = d * b.orbSpeed, life = S(b.orbLifetime) });
            }
            stats["volleys"]++;
            said.Reset("monolith");
            said.points = b.volleyCount;
            events.Emit("bossVolley", said);
        }

        void StartSurge()
        {
            Vec3 t = target.position();
            Vec3 v = playerVelocity != null ? playerVelocity() : Vec3.Zero;
            surges.Clear();
            for (int i = 0; i < b.surgeCount; i++)
            {
                double lead = i * b.surgeLead;
                double x = t.x + v.x * lead, z = t.z + v.z * lead;
                surges.Add(new MonoSurge { x = x, z = z, y = SurfaceY, steps = S(b.surgeTell) });
            }
            foreach (MonoSurge g in surges)
            {
                warns.Add(new MonoWarn { x = g.x, z = g.z, y = g.y, r = b.surgeRadius, steps = g.steps, total = g.steps });
                kit.telegraph.Circle(g.x, g.y, g.z, b.surgeRadius, b.surgeTell);
            }
            action = "surge";
            actionSteps = S(b.surgeTell + b.surgeRecover);
            stats["surges"]++;
            Tell("surge", b.surgeTell);
        }

        void StartSweep()
        {
            Vec3 t = target.position();
            double dx = t.x - body.position.x, dz = t.z - body.position.z;
            double len = JsMath.Hypot(dx, dz);
            if (len == 0) len = 1;
            sweepDx = dx / len;
            sweepDz = dz / len;
            sweepDist = Math.Min(b.sweepDistance, len + b.sweepOvershoot);
            sweepDone = 0;
            sweepHit = false;
            sweepOn = true;
            kit.telegraph.Line(body.position.x, FloorY, body.position.z, sweepDx, sweepDz, sweepDist, b.bodyHalf * 2, b.sweepTell);
            action = "sweepTell";
            actionSteps = S(b.sweepTell);
            stats["sweeps"]++;
            Tell("sweep", b.sweepTell);
        }

        void StartLaser()
        {
            Vec3 t = target.position();
            laserAngle = JsMath.Atan2(t.z - body.position.z, t.x - body.position.x) - b.laserArc / 2;
            laserCharge = S(b.laserTell);
            laserCool = 0;
            laserOn = true;
            action = "laser";
            actionSteps = S(b.laserTell + b.laserTime);
            stats["lasers"]++;
            Tell("laser", b.laserTell);
        }

        Capsule PlayerCapsule()
        {
            if (playerBody != null) return playerBody();
            Vec3 f = target.position();
            return new Capsule { start = new Vec3(f.x, f.y + b.capsuleRadius, f.z), end = new Vec3(f.x, f.y + b.capsuleHeight - b.capsuleRadius, f.z), radius = b.capsuleRadius };
        }

        static double Norm(double v) => (v % (2 * Math.PI) + 2 * Math.PI) % (2 * Math.PI);

        static bool AngleBetween(double a0, double a1, double x) => Norm(x - a0) <= Norm(a1 - a0) + 1e-6;

        void StepAttacks(double dt)
        {
            Vec3 tp = target.position();
            // Shockwave rings run along the surface: inside the band, low = 25; a jump clears it.
            double underYou = SurfaceY;
            foreach (MonoRing w in rings)
            {
                if (w.delay > 0)
                {
                    w.delay--;
                    continue;
                }
                w.r += w.speed * dt;
                double d = JsMath.Hypot(tp.x - body.position.x, tp.z - body.position.z);
                if (!w.hit && Math.Abs(d - w.r) <= b.ringWidth && tp.y < underYou + b.ringHeight)
                {
                    w.hit = true;
                    if (HurtPlayer(b.ringDamage)) stats["ringHits"]++;
                }
            }
            rings.RemoveAll(w => w.r >= b.ringMaxRadius);
            foreach (MonoWarn wn in warns) wn.steps--;
            warns.RemoveAll(wn => wn.steps <= 0);
            // Pillar surges erupt from the surface after their tell: 30 + a launch if you are on the circle.
            foreach (MonoSurge g in surges)
            {
                if (g.steps > 0 && --g.steps == 0)
                {
                    g.erupt = S(b.surgeErupt);
                    if (JsMath.Hypot(tp.x - g.x, tp.z - g.z) <= b.surgeRadius && tp.y < g.y + b.surgeHitHeight && HurtPlayer(b.surgeDamage, 0, b.surgeLaunch, 0, true)) stats["surgeHits"]++;
                    said.Reset("monolith");
                    said.position = new Vec3(g.x, g.y, g.z);
                    events.Emit("bossSurge", said);
                }
                else if (g.erupt > 0) g.erupt--;
            }
            // The sweep charge along its painted line.
            if (action == "sweep" && sweepOn)
            {
                double step = b.sweepSpeed * dt;
                sweepDone += step;
                body.position.x += sweepDx * step;
                body.position.z += sweepDz * step;
                if (!sweepHit && JsMath.Hypot(tp.x - body.position.x, tp.z - body.position.z) <= b.bodyHalf + 1 && tp.y < FloorY + b.height * 0.5)
                {
                    sweepHit = true;
                    if (HurtPlayer(b.sweepDamage, sweepDx * b.sweepPush, b.sweepPushUp, sweepDz * b.sweepPush, true)) stats["sweepHits"]++;
                }
            }
            // The laser: a rotating horizontal beam at chest height (jump or ride a wall over it).
            if (action == "laser" && laserOn)
            {
                if (laserCharge > 0) laserCharge--;
                else
                {
                    double prev = laserAngle;
                    laserAngle += b.laserArc / b.laserTime * dt;
                    if (laserCool > 0) laserCool--;
                    double pa = JsMath.Atan2(tp.z - body.position.z, tp.x - body.position.x);
                    bool crossed = AngleBetween(prev, laserAngle, pa);
                    double beamY = FloorY + b.laserHeight;
                    double d = JsMath.Hypot(tp.x - body.position.x, tp.z - body.position.z);
                    if (crossed && laserCool == 0 && d <= b.laserRange && tp.y < beamY && tp.y + b.capsuleHeight > beamY)
                    {
                        laserCool = S(b.laserHitCooldown);
                        if (HurtPlayer(b.laserDamage)) stats["laserHits"]++;
                    }
                }
            }
            // the boss's orbs (the web's orb pool: speed orbSpeed, config orb.*)
            Capsule c = PlayerCapsule();
            Vec3 mid = new Vec3((c.start.x + c.end.x) * 0.5, (c.start.y + c.end.y) * 0.5, (c.start.z + c.end.z) * 0.5);
            for (int i = orbs.Count - 1; i >= 0; i--)
            {
                MonolithOrb o = orbs[i];
                Vec3 prev = o.pos;
                o.pos = prev + o.vel * dt;
                bool dead = --o.life <= 0;
                if (!dead && world != null)
                {
                    double len = Vec3.DistanceTo(prev, o.pos);
                    if (len > 1e-9 && world.RayIntersect(prev, Vec3.Normalize(o.pos - prev), out RayHit hit) && hit.distance <= len) dead = true;
                }
                if (!dead)
                {
                    double rad = b.orbRadius + c.radius;
                    if (Vec3.DistanceTo(o.pos, c.start) <= rad || Vec3.DistanceTo(o.pos, mid) <= rad || Vec3.DistanceTo(o.pos, c.end) <= rad)
                    {
                        if (HurtPlayer(b.orbDamage)) stats["orbHits"]++;
                        dead = true;
                    }
                }
                if (dead)
                {
                    orbs[i] = orbs[orbs.Count - 1];
                    orbs.RemoveAt(orbs.Count - 1);
                }
            }
        }

        /// <summary>The phase's attack pattern: the next action when the last one is done (boss.js nextAction; the port's stream for Math.random).</summary>
        void NextAction()
        {
            double r = rng.Next();
            if (phase == "p1")
            {
                if (slams > 0 && slams % 3 == 0 && !kneltAt3.Contains(slams)) { Kneel(b.kneelTime); return; }
                if (r < 0.6) StartSlam(1, false);
                else Volley();
                return;
            }
            if (phase == "p2")
            {
                if (slams > 0 && slams % 3 == 0 && !kneltAt3.Contains(slams)) { Kneel(b.kneelTime); return; }
                if (r < 0.35) StartSlam(1, false);
                else if (r < 0.55) Volley();
                else if (r < 0.8) StartSurge();
                else StartSweep();
                return;
            }
            if (phase == "p3")
            {
                if (comboDone)
                {
                    comboDone = false;
                    Kneel(b.p3Window);
                    return;
                }
                if (r < 0.45) StartSlam(3, true);
                else if (r < 0.75) StartLaser();
                else Volley();
            }
        }

        void Volley()
        {
            action = "volleyTell";
            actionSteps = S(b.volleyTell);
            Tell("volley", b.volleyTell);
        }

        void Kneel(double seconds)
        {
            kneltAt3.Add(slams);
            action = "kneel";
            kneel = S(seconds);
            vulnerable = S(seconds);
            actionSteps = S(seconds);
            stats["kneels"]++;
            said.Reset("monolith");
            said.seconds = seconds;
            events.Emit("bossKneel", said);
        }

        // --- hits -------------------------------------------------------------------------------------------------------------
        /// <summary>boss.js raycast: the core sphere, else the body's vertical capsule (the core wins within 0.8 m of the body); hitPart names it.</summary>
        public double Raycast(Vec3 o, Vec3 d)
        {
            if (!body.alive || phase == "dying" || phase == "dead") return -1;
            double c = CombatMath.RaySphere(o, d, CoreWorld(), b.coreRadius);
            double bd = CombatMath.RayVerticalCapsule(o, d, body.position, b.height * Math.Min(1, rise), b.bodyHalf);
            if (c >= 0 && (bd < 0 || c <= bd + 0.8))
            {
                hitPart = "core";
                return c;
            }
            if (bd >= 0)
            {
                hitPart = "body";
                return bd;
            }
            return -1;
        }

        /// <summary>boss.js damage()'s first half: x0.5 on the body, the core x1.6 (x3 while it kneels). Blocked (nothing lands) in the intro, the midpoint
        /// and the finisher.</summary>
        public double Filter(double amount, out bool blocked)
        {
            blocked = false;
            if (!body.alive || phase == "intro" || phase == "dying" || phase == "mid")
            {
                blocked = true;
                return 0;
            }
            string part = hitPart ?? "body";
            hitPart = null;
            lastPart = part;
            if (part == "core") return amount * (vulnerable > 0 ? b.coreVulnerableMult : b.coreMult);
            return amount * b.bodyMult;
        }

        /// <summary>One hitscan shot: the ray, the part, the HP; the damage dealt (-1: a miss, 0: blocked).</summary>
        public double TakeHit(Vec3 origin, Vec3 dir, double amount, out double distance)
        {
            distance = Raycast(origin, dir);
            if (distance < 0) return -1;
            double dmg = Filter(amount, out bool blocked);
            if (blocked) return 0;
            Apply(dmg);
            return dmg;
        }

        /// <summary>boss.js damage()'s second half: the HP, bossHit, the midpoint's roar and beat at 66 %, phase 3 at 33 %, the finisher at 0.</summary>
        public void Apply(double dmg)
        {
            if (!body.alive) return;
            body.hp -= dmg;
            body.flash = 0.08;
            string part = lastPart ?? "body";
            stats[part == "core" ? "coreHits" : "bodyHits"]++;
            said.Reset("monolith");
            said.damage = JsMath.Round(dmg);
            said.part = part;
            said.core = part == "core";
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            if (phase == "p1" && HpFrac <= b.midAt)
            {
                body.hp = body.maxHp * b.midAt;
                SetPhase("mid");
                said.Reset("monolith");
                events.Emit("bossRoar", said);
                beatSteps = kit.Beat("monolith", "PHASE 2", new Vec3(body.position.x, body.position.y + b.coreHeight, body.position.z));
                action = "idle";
                sweepOn = laserOn = false;
            }
            else if (phase == "p2" && HpFrac <= b.p3At)
            {
                SetPhase("p3");
                kit.Beat("monolith", "PHASE 3", new Vec3(body.position.x, body.position.y + b.coreHeight, body.position.z));
            }
            else if (body.hp <= 0)
            {
                body.hp = 0;
                SetPhase("dying");
                killSteps = S(b.killTime);
                kit.Dying("monolith", CoreWorld());
            }
        }

        /// <summary>NIGHT25 W (design.n25.bosses): the phase a player's death restores ("p2": the midpoint's start, also a death inside the beat; "p3"), or null in the intro and phase 1.</summary>
        public string RetryPhase => !body.alive ? null : phase == "mid" || phase == "p2" ? "p2" : phase == "p3" ? "p3" : null;

        /// <summary>NIGHT25 W: a player death inside the boss room: back to the START of the phase (the HP at its threshold, every ring, warning, surge and orb gone, the sweep and
        /// the laser off); the seed stream continues. false: not in phase 2 or 3.</summary>
        public bool RestorePhaseStart()
        {
            string ph = RetryPhase;
            if (ph == null) return false;
            body.hp = body.maxHp * (ph == "p3" ? b.p3At : b.midAt);
            rings.Clear();
            warns.Clear();
            surges.Clear();
            orbs.Clear();
            sweepOn = laserOn = false;
            laserCharge = 0;
            kneel = vulnerable = 0;
            if (phase != ph) SetPhase(ph);
            else
            {
                action = "idle";
                actionSteps = S(b.actionGap);
            }
            return true;
        }

        public BossBar Bar() => new BossBar { valid = body.alive, id = body.id, name = "MONOLITH", phase = phase, hp = Math.Max(0, body.hp), max = body.maxHp, segment = b.midAt };

        // --- the step (boss.js step) --------------------------------------------------------------------------------------------
        public void Step(double dt)
        {
            if (!body.alive) return;
            body.previous = body.position;
            if (Frozen) return;
            stepNo++;
            age += dt;
            phaseSteps++;
            if (kneel > 0) kneel--;
            if (vulnerable > 0) vulnerable--;
            Vec3 t = target.position();
            if (action != "sweep") body.yaw = JsMath.Atan2(-(t.x - body.position.x), -(t.z - body.position.z));
            if (phase == "intro")
            {
                rise = Math.Min(1, (double)phaseSteps / S(b.introTime));
                body.position.y = center.y - b.height * (1 - rise);
                PushOutOfBody();
                if (phaseSteps >= S(b.introTime)) SetPhase("p1");
            }
            else if (phase == "mid")
            {
                if (phaseSteps >= S(b.midTime)) SetPhase("p2");
            }
            else if (phase == "dying")
            {
                if (--killSteps <= 0)
                {
                    body.alive = false;
                    phase = "dead";
                    kit.Down("monolith", body.id, CoreWorld());
                }
            }
            else if (phase == "p1" || phase == "p2" || phase == "p3")
            {
                if (actionSteps > 0) actionSteps--;
                if (actionSteps == 0)
                {
                    if (action == "slamTell")
                    {
                        SlamImpact(phase == "p3");
                        if (--combo > 0)
                        {
                            action = "slamTell";
                            actionSteps = S(b.slamTellFast);
                            warns.Add(new MonoWarn { x = body.position.x, z = body.position.z, y = FloorY, r = b.slamWarnRadius, steps = actionSteps, total = actionSteps });
                            Tell("slam", b.slamTellFast);
                        }
                        else
                        {
                            if (phase == "p3") comboDone = true;
                            action = "recover";
                            actionSteps = S(b.actionGap);
                        }
                    }
                    else if (action == "volleyTell")
                    {
                        FireVolley();
                        action = "recover";
                        actionSteps = S(b.actionGap);
                    }
                    else if (action == "sweepTell")
                    {
                        action = "sweep";
                        actionSteps = S(sweepDist / b.sweepSpeed);
                    }
                    else if (action == "sweep")
                    {
                        sweepOn = false;
                        action = "recover";
                        actionSteps = S(b.actionGap);
                    }
                    else
                    {
                        if (action == "laser") laserOn = false;
                        NextAction();
                    }
                }
                StepAttacks(dt);
            }
            double half = b.arenaHalf;
            body.position.x = JsMath.Max(center.x - half, JsMath.Min(center.x + half, body.position.x));
            body.position.z = JsMath.Max(center.z - half, JsMath.Min(center.z + half, body.position.z));
            UpdateLayer();
        }
    }
}
