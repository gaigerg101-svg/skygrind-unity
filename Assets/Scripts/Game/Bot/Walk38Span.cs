// NIGHT38 builder O2 (NIGHT38.md L4): the walk pilot's script for level 4 rebuilt as n38-span (Design/levels/n38-span.json, rooms p1-p5;
// Design/levels/n38/span.beats.md; Tools/night/gen-n38-span.cjs). The arenas (p1 STITCH YARD, p3 KNOT YARD, p5 SPOOL HALL) are the autopilot's
// (it fights, then walks to the next room). The flight rooms (p2 FIRST GAP, p4 PULL LINE) are scripted from the room row's `pull38` (the level's
// own numbers: each gate's lip, the ring over its landing and the landing point): walk to the lip, run off it with a jump, look up and PULL to the
// ring (a charge in hand first), drift onto the landing 10 m up, the next gate. A fall (the room's respawnY) puts the player back at the room's
// spawn: the script starts over from the first gate north of the feet.
// Pure input through the sim's public state (headless `n38span walk` and a Player). Numbers here are the bot's own (one scripted player), no rule.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Walk29Pilot
    {
        /// <summary>One gate of a flight room (world points from the room row's pull38.gates).</summary>
        sealed class Gate38
        {
            public Vec3 lip, ring, land;
        }

        bool Level38Span => sim.campaignLevel != null && ((sim.campaignLevel.id ?? "") == "n38-span" || N45Levels.BaseId(sim.cfg, sim.campaignLevel.id) == "n40-span"); // NIGHT40: and its n40 level (loom40); NIGHT46: and n46-span (NightRows.BaseId)

        Dictionary<string, List<Gate38>> gates38;
        string room38 = "";
        int gate38, phase38, phaseSteps38;
        bool sawRope38;
        Vec3 last38;

        /// <summary>Counters (the harness's line): pulls pressed, ropes seen, gates crossed, falls back to a room's spawn.</summary>
        public int pulls38, ropes38, gatesCrossed38, restarts38;
        /// <summary>The flight script's state (the harness's --trace).</summary>
        public string State38 => $"{room38} step {gate38} phase {phase38}";

        /// <summary>The flight rooms' gates from the level file (campaign.rooms[].pull38.gates), read once.</summary>
        Dictionary<string, List<Gate38>> Gates38()
        {
            if (gates38 != null) return gates38;
            gates38 = new Dictionary<string, List<Gate38>>();
            if (sim.map.json == null || !sim.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return gates38;
            foreach (object ro in PortData.Arr(camp["rooms"]))
            {
                var r = PortData.Obj(ro);
                if (!r.TryGetValue("pull38", out object po) || !(po is Dictionary<string, object> p) || !p.TryGetValue("gates", out object go)) continue;
                var list = new List<Gate38>();
                foreach (object g in PortData.Arr(go))
                {
                    var d = PortData.Obj(g);
                    list.Add(new Gate38 { lip = PortData.V3(d["lip"]), ring = PortData.V3(d["ring"]), land = PortData.V3(d["land"]) });
                }
                gates38[(string)r["id"]] = list;
            }
            return gates38;
        }

        /// <summary>n38-span's room frame, or false (the span pilot / the autopilot).</summary>
        bool Room38Span(RoomDef rd, Vec3 p, bool grounded, bool cleared, InputFrame auto, out InputFrame f)
        {
            f = auto;
            if (rd.id == "p1" || rd.id == "p3" || rd.id == "p5") return true; // the arenas: the autopilot's fight and walk (no span pilot pulls mid-fight)
            if (rd.id == "o1" || rd.id == "o2") return Hops38(rd, p, grounded, auto, out f); // NIGHT37's kept rehearsal rooms: a scripted hop list
            if (!Gates38().TryGetValue(rd.id, out List<Gate38> gates) || gates.Count == 0) return false;
            if (rd.id != room38 || JsMath.Hypot(p.x - last38.x, p.z - last38.z) > 6) // a new room, or a fall back to its spawn
            {
                if (rd.id == room38) restarts38++;
                room38 = rd.id;
                gate38 = 0;
                while (gate38 < gates.Count && p.z < gates[gate38].land.z + 0.5) gate38++; // the first gate still ahead (travel toward -z)
                phase38 = phaseSteps38 = 0;
            }
            last38 = p;
            phaseSteps38++;
            f = new InputFrame { moveZ = 0, yaw = 0 };
            if (gate38 >= gates.Count) // past the last gate: straight off the landing's north end to the exit (the autopilot's line ran off the pier's side), then the autopilot
            {
                if (p.z < rd.exit.z - 1) { f = auto; return true; }
                f.yaw = Toward(p, rd.exit.x, rd.exit.z - 3);
                f.moveZ = 1;
                return true;
            }
            Gate38 g = gates[gate38];
            Vec3 eye = sim.Eye;
            Mover m = sim.mover;
            bool ready = m.rope.phase == RopePhase.None && (!sim.Grapple32On || sim.grappleCharges32 > 0);
            switch (phase38)
            {
                case 0: // walk to 2 m short of the lip, on its line
                {
                    var a = new Vec3(g.lip.x, p.y, g.lip.z + 2);
                    if (JsMath.Hypot(p.x - a.x, p.z - a.z) < 0.7 && grounded) { Next38(1); break; }
                    f.yaw = Toward(p, a.x, a.z);
                    f.moveZ = 1;
                    break;
                }
                case 1: // a charge in hand (the 4 s recharge after a pull), then run off the lip with a jump
                    f.yaw = 0;
                    if (sim.Grapple32On && sim.grappleCharges32 < 1 && phaseSteps38 < 120 * 10) break;
                    f.moveZ = 1;
                    if (grounded && p.z <= g.lip.z + 0.3) { f.jump = Press(true, true); Next38(2); }
                    break;
                case 2: // in the air: look up at the ring over the landing and pull
                    (f.yaw, f.pitch) = Loom37Pilot.LookAt(eye, g.ring);
                    f.jump = Press(true, false);
                    if (phaseSteps38 < 8) break;
                    if (ready && step % 4 == 0) { f.grapple = Press(true, true); pulls38++; sawRope38 = false; Next38(3); }
                    break;
                case 3: // hold through the pull; at its arrival, land
                    (f.yaw, f.pitch) = Loom37Pilot.LookAt(eye, g.ring);
                    f.grapple = Press(true, false);
                    if (m.rope.phase != RopePhase.None && !sawRope38) { sawRope38 = true; ropes38++; }
                    if (phaseSteps38 > 6 && m.rope.phase == RopePhase.None) Next38(sawRope38 ? 4 : 2); // the hook never went out: press again
                    else if (phaseSteps38 > 240) Next38(2);
                    break;
                case 4: // drift onto the landing, then the next gate
                    f.yaw = Toward(p, g.land.x, g.land.z);
                    f.moveZ = 1;
                    if (grounded && p.y > g.land.y - 0.5 && phaseSteps38 > 4) { gatesCrossed38++; gate38++; Next38(0); }
                    else if (phaseSteps38 > 360) Next38(0);
                    break;
            }
            return true;
        }

        /// <summary>One hop of a kept rehearsal room: walk to `stand` (room-local lx, y, lz; null: pressed in the air right after the last pull's
        /// arrival), then pull at `aim` (a grapple node, or a point on a surface: grapple35 pulls to what the hook hits).</summary>
        sealed class Hop38
        {
            public double[] stand, aim;
        }

        // NIGHT37's o1 / o2 (Tools/night/gen-n37-span-boss.cjs; the numbers are the level file's, room-local): o1 THREAD LINE: thread 1 from the ledge,
        // then thread 2 in the air at its arrival (34.6 m, over the tower: a pull at the tower's top was taken by thread 2's aim cone); o2 SWING ANCHOR: the post's
        // thread from the ledge (the swing anchor is not needed to cross), then the far thread from the post's north end.
        static readonly Dictionary<string, Hop38[]> Hops38Rooms = new Dictionary<string, Hop38[]>
        {
            ["o1"] = new[] { new Hop38 { stand = new[] { 0.0, 0, 5 }, aim = new[] { 0.0, 9, 15 } }, new Hop38 { aim = new[] { -1.0, 9, 47 } } },
            ["o2"] = new[] { new Hop38 { stand = new[] { 0.0, 0, 5 }, aim = new[] { 0.0, 8, 27 } }, new Hop38 { stand = new[] { 0.0, 4, 30 }, aim = new[] { -2.0, 9, 42 } } },
        };

        bool Hops38(RoomDef rd, Vec3 p, bool grounded, InputFrame auto, out InputFrame f)
        {
            f = new InputFrame { moveZ = 0, yaw = 0 };
            Hop38[] hops = Hops38Rooms[rd.id];
            double cx = (rd.x0 + rd.x1) / 2;
            Vec3 W(double[] q) => new Vec3(cx + q[0], q[1], rd.z1 - q[2]);
            if (rd.id != room38 || JsMath.Hypot(p.x - last38.x, p.z - last38.z) > 6) // a new room, or a fall back to its spawn: from the first hop
            {
                if (rd.id == room38) restarts38++;
                room38 = rd.id;
                gate38 = 0;
                phase38 = phaseSteps38 = 0;
            }
            last38 = p;
            phaseSteps38++;
            if (gate38 >= hops.Length) // across: to the exit, then the autopilot
            {
                if (p.z < rd.exit.z - 1 && grounded) { f = auto; return true; }
                f.yaw = Toward(p, rd.exit.x, rd.exit.z - 3);
                f.moveZ = 1;
                return true;
            }
            Hop38 h = hops[gate38];
            Mover m = sim.mover;
            bool ready = m.rope.phase == RopePhase.None && (!sim.Grapple32On || sim.grappleCharges32 > 0);
            Vec3 aim = W(h.aim);
            switch (phase38)
            {
                case 0: // to the stand (an in-air hop presses at once)
                    if (h.stand == null) { Next38(1); break; }
                    {
                        Vec3 s = W(h.stand);
                        if (JsMath.Hypot(p.x - s.x, p.z - s.z) < 0.7 && grounded) { Next38(1); break; }
                        f.yaw = Toward(p, s.x, s.z);
                        f.moveZ = 1;
                        if (phaseSteps38 > 120 * 8) Next38(1); // could not stand there: pull from here
                    }
                    break;
                case 1: // a charge in hand, then the pull
                    (f.yaw, f.pitch) = Loom37Pilot.LookAt(sim.Eye, aim);
                    if (h.stand == null) f.jump = Press(true, false);
                    if (ready && step % 4 == 0) { f.grapple = Press(true, true); pulls38++; sawRope38 = false; Next38(2); }
                    else if (h.stand != null && phaseSteps38 > 120 * 10) Next38(0);
                    break;
                case 2: // hold through the pull; at its arrival, the next hop (an in-air one at once), else land first
                    (f.yaw, f.pitch) = Loom37Pilot.LookAt(sim.Eye, aim);
                    f.grapple = Press(true, false);
                    if (m.rope.phase != RopePhase.None && !sawRope38) { sawRope38 = true; ropes38++; }
                    if (phaseSteps38 > 6 && m.rope.phase == RopePhase.None)
                    {
                        if (!sawRope38) { Next38(1); break; } // the hook never went out: press again
                        gate38++;
                        Next38(gate38 < hops.Length && hops[gate38].stand == null ? 1 : 3);
                    }
                    else if (phaseSteps38 > 240) Next38(1);
                    break;
                case 3: // land (drift on toward the next stand, or the exit), then the next hop
                {
                    Vec3 to = gate38 < hops.Length && hops[gate38].stand != null ? W(hops[gate38].stand) : rd.exit;
                    f.yaw = Toward(p, to.x, to.z);
                    f.moveZ = 1;
                    if (grounded && phaseSteps38 > 4) { gatesCrossed38++; Next38(0); }
                    else if (phaseSteps38 > 600) Next38(0);
                    break;
                }
            }
            return true;
        }

        void Next38(int ph)
        {
            phase38 = ph;
            phaseSteps38 = 0;
        }
    }
}
