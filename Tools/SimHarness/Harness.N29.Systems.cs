// NIGHT29 (builder S): the systems rows' headless judges. ONE entry method, N29Systems(cmd, args, ref code); the lead adds its call line to Program.cs.
//   lockout29 [--off] [--steps n]   contract lockout (item 8): 10,000 random input steps (the trigger held in runs, melee presses, weapon asks) on the
//                                   revolver, shotgun, nailgun and rocket, then the gauntlet; counts OVERLAPS: a punch landing inside a gun's own cycle, a shot
//                                   inside a melee swing's cycle, a switch run inside either, a separate melee with the gauntlet in hand. Cycle lengths are
//                                   MEASURED (the shortest gap of each gun's shots with the trigger held alone, the shortest gap of punch presses alone).
//                                   `--off` = design.n29.lockout off = NIGHT28's game: the RED side (the old game overlaps).
//   shotgun29 [--off]               contract shotgun (item 10): the THUNDERCLAP's effective shot-to-shot time before (block off) and after (block on), the
//                                   pump delay unchanged; PASS when after = before x design.n29.shotgun.fireIntervalMult within a step.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void N29Systems(string cmd, string[] args, ref int code)
    {
        if (cmd == "lockout29") Lockout29Command(args, ref code);
        else if (cmd == "shotgun29") Shotgun29Command(args, ref code);
        else if (cmd == "tank29") Tank29Command(args, ref code);
        else if (cmd == "doorfree29") DoorFree29Command(args, ref code);
        else if (cmd == "tape29") Tape29Command(args, ref code);
        else if (cmd == "pickups29") Pickups29Command(args, ref code);
        else if (cmd == "standins29") StandIns29Command(args, ref code);
        else if (cmd == "fall29") Fall29Command(args, ref code);
        else if (cmd == "systems29")
        {
            // the pure rules' tests (Assets/Tests/EditMode/Systems29Tests.cs) through the harness's NUnit stand-in; the file is compiled in with
            // `-p:Extra=../../Assets/Tests/EditMode/Systems29Tests.cs`
            Type t = Type.GetType("Skygrind.Core.Tests.Systems29Tests");
            if (t == null) { Console.WriteLine("systems29: the test class is not compiled in (pass -p:Extra=../../Assets/Tests/EditMode/Systems29Tests.cs)"); code = 2; return; }
            code = NUnit.Framework.MiniRunner.Run(t, Console.WriteLine) == 0 ? 0 : 1;
        }
    }

    static SurvivalSim Sim29(GameConfig cfg, SurvivalMap map, N29Blocks n29, uint seed)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n29Override = n29;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(seed, null, null);
        sim.mover.invulnerable = true;
        return sim;
    }

    // ---- lockout29 -------------------------------------------------------------------------------------------------------------------------

    /// <summary>One gun's measured cycle: the shortest gap (steps) between two shots with the trigger held every step, the bag in front.</summary>
    static int GunGap29(GameConfig cfg, SurvivalMap map, string weapon)
    {
        SurvivalSim sim = Sim29(cfg, map, N29Blocks.Off, 2901);
        if (!sim.loadout.owned.Contains(weapon)) sim.loadout.AddWeapon(weapon);
        int slot = sim.loadout.owned.IndexOf(weapon);
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Husk h = Bag25(sim, map.playerSpawn, ux, uz, 1.6);
        int step = 0, last = -1, min = int.MaxValue;
        sim.events.On("fire", o => { if (o as string != weapon) return; if (last >= 0) min = Math.Min(min, step - last); last = step; });
        for (int s = 0; s < 1200; s++)
        {
            step = s;
            Pin25(h, map.playerSpawn, ux, uz, 1.6);
            double py = 0;
            if (h != null && h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
            Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = true, pressed = s == 0 } });
        }
        return min == int.MaxValue ? 0 : min;
    }

    /// <summary>The melee swing's measured cycle: the shortest gap between "punch" events with the press made every step (the revolver in hand).</summary>
    static int PunchGap29(GameConfig cfg, SurvivalMap map)
    {
        SurvivalSim sim = Sim29(cfg, map, N29Blocks.Off, 2902);
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Husk h = Bag25(sim, map.playerSpawn, ux, uz, 1.6);
        int step = 0, last = -1, min = int.MaxValue;
        sim.events.On<MeleeEvent>("punch", e => { if (last >= 0) min = Math.Min(min, step - last); last = step; });
        for (int s = 0; s < 600; s++)
        {
            step = s;
            Pin25(h, map.playerSpawn, ux, uz, 1.6);
            double py = 0;
            if (h != null && h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
            Tick27(sim, yaw, py, new CombatInput { weapon = -1, punch = new Button { held = true, pressed = true } });
        }
        return min == int.MaxValue ? 0 : min;
    }

    static void Lockout29Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        int total = 10000;
        for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "--steps") total = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        string[] guns = { "revolver", "shotgun", "nailgun", "rocket", "gauntlet" };
        var gap = new Dictionary<string, int>();
        foreach (string g in guns) gap[g] = GunGap29(cfg, map, g);
        int punchGap = PunchGap29(cfg, map);
        Console.WriteLine("lockout29 measured cycles (steps at 120 Hz): " + string.Join(", ", Array.ConvertAll(guns, g => g + " " + gap[g])) + ", punch " + punchGap);

        SurvivalSim sim = Sim29(cfg, map, off ? N29Blocks.Off : new N29Blocks { lockout = true }, 2903);
        foreach (string g in guns) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Husk h = Bag25(sim, map.playerSpawn, ux, uz, 1.6);
        var rng = new Mulberry32(2929);
        int step = 0, lastFire = -100000, lastPunch = -100000, shots = 0, punches = 0, swaps = 0, swingsG = 0;
        string lastFireWeapon = "revolver", weaponBefore = "revolver";
        int ovPunchInGun = 0, ovFireInMelee = 0, ovSwitchInCycle = 0, ovGauntletMelee = 0;
        sim.events.On("fire", o =>
        {
            string w = o as string ?? "";
            shots++;
            if (step - lastPunch < punchGap) ovFireInMelee++;
            lastFire = step;
            lastFireWeapon = w;
        });
        sim.events.On<MeleeEvent>("punch", e =>
        {
            punches++;
            if (gap.TryGetValue(lastFireWeapon, out int gg) && gg > 0 && step - lastFire < gg) ovPunchInGun++;
            if (weaponBefore == "gauntlet" && sim.loadout.Weapon == "gauntlet") ovGauntletMelee++;
            lastPunch = step;
        });
        sim.events.On<MeleeEvent>("meleeSwing", e => swingsG++);
        sim.events.On("weaponSwap", o =>
        {
            swaps++;
            if (gap.TryGetValue(lastFireWeapon, out int gg) && gg > 0 && step - lastFire < gg) ovSwitchInCycle++;
            if (step - lastPunch < punchGap) ovSwitchInCycle++;
        });
        bool fireHeld = false;
        for (step = 0; step < total; step++)
        {
            double py = 0;
            Pin25(h, map.playerSpawn, ux, uz, 1.6);
            if (h != null && h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
            if (rng.Next() < 0.06) fireHeld = !fireHeld; // the trigger is held in runs of about 16 steps, released in runs of about 16
            bool punch = rng.Next() < 0.07;
            int want = rng.Next() < 0.012 ? (int)(rng.Next() * sim.loadout.owned.Count) : -1;
            var c = new CombatInput { weapon = want };
            if (fireHeld) c.fire = new Button { held = true, pressed = rng.Next() < 0.5 };
            if (punch) c.punch = new Button { held = true, pressed = true };
            weaponBefore = sim.loadout.Weapon;
            Tick27(sim, yaw, py, c);
        }
        int overlaps = ovPunchInGun + ovFireInMelee + ovSwitchInCycle + ovGauntletMelee;
        string drops = sim.lockout29 != null && !off ? $"; dropped {sim.lockout29.droppedMelee} melee presses, {sim.lockout29.droppedFireSteps} trigger steps, {sim.lockout29.waitedSwitches} switches waited" : "";
        bool alive = shots > 50 && punches > 30 && swaps > 20;
        Console.WriteLine($"lockout29 {(off ? "OFF (NIGHT28's game)" : "ON")}: {total} random input steps: {shots} shots, {punches} punches, {swingsG} gauntlet swings, {swaps} switches{drops}; overlaps {overlaps} (punch inside a gun cycle {ovPunchInGun}, shot inside a melee cycle {ovFireInMelee}, switch inside a cycle {ovSwitchInCycle}, melee with the gauntlet in hand {ovGauntletMelee})");
        bool pass = off ? overlaps > 0 : overlaps == 0 && alive;
        Console.WriteLine($"lockout29 {(off ? (pass ? "RED (as expected: the old game overlaps)" : "NOT RED: the old game showed no overlap, the judge is blind") : (pass ? "GREEN" : "FAIL"))}{(alive ? "" : " (the run did too little: shots/punches/switches below the floor)")}");
        code = off ? (pass ? 1 : 2) : (pass ? 0 : 1); // --off exits 1 = RED seen
    }

    // ---- round 2: pickups29 / standins29 / fall29 ------------------------------------------------------------------------------------------

    /// <summary>A CAMPAIGN run on a built n29 level (Design/levels/n29/*.json), the first 2 s played; `n29` the blocks; DOORFREE on when asked (to walk into any room).</summary>
    static SurvivalSim LevelSim29(GameConfig cfg, string path, N29Blocks n29, out LevelDef level)
    {
        SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
        level = LevelDef.From(json);
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n24Override = new N24Blocks { campaign = true };
        sim.n25Override = new N25Blocks { tutorial = true };
        sim.n29Override = n29;
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(2907, null, null);
        for (int s = 0; s < 240; s++) { sim.health.hp = sim.health.max; sim.Step(); }
        return sim;
    }

    static void Go29(SurvivalSim sim, RoomDef r)
    {
        sim.Respawn(r.spawn, r.spawnYawDeg);
        sim.mover.invulnerable = true;
    }

    static void Pickups29Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        GameConfig cfg = Config();
        SurvivalSim sim = LevelSim29(cfg, "Design/levels/n29/kiln.json", new N29Blocks { pickups = !off, doorFree = true }, out LevelDef level);
        sim.zones.OpenAll();
        PickupDef gun = level.pickups.Find(p => p.kind == "weaponPickup"), med = level.pickups.Find(p => p.kind == "healthPickup");
        int earned = 0, taken = 0;
        sim.events.On<WeaponEarnedEvent>("weaponEarned", e => earned++);
        sim.events.On<PickupEvent29>("pickup29", e => taken++);
        bool hadBefore = sim.loadout.owned.Contains("shotgun");
        // the weapon: stand on it (its floor is 1 m under its centre), many steps
        sim.Respawn(new Vec3(gun.at.x, gun.at.y - 1, gun.at.z), 0);
        for (int s = 0; s < 360; s++) { sim.mover.invulnerable = true; sim.health.hp = sim.health.max; sim.Step(); }
        bool gotGun = sim.loadout.owned.Contains("shotgun");
        int earnedAfterGun = earned;
        // 200 more steps standing there and a walk off and on: not twice
        sim.Respawn(new Vec3(gun.at.x + 6, gun.at.y - 1, gun.at.z), 0);
        for (int s = 0; s < 60; s++) sim.Step();
        sim.Respawn(new Vec3(gun.at.x, gun.at.y - 1, gun.at.z), 0);
        for (int s = 0; s < 120; s++) sim.Step();
        // the heal: half health, stand on the medkit
        sim.health.hp = sim.health.max * 0.5;
        double hp0 = sim.health.hp;
        sim.Respawn(new Vec3(med.at.x, med.at.y - 0.9, med.at.z), 0);
        sim.Step();
        double hp1 = sim.health.hp;
        sim.health.hp = sim.health.max * 0.5;
        for (int s = 0; s < 120; s++) { sim.health.hp = Math.Min(sim.health.hp, sim.health.max * 0.5); sim.Step(); }
        double hp2 = sim.health.hp;
        Console.WriteLine($"pickups29 {(off ? "OFF" : "ON")}: shotgun before {hadBefore}, after standing on the THUNDERCLAP prop {gotGun} (weaponEarned x{earnedAfterGun}, after walking off and on x{earned}); heal: {hp0:0} -> {hp1:0} of {sim.health.max:0} (once), standing again {hp2:0}; pickup29 events {taken}; taken {sim.campaign?.takenPickups.Count}");
        bool ok = !hadBefore && gotGun && earnedAfterGun == 1 && earned == 1 && hp1 > hp0 + 1 && hp2 <= sim.health.max * 0.5 + 1e-9 && taken == 2;
        if (off) { bool red = !gotGun && hp1 <= hp0 + 1e-9; Console.WriteLine(red ? "pickups29 RED (as expected: off, standing on them gives nothing)" : "pickups29 NOT RED"); code = red ? 1 : 2; }
        else { Console.WriteLine(ok ? "pickups29 GREEN: the shotgun once, the heal once" : "pickups29 FAIL"); code = ok ? 0 : 1; }
    }

    static void StandIns29Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        GameConfig cfg = Config();
        SurvivalSim sim = LevelSim29(cfg, "Design/levels/n29/foundry.json", new N29Blocks { roomRules = !off, doorFree = true }, out LevelDef level);
        for (int s = 0; s < 360; s++) { sim.doorFreeHeld = true; sim.Step(); }
        sim.doorFreeHeld = false;
        int f5 = level.rooms.FindIndex(r => r.id == "f5");
        Go29(sim, level.rooms[f5]);
        for (int s = 0; s < 30; s++) sim.Step();
        var bodies = new List<Husk>();
        foreach (Husk h in sim.husks.PoolOf("husk")) if (h.alive && h.standIn29) bodies.Add(h);
        int n = bodies.Count;
        double hpS = n > 0 ? bodies[0].hp : 0;
        // not moving by themselves: the static one's position over 2 s; the strafer's z range over 6 s
        Vec3 p0 = n > 0 ? bodies[0].position : default;
        double zMin = double.MaxValue, zMax = double.MinValue;
        for (int s = 0; s < 720; s++)
        {
            sim.Step();
            if (n > 1) { double z = bodies[1].position.z; zMin = Math.Min(zMin, z); zMax = Math.Max(zMax, z); }
        }
        bool stayed = n > 0 && Math.Abs(bodies[0].position.x - p0.x) < 1e-9 && Math.Abs(bodies[0].position.z - p0.z) < 1e-9;
        // a shot from the floor does nothing; from the wall it lands
        bool floorHit = false, wallHit = false;
        if (n > 0)
        {
            sim.mover.state = MoveState.Ground;
            sim.loadout.Damage(bodies[0], 25, false, "revolver", sim.Eye); floorHit = bodies[0].hp < hpS;
            double afterFloor = bodies[0].hp;
            sim.mover.state = MoveState.WallRide;
            sim.loadout.Damage(bodies[0], 25, false, "revolver", sim.Eye); wallHit = bodies[0].hp < afterFloor;
            Console.WriteLine($"standins29 {(off ? "OFF" : "ON")}: {n} stand-ins in f5 ({bodies.Count} bodies), hp {hpS:0}; static one moved: {!stayed}; strafer z {(n > 1 ? zMin.ToString("0.0") + " .. " + zMax.ToString("0.0") : "n/a")}; floor shot landed {floorHit} (hp {afterFloor:0}); wall-ride shot landed {wallHit} (hp {bodies[0].hp:0})");
        }
        else Console.WriteLine($"standins29 {(off ? "OFF" : "ON")}: {n} stand-ins in f5");
        bool ok = n == 2 && stayed && zMax - zMin > 4 && !floorHit && wallHit;
        if (off) { Console.WriteLine(n == 0 ? "standins29 RED (as expected: off, no stand-ins)" : "standins29 NOT RED"); code = n == 0 ? 1 : 2; }
        else { Console.WriteLine(ok ? "standins29 GREEN" : "standins29 FAIL"); code = ok ? 0 : 1; }
    }

    static void Fall29Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        GameConfig cfg = Config();
        SurvivalSim sim = LevelSim29(cfg, "Design/levels/n29/foundry.json", new N29Blocks { roomRules = !off, doorFree = true }, out LevelDef level);
        for (int s = 0; s < 360; s++) { sim.doorFreeHeld = true; sim.Step(); }
        sim.doorFreeHeld = false;
        int f2 = level.rooms.FindIndex(r => r.id == "f2");
        RoomDef r2 = level.rooms[f2];
        Go29(sim, r2);
        for (int s = 0; s < 30; s++) sim.Step();
        sim.mover.invulnerable = false;
        sim.health.hp = sim.health.max;
        int before = sim.campaign.stageRespawns;
        sim.Respawn(new Vec3(r2.spawn.x, r2.respawnY - 2, r2.spawn.z), 0);
        for (int s = 0; s < 6; s++) sim.Step();
        double hp = sim.health.hp, max = sim.health.max;
        int respawns = sim.campaign.stageRespawns - before;
        Console.WriteLine($"fall29 {(off ? "OFF" : "ON")}: a fall below respawnY {r2.respawnY} in {r2.id}: stage respawns {respawns}, health {hp:0} of {max:0} ({100 * hp / max:0} %)");
        bool cost = Math.Abs(hp - max * 0.75) < 1e-6;
        if (off) { Console.WriteLine(respawns == 1 && hp >= max - 1e-9 ? "fall29 RED (as expected: off, the fall is free)" : "fall29 NOT RED"); code = respawns == 1 && hp >= max - 1e-9 ? 1 : 2; }
        else { Console.WriteLine(respawns == 1 && cost ? "fall29 GREEN: respawned, 25 % health lost" : "fall29 FAIL"); code = respawns == 1 && cost ? 0 : 1; }
    }

    // ---- tape29 ----------------------------------------------------------------------------------------------------------------------------

    /// <summary>`tape29 [--off] [--steps n]`: a taped run with my four blocks ON (lockout, shotgun, tank, doorFree; the shotgun in hand part of the time, fire HELD and
    /// punch presses all through so the lockout drops things), replayed from the tape alone on a fresh composition: EQUAL, and the tape carries the bits (format 12).
    /// `--off`: every NIGHT29 block off (NIGHT28's game): the tape's byte is 0.</summary>
    static void Tape29Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        int steps = 3600;
        for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "--steps") steps = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.n29Override = off ? N29Blocks.Off : new N29Blocks { lockout = true, shotgun = true, tank = true, doorFree = true };
        var si = new ScriptedInput();
        sim.UseInput(si);
        RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, steps, "tape29", (sm, i) =>
        {
            si.Set(new InputFrame { moveX = (i / 120) % 2 == 0 ? 1 : -1, moveZ = 1, jump = new Button { pressed = i % 180 == 0, held = i % 180 < 10 } },
                new CombatInput { fire = new Button { held = true, pressed = i % 40 == 0 }, punch = new Button { held = i % 17 == 0, pressed = i % 17 == 0 }, weapon = i % 300 == 150 ? (i / 300) % 2 : -1 });
        }, TapeScript.God, false);
        Console.WriteLine($"tape29: {tape.steps} steps taped with {sim.n29} (tape bits {tape.n29}); kills {sim.run.stats.kills}, shots {sim.run.stats.shots}; lockout dropped {(sim.lockout29 != null ? sim.lockout29.droppedMelee + " melee presses, " + sim.lockout29.droppedFireSteps + " trigger steps, " + sim.lockout29.waitedSwitches + " switches waited" : "nothing (off)")}");
        TapeResult r = TapeReplay.Replay(Config(), Map(), tape);
        Console.WriteLine("tape29 replay: " + r.Line);
        bool bitsOk = off ? tape.n29 == 0 : tape.n29 == (1 | 2 | 4 | 32);
        Console.WriteLine($"tape29 {(r.Equal && bitsOk ? "PASS" : "FAIL")}: replay {(r.Equal ? "EQUAL" : "DIFFERS")}, tape bits {tape.n29} ({(bitsOk ? "as set" : "WRONG")})");
        code = r.Equal && bitsOk ? 0 : 1;
    }

    // ---- doorfree29 ------------------------------------------------------------------------------------------------------------------------

    /// <summary>A CAMPAIGN run on the tutorial plan (the old one; builder R's plans are not in this tree) with the scripted input, the first stage entered.</summary>
    static SurvivalSim DoorFreeSim29(GameConfig cfg, bool on, out LevelDef level)
    {
        SurvivalMap map = CampaignMapFrom("Design/levels/tutorial.plan.json", cfg, out var json);
        level = LevelDef.From(json);
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n24Override = new N24Blocks { campaign = true };
        sim.n25Override = new N25Blocks { tutorial = true };
        sim.n29Override = on ? new N29Blocks { doorFree = true } : N29Blocks.Off;
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(2906, null, null);
        for (int s = 0; s < 240; s++) { sim.health.hp = sim.health.max; sim.Step(); }
        return sim;
    }

    static int LockedGates29(SurvivalSim sim, LevelDef level, bool doors)
    {
        int n = 0;
        if (doors) foreach (DoorDef d in level.doors) { int z = sim.map.ZoneIndex(d.id); if (z >= 0 && sim.zones.IsLocked(z)) n++; }
        else foreach (RoomDef r in level.rooms) { int z = sim.map.ZoneIndex(r.id); if (z >= 0 && sim.zones.IsLocked(z)) n++; }
        return n;
    }

    /// <summary>Steps with the bumper held for `steps` fixed steps (the player topped up and standing still).</summary>
    static void HoldRb29(SurvivalSim sim, int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            sim.doorFreeHeld = true;
            sim.health.hp = sim.health.max;
            sim.Step();
        }
        sim.doorFreeHeld = false;
        sim.health.hp = sim.health.max;
        sim.Step();
    }

    static void DoorFree29Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        GameConfig cfg = Config();
        int hold = MovementMath.StepsFor(cfg.Num("design.n29.doorFree.holdSeconds"), 1.0 / cfg.movement.loop.simHz);
        SurvivalSim sim = DoorFreeSim29(cfg, !off, out LevelDef level);
        int doorsLocked0 = LockedGates29(sim, level, true), roomsLocked0 = LockedGates29(sim, level, false);
        // A. a hold one step short of the 3.0 s does nothing
        HoldRb29(sim, hold - 1);
        bool shortToggled = sim.DoorFreeOn;
        // B. the full hold flips it: every door and every room zone opens
        HoldRb29(sim, hold);
        bool on = sim.DoorFreeOn;
        int doorsLocked1 = LockedGates29(sim, level, true), roomsLocked1 = LockedGates29(sim, level, false);
        // C. a fight room entered from the first room spawns its wave
        int fightIdx = level.rooms.FindIndex(r => r.kind == "room" && r.WaveCount > 0);
        sim.Respawn(level.rooms[fightIdx].spawn, level.rooms[fightIdx].spawnYawDeg);
        sim.mover.invulnerable = true;
        int enemies = 0;
        for (int s = 0; s < 600 && enemies == 0; s++) { sim.Step(); enemies = sim.husks.Count; }
        int roomNow = sim.campaign != null ? sim.campaign.room : -1;
        // D. the boss room entered at once from here
        int bossIdx = level.rooms.FindIndex(r => r.kind == "boss");
        sim.Respawn(level.rooms[bossIdx].spawn, level.rooms[bossIdx].spawnYawDeg);
        for (int s = 0; s < 120; s++) sim.Step();
        int roomBoss = sim.campaign != null ? sim.campaign.room : -1;
        bool unranked = sim.Unranked29 && sim.campaign != null && sim.campaign.unranked29;
        // E. a second hold flips it off and the locks come back (except where the player stands)
        sim.Respawn(level.rooms[0].spawn, level.rooms[0].spawnYawDeg);
        HoldRb29(sim, hold);
        bool offAgain = !sim.DoorFreeOn;
        int doorsLocked2 = LockedGates29(sim, level, true);
        Console.WriteLine($"doorfree29 {(off ? "OFF (block off)" : "ON")}: hold {hold} steps ({cfg.Num("design.n29.doorFree.holdSeconds"):0.0} s); locked doors / rooms at the start {doorsLocked0}/{roomsLocked0}; after {hold - 1} steps toggled={shortToggled}; after {hold}: DOORS OPEN={on}, locked doors / rooms {doorsLocked1}/{roomsLocked1}; entered room {roomNow + 1} ('{level.rooms[fightIdx].id}' wanted {fightIdx + 1}) with {enemies} enemies spawned; boss room entered from the first area: room {roomBoss + 1} (wanted {bossIdx + 1}); unranked={unranked}; second hold: off={offAgain}, locked doors {doorsLocked2}");
        bool ok = !shortToggled && on && doorsLocked1 == 0 && roomsLocked1 == 0 && roomNow == fightIdx && enemies > 0 && roomBoss == bossIdx && unranked && offAgain && doorsLocked2 > 0;
        if (off) { Console.WriteLine(!on && doorsLocked1 == doorsLocked0 ? "doorfree29 RED (as expected: off, the hold opens nothing)" : "doorfree29 NOT RED: the block is off yet the doors opened"); code = !on && doorsLocked1 == doorsLocked0 ? 1 : 2; }
        else { Console.WriteLine(ok ? "doorfree29 GREEN" : "doorfree29 FAIL"); code = ok ? 0 : 1; }
    }

    // ---- tank29 ----------------------------------------------------------------------------------------------------------------------------

    /// <summary>One tank-or-husk body spawned as kind "tank", hit by `weapon` until it dies or `steps` pass: (isTank, hp at spawn, hits to kill (-1: alive), damage of the first hit).</summary>
    static (bool tank, double hp, int hits, double first) TankRun29(GameConfig cfg, SurvivalMap map, bool on, string weapon, int steps)
    {
        SurvivalSim sim = Sim29(cfg, map, on ? new N29Blocks { tank = true } : N29Blocks.Off, 2905);
        if (!sim.loadout.owned.Contains(weapon)) sim.loadout.AddWeapon(weapon);
        int slot = sim.loadout.owned.IndexOf(weapon);
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Vec3 p = map.playerSpawn;
        Husk h = sim.husks.Spawn("tank", new Vec3(p.x + ux * 1.6, p.y, p.z + uz * 1.6), p);
        if (h == null) return (false, 0, -1, 0);
        double hp = h.hp;
        int hits = 0;
        double first = 0;
        sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.damage > 0) { if (hits == 0) first = e.damage; hits++; } });
        int dead = -1;
        for (int s = 0; s < steps && dead < 0; s++)
        {
            double py = 0;
            if (h.alive) h.position = new Vec3(p.x + ux * 1.6, p.y, p.z + uz * 1.6);
            if (h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
            Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = true, pressed = s % 2 == 0 } });
            if (!h.alive) dead = hits;
        }
        return (h.tank29, hp, dead, first);
    }

    static void Tank29Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        var g = TankRun29(cfg, map, !off, "gauntlet", 600);
        var r = TankRun29(cfg, map, !off, "revolver", 120 * 90);
        double hpWant = cfg.Num("design.n29.tank.hp");
        Console.WriteLine($"tank29 {(off ? "OFF (kind 'tank' is a plain husk)" : "ON")}: spawned as tank {g.tank}, hp {g.hp}; gauntlet: {(g.hits < 0 ? "not dead in 5 s" : g.hits + " hit(s) to kill")}, first hit {g.first:0.#}; pistol: {(r.hits < 0 ? "not dead in 90 s" : r.hits + " shots to kill")}, first hit {r.first:0.##}");
        bool ok = g.tank && Math.Abs(g.hp - hpWant) < 1e-9 && g.hits == 1 && r.hits > 100;
        if (off) { Console.WriteLine(ok ? "tank29 NOT RED: the block is off yet a tank spawned" : "tank29 RED (as expected: off, no tank)"); code = ok ? 2 : 1; }
        else { Console.WriteLine(ok ? "tank29 GREEN: one gauntlet hit kills it, the pistol needs over a hundred" : "tank29 FAIL"); code = ok ? 0 : 1; }
    }

    // ---- shotgun29 -------------------------------------------------------------------------------------------------------------------------

    static (int steps, double mult29, double mult24, int shots) ShotgunGap29(GameConfig cfg, SurvivalMap map, bool on)
    {
        SurvivalSim sim = Sim29(cfg, map, on ? new N29Blocks { shotgun = true } : N29Blocks.Off, 2904);
        if (!sim.loadout.owned.Contains("shotgun")) sim.loadout.AddWeapon("shotgun");
        int slot = sim.loadout.owned.IndexOf("shotgun");
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Husk h = Bag25(sim, map.playerSpawn, ux, uz, 3);
        int step = 0;
        var at = new List<int>();
        sim.events.On("fire", o => { if (o as string == "shotgun") at.Add(step); });
        for (int s = 0; s < 120 * 12; s++)
        {
            step = s;
            Pin25(h, map.playerSpawn, ux, uz, 3);
            double py = 0;
            if (h != null && h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
            Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = true, pressed = true } });
        }
        var gaps = new List<int>();
        for (int i = 1; i < at.Count; i++) gaps.Add(at[i] - at[i - 1]);
        gaps.Sort();
        return (gaps.Count == 0 ? 0 : gaps[gaps.Count / 2], sim.loadout.sgIntervalMult29, sim.loadout.sgIntervalMult24, at.Count);
    }

    static void Shotgun29Command(string[] args, ref int code)
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        var before = ShotgunGap29(cfg, map, false);
        var after = ShotgunGap29(cfg, map, true);
        double want = cfg.Num("design.n29.shotgun.fireIntervalMult"), hz = cfg.movement.loop.simHz, pump = cfg.Num("weapons.shotgun.pumpDelay");
        Console.WriteLine($"shotgun29 BEFORE (block off): shot-to-shot {before.steps} steps = {(before.steps / hz).ToString("0.000", ci)} s ({before.shots} shots in 12 s; NIGHT24 interval x{before.mult24.ToString("0.###", ci)}, NIGHT29 x{before.mult29.ToString("0.###", ci)}); viewmodel pump {pump.ToString("0.00", ci)} s");
        Console.WriteLine($"shotgun29 AFTER  (block on) : shot-to-shot {after.steps} steps = {(after.steps / hz).ToString("0.000", ci)} s ({after.shots} shots; NIGHT24 interval x{after.mult24.ToString("0.###", ci)}, NIGHT29 x{after.mult29.ToString("0.###", ci)}); viewmodel pump {pump.ToString("0.00", ci)} s (unchanged: Combat.cs reads no pumpDelay)");
        bool ok = before.steps > 0 && Math.Abs(after.steps - before.steps * want) <= 1.5 && before.mult29 == 1;
        Console.WriteLine($"shotgun29 {(ok ? "PASS" : "RED")}: AFTER/BEFORE interval {(before.steps > 0 ? (after.steps / (double)before.steps).ToString("0.000", ci) : "n/a")} (want x{want.ToString("0.00", ci)} within a step and a half)");
        code = ok ? 0 : 1;
    }
}
