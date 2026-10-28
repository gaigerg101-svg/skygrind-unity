// NIGHT33 builder R (DESIGN33 card R; rows design.n33.arenas33 / devices33): `arenas33`, the overhauled rooms' checks and the walk bot on the n33 level files.
// Declared and called by the lead's Program.cs (N33Arenas).
//   arenas33 [foundry|iceline|kiln|span|spire|all] [--seeds n] [--nowalk] [--rooms all]
//     per overhauled room (a room row with an `arenas33` block, written by Tools/night/gen-n33-*.cjs; --rooms all: every room) from the PLAN
//     (Design/levels/n33/<name>.plan.json, after K's dressing pass): the area; the heights (the floor plus every deck / slab top >= 0.4 m in the room);
//     the COVER rule (K1 7: every floor point within 8 m of cover; cover = a colliding prop >= 1 m tall or a box whose top is >= 1 m; a 1 m grid
//     over the room's floor, pits and lava skipped); the devices (count, types <= 3, each refuge within 6 m); the enemies' climb route (a raised tier
//     has a ramp or a stair in the room; levelgen's report proves walkers reach every tier); the landmark footprint (the block's `landmark`).
//     Then the walk29 pilot (walk30's code) on Design/levels/n33-<name>.json with the devices ON for the run (SurvivalSim.Devices33Force; THE SPIRE
//     inside NIGHT32's Spire32Guard, as spire32): every playable room cleared in every seed is the gate (10/10).
//   arenas33 devices [level|all]
//     every device of the level in the real sim (headless, DOORFREE, devices on): a FAN lifts a player stood on its grate (the feet rise >= 3 m), a
//     VENT's burst hits a player stood on it once (ventHits 1, a knock up), a PLATFORM carries a player stood on its top (the feet move >= 80 % of
//     the path and stay on it). One line per device, GREEN / RED.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static readonly string[] A33Levels = { "foundry", "iceline", "kiln", "span", "spire" };

    static partial void N33Arenas(string cmd, string[] args, ref int code)
    {
        if (cmd != "arenas33") return;
        code = 0;
        if (args.Length > 1 && args[1] == "devices")
        {
            string which = args.Length > 2 ? args[2] : "all";
            foreach (string g in A33Levels) if (which == "all" || which == g) code |= A33Devices(g);
            return;
        }
        string lvl = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = 10;
        for (int i = 1; i < args.Length - 1; i++) if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
        bool noWalk = Array.IndexOf(args, "--nowalk") >= 0;
        bool allRooms = Array.IndexOf(args, "--rooms") >= 0;
        a33NoDevices = Array.IndexOf(args, "--nodevices") >= 0;
        a33Burns = Array.IndexOf(args, "--burns") >= 0; // diagnosis: where the walk pilot burns and dies, per room (2 m cells, room-local lx / lz) // diagnosis only: the walk with the devices left as the design row says
        foreach (string g in A33Levels)
        {
            if (lvl != "all" && lvl != g) continue;
            code |= A33Rooms(g, allRooms);
            if (!noWalk) code |= A33Walk(g, seeds);
        }
        Console.WriteLine($"arenas33: {(code == 0 ? "GREEN" : "RED")}");
    }

    // ---- the plan checks ----------------------------------------------------------------------------------------------------------------------

    struct A33Rect
    {
        public double x0, z0, x1, z1;
        public A33Rect(double a, double b, double c, double d) { x0 = Math.Min(a, c); x1 = Math.Max(a, c); z0 = Math.Min(b, d); z1 = Math.Max(b, d); }
        public bool In(double x, double z) => x >= x0 && x <= x1 && z >= z0 && z <= z1;
        public double Dist(double x, double z)
        {
            double dx = Math.Max(0, Math.Max(x0 - x, x - x1)), dz = Math.Max(0, Math.Max(z0 - z, z - z1));
            return Math.Sqrt(dx * dx + dz * dz);
        }
    }

    static double A33N(object o) => PortData.Num(o);

    static int A33Rooms(string g, bool allRooms)
    {
        string path = Path.Combine("Design", "levels", "n33", g + ".plan.json");
        var plan = PortData.Obj(PortData.LoadJson(Path.GetFullPath(path)));
        var boxes = PortData.Arr(plan["boxes"]);
        var ramps = PortData.Arr(plan["ramps"]);
        var pits = plan.TryGetValue("pits", out object po) ? PortData.Arr(po) : new List<object>();
        var props = plan.TryGetValue("props", out object pr) ? PortData.Arr(pr) : new List<object>();
        int bad = 0, checkedRooms = 0;
        foreach (object ro in PortData.Arr(plan["rooms"]))
        {
            var room = PortData.Obj(ro);
            string id = (string)room["id"];
            bool over = room.TryGetValue("arenas33", out object ao) && ao is Dictionary<string, object>;
            if (!over && !allRooms) continue;
            checkedRooms++;
            var rr = PortData.Arr(room["rect"]);
            var rect = new A33Rect(A33N(rr[0]), A33N(rr[1]), A33N(rr[2]), A33N(rr[3]));
            double area = (rect.x1 - rect.x0) * (rect.z1 - rect.z0);
            // heights and cover from the boxes
            var tops = new SortedSet<double> { 0 };
            var cover = new List<A33Rect>();
            var holes = new List<A33Rect>();
            bool raised = false;
            foreach (object bo in boxes)
            {
                var b = PortData.Obj(bo);
                if ((string)b["zone"] != id) continue;
                var mn = PortData.Arr(b["min"]);
                var mx = PortData.Arr(b["max"]);
                double top = A33N(mx[1]);
                string kind = (string)b["kind"];
                if (top >= 0.4 && (kind == "deck" || kind == "slab")) { tops.Add(Math.Round(top * 10) / 10); raised = true; }
                if (top >= 1.0) cover.Add(new A33Rect(A33N(mn[0]), A33N(mn[2]), A33N(mx[0]), A33N(mx[2])));
            }
            foreach (object pp in props)
            {
                var p = PortData.Obj(pp);
                if (!(p.TryGetValue("room", out object prm) && prm as string == id)) continue;
                if (!(p.TryGetValue("collider", out object col) && col is bool c && c)) continue;
                var at = PortData.Arr(p["at"]);
                var sz = PortData.Arr(p["size"]);
                if (A33N(sz[1]) < 1.0) continue;
                double hx = A33N(sz[0]) / 2, hz = A33N(sz[2]) / 2;
                cover.Add(new A33Rect(A33N(at[0]) - hx, A33N(at[2]) - hz, A33N(at[0]) + hx, A33N(at[2]) + hz));
            }
            foreach (object pp in pits)
            {
                var p = PortData.Obj(pp);
                if ((string)p["zone"] != id) continue;
                var r = PortData.Arr(p["rect"]);
                holes.Add(new A33Rect(A33N(r[0]), A33N(r[1]), A33N(r[2]), A33N(r[3])));
            }
            int cells = 0, covered = 0;
            double worst = 0, worstX = 0, worstZ = 0;
            for (double x = rect.x0 + 0.5; x < rect.x1; x += 1)
                for (double z = rect.z0 + 0.5; z < rect.z1; z += 1)
                {
                    bool hole = false;
                    foreach (A33Rect h in holes) if (h.In(x, z)) { hole = true; break; }
                    if (hole) continue;
                    double best = 1e9;
                    foreach (A33Rect c in cover) best = Math.Min(best, c.Dist(x, z));
                    cells++;
                    if (best <= 8) covered++;
                    if (best > worst) { worst = best; worstX = x; worstZ = z; }
                }
            double coverShare = cells == 0 ? 1 : covered / (double)cells;
            // ramps / stairs (the climb route)
            int rampN = 0, stairN = 0;
            foreach (object ro2 in ramps) if ((string)PortData.Obj(ro2)["zone"] == id) rampN++;
            foreach (object bo in boxes)
            {
                var b = PortData.Obj(bo);
                if ((string)b["zone"] == id && b.TryGetValue("note", out object n) && n is string s && s.Contains("step")) stairN++;
            }
            bool climbOk = !raised || rampN + stairN > 0;
            // devices
            var types = new HashSet<string>();
            int devN = 0, refugeBad = 0;
            double refugeWorst = 0;
            if (room.TryGetValue("devices33", out object dvo) && dvo is List<object> dl)
                foreach (object d0 in dl)
                {
                    var d = PortData.Obj(d0);
                    string kind = (string)d["kind"];
                    types.Add(kind);
                    devN++;
                    if (!d.TryGetValue("refuge", out object rf)) { refugeBad++; continue; }
                    var rfa = PortData.Arr(rf);
                    double rx = A33N(rfa[0]), rz = A33N(rfa[2]), dist;
                    if (kind == "platform")
                    {
                        var f = PortData.Arr(d["from"]);
                        var t = PortData.Arr(d["to"]);
                        dist = Math.Min(Math.Sqrt(Math.Pow(A33N(f[0]) - rx, 2) + Math.Pow(A33N(f[2]) - rz, 2)), Math.Sqrt(Math.Pow(A33N(t[0]) - rx, 2) + Math.Pow(A33N(t[2]) - rz, 2)));
                    }
                    else
                    {
                        var r = PortData.Arr(d["rect"]);
                        dist = new A33Rect(A33N(r[0]), A33N(r[1]), A33N(r[2]), A33N(r[3])).Dist(rx, rz);
                    }
                    refugeWorst = Math.Max(refugeWorst, dist);
                    if (dist > 6) refugeBad++;
                }
            bool landmark = over && PortData.Obj(ao).ContainsKey("landmark");
            bool heightsOk = tops.Count >= 2, coverOk = coverShare >= 0.999, devOk = !over || (devN >= 1 && types.Count <= 3 && refugeBad == 0);
            bool ok = !over || (heightsOk && coverOk && devOk && climbOk && landmark);
            if (!ok) bad++;
            Console.WriteLine($"arenas33 {g} {id} {(over ? "OVERHAULED" : "(not overhauled)")}: area {area:0} m2, heights {tops.Count} ({string.Join(" / ", tops)}) {(heightsOk ? "ok" : "LOW")}, cover {100 * coverShare:0.0} % of {cells} cells within 8 m (worst {worst:0.0} m at local ({worstX - (rect.x0 + rect.x1) / 2:0.0}, {rect.z1 - worstZ:0.0}), {cover.Count} pieces) {(coverOk ? "ok" : "GAP")}, devices {devN} [{string.Join(", ", types)}] refuge worst {refugeWorst:0.0} m{(refugeBad > 0 ? $" ({refugeBad} over 6 m or missing)" : "")}, climb {(climbOk ? "ok" : "NONE")} (ramps {rampN}, stair steps {stairN}), landmark {(landmark ? "ok" : "none")}{(over ? (ok ? ": GREEN" : ": RED") : "")}");
        }
        if (checkedRooms == 0) Console.WriteLine($"arenas33 {g}: no overhauled room yet (no room row with an arenas33 block)");
        return bad == 0 ? 0 : 1;
    }

    // ---- the walk bot -------------------------------------------------------------------------------------------------------------------------

    static bool a33NoDevices, a33Burns;
    static readonly Dictionary<string, int> a33Cells = new Dictionary<string, int>();

    static int A33Walk(string g, int seeds)
    {
        var a = new List<string> { "walk29", g, "--seeds", seeds.ToString(CultureInfo.InvariantCulture) };
        string old = walkPrefix;
        Action<SurvivalSim> oldHook = walkAfterStart;
        walkPrefix = "n33";
        walkAfterStart = sim =>
        {
            sim.Devices33Force(!a33NoDevices); // --nodevices: off for the diagnosis even with the design row on
            if (g == "spire" && sim.input is Walk29Pilot w) sim.UseInput(new Spire33Guard(sim, w));
            if (a33Burns)
            {
                void Count(string what)
                {
                    Campaign24 c = sim.campaign;
                    if (c == null || c.room < 0) return;
                    RoomDef rd = sim.campaignLevel.rooms[c.room];
                    Vec3 p = sim.mover.position;
                    string key = $"{rd.id} {what} at local ({2 * Math.Floor((p.x - (rd.x0 + rd.x1) / 2) / 2):0}, {2 * Math.Floor((rd.z1 - p.z) / 2):0})";
                    a33Cells.TryGetValue(key, out int n);
                    a33Cells[key] = n + 1;
                }
                sim.events.On<Lava30Event>("lavaBurn", e => Count("burn"));
                sim.events.On<HurtEvent>("hurt", e => Count((e.hp <= 0 ? "DEATH" : "hurt") + " by " + (e.source ?? "?")));
            }
        };
        int code = 0;
        Console.WriteLine($"arenas33 walk {g}: walk29's pilot on Design/levels/n33-{g}.json, devices ON{(g == "spire" ? " + Spire32Guard" : "")}");
        try { N29Walk("walk29", a.ToArray(), ref code); }
        finally { walkPrefix = old; walkAfterStart = oldHook; }
        if (a33Burns)
        {
            var rows = new List<KeyValuePair<string, int>>(a33Cells);
            rows.Sort((x, y) => y.Value.CompareTo(x.Value));
            for (int i = 0; i < Math.Min(14, rows.Count); i++) Console.WriteLine($"arenas33 burns {g} {rows[i].Key}: {rows[i].Value}");
            a33Cells.Clear();
        }
        return code;
    }

    /// <summary>NIGHT32's Spire32Guard (Harness.N32.Finale.cs) for the n33 file, with one fix: its dry-ground step guard tested the lava rects in plan only,
    /// so on a deck standing IN a lava rect (v5's gantry over the flue, 3 m up; the vent pad throws the bot there) every step was cancelled and the bot stood
    /// still for the rest of the run (arenas33 spire seeds 5 and 8 after the v2 / v4 overhauls shifted the timing). Feet 0.5 m or more over the floor are on
    /// a deck: no guard. Bot-only numbers (the harness pilot's), not gameplay.</summary>
    sealed class Spire33Guard : IPlayerInput
    {
        readonly SurvivalSim sim;
        readonly Walk29Pilot inner;
        readonly List<double[]> lava = new List<double[]>(); // [x0, z0, x1, z1]
        const double Probe = 1.2, RimOut = 1.5, DeckY = 0.5;
        int step, lavaSteps;

        public Spire33Guard(SurvivalSim sim, Walk29Pilot inner)
        {
            this.sim = sim;
            this.inner = inner;
            var json = PortData.Obj(PortData.LoadJson(Path.GetFullPath(Path.Combine("Design", "levels", "n33-spire.json"))));
            if (json.TryGetValue("lava", out object lo))
                foreach (object o in PortData.Arr(lo))
                {
                    List<object> r = PortData.Arr(PortData.Obj(o)["rect"]);
                    lava.Add(new[] { PortData.Num(r[0]), PortData.Num(r[1]), PortData.Num(r[2]), PortData.Num(r[3]) });
                }
        }

        public double Yaw => inner.Yaw;
        public double Pitch => inner.Pitch;
        public void SetLook(double y, double p) => inner.SetLook(y, p);
        public bool Captured => inner.Captured;
        public CombatInput SampleCombat(bool active) => inner.SampleCombat(active);

        double[] LavaAt(double x, double z)
        {
            foreach (double[] r in lava) if (x > r[0] && x < r[2] && z > r[1] && z < r[3]) return r;
            return null;
        }

        public InputFrame SampleMove(bool active)
        {
            InputFrame f = inner.SampleMove(active);
            step++;
            Campaign24 c = sim.campaign;
            if (!active || c == null || c.room < 0 || sim.mover.rope.phase != RopePhase.None) return f;
            RoomDef rd = sim.campaignLevel.rooms[c.room];
            Vec3 p = sim.mover.position;
            double[] inLava = LavaAt(p.x, p.z);
            if (inLava != null && p.y < -0.3) lavaSteps = 180;
            else if (inLava == null || p.y >= DeckY) lavaSteps = 0; // the fix: on a deck over the rect, not in the lava
            else if (lavaSteps > 0) lavaSteps--;
            if (inLava != null && lavaSteps > 0)
            {
                double dW = p.x - inLava[0], dE = inLava[2] - p.x, dN = p.z - inLava[1], dS = inLava[3] - p.z, m = Math.Min(Math.Min(dW, dE), Math.Min(dN, dS));
                double tx = p.x, tz = p.z;
                if (m == dW) tx = inLava[0] - RimOut; else if (m == dE) tx = inLava[2] + RimOut; else if (m == dN) tz = inLava[1] - RimOut; else tz = inLava[3] + RimOut;
                return new InputFrame { moveZ = 1, yaw = Math.Atan2(-(tx - p.x), -(tz - p.z)), pitch = f.pitch, jump = new Button { held = true, pressed = sim.mover.grounded && step % 12 == 0 } };
            }
            if (!sim.mover.grounded || p.y < -0.3 || p.y >= DeckY || inLava != null) return f; // the fix: no step guard on a deck, nor grounded over a lava rect (on a ferry platform: v2 seed 7)
            double lastRing = double.PositiveInfinity;
            foreach (Vec3 n in sim.map.grappleNodes)
                if (n.x >= rd.x0 && n.x <= rd.x1 && n.z >= rd.z0 && n.z <= rd.z1) lastRing = Math.Min(lastRing, n.z);
            if (!double.IsPositiveInfinity(lastRing) && p.z > lastRing - 1) return f;
            double fx = -Math.Sin(f.yaw), fz = -Math.Cos(f.yaw), rx = Math.Cos(f.yaw), rz = -Math.Sin(f.yaw);
            double dx = fx * f.moveZ + rx * f.moveX, dz = fz * f.moveZ + rz * f.moveX, len = Math.Sqrt(dx * dx + dz * dz);
            if (len < 0.05) return f;
            if (LavaAt(p.x + dx / len * Probe, p.z + dz / len * Probe) == null) return f;
            f.moveX = 0; f.moveZ = 0; f.jump = default; f.dash = default;
            return f;
        }
    }

    // ---- the devices in the sim ---------------------------------------------------------------------------------------------------------------

    static int A33Devices(string g)
    {
        GameConfig cfg = Config();
        string path = Path.Combine("Design", "levels", $"n33-{g}.json");
        var json = PortData.Obj(PortData.LoadJson(Path.GetFullPath(path)));
        var probe = new Devices33(Devices33Params.From(cfg), cfg.Dt);
        probe.Load(json);
        if (probe.Count == 0) { Console.WriteLine($"arenas33 devices {g}: no devices"); return 0; }
        int bad = 0;
        for (int i = 0; i < probe.Count; i++)
        {
            Device33 d0 = probe.devices[i];
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            sim.StartRun(1, null, null);
            for (int k = 0; k < 240 && (sim.campaign == null || sim.campaign.room < 0); k++) sim.Step();
            sim.campaign?.SetDoorFree29(true);
            sim.Devices33Force(true);
            Device33 d = sim.devices33.devices[i];
            string line;
            bool ok;
            if (d.kind == Device33Kind.Fan)
            {
                sim.Respawn(new Vec3(d.CenterX, d.y, d.CenterZ), 0);
                double y0 = sim.mover.position.y, top = y0;
                for (int k = 0; k < MovementMath.StepsFor(2.5, sim.step); k++) { sim.Step(); top = Math.Max(top, sim.mover.position.y); }
                ok = top - y0 >= 3 && sim.devices33.fanLifts >= 1;
                line = $"fan rose {top - y0:0.0} m (column {d.height:0.0} m), lifts {sim.devices33.fanLifts}";
            }
            else if (d.kind == Device33Kind.Vent)
            {
                sim.Respawn(new Vec3(d.CenterX, d.y, d.CenterZ), 0);
                double hp0 = sim.health.hp, maxY = sim.mover.position.y;
                int cycle = MovementMath.StepsFor(cfg.Num("design.n33.devices33.vent.idleSeconds") + cfg.Num("design.n33.devices33.vent.warnSeconds") + cfg.Num("design.n33.devices33.vent.burstSeconds"), sim.step);
                int tells = 0;
                sim.events.On<Device33Event>("ventTell", e => { if (e.index == i) tells++; });
                for (int k = 0; k < cycle + 2; k++) { sim.Step(); maxY = Math.Max(maxY, sim.mover.position.y); }
                ok = sim.devices33.ventHits == 1 && tells >= 1 && maxY - d.y > 0.5;
                line = $"vent hits {sim.devices33.ventHits} in one cycle, tells {tells}, bursts {sim.devices33.ventBursts}, hp {hp0:0} -> {sim.health.hp:0}, knocked to {maxY - d.y:0.0} m";
            }
            else
            {
                Vec3 a = sim.devices33.PlatformAt(d, sim.devices33.steps);
                sim.Respawn(new Vec3(a.x, a.y + 0.05, a.z), 0);
                double len = Math.Sqrt(Math.Pow(d.to.x - d.from.x, 2) + Math.Pow(d.to.z - d.from.z, 2) + Math.Pow(d.to.y - d.from.y, 2));
                Vec3 p0 = sim.mover.position;
                double far = 0;
                int cycle = sim.devices33.CycleSteps(d);
                for (int k = 0; k < cycle / 2 + 10; k++)
                {
                    sim.Step();
                    Vec3 p = sim.mover.position;
                    far = Math.Max(far, Math.Sqrt(Math.Pow(p.x - p0.x, 2) + Math.Pow(p.z - p0.z, 2) + Math.Pow(p.y - p0.y, 2)));
                }
                Vec3 e = sim.mover.position;
                bool onTop = sim.devices33.Riding(d, d.pos, e, sim.mover.grounded);
                ok = far >= 0.8 * len && onTop;
                line = $"platform path {len:0.0} m, the rider moved {far:0.0} m, carry steps {sim.devices33.carrySteps}, on top at the end {onTop}";
            }
            if (!ok) bad++;
            Console.WriteLine($"arenas33 devices {g} #{i} {d.room} {d.note ?? ""}: {line}: {(ok ? "GREEN" : "RED")}");
        }
        return bad == 0 ? 0 : 1;
    }
}
