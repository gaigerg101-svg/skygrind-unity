// NIGHT33 builder K (design.n33.kit33): `kit33 <plan.json | level.json> [--room id] [--quiet]`: K1-arena-kit.md's plan checks on a level plan or a
// level file (its `campaign` block), one table row per area, RED / PASS per check, exit 1 when any area is RED. Declared and called by the lead's
// Program.cs (N33Kit). A NIGHT32 plan has no decor: it is RED by design; a plan dressed by Tools/night/dress33.cjs must be PASS.
// The checks (K1 section 8; the numbers are UnityTuning.Kit33*):
//  1 decor rows never collide and stay out of the clear lanes (3 m wide, each entrance door to the exit and to each ramp foot of the room)
//  2 decor below 3 m lies within 1.5 m of a wall or is flush (<= 0.05 m)          3 at least 40 % of decor has min y >= 3
//  4 decor per 100 m2 in 8-14 (RED below 6 or above 18; WARN between)              5 skirt and cornice on 100 % of wall length; a rib / recess /
//    pilaster / door frame at least every 8 m of wall                             6 exactly one hero landmark, outside the walkable footprint (its
//    footprint off the rect or its base >= 3 m), its centre seen from the entry's eye (1.7 m) past every plan box
//  7 every doorway of the area framed (a doorFrame row on it); no other decor in a door opening below its lintel
//  8 the enemy route to every tier: levelgen's report row 4 (not re-run here)     9 the floor stays flat: no decor raised > 0.05 m on the floor away
//    from the walls (1 + 2 cover it; a decor row that is not flush and not by a wall is counted here)
// 10 cover: every 1 m floor cell within 8 m of a cover piece (a collider prop, or a plan box of the area rising >= 1 m); rooms >= 100 m2 have 1 per 40 m2
// 11 devices: pads per 150 m2 (information: builder R's devices)               12 palette: no decor `color` cyan / usable / magenta; orange only on hazards
// 13 decor adds 0 colliders; decor rows per area <= 400                          14 no two axis-aligned decor boxes share a face plane within 0.01 m
//    while overlapping (z-fight)                                                 15 determinism: the decor rows' FNV-1a hash is printed (dress33 run twice
//    must print the same hash)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N33Kit(string cmd, string[] args, ref int code)
    {
        if (cmd != "kit33") return;
        code = Kit33(args);
    }

    sealed class Kit33Box
    {
        public string id, kind, room, color;
        public bool decor, collider, axis;
        public List<double> gaps = new List<double>(); // round 2: dress33's `gap` [lo, hi, lo, hi, ...] (the clear bands the view leaves out of the piece)
        public bool Clears(double lo, double hi) { for (int k = 0; k + 1 < gaps.Count; k += 2) if (gaps[k] <= lo + 1e-6 && gaps[k + 1] >= hi - 1e-6) return true; return false; }
        public double x0, y0, z0, x1, y1, z1;
        public double H => y1 - y0;
        public double Cx => (x0 + x1) / 2;
        public double Cz => (z0 + z1) / 2;
    }

    static readonly HashSet<string> Kit33WallPieces = new HashSet<string> { "rib", "recess", "pilaster", "doorFrame", "kilnArch", "fin", "hero" }; // the landmark is a wall relief
    static readonly HashSet<string> Kit33Legacy = new HashSet<string> { "heatVent", "checkpoint", "plinth", "slot" };
    static readonly HashSet<string> Kit33Hazards = new HashSet<string> { "hazardStripe", "lavaChannel", "heatVent" };

    static double K33N(object o) => PortData.Num(o);
    static List<object> K33L(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out object v) && v is List<object> l ? l : new List<object>();

    static Kit33Box K33Row(Dictionary<string, object> p)
    {
        var b = new Kit33Box
        {
            id = p.TryGetValue("id", out object io) ? io as string ?? "" : "",
            kind = p.TryGetValue("kind", out object ko) ? ko as string ?? "crate" : "crate",
            room = p.TryGetValue("room", out object ro) ? ro as string ?? "" : "",
            color = p.TryGetValue("color", out object co) ? co as string : null,
            decor = p.TryGetValue("decor", out object dc) && dc is bool db && db,
            collider = p.TryGetValue("collider", out object cl) && cl is bool cb && cb,
        };
        if (p.TryGetValue("gap", out object go) && go is List<object> gl) foreach (object gv in gl) b.gaps.Add(K33N(gv));
        List<object> at = PortData.Arr(p["at"]), sz = PortData.Arr(p["size"]);
        double w = K33N(sz[0]), h = K33N(sz[1]), d = K33N(sz[2]);
        double yaw = p.TryGetValue("yawDeg", out object yo) ? K33N(yo) : 0;
        double r = yaw * Math.PI / 180, c = Math.Abs(Math.Cos(r)), s = Math.Abs(Math.Sin(r));
        b.axis = c < 1e-6 || s < 1e-6;
        double ex = (w * c + d * s) / 2, ez = (w * s + d * c) / 2;
        double ax = K33N(at[0]), ay = K33N(at[1]), az = K33N(at[2]);
        b.x0 = ax - ex; b.x1 = ax + ex; b.y0 = ay - h / 2; b.y1 = ay + h / 2; b.z0 = az - ez; b.z1 = az + ez;
        return b;
    }

    /// <summary>xz distance from a point to a box (0 inside).</summary>
    static double K33Dist(double x, double z, Kit33Box b)
    {
        double dx = Math.Max(Math.Max(b.x0 - x, 0), x - b.x1), dz = Math.Max(Math.Max(b.z0 - z, 0), z - b.z1);
        return Math.Sqrt(dx * dx + dz * dz);
    }

    /// <summary>xz distance from a segment to a box (sampled every 0.25 m: the lanes are short).</summary>
    static double K33SegDist(double ax, double az, double bx, double bz, Kit33Box b)
    {
        double len = Math.Sqrt((bx - ax) * (bx - ax) + (bz - az) * (bz - az));
        int n = Math.Max(1, (int)Math.Ceiling(len / 0.25));
        double best = double.MaxValue;
        for (int i = 0; i <= n; i++) best = Math.Min(best, K33Dist(ax + (bx - ax) * i / n, az + (bz - az) * i / n, b));
        return best;
    }

    /// <summary>Does the segment p -> q pass through the box (slab test, ends excluded by 0.05 m)?</summary>
    static bool K33RayHits(double[] p, double[] q, Kit33Box b)
    {
        double t0 = 0.0, t1 = 1.0;
        double[] lo = { b.x0, b.y0, b.z0 }, hi = { b.x1, b.y1, b.z1 };
        for (int a = 0; a < 3; a++)
        {
            double d = q[a] - p[a];
            if (Math.Abs(d) < 1e-12) { if (p[a] < lo[a] || p[a] > hi[a]) return false; continue; }
            double ta = (lo[a] - p[a]) / d, tb = (hi[a] - p[a]) / d;
            if (ta > tb) (ta, tb) = (tb, ta);
            t0 = Math.Max(t0, ta); t1 = Math.Min(t1, tb);
            if (t0 > t1) return false;
        }
        double len = Math.Sqrt((q[0] - p[0]) * (q[0] - p[0]) + (q[1] - p[1]) * (q[1] - p[1]) + (q[2] - p[2]) * (q[2] - p[2]));
        return t1 > 0.05 / Math.Max(len, 1e-9) && t0 < 1 - 0.05 / Math.Max(len, 1e-9);
    }

    /// <summary>Covered length of [a, b] by the union of intervals.</summary>
    static double K33Covered(double a, double b, List<(double lo, double hi)> iv)
    {
        var s = iv.Select(t => (Math.Max(a, t.lo), Math.Min(b, t.hi))).Where(t => t.Item2 > t.Item1).OrderBy(t => t.Item1).ToList();
        double sum = 0, cur = a;
        foreach (var (lo, hi) in s) { if (hi <= cur) continue; sum += hi - Math.Max(lo, cur); cur = hi; }
        return sum;
    }

    static int Kit33(string[] args)
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        string F(double v) => v.ToString("0.#", ci);
        string path = args.Skip(1).FirstOrDefault(a => !a.StartsWith("--")); // args[0] is the command
        if (path == null || !File.Exists(path)) { Console.WriteLine("usage: kit33 <plan.json | level.json> [--room id] [--quiet]"); return 2; }
        string onlyRoom = null;
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == "--room") onlyRoom = args[i + 1];
        bool quiet = Array.IndexOf(args, "--quiet") >= 0;
        bool strict = Array.IndexOf(args, "--strict") >= 0; // cover (R's layout) counts in the verdict
        var layout = new List<string>();
        int totalObjects = 0, totalRows = 0;
        var root = PortData.Obj(PortData.LoadJson(path));
        // a level file carries the plan's rows in its `campaign` block; a plan has them at the top
        bool isLevel = !root.ContainsKey("rooms") && root.ContainsKey("campaign");
        var src = isLevel ? PortData.Obj(root["campaign"]) : root;
        double wallH = src.TryGetValue("wallHeight", out object wh) ? K33N(wh) : 14;
        var props = K33L(src, "props").Select(o => K33Row(PortData.Obj(o))).ToList();
        var boxes = new List<Kit33Box>();
        foreach (object o in K33L(src, "boxes"))
        {
            var b = PortData.Obj(o);
            List<object> mn = PortData.Arr(b["min"]), mx = PortData.Arr(b["max"]);
            boxes.Add(new Kit33Box { id = b.TryGetValue("note", out object nt) ? nt as string : "", kind = b.TryGetValue("kind", out object k) ? k as string : "hull", room = b.TryGetValue("zone", out object z) ? z as string : "", collider = true,
                x0 = K33N(mn[0]), y0 = K33N(mn[1]), z0 = K33N(mn[2]), x1 = K33N(mx[0]), y1 = K33N(mx[1]), z1 = K33N(mx[2]) });
        }
        var ramps = K33L(src, "ramps").Select(o => PortData.Obj(o)).ToList();
        var pits = K33L(src, "pits").Select(o => PortData.Obj(o)).ToList();
        // L's light sockets as boxes (LevelPlan29's axes: normal x -> [d, h, w], normal y -> [w, d, h], else [w, h, d])
        var sockets = new List<Kit33Box>();
        foreach (object o in K33L(src, "lights"))
        {
            var l = PortData.Obj(o);
            if (!l.ContainsKey("at") || !l.ContainsKey("size")) continue;
            List<object> la = PortData.Arr(l["at"]), ls = PortData.Arr(l["size"]);
            List<object> ln = l.TryGetValue("normal", out object no) && no is List<object> nl ? nl : new List<object> { 0.0, 0.0, 1.0 };
            double w = K33N(ls[0]), h = K33N(ls[1]), d = K33N(ls[2]), ex, ey, ez;
            if (Math.Abs(K33N(ln[0])) > 0.5) { ex = d; ey = h; ez = w; } else if (Math.Abs(K33N(ln[1])) > 0.5) { ex = w; ey = d; ez = h; } else { ex = w; ey = h; ez = d; }
            sockets.Add(new Kit33Box { kind = Math.Abs(K33N(ln[1])) > 0.5 ? "floorSocket" : "socket", id = l.TryGetValue("id", out object li) ? li as string : "", x0 = K33N(la[0]) - ex / 2, x1 = K33N(la[0]) + ex / 2, y0 = K33N(la[1]) - ey / 2, y1 = K33N(la[1]) + ey / 2, z0 = K33N(la[2]) - ez / 2, z1 = K33N(la[2]) + ez / 2 });
        }
        var pads = K33L(src, "pads").Select(o => PortData.Obj(o)).ToList();
        var doors = K33L(src, "doors").Select(o => PortData.Obj(o)).ToList();
        var rooms = K33L(src, "rooms").Select(o => PortData.Obj(o)).ToList();

        // 15: the decor rows' hash (determinism of the dressing pass)
        uint hash = 2166136261;
        foreach (Kit33Box p in props)
        {
            if (!p.decor) continue;
            string key = string.Join("|", p.id, p.kind, p.room, p.x0.ToString("R", CultureInfo.InvariantCulture), p.y0.ToString("R", CultureInfo.InvariantCulture), p.z0.ToString("R", CultureInfo.InvariantCulture),
                p.x1.ToString("R", CultureInfo.InvariantCulture), p.y1.ToString("R", CultureInfo.InvariantCulture), p.z1.ToString("R", CultureInfo.InvariantCulture));
            foreach (char ch in key) { hash ^= ch; hash *= 16777619; }
        }

        Console.WriteLine($"kit33: {Path.GetFileName(path)} ({(isLevel ? "level" : "plan")}): {rooms.Count} areas, {props.Count} props ({props.Count(p => p.decor)} decor, {props.Count(p => p.collider)} colliders), decor hash {hash:x8}");
        // round 2: the bands other systems own, from the config: the wall-ride strip (WallRideStrips26) and L's floor trim (LightPlan33 Trim)
        GameConfig kcfg = Config();
        double rideY = kcfg.Num("design.n26.wallRide.stripHeight"), rideTh = kcfg.Num("design.n26.wallRide.stripThickness"), rideOff = kcfg.Num("design.n26.wallRide.stripOffset");
        double rideLo = rideY - rideTh / 2, rideHi = rideY + rideTh / 2, rideFront = rideOff + rideTh / 2;
        double trimNear = kcfg.Num("design.n24.lighting.trim.inset") - kcfg.Num("design.n24.lighting.trim.width") / 2, trimH = kcfg.Num("design.n24.lighting.trim.height");
        string F2(double v) => v.ToString("0.00", ci);
        Console.WriteLine($"kit33: ride strip band y {F2(rideLo)}-{F2(rideHi)} (+-{UnityTuning.Kit33RideGap} clear), its front {F2(rideFront)} m off the wall; L's trim from {F2(trimNear)} m off the wall, {F2(trimH)} m tall");
        Console.WriteLine("| area | kind | m2 | decor | /100m2 | >=3m | wall-band | skirt | cornice | wall gap | doors framed | hero | lanes | cover reach | cover n | colliders | palette | z-fight | pads/150m2 | ride / trim | objects | verdict |");
        Console.WriteLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");
        int red = 0, graded = 0;
        var why = new List<string>();
        foreach (var room in rooms)
        {
            string id = room["id"] as string;
            if (onlyRoom != null && id != onlyRoom) continue;
            string kind = room.TryGetValue("kind", out object rk) ? rk as string : "room";
            List<object> rr = PortData.Arr(room["rect"]);
            double x0 = K33N(rr[0]), z0 = K33N(rr[1]), x1 = K33N(rr[2]), z1 = K33N(rr[3]);
            double area = (x1 - x0) * (z1 - z0);
            double roomTop = room.TryGetValue("height", out object rh) ? K33N(rh) : wallH;
            var mine = props.Where(p => p.room == id).ToList();
            var decor = mine.Where(p => p.decor && p.kind != "wash").ToList();
            // (the socket test below uses `decor`: the wash is not in it) // a wash is a wall surface (K1 5.2), not a piece: not counted, never in a lane
            int washes = mine.Count(p => p.decor && p.kind == "wash");
            var fails = new List<string>();
            bool warn = false;
            graded++;

            // 4 density
            double dens = decor.Count / area * 100;
            // (graded for areas of Kit33DensityMinArea m2 or more: a 5 x 8 m hall is all wall, its count per m2 means nothing; a small area needs
            // only some decor)
            if (area >= UnityTuning.Kit33DensityMinArea)
            {
                if (dens < UnityTuning.Kit33DensityFailLow || dens > UnityTuning.Kit33DensityFailHigh) fails.Add($"density {F(dens)}/100m2");
                else if (dens < UnityTuning.Kit33DensityMin || dens > UnityTuning.Kit33DensityMax) warn = true;
            }
            else if (decor.Count == 0) fails.Add("no decor");
            // 3 high share
            double high = decor.Count > 0 ? decor.Count(p => p.y0 >= UnityTuning.Kit33HighY - 1e-6) / (double)decor.Count : 0;
            if (high < UnityTuning.Kit33HighShare) fails.Add($"high share {high:P0}");
            // 2 / 9 low decor by a wall or flush
            double WallDist(Kit33Box b) => Math.Max(0, Math.Min(Math.Min(b.x0 - x0, x1 - b.x1), Math.Min(b.z0 - z0, z1 - b.z1)));
            int loose = decor.Count(p => p.y0 < UnityTuning.Kit33HighY && p.H > UnityTuning.Kit33FlushH && WallDist(p) > UnityTuning.Kit33WallBand);
            if (loose > 0) fails.Add($"{loose} decor raised on the open floor");
            // 5 wall coverage: the four edges minus door gaps
            var myDoors = doors.Where(d => PortData.Arr(d["between"]).Contains(id)).ToList();
            double wallLen = 0, skirtWall = 0, skirtLen = 0, cornLen = 0, worstGap = 0;
            for (int e = 0; e < 4; e++)
            {
                bool alongX = e < 2;
                double fixedC = e == 0 ? z0 : e == 1 ? z1 : e == 2 ? x0 : x1;
                double a = alongX ? x0 : z0, bnd = alongX ? x1 : z1;
                var gaps = new List<(double lo, double hi)>();
                foreach (var d in myDoors)
                {
                    List<object> dr = PortData.Arr(d["rect"]);
                    double dx0 = K33N(dr[0]), dz0 = K33N(dr[1]), dx1 = K33N(dr[2]), dz1 = K33N(dr[3]);
                    bool touches = alongX ? (fixedC >= dz0 - 0.01 && fixedC <= dz1 + 0.01) : (fixedC >= dx0 - 0.01 && fixedC <= dx1 + 0.01);
                    if (touches) gaps.Add(alongX ? (dx0, dx1) : (dz0, dz1));
                }
                // other areas sharing this edge with no wall (open joins) count as gaps too: a room touching another room's rect along an edge
                double edgeLen = bnd - a, gapLen = K33Covered(a, bnd, gaps);
                double wl = edgeLen - gapLen;
                if (wl <= 0.01) continue;
                wallLen += wl;
                List<(double, double)> On(Func<Kit33Box, bool> f) => decor.Where(f).Where(p => (alongX ? Math.Min(Math.Abs(p.z0 - fixedC), Math.Abs(p.z1 - fixedC)) : Math.Min(Math.Abs(p.x0 - fixedC), Math.Abs(p.x1 - fixedC))) <= UnityTuning.Kit33WallBand)
                    .Select(p => alongX ? (p.x0, p.x1) : (p.z0, p.z1)).ToList();
                var walls = new List<(double, double)>();
                // wall intervals = edge minus gaps
                double cur = a;
                foreach (var g in gaps.OrderBy(g => g.lo)) { if (g.lo > cur) walls.Add((cur, Math.Min(g.lo, bnd))); cur = Math.Max(cur, g.hi); }
                if (cur < bnd) walls.Add((cur, bnd));
                var sk = On(p => p.kind == "skirt");
                var co = On(p => p.kind == "cornice");
                var pieces = decor.Where(p => Kit33WallPieces.Contains(p.kind)).Where(p => (alongX ? Math.Min(Math.Abs(p.z0 - fixedC), Math.Abs(p.z1 - fixedC)) : Math.Min(Math.Abs(p.x0 - fixedC), Math.Abs(p.x1 - fixedC))) <= UnityTuning.Kit33WallBand)
                    .SelectMany(p => p.kind == "hero" // the landmark covers its stretch of wall: it stops the run at both ends
                        ? (alongX ? new[] { p.x0, p.x1 } : new[] { p.z0, p.z1 })
                        : p.kind == "doorFrame" // a door frame stops the run at its two jambs
                        ? (alongX ? new[] { p.x0 + UnityTuning.Kit33DoorJamb / 2, p.x1 - UnityTuning.Kit33DoorJamb / 2 } : new[] { p.z0 + UnityTuning.Kit33DoorJamb / 2, p.z1 - UnityTuning.Kit33DoorJamb / 2 })
                        : new[] { alongX ? p.Cx : p.Cz }).ToList();
                foreach (var (wa, wb) in walls)
                {
                    skirtLen += K33Covered(wa, wb, sk);
                    // the skirt is not owed over a pit that reaches this wall (the floor is gone there)
                    var pitSpans = new List<(double, double)>();
                    foreach (var pt in pits)
                    {
                        if (pt.TryGetValue("zone", out object pz) && pz is string pzs && pzs != id) continue;
                        List<object> pr = PortData.Arr(pt["rect"]);
                        double px0 = K33N(pr[0]), pz0 = K33N(pr[1]), px1 = K33N(pr[2]), pz1 = K33N(pr[3]);
                        bool near = alongX ? (Math.Min(Math.Abs(pz0 - fixedC), Math.Abs(pz1 - fixedC)) <= UnityTuning.Kit33WallBand || (fixedC >= pz0 && fixedC <= pz1))
                            : (Math.Min(Math.Abs(px0 - fixedC), Math.Abs(px1 - fixedC)) <= UnityTuning.Kit33WallBand || (fixedC >= px0 && fixedC <= px1));
                        if (near) pitSpans.Add(alongX ? (px0, px1) : (pz0, pz1));
                    }
                    skirtWall += (wb - wa) - K33Covered(wa, wb, pitSpans);
                    cornLen += K33Covered(wa, wb, co);
                    // the gaps between wall pieces along this wall run (its ends are corners or door jambs: a piece within 1 m of an end closes it)
                    var at = pieces.Where(x => x >= wa - 1 && x <= wb + 1).OrderBy(x => x).ToList();
                    var stops = new List<double> { wa };
                    stops.AddRange(at);
                    stops.Add(wb);
                    // the stretch a landmark covers is dressed by it (no gap inside its span)
                    var heroSpans = decor.Where(p => p.kind == "hero" && WallDist(p) <= UnityTuning.Kit33WallBand).Select(p => alongX ? (p.x0, p.x1) : (p.z0, p.z1)).ToList();
                    double prev = wa;
                    for (int i = 1; i < stops.Count; i++)
                    {
                        double a0 = prev, a1 = stops[i];
                        if (!heroSpans.Any(hs => a0 >= hs.Item1 - 0.01 && a1 <= hs.Item2 + 0.01)) worstGap = Math.Max(worstGap, a1 - a0);
                        prev = stops[i];
                    }
                }
            }
            double skirtPct = skirtWall > 0.01 ? Math.Min(1, skirtLen / skirtWall) : 1, cornPct = wallLen > 0 ? cornLen / wallLen : 1;
            if (skirtPct < 0.98) fails.Add($"skirt {skirtPct:P0}");
            if (cornPct < 0.98) fails.Add($"cornice {cornPct:P0}");
            if (worstGap > UnityTuning.Kit33WallPieceEvery + 1e-6) fails.Add($"wall gap {F(worstGap)} m");
            // 7 door frames, nothing else in an opening
            int framed = 0, blockedDoors = 0;
            foreach (var d in myDoors)
            {
                List<object> dr = PortData.Arr(d["rect"]);
                var door = new Kit33Box { x0 = K33N(dr[0]) - 0.6, z0 = K33N(dr[1]) - 0.6, x1 = K33N(dr[2]) + 0.6, z1 = K33N(dr[3]) + 0.6 };
                double lintel = d.TryGetValue("height", out object dh) ? K33N(dh) : 4;
                if (props.Any(p => p.decor && p.kind == "doorFrame" && K33Dist(door.Cx, door.Cz, p) <= Math.Max(door.x1 - door.x0, door.z1 - door.z0) / 2)) framed++;
                var open = new Kit33Box { x0 = K33N(dr[0]), z0 = K33N(dr[1]), x1 = K33N(dr[2]), z1 = K33N(dr[3]) };
                if (decor.Any(p => p.kind != "doorFrame" && p.kind != "lintelLight" && p.y0 < lintel && p.H > UnityTuning.Kit33FlushH && p.x0 < open.x1 && p.x1 > open.x0 && p.z0 < open.z1 && p.z1 > open.z0)) blockedDoors++;
            }
            if (framed < myDoors.Count) fails.Add($"doors framed {framed}/{myDoors.Count}");
            if (blockedDoors > 0) fails.Add($"{blockedDoors} door opening(s) with decor");
            // 6 the hero landmark
            var heroes = decor.Where(p => p.kind == "hero").ToList();
            string heroCell;
            if (heroes.Count != 1) { heroCell = heroes.Count.ToString(ci); fails.Add($"{heroes.Count} hero landmarks"); }
            else
            {
                Kit33Box h = heroes[0];
                bool offFloor = h.x1 <= x0 + 1e-6 || h.x0 >= x1 - 1e-6 || h.z1 <= z0 + 1e-6 || h.z0 >= z1 - 1e-6 || h.y0 >= UnityTuning.Kit33HighY - 1e-6
; // round 2 (the lead): a relief on the floor-level wall no longer counts: the landmark is outside the rect or above 3 m
                // the entry: the room's spawn (where the player stands on entering)
                double[] eye = { (x0 + x1) / 2, UnityTuning.Kit33EyeY, z1 };
                if (room.TryGetValue("spawn", out object so) && so is Dictionary<string, object> sp) { List<object> sa = PortData.Arr(sp["at"]); eye = new[] { K33N(sa[0]), K33N(sa[1]) + UnityTuning.Kit33EyeY, K33N(sa[2]) }; }
                double[] tgt = { h.Cx, (h.y0 + h.y1) / 2, h.Cz };
                bool behindWall = (tgt[0] < x0 || tgt[0] > x1 || tgt[2] < z0 || tgt[2] > z1) && tgt[1] < roomTop;
                var blocker = boxes.FirstOrDefault(b => !(tgt[0] >= b.x0 && tgt[0] <= b.x1 && tgt[1] >= b.y0 && tgt[1] <= b.y1 && tgt[2] >= b.z0 && tgt[2] <= b.z1) && K33RayHits(eye, tgt, b))
                    ?? props.FirstOrDefault(p => p.collider && K33RayHits(eye, tgt, p));
                bool seen = !behindWall && blocker == null;
                heroCell = (offFloor ? "off" : "ON FLOOR") + (seen ? " seen" : " HIDDEN");
                if (!offFloor) fails.Add("hero on the walkable floor");
                if (!seen) fails.Add("hero not seen from the entry" + (blocker != null ? $" (blocked by '{blocker.id}')" : behindWall ? " (behind the wall)" : ""));
            }
            // 1 the clear lanes: each entrance door's centre to the exit and to each ramp foot of this area
            var lanes = new List<(double ax, double az, double bx, double bz)>();
            var targets = new List<(double x, double z)>();
            if (room.TryGetValue("exit", out object eo) && eo is List<object> el) targets.Add((K33N(el[0]), K33N(el[2])));
            foreach (var rp in ramps.Where(r => (r.TryGetValue("zone", out object zz) ? zz as string : "") == id)) { List<object> f0 = PortData.Arr(rp["from"]); targets.Add((K33N(f0[0]), K33N(f0[1]))); }
            foreach (var d in myDoors)
            {
                List<object> dr = PortData.Arr(d["rect"]);
                double cx = (K33N(dr[0]) + K33N(dr[2])) / 2, cz = (K33N(dr[1]) + K33N(dr[3])) / 2;
                foreach (var t in targets) lanes.Add((cx, cz, t.x, t.z));
            }
            // decor only (collider props in a lane are builder R's layout); a piece flat on a wall (its far face within Kit33WallFlat of the wall),
            // a door frame (beside its opening) and the landmark (on a wall run) never clutter a lane: decor has no collider
            string laneHit = null;
            int decorInLane = 0;
            foreach (var p in decor.Where(p => p.y0 < UnityTuning.Kit33HighY && p.H > UnityTuning.Kit33FlushH && p.kind != "doorFrame" && p.kind != "hero"
                && Math.Min(Math.Min(p.x1 - x0, x1 - p.x0), Math.Min(p.z1 - z0, z1 - p.z0)) > UnityTuning.Kit33WallFlat + 1e-6))
                foreach (var l in lanes)
                    if (K33SegDist(l.ax, l.az, l.bx, l.bz, p) < UnityTuning.Kit33LaneHalf - 1e-6) { decorInLane++; laneHit ??= p.id; break; }
            if (decorInLane > 0) fails.Add($"{decorInLane} decor in a clear lane ({laneHit})");
            // 10 cover reach
            var cover = mine.Where(p => p.collider && p.H >= 1.0).Concat(boxes.Where(b => b.room == id && b.y1 >= 1.0 && b.y0 <= 0.5)).ToList();
            int cells = 0, reached = 0;
            for (double cx = x0 + 0.5; cx < x1; cx += UnityTuning.Kit33CoverCell)
                for (double cz = z0 + 0.5; cz < z1; cz += UnityTuning.Kit33CoverCell)
                {
                    cells++;
                    if (cover.Any(c => K33Dist(cx, cz, c) <= UnityTuning.Kit33CoverReach)) reached++;
                }
            double reach = cells > 0 ? reached / (double)cells : 1;
            int coverPieces = mine.Count(p => p.collider);
            // cover is builder R's layout (gameplay colliders): a WARN in the dressing verdict, RED only with --strict
            if (reach < 0.999) { if (strict) fails.Add($"cover reach {reach:P0}"); else { warn = true; layout.Add($"{id}: cover reach {reach:P0}"); } }
            if (area >= UnityTuning.Kit33CoverOpenMin && kind != "link" && cover.Count < Math.Floor(area / UnityTuning.Kit33CoverMinM2)) warn = true;
            // 13 colliders and the box cap
            int decorColliders = decor.Count(p => p.collider);
            if (decorColliders > 0) fails.Add($"{decorColliders} decor colliders");
            if (decor.Count > UnityTuning.Kit33DrawBoxCap) fails.Add($"{decor.Count} decor rows (cap {UnityTuning.Kit33DrawBoxCap})");
            // 12 palette
            int badColour = decor.Count(p => p.color != null && (p.color == "cyan" || p.color == "usable" || p.color == "magenta" || (p.color == "orange" && !Kit33Hazards.Contains(p.kind))));
            if (badColour > 0) fails.Add($"{badColour} decor off-palette");
            // 14 z-fight
            int zf = 0;
            string zfFirst = null;
            var ax = decor.Where(p => p.axis && p.kind != "scorch" && p.kind != "inlay").ToList();
            for (int i = 0; i < ax.Count; i++)
                for (int j = i + 1; j < ax.Count; j++)
                {
                    Kit33Box a = ax[i], b = ax[j];
                    double ox = Math.Min(a.x1, b.x1) - Math.Max(a.x0, b.x0), oy = Math.Min(a.y1, b.y1) - Math.Max(a.y0, b.y0), oz = Math.Min(a.z1, b.z1) - Math.Max(a.z0, b.z0);
                    // a shared plane ON a wall face or the floor is hidden (both faces lie against the wall / the floor): not a z-fight
                    bool Hid(double v, double w0, double w1) => Math.Abs(v - w0) < 0.01 || Math.Abs(v - w1) < 0.01;
                    bool fx = ((Math.Abs(a.x0 - b.x0) < 0.01 && !Hid(a.x0, x0, x1)) || (Math.Abs(a.x1 - b.x1) < 0.01 && !Hid(a.x1, x0, x1))) && oy > 0.01 && oz > 0.01;
                    bool fy = ((Math.Abs(a.y0 - b.y0) < 0.01 && Math.Abs(a.y0) >= 0.01) || (Math.Abs(a.y1 - b.y1) < 0.01 && Math.Abs(a.y1 - roomTop) >= 0.01)) && ox > 0.01 && oz > 0.01; // the floor and the roof hide their planes
                    bool fz = ((Math.Abs(a.z0 - b.z0) < 0.01 && !Hid(a.z0, z0, z1)) || (Math.Abs(a.z1 - b.z1) < 0.01 && !Hid(a.z1, z0, z1))) && ox > 0.01 && oy > 0.01;
                    if ((fx || fy || fz) && ox > -0.01 && oy > -0.01 && oz > -0.01) { zf++; if (zfFirst == null) zfFirst = a.id + " / " + b.id; }
                }
            if (zf > 0) fails.Add($"{zf} z-fight pairs (first {zfFirst})");
            // round 2: the ride strip and L's trim stay in view. A piece touching a wall (within 0.01 m), deeper than the strip's front face, may not cover
            // the strip's band (+- Kit33RideGap) unless its gap clears it; a piece on the floor (base under the trim's top) touching a wall reaches at most
            // to L's trim line. The wash (2 cm, behind the strip's front) and the door frames' lintel lights are thin.
            int hidesRide = 0, onTrim = 0;
            string rideFirst = null, trimFirst = null;
            foreach (var p in decor)
            {
                if (WallDist(p) > 0.01) continue;
                double depth = Math.Min(Math.Min(p.x1 - x0, x1 - p.x0), Math.Min(p.z1 - z0, z1 - p.z0)); // the far face's distance from its wall
                double bandLo = rideLo - UnityTuning.Kit33RideGap + 0.01, bandHi = rideHi + UnityTuning.Kit33RideGap - 0.01;
                bool covers = p.y0 < bandHi && p.y1 > bandLo && depth > rideFront - 1e-6;
                bool cleared = p.Clears(bandLo, bandHi);
                if (covers && !cleared) { hidesRide++; rideFirst ??= p.id; }
                if (p.y0 < trimH && depth > trimNear + 1e-6) { onTrim++; trimFirst ??= p.id; }
            }
            // round 2: L's light sockets (the plan's / level's `lights`) are never covered by decor (the wash, 2 cm, sits behind a socket's face)
            int overSocket = 0;
            string socketFirst = null;
            foreach (var p in decor)
                foreach (var sk in sockets)
                    if (p.x0 < sk.x1 && p.x1 > sk.x0 && p.y0 < sk.y1 && p.y1 > sk.y0 && p.z0 < sk.z1 && p.z1 > sk.z0 && !p.Clears(sk.y0, sk.y1) && !(p.kind == "skirt" && sk.kind == "floorSocket")) { overSocket++; socketFirst ??= p.id + " on " + sk.id; break; }
            if (overSocket > 0) fails.Add($"{overSocket} decor over L's light sockets (first {socketFirst})");
            if (hidesRide > 0) fails.Add($"{hidesRide} decor over the wall-ride strip's band (first {rideFirst})");
            if (onTrim > 0) fails.Add($"{onTrim} floor decor reaching L's trim line (first {trimFirst})");
            // the mesh objects (Kit33Props round 2): one stone mesh for the area + one per glow colour it uses (its draws before culling)
            var glowOf = new Dictionary<string, string> { ["lintelLight"] = "lamp", ["edgeLight"] = "lamp", ["lamp"] = "lamp", ["lampPost"] = "lamp", ["slot"] = "lamp", ["inlay"] = "inlay",
                ["lavaChannel"] = "danger", ["hazardStripe"] = "danger", ["heatVent"] = "danger", ["kilnArch"] = "mouth", ["hero"] = "accent", ["checkpoint"] = "usable" };
            var glowSet = new HashSet<string>(mine.Where(p => p.decor || Kit33Legacy.Contains(p.kind)).Select(p => glowOf.TryGetValue(p.kind, out string gk) ? gk : null).Where(gk => gk != null));
            int objects = (mine.Any(p => p.decor) ? 1 : 0) + glowSet.Count;
            totalObjects += objects;
            totalRows += mine.Count(p => p.decor);
            // 11 devices (information)
            double padRate = pads.Count(p => (p.TryGetValue("zone", out object pz) ? pz as string : "") == id) / area * 150;
            bool isRed = fails.Count > 0;
            if (isRed) { red++; why.Add($"{id}: {string.Join("; ", fails)}"); }
            if (!quiet || isRed)
                Console.WriteLine($"| {id} | {kind} | {F(area)} | {decor.Count} | {F(dens)} | {high:P0} | {(loose == 0 ? "ok" : loose + " loose")} | {skirtPct:P0} | {cornPct:P0} | {F(worstGap)} | {framed}/{myDoors.Count} | {heroCell} | {(decorInLane == 0 ? "clear" : decorInLane + " in")} | {reach:P0} | {coverPieces} | {decorColliders} | {(badColour == 0 ? "ok" : badColour + " bad")} | {zf} | {padRate.ToString("0.0", ci)} | {(hidesRide == 0 && onTrim == 0 ? "clear" : hidesRide + " / " + onTrim)} | {objects} | {(isRed ? "RED" : warn ? "WARN" : "PASS")} |");
        }
        foreach (string w in why) Console.WriteLine("  RED " + w);
        Console.WriteLine($"kit33 draw cost: {totalRows} decor rows -> {totalObjects} mesh objects (one stone mesh per area + one per area and glow colour; round 1 drew the whole level as 1 stone mesh + one per colour, never culled)");
        foreach (string w in layout) Console.WriteLine("  LAYOUT (builder R, --strict makes it RED) " + w);
        Console.WriteLine($"kit33 {(red == 0 ? "PASS" : "RED")}: {graded - red}/{graded} areas pass (density target {UnityTuning.Kit33DensityMin}-{UnityTuning.Kit33DensityMax} per 100 m2, >= {UnityTuning.Kit33HighShare:P0} above {UnityTuning.Kit33HighY} m; check 8 is levelgen's report row 4); decor hash {hash:x8}");
        return red == 0 ? 0 : 1;
    }
}
