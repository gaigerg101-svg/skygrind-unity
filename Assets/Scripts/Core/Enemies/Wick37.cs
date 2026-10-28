// NIGHT37 builder W (wick37; NIGHT37.md W1, Research/2026-10-12-wave37/BOSSES.md "L1 THE WICK"): THE WICK, level 1's boss, on K's Boss37 brain
// (Core/Boss37.cs), fought up THE STACK (Wick37Stack: a 40 m furnace shaft climbed on a spiral of ride panels away from rising slag).
//   THE BODY     a cage of `plates` iron plates round a flame core, floating on the shaft's axis at the player's height + aboveP (legs 1-2), circling
//                the cap's catwalk in leg 3. The cage turns slowly; a CRACKED plate is a gap in it for crackSeconds.
//   DAMAGE       (BOSSES.md rule 2: movement is the only way): a CHARGED KESTREL shot fired FROM A WALL RIDE (a ride within rideGrace) whose line
//                enters the cage through a cracked plate's gap hits the core in full (the brain's right move: it drains `pierceStagger`);
//                anything else is the brain's chip (design.n37.boss37.chipScale); the CYAN window (stagger empty) takes everything in full.
//                A parried ember is full and cracks the plate it returns into.
//   ATTACKS      (Boss37's pick; one tell at a time):
//                EMBER   YELLOW: `emberCount` slow globs at where the player will be. PARRY one back into the cage: it cracks a plate.
//                FLARE   RED: a ring of fire wipes the half of the shaft wall the player is on (centred on his angle, a height band round him):
//                        leap across to the other half.
//                SLAM    RED (camp: a player on one spot of a ledge > the brain's campSeconds): a plate drops on that spot.
//                DIVE    RED (leg 3, the cap): it dives at the player across the catwalk, then STALLS at the wall (the dive's tail): a launch
//                        punch (a punch while dashing / fast) knocks it into the shaft: launchDamage, and below finishAt it falls into the slag.
//                DRAFT   (no damage) at every brain rest of a burst: it drifts up past the player and the slag jumps draftRise.
//   THE SLAG     (Wick37Slag) rises while the brain attacks (TELL / ACTIVE / RECOVER), still in REST / WINDOW / LEGCHANGE.
//   LEGS         places (autoLegs off): leg 1 from the floor (checkpoint ledge 1), leg 2 from ledge 2 (its checkpoint), leg 3 on the cap. The bar
//                is locked at legLock[leg] until the leg ends (the climb paces the fight; the bar never passes the leg).
// Fixed 120 Hz, whole steps, Mulberry32 via the brain, JsMath, no audio (events: boss37* from the brain, wick37Ember / wick37Flare /
// wick37Slam / wick37Dive / wick37Crack / wick37Draft / wick37Launch), no UnityEngine.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Wick37Params
    {
        public string name;
        public double pool, cageR, coreR, cageHeight, aboveP, followRate, revealSeconds, riseFrom, dieSeconds;
        public int plates;
        public double turnDegPerS, crackSeconds, chargedMin, rideGrace, pierceStagger, parryStagger, parryReach, parryConeDeg, parryBuffer;
        public double emberHome;
        public bool anyShotFromRide;
        public double emberSpeed, emberRadius, emberDamage, emberLife, emberSpreadDeg, emberLeadMax, returnSpeed, parryDamage;
        public int emberCount;
        public double flareDamage, flareBand, flareBelow, flareAbove, flareHalfDeg;
        public double slamDamage, slamRadius;
        public double diveSpeed, diveDamage, diveReach, circleR, circleY, circleDegPerS, launchReach, launchDamage, launchSpeed, finishAt;
        public double[] legLock;
        public double capsuleHeight, capsuleRadius;
        public double restartCost;

        public static Wick37Params From(GameConfig c)
        {
            const string g = "design.n37.wick37.boss.";
            double N(string k) => c.Num(g + k);
            return new Wick37Params
            {
                name = c.Str(g + "name"), pool = N("pool"), cageR = N("cageR"), coreR = N("coreR"), cageHeight = N("cageHeight"), aboveP = N("aboveP"),
                followRate = N("followRate"), revealSeconds = N("revealSeconds"), riseFrom = N("riseFrom"), dieSeconds = N("dieSeconds"), plates = c.Int(g + "plates"),
                turnDegPerS = N("turnDegPerS"), crackSeconds = N("crackSeconds"), chargedMin = N("chargedMin"), rideGrace = N("rideGrace"),
                pierceStagger = N("pierceStagger"), parryStagger = N("parryStagger"), parryReach = N("parryReach"), parryConeDeg = N("parryConeDeg"), parryBuffer = N("parryBuffer"),
                emberSpeed = N("ember.speed"), emberRadius = N("ember.radius"), emberDamage = N("ember.damage"), emberLife = N("ember.life"),
                emberSpreadDeg = N("ember.spreadDeg"), emberLeadMax = N("ember.leadMax"), emberHome = N("ember.homeDegPerS"), anyShotFromRide = c.Design("n37.wick37.boss.anyShotFromRide"), returnSpeed = N("ember.returnSpeed"), parryDamage = N("ember.parryDamage"),
                emberCount = c.Int(g + "ember.count"),
                flareDamage = N("flare.damage"), flareBand = N("flare.wallBand"), flareBelow = N("flare.below"), flareAbove = N("flare.above"), flareHalfDeg = N("flare.halfDeg"),
                slamDamage = N("slam.damage"), slamRadius = N("slam.radius"),
                diveSpeed = N("dive.speed"), diveDamage = N("dive.damage"), diveReach = N("dive.reach"), circleR = N("dive.circleR"), circleY = N("dive.circleY"),
                circleDegPerS = N("dive.circleDegPerS"), launchReach = N("dive.launchReach"), launchDamage = N("dive.launchDamage"), launchSpeed = N("dive.launchSpeed"),
                finishAt = N("dive.finishAt"),
                legLock = c.Nums(g + "legLock"), restartCost = N("restartCost"),
                capsuleHeight = c.Num("body.capsuleHeight"), capsuleRadius = c.Num("body.capsuleRadius"),
            };
        }
    }

    public sealed class Wick37Body : Enemy, IBossBody27
    {
        public readonly Wick37 system;

        public Wick37Body(Wick37 system)
        {
            this.system = system;
            kind = "wick";
        }

        public override double Height => system.p.cageHeight;
        public override double Radius => system.p.cageR;
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
        public double Filter(double amount, string weapon, Vec3 from, out bool blocked) => system.Filter(amount, weapon, from, out blocked);
        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt) { }
        public override void Stagger(double time, double dt) { }
        public override void Stun(double time, double dt) { }
    }

    public sealed class Wick37Ember
    {
        public Vec3 pos, prev, vel;
        public int life;
        public bool alive, back;
        public bool ride47; // NIGHT47 W (wickRideParry47): parried from a ride: it cracks two plates
    }

    /// <summary>The payload of wick37Ember / wick37Flare / wick37Slam / wick37Dive / wick37Crack / wick37Draft / wick37Launch (pooled).</summary>
    public sealed class Wick37Fx
    {
        public string stage;
        public int plate;
        public double angleDeg, y;
        public Vec3 at;
    }

    public sealed partial class Wick37 : IBossSystem // NIGHT43 B1: partial (wick43 lives in Wick37.N43.cs)
    {
        public const string Ember = "wickEmber", Flare = "wickFlare", Slam = "wickSlam", Dive = "wickDive";
        public readonly Wick37Params p;
        public readonly Wick37Body body;
        public Boss37Brain brain;
        readonly GameConfig cfg;
        readonly EventBus events;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt0;
        readonly BossEvent said = new BossEvent();
        readonly Wick37Fx fx = new Wick37Fx();
        readonly HitEvent parried = new HitEvent();
        BossKit kit;
        BossScale scale = BossScale.One;

        public Vec3 origin;
        public Wick37Stack stack;
        public Wick37Slag slag;
        public Tells36 tells;
        public Func<bool> playerRiding, playerGrounded;
        public Func<Capsule> playerBody;
        public Func<Vec3> playerVel;
        /// <summary>A returned ember reached the cage: the sim lands `damage` as a parry hit (weapon parry31: Filter takes it in full).</summary>
        public Action<double, Vec3> parryHit;
        public readonly List<Wick37Ember> embers = new List<Wick37Ember>();

        /// <summary>The core's centre, arena-local.</summary>
        public Vec3 local;
        public double cageTurn; // radians
        public readonly int[] crackLeft = new int[12];
        public int stepNo, lastRideStep = -100000, lastPunchStep = -100000;
        bool punchUsed, punchLaunch;
        Vec3 punchEye, punchDir;
        public double flareAngle; // radians, arena-local atan2(z, x): the burning half's centre
        public bool flareLive, stalled;
        int flareHitCool;
        Vec3 diveFrom, diveTo;
        bool diveHitDone;
        public double circleAngle;
        public bool falling;
        public double hpLock;

        // stats for the harness and the reels
        public int emberHits, parries, cracks, pierces, chips, fulls, flareHits, slamHits, diveHits, launches, drafts, slagTouches, restarts;
        public double dealt, dealtFull, dealtRight, lastStallPunchDist;
        public int punchesSeen, stallPunches, stallLaunchPunches, stalls;

        public Wick37(GameConfig cfg, Wick37Params p, EventBus events, BossTarget target, Func<int> nextId)
        {
            this.cfg = cfg;
            this.p = p;
            this.events = events;
            this.target = target;
            this.nextId = nextId;
            dt0 = cfg.Dt;
            body = new Wick37Body(this);
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;
        public Vec3 Centre => origin + local;
        public int Plates => Math.Max(1, Math.Min(12, p.plates));
        public string AttackId => brain?.Current?.id;
        /// <summary>The slag rises now (the brain attacks).</summary>
        public bool SlagRising => brain != null && (brain.state == Boss37State.Tell || brain.state == Boss37State.Active || brain.state == Boss37State.Recover);
        public uint seed = 37;

        public void StartFight(BossFight ctx)
        {
            kit = ctx.kit;
            scale = ctx.scale;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(Pool54 * Math.Max(0.01, scale.hp)); // NIGHT54 W (wick54; Wick37.N54.cs): the row's pool (p.pool with it off)
            body.hp = body.maxHp;
            stepNo = 0;
            embers.Clear();
            Array.Clear(crackLeft, 0, crackLeft.Length);
            lastRideStep = lastPunchStep = -100000;
            punchUsed = true;
            flareLive = stalled = falling = false;
            emberHits = parries = cracks = pierces = chips = fulls = flareHits = slamHits = diveHits = launches = drafts = slagTouches = restarts = 0;
            dealt = dealtFull = dealtRight = 0;
            brain = new Boss37Brain(Boss37Params.From(cfg, "wick37"), "wick", dt0, events, seed) { tells = tells, source = body.id };
            brain.p.autoLegs = false;
            foreach (string id in new[] { "ember", "flare", "slam", "dive" })
                brain.attacks.Add(Boss37Attack.From(cfg, "design.n37.wick37.attacks." + id, dt0));
            Vec3 pw = target.position();
            for (int leg = 1; leg <= 3; leg++)
            {
                double y = leg == 1 ? stack.p.ledges[0] : leg == 2 ? stack.p.ledges[Math.Min(1, stack.p.ledges.Length - 1)] : stack.p.capY;
                double rr = leg == 3 ? (stack.p.radius + stack.p.capRadius) / 2 : stack.p.radius - stack.p.ledgeWidth * 0.5;
                double ca = leg == 3 ? Math.PI / 2 : stack.ledgeAngle[Math.Min(leg - 1, stack.ledgeAngle.Length - 1)];
                brain.SetCheckpoint(leg, origin + new Vec3(JsMath.Cos(ca) * rr, y, JsMath.Sin(ca) * rr), 0); // on the leg's ledge arc / the catwalk
            }
            hpLock = p.legLock.Length > 0 ? p.legLock[0] : 0;
            local = new Vec3(0, p.riseFrom, 0);
            brain.Begin(S(p.revealSeconds), pw);
            body.position = Centre;
            body.previous = body.position;
            said.Reset("wick");
            said.name = p.name;
            said.hp = body.maxHp;
            events.Emit("bossRise", said);
            if (!restHooked) { restHooked = true; events.On<Boss37Event>(Boss37Brain.EvRest, OnRest); }
            StartFight43(); // NIGHT43 B1 (wick43; Wick37.N43.cs): nothing with the row off
            StartFight47(); // NIGHT47 W (wick47; Wick37.N47.cs): the phases' kit and gate (nothing with the row off)
            StartFight54(); // NIGHT54 W (wick54; Wick37.N54.cs): the FLARE out of the kit, the encounter's numbers (nothing with the row off)
            StartFight55(); // NIGHT55 B (wick55; Wick37.N55.cs): the window, the meter and the chip (nothing with the row off)
        }

        bool restHooked;
        void OnRest(Boss37Event e)
        {
            if (e.boss != "wick" || !body.alive || brain == null) return;
            if (e.stage != "burst" && e.stage != "budget") return;
            // the DRAFT: at rest it drifts up past you and the slag jumps
            drafts++;
            slag?.Draft();
            fx.stage = "draft";
            fx.at = Centre;
            fx.y = slag != null ? slag.y : 0;
            events.Emit("wick37Draft", fx);
        }

        public void Clear()
        {
            body.alive = false;
            embers.Clear();
            flareLive = stalled = false;
            if (brain != null) { brain.tells?.End(brain.source); brain.Reset(); }
        }

        public BossBar Bar() => new BossBar
        {
            valid = body.alive, id = body.id, name = p.name, phase = brain != null ? brain.state.ToString() : "", hp = Math.Max(0, body.hp), max = body.maxHp,
            segment = hpLock, pips = Plates, pipsOn = CrackedCount,
        };

        public int CrackedCount
        {
            get { int n = 0; for (int i = 0; i < Plates; i++) if (crackLeft[i] > 0) n++; return n; }
        }

        /// <summary>The plate facing arena-local direction angle `a` (radians) now.</summary>
        public int PlateAt(double a)
        {
            double sec = 2 * Math.PI / Plates, x = a - cageTurn;
            x %= 2 * Math.PI;
            if (x < 0) x += 2 * Math.PI;
            int i = (int)Math.Floor(x / sec);
            return i < 0 ? 0 : i >= Plates ? Plates - 1 : i;
        }

        /// <summary>The ray's distance to the cage's sphere (-1: a miss).</summary>
        public double Raycast(Vec3 o, Vec3 d)
        {
            if (!body.alive || brain == null || brain.state == Boss37State.Dead) return -1;
            Vec3 c = Centre, oc = o - c;
            double b = Vec3.Dot(oc, d), cc = Vec3.Dot(oc, oc) - p.cageR * p.cageR, disc = b * b - cc;
            if (disc < 0) return -1;
            double t = -b - Math.Sqrt(disc);
            if (t < 0) t = -b + Math.Sqrt(disc);
            return t >= 0 ? t : -1;
        }

        bool RidingNow => stepNo - lastRideStep <= S(p.rideGrace);

        /// <summary>The share that lands. `from` is the shot's origin (the gap test uses the direction the shot comes in from).</summary>
        public double Filter(double amount, string weapon, Vec3 from, out bool blocked)
        {
            blocked = false;
            if (brain == null || !body.alive || !brain.Fighting) { blocked = true; return 0; }
            if (weapon == "parry31" || weapon == "parry36") { fulls++; return amount; }
            if (brain.Open) { fulls++; return Window55(amount * brain.Scale(true)); } // NIGHT55 B (wick55): x windowGain inside the window (unchanged off)
            if (w47 && Filter47(amount, weapon, from, out double share47)) return share47; // NIGHT47 W (wick47): the bare core, the floor crack, the chip
            bool charged = weapon == "revolver" && (amount >= p.chargedMin || p.anyShotFromRide);
            Vec3 c = Centre;
            int plate = PlateAt(JsMath.Atan2(from.z - c.z, from.x - c.x));
            if (charged && RidingNow && crackLeft[plate] > 0)
            {
                pierces++;
                fulls++;
                dealtRight += amount;
                crackLeft[plate] = 0; // the shot through the gap spends it
                brain.Stagger(amount >= p.chargedMin ? p.pierceStagger * 2 : p.pierceStagger, "pierce");
                fx.stage = "pierce";
                fx.plate = plate;
                fx.at = c;
                events.Emit("wick37Crack", fx);
                return amount * brain.Scale(true);
            }
            chips++;
            return amount * brain.Scale(false);
        }

        public void Apply(double dmg)
        {
            if (!body.alive) return;
            double floor = body.maxHp * hpLock;
            if (brain != null && brain.state == Boss37State.Dying) return;
            double before = body.hp;
            body.hp = Math.Max(floor, body.hp - dmg);
            double d = before - body.hp;
            dealt += d;
            body.flash = 0.06;
            said.Reset("wick");
            said.damage = JsMath.Round(d);
            said.part = brain != null && brain.Open ? "core" : "cage";
            said.core = brain != null && brain.Open;
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            if (body.hp <= 0) Kill();
        }

        void Kill()
        {
            if (brain == null || brain.state == Boss37State.Dying || brain.state == Boss37State.Dead) return;
            body.hp = 0;
            embers.Clear();
            flareLive = false;
            falling = true;
            brain.Die(S(p.dieSeconds));
            kit?.Dying("wick", Centre);
        }

        /// <summary>A punch from `eye` along `dir`: the nearest live ember in reach goes back (the parry, with an early buffer); in the dive's STALL
        /// a launch punch (`launch`: the player dashes or is fast) within launchReach of the core knocks THE WICK into the shaft.</summary>
        public bool Punch(Vec3 eye, Vec3 dir, bool launch)
        {
            lastPunchStep = stepNo;
            punchUsed = false;
            punchLaunch = launch;
            punchesSeen++;
            if (stalled) { stallPunches++; if (launch) stallLaunchPunches++; lastStallPunchDist = Vec3.DistanceTo(eye, Centre); }
            if (w47 && CorePunch47(eye)) { punchUsed = true; return true; } // NIGHT47 W (wick47): the open core's punch
            if (TryLaunch(eye, launch)) { punchUsed = true; return true; }
            return TryParry(eye, dir);
        }

        /// <summary>The dash INTO the stalled cage (the sim calls it while the player dashes or just dashed): the launch, as the dash-punch.</summary>
        public bool LaunchContact(Vec3 eye) { if (stalled) stallDashes++; return TryLaunch(eye, true); }
        public int stallDashes;

        bool TryLaunch(Vec3 eye, bool launch)
        {
            if (!launch || !stalled || !body.alive) return false;
            if (Vec3.DistanceTo(eye, Centre) > p.launchReach + p.cageR) return false;
            launches++;
            stalled = false;
            Vec3 c = Centre;
            fx.stage = "launch";
            fx.at = c;
            events.Emit("wick37Launch", fx);
            // knocked off the wall over the shaft: it falls toward the axis
            diveFrom = local;
            diveTo = new Vec3(0, local.y - 6, 0);
            double dmg = p.launchDamage * body.maxHp, before = body.hp;
            if (body.hp - dmg <= body.maxHp * p.finishAt)
            {
                // the end: into the shaft and the slag
                dealtRight += body.hp;
                dealt += body.hp;
                said.Reset("wick");
                said.damage = JsMath.Round(body.hp);
                said.hp = 0;
                events.Emit("bossHit", said);
                Kill();
                return true;
            }
            Apply(dmg);
            dealtRight += before - body.hp;
            brain.Stagger(p.pierceStagger, "launch");
            return true;
        }

        bool TryParry(Vec3 eye, Vec3 dir)
        {
            punchEye = eye;
            punchDir = dir;
            if (!body.alive) return false;
            double cosCone = JsMath.Cos(p.parryConeDeg * Math.PI / 180), best = p.parryReach + p.emberRadius;
            int idx = -1;
            for (int i = 0; i < embers.Count; i++)
            {
                Wick37Ember o = embers[i];
                if (!o.alive || o.back) continue;
                Vec3 to = o.pos - eye;
                double d = to.Length;
                if (d > best) continue;
                if (d > 0.5 && Vec3.Dot(to, dir) / d < cosCone) continue;
                best = d;
                idx = i;
            }
            if (idx < 0) return false;
            Wick37Ember ob = embers[idx];
            ob.back = true;
            ob.life = S(p.emberLife);
            Vec3 home = Centre - ob.pos;
            double len = Math.Max(1e-6, home.Length);
            ob.vel = home * (p.returnSpeed / len);
            if (w47 && rideParry47 && RidingNow) RideParry47(ob); // NIGHT47 W (wickRideParry47; Wick37.N47.cs): the ride + parry combine
            parries++;
            punchUsed = true;
            parried.owner = "player";
            parried.weapon = "punch";
            parried.kind = Ember;
            parried.enemy = body.id;
            parried.damage = p.parryDamage;
            parried.position = ob.pos;
            events.Emit("parry36", parried);
            if (AttackId == null || AttackId == Ember) brain.Parried(p.parryStagger); // a parried volley refunds and breaks the volley
            else brain.Stagger(p.parryStagger, "parry"); // an ember still in the air while another attack winds up: the drain only (the dive is not broken by it)
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
            return h.Damage(amount * scale.damage, "boss");
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

        /// <summary>A fall / death retry (D-6): the leg's checkpoint (the brain's), the slag pulled back under it; the health cost (restartCost).</summary>
        public Vec3 RestartLeg(Health h)
        {
            if (brain == null) return origin;
            restarts++;
            Vec3 at = brain.RestartLeg();
            embers.Clear();
            flareLive = stalled = false;
            if (slag != null) slag.y = Math.Min(slag.y, at.y - origin.y - slag.p.belowLedge);
            if (h != null && !h.Dead) h.Damage(RestartCost54, "boss"); // NIGHT54 W (wick54): the row's cost (p.restartCost with it off)
            return at;
        }

        public void SlagTouched() => slagTouches++;

        /// <summary>The sim's leg changes (places): the player stood on ledge 2 / the cap.</summary>
        public void ReachedFloor(double y)
        {
            if (w47) { ReachedFloor47(y); return; } // NIGHT47 W (wick47): the legs are phases
            if (brain == null || !body.alive) return;
            int want = y >= stack.p.capY - 0.1 ? 3 : stack.p.ledges.Length > 1 && y >= stack.p.ledges[1] - 0.1 ? 2 : 1;
            while (brain.leg < want && brain.leg < 3)
            {
                int before = brain.leg;
                brain.NextLeg();
                if (brain.leg == before) break;
                hpLock = p.legLock.Length >= brain.leg ? p.legLock[brain.leg - 1] : 0;
                if (brain.leg == 3) circleAngle = JsMath.Atan2(target.position().z - origin.z, target.position().x - origin.x) + Math.PI;
            }
        }

        void FireEmbers(Vec3 pw)
        {
            Vec3 c = Centre;
            Capsule cap = PlayerCapsule();
            Vec3 aim = (cap.start + cap.end) * 0.5;
            Vec3 v0 = playerVel != null ? playerVel() : default;
            double tf = Math.Min(p.emberLeadMax, (aim - c).Length / p.emberSpeed);
            Vec3 at = aim + new Vec3(v0.x, 0, v0.z) * tf;
            Vec3 d0 = at - c;
            double len = Math.Max(1e-6, d0.Length);
            double baseYaw = JsMath.Atan2(d0.z, d0.x), pitch = JsMath.Asin(Math.Max(-1, Math.Min(1, d0.y / len)));
            int n = Math.Max(1, p.emberCount);
            for (int i = 0; i < n; i++)
            {
                double yaw = baseYaw + (i - (n - 1) * 0.5) * p.emberSpreadDeg * Math.PI / 180;
                double cp = JsMath.Cos(pitch);
                var v = new Vec3(JsMath.Cos(yaw) * cp, JsMath.Sin(pitch), JsMath.Sin(yaw) * cp) * p.emberSpeed;
                embers.Add(new Wick37Ember { pos = c, prev = c, vel = v, life = S(p.emberLife), alive = true });
            }
            fx.stage = "fire";
            fx.at = c;
            events.Emit("wick37Ember", fx);
        }

        void StepEmbers(double dt)
        {
            Capsule cap = PlayerCapsule();
            double lim = stack != null ? stack.p.capRadius + 1 : 20;
            for (int i = embers.Count - 1; i >= 0; i--)
            {
                if (i >= embers.Count) continue;
                Wick37Ember o = embers[i];
                o.prev = o.pos;
                if (o.back)
                {
                    Vec3 home = Centre - o.pos;
                    double len = home.Length;
                    if (len <= p.cageR + p.emberRadius || len <= p.returnSpeed * dt)
                    {
                        embers.RemoveAt(i);
                        int plate = PlateAt(JsMath.Atan2(o.pos.z - Centre.z, o.pos.x - Centre.x));
                        crackLeft[plate] = S(p.crackSeconds);
                        if (o.ride47) RideCrack47(plate); // NIGHT47 W (wickRideParry47): the next plate cracks too
                        cracks++;
                        fx.stage = "crack";
                        fx.plate = plate;
                        fx.at = o.pos;
                        events.Emit("wick37Crack", fx);
                        parryHit?.Invoke(p.parryDamage, o.pos);
                        continue;
                    }
                    o.vel = home * (p.returnSpeed / len);
                }
                if (!o.back && p.emberHome > 0)
                {
                    // the glob drifts toward the climber (at most emberHome deg/s): a slow homing, so a moving rider meets it (parry or dodge)
                    Vec3 aim = (cap.start + cap.end) * 0.5 - o.pos;
                    double al = aim.Length, vl = o.vel.Length;
                    if (al > 1e-6 && vl > 1e-6)
                    {
                        Vec3 want = aim * (vl / al), dv = want - o.vel;
                        double mx = vl * p.emberHome * Math.PI / 180 * dt, dl = dv.Length;
                        o.vel = dl <= mx ? want : o.vel + dv * (mx / dl);
                        double nl = o.vel.Length;
                        if (nl > 1e-6) o.vel = o.vel * (vl / nl);
                    }
                }
                o.pos = o.pos + o.vel * dt;
                double rx = o.pos.x - origin.x, rz = o.pos.z - origin.z;
                bool dead = --o.life <= 0 || (!o.back && rx * rx + rz * rz > lim * lim);
                if (!dead && !o.back && SegDist(o.pos, cap.start, cap.end) < p.emberRadius + cap.radius)
                {
                    if (HurtPlayer(p.emberDamage)) emberHits++;
                    dead = true;
                }
                if (dead) embers.RemoveAt(i);
            }
        }

        double PlayerAngle(Vec3 pw) => JsMath.Atan2(pw.z - origin.z, pw.x - origin.x);

        void StartTell(Vec3 pw)
        {
            string id = AttackId;
            if (w47 && StartTell47(id, pw)) return; // NIGHT47 W (wick47): the new attacks' tells
            if (id == Flare)
            {
                flareTells54++; // NIGHT54 W: the count (0 with wick54 on)
                flareAngle = PlayerAngle(pw); // the half he is riding: shown red through the tell
                fx.stage = "tell";
                fx.angleDeg = flareAngle * 180 / Math.PI;
                fx.y = pw.y - origin.y;
                events.Emit("wick37Flare", fx);
            }
            else if (id == Slam)
            {
                fx.stage = "tell";
                fx.at = brain.campAnchor;
                events.Emit("wick37Slam", fx);
            }
            else if (id == Dive)
            {
                stalled = false;
                fx.stage = "tell";
                fx.at = Centre;
                events.Emit("wick37Dive", fx);
            }
        }

        void StartActive(Vec3 pw)
        {
            string id = AttackId;
            if (w47 && StartActive47(id, pw)) return; // NIGHT47 W (wick47): the new attacks
            if (id == Ember) { FireEmbers(pw); Volley43Arm(); } // NIGHT43 B1 (wick43): leg 2 arms a second volley (nothing with the row off)
            else if (id == Flare)
            {
                flareBurns54++; // NIGHT54 W: the count (0 with wick54 on)
                flareLive = true;
                flareHitCool = 0;
                fx.stage = "burn";
                fx.angleDeg = flareAngle * 180 / Math.PI;
                events.Emit("wick37Flare", fx);
            }
            else if (id == Slam)
            {
                Vec3 a = brain.campAnchor;
                double dx = pw.x - a.x, dz = pw.z - a.z;
                if (dx * dx + dz * dz <= p.slamRadius * p.slamRadius && Math.Abs(pw.y - a.y) < 2.5 && HurtPlayer(p.slamDamage)) slamHits++;
                fx.stage = "slam";
                fx.at = a;
                events.Emit("wick37Slam", fx);
            }
            else if (id == Dive)
            {
                diveFrom = local;
                Vec3 pl = pw - origin;
                double r = Math.Max(1e-6, JsMath.Hypot(pl.x, pl.z)), wallR = stack.p.capRadius - 1.2;
                diveTo = new Vec3(pl.x / r * wallR, stack.p.capY + 1.4, pl.z / r * wallR); // through where he stood, to the wall
                diveHitDone = false;
                fx.stage = "dive";
                fx.at = Centre;
                events.Emit("wick37Dive", fx);
            }
        }

        void StepFlare(Vec3 pw)
        {
            if (!flareLive) return;
            if (flareHitCool > 0) { flareHitCool--; return; }
            Vec3 l = pw - origin;
            double r = JsMath.Hypot(l.x, l.z), a = JsMath.Atan2(l.z, l.x);
            double y = l.y, fy = Centre.y - origin.y;
            if ((!(w47 && brain.leg != 2) && r < stack.p.radius - p.flareBand) || Math.Abs(Wrap(a - flareAngle)) > FlareHalfDeg43 * Math.PI / 180) return; // NIGHT43 B1 (wick43): the cap flare's half in leg 3
            if (y < fy - p.flareBelow - 4 || y > fy + p.flareAbove) return;
            if (HurtPlayer(p.flareDamage)) flareHits++;
            flareHitCool = brain.Current != null ? brain.Current.activeSteps : S(1);
        }

        public void Step(double dt)
        {
            if (!body.alive || brain == null) return;
            body.previous = body.position;
            if (Frozen) return;
            stepNo++;
            Vec3 pw = target.position();
            bool riding = playerRiding != null && playerRiding();
            bool grounded = playerGrounded != null && playerGrounded();
            if (riding) lastRideStep = stepNo;
            Vec3 pv = playerVel != null ? playerVel() : default;
            Boss37State before = brain.state;
            if (w54) Step54(); // NIGHT54 W (wick54; Wick37.N54.cs): the leg's rest (nothing with the row off)
            var ctx = new Boss37Context
            {
                player = pw, playerVel = pv, boss = Centre, grounded = grounded, airborne = !grounded && !riding, riding = riding,
                movingTheWay = riding || pv.y > 0.5, lineOfSight = true, outOfReach = false, hpFrac = HpFrac, aim = Centre,
            };
            brain.Step(ctx);
            if (brain.entered)
            {
                if (before == Boss37State.Active) { flareLive = false; if (AttackId == Dive || brain.state == Boss37State.Recover) { } }
                if (brain.state == Boss37State.Tell) StartTell(pw);
                else if (brain.state == Boss37State.Active) StartActive(pw);
                else if (brain.state == Boss37State.Recover && before == Boss37State.Active && brain.Current != null && brain.Current.id == Dive) { stalled = true; stalls++; fx.stage = "stall"; fx.at = Centre; events.Emit("wick37Dive", fx); }
                if (brain.state != Boss37State.Active) flareLive = false;
                if (w47 && brain.state == Boss37State.Recover && before == Boss37State.Active) Recover47(); // NIGHT47 W (wick47): the charge cracks open
                if (brain.state == Boss37State.Window || brain.state == Boss37State.Rest || brain.state == Boss37State.LegChange) { if (brain.state != Boss37State.Window) stalled = false; }
            }
            if (brain.state == Boss37State.Dead)
            {
                body.alive = false;
                kit?.Down("wick", body.id, Centre);
                return;
            }
            for (int i = 0; i < Plates; i++) if (crackLeft[i] > 0) crackLeft[i]--;
            {
                // the cage turns to face the player (plate 0's centre toward him, at most turnDegPerS): a crack made by a parry stays on his side
                double want = Wrap(JsMath.Atan2(pw.z - Centre.z, pw.x - Centre.x) - Math.PI / Plates), d = Wrap(want - cageTurn), mx = p.turnDegPerS * Math.PI / 180 * dt;
                cageTurn = Wrap(cageTurn + Math.Max(-mx, Math.Min(mx, d)));
            }
            if (!punchUsed && stepNo - lastPunchStep <= S(p.parryBuffer)) { if (!TryLaunch(punchEye, punchLaunch)) TryParry(punchEye, punchDir); }
            if (brain.Fighting)
            {
                StepEmbers(dt);
                if (brain.state == Boss37State.Active && AttackId == Flare) StepFlare(pw);
            }
            Move(pw, dt);
            if (n43) Step43(pw); // NIGHT43 B1 (wick43; Wick37.N43.cs): the second volley, the leg beat, the gift crack, bossOpen43
            if (w47) Step47(pw, dt); // NIGHT47 W (wick47; Wick37.N47.cs): the phase gate, the ring, the charge's hit, the open clock
            body.position = Centre;
            body.velocity = default;
        }

        void Move(Vec3 pw, double dt)
        {
            Vec3 pl = pw - origin;
            if (falling)
            {
                // the finisher: knocked over the shaft, it drops into the slag
                local = new Vec3(local.x * 0.97, local.y - 9 * dt, local.z * 0.97);
                return;
            }
            if (brain.state == Boss37State.Reveal)
            {
                double t = Math.Min(1, brain.stateSteps * dt / Math.Max(1e-6, p.revealSeconds));
                double want = pl.y + p.aboveP;
                local = new Vec3(0, p.riseFrom + (want - p.riseFrom) * (1 - (1 - t) * (1 - t)), 0);
                return;
            }
            if (Move47(pw, dt)) return; // NIGHT47 W (wick47): P2's height, the charge, the geyser's sink
            if (brain.leg < 3)
            {
                double want = Math.Min(stack.p.capY - 2, Math.Max(slag != null ? slag.y + 2 : 1, pl.y + p.aboveP));
                if (brain.state == Boss37State.Rest) want += 2; // the draft: it drifts up past you
                double stp = p.followRate * dt, dy = want - local.y;
                local = new Vec3(local.x * 0.95, local.y + Math.Max(-stp, Math.Min(stp, dy)), local.z * 0.95);
                return;
            }
            // leg 3: circling the cap; the dive and the stall
            if (brain.state == Boss37State.Active && AttackId == Dive)
            {
                Vec3 d = diveTo - local;
                double len = d.Length, stp = p.diveSpeed * dt;
                local = len <= stp ? diveTo : local + d * (stp / len);
                Capsule cap = PlayerCapsule();
                if (!diveHitDone && SegDist(Centre, cap.start, cap.end) < p.diveReach + cap.radius) { diveHitDone = true; if (HurtPlayer(p.diveDamage)) diveHits++; }
                return;
            }
            if (stalled) return; // stuck at the wall: the last window
            circleAngle = Wrap(circleAngle + p.circleDegPerS * Math.PI / 180 * dt);
            var aimL = new Vec3(JsMath.Cos(circleAngle) * p.circleR, stack.p.capY + p.circleY, JsMath.Sin(circleAngle) * p.circleR);
            Vec3 dd = aimL - local;
            double l2 = dd.Length, s2 = p.diveSpeed * 0.5 * dt;
            local = l2 <= s2 ? aimL : local + dd * (s2 / l2);
        }
    }
}
