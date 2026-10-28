// NIGHT36 builder M (NIGHT36.md M1, L-041): the walk pilot's scripts for builder L's new level 1 (Design/levels/n36-foundry.json, rooms a1..a8;
// Design/levels/n36/foundry.beats.md). Room-relative positions as Walk29Pilot's (lz = metres north of the room's south edge, lx = east of its centre
// line); the numbers are the level file's (Tools/night/gen-n36-foundry.cjs). The autopilot fights and walks everywhere a room has no movement job; the
// boss room a8 is not played (builder B's).
//   a1 INTAKE (a link, no enemy): the autopilot climbs the 1.0 m step and crosses the 7 m trench; the SLIDE slot (lz 22..27, a 1.2 m ceiling, the only
//      way north) is scripted: straight up the centre line, the slide pressed at lz 20 and held to lz 27.5 (the lesson `slidTunnel` wants a slide seen in
//      the slot, then the player grounded past it).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Walk29Pilot
    {
        bool slid36;

        bool Level36 => sim.campaignLevel != null && (sim.campaignLevel.id ?? "").StartsWith("n36");

        void Reset36()
        {
            slid36 = false;
            wp36 = 0;
        }

        /// <summary>The n36 room's own frame, or false: the autopilot's.</summary>
        bool Room36(RoomDef rd, Vec3 p, double cx, double lz, double lx, bool grounded, bool cleared, InputFrame auto, out InputFrame f)
        {
            f = new InputFrame { moveZ = 1 };
            // the lava pits (n36move deaths a5, round 3: 71 of 76 deaths were lava: the autopilot's graph has no lava, so it walked straight at the deck's
            // spitters across the pool and, once in, hopped about in it (the lava's knock-ups) until dead): out by the nearest rim, and never walk in
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
                    f = auto; // stand at the rim and fight from there (the autopilot's aim and fire kept)
                    f.moveX = f.moveZ = 0;
                    f.jump = default;
                    return true;
                }
            }
            if (!IdScripts65(rd)) return false; // NIGHT65 T1: a rebuilt room's row says bot43: false (BotRows65): the autopilot's, not the old id script
            switch (rd.id)
            {
                case "a1":
                    // round 3 (n36move deaths a1 --trace: 29 s against par 12): the autopilot does not move in a1 (8 s still at the spawn until the
                    // unstick), and it fell into the trench (no dash: 7 s stuck in it). Scripted end to end: up the 1.0 m step, a jump + dash over the 7 m
                    // trench (lz 11..18) from the step's edge, out of the trench if it ever lands in it (north, a jump and a double jump), the slot
                    if (lz >= 28) return false;
                    if (lz < 19.5)
                    {
                        f.yaw = Toward(p, cx, rd.z1 - 30);
                        if (p.y < -1.0 && lz > 11 && lz < 18) // in the trench: hop out north (1.5 m: a jump and a double jump)
                        {
                            if (grounded) { f.jump = Press(true, true); jumpStep = step; doubled = false; }
                            else if (!doubled && step - jumpStep >= 30) { f.jump = Press(true, true); doubled = true; }
                            else f.jump = Press(true, false);
                            return true;
                        }
                        if (lz < 9.5)
                        {
                            if (grounded && p.y < 0.5 && lz >= 3.5) f.jump = Press(true, true); // the step (lz 6..10, 1.0 m): jumped 2.5 m out
                            else if (!grounded) f.jump = Press(true, false); // held: a released jump is cut to half its height (jump.releaseMult)
                            jumped = dashed = doubled = false;
                            return true;
                        }
                        f = GapRoom(rd, p, cx, lz, grounded, 11, 7, true, auto); // the jump at lz 9.8 (the step's edge), the dash 26 steps later
                        if (lz >= 18.5) { f = new InputFrame { moveZ = 1, yaw = Toward(p, cx, rd.z1 - 30) }; }
                        return true;
                    }
                    f.yaw = Toward(p, cx, p.z - 10);
                    f.moveX = 0;
                    if (lz < 21.5) f.yaw = Toward(p, cx, rd.z1 - 22); // onto the centre line before the slot
                    bool slide = lz >= 20 && lz < 27.5;
                    f.slide = Press(slide, slide && !slid36);
                    if (slide) slid36 = true;
                    return true;
                case "a3":
                    // the room clears on the husks AND on reaching the north floor (lz 23..26): with the husks dead (or never come), the bridge (lx 6..8,
                    // the walkers' dry route along the east wall) to the north floor
                    if (cleared || AliveNonStandIn() > 0) return false;
                    return Via36(rd, p, cx, A3Via, ref f);
                case "a5":
                    // cleared: the straight line to the exit crosses the lava (lx -6..6, lz 6..17; the autopilot walked in, 426 deaths on 2 of 10 seeds):
                    // the level's walk proof instead: the west rim, its ramp onto the deck (lz 17 -> 19.5, 1.2 m), the deck's middle, the exit
                    if (!cleared) return false;
                    return Via36(rd, p, cx, A5Via, ref f);
            }
            return false;
        }

        /// <summary>The pilot's look-ahead (m) for walking into lava (a bot number, not gameplay).</summary>
        const double LavaLook36 = 1.5;
        System.Collections.Generic.List<double[]> lava36;

        /// <summary>The level's lava pits (json campaign.pits with lava true: [x0, z0, x1, z1]), read once.</summary>
        System.Collections.Generic.List<double[]> Lava36Rects()
        {
            if (lava36 != null) return lava36;
            lava36 = new System.Collections.Generic.List<double[]>();
            if (sim.map.json != null && sim.map.json.TryGetValue("campaign", out object co) && co is System.Collections.Generic.Dictionary<string, object> camp
                && camp.TryGetValue("pits", out object po) && po is System.Collections.Generic.List<object> pits)
                foreach (object o in pits)
                    if (o is System.Collections.Generic.Dictionary<string, object> d && d.TryGetValue("lava", out object lv) && lv is bool b && b && d.TryGetValue("rect", out object ro))
                    {
                        var r = PortData.Arr(ro);
                        lava36.Add(new[] { PortData.Num(r[0]), PortData.Num(r[1]), PortData.Num(r[2]), PortData.Num(r[3]) });
                    }
            return lava36;
        }

        bool InLava36(double x, double z)
        {
            foreach (double[] r in Lava36Rects()) if (x > r[0] && x < r[2] && z > r[1] && z < r[3]) return true;
            return false;
        }

        /// <summary>In a lava pit (down in it, or in the air over it under 1 m: the lava's knock-up throws the player about): a dry point 1.5 m past its
        /// nearer south / north edge (round 3: the east / west edges of a5's pool sit behind the ride panels standing in it).</summary>
        bool Lava36(Vec3 p, bool grounded, out double ex, out double ez)
        {
            ex = ez = 0;
            if (p.y > -0.3 && (grounded || p.y > 1.0)) return false;
            foreach (double[] r in Lava36Rects())
            {
                if (!(p.x > r[0] && p.x < r[2] && p.z > r[1] && p.z < r[3])) continue;
                double ds = p.z - r[1], dn = r[3] - p.z;
                ex = p.x;
                ez = ds <= dn ? r[1] - 1.5 : r[3] + 1.5;
                return true;
            }
            return false;
        }

        System.Collections.Generic.HashSet<string> off65;

        /// <summary>NIGHT65 T1 (redteam.md 5): false when the level file's row of this room carries <c>"bot43": false</c> (a room rebuilt in place under
        /// its old id: the id-keyed scripts of Walk36 / Walk38Foundry / Walk43Foundry would drive the OLD room's geometry); the generic autopilot then
        /// runs there. Every room of every level before NIGHT65 has no such key: true (the scripts as before).</summary>
        public bool IdScripts65(RoomDef rd)
        {
            if (rd == null) return true;
            off65 ??= BotRows65.OffRooms(sim.map.json);
            return !off65.Contains(rd.id);
        }

        static readonly double[][] A3Via = { new[] { 7.0, 10 }, new[] { 7.0, 23.5 }, new[] { 0.0, 24.5 } };
        static readonly double[][] A5Via = { new[] { -8.0, 2 }, new[] { -8.0, 13 }, new[] { -8.0, 19.5 }, new[] { 0.0, 20 }, new[] { 0.0, 22.5 } };
        int wp36;

        /// <summary>Walks the room-relative points (lx, lz) in order from the nearest one not yet passed; false when past the last.</summary>
        bool Via36(RoomDef rd, Vec3 p, double cx, double[][] via, ref InputFrame f)
        {
            while (wp36 < via.Length && JsMath.Hypot(p.x - (cx + via[wp36][0]), p.z - (rd.z1 - via[wp36][1])) < 1.0) wp36++;
            if (wp36 >= via.Length) return false;
            f.yaw = Toward(p, cx + via[wp36][0], rd.z1 - via[wp36][1]);
            f.moveZ = 1;
            return true;
        }
    }

    /// <summary>NIGHT65 T1: the level file's campaign rooms whose row says <c>"bot43": false</c> (read once per pilot; a bot rule, never gameplay).</summary>
    public static class BotRows65
    {
        public static System.Collections.Generic.HashSet<string> OffRooms(System.Collections.Generic.Dictionary<string, object> json)
        {
            var off = new System.Collections.Generic.HashSet<string>();
            if (json != null && json.TryGetValue("campaign", out object co) && co is System.Collections.Generic.Dictionary<string, object> camp
                && camp.TryGetValue("rooms", out object ro) && ro is System.Collections.Generic.List<object> rooms)
                foreach (object o in rooms)
                    if (o is System.Collections.Generic.Dictionary<string, object> r && r.TryGetValue("bot43", out object b) && b is bool on && !on
                        && r.TryGetValue("id", out object id) && id is string s)
                        off.Add(s);
            return off;
        }
    }
}
