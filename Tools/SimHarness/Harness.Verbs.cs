// NIGHT15 U3-28.6 (the lead): "each zone's verb identity in geometry", measured. `verbs <map.json>` drives the web's
// Mover (Core/Mover.cs, untouched) on a map's own collider (SurvivalMap.BuildOctree: boxes, barriers, wedges) at 120 Hz,
// pads checked after every step (arena.js checkPads, as the trajectory recorder does), no enemies, and reports what
// DRYDOCK's signature moves really do: the slide down lane A, the slide-jump off the kicker onto LAUNCH RAIL, a wall-ride
// in a rib alley, every pad's landing, and the dash-jump from the slipway deck's lip onto the hull deck. Numbers only:
// a move that falls short is a finding for MAP-DESIGN.md "As built", not a pass / fail gate.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;

public static partial class Harness
{
    sealed class VerbRun
    {
        public double peakSpeed, maxY, wallRideTime, slideTime, airTime;
        public Vec3 end, firstLanding;
        public bool landed;
        public int steps;
    }

    static string F(double v) => v.ToString("0.00", CultureInfo.InvariantCulture);

    static string V(Vec3 v) => $"({F(v.x)}, {F(v.y)}, {F(v.z)})";

    /// <summary>Run `steps` 120 Hz steps from `at` (feet) with input(i, mover); 30 settle steps first (no input).</summary>
    static VerbRun VerbDrive(MovementConfig mc, ICollisionWorld world, SurvivalMap map, Vec3 at, int steps, Func<int, Mover, InputFrame> input)
    {
        var m = new Mover(mc, world);
        m.Reset(at.x, at.y, at.z);
        double dt = 1 / mc.loop.simHz;
        for (int i = 0; i < 30; i++)
        {
            m.Step(new InputFrame { yaw = input(0, m).yaw }, dt);
            map.CheckPads(m, mc, null);
        }
        var r = new VerbRun();
        bool wasAir = false;
        double airStart = 0;
        for (int i = 0; i < steps; i++)
        {
            m.Step(input(i, m), dt);
            map.CheckPads(m, mc, null);
            r.peakSpeed = Math.Max(r.peakSpeed, m.Speed);
            r.maxY = Math.Max(r.maxY, m.position.y);
            if (m.state == MoveState.WallRide) r.wallRideTime += dt;
            if (m.state == MoveState.Slide) r.slideTime += dt;
            if (!m.grounded) r.airTime += dt;
            if (!m.grounded && !wasAir) airStart = i * dt;
            if (m.grounded && wasAir && !r.landed && i * dt - airStart > 0.25)
            {
                r.landed = true;
                r.firstLanding = m.position;
            }
            wasAir = !m.grounded;
            r.steps = i + 1;
        }
        r.end = m.position;
        return r;
    }

    static int Verbs(string path)
    {
        GameConfig cfg = Config();
        MovementConfig mc = cfg.movement;
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
        ThreeOctree world = map.BuildOctree();
        var lines = new List<string>();
        void Out(string s)
        {
            Console.WriteLine("verbs " + s);
            lines.Add(s);
        }
        Out($"{map.name}: Mover on the map's octree, 120 Hz, pads checked each step, no enemies");
        Button Press(bool on, bool edge) => new Button { held = on, pressed = edge };

        // 1. THE SLIPWAY: lane A from the deck's top, run 0.25 s then hold slide, facing south (yaw pi), 3 s.
        {
            VerbRun r = VerbDrive(mc, world, map, new Vec3(30, 10, -25), 360, (i, m) => new InputFrame
            {
                yaw = Math.PI, moveZ = 1, slide = Press(i >= 30, i == 30),
            });
            Out($"slipway lane A slide: peak {F(r.peakSpeed)} m/s, slide {F(r.slideTime)} s, end {V(r.end)} (the ramp's foot is z 18)");
        }
        // 2. LAUNCH RAIL: the same slide, a jump pressed when the feet pass jumpZ (the kicker's lip is z 25, the rail
        //    platform x 26..34, z 30..34, top 3.0): which take-off points land on it.
        {
            var hits = new List<string>();
            string best = null;
            foreach (double jumpZ in new[] { 22.5, 23.0, 23.5, 24.0, 24.4, 24.8 })
            {
                int jumped = -1;
                VerbRun r = VerbDrive(mc, world, map, new Vec3(30, 10, -25), 600, (i, m) =>
                {
                    bool now = jumped < 0 && m.position.z >= jumpZ;
                    if (now) jumped = i;
                    return new InputFrame { yaw = Math.PI, moveZ = 1, slide = Press(i >= 30 && jumped < 0, i == 30), jump = Press(jumped >= 0 && i - jumped < 30, now) };
                });
                bool onRail = r.landed && r.firstLanding.y > 2.5 && r.firstLanding.x >= 26 && r.firstLanding.x <= 34 && r.firstLanding.z >= 30 && r.firstLanding.z <= 34;
                string s = $"jump at z {F(jumpZ)}: landed {V(r.firstLanding)} max y {F(r.maxY)}{(onRail ? " ON THE RAIL" : "")}";
                if (onRail) hits.Add(F(jumpZ));
                best ??= s;
                Out("launch rail " + s);
            }
            Out($"launch rail: {hits.Count} of 6 take-off points land on the 3.0 m rail ({string.Join(", ", hits)})");
        }
        // 3. THE RIBS: from the cross alley, run north angled 0.25 rad toward the west slab (x -47), jump at 0.6 s,
        //    hold forward (wall_ride.json's input on a rib).
        {
            VerbRun r = VerbDrive(mc, world, map, new Vec3(-45.8, 0, -4), 360, (i, m) => new InputFrame // alongside the slab, 1.2 m from its face (x -47), as wall_ride.json starts 1.2 m from its wall (from the cross alley at z 1 the run clipped the slab's end at z -3: 8 -> 4.6 m/s, no ride)
            {
                yaw = 0.25, moveZ = 1, jump = Press(i >= 72 && i < 120, i == 72),
            });
            Out($"ribs wall-ride: wall-ride {F(r.wallRideTime)} s, max y {F(r.maxY)}, end {V(r.end)}, peak {F(r.peakSpeed)} m/s");
        }
        // 4. Every pad: stand on it; where the first landing is against the pad's target.
        for (int p = 0; p < map.pads.Count; p++)
        {
            PadSpec pad = map.pads[p];
            VerbRun r = VerbDrive(mc, world, map, pad.pos, 360, (i, m) => new InputFrame { yaw = 0 });
            string target = pad.aimed ? $"target {V(pad.target)}, off by {F(Math.Sqrt(Sq(r.firstLanding.x - pad.target.x) + Sq(r.firstLanding.z - pad.target.z)))} m across, {F(r.firstLanding.y - pad.target.y)} m in height" : "plain";
            Out($"pad {p} at {V(pad.pos)}: landed {(r.landed ? V(r.firstLanding) : "never")}, max y {F(r.maxY)}; {target}");
        }
        // 5. The slipway deck's lip to the hull deck (the shortcut: 17-18 m to the hull deck's east edge at x 7, top 11):
        //    run west-north-west, dash when the feet pass dashX, jump 0.1 s later.
        {
            double yaw = Math.Atan2(23 - 7, -(-26.5 + 18)); // toward (7, -18) from (30, -26.5): atan2(-dx, -dz)
            yaw = Math.Atan2(-(7 - 30.0), -(-18 + 26.5));
            var hits = new List<string>();
            foreach (double dashX in new[] { 27.0, 26.0, 25.0, 24.4 })
            {
                int dashed = -1;
                VerbRun r = VerbDrive(mc, world, map, new Vec3(30, 10, -26.5), 300, (i, m) =>
                {
                    bool now = dashed < 0 && m.position.x <= dashX;
                    if (now) dashed = i;
                    bool jumpNow = dashed >= 0 && i - dashed == 12;
                    return new InputFrame { yaw = yaw, moveZ = 1, dash = Press(now, now), jump = Press(dashed >= 0 && i - dashed >= 12 && i - dashed < 40, jumpNow) };
                });
                bool onHull = r.landed && r.firstLanding.y > 10.5 && Math.Abs(r.firstLanding.x) <= 7;
                if (onHull) hits.Add(F(dashX));
                Out($"deck lip dash-jump, dash at x {F(dashX)}: landed {(r.landed ? V(r.firstLanding) : "never")}{(onHull ? " ON THE HULL DECK" : "")}");
            }
            Out($"deck lip dash-jump: {hits.Count} of 4 timings land on the hull deck ({string.Join(", ", hits)})");
        }
        return 0;
    }

    static double Sq(double v) => v * v;
}
