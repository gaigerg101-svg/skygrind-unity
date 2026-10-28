// NIGHT33 L (design.n33.look33; DESIGN33 L, L1 section 5): WHERE every fixture and every light of a CAMPAIGN level goes, from the level JSON alone.
// Unity-free on purpose (no UnityEngine): the views (CampaignLighting24, LightKit29, RoofView27) build what Green() returns, and the harness's `light33`
// scans every level with the same code. Web coordinates throughout (x, y, z as in the level JSON; the views flip z).
//   Red()   = NIGHT32's placement, modelled for the check: full-length floor trims (they cross pits and lava), the moving room light at a fixed
//             height in each room's middle, the guide light floating over the exit, pool lights 1.5 m over the floor with no fixture (look30 dropped
//             the floor quad), ceiling panels hung 0.05 m under the roof, roof strips at ONE level-wide height, pool shafts rising 6 m from the floor.
//   Green() = look33: trims cut at every pit / lava edge; each room's light under a flush framed ceiling panel at its middle; the guide under a lamp on
//             the exit door's lintel; sockets snapped flush onto their wall / ceiling; a floor pool becomes a framed downlight on the ceiling above it
//             (or a lava light 1.2 m over the lava when the pool is over lava); roof strips under each room's own roof; pool shafts hang from their panel.
//   Check() = the light33 numbers: lights, lights with no fixture box within 0.6 m, emissive bars not mounted (a 0.5 m slice > 0.02 m from geometry),
//             lights over a pit or lava (a lava light is by design and counted apart).
//   NIGHT35 V lights35 (Nums.lights35; the lead's n34-r5 frames f5 / f5b: ceiling panels, frames and white roof strips hung at the level-wide ceiling of
//             rooms with NO roof read as floating "little triangle displays" in the sky): a fixture only where a surface holds it. An open room's key is a
//             warm FLOOD lamp on its nearest long wall (under the wall's top), its pools and ceiling sockets become wall sconces, it gets no roof strips,
//             and its guide lamp hangs only under a real lintel or roof. SkyFixtures counts what is left in the sky (0 with lights35).
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class LightPlan33
    {
        public struct Box
        {
            public double x0, y0, z0, x1, y1, z1;
            public Box(double ax, double ay, double az, double bx, double by, double bz)
            {
                x0 = Math.Min(ax, bx); y0 = Math.Min(ay, by); z0 = Math.Min(az, bz); x1 = Math.Max(ax, bx); y1 = Math.Max(ay, by); z1 = Math.Max(az, bz);
            }
            public static Box Around(double cx, double cy, double cz, double hx, double hy, double hz) => new Box(cx - hx, cy - hy, cz - hz, cx + hx, cy + hy, cz + hz);
            public double Gap(Box o)
            {
                double dx = Math.Max(0, Math.Max(o.x0 - x1, x0 - o.x1)), dy = Math.Max(0, Math.Max(o.y0 - y1, y0 - o.y1)), dz = Math.Max(0, Math.Max(o.z0 - z1, z0 - o.z1));
                return Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
            public double Dist(double x, double y, double z)
            {
                double dx = Math.Max(0, Math.Max(x0 - x, x - x1)), dy = Math.Max(0, Math.Max(y0 - y, y - y1)), dz = Math.Max(0, Math.Max(z0 - z, z - z1));
                return Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
        }

        /// <summary>A fixture: a box (web space). kind: trim | roofStrip | socket | panel | frame | poolPanel | guideLamp | beam. Emissive ones glow; frames are dark.</summary>
        public sealed class Fix
        {
            public string kind, room, color, id;
            public string type; // round 2: the L1 fixture type (sconce | strip | pole | panel | brazier | lava | trim | roofStrip | guide)
            public double hdr; // the emissive HDR multiplier the view uses (L1: <= 4 fixtures, <= 2.2 lava / area)
            public bool hasHolder; public Box holder; // a part held by another part (a lens on its body): the mount check counts the holder as geometry
            public Box box;
            public bool emissive;
            public bool bar; // a long thin emissive (trim, strip, socket bar): the mount check slices it
            public double tx, ty, tz; // a beam's top end (where its light comes from)
            public double bx, by, bz; // a beam's bottom end
            public double topW, bottomW; // a beam's widths
        }

        /// <summary>A light position. kind: room | guide | socket | pool | lava | roof. A socket / pool light keeps its plan row id.</summary>
        public sealed class Lit
        {
            public string kind, room, color, id, type;
            public double x, y, z, range, intensity;
            public bool accent;
        }

        public sealed class Plan
        {
            public readonly List<Fix> fixes = new List<Fix>();
            public readonly List<Lit> lights = new List<Lit>();
            public readonly Dictionary<string, double> roomCeil = new Dictionary<string, double>(); // the y the room's ceiling and strips sit under
            public double levelCeil; // RED: the one level-wide ceiling
        }

        public sealed class Room { public string id, kind; public double x0, z0, x1, z1, height, sx, sz, ex, ey, ez; public bool roof; }
        public sealed class Door { public string id, a, b; public double x0, z0, x1, z1, height; }
        public sealed class Pit { public double x0, z0, x1, z1, floorY; public bool lava; }
        public sealed class Socket { public string id, room, kind, color; public double x, y, z, nx, ny, nz, w, h, d, range, intensity; }

        public sealed class Level
        {
            public string name;
            public readonly List<Room> rooms = new List<Room>();
            public readonly List<Door> doors = new List<Door>();
            public readonly List<Pit> pits = new List<Pit>();
            public readonly List<Socket> sockets = new List<Socket>();
            public readonly List<Box> solid = new List<Box>(); // collision boxes: floors, walls, roofs, lintels, ledges
            public readonly List<double[]> avoid = new List<double[]>(); // room spawns and pickups (look30's shafts keep clear of them)
        }

        /// <summary>The numbers the plan needs (config rows + UnityTuning), filled once by the caller.</summary>
        public sealed class Nums
        {
            public double trimW = 0.5, trimH = 0.12, trimInset = 0.6, roomLightH = 7, guideH = 3, roomRange = 38, roomIntensity = 6, guideRange = 16, guideIntensity = 12;
            public double stripW = 0.25, ceilingAbove = 0.5, maxWallDepth = 0.15, trimBright = 2.5, stripBright = 2.2;
            public int stripMin = 6, stripMax = 10;
            public double ceilFixtureMaxDepth = 0.05, fixW = 2, fixH = 0.12, fixD = 0.1;
            public double beamPoolHeight = 6, beamLength = 5, beamTilt = 0.6, beamNarrow = 0.35, beamSpread = 2.2, beamMaxWidth = 2.2, beamKeepClear = 5;
            public bool look30 = true;
            public bool lights35; // NIGHT35 V: fixtures only where a surface holds them (the views set it from design.n35.lights35 through Lights35On)

            public static Nums From(GameConfig cfg)
            {
                var n = new Nums();
                if (cfg == null) return n;
                double D(string p, double d) { try { return cfg.Num(p); } catch { return d; } }
                n.trimW = D("design.n24.lighting.trim.width", n.trimW); n.trimH = D("design.n24.lighting.trim.height", n.trimH); n.trimInset = D("design.n24.lighting.trim.inset", n.trimInset);
                n.trimBright = D("design.n24.lighting.trim.brightness", n.trimBright); n.stripBright = D("design.n27.roof27.stripBrightness", n.stripBright);
                n.roomLightH = D("design.n24.lighting.roomLight.height", n.roomLightH); n.guideH = D("design.n24.lighting.guide.height", n.guideH);
                n.roomRange = D("design.n24.lighting.roomLight.range", n.roomRange); n.roomIntensity = D("design.n24.lighting.roomLight.intensity", n.roomIntensity);
                n.guideRange = D("design.n24.lighting.guide.range", n.guideRange); n.guideIntensity = D("design.n24.lighting.guide.intensity", n.guideIntensity);
                n.stripW = D("design.n27.roof27.stripWidth", n.stripW); n.ceilingAbove = D("design.n27.roof27.ceilingAbove", n.ceilingAbove);
                n.stripMin = (int)D("design.n27.roof27.stripMin", n.stripMin); n.stripMax = (int)D("design.n27.roof27.stripMax", n.stripMax);
                n.maxWallDepth = D("design.n29.lights.maxWallDepth", n.maxWallDepth);
                return n;
            }
        }

        /// <summary>The five campaign themes (L1's post table rows; UnityTuning.Look33Theme* arrays are in this order).</summary>
        public static readonly string[] Themes = { "foundry", "iceline", "kiln", "span", "spire" };

        /// <summary>The theme of a map JSON by its name ("n32-kiln" -> 2); -1 when none matches.</summary>
        public static int Theme(Dictionary<string, object> json)
        {
            string nm = (json != null && json.TryGetValue("name", out object o) ? o as string : null) ?? "";
            nm = nm.ToLowerInvariant();
            for (int i = 0; i < Themes.Length; i++) if (nm.Contains(Themes[i])) return i;
            return -1;
        }

        static double N(object o, double d = 0) => o == null ? d : PortData.Num(o);
        static object G(Dictionary<string, object> m, string k) => m != null && m.TryGetValue(k, out object v) ? v : null;
        static List<object> L(object o) => o as List<object>;

        /// <summary>Reads a campaign level's JSON (the map file). Null when it is not a campaign level.</summary>
        public static Level Read(Dictionary<string, object> json, string name = "")
        {
            if (json == null || !(G(json, "campaign") is Dictionary<string, object> c)) return null;
            var lv = new Level { name = name };
            double wallH = N(G(c, "wallHeight"), 14);
            if (L(G(c, "rooms")) is List<object> rooms)
                foreach (object o in rooms)
                {
                    if (!(o is Dictionary<string, object> r) || !(L(G(r, "rect")) is List<object> rc) || rc.Count < 4) continue;
                    var rm = new Room { id = G(r, "id") as string ?? "", kind = G(r, "kind") as string ?? "room", x0 = N(rc[0]), z0 = N(rc[1]), x1 = N(rc[2]), z1 = N(rc[3]), height = N(G(r, "height"), wallH), roof = G(r, "roof") is bool b && b };
                    if (G(r, "spawn") is Dictionary<string, object> sp && L(G(sp, "at")) is List<object> sa && sa.Count >= 3) { rm.sx = N(sa[0]); rm.sz = N(sa[2]); lv.avoid.Add(new[] { rm.sx, rm.sz }); }
                    if (L(G(r, "exit")) is List<object> ex && ex.Count >= 3) { rm.ex = N(ex[0]); rm.ey = N(ex[1]); rm.ez = N(ex[2]); }
                    lv.rooms.Add(rm);
                }
            if (L(G(c, "doors")) is List<object> doors)
                foreach (object o in doors)
                {
                    if (!(o is Dictionary<string, object> d) || !(L(G(d, "rect")) is List<object> rc) || rc.Count < 4) continue;
                    var bt = L(G(d, "between"));
                    lv.doors.Add(new Door { id = G(d, "id") as string ?? "", a = bt != null && bt.Count > 0 ? bt[0] as string : null, b = bt != null && bt.Count > 1 ? bt[1] as string : null, x0 = N(rc[0]), z0 = N(rc[1]), x1 = N(rc[2]), z1 = N(rc[3]), height = N(G(d, "height"), -1) });
                }
            if (L(G(c, "pits")) is List<object> pits)
                foreach (object o in pits)
                {
                    if (!(o is Dictionary<string, object> p) || !(L(G(p, "rect")) is List<object> rc) || rc.Count < 4) continue;
                    lv.pits.Add(new Pit { x0 = N(rc[0]), z0 = N(rc[1]), x1 = N(rc[2]), z1 = N(rc[3]), floorY = N(G(p, "floorY"), -0.6), lava = G(p, "lava") is bool lb && lb });
                }
            if (L(G(c, "lights")) is List<object> lights)
                foreach (object o in lights)
                {
                    if (!(o is Dictionary<string, object> r) || !(L(G(r, "at")) is List<object> at) || at.Count < 3) continue;
                    string kind = G(r, "kind") as string ?? "wall";
                    var s = new Socket { id = G(r, "id") as string ?? "", room = G(r, "room") as string ?? "", kind = kind, color = G(r, "color") as string ?? "cyan", x = N(at[0]), y = N(at[1]), z = N(at[2]), range = N(G(r, "range"), 8), intensity = N(G(r, "intensity"), 1) };
                    if (L(G(r, "normal")) is List<object> nl && nl.Count >= 3) { s.nx = N(nl[0]); s.ny = N(nl[1]); s.nz = N(nl[2]); }
                    if (s.nx * s.nx + s.ny * s.ny + s.nz * s.nz < 1e-6) { s.nx = 0; s.ny = kind == "ceiling" ? -1 : 1; s.nz = kind == "wall" ? -1 : 0; } // Unity's forward is web -z
                    double nn = Math.Sqrt(s.nx * s.nx + s.ny * s.ny + s.nz * s.nz); s.nx /= nn; s.ny /= nn; s.nz /= nn;
                    s.w = -1; s.h = -1; s.d = -1;
                    if (L(G(r, "size")) is List<object> sz && sz.Count >= 3) { s.w = N(sz[0]); s.h = N(sz[1]); s.d = N(sz[2]); }
                    lv.sockets.Add(s);
                }
            if (L(G(c, "props")) is List<object> props)
                foreach (object o in props)
                    if (o is Dictionary<string, object> p && G(p, "pickup") is bool pk && pk && L(G(p, "at")) is List<object> pa && pa.Count >= 3) lv.avoid.Add(new[] { N(pa[0]), N(pa[2]) });
            if (G(json, "collision") is Dictionary<string, object> col && L(G(col, "boxes")) is List<object> bx)
                foreach (object o in bx)
                    if (o is Dictionary<string, object> b && L(G(b, "min")) is List<object> mn && L(G(b, "max")) is List<object> mx && mn.Count >= 3 && mx.Count >= 3)
                        lv.solid.Add(new Box(N(mn[0]), N(mn[1]), N(mn[2]), N(mx[0]), N(mx[1]), N(mx[2])));
            return lv;
        }

        static Room RoomOf(Level lv, string id) => lv.rooms.Find(r => r.id == id);

        static bool InPit(Level lv, double x, double z, bool lavaOnly, out Pit pit)
        {
            foreach (Pit p in lv.pits)
                if ((!lavaOnly || p.lava) && x >= p.x0 && x <= p.x1 && z >= p.z0 && z <= p.z1) { pit = p; return true; }
            pit = null;
            return false;
        }

        // the fixture's box from a socket: its back face on the surface point, `d` out along the normal; w along the surface, h up (a ceiling: w along x, h along z)
        static Box SocketBox(Socket s, double x, double y, double z, double w, double h, double d)
        {
            double cx = x + s.nx * d * 0.5, cy = y + s.ny * d * 0.5, cz = z + s.nz * d * 0.5;
            if (Math.Abs(s.ny) > 0.9) return Box.Around(cx, cy, cz, w * 0.5, d * 0.5, h * 0.5);
            if (Math.Abs(s.nx) >= Math.Abs(s.nz)) return Box.Around(cx, cy, cz, d * 0.5, h * 0.5, w * 0.5); // a wall facing +-x: w runs along z
            return Box.Around(cx, cy, cz, w * 0.5, h * 0.5, d * 0.5);
        }

        static void SocketSize(Socket s, Nums n, out double w, out double h, out double d)
        {
            w = s.w > 0 ? s.w : n.fixW; h = s.h > 0 ? s.h : n.fixH; d = s.d > 0 ? s.d : n.fixD;
            if (s.kind == "pool") d = 0;
            else d = Math.Max(0.005, Math.Min(d, s.kind == "ceiling" ? n.ceilFixtureMaxDepth : n.maxWallDepth));
        }

        /// <summary>The socket point pushed back along -normal onto the nearest solid face within Look33SnapReach (flush), or as it is.</summary>
        public static void Snap(Level lv, Socket s, out double x, out double y, out double z)
        {
            x = s.x; y = s.y; z = s.z;
            double best = UnityTuning.Look33SnapReach + 1e-9;
            foreach (Box b in lv.solid)
            {
                double t;
                // the face on the side the normal points away from: n = +x -> the box's x1 face, the point in front of it
                if (Math.Abs(s.nx) > 0.9) { double f = s.nx > 0 ? b.x1 : b.x0; t = (s.x - f) * Math.Sign(s.nx); if (s.y < b.y0 - 0.01 || s.y > b.y1 + 0.01 || s.z < b.z0 - 0.01 || s.z > b.z1 + 0.01) continue; if (t >= -0.01 && t < best) { best = t; x = f; y = s.y; z = s.z; } }
                else if (Math.Abs(s.ny) > 0.9) { double f = s.ny > 0 ? b.y1 : b.y0; t = (s.y - f) * Math.Sign(s.ny); if (s.x < b.x0 - 0.01 || s.x > b.x1 + 0.01 || s.z < b.z0 - 0.01 || s.z > b.z1 + 0.01) continue; if (t >= -0.01 && t < best) { best = t; x = s.x; y = f; z = s.z; } }
                else if (Math.Abs(s.nz) > 0.9) { double f = s.nz > 0 ? b.z1 : b.z0; t = (s.z - f) * Math.Sign(s.nz); if (s.x < b.x0 - 0.01 || s.x > b.x1 + 0.01 || s.y < b.y0 - 0.01 || s.y > b.y1 + 0.01) continue; if (t >= -0.01 && t < best) { best = t; x = s.x; y = s.y; z = f; } }
            }
        }

        /// <summary>The roof strips of one room under a ceiling at `y` (RoofView27's layout: n segments round the perimeter inset 0.4 m, 55 % of each slot).
        /// Each segment piece is a flat bar box from y - depth to y. The two nearest the exit are cyan, the rest the boundary's pale violet-white.</summary>
        public static void RoofStrips(Room r, Nums n, double y, double depth, List<Fix> into, List<Lit> lights, bool lightUnderPiece = true)
        {
            double w = r.x1 - r.x0, d = r.z1 - r.z0, inset = 0.4, iw = w - 2 * inset, id = d - 2 * inset;
            if (iw <= 0 || id <= 0) return;
            int cnt = (int)Math.Round(2 * (w + d) / 6.0); // SkateMath27.StripCount(per, 6, min, max), inlined (that file is not in the harness)
            cnt = cnt < n.stripMin ? n.stripMin : cnt > n.stripMax ? n.stripMax : cnt;
            double ip = 2 * (iw + id);
            var mids = new List<double[]>();
            for (int i = 0; i < cnt; i++) { Perim(r, inset, iw, id, ip * (i + 0.5) / cnt, out double mx, out double mz); mids.Add(new[] { mx, mz }); }
            var order = new List<int>();
            for (int i = 0; i < cnt; i++) order.Add(i);
            order.Sort((p, q) => Sq(mids[p][0] - r.ex, mids[p][1] - r.ez).CompareTo(Sq(mids[q][0] - r.ex, mids[q][1] - r.ez)));
            for (int i = 0; i < cnt; i++)
            {
                string col = order.IndexOf(i) < 2 ? "cyan" : "white";
                double a0 = ip * (i + 0.225) / cnt, a1 = ip * (i + 0.775) / cnt;
                int pieces = Math.Max(1, (int)Math.Ceiling((a1 - a0) / 2.0));
                double lx = mids[i][0], lz = mids[i][1];
                for (int pc = 0; pc < pieces; pc++)
                {
                    Perim(r, inset, iw, id, a0 + (a1 - a0) * pc / pieces, out double x0, out double z0);
                    Perim(r, inset, iw, id, a0 + (a1 - a0) * (pc + 1) / pieces, out double x1, out double z1);
                    double len = Math.Sqrt(Sq(x1 - x0, z1 - z0));
                    if (len < 1e-4) continue;
                    double hw = n.stripW * 0.5;
                    var b = Math.Abs(x1 - x0) >= Math.Abs(z1 - z0) ? new Box(x0, y - depth, z0 - hw, x1, y, z0 + hw) : new Box(x0 - hw, y - depth, z0, x0 + hw, y, z1);
                    into.Add(new Fix { kind = "roofStrip", type = "roofStrip", room = r.id, color = col, box = b, emissive = true, bar = true, hdr = n.stripBright });
                    if (lightUnderPiece && pc == pieces / 2) { lx = (x0 + x1) * 0.5; lz = (z0 + z1) * 0.5; } // the light under the middle piece (a slot's middle can fall on a corner the strip cuts)
                }
                lights?.Add(new Lit { kind = "roof", room = r.id, color = col, x = lx, y = y - depth - 0.4, z = lz, range = 10, intensity = 1 });
            }
        }

        static double Sq(double a, double b) => a * a + b * b;

        static void Perim(Room r, double inset, double iw, double id, double t, out double x, out double z)
        {
            double x0 = r.x0 + inset, z0 = r.z0 + inset, ip = 2 * (iw + id);
            t = ((t % ip) + ip) % ip;
            if (t < iw) { x = x0 + t; z = z0; }
            else if (t < iw + id) { x = x0 + iw; z = z0 + (t - iw); }
            else if (t < 2 * iw + id) { x = x0 + iw - (t - iw - id); z = z0 + id; }
            else { x = x0; z = z0 + id - (t - 2 * iw - id); }
        }

        static Fix Trim(string room, string color, Box b, double hdr = 2.5) => new Fix { kind = "trim", type = "trim", room = room, color = color, box = b, emissive = true, bar = true, hdr = hdr };

        // ============================================================ RED ============================================================

        /// <summary>NIGHT32's placement (the code as it stands with look33 off), for the check.</summary>
        public static Plan Red(Level lv, Nums n)
        {
            var p = new Plan();
            double maxY = 0;
            foreach (Box b in lv.solid) maxY = Math.Max(maxY, b.y1);
            p.levelCeil = maxY + n.ceilingAbove;
            for (int ri = 0; ri < lv.rooms.Count; ri++)
            {
                Room r = lv.rooms[ri];
                p.roomCeil[r.id] = p.levelCeil;
                if (r.x1 <= r.x0 || r.z1 <= r.z0) continue;
                double x0 = r.x0 + n.trimInset, x1 = r.x1 - n.trimInset, z0 = r.z0 + n.trimInset, z1 = r.z1 - n.trimInset, hw = n.trimW * 0.5;
                string acc = "accent" + (ri % 3);
                p.fixes.Add(Trim(r.id, acc, new Box(x0, 0, z0 - hw, x1, n.trimH, z0 + hw)));
                p.fixes.Add(Trim(r.id, acc, new Box(x0, 0, z1 - hw, x1, n.trimH, z1 + hw)));
                p.fixes.Add(Trim(r.id, acc, new Box(x0 - hw, 0, z0, x0 + hw, n.trimH, z1)));
                p.fixes.Add(Trim(r.id, acc, new Box(x1 - hw, 0, z0, x1 + hw, n.trimH, z1)));
                p.lights.Add(new Lit { kind = "room", room = r.id, color = acc, x = (r.x0 + r.x1) * 0.5, y = n.roomLightH, z = (r.z0 + r.z1) * 0.5, range = n.roomRange, intensity = n.roomIntensity });
                p.lights.Add(new Lit { kind = "guide", room = r.id, color = "cyan", x = r.ex, y = r.ey + n.guideH, z = r.ez, range = n.guideRange, intensity = n.guideIntensity });
                RoofStrips(r, n, p.levelCeil - 0.03, 0, p.fixes, p.lights, false);
            }
            foreach (Socket s in lv.sockets)
            {
                SocketSize(s, n, out double w, out double h, out double d);
                bool accent = s.color == "amber" || s.color == "orange";
                if (s.kind != "pool") p.fixes.Add(new Fix { kind = "socket", id = s.id, room = s.room, color = s.color, box = SocketBox(s, s.x, s.y, s.z, w, h, d), emissive = true, bar = true });
                double off = s.kind == "pool" ? 1.5 : 0.6;
                p.lights.Add(new Lit { kind = s.kind == "pool" ? "pool" : "socket", id = s.id, room = s.room, color = s.color, accent = accent, x = s.x + s.nx * off, y = s.y + s.ny * off, z = s.z + s.nz * off, range = s.range, intensity = s.intensity });
                if (!n.look30 || (s.kind != "pool" && !accent) || NearAvoid(lv, s.x, s.z, n.beamKeepClear)) continue;
                if (s.kind == "pool")
                    p.fixes.Add(Beam(s, s.x, s.y + n.beamPoolHeight, s.z, s.x, s.y + 0.02, s.z, n.beamNarrow, Math.Max(n.beamNarrow, Math.Min(Math.Max(w, n.beamNarrow), n.beamMaxWidth))));
                else AccentBeam(p, s, n, s.x, s.y, s.z, w);
            }
            return p;
        }

        static bool NearAvoid(Level lv, double x, double z, double r)
        {
            foreach (double[] a in lv.avoid) if (Sq(a[0] - x, a[1] - z) < r * r) return true;
            return false;
        }

        static Fix Beam(Socket s, double tx, double ty, double tz, double bx, double by, double bz, double wTop, double wBottom)
        {
            return new Fix { kind = "beam", id = s.id, room = s.room, color = s.color, emissive = true, tx = tx, ty = ty, tz = tz, bx = bx, by = by, bz = bz, topW = wTop, bottomW = wBottom, box = new Box(tx, ty, tz, bx, by, bz) };
        }

        static void AccentBeam(Plan p, Socket s, Nums n, double x, double y, double z, double w)
        {
            double dx = s.nx, dy = s.ny - n.beamTilt, dz = s.nz, dl = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            double tx = x + s.nx * 0.05, ty = y + s.ny * 0.05, tz = z + s.nz * 0.05;
            p.fixes.Add(Beam(s, tx, ty, tz, tx + dx / dl * n.beamLength, ty + dy / dl * n.beamLength, tz + dz / dl * n.beamLength, Math.Max(n.beamNarrow, w * 0.5), n.beamSpread));
        }

        // ============================================================ GREEN ============================================================

        /// <summary>look33's placement: every light under or in front of a fixture that touches the level (see the file header).</summary>
        public static Plan Green(Level lv, Nums n)
        {
            var p = new Plan();
            double maxY = 0;
            foreach (Box b in lv.solid) maxY = Math.Max(maxY, b.y1);
            p.levelCeil = maxY + n.ceilingAbove;
            for (int ri = 0; ri < lv.rooms.Count; ri++)
            {
                Room r = lv.rooms[ri];
                double cy = r.roof ? r.height : p.levelCeil;
                p.roomCeil[r.id] = cy;
                if (r.x1 <= r.x0 || r.z1 <= r.z0) continue;
                string acc = "accent" + (ri % 3);
                // the floor trims, cut at every pit and lava edge (L1: never over the pit)
                double x0 = r.x0 + n.trimInset, x1 = r.x1 - n.trimInset, z0 = r.z0 + n.trimInset, z1 = r.z1 - n.trimInset, hw = n.trimW * 0.5;
                foreach (double[] seg in Cut(lv, true, z0, x0, x1, hw)) p.fixes.Add(Trim(r.id, acc, new Box(seg[0], 0, z0 - hw, seg[1], n.trimH, z0 + hw), n.trimBright));
                foreach (double[] seg in Cut(lv, true, z1, x0, x1, hw)) p.fixes.Add(Trim(r.id, acc, new Box(seg[0], 0, z1 - hw, seg[1], n.trimH, z1 + hw), n.trimBright));
                foreach (double[] seg in Cut(lv, false, x0, z0, z1, hw)) p.fixes.Add(Trim(r.id, acc, new Box(x0 - hw, 0, seg[0], x0 + hw, n.trimH, seg[1]), n.trimBright));
                foreach (double[] seg in Cut(lv, false, x1, z0, z1, hw)) p.fixes.Add(Trim(r.id, acc, new Box(x1 - hw, 0, seg[0], x1 + hw, n.trimH, seg[1]), n.trimBright));
                // the room's key: a flush framed panel at the middle of its own ceiling, the light just under it
                double mx = (r.x0 + r.x1) * 0.5, mz = (r.z0 + r.z1) * 0.5;
                bool open35 = n.lights35 && !r.roof; // NIGHT35 lights35: no roof, so nothing hangs from a ceiling here
                if (open35)
                {
                    // the key is a warm flood lamp on the room's nearest wall, under the wall's top (no wall face: no key fixture; the view keeps its light)
                    var probe = new Socket { id = "key-" + r.id, room = r.id, kind = "pool", color = "warm", x = mx, z = mz, range = n.roomRange, intensity = n.roomIntensity };
                    if (WallAt35(lv, r, probe, UnityTuning.Look35KeyYs, UnityTuning.Look35FloodW, UnityTuning.Look35FloodH, out Socket ks, out double kx, out double ky, out double kz))
                    {
                        double kd = Fixture(p, ks, "flood", kx, ky, kz, ks.w, ks.h, ks.d);
                        double ko = kd + UnityTuning.Look33LightOff;
                        p.lights.Add(new Lit { kind = "room", type = "flood", room = r.id, color = "warm", x = kx + ks.nx * ko, y = ky + ks.ny * ko, z = kz + ks.nz * ko, range = n.roomRange, intensity = n.roomIntensity });
                    }
                }
                // the lead (11:57, n33-r5): a ceiling panel is lighting, not a usable thing: warm white, never cyan (cyan = player-usable only)
                else
                {
                    Panel(p, r.id, "warm", "panel", mx, cy, mz, UnityTuning.Look33PanelSize);
                    p.lights.Add(new Lit { kind = "room", room = r.id, color = "warm", x = mx, y = cy - UnityTuning.Look33PanelDepth - UnityTuning.Look33LightOff, z = mz, range = n.roomRange, intensity = n.roomIntensity });
                }
                // the guide: a lamp under the lintel of the door this room leaves by (the door shared with the next room), else under the ceiling over the exit
                Door door = null;
                Room next = ri + 1 < lv.rooms.Count ? lv.rooms[ri + 1] : null;
                foreach (Door dd in lv.doors) if ((dd.a == r.id && (next == null || dd.b == next.id)) || (dd.b == r.id && next != null && dd.a == next.id)) { door = dd; break; }
                double gx = r.ex, gz = r.ez, gy = cy;
                bool alongX = true;
                if (door != null)
                {
                    gx = (door.x0 + door.x1) * 0.5; gz = (door.z0 + door.z1) * 0.5;
                    gy = door.height > 0 ? door.height : Math.Min(cy, next != null && next.roof ? next.height : cy);
                    alongX = door.x1 - door.x0 >= door.z1 - door.z0;
                }
                double lw = UnityTuning.Look33GuideLampW * 0.5, ld = UnityTuning.Look33GuideLampD * 0.5;
                // NIGHT35 lights35: the guide lamp hangs under a real lintel or a real roof only (an open door between open rooms has nothing over it)
                bool guideHeld = !n.lights35 || (door != null && door.height > 0) || (door == null ? r.roof : r.roof && (next == null || next.roof));
                if (guideHeld)
                {
                    p.fixes.Add(new Fix { kind = "guideLamp", type = "guide", hdr = UnityTuning.Look33LampHdr, room = r.id, color = "cyan", emissive = true, box = alongX ? new Box(gx - lw, gy - UnityTuning.Look33GuideLampH, gz - ld, gx + lw, gy, gz + ld) : new Box(gx - ld, gy - UnityTuning.Look33GuideLampH, gz - lw, gx + ld, gy, gz + lw) });
                    p.lights.Add(new Lit { kind = "guide", room = r.id, color = "cyan", x = gx, y = gy - UnityTuning.Look33GuideLampH - UnityTuning.Look33LightOff, z = gz, range = n.guideRange, intensity = n.guideIntensity });
                }
                // NIGHT35 lights35: no roof strips (the white bars along the wall tops read as floating in the sky: the ceiling is a flat sky-like violet), and
                // dark joists across a roofed room's ceiling so it reads as a roof the key panel hangs from
                if (!n.lights35) RoofStrips(r, n, cy - UnityTuning.Look33CeilLift, UnityTuning.Look33StripLift, p.fixes, p.lights);
                else if (r.roof) Joists35(p, r, cy);
            }
            foreach (Socket s0 in lv.sockets)
            {
                Socket s = s0;
                SocketSize(s, n, out double w, out double h, out double d);
                bool accent = s.color == "amber" || s.color == "orange";
                Room r = RoomOf(lv, s.room);
                double cy = r != null && p.roomCeil.TryGetValue(r.id, out double c) ? c : p.levelCeil;
                // round 2: a danger (orange) pool off the lava becomes a brazier on the room's nearest wall (L1: a cage on a wall; never a floor obstacle in the run)
                if (s.kind == "pool" && s.color == "orange" && !InPit(lv, s.x, s.z, true, out _) && r != null && WallSocket(lv, r, s, out Socket ws))
                {
                    s = ws;
                    SocketSize(s, n, out w, out h, out d);
                }
                if (n.lights35 && r != null && (s.kind == "pool" || s.kind == "ceiling") && !InPit(lv, s.x, s.z, true, out _))
                {
                    // NIGHT35 lights35: a floor pool or ceiling socket is a warm sconce on its nearest wall (fewer, stronger lights: the room's one ceiling fixture is its
                    // key panel; the many small framed downlights read as "little displays"; no shaft). No wall there: the socket is dropped.
                    var probe = new Socket { id = s.id, room = s.room, kind = "pool", color = "warm", x = s.x, z = s.z, range = s.range, intensity = s.intensity };
                    if (WallAt35(lv, r, probe, UnityTuning.Look35SconceYs, UnityTuning.Look33SconceW, UnityTuning.Look33SconceH, out Socket ws35, out double wx, out double wy, out double wz))
                    {
                        double wd = Fixture(p, ws35, "sconce", wx, wy, wz, ws35.w, ws35.h, ws35.d);
                        double wo = wd + UnityTuning.Look33LightOff;
                        Look33Light("sconce", out double wr, out double wi);
                        p.lights.Add(new Lit { kind = "socket", type = "sconce", id = s.id, room = s.room, color = "warm", x = wx + ws35.nx * wo, y = wy + ws35.ny * wo, z = wz + ws35.nz * wo, range = wr, intensity = wi });
                    }
                    continue;
                }
                if (s.kind == "pool")
                {
                    if (InPit(lv, s.x, s.z, true, out Pit lava))
                    {
                        // a pool over lava: the lava is the fixture; a low local light (L1 lava row), no shaft
                        p.lights.Add(new Lit { kind = "lava", type = "lava", id = s.id, room = s.room, color = "orange", x = s.x, y = lava.floorY + UnityTuning.Look33LavaLightUp, z = s.z, range = UnityTuning.Look33LavaLightRange, intensity = UnityTuning.Look33LavaLightIntensity, accent = true });
                        continue;
                    }
                    // a floor pool: a framed downlight on the ceiling straight over it, the light under it, the shaft hanging from it
                    Panel(p, s.room, "warm", "poolPanel", s.x, cy, s.z, UnityTuning.Look33PoolPanelSize, s.id);
                    p.lights.Add(new Lit { kind = "pool", type = "panel", id = s.id, room = s.room, color = "warm", accent = accent, x = s.x, y = cy - UnityTuning.Look33PanelDepth - UnityTuning.Look33LightOff, z = s.z, range = UnityTuning.Look33PanelRange, intensity = UnityTuning.Look33PanelIntensity });
                    if (n.look30 && !NearAvoid(lv, s.x, s.z, n.beamKeepClear))
                    {
                        Fix shaft = Beam(s, s.x, cy - UnityTuning.Look33PanelDepth, s.z, s.x, s.y + 0.02, s.z, n.beamNarrow, Math.Max(n.beamNarrow, Math.Min(Math.Max(w, n.beamNarrow), n.beamMaxWidth)));
                        shaft.color = "warm"; // the shaft is its warm panel's light, not a cyan marker
                        p.fixes.Add(shaft);
                    }
                    continue;
                }
                Snap(lv, s, out double sx, out double sy, out double sz);
                if (s.kind == "ceiling") sy = Math.Max(sy, cy); // a ceiling socket hung 0.05 under the roof goes up flush
                // round 2: the socket becomes one of L1's fixture types (built from boxes; light parameters from L1's table)
                string type = s.kind == "ceiling" ? "panel" : s.color == "orange" ? "brazier" : s.color == "amber" || w <= UnityTuning.Look33SconceMaxW ? "sconce" : "strip"; // amber = the warm tungsten accent: a sconce (L1: cyan or amber)
                double depth = Fixture(p, s, type, sx, sy, sz, w, h, d);
                double off = depth + UnityTuning.Look33LightOff;
                Look33Light(type, out double range, out double inten);
                p.lights.Add(new Lit { kind = "socket", type = type, id = s.id, room = s.room, color = type == "panel" ? "warm" : s.color, accent = accent, x = sx + s.nx * off, y = sy + s.ny * off, z = sz + s.nz * off, range = range, intensity = inten });
                if (n.look30 && accent && !NearAvoid(lv, s.x, s.z, n.beamKeepClear)) AccentBeam(p, s, n, sx, sy, sz, w);
            }
            foreach (Room r in lv.rooms) Pole(lv, p, r);
            LavaLights(lv, p);
            // a danger brazier on the wall nearest each room's first lava pit (L1: orange kept to lava and braziers; one per room, the hazard's own lamp)
            var braz = new HashSet<string>();
            foreach (Lit l in p.lights) if (l.type == "brazier" && l.room != null) braz.Add(l.room);
            foreach (Pit pit in lv.pits)
            {
                if (!pit.lava) continue;
                double px = (pit.x0 + pit.x1) * 0.5, pz = (pit.z0 + pit.z1) * 0.5;
                Room r = lv.rooms.Find(q => px >= q.x0 && px <= q.x1 && pz >= q.z0 && pz <= q.z1);
                if (r == null || r.kind == "boss" || braz.Contains(r.id)) continue;
                var probe = new Socket { id = "brazier-" + r.id, room = r.id, kind = "pool", color = "orange", x = px, z = pz };
                if (!WallSocket(lv, r, probe, out Socket ws)) continue;
                Snap(lv, ws, out double sx, out double sy, out double sz);
                double depth = Fixture(p, ws, "brazier", sx, sy, sz, ws.w, ws.h, ws.d);
                double off = depth + UnityTuning.Look33LightOff;
                Look33Light("brazier", out double range, out double inten);
                p.lights.Add(new Lit { kind = "socket", type = "brazier", id = ws.id, room = r.id, color = "orange", accent = true, x = sx + ws.nx * off, y = sy + ws.ny * off, z = sz + ws.nz * off, range = range, intensity = inten });
                braz.Add(r.id);
            }
            return p;
        }

        /// <summary>A wall socket on the room wall nearest the point (the room rect's edge, at Look33BrazierY), facing into the room, snapped onto the wall's face; false when no
        /// wall face is there (an open edge or a door).</summary>
        static bool WallSocket(Level lv, Room r, Socket s, out Socket ws)
        {
            double[] dist = { s.x - r.x0, r.x1 - s.x, s.z - r.z0, r.z1 - s.z };
            int[] order = { 0, 1, 2, 3 };
            Array.Sort((double[])dist.Clone(), order);
            foreach (int k in order)
            {
                ws = new Socket { id = s.id, room = s.room, kind = "wall", color = s.color, y = UnityTuning.Look33BrazierY, w = UnityTuning.Look33BrazierW, h = UnityTuning.Look33BrazierH, d = UnityTuning.Look33PlateD, range = s.range, intensity = s.intensity };
                switch (k)
                {
                    case 0: ws.x = r.x0; ws.z = s.z; ws.nx = 1; break;
                    case 1: ws.x = r.x1; ws.z = s.z; ws.nx = -1; break;
                    case 2: ws.z = r.z0; ws.x = s.x; ws.nz = 1; break;
                    default: ws.z = r.z1; ws.x = s.x; ws.nz = -1; break;
                }
                Snap(lv, ws, out double sx, out double sy, out double sz);
                bool onFace = false;
                foreach (Box b in lv.solid) if (b.Dist(sx - ws.nx * 0.01, sy, sz - ws.nz * 0.01) < 1e-6 && b.y1 > UnityTuning.Look33BrazierY + 0.5) { onFace = true; break; }
                if (onFace) return true;
            }
            ws = null;
            return false;
        }

        /// <summary>NIGHT35 lights35: a wall socket on the room's wall nearest the probe point (the room rect's edges by distance), at the first height of `ys`
        /// where a solid box's face is there and reaches at least Look35WallTopGap over the fixture's top; snapped flush, facing into the room. False when
        /// no edge has such a face (open edges, doors, low walls).</summary>
        static bool WallAt35(Level lv, Room r, Socket s, double[] ys, double w, double h, out Socket ws, out double sx, out double sy, out double sz)
        {
            double[] dist = { s.x - r.x0, r.x1 - s.x, s.z - r.z0, r.z1 - s.z };
            int[] order = { 0, 1, 2, 3 };
            Array.Sort((double[])dist.Clone(), order);
            foreach (double y in ys)
                foreach (int k in order)
                {
                    ws = new Socket { id = s.id, room = s.room, kind = "wall", color = s.color, y = y, w = w, h = h, d = UnityTuning.Look33SconceD, range = s.range, intensity = s.intensity };
                    switch (k)
                    {
                        case 0: ws.x = r.x0; ws.z = s.z; ws.nx = 1; break;
                        case 1: ws.x = r.x1; ws.z = s.z; ws.nx = -1; break;
                        case 2: ws.z = r.z0; ws.x = s.x; ws.nz = 1; break;
                        default: ws.z = r.z1; ws.x = s.x; ws.nz = -1; break;
                    }
                    Snap(lv, ws, out sx, out sy, out sz);
                    foreach (Box b in lv.solid)
                        if (b.Dist(sx - ws.nx * 0.01, sy, sz - ws.nz * 0.01) < 1e-6 && b.y1 >= y + h * 0.5 + UnityTuning.Look35WallTopGap && b.y0 <= y - h * 0.5)
                            return true;
                }
            ws = null; sx = sy = sz = 0;
            return false;
        }

        /// <summary>NIGHT35 lights35: dark joists across a roofed room's ceiling (along the room's short side, wall to wall, flush under the roof at `y`), every
        /// Look35JoistGap m and placed so the room's middle (the key panel) falls between two of them.</summary>
        static void Joists35(Plan p, Room r, double y)
        {
            double w = r.x1 - r.x0, d = r.z1 - r.z0, gap = UnityTuning.Look35JoistGap, jw = UnityTuning.Look35JoistW * 0.5, jd = UnityTuning.Look35JoistD;
            bool acrossX = w <= d; // the joists span the short side, so they repeat along the long one
            double a0 = acrossX ? r.z0 : r.x0, a1 = acrossX ? r.z1 : r.x1, mid = (a0 + a1) * 0.5;
            int k0 = (int)Math.Ceiling((a0 + UnityTuning.Look35JoistEnd - (mid + gap * 0.5)) / gap);
            for (int k = k0; ; k++)
            {
                double at = mid + gap * 0.5 + k * gap;
                if (at > a1 - UnityTuning.Look35JoistEnd) break;
                if (at < a0 + UnityTuning.Look35JoistEnd) continue;
                var b = acrossX ? new Box(r.x0, y - jd, at - jw, r.x1, y, at + jw) : new Box(at - jw, y - jd, r.z0, at + jw, y, r.z1);
                p.fixes.Add(new Fix { kind = "body", type = "joist", id = "joist-" + r.id + "-" + k, room = r.id, box = b });
            }
        }

        /// <summary>L1's light row per fixture type: (range m, intensity).</summary>
        public static void Look33Light(string type, out double range, out double intensity)
        {
            switch (type)
            {
                case "panel": range = UnityTuning.Look33PanelRange; intensity = UnityTuning.Look33PanelIntensity; return;
                case "brazier": range = UnityTuning.Look33BrazierRange; intensity = UnityTuning.Look33BrazierIntensity; return;
                case "strip": range = UnityTuning.Look33StripRange; intensity = UnityTuning.Look33StripIntensity; return;
                case "pole": range = UnityTuning.Look33PoleRange; intensity = UnityTuning.Look33PoleIntensity; return;
                default: range = UnityTuning.Look33SconceRange; intensity = UnityTuning.Look33SconceIntensity; return;
            }
        }

        // a part of a wall / ceiling fixture: `du` along the surface (w axis), `dv` up the surface (h axis; on a ceiling: along z), `dd` out of the surface to the
        // part's back face; size w x h x d in those axes
        static Box Part(Socket s, double x, double y, double z, double du, double dv, double dd, double w, double h, double d)
        {
            double ux, uy, uz, vx, vy, vz;
            if (Math.Abs(s.ny) > 0.9) { ux = 1; uy = 0; uz = 0; vx = 0; vy = 0; vz = 1; }
            else { ux = s.nz; uy = 0; uz = -s.nx; vx = 0; vy = 1; vz = 0; } // cross(up, n): along the wall
            double cx = x + ux * du + vx * dv + s.nx * (dd + d * 0.5), cy = y + uy * du + vy * dv + s.ny * (dd + d * 0.5), cz = z + uz * du + vz * dv + s.nz * (dd + d * 0.5);
            double hx = Math.Abs(ux) * w * 0.5 + Math.Abs(vx) * h * 0.5 + Math.Abs(s.nx) * d * 0.5;
            double hy = Math.Abs(uy) * w * 0.5 + Math.Abs(vy) * h * 0.5 + Math.Abs(s.ny) * d * 0.5;
            double hz = Math.Abs(uz) * w * 0.5 + Math.Abs(vz) * h * 0.5 + Math.Abs(s.nz) * d * 0.5;
            return Box.Around(cx, cy, cz, hx, hy, hz);
        }

        static Fix Lens(Socket s, string type, Box b, double hdr, Box? holder, string color = null) =>
            new Fix { kind = "lens", type = type, id = s?.id, room = s?.room, color = color ?? s?.color, box = b, emissive = true, bar = true, hdr = hdr, hasHolder = holder.HasValue, holder = holder ?? default };

        static Fix Body(Socket s, string type, Box b, Box? holder) =>
            new Fix { kind = "body", type = type, id = s?.id, room = s?.room, box = b, hasHolder = holder.HasValue, holder = holder ?? default };

        /// <summary>Builds the fixture's boxes at the snapped socket; returns how far its front stands out of the surface (the light goes Look33LightOff past it).</summary>
        static double Fixture(Plan p, Socket s, string type, double x, double y, double z, double w, double h, double d)
        {
            switch (type)
            {
                case "panel":
                {
                    // flush ceiling panel: the socket's own size, at most 2 x 2 m, HDR <= 1.2, in a dark frame
                    Box b = SocketBox(s, x, y, z, Math.Min(w, UnityTuning.Look33PanelMax), Math.Min(h, UnityTuning.Look33PanelMax), d);
                    p.fixes.Add(Lens(s, type, b, UnityTuning.Look33PanelHdr, null, "warm")); // a ceiling panel is warm white whatever the socket row said (the lead 11:57)
                    Frame(p, s.room, b, y);
                    return d;
                }
                case "brazier":
                {
                    // a wall cage: back plate flush, a tray, two side bars, the ember core sitting in the tray
                    double bw = UnityTuning.Look33BrazierW, bh = UnityTuning.Look33BrazierH, bd = UnityTuning.Look33BrazierD, pl = UnityTuning.Look33PlateD;
                    Box plate = Part(s, x, y, z, 0, 0, 0, bw, bh, pl);
                    Box tray = Part(s, x, y, z, 0, -bh * 0.5 + 0.03, pl, bw, 0.06, bd);
                    p.fixes.Add(Body(s, type, plate, null));
                    p.fixes.Add(Body(s, type, tray, plate));
                    p.fixes.Add(Body(s, type, Part(s, x, y, z, -bw * 0.5 + 0.025, 0, pl, 0.05, bh, bd), plate));
                    p.fixes.Add(Body(s, type, Part(s, x, y, z, bw * 0.5 - 0.025, 0, pl, 0.05, bh, bd), plate));
                    p.fixes.Add(Lens(s, type, Part(s, x, y, z, 0, -bh * 0.5 + 0.06 + 0.12, pl + 0.03, bw * 0.6, 0.24, bd * 0.6), UnityTuning.Look33BrazierHdr, tray, "orange"));
                    return pl + bd;
                }
                case "strip":
                {
                    // a recessed strip: an emissive inset between two dark trims, in segments of at most 3 m with a 0.1 m gap
                    int segs = Math.Max(1, (int)Math.Ceiling(w / UnityTuning.Look33StripSeg));
                    double sl = w / segs, gap = UnityTuning.Look33StripGap, ih = UnityTuning.Look33StripInsetH, th = UnityTuning.Look33StripTrimH;
                    for (int i = 0; i < segs; i++)
                    {
                        double du = -w * 0.5 + sl * (i + 0.5), len = sl - gap;
                        p.fixes.Add(Body(s, type, Part(s, x, y, z, du, (ih + th) * 0.5, 0, len, th, UnityTuning.Look33StripTrimD), null));
                        p.fixes.Add(Body(s, type, Part(s, x, y, z, du, -(ih + th) * 0.5, 0, len, th, UnityTuning.Look33StripTrimD), null));
                        p.fixes.Add(Lens(s, type, Part(s, x, y, z, du, 0, 0, len, ih, UnityTuning.Look33StripInsetD), UnityTuning.Look33StripHdr, null));
                    }
                    return UnityTuning.Look33StripTrimD;
                }
                case "flood":
                {
                    // NIGHT35 lights35: an open room's key: a wide dark housing bolted flush on the wall, a warm lens on its lower front (it throws down)
                    Box body = Part(s, x, y, z, 0, 0, 0, UnityTuning.Look35FloodW, UnityTuning.Look35FloodH, UnityTuning.Look35FloodD);
                    p.fixes.Add(Body(s, type, body, null));
                    p.fixes.Add(Lens(s, type, Part(s, x, y, z, 0, -UnityTuning.Look35FloodH * 0.15, UnityTuning.Look35FloodD, UnityTuning.Look35FloodW * 0.85, UnityTuning.Look35FloodH * 0.5, UnityTuning.Look33LensD), UnityTuning.Look35FloodHdr, body, "warm"));
                    return UnityTuning.Look35FloodD + UnityTuning.Look33LensD;
                }
                default:
                {
                    // a wall sconce: a dark body bolted flush, a lens on its front
                    Box body = Part(s, x, y, z, 0, 0, 0, UnityTuning.Look33SconceW, UnityTuning.Look33SconceH, UnityTuning.Look33SconceD);
                    p.fixes.Add(Body(s, type, body, null));
                    p.fixes.Add(Lens(s, type, Part(s, x, y, z, 0, UnityTuning.Look33SconceH * 0.1, UnityTuning.Look33SconceD, UnityTuning.Look33SconceW * 0.75, UnityTuning.Look33SconceH * 0.6, UnityTuning.Look33LensD), UnityTuning.Look33SconceHdr, body));
                    return UnityTuning.Look33SconceD + UnityTuning.Look33LensD;
                }
            }
        }

        /// <summary>A pole lamp in a big room (both sides >= Look33PoleMinRoom): base plate on the floor at the room corner farthest from the entry that has floor
        /// under it (no pit, no box), the pole, the head; amber (a warm accent, never cyan: it marks nothing usable).</summary>
        static void Pole(Level lv, Plan p, Room r)
        {
            if (r.kind == "boss" || r.x1 - r.x0 < UnityTuning.Look33PoleMinRoom || r.z1 - r.z0 < UnityTuning.Look33PoleMinRoom) return;
            double ins = UnityTuning.Look33PoleInset, best = -1, bx = 0, bz = 0;
            foreach (double cx in new[] { r.x0 + ins, r.x1 - ins })
                foreach (double cz in new[] { r.z0 + ins, r.z1 - ins })
                {
                    if (InPit(lv, cx, cz, false, out _)) continue;
                    var foot = Box.Around(cx, UnityTuning.Look33PoleH * 0.5 + 0.05, cz, 0.25, UnityTuning.Look33PoleH * 0.5, 0.25);
                    bool blocked = false, floor = false;
                    foreach (Box b in lv.solid)
                    {
                        if (b.Gap(foot) <= 0 && b.y1 > 0.05) { blocked = true; break; }
                        if (cx >= b.x0 && cx <= b.x1 && cz >= b.z0 && cz <= b.z1 && Math.Abs(b.y1) < 0.01) floor = true;
                    }
                    if (blocked || !floor) continue;
                    double dd = Sq(cx - r.sx, cz - r.sz);
                    if (dd > best) { best = dd; bx = cx; bz = cz; }
                }
            if (best < 0) return;
            double bpw = UnityTuning.Look33PoleBaseW, pw = UnityTuning.Look33PoleW, hs = UnityTuning.Look33PoleHead, ph = UnityTuning.Look33PoleH;
            var plate = new Box(bx - bpw * 0.5, 0, bz - bpw * 0.5, bx + bpw * 0.5, 0.04, bz + bpw * 0.5);
            var pole = new Box(bx - pw * 0.5, 0.04, bz - pw * 0.5, bx + pw * 0.5, ph - hs, bz + pw * 0.5);
            var head = new Box(bx - hs * 0.5, ph - hs, bz - hs * 0.5, bx + hs * 0.5, ph, bz + hs * 0.5);
            p.fixes.Add(new Fix { kind = "body", type = "pole", id = "pole-" + r.id, room = r.id, box = plate });
            p.fixes.Add(new Fix { kind = "body", type = "pole", id = "pole-" + r.id, room = r.id, box = pole, hasHolder = true, holder = plate });
            p.fixes.Add(new Fix { kind = "lens", type = "pole", id = "pole-" + r.id, room = r.id, color = "amber", box = head, emissive = true, hdr = UnityTuning.Look33PoleHdr, hasHolder = true, holder = pole });
            Look33Light("pole", out double range, out double inten);
            p.lights.Add(new Lit { kind = "socket", type = "pole", id = "pole-" + r.id, room = r.id, color = "amber", x = bx, y = ph - hs * 0.5, z = bz, range = range, intensity = inten });
        }

        /// <summary>L1's lava row: one light per Look33LavaArea m2 of each lava pit (at least one), 1.2 m over its surface, spread along its long side;
        /// a pool socket over lava already counts for its pit.</summary>
        static void LavaLights(Level lv, Plan p)
        {
            int k = 0;
            foreach (Pit pit in lv.pits)
            {
                if (!pit.lava) continue;
                double w = pit.x1 - pit.x0, d = pit.z1 - pit.z0;
                int want = Math.Max(1, (int)Math.Round(w * d / UnityTuning.Look33LavaArea));
                int have = 0;
                foreach (Lit l in p.lights) if (l.kind == "lava" && l.x >= pit.x0 && l.x <= pit.x1 && l.z >= pit.z0 && l.z <= pit.z1) have++;
                string room = null;
                foreach (Room r in lv.rooms) if ((pit.x0 + pit.x1) * 0.5 >= r.x0 && (pit.x0 + pit.x1) * 0.5 <= r.x1 && (pit.z0 + pit.z1) * 0.5 >= r.z0 && (pit.z0 + pit.z1) * 0.5 <= r.z1) { room = r.id; break; }
                for (int i = have; i < want; i++)
                {
                    double t = (i + 0.5) / want;
                    double x = w >= d ? pit.x0 + w * t : (pit.x0 + pit.x1) * 0.5, z = w >= d ? (pit.z0 + pit.z1) * 0.5 : pit.z0 + d * t;
                    p.lights.Add(new Lit { kind = "lava", type = "lava", id = "lava-" + (k++), room = room, color = "orange", x = x, y = pit.floorY + UnityTuning.Look33LavaLightUp, z = z, range = UnityTuning.Look33LavaLightRange, intensity = UnityTuning.Look33LavaLightIntensity, accent = true });
                }
            }
        }

        // a flush emissive panel (size x size, Look33PanelDepth under the ceiling y) and its dark frame
        static void Panel(Plan p, string room, string color, string kind, double x, double y, double z, double size, string id = null)
        {
            double h = size * 0.5;
            var b = new Box(x - h, y - UnityTuning.Look33PanelDepth, z - h, x + h, y, z + h);
            p.fixes.Add(new Fix { kind = kind, type = "panel", hdr = UnityTuning.Look33PanelHdr, id = id, room = room, color = color, box = b, emissive = true });
            Frame(p, room, b, y);
        }

        static void Frame(Plan p, string room, Box b, double y)
        {
            double fw = UnityTuning.Look33FrameWidth, fy = y - UnityTuning.Look33FrameDepth;
            p.fixes.Add(new Fix { kind = "frame", room = room, box = new Box(b.x0 - fw, fy, b.z0 - fw, b.x1 + fw, y, b.z0) });
            p.fixes.Add(new Fix { kind = "frame", room = room, box = new Box(b.x0 - fw, fy, b.z1, b.x1 + fw, y, b.z1 + fw) });
            p.fixes.Add(new Fix { kind = "frame", room = room, box = new Box(b.x0 - fw, fy, b.z0, b.x0, y, b.z1) });
            p.fixes.Add(new Fix { kind = "frame", room = room, box = new Box(b.x1, fy, b.z0, b.x1 + fw, y, b.z1) });
        }

        /// <summary>A trim line at `at` (z when alongX, else x) from a0 to a1, minus every pit / lava rect it crosses (with Look33PitMargin); pieces under Look33TrimMinLen go.</summary>
        static List<double[]> Cut(Level lv, bool alongX, double at, double a0, double a1, double hw)
        {
            var segs = new List<double[]> { new[] { a0, a1 } };
            double m = UnityTuning.Look33PitMargin;
            foreach (Pit p in lv.pits)
            {
                double c0 = alongX ? p.z0 : p.x0, c1 = alongX ? p.z1 : p.x1; // the pit's extent across the trim
                if (at + hw < c0 - m || at - hw > c1 + m) continue;
                double s0 = (alongX ? p.x0 : p.z0) - m, s1 = (alongX ? p.x1 : p.z1) + m; // along the trim
                var next = new List<double[]>();
                foreach (double[] sg in segs)
                {
                    if (s1 <= sg[0] || s0 >= sg[1]) { next.Add(sg); continue; }
                    if (s0 > sg[0]) next.Add(new[] { sg[0], s0 });
                    if (s1 < sg[1]) next.Add(new[] { s1, sg[1] });
                }
                segs = next;
            }
            segs.RemoveAll(sg => sg[1] - sg[0] < UnityTuning.Look33TrimMinLen);
            return segs;
        }

        // ============================================================ CHECK ============================================================

        public struct Report { public int lights, noFixture, bars, barsUnmounted, overPit, overPitFloating, lava, lavaFloating, beams, beamsUnmounted, hdrOver, realMin, realMax; public double hdrMax; public string types; }

        /// <summary>The light33 numbers of a plan (see the file header).</summary>
        /// <param name="kitSlots">LightKit29's real point lights (they follow the fixtures of the room you are in)</param>
        /// <param name="lavaSlot">look33: one more real light kept for the room's lava</param>
        /// <param name="roofSlots">RoofView27's real point lights (NIGHT32: 2; look33: 0, the strips are emissive only)</param>
        public static Report Check(Level lv, Plan p, List<string> why = null, int kitSlots = 3, bool lavaSlot = false, int roofSlots = 2)
        {
            var rep = new Report { realMin = int.MaxValue };
            // the real lights at once in each room: the room light + the guide + the kit's slots (as many as the room has fixture lights) + the lava slot + the roof's
            foreach (Room r in lv.rooms)
            {
                int fx = 0, lava = 0;
                foreach (Lit l in p.lights) if (l.room == r.id) { if (l.kind == "lava") lava++; else if (l.kind == "socket" || l.kind == "pool") fx++; }
                int real = 2 + Math.Min(kitSlots, fx + (lavaSlot ? 0 : lava)) + (lavaSlot && lava > 0 ? 1 : 0) + roofSlots;
                rep.realMin = Math.Min(rep.realMin, real);
                rep.realMax = Math.Max(rep.realMax, real);
                if (real > 6 || real < 3) why?.Add($"room {r.id}: {real} real lights (L1: 3-6)");
            }
            var types = new SortedDictionary<string, int>();
            foreach (Lit l in p.lights) { string t = l.type ?? l.kind; types[t] = types.TryGetValue(t, out int c) ? c + 1 : 1; }
            rep.types = string.Join(", ", System.Linq.Enumerable.Select(types, kv => kv.Key + " " + kv.Value));
            foreach (Lit l in p.lights)
            {
                rep.lights++;
                if (l.kind == "lava")
                {
                    rep.lava++;
                    if (!InPit(lv, l.x, l.z, true, out Pit lp) || l.y - lp.floorY > UnityTuning.Look33LavaLightMaxUp + 1e-6) { rep.lavaFloating++; why?.Add($"lava light {l.id} is not on its lava"); }
                    continue;
                }
                double best = double.MaxValue;
                foreach (Fix f in p.fixes) if (f.kind != "beam") best = Math.Min(best, f.box.Dist(l.x, l.y, l.z));
                if (best > UnityTuning.Look33MaxLightToFixture) { rep.noFixture++; why?.Add($"{l.kind} light {l.id ?? l.room} at ({l.x:0.0},{l.y:0.0},{l.z:0.0}): nearest fixture {(best > 1e6 ? "none" : best.ToString("0.00"))} m"); }
                if (InPit(lv, l.x, l.z, false, out _)) { rep.overPit++; if (best > UnityTuning.Look33MaxLightToFixture) { rep.overPitFloating++; why?.Add($"{l.kind} light {l.id ?? l.room} floats over a pit / lava"); } } // a light on a ceiling fixture over a pit is mounted; one with no fixture is the hovering kind
            }
            foreach (Fix f in p.fixes)
            {
                if (!f.emissive)
                {
                    // round 2: a dark part (a body, a frame) floats too when it touches nothing
                    if (f.kind == "body" || f.kind == "frame") { rep.bars++; if (!Mounted(lv, f.box, f.hasHolder ? f.holder : (Box?)null, false)) { rep.barsUnmounted++; why?.Add($"{f.kind} {f.type} {f.id ?? f.room} floats"); } }
                    continue;
                }
                if (f.kind != "beam")
                {
                    rep.hdrMax = Math.Max(rep.hdrMax, f.hdr);
                    if (f.hdr > UnityTuning.Look33HdrMax + 1e-9) { rep.hdrOver++; why?.Add($"{f.kind} {f.type} {f.id ?? f.room}: HDR {f.hdr} over {UnityTuning.Look33HdrMax}"); }
                }
                if (f.kind == "beam")
                {
                    rep.beams++;
                    bool topped = false;
                    foreach (Fix o in p.fixes) if (o != f && o.kind != "beam" && o.box.Dist(f.tx, f.ty, f.tz) <= UnityTuning.Look33BeamTopGap) { topped = true; break; }
                    if (!topped) { rep.beamsUnmounted++; why?.Add($"shaft {f.id} top ({f.tx:0.0},{f.ty:0.0},{f.tz:0.0}) has no fixture"); }
                    continue;
                }
                rep.bars++;
                if (!Mounted(lv, f.box, f.hasHolder ? f.holder : (Box?)null)) { rep.barsUnmounted++; why?.Add($"{f.kind} {f.id ?? f.room} [{f.box.x0:0.0},{f.box.y0:0.00},{f.box.z0:0.0} .. {f.box.x1:0.0},{f.box.y1:0.00},{f.box.z1:0.0}] floats"); }
            }
            return rep;
        }

        /// <summary>Every Look33MountSample slice of the box along its longest horizontal axis is within Look33MountGap of a solid box.</summary>
        public static bool Mounted(Level lv, Box b, Box? holder = null, bool slice = true)
        {
            bool alongX = b.x1 - b.x0 >= b.z1 - b.z0;
            double a0 = alongX ? b.x0 : b.z0, a1 = alongX ? b.x1 : b.z1, step = UnityTuning.Look33MountSample;
            int n = slice ? Math.Max(1, (int)Math.Ceiling((a1 - a0) / step)) : 1; // a rigid part (a body, a frame) is held when any of it touches; a bar must be held all along
            for (int i = 0; i < n; i++)
            {
                double s0 = a0 + (a1 - a0) * i / n, s1 = a0 + (a1 - a0) * (i + 1) / n;
                // the slice's middle tenth (a slice that only grazes a neighbour's corner does not count as held)
                double m0 = slice ? s0 + (s1 - s0) * 0.45 : s0, m1 = slice ? s0 + (s1 - s0) * 0.55 : s1;
                Box sl = alongX ? new Box(m0, b.y0, b.z0, m1, b.y1, b.z1) : new Box(b.x0, b.y0, m0, b.x1, b.y1, m1);
                bool held = false;
                if (holder.HasValue && holder.Value.Gap(sl) <= UnityTuning.Look33MountGap) held = true; // a part held by its own body (a lens on a sconce)
                if (!held) foreach (Box s in lv.solid) if (s.Gap(sl) <= UnityTuning.Look33MountGap) { held = true; break; }
                if (!held) return false;
            }
            return true;
        }

        /// <summary>NIGHT35 V (lights35; the lead's n34-r5 frames f5 / f5b): fixtures hanging in the SKY: a part not held by another part whose box has no
        /// solid box beside or over it (within Look35SkyReach horizontally) reaching up to its own bottom. A ceiling panel under a real roof, a sconce on a
        /// wall, a trim on the floor, a lamp under a lintel are held; a panel, frame, roof strip or guide lamp at the level-wide ceiling of an OPEN room is not.
        /// `rooms` = how many rooms have one.</summary>
        public static int SkyFixtures(Level lv, Plan p, out int rooms)
        {
            int n = 0;
            var rs = new HashSet<string>();
            double m = UnityTuning.Look35SkyReach;
            foreach (Fix f in p.fixes)
            {
                if (f.kind == "beam" || f.hasHolder) continue;
                var probe = new Box(f.box.x0 - m, f.box.y0 - m, f.box.z0 - m, f.box.x1 + m, f.box.y1 + m, f.box.z1 + m);
                bool held = false;
                foreach (Box b in lv.solid) if (b.Gap(probe) <= 0 && b.y1 >= f.box.y0 - UnityTuning.Look35SkyDrop) { held = true; break; }
                if (held) continue;
                n++;
                rs.Add(f.room ?? "");
            }
            rooms = rs.Count;
            return n;
        }

        // ==================== NIGHT49 builder L (design.n49.lights49, a VIEW row; G2-G4): the lights in their roles ====================
        // CLAUDE.md rule 8 / DECIDE49 D-4: cyan only on a USABLE fixture (within L49UsableReachM of a jump pad, a grapple node, a ride panel or a climb
        // step of the level JSON), pale violet-white only on THE BOUNDARY (a floor / wall fixture within L49BoundaryReachM of a fall edge: a non-lava
        // pit), orange only on danger (braziers, lava: unchanged), and every other fixture the LEVEL ACCENT ("level49": the theme key, guarded against
        // the role hues by LightKit29.Named) under the bloom threshold (L49TierDim). The exit's guide lamp stays cyan (the way on: the door you use;
        // design.n24.lighting.guide's own rule) and is counted apart. Each level's landmark gets one light of its own (Lit kind "landmark"), which
        // LightKit29 keeps the room's strongest light on the landmark. Off (the row off): never called, the plan is NIGHT48's. Numbers: UnityTuning.L49.cs.

        /// <summary>lights49's surfaces of one level: the usable ones (boxes; a point is a zero-size box) and the fall edges.</summary>
        public sealed class Surfaces49
        {
            public readonly List<Box> usable = new List<Box>(), edges = new List<Box>();
            public int pads, nodes, ride, climb;
        }

        /// <summary>The per-level line of lights49 (the harness's n49lights, the reel a449): per campaign.lights row its drawn colour before / after.</summary>
        public sealed class Roles49Report
        {
            public string level;
            public int rows, rowsUsable, rowsBoundary, rowsDanger, rowsAccent, rowsUndrawn, exits, fixUsable, fixBoundary, fixDanger, fixAccent;
            public int offCyanBefore, offWhiteBefore, offCyan, offWhite; // drawn cyan on a non-usable row / drawn white on a non-boundary row
            public readonly SortedDictionary<string, int> json = new SortedDictionary<string, int>(), before = new SortedDictionary<string, int>(), after = new SortedDictionary<string, int>();
            public readonly SortedDictionary<string, int> usableByRoom = new SortedDictionary<string, int>(StringComparer.Ordinal);
            public string landmark = "none", sheetRoom;
            public double lx, ly, lz, tx, ty, tz; // the landmark light and its target (web space)
            public string landmarkRoom;
            public Surfaces49 surf;
            public readonly List<string> rowLines = new List<string>(); // per row: id room kind json -> before -> after (role)
        }

        static void Pt49(object o, List<Box> into)
        {
            if (o is Dictionary<string, object> d && d.ContainsKey("x") && d.ContainsKey("z")) { double x = N(G(d, "x")), y = N(G(d, "y")), z = N(G(d, "z")); into.Add(new Box(x, y, z, x, y, z)); }
            else if (o is List<object> a && a.Count >= 3) { double x = N(a[0]), y = N(a[1]), z = N(a[2]); into.Add(new Box(x, y, z, x, y, z)); }
        }

        /// <summary>RideTrim36.IsRidePanel's rule, Unity-free: a campaign box of kind "wall" with `ride: true`; on a level that tags none, one whose note / why
        /// names a ride panel or a ride wall.</summary>
        static bool Ride49(Dictionary<string, object> b, bool tagged)
        {
            if (!(G(b, "kind") is string k) || k != "wall") return false;
            if (G(b, "ride") is bool r && r) return true;
            if (tagged) return false;
            foreach (string key in new[] { "note", "why" })
                if (G(b, key) is string s && System.Text.RegularExpressions.Regex.IsMatch(s, @"ride (panel|wall)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) return true;
            return false;
        }

        /// <summary>The usable surfaces and fall edges of a campaign level (fields read: `pads`, `grappleNodes`, campaign.rooms[].grappleNodes,
        /// campaign.boxes `ride` / `job: "climb"`, campaign.pits `lava`). zoneParts is NOT used: its parts are the room volumes (every fixture is in one).</summary>
        public static Surfaces49 ReadSurfaces49(Dictionary<string, object> json, Level lv)
        {
            var s = new Surfaces49();
            if (L(G(json, "pads")) is List<object> pads) foreach (object o in pads) { int n0 = s.usable.Count; Pt49(o, s.usable); s.pads += s.usable.Count - n0; }
            if (L(G(json, "grappleNodes")) is List<object> gn) foreach (object o in gn) { int n0 = s.usable.Count; Pt49(o, s.usable); s.nodes += s.usable.Count - n0; }
            if (G(json, "campaign") is Dictionary<string, object> c)
            {
                if (L(G(c, "rooms")) is List<object> rooms)
                    foreach (object ro in rooms)
                        if (ro is Dictionary<string, object> r && L(G(r, "grappleNodes")) is List<object> rn) foreach (object o in rn) { int n0 = s.usable.Count; Pt49(o, s.usable); s.nodes += s.usable.Count - n0; }
                if (L(G(c, "boxes")) is List<object> boxes)
                {
                    bool tagged = false;
                    foreach (object o in boxes) if (o is Dictionary<string, object> d && G(d, "ride") is bool rt && rt) { tagged = true; break; }
                    foreach (object o in boxes)
                    {
                        if (!(o is Dictionary<string, object> b) || !(L(G(b, "min")) is List<object> mn) || !(L(G(b, "max")) is List<object> mx) || mn.Count < 3 || mx.Count < 3) continue;
                        bool ride = Ride49(b, tagged), climb = G(b, "job") is string j && j == "climb";
                        if (!ride && !climb) continue;
                        s.usable.Add(new Box(N(mn[0]), N(mn[1]), N(mn[2]), N(mx[0]), N(mx[1]), N(mx[2])));
                        if (ride) s.ride++; else s.climb++;
                    }
                }
            }
            if (lv != null) foreach (Pit p in lv.pits) if (!p.lava && p.floorY <= -UnityTuning.L49FallMinDepthM + 1e-9) s.edges.Add(new Box(p.x0, Math.Min(p.floorY, 0), p.z0, p.x1, 0, p.z1));
            return s;
        }

        /// <summary>A fixture box's role: "orange" stays danger; "cyan" when usable; "white" when a floor / wall box is on a fall edge; else "level49".</summary>
        public static string Role49(Surfaces49 s, Box b, string color, bool ceiling)
        {
            if (color == "orange") return "orange";
            foreach (Box u in s.usable) if (u.Gap(b) <= UnityTuning.L49UsableReachM + 1e-9) return "cyan";
            if (!ceiling) foreach (Box e in s.edges) if (e.Gap(b) <= UnityTuning.L49BoundaryReachM + 1e-9) return "white";
            return "level49";
        }

        static int Rank49(string role) => role == "orange" ? 3 : role == "cyan" ? 2 : role == "white" ? 1 : 0;

        /// <summary>The emissive HDR of a role's lens (UnityTuning.L49Tier* x the bloom threshold).</summary>
        public static double Hdr49(string role) => UnityTuning.L49BloomThreshold * (role == "cyan" ? UnityTuning.L49TierUsable : role == "white" ? UnityTuning.L49TierBoundary : role == "orange" ? UnityTuning.L49TierDanger : UnityTuning.L49TierDim);

        static bool CeilingFix49(Fix f) => f.kind == "panel" || f.kind == "poolPanel" || f.type == "panel" || f.kind == "roofStrip";

        /// <summary>The drawn colour of each campaign.lights row in a plan: its lens's (the first emissive non-beam fix with its id), else its light's; null = not drawn.</summary>
        static Dictionary<string, string> Drawn49(Plan p)
        {
            var d = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (Fix f in p.fixes) if (f.emissive && f.kind != "beam" && f.id != null && !d.ContainsKey(f.id)) d[f.id] = f.color ?? "cyan";
            foreach (Lit l in p.lights) if (l.id != null && !d.ContainsKey(l.id)) d[l.id] = l.color ?? "cyan";
            return d;
        }

        /// <summary>lights49 on a GREEN plan (in place): every fixture, light and shaft takes its role's colour; every lens its role's tier; the landmark
        /// light is added. Returns the report (the before colours are the plan's as it came in).</summary>
        public static Roles49Report Roles49(Dictionary<string, object> json, Level lv, Plan p)
        {
            var rep = new Roles49Report { level = lv?.name };
            Surfaces49 s = rep.surf = ReadSurfaces49(json, lv);
            Dictionary<string, string> before = Drawn49(p);
            var byId = new Dictionary<string, string>(StringComparer.Ordinal);
            // the socket's own point first (a socket with no lens: a lava pool, a dropped one), then every lens of that id (a drawn lens decides: any usable lens -> usable)
            foreach (Socket so in lv.sockets) if (!byId.ContainsKey(so.id)) byId[so.id] = Role49(s, new Box(so.x, so.y, so.z, so.x, so.y, so.z), so.color, so.kind == "ceiling");
            var lensRole = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (Fix f in p.fixes)
            {
                if (!f.emissive || f.kind == "beam" || f.id == null || f.kind == "guideLamp") continue;
                string r = Role49(s, f.box, f.color, CeilingFix49(f));
                if (!lensRole.TryGetValue(f.id, out string had) || Rank49(r) > Rank49(had)) lensRole[f.id] = r;
            }
            foreach (var kv in lensRole) byId[kv.Key] = kv.Value;
            foreach (Fix f in p.fixes)
            {
                if (!f.emissive) continue;
                string r;
                if (f.kind == "guideLamp") { r = "cyan"; rep.exits++; }
                else if (f.id != null && byId.TryGetValue(f.id, out string ir)) r = ir;
                else r = f.kind == "beam" ? (f.color == "orange" ? "orange" : "level49") : Role49(s, f.box, f.color, CeilingFix49(f));
                f.color = r;
                if (f.kind == "beam") continue;
                f.hdr = Math.Min(Hdr49(r), UnityTuning.Look33HdrMax);
                if (r == "cyan" && f.kind != "guideLamp") rep.fixUsable++; else if (r == "white") rep.fixBoundary++; else if (r == "orange") rep.fixDanger++; else if (r == "level49") rep.fixAccent++;
            }
            foreach (Lit l in p.lights)
            {
                if (l.kind == "lava" || l.kind == "room" || l.kind == "guide") continue; // the lava is danger; the key and the guide are CampaignLighting24's
                if (l.id != null && byId.TryGetValue(l.id, out string ir)) l.color = ir;
                else if (l.color != "orange") l.color = Role49(s, new Box(l.x, l.y, l.z, l.x, l.y, l.z), l.color, false);
            }
            Dictionary<string, string> after = Drawn49(p);
            // the counts per campaign.lights row (the brief's acceptance: cyan off every non-usable row 0; white off every non-boundary row 0)
            foreach (Socket so in lv.sockets)
            {
                rep.rows++;
                string role = byId.TryGetValue(so.id, out string rr) ? rr : "level49";
                Inc49(rep.json, so.color ?? "cyan");
                string b0 = before.TryGetValue(so.id, out string bc) ? bc : "undrawn", a0 = after.TryGetValue(so.id, out string ac) ? ac : "undrawn";
                Inc49(rep.before, b0);
                Inc49(rep.after, a0);
                if (a0 == "undrawn") rep.rowsUndrawn++;
                if (role == "cyan") { rep.rowsUsable++; Inc49(rep.usableByRoom, so.room ?? ""); }
                else if (role == "white") rep.rowsBoundary++;
                else if (role == "orange") rep.rowsDanger++;
                else rep.rowsAccent++;
                if (b0 == "cyan" && role != "cyan") rep.offCyanBefore++;
                if (b0 == "white" && role != "white") rep.offWhiteBefore++;
                if (a0 == "cyan" && role != "cyan") rep.offCyan++;
                if (a0 == "white" && role != "white") rep.offWhite++;
                rep.rowLines.Add($"{so.id} ({so.room}, {so.kind}) json {so.color} -> drawn {b0} -> {a0} (role {role})");
            }
            // the blind sheet's room: the non-boss room with the most usable rows (ties: the first in the level's order); none -> the first room
            int best = -1;
            foreach (Room r in lv.rooms)
            {
                if (r.kind == "boss") continue;
                int n = rep.usableByRoom.TryGetValue(r.id, out int c) ? c : 0;
                if (n > best) { best = n; rep.sheetRoom = r.id; }
            }
            Landmark49(json, lv, p, rep);
            return rep;
        }

        static void Inc49(SortedDictionary<string, int> d, string k) => d[k] = d.TryGetValue(k, out int c) ? c + 1 : 1;

        /// <summary>The level's landmark: in its boss room (the WICK's furnace, the BRAWL's floor, the KILN's ring, the LOOM's warp, the SPIRE's column),
        /// (1) a prop with `job: "landmark"` (lit from L49LandmarkPropDropM under it), else (2) the room's first `centre` / `axis` field (the boss's own
        /// arena centre: wick37.centre, brawl40.centre, spire37.axis; lit from L49LandmarkLiftM over it), else (3) the room's middle at the floor.</summary>
        static void Landmark49(Dictionary<string, object> json, Level lv, Plan p, Roles49Report rep)
        {
            if (!(G(json, "campaign") is Dictionary<string, object> c) || !(L(G(c, "rooms")) is List<object> rooms)) return;
            Dictionary<string, object> boss = null;
            foreach (object o in rooms) if (o is Dictionary<string, object> r && G(r, "kind") as string == "boss") { boss = r; break; }
            if (boss == null) return;
            string id = G(boss, "id") as string ?? "";
            Room rm = RoomOf(lv, id);
            if (rm == null) return;
            bool found = false;
            if (L(G(c, "props")) is List<object> props)
                foreach (object o in props)
                    if (o is Dictionary<string, object> pr && G(pr, "room") as string == id && G(pr, "job") as string == "landmark" && L(G(pr, "at")) is List<object> at && at.Count >= 3)
                    {
                        rep.tx = N(at[0]); rep.ty = N(at[1]); rep.tz = N(at[2]);
                        rep.lx = rep.tx; rep.ly = rep.ty - UnityTuning.L49LandmarkPropDropM; rep.lz = rep.tz;
                        rep.landmark = "prop " + (G(pr, "id") as string ?? "?");
                        found = true;
                        break;
                    }
            if (!found)
                foreach (var kv in boss)
                {
                    if (!(kv.Value is Dictionary<string, object> sub)) continue;
                    foreach (var f in sub)
                        if ((f.Key == "centre" || f.Key == "center" || f.Key == "axis") && L(f.Value) is List<object> a && a.Count >= 3)
                        {
                            rep.tx = N(a[0]); rep.ty = N(a[1]); rep.tz = N(a[2]);
                            rep.landmark = kv.Key + "." + f.Key;
                            found = true;
                            break;
                        }
                    if (found) break;
                }
            if (!found) { rep.tx = (rm.x0 + rm.x1) * 0.5; rep.ty = 0; rep.tz = (rm.z0 + rm.z1) * 0.5; rep.landmark = "room middle"; }
            if (!rep.landmark.StartsWith("prop")) { rep.lx = rep.tx; rep.ly = rep.ty + UnityTuning.L49LandmarkLiftM; rep.lz = rep.tz; }
            rep.landmarkRoom = id;
            p.lights.Add(new Lit { kind = "landmark", type = "landmark", id = "landmark49-" + id, room = id, color = "level49", x = rep.lx, y = rep.ly, z = rep.lz, range = UnityTuning.L49LandmarkRangeM, intensity = UnityTuning.L49LandmarkMinIntensity });
        }

        /// <summary>The lights49 report of a map JSON as the views would build it (a fresh GREEN plan with the config's rows, then Roles49 on it); the
        /// plan the views draw is not touched. Null when the map is not a campaign level.</summary>
        public static Roles49Report Report49(Dictionary<string, object> json, GameConfig cfg, bool look30, bool lights35, string name = "")
        {
            Level lv = Read(json, name);
            if (lv == null) return null;
            Nums n = Nums.From(cfg);
            n.look30 = look30;
            n.lights35 = lights35;
            Plan p = Green(lv, n);
            if (Rods40.On(cfg)) Rods40Filter(lv, p);
            return Roles49(json, lv, p);
        }

        /// <summary>Is lights49 on (design.n49.lights49.enabled, not covered by an off switch)?</summary>
        public static bool On49(GameConfig cfg) => cfg != null && NightRows.ViewOn(cfg, "lights49");

        /// <summary>The lights49 report of the cached plan (null with the row off or before a campaign level is built): the reel's line.</summary>
        public static Roles49Report Cached49;

        // ============================================================ the views' cache ============================================================

        static Dictionary<string, object> cachedJson;
        static Plan cachedGreen;
        static Level cachedLevel;
        static bool cached49; // NIGHT49 L: the plan was built with lights49 on

        /// <summary>NIGHT35 V: is design.n35.lights35 on (and not -skygrindOff35)? Set by the Unity side (Fixtures33's hook: this file stays free of N35Fixes
        /// for the harness); null = off.</summary>
        public static Func<GameConfig, bool> Lights35On;

        /// <summary>The GREEN plan of this map JSON (built once per map; the three views share it).</summary>
        /// <summary>NIGHT40 S1 (rods40): take out of the plan every fixture Rods40.HideFix skips (a light shaft, a free-standing pole lamp: the
        /// "white rods" GG clips through) and the pole lamps' own lights with them (every light keeps a fixture). Runs once per level (GreenFor's
        /// cache), so LightKit29's shafts and Fixtures33's bodies both read the filtered plan. Off (rods40 off): never called.</summary>
        public static int Rods40Filter(Level lv, Plan p)
        {
            var hide = new List<Fix>();
            var poles = new HashSet<string>();
            foreach (Fix f in p.fixes)
                if (Rods40.HideFix(lv, f, p))
                {
                    hide.Add(f);
                    if (f.type == "pole" && f.id != null) poles.Add(f.id);
                }
            foreach (Fix f in hide) p.fixes.Remove(f);
            p.lights.RemoveAll(l => l.type == "pole" && l.id != null && poles.Contains(l.id));
            return hide.Count;
        }

        public static Plan GreenFor(Dictionary<string, object> json, GameConfig cfg, bool look30, out Level level)
        {
            bool on49 = On49(cfg); // NIGHT49 L (lights49): a plan built with the row on is not the plan with it off
            if (!ReferenceEquals(json, cachedJson) || cachedGreen == null || on49 != cached49)
            {
                cachedJson = json;
                cached49 = on49;
                Cached49 = null;
                cachedLevel = Read(json);
                Nums n = Nums.From(cfg);
                n.look30 = look30;
                n.lights35 = Lights35On != null && Lights35On(cfg);
                cachedGreen = cachedLevel != null ? Green(cachedLevel, n) : null;
                if (cachedGreen != null && Rods40.On(cfg)) Rods40Filter(cachedLevel, cachedGreen); // NIGHT40 S1 (rods40, a view row): no rod in the play volume
                if (cachedGreen != null && on49) Cached49 = Roles49(json, cachedLevel, cachedGreen); // NIGHT49 L (lights49): the lights in their roles
            }
            level = cachedLevel;
            return cachedGreen;
        }
    }
}
