// NIGHT43 builder L1 (foundry43; NIGHT43.md L1: "the walk 10/10", "the share of scripted gantry crossings that succeed"): the walk pilot's scripts for
// n43-foundry's rebuilt rooms (Tools/night/gen-n43-foundry.cjs): b2 THE GANTRY (gate 1 one long wall, gate 2 the chain with a dash gap), a7 THE FLUE
// (the chimney), a8 PIERCE (P1 to the perch, then P2 firing at the hatch's husks). The same controllers drive the harness's person sweeps
// (Tools/SimHarness/Harness.N43.Level.cs `n43level cross`), with their numbers jittered. Called from Walk38Pilot.SampleMove / SampleCombat by one
// line each (Walk38Foundry.cs) only on n43-foundry. Bot numbers only (never the player's, never gameplay). No UnityEngine (SimHarness compiles it).
// Room-relative positions as Walk29Pilot's (lz = metres north of the room's south edge z1, cx = the room's centre line x).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>One wall of a scripted ride chain: its side (-1 west, +1 east), metres past the gate's lip it spans, and whether its end is a dash gap.</summary>
    public struct Wall43
    {
        public int side;
        public double from, to;
        public bool dashAfter;
        public Wall43(int side, double from, double to, bool dashAfter = false) { this.side = side; this.from = from; this.to = to; this.dashAfter = dashAfter; }
    }

    /// <summary>A ride chain over one gate (THE GANTRY): the run-up on the first wall's line, the jump at the lip held into the first ride, per wall
    /// the release (the jump-off) before its end and the re-press held into the next, or the DASH along the line at a dash gap, then the far floor.</summary>
    public sealed class Chain43
    {
        public Wall43[] walls;
        /// <summary>The gate's lip (lz), its length, the walls' plane (|x| from cx), the room's centre x and south edge z.</summary>
        public double t, gap, plane, cx, z1;
        // the person's numbers (the harness jitters them)
        public double releaseBefore = 3.0, lastReleaseBefore = 2.5, releasePitch = 0.52, ridePitch = 0, dashBefore = 0.8, lean = 0.14, stick = 0.4;
        public int repressSteps = 40, dashSteps = 30;
        /// <summary>The bot only: the ceiling's underside over the gate (0: none / the person sweep).</summary>
        public double ceilingY;
        public bool sprintRunUp = true; // the bot: false (the sprint drains the bar the gap's dash needs)
        public bool dashStickInto; // the dash pressed with the stick still into the wall (a person's mistake: the reflection sends it off the line)
        // the state
        public int k, released = -1, step, rides, dashes, dashAt = -1;
        public bool onWall;

        public enum R { Running, Across, Fell }

        public void Reset() { k = 0; released = -1; dashAt = -1; onWall = false; }

        static Button Press(bool held, bool pressed) => new Button { held = held, pressed = pressed };
        static double Toward(Vec3 from, double tx, double tz) => Math.Atan2(-(tx - from.x), -(tz - from.z));

        public R Drive(Vec3 p, MoveState st, bool grounded, out InputFrame f)
        {
            step++;
            double lz = z1 - p.z;
            f = new InputFrame { moveZ = 1 };
            if (lz >= t + gap + 0.5 && grounded && p.y > -0.2) return R.Across;
            if (p.y < -0.3 && lz > t - 0.5 && lz < t + gap + 0.5) return R.Fell;
            bool riding = st == MoveState.WallRide;
            if (lz < t - 0.2 && grounded)
            {
                Reset();
                double tx = cx + walls[0].side * (plane - 0.9);
                if (lz < t - 3.5 || Math.Abs(p.x - tx) > 0.8 && lz < t - 1.2) { f.yaw = Toward(p, tx, z1 - Math.Max(lz + 0.5, t - 3)); return R.Running; }
                f.yaw = Toward(p, tx, z1 - (t + 6));
                f.sprint = Press(sprintRunUp, false);
                if (lz >= t - 0.9) f.jump = Press(true, true);
                return R.Running;
            }
            if (riding && !onWall) { onWall = true; rides++; released = -1; dashAt = -1; }
            if (onWall && !riding)
            {
                onWall = false;
                if (released < 0 && dashAt < 0) released = step;
                k++;
            }
            if (k >= walls.Length)
            {
                // past the last wall: to the far floor, the double jump re-pressed RepressSteps after the release (held 6 steps), the stick let go over it
                f.yaw = Toward(p, cx, z1 - (t + gap + 4));
                int s2 = released >= 0 ? step - released : 0;
                f.jump = Press(s2 >= repressSteps && s2 < repressSteps + 6, s2 == repressSteps);
                if (lz > t + gap + 1) { f.moveZ = 0; f.moveX = 0; f.jump = default; }
                return R.Running;
            }
            Wall43 w = walls[k];
            if (riding)
            {
                f.yaw = w.side < 0 ? lean : -lean;
                f.moveX = w.side * stick;
                f.pitch = ridePitch;
                double end = t + w.to;
                bool last = k >= walls.Length - 1;
                if (w.dashAfter && !last)
                {
                    if (lz >= end - dashBefore)
                    {
                        // the dash along the wall's line (the stick forward only: WallRideDashDir keeps a stick along the wall, reflects one into it)
                        f.yaw = 0;
                        f.moveX = dashStickInto ? w.side * 0.6 : 0;
                        f.dash = Press(true, true);
                        f.jump = Press(true, false);
                        dashAt = step;
                        dashes++;
                        return R.Running;
                    }
                }
                else if (lz >= end - (last ? lastReleaseBefore : releaseBefore))
                {
                    f.jump = Press(false, false); // the release: the jump-off
                    released = step;
                    f.pitch = ceilingY > 0 && p.y + 2.6 > ceilingY ? -0.2 : releasePitch; // the bot: a ride high under the gantry's ceiling lets go level (a jump-off up bonks the slab)
                    double nx = last ? cx : cx + walls[k + 1].side * plane, nz = last ? t + gap + 3 : t + Math.Max(walls[k + 1].from, lz - t) + 4;
                    f.yaw = Toward(p, nx, z1 - nz);
                    return R.Running;
                }
                f.jump = Press(true, false);
                return R.Running;
            }
            if (dashAt >= 0)
            {
                // after the dash: along the line, then the stick into the next wall on the same side, the jump held (the ride starts on contact)
                f.yaw = 0;
                f.moveX = step - dashAt > dashSteps ? w.side * 0.6 : 0;
                f.jump = Press(true, false);
                return R.Running;
            }
            // the look at the wall a few metres ahead (fast in the air, a far aim point turns the stick too little to steer: the air accel adds
            // nothing along a wish the speed already exceeds)
            double tz = t + Math.Min(Math.Max(lz - t + 3, w.from + 1), w.to - 1);
            f.yaw = Toward(p, cx + w.side * (plane + 1), z1 - tz);
            f.moveX = w.side * stick; // the stick into the wall it flies at (ride40's attach asks for intent: dot >= 0.1)
            if (released < 0) { f.jump = Press(true, false); return R.Running; } // from the lip: still held from the jump
            int since = step - released;
            f.jump = since < repressSteps ? Press(false, false) : Press(true, since == repressSteps);
            return R.Running;
        }
    }

    /// <summary>THE FLUE's climb (the chimney): held rides wall to wall, the look up while riding, a release after releaseAfter s on each wall
    /// (the jump-off up and across), the re-press held into the facing wall; over the deck's height at the chimney's end: onto the deck.</summary>
    public sealed class Flue43
    {
        public double cx, z1, half = 2.25, from = 5, to = 25, deckY = 8;
        public double ridePitch = 0.78, releasePitch = 0.78, releaseAfter = 0.35, lean = 0.2, stick = 0.4, jumpLead = 1.0;
        public int repressSteps = 18;
        /// <summary>The pressed (tap) climb instead of held rides: a jump press on every new wall contact, held 0.1 s (chimney43's own case).</summary>
        public bool taps;
        /// <summary>Presses straight across: stop straightIn m inside the chimney's mouth, then bounce wall to wall (no along speed).</summary>
        public bool straight;
        public double straightIn = 2, entryAim = 0.8;
        int side = -1, rideAge, released = -1, step, lastTap = -1000, tapUntil = -1;
        bool onWall;
        public int rides, top;
        public double maxY;

        public enum R { Running, Top, Floor }

        static Button Press(bool held, bool pressed) => new Button { held = held, pressed = pressed };
        static double Toward(Vec3 from, double tx, double tz) => Math.Atan2(-(tx - from.x), -(tz - from.z));

        public void Reset() { side = -1; released = -1; onWall = false; rideAge = 0; }

        public R Drive(Vec3 p, MoveState st, Vec3 wallNormal, bool grounded, double dt, bool touchingWall, out InputFrame f)
        {
            step++;
            double lz = z1 - p.z;
            f = new InputFrame { moveZ = 1 };
            if (p.y > maxY) maxY = p.y;
            if (grounded && (p.y > deckY - 0.6 || lz > to + 1)) return R.Top; // on the deck, or over it onto the floor by the door
            if (p.y > deckY + 0.15 && lz > to - 2) { f.yaw = 0; f.jump = default; return R.Running; } // over the deck's lip: onto it
            bool riding = st == MoveState.WallRide;
            if (grounded && p.y < 1)
            {
                Reset();
                if (lz > from + 2.5) { f.yaw = Toward(p, cx, z1 - (from - 2.5)); return R.Floor; } // a fall: back to the chimney's mouth
                if (straight)
                {
                    // presses straight across (M1's chimney43 case): walk into the chimney's mouth, stop, face the west wall, jump at it
                    if (Math.Abs(p.x - cx) > half - 0.2 && lz > from - 0.5) { f.yaw = Toward(p, p.x, z1 - (from - 1.5)); f.moveZ = 1; return R.Running; } // outside the lane (beside a wall): back out past the mouth first
                    if (lz < from + straightIn - 0.4 || Math.Abs(p.x - cx) > 0.6) { f.yaw = Toward(p, cx, z1 - (from + straightIn)); f.moveZ = 1; return R.Running; }
                    f.yaw = Toward(p, cx - half - 1, p.z);
                    f.jump = Press(true, true);
                    tapUntil = step + 30;
                    lastTap = step;
                    side = -1;
                    return R.Running;
                }
                if (lz < from - 3.5) { f.yaw = Toward(p, cx, z1 - from); return R.Running; }
                // the run-up angled at the first (west) wall a few metres in: a person heads at a wall (straight up the axis, fast, the air
                // control cannot turn you onto either wall: 0 rides measured at 3.5 / 4.0 m lanes)
                f.yaw = Toward(p, cx - half + entryAim, z1 - (from + 2.5));
                f.sprint = Press(true, false);
                // a lane that starts at the door (from < 3): the run-up is inside it, the jump ~2 m in (M1: a takeoff before the plates catches an END face)
                double jumpLz = from < 3 ? from + 1 + jumpLead : from - jumpLead;
                if (lz >= jumpLz) { f.jump = Press(true, true); tapUntil = step + 30; lastTap = step; }
                return R.Running;
            }
            if (straight)
            {
                // the press at each wall straight across: face the target wall, the stick into it, a press on contact, then the other wall
                f.yaw = Toward(p, cx + side * (half + 1), p.z - 0.3);
                f.pitch = 0.35;
                f.moveZ = 1;
                bool nearS = touchingWall || riding;
                if (nearS && step - lastTap > MovementMath.StepsFor(0.15, dt)) { f.jump = Press(true, true); tapUntil = step + MovementMath.StepsFor(0.1, dt); lastTap = step; side = -side; }
                else f.jump = Press(step < tapUntil, false);
                if (p.y > deckY + 0.15) { f.yaw = 0; f.moveX = 0; } // over the deck's height: forward to it
                return R.Running;
            }
            if (taps)
            {
                // the pressed climb: the stick into the target wall, a press on each new contact (0.15 s apart), held 0.1 s; the look up
                f.pitch = ridePitch;
                bool near = touchingWall || riding;
                f.yaw = Toward(p, cx + side * (half + 1), z1 - (lz + 2));
                f.moveX = side * 0.6;
                if (near && step - lastTap > MovementMath.StepsFor(0.15, dt)) { f.jump = Press(true, true); tapUntil = step + MovementMath.StepsFor(0.1, dt); lastTap = step; side = -side; }
                else f.jump = Press(step < tapUntil, false);
                return R.Running;
            }
            if (riding && !onWall) { onWall = true; rides++; rideAge = 0; released = -1; side = wallNormal.x > 0 ? -1 : 1; }
            if (onWall && !riding) { onWall = false; if (released < 0) released = step; side = -side; }
            if (riding)
            {
                rideAge++;
                f.yaw = side < 0 ? lean : -lean;
                f.moveX = side * stick;
                f.pitch = ridePitch;
                if (rideAge * dt >= releaseAfter || lz >= to - 1.0)
                {
                    f.jump = Press(false, false);
                    released = step;
                    f.pitch = releasePitch;
                    f.yaw = Toward(p, cx - side * (half + 1), z1 - (lz + 2.5));
                    return R.Running;
                }
                f.jump = Press(true, false);
                return R.Running;
            }
            f.yaw = Toward(p, cx + side * (half + 1), z1 - (lz + 2.5));
            f.pitch = releasePitch;
            f.moveX = side * stick; // the stick into the wall it flies at (ride40's intent)
            if (released < 0) { f.jump = Press(true, false); return R.Running; }
            int since = step - released;
            f.jump = since < repressSteps ? Press(false, false) : Press(true, since == repressSteps);
            return R.Running;
        }
    }

    public sealed partial class Walk38Pilot
    {
        // ---- NIGHT43 L1: n43-foundry's rebuilt rooms
        string lastId43 = "";
        Vec3 lastPos43;
        /// <summary>Teleports back to a lip seen in THE GANTRY (E1's fallReturn43 with rooms43 on): the harness's count.</summary>
        public int returns43;
        Chain43 g1_43, g2_43;
        Flue43 flue43;
        int pierce43Phase, pierce43Wait, fallLz43Steps;
        bool fire43;
        Enemy fireAt43;

        /// <summary>NIGHT43 L1: the gates' walls as gen-n43-foundry.cjs builds them (metres past each lip; gate 2's dash gap after W2).</summary>
        public static readonly Wall43[] Gate1Walls43 = { new Wall43(-1, -6, 36.5) };
        public static readonly Wall43[] Gate2Walls43 ={ new Wall43(-1, -6, 12), new Wall43(1, 8, 18, true), new Wall43(1, 23, 31), new Wall43(-1, 27, 36.5) };

        void Reset43(RoomDef rd)
        {
            double cx = (rd.x0 + rd.x1) / 2;
            g1_43 = new Chain43 { walls = Gate1Walls43, t = 8, gap = 37, plane = 3, cx = cx, z1 = rd.z1 };
            g2_43 = new Chain43 { walls = Gate2Walls43, t = 57, gap = 37, plane = 2.25, cx = cx, z1 = rd.z1, releaseBefore = 2.05, lastReleaseBefore = 2.5, releasePitch = 0.36, ridePitch = 0.17, dashBefore = 1.0, lean = 0.05, stick = 0.31, repressSteps = 30, sprintRunUp = false }; // person 16 of `n43level cross --m1` (crosses g2 first try)
            // THE FLUE: half lane 1.75 (gen-n43-foundry.cjs FLUE_LANE 3.5); with chimney43 on, presses straight across (40/40 measured), else held rides
            flue43 = new Flue43 { cx = cx, z1 = rd.z1, half = 1.75, from = 1.5, straight = sim.n43.chimney43 }; // FLUE_FROM 1.5: the lane starts at the door
            pierce43Phase = 0;
            pierce43Wait = 0;
            fire43 = false;
            fireAt43 = null;
        }

        /// <summary>NIGHT43 L1: this room's frame on n43-foundry (false: the other scripts / the autopilot).</summary>
        bool Walk43(RoomDef rd, Vec3 p, double cx, double lz, bool grounded, InputFrame auto, out InputFrame f)
        {
            f = auto;
            fire43 = false;
            if (sim.campaignLevel == null || N45Levels.BaseId(sim.cfg, sim.campaignLevel.id) != "n43-foundry") return false; // NIGHT45: n45-foundry walks as n43-foundry
            if (!inner.IdScripts65(rd)) return false; // NIGHT65 T1: the row says bot43: false (rebuilt under its old id): the autopilot's
            if (rd.id != lastId43 || g1_43 == null) { lastId43 = rd.id; Reset43(rd); }
            Mover m = sim.mover;
            if (rd.id == "b2")
            {
                if (JsMath.Hypot(p.x - lastPos43.x, p.z - lastPos43.z) > 4 && fallLz43Steps > 0) returns43++; // a jump back to a lip from the lava
                lastPos43 = p;
                Chain43 c = lz >= 51 || lz >= 45.5 && grounded ? g2_43 : g1_43; // gate 2 from the landing between the lavas
                if (c == g2_43 && grounded && lz >= 45.5 && lz < 57) Crossed(rd.id + "@8");
                if (c == g2_43 && lz < 57 && !grounded && g1_43.k < g1_43.walls.Length && lz < 50) c = g1_43;
                Chain43.R r = c.Drive(p, m.state, grounded, out f);
                if (r == Chain43.R.Across) { Crossed(rd.id + "@" + c.t); return c == g1_43 ? Walk43Landing(rd, p, cx, out f) : false; }
                if (r == Chain43.R.Fell)
                {
                    // in the lava: rooms43's fallReturn43 puts us back at the lip; without it, out over the nearest rim (lava burns and knocks up)
                    if (fallLz43Steps++ == 0) pitFalls++;
                    double rimLz = lz - c.t < c.gap / 2 ? c.t - 1.5 : c.t + c.gap + 1.5;
                    f = new InputFrame { moveZ = 1, yaw = Toward(p, p.x, rd.z1 - rimLz), jump = Press(true, grounded && step % 12 == 0) };
                    return true;
                }
                fallLz43Steps = 0;
                return true;
            }
            if (rd.id == "a7")
            {
                if (sim.campaign.phase == Campaign24.Phase.Cleared) return false; // on the deck: the autopilot walks north and drops to the door
                Flue43.R r = flue43.Drive(p, m.state, m.wallNormal, grounded, sim.step, m.touchingWall, out f);
                if (r == Flue43.R.Top) return false;
                return true;
            }
            if (rd.id == "a8") return Pierce43(rd, p, cx, lz, grounded, auto, out f);
            return false;
        }

        /// <summary>Gate 1 crossed: on the landing, walk to gate 2's run-up (the chain starts from there).</summary>
        bool Walk43Landing(RoomDef rd, Vec3 p, double cx, out InputFrame f)
        {
            Chain43.R r = g2_43.Drive(p, sim.mover.state, sim.mover.grounded, out f);
            return r != Chain43.R.Across;
        }

        /// <summary>a8 PIERCE: P1 from the lip, held to its end over the perch (no jump-off: it would carry you past the perch); on the perch wait for
        /// the hatch's husks (lesson 0 done, the wave's tell); then onto P2 (east), riding north firing at the nearest husk (lesson 1); the far floor
        /// is the autopilot's fight. A fall into the pit: out by the south steps.</summary>
        bool Pierce43(RoomDef rd, Vec3 p, double cx, double lz, bool grounded, InputFrame auto, out InputFrame f)
        {
            f = new InputFrame { moveZ = 1 };
            Mover m = sim.mover;
            bool riding = m.state == MoveState.WallRide;
            if (lz >= 18.2 && grounded)
            {
                // the far floor: the autopilot's aim and fire, but never back south into the pit (a fight room's goal is its spawn, across it)
                if (sim.campaign.phase == Campaign24.Phase.Cleared) return false;
                f = auto;
                bool north = Math.Cos(auto.yaw) * auto.moveZ + Math.Sin(auto.yaw) * auto.moveX > 0; // the wish's -z (north) part (wish.z = -cos y moveZ - sin y moveX)
                if (lz < 19.5 && !north) { f.moveX = 0; f.moveZ = 0; }
                f.jump = default;
                return true;
            }
            if (p.y < -1.0) // in the pit: the south steps (x -7..-4.8, z -349.5..-345) are behind P1: round its north end and the perch, then up them
            {
                pierce43Phase = 0;
                double lzp = rd.z1 - p.z;
                bool behind = p.x < cx - 4.65; // west of P1's plane (the steps' strip)
                double tx, tz;
                if (behind) { tx = cx - 5.9; tz = rd.z1 - 1.5; } // south up the steps
                else { tx = cx - 5.9; tz = rd.z1 - 8.5; } // round P1's north end (lz 7.5) to the steps' strip
                f.yaw = Toward(p, tx, tz);
                bool hop = behind && grounded && lzp < 8 && step % 24 == 0;
                f.jump = Press(behind && step % 24 < 18, hop);
                return true;
            }
            bool onPerch = grounded && p.y > -0.3 && lz > 10.5 && lz < 15.8 && p.x > cx - 1.3; // the perch: x -1..4, lz 11..15.5
            if (grounded && lz < 3.2 && p.y > -0.3) // the entry floor: the run-up on P1's line, the jump at the lip held
            {
                pierce43Phase = 0;
                double tx = cx - 3.1;
                if (Math.Abs(p.x - tx) > 0.6 && lz < 2.4) { f.yaw = Toward(p, tx, rd.z1 - Math.Max(lz + 0.5, 1.5)); return true; }
                f.yaw = Toward(p, tx, rd.z1 - 8);
                f.sprint = Press(true, false);
                if (lz >= 2.4) f.jump = Press(true, true);
                return true;
            }
            if (riding && m.wallNormal.x > 0) // on P1: along it, held, the stick into it; near its end the jump-off onto the perch
            {
                f.yaw = 0.1;
                f.moveX = -0.4;
                if (lz >= 6.6) { f.jump = Press(false, false); f.yaw = Toward(p, cx + 1.5, rd.z1 - 13.5); f.moveX = 0; return true; } // the release: the jump-off east onto the perch
                f.jump = Press(true, false);
                return true;
            }
            if (onPerch)
            {
                // wait for the hatch's husks (up to 4 s), then from the perch's east half a run and a jump north-east onto P2, held
                if (pierce43Phase == 0) { pierce43Phase = 1; pierce43Wait = 0; }
                if (pierce43Phase == 1 && sim.enemies.AliveCount == 0 && ++pierce43Wait * sim.step < 4) { f.moveZ = 0; f.yaw = Toward(p, cx + 3, rd.z1 - 20); return true; }
                pierce43Phase = 2;
                f.yaw = Toward(p, cx + 3.9, rd.z1 - 16);
                f.sprint = Press(true, false);
                if (p.x > cx + 2.6) f.jump = Press(true, true); // at the perch's east edge: the jump at P2, held
                return true;
            }
            if (riding && m.wallNormal.x < 0) // on P2: along it, held; fire at the nearest husk (lesson 1: a shot from the wall)
            {
                f.yaw = -0.1;
                f.moveX = 0.4;
                f.jump = Press(true, false);
                Enemy e = NearestAlive43(p);
                if (e != null)
                {
                    double dx = e.position.x - p.x, dz = e.position.z - p.z, d = Math.Sqrt(dx * dx + dz * dz);
                    f.yaw = Math.Atan2(-dx, -dz);
                    f.pitch = Math.Atan2(e.AimPoint.y - (p.y + 1.5), Math.Max(0.01, d));
                    // the look on the husk, the stick still north and into P2 in the world (the stick is camera-relative: a stick along the
                    // look would point off the wall, the ride's "away" end)
                    double wx = 0.4, wz = -1, fx = -Math.Sin(f.yaw), fz = -Math.Cos(f.yaw), rx = Math.Cos(f.yaw), rz = -Math.Sin(f.yaw);
                    f.moveZ = wx * fx + wz * fz;
                    f.moveX = wx * rx + wz * rz;
                    fire43 = true;
                    fireAt43 = e;
                }
                return true;
            }
            // in the air: toward P2 once off the perch (phase 2), else along P1's line (the drop onto the perch)
            if (pierce43Phase >= 2) { f.yaw = Toward(p, cx + 4.5, rd.z1 - 16); f.moveX = 0.3; f.jump = Press(true, false); return true; }
            if (lz > 6.6) { f.yaw = Toward(p, cx + 1.5, rd.z1 - 13.5); f.jump = default; return true; } // off P1 (the jump-off): onto the perch
            f.yaw = Toward(p, cx - 4.6, rd.z1 - (lz + 3)); // at P1 a few metres ahead, the stick into it (ride40's intent)
            f.moveX = -0.5;
            f.jump = Press(true, false);
            return true;
        }

        Enemy NearestAlive43(Vec3 p)
        {
            Enemy best = null;
            double bd = double.MaxValue;
            foreach (Enemy e in sim.enemies.all)
            {
                if (!e.alive || e.standIn29) continue;
                double dx = e.position.x - p.x, dz = e.position.z - p.z, d = dx * dx + dz * dz;
                if (d < bd) { bd = d; best = e; }
            }
            return best;
        }

        /// <summary>NIGHT43 L1: a8's shots from P2 (a press every 12 steps while riding at a husk); false: the other scripts' combat.</summary>
        bool Combat43(ref CombatInput c)
        {
            if (!fire43) return false;
            bool press = step % 12 == 0;
            c.fire = Press(press, press);
            c.punch = default;
            c.altFire = default;
            return true;
        }
    }
}
