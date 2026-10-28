// NIGHT27 builder B (boss2): the scripted pilot that chases THE GLACIER the way the fight asks (Core/Enemies/Glacier27.cs): sprint the ice, dodge the lane attacks (jump the sweep and the roller,
// slide the low bar, hug a wall column for the icicle fall), and when the PULSE-READY window is open aim at the skater and let go of LT with a charged gauntlet in hand: the Fist24 launch carries the
// player into it and its hit is the only thing that hurts it. The gauntlet is the only weapon it carries (the level's rule). Nothing is forced: it uses the same buttons a player has (LT held, LT
// released). State lives here; the pilot calls Step once per fixed step and reads Move / Combat. Numbers are bot-side, not gameplay.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class GlacierPilot27
    {
        public InputFrame Move;
        public CombatInput Combat;
        public int launches, dodgesJump, dodgesSlide;
        /// <summary>The RED pilot: chases and dodges but never lets go of LT.</summary>
        public bool noLaunch;
        /// <summary>The human's hand (seeded): when in the window the launch is released (reaction + aim: a late release misses the window).</summary>
        public Mulberry32 hand;
        double releaseDelay = 0.3; // s after the window opens that the human lets go (0.1-1.9 s: a late one misses the 1.5 s window)
        int winSteps;
        bool winOpenPrev;
        double yawL, pitchL;
        int stepNo = -1, jumpHold, slideHold, lastLaneJump = -100, ltHold, ltCool;
        bool started;
        double? wallX;
        const double AimRate = 8.0;

        public void Step(SurvivalSim sim, double yawNow, int tick)
        {
            if (tick == stepNo) return;
            stepNo = tick;
            Glacier27 g = sim.glacier27;
            double dt = sim.step;
            Vec3 o = sim.glacier27Origin;
            if (!started) { started = true; yawL = yawNow; pitchL = 0; }
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            Vec3 aim = g.box.c;
            double aimYaw = JsMath.Atan2(-(aim.x - eye.x), -(aim.z - eye.z)), aimPitch = JsMath.Atan2(aim.y - eye.y, JsMath.Hypot(aim.x - eye.x, aim.z - eye.z));
            double stp = AimRate * dt, dy = aimYaw - yawL;
            while (dy > Math.PI) dy -= 2 * Math.PI;
            while (dy < -Math.PI) dy += 2 * Math.PI;
            yawL += Math.Max(-stp, Math.Min(stp, dy));
            pitchL += Math.Max(-stp, Math.Min(stp, aimPitch - pitchL));
            // steer: the middle lane (a rail is a jump), a wall column for the icicle fall
            double targetX = g.gap < 40 ? Math.Max(o.x - 9, Math.Min(o.x + 9, g.BossWorld.x)) : o.x; // close behind it, in its lane (a rail is a jump)
            bool jump = false, slide = false;
            double vz = Math.Max(2, -sim.mover.velocity.z);
            bool icicle = false;
            foreach (Glacier27Obstacle ob in g.obstacles)
            {
                double dist = me.z - ob.z; // metres ahead (the course runs toward -z)
                if (dist < -2.5) continue;
                if (ob.kind == "icicle")
                {
                    icicle = true;
                    if (wallX == null) wallX = o.x + (me.x >= o.x ? 1 : -1) * (g.p.laneHalf - g.p.icicleSafe * 0.5);
                }
                else if ((ob.kind == "sweep") && ob.tellLeft <= 0 && dist < vz * 0.2 + 0.3 && dist > vz * 0.1 && sim.mover.grounded) jump = true;
                else if (ob.kind == "low" && ob.tellLeft <= 0 && dist < Math.Max(5, vz * 0.6) && dist > -1.5) slide = true;
                else if (ob.kind == "roller" && ob.tellLeft <= 0 && dist < vz * 0.35 + 0.8 && dist > vz * 0.1 && sim.mover.grounded && Math.Abs(ob.rollX - me.x) < 6) jump = true;
            }
            if (!icicle && wallX != null) { bool stillLive = false; foreach (Glacier27Obstacle ob in g.obstacles) if (ob.kind == "icicle") stillLive = true; if (!stillLive) wallX = null; }
            if (wallX != null) targetX = wallX.Value;
            bool nearBar = false;
            foreach (Glacier27Obstacle ob in g.obstacles) if (ob.kind != "icicle" && me.z - ob.z > -3 && me.z - ob.z < 9) nearBar = true;
            if (!jump && !nearBar && sim.mover.grounded && Math.Abs(targetX - me.x) > 3.2 && tick - lastLaneJump > 50) { jump = true; lastLaneJump = tick; }
            if (jump) { jumpHold = 14; dodgesJump++; }
            if (slide) { slideHold = 24; dodgesSlide++; }
            var f = new InputFrame { yaw = yawL, pitch = pitchL };
            double wx = Math.Max(-1, Math.Min(1, (targetX - me.x) * 0.8 - sim.mover.velocity.x * 0.25)), wz = g.gap < 0 ? 1 : g.gap < 3 ? 0.3 : g.gap < 8 ? -0.4 : -1; // never run past it: hold station behind it until the launch
            Vec3 fwd = MovementMath.WishDirection(0, 1, yawL), right = MovementMath.WishDirection(1, 0, yawL);
            double ml = JsMath.Hypot(wx, wz);
            wx /= ml; wz /= ml;
            f.moveZ = wx * fwd.x + wz * fwd.z;
            f.moveX = wx * right.x + wz * right.z;
            f.sprint = new Button { held = true, pressed = tick % 60 == 0 };
            if (jumpHold > 0) { f.jump = new Button { held = true, pressed = jumpHold == 14 }; jumpHold--; }
            if (slideHold > 0) { f.slide = new Button { held = true, pressed = slideHold == 24 }; slideHold--; }
            Move = f;
            // the pulse: LT held for a moment then released with the window open and the skater in the aim
            int gun = sim.loadout.owned.IndexOf("gauntlet");
            Combat = new CombatInput { weapon = gun >= 0 && sim.loadout.Weapon != "gauntlet" ? gun : -1 };
            if (ltCool > 0) ltCool--;
            if (winOpenPrev && !g.WindowOpen && hand != null) releaseDelay = 0.1 + hand.Next() * 1.4; // a window ended: a fresh reaction for the next one
            winOpenPrev = g.WindowOpen;
            winSteps = g.WindowOpen ? winSteps + 1 : 0;
            Vec3 dir = AimDir(yawL, pitchL);
            bool onTarget = g.Raycast(eye, dir) >= 0 && Math.Abs(dy) < 0.15;
            if (ltHold > 0)
            {
                ltHold--;
                if (ltHold > 0) Combat.altFire = new Button { held = true };
                else { ltCool = 600; launches++; if (hand != null) releaseDelay = 0.1 + hand.Next() * 1.8; } // released this step: the launch
            }
            else if (!noLaunch && g.WindowOpen && onTarget && winSteps >= (int)(releaseDelay / dt) && ltCool == 0 && sim.loadout.Weapon == "gauntlet")
            {
                ltHold = 6;
                Combat.altFire = new Button { held = true, pressed = true };
            }
        }

        static Vec3 AimDir(double yaw, double pitch)
        {
            Vec3 fw = MovementMath.WishDirection(0, 1, yaw);
            double cp = JsMath.Cos(pitch);
            return new Vec3(fw.x * cp, JsMath.Sin(pitch), fw.z * cp);
        }
    }
}
