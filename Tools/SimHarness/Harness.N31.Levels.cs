// NIGHT31 (builder L): the level commands (dispatched by the lead's Program.cs: N31Levels(cmd, args, ref code)).
//   walk31 <foundry|iceline|kiln|span> [--kestrel] [walk30's args]
//     walk30 (the walk29 pilot on Design/levels/n30-NAME.json, the design config as it is); --kestrel takes the PULSE gauntlet out of the loadout right after
//     StartRun, as design.n31.unlocks will for level 1 (KESTREL only) until W's unlock row is wired: f9 is then crossed by the west wall lane, not the blast.
//   spawn31 [foundry|iceline|kiln|span ...]
//     the validator's spawn31 rule read from each Design/levels/n30-NAME.json (LevelValidate29.Spawn31's numbers): per combat room the nearest encounter node to
//     the entry (m) and its angle off the entry's facing, the violations; one SUMMARY line; exit 1 on a violation.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N31Levels(string cmd, string[] args, ref int code)
    {
        // the lead (12:02): Program.cs prints "unknown command" when `code` is still < 0 after every partial, so each handler sets it first (0 PASS, 1 a failure)
        if (cmd == "walk31") { code = 0; Walk31(args, ref code); return; }
        if (cmd == "spawn31") { code = 0; Spawn31(args, ref code); return; }
        if (cmd == "pull31") { code = 0; Pull31(args, ref code); return; }
    }

    /// <summary>`pull31 [--level path] [--rooms s2,s3] [--fwd]` (round 3, the lead: "does any gap become trivial with the pull?"): for every grapple node of each
    /// room, a fresh campaign sim (G's G31Sim: the ARCLINE owned, design.n31.grapplePull on) stands at the room's entry (or `from` lz on the centre line when the
    /// room carries pullFrom31), TAPS the grapple at that node once (held 6 steps, G's chain), keeps aiming at it until the pull arrives, then (with --fwd) holds
    /// forward; it runs until 0.4 s grounded or 4 s. Prints the farthest lz reached, where it came down (lz, y; lava when y < -0.3), the arrival speed.
    /// A gap is TRIVIAL when one tap from the entry lands past the room's gap row (`gapLz31`).</summary>
    static void Pull31(string[] args, ref int code)
    {
        var ci = CultureInfo.InvariantCulture;
        GameConfig cfg = Config();
        string path = Opt30(args, "--level", "Design/levels/n30-span.json");
        SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
        LevelDef level = LevelDef.From(json);
        string only = Opt30(args, "--rooms", "");
        bool fwd = Flag(args, "--fwd");
        var gapLz = new Dictionary<string, double>();
        var fromLz = new Dictionary<string, double>();
        if (json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp)
            foreach (object ro in PortData.Arr(camp["rooms"]))
            {
                var r = PortData.Obj(ro);
                string id = (string)r["id"];
                if (r.TryGetValue("gapLz31", out object g)) gapLz[id] = PortData.Num(g);
                if (r.TryGetValue("pullFrom31", out object f)) fromLz[id] = PortData.Num(f);
            }
        int dt = (int)Math.Round(cfg.movement.loop.simHz);
        foreach (RoomDef rd in level.rooms)
        {
            if (rd.kind == "boss" || only != "" && Array.IndexOf(only.Split(','), rd.id) < 0) continue;
            for (int i = 0; i < map.grappleNodes.Count; i++)
            {
                Vec3 node = map.grappleNodes[i];
                if (node.x < rd.x0 || node.x > rd.x1 || node.z < rd.z0 || node.z > rd.z1) continue;
                var si = new ScriptedInput();
                GrapplePull31Rules rules = null;
                string sp = Opt30(args, "--speed", "");
                if (sp != "") { rules = GrapplePull31Rules.From(cfg); rules.speed = double.Parse(sp, ci); } // --speed v: the pull speed swapped for this run (G's grapple31 B does the same)
                SurvivalSim sim = G31Sim(cfg, map, true, si, rules);
                G31Warm(sim, si, 3131);
                sim.campaign?.SetDoorFree29(true);
                double cx = (rd.x0 + rd.x1) / 2;
                Vec3 start = fromLz.TryGetValue(rd.id, out double fl) ? new Vec3(cx, 0, rd.z1 - fl) : rd.spawn;
                sim.Respawn(start, 0);
                for (int s = 0; s < 30; s++) G31Tick(sim, si, null, false, false);
                int arrived = 0;
                double arriveSpeed = -1;
                sim.events.On<GrapplePull31Event>("grappleArrive", e => { arrived++; arriveSpeed = e.speed; });
                double maxLz = -1e9, minY = 1e9;
                int grounded = 0, s2 = 0;
                bool lava = false;
                string keepS = Opt30(args, "--keep", "");
                double keep = keepS != "" ? double.Parse(keepS, ci) : -1;
                bool clamped = false;
                int rideSteps = 0;
                for (; s2 < 4 * dt; s2++)
                {
                    // --keep v (a PROPOSAL for builder G, not the game): the step after the arrival, the horizontal speed is clamped to v m/s from outside the Mover
                    // (as the arrival pop already writes Mover.velocity), to see the levels with a capped carry
                    if (keep >= 0 && arrived > 0 && !clamped)
                    {
                        clamped = true;
                        Vec3 v = sim.mover.velocity;
                        double h = Math.Sqrt(v.x * v.x + v.z * v.z);
                        if (h > keep) sim.mover.velocity = new Vec3(v.x * keep / h, v.y, v.z * keep / h);
                    }
                    bool pullOn = arrived == 0 && s2 < dt;
                    G31Tick(sim, si, pullOn ? node : (Vec3?)null, s2 < 6, s2 == 0, false, arrived > 0 && fwd ? 1 : 0);
                    Vec3 p = sim.mover.position;
                    if (sim.mover.state == MoveState.WallRide) rideSteps++;
                    maxLz = Math.Max(maxLz, rd.z1 - p.z);
                    minY = Math.Min(minY, p.y);
                    if (p.y < -0.3) lava = true;
                    grounded = sim.mover.grounded && s2 > 12 ? grounded + 1 : 0;
                    if (grounded > dt * 0.4 || lava) break;
                }
                Vec3 q = sim.mover.position;
                double landLz = rd.z1 - q.z;
                string verdict = gapLz.TryGetValue(rd.id, out double gl) ? (!lava && landLz >= gl ? $"TRIVIAL (one tap lands past the gap, lz {gl.ToString("0.#", ci)})" : $"one tap does not cross (gap ends at lz {gl.ToString("0.#", ci)})") : "";
                if (verdict.StartsWith("TRIVIAL")) code = 1;
                Console.WriteLine($"pull31 {rd.id} node {i} (lz {(rd.z1 - node.z).ToString("0.0", ci)}, y {node.y.ToString("0.0", ci)}) from lz {(rd.z1 - start.z).ToString("0.0", ci)}{(fwd ? " +fwd" : "")}: arrived {arrived > 0} at {arriveSpeed.ToString("0.0", ci)} m/s, farthest lz {maxLz.ToString("0.0", ci)}, came down at lz {landLz.ToString("0.0", ci)} y {q.y.ToString("0.00", ci)}{(lava ? " IN LAVA" : "")} after {(s2 / (double)dt).ToString("0.00", ci)} s, wall ride {(rideSteps / (double)dt).ToString("0.00", ci)} s {verdict}");
            }
        }
    }

    static void Walk31(string[] args, ref int code)
    {
        bool kestrel = Array.IndexOf(args, "--kestrel") >= 0;
        var rest = new List<string>();
        foreach (string a in args) if (a != "--kestrel") rest.Add(a);
        string old = walkPrefix;
        Action<SurvivalSim> oldHook = walkAfterStart;
        walkPrefix = "n30";
        if (kestrel) walkAfterStart = sim => { sim.loadout.owned.Remove("gauntlet"); sim.loadout.owned.Remove("pulse"); };
        Console.WriteLine($"walk31: walk30 with {(kestrel ? "the KESTREL only (--kestrel: the gauntlet removed after StartRun)" : "the design config's loadout")}");
        try { N29Walk("walk29", rest.ToArray(), ref code); }
        finally { walkPrefix = old; walkAfterStart = oldHook; }
    }

    static void Spawn31(string[] args, ref int code)
    {
        var ci = CultureInfo.InvariantCulture;
        GameConfig cfg = Config();
        double far = cfg.Num("design.n24.campaign.minSpawnDist");
        const double MinEntry = 6; // LevelValidate29.Spawn31's layout rules
        double HalfDeg = Skygrind.Game.MapAuthoring.LevelValidate29.SpawnConeHalfDeg(cfg);
        var names = new List<string>();
        for (int i = 1; i < args.Length; i++) if (!args[i].StartsWith("--")) names.Add(args[i]);
        if (names.Count == 0) names.AddRange(new[] { "foundry", "iceline", "kiln", "span" });
        int bad = 0, rooms = 0;
        foreach (string name in names)
        {
            var json = PortData.Obj(PortData.LoadJson(Path.GetFullPath(Path.Combine("Design", "levels", $"n30-{name}.json"))));
            var camp = PortData.Obj(json["campaign"]);
            foreach (object ro in PortData.Arr(camp["rooms"]))
            {
                var r = PortData.Obj(ro);
                if (!(r.TryGetValue("kind", out object k) && (string)k == "room") || !r.TryGetValue("spawn", out object spo)) continue;
                var sp = PortData.Obj(spo);
                List<object> at = PortData.Arr(sp["at"]);
                double sx = PortData.Num(at[0]), sz = PortData.Num(at[2]);
                double yaw = (sp.TryGetValue("yawDeg", out object y) ? PortData.Num(y) : 0) * Math.PI / 180, fx = -Math.Sin(yaw), fz = -Math.Cos(yaw);
                var nodes = new List<object>();
                if (r.TryGetValue("nodes", out object no)) nodes.AddRange(PortData.Arr(no));
                if (r.TryGetValue("highNodes", out object hn)) nodes.AddRange(PortData.Arr(hn));
                if (nodes.Count == 0) continue;
                rooms++;
                double best = double.MaxValue, bestAng = 0;
                int roomBad = 0;
                foreach (object o in nodes)
                {
                    List<object> n = PortData.Arr(o);
                    double dx = PortData.Num(n[0]) - sx, dz = PortData.Num(n[2]) - sz, d = Math.Sqrt(dx * dx + dz * dz);
                    double ang = d > 1e-9 ? Math.Acos(Math.Max(-1, Math.Min(1, (dx * fx + dz * fz) / d))) * 180 / Math.PI : 0;
                    if (d < best) { best = d; bestAng = ang; }
                    if (d < MinEntry - 1e-9 || ang < HalfDeg && d < far - 1e-9) roomBad++;
                }
                bad += roomBad;
                Console.WriteLine($"spawn31 n30-{name} {(string)r["id"]}: {nodes.Count} nodes, nearest {best.ToString("0.0", ci)} m at {bestAng.ToString("0", ci)} deg off the entry's facing, violations {roomBad}");
            }
        }
        Console.WriteLine($"spawn31 SUMMARY: {rooms} combat rooms, {bad} violations (rule: >= {MinEntry.ToString("0", ci)} m from the entry; inside the {(2 * HalfDeg).ToString("0.0", ci)} degree view cone (the camera's width + 5 each side) only at >= {far.ToString("0", ci)} m)");
        if (bad > 0) code = 1;
    }
}
