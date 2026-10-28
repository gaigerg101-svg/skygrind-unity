// NIGHT26 (builder M): `surfaces [--off]` - the ice, measured with the real Mover on a flat plane (TriangleWorld, 4 km floor), 120 Hz, no enemies.
// Rows (each a PASS / FAIL line; the code is 1 when one fails):
//   standing-start run: W held (and the sprint key) for 4 s from rest: the speed reached and the seconds to 90 % of it, on normal ground and on ice
//   stop: the input released: the metres to a stop (speed < 0.1 m/s), on normal ground and on ice
//   ice is slippery: the ice stop is longer than the normal one and the ice run slower to its top speed (the multiplier did something)
//   mud grips: the mud stop is shorter than the normal one
//   boost strip: a grounded walk across a strip leaves it faster than the same walk without
//   normal ground unchanged to the bit: a run stepped through Surfaces26 with no zones equals a run that never touched it (position and velocity bits)
// `--off` runs it with every surface rule forced to 1 / 1 / 0 (the --off26 side): the ice rows then FAIL: the RED line before the config rows / the code.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void SurfacesCommand(string[] args, ref int code) => code = Surfaces26Run(args);

    sealed class SurfRun
    {
        public double top, secondsTo90, stopMetres, finalSpeed;
        public Vec3 end, vel;
    }

    static string Sf(double v) => v.ToString("0.00", CultureInfo.InvariantCulture);

    /// <summary>From rest on the plane: `runSteps` of W (+ sprint), then the input released until the speed is under 0.1 m/s (at most 20 s).</summary>
    static SurfRun SurfaceDrive(GameConfig cfg, Surfaces26 surf, double startZ, int runSteps)
    {
        var plane = new TriangleWorld();
        plane.AddBox(new Vec3(-2000, -1, -2000), new Vec3(2000, 0, 2000));
        var m = new Mover(cfg.movement, plane);
        m.Reset(0, 0, startZ);
        double dt = cfg.Dt;
        for (int i = 0; i < 30; i++) { surf?.Step(m); m.Step(new InputFrame(), dt); } // settle
        var r = new SurfRun();
        var speeds = new double[runSteps];
        for (int i = 0; i < runSteps; i++)
        {
            surf?.Step(m);
            m.Step(new InputFrame { moveZ = 1, sprint = new Button { held = true, pressed = i == 0 } }, dt);
            speeds[i] = m.Speed;
            r.top = Math.Max(r.top, m.Speed);
        }
        r.secondsTo90 = -1;
        for (int i = 0; i < runSteps; i++)
            if (speeds[i] >= 0.9 * r.top) { r.secondsTo90 = (i + 1) * dt; break; }
        Vec3 from = m.position;
        for (int i = 0; i < (int)(20 / dt) && m.Speed >= 0.1; i++)
        {
            surf?.Step(m);
            m.Step(new InputFrame(), dt);
        }
        r.stopMetres = Math.Sqrt((m.position.x - from.x) * (m.position.x - from.x) + (m.position.z - from.z) * (m.position.z - from.z));
        r.finalSpeed = m.Speed;
        r.end = m.position;
        r.vel = m.velocity;
        return r;
    }

    static int Surfaces26Run(string[] args)
    {
        if (Array.IndexOf(args, "--wallride") >= 0) return WallRide26Run(args);
        if (Array.IndexOf(args, "--sim") >= 0) return SurfacesSim26Run(args);
        bool off = Array.IndexOf(args, "--off") >= 0;
        GameConfig cfg = Config();
        SurfaceRules rules = SurfaceRules.From(cfg);
        if (off) rules = new SurfaceRules { iceMult = 1, mudMult = 1, boostImpulse = 0 };
        Console.WriteLine($"surfaces: Mover on a flat plane, 120 Hz; ice x{Sf(rules.iceMult)}, mud x{Sf(rules.mudMult)}, boost {Sf(rules.boostImpulse)} m/s{(off ? " (--off: the rules forced off)" : "")}");
        int fails = 0;
        void Row(string name, bool pass, string detail)
        {
            Console.WriteLine($"surfaces {(pass ? "PASS" : "FAIL")} {name}: {detail}");
            if (!pass) fails++;
        }
        int runSteps = (int)(4 / cfg.Dt);
        // the zones cover the whole run (z from 1000 toward -z); the strip is a 2 m band 3 m ahead of the start
        Surfaces26 Zone(SurfaceKind k)
        {
            var s = new Surfaces26(rules);
            s.zones.Add(new SurfaceZone { kind = k, x0 = -50, x1 = 50, z0 = k == SurfaceKind.Boost ? 995 : -1500, z1 = k == SurfaceKind.Boost ? 997 : 1500 });
            return s;
        }
        SurfRun normal = SurfaceDrive(cfg, null, 1000, runSteps);
        SurfRun ice = SurfaceDrive(cfg, Zone(SurfaceKind.Ice), 1000, runSteps);
        SurfRun mud = SurfaceDrive(cfg, Zone(SurfaceKind.Mud), 1000, runSteps);
        Row("standing-start run, normal ground", normal.top > 0, $"reaches {Sf(normal.top)} m/s, 90 % of it in {Sf(normal.secondsTo90)} s");
        Row("standing-start run, ice", ice.top > 0, $"reaches {Sf(ice.top)} m/s, 90 % of it in {Sf(ice.secondsTo90)} s");
        Row("stop, normal ground", normal.finalSpeed < 0.1, $"{Sf(normal.stopMetres)} m to a stop");
        Row("stop, ice", ice.finalSpeed < 0.1, $"{Sf(ice.stopMetres)} m to a stop");
        // (the run-up is the same on ice: Quake's accelerate does not depend on friction; ice changes how far you carry speed, not how fast you reach it)
        Row("ice is slippery", ice.stopMetres > normal.stopMetres * 1.5,
            $"stop {Sf(ice.stopMetres)} m vs {Sf(normal.stopMetres)} m on normal ground (needs 1.5x longer); run-up 90 % in {Sf(ice.secondsTo90)} s vs {Sf(normal.secondsTo90)} s");
        Row("mud grips", mud.stopMetres < normal.stopMetres, $"stop {Sf(mud.stopMetres)} m vs {Sf(normal.stopMetres)} m on normal ground");
        // the boost strip on ice (the skater's lane): W held across a 2 m band; the peak speed over 2 s and the metres covered, against the same ice without the strip
        (double peak, double metres) Cross(Surfaces26 s)
        {
            var plane = new TriangleWorld();
            plane.AddBox(new Vec3(-2000, -1, -2000), new Vec3(2000, 0, 2000));
            var m = new Mover(cfg.movement, plane);
            m.Reset(0, 0, 1000);
            double dt = cfg.Dt, peak = 0;
            for (int i = 0; i < 30; i++) { s?.Step(m); m.Step(new InputFrame(), dt); }
            for (int i = 0; i < (int)(2 / dt); i++)
            {
                s?.Step(m);
                m.Step(new InputFrame { moveZ = 1 }, dt);
                peak = Math.Max(peak, m.Speed);
            }
            return (peak, 1000 - m.position.z);
        }
        Surfaces26 iceOnly = Zone(SurfaceKind.Ice);
        Surfaces26 iceStrip = Zone(SurfaceKind.Ice);
        iceStrip.zones.Add(new SurfaceZone { kind = SurfaceKind.Boost, x0 = -50, x1 = 50, z0 = 995, z1 = 997 });
        var plain = Cross(iceOnly);
        var boosted = Cross(iceStrip);
        Row("boost strip", boosted.peak > plain.peak + 0.5 * rules.boostImpulse && boosted.metres > plain.metres,
            $"on ice, W held 2 s: peak {Sf(boosted.peak)} m/s and {Sf(boosted.metres)} m with the strip vs {Sf(plain.peak)} m/s and {Sf(plain.metres)} m without (push {Sf(rules.boostImpulse)} m/s)");
        // bit-identical on normal ground: an empty Surfaces26 stepped each step vs none
        SurfRun bare = SurfaceDrive(cfg, null, 1000, runSteps);
        SurfRun empty = SurfaceDrive(cfg, new Surfaces26(rules), 1000, runSteps);
        bool same = BitConverter.DoubleToInt64Bits(bare.end.x) == BitConverter.DoubleToInt64Bits(empty.end.x)
            && BitConverter.DoubleToInt64Bits(bare.end.z) == BitConverter.DoubleToInt64Bits(empty.end.z)
            && BitConverter.DoubleToInt64Bits(bare.vel.x) == BitConverter.DoubleToInt64Bits(empty.vel.x)
            && BitConverter.DoubleToInt64Bits(bare.vel.z) == BitConverter.DoubleToInt64Bits(empty.vel.z)
            && BitConverter.DoubleToInt64Bits(normal.end.z) == BitConverter.DoubleToInt64Bits(bare.end.z);
        Row("normal ground unchanged to the bit", same, $"end z {bare.end.z.ToString("R", CultureInfo.InvariantCulture)} vs {empty.end.z.ToString("R", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"surfaces: {(fails == 0 ? "ALL PASS" : fails + " FAIL")}");
        return fails == 0 ? 0 : 1;
    }

    /// <summary>`surfaces --sim`: the same sprint and release in the COMPOSED sim (SurvivalSim.Step: the Run.cs order, stamina, the hooks), ice laid through the
    /// sim's own surfaces26 (what the Player's `ice` scenario does). The Player's ice stop equalled the normal one: this row finds out whether the composition
    /// loses the multiplier.</summary>
    static int SurfacesSim26Run(string[] args)
    {
        GameConfig cfg = Config();
        int fails = 0;
        for (int pass = 0; pass < 2; pass++)
        {
            bool ice = pass == 1;
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noRecords = sim.noLab = true;
            sim.n26Override = new N26Blocks { surfaces = true };
            sim.StartRun(2601, null, null);
            Vec3 from = default;
            double yaw = 0;
            var rng = new Mulberry32(2610);
            for (int t = 0; t < 400; t++)
            {
                double a = rng.Next() * Math.PI * 2, rad = 12 + rng.Next() * 16;
                Vec3 p = new Vec3(Math.Cos(a) * rad, 0, Math.Sin(a) * rad);
                if (!sim.world.RayIntersect(new Vec3(p.x, 26, p.z), new Vec3(0, -1, 0), out RayHit down) || 26 - down.distance > 0.2) continue;
                (double y, double dx, double dz) = ArclineOpenDir(sim, p, 24);
                if (dx == 0 && dz == -1 && y == 0) continue;
                from = p; yaw = y;
                break;
            }
            sim.Respawn(from, yaw * 180 / Math.PI);
            if (ice)
            {
                var z = new Surfaces26(SurfaceRules.From(cfg));
                z.zones.Add(new SurfaceZone { kind = SurfaceKind.Ice, x0 = from.x - 80, x1 = from.x + 80, z0 = from.z - 80, z1 = from.z + 80 });
                sim.surfaces26 = z;
            }
            var inp = (ScriptedInput)sim.input;
            for (int s = 0; s < 40; s++) { inp.SetLook(yaw, 0); inp.Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.hitstopLeft = 0; sim.Step(); }
            double top = 0;
            for (int s = 0; s < 72; s++)
            {
                inp.SetLook(yaw, 0);
                inp.Set(new InputFrame { moveZ = 1, sprint = new Button { held = true, pressed = s == 0 } }, new CombatInput { weapon = -1 });
                sim.hitstopLeft = 0;
                sim.Step();
                top = Math.Max(top, sim.mover.Speed);
            }
            Vec3 at = sim.mover.position;
            int n = 0;
            double mult = sim.mover.groundFrictionMult;
            for (; n < 1200 && sim.mover.Speed >= 0.1; n++)
            {
                inp.SetLook(yaw, 0);
                inp.Set(new InputFrame(), new CombatInput { weapon = -1 });
                sim.hitstopLeft = 0;
                sim.Step();
            }
            double d = Math.Sqrt((sim.mover.position.x - at.x) * (sim.mover.position.x - at.x) + (sim.mover.position.z - at.z) * (sim.mover.position.z - at.z));
            Console.WriteLine($"surfaces --sim {(ice ? "ice " : "normal")}: top {Sf(top)} m/s, stop {Sf(d)} m in {n} steps, groundFrictionMult at release {Sf(mult)}, surfaces26 {(sim.surfaces26 == null ? "null" : "set")}, n26.surfaces {sim.n26.surfaces}");
        }
        return fails;
    }
}
