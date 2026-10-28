// NIGHT43 builder F1 round 2 (design.n43.look43, a view row): the look kit's pure half (EditMode tests: Assets/Tests/EditMode/Game/Look43Tests.cs).
// Reads a campaign level's json (LEVEL-FORMAT.md, web x / z, z south) into what the kit lights: the rooms (rect, height, kind, exit), the doors
// (between, rect, height), the lava pools (the top-level `lava`), the ride panels (campaign boxes RideTrim36.IsRidePanel picks: `ride: true`, or
// a "ride panel / wall" note on an untagged level), and answers: which door leaves a room, which door leaves the room after it, which way a door
// opens (its posts), where the dashes of a ride strip go, a colour's share of its saturation drained. No engine objects (doubles, web space).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class LevelLook43
    {
        public sealed class Room
        {
            public string id, kind;
            public double x0, z0, x1, z1, height, floorY;
            public double exitX, exitZ;
        }

        public sealed class Door
        {
            public string id, a, b;
            public double x0, z0, x1, z1, height;
        }

        public struct Box
        {
            public double x0, y0, z0, x1, y1, z1;
        }

        public readonly List<Room> rooms = new List<Room>();
        public readonly List<Door> doors = new List<Door>();
        public readonly List<Box> lava = new List<Box>(); // y0 = y1 = the surface
        public readonly List<Box> ride = new List<Box>();
        public double wallHeight = 6;

        static double N(object o) => PortData.Num(o);

        static bool Rect(Dictionary<string, object> d, string key, out double x0, out double z0, out double x1, out double z1)
        {
            x0 = z0 = x1 = z1 = 0;
            if (d == null || !d.TryGetValue(key, out object r) || !(r is List<object> a) || a.Count < 4) return false;
            double ax = N(a[0]), az = N(a[1]), bx = N(a[2]), bz = N(a[3]);
            x0 = Math.Min(ax, bx); x1 = Math.Max(ax, bx); z0 = Math.Min(az, bz); z1 = Math.Max(az, bz);
            return x1 > x0 && z1 > z0;
        }

        /// <summary>The level's look data; an empty model for a map with no campaign block.</summary>
        public static LevelLook43 From(Dictionary<string, object> json)
        {
            var L = new LevelLook43();
            if (json == null || !json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return L;
            if (camp.TryGetValue("wallHeight", out object wh) && wh != null) L.wallHeight = N(wh);
            if (camp.TryGetValue("rooms", out object ro) && ro is List<object> rooms)
                foreach (object o in rooms)
                {
                    if (!(o is Dictionary<string, object> d) || !Rect(d, "rect", out double x0, out double z0, out double x1, out double z1)) continue;
                    var r = new Room { id = d.TryGetValue("id", out object id) ? id as string : null, kind = d.TryGetValue("kind", out object k) && k is string ks ? ks : "room", x0 = x0, z0 = z0, x1 = x1, z1 = z1 };
                    r.height = d.TryGetValue("height", out object h) && h != null ? N(h) : L.wallHeight;
                    if (d.TryGetValue("exit", out object ex) && ex is List<object> e && e.Count >= 3) { r.exitX = N(e[0]); r.floorY = N(e[1]); r.exitZ = N(e[2]); }
                    else { r.exitX = (x0 + x1) / 2; r.exitZ = z0; }
                    L.rooms.Add(r);
                }
            if (camp.TryGetValue("doors", out object dor) && dor is List<object> doors)
                foreach (object o in doors)
                {
                    if (!(o is Dictionary<string, object> d) || !Rect(d, "rect", out double x0, out double z0, out double x1, out double z1)) continue;
                    if (!d.TryGetValue("between", out object bt) || !(bt is List<object> b) || b.Count < 2) continue;
                    L.doors.Add(new Door { id = d.TryGetValue("id", out object id) ? id as string : null, a = b[0] as string, b = b[1] as string, x0 = x0, z0 = z0, x1 = x1, z1 = z1, height = d.TryGetValue("height", out object h) && h != null ? N(h) : 4 });
                }
            if (json.TryGetValue("lava", out object lv) && lv is List<object> lavas)
                foreach (object o in lavas)
                    if (LavaView30Math.Rect(o, out double x0, out double z0, out double x1, out double z1, out double y))
                        L.lava.Add(new Box { x0 = x0, z0 = z0, x1 = x1, z1 = z1, y0 = y, y1 = y });
            if (camp.TryGetValue("boxes", out object bo) && bo is List<object> boxes)
            {
                bool tagged = false;
                foreach (object o in boxes)
                    if (o is Dictionary<string, object> d && d.TryGetValue("ride", out object rv) && rv is bool rt && rt) { tagged = true; break; }
                foreach (object o in boxes)
                {
                    var d = o as Dictionary<string, object>;
                    if (!RideTrim36.IsRidePanel(d, tagged) || !d.ContainsKey("min") || !d.ContainsKey("max")) continue;
                    if (d.TryGetValue("noride", out object nr) && nr is bool nrb && nrb) continue; // V-3: a wall that cannot be ridden never has it
                    List<object> mn = PortData.Arr(d["min"]), mx = PortData.Arr(d["max"]);
                    var bx = new Box { x0 = N(mn[0]), y0 = N(mn[1]), z0 = N(mn[2]), x1 = N(mx[0]), y1 = N(mx[1]), z1 = N(mx[2]) };
                    if (bx.x1 > bx.x0 && bx.z1 > bx.z0 && bx.y1 > bx.y0) L.ride.Add(bx);
                }
            }
            return L;
        }

        public int RoomIndex(string id)
        {
            for (int i = 0; i < rooms.Count; i++) if (rooms[i].id == id) return i;
            return -1;
        }

        /// <summary>The door that leaves room `id` (between[0] is the room), -1 when none (the last room, a boss).</summary>
        public int ExitDoor(string id)
        {
            if (id == null) return -1;
            for (int i = 0; i < doors.Count; i++) if (doors[i].a == id) return i;
            return -1;
        }

        /// <summary>The door that leaves the room after `id` (through `id`'s exit door), -1 when none.</summary>
        public int NextDoor(string id)
        {
            int d = ExitDoor(id);
            return d < 0 ? -1 : ExitDoor(doors[d].b);
        }

        /// <summary>Does the passage through door `d` run along z (so its posts stand at x0 / x1)? From its two rooms' centres; the door's own
        /// shape when a room is missing (the long side is the opening's width).</summary>
        public bool PassAlongZ(int d)
        {
            Door door = doors[d];
            int a = RoomIndex(door.a), b = RoomIndex(door.b);
            if (a >= 0 && b >= 0)
            {
                double dx = (rooms[b].x0 + rooms[b].x1) / 2 - (rooms[a].x0 + rooms[a].x1) / 2;
                double dz = (rooms[b].z0 + rooms[b].z1) / 2 - (rooms[a].z0 + rooms[a].z1) / 2;
                return Math.Abs(dz) >= Math.Abs(dx);
            }
            return door.x1 - door.x0 >= door.z1 - door.z0;
        }

        /// <summary>A room big enough to be an arena (gets a landmark): kind "room" and its short side >= Look43MarkMinRoom.</summary>
        public static bool IsArena(Room r) => r != null && r.kind == "room" && Math.Min(r.x1 - r.x0, r.z1 - r.z0) >= UnityTuning.Look43MarkMinRoom;

        /// <summary>The dashes' starts along a strip of length `len` (m): dash + gap repeating, centred so both ends keep the same margin.</summary>
        public static int Dashes(double len, double dash, double gap, List<double> starts)
        {
            starts?.Clear();
            if (len < dash || dash <= 0) return 0;
            int n = (int)Math.Floor((len + gap) / (dash + gap));
            double used = n * dash + (n - 1) * gap, margin = (len - used) / 2;
            for (int i = 0; i < n; i++) starts?.Add(margin + i * (dash + gap));
            return n;
        }

        /// <summary>A colour drained `share` toward its luma (Rec. 709) and scaled by `value`; alpha kept.</summary>
        public static Color Drain(Color c, float share, float value)
        {
            float l = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
            return new Color((c.r + (l - c.r) * share) * value, (c.g + (l - c.g) * share) * value, (c.b + (l - c.b) * share) * value, c.a);
        }

        /// <summary>A pulse 1 +- depth at hz (1 with reduced motion).</summary>
        public static float Pulse(double t, float hz, float depth, bool reduced) => reduced ? 1f : 1f + depth * (float)Math.Sin(2 * Math.PI * hz * t);

        /// <summary>The room whose rect holds (x, z) (the first), null when none.</summary>
        public Room RoomAt(double x, double z)
        {
            foreach (Room r in rooms) if (x >= r.x0 && x <= r.x1 && z >= r.z0 && z <= r.z1) return r;
            return null;
        }

        /// <summary>Does the edge (ax, az) -> (bx, bz) lie on the room's own wall line (within 0.15 m)? A lava edge there has a wall to climb.</summary>
        public static bool OnRoomWall(Room r, double ax, double az, double bx, double bz)
        {
            const double e = 0.15;
            bool sameX = Math.Abs(ax - bx) < 1e-6, sameZ = Math.Abs(az - bz) < 1e-6;
            if (sameX) return Math.Abs(ax - r.x0) < e || Math.Abs(ax - r.x1) < e;
            if (sameZ) return Math.Abs(az - r.z0) < e || Math.Abs(az - r.z1) < e;
            return false;
        }

        /// <summary>The lava pool nearest (x, z) inside the room's rect (or touching it), -1 when the room has none.</summary>
        public int LavaIn(Room r, double x, double z)
        {
            int best = -1;
            double bd = double.MaxValue;
            for (int i = 0; i < lava.Count; i++)
            {
                Box b = lava[i];
                if (b.x1 < r.x0 || b.x0 > r.x1 || b.z1 < r.z0 || b.z0 > r.z1) continue;
                double cx = (b.x0 + b.x1) / 2 - x, cz = (b.z0 + b.z1) / 2 - z, d = cx * cx + cz * cz;
                if (d < bd) { bd = d; best = i; }
            }
            return best;
        }
    }
}
