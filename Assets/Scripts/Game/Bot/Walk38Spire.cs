// NIGHT38 builder C2 (NIGHT38.md "the walk bot through every room"): the walk pilot's script for n38-spire's GATE ROOMS (Design/levels/n38-spire.json:
// BELFRY I-III and THE LONG BELL, rooms with a meter38 pad; Tools/night/gen-n38-spire.cjs). Every other room of the level (THE LANDING, the arenas
// THE NAVE / THE CHOIR, THE VESTRY, and NIGHT37's v9 / v10) is the autopilot's and the existing scripts'.
//   a gate room: run north up the pad on the room's centre line with the sprint held, DASH at the lip (lipLead38 m before it), JUMP inside the dash
//   (jumpIn38 steps in: harness `spire38 gates` measured the dash 0.2 m before the lip, the jump 15-20 steps in, landing 6.75 m past the near edge)
//   and hold forward and jump to the landing; a miss in the pit climbs the recovery stair (three 1.25 m steps along the takeoff face, lx -5..4,
//   lz 10..13: the lowest east) back to the pad. The lip is read from the room's meter38 pad (its north edge), never typed.
// The hook line (the lead's, in Walk29Pilot.SampleMove, right after NIGHT36's Room36 line):
//   if (Level38Spire && Room38Spire(rd, p, cx, lz, lx, grounded, cleared, auto, out InputFrame f38c)) return Keep(f38c); // NIGHT38 C2 (Walk38Spire.cs)
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Walk29Pilot
    {
        const double SpireLipLead38 = 0.4; // m before the lip the dash is pressed (the harness's 0.2 m plus a step of sprint)
        const int SpireJumpIn38 = 15; // steps into the dash the jump is pressed (the dash lasts 30 steps at 120 Hz)

        Dictionary<string, double> spireLips38;
        string spireRoom38 = "";
        int spireDash38 = -1;
        Vec3 spireLast38;

        /// <summary>The harness's trace (spire38 walk --trace): one line per dash, jump and pit step; null: none.</summary>
        public static Action<string> SpireTrace38;

        bool Level38Spire => sim.campaignLevel != null && ((sim.campaignLevel.id ?? "") == "n38-spire" || sim.N41BaseId(sim.campaignLevel.id) == "n40-spire"); // NIGHT41: and n41-spire (N41BaseId); NIGHT40: and its n40 level (crown40)

        /// <summary>Each gate room's lip (web z): its meter38 pad's north edge.</summary>
        void SpireLips38()
        {
            spireLips38 = new Dictionary<string, double>();
            if (sim.map?.json == null || !sim.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return;
            if (!camp.TryGetValue("rooms", out object ro) || !(ro is List<object> rooms)) return;
            foreach (object r in rooms)
            {
                var room = PortData.Obj(r);
                if (!room.TryGetValue("meter38", out object mo) || !(mo is List<object> pads) || pads.Count == 0) continue;
                var q = PortData.Arr(PortData.Obj(pads[0])["rect"]);
                spireLips38[(string)room["id"]] = PortData.Num(q[1]);
            }
        }

        // ---- NIGHT37's rehearsal rooms (kept as they are in n38-spire; web coordinates from Tools/night/gen-n37-spire-boss.cjs):
        // v9 FALLING STONES: the pit's entry edge z -173, stone 1 (-13, -184), stone 2 (-7, -197) (4 x 4 m tops at 0; each drops 2 s after a landing),
        //    the exit floor from z -208; v10 THE RING: the gap's entry edge z -221, the far edge z -231 (the ring at (-10, 1.6, -226)).
        // Each a list of hops: run (or turn on the stone) at the hop's target, JUMP at the takeoff, DASH spireHopDash38 steps later (a jump + air dash:
        // harness n38 envelopes: about 11-14 m from the lip). A miss: the west recovery stair (x -22..-19: tops -3.75 / -2.5 / -1.25 at 10 / 6 / 2 m
        // past the edge) back to the entry floor, then the hops again.
        static readonly (double x, double z, double edge, int dash)[] V9Hops38 = { (-13, -184, -172.7, 24), (-5.5, -197.5, double.NaN, 1054), (-10, -210, double.NaN, 45) }; // dash < 0: a DASH-JUMP (the dash at the takeoff, the jump -dash steps in); dash >= 1000: no dash, the DOUBLE jump dash - 1000 steps in (stone 1 -> 2 spends no bar, so the last hop has a dash: the bar refills 0.5 / s after 0.4 s)
        static readonly (double x, double z, double edge, int dash)[] V10Hops38 = { (-10, -236, -220.7, 1054) }; // a jump and the double (no bar: v9's last dash leaves it low)
        int spireHop38, spireJump38 = -1, spireLand38;
        const int SpireRunUp38 = 22, SpireBrake38 = 8; // on a stone: SpireBrake38 steps of no input (the landing's carry dies on the stone), then a run at the next target, the hop at SpireRunUp38 // steps of run on a stone before its hop (about 1.2 m of run-up from the landing, well inside the 2 s drop)

        bool Rehearse38(RoomDef rd, Vec3 p, bool grounded, out InputFrame f)
        {
            var hops = rd.id == "v9" ? V9Hops38 : V10Hops38;
            double edge = rd.id == "v9" ? -173 : -221;
            if (rd.id != spireRoom38 || JsMath.Hypot(p.x - spireLast38.x, p.z - spireLast38.z) > 6) { spireRoom38 = rd.id; spireHop38 = 0; spireJump38 = -1; }
            spireLast38 = p;
            f = new InputFrame { moveZ = 1, sprint = Press(true, false) };
            if (p.y < -0.6)
            {
                // in the pit: the west stair, then back onto the entry floor; the hops start over
                double tz = p.y < -4.2 ? edge - 10 : p.y < -3.0 ? edge - 6 : p.y < -1.8 ? edge - 2 : edge + 2;
                if (spireJump38 >= 0) SpireTrace38?.Invoke($"{rd.id} hop {spireHop38} fell at ({p.x:0.0}, {p.z:0.0}) {step - spireJump38} steps after the jump");
                spireHop38 = 0;
                spireJump38 = -1;
                f.yaw = Toward(p, -20.5, tz);
                f.jump = Press(true, grounded && step % 12 == 0);
                return true;
            }
            if (spireHop38 >= hops.Length) return false; // across: the exit floor is the autopilot's
            var h = hops[spireHop38];
            f.yaw = Toward(p, h.x, h.z);
            if (spireJump38 >= 0)
            {
                int k = step - spireJump38;
                bool dj = h.dash < 0; int jAt = dj ? -h.dash : 0;
                bool dbl = h.dash >= 1000;
                f.jump = Press(k >= jAt && k < jAt + 30 || dbl && k >= h.dash - 1000 && k < h.dash - 970, dj && k == jAt || dbl && k == h.dash - 1000);
                f.dash = dj ? Press(k < 2, false) : dbl ? default : Press(k >= h.dash && k < h.dash + 2, k == h.dash);
                if (k > jAt + 8 && grounded)
                {
                    spireJump38 = -1;
                    spireLand38 = step;
                    if (JsMath.Hypot(p.x - h.x, p.z - h.z) < 3.5 || spireHop38 == hops.Length - 1) spireHop38++;
                    SpireTrace38?.Invoke($"{rd.id} landed at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), hop {spireHop38}");
                }
                return true;
            }
            // the takeoff: at the edge from the entry floor, or at once on a stone (it drops 2 s after the landing)
            if (!grounded) spireLand38 = step;
            if (double.IsNaN(h.edge) && grounded && step - spireLand38 < SpireBrake38) f.moveZ = 0;
            bool go = double.IsNaN(h.edge) ? grounded && step - spireLand38 >= SpireRunUp38 : grounded && p.z <= h.edge;
            if (go)
            {
                spireJump38 = step;
                if (h.dash < 0) f.dash = Press(true, true); else f.jump = Press(true, true);
                SpireTrace38?.Invoke($"{rd.id} hop {spireHop38} jump at ({p.x:0.0}, {p.z:0.0}) to ({h.x}, {h.z})");
            }
            return true;
        }

        /// <summary>n38-spire's gate room's own frame, or false: the autopilot's.</summary>
        bool Room38Spire(RoomDef rd, Vec3 p, double cx, double lz, double lx, bool grounded, bool cleared, InputFrame auto, out InputFrame f)
        {
            f = auto;
            if (spireLips38 == null) SpireLips38();
            if (rd != null && (rd.id == "v9" || rd.id == "v10")) return Rehearse38(rd, p, grounded, out f);
            if (rd == null || !spireLips38.TryGetValue(rd.id, out double lip)) return false;
            if (rd.id != spireRoom38 || JsMath.Hypot(p.x - spireLast38.x, p.z - spireLast38.z) > 6) { spireRoom38 = rd.id; spireDash38 = -1; } // a new room or a respawn
            spireLast38 = p;
            double north = rd.z1 - 200; // straight up the room
            if (grounded && p.z < lip - 0.5 && p.y > -0.5) { spireDash38 = -1; return false; } // landed over the gap: the landing floor is the autopilot's (it walks to the exit)
            if (p.y < -0.6 && spireDash38 < 0 || p.y < -2.5)
            {
                // in the pit: the recovery stair along the takeoff face (the steps' tops -3.75 east, -2.5 middle, -1.25 west, then the pad), a jump at each
                double tx = p.y < -4.2 ? cx + 2.5 : p.y < -3.0 ? cx - 0.5 : p.y < -1.8 ? cx - 3.5 : cx - 3.5;
                double tz = p.y < -1.8 ? lip - 1.5 : lip + 3;
                spireDash38 = -1;
                f = new InputFrame { moveZ = 1, yaw = Toward(p, tx, tz), jump = Press(true, grounded && step % 12 == 0) };
                return true;
            }
            f = new InputFrame { moveZ = 1, yaw = Toward(p, cx, north), sprint = Press(true, false) };
            double toLip = p.z - lip;
            if (spireDash38 < 0 && grounded && toLip <= SpireLipLead38) { spireDash38 = step; f.dash = Press(true, true); SpireTrace38?.Invoke($"{rd.id} dash at z {p.z:0.00} (lip {lip}), bar {sim.mover.stamina.bar:0.00}, state {sim.mover.state}"); return true; }
            if (spireDash38 >= 0)
            {
                int k = step - spireDash38;
                f.dash = Press(k < 2, false);
                if (k == SpireJumpIn38) { f.jump = Press(true, true); SpireTrace38?.Invoke($"{rd.id} jump at z {p.z:0.00} y {p.y:0.00}, state {sim.mover.state}, bar {sim.mover.stamina.bar:0.00}"); }
                else if (k > SpireJumpIn38) f.jump = Press(true, false);
            }
            return true;
        }
    }
}
