// NIGHT37 builder S (stag37; NIGHT37.md S1, BOSSES.md "L2 THE STAG"): THE STAG from scratch, a CHASE round THE CANAL (Stag37Arena) on K's Boss37
// brain (Core/Boss37.cs). Pure: no UnityEngine, fixed 120 Hz, whole-step timers, Mulberry32 (its lane weave), JsMath.
//   THE PATH FOLLOWER  it runs the loop's path coordinate s (Stag37Arena: s along the centre line, lat outward) at run.speed on the straights and
//                      run.turnSpeed in the turns, eased by accel / decel; its lane weaves (laneEvery, Mulberry32; never through a lane wall);
//                      slower while it attacks (attackSlow) or stumbles; the rubber band: past farGap it eases to farSpeed, past leash it trots.
//   THE SLIPSTREAM     InSlip: the player is behind it within slip.range on its line (laneHalf) and on the ice: the sim pushes his skating speed
//                      toward slip.base x (1 + gain). Sitting in it past slip.habitSeconds forces the KICK BACK.
//   DAMAGE             catch it (catch.range of its hind legs, skating at least catch.minSpeed) and punch: a leg punch (stagger -2, a stumble).
//                      Stagger empty (Boss37's CYAN window): it falls and skids; a LAUNCH punch (within launchGrace of a dash / slide / ride)
//                      drives it into the canal wall: launchDamage in full (a leg's worth: Boss37's autoLegs move to the next leg). Guns chip 10 %.
//   ATTACKS            shard kick (RED: the lane behind it), ice chunk (YELLOW: punch it back = a parry, stagger -2 and a stumble), lane cut
//                      (RED: a barrier across your lane behind it, the sim's layer), kick back (RED: the slipstream habit / the camp answer).
//   LEG 3              it bolts to the tunnel's south mouth, turns, and the antler charge (YELLOW 1.5 s) comes head-on down your lane: parry it
//                      (punch its antlers) and it crashes into the tunnel wall: the kill. A missed charge: it turns toward you and charges again.
// Events for GG's sounds (no audio): stag37Bolt, stag37LegPunch, stag37Skid, stag37Launch, stag37Shards, stag37Chunk, stag37Cut, stag37Kick,
// stag37Charge, stag37Crash; Boss37's own (boss37Tell / Attack / Rest / Stagger / Window / Leg); bossHit / bossDying / bossDown.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Stag37Params
    {
        public string name;
        public double pool, revealSeconds, startAhead;
        public double height, radius, length, hindBack, hindY;
        public double passBy, runSpeed, turnSpeed, farGap, farShare, slipShare, leash, leashSpeed, accel, decel, attackSlow, laneShift, laneEvery;
        public double slipRange, slipGain, slipBase, slipAccel, slipLaneHalf, slipHabit, slipWindow, floeLead;
        public double catchRange, catchMinSpeed, catchStagger, catchDamage, catchCooldown, catchRebound, stumbleSeconds, stumbleSpeed;
        public double skidSeconds, launchReach, launchGrace, launchDamage, windowPunchDamage;
        public double shardRange, shardDamage, shardHeight;
        public double chunkFlight, chunkGravity, chunkRadius, chunkDamage, chunkParryReach, chunkParryCone, chunkReturn, chunkParryDamage, chunkStagger, chunkBuffer;
        public double cutHeight, cutThick, cutSeconds, cutBehind;
        public double kickRange, kickDamage, kickKnock, kickKnockUp;
        public double chargeRunSpeed, chargeSpeed, chargeParryReach, chargeHitRadius, chargeDamage, chargeKnock, chargeCrash, chargeBuffer;
        public double floeEvery, floeTell, floeDown, floeAhead, fallY, fallCost;
        public double pacerSpeed, pacerStartSpeed, pacerWait;
        public double capsuleHeight, capsuleRadius;

        public const string Row = "design.n37.stag37.";

        public static Stag37Params From(GameConfig c)
        {
            double N(string k) => c.Num(Row + k);
            return new Stag37Params
            {
                name = c.Str(Row + "name"), pool = N("pool"), revealSeconds = N("revealSeconds"), startAhead = N("startAhead"),
                height = N("body.height"), radius = N("body.radius"), length = N("body.length"), hindBack = N("body.hindBack"), hindY = N("body.hindY"),
                passBy = N("run.passBy"), runSpeed = N("run.speed"), turnSpeed = N("run.turnSpeed"), farGap = N("run.farGap"), farShare = N("run.farShare"), slipShare = N("run.slipShare"), leash = N("run.leash"),
                leashSpeed = N("run.leashSpeed"), accel = N("run.accel"), decel = N("run.decel"), attackSlow = N("run.attackSlow"), laneShift = N("run.laneShift"),
                laneEvery = N("run.laneEvery"),
                slipRange = N("slip.range"), slipGain = N("slip.gain"), slipBase = N("slip.base"), slipAccel = N("slip.accel"), slipLaneHalf = N("slip.laneHalf"),
                slipHabit = N("slip.habitSeconds"), slipWindow = N("slip.habitWindow"), floeLead = N("floes.leadSeconds"),
                catchRange = N("catch.range"), catchMinSpeed = N("catch.minSpeed"), catchStagger = N("catch.stagger"), catchDamage = N("catch.damage"),
                catchCooldown = N("catch.cooldown"), catchRebound = N("catch.reboundSeconds"), stumbleSeconds = N("catch.stumbleSeconds"), stumbleSpeed = N("catch.stumbleSpeed"),
                skidSeconds = N("window.skidSeconds"), launchReach = N("window.launchReach"), launchGrace = N("window.launchGrace"),
                launchDamage = N("window.launchDamage"), windowPunchDamage = N("window.punchDamage"),
                shardRange = N("shard.range"), shardDamage = N("shard.damage"), shardHeight = N("shard.height"),
                chunkFlight = N("chunk.flight"), chunkGravity = N("chunk.gravity"), chunkRadius = N("chunk.radius"), chunkDamage = N("chunk.damage"),
                chunkParryReach = N("chunk.parryReach"), chunkParryCone = N("chunk.parryCone"), chunkReturn = N("chunk.returnSpeed"),
                chunkParryDamage = N("chunk.parryDamage"), chunkStagger = N("chunk.stagger"), chunkBuffer = N("chunk.bufferSeconds"),
                cutHeight = N("cut.height"), cutThick = N("cut.thick"), cutSeconds = N("cut.seconds"), cutBehind = N("cut.behind"),
                kickRange = N("kick.range"), kickDamage = N("kick.damage"), kickKnock = N("kick.knock"), kickKnockUp = N("kick.knockUp"),
                chargeRunSpeed = N("charge.runSpeed"), chargeSpeed = N("charge.speed"), chargeParryReach = N("charge.parryReach"),
                chargeHitRadius = N("charge.hitRadius"), chargeDamage = N("charge.damage"), chargeKnock = N("charge.knock"), chargeCrash = N("charge.crashSeconds"), chargeBuffer = N("charge.bufferSeconds"),
                floeEvery = N("floes.every"), floeTell = N("floes.tell"), floeDown = N("floes.down"), floeAhead = N("floes.ahead"),
                fallY = N("fallY"), fallCost = N("fallCost"),
                pacerSpeed = N("pacer.speed"), pacerStartSpeed = N("pacer.startSpeed"), pacerWait = N("pacer.waitSeconds"),
                capsuleHeight = c.Num("body.capsuleHeight"), capsuleRadius = c.Num("body.capsuleRadius"),
            };
        }
    }

    public sealed class Stag37Body : Enemy, IBossBody27
    {
        public readonly Stag37 system;

        public Stag37Body(Stag37 system)
        {
            this.system = system;
            kind = "stag";
        }

        public override double Height => system.p.height;
        public override double Radius => system.p.radius;
        public override double AimHeight => system.p.height * 0.65;
        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            headshot = false;
            return system.Raycast(o, d);
        }
        public override bool Damage(double amount, double flashTime)
        {
            system.Apply(amount, "hit");
            return false;
        }
        public double Filter(double amount, string weapon, Vec3 from, out bool blocked) => system.Filter(amount, weapon, out blocked);
        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt) { }
        public override void Stagger(double time, double dt) { }
        public override void Stun(double time, double dt) { }
    }

    /// <summary>The ice chunk (one at a time).</summary>
    public sealed class Stag37Chunk
    {
        public Vec3 pos, prev, vel;
        public int life;
        public bool alive, back;
    }

    /// <summary>The payload of the stag37 events (pooled: read it, don't keep it).</summary>
    public sealed class Stag37Event
    {
        public string stage;
        public int lane, count;
        public double s;
        public Vec3 at;
    }

    public enum Stag37Leg3 : byte { None, Run, Turned, Charge, Crash }

    public sealed class Stag37 : IBossSystem
    {
        public const string ShardKick = "stagShardKick", IceChunk = "stagIceChunk", LaneCut = "stagLaneCut", KickBack = "stagKickBack", AntlerCharge = "stagAntlerCharge";
        public readonly Stag37Params p;
        public readonly Stag37Arena arena;
        public readonly Stag37Body body;
        /// <summary>K's brain: built again at every fight's start with that run's seed (rule 6: per-run randomness).</summary>
        public Boss37Brain brain;
        readonly GameConfig cfg;
        readonly Tells36 tellsReg;
        /// <summary>The run's seed for this fight (the sim sets it before StartFight: runSeed mixed with design.n37.stag37.salt).</summary>
        public uint seed = 1;
        readonly EventBus events;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt0;
        readonly BossEvent said = new BossEvent();
        readonly Stag37Event ev = new Stag37Event();
        readonly HitEvent parried = new HitEvent();
        Mulberry32 rng;
        BossKit kit;

        /// <summary>The canal's origin (world) and the player's readers (the sim sets them).</summary>
        public Vec3 origin;
        public Func<Vec3> playerVelocity;
        public Func<bool> playerGrounded, playerRiding, playerSpeedVerb;
        public Func<Capsule> playerBody;
        /// <summary>A returned chunk reached it: the sim lands `damage` as a parry hit (Filter takes it in full).</summary>
        public Action<double, Vec3> parryHit;
        /// <summary>The lane cut drops a barrier (arena-local box) / the sim clears them all.</summary>
        public Action<Vec3, Vec3> dropBarrier;

        // the path state
        public double s, lat, speed, dir = 1;
        public int laneTarget;
        public Vec3 heading = new Vec3(0, 0, -1);
        /// <summary>The slipstream habit: steps in the slipstream over the last slip.habitWindow seconds (a ring of steps).</summary>
        bool[] slipRing = new bool[1];
        int slipRingAt;
        public int slipRecent, slipRecentMax, kickForces, kickTells;
        /// <summary>The rubber band is holding it back (the player fell past farGap behind; until he is in its slipstream again).</summary>
        public bool waiting;
        public int waits;
        public int stumbleLeft, laneLeft, slipSteps, stepNo, legPunchCool, lastSpeedVerbStep = -100000, lastPunchStep = -100000, crashLeft;
        bool punchUsed;
        Vec3 punchEye, punchDir;
        public Stag37Leg3 leg3;
        public readonly Stag37Chunk chunk = new Stag37Chunk();
        /// <summary>The player on the path this step: s, lat, the signed gap (positive: the Stag is ahead), lane.</summary>
        public double ps, plat, gap;
        public int playerLane;
        /// <summary>The player is in the slipstream now (the sim pushes; the view draws the streak).</summary>
        public bool InSlip;
        /// <summary>The shard spray: its lane, its s span (s1 at the Stag's hind legs, s0 shardRange behind), live during ACTIVE.</summary>
        public int shardLane;
        public double shardS0, shardS1;
        /// <summary>The lane cut's spot (the tell's RED stripe follows it) and the lane.</summary>
        public double cutS;
        public int cutLane;
        bool hitThisAttack;
        public bool resume;

        // stats (harness, reels)
        public int legPunches, launches, chunkParries, chunkHits, shardHits, kickHits, chargeHits, chargeParries, charges, cuts, kicks, windowPunches, stumbles, skids;
        public double dealt, slipSeconds, dealtStill;

        public Stag37(GameConfig cfg, Stag37Params p, Stag37Arena arena, EventBus events, BossTarget target, Func<int> nextId, Tells36 tells)
        {
            this.p = p;
            this.arena = arena;
            this.events = events;
            this.target = target;
            this.nextId = nextId;
            dt0 = cfg.Dt;
            body = new Stag37Body(this);
            this.cfg = cfg;
            tellsReg = tells;
            NewBrain();
        }

        void NewBrain()
        {
            brain = new Boss37Brain(Boss37Params.From(cfg, "stag37"), "stag37", dt0, events, seed) { tells = tellsReg };
            foreach (string k in new[] { "shardKick", "iceChunk", "laneCut", "kickBack", "antlerCharge" })
                brain.attacks.Add(Boss37Attack.From(cfg, "design.n37.stag37.attacks." + k, dt0));
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public double HpFrac => body.maxHp > 0 ? Math.Max(0, body.hp) / body.maxHp : 0;
        public bool Hurtable => body.alive && brain.Fighting;
        public string AttackId => brain.Current?.id;
        public bool Telling(string id) => brain.state == Boss37State.Tell && AttackId == id;
        public bool Acting(string id) => brain.state == Boss37State.Active && AttackId == id;
        /// <summary>The body's feet (world), its centre (the torso), its hind legs' target, its antlers' tip (the charge's head).</summary>
        public Vec3 Feet => arena.PointAt(s, lat) + origin;
        public Vec3 Facing => heading * dir;
        public Vec3 Centre => Feet + new Vec3(0, p.height * 0.62, 0);
        public Vec3 Hind => Feet - Facing * p.hindBack + new Vec3(0, p.hindY, 0);
        public Vec3 Head => Feet + Facing * (p.length * 0.5 + 0.6) + new Vec3(0, p.height * 0.8, 0);

        public void StartFight(BossFight ctx)
        {
            if (resume) { kit = ctx.kit; return; }
            kit = ctx.kit;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(p.pool * ctx.scale.hp);
            body.hp = body.maxHp;
            Tells36 tl = brain.tells;
            NewBrain();
            brain.tells = tl ?? tellsReg;
            rng = new Mulberry32(seed * 7919u + 37u);
            Array.Clear(slipRing, 0, slipRing.Length);
            slipRingAt = 0;
            slipRecent = 0;
            stepNo = 0;
            leg3 = Stag37Leg3.None;
            chunk.alive = false;
            stumbleLeft = slipSteps = legPunchCool = crashLeft = waits = 0;
            waiting = false;
            laneLeft = S(p.laneEvery);
            dir = 1;
            speed = 0;
            legPunches = launches = chunkParries = chunkHits = shardHits = kickHits = chargeHits = chargeParries = charges = cuts = kicks = windowPunches = stumbles = skids = 0;
            dealt = dealtStill = slipSeconds = 0;
            punchUsed = true;
            brain.source = body.id;
            Vec3 pw = target.position() - origin;
            arena.Project(pw.x, pw.z, out double s0, out double lat0);
            Place(s0 + p.startAhead, arena.LaneLat(1));
            brain.SetCheckpoint(1, target.position(), 0);
            brain.Begin(S(p.revealSeconds), target.position());
            said.Reset("stag37");
            said.name = p.name;
            said.hp = body.maxHp;
            events.Emit("bossRise", said);
        }

        /// <summary>Put the Stag at (s, lat), standing.</summary>
        public void Place(double sNew, double latNew)
        {
            s = arena.Wrap(sNew);
            lat = latNew;
            laneTarget = arena.LaneOf(lat);
            speed = 0;
            arena.PointAt(s, lat, out heading);
            body.position = Feet;
            body.previous = body.position;
        }

        public void Clear()
        {
            if (resume) return;
            body.alive = false;
            chunk.alive = false;
            brain.tells?.End(brain.source);
            brain.Reset();
        }

        public BossBar Bar() => new BossBar
        {
            valid = body.alive, id = body.id, name = p.name, phase = brain.state.ToString(), hp = Math.Max(0, body.hp), max = body.maxHp,
            segment = brain.p.legAt.Length > 0 ? brain.p.legAt[0] : 0,
        };

        static double RaySphere(Vec3 o, Vec3 d, Vec3 c, double r)
        {
            Vec3 oc = o - c;
            double b = Vec3.Dot(oc, d), cc = Vec3.Dot(oc, oc) - r * r, disc = b * b - cc;
            if (disc < 0) return -1;
            double sq = Math.Sqrt(disc), t = -b - sq;
            if (t < 0) t = -b + sq;
            return t >= 0 ? t : -1;
        }

        /// <summary>The ray's distance to the body (torso front and back, the head; -1: a miss).</summary>
        public double Raycast(Vec3 o, Vec3 d)
        {
            if (!body.alive || brain.state == Boss37State.Dead) return -1;
            Vec3 c = Centre, f = Facing * (p.length * 0.3);
            double best = -1;
            foreach (var (q, r) in new[] { (c + f, p.radius), (c - f, p.radius), (Head, p.radius * 0.6) })
            {
                double t = RaySphere(o, d, q, r);
                if (t >= 0 && (best < 0 || t < best)) best = t;
            }
            return best;
        }

        /// <summary>The share of a hit that lands. The punch is the sim's catch rule (blocked here); a returned chunk (parry31) lands in full;
        /// everything else is Boss37's scale (10 % chip, full in the CYAN window).</summary>
        public double Filter(double amount, string weapon, out bool blocked)
        {
            blocked = false;
            if (!Hurtable) { blocked = true; return 0; }
            if (weapon == "punch") { blocked = true; return 0; }
            if (weapon == "parry31" || weapon == "parry36") return amount;
            return amount * brain.Scale(false);
        }

        public void Apply(double dmg, string part)
        {
            if (!body.alive || dmg <= 0) return;
            // the legs are the fight's shape: no hit carries the bar past the next leg's cut (one leg at a time, BOSSES.md rule 1), and only the
            // parried charge (the crash) kills it in leg 3 ("The end"): the excess is lost
            double floor = 0;
            if (brain.leg < 3 && brain.leg - 1 < brain.p.legAt.Length) floor = body.maxHp * brain.p.legAt[brain.leg - 1] - 1;
            else if (part != "crash") floor = 1;
            if (body.hp - dmg < floor) dmg = Math.Max(0, body.hp - floor);
            if (dmg <= 0) return;
            body.hp -= dmg;
            dealt += dmg;
            Vec3 pv = playerVelocity != null ? playerVelocity() : default;
            if ((playerGrounded == null || playerGrounded()) && JsMath.Hypot(pv.x, pv.z) < 1.0) dealtStill += dmg;
            body.flash = 0.06;
            said.Reset("stag37");
            said.damage = JsMath.Round(dmg);
            said.part = part;
            said.core = brain.Open;
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            if (body.hp <= 0 && brain.state != Boss37State.Dying && brain.state != Boss37State.Dead)
            {
                body.hp = 0;
                chunk.alive = false;
                brain.Die(S(Math.Max(1.0, p.chargeCrash + 0.6)));
                kit?.Dying("stag37", Centre);
            }
        }

        Stag37Event Say(string stage, Vec3 at)
        {
            ev.stage = stage;
            ev.at = at;
            ev.s = s;
            ev.lane = arena.LaneOf(lat);
            return ev;
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
            return h.Damage(amount, "boss");
        }

        static double SegDist(Vec3 pt, Vec3 a, Vec3 b)
        {
            Vec3 ab = b - a, ap = pt - a;
            double len2 = Vec3.Dot(ab, ab), t = len2 < 1e-12 ? 0 : Math.Max(0, Math.Min(1, Vec3.Dot(ap, ab) / len2));
            return Vec3.DistanceTo(pt, a + ab * t);
        }

        // ---- the punch (the sim's "punch" event): the chunk parry, the charge parry, the window (launch), the leg punch -----------------------

        /// <summary>A punch from `eye` along `dir`. Returns what it did ("" nothing).</summary>
        public string Punch(Vec3 eye, Vec3 dirV)
        {
            lastPunchStep = stepNo;
            punchUsed = false;
            punchEye = eye;
            punchDir = dirV;
            if (!body.alive || !brain.Fighting && brain.state != Boss37State.Window) return "";
            if (TryChunkParry(eye, dirV)) return "chunk";
            if (TryChargeParry(eye)) return "charge";
            Vec3 feet = target.position();
            Vec3 pv = playerVelocity != null ? playerVelocity() : default;
            double pspeed = JsMath.Hypot(pv.x, pv.z);
            if (brain.Open)
            {
                if (Vec3.DistanceTo(eye, Centre) > p.launchReach + p.radius) return "";
                bool launch = stepNo - lastSpeedVerbStep <= S(p.launchGrace);
                if (launch)
                {
                    launches++;
                    // the burst: it is driven into the canal wall (its line jumps to the outer wall), full damage
                    lat = arena.CanalHalf - p.radius;
                    speed = 0;
                    events.Emit("stag37Launch", Say("burst", Centre));
                    Apply(p.launchDamage, "launch");
                    return "launch";
                }
                windowPunches++;
                Apply(p.windowPunchDamage, "window");
                return "window";
            }
            if (legPunchCool > 0 || leg3 != Stag37Leg3.None) return "";
            if (pspeed < p.catchMinSpeed) return "";
            if (gap < -1.5 || Vec3.DistanceTo(new Vec3(feet.x, feet.y + 1.0, feet.z), Hind) > p.catchRange) return "";
            legPunches++;
            legPunchCool = S(p.catchCooldown);
            Stumble();
            events.Emit("stag37LegPunch", Say("punch", Hind));
            Apply(p.catchDamage * brain.Scale(true), "leg");
            brain.Stagger(p.catchStagger, "leg");
            return "leg";
        }

        bool TryChargeParry(Vec3 eye)
        {
            if (leg3 != Stag37Leg3.Charge || brain.state != Boss37State.Active || Vec3.DistanceTo(eye, Head) > p.chargeParryReach) return false;
            punchUsed = true;
            Crash();
            return true;
        }

        void Stumble()
        {
            stumbles++;
            stumbleLeft = S(p.stumbleSeconds);
        }

        bool TryChunkParry(Vec3 eye, Vec3 dirV)
        {
            if (!chunk.alive || chunk.back) return false;
            Vec3 to = chunk.pos - eye;
            double d = to.Length;
            if (d > p.chunkParryReach + p.chunkRadius) return false;
            if (d > 0.5 && Vec3.Dot(to, dirV) / d < JsMath.Cos(p.chunkParryCone * Math.PI / 180)) return false;
            chunk.back = true;
            chunk.life = S(3);
            punchUsed = true;
            chunkParries++;
            parried.owner = "player";
            parried.weapon = "punch";
            parried.kind = IceChunk;
            parried.enemy = body.id;
            parried.damage = p.chunkParryDamage;
            parried.position = chunk.pos;
            events.Emit("parry36", parried);
            events.Emit("stag37Chunk", Say("parried", chunk.pos));
            return true;
        }

        void Crash()
        {
            chargeParries++;
            leg3 = Stag37Leg3.Crash;
            crashLeft = S(p.chargeCrash);
            parried.owner = "player";
            parried.weapon = "punch";
            parried.kind = AntlerCharge;
            parried.enemy = body.id;
            parried.damage = body.hp;
            parried.position = Head;
            events.Emit("parry36", parried);
            events.Emit("stag37Crash", Say("parried", Head));
            brain.Parried(0);
        }

        // ---- one step -----------------------------------------------------------------------------------------------------------------------

        public void Step(double dt)
        {
            if (!body.alive) return;
            body.previous = body.position;
            if (Frozen) return;
            stepNo++;
            Vec3 pw = target.position(), pl = pw - origin;
            arena.Project(pl.x, pl.z, out ps, out plat);
            playerLane = arena.LaneOf(plat);
            gap = arena.Ahead(ps, s);
            Vec3 pv = playerVelocity != null ? playerVelocity() : default;
            bool grounded = playerGrounded == null || playerGrounded();
            bool riding = playerRiding != null && playerRiding();
            if (playerSpeedVerb != null && playerSpeedVerb()) lastSpeedVerbStep = stepNo;
            if (legPunchCool > 0) legPunchCool--;
            // the slipstream (and its habit: the kick back)
            InSlip = brain.Fighting && leg3 == Stag37Leg3.None && dir > 0 && gap > 0.3 && gap <= p.slipRange && Math.Abs(plat - lat) <= p.slipLaneHalf && pw.y < origin.y + 1.0;
            if (InSlip) { slipSteps++; slipSeconds += dt; } else slipSteps = 0;
            // the habit: the slipstream time over the last habitWindow seconds (not one unbroken stretch): past habitSeconds, the kick back
            int win = Math.Max(1, S(p.slipWindow));
            if (slipRing.Length != win) { slipRing = new bool[win]; slipRingAt = 0; slipRecent = 0; }
            if (slipRing[slipRingAt]) slipRecent--;
            slipRing[slipRingAt] = InSlip;
            if (InSlip) slipRecent++;
            slipRingAt = (slipRingAt + 1) % win;
            if (slipRecent > slipRecentMax) slipRecentMax = slipRecent;
            if (slipRecent >= S(p.slipHabit) && brain.Fighting && !brain.Open && brain.leg < 3 && brain.Force(KickBack))
            {
                kickForces++;
                Array.Clear(slipRing, 0, slipRing.Length);
                slipRecent = 0;
            }
            Boss37State before = brain.state;
            string atkBefore = AttackId;
            var ctx = new Boss37Context
            {
                player = pw, playerVel = pv, boss = Centre, grounded = grounded, airborne = !grounded && !riding, riding = riding,
                movingTheWay = JsMath.Hypot(pv.x, pv.z) > p.catchMinSpeed, lineOfSight = true, outOfReach = false, hpFrac = HpFrac, aim = Head,
            };
            brain.Step(ctx);
            if (brain.entered)
            {
                if (brain.state == Boss37State.Tell) StartTell(pw);
                else if (brain.state == Boss37State.Active) StartActive(pw, pv);
                else if (brain.state == Boss37State.Window) { skids++; events.Emit("stag37Skid", Say("fall", Centre)); }
                else if (brain.state == Boss37State.LegChange) LegChange();
            }
            if (brain.state == Boss37State.Dead)
            {
                body.alive = false;
                kit?.Down("stag37", body.id, Centre);
                return;
            }
            if (brain.RepositionDue && brain.ConsumeReposition() && leg3 == Stag37Leg3.None) { Place(ps + p.startAhead, arena.LaneLat(playerLane)); }
            // the parry's early buffer
            if (!punchUsed && stepNo - lastPunchStep <= S(p.chunkBuffer)) { if (TryChunkParry(punchEye, punchDir)) { } }
            if (!punchUsed && stepNo - lastPunchStep <= S(p.chargeBuffer)) TryChargeParry(target.position() + new Vec3(0, punchEye.y - target.position().y, 0));
            if (brain.Fighting || brain.state == Boss37State.Dying)
            {
                StepAttacks(pw, pv, dt);
                StepChunk(dt);
            }
            Move(dt);
            body.position = Feet;
            body.velocity = Facing * speed;
        }

        void LegChange()
        {
            chunk.alive = false;
            stumbleLeft = 0;
            if (brain.leg == 2) brain.SetCheckpoint(2, target.position(), 0);
            if (brain.leg == 3)
            {
                brain.SetCheckpoint(3, target.position(), 0);
                leg3 = Stag37Leg3.Run;
                PickTurn();
                brain.Force(AntlerCharge); // the leg's first pick is the charge: it squares up wherever its run has got to when the wind-up starts
            }
            events.Emit("stag37Bolt", Say("leg" + brain.leg, Centre));
        }

        /// <summary>Where leg 3 turns (picked when the run starts): the first of the tunnel's two mouths or the two turns' apexes (the bank) at least
        /// 15 m ahead: the charge comes back out of the tunnel (or off the bank) head-on.</summary>
        public double TurnS;
        public string TurnAt = "";
        void PickTurn()
        {
            var c = new[] { (arena.SB + arena.p.tunnelTo, "tunnel south mouth"), (arena.SB + arena.p.tunnelFrom, "tunnel north mouth"), (arena.STurnN + arena.TurnLength / 2, "north bank"), (arena.STurnS + arena.TurnLength / 2, "south bank") };
            double best = 1e9;
            foreach (var (cs, name) in c)
            {
                double d = arena.Wrap(cs - s);
                if (d >= 15 && d < best) { best = d; TurnS = cs; TurnAt = name; }
            }
        }
        int wheels;

        void StartTell(Vec3 pw)
        {
            hitThisAttack = false;
            switch (AttackId)
            {
                case LaneCut:
                    cutLane = playerLane;
                    break;
                case KickBack:
                    kickTells++;
                    break;
                case AntlerCharge:
                    charges++;
                    if (leg3 == Stag37Leg3.None) leg3 = Stag37Leg3.Turned; // a camp answer outside leg 3: it turns and charges too
                    else leg3 = Stag37Leg3.Turned;
                    break;
            }
        }

        void StartActive(Vec3 pw, Vec3 pv)
        {
            hitThisAttack = false;
            switch (AttackId)
            {
                case ShardKick:
                    shardLane = arena.LaneOf(lat);
                    shardS1 = s - p.hindBack;
                    shardS0 = shardS1 - p.shardRange;
                    events.Emit("stag37Shards", Say("spray", Hind));
                    break;
                case IceChunk:
                    {
                        Vec3 from = Centre + new Vec3(0, 1.0, 0);
                        double tf = p.chunkFlight;
                        Vec3 aim = pw + pv * tf + new Vec3(0, 1.0, 0);
                        Vec3 v = (aim - from) / tf;
                        v = new Vec3(v.x, v.y + 0.5 * p.chunkGravity * tf, v.z);
                        chunk.pos = chunk.prev = from;
                        chunk.vel = v;
                        chunk.life = S(tf * 2.5);
                        chunk.alive = true;
                        chunk.back = false;
                        events.Emit("stag37Chunk", Say("throw", from));
                        break;
                    }
                case LaneCut:
                    {
                        cuts++;
                        cutS = s - p.hindBack - p.cutBehind;
                        double ahead = arena.Ahead(ps, cutS);
                        if (ahead < 3) cutS = ps + 3; // never on top of the player
                        double half = arena.p.laneWidth * 0.5 - 0.25, l0 = arena.LaneLat(cutLane);
                        Vec3 a0 = arena.PointAt(cutS, l0 - half, out Vec3 h0), a1 = arena.PointAt(cutS, l0 + half);
                        Vec3 t = h0 * (p.cutThick * 0.5);
                        Vec3 lo = new Vec3(Math.Min(Math.Min(a0.x, a1.x) - Math.Abs(t.x), Math.Min(a0.x, a1.x) + Math.Abs(t.x)), 0, Math.Min(a0.z, a1.z) - Math.Abs(t.z));
                        Vec3 hi = new Vec3(Math.Max(a0.x, a1.x) + Math.Abs(t.x), p.cutHeight, Math.Max(a0.z, a1.z) + Math.Abs(t.z));
                        dropBarrier?.Invoke(lo, hi);
                        events.Emit("stag37Cut", Say("drop", arena.PointAt(cutS, l0) + origin));
                        break;
                    }
                case KickBack:
                    {
                        kicks++;
                        events.Emit("stag37Kick", Say("kick", Hind));
                        if (gap > 0 && gap <= p.kickRange + p.hindBack && Math.Abs(plat - lat) <= p.slipLaneHalf && pw.y < origin.y + 1.8)
                        {
                            if (HurtPlayer(p.kickDamage)) kickHits++;
                            Vec3 back = -Facing;
                            target.impulse?.Invoke(back.x * p.kickKnock, p.kickKnockUp, back.z * p.kickKnock);
                        }
                        break;
                    }
                case AntlerCharge:
                    // charge toward the player along the path
                    dir = arena.Ahead(s, ps) >= 0 ? 1 : -1;
                    arena.PointAt(s, lat, out heading);
                    leg3 = Stag37Leg3.Charge;
                    wheels = 0;
                    speed = p.chargeSpeed * 0.5;
                    events.Emit("stag37Charge", Say("charge", Head));
                    break;
            }
        }

        void StepAttacks(Vec3 pw, Vec3 pv, double dt)
        {
            if (brain.state != Boss37State.Active) return;
            Capsule cap = PlayerCapsule();
            switch (AttackId)
            {
                case ShardKick:
                    {
                        if (hitThisAttack) break;
                        bool inLane = arena.LaneOf(plat) == shardLane && Math.Abs(plat - arena.LaneLat(shardLane)) <= arena.p.laneWidth * 0.5;
                        double back = arena.Ahead(ps, shardS1);
                        if (inLane && back >= 0 && back <= p.shardRange && pw.y - origin.y < p.shardHeight)
                        {
                            hitThisAttack = true;
                            if (HurtPlayer(p.shardDamage)) shardHits++;
                        }
                        break;
                    }
                case AntlerCharge:
                    {
                        if (hitThisAttack || leg3 != Stag37Leg3.Charge) break;
                        Vec3 h = Head;
                        if (SegDist(new Vec3(h.x, Math.Min(h.y, cap.end.y), h.z), cap.start, cap.end) <= p.chargeHitRadius + cap.radius)
                        {
                            hitThisAttack = true;
                            if (HurtPlayer(p.chargeDamage)) chargeHits++;
                            Vec3 f = Facing;
                            target.impulse?.Invoke(f.x * p.chargeKnock, 4, f.z * p.chargeKnock);
                        }
                        break;
                    }
            }
        }

        void StepChunk(double dt)
        {
            if (!chunk.alive) return;
            chunk.prev = chunk.pos;
            if (chunk.back)
            {
                Vec3 home = Centre - chunk.pos;
                double len = home.Length;
                if (len <= p.radius + p.chunkRadius || len <= p.chunkReturn * dt)
                {
                    chunk.alive = false;
                    events.Emit("stag37Chunk", Say("land", Centre));
                    Stumble();
                    parryHit?.Invoke(p.chunkParryDamage, Centre);
                    brain.Parried(p.chunkStagger);
                    return;
                }
                chunk.vel = home * (p.chunkReturn / len);
                chunk.pos = chunk.pos + chunk.vel * dt;
                if (--chunk.life <= 0) chunk.alive = false;
                return;
            }
            chunk.vel = new Vec3(chunk.vel.x, chunk.vel.y - p.chunkGravity * dt, chunk.vel.z);
            chunk.pos = chunk.pos + chunk.vel * dt;
            Capsule cap = PlayerCapsule();
            if (SegDist(chunk.pos, cap.start, cap.end) < p.chunkRadius + cap.radius)
            {
                chunk.alive = false;
                if (HurtPlayer(p.chunkDamage)) chunkHits++;
                events.Emit("stag37Chunk", Say("hit", chunk.pos));
                return;
            }
            if (--chunk.life <= 0 || chunk.pos.y < origin.y - 0.5) { chunk.alive = false; events.Emit("stag37Chunk", Say("shatter", chunk.pos)); }
        }

        void Move(double dt)
        {
            double want;
            Boss37State st = brain.state;
            if (st == Boss37State.Reveal)
            {
                // it stands and looks back; a player who skates in fast finds it trotting off ahead (never overtaken in the walk-in)
                Vec3 pv = playerVelocity != null ? playerVelocity() : default;
                want = gap < p.startAhead ? Math.Max(p.leashSpeed, JsMath.Hypot(pv.x, pv.z) + 2) : 0;
            }
            else if (st == Boss37State.Dying || st == Boss37State.Dead) want = 0;
            else if (leg3 == Stag37Leg3.Crash)
            {
                // the parried Stag is thrown back into the tunnel's wall
                if (crashLeft > 0 && --crashLeft == 0 && body.alive) Apply(body.hp, "crash");
                speed = p.chargeSpeed * 0.6;
                s = arena.Wrap(s - dir * speed * dt / arena.ArcFactor(s, lat));
                arena.PointAt(s, lat, out heading);
                return;
            }
            else if (st == Boss37State.Window)
            {
                // the fall: it skids on along its line, drifting to the outer wall, and stops
                double dec = Math.Max(speed, 1) / Math.Max(dt, p.skidSeconds) * dt;
                speed = Math.Max(0, speed - dec);
                double wall = arena.CanalHalf - p.radius;
                lat += Math.Max(0, Math.Min(p.laneShift * 1.5 * dt, wall - lat));
                s = arena.Wrap(s + dir * speed * dt / arena.ArcFactor(s, lat));
                arena.PointAt(s, lat, out heading);
                return;
            }
            else if (leg3 == Stag37Leg3.Run)
            {
                dir = 1;
                double toTurn = arena.Wrap(TurnS - s);
                want = toTurn < 2 ? 0 : p.chargeRunSpeed;
                if (toTurn < 2 || (toTurn > arena.Length - 3))
                {
                    // at the tunnel's mouth: it turns to face back up the lane and waits for its charge
                    speed = 0;
                    dir = -1;
                    leg3 = Stag37Leg3.Turned;
                    brain.Force(AntlerCharge);
                    events.Emit("stag37Bolt", Say("turn", Centre));
                }
            }
            else if (leg3 == Stag37Leg3.Turned)
            {
                want = 0;
                if (st == Boss37State.Rest && brain.leg == 3) brain.Force(AntlerCharge);
                // it squares up to your lane through the wind-up (locks at 70 %)
                if (!(st == Boss37State.Tell && brain.Progress > 0.7))
                {
                    double tl = arena.LaneLat(playerLane), stp = p.laneShift * dt;
                    lat += Math.Max(-stp, Math.Min(stp, tl - lat));
                }
                if (st == Boss37State.Rest && brain.leg < 3) leg3 = Stag37Leg3.None; // a camp charge outside leg 3 ends with its rest
                dir = arena.Ahead(s, ps) >= 0 ? 1 : -1;
            }
            else if (leg3 == Stag37Leg3.Charge)
            {
                want = p.chargeSpeed;
                // passed by a player who did not change lanes: it wheels round once and comes again (only a lane change or the parry beats it)
                if (!hitThisAttack && wheels == 0 && arena.Ahead(s, ps) * dir < -2 && Math.Abs(plat - lat) <= p.slipLaneHalf) { dir = -dir; wheels++; speed = p.chargeSpeed * 0.3; events.Emit("stag37Charge", Say("wheel", Head)); }
                if (st != Boss37State.Active) { leg3 = brain.leg >= 3 ? Stag37Leg3.Turned : Stag37Leg3.None; want = 0; if (brain.leg >= 3) brain.Force(AntlerCharge); }
            }
            else
            {
                dir = 1;
                want = arena.OnTurn(s) ? p.turnSpeed : p.runSpeed;
                if (st == Boss37State.Tell || st == Boss37State.Active || st == Boss37State.Recover) want *= p.attackSlow;
                if (st == Boss37State.LegChange) want = p.runSpeed;
                // the rubber band (the chase stays a chase for a player who only sprint-glides): fallen past farGap behind, it LOOKS BACK and
                // runs under his own speed (farShare of it, at least leashSpeed) until he is back in its slipstream (within slip.range - 1 m)
                if (gap > p.farGap && !waiting) { waiting = true; waits++; }
                else if ((gap <= p.slipRange - 1 && InSlip) || gap < 0) waiting = false; // back in its slipstream (its lane, within range): it runs again
                if (InSlip && !waiting)
                {
                    // slower than you in its slipstream (BOSSES.md): held to slipShare of your speed
                    Vec3 pvs = playerVelocity != null ? playerVelocity() : default;
                    want = Math.Min(want, Math.Max(p.leashSpeed, JsMath.Hypot(pvs.x, pvs.z) * p.slipShare));
                }
                if (waiting)
                {
                    Vec3 pvw = playerVelocity != null ? playerVelocity() : default;
                    want = Math.Min(want, Math.Max(p.leashSpeed, JsMath.Hypot(pvw.x, pvw.z) * p.farShare));
                }
                if (stumbleLeft > 0) { stumbleLeft--; want = Math.Min(want, p.stumbleSpeed); }
                else if (gap < -2)
                {
                    // passed: it will not stay behind you (a chase, not an escort): it runs you down at your speed + passBy
                    Vec3 pv = playerVelocity != null ? playerVelocity() : default;
                    want = Math.Max(want, Math.Min(p.chargeRunSpeed, JsMath.Hypot(pv.x, pv.z) + p.passBy));
                }
                // the lane weave (never across a lane wall)
                if (--laneLeft <= 0)
                {
                    laneLeft = S(p.laneEvery);
                    int pick = (int)Math.Floor(rng.Next() * arena.p.lanes);
                    if (!arena.LaneWallAt(s, 4) && !arena.LaneWallAt(s + 15, 4)) laneTarget = Math.Max(0, Math.Min(arena.p.lanes - 1, pick));
                }
                if (!arena.LaneWallAt(s, 0.5))
                {
                    double tl = arena.LaneLat(laneTarget), stp = p.laneShift * dt;
                    lat += Math.Max(-stp, Math.Min(stp, tl - lat));
                }
            }
            double a = want > speed ? p.accel : p.decel;
            speed += Math.Max(-a * dt, Math.Min(a * dt, want - speed));
            s = arena.Wrap(s + dir * speed * dt / arena.ArcFactor(s, lat));
            arena.PointAt(s, lat, out heading);
        }

        /// <summary>A fall or a death (D-6): the leg starts again; the player is put at the checkpoint (returned) and the Stag ahead of it.</summary>
        public Vec3 RestartLeg()
        {
            Vec3 at = brain.RestartLeg();
            chunk.alive = false;
            stumbleLeft = slipSteps = 0;
            Array.Clear(slipRing, 0, slipRing.Length);
            slipRecent = 0;
            Vec3 l = at - origin;
            arena.Project(l.x, l.z, out double s0, out _);
            if (leg3 == Stag37Leg3.None || brain.leg < 3) { leg3 = Stag37Leg3.None; Place(s0 + p.startAhead, arena.LaneLat(1)); }
            else { leg3 = Stag37Leg3.Run; Place(s0 + p.startAhead, arena.LaneLat(1)); PickTurn(); }
            return at;
        }
    }
}
