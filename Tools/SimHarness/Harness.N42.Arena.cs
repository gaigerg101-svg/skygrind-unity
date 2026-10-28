// NIGHT42 builder K1 (arena42; G12): the arena kit's checks. One entry, N42Arena (declared and called in Program.cs). Commands:
//   n42arena grinder [--off] [--seed n]
//     On the survival map (flat floor), a 4 x 4 m grinder (`grinders42`, injected into map.json before the run) 7.5 m out along an open line from
//     the player. Case KNOCK: a husk 1.6 m in front of the player, the grinder's near edge 0.6 m behind it; the player PUNCHES it (the real F press,
//     punch.knockbackSpeed / knockbackTime). Case WALK: a husk spawned beyond the grinder walks at the player (12 s): it must go round (the nav mask)
//     or be stopped at the rim (the edge guard), never ground. Case FALL-LESS: the knock case with arena42 forced off (--off: the RED run).
//     PASS (on): KNOCK ground Y, GROUND paid (style bonus 80, event arena42Ground knocked); WALK alive, ground 0. With --off: KNOCK alive.
//   n42arena cover [--off] [--seed n]
//     A cover box (`cover42`, 1.0 x 1.6 x 2.4 m, 6 hits) 5 m out on an open line, two husks standing 1 m behind it (held still). The player fires the
//     KESTREL at the box's face (the real fire press, one shot per cycle) until it breaks. PASS: the shots stop at the box (no hit on the husks before
//     the break), it breaks after its hits, the shatter hits both husks (burstDamage 60 > a husk's 40 hp: both die), SHATTER paid, the box gone from
//     the world (a ray through its centre passes).
//   n42arena pad [--seed n]
//     The existing aimed jump pad (SurvivalMap.CheckPads, MovementMath.PadLaunchVelocity; level JSON `pads`: [{x, y, z, target: {x, y, z},
//     apexAbove}]) as the kit's launch pad: a pad injected at the player's spawn aimed 14 m out along an open line (apexAbove 4). The player stands
//     on it. PASS: launched, lands within 1.0 m of the target; the apex height over the pad printed (it clears a fight under the arc).
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N42Arena(string cmd, string[] args, ref int code)
    {
        if (cmd != "n42arena") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "grinder";
        bool off = Array.IndexOf(args, "--off") >= 0;
        uint seed = 4201;
        int si = Array.IndexOf(args, "--seed");
        if (si > 0 && si < args.Length - 1) seed = uint.Parse(args[si + 1], CultureInfo.InvariantCulture);
        SurvivalSim.Force42Arena = !off;
        try
        {
            if (what == "grinder") code = Arena42Grinder(off, seed);
            else if (what == "cover") code = Arena42CoverCheck(off, seed);
            else if (what == "pad") code = Arena42Pad(seed);
            else { Console.WriteLine("n42arena: grinder | cover | pad"); code = 2; }
        }
        finally { SurvivalSim.Force42Arena = null; }
    }

    static List<object> A42L(params double[] v)
    {
        var l = new List<object>();
        foreach (double d in v) l.Add(d);
        return l;
    }

    /// <summary>A survival run on the survival map with `inject` applied to its JSON before the run starts (Arena42 loads at StartRun).</summary>
    static SurvivalSim Arena42Sim(uint seed, Action<SurvivalMap> inject, out SurvivalMap map)
    {
        GameConfig cfg = Config();
        map = Map();
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        inject?.Invoke(map);
        sim.StartRun(seed, null, null);
        return sim;
    }

    /// <summary>An open flat line of dist m: from the spawn or a point round the arena's middle, 32 directions each; clear from the eye at five heights and the
    /// floor at the far end at the start's height (a ray down). The first found; the spawn looking -z if none.</summary>
    static void A42Open(SurvivalSim sim, SurvivalMap map, double dist, out Vec3 at, out double yaw, out double ux, out double uz)
    {
        var starts = new[] { map.playerSpawn, new Vec3(0, 0, 12), new Vec3(12, 0, 0), new Vec3(-12, 0, 0), new Vec3(0, 0, -12), new Vec3(0, 0, 0) };
        foreach (Vec3 p in starts)
            for (int k = 0; k < 32; k++)
            {
                double a = k * Math.PI / 16, dx = Math.Sin(a), dz = -Math.Cos(a);
                bool clear = true;
                var eyeAt = new Vec3(p.x, p.y + 1.6, p.z);
                foreach (double h in new[] { 0.3, 0.8, 1.2, 2.0, 3.0, 6.0 })
                {
                    var t = new Vec3(p.x + dx * dist, p.y + h, p.z + dz * dist);
                    double len = Vec3.DistanceTo(t, eyeAt);
                    if (sim.world.RayIntersect(eyeAt, (t - eyeAt) / len, out RayHit hit) && hit.distance < len + 1) clear = false;
                }
                for (double f = 2; f <= dist && clear; f += 2)
                {
                    var top = new Vec3(p.x + dx * f, p.y + 0.5, p.z + dz * f);
                    if (!sim.world.RayIntersect(top, new Vec3(0, -1, 0), out RayHit down) || Math.Abs(down.distance - 0.5) > 0.05) clear = false;
                }
                if (!clear) continue;
                at = p;
                yaw = Math.Atan2(-dx, -dz);
                ux = dx;
                uz = dz;
                return;
            }
        Console.WriteLine("    (no open line found: the spawn looking -z)");
        at = map.playerSpawn;
        yaw = 0;
        ux = 0;
        uz = -1;
    }

    static void A42Tick(SurvivalSim sim, double yaw, double pitch, bool fire = false, bool punch = false, Husk hold = null, Husk hold2 = null)
    {
        var inp = (ScriptedInput)sim.input;
        inp.SetLook(yaw, pitch);
        inp.Set(new InputFrame(), new CombatInput { weapon = -1, fire = new Button { held = fire, pressed = fire }, punch = new Button { held = punch, pressed = punch } });
        sim.mover.invulnerable = true;
        sim.health.hp = sim.health.max;
        sim.hitstopLeft = 0;
        sim.Step();
        foreach (Husk h in new[] { hold, hold2 })
            if (h != null && h.alive && h.knockSteps == 0) { h.velocity.x = 0; h.velocity.z = 0; h.windupSteps = 0; } // held still (the cover case: they wait behind it)
    }

    static int Arena42Grinder(bool off, uint seed)
    {
        Vec3 player = default;
        double ux = 0, uz = 1, yaw = 0;
        // the open line first (a throwaway sim), then the real run with the grinder on it
        {
            SurvivalSim probe = Arena42Sim(seed, null, out SurvivalMap m0);
            A42Open(probe, m0, 17, out player, out yaw, out ux, out uz);
        }
        double cx = player.x + ux * 4.2, cz = player.z + uz * 4.2; // the grinder's centre: its near edge 2.2 m out (a 4 m square, axis-aligned round the centre)
        double half = 2.0;
        // the near edge along the line: the square's boundary where the line enters it
        double tEnter = 4.2 - half / Math.Max(Math.Abs(ux), Math.Abs(uz));
        SurvivalSim sim = Arena42Sim(seed, m => m.json["grinders42"] = new List<object>
        {
            new Dictionary<string, object> { { "rect", A42L(cx - half, cz - half, cx + half, cz + half) }, { "y", player.y }, { "room", "test" } },
        }, out SurvivalMap map);
        int groundEv = 0, groundKnocked = 0;
        double groundStyle = 0;
        sim.events.On<Arena42GroundEvent>(Arena42.GroundEvent, e => { groundEv++; if (e.knocked) groundKnocked++; Console.WriteLine($"    ground: id {e.id} {e.kind} knocked {e.knocked} at ({e.x:0.0}, {e.y:0.00}, {e.z:0.0}) t {sim.SimTime:0.00} s"); });
        sim.events.On<StyleBonusEvent>("styleBonus", e => { if (e.name == Arena42.GroundStyleName) groundStyle += e.points; });
        Console.WriteLine($"n42arena grinder (arena42 {(off ? "OFF (forced)" : "ON (forced)")}; seed {seed}): grinder 4 x 4 m, its near edge {tEnter:0.00} m out along yaw {yaw * 180 / Math.PI:0} deg; the sim's grinders {(sim.arena42 != null ? sim.arena42.grinders.Count : 0)}");
        // KNOCK: a husk 1.6 m out, the edge 0.6 m behind it
        sim.Respawn(player, yaw * 180 / Math.PI);
        sim.husks.KillAll();
        for (int s = 0; s < 3; s++) A42Tick(sim, yaw, 0);
        double at = tEnter - 0.6;
        Husk h = sim.husks.Spawn("husk", new Vec3(player.x + ux * at, player.y, player.z + uz * at), player);
        if (h == null) { Console.WriteLine("  KNOCK: the husk could not be placed"); return 1; }
        h.hp = h.maxHp = 1000; // the punch's 10 damage never kills it: only the grinder can
        for (int s = 0; s < 2; s++) A42Tick(sim, yaw, 0);
        A42Tick(sim, yaw, -0.2, punch: true);
        int maxSteps = MovementMath.StepsFor(2.0, sim.step);
        for (int s = 0; s < maxSteps && h.alive; s++) A42Tick(sim, yaw, -0.2);
        bool knockDied = !h.alive;
        double flew = Math.Sqrt((h.position.x - player.x) * (h.position.x - player.x) + (h.position.z - player.z) * (h.position.z - player.z));
        Console.WriteLine($"  KNOCK: punched from {at:0.00} m (the edge at {tEnter:0.00} m): ground {(knockDied ? "Y" : "N")}, at {flew:0.00} m; events arena42Ground {groundEv} (knocked {groundKnocked}), GROUND style {groundStyle:0}");
        // WALK: a husk beyond the grinder walks at the player
        int groundBefore = groundEv;
        int sparedBefore = sim.arena42 != null ? sim.arena42.spared : 0;
        Husk w = sim.husks.Spawn("husk", new Vec3(player.x + ux * 9.5, player.y, player.z + uz * 9.5), player);
        bool walkAlive = true;
        double nearest = double.MaxValue;
        int walkSteps = MovementMath.StepsFor(12.0, sim.step);
        if (w != null)
        {
            for (int s = 0; s < walkSteps; s++)
            {
                A42Tick(sim, yaw, 0);
                double d = Math.Sqrt((w.position.x - player.x) * (w.position.x - player.x) + (w.position.z - player.z) * (w.position.z - player.z));
                if (w.alive && d < nearest) nearest = d;
            }
            walkAlive = w.alive;
        }
        int spared = (sim.arena42 != null ? sim.arena42.spared : 0) - sparedBefore;
        Console.WriteLine($"  WALK: a husk from 9.5 m (beyond the grinder) at the player for 12 s: alive {(walkAlive ? "Y" : "N")}, ground {groundEv - groundBefore}, nearest {(nearest < 1e9 ? nearest.ToString("0.0", CultureInfo.InvariantCulture) : "-")} m, edge-guard put-backs {spared}{(w == null ? " (not placed)" : "")}");
        bool pass = off ? !knockDied : knockDied && groundKnocked >= 1 && Math.Abs(groundStyle - sim.cfg.Num("design.n42.arena42.grinder.groundStyle")) < 1e-9 && w != null && walkAlive && groundEv == groundBefore;
        Console.WriteLine($"n42arena grinder {(off ? "(row OFF) " : "")}{(pass ? "PASS" : "RED")}");
        return pass ? 0 : 1;
    }

    static int Arena42CoverCheck(bool off, uint seed)
    {
        Vec3 player;
        double ux, uz, yaw;
        {
            SurvivalSim probe = Arena42Sim(seed, null, out SurvivalMap m0);
            A42Open(probe, m0, 17, out player, out yaw, out ux, out uz);
        }
        // the box: 5 m out, 1.0 m deep along the line, 2.4 m across, 1.6 m tall (axis-aligned: its extents along the line's dominant axis)
        double cx = player.x + ux * 5, cz = player.z + uz * 5;
        bool alongX = Math.Abs(ux) >= Math.Abs(uz);
        double hx = alongX ? 0.5 : 1.2, hz = alongX ? 1.2 : 0.5;
        double y0 = player.y, y1 = player.y + 1.6;
        int hits = 6;
        SurvivalSim sim = Arena42Sim(seed, m => m.json["cover42"] = new List<object>
        {
            new Dictionary<string, object> { { "min", A42L(cx - hx, y0, cz - hz) }, { "max", A42L(cx + hx, y1, cz + hz) }, { "hits", (double)hits }, { "room", "test" } },
        }, out SurvivalMap map);
        int hitEv = 0, breakEv = 0, enemyHitsBefore = 0, burstBodies = 0;
        double shatter = 0;
        bool broken = false;
        sim.events.On<Arena42CoverEvent>(Arena42.CoverHitEvent, e => hitEv++);
        sim.events.On<Arena42CoverEvent>(Arena42.CoverBreakEvent, e => { breakEv++; burstBodies = e.bodies; });
        sim.events.On<StyleBonusEvent>("styleBonus", e => { if (e.name == Arena42.ShatterStyleName) shatter += e.points; });
        sim.events.On<HitEvent>("hit", e => { if (!broken && e.weapon != Arena42.Weapon) enemyHitsBefore++; });
        Console.WriteLine($"n42arena cover (arena42 {(off ? "OFF (forced)" : "ON (forced)")}; seed {seed}): a 1.0 x 2.4 x 1.6 m box 5 m out along yaw {yaw * 180 / Math.PI:0} deg, {hits} hits; the sim's cover {(sim.arena42 != null ? sim.arena42.cover.Count : 0)}");
        sim.Respawn(player, yaw * 180 / Math.PI);
        sim.husks.KillAll();
        for (int s = 0; s < 3; s++) A42Tick(sim, yaw, 0);
        double side = 0.7; // two husks 1 m behind the box, 0.7 m either side of the line
        Husk a = sim.husks.Spawn("husk", new Vec3(player.x + ux * 6.6 - uz * side, player.y, player.z + uz * 6.6 + ux * side), player);
        Husk b = sim.husks.Spawn("husk", new Vec3(player.x + ux * 6.6 + uz * side, player.y, player.z + uz * 6.6 - ux * side), player);
        if (a == null || b == null) { Console.WriteLine("  the husks could not be placed"); return 1; }
        // aim at the box's face centre (0.9 m up)
        Vec3 eye = sim.Eye;
        var aimAt = new Vec3(player.x + ux * (5 - (alongX ? hx : hz)), player.y + 0.9, player.z + uz * (5 - (alongX ? hx : hz)));
        (double ay, double ap) = ArclineLook(eye, aimAt);
        int shots = 0, fired = 0;
        sim.events.On("fire", _ => fired++);
        int cap = MovementMath.StepsFor(8.0, sim.step);
        int brokeAtShot = -1;
        for (int s = 0; s < cap; s++)
        {
            bool press = s % 2 == 0; // a press every other step: the gun fires each time its cycle allows
            A42Tick(sim, ay, ap, fire: press, hold: a, hold2: b);
            if (!broken && breakEv > 0) { broken = true; brokeAtShot = fired; }
            if (broken && s > 0) break;
        }
        shots = fired;
        bool rayPasses = false;
        if (broken)
        {
            Vec3 c = new Vec3(cx, player.y + 0.8, cz);
            Vec3 from = new Vec3(player.x + ux * 3, player.y + 0.8, player.z + uz * 3);
            Vec3 d = c - from;
            double len = d.Length;
            rayPasses = !(sim.world.RayIntersect(from, d / len, out RayHit rh) && rh.distance < len);
        }
        Console.WriteLine($"  shots fired {shots}; cover hits {hitEv} + break {breakEv} (at shot {brokeAtShot}); husk hits before the break {enemyHitsBefore}; the shatter hit {burstBodies} bodies, husks dead {(a.alive ? 0 : 1) + (b.alive ? 0 : 1)} / 2; SHATTER {shatter:0}; a ray through the box's centre passes after: {(rayPasses ? "Y" : "N")}");
        bool pass = off ? breakEv == 0 : breakEv == 1 && hitEv == hits - 1 && enemyHitsBefore == 0 && burstBodies == 2 && !a.alive && !b.alive && shatter > 0 && rayPasses;
        Console.WriteLine($"n42arena cover {(off ? "(row OFF) " : "")}{(pass ? "PASS" : "RED")}");
        return pass ? 0 : 1;
    }

    static int Arena42Pad(uint seed)
    {
        Vec3 player;
        double ux, uz, yaw;
        {
            SurvivalSim probe = Arena42Sim(seed, null, out SurvivalMap m0);
            A42Open(probe, m0, 17, out player, out yaw, out ux, out uz);
        }
        double dist = 14, apexAbove = 4;
        Vec3 target = new Vec3(player.x + ux * dist, player.y, player.z + uz * dist);
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        map.pads.Add(new PadSpec { pos = player, aimed = true, target = target, apexAbove = apexAbove });
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(seed, null, null);
        int padEv = 0;
        sim.events.On("jumpPad", _ => padEv++);
        sim.zones.OpenAll(); // every zone open: a pad in a locked zone sleeps (Zones.padEnabled); a level's pad is in its room
        sim.Respawn(new Vec3(player.x, player.y + 0.05, player.z), yaw * 180 / Math.PI);
        sim.husks.KillAll();
        bool launched = false, landed = false;
        double apex = player.y;
        int cap = MovementMath.StepsFor(5.0, sim.step);
        for (int s = 0; s < cap; s++)
        {
            A42Tick(sim, yaw, 0);
            if (padEv > 0) launched = true;
            if (launched) apex = Math.Max(apex, sim.mover.position.y);
            if (launched && s > 10 && sim.mover.grounded && sim.mover.velocity.y <= 0) { landed = true; break; }
        }
        Vec3 p = sim.mover.position;
        double err = Math.Sqrt((p.x - target.x) * (p.x - target.x) + (p.z - target.z) * (p.z - target.z));
        Console.WriteLine($"n42arena pad (seed {seed}): a pad at the spawn aimed {dist:0} m out (apexAbove {apexAbove:0}): pads in the map {map.pads.Count}, launched {(launched ? "Y" : "N")} (jumpPad events {padEv}), landed {(landed ? "Y" : "N")} {err:0.00} m from the target, apex {apex - player.y:0.00} m over the pad");
        bool pass = launched && landed && err <= 1.0;
        Console.WriteLine($"n42arena pad {(pass ? "PASS" : "RED")}");
        return pass ? 0 : 1;
    }
}
