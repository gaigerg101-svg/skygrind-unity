// NIGHT48 builder F: `n48feel`, F's own meters (hooked without a Program.cs line: a module initializer runs the command and exits when argv[1]
// is "n48feel", Harness.N47.Feel.cs's pattern).
//   n48feel shield
//     G12's geometry, measured before any Player frame: the WARDEN's shield (an arc of the sim's +-60 deg block cone) and its arm, posed as
//     EnemyViews.Shield47.cs (foes47) and EnemyViews.Foes48.cs (foes48) pose them, at the eased down share k = 0 (up), 0.25, 0.5, 0.75, 1
//     (down), with no block kick and with the block's 4 deg kick, and with the tell / swing arm angles. The same test as the a948 probe
//     (Foes48.Probe): the shield's points against the body's drawn capsule (WardenView: a unit capsule r 0.5, y -0.5..0.5, at (0, h/2, 0),
//     scaled (1.5, h/2, 1.32)): the least clearance in m (< 0: inside) and the points inside; the pauldrons (models43: Pauldrons2 at 0.82 h,
//     two domes at x +-0.672 m, half sizes 0.378 x 0.30 x 0.40, taken as ellipsoids) and the helmet (1.02 h, half 0.35 x 0.275 x 0.33) the same
//     way; the arm's 8 corners out past the shield's front face; the shield's centre (x off the axis, y as a share of the height) and its
//     bottom over the floor. The numbers are UnityTuning.F48.cs's (compiled in) and foes47's (UnityTuning.F47.cs, mirrored below: that file
//     holds UnityEngine types the harness cannot compile). Unity's quaternions are Hamilton products (the same formula as Q48 below).
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
    internal static void F48FeelInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n48feel") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            string sub = args.Length > 1 ? args[1] : "shield";
            code = sub == "shield" ? F48Shield(args) : 2;
            if (code == 2) Console.WriteLine("n48feel: unknown subcommand " + sub + " (shield)");
        }
        catch (Exception e) { Console.WriteLine("n48feel: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    // foes47's numbers (UnityTuning.F47.cs; mirrored: keep equal)
    const float S47UpY = 0.565f, S47ArcR = 0.80f, S47ArcDeg = 60f, S47W = 1.45f, S47H = 1.75f, S47Thick = 0.10f, S47BodyHalfW = 0.75f, S47DownClear = 0.10f;
    const float S47DownY = 0.40f, S47DownZ = 0.10f, S47DownTiltDeg = 22f, S47KickDeg = 4f, S47ArmX = 0.55f, S47ArmUpY = 0.50f, S47ArmUpZ = 0.38f, S47ArmForwardDeg = 62f;
    const float WardenArmTell = -1.2f, WardenArmSwing = 1.1f; // UnityTuning.Warden20.cs (rad)

    struct V48
    {
        public double x, y, z;
        public V48(double x, double y, double z) { this.x = x; this.y = y; this.z = z; }
        public static V48 operator +(V48 a, V48 b) => new V48(a.x + b.x, a.y + b.y, a.z + b.z);
        public static V48 operator -(V48 a, V48 b) => new V48(a.x - b.x, a.y - b.y, a.z - b.z);
        public static V48 operator *(V48 a, double k) => new V48(a.x * k, a.y * k, a.z * k);
        public double Len => Math.Sqrt(x * x + y * y + z * z);
        public static V48 Lerp(V48 a, V48 b, double k) => a + (b - a) * k;
    }

    struct Q48
    {
        public double x, y, z, w;
        public Q48(double x, double y, double z, double w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public static Q48 Id => new Q48(0, 0, 0, 1);
        public static Q48 Axis(double deg, V48 a)
        {
            double l = a.Len, h = deg * Math.PI / 360, s = Math.Sin(h) / l;
            return new Q48(a.x * s, a.y * s, a.z * s, Math.Cos(h));
        }
        /// <summary>Unity's Quaternion.Euler(x, y, z): z first, then x, then y.</summary>
        public static Q48 Euler(double ex, double ey, double ez) => Axis(ey, new V48(0, 1, 0)) * Axis(ex, new V48(1, 0, 0)) * Axis(ez, new V48(0, 0, 1));
        public static Q48 operator *(Q48 a, Q48 b) => new Q48(
            a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
            a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
            a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w,
            a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z);
        public static V48 operator *(Q48 q, V48 v)
        {
            // v' = q v q*
            double tx = 2 * (q.y * v.z - q.z * v.y), ty = 2 * (q.z * v.x - q.x * v.z), tz = 2 * (q.x * v.y - q.y * v.x);
            return new V48(v.x + q.w * tx + (q.y * tz - q.z * ty), v.y + q.w * ty + (q.z * tx - q.x * tz), v.z + q.w * tz + (q.x * ty - q.y * tx));
        }
        public Q48 Inv => new Q48(-x, -y, -z, w);
        /// <summary>Unity's FromToRotation (the shortest arc).</summary>
        public static Q48 FromTo(V48 a, V48 b)
        {
            a = a * (1 / a.Len);
            b = b * (1 / b.Len);
            var c = new V48(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
            double d = a.x * b.x + a.y * b.y + a.z * b.z;
            var q = new Q48(c.x, c.y, c.z, 1 + d);
            double n = Math.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            return new Q48(q.x / n, q.y / n, q.z / n, q.w / n);
        }
    }

    /// <summary>A posed part: local point -> root point.</summary>
    struct Xf48
    {
        public V48 pos, scale;
        public Q48 rot;
        public V48 Of(V48 local) => pos + rot * new V48(local.x * scale.x, local.y * scale.y, local.z * scale.z);
        public V48 Into(V48 root)
        {
            V48 l = rot.Inv * (root - pos);
            return new V48(l.x / scale.x, l.y / scale.y, l.z / scale.z);
        }
    }

    /// <summary>The arc's points in METRES in its mesh frame (origin at the inner face's front centre, the axis at z = -r): both faces x 19
    /// angles x 5 heights, and the top / bottom edges.</summary>
    static List<V48> ArcPoints48(double r)
    {
        var pts = new List<V48>();
        double half = S47ArcDeg * Math.PI / 180, hh = S47H * 0.5;
        for (int face = 0; face < 2; face++)
            for (int i = 0; i <= 18; i++)
                for (int j = 0; j <= 4; j++)
                {
                    double a = -half + 2 * half * i / 18, rad = face == 0 ? r + S47Thick : r, y = -hh + 2 * hh * j / 4;
                    pts.Add(new V48(Math.Sin(a) * rad, y, Math.Cos(a) * rad - r));
                }
        return pts;
    }

    static int F48Shield(string[] args)
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        GameConfig cfg = Config();
        double h = cfg.Num("warden.height"), radius = cfg.Num("warden.radius");
        var body = new Xf48 { pos = new V48(0, h * 0.5, 0), scale = new V48(radius * 2 * 1.25, h / 2, radius * 2 * 1.1), rot = Q48.Id }; // WardenView.Make (WardenBodyX / Z)
        var pauldrons = new[] { (new V48(-0.672, 0.82 * h, 0), new V48(0.378, 0.30, 0.40)), (new V48(0.672, 0.82 * h, 0), new V48(0.378, 0.30, 0.40)) };
        var helmet = (new V48(0, 1.02 * h, 0.08), new V48(0.35, 0.275, 0.33));
        Console.WriteLine($"n48feel shield: warden height {h.ToString("0.00", ci)} m, the drawn body {(body.scale.x * 0.5).ToString("0.00", ci)} x {(body.scale.z * 0.5).ToString("0.00", ci)} m (half width x depth)");
        int bad48 = 0;
        foreach (string mode in new[] { "47", "48" })
        {
            double r = mode == "48" ? UnityTuning.Shield48ArcR : S47ArcR;
            List<V48> arc = ArcPoints48(r);
            foreach (double k in new[] { 0.0, 0.25, 0.5, 0.75, 1.0 })
                foreach ((string tag, double kick, double arm) in new[] { ("rest", 0.0, 0.0), ("block", S47KickDeg, 0.0), ("tell", 0.0, (double)WardenArmTell), ("swing", 0.0, (double)WardenArmSwing) })
                {
                    if (k > 0 && tag != "rest" && tag != "block") continue; // the tell / swing only while up
                    Xf48 shield, armX;
                    var sc = new V48(S47W, S47H, S47Thick);
                    if (mode == "47")
                    {
                        V48 upPos = new V48(0, h * S47UpY, S47ArcR), downPos = new V48(S47BodyHalfW + S47DownClear + S47Thick, h * S47DownY, S47DownZ);
                        // Unity's Slerp between Euler(-kick, 0, 0) and Euler(-22, -90, 0): at k 0 / 1 exactly; between: a normalized lerp (close enough for a probe)
                        Q48 a = Q48.Euler(-kick, 0, 0), b = Q48.Euler(-S47DownTiltDeg, -90, 0);
                        double dot = a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w, sg = dot < 0 ? -1 : 1;
                        var m = new Q48(a.x + (b.x * sg - a.x) * k, a.y + (b.y * sg - a.y) * k, a.z + (b.z * sg - a.z) * k, a.w + (b.w * sg - a.w) * k);
                        double n = Math.Sqrt(m.x * m.x + m.y * m.y + m.z * m.z + m.w * m.w);
                        shield = new Xf48 { pos = V48.Lerp(upPos, downPos, k), rot = new Q48(m.x / n, m.y / n, m.z / n, m.w / n), scale = new V48(1, 1, 1) };
                        armX = new Xf48
                        {
                            pos = V48.Lerp(new V48(S47ArmX, h * S47ArmUpY, S47ArmUpZ), new V48(S47BodyHalfW + 0.05, h * 0.45, 0.05), k),
                            rot = Q48.Euler(S47ArmForwardDeg * (1 - k) - arm * 180 / Math.PI, 0, 0),
                            scale = new V48(0.35, 1.3, 0.35),
                        };
                    }
                    else
                    {
                        // EnemyViews.Foes48.cs Shield48, line for line
                        double kY = Math.Min(1, Math.Max(0, k / UnityTuning.Shield48DropShare)), kS = Math.Min(1, Math.Max(0, (k - UnityTuning.Shield48SwingLag) / (1 - UnityTuning.Shield48SwingLag)));
                        kY = kY * kY * (3 - 2 * kY);
                        kS = kS * kS * (3 - 2 * kS);
                        double hh = S47H * 0.5, s = UnityTuning.Shield48DownSwingDeg * kS, tau = UnityTuning.Shield48DownTipDeg * kY, sRad = s * Math.PI / 180;
                        var d = new V48(Math.Sin(sRad), 0, Math.Cos(sRad));
                        Q48 qTip = Q48.Axis(tau, new V48(Math.Cos(sRad), 0, -Math.Sin(sRad)));
                        Q48 q = qTip * Q48.Euler(0, s, 0) * Q48.Euler(-kick, 0, 0);
                        double yb = (h * UnityTuning.Shield48UpY - hh) + (UnityTuning.Shield48DownBottomY - (h * UnityTuning.Shield48UpY - hh)) * kY;
                        V48 origin = d * r + new V48(0, yb, 0) + qTip * new V48(0, hh, 0);
                        double rho = r - (UnityTuning.Shield48GripIn + (UnityTuning.Shield48DownGripIn - UnityTuning.Shield48GripIn) * kS), g = UnityTuning.Shield48GripDeg * (1 - kS) * Math.PI / 180;
                        V48 grip = origin + q * new V48(rho * Math.Sin(g), UnityTuning.Shield48GripY, rho * Math.Cos(g) - r);
                        var shoulder = new V48(UnityTuning.Shield48ShoulderX, h * UnityTuning.Shield48ShoulderY, UnityTuning.Shield48ShoulderZ);
                        Q48 bash = Q48.Axis(UnityTuning.Shield48BashK * Math.Min(0, arm) * 180 / Math.PI * (1 - k), new V48(1, 0, 0));
                        var shove = new V48(0, 0, UnityTuning.Shield48ShoveM * Math.Min(1, Math.Max(0, arm / WardenArmSwing)) * (1 - k));
                        origin = shoulder + bash * (origin - shoulder) + shove;
                        grip = shoulder + bash * (grip - shoulder) + shove;
                        shield = new Xf48 { pos = origin, rot = bash * q, scale = new V48(1, 1, 1) };
                        V48 dir = grip - shoulder;
                        double len = Math.Max(0.05, dir.Len);
                        armX = new Xf48 { pos = (shoulder + grip) * 0.5, rot = Q48.FromTo(new V48(0, 1, 0), dir), scale = new V48(UnityTuning.Shield48ArmThick, len, UnityTuning.Shield48ArmThick) };
                    }
                    // the shield's points (metres in its frame; scale 1 here: the mesh's unit space times its localScale is metres)
                    double clear = double.MaxValue, pClear = double.MaxValue, top = double.MinValue, bottom = double.MaxValue;
                    int inside = 0, inP = 0, inHelm = 0;
                    double cx = 0, cy = 0;
                    foreach (V48 p in arc)
                    {
                        V48 w = shield.Of(p);
                        V48 qb = body.Into(w);
                        double dy = qb.y - Math.Max(-0.5, Math.Min(0.5, qb.y));
                        double dseg = Math.Sqrt(qb.x * qb.x + dy * dy + qb.z * qb.z);
                        if (dseg < 0.5) inside++;
                        clear = Math.Min(clear, (dseg - 0.5) * Math.Min(body.scale.x, body.scale.z));
                        foreach (var (c, e) in pauldrons)
                        {
                            double ex = (w.x - c.x) / e.x, ey = (w.y - c.y) / e.y, ez = (w.z - c.z) / e.z, rr = Math.Sqrt(ex * ex + ey * ey + ez * ez);
                            if (rr < 1) inP++;
                            pClear = Math.Min(pClear, (rr - 1) * Math.Min(e.x, Math.Min(e.y, e.z)));
                        }
                        {
                            var (c, e) = helmet;
                            double ex = (w.x - c.x) / e.x, ey = (w.y - c.y) / e.y, ez = (w.z - c.z) / e.z;
                            if (ex * ex + ey * ey + ez * ez < 1) inHelm++;
                        }
                        top = Math.Max(top, w.y);
                        bottom = Math.Min(bottom, w.y);
                        cx += w.x; cy += w.y;
                    }
                    cx /= arc.Count; cy /= arc.Count;
                    int outFront = 0;
                    double half = S47ArcDeg * Math.PI / 180;
                    for (int c = 0; c < 8; c++)
                    {
                        var corner = new V48((c & 1) == 0 ? -0.5 : 0.5, (c & 2) == 0 ? -0.5 : 0.5, (c & 4) == 0 ? -0.5 : 0.5);
                        V48 m = shield.Into(armX.Of(corner));
                        double rho = Math.Sqrt(m.x * m.x + (m.z + r) * (m.z + r)), ang = Math.Atan2(m.x, m.z + r);
                        if (rho > r + S47Thick && Math.Abs(ang) < half && Math.Abs(m.y) < S47H * 0.5) outFront++;
                    }
                    bool bad = inside > 0 || outFront > 0 || inP > 0 || inHelm > 0 || bottom < -0.01;
                    if (mode == "48" && bad) bad48++;
                    Console.WriteLine($"  foes{mode} k {k.ToString("0.00", ci)} {tag,-5}: body clear {clear.ToString("0.000", ci)} m (inside {inside} / {arc.Count}), pauldron clear {pClear.ToString("0.000", ci)} m (inside {inP}), helmet inside {inHelm}, arm corners out the front {outFront} / 8, centre x {cx.ToString("0.00", ci)} m y {(cy / h).ToString("0.00", ci)} h, top {top.ToString("0.00", ci)} bottom {bottom.ToString("0.00", ci)} m{(bad ? "  <- BAD" : "")}");
                }
        }
        Console.WriteLine($"G12 shield geometry (foes48): {(bad48 == 0 ? "GREEN" : "RED")} ({bad48} poses with a point inside the body / a pauldron / the helmet, the arm out the front or the shield under the floor)");
        return bad48 == 0 ? 0 : 1;
    }
}
