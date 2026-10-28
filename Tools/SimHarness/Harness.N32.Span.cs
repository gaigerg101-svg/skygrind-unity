// NIGHT32 (builder S, DESIGN32 card S, row design.n32.span32): level 4 rebuilt (Design/levels/n32-span.json). Dispatched by the lead's Program.cs:
// N32Span(cmd, args, ref code).
//   span32 walk [--seeds n] [--first n] [--cap seconds] [--trace]
//     walk29's pilot (Walk29Pilot: the autopilot's fights, Span30Pilot's grapple crossings, the lava guard, the unstick) on Design/levels/n32-span.json
//     through the REAL sim with the design config as it is: per room cleared k of n, median seconds, deaths; one SUMMARY line; exit 1 unless every seed
//     clears every playable room (the boss room is builder B's).
//   span32 rings [--level path] [--rooms s2,s3] [--trace]
//     every ring route of the level (the rooms' `ringRoute32` rows, Tools/night/gen-n32-span.cjs) flown with the REAL pull (design.n31.grapplePull: a TAP
//     aimed at the ring, the Mover's zip, the arrival clamp and pop) from the route's standing point, the forward stick held toward the next ring after each
//     arrival (a wall ride counts when the Mover is in WallRide). Around the real pull the checker SIMULATES DESIGN32 G's charge rule (2 charges, one back
//     every 4 s, >= 0.8 s between pulls, landing never refills, range 40 m, a ring in sight), because G's grapple32 code is not in this tree (port/stage3 at
//     5b89bc20): a press waits for a charge and the 0.8 s gap; a press with no charge left, a ring out of range or out of sight, three pulls without ground,
//     a wall ride or a kill, or a landing outside the route's goal (or in lava) is RED. Prints one line per route and a SUMMARY; exit 1 on a RED.
//     The charge numbers are the checker's copy of D-1 (DECIDE32.md), not gameplay: G's row design.n32.grapple32 owns the game's.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N32Span(string cmd, string[] args, ref int code)
    {
        if (cmd != "span32") return;
        code = 0;
        string sub = args.Length > 1 ? args[1] : "walk";
        if (sub == "walk") { Span32Walk(args, ref code); return; }
        if (sub == "rings") { Span32Rings(args, ref code); return; }
        if (sub == "watch") { Span32Watch(args); return; }
        if (sub == "deaths") { Span32Deaths(args, ref code); return; }
        Console.WriteLine("span32: walk | rings | watch <room> [--seed n] | deaths [--seeds n]");
        code = 1;
    }

    static void Span32Walk(string[] args, ref int code)
    {
        var a = new List<string> { "walk29", "span" };
        for (int i = 2; i < args.Length; i++) a.Add(args[i]);
        string old = walkPrefix;
        walkPrefix = "n32"; // Design/levels/n32-span.json
        Console.WriteLine("span32 walk: walk29's pilot on Design/levels/n32-span.json (the design config as it is)");
        try { N29Walk("walk29", a.ToArray(), ref code); }
        finally { walkPrefix = old; }
    }

    /// <summary>`span32 watch <room> [--seed n]`: one walk29 run of n32-span (the same pilot and setup as `span32 walk`) printing, every 0.5 s while the
    /// campaign is in that room, the player's position, state, health, the alive count and the campaign phase (a debugging aid for the room's deaths).</summary>
    static void Span32Watch(string[] args)
    {
        string room = args.Length > 2 ? args[2] : "s3";
        uint seed = uint.Parse(Opt30(args, "--seed", "1"), CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        string path = Path.Combine("Design", "levels", "n32-span.json");
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var ap = new AutopilotInput(sim, "normal", seed);
        ap.GoalProvider = () =>
        {
            Campaign24 c = sim.campaign;
            LevelDef lv = sim.campaignLevel;
            if (c == null || c.room < 0) return lv.rooms[0].spawn;
            if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
            return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
        };
        sim.UseInput(new Walk29Pilot(sim, ap));
        sim.events.On<RoomEvent>("roomRetry", e => Console.WriteLine($"  RETRY room {e.id} at {sim.run.steps * sim.step:0.0} s, at ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.0}, {sim.mover.position.z:0.0})"));
        sim.StartRun(seed, null, null);
        int half = MovementMath.StepsFor(0.5, sim.step), max = MovementMath.StepsFor(900, sim.step);
        bool seen = false;
        for (int st = 0; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
        {
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            Campaign24 c = sim.campaign;
            if (c == null || c.room < 0) continue;
            string id = sim.campaignLevel.rooms[c.room].id;
            if (id != room) { if (seen) break; continue; }
            seen = true;
            if (st % half != 0) continue;
            Vec3 p = sim.mover.position;
            RoomDef rd = sim.campaignLevel.rooms[c.room];
            Console.WriteLine($"  {sim.run.steps * sim.step,6:0.0} s  lx {p.x - (rd.x0 + rd.x1) / 2,6:0.0} lz {rd.z1 - p.z,5:0.0} y {p.y,5:0.0}  {sim.mover.state} hp {sim.health.hp:0} alive {sim.enemies.AliveCount} {c.phase}");
        }
        ap.Detach();
    }

    /// <summary>`span32 deaths [--seeds n] [--first n]`: walk29's pilot on n32-span (as `span32 walk`), and for every room death (roomRetry) its cause:
    /// LAVA when the feet were in a lava zone in the last 1.5 s before it, else FIGHT; per room the deaths, lava deaths, the longest single stay in the
    /// lava (s) and the lava entries. A long stay is a body that could not get out (the pilot's lava handling); the level report's pit-recovery row
    /// proves a player can walk and jump out of every lava cell.</summary>
    static void Span32Deaths(string[] args, ref int code)
    {
        int seeds = int.Parse(Opt30(args, "--seeds", "10"), CultureInfo.InvariantCulture), first = int.Parse(Opt30(args, "--first", "1"), CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        string path = Path.Combine("Design", "levels", "n32-span.json");
        var deaths = new SortedDictionary<string, int[]>(); // room -> [deaths, lava deaths, entries, longest stay steps]
        int stalls = 0;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.GoalProvider = () =>
            {
                Campaign24 c = sim.campaign;
                LevelDef lv = sim.campaignLevel;
                if (c == null || c.room < 0) return lv.rooms[0].spawn;
                if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
            };
            sim.UseInput(new Walk29Pilot(sim, ap));
            int lastLava = -100000, stay = 0;
            bool inLava = false;
            int stepNow = 0;
            int[] Row(string id) { if (!deaths.TryGetValue(id, out int[] r)) deaths[id] = r = new int[4]; return r; }
            sim.events.On<RoomEvent>("roomRetry", e =>
            {
                int[] r = Row(e.id);
                r[0]++;
                if (stepNow - lastLava <= MovementMath.StepsFor(1.5, sim.step)) r[1]++;
            });
            sim.StartRun(seed, null, null);
            int max = MovementMath.StepsFor(900, sim.step);
            LevelDef level = sim.campaignLevel;
            int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
            for (; stepNow < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); stepNow++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                Campaign24 c = sim.campaign;
                if (c != null && c.clearSteps.Count >= playable) break;
                Vec3 p = sim.mover.position;
                bool lava = false;
                if (sim.lava30 != null) foreach (Lava30Zone z in sim.lava30.zones) if (p.x >= z.x0 && p.x <= z.x1 && p.z >= z.z0 && p.z <= z.z1 && p.y < z.y + 0.6) { lava = true; break; }
                string id = c != null && c.room >= 0 ? level.rooms[c.room].id : "?";
                if (lava) { lastLava = stepNow; if (!inLava) Row(id)[2]++; stay++; Row(id)[3] = Math.Max(Row(id)[3], stay); }
                else if (p.y > -0.2 && sim.mover.grounded) stay = 0; // out of the pit (a knock-up arc over the lava is still the same stay)
                inLava = lava;
            }
            if (sim.campaign == null || sim.campaign.clearSteps.Count < playable) stalls++;
            ap.Detach();
        }
        int total = 0;
        foreach (var kv in deaths)
        {
            total += kv.Value[0];
            Console.WriteLine($"span32 deaths {kv.Key}: {kv.Value[0]} deaths, {kv.Value[1]} in the lava (feet in a lava zone within 1.5 s), {kv.Value[0] - kv.Value[1]} in the fight; lava entries {kv.Value[2]}, longest stay {kv.Value[3] * sim0Step(cfg):0.0} s");
        }
        Console.WriteLine($"span32 deaths SUMMARY: {seeds} seeds, {total} room deaths, {stalls} runs that did not clear every playable room");
        if (stalls > 0) code = 1;
    }

    static double sim0Step(GameConfig cfg) => 1.0 / cfg.movement.loop.simHz;

    // DESIGN32 G / DECIDE32 D-1: the checker's copy of the charge rule (see the header)
    const int S32Charges = 2;
    const double S32Refill = 4.0, S32Gap = 0.8, S32Range = 40.0, S32TapSeconds = 0.05;

    static void Span32Rings(string[] args, ref int code)
    {
        var ci = CultureInfo.InvariantCulture;
        GameConfig cfg = Config();
        string path = Opt30(args, "--level", "Design/levels/n32-span.json");
        string only = Opt30(args, "--rooms", "");
        bool trace = Flag(args, "--trace");
        SurvivalMap map0 = CampaignMapFrom(path, cfg, out var json);
        var camp = PortData.Obj(json["campaign"]);
        int dt = (int)Math.Round(cfg.movement.loop.simHz);
        double step = 1.0 / dt;
        int routes = 0, red = 0;
        foreach (object ro in PortData.Arr(camp["rooms"]))
        {
            var r = PortData.Obj(ro);
            string id = (string)r["id"];
            if (only != "" && Array.IndexOf(only.Split(','), id) < 0) continue;
            if (!r.TryGetValue("ringRoute32", out object rro)) continue;
            var rings = new List<Vec3>();
            if (r.TryGetValue("grappleNodes", out object gn)) foreach (object o in PortData.Arr(gn)) rings.Add(V3(o));
            foreach (object rto in PortData.Arr(rro))
            {
                var rt = PortData.Obj(rto);
                routes++;
                string name = (string)rt["name"];
                Vec3 from = V3(rt["from"]);
                double yawDeg = PortData.Num(rt["yawDeg"]);
                var pulls = new List<int>();
                foreach (object p in PortData.Arr(rt["pulls"])) pulls.Add((int)PortData.Num(p));
                List<object> g = PortData.Arr(rt["goal"]);
                double gx0 = PortData.Num(g[0]), gz0 = PortData.Num(g[1]), gx1 = PortData.Num(g[2]), gz1 = PortData.Num(g[3]), gMinY = PortData.Num(rt["goalMinY"]);

                SurvivalMap map = CampaignMapFrom(path, cfg, out _);
                var si = new ScriptedInput();
                SurvivalSim sim = G31Sim(cfg, map, true, si);
                G31Warm(sim, si, 3232);
                sim.campaign?.SetDoorFree29(true);
                sim.Respawn(from, yawDeg);
                si.SetLook(yawDeg * Math.PI / 180, 0);
                for (int s = 0; s < 30; s++) G31Tick(sim, si, null, false, false);
                int arrivals = 0;
                sim.events.On<GrapplePull31Event>("grappleArrive", e => arrivals++);

                // the simulated charges
                int charges = S32Charges;
                double refillLeft = 0, sinceLast = 1e9;
                int sinceReset = 0, maxRun = 0, rideSteps = 0, steps = 0;
                string why = "";
                bool lava = false;
                int k = 0, pressAt = -1, groundedSteps = 0;
                int maxSteps = 12 * dt;
                double minCharges = charges;
                int minReal = sim.Grapple32On ? sim.grappleCharges32 : -1; // G's grapple32 (merged at f896d9f4): its own charge count, when the row is on
                var log = new List<string>();
                for (; steps < maxSteps; steps++)
                {
                    // the charge clock (one charge back every S32Refill s, one at a time)
                    if (charges < S32Charges) { refillLeft -= step; if (refillLeft <= 0) { charges++; refillLeft = charges < S32Charges ? S32Refill : 0; } }
                    sinceLast += step;
                    Mover m = sim.mover;
                    Vec3 p = m.position;
                    if (m.state == MoveState.WallRide) { rideSteps++; if (sinceReset > 0) log.Add($"ride at {steps * step:0.00} s"); sinceReset = 0; }
                    if (m.grounded && pressAt >= 0 && steps - pressAt > 6 && arrivals >= k) sinceReset = 0;
                    bool pending = k < pulls.Count;
                    Vec3? aim = null;
                    bool held = false, pressed = false;
                    double moveZ = 0;
                    if (pending)
                    {
                        Vec3 ring = rings[pulls[k]];
                        bool previousDone = k == 0 || arrivals >= k; // the last pull arrived (a press before the arrival is a cancel: not this route's line)
                        double dist = (ring - sim.Eye).Length;
                        bool sees = G31Sees(sim, sim.Eye, ring);
                        if (previousDone && sinceLast >= S32Gap && charges > 0 && dist <= S32Range && sees)
                        {
                            aim = ring; held = true; pressed = true;
                            charges--; if (refillLeft <= 0) refillLeft = S32Refill;
                            minCharges = Math.Min(minCharges, charges);
                            sinceLast = 0; sinceReset++; maxRun = Math.Max(maxRun, sinceReset);
                            pressAt = steps;
                            log.Add($"pull {k + 1} (ring {pulls[k]}) at {steps * step:0.00} s from ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), {dist:0.0} m, charges left {charges}");
                            k++;
                        }
                        else if (previousDone && k > 0 && steps - pressAt > 4 * dt) { why = dist > S32Range ? $"ring {pulls[k]} out of range ({dist:0.0} m)" : !sees ? $"ring {pulls[k]} out of sight" : charges <= 0 ? "no charge left" : "stalled"; break; }
                        else if (k > 0 && arrivals >= k) { aim = null; moveZ = 1; LookToward(si, p, ring); } // between pulls: the stick toward the next ring (the ride)
                        else if (k > 0) { aim = rings[pulls[k - 1]]; held = steps - pressAt < Math.Max(1, (int)Math.Round(S32TapSeconds * dt)); } // the pull in flight: keep looking at its ring
                        else if (k == 0 && !(sees && dist <= S32Range)) { why = dist > S32Range ? $"ring {pulls[0]} out of range ({dist:0.0} m) from the start" : $"ring {pulls[0]} out of sight from the start"; break; }
                    }
                    else
                    {
                        // after the last pull: the tap's release, then the stick toward the goal's middle once the pull has arrived
                        if (steps - pressAt < Math.Max(1, (int)Math.Round(S32TapSeconds * dt))) { aim = rings[pulls[k - 1]]; held = true; }
                        else if (arrivals >= k) { moveZ = 1; LookToward(si, p, new Vec3((gx0 + gx1) / 2, p.y, (gz0 + gz1) / 2)); }
                        groundedSteps = m.grounded && arrivals >= k ? groundedSteps + 1 : 0;
                        if (groundedSteps > 0.4 * dt) break;
                    }
                    if (sinceReset > 2) { why = "three pulls without ground, a wall ride or a kill"; break; }
                    if (p.y < -0.3) { lava = true; why = $"in the lava at ({p.x:0.0}, {p.z:0.0})"; break; }
                    if (sim.Grapple32On) minReal = Math.Min(minReal, sim.grappleCharges32);
                    G31Tick(sim, si, aim, held, pressed, false, moveZ);
                }
                Vec3 q = sim.mover.position;
                bool inGoal = q.x >= gx0 - 0.5 && q.x <= gx1 + 0.5 && q.z >= gz0 - 0.5 && q.z <= gz1 + 0.5 && q.y >= gMinY;
                if (why == "" && k < pulls.Count) why = $"only {k} of {pulls.Count} pulls fired";
                if (why == "" && arrivals < pulls.Count) why = $"only {arrivals} of {pulls.Count} pulls arrived";
                if (why == "" && !inGoal) why = $"landed outside the goal at ({q.x:0.0}, {q.y:0.0}, {q.z:0.0})";
                bool ok = why == "" && !lava;
                if (!ok) red++;
                Console.WriteLine($"span32 rings {id} \"{name}\": {(ok ? "GREEN" : "RED: " + why)}; {k} pull(s), {arrivals} arrival(s), at most {maxRun} pulls before ground / ride, charges left at least {minCharges:0} (simulated), {(sim.Grapple32On ? $"grapple32 ON: its own charges left at least {minReal}" : "grapple32 off")}, wall ride {rideSteps * step:0.00} s, came down at ({q.x:0.0}, {q.y:0.0}, {q.z:0.0}) after {steps * step:0.00} s");
                if (trace) foreach (string l in log) Console.WriteLine("    " + l);
            }
        }
        Console.WriteLine($"span32 rings SUMMARY: {routes} ring routes, {routes - red} GREEN, {red} RED (the real pull; the 2-charge rule simulated by the checker: 2 charges, 4 s refill, 0.8 s gap, 40 m, no landing refill)");
        if (red > 0 || routes == 0) code = 1;
    }

    static Vec3 V3(object o)
    {
        List<object> a = PortData.Arr(o);
        return new Vec3(PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2]));
    }

    static void LookToward(ScriptedInput si, Vec3 from, Vec3 to) => si.SetLook(Math.Atan2(-(to.x - from.x), -(to.z - from.z)), 0);
}
