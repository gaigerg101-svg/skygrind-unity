// NIGHT25 (builder L): the tutorial's pilot, shared by the harness (`tutorial`) and the Player reel (FeelReel.N25.Level.cs): scripted frames in the movement
// stages (by position, on the real Mover: a slide tunnel and a wall-jump flue need the real verbs, the autopilot's graph has neither), the autopilot's
// everywhere else (the fights). Everything goes through the sim's public state, so it runs on a Player (no Editor). Stage ids are Design/levels/tutorial.plan.json's.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class TutorialPilot25 : IPlayerInput
    {
        readonly SurvivalSim sim;
        readonly AutopilotInput ap;
        double yaw, pitch;
        bool wallMode, westWall = true, onDeck, jumped, dashed, doubled;
        int rideSteps, lastWallJumps, jumpStep = -1, step, slideStep = -1;

        public TutorialPilot25(SurvivalSim sim, AutopilotInput ap)
        {
            this.sim = sim;
            this.ap = ap;
            yaw = ap.Yaw;
            pitch = ap.Pitch;
        }

        public double Yaw => yaw;
        public double Pitch => pitch;
        public void SetLook(double y, double p) { yaw = y; pitch = p; ap.SetLook(y, p); }
        public bool Captured => ap.Captured;
        // stage 1.5 (the fist): 0 = the autopilot's; 1 = punchWhileFiring (the revolver's fire and the punch key together); 2 = charge the PULSE on the nearest husk
        // (the gauntlet in hand, the left click held within reach); 3 = a full notch: run at the pit's lip, jump, and LET GO of the left click in the air (the launch)
        int s5Mode, s5JumpStep = -1;
        bool s5Jumped, s5Doubled;

        static int SlotOf(Loadout l, string weapon) => l.owned.IndexOf(weapon);

        public CombatInput SampleCombat(bool active)
        {
            CombatInput c = ap.SampleCombat(active);
            if (active && LanternFight()) // NIGHT27: level 1's boss is THE LANTERN (boss1 on): its own pilot
            {
                lanternPilot.Step(sim, yaw, sim.run.steps);
                return lanternPilot.Combat;
            }
            if (active && HulkFight())
            {
                hulkPilot.Step(sim, yaw, sim.run.steps);
                return hulkPilot.Combat;
            }
            if (!active || s5Mode == 0) return c;
            Loadout l = sim.loadout;
            if (s5Mode == 1)
            {
                int gun = SlotOf(l, "revolver");
                c.weapon = gun >= 0 && l.Weapon != "revolver" ? gun : -1;
                c.fire = Press(true, true);
                c.punch = Press(true, true);
                return c;
            }
            int pulse = SlotOf(l, "gauntlet");
            c.weapon = pulse >= 0 && l.Weapon != "gauntlet" ? pulse : -1;
            c.punch = default;
            bool swing = s5Mode == 3 ? !(s5Jumped && step - s5JumpStep >= 14) : s5Near;
            if (P27) // NIGHT27 pulse27: the click swings (2.5 s), kills fill the pips; LT, pressed once in the air with a full glove, is the blast + boost
            {
                c.fire = Press(swing && s5Mode == 2, swing && s5Mode == 2);
                bool blast = s5Mode == 3 && s5Jumped && !s5Blasted && step - s5JumpStep >= 10;
                c.altFire = Press(blast, blast);
                if (blast) s5Blasted = true;
            }
            else if (Pulse26Lt) // NIGHT26: the PULSE blast is the left trigger (design.n26.pulseLT): altFire is HELD to charge (the gauntlet swings by itself and every landed hit fills the charge), RELEASED to launch
            {
                c.fire = default;
                c.altFire = Press(swing, swing && !ltHeld);
                ltHeld = swing;
            }
            else c.fire = Press(swing, swing);
            return c;
        }

        bool s5Near, s5Blasted;
        bool P27 => sim.pulse27 != null && sim.pulse27.active;
        bool ltHeld;
        readonly HulkPilot26 hulkPilot = new HulkPilot26();
        readonly LanternPilot27 lanternPilot = new LanternPilot27();
        bool LanternFight() => sim.n27.boss1 && sim.campaign != null && sim.campaign.room >= 0 && sim.campaignLevel.rooms[sim.campaign.room].kind == "boss" && sim.campaignLevel.rooms[sim.campaign.room].boss == "hulk" && sim.campaign.phase == Campaign24.Phase.Fight && sim.lantern27 != null && sim.lantern27.Alive;
        /// <summary>The boss room's hulk is alive and fought now (the level1 block on; off, the room clears at once and there is nothing to fight).</summary>
        bool HulkFight() => sim.campaign != null && sim.campaign.room >= 0 && sim.campaignLevel.rooms[sim.campaign.room].kind == "boss" && sim.campaignLevel.rooms[sim.campaign.room].boss == "hulk" && sim.campaign.phase == Campaign24.Phase.Fight && sim.hulk26 != null && sim.hulk26.Alive;
        bool Pulse26Lt => !N26Blocks.ForceOff && N26Blocks.From(sim.cfg).pulseLT;
        int lipI;
        const double S5Lip = -375; // NIGHT26 coordinates; NIGHT27 moves the stage and the pilot works room-relative (Dz)
        string lastId = "";
        // NIGHT26: the south lips of the gaps in Design/levels/tutorial.plan.json's p1 (islands 7 m apart) and p4 (the wall hall's pits); a plan change moves them
        static readonly double[] Lips1 = { -84, -100, -116 }, Lips4 = { -252, -270, -288 };
        static readonly bool Debug25 = Environment.GetEnvironmentVariable("TUTORIAL_DEBUG") == "1";

        static Button Press(bool held, bool pressed) => new Button { held = held, pressed = pressed };

        // NIGHT27: each stage's spawn z in the NIGHT26 plan (the slide room `sl` is the old s2's tunnel half, whose spawn point was z 110);
        // the pilot's z numbers are those plans', and a stage that moved is offset by where its spawn is now.
        static double OldSpawnZ(string id)
        {
            switch (id)
            {
                case "s1": return 168;
                case "s2": return 132;
                case "sl": return 110;
                case "s3a": return 82;
                case "s3c": return -24;
                case "p1": return -74;
                case "p3": return -190;
                case "p4": return -244;
                case "s5": return -358;
                default: return 0;
            }
        }

        double Dz(string id)
        {
            Campaign24 c = sim.campaign;
            RoomDef r = null;
            if (c != null && c.room >= 0 && sim.campaignLevel.rooms[c.room].id == id) r = sim.campaignLevel.rooms[c.room];
            else if (sim.campaignLevel != null) r = sim.campaignLevel.rooms.Find(x => x.id == id);
            return r == null ? 0 : r.spawn.z - OldSpawnZ(id);
        }

        string RoomId()
        {
            Campaign24 c = sim.campaign;
            if (c == null || c.room < 0) return "s1";
            return sim.campaignLevel.rooms[c.room].id;
        }

        public InputFrame SampleMove(bool active)
        {
            InputFrame auto = ap.SampleMove(active);
            step++;
            string id = RoomId();
            if (active && LanternFight()) // NIGHT27: THE LANTERN (boss1)
            {
                lanternPilot.Step(sim, yaw, sim.run.steps);
                yaw = lanternPilot.Move.yaw;
                pitch = lanternPilot.Move.pitch;
                return lanternPilot.Move;
            }
            if (active && HulkFight()) // NIGHT26: the boss room: the pilot fights THE SLAG HULK itself (HulkPilot26)
            {
                hulkPilot.Step(sim, yaw, sim.run.steps);
                yaw = hulkPilot.Move.yaw;
                pitch = hulkPilot.Move.pitch;
                return hulkPilot.Move;
            }
            if (id != lastId) { lastId = id; if (id == "p3") { wallMode = onDeck = false; westWall = true; rideSteps = lastWallJumps = 0; } } // NIGHT26: the flue again in p3: its state starts fresh
            // the fist stage's pit is crossed by the pilot once the fight is won (the autopilot's graph crossing fails on some seeds: a respawn loop)
            bool s5Cross = id == "s5" && sim.campaign != null && sim.campaign.phase == Campaign24.Phase.Cleared;
            s5Mode = 0;
            Campaign24 camp = sim.campaign;
            if (id == "s5" && camp != null && camp.phase == Campaign24.Phase.Fight && camp.lesson != null && !camp.lesson.Done)
            {
                LessonKind kind = camp.lesson.Current.kind;
                if (kind == LessonKind.PunchWhileFiring) s5Mode = 1;
                else if (kind == LessonKind.FlickCrossed)
                {
                    Fist24 pf = sim.loadout.pulseFist;
                    s5Mode = P27 ? (sim.pulse27.Ready || s5Jumped && step - s5JumpStep < 150 ? 3 : 2) : pf != null && pf.Tier >= 1 && pf.CanFlick || s5Jumped && step - s5JumpStep < 150 ? 3 : 2; // the launch is flown to its landing (the charge is spent, but the crossing is not over)
                }
            }
            if (s5Mode >= 2 && active)
            {
                var sp = sim.mover.position;
                var fr = new InputFrame { yaw = 0, pitch = 0 };
                if (s5Mode == 3)
                {
                    const double lipS = S5Lip;
                    if ((sp.z - Dz("s5")) > lipS + 6 && sim.mover.grounded) { s5Jumped = false; s5Doubled = false; s5Blasted = false; }
                    fr.moveZ = 1;
                    fr.moveX = Math.Max(-1, Math.Min(1, -sp.x * 0.5));
                    fr.pitch = 0.15; // a little up: the launch carries
                    if (sim.mover.grounded && !s5Jumped && (sp.z - Dz("s5")) <= lipS + 1.2 && (sp.z - Dz("s5")) > lipS - 1) { fr.jump = Press(true, true); s5Jumped = true; s5JumpStep = step; }
                    else if (s5Jumped && !sim.mover.grounded && !s5Doubled && step - s5JumpStep >= 30) { fr.jump = Press(true, true); s5Doubled = true; } // the double jump after the launch
                    else if (s5Jumped && !sim.mover.grounded) fr.jump = Press(true, false);
                }
                else
                {
                    s5Jumped = false;
                    Enemy best = null;
                    double bd = double.MaxValue;
                    foreach (Enemy e in sim.enemies.all)
                    {
                        if (!e.alive) continue;
                        double d = (e.position.x - sp.x) * (e.position.x - sp.x) + (e.position.z - sp.z) * (e.position.z - sp.z);
                        if (d < bd) { bd = d; best = e; }
                    }
                    s5Near = false;
                    if (best != null)
                    {
                        double dx = best.position.x - sp.x, dz = best.position.z - sp.z, dist = Math.Sqrt(bd);
                        fr.yaw = Math.Atan2(-dx, -dz);
                        fr.pitch = Math.Atan2(best.AimPoint.y - (sp.y + 1.5), Math.Max(0.01, dist));
                        fr.moveZ = dist > 1.8 ? 1 : 0;
                        s5Near = dist < 3.2;
                    }
                }
                if (Debug25 && (step % 30 == 0 || s5Jumped && step % 4 == 0)) Console.WriteLine($"    dbg s5 mode {s5Mode} step {step} weapon {sim.loadout.Weapon} near {s5Near} charge {sim.loadout.pulseFist?.charge:0.00} tier {sim.loadout.pulseFist?.Tier} canFlick {sim.loadout.pulseFist?.CanFlick} pos ({sp.x:0.0},{sp.y:0.0},{sp.z:0.0}) alive {sim.enemies.AliveCount} pulseOn {sim.loadout.pulse25} active {sim.loadout.pulseFist?.Active}");
                yaw = fr.yaw;
                pitch = fr.pitch;
                return fr;
            }
            if (!active || !(id == "s1" || id == "s2" || id == "sl" || id == "s3a" || id == "s3b" || id == "s3c" || id == "p1" || id == "p2" || id == "p3" || id == "p4" || s5Cross))
            {
                yaw = auto.yaw;
                pitch = auto.pitch;
                return auto;
            }
            Vec3 p = sim.mover.position;
            double roomDz = Dz(id), pz = p.z - roomDz; // NIGHT27: the stages move along z; the z numbers below are the NIGHT26 plan's, taken room-relative
            bool grounded = sim.mover.grounded;
            var f = new InputFrame { moveZ = 1, yaw = 0, pitch = 0 };
            f.moveX = Math.Max(-1, Math.Min(1, -p.x * 0.5)); // the hall's centre line
            if (id == "s2" || id == "sl")
            {
                if (grounded && pz <= 127.4 && pz >= 121) f.jump = Press(true, true); // the pit's lip
                else if (!grounded && pz < 127.4 && pz > 119) f.jump = Press(true, false); // held in the air: a released jump is cut short
                bool tunnelRoom = sim.campaignLevel.rooms[sim.campaign.room].lessons.Exists(ls => ls.kind == LessonKind.SlidTunnel); // NIGHT27: the tunnel is its own stage (sl); the jump stage's vent hall is not a tunnel
                if (tunnelRoom && pz <= 110.5 && pz > 94) // 4 m before the tunnel: a slide needs speed; held to its far end
                {
                    f.slide = Press(true, slideStep < 0);
                    if (slideStep < 0) slideStep = step;
                }
            }
            else if (id == "s3a" || id == "p3")
            {
                bool deck = grounded && p.y >= 3.35;
                if (deck) onDeck = true;
                if (!onDeck && !wallMode && p.z <= sim.campaignLevel.rooms[sim.campaign.room].spawn.z - 1.5) wallMode = true; // 1.5 m past the spawn: the corridor's mouth (NIGHT26: p3 is the same flue elsewhere)
                if (!onDeck && !wallMode)
                {
                    f.moveX = Math.Max(-1, Math.Min(1, (-1.0 - p.x) * 0.8)); // from the spawn to the corridor's mouth, beside the west wall
                }
                else if (wallMode && !onDeck)
                {
                    f.moveX = 0;
                    f.yaw = westWall ? 0.25 : -0.25; // the WallClimb of LevelValidate24: a lean along the wall
                    bool onWall = sim.mover.state == MoveState.WallRide;
                    rideSteps = onWall ? rideSteps + 1 : 0;
                    if (grounded) f.jump = Press(true, true);
                    else if (onWall && rideSteps * sim.step >= 0.3) f.jump = Press(true, true);
                    else f.jump = Press(true, false);
                    if (sim.mover.wallJumpsLeft < lastWallJumps || lastWallJumps == 0 && sim.mover.wallJumpsLeft < sim.mover.MaxWallJumps)
                        westWall = !westWall;
                    lastWallJumps = sim.mover.wallJumpsLeft;
                }
            }
            else if (id == "s3c" || id == "p1" || id == "p4" || s5Cross)
            {
                // NIGHT26: p1 (three 7 m gaps) and p4 (three 8 m pits under the wall) are crossed gap by gap with the same jump, dash, double jump; lipI is the gap being crossed
                double[] lips = id == "p1" ? Lips1 : id == "p4" ? Lips4 : null;
                if (lips != null)
                {
                    if (pz > lips[0] + 6) { lipI = 0; jumped = dashed = doubled = false; } // a stage respawn: from the first gap again
                    if (jumped && grounded && lipI + 1 < lips.Length && pz < lips[lipI] - 3) { lipI++; jumped = dashed = doubled = false; } // landed past the gap: the next one
                }
                double lip = lips != null ? lips[lipI] : id == "s3c" ? -36 : S5Lip; // the pit's south edge
                if (lips == null && pz > lip + 6) { jumped = dashed = doubled = false; } // back before the pit (a respawn): the script restarts
                if (grounded && !jumped && pz <= lip + 1.2 && pz > lip - 1)
                {
                    f.jump = Press(true, true);
                    jumped = true;
                    jumpStep = step;
                }
                else if (jumped && !dashed && step - jumpStep >= 26)
                {
                    f.dash = Press(true, true); // the dash in the air over the gap, near the jump's apex
                    dashed = true;
                }
                else if (dashed && !doubled && sim.mover.state != MoveState.Dash && !grounded)
                {
                    f.jump = Press(true, true); // the double jump once the dash ends, to land on the far lip
                    doubled = true;
                }
                else if (jumped && !grounded) f.jump = Press(true, false);
            }
            yaw = f.yaw;
            pitch = f.pitch;
            return f;
        }
    }
}
