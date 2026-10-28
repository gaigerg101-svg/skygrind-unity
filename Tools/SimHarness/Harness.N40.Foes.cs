// NIGHT40 builder E3 (NIGHT40.md card E3): the campaign roster (the DRONE, the WARDEN, the CHARGER) and foes40. One entry, N40Foes (declared and called
// in Program.cs). Commands:
//   n40foes spawn [--off40] [--level path] [--room id] [--seconds s]
//     the root-cause line: can a campaign wave spawn each kind? Per kind (drone, warden, charger): the level (n37-iceline by default) loaded, the room's
//     first wave replaced IN MEMORY by [kind] (the files are untouched), a CAMPAIGN run, the player idle at the room's spawn (kept alive) for s seconds
//     (12). Prints: spawned (the enemySpawn event of that kind through Campaign24.SpawnWave -> the sim's world.Spawn -> Husks.Spawn), the brain attached,
//     its first attack (droneCharge / wardenTell / chargerTell) and its first hurt on the player, by source. PASS: all three spawn and attack.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N40Foes(string cmd, string[] args, ref int code)
    {
        if (cmd != "n40foes") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "spawn";
        if (what == "spawn") { code = Foes40Spawn(args); return; }
        if (what == "drone") { code = Foes40Drone(args); return; }
        if (what == "ttk") { code = Foes40Ttk(args); return; }
        if (what == "trace") { code = Foes40Trace(args); return; }
        if (what == "walk") { code = Foes40Walk(args); return; }
        if (what == "level") { code = Foes40Level(args); return; }
        Console.WriteLine($"n40foes: unknown sub-command '{what}' (spawn | drone | ttk)");
        code = 2;
    }

    static string Opt40F(string[] args, string key, string def)
    {
        int i = Array.IndexOf(args, key);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : def;
    }

    static readonly string[] Foes40Kinds = { "drone", "warden", "charger" };

    /// <summary>A CAMPAIGN sim on `path` whose room `roomId` (null: the first room with waves) has its waves replaced by `waves` (in memory).</summary>
    static SurvivalSim Foes40Sim(string path, string roomId, List<object> waves, N40Blocks? n40, uint seed, out RoomDef room, bool noPickup = false)
    {
        GameConfig cfg = Config();
        var json = PortData.Obj(PortData.LoadJson(path));
        var rooms = PortData.Arr(PortData.Obj(json["campaign"])["rooms"]);
        string id = null;
        foreach (object ro in rooms)
        {
            var rd = PortData.Obj(ro);
            if (roomId != null ? (string)rd["id"] == roomId : rd.ContainsKey("waves") && rd["waves"] != null)
            {
                rd["waves"] = waves;
                if (noPickup) { rd.Remove("pickup31"); rd.Remove("pickups"); } // the TTK: no weapon picked up on the way
                id = (string)rd["id"];
                break;
            }
        }
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(json));
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        if (n40.HasValue) sim.n40Override = n40;
        sim.StartRun(seed, null, null);
        room = sim.campaignLevel.rooms.Find(r => r.id == id);
        return sim;
    }

    static int Foes40Spawn(string[] args)
    {
        string path = Opt40F(args, "--level", "Design/levels/n37-iceline.json");
        string roomId = Opt40F(args, "--room", null);
        double seconds = double.Parse(Opt40F(args, "--seconds", "12"), CultureInfo.InvariantCulture);
        bool off = Array.IndexOf(args, "--off40") >= 0;
        int bad = 0;
        Console.WriteLine($"n40foes spawn: {path}, the room's first wave replaced by one enemy of each kind (in memory), the player idle at the room's spawn {seconds:0} s, kept alive; foes40 {(off ? "OFF" : "ON (forced)")}");
        foreach (string kind in Foes40Kinds)
        {
            var waves = new List<object> { new List<object> { kind } };
            SurvivalSim sim = Foes40Sim(path, roomId, waves, off ? N40Blocks.Off : new N40Blocks { foes40 = true }, 1, out RoomDef room);
            var si = (ScriptedInput)sim.input;
            int spawned = 0, attacks = 0, hurts = 0, spawnId = -1;
            double tSpawn = -1, tAttack = -1, tHurt = -1;
            string firstAttack = "-";
            Vec3 at = default;
            sim.events.On<SpawnEvent>("enemySpawn", e =>
            {
                if (e.kind != kind) return;
                spawned++;
                if (tSpawn < 0) { tSpawn = sim.SimTime; spawnId = e.id; at = e.position; }
            });
            void Attack(string name)
            {
                attacks++;
                if (tAttack < 0) { tAttack = sim.SimTime; firstAttack = name; }
            }
            sim.events.On<DroneEvent>("droneCharge", _ => Attack("droneCharge"));
            sim.events.On<WardenEvent>("wardenTell", _ => Attack("wardenTell"));
            sim.events.On<object>("chargerTell", _ => Attack("chargerTell"));
            sim.events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner != "player" || e.source != kind) return;
                hurts++;
                if (tHurt < 0) tHurt = sim.SimTime;
            });
            double t0 = sim.SimTime;
            if (room != null) sim.Respawn(new Vec3(room.spawn.x, room.spawn.y + 0.05, room.spawn.z), room.spawnYawDeg);
            int max = MovementMath.StepsFor(seconds, sim.step);
            for (int s = 0; s < max; s++)
            {
                sim.health.hp = sim.health.max;
                if (sim.flow == Skygrind.Game.Flow.Down) { sim.downSteps = 0; sim.flow = Skygrind.Game.Flow.Playing; }
                si.Set(new InputFrame { yaw = room != null ? room.spawnYawDeg * Math.PI / 180 : 0 }, default);
                sim.Step();
                sim.hitstopLeft = 0;
            }
            bool ok = spawned > 0 && attacks > 0;
            if (!ok) bad++;
            string F(double t) => t >= 0 ? (t - t0).ToString("0.00", CultureInfo.InvariantCulture) + " s" : "never";
            Console.WriteLine($"  {kind,-8} room {room?.id ?? "?",-4}: spawned {spawned} (at {F(tSpawn)}, ({at.x:0.0}, {at.y:0.0}, {at.z:0.0})), alive {sim.husks.CountOf(kind)}, "
                + $"first attack {firstAttack} at {F(tAttack)} ({attacks} in all), first hurt by {kind} at {F(tHurt)} ({hurts}): {(ok ? "PASS" : "RED")}");
        }
        Console.WriteLine($"n40foes spawn {(bad == 0 ? "PASS" : "RED")}: {Foes40Kinds.Length - bad}/{Foes40Kinds.Length} kinds spawn from a campaign wave and attack");
        return bad == 0 ? 0 : 1;
    }

    /// <summary>The approach: on the survival map (open floor), one drone placed 40 m out along an open line with its brain, the player still; the median
    /// horizontal speed while it is farther than the far ring + 2 m (ai.rings[2] 24 m: it is flying in), and the same for a husk (the reference).</summary>
    static (double drone, double husk) Foes40Approach(bool on)
    {
        var res = new double[2];
        string[] kinds = { "drone", "husk" };
        for (int k = 0; k < 2; k++)
        {
            GameConfig cfg = Config();
            Core36Keep.Clear();
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noRecords = sim.noLab = true;
            sim.n40Override = on ? new N40Blocks { foes40 = true } : N40Blocks.Off;
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(4001, null, null);
            sim.holdRunClock = true;
            sim.mover.invulnerable = true;
            Vec3 sp = sim.map.playerSpawn;
            sim.Respawn(sp, 0);
            (double yaw, double ux, double uz) = Core36OpenDir(sim, sp, 40);
            foreach (Enemy e in sim.enemies.all) e.alive = false;
            Husk h = sim.husks.Spawn(kinds[k], new Vec3(sp.x + ux * 40, sp.y, sp.z + uz * 40), sp);
            if (h == null) { res[k] = double.NaN; continue; }
            sim.brain.Attach(h);
            Core36Keep.Add(h);
            var speeds = new List<double>();
            double far = PortData.Num(PortData.Obj(cfg.Arr("ai.rings")[2])["radius"]) + 2;
            for (int i = 0; i < MovementMath.StepsFor(8, sim.step); i++)
            {
                double dist = JsMath.Hypot(h.position.x - sim.mover.position.x, h.position.z - sim.mover.position.z);
                if (i * sim.step > 1.0 && dist > far) speeds.Add(JsMath.Hypot(h.velocity.x, h.velocity.z));
                Core36Tick(sim, yaw, 0, NoCombat36);
            }
            speeds.Sort();
            res[k] = speeds.Count > 0 ? speeds[speeds.Count / 2] : double.NaN;
        }
        return (res[0], res[1]);
    }

    /// <summary>`n40foes drone [--seeds n]`: the DRONE with foes40 forced ON and OFF, on n37-iceline i1a (one drone, the player at the room's exit, kept alive).
    /// Measures: the glow (droneCharge -> orbFire, s), the orb's speed, the approach (Foes40Approach: the survival map, 40 m out, vs a husk), and the parry: a
    /// scripted player looks at the nearest hostile orb and punches (or swings the gauntlet, when it is in hand) when the orb is inside 1.6 m of the eye; the
    /// reflected orbs that reach their own drone, and whether that drone died (time from its first glow). PASS (ON): glow 1.0 s, orb 12 m/s, approach
    /// 3.6 m/s +- 0.3 (0.6x the husk's 6), every seed's drone killed by its own returned orb.</summary>
    static int Foes40Drone(string[] args)
    {
        int seeds = int.Parse(Opt40F(args, "--seeds", "5"), CultureInfo.InvariantCulture);
        int fails = 0;
        foreach (bool on in new[] { true, false })
        {
            (double appDrone, double appHusk) = Foes40Approach(on);
            var glows = new List<double>();
            var orbSpeeds = new List<double>();
            var approach = new List<double>();
            int kills = 0, returns = 0, reflects = 0, punches = 0;
            double killT = 0;
            string weapon = "";
            for (int s = 0; s < seeds; s++)
            {
                var waves = new List<object> { new List<object> { "drone" } };
                SurvivalSim sim = Foes40Sim("Design/levels/n37-iceline.json", null, waves, on ? new N40Blocks { foes40 = true } : N40Blocks.Off, (uint)(1 + s), out RoomDef room);
                var si = (ScriptedInput)sim.input;
                // the player at the room's exit (the far end from the wave's nodes) so the drone flies in from beyond its range
                sim.Respawn(new Vec3(room.exit.x, room.exit.y + 0.05, room.exit.z), room.spawnYawDeg);
                weapon =sim.loadout.Weapon + " [" + string.Join(",", sim.loadout.owned) + "]";
                int chargeStep = -1, firstGlow = -1, deathStep = -1;
                sim.events.On<DroneEvent>("droneCharge", _ => { chargeStep = sim.simStep; if (firstGlow < 0) firstGlow = sim.simStep; });
                sim.events.On<DroneOrbs.Orb>("orbFire", o =>
                {
                    if (chargeStep >= 0) glows.Add((sim.simStep - chargeStep) * sim.step);
                    chargeStep = -1;
                    orbSpeeds.Add(o.velocity.Length);
                });
                sim.events.On<int>("orbReflect", _ => reflects++);
                sim.events.On<KillEvent>("kill", k => { if (k.kind == "drone" && deathStep < 0) deathStep = sim.simStep; });
                int returns0 = sim.husks.orbs.shooterReturns;
                double yaw = room.spawnYawDeg * Math.PI / 180, pitch = 0;
                int max = MovementMath.StepsFor(20, sim.step);
                for (int k = 0; k < max && deathStep < 0; k++)
                {
                    sim.health.hp = sim.health.max;
                    if (sim.flow == Skygrind.Game.Flow.Down) { sim.downSteps = 0; sim.flow = Skygrind.Game.Flow.Playing; }
                    Vec3 eye = new Vec3(sim.mover.position.x, sim.mover.position.y + sim.cfg.movement.body.eyeHeight, sim.mover.position.z);
                    List<Husk> drones = sim.husks.PoolOf("drone");
                    Husk d = drones.Count > 0 && drones[0].alive ? drones[0] : null;
                    DroneOrbs.Orb near = null;
                    double nd = double.PositiveInfinity;
                    foreach (DroneOrbs.Orb o in sim.husks.orbs.pool)
                    {
                        if (!o.alive || o.reflected) continue;
                        double dd = Vec3.DistanceTo(o.position, eye);
                        if (dd < nd) { nd = dd; near = o; }
                    }
                    Vec3 look = near != null ? near.position : d != null ? d.position : eye + new Vec3(0, 0, -1);
                    Vec3 v = look - eye;
                    if (v.Length > 1e-6) { yaw = JsMath.Atan2(-v.x, -v.z); pitch = JsMath.Atan2(v.y, JsMath.Hypot(v.x, v.z)); }
                    bool punch = near != null && nd <= 1.6;
                    if (punch) punches++;
                    si.SetLook(yaw, pitch);
                    // the gauntlet in hand drops the punch (its swing is the melee): the swing is pressed instead
                    bool gauntlet = sim.loadout.Weapon == "gauntlet";
                    var press = new Button { pressed = punch, held = punch };
                    si.Set(new InputFrame { yaw = yaw, pitch = pitch }, gauntlet ? new CombatInput { weapon = -1, fire = press } : new CombatInput { weapon = -1, punch = press });
                    if (d != null)
                    {
                        double dist = JsMath.Hypot(d.position.x - sim.mover.position.x, d.position.z - sim.mover.position.z);
                        double hs = JsMath.Hypot(d.velocity.x, d.velocity.z);
                        if (hs > 0.5) approach.Add(hs); _ = dist;
                    }
                    sim.Step();
                    sim.hitstopLeft = 0;
                }
                returns += sim.husks.orbs.shooterReturns - returns0;
                if (Array.IndexOf(args, "--dbg") >= 0)
                    Console.WriteLine($"    seed {s + 1}: punch events {(sim.events.Emitted.TryGetValue("punch", out int pe) ? pe : 0)}, parry {(sim.events.Emitted.TryGetValue("parry", out int pa) ? pa : 0)}, orbDestroyed {(sim.events.Emitted.TryGetValue("orbDestroyed", out int od) ? od : 0)}, hurt {(sim.events.Emitted.TryGetValue("hurt", out int hu) ? hu : 0)}, flow {sim.flow}, campaign room {sim.campaign?.room}, in hand {sim.loadout.Weapon}, rules30 dropped melee {sim.loadout.rules30?.droppedMelee}, lockout29 {(sim.loadout.lockout29 != null)}");
                if (deathStep >= 0)
                {
                    kills++;
                    killT += (deathStep - firstGlow) * sim.step;
                }
            }
            glows.Sort();
            orbSpeeds.Sort();
            approach.Sort();
            double Med(List<double> l) => l.Count > 0 ? l[l.Count / 2] : double.NaN;
            double glow = Med(glows), orb = Med(orbSpeeds);
            bool ok = !on || Math.Abs(glow - 1.0) < 0.02 && Math.Abs(orb - 12) < 0.01 && Math.Abs(appDrone - 3.6) <= 0.3 && kills == seeds;
            if (!ok) fails++;
            Console.WriteLine($"  foes40 {(on ? "ON " : "OFF")}: weapon {weapon}; glow {glow:0.00} s (n {glows.Count}), orb {orb:0.0} m/s, approach {appDrone:0.00} m/s (a husk {appHusk:0.00}: x{appDrone / appHusk:0.00}); "
                + $"punches {punches}, orbs reflected {reflects}, back at their own drone {returns}, drones killed {kills}/{seeds}{(kills > 0 ? $" ({killT / kills:0.0} s after the first glow)" : "")}{(on ? (ok ? ": PASS" : ": RED") : "")}");
        }
        Console.WriteLine($"n40foes drone {(fails == 0 ? "PASS" : "RED")}");
        return fails == 0 ? 0 : 1;
    }

    // ---- ttk: the time-to-kill matrix -----------------------------------------------------------------------------------------------------------------
    // n40foes ttk [--off40] [--seeds n] [--cap s] [--kinds a,b] : on the survival map's open floor (Core36's setup: the director's own enemies removed every
    // step, the player invulnerable), one enemy of each kind placed out along an open line (drone 14 m, warden 8 m, charger 14 m) with its brain, and a
    // scripted player holding ONE weapon (the loadout reset to it) who aims perfectly and taps the trigger every other step:
    //   revolver / shotgun: stand and shoot (dodging the CHARGER's line sideways during its tell and charge); gauntlet: walk into 2 m and swing;
    //   counter: the kind's answer: DRONE = the parry (gauntlet in hand, swing only at an orb in reach, never at the drone), WARDEN = the gauntlet's swing
    //   into its shield (the punch-stun) and on, CHARGER = the shotgun: dodge the charge, then into its back while it is stunned (x3).
    // Prints the median TTK (s, spawn -> death; cap s = not killed) per kind x mode and the counter's ratio to the best other mode. PASS: counter >= 1.5x.
    static readonly string[] Ttk40Modes = { "revolver", "shotgun", "gauntlet", "counter" };

    static int Foes40Ttk(string[] args)
    {
        bool off = Array.IndexOf(args, "--off40") >= 0;
        int seeds = int.Parse(Opt40F(args, "--seeds", "3"), CultureInfo.InvariantCulture);
        ttk40Dbg = Array.IndexOf(args, "--dbg") >= 0;
        ttk40Level = Opt40F(args, "--level", ttk40Level);
        double cap = double.Parse(Opt40F(args, "--cap", "40"), CultureInfo.InvariantCulture);
        string[] kinds = Opt40F(args, "--kinds", "drone,warden,charger").Split(',');
        Console.WriteLine($"n40foes ttk (foes40 {(off ? "OFF" : "ON (forced)")}; {seeds} seeds; median s spawn -> death, cap {cap:0} s = not killed)");
        Console.WriteLine($"  {"kind",-8} {"revolver",9} {"shotgun",9} {"gauntlet",9} {"counter",9}  counter vs best other");
        int bad = 0;
        foreach (string kind in kinds)
        {
            var med = new double[Ttk40Modes.Length];
            for (int m = 0; m < Ttk40Modes.Length; m++)
            {
                var t = new List<double>();
                for (int s = 0; s < seeds; s++) t.Add(Ttk40Run(kind, Ttk40Modes[m], off, (uint)(4100 + s), cap));
                t.Sort();
                med[m] = t[t.Count / 2];
            }
            // the WARDEN's counter IS the gauntlet column (its swing into the shield): compared with the two guns only
            double best = kind == "warden" ? Math.Min(med[0], med[1]) : Math.Min(med[0], Math.Min(med[1], med[2]));
            double ratio = best / med[3];
            bool ok = med[3] < cap && ratio >= 1.5;
            if (!ok) bad++;
            string F(double v) => v >= cap ? "never" : v.ToString("0.0", CultureInfo.InvariantCulture);
            Console.WriteLine($"  {kind,-8} {F(med[0]),9} {F(med[1]),9} {F(med[2]),9} {F(med[3]),9}  x{ratio:0.00} {(ok ? "PASS" : "RED")}");
        }
        Console.WriteLine($"n40foes ttk {(bad == 0 ? "PASS" : "RED")}: {kinds.Length - bad}/{kinds.Length} kinds die >= 1.5x faster to their counter");
        return off ? 0 : bad == 0 ? 0 : 1;
    }

    static bool ttk40Dbg;
    static string ttk40Level = "Design/levels/n37-iceline.json";
    static Vec3? ttk40Spot;
    static double ttk40Ux, ttk40Uz;

    /// <summary>A floor point of the survival map with 26 m of open floor along one direction and 8 m to each side of that line (found once: a grid
    /// search, spheres of 0.5 m at 0.6 and 1.4 m up touching nothing, a floor under every sample within 0.5 m of the start's height).</summary>
    static (Vec3, double, double) Ttk40Spot(SurvivalSim sim)
    {
        if (ttk40Spot.HasValue) return (ttk40Spot.Value, ttk40Ux, ttk40Uz);
        bool Clear(Vec3 c)
        {
            foreach (double hh in new[] { 0.6, 1.4 })
            {
                var q = new Vec3(c.x, c.y + hh, c.z);
                if (WorldCollider.CapsuleIntersect(sim.world, new Capsule { start = q, end = q, radius = 0.5 }, out _, out _)) return false;
            }
            return sim.world.RayIntersect(new Vec3(c.x, c.y + 0.5, c.z), new Vec3(0, -1, 0), out RayHit hit) && hit.distance < 1.0;
        }
        for (int gx = -12; gx <= 12; gx++)
            for (int gz = -12; gz <= 12; gz++)
            {
                var top = new Vec3(gx * 8.0, 60, gz * 8.0);
                if (!sim.world.RayIntersect(top, new Vec3(0, -1, 0), out RayHit down)) continue;
                var p = new Vec3(top.x, top.y - down.distance + 0.05, top.z);
                for (int k = 0; k < 8; k++)
                {
                    double a = k * Math.PI / 4, dx = Math.Sin(a), dz = -Math.Cos(a);
                    bool ok = true;
                    for (double s = 0; s <= 26 && ok; s += 1)
                        for (double w = -8; w <= 8 && ok; w += 2)
                            ok = Clear(new Vec3(p.x + dx * s - dz * w, p.y, p.z + dz * s + dx * w));
                    if (!ok) continue;
                    ttk40Spot = p;
                    ttk40Ux = dx;
                    ttk40Uz = dz;
                    return (p, dx, dz);
                }
            }
        Console.WriteLine("    (ttk: no open spot found; the spawn)");
        ttk40Spot = sim.map.playerSpawn;
        ttk40Ux = 0;
        ttk40Uz = -1;
        return (ttk40Spot.Value, 0, -1);
    }

    /// <summary>One idle step that keeps every enemy (the wave's spawn comes on the room's first steps).</summary>
    static void Core36TickAll(SurvivalSim sim, double yaw)
    {
        var inp = (ScriptedInput)sim.input;
        inp.SetLook(yaw, 0);
        inp.Set(default, new CombatInput { weapon = -1 });
        sim.hitstopLeft = 0;
        sim.Step();
    }

    static double Ttk40Run(string kind, string mode, bool off, uint seed, double cap)
    {
        // a walled campaign room (the survival map's open floor let a charge or a knock carry the body off an edge): the level's first room with
        // waves (n37-iceline i1a by default, --level), its wave replaced in memory by the one enemy; the player at the room's spawn
        Core36Keep.Clear();
        SurvivalSim sim = Foes40Sim(ttk40Level, null, new List<object> { new List<object> { kind } }, off ? N40Blocks.Off : new N40Blocks { foes40 = true }, seed, out RoomDef room, noPickup: true);
        sim.mover.invulnerable = true;
        string weapon = mode == "counter" ? (kind == "charger" ? "shotgun" : "gauntlet") : mode;
        sim.Respawn(new Vec3(room.spawn.x, room.spawn.y + 0.05, room.spawn.z), room.spawnYawDeg);
        double yaw = room.spawnYawDeg * Math.PI / 180;
        List<Husk> list = sim.husks.PoolOf(kind);
        for (int w = 0; w < 120 && !(list.Count > 0 && list[0].alive); w++) Core36TickAll(sim, yaw);
        Husk h = list.Count > 0 && list[0].alive ? list[0] : null;
        if (h == null) return cap;
        sim.loadout.Reset(new List<string> { weapon }, seed); // after the campaign's own start set
        Core36Keep.Add(h);
        string killedBy = "-";
        sim.events.On<KillEvent>("kill", k => { if (k.enemy == h.id) killedBy = k.owner + "/" + k.weapon; });
        double pitch = 0;
        int max = MovementMath.StepsFor(cap, sim.step);
        int blocked = 0;
        var byWeapon = new Dictionary<string, double>();
        sim.events.On<EnemyBlockedEvent>("enemyBlocked", _ => blocked++);
        sim.events.On<HitEvent>("hit", e => { if (e.enemy == h.id) { byWeapon.TryGetValue(e.weapon ?? "?", out double v); byWeapon[e.weapon ?? "?"] = v + e.damage; } });
        for (int i = 0; i < max; i++)
        {
            if (!h.alive || i == max - 1)
            {
                if (ttk40Dbg)
                {
                    var parts = new List<string>();
                    foreach (var kv in byWeapon) parts.Add($"{kv.Key} {kv.Value:0}");
                    Console.WriteLine($"    [{kind} {mode} seed {seed}] in hand {sim.loadout.Weapon} [{string.Join(",", sim.loadout.owned)}], blocked {blocked}, damage by weapon: {string.Join(", ", parts)}, {(h.alive ? "ALIVE" : "dead by " + killedBy)} at {i * sim.step:0.0} s");
                }
                if (!h.alive) return i * sim.step;
            }
            Vec3 eye = sim.Eye, me = sim.mover.position;
            Vec3 aimAt = h.AimPoint;
            Vec3 goal = me;
            bool shoot = true;
            var ch = h as Charger;
            // the CHARGER's line: off it sideways during the tell and the charge (every mode dodges; the counter differs in where it shoots from)
            if (ch != null && (ch.state == "tell" || ch.state == "charge"))
            {
                double fx = -JsMath.Sin(ch.chargeYaw), fz = -JsMath.Cos(ch.chargeYaw);
                double rx = me.x - ch.position.x, rz = me.z - ch.position.z;
                double side = rx * fz - rz * fx; // which side of the line the player is on
                double sx = fz * (side >= 0 ? 1 : -1), sz = -fx * (side >= 0 ? 1 : -1);
                goal = new Vec3(me.x + sx * 4, me.y, me.z + sz * 4);
            }
            else if (mode == "gauntlet" || (mode == "counter" && kind == "warden"))
            {
                double d = JsMath.Hypot(h.position.x - me.x, h.position.z - me.z);
                if (d > 2.0) goal = h.position;
            }
            else if (mode == "counter" && ch != null)
            {
                // into its back while it is stunned: the spot 2.5 m behind it (opposite its facing), shooting only from behind
                shoot = false;
                if (ch.state == "stunned")
                {
                    double fx = -JsMath.Sin(ch.yaw), fz = -JsMath.Cos(ch.yaw);
                    goal = new Vec3(ch.position.x - fx * 2.5, me.y, ch.position.z - fz * 2.5);
                    shoot = (me.x - ch.position.x) * fx + (me.z - ch.position.z) * fz < 0;
                }
            }
            if (mode == "counter" && kind == "drone")
            {
                // the parry: look at the nearest hostile orb; swing only when it is inside 1.6 m of the eye
                shoot = false;
                DroneOrbs.Orb near = null;
                double nd = double.PositiveInfinity;
                foreach (DroneOrbs.Orb o in sim.husks.orbs.pool)
                {
                    if (!o.alive || o.reflected) continue;
                    double dd = Vec3.DistanceTo(o.position, eye);
                    if (dd < nd) { nd = dd; near = o; }
                }
                if (near != null) { aimAt = near.position; shoot = nd <= 1.6; }
            }
            Vec3 v = aimAt - eye;
            if (v.Length > 1e-6) { yaw = JsMath.Atan2(-v.x, -v.z); pitch = JsMath.Atan2(v.y, JsMath.Hypot(v.x, v.z)); }
            var f = new InputFrame();
            Vec3 to = goal - me;
            double gl = JsMath.Hypot(to.x, to.z);
            if (gl > 0.3)
            {
                Vec3 want = new Vec3(to.x / gl, 0, to.z / gl);
                Vec3 fwd = MovementMath.WishDirection(0, 1, yaw), right = MovementMath.WishDirection(1, 0, yaw);
                f.moveZ = want.x * fwd.x + want.z * fwd.z;
                f.moveX = want.x * right.x + want.z * right.z;
            }
            bool press = shoot && i % 2 == 0;
            Core36Tick(sim, yaw, pitch, new CombatInput { weapon = -1, fire = new Button { pressed = press, held = press } }, f);
        }
        return cap;
    }

    /// <summary>`n40foes walk [--seeds n] [--cap s] [--wave a,b] [--level path] [--weapons a,b]`: the walk bot (AutopilotInput + Walk29Pilot) in the level's first
    /// room with waves, its wave replaced by `wave` (default warden / charger / drone, one run each), with Foes40Combat (Foes40WalkInput) and without it (the
    /// control), foes40 on, the loadout set to `weapons` (default gauntlet: level 2's start) once the run began. Per run: the room cleared in s (cap: not),
    /// the player's deaths (room retries). PASS: every kind cleared on every seed with Foes40Combat.</summary>
    static int Foes40Walk(string[] args)
    {
        int seeds = int.Parse(Opt40F(args, "--seeds", "5"), CultureInfo.InvariantCulture);
        double cap = double.Parse(Opt40F(args, "--cap", "90"), CultureInfo.InvariantCulture);
        string path = Opt40F(args, "--level", "Design/levels/n37-iceline.json");
        string[] waveKinds = Opt40F(args, "--wave", "warden,charger,drone").Split(',');
        var weapons = new List<string>(Opt40F(args, "--weapons", "gauntlet").Split(','));
        int bad = 0;
        Console.WriteLine($"n40foes walk: {path}'s first wave room, one kind each, the walk bot with the loadout [{string.Join(",", weapons)}], foes40 on, cap {cap:0} s, {seeds} seeds");
        foreach (string kind in waveKinds)
            foreach (bool with in new[] { true, false })
            {
                var times = new List<double>();
                int cleared = 0, deaths = 0;
                for (int s = 0; s < seeds; s++)
                {
                    uint seed = (uint)(1 + s);
                    SurvivalSim sim = Foes40Sim(path, null, new List<object> { new List<object> { kind } }, new N40Blocks { foes40 = true }, seed, out RoomDef room, noPickup: true);
                    AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
                    AutopilotInput.GraphSource = BotNav.Survival;
                    var ap = new AutopilotInput(sim, "normal", seed);
                    ap.GoalProvider = () => room.spawn;
                    var pilot = new Walk29Pilot(sim, ap);
                    sim.UseInput(with ? new Foes40WalkInput(pilot) : (IPlayerInput)pilot);
                    double t = -1;
                    sim.events.On<RoomEvent>("roomClear", e => { if (e.id == room.id && t < 0) t = e.time; });
                    sim.events.On<RoomEvent>("roomRetry", e => { if (e.id == room.id) deaths++; });
                    sim.loadout.Reset(weapons, seed);
                    int max = MovementMath.StepsFor(cap, sim.step);
                    for (int k = 0; k < max && t < 0; k++)
                    {
                        if (k == 1) sim.loadout.Reset(weapons, seed); // after the campaign's own start set
                        AutopilotInput.TakeDraft(sim);
                        sim.Step();
                    }
                    ap.Detach();
                    if (t >= 0) { cleared++; times.Add(t); }
                }
                times.Sort();
                bool ok = !with || cleared == seeds;
                if (!ok) bad++;
                Console.WriteLine($"  {kind,-8} {(with ? "Foes40Combat" : "control    ")}: cleared {cleared}/{seeds}, median {(times.Count > 0 ? times[times.Count / 2].ToString("0.0", CultureInfo.InvariantCulture) + " s" : "-")}, deaths {deaths}{(with ? (ok ? ": PASS" : ": RED") : "")}");
            }
        Console.WriteLine($"n40foes walk {(bad == 0 ? "PASS" : "RED")}");
        return bad == 0 ? 0 : 1;
    }

    /// <summary>`n40foes level --level path [--seeds n] [--cap s] [--off]`: the walk bot (Walk29Pilot + Foes40WalkInput; --off: the pilot alone) through a whole
    /// campaign level (L2's n40-iceline: pass its path), foes40 and iceline40 on, the player kept alive. Per room: cleared k / n and the median s; over the
    /// level: the most enemies committed to an attack at once (a wind-up, a charge, a tell: Husk.committed) and the share of fight time with more than two
    /// (Doom 2016's token pool, the card's 'at most two winding up at once').</summary>
    static int Foes40Level(string[] args)
    {
        string path = Opt40F(args, "--level", null);
        if (path == null) { Console.WriteLine("n40foes level: --level <path> (n40-iceline.json)"); return 2; }
        int seeds = int.Parse(Opt40F(args, "--seeds", "3"), CultureInfo.InvariantCulture);
        double cap = double.Parse(Opt40F(args, "--cap", "600"), CultureInfo.InvariantCulture);
        bool with = Array.IndexOf(args, "--off") < 0;
        string traceRoom = Opt40F(args, "--trace", null);
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        var times = new List<double>[playable];
        var cleared = new int[playable];
        for (int i = 0; i < playable; i++) times[i] = new List<double>();
        int maxCommitted = 0;
        long fightSteps = 0, overTwo = 0;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(int.Parse(Opt40F(args, "--first", "1"), CultureInfo.InvariantCulture) + s);
            SurvivalSim sim = SurvivalSim.Headless(Config(), SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            sim.n40Override = new N40Blocks { foes40 = true, iceline40 = true };
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.GoalProvider = () =>
            {
                Campaign24 c = sim.campaign;
                LevelDef lv = sim.campaignLevel;
                if (c == null || c.room < 0) return lv.rooms[0].spawn;
                if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
            };
            var pilot = new Walk29Pilot(sim, ap);
            sim.UseInput(with ? new Foes40WalkInput(pilot) : (IPlayerInput)pilot);
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room < playable) { cleared[e.room]++; times[e.room].Add(e.time); } });
            sim.StartRun(seed, null, null);
            int max = MovementMath.StepsFor(cap, sim.step);
            for (int st = 0; st < max; st++)
            {
                sim.health.hp = sim.health.max;
                if (sim.flow == Skygrind.Game.Flow.Down) { sim.downSteps = 0; sim.flow = Skygrind.Game.Flow.Playing; }
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (traceRoom != null && s == 0 && st % MovementMath.StepsFor(3, sim.step) == 0 && sim.campaign != null && sim.campaign.room >= 0 && sim.campaignLevel.rooms[sim.campaign.room].id == traceRoom)
                {
                    var al = new List<string>();
                    foreach (Enemy e in sim.enemies.all)
                        if (e.alive && e is Husk h) al.Add($"{h.kind}{(h is Warden w ? "/" + w.state + (w.ShieldDown ? "/open" : "") : "")} hp {h.hp:0} d {JsMath.Hypot(h.position.x - sim.mover.position.x, h.position.z - sim.mover.position.z):0.0} dy {h.position.y - sim.mover.position.y:0.0}");
                    Console.WriteLine($"    {sim.SimTime:0} s {traceRoom} phase {sim.campaign.phase} in hand {sim.loadout.Weapon} at ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0}) | {string.Join("; ", al)}");
                }
                if (sim.campaign != null && sim.campaign.phase == Campaign24.Phase.Fight)
                {
                    int n = 0;
                    foreach (Enemy e in sim.enemies.all) if (e.alive && e is Husk h && h.committed) n++;
                    fightSteps++;
                    if (n > 2) overTwo++;
                    if (n > maxCommitted) maxCommitted = n;
                }
                if (sim.campaign != null && sim.campaign.clearSteps.Count >= playable) break;
            }
            ap.Detach();
        }
        int bad = 0;
        for (int i = 0; i < playable; i++)
        {
            times[i].Sort();
            if (cleared[i] < seeds) bad++;
            Console.WriteLine($"  room {level.rooms[i].id,-4} {level.rooms[i].name,-18} cleared {cleared[i]}/{seeds}, median {(times[i].Count > 0 ? times[i][times[i].Count / 2].ToString("0.0", CultureInfo.InvariantCulture) + " s" : "-")}");
        }
        Console.WriteLine($"n40foes level {(with ? "Foes40Combat" : "control (the pilot alone)")}: {playable - bad}/{playable} rooms cleared on every seed; the most attackers committed at once {maxCommitted}, more than two for {(fightSteps > 0 ? 100.0 * overTwo / fightSteps : 0):0.0} % of the fight time");
        return bad == 0 ? 0 : 1;
    }

    /// <summary>`n40foes trace &lt;kind&gt; [--off40] [--level path] [--room id] [--seconds s]`: one enemy of the kind in the room's first wave, the player idle at the
    /// room's spawn: every 0.5 s its state, distance, grounded, the brain's attack / los, its cooldown (diagnosis).</summary>
    static int Foes40Trace(string[] args)
    {
        string kind = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : "charger";
        string path = Opt40F(args, "--level", "Design/levels/n37-iceline.json");
        double seconds = double.Parse(Opt40F(args, "--seconds", "14"), CultureInfo.InvariantCulture);
        bool off = Array.IndexOf(args, "--off40") >= 0;
        SurvivalSim sim = Foes40Sim(path, Opt40F(args, "--room", null), new List<object> { new List<object> { kind } }, off ? N40Blocks.Off : new N40Blocks { foes40 = true }, 1, out RoomDef room);
        var si = (ScriptedInput)sim.input;
        sim.Respawn(new Vec3(room.spawn.x, room.spawn.y + 0.05, room.spawn.z), room.spawnYawDeg);
        foreach (string ev in new[] { "chargerTell", "chargerCharge", "chargerStunned", "chargerBlocked", "wardenTell", "wardenSwing", "droneCharge" })
        {
            string name = ev;
            sim.events.On(ev, _ => Console.WriteLine($"    {sim.SimTime:0.00} s {name}"));
        }
        sim.events.On<HurtEvent>("hurt", e => { if (e.owner == "player") Console.WriteLine($"    {sim.SimTime:0.00} s hurt {e.amount:0} by {e.source}"); });
        int max = MovementMath.StepsFor(seconds, sim.step), every = MovementMath.StepsFor(0.5, sim.step);
        for (int s = 0; s < max; s++)
        {
            sim.health.hp = sim.health.max;
            si.Set(new InputFrame { yaw = room.spawnYawDeg * Math.PI / 180 }, default);
            sim.Step();
            sim.hitstopLeft = 0;
            if (s % every != 0) continue;
            List<Husk> l = sim.husks.PoolOf(kind);
            Husk h = l.Count > 0 ? l[0] : null;
            if (h == null) { Console.WriteLine($"  {sim.SimTime:0.00} s: none"); continue; }
            double d = JsMath.Hypot(h.position.x - sim.mover.position.x, h.position.z - sim.mover.position.z);
            Console.WriteLine($"  {sim.SimTime:0.00} s: alive {h.alive} state {(h is Charger hc ? hc.state + " " + hc.stateSteps : h is Warden hw ? hw.state + " " + hw.stateSteps : "-")} d {d:0.0} at ({h.position.x:0.0},{h.position.y:0.0},{h.position.z:0.0}) grounded {h.grounded} cd {h.cooldownSteps} committed {h.committed} "
                + $"ai {(h.ai == null ? "none" : $"attack {h.ai.attack} los {h.ai.los} blocked {h.ai.blocked}")} v ({h.velocity.x:0.0},{h.velocity.z:0.0}) player ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0})");
        }
        return 0;
    }
}
