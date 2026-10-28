// NIGHT61 builder V (slate S1, the exit frame; view row design.n61.exitFrame61): the exit frame's pure parts, in web space (three.js axes, metres),
// with no Unity object, so EditMode tests pin them (N61ExitFrameTests): a door's passage, the frame at room A's face, the gate at the far mouth,
// the passage light's spot, the floor line's run and dashes, the things on a level that stop the line, the switch and the Player's proof line.
// ExitFrame61.N61V.cs draws them. The geometry follows n57-kiln's campaign doors: a door's rect is its whole passage (4 m deep there), its floor
// at y 0 (Exit58 and a157's clear-shot box assume the same), the opening `height` tall; room A is between[0] (the room the clear opens it from).
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class ExitFrame61Math
    {
        /// <summary>The view row's name (design.n61.exitFrame61).</summary>
        public const string Row = "exitFrame61";
        /// <summary>The one-row off switch: this row alone off.</summary>
        public const string NoSwitch = "-skygrindNoExitFrame61";
        /// <summary>lamp60's one-row switch, the slate's off side ("on vs -skygrindNoExit58"): no exit lamp at all.</summary>
        public const string OldSwitch = "-skygrindNoExit58";
        /// <summary>The night of the row (its -skygrindOffNN switch covers it from this night on).</summary>
        public const int Night = 61;

        /// <summary>An axis-aligned box in web space: its centre and its size (m).</summary>
        public readonly struct Part
        {
            public readonly double x, y, z, sx, sy, sz;

            public Part(double x, double y, double z, double sx, double sy, double sz)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.sx = sx;
                this.sy = sy;
                this.sz = sz;
            }

            public override string ToString() => $"({x:0.###}, {y:0.###}, {z:0.###}) size ({sx:0.###}, {sy:0.###}, {sz:0.###})";
        }

        /// <summary>A door's passage: the rect's centre (web x, z), the unit direction from room A toward room B (one axis), the opening's width
        /// (across the passage), the passage's length (along it), the opening's height, and whether the opening spans x.</summary>
        public readonly struct Door
        {
            public readonly double cx, cz, ax, az, width, depth, height;
            public readonly bool spansX;

            public Door(double cx, double cz, double ax, double az, double width, double depth, double height, bool spansX)
            {
                this.cx = cx;
                this.cz = cz;
                this.ax = ax;
                this.az = az;
                this.width = width;
                this.depth = depth;
                this.height = height;
                this.spansX = spansX;
            }

            /// <summary>Room A's face of the passage, on its axis (web x).</summary>
            public double FaceX => cx - ax * depth / 2;
            /// <summary>Room A's face of the passage, on its axis (web z).</summary>
            public double FaceZ => cz - az * depth / 2;

            /// <summary>The point `along` m from room A's face into room A (negative: into the passage, -depth at room B's face), `lateral` m
            /// across the opening (+x when it spans x, else +z), `y` m up.</summary>
            public Vec3 At(double along, double lateral, double y) =>
                new Vec3(FaceX - ax * along + (spansX ? lateral : 0), y, FaceZ - az * along + (spansX ? 0 : lateral));

            /// <summary>A box centred there, `across` wide along the opening, `up` tall, `deep` along the passage.</summary>
            public Part Box(double along, double lateral, double y, double across, double up, double deep)
            {
                Vec3 c = At(along, lateral, y);
                return spansX ? new Part(c.x, c.y, c.z, across, up, deep) : new Part(c.x, c.y, c.z, deep, up, across);
            }
        }

        /// <summary>A thing a floor line may meet (web min / max): a solid, or a floor (a floor slab, lava: its top is the floor there).</summary>
        public readonly struct Block
        {
            public readonly double x0, y0, z0, x1, y1, z1;
            public readonly bool floor;

            public Block(double x0, double y0, double z0, double x1, double y1, double z1, bool floor)
            {
                this.x0 = x0;
                this.y0 = y0;
                this.z0 = z0;
                this.x1 = x1;
                this.y1 = y1;
                this.z1 = z1;
                this.floor = floor;
            }
        }

        /// <summary>A door's passage from campaign.doors[] (rect [x0, z0, x1, z1], height) and Doors38Math's axis (Ahead, SpansX).</summary>
        public static Door DoorOf(double[] rect, double height, double ax, double az, bool spansX)
        {
            double w = spansX ? rect[2] - rect[0] : rect[3] - rect[1];
            double d = spansX ? rect[3] - rect[1] : rect[2] - rect[0];
            return new Door((rect[0] + rect[2]) / 2, (rect[1] + rect[3]) / 2, ax, az, w, d, height, spansX);
        }

        /// <summary>THE FRAME at room A's face: two posts and a lintel, `barW` wide from the opening's edges outward (the posts from the floor to
        /// the opening's top, the lintel over the opening and both posts), `barD` deep, their back `gap` in front of the face (never in the wall).</summary>
        public static Part[] Frame(Door d, double barW, double barD, double gap)
        {
            double along = gap + barD / 2;
            double side = d.width / 2 + barW / 2;
            return new[]
            {
                d.Box(along, -side, d.height / 2, barW, d.height, barD),
                d.Box(along, side, d.height / 2, barW, d.height, barD),
                d.Box(along, 0, d.height + barW / 2, d.width + 2 * barW, barW, barD),
            };
        }

        /// <summary>THE GATE inside the passage's far mouth (room B's face), the passage light's fixture: two posts and a lintel `gateW` wide, `barD`
        /// deep, `inset` clear of the passage's walls, its ceiling and its far face (never coplanar with them).</summary>
        public static Part[] Gate(Door d, double gateW, double barD, double inset)
        {
            double along = -(d.depth - inset - barD / 2);
            double postH = d.height - inset - gateW;
            double side = d.width / 2 - inset - gateW / 2;
            return new[]
            {
                d.Box(along, -side, postH / 2, gateW, postH, barD),
                d.Box(along, side, postH / 2, gateW, postH, barD),
                d.Box(along, 0, d.height - inset - gateW / 2, d.width - 2 * inset, gateW, barD),
            };
        }

        /// <summary>THE PASSAGE LIGHT's spot: on the door's axis, `back` m on room A's side of the gate and `drop` m under its lintel.</summary>
        public static Vec3 LightSpot(Door d, double gateW, double barD, double inset, double back, double drop) =>
            d.At(-(d.depth - inset - barD) + back, 0, d.height - inset - gateW - drop);

        /// <summary>A point's distance from room A's face into room A along the door's axis (m; negative in the passage).</summary>
        public static double Along(Door d, double x, double z) => d.spansX ? (z - d.FaceZ) * -d.az : (x - d.FaceX) * -d.ax;

        /// <summary>THE FLOOR LINE's run back into room A from its face (m): `maxLen`, or `clear` short of the first thing on its strip (`half` m
        /// either side of the axis): a solid whose top is over the floor (by more than `step`) and whose bottom is under `headRoom` (a ledge higher
        /// up is passed under), or a floor at another height (a pit, a tier, lava). Things beside the strip or behind the face are ignored; 0 when
        /// the first thing reaches the face.</summary>
        public static double FloorRun(Door d, IList<Block> blocks, double floorY, double maxLen, double half, double clear, double headRoom, double step)
        {
            double best = maxLen;
            if (blocks == null) return best;
            for (int i = 0; i < blocks.Count; i++)
            {
                Block b = blocks[i];
                bool stops = b.floor ? Math.Abs(b.y1 - floorY) > step : b.y1 > floorY + step && b.y0 < floorY + headRoom;
                if (!stops) continue;
                double l0, l1, p, q;
                if (d.spansX)
                {
                    l0 = b.x0 - d.FaceX;
                    l1 = b.x1 - d.FaceX;
                    p = (b.z0 - d.FaceZ) * -d.az;
                    q = (b.z1 - d.FaceZ) * -d.az;
                }
                else
                {
                    l0 = b.z0 - d.FaceZ;
                    l1 = b.z1 - d.FaceZ;
                    p = (b.x0 - d.FaceX) * -d.ax;
                    q = (b.x1 - d.FaceX) * -d.ax;
                }
                if (l1 <= -half || l0 >= half) continue; // beside the strip
                double a0 = Math.Min(p, q), a1 = Math.Max(p, q);
                if (a1 <= 0) continue; // behind room A's face: the passage, room B
                best = Math.Min(best, a0 - clear);
            }
            return Math.Max(0, best);
        }

        /// <summary>THE FLOOR LINE's dashes: from room B's face (the passage's far mouth) through the doorway into room A to `run` m from its face,
        /// `dash` long with `gap` between, `width` wide on the axis, `lift` over the floor and `thick` thick; the last dash in room A ends at the
        /// run's end (a sliver under 5 cm is left out).</summary>
        public static List<Part> Line(Door d, double run, double floorY, double width, double dash, double gap, double lift, double thick)
        {
            var parts = new List<Part>();
            if (dash <= 0) return parts;
            double pitch = dash + Math.Max(0, gap);
            for (int i = 0; i < 1000; i++)
            {
                double s = -d.depth + i * pitch;
                if (s >= run) break;
                double e = Math.Min(s + dash, run);
                if (e - s < 0.05) break;
                parts.Add(d.Box((s + e) / 2, 0, floorY + lift + thick / 2, width, thick, e - s));
            }
            return parts;
        }

        /// <summary>A dash's share of the line's glow at `along` (m from room A's face): 1 in the doorway and at the door, falling to `dim` at the
        /// run's end in room A (the line brightens toward the door).</summary>
        public static double LineGlow(double along, double run, double dim)
        {
            if (along <= 0 || run <= 0) return 1;
            return 1 - (1 - dim) * Math.Min(1, along / run);
        }

        /// <summary>The things a floor line may meet on a level (its map JSON): collision.boxes (a "floor" box is a floor at its top), barriers,
        /// the ramps (collision.wedges: their footprint's box, 0 up to their rise) and the lava (its rect, its surface y a floor).</summary>
        public static List<Block> Blocks(Dictionary<string, object> mapJson)
        {
            var list = new List<Block>();
            if (mapJson == null) return list;
            if (mapJson.TryGetValue("collision", out object co) && co is Dictionary<string, object> col)
            {
                AddBoxes(list, col, "boxes", true);
                AddBoxes(list, col, "barriers", false);
                if (col.TryGetValue("wedges", out object wo) && wo is List<object> wedges)
                {
                    foreach (object w in wedges)
                    {
                        if (!(w is Dictionary<string, object>)) continue;
                        WedgeSpec spec = PortData.Wedge(w);
                        Vec3[] c = ArenaMap.WedgeCorners(spec);
                        double x0 = double.MaxValue, z0 = double.MaxValue, x1 = double.MinValue, z1 = double.MinValue, top = 0;
                        foreach (Vec3 v in c)
                        {
                            x0 = Math.Min(x0, v.x);
                            x1 = Math.Max(x1, v.x);
                            z0 = Math.Min(z0, v.z);
                            z1 = Math.Max(z1, v.z);
                            top = Math.Max(top, v.y);
                        }
                        list.Add(new Block(x0, 0, z0, x1, top, z1, false));
                    }
                }
            }
            if (mapJson.TryGetValue("lava", out object lo) && lo is List<object> lava)
            {
                foreach (object o in lava)
                {
                    if (!(o is Dictionary<string, object> l) || !l.TryGetValue("rect", out object ro) || !(ro is List<object> r) || r.Count < 4) continue;
                    double y = l.TryGetValue("y", out object yo) ? PortData.Num(yo) : 0;
                    list.Add(new Block(PortData.Num(r[0]), y - 1, PortData.Num(r[1]), PortData.Num(r[2]), y, PortData.Num(r[3]), true));
                }
            }
            return list;
        }

        static void AddBoxes(List<Block> list, Dictionary<string, object> col, string key, bool kinds)
        {
            if (!col.TryGetValue(key, out object bo) || !(bo is List<object> boxes)) return;
            foreach (object o in boxes)
            {
                if (!(o is Dictionary<string, object> b) || !b.TryGetValue("min", out object mn) || !(mn is List<object> lo) || lo.Count < 3
                    || !b.TryGetValue("max", out object mx) || !(mx is List<object> hi) || hi.Count < 3) continue;
                bool floor = kinds && b.TryGetValue("kind", out object k) && k as string == "floor";
                list.Add(new Block(PortData.Num(lo[0]), PortData.Num(lo[1]), PortData.Num(lo[2]), PortData.Num(hi[0]), PortData.Num(hi[1]), PortData.Num(hi[2]), floor));
            }
        }

        /// <summary>Is the exit frame off for this config and command line? Off when the row is off (NightRows.ViewOn: its design `enabled`, the
        /// process's -skygrindOffNN), under -skygrindOff61 or an older night's switch, -skygrindNoExitFrame61 (this row alone) or
        /// -skygrindNoExit58 (no exit lamp at all: the slate's off side).</summary>
        public static bool OffFor(GameConfig cfg, string[] args) => OffReason(cfg, args) != null;

        /// <summary>Why it is off (for the reel's line), null when on.</summary>
        public static string OffReason(GameConfig cfg, string[] args)
        {
            if (!NightRows.ViewOn(cfg, Row)) return "design.n61." + Row + " off";
            if (NightRows.OffFrom(args, "-skygrindOff") <= Night) return "-skygrindOff61 (or older)";
            if (Has(args, NoSwitch)) return NoSwitch;
            if (Has(args, OldSwitch)) return OldSwitch;
            return null;
        }

        /// <summary>NIGHT63 R1 N1 (Logs/n61/review-n61.md N1): Build makes the one-time cube and additive material only when it frames doors: the row
        /// on (no off reason) and the map is the frame's own level. Off (or another level): nothing is made (no cube with a collider for a frame, no
        /// unused material).</summary>
        public static bool MakesAssets(string offReason, string levelName) => offReason == null && levelName == ExitFrame61.Level;

        /// <summary>A switch on the command line (case ignored: NightRows.HasSwitch, the one rule since NIGHT63 R1 N5).</summary>
        static bool Has(string[] args, string flag) => NightRows.HasSwitch(args, flag);

        /// <summary>The Player's proof line when a door lights (L-089: the reel greps "[exit61] door lit: room").</summary>
        public static string LitLine(string room, string door, int frame) => "[exit61] door lit: room " + room + " (door " + door + ", frame " + frame + ")";
    }
}
