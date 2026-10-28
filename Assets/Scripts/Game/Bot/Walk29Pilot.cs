// NIGHT29 (builder R, round 3): the pilot of the rebuilt levels 1-3 (Design/levels/n29/*.plan.json): scripted frames in the movement rooms (the route proofs of
// LevelValidate29.Proofs as input scripts: a jump, a dash, the wall-jump flue, the wall-ride lane with shots fired from the wall, a slide, the PULSE blast
// launch, a skate lane's lip, the two lane switches, the pickup), the autopilot's everywhere else (the fights and the plain walks). Everything goes through the
// sim's public state and the same input the player has, so it runs headless (`walk29`, Tools/SimHarness/Harness.N29.Walk.cs) and on a Player.
// Positions are ROOM-relative (lz = metres north of the room's south edge, lx = metres east of its centre line), so a plan that moves a room needs no edit here;
// the numbers (lip lz, gap) are the plans' (Tools/night/gen-n29-*.cjs): a plan change that moves a lip changes them here.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Walk29Pilot : IPlayerInput // NIGHT36 M: partial (Walk36.cs: the n36 level's rooms)
    {
        readonly SurvivalSim sim;
        readonly AutopilotInput ap;
        /// <summary>NIGHT30 S: the grapple crossings of LEVEL 4 (walk30's span line reads its counters).</summary>
        public readonly Span30Pilot span30;
        double yaw, pitch;
        int step;
        string lastId = "";
        Vec3 lastPos;
        // per-room script state (reset on a room change and on a respawn)
        bool jumped, dashed, doubled, wallMode, onDeck, westWall = true, slid, blasted, gaveUp;
        int jumpStep, phase, wp, rs, rideSteps, lastWallJumps, retries;
        // NIGHT31 L: the UNSTICK (walk30 after the spawn31 node moves: seed 2 stood in f8's pocket west of the ramp and span seed 5 on s1's crate top, both after the
        // room was cleared, for 900 s): a cleared room where the pilot moved under StuckMetres in StuckSeconds -> UnstickSeconds straight at the exit, hopping.
        // Bot-only numbers (the harness pilot's, never the player's), not gameplay.
        const double StuckFightMetres = 2; // NIGHT31 G round 4: a fight's stuck radius (the autopilot's hops wander ~1 m about a wedge)
        const double StuckMetres = 0.5, StuckSeconds = 4, UnstickSeconds = 1.5, StuckFightSeconds = 8, RopeMaxSeconds = 6;
        int ropeSteps;
        Vec3 stuckRef;
        int stuckSteps, unstickLeft;
        /// <summary>NIGHT31 L: how many times the unstick fired (walk30 may print it).</summary>
        public int unsticks;

        public Walk29Pilot(SurvivalSim sim, AutopilotInput ap)
        {
            this.sim = sim;
            this.ap = ap;
            span30 = new Span30Pilot(sim); // NIGHT30 S: the grapple part (design.n30.span; drives only in a room with grapple nodes and the ARCLINE owned)
            yaw = ap.Yaw;
            pitch = ap.Pitch;
        }

        public double Yaw => yaw;
        public double Pitch => pitch;
        public void SetLook(double y, double p) { yaw = y; pitch = p; ap.SetLook(y, p); }
        public bool Captured => ap.Captured;

        static Button Press(bool held, bool pressed) => new Button { held = held, pressed = pressed };

        void Reset()
        {
            jumped = dashed = doubled = wallMode = onDeck = slid = blasted = gaveUp = false;
            westWall = true;
            jumpStep = phase = wp = rs = rideSteps = lastWallJumps = 0;
            pierceHold = pierceWait = 0;
            Reset36(); // NIGHT36 M (Walk36.cs)
            Reset38K(); // NIGHT38 K3 (Walk38Kiln.cs)
        }

        RoomDef Room()
        {
            Campaign24 c = sim.campaign;
            if (c == null || c.room < 0 || c.room >= sim.campaignLevel.rooms.Count) return null;
            return sim.campaignLevel.rooms[c.room];
        }

        static double Toward(Vec3 from, double tx, double tz) => Math.Atan2(-(tx - from.x), -(tz - from.z));

        bool P27 => sim.pulse27 != null && sim.pulse27.active;

        /// <summary>NIGHT31 L: the PULSE gauntlet is owned (design.n31.unlocks off: from the start; on: from level 2's i1a).</summary>
        bool OwnsGauntlet => sim.loadout != null && sim.loadout.owned.Contains("gauntlet");

        // NIGHT31 L round 3: f8's standIns31 line (W wired it at 11:4x: the stand-ins die only to a charged KESTREL shot or a shot from the wall, so walk30 stalled
        // 10 of 10 in f8): once the room's real enemies are dead, stand at the line's south end (lx 4.5, lz 4), aim at the nearest stand-in, hold fire
        // PierceHoldSteps (1.2 s: the charge's 0.4 s hold + 0.5 s fill, with margin; the pilot's number), release, wait PierceWaitSteps (the 4 s cooldown + margin)
        const int PierceHoldSteps = 144, PierceWaitSteps = 600;
        bool pierceActive;
        int pierceHold, pierceWait;

        Enemy NearestStandIn(Vec3 p, RoomDef rd)
        {
            Enemy best = null;
            double bd = double.MaxValue;
            foreach (Enemy e in sim.enemies.all)
            {
                if (!e.alive || !e.standIn29) continue;
                if (e.position.x < rd.x0 || e.position.x > rd.x1 || e.position.z < rd.z0 || e.position.z > rd.z1) continue; // this room's line only (f5's strafer can still be up)
                double d = JsMath.Hypot(e.position.x - p.x, e.position.z - p.z);
                if (d < bd) { bd = d; best = e; }
            }
            return best;
        }

        InputFrame Pierce(RoomDef rd, Vec3 p, double cx, Enemy si)
        {
            pierceActive = true;
            double tx = cx + 4.5, tz = rd.z1 - 4;
            var f = new InputFrame();
            if (JsMath.Hypot(p.x - tx, p.z - tz) > 0.6 && pierceHold == 0) { f.moveZ = 1; f.yaw = Toward(p, tx, tz); pierceActive = false; return Keep(f); }
            double dx = si.position.x - p.x, dz = si.position.z - p.z, dist = Math.Sqrt(dx * dx + dz * dz);
            f.yaw = Math.Atan2(-dx, -dz);
            f.pitch = Math.Atan2(si.AimPoint.y - (p.y + 1.5), Math.Max(0.01, dist));
            if (pierceWait > 0) { pierceWait--; return Keep(f); }
            if (pierceHold == 0) pierceHold = PierceHoldSteps;
            else if (--pierceHold == 0) pierceWait = PierceWaitSteps;
            return Keep(f);
        }

        int AliveNonStandIn()
        {
            int n = 0;
            foreach (Enemy e in sim.enemies.all) if (e.alive && !e.standIn29) n++;
            return n;
        }

        /// <summary>True while the BLAST script of f9 / i2 runs (the gauntlet in hand, the glove full, the room's enemies dead, the pit not yet crossed).</summary>
        bool tankMode, tankNear;

        // the nearest alive tank when it is the job now: no other enemy alive, or the tank is within 5 m
        Enemy TankTarget(Vec3 p)
        {
            Enemy best = null;
            double bd = double.MaxValue;
            int others = 0;
            foreach (Enemy e in sim.enemies.all)
            {
                if (!e.alive || e.standIn29) continue;
                if (!e.tank29) { others++; continue; }
                double d = (e.position.x - p.x) * (e.position.x - p.x) + (e.position.z - p.z) * (e.position.z - p.z);
                if (d < bd) { bd = d; best = e; }
            }
            return best != null && (others == 0 || bd < 25) ? best : null;
        }

        /// <summary>NIGHT34 A: the rooms this pilot's switch scripts itself (the span pilot never drives them).</summary>
        static readonly System.Collections.Generic.HashSet<string> Scripted34 = new System.Collections.Generic.HashSet<string> { "f2", "f3", "f4", "f5", "f5b", "f7", "f8", "f9", "i2", "i4", "i5", "k1", "k2", "k4" };

        /// <summary>NIGHT34 A: the span pilot may drive this room: not a scripted room, and the level is one built around ring routes (an id with
        /// "span" or "spire": NIGHT30 / NIGHT32's grapple levels, where Span30Pilot was written and measured).</summary>
        bool Span30Rooms(RoomDef rd)
        {
            if (rd == null) return false;
            if (SwingAlso34.Contains(rd.id)) return !(sim.n35.grapple35 && !sim.grapple35Here); // NIGHT35 M: no grapple on levels 1-3 under grapple35: the room's own script / the autopilot

            if (Scripted34.Contains(rd.id)) return false;
            string lid = sim.campaignLevel != null ? sim.campaignLevel.id ?? "" : "";
            return lid.Contains("span") || lid.Contains("spire");
        }

        /// <summary>NIGHT34 A: the n34-foundry rooms the span pilot drives well when they have nodes (round 2: 10 / 10 with the span pilot in f5, f5b, f8,
        /// f9b and f10; round 3 with their own script or the autopilot instead: 9 / 10, seed 7 stuck in f5b, then seed 1 stuck in f9b after its clear).
        /// With no nodes in the room Drive returns false and the room's script or the autopilot runs, as before.</summary>
        static readonly System.Collections.Generic.HashSet<string> SwingAlso34 = new System.Collections.Generic.HashSet<string> { "f5", "f5b", "f8", "f9b", "f10", "i1a", "i1b" };

        bool blastActive;
        bool rideFire;

        public InputFrame SampleMove(bool active)
        {
            InputFrame auto = ap.SampleMove(active);
            step++;
            RoomDef rd = Room();
            blastActive = rideFire = pierceActive = false;
            Vec3 p = sim.mover.position;
            if (rd != null && rd.id != lastId) retries = 0; // NIGHT30: the ride retries count per room (a respawn keeps them)
            if (JsMath.Hypot(p.x - lastPos.x, p.z - lastPos.z) > 6 || rd == null || rd.id != lastId) { Reset(); lastId = rd != null ? rd.id : ""; } // a respawn or a new room
            lastPos = p;
            if (!active || rd == null || sim.campaign.phase == Campaign24.Phase.None) return Keep(auto);
            double cx = (rd.x0 + rd.x1) / 2, lz = rd.z1 - p.z, lx = p.x - cx;
            bool grounded = sim.mover.grounded;
            bool cleared = sim.campaign.phase == Campaign24.Phase.Cleared;
            // round 3: also in a fight when the AUTOPILOT asks to move and does not (span seed 6: wedged between s6's ramp side and the deck's north face, chasing
            // the deck's spitters), after StuckFightSeconds; a scripted hold (f5b's moveZ 0 on the entry floor) never counts
            bool wantsMove = Math.Abs(auto.moveZ) + Math.Abs(auto.moveX) > 0.1 || rd.id != "f5b"; // (the autopilot stood still there, shooting at spitters it could not hit)
            // NIGHT31 G round 4 (walk30 span seed 6, "frozen in the air" at (-3.6, 0.9, -157.8): it was NOT in the air and NOT on a rope: grounded against s6's deck
            // face, the autopilot hopping at the deck's last spitter, every hop's height reset the stuck reference, so 850 s with 0 unsticks): the height counts
            // only on the ground (a hop in place is still stuck)
            // only on the ground (a hop in place is still stuck), and in a fight the reference moves only past StuckFightMetres (the hops jitter it ~1 m)
            double stuckR = cleared ? StuckMetres : StuckFightMetres;
            if (JsMath.Hypot(p.x - stuckRef.x, p.z - stuckRef.z) > stuckR || (grounded && Math.Abs(p.y - stuckRef.y) > stuckR) || !(cleared || wantsMove)) { stuckRef = p; stuckSteps = 0; }
            else if (++stuckSteps * sim.step >= (cleared ? StuckSeconds : StuckFightSeconds)) { stuckSteps = 0; unstickLeft = MovementMath.StepsFor(UnstickSeconds, sim.step); unsticks++; }
            if (unstickLeft > 0) // NIGHT31 L: the unstick (see the fields)
            {
                unstickLeft--;
                // round 3 (walk30 iceline seeds 1 and 10: wedged between i5's divider and the east block, south of it, where a run at the exit hits the block):
                // odd unsticks run at the exit, even ones back toward the room's entry (then the room's own script or the autopilot takes over again)
                Vec3 to = unsticks % 2 == 1 ? rd.exit : rd.spawn;
                return Keep(new InputFrame { moveZ = 1, yaw = Toward(p, to.x, to.z), jump = Press(true, grounded && step % 24 == 0) });
            }
            // round 3 (span seed 6: the span pilot hooked s6's deck node and swung under it for 900 s, in the air, the room's last spitter alive): a rope held longer
            // than RopeMaxSeconds is let go with a jump (the Mover's own release in the air), the grapple button up
            ropeSteps = sim.mover.rope.phase != RopePhase.None ? ropeSteps + 1 : 0;
            if (ropeSteps * sim.step >= RopeMaxSeconds) { ropeSteps = 0; unsticks++; return Keep(new InputFrame { moveZ = 1, yaw = yaw, jump = Press(true, true) }); }
            // NIGHT30 S: grapple nodes in this room: swing across them first (round 3: after the unstick)
            // NIGHT34 A: only where the grapple is the room's crossing: a room with its own script below (f7's slide, f9's ride, ...) keeps its script,
            // and only the levels built around ring routes (THE SPAN, THE SPIRE) hand a room to the span pilot. NIGHT34's lines34 nodes in the foundry /
            // kiln / ice line rooms are an extra line for a player, not the route (round 2: a node in f7 stalled 0 / 10 seeds, i6 0 / 10).
            if (Level38Span && Room38Span(rd, p, grounded, cleared, auto, out InputFrame f38s)) return Keep(f38s); // NIGHT38 O2: n38-span's rooms p1-p5 (Walk38Span.cs)
            if (Span30Rooms(rd) && span30.Drive(rd, auto, out InputFrame spanFrame)) return Keep(spanFrame);
            if (Level36 && Room36(rd, p, cx, lz, lx, grounded, cleared, auto, out InputFrame f36)) return Keep(f36); // NIGHT36 M: n36-foundry's rooms (Walk36.cs)
            boost38K = false;
            if (Level38K && Room38K(rd, p, cx, lz, grounded, auto, out InputFrame f38k)) return Keep(f38k); // NIGHT38 K3: n38-kiln's rooms (Walk38Kiln.cs)
            if (Ice38(out InputFrame f38i)) return Keep(f38i); // NIGHT38 S2: n38-iceline's THE RUN (Walk38Iceline.cs)
            if (Ice40(out InputFrame f40i)) return Keep(f40i); // NIGHT40 L2: n40-iceline (Walk40Iceline.cs)
            if (Foes40(out InputFrame f40e)) return Keep(f40e); // NIGHT40 E3: the DRONE / WARDEN / CHARGER answers (Foes40Combat.cs)
            if (Level38Spire && Room38Spire(rd, p, cx, lz, lx, grounded, cleared, auto, out InputFrame f38c)) return Keep(f38c); // NIGHT38 C2 (Walk38Spire.cs)
            var f = new InputFrame { moveZ = 1 };
            bool f9Ride = rd.id == "f9" && !OwnsGauntlet; // NIGHT31 L: f9 ridden (no gauntlet)
            if (p.y < -0.3 && p.y > -1.2 && (rd.id == "f5" || rd.id == "f5b" || f9Ride)) // NIGHT30 round 3: in a lava room's trench: out to the right rim (lava burns and knocks up; the autopilot's graph has no way out)
            {
                double tx = cx, tz = rd.z1 - 3; // not cleared: back to the entry floor (ride again)
                if (f9Ride && lz > 13) tz = rd.z1 - 19.5; // NIGHT31 L: f9: past the middle, out over the north rim (the far floor)
                else if (cleared && rd.id == "f5") { tx = cx; tz = rd.z1 - 22; } // the exit platform
                else if (cleared) { tx = p.x; tz = rd.z1 - 23; } // f5b: straight north up the catch steps (the deck's whole width)
                var o = new InputFrame { moveZ = 1, yaw = Toward(p, tx, tz), jump = Press(true, grounded && step % 12 == 0) };
                return Keep(o);
            }
            Enemy tank = TankTarget(p);
            tankMode = tank != null;
            if (tankMode) // the TANK dies to one gauntlet punch (x10; the pistol does x0.15): walk into it and swing
            {
                double dx = tank.position.x - p.x, dz = tank.position.z - p.z, dist = Math.Sqrt(dx * dx + dz * dz);
                f.yaw = Math.Atan2(-dx, -dz);
                f.pitch = Math.Atan2(tank.AimPoint.y - (p.y + 1.5), Math.Max(0.01, dist));
                f.moveZ = dist > 1.8 ? 1 : 0;
                tankNear = dist < 3.2;
                return Keep(f);
            }
            switch (rd.id)
            {
                case "f2": return GapRoom(rd, p, cx, lz, grounded, 7, 3.5, false, auto);
                case "f3": return GapRoom(rd, p, cx, lz, grounded, 8, 6, true, auto);
                case "f4": return Flue(rd, p, cx, lz, grounded, auto);
                case "f5": return Ride(rd, p, lz, grounded, 21, auto, true); // the lane: north to lz 21
                case "f5b": // NIGHT30 (n30-foundry): fight the deck's spitters from the entry floor, then the W lane, a wall jump at lz 9 across to the E lane, onto the deck
                    if (!cleared) { if (lz > 3.0) { auto.moveZ = 0; auto.moveX = 0; } return Keep(auto); } // hold the entry floor (the lava starts at lz 5.5) and shoot
                    if (lz > 6 && lz < 21 && p.y < 1.9 && grounded) { f.yaw = Toward(p, p.x, rd.z1 - 24); f.jump = Press(true, step % 12 == 0); return Keep(f); } // on the catch stair: hop straight up onto the deck
                    if (p.y < -0.3 && lz > 12 && lz < 21.5) // dropped short of the deck: climb the catch steps (west corner, lz 18-21)
                    {
                        f.yaw = Toward(p, cx - 5.5, rd.z1 - 23);
                        f.jump = Press(true, grounded && step % 24 == 0);
                        return Keep(f);
                    }
                    return Ride(rd, p, lz, grounded, 23, auto, false, 9);
                case "f7":
                    if (lz >= 14.3) return Keep(auto);
                    f.yaw = Toward(p, cx, rd.z0 - 20);
                    if (lz >= 3.5 && lz < 13.8) { f.slide = Press(true, !slid); slid = true; } // held to the tunnel's far end (lz 14)
                    return Keep(f);
                case "f8":
                    {
                        Enemy si = AliveNonStandIn() == 0 && !cleared ? NearestStandIn(p, rd) : null; // NIGHT31 L: the pierce line (standIns31)
                        if (si != null) return Pierce(rd, p, cx, si);
                        return Keep(auto);
                    }
                case "f9":
                    // NIGHT31 L: no gauntlet (design.n31.unlocks: level 1 is KESTREL-only): fight, then the W wall lane over the lava (the plan's `ride` proof), as f5
                    if (!OwnsGauntlet) return AliveNonStandIn() > 0 ? Keep(auto) : Ride(rd, p, lz, grounded, 19, auto, false);
                    return Blast(rd, p, cx, lz, grounded, 8, 10, auto);
                case "i2": return Blast(rd, p, cx, lz, grounded, 8, 10, auto);
                case "i4":
                    if (lz >= 23.5) return Keep(auto);
                    f.yaw = Toward(p, cx, rd.z0 - 10);
                    if (grounded && lz >= 21.0 && !jumped) { f.jump = Press(true, true); jumped = true; }
                    else if (jumped && !grounded) f.jump = Press(true, false);
                    if (jumped && grounded && lz < 20) jumped = false; // back before the lip (a respawn): again
                    return Keep(f);
                case "i5":
                    {
                        double[][] via = { new[] { -3.5, 4.5 }, new[] { -3.5, 10 }, new[] { 3.5, 10.5 }, new[] { 3.5, 17 } };
                        if (wp >= via.Length) return Keep(auto);
                        double tx = cx + via[wp][0], tz = rd.z1 - via[wp][1];
                        if (JsMath.Hypot(p.x - tx, p.z - tz) < 1.0) { wp++; if (wp >= via.Length) return Keep(auto); tx = cx + via[wp][0]; tz = rd.z1 - via[wp][1]; }
                        f.yaw = Toward(p, tx, tz);
                        return Keep(f);
                    }
                case "k1":
                    if (phase == 0)
                    {
                        double tz = rd.z1 - 5.5;
                        if (JsMath.Hypot(p.x - cx, p.z - tz) < 0.9) phase = 1;
                        else { f.yaw = Toward(p, cx, tz); return Keep(f); }
                    }
                    return Keep(auto);
                case "k2": // the 2 m bridge (lz 6..16, on the room's centre line cx + 0 .. the bridge is x cx-1..cx+1 in the plan): straight across once the fight is won
                    if (cleared && lz < 17) { f.yaw = Toward(p, cx, rd.z0 - 10); f.moveX = Math.Max(-1, Math.Min(1, -(p.x - cx) * 0.8)); return Keep(f); }
                    return Keep(auto);
                case "k4":
                    if (cleared && lz < 24) return Ride(rd, p, lz, grounded, 26, auto, false);
                    return Keep(auto);
            }
            return Keep(auto);
        }

        InputFrame Keep(InputFrame f)
        {
            yaw = f.yaw;
            pitch = f.pitch;
            return f;
        }

        // a jump (and a second jump or a dash) over a pit that spans the room
        InputFrame GapRoom(RoomDef rd, Vec3 p, double cx, double lz, bool grounded, double lipLz, double gap, bool dash, InputFrame auto)
        {
            if (lz >= lipLz + gap + 0.5) return Keep(auto);
            if (grounded && lz < lipLz - 1.5) { jumped = dashed = doubled = false; }
            var f = new InputFrame { moveZ = 1, yaw = Toward(p, cx, rd.z0 - 6) };
            if (grounded && !jumped && lz >= lipLz - 1.2 && lz < lipLz + 1) { f.jump = Press(true, true); jumped = true; jumpStep = step; }
            else if (jumped && !dashed && step - jumpStep >= (dash ? 26 : 36))
            {
                dashed = true;
                if (dash) f.dash = Press(true, true); else { f.jump = Press(true, true); doubled = true; }
            }
            else if (dash && dashed && !doubled && sim.mover.state != MoveState.Dash && !grounded) { f.jump = Press(true, true); doubled = true; }
            else if (jumped && !grounded) f.jump = Press(true, false);
            return Keep(f);
        }

        // the wall-jump flue (LevelValidate24.WallClimb as input): run to the mouth beside the west wall, jump into the ride, wall-jump 0.3 s into every ride
        InputFrame Flue(RoomDef rd, Vec3 p, double cx, double lz, bool grounded, InputFrame auto)
        {
            if (grounded && p.y >= 3.3) onDeck = true;
            var f = new InputFrame { moveZ = 1 };
            if (onDeck) // the graph has no ramp edges: walk straight down the ramp (north), then the autopilot
            {
                if (lz >= rd.z1 - rd.z0 - 2.5) return Keep(auto);
                f.yaw = Toward(p, cx, rd.z0 - 5);
                return Keep(f);
            }
            if (!wallMode && lz >= 4.5 && Math.Abs(p.x - (cx - 1.2)) < 0.6) wallMode = true;
            if (!wallMode)
            {
                f.yaw = lz < 4.5 ? Toward(p, cx - 1.2, rd.z1 - 4.6) : 0;
                if (lz >= 4.5) f.yaw = Toward(p, cx - 1.2, rd.z1 - lz - 3);
                return Keep(f);
            }
            f.yaw = westWall ? 0.25 : -0.25;
            bool onWall = sim.mover.state == MoveState.WallRide;
            rideSteps = onWall ? rideSteps + 1 : 0;
            if (sim.mover.flow34.ride35.rules.on) return Keep(Flue35(f, onWall, grounded)); // NIGHT35 M: the Lucio ride (design.n35.wallride35)
            if (grounded) f.jump = Press(true, true);
            else if (onWall && rideSteps * sim.step >= 0.3) f.jump = Press(true, true);
            else f.jump = Press(true, false);
            if (sim.mover.wallJumpsLeft < lastWallJumps || lastWallJumps == 0 && sim.mover.wallJumpsLeft < sim.mover.MaxWallJumps) westWall = !westWall;
            lastWallJumps = sim.mover.wallJumpsLeft;
            return Keep(f);
        }

        // NIGHT35 M: the flue under wallride35, as a person climbs it: the jump held into every ride (a HOLD ride), 0.3 s into it the jump RELEASED
        // looking up (the jump-off: up to upHigh), then pressed and held again 0.1 s later for the opposite wall (a press too soon after the jump-off
        // would wall-jump off the same wall). The side flips on every jump-off (the WallJump event; a jump-off spends no airtime wall jump).
        int released35 = -1, lastOffs35, step35;
        InputFrame Flue35(InputFrame f, bool onWall, bool grounded)
        {
            step35++;
            int offs = sim.mover.flow34.ride35.jumpOffs;
            if (offs != lastOffs35) { lastOffs35 = offs; westWall = !westWall; f.yaw = westWall ? 0.25 : -0.25; }
            if (grounded) { released35 = -1; f.jump = Press(true, true); return f; }
            if (onWall && rideSteps * sim.step >= 0.3)
            {
                if (sim.mover.HoldRide35) { f.jump = Press(false, false); f.pitch = 0.7; released35 = step35; return f; }
                f.jump = Press(true, true); // a ride without the jump held (the web's): the press is the wall jump
                return f;
            }
            if (released35 >= 0 && step35 - released35 < 12) { f.jump = Press(false, false); f.pitch = 0.7; return f; }
            if (released35 >= 0) { released35 = -1; f.jump = Press(true, true); return f; }
            f.jump = Press(true, false);
            return f;
        }

        // the wall lane over a hazard floor (the `ride` proof): from beside the west wall, a run-up, jump, lean, dash; shots are fired while riding
        InputFrame Ride(RoomDef rd, Vec3 p, double lz, bool grounded, double endLz, InputFrame auto, bool fire, double wallJumpLz = -1)
        {
            // NIGHT30 round 3: back on the entry floor after a ride that did not finish the room (a lava room: knocked out of the lava): ride again, at most 6 times
            if ((phase >= 2 || gaveUp) && grounded && p.y > -0.3 && lz < 5 && retries < 6 && (rd.id == "f5" || rd.id == "f5b" || rd.id == "f9")) { retries++; phase = 0; rs = 0; dashed = doubled = gaveUp = false; }
            if (wallJumpLz > 0 && phase == 1 && !doubled && sim.mover.state == MoveState.WallRide && lz >= wallJumpLz) // NIGHT30: one wall jump off the lane (f5b)
            {
                doubled = true;
                rs++;
                return Keep(new InputFrame { moveZ = 1, yaw = 0, jump = Press(true, true) });
            }
            if (gaveUp || phase >= 2 || lz >= endLz) return Keep(auto);
            var f = new InputFrame { moveZ = 1 };
            if (phase == 0)
            {
                double tx = rd.x0 + 1.2, tz = rd.z1 - 1.0;
                if (JsMath.Hypot(p.x - tx, p.z - tz) < 0.7 && grounded) { phase = 1; rs = 0; }
                else
                {
                    f.yaw = Toward(p, tx, tz); f.moveZ = JsMath.Hypot(p.x - tx, p.z - tz) > 0.7 ? 1 : 0;
                    if (p.y < -0.3) f.jump = Press(true, grounded && step % 24 == 0); // NIGHT30: in a lava trench (no knock with design.n30.lava off): jump out toward the start
                    return Keep(f);
                }
            }
            rs++;
            int jumpAt = 60; // 0.5 s of run-up
            f.yaw = rs >= jumpAt ? 0.25 : 0;
            if (rs == jumpAt) f.jump = Press(true, true);
            else if (rs > jumpAt) f.jump = Press(true, grounded);
            if (!dashed && rs > jumpAt + 6 && !grounded) { dashed = true; f.dash = Press(true, true); }
            rideFire = fire && sim.mover.state == MoveState.WallRide;
            if (rs > jumpAt + 30 && grounded) phase = 2; // landed after the ride: the autopilot takes it from here (on the floor or past the strip)
            if (rs > 600) gaveUp = true;
            return Keep(f);
        }

        // the PULSE blast over the pit (f9, i2): the gauntlet in hand, run to the lip, jump, LT (altFire) in the air aimed 25 degrees up
        InputFrame Blast(RoomDef rd, Vec3 p, double cx, double lz, bool grounded, double lipLz, double gap, InputFrame auto)
        {
            if (lz >= lipLz + gap + 0.5) return Keep(auto);
            bool ready = P27 && sim.pulse27.Ready && AliveNonStandIn() == 0;
            if (!ready && !jumped) return Keep(auto);
            if (grounded && lz < lipLz - 3 && jumped) { jumped = blasted = doubled = false; } // a respawn
            blastActive = true;
            var f = new InputFrame { moveZ = 1, yaw = Toward(p, cx, rd.z0 - 6) };
            if (!jumped && sim.loadout.Weapon != "gauntlet") f.moveZ = 0; // wait for the gauntlet in hand: a blast without it is denied
            else if (grounded && !jumped && lz >= lipLz - 1.0) { f.jump = Press(true, true); jumped = true; jumpStep = step; }
            else if (jumped && !grounded) f.jump = Press(true, false);
                        if (jumped) f.pitch = 0.44;
            return Keep(f);
        }

        public CombatInput SampleCombat(bool active)
        {
            CombatInput c = ap.SampleCombat(active);
            if (active && Ice40Combat(out CombatInput c40)) return c40; // NIGHT40 L2: n40-iceline's parry swing and blast (Walk40Iceline.cs)
            if (active && Foes40Combat(out CombatInput c40e)) return c40e; // NIGHT40 E3 (Foes40Combat.cs)
            if (!active) return c;
            if (tankMode)
            {
                int g = sim.loadout.owned.IndexOf("gauntlet");
                c.weapon = g >= 0 && sim.loadout.Weapon != "gauntlet" ? g : -1;
                c.punch = default;
                if (P27) c.fire = Press(tankNear, tankNear);
                else { c.fire = default; c.altFire = default; }
            }
            else if (blastActive)
            {
                int g = sim.loadout.owned.IndexOf("gauntlet");
                c.weapon = g >= 0 && sim.loadout.Weapon != "gauntlet" ? g : -1;
                c.fire = default;
                c.punch = default;
                bool blast = jumped && !blasted && step - jumpStep >= 10 && !sim.mover.grounded;
                c.altFire = Press(blast, blast);
                if (blast) blasted = true;
            }
            else if (pierceActive) // NIGHT31 L: f8's pierce line: the KESTREL in hand, fire HELD for the charge, then released (the charged shot fires on release)
            {
                int k = sim.loadout.owned.IndexOf("revolver");
                c.weapon = k >= 0 && sim.loadout.Weapon != "revolver" ? k : -1;
                c.punch = default;
                c.altFire = default;
                bool hold = pierceHold > 0;
                c.fire = Press(hold, hold && pierceHold == PierceHoldSteps);
            }
            else if (boost38K) Combat38K(ref c); // NIGHT38 K3: n38-kiln's boost gates (Walk38Kiln.cs)
            else if (rideFire) { bool t = step % 12 < 3; c.fire = Press(t, step % 12 == 0); } // NIGHT31 L: TAPS from the wall (a held trigger now charges first: design.n31.charge; the ride is shorter than the 0.4 s hold, so a hold fired nothing)
            return c;
        }
    }
}
