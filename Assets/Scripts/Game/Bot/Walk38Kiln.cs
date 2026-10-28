// NIGHT38 builder K3, rewritten by NIGHT39 builder K4 (NIGHT39.md card K4): the walk pilot's scripts for the rebuilt level 3 (Design/levels/n38-kiln.json;
// Design/levels/n38/kiln.beats.md; Tools/night/gen-n38-kiln.cjs). The autopilot fights and walks the fight rooms (kb1, kb2, kb3) with NIGHT36's lava rule
// (out by the nearer rim, never walk in: Walk36.cs's Lava36 / InLava36, read from the level's own lava pits). Every FLIGHT room (kb1b, kb2b, kb3b) holds
// one or two CLIMB gates (a noride cliff 9.5 m over its take-off), read from the level file itself (the campaign boxes tagged `gate`: the take-off point
// `from`, the landing point `to`, the cliff's top). The pilot runs up the axis to the take-off and makes THE CLIMB (builder E's checker's best try,
// `boost x3 p-89 @0.05+double@apex`): jump, the THUNDERCLAP shot straight down 0.05 s after the take-off, the double jump at the apex, then a shot at every
// chance the gun gives (its own interval) until the feet are over the top, then the stick carries it onto the cliff. In a room with two climbs (THE STACK)
// the gate is the one whose take-off level the feet stand on. A fall into THE SPOUT's pit: the recovery stair in the west notch, then the run again.
// Bot-only numbers (the pilot's), not gameplay.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Walk29Pilot
    {
        bool Level38K => sim.campaignLevel != null && (sim.campaignLevel.id == "n38-kiln" || sim.N41BaseId(sim.campaignLevel.id) == "n40-kiln"); // NIGHT40: and its n40 level (kiln40); NIGHT41: and n41-kiln (N41BaseId)

        /// <summary>One gate of the level file: its room, the take-off point (feet), the landing point, the cliff's top.</summary>
        sealed class Gate38K
        {
            public string room;
            public Vec3 from, to;
            public double top;
        }

        List<Gate38K> gates38K;
        bool boost38K, fired38K, doubled38K;
        int jump38KStep = -1, gate38KIndex = -1;

        /// <summary>The level's gates (campaign.boxes rows with a `gate` object), read once, in file order (a room's lower climb first).</summary>
        List<Gate38K> Gates38K()
        {
            if (gates38K != null) return gates38K;
            gates38K = new List<Gate38K>();
            if (sim.map.json != null && sim.map.json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp
                && camp.TryGetValue("boxes", out object bo) && bo is List<object> boxes)
                foreach (object o in boxes)
                    if (o is Dictionary<string, object> b && b.TryGetValue("gate", out object go) && go is Dictionary<string, object> g)
                    {
                        var max = PortData.Arr(b["max"]);
                        gates38K.Add(new Gate38K { room = b.TryGetValue("zone", out object z) ? z as string : "", from = PortData.V3(g["from"]), to = PortData.V3(g["to"]), top = PortData.Num(max[1]) });
                    }
            return gates38K;
        }

        void Reset38K()
        {
            boost38K = fired38K = doubled38K = false;
            jump38KStep = -1;
            gate38KIndex = -1;
        }

        /// <summary>The n38-kiln room's own frame, or false: the autopilot's.</summary>
        bool Room38K(RoomDef rd, Vec3 p, double cx, double lz, bool grounded, InputFrame auto, out InputFrame f)
        {
            f = new InputFrame { moveZ = 1 };
            boost38K = false;
            // the pours (NIGHT36's lava rule, the level's own pits): out by the nearer rim; at a rim, stand and fight
            if (Lava36(p, grounded, out double ex, out double ez))
            {
                f.yaw = Toward(p, ex, ez);
                f.jump = Press(true, grounded && step % 12 == 0);
                return true;
            }
            if (grounded)
            {
                Vec3 w = MovementMath.WishDirection(auto.moveX, auto.moveZ, auto.yaw);
                if (InLava36(p.x + w.x * LavaLook36, p.z + w.z * LavaLook36))
                {
                    f = auto;
                    f.moveX = f.moveZ = 0;
                    f.jump = default;
                    // round 2: a CLEARED room's route to the door runs over the pour (the nav takes a lava pit's floor for floor: CASTING FLOOR's
                    // pour on the axis, 4 of 10 seeds stood at its south edge for 900 s): round it by its nearer east / west side, 1.5 m out
                    if (sim.campaign != null && sim.campaign.phase == Campaign24.Phase.Cleared)
                        foreach (double[] lr in Lava36Rects())
                        {
                            double ax = p.x + w.x * LavaLook36, az = p.z + w.z * LavaLook36;
                            if (!(ax > lr[0] && ax < lr[2] && az > lr[1] && az < lr[3])) continue;
                            double sx = p.x - lr[0] < lr[2] - p.x ? lr[0] - 1.5 : lr[2] + 1.5;
                            f.yaw = Toward(p, sx, p.z);
                            f.moveZ = 1;
                            break;
                        }
                    return true;
                }
            }
            // a cleared room near its door: through it (the autopilot stops ~2.4 m short of the next room's spawn), to a point 3 m past the next spawn
            if (sim.campaign != null && sim.campaign.phase == Campaign24.Phase.Cleared && sim.campaign.room + 1 < sim.campaignLevel.rooms.Count)
            {
                RoomDef nx = sim.campaignLevel.rooms[sim.campaign.room + 1];
                if (JsMath.Hypot(p.x - nx.spawn.x, p.z - nx.spawn.z) < 6)
                {
                    f.yaw = Toward(p, nx.spawn.x, nx.spawn.z - 3);
                    return true;
                }
            }
            List<Gate38K> all = Gates38K();
            List<Gate38K> mine = all.FindAll(q => q.room == rd.id);
            if (mine.Count == 0) return false;
            // the room's last cliff behind us (past its landing, or on its top): straight to the exit / the next room
            Gate38K last = mine[mine.Count - 1];
            double lastFarLz = rd.z1 - last.to.z;
            if ((lz > lastFarLz + 1 && p.y > -0.5) || (grounded && p.y > last.top - 0.6))
            {
                Reset38K();
                int ri = sim.campaign != null ? sim.campaign.room : -1;
                bool clearedHere = sim.campaign != null && sim.campaign.phase == Campaign24.Phase.Cleared;
                Vec3 goal = clearedHere && ri >= 0 && ri + 1 < sim.campaignLevel.rooms.Count ? sim.campaignLevel.rooms[ri + 1].spawn : rd.exit;
                f.yaw = Toward(p, goal.x, goal.z);
                return true;
            }
            // in a pit (THE SPOUT): the recovery stair in the west notch (rd.x0 + 1.6), hopping up its 1.3 m steps, then back south
            Gate38K first = mine[0];
            double lipLz = rd.z1 - first.from.z + 0.5;
            if (p.y < -0.5)
            {
                Reset38K();
                double sx = rd.x0 + 1.6;
                bool atNotch = Math.Abs(p.x - sx) < 1.2;
                f.yaw = atNotch ? Toward(p, sx, rd.z1 - (lipLz - 4)) : Toward(p, sx, rd.z1 - (lipLz + 0.5));
                f.jump = Press(true, grounded && step % 18 == 0);
                return true;
            }
            // the gate to climb now: the one whose take-off level the feet are on (the higher of two when on tier 1)
            Gate38K g = mine[0];
            foreach (Gate38K q in mine) if (p.y > q.from.y - 0.6) g = q;
            int gi = all.IndexOf(g);
            // airborne after the take-off: THE CLIMB (the shots are Combat38K's)
            if (jump38KStep >= 0 && !grounded)
            {

                int t = step - jump38KStep;
                boost38K = true;
                f.yaw = 0; // facing north, the way the rooms run
                f.pitch = -89 * Math.PI / 180; // straight down the whole flight (a shot at pitch 0 would push back off the top: trace, round 1)
                f.moveZ = 1;
                bool dj = !doubled38K && t > 8 && sim.mover.velocity.y <= 0;
                f.jump = Press(true, dj);
                if (dj) doubled38K = true;
                return true;
            }
            if (jump38KStep >= 0 && grounded && step - jump38KStep > 10) Reset38K(); // landed short (on the floor or tier 1): run again
            // the run-up: north up the axis to the take-off point, the jump there (a take-off past it, at a cliff's foot, is a standing climb)
            boost38K = true; // the THUNDERCLAP in hand during the run-up (the swap takes its steps)
            f.yaw = 0;
            f.moveZ = 1;
            f.moveX = 0;
            double axisErr = p.x - g.from.x;
            if (Math.Abs(axisErr) > 0.3) f.moveX = -Math.Sign(axisErr) * 0.5; // onto the axis (moveX is +right of the facing)
            double fromLz = rd.z1 - g.from.z;
            if (lz >= fromLz - 0.1 && grounded && sim.loadout.Weapon == "shotgun")
            {
                f.jump = Press(true, true);
                jump38KStep = step;
                gate38KIndex = gi;
                fired38K = doubled38K = false;
            }
            else if (lz >= fromLz - 0.1 && grounded) f.moveZ = 0; // wait at the take-off for the THUNDERCLAP to be in hand
            return true;
        }

        /// <summary>The climb's trigger: the THUNDERCLAP in hand, a tap 0.05 s after the take-off, then a tap on every other step after the double jump
        /// (the gun fires at its own interval) until the feet are over the cliff's top.</summary>
        void Combat38K(ref CombatInput c)
        {
            int g = sim.loadout.owned.IndexOf("shotgun"); // the THUNDERCLAP's loadout id
            c.weapon = g >= 0 && sim.loadout.Weapon != "shotgun" ? g : -1;
            c.punch = default;
            c.altFire = default;
            c.fire = default;
            if (jump38KStep < 0 || sim.mover.grounded) return;
            int t = step - jump38KStep;
            double top = gate38KIndex >= 0 && gate38KIndex < Gates38K().Count ? Gates38K()[gate38KIndex].top : double.PositiveInfinity;
            bool shot = !fired38K && t >= 6; // 0.05 s
            if (shot) fired38K = true;
            // then (builder E's checker's best try, boost x3 p-89 @0.05 + double@apex) a fresh trigger pull on every other step after the double
            // jump while the feet are under the top + 1 m: the gun fires at its own interval
            bool more = fired38K && doubled38K && sim.mover.position.y < top + 1 && step % 2 == 0;
            bool press = shot || more;
            c.fire = Press(press, press);
        }
    }
}
