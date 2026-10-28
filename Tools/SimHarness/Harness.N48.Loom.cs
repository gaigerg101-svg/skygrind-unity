// NIGHT48 builder L (NIGHT48.md G36 / G37 / kneeHold47): THE LOOM's view and R4 meters, headless. Hooked without a Program.cs line: a module
// initializer runs the command and exits when argv[1] is "n48loom" (Harness.N46.Flow.cs's head).
//   n48loom gait [--seeds N] [--level n47-span] [--rows r1,r2] [--cap S]
//     The fight as n47loom base --seam plays it (Loom37Pilot + the seam policy, from the "start" ledge, the config's rows + --rows), and
//     each step the sim's six FOOT GOALS (Loom37Knee.foot, the truth the view's legs reach for). A MODEL of loomLook47's gait (a copy of
//     Loom37View.Look47.cs's step rule at the reel's 60 fps: a planted foot steps when its goal (plus the idle shuffle) is Look47StepDist
//     away, over Look47StepS, two diagonal groups never lifting at once) is run on those goals twice: NIGHT47's (the foot aims at the goal)
//     and loomHull48's (the goal led by its velocity x StepS x Hull48Lead, smoothed as the view smooths it). Per model, pooled over seeds:
//       steps a minute; planted LAG (the planted foot's distance from its goal: p50 / p90 / max m) and the share of planted frames over
//       2.5 m (a stretched, skating leg); the goals' speed p90 / max (m/s) per phase.
//     A model of the view, not the view: the reel a448 measures the drawn legs (its step strip's line).
//   n48loom hold [--seeds N] [--level n47-span] [--cap S]
//     kneeHold47 (R4) judged: the same fight twice per seed, the row off and on. Per BEND that latches a knee in its first 0.1 s: the latched
//     knee's motion over the bend's first holdS (world m; and its angular speed seen from the player's eye, deg/s), and whether that bend
//     ends in a tear of that knee. Pooled: bends, mean / p90 knee motion, the tear share per latched bend, kill median, deaths, falls.
//     The band itself is n47lhuman's (the lead's pilot): run it with and without --rows kneeHold47.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void L48LoomInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n48loom") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "gait";
            if (what == "gait") code = L48Gait(args);
            else if (what == "hold") code = L48HoldCmd(args);
            else if (what == "idle") code = L48Idle();
            else if (what == "reach") code = L48Reach(args);
            else { Console.WriteLine("usage: n48loom gait|hold [--seeds N] [--level id] [--rows r1,r2] [--cap S]"); code = 1; }
        }
        catch (Exception e) { Console.WriteLine("n48loom: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static readonly CultureInfo L48Inv = CultureInfo.InvariantCulture;

    // the view's gait numbers, COPIED from Assets/Scripts/Game/UnityTuning.Loom47.cs (Look47StepDist / StepS / ShuffleM / ShuffleHz) and
    // UnityTuning.L48.cs (Hull48Lead / LeadSmooth): the harness compiles no UnityTuning render file; change them together
    const double L48StepDist = 1.5, L48StepS = 0.35, L48ShuffleM = 1.2, L48ShuffleHz = 0.22, L48Lead = 0.6, L48LeadSmooth = 0.2;
    // the candidates (UnityTuning.L48.cs Hull48PaceMs / StepMinS / LeashM / SnapM once picked)
    const double L48PaceMs = 4.0, L48StepMinS = 0.16, L48LeashM = 2.5, L48SnapM = 3.0, L48IdleSpeed = 0.5, L48IdleStepM = 0.9;

    // the drawn hull's numbers, COPIED from UnityTuning.L48.cs (round 4) / UnityTuning.Loom37.cs: length Loom37BodyL 8 x Hull48LenMul 1.7,
    // width Loom37BodyW 6.4 x Hull48WidthMul 0.42, height Loom37BodyH 3 x Hull48HeightMul 0.8, the straight middle 0.5, the taper power 1.0
    const double L48HullLen = 8 * 1.7, L48HullW = 6.4 * 0.42, L48HullH = 3 * 0.8, L48Flat = 0.5, L48Taper = 1.0;

    static double L48Profile(double u)
    {
        double a = Math.Abs(u);
        if (a <= L48Flat) return 1;
        double s = Math.Min(1, (a - L48Flat) / (1 - L48Flat));
        return Math.Max(0, 1 - Math.Pow(s, L48Taper));
    }

    /// <summary>Inside the drawn spindle (its local frame: x across, y up, z along; a six-sided cross-section, vertices at 30 + 60k deg,
    /// scaled by the profile at z, as Loom37View.Hull48.cs Spindle48 builds it)?</summary>
    static bool L48InHull(double x, double y, double z)
    {
        double u = z / (L48HullLen * 0.5);
        if (Math.Abs(u) > 1) return false;
        double pr = L48Profile(u);
        if (pr <= 0) return false;
        // the unit hexagon (circumradius 0.5, a vertex at 90 deg): inside when every edge's half-plane holds; scaled per axis by w, h
        double hx = x / (L48HullW * pr), hy = y / (L48HullH * pr);
        for (int k = 0; k < 6; k++)
        {
            double a = (k * 60 + 60) * Math.PI / 180; // edge normals at 0, 60, 120 .. deg (between the vertices)
            if (hx * Math.Cos(a) + hy * Math.Sin(a) > 0.5 * Math.Cos(Math.PI / 6)) return false;
        }
        return true;
    }

    /// <summary>n48loom reach: the drawn hull against the sim's hit sphere. Begins THE LOOM's fight on n47-span (the config's rows), reads the
    /// live body's hit radius (Loom37 p.bodyHalfW) and its centre, then (1) the share of the drawn hull's volume outside that sphere (a 2 cm
    /// grid over the hull's box, the hull laid along world x, its rest pose: Loom37View.Hull48.cs's yaw is 0 for a player due south), the
    /// share of its LENGTH whose whole cross-section lies outside; (2) rays of the game's own test (Loom37.Raycast, the one SurvivalSim's
    /// hitscan asks) from the start ledge's eye and from 10 m and 20 m south of the body, aimed at points along the drawn hull's ridge line
    /// from its middle to each point: hit the body (and at what range from the centre), a knee, the spinneret, or nothing.</summary>
    static int L48Reach(string[] args)
    {
        SurvivalSim sim = L47Sim("n47-span", 1, L47Arg(args, "--rows", ""));
        var start = sim.loom37Ledges.Find(l => l.id == "start");
        sim.Respawn(new Vec3(start.at.x, 0, start.at.z), 0);
        for (int i = 0; i < 10; i++) sim.Step();
        if (!sim.BossRoomBegin("loom37", out _, out _) || sim.loom37 == null) { Console.WriteLine("n48loom reach: NOT BEGUN"); return 1; }
        Loom37 L = sim.loom37;
        for (int i = 0; i < 240; i++) sim.Step(); // the reveal and the first glide
        double r = L.p.bodyHalfW;
        Vec3 c = L.centre;
        // (1) the volume outside the sphere
        double step = 0.02;
        long inHull = 0, outside = 0;
        for (double z = -L48HullLen / 2; z <= L48HullLen / 2; z += step)
            for (double y = -L48HullH / 2; y <= L48HullH / 2; y += step)
                for (double x = -L48HullW / 2; x <= L48HullW / 2; x += step)
                {
                    if (!L48InHull(x, y, z)) continue;
                    inHull++;
                    if (x * x + y * y + z * z > r * r) outside++;
                }
        // the length whose whole cross-section is outside: the nearest point of the cross-section at z is (0, 0, z) itself
        double lenOut = Math.Max(0, L48HullLen / 2 - r) * 2;
        Console.WriteLine($"n48loom reach (n47-span, the config's rows): the live hit sphere radius {r:0.00} m (Loom37 p.bodyHalfW) at ({c.x:0.0}, {c.y:0.0}, {c.z:0.0}); "
            + $"the drawn hull {L48HullLen:0.0} x {L48HullW:0.00} x {L48HullH:0.0} m, its points {L48HullLen / 2:0.0} m from the centre");
        Console.WriteLine($"  volume: {outside * 100.0 / Math.Max(1, inHull):0.0} % of the drawn hull lies outside the hit sphere ({inHull} cells of {step * 100:0} cm); "
            + $"length: {lenOut / L48HullLen * 100:0.0} % of its length ({lenOut:0.0} of {L48HullLen:0.0} m) is wholly outside");
        // (2) rays at the drawn ridge line (the top vertex row, y = H/2 x profile) from three eyes
        var eyes = new (string, Vec3)[]
        {
            ("start ledge", new Vec3(start.at.x, 1.6, start.at.z)),
            ("10 m south, 3.5 m under", new Vec3(c.x, c.y - 3.5, c.z + 10)),
            ("20 m south, 3.5 m under", new Vec3(c.x, c.y - 3.5, c.z + 20)),
            ("20 m south-east (bearing 0.35 / 0.94: a448's shuttle camera), 3.5 m under", new Vec3(c.x + 20 * 0.35, c.y - 3.5, c.z + 20 * 0.94)),
            ("20 m south-west at 60 deg off south, 3.5 m under", new Vec3(c.x - 20 * 0.866, c.y - 3.5, c.z + 20 * 0.5)),
        };
        bool tipsAll = true;
        foreach (var (name, eye) in eyes)
        {
            // the drawn hull's long axis for this eye: the VIEW's own rule (Loom37View.Hull48.cs: world x turned toward the broadside by
            // Hull48YawFollow 0.35 of the exact angle, capped at 20 deg), in sim axes; the sim's capsules use their own (normalized-lerp) copy
            double dx = eye.x - c.x, dz = eye.z - c.z, n = Math.Sqrt(dx * dx + dz * dz);
            double sx = -dz / n, sz = dx / n;
            if (sx < 0) { sx = -sx; sz = -sz; }
            double yaw = Math.Max(-20 * Math.PI / 180, Math.Min(20 * Math.PI / 180, Math.Atan2(sz, sx) * 0.35));
            Vec3 axis = new Vec3(Math.Cos(yaw), 0, Math.Sin(yaw));
            Vec3 flank = new Vec3(-axis.z, 0, axis.x);
            if (flank.x * dx + flank.z * dz < 0) flank = new Vec3(-flank.x, 0, -flank.z); // the flank facing the eye
            var sb = new System.Text.StringBuilder();
            double lastHitX = 0;
            bool tips = true;
            for (int side = -1; side <= 1; side += 2)
                for (int k = 0; k <= 17; k++)
                {
                    double ax = k * 0.4, u = ax / (L48HullLen / 2);
                    // the near flank's face, mid height: what the player sees and aims at
                    Vec3 at = c + axis * (side * ax) + flank * (L48HullW * 0.433 * L48Profile(u));
                    Vec3 d = at - eye;
                    double len = d.Length;
                    d = d * (1.0 / len);
                    double t = L.Raycast(eye, d);
                    string what = t < 0 ? "miss" : L.hitSpinneret ? "spinneret" : L.hitKnee >= 0 ? "knee" : "body";
                    L.hitKnee = -1; L.hitSpinneret = false;
                    if (what == "body") lastHitX = Math.Max(lastHitX, ax);
                    if (k == 12 || k == 15 || k == 17) tips &= what != "miss"; // 4.8 / 6.0 / 6.8 m: the points must be hittable
                    if (k % 3 == 0 || k == 17) sb.Append($" {(side < 0 ? "W" : "E")}{ax:0.0}:{what}");
                }
            tipsAll &= tips;
            Console.WriteLine($"  rays from {name}: yaw {yaw * 180 / Math.PI:0.0} deg; the body is hit out to {lastHitX:0.0} m along the hull (of {L48HullLen / 2:0.0}); points 4.8 / 6.0 / 6.8 m hittable {(tips ? "YES" : "NO")}; samples{sb}");
        }
        Console.WriteLine($"  loomReach48 {(sim.rows.On(sim.idLoomReach48) ? "ON" : "off")} (capsules: radius {L.reach48R:0.00}, {L.reach48Inner:0.0}..{L.reach48Outer:0.0} m; outer + radius {L.reach48Outer + L.reach48R:0.0} vs the point {L48HullLen / 2:0.0} m): every point hittable from every eye {(tipsAll ? "YES" : "NO")}");
        return 0;
    }

    /// <summary>n48loom idle: a resting machine (every foot goal still, only the idle shuffle sways), 60 s at 60 fps, from 40 start times
    /// (the shuffle's phase): steps per leg a minute and the share of legs that never step again after 10 s, for NIGHT47's rule (mode 0) and
    /// loomHull48's (mode 5: the lead, quick steps, the leash, the idle step).</summary>
    static int L48Idle()
    {
        foreach (int mode in new[] { 0, 3, 5 })
        {
            int steps = 0, stalled = 0, legs = 0;
            for (int s = 0; s < 40; s++)
            {
                var m = new L48GaitModel(mode);
                var goals = new Vec3[6];
                for (int i = 0; i < 6; i++) goals[i] = new Vec3(i % 2 == 0 ? -21 : 21, 6, (1 - i / 2) * 9.8);
                double t0 = s * 0.37, dt = 1.0 / 60;
                var lastStep = new double[6];
                int[] before = new int[6];
                for (int f = 0; f < 3600; f++)
                {
                    int was = m.steps;
                    m.Frame(goals, t0 + f * dt, dt);
                    if (m.steps > was) for (int i = 0; i < 6; i++) if (m.StepCount(i) > before[i]) { before[i] = m.StepCount(i); lastStep[i] = f * dt; }
                }
                steps += m.steps;
                for (int i = 0; i < 6; i++) { legs++; if (lastStep[i] < 50) stalled++; }
            }
            Console.WriteLine($"  idle, mode {mode}: {steps / (40.0 * 6):0.0} steps per leg a minute; legs with no step in the last 10 s of 60: {stalled} of {legs} ({stalled * 100.0 / legs:0.0} %)");
        }
        return 0;
    }

    static double L48Med(IEnumerable<double> v) { var a = v.OrderBy(x => x).ToArray(); return a.Length == 0 ? double.NaN : a.Length % 2 == 1 ? a[a.Length / 2] : (a[a.Length / 2 - 1] + a[a.Length / 2]) / 2; }
    static double L48Pct(IEnumerable<double> v, double q) { var a = v.OrderBy(x => x).ToArray(); return a.Length == 0 ? double.NaN : a[Math.Min(a.Length - 1, Math.Max(0, (int)Math.Ceiling(q * a.Length) - 1))]; }

    /// <summary>One seam-policy fight; `each` is called after every sim step with the step index (from the begin). Returns the kill step (-1: none).</summary>
    static int L48Fight(SurvivalSim sim, double cap, Action<int> each, out int deaths)
    {
        deaths = 0;
        var start = sim.loom37Ledges.Find(l => l.id == "start");
        if (start == null) return -2;
        sim.Respawn(new Vec3(start.at.x, 0, start.at.z), 0);
        for (int i = 0; i < 10; i++) sim.Step();
        if (!sim.BossRoomBegin("loom37", out _, out _) || sim.loom37 == null) return -2;
        Loom37 L = sim.loom37;
        var si = (ScriptedInput)sim.input;
        var pilot = new Loom37Pilot();
        sim.mover.invulnerable = false;
        int st = 0, capSteps = (int)(cap * 120), killStep = -1;
        sim.events.On<Loom37Event>("loom37Down", e => { if (killStep < 0) killStep = st; });
        bool seamWas = L47Seam;
        L47Seam = true;
        while (st < capSteps && L.Alive)
        {
            if (sim.health.hp <= 0 || sim.flow != Skygrind.Game.Flow.Playing)
            {
                deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                var back = sim.loom37LastLedge ?? start;
                sim.Respawn(new Vec3(back.at.x, back.y, back.at.z), 0);
                L.brain.RestartLeg();
            }
            pilot.Step(sim, st);
            L47SeamPolicy(sim, L, pilot, st);
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            each(st);
            st++;
        }
        L47Seam = seamWas;
        return killStep;
    }

    /// <summary>A model of loomLook47's step rule (Loom37View.Look47.cs) in sim axes (the view's Unity +z is the sim's -z: the shuffle's sign
    /// is kept as the view's), run at 60 fps on the sim's foot goals. lead: loomHull48's Lead48 on.</summary>
    sealed class L48GaitModel
    {
        // mode 0 NIGHT47 (the goal); 1 + the lead; 2 + a step as quick as the goal is fast (StepS x PaceMs / speed, never under StepMinS);
        // 3 + the leash (a planted foot more than LeashM from its goal steps even while the other group lifts); 4 = 3 + a snap (a goal that
        // jumps more than SnapM in a frame: a station change / a drop: the foot is put there, not walked)
        readonly int mode;
        readonly Vec3[] plant = new Vec3[6], from = new Vec3[6], last = new Vec3[6], vel = new Vec3[6];
        readonly double[] stepT = { -1, -1, -1, -1, -1, -1 }, dur = new double[6];
        bool init;
        public int steps, leashed, snaps, frames, bothLift;
        readonly int[] legSteps = new int[6];
        public int StepCount(int i) => legSteps[i];
        public readonly List<double> lag = new List<double>();
        public L48GaitModel(int mode) { this.mode = mode; }

        static int Group(int i) => ((i / 2) + (i % 2)) & 1;

        public void Frame(Vec3[] simGoal, double t, double dt)
        {
            var goal = new Vec3[6];
            var speed = new double[6];
            for (int i = 0; i < 6; i++)
            {
                // the idle shuffle (Look47ShuffleM along Unity +z = sim -z, at Look47ShuffleHz, phase i x 1.9)
                double sh = Math.Sin(t * L48ShuffleHz * 2 * Math.PI + i * 1.9) * L48ShuffleM;
                Vec3 gi = new Vec3(simGoal[i].x, simGoal[i].y, simGoal[i].z - sh);
                if (!init) { last[i] = gi; vel[i] = new Vec3(0, 0, 0); }
                else
                {
                    Vec3 raw = (gi - last[i]) * (1.0 / dt);
                    if (mode >= 4 && Vec3.DistanceTo(gi, last[i]) > L48SnapM) { plant[i] = gi; stepT[i] = -1; snaps++; raw = new Vec3(0, 0, 0); vel[i] = raw; }
                    vel[i] = vel[i] + (raw - vel[i]) * L48LeadSmooth;
                    last[i] = gi;
                }
                speed[i] = vel[i].Length;
                if (mode >= 1) gi = gi + vel[i] * (L48StepS * L48Lead);
                goal[i] = gi;
            }
            if (!init) { for (int i = 0; i < 6; i++) plant[i] = goal[i]; init = true; }
            for (int i = 0; i < 6; i++)
            {
                if (stepT[i] < 0)
                {
                    bool other = false;
                    for (int j = 0; j < 6; j++) if (j != i && stepT[j] >= 0 && Group(j) != Group(i)) other = true;
                    double d = Vec3.DistanceTo(goal[i], plant[i]);
                    bool leash = mode >= 3 && d > L48LeashM;
                    double trigger = mode >= 5 && speed[i] < L48IdleSpeed ? L48IdleStepM : L48StepDist; // mode 5: + the idle step (round 2)
                    if ((!other || leash) && d > trigger)
                    {
                        if (other) leashed++;
                        from[i] = plant[i]; stepT[i] = 0;
                        dur[i] = mode >= 2 ? Math.Max(L48StepMinS, Math.Min(L48StepS, L48StepS * L48PaceMs / Math.Max(1e-6, speed[i]))) : L48StepS;
                    }
                }
                if (stepT[i] >= 0)
                {
                    stepT[i] += dt / dur[i];
                    if (stepT[i] >= 1) { plant[i] = goal[i]; stepT[i] = -1; steps++; legSteps[i]++; }
                }
                // the LAG the eye sees, every frame (planted or stepping): the drawn foot (the plant, or the step's eased point between its
                // start and its goal, as the view draws it without the lift) from the goal
                Vec3 drawn = plant[i];
                if (stepT[i] >= 0) { double u = Math.Min(1, stepT[i]), e = u * u * (3 - 2 * u); drawn = from[i] + (goal[i] - from[i]) * e; }
                lag.Add(Vec3.DistanceTo(goal[i], drawn));
            }
            bool g0 = false, g1 = false;
            for (int j = 0; j < 6; j++) if (stepT[j] >= 0) { if (Group(j) == 0) g0 = true; else g1 = true; }
            frames++;
            if (g0 && g1) bothLift++;
        }
    }

    static int L48Gait(string[] args)
    {
        int seeds = int.Parse(L47Arg(args, "--seeds", "10"), L48Inv);
        double cap = double.Parse(L47Arg(args, "--cap", "240"), L48Inv);
        string level = L47Arg(args, "--level", "n47-span"), rows = L47Arg(args, "--rows", "");
        const int NM = 6;
        var models = Enumerable.Range(0, NM).Select(k => new L48GaitModel(k)).ToArray();
        var speed = new List<double>[4];
        for (int p = 0; p < 4; p++) speed[p] = new List<double>();
        double fightS = 0;
        int killed = 0;
        for (int s = 1; s <= seeds; s++)
        {
            SurvivalSim sim = L47Sim(level, (uint)s, rows);
            var mods = Enumerable.Range(0, NM).Select(k => new L48GaitModel(k)).ToArray();
            Vec3[] prev = null;
            var goals = new Vec3[6];
            int kill = L48Fight(sim, cap, st =>
            {
                Loom37 L = sim.loom37;
                if (L == null || !L.Alive) return;
                for (int i = 0; i < 6; i++) goals[i] = L.knees[i].foot;
                if (prev != null)
                {
                    int leg = Math.Max(1, Math.Min(3, L.brain.leg));
                    for (int i = 0; i < 6; i++) speed[leg].Add(Vec3.DistanceTo(goals[i], prev[i]) / sim.step);
                }
                prev ??= new Vec3[6];
                Array.Copy(goals, prev, 6);
                if (st % 2 == 0) foreach (var m in mods) m.Frame(goals, st * sim.step, 2 * sim.step);
            }, out _);
            if (kill == -2) { Console.WriteLine($"  seed {s}: NOT BEGUN"); continue; }
            double secs = (kill >= 0 ? kill : cap * 120) * sim.step;
            fightS += secs;
            if (kill >= 0) killed++;
            for (int k = 0; k < NM; k++) { models[k].steps += mods[k].steps; models[k].leashed += mods[k].leashed; models[k].snaps += mods[k].snaps; models[k].frames += mods[k].frames; models[k].bothLift += mods[k].bothLift; models[k].lag.AddRange(mods[k].lag); }
            Console.WriteLine($"  seed {s}: {(kill >= 0 ? "killed" : "cap")} {secs:0.0} s; planted over 2.5 m per model {string.Join(" / ", mods.Select(m => (m.lag.Count(x => x > 2.5) * 100.0 / Math.Max(1, m.lag.Count)).ToString("0.00", L48Inv)))} %");
        }
        Console.WriteLine($"n48loom gait ({level}, {seeds} seeds, killed {killed}, {fightS:0} s of fight{(rows.Length > 0 ? ", rows " + rows : "")}):");
        string[] name = { "0 NIGHT47 (the goal)", "1 + the lead", "2 + quick steps", "3 + the leash", "4 + the snap", "5 = 3 + the idle step (loomHull48 now)" };
        for (int k = 0; k < NM; k++)
        {
            var m = models[k];
            double over = m.lag.Count == 0 ? double.NaN : m.lag.Count(x => x > 2.5) / (double)m.lag.Count;
            Console.WriteLine($"  {name[k]}: steps {m.steps} ({m.steps / Math.Max(1e-9, fightS / 60):0.0} a minute); drawn-foot lag p50 {L48Med(m.lag):0.00} m, p90 {L48Pct(m.lag, 0.9):0.00}, p99 {L48Pct(m.lag, 0.99):0.00}, max {(m.lag.Count > 0 ? m.lag.Max() : double.NaN):0.00}; foot-frames over 2.5 m {over * 100:0.00} %; both groups lifting {m.bothLift * 100.0 / Math.Max(1, m.frames):0.00} % of frames; leashed steps {m.leashed}, snaps {m.snaps}");
        }
        for (int p = 1; p <= 3; p++)
            Console.WriteLine($"  phase {p}: foot-goal speed p50 {L48Med(speed[p]):0.00} m/s, p90 {L48Pct(speed[p], 0.9):0.00}, p99 {L48Pct(speed[p], 0.99):0.00}, max {(speed[p].Count > 0 ? speed[p].Max() : double.NaN):0.00} (the gait's own pace: StepDist / StepS = {L48StepDist / L48StepS:0.00} m/s)");
        return 0;
    }

    sealed class L48HoldStats
    {
        public readonly List<double> moveM = new List<double>(), degS = new List<double>(), kills = new List<double>();
        public int bends, tears, deaths, falls, notKilled, seamTears, heldSteps, freeSteps;
        public double heldMove, freeMove, freeMax;
        public int tornEarly;
    }

    static int L48HoldCmd(string[] args)
    {
        int seeds = int.Parse(L47Arg(args, "--seeds", "20"), L48Inv);
        double cap = double.Parse(L47Arg(args, "--cap", "240"), L48Inv);
        string level = L47Arg(args, "--level", "n47-span");
        var res = new[] { new L48HoldStats(), new L48HoldStats() };
        for (int s = 1; s <= seeds; s++)
            for (int k = 0; k < 2; k++)
            {
                L48HoldStats r = res[k];
                SurvivalSim sim = L47Sim(level, (uint)s, k == 1 ? "kneeHold47" : "");
                double holdS = sim.cfg.Has("design.n47.kneeHold47.numbers.holdS") ? sim.cfg.Num("design.n47.kneeHold47.numbers.holdS") : 0.5;
                int bendStart = -1, knee = -1, tearsAtBend = 0;
                Vec3 k0 = default, e0 = default, kPrev = default;
                int hPrev = 0;
                bool open = false;
                int lastTorn = 0;
                int kill = L48Fight(sim, cap, st =>
                {
                    Loom37 L = sim.loom37;
                    if (L == null || !L.Alive) return;
                    bool inside = sim.Bend34Inside;
                    if (inside && bendStart < 0) { bendStart = st; knee = -1; open = true; }
                    if (!inside && bendStart >= 0)
                    {
                        // the bend ended: a tear of the latched knee inside it?
                        if (knee >= 0) { r.bends++; if (L.knees[knee].torn) r.tears++; }
                        bendStart = -1; knee = -1; open = false;
                        return;
                    }
                    if (!open) return;
                    double el = (st - bendStart) * sim.step;
                    Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
                    if (knee >= 0 && L.knees[knee].torn) { open = false; r.tornEarly++; return; } // torn inside the window: its droop is not motion to aim at
                    if (knee >= 0)
                    {
                        // the knee's travel split by whether the sim held this step (Loom37.holds47 counted it)
                        double dk = Vec3.DistanceTo(L.knees[knee].at, kPrev);
                        if (L.holds47 > hPrev) { r.heldSteps++; r.heldMove += dk; } else { r.freeSteps++; r.freeMove += dk; r.freeMax = Math.Max(r.freeMax, dk); }
                    }
                    if (knee < 0 && el <= 0.1)
                        for (int i = 0; i < 6; i++) if (L.Latched40(i)) { knee = i; k0 = L.knees[i].at; e0 = eye; break; }
                    if (knee >= 0) { kPrev = L.knees[knee].at; hPrev = L.holds47; }
                    if (knee >= 0 && el >= holdS && open)
                    {
                        Vec3 k1 = L.knees[knee].at;
                        r.moveM.Add(Vec3.DistanceTo(k0, k1));
                        // the knee's direction from the eye at the start and now (the eye's own travel counts: that is what the aim sees)
                        Vec3 d0 = k0 - e0, d1 = k1 - eye;
                        double c = (d0.x * d1.x + d0.y * d1.y + d0.z * d1.z) / Math.Max(1e-9, d0.Length * d1.Length);
                        r.degS.Add(Math.Acos(Math.Max(-1, Math.Min(1, c))) * 180 / Math.PI / Math.Max(1e-9, el));
                        open = false; // measured once a bend; the bend's end still counts the tear
                    }
                }, out int deaths);
                if (kill == -2) continue;
                r.deaths += deaths;
                r.falls += (int)sim.loom37Falls;
                r.seamTears += sim.loom37?.seamTears47 ?? 0;
                if (kill >= 0) r.kills.Add(kill * sim.step); else r.notKilled++;
            }
        Console.WriteLine($"n48loom hold ({level}, {seeds} seeds, the seam-policy pilot; per bend that latched a knee in its first 0.1 s, measured over holdS):");
        string[] name = { "kneeHold47 OFF", "kneeHold47 ON " };
        for (int k = 0; k < 2; k++)
        {
            var r = res[k];
            Console.WriteLine($"  {name[k]}: latched bends {r.bends}, knee moved mean {(r.moveM.Count > 0 ? r.moveM.Average() : double.NaN):0.000} m (p90 {L48Pct(r.moveM, 0.9):0.000}), seen from the eye mean {(r.degS.Count > 0 ? r.degS.Average() : double.NaN):0.0} deg/s (p90 {L48Pct(r.degS, 0.9):0.0}); "
                + $"that knee torn by the bend's end {r.tears} ({r.tears * 100.0 / Math.Max(1, r.bends):0.0} %); seam tears {r.seamTears}; kill median {L48Med(r.kills):0.0} s, p90 {L48Pct(r.kills, 0.9):0.0}, min {(r.kills.Count > 0 ? r.kills.Min() : double.NaN):0.0}, not killed {r.notKilled}; deaths {r.deaths}, falls {r.falls}; "
                + $"inside the measured windows: held steps {r.heldSteps} (knee travel {r.heldMove:0.0} m), free steps {r.freeSteps} (knee travel {r.freeMove:0.0} m, the most in one step {r.freeMax:0.000} m); windows ended by a tear inside them {r.tornEarly}");
        }
        return 0;
    }
}
