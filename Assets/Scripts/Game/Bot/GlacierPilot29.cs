// NIGHT29 builder B: a copy of GlacierPilot28 that chases the boss's LANE (design.n29.boss2: the skate lane rule pins a grounded skater to a lane line; a switch is a ground dash sideways) and fires the
// pistol all the time (guns do light damage now; the gauntlet blast is for the exposed window). Original header: NIGHT28 builder B2 (boss): the scripted pilot that CATCHES THE GLACIER on its rink (Core/Enemies/Glacier28.cs): it skates the centre line toward the boss (along the shorter arc), slides the straights to
// close the gap, takes the line each attack leaves open (shards: the open side or the bank; columns: a dash slalom; roller: a jump; low bar: a slide; chasm: a jump), cracks the ice glaze with the PISTOL
// while the glove recharges, and when the PULSE-READY window is open and the boss is in reach it aims and presses LT (the pulse27 blast; the gauntlet in hand, a full glove). It uses the buttons a player
// has: sprint, slide, jump, dash, fire, LT. State lives here; the harness calls Step once per fixed step and reads Move / Combat. Numbers are bot-side, not gameplay.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class GlacierPilot29
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
        int laneDashCool;
        public static bool DbgOn;
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
                if (g.rule29 != null && a > g.rink.Lap - h.hl - 1.5) a -= g.rink.Lap; // NIGHT29: a shard the player is still alongside (its centre just behind) still counts as ahead (negative)
                if (h.kind == "shard" || h.kind == "column") { if (a < (g.rule29 != null ? 30 : 16) && a < nearAhead) { near = h; nearAhead = a; } }
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
            double lat = g.rule29 != null ? g.bossLat29 : 0; // NIGHT29: chase the boss's lane (the skate lane rule pins the player to a line: a lane switch is a ground dash sideways)
            if (near != null)
            {
                double cur = g.playerLat, m = 0.9;
                double right = near.lat1 + m, left = near.lat0 - m;
                lat = Math.Abs(right - cur) <= Math.Abs(left - cur) ? right : left;
                if (left < -2.4) lat = right;
                lat = Math.Max(-2.4, Math.Min(5.4, lat));
                dodges++;
                if (g.rule29 != null && sim.glacier29 != null && near.kind == "shard")
                {
                    // the skate lane pins the skater to a line: take the nearest lane line the trail leaves open (a dash sideways)
                    SkateLane29 sl = sim.glacier29.lane;
                    int from = sl.NearestLine(g.playerLat), best = from;
                    double bd = 1e9;
                    for (int i = 0; i < sl.count; i++)
                    {
                        double ln = sl.LineLat(i);
                        if (ln > near.lat0 - 0.5 && ln < near.lat1 + 0.5) continue;
                        if (Math.Abs(i - from) < bd) { bd = Math.Abs(i - from); best = i; }
                    }
                    lat = sl.LineLat(best);
                }
            }
            int chase = g.gapSigned >= 0 ? 1 : -1; // toward the boss along the shorter arc
            double lead = Math.Max(5, Math.Min(9, speed * 0.6));
            g.rink.PointAt(g.playerS + chase * lead, lat, out double tx, out double tz, out _, out _);
            double wx = tx - me.x, wz = tz - me.z, wl = JsMath.Hypot(wx, wz);
            if (wl < 1e-6) { wx = 0; wz = 1; wl = 1; }
            wx /= wl; wz /= wl;
            bool laneDash = false, needBoost = false;
            if (laneDashCool > 0) laneDashCool--;
            if (sim.glacier29 != null && g.rule29 != null)
            {
                SkateLane29 sl = sim.glacier29.lane;
                int cur = sl.NearestLine(g.playerLat), tgt = sl.NearestLine(lat);
                if (DbgOn && laneDashCool > 50) { g.rink.PointAt(g.playerS, 0, out double bx0, out double bz0, out _, out _); g.rink.PointAt(g.playerS, 1, out double bx1, out double bz1, out _, out _); Console.WriteLine($"        dashstep t={tick * dt:0.000} state {sim.mover.state} vlat {(sim.mover.velocity.x * (bx1 - bx0) + sim.mover.velocity.z * (bz1 - bz0)):0.0} speed {speed:0.0} lat {g.playerLat:0.00} switches {sl.switches} pips {sim.mover.pips:0.0}"); }
                if (DbgOn && near != null && tick % 20 == 0) Console.WriteLine($"      dodge t={tick * dt:0.00} near {near.kind} ahead {nearAhead:0.0} band {near.lat0:0.0}..{near.lat1:0.0} cur {cur} tgt {tgt} lat {lat:0.0} cool {laneDashCool} grounded {sim.mover.grounded} speed {speed:0.0} state {sim.mover.state}");
                bool boostOk = !sim.mover.stamina.rules.Any || sim.mover.stamina.bar >= Stamina.FloorFill(sim.mover.stamina.rules);
                if (cur != tgt && !boostOk) needBoost = true;
                if (cur != tgt && boostOk && laneDashCool == 0 && sim.mover.grounded && speed > 3)
                {
                    g.rink.PointAt(g.playerS, 0, out double ax0, out double az0, out _, out _);
                    g.rink.PointAt(g.playerS, 1, out double ax1, out double az1, out _, out _);
                    double sgn = tgt > cur ? 1 : -1;
                    wx = (ax1 - ax0) * sgn; wz = (az1 - az0) * sgn;
                    laneDash = true;
                    laneDashCool = 60;
                    if (DbgOn) Console.WriteLine($"        PRESS t={tick * dt:0.000} cur {cur} tgt {tgt} wish ({wx:0.00},{wz:0.00}) state {sim.mover.state} pips {sim.mover.pips:0.0} bar {sim.mover.stamina.bar:0.00} floor {Stamina.FloorFill(sim.mover.stamina.rules):0.00} dashStarts {sl.dashStarts} switches {sl.switches}");
                    dashes++;
                }
            }
            var f = new InputFrame { yaw = yawL, pitch = pitchL };
            Vec3 fwd = MovementMath.WishDirection(0, 1, yawL), right2 = MovementMath.WishDirection(1, 0, yawL);
            f.moveZ = wx * fwd.x + wz * fwd.z;
            f.moveX = wx * right2.x + wz * right2.z;
            // the stamina bar pays for sprint AND boost (dash): a player keeps a boost's worth in reserve for the lane switches, so the bot sprints only above twice the boost's floor
            bool reserve = needBoost;
            f.sprint = new Button { held = !reserve, pressed = !reserve && tick % 60 == 0 };
            if (laneDash) f.dash = new Button { held = true, pressed = true };
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
            bool needCrack = g.rule29 != null || (g.glazed && !g.CutOpen); // NIGHT29: guns hurt at any time: the pistol stays in hand
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
