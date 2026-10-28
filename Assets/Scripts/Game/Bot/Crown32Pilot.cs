// NIGHT32 builder C (boss5): THE CROWN's scripted pilots, shared by the harness (crown32) and the Player reel (FeelReel.N32.Crown.cs boss5).
//  - Crown32Pilot: the MOVING pilot with all four weapons, a human-lag model on the real Mover: circles the ring (beat 1 at 9 m from the middle, beat 2 on the inner disc's rim,
//    beat 3 at 10 m), swaps the circling side every 6 s; jumps the beam when it is about a third of a second away; parries an orb with the PULSE gauntlet's LT when one comes within
//    reach (it switches to the glove when an orb is close); steers off a sector that glows or is down; reads the beat-3 charge tell after reactSeconds and sprints and dashes off the
//    line, then runs to the stunned core's open plate. Guns by range: the THUNDERCLAP inside 8 m, the NAILGUN to 16 m, the KESTREL beyond; it aims at the open plate that faces it
//    most, else the core.
//  - Crown32Camper: a CAMPER (DESIGN32 rule 2): holds its spot (walks back when knocked off), aims at the core and fires its best weapon by range (the camp31 rule).
// Numbers are bot-side: they shape one scripted opponent and change no rule. No UnityEngine.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class Crown32Pilot
    {
        public InputFrame Move;
        public CombatInput Combat;
        public int jumps, parries, dodges, swaps, detonates;
        bool lastHeldCharge;
        public string gun = "revolver", jumpNote = "";
        int tellSeen = -1, side = 1, dodgeSide = 1, parryHold, lastJump = -1000, lastPitDash = -1000;
        double reactSeconds = 0.3;
        // the human model (bot-side, seeded per pilot with the ported Mulberry32): an aim error that wanders every aimEverySteps (up to aimErrorM metres off the point),
        // a reaction time between 0.25 and 0.4 s, the first circling side
        readonly Mulberry32 rng;
        Vec3 aimOff;
        const double aimErrorM = 1.6;
        const int aimEverySteps = 30;

        public Crown32Pilot(uint seed = 1)
        {
            rng = new Mulberry32(seed * 2654435761u + 12345u);
            side = rng.Next() < 0.5 ? 1 : -1;
            reactSeconds = 0.25 + 0.15 * rng.Next();
        }

        static double Wrap(double a)
        {
            while (a < 0) a += 2 * Math.PI;
            while (a >= 2 * Math.PI) a -= 2 * Math.PI;
            return a;
        }

        public void Step(SurvivalSim sim, int step)
        {
            Crown32 cr = sim.crown32;
            Crown32Arena a = cr.arena;
            double dt = sim.cfg.Dt;
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z), o = cr.origin;
            Vec3 c = cr.Centre;
            // the aim: the open plate that faces the player most, else the core
            Vec3 aim = c;
            if (cr.PlatesOpen)
            {
                double bestDot = 0.35;
                Vec3 to = Vec3.Normalize(new Vec3(me.x - c.x, 0, me.z - c.z));
                for (int k = 0; k < cr.p.plateCount; k++)
                {
                    double dd = Vec3.Dot(cr.PlateNormal(k), to);
                    if (dd > bestDot) { bestDot = dd; aim = cr.PlateAt(k); }
                }
            }
            if (step % aimEverySteps == 0)
            {
                double ang = rng.Next() * 2 * Math.PI, r = aimErrorM * Math.Sqrt(rng.Next()), up = (rng.Next() - 0.5) * aimErrorM;
                aimOff = new Vec3(JsMath.Cos(ang) * r, up, JsMath.Sin(ang) * r);
            }
            Vec3 d = aim + aimOff - eye;
            double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
            Vec3 fwd = MovementMath.WishDirection(0, 1, wy), right = MovementMath.WishDirection(1, 0, wy);
            if (step % 720 == 0 && step > 0) { side = -side; swaps++; }
            Vec3 lm = me - o;
            double lr = JsMath.Hypot(lm.x, lm.z);
            Vec3 radial = lr > 1e-6 ? new Vec3(lm.x / lr, 0, lm.z / lr) : new Vec3(0, 0, 1);
            // circle the arena's middle (beats 1 and 3) or, in beat 2, the orbiting core itself (6.5 m off it, kept off the wall)
            Vec3 pivot = cr.beat == 2 ? new Vec3(c.x - o.x, 0, c.z - o.z) : default;
            double px = lm.x - pivot.x, pz = lm.z - pivot.z, pr = JsMath.Hypot(px, pz);
            Vec3 pradial = pr > 1e-6 ? new Vec3(px / pr, 0, pz / pr) : radial;
            Vec3 tangent = new Vec3(-pradial.z * side, 0, pradial.x * side);
            Vec3 want;
            bool sprint = false, dash = false, jump = false;
            double ring = cr.beat == 2 ? 6.5 : cr.beat == 3 ? 10 : 9;
            want = tangent + pradial * Math.Max(-1, Math.Min(1, (ring - pr) * 0.5));
            if (lr > a.p.apothem - 3) want = want - radial * 1.5;
            sprint = cr.beat == 2;
            if (cr.beat == 3 && (cr.ChargeTelling || cr.Charging))
            {
                if (tellSeen < 0)
                {
                    tellSeen = step;
                    double cross = (me.x - c.x) * -cr.chargeDir.z + (me.z - c.z) * cr.chargeDir.x;
                    dodgeSide = cross >= 0 ? 1 : -1;
                }
                if ((step - tellSeen) * dt >= reactSeconds)
                {
                    want = new Vec3(-cr.chargeDir.z * dodgeSide, 0, cr.chargeDir.x * dodgeSide);
                    sprint = true;
                    if ((step - tellSeen) * dt < reactSeconds + dt * 1.5) { dash = true; dodges++; }
                }
                else want = default;
            }
            else
            {
                tellSeen = -1;
                if (cr.Stunned)
                {
                    // run to 3.5 m in front of the plate that faces it most
                    int bk = 0;
                    double bd = -2;
                    Vec3 to = Vec3.Normalize(new Vec3(me.x - c.x, 0, me.z - c.z));
                    for (int k = 0; k < cr.p.plateCount; k++) { double dd = Vec3.Dot(cr.PlateNormal(k), to); if (dd > bd) { bd = dd; bk = k; } }
                    Vec3 spot = c + cr.PlateNormal(bk) * 3.5;
                    double sx = spot.x - me.x, sz = spot.z - me.z, sl = JsMath.Hypot(sx, sz);
                    want = sl > 0.8 ? new Vec3(sx / sl, 0, sz / sl) : default;
                    sprint = sl > 4;
                }
            }
            // beat 2: off a sector that glows or is down (toward the inner disc)
            if (cr.beat == 2 && !a.InInner(lm.x, lm.z) && a.WallDistance(lm.x, lm.z) > 0)
            {
                int s = a.SectorAt(lm.x, lm.z), bad = cr.downMask | cr.dropTellMask, n = a.p.sectors;
                if ((bad & (1 << s)) != 0)
                {
                    // the shortest way off: inward to the disc, or sideways into a neighbour sector that is safe
                    double ang = JsMath.Atan2(lm.z, lm.x);
                    if (ang < 0) ang += 2 * Math.PI;
                    double toLo = (ang - s * a.SectorAngle) * lr, toHi = ((s + 1) * a.SectorAngle - ang) * lr, inward = lr - a.p.innerRadius;
                    bool loSafe = (bad & (1 << ((s - 1 + n) % n))) == 0, hiSafe = (bad & (1 << ((s + 1) % n))) == 0;
                    Vec3 ccw = new Vec3(-radial.z, 0, radial.x); // increasing angle
                    want = -radial;
                    double best = inward;
                    if (hiSafe && toHi + 1 < best) { best = toHi + 1; want = ccw; }
                    if (loSafe && toLo + 1 < best) { want = -ccw; }
                    sprint = true;
                }
                // in a pit: jump out toward the disc
                if (lm.y < -0.2)
                {
                    want = -radial;
                    sprint = true;
                    if (sim.mover.grounded && step - lastJump > 30) { jump = true; lastJump = step; }
                    if (!sim.mover.grounded && step - lastPitDash > 60) { dash = true; lastPitDash = step; }
                }
            }
            // the beam: jump when it is about a third of a second away (the jump clears beamHigh between about 0.25 and 0.47 s after the press)
            if (cr.Sweeping && sim.mover.grounded)
            {
                double pa = JsMath.Atan2(me.z - c.z, me.x - c.x);
                double ahead = Wrap((pa - cr.sweepAngle) * cr.sweepDir);
                // the closing rate: the beam's turn less the player's own turn round the core in the beam's direction
                double rx = me.x - c.x, rz = me.z - c.z, rr = Math.Max(1, rx * rx + rz * rz);
                Vec3 pv = sim.mover.velocity;
                double omega = (rx * pv.z - rz * pv.x) / rr;
                double closing = cr.p.sweepRate - cr.sweepDir * omega;
                double tReach = closing > 0.05 ? ahead / closing : 99;
                if (tReach > 0.31 && tReach < 0.39 && step - lastJump > 72) { jump = true; jumps++; lastJump = step; jumpNote = $"jump at step {step}: tReach {tReach:0.00} ahead {ahead:0.00} omega {omega:0.00} r {Math.Sqrt(rr):0.0}"; }
            }
            // an orb near: the glove's parry (LT) when it is within the reflect reach; switch to the glove when one is within 7 m
            bool orbNear = false, orbReach = false;
            foreach (Crown32Orb ob in cr.orbs)
            {
                if (!ob.alive) continue;
                double od = Vec3.DistanceTo(ob.pos, eye);
                Vec3 rel = eye - ob.pos;
                if (Vec3.Dot(rel, ob.vel) <= 0) continue;
                if (od < 7) orbNear = true;
                if (od < 1.9) orbReach = true;
            }
            double dist = d.Length;
            // the kit rotation (each special is a plate answer: design.n32.boss5.kit): every 6 s, NAILGUN 2 s then its DETONATE, a charged KESTREL shot (held until it
            // fires itself), then the THUNDERCLAP inside 10 m or KESTREL taps; a kit without a gun falls back to the KESTREL
            double cyc = (step % 720) * dt;
            bool detonate = false, chargeHold = false, chargeGap = false;
            if (cyc < 2.0) gun = "nailgun";
            else if (cyc < 2.2) { gun = "nailgun"; detonate = cyc < 2.0 + dt * 1.5; }
            else if (cyc < 3.7) { gun = "revolver"; chargeHold = true; }
            else if (cyc < 3.85) { gun = "revolver"; chargeGap = true; } // let go of the trigger: the charge hold carries across a KESTREL -> THUNDERCLAP swap (a slug by accident)
            else gun = dist < 10 ? "shotgun" : "revolver";
            if (!sim.loadout.owned.Contains(gun)) gun = "revolver";
            if (gun == "revolver" && !chargeHold && !sim.loadout.owned.Contains("nailgun")) chargeHold = (step % 480) < 170; // the pistol + fist kit charges every 4 s
            bool parry = false;
            if (orbNear && sim.loadout.owned.Contains("gauntlet")) { gun = "gauntlet"; parryHold = 30; }
            else if (parryHold > 0) { parryHold--; gun = "gauntlet"; }
            if (gun == "gauntlet" && orbReach && sim.loadout.Weapon == "gauntlet") { parry = true; parries++; }
            if (!sim.loadout.owned.Contains(gun)) gun = "revolver";
            double wl = JsMath.Hypot(want.x, want.z);
            Move = new InputFrame { yaw = wy, pitch = wp };
            if (wl > 1e-6)
            {
                Move.moveZ = (want.x * fwd.x + want.z * fwd.z) / Math.Max(1, wl);
                Move.moveX = (want.x * right.x + want.z * right.z) / Math.Max(1, wl);
            }
            Move.sprint = new Button { held = sprint, pressed = sprint && step % 60 == 0 };
            Move.dash = new Button { held = dash, pressed = dash };
            Move.jump = new Button { held = jump || step - lastJump < 48, pressed = jump }; // held through the rise (an early release cuts the jump: jump.releaseMult)
            int gi = sim.loadout.owned.IndexOf(gun);
            bool canShoot = gun != "gauntlet" && cr.Hurtable && !chargeGap;
            bool charging = gun == "revolver" && chargeHold;
            bool held = gun == "nailgun" || charging ? canShoot : canShoot && (step / 2) % 2 == 0;
            bool pressed = charging ? canShoot && !lastHeldCharge : canShoot && step % 4 == 0;
            lastHeldCharge = charging && canShoot;
            if (detonate && gun == "nailgun" && sim.loadout.Weapon == "nailgun") detonates++;
            bool det = detonate && sim.loadout.Weapon == "nailgun";
            Combat = new CombatInput { weapon = sim.loadout.Weapon != gun ? gi : -1, fire = new Button { held = held && !det, pressed = pressed && !det }, altFire = new Button { held = parry, pressed = parry }, special = new Button { held = det, pressed = det } };
        }
    }

    public sealed class Crown32Camper
    {
        public InputFrame Move;
        public CombatInput Combat;
        public string gun = "revolver";
        public bool see;
        public double dist;

        public void Step(SurvivalSim sim, Vec3 spot, int step, bool fireOn)
        {
            Crown32 cr = sim.crown32;
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z), d = cr.Centre - eye;
            double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
            Move = new InputFrame { yaw = wy, pitch = wp };
            double cx = spot.x - me.x, cz = spot.z - me.z, cl = JsMath.Hypot(cx, cz);
            if (cl > 1.0)
            {
                Vec3 fwd = MovementMath.WishDirection(0, 1, wy), right = MovementMath.WishDirection(1, 0, wy);
                Move.moveZ = (cx * fwd.x + cz * fwd.z) / cl;
                Move.moveX = (cx * right.x + cz * right.z) / cl;
            }
            dist = d.Length;
            Vec3 dn = Vec3.Normalize(d);
            double tHit = cr.Raycast(eye, dn);
            see = tHit >= 0 && !(sim.world.RayIntersect(eye, dn, out RayHit wh) && wh.distance < tHit);
            double reach = sim.cfg.Num("weapons.gauntlet.range");
            gun = dist < 12 ? "shotgun" : "nailgun";
            if (tHit >= 0 && tHit <= reach) gun = "gauntlet";
            if (!sim.loadout.owned.Contains(gun)) gun = "revolver";
            int gi = sim.loadout.owned.IndexOf(gun);
            bool fire = fireOn && see;
            Combat = new CombatInput { weapon = sim.loadout.Weapon != gun ? gi : -1, fire = new Button { held = fire, pressed = fire && step % 2 == 0 } };
        }
    }
}
