// NIGHT38 builder F: the walk pilot's scripts for n38-finale's four gates (Tools/night/gen-n38-finale.cjs; Design/levels/n38/finale.beats.md). The arenas
// (x2 THE FLOOR, x5 THE SPINE) and the links (x1, x8) are the autopilot's (Walk38Foundry.cs's inner Walk29Pilot); the boss room x9 is not played.
//   x3 THE RIDE   the ride chain (Walk38Foundry.cs Chain): gate lz 8..38 (RIDE_GAP 30), the gantry's two walls, planes +-4 m.
//   x4 THE STEP   the boost: the THUNDERCLAP in hand, a run at the step (lz 12, STEP_H 3.6 m), the jump, the shot fired straight down two steps later
//                 (Combat.cs: airborne, velocity -= aim x weapons.shotgun.jumpSpeed), on over the lip.
//   x6 THE VOID   the grapple: Span30Pilot's crossing (the room's rings in a line along the travel), the inner pilot's own.
//   x7 THE LEAP   the dash jump: a run along the centre line, the jump at the lip (lz 6), the dash DashAfterSteps later, the jump held.
// Bot numbers only. No UnityEngine.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Walk38Pilot
    {
        const int BoostAfterSteps = 2, DashAfterSteps = 54, DoubleAfterSteps = 84;
        /// <summary>THE LEAP's gap (gen-n38-finale.cjs LEAP_GAP).</summary>
        const double LeapGap = 13.5;
        bool doubled38;
        const double StepLz = 12, StepRunFrom = 3.2, LeapLz = 8;
        int jumpStep38 = -1;
        bool dashed38;

        void ResetFinale38() { jumpStep38 = -1; dashed38 = doubled38 = false; }

        bool Finale38(RoomDef rd, Vec3 p, double cx, double lz, bool grounded, InputFrame auto, out InputFrame f)
        {
            f = auto;
            switch (rd.id)
            {
                case "x3": // gen-n38-finale.cjs: T 8, RIDE_GAP 30, RIDE_WALLS (the gantry's)
                    return Chain(rd, p, cx, lz, grounded, 8, 30, RideWalls30, 4, out f);
                case "x4":
                    return Boost(rd, p, cx, lz, grounded, out f);
                case "x6":
                    return inner.span30.Drive(rd, auto, out f);
                case "x7":
                    return Leap(rd, p, cx, lz, grounded, out f);
            }
            return false;
        }

        bool Boost(RoomDef rd, Vec3 p, double cx, double lz, bool grounded, out InputFrame f)
        {
            f = new InputFrame { moveZ = 1, yaw = Toward(p, cx, rd.z1 - (StepLz + 4)) };
            if (p.y > 3.0 && grounded) // on the step: done
            {
                Crossed(rd.id);
                return false;
            }
            if (lz >= StepLz + 0.5 && p.y < 1) return false; // past the step on the north floor: the autopilot to the door
            combat38 = Combat38.Boost; // the THUNDERCLAP in hand from the room's entry
            if (grounded && p.y < 1)
            {
                if (jumpStep38 >= 0 && step - jumpStep38 > 30) jumpStep38 = -1; // landed short: again
                if (lz < StepLz - StepRunFrom - 1.5) return true; // the run-up
                bool ready = sim.loadout.Weapon == "shotgun";
                if (!ready) { f.moveZ = 0; return true; }
                if (jumpStep38 < 0 && lz >= StepLz - StepRunFrom) { f.jump = Press(true, true); jumpStep38 = step; boostAt = step + BoostAfterSteps + 1; }
                return true;
            }
            f.jump = Press(true, false);
            if (jumpStep38 >= 0 && step <= boostAt) f.pitch = -1.45; // the shot's aim: straight down
            return true;
        }

        bool Leap(RoomDef rd, Vec3 p, double cx, double lz, bool grounded, out InputFrame f)
        {
            f = new InputFrame { moveZ = 1, yaw = Toward(p, cx, rd.z1 - (LeapLz + 30)) };
            if (lz >= LeapLz + LeapGap + 0.5 && grounded && p.y > -0.5)
            {
                Crossed(rd.id);
                return false;
            }
            if (p.y < -0.3 && lz > LeapLz - 0.5 && (grounded || p.y < -1.0)) // in the pit (or on its steps): out by the steps up the west wall at its south end
            {
                if (jumpStep38 >= 0) { pitFalls++; jumpStep38 = -1; dashed38 = false; }
                // round 2 (seeds 1-2 hopped 600 s against the top step's face from the pit floor): to the steps' column north of them first, then south up them
                bool onSteps = p.y > -2.0, inColumn = p.x - cx < -5.2;
                if (!onSteps && !(inColumn && lz > LeapLz + 2.6 && lz < LeapLz + 4.5)) f.yaw = Toward(p, cx - 6, rd.z1 - (LeapLz + 3.6));
                else f.yaw = Toward(p, cx - 6, rd.z1 - (LeapLz - 2));
                f.jump = Press(true, (onSteps || inColumn) && grounded && step % 12 == 0);
                return true;
            }
            if (grounded && lz < LeapLz)
            {
                jumpStep38 = -1; dashed38 = false; doubled38 = false;
                f.sprint = new Button { held = true }; // the run-up sprinted, as E's tries (envelopes38: "the run-up sprint held")
                if (lz >= LeapLz - 0.7) { f.jump = Press(true, true); jumpStep38 = step; }
                return true;
            }
            // E's best dash try under the 3.6 m roof (x7 --verbose): the jump, the dash 0.45 s in, then the double jump
            f.jump = Press(true, false);
            if (jumpStep38 >= 0 && !dashed38 && step - jumpStep38 >= DashAfterSteps) { f.dash = Press(true, true); dashed38 = true; }
            else if (dashed38 && !doubled38 && step - jumpStep38 >= DoubleAfterSteps) { f.jump = Press(true, true); doubled38 = true; }
            return true;
        }
    }
}
