// EXT53 builder N (row N, RIDE KILL; design.n53.rideKill53): `n53ridekill`, N's own checks. Hooked without a Program.cs line (a module
// initializer runs the command and exits when argv[1] is "n53ridekill", Harness.N47.Feel.cs's pattern); it applies the off switches itself.
//   n53ridekill selftest     RideKill53Tests (Core) and N53RideKillTests (Game) on the harness's mini NUnit; build with
//                            -p:Extra="../../Assets/Tests/EditMode/RideKill53Tests.cs;../../Assets/Tests/EditMode/Game/N53RideKillTests.cs"
//   n53ridekill ride [--level Design/levels/n52-iceline.json] [--room g4] [--rows rideKill53] [--off53] [--shoot]
//     The a253 reel's scenario headless: a CAMPAIGN run on the level, the player put at the room's spawn; RideKill53Wall.Find's wall; first a
//     kill ON FOOT (the control: no refund), then the player put 1.2 m up beside the wall moving along it at 11 m/s with jump held and the
//     stick forward until the Mover is in WallRide, a husk 8 m ahead and 1.5 m off the wall killed by a player hit (Loadout.Damage) while
//     riding (the bar emptied first). GREEN (row on): the ride happened, the ride kill fired once, the bar rose by one boost over the foot
//     kill's own refill; RED with the row off (the config's own: no event).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void N53RideKillInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n53ridekill") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            string sub = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "ride";
            code = sub == "selftest" ? N53RkSelftest() : sub == "ride" ? N53RkRide(args) : 2;
            if (code == 2 && sub != "selftest" && sub != "ride") Console.WriteLine("n53ridekill: unknown subcommand " + sub + " (selftest | ride)");
        }
        catch (Exception e) { Console.WriteLine("n53ridekill: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static int N53RkSelftest()
    {
        int failed = 0, classes = 0;
        foreach (string name in new[] { "Skygrind.Core.Tests.RideKill53Tests", "Skygrind.Game.Tests.N53RideKillTests" })
        {
            Type t = typeof(Harness).Assembly.GetType(name);
            if (t == null) { Console.WriteLine("n53ridekill selftest: " + name + " not compiled in"); continue; }
            classes++;
            failed += NUnit.Framework.MiniRunner.Run(t, Console.WriteLine);
        }
        Console.WriteLine($"n53ridekill selftest: {(failed == 0 && classes == 2 ? "GREEN" : "RED")} ({classes} class(es), {failed} failing)");
        return failed == 0 && classes == 2 ? 0 : 1;
    }

    static string N53RkOpt(string[] args, string name, string dflt)
    {
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : dflt;
    }

    static bool N53RkShoot; // --shoot: the ride kill by a real shot (the reel's way), not Loadout.Damage

    const double Ahead = 10; // m along the wall the target stands (UnityTuning.RideKill53ReelAhead; ~9 deg down from the rider's eye, inside ride38's 15 deg)

    static int N53RkRide(string[] args)
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        string path = N53RkOpt(args, "--level", "Design/levels/n52-iceline.json");
        string roomId = N53RkOpt(args, "--room", "g4");
        string rowsArg = N53RkOpt(args, "--rows", "");
        N53RkShoot = Array.IndexOf(args, "--shoot") >= 0;
        GameConfig cfg = Config();
        SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
        LevelDef level = LevelDef.From(json);
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n24Override = new N24Blocks { campaign = true };
        sim.n25Override = new N25Blocks { tutorial = true };
        if (rowsArg.Length > 0)
        {
            var o = sim.rowsRegistry.Defaults();
            foreach (string r in rowsArg.Split(',')) o = o.Set(r.Trim(), NightRows.NightOfName(r.Trim()) < NightRows.ForceOffFrom); // --offNN still wins (the Player's switch)
            sim.rowsOverride = o;
        }
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(5301, null, null);
        for (int s = 0; s < 240; s++) { sim.health.hp = sim.health.max; sim.Step(); }
        var fired = new List<string>();
        int events = 0;
        double lastBefore = double.NaN, lastAfter = double.NaN;
        sim.events.On<RideKill53Event>(RideKill53.Event, e =>
        {
            events++;
            lastBefore = e.before;
            lastAfter = e.after;
            fired.Add($"rideKill53 #{e.count}: on the wall {e.onWall}, steps since the ride {e.stepsSinceRide}, {(e.bar ? "bar" : "pips")} {e.before.ToString("0.000", ci)} -> {e.after.ToString("0.000", ci)}, weapon {e.weapon}");
        });
        RoomDef rd = level.rooms.Find(r => r.id == roomId);
        if (rd == null) { Console.WriteLine($"n53ridekill ride: RED no room {roomId} in {path}"); return 1; }
        sim.zones?.OpenAll();
        sim.campaign?.SetDoorFree29(true); // the reel's DOORFREE (FeelReel.N41.DoorFree41): no door shuts on the line
        sim.Respawn(rd.spawn, rd.spawnYawDeg);
        sim.mover.invulnerable = true;
        void Idle(int n, InputFrame f) { var inp = (ScriptedInput)sim.input; for (int i = 0; i < n; i++) { inp.SetLook(f.yaw, f.pitch); inp.Set(f, new CombatInput { weapon = -1 }); sim.hitstopLeft = 0; sim.health.hp = sim.health.max; sim.Step(); } }
        Idle(30, new InputFrame { yaw = rd.spawnYawDeg * Math.PI / 180 });
        // the KESTREL in hand (the reel's Equip27: a swap through the combat input's slot, then the raise)
        if (sim.loadout.owned.IndexOf("revolver") < 0) sim.loadout.AddWeapon("revolver");
        {
            var inp0 = (ScriptedInput)sim.input;
            int slot = sim.loadout.owned.IndexOf("revolver");
            for (int i = 0; i < 360 && (sim.loadout.Weapon != "revolver" || i < 24); i++) { inp0.Set(new InputFrame(), new CombatInput { weapon = slot }); sim.hitstopLeft = 0; sim.Step(); }
            Idle(120, new InputFrame());
        }
        sim.husks.KillAll();
        double[] rect = { rd.x0, rd.z0, rd.x1, rd.z1 };
        // the reel's search exactly (FeelReel.N53.RideKill.cs: the room's spawn, its rect first; the numbers are UnityTuning.RideKill53ReelReach /
        // Length / StandOff, repeated here because UnityTuning.RideKill53.cs is not compiled into the harness)
        const double reach = 24, len = 14, off = 0.45;
        bool found = RideKill53Wall.Find(sim.world, rd.spawn, reach, len, off, rect, out Vec3 f0, out Vec3 u, out Vec3 n);
        bool inRoom = found;
        if (!found) found = RideKill53Wall.Find(sim.world, rd.spawn, reach, len, off, out f0, out u, out n);
        if (!found) { Console.WriteLine($"n53ridekill ride: RED no ride wall within 24 m of {roomId}'s spawn ({rd.spawn.x}, {rd.spawn.y}, {rd.spawn.z})"); return 1; }
        RideKill53Wall.Stick(u, n, 0.5, out double yaw, out double mx, out double mz);
        Console.WriteLine($"wall {(inRoom ? "in" : "near")} room {roomId}: start ({f0.x:0.00}, {f0.y:0.00}, {f0.z:0.00}), along ({u.x:0.00}, {u.z:0.00}), normal ({n.x:0.00}, {n.z:0.00}); row on {sim.RideKill53On}");
        // 1. the control: a kill on foot at the run line's start
        sim.Respawn(f0, yaw * 180 / Math.PI);
        sim.mover.invulnerable = true;
        Idle(30, new InputFrame { yaw = yaw });
        bool footGrounded = sim.mover.grounded && sim.mover.state != MoveState.WallRide;
        Husk hf = sim.husks.Spawn("husk", new Vec3(f0.x + u.x * Ahead + n.x * 1.5, f0.y, f0.z + u.z * Ahead + n.z * 1.5), f0);
        if (hf == null) Console.WriteLine("foot: the husk did not spawn");
        sim.mover.stamina.bar = 0;
        sim.mover.pips = 0;
        int missed0 = sim.RideKill53Missed;
        if (hf != null) sim.loadout.Damage(hf, 1e6, false, "revolver", sim.Eye);
        double footBar = sim.mover.stamina.rules.Any ? sim.mover.stamina.bar : sim.mover.pips;
        bool footKill = hf != null && !hf.alive, footPaid = events > 0;
        Console.WriteLine($"foot: grounded {footGrounded}, kill {footKill}, bar after {footBar.ToString("0.000", ci)}, rideKill53 {(footPaid ? "FIRED" : "no")}, missed +{sim.RideKill53Missed - missed0}");
        // 2. the ride: 1.2 m up beside the wall, along it at 11 m/s, a push into it, jump held, the stick forward
        Idle(60, new InputFrame { yaw = yaw });
        // the target first, its spawn tell waited out (1 s held in place, as the reel does: a shot at a husk still rising does not land)
        var rideSpot = new Vec3(f0.x + u.x * (Ahead + 2) + n.x * 1.5, f0.y, f0.z + u.z * (Ahead + 2) + n.z * 1.5);
        Husk hr = sim.husks.Spawn("husk", rideSpot, f0);
        for (int i = 0; i < 120 && hr != null; i++) { hr.position = hr.previous = rideSpot; hr.hp = hr.maxHp = 1; hr.alive = true; Idle(1, new InputFrame { yaw = yaw }); }
        sim.Respawn(new Vec3(f0.x, f0.y + 1.2, f0.z), yaw * 180 / Math.PI);
        sim.mover.invulnerable = true;
        sim.mover.velocity = new Vec3(u.x * 11 - n.x * 1.0, 0, u.z * 11 - n.z * 1.0);
        var ride = new InputFrame { yaw = yaw, moveX = mx, moveZ = mz, jump = new Button { held = true } };
        int rideStarts = 0;
        Action<MoverEvent> onMove = e => { if (e.type == MoverEventType.WallRideStart) rideStarts++; };
        sim.mover.Events += onMove;
        int t = 0;
        for (; t < 90 && sim.mover.state != MoveState.WallRide; t++) Idle(1, ride);
        bool riding = sim.mover.state == MoveState.WallRide;
        Console.WriteLine($"ride: {(riding ? "yes" : "NO")} after {t} steps (WallRideStart {rideStarts}), feet ({sim.mover.position.x:0.00}, {sim.mover.position.y:0.00}, {sim.mover.position.z:0.00}), speed {sim.mover.Speed:0.0} m/s");
        Idle(8, ride);
        riding = sim.mover.state == MoveState.WallRide;
        if (hr != null) { hr.position = hr.previous = rideSpot; hr.hp = hr.maxHp = 1; }
        sim.mover.stamina.bar = 0;
        sim.mover.pips = 0;
        int ev0 = events;
        int shots = 0;
        bool shotOnWall = false;
        if (hr != null && riding && N53RkShoot)
        {
            // --shoot: the a253 reel's way: a 1 hp husk held in place, the look from the eye at its aim point, the stick re-aimed along the wall
            // under that yaw, the weapon in hand fired by a one-step press (step 4, then every 40) until the husk dies (at most 120 steps)
            Vec3 spot = hr.position;
            var inp = (ScriptedInput)sim.input;
            int fires = 0, hits = 0, lastPress = -100;
            Action offF = sim.events.On("fire", o => fires++), offH = sim.events.On("hit", o => hits++);
            for (int i = 0; i < 120 && hr.alive; i++)
            {
                hr.position = hr.previous = spot;
                hr.hp = hr.maxHp = 1;
                sim.mover.stamina.bar = 0;
                sim.mover.pips = 0;
                Vec3 eye = sim.Eye, at = hr.AimPoint;
                double dx = at.x - eye.x, dy = at.y - eye.y, dz = at.z - eye.z;
                double ay = Math.Atan2(-dx, -dz), ap = Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz));
                double wx = u.x - n.x * 0.5, wz = u.z - n.z * 0.5, wl = Math.Sqrt(wx * wx + wz * wz);
                double fx = -Math.Sin(ay), fz = -Math.Cos(ay), rx = Math.Cos(ay), rz = -Math.Sin(ay);
                var mv = new InputFrame { yaw = ay, pitch = ap, moveZ = (wx * fx + wz * fz) / wl, moveX = (wx * rx + wz * rz) / wl, jump = new Button { held = true } };
                // a press when the shot's line is clear and the rider is on the wall (at most every 20 steps; the reel's rule)
                bool press = i - lastPress >= 20 && sim.mover.state == MoveState.WallRide && RideKill53Wall.LineClear(sim.world, eye, at);
                if (press) lastPress = i;
                if (Array.IndexOf(args, "--trace") >= 0 && i < 40 && i % 3 == 0)
                {
                    double tdd = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    bool hitW = sim.world.RayIntersect(eye, new Vec3(dx / tdd, dy / tdd, dz / tdd), out RayHit th);
                    Console.WriteLine($"  step {i}: {sim.mover.state}, feet ({sim.mover.position.x:0.00}, {sim.mover.position.y:0.00}, {sim.mover.position.z:0.00}), eye ({eye.x:0.00}, {eye.y:0.00}, {eye.z:0.00}), target ({at.x:0.00}, {at.y:0.00}, {at.z:0.00}) {tdd:0.00} m, ray {(hitW ? $"hit {th.distance:0.00} m n ({th.normal.x:0.00}, {th.normal.y:0.00}, {th.normal.z:0.00})" : "clear")}, wallNormal ({sim.mover.wallNormal.x:0.00}, {sim.mover.wallNormal.z:0.00})");
                }
                if (press)
                {
                    shots++;
                    shotOnWall = sim.mover.state == MoveState.WallRide;
                    double td = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    bool blocked = sim.world.RayIntersect(eye, new Vec3(dx / td, dy / td, dz / td), out RayHit bh) && bh.distance < td - 0.6;
                    Console.WriteLine($"  press {shots}: step {i}, riding {shotOnWall}, target {td:0.00} m, pitch {ap * 180 / Math.PI:0.0} deg, line blocked {(blocked ? $"YES at {bh.distance:0.00} m" : "no")}, feet y {sim.mover.position.y:0.00}, state {sim.mover.state}");
                }
                inp.SetLook(ay, ap);
                inp.Set(mv, press ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } } : new CombatInput { weapon = -1 });
                sim.hitstopLeft = 0;
                sim.health.hp = sim.health.max;
                sim.Step();
            }
            offF(); offH();
            Console.WriteLine($"shoot: weapon {sim.loadout.Weapon}, presses {shots} (fire events {fires}, hit events {hits}), the last on the wall {(shotOnWall ? "yes" : "NO")}, husk dead {(!hr.alive ? "yes" : "NO")}, riding now {sim.mover.state == MoveState.WallRide}");
        }
        else if (hr != null && riding) sim.loadout.Damage(hr, 1e6, false, "revolver", sim.Eye);
        double rideBar = sim.mover.stamina.rules.Any ? sim.mover.stamina.bar : sim.mover.pips;
        bool rideKill = hr != null && !hr.alive;
        int got = events - ev0;
        double unit = sim.mover.stamina.rules.Any ? cfg.Num("design.stamina.boost.cost") : cfg.movement.dash.pipCost;
        // the Damage path: the bar against the foot kill's (no step between); --shoot: the event's own before / after (the step's refill runs too)
        bool paid = got == 1 && (N53RkShoot ? Math.Abs(lastAfter - lastBefore - unit) < 1e-9 : Math.Abs(rideBar - footBar - unit) < 1e-9);
        if (N53RkShoot && !shotOnWall) paid = false;
        sim.mover.Events -= onMove;
        foreach (string l in fired) Console.WriteLine("  " + l);
        bool green = riding && footKill && !footPaid && rideKill && paid && sim.RideKill53On;
        Console.WriteLine($"n53ridekill ride: {(green ? "GREEN" : "RED")} | level {level.id}, room {roomId}, rideKill53 on {sim.RideKill53On} | ride {(riding ? "yes" : "NO")} (WallRideStart {rideStarts}), kill on the wall {(rideKill ? "yes" : "NO")}, "
            + $"rideKill53 events {got}, bar after {rideBar.ToString("0.000", ci)} vs the foot kill's {footBar.ToString("0.000", ci)} (+{(rideBar - footBar).ToString("0.000", ci)}, one boost = {unit.ToString("0.000", ci)}) | foot kill {(footKill ? "yes" : "NO")}, paid {(footPaid ? "YES" : "no")}");
        return green ? 0 : 1;
    }
}
