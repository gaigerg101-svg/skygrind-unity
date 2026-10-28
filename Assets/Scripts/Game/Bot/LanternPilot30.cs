// NIGHT30 builder B (lantern30): the scripted rider for THE LANTERN RING (Core/Enemies/Lantern30.cs, Lantern30Arena.cs), LanternPilot29's movement on a ring of flat panels:
// it runs round the ring a few metres off the wall, angles in and jumps so the Mover starts a WALL RIDE (the Mover's own rule; nothing is forced), kicks off and back in to chain
// rides while the floor is live or about to be, leaves a ride whose panel (or the next one) is swept, and fires the pistol whenever its ray lands on the lantern (any face; the
// rule decides what lands). The aim slews at a fixed rate (a human's limit) and a seeded hand varies the jump distance and the aim rate. Numbers are bot-side: they shape one
// scripted opponent and change no rule.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class LanternPilot30
    {
        public InputFrame Move;
        public CombatInput Combat;
        public int rides, jumps, wallJumps, sweepDodges;
        double yawL, pitchL;
        int stepNo = -1, jumpHold, jumpCool, dirSign = 1, rideSteps;
        bool started;
        MoveState prev = MoveState.Ground;
        const double AimRate = 9.0; // rad/s the aim slews at
        const double RunLane = 4.0; // m from the wall the run keeps
        const double MinSpeed = 6.5; // m/s along the wall before a jump
        const int ChainAt = 120; // steps into a ride (1.0 s of the 1.2 s) at which the chain kicks off
        public Mulberry32 hand;
        double jumpOff = 2.1, aimRate = AimRate, stuckX, stuckZ;
        int unstick, offWall = 1 << 20;

        static bool LavaNear(SurvivalSim sim) => sim.lantern29 != null && sim.lantern29.Stage != Lantern29Stage.Rest;

        public void Step(SurvivalSim sim, double yawNow, int tick)
        {
            if (tick == stepNo) return;
            stepNo = tick;
            Lantern27 l = sim.lantern27;
            Lantern30 l30 = sim.lantern30;
            Lantern30Arena a = sim.lantern30Arena;
            double dt = sim.step;
            if (!started) { started = true; yawL = yawNow; pitchL = 0; }
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            Vec3 o = l.OriginWorld;
            double dx = me.x - o.x, dz = me.z - o.z;
            int pan = a.PanelAt(dx, dz);
            Vec3 n = a.InwardNormalAt(dx, dz), tg = a.TangentAt(dx, dz) * dirSign; // NIGHT32 A: the facet's (the panel's when the ring is not round: the same as before)
            double nx = n.x, nz = n.z, tx = tg.x, tz = tg.z, off = a.WallDistance(dx, dz);
            double vAlong = sim.mover.velocity.x * tx + sim.mover.velocity.z * tz;
            MoveState st = sim.mover.state;
            // NIGHT32 A: on the round ring the Mover ends a ride every ~30 degrees of curve ('corner') and starts the next at once, so the chain counts
            // the whole stay on the wall (a ride that starts within 6 steps of the last one continues it); the flat ring keeps NIGHT31's count
            if (st == MoveState.WallRide && prev != MoveState.WallRide && !(a.Round && offWall <= 6)) { rides++; rideSteps = 0; }
            offWall = st == MoveState.WallRide ? 0 : offWall + 1;
            prev = st;
            double wx, wz;
            bool jump = false;
            if (jumpCool > 0) jumpCool--;
            int next = ((pan + dirSign) % a.p.panels + a.p.panels) % a.p.panels;
            bool swept = l30 != null && l30.SweepPanel >= 0 && (l30.SweepPanel == pan || l30.SweepPanel == next);
            if (st == MoveState.WallRide)
            {
                wx = tx + (-nx) * 0.25;
                wz = tz + (-nz) * 0.25;
                rideSteps++;
                if (swept && jumpCool == 0) { jump = true; jumpCool = 30; sweepDodges++; }
                else if (LavaNear(sim) && rideSteps >= ChainAt && jumpCool == 0) { jump = true; jumpCool = 30; wallJumps++; }
            }
            else if (!sim.mover.grounded)
            {
                wx = tx * 0.6 + (-nx) * 0.9;
                wz = tz * 0.6 + (-nz) * 0.9;
            }
            else
            {
                bool ready = vAlong > MinSpeed && jumpCool == 0 && !swept;
                if (ready)
                {
                    wx = tx * 0.85 - nx * 0.55;
                    wz = tz * 0.85 - nz * 0.55;
                    double vn = -(sim.mover.velocity.x * nx + sim.mover.velocity.z * nz);
                    if (off < 1.1 && vn < 4.0) jumpCool = 40;
                    else if (off < jumpOff && vn > 4.0) { jump = true; jumpCool = LavaNear(sim) ? 45 : 150; jumps++; if (hand != null) { jumpOff = 1.95 + hand.Next() * 0.3; aimRate = AimRate * (0.75 + hand.Next() * 0.5); } }
                }
                else
                {
                    wx = tx + nx * (off < RunLane ? 0.6 : 0) - nx * (off > RunLane + 1.5 ? 0.4 : 0);
                    wz = tz + nz * (off < RunLane ? 0.6 : 0) - nz * (off > RunLane + 1.5 ? 0.4 : 0);
                }
            }
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
            Vec3 aim = l.Centre;
            Vec3 dv = aim - eye;
            double wantYaw = JsMath.Atan2(-dv.x, -dv.z), wantPitch = JsMath.Atan2(dv.y, JsMath.Hypot(dv.x, dv.z));
            double stp = aimRate * dt, dy = wantYaw - yawL;
            while (dy > Math.PI) dy -= 2 * Math.PI;
            while (dy < -Math.PI) dy += 2 * Math.PI;
            yawL += Math.Max(-stp, Math.Min(stp, dy));
            pitchL += Math.Max(-stp, Math.Min(stp, wantPitch - pitchL));
            Vec3 fw = MovementMath.WishDirection(0, 1, yawL);
            double cp = JsMath.Cos(pitchL);
            Vec3 dir = new Vec3(fw.x * cp, JsMath.Sin(pitchL), fw.z * cp);
            double t = l.Raycast(eye, dir);
            bool fire = t >= 0 && !(sim.world.RayIntersect(eye, dir, out RayHit wh) && wh.distance < t);
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
            if (fire) Combat.fire = new Button { held = true, pressed = tick % 2 == 0 };
        }
    }
}
