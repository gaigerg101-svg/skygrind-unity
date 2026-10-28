// NIGHT46 builder V1 (NIGHT46.md card V1, round 1: the feel audit; Research/2026-10-17-wave46/V1.md): `n46feel`, headless view math on the
// sim's own events. No row, no sim change: it only listens.
//   n46feel unseen [--seconds 300] [--map <path>]   an AUTOPILOT run (seed 4242 normal, the next seed when a run ends, as `impact` does) on
//       the survival arena and DRYDOCK (or one map). Per player `hurt` that carries a source position (HurtEvent.from): the blow's direction
//       in the camera's frame (the mover's yaw / pitch, the eye at body.eyeHeight) against the base view cone (settings.fovDeg vertical,
//       16:9): in view, off-screen to the side, or behind (more than 90 deg from the look). feel-flow-web F21 / F32 "unseen hits".
//       Also: the player's hits per minute that did not kill (F12 / C-2: today none of them freezes; `impact` lists the freeze causes).
// The view cone is the base FOV: the kicks only widen it, so "off-screen" here is an upper bound on what the player could not see.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;
using Flow = Skygrind.Game.Flow;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void V1N46FeelInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n46feel") return;
        string[] args = argv.Skip(1).ToArray();
        int code = 0;
        try { code = V1Feel(args); }
        catch (Exception e) { Console.WriteLine("n46feel: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static string V1FeelArg(string[] args, string key, string def)
    {
        int i = Array.IndexOf(args, key);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : def;
    }

    static int V1Feel(string[] args)
    {
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "unseen";
        double seconds = double.Parse(V1FeelArg(args, "--seconds", "300"), CultureInfo.InvariantCulture);
        string one = V1FeelArg(args, "--map", null);
        var maps = new List<string>();
        if (one != null) maps.Add(Path.GetFullPath(one));
        else
        {
            maps.Add(Path.Combine(Root, "maps", "survival.json"));
            maps.Add(Path.GetFullPath(Path.Combine("Design", "maps", "drydock.json")));
        }
        if (what == "shift") return V1FeelShift(V1FeelArg(args, "--level", "n46-foundry"), V1FeelArg(args, "--room", "b1"));
        if (what != "unseen") { Console.WriteLine("n46feel: unknown '" + what + "' (unseen | shift)"); return 2; }
        foreach (string m in maps) V1FeelUnseen(m, seconds);
        return 0;
    }

    static void V1FeelUnseen(string mapPath, double seconds)
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(mapPath)));
        double vHalf = cfg.Num("settings.fovDeg") * Math.PI / 360; // the base vertical FOV's half angle
        double tanV = Math.Tan(vHalf), tanH = tanV * 16.0 / 9.0;
        int hurts = 0, noFrom = 0, inView = 0, side = 0, behind = 0, hitsNoKill = 0, kills = 0, runs = 0;
        // G1's proof (feel46): Feel46Warn.Scan on the sim's own look every step, when the harness was built with it (-p:Extra); a hurt from out
        // of view is "warned" when its attacker (the enemy nearest the blow's source, within V1WarnMatchM) was marked within V1WarnWindowSteps
        var scan = V1FeelScan();
        int outOfView = 0, warned = 0, unmatched = 0;
        var lastWarn = new Dictionary<int, long>();
        var wAngle = new double[8];
        var wProg = new double[8];
        var wSrc = new int[8];
        var wDist = new double[8];
        long stepNo = 0;
        var outBySource = new Dictionary<string, int[]>();
        double dmgIn = 0, dmgSide = 0, dmgBehind = 0, simTime = 0;
        var bySource = new Dictionary<string, int[]>();
        uint seed = 4242;
        while (simTime < seconds - 1e-9)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            var ap = new AutopilotInput(sim, "normal", seed);
            sim.UseInput(ap);
            sim.events.On<KillEvent>("kill", e => { if (e.owner == "player") kills++; });
            sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && !e.killed) hitsNoKill++; });
            sim.events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner != "player" || !(e.amount > 0)) return;
                hurts++;
                string src = e.source ?? "?";
                if (!bySource.TryGetValue(src, out int[] s)) bySource[src] = s = new int[4];
                if (!e.hasFrom) { noFrom++; s[3]++; return; }
                var (eye, look) = sim.Aim(); // the sim's eye and look (yaw 0 looks toward -z, + pitch looks up)
                double dx = e.from.x - eye.x, dy = e.from.y - eye.y, dz = e.from.z - eye.z;
                double yawR = Math.Atan2(-look.x, -look.z), pitchR = Math.Asin(Math.Max(-1, Math.Min(1, look.y)));
                double cy = Math.Cos(yawR), sy = Math.Sin(yawR), cp = Math.Cos(pitchR), sp = Math.Sin(pitchR);
                // the camera's frame: yaw 0 looks toward -z (Mover.cs); right = (cos yaw, 0, -sin yaw) as CameraFeel.UpdateRoll
                double rx = cy, rz = -sy;
                double fx = -sy * cp, fyy = sp, fz = -cy * cp; // forward
                double ux = sy * sp, uy = cp, uz = cy * sp; // up = right x forward's partner (orthonormal with the two above)
                double lx = dx * rx + dz * rz, lf = dx * fx + dy * fyy + dz * fz, lu = dx * ux + dy * uy + dz * uz;
                double horiz = Math.Sqrt(dx * dx + dz * dz);
                double flatF = horiz > 1e-9 ? (dx * -sy + dz * -cy) / horiz : 1; // cos of the yaw angle to the source
                bool seen = false;
                if (flatF < 0) { behind++; dmgBehind += e.amount; s[2]++; }
                else if (lf > 0 && Math.Abs(lx / lf) <= tanH && Math.Abs(lu / lf) <= tanV) { inView++; dmgIn += e.amount; s[0]++; seen = true; }
                else { side++; dmgSide += e.amount; s[1]++; }
                if (seen || scan == null) return;
                outOfView++;
                if (!outBySource.TryGetValue(src, out int[] o)) outBySource[src] = o = new int[3];
                o[0]++;
                int id = -1;
                double best = V1WarnMatchM;
                foreach (Enemy en in sim.enemies.all)
                {
                    double ddx = en.position.x - e.from.x, ddz = en.position.z - e.from.z, ddy = en.position.y - e.from.y;
                    double dd = Math.Sqrt(ddx * ddx + ddy * ddy + ddz * ddz);
                    if (dd < best) { best = dd; id = en.id; }
                }
                if (id < 0) { unmatched++; o[2]++; return; }
                if (lastWarn.TryGetValue(id, out long ws) && stepNo - ws <= V1WarnWindowSteps) { warned++; o[1]++; }
            });
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(seed, null, null);
            runs++;
            double runSim = 0;
            while (sim.flow != Flow.Results && simTime + runSim < seconds - 1e-9)
            {
                AutopilotInput.TakeDraft(sim);
                sim.hitstopRequested = false;
                sim.Step();
                sim.hitstopLeft = 0;
                runSim += sim.step;
                stepNo++;
                if (scan != null)
                {
                    var (eye, look) = sim.Aim();
                    double yawR = Math.Atan2(-look.x, -look.z), pitchR = Math.Asin(Math.Max(-1, Math.Min(1, look.y)));
                    int n = scan(sim.tells36, eye, yawR, pitchR, tanV, 16.0 / 9.0, wAngle, wProg, wSrc, wDist);
                    for (int k = 0; k < n; k++) lastWarn[wSrc[k]] = stepNo;
                }
            }
            simTime += runSim;
            ap.Detach();
            seed++;
        }
        double min = simTime / 60;
        int withFrom = inView + side + behind;
        string Pct(int n) => withFrom > 0 ? (100.0 * n / withFrom).ToString("0.0", ci) + " %" : "-";
        Console.WriteLine($"n46feel unseen {Path.GetFileName(mapPath)}: {runs} run(s) from seed 4242 normal, sim {simTime.ToString("0.0", ci)} s; base view {cfg.Num("settings.fovDeg")} deg vertical, 16:9");
        Console.WriteLine($"  player hurts {hurts} ({(hurts / min).ToString("0.0", ci)} / min), {noFrom} without a source position");
        Console.WriteLine($"  of {withFrom} with a source: in view {inView} ({Pct(inView)}, {dmgIn.ToString("0", ci)} hp), off-screen to the side {side} ({Pct(side)}, {dmgSide.ToString("0", ci)} hp), behind {behind} ({Pct(behind)}, {dmgBehind.ToString("0", ci)} hp)");
        foreach (var kv in bySource.OrderByDescending(k => k.Value.Sum()))
            Console.WriteLine($"    source {kv.Key,-10} in view {kv.Value[0]}, side {kv.Value[1]}, behind {kv.Value[2]}, no position {kv.Value[3]}");
        Console.WriteLine($"  player hits that did not kill: {hitsNoKill} ({(hitsNoKill / min).ToString("0.0", ci)} / min); kills {kills} ({(kills / min).ToString("0.0", ci)} / min)");
        if (scan == null)
        {
            Console.WriteLine("  warned before the hurt: not measured (build with -p:Extra=../../Assets/Scripts/Game/Feel46Warn.cs%3B../../Assets/Scripts/Game/UnityTuning.Feel46.cs)");
            return;
        }
        string share = outOfView > 0 ? (100.0 * warned / outOfView).ToString("0.0", ci) + " %" : "-";
        Console.WriteLine($"  feel46 G1 (Feel46Warn, the sim's look, every step): out-of-view hurts {outOfView}, warned before the hurt {warned} ({share}; target >= 80 %), attacker not matched {unmatched}");
        foreach (var kv in outBySource.OrderByDescending(k => k.Value[0]))
            Console.WriteLine($"    source {kv.Key,-10} out of view {kv.Value[0]}, warned {kv.Value[1]}, not matched {kv.Value[2]}");
    }

    // ---- shift46's rule on the campaign's own flow (the leftover row; Shift46View reads the same rule every frame) ---------------------------
    // n46feel shift --level <id> --room <room>: the level's campaign headless, the room entered as a door would (Campaign24.Enter by reflection,
    // n45mix's way), the player held still at the room's spawn with full health, every enemy killed each 2 s (Campaign24's own KillAllEnemies
    // world call: husks.KillAll + brain.Clear) so the waves come and go; prints each change of Shift46Rule.RoomState with its time, the waves
    // spawned and the events around it. PASS when the room went None -> LastWave -> Cleared. Needs the rule compiled in:
    // -p:Extra=../../Assets/Scripts/Game/UnityTuning.*46.cs (UnityTuning.Feel46.cs + UnityTuning.Shift46.cs).
    sealed class V1Still : IPlayerInput
    {
        public double Yaw => 0;
        public double Pitch => 0;
        public void SetLook(double y, double p) { }
        public bool Captured => true;
        public InputFrame SampleMove(bool active) => new InputFrame();
        public CombatInput SampleCombat(bool active) => new CombatInput { weapon = -1 };
    }

    static int V1FeelShift(string level, string roomId)
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        Type t = typeof(Harness).Assembly.GetType("Skygrind.Game.Shift46Rule");
        var m = t?.GetMethod("RoomState");
        if (m == null) { Console.WriteLine("n46feel shift: Shift46Rule not compiled in (build with -p:Extra=../../Assets/Scripts/Game/UnityTuning.*46.cs)"); return 2; }
        var rule = (Func<Campaign24, LevelDef, string, int>)Delegate.CreateDelegate(typeof(Func<Campaign24, LevelDef, string, int>), m);
        string path = level.EndsWith(".json") ? Path.GetFullPath(level) : Path.GetFullPath(Path.Combine("Design", "levels", level + ".json")); // a path: a level file elsewhere (port/stage3's, copied)
        if (!File.Exists(path)) { Console.WriteLine("n46feel shift: no " + path); return 2; }
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        sim.UseInput(new V1Still());
        sim.StartRun(1, null, null);
        sim.Step();
        LevelDef lv = sim.campaignLevel;
        int from = lv != null ? lv.rooms.FindIndex(r => r.id == roomId) : -1;
        if (from < 0) { Console.WriteLine($"n46feel shift {level}: no room {roomId}"); return 2; }
        sim.husks.KillAll();
        sim.brain.Clear();
        typeof(Campaign24).GetMethod("Enter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(sim.campaign, new object[] { from });
        sim.Respawn(lv.rooms[from].spawn, lv.rooms[from].spawnYawDeg);
        var log = new List<string>();
        int clears = 0, waves = lv.rooms[from].WaveCount;
        sim.events.On<RoomEvent>("roomClear", e => { if (e.id == roomId) { clears++; log.Add($"roomClear at {sim.SimTime.ToString("0.00", ci)} s"); } });
        int last = -1;
        bool sawLast = false, sawClear = false, order = true;
        int max = MovementMath.StepsFor(180, sim.step), every = MovementMath.StepsFor(2, sim.step);
        for (int st = 0; st < max; st++)
        {
            sim.health.hp = sim.health.max;
            sim.Step();
            if (st % every == every - 1) { sim.husks.KillAll(); sim.brain.Clear(); }
            int s = rule(sim.campaign, lv, roomId);
            if (s != last)
            {
                log.Add($"t {sim.SimTime.ToString("0.00", ci)} s: state {s} (phase {sim.campaign.phase}, wave {sim.campaign.wave}/{waves}, alive {sim.enemies.AliveCount})");
                if (s == 1) sawLast = true;
                if (s == 2) { if (!sawLast) order = false; sawClear = true; }
                last = s;
            }
            if (sawClear && st > 0 && sim.campaign.phase == Campaign24.Phase.Cleared) break;
        }
        bool pass = sawLast && sawClear && order;
        Console.WriteLine($"n46feel shift {level} {roomId}: {(pass ? "PASS" : "FAIL")} (waves {waves}; states 0 none, 1 last wave, 2 cleared) | {string.Join(" | ", log)}");
        return pass ? 0 : 1;
    }

    /// <summary>m: a hurt's source position (the attacker's, HurtEvent.from) matches the nearest enemy within this.</summary>
    const double V1WarnMatchM = 2.5;
    /// <summary>steps: the marker counts when shown within this before the hurt (a wind-up is <= ~1.5 s; the blow lands as it ends).</summary>
    const long V1WarnWindowSteps = 240;

    delegate int V1ScanFn(Tells36 tells, Vec3 eye, double yaw, double pitch, double tanV, double aspect, double[] angle, double[] progress, int[] source, double[] dist);

    /// <summary>Feel46Warn.Scan when the harness was built with it (-p:Extra), else null: the default harness build does not compile Game's view files.</summary>
    static V1ScanFn V1FeelScan()
    {
        Type t = typeof(Harness).Assembly.GetType("Skygrind.Game.Feel46Warn");
        var m = t?.GetMethod("Scan");
        return m == null ? null : (V1ScanFn)Delegate.CreateDelegate(typeof(V1ScanFn), m);
    }
}
