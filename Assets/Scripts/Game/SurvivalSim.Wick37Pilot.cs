// NIGHT37 builder W (wick37): a scripted CLIMBER for THE STACK (the harness's climb check and band, the Player reel wick37). Bot-side numbers only;
// no rule changes. It reads the stack's panel list (arena-local) and plays the level's verb:
//   GROUND   run to the next panel's start (the lowest panel above the feet), along its face, and jump with jump HELD beside it (the Lucio attach).
//   RIDE     hold jump, look along the ride pitched up `pitchDeg`; near the panel's end RELEASE (the jump-off: its up speed follows the pitch).
//   AIR      turn to the next panel's face; after `djDelay` the double jump (a press), jump held again (attach on contact); a dash toward the next
//            face when it is still far (`dashFar`).
//   FIGHT    (shoot / parry on) punch a yellow ember within reach (the parry); with a cracked plate (or the window) and on a ride, hold fire to
//            charge the KESTREL and release at the core; on the cap: keep moving round the catwalk, side-dash out of a dive's line, and when THE
//            WICK stalls at the wall, dash at it and punch (the launch).
// `Mode`: Climb (all of it), Level (the control: pitch 0, no double jump, no dash: a ride alone), NoDash (pitch up, no dash).
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class Wick37Pilot
    {
        public enum Mode { Climb, Level, NoDash }
        public Mode mode = Mode.Climb;
        public bool shoot = true, parry = true;
        public InputFrame Move;
        public CombatInput Combat;
        public int rides, leaps, doubles, dashes, punches, charges, launches;
        public readonly List<int> ridden = new List<int>();
        public double pitchDeg = 45, releaseBefore = 1.2, crossAt = 0.45, ledgeRel = 0.5, panelRel = 0.6, djDelay = 0.1, djVy = 1.0, dashFar = 1e9, aimRate = 24;
        bool approach;
        public bool holdOffWall, forgetOnFall;
        public Mulberry32 hand;
        double yawL, pitchL;
        int stepNo = -1, lastPanel = -1, sinceRelease = 100000, punchCool, chargeHold, dashCool, groundSteps;
        bool started, released, doubled, dashed;
        MoveState prev = MoveState.Ground;
        int stuckSteps;
        double stuckY;

        static double Wrap(double a)
        {
            while (a > Math.PI) a -= 2 * Math.PI;
            while (a < -Math.PI) a += 2 * Math.PI;
            return a;
        }

        /// <summary>The panel the player is riding now (arena-local feet `l`), or -1.</summary>
        public static int RiddenPanel(Wick37Stack s, Vec3 l, double capR)
        {
            int best = -1;
            double bd = 1e9;
            foreach (Wick37Panel p in s.panels)
            {
                if (l.y + 1.8 < p.bottom - 0.2 || l.y > p.top + 0.2) continue;
                Vec3 a = p.start, b = p.end, ab = b - a;
                double t = Math.Max(-0.2, Math.Min(1.2, ((l.x - a.x) * ab.x + (l.z - a.z) * ab.z) / (ab.x * ab.x + ab.z * ab.z)));
                double px = a.x + ab.x * t, pz = a.z + ab.z * t, d = JsMath.Hypot(l.x - px, l.z - pz);
                double front = (l.x - a.x) * p.normal.x + (l.z - a.z) * p.normal.z;
                if (front < 0) continue; // behind the face's plane: the hot wall, not this panel
                if (d < bd) { bd = d; best = p.index; }
            }
            return bd < capR + 0.6 ? best : -1;
        }

        /// <summary>The along-face coordinate of `l` on panel p: 0 at its start, 1 at its end.</summary>
        static double Along(Wick37Panel p, Vec3 l)
        {
            Vec3 ab = p.end - p.start;
            return ((l.x - p.start.x) * ab.x + (l.z - p.start.z) * ab.z) / (ab.x * ab.x + ab.z * ab.z);
        }

        /// <summary>On: the climb in the air is SCRIPTED per panel (the harness `wick37 gaps` leap: ride the target panel, release at scriptRel of it
        /// looking up scriptPitch, aim at 40 % of the next panel (or the ledge arc's middle), double jump once falling, jump held again to attach);
        /// the target advances when the ride is on the next panel, and is re-read from the feet after a fall.</summary>
        public bool scripted = true;
        public double ledgeJumpAt = 0.1;
        public readonly List<string> log = new List<string>();
        public double scriptRel = 0.6, scriptLedgeRel = 0.7, scriptPitch = 45;
        /// <summary>NIGHT43 B1 (T-59): metres BEHIND a panel's face the air approach aims (the stick held into the wall, as a person rides; 0 =
        /// NIGHT37's aim on the face). NIGHT40's ride40 asks the attach for intent (the stick toward the wall, design.n40.ride40.inputDot) and draws
        /// only 0.5 m (attachReach): NIGHT37's aim along the face never attached once (n43wick climb --off: 0/10, refusals no-intent / slow).</summary>
        public double wallLean = 1.0;
        /// <summary>NIGHT43 B1 (the casual bot, L-058): each release at a fresh random point of the panel (35-85 %, ledges 50-85 %) looking up
        /// 28-50 degrees (the skilled hand: one point and 40-50 degrees per seed), late parries (1.9 m), a slower turn (set aimRate 10 with it).</summary>
        public bool casual;
        /// <summary>NIGHT43 B1 (G13, the harness only): the PRESS climb: no jump hold anywhere; each panel is left by a pressed wall jump (it spends
        /// one of wallJump.maxPerAirtime until a landing; chimney43 refills it on a new wall), the next attached by contact.</summary>
        public bool press;
        int pressRide;
        bool pressNow;
        /// <summary>NIGHT43 B1 round 2 (G13, the harness only): the MIXED climb: after each release the next panel is pressed (as `press`) with this chance, held otherwise.</summary>
        public double pressShare;
        int target = -1, sSince = 1000;
        bool sReleased, sDoubled;

        void Scripted(SurvivalSim sim, Wick37Stack s, Vec3 l, MoveState st, double capR, double dt, ref double wantYaw, ref double wantPitch, ref double mz, ref bool jumpHeld, ref bool jumpPress)
        {
            sSince++;
            if (target < 0)
            {
                int nf = Math.Max(0, NextFromFeet(s, l.y));
                if (st == MoveState.WallRide) { target = nf; sReleased = false; }
                else
                {
                    // off the ground into the air (a jump at a panel from a floor): head for the panel itself, jump held: the attach
                    // NIGHT43 B1 (T-59, with wallLean > 0): the panel the ground run jumped at (lastPanel + 1), not the next one above the feet:
                    // 0.7 m into the jump NextFromFeet already names the panel after it (feet 0.77 > panel 0's bottom + 0.7), and the stick
                    // turned away from the wall it was jumping at (wish into the face -0.45: ride40 refused every attach as no-intent)
                    if (wallLean > 0 && lastPanel + 1 >= 0 && lastPanel + 1 < s.panels.Count && s.panels[lastPanel + 1].top > l.y + 0.3) nf = lastPanel + 1;
                    target = nf;
                    Wick37Panel ap = s.panels[nf];
                    Vec3 aim0 = ap.start + (ap.end - ap.start) * 0.35 - ap.normal * wallLean;
                    wantYaw = JsMath.Atan2(-(aim0.x - l.x), -(aim0.z - l.z));
                    mz = 1;
                    jumpHeld = true;
                    target = -1;
                    return;
                }
            }
            if (st == MoveState.WallRide)
            {
                int rp = RiddenPanel(s, l, capR);
                if (rp >= 0 && rp != target && (!sReleased || rp == target + 1 || rp < target)) { target = rp; sReleased = false; log.Add($"  caught p{rp} at a {Along(s.panels[rp], l):0.00} feet-bottom {l.y - s.panels[rp].bottom:+0.00;-0.00}"); }
                if (rp == target && sReleased && sSince > 6) sReleased = false; // re-caught the same panel
            }
            if (target >= s.panels.Count) target = s.panels.Count - 1;
            Wick37Panel p = s.panels[target];
            Vec3 tg = p.end - p.start;
            double a = Along(p, l);
            bool last = target + 1 >= s.panels.Count, toLedge = last || s.panels[target + 1].section != p.section;
            if (st == MoveState.WallRide && !sReleased)
            {
                lastPanel = target;
                wantYaw = JsMath.Atan2(-tg.x, -tg.z);
                double hi = l.y - p.bottom; // the leap's look by where on the panel the ride is: low = look up hard; high = less (a hop rises ~0.25 m too much at 45 deg: the ride would creep off the top)
                wantPitch = mode == Mode.Level ? 0 : (hi < 1.2 ? scriptPitch : hi < 2.0 ? 30 : 15) * Math.PI / 180;
                mz = 1;
                bool go = a >= (toLedge ? scriptLedgeRel : scriptRel) || (sim.mover.velocity.x * tg.x + sim.mover.velocity.z * tg.z) < 0;
                jumpHeld = !go;
                if (press || pressNow) { pressRide++; go = go || pressRide >= 6; jumpHeld = false; if (go) { jumpPress = true; pressRide = 0; } } // NIGHT43 B1: the PRESS climb: a tapped wall jump 0.05 s into the contact ride (no hold keeps it), never a hold
                if (go && pressShare > 0 && hand != null) pressNow = hand.Next() < pressShare; // NIGHT43 B1 round 2: the MIXED climb: the next panel pressed or held
                if (go && casual && hand != null) { scriptRel = 0.35 + 0.5 * hand.Next(); scriptLedgeRel = 0.5 + 0.35 * hand.Next(); scriptPitch = 28 + 22 * hand.Next(); } // NIGHT43 B1: a casual hand lets go anywhere, looking up less
                if (go) { sReleased = true; sSince = 0; sDoubled = false; leaps++; log.Add($"rel p{target} a {a:0.00} v {JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z):0.0} feet-bottom {l.y - p.bottom:+0.00;-0.00}"); }
                return;
            }
            if (!sReleased && st != MoveState.WallRide) { sReleased = true; sSince = 0; sDoubled = false; } // the ride ended on its own: the late jump
            // in the air after the release: at the next panel / the ledge / the cap
            Vec3 aim;
            if (toLedge)
            {
                if (p.section < s.ledgeAngle.Length) { double la = s.ledgeAngle[p.section] - 0.15; aim = new Vec3(JsMath.Cos(la) * 8.4, 0, JsMath.Sin(la) * 8.4); }
                else { double la = JsMath.Atan2(l.z, l.x) + 0.6; aim = new Vec3(JsMath.Cos(la) * 11, 0, JsMath.Sin(la) * 11); }
            }
            else aim = s.panels[target + 1].start + (s.panels[target + 1].end - s.panels[target + 1].start) * 0.4 - s.panels[target + 1].normal * wallLean;
            wantYaw = JsMath.Atan2(-(aim.x - l.x), -(aim.z - l.z));
            wantPitch = sSince < 4 && mode != Mode.Level ? scriptPitch * Math.PI / 180 : 0.2;
            mz = 1;
            if (mode != Mode.Level && !sDoubled && sSince > 4 && sim.mover.velocity.y < 1.0 && sSince < 150) { sDoubled = true; jumpPress = true; doubles++; }
            jumpHeld = sSince > 3 && !toLedge;
            if (st == MoveState.WallRide) jumpHeld = true; // keep a ride we are on (the target logic above re-reads it)
            if (press || pressNow) jumpHeld = false; // NIGHT43 B1: the PRESS climb attaches by contact (a tap ride)
            if (l.y < p.bottom - 3) target = -1; // a fall: re-read from the feet
        }

        void Ground(SurvivalSim sim, Wick37Stack s, Vec3 l, ref double wantYaw, ref double mz, ref bool jumpHeld, ref bool jumpPress)
        {
            // on a ledge arc: run along it (its middle radius) toward the next panel and jump at its end, jump held: the attach
            for (int li = 0; li < s.p.ledges.Length && l.y > 1; li++)
            {
                if (Math.Abs(l.y - s.p.ledges[li]) > 0.3) continue;
                int np = NextFromFeet(s, l.y);
                if (np < 0) break;
                double la = JsMath.Atan2(l.z, l.x), end = s.ledgeAngle[li] + s.p.ledgeArcDeg * Math.PI / 360, d = end - la;
                while (d > Math.PI) d -= 2 * Math.PI;
                while (d < -Math.PI) d += 2 * Math.PI;
                Wick37Panel q = s.panels[np];
                Vec3 into = q.start + (q.end - q.start) * 0.25 + q.normal * 0.4;
                double rl = s.p.radius - 0.9, ah = la + 0.25;
                Vec3 goal = d > 0.2 ? new Vec3(JsMath.Cos(ah) * rl, 0, JsMath.Sin(ah) * rl) : into;
                wantYaw = JsMath.Atan2(-(goal.x - l.x), -(goal.z - l.z));
                mz = 1;
                if (d < ledgeJumpAt && sim.mover.Speed > 4) { jumpPress = true; jumpHeld = true; }
                return;
            }
    groundSteps++;
    int nx = NextFromFeet(s, l.y);
    lastPanel = nx - 1;
    if (nx >= 0)
    {
        Wick37Panel p = s.panels[nx];
        Vec3 tg = p.end - p.start;
        double tl = Math.Max(1e-6, tg.Length);
        Vec3 tu = tg * (1 / tl);
        // the run-up spot: in from the face and back from its start (kept inside the shaft); then a diagonal run into the face's start
        Vec3 run = p.start - tu * 2.5 + p.normal * 2.5;
        double rr0 = JsMath.Hypot(run.x, run.z), lim = s.p.panelApothem - 1.0;
        if (rr0 > lim) run = new Vec3(run.x * lim / rr0, 0, run.z * lim / rr0);
        if (l.y > 1)
        {
            // on a ledge arc: the run-up is back along the arc (it is 1.2 m wide), then along it into the face's start
            double sa = JsMath.Atan2(p.start.z, p.start.x) - 0.45, rl = s.p.radius - s.p.ledgeWidth * 0.5;
            run = new Vec3(JsMath.Cos(sa) * rl, 0, JsMath.Sin(sa) * rl);
        }
        Vec3 into = p.start + tu * 2.0 + p.normal * 0.4;
        double alongNow = Along(p, l), off = (l.x - p.start.x) * p.normal.x + (l.z - p.start.z) * p.normal.z;
        double vAlong = sim.mover.velocity.x * tu.x + sim.mover.velocity.z * tu.z;
        bool near = JsMath.Hypot(run.x - l.x, run.z - l.z) < 1.2;
        if (near) approach = true;
        if (alongNow > 0.6 || off > 4.5) approach = false;
        Vec3 goal = approach ? into : run;
        wantYaw = JsMath.Atan2(-(goal.x - l.x), -(goal.z - l.z));
        mz = 1;
        if (approach && off < 1.6 && vAlong > 3 && alongNow > (l.y > 1 ? -0.7 : -0.3)) { jumpPress = true; jumpHeld = true; approach = false; }
    }
    else { wantYaw = JsMath.Atan2(l.x, l.z); mz = 0; }
        }

        int NextFromFeet(Wick37Stack s, double feet)
        {
            foreach (Wick37Panel p in s.panels) if (p.bottom >= feet - 0.7) return p.index;
            return -1;
        }

        public void Step(SurvivalSim sim, double yawNow, int tick)
        {
            if (tick == stepNo) return;
            stepNo = tick;
            Wick37Stack s = sim.wick37Stack;
            Vec3 o = sim.wick37Centre.Value, me = sim.mover.position, l = me - o, eye = new Vec3(me.x, me.y + 1.6, me.z);
            double dt = sim.step, capR = sim.cfg.movement.body.capsuleRadius;
            if (!started) { started = true; yawL = yawNow; pitchL = 0; }
            MoveState st = sim.mover.state;
            bool grounded = sim.mover.grounded;
            Wick37 w = sim.wick37;
            bool cap = l.y >= s.p.capY - 0.5 && (grounded || JsMath.Hypot(l.x, l.z) > s.p.radius); // on the cap (not riding the last panel past its height)
            if (punchCool > 0) punchCool--;
            if (dashCool > 0) dashCool--;
            sinceRelease++;
            double wantYaw = yawL, wantPitch = 0, mx = 0, mz = 0;
            bool jumpHeld = false, jumpPress = false, dash = false;
            int ride = st == MoveState.WallRide ? RiddenPanel(s, l, capR) : -1;
            if (st == MoveState.WallRide && prev != MoveState.WallRide) { rides++; if (ride >= 0 && (ridden.Count == 0 || ridden[ridden.Count - 1] != ride)) ridden.Add(ride); released = false; }
            bool lateRelease = false;
            if (prev == MoveState.WallRide && st == MoveState.Air && !released)
            {
                // the ride ended on its own (the panel's end) before the release: release now (the late jump: design.n35.wallride35.lateJump)
                lateRelease = true;
                released = true;
                sinceRelease = 0;
                doubled = dashed = false;
                leaps++;
            }
            prev = st;
            if (cap)
            {
                CapPlay(sim, s, o, l, w, ref wantYaw, ref wantPitch, ref mx, ref mz, ref jumpHeld, ref jumpPress, ref dash);
            }
            else if (scripted && grounded) { target = -1; sReleased = false; Ground(sim, s, l, ref wantYaw, ref mz, ref jumpHeld, ref jumpPress); }
            else if (scripted && !grounded)
            {
                Scripted(sim, s, l, st, capR, dt, ref wantYaw, ref wantPitch, ref mz, ref jumpHeld, ref jumpPress);
            }
            else if (st == MoveState.WallRide && ride >= 0)
            {
                Wick37Panel p = s.panels[ride];
                lastPanel = ride;
                Vec3 tg = p.end - p.start;
                wantYaw = JsMath.Atan2(-tg.x, -tg.z);
                wantPitch = (mode == Mode.Level ? 0 : pitchDeg) * Math.PI / 180;
                mz = 1;
                double a = Along(p, l), relAt = 1 - releaseBefore / Math.Max(1, (p.end - p.start).Length);
                bool go = a >= relAt;
                go = a >= panelRel; // the harness `wick37 gaps` window: a release anywhere from about 20 % to 100 % of the panel rides the next one
                if (false)
                {
                    // where would a jump-off now cross the next panel's face? (out 8 m/s + the ride's speed + the exit boost, straight)
                    Wick37Panel q = s.panels[ride + 1];
                    double tl = Math.Max(1e-6, tg.Length), speed = JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z) + 2;
                    Vec3 v = p.normal * 8 + tg * (speed / tl);
                    double vn = v.x * q.normal.x + v.z * q.normal.z;
                    if (vn < -1e-6)
                    {
                        double gap = (l.x - q.start.x) * q.normal.x + (l.z - q.start.z) * q.normal.z - capR;
                        double tc = gap / -vn;
                        Vec3 c = new Vec3(l.x + v.x * tc, 0, l.z + v.z * tc);
                        double aq = Along(q, c);
                        go = aq <= crossAt; // the crossing slides toward the next panel's start as you ride on: leap when it is crossAt along it
                    }
                }
                if (ride + 1 < s.panels.Count && s.panels[ride + 1].section != p.section) go = a >= ledgeRel; // the next floor is a ledge arc in the next slot: leap late, land on it
                if (ride + 1 >= s.panels.Count) go = a >= ledgeRel; // the last panel: the cap
                if ((sim.mover.velocity.x * tg.x + sim.mover.velocity.z * tg.z) < 0) go = true; // riding the wrong way: let go and come again
                jumpHeld = !go;
                if (!jumpHeld) { released = true; sinceRelease = 0; doubled = dashed = false; leaps++; }
            }
            else if (st == MoveState.WallRide)
            {
                jumpHeld = false; // a ride on something else (a ledge lip): let go
            }
            else if (!grounded)
            {
                int nx = lastPanel >= 0 ? lastPanel + 1 : NextFromFeet(s, l.y);
                bool toFloor = lastPanel >= 0 && (nx >= s.panels.Count || s.panels[nx].section != s.panels[lastPanel].section);
                if (toFloor && sinceRelease < 150)
                {
                    // a leap onto the ledge arc (or the cap): aim at its middle, double jump at the top, no attach
                    int sec = s.panels[lastPanel].section;
                    Vec3 aim;
                    if (sec < s.ledgeAngle.Length) { double la = s.ledgeAngle[sec] - 0.15; aim = new Vec3(JsMath.Cos(la) * 8.4, 0, JsMath.Sin(la) * 8.4); }
                    else { double la = JsMath.Atan2(l.z, l.x) + 0.5; aim = new Vec3(JsMath.Cos(la) * 11, 0, JsMath.Sin(la) * 11); }
                    wantYaw = JsMath.Atan2(-(aim.x - l.x), -(aim.z - l.z));
                    mz = 1;
                    if (!doubled && sinceRelease >= (int)(djDelay / dt) && sim.mover.velocity.y < djVy) { doubled = true; jumpPress = true; doubles++; }
                    jumpHeld = false;
                    wantPitch = 0.2;
                }
                else if (nx >= 0 && nx < s.panels.Count)
                {
                    Wick37Panel p = s.panels[nx];
                    Vec3 aim = p.start + (p.end - p.start) * 0.4;
                    wantYaw = JsMath.Atan2(-(aim.x - l.x), -(aim.z - l.z));
                    double far = JsMath.Hypot(aim.x - l.x, aim.z - l.z);
                    mz = 1;
                    if (sinceRelease < 120 && mode != Mode.Level)
                    {
// (round 2: no dash on a panel gap: the gaps harness makes every gap without it)
                        if (!doubled && sinceRelease >= (int)(djDelay / dt) && sim.mover.velocity.y < djVy) { doubled = true; jumpPress = true; doubles++; }
                        if (!dashed && doubled && mode == Mode.Climb && sim.mover.velocity.y < djVy && far > dashFar) { dashed = true; dash = true; dashes++; }
                    }
                }
                if (!(toFloor && sinceRelease < 150))
                {
                    jumpHeld = sinceRelease > 3 && !lateRelease; // held again: the attach
                    wantPitch = sinceRelease < 4 ? pitchDeg * Math.PI / 180 : 0.2;
                }
                // a person lets go of jump by the hot wall (or the hold would draw him onto it); a fall well under the last panel forgets it
                if (holdOffWall && JsMath.Hypot(l.x, l.z) > s.p.radius - 0.8 && RiddenPanel(s, l, capR) < 0) jumpHeld = false;
                if (forgetOnFall && lastPanel >= 0 && l.y < s.panels[lastPanel].bottom - 2.5) lastPanel = -1;
            }
            else
            {
                // on a floor (the floor, a ledge, the cap): run to the next panel's start and jump into it with jump held
                Ground(sim, s, l, ref wantYaw, ref mz, ref jumpHeld, ref jumpPress);
            }
            // unstick: no height gained for 4 s on the ground: hop toward the axis
            if (grounded) { if (Math.Abs(l.y - stuckY) > 0.5) { stuckY = l.y; stuckSteps = 0; } else if (++stuckSteps > 480) { stuckSteps = 0; wantYaw += Math.PI * 0.5; } }
            // the movement as a world direction (the look may turn away from it to aim: the wish stays)
            Vec3 wf = MovementMath.WishDirection(0, 1, wantYaw), wr = MovementMath.WishDirection(1, 0, wantYaw);
            double wx = wf.x * mz + wr.x * mx, wz = wf.z * mz + wr.z * mx;
            // the fight: aim, parry, charge
            bool fire = false;
            CombatAim(sim, w, eye, ref wantYaw, ref wantPitch, st == MoveState.WallRide, ref fire, out bool punch);
            double stp = aimRate * dt, dy = Wrap(wantYaw - yawL);
            yawL += Math.Max(-stp, Math.Min(stp, dy));
            pitchL += Math.Max(-stp, Math.Min(stp, wantPitch - pitchL));
            Vec3 ff = MovementMath.WishDirection(0, 1, yawL), rr = MovementMath.WishDirection(1, 0, yawL);
            var fr = new InputFrame { yaw = yawL, pitch = pitchL, moveZ = wx * ff.x + wz * ff.z, moveX = wx * rr.x + wz * rr.z };
            fr.sprint = new Button { held = true, pressed = tick % 60 == 0 };
            if (jumpPress) fr.jump = new Button { held = true, pressed = true };
            else if (jumpHeld) fr.jump = new Button { held = true };
            if (dash && dashCool == 0) { fr.dash = new Button { held = true, pressed = true }; dashCool = 30; }
            Move = fr;
            int gun = sim.loadout.owned.IndexOf("revolver");
            Combat = new CombatInput { weapon = gun >= 0 && sim.loadout.Weapon != "revolver" ? gun : -1 };
            if (fire) Combat.fire = new Button { held = true, pressed = chargeHold == 1 };
            else if (chargeHold > 0) { Combat.fire = new Button { held = false }; chargeHold = 0; }
            if (punch && punchCool == 0) { Combat.punch = new Button { held = true, pressed = true }; punchCool = 12; punches++; }
        }

        /// <summary>The cap (leg 3): keep moving round the catwalk; out of a dive's line; at a stall, dash in and punch.</summary>
        void CapPlay(SurvivalSim sim, Wick37Stack s, Vec3 o, Vec3 l, Wick37 w, ref double wantYaw, ref double wantPitch, ref double mx, ref double mz, ref bool jumpHeld, ref bool jumpPress, ref bool dash)
        {
            double r = JsMath.Hypot(l.x, l.z), a = JsMath.Atan2(l.z, l.x);
            double runR = (s.p.radius + s.p.capRadius) / 2;
            // run counter-clockwise round the catwalk's middle
            double ta = a + 0.6;
            var goal = new Vec3(JsMath.Cos(ta) * runR, l.y, JsMath.Sin(ta) * runR);
            wantYaw = JsMath.Atan2(-(goal.x - l.x), -(goal.z - l.z));
            mz = 1;
            if (r < s.p.radius + 0.9) { var outGoal = new Vec3(JsMath.Cos(a) * runR, l.y, JsMath.Sin(a) * runR); wantYaw = JsMath.Atan2(-(outGoal.x - l.x), -(outGoal.z - l.z)); } // near the hole's rim: out
            if (w == null || !w.Alive || w.brain == null) return;
            Vec3 c = w.Centre - o;
            double dc = JsMath.Hypot(c.x - l.x, c.z - l.z);
            if (w.stalled)
            {
                wantYaw = JsMath.Atan2(-(c.x - l.x), -(c.z - l.z));
                wantPitch = JsMath.Atan2(c.y - (l.y + 1.6), Math.Max(0.5, dc));
                if (dc < 4.5 && dashCool == 0) { dash = true; launches++; }
            }
            else if (w.AttackId == Wick37.Dive && (w.brain.state == Boss37State.Tell || w.brain.state == Boss37State.Active))
            {
                // the dodge: keep running round the catwalk (never in toward the hole), dash along it when the dive comes close
                if (w.brain.state == Boss37State.Active && dc < 5 && dashCool == 0) dash = true;
            }
        }

        void CombatAim(SurvivalSim sim, Wick37 w, Vec3 eye, ref double wantYaw, ref double wantPitch, bool riding, ref bool fire, out bool punch)
        {
            punch = false;
            if (w == null || !w.Alive || w.brain == null) return;
            // the parry: the nearest incoming ember within 6 m
            Wick37Ember near = null;
            double nd = 6;
            foreach (Wick37Ember e in w.embers)
            {
                if (!e.alive || e.back) continue;
                double d = Vec3.DistanceTo(e.pos, eye);
                if (d < nd) { nd = d; near = e; }
            }
            if (parry && near != null)
            {
                Vec3 dv = near.pos - eye;
                wantYaw = JsMath.Atan2(-dv.x, -dv.z);
                wantPitch = JsMath.Atan2(dv.y, JsMath.Hypot(dv.x, dv.z));
                punch = nd < (casual ? 1.9 : 2.8); // NIGHT43 B1: the casual hand punches late (inside 1.9 m of the 3 m reach)
                return;
            }
            if (w.stalled)
            {
                Vec3 dv = w.Centre - eye;
                punch = dv.Length < w.p.launchReach + w.p.cageR - 0.2;
                return;
            }
            if (!shoot) return;
            bool gap = w.CrackedCount > 0 || w.brain.Open;
            if ((riding || chargeHold > 0) && gap && w.brain.Fighting)
            {
                // charge on the ride: look at the core while holding, release after the charge
                Vec3 dv = w.Centre - eye;
                wantYaw = JsMath.Atan2(-dv.x, -dv.z);
                wantPitch = JsMath.Atan2(dv.y, JsMath.Hypot(dv.x, dv.z));
                chargeHold++;
                fire = chargeHold < 72; // hold 0.6 s, then the release fires the charged shot
                if (chargeHold == 72) charges++;
                if (chargeHold >= 80) chargeHold = 0;
            }
        }
    }
}
