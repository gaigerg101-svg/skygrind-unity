// NIGHT41 builder P3 (aim41; INVENTORY C2, DECIDE41 D-6): the pad's ASSIST mode on a boss's weak point, before / after. One entry, N41Pad.
//   n41pad aim [--seeds 5] [--secs 20]
//     A Core rig (Core/AimAssist.cs, the pad's "assist" mode, a finite breakaway cap = gamepad.yawRate x breakawayShare) and one moving
//     boss body per case, its weak point PLANTED by this harness (until B8's LOOM / ICEBREAKER bodies land): knee (THE LOOM's lit knee:
//     2 m under the centre, 0.9 m aside, the body striding round the player), fist (THE ICEBREAKER's stuck fist: on the floor 2.5 m ahead
//     of a swaying body), core (the CYAN core in a window: 0.35 m, 0.6 m over the centre, a fast strafe), crown (THE CROWN: its opening =
//     the centre, passRadius). The pad pilot: aims AT the weak point (he knows to) with a 0.2 s reaction lag, the pad's top turn rate
//     (gamepad.yawRate deg/s) and +-1 deg of seeded jitter (Mulberry32); fires 4 times a second. A hit: the look ray within the weak point's
//     radius. Round 2 (the lead's ask): the pilot LEADS what he saw (its velocity over 0.1 s, times lag + 1/gain = 0.4 s ahead), so a
//     striding knee comes inside the assist's 6 deg cone; --nolead = round 1's pilot. Hits per second, AimAssist.weakPoint41 off (NIGHT40: the pull to the centre) vs on (aim41). PASS: on >= off on every case and
//     on > off on knee / fist / core.
using System;
using Skygrind.Core;

public static partial class Harness
{
    static partial void N41Pad(string cmd, string[] args, ref int code)
    {
        if (cmd != "n41pad") return;
        int seeds = 5;
        double secs = 20;
        for (int i = 1; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") seeds = int.Parse(args[i + 1]);
            if (args[i] == "--secs") secs = double.Parse(args[i + 1], System.Globalization.CultureInfo.InvariantCulture);
        }
        PadLead41 = Array.IndexOf(args, "--nolead") < 0;
        PadFriction41 = Array.IndexOf(args, "--nofriction") < 0;
        {
            // round 3: design.n41.aim41.reachM / acquireScale as the sim hands them (N41AimStartRun); --reach m / --acq x override; 0 / 1 = round 2's assist
            GameConfig c0 = Config();
            PadReach41 = c0.Has("design.n41.aim41.reachM") ? c0.Num("design.n41.aim41.reachM") : 0;
            PadAcq41 = c0.Has("design.n41.aim41.acquireScale") ? c0.Num("design.n41.aim41.acquireScale") : 1;
            int ri = Array.IndexOf(args, "--reach"), ai = Array.IndexOf(args, "--acq");
            if (ri > 0 && ri < args.Length - 1) PadReach41 = double.Parse(args[ri + 1], System.Globalization.CultureInfo.InvariantCulture);
            if (ai > 0 && ai < args.Length - 1) PadAcq41 = double.Parse(args[ai + 1], System.Globalization.CultureInfo.InvariantCulture);
            Console.WriteLine($"n41pad: aim41 reachM {PadReach41}, acquireScale {PadAcq41}");
        }
        if (args.Length > 1 && args[1] == "glyphs") // round 2 C1: every n40 / n41 line's {token} has a pad button (Harness.N41.PadGlyphs.cs)
        {
            int gf = N41PadGlyphs();
            Console.WriteLine(gf == 0 ? "n41pad glyphs: GREEN" : $"n41pad glyphs: RED ({gf} failing)");
            code = gf == 0 ? 0 : 1;
            return;
        }
        if (args.Length > 1 && args[1] == "real") // round 2: the real bodies (Harness.N41.PadReal.cs)
        {
            int rf = N41PadReal(args);
            Console.WriteLine(rf == 0 ? "n41pad real: GREEN" : $"n41pad real: RED ({rf} failing)");
            code = rf == 0 ? 0 : 1;
            return;
        }
        GameConfig cfg = Config();
        int fails = 0;
        foreach (string c in new[] { "knee", "fist", "core", "crown" })
        {
            double off = 0, on = 0;
            for (int s = 1; s <= seeds; s++)
            {
                off += PadWeakRun(cfg, c, (uint)s, secs, false);
                on += PadWeakRun(cfg, c, (uint)s, secs, true);
            }
            off /= seeds;
            on /= seeds;
            bool ok = c == "crown" ? on >= off - 1e-9 : on > off;
            if (!ok) fails++;
            Console.WriteLine($"n41pad aim {c,-5}: hits/s off {off:0.000}  on {on:0.000}  ({(off > 0 ? (on / off - 1) * 100 : 0):+0;-0} %)  {(ok ? "PASS" : "FAIL")}");
        }
        Console.WriteLine(fails == 0 ? "n41pad: GREEN" : $"n41pad: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    /// <summary>Round 2: the pilot leads what he sees (its velocity over the last 0.1 s he saw, by his lag + his thumb's time constant); --nolead = round 1's pilot.</summary>
    static bool PadLead41 = true;

    /// <summary>Round 3: the stick x AimAssist.LookScale (ASSIST's friction 0.55 on the locked body), as SurvivalGame.cs:283 does; --nofriction = rounds 1-2.</summary>
    static bool PadFriction41 = true;

    /// <summary>Round 3: the rig's AimAssist.weakReach41 / weakAcquire41 (design.n41.aim41.reachM / acquireScale, or --reach / --acq).</summary>
    static double PadReach41, PadAcq41 = 1;

    sealed class PadBoss41 : Enemy
    {
        public override double Height => 6;
        public override double Radius => 1.5;
        public override double AimHeight => 3;
        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            headshot = false;
            Vec3 c = AimPoint;
            double bx = c.x - o.x, by = c.y - o.y, bz = c.z - o.z;
            double t = bx * d.x + by * d.y + bz * d.z;
            if (t < 0) return -1;
            double qx = o.x + d.x * t - c.x, qy = o.y + d.y * t - c.y, qz = o.z + d.z * t - c.z;
            return qx * qx + qy * qy + qz * qz <= 3 * 3 ? t : -1;
        }
    }

    /// <summary>One run: hits per second on the planted weak point.</summary>
    static double PadWeakRun(GameConfig cfg, string kase, uint seed, double secs, bool aim41)
    {
        double dt = cfg.Dt, DEG = Math.PI / 180;
        var events = new EventBus();
        var enemies = new EnemyRegistry();
        var world = new TriangleWorld();
        world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
        Vec3 eye = new Vec3(0, 1.6, 0);
        double yaw = 0, pitch = 0;
        double cap = cfg.Num("gamepad.yawRate") * cfg.Num("gamepad.breakawayShare");
        var assist = new AimAssist(cfg, events, enemies, world, () => (eye, AssistMath.LookDir(yaw, pitch)), () => "revolver", () => false, () => "assist",
            () => cfg.Num("settings.snapStrength"), () => cap) { weakPoint41 = aim41, weakReach41 = PadReach41, weakAcquire41 = PadAcq41 };
        var boss = new PadBoss41 { kind = "padBoss41", id = 1, alive = true, hp = 1e9, maxHp = 1e9 };
        enemies.all.Add(boss);
        double radius = kase == "core" ? 0.35 : kase == "crown" ? (cfg.Has("design.n37.crown37.crown.passRadius") ? cfg.Num("design.n37.crown37.crown.passRadius") : 1.2) : 0.45;
        var rng = new Mulberry32(seed * 7919 + 41);
        double phase = rng.Next() * Math.PI * 2;
        int lag = MovementMath.StepsFor(0.2, dt), every = MovementMath.StepsFor(0.25, dt), n = MovementMath.StepsFor(secs, dt);
        int span = MovementMath.StepsFor(0.1, dt), size = lag + span + 1;
        var past = new Vec3[size];
        double leadT = 0.2 + 1.0 / 5; // the reaction lag + the thumb's time constant (1 / its gain)
        double turn = cfg.Num("gamepad.yawRate") * DEG, jy = 0, jp = 0;
        int hits = 0;
        Vec3 Weak(double t, out Vec3 pos)
        {
            switch (kase)
            {
                case "knee": // strides round the player at 14 m, 5 m/s
                {
                    double a = phase + t * 5 / 14;
                    pos = new Vec3(Math.Sin(a) * 14, 0, -Math.Cos(a) * 14);
                    double ax = Math.Cos(a), az = Math.Sin(a); // the stride's direction (tangent)
                    return new Vec3(pos.x + ax * 0.9, pos.y + 1.0, pos.z + az * 0.9);
                }
                case "fist": // the body sways 1.5 m at 0.7 Hz, 16 m ahead; the fist stuck on the floor 2.5 m in front of it
                {
                    pos = new Vec3(Math.Sin(phase + t * 2 * Math.PI * 0.7) * 1.5, 0, -16);
                    return new Vec3(0.8, 0.4, -13.5);
                }
                case "core": // a fast strafe (8 m at 0.5 Hz), 12 m ahead; the core 0.6 m over the centre
                {
                    pos = new Vec3(Math.Sin(phase + t * Math.PI) * 4, 0, -12);
                    return new Vec3(pos.x, pos.y + 3.6, pos.z);
                }
                default: // crown: the ring drifts beside the player; its opening = the centre
                {
                    pos = new Vec3(Math.Sin(phase + t * 0.6) * 5, 2 + Math.Sin(t * 0.9), -10);
                    return new Vec3(pos.x, pos.y + 3, pos.z);
                }
            }
        }
        // start on the body's centre
        Weak(0, out Vec3 p0);
        boss.position = p0;
        (yaw, pitch) = AssistMath.YawPitchTo(eye, boss.AimPoint);
        for (int i = 0; i < size; i++) past[i] = Weak(0, out _);
        for (int k = 0; k < n; k++)
        {
            double t = (k + 1) * dt;
            Vec3 w = Weak(t, out Vec3 pos);
            boss.position = pos;
            boss.weakPoint41.Set(w);
            past[k % size] = w;
            Vec3 seen = past[(k + span + 1) % size]; // lag steps ago
            if (PadLead41)
            {
                Vec3 older = past[(k + 1) % size]; // lag + span steps ago
                double f = leadT / (span * dt);
                seen = new Vec3(seen.x + (seen.x - older.x) * f, seen.y + (seen.y - older.y) * f, seen.z + (seen.z - older.z) * f);
            }
            if (k % 12 == 0) { jy = (rng.Next() * 2 - 1) * DEG; jp = (rng.Next() * 2 - 1) * DEG; }
            var (ty, tp) = AssistMath.YawPitchTo(eye, seen);
            double dy = Math.Atan2(Math.Sin(ty + jy - yaw), Math.Cos(ty + jy - yaw)), dp = tp + jp - pitch;
            dy *= 5 * dt; dp *= 5 * dt; // a thumb, not a servo: the stick moves 5 x the seen error per second (then the pad's top turn rate)
            double mag = Math.Sqrt(dy * dy + dp * dp), most = turn * dt;
            if (mag > most) { dy *= most / mag; dp *= most / mag; }
            if (PadFriction41) { double ls = assist.LookScale; dy *= ls; dp *= ls; } // round 3: the game scales the stick by the lock's friction (SurvivalGame.cs:283, input.sensMult = ... x assist.LookScale)
            yaw += dy;
            pitch += dp;
            bool shoot = k % every == 0;
            var frame = new CombatInput { weapon = -1, fire = new Button { held = shoot, pressed = shoot } };
            assist.Step(dt, ref frame, ref yaw, ref pitch);
            if (frame.fire.pressed)
            {
                Vec3 d = AssistMath.LookDir(yaw, pitch);
                double bx = w.x - eye.x, by = w.y - eye.y, bz = w.z - eye.z;
                double tt = bx * d.x + by * d.y + bz * d.z;
                double qx = eye.x + d.x * tt - w.x, qy = eye.y + d.y * tt - w.y, qz = eye.z + d.z * tt - w.z;
                if (tt > 0 && qx * qx + qy * qy + qz * qz <= radius * radius) hits++;
            }
        }
        if (Environment.GetEnvironmentVariable("N41PAD_DEBUG") == "1") Console.WriteLine($"  {kase} seed {seed} aim41 {aim41}: locks {assist.locks} assisted {assist.assisted} holds {assist.holds} target {(assist.Target != null)} params {(assist.Params() != null)}");
        return hits / secs;
    }
}
