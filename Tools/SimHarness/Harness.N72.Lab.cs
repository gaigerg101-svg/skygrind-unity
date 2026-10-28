// NIGHT72 builder N, slice N5 (GG 2026-10-26: the movement "almost a one to one copy" of ULTRAKILL; FORGE72 D-7, scout-movement.md 3-4): the movement
// PARITY LAB. From the repo root:
//   dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll n72lab [--set LABEL:ground.friction=31,ground.accel=31,air.accel=3.63 ...] [--out file]
// The real Mover (Skygrind.Core, the 120 Hz fixed step) on a flat triangle floor and one wall, under these profiles side by side:
//   web      PortExport/config.json's movement (the golden vectors' numbers)
//   now      web + design.ultra71.set from Design/unity-config.json (the ULTRAKILL profile as it ships; MovementTuning71 does the same in the Player)
//   LABEL    now + the `--set` pairs of that label (dotted MovementConfig paths, as design.ultra71.set; any number of --set options)
// Each row prints the measure, ULTRAKILL's value scaled to the 1.8 m body (Research/2026-10-26-ultrakill/movement.md: lengths and speeds x0.5143, times
// kept) and each profile's value with its deviation from that target; then, per --set label, how many rows moved TOWARD the target against "now" (the
// profile rule of NIGHT72 N5: a change only if its numbers move toward the target and verbs37 holds).
// NOT applied: the sim's design rows (flow34, stamina, design.move, ride rows, perks): the lab measures the movement config on the bare Mover, so a
// row that reads walk speed, friction or accel on top of it is verbs37's to prove in a Player. The ground / air rows are the Quake model
// (MovementMath.ApplyFriction + Accelerate), the target ULTRAKILL's lerp-to-target (25 % of the gap per 8 ms tick: half-life 19 ms, 95 % in 83 ms).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Skygrind.Core;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void N72LabInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n72lab") return;
        int code;
        try { code = N72Lab(argv.Skip(1).ToArray()); }
        catch (Exception e) { Console.WriteLine("n72lab: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    const double N72Dt = 1.0 / 120;
    static readonly CultureInfo N72Ci = CultureInfo.InvariantCulture;

    // ULTRAKILL's numbers x0.5143 (movement.md); a time is kept as is. null = no target (printed only)
    // name, unit, target, source note
    static readonly (string name, string unit, double target, string note)[] N72Rows =
    {
        ("jump apex", "m", 5.07, "9.86 u x0.5143"),
        ("jump time to apex", "s", 0.702, "0.702 s"),
        ("jump air time (flat)", "s", 1.404, "1.40 s"),
        ("double jump: apex above the floor", "m", double.NaN, "ULTRAKILL has none; kept (GG: keep double jump)"),
        ("dash distance", "m", 5.09, "9.9 u x0.5143"),
        ("dash speed", "m/s", 25.46, "49.5 u/s"),
        ("dash duration", "s", 0.200, "25 ticks"),
        ("slide speed, first step from walk", "m/s", 12.34, "24 u/s"),
        ("slide speed after 1 s", "m/s", 12.34, "decays to 24 u/s"),
        ("slam speed", "m/s", 51.43, "100 u/s"),
        ("wall jump: speed away from the wall", "m/s", 12.34, "24 u/s"),
        ("wall jump: upward speed", "m/s", 12.34, "24 u/s"),
        // ULTRAKILL ticks at 8 ms and lerps 25 % of the gap each: 0.75^3 = 0.42 (3 ticks = 24 ms; the continuous half-life is 19 ms), 0.75^11 = 0.042
        // (11 ticks = 88 ms; the continuous 95 % point is 83 ms). The 120 Hz sim has 8.33 ms steps, so its own steps are 3 and 11 / 10 as well
        ("stop: time to half of walk speed", "s", 0.024, "3 ticks x 8 ms (continuous 0.019)"),
        ("stop: time to 5 % of walk speed", "s", 0.088, "11 ticks (continuous 0.083)"),
        ("ground accel: time to 50 % of walk speed", "s", 0.024, "3 ticks x 8 ms (continuous 0.019)"),
        ("ground accel: time to 95 % of walk speed", "s", 0.088, "11 ticks (continuous 0.083)"),
        ("air accel from rest: time to 95 % of walk speed", "s", 0.261, "0.275 s to the cap, x0.95"),
        ("air turn 90 deg at walk speed: time to 95 % across", "s", 0.261, "per-axis: same as from rest"),
        ("air turn 90 deg: speed kept along the old axis", "%", 100.0, "no input adds nothing"),
        ("bunny hop: speed kept per hop (worst of 3)", "%", 100.0, "reel bar: >= 99"),
        ("bunny hop: time between hops", "s", 1.404, "flat air time"),
        // G (my arithmetic): ULTRAKILL's air input caps each axis at 16.5 u/s = 8.49 m/s, so input alone reaches 8.49 x sqrt(2) = 12.0 m/s from walk speed,
        // +3.5 m/s; a speed already above the cap is kept (patch 17 made air strafing stronger: the target is a floor for how much a copy may allow)
        ("air strafe, optimal turn: speed gained over one flat jump", "m/s", 3.5, "G: the per-axis cap's diagonal, 12.0 - 8.49"),
        // the surfaces (design.n26.surfaces: ice multiplies ground friction by 0.25, mud by 2; Mover.groundFrictionMult): printed only, no ULTRAKILL target (the ice is the web's own system)
        ("walk stop: distance from walk speed on plain ground", "m", double.NaN, "printed only"),
        ("walk stop: distance on ice (ground friction x0.25)", "m", double.NaN, "printed only"),
        ("walk stop: distance on mud (ground friction x2)", "m", double.NaN, "printed only"),
        ("ice vs ground: the ice's stop distance over the ground's", "x", double.NaN, "the web's rule: about 4 (iceFrictionMult 0.25)"),
        // what iceFrictionMult would be to keep ice's ABSOLUTE friction (0.25 x the web's 6 = 1.5 /s) under friction 31.07: 1.5 / 31.07 = 0.0483; and the mud (2 x 6 = 12 /s): 0.386
        ("walk stop: distance on ice at iceFrictionMult 0.0483 (1.5 /s at 31.07)", "m", double.NaN, "printed only: the web's ice is 4.4 m"),
        ("walk stop: distance on mud at mudFrictionMult 0.386 (12 /s at 31.07)", "m", double.NaN, "printed only: the web's mud is 0.41 m"),
    };

    static bool N72Set(MovementConfig mc, string dotted, double value)
    {
        string[] p = dotted.Split('.');
        if (p.Length != 2) return false;
        FieldInfo section = typeof(MovementConfig).GetField(p[0], BindingFlags.Public | BindingFlags.Instance);
        object sec = section?.GetValue(mc);
        FieldInfo f = sec?.GetType().GetField(p[1], BindingFlags.Public | BindingFlags.Instance);
        if (f == null) return false;
        if (f.FieldType == typeof(double)) f.SetValue(sec, value);
        else if (f.FieldType == typeof(int)) f.SetValue(sec, (int)Math.Round(value));
        else return false;
        return true;
    }

    static MovementConfig N72Profile(bool ultra, string pairs, List<string> notes)
    {
        GameConfig g = GameConfig.Load(Path.Combine(Root, "config.json"));
        MovementConfig mc = g.movement;
        if (ultra && g.Get("design.ultra71.set") is Dictionary<string, object> set)
            foreach (KeyValuePair<string, object> kv in set)
                if (!N72Set(mc, kv.Key, PortData.Num(kv.Value))) notes.Add("design.ultra71.set." + kv.Key + ": not a movement field");
        if (!string.IsNullOrEmpty(pairs))
            foreach (string pair in pairs.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                int eq = pair.IndexOf('=');
                if (eq < 0 || !double.TryParse(pair.Substring(eq + 1), NumberStyles.Float, N72Ci, out double v) || !N72Set(mc, pair.Substring(0, eq).Trim(), v)) notes.Add("--set " + pair + ": not a dotted movement path = number");
            }
        return mc;
    }

    static TriangleWorld N72World()
    {
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-300, -1, -300), new Vec3(300, 0, 300)); // the floor, top at y 0
        w.AddBox(new Vec3(4, 0, -300), new Vec3(5, 80, 300)); // a wall, its west face at x 4
        return w;
    }

    static Mover N72Settled(MovementConfig mc, double x = 0, double z = 0)
    {
        var m = new Mover(mc, N72World());
        m.Reset(x, 0, z);
        var f = new InputFrame();
        for (int i = 0; i < 30; i++) m.Step(f, N72Dt);
        return m;
    }

    static double[] N72Measure(MovementConfig mc)
    {
        var v = new double[N72Rows.Length];
        for (int i = 0; i < v.Length; i++) v[i] = double.NaN;
        double walk = mc.ground.walkSpeed;

        // ---- jump, double jump (the second press at the first apex) ----
        {
            Mover m = N72Settled(mc);
            double apex = 0, tApex = 0, air = double.NaN;
            bool left = false;
            for (int i = 0; i < 600; i++)
            {
                var f = new InputFrame();
                f.jump.held = true;
                f.jump.pressed = i == 0;
                m.Step(f, N72Dt);
                if (!m.grounded) left = true;
                if (m.position.y > apex) { apex = m.position.y; tApex = (i + 1) * N72Dt; }
                if (left && m.grounded && i > 3) { air = (i + 1) * N72Dt; break; }
            }
            v[0] = apex; v[1] = tApex; v[2] = air;
            Mover d = N72Settled(mc);
            double dApex = 0;
            bool second = false, dLeft = false;
            for (int i = 0; i < 900; i++)
            {
                var f = new InputFrame();
                f.jump.held = true;
                f.jump.pressed = i == 0;
                if (!second && i > 3 && d.velocity.y <= 0 && !d.grounded) { f.jump.pressed = true; second = true; }
                d.Step(f, N72Dt);
                if (!d.grounded) dLeft = true;
                if (d.position.y > dApex) dApex = d.position.y;
                if (dLeft && d.grounded && i > 3) break;
            }
            v[3] = second ? dApex : double.NaN;
        }

        // ---- dash from a steady walk ----
        {
            Mover m = N72Settled(mc);
            Vec3 start = default;
            int steps = 0;
            bool began = false;
            double peak = 0, dist = double.NaN;
            for (int i = 0; i < 400; i++)
            {
                var f = new InputFrame { moveZ = 1 };
                f.dash.pressed = i == 90;
                f.dash.held = i >= 90 && i < 92;
                m.Step(f, N72Dt);
                if (m.state == MoveState.Dash)
                {
                    if (!began) { began = true; start = m.position; }
                    steps++;
                    peak = Math.Max(peak, m.Speed);
                }
                else if (began) { dist = Math.Sqrt((m.position.x - start.x) * (m.position.x - start.x) + (m.position.z - start.z) * (m.position.z - start.z)); break; }
            }
            v[4] = dist; v[5] = peak; v[6] = began ? steps * N72Dt : double.NaN;
        }

        // ---- slide from a steady walk ----
        {
            Mover m = N72Settled(mc);
            double first = double.NaN, after1 = double.NaN;
            int slideAt = -1;
            for (int i = 0; i < 400; i++)
            {
                var f = new InputFrame { moveZ = 1 };
                if (i >= 90) { f.slide.held = true; f.slide.pressed = i == 90; }
                m.Step(f, N72Dt);
                if (m.state == MoveState.Slide && slideAt < 0) { slideAt = i; first = m.Speed; }
                if (slideAt >= 0 && i == slideAt + 119) { after1 = m.Speed; break; }
            }
            v[7] = first; v[8] = after1;
        }

        // ---- slam from 40 m ----
        {
            var m = new Mover(mc, N72World());
            m.Reset(-20, 40, 0);
            double maxFall = 0;
            bool slamState = false;
            for (int i = 0; i < 600; i++)
            {
                var f = new InputFrame();
                if (i >= 2) { f.slide.held = true; f.slide.pressed = i == 2; }
                m.Step(f, N72Dt);
                if (m.state == MoveState.Slam) { slamState = true; maxFall = Math.Max(maxFall, -m.velocity.y); }
                if (m.grounded && i > 4) break;
            }
            v[9] = slamState ? maxFall : double.NaN;
        }

        // ---- wall jump: strafe east into the wall (face x 4), jump, press jump again at the wall ----
        for (int pressAt = 20; pressAt <= 70 && double.IsNaN(v[10]); pressAt += 5)
        {
            Mover m = N72Settled(mc, 2.2, 0);
            bool got = false;
            double vx = double.NaN, vy = double.NaN;
            m.Events += e => { if (e.type == MoverEventType.WallJump) got = true; };
            for (int i = 0; i < 160; i++)
            {
                var f = new InputFrame { moveX = 1 };
                f.jump.held = i < pressAt + 20;
                f.jump.pressed = i == 0 || i == pressAt;
                m.Step(f, N72Dt);
                if (got) { vx = -m.velocity.x; vy = m.velocity.y; break; }
            }
            if (got) { v[10] = vx; v[11] = vy; }
        }

        // ---- stop from a steady walk, and the ground accel from rest ----
        {
            Mover m = N72Settled(mc);
            for (int i = 0; i < 120; i++) m.Step(new InputFrame { moveZ = 1 }, N72Dt);
            double t50 = double.NaN, t95 = double.NaN;
            for (int i = 1; i <= 240; i++)
            {
                m.Step(new InputFrame(), N72Dt);
                if (double.IsNaN(t50) && m.Speed <= 0.5 * walk) t50 = i * N72Dt;
                if (double.IsNaN(t95) && m.Speed <= 0.05 * walk) { t95 = i * N72Dt; break; }
            }
            v[12] = t50; v[13] = t95;
            Mover a = N72Settled(mc);
            double a50 = double.NaN, a95 = double.NaN;
            for (int i = 1; i <= 240; i++)
            {
                a.Step(new InputFrame { moveZ = 1 }, N72Dt);
                if (double.IsNaN(a50) && a.Speed >= 0.5 * walk) a50 = i * N72Dt;
                if (double.IsNaN(a95) && a.Speed >= 0.95 * walk) { a95 = i * N72Dt; break; }
            }
            v[14] = a50; v[15] = a95;
        }

        // ---- the air: from rest to 95 % of walk speed; a 90 degree turn at walk speed ----
        {
            var m = new Mover(mc, N72World());
            m.Reset(-20, 60, 0);
            double t = double.NaN;
            for (int i = 1; i <= 240; i++)
            {
                m.Step(new InputFrame { moveZ = 1 }, N72Dt);
                if (Math.Abs(m.velocity.z) >= 0.95 * walk) { t = i * N72Dt; break; }
            }
            v[16] = t;
            var k = new Mover(mc, N72World());
            k.Reset(-20, 60, 0);
            k.velocity = new Vec3(walk, 0, 0);
            double tt = double.NaN, kept = double.NaN;
            for (int i = 1; i <= 240; i++)
            {
                k.Step(new InputFrame { moveZ = 1 }, N72Dt);
                if (Math.Abs(k.velocity.z) >= 0.95 * walk) { tt = i * N72Dt; kept = 100.0 * Math.Abs(k.velocity.x) / walk; break; }
            }
            v[17] = tt; v[18] = kept;
        }

        // ---- bunny hops: a steady walk, then the reel's press rule (a press near the floor, buffered into the landing) ----
        {
            Mover m = N72Settled(mc);
            var kept = new List<double>();
            var at = new List<double>();
            int step = 0;
            m.Events += e => { if (e.type == MoverEventType.Jump && e.speedBefore > 0.5 && kept.Count < 4) { kept.Add(100.0 * e.speedAfter / e.speedBefore); at.Add(step * N72Dt); } };
            for (; step < 120; step++) m.Step(new InputFrame { moveZ = 1 }, N72Dt);
            for (int i = 0; i < 1500 && kept.Count < 4; i++, step++)
            {
                bool press = m.grounded || (!m.grounded && m.velocity.y < 0 && m.position.y <= -m.velocity.y * 0.05);
                var f = new InputFrame { moveZ = 1 };
                f.jump.held = true;
                f.jump.pressed = press;
                m.Step(f, N72Dt);
            }
            if (kept.Count >= 4)
            {
                v[19] = kept.Skip(1).Min(); // hops 2-4 are the landings' jumps (the first is the ground jump)
                v[20] = (at[3] - at[1]) / 2.0;
            }
        }
        // ---- the air strafe at its best: in the air the wish direction is turned each step to the angle that adds the most (the largest angle off the
        // velocity whose projection is still under walk speed minus one step's accel), so the number is the most a player can gain in one jump ----
        {
            Mover m = N72Settled(mc);
            for (int i = 0; i < 120; i++) m.Step(new InputFrame { moveZ = 1 }, N72Dt);
            double takeoff = m.Speed, landed = double.NaN;
            bool left = false;
            for (int i = 0; i < 600; i++)
            {
                var f = new InputFrame { moveZ = 1 };
                f.jump.held = true;
                f.jump.pressed = i == 0;
                if (left && !m.grounded)
                {
                    double sp = m.Speed;
                    double stepGain = mc.air.accel * walk * N72Dt;
                    double cosA = sp > 1e-6 ? Math.Max(-1, Math.Min(1, (walk - stepGain) / sp)) : 1;
                    double al = Math.Acos(cosA);
                    double ux = sp > 1e-6 ? m.velocity.x / sp : 0, uz = sp > 1e-6 ? m.velocity.z / sp : -1;
                    double wx = ux * Math.Cos(al) - uz * Math.Sin(al), wz = ux * Math.Sin(al) + uz * Math.Cos(al);
                    f.yaw = Math.Atan2(-wx, -wz);
                }
                m.Step(f, N72Dt);
                if (!m.grounded) left = true;
                if (left && m.grounded && i > 3) { landed = m.Speed; break; }
            }
            v[21] = double.IsNaN(landed) ? double.NaN : landed - takeoff;
        }
        // ---- the surfaces: the walk stop's distance with Mover.groundFrictionMult 1 (ground), 0.25 (ice), 2 (mud) ----
        {
            double[] mults = { 1.0, 0.25, 2.0, 1.5 / 31.07, 12.0 / 31.07 };
            var dist = new double[5];
            for (int k = 0; k < 5; k++)
            {
                Mover m = N72Settled(mc);
                m.groundFrictionMult = mults[k];
                for (int i = 0; i < 120; i++) m.Step(new InputFrame { moveZ = 1 }, N72Dt);
                double z0 = m.position.z;
                for (int i = 0; i < 1200 && m.Speed > 0.05; i++) m.Step(new InputFrame(), N72Dt);
                dist[k] = Math.Abs(m.position.z - z0);
            }
            v[22] = dist[0]; v[23] = dist[1]; v[24] = dist[2];
            v[25] = dist[0] > 1e-9 ? dist[1] / dist[0] : double.NaN;
            v[26] = dist[3]; v[27] = dist[4];
        }
        return v;
    }

    static string N72Cell(double value, double target, string unit)
    {
        if (double.IsNaN(value)) return "none";
        string s = value.ToString(Math.Abs(value) >= 100 ? "0.0" : "0.000", N72Ci);
        if (double.IsNaN(target) || target == 0) return s;
        double dev = 100.0 * (value - target) / target;
        return s + " (" + (dev >= 0 ? "+" : "") + dev.ToString("0", N72Ci) + "%)";
    }

    /// <summary>The walk stop on a surface: the steady walk speed (after 1 s), the distance covered from the release until the speed is under 0.05 m/s, and the time that takes.</summary>
    static (double steady, double dist, double secs) N72Stop(MovementConfig mc, double frictionMult)
    {
        Mover m = N72Settled(mc);
        m.groundFrictionMult = frictionMult;
        for (int i = 0; i < 120; i++) m.Step(new InputFrame { moveZ = 1 }, N72Dt);
        double steady = m.Speed, z0 = m.position.z;
        int n = 0;
        for (; n < 1200 && m.Speed > 0.05; n++) m.Step(new InputFrame(), N72Dt);
        return (steady, Math.Abs(m.position.z - z0), n * N72Dt);
    }

    /// <summary>`--solve ice=4.766,mud=0.437`: the groundFrictionMult that gives the walk stop that distance under the profile "now" (bisection: the distance falls as the multiplier rises). Prints the multiplier
    /// to 6 places, the distance it gives at that value and at the 4-place rounding, and the steady walk speed on that surface (the web's mud caps walking at 83 %).</summary>
    static string N72Solve(string spec, MovementConfig now, MovementConfig web)
    {
        var sb = new StringBuilder();
        foreach (string part in spec.Split(','))
        {
            int eq = part.IndexOf('=');
            string name = part.Substring(0, eq);
            double target = double.Parse(part.Substring(eq + 1), N72Ci);
            double lo = 0.001, hi = 8.0; // dist(lo) large, dist(hi) small
            for (int it = 0; it < 80; it++)
            {
                double mid = 0.5 * (lo + hi);
                if (N72Stop(now, mid).dist > target) lo = mid; else hi = mid;
            }
            double m6 = Math.Round(0.5 * (lo + hi), 6), m4 = Math.Round(m6, 4), m3 = Math.Round(m6, 3);
            var a = N72Stop(now, m6); var b = N72Stop(now, m4); var c = N72Stop(now, m3);
            double webMult = name == "ice" ? 0.25 : 2.0;
            var w = N72Stop(web, webMult);
            sb.AppendLine($"solve {name}: target stop {target.ToString("0.000", N72Ci)} m under the profile now: groundFrictionMult {m6.ToString("0.000000", N72Ci)} gives {a.dist.ToString("0.000", N72Ci)} m (steady walk {a.steady.ToString("0.00", N72Ci)} m/s, {a.secs.ToString("0.00", N72Ci)} s); rounded to 4 places {m4.ToString("0.0000", N72Ci)}: {b.dist.ToString("0.000", N72Ci)} m (steady {b.steady.ToString("0.00", N72Ci)}); to 3 places {m3.ToString("0.000", N72Ci)}: {c.dist.ToString("0.000", N72Ci)} m. The web's {name} (x{webMult.ToString(N72Ci)} on its friction 6, accel 10): stop {w.dist.ToString("0.000", N72Ci)} m, steady walk {w.steady.ToString("0.00", N72Ci)} m/s.");
        }
        return sb.ToString();
    }

    /// <summary>`--committed`: reads design.n26.surfaces.iceFrictionMult / mudFrictionMult from THIS tree's config (the values the game would use) and prints the walk stop under each
    /// (the profile "now" = design.ultra71.set), next to the plain ground's and the web's own ice / mud, so the committed numbers are checked, not just the solver's.</summary>
    static string N72Committed(MovementConfig now, MovementConfig web, GameConfig g)
    {
        double ice = g.Num("design.n26.surfaces.iceFrictionMult"), mud = g.Num("design.n26.surfaces.mudFrictionMult");
        var ground = N72Stop(now, 1.0);
        var i = N72Stop(now, ice); var m = N72Stop(now, mud);
        var wi = N72Stop(web, 0.25); var wm = N72Stop(web, 2.0);
        var sb = new StringBuilder();
        sb.AppendLine($"committed design.n26.surfaces: iceFrictionMult {ice.ToString("0.######", N72Ci)}, mudFrictionMult {mud.ToString("0.######", N72Ci)}; ground friction {now.ground.friction.ToString("0.##", N72Ci)}, accel {now.ground.accel.ToString("0.##", N72Ci)} (web: {web.ground.friction.ToString("0.##", N72Ci)} / {web.ground.accel.ToString("0.##", N72Ci)})");
        sb.AppendLine($"  plain ground (now): stop {ground.dist.ToString("0.000", N72Ci)} m, steady walk {ground.steady.ToString("0.00", N72Ci)} m/s");
        sb.AppendLine($"  ice (now, x{ice.ToString("0.######", N72Ci)}): stop {i.dist.ToString("0.000", N72Ci)} m in {i.secs.ToString("0.00", N72Ci)} s, steady walk {i.steady.ToString("0.00", N72Ci)} m/s | the web's ice (x0.25): stop {wi.dist.ToString("0.000", N72Ci)} m, steady {wi.steady.ToString("0.00", N72Ci)} m/s");
        sb.AppendLine($"  mud (now, x{mud.ToString("0.######", N72Ci)}): stop {m.dist.ToString("0.000", N72Ci)} m in {m.secs.ToString("0.00", N72Ci)} s, steady walk {m.steady.ToString("0.00", N72Ci)} m/s | the web's mud (x2): stop {wm.dist.ToString("0.000", N72Ci)} m, steady {wm.steady.ToString("0.00", N72Ci)} m/s");
        return sb.ToString();
    }

    static int N72Lab(string[] args)
    {
        var sets = new List<(string label, string pairs)>();
        string outFile = null, solve = null;
        bool committed = false;
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--committed") committed = true;
            else if (args[i] == "--solve" && i + 1 < args.Length) solve = args[++i];
            else if (args[i] == "--out" && i + 1 < args.Length) outFile = args[++i];
            else if (args[i] == "--set" && i + 1 < args.Length)
            {
                string s = args[++i];
                int c = s.IndexOf(':');
                sets.Add(c > 0 ? (s.Substring(0, c), s.Substring(c + 1)) : ("cand" + (sets.Count + 1), s));
            }
            else { Console.WriteLine("n72lab: unknown argument " + args[i]); return 2; }
        }
        var notes = new List<string>();
        var names = new List<string> { "web", "now" };
        var cfgs = new List<MovementConfig> { N72Profile(false, null, notes), N72Profile(true, null, notes) };
        foreach (var (label, pairs) in sets) { names.Add(label); cfgs.Add(N72Profile(true, pairs, notes)); }
        if (committed)
        {
            string text = "n72lab --committed (" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", N72Ci) + "): the real Mover (120 Hz), flat floor; the profile 'now' is design.ultra71.set of this tree\n" + N72Committed(cfgs[1], cfgs[0], GameConfig.Load(Path.Combine(Root, "config.json")));
            Console.Write(text);
            if (outFile != null) File.WriteAllText(outFile, text);
            return 0;
        }
        if (solve != null)
        {
            string text = "n72lab --solve " + solve + " (" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", N72Ci) + "): the profile 'now' is design.ultra71.set of this tree\n" + N72Solve(solve, cfgs[1], cfgs[0]);
            Console.Write(text);
            if (outFile != null) File.WriteAllText(outFile, text);
            return 0;
        }
        var res = cfgs.Select(N72Measure).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("n72lab " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", N72Ci) + ": the real Mover (120 Hz) on a flat floor and one wall; ULTRAKILL targets x0.5143 (movement.md); the sim's design rows NOT applied");
        foreach (string n in notes) sb.AppendLine("  note: " + n);
        sb.AppendLine("profiles: web = PortExport/config.json; now = web + design.ultra71.set; " + string.Join("; ", sets.Select(s => s.label + " = now + " + s.pairs)));
        var line = new StringBuilder("| measure | unit | target | web | now |");
        var sep = new StringBuilder("|---|---|---|---|---|");
        foreach (var s in sets) { line.Append(" " + s.label + " |"); sep.Append("---|"); }
        sb.AppendLine(line.ToString());
        sb.AppendLine(sep.ToString());
        for (int r = 0; r < N72Rows.Length; r++)
        {
            var row = N72Rows[r];
            var cells = new StringBuilder("| " + row.name + " | " + row.unit + " | " + (double.IsNaN(row.target) ? "none (" + row.note + ")" : row.target.ToString(N72Ci) + " (" + row.note + ")") + " |");
            for (int c = 0; c < res.Count; c++) cells.Append(" " + N72Cell(res[c][r], row.target, row.unit) + " |");
            sb.AppendLine(cells.ToString());
        }
        sb.AppendLine("");
        sb.AppendLine("toward the target against \"now\" (a row with a target and a value in both; |deviation| smaller = toward; equal within 0.5 % = same):");
        for (int c = 2; c < res.Count; c++)
        {
            int toward = 0, same = 0, away = 0, lost = 0;
            var awayRows = new List<string>();
            for (int r = 0; r < N72Rows.Length; r++)
            {
                double t = N72Rows[r].target;
                if (double.IsNaN(t)) continue;
                double a = res[1][r], b = res[c][r];
                if (double.IsNaN(a) || double.IsNaN(b)) { if (double.IsNaN(b) && !double.IsNaN(a)) { lost++; awayRows.Add(N72Rows[r].name + " (lost)"); } continue; }
                double da = Math.Abs(a - t) / t, db = Math.Abs(b - t) / t;
                if (Math.Abs(da - db) <= 0.005) same++;
                else if (db < da) toward++;
                else { away++; awayRows.Add(N72Rows[r].name + " " + N72Cell(a, t, "") + " -> " + N72Cell(b, t, "")); }
            }
            sb.AppendLine("  " + names[c] + ": toward " + toward + ", same " + same + ", away " + away + ", lost " + lost + (awayRows.Count > 0 ? "; away: " + string.Join("; ", awayRows) : ""));
        }
        Console.Write(sb.ToString());
        if (outFile != null) File.WriteAllText(outFile, sb.ToString());
        return 0;
    }
}
