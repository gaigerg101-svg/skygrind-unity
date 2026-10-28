// NIGHT37 builder C (crown37; NIGHT37.md C1, BOSSES.md "L5 THE CROWN"; DECIDE37 D-3 / D-5 / D-6): THE CROWN, level 5's boss, from scratch, on
// K's Boss37 brain (Core/Boss37.cs). A ring of 8 gem-set blades round an empty centre (~6 m across), spinning, that hovers beside the player
// as he DESCENDS the spire (Core/Orbit37.cs: the summit, the orbiting rings, the gallery, the plaza).
//   THE PASS (its damage rule, BOSSES.md rule 2): a body point (the chest) crossing the ring's plane within passRadius of its centre while
//            airborne (a jump-dash THROUGH it) breaks passGems gems (torchGems with the TORCH burning: fire held with the TORCH in hand
//            within torchWindow) and drains the stagger meter by passStagger. Weapons do nothing to the gems outside the CYAN window
//            (BOSSES.md: "the gems take damage only at close range"); in the window every hit lands in full (D-5).
//   THE PLACE it hovers: between the player and the next lower piece he can reach (passLead of the way, passAbove over it): jump-dash
//            through it and land on that piece. With no piece to aim at, hoverDistance m from him along the spire's turn. Its facing turns to
//            the player; within lockRadius it locks (a still plane to pass through).
//   ATTACKS  (the brain picks them: Boss37Attack rows in design.n37.crown37.attacks): bladeRing (RED: a ring cut sweeping out from the
//            core at your chest's height: drop or jump), gemShard (YELLOW: a shard at you: punch it back and a gem socket shatters: a
//            parry), beamCut (RED: the piece you head for tells, then drops), lowFire (RED, the air habit: a shard under your feet's path;
//            forced after airborneSeconds in the air), plazaDrop (RED, leg 3: the plaza sector nearest you drops).
//   HABIT    one piece stood on for habitStandSeconds: it tells habitTellSeconds and drops (never a checkpoint).
//   LEGS     places (autoLegs off): the summit shatters when the reveal ends (leg 1: the orbits); gems left <= legGemsLeft[0]: leg 2, the
//            orbits fall away (the gallery); <= legGemsLeft[1]: leg 3, the gallery falls (the plaza; the Crown lands at landY). The sim's
//            fall rule (feet under the leg's floor) restarts the leg from its checkpoint (Boss37Brain.RestartLeg, -fallDamage).
//   FINISH   in leg 3 a KESTREL charge (revolver >= finisherMin.charge) or a THUNDERCLAP slug (shotgun >= finisherMin.slug) hitting the
//            ring's opening ends it (every gem left).
// Pure (no UnityEngine), 120 Hz whole steps, JsMath, no randomness but the brain's own Mulberry32. No audio: events keep names for GG
// (crown37Pass, crown37Gem, crown37Shatter, crown37Cut, crown37Shard, crown37Fall; plus the brain's boss37* and the kit's boss*).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>The payload of crown37* events (pass, gem, leg, fall, attack stages): pooled, read it in the handler.</summary>
    public sealed class Crown37Event
    {
        public string stage, attack;
        public int gems, leg, broke;
        public Vec3 at;
    }

    public sealed class Crown37Params
    {
        public string name;
        public int gems, passGems, torchGems;
        public double gemHp, ringRadius, passRadius, lockRadius, spinRate, moveSpeed, hoverDistance, passAbove, targetMin, targetMax, passSpeed, torchWindow, passStagger,
            parryStagger, chargeMin, slugMin, fallDamage, revealSeconds, habitStandSeconds, habitTellSeconds, airborneSeconds, landY, dieSeconds;
        public double[] legGemsLeft;
        public bool grantTorch;
        public double dashSpeed, launchLead;
        /// <summary>Each leg's opening attacks, forced in order as the leg's fight starts (crown.openers: the pressure every leg meets).</summary>
        public List<string>[] openers = new List<string>[3];
        public double bladeDamage, bladeHalfThick, bladeReach, shardSpeed, shardDamage, shardRadius, parryReach, lowSpeed, lowDamage, lowRadius, lowDrop;

        public static Crown37Params From(GameConfig c)
        {
            const string g = "design.n37.crown37.crown.", a = "design.n37.crown37.attacks.";
            return new Crown37Params
            {
                name = c.Str("design.n37.crown37.name"),
                gems = c.Int(g + "gems"), gemHp = c.Num(g + "gemHp"), legGemsLeft = c.Nums(g + "legGemsLeft"),
                ringRadius = c.Num(g + "ringRadius"), passRadius = c.Num(g + "passRadius"), lockRadius = c.Num(g + "lockRadius"), spinRate = c.Num(g + "spinRate"),
                moveSpeed = c.Num(g + "moveSpeed"), hoverDistance = c.Num(g + "hoverDistance"), passAbove = c.Num(g + "passAbove"), targetMin = c.Num(g + "targetMin"), targetMax = c.Num(g + "targetMax"), passSpeed = c.Num(g + "passSpeed"),
                passGems = c.Int(g + "passGems"), torchGems = c.Int(g + "torchGems"), torchWindow = c.Num(g + "torchWindow"), grantTorch = c.Bool(g + "grantTorch"),
                passStagger = c.Num(g + "passStagger"), parryStagger = c.Num(g + "parryStagger"),
                chargeMin = c.Num(g + "finisherMin.charge"), slugMin = c.Num(g + "finisherMin.slug"), fallDamage = c.Num(g + "fallDamage"),
                revealSeconds = c.Num(g + "revealSeconds"), habitStandSeconds = c.Num(g + "habitStandSeconds"), habitTellSeconds = c.Num(g + "habitTellSeconds"),
                airborneSeconds = c.Num(g + "airborneSeconds"), dashSpeed = c.movement.dash.speed, launchLead = c.Num(g + "launchLead"), landY = c.Num(g + "landY"), dieSeconds = 1.5,
                bladeDamage = c.Num(a + "bladeRing.damage"), bladeHalfThick = c.Num(a + "bladeRing.halfThick"), bladeReach = c.Num(a + "bladeRing.reach"),
                shardSpeed = c.Num(a + "gemShard.speed"), shardDamage = c.Num(a + "gemShard.damage"), shardRadius = c.Num(a + "gemShard.radius"), parryReach = c.Num(a + "gemShard.parryReach"),
                openers = new[] { c.Strs(g + "openers.leg1"), c.Strs(g + "openers.leg2"), c.Strs(g + "openers.leg3") },
                lowSpeed = c.Num(a + "lowFire.speed"), lowDamage = c.Num(a + "lowFire.damage"), lowRadius = c.Num(a + "lowFire.radius"), lowDrop = c.Num(a + "lowFire.drop"),
            };
        }
    }

    public sealed class Crown37Body : Enemy, IBossBody27
    {
        public readonly Crown37 system;
        public Crown37Body(Crown37 system) { this.system = system; kind = "crown37"; }
        public override double Height => system.p.ringRadius * 2;
        public override double Radius => system.p.ringRadius;
        public override double AimHeight => system.p.ringRadius;
        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            double t = system.Raycast(o, d, out bool centre);
            headshot = centre;
            return t;
        }
        public override bool Damage(double amount, double flashTime) { system.Apply(amount, null); return false; }
        public double Filter(double amount, string weapon, Vec3 from, out bool blocked) => system.Filter(amount, weapon, out blocked);
        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt) { }
        public override void Stagger(double time, double dt) { }
        public override void Stun(double time, double dt) { }
    }

    /// <summary>A shard in flight (gemShard: yellow, parryable; lowFire: red).</summary>
    public sealed class Crown37Shard
    {
        public Vec3 pos, prev, vel;
        public int life;
        public bool alive, back, red;
    }

    /// <summary>What the sim hands the Crown each step (the player's side).</summary>
    public struct Crown37Input
    {
        public Vec3 feet, vel, chestPrev, chest;
        public bool grounded, riding, torchBurning;
    }

    public sealed partial class Crown37 : IBossSystem
    {
        public readonly Crown37Params p;
        public readonly Crown37Body body;
        public readonly Boss37Brain brain;
        readonly EventBus events;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt;
        readonly BossEvent said = new BossEvent();
        readonly Crown37Event ev = new Crown37Event();
        BossKit kit;

        /// <summary>The descent's pieces (the sim's Orbit37) and the spire's axis, leg floors.</summary>
        public Orbit37 orbit;
        public Vec3 axis;
        public double plazaY;

        /// <summary>The ring's centre, facing (unit), the blades' spin (rad); the facing is locked within lockRadius.</summary>
        public Vec3 centre, normal = new Vec3(0, 0, 1), prevCentre;
        public double spin;
        public bool locked;
        /// <summary>The piece the Crown sets the pass toward (-1: none).</summary>
        public int passTarget = -1;
        public readonly List<Crown37Shard> shards = new List<Crown37Shard>();
        /// <summary>bladeRing: its cut's height (world y) and the front's radius from the axis now (0: none live).</summary>
        public double cutY, cutR, cutPrevR;
        bool cutHit;
        /// <summary>The step of the last pass (the view's cyan flash), the last gem broken.</summary>
        public int stepNo, lastPassStep = -100000, lastGemStep = -100000;
        double airTime;
        int openerLeg, openerAt;
        /// <summary>Per leg (index 0 = leg 1): beam cuts (and plaza drops) of the piece the player was offered / heading to, blade rings, blade rings that hit.</summary>
        public readonly int[] legCuts = new int[3], legBlades = new int[3], legBladeHits = new int[3];
        bool shattered;
        int dieSteps;
        /// <summary>Counters (the harness): passes, torch passes, gems broken by pass / parry / window / finisher, hits taken by the player per attack,
        /// blocked weapon hits, falls.</summary>
        public int passes, torchPasses, gemsByPass, gemsByParry, finishers, bladeHits, shardHits, lowHits, blocked, habitDrops, cuts, parries;
        public double windowDamage;
        /// <summary>The airborne habit: times it forced lowFire, the step of the last.</summary>
        public int airForces, airForceStep;

        public Crown37(GameConfig cfg, Crown37Params p, EventBus events, BossTarget target, Func<int> nextId, uint seed)
        {
            this.p = p;
            this.events = events;
            this.target = target;
            this.nextId = nextId;
            dt = cfg.Dt;
            body = new Crown37Body(this);
            brain = new Boss37Brain(Boss37Params.From(cfg, "crown37"), "crown37", dt, events, seed);
            foreach (string id in new[] { "bladeRing", "gemShard", "beamCut", "lowFire", "plazaDrop" })
                brain.attacks.Add(Boss37Attack.From(cfg, "design.n37.crown37.attacks." + id, dt));
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt);
        int LegIx => Math.Max(0, Math.Min(2, brain.leg - 1));

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public int GemsLeft => body.hp <= 0 ? 0 : (int)Math.Ceiling(body.hp / p.gemHp - 1e-9);
        public bool Open => brain.Open;
        public bool Hurtable => body.alive && brain.Fighting;
        public int Leg => brain.leg;
        public bool Dying => brain.state == Boss37State.Dying;

        public void StartFight(BossFight ctx)
        {
            kit = ctx.kit;
            body.id = nextId();
            body.alive = true;
            body.maxHp = p.gems * p.gemHp;
            body.hp = body.maxHp;
            stepNo = 0;
            shards.Clear();
            cutR = cutPrevR = 0;
            airTime = 0;
            shattered = false;
            passes = torchPasses = gemsByPass = gemsByParry = finishers = bladeHits = shardHits = lowHits = blocked = habitDrops = cuts = parries = 0;
            windowDamage = 0;
            openerLeg = openerAt = 0;
            closed = false;
            Array.Clear(legCuts, 0, 3); Array.Clear(legBlades, 0, 3); Array.Clear(legBladeHits, 0, 3);
            lastPassStep = lastGemStep = -100000;
            brain.source = body.id;
            Vec3 me = target.position();
            // it rises out of the spire's top through the reveal (crown40: from just over the summit, never inside the core)
            centre = n40 != null ? new Vec3(axis.x, axis.y + p.ringRadius + n40.coreMargin, axis.z) : new Vec3(axis.x, axis.y - 8, axis.z);
            corePushes40 = clamps40 = fast40 = 0;
            lastBlockStep = lastHitStep = -100000;
            lastBlockWeapon = null;
            prevCentre = centre;
            brain.Begin(S(p.revealSeconds), me);
            Pose();
            body.previous = body.position;
            said.Reset("crown37");
            said.name = p.name;
            said.hp = body.maxHp;
            events.Emit("bossRise", said);
        }

        public void Clear()
        {
            body.alive = false;
            shards.Clear();
            cutR = 0;
            brain.tells?.End(brain.source);
            brain.Reset();
        }

        public BossBar Bar() => new BossBar
        {
            valid = body.alive, id = body.id, name = p.name, phase = brain.state.ToString(), hp = Math.Max(0, body.hp), max = body.maxHp, segment = brain.leg - 1,
            pips = p.gems, pipsOn = GemsLeft,
        };

        void Pose() => body.position = new Vec3(centre.x, centre.y - p.ringRadius, centre.z);

        void Say(string name, string stage, string attack = null, int broke = 0, Vec3? at = null)
        {
            ev.stage = stage;
            ev.attack = attack;
            ev.gems = GemsLeft;
            ev.leg = brain.leg;
            ev.broke = broke;
            ev.at = at ?? centre;
            events.Emit(name, ev);
        }

        // ---- damage ------------------------------------------------------------------------------------------------------------------------------

        /// <summary>The ray's distance to the ring's disc (the blades and the opening: -1 a miss); centre = through the opening.</summary>
        public double Raycast(Vec3 o, Vec3 d, out bool throughCentre)
        {
            throughCentre = false;
            if (!body.alive || brain.state == Boss37State.Dead) return -1;
            double dn = d.x * normal.x + d.y * normal.y + d.z * normal.z;
            if (Math.Abs(dn) < 1e-6) return -1;
            double t = ((centre.x - o.x) * normal.x + (centre.y - o.y) * normal.y + (centre.z - o.z) * normal.z) / dn;
            if (t < 0) return -1;
            Vec3 hit = new Vec3(o.x + d.x * t, o.y + d.y * t, o.z + d.z * t);
            double r = Vec3.DistanceTo(hit, centre);
            if (r > p.ringRadius + 0.6) return -1;
            throughCentre = r <= p.passRadius;
            lastRayCentre = throughCentre;
            return t;
        }

        bool lastRayCentre;

        /// <summary>The share of a weapon hit that lands: the CYAN window takes everything; leg 3's finisher through the opening ends it; else 0.</summary>
        public double Filter(double amount, string weapon, out bool isBlocked)
        {
            isBlocked = false;
            bool centreHit = lastRayCentre;
            lastRayCentre = false;
            if (!Hurtable) { isBlocked = true; blocked++; return 0; }
            if (brain.leg >= 3 && centreHit && (weapon == "revolver" && amount >= p.chargeMin || weapon != null && weapon.IndexOf("charge", StringComparison.Ordinal) >= 0 || weapon == "shotgun" && amount >= p.slugMin))
            {
                finishers++;
                Say("crown37Gem", "finisher", weapon, GemsLeft);
                return Math.Max(amount, body.hp + 1);
            }
            if (brain.Open) { if (crown47) amount = Window47(amount); windowDamage += amount; lastHitStep = stepNo; return amount; } // NIGHT47 L: a window's gem cap
            if (crown47) return Chip47(amount, weapon); // NIGHT47 L (crown47; Crown37.N47.cs): every gun chips outside the window
            isBlocked = true;
            blocked++;
            lastBlockStep = stepNo; // NIGHT40 B5: the view's BLOCKED (not the sim's state: nothing reads it back)
            lastBlockWeapon = weapon;
            return 0;
        }

        public void Apply(double dmg, string part)
        {
            if (!body.alive || dmg <= 0) return;
            int before = GemsLeft;
            body.hp -= dmg;
            body.flash = 0.06;
            said.Reset("crown37");
            said.damage = JsMath.Round(dmg);
            said.part = part ?? (brain.Open ? "window" : "gem");
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            int after = GemsLeft;
            if (after < before) { lastGemStep = stepNo; Say("crown37Gem", "break", part, before - after); }
            if (body.hp <= 0 && !Dying && brain.state != Boss37State.Dead)
            {
                body.hp = 0;
                shards.Clear();
                cutR = 0;
                brain.Die(S(p.dieSeconds));
                dieSteps = S(p.dieSeconds);
                kit?.Dying("crown37", centre);
                return;
            }
            LegCheck();
        }

        /// <summary>Break `n` whole gems now (a pass, a parried shard).</summary>
        void BreakGems(int n, string part)
        {
            int left = Math.Max(0, GemsLeft - n);
            double want = left * p.gemHp;
            if (want < body.hp) Apply(body.hp - want, part);
        }

        void LegCheck()
        {
            if (!body.alive || Dying) return;
            int left = GemsLeft;
            if (brain.leg == 1 && left <= p.legGemsLeft[0]) ToLeg(2);
            if (brain.leg == 2 && left <= p.legGemsLeft[1]) ToLeg(3);
        }

        void ToLeg(int leg)
        {
            brain.NextLeg();
            shards.Clear();
            cutR = 0;
            // the arena changes: the leg above falls away (the orbits in leg 2, the gallery in leg 3)
            Piece37Kind gone = leg == 2 ? Piece37Kind.Orbit : Piece37Kind.Gallery;
            for (int i = 0; i < orbit.pieces.Count; i++) if (orbit.pieces[i].kind == gone) orbit.Drop(i, 0, "leg", events);
            Say("crown37Leg", "change", null, 0);
        }

        /// <summary>A fall (the sim's rule): the brain restarts the leg; the leg's pieces come back. Returns the checkpoint.</summary>
        public Vec3 FallRestart()
        {
            Vec3 at = brain.RestartLeg();
            shards.Clear();
            cutR = 0;
            airTime = 0;
            Piece37Kind mine = brain.leg == 1 ? Piece37Kind.Orbit : brain.leg == 2 ? Piece37Kind.Gallery : Piece37Kind.Plaza;
            for (int i = 0; i < orbit.pieces.Count; i++) if (orbit.pieces[i].kind == mine) orbit.Raise(i, events);
            Say("crown37Fall", "restart");
            return at;
        }

        // ---- the parry -----------------------------------------------------------------------------------------------------------------------------

        /// <summary>A punch from `eye` along `dir`: the nearest live yellow shard within parryReach in front goes back, a gem socket shatters (a gem) and
        /// the brain takes a parry (refund + stagger).</summary>
        public bool Punch(Vec3 eye, Vec3 dir)
        {
            if (!body.alive) return false;
            double best = p.parryReach + p.shardRadius, cosCone = 0.5;
            int idx = -1;
            for (int i = 0; i < shards.Count; i++)
            {
                Crown37Shard s = shards[i];
                if (!s.alive || s.back || s.red) continue;
                Vec3 to = s.pos - eye;
                double d = to.Length;
                if (d > best) continue;
                if (d > 0.5 && Vec3.Dot(to, dir) / d < cosCone) continue;
                best = d;
                idx = i;
            }
            if (idx < 0) return false;
            Crown37Shard sh = shards[idx];
            sh.back = true;
            Vec3 home = centre - sh.pos;
            sh.vel = home * (p.shardSpeed * 1.5 / Math.Max(1e-6, home.Length));
            sh.life = S(2);
            parries++;
            gemsByParry++;
            brain.Parried(p.parryStagger); // the socket shatters: stagger (the gems break only to a pass or in the window: no damage from a standing parry)
            Say("crown37Shard", "parry", "gemShard", 1, sh.pos);
            events.Emit("parry", new HitEvent { owner = "player", weapon = "gauntlet", kind = "crown37Shard", enemy = body.id, damage = p.gemHp, position = sh.pos });
            return true;
        }

        // ---- one step -----------------------------------------------------------------------------------------------------------------------------

        public void Step(in Crown37Input inp)
        {
            if (!body.alive) return;
            body.previous = body.position;
            prevCentre = centre;
            if (Frozen) return;
            stepNo++;
            if (Dying)
            {
                centre = new Vec3(centre.x, centre.y - 6 * dt, centre.z); // it sinks as it dies
                spin += p.spinRate * 3 * dt;
                brain.Step(Ctx(inp));
                Pose();
                if (--dieSteps <= 0)
                {
                    body.alive = false;
                    brain.state = Boss37State.Dead;
                    kit?.Down("crown37", body.id, centre);
                }
                return;
            }
            bool wasReveal = brain.state == Boss37State.Reveal;
            brain.Step(Ctx(inp));
            if (wasReveal && brain.state != Boss37State.Reveal && !shattered && !crown47) Shatter();
            if (crown47) Crown47Step(inp); // NIGHT47 L (crown47): the summit stays in phase 1, the window's gems, the burn
            if (brain.state == Boss37State.Reveal)
            {
                // the reveal: it rises out of the spire's top to hover over the summit, between the player and the core
                Vec3 to = new Vec3(axis.x + (inp.feet.x - axis.x) * 0.4, axis.y + 5, axis.z + (inp.feet.z - axis.z) * 0.4);
                MoveTo(Keep40(to), p.moveSpeed * 0.5);
                Face(inp.chest);
            }
            else
            {
                if (brain.ConsumeReposition())
                {
                    if (n40 == null) centre = HoverPoint(inp); // the anti-hide: onto the player at once
                    else fast40 = S(1.5); // crown40: no teleport: it flies to him fast, seen (repositionSpeed for up to 1.5 s)
                }
                Vec3 want = Keep40(HoverPoint(inp), true, inp.feet);
                double dPlayer = Vec3.DistanceTo(inp.chest, centre);
                locked = dPlayer <= p.lockRadius && !inp.grounded;
                if (fast40 > 0) { fast40--; if (Vec3.DistanceTo(want, centre) < 0.5) fast40 = 0; }
                Vec3 before40 = centre;
                double speed = fast40 > 0 ? n40.repositionSpeed : p.moveSpeed;
                if (!locked && !Hold47) { MoveTo(want, speed); Face(inp.chest); } // NIGHT47 L (crown47): it holds still through the flame and the toll
                if (n40 != null && !Hold47) centre = Keep40(Round40(before40, centre, want, speed * dt)); // round the core, inside the room
            }
            spin += (brain.Open ? 0 : p.spinRate) * dt;
            // THE PASS
            bool slam47 = crown47 && inp.vel.y <= -p47.slamSpeed; // NIGHT47 L (crown47): a slam (VERBS 7) down through the ring is a pass too
            if (brain.Fighting && !inp.grounded && (JsMath.Hypot(inp.vel.x, inp.vel.z) >= p.passSpeed || slam47) && Hoop37.Passed(inp.chestPrev, inp.chest, prevCentre, normal, p.passRadius, out Vec3 at))
            {
                passes++;
                lastPassStep = stepNo;
                int n = inp.torchBurning ? p.torchGems : p.passGems;
                if (crown47) n = slam47 ? p47.slamGems : inp.torchBurning ? p47.torchPassGems : p47.passGems; // NIGHT47 L (crown47): the pass cracks it open; the window breaks the gems; a slam pass breaks slamGems
                if (slam47) slams47++;
                if (inp.torchBurning) torchPasses++;
                Say("crown37Pass", "pass", null, n, at);
                gemsByPass += Math.Min(n, GemsLeft);
                brain.Stagger(p.passStagger, "pass");
                BreakGems(n, inp.torchBurning ? "torch" : "pass");
                if (!body.alive) return;
            }
            // the leg changes a pass crossed two thresholds of (a 2-gem pass from 6 to 4 gems: leg 2 then leg 3 once the change ends)
            if (brain.state != Boss37State.LegChange) LegCheck();
            // the leg's openers: forced in order from the leg's first rest
            Openers();
            // habits
            airTime = inp.grounded || inp.riding ? 0 : airTime + dt;
            if (airTime >= p.airborneSeconds && brain.Fighting) { brain.Force("lowFire"); airTime = 0; airForces++; airForceStep = stepNo; }
            if (brain.Fighting && orbit.standing >= 0 && orbit.standSteps == S(p.habitStandSeconds))
            {
                Piece37 q = orbit.pieces[orbit.standing];
                if (q.kind == Piece37Kind.Orbit || q.kind == Piece37Kind.Gallery || q.kind == Piece37Kind.Plaza)
                    if (orbit.Mark(orbit.standing, p.habitTellSeconds, q.kind == Piece37Kind.Plaza ? 0 : ReturnOf(q), "habit", events)) habitDrops++;
            }
            Attacks(inp);
            if (crown47) Crown47Attacks(brain.Current, inp); // NIGHT47 L (crown47): CROWN FIRE, BELL TOLL (only in the list while on)
            StepShards(inp);
            Pose();
        }

        Boss37Context Ctx(in Crown37Input inp)
        {
            double flat = JsMath.Hypot(inp.vel.x, inp.vel.z);
            return new Boss37Context
            {
                player = inp.feet, playerVel = inp.vel, boss = centre, grounded = inp.grounded, airborne = !inp.grounded && !inp.riding, riding = inp.riding,
                movingTheWay = !inp.grounded || inp.riding || flat > 4, lineOfSight = true, outOfReach = false, hpFrac = body.maxHp > 0 ? body.hp / body.maxHp : 0, aim = centre,
            };
        }

        double ReturnOf(Piece37 q) => q.kind == Piece37Kind.Orbit ? orbit.p.orbitReturn : q.kind == Piece37Kind.Gallery ? orbit.p.galleryReturn : 0;

        void Openers()
        {
            int leg = brain.leg;
            List<string> list = p.openers[Math.Max(0, Math.Min(2, leg - 1))];
            if (openerLeg != leg) { openerLeg = leg; openerAt = 0; closed = list != null && list.Count > 0; }
            if (closed && (list == null || openerAt >= list.Count)) closed = false; // the line opens as the last opener's wind-up starts (the cut shows which piece is gone)
            if (list == null || openerAt >= list.Count || !brain.Fighting) return;
            if (brain.state == Boss37State.Tell && brain.entered && brain.Current != null && brain.Current.id == list[openerAt]) { openerAt++; if (openerAt >= list.Count) closed = false; return; }
            if (brain.state == Boss37State.Rest) brain.Force(list[openerAt]);
        }

        void Shatter()
        {
            shattered = true;
            for (int i = 0; i < orbit.pieces.Count; i++) if (orbit.pieces[i].kind == Piece37Kind.Summit) orbit.Mark(i, orbit.p.shatterTell, 0, "shatter", events);
            Say("crown37Shatter", "tell");
        }

        void MoveTo(Vec3 want, double speed)
        {
            Vec3 d = want - centre;
            double l = d.Length, stepL = speed * dt;
            centre = l <= stepL ? want : centre + d * (stepL / l);
        }

        void Face(Vec3 chest)
        {
            Vec3 d = chest - centre;
            double l = d.Length;
            if (l > 0.5) normal = d * (1 / l);
        }

        /// <summary>The landing the Crown offers past it (world: the piece's top, or leg 3's plaza point); valid with passTarget >= 0 or in leg 3.</summary>
        public Vec3 landHint;
        public bool HasLanding => !closed && (passTarget >= 0 || brain.leg >= 3);
        /// <summary>The line is shut (no landing offered, the Crown hovers beside the player) until the leg's openers have all begun (crown.openers): the Crown fights back first.</summary>
        public bool closed;

        /// <summary>Where the Crown wants to hover: hoverDistance m (flat) from the player on the line toward the piece it offers as the landing
        /// (targetMin..targetMax m away, in the leg's own pieces: a jump-dash through it lands there), passAbove over his chest (the jump-dash's
        /// rise there). Leg 3: the line along the spire's turn over the plaza.</summary>
        public Vec3 HoverPoint(in Crown37Input inp)
        {
            Vec3 me = inp.chest;
            if (brain.leg >= 3)
            {
                passTarget = -1;
                Vec3 t = Tangent(inp.feet);
                double lx = inp.feet.x + t.x * 14 - axis.x, lz = inp.feet.z + t.z * 14 - axis.z, lr = JsMath.Hypot(lx, lz), want = 11;
                landHint = lr > 1e-6 ? new Vec3(axis.x + lx / lr * want, plazaY, axis.z + lz / lr * want) : new Vec3(axis.x + want, plazaY, axis.z);
            }
            else
            {
                // keep the offered piece while it stays good (no flicker), else the best one: flat distance near the middle of the band, a small drop
                if (!Offerable(passTarget, inp.feet, out _)) passTarget = -1;
                if (passTarget < 0)
                {
                    double best = 1e9, mid = (p.targetMin + p.targetMax) / 2;
                    for (int i = 0; i < orbit.pieces.Count; i++)
                    {
                        if (!Offerable(i, inp.feet, out double h)) continue;
                        double score = Math.Abs(h - mid) + 0.3 * (inp.feet.y - orbit.pieces[i].pos.y);
                        if (score < best) { best = score; passTarget = i; }
                    }
                }
                if (passTarget >= 0)
                {
                    // where the piece will be when a jump-dash (24 m/s: config dash.speed) gets over it: the line the Crown sets the pass on
                    Piece37 q = orbit.pieces[passTarget];
                    double h0 = JsMath.Hypot(q.pos.x - inp.feet.x, q.pos.z - inp.feet.z);
                    landHint = orbit.At(q, orbit.steps + MovementMath.StepsFor(h0 / Math.Max(1, p.dashSpeed) + p.launchLead, dt));
                }
            }
            if ((passTarget < 0 && brain.leg < 3) || closed)
            {
                Vec3 tg = Tangent(inp.feet);
                return new Vec3(inp.feet.x + tg.x * Hover40, me.y + p.passAbove, inp.feet.z + tg.z * Hover40);
            }
            double dx = landHint.x - inp.feet.x, dz = landHint.z - inp.feet.z, dl = Math.Max(1e-6, JsMath.Hypot(dx, dz));
            return new Vec3(inp.feet.x + dx / dl * Hover40, me.y + p.passAbove, inp.feet.z + dz / dl * Hover40);
        }

        bool Offerable(int i, Vec3 feet, out double h)
        {
            h = 0;
            if (i < 0 || i >= orbit.pieces.Count || i == orbit.standing) return false;
            Piece37 q = orbit.pieces[i];
            if (q.state != Piece37State.Up || q.leg != brain.leg || q.kind == Piece37Kind.Summit || q.kind == Piece37Kind.Fall || q.kind == Piece37Kind.Plaza) return false;
            double drop = feet.y - q.pos.y;
            h = JsMath.Hypot(q.pos.x - feet.x, q.pos.z - feet.z);
            if (!(drop >= -0.5 && drop <= 12 && h >= p.targetMin && h <= p.targetMax)) return false;
            return !CrossesCore(feet, q.pos); // never a line through the spire's core
        }

        /// <summary>Does the flat segment a -> b cross the core's square (its half width + 0.8 m)?</summary>
        bool CrossesCore(Vec3 a, Vec3 b)
        {
            double h = orbitCoreHalf + 0.8, x0 = axis.x - h, x1 = axis.x + h, z0 = axis.z - h, z1 = axis.z + h;
            if (a.x >= x0 && a.x <= x1 && a.z >= z0 && a.z <= z1) return false; // from the core's top: off it, any way
            double t0 = 0, t1 = 1, dx = b.x - a.x, dz = b.z - a.z;
            if (!Clip(-dx, a.x - x0, ref t0, ref t1) || !Clip(dx, x1 - a.x, ref t0, ref t1) || !Clip(-dz, a.z - z0, ref t0, ref t1) || !Clip(dz, z1 - a.z, ref t0, ref t1)) return false;
            return t0 <= t1;
        }

        static bool Clip(double p, double q, ref double t0, ref double t1)
        {
            if (Math.Abs(p) < 1e-12) return q >= 0;
            double r = q / p;
            if (p < 0) { if (r > t1) return false; if (r > t0) t0 = r; }
            else { if (r < t0) return false; if (r < t1) t1 = r; }
            return true;
        }

        /// <summary>The core's half width (the spire row's coreHalf; the sim sets it).</summary>
        public double orbitCoreHalf = 4;

        Vec3 Tangent(Vec3 feet)
        {
            double rx = feet.x - axis.x, rz = feet.z - axis.z, l = JsMath.Hypot(rx, rz);
            if (l < 1e-6) return new Vec3(1, 0, 0);
            return new Vec3(-rz / l, 0, rx / l);
        }

        // ---- the attacks ----------------------------------------------------------------------------------------------------------------------------

        void Attacks(in Crown37Input inp)
        {
            Boss37Attack a = brain.Current;
            if (brain.state == Boss37State.Tell && brain.entered && a != null)
            {
                switch (a.id)
                {
                    case "bladeRing":
                        cutY = inp.feet.y + 0.9;
                        cutR = cutPrevR = 0;
                        Say("crown37Cut", "tell", a.id);
                        break;
                    case "beamCut":
                    {
                        int i = BeamTarget(inp);
                        if (i >= 0 && orbit.Mark(i, a.windupSteps * dt, ReturnOf(orbit.pieces[i]), "cut", events)) { cuts++; legCuts[LegIx]++; Say("crown37Cut", "beam", a.id, 0, orbit.pieces[i].pos); }
                        break;
                    }
                    case "plazaDrop":
                    {
                        int best = -1;
                        double bd = 1e9;
                        for (int i = 0; i < orbit.pieces.Count; i++)
                        {
                            Piece37 q = orbit.pieces[i];
                            if (q.kind != Piece37Kind.Plaza || q.state != Piece37State.Up) continue;
                            double d = JsMath.Hypot(q.pos.x - inp.feet.x, q.pos.z - inp.feet.z);
                            if (d < bd) { bd = d; best = i; }
                        }
                        if (best >= 0 && orbit.Mark(best, a.windupSteps * dt, 0, "plaza", events)) { cuts++; legCuts[LegIx]++; Say("crown37Cut", "plaza", a.id, 0, orbit.pieces[best].pos); }
                        break;
                    }
                }
            }
            if (brain.state == Boss37State.Active && brain.entered && a != null)
            {
                switch (a.id)
                {
                    case "bladeRing":
                        cutR = cutPrevR = 4.5;
                        cutHit = false;
                        legBlades[LegIx]++;
                        Say("crown37Cut", "ring", a.id, 0, new Vec3(axis.x, cutY, axis.z));
                        break;
                    case "gemShard":
                        Fire(inp, false);
                        if (crown47 && brain.leg >= 3) Volley47(inp); // NIGHT47 L (crown47): SHARD VOLLEY in phase 3 (Crown37.N47.cs)
                        break;
                    case "lowFire":
                        Fire(inp, true);
                        break;
                }
            }
            // the ring cut sweeps out from the core during its active steps
            if (cutR > 0)
            {
                if (brain.state != Boss37State.Active || a == null || a.id != "bladeRing") { cutR = 0; return; }
                cutPrevR = cutR;
                cutR = 4.5 + (p.bladeReach - 4.5) * brain.Progress;
                double rr = JsMath.Hypot(inp.feet.x - axis.x, inp.feet.z - axis.z);
                bool overlap = inp.feet.y < cutY + p.bladeHalfThick && inp.feet.y + 1.8 > cutY - p.bladeHalfThick;
                if (!cutHit && overlap && rr >= cutPrevR - 0.5 && rr <= cutR + 0.5)
                {
                    cutHit = true;
                    bladeHits++;
                    legBladeHits[LegIx]++;
                    Hurt(p.bladeDamage);
                    Say("crown37Cut", "hit", a.id, 0, inp.feet);
                }
            }
        }

        /// <summary>The piece the beam cuts: the pass target if any, else the solid piece nearest where the player will be in 1 s (not the one
        /// he stands on, never the summit or a stone).</summary>
        int BeamTarget(in Crown37Input inp)
        {
            if (passTarget >= 0 && orbit.pieces[passTarget].state == Piece37State.Up) return passTarget;
            Vec3 ahead = new Vec3(inp.feet.x + inp.vel.x, inp.feet.y + Math.Min(0, inp.vel.y), inp.feet.z + inp.vel.z);
            int best = -1;
            double bd = 18;
            for (int i = 0; i < orbit.pieces.Count; i++)
            {
                Piece37 q = orbit.pieces[i];
                if (q.state != Piece37State.Up || i == orbit.standing || q.kind == Piece37Kind.Summit || q.kind == Piece37Kind.Fall) continue;
                if (q.leg != brain.leg) continue;
                double d = Vec3.DistanceTo(q.pos, ahead);
                if (d < bd) { bd = d; best = i; }
            }
            return best;
        }

        void Fire(in Crown37Input inp, bool low)
        {
            Vec3 aim = low ? new Vec3(inp.feet.x, inp.feet.y - p.lowDrop, inp.feet.z) : inp.chest;
            double speed = low ? p.lowSpeed : p.shardSpeed;
            double tFly = Vec3.DistanceTo(centre, aim) / Math.Max(1e-6, speed);
            Vec3 led = new Vec3(aim.x + inp.vel.x * tFly, aim.y + (low ? Math.Min(0, inp.vel.y) * tFly : 0), aim.z + inp.vel.z * tFly);
            Vec3 d = Vec3.Normalize(led - centre);
            shards.Add(new Crown37Shard { pos = centre, prev = centre, vel = d * speed, life = S(3), alive = true, red = low });
            Say("crown37Shard", "fire", low ? "lowFire" : "gemShard");
        }

        void StepShards(in Crown37Input inp)
        {
            for (int i = shards.Count - 1; i >= 0; i--)
            {
                Crown37Shard s = shards[i];
                s.prev = s.pos;
                s.pos = s.pos + s.vel * dt;
                if (--s.life <= 0 || !s.alive) { shards.RemoveAt(i); continue; }
                if (s.back)
                {
                    if (Vec3.DistanceTo(s.pos, centre) < p.ringRadius) shards.RemoveAt(i); // home: its gem already shattered at the parry
                    continue;
                }
                double r = s.red ? p.lowRadius : p.shardRadius;
                if (Vec3.DistanceTo(s.pos, inp.chest) <= r + 0.45 || Vec3.DistanceTo(s.pos, inp.feet + new Vec3(0, 0.3, 0)) <= r + 0.35)
                {
                    if (s.red) lowHits++; else shardHits++;
                    Hurt(s.red ? p.lowDamage : p.shardDamage);
                    Say("crown37Shard", "hit", s.red ? "lowFire" : "gemShard", 0, s.pos);
                    shards.RemoveAt(i);
                }
            }
        }

        void Hurt(double amount)
        {
            Health h = target.health;
            if (h != null && !h.Dead) h.Damage(amount, "boss");
        }
    }
}
