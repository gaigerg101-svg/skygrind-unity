// NIGHT26 (builder L): the tutorial pilot's fight with THE SLAG HULK (level 1's boss room). The autopilot only knows enemies, not a boss system, so the pilot drives this itself
// with the KESTREL, the way builder B's harness bot does (Tools/SimHarness/Harness.N26.Boss.cs HulkRun, without its human wobble and lag): a standoff ring around the hulk,
// strafing, the aim slewed at a fixed rate onto its head (the torso when the head is hidden), fire only when the ray lands on it, never while a tell or a charge runs,
// leave a locked charge line to the near side, jump the shockwave rings. State lives here; the pilot calls Step once per fixed step and reads Move / Combat.
// Numbers are bot-side, not gameplay (AimRate, the standoff, the jump window): they shape one scripted opponent, they change no rule.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class HulkPilot26
    {
        public InputFrame Move;
        public CombatInput Combat;
        double yawL, pitchL;
        int stepNo = -1, strafeFlip, jumpHold, tellSeen, reacquire;
        bool started;
        const double AimRate = 3.5; // rad/s the aim slews at
        const double Standoff = 8.5; // m from the hulk
        const double AimOk = 0.02; // rad: the aim error that may fire

        /// <summary>Compute this fixed step's input once (a second call in the same step keeps the answer).</summary>
        public void Step(SurvivalSim sim, double yawNow, int tick)
        {
            if (tick == stepNo) return;
            stepNo = tick;
            Hulk h = sim.hulk26;
            double dt = sim.step;
            if (!started) { started = true; yawL = yawNow; pitchL = 0; reacquire = MovementMath.StepsFor(0.3, dt); }
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            Vec3 hp = h.FeetWorld;
            Vec3 aim = h.HeadPosition;
            if (!(h.Raycast(eye, Vec3.Normalize(aim - eye)) >= 0)) aim = new Vec3(hp.x, hp.y + 2.5, hp.z);
            bool busy = jumpHold > 0 || h.state == "slamTell" || h.state == "chargeTell" || h.state == "charge" || h.state == "slamRecover";
            Vec3 d = aim - eye;
            double wantYaw = JsMath.Atan2(-d.x, -d.z), wantPitch = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
            double stp = AimRate * dt, dy = wantYaw - yawL;
            while (dy > Math.PI) dy -= 2 * Math.PI;
            while (dy < -Math.PI) dy += 2 * Math.PI;
            double aimErr = Math.Max(Math.Abs(dy), Math.Abs(wantPitch - pitchL));
            yawL += Math.Max(-stp, Math.Min(stp, dy));
            pitchL += Math.Max(-stp, Math.Min(stp, wantPitch - pitchL));
            if (reacquire > 0) reacquire--;
            Vec3 fw = MovementMath.WishDirection(0, 1, yawL);
            double cp = JsMath.Cos(pitchL);
            Vec3 dir = new Vec3(fw.x * cp, JsMath.Sin(pitchL), fw.z * cp);
            float hitD = (float)h.Raycast(eye, dir);
            bool fire = !busy && reacquire == 0 && aimErr < AimOk && hitD >= 0 && !(sim.world.RayIntersect(eye, dir, out RayHit wh) && wh.distance < hitD);
            double dx = me.x - hp.x, dz = me.z - hp.z, dist = JsMath.Hypot(dx, dz);
            if (dist < 1e-6) { dx = 0; dz = 1; dist = 1; }
            double rx = dx / dist, rz = dz / dist, tx = -rz, tz = rx;
            if (tick % (120 * 3) == 0) strafeFlip ^= 1;
            double side = strafeFlip == 0 ? 1 : -1, radial = Math.Max(-1, Math.Min(1, (Standoff - dist) / 3));
            double mx = rx * radial + tx * 0.5 * side, mz = rz * radial + tz * 0.5 * side;
            if (h.state == "chargeTell" || h.state == "charge") tellSeen++; else tellSeen = 0;
            if (tellSeen > MovementMath.StepsFor(0.3, dt))
            {
                Vec3 cd = h.ChargeDir;
                double ox = me.x - hp.x, oz = me.z - hp.z, along = ox * cd.x + oz * cd.z, perp = ox * cd.z - oz * cd.x;
                if (along > -2 && Math.Abs(perp) < 4.5)
                {
                    double s = perp < 0 ? -1 : 1;
                    mx = cd.z * s * 3;
                    mz = -cd.x * s * 3;
                }
            }
            bool jump = false;
            for (int i = 0; i < h.rings.Count; i++)
            {
                double gapR = JsMath.Hypot(me.x - hp.x, me.z - hp.z) - h.rings[i].r;
                if (!h.rings[i].hit && gapR > -0.5 && gapR < 2.8 && sim.mover.grounded) jump = true;
            }
            double ml = JsMath.Hypot(mx, mz);
            var f = new InputFrame { yaw = yawL, pitch = pitchL };
            if (ml > 0.05)
            {
                mx /= Math.Max(1, ml);
                mz /= Math.Max(1, ml);
                Vec3 fwd = MovementMath.WishDirection(0, 1, yawL), right = MovementMath.WishDirection(1, 0, yawL);
                f.moveZ = mx * fwd.x + mz * fwd.z;
                f.moveX = mx * right.x + mz * right.z;
            }
            if (jump) { f.jump = new Button { held = true, pressed = true }; jumpHold = 48; }
            else if (jumpHold > 0) { f.jump = new Button { held = true }; jumpHold--; }
            Move = f;
            int gun = sim.loadout.owned.IndexOf("revolver");
            Combat = new CombatInput { weapon = gun >= 0 && sim.loadout.Weapon != "revolver" ? gun : -1 };
            if (fire) Combat.fire = new Button { held = true, pressed = true };
        }
    }
}
