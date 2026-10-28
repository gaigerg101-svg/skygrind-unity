// NIGHT72 builder N, slice N4: `n72cam` drives Cam72Math (the ULTRAKILL camera's three motions, Assets/Scripts/Game/Cam72Math.cs) at the Player's 60 fps through planted cases, one PASS / FAIL line each:
//   strafe        full strafe input held 1 s: the roll reaches +1.0 deg (a half second in at most), then lets go: it is back under 0.05 deg within 2 s (the 0.5 s smooth damp), never past 0
//   dash / slide  boosted strafe: the roll goes to +-5.0 deg at 100 deg/s x (|error| + 0.01)
//   fov           a dash forward narrows the FOV x0.95 (105 -> 99.75), backward widens it x1.1 (115.5), sideways nothing; the change arrives and goes at 300 deg/s (a frame at 60 fps)
//   drop          the eye drop reaches 0.32 m in a few frames and comes back; the ceiling clamp holds the eye under a 1.2 m roof (eye 1.08 m) and leaves a 4 m roof alone
// Exit 1 on any FAIL. Usage: dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll n72cam
using System;
using System.Runtime.CompilerServices;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void N72CamInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n72cam") return;
        int code;
        try { code = N72Cam(); }
        catch (Exception e) { Console.WriteLine("n72cam: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static int N72Cam()
    {
        int bad = 0;
        void Say(bool ok, string m) { Console.WriteLine((ok ? "PASS " : "FAIL ") + m); if (!ok) bad++; }
        const double dt = 1.0 / 60;
        // strafe: 1 s held, then released
        {
            double roll = 0, vel = 0, at1 = -1, peak = 0, back = -1, minAfter = 0;
            for (int i = 0; i < 60 * 4; i++)
            {
                double t = i * dt;
                double target = t < 1.0 ? Cam72Math.RollTarget(1.0, false) : 0;
                roll = Cam72Math.RollStep(roll, ref vel, target, false, dt);
                peak = Math.Max(peak, roll);
                if (at1 < 0 && roll >= 0.99) at1 = t;
                if (t >= 1.0) { minAfter = Math.Min(minAfter, roll); if (back < 0 && Math.Abs(roll) < 0.05) back = t - 1.0; }
            }
            Say(Math.Abs(peak - 1.0) < 0.01 && at1 > 0 && at1 < 0.5 && back > 0 && back < 2.0 && minAfter > -0.01, $"strafe: peak {peak:0.000} deg, reached at {at1:0.00} s, back under 0.05 deg {back:0.00} s after the release, undershoot {minAfter:0.0000}");
        }
        // boosted
        {
            double roll = 0, vel = 0, at5 = -1;
            for (int i = 0; i < 60; i++)
            {
                roll = Cam72Math.RollStep(roll, ref vel, Cam72Math.RollTarget(-1.0, true), true, dt);
                if (at5 < 0 && roll <= -4.95) at5 = i * dt;
            }
            // 100 deg/s x (|error| + 0.01) is 500 deg/s at a 5 deg error: the port's roll is there within a frame or two (a dash's strafe snaps, as ULTRAKILL's does)
            Say(Math.Abs(roll + 5.0) < 0.01 && at5 >= 0 && at5 < 0.1, $"dashing / sliding left: roll {roll:0.000} deg (+-5.0), 4.95 reached at {at5:0.000} s (100 deg/s x (error + 0.01): a frame or two)");
        }
        // fov
        {
            double baseFov = 105;
            double fwd = baseFov * (Cam72Math.FovFactor(1.0, true) - 1), back = baseFov * (Cam72Math.FovFactor(-1.0, true) - 1), side = baseFov * (Cam72Math.FovFactor(0.0, true) - 1), idle = baseFov * (Cam72Math.FovFactor(1.0, false) - 1);
            double off = 0;
            int frames = 0;
            while (Math.Abs(off - fwd) > 1e-9 && frames < 600) { off = Cam72Math.FovStep(off, fwd, dt); frames++; }
            double outOff = off;
            int back0 = 0;
            while (Math.Abs(outOff) > 1e-9 && back0 < 600) { outOff = Cam72Math.FovStep(outOff, 0, dt); back0++; }
            Say(Math.Abs(baseFov + fwd - 99.75) < 1e-6 && Math.Abs(baseFov + back - 115.5) < 1e-6 && side == 0 && idle == 0 && frames == 2 && back0 == 2,
                $"fov at 105: forward {baseFov + fwd:0.00} (x0.95), back {baseFov + back:0.00} (x1.1), sideways {baseFov + side:0.00}, not dashing {baseFov + idle:0.00}; the change takes {frames} frames in and {back0} out at 60 fps (300 deg/s)");
        }
        // drop
        {
            double drop = 0;
            int f = 0;
            while (drop < 0.32 - 1e-9 && f < 600) { drop = Cam72Math.DropStep(drop, 0.32, dt); f++; }
            int fin = f;
            int g2 = 0;
            while (drop > 1e-9 && g2 < 600) { drop = Cam72Math.DropStep(drop, 0, dt); g2++; }
            double standEye = 1.6;
            double low = Cam72Math.EyeY(standEye, 0.32, 1.2), high = Cam72Math.EyeY(standEye, 0.32, 4.0), none = Cam72Math.EyeY(standEye, 0.32, double.PositiveInfinity);
            Say(fin <= 4 && g2 <= 4 && Math.Abs(low - 1.08) < 1e-9 && Math.Abs(high - 1.28) < 1e-9 && Math.Abs(none - 1.28) < 1e-9,
                $"slide drop 0.32 m in {fin} frames and back in {g2}; the eye {none:0.00} m with no ceiling, {high:0.00} m under a 4 m roof, {low:0.00} m under a 1.2 m roof (the slide capsule is 1.0 m)");
        }
        Console.WriteLine("n72cam: " + (bad == 0 ? "all PASS" : bad + " FAIL"));
        return bad == 0 ? 0 : 1;
    }
}
