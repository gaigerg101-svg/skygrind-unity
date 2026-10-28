// NIGHT28 builder B2 (boss): THE GLACIER as a LAP CHASE on a closed oval rink (Research/2026-10-09-wave28/GLACIER-REDESIGN.md sections 3.3-3.6; GG: "the second boss fight, it's just so horrible again ...
// the boss fight is still super boring and not fun, doesn't encourage unique gameplay"). The boss lives on the arc coordinate s of Rink28Math's centre line (a lap of about 84 m) and FLEES from you along
// the shorter arc; the verbs that catch it are the movement verbs (slide the bank, dash, jump), and the only thing that hurts it is the PULSE blast, three hits.
//   GAP CONTROLLER  gap = the metres along the boss's flee direction from your projection to the boss. Its speed = base + clamp(k x (gap - target), +-clampFrac x slideFloor); the target depends on the
//                   glove (ready: the boss is tired and can be run down; spent: it runs flat out). Gap based, never a timer. Reuses Glacier27's rules (SpeedAt, the window, a miss costs gap never health).
//   FLEE DIRECTION  flips only when the other arc is shorter by more than `hysteresis` m (no oscillation at the far side). A flip is a visible SKID (the speed brakes through zero and builds back the
//                   other way: about skidSeconds). Passing the boss THROUGH A DOOR OF THE ISLAND is such a flip: the CUT, and a pulse hit during it counts even when glazed, once per phase.
//   WINDOW          the gap under windowAt (12 m) and the glove ready: a window of windowTime (1.5 s); a PULSE blast that lands on the boss counts ONCE; a window that runs out costs missCost m
//                   (the boss runs on over surgeSeconds: never a teleport, never health). Three hits kill; the first two start phases 2 and 3; after a hit the boss surges hitBack m ahead.
//   THE GLAZE       the boss wears ice plates: a blast that reaches it glazed CLANGS OFF (clangCost m of gap, the glove refilled). The pistol (8 hits, or one charged shot) or the shotgun (a hit within
//                   10 m) cracks it for openSeconds; a window needs it cracked (or a cut). Guns never hurt it; only the pulse does.
//   ATTACKS         every attack forces a different movement: P1 SHARD TRAIL (take the line the trail leaves open: the bank), P2 THAW MELT STRIP on a straight (ride the rim wall over it), P3 ICE COLUMNS
//                   (dash slalom), ROLLER (jump), LOW BAR (slide), CHASM (jump / launch). A phase starts again from its start when you die (Lantern27's pattern).
// Fixed 120 Hz, timers in whole steps, Mulberry32 + JsMath, no UnityEngine, no audio, no PhysX. A new module (the web has no glacier). The NIGHT27 chase (Glacier27.cs) is untouched.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.n28.glacier (+ the rink's numbers, design.n28.rink via Rink28Params), read once.</summary>
    public sealed class Glacier28Params
    {
        public string name = "THE GLACIER";
        public double pool;
        public int hitsToKill;
        public double L, rc, laneHalf, rimLateral, islandEdge, islandRadius, doorHalf, bankRun, tanBank, spawnS;
        public double[] origin = new double[3];
        public uint salt;
        public double baseSpeed, slideFloor, clampFrac, k, targetGap, spentTarget, startGap, hysteresis, skidSeconds, flipHold, introHold;
        public double windowAt, windowTime, missCost, armTime, hitBack, surgeSeconds;
        public double reach, coneDeg, refill;
        public double[] glazeHits;
        public double glazeCharged, glazeChargedHits, shotgunRange, glazeOpen, clangCost, revolverDamage;
        public double cutMemory, cutHold;
        public int shardCount;
        public double shardSize, shardTell, shardLife, shardDamage, shardPush, shardEvery, shardSpacing, shardLead, shardMinGap;
        public double[] shardLateral;
        public double meltLength, meltFirst, meltTell, meltLive, meltSpacing, meltDps, meltStand, meltTick;
        public bool meltSafeFirst;
        public int colCount;
        public double colSize, colHeight, colOffset, colSpacing, colTell, colLife, colDamage, colPush, p3Every;
        public double rollRadius, rollTell, rollSpeed, rollLife, rollAhead, rollDamage;
        public double barBottom, barTop, barThick, barTell, barLife, barAhead, barDamage;
        public double chasmLength, chasmTell, chasmLife, chasmAhead, chasmDamage, chasmPush;
        public double boxW, boxH, boxL, dieTime;
        public double capsuleHeight, capsuleRadius;

        public static Glacier28Params From(GameConfig c) => From(c, Rink28Params.From(c));

        /// <summary>The same, on a given rink (NIGHT30 B: design.n30.glacier30's bigger rink; the overload above is NIGHT28's, bit for bit).</summary>
        public static Glacier28Params From(GameConfig c, Rink28Params r)
        {
            const string g = "design.n28.glacier.";
            const string b = "design.n27.boss2.body."; // the same blocky skater: the NIGHT27 body numbers (the view's Design/bosses/glacier27.json fits it)
            const string h = "design.n27.hazardFloor27."; // the melt strip's times and damage are the hazardFloor27 numbers
            double rad = r.bankDeg * Math.PI / 180;
            var p = new Glacier28Params
            {
                name = c.Str(g + "name"), pool = c.Num(g + "hits.pool"), hitsToKill = c.Int(g + "hits.toKill"),
                L = r.straight, rc = r.centreRadius, islandRadius = r.islandRadius, doorHalf = r.doorWidth / 2, bankRun = r.bankRun, tanBank = JsMath.Sin(rad) / JsMath.Cos(rad),
                laneHalf = r.laneWidth / 2, islandEdge = r.islandRadius - r.centreRadius, rimLateral = r.islandRadius - r.centreRadius + r.laneWidth + r.bankRun + r.lip,
                spawnS = r.playerStartS, startGap = r.spawnGap, salt = (uint)c.Num("design.n27.boss2.room.salt"),
                baseSpeed = c.Num(g + "speed.base"), slideFloor = c.Num(g + "speed.slideFloor"), clampFrac = c.Num(g + "speed.clampFrac"), k = c.Num(g + "speed.k"),
                targetGap = c.Num(g + "speed.targetGap"), spentTarget = c.Num(g + "speed.spentTarget"), hysteresis = c.Num(g + "speed.hysteresis"), skidSeconds = c.Num(g + "speed.skidSeconds"),
                flipHold = c.Num(g + "speed.flipHoldSeconds"), introHold = c.Num(g + "speed.introHold"),
                windowAt = c.Num(g + "gap.windowAt"), windowTime = c.Num(g + "gap.windowTime"), missCost = c.Num(g + "gap.missCost"), armTime = c.Num(g + "gap.armTime"), hitBack = c.Num(g + "gap.hitBack"), surgeSeconds = c.Num(g + "gap.surgeSeconds"),
                reach = c.Num(g + "launch.reach"), coneDeg = c.Num(g + "launch.coneDeg"), refill = c.Num(g + "launch.refill"),
                glazeHits = c.Nums(g + "glaze.hits"), glazeCharged = c.Num(g + "glaze.chargedFactor"), glazeChargedHits = c.Num(g + "glaze.chargedHits"), shotgunRange = c.Num(g + "glaze.shotgunRange"),
                glazeOpen = c.Num(g + "glaze.openSeconds"), clangCost = c.Num(g + "glaze.clangCost"), revolverDamage = c.Num("weapons.revolver.damage"),
                cutMemory = c.Num(g + "cut.memorySeconds"), cutHold = c.Num(g + "cut.holdSeconds"),
                shardCount = c.Int(g + "shards.count"), shardSize = c.Num(g + "shards.size"), shardTell = c.Num(g + "shards.tell"), shardLife = c.Num(g + "shards.life"), shardDamage = c.Num(g + "shards.damage"),
                shardPush = c.Num(g + "shards.push"), shardEvery = c.Num(g + "shards.every"), shardSpacing = c.Num(g + "shards.spacing"), shardLead = c.Num(g + "shards.leadMetres"), shardMinGap = c.Num(g + "shards.minGap"),
                shardLateral = c.Nums(g + "shards.lateral"),
                meltLength = c.Num(g + "melt.length"), meltFirst = c.Num(g + "melt.firstStartSeconds"), meltSafeFirst = c.Bool(g + "melt.safeFirst"),
                meltTell = c.Num(h + "telegraphSeconds"), meltLive = c.Num(h + "liveSeconds"), meltSpacing = c.Num(h + "spacingSeconds"), meltDps = c.Num(h + "damagePerSecond"), meltStand = c.Num(h + "standHeight"), meltTick = c.Num(h + "tickSeconds"),
                colCount = c.Int(g + "columns.count"), colSize = c.Num(g + "columns.size"), colHeight = c.Num(g + "columns.height"), colOffset = c.Num(g + "columns.offset"), colSpacing = c.Num(g + "columns.spacing"),
                colTell = c.Num(g + "columns.tell"), colLife = c.Num(g + "columns.life"), colDamage = c.Num(g + "columns.damage"), colPush = c.Num(g + "columns.push"), p3Every = c.Num(g + "columns.every"),
                rollRadius = c.Num(g + "roller.radius"), rollTell = c.Num(g + "roller.tell"), rollSpeed = c.Num(g + "roller.speed"), rollLife = c.Num(g + "roller.life"), rollAhead = c.Num(g + "roller.ahead"), rollDamage = c.Num(g + "roller.damage"),
                barBottom = c.Num(g + "bar.bottom"), barTop = c.Num(g + "bar.top"), barThick = c.Num(g + "bar.thick"), barTell = c.Num(g + "bar.tell"), barLife = c.Num(g + "bar.life"), barAhead = c.Num(g + "bar.ahead"), barDamage = c.Num(g + "bar.damage"),
                chasmLength = c.Num(g + "chasm.length"), chasmTell = c.Num(g + "chasm.tell"), chasmLife = c.Num(g + "chasm.life"), chasmAhead = c.Num(g + "chasm.ahead"), chasmDamage = c.Num(g + "chasm.damage"), chasmPush = c.Num(g + "chasm.push"),
                boxW = c.Num(b + "boxW"), boxH = c.Num(b + "boxH"), boxL = c.Num(b + "boxL"), dieTime = c.Num(b + "dieTime"),
                capsuleHeight = c.Num("body.capsuleHeight"), capsuleRadius = c.Num("body.capsuleRadius"),
            };
            p.origin[0] = r.cx; p.origin[1] = r.cy; p.origin[2] = r.cz;
            return p;
        }
    }

    public sealed class Glacier28Body : Enemy, IBossBody27
    {
        public readonly Glacier28 system;

        public Glacier28Body(Glacier28 system)
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
        public double Filter(double amount, string weapon, Vec3 from, out bool blocked) => system.Filter(amount, weapon, from, out blocked);
        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt) { }
        public override void Stagger(double time, double dt) { }
        public override void Stun(double time, double dt) { }
    }

    /// <summary>One attack object of the lap fight on the rink: a shard / column (a square standing at arc s, lateral), a roller (moves along s), a low bar and a chasm (across the lane), a melt strip.
    /// kind: shard, column, roller, bar, chasm, melt. The interval along the line is s +- hl; the lateral extent lat0..lat1; the height y0..y1 (above the floor's y).</summary>
    public sealed class Glacier28Hazard
    {
        public string kind;
        public double s, hl, lat0, lat1, y0, y1, x, z, tx, tz, rollDir, damage, push;
        public int tellLeft, lifeLeft, total, tick;
        public bool hit, safe;
        public bool Live => tellLeft <= 0 && lifeLeft > 0;
    }

    public struct Glacier28Log
    {
        public int step;
        public string name;
        public double seconds;
        public bool resolve;
    }

    public sealed class Glacier28 : IBossSystem
    {
        public readonly Glacier28Params p;
        public readonly Glacier28Body body;
        public Rink28Math rink;
        public readonly Vec3 origin;
        readonly EventBus events;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt0;
        readonly BossEvent said = new BossEvent();
        BossKit kit;
        BossScale scale = BossScale.One;
        public readonly Fsm27 fsm;

        public WyrmBox box;
        public Vec3 prevCenter;
        public readonly List<Glacier28Hazard> hazards = new List<Glacier28Hazard>();
        public readonly List<Glacier28Log> log = new List<Glacier28Log>();
        public readonly Dictionary<string, int> stats = new Dictionary<string, int>();
        static readonly string[] StatNames = { "attacks", "attackHits", "blocked", "pulseHits", "windows", "misses", "flips", "shards", "clangs", "cracks", "cuts", "cutHits", "meltTicks", "columns", "rollers", "bars", "chasms", "melts" };

        public string phase = "dead";
        public int stepNo, phaseSteps, hits;
        int beatSteps, dieSteps, lastResolve = -100000, atkLeft, introSteps, windowLeft, armLeft, skidLeft, flipLock, doorMemory, openLeft, cutLeft, meltLeft, p3Index;
        double bossS, vel, surge, surgeRate, crackPts;
        bool cutUsed, meltSeen;
        /// <summary>+1: the boss flees counter-clockwise (toward rising s), -1 clockwise.</summary>
        public int dir = 1;
        /// <summary>The metres along the flee direction from the player's projection to the boss (the HUD's tape), the signed shortest arc (counter-clockwise positive) and the player's projection.</summary>
        public double gap, gapSigned, playerS, playerLat;
        /// <summary>The ice glaze is on (the plates are lit; a pulse blast clangs off).</summary>
        public bool glazed = true;
        public Func<Capsule> playerBody;
        /// <summary>The player's velocity (the sim hands it in): attacks are placed ahead of where you will be.</summary>
        public Func<Vec3> playerVelocity;
        /// <summary>The player's aim direction (the sim hands it in): the pulse27 blast launches along it.</summary>
        public Func<Vec3> playerAim;
        /// <summary>The PULSE's kill pips: a hit on the boss refills launch.refill of them (there are no kills on the ice), so three blasts win the fight.</summary>
        public IPulseKillSink27 pulseSink;
        /// <summary>Is a PULSE blast ready right now (the sim hands it in; null: always)? The window opens only when it is: the PULSE-READY flash is the window.</summary>
        public Func<bool> pulseReady;

        public Glacier28(GameConfig cfg, Glacier28Params p, EventBus events, BossTarget target, Func<int> nextId)
        {
            this.p = p;
            this.events = events;
            this.target = target;
            this.nextId = nextId;
            dt0 = cfg.Dt;
            origin = new Vec3(p.origin[0], p.origin[1], p.origin[2]);
            rink = new Rink28Math(p.L, p.rc, p.origin[0], p.origin[2]);
            body = new Glacier28Body(this);
            foreach (string n in StatNames) stats[n] = 0;
            fsm = BuildFsm();
            events.On<Fist24FlickEvent>("fistFlick", e => LaunchLanded(target.position(), playerVelocity != null ? playerVelocity() : default));
            events.On("pulse27Blast", _ => LaunchLanded(target.position(), playerAim != null ? playerAim() : (playerVelocity != null ? playerVelocity() : default)));
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;
        public double BossS => bossS;
        public bool WindowOpen => windowLeft > 0;
        public double WindowFrac => windowLeft > 0 ? (double)windowLeft / S(p.windowTime) : 0;
        /// <summary>The flee direction has just flipped: the boss is braking and turning (the skid; the view shows it).</summary>
        public bool Skidding => skidLeft > 0;
        public int Phase => hits >= 2 ? 3 : hits == 1 ? 2 : 1;
        /// <summary>The cut's hit window is open: a pulse hit lands even through the glaze (once per phase).</summary>
        public bool CutOpen => cutLeft > 0 && !cutUsed;
        /// <summary>How much of the glaze's crack is done, 0..1 (the plates' glow), and how much of the open time is left, 0..1.</summary>
        public double CrackFrac => glazed ? Math.Min(1.0, crackPts / CrackNeed) : 1;
        public double OpenFrac => !glazed && openLeft > 0 ? (double)openLeft / S(p.glazeOpen) : 0;
        double CrackNeed => p.glazeHits[Math.Min(p.glazeHits.Length - 1, Phase - 1)];
        public Vec3 BossWorld
        {
            get
            {
                rink.PointAt(bossS, bossLat29, out double x, out double z, out _, out _);
                return new Vec3(x, origin.y, z);
            }
        }
        /// <summary>Is the boss over a chasm now (the view leaps it)?</summary>
        public bool BossOverChasm
        {
            get
            {
                foreach (Glacier28Hazard h in hazards) if (h.kind == "chasm" && h.Live && Math.Abs(rink.SignedGap(h.s, bossS)) < h.hl) return true;
                return false;
            }
        }
        public uint RngState { get => fsm.rng.State; set => fsm.rng.State = value; }
        public void NewRun(uint seed) => fsm.rng = new Mulberry32(seed);

        /// <summary>The boss's speed at this gap: base + clamp(k x (gap - target), +-clampFrac x slideFloor); a player who has RUN PAST it (gap under 0) meets its fastest speed (pure: the harness asserts the clamp).</summary>
        public static double SpeedAt(Glacier28Params p, double gap, bool ready = true)
        {
            double lim = p.clampFrac * p.slideFloor;
            if (gap < 0) return p.baseSpeed + lim;
            double d = p.k * (gap - (ready ? p.targetGap : p.spentTarget));
            return p.baseSpeed + Math.Max(-lim, Math.Min(lim, d));
        }

        public double CurrentSpeed => SpeedAt(p, gap, Ready) * speedMult29;
        /// <summary>Can the PULSE blast go now (the sim's glove)? While it cannot, the boss keeps its distance (speed.spentTarget): it never lets you hang on its heels with nothing to hit it with.</summary>
        public bool Ready => rule29 != null || pulseReady == null || pulseReady(); // NIGHT29: with the reworked rule the glove is not the weapon: the boss stays in its tired band (it can always be run down; patience, not the cooldown, paces it)
        bool Playing => phase == "p1" || phase == "p2" || phase == "p3";
        public bool Hurtable => body.alive && windowLeft > 0 && Playing;

        /// <summary>The shorter-arc bookkeeping for a player at s and a boss at b: the arc from the player to the boss going counter-clockwise, the flee direction after the hysteresis rule, and the gap along it.</summary>
        public static void Arcs(Rink28Math r, double hysteresis, double playerS, double bossS, ref int dir, out double gapAlong)
        {
            double lap = r.Lap, plus = r.Wrap(bossS - playerS), minus = lap - plus;
            if (dir > 0 && minus + hysteresis < plus) dir = -1;
            else if (dir < 0 && plus + hysteresis < minus) dir = 1;
            gapAlong = dir > 0 ? plus : minus;
        }

        /// <summary>The floor's height above the rink's floor at a lateral offset (the lane is flat, the bank rises, the lip is flat).</summary>
        public double FloorAt(double lat)
        {
            if (lat <= p.laneHalf) return 0;
            return Math.Min(lat - p.laneHalf, p.bankRun) * p.tanBank;
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
            top.Add("beat", enter: s => { SetPhase("beat"); beatSteps = kit.Beat("glacier", nextPhase == "p3" ? "PHASE 3" : "PHASE 2", new Vec3(body.position.x, body.position.y + 2, body.position.z)); hazards.Clear(); atkLeft = S(p.shardEvery); },
                needsExitTime: true, canExit: s => s.steps >= beatSteps);
            top.Add("dying", enter: s => { SetPhase("dying"); dieSteps = S(p.dieTime); hazards.Clear(); windowLeft = 0; kit.Dying("glacier", body.position); });
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
            if (ph == "p1" || ph == "p2" || ph == "p3") PhaseStart(ph);
            said.Reset("glacier");
            said.phase = ph;
            said.hp = body.hp;
            said.maxHp = body.maxHp;
            events.Emit("bossPhase", said);
        }

        /// <summary>Each phase starts glazed, uncracked, with its own attack clock and the once-per-phase cut available.</summary>
        void PhaseStart(string ph)
        {
            glazed = true;
            crackPts = 0;
            openLeft = 0;
            cutUsed = false;
            cutLeft = 0;
            hazards.Clear();
            meltLeft = S(p.meltFirst);
            meltSeen = false;
            p3Index = 0;
            atkLeft = S(ph == "p3" ? p.p3Every : p.shardEvery);
        }

        void Tell(string name, double seconds, Vec3 at)
        {
            said.Reset("glacier");
            seconds = Math.Max(seconds, tellMin29);
            said.name = name;
            said.seconds = seconds;
            events.Emit("bossTell", said);
            log.Add(new Glacier28Log { step = stepNo, name = name, seconds = seconds });
            kit.telegraph.Threat(at.x, at.y, at.z);
        }

        void Resolve(string name)
        {
            lastResolve = stepNo;
            log.Add(new Glacier28Log { step = stepNo, name = name, resolve = true });
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
            hazards.Clear();
            bossS = rink.Wrap(p.spawnS + p.startGap);
            playerS = p.spawnS;
            playerLat = 0;
            dir = 1;
            vel = p.baseSpeed; // already skating at its base speed (a start from rest gave the first hit away)
            surge = 0;
            skidLeft = 0;
            flipLock = 0;
            doorMemory = 0;
            atkLeft = S(p.shardEvery);
            windowLeft = 0;
            armLeft = S(p.armTime);
            gap = p.startGap;
            gapSigned = gap;
            Pose();
            prevCenter = box.c;
        }

        public void StartFight(BossFight ctx)
        {
            kit = ctx.kit;
            scale = ctx.scale;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(p.pool * ctx.scale.hp);
            body.hp = body.maxHp;
            hits = 0;
            stepNo = 0;
            log.Clear();
            foreach (string n in StatNames) stats[n] = 0;
            lastResolve = -100000;
            nextPhase = null;
            glazed = true;
            crackPts = 0;
            openLeft = 0;
            cutLeft = 0;
            cutUsed = false;
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
            hazards.Clear();
            windowLeft = 0;
            fsm.Reset();
        }

        void Pose()
        {
            prevCenter = box.c;
            rink.PointAt(bossS, bossLat29, out double x, out double z, out double tx, out double tz);
            var b = default(WyrmBox);
            b.Orient(new Vec3(tx * dir, 0, tz * dir));
            b.c = new Vec3(x, origin.y + p.boxH * 0.5 + 0.2, z);
            b.hr = p.boxW / 2; b.hu = p.boxH / 2; b.hf = p.boxL / 2;
            box = b;
        }

        public double Raycast(Vec3 o, Vec3 d)
        {
            if (!body.alive || phase == "dying" || phase == "dead") return -1;
            return box.Ray(o, d);
        }

        /// <summary>A PULSE blast from `from` along `aim`: it LANDS on the boss when the window is open, the boss is within launch.reach m and the throw is within launch.coneDeg of the line to it; a blast
        /// that reaches the boss while it is GLAZED (and no cut is open) CLANGS OFF instead.</summary>
        public bool LaunchLanded(Vec3 from, Vec3 aim)
        {
            if (!body.alive || !Playing) return false;
            double dl = aim.Length;
            if (dl < 1e-6) return false;
            Vec3 to = box.c - new Vec3(from.x, from.y + 1.0, from.z);
            double d = to.Length;
            if (d > p.reach || d < 1e-6) return false;
            double cos = Vec3.Dot(to / d, aim / dl);
            if (cos < JsMath.Cos(p.coneDeg * Math.PI / 180)) return false;
            if (rule29 != null) return rule29.Launch(this); // NIGHT29 builder B
            if (!Hurtable)
            {
                if (glazed && !CutOpen) { Clang(); return true; }
                return false;
            }
            Apply(body.maxHp / p.hitsToKill);
            return true;
        }

        /// <summary>The blast reached a glazed boss: nothing lands, the boss runs clangCost m further, the glove refills (the cooldown still runs), never health.</summary>
        void Clang()
        {
            windowLeft = 0;
            AddSurge(p.clangCost);
            armLeft = S(p.armTime);
            stats["clangs"]++;
            pulseSink?.PulseKillFilled((int)p.refill);
            said.Reset("glacier");
            said.name = "clang";
            events.Emit("bossPunish", said);
        }

        /// <summary>The hit rule: ONLY a gauntlet hit (the PULSE blast) inside the window lands, once per window, and not through the glaze (unless a cut is open); a pistol hit or a close shotgun hit CRACKS the
        /// glaze but never hurts the boss; everything else is blocked (the HUD says why).</summary>
        public double Filter(double amount, string weapon, Vec3 from, out bool blocked)
        {
            blocked = false;
            if (rule29 != null) return rule29.Filter(this, amount, weapon, from, out blocked); // NIGHT29 builder B (design.n29.boss2)
            if (weapon == "gauntlet")
            {
                if (!Hurtable) { blocked = true; stats["blocked"]++; return 0; }
                if (glazed && !CutOpen) { blocked = true; return 0; } // the clang is LaunchLanded's (the blast's event)
                return body.maxHp / p.hitsToKill; // one hit is a third of the pool, whatever the blast's damage
            }
            if (Playing && glazed)
            {
                double pts = 0;
                if (weapon == "revolver") pts = amount >= p.glazeCharged * p.revolverDamage ? p.glazeChargedHits : 1;
                else if (weapon == "shotgun" && Vec3.DistanceTo(from, box.c) <= p.shotgunRange) pts = p.glazeChargedHits;
                if (pts > 0) Crack(pts);
            }
            blocked = true;
            stats["blocked"]++;
            return 0;
        }

        void Crack(double pts)
        {
            crackPts += pts;
            if (crackPts < CrackNeed) return;
            glazed = false;
            openLeft = S(p.glazeOpen);
            crackPts = 0;
            stats["cracks"]++;
            said.Reset("glacier");
            said.part = "glaze";
            said.reason = "crack";
            events.Emit("bossPart", said);
        }

        // --- NIGHT29 builder B: the hooks the reworked rule (Glacier29) uses; all inert with rule29 null (NIGHT28's fight bit for bit) -----------------------------------------------------------

        /// <summary>The reworked rule (design.n29.boss2); null = NIGHT28's pulse-only rule.</summary>
        public IGlacierRule29 rule29;
        /// <summary>The boss's lateral offset on the rink (outward positive; 0 = the centre line: NIGHT28).</summary>
        public double bossLat29;
        /// <summary>The boss speed scale (1 = NIGHT28; design.n29.boss2.boss.speedMult with the reworked rule).</summary>
        public double speedMult29 = 1;
        /// <summary>NIGHT29: the shortest tell any attack gets with the rule on (design.n29.boss2.attackTellMinSeconds); 0 = NIGHT28.</summary>
        public double tellMin29;
        public bool IsPlaying => Playing;
        public bool AnyTelling29 => AnyTelling();
        public bool SpawnShards29() => SpawnShards();
        public bool SpawnRoller29() => SpawnRoller();

        /// <summary>A hit of `dmg` from a gun or the glove, never taking the bar below floorFrac of its pool (0 = no floor). Phases are crossed one third at a time (the bar stops at each third until the beat has run);
        /// `hits` counts the thirds taken. glove: a gauntlet hit refills the pips (NIGHT28's rule).</summary>
        public void Hit29(double dmg, double floorFrac, bool glove)
        {
            if (!body.alive || !Playing || dmg <= 0) return;
            double floor = body.maxHp * floorFrac;
            double third = body.maxHp / p.hitsToKill;
            double lim = phase == "p1" ? body.maxHp - third : phase == "p2" ? body.maxHp - 2 * third : 0;
            double to = Math.Max(Math.Max(floor, lim), body.hp - dmg);
            if (to >= body.hp) return;
            body.hp = to;
            body.flash = 0.08;
            hits = body.hp <= 0 ? p.hitsToKill : Math.Min(p.hitsToKill - 1, (int)Math.Floor((1.0 - body.hp / body.maxHp) * p.hitsToKill + 1e-9));
            if (glove && hits < p.hitsToKill) pulseSink?.PulseKillFilled((int)p.refill);
            said.Reset("glacier");
            said.damage = JsMath.Round(dmg);
            said.part = glove ? "pulse" : "gun";
            said.hp = body.hp;
            events.Emit("bossHit", said);
            if (hits < p.hitsToKill) nextPhase = hits == 1 ? "p2" : hits == 2 ? "p3" : null;
            if (nextPhase != null && ((nextPhase == "p2" && phase != "p1") || (nextPhase == "p3" && phase != "p2"))) nextPhase = null;
        }

        public void Apply(double dmg)
        {
            if (rule29 != null) { rule29.Apply(this, dmg); return; } // NIGHT29 builder B
            if (!body.alive || windowLeft <= 0) return;
            if (glazed)
            {
                if (!CutOpen) { Clang(); return; }
                cutUsed = true; // the cut's hit, once per phase
                stats["cutHits"]++;
            }
            hits++;
            stats["pulseHits"]++;
            windowLeft = 0;
            AddSurge(p.hitBack);
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

        /// <summary>Metres of gap the boss runs on by, spread over surgeSeconds (a hit's, a miss's, a clang's).</summary>
        void AddSurge(double metres)
        {
            surge += metres;
            surgeRate = surge / p.surgeSeconds;
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

        // --- the attacks ---------------------------------------------------------------------------------------------------------------------

        double PlayerSpeed()
        {
            Vec3 v = playerVelocity != null ? playerVelocity() : default;
            return JsMath.Hypot(v.x, v.z);
        }

        /// <summary>Metres ahead (along the flee direction) that stay ahead of you until an attack with this tell stands.</summary>
        double MinAhead(double tell) => p.shardLead + PlayerSpeed() * tell;

        Glacier28Hazard Make(string kind, double s, double hl, double lat0, double lat1, double y0, double y1, double tell, double life, double damage, double push)
        {
            var h = new Glacier28Hazard { kind = kind, s = rink.Wrap(s), hl = hl, lat0 = lat0, lat1 = lat1, y0 = y0, y1 = y1, tellLeft = S(Math.Max(tell, tellMin29)), lifeLeft = S(life), total = S(Math.Max(tell, tellMin29)), damage = damage, push = push };
            double lc = (lat0 + lat1) * 0.5;
            rink.PointAt(h.s, lc, out h.x, out h.z, out h.tx, out h.tz);
            hazards.Add(h);
            return h;
        }

        /// <summary>The P1 trail: shards on the racing line from just behind the boss back toward you, one lateral per trail; one that you would reach before its decal is over is skipped.</summary>
        bool SpawnShards()
        {
            if (gap < p.shardMinGap) return false;
            double reachIn = MinAhead(p.shardTell);
            int placed = 0;
            double lat = p.shardLateral[fsm.RollInt(p.shardLateral.Length)]; // one lateral per trail: a free line in the lane always exists
            double hs = p.shardSize / 2;
            for (int i = 0; i < p.shardCount; i++)
            {
                double d = gap - 2 - i * p.shardSpacing; // metres ahead of the player along the flee direction (i = 0 nearest the boss)
                if (d < reachIn) break;
                Glacier28Hazard sh = Make("shard", playerS + dir * d, hs, lat - hs, lat + hs, 0, p.shardSize, p.shardTell, p.shardLife, p.shardDamage, p.shardPush);
                kit.telegraph.Line(sh.x - hs, origin.y, sh.z, 1, 0, p.shardSize, p.shardSize, p.shardTell);
                placed++;
                stats["shards"]++;
                if (placed == 1) Tell("shard", p.shardTell, new Vec3(sh.x, origin.y, sh.z));
            }
            return placed > 0;
        }

        bool SpawnColumns()
        {
            double reachIn = MinAhead(p.colTell) + 3;
            if (gap - 2 < reachIn) return false;
            int side = fsm.Roll() < 0.5 ? -1 : 1, placed = 0;
            double hs = p.colSize / 2;
            for (int i = 0; i < p.colCount; i++)
            {
                double d = reachIn + i * p.colSpacing;
                if (d > gap - 2) break;
                double lat = side * p.colOffset * (i % 2 == 0 ? 1 : -1);
                Glacier28Hazard h = Make("column", playerS + dir * d, hs, lat - hs, lat + hs, 0, p.colHeight, p.colTell, p.colLife, p.colDamage, p.colPush);
                kit.telegraph.Line(h.x - hs, origin.y, h.z, 1, 0, p.colSize, p.colSize, p.colTell);
                placed++;
                stats["columns"]++;
                if (placed == 1) Tell("column", p.colTell, new Vec3(h.x, origin.y, h.z));
            }
            return placed > 0;
        }

        bool SpawnRoller()
        {
            double d = Math.Min(p.rollAhead, gap - 2);
            if (d < MinAhead(Math.Max(p.rollTell, tellMin29)) + 2) return false;
            Glacier28Hazard h = Make("roller", playerS + dir * d, p.rollRadius, -p.laneHalf, p.laneHalf, 0, p.rollRadius * 2, p.rollTell, p.rollLife, p.rollDamage, p.colPush);
            h.rollDir = -dir; // it rolls toward you
            double w = p.laneHalf * 2;
            kit.telegraph.Line(h.x - w / 2, origin.y, h.z, 1, 0, w, p.rollRadius * 2, Math.Max(p.rollTell, tellMin29));
            stats["rollers"]++;
            Tell("roller", Math.Max(p.rollTell, tellMin29), new Vec3(h.x, origin.y, h.z));
            return true;
        }

        bool SpawnBar()
        {
            double d = Math.Min(p.barAhead, gap - 2);
            if (d < MinAhead(p.barTell) + 2) return false;
            Glacier28Hazard h = Make("bar", playerS + dir * d, p.barThick / 2, -p.laneHalf, p.rimLateral, p.barBottom, p.barTop, p.barTell, p.barLife, p.barDamage, p.colPush);
            kit.telegraph.Line(h.x - p.laneHalf, origin.y, h.z, 1, 0, p.laneHalf * 2, p.barThick, p.barTell);
            stats["bars"]++;
            Tell("bar", p.barTell, new Vec3(h.x, origin.y, h.z));
            return true;
        }

        bool SpawnChasm()
        {
            double d = Math.Min(p.chasmAhead, gap - 2 - p.chasmLength / 2);
            if (d < MinAhead(p.chasmTell) + p.chasmLength / 2) return false;
            Glacier28Hazard h = Make("chasm", playerS + dir * d, p.chasmLength / 2, -p.laneHalf, p.rimLateral, 0, 0.35, p.chasmTell, p.chasmLife, p.chasmDamage, p.chasmPush);
            kit.telegraph.Line(h.x - p.laneHalf, origin.y, h.z, 1, 0, p.laneHalf * 2, p.chasmLength, p.chasmTell);
            stats["chasms"]++;
            Tell("chasm", p.chasmTell, new Vec3(h.x, origin.y, h.z));
            return true;
        }

        /// <summary>The middle `length` m of the straight you reach next (far enough ahead to see the decal): meltwater across the lane and the bank.</summary>
        bool SpawnMelt()
        {
            double mid0 = p.L / 2, mid1 = p.L + Math.PI * p.rc + p.L / 2, hl = p.meltLength / 2;
            double minD = MinAhead(p.meltTell) + hl + 4, bestD = 1e9, bestS = 0;
            foreach (double m in new[] { mid0, mid1 })
            {
                double d = dir > 0 ? rink.Wrap(m - playerS) : rink.Wrap(playerS - m);
                if (d < minD) d += rink.Lap;
                if (d < bestD) { bestD = d; bestS = m; }
            }
            Glacier28Hazard h = Make("melt", bestS, hl, -p.laneHalf, p.rimLateral, 0, 0, p.meltTell, p.meltLive, p.meltDps, 0);
            h.safe = p.meltSafeFirst && !meltSeen;
            meltSeen = true;
            double w = p.laneHalf + p.rimLateral;
            rink.PointAt(bestS, (p.rimLateral - p.laneHalf) / 2, out double mx, out double mz, out double tx, out double tz);
            kit.telegraph.Line(mx - tx * hl, origin.y, mz - tz * hl, tx, tz, p.meltLength, w, p.meltTell);
            stats["melts"]++;
            Tell("melt", p.meltTell, new Vec3(mx, origin.y, mz));
            return true;
        }

        bool AnyTelling()
        {
            foreach (Glacier28Hazard s in hazards) if (s.tellLeft > 0) return true;
            return false;
        }

        bool AnyKind(string kind)
        {
            foreach (Glacier28Hazard s in hazards) if (s.kind == kind) return true;
            return false;
        }

        /// <summary>The phase's attack clock: P1 shards, P2 the melt strip (its own 20 s clock), P3 the rotation columns, roller, bar, chasm.</summary>
        void StepAttacks()
        {
            if (phase == "p2")
            {
                if (meltLeft > 0) meltLeft--;
                if (meltLeft <= 0 && !AnyKind("melt") && !AnyTelling())
                {
                    if (SpawnMelt()) meltLeft = S(p.meltSpacing);
                    else meltLeft = S(0.5);
                }
                return;
            }
            if (--atkLeft <= 0 && !AnyTelling())
            {
                bool ok;
                if (phase == "p3")
                {
                    int k = p3Index % 4;
                    ok = k == 0 ? SpawnColumns() : k == 1 ? SpawnRoller() : k == 2 ? SpawnBar() : SpawnChasm();
                    if (ok) p3Index++;
                    atkLeft = ok ? S(p.p3Every) : S(0.5);
                }
                else atkLeft = SpawnShards() ? S(p.shardEvery) : S(0.5);
            }
            else if (atkLeft < 0) atkLeft = 0;
        }

        bool InDoor(double x, double z) => Math.Abs(x - origin.x) < p.doorHalf && Math.Abs(z - origin.z) < p.islandRadius;

        void StepHazards(double dt)
        {
            Capsule c = PlayerCapsule();
            double feet = Math.Min(c.start.y, c.end.y) - c.radius - origin.y, top = Math.Max(c.start.y, c.end.y) + c.radius - origin.y;
            double r = c.radius;
            for (int i = hazards.Count - 1; i >= 0; i--)
            {
                Glacier28Hazard o = hazards[i];
                if (o.tellLeft > 0)
                {
                    if (--o.tellLeft == 0) { Resolve(o.kind); stats["attacks"]++; }
                    continue;
                }
                if (--o.lifeLeft <= 0) { hazards.RemoveAt(i); continue; }
                if (o.kind == "roller")
                {
                    o.s = rink.Wrap(o.s + o.rollDir * p.rollSpeed * dt);
                    rink.PointAt(o.s, 0, out o.x, out o.z, out o.tx, out o.tz);
                }
                if (phase == "intro") continue;
                bool inS = Math.Abs(rink.SignedGap(o.s, playerS)) < o.hl + r;
                bool inLat = playerLat + r > o.lat0 && playerLat - r < o.lat1;
                if (!inS || !inLat) continue;
                if (o.kind == "melt")
                {
                    if (o.safe || feet >= p.meltStand + FloorAt(playerLat)) continue; // a wall rider (feet above the floor) is safe; the first strip is harmless
                    if (++o.tick < S(p.meltTick)) continue;
                    o.tick = 0;
                    Health h = target.health;
                    if (h != null && !h.Dead && h.Damage(o.damage * p.meltTick, "hazard")) stats["meltTicks"]++;
                    continue;
                }
                if (o.hit) continue;
                if (o.kind == "chasm")
                {
                    if (feet >= FloorAt(playerLat) + o.y1) continue; // airborne over it: crossed
                }
                else if (!(top > o.y0 && feet < o.y1)) continue;
                o.hit = true;
                if (HurtPlayer(o.damage))
                {
                    stats["attackHits"]++;
                    // pushed out the side you came from along the line
                    double side = rink.SignedGap(o.s, playerS) >= 0 ? 1 : -1;
                    rink.PointAt(playerS, 0, out _, out _, out double tx, out double tz);
                    target.impulse?.Invoke(tx * side * o.push, 2, tz * side * o.push);
                }
            }
        }

        // --- the step ----------------------------------------------------------------------------------------------------------------------

        void UpdateGap(bool mayFlip)
        {
            int d0 = dir;
            int d = dir;
            if (flipLock > 0) { flipLock--; mayFlip = false; }
            Arcs(rink, mayFlip ? p.hysteresis : 1e9, playerS, bossS, ref d, out double along);
            dir = d;
            // a player who has passed the boss while a reversal is on hold reads as a NEGATIVE gap (the boss is behind you)
            gap = along > rink.Lap * 0.5 + p.hysteresis * 0.5 ? along - rink.Lap : along;
            gapSigned = rink.SignedGap(playerS, bossS);
            if (dir != d0)
            {
                skidLeft = S(p.skidSeconds);
                flipLock = S(p.flipHold);
                stats["flips"]++;
                bool cut = doorMemory > 0 && Playing;
                said.Reset("glacier");
                said.name = cut ? "cut" : "reverse";
                said.seconds = p.skidSeconds;
                events.Emit("bossTell", said);
                if (cut)
                {
                    stats["cuts"]++;
                    cutLeft = S(p.cutHold);
                    armLeft = 0; // a window may open at once
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
            rink.Project(pw.x, pw.z, out playerS, out playerLat);
            if (InDoor(pw.x, pw.z)) doorMemory = S(p.cutMemory);
            else if (doorMemory > 0) doorMemory--;
            UpdateGap(true);
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
            bool play = Playing;
            if (play || phase == "intro") // the boss skates from the first moment (an intro that stood still gave the first hit away)
            {
                if (skidLeft > 0) skidLeft--;
                double want = dir * SpeedAt(p, gap, Ready) * speedMult29, brake = 2 * p.baseSpeed / p.skidSeconds * dt;
                vel += Math.Max(-brake, Math.Min(brake, want - vel));
                double ds = vel * dt;
                if (surge > 0)
                {
                    double add = Math.Min(surge, surgeRate * dt);
                    surge -= add;
                    ds += dir * add;
                }
                bossS = rink.Wrap(bossS + ds);
                UpdateGap(true);
            }
            if (play)
            {
                if (cutLeft > 0) cutLeft--;
                if (!glazed && --openLeft <= 0) { glazed = true; crackPts = 0; said.Reset("glacier"); said.part = "glaze"; said.reason = "reglaze"; events.Emit("bossPart", said); }
                // the window: the gap under windowAt opens it (a PULSE-ready flash) when the glaze is cracked (or a cut is open); a window that runs out is a miss: the boss runs on, never health
                if (windowLeft > 0)
                {
                    if (--windowLeft == 0)
                    {
                        AddSurge(p.missCost);
                        stats["misses"]++;
                        armLeft = S(p.armTime);
                        said.Reset("glacier");
                        said.name = "miss";
                        events.Emit("bossPunish", said);
                    }
                }
                else if (armLeft > 0) armLeft--;
                else if (rule29 != null ? rule29.WantWindow(this) : gap <= p.windowAt && gap >= -1.5 && (!glazed || CutOpen) && Ready)
                {
                    windowLeft = S(rule29 != null ? rule29.WindowSeconds : p.windowTime);
                    if (rule29 != null) { glazed = false; openLeft = windowLeft; } // NIGHT29: the exposed window shows the plates cracked
                    stats["windows"]++;
                    said.Reset("glacier");
                    said.name = "window";
                    said.seconds = p.windowTime;
                    events.Emit("bossWindow", said);
                }
                if (rule29 == null) StepAttacks(); // NIGHT29: Glacier29 schedules its own attacks
            }
            body.position = BossWorld;
            rink.PointAt(bossS, 0, out _, out _, out double tvx, out double tvz);
            body.velocity = new Vec3(tvx * vel, 0, tvz * vel);
            Pose();
            if (play) StepHazards(dt);
        }
    }
}
