// NIGHT37 builder O (NIGHT37.md O1; Research/2026-10-12-wave37/BOSSES.md "L4 THE LOOM"): THE LOOM from scratch, on builder K's Boss37 brain.
// A weaving machine hung over THE CANYON (level 4's last room, n37-span o3): a body slung between six long legs, each leg's KNEE a glowing joint
// and a MOVING GRAPPLE NODE (the sim adds the six to map.grappleNodes for the fight and moves them each step), a shielded spinneret underneath.
//   THE FIGHT TRAVELS  (rule 1) three stations along the canyon, one per leg (the room row's loom37.stations): leg 1 between tower pairs 1-2,
//                      leg 2 between 2-3 (faster), leg 3 the nest (it turns to fight). The body glides to its leg's station at stationSpeed.
//   DAMAGE (rule 2)    nail a KNEE of the leg's pair (nails stick: nailsToTear in it), then pull yourself AWAY with the grapple (the distance
//                      grows tearDistM past its nearest since the nailing, while a rope is out) or press LT (the NAILGUN's detonate, event
//                      nailDetonate): the nails TEAR the knee (Stagger tearStagger; event loom37Tear). Two knees torn empty the leg's meter:
//                      the spinneret opens CYAN (the brain's WINDOW): every hit lands in full (the sim adds a node on it: whip onto the body).
//                      Outside the window everything chips (Boss37 chipScale). When the window closes: the next leg (legs are places);
//                      in leg 3 it falls into the canyon (the kill).
//   ATTACKS            THREAD LASH (RED): a thread whips along the line from the body to where you were at the wind-up's start: off the line
//                      at the active step or lashDamage. SPOOL SHOT (YELLOW): a weighted spool flies at you: punch it back (Parried: the
//                      budget refund + parryStagger) or eat spoolDamage. WEB WALL (RED, the camp habit, legs 2-3): a sheet between the
//                      tower pair ahead (the sim swaps a world layer in for its active time).
// Pure: no UnityEngine, 120 Hz whole steps, JsMath, no randomness of its own (the brain's Mulberry32 picks).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.n37.loom37's fight numbers (each has its _why there).</summary>
    public sealed class Loom37Params
    {
        public string name = "THE LOOM";
        public double hp = 750, kneeRadius = 1.3, tearDistM = 7, nailHoldS = 6, tearStagger = 3, parryStagger = 1.5, bodyY = 20;
        public int nailsToTear = 3;
        public double[] stationSpeed = { 5, 8, 0 };
        public double lashDamage = 30, lashWidthM = 2.5, lashLength = 60, spoolDamage = 25, spoolSpeed = 22, spoolRadius = 0.8, parryReach = 3.5, parryConeDeg = 60;
        public double revealS = 2, dieS = 2.5, kneeSpanX = 9, kneeDropY = 6, kneeSpreadZ = 7, footSpanX = 21, footDropY = 12, bobM = 0.8, bobPeriodS = 2.4, bodyHalfW = 3, bodyHalfH = 1.6, parryReturnMul = 1.5;

        public static Loom37Params From(GameConfig c)
        {
            const string R = "design.n37.loom37.";
            var p = new Loom37Params();
            double N(string k, double d) => c.Has(R + k) ? c.Num(R + k) : d;
            p.hp = N("hp", p.hp);
            p.kneeRadius = N("kneeRadius", p.kneeRadius);
            p.nailsToTear = (int)N("nailsToTear", p.nailsToTear);
            p.tearDistM = N("tearDistM", p.tearDistM);
            p.nailHoldS = N("nailHoldS", p.nailHoldS);
            p.tearStagger = N("tearStagger", p.tearStagger);
            p.parryStagger = N("parryStagger", p.parryStagger);
            p.bodyY = N("bodyY", p.bodyY);
            if (c.Has(R + "stationSpeed")) p.stationSpeed = c.Nums(R + "stationSpeed");
            p.lashDamage = N("lashDamage", p.lashDamage);
            p.lashWidthM = N("lashWidthM", p.lashWidthM);
            p.lashLength = N("lashLength", p.lashLength);
            p.spoolDamage = N("spoolDamage", p.spoolDamage);
            p.spoolSpeed = N("spoolSpeed", p.spoolSpeed);
            p.spoolRadius = N("spoolRadius", p.spoolRadius);
            p.parryReach = N("parryReach", p.parryReach);
            p.parryConeDeg = N("parryConeDeg", p.parryConeDeg);
            p.revealS = N("revealS", p.revealS);
            p.dieS = N("dieS", p.dieS);
            p.kneeSpanX = N("kneeSpanX", p.kneeSpanX);
            p.kneeDropY = N("kneeDropY", p.kneeDropY);
            p.kneeSpreadZ = N("kneeSpreadZ", p.kneeSpreadZ);
            p.footSpanX = N("footSpanX", p.footSpanX);
            p.footDropY = N("footDropY", p.footDropY);
            p.bobM = N("bobM", p.bobM);
            p.bobPeriodS = N("bobPeriodS", p.bobPeriodS);
            p.bodyHalfW = N("bodyHalfW", p.bodyHalfW);
            p.bodyHalfH = N("bodyHalfH", p.bodyHalfH);
            p.parryReturnMul = N("parryReturnMul", p.parryReturnMul);
            return p;
        }
    }

    /// <summary>One knee: a moving grapple node, the nails stuck in it, torn or not.</summary>
    public sealed class Loom37Knee
    {
        public Vec3 at, hip, foot;
        public int nails, nailAge;
        public bool torn;
        /// <summary>The nearest the player has been since the knee carried nailsToTear nails (the pull-away measures from it).</summary>
        public double nearest = 1e9;
        public double tornT; // steps since it tore (the view's limp)
    }

    /// <summary>The weighted spool in flight.</summary>
    public sealed class Loom37Spool
    {
        public Vec3 pos, prev, vel;
        public bool alive, back;
        /// <summary>NIGHT47 L (loom47): a BOBBIN RAIN spool (falls straight), RED (not parryable), its damage (0: spoolDamage).</summary>
        public bool bob47, red47;
        public double dmg47;
        public int life;
    }

    /// <summary>Pooled payload of loom37Tear / loom37Open / loom37Lash / loom37Spool / loom37Leg / loom37Down (read in the handler).</summary>
    public sealed class Loom37Event
    {
        public string stage;
        public int knee, leg;
        public Vec3 at;
        public double seconds;
    }

    public sealed class Loom37Body : Enemy, IBossBody27
    {
        public readonly Loom37 system;
        public Loom37Body(Loom37 system) { this.system = system; kind = "loom"; }
        public override double Height => system.p.bodyHalfH * 2;
        public override double Radius => system.p.bodyHalfW;
        public override double AimHeight => 0;
        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            double t = system.Raycast(o, d);
            headshot = t >= 0 && system.hitKnee >= 0;
            return t;
        }
        public override bool Damage(double amount, double flashTime) { system.Apply(amount); return false; }
        public double Filter(double amount, string weapon, Vec3 from, out bool blocked) => system.Filter(amount, weapon, out blocked);
        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt) { }
        public override void Stagger(double time, double dt) { }
        public override void Stun(double time, double dt) { }
    }

    public sealed partial class Loom37 : IBossSystem
    {
        public const string Lash = "lash", Spool = "spool", Web = "web";
        public const string ParryWeapon = "parry37";
        public readonly Loom37Params p;
        public readonly Boss37Brain brain;
        public readonly Loom37Body body;
        readonly EventBus events;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt;
        readonly BossEvent said = new BossEvent();
        readonly Loom37Event ev = new Loom37Event();
        BossKit kit;

        /// <summary>The stations (world: the body's place for legs 1..3), the canyon's centre line x and its walls' x (feet).</summary>
        public readonly Vec3[] stations = new Vec3[3];
        public double centreX;
        /// <summary>The body's centre now (the spinneret is under it).</summary>
        public Vec3 centre;
        public readonly Loom37Knee[] knees = new Loom37Knee[6];
        public readonly List<Loom37Spool> spools = new List<Loom37Spool>();
        public int hitKnee = -1;
        public bool hitSpinneret;
        public int stepNo;
        bool falling;
        double fallV;
        int dieLeft;

        /// <summary>The sim hands these in.</summary>
        public Func<Vec3> playerVel;
        public Func<bool> playerGrounded, playerRiding, ropeOut;
        /// <summary>The lash's line (world) while it tells / strikes; the web wall's index (-1 none) while it tells / stands.</summary>
        public Vec3 lashFrom, lashTo;
        public bool LashLive => brain.Current?.id == Lash && (brain.state == Boss37State.Tell || brain.state == Boss37State.Active);
        public int webPair = -1;
        public bool WebSolid => brain.Current?.id == Web && brain.state == Boss37State.Active;
        public bool WebTelling => brain.Current?.id == Web && brain.state == Boss37State.Tell;
        /// <summary>The pair of tower webs (the sim's layer) count, set by the sim from the level row.</summary>
        public int webCount;
        /// <summary>The spinneret's place (the CYAN node in the window).</summary>
        public Vec3 Spinneret => new Vec3(centre.x, centre.y - p.bodyHalfH - 0.8, centre.z);
        public bool Open => brain.Open;
        public bool Falling => falling;

        // the harness's counters
        public int tears, ltTears, nailsIn, lashes, lashHits, spoolsShot, spoolHits, spoolParries, webs, fullHits, chipHits, windowsOpened;
        public double dealt, dealtInWindow;

        public Loom37(GameConfig cfg, Loom37Params p, Boss37Params bp, EventBus events, BossTarget target, Func<int> nextId, uint seed)
        {
            this.p = p;
            this.events = events;
            this.target = target;
            this.nextId = nextId;
            dt = cfg.Dt;
            body = new Loom37Body(this);
            brain = new Boss37Brain(bp, "loom37", dt, events, seed);
            foreach (string id in new[] { Lash, Spool, Web })
                if (cfg.Has("design.n37.loom37.attacks." + id)) brain.attacks.Add(Boss37Attack.From(cfg, "design.n37.loom37.attacks." + id, dt));
            for (int i = 0; i < 6; i++) knees[i] = new Loom37Knee();
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;
        public bool Hurtable => body.alive && brain.Fighting;
        /// <summary>The knees of the current leg (pair leg-1: 2(leg-1), 2(leg-1)+1).</summary>
        public bool ActiveKnee(int k) => !knees[k].torn && k / 2 == Math.Max(0, Math.Min(2, brain.leg - 1));
        public int Torn { get { int n = 0; foreach (var k in knees) if (k.torn) n++; return n; } }

        /// <summary>The fight's places: three stations (world; the body's place per leg) and the canyon's centre line.</summary>
        public void Place(Vec3[] st, double canyonCentreX)
        {
            for (int i = 0; i < 3; i++) { Vec3 s = st[Math.Min(i, st.Length - 1)]; stations[i] = new Vec3(s.x, p.bodyY, s.z); } // bodyY over the ledges (y 0)
            centreX = canyonCentreX;
            for (int i = 0; i < 3; i++) home40[i] = stations[i]; // NIGHT40 B4: each leg's own station (an anti-hide's move is undone to it)
            centre = stations[0];
            Pose();
        }

        public void StartFight(BossFight ctx)
        {
            kit = ctx.kit;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(p.hp * ctx.scale.hp);
            body.hp = body.maxHp;
            stepNo = 0;
            falling = false;
            fallV = 0;
            spools.Clear();
            foreach (var k in knees) { k.torn = false; k.nails = 0; k.nailAge = 0; k.nearest = 1e9; k.tornT = 0; }
            tears = ltTears = nailsIn = lashes = lashHits = spoolsShot = spoolHits = spoolParries = webs = fullHits = chipHits = windowsOpened = 0;
            dealt = dealtInWindow = 0;
            webPair = -1;
            centre = stations[0];
            if (stay40) Stay40Start(); // NIGHT40 B4: every station home, the glide at rest, the centre inside the canyon
            brain.source = body.id;
            brain.SetCheckpoint(1, target.position(), 0);
            brain.Begin(S(p.revealS), target.position());
            Pose();
            body.position = centre;
            body.previous = centre;
            said.Reset("loom37");
            said.name = p.name;
            said.hp = body.maxHp;
            events.Emit("bossRise", said);
        }

        public void Clear()
        {
            body.alive = false;
            spools.Clear();
            webPair = -1;
            brain.tells?.End(brain.source);
            brain.Reset();
        }

        public BossBar Bar() => new BossBar { valid = body.alive, id = body.id, name = p.name, phase = brain.state.ToString(), hp = Math.Max(0, body.hp), max = body.maxHp, segment = 0 };

        /// <summary>The knees' and feet's places for the body's centre now (a slow bob: the knees are moving nodes).</summary>
        void Pose()
        {
            // NIGHT48 L (kneeHold47's release pop, measured by n48loom hold: the bob's clock ran on through the hold, so the knee jumped up to
            // 1.8 m in the one step the hold ended): the bob's clock stops while held (holds47 counts only kneeHold47's held steps: 0 with it off)
            double ph = 2 * Math.PI * (stepNo - holds47) * dt / Math.Max(0.1, p.bobPeriodS);
            for (int i = 0; i < 6; i++)
            {
                int pair = i / 2;
                double side = (i % 2 == 0) ? -1 : 1;
                double dz = (1 - pair) * p.kneeSpreadZ; // pair 0 south (+z: toward the player), pair 2 north
                Loom37Knee k = knees[i];
                double bob = k.torn ? -2.5 : p.bobM * JsMath.Sin(ph + i * 1.3);
                k.hip = new Vec3(centre.x + side * p.bodyHalfW, centre.y, centre.z + dz * 0.4);
                k.at = new Vec3(centre.x + side * p.kneeSpanX, centre.y - p.kneeDropY + bob, centre.z + dz);
                k.foot = new Vec3(centreX + side * p.footSpanX, centre.y - p.footDropY, centre.z + dz * 1.4);
            }
        }

        /// <summary>The nearest knee, the spinneret or the body along a unit ray (-1: a miss); hitKnee / hitSpinneret name what.</summary>
        public double Raycast(Vec3 o, Vec3 d)
        {
            hitKnee = -1;
            hitSpinneret = false;
            if (!body.alive || falling) return -1;
            double best = -1;
            for (int i = 0; i < 6; i++)
            {
                if (knees[i].torn) continue;
                double t = WyrmMath.RaySphere(o, d, knees[i].at, p.kneeRadius);
                if (t >= 0 && (best < 0 || t < best)) { best = t; hitKnee = i; }
            }
            double tb = WyrmMath.RaySphere(o, d, centre, p.bodyHalfW);
            if (tb >= 0 && (best < 0 || tb < best)) { best = tb; hitKnee = -1; }
            double ts = WyrmMath.RaySphere(o, d, Spinneret, 1.2);
            if (ts >= 0 && (best < 0 || ts < best)) { best = ts; hitKnee = -1; hitSpinneret = true; }
            // NIGHT48 L (loomReach48, a sim row; DECIDE48 D-34): the drawn shuttle runs 13.6 m point to point and its outer 3.8 m of each
            // end lay outside the 3 m sphere (n48loom reach: every ray at 4.8 / 6.0 / 6.8 m missed). Two capsules along the hull's long axis,
            // from the middle to each point (one segment, +-reach48Outer, radius reach48R: its round end reaches the point), the axis turned
            // as the view turns the hull (world x, toward the shooter by reach48Follow of the angle, at most reach48YawMax deg). Off: the
            // sphere alone, NIGHT47's test.
            // only for a ray that hit NOTHING else (round 5, measured: with the capsules competing on distance, rays at a knee behind the
            // hull's ends landed on the body instead: the human band's median went 92.4 -> 95.3 s): a knee, the spinneret or the sphere wins
            if (reach48 && best < 0)
            {
                double tc = Reach48Ray(o, d);
                if (tc >= 0 && (best < 0 || tc < best)) { best = tc; hitKnee = -1; hitSpinneret = false; }
            }
            return best;
        }

        /// <summary>loomReach48 (the sim hands it in each step; numbers design.n48.loomReach48.numbers): on, and the capsules' numbers.</summary>
        public bool reach48;
        public double reach48R = 1.2, reach48Inner = 0, reach48Outer = 5.6, reach48Follow = 0.35, reach48YawMax = 20;

        /// <summary>The hull's long axis (flat, unit) for a shooter at `o`: world x turned toward the broadside by reach48Follow of the angle
        /// (a normalized lerp, no transcendental: within 1 deg of the view's angle x Follow up to its cap), capped at reach48YawMax.</summary>
        Vec3 Reach48Axis(Vec3 o)
        {
            double dx = o.x - centre.x, dz = o.z - centre.z, n = JsMath.Hypot(dx, dz);
            if (n < 1e-6) return new Vec3(1, 0, 0);
            // the broadside to the shooter, on the +x side: perpendicular to (dx, dz)
            double sx = -dz / n, sz = dx / n;
            if (sx < 0) { sx = -sx; sz = -sz; }
            double ax = 1 + (sx - 1) * reach48Follow, az = sz * reach48Follow, an = JsMath.Hypot(ax, az);
            ax /= an; az /= an;
            double cap = JsMath.Sin(reach48YawMax * Math.PI / 180);
            if (Math.Abs(az) > cap) { az = az < 0 ? -cap : cap; ax = JsMath.Cos(reach48YawMax * Math.PI / 180); }
            return new Vec3(ax, 0, az);
        }

        /// <summary>The ray against the two capsules (the segment centre +- axis x [reach48Inner, reach48Outer], radius reach48R): the entry
        /// distance or -1.</summary>
        double Reach48Ray(Vec3 o, Vec3 d)
        {
            Vec3 ax = Reach48Axis(o);
            double best = -1;
            for (int side = -1; side <= 1; side += 2)
            {
                Vec3 a = centre + ax * (side * reach48Inner), b = centre + ax * (side * reach48Outer);
                // the closest points of the ray o + t d (t >= 0, d unit) and the segment a + s (b - a) (s in 0..1)
                Vec3 u = b - a, w = o - a;
                double uu = u.x * u.x + u.y * u.y + u.z * u.z, ud = u.x * d.x + u.y * d.y + u.z * d.z;
                double uw = u.x * w.x + u.y * w.y + u.z * w.z, dw = d.x * w.x + d.y * w.y + d.z * w.z;
                double den = uu - ud * ud, s, t;
                if (uu < 1e-9) { s = 0; t = -dw; }
                else if (den < 1e-9) { s = 0; t = -dw; } // parallel: from the segment's start
                else { s = (uw - ud * dw) / den; t = (ud * uw - uu * dw) / den; }
                if (s < 0) s = 0; else if (s > 1) s = 1;
                Vec3 q = a + u * s;
                t = (q.x - o.x) * d.x + (q.y - o.y) * d.y + (q.z - o.z) * d.z; // the ray's closest point to q
                if (t < 0) t = 0;
                Vec3 p = o + d * t;
                double dd = (p.x - q.x) * (p.x - q.x) + (p.y - q.y) * (p.y - q.y) + (p.z - q.z) * (p.z - q.z), rr = reach48R * reach48R;
                if (dd > rr) continue;
                double entry = Math.Max(0, t - Math.Sqrt(rr - dd));
                if (best < 0 || entry < best) best = entry;
            }
            return best;
        }

        public double Filter(double amount, string weapon, out bool blocked)
        {
            blocked = false;
            int k = hitKnee;
            hitKnee = -1;
            hitSpinneret = false;
            if (!Hurtable) { blocked = true; return 0; }
            if (loom47) Seam47(k, amount); // NIGHT47 L (loom47): a latched knee's seam fills inside a BEND
            // nails stick in a knee of the leg's pair (outside the window)
            if (k >= 0 && weapon == "nailgun" && ActiveKnee(k) && !brain.Open)
            {
                Loom37Knee kn = knees[k];
                kn.nails++;
                kn.nailAge = 0;
                nailsIn++;
                if (kn.nails == p.nailsToTear) kn.nearest = Vec3.DistanceTo(target.position(), kn.at);
            }
            bool right = weapon == ParryWeapon;
            double s = brain.Scale(right);
            if (s >= 1) fullHits++; else chipHits++;
            return amount * s;
        }

        public void Apply(double dmg)
        {
            if (!body.alive || falling) return;
            if (loom47) dmg = Loom47Floor(dmg); // NIGHT47 L (loom47): a phase's window takes the bar to its third, never past it
            body.hp -= dmg;
            dealt += dmg;
            if (brain.Open) dealtInWindow += dmg;
            body.flash = 0.06;
            said.Reset("loom37");
            said.damage = JsMath.Round(dmg);
            said.part = brain.Open ? "spinneret" : "shell";
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            if (body.hp <= 0) Fall();
        }

        /// <summary>It falls into the canyon (the kill): the brain dies, the body drops.</summary>
        void Fall()
        {
            if (falling) return;
            body.hp = 0;
            falling = true;
            fallV = 0;
            spools.Clear();
            webPair = -1;
            brain.Die(S(p.dieS));
            dieLeft = S(p.dieS);
            Say("loom37Down", "fall", -1, centre);
            kit?.Dying("loom37", centre);
        }

        /// <summary>The NAILGUN's detonate (LT; the sim forwards nailDetonate): every active knee carrying nailsToTear nails tears.</summary>
        public void Detonated()
        {
            for (int i = 0; i < 6; i++)
                if (ActiveKnee(i) && knees[i].nails >= p.nailsToTear) { ltTears++; Tear(i, "detonate"); }
        }

        void Tear(int i, string how)
        {
            Loom37Knee k = knees[i];
            k.torn = true;
            k.nails = 0;
            k.tornT = 0;
            tears++;
            Say("loom37Tear", how, i, k.at);
            if (brain.Stagger(p.tearStagger, "tear")) { windowsOpened++; Say("loom37Open", "open", -1, Spinneret); }
        }

        /// <summary>A punch from `eye` along `dir`: the nearest live spool within parryReach in the cone goes back (a parry).</summary>
        public bool Punch(Vec3 eye, Vec3 dir)
        {
            if (!body.alive) return false;
            double cosCone = JsMath.Cos(p.parryConeDeg * Math.PI / 180), best = p.parryReach + p.spoolRadius;
            Loom37Spool pick = null;
            foreach (var s in spools)
            {
                if (!s.alive || s.back || s.red47) continue; // NIGHT47 L: a RED bobbin is not parryable
                Vec3 to = s.pos - eye;
                double d = to.Length;
                if (d > best) continue;
                if (d > 0.5 && Vec3.Dot(to, dir) / d < cosCone) continue;
                best = d;
                pick = s;
            }
            if (pick == null) return false;
            pick.back = true;
            Vec3 home = centre - pick.pos;
            double hl = Math.Max(1e-6, home.Length);
            pick.vel = home * (p.spoolSpeed * p.parryReturnMul / hl);
            pick.life = S(hl / (p.spoolSpeed * p.parryReturnMul) + 0.5);
            spoolParries++;
            Say("loom37Spool", "parry", -1, pick.pos);
            // the parry's refund + drain WITHOUT Boss37Brain.Parried's break: the spool is already in flight, the attack in TELL now may be another
            // one (a lash winding up): breaking it cut that tell short (MEASURED n37-r3 Player reel: lash 0.38 s, under rule 7's 1.0 s floor)
            if (brain.Fighting && !brain.Open)
            {
                brain.parries++;
                brain.budget = Math.Max(0, brain.budget - brain.p.parryRefund);
                if (brain.Stagger(p.parryStagger, "parry")) { windowsOpened++; Say("loom37Open", "open", -1, Spinneret); }
            }
            return true;
        }

        void Say(string name, string stage, int knee, Vec3 at)
        {
            ev.stage = stage;
            ev.knee = knee;
            ev.leg = brain.leg;
            ev.at = at;
            ev.seconds = 0;
            events.Emit(name, ev);
        }

        /// <summary>One fixed step: the brain, the body's glide to its station, the knees (nails, the pull-away tear), the attacks.</summary>
        public void Step(double dt0, in Boss37Context ctxIn)
        {
            if (!body.alive) return;
            body.previous = body.position;
            if (Frozen) return;
            stepNo++;
            Vec3 me = target.position();
            if (falling)
            {
                fallV += 23 * dt;
                centre = new Vec3(centre.x, centre.y - fallV * dt, centre.z);
                if (stay40) Stay40Land(ref fallV); // NIGHT40 B4: the fall lands on the canyon floor
                Pose();
                body.position = centre;
                if (--dieLeft <= 0) { body.alive = false; kit?.Down("loom37", body.id, centre); }
                return;
            }
            int legBefore = brain.leg;
            Boss37State before = brain.state;
            Boss37Context ctx = ctxIn;
            ctx.boss = centre;
            ctx.hpFrac = HpFrac;
            if (stay40) Stay40Before(); // NIGHT40 B4: a station home again, the leg change held while the body still glides
            if (sweep41) Sweep41Gate(); // NIGHT41 B8 (sweep41): the sweep waits gapS after another attack's tell / strike
            brain.Step(ctx);
            // the window closed: the next leg (legs are places), or in leg 3 the fall
            if (before == Boss37State.Window && brain.state != Boss37State.Window)
            {
                if (brain.leg >= 3) { Fall(); return; }
                brain.NextLeg();
            }
            if (brain.leg != legBefore)
            {
                Say("loom37Leg", "change", -1, stations[Math.Min(2, brain.leg - 1)]);
                brain.SetCheckpoint(brain.leg, me, 0);
            }
            if (brain.ConsumeReposition())
            {
                // the anti-hide: it swings its station over the player (along the canyon only)
                int li = Math.Min(2, brain.leg - 1);
                if (stay40) Stay40Reposition(li, me); // NIGHT40 B4: clamped to the canyon, home again later
                else stations[li] = new Vec3(stations[li].x, stations[li].y, me.z - 6);
            }
            // NIGHT47 L (kneeHold47, with loom47): while a BEND's first holdS runs the body and its knees hold still (no glide, no bob, no dive move)
            bool hold47 = loom47 && holdKnees47 != null && holdKnees47();
            if (hold47) holds47++;
            // glide to the leg's station
            Vec3 goal = stations[Math.Min(2, brain.leg - 1)];
            if (!hold47) {
            if (stay40) Stay40Glide(goal); // NIGHT40 B4: a readable glide with an ease, the centre clamped to the canyon
            else
            {
                double sp = p.stationSpeed.Length > 0 ? p.stationSpeed[Math.Min(p.stationSpeed.Length - 1, brain.leg - 1)] : 5;
                if (sp <= 0) sp = 6; // the nest: it still settles onto its perch
                Vec3 dv = goal - centre;
                double dl = dv.Length, mv = sp * dt;
                centre = dl <= mv ? goal : centre + dv * (mv / dl);
            }
            if (loom47) Loom47Body(me); // NIGHT47 L (loom47; Loom37.N47.cs): the dive's lunge / stick, the window's sink
            Pose();
            } // !hold47
            body.position = centre;
            // the knees: nails fall out after nailHold; the pull-away tear
            bool rope = ropeOut != null && ropeOut();
            for (int i = 0; i < 6; i++)
            {
                Loom37Knee k = knees[i];
                if (k.torn) { k.tornT++; continue; }
                if (k.nails <= 0) continue;
                if (++k.nailAge > S(p.nailHoldS)) { k.nails = 0; k.nearest = 1e9; continue; }
                if (k.nails < p.nailsToTear || !ActiveKnee(i)) continue;
                double d = Vec3.DistanceTo(me, k.at);
                if (d < k.nearest) k.nearest = d;
                if (rope && d - k.nearest >= p.tearDistM) Tear(i, "pull");
                if (brain.Open) break;
            }
            // the attacks: fire once on the active step
            Boss37Attack a = brain.Current;
            if (a != null && brain.entered && brain.state == Boss37State.Tell)
            {
                if (a.id == Lash) { lashFrom = centre; lashTo = me; }
                if (a.id == Web) { webPair = WebPairAhead(me); webs++; }
                if (a.id == Sweep) Sweep41Tell(me); // NIGHT41 B8 (sweep41; Loom37.N41.cs): only in the brain's list while on
            }
            if (a != null && brain.entered && brain.state == Boss37State.Active)
            {
                if (a.id == Lash) LashStrike(me);
                else if (a.id == Spool) ShootSpool(me);
            }
            if (a != null && a.id == Sweep && brain.state == Boss37State.Active) Sweep41Active(me); // NIGHT41 B8: the line moves every active step
            if (loom47 && a != null) Loom47Attacks(a, me); // NIGHT47 L (loom47): the dive, the cage, the bobbins (only in the list while on)
            if (!(a != null && a.id == Web && (brain.state == Boss37State.Tell || brain.state == Boss37State.Active))) webPair = -1;
            StepSpools(me);
        }

        /// <summary>The tower pair ahead of the player (north: smaller z), as the web's index (0..webCount-1), -1 none.</summary>
        public Func<Vec3, int> webAhead;
        int WebPairAhead(Vec3 me) => webAhead != null ? webAhead(me) : -1;

        void LashStrike(Vec3 me)
        {
            lashes++;
            Vec3 d = lashTo - lashFrom;
            double len = d.Length;
            if (len < 1e-6) return;
            Vec3 u = d / len;
            Vec3 rel = me - lashFrom;
            double t = Vec3.Dot(rel, u);
            Vec3 c = lashFrom + u * Math.Max(0, Math.Min(p.lashLength, t));
            double off = Vec3.DistanceTo(c, new Vec3(me.x, me.y + 0.9, me.z));
            bool hit = t > 0 && off <= p.lashWidthM;
            Say("loom37Lash", hit ? "hit" : "miss", -1, lashTo);
            if (!hit) return;
            lashHits++;
            target.health.Damage(p.lashDamage, "loom37");
            target.impulse?.Invoke(u.x * 8, 3, u.z * 8);
        }

        void ShootSpool(Vec3 me)
        {
            spoolsShot++;
            Vec3 from = Spinneret;
            Vec3 aim = new Vec3(me.x, me.y + 1.2, me.z) - from;
            double l = Math.Max(1e-6, aim.Length);
            spools.Add(new Loom37Spool { pos = from, prev = from, vel = aim * (p.spoolSpeed / l), alive = true, life = S(l / p.spoolSpeed + 1.5) });
            Say("loom37Spool", "shot", -1, from);
        }

        void StepSpools(Vec3 me)
        {
            Vec3 chest = new Vec3(me.x, me.y + 1.2, me.z);
            for (int i = spools.Count - 1; i >= 0; i--)
            {
                Loom37Spool s = spools[i];
                s.prev = s.pos;
                if (stay40 && !s.back && !s.bob47) Stay40Steer(s, chest); // NIGHT40 B4: the spool bends toward you (a parry, not a sidestep)
                s.pos = s.pos + s.vel * dt;
                if (--s.life <= 0) { spools.RemoveAt(i); continue; }
                if (s.back)
                {
                    if (Vec3.DistanceTo(s.pos, centre) < p.bodyHalfW + p.spoolRadius)
                    {
                        spools.RemoveAt(i);
                        Say("loom37Spool", "home", -1, centre);
                        parryHit?.Invoke(p.spoolDamage, centre);
                        // NIGHT47 L: a returned spool that kills THE LOOM clears the list (Fall): stop here (the old loop read past the cleared list,
                        // MEASURED: an ArgumentOutOfRangeException in n47lhuman on n47-span; with i == 0 the loop ended anyway, so nothing else changes)
                        if (falling) return;
                    }
                    continue;
                }
                if (Vec3.DistanceTo(s.pos, chest) < p.spoolRadius + 0.6)
                {
                    spools.RemoveAt(i);
                    spoolHits++;
                    target.health.Damage(s.dmg47 > 0 ? s.dmg47 : p.spoolDamage, "loom37"); // NIGHT47 L: a bobbin's own damage (0: a spool)
                    Say("loom37Spool", "hit", -1, s.pos);
                }
            }
        }

        /// <summary>A parried spool lands on the body (the sim routes it through the loadout as a ParryWeapon hit: full damage).</summary>
        public Action<double, Vec3> parryHit;
    }
}
