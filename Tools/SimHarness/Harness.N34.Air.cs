// NIGHT34 builder B (leftover 1, design.n34.air34): the air multiplier's headless judge (the entry is N34Bend in Harness.N34.Bend.cs).
//   air34   the SURVIVAL map, the round clock held, invulnerable, the KESTREL in hand; the row forced on / off by n34Override. Four runs of the same
//           scripted kill: a 1 hp husk 6 m ahead in an open direction, both pinned, the player GROUNDED (on the spawn's floor) or AIRBORNE (pinned 4 m
//           up, the husk beside it), 1.2 s held still, then ONE KESTREL tap at the husk's aim point. Per run: the multiplier at the shot, the style
//           meter's score gained, the combat points earned, the damage the shot did (the hit event).
//           GREEN: (row on) airborne pays the multiplier (3x after 1 s): points x max the off run; style = the off run / style.stateMult x max (the
//           web's AIRSHOT bonus stays, its flat stateMult goes); grounded on = grounded off
//           (points and style); the damage identical in all four runs; the clock: 1 -> max in (max - 1) / risePerSecond s, max -> 1 in fallSeconds.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class Air34Result
    {
        public double mult, style, points, damage;
        public int kills, hits;
    }

    static Air34Result Air34Run(GameConfig cfg, bool on, bool air, uint seed)
    {
        double dt = 1 / cfg.movement.loop.simHz;
        int Steps(double sec) => MovementMath.StepsFor(sec, dt);
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.n34Override = new N34Blocks { air34 = on };
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(seed, null, null);
        sim.holdRunClock = true;
        sim.mover.invulnerable = true;
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        int slot = sim.loadout.owned.IndexOf("revolver");
        Vec3 sp = sim.map.playerSpawn;
        sim.Respawn(sp, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, sp, 15);
        Vec3 feet = sim.mover.position;
        double up = air ? 4 : 0;
        Vec3 pinP = new Vec3(feet.x, feet.y + up, feet.z);
        Husk h = null;
        foreach (double d in new[] { 6.0, 5.0, 7.0, 8.0 })
            for (int k = 0; k < 6 && h == null; k++) h = sim.husks.Spawn("husk", new Vec3(feet.x + ux * d, feet.y + 0.25 * k, feet.z + uz * d), feet);
        for (int s = 0; s < Steps(0.5); s++) Tick27(sim, yaw, 0, new CombatInput { weapon = s == 0 ? slot : -1 });
        var r = new Air34Result();
        if (h == null) { Console.WriteLine($"  (no husk placed at {feet.x + ux * 6:0.0}, {feet.y:0.0}, {feet.z + uz * 6:0.0}; feet {feet.x:0.0}, {feet.y:0.0}, {feet.z:0.0}; dir {ux:0.00}, {uz:0.00})"); return r; }
        Vec3 pinH = new Vec3(h.position.x, h.position.y + up, h.position.z);
        h.hp = h.maxHp = 1;
        sim.events.On<HitEvent>("hit", e => { if (e.owner == "player") { r.hits++; r.damage += e.damage; } });
        sim.events.On<KillEvent>("kill", e => { if (e.owner == "player") r.kills++; });
        double style0 = 0, points0 = 0;
        int warm = Steps(1.2), shot = warm, end = warm + Steps(0.3);
        for (int s = 0; s < end; s++)
        {
            if (air || s < warm) { sim.mover.position = pinP; sim.mover.velocity = new Vec3(0, 0, 0); }
            if (h.alive) { h.position = pinH; h.velocity = default; }
            double py = 0;
            if (h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
            if (s == shot)
            {
                r.mult = sim.Air34Mult;
                style0 = sim.style.core.total;
                points0 = sim.economy.earned;
            }
            Tick27(sim, yaw, py, new CombatInput { weapon = -1, fire = new Button { held = s == shot, pressed = s == shot } });
        }
        r.style = sim.style.core.total - style0;
        r.points = sim.economy.earned - points0;
        return r;
    }

    static void Air34Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        Air34Rules rules = Air34Rules.From(cfg);
        double dt = 1 / cfg.movement.loop.simHz;
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        Console.WriteLine($"air34: max {rules.max}, +{rules.risePerSecond} / s airborne or sliding, back to 1 in {rules.fallSeconds} s; the row forced per run; web style.stateMult {cfg.Num("style.stateMult")}");
        // the clock (Core/Air34.cs)
        var a = new Air34();
        a.Reset(rules);
        int up = 0, down = 0;
        while (a.mult < rules.max && up < 100000) { a.Step(true, dt); up++; }
        while (a.mult > 1 && down < 100000) { a.Step(false, dt); down++; }
        int upWant = MovementMath.StepsFor((rules.max - 1) / rules.risePerSecond, dt), downWant = MovementMath.StepsFor(rules.fallSeconds, dt);
        Console.WriteLine($"  clock: 1 -> {rules.max} in {up} steps ({up * dt:0.000} s, want ~{upWant}), {rules.max} -> 1 in {down} steps ({down * dt:0.000} s, want ~{downWant})");
        Need(Math.Abs(up - upWant) <= 1 && Math.Abs(down - downWant) <= 1, "the clock");
        var runs = new Dictionary<string, Air34Result>();
        foreach (bool on in new[] { true, false })
            foreach (bool air in new[] { false, true })
            {
                Air34Result r = Air34Run(cfg, on, air, 3420);
                string key = (on ? "on" : "off") + "/" + (air ? "air" : "ground");
                runs[key] = r;
                Console.WriteLine($"  {key,-10}: multiplier at the shot {r.mult:0.00}; hits {r.hits}, kills {r.kills}, damage {r.damage:0.###}; style +{r.style:0}, points +{r.points:0}");
            }
        Air34Result ga = runs["on/ground"], aa = runs["on/air"], go = runs["off/ground"], ao = runs["off/air"];
        double sr = ga.style > 0 ? aa.style / ga.style : 0, pr = ga.points > 0 ? aa.points / ga.points : 0;
        Console.WriteLine($"  row on: airborne / grounded style x{sr:0.00}, points x{pr:0.00} (the multiplier {aa.mult:0.00}); row off: airborne / grounded style x{(go.style > 0 ? ao.style / go.style : 0):0.00} (the web's stateMult), points x{(go.points > 0 ? ao.points / go.points : 0):0.00}");
        Need(ga.kills == 1 && aa.kills == 1 && go.kills == 1 && ao.kills == 1, "one kill each run");
        Need(Math.Abs(aa.mult - rules.max) < 1e-9 && Math.Abs(ga.mult - 1) < 1e-9, "the multiplier at the shot: max airborne, 1 grounded");
        // style: the airborne awards (KILL + the web's AIRSHOT bonus) at x max instead of the web's x stateMult; points: x max (the points had no air rule)
        double stateMult = cfg.Num("style.stateMult"), styleWant = ao.style / stateMult * rules.max, styleVsOff = ao.style > 0 ? aa.style / ao.style : 0;
        Console.WriteLine($"  airborne, row on vs off: style {aa.style:0} vs {ao.style:0} (want ~{styleWant:0} = off / {stateMult} x {rules.max}: x{styleVsOff:0.00}), points x{(ao.points > 0 ? aa.points / ao.points : 0):0.00}");
        Need(Math.Abs(aa.style - styleWant) <= 3 && Math.Abs(pr - rules.max) < 0.02 && Math.Abs(aa.points - ao.points * rules.max) <= 1, "airborne pays the multiplier on style and points");
        Need(ga.style == go.style && ga.points == go.points, "grounded: row on = row off");
        Need(ga.damage == aa.damage && ga.damage == go.damage && ga.damage == ao.damage && ga.damage > 0, "the damage identical in all four runs");
        Console.WriteLine(fails.Count == 0 ? "air34 GREEN" : $"air34 FAIL: {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }
}
