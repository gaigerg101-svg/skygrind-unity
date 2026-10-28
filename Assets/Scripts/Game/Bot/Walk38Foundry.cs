// NIGHT38 builder F (NIGHT38-BRIEF card F: "the walk bot through every room ... a Bot/Walk38Foundry.cs / Walk38Finale.cs script of your own rooms, the
// way Walk36 does it"): the walk pilot of builder F's new rooms (n38-foundry's b1 THE YARD and b2 THE GANTRY; n38-finale's rooms: Walk38Finale.cs).
// A WRAPPER round Walk29Pilot (no line in Walk29Pilot / Walk36 changes): every step the inner pilot runs (the autopilot's look, its fights, the kept rooms'
// scripts), then, in one of builder F's gate rooms, this pilot's own frame replaces the move: the ride CHAIN over a gate (b2's two gates, n38-finale's x3),
// the boost, the grapple and the dash jump (Walk38Finale.cs). The fights (b1, x2, x5) are the autopilot's. Room-relative positions as Walk29Pilot's
// (lz = metres north of the room's south edge, lx = east of its centre line); the numbers are the generators' (Tools/night/n38f-foundry-rooms.cjs,
// Tools/night/gen-n38-finale.cjs): a plan change that moves a wall changes them here. Bot numbers only (never the player's, never gameplay).
// No UnityEngine: SimHarness compiles this file (`n38f walk`, Tools/SimHarness/Harness.N38.Foundry.cs).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Walk38Pilot : IPlayerInput
    {
        readonly SurvivalSim sim;
        readonly AutopilotInput ap;
        /// <summary>The inner pilot (Walk29Pilot: the autopilot, the kept rooms' scripts); its frame is used wherever this pilot has no script.</summary>
        public readonly Walk29Pilot inner;
        double yaw, pitch;
        int step;
        string lastId = "";
        Vec3 lastPos;

        /// <summary>Counters the harness prints: gates crossed by the scripted verb (per room id), falls into a gate's pit.</summary>
        public readonly System.Collections.Generic.Dictionary<string, int> crossed = new System.Collections.Generic.Dictionary<string, int>();
        public int pitFalls, rides;
        /// <summary>NIGHT65 T1: steps on which Walk43Foundry's id script (b2 / a7 / a8) drove the move (the bot opt-out's test reads it).</summary>
        public int idScriptSteps65;
        readonly System.Collections.Generic.HashSet<string> crossedKeys = new System.Collections.Generic.HashSet<string>();

        /// <summary>One gate crossed (counted once per run per gate key: the room id, and the gate's lz in a two-gate room).</summary>
        void Crossed(string key)
        {
            if (!crossedKeys.Add(key)) return;
            string id = key.Split('@')[0];
            crossed.TryGetValue(id, out int n);
            crossed[id] = n + 1;
        }

        public Walk38Pilot(SurvivalSim sim, AutopilotInput ap)
        {
            this.sim = sim;
            this.ap = ap;
            inner = new Walk29Pilot(sim, ap);
            yaw = ap.Yaw;
            pitch = ap.Pitch;
        }

        public double Yaw => yaw;
        public double Pitch => pitch;
        public void SetLook(double y, double p) { yaw = y; pitch = p; inner.SetLook(y, p); }
        public bool Captured => inner.Captured;

        static Button Press(bool held, bool pressed) => new Button { held = held, pressed = pressed };
        static double Toward(Vec3 from, double tx, double tz) => Math.Atan2(-(tx - from.x), -(tz - from.z));

        RoomDef Room()
        {
            Campaign24 c = sim.campaign;
            if (c == null || c.room < 0 || sim.campaignLevel == null || c.room >= sim.campaignLevel.rooms.Count) return null;
            return sim.campaignLevel.rooms[c.room];
        }

        string LevelId => sim.campaignLevel != null ? sim.N41BaseId(sim.campaignLevel.id) ?? "" : ""; // NIGHT41: n41-foundry walks as n38-foundry (N41BaseId)

        InputFrame Keep(InputFrame f)
        {
            yaw = f.yaw;
            pitch = f.pitch;
            inner.SetLook(yaw, pitch);
            return f;
        }

        public InputFrame SampleMove(bool active)
        {
            InputFrame auto = inner.SampleMove(active);
            step++;
            combat38 = Combat38.None;
            chainCeil = false;
            RoomDef rd = Room();
            Vec3 p = sim.mover.position;
            if (rd == null || rd.id != lastId || JsMath.Hypot(p.x - lastPos.x, p.z - lastPos.z) > 6) { Reset38(); lastId = rd != null ? rd.id : ""; }
            lastPos = p;
            yaw = auto.yaw; pitch = auto.pitch;
            if (!active || rd == null || sim.campaign.phase == Campaign24.Phase.None) return auto;
            double cx = (rd.x0 + rd.x1) / 2, lz = rd.z1 - p.z;
            bool grounded = sim.mover.grounded;
            InputFrame f;
            // NIGHT65 T1: a room whose row says bot43: false (rebuilt under its old id) skips every id script below (Walk43's b2 / a7 / a8, n38's
            // b2 chain, the finale's): the autopilot's frame (the inner pilot's, Walk36 skipped there too)
            if (!inner.IdScripts65(rd)) { fire43 = false; return auto; }
            if (Walk43(rd, p, cx, lz, grounded, auto, out f)) { idScriptSteps65++; return Keep(f); } // NIGHT43 L1: n43-foundry's b2 / a7 / a8 (Walk43Foundry.cs; false elsewhere)
            if (LevelId == "n38-foundry")
            {
                if (rd.id == "b2")
                {
                    // the two gates: lz 8..38 and 46..76 (n38f-foundry-rooms.cjs: TAKEOFF 8, GANTRY_GAP 30, LEDGE 8; WALLS); the second gate's pilot starts on L1
                    // NIGHT39 R1: gate 2 also once grounded on L1 (lz 45..57) or once its chain began (ride39's slower chain lands on L1 short of lz 51:
                    // the pilot read "gate 1 crossed", handed the move to the autopilot and stood on L1 ~8 s)
                    double t = lz >= 51 || lz >= 45.5 && (grounded || chainT == 57) ? 57 : 8; // round 3: GANTRY_GAP 37, LEDGE 12 (gates lz 8..45 and 57..94)
                    if (t == 57 && chainT == 8 && grounded && lz >= 45.5) Crossed(rd.id + "@8"); // NIGHT39 R1: gate 1 landed on L1 short of lz 51
                    chainCeil = true;
                    bool n42 = pilot42 = sim.campaignLevel.id == "n42-foundry"; // NIGHT42 L1: n42-foundry's GANTRY (Tools/night/gen-n42-foundry.cjs): planes +-3, W2 from 8 m
                    if (Chain(rd, p, cx, lz, grounded, t, 37, n42 ? GantryWalls42 : GantryWalls, n42 ? 3 : 4, out f)) return Keep(f);
                }
            }
            else if (LevelId == "n38-finale" && Finale38(rd, p, cx, lz, grounded, auto, out f)) return Keep(f);
            return auto;
        }

        enum Combat38 { None, Boost }
        Combat38 combat38;
        int boostAt = -1;

        public CombatInput SampleCombat(bool active)
        {
            CombatInput c = inner.SampleCombat(active);
            if (active && Combat43(ref c)) return c; // NIGHT43 L1: a8's shots from the wall (Walk43Foundry.cs)
            if (!active || combat38 == Combat38.None) return c;
            int g = sim.loadout.owned.IndexOf("shotgun");
            c.weapon = g >= 0 && sim.loadout.Weapon != "shotgun" ? g : -1;
            c.punch = default;
            c.altFire = default;
            bool fire = step == boostAt;
            c.fire = Press(fire, fire);
            return c;
        }

        // ---- the per-room script state (reset on a room change and on a respawn)
        int wall, released = -1;
        double chainT = double.NaN;
        bool onWall, jumpedLip;

        void Reset38()
        {
            wall = 0; released = -1;
            onWall = jumpedLip = false;
            boostAt = -1;
            ResetFinale38();
        }

        /// <summary>The bot's ride-chain numbers: the release this many metres before a wall's end (the hop then lands inside the next wall), the
        /// re-press this many steps after the release (the next ride starts on the press, held), the yaw into the next wall (rad).</summary>
        double ReleaseBeforeEnd => chainCeil ? (Probe39Rel ?? 3.0) : 6.0;
        double LastReleaseBeforeEnd => Probe39Last ?? 2.5;
        const double LeanRad = 0.35;
        int RepressSteps => chainCeil ? (Probe39Repress ?? 40) : 14; // NIGHT39 R1: 30 -> 40 (ride39's 9.2 m/s ride: a re-press 0.25 s after the release caught gate 2's W2 1.8 m low, under the far floor's lip, 0/5; 0.33 s: 5/5 with ride39 on and off)
        /// <summary>NIGHT39 R1 probe (the harness's `n39ride walk --rel / --last / --pitch / --repress`; null: the numbers above).</summary>
        public static double? Probe39Rel, Probe39Last, Probe39Pitch;
        public static int? Probe39Repress;
        /// <summary>Round 3: THE GANTRY's gates have a ceiling (underside 3.6 m): a release 3 m before the wall's end looking up 30 deg, the re-press 0.25 s
        /// later (E's best try: "ride chain L wall-end 3.0m p30"); the old numbers (6 m, level, 0.12 s) bonked under it and caught the next wall 3 m low.</summary>
        bool chainCeil;
        int hopStep = -1000;
        const double CeilReleasePitch = 0.52;

        /// <summary>THE GANTRY's walls per gate (n38f-foundry-rooms.cjs WALLS at GANTRY_GAP 30): metres past the lip, west first, faces alternating.</summary>
        static readonly double[][] GantryWalls = { new[] { -6.0, 26 }, new[] { 20.0, 36.5 } }; // round 3: GANTRY_GAP 37
        /// <summary>NIGHT42 L1: n42-foundry's walls as the bot aims at them (gen-n42-foundry.cjs: W1 -6..22, W2 8..36.5, W3 26..36.5 at planes +-3): W2's
        /// start is read as 18 m (the bot's hop off W1 at 19 m aims ahead, never back at W2's real start), W3 (the person's net) left out.</summary>
        static readonly double[][] GantryWalls42 = { new[] { -6.0, 22 }, new[] { 18.0, 36.5 } };
        /// <summary>NIGHT42 L1: this run is on n42-foundry (its GANTRY numbers; a hop out of the pit stays in the pit script until the bot is grounded out of it).</summary>
        bool pilot42;
        /// <summary>n38-finale x3's walls (gen-n38-finale.cjs RIDE_WALLS at RIDE_GAP 30).</summary>
        static readonly double[][] RideWalls30 = { new[] { -6.0, 19 }, new[] { 13.0, 29.5 } };

        /// <summary>The ride chain over one gate: the run-up along the first wall's line, the jump at the lip held into the first ride, then per wall the
        /// release (the jump-off) and the re-press held into the next, alternating west / east; out of the pit by its steps when a hop falls short.
        /// t: the gate's south edge (lz), gap: its length, walls: [from, to] metres past the lip (west first, alternating), plane: the walls' planes (lx +-plane).
        /// False: past the gate (the autopilot / the next script takes over).</summary>
        bool Chain(RoomDef rd, Vec3 p, double cx, double lz, bool grounded, double t, double gap, double[][] walls, double plane, out InputFrame f)
        {
            f = new InputFrame { moveZ = 1 };
            int per = walls.Length;
            // round 2 (seed 2 flew from gate 1's last wall straight onto gate 2's first with the wall count still past gate 1's: it rode W1 round its north
            // end, the corner rule, onto its back face and off into the room's wall): a new gate starts its own count
            if (t != chainT) { chainT = t; wall = 0; onWall = false; released = -1; }
            if (lz >= t + gap + 0.5 && grounded) // across
            {
                Crossed(rd.id + "@" + t);
                return false;
            }
            if (p.y < -0.3 && lz > t - 0.5 && (grounded || p.y < -1.0) || pilot42 && wall < 0 && !grounded && lz > t - 0.5) // in the pit or on its steps (round 2: the top step, -0.4 m, read as out of it and the bot ran back in): out by the steps, then the chain again
            {
                if (wall >= 0) { pitFalls++; wall = -1; }
                // round 1 (seed 1 hopped 600 s against step 3's east face): first into the steps' column north of the steps, then south up them.
                // round 2: the steps are on the EAST wall (W1 stands in the west half from the takeoff floor); west of W1, first round its north end
                double lx = p.x - cx, w1End = t + walls[0][1];
                bool inColumn = lx > 7.9;
                if (lx < -plane - 0.3 && lz < w1End + 1) f.yaw = Toward(p, p.x, rd.z1 - (w1End + 2));
                else f.yaw = inColumn ? Toward(p, cx + 8.9, rd.z1 - (t - 2)) : Toward(p, cx + 8.9, rd.z1 - (t + 5.5));
                bool hop = inColumn && grounded && step % 12 == 0;
                if (hop) hopStep = step;
                // held through the hop's rise (a released jump is cut to half: 0.8 m under a 1.2 m step), never between hops
                f.jump = chainCeil ? Press(step - hopStep < 30, hop) : Press(true, hop); // round 3 (THE GANTRY): not held between hops (held, it rode the room's east wall above the steps)
                return true;
            }
            if (lz < t - 0.2 && grounded) // the run-up on the floor before the lip: along the first wall's line (lx -plane + 1.3)
            {
                wall = 0; onWall = false; released = -1; jumpedLip = false;
                double tx = cx - plane + (pilot42 ? 0.9 : 1.3); // NIGHT42 L1: 0.9 m off W1 on n42-foundry (from L1 the bot ran straight at 1.3 m and never touched W1 at planes +-3)
                // round 1 (seed 1 landed on L1 at lx -9.5 and ran at the first wall's BACK face): onto the line first, then the run
                if (lz < t - 3.5 || Math.Abs(p.x - tx) > 0.8 && lz < t - 1.2) { f.yaw = Toward(p, tx, rd.z1 - Math.Max(lz + 0.5, t - 3)); return true; }
                f.yaw = Toward(p, tx, rd.z1 - (t + 6));
                if (lz >= t - 0.9) { f.jump = Press(true, true); jumpedLip = true; }
                return true;
            }
            if (wall < 0) wall = 0;
            bool west = wall % 2 == 0;
            double we = t + walls[Math.Min(wall, per - 1)][1];
            bool riding = sim.mover.state == MoveState.WallRide;
            if (riding && !onWall) { onWall = true; rides++; }
            if (onWall && !riding) // the ride ended (our release, or the wall's end): on to the next wall
            {
                onWall = false;
                if (released < 0) released = step; // the wall let go by itself: re-press as after a release
                wall = Math.Min(wall + 1, per);
                return Air(p, cx, lz, t, gap, walls, plane, ref f);
            }
            if (riding)
            {
                // along the wall, leaning into it (a held ride); the release before its end, looking at the next wall
                f.yaw = west ? LeanRad * 0.4 : -LeanRad * 0.4;
                f.moveX = west ? -0.4 : 0.4;
                bool last = wall >= per - 1;
                if (lz >= we - (last ? LastReleaseBeforeEnd : ReleaseBeforeEnd))
                {
                    f.jump = Press(false, false); // the release: the jump-off
                    released = step;
                    if (chainCeil) f.pitch = Probe39Pitch ?? CeilReleasePitch;
                    double nx = last ? 0 : (west ? plane : -plane), nz = last ? t + gap + 3 : t + walls[wall + 1][0] + 4;
                    f.yaw = Toward(p, cx + nx, rd.z1 - nz);
                    return true;
                }
                f.jump = Press(true, false);
                return true;
            }
            return Air(p, cx, lz, t, gap, walls, plane, ref f);
        }

        /// <summary>In the air between walls: toward the target wall, the jump re-pressed RepressSteps after the release and held (the next ride).</summary>
        bool Air(Vec3 p, double cx, double lz, double t, double gap, double[][] walls, double plane, ref InputFrame f)
        {
            int per = walls.Length;
            if (wall >= per) // past the last wall: to the far floor
            {
                f.yaw = Toward(p, cx, sim.campaignLevel.rooms[sim.campaign.room].z1 - (t + gap + 4));
                // the double jump carries the hop onto the floor; the jump is held only round that press (round 2: held all the way it caught every wall near
                // the landing and the bot bounced between them for 600 s)
                int s2 = released >= 0 ? step - released : 0;
                f.jump = Press(s2 >= RepressSteps && s2 < RepressSteps + 6, s2 == RepressSteps);
                // over the landing: let go of the stick and come down on it (round 2: with the doors free the bot flew the 6 m landing into the next room,
                // the room's `reached` lesson never saw it grounded there)
                if (lz > t + gap + 1) { f.moveZ = 0; f.moveX = 0; f.jump = default; }
                return true;
            }
            bool west = wall % 2 == 0;
            double tz = t + Math.Max(walls[wall][0], 0) + (walls[wall][1] - Math.Max(walls[wall][0], 0)) * 0.6;
            RoomDef rd = sim.campaignLevel.rooms[sim.campaign.room];
            f.yaw = Toward(p, cx + (west ? -plane - 1 : plane + 1), rd.z1 - tz);
            f.moveX = 0;
            if (released < 0) { f.jump = Press(true, false); return true; } // from the lip: still held from the jump
            int since = step - released;
            if (since < RepressSteps) { f.jump = Press(false, false); return true; }
            f.jump = Press(true, since == RepressSteps);
            return true;
        }
    }
}
