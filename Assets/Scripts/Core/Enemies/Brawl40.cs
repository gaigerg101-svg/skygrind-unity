// NIGHT40 builder B2 (brawl40; NIGHT40.md card B2, <boss_rules>; GG 2026-10-13: "The second boss fight should be completely redesigned into a
// brawler-style encounter"): THE ICEBREAKER (working name, DECIDE40 D-3), level 2's boss in n40-iceline's last room. Its own small brain
// (Boss37.cs is B4's tonight: nothing here calls or edits it; the shape is copied: a state per beat, whole-step timers, a stagger meter that
// opens a window). Pure: no UnityEngine, fixed 120 Hz, every timer in whole steps (StepsFor), every number in design.n40.brawl40, JsMath for
// the transcendentals, Mulberry32 seeded per fight.
//
// THE RULE (boss_rules 5, research/boss-web.md section 5: the Marauder is open only at ~3-4.5 m; the Hideous Mass's shell takes melee only):
//   RANGE    a gun's hit lands x1 within range.close of its body and fades to x range.farMult at range.far and beyond (linear between);
//            the PULSE GAUNTLET's swing and the F punch always land in full and drain its STAGGER meter.
//   STUCK    after the haymaker its fist is stuck in the ice (stuckSeconds, Minos Prime's stuck fist), after a rush into a wall or pillar it
//            is dazed (rush.stuckSeconds): every hit x stuckMult, melee stagger x stuckMult. The punish.
//   WINDOW   the meter empty: it reels (windowSeconds; every hit x windowMult), then the meter refills to the phase's value.
//   PHASES   two (boss_rules 6: ULTRAKILL splits about evenly): at phase2At of its bar a phaseSeconds transition (no damage, the kit's beat:
//            a banner, a flash and a push), then faster: phase 2's walk, wind-ups and rests, and a haymaker chains into a slam up close.
// THE ATTACKS (boss_rules 1-3; each wind-up raised through Tells36 at the attacking part; one at a time):
//   HAYMAKER (heavy RED, windup >= 0.9 s, the fist glows): it tracks you for the first trackShare of the wind-up, then commits: a lunge and a
//            swing in a cone in front of it; answer: a dash or a jump out of the cone; then STUCK.
//   SLAM     (RED, a ground ring decal): both fists into the ice; a shockwave ring runs out from it along the floor; answer: be in the air
//            (jump, ride a wall) when the ring passes. Its camp answer (boss_rules 3): a grounded player within camp.radius of one spot for
//            camp.seconds gets the slam next. Also picked at mid range.
//   BLOCK    (YELLOW: parryable) far away it tears a block of ice out of the floor and throws it at you; punch it (the F punch or the
//            gauntlet's swing) and it flies back: block.parryDamage and block.stagger. Unparried it hits for block.damage.
//   RUSH     (RED, a ground line) past rush.startDist: a shoulder charge along a locked line; a wall or pillar stops it dazed (STUCK).
// Events (names for GG's sounds, no audio): brawl40Rise, brawl40Haymaker / brawl40Slam / brawl40Block / brawl40Rush (stage "tell" | "swing" |
// "hit" | "stuck" | "free" | "ring" | "throw" | "parried" | "return" | "shatter" | "charge" | "wall"), brawl40Floe ("crack" | "open" | "bite"), brawl40Stagger ("drain" | "open" |
// "close"), brawl40Phase ("change" | "start"); the shared bossHit / bossDying / bossRise / bossDown and parry36.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Brawl40Params
    {
        public string name;
        public double pool, revealSeconds, phase2At, phaseSeconds;
        public double height, radius, fistRadius, margin;
        public double walkSpeed, walkSpeed2, turnRate, stopDist;
        public double rangeClose, rangeFar, rangeFarMult;
        public double[] stagger = { 6, 8 };
        public double gauntletStagger, punchStagger, stuckMult, windowSeconds, windowMult;
        public double restSeconds, restSeconds2;
        public double hayWindup, hayWindup2, hayActive, hayRecover, hayReach, hayCone, hayLunge, hayDamage, hayKnock, hayKnockUp, hayTrack, hayStart, stuckSeconds;
        public double campSeconds, campRadius;
        public double slamWindup, slamWindup2, slamRecover, slamSpeed, slamRadius, slamWidth, slamHeight, slamDamage, slamKnockUp, slamMidChance, slamChain2;
        public double blockWindup, blockRecover, blockStartDist, blockSpeed, blockGravity, blockRadius, blockDamage, blockParryReach, blockParryCone, blockReturn, blockParryDamage, blockStagger, blockBuffer;
        public double rushWindup, rushStartDist, rushSpeed, rushMaxSeconds, rushHitRadius, rushDamage, rushKnock, rushStuckSeconds, rushRecover, rushChance;
        public int floeGrid;
        public double floeEvery, floeTell, floeOpen, floeDps, floeTick, floeLead;
        /// <summary>NIGHT47 B brawl47 (SurvivalSim.N47.Icebreaker.cs sets it while the row is on; 0 = NIGHT46's two phases): at this share of its
        /// bar a third phase begins: a second transition beat ("PHASE 3", event brawl47Phase3) and Brawl40.Phase3_47 true (the sim's P3 rules).</summary>
        public double phase3At47;
        /// <summary>NIGHT47 B brawl47 (0 = NIGHT46: x1): every hit outside STUCK and the WINDOW lands x this (NIGHT47.md section 3: the chip
        /// outside windows >= 30 % of full; full = the WINDOW's windowMult 2), so the length comes from the windows, never from HP.</summary>
        public double chip47;
        /// <summary>NIGHT47 B brawl47 (0 = NIGHT46: no armour): THE ICE ARMOUR. At each phase change (and at the fight's start with armorP1_47)
        /// it re-armours: every hit outside STUCK / the WINDOW lands x armorChip47 until the phase's first WINDOW (the meter emptied: the
        /// gauntlet, a punch, a phase-3 charger) breaks it, or armorMaxS47 s pass (B-1: LOCKED says what opens it). Event brawl47Armor.</summary>
        public double armorChip47, armorMaxS47;
        public bool armorP1_47;
        /// <summary>NIGHT47 B brawl47: phase 3's armour is THE HERD's: a WINDOW does not break it, only a charger run into it or the phase-3
        /// chargers all dead (the sim: Brawl40.BreakArmor47), or armorMaxS47.</summary>
        public bool armorP3Herd47;
        /// <summary>NIGHT47 B brawl47 (0 = the window's windowMult as NIGHT46): what a WINDOW that opened on the armour (it cracks it) lands.</summary>
        public double crackWindowMult47;

        public const string Row = "design.n40.brawl40.";

        public static Brawl40Params From(GameConfig c)
        {
            double N(string k) => c.Num(Row + k);
            return new Brawl40Params
            {
                name = c.Str(Row + "name"), pool = N("pool"), revealSeconds = N("revealSeconds"), phase2At = N("phase2At"), phaseSeconds = N("phaseSeconds"),
                height = N("body.height"), radius = N("body.radius"), fistRadius = N("body.fistRadius"), margin = N("body.margin"),
                walkSpeed = N("move.walkSpeed"), walkSpeed2 = N("move.walkSpeed2"), turnRate = N("move.turnRateDeg") * Math.PI / 180, stopDist = N("move.stopDist"),
                rangeClose = N("range.close"), rangeFar = N("range.far"), rangeFarMult = N("range.farMult"),
                stagger = c.Nums(Row + "stagger.meter"), gauntletStagger = N("stagger.gauntlet"), punchStagger = N("stagger.punch"),
                stuckMult = N("stagger.stuckMult"), windowSeconds = N("stagger.windowSeconds"), windowMult = N("stagger.windowMult"),
                restSeconds = N("rest.seconds"), restSeconds2 = N("rest.seconds2"),
                hayWindup = N("haymaker.windup"), hayWindup2 = N("haymaker.windup2"), hayActive = N("haymaker.active"), hayRecover = N("haymaker.recover"),
                hayReach = N("haymaker.reach"), hayCone = N("haymaker.coneDeg"), hayLunge = N("haymaker.lunge"), hayDamage = N("haymaker.damage"),
                hayKnock = N("haymaker.knock"), hayKnockUp = N("haymaker.knockUp"), hayTrack = N("haymaker.trackShare"), hayStart = N("haymaker.startDist"),
                stuckSeconds = N("haymaker.stuckSeconds"),
                campSeconds = N("camp.seconds"), campRadius = N("camp.radius"),
                slamWindup = N("slam.windup"), slamWindup2 = N("slam.windup2"), slamRecover = N("slam.recover"), slamSpeed = N("slam.ringSpeed"),
                slamRadius = N("slam.ringRadius"), slamWidth = N("slam.ringWidth"), slamHeight = N("slam.ringHeight"), slamDamage = N("slam.damage"),
                slamKnockUp = N("slam.knockUp"), slamMidChance = N("slam.midChance"), slamChain2 = N("slam.chain2"),
                blockWindup = N("block.windup"), blockRecover = N("block.recover"), blockStartDist = N("block.startDist"), blockSpeed = N("block.speed"),
                blockGravity = N("block.gravity"), blockRadius = N("block.radius"), blockDamage = N("block.damage"), blockParryReach = N("block.parryReach"),
                blockParryCone = N("block.parryConeDeg"), blockReturn = N("block.returnSpeed"), blockParryDamage = N("block.parryDamage"),
                blockStagger = N("block.stagger"), blockBuffer = N("block.bufferSeconds"),
                rushWindup = N("rush.windup"), rushStartDist = N("rush.startDist"), rushSpeed = N("rush.speed"), rushMaxSeconds = N("rush.maxSeconds"),
                rushHitRadius = N("rush.hitRadius"), rushDamage = N("rush.damage"), rushKnock = N("rush.knock"), rushStuckSeconds = N("rush.stuckSeconds"),
                rushRecover = N("rush.recover"), rushChance = N("rush.chance"),
                floeGrid = c.Int(Row + "floes.grid"), floeEvery = N("floes.every"), floeTell = N("floes.tell"), floeOpen = N("floes.open"),
                floeDps = N("floes.dps"), floeTick = N("floes.tick"), floeLead = N("floes.lead"),
            };
        }

        public double StaggerOf(int phase) => stagger.Length == 0 ? 6 : stagger[Math.Min(stagger.Length, Math.Max(1, phase)) - 1];
    }

    public sealed class Brawl40Body : Enemy, IBossBody27
    {
        public readonly Brawl40 system;

        public Brawl40Body(Brawl40 system)
        {
            this.system = system;
            kind = "brawler";
        }

        public override double Height => system.p.height;
        public override double Radius => system.p.radius;
        public override double AimHeight => system.p.height * 0.6;
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

    public enum Brawl40State : byte { Off, Reveal, Idle, Tell, Active, Stuck, Recover, Window, PhaseChange, Dying, Dead }

    /// <summary>The payload of the brawl40 events (pooled: read it, don't keep it).</summary>
    public sealed class Brawl40Event
    {
        public string stage, attack;
        public int phase;
        public double stagger;
        public Vec3 at;
    }

    /// <summary>The thrown ice block (one at a time).</summary>
    public sealed class Brawl40Block
    {
        public Vec3 pos, prev, vel;
        public int life;
        public bool alive, back;
    }

    public sealed partial class Brawl40 : IBossSystem // NIGHT42 B1: partial (brawl42's attacks in Brawl40.N42.cs)
    {
        public const string Haymaker = "brawlHaymaker", Slam = "brawlSlam", Block = "brawlBlock", Rush = "brawlRush";
        public readonly Brawl40Params p;
        public readonly Brawl40Body body;
        readonly EventBus events;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt0;
        readonly BossEvent said = new BossEvent();
        readonly Brawl40Event ev = new Brawl40Event();
        readonly HitEvent parried = new HitEvent();
        public Tells36 tells;
        BossKit kit;
        Mulberry32 rng;
        /// <summary>The run's seed for this fight (the sim sets it before StartFight).</summary>
        public uint seed = 1;
        /// <summary>The arena's centre (world, on the floor), its half size (the clamp), and its pillars (world boxes the body is pushed out of).</summary>
        public Vec3 origin;
        public double half = 18;
        public readonly List<(Vec3 min, Vec3 max)> pillars = new List<(Vec3, Vec3)>();
        public Func<Capsule> playerBody;
        public Func<bool> playerGrounded;
        /// <summary>A returned block reached it: the sim lands `damage` as a parry hit (Filter takes it in full).</summary>
        public Action<double, Vec3> parryHit;
        /// <summary>The sim's start-of-fight resume flag (the name card re-enters StartFight through the kit: keep the fight).</summary>
        public bool resume;

        public Brawl40State state = Brawl40State.Off;
        public int stateSteps, stateLeft, stateTotal, stepNo, phase = 1;
        public bool entered;
        public string attack, previous;
        public double stagger, yaw;
        public Vec3 facing = new Vec3(0, 0, 1);
        bool hitThisAttack, chained;
        string lastWeapon = "";
        Vec3 lungeDir;
        int campSteps;
        Vec3 campAnchor;
        /// <summary>The slam's ring: live, its radius now (world, round the centre), its centre.</summary>
        public bool ringOn;
        public double ringR;
        public Vec3 ringAt;
        bool ringHit;
        /// <summary>The thrown block.</summary>
        public readonly Brawl40Block block = new Brawl40Block();
        /// <summary>The rush's locked line (its start and direction) for the view's ground line.</summary>
        public Vec3 rushFrom, rushDir;
        int rushLeft;
        bool punchUsed;
        int lastPunchStep = -100000;
        Vec3 punchEye, punchDir;

        // stats (the harness, the reels)
        public int haymakers, hayHits, hayDodged, windows, stuckHits, phaseChanges, campPicks, outside;
        public int slams, slamHits, blocks, blockHits, blockParries, rushes, rushHits, rushWalls, floesCracked, floeTicks;
        public double dealt, dealtMelee, dealtStill;
        /// <summary>Damage landed per weapon id (the harness's share table).</summary>
        public readonly Dictionary<string, double> dealtBy = new Dictionary<string, double>();
        public readonly List<(string attack, int steps)> tellLog = new List<(string, int)>();

        public Brawl40(GameConfig cfg, Brawl40Params p, EventBus events, BossTarget target, Func<int> nextId, Tells36 tells)
        {
            this.p = p;
            this.events = events;
            this.target = target;
            this.nextId = nextId;
            this.tells = tells;
            dt0 = cfg.Dt;
            body = new Brawl40Body(this);
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public double HpFrac => body.maxHp > 0 ? Math.Max(0, body.hp) / body.maxHp : 0;
        public bool Fighting => state == Brawl40State.Idle || state == Brawl40State.Tell || state == Brawl40State.Active || state == Brawl40State.Stuck || state == Brawl40State.Recover || state == Brawl40State.Window;
        public bool Open => state == Brawl40State.Window;
        public bool StuckNow => state == Brawl40State.Stuck;
        public double StaggerFull => p.StaggerOf(phase);
        /// <summary>0 at the state's start, 1 at its end.</summary>
        public double Progress => stateTotal > 0 ? Math.Min(1, stateSteps / (double)stateTotal) : 0;
        public bool Telling(string id) => state == Brawl40State.Tell && attack == id;
        public bool Acting(string id) => state == Brawl40State.Active && attack == id;
        public Vec3 Feet => body.position;
        public Vec3 Centre => Base + new Vec3(0, p.height * 0.55, 0);
        public Vec3 Head => Base + new Vec3(0, p.height * 0.92, 0);
        Vec3 Side => new Vec3(facing.z, 0, -facing.x);
        /// <summary>The right fist (the haymaker's): raised and back in the wind-up, forward in the swing, on the ice when stuck.</summary>
        public Vec3 Fist
        {
            get
            {
                Vec3 f = Base + Side * (p.radius * 0.95);
                if (attack == Sweep || attack == Leap) return Fist42(f, true); // NIGHT42 B1 (brawl42; never with the row off)
                if (state == Brawl40State.Tell && attack == Haymaker) return f - facing * 0.6 + new Vec3(0, p.height * 0.85, 0);
                if (state == Brawl40State.Tell && (attack == Slam || attack == Block)) return f + facing * 0.3 + new Vec3(0, p.height * 1.05, 0);
                if (state == Brawl40State.Active && attack == Haymaker) return f + facing * (p.radius + 1.2) + new Vec3(0, p.height * 0.45, 0);
                if (state == Brawl40State.Stuck && attack == Haymaker) return f + facing * (p.radius + 1.4) + new Vec3(0, p.fistRadius * 0.6, 0);
                if ((state == Brawl40State.Active || state == Brawl40State.Recover) && attack == Slam) return f + facing * (p.radius * 0.6) + new Vec3(0, p.fistRadius * 0.6, 0);
                return f + facing * 0.4 + new Vec3(0, p.height * 0.35, 0);
            }
        }
        /// <summary>The left fist (both slam; the block is held in it).</summary>
        public Vec3 FistL
        {
            get
            {
                Vec3 f = Base - Side * (p.radius * 0.95);
                if (attack == Leap) return Fist42(f, false); // NIGHT42 B1 (brawl42; never with the row off)
                if (state == Brawl40State.Tell && (attack == Slam || attack == Block)) return f + facing * 0.3 + new Vec3(0, p.height * 1.05, 0);
                if ((state == Brawl40State.Active || state == Brawl40State.Recover) && attack == Slam) return f + facing * (p.radius * 0.6) + new Vec3(0, p.fistRadius * 0.6, 0);
                return f + facing * 0.4 + new Vec3(0, p.height * 0.35, 0);
            }
        }

        public void StartFight(BossFight ctx)
        {
            kit = ctx.kit;
            if (resume) return;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(p.pool * ctx.scale.hp);
            body.hp = body.maxHp;
            body.previous = body.position;
            rng = new Mulberry32(seed * 7919u + 40u);
            phase = 1;
            Phase3_47 = false; // NIGHT47 B brawl47
            Armored47 = crackWindow47 = false;
            armorBroken47 = armorTimedOut47 = 0;
            if (p.armorP1_47) Armor47(true);
            stagger = StaggerFull;
            attack = previous = forced = null;
            stepNo = campSteps = 0;
            campAnchor = target.position();
            ringOn = block.alive = false;
            haymakers = hayHits = hayDodged = windows = stuckHits = phaseChanges = campPicks = outside = 0;
            slams = slamHits = blocks = blockHits = blockParries = rushes = rushHits = rushWalls = floesCracked = floeTicks = 0;
            FloesReset();
            Reset42(); // NIGHT42 B1: brawl42's counters and the leap's height (nothing with the row off)
            dealt = dealtMelee = dealtStill = 0;
            dealtBy.Clear();
            tellLog.Clear();
            Face(target.position(), 10);
            Go(Brawl40State.Reveal, Math.Max(1, S(p.revealSeconds)));
            said.Reset("brawl40");
            said.name = p.name;
            said.hp = body.maxHp;
            events.Emit("bossRise", said);
            events.Emit("brawl40Rise", Say("rise", Centre));
        }

        /// <summary>Put it at a world point (feet), facing the player.</summary>
        public void Place(Vec3 feet)
        {
            body.position = Clamp(feet);
            body.previous = body.position;
            Face(target.position(), 10);
        }

        /// <summary>A retry (the player died in the fight): its bar and phase kept, the live attack, ring and block gone, a rest.</summary>
        public void Retry()
        {
            if (!body.alive || state == Brawl40State.Dying || state == Brawl40State.Dead) return;
            tells?.End(body.id);
            ringOn = block.alive = false;
            campSteps = 0;
            FloesReset(); // a retry starts on whole ice
            air42 = 0; // NIGHT42 B1: a retry mid-leap puts it on the ice
            campAnchor = target.position();
            if (state != Brawl40State.Reveal && state != Brawl40State.PhaseChange) ToIdle(Math.Max(p.restSeconds, 1.5));
        }

        public void Clear()
        {
            if (resume) return;
            body.alive = false;
            ringOn = block.alive = false;
            tells?.End(body.id);
            state = Brawl40State.Off;
        }

        public BossBar Bar() => new BossBar
        {
            valid = body.alive, id = body.id, name = p.name, phase = state.ToString(), hp = Math.Max(0, body.hp), max = body.maxHp, segment = p.phase2At,
        };

        void Go(Brawl40State s, int steps)
        {
            if (state == Brawl40State.Tell && attack != null) tellLog.Add((attack, stateSteps));
            state = s;
            stateSteps = 0;
            stateLeft = stateTotal = steps;
            entered = true;
        }

        Brawl40Event Say(string stage, Vec3 at)
        {
            ev.stage = stage;
            ev.attack = attack;
            ev.phase = phase;
            ev.stagger = stagger;
            ev.at = at;
            return ev;
        }

        static double RaySphere(Vec3 o, Vec3 d, Vec3 c, double r)
        {
            Vec3 oc = o - c;
            double b = Vec3.Dot(oc, d), cc = Vec3.Dot(oc, oc) - r * r, disc = b * b - cc;
            if (disc < 0) return -1;
            double sq = Math.Sqrt(disc), t = -b - sq;
            if (t < 0) t = -b + sq;
            return t >= 0 ? t : -1;
        }

        /// <summary>The ray's distance to the body (the torso, the hips, the head, the two fists; -1: a miss).</summary>
        public double Raycast(Vec3 o, Vec3 d)
        {
            if (!body.alive || state == Brawl40State.Dead) return -1;
            double best = -1;
            void Try(Vec3 c, double r)
            {
                double t = RaySphere(o, d, c, r);
                if (t >= 0 && (best < 0 || t < best)) best = t;
            }
            Try(Centre, p.radius);
            Try(Base + new Vec3(0, p.height * 0.25, 0), p.radius * 0.8);
            Try(Head, p.radius * 0.45);
            Try(Fist, p.fistRadius);
            Try(FistL, p.fistRadius);
            return best;
        }

        /// <summary>The player's distance to its body's surface (horizontal), the range rule's metres.</summary>
        public double RangeToPlayer()
        {
            Vec3 t = target.position();
            return Math.Max(0, JsMath.Hypot(t.x - body.position.x, t.z - body.position.z) - p.radius);
        }

        /// <summary>A gun's share at `metres` from the body: 1 within range.close, range.farMult at range.far and beyond, linear between.</summary>
        public double RangeMult(double metres)
        {
            if (metres <= p.rangeClose) return 1;
            if (metres >= p.rangeFar) return p.rangeFarMult;
            double k = (metres - p.rangeClose) / Math.Max(1e-6, p.rangeFar - p.rangeClose);
            return 1 + (p.rangeFarMult - 1) * k;
        }

        public static bool IsMelee(string weapon) => weapon == "gauntlet" || weapon == "punch";

        /// <summary>The share of a hit that lands (THE RULE above). Blocked outside the fight (the reveal, the phase change, the dying).</summary>
        public double Filter(double amount, string weapon, out bool blocked)
        {
            blocked = false;
            lastWeapon = weapon ?? "";
            if (!body.alive || !Fighting) { blocked = true; return 0; }
            double m = weapon == "parry31" || weapon == "parry36" || IsMelee(weapon) ? 1 : RangeMult(RangeToPlayer());
            if (state == Brawl40State.Stuck) m *= p.stuckMult;
            if (state == Brawl40State.Window) m *= crackWindow47 ? p.crackWindowMult47 : p.windowMult; // NIGHT47 B brawl47: the window that cracked the armour
            else if (state != Brawl40State.Stuck && Armored47) m *= p.armorChip47; // NIGHT47 B brawl47: ARMOURED (a phase's start, until its first WINDOW)
            else if (state != Brawl40State.Stuck && p.chip47 > 0) m *= p.chip47; // NIGHT47 B brawl47: the chip outside STUCK / WINDOW (0 = NIGHT46: x1)
            return amount * m;
        }

        /// <summary>The last hit's range multiplier (the sim's "TOO FAR" line reads it).</summary>
        public double LastRangeMult = 1;

        public void Apply(double dmg, string part)
        {
            if (!body.alive || dmg <= 0) return;
            string w = lastWeapon;
            lastWeapon = "";
            LastRangeMult = w.Length > 0 && !IsMelee(w) && w != "parry31" && w != "parry36" ? RangeMult(RangeToPlayer()) : 1;
            body.hp -= dmg;
            dealt += dmg;
            string key = w.Length > 0 ? w : part;
            dealtBy.TryGetValue(key, out double had);
            dealtBy[key] = had + dmg;
            if (state == Brawl40State.Stuck) stuckHits++;
            bool melee = IsMelee(w);
            if (melee) dealtMelee += dmg;
            if ((playerGrounded == null || playerGrounded()) && Vec3.DistanceTo(target.position(), campAnchor) < 0.5) dealtStill += dmg;
            body.flash = 0.06;
            said.Reset("brawl40");
            said.damage = JsMath.Round(dmg);
            said.part = key;
            said.core = Open;
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            if (melee) Drain(w == "gauntlet" ? p.gauntletStagger : p.punchStagger);
            if (body.hp <= 0)
            {
                body.hp = 0;
                tells?.End(body.id);
                attack = null;
                ringOn = block.alive = false;
                air42 = 0; // NIGHT42 B1: killed mid-leap, it falls where it is
                Go(Brawl40State.Dying, Math.Max(1, S(1.2)));
                kit?.Dying("brawl40", Centre);
                return;
            }
            if (phase == 1 && HpFrac <= p.phase2At) PhaseChange();
            else if (phase == 2 && !Phase3_47 && p.phase3At47 > 0 && HpFrac <= p.phase3At47 && state != Brawl40State.PhaseChange) Phase3Change47();
        }

        /// <summary>NIGHT47 B brawl47: in the third phase (only with design.n47.brawl47's phase3At on); reset at each fight's start.</summary>
        public bool Phase3_47 { get; private set; }

        /// <summary>NIGHT47 B brawl47: THE ICE ARMOUR is on (Brawl40Params.armorChip47); the steps it has left.</summary>
        public bool Armored47 { get; private set; }
        int armorLeft47;
        bool crackWindow47;
        /// <summary>NIGHT47 B brawl47: how many times the armour was broken by a WINDOW / ran out (the harness's).</summary>
        public int armorBroken47, armorTimedOut47;

        /// <summary>NIGHT47 B brawl47: armour now (the sim's, when its numbers reach a fight that began before them; nothing with armorChip47 0).</summary>
        public void Arm47() { if (!Armored47 && body.alive) Armor47(true); }
        /// <summary>NIGHT47 B brawl47: the sim breaks phase 3's armour (a charger baited into it, or the herd cleared).</summary>
        public void BreakArmor47() { if (Armored47) Armor47(false); }

        void Armor47(bool on)
        {
            if (on)
            {
                if (p.armorChip47 <= 0) return;
                Armored47 = true;
                armorLeft47 = Math.Max(1, S(p.armorMaxS47));
                events.Emit("brawl47Armor", Say("on", Centre));
                return;
            }
            if (!Armored47) return;
            Armored47 = false;
            if (armorLeft47 <= 0) armorTimedOut47++; else armorBroken47++;
            events.Emit("brawl47Armor", Say("off", Centre));
        }

        /// <summary>NIGHT47 B brawl47: the third phase's transition, PhaseChange's beat again (phase stays 2: the floes and the chains go on).</summary>
        void Phase3Change47()
        {
            Phase3_47 = true;
            phaseChanges++;
            tells?.End(body.id);
            attack = null;
            ringOn = false;
            air42 = 0;
            stagger = StaggerFull;
            int steps = Math.Max(1, S(p.phaseSeconds));
            if (kit != null) steps = Math.Max(steps, kit.Beat("brawl40", "PHASE 3", Centre));
            Go(Brawl40State.PhaseChange, steps);
            events.Emit("brawl47Phase3", Say("change", Centre));
            Armor47(true);
        }

        /// <summary>Its right move landed: drain the meter (x stuckMult while it is stuck); empty: the WINDOW.</summary>
        public void Drain(double amount)
        {
            if (!Fighting || Open || amount <= 0) return;
            if (state == Brawl40State.Stuck) amount *= p.stuckMult;
            stagger = Math.Max(0, stagger - amount);
            events.Emit("brawl40Stagger", Say("drain", Centre));
            if (stagger > 0) return;
            windows++;
            tells?.End(body.id);
            attack = null;
            Go(Brawl40State.Window, Math.Max(1, S(p.windowSeconds)));
            crackWindow47 = Armored47 && p.crackWindowMult47 > 0; // NIGHT47 B brawl47: a window that opens on the armour cracks it: it lands crackWindowMult47
            if (Armored47 && !(Phase3_47 && p.armorP3Herd47)) Armor47(false); // NIGHT47 B brawl47: the first WINDOW of a phase breaks the ice armour (phase 3's: the herd's)
            events.Emit("brawl40Stagger", Say("open", Centre));
        }

        void PhaseChange()
        {
            phase = 2;
            phaseChanges++;
            tells?.End(body.id);
            attack = null;
            ringOn = false;
            air42 = 0; // NIGHT42 B1: the phase change mid-leap puts it on the ice
            stagger = StaggerFull;
            int steps = Math.Max(1, S(p.phaseSeconds));
            if (kit != null) steps = Math.Max(steps, kit.Beat("brawl40", "PHASE 2", Centre));
            Go(Brawl40State.PhaseChange, steps);
            events.Emit("brawl40Phase", Say("change", Centre));
            Armor47(true); // NIGHT47 B brawl47 (nothing with armorChip47 0)
        }

        Capsule PlayerCapsule()
        {
            if (playerBody != null) return playerBody();
            Vec3 f = target.position();
            return new Capsule { start = new Vec3(f.x, f.y + 0.4, f.z), end = new Vec3(f.x, f.y + 1.4, f.z), radius = 0.4 };
        }

        bool HurtPlayer(double amount)
        {
            Health h = target.health;
            if (h == null || h.Dead) return false;
            hurt42 += amount; // NIGHT42 B1: a stat only (the harness's "harder" number)
            return h.Damage(amount, "boss");
        }

        static double SegDist(Vec3 pt, Vec3 a, Vec3 b)
        {
            Vec3 ab = b - a, ap = pt - a;
            double len2 = Vec3.Dot(ab, ab), t = len2 < 1e-12 ? 0 : Math.Max(0, Math.Min(1, Vec3.Dot(ap, ab) / len2));
            return Vec3.DistanceTo(pt, a + ab * t);
        }

        void Face(Vec3 at, double maxTurn)
        {
            double dx = at.x - body.position.x, dz = at.z - body.position.z;
            if (JsMath.Hypot(dx, dz) < 1e-6) return;
            double want = JsMath.Atan2(dx, dz), d = want - yaw;
            while (d > Math.PI) d -= 2 * Math.PI;
            while (d < -Math.PI) d += 2 * Math.PI;
            yaw += Math.Max(-maxTurn, Math.Min(maxTurn, d));
            facing = new Vec3(JsMath.Sin(yaw), 0, JsMath.Cos(yaw));
        }

        /// <summary>A world point clamped to the arena rect minus (radius + margin), pushed out of every pillar (boss_rules 4).</summary>
        public Vec3 Clamp(Vec3 w)
        {
            double lim = half - p.radius - p.margin;
            double x = Math.Max(origin.x - lim, Math.Min(origin.x + lim, w.x)), z = Math.Max(origin.z - lim, Math.Min(origin.z + lim, w.z));
            foreach (var (mn, mx) in pillars)
            {
                double r = p.radius;
                double cx = Math.Max(mn.x, Math.Min(mx.x, x)), cz = Math.Max(mn.z, Math.Min(mx.z, z));
                double dx = x - cx, dz = z - cz, d = JsMath.Hypot(dx, dz);
                if (d >= r) continue;
                if (d < 1e-6)
                {
                    // inside the box: out through the nearest face
                    double l = x - mn.x, rr = mx.x - x, b = z - mn.z, f = mx.z - z, m = Math.Min(Math.Min(l, rr), Math.Min(b, f));
                    if (m == l) x = mn.x - r; else if (m == rr) x = mx.x + r; else if (m == b) z = mn.z - r; else z = mx.z + r;
                }
                else { x = cx + dx / d * r; z = cz + dz / d * r; }
            }
            return new Vec3(x, origin.y, z);
        }

        /// <summary>Is its body outside the arena rect (minus its radius)? The harness's boss_rules 4 count.</summary>
        public bool OutsideArena => Math.Abs(body.position.x - origin.x) > half - p.radius + 1e-6 || Math.Abs(body.position.z - origin.z) > half - p.radius + 1e-6;

        // ---- the punch (the sim's "punch" / gauntlet "meleeSwing" events): the block's parry ----------------------------------------------------

        /// <summary>A punch or a gauntlet swing from `eye` along `dir`: parries the block in reach. Returns what it did ("" nothing).</summary>
        public string Punch(Vec3 eye, Vec3 dirV)
        {
            lastPunchStep = stepNo;
            punchUsed = false;
            punchEye = eye;
            punchDir = dirV;
            if (!body.alive) return "";
            return TryBlockParry(eye, dirV) ? "block" : "";
        }

        bool TryBlockParry(Vec3 eye, Vec3 dirV)
        {
            if (!block.alive || block.back) return false;
            Vec3 to = block.pos - eye;
            double d = to.Length;
            if (d > p.blockParryReach + p.blockRadius) return false;
            if (d > 0.5 && Vec3.Dot(to, dirV) / d < JsMath.Cos(p.blockParryCone * Math.PI / 180)) return false;
            block.back = true;
            block.life = S(3);
            punchUsed = true;
            blockParries++;
            parried.owner = "player";
            parried.weapon = "punch";
            parried.kind = Block;
            parried.enemy = body.id;
            parried.damage = p.blockParryDamage;
            parried.position = block.pos;
            events.Emit("parry36", parried);
            events.Emit("brawl40Block", Say("parried", block.pos));
            return true;
        }

        // ---- one step -----------------------------------------------------------------------------------------------------------------------

        public void Step(double dt)
        {
            if (!body.alive) return;
            body.previous = body.position;
            entered = false;
            if (Frozen) return;
            stepNo++;
            stateSteps++;
            if (Armored47 && --armorLeft47 <= 0) Armor47(false); // NIGHT47 B brawl47: the ice armour's time runs out
            Vec3 pw = target.position();
            double dist = RangeToPlayer();
            Camp(pw);
            if (!punchUsed && stepNo - lastPunchStep <= S(p.blockBuffer)) TryBlockParry(punchEye, punchDir); // the parry's early buffer
            switch (state)
            {
                case Brawl40State.Reveal:
                    Face(pw, p.turnRate * dt);
                    if (--stateLeft <= 0) ToIdle(p.restSeconds);
                    break;
                case Brawl40State.Idle:
                    Walk(pw, dist, dt);
                    if (--stateLeft <= 0) Pick(pw, dist);
                    break;
                case Brawl40State.Tell:
                    if (attack == Haymaker ? Progress < p.hayTrack : attack != Rush || Progress < 0.6) Face(pw, p.turnRate * 1.5 * dt);
                    if (attack == Rush) { rushFrom = body.position; rushDir = facing; }
                    if (attack == Leap) TellLeap(pw); // NIGHT42 B1: the landing circle tracks, then locks
                    tells?.Move(body.id, TellAt(attack));
                    if (--stateLeft <= 0) Active(pw);
                    break;
                case Brawl40State.Active:
                    if (attack == Haymaker) StepHaymaker(dt);
                    else if (attack == Rush) { StepRush(dt); break; }
                    else if (attack == Sweep) StepSweep(); // NIGHT42 B1
                    else if (attack == Leap) StepLeap(); // NIGHT42 B1
                    if (--stateLeft <= 0) EndActive();
                    break;
                case Brawl40State.Stuck:
                    if (--stateLeft <= 0)
                    {
                        if (attack == Leap) { events.Emit("brawl42Leap", Say("free", Fist)); Go(Brawl40State.Recover, Math.Max(1, S(p42.leapRecover))); break; } // NIGHT42 B1
                        events.Emit(attack == Rush ? "brawl40Rush" : "brawl40Haymaker", Say("free", Fist));
                        Go(Brawl40State.Recover, Math.Max(1, S(attack == Rush ? p.rushRecover : p.hayRecover)));
                    }
                    break;
                case Brawl40State.Recover:
                    Face(pw, p.turnRate * (attack == Sweep ? 0.25 : 0.5) * dt); // NIGHT42 B1: dizzy after the sweep (never with the row off)
                    if (--stateLeft <= 0)
                    {
                        // phase 2's chain: a haymaker up close goes straight into a slam
                        if (phase == 2 && attack == Haymaker && !chained && dist <= p.hayStart + 1 && rng.Next() < p.slamChain2) { chained = true; StartAttack(Slam, false); }
                        // NIGHT42 B1 (brawl42): else, phase 2, into a sweep (the punish window ends in a tell you jump)
                        else if (on42 && phase == 2 && attack == Haymaker && !chained && dist <= p42.sweepStart && rng.Next() < p42.sweepChain2) { chained = true; sweepChains++; StartAttack(Sweep, false); }
                        else ToIdle(RestNow);
                    }
                    break;
                case Brawl40State.Window:
                    if (--stateLeft <= 0)
                    {
                        stagger = StaggerFull;
                        events.Emit("brawl40Stagger", Say("close", Centre));
                        ToIdle(RestNow);
                    }
                    break;
                case Brawl40State.PhaseChange:
                    Face(pw, p.turnRate * dt);
                    if (--stateLeft <= 0)
                    {
                        events.Emit("brawl40Phase", Say("start", Centre));
                        ToIdle(on42 ? p42.rest2 : p.restSeconds2);
                    }
                    break;
                case Brawl40State.Dying:
                    if (--stateLeft <= 0)
                    {
                        state = Brawl40State.Dead;
                        body.alive = false;
                        kit?.Down("brawl40", body.id, Centre);
                    }
                    break;
            }
            StepRing(dt);
            StepFloes(dt);
            StepBlock(dt);
            if (air42 != 0 && !(state == Brawl40State.Active && attack == Leap)) air42 = 0; // NIGHT42 B1: off a leap (a window, a phase change) it is on the ice
            body.position = Clamp(body.position);
            if (OutsideArena) outside++;
            body.velocity = (body.position - body.previous) / dt;
        }

        void ToIdle(double seconds)
        {
            attack = null;
            chained = false;
            Go(Brawl40State.Idle, Math.Max(1, S(seconds)));
        }

        void Camp(Vec3 pw)
        {
            double dx = pw.x - campAnchor.x, dz = pw.z - campAnchor.z;
            if (dx * dx + dz * dz > p.campRadius * p.campRadius || (playerGrounded != null && !playerGrounded())) { campAnchor = pw; campSteps = 0; }
            else campSteps++;
        }

        public bool CampDue => campSteps >= S(p.campSeconds);

        /// <summary>Walk at the player (phase 2 faster), stopping stopDist off its body; turn toward him at turnRate.</summary>
        void Walk(Vec3 pw, double dist, double dt)
        {
            Face(pw, p.turnRate * dt);
            if (dist <= p.stopDist) return;
            double v = phase == 1 ? p.walkSpeed : p.walkSpeed2;
            double stepLen = Math.Min(v * dt, dist - p.stopDist);
            body.position = body.position + facing * stepLen;
        }

        /// <summary>The next attack. Camp due: the slam. Up close: the haymaker. Mid range: the slam (slam.midChance) or keep walking. Far (past
        /// block.startDist): the block, or past rush.startDist the rush (rush.chance). Never the same ranged attack twice running.</summary>
        void Pick(Vec3 pw, double dist)
        {
            if (forced != null) { string f = forced; forced = null; StartAttack(f, false); return; }
            if (CampDue) { campPicks++; StartAttack(Slam, true); return; }
            if (on42 && Pick42(dist)) return; // NIGHT42 B1 (brawl42): the sweep up close, the leap from mid range
            if (dist <= p.hayStart) { StartAttack(Haymaker, false); return; }
            if (dist > p.rushStartDist)
            {
                bool rush = previous == Block || (previous != Rush && rng.Next() < p.rushChance);
                StartAttack(rush ? Rush : Block, false);
                return;
            }
            if (dist > p.blockStartDist && previous != Block) { StartAttack(Block, false); return; }
            if (previous != Slam && rng.Next() < p.slamMidChance) { StartAttack(Slam, false); return; }
            stateLeft = S(0.25); // keep walking; look again soon
        }

        string forced;

        /// <summary>The reel's way to see every attack: the next pick is `id` whatever the distance (Haymaker, Slam, Block, Rush).</summary>
        public void Force(string id) => forced = id;

        /// <summary>A forced attack is queued and not yet taken.</summary>
        public bool ForcePending => forced != null;

        void StartAttack(string id, bool camp)
        {
            attack = id;
            previous = id;
            hitThisAttack = false;
            double w;
            Tell36Kind kind = Tell36Kind.Red;
            switch (id)
            {
                case Haymaker: haymakers++; w = phase == 1 ? p.hayWindup : p.hayWindup2; break;
                case Slam: slams++; w = phase == 1 ? p.slamWindup : p.slamWindup2; if (camp) { campSteps = 0; campAnchor = target.position(); } break;
                case Block: blocks++; w = p.blockWindup; kind = Tell36Kind.Yellow; break;
                case Sweep: sweeps++; w = phase == 1 ? p42.sweepWindup : p42.sweepWindup2; break; // NIGHT42 B1 (only with brawl42 on)
                case Leap: leaps++; w = p42.leapWindup; leapTo = Clamp(target.position()); leapLocked = false; break; // NIGHT42 B1
                default: rushes++; w = p.rushWindup; break;
            }
            int steps = Math.Max(1, S(w));
            Go(Brawl40State.Tell, steps);
            Vec3 at = TellAt(id);
            tells?.Raise(body.id, id, kind, steps, at);
            events.Emit(EventOf(id), Say("tell", at));
        }

        static string EventOf(string id) => id == Haymaker ? "brawl40Haymaker" : id == Slam ? "brawl40Slam" : id == Block ? "brawl40Block" : id == Sweep ? "brawl42Sweep" : id == Leap ? "brawl42Leap" : "brawl40Rush";

        /// <summary>Where an attack's tell36 sits (the rush and the leap: its head; the block: the left fist; else the right fist).</summary>
        Vec3 TellAt(string id) => id == Rush || id == Leap ? Head : id == Block ? FistL : Fist;

        void Active(Vec3 pw)
        {
            tells?.End(body.id);
            hitThisAttack = false;
            switch (attack)
            {
                case Haymaker:
                    lungeDir = facing;
                    Go(Brawl40State.Active, Math.Max(1, S(p.hayActive)));
                    events.Emit("brawl40Haymaker", Say("swing", Fist));
                    break;
                case Slam:
                    ringOn = true;
                    ringHit = false;
                    ringR = p.radius;
                    ringAt = body.position;
                    Go(Brawl40State.Active, 1);
                    events.Emit("brawl40Slam", Say("ring", body.position));
                    break;
                case Block:
                    {
                        Vec3 from = FistL;
                        double dist = Vec3.DistanceTo(from, pw);
                        double tf = Math.Max(0.35, dist / p.blockSpeed);
                        Vec3 aim = pw + new Vec3(0, 1.0, 0);
                        Vec3 v = (aim - from) / tf;
                        v = new Vec3(v.x, v.y + 0.5 * p.blockGravity * tf, v.z);
                        block.pos = block.prev = from;
                        block.vel = v;
                        block.life = S(tf * 2.5);
                        block.alive = true;
                        block.back = false;
                        Go(Brawl40State.Active, 1);
                        events.Emit("brawl40Block", Say("throw", from));
                        break;
                    }
                case Sweep: ActiveSweep(pw); break; // NIGHT42 B1
                case Leap: ActiveLeap(); break; // NIGHT42 B1
                default:
                    rushFrom = body.position;
                    rushDir = facing;
                    rushLeft = Math.Max(1, S(p.rushMaxSeconds));
                    Go(Brawl40State.Active, rushLeft);
                    events.Emit("brawl40Rush", Say("charge", Head));
                    break;
            }
        }

        void EndActive()
        {
            switch (attack)
            {
                case Haymaker:
                    if (!hitThisAttack) hayDodged++;
                    Go(Brawl40State.Stuck, Math.Max(1, S(p.stuckSeconds)));
                    events.Emit("brawl40Haymaker", Say("stuck", Fist));
                    break;
                case Slam:
                    Go(Brawl40State.Recover, Math.Max(1, S(p.slamRecover)));
                    break;
                case Sweep: EndSweep(); break; // NIGHT42 B1
                case Leap: Land(); break; // NIGHT42 B1
                default:
                    Go(Brawl40State.Recover, Math.Max(1, S(p.blockRecover)));
                    break;
            }
        }

        /// <summary>The swing: a lunge along its committed facing and one hit on a player inside the cone and reach.</summary>
        void StepHaymaker(double dt)
        {
            body.position = body.position + lungeDir * (p.hayLunge / Math.Max(dt, p.hayActive) * dt);
            if (hitThisAttack) return;
            Capsule cap = PlayerCapsule();
            Vec3 pc = (cap.start + cap.end) * 0.5;
            double dx = pc.x - body.position.x, dz = pc.z - body.position.z, d = JsMath.Hypot(dx, dz);
            if (d - cap.radius > p.radius + p.hayReach) return;
            if (cap.start.y - cap.radius > body.position.y + p.height) return; // over its head: a jump answers it
            if (d > 1e-6 && (dx * lungeDir.x + dz * lungeDir.z) / d < JsMath.Cos(p.hayCone * Math.PI / 180)) return;
            hitThisAttack = true;
            hayHits++;
            HurtPlayer(p.hayDamage);
            target.impulse?.Invoke(lungeDir.x * p.hayKnock, p.hayKnockUp, lungeDir.z * p.hayKnock);
            events.Emit("brawl40Haymaker", Say("hit", Fist));
        }

        /// <summary>The rush: along its locked line at rush.speed until rush.maxSeconds, the player hit (once) or a wall / pillar stops it (dazed).</summary>
        void StepRush(double dt)
        {
            Vec3 want = body.position + rushDir * (p.rushSpeed * dt);
            Vec3 got = Clamp(want);
            body.position = got;
            if (!hitThisAttack)
            {
                Capsule cap = PlayerCapsule();
                Vec3 c = Centre;
                if (SegDist(new Vec3(c.x, Math.Min(c.y, cap.end.y), c.z), cap.start, cap.end) <= p.radius + p.rushHitRadius + cap.radius)
                {
                    hitThisAttack = true;
                    rushHits++;
                    HurtPlayer(p.rushDamage);
                    target.impulse?.Invoke(rushDir.x * p.rushKnock, 5, rushDir.z * p.rushKnock);
                    events.Emit("brawl40Rush", Say("hit", Head));
                }
            }
            bool wall = Vec3.DistanceTo(got, want) > 0.02;
            if (wall)
            {
                rushWalls++;
                Go(Brawl40State.Stuck, Math.Max(1, S(p.rushStuckSeconds)));
                events.Emit("brawl40Rush", Say("wall", Head));
                return;
            }
            if (--stateLeft <= 0) Go(Brawl40State.Recover, Math.Max(1, S(p.rushRecover)));
        }

        /// <summary>The slam's ring: it runs out along the floor; a player whose feet are within ring.height of the floor where it passes is hit once.</summary>
        void StepRing(double dt)
        {
            if (!ringOn) return;
            double before = ringR;
            ringR += p.slamSpeed * dt;
            if (ringR > p.slamRadius) { ringOn = false; return; }
            if (ringHit) return;
            Vec3 f = target.position();
            double d = JsMath.Hypot(f.x - ringAt.x, f.z - ringAt.z);
            bool crossed = d >= before - p.slamWidth && d <= ringR + p.slamWidth;
            if (crossed && f.y - ringAt.y <= p.slamHeight)
            {
                ringHit = true;
                slamHits++;
                HurtPlayer(p.slamDamage);
                target.impulse?.Invoke(0, p.slamKnockUp, 0);
                events.Emit("brawl40Slam", Say("hit", f));
            }
        }

        // ---- phase 2's FLOES: the ice cracks (NIGHT40.md B2: "Phase 2 at 50 %: faster chains, the ice cracking into floes") -----------------
        // The floor is a floes.grid x floes.grid grid of floes over the arena. In phase 2, every floes.every seconds the floe the player will be on
        // floes.lead seconds from now (where he runs) cracks: RED for floes.tell (the view tints it), then it is OPEN WATER for floes.open: feet
        // on it (grounded) lose floes.dps health a second, in floes.tick bites. No geometry changes (the room's own floor stays under it): the
        // arena changes what is safe to stand on, and a player who keeps moving (or rides a wall) never pays (boss_rules 3 and 6).

        /// <summary>Each floe's RED steps left (0: none) and its open-water steps left (0: ice).</summary>
        public int[] floeTellLeft = new int[0], floeOpenLeft = new int[0];
        int floeTimer, floeBite;

        void FloesReset()
        {
            int n = Math.Max(1, p.floeGrid) * Math.Max(1, p.floeGrid);
            if (floeTellLeft.Length != n) { floeTellLeft = new int[n]; floeOpenLeft = new int[n]; }
            Array.Clear(floeTellLeft, 0, n);
            Array.Clear(floeOpenLeft, 0, n);
            floeTimer = floeBite = 0;
        }

        /// <summary>The floe index under a world point (-1: off the arena).</summary>
        public int FloeAt(Vec3 w)
        {
            int g = Math.Max(1, p.floeGrid);
            double size = 2 * half / g;
            int ix = (int)Math.Floor((w.x - (origin.x - half)) / size), iz = (int)Math.Floor((w.z - (origin.z - half)) / size);
            return ix < 0 || iz < 0 || ix >= g || iz >= g ? -1 : iz * g + ix;
        }

        /// <summary>A floe's rect (world, x0 z0 x1 z1).</summary>
        public void FloeRect(int i, out double x0, out double z0, out double x1, out double z1)
        {
            int g = Math.Max(1, p.floeGrid);
            double size = 2 * half / g;
            x0 = origin.x - half + (i % g) * size;
            z0 = origin.z - half + (i / g) * size;
            x1 = x0 + size;
            z1 = z0 + size;
        }

        /// <summary>Is the floe at a world point cracking (RED) or open water now?</summary>
        public bool FloeBad(Vec3 w)
        {
            int i = FloeAt(w);
            return i >= 0 && (floeTellLeft[i] > 0 || floeOpenLeft[i] > 0);
        }

        void StepFloes(double dt)
        {
            if (floeTellLeft.Length == 0) return;
            for (int i = 0; i < floeTellLeft.Length; i++)
            {
                if (floeTellLeft[i] > 0 && --floeTellLeft[i] == 0) { floeOpenLeft[i] = Math.Max(1, S(p.floeOpen)); events.Emit("brawl40Floe", Say("open", Centre)); }
                else if (floeOpenLeft[i] > 0) floeOpenLeft[i]--;
            }
            bool live = phase == 2 && (Fighting || state == Brawl40State.PhaseChange);
            if (!live) return;
            // the bite: feet on open water
            Vec3 f = target.position();
            int under = FloeAt(f);
            bool onWater = under >= 0 && floeOpenLeft[under] > 0 && f.y - origin.y < 0.3 && (playerGrounded == null || playerGrounded());
            if (onWater)
            {
                if (++floeBite >= Math.Max(1, S(p.floeTick)))
                {
                    floeBite = 0;
                    floeTicks++;
                    HurtPlayer(p.floeDps * p.floeTick);
                    events.Emit("brawl40Floe", Say("bite", f));
                }
            }
            else floeBite = 0;
            if (++floeTimer < Math.Max(1, S(p.floeEvery))) return;
            floeTimer = 0;
            // where he will be floes.lead seconds from now (where he runs), else where he stands
            Vec3 v = playerVelocityOr();
            int pick = FloeAt(new Vec3(f.x + v.x * p.floeLead, f.y, f.z + v.z * p.floeLead));
            if (pick < 0 || floeTellLeft[pick] > 0 || floeOpenLeft[pick] > 0) pick = under;
            if (pick < 0 || floeTellLeft[pick] > 0 || floeOpenLeft[pick] > 0) return;
            floeTellLeft[pick] = Math.Max(1, S(p.floeTell));
            floesCracked++;
            events.Emit("brawl40Floe", Say("crack", Centre));
        }

        /// <summary>The player's velocity (the sim sets the reader; zero without one).</summary>
        public Func<Vec3> playerVelocity;
        Vec3 playerVelocityOr() => playerVelocity != null ? playerVelocity() : default;

        void StepBlock(double dt)
        {
            if (!block.alive) return;
            block.prev = block.pos;
            if (block.back)
            {
                Vec3 home = Centre - block.pos;
                double len = home.Length;
                if (len <= p.radius + p.blockRadius || len <= p.blockReturn * dt)
                {
                    block.alive = false;
                    events.Emit("brawl40Block", Say("return", Centre));
                    parryHit?.Invoke(p.blockParryDamage, Centre);
                    Drain(p.blockStagger);
                    return;
                }
                block.vel = home * (p.blockReturn / len);
                block.pos = block.pos + block.vel * dt;
                if (--block.life <= 0) block.alive = false;
                return;
            }
            block.vel = new Vec3(block.vel.x, block.vel.y - p.blockGravity * dt, block.vel.z);
            block.pos = block.pos + block.vel * dt;
            Capsule cap = PlayerCapsule();
            if (SegDist(block.pos, cap.start, cap.end) < p.blockRadius + cap.radius)
            {
                block.alive = false;
                blockHits++;
                HurtPlayer(p.blockDamage);
                events.Emit("brawl40Block", Say("hit", block.pos));
                return;
            }
            if (--block.life <= 0 || block.pos.y < origin.y - 0.2) { block.alive = false; events.Emit("brawl40Block", Say("shatter", block.pos)); }
        }
    }
}
