// NIGHT44 builder R2 (proof on Player frames; NIGHT44.md card R2). Headless reproductions of the Player reels' loops.
//   n44proof wick [--seeds N] [--cap S]
//     THE WICK's fight as the wick37 Player reel drives it, two ways, seeds 1..N:
//       frame : the OLD reel loop (FeelReel.N37.Wick.cs before NIGHT44): pilot.Step `ran` (2) times against one post-frame position, the presses
//               OR-ed, the result applied to every sim step of the next frame (ReaderInput forcedMove, L-053). The BEFORE.
//       step  : the NEW reel loop: Wick44ProofInput (Bot/Wick44ProofInput.cs) as sim.input, one pilot step per sim step. The AFTER.
//     Line per way: kills / seeds, the median kill time, panels ridden, parries, cracks, windows, the legs reached.
// Hooked without a Program.cs line: a module initializer runs the command and exits when argv[1] starts with "n44proof" (no lead file touched).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void R2N44ProofInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || !argv[1].StartsWith("n44proof")) return;
        string[] args = argv.Skip(1).ToArray();
        int code = 0;
        try { R2N44Proof(args, ref code); }
        catch (Exception e) { Console.WriteLine("n44proof: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static void R2N44Proof(string[] args, ref int code)
    {
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "wick";
        int seeds = ArgInt36(args, "--seeds", 5);
        double cap = ArgInt36(args, "--cap", 75);
        if (what == "wick")
        {
            Wick43Blocks = Blocks43W("wick43,chimney43,ride43");
            int killsF = R2WickWay(false, seeds, cap);
            int killsS = R2WickWay(true, seeds, cap);
            bool ok = killsS == seeds;
            Console.WriteLine($"n44proof wick: {(ok ? "GREEN" : "RED")} (per step kills {killsS}/{seeds}; per frame kills {killsF}/{seeds})");
            if (!ok) code = 1;
            return;
        }
        if (what == "chim") { int ti = Array.IndexOf(args, "--top"); if (ti >= 0 && ti + 1 < args.Length) R2ChimTop = double.Parse(args[ti + 1], System.Globalization.CultureInfo.InvariantCulture); code = R2Chim(Config()); return; }
        Console.WriteLine("n44proof: unknown '" + what + "' (wick, chim)");
        code = 1;
    }

    /// <summary>n44proof chim: move43's chimney pilot (FeelReel.N43.Move.cs) on the box chimney (N43ChimneyWorld, 8 m tall), the chimney43 rules
    /// on, over gap {3.5, 3.75, 4.0, 4.5} x start {1.5, 3.0} m, two ways: the reel's per FRAME loop (the press and the target decided once a
    /// frame of 2 sim steps, the flip seen after the frame) and per SIM STEP (N43ReelChimney's loop). Prints the presses, wall jumps and the top
    /// over the start 1.5 s after the 4th press (the reel's -top frame).</summary>
    static double R2ChimTop = 20;
    static int R2Chim(GameConfig cfg)
    {
        double dt = 1 / cfg.movement.loop.simHz, r = cfg.movement.body.capsuleRadius;
        foreach (bool perFrame in new[] { true, false })
            foreach (double y0 in new[] { 1.5, 3.0 })
                foreach (double gap in new[] { 3.5, 3.75, 4.0, 4.5 })
                {
                    double half = gap / 2;
                    var log = new List<MoverEvent>();
                    Mover m = N43Mover(cfg, N43ChimneyWorld(half, R2ChimTop), true, true, log);
                    m.Reset(-half + r + 0.02, y0, 0);
                    double yaw = YawOf(new Vec3(0, 0, -1)), hi = y0, topAt4 = double.NaN;
                    Vec3 target = new Vec3(-1, 0, 0);
                    bool armed = true, press = false;
                    int presses = 0, wj = 0, seen = 0, tapUntil = -1, p4 = -1;
                    double mx = 0, mz = 0;
                    for (int s = 0; s < 720; s++)
                    {
                        if (!perFrame || s % 2 == 0)
                        {
                            bool onTarget = (m.touchingWall || m.state == MoveState.WallRide) && -(m.wallNormal.x * target.x + m.wallNormal.z * target.z) > 0.7;
                            press = false;
                            if (!onTarget) armed = true;
                            else if (armed && !m.grounded) { press = true; armed = false; presses++; tapUntil = s + 4; if (presses == 4) p4 = s; }
                            mx = target.x * Math.Cos(yaw) - target.z * Math.Sin(yaw);
                            mz = -target.x * Math.Sin(yaw) - target.z * Math.Cos(yaw);
                        }
                        // Edge35Input: the press is an edge on the first sim step only
                        bool edge = press && (!perFrame || s % 2 == 0);
                        m.Step(new InputFrame { moveX = mx, moveZ = mz, yaw = yaw, pitch = 15 * Math.PI / 180, jump = new Button { pressed = edge, held = press || s < tapUntil } }, dt);
                        bool flip = false;
                        for (; seen < log.Count; seen++) if (log[seen].type == MoverEventType.WallJump) { wj++; flip = true; }
                        if (flip) target = new Vec3(-target.x, 0, -target.z); // per frame: still applied after the step, but the stick only changes at the next frame
                        hi = Math.Max(hi, m.position.y);
                        if (p4 >= 0 && (s - p4) * dt >= 1.5) { topAt4 = m.position.y - y0; break; }
                        if (m.grounded && s > 60 && p4 < 0) break;
                    }
                    Console.WriteLine($"  chim {(perFrame ? "frame" : "step ")} start {F42(y0)} gap {F42(gap)}: presses {presses}, WallJump {wj}, max {F42(hi - y0)} m over the start, at the -top frame {(double.IsNaN(topAt4) ? "never (no 4th press)" : F42(topAt4) + " m")}");
                }
        return 0;
    }

    sealed class R2WickRun { public bool killed; public double t; public int parries, cracks, windows, leg, ridden; }

    static int R2WickWay(bool perStep, int seeds, double cap)
    {
        var runs = new List<R2WickRun>();
        for (uint s = 1; s <= seeds; s++) runs.Add(R2WickFight(s, perStep, cap));
        int kills = runs.Count(r => r.killed);
        var kt = runs.Where(r => r.killed).Select(r => r.t).OrderBy(x => x).ToList();
        string med = kt.Count > 0 ? kt[kt.Count / 2].ToString("0.0") + " s" : "-";
        Console.WriteLine($"  {(perStep ? "step " : "frame")}: kills {kills}/{seeds}, median kill {med}; per seed: " +
            string.Join("; ", runs.Select((r, i) => $"s{i + 1} {(r.killed ? "killed " + r.t.ToString("0.0") : "alive")} leg {r.leg} rides {r.ridden} parries {r.parries} cracks {r.cracks} windows {r.windows}")));
        return kills;
    }

    /// <summary>The reel's part B: the player at the stack's mouth, Wick37Reset + Wick37Begin, then the pilot until the kill or `cap` s.</summary>
    static R2WickRun R2WickFight(uint seed, bool perStep, double cap)
    {
        var r = new R2WickRun();
        SurvivalSim sim = Sim43W(seed);
        Vec3 c = Stack37;
        sim.Respawn(c + new Vec3(0, 0.05, sim.wick37Stack.p.radius + 1.0), 0);
        sim.health.max = sim.health.hp = 1e6; // the reel's God(g)
        for (int k = 0; k < 20; k++) sim.Step();
        sim.Wick37Reset();
        if (!sim.Wick37Begin(out Vec3 spawn, out double yawDeg)) { Console.WriteLine("  Wick37Begin false"); return r; }
        Wick37 w = sim.wick37;
        var pilot = new Wick37Pilot { hand = new Mulberry32(seed * 7919u + 13u) };
        var si = (ScriptedInput)sim.input;
        Wick44ProofInput ad = null;
        if (perStep) { ad = new Wick44ProofInput(sim, pilot, si); sim.UseInput(ad); }
        double dt = sim.cfg.Dt, yaw = 0;
        int steps = (int)(cap / dt), tick = 0;
        int i = 0;
        while (i < steps && w.Alive)
        {
            if (perStep)
            {
                sim.Step();
                sim.hitstopLeft = 0;
                i++;
            }
            else
            {
                // the old reel: a 60 fps frame runs 2 sim steps; after the frame the pilot steps twice against the same position, the presses
                // OR-ed into one frame applied to both sim steps of the next frame
                InputFrame mv = default;
                CombatInput cb = new CombatInput { weapon = -1 };
                for (int k = 0; k < 2; k++)
                {
                    pilot.Step(sim, yaw, tick++);
                    yaw = pilot.Move.yaw;
                    InputFrame m = pilot.Move;
                    m.jump.pressed |= mv.jump.pressed; m.dash.pressed |= mv.dash.pressed; m.sprint.pressed |= mv.sprint.pressed;
                    CombatInput cc = pilot.Combat;
                    cc.fire.pressed |= cb.fire.pressed; cc.punch.pressed |= cb.punch.pressed; cc.punch.held |= cb.punch.pressed;
                    if (cc.weapon < 0) cc.weapon = cb.weapon;
                    mv = m;
                    cb = cc;
                }
                for (int k = 0; k < 2 && w.Alive; k++)
                {
                    si.SetLook(mv.yaw, mv.pitch);
                    si.Set(mv, cb);
                    sim.Step();
                    sim.hitstopLeft = 0;
                    i++;
                }
            }
            if (sim.health.hp < sim.health.max - 100) sim.health.hp = sim.health.max;
        }
        r.killed = !w.Alive;
        r.t = i * dt;
        r.parries = w.parries;
        r.cracks = w.cracks;
        r.windows = w.brain.windows;
        r.leg = w.brain.leg;
        r.ridden = pilot.ridden.Count;
        if (ad != null) sim.UseInput(si);
        return r;
    }
}
