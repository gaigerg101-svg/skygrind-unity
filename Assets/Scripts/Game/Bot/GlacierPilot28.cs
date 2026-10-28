// NIGHT28 builder B2 (boss): the scripted pilot that CATCHES THE GLACIER on its rink (Core/Enemies/Glacier28.cs): it skates the centre line toward the boss (along the shorter arc), slides the straights to
// close the gap, takes the line each attack leaves open (shards: the open side or the bank; columns: a dash slalom; roller: a jump; low bar: a slide; chasm: a jump), cracks the ice glaze with the PISTOL
// while the glove recharges, and when the PULSE-READY window is open and the boss is in reach it aims and presses LT (the pulse27 blast; the gauntlet in hand, a full glove). It uses the buttons a player
// has: sprint, slide, jump, dash, fire, LT. State lives here; the harness calls Step once per fixed step and reads Move / Combat. Numbers are bot-side, not gameplay.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class GlacierPilot28
    {
        public InputFrame Move;
        public CombatInput Combat;
        public int blasts, slides, dodges, dashes, jumps, shots;
        /// <summary>The RED pilot: chases and dodges but never presses LT.</summary>
        public bool noLaunch;
        /// <summary>The RED pilot that never shoots: it never cracks the glaze (the pulse then only clangs).</summary>
        public bool noShoot;
        /// <summary>The human's hand (seeded): the reaction time between the window opening and LT.</summary>
        public Mulberry32 hand;
        double releaseDelay = 0.3;
        int ltHold, winSteps, ltCool, slideHold, jumpHold, dashCool, stepNo = -1;
        bool winOpenPrev, started;
        double yawL, pitchL;
        const double AimRate = 8.0;
        const int LtHoldSteps = 12; // 0.1 s

        static double Ahead(Glacier28 g, Glacier28Hazard h) => g.dir > 0 ? g.rink.Wrap(h.s - g.playerS) : g.rink.Wrap(g.playerS - h.s);

        public void Step(SurvivalSim sim, double yawNow, int tick)
        {
            if (tick == stepNo) return;
            stepNo = tick;
            Glacier28 g = sim.glacier28;
            double dt = sim.step;
            if (!started) { started = true; yawL = yawNow; pitchL = 0; }
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z), vel = sim.mover.velocity;
            double speed = JsMath.Hypot(vel.x, vel.z);
            Vec3 aim = g.box.c;
            double aimYaw = JsMath.Atan2(-(aim.x - eye.x), -(aim.z - eye.z)), aimPitch = JsMath.Atan2(aim.y - eye.y, JsMath.Hypot(aim.x - eye.x, aim.z - eye.z));
            double stp = AimRate * dt, dy = aimYaw - yawL;
            while (dy > Math.PI) dy -= 2 * Math.PI;
            while (dy < -Math.PI) dy += 2 * Math.PI;
            yawL += Math.Max(-stp, Math.Min(stp, dy));
            pitchL += Math.Max(-stp, Math.Min(stp, aimPitch - pitchL));

            // the attacks ahead: the nearest standing object (shard / column) sets the line; a roller, bar or chasm sets a jump or a slide
            Glacier28Hazard near = null;
            double nearAhead = 1e9;
            bool jump = false, slide = false;
            double vz = Math.Max(4, speed);
            foreach (Glacier28Hazard h in g.hazards)
            {
                if (h.tellLeft <= 0 && h.lifeLeft <= 0) continue;
                double a = Ahead(g, h);
                if (h.kind == "shard" || h.kind == "column") { if (a < 16 && a < nearAhead) { near = h; nearAhead = a; } }
                else if (h.kind == "roller")
                {
                    double d = Math.Abs(g.rink.SignedGap(h.s, g.playerS));
                    if (h.tellLeft <= 0 && d < (vz + 9) * 0.28 + h.hl + 0.8 && d > 0.8 && sim.mover.grounded) jump = true;
                }
                else if (h.kind == "bar")
                {
                    double d = g.rink.SignedGap(h.s, g.playerS);
                    if (Math.Abs(d) < Math.Max(5.5, vz * 0.6) && (g.dir > 0 ? d < 0.5 : d > -0.5)) slide = true;
                }
                else if (h.kind == "chasm")
                {
                    double d = g.rink.SignedGap(h.s, g.playerS);
                    double toEdge = (g.dir > 0 ? -d : d) - h.hl; // metres to the near edge
                    if (toEdge < 2.2 && toEdge > -0.5 && sim.mover.grounded) jump = true;
                }
            }
            double lat = 0;
            if (near != null)
            {
                double cur = g.playerLat, m = 0.9;
                double right = near.lat1 + m, left = near.lat0 - m;
                lat = Math.Abs(right - cur) <= Math.Abs(left - cur) ? right : left;
                if (left < -2.4) lat = right;
                lat = Math.Max(-2.4, Math.Min(5.4, lat));
                dodges++;
            }
            int chase = g.gapSigned >= 0 ? 1 : -1; // toward the boss along the shorter arc
            double lead = Math.Max(5, Math.Min(9, speed * 0.6));
            g.rink.PointAt(g.playerS + chase * lead, lat, out double tx, out double tz, out _, out _);
            double wx = tx - me.x, wz = tz - me.z, wl = JsMath.Hypot(wx, wz);
            if (wl < 1e-6) { wx = 0; wz = 1; wl = 1; }
            wx /= wl; wz /= wl;
            var f = new InputFrame { yaw = yawL, pitch = pitchL };
            Vec3 fwd = MovementMath.WishDirection(0, 1, yawL), right2 = MovementMath.WishDirection(1, 0, yawL);
            f.moveZ = wx * fwd.x + wz * fwd.z;
            f.moveX = wx * right2.x + wz * right2.z;
            f.sprint = new Button { held = true, pressed = tick % 60 == 0 };
            // a dash across the line when the next column is a lateral hop away
            if (dashCool > 0) dashCool--;
            if (near != null && near.kind == "column" && Math.Abs(lat - g.playerLat) > 1.8 && nearAhead < 8 && dashCool == 0 && sim.mover.grounded) { f.dash = new Button { held = true, pressed = true }; dashCool = 50; dashes++; }
            // slide on the straights (the direction is locked at the start): a hold when below the slide's floor speed and the line ahead is straight; under a bar
            g.rink.PointAt(g.playerS, 0, out _, out _, out double t0x, out double t0z);
            g.rink.PointAt(g.playerS + chase * 7, 0, out _, out _, out double t1x, out double t1z);
            bool straight = Math.Abs(t0x * t1z - t0z * t1x) < 0.04 && Math.Abs(g.playerLat - lat) < 0.8;
            if (slide && slideHold < 30) { slideHold = 60; slides++; f.slide = new Button { held = true, pressed = true }; }
            else if (slideHold > 0) { f.slide = new Button { held = true }; slideHold--; }
            else if (straight && sim.mover.grounded && speed > 4 && speed < 12.5 && near == null) { slideHold = 50; f.slide = new Button { held = true, pressed = true }; slides++; }
            if (jump && jumpHold == 0) { jumpHold = 14; jumps++; }
            if (jumpHold > 0) { f.jump = new Button { held = true, pressed = jumpHold == 14 }; jumpHold--; }
            Move = f;

            // the weapons: the pistol cracks the glaze (aim at the boss whenever it is in the ray), the gauntlet blasts when the glaze is cracked
            bool needCrack = g.glazed && !g.CutOpen;
            string want = needCrack && !noShoot ? "revolver" : "gauntlet";
            int idx = sim.loadout.owned.IndexOf(want);
            Combat = new CombatInput { weapon = idx >= 0 && sim.loadout.Weapon != want ? idx : -1 };
            Vec3 dir = AimDir(yawL, pitchL);
            bool onTarget = g.Raycast(eye, dir) >= 0 && Math.Abs(dy) < 0.12;
            if (sim.loadout.Weapon == "revolver" && needCrack && onTarget)
            {
                Combat.fire = new Button { held = true, pressed = tick % 4 == 0 };
                shots++;
            }
            if (ltCool > 0) ltCool--;
            if (winOpenPrev && !g.WindowOpen && hand != null) releaseDelay = 0.1 + hand.Next() * 0.9;
            winOpenPrev = g.WindowOpen;
            winSteps = g.WindowOpen ? winSteps + 1 : 0;
            double dist = Vec3.DistanceTo(me, g.box.c);
            bool inReach = dist < g.p.reach - 1.5 && Math.Abs(dy) < 0.15;
            // LT is HELD for LtHoldSteps steps (the glove's blast reads the held edge): a frame-stepped driver (the Player reel merges several sim steps into one frame and keeps the LAST step's held
            // state) would lose a one-step press, which is why the first Player run landed 0 of 5 blasts
            if (ltHold > 0) { ltHold--; Combat.altFire = new Button { held = true }; }
            else if (!noLaunch && g.WindowOpen && inReach && winSteps >= (int)(releaseDelay / dt) && ltCool == 0 && sim.loadout.Weapon == "gauntlet")
            {
                Combat.altFire = new Button { held = true, pressed = true };
                ltHold = LtHoldSteps;
                ltCool = 60;
                blasts++;
                if (hand != null) releaseDelay = 0.1 + hand.Next() * 0.9;
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
