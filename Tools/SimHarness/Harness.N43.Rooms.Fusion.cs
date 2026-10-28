// NIGHT43 builder E1 round 2: `n43rooms fusion [--seeds n]` (G19's gun-again room as a real beat). n43-iceline with rooms43, iceline43 and W1's
// fusion43 forced on: a campaign run, DOORFREE into g6 THE ARMOURY, the player steps in (the commit gate) and kills the KESTREL's two husks
// (lesson 0), then stands at the near end of the dash lane (g6's waveNodes43 spot's x, 2 m in) facing down it with the PULSE gauntlet in hand.
// The FUSION husk (wave 1, afterLesson43) spawns at the lane's far end and walks at the player; when it is within D m the scripted person
// dashes at it (stick forward) and presses fire 0.1 s later. Per D (4-7 m) and seed: the husk's spawn distance, a hit, a FUSION
// (N43Events.Fusion). PASS: a FUSION in most tries at D 5-7 m and the husk's spawn >= 8 m down the lane.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static int N43RmFusion(string[] args)
    {
        GameConfig cfg = Config();
        int seeds = int.Parse(N43Arg(args, "--seeds", "3"), CultureInfo.InvariantCulture);
        string path = Path.GetFullPath(Path.Combine("Design", "levels", cfg.Str("design.n43.iceline43.levelTo") + ".json"));
        double[] ds = { 7, 6, 5, 4 };
        int tries = 0, fusions = 0, near = 0, mid = 0, midFusions = 0;
        double minSpawn = double.MaxValue;
        Console.WriteLine($"n43rooms fusion: {Path.GetFileNameWithoutExtension(path)} g6, rooms43 + iceline43 + fusion43 forced on; dash at the husk when it is D m away, fire 0.1 s after the dash");
        foreach (double D in ds)
        {
            var line = new List<string>();
            for (int s = 1; s <= seeds; s++)
            {
                var (spawnD, hit, fusion, why) = N43RmFusionTry(cfg, path, (uint)s, D);
                tries++;
                if (fusion) fusions++;
                if (D >= 5) { mid++; if (fusion) midFusions++; }
                if (spawnD > 0) minSpawn = Math.Min(minSpawn, spawnD);
                line.Add($"seed {s}: spawn {spawnD:0.0} m, {(fusion ? "FUSION" : hit ? "hit" : "miss")}{(why != null ? " (" + why + ")" : "")}");
            }
            Console.WriteLine($"  D {D:0} m: {string.Join(" | ", line)}");
        }
        bool ok = mid > 0 && midFusions * 3 >= mid * 2 && minSpawn >= 8;
        Console.WriteLine($"n43rooms fusion SUMMARY: FUSION {fusions}/{tries} (D 5-7 m: {midFusions}/{mid}); the husk spawned >= {minSpawn:0.0} m down the lane: {(ok ? "PASS" : "RED")}");
        return ok ? 0 : 1;
    }

    /// <summary>docs/ROOMS.md's roles (E-1).</summary>
    static string N43Role(string kind) => kind == "husk" || kind == "swarmer" ? "fodder" : kind == "spitter" ? "ranged" : kind == "drone" ? "flyer"
        : kind == "charger" ? "charger" : kind == "warden" ? "shield" : kind == "tank" || kind == "heavy" ? "heavy" : kind;

    /// <summary>`n43rooms roles [--levels iceline]`: DESIGN-RULES R-3 per room of n40-iceline and n43-iceline: the roles in the room's roster, per
    /// wave, and the most roles ALIVE AT ONCE in a slow fight (an invulnerable player at the exit side kills one enemy every 1.5 s); the kinds
    /// in the level before / after (none lost). PASS: n43's rooms <= 3 roles before level 4 and every n40 kind still in the level.</summary>
    static int N43RmRoles(string[] args)
    {
        GameConfig cfg = Config();
        string lvName = N43Arg(args, "--level", "iceline");
        int bad = 0;
        var kindsBy = new List<HashSet<string>>();
        for (int k = 0; k < 2; k++)
        {
            bool n43 = k == 1;
            string id = N43RmLevelId(cfg, lvName, n43);
            string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
            LevelDef lv = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
            var kinds = new HashSet<string>();
            var atOnce = N43RmRolesAtOnce(cfg, path, n43);
            Console.WriteLine($"  {id}:");
            foreach (RoomDef r in lv.rooms)
            {
                if (r.WaveCount == 0) continue;
                var roles = new SortedSet<string>();
                var perWave = new List<string>();
                foreach (List<string> w in r.waves)
                {
                    var wr = new SortedSet<string>();
                    foreach (string kd in w) { kinds.Add(kd); roles.Add(N43Role(kd)); wr.Add(N43Role(kd)); }
                    perWave.Add(string.Join("+", wr));
                }
                atOnce.TryGetValue(r.id, out int once);
                bool over = roles.Count > 3;
                if (n43 && over) bad++;
                Console.WriteLine($"      {r.id,-4} roles {roles.Count} ({string.Join(", ", roles)}){(over ? " OVER R-3" : "")}; waves [{string.Join("] [", perWave)}]; most alive at once {once}");
            }
            kindsBy.Add(kinds);
            Console.WriteLine($"      kinds in the level: {string.Join(", ", new SortedSet<string>(kinds))}");
        }
        var lost = new SortedSet<string>(kindsBy[0]);
        lost.ExceptWith(kindsBy[1]);
        if (lost.Count > 0) bad++;
        Console.WriteLine($"n43rooms roles SUMMARY: kinds lost {(lost.Count == 0 ? "none" : string.Join(", ", lost))}; {(bad == 0 ? "PASS" : "RED")}");
        return bad == 0 ? 0 : 1;
    }

    static Dictionary<string, int> N43RmRolesAtOnce(GameConfig cfg, string path, bool n43Levels)
    {
        var most = new Dictionary<string, int>();
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        sim.n43Override = N43RmBlocks(cfg, true, n43Levels);
        sim.StartRun(1, null, null);
        LevelDef level = sim.campaignLevel;
        int placed = -2, forced = -1, lastKill = 0, enter = 0, st = 0;
        sim.events.On<RoomEvent>("roomEnter", _ => enter = st);
        int total = MovementMath.StepsFor(150 * level.rooms.Count, sim.step);
        for (; st < total; st++)
        {
            Campaign24 c = sim.campaign;
            if (c != null && c.room >= 0)
            {
                RoomDef r = level.rooms[c.room];
                if (r.kind == "boss") break;
                if (c.phase == Campaign24.Phase.Cleared && c.room + 1 < level.rooms.Count)
                {
                    if (level.rooms[c.room + 1].kind == "boss") break;
                    sim.Respawn(level.rooms[c.room + 1].spawn, level.rooms[c.room + 1].spawnYawDeg);
                }
                else if (c.phase == Campaign24.Phase.Fight)
                {
                    if (placed != c.room) { placed = c.room; Vec3 at = r.exit; if (sim.zones.ZoneAt(new Vec3(at.x, at.y + 1, at.z)) != r.id) at = r.spawn; sim.Respawn(at, r.spawnYawDeg); }
                    if (r.WaveCount == 0 && c.lesson != null && !c.lesson.Done && forced != c.room && (st - enter) * sim.step > 1) { forced = c.room; c.lesson.index = c.lesson.defs.Count; }
                    var roles = new HashSet<string>();
                    Husk first = null;
                    foreach (Enemy e in sim.enemies.all)
                        if (e.alive && !e.standIn29 && e is Husk h) { roles.Add(N43Role(h.kind == "husk" && h.tank29 ? "tank" : h.kind)); first ??= h; }
                    most.TryGetValue(r.id, out int m0);
                    if (roles.Count > m0) most[r.id] = roles.Count;
                    if (first != null && st - lastKill >= MovementMath.StepsFor(1.5, sim.step)) { sim.loadout.Damage(first, first.hp + 1e5, false, "revolver", sim.mover.position, shot: 0); lastKill = st; }
                    if ((st - enter) * sim.step > 140) break;
                }
            }
            sim.health.hp = sim.health.max;
            if (sim.flow == Skygrind.Game.Flow.Down) sim.flow = Skygrind.Game.Flow.Playing;
            sim.Step();
            if (sim.flow == Skygrind.Game.Flow.Results) break;
        }
        return most;
    }

    static (double spawnD, bool hit, bool fusion, string why) N43RmFusionTry(GameConfig cfg, string path, uint seed, double D)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        int bits = N43RmBlocks(cfg, true, true).Bits | 1 << Array.IndexOf(N43Blocks.Names, "fusion43");
        sim.n43Override = N43Blocks.FromBits(bits);
        bool hit = false, fusion = false;
        sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "gauntlet") hit = true; });
        sim.events.On<Fusion43Event>(N43Events.Fusion, e => fusion = true);
        sim.StartRun(seed, null, null);
        LevelDef lv = sim.campaignLevel;
        int gi = lv.rooms.FindIndex(q => q.id == "g6");
        RoomDef r = lv.rooms[gi];
        var inp = (ScriptedInput)sim.input;
        void Tick(double yaw, double pitch, InputFrame f, CombatInput c) { sim.health.hp = sim.health.max; inp.SetLook(yaw, pitch); inp.Set(f, c); sim.Step(); }
        var idle = new CombatInput { weapon = -1 };
        for (int k = 0; k < 120 && sim.campaign == null; k++) Tick(0, 0, default, idle);
        sim.campaign.SetDoorFree29(true);
        sim.Respawn(r.spawn, r.spawnYawDeg);
        for (int k = 0; k < 2400 && sim.campaign.room != gi; k++) Tick(0, 0, default, idle);
        sim.campaign.SetDoorFree29(false);
        double yaw0 = r.spawnYawDeg * Math.PI / 180;
        sim.Respawn(r.spawn + new Vec3(-Math.Sin(yaw0) * 4, 0, -Math.Cos(yaw0) * 4), r.spawnYawDeg);
        // the KESTREL's two husks: killed as they appear (lesson 0)
        var kestrel = new HashSet<int>(); // by id: a dead body is reused by the pool for the next spawn
        for (int k = 0; k < 2400 && kestrel.Count < 2; k++)
        {
            foreach (Enemy e in sim.enemies.all)
                if (e.alive && !e.standIn29 && e is Husk h && kestrel.Add(e.id)) sim.loadout.Damage(h, h.hp + 1e5, false, "revolver", sim.mover.position, shot: 0);
            Tick(yaw0, 0, default, idle);
        }
        List<Vec3> spots = r.WaveNodes43(1);
        if (spots == null) return (-1, false, false, "g6 has no waveNodes43");
        Vec3 far = spots[0];
        var start = new Vec3(far.x, r.spawn.y, r.z1 - 2);
        double laneYaw = Math.Atan2(-(far.x - start.x), -(far.z - start.z));
        sim.Respawn(start, laneYaw * 180 / Math.PI);
        int slot = sim.loadout.owned.IndexOf("gauntlet");
        if (slot < 0) return (-1, false, false, "no gauntlet owned");
        Tick(laneYaw, 0, default, new CombatInput { weapon = slot });
        Husk target = null;
        double spawnD = -1;
        for (int k = 0; k < 2400 && target == null; k++)
        {
            foreach (Enemy e in sim.enemies.all)
                if (e.alive && !e.standIn29 && !kestrel.Contains(e.id) && e is Husk h) { target = h; break; }
            Tick(laneYaw, 0, default, idle);
        }
        if (target == null) return (-1, false, false, $"the FUSION husk never came: room {sim.campaign.Room?.id} {sim.campaign.phase}, wave {sim.campaign.Wave33}, lesson {(sim.campaign.lesson == null ? "none" : sim.campaign.lesson.index + "/" + sim.campaign.lesson.defs.Count)}, KESTREL husks seen {kestrel.Count}, alive {sim.enemies.AliveCount}");
        Vec3 p = sim.mover.position;
        spawnD = Math.Sqrt((target.position.x - p.x) * (target.position.x - p.x) + (target.position.z - p.z) * (target.position.z - p.z));
        target.hp = target.maxHp = 1e6; // the damage is read, it does not die mid-test
        // wait for it to come within D m (it walks down the lane)
        int w = 0;
        for (; w < 1200; w++)
        {
            p = sim.mover.position;
            double d = Math.Sqrt((target.position.x - p.x) * (target.position.x - p.x) + (target.position.z - p.z) * (target.position.z - p.z));
            if (d <= D) break;
            (double yy, double pp) = ArclineLook(sim.Eye, target.AimPoint);
            Tick(yy, pp, default, idle);
        }
        int press = MovementMath.StepsFor(0.1, sim.step);
        for (int s = 0; s < 90 && !fusion; s++)
        {
            (double yy, double pp) = ArclineLook(sim.Eye, target.AimPoint);
            var f = new InputFrame { yaw = yy, pitch = pp, moveZ = s < 40 ? 1 : 0, dash = new Button { pressed = s == 0, held = s < 3 } };
            var c = new CombatInput { weapon = -1 };
            if (s == press) c.fire = new Button { pressed = true, held = true };
            Tick(yy, pp, f, c);
        }
        return (spawnD, hit, fusion, w >= 1200 ? "never came within D" : null);
    }
}
