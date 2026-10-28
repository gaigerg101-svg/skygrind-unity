// NIGHT30 builder B (span; DESIGN30 C4 / D; GG 2026-10-09: "every level should revolve around a mechanic and a weapon with a fun, unique boss at the end that's inspired by that level's
// design"). THE LOOM, LEVEL 4's boss: a blocky crane-spider hanging UNDER the span's deck (room s8), four legs gripping the deck's corners (the plan's loomLegs), each leg with two JOINTS
// that are GRAPPLE NODES (the sim adds them to the level's grappleNodes for the fight; THE ARCLINE hooks them as any node). Built from boxes in the web's blocky style, as THE LANTERN was.
//   THE RULE   a shot (the NAILGUN's job; any gun) on a JOINT lands in full while you hang on that leg's line (the rope's anchor within lineReach of the leg) or stand within fullReach of
//              the joint; lightScale anywhere else, and on the body. JointFull(j) is the view's cyan glow (the joint that takes full damage now).
//   CUT        (its first attack; SWEEP and FLYERS are the cut list) every cutEverySeconds, while you hang on a rope: a cutTellSeconds red tell on your line (event loomCut "tell", the
//              anchor), then the line is plucked: you DROP (the sim's cutLine: the rope released, no boost; grab another node). Let go before the tell ends and nothing happens.
// Fixed 120 Hz, whole steps, no randomness (none needed yet), JsMath, no audio, no UnityEngine. The fight is in the level itself (no arena of its own: the room is builder L's).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.n30.span.loom (every number Unity-only; each has its _why).</summary>
    public sealed class Loom30Params
    {
        public string name = "THE LOOM";
        public double pool, bodyDrop, bodyHalfW, bodyHalfH, kneeOut, kneeUp, hipOut, hipDrop, jointRadius, legHalf, fullReach, lineReach, lightScale, cutEverySeconds, cutFirstSeconds, cutTellSeconds, cutDropSpeed, introHold, dieTime, legThick;
        public double sweepFirstSeconds, sweepEverySeconds, sweepTellSeconds, sweepLiveSeconds, sweepHeight, sweepDamage, sweepKnock, sweepUp, sweepBarHalf;

        public static Loom30Params From(GameConfig c)
        {
            const string g = "design.n30.span.loom.";
            return new Loom30Params
            {
                name = c.Str(g + "name"), pool = c.Num(g + "pool"), bodyDrop = c.Num(g + "bodyDrop"), bodyHalfW = c.Num(g + "bodyHalfW"), bodyHalfH = c.Num(g + "bodyHalfH"),
                kneeOut = c.Num(g + "kneeOut"), kneeUp = c.Num(g + "kneeUp"), hipOut = c.Num(g + "hipOut"), hipDrop = c.Num(g + "hipDrop"), jointRadius = c.Num(g + "jointRadius"), legHalf = c.Num(g + "legHalf"),
                fullReach = c.Num(g + "fullReach"), lineReach = c.Num(g + "lineReach"), lightScale = c.Num(g + "lightScale"),
                cutEverySeconds = c.Num(g + "cutEverySeconds"), cutFirstSeconds = c.Num(g + "cutFirstSeconds"), cutTellSeconds = c.Num(g + "cutTellSeconds"), cutDropSpeed = c.Num(g + "cutDropSpeed"),
                introHold = c.Num(g + "introHold"), dieTime = c.Num(g + "dieTime"), legThick = c.Num(g + "legThick"),
                sweepFirstSeconds = c.Num(g + "sweepFirstSeconds"), sweepEverySeconds = c.Num(g + "sweepEverySeconds"), sweepTellSeconds = c.Num(g + "sweepTellSeconds"),
                sweepLiveSeconds = c.Num(g + "sweepLiveSeconds"), sweepHeight = c.Num(g + "sweepHeight"), sweepDamage = c.Num(g + "sweepDamage"), sweepKnock = c.Num(g + "sweepKnock"), sweepUp = c.Num(g + "sweepUp"), sweepBarHalf = c.Num(g + "sweepBarHalf"),
            };
        }
    }

    /// <summary>The payload of "loomCut": stage "tell" (the line glows red) or "cut" (the line is plucked); the rope's anchor.</summary>
    public sealed class LoomCut30
    {
        public string stage;
        public Vec3 anchor;
        public double seconds;
    }

    public sealed class Loom30Body : Enemy, IBossBody27
    {
        public readonly Loom30 system;
        public Loom30Body(Loom30 system) { this.system = system; kind = "loom"; }
        public override double Height => system.p.bodyHalfH * 2;
        public override double Radius => system.p.bodyHalfW * 1.4;
        public override double AimHeight => system.p.bodyHalfH;
        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            double t = system.Raycast(o, d);
            headshot = t >= 0 && system.hitJoint >= 0;
            return t;
        }
        public override bool Damage(double amount, double flashTime) { system.Apply(amount); return false; }
        public double Filter(double amount, string weapon, Vec3 from, out bool blocked) => system.Filter(amount, weapon, out blocked);
        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt) { }
        public override void Stagger(double time, double dt) { }
        public override void Stun(double time, double dt) { }
    }

    public sealed class Loom30 : IBossSystem
    {
        public readonly Loom30Params p;
        public readonly Loom30Body body;
        readonly EventBus events;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt0;
        readonly BossEvent said = new BossEvent();
        readonly LoomCut30 cutSaid = new LoomCut30();
        BossKit kit;
        BossScale scale = BossScale.One;

        /// <summary>The deck's centre (world, on the deck) and the four grips (world; SW, SE, NE, NW as the plan lists them).</summary>
        public Vec3 centre;
        public readonly Vec3[] grips = new Vec3[4];
        /// <summary>The eight joints (world): joint 2i is leg i's KNEE (out over the lava, high), 2i + 1 its HIP (between the knee and the body). Grapple nodes.</summary>
        public readonly Vec3[] joints = new Vec3[8];
        /// <summary>Does joint j take full damage now (the view glows it cyan)?</summary>
        public readonly bool[] jointFull = new bool[8];
        public WyrmBox box;

        /// <summary>The sim hands these in: is the player hanging on a rope now, its anchor, and the cut itself (the rope released, the drop).</summary>
        public Func<bool> playerHanging;
        public Func<Vec3> ropeAnchor;
        public Action<double> cutLine;

        public string phase = "dead";
        public int stepNo, phaseSteps;
        int introSteps, dieSteps, cutTimer, cutTell;
        public int hitJoint = -1;
        string lastPart;
        /// <summary>The cut's tell is on (the view paints the line red) and the anchor it marks.</summary>
        public bool CutTelling => cutTell > 0;
        public Vec3 CutAnchor { get; private set; }
        public double CutTellProgress => cutTell > 0 ? 1.0 - (double)cutTell / Math.Max(1, S(p.cutTellSeconds)) : 0;
        // the harness's counters (this fight)
        public int cuts, cutTells, cutsDodged, fullHits, lightHits;

        // SWEEP: a leg sweeps the deck at shin height (a jump or a grapple off dodges it); only at a player standing on the deck
        int sweepTimer, sweepTell, sweepLive, sweepLeg = -1;
        bool sweepHit;
        readonly LoomCut30 sweepSaid = new LoomCut30();
        /// <summary>The sweep's tell is on / it is live, and the leg that sweeps (the view's red band across the deck from that leg's grip).</summary>
        public bool SweepTelling => sweepTell > 0;
        public bool SweepLive => sweepLive > 0;
        public int SweepLeg => sweepLeg;
        public double SweepTellProgress => sweepTell > 0 ? 1.0 - (double)sweepTell / Math.Max(1, S(p.sweepTellSeconds)) : 0;
        /// <summary>0..1 where the sweeping bar is across the deck (from the leg's grip to the far corner) while live.</summary>
        public double SweepBar { get; private set; }
        public int sweeps, sweepHits, sweepsDodged;
        /// <summary>The sim hands this in: is the player standing on the ground (the sweep only starts at a player on the deck).</summary>
        public Func<bool> playerGrounded;

        /// <summary>NIGHT31 B (design.n31.loom31): the full-damage reach and the hips' place with the row on (-1 / NaN: design.n30.span.loom's).</summary>
        public double fullReach31 = -1, hipOut31 = double.NaN, hipDrop31 = double.NaN;
        public bool nailOnly31;
        public double otherScale31 = 1, lightScale31 = -1;

        /// <summary>Is a point (feet) on the deck (inside the grips' square, at the deck's height)?</summary>
        public bool OnDeck(Vec3 feet)
        {
            double x0 = Math.Min(grips[0].x, grips[2].x), x1 = Math.Max(grips[0].x, grips[2].x), z0 = Math.Min(grips[0].z, grips[2].z), z1 = Math.Max(grips[0].z, grips[2].z);
            return feet.x >= x0 - 0.5 && feet.x <= x1 + 0.5 && feet.z >= z0 - 0.5 && feet.z <= z1 + 0.5 && feet.y >= centre.y - 0.5 && feet.y <= centre.y + 0.6;
        }

        public Loom30(GameConfig cfg, Loom30Params p, EventBus events, BossTarget target, Func<int> nextId)
        {
            this.p = p;
            this.events = events;
            this.target = target;
            this.nextId = nextId;
            dt0 = cfg.Dt;
            body = new Loom30Body(this);
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;
        public bool Hurtable => body.alive && phase == "p1";

        /// <summary>The legs from the deck's centre and the grips (the plan's loomLegs; or the centre +- legHalf when none is given).</summary>
        public void Place(Vec3 deckCentre, Vec3[] legGrips)
        {
            centre = deckCentre;
            for (int i = 0; i < 4; i++)
                grips[i] = legGrips != null && legGrips.Length == 4 ? legGrips[i]
                    : new Vec3(deckCentre.x + (i == 0 || i == 3 ? -p.legHalf : p.legHalf), deckCentre.y, deckCentre.z + (i < 2 ? p.legHalf : -p.legHalf));
            Pose();
        }

        Vec3 BodyCentre => new Vec3(centre.x, centre.y - p.bodyDrop, centre.z);

        void Pose()
        {
            Vec3 bc = BodyCentre;
            for (int i = 0; i < 4; i++)
            {
                Vec3 g = grips[i];
                double ox = g.x - centre.x, oz = g.z - centre.z, ol = JsMath.Hypot(ox, oz);
                if (ol < 1e-6) { ox = 1; oz = 0; ol = 1; }
                ox /= ol; oz /= ol;
                // the knee: out past the grip over the lava and up (the leg arches over the deck's edge and grips it from outside)
                var knee = new Vec3(g.x + ox * p.kneeOut, g.y + p.kneeUp, g.z + oz * p.kneeOut);
                // the hip: just outside the deck's edge and under it (the leg runs from the body under the deck, out past the edge, up to the knee and back down to the grip):
                // over the lava, so only a swing reaches it (a player on the deck stands more than fullReach from it)
                joints[2 * i] = knee;
                double hipOut = double.IsNaN(hipOut31) ? p.hipOut : hipOut31, hipDrop = double.IsNaN(hipDrop31) ? p.hipDrop : hipDrop31; // NIGHT31 B: loom31 raises the hips into sight
                joints[2 * i + 1] = new Vec3(g.x + ox * hipOut, g.y - hipDrop, g.z + oz * hipOut);
            }
            var b = default(WyrmBox);
            b.Orient(new Vec3(0, 0, 1));
            b.c = bc;
            b.hr = b.hf = p.bodyHalfW;
            b.hu = p.bodyHalfH;
            box = b;
        }

        /// <summary>Leg i's polyline (the body's side, the hip, the knee, the grip): the rope's anchor near it means "hanging on that leg's line".</summary>
        double DistToLeg(int i, Vec3 q)
        {
            Vec3 g = grips[i], k = joints[2 * i], h = joints[2 * i + 1], s = LegRoot(i);
            return Math.Min(SegDist(q, s, h), Math.Min(SegDist(q, h, k), SegDist(q, k, g)));
        }

        /// <summary>Where leg i leaves the body (its side, toward the grip).</summary>
        public Vec3 LegRoot(int i)
        {
            Vec3 bc = BodyCentre, g = grips[i];
            double ox = g.x - centre.x, oz = g.z - centre.z, ol = JsMath.Hypot(ox, oz);
            if (ol < 1e-6) { ox = 1; oz = 0; ol = 1; }
            return new Vec3(bc.x + ox / ol * p.bodyHalfW, bc.y, bc.z + oz / ol * p.bodyHalfW);
        }

        static double SegDist(Vec3 pt, Vec3 a, Vec3 b)
        {
            Vec3 ab = b - a, ap = pt - a;
            double len2 = Vec3.Dot(ab, ab), t = len2 < 1e-12 ? 0 : Math.Max(0, Math.Min(1, Vec3.Dot(ap, ab) / len2));
            return Vec3.DistanceTo(pt, a + ab * t);
        }

        /// <summary>Does joint j take full damage now: the player hangs on that leg's line, or stands within fullReach of the joint.</summary>
        public bool FullOn(int j)
        {
            int leg = j / 2;
            Vec3 me = target.position(), chest = new Vec3(me.x, me.y + 1.0, me.z);
            if (Vec3.DistanceTo(chest, joints[j]) <= (fullReach31 >= 0 ? fullReach31 : p.fullReach)) return true;
            if (playerHanging != null && playerHanging() && ropeAnchor != null && DistToLeg(leg, ropeAnchor()) <= p.lineReach) return true;
            return false;
        }

        // --- the fight ------------------------------------------------------------------------------------------------------------------------

        public void StartFight(BossFight ctx)
        {
            kit = ctx.kit;
            scale = ctx.scale;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(p.pool * ctx.scale.hp);
            body.hp = body.maxHp;
            stepNo = 0;
            cuts = cutTells = cutsDodged = fullHits = lightHits = 0;
            cutTell = 0;
            cutTimer = S(p.cutFirstSeconds);
            sweepTell = sweepLive = 0;
            sweepLeg = -1;
            sweepTimer = S(p.sweepFirstSeconds);
            sweeps = sweepHits = sweepsDodged = 0;
            introSteps = Math.Max(kit != null ? kit.IntroSteps() : 0, S(p.introHold));
            Pose();
            body.position = box.c;
            body.previous = body.position;
            SetPhase("intro");
            said.Reset("loom");
            said.name = "LOOM";
            said.hp = body.maxHp;
            events.Emit("bossRise", said);
        }

        /// <summary>A retry (the player died in the room): the fight goes on (its HP kept), the cut's clock starts over.</summary>
        public void Retry()
        {
            cutTell = 0;
            cutTimer = S(p.cutFirstSeconds);
            sweepTell = sweepLive = 0;
            sweepLeg = -1;
            sweepTimer = S(p.sweepFirstSeconds);
        }

        public void Clear()
        {
            body.alive = false;
            phase = "dead";
            cutTell = 0;
        }

        void SetPhase(string ph)
        {
            phase = ph;
            phaseSteps = 0;
            said.Reset("loom");
            said.phase = ph;
            said.hp = body.hp;
            said.maxHp = body.maxHp;
            events.Emit("bossPhase", said);
        }

        public BossBar Bar() => new BossBar { valid = body.alive, id = body.id, name = p.name, phase = phase, hp = Math.Max(0, body.hp), max = body.maxHp, segment = 0 };

        /// <summary>The nearest joint or the body along a unit ray (-1: a miss); hitJoint names the joint (-1: the body).</summary>
        public double Raycast(Vec3 o, Vec3 d)
        {
            if (!body.alive || phase == "dying" || phase == "dead") return -1;
            double best = box.Ray(o, d);
            int j = -1;
            for (int i = 0; i < joints.Length; i++)
            {
                double t = WyrmMath.RaySphere(o, d, joints[i], p.jointRadius);
                if (t >= 0 && (best < 0 || t < best)) { best = t; j = i; }
            }
            hitJoint = best >= 0 ? j : -1;
            return best;
        }

        public double Filter(double amount, string weapon, out bool blocked)
        {
            blocked = false;
            int j = hitJoint;
            hitJoint = -1;
            if (!Hurtable) { blocked = true; return 0; }
            if (nailOnly31 && weapon == Stag31.ParryWeapon) { fullHits++; lastPart = j >= 0 ? "joint" : "body"; return amount; } // NIGHT31 B (loom31): a parried projectile always lands in full (the lead, 12:18)
            bool full = j >= 0 && FullOn(j) && (!nailOnly31 || weapon == "nailgun"); // NIGHT31 B (loom31): only the NAILGUN lands in full on a joint
            if (full) fullHits++; else lightHits++;
            lastPart = j >= 0 ? "joint" : "body";
            if (nailOnly31 && weapon != "nailgun" && weapon != "gauntlet") return amount * otherScale31; // NIGHT31 B (loom31): the NAILGUN boss: other guns chip
            return full ? amount : amount * (lightScale31 >= 0 ? lightScale31 : p.lightScale);
        }

        public void Apply(double dmg)
        {
            if (!body.alive) return;
            body.hp -= dmg;
            body.flash = 0.06;
            said.Reset("loom");
            said.damage = JsMath.Round(dmg);
            said.part = lastPart ?? "body";
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            if (body.hp <= 0 && phase != "dying")
            {
                body.hp = 0;
                SetPhase("dying");
                dieSteps = S(p.dieTime);
                cutTell = 0;
                kit?.Dying("loom", body.position);
            }
        }

        public void Step(double dt)
        {
            if (!body.alive) return;
            body.previous = body.position;
            if (Frozen) return;
            stepNo++;
            phaseSteps++;
            Pose();
            body.position = box.c;
            for (int i = 0; i < joints.Length; i++) jointFull[i] = phase == "p1" && FullOn(i);
            if (phase == "intro" && phaseSteps >= introSteps) SetPhase("p1");
            if (phase == "p1") { StepCut(); StepSweep(); }
            if (phase == "dying" && --dieSteps <= 0)
            {
                body.alive = false;
                phase = "dead";
                kit?.Down("loom", body.id, body.position);
            }
        }

        void StepCut()
        {
            bool hanging = playerHanging != null && playerHanging();
            if (cutTell > 0)
            {
                if (--cutTell > 0) return;
                if (hanging)
                {
                    cuts++;
                    cutLine?.Invoke(p.cutDropSpeed);
                    Say("cut", 0);
                }
                else cutsDodged++;
                cutTimer = S(p.cutEverySeconds);
                return;
            }
            if (cutTimer > 0) { cutTimer--; return; }
            if (!hanging || sweepTell > 0 || sweepLive > 0) return; // it waits for a line to cut (never during a sweep)
            CutAnchor = ropeAnchor != null ? ropeAnchor() : target.position();
            cutTell = Math.Max(1, S(p.cutTellSeconds));
            cutTells++;
            Say("tell", p.cutTellSeconds);
            said.Reset("loom");
            said.name = "cut";
            said.seconds = p.cutTellSeconds;
            events.Emit("bossTell", said);
        }

        /// <summary>The SWEEP: every sweepEverySeconds, while the player stands on the deck (not on a rope, no cut telling), the leg nearest him is marked (sweepTellSeconds, event
        /// loomSweep "tell"), then it sweeps the deck for sweepLiveSeconds: a player on the deck with his feet under sweepHeight takes sweepDamage once and is knocked outward
        /// (sweepKnock, sweepUp). A jump over it, or a grapple off the deck, dodges it.</summary>
        void StepSweep()
        {
            Vec3 me = target.position();
            bool hanging = playerHanging != null && playerHanging();
            if (sweepTell > 0)
            {
                if (--sweepTell == 0) { sweepLive = Math.Max(1, S(p.sweepLiveSeconds)); sweepHit = false; SaySweep("live", p.sweepLiveSeconds); }
                return;
            }
            if (sweepLive > 0)
            {
                // the leg is a bar that crosses the deck from its grip to the far corner over sweepLiveSeconds: it hits where it is NOW (a jump timed over it clears it)
                int total = Math.Max(1, S(p.sweepLiveSeconds));
                double prog = 1.0 - (double)(sweepLive - 1) / total;
                Vec3 g0 = grips[sweepLeg], g1 = grips[(sweepLeg + 2) % 4];
                double ax = g1.x - g0.x, az = g1.z - g0.z, al = JsMath.Hypot(ax, az);
                double along = al > 1e-6 ? ((me.x - g0.x) * ax + (me.z - g0.z) * az) / al : 0, bar = prog * al;
                SweepBar = prog;
                bool low = OnDeck(me) && me.y <= centre.y + p.sweepHeight && !hanging && Math.Abs(along - bar) <= p.sweepBarHalf;
                if (low && !sweepHit)
                {
                    sweepHit = true;
                    Health h = target.health;
                    if (h != null && !h.Dead && h.Damage(p.sweepDamage * scale.damage, "boss")) sweepHits++;
                    double ox = me.x - centre.x, oz = me.z - centre.z, ol = JsMath.Hypot(ox, oz);
                    if (ol < 1e-6) { ox = 1; oz = 0; ol = 1; }
                    target.impulse?.Invoke(ox / ol * p.sweepKnock, p.sweepUp, oz / ol * p.sweepKnock);
                }
                if (--sweepLive == 0)
                {
                    if (!sweepHit) sweepsDodged++;
                    SaySweep("end", 0);
                    sweepLeg = -1;
                    sweepTimer = S(p.sweepEverySeconds);
                }
                return;
            }
            if (sweepTimer > 0) { sweepTimer--; return; }
            bool grounded = playerGrounded == null || playerGrounded();
            if (hanging || cutTell > 0 || !grounded || !OnDeck(me)) return; // it waits for a player standing on the deck
            double best = 1e9;
            for (int i = 0; i < 4; i++) { double d = JsMath.Hypot(grips[i].x - me.x, grips[i].z - me.z); if (d < best) { best = d; sweepLeg = i; } }
            sweepTell = Math.Max(1, S(p.sweepTellSeconds));
            sweeps++;
            SaySweep("tell", p.sweepTellSeconds);
            said.Reset("loom");
            said.name = "sweep";
            said.seconds = p.sweepTellSeconds;
            events.Emit("bossTell", said);
        }

        void SaySweep(string stage, double sec)
        {
            sweepSaid.stage = stage;
            sweepSaid.anchor = sweepLeg >= 0 ? grips[sweepLeg] : centre;
            sweepSaid.seconds = sec;
            events.Emit("loomSweep", sweepSaid);
        }

        void Say(string stage, double sec)
        {
            cutSaid.stage = stage;
            cutSaid.anchor = CutAnchor;
            cutSaid.seconds = sec;
            events.Emit("loomCut", cutSaid);
        }
    }
}
