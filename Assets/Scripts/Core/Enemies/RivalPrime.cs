// NIGHT25 builder W: RIVAL PRIME as a campaign duel (design.n25.bosses.rival; Research/2026-10-08-wave24/DESIGN24-bosses.md section 3: "the V2-style rival duel").
// The web's RIVAL PRIME (skygrind/src/bot/rivalPrime.js) is a mirror duelist: the same Mover as the player (dash, slide, jump) driven by the Duel RIVAL's brain
// (Core/Bot/BotBrain.cs, difficulty "hard"), plus telegraphed actions (a shape over its head for shapeTime, then the act). This is a compact port in one room with
// no adds: the SAME Mover and BotBrain (directChase: no nav graph, it slides along the cover pillars it runs into), the web's numbers from PortExport
// `rivalPrime.*` (hp 3000, p2At 0.5, copyMult 1.15, actionEvery, dashRange / dashHitRange / dashDamage, volleyCount / volleyFan / orbSpeed, the weights), and three
// telegraphed actions of the web's four: DASH (a ring tell, then a dash strike at where you stood), VOLLEY (a fan of orbs) and a BURST of hitscan shots (a line tell
// along the aim; this is the brain's own gunfire, gated so no shot lands without a tell). Not ported (named, not hidden): the SLAM (a leap and slam), the RETREAT
// diamond (the brain's own evade moves it), the magenta RAGE flare and the dash-cooldown hunt, the cover pillars' HP (the rockets), the ARCLINE and the copied weapon
// of phase 2 (phase 2 here: damage x copyMult and a quicker cadence), parrying the orbs. Every tell is at least minTell seconds (>= 0.5 s: the web's shapeTime is
// 0.45 s, raised). A death in the room restores the rival to the START of phase 2 (RestorePhaseStart), as the other bosses do. No UnityEngine.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class RivalParams
    {
        public double hp, p2At, copyMult, closeRange, dashRange, dashStrike, dashHitRange, dashDamage, dashPush, volleyFan, orbSpeed, orbLife, orbRadius, orbDamage;
        public int volleyCount, burstShots;
        public double[] actionEvery = new double[2];
        public double minTell, dashTell, volleyTell, burstTell, burstInterval, burstDamage, burstRange, burstCooldown, p2Cadence, killTime;
        public double height, radius, headRadius, eyeHeight, hpMult;
        public double spawnX, spawnZ;
        public string difficulty, name;
        public readonly Dictionary<string, Dictionary<string, double>> weights = new Dictionary<string, Dictionary<string, double>>();

        public static RivalParams From(GameConfig c)
        {
            const string w = "rivalPrime.", n = "design.n25.bosses.rival.";
            var p = new RivalParams
            {
                hp = c.Num(w + "hp"), p2At = c.Num(w + "p2At"), copyMult = c.Num(w + "copyMult"), closeRange = c.Num(w + "closeRange"), dashRange = c.Num(w + "dashRange"),
                dashStrike = c.Num(w + "dashStrike"), dashHitRange = c.Num(w + "dashHitRange"), dashDamage = c.Num(w + "dashDamage"),
                volleyCount = c.Int(w + "volleyCount"), volleyFan = c.Num(w + "volleyFan"), orbSpeed = c.Num(w + "orbSpeed"), difficulty = c.Str(w + "difficulty"),
                orbDamage = c.Num(n + "volley.damage"), orbLife = c.Num(n + "volley.life"), orbRadius = c.Num(n + "volley.radius"), dashPush = c.Num(n + "dash.push"),
                minTell = c.Num(n + "minTell"), dashTell = c.Num(n + "dash.tell"), volleyTell = c.Num(n + "volley.tell"), burstTell = c.Num(n + "burst.tell"),
                burstShots = c.Int(n + "burst.shots"), burstInterval = c.Num(n + "burst.interval"), burstDamage = c.Num(n + "burst.damage"), burstRange = c.Num(n + "burst.range"),
                burstCooldown = c.Num(n + "burst.cooldown"), p2Cadence = c.Num(n + "p2Cadence"), killTime = c.Num(n + "killTime"), hpMult = c.Num(n + "hpMult"),
                height = c.Num("body.capsuleHeight"), radius = c.Num("body.capsuleRadius"), headRadius = c.Num(n + "headRadius"), eyeHeight = c.Num("bot.eyeHeight"),
                name = c.Str(n + "name"),
            };
            double[] ae = c.Nums(w + "actionEvery");
            p.actionEvery[0] = ae[0];
            p.actionEvery[1] = ae[1];
            double[] sp = c.Nums(n + "spawn");
            p.spawnX = sp[0];
            p.spawnZ = sp[1];
            foreach (var kv in c.Obj(w + "weights"))
            {
                var row = new Dictionary<string, double>();
                foreach (var kv2 in PortData.Obj(kv.Value)) row[kv2.Key] = PortData.Num(kv2.Value);
                p.weights[kv.Key] = row;
            }
            return p;
        }
    }

    /// <summary>The rival as the enemies' registry sees it: the shots' target (a head sphere and a vertical capsule).</summary>
    public sealed class RivalBody : Enemy
    {
        public readonly RivalPrime system;

        public RivalBody(RivalPrime system)
        {
            this.system = system;
            kind = "rival";
        }

        public override double Height => system.p.height;
        public override double Radius => system.p.radius;
        public override double AimHeight => system.p.height * 0.65;

        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            Vec3 feet = system.mover.position;
            double head = CombatMath.RaySphere(o, d, new Vec3(feet.x, feet.y + system.p.height - system.p.headRadius, feet.z), system.p.headRadius);
            double body = CombatMath.RayVerticalCapsule(o, d, feet, system.p.height, system.p.radius);
            headshot = head >= 0 && (body < 0 || head <= body);
            return headshot ? head : body;
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

    public sealed class RivalOrb
    {
        public Vec3 pos, vel;
        public int life;
    }

    public sealed class RivalPrime : IBossSystem
    {
        public readonly RivalParams p;
        public readonly RivalBody body;
        public readonly Mover mover;
        readonly GameConfig cfg;
        readonly EventBus events;
        readonly ICollisionWorld world;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly BossEvent said = new BossEvent();
        readonly BotBrain brain;
        readonly BotDeps deps;
        readonly BotTarget perceived = new BotTarget();
        readonly Health dummy;
        readonly double dt0;
        Mulberry32 rng = new Mulberry32(1);
        BossKit kit;
        BossScale scale = BossScale.One;
        Vec3 center, lastPlayer;
        bool havePlayer;
        public Vec3? arenaCenter;
        public Func<Capsule> playerBody;
        public Func<Vec3> playerVelocity;

        public string phase = "dead", action = "idle";
        public int phaseSteps, actionSteps, stepNo, shotsLeft, shotGap, beatSteps, dieSteps, cooldown, dashLeft;
        bool dashHit;
        Vec3 lockedDir, lockedPoint;
        public readonly List<RivalOrb> orbs = new List<RivalOrb>();
        public readonly List<(int step, string name, double seconds, bool resolve)> log = new List<(int, string, double, bool)>();
        public readonly Dictionary<string, int> stats = new Dictionary<string, int>();
        static readonly string[] StatNames = { "dashes", "dashHits", "volleys", "orbHits", "bursts", "shotsHit", "bodyHits", "headHits" };
        string pending;
        string lastPart;
        public bool Frozen { get; set; }
        public bool Alive => body.alive;
        public Vec3 Center => center;
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;

        public RivalPrime(GameConfig cfg, EventBus events, ICollisionWorld world, BossTarget target, Func<int> nextId)
        {
            this.cfg = cfg;
            p = RivalParams.From(cfg);
            this.events = events;
            this.world = world;
            this.target = target;
            this.nextId = nextId;
            dt0 = cfg.Dt;
            body = new RivalBody(this);
            mover = new Mover(cfg.movement, world);
            dummy = new Health(1e9, new EventBus(), "rival"); // the brain's own HP (its RECOVER state reads it): never low, the boss's HP is body.hp
            deps = new BotDeps { getPlayer = () => perceived, ray = world.RayIntersect, getGraph = () => null, rng = rng };
            brain = new BotBrain(cfg, new BotSelf { mover = mover, health = dummy, weapon = () => "revolver" }, deps) { directChase = true };
            foreach (string s in StatNames) stats[s] = 0;
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        double Tell(double sec) => Math.Max(sec, p.minTell); // every tell is at least minTell seconds (>= 0.5 s)

        public void NewRun(uint seed)
        {
            rng = new Mulberry32(seed);
            deps.rng = rng;
        }

        public uint RngState
        {
            get => rng.State;
            set => rng.State = value;
        }

        Vec3 Spawn => new Vec3(center.x + p.spawnX, center.y, center.z + p.spawnZ);

        public void StartFight(BossFight ctx)
        {
            kit = ctx.kit;
            scale = ctx.scale;
            center = arenaCenter ?? ctx.center;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(p.hp * p.hpMult * ctx.scale.hp);
            body.hp = body.maxHp;
            body.flash = 0;
            Vec3 sp = Spawn;
            mover.Reset(sp.x, sp.y, sp.z);
            body.position = mover.position;
            body.previous = body.position;
            brain.look.yaw = Math.PI; // facing +z, into the room
            brain.look.pitch = 0;
            brain.Reset(p.difficulty);
            action = "idle";
            pending = null;
            orbs.Clear();
            log.Clear();
            foreach (string s in StatNames) stats[s] = 0;
            cooldown = S(p.actionEvery[1]);
            actionSteps = 0;
            shotsLeft = 0;
            dashLeft = 0;
            havePlayer = false;
            stepNo = 0;
            SetPhase("intro");
            said.Reset("rival");
            said.name = p.name;
            said.hp = body.maxHp;
            said.round = ctx.round;
            events.Emit("bossRise", said);
        }

        void SetPhase(string ph)
        {
            phase = ph;
            phaseSteps = 0;
            said.Reset("rival");
            said.phase = ph;
            said.hp = body.hp;
            said.maxHp = body.maxHp;
            events.Emit("bossPhase", said);
        }

        public void Clear()
        {
            body.alive = false;
            phase = "dead";
            orbs.Clear();
            action = "idle";
        }

        /// <summary>The kit finished its intro card; the part's multiplier is the weapon's (headshot included): only the intro, the beat and the finisher block.</summary>
        public double Filter(double amount, bool headshot, out bool blocked)
        {
            blocked = !body.alive || phase == "intro" || phase == "beat" || phase == "dying";
            lastPart = headshot ? "head" : "body";
            return blocked ? 0 : amount;
        }

        public void Apply(double dmg)
        {
            if (!body.alive) return;
            body.hp -= dmg;
            body.flash = 0.06;
            stats[lastPart == "head" ? "headHits" : "bodyHits"]++;
            said.Reset("rival");
            said.damage = JsMath.Round(dmg);
            said.part = lastPart ?? "body";
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            if (phase == "p1" && HpFrac <= p.p2At)
            {
                body.hp = body.maxHp * p.p2At;
                SetPhase("beat");
                beatSteps = kit.Beat("rival", "PHASE 2", new Vec3(mover.position.x, mover.position.y + 2.6, mover.position.z));
                action = "idle";
                pending = null;
                orbs.Clear();
            }
            else if (body.hp <= 0)
            {
                body.hp = 0;
                SetPhase("dying");
                dieSteps = S(p.killTime);
                kit.Dying("rival", new Vec3(mover.position.x, mover.position.y + 1, mover.position.z));
            }
        }

        /// <summary>The phase a player's death restores ("p2", also inside the beat), or null in the intro and phase 1.</summary>
        public string RetryPhase => !body.alive ? null : phase == "beat" || phase == "p2" ? "p2" : null;

        /// <summary>A death inside the room: back to the START of phase 2 (HP at p2At, no action under way, the orbs gone, the rival at its spawn), the stream continues.</summary>
        public bool RestorePhaseStart()
        {
            if (RetryPhase == null) return false;
            body.hp = body.maxHp * p.p2At;
            orbs.Clear();
            action = "idle";
            pending = null;
            Vec3 sp = Spawn;
            mover.Reset(sp.x, sp.y, sp.z);
            brain.ClearPath();
            cooldown = S(p.actionEvery[1]);
            if (phase != "p2") SetPhase("p2");
            return true;
        }

        public BossBar Bar() => new BossBar { valid = body.alive, id = body.id, name = p.name, phase = phase, hp = Math.Max(0, body.hp), max = body.maxHp, segment = p.p2At, pips = 0, pipsOn = 0 };

        double DamageMult => phase == "p2" || (phase == "beat") ? p.copyMult : 1.0;

        Vec3 Eye => new Vec3(mover.position.x, mover.position.y + p.eyeHeight, mover.position.z);

        Vec3 PlayerChest()
        {
            Capsule c = playerBody != null ? playerBody() : default;
            if (playerBody != null) return new Vec3((c.start.x + c.end.x) * 0.5, (c.start.y + c.end.y) * 0.5, (c.start.z + c.end.z) * 0.5);
            Vec3 f = target.position();
            return new Vec3(f.x, f.y + p.height * 0.55, f.z);
        }

        string PickAction(double dist)
        {
            string cls = dist < p.closeRange ? "close" : dist < p.dashRange ? "mid" : "far";
            var row = p.weights[cls];
            // the web's four actions: slam and retreat are not ported here: their weight goes to the burst (the brain's own gunfire)
            double dash = row.TryGetValue("dash", out double d) ? d : 0, volley = row.TryGetValue("volley", out double v) ? v : 0;
            double burst = (row.TryGetValue("slam", out double s) ? s : 0) + (row.TryGetValue("retreat", out double r) ? r : 0);
            double total = dash + volley + burst, roll = rng.Next() * total;
            if (roll < dash) return "dash";
            if (roll < dash + volley) return "volley";
            return "burst";
        }

        void Begin(string name, double tellSec)
        {
            action = name;
            pending = name;
            actionSteps = S(Tell(tellSec));
            Vec3 f = target.position();
            lockedPoint = new Vec3(f.x, mover.position.y, f.z);
            double dx = lockedPoint.x - mover.position.x, dz = lockedPoint.z - mover.position.z, len = JsMath.Hypot(dx, dz);
            if (len < 1e-6) { dx = 0; dz = -1; len = 1; }
            lockedDir = new Vec3(dx / len, 0, dz / len);
            log.Add((stepNo, name, Tell(tellSec), false));
            said.Reset("rival");
            said.label = name;
            said.seconds = Tell(tellSec);
            events.Emit("bossTell", said);
            if (name == "dash") kit.telegraph.Line(mover.position.x, center.y, mover.position.z, lockedDir.x, lockedDir.z, Math.Min(p.dashRange, len + 3), 2.0, Tell(tellSec));
            else if (name == "burst") kit.telegraph.Line(mover.position.x, center.y, mover.position.z, lockedDir.x, lockedDir.z, p.burstRange, 0.8, Tell(tellSec));
            else kit.telegraph.Circle(lockedPoint.x, center.y, lockedPoint.z, 2.0, Tell(tellSec));
        }

        void Resolve()
        {
            log.Add((stepNo, action, 0, true));
            if (action == "dash")
            {
                stats["dashes"]++;
                double speed = cfg.movement.dash.speed;
                mover.Impulse(lockedDir.x * speed, 0, lockedDir.z * speed);
                dashLeft = S(p.dashStrike);
                dashHit = false;
                kit.telegraph.Flash(mover.position.x, mover.position.y + 1, mover.position.z, 1.6);
            }
            else if (action == "volley")
            {
                stats["volleys"]++;
                Vec3 eye = Eye, aim = PlayerChestLocked();
                double bx = aim.x - eye.x, bz = aim.z - eye.z, bl = JsMath.Hypot(bx, bz);
                if (bl < 1e-6) { bx = 0; bz = -1; bl = 1; }
                double baseA = JsMath.Atan2(bz, bx);
                for (int i = 0; i < p.volleyCount; i++)
                {
                    double f = p.volleyCount == 1 ? 0 : (double)i / (p.volleyCount - 1) - 0.5;
                    double a = baseA + f * p.volleyFan;
                    double dy = (aim.y - eye.y) / bl;
                    var dir = Vec3.Normalize(new Vec3(JsMath.Cos(a), dy, JsMath.Sin(a)));
                    orbs.Add(new RivalOrb { pos = eye, vel = dir * p.orbSpeed, life = S(p.orbLife) });
                }
                kit.telegraph.Flash(eye.x, eye.y, eye.z, 1.4);
            }
            else if (action == "burst")
            {
                stats["bursts"]++;
                shotsLeft = p.burstShots;
                shotGap = 0;
            }
            action = action == "burst" ? "burstFire" : "idle";
            pending = null;
            if (action == "idle") cooldown = CooldownSteps();
        }

        Vec3 PlayerChestLocked() => new Vec3(lockedPoint.x, PlayerChest().y, lockedPoint.z);

        int CooldownSteps()
        {
            double lo = p.actionEvery[0], hi = p.actionEvery[1];
            double sec = lo + (hi - lo) * rng.Next();
            if (phase == "p2") sec *= p.p2Cadence;
            return S(sec);
        }

        void FireShot()
        {
            Vec3 eye = Eye;
            double yaw = brain.look.yaw, pitch = brain.look.pitch;
            double cp = JsMath.Cos(pitch);
            var d = new Vec3(-JsMath.Sin(yaw) * cp, JsMath.Sin(pitch), -JsMath.Cos(yaw) * cp);
            Vec3 feet = target.position();
            Capsule c = playerBody != null ? playerBody() : default;
            double hit = playerBody != null
                ? CombatMath.RayVerticalCapsule(eye, d, new Vec3(c.start.x, c.start.y - c.radius, c.start.z), c.end.y - c.start.y + 2 * c.radius, c.radius)
                : CombatMath.RayVerticalCapsule(eye, d, feet, p.height, p.radius);
            if (hit >= 0 && hit <= p.burstRange && !(world.RayIntersect(eye, d, out RayHit wall) && wall.distance < hit))
                if (Hurt(p.burstDamage)) stats["shotsHit"]++;
            kit.telegraph.Flash(eye.x + d.x, eye.y + d.y, eye.z + d.z, 0.5);
        }

        bool Hurt(double amount)
        {
            Health h = target.health;
            if (h == null || h.Dead) return false;
            return h.Damage(amount * DamageMult * scale.damage, "boss");
        }

        public void Step(double dt)
        {
            if (!body.alive) return;
            body.previous = body.position;
            if (Frozen) return;
            stepNo++;
            phaseSteps++;
            // the brain perceives the player
            Vec3 pf = target.position();
            perceived.position = pf;
            perceived.velocity = havePlayer ? new Vec3((pf.x - lastPlayer.x) / dt0, (pf.y - lastPlayer.y) / dt0, (pf.z - lastPlayer.z) / dt0) : default;
            if (playerVelocity != null) perceived.velocity = playerVelocity();
            perceived.dead = target.health != null && target.health.Dead;
            perceived.crouched = false;
            lastPlayer = pf;
            havePlayer = true;
            var frame = new BotFrame();
            if (phase == "intro" || phase == "beat" || phase == "dying")
            {
                // standing: the card, the beat and the finisher
                frame.move.yaw = brain.look.yaw;
                if (phase == "intro" && phaseSteps >= kit.IntroSteps()) SetPhase("p1");
                else if (phase == "beat" && phaseSteps >= beatSteps) SetPhase("p2");
                else if (phase == "dying" && --dieSteps <= 0)
                {
                    body.alive = false;
                    phase = "dead";
                    kit.Down("rival", body.id, mover.position);
                }
            }
            else brain.Step(ref frame, dt); // dash, slide, jump, aim: the mirror duelist
            frame.fire = default; // its gunfire is the burst's: gated by a tell
            mover.Step(frame.move, dt);
            if (mover.position.y < center.y - 20)
            {
                Vec3 sp = Spawn;
                mover.Reset(sp.x, sp.y, sp.z);
                brain.ClearPath();
            }
            body.position = mover.position;
            body.velocity = mover.velocity;
            body.yaw = brain.look.yaw;
            if (!body.alive) return;
            // the telegraphed actions
            if (phase == "p1" || phase == "p2")
            {
                if (action == "idle")
                {
                    if (--cooldown <= 0)
                    {
                        double dist = JsMath.Hypot(pf.x - mover.position.x, pf.z - mover.position.z);
                        string next = PickAction(dist);
                        Begin(next, next == "dash" ? p.dashTell : next == "volley" ? p.volleyTell : p.burstTell);
                    }
                }
                else if (action == "dash" || action == "volley" || action == "burst")
                {
                    if (--actionSteps <= 0) Resolve();
                }
                else if (action == "burstFire")
                {
                    if (shotsLeft <= 0) { action = "idle"; cooldown = S(p.burstCooldown) + CooldownSteps() / 2; }
                    else if (--shotGap <= 0)
                    {
                        FireShot();
                        shotsLeft--;
                        shotGap = S(p.burstInterval);
                    }
                }
                if (dashLeft > 0)
                {
                    dashLeft--;
                    if (!dashHit)
                    {
                        Capsule c = playerBody != null ? playerBody() : default;
                        Vec3 ch = PlayerChest();
                        if (JsMath.Hypot(ch.x - mover.position.x, ch.z - mover.position.z) <= p.dashHitRange && Math.Abs(ch.y - (mover.position.y + p.height * 0.5)) < 2.2)
                        {
                            dashHit = true;
                            if (Hurt(p.dashDamage)) stats["dashHits"]++;
                            double dx = ch.x - mover.position.x, dz = ch.z - mover.position.z, len = JsMath.Hypot(dx, dz);
                            if (len < 1e-6) { dx = lockedDir.x; dz = lockedDir.z; len = 1; }
                            target.impulse?.Invoke(dx / len * p.dashPush, 3, dz / len * p.dashPush);
                        }
                    }
                }
            }
            StepOrbs(dt);
        }

        void StepOrbs(double dt)
        {
            if (orbs.Count == 0) return;
            Vec3 ch = PlayerChest();
            for (int i = orbs.Count - 1; i >= 0; i--)
            {
                RivalOrb o = orbs[i];
                Vec3 prev = o.pos;
                o.pos = new Vec3(prev.x + o.vel.x * dt, prev.y + o.vel.y * dt, prev.z + o.vel.z * dt);
                bool dead = --o.life <= 0;
                if (!dead)
                {
                    double len = Vec3.DistanceTo(prev, o.pos);
                    if (len > 1e-9 && world.RayIntersect(prev, Vec3.Normalize(o.pos - prev), out RayHit hit) && hit.distance <= len) dead = true;
                }
                if (!dead && Vec3.DistanceTo(o.pos, ch) <= p.orbRadius + p.radius)
                {
                    if (Hurt(p.orbDamage)) stats["orbHits"]++;
                    dead = true;
                }
                if (dead)
                {
                    orbs[i] = orbs[orbs.Count - 1];
                    orbs.RemoveAt(orbs.Count - 1);
                }
            }
        }
    }
}
