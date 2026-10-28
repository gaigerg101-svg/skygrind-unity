// NIGHT27 (builder A): the arena rules of the level report, added to LevelValidate24's rows when the plan carries `"n27": {"arenas": true}`
// (copied into the map JSON's `campaign` block by LevelPlan24). Pure (no UnityEngine). The numbers are design.n27.budget (Design/unity-config.json),
// never typed here.
//   - SIZE: each combat room is at most roomMaxWidth wide (short axis) x roomMaxLength long, wallHeight in [roomMinHeight, roomMaxHeight]
//   - BUDGET: a wave's cost (husk, swarmer, spitter, heavy) is at most waveBase + wavePerIndex x its index in the room; a wave has at most
//     maxWaveSize enemies, at most 2 kinds (3 when it holds a heavy)
//   - KILLS: all the level's waves together are killsMin..killsMax enemies
//   - LIMITS: a room's LIMIT flag needs a `limitReason` (the banner's words); none in a rebuilt level's rooms otherwise
//   - WALL LANES: both long walls carry a lane (a stretch at least stripMinLength long with nothing taller than 0.5 m within laneClear of the wall);
//     the wall route (floor leg in at dash then walk speed, the ride at wallLaneSpeed with a wall jump penalty per maxTime, the floor leg out) must be
//     at least wallFasterPct percent faster than the floor route (RIVAL's graph, spawn to exit); both times and the ratio are printed
//   - HAZARD: a room's `hazard` strip lies inside the room
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game.MapAuthoring
{
    public static class LevelValidate27
    {
        static Dictionary<string, object> O(object o) => o as Dictionary<string, object>;
        static List<object> L(object o) => o as List<object> ?? new List<object>();
        static double N(object o) => PortData.Num(o);
        static object Get(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out object v) ? v : null;
        static string F(double v) => (Math.Round(v * 100) / 100).ToString("0.##", CultureInfo.InvariantCulture);

        /// <summary>The plan asks for the NIGHT27 arena rules.</summary>
        public static bool Wanted(Dictionary<string, object> map) => Get(O(Get(O(map["campaign"]), "n27")), "arenas") is bool b && b;

        static double Cost(GameConfig cfg, string kind)
        {
            switch (kind)
            {
                case "husk": return cfg.Num("design.n27.budget.costHusk");
                case "swarmer": return cfg.Num("design.n27.budget.costSwarmer");
                case "spitter": return cfg.Num("design.n27.budget.costSpitter");
                case "heavy": return cfg.Num("design.n27.budget.costHeavy");
                case "tank": return cfg.Num("design.n27.budget.costHeavy"); // NIGHT29: the tank costs a heavy (6): no new number
                default: return double.NaN;
            }
        }

        static double LegTime(double d, double walk, double dashSpeed, double dashDur)
        {
            double dashDist = dashSpeed * dashDur;
            return d <= dashDist ? d / dashSpeed : dashDur + (d - dashDist) / walk;
        }

        public static void Rows(GameConfig cfg, Dictionary<string, object> map, LevelDef level, NavGraph graph, List<(string check, bool pass, string detail)> rows, List<string> printed)
        {
            if (!Wanted(map)) return;
            var camp = O(map["campaign"]);
            double wallH = camp.ContainsKey("wallHeight") ? N(camp["wallHeight"]) : 14;
            double maxW = cfg.Num("design.n27.budget.roomMaxWidth"), maxL = cfg.Num("design.n27.budget.roomMaxLength");
            double minH = cfg.Num("design.n27.budget.roomMinHeight"), maxH = cfg.Num("design.n27.budget.roomMaxHeight");
            double wBase = cfg.Num("design.n27.budget.waveBase"), wPer = cfg.Num("design.n27.budget.wavePerIndex");
            int maxWave = (int)cfg.Num("design.n27.budget.maxWaveSize");
            double fasterPct = cfg.Num("design.n27.budget.wallFasterPct"), laneSpeed = cfg.Num("design.n27.budget.wallLaneSpeed");
            double segPen = cfg.Num("design.n27.budget.wallLaneSegmentPenalty"), laneClear = cfg.Num("design.n27.budget.laneClear");
            double walk = cfg.Num("ground.walkSpeed"), dashSpeed = cfg.Num("dash.speed"), dashDur = cfg.Num("dash.duration");
            double maxRide = cfg.Num("wallRide.maxTime"), minLane = cfg.Num("design.n26.wallRide.stripMinLength");
            int totalKills = 0;
            var path = new NavPath();
            var boxes = new List<(Vec3 min, Vec3 max)>();
            foreach (object b in L(O(map["data"])["boxes"]))
            {
                var d = O(b);
                if ((Get(d, "kind") as string) == "floor") continue;
                var mn = L(d["min"]);
                var mx = L(d["max"]);
                boxes.Add((new Vec3(N(mn[0]), N(mn[1]), N(mn[2])), new Vec3(N(mx[0]), N(mx[1]), N(mx[2]))));
            }

            foreach (RoomDef r in level.rooms)
            {
                var rj = RoomJson(camp, r.id);
                double w = r.x1 - r.x0, l = r.z1 - r.z0;
                bool combat = r.kind == "room" && r.waves.Count > 0;
                var lim = L(Get(rj, "limit"));
                if (lim.Count > 0)
                {
                    string reason = Get(rj, "limitReason") as string ?? "";
                    rows.Add(($"n27 limits: {r.id}", reason.Trim().Length > 0, reason.Trim().Length > 0 ? $"limit {Limit24.Banner(r.limits)} kept, the banner says why: \"{reason}\"" : $"limit {Limit24.Banner(r.limits)} has no limitReason (a random limit: GG removed them)"));
                }
                if (r.kind != "room" && r.kind != "link") { continue; }
                bool longZ = l >= w;
                double shortAx = Math.Min(w, l), longAx = Math.Max(w, l);
                if (combat)
                {
                    rows.Add(($"n27 size: {r.id}", shortAx <= maxW + 1e-9 && longAx <= maxL + 1e-9 && wallH >= minH && wallH <= maxH,
                        $"{F(shortAx)} m wide x {F(longAx)} m long (at most {F(maxW)} x {F(maxL)}), {F(wallH)} m tall (between {F(minH)} and {F(maxH)})"));
                    var bad = new List<string>();
                    var detail = new List<string>();
                    for (int wi = 0; wi < r.waves.Count; wi++)
                    {
                        double budget = wBase + wPer * wi, cost = 0;
                        var kinds = new List<string>();
                        bool heavy = false, unknown = false;
                        foreach (string k in r.waves[wi])
                        {
                            double c = Cost(cfg, k);
                            if (double.IsNaN(c)) { unknown = true; continue; }
                            cost += c;
                            if (!kinds.Contains(k)) kinds.Add(k);
                            if (k == "heavy") heavy = true;
                        }
                        detail.Add($"w{wi + 1}: {r.waves[wi].Count} enemies, {kinds.Count} kinds, {F(cost)} of {F(budget)} points");
                        if (unknown) bad.Add($"w{wi + 1} has a kind with no cost");
                        if (cost > budget + 1e-9) bad.Add($"w{wi + 1} costs {F(cost)} over its {F(budget)} points");
                        if (r.waves[wi].Count > maxWave) bad.Add($"w{wi + 1} has {r.waves[wi].Count} enemies (at most {maxWave})");
                        if (kinds.Count > (heavy ? 3 : 2)) bad.Add($"w{wi + 1} mixes {kinds.Count} kinds");
                    }
                    rows.Add(($"n27 budget: {r.id}", bad.Count == 0, bad.Count == 0 ? string.Join("; ", detail) : string.Join("; ", bad)));
                    totalKills += r.EnemyCount;
                }

                if (!combat) continue;
                // wall lanes: the long walls
                double wall0 = longZ ? r.x0 : r.z0, wall1 = longZ ? r.x1 : r.z1; // the two long walls' coordinates (across the long axis)
                double a0 = longZ ? r.z0 : r.x0, a1 = longZ ? r.z1 : r.x1; // the long axis' range
                var laneLen = new double[2];
                var laneFrom = new double[2];
                var laneTo = new double[2];
                for (int side = 0; side < 2; side++)
                {
                    double wc = side == 0 ? wall0 : wall1;
                    double lo = side == 0 ? wc : wc - laneClear, hi = side == 0 ? wc + laneClear : wc;
                    var blocked = new List<(double, double)>();
                    foreach (var (mn, mx) in boxes)
                    {
                        if (mx.y < 0.5) continue;
                        double cx = (mn.x + mx.x) / 2, cz = (mn.z + mx.z) / 2;
                        if (cx < r.x0 || cx > r.x1 || cz < r.z0 || cz > r.z1) continue; // a wall of the ring or another room's box
                        double blo = longZ ? mn.x : mn.z, bhi = longZ ? mx.x : mx.z;
                        if (bhi <= lo || blo >= hi) continue;
                        blocked.Add((longZ ? mn.z : mn.x, longZ ? mx.z : mx.x));
                    }
                    blocked.Sort((p, q) => p.Item1.CompareTo(q.Item1));
                    double cur = a0, bestLen = 0, bestFrom = a0, bestTo = a0;
                    foreach (var (b0, b1) in blocked)
                    {
                        if (b0 - cur > bestLen) { bestLen = b0 - cur; bestFrom = cur; bestTo = b0; }
                        cur = Math.Max(cur, b1);
                    }
                    if (a1 - cur > bestLen) { bestLen = a1 - cur; bestFrom = cur; bestTo = a1; }
                    laneLen[side] = bestLen; laneFrom[side] = bestFrom; laneTo[side] = bestTo;
                }
                // the floor route: RIVAL's graph, spawn to exit
                int sa = Astar.NearestNode(graph, r.spawn), sb = Astar.NearestNode(graph, r.exit);
                double floorT = sa >= 0 && sb >= 0 && Astar.Path(graph, sa, sb, cfg.movement.dash.speed, path) ? path.cost : double.NaN;
                // the wall route per lane: leg in, ride, leg out
                double bestWall = double.NaN;
                string bestName = "none";
                for (int side = 0; side < 2; side++)
                {
                    if (laneLen[side] < minLane) continue;
                    double wc = side == 0 ? wall0 : wall1;
                    double lx = side == 0 ? wc + 1.2 : wc - 1.2;
                    double sAx = longZ ? r.spawn.z : r.spawn.x, eAx = longZ ? r.exit.z : r.exit.x;
                    double sAcross = longZ ? r.spawn.x : r.spawn.z, eAcross = longZ ? r.exit.x : r.exit.z;
                    // ride from the lane end nearer the spawn to the lane end nearer the exit
                    double p0 = Math.Min(Math.Max(sAx, laneFrom[side]), laneTo[side]); // get on the lane level with the spawn, get off level with the exit
                    double p1 = Math.Min(Math.Max(eAx, laneFrom[side]), laneTo[side]);
                    double inD = Math.Sqrt((sAcross - lx) * (sAcross - lx) + (sAx - p0) * (sAx - p0));
                    double outD = Math.Sqrt((eAcross - lx) * (eAcross - lx) + (eAx - p1) * (eAx - p1));
                    double len = Math.Abs(p1 - p0);
                    int segs = Math.Max(1, (int)Math.Ceiling(len / (laneSpeed * maxRide) - 1e-9));
                    double t = LegTime(inD, walk, dashSpeed, dashDur) + len / laneSpeed + (segs - 1) * segPen + LegTime(outD, walk, dashSpeed, dashDur);
                    if (double.IsNaN(bestWall) || t < bestWall) { bestWall = t; bestName = side == 0 ? "west/north" : "east/south"; }
                }
                bool lanesOk = laneLen[0] >= minLane && laneLen[1] >= minLane;
                double pct = double.IsNaN(floorT) || double.IsNaN(bestWall) ? double.NaN : (floorT - bestWall) / floorT * 100;
                bool fast = !double.IsNaN(pct) && pct >= fasterPct;
                string line = $"lanes {F(laneLen[0])} m and {F(laneLen[1])} m clear (need {F(minLane)}); wall route {F(bestWall)} s ({bestName} lane) vs floor route {F(floorT)} s: ratio {F(floorT / bestWall)}x, the wall is {F(pct)} % faster (need {F(fasterPct)} %)";
                rows.Add(($"n27 wall lane: {r.id}", lanesOk && fast, line));
                printed.Add($"{r.id} {line}");

                var hz = L(Get(rj, "hazard"));
                if (Get(rj, "hazard") is Dictionary<string, object> hd)
                {
                    List<object> hr = L(hd["rect"]);
                    double hx0 = N(hr[0]), hz0 = N(hr[1]), hx1 = N(hr[2]), hz1 = N(hr[3]);
                    bool inside = hx0 >= r.x0 - 1e-9 && hz0 >= r.z0 - 1e-9 && hx1 <= r.x1 + 1e-9 && hz1 <= r.z1 + 1e-9 && hx1 > hx0 && hz1 > hz0;
                    rows.Add(($"n27 hazard: {r.id}", inside, inside ? $"strip x {F(hx0)}..{F(hx1)}, z {F(hz0)}..{F(hz1)} lies inside the room (telegraph {F(cfg.Num("design.n27.hazardFloor27.telegraphSeconds"))} s, live {F(cfg.Num("design.n27.hazardFloor27.liveSeconds"))} s, {F(cfg.Num("design.n27.hazardFloor27.damagePerSecond"))} HP/s)" : "the hazard strip is not inside the room"));
                }
            }
            if (Get(O(Get(camp, "n27")), "noKills") is bool nk && nk) return; // a boss level (level 2): the chase is the level, not the kill count
            int kMin = (int)cfg.Num("design.n27.budget.killsMin"), kMax = (int)cfg.Num("design.n27.budget.killsMax");
            rows.Add(("n27 kills per level", totalKills >= kMin && totalKills <= kMax, $"{totalKills} enemies in all the waves (between {kMin} and {kMax})"));
            printed.Add($"kills per level: {totalKills}");
        }

        static Dictionary<string, object> RoomJson(Dictionary<string, object> camp, string id)
        {
            foreach (object ro in L(Get(camp, "rooms")))
            {
                var rd = O(ro);
                if (rd != null && Get(rd, "id") is string s && s == id) return rd;
            }
            return null;
        }
    }
}
