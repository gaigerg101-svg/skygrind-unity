// NIGHT37 builder K: `verbs37probe <level-id> [--off37]`: the verbs37 reel's RED rows (n37-r5) re-performed headless on a campaign level, the same
// spots and inputs as FeelReel.N35.Verbs.cs, so a RED reads as a real loss or the check's setup. The control: the old level id with --off37 (NIGHT36's
// game on NIGHT36's level). Prints the start (owned, the hand, pads, the wall the reel picks) and rows 13, 15, 18, 19, 24 with what each saw.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{

    sealed class Probe37
    {
        public SurvivalSim sim;
        public ScriptedInput si;
        public int Ev(string n) => sim.events.Emitted.TryGetValue(n, out int k) ? k : 0;
        public readonly Dictionary<MoverEventType, int> mv = new Dictionary<MoverEventType, int>();
        public int M(MoverEventType t) => mv.TryGetValue(t, out int k) ? k : 0;

        public void Run(double seconds, Action<double> each)
        {
            int n = MovementMath.StepsFor(seconds, sim.step);
            for (int i = 0; i < n; i++)
            {
                each?.Invoke(i * sim.step);
                sim.mover.invulnerable = true;
                sim.Step();
            }
            si.Set(new InputFrame(), new CombatInput { weapon = -1 });
        }

        public void Reset(Vec3 at, double yaw, double pitch = 0)
        {
            si.Set(new InputFrame(), new CombatInput { weapon = -1 });
            sim.Respawn(at, yaw * 180 / Math.PI);
            si.SetLook(yaw, pitch);
            Run(0.35, null);
        }
    }

    static Vec3 PFwd(double yaw) => new Vec3(-Math.Sin(yaw), 0, -Math.Cos(yaw));
    static double PYaw(Vec3 d) => Math.Atan2(-d.x, -d.z);

    static int Verbs37Probe(string[] args)
    {
        string id = args.Length > 1 ? args[1] : "n37-iceline";
        bool off = Array.IndexOf(args, "--off37") >= 0;
        PFloorRule = Array.IndexOf(args, "--floors") >= 0;
        N37Blocks.ForceOff = off;
        GameConfig cfg = Config();
        string path = Path.Combine("Design", "levels", id + ".json");
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var si = new ScriptedInput();
        sim.UseInput(si);
        sim.StartRun(1, null, null);
        var p = new Probe37 { sim = sim, si = si };
        sim.mover.Events += e => p.mv[e.type] = p.M(e.type) + 1;
        sim.husks.KillAll();
        sim.husks.frozen = true;
        Mover m = sim.mover;
        Console.WriteLine($"verbs37probe {id} ({(off ? "--off37" : "n37 " + sim.n37)}): level {sim.campaignLevel?.id}, owned [{string.Join(", ", sim.loadout.owned)}], hand {sim.loadout.Weapon}, pads {map.pads.Count}, stamina bar rule {m.stamina.rules.Any}, walk-in {sim.Boss36WalkIn}");

        // ---- 24 punch (from the start's hand, as the reel: row 24 runs before any Hand35) ----
        {
            var (open, oy) = POpen(sim);
            p.Reset(open, oy);
            int pu0 = p.Ev("punch");
            string hand = sim.loadout.Weapon;
            p.Run(0.6, t => si.Set(new InputFrame(), new CombatInput { weapon = -1, punch = new Button { pressed = t < 0.02, held = t < 0.1 } }));
            Console.WriteLine($"  24 punch with {hand} in hand: punch events {p.Ev("punch") - pu0}");
            // the same with a gun in hand
            int slot = sim.loadout.owned.IndexOf("revolver");
            if (slot >= 0 && hand != "revolver")
            {
                p.Run(0.6, t => si.Set(new InputFrame(), new CombatInput { weapon = sim.loadout.Weapon == "revolver" ? -1 : slot }));
                int pu1 = p.Ev("punch");
                p.Run(0.6, t => si.Set(new InputFrame(), new CombatInput { weapon = -1, punch = new Button { pressed = t < 0.02, held = t < 0.1 } }));
                Console.WriteLine($"  24 punch with {sim.loadout.Weapon} in hand: punch events {p.Ev("punch") - pu1}");
            }
        }

        // ---- 13 slide bounce (the reel's wall) ----
        bool wallOk = PWall(sim, out Vec3 wStart, out Vec3 wAlong, out Vec3 wNormal, out double wLen, out string wRoom);
        Console.WriteLine($"  the reel's wall: {(wallOk ? $"{wLen} m in room {wRoom}, start ({wStart.x:0.0}, {wStart.y:0.0}, {wStart.z:0.0}), normal ({wNormal.x:0.00}, {wNormal.z:0.00})" : "none")}");
        if (wallOk)
        {
            Vec3 inDir = new Vec3((wAlong.x - wNormal.x) / Math.Sqrt(2), 0, (wAlong.z - wNormal.z) / Math.Sqrt(2));
            var back = new Vec3(wStart.x + wAlong.x * 2 + wNormal.x * 3.3, wStart.y, wStart.z + wAlong.z * 2 + wNormal.z * 3.3);
            bool floor = sim.world.RayIntersect(new Vec3(back.x, back.y + 1, back.z), new Vec3(0, -1, 0), out RayHit fh);
            p.Reset(back, PYaw(inDir));
            Vec3 settled = m.position;
            int wj0 = p.M(MoverEventType.WallJump);
            bool pressed = false, slid = false;
            double before = 0, after = 0, pressT = -1, minY = 1e9;
            p.Run(1.6, t =>
            {
                if (t < 0.05) m.velocity = new Vec3(inDir.x * 9.9, m.velocity.y, inDir.z * 9.9);
                if (m.state == MoveState.Slide) slid = true;
                minY = Math.Min(minY, m.position.y);
                bool press = !pressed && t > 0.05 && m.touchingWall;
                if (press) { pressed = true; pressT = t; before = m.Speed; }
                if (pressed && t - pressT > 0 && t - pressT < 0.15) after = Math.Max(after, m.Speed);
                si.SetLook(PYaw(inDir), 0);
                si.Set(new InputFrame { moveZ = 1, slide = new Button { pressed = t < 0.02, held = !pressed }, jump = new Button { pressed = press, held = press } }, new CombatInput { weapon = -1 });
            });
            Console.WriteLine($"  13 slide bounce: spot y {back.y:0.00}, floor under it {(floor ? $"{fh.distance - 1:0.00} m below" : "NONE")}, after the reset at ({settled.x:0.0}, {settled.y:0.00}, {settled.z:0.0}) grounded {m.grounded}; slid {slid}, wall contact {pressed}, WallJump {p.M(MoverEventType.WallJump) - wj0}, speed {before:0.00} -> {after:0.00}, lowest y {minY:0.0}");
        }

        // ---- 15 jump pad ----
        if (map.pads.Count == 0) Console.WriteLine("  15 jump pad: none on this level");
        else
        {
            for (int i = 0; i < map.pads.Count; i++)
            {
                Vec3 pad = map.pads[i].pos;
                bool fl = sim.world.RayIntersect(new Vec3(pad.x, pad.y + 20, pad.z), new Vec3(0, -1, 0), out RayHit ph);
                bool was = sim.zones.padEnabled[i];
                if (Array.IndexOf(args, "--openpads") >= 0)
                {
                    PadSpec ps = map.pads[i];
                    Vec3 tg = ps.aimed ? ps.target : ps.pos;
                    foreach (Vec3 q in new[] { ps.pos, tg })
                    {
                        int z = ZoneMath.ZoneIndexAt(map.zoneParts, q.x, q.y + 1, q.z);
                        if (z >= 0) sim.zones.Open(map.zones[z].id, free: true);
                    }
                }
                Console.WriteLine($"  15 pad {i}: awake at the start {was}, now {sim.zones.padEnabled[i]}");
                int jp0 = p.M(MoverEventType.JumpPad);
                p.Reset(new Vec3(pad.x, pad.y + 0.3, pad.z), 0);
                Vec3 at = m.position;
                p.Run(1.0, t => si.Set(new InputFrame { moveZ = t < 0.3 ? 0.3 : 0 }, new CombatInput { weapon = -1 }));
                Console.WriteLine($"  15 pad {i} at ({pad.x}, {pad.y}, {pad.z}): the floor under it at y {(fl ? (pad.y + 20 - ph.distance).ToString("0.00") : "none")}, feet after the reset ({at.x:0.0}, {at.y:0.00}, {at.z:0.0}): JumpPad events {p.M(MoverEventType.JumpPad) - jp0}");
            }
        }

        // ---- 19 hold-swing, 18 bare wall (the grapple: owned on level 4+) ----
        bool owned = sim.arcline21 != null && sim.arcline21.Owned;
        if (!owned) Console.WriteLine("  17-19: the grapple is not owned here");
        else
        {
            if (PNode(sim, out Vec3 node, out Vec3 from, out double ny, out double np))
            {
                p.Reset(from, ny, np);
                p.Run(1.0, null);
                Vec3 a = m.position + new Vec3(0, 1.0, 0);
                double dev = 0, devAttached = 0, closest = 1e9;
                int ra1 = p.M(MoverEventType.RopeAttach), rr1 = p.M(MoverEventType.RopeRelease);
                double releaseT = -1, closestT = -1;
                var trace = new List<string>();
                p.Run(1.6, t =>
                {
                    si.Set(new InputFrame { grapple = new Button { pressed = t < 0.02, held = true } }, new CombatInput { weapon = -1 });
                    Vec3 q = m.position + new Vec3(0, 1.0, 0), ab = node - a, ap = q - a;
                    double l2 = Vec3.Dot(ab, ab), u = l2 > 1e-9 ? Math.Max(0, Math.Min(1, Vec3.Dot(ap, ab) / l2)) : 0;
                    double d = (q - (a + ab * u)).Length;
                    double dn = (q - node).Length;
                    if (dn < closest) { closest = dn; closestT = t; }
                    if (p.M(MoverEventType.RopeAttach) > ra1) dev = Math.Max(dev, d);
                    if (p.M(MoverEventType.RopeAttach) > ra1 && p.M(MoverEventType.RopeRelease) == rr1) devAttached = Math.Max(devAttached, d);
                    if (releaseT < 0 && p.M(MoverEventType.RopeRelease) > rr1) releaseT = t;
                    if (((int)Math.Round(t / sim.step)) % 24 == 0) trace.Add($"{t:0.0}s d{d:0.0}/n{dn:0.0} {m.state}");
                });
                Console.WriteLine($"  19 hold-swing: node {(node - a).Length:0.0} m away; RopeAttach {p.M(MoverEventType.RopeAttach) - ra1}, RopeRelease at {releaseT:0.00} s, closest to the node {closest:0.00} m at {closestT:0.00} s; off the line: {dev:0.00} m over the whole 1.6 s (the reel's), {devAttached:0.00} m while the rope was attached; [{string.Join(", ", trace)}]");
            }
            else Console.WriteLine("  17/19: no node found");
            if (wallOk)
            {
                var face = new Vec3(-wNormal.x, 0, -wNormal.z);
                var spot = new Vec3(wStart.x + wAlong.x * 4 + wNormal.x * 9, wStart.y, wStart.z + wAlong.z * 4 + wNormal.z * 9);
                bool fl = sim.world.RayIntersect(new Vec3(spot.x, spot.y + 1, spot.z), new Vec3(0, -1, 0), out RayHit sh);
                p.Reset(spot, PYaw(face), 0.12);
                Vec3 at = m.position;
                p.Run(0.4, null);
                Vec3 eye = sim.Eye, aim = PFwd(PYaw(face));
                var dir3 = new Vec3(aim.x * Math.Cos(0.12), Math.Sin(0.12), aim.z * Math.Cos(0.12));
                bool hitW = sim.world.RayIntersect(eye, dir3, out RayHit wh);
                Vec3 hitP = hitW ? eye + dir3 * wh.distance : eye;
                int ra2 = p.M(MoverEventType.RopeAttach), rf2 = p.M(MoverEventType.RopeFire);
                double close = 1e9;
                p.Run(2.0, t =>
                {
                    si.Set(new InputFrame { grapple = new Button { pressed = t < 0.02, held = t < 0.1 } }, new CombatInput { weapon = -1 });
                    close = Math.Min(close, (m.position + new Vec3(0, 1.0, 0) - hitP).Length);
                });
                Console.WriteLine($"  18 bare wall: spot floor {(fl ? $"{sh.distance - 1:0.00} m below" : "NONE")}, feet after the reset ({at.x:0.0}, {at.y:0.0}, {at.z:0.0}) grounded {m.grounded}; the aim ray hits {(hitW ? $"at {wh.distance:0.0} m, normal y {wh.normal.y:0.00}" : "nothing")}; RopeFire {p.M(MoverEventType.RopeFire) - rf2}, RopeAttach {p.M(MoverEventType.RopeAttach) - ra2}, the chest came within {close:0.00} m");
            }
        }
        N37Blocks.ForceOff = false;
        return 0;
    }

    /// <summary>--floors: the spot finders require a floor under each spot the rows stand on (the fix under test).</summary>
    static bool PFloorRule;

    /// <summary>A floor within 1.6 m under (x, y + 1, z) (the clear lane's own floor test in Open35).</summary>
    static bool PFloorAt(SurvivalSim sim, double x, double y, double z) =>
        sim.world.RayIntersect(new Vec3(x, y + 1.0, z), new Vec3(0, -1, 0), out RayHit dh) && dh.distance <= 1.6 && dh.normal.y > 0.7;

    /// <summary>The wall's rows stand on: its start (9, 10, 11), the slide's spot 3.3 m out (13), the grapple's spot 9 m out (18).</summary>
    static bool PWallFloors(SurvivalSim sim, Vec3 p0, Vec3 t, Vec3 n, double y) =>
        PFloorAt(sim, p0.x, y, p0.z) && PFloorAt(sim, p0.x + t.x * 2 + n.x * 3.3, y, p0.z + t.z * 2 + n.z * 3.3) && PFloorAt(sim, p0.x + t.x * 4 + n.x * 9, y, p0.z + t.z * 4 + n.z * 9);

    // ---- copies of FeelReel.N35.Verbs.cs's spot finders (Open35, Wall35, Node35), the same rays ----
    static (Vec3, double) POpen(SurvivalSim sim)
    {
        Vec3 bestAt = default;
        double bestYaw = 0, bestLen = 0;
        RoomDef bestRoom = null;
        foreach (RoomDef r in sim.campaignLevel.rooms)
        {
            if (r.kind == "boss") continue;
            Vec3 o = r.spawn;
            for (int k = 0; k < 16; k++)
            {
                double yaw = k * Math.PI / 8;
                Vec3 f = PFwd(yaw);
                double clear = 30;
                foreach (double h in new[] { 0.5, 1.5 })
                    if (sim.world.RayIntersect(new Vec3(o.x, o.y + h, o.z), f, out RayHit hit)) clear = Math.Min(clear, hit.distance);
                double len = 0;
                for (double d = 2; d <= clear - 1.5; d += 2)
                {
                    var q = new Vec3(o.x + f.x * d, o.y + 1.0, o.z + f.z * d);
                    if (!sim.world.RayIntersect(q, new Vec3(0, -1, 0), out RayHit dh) || dh.distance > 1.6) break;
                    len = d;
                }
                if (len > bestLen) { bestLen = len; bestAt = new Vec3(o.x, o.y + 0.2, o.z); bestYaw = yaw; bestRoom = r; }
            }
            if (bestLen >= 14) break;
        }
        return (bestAt, bestYaw);
    }

    static bool PWall(SurvivalSim sim, out Vec3 start, out Vec3 along, out Vec3 normal, out double length, out string room)
    {
        start = along = normal = default;
        length = 0;
        room = "";
        foreach (RoomDef r in sim.campaignLevel.rooms)
        {
            if (r.kind == "boss") continue;
            var c = new Vec3((r.x0 + r.x1) / 2, r.spawn.y + 1.0, (r.z0 + r.z1) / 2);
            for (int k = 0; k < 16; k++)
            {
                double a = k * Math.PI / 8;
                var d = new Vec3(Math.Sin(a), 0, Math.Cos(a));
                if (!sim.world.RayIntersect(c, d, out RayHit h) || h.distance > 20 || h.distance < 2) continue;
                Vec3 n = h.normal;
                if (Math.Abs(n.y) > 0.2) continue;
                double nl = Math.Sqrt(n.x * n.x + n.z * n.z);
                n = new Vec3(n.x / nl, 0, n.z / nl);
                var hit = new Vec3(c.x + d.x * h.distance, c.y, c.z + d.z * h.distance);
                var p0 = new Vec3(hit.x + n.x * 0.7, hit.y, hit.z + n.z * 0.7);
                foreach (int sgn in new[] { 1, -1 })
                {
                    var t = new Vec3(-n.z * sgn, 0, n.x * sgn);
                    double len = 0;
                    for (int mm = 1; mm <= 30; mm++)
                    {
                        var q = new Vec3(p0.x + t.x * mm, p0.y, p0.z + t.z * mm);
                        bool wall = sim.world.RayIntersect(q, new Vec3(-n.x, 0, -n.z), out RayHit hw) && hw.distance < 1.4;
                        bool clear = !sim.world.RayIntersect(new Vec3(q.x - t.x, q.y, q.z - t.z), t, out RayHit hc) || hc.distance > 1.05;
                        if (!wall || !clear) break;
                        len = mm;
                    }
                    if (len > length && (!PFloorRule || PWallFloors(sim, p0, t, n, r.spawn.y))) { length = len; start = new Vec3(p0.x, r.spawn.y + 0.2, p0.z); along = t; normal = n; room = r.id; }
                }
            }
        }
        return length >= 8;
    }

    static bool PNode(SurvivalSim sim, out Vec3 node, out Vec3 from, out double yaw, out double pitch)
    {
        node = from = default;
        yaw = pitch = 0;
        foreach (Vec3 nd in sim.map.grappleNodes)
        {
            foreach (RoomDef r in sim.campaignLevel.rooms)
            {
                if (nd.x < r.x0 - 2 || nd.x > r.x1 + 2 || nd.z < r.z0 - 2 || nd.z > r.z1 + 2) continue;
                for (int k = 0; k < 8; k++)
                {
                    double a = k * Math.PI / 4;
                    var f = new Vec3(nd.x + Math.Sin(a) * 10, r.spawn.y + 0.2, nd.z + Math.Cos(a) * 10);
                    if (f.x < r.x0 + 1 || f.x > r.x1 - 1 || f.z < r.z0 + 1 || f.z > r.z1 - 1) continue;
                    var eye = new Vec3(f.x, f.y + 1.6, f.z);
                    Vec3 to = nd - eye;
                    double len = to.Length;
                    if (sim.world.RayIntersect(eye, to / len, out RayHit h) && h.distance < len - 0.5) continue;
                    if (PFloorRule && !PFloorAt(sim, f.x, r.spawn.y, f.z)) continue;
                    node = nd;
                    from = f;
                    yaw = PYaw(to);
                    pitch = Math.Atan2(to.y, Math.Sqrt(to.x * to.x + to.z * to.z));
                    return true;
                }
            }
        }
        return false;
    }
}
