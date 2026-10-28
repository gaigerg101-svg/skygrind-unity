// NIGHT27 builder B (boss1): the scripted pilot that fights THE LANTERN the way the fight asks (Core/Enemies/Lantern27.cs): it runs the ring along the wall, jumps into the wall at speed so the Mover
// starts a WALL RIDE (the Mover's own rule: airborne, touching a wall, along-wall speed over entrySpeed; nothing is forced), and shoots the face only while it is lit (LitSegment >= 0: the lit
// cyan face is what a human sees); while the shield is up it shoots a SENTRY if one is in sight. The aim slews at a fixed rate and the gun fires only when the ray lands (a human's limits).
// State lives here; the pilot calls Step once per fixed step and reads Move / Combat. Numbers are bot-side, not gameplay: they shape one scripted opponent and change no rule.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class LanternPilot27
    {
        public InputFrame Move;
        public CombatInput Combat;
        public int rides, jumps, wallJumps;
        double yawL, pitchL;
        int stepNo = -1, jumpHold, jumpCool, dirSign = 1, airSteps;
        bool started;
        MoveState prev = MoveState.Ground;
        const double AimRate = 9.0; // rad/s the aim slews at (a ride is 1.2 s long: the aim has to arrive inside it)
        const double RunLane = 4.6; // m from the wall the run keeps
        const double RunUp = 9.0; // m of wall ahead a jump into it needs
        const double MinSpeed = 6.5; // m/s along the wall before a jump
        public bool useGauntlet;
        public string dbg;
        /// <summary>The human's hand (a seeded Mulberry32: the run-up distance of each jump and the aim rate vary a little); null: none.</summary>
        public Mulberry32 hand;
        double jumpOff = 2.1, aimRate = AimRate, stuckX, stuckZ;
        int unstick;

        public void Step(SurvivalSim sim, double yawNow, int tick)
        {
            if (tick == stepNo) return;
            stepNo = tick;
            Lantern27 l = sim.lantern27;
            double dt = sim.step;
            if (!started) { started = true; yawL = yawNow; pitchL = 0; }
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            Vec3 c = l.Centre;
            double R = l.p.arenaHalf;
            double dx = me.x - c.x, dz = me.z - c.z;
            // the nearest wall: its inward normal and the direction round the ring
            double ax = Math.Abs(dx), az = Math.Abs(dz);
            double nx, nz, tx, tz, along, off;
            if (ax >= az) { nx = dx > 0 ? -1 : 1; nz = 0; tx = 0; tz = dx > 0 ? dirSign : -dirSign; along = tz * dz; off = R - ax; }
            else { nx = 0; nz = dz > 0 ? -1 : 1; tx = dz > 0 ? -dirSign : dirSign; tz = 0; along = tx * dx; off = R - az; }
            double vAlong = sim.mover.velocity.x * tx + sim.mover.velocity.z * tz;
            MoveState st = sim.mover.state;
            if (st == MoveState.WallRide && prev != MoveState.WallRide) rides++;
            if (st == MoveState.Air || st == MoveState.WallRide) airSteps++; else airSteps = 0;
            prev = st;
            double wx, wz;
            bool jump = false;
            if (jumpCool > 0) jumpCool--;
            if (st == MoveState.WallRide)
            {
                wx = tx + (-nx) * 0.25;
                wz = tz + (-nz) * 0.25;
            }
            else if (!sim.mover.grounded)
            {
                // in the air after the jump: along the wall and into it
                wx = tx * 0.6 + (-nx) * 0.9;
                wz = tz * 0.6 + (-nz) * 0.9;
            }
            else
            {
                // the air barely steers (design: air accel), so the run itself is angled into the wall and the jump is taken close to it
                // never jump at a wall that is gone or about to go (phase 3): the sectors a ride would cross, here and 6 m on
                int gone = l.retractedMask | l.retractWarnMask;
                bool wallOk = gone == 0 || ((gone & (1 << Lantern27Shield.SegmentAt(me.x - c.x, me.z - c.z, l.p.segments))) == 0 && (gone & (1 << Lantern27Shield.SegmentAt(me.x + tx * 6 - c.x, me.z + tz * 6 - c.z, l.p.segments))) == 0);
                dbg = $"vAlong {vAlong:0.0} runway {R - along:0.0} off {off:0.0} cool {jumpCool} wallOk {wallOk} dir {dirSign} unstick {unstick}";
                bool ready = vAlong > MinSpeed && R - along > RunUp && jumpCool == 0 && wallOk;
                if (ready)
                {
                    wx = tx * 0.85 - nx * 0.55;
                    wz = tz * 0.85 - nz * 0.55;
                    double vn = -(sim.mover.velocity.x * nx + sim.mover.velocity.z * nz); // m/s into the wall: the air barely steers, so the jump waits for it
                    if (off < 1.1 && vn < 4.0) jumpCool = 40; // too close and too slow: go round again
                    else if (off < jumpOff && vn > 4.0) { jump = true; jumpCool = 150; jumps++; if (hand != null) { jumpOff = 1.95 + hand.Next() * 0.3; aimRate = AimRate * (0.75 + hand.Next() * 0.5); } }
                }
                else
                {
                    wx = tx + nx * (off < RunLane ? 0.6 : 0) - nx * (off > RunLane + 1.5 ? 0.4 : 0);
                    wz = tz + nz * (off < RunLane ? 0.6 : 0) - nz * (off > RunLane + 1.5 ? 0.4 : 0);
                }
            }
            // a pilot that has not moved for 1.5 s (a corner, an orb's knock) turns round and cuts across the middle for a moment
            if (tick % 60 == 0)
            {
                if (Math.Abs(me.x - stuckX) + Math.Abs(me.z - stuckZ) < 2.0 && sim.mover.grounded) { dirSign = -dirSign; unstick = 50; }
                stuckX = me.x; stuckZ = me.z;
            }
            if (unstick > 0)
            {
                unstick--;
                double ul = Math.Max(1e-6, JsMath.Hypot(dx, dz));
                wx = -dx / ul; wz = -dz / ul;
                jump = false;
            }
            // the aim: the lit face of the lantern, else a sentry in sight
            Vec3 aim = l.Centre;
            bool lit = l.LitSegment >= 0;
            Sentry27 sentry = null;
            if (!lit)
            {
                double best = 1e9;
                foreach (Sentry27 s in l.sentries)
                {
                    if (!s.alive) continue;
                    double d2 = Vec3.DistanceTo(s.position, eye);
                    bool seen = !sim.world.RayIntersect(eye, Vec3.Normalize(s.position - eye), out RayHit wh0) || wh0.distance > d2;
                    if (d2 < best && seen) { best = d2; sentry = s; }
                }
                if (sentry != null) aim = sentry.position;
            }
            Vec3 dv = aim - eye;
            double wantYaw = JsMath.Atan2(-dv.x, -dv.z), wantPitch = JsMath.Atan2(dv.y, JsMath.Hypot(dv.x, dv.z));
            double stp = aimRate * dt, dy = wantYaw - yawL;
            while (dy > Math.PI) dy -= 2 * Math.PI;
            while (dy < -Math.PI) dy += 2 * Math.PI;
            yawL += Math.Max(-stp, Math.Min(stp, dy));
            pitchL += Math.Max(-stp, Math.Min(stp, wantPitch - pitchL));
            Vec3 dir = WyrmRigLike(yawL, pitchL);
            bool fire = false;
            if (lit)
            {
                double t = l.Raycast(eye, dir);
                fire = t >= 0 && l.ShieldDownOn(l.hitFace) && !(sim.world.RayIntersect(eye, dir, out RayHit wh) && wh.distance < t);
            }
            else if (sentry != null)
            {
                double t = sentry.RayHit(eye, dir, out _);
                fire = t >= 0 && !(sim.world.RayIntersect(eye, dir, out RayHit wh2) && wh2.distance < t);
            }
            double ml = JsMath.Hypot(wx, wz);
            var f = new InputFrame { yaw = yawL, pitch = pitchL };
            if (ml > 0.05)
            {
                wx /= Math.Max(1, ml);
                wz /= Math.Max(1, ml);
                Vec3 fwd = MovementMath.WishDirection(0, 1, yawL), right = MovementMath.WishDirection(1, 0, yawL);
                f.moveZ = wx * fwd.x + wz * fwd.z;
                f.moveX = wx * right.x + wz * right.z;
            }
            f.sprint = new Button { held = true, pressed = tick % 60 == 0 };
            if (jump) { f.jump = new Button { held = true, pressed = true }; jumpHold = 20; }
            else if (jumpHold > 0) { f.jump = new Button { held = true }; jumpHold--; }
            Move = f;
            int gun = sim.loadout.owned.IndexOf("revolver");
            Combat = new CombatInput { weapon = gun >= 0 && sim.loadout.Weapon != "revolver" ? gun : -1 };
            if (fire) Combat.fire = new Button { held = true, pressed = true };
        }

        static Vec3 WyrmRigLike(double yaw, double pitch)
        {
            Vec3 fw = MovementMath.WishDirection(0, 1, yaw);
            double cp = JsMath.Cos(pitch);
            return new Vec3(fw.x * cp, JsMath.Sin(pitch), fw.z * cp);
        }
    }
}
