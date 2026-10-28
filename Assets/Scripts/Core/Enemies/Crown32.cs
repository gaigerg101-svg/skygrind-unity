// NIGHT32 builder C (design.n32.boss5; DESIGN32 C, built to card B's damage rules; D-6: a proposal behind the row, GG owns enemy design). THE CROWN, LEVEL 5's boss:
// a blocky core (a cube, the web's style) under a crown of cubes, hovering over a TRUE CIRCLE (Crown32Arena: 64 facets, a drop ring, three rim ledges). Kill band 50-75 s.
//   DAMAGE   every hit on the core lands x1.0 (never a gate: card B's 25 % floor is met by 100 %); three cyan WEAK PLATES on three faces land x plates.mult (2.5); the core spins
//            (core.spinRate) so the plates face round the ring and the player circles. A parried orb (weapon id parry31) lands in full. Unhittable only in the intro and for
//            beatSeconds (1.2 s, under card B's 1.5 s) at each beat change. The sim hurtbox is core.hurtScale (1.3) x the drawn core and plates.
//   BEAT 1   (100-55 %) the core hovers high; a waist-high BEAM sweeps once round the ring every sweep.everySeconds (a sweep.tellSeconds tell; slide under or jump over); led ORBS
//            the gauntlet parries back.
//   BEAT 2   (55-20 %) the core drops low; the outer ring's sectors drop into lava in a rotating pattern (drop.*: a tell, down, up, turn one sector on); grapple RINGS over the rim
//            (the sim adds them to the level's grapple nodes); faster orbs.
//   BEAT 3   (20-0 %) the core sinks to the floor and CHARGES across the ring (a tell line, a straight run): the round wall always ends it in a SLAM that stuns it charge.stunSeconds
//            with its plates open (they are shut through the rest of beat 3), then it drifts back to the middle.
//   ANTI-CAMP every beat: a player whose flat speed stays under mortar.stillSpeed for mortar.stillSeconds (never while wall riding) has the spot MARKED; the MORTAR lands
//            mortar.delaySeconds later. Every attack has a tell of 0.6 s or more.
//   RETRY    a death in the room puts the bar back to the start of the current beat (retryBeatStart).
// Fixed 120 Hz, whole steps (stepsFor), Mulberry32 only (seeded per fight from the run seed: the sweep's and the drop ring's turn), JsMath, no audio, no UnityEngine.
// Events (new names, PORT-STATUS): crownBeat, crownSweep, crownOrb, crownMortar, crownDrop, crownCharge, crownSlam, crownPlate (payload Crown32Event.stage), plus the BossKit's
// bossRise / bossPhase / bossHit / bossTell.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.n32.boss5 (every number Unity-only; each has its _why).</summary>
    public sealed class Crown32Params
    {
        public string name = "THE CROWN";
        public double pool, introHold, dieTime, beatSeconds, beat2At, beat3At;
        public bool retryBeatStart;
        public double coreHalf, hurtScale, hoverSpeed, spinRate, orbitRadius, orbitRate, moveSpeed;
        public double[] hoverY;
        public int plateCount;
        public double plateHalfW, plateHalfH, plateDepth, plateMult;
        public double shotgunPlateMult, chargeMin, chargePlateMult, chargeCoreMult, detonateMin, detonateMult, slugMin, orbParryDamage;
        public double sweepFirst, sweepEvery, sweepTell, sweepRate, beamLow, beamHigh, beamHalfWidth, sweepDamageFrac, sweepKnock, sweepKnockUp;
        public double[] orbEvery;
        public double orbTell, orbSpeed, orbDamage, orbRadius, orbLife;
        public double stillSeconds, stillSpeed, mortarDelay, mortarRadius, mortarDamageFrac, mortarKnockUp;
        public double dropTell, dropDown, dropGap;
        public int[] dropPattern;
        public int ringCount;
        public double ringInset, ringHeight;
        public double chargeFirst, chargeTell, chargeSpeed, chargeHalfWidth, chargeDamageFrac, chargeKnock, chargeKnockUp, stunSeconds, recoverSeconds;
        public double lavaFraction, lavaKnockUp, lavaGrace;

        public static Crown32Params From(GameConfig c)
        {
            const string g = "design.n32.boss5.";
            double[] pat = c.Nums(g + "drop.pattern");
            var pi = new int[pat.Length];
            for (int i = 0; i < pat.Length; i++) pi[i] = (int)pat[i];
            return new Crown32Params
            {
                name = c.Str(g + "name"), pool = c.Num(g + "pool"), introHold = c.Num(g + "introHold"), dieTime = c.Num(g + "dieTime"), beatSeconds = c.Num(g + "beatSeconds"),
                beat2At = c.Num(g + "beat2At"), beat3At = c.Num(g + "beat3At"), retryBeatStart = c.Bool(g + "retryBeatStart"),
                coreHalf = c.Num(g + "core.coreHalf"), hurtScale = c.Num(g + "core.hurtScale"), hoverY = c.Nums(g + "core.hoverY"), hoverSpeed = c.Num(g + "core.hoverSpeed"), spinRate = c.Num(g + "core.spinRate"),
                orbitRadius = c.Num(g + "core.orbitRadius"), orbitRate = c.Num(g + "core.orbitRate"), moveSpeed = c.Num(g + "core.moveSpeed"),
                plateCount = c.Int(g + "plates.count"), plateHalfW = c.Num(g + "plates.halfW"), plateHalfH = c.Num(g + "plates.halfH"), plateDepth = c.Num(g + "plates.depth"), plateMult = c.Num(g + "plates.mult"),
                shotgunPlateMult = c.Num(g + "kit.shotgunPlateMult"), chargeMin = c.Num(g + "kit.chargeMin"), chargePlateMult = c.Num(g + "kit.chargePlateMult"), chargeCoreMult = c.Num(g + "kit.chargeCoreMult"),
                detonateMin = c.Num(g + "kit.detonateMin"), detonateMult = c.Num(g + "kit.detonateMult"), slugMin = c.Num(g + "kit.slugMin"), orbParryDamage = c.Num(g + "kit.parryDamage"),
                sweepFirst = c.Num(g + "sweep.firstSeconds"), sweepEvery = c.Num(g + "sweep.everySeconds"), sweepTell = c.Num(g + "sweep.tellSeconds"), sweepRate = c.Num(g + "sweep.rate"),
                beamLow = c.Num(g + "sweep.beamLow"), beamHigh = c.Num(g + "sweep.beamHigh"), beamHalfWidth = c.Num(g + "sweep.halfWidth"), sweepDamageFrac = c.Num(g + "sweep.damageFrac"),
                sweepKnock = c.Num(g + "sweep.knock"), sweepKnockUp = c.Num(g + "sweep.knockUp"),
                orbEvery = c.Nums(g + "orbs.everySeconds"), orbTell = c.Num(g + "orbs.tellSeconds"), orbSpeed = c.Num(g + "orbs.speed"), orbDamage = c.Num(g + "orbs.damage"),
                orbRadius = c.Num(g + "orbs.radius"), orbLife = c.Num(g + "orbs.lifeSeconds"),
                stillSeconds = c.Num(g + "mortar.stillSeconds"), stillSpeed = c.Num(g + "mortar.stillSpeed"), mortarDelay = c.Num(g + "mortar.delaySeconds"), mortarRadius = c.Num(g + "mortar.radius"),
                mortarDamageFrac = c.Num(g + "mortar.damageFrac"), mortarKnockUp = c.Num(g + "mortar.knockUp"),
                dropTell = c.Num(g + "drop.tellSeconds"), dropDown = c.Num(g + "drop.downSeconds"), dropGap = c.Num(g + "drop.gapSeconds"), dropPattern = pi,
                ringCount = c.Int(g + "rings.count"), ringInset = c.Num(g + "rings.inset"), ringHeight = c.Num(g + "rings.height"),
                chargeFirst = c.Num(g + "charge.firstSeconds"), chargeTell = c.Num(g + "charge.tellSeconds"), chargeSpeed = c.Num(g + "charge.speed"), chargeHalfWidth = c.Num(g + "charge.halfWidth"),
                chargeDamageFrac = c.Num(g + "charge.damageFrac"), chargeKnock = c.Num(g + "charge.knock"), chargeKnockUp = c.Num(g + "charge.knockUp"),
                stunSeconds = c.Num(g + "charge.stunSeconds"), recoverSeconds = c.Num(g + "charge.recoverSeconds"),
                lavaFraction = c.Num("design.n30.lava.damageFraction"), lavaKnockUp = c.Num("design.n30.lava.knockUp"), lavaGrace = c.Num("design.n30.lava.graceSeconds"),
            };
        }
    }

    /// <summary>The payload of every crown* event: stage (tell / start / hit / end / mark / land / down / rise / burn / run / slam / fire), the beat, where, a direction, seconds,
    /// the sectors down (crownDrop), the plate (crownPlate).</summary>
    public sealed class Crown32Event
    {
        public string stage;
        public int beat, mask, plate = -1;
        public Vec3 at, dir;
        public double seconds;
    }

    public sealed class Crown32Orb
    {
        public Vec3 pos, vel;
        public int life;
        public bool alive;
    }

    public sealed class Crown32Body : Enemy, IBossBody27
    {
        public readonly Crown32 system;
        public Crown32Body(Crown32 system) { this.system = system; kind = "crown"; }
        public override double Height => system.HurtHalf * 2;
        public override double Radius => system.HurtHalf;
        public override double AimHeight => system.HurtHalf;
        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            double t = system.Raycast(o, d);
            headshot = t >= 0 && system.hitPlate >= 0;
            return t;
        }
        public override bool Damage(double amount, double flashTime) { system.Apply(amount); return false; }
        public double Filter(double amount, string weapon, Vec3 from, out bool blocked) => system.Filter(amount, weapon, from, out blocked);
        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt) { }
        public override void Stagger(double time, double dt) { }
        public override void Stun(double time, double dt) { }
    }

    public sealed class Crown32 : IBossSystem
    {
        public readonly Crown32Params p;
        public readonly Crown32Body body;
        public readonly Crown32Arena arena;
        readonly EventBus events;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt0;
        readonly BossEvent said = new BossEvent();
        readonly Crown32Event ev = new Crown32Event();
        BossKit kit;
        BossScale scale = BossScale.One;
        Mulberry32 rng = new Mulberry32(1);

        /// <summary>The room's origin (world): the arena's centre on its floor.</summary>
        public Vec3 origin;
        /// <summary>The sim hands these in: the player's velocity, whether he wall rides (never "still"), his collider height now (a slide is lower).</summary>
        public Func<Vec3> playerVelocity;
        public Func<bool> playerWallRiding;
        public Func<double> playerHeight;

        public string phase = "dead";
        /// <summary>1, 2, 3 in the fight.</summary>
        public int beat;
        /// <summary>Beat 3's act: hover / tell / charge / stun / recover.</summary>
        public string act = "hover";
        public int stepNo, phaseSteps, actSteps, beatHold;
        int introSteps, dieSteps, actLen;
        // beat 1: the sweep
        public int sweepState; // 0 idle, 1 tell, 2 sweeping
        public double sweepAngle, sweepStartAngle, sweepDir = 1;
        int sweepTimer, sweepSteps;
        bool sweepHit;
        // the orbs
        public readonly List<Crown32Orb> orbs = new List<Crown32Orb>();
        int orbTimer;
        public int orbTellLeft;
        // the mortar
        public double stillTime;
        public bool mortarPending;
        public Vec3 mortarAt;
        public int mortarLeft;
        // beat 2: the drop ring
        public int dropState; // 0 gap, 1 tell, 2 down
        public int dropStart, dropTellMask, downMask;
        int dropLeft, dropTurn = 1, lavaGrace;
        // beat 3: the charge
        public Vec3 chargeDir, lineEnd;
        int chargeTimer;
        bool chargeHit;
        // the core
        public Vec3 core; // world centre
        public double yawRad; // the plates' turn (radians of atan2(z, x) of plate 0's normal)
        public double orbitAngle; // beat 2's orbit (radians of atan2(z, x) round the arena's centre)
        public int hitPlate = -1;
        string lastPart;
        // the harness's counters (this fight)
        public int sweeps, sweepHits, orbsFired, orbHits, mortars, mortarHits, drops, lavaHits, charges, chargeHits, slams, plateHits, coreHits, parryHits, blockedHits, retries;
        public double fullDamage, plateDamage, totalDamage;
        public int unhittableSteps, hurtableSteps;

        public Crown32(GameConfig cfg, Crown32Params p, Crown32Arena arena, EventBus events, BossTarget target, Func<int> nextId)
        {
            this.p = p;
            this.arena = arena;
            this.events = events;
            this.target = target;
            this.nextId = nextId;
            dt0 = cfg.Dt;
            body = new Crown32Body(this);
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;
        public bool Hurtable => body.alive && phase == "p1" && beatHold == 0;
        public double HurtHalf => p.coreHalf * p.hurtScale;
        /// <summary>The plates take x mult now (beats 1-2 always; beat 3 only through the slam stun).</summary>
        public bool PlatesOpen => Hurtable && (beat < 3 || act == "stun");
        public bool Stunned => phase == "p1" && beat == 3 && act == "stun";
        public bool ChargeTelling => phase == "p1" && beat == 3 && act == "tell";
        public bool Charging => phase == "p1" && beat == 3 && act == "charge";
        public bool SweepTelling => sweepState == 1;
        public bool Sweeping => sweepState == 2;
        public double ActProgress => actLen > 0 ? Math.Min(1, (double)actSteps / actLen) : 0;
        /// <summary>The grapple rings are up (beat 2): the sim adds RingAt(k) to the level's grapple nodes.</summary>
        public bool RingsUp => phase == "p1" && beat == 2;
        public Vec3 Centre => core;

        /// <summary>The floor under the arena's centre (world y).</summary>
        public double FloorY => origin.y;

        /// <summary>Grapple ring k (world): over the rim, between two sectors.</summary>
        public Vec3 RingAt(int k)
        {
            double a = (k + 0.5) * 2 * Math.PI / Math.Max(1, p.ringCount) + arena.SectorAngle * 0.5;
            Vec3 l = Crown32Arena.Polar(a, arena.p.apothem - p.ringInset, p.ringHeight);
            return l + origin;
        }

        /// <summary>Plate k's outward normal (flat).</summary>
        public Vec3 PlateNormal(int k)
        {
            double a = yawRad + k * Math.PI / 2;
            return new Vec3(JsMath.Cos(a), 0, JsMath.Sin(a));
        }

        /// <summary>Plate k's drawn centre (world).</summary>
        public Vec3 PlateAt(int k) => core + PlateNormal(k) * (p.coreHalf + p.plateDepth);

        WyrmBox CoreBox(double scaleBy)
        {
            var b = default(WyrmBox);
            b.Orient(PlateNormal(0));
            b.c = core;
            b.hr = b.hu = b.hf = p.coreHalf * scaleBy;
            return b;
        }

        WyrmBox PlateBox(int k, double scaleBy)
        {
            var b = default(WyrmBox);
            Vec3 n = PlateNormal(k);
            b.Orient(n);
            b.c = core + n * (p.coreHalf + p.plateDepth * scaleBy);
            b.hr = p.plateHalfW * scaleBy;
            b.hu = p.plateHalfH * scaleBy;
            b.hf = p.plateDepth * scaleBy;
            return b;
        }

        void Pose()
        {
            body.position = new Vec3(core.x, core.y - HurtHalf, core.z); // the punch's capsule stands on the hurtbox's bottom
        }

        public void StartFight(BossFight ctx)
        {
            kit = ctx.kit;
            scale = ctx.scale;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(p.pool * ctx.scale.hp);
            body.hp = body.maxHp;
            stepNo = 0;
            sweeps = sweepHits = orbsFired = orbHits = mortars = mortarHits = drops = lavaHits = charges = chargeHits = slams = plateHits = coreHits = parryHits = blockedHits = retries = 0;
            fullDamage = plateDamage = totalDamage = 0;
            byWeapon.Clear();
            unhittableSteps = hurtableSteps = 0;
            beat = 1;
            beatHold = 0;
            ResetActs();
            core = new Vec3(origin.x, origin.y + p.hoverY[0], origin.z);
            yawRad = 0;
            introSteps = Math.Max(kit != null ? kit.IntroSteps() : 0, S(p.introHold));
            Pose();
            body.previous = body.position;
            SetPhase("intro");
            said.Reset("crown");
            said.name = "CROWN";
            said.hp = body.maxHp;
            events.Emit("bossRise", said);
        }

        /// <summary>The fight's per-run stream (the sim seeds it from the run seed and the room's salt before StartFight).</summary>
        public void NewRun(uint seed)
        {
            rng = new Mulberry32(seed);
        }

        void ResetActs()
        {
            act = "hover";
            actSteps = actLen = 0;
            sweepState = 0;
            sweepTimer = S(p.sweepFirst);
            orbs.Clear();
            orbTimer = S(p.orbEvery[0]);
            orbTellLeft = 0;
            stillTime = 0;
            mortarPending = false;
            dropState = 0;
            dropLeft = S(p.dropGap);
            dropTellMask = 0;
            downMask = 0;
            lavaGrace = 0;
            chargeTimer = S(p.chargeFirst);
        }

        /// <summary>A retry (the player died in the room): the fight goes on in its beat, the bar back to the beat's start (retryBeatStart), every attack cleared.</summary>
        public void Retry()
        {
            retries++;
            if (p.retryBeatStart && phase == "p1")
            {
                double start = beat == 1 ? 1.0 : beat == 2 ? p.beat2At : p.beat3At;
                body.hp = Math.Max(body.hp, Math.Min(body.maxHp, JsMath.Round(body.maxHp * start)));
            }
            ResetActs();
            core = new Vec3(origin.x, origin.y + p.hoverY[Math.Max(0, Math.Min(2, beat - 1))], origin.z);
            Pose();
        }

        public void Clear()
        {
            body.alive = false;
            phase = "dead";
            orbs.Clear();
            downMask = dropTellMask = 0;
            mortarPending = false;
            sweepState = 0;
        }

        void SetPhase(string ph)
        {
            phase = ph;
            phaseSteps = 0;
            said.Reset("crown");
            said.phase = ph;
            said.hp = body.hp;
            said.maxHp = body.maxHp;
            events.Emit("bossPhase", said);
        }

        void Say(string name, string stage, double sec = 0, int plate = -1)
        {
            ev.stage = stage;
            ev.beat = beat;
            ev.mask = downMask | dropTellMask;
            ev.at = stage == "mark" || stage == "land" ? mortarAt : core;
            ev.dir = beat == 3 ? chargeDir : PlateNormal(0);
            ev.seconds = sec;
            ev.plate = plate;
            events.Emit(name, ev);
        }

        void Tell(string what, double sec)
        {
            said.Reset("crown");
            said.name = what;
            said.seconds = sec;
            events.Emit("bossTell", said);
        }

        public BossBar Bar() => new BossBar { valid = body.alive, id = body.id, name = p.name, phase = phase, hp = Math.Max(0, body.hp), max = body.maxHp, segment = beat > 0 ? beat - 1 : 0 };

        /// <summary>The nearest open plate or the core along a unit ray (-1: a miss); hitPlate names the plate (-1: the core).</summary>
        public double Raycast(Vec3 o, Vec3 d)
        {
            if (!body.alive || phase == "dying" || phase == "dead") return -1;
            double best = CoreBox(p.hurtScale).Ray(o, d);
            int k = -1;
            if (PlatesOpen)
                for (int i = 0; i < p.plateCount; i++)
                {
                    double t = PlateBox(i, p.hurtScale).Ray(o, d);
                    if (t >= 0 && (best < 0 || t <= best + p.plateDepth * 2)) { if (k < 0 || t < best) { best = t; k = i; } }
                }
            hitPlate = best >= 0 ? k : -1;
            return best;
        }

        public double Filter(double amount, string weapon, Vec3 from, out bool blocked)
        {
            blocked = false;
            int k = hitPlate;
            hitPlate = -1;
            if (!Hurtable) { blocked = true; blockedHits++; return 0; }
            string cls = WeaponClass(weapon, amount);
            double outAmt;
            if (cls == "parry") { parryHits++; lastPart = "parry"; fullDamage += amount; outAmt = amount; } // NIGHT31's rule: a parried projectile lands in full (the sim sends orbs.parryDamage)
            else if (cls == "detonate") { coreHits++; lastPart = "core"; fullDamage += amount; outAmt = amount * p.detonateMult; } // nails stuck in the core blow its seams: x detonateMult wherever they sit
            else if (k >= 0 && PlatesOpen)
            {
                plateHits++;
                lastPart = "plate";
                fullDamage += amount;
                double m = cls == "shotgun" || cls == "slug" ? p.shotgunPlateMult : cls == "charge" ? p.chargePlateMult : p.plateMult;
                outAmt = amount * m;
                plateDamage += outAmt;
                Say("crownPlate", "hit", 0, k);
            }
            else
            {
                coreHits++;
                lastPart = "core";
                fullDamage += amount;
                outAmt = cls == "charge" ? amount * p.chargeCoreMult : amount;
            }
            byWeapon[cls] = (byWeapon.TryGetValue(cls, out double was) ? was : 0) + outAmt;
            return outAmt;
        }

        /// <summary>The kit's answer a hit belongs to (its weapon id and size): parry, charge (a charged KESTREL shot: revolver at plates.chargeMin or more), detonate (a NAILGUN
        /// nail's blast: nailgun at plates.detonateMin or more), slug (a THUNDERCLAP hit at plates.slugMin or more), shotgun, nailgun, revolver, gauntlet, or the weapon id.</summary>
        public string WeaponClass(string weapon, double amount)
        {
            if (weapon == Stag31.ParryWeapon) return "parry";
            if (weapon == null) return "other";
            if (weapon.IndexOf("charge", StringComparison.Ordinal) >= 0 || (weapon == "revolver" && amount >= p.chargeMin)) return "charge";
            if (weapon == "nailDetonate32" || (weapon == "nailgun" && amount >= p.detonateMin)) return "detonate"; // nailDetonate32: the sim's every-nail detonate (SurvivalSim.Crown32.cs)
            if (weapon == "shotgun" && amount >= p.slugMin) return "slug";
            return weapon;
        }

        /// <summary>The damage dealt this fight by each kit answer (WeaponClass): the harness's per-weapon share.</summary>
        public readonly Dictionary<string, double> byWeapon = new Dictionary<string, double>();

        public void Apply(double dmg)
        {
            if (!body.alive) return;
            body.hp -= dmg;
            totalDamage += dmg;
            body.flash = 0.06;
            said.Reset("crown");
            said.damage = JsMath.Round(dmg);
            said.part = lastPart ?? "core";
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            lastPart = null;
            if (body.hp <= 0 && phase != "dying")
            {
                body.hp = 0;
                SetPhase("dying");
                dieSteps = S(p.dieTime);
                orbs.Clear();
                downMask = dropTellMask = 0;
                mortarPending = false;
                sweepState = 0;
                kit?.Dying("crown", body.position);
                return;
            }
            if (phase != "p1") return;
            int want = HpFrac <= p.beat3At ? 3 : HpFrac <= p.beat2At ? 2 : 1;
            if (want > beat) SetBeat(want);
        }

        void SetBeat(int b)
        {
            beat = b;
            beatHold = Math.Max(1, S(p.beatSeconds));
            ResetActs();
            orbTimer = S(p.orbEvery[Math.Min(p.orbEvery.Length - 1, b - 1)]);
            dropTurn = rng.Next() < 0.5 ? 1 : -1;
            dropStart = (int)Math.Floor(rng.Next() * arena.p.sectors) % arena.p.sectors;
            orbitAngle = (dropStart + 0.5) * arena.SectorAngle;
            Say("crownBeat", "start", p.beatSeconds);
            if (kit != null) kit.Beat("crown", "beat" + b, core);
        }

        public void Step(double dt)
        {
            if (!body.alive) return;
            body.previous = body.position;
            if (Frozen) return;
            hitPlate = -1;
            stepNo++;
            phaseSteps++;
            if (phase == "intro" && phaseSteps >= introSteps) { SetPhase("p1"); Say("crownBeat", "start", 0); }
            if (phase == "p1")
            {
                if (Hurtable) hurtableSteps++; else unhittableSteps++;
                StepFight(dt);
            }
            StepOrbs(dt);
            Pose();
            if (phase == "dying" && --dieSteps <= 0)
            {
                body.alive = false;
                phase = "dead";
                kit?.Down("crown", body.id, body.position);
            }
        }

        void StepFight(double dt)
        {
            Vec3 me = target.position();
            Vec3 v = playerVelocity != null ? playerVelocity() : default;
            bool riding = playerWallRiding != null && playerWallRiding();
            double flat = JsMath.Hypot(v.x, v.z);
            stillTime = flat < p.stillSpeed && !riding ? stillTime + dt : 0;
            if (lavaGrace > 0) lavaGrace--;
            // the core's height for the beat (it moves there through the beat hold)
            double wantY = origin.y + p.hoverY[Math.Max(0, Math.Min(2, beat - 1))];
            double dy = wantY - core.y, stepY = p.hoverSpeed * dt;
            core = new Vec3(core.x, core.y + (Math.Abs(dy) <= stepY ? dy : Math.Sign(dy) * stepY), core.z);
            if (beat < 3) yawRad += p.spinRate * dt;
            if (beat == 2)
            {
                // the low core orbits the ring (the player chases it across the drop ring)
                orbitAngle += p.orbitRate * dt;
                Vec3 to = origin + Crown32Arena.Polar(orbitAngle, p.orbitRadius, 0);
                double mx = to.x - core.x, mz = to.z - core.z, ml = JsMath.Hypot(mx, mz), stepM = p.moveSpeed * dt;
                if (ml > 1e-9) { double f = ml <= stepM ? 1 : stepM / ml; core = new Vec3(core.x + mx * f, core.y, core.z + mz * f); }
            }
            StepMortar(me);
            if (beatHold > 0) { beatHold--; return; }
            if (beat == 1) StepSweep(dt, me);
            if (beat == 2) StepDrop(me);
            if (beat == 3) StepCharge(dt, me);
            if (beat < 3) StepOrbFire(me, v);
        }

        // --- the mortar (every beat) ---------------------------------------------------------------------------------------------------------

        void StepMortar(Vec3 me)
        {
            if (mortarPending)
            {
                if (--mortarLeft > 0) return;
                mortarPending = false;
                Say("crownMortar", "land", 0);
                double d = JsMath.Hypot(me.x - mortarAt.x, me.z - mortarAt.z);
                if (d <= p.mortarRadius && Math.Abs(me.y - mortarAt.y) <= 2.5)
                {
                    Health h = target.health;
                    if (h != null && !h.Dead && h.Damage(h.max * p.mortarDamageFrac * scale.damage, "boss")) mortarHits++;
                    target.impulse?.Invoke(0, p.mortarKnockUp, 0);
                    Say("crownMortar", "hit", 0);
                }
                return;
            }
            if (stillTime < p.stillSeconds) return;
            stillTime = 0;
            mortars++;
            mortarPending = true;
            mortarLeft = Math.Max(1, S(p.mortarDelay));
            mortarAt = me;
            Say("crownMortar", "mark", p.mortarDelay);
            Tell("mortar", p.mortarDelay);
        }

        // --- beat 1: the sweeping beam ------------------------------------------------------------------------------------------------------

        void StepSweep(double dt, Vec3 me)
        {
            if (sweepState == 0)
            {
                if (--sweepTimer > 0) return;
                sweepState = 1;
                sweepSteps = Math.Max(1, S(p.sweepTell));
                actSteps = 0;
                actLen = sweepSteps;
                sweepDir = rng.Next() < 0.5 ? 1 : -1;
                double pa = JsMath.Atan2(me.z - core.z, me.x - core.x);
                sweepStartAngle = sweepAngle = pa + Math.PI; // it starts on the side away from the player
                sweepHit = false;
                Say("crownSweep", "tell", p.sweepTell);
                Tell("sweep", p.sweepTell);
                return;
            }
            actSteps++;
            if (sweepState == 1)
            {
                if (--sweepSteps > 0) return;
                sweepState = 2;
                sweeps++;
                actSteps = 0;
                actLen = Math.Max(1, (int)Math.Ceiling(2 * Math.PI / Math.Max(1e-6, p.sweepRate) / dt0));
                Say("crownSweep", "start", 2 * Math.PI / p.sweepRate);
                return;
            }
            sweepAngle += sweepDir * p.sweepRate * dt;
            if (!sweepHit && BeamTouches(me))
            {
                sweepHit = true;
                Health h = target.health;
                if (h != null && !h.Dead && h.Damage(h.max * p.sweepDamageFrac * scale.damage, "boss")) sweepHits++;
                Vec3 t = new Vec3(-JsMath.Sin(sweepAngle) * sweepDir, 0, JsMath.Cos(sweepAngle) * sweepDir);
                target.impulse?.Invoke(t.x * p.sweepKnock, p.sweepKnockUp, t.z * p.sweepKnock);
                Say("crownSweep", "hit", 0);
            }
            if (Math.Abs(sweepAngle - sweepStartAngle) >= 2 * Math.PI)
            {
                sweepState = 0;
                sweepTimer = S(p.sweepEvery);
                Say("crownSweep", "end", 0);
            }
        }

        /// <summary>Does the beam (from the core out to the wall at sweepAngle, beamLow..beamHigh over the floor) touch the player's collider now?</summary>
        public bool BeamTouches(Vec3 me)
        {
            double dx = me.x - core.x, dz = me.z - core.z;
            double cx = JsMath.Cos(sweepAngle), cz = JsMath.Sin(sweepAngle);
            double along = dx * cx + dz * cz, lat = Math.Abs(-dx * cz + dz * cx);
            if (along < p.coreHalf || lat > p.beamHalfWidth + 0.35) return false;
            double feet = me.y - FloorY, top = feet + (playerHeight != null ? playerHeight() : 1.8);
            return feet < p.beamHigh && top > p.beamLow;
        }

        // --- the orbs (beats 1-2) -----------------------------------------------------------------------------------------------------------

        void StepOrbFire(Vec3 me, Vec3 v)
        {
            if (orbTellLeft > 0)
            {
                if (--orbTellLeft > 0) return;
                Vec3 chest = new Vec3(me.x, me.y + 1.0, me.z);
                double tFly = Vec3.DistanceTo(core, chest) / Math.Max(1e-6, p.orbSpeed);
                Vec3 led = new Vec3(chest.x + v.x * tFly, chest.y, chest.z + v.z * tFly);
                Vec3 d = Vec3.Normalize(led - core);
                orbs.Add(new Crown32Orb { pos = core + d * (HurtHalf + 0.3), vel = d * p.orbSpeed, life = S(p.orbLife), alive = true });
                orbsFired++;
                Say("crownOrb", "fire", 0);
                orbTimer = S(p.orbEvery[Math.Min(p.orbEvery.Length - 1, beat - 1)]);
                return;
            }
            if (--orbTimer > 0) return;
            orbTellLeft = Math.Max(1, S(p.orbTell));
            Say("crownOrb", "tell", p.orbTell);
        }

        void StepOrbs(double dt)
        {
            if (orbs.Count == 0) return;
            Vec3 me = target.position();
            Vec3 chest = new Vec3(me.x, me.y + 1.0, me.z);
            for (int i = orbs.Count - 1; i >= 0; i--)
            {
                Crown32Orb o = orbs[i];
                o.pos = o.pos + o.vel * dt;
                double lx = o.pos.x - origin.x, lz = o.pos.z - origin.z;
                if (--o.life <= 0 || !o.alive || arena.WallDistance(lx, lz) < 0 || o.pos.y < FloorY - arena.p.lavaDepth) { orbs.RemoveAt(i); continue; }
                if (phase == "p1" && Vec3.DistanceTo(o.pos, chest) <= p.orbRadius + 0.35)
                {
                    Health h = target.health;
                    if (h != null && !h.Dead && h.Damage(p.orbDamage * scale.damage, "boss")) orbHits++;
                    Say("crownOrb", "hit", 0);
                    orbs.RemoveAt(i);
                }
            }
        }

        // --- beat 2: the drop ring ----------------------------------------------------------------------------------------------------------

        int PatternMask()
        {
            int m = 0, n = arena.p.sectors;
            foreach (int o in p.dropPattern) m |= 1 << ((((dropStart + o) % n) + n) % n);
            return m;
        }

        /// <summary>Is the arena-local point over a down sector's pit (not the inner disc, not a ledge)?</summary>
        public bool InPit(Vec3 local)
        {
            if (downMask == 0 || arena.InInner(local.x, local.z) || arena.WallDistance(local.x, local.z) < 0) return false;
            return (downMask & (1 << arena.SectorAt(local.x, local.z))) != 0;
        }

        void StepDrop(Vec3 me)
        {
            if (--dropLeft <= 0)
            {
                if (dropState == 0)
                {
                    dropState = 1;
                    dropTellMask = PatternMask();
                    dropLeft = Math.Max(1, S(p.dropTell));
                    Say("crownDrop", "tell", p.dropTell);
                    Tell("drop", p.dropTell);
                }
                else if (dropState == 1)
                {
                    dropState = 2;
                    downMask = dropTellMask;
                    dropTellMask = 0;
                    drops++;
                    dropLeft = Math.Max(1, S(p.dropDown));
                    Say("crownDrop", "down", p.dropDown);
                }
                else
                {
                    dropState = 0;
                    downMask = 0;
                    dropStart = (dropStart + dropTurn + arena.p.sectors) % arena.p.sectors;
                    dropLeft = Math.Max(1, S(p.dropGap));
                    Say("crownDrop", "rise", 0);
                }
            }
            // the lava: the player's feet in a down sector's pit (design.n30.lava's numbers)
            Vec3 local = me - origin;
            if (lavaGrace == 0 && InPit(local) && local.y <= -arena.p.lavaDepth + 0.25)
            {
                Health h = target.health;
                if (h != null && !h.Dead && h.Damage(h.max * p.lavaFraction, "boss")) lavaHits++;
                target.impulse?.Invoke(0, p.lavaKnockUp, 0);
                lavaGrace = S(p.lavaGrace);
                Say("crownDrop", "burn", 0);
            }
        }

        // --- beat 3: the charge -------------------------------------------------------------------------------------------------------------

        void SetAct(string a, int len)
        {
            act = a;
            actSteps = 0;
            actLen = len;
        }

        static Vec3 Flat(Vec3 v)
        {
            double l = JsMath.Hypot(v.x, v.z);
            return l < 1e-9 ? new Vec3(0, 0, -1) : new Vec3(v.x / l, 0, v.z / l);
        }

        /// <summary>Where a run from `from` (world) along the flat `dir` meets the wall less the core's half size (world, on the core's height).</summary>
        Vec3 WallStop(Vec3 from, Vec3 dir)
        {
            // the circle of radius apothem - HurtHalf (inside every facet): |l + t dir| = R
            double R = arena.p.apothem - HurtHalf - 0.1;
            double lx = from.x - origin.x, lz = from.z - origin.z;
            double b = lx * dir.x + lz * dir.z, c = lx * lx + lz * lz - R * R;
            double disc = b * b - c;
            double t = disc > 0 ? -b + Math.Sqrt(disc) : 0;
            if (t < 0) t = 0;
            return new Vec3(from.x + dir.x * t, from.y, from.z + dir.z * t);
        }

        void StepCharge(double dt, Vec3 me)
        {
            actSteps++;
            switch (act)
            {
                case "hover":
                    if (--chargeTimer > 0) return;
                    chargeDir = Flat(new Vec3(me.x - core.x, 0, me.z - core.z));
                    lineEnd = WallStop(core, chargeDir);
                    yawRad = JsMath.Atan2(chargeDir.z, chargeDir.x);
                    SetAct("tell", Math.Max(1, S(p.chargeTell)));
                    Say("crownCharge", "tell", p.chargeTell);
                    Tell("charge", p.chargeTell);
                    return;
                case "tell":
                    if (actSteps >= actLen) { SetAct("charge", 0); chargeHit = false; charges++; Say("crownCharge", "run", 0); }
                    return;
                case "charge":
                {
                    double stepL = p.chargeSpeed * dt;
                    double left = JsMath.Hypot(lineEnd.x - core.x, lineEnd.z - core.z);
                    if (left <= stepL)
                    {
                        core = new Vec3(lineEnd.x, core.y, lineEnd.z);
                        slams++;
                        SetAct("stun", Math.Max(1, S(p.stunSeconds)));
                        Say("crownSlam", "slam", p.stunSeconds);
                        return;
                    }
                    core = new Vec3(core.x + chargeDir.x * stepL, core.y, core.z + chargeDir.z * stepL);
                    if (!chargeHit)
                    {
                        double dx = me.x - core.x, dz = me.z - core.z;
                        double along = dx * chargeDir.x + dz * chargeDir.z, lat = Math.Abs(-dx * chargeDir.z + dz * chargeDir.x);
                        if (along > -HurtHalf && along < HurtHalf + 0.5 && lat <= p.chargeHalfWidth && me.y <= core.y + HurtHalf)
                        {
                            chargeHit = true;
                            Health h = target.health;
                            if (h != null && !h.Dead && h.Damage(h.max * p.chargeDamageFrac * scale.damage, "boss")) chargeHits++;
                            target.impulse?.Invoke(chargeDir.x * p.chargeKnock, p.chargeKnockUp, chargeDir.z * p.chargeKnock);
                            Say("crownCharge", "hit", 0);
                        }
                    }
                    return;
                }
                case "stun":
                    if (actSteps >= actLen) { SetAct("recover", Math.Max(1, S(p.recoverSeconds))); Say("crownSlam", "end", 0); }
                    return;
                case "recover":
                {
                    // back toward the middle at the speed that gets it there in recoverSeconds
                    double left = Math.Max(1, actLen - actSteps + 1);
                    core = new Vec3(core.x + (origin.x - core.x) / left, core.y, core.z + (origin.z - core.z) / left);
                    if (actSteps >= actLen) { SetAct("hover", 0); chargeTimer = S(p.chargeFirst); }
                    return;
                }
            }
        }
    }
}
