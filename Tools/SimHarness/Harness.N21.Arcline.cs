// NIGHT21 U3-35.3 (builder M): `arcline [--tag t]`, THE ARCLINE in Survival, scripted on SURVIVAL (PortExport/maps/survival.json).
//   swing   10 seeded spots (Mulberry32): from each, aim at the nearest grapple node, press and hold the grapple key, push
//           forward, let go: it attaches in >= 9 of 10, and the peak speed after a swing is >= 20 m/s (median of the attached).
//   yank    a husk 12 m ahead, aimed at, one press: it moves >= 3 m toward the player (a control husk walks the same time
//           unyanked, printed); a second press inside the yank cooldown does not yank again (arclinePull stays 1) and the
//           hook goes to a node or a surface instead; after the cooldown a third press yanks again.
//   zip     a charger ahead, aimed at, hold: the rope bites it (ropeAttach zip true), the player reels in at the zip speed
//           and lets go on arrival (ropeRelease reason "zip").
//   buy     the terminal: in the buy list, locked until its zone opens, then hold BUY with 5500 points: owned, points spent.
//   off     the block off (--off21's state): a press fires nothing, ropeEnabled stays false, the terminal is not in the list.
//   Prints one line per part and the contract's line; exit 0 only when all hold. The fingerprint (n21 block off) is the
//   `fingerprint` command's: it stays EQUAL (the run takes its own blocks off, FingerprintBaseline.N21OnPath).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void ArclineCommand(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        bool ok = true;

        // --- swing: 10 seeded spots --------------------------------------------------------------------------------
        var rng = new Mulberry32(2135);
        SurvivalSim ledgeProbe = ArclineSim(cfg, map, true, 2100);
        int attached = 0;
        var peaks = new List<double>();
        for (int i = 0; i < 10; i++)
        {
            // a seeded spot on the open floor (the ray down from 26 m meets the floor, not a box), 12-26 m from the centre
            Vec3 at = default;
            for (int tries = 0; tries < 2000; tries++)
            {
                double ang = rng.Next() * Math.PI * 2, rad = 12 + rng.Next() * 14;
                double sx = Math.Cos(ang) * rad, sz = Math.Sin(ang) * rad;
                if (!ledgeProbe.world.RayIntersect(new Vec3(sx, 26, sz), new Vec3(0, -1, 0), out RayHit hit) || 26 - hit.distance > 0.2) continue;
                if (ArclineSwingNode(map, ledgeProbe.world, new Vec3(sx, 1.6, sz)) == null) continue;
                at = new Vec3(sx, 0, sz);
                break;
            }
            (bool att, double peak, string note) = ArclineSwing(cfg, map, (uint)(2100 + i), at);
            if (att) attached++;
            if (att) peaks.Add(peak);
            Console.WriteLine($"  swing {i}: ({at.x:0.0}, {at.z:0.0}) {(att ? $"attached, peak {peak:0.0} m/s" : "NO attach")} {note}");
        }
        peaks.Sort();
        double median = peaks.Count == 0 ? 0 : peaks[peaks.Count / 2];
        double minPeak = peaks.Count == 0 ? 0 : peaks[0];
        bool swingOk = attached >= 9 && median >= 20;
        ok &= swingOk;

        // --- yank, its cooldown, the zip --------------------------------------------------------------------------------
        ArclineYankResult y = ArclineYank(cfg, map);
        Console.WriteLine($"  yank: the husk moved {y.moved:0.0} m toward the player in {y.seconds:0.0} s (a control husk, unyanked: {y.control:0.0} m); arclinePull events {y.pulls1} then {y.pullsInCooldown} (second press inside the cooldown) then {y.pullsAfter} (after {y.cooldown:0} s); the second press fired the hook at {y.secondBit}");
        bool yankOk = y.moved >= 3 && y.moved - y.control >= 2 && y.pulls1 == 1 && y.pullsInCooldown == 1 && y.pullsAfter == 2;
        ok &= yankOk;
        ArclineZipResult z = ArclineZip(cfg, map);
        Console.WriteLine($"  zip: ropeAttach zip {z.attachZip}, closed {z.closed:0.0} m of {z.start:0.0}, peak speed {z.peak:0.0} m/s (zip {z.zipSpeed:0} m/s), let go: {z.release ?? "never"}");
        bool zipOk = z.attachZip && z.peak >= z.zipSpeed * 0.9 && z.closed >= 5 && z.release == "zip";
        ok &= zipOk;

        // --- buy, off ---------------------------------------------------------------------------------------------------
        ArclineBuyResult b = ArclineBuy(cfg, map);
        Console.WriteLine($"  buy: in the list {b.inList}, available before the zone opens {b.availBefore}, after {b.availAfter}; bought with {b.bank:0} points: owned {b.owned}, spent {b.spent:0} (price {b.price:0}), arclineOwned events {b.ownedEvents}");
        bool buyOk = b.inList && !b.availBefore && b.availAfter && b.owned && Math.Abs(b.spent - b.price) < 0.5 && b.ownedEvents == 1;
        ok &= buyOk;
        (bool offQuiet, bool offList) = ArclineOff(cfg, map);
        Console.WriteLine($"  off: a press with the block off fires nothing {offQuiet}; terminal not in the buy list {offList}");
        ok &= offQuiet && offList;

        Console.WriteLine($"arcline {(ok ? "PASS" : "FAIL")}: attaches {attached}/10 (want >= 9), peak speed after a swing {median:0.0} m/s median (min {minPeak:0.0}; want >= 20), a yanked husk moved {y.moved:0.0} m toward the player (want >= 3); yank cooldown {(yankOk ? "holds" : "BROKEN")}, zip {(zipOk ? "ok" : "BROKEN")}, buy {(buyOk ? "ok" : "BROKEN")}, off {(offQuiet && offList ? "ok" : "BROKEN")}");
        code = ok ? 0 : 1;
    }

    // --- scaffolding ----------------------------------------------------------------------------------------------------

    static SurvivalSim ArclineSim(GameConfig cfg, SurvivalMap map, bool on, uint seed)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n21Override = on ? new N21Blocks { arcline = true } : N21Blocks.Off;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(seed, null, null);
        return sim;
    }

    static void ArclineTick(SurvivalSim sim, double yaw, double pitch, bool pressed, bool held, double moveZ = 0, bool interact = false, bool jump = false, bool dash = false)
    {
        var inp = (ScriptedInput)sim.input;
        inp.SetLook(yaw, pitch);
        inp.Set(new InputFrame { moveZ = moveZ, grapple = new Button { held = held, pressed = pressed }, jump = new Button { held = jump, pressed = jump }, dash = new Button { held = dash, pressed = dash } },
            new CombatInput { weapon = -1, interact = new Button { held = interact, pressed = false } });
        sim.hitstopLeft = 0;
        sim.Step();
    }

    /// <summary>The yaw / pitch that look from `from` at `to` (yaw 0 looks toward -z, + pitch up).</summary>
    static (double yaw, double pitch) ArclineLook(Vec3 from, Vec3 to)
    {
        Vec3 d = to - from;
        return (Math.Atan2(-d.x, -d.z), Math.Asin(d.y / d.Length));
    }

    /// <summary>The node a player picks to build speed: the highest one within 45 m that is not straight overhead (8 m off), with a clear line from the eye and 8 m of open run toward it.</summary>
    static Vec3? ArclineSwingNode(SurvivalMap map, GameWorld world, Vec3 eye)
    {
        Vec3? node = null;
        double best = double.MaxValue, bestY = double.MinValue;
        foreach (Vec3 n in map.grappleNodes)
        {
            double d = Vec3.DistanceTo(n, eye);
            double flat = Math.Sqrt((n.x - eye.x) * (n.x - eye.x) + (n.z - eye.z) * (n.z - eye.z));
            if (d > 45 || flat < 8) continue;
            if (world.RayIntersect(eye, (n - eye) / d, out RayHit los) && los.distance < d - 0.4) continue;
            var run = new Vec3((n.x - eye.x) / flat, 0, (n.z - eye.z) / flat);
            if (world.RayIntersect(new Vec3(eye.x, eye.y - 1.2, eye.z), run, out RayHit rh) && rh.distance < 8) continue;
            if (n.y > bestY || (n.y == bestY && d < best)) { best = d; bestY = n.y; node = n; }
        }
        return node;
    }

    static (bool attached, double peak, string note) ArclineSwing(GameConfig cfg, SurvivalMap map, uint seed, Vec3 at)
    {
        SurvivalSim sim = ArclineSim(cfg, map, true, seed);
        sim.arcline21.Own(true);
        sim.Respawn(at, 0);
        int attaches = 0;
        string release = null;
        sim.mover.Events += e =>
        {
            if (e.type == MoverEventType.RopeAttach) attaches++;
            if (e.type == MoverEventType.RopeRelease) release ??= e.reason;
        };
        for (int s = 0; s < 40; s++) ArclineTick(sim, 0, 0, false, false); // land
        Vec3 eye = sim.Eye;
        Vec3? picked = ArclineSwingNode(map, sim.world, eye);
        Vec3 node = picked ?? map.grappleNodes[0];
        double best = Vec3.DistanceTo(node, eye);
        (double yaw, double pitch) = ArclineLook(eye, node);
        double peak = 0;
        // the player's move: dash toward the node, hook 15 steps (0.125 s) into the dash, pump forward, let go after 1.25 s
        const int pre = 40, hl = 150, dashBefore = 15;
        for (int s = 0; s < 300 + pre; s++)
        {
            bool hold = s >= pre && s < pre + hl;
            ArclineTick(sim, yaw, pitch, s == pre, hold, 1, false, false, s == pre - dashBefore);
            if (attaches > 0) peak = Math.Max(peak, sim.mover.velocity.Length);
            if (Environment.GetEnvironmentVariable("ARC_TRACE") != null && s % 12 == 0) Console.WriteLine($"    s{s} state {sim.mover.state} rope {sim.mover.rope.phase} L {sim.mover.rope.L:0.0} pos {sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0} v {sim.mover.velocity.Length:0.0}");
        }
        return (attaches > 0, peak, $"(node {node.x:0},{node.y:0},{node.z:0} at {best:0.0} m, released: {release ?? "-"})");
    }

    /// <summary>A direction from `p` with nothing solid for `dist` + 3 m at knee and eye height (the map's centre cover stands near the spawn): (yaw, unit x, unit z).</summary>
    static (double yaw, double dx, double dz) ArclineOpenDir(SurvivalSim sim, Vec3 p, double dist)
    {
        for (int k = 0; k < 64; k++)
        {
            double a = k * Math.PI / 32;
            double dx = Math.Sin(a), dz = -Math.Cos(a);
            bool clear = true;
            var eyeAt = new Vec3(p.x, p.y + 1.6, p.z);
            foreach (double h in new[] { 0.3, 0.8, 1.2, 2.0, 3.0 }) // the lines from the eye to the enemy's body, low to high
            {
                var t = new Vec3(p.x + dx * dist, p.y + h, p.z + dz * dist);
                double len = Vec3.DistanceTo(t, eyeAt);
                if (sim.world.RayIntersect(eyeAt, (t - eyeAt) / len, out RayHit hit) && hit.distance < len + 1) clear = false;
            }
            if (clear) return (Math.Atan2(-dx, -dz), dx, dz);
        }
        Console.WriteLine($"    (no open line of {dist} m from ({p.x:0},{p.z:0}))");
        return (0, 0, -1);
    }

    struct ArclineYankResult
    {
        public double moved, control, seconds, cooldown;
        public int pulls1, pullsInCooldown, pullsAfter;
        public string secondBit;
    }

    static ArclineYankResult ArclineYank(GameConfig cfg, SurvivalMap map)
    {
        var r = new ArclineYankResult();
        Vec3 player = map.playerSpawn;
        double ahead = 12;
        SurvivalSim probe = ArclineSim(cfg, map, true, 2201);
        (double lookYaw, double ux, double uz) = ArclineOpenDir(probe, player, ahead);
        Vec3 spawnAt = new Vec3(player.x + ux * ahead, 0, player.z + uz * ahead);
        // the control: the same husk, no press
        SurvivalSim c = ArclineSim(cfg, map, true, 2201);
        c.arcline21.Own(true);
        Husk ch = c.husks.Spawn("husk", spawnAt, player);
        Vec3 cStart = ch.position;
        for (int s = 0; s < 60; s++) ArclineTick(c, 0, 0, false, false);
        r.control = Vec3.DistanceTo(cStart, c.mover.position) - Vec3.DistanceTo(ch.position, c.mover.position);

        SurvivalSim sim = ArclineSim(cfg, map, true, 2201);
        sim.arcline21.Own(true);
        r.cooldown = sim.arcline21.rules.yankCooldown;
        int pulls = 0;
        sim.events.On("arclinePull", _ => pulls++);
        Husk h = sim.husks.Spawn("husk", spawnAt, player);
        for (int s = 0; s < 4; s++) ArclineTick(sim, 0, 0, false, false);
        Vec3 me = sim.mover.position;
        double d0 = Vec3.DistanceTo(h.position, me);
        (double yaw, double pitch) = ArclineLook(sim.Eye, h.AimPoint);
        ArclineTick(sim, yaw, pitch, true, true);
        r.pulls1 = pulls;
        for (int s = 0; s < 59; s++) ArclineTick(sim, yaw, pitch, false, false);
        r.seconds = 60 * sim.step;
        r.moved = d0 - Vec3.DistanceTo(h.position, sim.mover.position);
        // a second press inside the cooldown, a husk in front again
        Husk h2 = sim.husks.Spawn("husk", spawnAt, player);
        int fires = 0;
        string bit = "nothing";
        sim.mover.Events += e =>
        {
            if (e.type == MoverEventType.RopeFire)
            {
                fires++;
                bit = e.node ? "a node" : "a surface";
            }
        };
        (yaw, pitch) = ArclineLook(sim.Eye, h2.AimPoint);
        ArclineTick(sim, yaw, pitch, true, true);
        r.pullsInCooldown = pulls;
        r.secondBit = fires > 0 ? bit : "nothing";
        for (int s = 0; s < 30; s++) ArclineTick(sim, yaw, pitch, false, false); // let go below
        // after the cooldown: wait it out with the husks dead, a fresh one in front
        sim.husks.KillAll();
        sim.mover.Reset(player.x, 0, player.z);
        int wait = MovementMath.StepsFor(r.cooldown, sim.step) + 10;
        for (int s = 0; s < wait; s++) { sim.husks.KillAll(); ArclineTick(sim, 0, 0, false, false); }
        Husk h3 = sim.husks.Spawn("husk", spawnAt, player);
        for (int s = 0; s < 4; s++) ArclineTick(sim, 0, 0, false, false);
        if (h3 != null)
        {
            (yaw, pitch) = ArclineLook(sim.Eye, h3.AimPoint);
            ArclineTick(sim, yaw, pitch, true, true);
        }
        r.pullsAfter = pulls;
        return r;
    }

    struct ArclineZipResult
    {
        public bool attachZip;
        public double start, closed, peak, zipSpeed;
        public string release;
    }

    static ArclineZipResult ArclineZip(GameConfig cfg, SurvivalMap map)
    {
        var r = new ArclineZipResult();
        SurvivalSim sim = ArclineSim(cfg, map, true, 2301);
        sim.arcline21.Own(true);
        r.zipSpeed = sim.arcline21.rules.zipReelSpeed;
        Vec3 player = map.playerSpawn;
        (double zy, double zx, double zz) = ArclineOpenDir(sim, player, 14);
        Husk g = sim.husks.Spawn("charger", new Vec3(player.x + zx * 14, 0, player.z + zz * 14), player);
        if (g == null) return r;
        sim.mover.Events += e =>
        {
            if (e.type == MoverEventType.RopeAttach) r.attachZip = e.zip;
            if (e.type == MoverEventType.RopeRelease && r.release == null) r.release = e.reason;
        };
        for (int s = 0; s < 4; s++) ArclineTick(sim, 0, 0, false, false);
        r.start = Vec3.DistanceTo(g.position, sim.mover.position);
        for (int s = 0; s < 200; s++)
        {
            (double yaw, double pitch) = ArclineLook(sim.Eye, g.AimPoint);
            ArclineTick(sim, yaw, pitch, s == 0, r.release == null);
            r.peak = Math.Max(r.peak, sim.mover.velocity.Length);
        }
        r.closed = r.start - Vec3.DistanceTo(g.position, sim.mover.position);
        return r;
    }

    struct ArclineBuyResult
    {
        public bool inList, availBefore, availAfter, owned;
        public double bank, spent, price;
        public int ownedEvents;
    }

    static ArclineBuyResult ArclineBuy(GameConfig cfg, SurvivalMap map)
    {
        var r = new ArclineBuyResult();
        SurvivalSim sim = ArclineSim(cfg, map, true, 2401);
        Buyables.Item item = sim.buyables.list.Find(i => i.id == "gadget:arcline");
        r.inList = item != null;
        if (item == null) return r;
        r.price = item.price();
        r.availBefore = item.available == null || item.available();
        sim.zones.Open("islands", true);
        r.availAfter = item.available == null || item.available();
        sim.economy.total = r.bank = r.price + 100;
        int ownedEvents = 0;
        sim.events.On("arclineOwned", _ => ownedEvents++);
        PerkSpot spot = sim.Arcline21Spot().Value;
        // stand 1.2 m in front of the terminal (it faces +x here: yaw pi / 2 faces the arena), looking at it
        double fx = Math.Sin(spot.yaw), fz = Math.Cos(spot.yaw);
        var stand = new Vec3(spot.pos.x + fx * 1.2, spot.pos.y, spot.pos.z + fz * 1.2);
        sim.Respawn(stand, 0);
        (double yaw, double pitch) = ArclineLook(sim.Eye, new Vec3(spot.pos.x, sim.Eye.y, spot.pos.z));
        double spent0 = sim.economy.spent;
        for (int s = 0; s < 200 && !sim.arcline21.Owned; s++) ArclineTick(sim, yaw, 0, false, false, 0, true);
        r.owned = sim.arcline21.Owned && sim.mover.ropeEnabled;
        r.spent = sim.economy.spent - spent0;
        r.ownedEvents = ownedEvents;
        return r;
    }

    static (bool quiet, bool noTerminal) ArclineOff(GameConfig cfg, SurvivalMap map)
    {
        SurvivalSim sim = ArclineSim(cfg, map, false, 2501);
        bool noTerminal = sim.buyables.list.Find(i => i.id == "gadget:arcline") == null && sim.arcline21 == null;
        int fires = 0;
        sim.mover.Events += e => { if (e.type == MoverEventType.RopeFire) fires++; };
        Vec3 player = map.playerSpawn;
        (double oy, double ox, double oz) = ArclineOpenDir(sim, player, 12);
        Husk h = sim.husks.Spawn("husk", new Vec3(player.x + ox * 12, 0, player.z + oz * 12), player);
        for (int s = 0; s < 4; s++) ArclineTick(sim, 0, 0, false, false);
        (double yaw, double pitch) = ArclineLook(sim.Eye, h.AimPoint);
        ArclineTick(sim, yaw, pitch, true, true);
        for (int s = 0; s < 30; s++) ArclineTick(sim, yaw, pitch, false, true);
        return (fires == 0 && !sim.mover.ropeEnabled && sim.mover.rope.phase == RopePhase.None, noTerminal);
    }
}
