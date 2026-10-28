// NIGHT47 builder W (wick47, sim row; NIGHT47.md G3 / G4 / G6 / G7; Research/2026-10-18-wave47/W-cards.md section 1): THE WICK in three
// PHASES, behind the wick47 bit (the sim sets `w47` and `p47` before StartFight; off: every line here is skipped, NIGHT46's fight bit for bit).
//   PHASES        the legs, re-read: leg 1 = P1 THE FLOOR (the panels folded into the wall; the bar floors at gate[0], and reaching it starts P2 at
//                 once: the leg's lock is the phase's door, never a silent stop), leg 2 = P2 THE CLIMB (the panels out; the bar floors at gate[1]
//                 until the player stands on the cap), leg 3 = P3 THE CAP (the glass over the shaft). wick47Phase (Wick47Event) at each change.
//   DAMAGE        outside a window a hit lands `chip` of itself (the brain's chipScale is 0.1: GG's "barely damage it"); a shot through a cracked
//                 plate lands in full from the floor too in P1 / P3 (groundCrack); a BARE core (the ring's plates out, a geyser's sink, the
//                 charge's crack-open) takes every hit in full.
//   NEW ATTACKS   SLAG GEYSER (P1, P3; red): the floor's vents nearest the player glow, then erupt: damage + a launch up; the WICK sinks to the
//                 player's height for sinkSeconds after (bare). EMBER CURTAIN (P2, at a ledge: the camp habit; yellow): globs fall round the
//                 player from above (the same parryable embers). PLATE RING (P3; red): the plates spread and sweep out as a ring at chest height
//                 (jump it); the core is bare while they are out. CRACK-OPEN CHARGE (P3; red; the DIVE's place in the kit): it backs to the rim,
//                 a lane lights to the player, it charges along it, stops at the wall and CRACKS OPEN (the core yellow for openSeconds): a melee
//                 hit takes punchShare of the bar and launches the player launchDist back (the finisher: at or under finishAt it falls).
// Pure (no UnityEngine), whole steps, randomness only from the brain's Mulberry32 (none here), no allocation per step (payloads pooled).
// Events (web-style names, for GG's sound): wick47Phase, wick47Geyser, wick47Curtain, wick47Ring, wick47Charge, wick47CrackOpen, wick47CorePunch.
using System;

namespace Skygrind.Core
{
    /// <summary>design.n47.wick47.numbers (each number's _why there).</summary>
    public sealed partial class Wick47Params // NIGHT54 W: partial (Copy54 in Wick37.N54.cs)
    {
        public double[] gate;
        public double chip, above2;
        public bool groundCrack;
        public double geyserRadius, geyserDamage, geyserLaunch, geyserSink;
        public int geyserCount;
        public double curtainHeight, curtainSpeed, curtainRadius;
        public int curtainCount;
        public double ringRadius, ringDamage, ringKnock, ringBelow, ringAbove, ringBare, ringSpeed;
        public double chargeSpeed, chargeDamage, chargeKnock, chargeReach, openSeconds, punchShare, punchReach, launchDist, launchDeg, finishAt, chargeRim;
        public int punches;

        public static Wick47Params From(GameConfig c)
        {
            const string g = "design.n47.wick47.numbers.";
            double N(string k) => c.Num(g + k);
            return new Wick47Params
            {
                gate = c.Nums(g + "gate"), chip = N("chip"), above2 = N("above2"), groundCrack = c.Bool(g + "groundCrack"),
                geyserRadius = N("geyser.radius"), geyserDamage = N("geyser.damage"), geyserLaunch = N("geyser.launch"), geyserSink = N("geyser.sink"), geyserCount = c.Int(g + "geyser.count"),
                curtainHeight = N("curtain.height"), curtainSpeed = N("curtain.speed"), curtainRadius = N("curtain.radius"), curtainCount = c.Int(g + "curtain.count"),
                ringRadius = N("ring.radius"), ringDamage = N("ring.damage"), ringKnock = N("ring.knock"), ringBelow = N("ring.below"), ringAbove = N("ring.above"), ringBare = N("ring.bare"), ringSpeed = N("ring.speed"),
                chargeSpeed = N("charge.speed"), chargeDamage = N("charge.damage"), chargeKnock = N("charge.knock"), chargeReach = N("charge.reach"), openSeconds = N("charge.openSeconds"),
                punchShare = N("charge.punchShare"), punchReach = N("charge.punchReach"), launchDist = N("charge.launchDist"), launchDeg = N("charge.launchDeg"), finishAt = N("charge.finishAt"),
                chargeRim = N("charge.rim"), punches = c.Int(g + "charge.punches"),
            };
        }
    }

    /// <summary>The payload of every wick47* event (pooled: read it, don't keep it).</summary>
    public sealed class Wick47Event
    {
        public string stage;
        public int phase;
        public Vec3 at, to;
        public double radius, value;
    }

    public sealed partial class Wick37
    {
        public const string Geyser47 = "wickGeyser", Curtain47 = "wickCurtain", Ring47 = "wickRing", Charge47 = "wickCharge";
        /// <summary>wick47 on for this fight (the sim sets it, p47 and the vents, before StartFight).</summary>
        public bool w47;
        public Wick47Params p47;
        /// <summary>The floor's vents (arena-local, on the floor) and the cap's (on the catwalk): the geyser's places.</summary>
        public Vec3[] vents47 = new Vec3[0], capVents47 = new Vec3[0];
        /// <summary>The sim's player launch (vertical, then horizontal x / z: Mover.Launch).</summary>
        public Action<double, double, double> launchPlayer;
        readonly Wick47Event e47 = new Wick47Event();

        /// <summary>The phase (1-3) with wick47 on (the leg); 0 off.</summary>
        public int Phase47 => w47 && brain != null ? brain.leg : 0;
        /// <summary>The core is open (the charge's crack): a melee hit is the big one. Steps left (the view's ring clock).</summary>
        public int coreOpen47, bare47;
        public int coreOpenSteps47 => p47 != null ? S(p47.openSeconds) : 1;
        /// <summary>The live geysers (arena-local positions, the eruption countdown) and the ring's radius (0: none).</summary>
        public readonly Vec3[] geyserAt47 = new Vec3[4];
        public int geyserN47;
        public double ring47;
        public Vec3 chargeFrom47, chargeTo47;
        bool chargeHit47, ringHit47;
        // counts for the harness and the reels
        public int phases47, geysers47, geyserHits47, curtains47, rings47, ringHits47, charges47, chargeHits47, opens47, corePunches47, coreMissed47;
        public double lastLaunch47;

        /// <summary>NIGHT47 W G9 (wickRideParry47; the sim sets these before StartFight): a parry from a ride returns the ember speedMult x faster
        /// and it cracks `cracks` plates (the one it hits and the next).</summary>
        public bool rideParry47;
        public double rideMult47 = 1.5;
        public int rideCracks47 = 2, rideParries47, rideCracksMade47;

        void RideParry47(Wick37Ember ob)
        {
            ob.vel = ob.vel * rideMult47;
            ob.ride47 = true;
            rideParries47++;
        }

        void RideCrack47(int plate)
        {
            for (int k = 1; k < rideCracks47; k++)
            {
                int q = (plate + k) % Plates;
                crackLeft[q] = S(p.crackSeconds);
                rideCracksMade47++;
                fx.stage = "crack";
                fx.plate = q;
                fx.at = Centre;
                events.Emit("wick37Crack", fx);
            }
        }

        void StartFight47()
        {
            coreOpen47 = bare47 = geyserN47 = 0;
            ring47 = 0;
            rideParries47 = rideCracksMade47 = 0;
            phases47 = geysers47 = geyserHits47 = curtains47 = rings47 = ringHits47 = charges47 = chargeHits47 = opens47 = corePunches47 = coreMissed47 = 0;
            lastLaunch47 = 0;
            if (!w47 || p47 == null || brain == null) return;
            // the kit: the DIVE leaves (the CHARGE takes its place in P3), the SLAM stays on the floor (P1; the CURTAIN is P2's camp answer)
            foreach (Boss37Attack a in brain.attacks)
            {
                if (a.id == Dive) a.legMask = 0;
                if (a.id == Slam) a.legMask = 1;
            }
            foreach (string id in new[] { "geyser", "curtain", "ring", "charge" })
                brain.attacks.Add(Boss37Attack.From(cfg, "design.n47.wick47.numbers.attacks." + id, dt0));
            hpLock = p47.gate.Length > 0 ? p47.gate[0] : hpLock;
            // P1 and P2 restart on the floor (by the entry gap, +z), not on ledge 1 (P1 has no panels to reach it)
            Vec3 floor = origin + new Vec3(0, 0, stack.p.radius - 2.5);
            brain.SetCheckpoint(1, floor, 0);
            brain.SetCheckpoint(2, floor, 0);
        }

        /// <summary>wick47: the leg changes are phases: P1 -> P2 when the bar reaches gate[0]; P2 -> P3 when the player stands on the cap.</summary>
        void ReachedFloor47(double y)
        {
            if (brain == null || !body.alive) return;
            if (brain.leg == 2 && y >= stack.p.capY - 0.1) PhaseTo47(3);
        }

        void PhaseTo47(int to)
        {
            if (brain.leg >= to || brain.leg >= 3) return;
            brain.NextLeg();
            if (brain.leg != to) return;
            hpLock = p47.gate.Length >= to ? p47.gate[to - 1] : 0; // P2: until the cap; P3: the guns stop at gate[2], only the core punch finishes it
            if (to == 3) { circleAngle = JsMath.Atan2(target.position().z - origin.z, target.position().x - origin.x) + Math.PI; local = new Vec3(JsMath.Cos(circleAngle) * p.circleR, stack.p.capY + p.circleY, JsMath.Sin(circleAngle) * p.circleR); } // the transition beat: it shoots up out of the shaft and hangs over the glass at once (round 2: it rose from under the glass for ~3 s)
            coreOpen47 = bare47 = 0;
            geyserN47 = 0;
            ring47 = 0;
            phases47++;
            e47.stage = "phase";
            e47.phase = to;
            e47.at = Centre;
            e47.value = HpFrac;
            events.Emit("wick47Phase", e47);
        }

        /// <summary>wick47's share of a hit (Filter): full when the core is bare / open or through a crack from the floor (P1 / P3), else chip.</summary>
        bool Filter47(double amount, string weapon, Vec3 from, out double share)
        {
            share = 0;
            if (!w47 || p47 == null) return false;
            if (bare47 > 0 || coreOpen47 > 0) { fulls++; share = amount; return true; }
            bool charged = weapon == "revolver" && (amount >= p.chargedMin || p.anyShotFromRide);
            Vec3 c = Centre;
            int plate = PlateAt(JsMath.Atan2(from.z - c.z, from.x - c.x));
            if (charged && crackLeft[plate] > 0 && (RidingNow || (p47.groundCrack && brain.leg != 2)))
            {
                pierces++;
                fulls++;
                dealtRight += amount;
                crackLeft[plate] = 0;
                brain.Stagger(amount >= p.chargedMin ? p.pierceStagger * 2 : p.pierceStagger, "pierce");
                fx.stage = "pierce";
                fx.plate = plate;
                fx.at = c;
                events.Emit("wick37Crack", fx);
                share = amount;
                return true;
            }
            chips++;
            share = amount * p47.chip;
            return true;
        }

        /// <summary>wick47: a punch at the OPEN core (the charge's crack-open) within punchReach of the cage: punchShare of the bar (the finisher at
        /// or under finishAt), the player launched launchDist back from the core at launchDeg up.</summary>
        bool CorePunch47(Vec3 eye)
        {
            if (!w47 || p47 == null || coreOpen47 <= 0 || !body.alive) return false;
            Vec3 c = Centre;
            if (Vec3.DistanceTo(eye, c) > p47.punchReach + p.cageR) { coreMissed47++; return false; }
            corePunches47++;
            coreOpen47 = 0;
            double dmg = p47.punchShare * body.maxHp, before = body.hp;
            Vec3 pw = target.position();
            double dx = pw.x - c.x, dz = pw.z - c.z, h = JsMath.Hypot(dx, dz);
            if (h < 1e-6) { dx = -(c.x - origin.x); dz = -(c.z - origin.z); h = Math.Max(1e-6, JsMath.Hypot(dx, dz)); }
            // the launch: back from the core over the glass (a ballistic range of launchDist at launchDeg, the game's gravity)
            double g = MovementMath.GravityFor(cfg.movement.jump.height, cfg.movement.jump.apexTime), ang = p47.launchDeg * Math.PI / 180;
            double v = Math.Sqrt(p47.launchDist * g / Math.Max(0.1, JsMath.Sin(2 * ang)));
            double vh = v * JsMath.Cos(ang), vy = v * JsMath.Sin(ang);
            launchPlayer?.Invoke(vy, dx / h * vh, dz / h * vh);
            lastLaunch47 = v;
            e47.stage = "punch";
            e47.phase = 3;
            e47.at = c;
            e47.to = pw;
            e47.value = p47.punchShare;
            events.Emit("wick47CorePunch", e47);
            if (corePunches47 >= p47.punches && body.hp - dmg <= body.maxHp * p47.finishAt) // the finisher: only from the punches-th punch on (P3 is that many charges)
            {
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

        bool StartTell47(string id, Vec3 pw)
        {
            if (!w47 || p47 == null) return false;
            Vec3 pl = pw - origin;
            if (id == Geyser47)
            {
                // the vents nearest the player (P1: the floor's, P3: the cap's)
                Vec3[] vs = brain.leg == 3 ? capVents47 : vents47;
                geyserN47 = 0;
                int want = Math.Min(Math.Min(p47.geyserCount, geyserAt47.Length), vs.Length);
                for (int n = 0; n < want; n++)
                {
                    int best = -1; double bd = 1e18;
                    for (int i = 0; i < vs.Length; i++)
                    {
                        bool used = false;
                        for (int k = 0; k < geyserN47; k++) if (geyserAt47[k].x == vs[i].x && geyserAt47[k].z == vs[i].z) used = true;
                        if (used) continue;
                        double d = (vs[i].x - pl.x) * (vs[i].x - pl.x) + (vs[i].z - pl.z) * (vs[i].z - pl.z);
                        if (d < bd) { bd = d; best = i; }
                    }
                    if (best >= 0) geyserAt47[geyserN47++] = vs[best];
                }
                geysers47++;
                for (int k = 0; k < geyserN47; k++) { e47.stage = "tell"; e47.at = origin + geyserAt47[k]; e47.radius = p47.geyserRadius; events.Emit("wick47Geyser", e47); }
                return true;
            }
            if (id == Curtain47)
            {
                curtains47++;
                e47.stage = "tell"; e47.at = brain.campAnchor; e47.radius = p47.curtainRadius;
                events.Emit("wick47Curtain", e47);
                return true;
            }
            if (id == Ring47)
            {
                rings47++;
                ring47 = 0;
                ringHit47 = false;
                e47.stage = "tell"; e47.at = Centre; e47.radius = p47.ringRadius;
                events.Emit("wick47Ring", e47);
                return true;
            }
            if (id == Charge47)
            {
                charges47++;
                chargeHit47 = false;
                // the lane: from the rim opposite the player, through him, to the wall
                double r = Math.Max(1e-6, JsMath.Hypot(pl.x, pl.z)), ux = pl.x / r, uz = pl.z / r;
                double rim = stack.p.capRadius - p47.chargeRim;
                if (r < 2) { double a = circleAngle; ux = JsMath.Cos(a); uz = JsMath.Sin(a); }
                chargeFrom47 = new Vec3(-ux * rim, stack.p.capY + 1.6, -uz * rim);
                chargeTo47 = new Vec3(ux * rim, stack.p.capY + 1.6, uz * rim);
                e47.stage = "tell"; e47.at = origin + chargeFrom47; e47.to = origin + chargeTo47;
                events.Emit("wick47Charge", e47);
                return true;
            }
            return false;
        }

        bool StartActive47(string id, Vec3 pw)
        {
            if (!w47 || p47 == null) return false;
            if (id == Geyser47)
            {
                Capsule cap = PlayerCapsule();
                for (int k = 0; k < geyserN47; k++)
                {
                    Vec3 g = origin + geyserAt47[k];
                    double dx = cap.start.x - g.x, dz = cap.start.z - g.z;
                    if (dx * dx + dz * dz <= p47.geyserRadius * p47.geyserRadius && cap.start.y - g.y < 2.5 && cap.start.y - g.y > -1)
                    {
                        if (HurtPlayer(p47.geyserDamage)) geyserHits47++;
                        launchPlayer?.Invoke(p47.geyserLaunch, double.NaN, double.NaN);
                    }
                    e47.stage = "erupt"; e47.at = g; e47.radius = p47.geyserRadius;
                    events.Emit("wick47Geyser", e47);
                }
                bare47 = S(p47.geyserSink); // it sinks to the player's height: bare
                return true;
            }
            if (id == Curtain47)
            {
                Vec3 a = brain.campAnchor;
                int n = Math.Max(1, p47.curtainCount);
                for (int i = 0; i < n; i++)
                {
                    double ang = i * 2 * Math.PI / n;
                    var at = new Vec3(a.x + JsMath.Cos(ang) * p47.curtainRadius, a.y + p47.curtainHeight, a.z + JsMath.Sin(ang) * p47.curtainRadius);
                    embers.Add(new Wick37Ember { pos = at, prev = at, vel = new Vec3(0, -p47.curtainSpeed, 0), life = S(p.emberLife), alive = true });
                }
                e47.stage = "fall"; e47.at = a; e47.radius = p47.curtainRadius;
                events.Emit("wick47Curtain", e47);
                return true;
            }
            if (id == Ring47)
            {
                ring47 = p.cageR;
                bare47 = S(p47.ringBare);
                e47.stage = "sweep"; e47.at = Centre; e47.radius = p47.ringRadius;
                events.Emit("wick47Ring", e47);
                return true;
            }
            if (id == Charge47)
            {
                e47.stage = "charge"; e47.at = Centre; e47.to = origin + chargeTo47;
                events.Emit("wick47Charge", e47);
                return true;
            }
            return false;
        }

        /// <summary>The charge's end (Active -> Recover): it cracks open where it stopped.</summary>
        void Recover47()
        {
            if (!w47 || p47 == null || AttackId != Charge47) return;
            coreOpen47 = S(p47.openSeconds);
            opens47++;
            e47.stage = "open"; e47.at = Centre; e47.value = p47.openSeconds;
            events.Emit("wick47CrackOpen", e47);
        }

        /// <summary>Per step (after the brain): the phase gate, the clocks, the ring's sweep, the charge's hit.</summary>
        void Step47(Vec3 pw, double dt)
        {
            if (!w47 || p47 == null || brain == null || !body.alive) return;
            if (brain.leg == 1 && p47.gate.Length > 0 && body.hp <= body.maxHp * p47.gate[0] + 1e-9 && brain.Fighting) PhaseTo47(2);
            if (coreOpen47 > 0) coreOpen47--;
            if (bare47 > 0) bare47--;
            if (ring47 > 0)
            {
                ring47 += p47.ringSpeed * dt;
                Capsule cap = PlayerCapsule();
                Vec3 c = Centre;
                double d = JsMath.Hypot(cap.start.x - c.x, cap.start.z - c.z), fy = stack.p.capY + origin.y;
                bool inBand = cap.start.y - fy < p47.ringAbove && cap.end.y - fy > p47.ringBelow;
                if (!ringHit47 && inBand && Math.Abs(d - ring47) < 0.6)
                {
                    ringHit47 = true;
                    if (HurtPlayer(p47.ringDamage)) ringHits47++;
                    double h = Math.Max(1e-6, d);
                    launchPlayer?.Invoke(4, (cap.start.x - c.x) / h * p47.ringKnock, (cap.start.z - c.z) / h * p47.ringKnock);
                }
                if (ring47 >= p47.ringRadius + stack.p.capRadius) ring47 = 0;
            }
            if (brain.state == Boss37State.Active && AttackId == Charge47 && !chargeHit47)
            {
                Capsule cap = PlayerCapsule();
                if (SegDist(Centre, cap.start, cap.end) < p47.chargeReach + cap.radius)
                {
                    chargeHit47 = true;
                    if (HurtPlayer(p47.chargeDamage)) chargeHits47++;
                    Vec3 c = Centre;
                    double dx = cap.start.x - c.x, dz = cap.start.z - c.z, h = Math.Max(1e-6, JsMath.Hypot(dx, dz));
                    launchPlayer?.Invoke(5, dx / h * p47.chargeKnock, dz / h * p47.chargeKnock);
                }
            }
        }

        /// <summary>wick47's movement where it differs (true: handled): P2 floats above2 over the climber; the charge's tell (back to the rim),
        /// its run along the lane, and the open core where it stopped; the geyser's sink.</summary>
        bool Move47(Vec3 pw, double dt)
        {
            if (!w47 || p47 == null) return false;
            Vec3 pl = pw - origin;
            if (brain.leg == 2 && brain.state != Boss37State.Reveal)
            {
                double want = Math.Min(stack.p.capY - 2, Math.Max(slag != null ? slag.y + 2 : 1, pl.y + p47.above2));
                double stp = p.followRate * dt, dy = want - local.y;
                local = new Vec3(local.x * 0.95, local.y + Math.Max(-stp, Math.Min(stp, dy)), local.z * 0.95);
                return true;
            }
            string id = AttackId;
            if (brain.leg == 3 && id == Charge47 && (brain.state == Boss37State.Tell || brain.state == Boss37State.Active || brain.state == Boss37State.Recover))
            {
                Vec3 goal = brain.state == Boss37State.Tell ? chargeFrom47 : chargeTo47;
                double spd = brain.state == Boss37State.Tell ? p.diveSpeed * 0.6 : brain.state == Boss37State.Active ? p47.chargeSpeed : 0;
                if (spd <= 0) return true; // open: it stays where it stopped
                Vec3 d = goal - local;
                double len = d.Length, stp = spd * dt;
                local = len <= stp ? goal : local + d * (stp / len);
                return true;
            }
            if (bare47 > 0 && id == Geyser47)
            {
                double want = pl.y + 1.4; // the core level with the player's head
                double stp = p.followRate * dt, dy = want - local.y;
                local = new Vec3(local.x, local.y + Math.Max(-stp, Math.Min(stp, dy)), local.z);
                return true;
            }
            return false;
        }
    }
}
