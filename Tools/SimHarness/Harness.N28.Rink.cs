// NIGHT28 (builder A2): `rink28 [build|friction|stop|bank|cap|lane|lip|all]` - THE GLACIER's rink (Core/Enemies/Rink28Arena.cs) measured with the real Mover on the real triangles (a TriangleWorld filled by the
// arena), 120 Hz, no enemies. The lead adds the dispatch line in Program.cs:   Rink28(cmd, args, ref code);
// Rows (PASS / FAIL lines, code 1 when one fails; PROBE lines are numbers to read, not pass / fail):
//   build     triangle counts by part, winding (every floor faces up, the rim faces in, the lid down), the widest facet-to-facet angle on the rim
//   friction  the ONE ice rule: a 1 m grid across the rink, every floor-grade point reads exactly the ice multiplier (Rink28Check.UniformIce)
//   stop      slide-and-stop distance at 6 sample points (lane, bank, island top, door, cap lane, cap bank): within +-5 % of their mean
//   bank      a bot down the 28 degree bank: standing, slide from rest, slide from 12 m/s, walking into a slide (the speed gained)
//   cap       a bot skating the east cap at 16 m/s steering along the cap: the speed kept, wallRide / corner endings
//   lane      a bot walking the lane for a full lap
//   lip       the player capsule pushed into the bank / rim corner for 120 steps: does it stick or jitter
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void Rink28(string cmd, string[] args, ref int code)
    {
        if (cmd == "rink28") code = Rink28Run(args);
    }

    static string R2(double v) => v.ToString("0.00", CultureInfo.InvariantCulture);

    sealed class RinkRig
    {
        public GameConfig cfg;
        public Rink28Arena arena;
        public TriangleWorld world;
        public Surfaces26 surf;
        public Mover m;
        public List<MoverEvent> events = new List<MoverEvent>();
        public double dt;

        public static RinkRig Make(string[] args = null, bool withIce = true)
        {
            var r = new RinkRig { cfg = Config() };
            Rink28Params prm = Rink28Params.From(r.cfg);
            // experiments: rink28 <row> --facet=3.75 --radius=5.75 --bank=28 --lip=0.4 --run=4.5 --straight=24 (each overrides the config number for this run only)
            if (args != null)
                foreach (string s in args)
                {
                    int eq = s.IndexOf('=');
                    if (!s.StartsWith("--") || eq < 0) continue;
                    double v = double.Parse(s.Substring(eq + 1), CultureInfo.InvariantCulture);
                    switch (s.Substring(2, eq - 2))
                    {
                        case "facet": prm.facetDeg = v; break;
                        case "radius": prm.centreRadius = v; break;
                        case "bank": prm.bankDeg = v; break;
                        case "lip": prm.lip = v; break;
                        case "run": prm.bankRun = v; break;
                        case "straight": prm.straight = v; break;
                    }
                }
            r.arena = new Rink28Arena(prm);
            r.world = new TriangleWorld();
            r.arena.FillWorld(r.world);
            r.surf = new Surfaces26(SurfaceRules.From(r.cfg));
            if (withIce) r.surf.zones.Add(r.arena.IceZone());
            r.dt = r.cfg.Dt;
            return r;
        }

        /// <summary>A fresh mover standing at feet position `at`, settled for 30 steps with no input.</summary>
        public void Place(Vec3 at)
        {
            m = new Mover(cfg.movement, world);
            events.Clear();
            m.Events += e => events.Add(e);
            m.Reset(at.x, at.y + 0.05, at.z);
            for (int i = 0; i < 30; i++) Step(new InputFrame());
        }

        public void Step(InputFrame f)
        {
            surf.Step(m);
            m.Step(f, dt);
        }

        public static double YawOf(double dx, double dz) => Math.Atan2(-dx, -dz);
    }

    static int Rink28Run(string[] args)
    {
        string which = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int fails = 0;
        void Row(string name, bool pass, string detail)
        {
            Console.WriteLine($"rink28 {(pass ? "PASS" : "FAIL")} {name}: {detail}");
            if (!pass) fails++;
        }
        bool Run(string k) => which == "all" || which == k;
        RinkRig rig = RinkRig.Make(args);
        Rink28Arena a = rig.arena;
        Console.WriteLine($"rink28: {a.tris.Count} triangles; straight {R2(a.p.straight)} m, centre radius {R2(a.p.centreRadius)}, lap {R2(a.Lap)} m; lane {R2(a.p.laneWidth)} m, bank {R2(a.p.bankDeg)} deg / {R2(a.p.bankRun)} run / {R2(a.bankRise)} rise, lip {R2(a.p.lip)}, rim {R2(a.p.rimHeight)}; ice x{R2(rig.surf.rules.iceMult)}");
        if (Run("build")) Rink28Build(rig, Row);
        if (Run("friction")) Rink28Friction(rig, Row);
        if (Run("stop")) Rink28Stop(rig, Row);
        if (Run("bank")) Rink28Bank(rig);
        if (Run("cap")) { Rink28Cap(rig); Rink28CapSlide(rig); }
        if (Run("sim")) { SurvivalSim.Wyrm24Dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Root, "..")); Rink28Sim(Row); SurvivalSim.Wyrm24Dir = null; }
        if (Run("lane")) Rink28Lane(rig);
        if (Run("ride")) Rink28Ride(rig);
        if (Run("lip")) Rink28Lip(rig);
        Console.WriteLine($"rink28: {(fails == 0 ? "all rows PASS" : fails + " row(s) FAIL")}");
        return fails == 0 ? 0 : 1;
    }

    static void Rink28Build(RinkRig rig, Action<string, bool, string> row)
    {
        Rink28Arena a = rig.arena;
        var counts = new SortedDictionary<string, int>();
        foreach (Rink28Tri t in a.tris) { string k = t.part.ToString(); counts[k] = counts.TryGetValue(k, out int c) ? c + 1 : 1; }
        var parts = new List<string>();
        foreach (var kv in counts) parts.Add($"{kv.Key} {kv.Value}");
        int bad = Rink28Check.WindingProblems(a, out string first);
        row("winding", bad == 0, bad == 0 ? $"{a.tris.Count} triangles ({string.Join(", ", parts)}): floors up, rim in, island out, lid down" : $"{bad} bad, first: {first}");
        double facet = Rink28Check.MaxFacetAngleDeg(a);
        double corner = rig.cfg.movement.wallRide.cornerAngleDeg;
        row("facet angle", facet <= a.p.facetDeg + 0.5 && facet < corner, $"the widest angle between neighbouring rim facets is {R2(facet)} deg (facetDeg {R2(a.p.facetDeg)}, the wall ride's corner rule {R2(corner)})");
        double bankN = Math.Cos(a.p.bankDeg * Math.PI / 180);
        double floorLimit = 0.7; // the Mover's floor rule: normal.y >= 0.7
        row("bank is floor-grade", bankN >= floorLimit, $"bank normal.y {R2(bankN)} vs the Mover's floor limit {R2(floorLimit)}");
        Rink28Tri sample = a.tris.Find(t => t.part == Rink28Part.Bank);
        Vec3 bn = TriangleWorld.TriangleNormal(sample.a, sample.b, sample.c);
        row("bank facet normal", bn.y >= floorLimit, $"a bank triangle's normal.y is {R2(bn.y)} (floor limit {R2(floorLimit)})");
    }

    static void Rink28Friction(RinkRig rig, Action<string, bool, string> row)
    {
        int n = Rink28Check.UniformIce(rig.arena, rig.world, rig.surf, 1.0, out int bad, out string first);
        row("ONE ice rule (1 m grid)", n > 1000 && bad == 0, $"{n} floor-grade points across the rink, {bad} not exactly x{R2(rig.surf.rules.iceMult)}{(first != null ? ": " + first : "")}; {rig.surf.zones.Count} zone(s)");
        // a second grid shifted by half a metre, so no point sits on a seam by luck
        double hx = rig.arena.p.straight / 2 + rig.arena.RimLat + rig.arena.p.centreRadius;
        row("ONE ice rule is one zone", rig.surf.zones.Count == 1 && rig.surf.zones[0].minY <= -1e9 && rig.surf.zones[0].maxY >= 1e9 && rig.surf.zones[0].x1 - rig.surf.zones[0].x0 > 2 * hx,
            $"a single {rig.surf.zones[0].kind} zone x {R2(rig.surf.zones[0].x0)}..{R2(rig.surf.zones[0].x1)}, z {R2(rig.surf.zones[0].z0)}..{R2(rig.surf.zones[0].z1)}, y -1e9..1e9 (the rink spans x +-{R2(hx)})");
    }

    /// <summary>One slide-and-stop: the mover standing at `at`, given `speed` along (dx, dz) with no input, the metres of plan distance until it is under 0.1 m/s.</summary>
    static double RinkStop(RinkRig rig, Vec3 at, double dx, double dz, double speed, out double y)
    {
        rig.Place(at);
        Vec3 from = rig.m.position;
        rig.m.velocity = new Vec3(dx * speed, 0, dz * speed);
        double dist = 0;
        for (int i = 0; i < 12 * 120 && rig.m.Speed >= 0.1; i++)
        {
            rig.Step(new InputFrame());
        }
        Vec3 end = rig.m.position;
        y = end.y;
        dist = Math.Sqrt((end.x - from.x) * (end.x - from.x) + (end.z - from.z) * (end.z - from.z));
        return dist;
    }

    static void Rink28Stop(RinkRig rig, Action<string, bool, string> row)
    {
        Rink28Arena a = rig.arena;
        double L = a.p.straight, cap = a.math.CapLength;
        var pts = new List<(string name, Vec3 at, double dx, double dz)>();
        Vec3 P(double s, double lat, out double tx, out double tz)
        {
            a.math.PointAt(s, lat, out double x, out double z, out tx, out tz);
            return new Vec3(x, a.SurfaceYAt(x, z), z);
        }
        Vec3 p;
        double ax, az;
        p = P(18, 0, out ax, out az); pts.Add(("lane", p, ax, az));
        p = P(18, a.latLaneOut + 2.0, out ax, out az); pts.Add(("bank", p, ax, az));
        pts.Add(("island top", new Vec3(a.p.cx + 6, a.islandTopY, a.p.cz), 1, 0));
        pts.Add(("door", new Vec3(a.p.cx, a.floorY, a.p.cz - 2), 0, 1));
        p = P(L + cap / 2, 0, out ax, out az); pts.Add(("cap lane", p, ax, az));
        p = P(L + cap / 2, a.latLaneOut + 2.0, out ax, out az); pts.Add(("cap bank", p, ax, az));
        var d = new List<double>();
        var det = new List<string>();
        foreach (var pt in pts)
        {
            double dist = RinkStop(rig, pt.at, pt.dx, pt.dz, 3.5, out double y);
            d.Add(dist);
            det.Add($"{pt.name} {R2(dist)} m");
        }
        double mean = 0;
        foreach (double v in d) mean += v / d.Count;
        double worst = 0;
        foreach (double v in d) worst = Math.Max(worst, Math.Abs(v - mean) / mean);
        row("slide-and-stop at 6 points (3.5 m/s, no input)", worst <= 0.05, $"{string.Join(", ", det)}; mean {R2(mean)} m, worst deviation {R2(worst * 100)} % (limit 5 %)");
    }

    static void Rink28Bank(RinkRig rig)
    {
        Rink28Arena a = rig.arena;
        // the bottom straight, mid-length, on the bank's top end, facing the island (+z): a slide goes down the fall line
        double s = 18, latTop = a.latBankTop - 0.35;
        a.math.PointAt(s, latTop, out double x, out double z, out _, out _);
        double y = a.HeightAtLateral(latTop);
        Vec3 at = new Vec3(x, y, z);
        double dx = 0, dz = 1, yaw = RinkRig.YawOf(dx, dz);
        double slope = 0;
        {
            // standing still: does the bot stay on the bank?
            rig.Place(at);
            Vec3 p0 = rig.m.position;
            for (int i = 0; i < 240; i++) rig.Step(new InputFrame { yaw = yaw });
            Vec3 p1 = rig.m.position;
            Console.WriteLine($"rink28 PROBE bank standing 2 s: moved {R2(Math.Sqrt((p1.x - p0.x) * (p1.x - p0.x) + (p1.z - p0.z) * (p1.z - p0.z)))} m, dy {R2(p1.y - p0.y)}, grounded {rig.m.grounded}, state {rig.m.state} (Ground state: no slope gravity, as the doc's fact 3 reads)");
        }
        foreach (double v0 in new[] { 0.0, 8.0, 12.0 })
        {
            rig.Place(at);
            rig.m.velocity = new Vec3(0, 0, v0);
            double entry = 0, bottomSpeed = 0, peak = 0;
            int stepsOnBank = 0, atBottom = -1;
            double startSpeed = rig.m.Speed;
            for (int i = 0; i < 12 * 120; i++)
            {
                rig.Step(new InputFrame { yaw = yaw, moveZ = v0 > 0 ? 1 : 0, slide = new Button { held = true, pressed = i == 0 } });
                a.math.Project(rig.m.position.x, rig.m.position.z, out _, out double lat);
                if (i == 0) entry = rig.m.Speed;
                peak = Math.Max(peak, rig.m.Speed);
                if (lat > a.latLaneOut - 0.0 && lat < a.latBankTop + 0.3) stepsOnBank++;
                if (atBottom < 0 && lat <= a.latLaneOut + 0.2 && stepsOnBank > 0) { atBottom = i; bottomSpeed = rig.m.Speed; }
                if (atBottom >= 0 && i > atBottom + 60) break;
            }
            Console.WriteLine($"rink28 PROBE bank slide from {R2(v0)} m/s: slide starts at {R2(entry)} m/s (the slide floor is {R2(rig.cfg.movement.slide.floorSpeed)}), reaches the lane at {R2(bottomSpeed)} m/s, peak {R2(peak)} m/s, GAIN {R2(bottomSpeed - Math.Max(entry, rig.cfg.movement.slide.floorSpeed))} m/s over the bank ({stepsOnBank} steps on it)");
        }
    }

    static void Rink28Cap(RinkRig rig)
    {
        Rink28Arena a = rig.arena;
        foreach (double lat0 in new[] { 0.0, 2.0 })
        {
            foreach (bool jump in new[] { false, true })
            {
                a.math.PointAt(a.p.straight - 4, lat0, out double x, out double z, out double tx, out double tz); // 4 m before the cap
                rig.Place(new Vec3(x, a.floorY + (lat0 > a.latLaneOut ? a.HeightAtLateral(lat0) - a.floorY : 0), z));
                rig.m.velocity = new Vec3(tx * 16, 0, tz * 16);
                double minSpeed = 99, maxLat = -99, minLat = 99, endSpeed = 0;
                int steps = 0, airSteps = 0;
                bool jumped = false;
                for (int i = 0; i < 3 * 120; i++)
                {
                    a.math.Project(rig.m.position.x, rig.m.position.z, out double s, out double lat);
                    a.math.PointAt(s, 0, out _, out _, out double ttx, out double ttz);
                    // wish along the tangent, nudged back toward the centre line when it is more than 1 m out
                    double wx = ttx, wz = ttz;
                    if (lat > 1)
                    {
                        a.math.PointAt(s, 0, out double cx0, out double cz0, out _, out _);
                        a.math.PointAt(s, 1, out double cx1, out double cz1, out _, out _);
                        wx -= (cx1 - cx0) * 0.35; wz -= (cz1 - cz0) * 0.35;
                    }
                    var f = new InputFrame { moveZ = 1, yaw = RinkRig.YawOf(wx, wz) };
                    if (jump && !jumped && lat > a.latBankTop - 0.5 && !rig.m.grounded == false) { f.jump = new Button { held = true, pressed = true }; jumped = true; }
                    rig.Step(f);
                    steps++;
                    if (!rig.m.grounded) airSteps++;
                    minSpeed = Math.Min(minSpeed, rig.m.Speed);
                    maxLat = Math.Max(maxLat, lat); minLat = Math.Min(minLat, lat);
                    endSpeed = rig.m.Speed;
                }
                var ev = new List<string>();
                foreach (MoverEvent e in rig.events)
                    if (e.type == MoverEventType.WallRideStart) ev.Add("rideStart");
                    else if (e.type == MoverEventType.WallRideEnd) ev.Add("rideEnd:" + e.reason);
                Console.WriteLine($"rink28 PROBE cap 16 m/s from lateral {R2(lat0)}{(jump ? " + jump at the wall" : "")}: 3 s, end speed {R2(endSpeed)} m/s (min {R2(minSpeed)}), lateral {R2(minLat)}..{R2(maxLat)} m (bank top at {R2(a.latBankTop)}, rim at {R2(a.RimLat)}), airborne {airSteps}/{steps} steps; events: {(ev.Count == 0 ? "none" : string.Join(" ", ev))}");
            }
        }
    }

    /// <summary>The room hookup in the real sim: with design.n28.rink on (the override) the glacier room is the rink (one ice zone, the triangle layer, the spawn on the lane, no tile window); off it is NIGHT27's.</summary>
    static void Rink28Sim(Action<string, bool, string> row)
    {
        GameConfig cfg = Config();
        foreach (bool on in new[] { true, false })
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n27Override = new N27Blocks { boss2 = true };
            sim.glacier28Override = false; // NIGHT28: this row probes A2's rink hookup on the NIGHT27 glacier route; the lap fight (design.n28.glacier) has its own harness (boss28)
            sim.n26Override = new N26Blocks { level2 = true, surfaces = true };
            sim.bosses25Override = true;
            sim.rink28Override = on;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(3, null, null);
            bool began = sim.BossRoomBegin("glacier", out Vec3 spawn, out double yaw);
            if (!began) { row("sim hookup " + (on ? "ON" : "OFF"), false, "BossRoomBegin(glacier) returned false"); continue; }
            sim.Respawn(spawn, yaw);
            if (on)
            {
                Rink28Arena a = sim.rink28;
                int zones = sim.Glacier27Surf?.zones.Count ?? -1;
                var si = (ScriptedInput)sim.input;
                double yawR = yaw * Math.PI / 180;
                si.SetLook(yawR, 0);
                double minY = 1e9, maxY = -1e9, mult = -1;
                int groundedN = 0;
                for (int i = 0; i < 3 * 120; i++)
                {
                    si.Set(new InputFrame { yaw = yawR }, new CombatInput { weapon = -1 });
                    sim.Step();
                    minY = Math.Min(minY, sim.mover.position.y); maxY = Math.Max(maxY, sim.mover.position.y);
                    if (sim.mover.grounded) groundedN++;
                    mult = sim.mover.groundFrictionMult;
                }
                bool ok = a != null && zones == 1 && Math.Abs(spawn.y - a.floorY) < 1e-9 && groundedN > 300 && Math.Abs(mult - SurfaceRules.From(cfg).iceMult) < 1e-12 && sim.Glacier27First == -999;
                a.math.Project(spawn.x, spawn.z, out double s, out double lat);
                row("sim hookup ON", ok, $"rink {(a != null ? a.tris.Count + " triangles" : "missing")}, {zones} surface zone(s), spawn at s {R2(s)} lateral {R2(lat)} y {R2(spawn.y)} yaw {R2(yaw)}, 3 s standing: grounded {groundedN}/360 steps, y {R2(minY)}..{R2(maxY)}, groundFrictionMult {R2(mult)} (ice {R2(SurfaceRules.From(cfg).iceMult)}), tile window {sim.Glacier27First}");
            }
            else
            {
                row("sim hookup OFF", sim.rink28 == null && sim.Glacier27First > -999, $"no rink (rink28 {(sim.rink28 == null ? "null" : "SET")}), the NIGHT27 tile window {sim.Glacier27First}..{sim.Glacier27Last}");
            }
        }
    }

    /// <summary>A SLIDE round the east cap: the direction is locked at the start and deflects off the bank / rim; the stick is held along the cap's tangent. The speed kept and where it ends.</summary>
    static void Rink28CapSlide(RinkRig rig)
    {
        Rink28Arena a = rig.arena;
        foreach (double v0 in new[] { 12.0, 16.0 })
        {
            a.math.PointAt(a.p.straight - 3, 0, out double x, out double z, out double tx, out double tz);
            rig.Place(new Vec3(x, a.floorY, z));
            rig.m.velocity = new Vec3(tx * v0, 0, tz * v0);
            var trace = new List<string>();
            double minLat = 99, maxLat = -99, maxY = 0;
            for (int i = 0; i < 4 * 120; i++)
            {
                a.math.Project(rig.m.position.x, rig.m.position.z, out double s, out double lat);
                a.math.PointAt(s, 0, out _, out _, out double ttx, out double ttz);
                rig.Step(new InputFrame { moveZ = 1, yaw = RinkRig.YawOf(ttx, ttz), slide = new Button { held = true, pressed = i == 0 } });
                minLat = Math.Min(minLat, lat); maxLat = Math.Max(maxLat, lat); maxY = Math.Max(maxY, rig.m.position.y);
                if (i % 60 == 59) trace.Add($"t{(i + 1) / 120.0:0.0}: s {R2(s)} lat {R2(lat)} v {R2(rig.m.Speed)} {rig.m.state}");
            }
            Console.WriteLine($"rink28 PROBE cap SLIDE from {R2(v0)} m/s (3 m before the cap, direction locked, stick along the tangent): {string.Join("; ", trace)}; lateral {R2(minLat)}..{R2(maxLat)}, highest y {R2(maxY)}");
        }
    }

    /// <summary>A wall ride on the straight carried into the east cap: jump from the lip at 16 m/s with the stick toward the rim; the ride's start, its end reason and where, and the speed.</summary>
    static void Rink28Ride(RinkRig rig)
    {
        Rink28Arena a = rig.arena;
        foreach (double sStart in new[] { 0.5, 6.0, 14.0, 22.0, 30.0 })
        {
            double lat = a.latLip - 0.34; // against the wall (the capsule radius 0.35): the stick cannot steer a 16 m/s body into it
            a.math.PointAt(sStart, lat, out double x, out double z, out double tx, out double tz);
            rig.Place(new Vec3(x, a.HeightAtLateral(lat), z));
            rig.m.velocity = new Vec3(tx * 16, 0, tz * 16);
            double yaw = RinkRig.YawOf(tx, tz);
            var log = new List<string>();
            double rideStartS = -1, rideStartSpeed = 0;
            int steps = 0;
            for (int i = 0; i < 3 * 120; i++)
            {
                a.math.Project(rig.m.position.x, rig.m.position.z, out double s0, out _);
                a.math.PointAt(s0, 0, out _, out _, out double ttx, out double ttz);
                // forward along the straight, left (toward the rim on the bottom straight, toward -z); on the cap the stick keeps pointing at the wall
                var f = new InputFrame { moveZ = 1, moveX = -1, yaw = RinkRig.YawOf(ttx, ttz) };
                if (i == 0) f.jump = new Button { held = true, pressed = true };
                else f.jump = new Button { held = i < 20 };
                int before = rig.events.Count;
                rig.Step(f);
                for (int k = before; k < rig.events.Count; k++)
                {
                    MoverEvent e = rig.events[k];
                    if (e.type == MoverEventType.WallRideStart) { rideStartS = s0; rideStartSpeed = e.speed; log.Add($"rideStart at s {R2(s0)} ({R2(e.speed)} m/s, t {R2(i / 120.0)} s)"); }
                    else if (e.type == MoverEventType.WallRideEnd) log.Add($"rideEnd:{e.reason} at s {R2(s0)} ({R2(e.speed)} m/s, t {R2(i / 120.0)} s)");
                }
                steps++;
                if (Array.IndexOf(new[] { 1, 3, 6, 9, 12, 15, 20, 30, 40, 60, 80 }, i) >= 0 && Environment.GetEnvironmentVariable("RINKDBG") != null)
                {
                    a.math.Project(rig.m.position.x, rig.m.position.z, out double ds, out double dl);
                    Console.WriteLine($"   dbg t{i}: s {R2(ds)} lat {R2(dl)} y {R2(rig.m.position.y)} v {R2(rig.m.Speed)} vy {R2(rig.m.velocity.y)} state {rig.m.state} grounded {rig.m.grounded} touchingWall {rig.m.touchingWall}");
                }
            }
            Console.WriteLine($"rink28 PROBE ride from the lip at s {R2(sStart)} (straight ends at {R2(a.p.straight)}, the cap is {R2(a.math.CapLength)} m long), 16 m/s, jump + stick at the rim: {(log.Count == 0 ? "no ride" : string.Join("; ", log))}; final speed {R2(rig.m.Speed)}");
        }
    }

    static void Rink28Lane(RinkRig rig)
    {
        Rink28Arena a = rig.arena;
        a.math.PointAt(2, 0, out double x, out double z, out _, out _);
        rig.Place(new Vec3(x, a.floorY, z));
        double lapSteps = 0, maxDev = 0, topSpeed = 0, sSum = 0;
        double s0 = 2, travelled = 0, prevS = 2;
        int onFloor = 0, n = 40 * 120;
        for (int i = 0; i < n; i++)
        {
            a.math.Project(rig.m.position.x, rig.m.position.z, out double s, out double lat);
            // look ahead along the centre line (the bot steers to the point 2.5 m ahead on the centre line)
            a.math.PointAt(s + 2.5, 0, out double tx, out double tz, out _, out _);
            double wx = tx - rig.m.position.x, wz = tz - rig.m.position.z;
            rig.Step(new InputFrame { moveZ = 1, yaw = RinkRig.YawOf(wx, wz) });
            maxDev = Math.Max(maxDev, Math.Abs(lat));
            topSpeed = Math.Max(topSpeed, rig.m.Speed);
            double ds = a.math.SignedGap(prevS, s);
            travelled += ds; prevS = s;
            if (rig.m.grounded) onFloor++;
        }
        Console.WriteLine($"rink28 PROBE lane walk 40 s (W held, steering at the centre line 2.5 m ahead): {R2(travelled)} m of arc ({R2(travelled / a.Lap)} laps), top speed {R2(topSpeed)} m/s, widest lateral offset {R2(maxDev)} m (lane half {R2(a.LaneHalf)}), grounded {R2(100.0 * onFloor / n)} % of steps");
    }

    static void Rink28Lip(RinkRig rig)
    {
        Rink28Arena a = rig.arena;
        double s = 18;
        foreach (string mode in new[] { "walk out", "stand in the corner", "slide out", "cap walk out" })
        {
            double ss = mode == "cap walk out" ? a.p.straight + a.math.CapLength / 2 : s;
            double lat = mode == "stand in the corner" ? a.latLip - 0.45 : a.latBankTop - 1.0;
            a.math.PointAt(ss, lat, out double x, out double z, out _, out _);
            a.math.PointAt(ss, lat + 1, out double ox, out double oz, out _, out _);
            double ux = ox - x, uz = oz - z, ul = Math.Sqrt(ux * ux + uz * uz);
            ux /= ul; uz /= ul;
            rig.Place(new Vec3(x, a.HeightAtLateral(lat), z));
            var trace = new List<string>();
            double maxStep = 0, maxY = -1e9, minY = 1e9;
            Vec3 prev = rig.m.position;
            int stuck = 0, groundedN = 0;
            for (int i = 0; i < 120; i++)
            {
                var f = new InputFrame { moveZ = mode == "stand in the corner" ? 0 : 1, yaw = RinkRig.YawOf(ux, uz) };
                if (mode == "slide out") f.slide = new Button { held = true, pressed = i == 0 };
                rig.Step(f);
                Vec3 p = rig.m.position;
                maxStep = Math.Max(maxStep, Math.Sqrt((p.x - prev.x) * (p.x - prev.x) + (p.y - prev.y) * (p.y - prev.y) + (p.z - prev.z) * (p.z - prev.z)));
                maxY = Math.Max(maxY, p.y); minY = Math.Min(minY, p.y);
                if (rig.m.Speed < 0.2 && i > 20 && mode != "stand in the corner") stuck++;
                if (rig.m.grounded) groundedN++;
                prev = p;
                if (i == 0 || i == 10 || i == 30 || i == 60 || i == 119)
                {
                    a.math.Project(p.x, p.z, out _, out double l2);
                    trace.Add($"t{i}: lat {R2(l2)} y {R2(p.y)} v {R2(rig.m.Speed)}");
                }
            }
            Console.WriteLine($"rink28 PROBE lip {mode} (120 steps from lateral {R2(lat)}; bank top {R2(a.latBankTop)}, lip out {R2(a.RimLat)}): {string.Join("; ", trace)}; y {R2(minY)}..{R2(maxY)}, biggest one-step move {R2(maxStep)} m, steps at under 0.2 m/s after settle {stuck}, grounded {groundedN}/120, state {rig.m.state}");
        }
    }
}
