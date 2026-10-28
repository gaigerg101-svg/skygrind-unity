// NIGHT32 builder A (design.n32.arenas32): `ring32` (THE LANTERN RING as a round 64-gon: a full wall-ride LAP with the real Mover, against NIGHT31's 16 flat panels)
// and `arenas32` (the n32 level files: levelgen, the spawn rule, the walk bot; added with the levels). Declared and called by the lead's Program.cs (N32Arenas).
// ring32: the lap bot rides counter-clockwise from the same start on each ring (4 m up, on the wall, 12 or 16 m/s along it), holds the stick along the wall and into it, kicks
// off at the end of a ride (a wall jump, at most the Mover's per-airtime limit), lands, runs along the wall and jumps back on, until it has gone 360 degrees round the centre.
// It prints the lap time, the mean speed over the lap (the speed kept = mean / start), the rides, and every seam crossed DURING a ride (speed before -> 6 steps after). The
// verdict compares the configured facets (design.n32.arenas32.facetsPerPanel; --off32 prints NIGHT31's 16 flat panels only) with the 16 flat panels. Mover.cs is not touched.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N32Arenas(string cmd, string[] args, ref int code)
    {
        if (cmd != "ring32" && cmd != "arenas32") return;
        code = 0;
        if (cmd == "ring32") { code = Ring32(args); return; }
        code = Arenas32(args);
    }

    sealed class Lap32
    {
        public bool done, fell;
        public double t, dist, start, end, minRide = 1e9, angle;
        public int rides, wallJumps, jumps, lands, seams;
        public double seamKeptLog; // sum of log(after / before) over ride seams
        public double rideTime;
        public string firstEnd = "";
        public double MeanSpeed => t > 0 ? dist / t : 0;
        public double SeamKeptMean => seams > 0 ? Math.Exp(seamKeptLog / seams) : 1;
    }

    static int Ring32(string[] args)
    {
        GameConfig cfg = Config();
        double dt = cfg.Dt;
        bool trace = Array.IndexOf(args, "--trace") >= 0 || Array.IndexOf(args, "--trace1") >= 0;
        int svgAt = Array.IndexOf(args, "--svg");
        if (svgAt >= 0 && svgAt + 1 < args.Length) { Ring32Svg(cfg, args[svgAt + 1]); return 0; }
        if (Array.IndexOf(args, "--drop") >= 0) return Ring32Drop(cfg);
        // the configured facets, read as the run reads them with the row on (the harness turns the row on for the measurement; --off32: NIGHT31's flat ring)
        Lantern30ArenaParams live = Lantern30ArenaParams.From(cfg, !N32Blocks.ForceOff);
        int fpp = Math.Max(1, live.facetsPerPanel);
        Console.WriteLine($"ring32: the Mover's corner rule {cfg.movement.wallRide.cornerAngleDeg} deg, ride min {cfg.movement.wallRide.minSpeed} m/s, max ride {cfg.movement.wallRide.maxTime} s, wall jumps per airtime {cfg.movement.wallJump.maxPerAirtime}; ring apothem {live.apothem} m, {live.panels} panels x {fpp} facets{(N32Blocks.ForceOff ? " (--off32: NIGHT31's flat panels)" : "")}");
        var sweep = new List<int> { 1 };
        if (!N32Blocks.ForceOff) { foreach (int f in new[] { 2, 3, 4, 6 }) if (!sweep.Contains(f)) sweep.Add(f); if (!sweep.Contains(fpp)) sweep.Add(fpp); }
        int fails = 0, starts = 8;
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == "--starts") int.TryParse(args[i + 1], out starts);
        Console.WriteLine($"ring32: {starts} starts per ring (offsets spread over one panel), medians; 'per panel' = the speed kept per 22.5 deg of ring ridden (NIGHT30 seam30: 61 % at 16 flat panels)");
        foreach (double speed in new[] { 12.0, 16.0 })
        {
            double[] flatRow = null, mineRow = null;
            foreach (int f in sweep)
            {
                Lantern30ArenaParams prm = live;
                prm.facetsPerPanel = f;
                prm.platformPanels = new int[0];
                prm.ventPanels = new int[0];
                var times = new List<double>();
                var means = new List<double>();
                var perPanel = new List<double>();
                var ends = new List<double>();
                int laps = 0, rides = 0;
                Lap32 first = null;
                for (int s = 0; s < starts; s++)
                {
                    Lap32 r = RingLap(cfg, prm, speed, dt, trace && f == (Array.IndexOf(args, "--trace1") >= 0 ? 1 : fpp) && s == 0, 0.1 + 0.8 * s / Math.Max(1, starts - 1));
                    first ??= r;
                    if (r.done) laps++;
                    times.Add(r.done ? r.t : 99);
                    means.Add(r.MeanSpeed);
                    perPanel.Add(Math.Pow(r.SeamKeptMean, f));
                    ends.Add(r.end);
                    rides += r.rides;
                }
                int nf = live.panels * f;
                double[] row = { Median30(times), Median30(means), Median30(perPanel), laps };
                if (f == 1) flatRow = row;
                if (f == fpp) mineRow = row;
                Console.WriteLine($"ring32 {nf,3} facets ({360.0 / nf:0.00} deg seam) start {speed:0} m/s: laps {laps}/{starts}, lap {row[0]:0.00} s, mean {row[1]:0.00} m/s = {100 * row[1] / speed:0} % kept over the lap, {100 * row[2]:0.0} % kept per panel ridden, end {Median30(ends):0.0} m/s, {(double)rides / starts:0.0} rides a lap (start 1: {first.seams} ride seams at {100 * first.SeamKeptMean:0.0} % each, first ride end '{first.firstEnd}')");
            }
            if (!N32Blocks.ForceOff && fpp > 1)
            {
                bool lapOk = mineRow[3] >= flatRow[3] && mineRow[1] >= flatRow[1], panelOk = mineRow[2] >= flatRow[2];
                bool ok = lapOk && panelOk;
                Console.WriteLine($"ring32 at {speed:0} m/s: {(ok ? "GREEN" : "RED")}: lap mean {(lapOk ? "GREEN" : "RED")} ({live.panels * fpp} facets {mineRow[1]:0.00} m/s = {100 * mineRow[1] / speed:0} % in {mineRow[0]:0.00} s vs NIGHT31's 16 flat panels {flatRow[1]:0.00} m/s = {100 * flatRow[1] / speed:0} % in {flatRow[0]:0.00} s: {100 * (mineRow[1] / flatRow[1] - 1):+0.0;-0.0} %), kept per panel ridden {(panelOk ? "GREEN" : "RED")} ({100 * mineRow[2]:0.0} % vs {100 * flatRow[2]:0.0} %)");
                if (!ok) fails++;
            }
        }
        return fails;
    }

    static Lap32 RingLap(GameConfig cfg, Lantern30ArenaParams prm, double speed, double dt, bool trace, double startFrac = 0.1)
    {
        var arena = new Lantern30Arena(prm);
        var w = new TriangleWorld();
        arena.FillWorld(w, default);
        var m = new Mover(cfg.movement, w);
        var evs = new List<MoverEvent>();
        m.Events += e => evs.Add(e);
        // the start of seam30 (panel 1, 0.4 of a panel before its centre, on the wall, 4 m up, rising 2 m/s, counter-clockwise), the same on every ring
        int p0 = 1;
        double a0 = arena.SeamAngle(p0) + startFrac * arena.PanelAngle; // startFrac 0.1: seam30's start (0.4 of a panel before its centre)
        Vec3 n0 = new Vec3(-JsMath.Cos(a0), 0, -JsMath.Sin(a0)), t0 = new Vec3(-JsMath.Sin(a0), 0, JsMath.Cos(a0));
        Vec3 at = -n0 * (prm.apothem - 0.35);
        m.Reset(at.x, 4.0, at.z);
        m.velocity = new Vec3(t0.x * speed, 2, t0.z * speed);
        var r = new Lap32 { start = speed };
        double angPrev = JsMath.Atan2(m.position.z, m.position.x), turned = 0;
        int rideSteps = 0, groundSteps = 0, jumpHold = 0, lastFacet = arena.FacetAt(m.position.x, m.position.z), settle = 0;
        double seamBefore = 0;
        bool seamPending = false;
        int cap = (int)(20.0 / dt);
        for (int i = 0; i < cap; i++)
        {
            double ang = JsMath.Atan2(m.position.z, m.position.x);
            Vec3 tan = new Vec3(-JsMath.Sin(ang), 0, JsMath.Cos(ang)), outward = new Vec3(JsMath.Cos(ang), 0, JsMath.Sin(ang));
            double yaw = Math.Atan2(-tan.x, -tan.z);
            Vec3 wishR = MovementMath.WishDirection(1, 1, yaw);
            double side = wishR.x * outward.x + wishR.z * outward.z > 0 ? 1 : -1; // the stick's sideways sign that points INTO the wall
            bool riding = m.state == MoveState.WallRide;
            double wallD = arena.WallDistance(m.position.x, m.position.z);
            var f = new InputFrame { yaw = yaw, sprint = new Button { held = true } };
            bool jump = false;
            if (riding)
            {
                rideSteps++;
                f.moveZ = 1; f.moveX = side;
                if (rideSteps >= (int)((cfg.movement.wallRide.maxTime - 0.12) / dt)) jump = true; // kick off just before the ride times out
            }
            else if (!m.grounded)
            {
                rideSteps = 0;
                f.moveZ = 0.6; f.moveX = side * 0.9;
            }
            else
            {
                rideSteps = 0;
                groundSteps++;
                f.moveZ = 1; f.moveX = side * 0.35;
                if (groundSteps >= 3 && wallD < 1.6 && jumpHold == 0) { jump = true; r.jumps++; }
            }
            if (!m.grounded) groundSteps = 0;
            if (jump && jumpHold == 0) { f.jump = new Button { held = true, pressed = true }; jumpHold = 12; }
            else if (jumpHold > 0) { f.jump = new Button { held = true }; jumpHold--; }
            int before = evs.Count;
            double vBefore = HSpeed(m);
            Vec3 pBefore = m.position;
            m.Step(f, dt);
            r.dist += JsMath.Hypot(m.position.x - pBefore.x, m.position.z - pBefore.z);
            r.t += dt;
            if (m.state == MoveState.WallRide) r.rideTime += dt;
            for (int k = before; k < evs.Count; k++)
            {
                MoverEvent e = evs[k];
                if (e.type == MoverEventType.WallRideStart) r.rides++;
                if (e.type == MoverEventType.WallJump) r.wallJumps++;
                if (e.type == MoverEventType.Land) r.lands++;
                if (e.type == MoverEventType.WallRideEnd && r.firstEnd == "") r.firstEnd = e.reason;
            }
            bool ridingNow = m.state == MoveState.WallRide;
            if (ridingNow) r.minRide = Math.Min(r.minRide, HSpeed(m));
            int fac = arena.FacetAt(m.position.x, m.position.z);
            if (fac != lastFacet)
            {
                lastFacet = fac;
                if (riding && ridingNow) { seamPending = true; seamBefore = vBefore; settle = 6; }
            }
            else if (seamPending && settle > 0 && --settle == 0)
            {
                seamPending = false;
                if (ridingNow) { r.seams++; r.seamKeptLog += Math.Log(Math.Max(1e-6, HSpeed(m)) / Math.Max(1e-6, seamBefore)); }
            }
            double angNow = JsMath.Atan2(m.position.z, m.position.x), da = angNow - angPrev;
            while (da > Math.PI) da -= 2 * Math.PI;
            while (da < -Math.PI) da += 2 * Math.PI;
            turned += da;
            angPrev = angNow;
            if (trace && i % 12 == 0) Console.WriteLine($"    t {r.t:0.000} facet {fac} y {m.position.y:0.00} wall {wallD:0.00} v {HSpeed(m):0.0} {m.state} turned {turned * 180 / Math.PI:0}");
            if (turned >= 2 * Math.PI) { r.done = true; break; }
        }
        r.angle = turned;
        r.end = HSpeed(m);
        return r;
    }

    static double HSpeed(Mover m) => JsMath.Hypot(m.velocity.x, m.velocity.z);

    /// <summary>`ring32 --drop`: the floor holds everywhere: a Mover dropped from 2 m on a 1 m grid over the disc, and a run across the centre
    /// in 16 directions at sprint, must stand at y 0 (a fall through the floor is a RED line).</summary>
    static int Ring32Drop(GameConfig cfg)
    {
        int bad = 0, n = 0;
        foreach (bool round in new[] { false, true })
        {
            Lantern30ArenaParams prm = Lantern30ArenaParams.From(cfg, round);
            if (round && prm.facetsPerPanel < 2) prm.facetsPerPanel = 4;
            var a = new Lantern30Arena(prm);
            var w = new TriangleWorld();
            a.FillWorld(w, default);
            int b0 = bad;
            for (double x = -14; x <= 14; x += 1)
                for (double z = -14; z <= 14; z += 1)
                {
                    if (JsMath.Hypot(x, z) > 14) continue;
                    var m = new Mover(cfg.movement, w);
                    m.Reset(x, 2, z);
                    for (int i = 0; i < 120; i++) m.Step(new InputFrame(), cfg.Dt);
                    n++;
                    if (Math.Abs(m.position.y) > 0.05) { bad++; if (bad - b0 <= 5) Console.WriteLine($"  {(round ? "round" : "flat")} drop at ({x}, {z}): y {m.position.y:0.00} {m.state}"); }
                }
            for (int d = 0; d < 16; d++)
            {
                double ang = d * Math.PI / 8;
                var m = new Mover(cfg.movement, w);
                m.Reset(Math.Cos(ang) * 12, 0, Math.Sin(ang) * 12);
                double yaw = Math.Atan2(Math.Cos(ang), Math.Sin(ang)); // look toward the centre (forward = -direction)
                double minY = 0;
                for (int i = 0; i < 240; i++) { m.Step(new InputFrame { moveZ = 1, yaw = yaw, sprint = new Button { held = true } }, cfg.Dt); minY = Math.Min(minY, m.position.y); }
                n++;
                if (minY < -0.05) { bad++; Console.WriteLine($"  {(round ? "round" : "flat")} run {d}: min y {minY:0.00}, end ({m.position.x:0.0}, {m.position.y:0.00}, {m.position.z:0.0})"); }
            }
            Console.WriteLine($"ring32 drop {(round ? $"round {a.Facets} facets" : "flat 16 panels")}: {bad - b0} falls");
        }
        Console.WriteLine($"ring32 drop: {(bad == 0 ? "GREEN" : "RED")} ({n} probes, {bad} falls)");
        return bad == 0 ? 0 : 1;
    }

    /// <summary>`ring32 --svg <file>`: the ring top-down from the SAME triangles the sim collides with: left NIGHT31's 16 flat panels, right the
    /// configured facets (floor fill, wall edges, platform tops, the 16 logical sections' seams as cyan ticks, the vents red-orange).</summary>
    static void Ring32Svg(GameConfig cfg, string path)
    {
        var sb = new System.Text.StringBuilder();
        const double S = 18, W = 640, H = 700;
        sb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{2 * W}' height='{H}' viewBox='0 0 {2 * W} {H}'><rect width='100%' height='100%' fill='#14101c'/>");
        for (int side = 0; side < 2; side++)
        {
            Lantern30ArenaParams prm = Lantern30ArenaParams.From(cfg, side == 1);
            if (side == 1 && prm.facetsPerPanel < 2) prm.facetsPerPanel = 4;
            var a = new Lantern30Arena(prm);
            double cx = side * W + W / 2, cy = H / 2 + 20;
            string P(Vec3 v) => $"{cx + v.x * S:0.0},{cy - v.z * S:0.0}";
            sb.Append($"<text x='{cx}' y='34' fill='#e8e0ff' font-family='Consolas' font-size='20' text-anchor='middle'>{(side == 0 ? "NIGHT31: 16 flat panels (off)" : $"NIGHT32 arenas32: {a.Facets} facets")}</text>");
            foreach (Lantern30Tri t in a.tris)
                if (t.part == Lantern30Part.Floor) sb.Append($"<polygon points='{P(t.a)} {P(t.b)} {P(t.c)}' fill='#2a2238' stroke='#2a2238' stroke-width='0.6'/>");
            foreach (Lantern30Tri t in a.tris)
                if (t.part == Lantern30Part.PlatformTop) sb.Append($"<polygon points='{P(t.a)} {P(t.b)} {P(t.c)}' fill='#6b6488' stroke='#6b6488' stroke-width='0.6'/>");
            foreach (Lantern30Tri t in a.tris)
            {
                if (t.part != Lantern30Part.Panel) continue;
                var low = new List<Vec3>();
                foreach (Vec3 v in new[] { t.a, t.b, t.c }) if (Math.Abs(v.y) < 1e-6) low.Add(v);
                if (low.Count == 2) sb.Append($"<line x1='{cx + low[0].x * S:0.0}' y1='{cy - low[0].z * S:0.0}' x2='{cx + low[1].x * S:0.0}' y2='{cy - low[1].z * S:0.0}' stroke='#d9d2f5' stroke-width='3'/>");
            }
            for (int i = 0; i < prm.panels; i++)
            {
                double s = a.SeamAngle(i);
                Vec3 i0 = new Vec3(Math.Cos(s) * (prm.apothem - 0.8), 0, Math.Sin(s) * (prm.apothem - 0.8)), o0 = new Vec3(Math.Cos(s) * (prm.apothem + 0.8), 0, Math.Sin(s) * (prm.apothem + 0.8));
                sb.Append($"<line x1='{cx + i0.x * S:0.0}' y1='{cy - i0.z * S:0.0}' x2='{cx + o0.x * S:0.0}' y2='{cy - o0.z * S:0.0}' stroke='#3fe0ff' stroke-width='2'/>");
                Vec3 lab = new Vec3(Math.Cos(a.PanelCentreAngle(i)) * (prm.apothem + 1.6), 0, Math.Sin(a.PanelCentreAngle(i)) * (prm.apothem + 1.6));
                sb.Append($"<text x='{cx + lab.x * S:0.0}' y='{cy - lab.z * S + 5:0.0}' fill='#3fe0ff' font-family='Consolas' font-size='13' text-anchor='middle'>{i}</text>");
            }
            for (int k = 0; k < prm.ventPanels.Length; k++)
            {
                Vec3 v = a.Vent(k);
                sb.Append($"<circle cx='{cx + v.x * S:0.0}' cy='{cy - v.z * S:0.0}' r='{UnityTuningVent * S:0.0}' fill='#ff6a3a'/>");
            }
            sb.Append($"<circle cx='{cx}' cy='{cy - 12 * S:0.0}' r='5' fill='#3fe0ff'/>"); // the player spawn (0, 0, 12)
        }
        sb.Append("</svg>");
        System.IO.File.WriteAllText(path, sb.ToString());
        Console.WriteLine($"ring32: wrote {path}");
    }

    const double UnityTuningVent = 0.7; // the drawing's vent radius (UnityTuning.Lava30VentSize 1.4 m across; the harness has no UnityTuning)

    /// <summary>`arenas32 walk <foundry|iceline|kiln> [walk30's args]`: the walk29 pilot (walk30's code) on Design/levels/n32-NAME.json (the arenas32 level files).
    /// The plans' own checks (levelgen: the climb rule, spawn31, the proofs) run in `bash Tools/night/n32-regen.sh`.</summary>
    static int Arenas32(string[] args)
    {
        if (args.Length < 3 || args[1] != "walk") { Console.WriteLine("usage: arenas32 walk <foundry|iceline|kiln> [--seeds n] [--first n] [--cap s] [--trace]"); return 2; }
        var a = new List<string> { "walk29" };
        for (int i = 2; i < args.Length; i++) a.Add(args[i]);
        string old = walkPrefix;
        walkPrefix = "n32";
        int code = 0;
        try { N29Walk("walk29", a.ToArray(), ref code); }
        finally { walkPrefix = old; }
        return code;
    }
}
