// NIGHT43 builder M1 round 2 (the lead's card: G13 at the root): THE STACK climbed on its real geometry (Core/Enemies/Wick37Stack, design.n37.wick37.stack,
// FillWorld at the origin: the shaft, 15 spiral panels 4 per turn 2.8 m apart, the ledges, the cap) by the REAL Mover with the game's ride rules
// (N43Mover: flow34, wallride35 + ride39, wallgrip36, ride40, wallfling42 per the design) and M1's rows off (BEFORE) / on (AFTER). No boss, no slag,
// no hot wall (B1's n43wick has them): the movement alone. Called from n43move (Harness.N43.Move.cs): `n43move stack [--seeds N] [--trace]`.
// The pilot (M1's own, Unity-free): it targets the next panel in the spiral; approaching, the stick aims 1 m behind the panel's face 25 % along it
// (into the wall, as a person rides) and the camera faces the way it moves; riding panel k, the stick runs along the face and 0.4 into it, the
// camera looks along the face and up `pitch`; at `frac` of the face it leaves:
//   held    jump held into each panel, released at frac (wallride35's jump-off), pressed and held again 1.2 m before the next face (the buffer);
//   press   no hold anywhere: on a ride, a tap at frac (a pressed wall jump);
//   mixed   held on even panels, press on odd ones;
//   casual  held, frac 0.35-0.85 and pitch 28-50 deg drawn per panel (Mulberry32, seeded).
// On the floor or a ledge it jumps (held: and holds) within 3 m of the next face. A run ends at the cap (the feet on it), on the floor after a ride,
// or after 60 s. Each seed jitters frac (+-0.08) and pitch (+-4 deg) and the start.
using System;
using System.Collections.Generic;
using Skygrind.Core;

public static partial class Harness
{
    struct N43StackRun
    {
        public bool cap;
        public double maxY, capT;
        public int maxPanel, rides, wallJumps, doubles, reRide, locked, noIntent, shortWall, low, slow, ends;
        public string panels, end;
    }

    static N43StackRun N43StackClimb(GameConfig cfg, Wick37Stack st, string policy, bool chimney, bool ride, uint seed, Action<Mover> tweak = null)
    {
        double dt = 1 / cfg.movement.loop.simHz;
        var w = new TriangleWorld();
        st.FillWorld(w, Vec3.Zero);
        var log = new List<MoverEvent>();
        Mover m = N43Mover(cfg, w, chimney, ride, log);
        tweak?.Invoke(m);
        var rng = new Mulberry32(seed);
        double J(double a) => (rng.Next() * 2 - 1) * a;
        double baseFrac = policy == "press" ? 0.75 : 0.6, basePitch = policy == "press" ? 30 : 40;
        double frac = baseFrac + J(0.08), pitchDeg = basePitch + J(4);
        Wick37Panel p0 = st.panels[0];
        Vec3 t0 = Vec3.Normalize(p0.end - p0.start);
        Vec3 start = p0.start - t0 * 2.5 + p0.normal * (3.0 + J(0.3)) + new Vec3(0, 0.05, 0); // inside the shaft (radius 9)
        m.Reset(start.x, start.y, start.z);
        var res = new N43StackRun { end = "time", maxPanel = -1 };
        var ridden = new List<int>();
        int k = 0, seen = 0, tapLeft = 0, lastRidePanel = -1;
        bool hold = false, pressedThisRide = false, ridingNow = false;
        for (int s = 0; s < (int)Math.Round(60 / dt); s++)
        {
            if (k >= st.panels.Count) k = st.panels.Count - 1;
            Wick37Panel P = st.panels[k];
            Vec3 t = Vec3.Normalize(P.end - P.start), n = P.normal;
            double len = Vec3.DistanceTo(P.start, P.end);
            bool heldPolicy = policy == "held" || policy == "casual" || (policy == "mixed" && (k & 1) == 0);
            var f = new InputFrame();
            bool press = false;
            double yaw, pitch = pitchDeg * Math.PI / 180;
            Vec3 stickDir;
            int riding = -1;
            if (m.state == MoveState.WallRide)
            {
                riding = st.NearestPanel(m.position, out double dd);
                if (riding >= 0 && Vec3.Dot(m.wallNormal, st.panels[riding].normal) < 0.9) riding = -1; // riding the shaft wall, not a panel
            }
            if (riding >= 0)
            {
                if (!ridingNow || riding != lastRidePanel)
                {
                    ridingNow = true;
                    lastRidePanel = riding;
                    pressedThisRide = false;
                    if (ridden.Count == 0 || ridden[ridden.Count - 1] != riding) ridden.Add(riding);
                    res.maxPanel = Math.Max(res.maxPanel, riding);
                    if (policy == "casual") { frac = 0.35 + rng.Next() * 0.5; pitchDeg = 28 + rng.Next() * 22; }
                }
                k = riding;
                P = st.panels[k];
                t = Vec3.Normalize(P.end - P.start);
                n = P.normal;
                len = Vec3.DistanceTo(P.start, P.end);
                heldPolicy = policy == "held" || policy == "casual" || (policy == "mixed" && (k & 1) == 0);
                double at = ((m.position.x - P.start.x) * t.x + (m.position.z - P.start.z) * t.z) / len;
                stickDir = new Vec3(t.x - n.x * 0.4, 0, t.z - n.z * 0.4);
                yaw = YawOf(t);
                if (heldPolicy)
                {
                    if (at >= frac) hold = false;
                }
                else if (at >= frac && !pressedThisRide)
                {
                    press = true;
                    pressedThisRide = true;
                    tapLeft = 3;
                }
            }
            else
            {
                ridingNow = false;
                // approach panel k: 1 m behind its face, 25 % along it
                Vec3 aim = P.start + t * (len * 0.25) - n * 1.0;
                Vec3 to = new Vec3(aim.x - m.position.x, 0, aim.z - m.position.z);
                double tl = Math.Sqrt(to.x * to.x + to.z * to.z);
                stickDir = tl > 1e-6 ? to / tl : t;
                Vec3 hv = new Vec3(m.velocity.x, 0, m.velocity.z);
                yaw = hv.Length > 1 ? YawOf(hv / hv.Length) : YawOf(stickDir);
                double faceDist = N43FaceDist(P, m.position);
                if (m.grounded)
                {
                    if (faceDist < 1.6 && m.position.y < P.bottom + 0.5)
                    {
                        press = true;
                        tapLeft = 6;
                        hold = heldPolicy;
                    }
                }
                else if (heldPolicy && !hold && faceDist < 1.2)
                {
                    press = true;
                    hold = true;
                }
            }
            if (tapLeft > 0) tapLeft--;
            double sl = Math.Sqrt(stickDir.x * stickDir.x + stickDir.z * stickDir.z);
            Vec3 sd = sl > 1e-9 ? stickDir / sl : t;
            f.moveX = sd.x * Math.Cos(yaw) - sd.z * Math.Sin(yaw);
            f.moveZ = -sd.x * Math.Sin(yaw) - sd.z * Math.Cos(yaw);
            f.yaw = yaw;
            f.pitch = pitch;
            f.jump = new Button { pressed = press, held = press || hold || tapLeft > 0 };
            m.Step(f, dt);
            for (; seen < log.Count; seen++)
            {
                MoverEvent e = log[seen];
                if (e.type == MoverEventType.WallJump)
                {
                    res.wallJumps++;
                    if (lastRidePanel >= 0) k = lastRidePanel + 1;
                    hold = false;
                }
                else if (e.type == MoverEventType.DoubleJump) res.doubles++;
                else if (e.type == MoverEventType.WallRideStart) res.rides++;
                else if (e.type == MoverEventType.WallRideEnd && e.reason != "jump") res.ends++;
            }
            res.maxY = Math.Max(res.maxY, m.position.y);
            if (n42Trace && s % 30 == 0)
                Console.WriteLine($"      {policy} s {s} y {F42(m.position.y)} k {k} {m.state} v {F42(m.velocity.x)},{F42(m.velocity.y)},{F42(m.velocity.z)} hold {hold} reRide {m.flow34.ride35.reRideBlocks} lock {m.flow34.ride35.r40Locked} short {m.flow34.ride35.r40Short} stick {m.flow34.ride35.r40NoIntent}");
            if (m.grounded && m.position.y >= st.p.capY - 0.1) { res.cap = true; res.capT = s * dt; res.end = "CAP"; break; }
            if (m.grounded && m.position.y < 0.3 && ridden.Count > 0 && s > 120) { res.end = "floor"; break; }
        }
        ref Ride35.State q = ref m.flow34.ride35;
        res.reRide = q.reRideBlocks;
        res.locked = q.r40Locked;
        res.noIntent = q.r40NoIntent;
        res.shortWall = q.r40Short;
        res.low = q.r40Low;
        res.slow = q.r40Slow;
        res.panels = string.Join(" ", ridden);
        return res;
    }

    static double YawOf(Vec3 d) => Math.Atan2(-d.x, -d.z);

    /// <summary>The horizontal distance from feet `l` to panel P's ride-face segment.</summary>
    static double N43FaceDist(Wick37Panel P, Vec3 l)
    {
        Vec3 a = P.start, ab = P.end - P.start;
        double tt = Math.Max(0, Math.Min(1, ((l.x - a.x) * ab.x + (l.z - a.z) * ab.z) / (ab.x * ab.x + ab.z * ab.z)));
        return JsMath.Hypot(l.x - (a.x + ab.x * tt), l.z - (a.z + ab.z * tt));
    }

    static int N43Stack(GameConfig cfg, string[] args)
    {
        int seeds = 10;
        int si = Array.IndexOf(args, "--seeds");
        if (si >= 0 && si + 1 < args.Length) seeds = int.Parse(args[si + 1]);
        string only = Array.IndexOf(args, "--policy") is int pi && pi >= 0 && pi + 1 < args.Length ? args[pi + 1] : null;
        var st = new Wick37Stack(Wick37StackParams.From(cfg));
        Console.WriteLine($"n43move stack: THE STACK's real geometry ({st.panels.Count} panels, {st.p.perTurn} per turn, rise {F42(st.p.rise)} m, panel {F42(st.p.panelLength)} x {F42(st.p.panelHeight)} m, ledges at {string.Join(" / ", Array.ConvertAll(st.p.ledges, v => F42(v)))}, cap {F42(st.p.capY)} m), the real Mover, M1's pilot, {seeds} seeds, 60 s");
        Console.WriteLine("  policy  rows                | cap   median s | best panel (of 14) | rides  wall jumps  doubles | refused: re-ride  locked  no-intent  short  low  slow | first seed's panels");
        foreach (string policy in new[] { "held", "casual", "press", "mixed" })
        {
            if (only != null && policy != only) continue;
            foreach ((bool ch, bool ri, string rows) in new[] { (false, false, "before (NIGHT42)"), (true, true, "chimney43+ride43") })
            {
                int caps = 0;
                var times = new List<double>();
                int best = -1;
                var sum = new N43StackRun();
                string first = null;
                for (uint sd = 1; sd <= seeds; sd++)
                {
                    N43StackRun r = N43StackClimb(cfg, st, policy, ch, ri, sd);
                    if (r.cap) { caps++; times.Add(r.capT); }
                    best = Math.Max(best, r.maxPanel);
                    sum.rides += r.rides; sum.wallJumps += r.wallJumps; sum.doubles += r.doubles;
                    sum.reRide += r.reRide; sum.locked += r.locked; sum.noIntent += r.noIntent; sum.shortWall += r.shortWall; sum.low += r.low; sum.slow += r.slow;
                    first ??= $"[{r.panels}] {r.end} max {F42(r.maxY)} m";
                }
                times.Sort();
                string med = times.Count > 0 ? F42(times[times.Count / 2]) : "-";
                Console.WriteLine($"  {policy,-7} {rows,-19} | {caps,2}/{seeds} {med,7} | {best,2} | {sum.rides,5}  {sum.wallJumps,10}  {sum.doubles,7} | {sum.reRide,16}  {sum.locked,6}  {sum.noIntent,9}  {sum.shortWall,5}  {sum.low,3}  {sum.slow,4} | {first}");
            }
        }
        return 0;
    }
}
