// NIGHT35 builder M (NIGHT35.md M1): `n35move pull`, design.n35.grapple35's before / after on the real Mover + the real tap latch (GrappleTap31)
// + the arrival rule (GrapplePull31Rules.Arrive) in a box world, wired as SurvivalSim.Grapple31 / 32 / Move35 wire them:
//   BEFORE: NIGHT31's latch (tap window 0.2 s) with NIGHT32's tapNodesOnly, NIGHT31's arrival (keep 12 m/s, pop 5).
//   AFTER:  grapple35 (pullOnFire, a bare wall not refused, the row's arrival).
// Scenarios (the player standing at the origin, the button pressed once and HELD 3 s unless "tap": up after 3 steps):
//   node-hold  a ring node 15 m ahead, 6 m up            wall-hold  a bare wall point 30 m ahead, 6 m right, 3 m up      wall-tap  the same, a tap
// Per scenario: arrived (the Mover's "zip" release), the time to it, the pull's speed min..max, the path's largest distance from the straight
// line chest -> anchor (straightness), the closest the chest came to the anchor, the speed after the arrival rule, the distance from the anchor 0.5 s later.
// PASS: AFTER arrives in all three with the path within 0.25 m of the line; BEFORE's holds do not arrive (the swing) and its wall tap is refused.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N35MoveMorePartial(string sub, string[] args, ref int code)
    {
        if (sub == "pull") code = N35PullCommand(args);
        else if (sub == "walk") code = N35WalkCommand(args);
        else if (sub == "verbs") code = N35VerbsCommand(args);
    }

    sealed class N35PullResult
    {
        public bool arrived, refused;
        public double time = double.NaN, vMin = double.MaxValue, vMax, maxOff, closest = double.MaxValue, after = double.NaN, settle = double.NaN;
        public string end = "none";
    }

    static N35PullResult N35Pull(GameConfig cfg, double dt, bool after, Vec3 anchor, int node, bool tap, int pressSteps = 1)
    {
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
        w.AddBox(new Vec3(-30, 0, -31), new Vec3(30, 20, -30)); // a wall, its face at z = -30
        ArclineConfig a = cfg.movement.arcline;
        GrapplePull31Rules rules = GrapplePull31Rules.From(cfg);
        if (after) rules = new GrapplePull31Rules { speed = rules.speed, tapWindow = rules.tapWindow, arrivePop = cfg.Num("design.n35.grapple35.arrivePop"), arriveKeepMax = cfg.Num("design.n35.grapple35.arriveKeepMax") };
        double webZip = a.zip.reelSpeed;
        a.zip.reelSpeed = rules.speed;
        var log = new List<MoverEvent>();
        var m = new Mover(cfg.movement, w);
        m.SetFlow34(Flow34.Params.From(cfg, cfg.Design("n34.flow34.enabled")));
        m.ropeEnabled = true;
        m.Events += e => log.Add(e);
        var t = new GrappleTap31();
        t.Reset(MovementMath.StepsFor(rules.tapWindow, dt));
        t.tapNodesOnly = !after && cfg.Design("n32.grapple32.tapNodesOnly");
        t.pullOnFire = after;
        m.Reset(0, 0.02, 0);
        for (int s = 0; s < 20; s++) m.Step(new InputFrame(), dt);
        var res = new N35PullResult();
        Vec3 from = default;
        int fired = -1, arrivedAt = -1;
        for (int s = 0; s < 360; s++)
        {
            bool held = tap ? s < 3 : true;
            var f = new InputFrame { grapple = new Button { pressed = s < pressSteps, held = held || s < pressSteps }, grappleTarget = s == 0 ? new GrappleTarget { point = anchor, node = node } : (GrappleTarget?)null };
            GrappleTapAction act = t.Apply(ref f, m.rope);
            if (act == GrappleTapAction.Refused) res.refused = true;
            int n0 = log.Count;
            m.Step(f, dt);
            if (fired < 0 && m.rope.phase != RopePhase.None)
            {
                fired = s;
                from = m.RopePoint;
            }
            for (int i = n0; i < log.Count; i++)
            {
                if (log[i].type != MoverEventType.RopeRelease) continue;
                res.end = log[i].reason;
                if (log[i].reason == "zip" && arrivedAt < 0)
                {
                    arrivedAt = s;
                    res.arrived = true;
                    res.time = (s - fired) * dt;
                    m.velocity = rules.Arrive(m.velocity);
                    res.after = m.Speed;
                }
            }
            Vec3 c = m.RopePoint;
            res.closest = Math.Min(res.closest, Vec3.DistanceTo(c, anchor));
            if (fired >= 0 && arrivedAt < 0 && m.rope.phase == RopePhase.Attached)
            {
                double sp = m.velocity.Length;
                res.vMin = Math.Min(res.vMin, sp);
                res.vMax = Math.Max(res.vMax, sp);
                // the distance from the straight line from -> anchor
                Vec3 d = anchor - from;
                double L = d.Length;
                Vec3 u = d * (1 / L);
                Vec3 q = c - from;
                double along = Vec3.Dot(q, u);
                Vec3 perp = q - u * along;
                res.maxOff = Math.Max(res.maxOff, perp.Length);
            }
            if (arrivedAt >= 0 && s == arrivedAt + 60)
            {
                res.settle = Vec3.DistanceTo(m.RopePoint, anchor);
                break;
            }
            if (fired >= 0 && m.rope.phase == RopePhase.None && arrivedAt < 0 && s > fired + 2 && !tap) break;
        }
        a.zip.reelSpeed = webZip;
        if (res.vMin == double.MaxValue) res.vMin = double.NaN;
        return res;
    }

    static int N35PullCommand(string[] args)
    {
        GameConfig cfg = Config();
        double dt = 1 / cfg.movement.loop.simHz;
        Console.WriteLine($"n35move pull (design.n35.grapple35 enabled {cfg.Design("n35.grapple35.enabled")}, swing {cfg.Design("n35.grapple35.swing")}; AFTER forced on: arrival keep {cfg.Num("design.n35.grapple35.arriveKeepMax")} m/s, pop {cfg.Num("design.n35.grapple35.arrivePop")}; the pull {cfg.Num("design.n31.grapplePull.speed")} m/s; zip arrive {cfg.movement.arcline.zip.arrive} m)");
        var cases = new (string name, Vec3 anchor, int node, bool tap, int press)[]
        {
            ("node-hold", new Vec3(0, 6, -15), 1, false, 1),
            ("wall-hold", new Vec3(6, 3, -30), 0, false, 1),
            ("wall-tap ", new Vec3(6, 3, -30), 0, true, 1),
            ("press x3 ", new Vec3(6, 3, -30), 0, false, 3), // the Player: a frame's press on every sim step of that frame (n35-r3: RopeAttach 0)
        };
        bool pass = true;
        foreach (var k in cases)
        {
            N35PullResult b = N35Pull(cfg, dt, false, k.anchor, k.node, k.tap, k.press);
            N35PullResult af = N35Pull(cfg, dt, true, k.anchor, k.node, k.tap, k.press);
            string Line(N35PullResult r) => r.arrived
                ? $"ARRIVED in {F35(r.time)} s, pull {F35(r.vMin)}..{F35(r.vMax)} m/s, off-line max {F35(r.maxOff)} m, after {F35(r.after)} m/s, 0.5 s later {F35(r.settle)} m from the point"
                : $"no arrival (end {r.end}{(r.refused ? ", REFUSED" : "")}), closest {F35(r.closest)} m, off-line max {F35(r.maxOff)} m, speed {F35(r.vMin)}..{F35(r.vMax)} m/s";
            Console.WriteLine($"  {k.name}  before: {Line(b)}");
            Console.WriteLine($"  {"",9}  after:  {Line(af)}");
            pass &= af.arrived && af.maxOff <= 0.25;
        }
        Console.WriteLine($"n35move pull: {(pass ? "PASS" : "FAIL")}");
        return pass ? 0 : 1;
    }
}
