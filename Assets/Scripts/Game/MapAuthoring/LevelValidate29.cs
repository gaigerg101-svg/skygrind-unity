// NIGHT29 (builder R): the ROOM STANDARD rows of the level report (DESIGN29.md section A), added to LevelValidate24's rows when the plan carries
// `"n29": true` (copied into the map JSON's `campaign` block by LevelPlan24 / LevelPlan29). Pure (no UnityEngine). Every test that touches the
// world uses the REAL collider (GameWorld.RayIntersect on the map's boxes), never the plan's own numbers alone. The only numbers are
// design.n29.lights.maxWallDepth and design.n29.doors.fitTolerance (Design/unity-config.json) and the body's capsule height (config body).
//   lights    each socket: kind wall|ceiling|pool, colour in the dusk palette, an axis normal, range and intensity > 0, inside its room, FLUSH to a
//             surface (rays along -normal from the centre and the four footprint corners meet the surface at half the depth) and no deeper than
//             maxWallDepth unless it is a collider; every room has a light; at most one accent (amber / orange) per room; orange only with
//             `danger: true`; a POOL light within poolReach (a fixed 8 m of this validator's own: the goal's pool) of each room's exit
//   props     authored rows: unique ids, inside their room, standing on the floor / a plan box top / hung from the roof / flat on a wall; a prop
//             with no collider is flat (thinnest side at most maxWallDepth); a collider prop clears the spawn, the exit and every encounter node
//   doorFit   a door's rect is exactly the gap between its two rooms (gap 0 to the rooms' rects, and the rooms' wall rings meet), lies inside both
//             rooms' extent along the wall, its opening is that rect (rays sideways at three depths meet the wall face at the rect's edge), and its
//             clear height is the plan's `height` (a ray up through its middle meets the lintel or roof there) and at least the body's height
//   closedRoom every room and link has `roof: true` and a `height`; rays UP from just under the roof at every 2 m of the room's floor meet the
//             roof within 0.35 m (no sky gap), and the ring walls reach the roof
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game.MapAuthoring
{
    public static class LevelValidate29
    {
        static Dictionary<string, object> O(object o) => o as Dictionary<string, object>;
        static List<object> L(object o) => o as List<object> ?? new List<object>();
        static double N(object o) => PortData.Num(o);
        static object Get(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out object v) ? v : null;
        static string S(Dictionary<string, object> d, string k) => Get(d, k) as string ?? "";
        static string F(double v) => (Math.Round(v * 100) / 100).ToString("0.##", CultureInfo.InvariantCulture);

        /// <summary>The plan asks for the NIGHT29 room standard.</summary>
        public static bool Wanted(Dictionary<string, object> map) => map.ContainsKey("campaign") && Get(O(map["campaign"]), "n29") is bool b && b;

        public static readonly string[] Kinds = { "wall", "ceiling", "pool" };
        public static readonly string[] Colors = { "cyan", "green", "amber", "white", "orange" };
        /// <summary>The goal's pool must stand within this many metres (on the floor plan) of the room's exit.</summary>
        const double PoolReach = 8;
        /// <summary>The wall ring's thickness (LevelPlan24.WallThick: a geometry constant of the plan conversion, not a gameplay number).</summary>
        const double WallThick = 2;

        static Vec3 V(object o)
        {
            List<object> a = L(o);
            return new Vec3(N(a[0]), N(a[1]), N(a[2]));
        }

        static bool Vec3Ok(object o) => o is List<object> a && a.Count == 3;

        static bool Hit(GameWorld world, Vec3 o, Vec3 d, out double dist)
        {
            bool h = world.RayIntersect(o, d, out RayHit rh);
            dist = h ? rh.distance : double.NaN;
            return h;
        }

        public static void Rows(GameConfig cfg, Dictionary<string, object> map, LevelDef level, GameWorld world, List<(string check, bool pass, string detail)> rows)
        {
            if (!Wanted(map)) return;
            var camp = O(map["campaign"]);
            double maxDepth = cfg.Num("design.n29.lights.maxWallDepth");
            double tol = cfg.Num("design.n29.doors.fitTolerance");
            double bodyH = cfg.Num("body.capsuleHeight");
            var roomJson = new Dictionary<string, Dictionary<string, object>>();
            foreach (object ro in L(Get(camp, "rooms"))) roomJson[S(O(ro), "id")] = O(ro);
            var roomDef = new Dictionary<string, RoomDef>();
            foreach (RoomDef r in level.rooms) roomDef[r.id] = r;

            Lights(maxDepth, camp, level, roomDef, world, rows);
            Props(maxDepth, camp, level, roomDef, roomJson, world, rows);
            DoorFit(tol, bodyH, camp, level, roomDef, roomJson, world, rows);
            ClosedRoom(level, roomJson, world, rows);
            BlastGaps(cfg, camp, world, rows);
            DashGaps(cfg, camp, world, rows);
            Proofs(cfg, camp, world, rows);
            Budget(cfg, level, rows);
            Lava(map, camp, roomJson, world, rows);
            Spawn31(cfg, camp, rows);
        }

        // ---------------------------------------------------------------- NIGHT31 (builder L, P0 c): no wave spawn beside the entry or in the first view
        /// <summary>A wave node never closer than this (m, on the floor plan) to the room's entry spawn (NIGHT31 P0 c; NIGHT30 frames: husks beside the entry in f9).
        /// A layout rule of the report, not a gameplay number.</summary>
        const double SpawnMinEntry = 6;
        /// <summary>The first view cone: the entry spawn's facing (its yawDeg), as wide as the camera's HORIZONTAL view at 16:9 (camera.fovDeg is vertical, like
        /// three.js: 74 -> 106.5 deg across) plus SpawnConeMarginDeg each side. Round 2 (the lead's Player frames, 11:27): the first pick, 90 degrees, left spawns
        /// at 45-53 degrees on the screen's edges (i2). Layout rules of the report, not gameplay numbers.</summary>
        const double SpawnAspect = 16.0 / 9.0, SpawnConeMarginDeg = 5;

        /// <summary>The cone's half angle in degrees (about 58.3 with camera.fovDeg 74).</summary>
        public static double SpawnConeHalfDeg(GameConfig cfg)
        {
            double v = cfg.Num("camera.fovDeg") * Math.PI / 180;
            return Math.Atan(Math.Tan(v / 2) * SpawnAspect) * 180 / Math.PI + SpawnConeMarginDeg;
        }

        /// <summary>Every encounter node (nodes and highNodes) of a combat room: at least SpawnMinEntry m from the room's spawn, and a node inside the first view
        /// cone at least design.n24.campaign.minSpawnDist m away (the runtime's own nearest spawn distance, Campaign24.SpawnWave): a node in front of the player
        /// is never a pop-in at arm's length, and the runtime's farthest-first fallback never has to place an enemy beside the door. Only for an "n30": true plan.
        /// (Read literally, "outside the view cone" would put every enemy beside or behind the player in these north-running rooms; the far-in-cone
        /// allowance is logged in NIGHT31-LOG.)</summary>
        static void Spawn31(GameConfig cfg, Dictionary<string, object> camp, List<(string check, bool pass, string detail)> rows)
        {
            if (!(Get(camp, "n30") is bool on) || !on) return;
            double far = cfg.Num("design.n24.campaign.minSpawnDist");
            double half = SpawnConeHalfDeg(cfg), cosHalf = Math.Cos(half * Math.PI / 180);
            var bad = new List<string>();
            int rooms = 0, nodes = 0;
            double nearest = double.MaxValue;
            foreach (object ro in L(Get(camp, "rooms")))
            {
                var rj = O(ro);
                if (S(rj, "kind") != "room" || !(Get(rj, "spawn") is Dictionary<string, object> sp) || !Vec3Ok(Get(sp, "at"))) continue;
                Vec3 s = V(sp["at"]);
                double yaw = (Get(sp, "yawDeg") != null ? N(sp["yawDeg"]) : 0) * Math.PI / 180;
                double fx = -Math.Sin(yaw), fz = -Math.Cos(yaw); // yaw 0 faces north (-z); +yaw turns left (west)
                var all = new List<object>(L(Get(rj, "nodes")));
                all.AddRange(L(Get(rj, "highNodes")));
                if (all.Count == 0) continue;
                rooms++;
                foreach (object no in all)
                {
                    if (!Vec3Ok(no)) continue;
                    nodes++;
                    Vec3 n = V(no);
                    double dx = n.x - s.x, dz = n.z - s.z, d = Math.Sqrt(dx * dx + dz * dz);
                    nearest = Math.Min(nearest, d);
                    bool inCone = d > 1e-9 && (dx * fx + dz * fz) / d > cosHalf;
                    if (d < SpawnMinEntry - 1e-9) bad.Add($"{S(rj, "id")} node ({F(n.x)}, {F(n.y)}, {F(n.z)}) is {F(d)} m from the entry (min {F(SpawnMinEntry)})");
                    else if (inCone && d < far - 1e-9) bad.Add($"{S(rj, "id")} node ({F(n.x)}, {F(n.y)}, {F(n.z)}) is in the first view cone at {F(d)} m (in the cone: min {F(far)})");
                }
            }
            if (rooms == 0) return;
            rows.Add(("spawn31", bad.Count == 0, bad.Count == 0
                ? $"{nodes} encounter nodes in {rooms} rooms: each at least {F(SpawnMinEntry)} m from its room's entry, and inside the entry's {F(2 * half)} degree view cone (the camera's width + {F(SpawnConeMarginDeg)} each side) only at {F(far)} m or more (nearest {F(nearest)} m)"
                : $"{bad.Count} node(s): " + string.Join("; ", bad)));
        }

        // ---------------------------------------------------------------- NIGHT30 (builder L): lava pits (LEVEL-FORMAT.md, the lava rows)
        /// <summary>The lava trench's depth band below the floor (0.6-1.0 m: a knocked-up player clears the rim; LEVEL-FORMAT.md). A geometry rule of the format, not a gameplay number.</summary>
        const double LavaMinDepth = 0.6, LavaMaxDepth = 1.0;

        /// <summary>Every `lava: true` pit: its surface 0.6-1.0 m below the floor, its room has NO respawnY (lava never teleports), the map's top-level `lava` array
        /// carries it (rect, y, room), and a ray down at its centre meets the lava surface at that y (the collider is where the format says).</summary>
        static void Lava(Dictionary<string, object> map, Dictionary<string, object> camp, Dictionary<string, Dictionary<string, object>> roomJson, GameWorld world, List<(string check, bool pass, string detail)> rows)
        {
            var bad = new List<string>();
            int n = 0;
            List<object> top = L(Get(map, "lava"));
            foreach (object po in L(Get(camp, "pits")))
            {
                var p = O(po);
                if (!(Get(p, "lava") is bool lv) || !lv) continue;
                n++;
                string room = S(p, "zone");
                List<object> rc = L(p["rect"]);
                double y = N(p["floorY"]);
                string tag = $"{room} lava ({F(N(rc[0]))}..{F(N(rc[2]))} x {F(N(rc[1]))}..{F(N(rc[3]))})";
                if (-y < LavaMinDepth - 1e-9 || -y > LavaMaxDepth + 1e-9) bad.Add($"{tag}: surface y {F(y)} is not {F(LavaMinDepth)}-{F(LavaMaxDepth)} m below the floor");
                if (roomJson.TryGetValue(room, out var rj) && Get(rj, "respawnY") != null) bad.Add($"{tag}: room {room} has a respawnY (lava never teleports)");
                bool found = false;
                foreach (object to in top)
                {
                    var t = O(to);
                    List<object> tr = L(Get(t, "rect"));
                    if (S(t, "room") == room && tr.Count == 4 && Math.Abs(N(Get(t, "y")) - y) < 1e-6 && Math.Abs(N(tr[0]) - N(rc[0])) < 1e-6 && Math.Abs(N(tr[1]) - N(rc[1])) < 1e-6 && Math.Abs(N(tr[2]) - N(rc[2])) < 1e-6 && Math.Abs(N(tr[3]) - N(rc[3])) < 1e-6) found = true;
                }
                if (!found) bad.Add($"{tag}: missing from the map's top-level `lava` array");
                Vec3 c = new Vec3((N(rc[0]) + N(rc[2])) / 2, 0.5, (N(rc[1]) + N(rc[3])) / 2);
                if (!Hit(world, c, new Vec3(0, -1, 0), out double d)) bad.Add($"{tag}: no surface under its centre");
                else if (Math.Abs(0.5 - d - y) > 0.02 && 0.5 - d < y - 0.02) bad.Add($"{tag}: the collider under its centre is at y {F(0.5 - d)}, below the lava surface {F(y)}");
            }
            if (top.Count != n) bad.Add($"the map's `lava` array has {top.Count} entries for {n} lava pits");
            if (n == 0 && top.Count == 0) return;
            rows.Add(("lava", bad.Count == 0, bad.Count == 0 ? $"{n} lava pits: each {F(LavaMinDepth)}-{F(LavaMaxDepth)} m below the floor, no respawnY in its room, in the map's `lava` array, the collider at the surface" : string.Join("; ", bad)));
        }

        // ---------------------------------------------------------------- the wave budget (design.n27.budget, the tank costs a heavy)
        static void Budget(GameConfig cfg, LevelDef level, List<(string check, bool pass, string detail)> rows)
        {
            double wBase = cfg.Num("design.n27.budget.waveBase"), wPer = cfg.Num("design.n27.budget.wavePerIndex");
            int maxWave = (int)cfg.Num("design.n27.budget.maxWaveSize");
            var bad = new List<string>();
            int total = 0;
            foreach (RoomDef r in level.rooms)
            {
                for (int wi = 0; wi < r.waves.Count; wi++)
                {
                    double cost = 0;
                    var kinds = new List<string>();
                    bool heavy = false;
                    foreach (string k in r.waves[wi])
                    {
                        double c = k == "husk" ? cfg.Num("design.n27.budget.costHusk") : k == "swarmer" ? cfg.Num("design.n27.budget.costSwarmer") : k == "spitter" ? cfg.Num("design.n27.budget.costSpitter")
                            : k == "heavy" || k == "tank" ? cfg.Num("design.n27.budget.costHeavy")
                            : cfg.Has("design.n40.iceline40.budgetCost." + k) ? cfg.Num("design.n40.iceline40.budgetCost." + k) : k == "burrower" && cfg.Has("design.n43.burrow43.budgetCost") ? cfg.Num("design.n43.burrow43.budgetCost") : double.NaN; // NIGHT40 L2: the WARDEN, DRONE and CHARGER's costs (n40-iceline places them)
                        if (double.IsNaN(c)) { bad.Add($"{r.id} w{wi + 1}: kind '{k}' has no cost"); continue; }
                        cost += c;
                        if (!kinds.Contains(k)) kinds.Add(k);
                        if (k == "heavy" || k == "tank") heavy = true;
                    }
                    double budget = wBase + wPer * wi;
                    if (cost > budget + 1e-9) bad.Add($"{r.id} w{wi + 1} costs {F(cost)} over its {F(budget)} points");
                    if (r.waves[wi].Count > maxWave) bad.Add($"{r.id} w{wi + 1} has {r.waves[wi].Count} enemies (at most {maxWave})");
                    if (kinds.Count > (heavy ? 3 : 2)) bad.Add($"{r.id} w{wi + 1} mixes {kinds.Count} kinds");
                    total += r.waves[wi].Count;
                }
            }
            rows.Add(("budget", bad.Count == 0, bad.Count == 0 ? $"{total} enemies: every wave within waveBase + wavePerIndex x its index (husk 1, swarmer 0.5, spitter 2, heavy and tank 6), at most {maxWave} per wave, at most 2 kinds (3 with a heavy or tank)" : string.Join("; ", bad)));
        }

        // ---------------------------------------------------------------- completability: the real Mover walks each room's intended route
        /// <summary>A room's `proofs` list, each run on the REAL Mover and the real collider (no doors, no sim): `walk` {points: [[x,y,z],...]} steers through the points on foot (success: within 1.3 m of the last, grounded;
        /// `jump: true` presses jump when the way is blocked), `jumpGap` {from, gap} (a running jump at the lip crosses), `fall` {from, to} (walks on and must drop below the room's respawnY: the stage respawn then
        /// returns the player to the room's start), `ride` {from, hazardZ: [zNorth, zSouth], toZ} (jump, dash, hug the west wall: no grounded step inside the hazard strip, reaches toZ).
        /// One row per room with the proof line printed.</summary>
        static void Proofs(GameConfig cfg, Dictionary<string, object> camp, GameWorld world, List<(string check, bool pass, string detail)> rows)
        {
            double dt = cfg.Dt;
            foreach (object ro in L(Get(camp, "rooms")))
            {
                var rj = O(ro);
                var proofs = L(Get(rj, "proofs"));
                if (proofs.Count == 0) continue;
                var lines = new List<string>();
                bool all = true;
                foreach (object po in proofs)
                {
                    var p = O(po);
                    string kind = S(p, "kind");
                    bool ok = false;
                    string line;
                    var m = new Mover(cfg.movement, world);
                    if (kind == "walk")
                    {
                        List<object> pts = L(p["points"]);
                        Vec3 start = V(pts[0]);
                        m.Reset(start.x, start.y, start.z);
                        int idx = 1, steps = MovementMath.StepsFor(p.ContainsKey("maxSeconds") ? N(p["maxSeconds"]) : 20, dt);
                        double t = -1, minY = 0;
                        for (int i = 0; i < steps && idx < pts.Count; i++)
                        {
                            Vec3 g = V(pts[idx]);
                            double dx = g.x - m.position.x, dz = g.z - m.position.z, dist = JsMath.Hypot(dx, dz);
                            bool last = idx == pts.Count - 1;
                            if (dist < (last ? 1.3 : 0.9) && Math.Abs(m.position.y - g.y) < 1.0 && (m.grounded || !last)) { idx++; if (idx >= pts.Count) { t = i * dt; ok = true; } continue; }
                            var f = new InputFrame { moveZ = 1, yaw = Math.Atan2(-dx, -dz) };
                            if (Get(p, "jump") is bool jb && jb) f.jump = new Button { held = true, pressed = m.grounded && i % 24 == 0 }; // held through the whole jump (releasing it cuts the jump short)
                            m.Step(f, dt);
                            if (m.position.y < minY) minY = m.position.y;
                        }
                        if (Get(p, "expect") as string == "blocked") { ok = !ok; line = ok ? $"the straight line is CLOSED: stopped at ({F(m.position.x)}, {F(m.position.y)}, {F(m.position.z)})" : "the straight line is OPEN (it should be closed)"; }
                        else line = ok ? $"walks {pts.Count - 1} leg(s) in {F(t)} s" : $"STOPPED at ({F(m.position.x)}, {F(m.position.y)}, {F(m.position.z)}) before point {idx + 1}";
                    }
                    else if (kind == "jumpGap")
                    {
                        Vec3 from = V(p["from"]);
                        double gap = N(p["gap"]);
                        bool dbl = Get(p, "dbl") is bool db && db;
                        ok = Cross(m, dt, from.x, from.z - 1, gap, true, false, false, 0, 1, dbl, out double reach);
                        line = $"a running jump{(dbl ? " and a second jump" : "")} at the lip crosses {F(gap)} m (reach {F(reach)} m): {(ok ? "yes" : "NO")}";
                    }
                    else if (kind == "fall")
                    {
                        Vec3 a = V(p["from"]), b = V(p["to"]);
                        double respawn = Get(rj, "respawnY") != null ? N(rj["respawnY"]) : double.NaN;
                        m.Reset(a.x, a.y, a.z);
                        int steps = MovementMath.StepsFor(4, dt);
                        double minY = 0;
                        for (int i = 0; i < steps; i++)
                        {
                            double dx = b.x - m.position.x, dz = b.z - m.position.z;
                            m.Step(new InputFrame { moveZ = 1, yaw = Math.Atan2(-dx, -dz) }, dt);
                            if (m.position.y < minY) minY = m.position.y;
                        }
                        ok = !double.IsNaN(respawn) && minY < respawn;
                        line = $"walking off the edge drops to y {F(minY)}, below the stage respawn line {F(respawn)}: the player returns to the room's start: {(ok ? "yes" : "NO")}";
                    }
                    else if (kind == "ride")
                    {
                        Vec3 a = V(p["from"]);
                        List<object> hz = L(p["hazardZ"]);
                        double zN = N(hz[0]), zS = N(hz[1]), toZ = N(p["toZ"]);
                        m.Reset(a.x, a.y, a.z);
                        int steps = MovementMath.StepsFor(8, dt);
                        int grounded = 0, rideSteps = 0, jumpAt = MovementMath.StepsFor(p.ContainsKey("runUp") ? N(p["runUp"]) : 0.5, dt);
                        bool dashed = false, reached = false, wallJumped = false;
                        double lean = p.ContainsKey("lean") ? N(p["lean"]) : 0.25;
                        double wjZ = p.ContainsKey("wallJumpAtZ") ? N(p["wallJumpAtZ"]) : double.NegativeInfinity; // NIGHT30: one wall jump off the lane once past this z (f5b: across to the E lane)
                        for (int i = 0; i < steps && !reached; i++)
                        {
                            var f = new InputFrame { moveZ = 1, yaw = i >= jumpAt ? lean : 0 };
                            if (i == jumpAt) f.jump = new Button { held = true, pressed = true };
                            else if (!wallJumped && m.state == MoveState.WallRide && m.position.z <= wjZ) { wallJumped = true; f.jump = new Button { held = true, pressed = true }; f.yaw = 0; }
                            else if (i > jumpAt) f.jump = new Button { held = true, pressed = m.grounded }; // jump again at every landing, as the flue climber does
                            if (!dashed && i > jumpAt + 6 && !m.grounded) { dashed = true; f.dash = new Button { held = true, pressed = true }; }
                            m.Step(f, dt);
                            if (m.state == MoveState.WallRide) rideSteps++;
                            if (m.grounded && m.position.z < zS && m.position.z > zN && m.position.y < 1) grounded++;
                            if (m.position.z <= toZ) reached = true;
                        }
                        if (reached && p.ContainsKey("toY") && m.position.y < N(p["toY"]) - 0.1) reached = false; // NIGHT30: the lane must END on a ledge this high (f5b's island)
                        ok = reached && grounded <= MovementMath.StepsFor(0.1, dt) && rideSteps > 0; // at most 0.1 s of floor contact on the strip (a hazard tick is 0.25 s)
                        line = $"wall lane: reaches z {F(toZ)} with {grounded} grounded steps on the hazard strip, {F(rideSteps * dt)} s riding, ends at ({F(m.position.x)}, {F(m.position.y)}, {F(m.position.z)}): {(ok ? "yes" : "NO")}";
                    }
                    else if (kind == "jumpDash")
                    {
                        // NIGHT30 (builder L): from `start` (on a ledge) run north, jump at lipZ, dash 0.1 s later (optionally `dbl`: a second jump at the apex
                        // instead of the dash); success: grounded at or above toY past lipZ - gap within 3 s
                        Vec3 st = V(p["start"]);
                        double lipZ = N(p["lipZ"]), gap = N(p["gap"]), toY = N(p["toY"]);
                        bool dbl = Get(p, "dbl") is bool db2 && db2;
                        m.Reset(st.x, st.y, st.z);
                        int steps = MovementMath.StepsFor(3, dt), fired = -1;
                        double reach = 0;
                        for (int i = 0; i < steps; i++)
                        {
                            var f = new InputFrame { moveZ = 1 };
                            if (fired < 0 && m.position.z <= lipZ + 0.3) { fired = i; f.jump = new Button { held = true, pressed = true }; }
                            else if (fired >= 0) f.jump = new Button { held = true, pressed = false };
                            if (fired >= 0 && !dbl && i == fired + MovementMath.StepsFor(0.1, dt)) f.dash = new Button { held = true, pressed = true };
                            if (fired >= 0 && dbl && i == fired + MovementMath.StepsFor(0.3, dt)) f.jump = new Button { held = true, pressed = true };
                            m.Step(f, dt);
                            if (fired >= 0 && lipZ - m.position.z > reach) reach = lipZ - m.position.z;
                            if (fired >= 0 && m.grounded && m.position.y >= toY - 0.05 && m.position.z < lipZ - gap) { ok = true; break; }
                            if (m.position.y < toY - 4) break;
                        }
                        line = $"from y {F(st.y)} a running jump then {(dbl ? "a second jump" : "a dash")} at the lip crosses {F(gap)} m onto y {F(toY)} (reach {F(reach)} m, ends at ({F(m.position.x)}, {F(m.position.y)}, {F(m.position.z)})): {(ok ? "yes" : "NO")}";
                        if (Get(p, "expectFail") is bool ef && ef) { ok = !ok; line += ok ? " (as designed: the gap needs the grapple)" : " (it should NOT cross: the gap does not force the grapple)"; } // NIGHT30: a grapple gap
                    }
                    else { line = $"unknown proof kind '{kind}'"; }
                    lines.Add($"{kind}: {line}");
                    all &= ok;
                }
                rows.Add(($"completable: {S(rj, "id")}", all, string.Join("; ", lines)));
            }
        }

        // ---------------------------------------------------------------- the PULSE blast gap
        /// <summary>The real Mover runs north from 6 m before the lip: jumps at the lip (jump true), then in the air optionally dashes (dashAfter) and takes the
        /// PULSE blast (boost: design.n27.pulse27.boostSpeed added along the aim in equal parts over boostSeconds). True when it stands grounded on the far side within 3 s.</summary>
        /// <summary>The blast's aim in this check: 25 degrees above the horizon (a test geometry of the validator, not a gameplay number).</summary>
        public static readonly double BlastPitchRad = 25 * Math.PI / 180;

        public static bool Cross(Mover m, double dt, double x, double lip, double gap, bool jump, bool dash, bool blast, double boostSpeed, int boostSteps, bool dbl, out double reach)
        {
            reach = 0;
            m.Reset(x, 0, lip + 6);
            int steps = MovementMath.StepsFor(3, dt);
            int fired = -1;
            for (int i = 0; i < steps; i++)
            {
                var f = new InputFrame { moveZ = 1 };
                if (fired < 0 && m.position.z <= lip + 0.3)
                {
                    fired = i;
                    if (jump) f.jump = new Button { held = true, pressed = true };
                    else if (dash) f.dash = new Button { held = true, pressed = true };
                }
                if (fired >= 0 && jump && dash && i == fired + MovementMath.StepsFor(0.1, dt)) f.dash = new Button { held = true, pressed = true };
                if (fired >= 0 && jump && dbl && i == fired + MovementMath.StepsFor(0.3, dt)) f.jump = new Button { held = true, pressed = true }; // the second jump, at the apex
                if (blast && fired >= 0 && i >= fired && i < fired + boostSteps)
                    m.Impulse(0, boostSpeed / boostSteps * JsMath.Sin(BlastPitchRad), -boostSpeed / boostSteps * JsMath.Cos(BlastPitchRad)); // aimed a little up, as a player aims at a far ledge
                m.Step(f, dt);
                if (fired >= 0 && lip - m.position.z > reach) reach = lip - m.position.z; // the farthest past the lip (the row prints it)
                if (m.grounded && m.position.y >= -0.05 && m.position.z < lip - gap) return true;
                if (m.position.y < -3) return false;
            }
            return false;
        }

        /// <summary>A room with `dashGap`: the real Mover with a jump and a second jump at the lip must fall short and a jump then a dash must cross (the lesson's own words:
        /// "jump, then dash"): the gap is a dash gap and not a double-jump gap (RIVAL's graph, which the lesson row uses, jumps farther than the real Mover does).</summary>
        static void DashGaps(GameConfig cfg, Dictionary<string, object> camp, GameWorld world, List<(string check, bool pass, string detail)> rows)
        {
            foreach (object ro in L(Get(camp, "rooms")))
            {
                var rj = O(ro);
                if (!(Get(rj, "dashGap") is Dictionary<string, object> dg)) continue;
                Vec3 from = V(dg["from"]);
                double gap = N(dg["gap"]), lip = from.z - 1;
                bool dbl = Cross(new Mover(cfg.movement, world), cfg.Dt, from.x, lip, gap, true, false, false, 0, 1, true, out double rDbl);
                bool jd = Cross(new Mover(cfg.movement, world), cfg.Dt, from.x, lip, gap, true, true, false, 0, 1, false, out double rJd);
                bool ok = !dbl && jd;
                rows.Add(($"dash lesson gap: {S(rj, "id")} ({F(gap)} m)", ok,
                    $"the real Mover at the lip: a jump then a second jump {(dbl ? "CROSSES" : "falls short")} (reach {F(rDbl)} m), a jump then a dash {(jd ? "lands on the far side" : "does NOT cross")} ({F(rJd)} m): {(ok ? "only the dash crosses" : "the gap does not force the dash")}"));
            }
        }

        static void BlastGaps(GameConfig cfg, Dictionary<string, object> camp, GameWorld world, List<(string check, bool pass, string detail)> rows)
        {
            double boostSpeed = cfg.Num("design.n27.pulse27.boostSpeed"), boostSeconds = cfg.Num("design.n27.pulse27.boostSeconds");
            int boostSteps = Math.Max(1, MovementMath.StepsFor(boostSeconds, cfg.Dt));
            foreach (object ro in L(Get(camp, "rooms")))
            {
                var rj = O(ro);
                if (!(Get(rj, "blastGap") is Dictionary<string, object> bg)) continue;
                Vec3 from = V(bg["from"]);
                double gap = N(bg["gap"]), lip = from.z - 1;
                bool jump = Cross(new Mover(cfg.movement, world), cfg.Dt, from.x, lip, gap, true, false, false, boostSpeed, boostSteps, false, out double rJump);
                bool dash = Cross(new Mover(cfg.movement, world), cfg.Dt, from.x, lip, gap, false, true, false, boostSpeed, boostSteps, false, out double rDash);
                bool jumpDash = Cross(new Mover(cfg.movement, world), cfg.Dt, from.x, lip, gap, true, true, false, boostSpeed, boostSteps, false, out double rJumpDash);
                bool blast = Cross(new Mover(cfg.movement, world), cfg.Dt, from.x, lip, gap, true, false, true, boostSpeed, boostSteps, false, out double rBlast);
                bool ok = blast && !jump && !dash && !jumpDash;
                rows.Add(($"blast gap: {S(rj, "id")} ({F(gap)} m)", ok,
                    $"the real Mover at the lip: a running jump {(jump ? "CROSSES" : "falls short")} (reach {F(rJump)} m), a dash {(dash ? "CROSSES" : "falls short")} ({F(rDash)} m), a jump then a dash {(jumpDash ? "CROSSES" : "falls short")} ({F(rJumpDash)} m), a jump with the PULSE blast (+{F(boostSpeed)} m/s over {F(boostSeconds)} s) {(blast ? "lands on the far side" : "does NOT cross")} ({F(rBlast)} m): {(ok ? "only the blast crosses" : "the gap does not force the blast")}"));
            }
        }

        // ---------------------------------------------------------------- lights
        static void Lights(double maxDepth, Dictionary<string, object> camp, LevelDef level, Dictionary<string, RoomDef> rooms, GameWorld world, List<(string check, bool pass, string detail)> rows)
        {
            var bad = new List<string>();
            var ids = new HashSet<string>();
            var perRoom = new Dictionary<string, int>();
            var accent = new Dictionary<string, int>();
            var pools = new Dictionary<string, bool>();
            List<object> lights = L(Get(camp, "lights"));
            foreach (object lo in lights)
            {
                var l = O(lo);
                string id = S(l, "id");
                string tag = id == "" ? "(no id)" : id;
                if (id == "" || !ids.Add(id)) { bad.Add($"{tag}: missing or duplicate id"); continue; }
                string kind = S(l, "kind"), color = S(l, "color"), room = S(l, "room");
                if (Array.IndexOf(Kinds, kind) < 0) { bad.Add($"{tag}: kind '{kind}' is not wall|ceiling|pool"); continue; }
                if (Array.IndexOf(Colors, color) < 0) { bad.Add($"{tag}: colour '{color}' is not cyan|green|amber|white|orange"); continue; }
                if (!rooms.TryGetValue(room, out RoomDef rd)) { bad.Add($"{tag}: room '{room}' does not exist"); continue; }
                if (!Vec3Ok(Get(l, "at")) || !Vec3Ok(Get(l, "normal")) || !Vec3Ok(Get(l, "size"))) { bad.Add($"{tag}: at / normal / size must be [x, y, z]"); continue; }
                if (Get(l, "range") == null || N(l["range"]) <= 0 || Get(l, "intensity") == null || N(l["intensity"]) <= 0) { bad.Add($"{tag}: range and intensity must be above 0"); continue; }
                Vec3 at = V(l["at"]), n = V(l["normal"]), size = V(l["size"]);
                bool collider = Get(l, "collider") is bool cb && cb;
                double nl = Math.Sqrt(n.x * n.x + n.y * n.y + n.z * n.z);
                int axis = Math.Abs(n.x) > 0.99 * nl ? 0 : Math.Abs(n.y) > 0.99 * nl ? 1 : Math.Abs(n.z) > 0.99 * nl ? 2 : -1;
                if (axis < 0) { bad.Add($"{tag}: the normal must run along one axis"); continue; }
                bool kindOk = kind == "ceiling" ? axis == 1 && n.y < 0 : kind == "pool" ? axis == 1 && n.y > 0 : axis != 1;
                if (!kindOk) { bad.Add($"{tag}: a {kind} light's normal ({F(n.x)}, {F(n.y)}, {F(n.z)}) does not suit its kind"); continue; }
                if (at.x < rd.x0 - 1e-6 || at.x > rd.x1 + 1e-6 || at.z < rd.z0 - 1e-6 || at.z > rd.z1 + 1e-6) { bad.Add($"{tag}: outside room {room}'s floor plan"); continue; }
                if (size.z > maxDepth + 1e-9 && !collider) { bad.Add($"{tag}: {F(size.z)} m deep (at most {F(maxDepth)} unless it is a collider)"); continue; }
                if (color == "orange" && !(Get(l, "danger") is bool db && db)) { bad.Add($"{tag}: red-orange is danger only (set danger: true)"); continue; }
                // flush: rays along -normal from the centre and the four footprint corners meet a surface half a depth away
                Vec3 u, v; // the two tangent axes
                if (axis == 0) { u = new Vec3(0, 0, 1); v = new Vec3(0, 1, 0); }
                else if (axis == 1) { u = new Vec3(1, 0, 0); v = new Vec3(0, 0, 1); }
                else { u = new Vec3(1, 0, 0); v = new Vec3(0, 1, 0); }
                double sign = axis == 0 ? Math.Sign(n.x) : axis == 1 ? Math.Sign(n.y) : Math.Sign(n.z);
                var back = new Vec3(axis == 0 ? -sign : 0, axis == 1 ? -sign : 0, axis == 2 ? -sign : 0);
                // size is [w, h, d]: w along u, h along v, d along the normal
                double tw = size.x * 0.45, th = size.y * 0.45;
                double half = size.z / 2;
                bool flush = true;
                string why = "";
                foreach (var (a, b2) in new[] { (0.0, 0.0), (tw, th), (tw, -th), (-tw, th), (-tw, -th) })
                {
                    var o = new Vec3(at.x + u.x * a + v.x * b2, at.y + u.y * a + v.y * b2, at.z + u.z * a + v.z * b2);
                    if (!Hit(world, o, back, out double dist)) { flush = false; why = "no surface behind it"; break; }
                    if (Math.Abs(dist - half) > 0.02) { flush = false; why = $"the surface is {F(dist)} m behind its centre (its back face is {F(half)})"; break; }
                }
                if (!flush) { bad.Add($"{tag}: not flush to a surface: {why}"); continue; }
                perRoom[room] = perRoom.TryGetValue(room, out int pc) ? pc + 1 : 1;
                if (color == "amber" || color == "orange") accent[room] = accent.TryGetValue(room, out int ac) ? ac + 1 : 1;
                if (kind == "pool" && JsMath.Hypot(at.x - rd.exit.x, at.z - rd.exit.z) <= PoolReach) pools[room] = true;
            }
            foreach (RoomDef r in level.rooms)
            {
                if (r.kind == "boss") continue;
                if (!perRoom.ContainsKey(r.id)) bad.Add($"room {r.id} has no light");
                if (accent.TryGetValue(r.id, out int a) && a > 1) bad.Add($"room {r.id} has {a} accent lights (amber / orange): one accent per room");
                if (!pools.ContainsKey(r.id)) bad.Add($"room {r.id} has no pool light within {F(PoolReach)} m of its exit (the goal's pool)");
            }
            rows.Add(("lights", bad.Count == 0, bad.Count == 0 ? $"{lights.Count} light sockets in {perRoom.Count} rooms: each flush to a surface by rays (back face on the surface, at most {F(maxDepth)} m deep), palette colours only, one accent per room, a pool at every goal" : string.Join("; ", bad)));
        }

        // ---------------------------------------------------------------- props
        static void Props(double maxDepth, Dictionary<string, object> camp, LevelDef level, Dictionary<string, RoomDef> rooms, Dictionary<string, Dictionary<string, object>> roomJson, GameWorld world, List<(string check, bool pass, string detail)> rows)
        {
            var bad = new List<string>();
            var ids = new HashSet<string>();
            List<object> props = L(Get(camp, "props"));
            var planBoxes = new List<(Vec3 min, Vec3 max, string zone)>();
            foreach (object bo in L(Get(camp, "boxes")))
            {
                var b = O(bo);
                planBoxes.Add((V(b["min"]), V(b["max"]), S(b, "zone")));
            }
            int colliders = 0;
            foreach (object po in props)
            {
                var p = O(po);
                string id = S(p, "id");
                string tag = id == "" ? "(no id)" : id;
                if (id == "" || !ids.Add(id)) { bad.Add($"{tag}: missing or duplicate id"); continue; }
                if (p.ContainsKey("random") || p.ContainsKey("scatter") || p.ContainsKey("seed")) { bad.Add($"{tag}: a prop is authored, never scattered"); continue; }
                if (!rooms.TryGetValue(S(p, "room"), out RoomDef rd)) { bad.Add($"{tag}: room '{S(p, "room")}' does not exist"); continue; }
                if (!Vec3Ok(Get(p, "at")) || !Vec3Ok(Get(p, "size")) || S(p, "kind") == "" || !(Get(p, "collider") is bool)) { bad.Add($"{tag}: needs kind, at [x, y, z], size [w, h, d] and collider true|false"); continue; }
                bool collider = (bool)p["collider"];
                LevelPlanBox(p, out Vec3 mn, out Vec3 mx);
                // NIGHT33 K (dress33's decor, K1): a `decor: true` row is view only: never a collider; it may stand in the room's wall ring or a band
                // behind it (a chimney, a landmark: UnityTuning.Kit33DecorOutside) and be thicker than a light socket (a truss, a pilaster). Its own
                // rules (density, wall coverage, lanes, palette) are the harness's `kit33` checks. Rows without the key: NIGHT32's checks below.
                if (Get(p, "decor") is bool dc && dc)
                {
                    double m = UnityTuning.Kit33DecorOutside;
                    if (collider) bad.Add($"{tag}: a decor row never collides (cover is a collider row without `decor`)");
                    else if (mn.x < rd.x0 - m || mx.x > rd.x1 + m || mn.z < rd.z0 - m || mx.z > rd.z1 + m) bad.Add($"{tag}: decor more than {F(m)} m outside room {rd.id}");
                    continue;
                }
                if (mn.x < rd.x0 - 1e-6 || mx.x > rd.x1 + 1e-6 || mn.z < rd.z0 - 1e-6 || mx.z > rd.z1 + 1e-6) { bad.Add($"{tag}: leaves room {rd.id}'s floor plan"); continue; }
                double roomH = Get(roomJson[rd.id], "height") != null ? N(roomJson[rd.id]["height"]) : double.NaN;
                if (!double.IsNaN(roomH) && mx.y > roomH + 1e-6) { bad.Add($"{tag}: taller than room {rd.id}"); continue; }
                double thin = Math.Min(Math.Min(mx.x - mn.x, mx.y - mn.y), mx.z - mn.z);
                if (collider)
                {
                    colliders++;
                    // support: the floor (base 0), the top of a plan box of this room, or hung from the roof
                    bool onFloor = Math.Abs(mn.y) <= 0.05;
                    bool onBox = false;
                    foreach (var (bmin, bmax, zone) in planBoxes)
                        if (zone == rd.id && Math.Abs(mn.y - bmax.y) <= 0.05 && (mn.x + mx.x) / 2 >= bmin.x && (mn.x + mx.x) / 2 <= bmax.x && (mn.z + mx.z) / 2 >= bmin.z && (mn.z + mx.z) / 2 <= bmax.z) onBox = true;
                    bool hung = !double.IsNaN(roomH) && Math.Abs(mx.y - roomH) <= 0.05;
                    if (!onFloor && !onBox && !hung) { bad.Add($"{tag}: a collider prop floats (base y {F(mn.y)}: not on the floor, a plan box top or the roof)"); continue; }
                    var cl = new List<Vec3> { rd.spawn, rd.exit };
                    cl.AddRange(rd.nodes);
                    bool blocked = false;
                    foreach (Vec3 c in cl)
                        if (c.x > mn.x - 0.6 && c.x < mx.x + 0.6 && c.z > mn.z - 0.6 && c.z < mx.z + 0.6 && c.y < mx.y && c.y + 1.8 > mn.y) blocked = true;
                    if (blocked) bad.Add($"{tag}: stands on a spawn, the exit or an encounter node");
                }
                else if (Get(p, "pickup") is bool pk && pk)
                {
                    // a pickup is meant to be walked through: it floats a little above a floor or deck (a ray down from it meets a surface within 1.5 m)
                    Vec3 pc = new Vec3((mn.x + mx.x) / 2, (mn.y + mx.y) / 2, (mn.z + mx.z) / 2);
                    if (!Hit(world, pc, new Vec3(0, -1, 0), out double pd) || pd > 1.5) bad.Add($"{tag}: a pickup with no floor within 1.5 m under it");
                }
                else
                {
                    if (thin > maxDepth + 1e-9) { bad.Add($"{tag}: {F(thin)} m thick with no collider (a prop you can run through is flat, at most {F(maxDepth)} m, else it needs a collider)"); continue; }
                    // flat on a surface: a ray down from the centre meets the floor / deck at its base, or a wall is within the thin side
                    Vec3 c = new Vec3((mn.x + mx.x) / 2, (mn.y + mx.y) / 2, (mn.z + mx.z) / 2);
                    bool mounted = false;
                    foreach (Vec3 dir in new[] { new Vec3(0, -1, 0), new Vec3(0, 1, 0), new Vec3(1, 0, 0), new Vec3(-1, 0, 0), new Vec3(0, 0, 1), new Vec3(0, 0, -1) })
                        if (Hit(world, c, dir, out double dist) && dist <= thin / 2 + 0.05) mounted = true;
                    if (!mounted) bad.Add($"{tag}: a flat prop with no surface under or behind it (it floats)");
                }
            }
            rows.Add(("props", bad.Count == 0, bad.Count == 0 ? $"{props.Count} authored props ({colliders} with colliders) in their rooms: unique ids, each on a surface, no scatter, no loose prop thicker than {F(maxDepth)} m without a collider, no collider on a spawn, exit or node" : string.Join("; ", bad)));
        }

        static void LevelPlanBox(Dictionary<string, object> p, out Vec3 min, out Vec3 max)
        {
            List<object> at = L(p["at"]), sz = L(p["size"]);
            double yaw = Get(p, "yawDeg") != null ? N(p["yawDeg"]) * Math.PI / 180 : 0;
            double c = Math.Abs(JsMath.Cos(yaw)), s = Math.Abs(JsMath.Sin(yaw));
            double w = N(sz[0]), h = N(sz[1]), d = N(sz[2]);
            double ex = (w * c + d * s) / 2, ez = (w * s + d * c) / 2;
            min = new Vec3(N(at[0]) - ex, N(at[1]) - h / 2, N(at[2]) - ez);
            max = new Vec3(N(at[0]) + ex, N(at[1]) + h / 2, N(at[2]) + ez);
        }

        // ---------------------------------------------------------------- doors
        static void DoorFit(double tol, double bodyH, Dictionary<string, object> camp, LevelDef level, Dictionary<string, RoomDef> rooms, Dictionary<string, Dictionary<string, object>> roomJson, GameWorld world, List<(string check, bool pass, string detail)> rows)
        {
            var bad = new List<string>();
            var doorsJson = L(Get(camp, "doors"));
            foreach (object dobj in doorsJson)
            {
                var dj = O(dobj);
                string id = S(dj, "id");
                List<object> between = L(dj["between"]);
                RoomDef a = rooms[(string)between[0]], b = rooms[(string)between[1]];
                List<object> rr = L(dj["rect"]);
                double dx0 = N(rr[0]), dz0 = N(rr[1]), dx1 = N(rr[2]), dz1 = N(rr[3]);
                // the wall axis: the rooms are apart along z or along x
                double gapZ = Math.Max(a.z0 - b.z1, b.z0 - a.z1), gapX = Math.Max(a.x0 - b.x1, b.x0 - a.x1);
                bool alongZ = gapZ >= gapX; // the door's depth runs along z
                double gap = alongZ ? gapZ : gapX;
                double depth = alongZ ? dz1 - dz0 : dx1 - dx0;
                double c0 = alongZ ? dx0 : dz0, c1 = alongZ ? dx1 : dz1; // the door's width across the wall
                if (Math.Abs(depth - gap) > tol) { bad.Add($"{id}: its depth {F(depth)} m is not the gap {F(gap)} m between {a.id} and {b.id}"); continue; }
                if (Math.Abs(gap - 2 * WallThick) > tol) { bad.Add($"{id}: the gap {F(gap)} m between the rooms is not two wall thicknesses: the wall rings do not meet"); continue; }
                double a0 = alongZ ? a.x0 : a.z0, a1 = alongZ ? a.x1 : a.z1, b0 = alongZ ? b.x0 : b.z0, b1 = alongZ ? b.x1 : b.z1;
                if (c0 < Math.Max(a0, b0) - tol || c1 > Math.Min(a1, b1) + tol) { bad.Add($"{id}: not inside both rooms' extent along the wall"); continue; }
                // the opening: rays sideways from the door's middle at three depths meet the wall faces at the rect's edges
                double mid = (c0 + c1) / 2;
                double[] ds = { 0.25, 0.5, 0.75 };
                string rayBad = "";
                foreach (double f in ds)
                {
                    double along = (alongZ ? dz0 : dx0) + depth * f;
                    for (int side = 0; side < 2 && rayBad == ""; side++)
                    {
                        double sgn = side == 0 ? -1 : 1;
                        Vec3 o = alongZ ? new Vec3(mid, 1, along) : new Vec3(along, 1, mid);
                        Vec3 dir = alongZ ? new Vec3(sgn, 0, 0) : new Vec3(0, 0, sgn);
                        double want = (c1 - c0) / 2;
                        if (!Hit(world, o, dir, out double dist)) rayBad = $"no wall on the {(side == 0 ? "low" : "high")} side";
                        else if (Math.Abs(dist - want) > tol) rayBad = $"the wall face is {F(dist)} m from the door's middle on the {(side == 0 ? "low" : "high")} side (the rect's half width is {F(want)}): a gap or an overlap";
                    }
                }
                if (rayBad != "") { bad.Add($"{id}: {rayBad}"); continue; }
                if (Get(dj, "height") == null) { bad.Add($"{id}: no `height` (the clear opening the frame is built from)"); continue; }
                double dh = N(dj["height"]);
                if (dh < bodyH + 0.2) { bad.Add($"{id}: opening {F(dh)} m high is lower than the body ({F(bodyH)} m) plus 0.2"); continue; }
                string upBad = "";
                foreach (double f in new[] { 0.25, 0.75 }) // both halves of the opening (each under its own room's roof, or the lintel)
                {
                    Vec3 up = alongZ ? new Vec3(mid, 0.1, dz0 + depth * f) : new Vec3(dx0 + depth * f, 0.1, mid);
                    if (!Hit(world, up, new Vec3(0, 1, 0), out double hh)) { upBad = "nothing above the doorway (a sky gap)"; break; }
                    if (Math.Abs(hh + 0.1 - dh) > tol) { upBad = $"the collider's opening is {F(hh + 0.1)} m high, the plan's `height` is {F(dh)}"; break; }
                }
                if (upBad != "") bad.Add($"{id}: {upBad}");
            }
            rows.Add(("doorFit", bad.Count == 0, bad.Count == 0 ? $"{doorsJson.Count} doors: each rect is exactly the gap between its rooms (gap 0, wall rings meet), inside both rooms, the wall faces by ray stand at the rect's edges (tolerance {F(tol)}), the clear height matches the lintel or roof" : string.Join("; ", bad)));
        }

        // ---------------------------------------------------------------- closed rooms
        static void ClosedRoom(LevelDef level, Dictionary<string, Dictionary<string, object>> roomJson, GameWorld world, List<(string check, bool pass, string detail)> rows)
        {
            var bad = new List<string>();
            int samples = 0, rooms = 0;
            foreach (RoomDef r in level.rooms)
            {
                if (r.kind == "boss") continue;
                var rj = roomJson[r.id];
                if (!(Get(rj, "roof") is bool roof) || !roof) { bad.Add($"{r.id}: no `roof: true`"); continue; }
                if (Get(rj, "height") == null) { bad.Add($"{r.id}: no `height`"); continue; }
                double h = N(rj["height"]);
                rooms++;
                int miss = 0;
                double firstX = 0, firstZ = 0;
                for (double x = r.x0 + 1; x < r.x1; x += 2)
                    for (double z = r.z0 + 1; z < r.z1; z += 2)
                    {
                        samples++;
                        bool ok = Hit(world, new Vec3(x, h - 0.3, z), new Vec3(0, 1, 0), out double dist) && dist <= 0.35;
                        if (!ok) { if (miss == 0) { firstX = x; firstZ = z; } miss++; }
                    }
                if (miss > 0) bad.Add($"{r.id}: {miss} floor samples have no roof within 0.35 m above y {F(h - 0.3)} (first at x {F(firstX)}, z {F(firstZ)})");
            }
            rows.Add(("closedRoom", bad.Count == 0, bad.Count == 0 ? $"{rooms} rooms with roof: true and a height: {samples} rays up from just under the roof (every 2 m of the floor) all meet it, the roof lies on the ring walls (LevelPlan29)" : string.Join("; ", bad)));
        }
    }
}
